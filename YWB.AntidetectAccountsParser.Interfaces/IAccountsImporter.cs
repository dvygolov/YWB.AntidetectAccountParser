using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Interfaces
{
    public interface IAccountsImporter
    {
        Task<List<AccountGroup>> GetExistingGroupsAsync();
        Task<AccountGroup> AddNewGroupAsync(string groupName);
        Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IEnumerable<SocialAccount> accounts, FlowSettings fs);
    }
}
