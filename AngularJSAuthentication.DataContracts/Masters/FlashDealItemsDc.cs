using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class FlashDealItemsDc
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ItemName { get; set; }
        public int Moq { get; set; }
        public int AvailableQty { get; set; }
        public int MaxQty { get; set; }
        public double FlashDealPrice { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long FlashDealRequestMasterId { get; set; }
        public bool IsRemove { get; set; }
        public string Status { get; set; }
        public double MRP { get; set; }
        public int ItemId { get; set; }
    }
    public class ResFlashDealItems
    {
        public List<FlashDealItemsDc> FlashDealItemsDcs { get; set; }
    }
    public class MinOrderQtyDc
    {
        public int MinOrderQty { get; set; }
        public int ItemMultiMRPId { get; set; }
    }

    public class FlashDealFilterDc
    {
        public int skip { get; set; }
        public int take { get; set; }
        public List<int> WarehouseIds { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string statusValue { get; set; }
    }
}
