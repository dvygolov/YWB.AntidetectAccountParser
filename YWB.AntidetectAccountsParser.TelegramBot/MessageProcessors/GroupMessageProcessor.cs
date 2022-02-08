using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class GroupMessageProcessor : AbstractMessageProcessor
    {
        public GroupMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => 
            update.Type==UpdateType.CallbackQuery&& 
            !string.IsNullOrEmpty(flow.Os)&&
            flow.Group == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var fromId = update.CallbackQuery.From.Id;
            var groups = await flow.Importer.GetExistingGroupsAsync();
            var selected = groups.FirstOrDefault(g => g.Name == update.CallbackQuery.Data);
            if (selected == null)
            {
                await b.SendTextMessageAsync(fromId, "Invalid Group Name! Choose again!");
                return;
            }
            flow.Group = selected;
            await b.SendTextMessageAsync(fromId, "Enter profile names prefix (for example, YWB_2212_NPPR70_) :");
        }
    }
}
