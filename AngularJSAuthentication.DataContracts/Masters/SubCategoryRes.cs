using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngularJSAuthentication.Model;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class SubCategoryRes
    {
        public bool Result { get; set; }
        public string msg { get; set; }
        public SubCategory SubCategory { get; set; }
    }
}
