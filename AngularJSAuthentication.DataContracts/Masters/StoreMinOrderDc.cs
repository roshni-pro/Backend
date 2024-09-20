using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace AngularJSAuthentication.DataContracts.Masters
{
  public  class StoreMinOrderDc
    {
        
        public string Id { get; set; }
        public int CityId { get; set; }
        public long StoreId { get; set; }
        public int MinOrderValue { get; set; }
        public string StoreName { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int MinLineItem { get; set; }
    }
}
