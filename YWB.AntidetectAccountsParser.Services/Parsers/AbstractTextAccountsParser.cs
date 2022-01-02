using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Logging;

namespace YWB.AntidetectAccountsParser.Services.Parsers
{
    public abstract class AbstractTextAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        protected readonly IAccountsLogger _logger;
        private List<string> _input;

        public AbstractTextAccountsParser(IAccountsLogger logger,List<string> input)
        {
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
            return Process(input);
        }

        protected abstract IEnumerable<T> Process(string input);
    }
}
