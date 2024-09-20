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
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ActionTask")]
    public class ActionTaskController : BaseAuthController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("")]
        public IEnumerable<ActionTask> Get()
        {
            logger.Info("start ActionTask: ");
            List<ActionTask> ass = new List<ActionTask>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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

                using (AuthContext db = new AuthContext())
                {
                    ass = db.ActionTaskDb.Where(x => x.active == true && x.Deleted == false && x.CompanyId == compid).OrderByDescending(x => x.ActionTaskid).ToList();
                    logger.Info("End  ActionTask: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActionTask " + ex.Message);
                logger.Info("End  ActionTask: ");
                return null;
            }
        }

        [Authorize]
        [Route("")]
        public ActionTask Get(int id)
        {
            logger.Info("start ActionTask: ");
            ActionTask ass = new ActionTask();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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

                using (AuthContext db = new AuthContext())
                {
                    ass = db.ActionTaskDb.Where(x => x.ActionTaskid == id && x.CompanyId == compid).FirstOrDefault();
                    logger.Info("End  ActionTask: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActionTask " + ex.Message);
                logger.Info("End  ActionTask: ");
                return null;
            }
        }

        //[Authorize]
        [Route("appActionTask")]
        public ActionTask Get(int PeopleId, string Skcode)
        {
            logger.Info("start ActionTask: ");
            ActionTask ass = new ActionTask();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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

                using (AuthContext db = new AuthContext())
                {
                    ass = db.ActionTaskDb.Where(x => x.PeopleID == PeopleId && x.Skcode == Skcode && x.CompanyId == compid).FirstOrDefault();

                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActionTask " + ex.Message);
                logger.Info("End  ActionTask: ");
                return null;
            }
        }



        [Route("Search")]
        public List<ActionTask> Getdata(int PeopleId)
        {
            logger.Info("start ActionTask: ");
            ActionTask ass = new ActionTask();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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

                using (AuthContext db = new AuthContext())
                {
                    var asss = db.ActionTaskDb.Where(x => x.PeopleID == PeopleId && x.CompanyId == compid).ToList();

                    return asss;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActionTask " + ex.Message);
                logger.Info("End  ActionTask: ");
                return null;
            }
        }






        [ResponseType(typeof(ActionTask))]
        [Route("")]
        [AcceptVerbs("POST")]
        public ActionTask add(ActionTask item)
        {
            logger.Info("start add ActionTask: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                if (compid == 0) { compid = 1; }
                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                using (AuthContext db = new AuthContext())
                {
                    item.active = true;
                    item.Deleted = false;
                    item.Status = "Pending";
                    item.CreatedDate = indianTime;
                    item.UpdatedDate = indianTime;
                    try
                    {
                        item.PeopleName = db.Peoples.Where(p => p.PeopleID == item.PeopleID && p.CompanyId == compid).FirstOrDefault().DisplayName;
                    }
                    catch (Exception ex) { }
                    db.ActionTaskDb.Add(item);
                    int id = db.Commit();
                  
                    return item;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add ActionTask " + ex.Message);
                logger.Info("End  addActionTask: ");
                return null;
            }
        }

        [ResponseType(typeof(ActionTask))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public ActionTask Put(ActionTask item)
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
                if (compid == 0) { compid = 1; }
                item.CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    if (compid > 0)
                    {
                        var edit = db.ActionTaskDb.Where(x => x.ActionTaskid == item.ActionTaskid && x.CompanyId == item.CompanyId).SingleOrDefault();

                        edit.Description = item.Description;
                        edit.Status = item.Status;
                        edit.UpdatedDate = indianTime;
                        db.ActionTaskDb.Attach(edit);
                        db.Entry(edit).State = EntityState.Modified;
                        db.Commit();

                    }
                    return item;
                }
            }
            catch
            {
                return null;
            }
        }

        [ResponseType(typeof(ActionTask))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteActionTasky: ");
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

                using (AuthContext context = new AuthContext())
                {
                    if (compid > 0)
                    {
                        var edit = context.ActionTaskDb.Where(x => x.ActionTaskid == id && x.CompanyId == compid).SingleOrDefault();
                        edit.active = false;
                        edit.Deleted = true;
                        context.ActionTaskDb.Attach(edit);
                        context.Entry(edit).State = EntityState.Modified;
                        context.Commit();
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteActionTask" + ex.Message);
            }
        }

        [Route("getcustomer")]
        [HttpGet]
        public HttpResponseMessage get(int id, int filter, DateTime start, DateTime end, int cattype, int catid, int GL_Type, double values) //get customers 
        {
            try
            {

                logger.Info("getcustomerby level");
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                if (compid > 0)
                {
                    var clist = getcustomerordercounts(id, filter, start, end, cattype, catid, GL_Type, values, compid);
                    return Request.CreateResponse(HttpStatusCode.OK, clist);

                }
                return Request.CreateResponse(HttpStatusCode.OK, "No data");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        public List<Customer> getcustomerordercounts(int id, int filter, DateTime start, DateTime end, int cattype, int catid, int GL_Type, double values, int compid)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<Customer> customers = new List<Customer>();
                    if (filter == 5)
                    {
                        customers = context.Customers.Where(x => x.ClusterId == id && x.Active == true && x.CompanyId == compid).ToList();
                    }
                    else if (filter == 2)
                    {
                        customers = context.Customers.Where(x => x.Warehouseid == id && x.Active == true && x.CompanyId == compid).ToList();
                    }
                    else if (filter == 3)
                    {
                        customers = context.Customers.Where(x => x.Cityid == id && x.Active == true && x.CompanyId == compid).ToList();
                    }
                    //else if (filter == 4)
                    //{
                    //    customers = context.Customers.Where(x => x.ExecutiveId == id && x.Active == true && x.CompanyId == compid).ToList();
                    //}
                    else { return null; }

                    List<Customer> clist = new List<Customer>();
                    foreach (var c in customers)
                    {
                        int thisordercount = 0;
                        int thisordercountCancelled = 0;
                        int thisordercountRedispatch = 0;
                        int thisordercountdelivered = 0;
                        int thisordercountpending = 0;
                        double thisordervalue = 0;
                        //var orders = context.DbOrderMaster.Where(x => x.CustomerId == c.CustomerId && x.CreatedDate >= start && x.CreatedDate <= end).ToList();
                        var od = filterData(c, start, end, filter, cattype, catid);
                        List<filCustomer> orders = new List<filCustomer>();
                        foreach (var o in od)
                        {
                            var odr = orders.Where(f => f.OrderId == o.OrderId).SingleOrDefault();
                            if (odr != null)
                            {
                                odr.TotalAmt += o.TotalAmt;
                            }
                            else
                            {
                                orders.Add(o);
                            }
                        }
                        if (orders.Count >= 0)
                        {
                            thisordercount = orders.Count();
                            Customer cc = new Customer();
                            cc = c;
                            foreach (var o in orders)
                            {
                                if (o.Status == "Order Canceled" || o.Status == "Delivery Canceled")
                                {
                                    thisordercountCancelled = thisordercountCancelled + 1;
                                }
                                else if (o.Status == "Delivered" || o.Status == "sattled" || o.Status == "Account settled" || o.Status == "Partial settled")
                                {
                                    thisordercountdelivered = thisordercountdelivered + 1;
                                }
                                else
                                {
                                    thisordercountpending = thisordercountpending + 1;
                                }
                                thisordervalue = thisordervalue + o.TotalAmt;
                            }
                            cc.thisordercount = thisordercount;
                            cc.thisordercountCancelled = thisordercountCancelled;
                            cc.thisordercountRedispatch = thisordercountRedispatch;
                            cc.thisordercountdelivered = thisordercountdelivered;
                            cc.thisordercountpending = thisordercountpending;
                            cc.thisordervalue = thisordervalue;
                            clist.Add(cc);
                        }
                    }
                    List<Customer> customerss = new List<Customer>();
                    foreach (var csttt in clist)
                    {
                        if (csttt.thisordervalue >= values && GL_Type == 1)
                        {
                            customerss.Add(csttt);
                        }
                        else if (csttt.thisordervalue <= values && GL_Type == 2)
                        {
                            customerss.Add(csttt);
                        }
                    }
                    var data = customerss.OrderByDescending(a => a.thisordervalue).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<filCustomer> filterData(Customer c, DateTime start, DateTime end, int filter, int cattype, int catid)
        {
            List<filCustomer> orders = new List<filCustomer>();
            using (AuthContext context = new AuthContext())
            {
                if (cattype == 1)
                {
                    orders = (from i in context.DbOrderDetails
                              where i.CustomerId == c.CustomerId && i.CreatedDate >= start && i.CreatedDate <= end
                              join j in context.itemMasters on i.ItemId equals j.ItemId
                              join k in context.Categorys on catid equals k.Categoryid
                              select new filCustomer
                              {
                                  OrderId = i.OrderId,
                                  OrderDate = i.OrderDate,
                                  TotalAmt = i.TotalAmt,
                                  Status = i.Status,

                              }).ToList();

                    if (orders.Count() > 0)
                    {

                    }

                }
                else if (cattype == 2)
                {
                    orders = (from i in context.DbOrderDetails
                              where i.CustomerId == c.CustomerId && i.CreatedDate >= start && i.CreatedDate <= end
                              join j in context.itemMasters on i.ItemId equals j.ItemId
                              join k in context.SubCategorys on catid equals k.SubCategoryId
                              select new filCustomer
                              {
                                  OrderId = i.OrderId,
                                  OrderDate = i.OrderDate,
                                  TotalAmt = i.TotalAmt,
                                  Status = i.Status,

                              }).ToList();
                }
                else if (cattype == 3)
                {
                    orders = (from i in context.DbOrderDetails
                              where i.CustomerId == c.CustomerId && i.CreatedDate >= start && i.CreatedDate <= end
                              join j in context.itemMasters on i.ItemId equals j.ItemId
                              join k in context.SubsubCategorys on catid equals k.SubCategoryId
                              select new filCustomer
                              {
                                  OrderId = i.OrderId,
                                  OrderDate = i.OrderDate,
                                  TotalAmt = i.TotalAmt,
                                  Status = i.Status,

                              }).ToList();
                }
                return orders;
            }
        }
    }
    public class filCustomer
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmt { get; set; }
        public string Status { get; set; }
    }
}