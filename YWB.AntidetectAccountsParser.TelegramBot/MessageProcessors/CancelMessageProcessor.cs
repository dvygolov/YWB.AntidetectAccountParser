using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class CancelMessageProcessor : AbstractMessageProcessor
    {
        public override bool Filter(BotFlow flow, Update update) =>
            update.Type == UpdateType.Message &&
            update.Message.Text == "Cancel";

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.Clear();
            await b.SendTextMessageAsync(
                chatId:m.Chat.Id, 
                text:"Operation canceled!",
                replyMarkup:new ReplyKeyboardRemove());
        }
    }
}
