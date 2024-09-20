using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class FlashDealUploadDc
    {
        public DateTime? OfferStartTime { get; set; }
        public DateTime? OfferEndTime { get; set; }
        public int FlashDealQtyAvaiable { get; set; }
        public int FlashDealMaxQtyPersonCanTake { get; set; }
        public double FlashDealSpecialPrice { get; set; }
        public string SellingSku { get; set; }
        public double MRP { get; set; }
        public int? userid { get; set; }
    }

    
}
