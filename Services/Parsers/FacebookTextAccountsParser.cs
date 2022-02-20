using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public class FacebookTextAccountsParser : AbstractTextAccountsParser<FacebookAccount>
    {
        private List<FacebookAccount> ProcessLoginsAndPasswords(string input)
        {
            var re = new Regex(@"^(?<Login>[^\:;\|\s]+)\s*[:;\|\s]\s*(?<Password>[^\:;\|\s]+)\s*[:;\|\s]", RegexOptions.Multiline);
            var matches = re.Matches(input);
            Console.WriteLine($"Found {matches.Count} logins/passwords!");

            List<FacebookAccount> lst = matches.Select(m => new FacebookAccount()
            {
                Login = m.Groups["Login"].Value,
                Password = m.Groups["Password"].Value
            }).ToList();
            return lst;
        }

        private List<FacebookAccount> ProcessTokens(string input,List<FacebookAccount> lst)
        {
            var re = new Regex(@"(?<Token>EAABsb[^\s:;\|]+)", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find access tokens!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found access tokens count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found access tokens!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Token = matches[i].Groups["Token"].Value;
                }
            }
            return lst;
        }

        private List<FacebookAccount> ProcessBMTokens(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"(?<Token>EAAG[^\s:;\|]+)", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find BM tokens!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found BM tokens count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found BM access tokens!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].BmToken = matches[i].Groups["Token"].Value;
                }
            }
            return lst;
        }

        private List<FacebookAccount> ProcessEmails(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"[:;\|\s](?<Email>[a-zA-Z0-9\._]+@[^\:;\|\s]+)[:;\|\s](?<EmailPassword>[^\:;\|\s]+)[:;\|\s]", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find emails and passwords.");
            }
            else if (matches.Count > lst.Count)
            {
                Console.WriteLine("Found duplicate emails, trying to remove...");
                var mList = matches.ToList();
                int i = 0;
                while (i + 1 < mList.Count)
                {
                    if (mList[i].Groups["Email"].Value == mList[i + 1].Groups["Email"].Value)
                    {
                        mList.RemoveAt(i);
                        continue;
                    }
                    i++;
                }
                if (mList.Count == lst.Count)
                {
                    Console.WriteLine("Found emails with passwords!");
                    for (int j = 0; j < mList.Count; j++)
                    {
                        lst[j].EmailLogin = mList[j].Groups["Email"].Value;
                        lst[j].EmailPassword = mList[j].Groups["EmailPassword"].Value;
                    }
                }
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found emails count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found emails with passwords!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].EmailLogin = matches[i].Groups["Email"].Value;
                    lst[i].EmailPassword = matches[i].Groups["EmailPassword"].Value;
                }
            }
            return lst;
        }

        private (List<FacebookAccount> accounts,string input) ProcessCookies(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"[\:;\|\s](?<Cookies>\[\s*\{.*?\}\s*\]\s*)($|[\:;\|\s])", RegexOptions.Multiline);
            var matches = re.Matches(input);
            var invalid = new List<int>();
            if (matches.Count == 0)
            {
                re = new Regex(@"[:;\|](?<Cookies>W[A-Za-z0-9+/=]{300,})", RegexOptions.Multiline);
                matches = re.Matches(input);
            }
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find cookies!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found cookies count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found cookies!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Cookies = CookieHelper.GetDomainCookies(matches[i].Groups["Cookies"].Value, lst[i].Domain);
                    var cUser = CookieHelper.GetCUserCookie(lst[i].AllCookies);
                    var ch = FbHeadersChecker.Check(cUser);
                    if (!ch) invalid.Add(i);
                }
            }

            if (invalid.Count > 0)
            {
                var split = input.Split(input, '\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                Console.WriteLine($"{invalid.Count} invalid accounts were found! Removing them...");
                for (int i = invalid.Count - 1; i >= 0; i--)
                {
                    split.RemoveAt(invalid[i]);
                    lst.RemoveAt(invalid[i]);
                }
                input = string.Join('\n', split);
            }
            return (lst, input);
        }

        private List<FacebookAccount> ProcessUserAgents(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"(?<UserAgent>Mozilla.*?Gecko\)\s+(\w+/[\d+\. ]+)+)", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find Useragents!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found useragents count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found UserAgents!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].UserAgent = matches[i].Groups["UserAgent"].Value;
                }
            }
            return lst;
        }

        private List<FacebookAccount> Process2FA(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"[:;\|\s](?<TwoFactor>[\dA-Z]{32})($|[\:;\|\s])", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find 2FA!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found 2FA count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found 2FA!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].TwoFactor = matches[i].Groups["TwoFactor"].Value;
                }
            }
            return lst;
        }

        private List<FacebookAccount> ProcessBirthdays(string input, List<FacebookAccount> lst)
        {
            var re = new Regex(@"(?<Birthday>\d{1,2}\s[а-я]+\s\d{4})", RegexOptions.Multiline);
            var matches = re.Matches(input);
            if (matches.Count == 0)
            {
                re = new Regex(@"(?<Birthday>\d{1,2}[\./\-]\d{1,2}[\./\-][12]\d{3})", RegexOptions.Multiline);
                matches = re.Matches(input);
            }
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find birthdays!");
            }
            else if (matches.Count > lst.Count)
            {
                Console.WriteLine("Found more birthdays than expected, trying to remove unnecessary...");
                var mList = matches.ToList();
                int i = 0;
                var now = DateTime.Now;
                while (i < mList.Count)
                {
                    var bDay = mList[i].Groups["Birthday"].Value;
                    var parsed = DateTime.TryParse(bDay, out DateTime dt);
                    if (parsed && dt.Year == now.Year)
                    {
                        mList.RemoveAt(i);
                        continue;
                    }
                    i++;
                }
                if (mList.Count == lst.Count)
                {
                    Console.WriteLine("Cleanup successfull, adding birthdays!");
                    for (int j = 0; j < mList.Count; j++)
                    {
                        lst[j].Birthday = mList[j].Groups["Birthday"].Value;
                    }
                }
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found birthdays count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found birthdays!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Birthday = matches[i].Groups["Birthday"].Value;
                }
            }
            return lst;
        }

        protected override IEnumerable<FacebookAccount> Process(string input)
        {
            var accounts=ProcessLoginsAndPasswords(input);
            (accounts,input) = ProcessCookies(input, accounts);
            accounts = ProcessTokens(input, accounts);
            accounts = ProcessBMTokens(input, accounts);
            accounts = ProcessEmails(input, accounts);
            accounts = ProcessUserAgents(input, accounts);
            accounts = Process2FA(input, accounts);
            accounts = ProcessBirthdays(input, accounts);
            return accounts;
        }
    }
}
