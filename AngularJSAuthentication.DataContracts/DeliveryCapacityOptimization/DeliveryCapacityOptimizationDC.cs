using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryCapacityOptimization
{

    public class OrderedAndDeliverdCountFilterDC
    {
        public int warehouseId { get; set; }
        public DateTime Fromdate { get; set; }
        public DateTime Todate { get; set; }
        public int Year { get; set; }
    }

    public class OrderedAndDeliverdCountDC
    {
      //  public int Warehouseid { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int OrderedCount { get; set; }
        public int DeliveredCount { get; set; }
        public double DeliveredPercent { get; set; }
        public int ThresholdCount { get; set; }
        public int cumCountPending { get; set; }
        public DateTime ETADelayDate { get; set; }
        public int OrderCapacity { get; set; }
        public int UpdateEta { get; set; }
        public int lastcapacity { get; set; }
    }
 
    public class DeliveryCapacity
    {
        public List<int> warehouseIds { get; set; }

        public List<string> Holidays { get; set; }
        public int DefaultCapacity { get; set; }
        public int year { get; set; }


    }
    public class SelectedList
    {
     
        public string Holiday { get; set; }
        public int DefaultCapacity { get; set; }
      


    }

    public class UpdateCapacity
    {
        public int warehouseId { get; set; }

        public DateTime Date { get; set; }
        public int UpdateThresholdCapacity { get; set; }
        public int year { get; set; }
        

    }
    public class selectedCapacity
    {
        public int warehouseId { get; set; }

        public DateTime Date { get; set; }
        
        public int year { get; set; }


    }
    public class AllList
    {
        public List<HolidayList> HolidayLists { get; set; }
        public List<WorkingList> WorkingLists { get; set; }
        public List<DateTime> Holidays { get; set; }

    }
    public class HolidayList
    {
       
        public string WarehouseName { get; set; }
        public DateTime Date { get; set; }
        public int UpdateThresholdCapacity { get; set; }
        public string UserName { get; set; }


    }
    public class WorkingList
    {
       
        public string WarehouseName { get; set; }
        public DateTime Date { get; set; }
        public int UpdateThresholdCapacity { get; set; }
        public string UserName { get; set; }


    }
    public class SelectedData
    {

        public int warehouseid { get; set; }
        public int year { get; set; }
       


    }

    public class TemporaryData
    {
        public DateTime Deliverydate { get; set; }
        public int ordercount { get; set; }

    }
    public class DeliveryCapacityHistroyDataHistroyDC
    {
        public long Id { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public string Holidays { get; set; }
        public DateTime ? CreatedDate { get; set; }
        public DateTime ? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int ? ModifiedBy { get; set; }
        public int ? Year { get; set; }
    }
    public class ClusterHolidayHistroyDataHistroyDC
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public long WarehouseId { get; set; }
        public long ClusterId { get; set; }
        public string ClusterName { get; set; }
 
        public string Holiday { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? ModifiedBy { get; set; }
        public int? CreatedBy { get; set; }
        public int? Year { get; set; }
    }
    public class CustomerHolidayHistroyDataHistroyDC
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public long WarehouseId { get; set; }
        public long ClusterId { get; set; }
        public string ClusterName { get; set; }
        public long CustomerId { get; set; }
        public string Holiday { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? ModifiedBy { get; set; }
        public int? CreatedBy { get; set; }
        public int? Year { get; set; }
    }

}
