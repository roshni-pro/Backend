using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class BeatEditDC
    {
        public long BeatEditId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsAnytime { get; set; }
        public int FromDate { get; set; }
        public int ToDate { get; set; }
    }
}
