using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EbayWeb.Pages.Payment;

public class PaymentSuccess : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? OrderId { get; set; }

    public string? Message { get; set; }

    public void OnGet()
    {
        if (OrderId.HasValue)
        {
            Message = $"Order created successfully: {OrderId}";
        }
        else
        {
            Message = "Your payment was successful!";
        }
    }
}