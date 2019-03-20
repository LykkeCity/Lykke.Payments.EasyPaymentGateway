using System;
using System.Linq;
using System.Web;

namespace Lykke.Payments.EasyPaymentGateway.DomainServices.Sdk
{
    public static class QueryStringExtensions
    {
        public static string BuildEncodedQueryString(this object src, bool camelCase = true)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            var properties = from p in src.GetType().GetProperties()
                             where p.GetValue(src, null) != null
                             select (camelCase ? p.Name.ToCamelCase() : p.Name) + "=" + HttpUtility.UrlEncode(p.GetValue(src, null).ToString());

            return string.Join("&", properties.ToArray());
        }
    }
}
