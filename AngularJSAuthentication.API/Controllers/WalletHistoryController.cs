using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/WalletHistory")]
    public class WalletHistoryController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int CompanyId { get; private set; }
        public int WarehouseId { get; private set; }
        public int CustomerId { get; private set; }

        [Route("")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Get(DateTime start, DateTime end, int WarehouseId)
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<CustomerWalletHistory> data = new List<CustomerWalletHistory>();
                    List<int> CustomerId = new List<int>();
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

                    data = context.CustomerWalletHistoryDb.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.CreatedDate >= start && x.CreatedDate <= end).ToList();

                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, data);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        //....
        [Route("Search")]
        [HttpGet]
        public HttpResponseMessage GetData(DateTime start, DateTime end, int WarehouseId)
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var pointList = (from i in context.WalletDb
                                     where i.Deleted == false && i.CreatedDate >= start && i.CreatedDate <= end
                                     join j in context.Customers on i.CustomerId equals j.CustomerId
                                     where j.Warehouseid == WarehouseId
                                     join k in context.Warehouses on j.Warehouseid equals k.WarehouseId into ts
                                     from k in ts.DefaultIfEmpty()
                                     select new
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         TotalAmount = i.TotalAmount,
                                         CreatedDate = i.CreatedDate,
                                         TransactionDate = i.TransactionDate,
                                         UpdatedDate = i.UpdatedDate,
                                         Skcode = j.Skcode,
                                         ShopName = j.ShopName,
                                         WarehouseName = k.WarehouseName,
                                         WarehouseId = j.Warehouseid,
                                         City = k.CityName
                                     }).ToList();
                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }
        }
        //..........
        [Route("customer")]
        public HttpResponseMessage GetCustomer(string skcode)
        {
            logger.Info("start custmer wallet: ");
            using (AuthContext context = new AuthContext())
            {
                Customer cust = new Customer();
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
                        cust = context.Customers.Where(x => x.Skcode.Equals(skcode) && x.Deleted == false).FirstOrDefault();

                        if (cust != null)
                        {
                            var Custs = context.Customers.Where(a => a.CustomerId == cust.CustomerId && a.Warehouseid == Warehouse_id).FirstOrDefault();

                            if (Custs != null)
                            {
                                logger.Info("End  custmer: ");
                                return Request.CreateResponse(HttpStatusCode.OK, cust);
                            }
                            else
                            {

                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                            }
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                        }
                    }


                    else
                    {

                        cust = context.Customers.Where(x => x.Skcode.ToLower().Equals(skcode.ToLower()) && x.Deleted == false).FirstOrDefault();

                        if (cust != null)
                        {
                            var Custs = context.Customers.Where(a => a.CustomerId == cust.CustomerId && a.CompanyId == compid).FirstOrDefault();

                            if (Custs != null)
                            {
                                logger.Info("End  custmer: ");
                                return Request.CreateResponse(HttpStatusCode.OK, cust);
                            }
                            else
                            {

                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                            }
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in cusomer " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }

        [Route("GetHistory")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage GetHistory(hisDTO obj)
        {
            logger.Info("start single  GetcusomerWallets: ");
            using (AuthContext context = new AuthContext())
            {
                WalletHistoryController Item = new WalletHistoryController();
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
                    List<CustomerWalletHistory> listobj = new List<CustomerWalletHistory>();

                    foreach (widclass q in obj.ids)
                    {
                        List<CustomerWalletHistory> h = context.CustomerWalletHistoryDb.Where(a => a.WarehouseId == q.id && a.CreatedDate >= obj.From && a.CreatedDate <= obj.TO).ToList();

                        foreach (CustomerWalletHistory a in h)
                        {
                            try
                            {
                                Customer c = context.Customers.Where(s => s.CustomerId == a.CustomerId).SingleOrDefault();
                                a.Skcode = c.Skcode;
                                a.ShopName = c.ShopName;

                                Warehouse warehouse = context.Warehouses.Where(x => x.WarehouseId == a.WarehouseId).SingleOrDefault();
                                a.WarehouseName = warehouse.WarehouseName + ' ' + warehouse.CityName;

                            }
                            catch { }
                            listobj.Add(a);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, listobj);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                    logger.Info("End  single GetcusomerWallets: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
    }

    public class widclass
    {
        public int id { get; set; }
    }

    public class hisDTO
    {
        public DateTime From { get; set; }
        public DateTime TO { get; set; }
        public List<widclass> ids { get; set; }
    }
}
