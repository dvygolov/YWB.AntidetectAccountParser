using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class FontsHelper
    {
        static List<string> _fonts = new List<string>() { "Arial", "Calibri", "Cambria", "Comic Sans MS", "Comic Sans MS Bold", "Consolas", "Constantia", "Corbel", "Courier New", "Caurier Regular", "Ebrima", "Fixedsys Regular", "Gabriola Regular", "Gadugi", "Georgia", "HoloLens MDL2 Assets Regular", "Impact Regular", "Leelawadee UI", "Lucida Console Regular", "Lucida Sans Unicode Regular", "Malgun Gothic", "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft Sans Serif Regular", "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei UI", "MingLiU_HKSCS-ExtB Regular", "Mongolia Baiti Regular", "MS Serif Regular", "MV Boli Regular", "Myanmar Text", "Nimarla UI", "Myanmar Tet", "NSimSun Regular", "Palatino Linotype", "PMingLiU-ExtB Regular", "Roman Regular", "Script Regular", "Segoe Print", "Segoe UI", "Segoe UI Emoji Regular", "Segoe UI Historic Regular", "Segoe UI Symbol Regular", "SimSun-ExtB Regular", "Sitka Banner", "Sitka Display", "Sitka Heading", "Sitka Subheading", "Sitka Text", "Small Fonts Regular", "Sylfaen Regular", "Symbol Regular", "System Bold", "Terminal", "Times New Roman", "Verdana", "Webdings Regular", "Wingdings Regular", "Yu Gothic", "Yu Gothic UI", "Arial Black", "Calibri Light", "Courier", "Franklin Gothic Medium", "Gabriola", "HoloLens MDL2 Assets", "Impact", "Leelawadee UI Semilight", "Lucida Console", "Lucida Sans Unicode", "MS Gothic", "MS PGothic", "MS Sans Serif", "MS UI Gothic", "MV Boli", "Malgun Gothic Semilight", "Marlett", "Microsoft Himalaya", "Microsoft JhengHei Light", "Microsoft JhengHei UI Light", "Microsoft New Tai Lue", "Microsoft PhagsPa", "Microsoft Sans Serif", "Microsoft YaHei Light", "Microsoft Yi Baiti", "MingLiU-ExtB", "MingLiU_HKSCS-ExtB", "Modern", "Mongolian Baiti", "NSimSun", "Nirmala UI Semilight", "PMingLiU-ExtB", "Roman", "Script", "Segoe MDL2 Assets", "Segoe UI Black", "Segoe UI Emoji", "Segoe UI Historic", "Segoe UI Semibold", "Segoe UI Semilight", "SimSun-ExtB", "Small Fonts", "System", "Webdings", "Wingdings", "Yu Gothic Light", "Yu Gothic Medium", "Yu Gothic UI Semibold" };

        public static List<string> GetRandomFonts(int count)
        {
            var tmpFonts = new List<string>();
            tmpFonts.AddRange(_fonts);
            while (tmpFonts.Count>count)
            {
                var toKill = tmpFonts.GetRandomEntryFromList();
                tmpFonts.Remove(toKill);
            }
            return tmpFonts;
        }
    }
}
