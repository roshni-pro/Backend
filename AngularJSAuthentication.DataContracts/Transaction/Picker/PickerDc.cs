using AngularJSAuthentication.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{

    public class rejectPkReportDc
    {
        public long currentMonthCount { get; set; }
        public long lastMonthCount { get; set; }
        public long todayCount { get; set; }        
        public string WarehouseName { get; set; }
        public long selectedDateCount { get; set; }
    }
    public class PickerExportDc
    {
        public List<int> warehouselist { get; set; }
        public int month { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? isSelectedDate { get; set; }
    }

    public class PickerExportReportDc
    {
        public List<int> warehouselist { get; set; }
        public List<int> allWarehouses { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class rejectedPickerdetailDc
    {
        public long Id { get; set; }
        public string pickerPerson { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string dboy { get; set; }
        public string ClusterName { get; set; }
        public string createdby { get; set; }
    }
    public class rejectedPickerdetailListDc
    {
        public long OrderId { get; set; }
        public string ItemName { get; set; }
        public long ItemMultiMrpId { get; set; }
        public long price { get; set; }
        public long Qty { get; set; }
        public string Comment { get; set; }
        public int id { get; set; }
    }
    public class rejPickerDc
    {
        public List<int> warehouselist { get; set; }
        public List<int> clusterId { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public int? keyword { get; set; }
        public int? skip { get; set; }
        public int? take { get; set; }
        public bool IsCount { get; set; }

    }
    public class canceledpickersDc
    {
        public List<canceledpickerslist> RejectedPickerList { get; set; }
        public int Totalcount { get; set; }
    }
    public class canceledpickerslist
    {
        public int PickerNumber { get; set; }
        public string PickerpersonName { get; set; }
        public string InventorySupNAme { get; set; }
        public int NoOfOrders { get; set; }
        public int LineItemQuantity { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime RejectedDate { get; set; }
        public string RejectedBy { get; set; }
        public int amt { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
    }

    public class OrderDetailsPickerDC
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public double OrderAmount { get; set; }
        public int OrderType { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string BillingAddress { get; set; }
        public double? TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string SalesPerson { get; set; }
        public string ShippingAddress { get; set; }

        public int? OrderTakenSalesPersonId { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public int? ReDispatchCount { get; set; }
        public int? ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string comments { get; set; }
        public string itemNumber { get; set; }
        public string itemname { get; set; }
        public int qty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool isCompleted { get; set; }
        public bool IsLessCurrentStock { get; set; }
        public bool IsItemInOtherOrder { get; set; }
        public string OfferCode { get; set; }
        public bool IsComplteda7 { get; set; }
        public string paymentMode { get; set; }
        public List<OrderDetailsDc> orderDetails { get; set; }
        public int? freeItemCount { get; set; }
        public string OrderTypestr
        {
            get
            {
                if (OrderType == 0 || OrderType == 1)
                    return "G"; //General
                else if (OrderType == 2)
                    return "B"; //Bundle
                else if (OrderType == 3)
                    return "R"; //Return
                else if (OrderType == 4)
                    return "D"; //Distributor
                else if (OrderType == 5)
                    return "Z"; //Zaruri
                else if (OrderType == 6)
                    return "DO"; //Damage Order
                else if (OrderType == 7)
                    return "F"; //Franshise
                else if (OrderType == 8)
                    return "CL"; //Clearance
                else if (OrderType == 11)
                    return "C"; //Consumer
                else
                    return "G";
            }
        }
        public string OrderColor { get; set; }
        public string Trupay { get; set; }
        public string Customerphonenum { get; set; }
        public double reditemavaiableValue { get; set; }
        public string reditemavaiableValuestr
        {
            get
            {
                string result = "";
                if ((OrderColor == "rgb(255, 153, 153)" || OrderColor == "yellow") && reditemavaiableValue > 0)
                {
                    result = Math.Round((reditemavaiableValue / (GrossAmount + (WalletAmount ?? 0) + (BillDiscountAmount ?? 0))) * 100, 2) + "%";
                }
                return result;
            }
        }
        public string CustomerType { get; set; }
        public int WarehouseId { get; set; }
        public bool IsSelected { get; set; }
    }
    public class OrderDetailsDc
    {

        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public String City { get; set; }
        public string Mobile { get; set; }
        public DateTime OrderDate { get; set; }
        public int CompanyId { get; set; }
        public int? CityId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }

        public int? FreeWithParentItemId { get; set; }//Child 

        public string Itempic { get; set; }
        public string itemname { get; set; }
        public string SellingUnitName { get; set; }
        public string itemcode { get; set; }
        public string itemNumber { get; set; }
        public string HSNCode { get; set; }
        public string Barcode { get; set; }

        public double price { get; set; }
        public double UnitPrice { get; set; }
        public double Purchaseprice { get; set; }

        public int MinOrderQty { get; set; }
        public double MinOrderQtyPrice { get; set; }
        public int qty { get; set; }
        // new calculation shopkirana
        // to show no of qty in peti
        public int Noqty { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TotalAmountAfterTaxDisc { get; set; }

        public double NetAmmount { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmmount { get; set; }
        public double NetAmtAfterDis { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        //for cess
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }


        public double TotalAmt { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public string Status { get; set; }
        public double SizePerUnit { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPoint { get; set; }
        public double NetPurchasePrice { get; set; }

        public bool IsFreeItem { get; set; }
        public bool IsDispatchedFreeStock //  Freeitem dispatched from Freestock (true)or currentstock (false)
        {
            get; set;
        }
        public double CurrentStock { get; set; }
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string status { get; set; }
        public string SupplierName { get; set; }
        //multimrp
        public int ItemMultiMRPId { get; set; }
        public double? OrderedTotalAmt { get; set; }//excel export
        public string ExecutiveName { get; set; }//Sudhir
        public long StoreId { get; set; }
        public string StoreName { get; set; }

    }

    public class pikeritemDC
    {
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string itemcode { get; set; }
        public string Number { get; set; }
        public string itemname { get; set; }
        public string SellingUnitName { get; set; }
        public int ItemId { get; set; }
        public string Barcode { get; set; }
    }
    public class GetItemOrdersList
    {
        public List<int> orderid { get; set; }
        public int DboyId { get; set; }
        public string MongoObjectId { get; set; }
    }
    public class GetItemOrdersListV2
    {
        public List<int> orderid { get; set; }
        public int peopleId { get; set; }
        public int ClusterId { get; set; }
        public int dboyid { get; set; }
        public List<LineItemCutItemDc> LineItemCutItems { get; set; }
        public string MongoObjectid { get; set; }
    }

    public class LineItemCutItemDc
    {
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int Qty { get; set; }
        public string QtyChangeReason { get; set; }

    }

    public class pickeritemlist
    {
        public List<int> orderid { get; set; }
        public List<pickeritemlistdc> Pickeritemlist { get; set; }
    }

    public class PickeritemlistByCluster
    {
        public List<int> orderid { get; set; }
        public int ClusterId { get; set; }
    }
    public class pickeritemlistdc
    {
        public double qty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string SellingUnitName { get; set; }
        public int orderIds { get; set; }
        public int OrderdetailsId { get; set; }
        public bool IsFreeItem { get; set; }
        //  public int AvailableQty { get; set; }

    }

    public class sendPickertoApp
    {
        public long pickerId { get; set; }
        public int Totalqty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string Number { get; set; }
        public string itemname { get; set; }
        public string SellingUnitName { get; set; }
        public List<string> Barcode { get; set; }
        public List<OrderDetailsSPA> OrderDetailsSPA { get; set; }
        public bool IsClearance { get; set; }

    }



    public class OrderDetailsSPA
    {
        public bool IsFreeItem { get; set; }
        public long pickerId { get; set; }
        public long pickerDetailsId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int orderid { get; set; }
        public int? OrderDetailsId { get; set; }
        public double qty { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }

    }

    public class PickerJobListDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int count { get; set; }
        public bool IsApproved { get; set; }
        public string Commenet { get; set; }
        public bool IsComplted { get; set; }
        public string Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? CanceledDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public string SubmittedBy { get; set; }
        public string CanceledBy { get; set; }
        public bool IsCheckerChecked { get; set; }
        public string DeliveryIssuanceId { get; set; }
        public double amount { get; set; }
        public string dboyName { get; set; }
        public string ClusterName { get; set; }
        public bool IsInventorySupervisor { get; set; }
        public bool IsInventory { get; set; }
        public bool IsInventorySupervisorStart { get; set; }
    }

    public class GetPendingOrderFilterDc
    {
        public int? ClusterId { get; set; }
        public int WareHouseID { get; set; }
        public int ItemPerPage { get; set; }
        public int PageNo { get; set; }
        public int TotalRecords { get; set; }
        public int? OrderId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public List<string> PaymentFrom { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public int CustomerId { get; set; }
        public int CustomerType { get; set; }
        public int OrderType { get; set; }       //cl=8
    }

    public class OrderPickerMasterDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? DboyId { get; set; }
        public int? AgentId { get; set; }

        public int PickerPersonId { get; set; }   //People ID of picker selected
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsComplted { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public string MultiDeliveryIssuanceIds { get; set; }
        public double? TotalAssignmentAmount { get; set; }

        public List<OrderPickerDetailsDc> orderPickerDetails { get; set; }
        public string CurrentStatus { get; set; }
        public string PickerPersonName { get; set; }
        public bool IsDispatched { get; set; }
        public bool IsApproved { get; set; }
        public int orderpickercount { get; set; }
        public int orderpickercountitem { get; set; }
        public int Status { get; set; } //Default value is 0 = New Request, 1= InProgress(Maker),  2=Submitted(Maker) , 3= Dispatched(ApprovedAndDispatched), 4=Canceled, 5 =RePicking(when Checker Reject Item) 
        public bool IsCanceled { get; set; }
        public string CreatedByName { get; set; }
        //public int differenceHrs { get; set; }
        public bool IsPickerGrabbed { get; set; }
        public DateTime? PickerGrabbedTime { get; set; }
        public bool IsCheckerGrabbed { get; set; }
        public DateTime? CheckerGrabbedTime { get; set; }
        public string DBoyName { get; set; }
        public string AgentName { get; set; }
        public int WarehouseId { get; set; }
        public string DboyMobile { get; set; }
        public string InventorySupervisorName { get; set; }
        public int? InventorySupervisorId { get; set; }
        public string Comment { get; set; }
    }

    public class OrderPickerDetailsDc
    {

        public int id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int Qty { get; set; }
        public int? DispatchedQty { get; set; }
        public string Comment { get; set; }
        public bool IsFreeItem { get; set; }
        public int Status { get; set; }
        public double MRP { get; set; }
        public string Orderids { get; set; }
        public int PurchaseMinOrderQty { get; set; }
    }

    public class AcceptRejectDC
    {
        public int OrderId { get; set; }
        public long pickerDetailsId { get; set; }
        //public int OrderPickerMasterId { get; set; }
        public string Status { get; set; }
        public string comment { get; set; }

    }


    public class OrderPickerDetailWithBatchDc
    {
        public List<OrderPickerDetailDC> OrderPickerDetailDCs { set; get; }
        public List<OrderPickerDetailBatchDc> OrderPickerDetailBatchDcs { set; get; }

    }


    public class OrderPickerDetailDC
    {
        public long OrderPickerMasterId { get; set; }
        public int pickerDetailsId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public int Qty { get; set; }
        public string Comment { get; set; }
        public bool IsFreeItem { get; set; }
        public int Status { get; set; } // // Default value is 0, Accept = 1 (Maker),Reject = 3(Maker), Approvered = 2(Approver), ApproverReject = 4(Approver) Picker Reject item for picking(reject all line item of that order)  
        public string Number { get; set; }
        public string Barcode { get; set; }
        public double MRP { get; set; }
        public bool IsRTD { get; set; }
        public bool isClearance { get; set; }
        public List<OrderPickerDetailBatchDc> OrderPickerDetailBatchs { set; get; }

    }
    public class OrderPickerDetailBatchDc
    {
        public string BatchCode { get; set; }
        public int Qty { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int pickerDetailsId { get; set; }

    }


    public class CurrentToPlanedStockMoveDC
    {

        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int Qty { get; set; }
        public int? DispatchedQty { get; set; }
        public string Comment { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsDispatchedFreeStock
        {
            get; set;
        }
        public int Status { get; set; } //Default value is 0, Accept = 1 (Maker),Reject = 3(Maker), Approvered = 2(Approver), ApproverReject = 4(Approver)  
        public int WarehouseId { get; set; }
    }



    public class ReviewerApprovedDc
    {
        public int PickerId { get; set; }
        public int UserId { get; set; }

    }
    public class ApprovedDispatchedDC
    {
        public int PickerId { get; set; }
        public int UserId { get; set; }
        public int DeliveryBoyId { get; set; }
        public int AgentId { get; set; }
        public List<int> OrderidRedispachedOrder { get; set; }
    }


    public class PickerJobListRes
    {
        public List<PickerJobListDc> PickerJobLists { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class PickerTimerDc
    {
        public long Id { get; set; }
        public long OrderPickerMasterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int CreatedBy { get; set; }
        public int differenceHrs { get; set; }
        public int Type { get; set; } // value 0 (Maker) and 1 Checker
    }


    public class PaggingDataPickerDc
    {
        public int total_count { get; set; }
        public List<OrderDetailsPickerDC> ordermaster { get; set; }
        public List<int> orderIds { get; set; }
    }

    public class OrderPickerMasterDcNew
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? DboyId { get; set; }
        public int? AgentId { get; set; }

        public int PickerPersonId { get; set; }   //People ID of picker selected
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsComplted { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public double? TotalAssignmentAmount { get; set; }

        public List<OrderPickerDetailsDcnew> orderPickerDetails { get; set; }
        public string CurrentStatus { get; set; }
        public string PickerPersonName { get; set; }
        public bool IsDispatched { get; set; }
        public bool IsApproved { get; set; }
        public int orderpickercount { get; set; }
        public int orderpickercountitem { get; set; }
        public int Status { get; set; } //Default value is 0 = New Request, 1= InProgress(Maker),  2=Submitted(Maker) , 3= Dispatched(ApprovedAndDispatched), 4=Canceled, 5 =RePicking(when Checker Reject Item) 
        public bool IsCanceled { get; set; }
        public string CreatedByName { get; set; }
        //public int differenceHrs { get; set; }
        public bool IsPickerGrabbed { get; set; }
        public DateTime? PickerGrabbedTime { get; set; }
        public bool IsCheckerGrabbed { get; set; }
        public DateTime? CheckerGrabbedTime { get; set; }
        public string DBoyName { get; set; }
        public string AgentName { get; set; }
        public int WarehouseId { get; set; }
        public int OrderType { get; set; }
        public bool IsNotCreateAssingment { get; set; }
        public string IRNNo { get; set; }
    }

    public class OrderPickerDetailsDcnew
    {

        public int id { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public string Status { get; set; }
        public string EwayBillNumber { get; set; }
        public bool IsGenerateIRN { get; set; }
        public string Skcode { get; set; }
        public DateTime OrderedDate { get; set; }
    }
    public class PickerChooseMasterDC
    {
        public string Id { get; set; }
        public int CreatedBy { get; set; }
        public int UpdateBy { get; set; }
        public bool Finalize { get; set; }
        public int ClusterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<MongoPickerOrderMasterDC> mongoPickerOrderMaster { get; set; }
    }
    public class MongoPickerOrderMasterDC
    {
        public string Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int WarehouseId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public int PickerOrderStatus { get; set; } // 1 Pick,2 Pick Generate,3 reject
        public string PickerSelectStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string OrderColor { get; set; }
        public string ShippingAddress { get; set; }
        public double GrossAmount { get; set; }
        public List<MongoPickerOrderDetailsDC> orderDetails { get; set; }
    }
    public class MongoPickerOrderDetailsDC
    {
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string itemNumber { get; set; }
        public string itemname { get; set; }
        public bool IsFreeItem { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public int CurrentStock { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsDispatchedFreeStock { get; set; }

    }

    public class OnRTDOrderPickerDetailDC
    {
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int Qty { get; set; }
        public int Status { get; set; } // // Default value is 0, Accept = 1 (Maker),Reject = 3(Maker), Approvered = 2(Approver), ApproverReject = 4(Approver) Picker Reject item for picking(reject all line item of that order)  
        public int ItemId { get; set; }
        public string HSNCode { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double TotalCessPercentage { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
    }



    public class PickerCustomerDc
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string GSTNo { get; set; }
        public string CustomerType { get; set; }
        public string FcmId { get; set; }
        public bool IsGstRequestPending { set; get; }
        public bool IsGenerateIRN { set; get; }
        public string PanNo { get; set; }

        public bool IsTCSExemption { get; set; }
        public bool IsPanVerified { get; set; }


    }
    public class PickerRejectDc
    {
        public List<int> OrderIds { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public int PickerId { get; set; }


    }


    public class OrderPickerDetailDTO
    {

        public int id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int Qty { get; set; }
        public bool IsFreeItem { get; set; }
        public int Status { get; set; } //Default value is 0, Accept = 1 (Maker),Reject = 3(Maker), Approvered = 2(Approver), ApproverReject = 4(Approver)  
        public long OrderPickerMasterId { get; set; }
        public bool IsClearance { get; set; }
        public string Comment { get; set; }

    }


    #region New Picker Changes 14April2023
    public class OrderPickingDetailDc
    {
        public List<OrderPickingItemDc> PickerLineItems { get; set; }
        public bool IsClearance { get; set; }
    }
    public class OrderPickingItemDc
    {
        public long OrderPickerMasterId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int Totalqty { get; set; }
        public int TotalAcceptedQty { get; set; }
        public int TotalRejectedQty { get; set; }
        public int NoOfOrders { get; set; }
        public bool IsClearance { get; set; }
        public bool IsActionTaken { get; set; }
        public bool IsRepicking { get; set; }
        public string CategoryName { get; set; }


    }

    public class SearchViaBarcodeDc
    {
        public int ItemMultiMrpId { get; set; }
        public bool IsClearance { get; set; }
        public string Barcode { get; set; }
        public long OrderPickerMasterId { get; set; }
    }

    public class OrderPickingItem
    {
        public List<OrderPickingItemBatchDc> OrderPickingItemBatchs { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int Totalqty { get; set; }
        public int TotalPickingQty { get; set; }
        public int TotalAcceptedQty { get; set; }
        public int TotalRejectedQty { get; set; }
        public int NoOfOrders { get; set; }
        public bool IsClearance { get; set; }
        public bool IsActionTaken { get; set; }
        public bool IsRepicking { get; set; }
        public long OrderPickerMasterId { get; set; }

    }
    public class OrderPickingItemBatchDc
    {
        public int AvlBatchQty { get; set; }
        public int PickingBatchQty { get; set; }
        public string BatchCode { get; set; }
        public bool IsChecked { get; set; }
        public List<BatchOrderDc> BatchOrders { set; get; }
    }


    public class SearchBatchOrderDc
    {
        public int ItemMultiMrpId { get; set; }
        public bool IsClearance { get; set; }
        public string BatchCode { get; set; }
        public long OrderPickerMasterId { get; set; }

    }

    public class ScanItemDc
    {
        public string Barcode { get; set; }
        public long OrderPickerMasterId { get; set; }
    }


    public class SubmitPickedBatchDc
    {
        public int ItemMultiMrpId { get; set; }
        public bool IsClearance { get; set; }
        public int UserId { get; set; }
        public long OrderPickerMasterId { get; set; }
        public List<int?> RejectedOrders { set; get; }
        public string Comment { get; set; }

    }


    public class BatchOrderDc
    {
        public int OrderId { get; set; }
        public int OrderPickingBatchQty { get; set; }
        public bool IsChecked { get; set; }
        public string BatchCode { get; set; }

    }
    public class PickerResMsg
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public dynamic Data { get; set; }
    }


    public class CheckerTaskListDc
    {
        public long OrderPickerMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public double TotalAmount { get; set; }
        public string DBoyName { get; set; }
        public int NoOfItems { get; set; }
        public string Pickerperson { get; set; }
        public string CreatedBy { get; set; }
    }


    public class MakerTaskListDc
    {
        public long OrderPickerMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public double TotalAmount { get; set; }
        public int NoOfOrders { get; set; }
        public int NoOfItems { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? StartTime { get; set; }//
    }

    public class CheckerGrabbedPickerListDC
    {
        public long OrderPickerMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public double TotalAmount { get; set; }
        public string DBoyName { get; set; }
        public int NoOfItems { get; set; }
        public string Pickerperson { get; set; }
        public string CreatedBy { get; set; }
        public int NoOfOrders { get; set; }
        public string Status { get; set; }
        public DateTime? StartTime { get; set; }
    }


    #endregion


}
