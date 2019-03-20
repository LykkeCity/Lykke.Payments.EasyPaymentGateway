using Lykke.Contracts.Payments;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using System;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentSystemRawLogEvent : IPaymentSystemRawLogEvent
    {
        public DateTime DateTime { get; set; }
        public string PaymentSystem { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }

        public static PaymentSystemRawLogEvent Create(CashInPaymentSystem paymentSystem, string eventType, string data)
        {
            return new PaymentSystemRawLogEvent
            {
                DateTime = DateTime.UtcNow,
                PaymentSystem = paymentSystem.ToString(),
                Data = data,
                EventType = eventType
            };
        }
    }
}
