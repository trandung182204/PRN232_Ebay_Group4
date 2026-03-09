using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace EbayCloneWeb.Pages.Order
{
    public class MyOrdersModel : PageModel
    {
        private readonly IHttpClientFactory _client;
        private const string ApiBase = "http://localhost:5174";

        public MyOrdersResponse? Response { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? StatusFilter { get; set; }

        public MyOrdersModel(IHttpClientFactory client) => _client = client;

        public async Task<IActionResult> OnGetAsync(int page = 1, int pageSize = 10, string? status = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Order/MyOrders" });

            Page = page;
            PageSize = pageSize;
            StatusFilter = status;

            var query = $"{ApiBase}/api/Order?userId={userId.Value}&page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(status))
                query += $"&status={Uri.EscapeDataString(status)}";

            var http = _client.CreateClient();
            var res = await http.GetAsync(query);
            if (!res.IsSuccessStatusCode)
            {
                Response = new MyOrdersResponse { total = 0, data = new List<OrderRow>() };
                return Page();
            }

            Response = await res.Content.ReadFromJsonAsync<MyOrdersResponse>();
            Response ??= new MyOrdersResponse { total = 0, data = new List<OrderRow>() };
            return Page();
        }
    }

    public class MyOrdersResponse
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
        public List<OrderRow> data { get; set; } = new();
    }

    public class OrderRow
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public object? Status { get; set; }
    }
}
