using AgileObjects.AgileMapper;
using AngularJSAuthentication.BusinessLayer.Managers;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.UserRating;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Rating;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Rating
{
    [AllowAnonymous]
    [RoutePrefix("api/UserRating")]
    public class UserRatingController : ApiController
    {
        [Route("GetUserRating")]
        [HttpGet]
        public UserRatingResDC GetUserRating(int AppType,string key,int skip,int take)
        {
            int Skiplist = (skip - 1) * take;
            UserRatingResDC res = new UserRatingResDC();
            using (var db = new AuthContext())
            {
                List<UserRatingBackendDC> list = new List<UserRatingBackendDC>();
                int userRatingCount = 0;
                var predicate = PredicateBuilder.True<UserRating>();
                if(AppType != 1)
                {
                    predicate = predicate.And(x => x.IsActive && x.IsDeleted != true && x.IsRemoveFront != true && x.AppType == AppType && x.IsTrip == false);

                    if (AppType > 0)
                    {
                        predicate = predicate.And(x => x.AppType == AppType);
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        var keyName = db.Peoples.Where(x => x.DisplayName.Contains(key) || x.PeopleFirstName.Contains(key) || x.Mobile.Contains(key) || x.Skcode.Contains(key)).Select(x => new { x.PeopleID, x.DisplayName, x.PeopleFirstName, x.Mobile, x.Skcode }).FirstOrDefault();
                        predicate = predicate.And(x => x.OrderId.ToString().Contains(key) || x.UserId.ToString().Contains((keyName.PeopleID).ToString()));
                    }
                    var userRatings = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).Skip(Skiplist).Take(take).ToList();
                    userRatingCount = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).Count();
                    var peopleids = userRatings.Select(x => x.UserId).ToList();
                    var people = db.Peoples.Where(x => peopleids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName, x.PeopleFirstName }).ToList();
                    if (userRatings != null && userRatings.Any())
                    {
                        list = Mapper.Map(userRatings).ToANew<List<UserRatingBackendDC>>();
                        list.ForEach(x =>
                        {
                            //var name = people.Where(y => y.PeopleID == x.UserId).FirstOrDefault().DisplayName.ToString();
                            //x.UserName = name;
                            var name = people.Where(y => y.PeopleID == x.UserId).FirstOrDefault();
                            if (name != null)
                            {
                                x.UserName = name.DisplayName;
                            }
                            if (x.AppType == 1)
                            {
                                x.AppTypeName = "Sales Rating";
                            }
                            else if (x.AppType == 3)
                            {
                                x.AppTypeName = "Retailer Rating";
                            }
                            else if (x.AppType == 2)
                            {
                                x.AppTypeName = "Delivery Rating";
                            }
                        });
                    }
                }
                else
                {
                    predicate = predicate.And(x => x.IsActive && x.IsDeleted != true && x.IsRemoveFront != true && x.AppType == AppType && x.IsTrip == true);

                    if (AppType > 0)
                    {
                        predicate = predicate.And(x => x.AppType == AppType);
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        var keyName = db.Customers.Where(x => x.Name.Contains(key) || x.Mobile.Contains(key) || x.Skcode.Contains(key)).Select(x => new { x.CustomerId, x.Name, x.Mobile, x.Skcode }).FirstOrDefault();
                        predicate = predicate.And(x => x.OrderId.ToString().Contains(key) || x.UserId.ToString().Contains((keyName.CustomerId).ToString()));
                    }
                    var userRatings = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).Skip(Skiplist).Take(take).ToList();
                    userRatingCount = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).Count();
                    var peopleids = userRatings.Select(x => x.UserId).ToList();
                    var people = db.Customers.Where(x => peopleids.Contains(x.CustomerId)).Select(x => new { x.CustomerId, x.Name}).ToList();
                    if (userRatings != null && userRatings.Any())
                    {
                        list = Mapper.Map(userRatings).ToANew<List<UserRatingBackendDC>>();
                        list.ForEach(x =>
                        {
                            var name = people.Where(y => y.CustomerId == x.UserId).FirstOrDefault();
                            if (name != null)
                            {
                                x.UserName = name.Name;
                            }
                            if (x.AppType == 1)
                            {
                                x.AppTypeName = "Sales Rating";
                            }
                            else if (x.AppType == 3)
                            {
                                x.AppTypeName = "Retailer Rating";
                            }
                            else if (x.AppType == 2)
                            {
                                x.AppTypeName = "Delivery Rating";
                            }
                        });
                    }
                }
                
                res.UserRating = list;
                //res.TotalCount = db.UserRatings.Where(x => x.AppType == AppType && x.IsActive == true && x.IsDeleted == false && x.IsRemoveFront == false).Count();
                res.TotalCount = userRatingCount;
                return res;
            }
        }

        #region Export UserRating
        [Route("ExportUserRating")]
        [HttpGet]
        public UserRatingResDC ExportUserRating(int AppType, string key)
        {
            UserRatingResDC res = new UserRatingResDC();
            using (var db = new AuthContext())
            {
                List<UserRatingBackendDC> list = new List<UserRatingBackendDC>();
                int ratingCount = 0;
                var predicate = PredicateBuilder.True<UserRating>();
                if (AppType != 1)
                {
                    predicate = predicate.And(x => x.IsActive && x.IsDeleted != true && x.IsRemoveFront != true && x.AppType == AppType && x.IsTrip == false);

                    if (AppType > 0)
                    {
                        predicate = predicate.And(x => x.AppType == AppType);
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        var keyName = db.Peoples.Where(x => x.DisplayName.Contains(key) || x.PeopleFirstName.Contains(key) || x.Mobile.Contains(key) || x.Skcode.Contains(key)).Select(x => new { x.PeopleID, x.DisplayName, x.PeopleFirstName, x.Mobile, x.Skcode }).FirstOrDefault();
                        predicate = predicate.And(x => x.OrderId.ToString().Contains(key) || x.UserId.ToString().Contains((keyName.PeopleID).ToString()));
                    }
                    var userRatings = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).ToList();
                    ratingCount = userRatings.Count();
                    var peopleids = userRatings.Select(x => x.UserId).ToList();
                    var people = db.Peoples.Where(x => peopleids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName, x.PeopleFirstName }).ToList();
                    if (userRatings != null && userRatings.Any())
                    {
                        list = Mapper.Map(userRatings).ToANew<List<UserRatingBackendDC>>();
                        list.ForEach(x =>
                        {
                            var name = people.Where(y => y.PeopleID == x.UserId).FirstOrDefault();
                            if (name != null)
                            {
                                x.UserName = name.DisplayName;
                            }
                            if (x.AppType == 1)
                            {
                                x.AppTypeName = "Sales Rating";
                            }
                            else if (x.AppType == 3)
                            {
                                x.AppTypeName = "Retailer Rating";
                            }
                            else if (x.AppType == 2)
                            {
                                x.AppTypeName = "Delivery Rating";
                            }
                        });
                    }
                }
                else
                {
                    predicate = predicate.And(x => x.IsActive && x.IsDeleted != true && x.IsRemoveFront != true && x.AppType == AppType && x.IsTrip == true);

                    if (AppType > 0)
                    {
                        predicate = predicate.And(x => x.AppType == AppType);
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        var keyName = db.Customers.Where(x => x.Name.Contains(key) || x.Mobile.Contains(key) || x.Skcode.Contains(key)).Select(x => new { x.CustomerId, x.Name, x.Mobile, x.Skcode }).FirstOrDefault();
                        predicate = predicate.And(x => x.OrderId.ToString().Contains(key) || x.UserId.ToString().Contains((keyName.CustomerId).ToString()));
                    }
                    var userRatings = db.UserRatings.Where(predicate).Include(x => x.UserRatingDetails).OrderByDescending(x => x.CreatedDate).ToList();
                    ratingCount = userRatings.Count();
                    var peopleids = userRatings.Select(x => x.UserId).ToList();
                    var people = db.Customers.Where(x => peopleids.Contains(x.CustomerId)).Select(x => new { x.CustomerId, x.Name}).ToList();
                    if (userRatings != null && userRatings.Any())
                    {
                        list = Mapper.Map(userRatings).ToANew<List<UserRatingBackendDC>>();
                        list.ForEach(x =>
                        {
                            var name = people.Where(y => y.CustomerId == x.UserId).FirstOrDefault();
                            if (name != null)
                            {
                                x.UserName = name.Name;
                            }
                            if (x.AppType == 1)
                            {
                                x.AppTypeName = "Sales Rating";
                            }
                            else if (x.AppType == 3)
                            {
                                x.AppTypeName = "Retailer Rating";
                            }
                            else if (x.AppType == 2)
                            {
                                x.AppTypeName = "Delivery Rating";
                            }
                        });
                    }
                }
                res.UserRating = list;
                res.TotalCount = ratingCount;
                return res;
            }
        }
        #endregion

        #region GetCityWiseHubList
        [Route("GetCityWiseHubList")]
        [HttpPost]
        public List<Warehouse> GetCityWiseHubList(List<int> cityIds)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();
                List<Warehouse> warehouseList = db.Warehouses.Where(x => cityIds.Contains(x.Cityid) && x.Deleted == false && x.active == true).ToList();
                return warehouseList;
            }
        }
        #endregion

        #region DBoyRating
        [Route("GetHubWiseDboyList")]
        [HttpPost]
        public async Task<List<DboyDC>> GetHubWiseDboyList(WarehouseIdDC wIds)
        {
            using (AuthContext db = new AuthContext())
            {
                List<DboyDC> DboyDCData = new List<DboyDC>();
                var manager = new UserRatingManager();
                DboyDCData = manager.GetDboyListHubWise(wIds);
                return DboyDCData;
            }
        }

        [Route("GetHubWiseDboyRatingList")]
        [HttpPost]
        public async Task<PaggingRatingrData> GetHubWiseDboyRatingList(DboyRatingFilter dboyRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int skip = (dboyRatingFilter.Skip - 1) * dboyRatingFilter.Take;
                int take = dboyRatingFilter.Take;
                int totalItems = 0;
                List<DboyRatingDC> dBoyRatingList = new List<DboyRatingDC>();
                var manager = new UserRatingManager();
                dBoyRatingList = manager.GetHubWiseDboyRatingList(dboyRatingFilter, skip, take, out totalItems);
                    paggingData.RatingDC = dBoyRatingList;
                    paggingData.total_count = totalItems;
                return paggingData;
            }
        }

        [Route("GetExportHubWiseDboyRatingList")]
        [HttpPost]
        public PaggingRatingrData GetExportHubWiseDboyRatingList(DboyRatingFilter dboyRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int totalItems = 0;
                List<DboyRatingDC> dBoyRatingList = new List<DboyRatingDC>();
                var manager = new UserRatingManager();
                dBoyRatingList = manager.GetExportHubWiseDboyRatingList(dboyRatingFilter, out totalItems);
                    paggingData.RatingDC = dBoyRatingList;
                    paggingData.total_count = totalItems;
                return paggingData;
            }
        }

        #endregion

        #region SalesRating
        [Route("GetHubWiseClusterList")]
        [HttpPost]
        public List<Cluster> GetHubWiseClusterList(WarehouseIdDC hubIds)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> clusterList = new List<Cluster>();
                //List<Cluster> clusterList = db.Clusters.Where(x => hubIds.Contains(x.WarehouseId) && x.Deleted == false && x.Active == true).ToList();
                var manager = new UserRatingManager();
                clusterList = manager.GetClusterListHubWise(hubIds);
                return clusterList;
            }
        }

        [Route("GetHubWiseSalesPersonList")]
        [HttpPost]
        public async Task<List<SalesPersonDC>> GetHubWiseSalesPersonList(WarehouseIdDC wIds)
        {
            using (AuthContext db = new AuthContext())
            {
                List<SalesPersonDC> SalesPersonData = new List<SalesPersonDC>();
                var manager = new UserRatingManager();
                SalesPersonData = manager.GetSalesPersonListHubWise(wIds);
                return SalesPersonData;
            }
        }

        [Route("GetHubWiseSalesPersonRatingList")]
        [HttpPost]
        public PaggingRatingrData GetHubWiseSalesPersonRatingList(SalesRatingFilter salesRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int skip = (salesRatingFilter.Skip - 1) * salesRatingFilter.Take;
                int take = salesRatingFilter.Take;
                int totalItems = 0;
                List<SalesRatingDC> salesRatingList = new List<SalesRatingDC>();
                var manager = new UserRatingManager();
                salesRatingList = manager.GetHubWiseSalesPersonRatingList(salesRatingFilter, skip, take, out totalItems);
                paggingData.RatingDC = salesRatingList;
                paggingData.total_count = totalItems;
                return paggingData;
            }
        }

        [Route("GetExportHubWiseSalesPersonRatingList")]
        [HttpPost]
        public PaggingRatingrData GetExportHubWiseSalesPersonRatingList(SalesRatingFilter salesRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int totalItems = 0;
                List<SalesRatingDC> salesRatingList = new List<SalesRatingDC>();
                var manager = new UserRatingManager();
                salesRatingList = manager.GetExportHubWiseSalesPersonRatingList(salesRatingFilter, out totalItems);

                    paggingData.RatingDC = salesRatingList;
                    paggingData.total_count = totalItems;

                return paggingData;
            }
        }
        #endregion

        #region CustomerRating
        [Route("GetHubWiseCustomerRatingList")]
        [HttpPost]
        public PaggingRatingrData GetHubWiseCustomerRatingList(CustomerRatingFilter customerRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int skip = (customerRatingFilter.Skip - 1) * customerRatingFilter.Take;
                int take = customerRatingFilter.Take;
                int totalItems = 0;
                List<CustomerRatingDC> customerRatingList = new List<CustomerRatingDC>();
                var manager = new UserRatingManager();
                customerRatingList = manager.GetHubWiseCustomerRatingList(customerRatingFilter, skip, take, out totalItems);

                    paggingData.RatingDC = customerRatingList;
                    paggingData.total_count = totalItems;

                return paggingData;
            }
        }

        [Route("GetExportHubWiseCustomerRatingList")]
        [HttpPost]
        public PaggingRatingrData GetExportHubWiseCustomerRatingList(CustomerRatingFilter customerRatingFilter)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingRatingrData paggingData = new PaggingRatingrData();
                int totalItems = 0;
                List<CustomerRatingDC> customerRatingList = new List<CustomerRatingDC>();
                var manager = new UserRatingManager();
                customerRatingList = manager.ExportHubWiseCustomerRatingList(customerRatingFilter, out totalItems);

                    paggingData.RatingDC = customerRatingList;
                    paggingData.total_count = totalItems;

                return paggingData;
            }
        }
        #endregion

    }
}
