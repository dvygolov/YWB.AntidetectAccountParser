using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Model
{
    public class FlowSettings
    {
        public string Os { get; set; }
        public AccountGroup Group { get; set; }
        public string NamingPrefix { get; set; }
        public int? NamingIndex { get; set; }
        public virtual bool IsFilled() =>
            !string.IsNullOrEmpty(Os) && Group!=null 
            && !string.IsNullOrEmpty(NamingPrefix) && NamingIndex != null;
    }
}