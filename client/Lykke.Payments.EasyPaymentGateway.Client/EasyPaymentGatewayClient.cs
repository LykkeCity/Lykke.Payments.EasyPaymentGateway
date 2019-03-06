using Lykke.HttpClientGenerator;

namespace Lykke.Payments.EasyPaymentGateway.Client
{
    /// <summary>
    /// EasyPaymentGateway API aggregating interface.
    /// </summary>
    public class EasyPaymentGatewayClient : IEasyPaymentGatewayClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to EasyPaymentGateway Api.</summary>
        public IEasyPaymentGatewayApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public EasyPaymentGatewayClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IEasyPaymentGatewayApi>();
        }
    }
}
