using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SubCategoryDTO
    {
        
      
        public int Categoryid { get; set; }
     
        public string CategoryName { get; set; }
       
        public string SubcategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }

        public string GroupedCategoryName { get; set; }
    }
}
