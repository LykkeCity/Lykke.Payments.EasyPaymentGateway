using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Payments.EasyPaymentGateway.DomainServices.Sdk;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.DomainServices
{
    public class PaymentUrlProvider
    {
        private readonly string _merchantId;
        private readonly string _merchantPassword;
        private readonly string _productId;
        private readonly string _webHookStatusUrl;
        private readonly HttpClient _httpClient;
        private readonly ILog _log;

        #region consts
        private const string Language = "EN";
        private const string TransactionDescription = "Lykke.Payments.EasyPaymentGateway transaction";
        #endregion

        public PaymentUrlProvider(
            string merchantId,
            string merchantPassword,
            string productId,
            string webHookStatusUrl,
            IHttpClientFactory httpClientFactory,
            ILogFactory logFactory)
        {
            _merchantId = merchantId;
            _merchantPassword = merchantPassword;
            _productId = productId;
            _webHookStatusUrl = webHookStatusUrl;
            _httpClient = httpClientFactory.CreateClient("epg");
            _log = logFactory.CreateLog(this);
        }

        public async Task<string> GetPaymentUrlAsync(string orderId, string clientId, double amount, string assetId, string otherInfoJson, string operationType = "debit")
        {
            if (string.IsNullOrEmpty(orderId))
                throw new ArgumentNullException(nameof(orderId));

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentNullException(nameof(assetId));

            if (otherInfoJson == null)
                throw new ArgumentNullException("Info can't be empty!");

            var otherInfo = otherInfoJson.DeserializeJson<OtherPaymentInfo>();
            if (otherInfo == null)
                throw new ArgumentException("Info must be deserialized into OtherPaymentInfo");

            if (!string.IsNullOrWhiteSpace(otherInfo.Country) && otherInfo.Country.Length == 3)
                otherInfo.Country = CountryManager.Iso3ToIso2(otherInfo.Country);

            var requestPayload = new
            {
                OperationType = operationType,
                MerchantId = _merchantId,
                CustomerId = clientId,
                CustomerEmail = otherInfo.Email,
                Amount = amount.ToString(CultureInfo.InvariantCulture),
                Description = TransactionDescription,
                Country = otherInfo.Country,
                Currency = assetId,
                AddressLine1 = otherInfo.Address,
                City = otherInfo.City,
                PostCode = otherInfo.Zip,
                Telephone = otherInfo.ContactPhone,
                FirstName = otherInfo.FirstName,
                LastName = otherInfo.LastName,
                CustomerCountry = otherInfo.Country,
                MerchantTransactionId = orderId,
                ProductId = _productId,
                Language = Language,
                Dob = otherInfo.DateOfBirth,
                StatusURL = _webHookStatusUrl,
                PaymentSolution = "CreditCards"
            };

            var requestQueryString = requestPayload.BuildEncodedQueryString();

            var encryptedQueryString = requestQueryString.Encrypt(_merchantPassword);

            var integrityCheck = requestQueryString.Hash();

            var request = new
            {
                Encrypted = encryptedQueryString,
                IntegrityCheck = integrityCheck,
                MerchantId = _merchantId
            };

            _log.Info(nameof(PaymentUrlProvider.GetPaymentUrlAsync), request.ToJson(), "EPG payment form request");

            var response = await _httpClient.PostAsync
                ("/EPGCheckout/rest/online/tokenize",
                new StringContent(request.BuildEncodedQueryString(), Encoding.UTF8, "application/x-www-form-urlencoded"));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            _log.Info(nameof(PaymentUrlProvider.GetPaymentUrlAsync), $"paymentFormUrl = {result}, Payment form url");

            return result;
        }
    }
}
