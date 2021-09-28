using System;

namespace YWB.AntidetectAccountParser.Model
{
    public class Proxy
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public string UpdateLink { get; set; }

        public override bool Equals(object obj) => obj is Proxy proxy && Address == proxy.Address && Port == proxy.Port && Login == proxy.Login && Password == proxy.Password;
        public override int GetHashCode() => HashCode.Combine(Address, Port, Login, Password);
    }
}
