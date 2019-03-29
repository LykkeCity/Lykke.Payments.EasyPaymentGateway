namespace Lykke.Payments.EasyPaymentGateway.Domain
{
    public class OrderStatus
    {
        public string OrderId { get; set; }

        public string TransactionId { get; set; }

        public OrderState? State { get; set; }

        public string ErrorMessage { get; set; }
    }

    public enum OrderState
    {
        Ok,
        Fail,
        Cancel
    }
}
