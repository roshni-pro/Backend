using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class ClearanceLiveItemDC
    {
        public long Id { get; set; }

        public int Qty { get; set; }

        public int AvailQty { get; set; }

        public string BatchCode { get; set; }

        public DateTime? MFGDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public int LiveQty { get; set; }
        public double UnitPrice { get; set; }

        public double? UpdatedUnitPrice { get; set; }

    }
}
