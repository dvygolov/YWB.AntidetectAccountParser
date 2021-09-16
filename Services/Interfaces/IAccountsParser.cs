using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Services
{
    public interface IAccountsParser
    {
        List<FacebookAccount> Parse();
    }
}
