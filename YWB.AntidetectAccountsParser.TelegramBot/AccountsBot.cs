using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors;

namespace YWB.AntidetectAccountsParser.TelegramBot
{
    internal class AccountsBot
    {
        private TelegramBotClient _bot;
        CancellationTokenSource _cts;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _configuration;
        private readonly ILogger _logger;
        private List<AbstractMessageProcessor> _processors;
        Dictionary<long, BotFlow> _flows = new Dictionary<long, BotFlow>();


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
                new CancelMessageProcessor(_sp),
                new UnknownFileMessageProcessor(_sp),
                new TxtFileMessageProcessor(_sp),
                new UnknownFlowProcessor(_sp),
                new ProxyMessageProcessor(_sp),
                new ImporterMessageProcessor(_sp),
                new OsMessageProcessor(_sp),
                new GroupMessageProcessor(_sp),
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
            var from = update.Message?.From.Username ?? update.CallbackQuery?.From.Username;
            var fromId = update.Message?.From.Id ?? update.CallbackQuery?.From.Id;
            var users = _sp.GetService<List<string>>();
            if (!users.Contains(from))
            {
                await b.SendTextMessageAsync(chatId: fromId, text: "FUCK OFF!" );
                return;
            }

            if (!_flows.ContainsKey(fromId.Value))
                _flows.Add(fromId.Value, new BotFlow());

            foreach (var p in _processors)
            {
                try
                {
                    if (await p.ProcessAsync(_flows, update, b, cancellationToken)) break;
                }
                catch (Exception e)
                {
                    await b.SendTextMessageAsync(
                        chatId: fromId,
                        text: $"An ERROR occured: {e}" );
                    _flows.Remove(fromId.Value);
                    return;
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

            _logger?.LogError(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
