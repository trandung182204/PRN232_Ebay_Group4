namespace EBayCloneAPI.Services
{
    public interface IPaymentService
    {
        IPaymentProvider GetProvider(string method);
    }
}
