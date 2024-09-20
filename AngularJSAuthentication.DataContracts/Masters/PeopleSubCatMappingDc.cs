using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class PeopleSubCatMappingDc
    {
        public long Id { get; set; }
        public int PeopleId { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string LogoUrl { get; set; }

        
    }

    public class SubCatMappingBrandDc
    {
        public int SubSubCategoryId { get; set; }
        public string SubSubCategoryName { get; set; }
    }
}

