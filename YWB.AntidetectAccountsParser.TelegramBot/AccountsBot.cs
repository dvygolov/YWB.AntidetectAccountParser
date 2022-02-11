using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors;

namespace YWB.AntidetectAccountsParser.TelegramBot
{
    internal class AccountsBot
    {
        private readonly string _apiKey;
        private readonly List<string> _allowedUsers;
        private readonly ILogger<AccountsBot> _logger;
        private readonly List<AbstractMessageProcessor> _processors;
        private TelegramBotClient _bot;
        private CancellationTokenSource _cts;
        private Dictionary<long, BotFlow> _flows = new Dictionary<long, BotFlow>();

        public AccountsBot(string apiKey, List<string> allowedUsers, IEnumerable<AbstractMessageProcessor> processors, ILogger<AccountsBot> logger)
        {
            _apiKey = apiKey;
            _allowedUsers = allowedUsers;
            _processors = processors.ToList();
            _logger = logger;
        }

        public void Start()
        {
            _bot = new TelegramBotClient(_apiKey);
            _cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            };
            _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: _cts.Token);
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
            if (fromId == null) return;
            if (!_allowedUsers.Contains(from))
            {
                await b.SendTextMessageAsync(chatId: fromId, text: "FUCK OFF!");
                await b.BanChatMemberAsync(chatId: fromId, userId: fromId.Value);
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
                    _logger.LogError(e, "An error occured:", update);
                    await b.SendTextMessageAsync(chatId: fromId, text: $"An ERROR occured: {e}");
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

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
