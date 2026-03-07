using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class TransactionLogHook : IPaymentEventHook
    {
        private readonly ApplicationDbContext _db;

        public TransactionLogHook(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnPaymentSuccess(OrderTable order, Payment payment, string transactionId)
        {
            _db.SystemLogs.Add(new SystemLog
            {
                Module = "Payment",
                Action = "PaymentSuccess",
                Status = "Success",
                ErrorMessage = null,
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}
