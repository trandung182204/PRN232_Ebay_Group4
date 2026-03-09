using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace EbayCloneWeb.Pages.Order
{
    public class SuccessModel : PageModel
    {
        private readonly IHttpClientFactory _client;
        private const string ApiBase = "http://localhost:5174";

        public OrderSuccessDto? Order { get; set; }
        public string? ErrorMessage { get; set; }

        public SuccessModel(IHttpClientFactory client) => _client = client;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login", new { ReturnUrl = $"/Order/Success?id={id}" });

            var http = _client.CreateClient();
            var url = $"{ApiBase}/api/Order/{id}?userId={userId.Value}";
            var res = await http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                ErrorMessage = "Order not found.";
                return Page();
            }
            Order = await res.Content.ReadFromJsonAsync<OrderSuccessDto>();
            return Page();
        }
    }

    public class OrderSuccessDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
        public List<ShippingInfoDto> ShippingInfos { get; set; } = new();
    }

    public class ShippingInfoDto
    {
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
    }
}
