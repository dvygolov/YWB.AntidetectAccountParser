using Microsoft.Extensions.Logging;

namespace YWB.AntidetectAccountsParser.Services.Logging
{
    public class BufferedLogger : ILogger
    {
        record Entry(LogLevel _logLevel, EventId _eventId, string _message);
        LogLevel _minLogLevel;
        List<Entry> _buffer;
        public BufferedLogger(LogLevel minLogLevel)
        {
            _minLogLevel = minLogLevel;
            _buffer = new List<Entry>();
        }
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLogLevel;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var str = formatter(state, exception);
                _buffer.Add(new Entry(logLevel,eventId,str));
            }
        }
        public void CopyToLogger(ILogger logger)
        {
            foreach (var entry in _buffer)
            {
                logger.Log(entry._logLevel, entry._eventId, entry._message);
            }
            _buffer.Clear();
        }
    }
}
