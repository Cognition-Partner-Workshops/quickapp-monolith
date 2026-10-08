// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using System.Net;

namespace QuickApp.Core.Services.Shop
{
    /// <summary>
    /// Represents an unexpected response from the external order-service.
    /// </summary>
    public class OrderServiceException(string? message, HttpStatusCode? statusCode = null, string? responseBody = null)
        : Exception(message)
    {
        public HttpStatusCode? StatusCode { get; } = statusCode;

        public string? ResponseBody { get; } = responseBody;
    }
}
