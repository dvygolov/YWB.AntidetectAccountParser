using System.Collections.Generic;
using System.Linq;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Helpers
{
    internal static class AccountNamesHelper
    {
        internal static void Process(IEnumerable<SocialAccount> accounts,FlowSettings fs)
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