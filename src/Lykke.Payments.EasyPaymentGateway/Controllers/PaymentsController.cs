﻿using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Payments.Contracts;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Services;
using Lykke.Payments.EasyPaymentGateway.DomainServices;
using Lykke.Payments.EasyPaymentGateway.Models;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Lykke.Payments.EasyPaymentGateway.Sdk;
using Lykke.Cqrs;

namespace Lykke.Payments.EasyPaymentGateway.Controllers
{
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentUrlProvider _paymentUrlProvider;
        private readonly IPaymentSystemsRawLog _paymentSystemsRawLog;
        private readonly RedirectSettings _redirectSettings;
        private readonly EasyPaymentGatewaySettings _easyPaymentGatewaySettings;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log; 

        public PaymentsController(
            PaymentUrlProvider paymentUrlProvider, 
            IPaymentSystemsRawLog paymentSystemsRawLog,
            RedirectSettings redirectSettings,
            EasyPaymentGatewaySettings easyPaymentGatewaySettings,
            ICqrsEngine cqrsEngine,
            ILogFactory logFactory)
        {
            _paymentUrlProvider = paymentUrlProvider;
            _paymentSystemsRawLog = paymentSystemsRawLog;
            _redirectSettings = redirectSettings;
            _easyPaymentGatewaySettings = easyPaymentGatewaySettings;
            _cqrsEngine = cqrsEngine;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Get payment form url
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get source client id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetSourceClientId")]
        [Produces("text/plain")]
        public Task<string> GetSourceClientId()
        {
            return Task.FromResult(_easyPaymentGatewaySettings.SourceClientId);
        }

        /// <summary>
        /// Callback for status update
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("api/Status")]
        public async Task<IActionResult> PostStatus([FromBody] CallbackStatusModel model)
        {
            if (model == null)
            {
                await _log.WriteWarningAsync(
                    nameof(PaymentsController.PostStatus), 
                    string.Empty, 
                    "Status update request is empty");

                return BadRequest(ErrorResponse.Create("Request is empty"));
            }

            var request = model.ToJson();

            await _paymentSystemsRawLog.RegisterEventAsync(PaymentSystemRawLogEvent.Create(
                CashInPaymentSystem.EasyPaymentGateway, 
                "Status update", 
                request));

            var command = new CashInCommand
            {
                OrderId = model.Operations.GetMerchantTransactionId(),
                Request = request
            };

            _cqrsEngine.SendCommand(
                command, 
                _easyPaymentGatewaySettings.EasyPaymentGatewayContext, 
                _easyPaymentGatewaySettings.EasyPaymentGatewayContext);

            return Accepted();
        }
    }
}
