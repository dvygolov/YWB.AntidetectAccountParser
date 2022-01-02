using Microsoft.Extensions.DependencyInjection;
using YWB.AntidetectAccountsParser.Services.Proxies;

namespace YWB.AntidetectAccountsParser.Terminal
{
    internal class TerminalServiceProvider
    {
        public static ServiceProvider Configure()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton((sp) =>
            {
                var apf = new ConsoleAccountsParserFactory();
                return apf.CreateParser();
            });
            sc.AddSingleton<AbstractProxyProvider, FileProxyProvider>();
            return sc.BuildServiceProvider();
        }
    }
}
