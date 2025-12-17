using System.Collections.Concurrent;

namespace EcommerceAPI.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        // IP başına istek sayısını tutan dictionary
        private static readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
        
        // Ayarlar
        private const int MaxRequestsPerMinute = 100;  // Dakikada maksimum istek sayısı
        private const int MaxRequestsPerSecond = 10;   // Saniyede maksimum istek sayısı
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
        private static DateTime _lastCleanup = DateTime.UtcNow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var now = DateTime.UtcNow;

            // Periyodik temizlik
            CleanupOldEntries(now);

            // Rate limit kontrolü
            if (!IsRequestAllowed(clientIp, now))
            {
                _logger.LogWarning("Rate limit aşıldı. IP={ClientIp}", clientIp);
                
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    statusCode = 429,
                    message = "Çok fazla istek gönderdiniz. Lütfen bir dakika bekleyin." 
                });
                return;
            }

            await _next(context);
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // X-Forwarded-For header'ını kontrol et (proxy/load balancer arkasında)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static bool IsRequestAllowed(string clientIp, DateTime now)
        {
            var info = _requestCounts.GetOrAdd(clientIp, _ => new RateLimitInfo());

            lock (info)
            {
                // Dakikalık pencereyi kontrol et
                if ((now - info.WindowStart).TotalMinutes >= 1)
                {
                    info.WindowStart = now;
                    info.RequestCountPerMinute = 0;
                }

                // Saniyelik pencereyi kontrol et
                if ((now - info.SecondWindowStart).TotalSeconds >= 1)
                {
                    info.SecondWindowStart = now;
                    info.RequestCountPerSecond = 0;
                }

                // Limitleri kontrol et
                if (info.RequestCountPerMinute >= MaxRequestsPerMinute || 
                    info.RequestCountPerSecond >= MaxRequestsPerSecond)
                {
                    return false;
                }

                info.RequestCountPerMinute++;
                info.RequestCountPerSecond++;
                info.LastRequest = now;
                return true;
            }
        }

        private static void CleanupOldEntries(DateTime now)
        {
            if ((now - _lastCleanup) < CleanupInterval)
                return;

            _lastCleanup = now;
            var cutoff = now.AddMinutes(-10);

            foreach (var key in _requestCounts.Keys.ToList())
            {
                if (_requestCounts.TryGetValue(key, out var info) && info.LastRequest < cutoff)
                {
                    _requestCounts.TryRemove(key, out _);
                }
            }
        }

        private class RateLimitInfo
        {
            public DateTime WindowStart { get; set; } = DateTime.UtcNow;
            public DateTime SecondWindowStart { get; set; } = DateTime.UtcNow;
            public DateTime LastRequest { get; set; } = DateTime.UtcNow;
            public int RequestCountPerMinute { get; set; }
            public int RequestCountPerSecond { get; set; }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
