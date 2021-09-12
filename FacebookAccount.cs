using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YWB.AntidetectAccountParser.Helpers;

namespace YWB.AntidetectAccountParser
{
    public class FacebookAccount
    {
        private List<string> _logins = new List<string>();
        private List<string> _passwords = new List<string>();
        private List<string> _cookies = new List<string>();

        public FacebookAccount() { }
        public FacebookAccount(string name) => Name = name;

        public string Name { get; set; }
        public string Login
        {
            get
            {
                return _logins.Count > 0 ? _logins[0] : null;
            }

            set
            {
                if (_logins.Count > 0)
                    _logins[0] = value;
                else
                    _logins.Add(value);
            }
        }
        public string Password
        {
            get
            {
                return _passwords.Count > 0 ? _passwords[0] : null;
            }

            set
            {
                if (_passwords.Count > 0)
                    _passwords[0] = value;
                else
                    _passwords.Add(value);
            }
        }
        public string UserAgent { get; set; }
        public string Birthday { get; set; }
        public string EmailLogin { get; set; }
        public string EmailPassword { get; set; }
        public string TwoFactor { get; set; }
        public string BmLinks { get; set; }
        public string Token { get; set; }
        public string Cookies
        {
            get
            {
                return _cookies.Count > 0 ? _cookies[0] : null;
            }

            set
            {
                if (_cookies.Count > 0)
                    _cookies[0] = value;
                else
                    _cookies.Add(value);
            }
        }

        public List<string> AllCookies => _cookies;

        public bool AddCookies(string cookies)
        {
            var newCookies = JArray.Parse(cookies);
            for (int i = 0; i < _cookies.Count; i++)
            {
                var oldCookies = JArray.Parse(_cookies[i]);
                if (newCookies
                    .All((dynamic nc) => oldCookies
                        .Any((dynamic oc) => oc.name == nc.name && oc.value == oc.value)))
                    return false;
            }
            _cookies.Add(cookies);
            return true;
        }

        public bool AddLoginPassword(string login, string password)
        {
            for (int i = 0; i < _logins.Count; i++)
            {
                if (_logins[i] == login && _passwords[i] == password) return false;
            }
            _logins.Add(login);
            _passwords.Add(password);
            return true;
        }

