using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BatchCode
{
    public class BatchCodeElastic
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public DateTime createddate { get; set; }

        public string idnew { get; set; }
    }
}
