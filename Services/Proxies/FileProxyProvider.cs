using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Proxies
{
    public class FileProxyProvider : IProxyProvider<SocialAccount>
    {
        private const string FileName = "proxy.txt";
        public List<Proxy> Get()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("There's no proxy.txt file!!!", fullPath);
            var split = File.ReadAllLines(fullPath).Where(l => !string.IsNullOrEmpty(l));

            var proxies = split.Select(l =>
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

    }
}
