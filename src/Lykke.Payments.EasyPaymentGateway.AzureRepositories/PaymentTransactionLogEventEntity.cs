﻿using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentTransactionLogEventEntity : TableEntity, IPaymentTransactionLogEvent
    {
        public string PaymentTransactrionId => PartitionKey;
        public DateTime DateTime { get; set; }
        public string TechData { get; set; }
        public string Message { get; set; }
        public string Who { get; set; }

        internal static string GeneratePartitionKey(string transactionId)
        {
            return transactionId;
        }

        public static PaymentTransactionLogEventEntity Create(IPaymentTransactionLogEvent src)
        {
            return new PaymentTransactionLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.PaymentTransactrionId),
                DateTime = src.DateTime,
                Message = src.Message,
                TechData = src.TechData,
                Who = src.Who
            };
        }
    }
}
