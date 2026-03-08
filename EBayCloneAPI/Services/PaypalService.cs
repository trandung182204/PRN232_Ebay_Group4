using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EBayAPI.Services;

public class PaypalService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    private readonly string _clientId;
    private readonly string _secret;
    private readonly string _baseUrl;

    public PaypalService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;

        _clientId = _config["PayPal:ClientId"];
        _secret = _config["PayPal:Secret"];
        _baseUrl = _config["PayPal:ApiUrl"];
    }

    // ===============================
    // GET ACCESS TOKEN
    // ===============================

    public async Task<string> GetAccessToken()
    {
        var auth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_clientId}:{_secret}")
        );

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/v1/oauth2/token"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", auth);

        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);

        var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<JsonElement>(json);

        return result.GetProperty("access_token").GetString();
    }

    // ===============================
    // CREATE ORDER
    // ===============================

    public async Task<(string orderId, string approveUrl)> CreateOrderAsync(decimal amount)
    {
        var token = await GetAccessToken();

        var body = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    amount = new
                    {
                        currency_code = "USD",
                        value = amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            },
            application_context = new
            {
                brand_name = "Test Store",
                landing_page = "LOGIN",
                user_action = "PAY_NOW",
                return_url = "http://localhost:5174/api/payments/paypal-success",
                cancel_url = "http://localhost:5174/api/payments/paypal-cancel"
            }
            
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/v2/checkout/orders"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<JsonElement>(json);

        var orderId = result.GetProperty("id").GetString();

        var approveUrl = result
            .GetProperty("links")
            .EnumerateArray()
            .First(x => x.GetProperty("rel").GetString() == "approve")
            .GetProperty("href")
            .GetString();

        return (orderId, approveUrl);
    }

    // ===============================
    // CAPTURE PAYMENT
    // ===============================

    public async Task<string?> CaptureOrderAsync(string orderId)
    {
        var token = await GetAccessToken();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/v2/checkout/orders/{orderId}/capture"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Paypal capture failed: {json}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        if (!result.TryGetProperty("purchase_units", out var purchaseUnits))
        {
            throw new Exception($"Paypal capture failed: {json}");
        }

        var captureId = purchaseUnits[0]
            .GetProperty("payments")
            .GetProperty("captures")[0]
            .GetProperty("id")
            .GetString();

        return captureId;
    }
}