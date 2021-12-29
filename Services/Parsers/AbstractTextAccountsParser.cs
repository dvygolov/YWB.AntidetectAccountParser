using System;
using System.Collections.Generic;
using System.Linq;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public abstract class AbstractTextAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        private readonly Func<List<string>> _get;
        public AbstractTextAccountsParser(Func<List<string>> get)
        {
            _get = get;
        }
        public string Preprocess()
        {
            var lines = _get();

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
                    lines = lines.ToList().ConvertAll(l => l.Substring(j - 2));
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
