namespace YWB.AntidetectAccountsParser.Services.Logging
{
    public class BufferAccountsLogger
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
