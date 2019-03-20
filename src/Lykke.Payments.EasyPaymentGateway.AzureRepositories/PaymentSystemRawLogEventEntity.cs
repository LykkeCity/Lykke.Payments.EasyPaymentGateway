using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentSystemRawLogEventEntity : TableEntity, IPaymentSystemRawLogEvent
    {
        public DateTime DateTime { get; set; }
        public string PaymentSystem => PartitionKey;
        public string EventType { get; set; }
        public string Data { get; set; }

        public static string GeneratePartitionKey(string paymentSystem)
        {
            return paymentSystem;
        }

        public static PaymentSystemRawLogEventEntity Create(IPaymentSystemRawLogEvent src)
        {
            return new PaymentSystemRawLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.PaymentSystem),
                DateTime = src.DateTime,
                Data = src.Data,
                EventType = src.EventType
            };
        }

        public static PaymentSystemRawLogEventEntity CreateByClient(IPaymentSystemRawLogEvent src, string clientId)
        {
            return new PaymentSystemRawLogEventEntity
            {
                PartitionKey = clientId,
                DateTime = src.DateTime,
                Data = src.Data,
                EventType = src.EventType
            };
        }
    }
}
