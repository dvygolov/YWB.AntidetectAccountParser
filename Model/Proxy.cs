namespace YWB.AntidetectAccountParser.Model
{
    public class Proxy
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public static Proxy Parse(string input)
        {
            var split = input.Split(':');
            var p = new Proxy();
            p.Type = split[0];
            p.Address = split[1];
            p.Port = split[2];
            p.Login = split[3];
            p.Password = split[4];
            return p;
        }
    }
}
