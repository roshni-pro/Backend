using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    #region Executive Attendance Config
    public class AttendanceRuleConfigDC
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long ChannelMasterId { get; set; }
        public string StoreName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }
    }
    public class AttendanceConfigCityDC
    {
        public long AttendaceConfigId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AttendanceRuleEditLog
    {
        public List<AttendanceRuleConfigLogDC> AttendanceRuleConfigLog { get; set; }
        public long TotalRecords { get; set; }       
    }

    public class AttendanceRuleConfigLogDC
    {
        public long Id { get; set; }
        public long AttendanceRuleConfigId { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }

    
        public bool? IsCheckinBeatShopUpdated { get; set; }
        public bool? IsCheckInTimeUpdated { get; set; }
        public bool? IsCheckOutBeatShopUpdated { get; set; }
        public bool? IsFullDayBeatShopUpdated { get; set; }
        public bool? IsMinimumVisitUpdated { get; set; }
        public bool? DayMinVisitsUpdated { get; set; }
        public bool? IsTADABeatShopUpdated { get; set; }
        public bool? IsTADARequiredUpdated { get; set; }
        public bool? TADACallsUpdated { get; set; }
        public bool? DescriptionUpdated { get; set; }
       
        public DateTime CreatedDate { get; set; }
        public string Remark { get; set; }
       
    }
    #endregion

    #region Executive Attendance Report
    public class AllExecutiveAttendanceReportFilter
    {
        public List<int> StoreIds { get; set; }
        public List<int> ChannelMasterId { get; set; }
        public List<int> CityIds { get; set; }
        public List<int> WarehouseIds { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    #endregion

    #region faltu
    public class ExecutiveData
    {
        //public ExecutiveData()
        //{
        //    ExecutiveDatas = new List<ExecutiveData>();
        //}

        public ObjectId Id { get; set; }

        [Key]
        public long ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }

        //[NotMapped]
        //public List<ExecutiveData> ExecutiveDatas { get; set; }
    }

    public class UpdateExecutiveAttendanceDetails
    {
        public int ExecutiveId { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public string WarehouseIds { get; set; }
        public string StoreIds { get; set; }
        public string ClusterIds { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public bool IsLate { get; set; }

    }

    public class AttendenceDetailDC
    {
        public int ExecutiveId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? FirstCheckIn { get; set; }
        public TimeSpan? LastCheckOut { get; set; }
        public string TADA { get; set; }
        public string Status { get; set; }
        public bool IsLate { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public string Description { get; set; }
    }
    public class ExecutiveAttendanceLogListDC
    {
        public double PresentDay { get; set; }
        public int AbsentDay { get; set; }
        public int TADADay { get; set; }
        public List<AttendenceDetailDC> AttendenceDetailList { get; set; }
    }

    public class updateAttendanceDc
    {
        public int ExecutiveId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsBeatShop { get; set; }
        public bool CheckOut { get; set; }
        public DateTime? checkin { get; set; }
        public bool IsPhoneOrder { get; set; }
    }

    public class ExecutiveAttendanceLogDC
    {
        public int ExecutiveId { get; set; }
        public string EmpCode { get; set; }
        public string ExecutiveName { get; set; }
        public string WarehouseIds { get; set; }
        public string StoreIds { get; set; }
        public string ClusterIds { get; set; }
        public int CityId { get; set; } //new add
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int TADA { get; set; }
    }
    public class ExecutiveAttendanceLogDCs
    {
        public int TotalRecords { get; set; }
        public List<ExecutiveAttendanceLogDC> executiveAttendanceLogDCs { get; set; }
    }


    public class AttendanceRuleConfigsLogDc
    {
        public long ConfigId { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public long StoreId { get; set; }
        public int CityId { get; set; }
        public long ChannelMasterId { get; set; }
    }

    public class ExecutiveAttendanceRowDetailsDC
    {
        public string EmployeeCode { get; set; }
        public string ExecuitveName { get; set; }
        public string Store { get; set; }
        public string Channel { get; set; }
        public string Warehouse { get; set; }
        public string Cluster { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string SkCode { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public string BeatCustomer { get; set; }
        public int OrderCount { get; set; }
    }



    public class ExecutiveAttendanceDetailDc
    {
        public long Id { get; set; }
        public int ExecutiveId { get; set; }
        public string Day { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public string WarehouseIds { get; set; }
        public string StoreIds { get; set; }
        public string ClusterIds { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public bool IsLate { get; set; }
        public bool ConfigIsBeatCheckIn { get; set; }
        public bool ConfigIsBeatCheckOut { get; set; }
        public bool ConfigIsBeatTADA { get; set; }
        public int ConfigFullDayMinVisitCount { get; set; }
        public int ConfigTADAMinProductiveCallCount { get; set; }
        public TimeSpan? ConfigFirstCheckInTime { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public int StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long ConfigId { get; set; }
        public long ChannelMasterId { get; set; }
    }

    public class ExecutiveAttendanceReportDC
    {
        public int ExecutiveId { get; set; }
        public string EmpCode { get; set; }
        public string ExecutiveName { get; set; }
        public string Mobile { get; set; }
        public int WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int ClusterId { get; set; }
        public int CityId { get; set; } //new add
        public string CityName { get; set; }
        public string Warehouse { get; set; }
        public string ChannelName { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public int TotalWorkingDays { get; set; }
        public double PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int TADA { get; set; }
        public long TotalRecords { get; set; }
    }
    public class ExecutiveAttendanceReportDCs
    {
        public long TotalRecords { get; set; }
        public List<ExecutiveAttendanceReportDC> executiveAttendanceLogDCs { get; set; }
    }
    public class ExecutiveStoreClusterDC
    {
        public int ExecutiveId { get; set; }
        public int ClusterId { get; set; }
        public int StoreId { get; set; }
        public int? CityId { get; set; }
        public int WarehouseId { get; set; }
    }

    public class ProductiveCallDc
    {
        public int ProductiveCall { get; set; }
        public int ExtraCall { get; set; }
        public int TotalProductiveCall { get; set; }
    }
    public class ExportExecutiveAttendanceDC
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public string Empcode { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
    }

    public class ExecutiveAttendanceMonthReportDC
    {
        public int ExecutiveId { get; set; }
        public string Day { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsLate { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public string Description { get; set; }
    }
    public class ExecutiveAttendanceReportDateWiseDC
    {
        public string EmpCode { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime Date { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public string CityName { get; set; }
        public string ChannelName { get; set; }
        public string Day { get; set; }
        public DateTime? Check_In { get; set; }
        public DateTime? Check_Out { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string TADA { get; set; }
        public string Attendance { get; set; }
        public string FullDay_HalfDay { get; set; }
    }
    public class SalesTargetDashboardReportDC
    {
        public string Salesperson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public int Target { get; set; }
        public int IncentiveAmount { get; set; }
        public string SalesKpi { get; set; }
    }
    public class ExecutiveDataList
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Empcode { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
    }
    #endregion
}
