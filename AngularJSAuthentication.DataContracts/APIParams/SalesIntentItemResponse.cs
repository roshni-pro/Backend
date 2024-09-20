using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class SalesIntentItemResponse
    {
        public string Itemname { get; set; }
        public string LogoUrl { get; set; }
        public double MRP { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int SystemForecastQty { get; set; }
        public int WarehouseId { get; set; }
        public List<int> PurchaseMOQList { get; set; }

    }

    public class SalesIntentItemRequest
    {
        public int WarehouseId { get; set; }        
        public double BuyerPrice { get; set; }
        public int AdditionalQty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int PeopleId { get; set; }
    }
}
