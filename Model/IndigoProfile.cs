using System;

namespace YWB.IndigoAccountParser.Model
{
    public class IndigoProfile
    {
        public string Sid { get; set; }
        public string Uuid { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public int BrowserType { get; set; }
        public int BrowserTypeVersion { get; set; }
        public DateTime Updated { get; set; }
    }

}
