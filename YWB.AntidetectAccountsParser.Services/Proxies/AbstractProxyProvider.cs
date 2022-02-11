using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public abstract class AbstractProxyProvider : IProxyProvider<SocialAccount>
    {
        protected string _source;
        public List<Proxy> Get()
        {
            var lines = GetLines(_source);
            var proxies = lines.Select(l =>
             {
                 var split = l.Split(':');
                 return new Proxy()
                 {
                     Type = split[0].Trim(),
                     Address = split[1].Trim(),
                     Port = split[2].Trim(),
                     Login = split[3].Trim(),
                     Password = split[4].Trim(),
                     UpdateLink = split.Length == 6 ? split[5].Trim() : string.Empty
                 };
             }).ToList();
            Console.WriteLine($"Found {proxies.Count} proxies!");
            return proxies;
        }

        public void SetProxies(IEnumerable<SocialAccount> accounts)
        {
            var proxies = Get();
            int i = 0;
            foreach (var acc in accounts)
            {
                var proxyIndex = i < proxies.Count - 1 ? i : i % proxies.Count;
                acc.Proxy = proxies[proxyIndex];
                i++;
            }
        }

        public abstract List<string> GetLines();

        public virtual void SetSource(string source)
        {
            _source = source;
        }
    }
}
