namespace EBayAPI.Models.Hooks
{
    public class PluginManager
    {
        public List<IPaymentHook> PaymentHooks { get; } = new();
        public List<IShippingHook> ShippingHooks { get; } = new();

        public void RegisterPayment(IPaymentHook plugin)
        {
            PaymentHooks.Add(plugin);
        }

        public void RegisterShipping(IShippingHook plugin)
        {
            ShippingHooks.Add(plugin);
        }

        public IPaymentHook GetPayment(string name)
        {
            return PaymentHooks.FirstOrDefault(p => p.Name == name);
        }

        public IShippingHook GetShipping(string name)
        {
            return ShippingHooks.FirstOrDefault(p => p.Name == name);
        }
    }
}
