using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AssignBrandCustomer")]
    public class AssignBrandCustomerController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [AcceptVerbs("GET")]
        public dynamic Get(int Warehouseid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start get all GpsCoordinate: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    List<WarehouseSubsubCategory> displist = new List<WarehouseSubsubCategory>();
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

                    //displist = db.DbWarehousesubsubcats.Where(x => x.Deleted == false && x.WarehouseId == Warehouseid).ToList();
                    //List<CustSupplier> newbrand = new List<CustSupplier>();
                    //List<string> fff = new List<string>();
                    //foreach (var data1 in displist)
                    //{
                    //    var newbrand1 = context.CustSupplierDb.Where(x => x.WarehouseId == Warehouseid && x.SubsubCode == data1.SubsubCode).FirstOrDefault();
                    //    if(newbrand1==null)
                    //    {
                    //        fff.Add(data1.SubsubCode);
                    //    }

                    //}
                    //displist.Clear();
                    //for (int x=0;x<fff.Count;x++)
                    //{
                    //    var myInt = fff[x];
                    //    var newbranddata = db.DbWarehousesubsubcats.Where(a => a.Deleted == false && a.WarehouseId == Warehouseid && a.SubsubCode== myInt).FirstOrDefault();
                    //    if(newbranddata!=null)
                    //    {
                    //        displist.Add(newbranddata);
                    //    }
                    //}
                    //return displist;

                    var query = from c in db.DbWarehousesubsubcats
                                where !(from o in db.CustSupplierDb
                                        where o.WarehouseId == Warehouseid
                                        select o.SubsubCode).Contains(c.SubsubCode) && c.Deleted == false
                                select c;

                    return query;
                    //logger.Info("End  UnitMaster: ");
                    //return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in get all  " + ex.Message);
                    logger.Info("End get all : ");
                    return null;
                }
            }
        }

        [Route("customer")]
        public HttpResponseMessage get(int CityId, int Warehouseid, string SubsubCode)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; string SUPPLIERCODES = "";
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
                        if (claim.Type == "SUPPLIERCODES")
                        {
                            SUPPLIERCODES = Convert.ToString(claim.Value);
                        }
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int CompanyId = compid;

                    var clist = context.getcust2assin(CityId, Warehouseid, SubsubCode, CompanyId).ToList();
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Customers");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("AddCustSupplier")]
        public HttpResponseMessage post(int wid, List<CustSupplier> obj)
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    if (Warehouse_id > 0)
                    {
                        var CustSuppliers = context.addcustsuppliermapping(obj, compid, Warehouse_id).ToList();
                        if (CustSuppliers == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "got error");
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}



