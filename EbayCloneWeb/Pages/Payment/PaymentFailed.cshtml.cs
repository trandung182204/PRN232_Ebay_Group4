using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EbayWeb.Pages.Payment;

public class PaymentFailed : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? OrderId { get; set; }

    public string? Message { get; set; }

    public void OnGet()
    {
        Message = OrderId.HasValue
            ? $"Payment for Order #{OrderId} could not be completed. Please try again or contact support."
            : "Your payment could not be processed. Please try again or contact support.";
    }
}
