using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AngularJSAuthentication.Model.PurchaseOrder;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrderDetail")]
    public class PurchaseOrderDetailController : ApiController
    {
      
        public static Logger logger = LogManager.GetCurrentClassLogger();

        // [Authorize]
        [Route("")]
        public IEnumerable<PurchaseOrderDetail> Get(string recordtype)
        {
            if (recordtype == "details")
            {
                logger.Info("start PurchaseOrderDetail: ");
                List<PurchaseOrderDetail> ass = new List<PurchaseOrderDetail>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    using (var context = new AuthContext())
                    {
                        ass = context.AllPOdetails(compid).ToList();
                    }
                    logger.Info("End  order: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
            return null;
        }

        [Authorize]
        [Route("")]
        public IEnumerable<PurchaseOrderDetail> Getallorderdetails(string id)
        {
            logger.Info("start : ");
            List<PurchaseOrderDetail> ass = new List<PurchaseOrderDetail>();

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            // Access claims
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            if (id != null && id != "undefined")
            {
                int idd = Int32.Parse(id);
                using (var db = new AuthContext())
                {
                    ass = db.DPurchaseOrderDeatil.Where(c => c.PurchaseOrderNewId == idd).ToList();
                    // ass = db.AllPOrderDetails(idd, compid, Warehouse_id).ToList();
                    var itemNumberList = ass.Select(x => x.ItemNumber);
                    var Centralitem = db.ItemMasterCentralDB.Where(c => itemNumberList.Contains(c.Number)).ToList();
                    var MultiMrpList = db.ItemMultiMRPDB.Where(c => itemNumberList.Contains(c.ItemNumber)).ToList();
                    foreach (PurchaseOrderDetail d in ass)
                    {
                        var item = Centralitem.Where(c => c.Number == d.ItemNumber).FirstOrDefault();
                        //d.Barcode = item.Barcode;
                        d.IsCommodity = false;
                        if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                        {
                            d.IsCommodity = false;
                        }
                        else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                        {
                            d.IsCommodity = true;
                        }
                        d.multiMrpIds = db.ItemMultiMRPDB.Where(a => a.ItemNumber == d.ItemNumber).ToList();
                    }
                }
            }
            logger.Info("End  : ");
            return ass;

        }

        ////Cityid=' + data.Cityid + "&&" + "Warehouseid="+ data.Warehouseid + "&&" + "datefrom="+ data.datefrom + "&&" + "dateto="+ data.dateto
        //[Authorize]
        //[Route("")]
        //public IEnumerable<DemandDetailsNew> Getallorderdetails(string Cityid, string Warehouseid,DateTime datefrom, DateTime dateto)
        //{
        //    logger.Info("start : ");
        //    IList<DemandDetailsNew> list = null;
        //    List<OrderDetails> ass = new List<OrderDetails>();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        //int idd = Int32.Parse(id);
        //        list = context.AllfilteredOrderDetails(Cityid, Warehouseid, datefrom, dateto).ToList();
        //        logger.Info("End  : ");
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in OrderDetails " + ex.Message);
        //        logger.Info("End  OrderDetails: ");
        //        return null;
        //    }
        //}





        [Route("GetPoForEdit")]
        [AllowAnonymous]
        public EditPODTO GetPoForEdit(int PurchaseOrderId)
        {
            PurchaseOrderMaster ass = new PurchaseOrderMaster();
            List<PurchaseOrderDetail> assv1 = new List<PurchaseOrderDetail>();
            EditPODTO sendObject = new EditPODTO();
            try
            {
                using (var context = new AuthContext())
                {
                    ass = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).SingleOrDefault();
                    assv1 = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PurchaseOrderId).ToList();
                }
                sendObject.PurchaseOrderMastersDTO = ass;
                sendObject.PurchaseOrderDetailsDTO = assv1;
                return sendObject;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return null;
            }
        }
    }


    public class EditPODTO
    {
        public PurchaseOrderMaster PurchaseOrderMastersDTO { get; set; }

        public List<PurchaseOrderDetail> PurchaseOrderDetailsDTO { get; set; }


    }
}