using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public EasyPaymentGatewaySettings EasyPaymentGatewayService { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }
    }
}
