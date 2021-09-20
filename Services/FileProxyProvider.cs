using System.Collections.Generic;
using System.IO;
using System.Linq;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public class FileProxyProvider : IProxyProvider
    {
        private const string FileName = "proxy.txt";
        public List<Proxy> Get()
        {
            return File.ReadAllLines(FileName).Select(l =>
            {
                var split = l.Split(':');
                return new Proxy()
                {
                    Type = split[0],
                    Address = split[1],
                    Port = split[2],
                    Login = split[3],
                    Password = split[4]
                };
            }).ToList();
        }
    }
}
