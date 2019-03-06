using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyPaymentGatewaySettings
    {
        public DbSettings Db { get; set; }
    }
}
