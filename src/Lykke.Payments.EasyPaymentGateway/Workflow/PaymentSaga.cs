using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Payments.EasyPaymentGateway.Domain;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Lykke.Payments.EasyPaymentGateway.Workflow.Events;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Workflow
{
    public class PaymentSaga
    {
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentNotifier[] _paymentNotifiers;
        private readonly EasyPaymentGatewaySettings _easyPaymentGatewaySettings;
        private readonly ILog _log;

        public PaymentSaga(
            IPaymentTransactionsRepository paymentTransactionsRepository, 
            IPaymentNotifier[] paymentNotifiers, 
            EasyPaymentGatewaySettings easyPaymentGatewaySettings,
            ILogFactory logFactory)
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentNotifiers = paymentNotifiers;
            _easyPaymentGatewaySettings = easyPaymentGatewaySettings;
            _log = logFactory.CreateLog(this);
        }

        public async Task Handle(ProcessingStartedEvent evt, ICommandSender commandSender)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(evt.OrderId);

            var transferCommand = new CreateTransferCommand
            {
                OrderId = evt.OrderId,
                TransferId = transaction.MeTransactionId,
                AssetId = transaction.AssetId,
                ClientId = transaction.ClientId,
                SourceClientId = _easyPaymentGatewaySettings.SourceClientId,
                Amount = transaction.Amount,
                WalletId = transaction.WalletId
            };

            commandSender.SendCommand(transferCommand, _easyPaymentGatewaySettings.MeContext);
        }

        public async Task Handle(TransferCreatedEvent evt, ICommandSender commandSender)
        {
            var command = new CompleteTransferCommand { Id = evt.OrderId, Amount = evt.Amount };

            commandSender.SendCommand(command, _easyPaymentGatewaySettings.EasyPaymentGatewayContext);
        }

        public async Task Handle(TransferCompletedEvent evt, ICommandSender commandSender)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(evt.OrderId);

            foreach (var notification in _paymentNotifiers)
                try
                {
                    await notification.NotifyAsync(transaction);
                }
                catch
                {
                    // TODO: process notifications at notification service
                }
        }
    }
}
