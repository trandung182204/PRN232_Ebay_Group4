using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace EbayCloneWeb.Pages.Admin.Orders
{
    public class DetailModel : PageModel
    {
        private readonly IHttpClientFactory _client;

        public OrderDetailDTO? Order { get; set; }

        public DetailModel(IHttpClientFactory client)
        {
            _client = client;
        }

        public async Task OnGet(int id)
        {
            var http = _client.CreateClient();

            Order = await http.GetFromJsonAsync<OrderDetailDTO>(
                $"http://localhost:5174/api/order/{id}"
            );
        }
    }


    public class OrderDetailDTO
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        public List<OrderItemDTO> OrderItems { get; set; } = new();

        public List<PaymentDTO> Payments { get; set; } = new();

        public List<ShippingDTO> ShippingInfos { get; set; } = new();
    }


    public class OrderItemDTO
    {
        public int Quantity { get; set; }

        public ProductDTO? Product { get; set; }
    }


    public class ProductDTO
    {
        public string Name { get; set; }

        public decimal Price { get; set; }
    }


    public class PaymentDTO
    {
        public string Method { get; set; }

        public decimal Amount { get; set; }

        public DateTime? PaidAt { get; set; }
    }


    public class ShippingDTO
    {
        public string TrackingNumber { get; set; }

        public string Status { get; set; }
    }
}