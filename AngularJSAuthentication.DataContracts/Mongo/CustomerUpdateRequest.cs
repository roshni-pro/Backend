using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class CustomerUpdateRequest
    {
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public string UpdateedAddress { get; set; }
        public double UpdatedLat { get; set; }
        public double UpdatedLng { get; set; }
        public int RequestBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int Status { get; set; } //0- Pending, 1- Approved, 2-Reject
        public string WarehouseName { get; set; }
        public string status { get; set; }
    }
}
