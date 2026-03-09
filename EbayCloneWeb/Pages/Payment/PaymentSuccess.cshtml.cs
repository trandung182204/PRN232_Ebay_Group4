using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EbayCloneWeb.Pages.Payment;

public class PaymentSuccess : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? OrderId { get; set; }

    public string? Message { get; set; }

    public IActionResult OnGet()
    {
        if (OrderId.HasValue)
        {
            // Chuyển đến trang Order Success để khách xem mã vận đơn (shipment đã tạo sau khi paid)
            return RedirectToPage("/Order/Success", new { id = OrderId.Value });
        }
        Message = "Your payment was successful!";
        return Page();
    }
}