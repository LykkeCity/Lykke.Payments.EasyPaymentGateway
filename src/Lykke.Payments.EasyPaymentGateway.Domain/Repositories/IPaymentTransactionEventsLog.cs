using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Domain.Repositories
{
    public interface IPaymentTransactionEventsLog
    {
        Task WriteAsync(IPaymentTransactionLogEvent newEvent);
    }
}
