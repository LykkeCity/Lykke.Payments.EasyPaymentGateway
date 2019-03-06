using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public EasyPaymentGatewaySettings EasyPaymentGatewayService { get; set; }
    }
}
