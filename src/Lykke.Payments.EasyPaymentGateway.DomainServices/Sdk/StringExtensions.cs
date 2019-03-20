using System;

namespace Lykke.Payments.EasyPaymentGateway.DomainServices.Sdk
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string src)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentNullException(nameof(src));

            return char.ToLowerInvariant(src[0]) + src.Substring(1);
        }
    }
}
