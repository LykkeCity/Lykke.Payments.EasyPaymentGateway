using AzureStorage;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Services;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.AzureRepositories
{
    public class PaymentSystemsRawLog : IPaymentSystemsRawLog
    {
        private readonly INoSQLTableStorage<PaymentSystemRawLogEventEntity> _tableStorage;

        public PaymentSystemsRawLog(INoSQLTableStorage<PaymentSystemRawLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task RegisterEventAsync(IPaymentSystemRawLogEvent evnt, string clientId = null)
        {
            var newEntity = PaymentSystemRawLogEventEntity.Create(evnt);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, evnt.DateTime);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var byCLient = PaymentSystemRawLogEventEntity.CreateByClient(evnt, clientId);
                await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(byCLient, evnt.DateTime);
            }
        }
    }
}
