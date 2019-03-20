using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Domain.Services
{
    public interface IPaymentSystemsRawLog
    {
        Task RegisterEventAsync(IPaymentSystemRawLogEvent evnt, string clientId = null);
    }
}
