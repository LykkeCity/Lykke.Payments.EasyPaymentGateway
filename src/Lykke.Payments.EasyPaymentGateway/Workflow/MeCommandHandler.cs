using Lykke.Cqrs;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Lykke.Payments.EasyPaymentGateway.Workflow.Events;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.Models;
using Lykke.Service.FeeCalculator.Client;
using System;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Workflow
{
    public class MeCommandHandler
    {
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly FeeSettings _feeSettings;
        private readonly AntiFraudChecker _antiFraudChecker;

        public MeCommandHandler(
            IExchangeOperationsServiceClient exchangeOperationsService,
            IPaymentTransactionEventsLog paymentTransactionEventsLog,
            IFeeCalculatorClient feeCalculatorClient,
            FeeSettings feeSettings,
            AntiFraudChecker antiFraudChecker)
        {
            _exchangeOperationsService = exchangeOperationsService;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _feeSettings = feeSettings;
            _antiFraudChecker = antiFraudChecker;
            _feeCalculatorClient = feeCalculatorClient;
        }

        public async Task<CommandHandlingResult> Handle(CreateTransferCommand createTransferCommand, IEventPublisher eventPublisher)
        {
            if (await _antiFraudChecker.IsPaymentSuspicious(createTransferCommand.ClientId, createTransferCommand.OrderId))
            {
                return new CommandHandlingResult { Retry = true, RetryDelay = (long)TimeSpan.FromMinutes(10).TotalMilliseconds };
            }

            var bankCardFees = await _feeCalculatorClient.GetBankCardFees();

            var result = await _exchangeOperationsService.ExchangeOperations.TransferWithNotificationAsync(new TransferWithNotificationRequestModel
            {
                TransferId = createTransferCommand.TransferId,
                DestClientId = createTransferCommand.ClientId,
                SourceClientId = createTransferCommand.SourceClientId,
                Amount = createTransferCommand.Amount,
                AssetId = createTransferCommand.AssetId,
                FeeClientId = _feeSettings.TargetClientId.BankCard,
                FeeSizePercentage = bankCardFees.Percentage,
                DestWalletId = createTransferCommand.WalletId
            });

            if (!result.IsOk())
            {
                await _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(createTransferCommand.OrderId, "N/A", $"{result.Code}:{result.Message}", nameof(CreateTransferCommand)));

                switch (result.Code)
                {
                    case 401: // LOW BALANCE
                        return CommandHandlingResult.Ok();
                        //throw new InvalidOperationException($"low balance: {createTransferCommand.SourceClientId}");
                }
            }

            eventPublisher.PublishEvent(new TransferCreatedEvent
            {
                OrderId = createTransferCommand.OrderId,
                TransferId = createTransferCommand.TransferId,
                ClientId = createTransferCommand.ClientId,
                Amount = createTransferCommand.Amount,
                AssetId = createTransferCommand.AssetId
            });

            return CommandHandlingResult.Ok();
        }
    }
}
