using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;

namespace EbayCloneWeb.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _client;

        public List<OrderDTO> Orders = new();

        public IndexModel(IHttpClientFactory client)
        {
            _client = client;
        }

        public async Task OnGet(string? status)
        {
            var http = _client.CreateClient();

            var url = "http://localhost:5174/api/order";

            if (!string.IsNullOrEmpty(status))
                url += $"?status={status}";

            var res = await http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Orders = new List<OrderDTO>();
                return;
            }

            var result = await res.Content.ReadFromJsonAsync<OrderResult>();
            Orders = result?.data ?? new List<OrderDTO>();
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
    }
}