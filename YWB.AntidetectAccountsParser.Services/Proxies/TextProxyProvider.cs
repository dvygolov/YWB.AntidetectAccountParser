using System;
using System.Collections.Generic;
using System.Linq;

namespace YWB.AntidetectAccountsParser.Services.Proxies
{
    public class TextProxyProvider : AbstractProxyProvider
    {
        private readonly string _input;

        public TextProxyProvider(string input)
        {
            _input = input;
        }
        public override List<string> GetLines() => _input.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
