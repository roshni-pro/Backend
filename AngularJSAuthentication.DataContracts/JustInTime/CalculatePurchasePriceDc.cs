using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.JustInTime
{
    public class CalculatePurchasePriceDc
    {
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
    }


    public class RiskCalculatePurchasePriceDc
    {
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }

    }

    public class CalculatePPAndUpdateItemMaster
    {
        public int ItemId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
    }
    public class DailyRateUpdateDc
    {
        public int ItemId { get; set; }
        public double Margin { get; set; }
        public double WholesalerMM { get; set; }
        public double TraderMM { get; set; }
        public double PurchasePrice { get; set; }
    }

}
