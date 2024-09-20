using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class BlockBrandDc
    {
       
        public string Id { get; set; }
        public int CatId { get; set; }
        public int SubCatId { get; set; }
        public int SubSubCatId { get; set; }
        public int CustomerType { get; set; } //1=kpp,2=retialer,3=distributor,4=all
        public int AppType { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string CustomerTypeName { get; set; }
        public string ApplicationType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public string WarehouseName { get; set; }
    }

    public class ResponseBlockBrandDc
    {
        public int TotalItem { get; set; }
        public List<BlockBrandDc> BlockBrandDcs { get; set; }
    }
    public class ResBlockBrandSave
    {
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class BlockBrandData
    {
        public string WarehouseName { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string ApplicationType { get; set; }
        public string CustomerTypeName { get; set; }
        
    }
}
