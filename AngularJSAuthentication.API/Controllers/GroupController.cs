using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using static AngularJSAuthentication.API.Controllers.WarehouseController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Group")]
    public class GroupController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);





        [Route("CreateGroupRetailer")]
        [HttpPost]
        public HttpResponseMessage PostgroupRetailer(Group item)
        {

            var user = GetUserId();

            //var groupCheck = db.Customers.Where(x => x. == item.GroupName).FirstOrDefault();
            //if (groupCheck == null) { }
            //else
            //{
            //    if (item.WarehouseID != groupCheck.Warehouseid)
            //    {
            //        item.Message = "Failed";
            //        return Request.CreateResponse(HttpStatusCode.OK, item);
            //    }

            //}



            logger.Info("start CreateGroupRetailer ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<GroupMapping> grpDatas = new List<GroupMapping>();
                    if (item != null)
                    {

                        foreach (var o in item.AllCustomersSK)
                        {
                            grpDatas.Add(new GroupMapping
                            {
                                WarehouseID = o.Warehouseid,
                                GroupID = item.GroupID,
                                CustomerID = o.CustomerId,
                                CreatedDate = indianTime,
                                ModifiedDate = indianTime,
                                CreatedBy = user.UserId,
                                ModifiedBy = user.UserId,
                                IsActive = true,
                                IsDeleted = false,
                            });
                            //db.Entry(grpData).State = EntityState.Modified;
                        }
                        db.GroupMappings.AddRange(grpDatas);
                        if (db.Commit() > 0)
                            return Request.CreateResponse(HttpStatusCode.OK, "Customer Update Successfully!!");
                        else
                            return Request.CreateResponse(HttpStatusCode.OK, "Error occured during save Group customer");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.OK, "Error occured while executing Create Group");
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("RemoveGroupRetailer")]
        [HttpPost]
        public HttpResponseMessage RemoveGroupRetailer(Group item)
        {
            using (AuthContext db = new AuthContext())
            {

                List<int> customerids = item.AllCustomersSK.Select(x => x.CustomerId).ToList();
                List<GroupMapping> list = db.GroupMappings.Where(p => customerids.Any(x => x == p.CustomerID) && p.GroupID == item.GroupID && p.WarehouseID == item.WarehouseID).ToList();

                if (list != null && list.Count > 0)
                {
                    foreach (var customer in list)
                    {
                        GroupMapping groupdata = db.GroupMappings.Where(x => x.Id == customer.Id).FirstOrDefault();
                        groupdata.IsActive = false;
                        groupdata.IsDeleted = true;
                        db.GroupMappings.Attach(groupdata);
                        db.Entry(groupdata).State = EntityState.Modified;
                    }

                    db.Commit();
                }

                //db.Entry(list).State = EntityState.Modified;

                //db.SaveChanges();


                logger.Info("start CreateGroupRetailer ");
                try
                {
                    GroupMapping grpData = new GroupMapping();
                    if (item != null)
                    {

                        foreach (var o in item.AllCustomersSK)
                        {
                            //var data =


                            //grpData.WarehouseID = o.Warehouseid;
                            //grpData.GroupID = item.GroupID;
                            //grpData.CustomerID = o.CustomerId;
                            //grpData.CreatedDate = indianTime;
                            //grpData.ModifiedDate = indianTime;
                            //grpData.CreatedBy = user.UserId;
                            //grpData.ModifiedBy = user.UserId;
                            //grpData.IsActive = false;
                            // db.GroupMappings.Add(grpData);
                            //db.Entry(grpData).State = EntityState.Modified;
                            //db.SaveChanges();

                        }

                        return Request.CreateResponse(HttpStatusCode.OK, "Customer Update Successfully!!");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.OK, "Error occured while executing Create Group");
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        //Get Mapped Customers
        [Route("GetMappedCustomers")]
        public IEnumerable<Customer> GetMappedCustomers()
        {
            claimdata claimdata = GetUserId();

            logger.Info("Start GetMappedCustomers: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<Customer> customerData = new List<Customer>();
                    List<GroupMapping> groupData = new List<GroupMapping>();
                    groupData = db.GroupMappings.Where(x => x.IsActive == true).ToList();

                    foreach (var o in groupData)
                    {
                        var cstData = db.Customers.Where(x => x.Deleted == false && x.CustomerId == o.CustomerID).FirstOrDefault();

                        customerData.Add(cstData);
                    }
                    return customerData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetMappedCustomers " + ex.Message);
                    logger.Info("End  GetMappedCustomers: ");
                    return null;
                }
            }
        }

        //Get UnMapped Customers
        [Route("GetUnMappedCustomers")]
        public IEnumerable<Customer> GetUnMappedCustomers()
        {
            claimdata claimdata = GetUserId();

            logger.Info("Start GetUnMappedCustomers: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var customerData = db.Customers.Where(x => x.Deleted == false).ToList();
                    var groupData = db.GroupMappings.Where(x => x.IsActive == true).ToList();
                    foreach (var o in groupData)
                    {
                        var cstData = db.Customers.Where(x => x.Deleted == false && x.CustomerId == o.CustomerID).FirstOrDefault();
                        customerData.Remove(cstData);
                    }
                    return customerData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetUnMappedCustomers " + ex.Message);
                    logger.Info("End  GetUnMappedCustomers: ");
                    return null;
                }
            }
        }


        //filter Mapped Customer By warehouse ID
        [Route("FilterMappedCustomers")]
        [HttpGet]
        public IEnumerable<Customer> FilterMappedCustomers(int wId)
        {
            claimdata claimdata = GetUserId();

            logger.Info("Start FilterMappedCustomers: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<Customer> customerData = new List<Customer>();
                    List<GroupMapping> groupData = new List<GroupMapping>();
                    groupData = db.GroupMappings.Where(x => x.IsActive == true).ToList();
                    foreach (var o in groupData)
                    {
                        var cstData = db.Customers.Where(x => x.Deleted == false && x.CustomerId == o.CustomerID).FirstOrDefault();
                        customerData.Add(cstData);
                    }
                    var filterData = customerData.Where(x => x.Warehouseid == wId).ToList();
                    return filterData;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }

        //filter Mapped Customer By Group ID
        [Route("FilterMappedCustomersByGroupID")]
        [HttpGet]
        public IEnumerable<Customer> FilterMappedCustomersByGroupID(int wId, int groupId)
        {
            claimdata claimdata = GetUserId();

            logger.Info("Start FilterMappedCustomers: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<Customer> customerData = new List<Customer>();
                    List<GroupMapping> groupData = new List<GroupMapping>();
                    groupData = db.GroupMappings.Where(x => x.IsActive == true && x.GroupID == groupId).ToList();
                    foreach (var o in groupData)
                    {
                        var cstData = db.Customers.Where(x => x.Deleted == false && x.CustomerId == o.CustomerID).FirstOrDefault();
                        customerData.Add(cstData);
                    }
                    var filterData = customerData.Where(x => x.Warehouseid == wId).ToList();
                    return filterData;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [Route("FilterMappedCustomersMappedData")]
        [HttpGet]
        public IEnumerable<Customer> FilterMappedCustomersMappedData(int wId, int groupId, string Status)
        {
            claimdata claimdata = GetUserId();

            logger.Info("Start FilterMappedCustomers: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<Customer> customerData = new List<Customer>();
                    List<GroupMapping> groupData = new List<GroupMapping>();



                    if (Status == "Unmapped")
                    {
                        var notMappedDate = from c in db.Customers.Where(c => c.Warehouseid == wId && c.Active == true)
                                            join p in db.GroupMappings.Where(o => o.WarehouseID == wId && o.GroupID == groupId && o.IsActive && (o.IsDeleted.HasValue && !o.IsDeleted.Value))
                                            on c.CustomerId equals p.CustomerID into ps
                                            from p in ps.DefaultIfEmpty()
                                            where (p.WarehouseID == null)
                                            select c;

                        customerData = notMappedDate.ToList();
                    }
                    else
                    {
                        var mappedData = from d in db.Customers.Where(c => c.Warehouseid == wId && c.Active == true)
                                         join e in db.GroupMappings on d.CustomerId equals e.CustomerID
                                         where (d.Warehouseid == e.WarehouseID && e.GroupID == groupId && d.Warehouseid == wId && e.IsActive && (e.IsDeleted.HasValue && !e.IsDeleted.Value))
                                         select d;
                        customerData = mappedData.ToList();
                    }

                    return customerData;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [Route("GetCity")]
        public object GetCity()
        {
            using (AuthContext db = new AuthContext())
            {
                //var list = db.CitysSms.Distinct().ToList();
                //ass = db.Customers.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.City != null && c.City != "")).OrderBy(c => c.City).Distinct().ToList();
                List<SpecificWarehousesDTO> cityData = db.Database.SqlQuery<SpecificWarehousesDTO>("exec GetActiveWarehouseCity").ToList();
                var distinctcity = cityData.Select(city => new { Source = city.CityName }).Distinct();
                return distinctcity;
            }

        }

        [Route("GetState")]
        public object GetState()
        {

            logger.Info("Start City: ");
            List<Customer> ass = new List<Customer>();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    //var list = db.CitysSms.Distinct().ToList();
                    // ass = db.Customers.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.State != null && c.State != "")).OrderBy(c => c.State).Distinct().ToList();
                    List<SpecificStateDTO> cityData = db.Database.SqlQuery<SpecificStateDTO>("exec GetActiveWarehouseState").ToList();
                    var distinctstate = cityData.Select(state => new { Source = state.StateName }).Distinct();
                    return distinctstate;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer Sms " + ex.Message);
                    logger.Info("End  Customer Sms: ");
                    return null;
                }
            }
        }

        [Route("GetHub")]
        public object GetLocation()
        {
            logger.Info("Start Hub: ");
            // List<Customer> ass = new List<Customer>();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    List<SpecificWarehouseDTO> whs = db.Database.SqlQuery<SpecificWarehouseDTO>("exec GetActiveWarehouse").ToList();
                    var distincthub = whs.Select(hub => new { Source = hub.WarehouseName }).Distinct();
                    //ass = db.Customers.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && c.Warehouseid.HasValue && c.Warehouseid.Value > 0).ToList();
                    // var whIds = ass.Select(x => x.Warehouseid.Value).Distinct().ToList();
                    //var distincthub = db.Warehouses.Where(x => whIds.Contains(x.WarehouseId)).Select(x => new { Source = x.WarehouseName }).ToList();
                    //var distincthub = ass.Select(warehouse => new { Source = warehouse.WarehouseName }).Distinct();
                    logger.Info("End Hub:");
                    return distincthub;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Hub " + ex.Message);
                    logger.Info("End  Hub ");
                    return null;
                }
            }
        }

        [Route("GetCluster")]
        public object GetCluster()
        {
            //claimdata claimdata = GetUserId();


            //logger.Info("Start Hub: ");
            //List<Customer> ass = new List<Customer>();
            //using (AuthContext db = new AuthContext())
            //{
            //    try
            //    {
            //        ass = db.Customers.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.ClusterName != null && c.ClusterName != "")).OrderBy(c => c.ClusterName).Distinct().ToList();
            //        var distinctcluster = ass.Select(cluster => new { Source = cluster.ClusterName }).Distinct();
            //        logger.Info("End Hub:");
            //        return distinctcluster;
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("Error in Hub " + ex.Message);
            //        logger.Info("End  Hub ");
            //        return null;
            //    }
            //}
            return null;
        }

        [Route("GetDepartment")]
        public object GetDepartment()
        {
            claimdata claimdata = GetUserId();


            logger.Info("Start Department: ");
            List<People> ass = new List<People>();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + claimdata.CompId + " and r.Name != 'null' and r.Name !='' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    ass = db.Database.SqlQuery<People>(query).OrderBy(c => c.Department).Distinct().ToList();
                    //ass = db.Peoples.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.Department != null && c.Department != "")).OrderBy(c => c.Department).Distinct().ToList();
                    var distinctdepartment = ass.Select(cluster => new { Source = cluster.Department }).Distinct();
                    logger.Info("End Department:");
                    return distinctdepartment;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Department " + ex.Message);
                    logger.Info("End  Department ");
                    return null;
                }
            }
        }

        //[Route("GetPermissions")]
        //public object GetPermissions()
        //{
        //    claimdata claimdata = GetUserId();


        //    logger.Info("Start Permissions: ");
        //    List<People> ass = new List<People>();
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            ass = db.Peoples.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.Permissions != null && c.Permissions != "")).OrderBy(c => c.Permissions).Distinct().ToList();
        //            var distinctPermissions = ass.Select(cluster => new { Source = cluster.Permissions }).Distinct();
        //            logger.Info("End Permissions:");
        //            return distinctPermissions;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Permissions " + ex.Message);
        //            logger.Info("End  Permissions ");
        //            return null;
        //        }
        //    }
        //}

        [Route("GetDesignations")]
        public object GetDesignations()
        {
            claimdata claimdata = GetUserId();


            logger.Info("Start Desgination: ");
            List<People> ass = new List<People>();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    ass = db.Peoples.Where(c => c.CompanyId == claimdata.CompId && c.Deleted == false && (c.Desgination != null && c.Desgination != "")).OrderBy(c => c.Desgination).Distinct().ToList();
                    var distinctDesgination = ass.Select(cluster => new { Source = cluster.Desgination }).Distinct();
                    logger.Info("End Desgination:");
                    return distinctDesgination;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Desgination " + ex.Message);
                    logger.Info("End  Desgination ");
                    return null;
                }
            }
        }


        // For Retailers
        [Route("SearchCustomerSK")]
        [HttpGet]

        public List<Customer> SearchCustomerSK(int WarehouseName, string GroupName, string State, string City, string ClusterName)
        {
            claimdata claimdata = GetUserId();

            using (AuthContext db = new AuthContext())
            {
                //All Five filters selected
                if ((WarehouseName != 0) && (GroupName != null && GroupName != "null") && (State != null) && (City != null) && (ClusterName != null && ClusterName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.GroupName == (GroupName) && a.State == (State) && a.City == (City) && a.ClusterName.Contains(ClusterName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }

                //Four filters selected


                else if ((WarehouseName != 0) && (GroupName != null && GroupName != "null") && (State != null) && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == GroupName && a.Warehouseid == (WarehouseName) && a.State == (State) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (GroupName != null && GroupName != "null") && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == GroupName && a.Warehouseid == (WarehouseName) && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (State != null) && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.State == (State) && a.Warehouseid == (WarehouseName) && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (State != null) && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == GroupName && a.State == (State) && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (GroupName != null && GroupName != "null") && (State != null) && (ClusterName != null && ClusterName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == GroupName && a.Warehouseid == (WarehouseName) && a.State == (State) && a.ClusterName.Contains(ClusterName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }



                //Three filters selected


                else if ((WarehouseName != 0) && (GroupName != null && GroupName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == GroupName && a.Warehouseid == (WarehouseName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (State != null) && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.State == (State) && a.Warehouseid == (WarehouseName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.ClusterName.Contains(ClusterName) && a.Warehouseid == (WarehouseName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((State != null) && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.ClusterName.Contains(ClusterName) && a.State == (State) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (State != null) && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.State == (State) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (ClusterName != null && ClusterName != "null") && (WarehouseName != 0))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.ClusterName.Contains(ClusterName) && a.Warehouseid == (WarehouseName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (WarehouseName != 0) && (State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.Warehouseid == (WarehouseName) && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (ClusterName != null && ClusterName != "null") && (State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.ClusterName.Contains(ClusterName) && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }



                //Two filter selected

                else if ((WarehouseName != 0) && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((ClusterName != null && ClusterName != "null") && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.ClusterName.Contains(ClusterName) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((State != null) && (City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.State == (State) && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((ClusterName != null && ClusterName != "null") && (State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.ClusterName.Contains(ClusterName) && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (GroupName != null && GroupName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.GroupName == (GroupName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((GroupName != null && GroupName != "null") && (ClusterName != null && ClusterName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName) && a.ClusterName.Contains(ClusterName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((WarehouseName != 0) && (ClusterName != null && ClusterName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName) && a.ClusterName.Contains(ClusterName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }



                //One filter selected

                else if (WarehouseName != 0)
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.Warehouseid == (WarehouseName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if (GroupName != null && GroupName != "null")
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.GroupName == (GroupName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((ClusterName != null && ClusterName != "null"))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.ClusterName.Contains(ClusterName)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((State != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.State == (State)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else if ((City != null))
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId && a.City == (City)).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
                else
                {
                    var listOrders = db.Customers.Where(a => a.Deleted == false && a.CompanyId == claimdata.CompId).OrderBy(x => x.Name).ToList();
                    return listOrders;
                }
            }
        }








        //=======================Function to get claim ID(of loggged in person) Start========================//

        public claimdata GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;
            //Access claims
            foreach (Claim claim in identity.Claims)
            {
                if (claim.Type == "compid")
                {
                    compid = int.Parse(claim.Value);
                }
                if (claim.Type == "userid")
                {
                    userid = int.Parse(claim.Value);
                }
                if (claim.Type == "Warehouseid")
                {
                    Warehouse_id = int.Parse(claim.Value);
                }
            }
            claimdata claimdata = new claimdata
            {
                UserId = userid,
                CompId = compid,
                WarehouseId = Warehouse_id
            };
            return claimdata;
        }

        public class claimdata
        {
            public int UserId { get; set; }
            public int CompId { get; set; }
            public int WarehouseId { get; set; }
        }

        //=======================Function to get claim ID(of loggged in person) End========================//



        string msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15;

        [Authorize]
        [HttpPost]
        public IHttpActionResult GroupUploadFile(int warehouseid, int groupid)
        {

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {

                var formData1 = HttpContext.Current.Request.Form["compid"];
                logger.Info("start Transfer Order Upload Exel File: ");

                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }

                    httpPostedFile.SaveAs(FileUrl);
                    return ReadUnmappedUploadFile(hssfwb, userid, warehouseid, groupid);


                }
            }

            return Created("Error", "Error");
        }
        public IHttpActionResult ReadUnmappedUploadFile(XSSFWorkbook hssfwb, int userid, int warehouseid, int groupid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            List<UnmappedDC> Unmappedlist = new List<UnmappedDC>();

            int? Skcode = null;



            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            {
                if (iRowIdx == 0)
                {
                    rowData = sheet.GetRow(iRowIdx);

                    if (rowData != null)
                    {



                        Skcode = rowData.Cells.Any(x => x.ToString().Trim() == "Skcode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Skcode").ColumnIndex : (int?)null;
                        if (!Skcode.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                            strJSON = objJSSerializer.Serialize("ClusterName does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }

                    }
                }
                else
                {

                    rowData = sheet.GetRow(iRowIdx);
                    DateTime datetoday = DateTime.Today;
                    cellData = rowData.GetCell(0);
                    rowData = sheet.GetRow(iRowIdx);
                    if (rowData != null)
                    {
                        UnmappedDC exceluplaod = new UnmappedDC();



                        cellData = rowData.GetCell(0);
                        col0 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.Skcode = Convert.ToString(col0);
                        logger.Info("Skcode :" + exceluplaod.Skcode);


                        exceluplaod.userid = userid;
                        Unmappedlist.Add(exceluplaod);


                    }

                }
            }
            List<GroupMapping> groupMapping = MappedCustomer(Unmappedlist, userid, warehouseid, groupid);

            if (groupMapping.Count > 0)
            {
                return Created("Success", "Success");
            }
            else
            {
                return Created("Error", "Error");
            }



        }


        public List<GroupMapping> MappedCustomer(List<UnmappedDC> Unmappedlist, int userId, int WarehouseId, int groupid)
        {
            using (var context = new AuthContext())
            {
                List<GroupMapping> groupMapping = new List<GroupMapping>();



                //foreach (var data in Unmappedlist)
                //{
                //    var customerdata = context.Customers.Where(x => x.Skcode == data.Skcode).Select(x => new { x.CustomerId, x.Name }).FirstOrDefault();
                //    groupMapping.Add(new GroupMapping
                //    {

                //        WarehouseID = WarehouseId,
                //        GroupID = groupid,
                //        CustomerID = customerdata.CustomerId,
                //        CreatedDate = indianTime,
                //        ModifiedDate = indianTime,
                //        CreatedBy = userId,
                //        ModifiedBy = userId,
                //        IsActive = true,
                //        IsDeleted = false,
                //    });
                //    //db.Entry(grpData).State = EntityState.Modified;
                //}
                //context.GroupMappings.AddRange(groupMapping);
                foreach (var data in Unmappedlist)
                {



                    var customerdata = context.Customers.Where(x => x.Skcode == data.Skcode).Select(x => new { x.CustomerId, x.Name }).FirstOrDefault();
                    if (customerdata != null)
                    {
                        //var verfy = context.GroupMappings.Where(x => x.CustomerID == customerdata.CustomerId).Any();
                        //if (!verfy)
                        //{
                        GroupMapping mapping = new GroupMapping();
                        mapping.CustomerID = customerdata.CustomerId;
                        mapping.CustomerName = customerdata.Name;
                        mapping.CreatedDate = DateTime.Now;
                        mapping.IsActive = true;
                        mapping.IsDeleted = false;
                        mapping.WarehouseID = WarehouseId;
                        mapping.GroupID = groupid;
                        mapping.CreatedBy = userId;
                        groupMapping.Add(mapping);

                        //}
                    }

                }
                context.GroupMappings.AddRange(groupMapping);
                context.Commit();
                return groupMapping;
            }

        }
        public class UnmappedDC
        {

            [Required]
            [StringLength(100)]
            [Index("IX_Customers_SkCode", 1, IsUnique = true)]
            public string Skcode { get; set; }

            public int? userid { get; set; }
        }
    }
}
