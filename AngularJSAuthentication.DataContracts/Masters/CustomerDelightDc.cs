using AngularJSAuthentication.DataContracts.Transaction.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CustomerDelightDc
    {

        public class CustomerDelightFilterDc
        {
            public int Skip { get; set; }
            public int Take { get; set; }
            public int CityID { get; set; }
            public int ClusterId { get; set; }
            public int IsActive { get; set; }
            public string CustomerType { get; set; }
            public int Status { get; set; }
            public string Keyward { get; set; }
            public int IsDocument { get; set; }

        }
        public class CustomerDelightRes
        {
            public List<CustomerDelightListDc> CustomerDelightListDcs { get; set; }
            public int TotalCount { get; set; }
        }
        public class CustomerDelightListDc
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public int Warehouseid { get; set; }
            public string Mobile { get; set; }
            public string Name { get; set; }
            public string ShippingAddress { get; set; }
            public string UpdatedAddress { get; set; }
            public string LandMark { get; set; }
            public string State { get; set; }
            public int Cityid { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public string FSAAI { get; set; }
            public int ClusterId { get; set; }
            public string ClusterName { get; set; }
            public double lat { get; set; }
            public double lg { get; set; }
            public string Shopimage { get; set; }
            public string CustomerVerify { get; set; }
            public string ShippingCity { get; set; }
            public int Status { get; set; }
            public string CustomerStatus { get; set; }
            public bool Active { get; set; }
            public string CustomerType { get; set; }
            public int ShopFound { get; set; }
            public string CaptureImagePath { get; set; }
            public string NewShippingAddress { get; set; }
            public double Newlat { get; set; }
            public double Newlg { get; set; }
            public string GSTno { get; set; }
            public string GSTImage { get; set; }
            public string WarehouseName { get; set; }
            public double Aerialdistance { get; set; }
            public string ShopStatus { get; set; }
            public string AreaName { get; set; }
            public string UploadRegistration { get; set; }
            public string CreatedBy { get; set; }
            public bool Nodocument { get; set; }
            public string Comment { get; set; }
            public double? Shoplat { get; set; }
            public double? Shoplg { get; set; }
            public string BillingAddress { get; set; }
            public string BillingAddress1 { get; set; }
            public string BillingState { get; set; }
            public string BillingCity { get; set; }
            public string BillingZipCode { get; set; }
            public int CustomerDocumentStatus { get; set; }
            public bool IsGSTCustomer { get; set; }
            public int ShippingAddressStatus { get; set; }
            public int? AddressId { get; set; }
            public string CRMTag { get; set; }
            public string TypeOfBuissness { get; set; }
            public long ChannelMasterId { get; set; }
        }

        public class CustomerTrackingVM{
            public int Id { get; set; }
            public int Status { get; set; }
            public string CustomerStatus { get; set; }
            public string CaptureImagePath { get; set; }
            public string NewShippingAddress { get; set; }
            public double Newlat { get; set; }
            public double Newlg { get; set; }
            public string ShopStatus { get; set; }
            public bool Nodocument { get; set; }
            public string Comment { get; set; }
            public int AppType { get; set; }
            public string ExecutiveName { get; set; }
        }

        public class CustomerTrackingWrapper {
            public CustomerDelightListDc Customer { get; set; }
            public List<CustomerTrackingVM> RequestList { get; set; }
            
        } 


        public class UpdateDelightDc
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public string Address { get; set; }
            public double Newlat { get; set; }
            public double Newlg { get; set; }
            public bool IsActive { get; set; }
            public string CustomerVerify { get; set; }
            public string AreaName { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public bool Nodocument { get; set; }
            public string Comment { get; set; }
            public string BillingAddress { get; set; }
            public string BillingAddress1 { get; set; }
            public string BillingState { get; set; }
            public string BillingCity { get; set; }
            public string BillingZipCode { get; set; }
            public string CustomerType { get; set; }
            public CustomerAddressVM CustomerAddressVM { set; get; }
        }
        public class CustomerDelightExportDc
        {
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public string Mobile { get; set; }
            public string Name { get; set; }
            public string ShippingAddress { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public string ClusterName { get; set; }
            public string CustomerVerify { get; set; }
            public string ShippingCity { get; set; }
            public string CustomerStatus { get; set; }
            public string CustomerType { get; set; }
            public string NewShippingAddress { get; set; }
            public string WarehouseName { get; set; }
            public double Aerialdistance { get; set; }
            public string ShopStatus { get; set; }
            public string AreaName { get; set; }
            public string  CreatedBy { get; set; }
            public DateTime? LocationDate { get; set; }
            public string CustomerAddress { get; set; }
            public string RequestedAddress { get; set; }
            public string CRMTag { get; set; }
        }
    }

    public class CustomerLocationDc
    {       
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Location { get; set; }
    }




    public class CustomerRejectRequestDc
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        
    }
}
