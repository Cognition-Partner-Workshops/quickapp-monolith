// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

namespace QuickApp.Server.Services
{
    /// <summary>
    /// Propagates the inbound correlation id (or the request trace id) to downstream services.
    /// </summary>
    public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        public const string HeaderName = "X-Correlation-ID";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext != null && !request.Headers.Contains(HeaderName))
            {
                var correlationId = httpContext.Request.Headers[HeaderName].FirstOrDefault()
                    ?? httpContext.TraceIdentifier;
                request.Headers.TryAddWithoutValidation(HeaderName, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
