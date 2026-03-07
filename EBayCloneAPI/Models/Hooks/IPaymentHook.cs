using EBayAPI.DTOs;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IPaymentHook
    {
        string Name { get; }

        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);

        Task<bool> RefundAsync(string transactionId);
    }
}
