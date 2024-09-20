using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class SpillOverOrderDC
    {
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime OrderDate { get; set; }
        public int avgOrder { get; set; }
        public int RedCount { get; set; }
        public int TotalCount { get; set; }
        public double SpillOverinPercentage { get; set; }
    }
    public class SpillOverOrderFilter
    {
        //public List<int> CityIds { get; set; }
        public List<int> WarehouseIds { get; set; }
        public DateTime SelectedStartDate { get; set; }
        public DateTime SelectedEndDate { get; set; }
    }

    public class ExportDataNewDC
    {
        public string CityName { get; set; }
        public int? OrderId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public double? OrderAmount { get; set; }
        public int? MultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public DateTime? OrderEta { get; set; }
        public string Brand { get; set; }
        public DateTime? spoDate { get; set; }
    }
    public class SpillOverRedOrderCount
    {
        public int RedCount { get; set; }
        public int WarehouseId { get; set; }
    }
    public class SpillOverTotalOrderCount
    {
        public int TotalCount { get; set; }
        public int WarehouseId { get; set; }
     
    }

    public class SpillOverOrderDetailSP
    {
        public int OrderId { get; set; }
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CurrentStock { get; set; }
        public int TotalQtyRequired { get; set; }

    }
}
