using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YWB.AntidetectAccountsParser.TelegramBot
{
    internal class BotServiceStarter
    {
        private AccountsBot _bot;
        public void OnStart()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot configuration = builder.Build();
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddConsole()
                    .AddFile("Logs\\AAP.Telegram.log", LogLevel.Trace));
            ILogger logger = loggerFactory.CreateLogger<AccountsBot>();
            _bot = new AccountsBot(configuration, logger);
            _bot.Start();
        }

        public void OnStop()
        {
            _bot.Stop();
        }
    }
}
