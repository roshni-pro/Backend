using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrderMasterRecived")]
    public class PurchaseOrderMasterRecivedController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public IEnumerable<PurchaseOrderMasterRecived> Get()
        {
            logger.Info("start PurchaseOrderMaster: ");
            List<PurchaseOrderMasterRecived> ass = new List<PurchaseOrderMasterRecived>();
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
                //  ass = context.AllPOMaster().ToList();
                logger.Info("End PurchaseOrderMaster: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }

        [Route("")]
        public Warehouse Get(string id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            Warehouse wh = new Warehouse();

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
                if (id != null)
                {
                    int Id = Convert.ToInt32(id);
                   
                    using (var db = new AuthContext())
                    {
                        wh = db.Warehouses.Where(x => x.WarehouseId == Id && x.CompanyId == compid).SingleOrDefault();
                    }
                    return wh;

                }
                return null;

            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }

        [ResponseType(typeof(PurchaseOrderMasterRecived))]
        [Route("")]
        [AcceptVerbs("POST")]
        public PurchaseOrderMasterRecived add(PurchaseOrderMasterRecived item)
        {

            logger.Info("start PurchaseOrderMasterRecived: ");
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
                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    context.AddPurchaseOrderMasterRecived(item);
                }
                logger.Info("End  UnitMaster: ");
                return item;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMasterRecived " + ex.Message);
                logger.Info("End  PurchaseOrderMasterRecived: ");
                return null;
            }
        }

    

    }
    public class PurchaseOrderIdGroupdata
    {

        public int PurchaseOrderId { get; set; }

    }
    public class IrCorrectData
    {
        public long InvoiceRecieptId { get; set; }
        public int GoodReceievdCount { get; set; }
        public long GoodRecievedId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int IrMasterId { get; set; }
    }
}