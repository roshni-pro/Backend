using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using LinqKit;
using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrderMaster")]
    public class PurchaseOrderMasterController : BaseAuthController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //[Route("")]
        //public IEnumerable<PurchaseOrderMaster> Get()
        //{
        //    logger.Info("start PurchaseOrderMaster: ");
        //    List<PurchaseOrderMaster> ass = new List<PurchaseOrderMaster>();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

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
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }

        //        if (Warehouse_id > 0) {
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            ass = context.AllPOMasterWid(compid, Warehouse_id).OrderByDescending(x => x.PurchaseOrderId).ToList();
        //            logger.Info("End PurchaseOrderMaster: ");
        //            return ass;
        //        }
        //        else {
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            ass = context.AllPOMaster(compid).OrderByDescending(x => x.PurchaseOrderId).ToList();
        //            logger.Info("End PurchaseOrderMaster: ");
        //            return ass;
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in PurchaseOrderMaster " + ex.Message);
        //        logger.Info("End  PurchaseOrderMaster: ");
        //        return null;
        //    }
        //}

        [Route("")]
        public Warehouse Get(int id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            Warehouse wh = new Warehouse();

            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int WarehouseId = 0;

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
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    //if (WarehouseId > 0)
                    //{
                    //    wh = db.Warehouses.Where(x => x.WarehouseId == WarehouseId && x.CompanyId == compid).SingleOrDefault();
                    //    return wh;
                    //}
                    //else
                    //{
                    //    wh = db.Warehouses.Where(x => x.WarehouseId == id && x.CompanyId == compid).SingleOrDefault();
                    //    return wh;
                    //}
                    wh = db.Warehouses.Where(x => x.WarehouseId == id && x.CompanyId == compid).SingleOrDefault();
                    return wh;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                    logger.Info("End  PurchaseOrderMaster: ");
                    return null;
                }
            }
        }

        [Route("")]
        public PaggingData Get(int list, int page, int Warehouseid, string Status)
        {
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouseid);
                using (var context = new AuthContext())
                {
                    if (Warehouseid > 0)
                    {
                        // var itemPagedListWid = context.AllPOMasterWid(list, page, Warehouseid, CompanyId);
                        PaggingData Result = new PaggingData();
                        if (Status == null)
                        {
                            Result.total_count = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).Count();
                            Result.ordermaster = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        }
                        else
                        {
                            Result.total_count = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0 && x.Status == Status).Count();
                            Result.ordermaster = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0 && x.Status == Status).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        }

                        //Result.total_count = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).Count();
                        //Result.ordermaster = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        var listOrders = Result.ordermaster;
                        // var listOrders = context.FreeStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.FreeStockId == FreeStockId).OrderByDescending(x => x.CreationDate).Skip((page - 1) * list).Take(list).ToList();
                        foreach (var item in listOrders)
                        {
                            int PurchaseOrderId = item.PurchaseOrderId;
                            item.IsGDN = context.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsGDNGenerate == true && x.IsActive == true && x.IsDeleted == false);

                            var POMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.PRPaymentType == "AdvancePR").FirstOrDefault();
                            if (POMaster != null)
                            {
                                var Payment = context.PRPaymentAppoved.Where(x => x.PRId == POMaster.PurchaseOrderId && x.IsApproved == true && x.IsActive == true).FirstOrDefault();
                                if (Payment != null)
                                {
                                    if (Payment.IsPaymentDone == true)
                                    {

                                        item.IsAdvancePayment = true;
                                    }
                                    else
                                    {
                                        item.IsAdvancePayment = false;
                                    }
                                }
                            }
                        }
                        Result.ordermaster = listOrders;

                        return Result;
                    }
                    else
                    {
                        // var itemPagedList = context.AllPOMasterWid(list, page, Warehouseid, CompanyId);
                        PaggingData Result = new PaggingData();
                        Result.total_count = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).Count();
                        Result.ordermaster = context.DPurchaseOrderMaster.Where(x => x.WarehouseId == Warehouseid && x.Status != "Blank PO" && x.PRType == 0).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        var listOrders = Result.ordermaster;
                        // var listOrders = context.FreeStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.FreeStockId == FreeStockId).OrderByDescending(x => x.CreationDate).Skip((page - 1) * list).Take(list).ToList();
                        foreach (var item in listOrders)
                        {
                            int PurchaseOrderId = item.PurchaseOrderId;
                            item.IsGDN = context.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsGDNGenerate == true && x.IsActive == true && x.IsDeleted == false);

                            var POMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.PRPaymentType == "AdvancePR").FirstOrDefault();
                            if (POMaster != null)
                            {
                                var Payment = context.PRPaymentAppoved.Where(x => x.PRId == POMaster.PurchaseOrderId && x.IsApproved == true && x.IsActive == true).FirstOrDefault();
                                if (Payment != null)
                                {
                                    if (Payment.IsPaymentDone == true)
                                    {

                                        item.IsAdvancePayment = true;
                                    }
                                    else
                                    {
                                        item.IsAdvancePayment = false;
                                    }
                                }
                            }
                        }
                        Result.ordermaster = listOrders;
                        logger.Info("End ItemMaster: ");
                        return Result;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }



        [Route("SearchPo")]
        [HttpGet]
        public dynamic SearchPo(string PoId)
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

                int poid = 0;
                string Suplliername = "";
                bool bNum = false;
                if (PoId != null)
                {
                    int i;
                    bNum = int.TryParse(PoId, out i);
                    if (bNum)
                    {
                        poid = Convert.ToInt32(PoId);
                    }
                    else
                    {
                        Suplliername = PoId;//if search by name
                    }
                }
                using (var db = new AuthContext())
                {
                    var Podata = new List<PurchaseOrderMaster>();
                    if (Warehouse_id > 0)
                    {

                        if (bNum)
                        {
                            Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.PurchaseOrderId == poid && x.PRType == 0 && x.Status != "Blank PO").OrderByDescending(x => x.CreationDate).ToList();

                        }
                        else
                        {
                            Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.PRType == 0 && x.SupplierName.ToLower().Contains(Suplliername.Trim().ToLower()) && x.Status != "Blank PO").OrderByDescending(x => x.CreationDate).Take(10).ToList();
                        }

                        foreach (var item in Podata)
                        {
                            int PurchaseOrderId = item.PurchaseOrderId;
                            var POMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.PRPaymentType == "AdvancePR").FirstOrDefault();
                            if (POMaster != null)
                            {
                                var Payment = db.PRPaymentAppoved.Where(x => x.PRId == POMaster.PurchaseOrderId && x.IsApproved == true && x.IsActive == true).FirstOrDefault();
                                if (Payment != null)
                                {
                                    if (Payment.IsPaymentDone == true)
                                    {
                                        item.IsAdvancePayment = true;
                                    }
                                    else
                                    {
                                        item.IsAdvancePayment = false;
                                    }
                                }
                            }
                            item.IsGDN = db.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsGDNGenerate == true && x.IsActive == true && x.IsDeleted == false);

                        }

                        return Podata;
                    }
                    else
                    {
                        if (bNum)
                        {
                            Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.PurchaseOrderId == poid && x.PRType == 0 && x.Status != "Blank PO").OrderByDescending(x => x.CreationDate).ToList();
                        }
                        else
                        {
                            Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.PRType == 0 && x.SupplierName.ToLower().Contains(Suplliername.Trim().ToLower()) && x.Status != "Blank PO").OrderByDescending(x => x.CreationDate).Take(10).ToList();
                        }
                        foreach (var item in Podata)
                        {
                            int PurchaseOrderId = item.PurchaseOrderId;
                            var POMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.PRPaymentType == "AdvancePR").FirstOrDefault();
                            if (POMaster != null)
                            {
                                var Payment = db.PRPaymentAppoved.Where(x => x.PRId == POMaster.PurchaseOrderId && x.IsApproved == true && x.IsActive == true).FirstOrDefault();
                                if (Payment != null)
                                {
                                    if (Payment.IsPaymentDone == true)
                                    {

                                        item.IsAdvancePayment = true;
                                    }
                                    else
                                    {
                                        item.IsAdvancePayment = false;
                                    }
                                }
                            }
                            item.IsGDN = db.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsGDNGenerate == true && x.IsActive == true && x.IsDeleted == false);
                        }
                        return Podata;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }

        //[Route("Export")]
        //[HttpPost]
        //public HttpResponseMessage getExports(objDTO obj) //get search orders for delivery
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

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
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }
        //        //Warehouse_id = wid;
        //        using (var db = new AuthContext())
        //        {

        //            //if (Warehouse_id > 0)
        //            //{

        //            List<PurchaseOrderDetailRecived> newdata = new List<PurchaseOrderDetailRecived>();
        //            List<PurchaseOrderDetailRecivedDTM> newResultdata = new List<PurchaseOrderDetailRecivedDTM>();
        //            foreach (var item in obj.ids)
        //            {
        //                var newdatareport = db.PurchaseOrderRecivedDetails.Where(x => x.CompanyId == compid && x.WarehouseId == item.id && x.CreationDate > obj.From && x.CreationDate <= obj.TO).OrderByDescending(x => x.PurchaseOrderId).ToList();

        //                newdata.AddRange(newdatareport);
        //            }

        //            if (newdata.Count > 0)
        //            {

        //                foreach (var ipO in newdata)
        //                {

        //                    var ItemQT = ipO.QtyRecived1 + ipO.QtyRecived2 + ipO.QtyRecived3 + ipO.QtyRecived4 + ipO.QtyRecived5;

        //                    double ItemFIlRate = Convert.ToDouble(ItemQT) * 100 / Convert.ToDouble(ipO.TotalQuantity);
        //                    var GRdata = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == ipO.PurchaseOrderId).FirstOrDefault();// add by raj,this is add because we required gr date

        //                    PurchaseOrderDetailRecivedDTM b = new PurchaseOrderDetailRecivedDTM();
        //                    b.PurchaseOrderId = ipO.PurchaseOrderId;
        //                    b.QtyRecivedTotal = ipO.QtyRecived;
        //                    b.PriceRecived = ipO.PriceRecived;
        //                    b.WarehouseName = ipO.WarehouseName;
        //                    b.QtyRecived1 = ipO.QtyRecived1;
        //                    b.Price1 = ipO.Price1;
        //                    b.Gr1Date = GRdata?.Gr1_Date;// GR1 date add by raj
        //                    b.QtyRecived2 = ipO.QtyRecived2;
        //                    b.Price2 = ipO.Price2;
        //                    b.Gr2Date = GRdata?.Gr2_Date;// GR2 date add by raj
        //                    b.QtyRecived3 = ipO.QtyRecived3;
        //                    b.Price3 = ipO.Price3;
        //                    b.Gr3Date = GRdata?.Gr3_Date;// GR3 date add by raj
        //                    b.QtyRecived4 = ipO.QtyRecived4;
        //                    b.Price4 = ipO.Price4;
        //                    b.Gr4Date = GRdata?.Gr4_Date;// GR4 date add by raj
        //                    b.QtyRecived5 = ipO.QtyRecived5;
        //                    b.Price5 = ipO.Price5;
        //                    b.Gr5Date = GRdata?.Gr5_Date;// GR5 date add by raj
        //                    b.ItemName = ipO.ItemName;
        //                    //b.ItemId = ipO.ItemId;
        //                    b.ItemNumber = ipO.ItemNumber;
        //                    b.MRP = ipO.MRP;
        //                    b.CreationDate = GRdata?.CreationDate;
        //                    b.SupplierName = GRdata.SupplierName;
        //                    b.Status = GRdata.Status;
        //                    b.POItemFillRate = ItemFIlRate;
        //                    #region  Po to GR TAT  add by raj
        //                    try

        //                    {
        //                        string DiffDate = null;
        //                        if (GRdata != null)
        //                        {
        //                            if (GRdata.Gr5_Date != null && GRdata.Status == "Received")
        //                            {
        //                                DiffDate = Convert.ToString(GRdata.Gr5_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr4_Date != null && GRdata.Status == "Received")
        //                            {
        //                                DiffDate = Convert.ToString(GRdata.Gr4_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr3_Date != null && GRdata.Status == "Received")
        //                            {
        //                                DiffDate = Convert.ToString(GRdata.Gr3_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr2_Date != null && GRdata.Status == "Received")
        //                            {
        //                                DiffDate = Convert.ToString(GRdata.Gr2_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr1_Date != null && GRdata.Status == "Received")
        //                            {
        //                                DiffDate = Convert.ToString(GRdata.Gr1_Date - GRdata.CreationDate);
        //                            }
        //                            if (GRdata.Gr1_Date != null && GRdata.Status == "Received")
        //                            {
        //                                double pogrdifftime = TimeSpan.Parse(DiffDate).TotalMinutes;
        //                                TimeSpan timeSpan = TimeSpan.FromMinutes(pogrdifftime);
        //                                var hours = Math.Floor(pogrdifftime / 60);
        //                                int tt = Convert.ToInt16(hours);
        //                                var minutes = Math.Round(pogrdifftime - (hours * 60), 2);
        //                                var TotalPOGRhm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());

        //                            }
        //                        }
        //                    }
        //                    catch (Exception Ex)
        //                    {

        //                    }
        //                    #endregion
        //                    #region  GR TAT add by raj 
        //                    try
        //                    {


        //                        double AverageTAT = 0;
        //                        double TATGR1 = 0;
        //                        double TATGR2 = 0;
        //                        double TATGR3 = 0;
        //                        double TATGR4 = 0;
        //                        double TATGR5 = 0;
        //                        if (GRdata != null)
        //                        {
        //                            if (GRdata.Gr1_Date != null)
        //                            {
        //                                string GR1date = Convert.ToString(GRdata.Gr1_Date - GRdata.CreationDate);
        //                                TATGR1 = TimeSpan.Parse(GR1date).TotalMinutes;
        //                                AverageTAT = TATGR1;
        //                            }
        //                            if (GRdata.Gr2_Date != null)
        //                            {
        //                                string GR2date = Convert.ToString(GRdata.Gr2_Date - GRdata.Gr1_Date);
        //                                TATGR2 = TimeSpan.Parse(GR2date).TotalMinutes;
        //                                AverageTAT = (TATGR1 + TATGR2) / 2;
        //                            }
        //                            if (GRdata.Gr3_Date != null)
        //                            {
        //                                string GR3date = Convert.ToString(GRdata.Gr3_Date - GRdata.Gr2_Date);
        //                                TATGR3 = TimeSpan.Parse(GR3date).TotalMinutes;
        //                                AverageTAT = (TATGR1 + TATGR2 + TATGR3) / 3;
        //                            }
        //                            if (GRdata.Gr4_Date != null)
        //                            {
        //                                string GR4date = Convert.ToString(GRdata.Gr4_Date - GRdata.Gr3_Date);
        //                                TATGR4 = TimeSpan.Parse(GR4date).TotalMinutes;
        //                                AverageTAT = (TATGR1 + TATGR2 + TATGR3 + TATGR4) / 4;
        //                            }
        //                            if (GRdata.Gr5_Date != null)
        //                            {
        //                                string GR5date = Convert.ToString(GRdata.Gr5_Date - GRdata.Gr4_Date);
        //                                TATGR5 = TimeSpan.Parse(GR5date).TotalMinutes;
        //                                AverageTAT = (TATGR1 + TATGR2 + TATGR3 + TATGR4 + TATGR5) / 5;
        //                            }
        //                            if (GRdata.Gr1_Date != null)
        //                            {
        //                                TimeSpan timeSpan = TimeSpan.FromMinutes(AverageTAT);
        //                                var hours = Math.Floor(AverageTAT / 60);
        //                                var minutes = Math.Round(AverageTAT - (hours * 60), 2);
        //                                var AverageTAThm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());
        //                                b.AverageTAT = AverageTAThm;
        //                            }
        //                        }
        //                    }
        //                    catch (Exception Ex)
        //                    {

        //                    }
        //                    #endregion




        //                    newResultdata.Add(b);

        //                }
        //            }
        //            else
        //            {

        //                return Request.CreateResponse(HttpStatusCode.OK, "No Record Found");

        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, newResultdata);

        //            //}

        //            //else
        //            //{

        //            //    return Request.CreateResponse(HttpStatusCode.OK, "null");
        //            //}
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }

        //}

        [Route("Export")]
        [HttpPost]
        public async Task<HttpResponseMessage> getExports(objDTO obj)
        {
            try
            {
                logger.Info("start : ");

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (var myContext = new AuthContext())
                {

                    string Start = obj.From.ToString("yyyy-MM-dd HH:mm:ss");
                    string End = obj.TO.ToString("yyyy-MM-dd HH:mm:ss");


                    List<GRReportDC> purchaseRData = new List<GRReportDC>();
                    List<GRReportDC> addPurchaseReport = new List<GRReportDC>();
                    //var warehouseids = obj.ids.Select(x => x.id).ToList();
                    foreach (var item in obj.ids)
                    {
                        var purchaseReport = "select p.PurchaseOrderId,isnull(po.FreightCharge,cast(0 as float)) as FreightCharge,case when d.Status =1 then 'Pending for Checker Side' when d.Status = 2 then 'Approved' " +
                             " when d.Status = 3 then 'Reject'" +
                             " end as GRStatus,case when d.GrSerialNumber = 1 then 'GR1'" +
                             " when d.GrSerialNumber = 2 then 'GR2'" +
                             "when d.GrSerialNumber = 3 then 'GR3'" +
                             "when d.GrSerialNumber = 4 then 'GR4'" +
                             "when d.GrSerialNumber = 5 then 'GR5'" +                            
                             "end as GRNo, p.TotalQuantity,p.MRP,d.Qty, (d.qty * d.Price) as PriceRecived, p.WarehouseName, d.Price, d.CreatedDate as GrDate, p.ItemName,d.PurchaseOrderDetailId, " +
                             " x.InvoiceNumber,x.InvoiceDate," +
                             "p.ItemNumber,p.CreationDate,d.ItemMultiMRPId, p.SupplierName,p.WarehouseId,case when i.CompanyStockCode is not Null then  i.CompanyStockCode else '' end as CompanyStockCode,po.PickerType,im.Categoryid,im.SubCategoryId,im.SubsubCategoryid from PurchaseOrderDetails p join GoodsReceivedDetails d on" +
                             " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId inner join PurchaseOrderMasters po on p.PurchaseOrderId=po.PurchaseOrderId left join ItemMultiMRPs as i on i.ItemMultiMRPId = p.ItemMultiMRPId inner join  ItemMasters im on p.ItemId=im.ItemId" +
                             " OUTER APPly( " +
                             " select string_agg(pp.InvoiceNumber,',') InvoiceNumber,string_agg(pp.InvoiceDate, ',') InvoiceDate from "+
                             "(select Distinct GI.InvoiceNumber, GI.InvoiceDate from InvoiceReceiptImages GI with(nolock) where p.PurchaseOrderId = GI.PurchaseOrderId and d.GrSerialNumber = GI.GrSerialNumber )pp )x " +
                             " where p.CreationDate > '" + Start + "' and p.CreationDate <= ' " + End + "' and p.WarehouseId in (" + item.id + ") ";

                        var poReport = myContext.Database.SqlQuery<GRReportDC>(purchaseReport).ToList();
                        purchaseRData.AddRange(poReport);
                    }


                    if (purchaseRData.Count > 0)
                    {

                        List<ItemClassificationDC> ABCitemsList = purchaseRData.Select(item => new ItemClassificationDC { ItemNumber = item.ItemNumber, WarehouseId = item.WarehouseId }).ToList();

                        var manager = new ItemLedgerManager();
                        var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);

                        RetailerAppManager retailerAppManager = new RetailerAppManager();
                        List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                        string storeName = "";
                        foreach (var poDetails in purchaseRData)
                        {

                            var ItemQT = poDetails.Qty;

                            double ItemFIlRate = Convert.ToDouble(ItemQT) * 100 / Convert.ToDouble(poDetails.TotalQuantity);
                            var GRdata = myContext.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == poDetails.PurchaseOrderId).FirstOrDefault();// this is add because we required gr date
                            var GRList = myContext.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetailId == poDetails.PurchaseOrderDetailId).ToList();
                            var GR1 = GRList.Where(x => x.GrSerialNumber == 1).FirstOrDefault();
                            var GR2 = GRList.Where(x => x.GrSerialNumber == 2).FirstOrDefault();
                            var GR3 = GRList.Where(x => x.GrSerialNumber == 3).FirstOrDefault();
                            var GR4 = GRList.Where(x => x.GrSerialNumber == 4).FirstOrDefault();
                            var GR5 = GRList.Where(x => x.GrSerialNumber == 5).FirstOrDefault();
                            var VehicleNumber = GRList.Select(x => x.VehicleNumber).Distinct().ToList();
                            var VehicleType = GRList.Select(x => x.VehicleType).Distinct().ToList();
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == poDetails.Categoryid && x.SubCategoryId == poDetails.SubCategoryId && x.BrandId == poDetails.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == poDetails.Categoryid && x.SubCategoryId == poDetails.SubCategoryId && x.BrandId == poDetails.SubsubCategoryid);
                                storeName = store.StoreName;
                            }
                            GRReportDC gR = new GRReportDC();
                            gR.PurchaseOrderId = poDetails.PurchaseOrderId;
                            gR.TotalQuantity = poDetails.TotalQuantity;
                            gR.PriceRecived = poDetails.PriceRecived;
                            gR.WarehouseName = poDetails.WarehouseName;
                            gR.Qty = poDetails.Qty;
                            gR.GRNo = poDetails.GRNo;
                            gR.Price = poDetails.Price;
                            gR.GrDate = poDetails.GrDate;// GR1 date add by raj 
                            gR.ItemName = poDetails.ItemName;
                            gR.POStatus = poDetails.POStatus;
                            gR.ItemNumber = poDetails.ItemNumber;
                            gR.MRP = poDetails.MRP;
                            gR.CreationDate = poDetails.CreationDate;
                            gR.SupplierName = GRdata.SupplierName;
                            gR.GRStatus = GRdata.Status;
                            gR.POItemFillRate = ItemFIlRate;
                            gR.CompanyStockCode = poDetails.CompanyStockCode;
                            gR.Category = GetItem.Where(x => x.ItemNumber == poDetails.ItemNumber).Select(x => x.Category).FirstOrDefault() != null ? GetItem.Where(x => x.ItemNumber == poDetails.ItemNumber).Select(x => x.Category).FirstOrDefault() : "D";
                            gR.PickerType = poDetails.PickerType;
                            gR.storeName = storeName;
                            gR.VehicleNo = string.Join(",", VehicleNumber);
                            gR.VehicleType = string.Join(",", VehicleType);
                            gR.InvoiceNumber = poDetails.InvoiceNumber;
                            gR.InvoiceDate = poDetails.InvoiceDate;
                            gR.FreightCharge = poDetails.FreightCharge;
                            #region  Po to GR TAT
                            try

                            {

                                string DiffDate = null;
                                if (GRdata != null)
                                {
                                    if (GR5 != null && GRdata.Status == "Received")
                                    {
                                        DiffDate = Convert.ToString(GR5.CreatedDate - GRdata.CreationDate);
                                    }
                                    else if (GR4 != null && GRdata.Status == "Received")
                                    {
                                        DiffDate = Convert.ToString(GR4.CreatedDate - GRdata.CreationDate);
                                    }
                                    else if (GR3 != null && GRdata.Status == "Received")
                                    {
                                        DiffDate = Convert.ToString(GR3.CreatedDate - GRdata.CreationDate);
                                    }
                                    else if (GR2 != null && GRdata.Status == "Received")
                                    {
                                        DiffDate = Convert.ToString(GR2.CreatedDate - GRdata.CreationDate);
                                    }
                                    else if (GR1 != null && GRdata.Status == "Received")
                                    {
                                        DiffDate = Convert.ToString(GR1.CreatedDate - GRdata.CreationDate);
                                    }
                                    if (GR1 != null && GRdata.Status == "Received")
                                    {
                                        double pogrdifftime = TimeSpan.Parse(DiffDate).TotalMinutes;
                                        TimeSpan timeSpan = TimeSpan.FromMinutes(pogrdifftime);
                                        var hours = Math.Floor(pogrdifftime / 60);
                                        int tt = Convert.ToInt16(hours);
                                        var minutes = Math.Round(pogrdifftime - (hours * 60), 2);
                                        var TotalPOGRhm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());

                                    }
                                }
                            }
                            catch (Exception Ex)
                            {

                            }
                            #endregion
                            #region  GR TAT
                            try
                            {


                                double AverageTAT = 0;
                                double TATGR1 = 0;
                                double TATGR2 = 0;
                                double TATGR3 = 0;
                                double TATGR4 = 0;
                                double TATGR5 = 0;
                                if (GRdata != null)
                                {
                                    if (GR1 != null)
                                    {
                                        string GR1date = Convert.ToString(GR1.CreatedDate - GRdata.CreationDate);
                                        TATGR1 = TimeSpan.Parse(GR1date).TotalMinutes;
                                        AverageTAT = TATGR1;
                                    }
                                    if (GR2 != null && GR1 != null)
                                    {
                                        string GR2date = Convert.ToString(GR2.CreatedDate - GR1.CreatedDate);
                                        TATGR2 = TimeSpan.Parse(GR2date).TotalMinutes;
                                        AverageTAT = (TATGR1 + TATGR2) / 2;
                                    }
                                    if (GR3 != null && GR2 != null)
                                    {
                                        string GR3date = Convert.ToString(GR3.CreatedDate - GR2.CreatedDate);
                                        TATGR3 = TimeSpan.Parse(GR3date).TotalMinutes;
                                        AverageTAT = (TATGR1 + TATGR2 + TATGR3) / 3;
                                    }
                                    if (GR4 != null && GR3 != null)
                                    {
                                        string GR4date = Convert.ToString(GR4.CreatedDate - GR3.CreatedDate);
                                        TATGR4 = TimeSpan.Parse(GR4date).TotalMinutes;
                                        AverageTAT = (TATGR1 + TATGR2 + TATGR3 + TATGR4) / 4;
                                    }
                                    if (GR5 != null && GR4 != null)
                                    {
                                        string GR5date = Convert.ToString(GR5.CreatedDate - GR4.CreatedDate);
                                        TATGR5 = TimeSpan.Parse(GR5date).TotalMinutes;
                                        AverageTAT = (TATGR1 + TATGR2 + TATGR3 + TATGR4 + TATGR5) / 5;
                                    }
                                    if (GR1 != null)
                                    {
                                        TimeSpan timeSpan = TimeSpan.FromMinutes(AverageTAT);
                                        var hours = Math.Floor(AverageTAT / 60);
                                        var minutes = Math.Round(AverageTAT - (hours * 60), 2);
                                        var AverageTAThm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());
                                        gR.AverageTAT = AverageTAThm;
                                    }
                                }
                            }
                            catch (Exception Ex)
                            {

                            }
                            #endregion

                            addPurchaseReport.Add(gR);

                        }
                    }
                    else
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, "No Record Found");

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, addPurchaseReport);

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PSCostPO")]
        public HttpResponseMessage Get(PscostParam pscostParam)
        {
            using (var context = new AuthContext())
            {
                if (pscostParam.Warehouseid.Count > 0)
                {
                    if (pscostParam.StartDate == "null")
                         { pscostParam.StartDate = null; }
                    if (pscostParam.EndDate == "null") 
                         { pscostParam.EndDate = null; }
                    if (pscostParam.supplyer == "null")

                       { pscostParam.supplyer = null; }
                       
                    
                        var wIds = new DataTable();
                        wIds.Columns.Add("IntValue");
                        foreach (var item in pscostParam.Warehouseid)
                        {
                            var dr = wIds.NewRow();
                            dr["IntValue"] = item;
                            wIds.Rows.Add(dr);
                        }

                        var param = new SqlParameter
                        {
                            ParameterName= "warehouse",
                           Value= wIds
                        };
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";


                        var StartDateParam = new SqlParameter
                    {
                        ParameterName = "startDate",
                        Value = pscostParam.StartDate == null ? DBNull.Value : (object)pscostParam.StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "endDate",
                        Value = pscostParam.EndDate == null ? DBNull.Value : (object)pscostParam.EndDate

                    };

                    var SParam = new SqlParameter
                    {
                        ParameterName = "supplyer",
                        Value = pscostParam.supplyer == null ? DBNull.Value : (object)pscostParam.supplyer

                    };
                    var skip = new SqlParameter("@skip", pscostParam.skip);
                    var take = new SqlParameter("@take", pscostParam.take);


                    string ProcedureName = "PSCostPOIDWise @startDate,@endDate,@supplyer,@warehouse,@skip,@take";
                    context.Database.CommandTimeout = 1200;
                    List<POReport> Reportdata = context.Database.SqlQuery<POReport>(ProcedureName, StartDateParam, EndDateParam, SParam, param, skip, take).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, Reportdata);
                    
                  
                }
                else
                {
                    return null;
                }
            }
        }


        public class PscostParam
        {
             public  List<int> Warehouseid { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string supplyer { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
        }

        //public List<POReport> GetPurchaseOrderMaster(string ProcedureName, int Warehouseid, string StartDate, string EndDate, string supplyer)
        //{

        //    //int Skip = (page - 1) * list;
        //    //int Take = list;
        //    using (var context = new AuthContext())
        //    {
        //        var WarehouseidParam = new SqlParameter
        //        {
        //            ParameterName = "warehouse",
        //            Value = Warehouseid
        //        };

        //        var StartDateParam = new SqlParameter
        //        {
        //            ParameterName = "startDate",
        //            Value = StartDate == null ? DBNull.Value : (object)StartDate
        //        };

        //        var EndDateParam = new SqlParameter
        //        {
        //            ParameterName = "endDate",
        //            Value = EndDate == null ? DBNull.Value : (object)EndDate

        //        };

        //        var StatusParam = new SqlParameter
        //        {
        //            ParameterName = "supplyer",
        //            Value = supplyer == null ? DBNull.Value : (object)supplyer

        //        };

        //        //var TransferOrderIdParam = new SqlParameter
        //        //{
        //        //    ParameterName = "@Poid",
        //        //    Value = Poid

        //        //};


        //        List<POReport> objPurchaserOrderMasterDc = context.Database.SqlQuery<POReport>(ProcedureName, WarehouseidParam, StartDateParam,
        //        EndDateParam, StatusParam).ToList();

        //        return objPurchaserOrderMasterDc;
        //    }
        //}

        #region  Internal Ps Cost Data
        [AllowAnonymous]
        [HttpPost]
        [Route("InternalPsCost")]
        public  HttpResponseMessage GetInternalCost(Internalparam internalparam)
        {
            
            List<InternalPsCost> Internaldata = new List<InternalPsCost>();
            InternalDTO internalDTO = new InternalDTO();
            using (var db = new AuthContext())
            {
                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();
                if (internalparam.Warehouseid.Count > 0)
                {
                    if (internalparam.StartDate == "null") 
                        { internalparam.StartDate = null; }
                    if (internalparam.EndDate == "null") { internalparam.EndDate = null; }


                    string ProcedureName = "InternalItemPsCost @Warehouseid,@StartDate,@EndDate,@TransferOrderId,@skip,@take";
                    
                    var wIds = new DataTable();
                    wIds.Columns.Add("IntValue");
                    foreach (var item in internalparam.Warehouseid)
                    {
                        var dr = wIds.NewRow();
                        dr["IntValue"] = item;
                        wIds.Rows.Add(dr);
                    }

                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = wIds
                    };
                    WarehouseidParam.SqlDbType = SqlDbType.Structured;
                    WarehouseidParam.TypeName = "dbo.IntValues";

                    var StartDateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = internalparam.StartDate == null ? DBNull.Value : (object)internalparam.StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = internalparam.EndDate == null ? DBNull.Value : (object)internalparam.EndDate

                    };

                    var TransferOrderIdParam = new SqlParameter
                    {
                        ParameterName = "@TransferOrderId",
                        Value = internalparam.TransferId 

                    };
                    var skips = new SqlParameter("@skip", internalparam.skip);
                    var takes = new SqlParameter("@take", internalparam.take);

                    db.Database.CommandTimeout = 3000;
                        Internaldata =  db.Database.SqlQuery<InternalPsCost>(ProcedureName, WarehouseidParam, StartDateParam, EndDateParam, TransferOrderIdParam, skips,takes).ToList();

                    internalDTO.internalPsCostlist = Internaldata;
                    internalDTO.TotalRecords = Internaldata.Select(x => x.TotalRecords).FirstOrDefault();


                    //var cmd = context.Database.Connection.CreateCommand();
                    //cmd.CommandText = "[dbo].[InternalItemPsCost]";
                    //cmd.CommandType = CommandType.StoredProcedure;
                    ////sqlCmd.CommandTimeout = 0
                    //cmd.CommandTimeout = 600;
                    //cmd.Parameters.Add(WarehouseidParam);
                    //cmd.Parameters.Add(StartDateParam);
                    //cmd.Parameters.Add(EndDateParam);
                    //cmd.Parameters.Add(TransferOrderIdParam);
                    //cmd.Parameters.Add(new SqlParameter("@skip",internalparam.skip));
                    //cmd.Parameters.Add(new SqlParameter("@take", internalparam.take));
                    //// Run the sproc
                    //var reader = cmd.ExecuteReader();
                    //Internaldata = ((IObjectContextAdapter)context).ObjectContext.Translate<InternalPsCost>(reader).ToList();



                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Internaldata);
        }
        #endregion
        public class Internalparam
        {
            public List<int> Warehouseid { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public int TransferId { get; set; }
            public int skip { get; set; }
            public int take { get; set; }

        }

        public class POReport
        {
            public string itemnumber { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string Itemname { get; set; }
            public int PurchaseOrderId { get; set; }
            public string InvoiceNumbers { get; set; }
            public string WarehouseName { get; set; }
            public string BuyerName { get; set; }
            public string SupplierName { get; set; }
            public string Status { get; set; }
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public string SubsubcategoryName { get; set; }
            public string Brand { get; set; }
            public double PORate { get; set; }
            public int? IRTotalQty { get; set; }
            public double IRWeightAvgPrice { get; set; }
            public decimal IrAmount { get; set; }
            public double WeightPOFreightCharges { get; set; }
            public double FrieghtPerPC { get; set; }
            public double Landed_Price { get; set; }

            public int TotalRecords { get; set; }
        }
        public class InternalPsCost
        {
            public string ItemNumber { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string ItemName { get; set; }
            public string InvoiceNumber { get; set; } //Internal Transfer Detail
            public string Fromwarehouse { get; set; }
            public string ToWarehouse { get; set; }
            public string Status { get; set; }
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public string SubsubcategoryName { get; set; }
            public double ItemRatePerPc { get; set; }
            public int? TotalQuantity { get; set; }
            public double Amount { get; set; }
            public double WeightPoFreightCharge { get; set; }
            public double FreightPerPc { get; set; }
            public double LandedPricePerPc { get; set; }
            public double freaD { get; set; }
            public int WarehouseId { get; set; }
            public DateTime CreationDate { get; set; }
            public int TransferOrderId { get; set; }
            public int TotalRecords { get; set; }
        }

        public class InternalDTO
        {
            public int TotalRecords { get; set; }
            public List<InternalPsCost> internalPsCostlist { get; set; }


        }

        [Authorize]
        [Route("GetNewExprt")]
        [HttpPost]
        public List<PurchaseOrderDetailExport> PODetails(objDTO obj)
        {
            logger.Info("start : ");
            var poReport = new List<PurchaseOrderDetailExport>();
            // List<PurchaseOrderNewDTO> newdata = new List<PurchaseOrderNewDTO>();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                List<PurchaseOrderDetail> purchaseOrderDetails = new List<PurchaseOrderDetail>();

                var warehouseids = obj.ids.Select(x => x.id).ToList();
                purchaseOrderDetails = db.DPurchaseOrderDeatil.Where(x => warehouseids.Contains(x.WarehouseId.Value) && x.CreationDate > obj.From && x.CreationDate <= obj.TO).ToList();


                var PoDetailids = purchaseOrderDetails.Select(x => x.PurchaseOrderDetailId).ToList();
                var POGRDetails = db.GoodsReceivedDetail.Where(x => PoDetailids.Contains(x.PurchaseOrderDetailId) && x.IsActive).ToList();

                foreach (var purchaseOrderDetail in purchaseOrderDetails)
                {
                    PurchaseOrderDetailExport b = new PurchaseOrderDetailExport();
                    var POGR = POGRDetails.FirstOrDefault(x => x.PurchaseOrderDetailId == purchaseOrderDetail.PurchaseOrderDetailId);
                    b.PurchaseOrderId = purchaseOrderDetail.PurchaseOrderId;
                    b.ItemName = purchaseOrderDetail.ItemName;
                    b.MOQ = purchaseOrderDetail.MOQ;
                    b.Price = purchaseOrderDetail.Price;
                    b.MRP = purchaseOrderDetail.MRP;
                    b.ItemNumber = purchaseOrderDetail.ItemNumber;
                    b.Total_No_Pieces = purchaseOrderDetail.MOQ * purchaseOrderDetail.MRP;
                    b.WarehouseName = purchaseOrderDetail.WarehouseName;
                    b.SupplierName = purchaseOrderDetail.SupplierName;
                    b.DepoName = purchaseOrderDetail.DepoName;
                    b.CreationDate = purchaseOrderDetail.CreationDate;
                    //b.Grserilanumber = POGR != null?POGR.GrSerialNumber :0;
                    // b.GRDate = POGR != null ? POGR.CreatedDate: null;
                    //if(POGR != null){
                    //    b.GrSerilanumber = POGR.GrSerialNumber;
                    //    b.GRDate = POGR.CreatedDate;
                    //    b.GRQty = POGR.Qty;
                    //}
                    poReport.Add(b);

                }

            }
            return poReport;
        }


        [Route("sendApproval")]
        [HttpPut]
        public dynamic SendApproval(string PoId)
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
                using (var db = new AuthContext())
                {
                    int poid = Convert.ToInt32(PoId);
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.PurchaseOrderId == poid).ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.PurchaseOrderId == poid).ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }

        [Route("selfApproved")]
        [HttpPut]
        public dynamic SelfApproved(string PoId)
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
                int poid = Convert.ToInt32(PoId);
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.PurchaseOrderId == poid).ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = db.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.PurchaseOrderId == poid).ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }

        [Route("Received")]
        [HttpGet]
        public IEnumerable<IRData> Getdata(int WarehouseId)
        {
            logger.Info("start SupplierStatus: ");

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
                //ass = context.DPurchaseOrderMaster.Where(x => x.TotalAmount !=0 && x.WarehouseId== WarehouseId).ToList();
                using (AuthContext db = new AuthContext())
                {
                    var ass = (from i in db.FullSupplierPaymentDataDB
                               where i.WarehouseId == WarehouseId
                               join k in db.Warehouses on i.WarehouseId equals k.WarehouseId
                               join s in db.Suppliers on i.SupplierId equals s.SupplierId
                               select new IRData
                               {
                                   Id = i.FullSupplierPaymentDataId,
                                   CompanyId = i.CompanyId,
                                   WarehouseId = i.WarehouseId,
                                   SupplierId = i.SupplierId,
                                   SupplierName = s.Name,
                                   WarehouseName = k.WarehouseName,
                                   PaymentStatus = i.SupplierPaymentStatus,
                                   TotalAmount = i.InVoiceAmount,
                                   CreationDate = i.CreatedDate,
                                   PaymentTerms = s.PaymentTerms,
                                   TotalAmountRemaining = i.InVoiceRemainingAmount
                               }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        try
                        {
                            var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                            kk.PaymentTermDate = rr;
                            if (kk.PaymentTermDate < DateTime.Now)
                            {
                                kk.GonePaymentTermDate = true;
                            }
                            else
                            {
                                kk.GonePaymentTermDate = false;
                            }
                            ss.Add(kk);
                        }
                        catch (Exception EX)
                        {

                        }
                    }
                    //tt = context.DPurchaseOrderMaster.Where(x => x.Status == "pending" ).ToList();

                    return ss;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in SupplierStatus " + ex.Message);
                logger.Info("End  Supplier: ");
                return null;
            }
        }

        [Route("getDataForExcel")]
        [HttpGet]
        public List<ExcelDataForIR> getDataForExcel(int WarehouseId)
        {
            try
            {


                using (var context = new AuthContext())
                {
                    var data = (from i in context.InvoiceImageDb
                                where i.WarehouseId == WarehouseId
                                join s in context.Suppliers
                                 on i.SupplierId equals s.SupplierId
                                join ir in context.InvoiceReceiveDb on
                                i.InvoiceReceiveId equals ir.Id
                                join it in context.itemMasters on
                                i.ItemId equals it.ItemId
                                select new ExcelDataForIR
                                {
                                    SupplierGstNumber = s.TINNo,
                                    SupplierName = s.Name,
                                    InvoiceNumber = i.InvoiceNumber,
                                    InvoiceDate = i.InvoiceDate,
                                    InvoiceValue = i.IRAmount,
                                    GstRate = i.GSTPercentage,
                                    TaxableValue = i.TotalAmountWithoutTax,
                                    CGstAmount = i.CGSTAmount,
                                    SGstAmount = i.SGSTAmount,
                                    ItemName = it.itemname,
                                    Status = ir.PaymentStatus,
                                    TotalAmount = i.GSTAmount,
                                }).ToList();
                    return data;
                }
            }
            catch (Exception ee)
            {
                return null;
            }

        }

        [Route("AddSupplierPayment")]
        [HttpPost]
        public string AddSupplierPayment(SupplierPaymentData data)
        {

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

                data.CreatedDate = DateTime.Now;
                data.UpdatedDate = DateTime.Now;
                data.InVoiceDate = DateTime.Now;
                data.PaymentStatusCorD = "Debit";
                data.VoucherType = "Payment";
                using (var context = new AuthContext())
                {
                    var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).ToList();
                    if (supplierpay.Count != 0)
                    {
                        var maxTimestamp = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).Max(task => task.CreatedDate);
                        var datadate = context.SupplierPaymentDataDB.Where(x => x.CreatedDate.Year == maxTimestamp.Year &&
                        x.CreatedDate.Month == maxTimestamp.Month && x.CreatedDate.Day == maxTimestamp.Day && x.CreatedDate.Hour == maxTimestamp.Hour && x.CreatedDate.Minute == maxTimestamp.Minute &&
                        x.CreatedDate.Second == maxTimestamp.Second && x.CreatedDate.Millisecond == maxTimestamp.Millisecond &&
                        x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                        data.ClosingBalance = datadate.ClosingBalance - data.DebitInvoiceAmount;
                    }
                    else
                    {
                        data.ClosingBalance = data.DebitInvoiceAmount;
                    }
                    context.SupplierPaymentDataDB.Add(data);
                    int id = context.Commit();
                    if (id != 0)
                    {
                        //InvoiceReceive ir = context.InvoiceReceiveDb.Where(x => x.Id == data.IrId).SingleOrDefault();
                        //if (ir != null)
                        //{
                        //    if (data.PaymentMode == "Partial Payment")
                        //    {
                        //        var totalAmount = ir.TotalAmount;
                        //        var newTotalAmount = totalAmount - data.InVoiceAmount;
                        //        ir.TotalAmountRemaining = newTotalAmount;
                        //        ir.PaymentStatus = "Partial Paid";
                        //        context.Entry(ir).State = EntityState.Modified;
                        //        context.SaveChanges();
                        //    }
                        //    else
                        //    {
                        //        ir.TotalAmount = 0;
                        //        ir.PaymentStatus = "Full Paid";
                        //        var totalAmount = ir.TotalAmountRemaining;
                        //        var newTotalAmount = totalAmount - data.InVoiceAmount;
                        //        ir.TotalAmountRemaining = newTotalAmount;
                        //        context.Entry(ir).State = EntityState.Modified;
                        //        context.SaveChanges();
                        //    }
                        //}
                        //InvoiceImage ii = context.InvoiceImageDb.Where(x => x.SupplierId == data.SupplierId).SingleOrDefault();
                        if (data.PaymentType == "Partial Payment")
                        {
                            //ii.IRRemainingAmount = ii.IRRemainingAmount - data.DebitInvoiceAmount;
                            //ii.IRPaymentStatus = "Partial Paid";
                            //context.Entry(ii).State = EntityState.Modified;
                            //context.SaveChanges();
                            FullSupplierPaymentData fsp = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                            fsp.InVoiceRemainingAmount = fsp.InVoiceRemainingAmount - data.DebitInvoiceAmount;
                            fsp.SupplierPaymentStatus = "Partial Paid";
                            context.Entry(fsp).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            //ii.IRPaymentStatus = "Full Paid";
                            //ii.IRRemainingAmount = ii.IRRemainingAmount - data.DebitInvoiceAmount;
                            //context.Entry(ii).State = EntityState.Modified;
                            //context.SaveChanges();
                            FullSupplierPaymentData fsp = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                            fsp.InVoiceRemainingAmount = fsp.InVoiceRemainingAmount - data.DebitInvoiceAmount;
                            fsp.SupplierPaymentStatus = "Full Paid";
                            context.Entry(fsp).State = EntityState.Modified;
                            context.Commit();
                        }


                    }
                    return "Success";
                }
            }

            catch (Exception ee)
            {
                return ee.Message;
            }
        }



        #region Add Credit Note 
        /// <summary>
        /// add credit note amount and credit/debit note remark it changes  data.ClosingBalance can view changes on  \views\suppliers\SupplierPayments.html 
        /// created by 26/02/2019
        /// </summary>                  ---     tejas 24-04-2019
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("AddSupplierCreditNote")]
        [HttpPost]
        public string AddSupplierCreditNote(SupplierPaymentData data)
        {

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
                data.CreatedDate = DateTime.Now;
                data.UpdatedDate = DateTime.Now;
                using (var context = new AuthContext())
                {
                    var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).ToList();
                    if (supplierpay.Count != 0)
                    {
                        var maxTimestamp = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).Max(task => task.CreatedDate);
                        var datadate = context.SupplierPaymentDataDB.Where(x => x.CreatedDate.Year == maxTimestamp.Year &&
                        x.CreatedDate.Month == maxTimestamp.Month && x.CreatedDate.Day == maxTimestamp.Day && x.CreatedDate.Hour == maxTimestamp.Hour && x.CreatedDate.Minute == maxTimestamp.Minute &&
                        x.CreatedDate.Second == maxTimestamp.Second && x.CreatedDate.Millisecond == maxTimestamp.Millisecond &&
                        x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                        data.ClosingBalance = datadate.ClosingBalance - data.CreditInVoiceAmount;
                        data.InVoiceNumber = "CN" + context.SupplierPaymentDataDB.Count();
                        data.InVoiceDate = DateTime.Now;

                        /// changes made to ClosingBalance changes also have to made to InVoiceRemainingAmount 

                        FullSupplierPaymentData fsp = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                        fsp.InVoiceRemainingAmount = fsp.InVoiceRemainingAmount - data.CreditInVoiceAmount;
                        fsp.InVoiceAmount = fsp.InVoiceAmount - data.CreditInVoiceAmount;
                        fsp.SupplierPaymentLastChange = "Credit note added of:" + data.CreditInVoiceAmount;
                        context.Entry(fsp).State = EntityState.Modified;
                        context.Commit();

                    }
                    else
                    {
                        //data.ClosingBalance = data.DebitInvoiceAmount;
                    }
                    context.SupplierPaymentDataDB.Add(data);
                    int id = context.Commit();

                    return "Success";
                }
            }

            catch (Exception ee)
            {
                return ee.Message;
            }
        }
        #endregion



        #region Add debit Note 
        /// <summary>
        /// add debit note amount and credit/debit note remark it changes  data.ClosingBalance can view changes on  \views\suppliers\SupplierPayments.html 
        /// created by 26/02/2019
        /// </summary>                  ---     tejas 24-04-2019
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("AddSupplierDebitNote")]
        [HttpPost]
        public string AddSupplierDebitNote(SupplierPaymentData data)
        {

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
                data.CreatedDate = DateTime.Now;
                data.UpdatedDate = DateTime.Now;
                using (var context = new AuthContext())
                {
                    var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).ToList();
                    if (supplierpay.Count != 0)
                    {
                        var maxTimestamp = context.SupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).Max(task => task.CreatedDate);
                        var datadate = context.SupplierPaymentDataDB.Where(x => x.CreatedDate.Year == maxTimestamp.Year &&
                        x.CreatedDate.Month == maxTimestamp.Month && x.CreatedDate.Day == maxTimestamp.Day && x.CreatedDate.Hour == maxTimestamp.Hour && x.CreatedDate.Minute == maxTimestamp.Minute &&
                        x.CreatedDate.Second == maxTimestamp.Second && x.CreatedDate.Millisecond == maxTimestamp.Millisecond &&
                        x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                        data.ClosingBalance = datadate.ClosingBalance + data.DebitInvoiceAmount;
                        data.InVoiceNumber = "DN" + context.SupplierPaymentDataDB.Count();
                        data.InVoiceDate = DateTime.Now;

                        /// changes made to ClosingBalance changes also have to made to InVoiceRemainingAmount 

                        FullSupplierPaymentData fsp = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.WarehouseId == data.WarehouseId && x.CompanyId == compid).SingleOrDefault();
                        fsp.InVoiceRemainingAmount = fsp.InVoiceRemainingAmount + data.DebitInvoiceAmount;
                        fsp.InVoiceAmount = fsp.InVoiceAmount + data.DebitInvoiceAmount;
                        fsp.SupplierPaymentLastChange = "debit note added of:" + data.DebitInvoiceAmount;
                        context.Entry(fsp).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        //data.ClosingBalance = data.DebitInvoiceAmount;
                    }
                    context.SupplierPaymentDataDB.Add(data);
                    int id = context.Commit();

                    return "Success";
                }
            }

            catch (Exception ee)
            {
                return ee.Message;
            }
        }
        #endregion


        [Route("SearchIO")]
        [HttpGet]
        public IEnumerable<IRData> SearchIO(DateTime? start, DateTime? End, int WarehouseId = 0, string StatusOption = null, int SupplierId = 0)
        {

            List<IRData> ass = new List<IRData>();
            using (var context = new AuthContext())
            {
                if (WarehouseId != 0 && StatusOption != null && SupplierId != 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.WarehouseId == WarehouseId && i.SupplierPaymentStatus == StatusOption
                           && i.SupplierId == SupplierId && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();
                    List<IRData> ss = new List<IRData>();


                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }
                if (WarehouseId != 0 && StatusOption != null && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.WarehouseId == WarehouseId && i.SupplierPaymentStatus == StatusOption
                           && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();
                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (SupplierId != 0 && WarehouseId == 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.SupplierId == SupplierId
                           && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();
                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (WarehouseId != 0 && SupplierId == 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.WarehouseId == WarehouseId
                            && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();
                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (StatusOption != null && SupplierId != 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.SupplierPaymentStatus == StatusOption
                           && i.SupplierId == SupplierId && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (StatusOption != null && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.SupplierPaymentStatus == StatusOption && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (StatusOption != null && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.SupplierPaymentStatus == StatusOption && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (SupplierId != 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.SupplierId == SupplierId && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (WarehouseId != 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.WarehouseId == WarehouseId && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();

                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }

                if (StatusOption != null && WarehouseId != 0 && start != null && End != null)
                {
                    ass = (from i in context.FullSupplierPaymentDataDB
                           where i.WarehouseId == WarehouseId && i.SupplierPaymentStatus == StatusOption && i.CreatedDate >= start && i.CreatedDate <= End
                           join k in context.Warehouses on i.WarehouseId equals k.WarehouseId
                           join s in context.Suppliers on i.SupplierId equals s.SupplierId
                           select new IRData
                           {
                               Id = i.FullSupplierPaymentDataId,
                               CompanyId = i.CompanyId,
                               WarehouseId = i.WarehouseId,
                               SupplierId = i.SupplierId,
                               SupplierName = s.Name,
                               WarehouseName = k.WarehouseName,
                               PaymentStatus = i.SupplierPaymentStatus,
                               TotalAmount = i.InVoiceAmount,
                               CreationDate = i.CreatedDate,
                               PaymentTerms = s.PaymentTerms,
                               TotalAmountRemaining = i.InVoiceRemainingAmount
                           }).ToList();
                    List<IRData> ss = new List<IRData>();
                    foreach (var kk in ass)
                    {
                        var rr = kk.CreationDate.AddDays(Convert.ToDouble(kk.PaymentTerms));
                        kk.PaymentTermDate = rr;
                        if (kk.PaymentTermDate < DateTime.Now)
                        {
                            kk.GonePaymentTermDate = true;
                        }
                        else
                        {
                            kk.GonePaymentTermDate = false;
                        }
                        ss.Add(kk);
                    }
                    return ss;
                }
                return null;
            }
        }

        #region Supplier App API to get approved and self approved PO
        ///// <summary>
        ///// created by 26/02/2019
        ///// </summary>
        ///// <param name="sid"></param>
        ///// <returns></returns>
        [Route("getApprovedPo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> GetPoSended(int sid, int WarehouseId) // wid
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {

                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                                      " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + " and p.WarehouseId = " + WarehouseId + " and (p.Status = 'Approved' or  p.Status = 'Self Approved') and p.SupplierStatus =0   and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName,p.SupplierStatus,p.Status,p.WarehouseName " +
                                      " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId";
                    _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();


                    if (sid > 0 && WarehouseId > 0)
                    {
                        var SupplierId = new SqlParameter("@SupplierId", sid);
                        var wid = new SqlParameter("@WarehouseId", WarehouseId);
                        _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getApprovedPo @SupplierId,@WarehouseId", SupplierId, wid).ToListAsync();
                    }


                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };

                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                SupplierPurchaseOrderObj res = new SupplierPurchaseOrderObj()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }


        /// <summary>
        /// created by 26/02/2019
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getApprovedPoWithPage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> getApprovedPoWithPage(int sid, int PageNumber, int PageSized) // wid
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {

                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    //string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                    //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + "  and (p.Status = 'Approved' or  p.Status = 'Self Approved') and p.SupplierStatus =0   and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName,p.SupplierStatus,p.Status,p.WarehouseName " +
                    //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId Order by p.PurchaseOrderId desc OFFSET " + PageSized * (PageNumber - 1) + " ROWS FETCH NEXT " + PageSized + " ROWS ONLY";
                    //_result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();

                    if (sid > 0 && PageNumber > 0 && PageSized > 0)
                    {
                        var SupplierId = new SqlParameter("@SupplierId", sid);
                        var PNumber = new SqlParameter("@PageNumber", PageNumber);
                        var PSized = new SqlParameter("@PageSized", PageSized);
                        _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getApprovedPo @SupplierId,@PageNumber,@PageSized", SupplierId, PNumber, PSized).ToListAsync();
                    }
                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };

                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                SupplierPurchaseOrderObj res = new SupplierPurchaseOrderObj()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion


        #region Supplier App API to get in progress PO
        ///// <summary>
        ///// created by tejas 07/2019
        ///// </summary>   
        ///// <param name="sid"></param>
        ///// <returns></returns>
        //[Route("getOngoingPo")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<HttpResponseMessage> getOngoingPo(int sid, int WarehouseId)
        //{
        //    try
        //    {
        //        SupplierPurchaseOrderObj res;
        //        using (var db = new AuthContext())
        //        {
        //            List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
        //            //string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
        //            //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + " and p.WarehouseId = " + WarehouseId + " and (p.Status = 'UN Received' or  p.Status = 'UN Partial Received' or (p.SupplierStatus=1 and p.Status not in('CN Received', 'CN Partial Received')))  and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName " +
        //            //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId";
        //            //_result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();

        //            if (sid > 0 && WarehouseId > 0)
        //            {

        //                var SupplierId = new SqlParameter("@SupplierId", sid);
        //                var wid = new SqlParameter("@WarehouseId", WarehouseId);


        //                _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getOngoingPo @SupplierId,@WarehouseId", SupplierId, wid).ToListAsync();
        //            }
        //            res = new SupplierPurchaseOrderObj()
        //            {
        //                pom = _result,
        //                Status = true,
        //                Message = "Success."
        //            };
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //    catch (Exception ex)
        //    {
        //        suppAppData res = new suppAppData()
        //        {
        //            pom = null,
        //            Status = false,
        //            Message = ex.Message
        //        };
        //        return Request.CreateResponse(HttpStatusCode.BadGateway, res);
        //    }
        //}

        [Route("getOngoingPoWithPage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> getOngoingPoWithPage(int sid, int PageNumber, int PageSized,int PoId)
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {
                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    //string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                    //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + " and ((p.Status in('CN Received', 'CN Partial Received') and (p.IrStatus != 'Approved from Buyer side' or p.IrStatus is null)))  and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName " +
                    //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId Order by p.PurchaseOrderId desc OFFSET  " + PageSized * (PageNumber - 1) + "  ROWS FETCH NEXT  " + PageSized + "  ROWS ONLY";
                    //_result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();

                    if (sid > 0 && PageNumber > 0 && PageSized > 0)
                    {
                        var SupplierId = new SqlParameter("@SupplierId", sid);
                        var PNumber = new SqlParameter("@PageNumber", PageNumber);
                        var PSized = new SqlParameter("@PageSized", PageSized);
                        var PoIdParam = new SqlParameter("@PoId", PoId);
                        _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getOngoingPo @SupplierId,@PageNumber,@PageSized,@PoId", SupplierId, PNumber, PSized, PoIdParam).ToListAsync();
                    }



                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion


        #region Supplier App API to get in progress PO
        ///// <summary>
        ///// created by tejas 07/2019
        ///// </summary>   
        ///// <param name="sid"></param>
        ///// <returns></returns>
        //[Route("getPastPo")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<HttpResponseMessage> getPastPo(int sid, int WarehouseId)
        //{
        //    try
        //    {
        //        SupplierPurchaseOrderObj res;
        //        using (var db = new AuthContext())
        //        {
        //            List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
        //            //string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
        //            //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + " and p.WarehouseId = " + WarehouseId + " and (p.Status = 'CN Received' or p.Status = 'Received' or  p.Status = 'CN Partial Received') and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName " +
        //            //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId";
        //            // _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();


        //            if (sid > 0 && WarehouseId > 0)
        //            {
        //                var SupplierId = new SqlParameter("@SupplierId", sid);
        //                var wid = new SqlParameter("@WarehouseId", WarehouseId);

        //                _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getPastPo @SupplierId,@WarehouseId", SupplierId, wid).ToListAsync();
        //            }


        //            res = new SupplierPurchaseOrderObj()
        //            {
        //                pom = _result,
        //                Status = true,
        //                Message = "Success."
        //            };
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //    catch (Exception ex)
        //    {
        //        suppAppData res = new suppAppData()
        //        {
        //            pom = null,
        //            Status = false,
        //            Message = ex.Message
        //        };
        //        return Request.CreateResponse(HttpStatusCode.BadGateway, res);
        //    }
        //}

        /// <summary>
        /// created by tejas 07/2019
        /// </summary>   
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getPastPoWithPage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> getPastPoWithPage(int sid, int PageNumber, int PageSized,int PoId)
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {
                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    //string sqlquery = "select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                    //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + sid + " and (p.IrStatus = 'Approved from Buyer side') and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName " +
                    //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId Order by p.PurchaseOrderId desc OFFSET " + PageSized * (PageNumber - 1) + "  ROWS FETCH NEXT  " + PageSized + " ROWS ONLY";
                    //_result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();
                    if (sid > 0 && PageNumber > 0 && PageSized > 0)
                    {
                        var SupplierId = new SqlParameter("@SupplierId", sid);
                        var PNumber = new SqlParameter("@PageNumber", PageNumber);
                        var PSized = new SqlParameter("@PageSized", PageSized);
                        var PoIdParam = new SqlParameter("@PoId", PoId);
                        _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec getPastPo @SupplierId,@PageNumber,@PageSized,@PoId", SupplierId, PNumber, PSized, PoIdParam).ToListAsync();
                    }

                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion


        #region Supplier App API to get completed PO and Approved IR  
        /// <summary>
        /// created by tejas 07/2019
        /// </summary>   
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getCompletedPOandIR")]
        [HttpGet]
        [AllowAnonymous]  // remove from final build
        public async Task<HttpResponseMessage> getCompletedPOandIR(int sid, int WarehouseId)  // sid is Supplier ID
        {
            try
            {
                suppAppData1 res;
                using (var db = new AuthContext())
                {
                    var ass = (from i in db.IRMasterDB
                               where i.supplierId == sid && i.WarehouseId == WarehouseId && (i.IRStatus == "Approved from Buyer side" || i.IRStatus == "Paid")
                               join k in db.DPurchaseOrderMaster on i.PurchaseOrderId equals k.PurchaseOrderId
                               join j in db.DPurchaseOrderDeatil on i.PurchaseOrderId equals j.PurchaseOrderId
                               group j by new { i.PurchaseOrderId, k.Status, i.TotalAmount, i.CreationDate, i.SupplierName, i.IRStatus, i.PaymentStatus, i.IRID, k.progress, k.IR_Progress, i.IRType }
                           into result
                               select new itemDetailsAppDatav2
                               {
                                   Count = result.Count(),
                                   PurchaseOrderId = result.Key.PurchaseOrderId,
                                   Status = result.Key.Status,
                                   TotalAmount = result.Key.TotalAmount,
                                   CreationDate = result.Key.CreationDate,
                                   SupplierName = result.Key.SupplierName,
                                   IRStatus = result.Key.IRStatus,
                                   PaymentStatus = result.Key.PaymentStatus,
                                   Invoice = result.Key.IRID,
                                   progress = result.Key.progress,
                                   IR_Progress = result.Key.IR_Progress,
                                   IRType = result.Key.IRType,
                               }).OrderByDescending(b => b.PurchaseOrderId).ToList();




                    res = new suppAppData1()
                    {
                        pom = ass,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData1 res = new suppAppData1()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Get PO Detail
        /// <summary>
        /// created by 26/02/2019
        /// </summary>
        /// <param name="poid"></param>
        /// <returns></returns>
        [Route("getPoDetail")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetPoDetail(int poid)
        {
            try
            {
                suppPoDetailData res;
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderDetail> po = await db.DPurchaseOrderDeatil.Where(a => a.PurchaseOrderId == poid).ToListAsync();

                    res = new suppPoDetailData()
                    {
                        pom = po,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppPoDetailData res = new suppPoDetailData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Accept/Reject PO by Supplier 
        /// <summary>
        /// created by 26/02/2019
        /// Supplier can change following status
        /// Accepted by supplier
        /// Rejected by supplier
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("AcceptRejectPO")]
        [HttpPut]
        public async Task<HttpResponseMessage> AcceptRejectPObySupp(AcceptRejectDTO obj)
        {
            try
            {
                POResDTO res;
                using (var db = new AuthContext())
                {
                    PurchaseOrderMaster po = await db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.SupplierId == obj.SupplierId).SingleOrDefaultAsync();
                    if (po != null && (po.Status == "Approved" || po.Status == "Self Approved"))
                    {
                        //if (obj.SupplierRejectReason != null)
                        //{
                        //    po.Comment = "Reason of rejection of PO:" + obj.PurchaseOrderId + " from supplier app :" + obj.SupplierRejectReason;
                        //}
                        //po.SupplierStatus = obj.SupplierStatus;
                        //db.Entry(po).State = EntityState.Modified;
                        //db.Commit();
                        //res = new POResDTO()
                        //{

                        //    Status = true,
                        //};
                        //return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    res = new POResDTO()
                    {

                        Status = true,
                        Message = "PO can't not rejected due to current status in : " + po.Status
                    };
                }
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
            catch (Exception ex)
            {
                suppPoDetailData res = new suppPoDetailData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Supplier get po bases of status App API
        /// <summary>
        /// created by 26/02/2019
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getSupPoByStatus")]
        [HttpPost]
        public async Task<HttpResponseMessage> getSupPoByStatus(PostTDO obj)
        {
            try
            {
                suppAppData res;
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> po = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == obj.SupplierId && a.Status == obj.Status && a.IsSendSupplierApp == true).ToListAsync();

                    res = new suppAppData()
                    {
                        pom = po,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        //#region Supplier App get accepted & rejected PO API
        ///// <summary>
        ///// created by 28/02/2019
        ///// </summary>
        ///// <param name="sid"></param>
        ///// <returns></returns>
        //[Route("getAccRejPo")]
        //[HttpGet]
        //public async Task<HttpResponseMessage> GetPoAccRej(int sid)
        //{
        //    try
        //    {
        //        suppAppData res;
        //        using (var db = new AuthContext())
        //        {
        //            List<PurchaseOrderMaster> po = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == sid && a.IsSendSupplierApp == true).ToListAsync();
        //            foreach (PurchaseOrderMaster a in po)
        //            {
        //                a.Itemcount = db.DPurchaseOrderDeatil.Where(s => s.PurchaseOrderId == a.PurchaseOrderId).Count();
        //            }
        //            res = new suppAppData()
        //            {
        //                pom = po.Where(a => a.Status == "Accepted by supplier" || a.Status == "Rejected by supplier" || a.Status == "Received" || a.Status == "Partial Received").ToList(),
        //                Status = true,
        //                Message = "Success."
        //            };
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //    catch (Exception ex)
        //    {
        //        suppAppData res = new suppAppData()
        //        {
        //            pom = null,
        //            Status = false,
        //            Message = ex.Message
        //        };
        //        return Request.CreateResponse(HttpStatusCode.BadGateway, res);
        //    }
        //}
        //#endregion

        #region Supplier App get Partial Received && Received PO API
        /// <summary>
        /// created by 04/03/2019
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getReceivedPo")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetRecievedPO(int sid)
        {
            try
            {
                suppAppData res;
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> po = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == sid && a.IsSendSupplierApp == true).ToListAsync();
                    foreach (PurchaseOrderMaster a in po)
                    {
                        a.Itemcount = db.PurchaseOrderRecivedDetails.Where(s => s.PurchaseOrderId == a.PurchaseOrderId).Count();
                    }
                    res = new suppAppData()
                    {
                        pom = po.Where(a => a.Status == "Received" && a.Status == "Partial Received").ToList(),

                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Supplier App get Partial Received PO API
        /// <summary>
        /// created by 04/03/2019
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("getPartialReceivedPo")]
        [HttpGet]
        public async Task<HttpResponseMessage> getPartialReceivedPo(int sid)
        {
            try
            {
                suppAppData res;
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> po = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == sid && a.IsSendSupplierApp == true).ToListAsync();

                    res = new suppAppData()
                    {
                        pom = po.Where(a => a.Status == "Partial Received").ToList(),
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Get Recieved PO Detail
        /// <summary>
        /// created by 04/03/2019
        /// Get purchase order detail recieved
        /// </summary>
        /// <param name="poid"></param>
        /// <returns></returns>
        [Route("getPoReceivedDetail")]
        [HttpGet]
        public async Task<HttpResponseMessage> getPoReceivedDetail(int poid)
        {
            try
            {
                suppPoDetailRecievedData res;
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderDetailRecived> po = await db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == poid).ToListAsync();

                    res = new suppPoDetailRecievedData()
                    {
                        pom = po,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppPoDetailRecievedData res = new suppPoDetailRecievedData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region Tat
        [Route("gettat")]
        [HttpGet]
        public async Task<HttpResponseMessage> tot(int sid)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> pm = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == sid && a.IsSendSupplierApp == true).ToListAsync();
                    tatcount s = new tatcount
                    {
                        Accepted_by_supplier = pm.Where(z => z.Status == "Accepted by supplier").Count(),
                        Rejected_by_supplier = pm.Where(z => z.Status == "Rejected by supplier").Count(),
                        Partial_Received = pm.Where(z => z.Status == "Partial Received").Count(),
                        Received = pm.Where(z => z.Status == "Received").Count(),
                        UnPaid = pm.Where(z => z.Status == "UnPaid").Count(),
                        Paid = pm.Where(z => z.Status == "Paid").Count()
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, s);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadGateway, ex.Message);
            }
        }
        #endregion

        #region Supplier App get approved PO by wid & sid PO API
        /// <summary>
        /// created by 14/03/2019        
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("GetApprovedPobyWidSid")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetApprovedPobyWidSid(ObjSidWid obj)
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {

                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    string sqlquery = "select p.PurchaseOrderId,p.SupplierName,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                                      " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + obj.Sid + " and p.WarehouseId = " + obj.Wid + " and (p.Status = 'Self Approved' or  p.Status = 'Approved' or p.SupplierStatus!=2) and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName,p.Status,p.WarehouseName " +
                                      " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId";
                    _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();

                    // List<PurchaseOrderMaster> po = await db.DPurchaseOrderMaster.Where(a => a.SupplierId == obj.Sid  && (a.Status == "Approved" || a.Status == "Self Approved") && a.WarehouseId == obj.Wid && a.SupplierStatus!=2).ToListAsync();

                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                SupplierPurchaseOrderObj res = new SupplierPurchaseOrderObj()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region get IR for buyer approver

        [Route("buyer")]
        [HttpPost]
        public PaggingData GetIr(objDTIR objIR)
        {
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    PaggingData obj = new PaggingData();
                    int Warehouseid = objIR.WHID;
                    List<string> status = objIR.status;
                    DateTime? TO = objIR.TO;
                    DateTime? From = objIR.From;
                    int list = objIR.list;
                    int page = objIR.page;

                    string sqlquery = "SELECT * FROM IRMasters INNER JOIN IRApprovalStatus"
                + " ON IRMasters.Id = IRApprovalStatus.IRMasterId and  IRMasters.PurchaseOrderId = IRApprovalStatus.PurchaseOrderId  where"
                + " IRApprovalStatus.BuyerId =" + userid
                + " and IRMasters.WarehouseId = " + Warehouseid + "and IRApprovalStatus.IsActive = 1 and IRMasters.Deleted=0 ";


                    List<NewIRMasterDTO> IRList = context.Database.SqlQuery<NewIRMasterDTO>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                    // return PRSts;

                    IList<string> statuslist = new List<string>();
                    statuslist.Add("Pending from Buyer side");
                    statuslist.Add("Approved from Buyer side");
                    statuslist.Add("Rejected from Buyer side");

                    if (Warehouseid > 0 && status != null && status.Count > 0 && From != null && TO != null)
                    {
                        obj = new PaggingData();
                        obj.total_count = IRList.Where(x => x.WarehouseId == Warehouseid && status.Contains(x.IRStatus) && x.CreationDate > From && x.CreationDate <= TO).OrderByDescending(x => x.PurchaseOrderId).Count();
                        obj.ordermaster = IRList.Where(x => x.WarehouseId == Warehouseid && status.Contains(x.IRStatus) && x.CreationDate > From && x.CreationDate <= TO).OrderByDescending(x => x.PurchaseOrderId).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        return obj;

                    }
                    if (Warehouseid > 0 && status != null && status.Count > 0)
                    {
                        obj = new PaggingData();
                        obj.total_count = IRList.Where(x => x.WarehouseId == Warehouseid && status.Contains(x.IRStatus)).OrderByDescending(x => x.PurchaseOrderId).Count();
                        obj.ordermaster = IRList.Where(x => x.WarehouseId == Warehouseid && status.Contains(x.IRStatus)).OrderByDescending(x => x.PurchaseOrderId).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        return obj;
                    }
                    else if (Warehouseid > 0)
                    {
                        obj = new PaggingData();
                        obj.total_count = IRList.Where(x => x.WarehouseId == Warehouseid && statuslist.Contains(x.IRStatus)).OrderByDescending(x => x.PurchaseOrderId).Count();
                        obj.ordermaster = IRList.Where(x => x.WarehouseId == Warehouseid && statuslist.Contains(x.IRStatus)).OrderByDescending(x => x.PurchaseOrderId).OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        return obj;
                    }
                    return obj;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }



        #region
        [HttpGet]
        [Route("SearchIR")]
        public List<NewIRMasterDTO> GetIRReca(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start : ");
                List<NewIRMasterDTO> IRRec = new List<NewIRMasterDTO>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    // IRRec = context.IRMasterDB.AsEnumerable().Where(x => x.PurchaseOrderId == PurchaseOrderId).Where(a => a.IRStatus == "Pending from Buyer side" || a.IRStatus == "Approved from Buyer side" || a.IRStatus == "Rejected from Buyer side").OrderByDescending(x => x.PurchaseOrderId).ToList();
                    string sqlquery = "SELECT * FROM IRMasters INNER JOIN IRApprovalStatus"
                    + " ON IRMasters.Id = IRApprovalStatus.IRMasterId where"
                    + " IRApprovalStatus.BuyerId =" + userid
               + " and IRApprovalStatus.PurchaseOrderId = " + PurchaseOrderId + "and IRApprovalStatus.IsActive = 1";


                    IRRec = context.Database.SqlQuery<NewIRMasterDTO>(sqlquery).Where(a => a.IRStatus == "Pending from Buyer side" || a.IRStatus == "Approved from Buyer side" || a.IRStatus == "Rejected from Buyer side").OrderByDescending(a => a.PurchaseOrderId).ToList();
                    logger.Info("End  : ");
                    return IRRec;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        #endregion


        [Route("BuyerDateRange")]
        [HttpGet]
        public PaggingData GetIrDatewise(int list, int page, int Warehouseid, DateTime? start, DateTime? end)
        {
            logger.Info("start ItemMaster: ");
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
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var context = new AuthContext())
                {
                    if (Warehouseid > 0 && start != null)
                    {
                        PaggingData obj = new PaggingData();
                        obj.total_count = context.IRMasterDB.Where(x => x.WarehouseId == Warehouseid && x.BuyerId == userid && x.CreationDate >= start && x.CreationDate <= end).Where(a => a.IRStatus == "Pending from Buyer side" || a.IRStatus == "Approved from Buyer side" || a.IRStatus == "Rejected from Buyer side").Count();
                        obj.ordermaster = context.IRMasterDB.AsEnumerable().Where(x => x.WarehouseId == Warehouseid && x.BuyerId == userid && x.CreationDate >= start && x.CreationDate <= end).Where(a => a.IRStatus == "Pending from Buyer side" || a.IRStatus == "Approved from Buyer side" || a.IRStatus == "Rejected from Buyer side").OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
                        return obj;
                    }
                    else
                    {
                        var itemPagedList = context.AllIRMasterWid(list, page, Warehouseid, CompanyId, userid);
                        logger.Info("End ItemMaster: ");
                        return itemPagedList;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }

        [Route("buyerdetail")]
        [HttpGet]
        public IRMaster GetIrDetail(string IRID, int POID)
        {
            logger.Info("start ItemMaster: ");
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
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                    IRMaster IRM = db.IRMasterDB.Where(q => q.IRID == IRID && q.PurchaseOrderId == POID).SingleOrDefault();
                    List<IR_Confirm> IRItemDetail = db.IR_ConfirmDb.Where(q => q.PurchaseOrderId == IRM.PurchaseOrderId).ToList();
                    IRM.purDetails = IRItemDetail;
                    return IRM;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }

        /// <summary>
        /// Created by 20/03/2019
        /// Accept status == IrApproved
        /// Reject status == IrRejected
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("AcpRejIr")]
        [HttpPost]
        public PurchaseOrderMaster AccepIr(IrAcRjDto obj)
        {
            logger.Info("start ItemMaster: ");
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
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var db = new AuthContext())
                {
                    PurchaseOrderMaster pom = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                    IRMaster irm = db.IRMasterDB.Where(q => q.IRID == obj.IRID && q.PurchaseOrderId == obj.PurchaseOrderId).SingleOrDefault();
                    if (pom != null)
                    {
                        pom.IrStatus = obj.IrStatus;
                        pom.IrRejectComment = obj.IrRejectComment;
                        //db.DPurchaseOrderMaster.Attach(pom);
                        db.Entry(pom).State = EntityState.Modified;
                        db.Commit();
                    }
                    if (irm != null)
                    {
                        irm.RejectedComment = obj.IrRejectComment;
                        irm.IRStatus = obj.IrStatus;
                        db.IRMasterDB.Attach(irm);
                        db.Entry(irm).State = EntityState.Modified;
                        db.Commit();
                    }
                    return pom;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }

        [Route("SearchIOData")]
        [HttpGet]

        public List<PurchaseOrderMaster> SearchIOData(int PurchaseOrderId)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> purchaseOrderMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).Where(a => a.IrStatus == "pending" || a.IrStatus == "IrApproved" || a.IrStatus == "IrRejected").ToList();
                    return purchaseOrderMaster;
                }
            }
            catch (Exception ee)
            {
                return null;
            }

        }

        #region get data PO dashboard
        /// <summary>
        /// Created Date:07/06/2019
        /// Crreated By raj
        /// </summary>
        /// <returns></returns>
        [Route("podashboard")]
        [HttpGet]
        public HttpResponseMessage PODashBoard(DateTime? start, DateTime? end, int wid) //get search orders for delivery
        {
            //int? SupplierId
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                double TotalTATTime = 0;
                double TotalFillRate = 0;
                long TotalManualPO = 0;
                long TotalAutoPO = 0;
                long TotalSKVehicle = 0;
                long TotalSupplierVehicle = 0;
                double count = 0;
                int TotalGRN = 0;

                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);


                PODashboardDTO dashboard = new PODashboardDTO();
                using (var db = new AuthContext())
                {
                    List<AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder.GoodsReceivedDetailDc> newdata = new List<AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder.GoodsReceivedDetailDc>();



                    DateTime? RecivedDate;
                    double? TotalRatioPo = 0;
                    //&& x.SupplierId == SupplierId
                    List<PurchaseOrderMaster> podata = db.DPurchaseOrderMaster.Where(x => x.PRType == 0 && x.WarehouseId == wid && x.CreationDate > start && x.CreationDate <= end ).Include(x => x.PurchaseOrderDetail).ToList();
                    List<int> purchaseorderdetailids = podata.SelectMany(x => x.PurchaseOrderDetail.Select(y => y.PurchaseOrderDetailId)).ToList();


                    newdata = db.GoodsReceivedDetail.Where(x => purchaseorderdetailids.Contains(x.PurchaseOrderDetailId) && x.Status == 2)
                        .Select(x => new AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder.GoodsReceivedDetailDc
                        {
                            ApprovedBy = x.ApprovedBy,
                            Barcode = x.Barcode,
                            BatchNo = x.BatchNo,
                            Comment = x.Comment,
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            CurrentStockHistoryId = x.CurrentStockHistoryId,
                            DamageQty = x.DamageQty,
                            ExpiryQty = x.ExpiryQty,
                            GrSerialNumber = x.GrSerialNumber,
                            Id = x.Id,
                            IsActive = x.IsActive,
                            IsDeleted = x.IsDeleted,
                            IsFreeItem = x.IsFreeItem,
                            ItemMultiMRPId = x.ItemMultiMRPId,
                            MFGDate = x.MFGDate,
                            ModifiedBy = x.ModifiedBy,
                            ModifiedDate = x.ModifiedDate,
                            Price = x.Price,
                            PurchaseOrderDetailId = x.PurchaseOrderDetailId,
                            PurchaseOrderId = x.PurchaseOrderDetail.PurchaseOrderId,
                            Qty = x.Qty,
                            Status = x.Status,
                            VehicleNumber = x.VehicleNumber,
                            VehicleType = x.VehicleType
                        })
                        .ToList();
                    count = newdata.Count();



                    foreach (var pocreateddate in podata)
                    {
                        DateTime? CreatedDate = pocreateddate.CreationDate;
                        DateTime CD = DateTime.Now.AddHours(-48);
                        DateTime? ReceivedDate = null;

                        var lastGr = newdata.Where(x => x.PurchaseOrderId == pocreateddate.PurchaseOrderId).OrderByDescending(x => x.GrSerialNumber).FirstOrDefault();
                        if (lastGr != null)
                        {

                            ReceivedDate = lastGr.CreatedDate;
                            double hour = 0;
                            if (ReceivedDate != null)
                            {
                                String date = Convert.ToString(ReceivedDate - CreatedDate);
                                TimeSpan diff = TimeSpan.Parse(date);
                                hour = diff.TotalHours;
                            }
                            if (hour > 48)
                            {
                                double? TotalQty = 0;
                                double? ratioparticulerpo = 0;
                                var Grs = newdata.Where(x => x.PurchaseOrderId == pocreateddate.PurchaseOrderId).ToList();
                                TotalQty = pocreateddate.PurchaseOrderDetail.Sum(x => x.TotalQuantity);
                                double ReceivedQty = Grs.Sum(x => x.Qty);
                                ratioparticulerpo = ReceivedQty / TotalQty;

                                TotalRatioPo += ratioparticulerpo;
                            }
                        }

                        #region Manual po And Automatic PO
                        if (pocreateddate.PoType == "Manual")
                        {
                            TotalManualPO++;
                        }
                        //else if (pocreateddate.PoType == "Automated")
                        //{
                        //    TotalAutoPO++;
                        //}
                        #endregion
                        #region  Po to GR TAT  add by raj

                        var POGrs = newdata.Where(x => x.PurchaseOrderId == pocreateddate.PurchaseOrderId).ToList();
                        string DiffDate = null;
                        if (POGrs != null && POGrs.Any() && (pocreateddate.Status == "CN Received" || pocreateddate.Status == "CN Partial Received"))
                        {

                            var ItemQT = POGrs.Sum(x => x.Qty);
                            var POQty = pocreateddate.PurchaseOrderDetail.Sum(x => x.TotalQuantity);
                            double ItemFIlRate = Convert.ToDouble(ItemQT) * 100 / Convert.ToDouble(POQty);
                            TotalFillRate += ItemFIlRate;

                            var lastPOGr = POGrs.OrderByDescending(x => x.GrSerialNumber).FirstOrDefault();
                            if (lastPOGr != null)
                            {
                                RecivedDate = lastPOGr.CreatedDate;
                                DiffDate = Convert.ToString(lastPOGr.CreatedDate - pocreateddate.CreationDate);

                                TotalGRN++;
                                double pogrdifftime = TimeSpan.Parse(DiffDate).TotalMinutes;
                                TimeSpan timeSpan = TimeSpan.FromMinutes(pogrdifftime);
                                TotalTATTime += timeSpan.TotalMilliseconds;

                            }
                        }


                        #endregion
                        #region Total Vehicle

                        foreach (var GRdata in POGrs)
                        {
                            if (GRdata.VehicleType == "Supplier Vehicle")
                            {
                                TotalSupplierVehicle++;
                            }
                            else if (GRdata.VehicleType == "SK Vehicle")
                            {
                                TotalSKVehicle++;
                            }
                        }


                        #endregion


                    }

                    if (TotalGRN > 0)
                    {
                        double minute = (TotalTATTime / 60000);
                        var hours = Math.Round(Math.Floor(minute / 60) / TotalGRN);
                        int tt = Convert.ToInt16(hours / TotalGRN);
                        var minutes = Math.Round((minute - (hours * 60)) / TotalGRN, 0);


                        var TotalPOGRhm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());

                        // var totaltat = TotalPOGRhm / TotalGRN;

                        dashboard.TotalFillRatePO = Math.Round((TotalFillRate / count), 2);
                        dashboard.TotalTATPO = TotalPOGRhm;
                    }
                    else
                    {
                        dashboard.TotalFillRatePO = 0;
                        // dashboard.TotalTATPO = 0;

                    }
                    dashboard.TotalManualPO = TotalManualPO;
                    //dashboard.TotalAutoPO = TotalAutoPO;
                    dashboard.TotalSKVehicle = TotalSKVehicle;
                    dashboard.TotalSupplierVehicle = TotalSupplierVehicle;
                    dashboard.GRNBeyond48hr = Convert.ToInt16(TotalRatioPo);

                    // }

                    string query = "Select count(a.PurchaseOrderId) TotalPR,isnull(cast(sum(round(isnull(a.ETotalAmount,0),0) ) as int),0) TotalPRAmount, count(case when a.PRStatus=3 then 1 else null end) TotalPRApprovalPending, isnull(cast(sum(case when a.PRStatus=3 then round(isnull(a.ETotalAmount,0),0) else 0 end) as int),0) TotalPRApprovalPendingAmt, count(case when b.PrPaymentStatus='Approved' and b.PrPaymentStatus is not null then 1 else null end) TotalPRApproved,  "
                                 + "  isnull(cast(sum(case when b.PrPaymentStatus='Approved' and b.PrPaymentStatus is not null then isnull(PaidAmount,0) else 0 end) as int),0) TotalPRApprovedAmt,  count(case when b.PrPaymentStatus='Rejected' and b.PrPaymentStatus is not null then 1 else null end) TotalPRRejected,   isnull(cast(sum(case when b.PrPaymentStatus='Rejected' and b.PrPaymentStatus is not null then isnull(b.TotalAmount,0) else 0 end) as int),0) TotalPRRejectedAmt , "
                                 + "  count(case when a.PRStatus=5 and a.PRPaymentType = 'AdvancePR' and b.PrPaymentStatus not in ('Approved','Rejected') then 1 else null end) TotalPRPending, isnull(cast(sum(case when a.PRStatus=5 and a.PRPaymentType = 'AdvancePR' and b.PrPaymentStatus not in ('Approved','Rejected') then round(isnull(a.ETotalAmount,0),0) else 0 end) as int),0) TotalPRPendingAmt  "
                                 + " from PurchaseOrderMasters a left join PurchaseRequestPayments b on a.PurchaseOrderId=b.PurchaseOrderId   where  a.IsPR=1 and a.WarehouseId=" + wid + " and a.CreationDate > '" + start.Value.ToString("yyyy-MM-dd") + "' and a.CreationDate <= '" + end.Value.ToString("yyyy-MM-dd ") + " 23:59:59'";
                    //if (SupplierId.HasValue)
                    //{
                    //    query += " and a.SupplierId=" + SupplierId.Value;
                    //}
                    PRDashboardDTO pRDashboardDTO = db.Database.SqlQuery<PRDashboardDTO>(query).FirstOrDefault();
                    if (pRDashboardDTO != null)
                    {
                        dashboard.TotalPR = pRDashboardDTO.TotalPR;
                        dashboard.TotalPRAmount = pRDashboardDTO.TotalPRAmount;
                        dashboard.TotalPRApprovalPending = pRDashboardDTO.TotalPRApprovalPending;
                        dashboard.TotalPRApprovalPendingAmt = pRDashboardDTO.TotalPRApprovalPendingAmt;
                        dashboard.TotalPRApprovedAmt = pRDashboardDTO.TotalPRApprovedAmt;
                        dashboard.TotalPRApproved = pRDashboardDTO.TotalPRApproved;
                        dashboard.TotalPRRejected = pRDashboardDTO.TotalPRRejected;
                        dashboard.TotalPRRejectedAmt = pRDashboardDTO.TotalPRRejectedAmt;
                        dashboard.TotalPRPending = pRDashboardDTO.TotalPRPending;
                        dashboard.TotalPRPendingAmt = pRDashboardDTO.TotalPRPendingAmt;
                    }
                    var warehouseid = new SqlParameter
                    {
                        ParameterName = "@wid",
                        Value = wid
                    };
                    var startdate = new SqlParameter
                    {
                        ParameterName = "@start",
                        Value = start
                    };
                    var enddate = new SqlParameter
                    {
                        ParameterName = "@end",
                        Value = end
                    };
                    POdashboardDC data = db.Database.SqlQuery<POdashboardDC>("exec Sp_Podashboarddetail @wid,@start,@end", warehouseid, startdate, enddate).FirstOrDefault();
                    if (data != null)
                    {
                        dashboard.TotalGrnValue = Math.Round(data.TotalGrnValue, 2);
                        dashboard.PickupValue = Math.Round(data.PickupValue, 2);
                        dashboard.PickupValuefor = Math.Round(data.PickupValuefor, 2);
                        dashboard.PickupValuefreight = Math.Round(data.PickupValuefreight, 2);
                        dashboard.TotalPO = data.TotalPO;
                        dashboard.TotalPOValue = Math.Round(data.TotalPOValue, 2);
                        dashboard.TotalClosedPO = data.TotalClosedPO;
                        dashboard.TotalClosedPOValue = Math.Round(data.TotalClosedPOValue, 2);
                        dashboard.TotalOpenPO = data.TotalOpenPO;
                        dashboard.TotalOpenPOValue = Math.Round(data.TotalOpenPOValue, 2);
                        dashboard.Spilloverpercentage = Math.Round(data.Spilloverpercentage, 2);
                        dashboard.TotalDamageStock = Math.Round(data.TotalDamageStock, 2);
                        dashboard.DamageSales = Math.Round(data.DamageSales, 2);
                        dashboard.InternalValue = Math.Round(data.InternalValue, 2);
                        dashboard.TotalAutoPO = data.TotalAutoPO;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, dashboard);
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }
        #endregion
        #endregion
        #region Get All data blank PO
        /// <summary>
        /// Get Blank PO Data
        /// Created Date:04/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="list"></param>
        /// <param name="page"></param>
        /// <param name="Warehouseid"></param>
        /// <returns></returns>
        [Route("BlankPO")]
        public PaggingData GetBlankPO(int list, int page, int Warehouseid)
        {
            logger.Info("start ItemMaster: ");
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
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var context = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                        var itemPagedListWid = context.AllBlankPOWid(list, page, Warehouse_id, CompanyId);
                        return itemPagedListWid;
                    }
                    else
                    {
                        var itemPagedList = context.AllBlankPOWid(list, page, Warehouseid, CompanyId);

                        logger.Info("End ItemMaster: ");
                        return itemPagedList;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }
        #endregion
        #region get data for 48 hours pending
        /// <summary>
        /// Created Date:12/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("Pending48hr")]
        public PaggingData Getpodata(int WarehouseId)
        {
            logger.Info("start ItemMaster: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                PaggingData obj = new PaggingData();

                List<PurchaseOrderMaster> purchasedata = new List<PurchaseOrderMaster>();
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
                int CompanyId = compid;
                int count = 0;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var db = new AuthContext())
                {
                    List<PurchaseOrderMaster> podata = db.DPurchaseOrderMaster.AsEnumerable().Where(x => x.WarehouseId == WarehouseId && x.PRType == 0 && (x.Status == "Self Approved" || x.Status == "Send for Approval" || x.Status == "Approved")).OrderByDescending(x => x.PurchaseOrderId).ToList();
                    DateTime datenow = DateTime.Now;
                    foreach (var po in podata)
                    {
                        String date = Convert.ToString(datenow - po.CreationDate);
                        TimeSpan diff = TimeSpan.Parse(date);
                        double hour = diff.TotalHours;
                        if (hour > 48)
                        {
                            count++;
                            var PurchaseData = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == po.PurchaseOrderId).FirstOrDefault();
                            PurchaseData.hours = Convert.ToInt16(hour);
                            purchasedata.Add(PurchaseData);
                        }
                    }
                    obj.ordermaster = purchasedata;
                    obj.total_count = count;
                    return obj;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }
        #endregion

        #region Orders For Supplier
        /// <summary>
        /// created by 19/07/2019
        /// Supplier can change following status
        /// New Order
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("NewOrders")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> NewOrders(int SupplierId)
        {
            try
            {
                // POResDTO res;
                purchaseAppData res;
                //var po = await db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.SupplierId == obj.SupplierId).SingleOrDefaultAsync();
                var query = "select po.PurchaseOrderId as OrderId ,po.ETotalAmount as TotalAmount,count(pod.ItemId) as itemCount " +
                           " from PurchaseOrderMasters po inner join PurchaseOrderDetails pod on po.PurchaseOrderId = pod.PurchaseOrderId  where po.SupplierId = '" + SupplierId + "' " +
                           " and (po.Status = 'Approved' or po.Status = 'Self Approved') group by  po.PurchaseOrderId ,po.ETotalAmount,po.SupplierName ";
                using (var db = new AuthContext())
                {
                    var data = db.Database.SqlQuery<PurchaseStatusDTO>(query).ToList();



                    res = new purchaseAppData()
                    {
                        pom = data,
                        Status = true,


                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ex)
            {
                purchaseAppData res = new purchaseAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region ItemDetailWithPurchaseOrderId
        /// <summary>
        /// created by 19/07/2019
        /// Supplier can change following status
        /// New Order
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("ItemDetailWithPurchaseOrderId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> ItemDetailWithPurchaseOrderId(int PurchaseOrderId)
        {
            try
            {


                // POResDTO res;
                itemDetailsAppData res;
                //var po = await db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.PurchaseOrderId && a.SupplierId == obj.SupplierId).SingleOrDefaultAsync();
                var query = "select ItemName as ItemName,TotalQuantity as Quantity ,Price as Price from PurchaseOrderDetails where PurchaseOrderId=" + PurchaseOrderId + "";
                using (var db = new AuthContext())
                {
                    var data = db.Database.SqlQuery<PurchaseItemDetails>(query).ToList();

                    res = new itemDetailsAppData()
                    {
                        pom = data,
                        Status = true


                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
            catch (Exception ex)
            {
                itemDetailsAppData res = new itemDetailsAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

        #region to get current GR status for Supplier app
        /// <summary>
        /// app will send Supplier App and get Gr status 
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("GetGrstatusforSapp")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetGrstatusforSapp(int PurchaseOrderId)
        {
            logger.Info("start getting: GetGrstatusforSapp " + PurchaseOrderId);
            try
            {
                PurchaseOrderMaster pm = new PurchaseOrderMaster();
                var predicate = PredicateBuilder.True<PurchaseOrderMaster>();
                predicate = predicate.And(x => x.PurchaseOrderId == PurchaseOrderId && (x.Status == "Partial Received" || x.Status == "CN Partial Received" || x.Status == "Received" || x.Status == "UN Received" || x.Status == "CN Received" || x.Status == "UN Partial Received"));
                GetUCGRDTOforSappFinal MasterHead = new GetUCGRDTOforSappFinal();
                List<GetUCGRDTO> HeadData = new List<GetUCGRDTO>();
                using (AuthContext db = new AuthContext())
                {
                    pm = db.DPurchaseOrderMaster.Where(predicate).FirstOrDefault();
                    List<PurchaseOrderDetailRecived> _detail = db.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                    List<FreeItem> FItemList = db.FreeItemDb.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                    if (pm.Gr1_Amount != 0)
                    {
                        List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            GetListUCGRDTO UCL = new GetListUCGRDTO()
                            {
                                PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                ItemId = b.ItemId,
                                ItemName = b.ItemName1,
                                Price = Convert.ToDouble(b.Price1),
                                HSNCode = b.HSNCode,
                                TotalQuantity = b.TotalQuantity,
                                QtyRecived = Convert.ToInt32(b.QtyRecived1),
                                DamagQtyRecived = b.DamagQtyRecived1,
                                ExpQtyRecived = b.ExpQtyRecived1,
                                BatchNo = b.BatchNo1,
                                MFG = b.MFGDate1
                            };
                            HeadDetail.Add(UCL);
                        }

                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {
                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr1Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }




                        GetUCGRDTO UC = new GetUCGRDTO()
                        {
                            PurchaseOrderId = pm.PurchaseOrderId,
                            GrNumber = pm.Gr1Number,
                            GrDate = Convert.ToDateTime(pm.Gr1_Date),
                            GrAmount = pm.Gr1_Amount,
                            VehicleType = pm.VehicleType1,
                            VehicleNumber = pm.VehicleNumber1,
                            Status = pm.Gr1Status,
                            Detail = HeadDetail,
                            FreeDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (pm.Gr2_Amount != 0)
                    {
                        List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            GetListUCGRDTO UCL = new GetListUCGRDTO()
                            {
                                PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                ItemId = b.ItemId,
                                ItemName = b.ItemName2,
                                Price = Convert.ToDouble(b.Price2),
                                HSNCode = b.HSNCode,
                                TotalQuantity = b.TotalQuantity,
                                QtyRecived = Convert.ToInt32(b.QtyRecived2),
                                DamagQtyRecived = b.DamagQtyRecived2,
                                ExpQtyRecived = b.ExpQtyRecived2,
                                BatchNo = b.BatchNo2,
                                MFG = b.MFGDate2
                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {

                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr2Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        GetUCGRDTO UC = new GetUCGRDTO()
                        {
                            PurchaseOrderId = pm.PurchaseOrderId,
                            GrNumber = pm.Gr2Number,
                            GrDate = Convert.ToDateTime(pm.Gr2_Date),
                            GrAmount = pm.Gr2_Amount,
                            VehicleType = pm.VehicleType2,
                            VehicleNumber = pm.VehicleNumber2,
                            Status = pm.Gr2Status,
                            Detail = HeadDetail,
                            FreeDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (pm.Gr3_Amount != 0)
                    {
                        List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            GetListUCGRDTO UCL = new GetListUCGRDTO()
                            {
                                PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                ItemId = b.ItemId,
                                ItemName = b.ItemName3,
                                Price = Convert.ToDouble(b.Price3),
                                HSNCode = b.HSNCode,
                                TotalQuantity = b.TotalQuantity,
                                QtyRecived = Convert.ToInt32(b.QtyRecived3),
                                DamagQtyRecived = b.DamagQtyRecived3,
                                ExpQtyRecived = b.ExpQtyRecived3,
                                BatchNo = b.BatchNo3,
                                MFG = b.MFGDate3
                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {

                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr3Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        GetUCGRDTO UC = new GetUCGRDTO()
                        {
                            PurchaseOrderId = pm.PurchaseOrderId,
                            GrNumber = pm.Gr3Number,
                            GrDate = Convert.ToDateTime(pm.Gr3_Date),
                            GrAmount = pm.Gr3_Amount,
                            VehicleType = pm.VehicleType3,
                            VehicleNumber = pm.VehicleNumber3,
                            Status = pm.Gr3Status,
                            Detail = HeadDetail,
                            FreeDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (pm.Gr4_Amount != 0)
                    {
                        List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            GetListUCGRDTO UCL = new GetListUCGRDTO()
                            {
                                PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                ItemId = b.ItemId,
                                ItemName = b.ItemName4,
                                Price = Convert.ToDouble(b.Price4),
                                HSNCode = b.HSNCode,
                                TotalQuantity = b.TotalQuantity,
                                QtyRecived = Convert.ToInt32(b.QtyRecived4),
                                DamagQtyRecived = b.DamagQtyRecived4,
                                ExpQtyRecived = b.ExpQtyRecived4,
                                BatchNo = b.BatchNo4,
                                MFG = b.MFGDate4
                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {

                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr4Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        GetUCGRDTO UC = new GetUCGRDTO()
                        {
                            PurchaseOrderId = pm.PurchaseOrderId,
                            GrNumber = pm.Gr4Number,
                            GrDate = Convert.ToDateTime(pm.Gr4_Date),
                            GrAmount = pm.Gr4_Amount,
                            VehicleType = pm.VehicleType4,
                            VehicleNumber = pm.VehicleNumber4,
                            Status = pm.Gr4Status,
                            Detail = HeadDetail,
                            FreeDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (pm.Gr5_Amount != 0)
                    {
                        List<GetListUCGRDTO> HeadDetail = new List<GetListUCGRDTO>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            GetListUCGRDTO UCL = new GetListUCGRDTO()
                            {
                                PurchaseOrderDetailRecivedId = b.PurchaseOrderDetailRecivedId,
                                ItemId = b.ItemId,
                                ItemName = b.ItemName5,
                                Price = Convert.ToDouble(b.Price5),
                                HSNCode = b.HSNCode,
                                TotalQuantity = b.TotalQuantity,
                                QtyRecived = Convert.ToInt32(b.QtyRecived5),
                                DamagQtyRecived = b.DamagQtyRecived5,
                                ExpQtyRecived = b.ExpQtyRecived5,
                                BatchNo = b.BatchNo5,
                                MFG = b.MFGDate5
                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {

                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr5Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        GetUCGRDTO UC = new GetUCGRDTO()
                        {
                            PurchaseOrderId = pm.PurchaseOrderId,
                            GrNumber = pm.Gr5Number,
                            GrDate = Convert.ToDateTime(pm.Gr5_Date),
                            GrAmount = pm.Gr5_Amount,
                            VehicleType = pm.VehicleType5,
                            VehicleNumber = pm.VehicleNumber5,
                            Status = pm.Gr5Status,
                            Detail = HeadDetail,
                            FreeDetail = FreeDetail
                        };
                        HeadData.Add(UC);
                    }


                    MasterHead = new GetUCGRDTOforSappFinal()
                    {
                        GRdetail = HeadData,
                        Status = true,
                        Message = "Done"
                    };
                }
                logger.Info("End  : ");
                if (HeadData != null && HeadData.Count > 0)
                {
                    HeadData = HeadData.Where(x => x.GrNumber != null).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, MasterHead); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion


        #region to get current IR status for Supplier app
        /// <summary>
        /// app will send Supplier App and get Gr status 
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("GetIRstatusForSapp")]
        [HttpGet]
        [AllowAnonymous] // remove from final build
        [Obsolete]
        public HttpResponseMessage GetIRstatusForSapp(int PurchaseOrderId)
        {
            logger.Info("start : ");
            try
            {
                var predicate = PredicateBuilder.True<IRMaster>();
                predicate = predicate.And(x => x.PurchaseOrderId == PurchaseOrderId);
                // predicate = predicate.And(x => x.PaymentStatus == "Paid");
                GetUCIRDTOforSappFinal MasterHead = new GetUCIRDTOforSappFinal();
                List<GetIRDetailsForSapp> IRSapppData = new List<GetIRDetailsForSapp>();
                using (AuthContext db = new AuthContext())
                {
                    List<IRMaster> pm = db.IRMasterDB.Where(predicate).Include(x => x.InvoiceReceiptDetails).ToList();
                    var invoiceimages = db.InvoiceImageDb.Where(x => x.PurchaseOrderId == PurchaseOrderId).ToList();
                    if (pm != null && pm.Count > 0)
                    {
                        var warehouseId = pm.FirstOrDefault().WarehouseId;
                        var grIds = pm.SelectMany(x => x.InvoiceReceiptDetails.Select(y => y.GoodsReceivedDetailId)).ToList();
                        var goodRecivedDetails = db.GoodsReceivedDetail.Where(x => grIds.Contains(x.Id)).Include(x => x.PurchaseOrderDetail).ToList();
                        var itemMultiMRPIds = goodRecivedDetails.Select(k => k.ItemMultiMRPId).Distinct().ToList();
                        List<DataContracts.Transaction.PurchaseOrder.ItemWithMRPDc> items = new List<DataContracts.Transaction.PurchaseOrder.ItemWithMRPDc>();
                        if (db.Database.Connection.State != ConnectionState.Open)
                            db.Database.Connection.Open();

                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in itemMultiMRPIds)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("itemMultiMRPIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetItemWithMRPDetail]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(new SqlParameter("warehouseId", warehouseId));

                        // Run the sproc
                        using (var reader = cmd.ExecuteReader())
                        {
                            items = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<DataContracts.Transaction.PurchaseOrder.ItemWithMRPDc>(reader).ToList();
                        }
                        var fileUrl = string.Format("{0}://{1}:{2}/images/GrDraftInvoices/", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port);
                        foreach (var a in pm)
                        {

                            var invoiceImages = invoiceimages != null ? invoiceimages.Where(x => x.InvoiceNumber == a.IRID).Select(x =>
                                fileUrl +
                                x.IRLogoURL).ToList() : new List<string>();
                            List<GetListUCGRDTOV1> HeadDetailV1 = new List<GetListUCGRDTOV1>();
                            foreach (InvoiceReceiptDetail b in a.InvoiceReceiptDetails)
                            {
                                var gr = goodRecivedDetails.FirstOrDefault(x => x.Id == b.GoodsReceivedDetailId);
                                double discountamt = 0;
                                if (b.DiscountAmount.HasValue && b.DiscountAmount.Value > 0)
                                    discountamt = Convert.ToDouble(b.DiscountAmount);
                                else if (b.DiscountPercent.HasValue && b.DiscountPercent.Value > 0)
                                {
                                    discountamt = b.IRQuantity * b.Price * Convert.ToDouble(b.DiscountPercent) / 100;
                                }
                                GetListUCGRDTOV1 UCL = new GetListUCGRDTOV1()
                                {
                                    PurchaseOrderId = a.PurchaseOrderId,
                                    ItemName = gr.PurchaseOrderDetail.ItemName,
                                    Price = Convert.ToDouble(b.Price),
                                    HSNCode = items.Any(p => p.ItemMultiMRPId == gr.ItemMultiMRPId) ? items.FirstOrDefault(p => p.ItemMultiMRPId == gr.ItemMultiMRPId).HSNCode : "",
                                    TotalQuantity = b.IRQuantity,
                                    QtyRecived = Convert.ToInt32(gr.Qty),
                                    CreationDate = b.CreatedDate,
                                    IrStatus = a.IRStatus,
                                    IRType = a.IRType,
                                    SupplierName = a.SupplierName,
                                    Status = b.Status == 1 ? "Pending for Buyer Side" : (b.Status == 2 ? "Approved" : "Reject"),
                                    PriceRecived = b.Price,
                                    Discount = discountamt,
                                };
                                HeadDetailV1.Add(UCL);
                            }
                            GetIRDetailsForSapp UC = new GetIRDetailsForSapp()
                            {
                                PurchaseOrderId = a.PurchaseOrderId,
                                IRID = a.IRID,
                                IRStatus = a.IRStatus,
                                IRType = a.IRType,
                                IRAmountWithOutTax = a.IRAmountWithOutTax,
                                IRAmountWithTax = a.IRAmountWithTax,
                                CreationDate = a.CreationDate,
                                SupplierName = a.SupplierName,
                                TotalAmount = a.TotalAmount,
                                TotalAmountRemaining = a.TotalAmountRemaining,
                                Progres = a.Progres,
                                TotalTaxPercentage = a.TotalTaxPercentage,
                                Discount = a.Discount,
                                PaymentStatus = a.PaymentStatus == "" ? "UnPaid" : a.PaymentStatus,
                                Detail = HeadDetailV1,
                                InvoiceImages = invoiceImages
                            };
                            IRSapppData.Add(UC);

                        }
                    }
                    MasterHead = new GetUCIRDTOforSappFinal()
                    {
                        IRdetail = IRSapppData,
                        Status = true,
                        Message = "Done"
                    };
                }
                logger.Info("End  : ");
                return Request.CreateResponse(HttpStatusCode.OK, MasterHead); ;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                logger.Info("End  PurchaseOrderDetail: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }
        #endregion


        #region Supplier App API to  Search by Po Id
        /// <summary>
        /// created by tejas 07/2019
        /// </summary>   
        /// <param name="sid"></param>
        /// <returns></returns>
        [Route("SearchbyPoId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SearchbyPoId(int SupplierId, int PurchaseOrderId)
        {
            try
            {
                SupplierPurchaseOrderObj res;
                using (var db = new AuthContext())
                {
                    List<SupplierPurchaseOrderDC> _result = new List<SupplierPurchaseOrderDC>();
                    //string sqlquery = " select p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName, DATEDIFF(DAY, p.CreationDate, GETDATE()) AS POageinDays,p.PoType, p.CreationDate,p.progress,p.ETotalAmount,COUNT(d.PurchaseOrderId) as Itemcount" +
                    //                  " from PurchaseOrderMasters p  with(nolock) inner join PurchaseOrderDetails d with(nolock) on p.PurchaseOrderId = d.PurchaseOrderId and p.supplierId = " + SupplierId + " and p.PurchaseOrderId = " + PurchaseOrderId + "  and p.ETotalAmount > 0 group by p.PurchaseOrderId,p.SupplierName, p.SupplierStatus,p.Status,p.WarehouseName " +
                    //                  " ,p.PoType,  p.CreationDate,p.progress,p.ETotalAmount,d.PurchaseOrderId";
                    //_result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>(sqlquery).ToListAsync();
                    if (SupplierId > 0 && PurchaseOrderId > 0)
                    {
                        var sid = new SqlParameter("@SupplierId", SupplierId);
                        var pid = new SqlParameter("@PurchaseOrderId", PurchaseOrderId);
                        _result = await db.Database.SqlQuery<SupplierPurchaseOrderDC>("exec SearchbyPoIdSupplierApp @SupplierId,@PurchaseOrderId", sid, pid).ToListAsync();
                    }

                    res = new SupplierPurchaseOrderObj()
                    {
                        pom = _result,
                        Status = true,
                        Message = "Success."
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                suppAppData res = new suppAppData()
                {
                    pom = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadGateway, res);
            }
        }
        #endregion

    }

    #region DTO


    public class GetUCGRDTOforSappFinal
    {
        public List<GetUCGRDTO> GRdetail { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class widclass
    {
        public int id { get; set; }
    }

    public class objDTO
    {
        public DateTime From { get; set; }
        public DateTime TO { get; set; }
        public List<widclass> ids { get; set; }
    }

    public class GetUCIRDTOforSappFinal
    {
        public List<GetIRDetailsForSapp> IRdetail { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class IrAcRjDto
    {
        public int PurchaseOrderId { get; set; }
        public string IrStatus { get; set; }
        public string IrRejectComment { get; set; }
        public string IRID { get; set; }
        public int IRMasterId { get; set; }
    }

    public class ExcelDataForIR
    {
        public string SupplierGstNumber
        {
            get; set;
        }
        public string SupplierName
        {
            get; set;
        }
        public string InvoiceNumber
        {
            get; set;
        }
        public DateTime InvoiceDate
        {
            get; set;
        }
        public double InvoiceValue
        {
            get; set;
        }
        public double GstRate
        {
            get; set;
        }
        public double TaxableValue
        {
            get; set;
        }
        public double CGstAmount
        {
            get; set;
        }
        public double SGstAmount
        {
            get; set;
        }
        public double PaymentStatus
        {
            get; set;
        }
        public string ItemName
        {
            get; set;
        }

        public string Status
        {
            get; set;
        }
        public double TotalAmount
        {
            get; set;
        }

    }
    public class IRData
    {
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public double TotalAmount { get; set; }
        public int Id
        {
            get; set;
        }
        public int SupplierId
        {
            get; set;
        }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }

        public string PaymentStatus
        { get; set; }

        public DateTime CreationDate { get; set; }

        public int PaymentTerms { set; get; }

        public DateTime PaymentTermDate { set; get; }

        public bool GonePaymentTermDate
        {
            get; set;
        }

        public double TotalAmountRemaining
        {
            get; set;
        }
        public string GSTNumber { get; set; } 
    }
    public class PurchaseOrderDetailRecivedDTM
    {

        public int PurchaseOrderDetailRecivedId { get; set; }
        public int CompanyId { get; set; }
        public int PurchaseOrderMasterRecivedId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string SellingSku { get; set; }
        public string HSNCode { get; set; }
        public string SKUCode { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }

        [NotMapped]
        public double MRP { get; set; }
        public double PriceRecived { get; set; }
        public int? MOQ { get; set; }
        public int? TotalQuantity { get; set; }
        public double? TaxAmount { get; set; }
        public double? TotalTaxPercentage { get; set; }
        public int? PurchaseQty { get; set; }
        public double? TotalAmountIncTax { get; set; }
        public string Status { get; set; }
        public DateTime? CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public int? QtyRecived1 { get; set; }
        public int? QtyRecived2 { get; set; }
        public int? QtyRecived3 { get; set; }
        public int? QtyRecived4 { get; set; }
        public int? QtyRecived5 { get; set; }

        [NotMapped]
        public double? QtyRecivedTotal { get; set; }
        public double? Price1 { get; set; }
        public double? Price2 { get; set; }
        public double? Price3 { get; set; }
        public double? Price4 { get; set; }
        public double? Price5 { get; set; }
        public double? dis1 { get; set; }
        public double? dis2 { get; set; }
        public double? dis3 { get; set; }
        public double? dis4 { get; set; }
        public double? dis5 { get; set; }
        public double? QtyRecived
        {
            get
            {
                return (QtyRecived1 + QtyRecived2 + QtyRecived3 + QtyRecived4 + QtyRecived5);
            }
        }

        [NotMapped]
        public double? POItemFillRate { get; set; }

        public DateTime? Gr1Date { get; set; }
        public DateTime? Gr2Date { get; set; }
        public DateTime? Gr3Date { get; set; }
        public DateTime? Gr4Date { get; set; }
        public DateTime? Gr5Date { get; set; }

        public string TotalTAT { get; set; }
        public string AverageTAT { get; set; }
        public string ItemNumber { get; set; }

    }
    public class suppPoDetailData
    {
        public List<PurchaseOrderDetail> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class POResDTO
    {
        public PurchaseOrderMaster pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class AcceptRejectDTO
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int SupplierStatus { get; set; }
        public string Status { get; set; }

        public string SupplierRejectReason { get; set; }


    }
    public class PostTDO
    {
        public int SupplierId { get; set; }
        public string Status { get; set; }

    }
    public class suppPoDetailRecievedData
    {
        public List<PurchaseOrderDetailRecived> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class tatcount
    {
        public int Send_To_supplier { get; set; }
        public int Accepted_by_supplier { get; set; }
        public int Rejected_by_supplier { get; set; }
        public int Partial_Received { get; set; }
        public int Received { get; set; }
        public int UnPaid { get; set; }
        public int Paid { get; set; }
    }
    public class suppAppData
    {
        public List<PurchaseOrderMaster> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class suppAppData1
    {
        public List<itemDetailsAppDatav2> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class ObjSidWid
    {
        public int Wid { get; set; }
        public int Sid { get; set; }
    }
    #region DTO for  PO Dashboard 
    public class PODashboardDTO
    {
        public long? TotalManualPO { get; set; }
        public long? TotalAutoPO { get; set; }
        public string TotalTATPO { get; set; }
        public double? TotalFillRatePO { get; set; }
        public long? TotalSKVehicle { get; set; }
        public long? TotalSupplierVehicle { get; set; }

        public int? GRNBeyond48hr { get; set; }
        public int TotalPR { get; set; }
        public int TotalPRAmount { get; set; }
        public int TotalPRApprovalPending { get; set; }
        public int TotalPRApprovalPendingAmt { get; set; }
        public int TotalPRApproved { get; set; }
        public int TotalPRApprovedAmt { get; set; }
        public int TotalPRRejected { get; set; }
        public int TotalPRRejectedAmt { get; set; }
        public int TotalPRPending { get; set; }
        public int TotalPRPendingAmt { get; set; }
        //
        public double TotalGrnValue { get; set; }
        public double PickupValue { get; set; }
        public double PickupValuefor { get; set; }
        public double PickupValuefreight { get; set; }
        public int TotalPO { get; set; }
        public double TotalPOValue { get; set; }
        public int TotalClosedPO { get; set; }
        public double TotalClosedPOValue { get; set; }
        public int TotalOpenPO { get; set; }
        public double TotalOpenPOValue { get; set; }
        public double Spilloverpercentage { get; set; }
        public double TotalDamageStock { get; set; }
        public double DamageSales { get; set; }
        public double InternalValue { get; set; }
        //


    }
    public class PRDashboardDTO
    {
        public int TotalPR { get; set; }
        public int TotalPRAmount { get; set; }
        public int TotalPRApprovalPending { get; set; }
        public int TotalPRApprovalPendingAmt { get; set; }
        public int TotalPRApproved { get; set; }
        public int TotalPRApprovedAmt { get; set; }
        public int TotalPRRejected { get; set; }
        public int TotalPRRejectedAmt { get; set; }
        public int TotalPRPending { get; set; }
        public int TotalPRPendingAmt { get; set; }
    }

    #endregion
    public class PurchaseStatusDTO
    {
        public int itemCount { get; set; }
        public int OrderId { get; set; }
        public double TotalAmount { get; set; }


    }
    public class purchaseAppData
    {
        public List<PurchaseStatusDTO> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class PurchaseItemDetails
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }


    }
    public class itemDetailsAppData
    {
        public List<PurchaseItemDetails> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    public class itemDetailsAppDatav2
    {
        public int PurchaseOrderId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime CreationDate { get; set; }
        public string Status { get; set; }
        public string SupplierName { get; set; }
        public string IRStatus { get; set; }
        public string Message { get; set; }
        public string PaymentStatus { get; set; }
        public int Count { get; set; }
        public string Invoice { get; set; }
        public string progress { get; set; }
        public string IR_Progress { get; set; }
        public string IRType { get; set; }
    }
    public class GetIRDetailsForSapp
    {

        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public string SupplierName { get; set; }
        public double TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public double? Gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double IRAmountWithTax { get; set; }
        public double IRAmountWithOutTax { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string IRType { get; set; }
        public DateTime CreationDate { get; set; }
        public int? Progres { get; set; }
        public string Remark { get; set; }
        public double? Discount { get; set; }
        public string PaymentStatus { get; set; }
        public List<string> InvoiceImages { get; set; }
        public List<GetListUCGRDTOV1> Detail { get; set; }
    }

    public class GetListUCGRDTOV1
    {
        public int IRreceiveid { get; set; }
        public int PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string HSNCode { get; set; }
        public double PriceRecived { get; set; }
        public int? TotalQuantity { get; set; }
        public int? IRQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public int? QtyRecived { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? CessTaxPercentage { get; set; }
        public int? Qty { get; set; }
        public double? Price { get; set; }
        public string IrStatus { get; set; }
        public string IRType { get; set; }
        public double? Discount { get; set; }
        public double? Discount1 { get; set; }
        public double? Discount2 { get; set; }

    }


    public class SupplierPurchaseOrderDC
    {

        public int PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public string WarehouseName { get; set; }
        public int POageinDays { get; set; }
        public string PoType { get; set; }
        public DateTime CreationDate { get; set; }
        public string progress { get; set; }
        public double ETotalAmount { get; set; }
        public int Itemcount { get; set; }
        public int SupplierStatus { get; set; }


    }


    public class SupplierPurchaseOrderObj
    {
        public List<SupplierPurchaseOrderDC> pom { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    public class SupplierItemDC
    {
        public string WarehouseName { get; set; }
        public string Number { get; set; }
        public string itemname { get; set; }
        public string LogoUrl { get; set; }
        public bool active { get; set; }
        public double price { get; set; }

    }

    public class SupplierItemObj
    {
        public List<SupplierItemDC> item { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    public class GoodsReceivedDetailN
    {
        public long Id { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int GrSerialNumber { get; set; } //gr serial number
        public double Price { get; set; }
        public int Status { get; set; } // 1= Pending for Checker Side, 2=Approved , 3=Reject
        public int CurrentStockHistoryId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFGDate { get; set; }
        public string Barcode { get; set; }
        public int ApprovedBy { get; set; }//approved by or rejectby
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsFreeItem { get; set; }
        public string Comment { get; set; }

    }

    public class PurchaseOrderNewDTO
    {
        public string PoInvoiceNo { get; set; }
        public string TransactionNumber { get; set; }
        public int? CompanyId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double? discount { get; set; }
        public double TotalAmount { get; set; }
        public double Advance_Amt { get; set; }
        public string PoType { get; set; }
        public string Comment { get; set; }
        public int? ApprovalBy { get; set; }
        public string Level { get; set; }
        public string progress { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string SupplierRejectReason { get; set; }
        public double ETotalAmount { get; set; }
        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public bool IsCashPurchase { get; set; }
        public string CashPurchaseName { get; set; }
        public int SupplierStatus { get; set; } //  For Suplier  0.NA  1.PO Accepted , 2.PO Rejected, 3.PO Canceled 4. PO Ongoing Orders 5.Po Past Orders
        public List<PurchaseOrderDetailExport> PurchaseOrderDetail { get; set; }
        public int PurchaseOrderId { get; set; }
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public int? Approval1 { get; set; }

        public int? Approval2 { get; set; }

        public int? Approval3 { get; set; }

        public int? Approval4 { get; set; }

        public int? Approval5 { get; set; }

        public string ApprovalName1 { get; set; }

        public string ApprovalName2 { get; set; }

        public string ApprovalName3 { get; set; }

        public string ApprovalName4 { get; set; }

        public string ApprovalName5 { get; set; }
        public int PRStatus { get; set; }
        public string ApprovedBy { get; set; }
        public bool IsPR { get; set; }

    }


    public class PurchaseOrderDetailExport
    {
        public int PurchaseOrderDetailId { get; set; }
        public int? CompanyId { get; set; }
        public int PurchaseOrderId { get; set; }

        public long? PurchaseOrderNewId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string SupplierName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string HSNCode { get; set; }
        public string SKUCode { get; set; }
        public string ItemName { get; set; }
        public string itemBaseName { get; set; }
        public double Price { get; set; }
        public double MRP { get; set; }
        public int MOQ { get; set; }
        public int TotalQuantity { get; set; }

        public string PurchaseSku { get; set; }
        public double TaxAmount { get; set; }

        public double TotalAmountIncTax { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public int ConversionFactor { get; set; }
        public string PurchaseName { get; set; }
        public double PurchaseQty { get; set; }
        public double Total_No_Pieces { get; set; }
        public int QtyRecived { get; set; }

        //multimrp
        public int ItemMultiMRPId { get; set; }

        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public string Barcode { get; set; }
        public List<ItemMultiMRPDc> multiMrpIds { get; set; }
        public bool IsCommodity { get; set; }//just for comodity item not show mrp and multi mrp in drop down

        //public PurchaseOrderNewDc PurchaseOrder { get; set; }

        //public GoodsReceivedDetail GoodsReceivedDetail { get; set; }
        public bool IsFreeItem { get; set; }
        public int GrSerilanumber { get; set; }
        public DateTime GRDate { get; set; }
        public int GRQty { get; set; }
    }

    public class GRReportDC
    {
        public int PurchaseOrderId { get; set; }
        public int WarehouseId { get; set; }
        public string GRStatus { get; set; }
        public string GRNo { get; set; }
        public int TotalQuantity { get; set; }
        public int Qty { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public double PriceRecived { get; set; }
        public double Price { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public DateTime GrDate { get; set; }
        public DateTime CreationDate { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string TotalTAT { get; set; }
        public string AverageTAT { get; set; }
        [NotMapped]
        public double? POItemFillRate { get; set; }
        public string POStatus { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public string Category { get; set; }
        public string CompanyStockCode { get; set; }
        public string PickerType { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string storeName { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public double? FreightCharge { get; set; }
    }



    public class NewIRMasterDTO
    {

        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public int supplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public double? Gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? Discount { get; set; }
        public double IRAmountWithTax { get; set; }
        public double IRAmountWithOutTax { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string PaymentStatus { get; set; }
        public int PaymentTerms { get; set; }
        public string IRType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Deleted { get; set; }
        public int? Progres { get; set; }
        public string Remark { get; set; }
        public string RejectedComment { get; set; }
        public string ApprovedComment { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public int? DueDays { get; set; }
        public double? CashDiscount { get; set; }
        public double? FreightAmount { get; set; }
        public int IrSerialNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? IRApprovedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int IsApprove { get; set; }

    }

    public class POdashboardDC
    {
        //
        public double TotalGrnValue { get; set; }
        public double PickupValue { get; set; }
        public double PickupValuefor { get; set; }
        public double PickupValuefreight { get; set; }
        public int TotalPO { get; set; }
        public double TotalPOValue { get; set; }
        public int TotalClosedPO { get; set; }
        public double TotalClosedPOValue { get; set; }
        public int TotalOpenPO { get; set; }
        public double TotalOpenPOValue { get; set; }
        public double Spilloverpercentage { get; set; }
        public double TotalDamageStock { get; set; }
        public double DamageSales { get; set; }
        public double InternalValue { get; set; }
        public int TotalAutoPO { get; set; }
        //
    }
    #endregion

}

