using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public interface IProxyProvider<in T> where T:SocialAccount
    {
        List<Proxy> Get();
        void SetProxies(IEnumerable<T> accounts);
    }
}
