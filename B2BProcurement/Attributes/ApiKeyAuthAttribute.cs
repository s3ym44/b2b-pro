using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace B2BProcurement.Attributes
{
    /// <summary>
    /// API Key authentication attribute.
    /// Validates X-API-Key header against configured keys.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check for API key header
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    error = "API Key is missing",
                    code = "API_KEY_MISSING"
                });
                return;
            }

            // Get valid API keys from configuration
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var validApiKeys = configuration.GetSection("ApiSettings:ApiKeys").Get<string[]>() ?? Array.Empty<string>();
            var masterKey = configuration["ApiSettings:MasterApiKey"];

            // Check if key is valid
            var providedKey = potentialApiKey.ToString();
            if (!validApiKeys.Contains(providedKey) && providedKey != masterKey)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    error = "Invalid API Key",
                    code = "API_KEY_INVALID"
                });
                return;
            }

            // Store API key info for later use
            context.HttpContext.Items["ApiKey"] = providedKey;
            context.HttpContext.Items["IsMasterKey"] = providedKey == masterKey;

            await next();
        }
    }
}
