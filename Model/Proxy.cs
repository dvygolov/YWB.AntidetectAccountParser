using System;
using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Model
{
    public class Proxy
    {
        public string Id { get; set; }
        private List<string> _allowedTypes = new List<string> { "http", "socks", "socks5" };
        private string _type;

        public string Address { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Type
        {
            get => _type;
            set 
            {
                var t = value.TrimEnd(':');
                if (!_allowedTypes.Contains(t))
                    throw new Exception($"{t} is not a valid Proxy Type! Check your proxy.txt file.");
                _type = t;
            }
        }
        public string UpdateLink { get; set; }

        public override bool Equals(object obj) => obj is Proxy proxy && Address == proxy.Address && Port == proxy.Port && Login == proxy.Login && Password == proxy.Password;
        public override int GetHashCode() => HashCode.Combine(Address, Port, Login, Password);
        public override string ToString() => $"{Type}:{Address}:{Port}:{Login}:{Password}";
    }
}
