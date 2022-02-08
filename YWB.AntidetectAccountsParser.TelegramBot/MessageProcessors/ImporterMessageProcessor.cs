using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Services.Browsers;
using YWB.AntidetectAccountsParser.Services.Monitoring;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class ImporterMessageProcessor : AbstractMessageProcessor
    {
        public ImporterMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => 
            update.Type == UpdateType.CallbackQuery &&
            flow.Importer==null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var credentials = _sp.GetService<List<ServiceCredentials>>();
            var fromId = update.CallbackQuery.From.Id;
            var current = credentials.First(c => c.Name == update.CallbackQuery.Data).Credentials;
            flow.Importer = update.CallbackQuery.Data switch
            {
                "Indigo" => new IndigoApiService(current),
                "AdsPower" => new AdsPowerApiService(current),
                "DolphinAnty" => new DolphinAntyApiService(current),
                "Octo" => new OctoApiService(current),
                "FbTool" => new FbToolService(current),
                "Dolphin" => new DolphinService(current),
                _=>throw new Exception("Invalid importing service name!")
            };


            await b.SendTextMessageAsync(
                chatId:fromId, 
                text:"Choose your OS:");
        }
    }
}
