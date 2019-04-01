using JetBrains.Annotations;
using Refit;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Client
{
    // This is an example of service controller interfaces.
    // Actual interface methods must be placed here (not in IEasyPaymentGatewayClient interface).

    /// <summary>
    /// EasyPaymentGateway client API interface.
    /// </summary>
    [PublicAPI]
    public interface IEasyPaymentGatewayApi
    {
        /// <summary>
        /// Checks if client suspicious
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>IsClientSuspicious - true/false</returns>
        [Get("/api/IsClientSuspicious")]
        Task<bool> IsClientSuspicious(string clientId);
    }
}
