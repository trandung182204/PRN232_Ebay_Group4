using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EbayCloneWeb.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public LoginModel(IHttpClientFactory factory) => _factory = factory;

        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _factory.CreateClient();
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Email ?? string.Empty), "email");
            content.Add(new StringContent(Password ?? string.Empty), "password");
            client.BaseAddress = new System.Uri("http://localhost:5174/");
            var res = await client.PostAsync("api/Auth/login", content);
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return Page();
            }

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            int id = 0;
            if (json.ValueKind == JsonValueKind.Object)
            {
                if (json.TryGetProperty("id", out var idProp) || json.TryGetProperty("Id", out idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number)
                        id = idProp.GetInt32();
                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out var parsed))
                        id = parsed;
                }
            }

            if (id == 0)
            {
                ModelState.AddModelError(string.Empty, "Login failed (invalid response)");
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", id);

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Index");
        }
    }
}
