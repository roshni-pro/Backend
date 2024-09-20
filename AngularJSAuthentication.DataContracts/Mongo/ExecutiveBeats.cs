using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ExecutiveBeats
    {
        public ObjectId Id { get; set; }
        public int PeopleId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime AssignmentDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DayStartTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DayEndTime { get; set; }
        public double? DayStartLat { get; set; }
        public double? DayStartLng { get; set; }
        public string DayStartAddress { get; set; }
        public double? DayEndLat { get; set; }
        public double? DayEndLng { get; set; }
        public string DayEndAddress { get; set; }
        public double? TodayTarget { get; set; }
        public List<PlannedRoute> PlannedRoutes { get; set; }
        public List<ActualRoute> ActualRoutes { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Day { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
    }

    public class PlannedRoute : SalespDTO
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? TravalStart { get; set; }
        public bool IsVisited { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? VisitedOn { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ActualRoute : PlannedRoute
    {
        public double ExecLat { get; set; }
        public double ExecLng { get; set; }
        public string ExecAddress { get; set; }
        public double Distance { get; set; }
        public string Comment { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckIn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckOut { get; set; }
        public string ShopCloseImage { get; set; }
        public bool? IsBeat { get; set; }
        public List<CustomerCheckInOutHistory> CustomerCheckInOutHistories { get; set; }
    }
    public class CustomerCheckInOutHistory
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckIn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckOut { get; set; }
    }

    public class SalespDTO
    {
        public int CustomerId { get; set; }
        // public int Id { get; set; }
        public int CustSupplierid { get; set; }
        public string Skcode { get; set; }
        public string CompanyName { get; set; }
        public string Day { get; set; }
        public int? BeatNumber { get; set; }
        public int ExecutiveId { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public bool IsKPP { get; set; }
        public int? CompanyId { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public bool IsAssigned { get; set; }

        public int? Cityid { get; set; }
        public string City { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public bool check { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public int Areaid { get; set; }
        public string AreaName { get; set; }
        public DateTime? DOB { get; set; }
        public string UploadRegistration { get; set; }
        public string RefNo { get; set; }
        public string ResidenceAddressProof { get; set; }
        public string Emailid { get; set; }
        public string Password { get; set; }
        public String Exception { get; set; }
        public string ColourCode { get; set; }
        public int OrderCount { get; set; }
        public int MaxOrderCount { get; set; }
        public string CustomerVerify { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string CustomerType { get; set; }
        public bool IsPhoneOrder { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
    }

    public class ExecutiveCompanyTarget
    {
        public ObjectId Id { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int CustomerCount { get; set; }
        public string Color { get; set; }

    }


    public class ExecuteBeatTarget
    {
        public ObjectId Id { get; set; }
        public int CityId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public decimal VisitedPercent { get; set; }
        public decimal ConversionPercent { get; set; }
        public decimal CustomerPercent { get; set; }
        public decimal OrderPercent { get; set; }
        public double ProductPareto { get; set; }
        public double CustomerPareto { get; set; }
        public int AvgLineItem { get; set; }
        public int AvgOrderAmount { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

    }


    public class NextDayBeatPlan
    {
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime PlanDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class SalesDashboardTodayMTDData
    {
        public ObjectId Id { get; set; }
        public int StoreId { get; set; }
        public List<long> StoreIds { get; set; }
        public int WarehouseId { get; set; }
        public string ClusterIds { get; set; }
        public long ChannelMatserId { get; set; }
        public int SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public string ChannelName { get; set; }
        public int VisitPlanned { get; set; }
        public int TotalCall { get; set; }
        public int ProductiveCall { get; set; }
        public int BeatOrder { get; set; }
        public double BeatSales { get; set; }
        public int ExtraVisit { get; set; }
        public int ExtraCall { get; set; }
        public int ExtraOrder { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public double? TodayTarget { get; set; }
        public double OutletCoverage { get; set; }
        public double StrikeRate { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int PhoneOrder { get; set; }
        public int ECO { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
        public double TodayStrikeRate { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double Eco { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayTotalCall { get; set; }
        public int TodayVisitPlanned { get; set; }

        public double MtdMonthlyTarget { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckIn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckOut { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class DigitalSalesBeatDSR
    {
        public ObjectId Id { get; set; }
        public int StoreId { get; set; }
        public List<long> StoreIds { get; set; }
        public int WarehouseId { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
        public string ClusterIds { get; set; }
        public int SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public int VisitPlanned { get; set; }
        public int TotalCall { get; set; }
        public int ProductiveCall { get; set; }
        public int BeatOrder { get; set; }
        public double BeatSales { get; set; }
        public int ExtraVisit { get; set; }
        public int ExtraCall { get; set; }
        public int ExtraOrder { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public double? TodayTarget { get; set; }
        public double OutletCoverage { get; set; }
        public double StrikeRate { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int PhoneOrder { get; set; }
        public int ECO { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
        public double TodayStrikeRate { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double Eco { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayTotalCall { get; set; }
        public int TodayVisitPlanned { get; set; }

        public double MtdMonthlyTarget { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckIn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CheckOut { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
    public class PerformanceDashboardMongo
    {
        public ObjectId Id { get; set; }
        public int StoreId { get; set; }
        public int WarehouseId { get; set; }
        public string ClusterIds { get; set; }
        public int SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public int ActiveRetailers { get; set; }
        public int VerifiedRetailer { get; set; }
        public int NewCustomer { get; set; }
        public double SalesValue { get; set; }
        public double MTDDispatchValue { get; set; }
        public double Cancellation { get; set; }
        public double QoQ { get; set; }
        public double CustomerPercentage { get; set; }
        public double ProductPercentage { get; set; }
        public int AchieveActiveRetailers { get; set; }
        public int AchieveVerifiedRetailer { get; set; }
        public int AchieveNewCustomer { get; set; }
        public double AchieveSalesValue { get; set; }
        public double AchieveMTDDispatchValue { get; set; }
        public double AchieveCancellation { get; set; }
        public double AchieveQoQ { get; set; }
        public double AchieveCustomerPercentage { get; set; }
        public double AchieveProductPercentage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
    //public class WeeklyPlannedRoute
    //{
    //    public int CustomerId { get; set; }
    //    public string Day { get; set; }
    //}

    [BsonIgnoreExtraElements]
    public class DashBoardColourCode
    {
        public string Colour { get; set; }
        public int SalesFrom { get; set; }
        public int? SalesTo { get; set; }
        public int TCFrom { get; set; }
        public int? TCTo { get; set; }
        public int PCFrom { get; set; }
        public int? PCTo { get; set; }
    }


    public class ExecutiveBeats_History
    {
        public ObjectId Id { get; set; }
        public int PeopleId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime AssignmentDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DayStartTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DayEndTime { get; set; }
        public double? DayStartLat { get; set; }
        public double? DayStartLng { get; set; }
        public string DayStartAddress { get; set; }
        public double? DayEndLat { get; set; }
        public double? DayEndLng { get; set; }
        public string DayEndAddress { get; set; }
        public double? TodayTarget { get; set; }
        public List<PlannedRoute> PlannedRoutes { get; set; }
        public List<ActualRoute> ActualRoutes { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Day { get; set; }
    }
    
}
