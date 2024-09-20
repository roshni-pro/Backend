using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class TransferWHOrderSubmitDC
    {       
        public int RequestFromWarehouseId { get; set; } //RequestFromWarehouseId
        public int RequestToWarehouseId { get; set; }//RequestToWarehouseId
        public int CreatedById { get; set; }
        public List<ItemList> itemLists { get; set; }
        
    }

   public class ItemList
    {
        public int StockId
        {
            get; set;
        }
        public string Itemname
        {
            get; set;
        }
        public int Noofpics
        {
            get; set;
        }

    }

}
