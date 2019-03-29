using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Domain
{
    public interface IPaymentNotifier
    {
        Task NotifyAsync(IPaymentTransaction paymentTransaction);
    }
}
