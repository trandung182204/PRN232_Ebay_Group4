namespace EBayAPI;

public class PaymentAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secretKey;

    public PaymentAuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _secretKey = config["Payment:SecretKey"];
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/payments"))
        {
            var path = context.Request.Path.Value;

            // Cho phép PayPal redirect
            if (path.Contains("paypal-success") || path.Contains("confirm"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-PAYMENT-KEY", out var key)
                || key != _secretKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Payment Key");
                return;
            }
        }

        await _next(context);
    }
}