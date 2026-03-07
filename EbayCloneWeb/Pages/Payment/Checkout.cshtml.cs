using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EbayWeb.Pages.Payment;

public class CheckoutModel : PageModel
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalPrice { get; set; }

    public void OnGet(int orderId)
    {
        // demo data
        OrderId = orderId;
        UserId = 6;
        TotalPrice = 200;
    }
}