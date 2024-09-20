using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ExecutiveAttendanceLog
    {
        public ObjectId Id { get; set; }
        public int ExecutiveId { get; set; }
        public string Day { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public int CityId { get; set; }
        public List<ExecutiveStoreDC> StoreData { get; set; }
        public List<ExecutiveClusterDC> ClusterData { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseIds { get; set; }
        public string StoreIds { get; set; }
        public string ClusterIds { get; set; }
        public bool IsLate { get; set; }
        public bool IsPresent { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long ChannelMasterId { get; set; }
    }

    public class ExecutiveStoreDC
    {
        public long StoreId{ get; set; }
        public string StoreName{ get; set; }
    }
    public class ExecutiveClusterDC
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }
    public class ExecutiveStoreClusterListDC
    {
        public List<ExecutiveStoreDC> StoreDatas { get; set; }
        public List<ExecutiveClusterDC> ClusterDatas { get; set; }
    }
}
