using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.EasyPaymentGateway.Client 
{
    /// <summary>
    /// EasyPaymentGateway client settings.
    /// </summary>
    public class EasyPaymentGatewayServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
