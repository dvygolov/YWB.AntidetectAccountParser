namespace YWB.AntidetectAccountParser.Services.Telegram
{
    public class FlowSettings
    {
        public string Os { get; set; }
        public string Group { get; set; }
        public string NamingPrefix { get; set; }
        public int? NamingIndex { get; set; }
        public virtual bool IsFilled() =>
            !string.IsNullOrEmpty(Os) && !string.IsNullOrEmpty(Group) 
            && !string.IsNullOrEmpty(NamingPrefix) && NamingIndex != null;
    }
}