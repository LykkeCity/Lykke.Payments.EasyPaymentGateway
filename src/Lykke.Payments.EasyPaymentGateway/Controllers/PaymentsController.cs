﻿using Lykke.Payments.Contracts;
using Lykke.Payments.EasyPaymentGateway.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        [HttpPost]
        [Route("GetPaymentUrl")]
        public async Task<GetUrlDataResult> GetUrl([FromBody] GetUrlDataRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}