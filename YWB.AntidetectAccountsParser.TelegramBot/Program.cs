using Topshelf;
using YWB.AntidetectAccountsParser.TelegramBot;

var rc = HostFactory.Run(x =>
{
    x.Service<BotServiceStarter>(s =>
    {
        s.ConstructUsing(_ => new BotServiceStarter());
        s.WhenStarted(host => host.OnStart());
        s.WhenStopped(host => host.OnStop());
    });
    x.RunAsLocalSystem();
    x.StartAutomatically();
    x.EnableServiceRecovery(src => src.RestartService(5));

    x.SetDescription("Antidetect Accounts Parser Service");
    x.SetDisplayName("YWB.AAP.Telegram.Service");
    x.SetServiceName("YWB.AAP.Telegram.Service");
});
var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
Environment.ExitCode = exitCode;
