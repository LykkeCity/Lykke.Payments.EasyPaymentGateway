using Common;
using Common.Log;
using Lykke.Common.Log;
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
    public class PaymentsController : Controller
    {
        private readonly PaymentUrlProvider _paymentUrlProvider;
        private readonly IPaymentSystemsRawLog _paymentSystemsRawLog;
        private readonly RedirectSettings _redirectSettings;
        private readonly EasyPaymentGatewaySettings _easyPaymentGatewaySettings;
        private readonly ILog _log; 

        public PaymentsController(
            PaymentUrlProvider paymentUrlProvider, 
            IPaymentSystemsRawLog paymentSystemsRawLog,
            RedirectSettings redirectSettings,
            EasyPaymentGatewaySettings easyPaymentGatewaySettings,
            ILogFactory logFactory)
        {
            _paymentUrlProvider = paymentUrlProvider;
            _paymentSystemsRawLog = paymentSystemsRawLog;
            _redirectSettings = redirectSettings;
            _easyPaymentGatewaySettings = easyPaymentGatewaySettings;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [HttpPost]
        [Route("easypaymentgateway/ok")]
        public async Task<ActionResult> Ok()
        {
            await _paymentSystemsRawLog.RegisterEventAsync(
                PaymentSystemRawLogEvent.Create(CashInPaymentSystem.EasyPaymentGateway, "Ok page", Request.QueryString.ToString()));

            return View();
        }

        [HttpGet]
        [HttpPost]
        [Route("easypaymentgateway/fail")]
        public async Task<ActionResult> Fail()
        {
            await _paymentSystemsRawLog.RegisterEventAsync(
                PaymentSystemRawLogEvent.Create(CashInPaymentSystem.EasyPaymentGateway, "Fail page", Request.QueryString.ToString()));

            return View();
        }

        [HttpPost]
        [Route("api/GetPaymentUrl")]
        public async Task<GetUrlDataResult> GetPaymentUrl([FromBody] GetUrlDataRequestModel request)
        {
            await _log.WriteInfoAsync(nameof(PaymentsController.GetPaymentUrl), request.ToJson(), "Diagnostic logging");

            _log.Warning("Test log", "Incoming request for payment form url");

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

                result = new GetUrlDataResult
                {
                    PaymentUrl = url,
                    OkUrl = _redirectSettings.OkUrl,
                    FailUrl = _redirectSettings.ErrorUrl,
                    ReloadRegexp = "^$",
                    UrlsRegexp = "^$"
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

        [HttpGet]
        [Route("api/GetSourceClientId")]
        [Produces("text/plain")]
        public Task<string> GetSourceClientId() => Task.FromResult(_easyPaymentGatewaySettings.SourceClientId);
    }
}
