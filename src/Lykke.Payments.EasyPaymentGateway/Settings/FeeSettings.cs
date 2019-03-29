namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    public class FeeSettings
    {
        public TargetClientIdFeeSettings TargetClientId { get; set; }
    }

    public class TargetClientIdFeeSettings
    {
        public string BankCard { get; set; }
    }
}
