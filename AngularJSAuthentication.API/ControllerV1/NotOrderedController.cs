using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NotOrdered")]
    public class NotOrderedController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        public List<SalesPersonBeat> Get()
        {
            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<SalesPersonBeat> displist = new List<SalesPersonBeat>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    displist = db.SalesPersonBeatDb.Where(x => x.Skcode != null && x.CompanyId == compid && x.WarehouseId == Warehouse_id).ToList();
                    logger.Info("End  Sales Executive: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }
        [Route("VisitNotVisit")]
        [HttpGet]
        public List<SalesPersonBeat> Get1(int salespersonid)
        {
            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<SalesPersonBeat> displist = new List<SalesPersonBeat>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    displist = db.SalesPersonBeatDb.Where(x => x.Skcode != null && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.SalesPersonId == salespersonid).ToList();
                    logger.Info("End  Sales Executive: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }


        [Route("VisitNotVisitStatus")]
        [HttpGet]
        public List<SalesPersonBeat> Get2(int salespersonid, bool Visited)
        {
            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<SalesPersonBeat> displist = new List<SalesPersonBeat>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    displist = db.SalesPersonBeatDb.Where(x => x.Skcode != null && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.Visited == Visited && x.SalesPersonId == salespersonid).ToList();
                    logger.Info("End  Sales Executive: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }


        [Route("search")]
        [HttpGet]
        public dynamic search(DateTime? start, DateTime? end, string skcode, int? salespersonid)
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {

                        if (skcode != null && start == null && salespersonid == null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.Skcode == skcode && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }

                        else if (skcode != null && start == null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.Skcode == skcode && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }

                        else if (start != null && skcode != null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.Skcode == skcode && x.CompanyId == compid && x.WarehouseId == Warehouse_id && (x.CreatedDate > start && x.CreatedDate <= end) && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }


                        else if (start != null && skcode == null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {

                        if (skcode != null && start == null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.Skcode == skcode && x.CompanyId == compid && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }

                        else if (start != null && skcode != null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.Skcode == skcode && x.CompanyId == compid && (x.CreatedDate > start && x.CreatedDate <= end) && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }


                        else if (start != null && skcode == null)
                        {
                            var data = db.SalesPersonBeatDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.CompanyId == compid && x.SalesPersonId == salespersonid).ToList();

                            return data;
                        }
                        else
                        {
                            return null;
                        }

                    }

                }
                catch (Exception ex)
                {

                    return false;
                }
            }
        }
    }
}
