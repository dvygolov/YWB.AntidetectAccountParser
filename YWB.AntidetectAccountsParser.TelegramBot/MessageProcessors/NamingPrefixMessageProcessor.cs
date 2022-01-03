using Telegram.Bot;
using Telegram.Bot.Types;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class NamingPrefixMessageProcessor : AbstractMessageProcessor
    {
        public NamingPrefixMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => string.IsNullOrEmpty(flow.NamingPrefix);

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.NamingPrefix = m.Text;
            await b.SendTextMessageAsync(m.Chat.Id, "Enter profile names starting index(for example,1):");
        }
    }
}
