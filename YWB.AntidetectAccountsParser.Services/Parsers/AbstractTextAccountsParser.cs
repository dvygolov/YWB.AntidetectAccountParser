using Microsoft.Extensions.Logging;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Parsers
{
    public abstract class AbstractTextAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        private readonly IProxyProvider<T> _pp;
        protected readonly ILogger _logger;
        private List<string> _input;

        public AbstractTextAccountsParser(IProxyProvider<T> pp, ILogger logger,List<string> input)
        {
            _pp = pp;
            _logger = logger;
            _input = input;
        }

        protected string Preprocess()
        {
            if (_input.Count > 1)
            {
                //If all accounts lines start with the same shit - we must remove it!
                string sameStart;
                int j = 1;
                do
                {
                    sameStart = _input[0].Substring(0, j);
                    j++;
                }
                while (_input.All(l => l.StartsWith(sameStart)));

                if (sameStart.Length > 4) //then we are sure that it is not just random coincidence
                    _input = _input.ConvertAll(l => l.Substring(j - 2));
            }

            var input = string.Join("\r\n", _input);
            return input;
        }

        public IEnumerable<T> Parse()
        {
            var input = Preprocess();
            var accounts=Process(input);
            _pp.SetProxies(accounts);
            return accounts;
        }

        protected abstract IEnumerable<T> Process(string input);
    }
}
