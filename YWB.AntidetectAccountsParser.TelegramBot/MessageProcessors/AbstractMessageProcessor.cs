using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public abstract class AbstractMessageProcessor
    {
        protected readonly IServiceProvider _sp;

        public AbstractMessageProcessor(IServiceProvider sp)
        {
            _sp = sp;
        }

        public abstract bool Filter(BotFlow flow, Update update);
        public abstract Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct);
        public async Task<bool> ProcessAsync(Dictionary<long, BotFlow> flows, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var fromId = update.Message?.From.Id??update.CallbackQuery?.From.Id;
            var flow = flows[fromId.Value];
            if (Filter(flow, update))
            {
                await PayloadAsync(flow, update, b, ct);
                return true;
            }
            return false;
        }
    }
}
