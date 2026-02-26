using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace EbayCloneWeb.Pages.Order
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public CreateModel(IHttpClientFactory factory) => _factory = factory;

        [BindProperty]
        public int ProductId { get; set; }
        [BindProperty]
        public int Quantity { get; set; } = 1;
        [BindProperty]
        public string Region { get; set; } = "north";
        [BindProperty]
        public string PaymentMethod { get; set; } = "PayPal";
        [BindProperty]
        public string? Address { get; set; }

        public ProductDto? Product { get; set; }
        public List<string> ImagesList { get; set; } = new List<string>();
        public string? SellerEmail { get; set; }
        public string? SellerUsername { get; set; }
        public double AvgRating { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Load product
            var client = _factory.CreateClient();
            client.BaseAddress = new System.Uri("http://localhost:5174/");
            var res = await client.GetAsync($"api/Product/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToPage("/Index");
            Product = await res.Content.ReadFromJsonAsync<ProductDto>();
            ProductId = id;

            if (Product != null)
            {
                // parse images (may be JSON array or plain string or comma-separated)
                if (!string.IsNullOrEmpty(Product.Images))
                {
                    try
                    {
                        var imgs = JsonSerializer.Deserialize<string[]>(Product.Images);
                        if (imgs != null && imgs.Length > 0)
                        {
                            ImagesList = imgs.Where(s => !string.IsNullOrEmpty(s)).ToList();
                        }
                        else
                        {
                            ImagesList = new List<string> { Product.Images };
                        }
                    }
                    catch
                    {
                        ImagesList = Product.Images.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                        if (ImagesList.Count == 0 && !string.IsNullOrEmpty(Product.Images)) ImagesList.Add(Product.Images);
                    }
                }

                if (Product.Seller != null)
                {
                    SellerEmail = Product.Seller.Email;
                    SellerUsername = Product.Seller.Username;
                }

                // compute average rating
                if (Product.Reviews != null && Product.Reviews.Count > 0)
                {
                    var ratings = Product.Reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating!.Value);
                    if (ratings.Any()) AvgRating = ratings.Average();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // send user to login and return back to this order page after successful login
                var returnUrl = $"/Order/Create?id={ProductId}";
                return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
            }

            var client = _factory.CreateClient();
            client.BaseAddress = new System.Uri("http://localhost:5174/");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(ProductId.ToString()), "productId");
            content.Add(new StringContent(Quantity.ToString()), "quantity");
            content.Add(new StringContent(Address ?? string.Empty), "address");
            content.Add(new StringContent(Region ?? string.Empty), "region");
            content.Add(new StringContent(PaymentMethod ?? string.Empty), "paymentMethod");
            // include user id in form so API can associate order (API session is separate)
            var sessionUser = HttpContext.Session.GetInt32("UserId");
            if (sessionUser.HasValue)
            {
                content.Add(new StringContent(sessionUser.Value.ToString()), "userId");
            }
            // supply auth token and secureKey for payment/shipping simulation
            content.Add(new StringContent("valid_token"), "authToken");
            content.Add(new StringContent("SECURE_KEY_123"), "secureKey");

            var res = await client.PostAsync("api/Order/create", content);
            if (!res.IsSuccessStatusCode)
            {
                // try to read error detail from API
                string detail = string.Empty;
                try { detail = await res.Content.ReadAsStringAsync(); } catch { }
                TempData["Error"] = string.IsNullOrEmpty(detail) ? $"Failed to create order (status {res.StatusCode})" : $"Failed to create order: {detail}";
                return RedirectToPage("/Index");
            }

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            var orderId = 0;
            if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
            {
                orderId = idProp.GetInt32();
            }

            if (orderId > 0)
            {
                TempData["Success"] = $"Order created: {orderId}";
            }
            else
            {
                TempData["Success"] = "Order created";
            }
            return RedirectToPage("/Index");
        }

        public class ProductDto
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
            public string? Images { get; set; }
            public SellerDto? Seller { get; set; }
            public List<ReviewDto>? Reviews { get; set; }
        }

        public class SellerDto
        {
            public int Id { get; set; }
            public string? Email { get; set; }
            public string? Username { get; set; }
        }

        public class ReviewDto
        {
            public int Id { get; set; }
            public int? Rating { get; set; }
            public string? Comment { get; set; }
        }
    }
}
