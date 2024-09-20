using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class RetailerMinOrder
    {
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public int MinOrderValue { get; set; }
    }

    public class StoreMinOrder
    {
        public ObjectId Id { get; set; }
        public int CityId { get; set; }
        public long StoreId { get; set; }
        public long WarehouseId { get; set; }
        public int MinOrderValue { get; set; }
       
        public int MinLineItem { get; set; }
    }
    public class UpdateStoreMinOrder
    {
        public string Id { get; set; }
        //public int CityId { get; set; }
        //public long StoreId { get; set; }
        //public long WarehouseId { get; set; }
        public int MinOrderValue { get; set; }
        public int MinLineItem { get; set; }
    }

    public class VATMCustomers
    {
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public string Data { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class ExtandedCompanyDetail
    {
        public ObjectId Id { get; set; }
        public string AppType { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public bool IsShowCreditOption { get; set; }
        public bool ischeckBookShow { get; set; }
        public bool IsRazorpayEnable { get; set; }
        public bool IsePayLaterShow { get; set; }
        public bool IsOnlinePayment { get; set; }
        public bool? IsFinBox { get; set; }
        public bool IsCreditLineShow { get; set; }
        public int? DONCODChanges { get; set; }
        public bool? StopCOD { get; set; }
        public int? DeliveryCancelationPer { get; set; }
        public int? MinDealItemOrderAmt { get; set; }
        public int? MinDealItemPurchase { get; set; }
    }

    public class CustomerRedispatchCharges
    {
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; }
        public int RedispatchCharges { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }

    public class SalesAppDefaultCustomers
    {
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
    }
}