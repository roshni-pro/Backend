using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/freeitem")]
    public class FreeItemController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("")]
        public HttpResponseMessage Get(int PurchaseOrderId)
        {
            List<FreeItem> ass = new List<FreeItem>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;


                    ass = context.FreeItemDb.Where(c => c.Deleted == false && c.PurchaseOrderId == PurchaseOrderId && c.CompanyId == compid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("add")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(FreeItem item)
        {
            logger.Info("start Return: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                    item.CompanyId = compid;

                    if (item != null)
                    {
                        FreeItem FI = context.FreeItemDb.Where(a => a.PurchaseOrderId == item.PurchaseOrderId && a.GRNumber == item.GRNumber && a.itemNumber == item.itemNumber && a.ItemMultiMRPId == item.ItemMultiMRPId).SingleOrDefault();
                        if (FI == null)
                        {
                            item.CreationDate = indianTime;
                            context.FreeItemDb.Add(item);
                            context.Commit();
                        }
                        else
                        {
                            FI.TotalQuantity = item.TotalQuantity;
                            context.Entry(FI).State = EntityState.Modified;
                            context.Commit();

                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Authorize]
        [Route("SkFree")]
        public HttpResponseMessage Getitem(int oderid)
        {
            List<SKFreeItem> ass = new List<SKFreeItem>();
            using (AuthContext context = new AuthContext())
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
                    ass = context.SKFreeItemDb.Where(c => c.Deleted == false && c.OrderId == oderid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("View")]
        [HttpGet]
        public HttpResponseMessage GetView(int PurchaseOrderId, string GrNumber)
        {
            List<FreeItem> ass = new List<FreeItem>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;


                    ass = context.FreeItemDb.Where(c => c.Deleted == false && c.PurchaseOrderId == PurchaseOrderId && c.GRNumber == GrNumber && c.CompanyId == compid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("GetItem")]
        public HttpResponseMessage GetItem(int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var query = "select ItemId,WarehouseId,HSNCode,PurchaseSku,Itemname, price as MRP,ItemMultiMRPId,Number from ItemMasters where WarehouseId =" + WarehouseId;
                    List<PoFreeItemMasterDTO> ItemMasterList = context.Database.SqlQuery<PoFreeItemMasterDTO>(query).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, ItemMasterList);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Authorize]
        [Route("GetFreeItemGRbased")]
        public HttpResponseMessage GetFreeItemGRbased(int PurchaseOrderId, string GrNumber)
        {
            List<FreeItem> ass = new List<FreeItem>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    ass = context.FreeItemDb.Where(c => c.Deleted == false && c.PurchaseOrderId == PurchaseOrderId && c.GRNumber == GrNumber).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        


    }
    public class PoFreeItemMasterDTO
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public string HSNCode { get; set; }
        public string PurchaseSku { get; set; }
        public string Itemname { get; set; }
        public double MRP { get; set; }
        public int TotalQuantity { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Number { get; set; }

    }

    public class FreeItemSubCatTargetDTO
    {       
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemNumber { get; set; }

    }

}





