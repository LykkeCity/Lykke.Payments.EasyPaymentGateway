using JetBrains.Annotations;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyPaymentGatewaySettings
    {
        public DbSettings Db { get; set; }
        public MerchantSettings Merchant { get; set; }
    }
}
