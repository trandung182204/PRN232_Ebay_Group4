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
        private readonly string _secretKey;

        public CreateModel(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _secretKey = config["Payment:SecretKey"];
        }

        [BindProperty] public int ProductId { get; set; }
        [BindProperty] public int Quantity { get; set; } = 1;
        [BindProperty] public string Region { get; set; } = "north";
        [BindProperty] public string PaymentMethod { get; set; } = "PAYPAL";
        [BindProperty] public string? Address { get; set; }

        public ProductDto? Product { get; set; }
        public List<string> ImagesList { get; set; } = new List<string>();
        public string? SellerEmail { get; set; }
        public string? SellerUsername { get; set; }
        public double AvgRating { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Load product
            var client = _factory.CreateClient();
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
                        ImagesList = Product.Images
                            .Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                        if (ImagesList.Count == 0 && !string.IsNullOrEmpty(Product.Images))
                            ImagesList.Add(Product.Images);
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
                var returnUrl = $"/Order/Create?id={ProductId}";
                return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
            }

            var client = _factory.CreateClient();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(ProductId.ToString()), "productId");
            content.Add(new StringContent(Quantity.ToString()), "quantity");
            content.Add(new StringContent(Address ?? ""), "address");
            content.Add(new StringContent(Region ?? ""), "region");
            content.Add(new StringContent(PaymentMethod ?? ""), "paymentMethod");
            content.Add(new StringContent(userId.Value.ToString()), "userId");

            content.Add(new StringContent("valid_token"), "authToken");
            content.Add(new StringContent("SECURE_KEY_123"), "secureKey");

            var res = await client.PostAsync("api/Order/create", content);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Create order failed";
                return RedirectToPage("/Index");
            }

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();

            var orderId = json.GetProperty("id").GetInt32();

            // ============================
            // PAYPAL PAYMENT
            // ============================
            if (PaymentMethod == "PAYPAL")
            {
                var paymentBody = new
                {
                    orderId = orderId,
                    userId = userId.Value,
                    method = "PAYPAL"
                };
                // 🔴 QUAN TRỌNG: thêm header cho middleware
                client.DefaultRequestHeaders.Remove("X-PAYMENT-KEY");
                client.DefaultRequestHeaders.Add("X-PAYMENT-KEY", _secretKey);
                var paymentRes = await client.PostAsJsonAsync("api/payments", paymentBody);

                if (!paymentRes.IsSuccessStatusCode)
                {
                    var err = await paymentRes.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Create PayPal payment failed: {err}";
                    return RedirectToPage("/Index");
                }

                var paymentJson = await paymentRes.Content.ReadFromJsonAsync<JsonElement>();

                var approveUrl = paymentJson.GetProperty("approveUrl").GetString();

                return Redirect(approveUrl!);
            }

            // ============================
            // COD: chuyển đến trang Success để khách xem mã vận đơn
            // ============================
            TempData["Success"] = "Your order has been placed successfully! Check your tracking code below.";
            return RedirectToPage("/Order/Success", new { id = orderId });
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