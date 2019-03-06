using JetBrains.Annotations;

namespace Lykke.Payments.EasyPaymentGateway.Client
{
    /// <summary>
    /// EasyPaymentGateway client interface.
    /// </summary>
    [PublicAPI]
    public interface IEasyPaymentGatewayClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IEasyPaymentGatewayApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IEasyPaymentGatewayApi Api { get; }
    }
}
