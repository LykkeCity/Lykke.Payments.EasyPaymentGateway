using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Payments.EasyPaymentGateway.DomainServices
{
    public class PaymentUrlProvider
    {
        public async Task<string> GetPaymentUrlAsync(string orderId, string clientId, decimal amount, string assetId, string otherInfoJson)
        {
            throw new NotImplementedException();
        }
    }
}
