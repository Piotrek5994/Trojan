using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace IdoBookService.ApiBaseRequests;

public class ApiRequest : ApiClient
{
    public async Task<T> ExecuteApiRequestAsync<T>(string endpoint, Dictionary<string, string> parameters = null, HttpContent bodyContent = null, HttpMethod httpMethod = null)
    {
        try
        {
            string body = bodyContent == null ? "" : await bodyContent.ReadAsStringAsync();
            var url = "";
            if (parameters != null && parameters.Count > 0)
            {
                var queryParams = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
                url = $"{endpoint}?{queryParams}";
            }

            return await SendApiRequest<T>(endpoint, httpMethod, parameters, bodyContent);
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd podczas pobierania danych z API: {ex.Message}");
        }
    }
}
