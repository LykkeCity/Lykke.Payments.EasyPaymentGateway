using Common;
using Lykke.Contracts.Payments;
using Lykke.Payments.EasyPaymentGateway.Domain;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentTransactionEntity : TableEntity, IPaymentTransaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        string IPaymentTransaction.Id => Id.ToString();

        public string ClientId { get; set; }
        public DateTime Created { get; set; }

        public string Status { get; set; }

        internal void SetPaymentStatus(PaymentStatus data)
        {
            Status = data.ToString();
        }

        internal PaymentStatus GetPaymentStatus()
        {
            return Status.ParseEnum(PaymentStatus.Created);
        }

        PaymentStatus IPaymentTransaction.Status => GetPaymentStatus();

        public string PaymentSystem { get; set; }
        public string Info { get; set; }
        CashInPaymentSystem IPaymentTransaction.PaymentSystem => GetPaymentSystem();

        internal CashInPaymentSystem GetPaymentSystem()
        {
            return PaymentSystem.ParseEnum(CashInPaymentSystem.Unknown);
        }

        public double? Rate { get; set; }
        public string AggregatorTransactionId { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string WalletId { get; set; }
        public double? DepositedAmount { get; set; }
        public string DepositedAssetId { get; set; }
        public string MeTransactionId { get; set; }
        public string AntiFraudStatus { get; set; }
        public string CardHash { get; set; }
    }
}
