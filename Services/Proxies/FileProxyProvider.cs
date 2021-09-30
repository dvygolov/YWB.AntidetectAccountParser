using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Proxies
{
    public class FileProxyProvider : IProxyProvider
    {
        private const string FileName = "proxy.txt";
        public List<Proxy> Get()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("There's no proxy.txt file!!!");
            return File.ReadAllLines(fullPath).Select(l =>
            {
                var split = l.Split(':');
                return new Proxy()
                {
                    Type = split[0],
                    Address = split[1],
                    Port = split[2],
                    Login = split[3],
                    Password = split[4],
                    UpdateLink=split.Length==6?split[5]:string.Empty
                };
            }).ToList();
        }
    }
}
