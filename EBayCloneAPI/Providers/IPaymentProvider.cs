using EBayCloneAPI.Models;

namespace EBayCloneAPI.Services;

public interface IPaymentProvider
{
    string Method { get; }

    Task<object> CreatePayment(int orderId, int userId, decimal totalPrice);

    Task<bool> ConfirmPayment(int paymentId);
}