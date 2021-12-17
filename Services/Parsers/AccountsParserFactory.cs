using System;
using System.Collections.Generic;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public class AccountsParserFactory
    {
        enum AccountTypes { Google, Facebook };
        public IAccountsParser<SocialAccount> CreateParser()
        {
            Console.WriteLine("Which type of account do you want to parse?");
            var aType = SelectHelper.Select(new[] { AccountTypes.Google, AccountTypes.Facebook });
            IAccountsParser<SocialAccount> parser = null;
            switch (aType)
            {
                case AccountTypes.Google:
                    parser = new GoogleArchivesAccountsParser();
                    break;
                case AccountTypes.Facebook:
                    Console.WriteLine("What do you want to parse?");
                    var parsers = new Dictionary<string, Func<IAccountsParser<FacebookAccount>>> {
                        {"Accounts from text file",()=>new FacebookTextAccountsParser() },
                        {"Accounts from ZIP/RAR files or Folders",()=>new FacebookArchivesAccountsParser() }
                    };
                    parser = SelectHelper.Select(parsers, a => a.Key).Value();
                    break;
            }
            return parser;
        }
    }
}
