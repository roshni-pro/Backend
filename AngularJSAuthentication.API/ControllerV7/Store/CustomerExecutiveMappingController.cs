using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model.Store;
using AgileObjects.AgileMapper;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.Model;
using System.Globalization;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model.SalesApp;

namespace AngularJSAuthentication.API.ControllerV7.Store
{
    [RoutePrefix("api/CustomerExecutiveMapping")]
    public class CustomerExecutiveMappingController : ApiController
    {
        [Route("")]
        [HttpGet]
        public List<CustomerExecutiveMappingDc> Get(int CustomerId)
        {
            List<CustomerExecutiveMappingDc> result = new List<CustomerExecutiveMappingDc>();
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "CustomerId",
                    Value = CustomerId
                };
                result = context.Database.SqlQuery<CustomerExecutiveMappingDc>("exec Get_CustomerExecutiveMapping @CustomerId", idParam).ToList();
            }
            return result;
        }

        //[Route("")]
        //[HttpPost]
        //public string Insert(CustomerExecutiveMappingDc obj)
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    string result = "something went wrong";
        //    using (var context = new AuthContext())
        //    {
        //        CustomerExecutiveMapping item = new CustomerExecutiveMapping();
        //        var found = context.CustomerExecutiveMappings.Where(x => x.CustomerId == obj.CustomerId && x.ExecutiveId == obj.ExecutiveId && x.Day == obj.Day && x.IsDeleted == false).FirstOrDefault();
        //        if (found == null)
        //        {
        //            item.ExecutiveId = obj.ExecutiveId;
        //            item.CustomerId = obj.CustomerId;
        //            item.Day = obj.Day;
        //            item.Beat = obj.Beat;
        //            item.CreatedBy = userid;
        //            item.CreatedDate = DateTime.Now;
        //            item.ModifiedBy = userid;
        //            item.ModifiedDate = DateTime.Now;
        //            item.IsActive = true;
        //            item.IsDeleted = false;
        //            context.CustomerExecutiveMappings.Add(item);
        //            context.Commit();
        //            result = "Added Successfully";
        //        }
        //        else
        //        {
        //            result = "Already Mapped";
        //        }
        //    }
        //    return result;
        //}

        //[Route("")]
        //[HttpPut]
        //public string Update(CustomerExecutiveMappingDc obj)
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    string result = "something went wrong";

        //    using (var context = new AuthContext())
        //    {
        //        CustomerExecutiveMappingDc item = new CustomerExecutiveMappingDc();

        //        var update = context.CustomerExecutiveMappings.Where(x => x.Id == obj.Id && x.IsDeleted == false).FirstOrDefault();
        //        var IsAlreayMapped = context.CustomerExecutiveMappings.Any(x => x.CustomerId == obj.CustomerId && x.Id != obj.Id && x.Day == obj.Day && x.ExecutiveId == obj.ExecutiveId && x.IsDeleted == false);
        //        if (update != null && !IsAlreayMapped)
        //        {
        //            update.ExecutiveId = obj.ExecutiveId;
        //            update.Day = obj.Day;
        //            update.Beat = obj.Beat;
        //            update.ModifiedBy = userid;
        //            update.ModifiedDate = DateTime.Now;
        //            context.Entry(update).State = EntityState.Modified;
        //            context.Commit();
        //            result = "Updated Successfully";
        //        }
        //        else if (IsAlreayMapped)
        //        {
        //            result = "Already Mapped";

        //        }
        //        else
        //        {
        //            result = "Not exists";
        //        }
        //    }
        //    return result;
        //}

        //[Route("")]
        //[HttpDelete]
        //public string Delete(long Id)
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    string result = "something went wrong";

        //    using (var context = new AuthContext())
        //    {
        //        CustomerExecutiveMapping item = new CustomerExecutiveMapping();

        //        var update = context.CustomerExecutiveMappings.Where(x => x.Id == Id).FirstOrDefault();
        //        if (update != null)
        //        {
        //            update.IsDeleted = true;
        //            update.ModifiedBy = userid;
        //            update.ModifiedDate = DateTime.Now;
        //            context.Entry(update).State = EntityState.Modified;
        //            context.Commit();
        //            result = "Deleted Successfully";
        //        }
        //        else
        //        {
        //            result = "Not exists";
        //        }
        //    }
        //    return result;
        //}

        //[Route("AssignBeat")]
        //[HttpPost]
        //public string AssignBeat(List<ClusterExecutiveBeat> beatList)
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    string result = "";

        //    if (beatList != null && beatList.Any())
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            foreach (var beat in beatList)
        //            {
        //                if (!string.IsNullOrEmpty(beat.Skcode) && !string.IsNullOrEmpty(beat.Day))
        //                {
        //                    int customerId = context.Customers.Where(x => !string.IsNullOrEmpty(x.Skcode) && x.Skcode.Trim().ToLower() == beat.Skcode.ToLower() && x.Warehouseid == beat.WarehouseId && x.ClusterId == beat.ClusterId).Select(x => x.CustomerId).FirstOrDefault();
        //                    if (customerId > 0)
        //                    {
        //                        var found = context.CustomerExecutiveMappings.Where(x => x.CustomerId == customerId && x.IsDeleted == false && x.IsActive == true && !string.IsNullOrEmpty(x.Day) && x.Day.Trim().ToLower() == beat.Day.ToLower()).FirstOrDefault();

        //                        if (found == null)
        //                        {
        //                            CustomerExecutiveMapping mapping = new CustomerExecutiveMapping();
        //                            mapping.ExecutiveId = beat.ExecutiveId;
        //                            mapping.CustomerId = customerId;
        //                            mapping.Day = beat.Day;
        //                            mapping.Beat = beat.BeatNumber;
        //                            mapping.CreatedBy = userid;
        //                            mapping.CreatedDate = DateTime.Now;
        //                            mapping.ModifiedBy = userid;
        //                            mapping.ModifiedDate = DateTime.Now;
        //                            mapping.IsActive = true;
        //                            mapping.IsDeleted = false;
        //                            context.CustomerExecutiveMappings.Add(mapping);
        //                            context.Commit();
        //                        }
        //                        else
        //                        {
        //                            found.IsDeleted = true;
        //                            context.Entry(found).State = EntityState.Modified;
        //                            CustomerExecutiveMapping mapping = new CustomerExecutiveMapping();
        //                            mapping.ExecutiveId = beat.ExecutiveId;
        //                            mapping.CustomerId = customerId;
        //                            mapping.Day = beat.Day;
        //                            mapping.Beat = beat.BeatNumber;
        //                            mapping.CreatedBy = userid;
        //                            mapping.CreatedDate = DateTime.Now;
        //                            mapping.ModifiedBy = userid;
        //                            mapping.ModifiedDate = DateTime.Now;
        //                            mapping.IsActive = true;
        //                            mapping.IsDeleted = false;
        //                            context.CustomerExecutiveMappings.Add(mapping);
        //                            context.Commit();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        result += ", " + beat.Skcode;
        //                    }


        //                }

        //            }
        //        }
        //    }


        //    return result;
        //}

        //[Route("CustomerList/{executiveId}")]
        //[HttpGet]
        //public IHttpActionResult GetCustomerList(int executiveId)
        //{

        //    using (var context = new AuthContext())
        //    {
        //        var query = from cem in context.CustomerExecutiveMappings
        //                    join cus in context.Customers
        //                    on cem.CustomerId equals cus.CustomerId
        //                    where cem.ExecutiveId == executiveId
        //                    && cem.IsActive == true
        //                    && cem.IsDeleted == false
        //                    select new ExecutiveCustomer
        //                    {
        //                        CustomerId = cus.CustomerId,
        //                        SkCode = cus.Skcode,
        //                        Name = cus.Name,
        //                        ShopName = cus.ShopName
        //                    };

        //        var list = query.ToList();
        //        return Ok(list);
        //    }

        //}

        [Route("MappedCustomerOnCluster")]
        [HttpPost]
        public async Task<List<MappedCustomerOnClusterDc>> GetMappedCustomerListOnCluster(SearchMappedExeOnClusterDc SearchMappedExeOnCluster)
        {
            List<MappedCustomerOnClusterDc> result = new List<MappedCustomerOnClusterDc>();
            if (SearchMappedExeOnCluster != null && SearchMappedExeOnCluster.clusterIds.Any() && SearchMappedExeOnCluster.ExecutiveId > 0)
            {
                using (var context = new AuthContext())
                {
                    var clusterIdList = new DataTable();
                    clusterIdList.Columns.Add("IntValue");
                    foreach (var item in SearchMappedExeOnCluster.clusterIds)
                    {
                        var dr = clusterIdList.NewRow();
                        dr["IntValue"] = item;
                        clusterIdList.Rows.Add(dr);
                    }
                    var clIds = new SqlParameter("clusterIds", clusterIdList);
                    clIds.SqlDbType = SqlDbType.Structured;
                    clIds.TypeName = "dbo.IntValues";

                    var channelList = new DataTable();
                    channelList.Columns.Add("IntValue");
                    foreach (var item in SearchMappedExeOnCluster.ChannelMasterIds)
                    {
                        var dr = channelList.NewRow();
                        dr["IntValue"] = item;
                        channelList.Rows.Add(dr);
                    }
                    var ChannelIds = new SqlParameter("ChannelMasterIds", channelList);
                    ChannelIds.SqlDbType = SqlDbType.Structured;
                    ChannelIds.TypeName = "dbo.IntValues";

                    var ExecutiveId = new SqlParameter("ExecutiveId", SearchMappedExeOnCluster.ExecutiveId);
                    var StoreId = new SqlParameter("StoreId", SearchMappedExeOnCluster.StoreId);
                    result = await context.Database.SqlQuery<MappedCustomerOnClusterDc>("exec GetMappedCustomerOnCluster @ExecutiveId, @clusterIds,@ChannelMasterIds , @StoreId", ExecutiveId, clIds, ChannelIds, StoreId).ToListAsync();
                }
            }
            return result;
        }


        [Route("MappedCustomerOnStoreCluster")]
        [HttpPost]
        public async Task<List<MappedCustomerOnClusterDc>> GetMappedCustomerListOnStoreCluster(SearchMappedStoreClusterDc SearchMappedStoreCluster)
        {
            List<MappedCustomerOnClusterDc> result = new List<MappedCustomerOnClusterDc>();
            if (SearchMappedStoreCluster != null && SearchMappedStoreCluster.clusterIds.Any() && SearchMappedStoreCluster.StoreId > 0)
            {
                using (var context = new AuthContext())
                {
                    var clusterIdList = new DataTable();
                    clusterIdList.Columns.Add("IntValue");
                    foreach (var item in SearchMappedStoreCluster.clusterIds)
                    {
                        var dr = clusterIdList.NewRow();
                        dr["IntValue"] = item;
                        clusterIdList.Rows.Add(dr);
                    }
                    var clIds = new SqlParameter("clusterIds", clusterIdList);
                    clIds.SqlDbType = SqlDbType.Structured;
                    clIds.TypeName = "dbo.IntValues";

                    var StoreId = new SqlParameter("StoreId", SearchMappedStoreCluster.StoreId);
                    result = await context.Database.SqlQuery<MappedCustomerOnClusterDc>("exec GetMappedCustomerOnStoreCluster @clusterIds , @StoreId", clIds, StoreId).ToListAsync();
                }
            }
            return result;
        }


        //[Route("AssignBeat")]
        //[HttpPost]
        //public async Task<string> AssignBeat(ValidatingAssignBeatDc beatList)
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    string result = "";

        //    var ExecutiveId = beatList.ClusterExecutiveBeat.Where(x => x.ExecutiveId > 0).Select(x => x.ExecutiveId).FirstOrDefault();
        //    if (beatList != null && beatList.ClusterExecutiveBeat.Any() && beatList.clusterIds.Any() && userid > 0)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            var CustomerSkcode = beatList.ClusterExecutiveBeat.Select(x => x.Skcode).Distinct().ToList();
        //            var CustomerList = await context.Customers.Where(x => CustomerSkcode.Contains(x.Skcode)).ToListAsync();
        //            var CustNotExitsinCluster = CustomerList.Where(x => !beatList.clusterIds.Contains(x.ClusterId)).Select(x => x.Skcode).ToList();
        //            if (CustNotExitsinCluster != null && CustNotExitsinCluster.Any())
        //            {
        //                return result = string.Join(",", CustNotExitsinCluster);
        //            }
        //            var CustomeridList = CustomerList.Select(x => x.CustomerId).Distinct().ToList();
        //            var CustomerMappingList = await context.CustomerExecutiveMappings.Where(x => CustomeridList.Contains(x.CustomerId) && x.IsDeleted == false && x.IsActive == true).ToListAsync();
        //            List<CustomerExecutiveMapping> AddList = new List<CustomerExecutiveMapping>();
        //            List<CustomerExecutiveMapping> UpdateList = new List<CustomerExecutiveMapping>();
        //            foreach (var beat in beatList.ClusterExecutiveBeat)
        //            {
        //                if (!string.IsNullOrEmpty(beat.Skcode) && !string.IsNullOrEmpty(beat.Day))
        //                {
        //                    var Cust = CustomerList.Where(x => x.Skcode == beat.Skcode).FirstOrDefault();
        //                    if (Cust != null)
        //                    {
        //                        var found = new CustomerExecutiveMapping();
        //                        if (CustomerMappingList.Any(x => x.CustomerId == Cust.CustomerId && x.ExecutiveId == ExecutiveId && x.Day == null) && CustomerMappingList.Count(x => x.CustomerId == Cust.CustomerId && x.ExecutiveId == ExecutiveId) == 1)
        //                        {
        //                            found = CustomerMappingList.Where(x => x.CustomerId == Cust.CustomerId && x.ExecutiveId == ExecutiveId && x.Day == null).FirstOrDefault();

        //                        }
        //                        else
        //                        {
        //                            found = CustomerMappingList.Where(x => x.CustomerId == Cust.CustomerId && x.ExecutiveId == ExecutiveId && x.Day.Trim().ToLower() == beat.Day.ToLower()).FirstOrDefault();
        //                        }
        //                        if (found == null)
        //                        {
        //                            bool alreadyAdded = AddList.Any(x => x.CustomerId == Cust.CustomerId && x.Day.Trim().ToLower() == beat.Day.ToLower());
        //                            if (!alreadyAdded)
        //                            {
        //                                CustomerExecutiveMapping mapping = new CustomerExecutiveMapping();
        //                                mapping.ExecutiveId = ExecutiveId;
        //                                mapping.CustomerId = Cust.CustomerId;
        //                                mapping.Day = beat.Day;
        //                                mapping.Beat = beat.BeatNumber;
        //                                mapping.CreatedBy = userid;
        //                                mapping.CreatedDate = DateTime.Now;
        //                                mapping.ModifiedBy = userid;
        //                                mapping.ModifiedDate = DateTime.Now;
        //                                mapping.IsActive = true;
        //                                mapping.IsDeleted = false;
        //                                AddList.Add(mapping);
        //                            }
        //                        }
        //                        else if (found.Beat != beat.BeatNumber)
        //                        {
        //                            if (found.Day == null)
        //                            {
        //                                found.Day = beat.Day;
        //                            }
        //                            found.Beat = beat.BeatNumber;
        //                            bool alreadyAdded = UpdateList.Any(x => x.Id == found.Id);
        //                            if (!alreadyAdded)
        //                            {
        //                                UpdateList.Add(found);
        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //            if (AddList != null && AddList.Any())
        //            {
        //                context.CustomerExecutiveMappings.AddRange(AddList);
        //            }
        //            if (UpdateList != null && UpdateList.Any())
        //            {
        //                foreach (var item in UpdateList)
        //                {
        //                    item.ModifiedBy = userid;
        //                    context.Entry(item).State = EntityState.Modified;
        //                }
        //            }
        //            if ((AddList != null && AddList.Any()) || (UpdateList != null && UpdateList.Any()))
        //            {
        //                if (context.Commit() > 0)
        //                {
        //                    result = "Updated Successfully";

        //                }
        //                else
        //                {
        //                    result = "Something Went wrong";
        //                }
        //            }
        //            else
        //            {
        //                result = "There is no record to update or already updated ";

        //            }
        //        }
        //    }
        //    return result;
        //}


        [Route("ValidatingAssignBeat")]
        [HttpPost]
        public async Task<string> ValidatingAssignBeat(ValidatingAssignBeatDc ValidatingAssignBeat)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            if (ValidatingAssignBeat != null && ValidatingAssignBeat.ClusterExecutiveBeat.Any() && ValidatingAssignBeat.clusterIds.Any() && ValidatingAssignBeat.ChannelMasterIds.Any() && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var CustomerSkcode = ValidatingAssignBeat.ClusterExecutiveBeat.Select(x => x.Skcode).Distinct().ToList();
                    List<Customer> CustomerList = await context.Customers.Where(x => CustomerSkcode.Contains(x.Skcode)).ToListAsync();
                    var ClusterIds = ValidatingAssignBeat.clusterIds;

                    if (CustomerList != null && CustomerList.Any() && ClusterIds.Any())
                    {
                        var CustNotExitsinCluster = CustomerList.Where(x => !ClusterIds.Contains(x.ClusterId) ).Select(x => x.Skcode).ToList();
                        result = string.Join(",", CustNotExitsinCluster);
                    }
                }
            }
            return result;
        }

        [Route("AssignBeatFromUploader")]
        [HttpPost]
        public async Task<string> AssignBeatFromUploader(DataContracts.External.MobileExecutiveDC.ValidatingAssignBeatDc beatList)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            if (beatList != null && beatList.ClusterExecutiveBeat.Any() && beatList.clusterIds.Any() && userid > 0 && beatList.ClusterExecutiveBeat.Any(x => x.StoreId > 0))
            {
                using (var context = new AuthContext())
                {
                    var CustomerSkcode = beatList.ClusterExecutiveBeat.Select(x => x.Skcode).Distinct().ToList();
                    var StoreIds = beatList.ClusterExecutiveBeat.Select(x => x.StoreId).Distinct().ToList();
                    var CustomerList = await context.Customers.Where(x => CustomerSkcode.Contains(x.Skcode)).ToListAsync();
                    var CustNotExitsinCluster = CustomerList.Where(x => !beatList.clusterIds.Contains(x.ClusterId)).Select(x => x.Skcode).ToList();
                    if (CustNotExitsinCluster != null && CustNotExitsinCluster.Any())
                    {
                        return result = string.Join(",", CustNotExitsinCluster);
                    }
                    var ExecutiveId = beatList.ClusterExecutiveBeat.Select(x => x.ExecutiveId).FirstOrDefault();
                    var storelist = context.ClusterStoreExecutives.Where(x => x.IsActive == true && x.IsDeleted == false && x.ExecutiveId == ExecutiveId).Select(x => x.StoreId).Distinct().ToList();
                    var CustomeridList = CustomerList.Select(x => x.CustomerId).Distinct().ToList();
                    var CustomerMappingList = await context.CustomerExecutiveMappings.Where(x => CustomeridList.Contains(x.CustomerId) && storelist.Contains(x.StoreId) && x.IsDeleted == false && x.IsActive == true).ToListAsync();
                    List<CustomerExecutiveMapping> AddList = new List<CustomerExecutiveMapping>();
                    List<CustomerExecutiveMapping> UpdateList = new List<CustomerExecutiveMapping>();

                    #region TODO:SalesAppMarch2023
                    //foreach (var store in storelist)
                    //{
                    //    foreach (var beat in beatList.ClusterExecutiveBeat)
                    //    {
                    //        if (!string.IsNullOrEmpty(beat.Skcode) && CustomerList.Any(x => x.Skcode == beat.Skcode))
                    //        {
                    //            var CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId;
                    //            if (AddList.Count() > 0 )
                    //            {
                    //                var isExist = AddList.Any(x => x.CustomerId == CustomerId && x.Day == beat.Day && x.StoreId == store);
                    //                if (isExist)
                    //                {
                    //                    continue;
                    //                }
                    //            }

                    //            if (UpdateList.Count() > 0)
                    //            {
                    //                var isExist = UpdateList.Any(x => x.CustomerId == CustomerId && x.Day == beat.Day && x.StoreId == store);
                    //                if (isExist)
                    //                {
                    //                    continue;
                    //                }
                    //            }

                    //            DayOfWeek Day = readDayOfWeek(beat.Day);
                    //            CustomerExecutiveMapping AddBeat = new CustomerExecutiveMapping();
                    //            DateTime startdate = DateTime.Now;

                    //            if (beat.SkipDays == 0 && beat.SkipWeeks == 0 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                    //                startdate = CalculateDate(Day, DateTime.Now);
                    //            else if (beat.SkipDays == 0 && beat.SkipWeeks >= 1 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                    //                startdate = CalculateDate(Day, DateTime.Now.AddDays((beat.SkipWeeks) * 7 + 7));
                    //            else if (beat.SkipDays >= 1 && beat.SkipWeeks == 0 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                    //                startdate = CalculateDate(Day, DateTime.Now.AddDays((beat.SkipDays) * 1 + 1));
                    //            else if (!string.IsNullOrEmpty(beat.EvenOrOddWeek))
                    //            {
                    //                CultureInfo ciCurr = CultureInfo.CurrentCulture;
                    //                int weekNum = ciCurr.Calendar.GetWeekOfYear(startdate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    //                //int weekNum = GetWeekOfMonth(dt);
                    //                if (beat.EvenOrOddWeek.Trim().ToLower() == "odd")
                    //                {
                    //                    int addDays = weekNum % 2 != 0 ? 0 : 7;
                    //                    beat.SkipDays = 14;
                    //                    startdate = DateTime.Now.AddDays(addDays);
                    //                    startdate = CalculateDate(Day, startdate);
                    //                }
                    //                else if (beat.EvenOrOddWeek.Trim().ToLower() == "even")
                    //                {
                    //                    int addDays = weekNum % 2 == 0 ? 0 : 7;
                    //                    beat.SkipDays = 14;
                    //                    startdate = DateTime.Now.AddDays(addDays);
                    //                    startdate = CalculateDate(Day, startdate);
                    //                }
                    //            }


                    //            if (CustomerMappingList.Any(x => x.CustomerId == CustomerId && x.Day == beat.Day && x.StoreId == store))
                    //            {
                    //                var data = CustomerMappingList.Where(x => x.StoreId == store && x.Day == beat.Day && x.CustomerId == CustomerId).FirstOrDefault();
                    //                data.SkipDays = beat.SkipDays;
                    //                data.SkipWeeks = beat.SkipWeeks;
                    //                data.StartDate = startdate;
                    //                data.EvenOrOddWeek = beat.EvenOrOddWeek;
                    //                data.ModifiedBy = userid;
                    //                data.ModifiedDate = DateTime.Now;
                    //                UpdateList.Add(data);
                    //            }
                    //            else
                    //            {
                    //                AddList.Add(new CustomerExecutiveMapping
                    //                {
                    //                    CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId,
                    //                    StoreId = store,
                    //                    SkipDays = beat.SkipDays,
                    //                    SkipWeeks = beat.SkipWeeks,
                    //                    StartDate = startdate,
                    //                    Day = beat.Day ?? "NoBeat",
                    //                    Beat = beat.BeatNumber,
                    //                    CreatedBy = userid,
                    //                    CreatedDate = DateTime.Now,
                    //                    ModifiedBy = userid,
                    //                    ModifiedDate = DateTime.Now,
                    //                    IsActive = true,
                    //                    IsDeleted = false,
                    //                });
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion TODO:SalesAppMarch2023
                    if (CustomerMappingList.Any() && CustomerMappingList != null)
                    {
                        CustomerMappingList.ForEach(i =>
                        {
                            i.IsActive = false;
                            i.IsDeleted = true;
                            i.ModifiedBy = userid;
                            i.ModifiedDate = DateTime.Now;
                            context.Entry(i).State = EntityState.Modified;
                        });
                    }
                    foreach (var store in storelist)
                    {
                        foreach (var beat in beatList.ClusterExecutiveBeat)
                        {
                            if (!string.IsNullOrEmpty(beat.Skcode) && CustomerList.Any(x => x.Skcode == beat.Skcode))
                            {
                                var CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId;
                                DayOfWeek Day = readDayOfWeek(beat.Day);
                                CustomerExecutiveMapping AddBeat = new CustomerExecutiveMapping();
                                DateTime startdate = DateTime.Now;

                                //if (beat.SkipDays == 0 && beat.SkipWeeks == 0 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                //    startdate = CalculateDate(Day, DateTime.Now);
                                //else 
                                if (beat.SkipDays == 28 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                {
                                    beat.SkipWeeks = 0;
                                    startdate = CalculateDate(Day, DateTime.Now.AddDays((beat.MonthWeek > 1 ? beat.MonthWeek - 1 : 0) * 7));
                                }
                                else if (beat.SkipDays >= 1 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                {
                                    beat.SkipWeeks = 0;
                                    startdate = CalculateDate(Day, DateTime.Now.AddDays((beat.SkipDays) * 1 + 1));
                                }
                                else if (beat.SkipWeeks >= 1 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                {
                                    beat.SkipDays = 0;
                                    startdate = CalculateDate(Day, DateTime.Now.AddDays(beat.SkipWeeks > 1 ? (beat.SkipWeeks - 1) * 7 : 0));
                                }
                                else if (!string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                {
                                    CultureInfo ciCurr = CultureInfo.CurrentCulture;
                                    int weekNum = ciCurr.Calendar.GetWeekOfYear(startdate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                    if (beat.EvenOrOddWeek.Trim().ToLower() == "odd")
                                    {
                                        int addDays = weekNum % 2 != 0 ? 0 : 7;
                                        beat.SkipDays = 14;
                                        beat.SkipWeeks = 0;
                                        startdate = DateTime.Now.AddDays(addDays);
                                        startdate = CalculateDate(Day, startdate);
                                    }
                                    else if (beat.EvenOrOddWeek.Trim().ToLower() == "even")
                                    {
                                        int addDays = weekNum % 2 == 0 ? 0 : 7;
                                        beat.SkipDays = 14;
                                        beat.SkipWeeks = 0;
                                        startdate = DateTime.Now.AddDays(addDays);
                                        startdate = CalculateDate(Day, startdate);
                                    }
                                }
                                else
                                {
                                    startdate = CalculateDate(Day, DateTime.Now);
                                }

                                //if (beat.SkipDays == 28 && string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                //{
                                //    startdate = CalculateDate(Day, DateTime.Now.AddDays((beat.MonthWeek > 1 ? beat.MonthWeek - 1 : 0) * 7));

                                //}
                                //else
                                //{
                                //    startdate = CalculateDate(Day, DateTime.Now);
                                //}

                                var NewAddList = AddList.FirstOrDefault(x => x.CustomerId == CustomerId && x.Day == beat.Day && x.StoreId == store);
                                if (NewAddList != null)
                                {
                                    NewAddList.SkipDays = beat.SkipDays;
                                    NewAddList.SkipWeeks = beat.SkipWeeks;
                                    NewAddList.EvenOrOddWeek = beat.EvenOrOddWeek;
                                    NewAddList.MonthWeek = beat.MonthWeek;
                                    NewAddList.StartDate = startdate;
                                }
                                else
                                {
                                    AddList.Add(new CustomerExecutiveMapping
                                    {
                                        CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId,
                                        StoreId = store,
                                        SkipDays = beat.SkipDays,
                                        SkipWeeks = beat.SkipWeeks,
                                        StartDate = startdate,
                                        Day = beat.Day ?? "NoBeat",
                                        Beat = beat.BeatNumber,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        ModifiedBy = userid,
                                        ModifiedDate = DateTime.Now,
                                        IsActive = true,
                                        EvenOrOddWeek = beat.EvenOrOddWeek,
                                        MonthWeek = beat.MonthWeek,
                                        IsDeleted = false,
                                    });
                                }
                            }
                        }
                    }
                    if (AddList.Any() || UpdateList.Any())
                    {
                        if (AddList != null && AddList.Any())
                        {
                            context.CustomerExecutiveMappings.AddRange(AddList);
                        }
                        if (UpdateList != null && UpdateList.Any())
                        {
                            foreach (var item in UpdateList)
                            {
                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        if (context.Commit() > 0) { result = "Updated Successfully"; } else { result = "Something Went wrong"; }
                    }
                    else
                    {
                        result = "There is no record to update";
                    }

                }
            }
            return result;
        }

        private static DateTime CalculateDate(DayOfWeek dayOfWeek, DateTime date)
        {
            if (date.DayOfWeek != dayOfWeek)
            {
                var direction = date.DayOfWeek > dayOfWeek ? -1D : 1D;
                do
                {
                    date = date.AddDays(direction);
                } while (date.DayOfWeek != dayOfWeek);
            }
            return date;
        }
        public static DayOfWeek readDayOfWeek(string Day)
        {
            switch (Day)
            {
                case "Sunday":
                    return DayOfWeek.Sunday;
                case "Monday":
                    return DayOfWeek.Monday;
                case "Tuesday":
                    return DayOfWeek.Tuesday;
                case "Wednesday":
                    return DayOfWeek.Wednesday;
                case "Thursday":
                    return DayOfWeek.Thursday;
                case "Friday":
                    return DayOfWeek.Friday;
                case "Saturday":
                    return DayOfWeek.Saturday;
            }
            return DayOfWeek.Monday;
        }

        [Route("StoresOfMappedExecutive/{PeopleId}")]
        [HttpGet]
        public async Task<List<StoreViewModel>> GetStoreList(int PeopleId)
        {
            using (var context = new AuthContext())
            {
                List<StoreViewModel> result = new List<StoreViewModel>();
                var ExecutiveId = new SqlParameter("ExecutiveId", PeopleId);
                result = await context.Database.SqlQuery<StoreViewModel>("exec StoresOfMappedExecutive @ExecutiveId", ExecutiveId).ToListAsync();
                return result;
            }
        }
        public static int GetWeekOfMonth(DateTime date)
        {
            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }

        [Route("GetLatestBeatReport")]
        [HttpPost]
        public async Task<List<LatestBeatReportDc>> GetLatestBeatReport(BeatReportPostDc obj)
        {

            using (var authContext = new AuthContext())
            {
                var storeids = new System.Data.DataTable();
                var clusterids = new System.Data.DataTable();
                var channelids = new System.Data.DataTable();

                clusterids.Columns.Add("IntValue");
                storeids.Columns.Add("IntValue");
                channelids.Columns.Add("IntValue");

                foreach (var item in obj.clusterIds)
                {
                    var dr = clusterids.NewRow();
                    dr["IntValue"] = item;
                    clusterids.Rows.Add(dr);
                }
                var ClusterIds = new SqlParameter("MultiClusterId", clusterids);
                ClusterIds.SqlDbType = System.Data.SqlDbType.Structured;
                ClusterIds.TypeName = "dbo.IntValues";

                foreach (var item in obj.StoreIds)
                {
                    var dr = storeids.NewRow();
                    dr["IntValue"] = item;
                    storeids.Rows.Add(dr);
                }

                var StoreIdParam = new SqlParameter("MultiStoreIds", storeids);
                StoreIdParam.SqlDbType = System.Data.SqlDbType.Structured;
                StoreIdParam.TypeName = "dbo.IntValues";


                foreach (var item in obj.ChannelMasterIds)
                {
                    var dr = channelids.NewRow();
                    dr["IntValue"] = item;
                    channelids.Rows.Add(dr);
                }

                var ChannelIdParam = new SqlParameter("MultiChannelIds", channelids);
                ChannelIdParam.SqlDbType = System.Data.SqlDbType.Structured;
                ChannelIdParam.TypeName = "dbo.IntValues";

                var peopleid = new SqlParameter("peopleid", obj.PeopleId);

                var BeatList = await authContext.Database.SqlQuery<LatestBeatReportDc>("exec GetLatestBeatEditReport  @peopleId,@MultiClusterId,@MultiStoreIds,@MultiChannelIds", peopleid, ClusterIds, StoreIdParam,ChannelIdParam).ToListAsync();
                return BeatList;
            }
        }

        //TODO:SalesAppMarch2023
        [Route("ResetEditedBeat")]
        [HttpPost]
        [AllowAnonymous]
        public APIResponse ResetEditedBeatAsync(ResetEditBeatDC resetEditBeatDC)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var executiveid = new SqlParameter("ExecutiveId", resetEditBeatDC.ExecutiveId);
                    var storeid = new SqlParameter("StoreId", resetEditBeatDC.StoreId);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("IntValue");
                    foreach (var data in resetEditBeatDC.ClusterIds)
                    {
                        var dr = dt.NewRow();
                        dr["IntValue"] = data;
                        dt.Rows.Add(dr);
                    }
                    var clusters = new SqlParameter
                    {
                        ParameterName = "ClusterIds",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = dt
                    };

                    var res = context.Database.ExecuteSqlCommand("ResetEditedBeat @ExecutiveId,@StoreId,@ClusterIds", executiveid, storeid, clusters);
                    return new APIResponse { Status = true, Message = "Data Reset Succefully" };

                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        [Route("ExecutiveBeatUpload")]
        [HttpPost]
        public async Task<string> ExecutiveBeatUpload(ValidatingAssignBeatDc beatList)
        {
            using (var context = new AuthContext())
            {
                string result = "";
                int userid = 0;
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var PeopleId = beatList.ClusterExecutiveBeat.Select(x => x.ExecutiveId).FirstOrDefault();

                var ClusterStoreExecutiveData = (from cs in context.ClusterStoreExecutives
                                                 join s in context.StoreDB
                                                 on cs.StoreId equals s.Id
                                                 join ch in context.ChannelMasters
                                                 on cs.ChannelMasterId equals ch.ChannelMasterId
                                                 join c in context.Clusters
                                                 on cs.ClusterId equals c.ClusterId
                                                 where cs.ExecutiveId == PeopleId &&
                                                 cs.IsActive == true && cs.IsDeleted == false && s.IsActive == true && s.IsDeleted == false
                                                 && ch.Active == true && ch.Deleted == false && c.Active == true && c.Deleted == false
                                                 select cs
                                                 ).ToList();

                var clusterId = ClusterStoreExecutiveData.Select(x => x.ClusterId).Distinct().ToList();
                var ChannelId = ClusterStoreExecutiveData.Select(x => x.ChannelMasterId).Distinct().ToList();

                var CustomerSkcode = beatList.ClusterExecutiveBeat.Select(x => x.Skcode).Distinct().ToList();
                var StoreIds = beatList.ClusterExecutiveBeat.Select(x => x.StoreId).Distinct().ToList();
                var CustomerList = await context.Customers.Where(x => CustomerSkcode.Contains(x.Skcode) && clusterId.Contains((int)x.ClusterId)).ToListAsync();

                var CustomeridList = CustomerList.Select(x => x.CustomerId).Distinct().ToList();
                List<CustomerExecutiveMappingsBeatEdit> AddList = new List<CustomerExecutiveMappingsBeatEdit>();
                List<CustomerExecutiveMappingsBeatEdit> UpdateList = new List<CustomerExecutiveMappingsBeatEdit>();

                
                foreach (var beat in beatList.ClusterExecutiveBeat)
                {
                    var CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId;
                    if (CustomerId > 0)
                    {
                        var editCust = context.CustomerExecutiveMappingsBeatEditDb.Where(x => x.ExecutiveId == beat.ExecutiveId && x.StoreId == beat.StoreId && x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (editCust != null)
                        {
                            editCust.Day = beat.Day ?? "NoBeat";
                            editCust.ModifiedBy = userid;
                            editCust.ModifiedDate = DateTime.Now;
                            editCust.StoreId = (int)beat.StoreId;
                            UpdateList.Add(editCust);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(beat.Skcode) && CustomerList.Any(x => x.Skcode == beat.Skcode))
                            {
                                DayOfWeek Day = readDayOfWeek(beat.Day);
                                CustomerExecutiveMappingsBeatEdit AddBeat = new CustomerExecutiveMappingsBeatEdit();

                                if (string.IsNullOrEmpty(beat.EvenOrOddWeek))
                                {
                                    AddList.Add(new CustomerExecutiveMappingsBeatEdit
                                    {
                                        CustomerId = CustomerList.FirstOrDefault(x => x.Skcode == beat.Skcode).CustomerId,
                                        ExecutiveId = beat.ExecutiveId,
                                        StoreId = (int)beat.StoreId,
                                        StartDate = CalculateDate(Day, DateTime.Now),
                                        Day = beat.Day ?? "NoBeat",
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        ModifiedBy = userid,
                                        ModifiedDate = DateTime.Now,
                                        IsActive = true,
                                        IsDeleted = false,
                                    });
                                }
                            }
                        }
                    }
                }
                if(UpdateList !=null && UpdateList.Any())
                {
                    foreach(var Editbeat in UpdateList)
                    {
                        context.Entry(Editbeat).State = EntityState.Modified;
                    }
                }
                if (AddList != null && AddList.Any())
                {
                    context.CustomerExecutiveMappingsBeatEditDb.AddRange(AddList);
                }
                if (context.Commit() > 0) { return result = "Updated Successfully"; } else {return  result = "Something Went wrong"; }
            }
        }

        //TODO:SalesAppMarch2023
        [Route("GetOldExecutiveLatestBeatReport")]
        [HttpPost]
        public async Task<List<LatestBeatReportDc>> GetOldExecutiveLatestBeatReport(OldBeatReportPostDc obj)
        {

            using (var authContext = new AuthContext())
            {
                var peopleids = new System.Data.DataTable();
                var clusterids = new System.Data.DataTable();

                clusterids.Columns.Add("IntValue");
                peopleids.Columns.Add("IntValue");

                foreach (var item in obj.clusterIds)
                {
                    var dr = clusterids.NewRow();
                    dr["IntValue"] = item;
                    clusterids.Rows.Add(dr);
                }
                var ClusterIds = new SqlParameter("MultiClusterId", clusterids);
                ClusterIds.SqlDbType = System.Data.SqlDbType.Structured;
                ClusterIds.TypeName = "dbo.IntValues";
                
                foreach (var item in obj.PeopleId)
                {
                    var dr = peopleids.NewRow();
                    dr["IntValue"] = item;
                    peopleids.Rows.Add(dr);
                }
                var peopleid = new SqlParameter("Peopleids", peopleids);
                peopleid.SqlDbType = System.Data.SqlDbType.Structured;
                peopleid.TypeName = "dbo.IntValues";

                var channelid = new SqlParameter("ChannelMasterId", obj.ChannelMasterIds[0]);
                var storeid = new SqlParameter("StoreIds", obj.StoreIds);
                var warehouseid = new SqlParameter("WarehouseId", obj.WarehouseId);
                var executiveid = new SqlParameter("CurrentExecutiveId", obj.CurrentExecutiveId);

                var BeatList = await authContext.Database.SqlQuery<LatestBeatReportDc>("exec GetOldExecLatestBeatEditReport  @Peopleids,@MultiClusterId,@StoreIds,@WarehouseId,@CurrentExecutiveId,@ChannelMasterId", peopleid, ClusterIds, storeid, warehouseid, executiveid, channelid).ToListAsync();
                return BeatList;
            }
        }

        //TODO:SalesAppMarch2023
        [Route("GetOldExecutiveById")]
        [HttpGet]
        public async Task<APIResponse> GetOldExecutiveByIdAsync(long Executiveid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var OldExecutiveLists = (from e in context.ExecutiveStoreChangeHistories
                                             join p in context.Peoples on e.OldExecutiveId equals p.PeopleID
                                             where p.Active == true && p.Deleted == false && e.IsActive == true && e.IsDeleted == false
                                             && e.ExecutiveId == Executiveid
                                             select new OldExectuiveListDC
                                             {
                                                 OldExecutiveId = e.OldExecutiveId,
                                                 OldExecutiveName = p.DisplayName,
                                                 OldExecutiveEmpCode = p.Empcode,
                                                 OldStoreId = e.OldStoreId,
                                                 OldClusterId = e.OldClusterId
                                             }).ToList();
                    return new APIResponse { Data = OldExecutiveLists ,Status =true};
                }
                catch(Exception ex)
                {
                    return new APIResponse { Status = false ,Message=ex.Message};
                }
            }
        }
    }
}
