using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
  public  class GrowBusinessHistoryDc
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WarehouseId { get; set; }
        public int SubCatId { get; set; }
        public string SubCatName { get; set; }
        public string Type { get; set; }  // Banner, Flashdeal
        public string WarehouseName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ItemName { get; set; }

        public int? ObjectId { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalReceived { get; set; }
        public int? TotalSent { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ImageUrl { get; set; }
        public int? ItemMultiMrpId { get; set; }
        public int? Moq { get; set; }
        public int? AvailableQty { get; set; }
        public int? MaxQty { get; set; }
        public double? FlashDealPrice { get; set; }
        public string ReqTypeName { get; set; }
        public string Status { get; set; }
        public int? SequenceNo { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationDescription { get; set; }
        public string NotificationImage { get; set; }
        public string MurliDescription { get; set; }
        public string MurliNotificationMsg { get; set; }
        public string MurliNotificationTitle { get; set; }
        public string MurliFile { get; set; }
        public string AppBannerDiscription { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public List<FlashDealItemsDc> FlashDealItemsDcs { get; set; }
    }
    public class ResGrowBusinessHistory
    {
        public int totalcount { get; set; }
        public List<GrowBusinessHistoryDc> GrowBusinessHistoryDcs { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class ResGrowBusinessHistoryExportDc
    {
        public List<FlashDealHistoryExportDc> FlashDealHistoryExportDcs { get; set; }
        public List<AppBannerHistoryExportDc> AppBannerHistoryExportDcs { get; set; }
        public List<NotificationHistoryExportDc> NotificationHistoryExportDcs { get; set; }
        public List<MurliHistoryExportDc> MurliHistoryExportDcs { get; set; }
        public List<BrandStoreRHistoryExportDc> BrandStoreRHistoryExportDcs { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class FlashDealHistoryExportDc
    {
        public int? ObjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubCatName { get; set; }
        public string Type { get; set; }  // Banner, Flashdeal
        public string WarehouseName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string CategoryName { get; set; }
        public string ItemName { get; set; }

        //public int? TotalViews { get; set; }
        //public int? TotalReceived { get; set; }
        //public int? TotalSent { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ItemMultiMrpId { get; set; }
        public int? Moq { get; set; }
        public int? AvailableQty { get; set; }
        public int? MaxQty { get; set; }
        public double? FlashDealPrice { get; set; }
        public double MRP { get; set; }
        public string Status { get; set; }
        //public int? SequenceNo { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        
    }
    public class AppBannerHistoryExportDc
    {
        public int? ObjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubCatName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string Warehouse { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string AppBannerDiscription { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalReceived { get; set; }
        public int? TotalSent { get; set; }
    }
    public class NotificationHistoryExportDc
    {
        public int? ObjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubCatName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string Warehouse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Status { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationDescription { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalReceived { get; set; }
        public int? TotalSent { get; set; }
    }
    public class MurliHistoryExportDc
    {
        public int? ObjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubCatName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string Warehouse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Status { get; set; }
        public string MurliDescription { get; set; }
        public string MurliNotificationMsg { get; set; }
        public string MurliNotificationTitle { get; set; }
        public string MurliFile { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalReceived { get; set; }
        public int? TotalSent { get; set; }
    }
    public class BrandStoreRHistoryExportDc
    {
        public int? ObjectId { get; set; }
        public string CategoryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubCatName { get; set; }
        public string Comment { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string Warehouse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string Status { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalReceived { get; set; }
        public int? TotalSent { get; set; }
    }
}
