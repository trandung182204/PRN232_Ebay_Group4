using System.Text;
using EBayAPI.Configurations;
using EBayAPI.Events;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EBayCloneAPI.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _cfg;
    private readonly ILogger<EmailService> _logger;

    private static readonly TimeZoneInfo _hanoiTz =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");

    private static string Hanoi(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(utc, _hanoiTz).ToString("yyyy-MM-dd HH:mm") + " (GMT+7)";

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _cfg    = options.Value;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    //  Order Created — confirmation email (sent on order placement)
    // ─────────────────────────────────────────────────────────────
    public async Task SendOrderCreatedAsync(OrderCreatedEvent data)
    {
        var subject = $"Order #{data.OrderId} Placed Successfully — eBayClone";

        // Payment-method specific message
        var (actionColor, actionTitle, actionBody) = data.PaymentMethod.ToLower() switch
        {
            "cod" => ("#86B817",
                      "Cash on Delivery",
                      "Your order will be prepared and delivered to you. Please have the exact amount ready upon delivery."),
            _     => ("#0064D2",
                      "Complete your payment",
                      $"Your order is reserved for <strong>30 minutes</strong>. Please complete payment via <strong>{data.PaymentMethod}</strong> to confirm your order.")
        };

        var body = $"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"/></head>
            <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;">
            <table cellpadding="0" cellspacing="0" width="100%" style="background:#f4f4f4;padding:30px 0;">
              <tr><td align="center">
                <table cellpadding="0" cellspacing="0" width="580" style="background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">

                  <!-- HEADER -->
                  <tr>
                    <td style="background:#0064D2;padding:22px 32px;">
                      <span style="font-size:28px;font-weight:900;letter-spacing:-1px;font-family:Arial;">
                        <span style="color:#E53238;font-style:italic;">e</span><span style="color:#fff;font-style:italic;">b</span><span style="color:#F5AF02;font-style:italic;">a</span><span style="color:#86B817;font-style:italic;">y</span>
                        <span style="color:rgba(255,255,255,.7);font-size:14px;font-weight:400;font-style:normal;">Clone</span>
                      </span>
                    </td>
                  </tr>

                  <!-- BANNER -->
                  <tr>
                    <td style="background:#e8f0fd;padding:18px 32px;border-left:4px solid #0064D2;">
                      <table cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="font-size:26px;color:#0064D2;padding-right:12px;line-height:1;vertical-align:middle;">&#128230;</td>
                          <td>
                            <div style="font-size:17px;font-weight:700;color:#0a1f5c;">Order Placed Successfully!</div>
                            <div style="font-size:13px;color:#555;margin-top:3px;">Order #{data.OrderId} &bull; {Hanoi(data.OrderDate)}</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- BODY -->
                  <tr>
                    <td style="padding:28px 32px;">
                      <p style="margin:0 0 22px;color:#333;font-size:15px;">
                        Hi <strong>{data.BuyerName}</strong>, we have received your order. Here are the details:
                      </p>

                      <!-- Product summary -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;margin-bottom:22px;">
                        <tr style="background:#f5f5f5;">
                          <th colspan="2" style="padding:12px 16px;text-align:left;font-size:12px;color:#767676;text-transform:uppercase;letter-spacing:.6px;">Order Summary</th>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">Product</td>
                          <td style="padding:12px 16px;font-size:14px;font-weight:600;color:#191919;border-bottom:1px solid #f0f0f0;">{data.ProductName}</td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">Quantity</td>
                          <td style="padding:12px 16px;font-size:14px;font-weight:600;color:#191919;border-bottom:1px solid #f0f0f0;">x{data.Quantity}</td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">Unit Price</td>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">US ${data.UnitPrice:0.00}</td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">Shipping Fee</td>
                          <td style="padding:12px 16px;font-size:14px;color:#555;border-bottom:1px solid #f0f0f0;">US ${data.ShippingFee:0.00}</td>
                        </tr>
                        <tr style="background:#f9f9f9;">
                          <td style="padding:12px 16px;font-size:14px;font-weight:700;color:#333;">Total</td>
                          <td style="padding:12px 16px;font-size:18px;font-weight:700;color:#0064D2;">US ${data.TotalPrice:0.00}</td>
                        </tr>
                      </table>

                      <!-- Shipping & payment info -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:#f9f9f9;border-radius:6px;margin-bottom:22px;">
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;width:140px;">Shipping Address</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#333;">{data.ShippingAddress}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Region</td>
                          <td style="padding:10px 16px;font-size:13px;color:#555;">{data.Region}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Payment Method</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#333;">{data.PaymentMethod}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Order Status</td>
                          <td style="padding:10px 16px;">
                            <span style="background:#fff3cd;color:#856404;font-size:12px;font-weight:700;padding:3px 10px;border-radius:12px;">Pending Payment</span>
                          </td>
                        </tr>
                      </table>

                      <!-- Next-step action card -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:{actionColor}18;border-left:4px solid {actionColor};border-radius:0 6px 6px 0;padding:0;">
                        <tr>
                          <td style="padding:16px 20px;">
                            <div style="font-size:14px;font-weight:700;color:{actionColor};margin-bottom:6px;">{actionTitle}</div>
                            <div style="font-size:13px;color:#444;line-height:1.6;">{actionBody}</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- FOOTER -->
                  <tr>
                    <td style="background:#191919;padding:18px 32px;text-align:center;">
                      <p style="color:#888;font-size:12px;margin:0;">&#169; 2026 eBayClone &#8212; PRN232 Group 4. All rights reserved.</p>
                    </td>
                  </tr>

                </table>
              </td></tr>
            </table>
            </body></html>
            """;

        await SendAsync(data.BuyerEmail, subject, body);
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-16: Payment confirmation + order summary
    // ─────────────────────────────────────────────────────────────
    public async Task SendPaymentConfirmationAsync(OrderPaidEvent data)
    {
        var subject = $"Payment Confirmed — Order #{data.OrderId}";

        // Build item rows
        var rows = new StringBuilder();
        foreach (var item in data.Items)
        {
            rows.AppendLine($"""
                <tr>
                  <td style="padding:10px 16px;font-size:14px;color:#333;border-bottom:1px solid #f0f0f0;">{item.ProductName}</td>
                  <td style="padding:10px 16px;font-size:14px;color:#555;text-align:center;border-bottom:1px solid #f0f0f0;">x{item.Quantity}</td>
                  <td style="padding:10px 16px;font-size:14px;color:#555;text-align:right;border-bottom:1px solid #f0f0f0;">US ${item.UnitPrice:0.00}</td>
                  <td style="padding:10px 16px;font-size:14px;font-weight:600;color:#333;text-align:right;border-bottom:1px solid #f0f0f0;">US ${item.LineTotal:0.00}</td>
                </tr>
            """);
        }

        var body = $"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"/></head>
            <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;">
            <table cellpadding="0" cellspacing="0" width="100%" style="background:#f4f4f4;padding:30px 0;">
              <tr><td align="center">
                <table cellpadding="0" cellspacing="0" width="580" style="background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">

                  <!-- HEADER -->
                  <tr>
                    <td style="background:#0064D2;padding:22px 32px;">
                      <span style="font-size:28px;font-weight:900;letter-spacing:-1px;font-family:Arial;">
                        <span style="color:#E53238;font-style:italic;">e</span><span style="color:#fff;font-style:italic;">b</span><span style="color:#F5AF02;font-style:italic;">a</span><span style="color:#86B817;font-style:italic;">y</span>
                        <span style="color:rgba(255,255,255,.7);font-size:14px;font-weight:400;font-style:normal;">Clone</span>
                      </span>
                    </td>
                  </tr>

                  <!-- SUCCESS BANNER -->
                  <tr>
                    <td style="background:#e8f5e9;padding:18px 32px;border-left:4px solid #4CAF50;">
                      <table cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="font-size:28px;color:#4CAF50;padding-right:12px;line-height:1;vertical-align:middle;">&#10003;</td>
                          <td>
                            <div style="font-size:17px;font-weight:700;color:#2e7d32;">Payment Successful</div>
                            <div style="font-size:13px;color:#555;margin-top:3px;">Order #{data.OrderId} has been confirmed</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- BODY -->
                  <tr>
                    <td style="padding:28px 32px;">
                      <p style="margin:0 0 22px;color:#333;font-size:15px;">
                        Hi <strong>{data.BuyerName}</strong>, thank you for your purchase. Here is your order summary:
                      </p>

                      <!-- Order items table -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="border:1px solid #e0e0e0;border-radius:6px;overflow:hidden;margin-bottom:22px;">
                        <tr style="background:#f5f5f5;">
                          <th style="padding:10px 16px;text-align:left;font-size:11px;color:#767676;text-transform:uppercase;letter-spacing:.6px;">Product</th>
                          <th style="padding:10px 16px;text-align:center;font-size:11px;color:#767676;text-transform:uppercase;letter-spacing:.6px;">Qty</th>
                          <th style="padding:10px 16px;text-align:right;font-size:11px;color:#767676;text-transform:uppercase;letter-spacing:.6px;">Unit Price</th>
                          <th style="padding:10px 16px;text-align:right;font-size:11px;color:#767676;text-transform:uppercase;letter-spacing:.6px;">Total</th>
                        </tr>
                        {rows}
                        <tr style="background:#f9f9f9;">
                          <td colspan="3" style="padding:12px 16px;text-align:right;font-weight:700;font-size:14px;color:#333;">Order Total:</td>
                          <td style="padding:12px 16px;text-align:right;font-weight:700;font-size:17px;color:#0064D2;">US ${data.TotalPrice:0.00}</td>
                        </tr>
                      </table>

                      <!-- Payment details -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:#f9f9f9;border-radius:6px;">
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;width:140px;">Payment Method</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#333;">{data.PaymentMethod}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Paid At</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#333;">{Hanoi(data.PaidAt)}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Order ID</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#0064D2;">#{data.OrderId}</td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- FOOTER -->
                  <tr>
                    <td style="background:#191919;padding:18px 32px;text-align:center;">
                      <p style="color:#888;font-size:12px;margin:0;">&#169; 2026 eBayClone &#8212; PRN232 Group 4. All rights reserved.</p>
                    </td>
                  </tr>

                </table>
              </td></tr>
            </table>
            </body></html>
            """;

        await SendAsync(data.BuyerEmail, subject, body);
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-17: Order status-change notification
    // ─────────────────────────────────────────────────────────────
    public async Task SendOrderStatusChangeAsync(OrderStatusChangedEvent data)
    {
        // Pick color + icon per status
        var (accentColor, iconHtml, headline, subline) = data.NewStatus switch
        {
            "Delivered" => ("#4CAF50", "&#128666;", "Your order has been delivered!", "Your package has arrived. We hope you enjoy your purchase."),
            "Failed"    => ("#E53238", "&#10060;",  "Payment failed",                 "Unfortunately your payment could not be processed. Please try again or contact support."),
            "Cancelled" => ("#FF9800", "&#9888;",   "Order cancelled",                "Your order has been cancelled. If you have any questions please contact support."),
            _           => ("#0064D2", "&#8505;",   $"Order status updated: {data.NewStatus}", "Your order status has been updated.")
        };

        var subject = $"Order #{data.OrderId} — {data.NewStatus}";

        var body = $"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"/></head>
            <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;">
            <table cellpadding="0" cellspacing="0" width="100%" style="background:#f4f4f4;padding:30px 0;">
              <tr><td align="center">
                <table cellpadding="0" cellspacing="0" width="580" style="background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">

                  <!-- HEADER -->
                  <tr>
                    <td style="background:#0064D2;padding:22px 32px;">
                      <span style="font-size:28px;font-weight:900;letter-spacing:-1px;font-family:Arial;">
                        <span style="color:#E53238;font-style:italic;">e</span><span style="color:#fff;font-style:italic;">b</span><span style="color:#F5AF02;font-style:italic;">a</span><span style="color:#86B817;font-style:italic;">y</span>
                        <span style="color:rgba(255,255,255,.7);font-size:14px;font-weight:400;font-style:normal;">Clone</span>
                      </span>
                    </td>
                  </tr>

                  <!-- STATUS BANNER -->
                  <tr>
                    <td style="background:{accentColor}22;padding:18px 32px;border-left:4px solid {accentColor};">
                      <table cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="font-size:28px;padding-right:12px;line-height:1;vertical-align:middle;">{iconHtml}</td>
                          <td>
                            <div style="font-size:17px;font-weight:700;color:#191919;">{headline}</div>
                            <div style="font-size:13px;color:#555;margin-top:3px;">Order #{data.OrderId}</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- BODY -->
                  <tr>
                    <td style="padding:28px 32px;">
                      <p style="margin:0 0 24px;color:#333;font-size:15px;">
                        Hi <strong>{data.BuyerName}</strong>, {subline}
                      </p>

                      <!-- Order details card -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:#f9f9f9;border-radius:6px;margin-bottom:24px;">
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;width:140px;">Order ID</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#0064D2;">#{data.OrderId}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Previous Status</td>
                          <td style="padding:10px 16px;font-size:13px;color:#555;">{data.OldStatus}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">New Status</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:700;color:{accentColor};">{data.NewStatus}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Order Total</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:700;color:#0064D2;">US ${data.TotalPrice:0.00}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Updated At</td>
                          <td style="padding:10px 16px;font-size:13px;color:#555;">{Hanoi(data.ChangedAt)}</td>
                        </tr>
                      </table>

                      <p style="margin:0;color:#767676;font-size:13px;">
                        If you have any questions about your order, please contact our support team.
                      </p>
                    </td>
                  </tr>

                  <!-- FOOTER -->
                  <tr>
                    <td style="background:#191919;padding:18px 32px;text-align:center;">
                      <p style="color:#888;font-size:12px;margin:0;">&#169; 2026 eBayClone &#8212; PRN232 Group 4. All rights reserved.</p>
                    </td>
                  </tr>

                </table>
              </td></tr>
            </table>
            </body></html>
            """;

        await SendAsync(data.BuyerEmail, subject, body);
    }

    // ─────────────────────────────────────────────────────────────
    //  Order Shipping — "Your order is on the way" email
    // ─────────────────────────────────────────────────────────────
    public async Task SendOrderShippingAsync(OrderShippingEvent data)
    {
        var subject = $"Order #{data.OrderId} — Your order is on the way!";

        var body = $"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"/></head>
            <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;">
            <table cellpadding="0" cellspacing="0" width="100%" style="background:#f4f4f4;padding:30px 0;">
              <tr><td align="center">
                <table cellpadding="0" cellspacing="0" width="580" style="background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">

                  <!-- HEADER -->
                  <tr>
                    <td style="background:#0064D2;padding:22px 32px;">
                      <span style="font-size:28px;font-weight:900;letter-spacing:-1px;font-family:Arial;">
                        <span style="color:#E53238;font-style:italic;">e</span><span style="color:#fff;font-style:italic;">b</span><span style="color:#F5AF02;font-style:italic;">a</span><span style="color:#86B817;font-style:italic;">y</span>
                        <span style="color:rgba(255,255,255,.7);font-size:14px;font-weight:400;font-style:normal;">Clone</span>
                      </span>
                    </td>
                  </tr>

                  <!-- BANNER -->
                  <tr>
                    <td style="background:#e3f2fd;padding:18px 32px;border-left:4px solid #0064D2;">
                      <table cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="font-size:28px;padding-right:12px;line-height:1;vertical-align:middle;">&#128666;</td>
                          <td>
                            <div style="font-size:17px;font-weight:700;color:#0a1f5c;">Your order is on the way!</div>
                            <div style="font-size:13px;color:#555;margin-top:3px;">Order #{data.OrderId} &bull; {Hanoi(data.ShippedAt)}</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- BODY -->
                  <tr>
                    <td style="padding:28px 32px;">
                      <p style="margin:0 0 22px;color:#333;font-size:15px;">
                        Hi <strong>{data.BuyerName}</strong>, great news! Your order has been handed to the carrier and is now on its way to you.
                      </p>

                      <!-- Tracking info -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:#f0f7ff;border:1px solid #c2dbf7;border-radius:8px;margin-bottom:22px;">
                        <tr>
                          <td style="padding:16px 20px;">
                            <div style="font-size:12px;color:#767676;text-transform:uppercase;letter-spacing:.6px;margin-bottom:6px;">Tracking Number</div>
                            <div style="font-size:20px;font-weight:700;color:#0064D2;letter-spacing:1px;font-family:monospace;">{(string.IsNullOrEmpty(data.TrackingNumber) ? "—" : data.TrackingNumber)}</div>
                            {(string.IsNullOrEmpty(data.TrackingNumber) ? "" : "<div style=\"font-size:12px;color:#555;margin-top:4px;\">Use this code to track your shipment on our website.</div>")}
                          </td>
                        </tr>
                      </table>

                      <!-- Order details -->
                      <table cellpadding="0" cellspacing="0" width="100%" style="background:#f9f9f9;border-radius:6px;margin-bottom:22px;">
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;width:140px;">Order ID</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#0064D2;">#{data.OrderId}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Order Total</td>
                          <td style="padding:10px 16px;font-size:13px;font-weight:700;color:#0064D2;">US ${data.TotalPrice:0.00}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Shipped At</td>
                          <td style="padding:10px 16px;font-size:13px;color:#555;">{Hanoi(data.ShippedAt)}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 16px;font-size:13px;color:#767676;">Est. Delivery</td>
                          <td style="padding:10px 16px;font-size:13px;color:#555;"><strong>3–7 business days</strong></td>
                        </tr>
                      </table>

                      <p style="margin:0;color:#767676;font-size:13px;">
                        If you have any questions about your shipment, please contact our support team.
                      </p>
                    </td>
                  </tr>

                  <!-- FOOTER -->
                  <tr>
                    <td style="background:#191919;padding:18px 32px;text-align:center;">
                      <p style="color:#888;font-size:12px;margin:0;">&#169; 2026 eBayClone &#8212; PRN232 Group 4. All rights reserved.</p>
                    </td>
                  </tr>

                </table>
              </td></tr>
            </table>
            </body></html>
            """;

        await SendAsync(data.BuyerEmail, subject, body);
    }

    // ─────────────────────────────────────────────────────────────
    //  Internal SMTP dispatcher — MailKit (Gmail STARTTLS)
    // ─────────────────────────────────────────────────────────────
    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            // Build MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_cfg.SenderName, _cfg.SenderEmail));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };

            // Send via MailKit (handles Gmail STARTTLS correctly)
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.SmtpServer, _cfg.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_cfg.SenderEmail, _cfg.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(quit: true);

            _logger.LogInformation("[Email] Sent '{Subject}' to {To}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Failed to send '{Subject}' to {To}", subject, toEmail);
            throw;
        }
    }
}
