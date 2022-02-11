namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public class TextProxyProvider : AbstractProxyProvider
    {
        public override List<string> GetLines() => _source.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
