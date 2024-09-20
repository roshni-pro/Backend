using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/bouncecheq")]
    public class BounceCheqController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("saless")]
        [HttpGet]
        public PaggingData salessettlement(int list, int page)
        {
            using (var db = new AuthContext())
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    if (Warehouse_id > 0)
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }
                    else
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.CompanyId == compid).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.CompanyId == compid).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }


        [Route("search")]
        [HttpGet]
        public dynamic search(DateTime? start, DateTime? end, int? OrderId)
        {
            using (var db = new AuthContext())
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    if (Warehouse_id > 0)
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId) && (x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id) && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id) && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }
                    else
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.Status == "Partial receiving -Bounce" && x.CompanyId == compid).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId) && (x.Status == "Partial receiving -Bounce" && x.CompanyId == compid) && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.Status == "Partial receiving -Bounce" && x.CompanyId == compid) && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }



                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

        [Route("Bounce")]
        [HttpGet, HttpPut]
        public dynamic Bounce(OrderDispatchedMaster data)
        {
            using (var db = new AuthContext())
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    if (Warehouse_id > 0)
                    {
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == Warehouse_id).FirstOrDefault();
                        comp.Status = "Account settled";
                        //comp.UpdatedDate = indianTime;
                        //db.OrderDispatchedMasters.Attach(comp);
                        db.Entry(comp).State = EntityState.Modified;
                        db.Commit();
                        return comp;
                    }
                    else
                    {
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == compid).FirstOrDefault();
                        comp.Status = "Account settled";
                        //comp.UpdatedDate = indianTime;
                        //db.OrderDispatchedMasters.Attach(comp);
                        db.Entry(comp).State = EntityState.Modified;
                        db.Commit();
                        return comp;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }


    }
}
