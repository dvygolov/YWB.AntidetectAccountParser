using Microsoft.Extensions.Logging;
using System.Reflection;

namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public class FileProxyProvider : AbstractProxyProvider
    {
        private const string FileName = "proxy.txt";

        public FileProxyProvider(ILoggerFactory lf) : base(lf) { }

        public override List<string> GetLines()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("There's no proxy.txt file!!!", fullPath);
            var split = File.ReadAllLines(fullPath).Where(l => !string.IsNullOrEmpty(l)).ToList();
            return split;
        }

        public override void SetSource(string source)
        {
            if (string.IsNullOrEmpty(source))
                source = FileName;
            base.SetSource(source);
        }
    }
}
