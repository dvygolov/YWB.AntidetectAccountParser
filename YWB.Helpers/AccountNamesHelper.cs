using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.Helpers
{
    public static class AccountNamesHelper
    {
        public static void Process(IEnumerable<SocialAccount> accounts,FlowSettings fs)
        {
            if (accounts.All(a => !string.IsNullOrEmpty(a.Name))) return;
            int i = 0;
            foreach (var acc in accounts)
            {
                acc.Name = $"{fs.NamingPrefix}{i + fs.NamingIndex}";
                i++;
            }
        }
    }
}