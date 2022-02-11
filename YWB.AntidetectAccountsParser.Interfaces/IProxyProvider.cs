﻿using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Interfaces
{
    public interface IProxyProvider<in T> where T:SocialAccount
    {
        List<Proxy> Get();
        void SetSource(string source);
        void SetProxies(IEnumerable<T> accounts);
    }
}
