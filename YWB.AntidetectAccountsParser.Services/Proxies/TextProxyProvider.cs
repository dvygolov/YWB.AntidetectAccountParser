using Microsoft.Extensions.Logging;

namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public class TextProxyProvider : AbstractProxyProvider
    {
        public TextProxyProvider(ILoggerFactory lf) : base(lf) { }

        public override List<string> GetLines() => _source.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
