using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using System;

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
        public RabbitMqSettings RabbitMq { get; set; }
        public string LegalEntityCode { get; set; }
        public string EasyPaymentGatewayContext => "easy-payment-gateway" + (string.IsNullOrEmpty(LegalEntityCode) ? "" : "-" + LegalEntityCode);
        public string MeContext => "me" + (string.IsNullOrEmpty(LegalEntityCode) ? "" : "-" + LegalEntityCode);
        public string Environment { get; set; }

        [Optional]
        public DateTime AntiFraudCheckRegistrationDateSince { get; set; } = new DateTime(2018, 7, 1);

        [Optional]
        public TimeSpan AntiFraudCheckPaymentPeriod { get; set; } = TimeSpan.FromDays(7);
    }
}
