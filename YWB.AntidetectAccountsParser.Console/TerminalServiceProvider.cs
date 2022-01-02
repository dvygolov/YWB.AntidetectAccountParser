using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using YWB.AntidetectAccountsParser.Services.Proxies;

namespace YWB.AntidetectAccountsParser.Terminal
{
    internal class TerminalServiceProvider
    {
        public static ServiceProvider Configure()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                            .AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot configuration = builder.Build();

            var sc = new ServiceCollection();
            sc.AddSingleton(configuration);
            sc.AddSingleton((sp) =>
            {
                var apf = new ConsoleAccountsParserFactory();
                return apf.CreateParser();
            });
            sc.AddSingleton<AbstractProxyProvider, FileProxyProvider>();
            sc.AddLogging(builder=> builder.AddConsole()
                    .AddFile("Logs\\AAP.Terminal.log", LogLevel.Trace));
            return sc.BuildServiceProvider();
        }
    }
}
