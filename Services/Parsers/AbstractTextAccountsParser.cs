using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public abstract class AbstractTextAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        private const string FileName = "accounts.txt";
        public string Preprocess()
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

                if (sameStart.Length > 4) //then we are sure that it is not just random coincidence
                    lines = lines.ConvertAll(l => l.Substring(j - 2));
            }

            var input = string.Join("\r\n", lines);
            return input;
        }

        public IEnumerable<T> Parse()
        {
            var input = Preprocess();
            return Process(input);
        }

        protected abstract IEnumerable<T> Process(string input);
    }
}
