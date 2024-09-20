using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class StoreBrandViewModel
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long BrandCategoryMappingId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class StoreCategorySubCategoryBrand
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreLogo { get; set; }
        public int Categoryid { get; set; }
        public string categoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public bool AllowInactiveOrderToPending { get; set; }
    }
}
