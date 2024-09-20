using AngularJSAuthentication.Model.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class StoreOwnerDc
    {
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int PeopleId { get; set; }
    }
   public class WarehouseWiseSaleLeadDC
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
        public int WarehouseId { get; set; }
    }
    public class GetWarehouseWiseSaleLeadDC
    {
        public int Id { get; set; }
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }
        public string SalesLeadName { get; set; }
        public int WarehouseId { get; set; }
        public int StoreId { get; set; }
        public int SalesLeadId { get; set; } //warehouse Sales Lead
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
    public class PostWarehouseWiseSaleLeadDC 
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<WarehouseStoreMapping> warehouseStoreMapping { get; set; }
    }
    public class PostStoreDc
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int StoreId { get; set; }
        public int SalesLeadId { get; set; } //warehouse Sales Lead
        public bool Active { get; set; } //warehouse Sales Lead
    }
}
