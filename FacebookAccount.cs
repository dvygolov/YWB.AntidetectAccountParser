using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<string> Logins { get => _logins; set => _logins = value; }
        public List<string> Passwords { get => _passwords; set => _passwords = value; }

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

        public string ToString(bool toHtml = false, bool withCookies = true)
        {
            var sd = toHtml ? "<p>" : "\n"; //start delimiter
            var ed = toHtml ? "</p>" : string.Empty; //end delimiter
            string str = string.Empty;
            for (int i = 0; i < _logins.Count; i++)
            {
                str += $"{sd}Facebook: {_logins[i]}:{_passwords[i]}{ed}";
            }
            if (!string.IsNullOrEmpty(Birthday))
                str += $"{sd}Birthday: {Birthday}{ed}";
            if (!string.IsNullOrEmpty(TwoFactor))
                str += $"{sd}2FA: {TwoFactor}{ed}";
            if (!string.IsNullOrEmpty(EmailPassword))
            {
                var eLogin = EmailLogin ?? Login;
                str += $"{sd}Email: {eLogin}:{EmailPassword}{ed}";
            }
            if (!string.IsNullOrEmpty(Token))
            {
                str += $"{sd}Token: {Token} {ed}";
            }
            if (!string.IsNullOrEmpty(BmLinks))
            {
                str += $"{sd}BmLinks: {BmLinks} {ed}";
            }
            if (withCookies)
            {
                if (!string.IsNullOrEmpty(Cookies))
                {
                    var cookies = $"{sd}Cookies: {Regex.Replace(Cookies.Replace("\r\n", ""), "[ ]+", "")}{ed}";
                    if ((str + cookies).Length > 5000)
                    {
                        var fbCookies = CookieHelper.GetFacebookCookies(Cookies);
                        cookies = $"{sd}Cookies: {fbCookies}{ed}";

                        if ((str + cookies).Length > 5000)
                            Console.WriteLine("Length is more then 5000 symbols, skipping cookies...");
                        else
                            str += cookies;
                    }
                    else
                        str += cookies;
                }
            }
            return str;
        }
    }
}
