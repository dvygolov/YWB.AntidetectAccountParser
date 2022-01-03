using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        public async Task<bool> ProcessAsync(ConcurrentDictionary<long, BotFlow> flows, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            if (!flows.ContainsKey(m.From.Id))
            {
                await b.SendTextMessageAsync(m.Chat.Id, "Flow not found! Send your accounts file first!");
                return true;
            }
            var flow = flows[m.From.Id];
            if (Filter(flow, update))
            {
                await PayloadAsync(flow, update, b, ct);
                return true;
            }
            return false;
        }
    }

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
