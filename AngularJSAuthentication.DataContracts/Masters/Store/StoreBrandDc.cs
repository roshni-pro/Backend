using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class StoreBrandDc
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public int BrandCategoryMappingId { get; set; }
        public bool IsUniversal { get; set; }
    }

    public class PostBrandToDisplayDc
    {
        public string brandCategoryMappingIdList { get; set; }
    }
}
