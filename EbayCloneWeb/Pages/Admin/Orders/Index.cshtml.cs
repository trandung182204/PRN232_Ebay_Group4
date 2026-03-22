using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;

namespace EbayCloneWeb.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _client;
        private readonly IConfiguration _config;

        public List<OrderDTO> Orders = new();

        public IndexModel(IHttpClientFactory client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string ApiBaseUrl => (_config["ApiSettings:BaseUrl"] ?? "http://localhost:5174").TrimEnd('/');

        public async Task OnGet(string? status, int page = 1, int pageSize = 50)
        {
            var http = _client.CreateClient();


            // Admin: không gửi userId → API trả về tất cả đơn hàng (GetOrdersAsync)
            var url = $"http://localhost:5174/api/order?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var res = await http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Orders = new List<OrderDTO>();
                return;
            }

            var result = await res.Content.ReadFromJsonAsync<OrderResult>();
            Orders = result?.data ?? new List<OrderDTO>();
            TotalCount = result?.total ?? 0;
            Page = result?.page ?? 1;
            PageSize = result?.pageSize ?? pageSize;
        }
    }

    public class OrderResult
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }

        public List<OrderDTO> data { get; set; } = new();
    }

    public class OrderDTO
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        public int Buyer { get; set; }

        public List<string>? TrackingNumbers { get; set; }
    }
}