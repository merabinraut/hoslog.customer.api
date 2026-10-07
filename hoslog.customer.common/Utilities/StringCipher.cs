using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using hoslog.customer.shared.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using hoslog.customer.api.Library;

namespace hoslog.customer.common.Utilities
{
    public class StringCipher
    {
        public string Signature { get; set; } = string.Empty;
        private readonly byte[] _keyBytes;
        private readonly byte[] _ivBytes;
        private readonly byte[] _secretKeyBytes;
        public StringCipher(IOptions<SecurityKeyCommon> securityKey)
        {
            _keyBytes = Convert.FromBase64String(securityKey.Value.Key);
            _ivBytes = Convert.FromBase64String(securityKey.Value.IV);
            _secretKeyBytes = Encoding.UTF8.GetBytes(securityKey.Value.SecretKey);
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
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using var aesAlg = CreateAesAlgorithm();
            using var encryptor = aesAlg.CreateEncryptor();
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var streamWriter = new StreamWriter(cryptoStream);

            streamWriter.Write(plainText.Trim());
            streamWriter.Flush();
            cryptoStream.FlushFinalBlock();
            return URLHelper.ToUrlSafeBase64(Convert.ToBase64String(memoryStream.ToArray()));
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

        #region Signature

        private string ComputeHmacSha512Hash(string request)
        {
            var hash = new StringBuilder();
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            using (var hmac = new HMACSHA512(_secretKeyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(requestBytes);
                foreach (var item in hashValue)
                {
                    hash.Append(item.ToString("X2"));
                }
            }
            return hash.ToString();
        }

        /// <summary>
        /// Verify signature
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool VerifySign<T>(T request) => Signature == ComputeHmacSha512Hash(SerializeObjectForSignature(request));

        private static string SerializeObjectForSignature<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Get the type of the object
            var type = obj.GetType();

            // Create a dictionary to hold property names and values
            var properties = new SortedDictionary<string, object>();

            // Use reflection to get properties
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                // Get the value of each property and add to dictionary
                var value = prop.GetValue(obj);
                if (value != null)
                    properties.Add(prop.Name, value);
            }

            // Convert the dictionary to a JSON object
            var jsonObject = JObject.FromObject(properties);

            // Serialize to JSON string
            return jsonObject.ToString(Formatting.None);
        }
        #endregion
    }
}
