using Telegram.Bot;
using Telegram.Bot.Types;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class OsMessageProcessor : AbstractMessageProcessor
    {
        public OsMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => string.IsNullOrEmpty(flow.Os);

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.Os = m.Text;
            await b.SendTextMessageAsync(m.Chat.Id, "Enter profile names prefix (for examle, YWB_2212_NPPR70_):");
        }
    }
}
