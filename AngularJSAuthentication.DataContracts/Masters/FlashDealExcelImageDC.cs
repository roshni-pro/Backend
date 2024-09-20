using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class FlashDealExcelImageDC
    {
        public int SectionId { get; set; }
        public bool IsTile { get; set; }
        public bool HasBackgroundColor { get; set; }
        public string TileBackgroundColor { get; set; }
        public string BannerBackgroundColor { get; set; }
        public bool HasHeaderBackgroundColor { get; set; }
        public string TileHeaderBackgroundColor { get; set; }
        public bool HasBackgroundImage { get; set; }
        public string TileBackgroundImage { get; set; }
        public bool HasHeaderBackgroundImage { get; set; }
        public string TileHeaderBackgroundImage { get; set; }
        public string TileAreaHeaderBackgroundImage { get; set; }
        public string HeaderTextColor { get; set; }
        public string sectionBackgroundImage { get; set; }
    }
}
