using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DialPoint")]
    public class DialPointController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        public HttpResponseMessage get() //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);



                    // var clist = db.DialPointDB.Where(x => x.Deleted == false).ToList();

                    var clist = (from i in db.DialPointDB
                                 where i.Deleted == false
                                 join j in db.Customers on i.CustomerId equals j.CustomerId
                                 select new DialPointDTO
                                 {
                                     Id = i.Id,
                                     CustomerId = i.CustomerId,
                                     OrderId = i.OrderId,
                                     point = i.point,
                                     OrderAmount = i.OrderAmount,
                                     UsedUnused = i.UsedUnused,
                                     expired = i.expired,
                                     CreatedDate = i.CreatedDate,
                                     CustomerName = j.Name,
                                     ShopName = j.ShopName,
                                     Skcode = j.Skcode,
                                     BillingAddress = j.BillingAddress,
                                     ShippingAddress = j.ShippingAddress,
                                 }).ToList();




                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Requests");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("")]
        public HttpResponseMessage get(int CustomerId) //Request
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var clist = context.GetDialData(CustomerId);
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Requests");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        public HttpResponseMessage getExpired(int Id) //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var clist = db.DialPointDB.Where(x => x.Id == Id).FirstOrDefault();
                    try
                    {
                        double validHours = 24;
                        var latest = indianTime.AddHours(-validHours);
                        var expired = (from a in db.DialPointDB where a.CreatedDate < latest && a.Id == Id select a);

                        if (expired.Count() > 0)
                        {
                            var obj = new
                            {
                                Message = "Dial Expired due time exceed more than 24 hours"
                            };

                            clist.expired = true;
                            clist.UpdatedDate = indianTime;
                            db.DialPointDB.Attach(clist);
                            db.Entry(clist).State = EntityState.Modified;
                            db.Commit();

                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }



                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                    var obj1 = new
                    {
                        Message = "Dial Available"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, obj1);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpPut]
        public HttpResponseMessage postasgn(int Id)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var Dialpoint = context.updateDialPoint(Id);
                    if (Dialpoint == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Dialpoint);


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("Search")]
        [HttpGet]
        public HttpResponseMessage search(DateTime? start, DateTime? end, string skcode) //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (start != null && skcode == null)
                    {
                        var clist = (from i in db.DialPointDB
                                     where i.Deleted == false && i.CreatedDate > start && i.CreatedDate <= end
                                     join j in db.Customers on i.CustomerId equals j.CustomerId
                                     select new DialPointDTO
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         OrderId = i.OrderId,
                                         point = i.point,
                                         OrderAmount = i.OrderAmount,
                                         UsedUnused = i.UsedUnused,
                                         expired = i.expired,
                                         CreatedDate = i.CreatedDate,
                                         CustomerName = j.Name,
                                         ShopName = j.ShopName,
                                         Skcode = j.Skcode,
                                         BillingAddress = j.BillingAddress,
                                         ShippingAddress = j.ShippingAddress,
                                     }).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }

                    else if (start != null && skcode != null)
                    {
                        var clist = (from i in db.DialPointDB
                                     join j in db.Customers on i.CustomerId equals j.CustomerId
                                     where i.Deleted == false && i.CreatedDate > start && i.CreatedDate <= end
                                     && j.Skcode == skcode

                                     select new DialPointDTO
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         OrderId = i.OrderId,
                                         point = i.point,
                                         OrderAmount = i.OrderAmount,
                                         UsedUnused = i.UsedUnused,
                                         expired = i.expired,
                                         CreatedDate = i.CreatedDate,
                                         CustomerName = j.Name,
                                         ShopName = j.ShopName,
                                         Skcode = j.Skcode,
                                         BillingAddress = j.BillingAddress,
                                         ShippingAddress = j.ShippingAddress,
                                     }).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }
                    else
                    {
                        var clist = (from i in db.DialPointDB
                                     join j in db.Customers on i.CustomerId equals j.CustomerId
                                     where i.Deleted == false && j.Skcode == skcode

                                     select new DialPointDTO
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         OrderId = i.OrderId,
                                         point = i.point,
                                         OrderAmount = i.OrderAmount,
                                         UsedUnused = i.UsedUnused,
                                         expired = i.expired,
                                         CreatedDate = i.CreatedDate,
                                         CustomerName = j.Name,
                                         ShopName = j.ShopName,
                                         Skcode = j.Skcode,
                                         BillingAddress = j.BillingAddress,
                                         ShippingAddress = j.ShippingAddress,
                                     }).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("SalesmanDial")]
        [HttpGet]
        public HttpResponseMessage CheckDialForSalesman(int SalesManId) //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var Today = indianTime.Day;
                    var yesterday = indianTime.AddDays(-7);
                    List<DialPoint> dlist = new List<DialPoint>();
                    var clist = db.DialPointDB.Where(x => x.SalesPersonId == SalesManId && x.UsedUnused == false && x.expired == false && x.CreatedDate.Day <= Today && x.CreatedDate >= yesterday).ToList();
                    try
                    {
                        foreach (var ac in clist)
                        {
                            double validHours = 24;
                            var latest = indianTime.AddHours(-validHours);
                            var expired = (from a in db.DialPointDB where a.CreatedDate < latest && a.Id == ac.Id select a);
                            if (expired.Count() > 0)
                            {
                                ac.expired = true;
                                ac.UpdatedDate = indianTime;
                                db.DialPointDB.Attach(ac);
                                db.Entry(ac).State = EntityState.Modified;
                                db.Commit();
                            }
                            else
                            {
                                dlist.Add(ac);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dlist);
                }

                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("CustomerDeliveryDial")]
        [HttpGet]
        public HttpResponseMessage CheckDeliveryDialForCustomer(int CustomerId) //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var Today = indianTime.Day;
                    var yesterday = indianTime.AddDays(-2);
                    List<DialPoint> dlist = new List<DialPoint>();
                    var clist = db.DialPointDB.Where(x => x.CustomerId == CustomerId && x.UsedUnused == false && x.expired == false && x.CreatedDate.Day <= Today && x.CreatedDate >= yesterday).ToList();
                    try
                    {
                        foreach (var ac in clist)
                        {
                            double validHours = 24;
                            var latest = indianTime.AddHours(-validHours);
                            var expired = (from a in db.DialPointDB where a.CreatedDate < latest && a.Id == ac.Id select a);
                            if (expired.Count() > 0)
                            {
                                ac.expired = true;
                                ac.UpdatedDate = indianTime;
                                db.DialPointDB.Attach(ac);
                                db.Entry(ac).State = EntityState.Modified;
                                db.Commit();
                            }
                            else
                            {
                                dlist.Add(ac);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dlist);
                }

                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("value")]
        [HttpGet]
        public HttpResponseMessage DialValueData() //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var clist = db.DialValuePointDB.Where(x => x.CustomerId != 0).ToList();
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Requests");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("value/Search")]
        [HttpGet]
        public HttpResponseMessage SearchDialValueData(DateTime? start, DateTime? end, string skcode) //Request
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (start != null && skcode == null)
                    {
                        var clist = db.DialValuePointDB.Where(x => x.CustomerId != 0 && x.CreatedDate > start && x.CreatedDate <= end).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }

                    else if (start != null && skcode != null)
                    {
                        var clist = db.DialValuePointDB.Where(x => x.CustomerId != 0 && x.CreatedDate > start && x.CreatedDate <= end && x.Skcode == skcode).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }
                    else
                    {
                        var clist = db.DialValuePointDB.Where(x => x.Skcode == skcode).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, clist);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        public class DialPointDTO
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public int OrderId { get; set; }
            public double? OrderAmount { get; set; }
            public double? point { get; set; }
            public bool UsedUnused { get; set; }
            public bool expired { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CustomerName { get; set; }
            public string ShopName { get; set; }
            public string Skcode { get; set; }
            public string BillingAddress { get; set; }
            public string ShippingAddress { get; set; }
            public int? CityId { get; set; }
        }


    }
}