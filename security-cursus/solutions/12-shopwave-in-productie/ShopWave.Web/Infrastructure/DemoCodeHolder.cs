namespace ShopWave.Web.Infrastructure
{
    // DEMO-INFRASTRUCTUUR. In een echte webshop gaat een 2FA-code of resetcode per
    // e-mail of sms naar de klant. Die kanalen hebben we hier niet. Deze klasse vangt
    // de code op via de callback-techniek uit de cursus, zodat de demo hem op het
    // scherm kan tonen. In productie zou je dit nooit doen.
    public class DemoCodeHolder
    {
        private readonly Dictionary<string, string> codes;

        public DemoCodeHolder()
        {
            codes = new Dictionary<string, string>();
        }

        public void Store(string email, string code)
        {
            codes[email] = code;
        }

        public string GetCode(string email)
        {
            string result = string.Empty;

            if (codes.ContainsKey(email))
            {
                result = codes[email];
            }

            return result;
        }
    }
}
