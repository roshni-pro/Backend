using AngularJSAuthentication.Model;using NLog;using System;using System.Collections.Generic;using System.Data.Entity;
using System.Linq;using System.Net;using System.Net.Http;using System.Security.Claims;using System.Transactions;
using System.Web.Http;namespace AngularJSAuthentication.API.Controllers{    [RoutePrefix("api/CustSupplier")]    public class CustSupplierController : ApiController    {        private static Logger logger = LogManager.GetCurrentClassLogger();        [Route("customer")]        public HttpResponseMessage get(int CityId, int Warehouseid, string SubsubCode)        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0; string SUPPLIERCODES = "";
                    // Access claims
                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "SUPPLIERCODES")                        {                            SUPPLIERCODES = Convert.ToString(claim.Value);                        }                    }                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                    int CompanyId = compid;                    var clist = context.getcust2assin(CityId, Warehouseid, SubsubCode, CompanyId);                    if (clist == null)                    {                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Customers");                    }                    return Request.CreateResponse(HttpStatusCode.OK, clist);                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("getrequest")]        public HttpResponseMessage get1(int cid) //Request
        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0; string SUPPLIERCODES = "";
                    // Access claims
                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "SUPPLIERCODES")                        {                            SUPPLIERCODES = Convert.ToString(claim.Value);                        }                    }                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                    if (cid == compid)                    {                        var clist = context.GetcmRequest();                        if (clist == null)                        {                            return Request.CreateResponse(HttpStatusCode.OK, "No Requests");                        }                        return Request.CreateResponse(HttpStatusCode.OK, clist);                    }                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("mycustomer")]        public HttpResponseMessage getmycustomer() //get customers by cluster
        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0; int Warehouse_id = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "Warehouseid")                        {                            Warehouse_id = int.Parse(claim.Value);                        }                    }                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                    if (Warehouse_id > 0)                    {                        var clist = context.getmycustomerWid(compid, Warehouse_id);                        if (clist == null)                        {                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No Customers");                        }                        return Request.CreateResponse(HttpStatusCode.OK, clist);                    }                    else                    {                        var clist = context.getmycustomer(compid);                        if (clist == null)                        {                            return Request.CreateResponse(HttpStatusCode.BadRequest, "No Customers");                        }                        return Request.CreateResponse(HttpStatusCode.OK, clist);                    }                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("AddCustSupplier")]        public HttpResponseMessage post(int wid, List<CustSupplier> obj)        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0;                    int Warehouse_id = 0;                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "Warehouseid")                        {                            Warehouse_id = int.Parse(claim.Value);                        }                    }                    if (Warehouse_id > 0)                    {                        var CustSuppliers = context.addcustsuppliermapping(obj, compid, Warehouse_id);                        if (CustSuppliers == null)                        {                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");                        }                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);                    }                    else                    {                        return Request.CreateResponse(HttpStatusCode.BadRequest, "got error");                    }                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("request")]        public HttpResponseMessage post(CustSupplierRequest obj, int wId)        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0;                    int Warehouse_id = 0;                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "Warehouseid")                        {                            Warehouse_id = int.Parse(claim.Value);                        }                    }                    obj.CompanyId = compid;                    obj.WarehouseId = Warehouse_id;                    if (Warehouse_id > 0)                    {                        var CustSuppliers = context.addCustSupplierRequest(obj);                        if (CustSuppliers == null)                        {                            return Request.CreateResponse(HttpStatusCode.OK, "Already Requested");                        }                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);                    }                    else                    {                        return Request.CreateResponse(HttpStatusCode.BadRequest, "got an eerror");                    }                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("requestresp")]        public HttpResponseMessage put(CustSupplierRequest obj)        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0;                    int Warehouse_id = 0;                    foreach (Claim claim in identity.Claims)                    {                        if (claim.Type == "compid")                        {                            compid = int.Parse(claim.Value);                        }                        if (claim.Type == "userid")                        {                            userid = int.Parse(claim.Value);                        }                        if (claim.Type == "Warehouseid")                        {                            Warehouse_id = int.Parse(claim.Value);                        }                    }                    obj.WarehouseId = Warehouse_id;                    obj.CompanyId = compid;                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                    if (Warehouse_id > 0)                    {                        var CustSuppliers = context.addCustSupplierRequestput(obj);                        if (CustSuppliers == null)                        {                            return Request.CreateResponse(HttpStatusCode.OK, "Already Requested");                        }                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);                    }                    else                    {                        return Request.CreateResponse(HttpStatusCode.BadRequest, "error");                    }                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("Deactivate")]        [HttpPost]        public HttpResponseMessage post1(List<CustSupplier> obj)        {            using (var context = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0, Warehouse_id = 0;                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                    if (Warehouse_id > 0)                    {                        var CustSuppliers = context.RemoveCustSupplier(obj, Warehouse_id);                        if (CustSuppliers == null)                        {                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");                        }                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);                    }                    else                    {                        var CustSuppliers = context.RemoveCustSupplier(obj, Warehouse_id);                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);                    }                }                catch (Exception ex)                {                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);                }            }        }        [Route("Assighn")]        [HttpPut]        public HttpResponseMessage postasgn(Customer Customer)        {            using (var context = new AuthContext())            {                var identity = User.Identity as ClaimsIdentity;                int compid = 0, userid = 0;                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                Customer.CompanyId = compid;                bool Isupdated = context.addassighncustsuppliers(Customer);                if (Isupdated)                {                    return Request.CreateResponse(HttpStatusCode.OK, Isupdated);                }                return Request.CreateResponse(HttpStatusCode.OK, Isupdated);            }        }        [Route("AssignMultiple")]        [HttpPut]        public HttpResponseMessage AssignMultiple(ExecutiveAssignmentDC ExecutiveAssignment)        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, false);
            //try
            //{
            //    bool Result = false;
            //    using (AuthContext context = new AuthContext())
            //    {

            //        TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            //        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            //        var identity = User.Identity as ClaimsIdentity;
            //        int compid = 0, userid = 0;

            //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
            //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

            //        foreach (Customer c in ExecutiveAssignment.customerList)
            //        {
            //            var customer = context.Customers.FirstOrDefault(x => x.Skcode == c.Skcode);
            //            //var exec = context.Peoples.FirstOrDefault(x => x.PeopleID == c.ExecutiveId);
            //            var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == exec.WarehouseId);

            //            if (customer != null)
            //            {
            //                //customer.BeatNumber = c.BeatNumber;
            //                //customer.Day = c.Day;
            //                //customer.ExecutiveName = ExecutiveAssignment.ExecutiveName;
            //                //customer.ExecutiveId = ExecutiveAssignment.ExecutiveId;
            //                customer.UpdatedDate = indianTime;
            //                customer.AgentCode = ExecutiveAssignment.AgentCode.ToString();
            //                customer.ClusterId = ExecutiveAssignment.ClusterId;
            //                customer.ClusterName = ExecutiveAssignment.ClusterName;
            //                //customer.Warehouseid = exec != null ? exec.WarehouseId : 0;
            //                customer.WarehouseName = warehouse.WarehouseName;
            //                //customer.Cityid = exec != null ? warehouse.Cityid : 0;
            //                customer.City = warehouse.CityName;
            //                context.Entry(customer).State = EntityState.Modified;
            //                int id = context.Commit();
            //            }
            //        }
            //        context.SaveChanges();
            //        return Request.CreateResponse(HttpStatusCode.OK, true);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.InternalServerError, false);
            //    throw ex;

            //}
        }

        [Route("Search")]        [HttpPost]        public PaggingData GetOrderAdvanceSearch(FilterCustomerDTO filterOrderDTO)        {            logger.Info("start GetOrderAdvanceSearch: ");            PaggingData paggingData = new PaggingData();            try            {                var identity = User.Identity as ClaimsIdentity;                int compid = 0, userid = 0;                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);                using (AuthContext context = new AuthContext())                {                    int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;                    int take = filterOrderDTO.ItemPerPage;                    string whereclause = compid > 0 ? " and o.CompanyId = " + compid : "";                    if (filterOrderDTO.WarehouseId > 0)                        whereclause += " and o.WarehouseId = " + filterOrderDTO.WarehouseId;                    if (filterOrderDTO.Cityid > 0)                        whereclause += " and o.Cityid = " + filterOrderDTO.Cityid;                    if (!string.IsNullOrEmpty(filterOrderDTO.Mobile))                        whereclause += " and o.Mobile Like " + "'%" + filterOrderDTO.Mobile + "%'";                    if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))                        whereclause += " and o.Skcode Like " + "'%" + filterOrderDTO.Skcode + "%'";                    if (filterOrderDTO.ClusterId > 0)                        whereclause += " and o.ClusterId = " + filterOrderDTO.ClusterId;                    if (filterOrderDTO.start.HasValue && filterOrderDTO.end.HasValue)                        whereclause += " and (o.CreatedDate >= " + "'" + filterOrderDTO.start.Value.ToString("yyyy-MM-dd hh':'mm':'ss") + "'" + " And  o.CreatedDate <=" + "'" + filterOrderDTO.end.Value.ToString("yyyy-MM-dd hh':'mm':'ss") + "')";


                    //var sqlquery = "Select o.ClusterId, o.Skcode,o.CustomerId,o.ShopName,o.Mobile,o.BillingAddress,o.BeatNumber,o.ExecutiveId,o.AgentCode,o.ClusterName,o.Day,o.Active,o.CreatedDate,j.DisplayName,w.WarehouseName " +
                    //               " from Customers o join People j on o.ExecutiveId=j.PeopleID inner join Warehouses w on w.WarehouseId=o.Warehouseid where o.Deleted = 0 " + whereclause
                    //               + " Order by o.CustomerId desc offset " + skip + " rows fetch next " + take + " rows only";



                    //string sqlCountQuery = "Select Count(*) from Customers o join People j on o.ExecutiveId=j.PeopleID inner join Warehouses w on w.WarehouseId=o.Warehouseid where o.Deleted = 0  " + whereclause;


                    var sqlquery = "Select o.ClusterId, o.Skcode,o.CustomerId,o.ShopName,o.Mobile,o.BillingAddress,o.ClusterName,o.Active,o.CreatedDate,w.WarehouseName,w.WarehouseId " +
" from Customers o inner join Warehouses w on w.WarehouseId=o.Warehouseid where o.Deleted = 0 " + whereclause
+ " Order by o.CustomerId desc offset " + skip + " rows fetch next " + take + " rows only";                    string sqlCountQuery = "Select Count(*) from Customers o with(nolock) where o.Deleted = 0  " + whereclause;                    var newdata = context.Database.SqlQuery<CustomerDTOM>(sqlquery).ToList();
                    // return newdata;

                    int dataCount = context.Database.SqlQuery<int>(sqlCountQuery).FirstOrDefault();                    paggingData.total_count = 0;                    if (newdata != null && newdata.Any())                    {                        paggingData.total_count = dataCount;                        var orderids = newdata.Select(x => x.CustomerId).ToList();                    }                    paggingData.ordermaster = newdata;                }                return paggingData;            }            catch (Exception ex)            {                logger.Error("Error in GetOrderSearch " + ex.Message);                logger.Info("End  GetOrderSearch: ");                return null;            }        }        [Route("export")]        [HttpPost]        public dynamic export(FilterCustomerDTO filterOrderDTO)        {            logger.Info("start City: ");
            // dynamic customer = null;
            using (AuthContext db = new AuthContext())            {                try                {                    var identity = User.Identity as ClaimsIdentity;                    int compid = 0, userid = 0;                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);                    int CompanyId = compid;                    string whereclause = compid > 0 ? " and o.CompanyId = " + compid : "";                    if (filterOrderDTO.WarehouseId > 0)                        whereclause += " and o.WarehouseId = " + filterOrderDTO.WarehouseId;                    if (filterOrderDTO.Cityid > 0)                        whereclause += " and o.Cityid = " + filterOrderDTO.Cityid;                    if (!string.IsNullOrEmpty(filterOrderDTO.Mobile))                        whereclause += " and o.Mobile Like " + "'%" + filterOrderDTO.Mobile + "%'";                    if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))                        whereclause += " and o.Skcode Like " + "'%" + filterOrderDTO.Skcode + "%'";                    if (filterOrderDTO.ClusterId > 0)                        whereclause += " and o.ClusterId = " + filterOrderDTO.ClusterId;                    if (filterOrderDTO.start.HasValue && filterOrderDTO.end.HasValue)                        whereclause += " and (o.CreatedDate >= " + "'" + filterOrderDTO.start.Value.ToString("yyyy-MM-dd HH':'mm':'ss") + "'" + " And  o.CreatedDate <=" + "'" + filterOrderDTO.end.Value.ToString("yyyy-MM-dd HH':'mm':'ss") + "')";

                    //var sqlquery = "Select o.Skcode,o.CustomerId,o.ShopName,w.WarehouseName,o.Mobile,o.BillingAddress,o.BeatNumber,o.ExecutiveId,o.ExecutiveName,j.AgentCode,o.ClusterName,o.Day,o.Active,o.CreatedDate,j.DisplayName as AgentName,o.ClusterId " +
                    //               " from Customers o join People j on o.ExecutiveId=j.PeopleID inner join Warehouses w on w.WarehouseId=o.Warehouseid where o.Deleted = 0 " + whereclause
                    //               + " Order by o.CustomerId desc";

                    var sqlquery = "Select o.Skcode,o.CustomerId,o.ShopName,w.WarehouseName, STRING_AGG(j.DisplayName,',') As ExecutiveName, o.ClusterName, o.Active,o.CreatedDate,o.ClusterId  " +
