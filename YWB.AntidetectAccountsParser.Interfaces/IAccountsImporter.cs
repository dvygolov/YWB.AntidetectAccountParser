using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Interfaces
{
    public interface IAccountsImporter
    {
        Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IEnumerable<SocialAccount> accounts, FlowSettings fs);
    }
}
