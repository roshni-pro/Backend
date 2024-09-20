using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CMSReportDatadc
    {
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public DateTime BOD { get; set; }
        public int opening { get; set; }
        public int ExchangeIn { get; set; }
        public int ExchangeOut { get; set; }
        public int Closing { get; set; }
        public decimal onlineCollectionAmount { get; set; }
        public int CashCollection { get; set; }
        public int Bank { get; set; }
        public string CashExchangeComments { get; set; } 
    }

    public class CMSDetailDC
    {
        public List<int> Warehouseids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

}