"  from Customers o inner join CustomerExecutiveMappings cse on o.CustomerId=cse.CustomerId join People j on cse.ExecutiveId=j.PeopleID  inner join Warehouses w on w.WarehouseId=o.Warehouseid and  o.Deleted = 0 " + whereclause
+ " Group by o.Skcode,o.CustomerId,o.ShopName,w.WarehouseName,o.ClusterName,o.Active,o.CreatedDate,o.ClusterId ";                    var newdata = db.Database.SqlQuery<CustomerDTOM>(sqlquery).ToList();                    return newdata;                }                catch (Exception ex)                {                    logger.Error("Error in Customer " + ex.Message);                    logger.Info("End  Customer: ");                    return null;                }            }        }





























































        /// <summary>        /// Get Warehouse Agent        /// </summary>        /// <param name="WarehouseId"></param>        /// <returns></returns>        [Route("Agents")]        public HttpResponseMessage GetAgent(int WarehouseId)        {            using (var context = new AuthContext())            {                var identity = User.Identity as ClaimsIdentity;                int compid = 0, userid = 0;                foreach (Claim claim in identity.Claims)                {                    if (claim.Type == "compid")                    {                        compid = int.Parse(claim.Value);                    }                    if (claim.Type == "userid")                    {                        userid = int.Parse(claim.Value);                    }                }                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email and p.Deleted=0  inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and p.WarehouseId=" + WarehouseId + " and r.Name='Agent' and ur.isActive=1 and p.Active=1 and p.Deleted=0";                List<int> PeopleId = context.Database.SqlQuery<int>(query).ToList();                var person = (from a in context.Peoples                              where (PeopleId.Contains(a.PeopleID))                              join i in context.ClusterAgent on a.PeopleID equals i.AgentId into ps                              from i in ps.DefaultIfEmpty()                              select new AgentDTO                              {                                  AgentId = i.AgentId,                                  WarehouseId = a.WarehouseId,                                  ClusterId = i.ClusterId,                                  AgentName = a.DisplayName                              }).ToList();
                //var person = context.Peoples.Where(p => PeopleId.Contains(p.PeopleID)).ToList().OrderBy(x => x.DisplayName).ToList();
                foreach (var a in person)                {                    if (a.AgentId == null) { a.AgentId = 0; }                }                return Request.CreateResponse(HttpStatusCode.OK, person);            }        }


        #region Get all  GetExecutive         /// <summary>                                               /// Created Date 19/04/2019                                               /// </summary>                                               /// <returns>displist</returns>        [Route("GetExecutive")]        public HttpResponseMessage GetExecutive(int WarehouseId)        {            using (var db = new AuthContext())            {                var identity = User.Identity as ClaimsIdentity;                int compid = 0, userid = 0;                List<People> displist = new List<People>();                foreach (Claim claim in identity.Claims)                {                    if (claim.Type == "compid")                    {                        compid = int.Parse(claim.Value);                    }                    if (claim.Type == "userid")                    {                        userid = int.Parse(claim.Value);                    }                }                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + WarehouseId + " and p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";                displist = db.Database.SqlQuery<People>(query).ToList();                logger.Info("End  Sales Executive: ");                return Request.CreateResponse(HttpStatusCode.OK, displist);            }        }


        #endregion
        public class FilterCustomerDTO        {            public int ItemPerPage { get; set; }            public int PageNo { get; set; }            public int WarehouseId { get; set; }            public int Cityid { get; set; }            public DateTime? start { get; set; }            public DateTime? end { get; set; }            public int OrderId { get; set; }            public string Skcode { get; set; }            public string Mobile { get; set; }            public int? ClusterId { get; set; }        }        public class AgentDTO        {            public int? AgentId { get; set; }            public int WarehouseId { get; set; }            public int? ClusterId { get; set; }            public string AgentName { get; set; }        }

        public class ExecutiveAssignmentDC        {            public int? ExecutiveId { get; set; }            public int? AgentCode { get; set; }            public List<Customer> customerList { get; set; }            public string ExecutiveName { get; set; }
            public int? ClusterId { get; set; }
            public string ClusterName { get; set; }        }    }}