using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.CRM
{

    public class CustomerProfileDc
    {
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? TotalOrder { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? LastCallDate { get; set; }
        public int? TotalCalls { get; set; }
        public int? TotalVisit { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public double AOV { get; set; }
        public double TOV { get; set; }
        public bool IsPhysicalVisit { get; set; }
        public string ShopName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int? LastOrderBeforeDays { get; set; }
        public int? LastVisitDays { get; set; }
        public int? LastCallDays { get; set; }
        public DateTime? CallTime { get; set; }
        public DateTime? VisitTime { get; set; }
        public string CRMTags { get; set; }
        public string BillingAddress { get; set; }
        public long? CheckOutReasonId { get; set; }
    }

    public class CustomerProfileSearchDc
    {
        public int WarehouseId { get; set; }
        public List<int> ClusterId { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public string Keyword { get; set; }
    }
    public class CustomerProfileResponseDc
    {
        public List<CustomerProfileDc> customerProfileDcs { get; set; }
        public int totalRecords { get; set; }
    }

    public class CustomerListDc
    {
        public List<int> CustomerId { get; set; }
        public int totalRecords { get; set; }
    }

    public class GetSkOrderDataDc
    {
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrder { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public double AOV { get; set; }
        public double TOV { get; set; }
        public string ShopName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int? LastOrderBeforeDays { get; set; }
        public string CRMTags { get; set; }
        public string BillingAddress { get; set; }
    }

    public class GetCallDataDc
    {
        public int CustomerId { get; set; }
        public int? TotalCalls { get; set; }
        public int? LastCallDays { get; set; }
        public DateTime? CallTime { get; set; }
        public DateTime? VisitTime { get; set; }
        public int? TotalVisit { get; set; }
        public int? LastVisitDays { get; set; }
        public bool IsPhysicalVisit { get; set; }
        public DateTime? LastCallDate { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public long? CheckOutReasonId { get; set; }
    }

    public class CRmCustomerDataGetDc
    {
        public DateTime? ActionDate { get; set; }
        public DateTime? ActionTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string CallConnect { get; set; }
        public string OrderTaken { get; set; }
        public string PhysicalVisit { get; set; }
        public string OtherComment { get; set; }
        public long? Id { get; set; }

    }

    public class CallAndVisitHistoryDc
    { 
        public List<CRmCustomerDataGetDc> CallData { get; set; }
        public List<CRmCustomerDataGetDc> VisitData { get; set; }
    }

    public class CallandHistorySummaryDc
    {
        public DateTime? ActionDate { get; set; }
        public DateTime? ActionTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string CallConnect { get; set; }
        public string OrderTaken { get; set; }
        public string PhysicalVisit { get; set; }
        public string OtherComment { get; set; }
        public long? Id { get; set; }
        public string IsCustomerInterested { get; set; }
        public long? OrderId { get; set; }
        public double? OrderAmount { get; set; }
        public string TechProductInquiry { get; set; }
        public string SKUInquiry { get; set; }
        public string Offer { get; set; }
        public string MyTarget { get; set; }
        public string GameSection { get; set; }
        public string RequiredItemInquiry { get; set; }

    }

    public class CRMPlatformConfigListDc
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class InsertCRMPlatformConfigDc
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsDigital { get; set; }
        public List<string> Details { get; set; }
    }

    public class CRMPlatformListDc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class CRMPlatformMappingListDc
    {
        public long CrmId { get; set; }
        public int CRMPlatformId { get; set; }
        public int CRMPlatformMappingId { get; set; }
        public string CrmName { get; set; }
        public string CRMPlatformName { get; set; }

    }
}
