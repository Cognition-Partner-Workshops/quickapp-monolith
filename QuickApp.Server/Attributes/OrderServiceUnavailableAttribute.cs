// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuickApp.Core.Services.Shop;

namespace QuickApp.Server.Attributes
{
    /// <summary>
    /// Translates order-service failures into 503 responses instead of unhandled 500s.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class OrderServiceUnavailableAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (!IsOrderServiceFailure(context.Exception))
                return;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OrderServiceUnavailableAttribute>>();
            logger.LogError(context.Exception, "order-service call failed");

            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "The order service is unavailable."
            })
            { StatusCode = StatusCodes.Status503ServiceUnavailable };
            context.ExceptionHandled = true;
        }

        // Polly types are matched by namespace: both Polly v7 and Polly.Core are referenced transitively.
        private static bool IsOrderServiceFailure(Exception ex) =>
            ex is HttpRequestException or OrderServiceException
            || ex.GetType().Namespace?.StartsWith("Polly", StringComparison.Ordinal) == true;
    }
}
