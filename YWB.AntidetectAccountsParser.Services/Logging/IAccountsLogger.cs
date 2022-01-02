using System;

namespace YWB.AntidetectAccountsParser.Services.Logging
{
    public interface IAccountsLogger
    {
        void Log(string message);
    }

    public class ConsoleAccountsLogger : IAccountsLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
    public class BufferAccountsLogger : IAccountsLogger
    {
        public string Message { get; set; }
        public void Log(string message)
        {
            Message += message+Environment.NewLine;
        }
        public string Flush()
        {
            var buffer = Message;
            Message = string.Empty;
            return buffer;
        }
    }
}
