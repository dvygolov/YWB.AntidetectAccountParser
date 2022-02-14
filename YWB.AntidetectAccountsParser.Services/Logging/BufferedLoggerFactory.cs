using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YWB.AntidetectAccountsParser.Services.Logging
{
    internal class BufferedLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new BufferedLogger()
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
