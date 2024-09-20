using AngularJSAuthentication.Model.Stocks.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{

    public enum StockEnum
    {
        [Description("PendingToRTD")]
        PendingToRTD = 1
    }
    public class StockConfigDc
    {
        public StockConfigMaster Master { get; set; }
        public List<StockConfigDetail> DetailList { get; set; }
    }
}
