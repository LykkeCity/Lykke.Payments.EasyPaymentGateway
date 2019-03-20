using System;

namespace Lykke.Payments.EasyPaymentGateway.Domain.Repositories
{
    public interface IPaymentSystemRawLogEvent
    {
        DateTime DateTime { get; }
        string PaymentSystem { get; }

        string EventType { get; }
        string Data { get; }
    }
}
