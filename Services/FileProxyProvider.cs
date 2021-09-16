using System.IO;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public class FileProxyProvider : IProxyProvider
    {
        private const string FileName = "proxy.txt";
        public Proxy Get()
        {
            var split = File.ReadAllText(FileName).Split(':');
            return new Proxy()
            {
                Type = split[0],
                Address = split[1],
                Port = split[2],
                Login = split[3],
                Password = split[4]
            };
        }
    }
}
