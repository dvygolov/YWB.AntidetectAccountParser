using System.Collections.Generic;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Proxies
{
    public interface IProxyProvider<in T> where T:SocialAccount
    {
        void SetProxies(IEnumerable<T> accounts);
    }
}
