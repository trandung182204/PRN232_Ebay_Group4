using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EbayCloneWeb.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been logged out.";

            return RedirectToPage("/Account/Login");
        }
    }
}