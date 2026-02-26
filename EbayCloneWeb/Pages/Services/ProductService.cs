using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EbayCloneWeb.Services
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Images { get; set; }
        public int? CategoryId { get; set; }
    }

    public class ProductService
    {
        private readonly HttpClient _http;
        public ProductService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new System.Uri("http://localhost:5174/");
        }

        public async Task<IEnumerable<ProductDto>> ListAsync(int limit = 50)
        {
            var res = await _http.GetFromJsonAsync<IEnumerable<ProductDto>>("api/Product/list?limit=" + limit);
            return res ?? new List<ProductDto>();
        }
    }
}