        public static List<FacebookAccount> AutoParseFromZip(string folder)
        {
            var res = new List<FacebookAccount>();
            var files = Directory.GetFiles(folder, "*.zip");
            foreach (var f in files)
            {
                var fa = new FacebookAccount(Path.GetFileNameWithoutExtension(f));
                Console.WriteLine($"Обработка файла: {f}");
                using (var archive = ZipFile.OpenRead(f))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.ToLowerInvariant().Contains("password"))
                        {
                            Console.WriteLine($"Найден файл с паролями: {entry.FullName}");
                            using (var s = entry.Open())
                            {
                                var lines = Encoding.UTF8.GetString(s.ReadAllBytes()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                int index = -1;
                                while ((index = lines.FindIndex(index + 1, l => l.ToLowerInvariant().Contains("facebook"))) != -1)
                                {
                                    var login = lines[index + 1].Split(' ')[1];
                                    var password = lines[index + 2].Split(' ')[1];
                                    if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                                    {
                                        if (fa.AddLoginPassword(login, password))
                                            Console.WriteLine("Найден логин-пароль от фб!");
                                    }
                                }
                            }
                        }

                        if (entry.FullName.ToLowerInvariant().Contains("cookie") && entry.Length > 0)
                        {
                            Console.WriteLine($"Найден файл с cookies: {entry.FullName}");
                            using (var s = entry.Open())
                            {
                                var text = Encoding.UTF8.GetString(s.ReadAllBytes());
                                string cookies = string.Empty;
                                if (!text.Trim().StartsWith('['))
                                {
                                    cookies = CookieHelper.NetscapeCookiesToJSON(text);
                                }
                                else
                                    cookies = text;
                                var fbCookies = CookieHelper.GetFacebookCookies(cookies);
                                if (!string.IsNullOrEmpty(fbCookies))
                                    if (fa.AddCookies(fbCookies))
                                        Console.WriteLine("Найдены куки фб!");
                            }
                        }
                    }
                }
                if (fa.AllCookies.Any(c=>CookieHelper.HasCUserCookie(c))|| (fa.Login != null && fa.Password != null))
                    res.Add(fa);
                else
                {
                    var invalid = Path.Combine(folder, "Invalid");
                    if (!Directory.Exists(invalid)) Directory.CreateDirectory(invalid);
                    File.Move(f, Path.Combine(invalid, Path.GetFileName(f)));
                }
            }
            return res;
        }
        public static List<FacebookAccount> AutoParse(string input)
        {
            var re = new Regex(@"^(?<Login>[^\:;\|\s]+)[:;\|\s](?<Password>[^\:;\|\s]+)[:;\|\s]", RegexOptions.Multiline);
            var matches = re.Matches(input);
            Console.WriteLine($"Found {matches.Count} logins/passwords!");

            List<FacebookAccount> lst = matches.Select(m => new FacebookAccount()
            {
                Login = m.Groups["Login"].Value,
                Password = m.Groups["Password"].Value
            }).ToList();

            re = new Regex(@"(?<Token>EAABsb[^\s:;\|]+)", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find account tokens!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found tokens count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found account access tokens!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Token = matches[i].Groups["Token"].Value;
                }
            }

            re = new Regex(@"[:;\|\s](?<Email>[a-zA-Z0-9]+@[^\:;\|\s]+)[:;\|\s](?<EmailPassword>[^\:;\|\s]+)[:;\|\s]", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find emails and passwords.");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found emails count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found emails with passwords!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].EmailLogin = matches[i].Groups["Email"].Value;
                    lst[i].EmailPassword = matches[i].Groups["EmailPassword"].Value;
                }
            }

            re = new Regex(@"[\:;\|](?<Cookies>\[\s*\{.*?\}\s*\]\s*)($|[\:;\|])", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                re = new Regex(@"[:;\|](?<Cookies>W[A-Za-z0-9+/=]{300,})", RegexOptions.Multiline);
                matches = re.Matches(input);
            }
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find cookies!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found cookies count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found cookies!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Cookies = matches[i].Groups["Cookies"].Value;
                }
            }

            re = new Regex(@"(?<Birthday>\d{1,2}\s[а-я]+\s\d{4})", RegexOptions.Multiline);
            matches = re.Matches(input);
            if (matches.Count == 0)
            {
                re = new Regex(@"(?<Birthday>\d{1,2}[\./\-]\d{1,2}[\./\-][12]\d{3})", RegexOptions.Multiline);
                matches = re.Matches(input);
            }
            if (matches.Count == 0)
            {
                Console.WriteLine("Didn't find birthdays!");
            }
            else if (matches.Count != lst.Count)
            {
                Console.WriteLine("Found birthdays count does not match accounts count!");
            }
            else
            {
                Console.WriteLine("Found birthdays!");
                for (int i = 0; i < matches.Count; i++)
                {
                    lst[i].Birthday = matches[i].Groups["Birthday"].Value;
                }
            }

            return lst;
        }


        public override string ToString()
        {
            string str = string.Empty;
            for (int i = 0; i < _logins.Count; i++)
            {
                str += $"\nFacebook: {_logins[i]}:{_passwords[i]}";
            }
            if (!string.IsNullOrEmpty(Birthday))
                str += $"\nBirthday: {Birthday}";
            if (!string.IsNullOrEmpty(TwoFactor))
                str += $"\n2FA: {TwoFactor}";
            if (!string.IsNullOrEmpty(EmailPassword))
            {
                var eLogin = EmailLogin ?? Login;
                str += $"\nEmail: {eLogin}:{EmailPassword}";
            }
            if (!string.IsNullOrEmpty(Token))
            {
                str += $"\nToken: {Token}";
            }
            if (!string.IsNullOrEmpty(BmLinks))
            {
                str += $"\nBmLinks: {BmLinks}";
            }
            if (!string.IsNullOrEmpty(Cookies))
            {
                var cookies = $"\nCookies: {Regex.Replace(Cookies.Replace("\r\n", ""), "[ ]+", "")}";
                if ((str + cookies).Length > 5000)
                {
                    var fbCookies = CookieHelper.GetFacebookCookies(Cookies);
                    cookies = $"\nCookies: {fbCookies}";

                    if ((str + cookies).Length > 5000)
                        Console.WriteLine("Length is more then 5000 symbols, skipping cookies...");
                    else
                        str += cookies;
                }
                else
                    str += cookies;
            }
            return str;
        }
    }
}
