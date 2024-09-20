using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.VehicleMaster
{
    public class DriverMasterDc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string AadharNo { get; set; }
        public string AadharCopy { get; set; }
        public string AadharCopyBack { get; set; }
        public string Photo { get; set; }
        public string DLNo { get; set; }
        public DateTime DLNoValidity { get; set; }
        public string DLCopy { get; set; }
        public string TransportName { get; set; }
        public bool IsBlocked { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public bool IsActive { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
    }
    public class ResDriverMaster
    {
        public int totalcount { get; set; }
        public List<DriverMasterDc> DriverMasterDcs { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class DriverMasterExportDc
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string AadharNo { get; set; }
        public string DLNo { get; set; }
        public DateTime DLNoValidity { get; set; }
        public string TransportName { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
       
    }
}
