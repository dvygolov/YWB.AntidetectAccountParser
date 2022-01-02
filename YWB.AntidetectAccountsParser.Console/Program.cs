using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Services.Playwright;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Browsers;
using YWB.AntidetectAccountsParser.Services.Monitoring;
using YWB.AntidetectAccountsParser.Services.Parsers;
using YWB.AntidetectAccountsParser.Services.Proxies;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Terminal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            await CopyrightHelper.ShowAsync(false);

            var sp = TerminalServiceProvider.Configure();
            var parser = sp.GetService<IAccountsParser<SocialAccount>>();
            var accounts = parser.Parse();
            if (accounts.Count() == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import(((");
                return;
            }

            int answer = 0;
            if (apf.AccountType == AccountTypes.Facebook)
            {
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("1. Create Profiles in an Antidetect Browser");
                Console.WriteLine("2. Import accounts to FbTool/Dolphin");
                answer = YesNoSelector.GetMenuAnswer(2);
            }
            else if (apf.AccountType == AccountTypes.Google)
                answer = 1;

            if (answer == 1)
            {
                Console.WriteLine("Choose your antidetect browser:");
                var browsers = new Dictionary<string, Func<AbstractAntidetectApiService>>
                {
                    {"AdsPower",()=>new AdsPowerApiService() },
                    {"Dolphin Anty",()=>new DolphinAntyApiService() },
                    {"Indigo",()=> new IndigoApiService() },
                    {"Octo",()=>new OctoApiService() }
                };
                var selectedBrowser = SelectHelper.Select(browsers, b => b.Key).Value();

                var cff = new ConsoleBrowserFlowFiller(selectedBrowser);
                FlowSettings flow = await cff.FillAsync();

                var profiles = await selectedBrowser.ImportAccountsAsync(accounts.ToList(), flow);

                if (accounts?.All(a => a is FacebookAccount && 
                    !string.IsNullOrEmpty((a as FacebookAccount).Token)) ?? false)
                {
                    var add = YesNoSelector.ReadAnswerEqualsYes(
                        "All accounts have access tokens! Do you wand to add them to Dolphin/FbTool?");
                    if (add)
                        await ImportToMonitoringService(accounts.Cast<FacebookAccount>().ToList());
                }
                else
                {
                    var ipws = new IndigoPlaywrightService();
                    //await ipws.GetTokensAsync(profiles);
                }
            }
            else if (answer == 2)
            {
                var fbAccounts = accounts.Cast<FacebookAccount>().ToList();
                if (fbAccounts.All(a => !string.IsNullOrEmpty(a.Token)))
                {
                    await ImportToMonitoringService(fbAccounts);
                }
                else if (fbAccounts.Any(a => !string.IsNullOrEmpty(a.Token)))
                {
                    var anwser = YesNoSelector.ReadAnswerEqualsYes("Not all accounts have Facebook Access Tokens! Import only those, that have tokens?");
                    if (anwser)
                        await ImportToMonitoringService(fbAccounts.Where(a => !string.IsNullOrEmpty(a.Token)).ToList());
                }
                else
                    Console.WriteLine("No accounts with access tokens found!((");
            }

            Console.WriteLine("All done! Press any key to exit... and don't forget to donate ;-)");
            Console.ReadKey();
        }

        private static async Task ImportToMonitoringService(List<FacebookAccount> accounts)
        {
            var monitoringServices = new Dictionary<string, Func<AbstractMonitoringService>> 
            {
                {"FbTool",()=>new FbToolService() },
                {"Dolphin",()=>new DolphinService() }
            };
            Console.WriteLine("Choose your service:");
            var monitoringService = SelectHelper.Select(monitoringServices, ms => ms.Key).Value();
            var cff = new ConsoleMonitoringFlowFiller(monitoringService);
            FlowSettings flow = await cff.FillAsync();
            await monitoringService.ImportAccountsAsync(accounts, flow);
            Console.WriteLine("All accounts added to FbTool/Dolphin.");
        }
    }
}
