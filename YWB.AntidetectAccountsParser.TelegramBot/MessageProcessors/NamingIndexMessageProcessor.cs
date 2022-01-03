using Telegram.Bot;
using Telegram.Bot.Types;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class NamingIndexMessageProcessor : AbstractMessageProcessor
    {
        public NamingIndexMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow.NamingIndex == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.NamingIndex = int.Parse(m.Text);
            await b.SendTextMessageAsync(m.Chat.Id, "All data filled, starting import, PLEASE WAIT!");
        }
    }
}
