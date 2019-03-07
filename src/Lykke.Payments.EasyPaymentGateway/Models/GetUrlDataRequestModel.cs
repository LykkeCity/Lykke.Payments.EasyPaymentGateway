namespace Lykke.Payments.EasyPaymentGateway.Models
{
    public class GetUrlDataRequestModel
    {
        public string OrderId { get; set; }

        public string ClientId { get; set; }

        public double Amount { get; set; }

        public string AssetId { get; set; }

        public string OtherInfo { get; set; }
    }
}
