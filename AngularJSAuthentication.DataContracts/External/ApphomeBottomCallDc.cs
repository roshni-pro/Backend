using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{

    public class ApphomeBottomCallDc
    {
        public int Id { set; get; }
        public int Type { set; get; }  //1. Sales Rating, 2. Delivery Rating 3.  OrderTrack   
        public string RelativeUrl { set; get; }
    }

}
