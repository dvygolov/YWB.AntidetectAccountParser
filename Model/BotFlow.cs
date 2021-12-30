using System.Collections.Generic;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Telegram
{
    public class BotFlow : FlowSettings
    {
        public IEnumerable<SocialAccount> Accounts { get; set; }
        public List<Proxy> Proxies { get; set; }
        public IAccountsImporter Importer { get; set; }

        public override bool IsFilled() =>
            Accounts != null && Proxies != null && Importer != null && base.IsFilled();
    }
}
