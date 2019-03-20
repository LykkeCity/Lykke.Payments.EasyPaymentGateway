using Common;
using Lykke.Contracts.Payments;
using Lykke.Payments.Contracts;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Services;
using Lykke.Payments.EasyPaymentGateway.DomainServices;
using Lykke.Payments.EasyPaymentGateway.Models;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Controllers
{
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentUrlProvider _paymentUrlProvider;
        private readonly IPaymentSystemsRawLog _paymentSystemsRawLog;
        private readonly RedirectSettings _redirectSettings;

        public PaymentsController(
            PaymentUrlProvider paymentUrlProvider, 
            IPaymentSystemsRawLog paymentSystemsRawLog,
            RedirectSettings redirectSettings)
        {
            _paymentUrlProvider = paymentUrlProvider;
            _paymentSystemsRawLog = paymentSystemsRawLog;
            _redirectSettings = redirectSettings;
        }

        [HttpPost]
        [Route("api/GetPaymentUrl")]
        public async Task<GetUrlDataResult> GetPaymentUrl([FromBody] GetUrlDataRequestModel request)
        {
            if (request == null)
                return new GetUrlDataResult { ErrorMessage = "Invalid request data" };

            GetUrlDataResult result;

            try
            {
                string url = await _paymentUrlProvider.GetPaymentUrlAsync(
                    request.OrderId,
                    request.ClientId,
                    request.Amount,
                    request.AssetId,
                    request.OtherInfo);

                await _paymentSystemsRawLog.RegisterEventAsync(
                    PaymentSystemRawLogEvent.Create(CashInPaymentSystem.EasyPaymentGateway, "Payment Url has been created", request.ToJson()),
                    request.ClientId);

                var otherPaymentInfo = request.OtherInfo.DeserializeJson<OtherPaymentInfo>();
                var neverMatchUrlRegex = "^$";
                result = new GetUrlDataResult
                {
                    PaymentUrl = url,
                    OkUrl = otherPaymentInfo.OkUrl ?? _redirectSettings.OkUrl,
                    FailUrl = otherPaymentInfo.FailUrl ?? _redirectSettings.ErrorUrl,
                    ReloadRegexp = neverMatchUrlRegex,
                    UrlsRegexp = neverMatchUrlRegex
                };
            }
            catch (AggregateException exc)
            {
                result = new GetUrlDataResult
                {
                    ErrorMessage = exc.InnerExceptions[0].Message,
                };
            }
            catch (Exception exc)
            {
                result = new GetUrlDataResult
                {
                    ErrorMessage = exc.Message,
                };
            }

            return result;
        }
    }
}
