using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AssignCustomer")]
    public class AssignCustomerController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [AcceptVerbs("GET")]
        public List<WarehouseSubsubCategory> Get(int Warehouseid)
        {
            logger.Info("start get all GpsCoordinate: ");
            using (var db = new AuthContext())
            {
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

                    displist = db.DbWarehousesubsubcats.Where(x => x.Deleted == false && x.WarehouseId == Warehouseid).ToList();
                    logger.Info("End  UnitMaster: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in get all  " + ex.Message);
                    logger.Info("End get all : ");
                    return null;
                }
            }
        }

        [Route("data")]
        public MapData Get(string id, DateTime sDate, DateTime eDate)
        {
            MapData mapData = new MapData();
            logger.Info("start GpsCoordinate: ");

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

                    if (id != null)
                    {
                        int Id = Convert.ToInt32(id);

                        mapData.deliveryboy = db.GpsCoordinateDb.Where(x => x.DeliveryBoyId == Id && x.CreatedDate > sDate && x.CreatedDate <= eDate && x.CompanyId == compid).OrderBy(o => o.CreatedDate).ToList();
                        People p = db.Peoples.Where(x => x.PeopleID == Id && x.CompanyId == compid).SingleOrDefault();
                        var list = db.OrderDispatchedMasters.Where(x => x.DboyMobileNo == p.Mobile && x.Deliverydate >= sDate && x.Deliverydate <= eDate && x.CompanyId == compid).ToList();//&& (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")
                        List<GpsCoordinate> displist = new List<GpsCoordinate>();
                        foreach (OrderDispatchedMaster m in list)
                        {
                            Customer c = db.Customers.Where(x => x.CustomerId == m.CustomerId).SingleOrDefault();
                            GpsCoordinate gc = new GpsCoordinate()
                            {
                                DeliveryBoyId = Id,
                                lat = c.lat,
                                lg = c.lg,
                                CustomerName = c.ShopName,
                                isDestination = true,
                                status = m.Status
                            };

                            displist.Add(gc);
                        }
                        mapData.customer = displist;
                        return mapData;
                    }
                    return null;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in get all GpsCoordinate " + ex.Message);
                    logger.Info("End get all GpsCoordinate: ");
                    return null;
                }
            }
        }


    }
}



