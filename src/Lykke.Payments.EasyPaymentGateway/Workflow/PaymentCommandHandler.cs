using Lykke.Contracts.Payments;
using Lykke.Cqrs;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Models;
using Lykke.Payments.EasyPaymentGateway.Sdk;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Lykke.Payments.EasyPaymentGateway.Workflow.Events;
using Newtonsoft.Json;
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

            var operation = request.Operations.GetSingleOperation();

            var tx = await _paymentTransactionsRepository.GetByTransactionIdAsync(command.OrderId);

            if (tx != null && (tx.Status == PaymentStatus.NotifyDeclined || tx.Status == PaymentStatus.NotifyProcessed || tx.Status == PaymentStatus.Processing))
            {
                return CommandHandlingResult.Ok();
            }

            if (operation.PaymentDetails != null)
            {
                await _paymentTransactionsRepository.SaveCardHashAsync(command.OrderId, operation.PaymentDetails.CardNumberToken);

                if (tx != null)
                {
                    eventPublisher.PublishEvent(new CreditCardUsedEvent
                    {
                        ClientId = tx.ClientId,
                        OrderId = command.OrderId,
                        CardHash = operation.PaymentDetails.CardNumberToken,
                        CardNumber = operation.PaymentDetails.CardNumber,
                        CustomerName = operation.PaymentDetails.CardHolderName
                    });
                }
            }

            if (operation.Succeeded)
            {
                tx = await _paymentTransactionsRepository.StartProcessingTransactionAsync(command.OrderId, operation.PayFrexTransactionId);
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
                await _paymentTransactionsRepository.SetStatusAsync(command.OrderId, PaymentStatus.NotifyDeclined);

                await _paymentTransactionEventsLog.WriteAsync(
                    PaymentTransactionLogEvent.Create(
                        command.OrderId, command.Request, $"Declined by Payment status from payment system, status = {operation.Status}", nameof(CashInCommand)));

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
