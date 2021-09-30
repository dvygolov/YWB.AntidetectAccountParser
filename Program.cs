using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Services;
using YWB.AntidetectAccountParser.Services.Browsers;
using YWB.AntidetectAccountParser.Services.Interfaces;
using YWB.AntidetectAccountParser.Services.Parsers;

namespace YWB.AntidetectAccountParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Antidetect Accounts Parser v3.8 Yellow Web (https://yellowweb.top)");
            Console.WriteLine("If you like this software, please, donate!");
            Console.WriteLine("WebMoney: Z182653170916");
            Console.WriteLine("Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv");
            Console.WriteLine("Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B");
            await Task.Delay(5000);
            Console.WriteLine();

            var proxyProvider = new FileProxyProvider();
            Console.WriteLine("What do you want to parse?");
            var actions = new Dictionary<string, Func<IAccountsParser>> {
                {"Accounts from text file",()=>new TextAccountsParser() },
                {"Accounts from ZIP/RAR files",()=>new ArchiveAccountsParser() }
            };
            var selectedParser = SelectHelper.Select(actions, a => a.Key).Value();

            Console.WriteLine("Choose your antidetect browser:");
            var browsers = new Dictionary<string, Func<AbstractAntidetectApiService>>
            {
                {"Indigo",()=> new IndigoApiService(selectedParser,proxyProvider) },
                {"Dolphin Anty",()=>new DolphinApiService(selectedParser,proxyProvider) },
                {"AdsPower",()=>new AdsPowerApiService(selectedParser,proxyProvider) }
            };
            var selectedBrowser = SelectHelper.Select(browsers, b => b.Key).Value();

            await selectedBrowser.ImportAccountsAsync();


            Console.WriteLine("All done! Press any key to exit... and don't forget to donate ;-)");
            Console.ReadKey();
        }
    }
}
