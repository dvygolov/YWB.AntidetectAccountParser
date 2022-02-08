namespace YWB.AntidetectAccountsParser.Model.Accounts
{
    public class FacebookAccount : SocialAccount
    {
        public FacebookAccount() : base()
        {
            Domain = "facebook.com";
        }
        public FacebookAccount(string name) : base(name)
        {
            Domain = "facebook.com";
        }
        public string Birthday { get; set; }
        public string EmailLogin { get; set; }
        public string EmailPassword { get; set; }
        public string TwoFactor { get; set; }
        public string BmLinks { get; set; }
        public string Token { get; set; }
        public string BmToken { get; set; }

        public override string ToString(bool toHtml = false, bool withCookies = true)
        {
            var sd = toHtml ? "<p>" : "\n"; //start delimiter
            var ed = toHtml ? "</p>" : string.Empty; //end delimiter
            string str = string.Empty;
            for (int i = 0; i < _logins.Count; i++)
            {
                str += $"{sd}Facebook: {_logins[i]}:{_passwords[i]}{ed}";
            }
            if (!string.IsNullOrEmpty(Birthday))
                str += $"{sd}Birthday: {Birthday}{ed}";
            if (!string.IsNullOrEmpty(TwoFactor))
                str += $"{sd}2FA: {TwoFactor}{ed}";
            if (!string.IsNullOrEmpty(EmailPassword))
            {
                var eLogin = EmailLogin ?? Login;
                str += $"{sd}Email: {eLogin}:{EmailPassword}{ed}";
            }
            if (!string.IsNullOrEmpty(Token))
            {
                str += $"{sd}Token: {Token} {ed}";
            }
            if (!string.IsNullOrEmpty(BmToken))
            {
                str += $"{sd}BmToken: {BmToken} {ed}";
            }
            if (!string.IsNullOrEmpty(BmLinks))
            {
                str += $"{sd}BmLinks: {BmLinks} {ed}";
            }
            if (withCookies)
            {
                str = CookiesToString(str, sd, ed);
            }
            return str;
        }
    }
}
