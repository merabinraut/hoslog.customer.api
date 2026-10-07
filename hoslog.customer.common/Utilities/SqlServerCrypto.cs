using System.Security.Cryptography;
using System.Text;

namespace hoslog.customer.common.Utilities
{
    public static class SqlServerCrypto
    {
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string EncryptDbAES256ToBase64(string password, string plainTextToEncrypt)
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = DeriveKey(password, aesAlg.KeySize / 8);
                aesAlg.GenerateIV();
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                byte[] encryptedData;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                {
                    byte[] plainTextBytes = Encoding.Unicode.GetBytes(plainTextToEncrypt);

                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                        }

                        encryptedData = msEncrypt.ToArray();
                    }
                }

                return Convert.ToBase64String(encryptedData);
            }
        }

        public static byte[] EncryptDbAES256ToVarbinary(string password, string plainTextToEncrypt)
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = DeriveKey(password, aesAlg.KeySize / 8);
                aesAlg.GenerateIV();
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                byte[] encryptedData;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                {
                    byte[] plainTextBytes = Encoding.Unicode.GetBytes(plainTextToEncrypt);

                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                        }

                        encryptedData = msEncrypt.ToArray();
                    }
                }

                return encryptedData;
            }
        }

        public static string DecryptDbAES256(string password, string encryptedData)
        {
            byte[] encryptedDataBytes = Convert.FromBase64String(encryptedData);

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = DeriveKey(password, aesAlg.KeySize / 8);

                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(encryptedDataBytes, iv, iv.Length);
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                {
                    using (var msDecrypt = new MemoryStream(encryptedDataBytes, iv.Length, encryptedDataBytes.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        public static string DecryptDbAES256(string password, byte[] encryptedDataBytes)
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = DeriveKey(password, aesAlg.KeySize / 8);

                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(encryptedDataBytes, iv, iv.Length);
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                {
                    using (var msDecrypt = new MemoryStream(encryptedDataBytes, iv.Length, encryptedDataBytes.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private static byte[] DeriveKey(string password, int keySize)
        {
            const int saltSize = 16;
            const int iterations = 1000;
            byte[] salt = new byte[saltSize];
            new RNGCryptoServiceProvider().GetBytes(salt);

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return pbkdf2.GetBytes(keySize);
            }
        }

        /// <summary>
		/// SQL Server equivalent Encrypt By PassPhrase
		/// </summary>
		/// <param name="Password">encryption key</param>
		/// <param name="base64">Base 64 encoded or varbinary data</param>
		/// <param name="isBase64">if true data should be base64 else varbinary data. default base64 data</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string ENCRYPTBYPASSPHRASE(string Password, string base64, bool isBase64 = true, int version = 1)
        {
            try
            {
                string FromSql = base64;
                // Encode password as UTF16-LE
                byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);

                byte[] encrypted = Encoding.UTF8.GetBytes(base64);

                HashAlgorithm hashAlgo = null;
                SymmetricAlgorithm cryptoAlgo = null;
                int keySize = (version == 1 ? 16 : 32);

                if (version == 1)
                {
                    hashAlgo = SHA1.Create();
                    cryptoAlgo = TripleDES.Create();
                }
                else if (version == 2)
                {
                    hashAlgo = SHA256.Create();
                    cryptoAlgo = Aes.Create();
                }
                else
                {
                    throw new Exception("Unsupported encryption");
                }

                cryptoAlgo.Padding = PaddingMode.PKCS7;
                cryptoAlgo.Mode = CipherMode.CBC;

                cryptoAlgo.Key = hashAlgo.ComputeHash(passwordBytes).Take(keySize).ToArray();

                byte[] decrypted = cryptoAlgo.CreateEncryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
                int decryptLength = BitConverter.ToInt16(decrypted, 6);
                UInt32 magic = BitConverter.ToUInt32(decrypted, 0);
                if (magic != 0xbaadf00d)
                {
                    throw new Exception("Encrypt failed");
                }

                byte[] decryptedData = decrypted.Skip(8).ToArray();
                bool isUtf16 = (Array.IndexOf(decryptedData, (byte)0) != -1);
                string decryptText = (isUtf16 ? Encoding.Unicode.GetString(decryptedData) : Encoding.UTF8.GetString(decryptedData));

                return decryptText;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// SQL Server equivalent Decrypt By PassPhrase
        /// </summary>
        /// <param name="Password">encryption key</param>
        /// <param name="base64">Base 64 encoded or varbinary data</param>
        /// <param name="isBase64">if true data should be base64 else varbinary data. default base64 data</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DECRYPTBYPASSPHRASE(string Password, string base64, bool isBase64 = true)
        {
            try
            {
                string FromSql = "";
                if (isBase64)
                {
                    var text = Convert.FromBase64String(base64);
                    string hex = BitConverter.ToString(text);
                    hex = hex.Replace("-", "");
                    FromSql = "0x" + hex;
                }
                else
                {
                    FromSql = base64;
                }
                // Encode password as UTF16-LE
                byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);

                // Remove leading "0x"
                FromSql = FromSql.Substring(2);

                int version = BitConverter.ToInt32(StringToByteArray(FromSql.Substring(0, 8)), 0);
                byte[] encrypted = null;

                HashAlgorithm hashAlgo = null;
                SymmetricAlgorithm cryptoAlgo = null;
                int keySize = (version == 1 ? 16 : 32);

                if (version == 1)
                {
                    hashAlgo = SHA1.Create();
                    cryptoAlgo = TripleDES.Create();
                    cryptoAlgo.IV = StringToByteArray(FromSql.Substring(8, 16));
                    encrypted = StringToByteArray(FromSql.Substring(24));
                }
                else if (version == 2)
                {
                    hashAlgo = SHA256.Create();
                    cryptoAlgo = Aes.Create();
                    cryptoAlgo.IV = StringToByteArray(FromSql.Substring(8, 32));
                    encrypted = StringToByteArray(FromSql.Substring(40));
                }
                else
                {
                    throw new Exception("Unsupported encryption");
                }

                cryptoAlgo.Padding = PaddingMode.PKCS7;
                cryptoAlgo.Mode = CipherMode.CBC;

                hashAlgo.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
                cryptoAlgo.Key = hashAlgo.Hash.Take(keySize).ToArray();

                byte[] decrypted = cryptoAlgo.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
                int decryptLength = BitConverter.ToInt16(decrypted, 6);
                UInt32 magic = BitConverter.ToUInt32(decrypted, 0);
                if (magic != 0xbaadf00d)
                {
                    throw new Exception("Decrypt failed");
                }

                byte[] decryptedData = decrypted.Skip(8).ToArray();
                bool isUtf16 = (Array.IndexOf(decryptedData, (byte)0) != -1);
                string decryptText = (isUtf16 ? Encoding.Unicode.GetString(decryptedData) : Encoding.UTF8.GetString(decryptedData));

                return decryptText;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// SQL Server equivalent PWDCOMPARE
        /// </summary>
        /// <param name="Password">Clear Text</param>
        /// <param name="ToCompare">Hashed Data (accepts <varbinary eg 0x01> if isBase64 is false</varbinary> )</param>
        /// <param name="isBase64">default true</param>
        /// <returns>bool true|false</returns>
        public static bool PWDCOMPARE(string Password, string ToCompare, bool isBase64 = true)
        {
            try
            {
                if (isBase64 == true)
                {
                    return PWDCOMPARE(Password, Convert.FromBase64String(ToCompare));
                }
                var encPassword = ToCompare.Substring(2);
                var encHeader = encPassword.Substring(0, 4);
                int version = Convert.ToInt32(encHeader.Substring(1, 1));
                var encSalt = StringToByteArray(encPassword.Substring(4, 8));
                var encPasswordBytes = StringToByteArray(encPassword.Substring(12));
                HashAlgorithm shaHasher = null;
                if (version == 1)
                    shaHasher = SHA1.Create();
                else if (version == 2)
                    shaHasher = SHA512.Create();
                //var hash = sha512Hasher.ComputeHash(Encoding.Unicode.GetBytes(Password + Encoding.Unicode.GetString(encSalt)));
                var hash = shaHasher.ComputeHash(Combine(Encoding.Unicode.GetBytes(Password), encSalt));
                if (hash.SequenceEqual(encPasswordBytes))
                {
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        /// <summary>
        /// SQL Server equivalent PWDCOMPARE
        /// </summary>
        /// <param name="Password">Clear Text</param>
        /// <param name="Array">directly takes byte[] of encrypted passowrd</param>
        /// <returns>bool true|false</returns>
        public static bool PWDCOMPARE(string Password, byte[] Array)
        {
            try
            {
                var encPassword = ByteArrayToHexString(Array);
                var encHeader = encPassword.Substring(0, 4);
                int version = Convert.ToInt32(encHeader.Substring(1, 1));
                var encSalt = StringToByteArray(encPassword.Substring(4, 8));
                var encPasswordBytes = StringToByteArray(encPassword.Substring(12));
                HashAlgorithm shaHasher = null;
                if (version == 1)
                    shaHasher = SHA1.Create();
                else if (version == 2)
                    shaHasher = SHA512.Create();
                //var hash = sha512Hasher.ComputeHash(Encoding.Unicode.GetBytes(Password + Encoding.Unicode.GetString(encSalt)));
                var hash = shaHasher.ComputeHash(Combine(Encoding.Unicode.GetBytes(Password), encSalt));
                if (hash.SequenceEqual(encPasswordBytes))
                {
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public static byte[] PWDENCRYPTVarbinary(string password, int Version = 2)
        {
            try
            {
                if (password == null) password = "";

                // 1. Generating 4-byte salt
                byte[] salt = new byte[4];
                using (var random = new RNGCryptoServiceProvider())
                {
                    random.GetBytes(salt);
                }

                HashAlgorithm shaHasher = null;
                if (Version == 1)
                    shaHasher = SHA1.Create();
                else if (Version == 2)
                    shaHasher = SHA512.Create();

                // 2. Calculating SHA-512 hash

                byte[] passwordBytes = Encoding.Unicode.GetBytes(password); // Assuming mssql's nvarchar is UTF-16LE
                shaHasher.TransformBlock(passwordBytes, 0, passwordBytes.Length, passwordBytes, 0);
                shaHasher.TransformBlock(salt, 0, salt.Length, salt, 0);
                shaHasher.TransformFinalBlock(new byte[0], 0, 0);
                byte[] hash = shaHasher.Hash;

                // 3. Formatting the result in MSSQL PwdEncrypt format
                string saltHex = BitConverter.ToString(salt).Replace("-", "");
                string hashHex = BitConverter.ToString(hash).Replace("-", "");
                return HexStringToByteArray("0x0" + Version.ToString() + "00" + saltHex + hashHex);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw new InvalidOperationException(e.Message);
            }
        }
        public static string PWDENCRYPT(string password, int Version = 2)
        {
            try
            {
                if (password == null) password = "";

                // 1. Generating 4-byte salt
                byte[] salt = new byte[4];
                using (var random = new RNGCryptoServiceProvider())
                {
                    random.GetBytes(salt);
                }

                HashAlgorithm shaHasher = null;
                if (Version == 1)
                    shaHasher = SHA1.Create();
                else if (Version == 2)
                    shaHasher = SHA512.Create();

                // 2. Calculating SHA-512 hash

                byte[] passwordBytes = Encoding.Unicode.GetBytes(password); // Assuming mssql's nvarchar is UTF-16LE
                shaHasher.TransformBlock(passwordBytes, 0, passwordBytes.Length, passwordBytes, 0);
                shaHasher.TransformBlock(salt, 0, salt.Length, salt, 0);
                shaHasher.TransformFinalBlock(new byte[0], 0, 0);
                byte[] hash = shaHasher.Hash;

                // 3. Formatting the result in MSSQL PwdEncrypt format
                string saltHex = BitConverter.ToString(salt).Replace("-", "");
                string hashHex = BitConverter.ToString(hash).Replace("-", "");
                return Convert.ToBase64String(HexStringToByteArray("0x0" + Version.ToString() + "00" + saltHex + hashHex));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw new InvalidOperationException(e.Message);
            }
        }
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace("0x", ""); // Remove the "0x" prefix
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hexString = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hexString.AppendFormat("{0:X2}", b);
            }
            return hexString.ToString();
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
