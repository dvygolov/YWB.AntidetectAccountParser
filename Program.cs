using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Browsers;
using YWB.AntidetectAccountParser.Services.Interfaces;
using YWB.AntidetectAccountParser.Services.Monitoring;
using YWB.AntidetectAccountParser.Services.Parsers;
using YWB.AntidetectAccountParser.Services.Proxies;

namespace YWB.AntidetectAccountParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Antidetect Accounts Parser v4.0b Yellow Web (https://yellowweb.top)");
            Console.WriteLine("If you like this software, please, donate!");
            Console.WriteLine("WebMoney: Z182653170916");
            Console.WriteLine("Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv");
            Console.WriteLine("Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B");
            await Task.Delay(5000);
            Console.WriteLine();

            Console.WriteLine("What do you want to parse?");
            var parsers = new Dictionary<string, Func<IAccountsParser>> {
                {"Accounts from text file",()=>new TextAccountsParser() },
                {"Accounts from ZIP/RAR files",()=>new ArchiveAccountsParser() }
            };
            var selectedParser = SelectHelper.Select(parsers, a => a.Key).Value();
            var accounts = selectedParser.Parse();

            var proxyProvider = new FileProxyProvider();
            proxyProvider.SetProxies(accounts);

            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1. Create Profiles in an Antidetect Browser");
            Console.WriteLine("2. Import accounts to FbTool/Dolphin");
            var answer = YesNoSelector.GetMenuAnswer(2);

            if (answer == 1)
            {
                Console.WriteLine("Choose your antidetect browser:");
                var browsers = new Dictionary<string, Func<AbstractAntidetectApiService>>
                {
                    {"Indigo",()=> new IndigoApiService() },
                    {"Dolphin Anty",()=>new DolphinAntyApiService() },
                    {"AdsPower",()=>new AdsPowerApiService() }
                };
                var selectedBrowser = SelectHelper.Select(browsers, b => b.Key).Value();

                await selectedBrowser.ImportAccountsAsync(accounts);

                if (accounts?.All(a => !string.IsNullOrEmpty(a.Token)) ?? false)
                {
                    var add = YesNoSelector.ReadAnswerEqualsYes("All accounts have access tokens! Do you wand to add them to Dolphin/FbTool?");
                    if (add)
                    {
                        await ImportToMonitoringService(accounts);
                    }
                }
            }
            else if (answer == 2)
            {
                if (accounts?.All(a => !string.IsNullOrEmpty(a.Token)) ?? false)
                {
                    await ImportToMonitoringService(accounts);
                }
            }


            Console.WriteLine("All done! Press any key to exit... and don't forget to donate ;-)");
            Console.ReadKey();
        }

        private static async Task ImportToMonitoringService(List<FacebookAccount> accounts)
        {
            if (accounts.All(a=>string.IsNullOrEmpty(a.Name)))
            {
                Console.Write("Enter account name prefix:");
                var namePrefix = Console.ReadLine();
                for (int i = 0; i < accounts.Count; i++)
                {
                    accounts[i].Name = $"{namePrefix}{i + 1}";
                }
            }
            var monitoringServices = new Dictionary<string, Func<AbstractMonitoringService>> {
                            {"FbTool",()=>new FbToolService() },
                            {"Dolphin",()=>new DolphinService() }
                        };
            Console.WriteLine("Choose your service:");
            var monitoringService = SelectHelper.Select(monitoringServices, ms => ms.Key).Value();
            await monitoringService.AddAccountsAsync(accounts);
            Console.WriteLine("All accounts added to FbTool/Dolphin.");

        }
    }
}
