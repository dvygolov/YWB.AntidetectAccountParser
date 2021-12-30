using System.Collections.Generic;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Telegram
{
    public class BotFlow
    {
        public IEnumerable<SocialAccount> Accounts { get; set; }
        public List<Proxy> Proxies { get; set; }
        public IAccountsImporter Importer { get; set; }
        public string Group { get; set; }
        public string NamingPrefix { get; set; }
        public int? NamingIndex { get; set; }

        public bool IsFilled() => 
            Accounts != null && Proxies != null && Importer!=null && !string.IsNullOrEmpty(Group) && 
            !string.IsNullOrEmpty(NamingPrefix) && NamingIndex != null;
    }
}
