using Microsoft.Extensions.DependencyInjection;
using System;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Parsers;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Terminal
{
    public class ConsoleAccountsParserFactory
    {
        public AccountTypes AccountType { get; private set; }
        public IAccountsParser<SocialAccount> CreateParser()
        {
            Console.WriteLine("Which type of account do you want to parse?");
            AccountType= SelectHelper.Select(new[] { AccountTypes.Google, AccountTypes.Facebook });
            IAccountsParser<SocialAccount> parser = null;
            switch (AccountType)
            {
                case AccountTypes.Google:
                    //TODO:remake
                    //parser=_sp.GetService<GoogleArchivesAccountsParser>();
                    break;
                case AccountTypes.Facebook:
                    Console.WriteLine("What do you want to parse?");
                    //TODO:remake
                    /*var parsers = new Dictionary<string, Func<IAccountsParser<FacebookAccount>>> {
                        {"Accounts from text file", ()=>
                            new FacebookTextAccountsParser(new ConsoleAccountsLogger(), File.ReadAllLines(
                                 Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "accounts.txt")).ToList()) 
                        },
                        {"Accounts from ZIP/RAR files or Folders",()=>new FacebookArchivesAccountsParser() }
                    };
                    parser = SelectHelper.Select(parsers, a => a.Key).Value();*/
                    break;
            }
            return parser;
        }
    }
}
