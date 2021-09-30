using System;
using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Model
{
    public class Proxy
    {
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
                if (!_allowedTypes.Contains(value))
                    throw new Exception($"{value} is not a valid Proxy Type! Check your proxy.txt file.");
                _type = value;
            }
        }
        public string UpdateLink { get; set; }
    }
}
