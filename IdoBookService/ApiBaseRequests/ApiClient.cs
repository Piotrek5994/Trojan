using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Core.Models.ApiRequest;
using IdoBookService.Exceptions;

namespace IdoBookService.ApiBaseRequests;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string _token = string.Empty;
    private DateTime _tokenExpiration;

    public ApiClient()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient();
    }

    public async Task<T> SendApiRequest<T>(string endpoint, HttpMethod method, Dictionary<string, string> parameters = null, HttpContent bodyContent = null)
    {
        try
        {
            await EnsureToken();

            var url = $"http://localhost:5506/{endpoint}";
            if (parameters != null && parameters.Count > 0)
            {
                var queryParams = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
                url = $"{url}?{queryParams}";
            }

            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            if (bodyContent != null)
            {
                request.Content = bodyContent;
            }

            HttpResponseMessage response = _httpClient.Send(request);

            return await HandleResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException(response.StatusCode, errorContent);
        }

        if (typeof(T) == typeof(Stream))
        {
            return await HandleStreamResponseAsync<T>(response);
        }
        else
        {
            return await HandleJsonResponseAsync<T>(response);
        }
    }

    private async Task<T> HandleStreamResponseAsync<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();

        if (stream == null || stream.Length == 0)
        {
            throw new InvalidOperationException("The response stream is empty.");
        }

        return (T)(object)stream;
    }

    private async Task<T> HandleJsonResponseAsync<T>(HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonResponse) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    private async Task EnsureToken()
    {
        if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiration)
        {
            await RefreshToken();
        }
    }
    private async Task RefreshToken()
    {
        string url = "http://localhost:5506/user/authenticate";
        try
        {
            LoginRequest loginRequest = new LoginRequest();

            //loginRequest.UserName = "testKLKI";
            //loginRequest.Password = "test";

            var jsonContent = JsonSerializer.Serialize(loginRequest);
            var bodyContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (bodyContent != null)
            {
                request.Content = bodyContent;
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new ApiException(response.StatusCode, errorResponse);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
            _token = loginResponse.token;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(loginResponse.expirationDate);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
