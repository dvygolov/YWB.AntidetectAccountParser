using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Model
{
    public class FlowSettings
    {
        public string Os { get; set; }
        public AccountGroup Group { get; set; }
        public string NamingPrefix { get; set; }
        public int? NamingIndex { get; set; }
        public virtual bool IsFilled() =>
            !string.IsNullOrEmpty(Os) && !string.IsNullOrEmpty(NamingPrefix) && NamingIndex != null;
        public virtual bool IsEmpty() =>
            string.IsNullOrEmpty(Os) && string.IsNullOrEmpty(NamingPrefix) && NamingIndex == null;
        public virtual void Clear()
        {
            Os = null;
            NamingPrefix = null;
            NamingIndex = null;
        }
    }
}