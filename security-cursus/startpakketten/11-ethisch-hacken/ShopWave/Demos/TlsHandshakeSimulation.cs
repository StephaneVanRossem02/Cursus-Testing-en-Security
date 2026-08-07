using System.Security.Cryptography;
using ShopWave.Security;

namespace ShopWave
{
    // Demo uit de oplossingen: de TLS-handshake simuleren met RSA (sessiesleutel uitwisselen)
    // en AES (verdere communicatie). Zelfstandig uitvoerbaar, geen netwerk nodig.
    public static class TlsHandshakeSimulation
    {
        public static void Run()
        {
            // Stap 1: server genereert RSA-sleutelpaar
            RSA serverRsa = RSA.Create(2048);

            // Stap 2: client genereert sessiesleutel
            byte[] sessionKey = new byte[32];
            RandomNumberGenerator.Fill(sessionKey);

            Console.WriteLine($"Sessiesleutel (origineel):  {Convert.ToHexString(sessionKey)[..32]}...");

            // Stap 3: client versleutelt sessiesleutel met publieke RSA-sleutel
            byte[] encryptedSessionKey = serverRsa.Encrypt(
                sessionKey,
                RSAEncryptionPadding.OaepSHA256);

            Console.WriteLine($"Verstuurd (versleuteld):    {Convert.ToHexString(encryptedSessionKey)[..32]}...");

            // Stap 4: server ontsleutelt met private sleutel
            byte[] decryptedSessionKey = serverRsa.Decrypt(
                encryptedSessionKey,
                RSAEncryptionPadding.OaepSHA256);

            Console.WriteLine($"Sessiesleutel (ontvangen):  {Convert.ToHexString(decryptedSessionKey)[..32]}...");

            // Stap 5: vergelijken
            bool keysMatch = sessionKey.SequenceEqual(decryptedSessionKey);
            Console.WriteLine($"Sleutels gelijk:            {keysMatch}");

            // Stap 6: AES-communicatie met de gedeelde sessiesleutel
            AesEncryptor encryptor = new AesEncryptor(sessionKey);
            string message          = "alice@shopwave.be | Laptop | 999.99 EUR";
            string encrypted        = encryptor.Encrypt(message);
            string decrypted        = encryptor.Decrypt(encrypted);

            Console.WriteLine($"\nBericht:     {message}");
            Console.WriteLine($"Versleuteld: {encrypted[..40]}...");
            Console.WriteLine($"Ontsleuteld: {decrypted}");

            serverRsa.Dispose();
        }
    }
}
