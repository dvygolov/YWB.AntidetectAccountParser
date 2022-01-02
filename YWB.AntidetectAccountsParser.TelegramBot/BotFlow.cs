using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.TelegramBot
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
