using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.OrderProcess
{
    public class GeneratePickerAutoPickDc
    {
        public List<int> OrderIds { set; get; }  //Readytopick or Readytodispatched
        public int CreatedBy { set; get; }
        public int DboyId { set; get; }
        //public int ClusterId { set; get; }
        public int WarehouseId { set; get; }

    }
}
