using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lykke.Payments.EasyPaymentGateway.DomainServices.Sdk
{
    public static class EncryptionExtensions
    {
        public static string Encrypt(this string src, string key)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentNullException(nameof(src));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var aes = new AesManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var crypt = aes.CreateEncryptor();
            byte[] encBytes = Encoding.UTF8.GetBytes(src);
            byte[] resultBytes = crypt.TransformFinalBlock(encBytes, 0, encBytes.Length);

            return Convert.ToBase64String(resultBytes);
        }

        public static string Hash(this string src)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentNullException(nameof(src));

            using (var hash = SHA256.Create())
            {
                return string.Join("", hash
                  .ComputeHash(Encoding.UTF8.GetBytes(src))
                  .Select(item => item.ToString("x2")));
            }
        }
    }
}
