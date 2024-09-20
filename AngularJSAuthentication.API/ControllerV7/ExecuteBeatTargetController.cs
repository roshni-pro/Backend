using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using LinqKit;
using MongoDB.Bson;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/ExecuteBeatTarget")]
    public class ExecuteBeatTargetController : ApiController
    {
        [HttpPost]
        [Route("SaveExecuteBeatTarget")]
        public ResExecuteBeatTarget ExecuteBeatTarget(ExecBeattargetPostDc obj)
        {
            ResExecuteBeatTarget res = new ResExecuteBeatTarget
            {
                msg = "",
                Result = false
            };

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();

            bool flag = false;
            if (obj != null)
            {
                int cityid;
                using (var context = new AuthContext())
                {
                    cityid = context.Warehouses.FirstOrDefault(x => x.WarehouseId == obj.WarehouseId).Cityid;
                    // var cityname = context.Cities.FirstOrDefault(x => x.Cityid == cityid).CityName;
                }
                if (obj.Id != null)
                {
                    var chkExecutiveBeat = mongoDbHelper.Select(x => x.IsDeleted == false && x.Id != ObjectId.Parse(obj.Id) && x.ClusterId == obj.ClusterId && (x.EndDate >= obj.StartDate) && (x.StartDate <= obj.StartDate)).ToList();
                    if (chkExecutiveBeat != null && chkExecutiveBeat.Any())
                    {
                        res.msg = "Already Exist";
                        res.Result = false;
                        return res;
                    }

                    var ExecutiveBeatData = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(obj.Id) && x.IsDeleted == false).FirstOrDefault();
                    if (ExecutiveBeatData != null)
                    {
                        ExecutiveBeatData.CityId = cityid;
                        ExecutiveBeatData.ClusterId = obj.ClusterId;
                        ExecutiveBeatData.ClusterName = obj.ClusterName;
                        ExecutiveBeatData.WarehouseId = obj.WarehouseId;
                        ExecutiveBeatData.WarehouseName = obj.WarehouseName;
                        ExecutiveBeatData.VisitedPercent = obj.VisitedPercent;
                        ExecutiveBeatData.ConversionPercent = obj.ConversionPercent;
                        ExecutiveBeatData.CustomerPercent = obj.CustomerPercent;
                        ExecutiveBeatData.OrderPercent = obj.OrderPercent;
                        ExecutiveBeatData.ProductPareto = obj.ProductPareto;
                        ExecutiveBeatData.CustomerPareto = obj.CustomerPareto;
                        ExecutiveBeatData.AvgLineItem = obj.AvgLineItem;
                        ExecutiveBeatData.AvgOrderAmount = obj.AvgOrderAmount;
                        ExecutiveBeatData.StartDate = obj.StartDate;
                        ExecutiveBeatData.EndDate = obj.EndDate;
                        //ExecutiveBeatData.CreatedDate = DateTime.Now;
                        //ExecutiveBeatData.IsDeleted = false;
                        //ExecutiveBeatData.IsActive = true;
                        flag = mongoDbHelper.Replace(ObjectId.Parse(obj.Id), ExecutiveBeatData);
                    }
                }
                else
                {
                    var chkExecutiveBeat = mongoDbHelper.Select(x => x.IsDeleted == false && x.ClusterId == obj.ClusterId && (x.EndDate >= obj.StartDate) && (x.StartDate <= obj.StartDate)).ToList();

                    if (chkExecutiveBeat != null && chkExecutiveBeat.Any())
                    {
                        res.msg = "Already Exist";
                        res.Result = false;
                        return res;
                    }
                    ExecuteBeatTarget ExecuteBeatTargetdata = new ExecuteBeatTarget
                    {
                        CityId = cityid,
                        ClusterId = obj.ClusterId,
                        ClusterName = obj.ClusterName,
                        WarehouseId = obj.WarehouseId,
                        WarehouseName = obj.WarehouseName,
                        VisitedPercent = obj.VisitedPercent,
                        ConversionPercent = obj.ConversionPercent,
                        CustomerPercent = obj.CustomerPercent,
                        OrderPercent = obj.OrderPercent,
                        ProductPareto = obj.ProductPareto,
                        CustomerPareto = obj.CustomerPareto,
                        AvgLineItem = obj.AvgLineItem,
                        AvgOrderAmount = obj.AvgOrderAmount,
                        StartDate = obj.StartDate,
                        EndDate = obj.EndDate,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false,
                        IsActive = true
                    };
                    flag = mongoDbHelper.Insert(ExecuteBeatTargetdata);
                }
                if (flag)
                {
                    res.Result = true;
                    res.msg = "Execute Beat Target save successfully.";
                }
                else
                {
                    res.Result = false;
                    res.msg = "Failed";
                }

            }
            return res;
        }

        [HttpGet]
        [Route("ExecuteBeatTargetList")]
        public ResExecuteBeatTarget ExecuteBeatTargetList(int warehouseId, int? ClusterId, int skip, int take)
        {
            ResExecuteBeatTarget res = new ResExecuteBeatTarget();
            int Skiplist = (skip - 1) * take;
            MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();
            List<ExecuteBeatTarget> ExecuteBeatList = new List<ExecuteBeatTarget>();
            var searchPredicate = PredicateBuilder.New<ExecuteBeatTarget>(x => x.IsDeleted == false);
            if (warehouseId != 0 || warehouseId > 0)
            {
                searchPredicate = PredicateBuilder.New<ExecuteBeatTarget>(x => x.WarehouseId == warehouseId);
            }
            if (ClusterId != 0 || ClusterId > 0)
            {
                searchPredicate = PredicateBuilder.New<ExecuteBeatTarget>(x => x.ClusterId == ClusterId);
            }
            ExecuteBeatList = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.CreatedDate), Skiplist, take).ToList();
            int count = ExecuteBeatList.Count();

            res.Count = count;
            res.ExecuteBeatTargets = ExecuteBeatList;
            return res;
        }

        [Route("ClusterCustomerCount")]
        [HttpGet]
        public int ClusterCustomerCount(int clustId)
        {
            int totalcount = 0;
            using (var context = new AuthContext())
            {
                totalcount = context.Customers.Where(x => x.ClusterId == clustId && x.Deleted == false && x.Active == true).Count();
            }
            return totalcount;
        }
        [Route("ExecutiveBeatById")]
        [HttpGet]
        public ResExecuteBeatTarget ExecutiveBeatById(string objectId)
        {
            ResExecuteBeatTarget res = new ResExecuteBeatTarget();
            MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();
            if (objectId != null)
            {
                var ExecutiveBeatData = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(objectId) && x.IsDeleted == false).FirstOrDefault();
                if (ExecutiveBeatData != null)
                {
                    res.Data = ExecutiveBeatData;
                    res.Result = true;
                }
                else
                {
                    res.Result = false;
                    res.msg = "No Record";
                }
            }
            return res;
        }
        //[Route("DeleteExecutiveBeat")]
        //[HttpGet]
        //public ResExecuteBeatTarget DeleteExecutiveBeat(string objectId)
        //{
        //    ResExecuteBeatTarget res = new ResExecuteBeatTarget();
        //    MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();
        //    if (objectId != null)
        //    {
        //        bool flag = false;
        //        var ExecutiveBeatData = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(objectId) && x.IsDeleted == false).FirstOrDefault();

        //        if (ExecutiveBeatData != null)
        //        {
        //            ExecutiveBeatData.IsDeleted = true;
        //            ExecutiveBeatData.IsActive = false;
        //            flag = mongoDbHelper.Replace(ObjectId.Parse(objectId), ExecutiveBeatData);
        //            if (flag)
        //            {
        //                res.msg = "Executive Beat Target Deleted";
        //                res.Result = true;
        //            }
        //        }
        //        else
        //        {
        //            res.Result = false;
        //            res.msg = "Failed";
        //        }
        //    }
        //    return res;
        //}
        [Route("ActiveInActiveExecutiveBeat")]
        [HttpGet]
        public ResExecuteBeatTarget ActiveInActiveExecutiveBeat(string objectId, bool status)
        {
            ResExecuteBeatTarget res = new ResExecuteBeatTarget();
            MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();
            if (objectId != null)
            {
                bool flag = false;
                var ExecutiveBeatData = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(objectId) && x.IsDeleted == false).FirstOrDefault();

                if (ExecutiveBeatData != null)
                {
                    ExecutiveBeatData.IsActive = status;
                    flag = mongoDbHelper.Replace(ObjectId.Parse(objectId), ExecutiveBeatData);
                    if (flag)
                    {
                        string strstatus = status ? "Active" : "Inactive";
                        res.msg = strstatus + " successfully.";
                        return res;
                    }
                }
                else
                {
                    res.Result = false;
                    res.msg = "Failed";
                }
            }
            return res;
        }
    }
}

