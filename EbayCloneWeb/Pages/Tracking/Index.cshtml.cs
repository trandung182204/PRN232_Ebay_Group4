using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EbayCloneWeb.Pages.Tracking
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _client;
        private const string ApiBase = "http://localhost:5174";

        public string? TrackingCodeInput { get; set; }
        public TrackingResultDto? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Searched { get; set; }

        public IndexModel(IHttpClientFactory client) => _client = client;

        public void OnGet(string? code = null)
        {
            TrackingCodeInput = code;
            if (!string.IsNullOrWhiteSpace(code))
                Searched = true;
        }

        public async Task<IActionResult> OnPostAsync(string? trackingCode)
        {
            TrackingCodeInput = trackingCode?.Trim();
            Searched = true;

            if (string.IsNullOrWhiteSpace(TrackingCodeInput))
            {
                ErrorMessage = "Please enter the tracking number.";
                return Page();
            }

            var http = _client.CreateClient();
            var res = await http.GetAsync($"{ApiBase}/api/Tracking/{Uri.EscapeDataString(TrackingCodeInput)}");

            if (res.IsSuccessStatusCode)
            {
                Result = await res.Content.ReadFromJsonAsync<TrackingResultDto>();
                ErrorMessage = null;
            }
            else
            {
                Result = null;
                var body = await res.Content.ReadAsStringAsync();
                try
                {
                    var err = JsonSerializer.Deserialize<JsonElement>(body);
                    if (err.TryGetProperty("error", out var e))
                        ErrorMessage = e.GetString() ?? "Tracking number not found.";
                    else
                        ErrorMessage = "Tracking number not found.";
                }
                catch
                {
                    ErrorMessage = res.StatusCode == HttpStatusCode.NotFound
                        ? "Tracking number not found."
                        : "An error occurred while tracking.";
                }

            }

            return Page();
        }
    }

    public class TrackingResultDto
    {
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public int? OrderId { get; set; }
        public string? Carrier { get; set; }
        public DateTime? EstimatedArrival { get; set; }
    }
}
