﻿using System.Text;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Actions;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Actions
{
    public class PasswordAccountAction<T> : AccountAction<T> where T : SocialAccount
    {
        public PasswordAccountAction()
        {
            Condition = (fileName) => fileName.Contains("password");
            Action = (stream,sa) => ExtractLoginAndPassword(stream,sa);
            Message = "Found file with passwords: ";
        }

        private void ExtractLoginAndPassword(Stream s,T sa)
        {
            var needle = sa is FacebookAccount ? "facebook" : "google.com";

            var lines = Encoding.UTF8.GetString(s.ReadAllBytes()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int index = -1;
            while ((index = lines.FindIndex(index + 1, l => l.ToLowerInvariant().Contains(needle))) != -1)
            {
                if (index + 2 >= lines.Count) continue;
                var split = lines[index + 1].Split(' ');
                if (split.Length!=2) continue;
                var login = split[1];
                split = lines[index + 2].Split(' ');
                if (split.Length!=2) continue;
                var password = split[1];
                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                {
                    if (sa.AddLoginPassword(login, password))
                        Console.WriteLine("Found login/password!");
                    index += 2;
                }
            }
        }
    }
}
