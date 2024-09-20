using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryAssignment")]
    public class DeliveryAssignmentController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(string ids, int DeliveryIssuanceId)//get all 
        {
            using (AuthContext context = new AuthContext())
            {

                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;


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
                    int CompanyId = compid;


                    List<DBoySummary> SUmmarylist = new List<DBoySummary>();
                    string[] idss = ids.Split(',');


                    foreach (var od in idss)
                    {
                        var oid = Convert.ToInt32(od);

                        // var orderdipatchmaster = context.OrderDispatchedMasters.Where(x => x.OrderId == oid && x.CompanyId==CompanyId).SingleOrDefault();
                        //var orderdipatchmaster = context.OrderDeliveryMasterDB.Where(x => x.OrderId == oid && x.WarehouseId == WarehouseId &&x.CompanyId==compid&& x.DeliveryIssuanceId == DeliveryIssuanceId).Include("orderDetails").SingleOrDefault();
                        var orderdipatchmaster = context.OrderDeliveryMasterDB.Where(x => x.OrderId == oid && x.CompanyId == compid && x.DeliveryIssuanceId == DeliveryIssuanceId).SingleOrDefault();


                        DBoySummary Os = new DBoySummary();

                        if (orderdipatchmaster != null)
                        {
                            Os.chequeNo = orderdipatchmaster.CheckNo;
                            Os.CustomerId = orderdipatchmaster.CustomerId;
                            Os.CustomerName = orderdipatchmaster.CustomerName;
                            Os.DBoyName = orderdipatchmaster.DboyName;
                            Os.GrossAmount = orderdipatchmaster.GrossAmount;
                            Os.OrderId = orderdipatchmaster.OrderId;
                            //Os.SalesPerson = orderdipatchmaster.SalesPerson;
                            //Os.SalesPersonId = orderdipatchmaster.SalesPersonId;
                            Os.Status = orderdipatchmaster.Status;
                            Os.Skcode = orderdipatchmaster.Skcode;
                            Os.invoice_no = orderdipatchmaster.invoice_no;
                            Os.ShopName = orderdipatchmaster.ShopName;
                            Os.cashAmount = orderdipatchmaster.CashAmount;
                            Os.ElectronicAmount = orderdipatchmaster.ElectronicAmount;
                            Os.chequeAmount = orderdipatchmaster.CheckAmount;
                            Os.comments = orderdipatchmaster.comments;
                            Os.ReDispatchCount = orderdipatchmaster.ReDispatchCount;
                            SUmmarylist.Add(Os);
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, SUmmarylist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Getorders(string ids, int DeliveryIssuanceId, string test)//get all 
        {
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
                    int CompanyId = compid;

                    string[] idss = ids.Split(',');

                    List<OrderDispatchedMaster> Ordeersss = new List<OrderDispatchedMaster>();
                    foreach (var od in idss)
                    {
                        var oid = Convert.ToInt32(od);
                        try
                        {
                            var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => x.OrderId == oid && x.CompanyId == compid && x.DeliveryIssuanceId == DeliveryIssuanceId).Include("orderDetails").SingleOrDefault();

                            var orderdipatchmaster = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderDeliveryMaster.OrderId && x.CompanyId == CompanyId).Include("orderDetails").SingleOrDefault();
                            orderdipatchmaster.Status = OrderDeliveryMaster.Status;
                            if (orderdipatchmaster != null)
                            {
                                Ordeersss.Add(orderdipatchmaster);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, Ordeersss);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        ///// <summary>
        ///// Create shortitem list of Assignment
        ///// </summary>
        ///// <param name="AssignmentShortitemDTO"></param>
        ///// <returns></returns>
        //[Route("InsertShortItems")]
        //[HttpPost]
        //[Authorize]
        //public HttpResponseMessage Insert(AssignmentShortitemDTO AssignmentShortitemDTO)
        //{
        //    using (AuthContext context = new AuthContext())
        //    {

        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

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
        //        int CompanyId = compid;
        //        People People = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

        //        int Asid = AssignmentShortitemDTO.AssignmentShortItems[0].DeliveryIssuanceId;

        //        DeliveryIssuance DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == Asid && x.Status == "SavedAsDraft").SingleOrDefault();

        //        if (DeliveryIssuance != null)
        //        {
        //            foreach (var item in AssignmentShortitemDTO.AssignmentShortItems)
        //            {
        //                //if item already Exist & update
        //                ShortItemAssignment exits = context.ShortItemAssignmentDB.Where(x => x.DeliveryIssuanceId == item.DeliveryIssuanceId && x.ItemId == item.ItemId && x.itemNumber == item.itemNumber && x.OrderId == item.OrderId).FirstOrDefault();
        //                if (exits != null)
        //                {
        //                    exits.NotinStockQty = item.NotinStockQty;
        //                    exits.NotInStockComment = item.NotInStockComment;
        //                    exits.DamageStockQty = item.DamageStockQty;
        //                    exits.DamageComment = item.DamageComment;
        //                    exits.UpdatedDate = DateTime.Now;
        //                    exits.OrderId = item.OrderId;
        //                    exits.CreatedBy = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName;
        //                    context.ShortItemAssignmentDB.Attach(exits);
        //                    context.Entry(exits).State = EntityState.Modified;
        //                    context.Commit();
        //                }
        //                else
        //                {
        //                    ShortItemAssignment Asi = new ShortItemAssignment();
        //                    if ((item.DamageStockQty > 0 || item.NotinStockQty > 0))
        //                    {
        //                        Asi.DeliveryIssuanceId = item.DeliveryIssuanceId;
        //                        Asi.DboyId = item.DboyId;
        //                        Asi.ItemId = item.ItemId;
        //                        Asi.itemname = item.itemname;
        //                        Asi.itemNumber = item.itemNumber;
        //                        Asi.Orderqty = item.Orderqty;
        //                        Asi.NotinStockQty = item.NotinStockQty;
        //                        Asi.NotInStockComment = item.NotInStockComment;
        //                        Asi.DamageStockQty = item.DamageStockQty;
        //                        Asi.DamageComment = item.DamageComment;
        //                        Asi.Deleted = false;
        //                        Asi.OrderId = item.OrderId;
        //                        Asi.CreatedBy = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName;
        //                        Asi.CreatedDate = DateTime.Now;
        //                        Asi.UpdatedDate = DateTime.Now;
        //                        context.ShortItemAssignmentDB.Add(Asi);
        //                        context.Commit();
        //                    }
        //                }
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, true);
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, false);

        //    }
        //}

        /// <summary>
        /// get Short Item List
        /// </summary>
        /// <param name="DeliveryIssuanceId"></param>
        /// <returns></returns>
        [Route("GetShortItems")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage Getorders(int DeliveryIssuanceId)//get all 
        {
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
                    int CompanyId = compid;
                    List<ShortItemAssignment> AssignmentShortItem = context.ShortItemAssignmentDB.Where(aj => aj.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, AssignmentShortItem);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        /// <summary>
        /// Assignment Finalization
        /// </summary>
        /// <param name="Finalization"></param>
        /// <returns></returns>
        [Route("Finalization")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage Finalization(Int32 DeliveryIssuanceId)
        {
            using (AuthContext context = new AuthContext())
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
                int CompanyId = compid;
                People People = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                //List<Int32> Ids = new List<Int32>();
                try
                {
                    DeliveryIssuance DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId && x.Status == "SavedAsDraft").FirstOrDefault();
                    if (DeliveryIssuance != null  && DeliveryIssuance.TripPlannerConfirmedMasterId==0)
                    {
                        bool ischangedAssignment = false;
                        List<Int32> Ids = new List<Int32>();
                        List<ShortItemAssignment> ShortItemAssignment = context.ShortItemAssignmentDB.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                        // for OrderDispatchedMasters & orderDetails
                        if (ShortItemAssignment.Count > 0)
                        {
                            ischangedAssignment = true;
                            foreach (var Shortorder in ShortItemAssignment)
                            {
                                OrderDispatchedMaster OrderDispatchedMasterd = context.OrderDispatchedMasters.Where(X => X.OrderId == Shortorder.OrderId).Include("orderDetails").FirstOrDefault();
                                foreach (OrderDispatchedDetails pc in OrderDispatchedMasterd.orderDetails.Where(x => x.ItemId == Shortorder.ItemId))
                                {

                                    if (pc.qty >= (Shortorder.NotinStockQty + Shortorder.DamageStockQty))
                                    {
                                        if (Shortorder.DamageStockQty > 0)
                                        {
                                            DamageStock DamageStock = context.DamageStockDB.Where(x => x.ItemNumber == Shortorder.itemNumber && x.WarehouseId == pc.WarehouseId && x.ItemMultiMRPId == pc.ItemMultiMRPId).FirstOrDefault();

                                            if (DamageStock != null)
                                            {
                                                DamageStock.DamageInventory = DamageStock.DamageInventory + Shortorder.DamageStockQty;
                                                DamageStock.UnitPrice = Math.Round(pc.UnitPrice, 2);
                                                DamageStock.ReasonToTransfer = Shortorder.DamageComment;
                                                DamageStock.UpdatedDate = DateTime.Now;
                                                context.Entry(DamageStock).State = EntityState.Modified;
                                                context.Commit();
                                                DamageStockController dsc = new DamageStockController();
                                                dsc.InsertDatainDamageStockHistory(DamageStock, Shortorder.DamageStockQty, 0, userid, context);
                                            }
                                            else
                                            {
                                                DamageStock objst = new DamageStock();
                                                objst.WarehouseId = pc.WarehouseId;
                                                objst.WarehouseName = pc.WarehouseName;
                                                objst.ItemId = pc.ItemId;
                                                objst.MRP = pc.price;
                                                objst.ItemMultiMRPId = pc.ItemMultiMRPId;
                                                objst.ItemNumber = pc.itemNumber;
                                                objst.ItemName = pc.itemname;
                                                objst.DamageInventory = Shortorder.DamageStockQty;
                                                objst.UnitPrice = Math.Round(pc.UnitPrice, 2);
                                                objst.ReasonToTransfer = Shortorder.DamageComment;
                                                objst.CreatedDate = DateTime.Now;
                                                objst.CompanyId = pc.CompanyId;
                                                context.DamageStockDB.Add(objst);
                                                context.Commit();
                                                DamageStockController dsc = new DamageStockController();
                                                dsc.InsertDatainDamageStockHistory(objst, Shortorder.DamageStockQty, 0, userid, context);
                                            }
                                        }

                                        pc.qty = pc.qty - (Shortorder.NotinStockQty + Shortorder.DamageStockQty);
                                        pc.Noqty = pc.qty;
                                        context.AddOrderDispatchedDetails(pc);

                                        #region Order History

                                        try
                                        {
                                            OrderMasterHistories h1 = new OrderMasterHistories();
                                            OrderItemHistory h2 = new OrderItemHistory();
                                            if (OrderDispatchedMasterd != null)
                                            {
                                                h2.orderid = Shortorder.OrderId;
                                                h2.ItemId = Shortorder.ItemId;
                                                h2.ItemName = Shortorder.itemname;
                                                h2.Reasoncancel = "Assignment Qty Change for item due to DamageComment :" + Shortorder.DamageComment + " And NotInStockComment : " + Shortorder.NotInStockComment;
                                                h2.qty = pc.qty;// - Shortorder.ItemShortQty;
                                                h2.oldqty = Shortorder.Orderqty;
                                                context.OrderItemHistoryDB.Add(h2);
                                                context.Commit();

                                                h1.orderid = Shortorder.OrderId;
                                                h1.Status = OrderDispatchedMasterd.Status;
                                                h1.Reasoncancel = Shortorder.itemname + "Assignment Qty Change for item due to DamageComment :" + Shortorder.DamageComment + " And NotInStockComment : " + Shortorder.NotInStockComment;
                                                //h1.Warehousename = orderdata2[0].WarehouseName;
                                                if (People.DisplayName != null)
                                                {
                                                    h1.username = People.DisplayName;
                                                }
                                                else
                                                {
                                                    h1.username = People.PeopleFirstName;
                                                }
                                                h1.userid = userid;
                                                h1.CreatedDate = DateTime.Now;
                                                context.OrderMasterHistoriesDB.Add(h1);
                                                context.Commit();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                        }
                                        #endregion
                                    }

                                    if (Ids.Exists(p => p.Equals(pc.OrderId)))
                                    {
                                    }
                                    else
                                    {
                                        Ids.Add(pc.OrderId);
                                    }
                                }
                            }

                            var OrderDispatchedMasterList = context.OrderDispatchedMasters.Where(X => Ids.Contains(X.OrderId)).Include("orderDetails").ToList();
                            var itemIds = OrderDispatchedMasterList.SelectMany(x => x.orderDetails.Select(z => z.ItemId)).ToList();
                            var itemslist = context.itemMasters.Where(x => itemIds.Contains(x.ItemId) && x.CompanyId == compid).Select(x => x).ToList();
                           
                            if (context.Database.Connection.State != ConnectionState.Open)
                                context.Database.Connection.Open();
                            var orderIdDt = new DataTable();
                            orderIdDt.Columns.Add("IntValue");
                            foreach (var item in itemIds)
                            {
                                var dr = orderIdDt.NewRow();
                                dr["IntValue"] = item;
                                orderIdDt.Rows.Add(dr);
                            }
                            var param = new SqlParameter("ItemIds", orderIdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.IntValues";
                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetItemWiseIncentiveClassification]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            var incentiveClassificationList = ((IObjectContextAdapter)context)
                            .ObjectContext
                            .Translate<IncentiveClassificationDc>(reader).ToList();

                            foreach (var orderId in Ids)
                            {
                                /// for OrderDispatchedMasters & orderDetails
                                OrderDispatchedMaster OrderDispatchedMaster = OrderDispatchedMasterList.FirstOrDefault(X => X.OrderId == orderId);
                                bool isDiscount = false;
                                double finaltotal = 0;
                                double finalTaxAmount = 0;
                                double finalSGSTTaxAmount = 0;
                                double finalCGSTTaxAmount = 0;
                                double finalGrossAmount = 0;
                                double finalTotalTaxAmount = 0;
                                //cess 
                                double finalCessTaxAmount = 0;

                                foreach (OrderDispatchedDetails pc in OrderDispatchedMaster.orderDetails)
                                {
                                    try
                                    {
                                        ItemMaster items = itemslist.FirstOrDefault(x => x.ItemId == pc.ItemId && x.CompanyId == compid);

                                        if (pc != null && pc.isDeleted != true)
                                        {
                                            // calculation
                                            // OrderDispatchedDetails od = pc;
                                            // od.OrderDispatchedDetailsId = pc.OrderDispatchedDetailsId;
                                            pc.IncentiveClassification = incentiveClassificationList.Any() && incentiveClassificationList.Any(x => x.ItemId == pc.ItemId) ? incentiveClassificationList.FirstOrDefault(x => x.ItemId == pc.ItemId).IncentiveClassification : "General";
                                            if (pc.DiscountPercentage == 0)
                                            {
                                                isDiscount = false;
                                            }
                                            else
                                            {
                                                isDiscount = true;
                                            }
                                            pc.DiscountPercentage = pc.DiscountPercentage;
                                            pc.OrderDetailsId = pc.OrderDetailsId;
                                            pc.OrderId = pc.OrderId;
                                            pc.OrderDispatchedMasterId = pc.OrderDispatchedMasterId;
                                            pc.CustomerId = pc.CustomerId;
                                            pc.CustomerName = pc.CustomerName;
                                            pc.City = pc.City;
                                            pc.Mobile = pc.Mobile;
                                            pc.OrderDate = pc.OrderDate;
                                            pc.CompanyId = pc.CompanyId;
                                            pc.CityId = pc.CityId;
                                            pc.WarehouseId = pc.WarehouseId;
                                            pc.WarehouseName = pc.WarehouseName;
                                            pc.CategoryName = pc.CategoryName;
                                            ///multimrp
                                            pc.ItemMultiMRPId = pc.ItemMultiMRPId;
                                            pc.ItemId = pc.ItemId;
                                            pc.Itempic = pc.Itempic;
                                            pc.itemname = pc.itemname;
                                            pc.SellingUnitName = pc.SellingUnitName;
                                            pc.SubcategoryName = pc.SubcategoryName;
                                            pc.SubsubcategoryName = pc.SubsubcategoryName;
                                            pc.itemcode = pc.itemcode;
                                            pc.Barcode = pc.Barcode;
                                            pc.UnitPrice = pc.UnitPrice;
                                            pc.Purchaseprice = pc.Purchaseprice;
                                            pc.MinOrderQty = pc.MinOrderQty;
                                            pc.MinOrderQtyPrice = pc.MinOrderQtyPrice;
                                            pc.qty = pc.qty;
                                            pc.price = pc.price;
                                            int MOQ = pc.MinOrderQty;
                                            pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;
                                            pc.qty = Convert.ToInt32(pc.qty);
                                            pc.SizePerUnit = pc.SizePerUnit;


                                            pc.TaxPercentage = items.TotalTaxPercentage;
                                            pc.TotalCessPercentage = items.TotalCessPercentage;
                                            if (pc.TaxPercentage >= 0)
                                            {
                                                pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
                                                pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
                                            }

                                            pc.HSNCode = items.HSNCode;

                                            pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
                                            pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);


                                            //if there is cess for that item
                                            try
                                            {
                                                if (pc.TotalCessPercentage > 0)
                                                {

                                                    pc.TotalCessPercentage = items.TotalCessPercentage;
                                                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                                                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
                                                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                                    pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
                                                }

                                            }
                                            catch (Exception sd)
                                            {
                                            }
                                            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                            pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                            pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
                                            pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
                                            if (pc.TaxAmmount >= 0)
                                            {
                                                pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
                                                pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
                                            }

                                            //for cess
                                            if (pc.CessTaxAmount > 0)
                                            {
                                                //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                                pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                                pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                                pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

                                            }
                                            else
                                            {
                                                pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
                                            }


                                            //od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                                            finalGrossAmount = finalGrossAmount + pc.TotalAmountAfterTaxDisc;
                                            finalTotalTaxAmount = finalTotalTaxAmount + pc.TotalAmountAfterTaxDisc;
                                            pc.DiscountAmmount = 0;
                                            pc.NetAmtAfterDis = 0;
                                            pc.Purchaseprice = items.price;
                                            pc.Deleted = false;
                                            //pc.Status = pc.Status;
                                            pc.QtyChangeReason = pc.QtyChangeReason;
                                            pc.itemNumber = pc.itemNumber;
                                            pc.UpdatedDate = DateTime.Now;
                                            context.Entry(pc).State = EntityState.Modified;
                                            context.Commit();

                                            finaltotal = finaltotal + pc.TotalAmt;

                                            if (pc.CessTaxAmount > 0)
                                            {
                                                finalCessTaxAmount = finalCessTaxAmount + pc.CessTaxAmount;
                                                finalTaxAmount = finalTaxAmount + pc.TaxAmmount + pc.CessTaxAmount;
                                            }
                                            else
                                            {
                                                finalTaxAmount = finalTaxAmount + pc.TaxAmmount;
                                            }
                                            finalSGSTTaxAmount = finalSGSTTaxAmount + pc.SGSTTaxAmmount;
                                            finalCGSTTaxAmount = finalCGSTTaxAmount + pc.CGSTTaxAmmount;
                                        }
                                        else
                                        {
                                            logger.Warn("Error:106 OrderDispatchedDetails Object PC are Null.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error("Error:101" + ex.Message);
                                    }
                                }
                                finaltotal = finaltotal + OrderDispatchedMaster.deliveryCharge;
                                finalGrossAmount = finalGrossAmount + OrderDispatchedMaster.deliveryCharge;
                                if (OrderDispatchedMaster.WalletAmount > 0)
                                {
                                    OrderDispatchedMaster.TotalAmount = System.Math.Round(finaltotal, 2) - OrderDispatchedMaster.WalletAmount.GetValueOrDefault();
                                    OrderDispatchedMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                                    OrderDispatchedMaster.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                                    OrderDispatchedMaster.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                                    OrderDispatchedMaster.GrossAmount = finalGrossAmount > 0 ? System.Math.Round((Convert.ToInt32(finalGrossAmount) - OrderDispatchedMaster.WalletAmount.GetValueOrDefault()), 0) : 0;
                                }
                                else
                                {
                                    OrderDispatchedMaster.TotalAmount = System.Math.Round(finaltotal, 2);
                                    OrderDispatchedMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                                    OrderDispatchedMaster.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                                    OrderDispatchedMaster.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                                    OrderDispatchedMaster.GrossAmount = System.Math.Round((finalGrossAmount), 0);
                                }
                                if (isDiscount == true)
                                {
                                    OrderDispatchedMaster.DiscountAmount = finalTotalTaxAmount - finaltotal;
                                }
                                else
                                {
                                    OrderDispatchedMaster.DiscountAmount = 0;
                                }

                                #region Billdiscountamount

                                double offerDiscountAmount = 0;

                                var billdiscount = context.BillDiscountDb.Where(x => x.OrderId == OrderDispatchedMaster.OrderId && x.CustomerId== OrderDispatchedMaster.CustomerId).ToList();
                                var offerIds = billdiscount.Select(x => x.OfferId).ToList();
                                var offers = context.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToList();
                                List<int> flashdealItems = context.FlashDealItemConsumedDB.Where(x => x.OrderId == OrderDispatchedMaster.OrderId).Select(x => x.ItemId).ToList();
                                var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();
                                List<Offer> Offers = context.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).ToList();

                                foreach (var BillDiscount in billdiscount)
                                {

                                    var Offer = offers.FirstOrDefault(z => z.OfferId == BillDiscount.OfferId);

                                    double totalamount = 0;
                                    int OrderLineItems = 0;
                                    if (Offer.OfferOn != "ScratchBillDiscount")
                                    {
                                        List<int> Itemids = new List<int>();
                                        if (Offer.BillDiscountType == "category")
                                        {
                                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                            var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                            var CItemIds = OrderDispatchedMaster.orderDetails.Select(x => x.ItemId).ToList();
                                            if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                            {
                                                var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                                CItemIds = OrderDispatchedMaster.orderDetails.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                            }
                                            Itemids = itemslist.Where(x => ids.Contains(x.Categoryid) 
                                            && !iteminofferlist.Contains(x.ItemId) 
                                            && !flashdealItems.Contains(x.ItemId)                                            
                                            ).Select(x => x.ItemId).ToList();
                                            if (CItemIds.Any())
                                            {
                                                Itemids = itemslist.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                            }
                                            totalamount = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : OrderDispatchedMaster.orderDetails.Count();
                                        }
                                        else if (Offer.BillDiscountType == "subcategory")
                                        {
                                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);
                                            var CItemIds = OrderDispatchedMaster.orderDetails.Select(x => x.ItemId).ToList();
                                            if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                            {
                                                var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                                CItemIds = OrderDispatchedMaster.orderDetails.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                            }
                                            Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) 
                                            && !iteminofferlist.Contains(x.ItemId) 
                                            && !flashdealItems.Contains(x.ItemId)                                            
                                            ).Select(x => x.ItemId).ToList();
                                            if (CItemIds.Any())
                                            {
                                                Itemids = itemslist.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                            }
                                            totalamount = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : OrderDispatchedMaster.orderDetails.Count();
                                        }
                                        else if (Offer.BillDiscountType == "brand")
                                        {
                                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);
                                            var CItemIds = OrderDispatchedMaster.orderDetails.Select(x => x.ItemId).ToList();
                                            if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                            {
                                                var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                                CItemIds = OrderDispatchedMaster.orderDetails.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                            }
                                            Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                            && !iteminofferlist.Contains(x.ItemId) 
                                            && !flashdealItems.Contains(x.ItemId)                                            
                                            ).Select(x => x.ItemId).ToList();
                                            if (CItemIds.Any())
                                            {
                                                Itemids = itemslist.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                            }
                                            totalamount = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : OrderDispatchedMaster.orderDetails.Count();
                                        }
                                        else if (Offer.BillDiscountType == "items")
                                        {
                                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                            var CItemIds = OrderDispatchedMaster.orderDetails.Select(x => x.ItemId).ToList();
                                            if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                            {
                                                var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                                CItemIds = OrderDispatchedMaster.orderDetails.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                            }
                                            if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
                                            {
                                                Itemids = itemslist.Where(x => iteminofferlist.Contains(x.ItemId)                                                
                                                ).Select(x => x.ItemId).ToList();
                                            }
                                            if (CItemIds.Any())
                                            {
                                                Itemids = itemslist.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                            }
                                            totalamount = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : OrderDispatchedMaster.orderDetails.Count();
                                        }
                                        else
                                        {
                                            var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                            var CItemIds = OrderDispatchedMaster.orderDetails.Select(x => x.ItemId).ToList();
                                            if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                            {
                                                var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                                CItemIds = OrderDispatchedMaster.orderDetails.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                            }
                                            Itemids = itemslist.Where(x => ids.Contains(x.Categoryid)                                            
                                            ).Select(x => x.ItemId).ToList();
                                            if (CItemIds.Any())
                                            {
                                                Itemids = itemslist.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                            }
                                            totalamount = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : OrderDispatchedMaster.orderDetails.Where(x => !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? OrderDispatchedMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : OrderDispatchedMaster.orderDetails.Count();
                                        }


                                        if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                        {
                                            totalamount = Offer.MaxBillAmount;
                                        }
                                    }
                                    else
                                    {
                                        totalamount = OrderDispatchedMaster.orderDetails.Sum(x => x.qty * x.UnitPrice);
                                        if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                        {
                                            totalamount = Offer.MaxBillAmount;
                                        }
                                    }

                                    if (Offer.BillDiscountOfferOn == "Percentage")
                                    {
                                        BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
                                        BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                    }
                                    else
                                    {
                                        int WalletPoint = 0;
                                        if (Offer.WalletType == "WalletPercentage")
                                        {
                                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * ((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0) / 100)));
                                            WalletPoint = WalletPoint * 10;
                                        }
                                        else
                                        {
                                            WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0);
                                        }
                                        if (Offer.ApplyOn == "PostOffer")
                                        {
                                            BillDiscount.BillDiscountTypeValue = WalletPoint;
                                            BillDiscount.BillDiscountAmount = 0;
                                            BillDiscount.IsUsedNextOrder = true;
                                        }
                                        else
                                        {
                                            BillDiscount.BillDiscountTypeValue = Offer.BillDiscountWallet;
                                            BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10);
                                            BillDiscount.IsUsedNextOrder = false;
                                        }
                                    }
                                    if (Offer.MaxDiscount > 0)
                                    {

                                        var walletmultipler = 1;

                                        if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            walletmultipler = 10;
                                        }
                                        if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
                                            {
                                                BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
                                            }
                                            if (Offer.MaxDiscount < BillDiscount.BillDiscountTypeValue)
                                            {
                                                BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                            }
                                        }
                                    }
                                    BillDiscount.IsAddNextOrderWallet = false;
                                    BillDiscount.ModifiedDate = indianTime;
                                    BillDiscount.ModifiedBy = userid;
                                    context.Entry(BillDiscount).State = EntityState.Modified;

                                    offerDiscountAmount += BillDiscount.BillDiscountAmount.Value;
                                }


                                OrderDispatchedMaster.TotalAmount = OrderDispatchedMaster.TotalAmount - offerDiscountAmount;
                                OrderDispatchedMaster.BillDiscountAmount = offerDiscountAmount;
                                OrderDispatchedMaster.GrossAmount = System.Math.Round(OrderDispatchedMaster.TotalAmount, 0);

                                #endregion

                                var cashOldEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == OrderDispatchedMaster.OrderId && z.PaymentFrom == "Cash"
                                                       && z.status == "Success").ToList();

                                if (cashOldEntries != null && cashOldEntries.Any())
                                {
                                    foreach (var cash in cashOldEntries)
                                    {
                                        cash.status = "Failed";
                                        cash.statusDesc = "Due to Items Short on Assignment Finalization";
                                        context.Entry(cash).State = EntityState.Modified;
                                    }
                                }

                                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                {
                                    amount = OrderDispatchedMaster.GrossAmount,
                                    CreatedDate = DateTime.Now,
                                    currencyCode = "INR",
                                    OrderId = OrderDispatchedMaster.OrderId,
                                    PaymentFrom = "Cash",
                                    status = "Success",
                                    UpdatedDate = DateTime.Now,
                                    statusDesc = "Due to Items Short on Assignment Finalization"
                                };
                                context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);


                                context.Entry(OrderDispatchedMaster).State = EntityState.Modified;
                                context.Commit();
                            }

                        }

                        //update OrderDeliveryMaster status and if any order value change then reupdate the value or order OrderDeliveryMaster
                        List<OrderDeliveryMaster> OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                        if (OrderDeliveryMaster.Count > 0)
                        {
                            foreach (var OdmItm in OrderDeliveryMaster)
                            {
                                try
                                {
                                    if (Ids.Exists(p => p.Equals(OdmItm.OrderId)))
                                    {
                                        OrderDispatchedMaster OrderDispatchedMasterd = context.OrderDispatchedMasters.Where(X => X.OrderId == OdmItm.OrderId).FirstOrDefault();
                                        OdmItm.TotalAmount = OrderDispatchedMasterd.TotalAmount;
                                        OdmItm.GrossAmount = System.Math.Round(OrderDispatchedMasterd.GrossAmount, 0);
                                    }
                                    OdmItm.Status = "Assigned";
                                    OdmItm.UpdatedDate = DateTime.Now;
                                    context.OrderDeliveryMasterDB.Attach(OdmItm);
                                    context.Entry(OdmItm).State = EntityState.Modified;
                                    context.Commit();
                                }
                                catch (Exception es)
                                {

                                }
                            }
                        }

                        // update in  DeliveryIssuance
                        try
                        {
                            DeliveryIssuance.CreatedBy = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName;
                        }
                        catch (Exception ss) { }
                        if (ischangedAssignment)
                        {
                            DeliveryIssuance.TotalAssignmentAmount = OrderDeliveryMaster.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).Sum(x => x.GrossAmount);//recalculate total amount
                        }

                        DeliveryIssuance.Status = "Assigned";
                        DeliveryIssuance.UpdatedDate = DateTime.Now;
                        context.DeliveryIssuanceDb.Attach(DeliveryIssuance);
                        context.Entry(DeliveryIssuance).State = EntityState.Modified;
                        context.Commit();
                        try
                        {
                            context.AddOrderDelivery(DeliveryIssuance);
                        }
                        catch (Exception ex) { }


                        ///agent 
                        /// if chnaged in  OnChange

                        //    Ids
                        foreach (var order in Ids)
                        {
                            CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                            helper.OnChange(order, userid);
                        }
                    }
                    else
                    {
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }


            }
        }



        /// <summary>
        ///call to update Delivery Assignment payment
        /// </summary>
        /// <param name="DeliveryIssuanceId"></param>
        /// <returns></returns>
        [Route("AssignmentPayment")]
        [HttpPut]
        [Authorize]
        public HttpResponseMessage AssignmentPayment(Int32 DeliveryIssuanceId)
        {
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
                    int CompanyId = compid;
                    People People = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                    DeliveryIssuance DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).FirstOrDefault();
                    int checkDCRStatus = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && x.Status== "Delivery Canceled Request").Count(); //by sudhir
                    if (checkDCRStatus == 0)
                    {
                        if (!context.AssignmentRechangeOrder.Any(x => x.AssignmentId == DeliveryIssuanceId && x.Status == 1 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                        {
                            if (DeliveryIssuance != null && DeliveryIssuance.Status == "Submitted")
                            {
                                DeliveryIssuance.CreatedBy = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName;
                                if (DeliveryIssuanceId > 0)
                                {
                                    DeliveryIssuance.Status = "Payment Accepted";
                                }
                                DeliveryIssuance.userid = userid;//added later
                                DeliveryIssuance.UpdatedDate = DateTime.Now;
                                context.DeliveryIssuanceDb.Attach(DeliveryIssuance);
                                context.Entry(DeliveryIssuance).State = EntityState.Modified;

                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = AssginDeli.DeliveryIssuanceId;
                                //AssginDeli.OrderId = AssginDeli.o
                                AssginDeli.Cityid = AssginDeli.Cityid;
                                AssginDeli.city = AssginDeli.city;
                                AssginDeli.DisplayName = AssginDeli.DisplayName;
                                AssginDeli.Status = AssginDeli.Status;
                                AssginDeli.WarehouseId = AssginDeli.WarehouseId;
                                AssginDeli.PeopleID = AssginDeli.PeopleID;
                                AssginDeli.VehicleId = AssginDeli.VehicleId;
                                AssginDeli.VehicleNumber = AssginDeli.VehicleNumber;
                                AssginDeli.RejectReason = AssginDeli.RejectReason;
                                AssginDeli.OrderdispatchIds = AssginDeli.OrderdispatchIds;
                                AssginDeli.OrderIds = AssginDeli.OrderIds;
                                AssginDeli.Acceptance = AssginDeli.Acceptance;
                                AssginDeli.IsActive = AssginDeli.IsActive;
                                AssginDeli.IdealTime = AssginDeli.IdealTime;
                                AssginDeli.TravelDistance = AssginDeli.TravelDistance;
                                AssginDeli.CreatedDate = indianTime;
                                AssginDeli.UpdatedDate = indianTime;
                                AssginDeli.userid = AssginDeli.userid;
                                if (People != null)
                                {
                                    if (People.DisplayName == null)
                                    {
                                        AssginDeli.UpdatedBy = People.PeopleFirstName;
                                    }
                                    else
                                    {
                                        AssginDeli.UpdatedBy = People.DisplayName;
                                    }

                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);



                                context.Commit();
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                Status = true,
                                Message = "Update done now Assignment Wating for Payment freezed"
                            });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                Status = true,
                                Message = "Please approve all rejected Order."
                            });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Status = true,
                            Message = "Some Orders are still Waiting for DCR Approval !!"
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        /// <summary>
        /// get Short Item List
        /// </summary>
        /// <param name="OrderId"></param>
        ///  /// <param name="ItemId"></param>
        /// <returns></returns>
        [Route("GetOrderItems")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetOrderItems(int OrderId, int ItemId, string itemNumber)//get all 
        {
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
                    int CompanyId = compid;

                    string query = " SELECT CAST(CASE WHEN OrderType in ('2','3') THEN 1 ELSE 0 END AS BIT) FROM OrderMasters where  OrderId =" + OrderId;
                    bool IsPending = context.Database.SqlQuery<bool>(query).First();
                    OrderDispatchedDetails OrderDispatchedDetails = new OrderDispatchedDetails();
                    if (!IsPending)
                    {
                        OrderDispatchedMaster OrderDispatchedMaster = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).Include("orderDetails").FirstOrDefault();
                        var payment = context.PaymentResponseRetailerAppDb.Any(x => x.OrderId == OrderId && x.status == "Success" && x.IsOnline /*x.PaymentFrom != "Cash"*/);

                        if (payment || OrderDispatchedMaster.ReDispatchCount > 1 || (OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0).Count()) == 1)//stop short on onilne , redispatched and single item order  
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, OrderDispatchedDetails);
                        }
                        else
                        {
                            OrderDispatchedDetails = OrderDispatchedMaster.orderDetails.Where(aj => aj.OrderId == OrderId && aj.ItemId == ItemId).FirstOrDefault();
                            if (OrderDispatchedDetails == null)
                            {
                                OrderDispatchedDetails = OrderDispatchedMaster.orderDetails.Where(aj => aj.OrderId == OrderId && aj.itemNumber == itemNumber).FirstOrDefault();

                            }
                            return Request.CreateResponse(HttpStatusCode.OK, OrderDispatchedDetails);

                        }
                    }
                    else
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, OrderDispatchedDetails);


                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        //Backend Userr for Assignment Freezed IcVerified

        [Route("IcVerified")]
        [HttpPut]
        [Authorize]
        public HttpResponseMessage IcVerifiedAssignment(AssignmentIcUploadDc assignmetdata)
        {

            logger.Info("start IcUploadedFile  File: ");
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            // Access claims

            if (assignmetdata.DeliveryIssuanceId > 0 && userid > 0)
            {
                using (var db = new AuthContext())
                {
                    var people = db.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                    var DBIssuance = db.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == assignmetdata.DeliveryIssuanceId && x.Status == "Freezed");
                    if (DBIssuance != null)
                    {
                        DBIssuance.IsPhysicallyVerified = assignmetdata.IsPhysicallyVerify;
                        DBIssuance.IsIcVerified = assignmetdata.IsIcVerified;
                        DBIssuance.IcVerifiedComment = assignmetdata.Comment;
                        DBIssuance.UpdatedDate = DateTime.Now;
                        db.Entry(DBIssuance).State = EntityState.Modified;

                        OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                        AssginDeli.DeliveryIssuanceId = DBIssuance.DeliveryIssuanceId;
                        AssginDeli.Cityid = DBIssuance.Cityid;
                        AssginDeli.city = DBIssuance.city;
                        AssginDeli.DisplayName = DBIssuance.DisplayName;
                        AssginDeli.Status = DBIssuance.Status;
                        AssginDeli.WarehouseId = DBIssuance.WarehouseId;
                        AssginDeli.PeopleID = DBIssuance.PeopleID;
                        AssginDeli.VehicleId = DBIssuance.VehicleId;
                        AssginDeli.VehicleNumber = DBIssuance.VehicleNumber;
                        AssginDeli.CreatedDate = indianTime;
                        AssginDeli.UpdatedDate = indianTime;
                        AssginDeli.userid = people.PeopleID;
                        AssginDeli.Description = "Assignment No. : " + DBIssuance.DeliveryIssuanceId + "Is Ic Verified";
                        if (people.DisplayName == null)
                        {
                            AssginDeli.UpdatedBy = people.PeopleFirstName;
                        }
                        else
                        {
                            AssginDeli.UpdatedBy = people.DisplayName;
                        }
                        db.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);

                        int id = db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, DBIssuance);
                    }

                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, false);
        }

        #region get AssignmentTAT  Report details
        /// <summary>
        /// Created Date:21/10/2019
        /// Created by raj 
        /// </summary>
        /// <returns></returns>

        [Route("GetAssignmentTATReportDTO")]
        [HttpPost]

        public List<AssignmentTATReport> GetAssignmentTATReportDTO(AssignmentTATReportDTO tatreport)
        {
            using (var authContext = new AuthContext())
            {
                List<Object> parameters = new List<object>();
                List<AssignmentTATReport> newdata = new List<AssignmentTATReport>();
                string sqlquery = "exec TurnAroundTimeAssignment";



                if (tatreport.startdate.HasValue && tatreport.enddate.HasValue)
                {
                    parameters.Add(new SqlParameter("@StartDate", tatreport.startdate));
                    parameters.Add(new SqlParameter("@EndDate", tatreport.enddate));

                    sqlquery = sqlquery + " @StartDate" + ", @EndDate";
                }

                parameters.Add(new SqlParameter("@WarehouseID", tatreport.WarehouseId));
                if (tatreport.startdate.HasValue && tatreport.enddate.HasValue)
                {
                    sqlquery = sqlquery + " ,@WarehouseID";
                }
                else
                {
                    sqlquery = sqlquery + " @WarehouseID";
                }
                if (tatreport.DeliveryboyId != null)
                {

                    parameters.Add(new SqlParameter("@DboyId", tatreport.DeliveryboyId));

                    sqlquery = sqlquery + " ,@DboyId";

                }

                if (tatreport.AssignmentId.HasValue)
                {
                    parameters.Add(new SqlParameter("@AssignmentId", tatreport.AssignmentId));

                    sqlquery = sqlquery + " ,@AssignmentId";

                }
                try
                {

                    newdata = authContext.Database.SqlQuery<AssignmentTATReport>(sqlquery, parameters.ToArray()).OrderByDescending(x => x.SavedAsDraft).ToList();
                    foreach (var tatdata in newdata)
                    {

                        tatdata.DeliveryBoyName = authContext.Peoples.Where(x => x.PeopleID == tatdata.PeopleID).Select(x => x.DisplayName).FirstOrDefault();

                        //string query = "select STRING_AGG(pr.PaymentFrom,',') from OrderDeliveryMasters odm inner join PaymentResponseRetailerApps pr  on odm.OrderId = pr.OrderId where pr.status = 'Success' and odm.DeliveryIssuanceId = " + tatdata.DeliveryIssuanceId;
                        string query = "select pr.PaymentFrom as PaymentMode from OrderDeliveryMasters odm inner join PaymentResponseRetailerApps pr  on odm.OrderId = pr.OrderId where pr.status = 'Success' and odm.DeliveryIssuanceId = " + tatdata.DeliveryIssuanceId;
                        var pm = authContext.Database.SqlQuery<paymentmode>(query).ToList();

                        if (pm != null)
                        {
                            if (pm != null && pm.Any())
                                tatdata.PaymentMode = string.Join(",", pm.Select(x => x.PaymentMode).Distinct().ToList());
                        }

                    }



                }
                catch (Exception ss)
                {
                }


                return newdata;
            }
        }

        


        #endregion


        #region POC Verification 
        [Route("POCVerification")]
        [HttpGet]
        public HttpResponseMessage GetPOCVerification(int WarehouseId, DateTime start, DateTime end)//
        {
            using (var context = new AuthContext())
            {
                if (WarehouseId > 0 && start != null && end != null)
                {

                    List<POCVerificationReport> _Results = new List<POCVerificationReport>();
                    List<Object> parameters = new List<object>();
                    double POCPercentHubwise = 0;
                    List<POCVerificationDTO> newdata = new List<POCVerificationDTO>();

                    var param = new SqlParameter("StartDate", start);
                    var param1 = new SqlParameter("EndDate", end);
                    var param2 = new SqlParameter("WarehouseID", WarehouseId);

                    newdata = context.Database.SqlQuery<POCVerificationDTO>("exec GetPOCVerification @StartDate, @EndDate, @WarehouseID", param, param1, param2).ToList();
             
                    var result = new List<POCVerificationReport>();
                    if (newdata.Count > 0)
                    {
                        result = newdata.GroupBy(x => x.DboyName).Select(x => new POCVerificationReport
                        {
                            DboyName = x.FirstOrDefault().DboyName,
                            ClusterName = x.FirstOrDefault().ClusterName,
                            PocAMount = x.Sum(y => y.GrossAmount),
                            PocPercentage = (x.Count(y => y.Status == "Delivery Canceled") * 100 / (x.Count(y => y.Status != null))),
                            PocCancelAmount= x.Where(y=>y.Status== "Delivery Canceled").Sum(z=>z.GrossAmount)
                        }).ToList();

                        POCPercentHubwise =(result.Sum(x=>x.PocPercentage) / result.Count());
                      
                        //var ssresult = newdata.GroupBy(x => x.DboyName).ToList();
                    }

                    var response = new
                    {
                        Status = true,
                        Exportdata = newdata,
                        Reportdata = result,
                        POCPercentHub= POCPercentHubwise,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");

            }
        }
        #endregion

        ///// <summary>
        ///// Vinayak 14/01/2020
        ///// </summary>
        ///// <param name="getPOCancelledorders"></param>
        ///// <returns></returns>
        //[Route("GetPOCancelledOrder")]
        //[HttpPost]
        //public HttpResponseMessage GetPOCancelledOrder(POCinterface POCinterfaceDc)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        POCPagerListDTO pagerlist = new POCPagerListDTO();
        //        var postOrderCanceldata = new List<getPOCVerificationDc>();
        //        DateTime date = Convert.ToDateTime("2020-01-01");
        //        if (POCinterfaceDc.AgentId > 0)
        //        {
        //            var data = (from od in context.OrderDispatchedMasters
        //                         join c in context.Customers on
        //                         od.CustomerId equals c.CustomerId
        //                         where c.ExecutiveId == POCinterfaceDc.AgentId && od.Status == "Post Order Canceled"
        //                         && od.WarehouseId == POCinterfaceDc.WarehouseId && od.CreatedDate >= date
        //                         && od.IsPocVerified == false && od.active == true && od.Deleted == false
        //                               select new getPOCVerificationDc
        //                               {
        //                                   DboyName = od.DboyName,
        //                                   ClusterName = od.ClusterName,
        //                                   Status = od.Status,
        //                                   WarehouseName = od.WarehouseName,
        //                                   DeliveryIssuanceId = od.DeliveryIssuanceIdOrderDeliveryMaster,
        //                                   OrderId = od.OrderId,
        //                                   Skcode = od.Skcode,
        //                                   isPocVerified = od.IsPocVerified,
        //                                   Comment = od.comments,
        //                                   GrossAmount = od.GrossAmount,

        //                               }).ToList();

        //            pagerlist.TotalRecords = data.Count();
        //            pagerlist.getPOCVerificationlist = data.OrderBy(x => x.DboyName).Skip(POCinterfaceDc.skip).Take(POCinterfaceDc.take).ToList();
        //        }
        //        else
        //        {
        //            var result = (from od in context.OrderDispatchedMasters
        //                        join c in context.Customers on
        //                        od.CustomerId equals c.CustomerId
        //                        where  od.Status == "Post Order Canceled"
        //                        && od.WarehouseId == POCinterfaceDc.WarehouseId && od.CreatedDate >= date
        //                        && od.IsPocVerified == false && od.active == true && od.Deleted == false
        //                        select new getPOCVerificationDc
        //                        {
        //                            DboyName = od.DboyName,
        //                            ClusterName = od.ClusterName,
        //                            Status = od.Status,
        //                            WarehouseName = od.WarehouseName,
        //                            DeliveryIssuanceId = od.DeliveryIssuanceIdOrderDeliveryMaster,
        //                            OrderId = od.OrderId,
        //                            Skcode = od.Skcode,
        //                            isPocVerified = od.IsPocVerified,
        //                            Comment = od.comments,
        //                            GrossAmount = od.GrossAmount,

        //                        }).ToList();


        //            pagerlist.TotalRecords = result.Count();
        //            pagerlist.getPOCVerificationlist = result.OrderBy(x => x.DboyName).Skip(POCinterfaceDc.skip).Take(POCinterfaceDc.take).ToList();
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, pagerlist);
        //    }
        //}

        /// <summary>
        /// Vinayak 06/05/2020
        /// </summary>
        /// <param name="getPOCancelledorders"></param>
        /// <returns></returns>
        [Route("GetPOCancelledOrder")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<POCPagerListDc> GetPOCancelledOrder(POCinterfaceV1 POCinterfaceDc)
        {
            DeliveryAssignmentManager manager = new DeliveryAssignmentManager();
            var result = await manager.GetPOCancelledOrder(POCinterfaceDc);

            return result;
        }

        // Added by Anoop
        [Route("GetPOCancelledOrderbyMuliW")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<POCPagerListDc> GetPOCancelledOrderbyMuliW(POCinterfaceV1 POCinterfaceDc)
        {
            DeliveryAssignmentManager manager = new DeliveryAssignmentManager();
            var result = await manager.GetPOCancelledOrderbyMuliW(POCinterfaceDc);

            return result;
        }

        /// <summary>
        /// Vinayak 15/01/2020
        /// </summary>
        /// <param name="updatePOCOrderlist"></param>
        /// <returns></returns>
        [Route("updatePOCOrderlist")]
        [HttpPost]
        public HttpResponseMessage updatePOCOrderlist(List<getPOCVerificationDc> getPOCVerificationlist)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                foreach (var item in getPOCVerificationlist)
                {
                    var data = context.OrderDispatchedMasters.Where(x => x.OrderId == item.OrderId && x.active == true && x.Deleted == false && x.IsPocVerified == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.IsPocVerified = true;
                        data.comment = item.remarks;
                        data.UpdatedDate = indianTime;
                        context.Entry(data).State = EntityState.Modified;
                        context.Commit();
                    }

                }
                var response = new
                {
                    Status = true,
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        public class paymentmode 
        {
            public string PaymentMode { get; set; }
        }

        //temp class for Shortitems
        public class AssignmentShortitemDTO
        {
            public List<ShortItemAssignment> AssignmentShortItems { get; set; }
        }

        public class AssignmentTATReportDTO
        {
         

            public int WarehouseId { get; set; }
            public int? DeliveryboyId { get; set; }
            public List<ReportList> selectedReportList { get; set; }
            public DateTime? startdate { get; set; }
            public DateTime? enddate { get; set; }
            public int? AssignmentId { get; set; }

        }
        public class ReportList
        {
            public string label { get; set; }
            public string value { get; set; }
        }

        public class POCVerificationDTO
        {
            public int WarehouseId { get; set; }
            public int OrderId { get; set; }
            public int DeliveryIssuanceId { get; set; }
            public string DboyName { get; set; }
            public string ClusterName { get; set; }
            public string Skcode { get; set; }
            public string Status { get; set; }
            public double GrossAmount { get; set; }
            public string Comment { get; set; }
            public DateTime OrderedDate { get; set; }
            public DateTime? CancellationDate { get; set; }

        }

        public class POCVerificationReport
        {
            public string DboyName { get; set; }//AgentName
            public string ClusterName { get; set; }//AgentName
            public double PocPercentage { get; set; }//AgentName
            public double PocAMount { get; set; }//AgentName
            public double PocCancelAmount { get; set; }
            public List<POCVerificationDTO> POCVerifications { get; set; }//ExportList

        }
       
        public class getPOCVerificationDc
        {
            public string DboyName { get; set; }
            public string ClusterName { get; set; }//AgentName
            public string Status { get; set; }
            public int OrderId { get; set; }
            public string WarehouseName { get; set; }
            public int? DeliveryIssuanceId { get; set; }
            public string Skcode { get; set; }
            public bool isPocVerified { get; set; }
            public string Comment { get; set; }
            public string remarks { get; set; }
            public double GrossAmount { get; set; }

        }



    }
}


