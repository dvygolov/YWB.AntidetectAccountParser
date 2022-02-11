using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.TelegramBot
{
    public class BotFlow : FlowSettings
    {
        public IAccountsDataProvider AccountsDataProvider { get; set; }
        public IEnumerable<SocialAccount> Accounts { get; set; }
        public List<Proxy> Proxies { get; set; }
        public IAccountsImporter Importer { get; set; }

        public override bool IsFilled() =>
            AccountsDataProvider != null && Accounts != null && Proxies != null && Importer != null && base.IsFilled();
        public override bool IsEmpty() =>
            AccountsDataProvider == null && Accounts == null && Proxies == null && Importer == null && base.IsEmpty();

        public override void Clear()
        {
            AccountsDataProvider = null;
            Accounts = null;
            Proxies = null;
            Importer = null;
            base.Clear();
        }
    }
}
