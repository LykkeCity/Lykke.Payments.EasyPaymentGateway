using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Contracts.Payments;
using Lykke.Payments.EasyPaymentGateway.Domain;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentTransactionsRepository : IPaymentTransactionsRepository
    {
        private readonly INoSQLTableStorage<PaymentTransactionEntity> _tableStorage;
        private readonly INoSQLTableStorage<AzureMultiIndex> _tableStorageIndices;

        private const string IndexPartitinKey = "IDX";

        public PaymentTransactionsRepository(
            INoSQLTableStorage<PaymentTransactionEntity> tableStorage,
            INoSQLTableStorage<AzureMultiIndex> tableStorageIndices)
        {
            _tableStorage = tableStorage;
            _tableStorageIndices = tableStorageIndices;
        }

        public async Task<IPaymentTransaction> GetByTransactionIdAsync(string id)
        {
            return await _tableStorageIndices.GetFirstOrDefaultAsync(IndexPartitinKey, id, _tableStorage);
        }

        public async Task<IPaymentTransaction> StartProcessingTransactionAsync(string id, string paymentAggregatorTransactionId = null)
        {
            var meTransactionId = Guid.NewGuid().ToString();

            return await _tableStorageIndices.MergeAsync(IndexPartitinKey, id, _tableStorage, entity =>
            {
                if (entity.GetPaymentStatus() != PaymentStatus.Created)
                    return null;

                entity.SetPaymentStatus(PaymentStatus.Processing);
                entity.AggregatorTransactionId = paymentAggregatorTransactionId;
                entity.MeTransactionId = meTransactionId;
                return entity;
            });
        }

        public async Task<IPaymentTransaction> SetStatusAsync(string id, PaymentStatus status)
        {
            return await _tableStorageIndices.MergeAsync(IndexPartitinKey, id, _tableStorage, entity =>
            {
                entity.SetPaymentStatus(status);
                return entity;
            });
        }

        public async Task<IPaymentTransaction> SetAsOkAsync(string id, double depositedAmount, double? rate)
        {
            return await _tableStorageIndices.MergeAsync(IndexPartitinKey, id, _tableStorage, entity =>
            {
                entity.SetPaymentStatus(PaymentStatus.NotifyProcessed);
                entity.DepositedAmount = depositedAmount;
                entity.Rate = rate;
                return entity;
            });
        }

        public async Task<IPaymentTransaction> SetAntiFraudStatusAsync(string id, string antiFraudStatus)
        {
            return await _tableStorageIndices.MergeAsync(IndexPartitinKey, id, _tableStorage, entity =>
            {
                entity.AntiFraudStatus = antiFraudStatus;
                return entity;
            });
        }

        public async Task<bool> HasProcessedTransactionsAsync(string clientId, DateTime till)
        {
            var partitionKey = IndexByClient.GeneratePartitionKey(clientId);
            // todo: use GetTopRecordAsync instead
            var transactions = await _tableStorage.GetDataAsync(partitionKey, x => x.GetPaymentStatus() == PaymentStatus.NotifyProcessed && x.Created < till
                                                                                   && x.GetPaymentSystem() == CashInPaymentSystem.EasyPaymentGateway);
            return transactions.Any();
        }

        public async Task<IPaymentTransaction> SaveCardHashAsync(string id, string cardHash)
        {
            return await _tableStorageIndices.MergeAsync(IndexPartitinKey, id, _tableStorage, entity =>
            {
                entity.CardHash = cardHash;
                return entity;
            });
        }

        static class IndexByClient
        {
            public static string GeneratePartitionKey(string clientId)
            {
                return clientId;
            }
        }
    }
}
