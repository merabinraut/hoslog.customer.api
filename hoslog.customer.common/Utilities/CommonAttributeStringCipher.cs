using System.Security.Cryptography;
using hoslog.customer.api.Library;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.common.Utilities
{
    public class CommonAttributeStringCipher
    {
        private readonly IConfiguration _configuration;
        private readonly byte[] _keyBytes;
        private readonly byte[] _ivBytes;

        public CommonAttributeStringCipher(IConfiguration configuration)
        {
            _configuration = configuration;
            _keyBytes = Convert.FromBase64String(_configuration["CommonAttributeSecurityKey:Key"]);
            _ivBytes = Convert.FromBase64String(_configuration["CommonAttributeSecurityKey:IV"]);
        }

        /// <summary>
        /// Key and IV generation algorithm
        /// </summary>
        /// <returns></returns>
        private (string Key, string IV) GenerateKeyAndIV()
        {
            using var aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.KeySize = 256;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.GenerateKey();
            aesAlg.GenerateIV();
            string key = Convert.ToBase64String(aesAlg.Key);
            string iv = Convert.ToBase64String(aesAlg.IV);
            return (key, iv);
        }

        private Aes CreateAesAlgorithm()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keyBytes;
            aes.IV = _ivBytes;
            return aes;
        }

        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            using var aesAlg = CreateAesAlgorithm();
            using var encryptor = aesAlg.CreateEncryptor();
            using var memoryStream = new MemoryStream();

            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText.Trim());
            }

            var encryptedBase64 = Convert.ToBase64String(memoryStream.ToArray());

            return URLHelper.ToUrlSafeBase64(encryptedBase64);
        }

        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public string Decrypt(string encryptedText)
        {
            encryptedText = !string.IsNullOrEmpty(encryptedText) ? URLHelper.FromUrlSafeBase64(encryptedText) : string.Empty;
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

            var encryptedTextBytes = Convert.FromBase64String(encryptedText);

            using var aesAlg = CreateAesAlgorithm();
            using var decryptor = aesAlg.CreateDecryptor();
            using var memoryStream = new MemoryStream(encryptedTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd().Trim();
        }
    }
}