using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Parsers
{
    public abstract class AbstractTextAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        private readonly IProxyProvider<T> _pp;
        private readonly IAccountsDataProvider _adp;

        public AbstractTextAccountsParser(IProxyProvider<T> pp, IAccountsDataProvider adp)
        {
            _pp = pp;
            _adp = adp;
        }

        protected string Preprocess(List<string> input)
        {
            if (input.Count > 1)
            {
                //If all accounts lines start with the same shit - we must remove it!
                string sameStart;
                int j = 1;
                do
                {
                    sameStart = input[0].Substring(0, j);
                    j++;
                }
                while (input.All(l => l.StartsWith(sameStart)));

                if (sameStart.Length > 4) //then we are sure that it is not just random coincidence
                    input = input.ConvertAll(l => l.Substring(j - 2));
            }

            var strInput = string.Join("\r\n", input);
            return strInput;
        }

        public IEnumerable<T> Parse()
        {
            var input = _adp.GetData();
            var strInput = Preprocess(input);
            var accounts=Process(strInput);
            _pp.SetProxies(accounts);
            return accounts;
        }

        protected abstract IEnumerable<T> Process(string input);
    }
}
