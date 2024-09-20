using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.VehicleMaster
{
   public class DBoyMasterDc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string AadharNo { get; set; }
        public string AadharCopy { get; set; }
        public string AadharCopyBack { get; set; }
        public string Photo { get; set; }
        public string Type { get; set; }
        public string AgencyName { get; set; }
        public string AgentOrTransport { get; set; } //(Agent/Transport)
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTill { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public int CityId { get; set; }
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public string CityName { get; set; }
        public string AgentName { get; set; }
        public string WarehouseName { get; set; }
        public int TripTypeEnum { get; set; }
        public string TripTypeName { get; set; }

        public long VehicleMasterId { get; set; }
        public double DboyCost { get; set; }
    }

    public class ResDBoyMaster
    {
        public int totalcount { get; set; }
        public List<DBoyMasterDc> DBoyMasterDcs { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }

    public class RegistrationNoListDC
    {
        public string RegistrationNo { get; set; }
        public long Id { get; set; }

    }

    public class DboyExportDc
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string AadharNo { get; set; }
        public string Type { get; set; }
        public string AgencyName { get; set; }
        public string AgentOrTransport { get; set; } //(Agent/Transport)
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTill { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public string CityName { get; set; }
        public string AgentName { get; set; }
        public string WarehouseName { get; set; }
        public int CityId { get; set; }
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
      
    }
}
