using JetBrains.Annotations;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyPaymentGatewaySettings
    {
        public DbSettings Db { get; set; }
        public MerchantSettings Merchant { get; set; }
        public EpgProviderSettings EpgProvider { get; set; }
        public WebHookSettings WebHook { get; set; }
        public RedirectSettings Redirect { get; set; }
        public string SourceClientId { get; set; }
    }
}
