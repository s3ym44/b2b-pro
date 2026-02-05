using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace B2BProcurement.Attributes
{
    /// <summary>
    /// Rate limiting attribute for API endpoints.
    /// Limits requests per time window based on API key or IP address.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimits = new();

        /// <summary>
        /// Maximum number of requests allowed in the time window.
        /// </summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>
        /// Time window in seconds.
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get client identifier (API key or IP)
            var clientId = GetClientIdentifier(context);
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-WindowSeconds);

            // Get or create rate limit entry
            var entry = _rateLimits.GetOrAdd(clientId, _ => new RateLimitEntry());

            lock (entry)
            {
                // Remove old requests outside the window
                entry.Requests.RemoveAll(r => r < windowStart);

                // Check if limit exceeded
                if (entry.Requests.Count >= MaxRequests)
                {
                    var resetTime = entry.Requests.Min().AddSeconds(WindowSeconds);
                    var retryAfter = (int)Math.Ceiling((resetTime - now).TotalSeconds);

                    context.HttpContext.Response.Headers["X-RateLimit-Limit"] = MaxRequests.ToString();
                    context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.HttpContext.Response.Headers["X-RateLimit-Reset"] = resetTime.ToString("o");
                    context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

                    context.Result = new ObjectResult(new
                    {
                        success = false,
                        error = "Rate limit exceeded",
                        code = "RATE_LIMIT_EXCEEDED",
                        retryAfter = retryAfter
                    })
                    {
                        StatusCode = 429
                    };
                    return;
                }

                // Add current request
                entry.Requests.Add(now);
            }

            // Add rate limit headers
            context.HttpContext.Response.Headers["X-RateLimit-Limit"] = MaxRequests.ToString();
            context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = (MaxRequests - entry.Requests.Count).ToString();

            await next();

            // Cleanup old entries periodically
            if (_rateLimits.Count > 10000)
            {
                CleanupOldEntries();
            }
        }

        private string GetClientIdentifier(ActionExecutingContext context)
        {
            // Prefer API key if available
            if (context.HttpContext.Items.TryGetValue("ApiKey", out var apiKey) && apiKey != null)
            {
                return $"key:{apiKey}";
            }

            // Fall back to IP address
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var forwardedFor = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                ip = forwardedFor.Split(',').First().Trim();
            }

            return $"ip:{ip}";
        }

        private void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            var keysToRemove = _rateLimits
                .Where(kvp => !kvp.Value.Requests.Any() || kvp.Value.Requests.Max() < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimits.TryRemove(key, out _);
            }
        }

        private class RateLimitEntry
        {
            public List<DateTime> Requests { get; } = new();
        }
    }
}
