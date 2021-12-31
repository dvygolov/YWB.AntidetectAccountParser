using System.Collections.Generic;
using YWB.AntidetectAccountParser.Model.Accounts;
using YWB.AntidetectAccountParser.Services;

namespace YWB.AntidetectAccountParser.Model
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
