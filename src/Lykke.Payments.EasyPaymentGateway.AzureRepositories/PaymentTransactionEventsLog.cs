using AzureStorage;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentTransactionEventsLog : IPaymentTransactionEventsLog
    {
        private readonly INoSQLTableStorage<PaymentTransactionLogEventEntity> _tableStorage;

        public PaymentTransactionEventsLog(INoSQLTableStorage<PaymentTransactionLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task WriteAsync(IPaymentTransactionLogEvent newEvent)
        {
            var newEntity = PaymentTransactionLogEventEntity.Create(newEvent);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.DateTime);
        }
    }
}
