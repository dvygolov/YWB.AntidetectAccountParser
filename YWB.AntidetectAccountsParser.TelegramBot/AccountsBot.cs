using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountsParser.Services.Browsers;
using YWB.AntidetectAccountsParser.Services.Monitoring;
using YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors;

namespace YWB.AntidetectAccountsParser.TelegramBot
{
    internal class AccountsBot
    {
        ConcurrentDictionary<long, BotFlow> _flows = new ConcurrentDictionary<long, BotFlow>();
        private TelegramBotClient _bot;
        CancellationTokenSource _cts;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _configuration;
        private readonly ILogger _logger;
        private List<AbstractMessageProcessor> _processors;

        public AccountsBot(IServiceProvider sp)
        {
            _sp = sp;
            _configuration = sp.GetService<IConfigurationRoot>();
            _logger = sp.GetService<ILogger>();
        }

        public void Start()
        {
            _processors = new()
            {
                new UnknownFileMessageProcessor(_sp),
                new TxtFileMessageProcessor(_sp),
                new ProxyMessageProcessor(_sp),
                new OsMessageProcessor(_sp),
                new NamingPrefixMessageProcessor(_sp),
                new NamingIndexMessageProcessor(_sp)
            };
            _bot = new TelegramBotClient(_configuration.GetValue<string>("TelegramBotApiKey"));
            _cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: _cts.Token);
        }

        public void Stop()
        {
            _processors.Clear();
            _cts.Cancel();
            _bot = null;
        }

        async Task HandleUpdateAsync(ITelegramBotClient b, Update update, CancellationToken cancellationToken)
        {
            foreach (var p in _processors)
            {
                if (await p.ProcessAsync(_flows, update, b, cancellationToken)) break;
            }
            var m = update.Message;
            if (_flows.ContainsKey(m.From.Id))
            {
                var flow = _flows[m.From.Id];
                if (flow.IsFilled())
                {
                    await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
                    await b.SendTextMessageAsync(m.Chat.Id, "All done, HAPPY HACKING!");
                }
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
