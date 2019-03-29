using Lykke.Payments.EasyPaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Payments.EasyPaymentGateway.Sdk
{
    public static class OperationExtensions
    {
        public static string GetMerchantTransactionId(this List<Operation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return operations.GetSingleOperation().MerchantTransactionId;
        }

        public static string GetMessage(this List<Operation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return operations.Last().Message;
        }

        public static Operation GetSingleOperation(this List<Operation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return operations.Single();
        }
    }
}
