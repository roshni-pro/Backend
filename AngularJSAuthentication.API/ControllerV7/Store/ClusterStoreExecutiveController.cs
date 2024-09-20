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
using System.Data;
using Dapper;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using System.Threading.Tasks;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;

namespace AngularJSAuthentication.API.ControllerV7.Store
{
    [RoutePrefix("api/ClusterStoreExecutive")]
    public class ClusterStoreExecutiveController : ApiController
    {

        //Get  Executive List ()
        [Route("")]
        [HttpGet]
        public List<ClusterStoreExecutiveDc> GetExecutiveList(int WarehouseId)
        {
            List<ClusterStoreExecutiveDc> result = new List<ClusterStoreExecutiveDc>();
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = WarehouseId
                };
                result = context.Database.SqlQuery<ClusterStoreExecutiveDc>("exec Store_GetDistinctClusterExecutiveByWarehouseId @WarehouseId", idParam).ToList();
            }
            return result;
        }
        [Route("ClusterExecutiveChannelId")]
        [HttpGet]
        [AllowAnonymous]
        public List<ClusterStoreExecutiveDc> GetExecutiveList(int WarehouseId, int ChannelMasterId)
        {
            List<ClusterStoreExecutiveDc> result = new List<ClusterStoreExecutiveDc>();
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = WarehouseId
                };
                var idParam1 = new SqlParameter
                {
                    ParameterName = "ChannelMasterId",
                    Value = ChannelMasterId
                };
                result = context.Database.SqlQuery<ClusterStoreExecutiveDc>("exec ClusterExecutiveByWarehouseIdandChannelId @WarehouseId,@ChannelMasterId", idParam, idParam1).ToList();
            }
            return result;
        }

        //Get Mapped Cluster Executive List 
        [Route("Mapped")]
        [HttpGet]
        public List<ClusterStoreExecutiveDc> MappedClusterExecutiveList(int ClusterId)
        {
            List<ClusterStoreExecutiveDc> result = new List<ClusterStoreExecutiveDc>();
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter
                {
                    ParameterName = "ClusterId",
                    Value = ClusterId
                };
                result = context.Database.SqlQuery<ClusterStoreExecutiveDc>("exec Get_MappedClusterExecutive @ClusterId", idParam).ToList();
            }
            return result;
        }


        //        [Route("")]
        //        [HttpPost]
        //        public string Insert(ClusterStoreExecutiveDc obj)
        //        {
        //            int userid = 0;
        //            var identity = User.Identity as ClaimsIdentity;
        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            string result = "something went wrong";
        //            using (var context = new AuthContext())
        //            {
        //                ClusterStoreExecutive item = new ClusterStoreExecutive();
        //                var found = context.ClusterStoreExecutives.Where(x => x.ClusterId == obj.ClusterId && x.StoreId == obj.StoreId && x.IsDeleted == false).FirstOrDefault();
        //                if (found == null)
        //                {
        //                    item.ExecutiveId = obj.ExecutiveId;
        //                    item.StoreId = obj.StoreId;
        //                    item.ClusterId = obj.ClusterId;
        //                    item.CreatedBy = userid;
        //                    item.CreatedDate = DateTime.Now;
        //                    item.ModifiedBy = userid;
        //                    item.ModifiedDate = DateTime.Now;
        //                    item.IsActive = true;
        //                    item.IsDeleted = false;
        //                    context.ClusterStoreExecutives.Add(item);
        //                    context.Commit();
        //                    result = "Added Successfully";
        //#if !DEBUG
        //            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
        //            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
        //#endif
        //                }
        //                else
        //                {
        //                    result = "Already Mapped";
        //                }
        //            }
        //            return result;
        //        }

        [Route("")]
        [HttpPost]
        public ClusterStoreResPonse Insert(ClusterStoreExecutiveDc obj)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            ClusterStoreExecutiveeDc found = new ClusterStoreExecutiveeDc();
            ClusterStoreResPonse Result = new ClusterStoreResPonse();
            using (var context = new AuthContext())
            {
                if (obj != null)
                {
                    ClusterStoreExecutive item = new ClusterStoreExecutive();
                    ClusterStoreExecutiveHistory ClusterStore = new ClusterStoreExecutiveHistory();
                    var idParam = new SqlParameter
                    {
                        ParameterName = "@ClusterId",
                        Value = obj.ClusterId
                    };
                    var storeidParam = new SqlParameter
                    {
                        ParameterName = "@StoreId",
                        Value = obj.StoreId
                    };
                    var channelidParam = new SqlParameter
                    {
                        ParameterName = "@ChannelId",
                        Value = obj.ChannelMasterId
                    };
                    //var executiveidParam = new SqlParameter
                    //{
                    //    ParameterName = "@ExecutiveId",
                    //    Value = obj.ExecutiveId
                    //};

                    found = context.Database.SqlQuery<ClusterStoreExecutiveeDc>("exec GetDatabyStoreandClusterid @ClusterId , @StoreId, @ChannelId", idParam, storeidParam, channelidParam).FirstOrDefault();
                    var Warehousid = context.Clusters.Where(x => x.ClusterId == obj.ClusterId && x.Active == true && x.Deleted == false).Select(x => x.WarehouseId).FirstOrDefault();
                    var Clusters = context.Clusters.Where(x => x.WarehouseId == Warehousid && x.Active == true && x.Deleted == false).Select(x => x.ClusterId).ToList();
                    var ExistingExecutiveChannelist = context.ClusterStoreExecutives.Where(x => Clusters.Contains(x.ClusterId) && x.IsActive == true && x.IsDeleted == false).ToList();

                    var ChannelData = ExistingExecutiveChannelist.Where(x => x.ChannelMasterId == obj.ChannelMasterId).FirstOrDefault();
                    var ExecutiveData = ExistingExecutiveChannelist.Where(x => x.ExecutiveId == obj.ExecutiveId).FirstOrDefault();

                    if (found != null && found.ClusterId == obj.ClusterId && found.StoreId == obj.StoreId
                             && (found.ExecutiveId != obj.ExecutiveId || found.ExecutiveId == obj.ExecutiveId) && found.ChannelMasterId == obj.ChannelMasterId)
                    {
                        Result.msg = "Already Channel Assigned";
                        Result.Status = false;
                        Result.ClusterStoreResPonseDc = null;
                    }
                    else if (ExecutiveData != null && (ExecutiveData.ClusterId == obj.ClusterId || ExecutiveData.ClusterId != obj.ClusterId)
                                        && (ExecutiveData.StoreId == obj.StoreId || ExecutiveData.StoreId != obj.StoreId)
                                        && ExecutiveData.ExecutiveId == obj.ExecutiveId && ExecutiveData.ChannelMasterId != obj.ChannelMasterId)
                    {
                        Result.msg = ExecutiveData.ClusterId != obj.ClusterId ? " Please assign same channeltype as previous assigned channelType." : "Already Assigned";
                        Result.Status = false;
                        Result.ClusterStoreResPonseDc = null;
                    }
                    else if (ChannelData != null && (ChannelData.ClusterId == obj.ClusterId || ChannelData.ClusterId != obj.ClusterId)
                        && (ChannelData.StoreId == obj.StoreId || ChannelData.StoreId != obj.StoreId)
                        && ChannelData.ExecutiveId == obj.ExecutiveId && ChannelData.ChannelMasterId != obj.ChannelMasterId)
                    {
                        Result.msg = "Please assign same channeltype as previous assigned channelType";
                        Result.Status = false;
                        Result.ClusterStoreResPonseDc = null;
                    }
                    //else if (ChannelData != null && (ChannelData.ClusterId == obj.ClusterId )
                    //     && (ChannelData.StoreId == obj.StoreId || ChannelData.StoreId != obj.StoreId)
                    //     && ChannelData.ExecutiveId != obj.ExecutiveId && ChannelData.ChannelMasterId == obj.ChannelMasterId)
                    //{
                    //    Result.msg = "Already Channel Assigned";
                    //    Result.Status = false;
                    //    Result.ClusterStoreResPonseDc = null;
                    //}
                    else
                    {
                        item.ExecutiveId = obj.ExecutiveId;
                        item.StoreId = obj.StoreId;
                        item.ClusterId = obj.ClusterId;
                        item.ChannelMasterId = obj.ChannelMasterId;
                        item.CreatedBy = userid;
                        item.CreatedDate = DateTime.Now;
                        item.ModifiedBy = userid;
                        item.ModifiedDate = DateTime.Now;
                        item.IsActive = true;
                        item.IsDeleted = false;
                        var data = context.ClusterStoreExecutives.Add(item);
                        context.Commit();


                        ClusterStore.ClusterId = obj.ClusterId;
                        ClusterStore.StoreId = obj.StoreId;
                        ClusterStore.ExecutiveId = obj.ExecutiveId;
                        ClusterStore.ChannelMasterId = obj.ChannelMasterId;
                        ClusterStore.ClusterStoreExecutiveId = item.Id;
                        ClusterStore.StartDate = DateTime.Now;
                        context.ClusterStoreExecutiveHistories.Add(ClusterStore);
                        context.Commit();
                        Result.msg = "Added Successfully";
                        Result.Status = true;
                        Result.ClusterStoreResPonseDc = found;
#if !DEBUG

            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
#endif

                    }
                }
                else
                {
                    Result.msg = "something went wrong";
                    Result.Status = false;
                    Result.ClusterStoreResPonseDc = null;
                    return Result;
                }
                return Result;
            }

        }


        [Route("ClearCache")]
        [HttpGet]
        public bool ClearCache()
        {

#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
#endif
            return true;
        }

        [Route("")]
        [HttpPut]
        public ClusterStoreResPonse Update(ClusterStoreExecutiveDc obj)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            ClusterStoreResPonse res = new ClusterStoreResPonse();
            using (var context = new AuthContext())
            {
                ClusterStoreExecutive item = new ClusterStoreExecutive();
                if (obj != null)
                {
                    var ExistExecutive = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == obj.ExecutiveId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var update = context.ClusterStoreExecutives.Where(x => x.Id == obj.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (ExistExecutive != null && ExistExecutive.ChannelMasterId != obj.ChannelMasterId)
                    {
                        res.msg = "Please assign same channeltype as previous assigned channelType";
                        res.Status = false;
                        res.ClusterStoreResPonseDc = null;
                        return res;
                    }
                    else
                    {
                        if (ExistExecutive != null && ExistExecutive.ExecutiveId == obj.ExecutiveId && ExistExecutive.ChannelMasterId == obj.ChannelMasterId)
                        {
                            var executive = context.ExecutiveStoreChangeHistories.Where(x => x.ClusterStoreExecutiveId == obj.Id && x.ExecutiveId == update.ExecutiveId && x.IsDeleted == false).FirstOrDefault();
                            if (executive != null)
                            {
                                executive.ExecutiveId = obj.ExecutiveId;
                                executive.OldExecutiveId = update.ExecutiveId;
                                executive.OldStoreId = update.StoreId;
                                executive.OldClusterId = update.ClusterId;
                                executive.OldChannelMasterId = update.ChannelMasterId;
                                executive.ModifiedBy = userid;
                                executive.ModifiedDate = DateTime.Now;
                                context.Entry(executive).State = EntityState.Modified;

                            }
                            if (update != null)
                            {
                                update.ExecutiveId = obj.ExecutiveId;
                                update.StoreId = obj.StoreId;
                                update.ChannelMasterId = obj.ChannelMasterId;
                                update.ModifiedBy = userid;
                                update.ModifiedDate = DateTime.Now;
                                context.Entry(update).State = EntityState.Modified;

                                var data = context.ClusterStoreExecutiveHistories.Where(x => x.ClusterStoreExecutiveId == obj.Id && x.ClusterId == obj.ClusterId && x.StoreId == obj.StoreId && x.ChannelMasterId == obj.ChannelMasterId).FirstOrDefault();
                                if (data != null)
                                {
                                    //ClusterStoreExecutiveHistory clusters = new ClusterStoreExecutiveHistory();
                                    data.ExecutiveId = obj.ExecutiveId;
                                    data.EndDate = DateTime.Now;
                                    context.Entry(data).State = EntityState.Modified;
                                }
                            }
                        }
                        else
                        {
                            if (ExistExecutive == null)
                            {
                                var executive = context.ExecutiveStoreChangeHistories.Where(x => x.ClusterStoreExecutiveId == obj.Id && x.ExecutiveId == update.ExecutiveId && x.IsDeleted == false).FirstOrDefault();
                                if (executive != null)
                                {
                                    executive.ExecutiveId = obj.ExecutiveId;
                                    executive.OldExecutiveId = update.ExecutiveId;
                                    executive.OldStoreId = update.StoreId;
                                    executive.OldClusterId = update.ClusterId;
                                    executive.OldChannelMasterId = update.ChannelMasterId;
                                    executive.ModifiedBy = userid;
                                    executive.ModifiedDate = DateTime.Now;
                                    context.Entry(executive).State = EntityState.Modified;

                                }
                                else
                                {
                                    ExecutiveStoreChangeHistory executiveStoreChangeHistory = new ExecutiveStoreChangeHistory();
                                    executiveStoreChangeHistory.ClusterStoreExecutiveId = obj.Id;
                                    executiveStoreChangeHistory.ExecutiveId = obj.ExecutiveId;
                                    executiveStoreChangeHistory.OldExecutiveId = obj.ExecutiveId;
                                    executiveStoreChangeHistory.OldStoreId = obj.StoreId;
                                    executiveStoreChangeHistory.OldClusterId = obj.ClusterId;
                                    executiveStoreChangeHistory.OldChannelMasterId = obj.ChannelMasterId;
                                    executiveStoreChangeHistory.IsActive = true;
                                    executiveStoreChangeHistory.IsDeleted = false;
                                    executiveStoreChangeHistory.CreatedBy = userid;
                                    executiveStoreChangeHistory.CreatedDate = DateTime.Now;
                                    context.ExecutiveStoreChangeHistories.Add(executiveStoreChangeHistory);
                                }
                                if (update != null)
                                {
                                    update.ExecutiveId = obj.ExecutiveId;
                                    update.StoreId = obj.StoreId;
                                    update.ChannelMasterId = obj.ChannelMasterId;
                                    update.ModifiedBy = userid;
                                    update.ModifiedDate = DateTime.Now;
                                    context.Entry(update).State = EntityState.Modified;

                                    var data = context.ClusterStoreExecutiveHistories.Where(x => x.ClusterStoreExecutiveId == obj.Id && x.ClusterId == obj.ClusterId && x.StoreId == obj.StoreId && x.ChannelMasterId == obj.ChannelMasterId).FirstOrDefault();
                                    if (data != null)
                                    {
                                        //ClusterStoreExecutiveHistory clusters = new ClusterStoreExecutiveHistory();
                                        data.ExecutiveId = obj.ExecutiveId;
                                        data.EndDate = DateTime.Now;
                                        context.Entry(data).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        ClusterStoreExecutiveHistory clusterstore = new ClusterStoreExecutiveHistory();
                                        clusterstore.ClusterId = update.ClusterId;
                                        clusterstore.StoreId = obj.StoreId;
                                        clusterstore.ExecutiveId = obj.ExecutiveId;
                                        clusterstore.ChannelMasterId = obj.ChannelMasterId;
                                        clusterstore.StartDate = DateTime.Now;
                                        clusterstore.ClusterStoreExecutiveId = obj.Id;
                                        context.ClusterStoreExecutiveHistories.Add(clusterstore);
                                    }
                                }
                            }

                        }
                        if (context.Commit() > 0)
                        {
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
#endif
                            res.msg = "Updated Successfully";
                            res.Status = true;
                            res.ClusterStoreResPonseDc = null;
                            return res;
                        }
                    }
                }
                else
                {
                    res.msg = "something went wrong";
                    res.Status = false;
                    res.ClusterStoreResPonseDc = null;
                    return res;
                }
            }
            return res;
        }

        [Route("")]
        [HttpDelete]
        public string Delete(long Id)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "something went wrong";

            using (var context = new AuthContext())
            {
                ClusterStoreExecutive item = new ClusterStoreExecutive();

                var update = context.ClusterStoreExecutives.Where(x => x.Id == Id).FirstOrDefault();
                if (update != null)
                {
                    update.IsDeleted = true;
                    update.ModifiedBy = userid;
                    update.ModifiedDate = DateTime.Now;
                    context.Entry(update).State = EntityState.Modified;
                    context.Commit();
                    result = "Deleted Successfully";
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
#endif
                }
                else
                {
                    result = "Not exists";
                }
            }
            return result;
        }


        [Route("GetExecutiveList/{warehouseId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveListNew(int warehouseId)
        {
            List<ClusterExecutive> result = null;
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter("WarehouseId", SqlDbType.Int);
                idParam.Value = warehouseId;
                result = context.Database.SqlQuery<ClusterExecutive>("exec Store_GetDistinctClusterExecutiveByWarehouseId @WarehouseId", idParam).ToList();
            }
            return result;
        }

        [Route("GetExecutiveByCLusterId/{ClusterId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveByCLusterId(int ClusterId)
        {
            List<ClusterExecutive> result = null;
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter("ClusterId", SqlDbType.Int);
                idParam.Value = ClusterId;
                result = context.Database.SqlQuery<ClusterExecutive>("exec Store_GetDistinctClusterExecutiveByClusterId @ClusterId", idParam).ToList();
            }
            return result;
        }

        [Route("GetExecutiveWiseClusterList/{PeopleId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveWiseClusterList(int PeopleId)
        {
            List<ClusterExecutive> result = null;
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter("PeopleId", SqlDbType.Int);
                idParam.Value = PeopleId;
                result = context.Database.SqlQuery<ClusterExecutive>("exec Store_GetDistinctClusterExecutiveBypeopleId @PeopleID", idParam).ToList();
            }
            return result;
        }

        [Route("GetExecutiveListOfStoretwo/{warehouseId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveListOfStoretwo(int warehouseId)
        {
            List<ClusterExecutive> result = null;
            using (var context = new AuthContext())
            {
                var idParam = new SqlParameter("WarehouseId", SqlDbType.Int);
                idParam.Value = warehouseId;
                result = context.Database.SqlQuery<ClusterExecutive>("exec GetExecutiveListOfStoretwo @WarehouseId", idParam).ToList();
            }
            return result;
        }

        [Route("GetExecutiveListByClusterIds")]
        [HttpPost]
        public async Task<List<ClusterStoreExecutiveDc>> GetExecutiveListByClusterIds(ClusterIdDC ClusterIds)
        {
            List<ClusterStoreExecutiveDc> result = new List<ClusterStoreExecutiveDc>();
            var manager = new ClusterDashboardManager();
            result = manager.GetExecutiveListByClusterIds(ClusterIds);
            return result;
        }

        [Route("GetExecutiveWiseChannelList")]
        [HttpGet]
        public APIResponse GetExecutiveWiseChannelList(int ExecutiveId)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.External.MobileExecutiveDC.ChannelList> data = new List<DataContracts.External.MobileExecutiveDC.ChannelList>();
                data = (from ce in context.ClusterStoreExecutives
                        join c in context.ChannelMasters on ce.ChannelMasterId equals c.ChannelMasterId
                        where c.Deleted == false && c.Active == true
                        && ce.ExecutiveId == ExecutiveId && ce.IsActive == true && ce.IsDeleted == false
                        select new DataContracts.External.MobileExecutiveDC.ChannelList
                        {
                            ChannelMasterId = c.ChannelMasterId,
                            ChannelName = c.ChannelType
                        }).Distinct().ToList();
                if (data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Data = data };
            }
        }
        
        [Route("ClearStoreExecutiveCacheKey")]
        [HttpGet]
        public bool get()
        {
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreExecutiveCacheKey());
#endif
            return true;
        }
    }


    public class ClusterStoreExecutiveeDc
    {
        public int ClusterId { get; set; }

        public long StoreId { get; set; }

        public int ExecutiveId { get; set; }
        public long ChannelMasterId { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public class ClusterStoreResPonse
    {
        public ClusterStoreExecutiveeDc ClusterStoreResPonseDc { get; set; }
        public string msg { get; set; }
        public bool Status { get; set; }
    }
}
