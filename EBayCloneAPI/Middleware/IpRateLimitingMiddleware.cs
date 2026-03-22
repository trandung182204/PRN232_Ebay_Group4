using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EBayCloneAPI.Middleware
{
    public class IpRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IpRateLimitingMiddleware> _logger;

        // Default limits
        private readonly int _maxRequests = 10000; // requests
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        public IpRateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<IpRateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"rl_{ip}";

            var entry = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _period;
                return new RateLimitCounter { Count = 0, ExpiresAt = DateTime.UtcNow.Add(_period) };
            });

            if (entry.Count >= _maxRequests)
            {
                _logger.LogWarning("IP {Ip} has exceeded rate limit", ip);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = ((int)_period.TotalSeconds).ToString();
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            entry.Count++;
            _cache.Set(key, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _period
            });

            await _next(context);
        }

        private class RateLimitCounter
        {
            public int Count { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
