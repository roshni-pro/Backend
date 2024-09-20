using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.SalesApp
{
    public class BeatsManager
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> InsertBeatInMongo(int peopleId, double lat, double lng, string address)
        {
            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = DateTime.Now.Date;
            var todayBeats = await mongoDbHelper.SelectAsync(x => x.PeopleId == peopleId && x.AssignmentDate == today);
            if (!(todayBeats != null && todayBeats.Any(x => x.DayStartTime.HasValue)))
            {
                MongoDbHelper<NextDayBeatPlan> mongoDbCustomBeatPlanHelper = new MongoDbHelper<NextDayBeatPlan>();
                var CustomBeatPlans = (await mongoDbCustomBeatPlanHelper.SelectAsync(x => x.PlanDate == today && x.ExecutiveId == peopleId)).ToList();
                List<ExecutiveBeats> beats = new List<ExecutiveBeats>();

                ConcurrentBag<ExecutiveBeats> beatsBag = new ConcurrentBag<ExecutiveBeats>();

                using (var context = new AuthContext())
                {    
                    int WarehouseId = context.Peoples.Where(x => x.PeopleID == peopleId).Select(x => x.WarehouseId).FirstOrDefault();
                    //var query = string.Format("exec IsSalesAppLead {0}", peopleId);
                    //var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                    //bool Isdigitalexecutive = isSalesLead > 0;

                    var executiveBeats = new List<SalespDTO>();

                    //if (Isdigitalexecutive)
                    //{
                    //    context.Database.CommandTimeout = 900;
                    //    executiveBeats = await context.Database.SqlQuery<SalespDTO>(string.Format("exec GetDigitalExecutiveTodayBeat {0},{1}", peopleId, WarehouseId)).ToListAsync();
                    //}
                    //else
                    {
                        context.Database.CommandTimeout = 900;
                        executiveBeats = await context.Database.SqlQuery<SalespDTO>(string.Format("exec GetExecutiveTodayBeat {0},{1}", peopleId, WarehouseId)).ToListAsync();
                    }

                    var customerids = executiveBeats.Select(x => x.CustomerId).ToList();
                    var Todaycustomerids = executiveBeats.Select(x => x.CustomerId).ToList();

                    BusinessLayer.Managers.Masters.SalesAppManager salesAppManager = new BusinessLayer.Managers.Masters.SalesAppManager();
                    var res = salesAppManager.GetBeatTargetDashboardData(peopleId, WarehouseId, customerids, Todaycustomerids);

                    ParallelLoopResult parellelResult = Parallel.ForEach(executiveBeats.GroupBy(x => new { x.ExecutiveId,x.ChannelMasterId,x.ChannelName }), (item) =>
                    {
                        var assignmentDate = DateTime.Now.Date;
                        beatsBag.Add(new ExecutiveBeats
                        {
                            PeopleId = item.Key.ExecutiveId,
                            ChannelMasterId = item.Key.ChannelMasterId,
                            ChannelName = item.Key.ChannelName,
                            CreatedDate = DateTime.Now,
                            DayStartTime = assignmentDate == DateTime.Now.Date ? DateTime.Now : (DateTime?)null,
                            DayStartAddress = address,
                            DayStartLat = assignmentDate == DateTime.Now.Date ? lat : (double?)null,
                            DayStartLng = assignmentDate == DateTime.Now.Date ? lng : (double?)null,
                            AssignmentDate = assignmentDate,
                            TodayTarget = res != null && res.TargetSales != null ? res.TargetSales.TodayTargetSales : 0,
                            PlannedRoutes = item.Select(i => new PlannedRoute
                            {
                                CustomerId = i.CustomerId,
                                CompanyId = i.CompanyId,
                                Active = i.Active,
                                CustomerVerify = i.CustomerVerify,
                                City = i.City,
                                WarehouseId = i.WarehouseId,
                                WarehouseName = i.WarehouseName,
                                lat = i.lat,
                                lg = i.lg,
                                ExecutiveId = i.ExecutiveId,
                                BeatNumber = i.BeatNumber,
                                Day = i.Day,
                                Skcode = i.Skcode,
                                Mobile = i.Mobile,
                                ShopName = i.ShopName,
                                BillingAddress = i.BillingAddress,
                                ShippingAddress = i.ShippingAddress,
                                Name = i.Name,
                                Emailid = i.Emailid,
                                RefNo = i.RefNo,
                                Password = i.Password,
                                UploadRegistration = i.UploadRegistration,
                                ResidenceAddressProof = i.ResidenceAddressProof,
                                DOB = i.DOB,
                                MaxOrderCount = i.MaxOrderCount,
                                IsKPP = i.IsKPP,
                                ClusterId = i.ClusterId,
                                ClusterName = i.ClusterName,
                                CustomerType=i.CustomerType
                            }).ToList(),

                        });

                        var existingBeat = mongoDbHelper.Select(x => x.PeopleId == item.Key.ExecutiveId && x.AssignmentDate == assignmentDate);

                        if (existingBeat != null && existingBeat.Any())
                            mongoDbHelper.Delete(existingBeat.FirstOrDefault().Id);

                    });

                    if (parellelResult.IsCompleted && beatsBag.Any())
                    {
                        if (CustomBeatPlans != null && CustomBeatPlans.Any())
                        {
                            var customerIds = CustomBeatPlans.Select(x => x.CustomerId).ToList();
                            var customers = context.Customers.Where(x => customerIds.Contains(x.CustomerId)).ToList();
                            int i = beatsBag.FirstOrDefault(x => x.AssignmentDate == today).PlannedRoutes.Count;
                            foreach (var cust in customers)
                            {
                                beatsBag.FirstOrDefault(x => x.AssignmentDate == today).PlannedRoutes.Add(
                                new PlannedRoute
                                {
                                    CustomerId = cust.CustomerId,
                                    CompanyId = cust.CompanyId,
                                    Active = cust.Active,
                                    CustomerVerify = cust.CustomerVerify,
                                    City = cust.City,
                                    WarehouseId = cust.Warehouseid.Value,
                                    WarehouseName = cust.WarehouseName,
                                    lat = cust.lat,
                                    lg = cust.lg,
                                    ExecutiveId = cust.ExecutiveId ?? 0,
                                    BeatNumber = cust.BeatNumber,
                                    Day = today.DayOfWeek.ToString(),
                                    Skcode = cust.Skcode,
                                    Mobile = cust.Mobile,
                                    ShopName = cust.ShopName,
                                    BillingAddress = cust.BillingAddress,
                                    ShippingAddress = cust.ShippingAddress,
                                    Name = cust.Name,
                                    Emailid = cust.Emailid,
                                    RefNo = cust.RefNo,
                                    Password = cust.Password,
                                    UploadRegistration = cust.UploadRegistration,
                                    ResidenceAddressProof = cust.ResidenceAddressProof,
                                    DOB = cust.DOB,
                                    MaxOrderCount = cust.ordercount,
                                    IsKPP = cust.IsKPP,
                                    ClusterId = cust.ClusterId ?? 0,
                                    ClusterName = cust.ClusterName,
                                    CustomerType = cust.CustomerType
                                    //StoreId = i.StoreId,     // 17March2022
                                    //SchedulerType = i.SchedulerType, // 17March2022
                                    //SkipWeeks = i.SkipWeeks// 17March2022
                                });
                                i++;
                            }
                        }
                        await mongoDbHelper.InsertManyAsync(beatsBag.ToList().OrderBy(x => x.AssignmentDate).ToList());
                    }
                    else
                    {
                        var ChannelData = (from cs in context.ClusterStoreExecutives
                                           join ch in context.ChannelMasters
                                           on cs.ChannelMasterId equals ch.ChannelMasterId
                                           where cs.IsActive == true && cs.IsDeleted == false
                                           && ch.Active == true && ch.Deleted == false
                                           select new
                                           {
                                               ch.ChannelMasterId,
                                               ch.ChannelType
                                           }).FirstOrDefault();

                        var todaybeat = new ExecutiveBeats
                        {
                            PeopleId = peopleId,
                            CreatedDate = DateTime.Now,
                            DayStartTime = DateTime.Now,
                            DayStartAddress = address,
                            DayStartLat = lat,
                            DayStartLng = lng,
                            AssignmentDate = today,
                            TodayTarget = 0,
                            ChannelMasterId = ChannelData != null ? ChannelData.ChannelMasterId : 0,
                            ChannelName = ChannelData != null ? ChannelData.ChannelType : null,
                            PlannedRoutes = new List<PlannedRoute>()
                        };
                        await mongoDbHelper.InsertAsync(todaybeat);
                    }

                }
            }
            return true;
        }

        public async Task<bool> InsertNotStartExecutiveBeatInMongo(List<int> peopleIds)
        {

            logger.Trace("Missing Job Started for Executives Ids :" + string.Join(",", peopleIds));
            double lat = 0, lng = 0;
            string address = "";
            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = DateTime.Now.Date;
            MongoDbHelper<NextDayBeatPlan> mongoDbCustomBeatPlanHelper = new MongoDbHelper<NextDayBeatPlan>();
            ConcurrentBag<ExecutiveBeats> beatsBag = new ConcurrentBag<ExecutiveBeats>();
            ParallelLoopResult parellelResult = Parallel.ForEach(peopleIds, (peopleId) =>
            {
                var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);
                if (!(todayBeats != null && todayBeats.Any(x => x.DayStartTime.HasValue)))
                {
                    List<SalespDTO> executiveBeats = new List<SalespDTO>();
                    int WarehouseId = 0;
                    using (var context = new AuthContext())
                    {
                        WarehouseId = context.Peoples.Where(x => x.PeopleID == peopleId).Select(x => x.WarehouseId).FirstOrDefault();
                        executiveBeats = context.Database.SqlQuery<SalespDTO>(string.Format("exec GetExecutiveTodayBeat {0}", peopleId)).ToList();
                    }
                    var customerids = executiveBeats.Select(x => x.CustomerId).ToList();
                    var Todaycustomerids = executiveBeats.Select(x => x.CustomerId).ToList();
                    BusinessLayer.Managers.Masters.SalesAppManager salesAppManager = new BusinessLayer.Managers.Masters.SalesAppManager();
                    var res = salesAppManager.GetBeatTargetDashboardData(peopleId, WarehouseId, customerids, Todaycustomerids);
                    var assignmentDate = today;
                    if (executiveBeats != null && executiveBeats.Any())
                    {
                        beatsBag.Add(executiveBeats.GroupBy(x => new { x.ExecutiveId }).Select(item => new ExecutiveBeats
                        {
                            PeopleId = item.Key.ExecutiveId,
                            CreatedDate = DateTime.Now,
                            DayStartTime = assignmentDate == DateTime.Now.Date ? DateTime.Now : (DateTime?)null,
                            DayStartAddress = address,
                            DayStartLat = assignmentDate == DateTime.Now.Date ? lat : (double?)null,
                            DayStartLng = assignmentDate == DateTime.Now.Date ? lng : (double?)null,
                            AssignmentDate = assignmentDate,
                            TodayTarget = res != null && res.TargetSales != null ? res.TargetSales.TodayTargetSales : 0,
                            PlannedRoutes = item.Select(i => new PlannedRoute
                            {
                                CustomerId = i.CustomerId,
                                CompanyId = i.CompanyId,
                                Active = i.Active,
                                CustomerVerify = i.CustomerVerify,
                                City = i.City,
                                WarehouseId = i.WarehouseId,
                                WarehouseName = i.WarehouseName,
                                lat = i.lat,
                                lg = i.lg,
                                ExecutiveId = i.ExecutiveId,
                                BeatNumber = i.BeatNumber,
                                Day = i.Day,
                                Skcode = i.Skcode,
                                Mobile = i.Mobile,
                                ShopName = i.ShopName,
                                BillingAddress = i.BillingAddress,
                                ShippingAddress = i.ShippingAddress,
                                Name = i.Name,
                                Emailid = i.Emailid,
                                RefNo = i.RefNo,
                                Password = i.Password,
                                UploadRegistration = i.UploadRegistration,
                                ResidenceAddressProof = i.ResidenceAddressProof,
                                DOB = i.DOB,
                                MaxOrderCount = i.MaxOrderCount,
                                IsKPP = i.IsKPP,
                                ClusterId = i.ClusterId,
                                ClusterName = i.ClusterName,
                                CustomerType = i.CustomerType
                            }).ToList()

                        }).FirstOrDefault());
                    }
                }
            });
            if (parellelResult.IsCompleted && beatsBag.Any())
            {
                foreach (var item in beatsBag.Where(x => x.PeopleId > 0))
                {
                    var CustomBeatPlans = (await mongoDbCustomBeatPlanHelper.SelectAsync(x => x.PlanDate == today && x.ExecutiveId == item.PeopleId)).ToList();
                    if (CustomBeatPlans != null && CustomBeatPlans.Any())
                    {
                        var customerIds = CustomBeatPlans.Select(x => x.CustomerId).ToList();
                        var customers = new List<Customer>();
                        using (var context = new AuthContext())
                        {
                            customers = context.Customers.Where(x => customerIds.Contains(x.CustomerId)).ToList();
                        }
                        int i = item.PlannedRoutes.Count;
                        foreach (var cust in customers)
                        {
                            item.PlannedRoutes.Add(
                            new PlannedRoute
                            {
                                CustomerId = cust.CustomerId,
                                CompanyId = cust.CompanyId,
                                Active = cust.Active,
                                CustomerVerify = cust.CustomerVerify,
                                City = cust.City,
                                WarehouseId = cust.Warehouseid.Value,
                                WarehouseName = cust.WarehouseName,
                                lat = cust.lat,
                                lg = cust.lg,
                                ExecutiveId = cust.ExecutiveId ?? 0,
                                BeatNumber = cust.BeatNumber,
                                Day = today.DayOfWeek.ToString(),
                                Skcode = cust.Skcode,
                                Mobile = cust.Mobile,
                                ShopName = cust.ShopName,
                                BillingAddress = cust.BillingAddress,
                                ShippingAddress = cust.ShippingAddress,
                                Name = cust.Name,
                                Emailid = cust.Emailid,
                                RefNo = cust.RefNo,
                                Password = cust.Password,
                                UploadRegistration = cust.UploadRegistration,
                                ResidenceAddressProof = cust.ResidenceAddressProof,
                                DOB = cust.DOB,
                                MaxOrderCount = cust.ordercount,
                                IsKPP = cust.IsKPP,
                                ClusterId = cust.ClusterId ?? 0,
                                ClusterName = cust.ClusterName,
                                CustomerType = cust.CustomerType
                            });
                            i++;
                        }
                    }
                }
                await mongoDbHelper.InsertManyAsync(beatsBag.ToList().OrderBy(x => x.AssignmentDate).ToList());
            }

            return true;
        }

        internal async Task<bool> IsDayStarted(int peopleId)
        {
            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = DateTime.Now.Date;
            var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);
            return todayBeats != null && todayBeats.Any(x => x.DayStartTime.HasValue);
        }

        internal async Task<DateTime?> BeatStart(int peopleId, int customerId)
        {
            DateTime? startDateTime = (DateTime?)null;
            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = DateTime.Now.Date;
            var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);
            if (todayBeats != null && todayBeats.Any() && todayBeats.FirstOrDefault().PlannedRoutes != null && todayBeats.FirstOrDefault().PlannedRoutes.Any(x => x.CustomerId == customerId))
            {
                if (!todayBeats.FirstOrDefault().PlannedRoutes.FirstOrDefault(x => x.CustomerId == customerId).TravalStart.HasValue)
                {
                    todayBeats.FirstOrDefault().PlannedRoutes.FirstOrDefault(x => x.CustomerId == customerId).TravalStart = DateTime.Now;
                    if (mongoDbHelper.Replace(todayBeats.FirstOrDefault().Id, todayBeats.FirstOrDefault()))
                        startDateTime = todayBeats.FirstOrDefault().PlannedRoutes.FirstOrDefault(x => x.CustomerId == customerId).TravalStart;
                }
                else
                    startDateTime = todayBeats.FirstOrDefault().PlannedRoutes.FirstOrDefault(x => x.CustomerId == customerId).TravalStart;
            }

            return startDateTime;
        }

        internal async Task<int> InactiveCustOrderCount(int customerid)
        {
            using (var context = new AuthContext())
            {
                var orderCount = await context.Database.SqlQuery<int>(string.Format("exec GetInactiveCustOrderCount {0}", customerid)).FirstOrDefaultAsync();
                return orderCount;
            }
        }

        internal async Task<bool> UpdateActualRoute(List<DataContracts.External.MobileExecutiveDC.SalesAppRouteParam> param)
        {
            var now = DateTime.Now;

            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = now.Date;
            var peopleId = param.FirstOrDefault().PeopleId;

            var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);
            var mybeat = todayBeats != null && todayBeats.Any() ? todayBeats.FirstOrDefault() : new ExecutiveBeats();
            var custData = mybeat.PlannedRoutes.Where(s => param.Select(x => x.CustomerId).Contains(s.CustomerId));
            var existingActualRoute = mybeat.ActualRoutes != null && mybeat.ActualRoutes.Any()
                                        ? mybeat.ActualRoutes.Where(s => param.Select(x => x.CustomerId).Contains(s.CustomerId))
                                        : null;

            var StartTime = existingActualRoute != null && existingActualRoute.Any() ? existingActualRoute.FirstOrDefault().CheckIn : now;
            var EndTime = existingActualRoute != null && existingActualRoute.Any() ? existingActualRoute.FirstOrDefault().CheckOut : now;

            List<ActualRoute> actualRoute = new List<ActualRoute>();

            List<int> customerNotFound = custData != null && custData.Any()
                                        ? param.Select(x => x.CustomerId).Except(custData.Select(z => z.CustomerId)).ToList()
                                        : null;

            if (custData == null || !custData.Any())
            {
                foreach (var item in param)
                {
                    if (existingActualRoute == null || !existingActualRoute.Any(x => x.CustomerId == item.CustomerId))
                    {
                        using (var context = new AuthContext())
                        {
                            var i = await context.Database.SqlQuery<SalespDTO>(string.Format("exec GetCustDetailForBeat {0}", item.CustomerId)).FirstOrDefaultAsync();
                            if (i != null)
                            {
                                actualRoute.Add(new ActualRoute
                                {
                                    CustomerId = i.CustomerId,
                                    CompanyId = i.CompanyId,
                                    Active = i.Active,
                                    CustomerVerify = i.CustomerVerify,
                                    City = i.City,
                                    WarehouseId = i.WarehouseId,
                                    WarehouseName = i.WarehouseName,
                                    lat = i.lat,
                                    lg = i.lg,
                                    ExecutiveId = i.ExecutiveId,
                                    BeatNumber = i.BeatNumber,
                                    Day = i.Day,
                                    Skcode = i.Skcode,
                                    Mobile = i.Mobile,
                                    ShopName = i.ShopName,
                                    BillingAddress = i.BillingAddress,
                                    ShippingAddress = i.ShippingAddress,
                                    Name = i.Name,
                                    Emailid = i.Emailid,
                                    RefNo = i.RefNo,
                                    Password = i.Password,
                                    UploadRegistration = i.UploadRegistration,
                                    ResidenceAddressProof = i.ResidenceAddressProof,
                                    DOB = i.DOB,
                                    MaxOrderCount = i.MaxOrderCount,
                                    Distance = item.Distance,
                                    ExecAddress = item.CurrentAddress,
                                    ExecLat = item.CurrentLat,
                                    ExecLng = item.CurrentLng,
                                    Comment = item.Comment,
                                    ShopCloseImage = item.ShopCloseImage,
                                    IsVisited = true,
                                    VisitedOn = now,
                                    IsKPP = i.IsKPP,
                                    ClusterId = i.ClusterId,
                                    ClusterName = i.ClusterName
                                });
                            }
                            //if (!mybeat.PlannedRoutes.Any(s => param.Select(x => x.CustomerId).Contains(s.CustomerId)))
                            //{
                            //    mybeat.PlannedRoutes.Add(new PlannedRoute
                            //    {
                            //        CustomerId = i.CustomerId,
                            //        CompanyId = i.CompanyId,
                            //        Active = i.Active,
                            //        CustomerVerify = i.CustomerVerify,
                            //        City = i.City,
                            //        WarehouseId = i.WarehouseId,
                            //        WarehouseName = i.WarehouseName,
                            //        lat = i.lat,
                            //        lg = i.lg,
                            //        ExecutiveId = i.ExecutiveId,
                            //        BeatNumber = i.BeatNumber,
                            //        Day = today.DayOfWeek.ToString(),
                            //        Skcode = i.Skcode,
                            //        Mobile = i.Mobile,
                            //        ShopName = i.ShopName,
                            //        BillingAddress = i.BillingAddress,
                            //        ShippingAddress = i.ShippingAddress,
                            //        Name = i.Name,
                            //        Emailid = i.Emailid,
                            //        RefNo = i.RefNo,
                            //        Password = i.Password,
                            //        UploadRegistration = i.UploadRegistration,
                            //        ResidenceAddressProof = i.ResidenceAddressProof,
                            //        DOB = i.DOB,
                            //        MaxOrderCount = i.MaxOrderCount,
                            //        IsVisited = true,
                            //        VisitedOn = now,
                            //        IsKPP = i.IsKPP,
                            //        ClusterId = i.ClusterId,
                            //        ClusterName = i.ClusterName
                            //    });
                            //}
                        }
                    }
                }
            }

            if (customerNotFound != null && customerNotFound.Any())
            {
                foreach (var item in customerNotFound)
                {
                    var thisCust = param.FirstOrDefault(x => x.CustomerId == item);
                    if (existingActualRoute == null || !existingActualRoute.Any(x => x.CustomerId == thisCust.CustomerId))
                    {
                        using (var context = new AuthContext())
                        {
                            var i = await context.Database.SqlQuery<SalespDTO>(string.Format("exec GetCustDetailForBeat {0}", item)).FirstOrDefaultAsync();
                            if (i != null)
                            {
                                actualRoute.Add(new ActualRoute
                                {
                                    CustomerId = i.CustomerId,
                                    CompanyId = i.CompanyId,
                                    Active = i.Active,
                                    CustomerVerify = i.CustomerVerify,
                                    City = i.City,
                                    WarehouseId = i.WarehouseId,
                                    WarehouseName = i.WarehouseName,
                                    lat = i.lat,
                                    lg = i.lg,
                                    ExecutiveId = i.ExecutiveId,
                                    BeatNumber = i.BeatNumber,
                                    Day = i.Day,
                                    Skcode = i.Skcode,
                                    Mobile = i.Mobile,
                                    ShopName = i.ShopName,
                                    BillingAddress = i.BillingAddress,
                                    ShippingAddress = i.ShippingAddress,
                                    Name = i.Name,
                                    Emailid = i.Emailid,
                                    RefNo = i.RefNo,
                                    Password = i.Password,
                                    UploadRegistration = i.UploadRegistration,
                                    ResidenceAddressProof = i.ResidenceAddressProof,
                                    DOB = i.DOB,
                                    MaxOrderCount = i.MaxOrderCount,
                                    Distance = thisCust.Distance,
                                    ExecAddress = thisCust.CurrentAddress,
                                    ExecLat = thisCust.CurrentLat,
                                    ExecLng = thisCust.CurrentLng,
                                    IsVisited = true,
                                    VisitedOn = now,
                                    ClusterId = i.ClusterId,
                                    ClusterName = i.ClusterName,
                                    Comment = thisCust.Comment,
                                    ShopCloseImage = thisCust.ShopCloseImage
                                });
                            }
                        }
                    }
                }
            }


            if (custData != null)
            {
                if (existingActualRoute != null && existingActualRoute.Any())
                    custData.ToList().RemoveAll(x => existingActualRoute.Select(z => z.CustomerId).Contains(x.CustomerId));

                if (custData != null && custData.Any())
                    actualRoute = custData.Select(i => new ActualRoute
                    {
                        CustomerId = i.CustomerId,
                        CompanyId = i.CompanyId,
                        Active = i.Active,
                        CustomerVerify = i.CustomerVerify,
                        City = i.City,
                        WarehouseId = i.WarehouseId,
                        WarehouseName = i.WarehouseName,
                        lat = i.lat,
                        lg = i.lg,
                        ExecutiveId = i.ExecutiveId,
                        BeatNumber = i.BeatNumber,
                        Day = i.Day,
                        Skcode = i.Skcode,
                        Mobile = i.Mobile,
                        ShopName = i.ShopName,
                        BillingAddress = i.BillingAddress,
                        ShippingAddress = i.ShippingAddress,
                        Name = i.Name,
                        Emailid = i.Emailid,
                        RefNo = i.RefNo,
                        Password = i.Password,
                        UploadRegistration = i.UploadRegistration,
                        ResidenceAddressProof = i.ResidenceAddressProof,
                        DOB = i.DOB,
                        MaxOrderCount = i.MaxOrderCount,
                        Distance = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).Distance,
                        ExecAddress = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).CurrentAddress,
                        ExecLat = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).CurrentLat,
                        ExecLng = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).CurrentLng,
                        IsVisited = true,
                        VisitedOn = now,
                        ClusterId = i.ClusterId,
                        ClusterName = i.ClusterName,
                        Comment = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).Comment,
                        ShopCloseImage = param.FirstOrDefault(x => x.CustomerId == i.CustomerId).ShopCloseImage,
                        CheckIn = StartTime,
                        CheckOut = EndTime
                    }).ToList();
            }

            if (mybeat.ActualRoutes == null)
            {
                mybeat.ActualRoutes = new List<ActualRoute>();
            }

            mybeat.ActualRoutes.AddRange(actualRoute);
            mybeat.PlannedRoutes.Where(s => param.Select(x => x.CustomerId).Contains(s.CustomerId)).ToList().ForEach(x =>
            {
                x.IsVisited = true;
                x.VisitedOn = now;
            });
            await mongoDbHelper.ReplaceAsync(mybeat.Id, mybeat);

            return true;
        }

        internal async Task<DateTime> BeatActualRoute(DataContracts.External.MobileExecutiveDC.SalesAppRouteParam param)
        {
            var now = DateTime.Now;

            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = now.Date;
            var peopleId = param.PeopleId;

            var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);
            var mybeat = todayBeats != null && todayBeats.Any() ? todayBeats.FirstOrDefault() : new ExecutiveBeats();
            var custData = mybeat.PlannedRoutes != null ? mybeat.PlannedRoutes.FirstOrDefault(s => param.CustomerId == s.CustomerId) : null;
            var existingActualRoute = mybeat.ActualRoutes != null && mybeat.ActualRoutes.Any()
                                        ? mybeat.ActualRoutes.FirstOrDefault(s => param.CustomerId == s.CustomerId)
                                        : null;

            var StartTime = existingActualRoute != null && existingActualRoute.CheckIn.HasValue ? existingActualRoute.CheckIn.Value : now;
            DateTime? EndTime = null;
            if (existingActualRoute != null && existingActualRoute.CheckIn.HasValue && param.IsEnd)
                EndTime = now;

            DateTime? customerClockTime = null;

            if (!param.IsEnd)
                customerClockTime = param.StartDateTime;
            else
                customerClockTime = param.EndDateTime;



            ActualRoute actualRoute = new ActualRoute();


            if (custData == null)
            {
                if (existingActualRoute == null)
                {
                    using (var context = new AuthContext())
                    {
                        var i = await context.Database.SqlQuery<SalespDTO>(string.Format("exec GetCustDetailForSalesApp {0},{1}", param.CustomerId, param.PeopleId)).FirstOrDefaultAsync();
                        if (i != null)
                        {
                            actualRoute = new ActualRoute
                            {
                                CustomerId = i.CustomerId,
                                CompanyId = i.CompanyId,
                                Active = i.Active,
                                CustomerVerify = i.CustomerVerify,
                                City = i.City,
                                WarehouseId = i.WarehouseId,
                                WarehouseName = i.WarehouseName,
                                lat = i.lat,
                                lg = i.lg,
                                ExecutiveId = i.ExecutiveId,
                                BeatNumber = i.BeatNumber,
                                Day = i.Day,
                                Skcode = i.Skcode,
                                Mobile = i.Mobile,
                                ShopName = i.ShopName,
                                BillingAddress = i.BillingAddress,
                                ShippingAddress = i.ShippingAddress,
                                Name = i.Name,
                                Emailid = i.Emailid,
                                RefNo = i.RefNo,
                                Password = i.Password,
                                UploadRegistration = i.UploadRegistration,
                                ResidenceAddressProof = i.ResidenceAddressProof,
                                DOB = i.DOB,
                                MaxOrderCount = i.MaxOrderCount,
                                Distance = param.Distance,
                                ExecAddress = param.CurrentAddress,
                                ExecLat = param.CurrentLat,
                                ExecLng = param.CurrentLng,
                                Comment = param.Comment,
                                ShopCloseImage = param.ShopCloseImage,
                                IsVisited = true,
                                VisitedOn = now,
                                IsKPP = i.IsKPP,
                                ClusterId = i.ClusterId,
                                ClusterName = i.ClusterName,
                                CheckIn = StartTime,
                                CheckOut = EndTime,
                                IsBeat = param.IsBeat,
                                CustomerCheckInOutHistories = new List<CustomerCheckInOutHistory>()
                            };
                            actualRoute.CustomerCheckInOutHistories.Add(new CustomerCheckInOutHistory
                            {
                                CheckIn = customerClockTime,
                                CheckOut = null
                            });
                        }
                    }
                }

            }
            else if (custData != null)
            {
                if (existingActualRoute == null)
                {
                    actualRoute = new ActualRoute
                    {
                        CustomerId = custData.CustomerId,
                        CompanyId = custData.CompanyId,
                        Active = custData.Active,
                        CustomerVerify = custData.CustomerVerify,
                        City = custData.City,
                        WarehouseId = custData.WarehouseId,
                        WarehouseName = custData.WarehouseName,
                        lat = custData.lat,
                        lg = custData.lg,
                        ExecutiveId = custData.ExecutiveId,
                        BeatNumber = custData.BeatNumber,
                        Day = custData.Day,
                        Skcode = custData.Skcode,
                        Mobile = custData.Mobile,
                        ShopName = custData.ShopName,
                        BillingAddress = custData.BillingAddress,
                        ShippingAddress = custData.ShippingAddress,
                        Name = custData.Name,
                        Emailid = custData.Emailid,
                        RefNo = custData.RefNo,
                        Password = custData.Password,
                        UploadRegistration = custData.UploadRegistration,
                        ResidenceAddressProof = custData.ResidenceAddressProof,
                        DOB = custData.DOB,
                        MaxOrderCount = custData.MaxOrderCount,
                        Distance = param.Distance,
                        ExecAddress = param.CurrentAddress,
                        ExecLat = param.CurrentLat,
                        ExecLng = param.CurrentLng,
                        IsVisited = true,
                        VisitedOn = now,
                        ClusterId = custData.ClusterId,
                        ClusterName = custData.ClusterName,
                        Comment = param.Comment,
                        ShopCloseImage = param.ShopCloseImage,
                        CheckIn = StartTime,
                        CheckOut = EndTime,
                        IsBeat = param.IsBeat,
                        CustomerCheckInOutHistories = new List<CustomerCheckInOutHistory>()
                    };
                    actualRoute.CustomerCheckInOutHistories.Add(new CustomerCheckInOutHistory
                    {
                        CheckIn = customerClockTime,
                        CheckOut = null
                    });

                }
            }

            if (mybeat.ActualRoutes == null)
            {
                mybeat.ActualRoutes = new List<ActualRoute>();
            }
            if (existingActualRoute == null)
                mybeat.ActualRoutes.Add(actualRoute);
            else
            {
                mybeat.ActualRoutes.Where(s => param.CustomerId == s.CustomerId).ToList().ForEach(x =>
                {
                    x.CheckIn = StartTime;
                    x.CheckOut = EndTime;
                    if (x.CustomerCheckInOutHistories != null && x.CustomerCheckInOutHistories.Any())
                    {
                        if (x.CustomerCheckInOutHistories.Any(y => !y.CheckOut.HasValue))
                        {
                            x.CustomerCheckInOutHistories.FirstOrDefault(y => !y.CheckOut.HasValue).CheckOut = customerClockTime;
                        }
                        else
                        {
                            x.CustomerCheckInOutHistories.Add(new CustomerCheckInOutHistory
                            {
                                CheckIn = customerClockTime,
                                CheckOut = null
                            });
                        }
                    }
                    else
                    {
                        x.CustomerCheckInOutHistories = new List<CustomerCheckInOutHistory>();
                        x.CustomerCheckInOutHistories.Add(new CustomerCheckInOutHistory
                        {
                            CheckIn = StartTime,
                            CheckOut = null
                        });
                    }
                });
            }
            mybeat.PlannedRoutes.Where(s => param.CustomerId == s.CustomerId).ToList().ForEach(x =>
              {
                  x.IsVisited = true;
                  x.VisitedOn = now;
                  x.TravalStart = StartTime;
              });
            await mongoDbHelper.ReplaceAsync(mybeat.Id, mybeat);

            return StartTime;
        }


        internal async Task<bool> UpdateActualRouteForSkip(DataContracts.External.MobileExecutiveDC.SalesAppRouteParam param)
        {
            var now = DateTime.Now;

            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
            var today = now.Date;
            var peopleId = param.PeopleId;

            var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today).FirstOrDefault();

            if (todayBeats != null)
            {
                if (todayBeats.ActualRoutes.Any(s => s.CustomerId == param.CustomerId))
                {
                    todayBeats.ActualRoutes.FirstOrDefault(s => s.CustomerId == param.CustomerId).Comment = param.Comment;
                    todayBeats.ActualRoutes.FirstOrDefault(s => s.CustomerId == param.CustomerId).ShopCloseImage = param.ShopCloseImage;
                }
                await mongoDbHelper.ReplaceAsync(todayBeats.Id, todayBeats);
            }
            return true;
        }
    }
}