using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class BrandMinDc
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int BrandCategoryMappingId { get; set; }
        public int SubCategoryMappingId { get; set; }
    }

    public class BuyerBrands
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int? BuyerId { get; set; }
        public int? WarehouseId { get; set; }
        public List<WarehouseMinDc> Warehouses { get; set; }
        public List<PeopleMinDc> Buyers { get; set; }
    }


    public class SubSubCategoryDcF
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryMappingId { get; set; }
        public int BrandCategoryMappingId { get; set; }
    }


}
