using System.Net;

namespace IdoBookService.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorResponse { get; }

    public ApiException(HttpStatusCode statusCode, string errorResponse)
        : base($"Request failed with status code {statusCode}")
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }
}
