using Lykke.Contracts.Payments;
using Lykke.Cqrs;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Models;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Lykke.Payments.EasyPaymentGateway.Workflow.Events;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Workflow
{
    public class PaymentCommandHandler
    {
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;

        public PaymentCommandHandler(
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPaymentTransactionEventsLog paymentTransactionEventsLog)
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
        }

        public async Task<CommandHandlingResult> Handle(CashInCommand command, IEventPublisher eventPublisher)
        {
            var request = JsonConvert.DeserializeObject<CallbackStatusModel>(command.Request);

            var transactionOperations = request?.Operations.Where(x => x.MerchantTransactionId == command.OrderId);

            var operationPaymentDetails = transactionOperations.First().PaymentDetails;

            var tx = await _paymentTransactionsRepository.GetByTransactionIdAsync(command.OrderId);

            if (tx != null && (tx.Status == PaymentStatus.NotifyDeclined || tx.Status == PaymentStatus.NotifyProcessed || tx.Status == PaymentStatus.Processing))
            {
                return CommandHandlingResult.Ok();
            }

            if (operationPaymentDetails != null)
            {
                await _paymentTransactionsRepository.SaveCardHashAsync(command.OrderId, operationPaymentDetails.CardNumberToken);

                if (tx != null)
                {
                    eventPublisher.PublishEvent(new CreditCardUsedEvent
                    {
                        ClientId = tx.ClientId,
                        OrderId = command.OrderId,
                        CardHash = operationPaymentDetails.CardNumberToken,
                        CardNumber = operationPaymentDetails.CardNumber,
                        CustomerName = operationPaymentDetails.CardHolderName
                    });
                }
            }

            if (transactionOperations.Any(x => x.Succeeded))
            {
                var succeededOperations = transactionOperations.Where(x => x.Succeeded);

                var providerTransactionId = succeededOperations.Select(x => x.PayFrexTransactionId).Distinct().Single();

                tx = await _paymentTransactionsRepository.StartProcessingTransactionAsync(command.OrderId, providerTransactionId);
                if (tx != null) // initial status
                {
                    eventPublisher.PublishEvent(new ProcessingStartedEvent
                    {
                        OrderId = command.OrderId
                    });
                }

                return CommandHandlingResult.Ok();
            }
            else
            {
                var statuses = transactionOperations.Select(x => x.Status).Distinct();

                await _paymentTransactionsRepository.SetStatusAsync(command.OrderId, PaymentStatus.NotifyDeclined);

                await _paymentTransactionEventsLog.WriteAsync(
                    PaymentTransactionLogEvent.Create(
                        command.OrderId, command.Request, $"Declined by Payment status from payment system, operation statuses = [{string.Join(',', statuses)}]", nameof(CashInCommand)));

                return CommandHandlingResult.Ok();
            }
        }

        public async Task<CommandHandlingResult> Handle(CompleteTransferCommand cmd, IEventPublisher eventPublisher)
        {
            await _paymentTransactionsRepository.SetAsOkAsync(cmd.Id, cmd.Amount, null);

            await _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(cmd.Id, "", "Transaction processed as Ok", nameof(CompleteTransferCommand)));

            eventPublisher.PublishEvent(new TransferCompletedEvent { OrderId = cmd.Id });

            return CommandHandlingResult.Ok();
        }
    }
}
