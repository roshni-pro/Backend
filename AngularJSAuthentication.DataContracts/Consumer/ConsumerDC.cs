using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Consumer
{
    public class ConsumerDC
    {
        public string MobileNumber { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Otp { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string deviceId { get; set; }
        public string fcmId { get; set; }
        public bool TrueCustomer { get; set; }
    }
    public class LatLongChangeDC
    {        
        public double lat { get; set; }
        public double lg { get; set; }
        public int CustomerId { get; set; }
    }
    public class ConsumerUpdateDC
    {
        public string WhatsappNumber { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
        public string Mobile { get; set; }
        public string UploadProfilePichure { get; set; }
    }
    public class ConsumerAddressDC
    {
        public int CustomerId { get; set; }
        public string CompleteAddress { get; set; }
        public string ReciverName { get; set; }      
        public string Zipcode { get; set; }
        public string Floor { get; set; }
        public string Landmark { get; set; }
        public double Lat { get; set; }
        public double lng { get; set; }
        public string AddressName { get; set; }
        public string Address { get; set; }
        public string Address1 { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string Area { get; set; }
        public string CustomerName { get; set; }
        public string WhatsappNo { get; set; }
        public string ReferralCode { get; set; }

    }

    public class ItemNetInventoryDc
    {
        public int itemmultimrpid { get; set; }
        public int WarehouseId { get; set; }
        public bool isdispatchedfreestock { get; set; }
        public bool isavailable { get; set; }
        public int RemainingQty { get; set; }
    }
}
