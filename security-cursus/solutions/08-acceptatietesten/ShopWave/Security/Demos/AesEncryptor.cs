using System.Security.Cryptography;
using System.Text;

namespace ShopWave.Security
{
    public class AesEncryptor
    {
        private readonly byte[] key;

        public AesEncryptor(string key)
        {
            string paddedKey = key.PadRight(32).Substring(0, 32);
            this.key = Encoding.UTF8.GetBytes(paddedKey);
        }

        public AesEncryptor(byte[] key)
        {
            this.key = key;
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] inputBytes     = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(
                    inputBytes, 0, inputBytes.Length);

                byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV,         0, result, 0,             aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                byte[] inputBytes     = Convert.FromBase64String(cipherText);
                byte[] iv             = new byte[16];
                byte[] encryptedBytes = new byte[inputBytes.Length - 16];

                Array.Copy(inputBytes, 0,  iv,             0, 16);
                Array.Copy(inputBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                ICryptoTransform decryptor     = aes.CreateDecryptor();
                byte[]           decryptedBytes = decryptor.TransformFinalBlock(
                    encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
