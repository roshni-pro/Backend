using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class CustomerExecutiveMappingDc
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public int? Beat { get; set; }
        public string Day { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? SkipDays { get; set; }
        public int? SkipWeeks { get; set; }
        public string EvenOrOddWeek { get; set; }
    }
   
    public class OldExectuiveListDC
    {
        public long OldExecutiveId { get; set; }
        public string OldExecutiveName { get; set; }
        public string OldExecutiveEmpCode { get; set; }
        public long OldStoreId { get; set; }
        public long OldClusterId { get; set; }
    }
    public class MappedCustomerOnClusterDc
    {
        public int? BeatNumber { get; set; }
        public string Day { get; set; }
        public string Skcode { get; set; }
        public int? SkipDays { get; set; }
        public int? SkipWeeks { get; set; }
        public string EvenOrOddWeek { get; set; }
    }
    public class SearchMappedExeOnClusterDc
    {
        public int ExecutiveId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
        public long StoreId { get; set; }
    }

    public class SearchMappedStoreClusterDc
    {
        public List<int> clusterIds { get; set; }
        public long StoreId { get; set; }
    }


    public class LatestBeatReportDc
    {
        public string SkCode { get; set; }
        public string Day { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
    }

    public class BeatReportPostDc
    {
        public int PeopleId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> StoreIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
    }
    public class OldBeatReportPostDc
    {
        public List<int> PeopleId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
        public int CurrentExecutiveId { get; set; }
        public int StoreIds { get; set; }
        public int WarehouseId { get; set; }
    }



}
