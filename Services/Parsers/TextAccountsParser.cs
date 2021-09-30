using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public class TextAccountsParser : IAccountsParser
    {
        private const string FileName = "accounts.txt";
        public List<FacebookAccount> Parse()
        {
            var lines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName)).ToList();

            if (lines.Count > 1)
            {
                //If all accounts lines start with the same shit - we must remove it!
                string sameStart;
                int j = 1;
                do
                {
                    sameStart = lines[0].Substring(0, j);
                    j++;
                }
                while (lines.All(l => l.StartsWith(sameStart)));

                if (sameStart.Length>4) //then we are sure that it is not just random coincidence
                    lines = lines.ConvertAll(l => l.Substring(j - 2));
            }

            var input = string.Join("\r\n", lines);

            var re = new Regex(@"^(?<Login>[^\:;\|\s]+)[:;\|\s](?<Password>[^\:;\|\s]+)[:;\|\s]", RegexOptions.Multiline);
            var matches = re.Matches(input);
            Console.WriteLine($"Found {matches.Count} logins/passwords!");

            List<FacebookAccount> lst = matches.Select(m => new FacebookAccount()
            {
                Login = m.Groups["Login"].Value,
                Password = m.Groups["Password"].Value
            }).ToList();

            re = new Regex(@"(?<Token>EAABsb[^\s:;\|]+)", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find account tokens!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found tokens count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found account access tokens!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Token = matches[i].Groups["Token"].Value;
                }
            }

            re = new Regex(@"[:;\|\s](?<Email>[a-zA-Z0-9]+@[^\:;\|\s]+)[:;\|\s](?<EmailPassword>[^\:;\|\s]+)[:;\|\s]", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find emails and passwords.");
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

            re = new Regex(@"[\:;\|\s](?<Cookies>\[\s*\{.*?\}\s*\]\s*)($|[\:;\|\s])", RegexOptions.Multiline);
            matches = re.Matches(input);
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
                    lst[i].Cookies = matches[i].Groups["Cookies"].Value;
                }
            }

            re = new Regex(@"(?<Birthday>\d{1,2}\s[а-я]+\s\d{4})", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                re = new Regex(@"(?<Birthday>\d{1,2}[\./\-]\d{1,2}[\./\-][12]\d{3})", RegexOptions.Multiline);
                matches = re.Matches(input);
            }
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find birthdays!");
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

    }
}
