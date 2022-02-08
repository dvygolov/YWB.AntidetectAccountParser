using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace YWB.AntidetectAccountsParser.Model.Accounts
{
    public class SocialAccount
    {
        public SocialAccount() { }
        public SocialAccount(string name)
        {
            Name = name;
        }

        protected List<string> _logins = new List<string>();
        protected List<string> _passwords = new List<string>();
        private List<string> _cookies = new List<string>();

        public string Name { get; set; }
        public string Domain { get; set; } = "google.com";
        public string UserAgent { get; set; }
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
        public Proxy Proxy { get; set; }

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

        public virtual string ToString(bool toHtml = false, bool withCookies = true)
        {
            var sd = toHtml ? "<p>" : "\n"; //start delimiter
            var ed = toHtml ? "</p>" : string.Empty; //end delimiter
            string str = string.Empty;
            for (int i = 0; i < _logins.Count; i++)
            {
                str += $"{sd}{Domain}: {_logins[i]}:{_passwords[i]}{ed}";
            }
            if (withCookies)
            {
                str = CookiesToString(str, sd, ed);
            }
            return str;
        }

        protected string CookiesToString(string str, string sd, string ed)
        {
            if (!string.IsNullOrEmpty(Cookies))
            {
                var cookies = $"{sd}Cookies: {Regex.Replace(Cookies.Replace("\r\n", ""), "[ ]+", "")}{ed}";
                if ((str + cookies).Length > 5000)
                    Console.WriteLine("Length is more then 5000 symbols, skipping cookies...");
                else
                    str += cookies;
            }
            return str;
        }
    }
}