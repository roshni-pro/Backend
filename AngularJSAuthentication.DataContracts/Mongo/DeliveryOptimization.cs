using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
   public class DeliveryOptimization
    {
        public ObjectId Id { get; set; }
        public bool IsRunningUtility { get; set; }
        public int WarehouseId { get; set; }
    }
}
