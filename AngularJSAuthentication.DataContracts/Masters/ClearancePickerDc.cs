using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SearchClearancePickerDc
    {
        public string keyword { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public int warehouseid { get; set; }
        public int status { get; set; }   // 0 Approved , 1 Physically Moved


    }

    public class ClearancePickerAcceptRejDc
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }   // 0 pending , 1 accpet,2 reject
    }



    public class ClearancePickerDc
    {
        public long ClNonSaleableMovementOrderId { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int NumberofItems { get; set; }
        public string status { get; set; }   // Picking status  : Pending (Approved) , InProgress (Start time entry), Completed  (Physically Moved)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrderType { get; set; }  //
       // public int totalcount { get; set; }

    }
    public class pickerData
    {
        public int totalcount { get; set; }
        public List<ClearancePickerDc> ClearancePickerDc { get; set; }
    }

    public class ClearancePickerDetailsDc
    {

        public long ClNonSaleableMovementOrderId { get; set; }

        public long Id { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Quantity { get; set; }
        public string BatchCode { get; set; }
        public string itemname { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public DateTime MFGDate { get; set; }
        public int Status { get; set; }      //0 pending,1 accept,2 rejected
        public List<string> Barcode { get; set; }
    }

    public class GetPickerHistoryData
    {
        public long Id { get; set; }
        public string ModifiedBy { get; set; }
        public string Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long NumberofItems { get; set; }

    }


}
