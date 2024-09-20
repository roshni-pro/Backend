using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.SignalR;
using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SalesAppCounter")]
    public class SalesAppCounterController : BaseAuthController
    {
        int compid = 0, userid = 0;
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public SalesAppCounterController()
        {
            var identity = User.Identity as ClaimsIdentity;
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
            }

        }

        //[ResponseType(typeof(SalesAppCounter))]
        //[Route("")]
        //[AcceptVerbs("POST")]
        //[AllowAnonymous]
        //public HttpResponseMessage addsales(SalesAppCounterDc sale)
        //{



        //    logger.Info("start addSaleAppCounter: ");

        //    try
        //    {

        //        SalesAppCounter sales = new SalesAppCounter
        //        {
        //            Date = indianTime,
        //            Deleted = false,
        //            lat = sale.lat,
        //            Long = sale.Long,
        //            SalesPersonId = sale.SalesPersonId
        //        };


        //        logger.Info("End  addCustomer: ");


        //        using (var db = new AuthContext())
        //        {
        //            db.SalesAppCounterDB.Add(sales);
        //            db.Commit();

        //            SalesAppCounterDTO MUData = new SalesAppCounterDTO()
        //            {
        //                MUget = sales,
        //                Status = true,
        //                Message = " Added suscessfully."
        //            };
        //            //var query = @"select p.peopleId as SalesPersonId, p.Mobile, p.PeopleFirstName, p.PeopleLastName, p.Email, w.WarehouseName, w.WarehouseId from people p inner join Warehouses w on w.WarehouseId = p.WarehouseId where p.peopleId=#salesPersonID#";
        //            //query = query.Replace("#salesPersonID#", sale.SalesPersonId.ToString());

        //            InitialPoint initialPoint = new InitialPoint()
        //            {

        //                lat = sale.lat,
        //                Long = sale.Long,
        //                Mobile = sale.Mobile,
        //                PeopleFirstName = sale.PeopleFirstName,
        //                PeopleLastName = sale.PeopleLastName,
        //                WarehouseId = sale.WarehouseId,
        //                WarehouseName = sale.WarehouseName,
        //                SalesPersonId = sale.SalesPersonId
        //            };

        //            //var client = new SignalRMasterClient(DbConstants.URL + "signalr");
        //            //// Send message to server.
        //            //string message = JsonConvert.SerializeObject(initialPoint);
        //            //client.SayHello(message, initialPoint.WarehouseId.ToString());
        //            //client.Stop();
        //            string message = JsonConvert.SerializeObject(initialPoint);
        //            ChatFeed.SendChatMessage(message, initialPoint.WarehouseId.ToString());

        //            return Request.CreateResponse(HttpStatusCode.OK, MUData);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SalesAppCounterDTO MUData = new SalesAppCounterDTO()
        //        {
        //            MUget = null,
        //            Status = false,
        //            Message = "Something Went Wrong."
        //        };

        //        logger.Error("Error in Add data salesperson " + ex.Message);
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, MUData);
        //    }
        //}
        //#endregion

        [HttpGet]
        [Route("GetInitialPoint/warehouseid/{warehouseid}")]

        public IHttpActionResult GetInitialPoint(int warehouseid)
        {
            var query = @"select* from(select s.SalesPersonId, lat, Long,p.Mobile, p.PeopleFirstName, p.PeopleLastName, p.Email,  ROW_NUMBER() OVER(Partition by s.salespersonid ORDER BY s.Date desc) AS rn
                            from SalesAppCounters s inner join people p on s.SalesPersonId = p.PeopleID and warehouseid =#WID# 
                         and s.Date between '#FROMDATE#' and '#TODATE#' )a where rn=1";
            DateTime today = DateTime.Today;
            string fromDate = today.ToString("yyyy-MM-dd HH:mm:ss");
            string toDate = today.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            query = query.Replace("#FROMDATE#", fromDate);
            query = query.Replace("#TODATE#", toDate);
            query = query.Replace("#WID#", warehouseid.ToString());
            using (var context = new AuthContext())
            {
                List<InitialPoint> list = context.Database.SqlQuery<InitialPoint>(query).ToList();
                return Ok(list);
            }
        }

        [HttpGet]
        [Route("GetSalesInitialPoint/warehouseid/{warehouseid}")]
        public IHttpActionResult GetSalesInitialPoint(int warehouseid)
        {
            var query = @"select* from(select s.SalesPersonId, lat, Long,p.Mobile, p.PeopleFirstName, p.PeopleLastName, p.Email,  ROW_NUMBER() OVER(Partition by s.salespersonid ORDER BY s.Date desc) AS rn
                            from SalesAppCounters s inner join people p on s.SalesPersonId = p.PeopleID and warehouseid =#WID#   )a where rn=1";
            //DateTime today = DateTime.Today;
            //string fromDate = today.ToString("yyyy-MM-dd HH:mm:ss");
            //string toDate = today.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            //query = query.Replace("#FROMDATE#", fromDate);
            //query = query.Replace("#TODATE#", toDate);
            query = query.Replace("#WID#", warehouseid.ToString());
            using (var context = new AuthContext())
            {
                List<InitialPoint> list = context.Database.SqlQuery<InitialPoint>(query).ToList();

                return Ok(list);
            }

        }

        [HttpGet]
        [Route("GetSalesPersonPoints/salesPersonId/{salesPersonId}/fromDate/{fromDate}/toDate/{toDate}")]
        public IHttpActionResult GetSalesPersonPoints(int salesPersonId, DateTime fromDate, DateTime toDate)
        {
            using (var context = new AuthContext())
            {
                toDate = toDate.AddDays(1).AddSeconds(-1);
                var list = context.SalesAppCounterDB.Where(x => x.SalesPersonId == salesPersonId && x.Date >= fromDate && x.Date <= toDate).OrderBy(x => x.Date).ToList();

                if (list != null && list.Count > 25)
                {
                    List<SalesAppCounter> newList = new List<SalesAppCounter>();

                    newList.Add(list.First());
                    newList.AddRange(list.Skip(Math.Max(0, list.Count() - 24)));
                    list = newList;
                }
                return Ok(list);
            }
        }


        [HttpGet]
        [Route("GetSalesPersonPointsNew/salesPersonId/{salesPersonId}/fromDate/{fromDate}/toDate/{toDate}")]
        public IHttpActionResult GetSalesPersonPointsNew(int salesPersonId, DateTime fromDate, DateTime toDate)
        {
            using (var context = new AuthContext())
            {
                toDate = toDate.AddDays(1).AddSeconds(-1);
                var list = context.SalesAppCounterDB.Where(x => x.SalesPersonId == salesPersonId && x.Date >= fromDate && x.Date <= toDate).ToList();

                if (list != null && list.Count > 1)
                {
                    List<SalesAppCounter> newList = new List<SalesAppCounter>();

                    //if (list.Count > 25)
                    //{
                    //    newList.Add(list.First());
                    //    newList.AddRange(list.Skip(Math.Max(0, list.Count() - 24)));
                    //}
                    //else
                    //{
                    //    newList.AddRange(list);
                    //}
                    newList.AddRange(list);
                    list = newList.GroupBy(x => new { x.lat, x.Long }).Select(x => new SalesAppCounter
                    {
                        lat = x.Key.lat,
                        Long = x.Key.Long,
                        Date = x.FirstOrDefault().Date,
                        Deleted = x.FirstOrDefault().Deleted,
                        SalesPersonId = x.FirstOrDefault().SalesPersonId
                    }).OrderBy(x => x.Date).ToList();

                    if (list.Count > 25)
                    {
                        List<SalesAppCounter> newList1 = new List<SalesAppCounter>();

                        newList1.Add(list.First());
                        newList1.AddRange(list.Skip(Math.Max(0, list.Count() - 24)));
                        list = newList1;
                    }

                }
                return Ok(list);
            }
        }

        [HttpGet]
        [Route("Permissions")]
        public IHttpActionResult Permissions()
        {
            if (userid > 0)
            {
                using (var authContext = new AuthContext())
                {
                    var query = from p in authContext.Peoples
                                join w in authContext.Warehouses
                                on p.WarehouseId equals w.WarehouseId
                                where p.PeopleID == userid
                                select new SalesPersonDepartmentDc()
                                {
                                    PeopleID = p.PeopleID,
                                    Department = p.Department.ToString(),
                                    WarehouseName = w.WarehouseName

                                };
                    var item = authContext.Peoples.FirstOrDefault(p => p.PeopleID == userid);
                    return Ok(item);
                }
            }
            else
            {
                return Ok();
            }
        }


        [HttpGet]
        [Route("GetSalesPersonDetails/{Keyword}")]
        [AllowAnonymous]
        public IHttpActionResult GetSalesPersonDetails(string Keyword)
        {
            var query = @"select* from(select s.SalesPersonId, lat, Long,p.Mobile, p.PeopleFirstName, p.PeopleLastName,
                        p.PeopleFirstName +' '+ Isnull( p.PeopleLastName,'') as Name,
                     p.Email,  ROW_NUMBER() OVER(Partition by s.salespersonid ORDER BY s.Date desc) AS rn
                            from SalesAppCounters s inner join people p on s.SalesPersonId = p.PeopleID
                             and  p.PeopleFirstName+ ISNULL(p.PeopleLastName,'') like '%#Keyword#%'
                          )a where rn=1";
            DateTime today = DateTime.Today;

            query = query.Replace("#Keyword#", Keyword.ToString());
            using (var context = new AuthContext())
            {
                List<SalesPersonDC> list = context.Database.SqlQuery<SalesPersonDC>(query).ToList();

                return Ok(list);
            }

        }

        public class SalesAppCounterDTO
        {
            public SalesAppCounter MUget { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class InitialPoint
        {
            public int SalesPersonId { get; set; }
            public double lat { get; set; }
            public double Long { get; set; }
            public long rn { get; set; }

            public string Mobile { get; set; }
            public string Email { get; set; }
            public string PeopleFirstName { get; set; }
            public string PeopleLastName { get; set; }
            public string WarehouseName { get; set; }
            public int WarehouseId { get; set; }
        }
        public class SalesPersonDepartmentDc
        {
            public int PeopleID { get; set; }
            public string Department { get; set; }
            public string WarehouseName { get; set; }

        }
        public class SalesPersonDC
        {
            public string Mobile { get; set; }
            public string Name { get; set; }
        }
    }
}
