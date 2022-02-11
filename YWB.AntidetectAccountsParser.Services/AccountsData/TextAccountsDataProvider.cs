using YWB.AntidetectAccountsParser.Interfaces;

namespace YWB.AntidetectAccountsParser.Services.AccountsData
{
    public class TextAccountsDataProvider : IAccountsDataProvider
    {
        private readonly string _input;

        public TextAccountsDataProvider(string input)
        {
            _input = input;
        }
        public List<string> GetData()=> _input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
