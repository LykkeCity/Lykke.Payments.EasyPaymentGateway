using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Workflow
{
    public class AntiFraudChecker
    {
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly DateTime _registrationDateSince;
        private readonly TimeSpan _paymentPeriod;
        private readonly IPersonalDataService _personalDataService;
        private readonly ICreditCardsService _creditCardsService;

        public AntiFraudChecker(
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IClientAccountClient clientAccountClient,
            DateTime registrationDateSince,
            TimeSpan paymentPeriod, 
            IPersonalDataService personalDataService, 
            ICreditCardsService creditCardsService)
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _clientAccountClient = clientAccountClient;
            _registrationDateSince = registrationDateSince;
            _paymentPeriod = paymentPeriod;
            _personalDataService = personalDataService;
            _creditCardsService = creditCardsService;
        }

        public async Task<bool> IsPaymentSuspicious(string clientId, string orderId)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(orderId);
            if (transaction.AntiFraudStatus != null)
            {
                if (transaction.AntiFraudStatus == AntiFraudStatus.Pending.ToString())
                {
                    return true;
                }
                if (transaction.AntiFraudStatus == AntiFraudStatus.NotFraud.ToString())
                {
                    // FCT-? no extra information about the card
                    if (!string.IsNullOrWhiteSpace(transaction.CardHash))
                    {
                        await _creditCardsService.Approve(transaction.CardHash, clientId);
                    }

                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(transaction.CardHash)) // FCT-? no extra information about the card
            {
                await _paymentTransactionsRepository.SetAntiFraudStatusAsync(orderId, AntiFraudStatus.Pending.ToString());
                return true;
            }

            var card = await _creditCardsService.GetCard(transaction.CardHash, clientId);
            if (card == null)
                throw new InvalidOperationException("Credit card is not found");

            if (card.Approved) // FCT-2
                return false;

            await _paymentTransactionsRepository.SetAntiFraudStatusAsync(orderId,
                AntiFraudStatus.Pending.ToString());

            return true;
        }

        public async Task<bool> IsClientSuspicious(string clientId)
        {
            return await IsClientRegistrationFresh(clientId) || !await HasSuccessfulCreditCardDepositHistoryInPast(clientId);
        }

        private async Task<bool> IsClientRegistrationFresh(string clientId)
        {
            var registrationDate = (await _clientAccountClient.GetByIdAsync(clientId)).Registered;
            return registrationDate > _registrationDateSince;
        }

        private async Task<bool> HasSuccessfulCreditCardDepositHistoryInPast(string clientId)
        {
            return await _paymentTransactionsRepository.HasProcessedTransactionsAsync(clientId, DateTime.UtcNow.Subtract(_paymentPeriod));
        }

        private async Task<bool> IsCardUsedByOtherClients(string clientId, CreditCardModel card)
        {
            return (await _creditCardsService.GetCardsByHash(card.Hash)).Any(x => x.ClientId != clientId);
        }

        private async Task<bool> IsCardHolderNameValid(string clientId, CreditCardModel card)
        {
            var personalData = await _personalDataService.GetAsync(clientId);
            return IsNameSimilar(card.CardHolder, personalData.FirstName, personalData.LastName);
        }

        private static bool IsNameSimilar(string cardHolderName, string firstName, string lastName)
        {
            return Compare(cardHolderName.ToLower(), $"{firstName} {lastName}".ToLowerInvariant())
                   || Compare(cardHolderName.ToLower(), $"{lastName} {firstName}".ToLowerInvariant());
        }

        private static bool Compare(string expected, string actual)
        {
            const double allowedSimilarity = 0.8;
            var metric = new SimMetrics.Net.Metric.Levenstein();
            var v = metric.GetSimilarity(expected, actual);
            return v > allowedSimilarity;
        }
    }

    public enum AntiFraudStatus
    {
        Pending,
        NotFraud
    }

    public enum AntiFraudSuspicionReason
    {
        None,
        InvalidCardHolderName,
        CardIsUsedByOtherClients
    }
}
