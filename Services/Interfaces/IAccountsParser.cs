using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public interface IAccountsParser
    {
        List<FacebookAccount> Parse();
    }
}
