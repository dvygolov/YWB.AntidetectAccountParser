using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services
{
    public interface IAccountsImporter
    {
        Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IEnumerable<SocialAccount> accounts);
    }
}
