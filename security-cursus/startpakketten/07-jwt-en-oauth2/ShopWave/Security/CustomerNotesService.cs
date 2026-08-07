namespace ShopWave.Security
{
    public class CustomerNotesService
    {
        private const  string        KeyString = "ShopWaveNotitiesSleutel!";
        private readonly AesEncryptor  aes;
        private readonly Dictionary<string, string> encryptedNotes;

        public CustomerNotesService()
        {
            aes            = new AesEncryptor(KeyString);
            encryptedNotes = new Dictionary<string, string>();
        }

        public void AddNote(string email, string note)
        {
            encryptedNotes[email] = aes.Encrypt(note);
        }

        public string GetNote(string email)
        {
            string result;

            if (!encryptedNotes.ContainsKey(email))
            {
                result = string.Empty;
            }
            else
            {
                result = aes.Decrypt(encryptedNotes[email]);
            }

            return result;
        }

        public bool HasNote(string email)
        {
            return encryptedNotes.ContainsKey(email);
        }

        public void DeleteNote(string email)
        {
            encryptedNotes.Remove(email);
        }

        public Dictionary<string, string> ExportEncryptedNotes()
        {
            return new Dictionary<string, string>(encryptedNotes);
        }
    }
}
