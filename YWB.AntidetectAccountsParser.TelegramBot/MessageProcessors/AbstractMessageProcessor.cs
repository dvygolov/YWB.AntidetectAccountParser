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
            var m = update.Message;
            var flow = flows[m.From.Id];
            if (Filter(flow, update))
            {
                await PayloadAsync(flow, update, b, ct);
                return true;
            }
            return false;
        }
    }

    public class FilledFlowMessageProcessor : AbstractMessageProcessor
    {
        public FilledFlowMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow?.IsFilled() ?? false;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
            await b.SendTextMessageAsync(m.Chat.Id, "All done, HAPPY HACKING!");
        }
    }
}
