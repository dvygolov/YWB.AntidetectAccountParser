using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class GroupMessageProcessor : AbstractMessageProcessor
    {
        public GroupMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow.Group == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.Group = new Model.Accounts.AccountGroup() { Name = "new" }; //TODO:Redo!
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { 
                new KeyboardButton[] { /*Antidetect, Monitoring */} }) { ResizeKeyboard = true };
            //TODO:redo
            Message sentMessage = await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "Choose your OS:",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: ct);
        }
    }
}
