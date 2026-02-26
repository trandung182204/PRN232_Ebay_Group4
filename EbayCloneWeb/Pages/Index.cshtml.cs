using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace EbayCloneWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory factory, ILogger<IndexModel> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public IEnumerable<JsonElement> Products { get; set; } = new List<JsonElement>();
        public int TotalItems { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 16;

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? OrderBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortDir { get; set; }

        public async Task OnGetAsync()
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new System.Uri("http://localhost:5174/");
            var url = $"api/Product/list?page={Page}&pageSize={PageSize}";
            if (!string.IsNullOrEmpty(Q)) url += $"&q={System.Net.WebUtility.UrlEncode(Q)}";
            if (CategoryId.HasValue) url += $"&categoryId={CategoryId.Value}";
            if (!string.IsNullOrEmpty(OrderBy)) url += $"&orderBy={System.Net.WebUtility.UrlEncode(OrderBy)}";
            if (!string.IsNullOrEmpty(SortDir)) url += $"&sortDir={System.Net.WebUtility.UrlEncode(SortDir)}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                Products = new List<JsonElement>();
                TotalItems = 0;
                return;
            }

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            if (json.ValueKind == JsonValueKind.Object)
            {
                if (json.TryGetProperty("total", out var tot) && tot.ValueKind == JsonValueKind.Number)
                    TotalItems = tot.GetInt32();
                if (json.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    Products = items.EnumerateArray().ToArray();
                }
            }
            else
            {
                Products = new List<JsonElement>();
                TotalItems = 0;
            }
        }
    }
}
