using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts
{
    public class PageListDC
    {
       public List<AllPageListDC> AllPageListDC { get; set; }
       public List<AssignedPageListDc> AssignedPageListDc { get; set; }
    }
    public class AllPageListDC
    {
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public long Id { get; set; }
    }
    public class AssignedPageListDc
    {
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public long Id { get; set; }
    }
}
