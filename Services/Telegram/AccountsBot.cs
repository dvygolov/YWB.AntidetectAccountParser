using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Browsers;
using YWB.AntidetectAccountParser.Services.Logging;
using YWB.AntidetectAccountParser.Services.Monitoring;
using YWB.AntidetectAccountParser.Services.Parsers;
using YWB.AntidetectAccountParser.Services.Proxies;

namespace YWB.AntidetectAccountParser.Services.Telegram
{
    internal class AccountsBot
    {
        ConcurrentDictionary<long, BotFlow> _flows = new ConcurrentDictionary<long, BotFlow>();
        private const string FileName = "bot.txt";
        private TelegramBotClient _bot;
        private const string Antidetect = "Antidetect Browser";
        private const string Monitoring = "Dolphin/FbTool";

        public AccountsBot()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (!System.IO.File.Exists(fullPath)) return;
            _bot = new TelegramBotClient(System.IO.File.ReadAllText(fullPath));
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);
        }

        async Task HandleUpdateAsync(ITelegramBotClient b, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message) return;
            var m = update.Message;
            switch (m.Type)
            {
                case MessageType.Document:
                    if (!m.Document.FileName.EndsWith(".txt"))
                    {
                        await b.SendTextMessageAsync(m.Chat.Id, "Unknown file type!");
                        break;
                    }

                    await b.SendTextMessageAsync(m.Chat.Id, "Received file with accounts! Parsing...");
                    var ms = new MemoryStream();
                    await _bot.GetInfoAndDownloadFileAsync(m.Document.FileId, ms);
                    var content = Encoding.UTF8.GetString(ms.ToArray());
                    var logger = new BufferAccountsLogger();
                    var p = new FacebookTextAccountsParser(logger, content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList());
                    var accounts = p.Parse();
                    _flows.AddOrUpdate(m.From.Id, new BotFlow { Accounts = accounts }, (id, bf) => bf);
                    await b.SendTextMessageAsync(m.Chat.Id, logger.Flush());
                    await b.SendTextMessageAsync(m.Chat.Id, "Enter your proxy or proxies line by line\nFormat http(socks):192.168.0.1:6666:xxxx:yyyy");
                    break;
                case MessageType.Text:
                    {
                        if (!_flows.ContainsKey(m.From.Id))
                        {
                            await b.SendTextMessageAsync(m.Chat.Id, "Flow not found! Send your accounts file first!");
                            break;
                        }
                        var f = _flows[m.From.Id];
                        if (f.Proxies == null)
                        {
                            var pp = new TextProxyProvider(m.Text);
                            f.Proxies = pp.Get();
                            pp.SetProxies(f.Accounts);
                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { Antidetect, Monitoring } }) { ResizeKeyboard = true };
                            Message sentMessage = await b.SendTextMessageAsync(
                                chatId: m.Chat.Id,
                                text: "Choose, where to import your accounts:",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                            break;
                        }
                        if (f.Importer == null)
                        {
                            switch (m.Text)
                            {
                                case Antidetect:
                                    f.Importer = new IndigoApiService();
                                    break;
                                case Monitoring:
                                    f.Importer = new FbToolService();
                                    break;
                                default:
                                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { Antidetect, Monitoring } }) { ResizeKeyboard = true };
                                    Message sentMessage = await b.SendTextMessageAsync(
                                        chatId: m.Chat.Id,
                                        text: "Unknown answer, choose again:",
                                        replyMarkup: replyKeyboardMarkup,
                                        cancellationToken: cancellationToken);
                                    break;
                            }
                            if (f.Importer != null)
                            {
                                await b.SendTextMessageAsync(m.Chat.Id, "Enter your group name (for example, YWB):");
                            }
                            break;
                        }
                        if (f.Group == null)
                        {
                            f.Group = new Model.Accounts.AccountGroup() { Name = "new" }; //TODO:Redo!
                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { Antidetect, Monitoring } }) { ResizeKeyboard = true };
                            Message sentMessage = await b.SendTextMessageAsync(
                                chatId: m.Chat.Id,
                                text: "Choose your OS:",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                            break;
                        }
                        if (string.IsNullOrEmpty(f.Os))
                        {
                            f.Os = m.Text;
                            await b.SendTextMessageAsync(m.Chat.Id, "Enter profile names prefix (for examle, YWB_2212_NPPR70_):");
                            break;
                        }
                        if (string.IsNullOrEmpty(f.NamingPrefix))
                        {
                            f.NamingPrefix = m.Text;
                            await b.SendTextMessageAsync(m.Chat.Id, "Enter profile names starting index(for example,1):");
                            break;
                        }
                        if (f.NamingIndex == null)
                        {
                            f.NamingIndex = int.Parse(m.Text);
                            await b.SendTextMessageAsync(m.Chat.Id, "All data filled, starting import, PLEASE WAIT!");
                            break;
                        }
                        break;
                    }
            }

            if (_flows.ContainsKey(m.From.Id))
            {
                var flow = _flows[m.From.Id];
                if (flow.IsFilled())
                {
                    await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
                    await b.SendTextMessageAsync(m.Chat.Id, "All done, HAPPY HACKING!");
                }
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
