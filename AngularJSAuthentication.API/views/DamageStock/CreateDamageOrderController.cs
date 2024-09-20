using AngularJSAuthentication.DataContracts.NonRevenueOrderDc;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.API.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/damageorder")]
    public class CreateDamageOrderController : ApiController
    {      
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Create Multi Damage Order 
        [Route("createDS")]
        [HttpPost]
        public HttpResponseMessage postDS(List<DamageOrderCartDc> Do)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                DamageOrderMaster data = new DamageOrderMaster();
                ResponseMsg Res = new ResponseMsg();

                if (Do[0].OrderType == "NR")
                {
                    NonRevenueOrderHelper helper = new NonRevenueOrderHelper();
                    data = helper.CreateNonRevenueOrder(Do, userid);
                } 
                else
                {
                     data = AddDamageOrderMultiItem(Do, context, userid, compid);
                }
                if (data == null && Res.Status == false)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Occured");
                }
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
        }
        #endregion

        public DamageOrderMaster AddDamageOrderMultiItem(List<DamageOrderCartDc> sc, AuthContext context, int UserId, int CompanyId)
        {
            List<ItemMaster> itemsList = null;
            List<DamageStock> DamageStockList = null;
            List<NonSellableStock> NonSellableStockList = null;
            DamageOrderMaster objOrderMaster = new DamageOrderMaster();
            double finaltotal = 0;
            double finalTaxAmount = 0;
            double finalGrossAmount = 0;
            double TotalAmount = 0;
            double finalTotalTaxAmount = 0;
            int Customerid = sc[0].CustomerId;
            int warehouseid = sc[0].Warehouseid;
            int companyid = CompanyId;
            Customer cust = context.Customers.Where(c => c.CustomerId == Customerid).SingleOrDefault();
            if (cust != null)
            {
                cust.ShippingAddress = cust.ShippingAddress;
                cust.CompanyId = 1;
                cust.ordercount = cust.ordercount + 1;
                cust.MonthlyTurnOver = cust.MonthlyTurnOver + sc[0].TotalAmount;
                context.Entry(cust).State = EntityState.Modified;

                objOrderMaster.CompanyId = companyid;
                objOrderMaster.CustomerCategoryId = 2;
                objOrderMaster.Status = "Pending";
                objOrderMaster.CustomerName = cust.Name;
                objOrderMaster.Customerphonenum = cust.Mobile;
                objOrderMaster.ShopName = cust.ShopName;
                objOrderMaster.Skcode = cust.Skcode;
                objOrderMaster.Tin_No = cust.RefNo;
                objOrderMaster.CustomerType = cust.CustomerType;
                objOrderMaster.WarehouseId = cust.Warehouseid ?? 0;
                objOrderMaster.WarehouseName = cust.WarehouseName;
                objOrderMaster.CustomerId = cust.CustomerId;
                objOrderMaster.CityId = cust.Cityid;
                objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
                objOrderMaster.ClusterName = cust.ClusterName;
                objOrderMaster.BillingAddress = cust.ShippingAddress;
                objOrderMaster.ShippingAddress = cust.ShippingAddress;
                objOrderMaster.active = true;
                objOrderMaster.CreatedDate = indianTime;
                objOrderMaster.isDamageOrder = sc[0].OrderType == "N" ? true : false;
                objOrderMaster.OrderTypes = sc[0].OrderType=="N" ? 1: 0;
                
                if (indianTime.Hour > 16)
                {
                    objOrderMaster.Deliverydate = indianTime.AddDays(2);
                }
                else
                {
                    objOrderMaster.Deliverydate = indianTime.AddDays(1);
                }
                objOrderMaster.UpdatedDate = indianTime;
                objOrderMaster.Deleted = false;
                objOrderMaster.Createby = UserId;
                List<DamageOrderDetails> collection = new List<DamageOrderDetails>();
                objOrderMaster.DamageorderDetails = collection;
                var DamageStockIds = sc.Select(x => x.DamageStockId).ToList();
                List<long> longstockItd = DamageStockIds.ConvertAll(i => (long)i);
                List<long> vOut = DamageStockIds.Select(x => (long)x).ToList();

                var isdamageStockList = sc.Select(x => x.isDamageOrder).FirstOrDefault();
                if(isdamageStockList)
                {
                    NonSellableStockList = context.NonSellableStockDB.Where(x => vOut.Contains(x.NonSellableStockId) && x.WarehouseId == warehouseid).ToList();

                    var ItemMultiMRPIds = sc.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    itemsList = context.itemMasters.Where(x => ItemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == warehouseid).ToList();                   
                }
                else
                {
                    DamageStockList = context.DamageStockDB.Where(x => DamageStockIds.Contains(x.DamageStockId) && x.WarehouseId == warehouseid).ToList();
                    var ItemNumbers = sc.Select(x => x.ItemNumber).Distinct().ToList();
                    itemsList = context.itemMasters.Where(x => ItemNumbers.Contains(x.Number) && x.WarehouseId == warehouseid).ToList();
                }                

                foreach (var ds in sc)
                {
                    if (isdamageStockList)
                    {
                        var stock = NonSellableStockList.FirstOrDefault(x => x.NonSellableStockId == ds.DamageStockId);
                        if (ds.qty != 0 && ds.qty > 0 && stock != null && ds.qty <= stock.Inventory)
                        {
                            if (ds.ItemNumber == null)
                            {
                                ds.ItemNumber = context.ItemMultiMRPDB.Where(x => x.ItemMultiMRPId == ds.ItemMultiMRPId && x.Deleted == false).Select(x=>x.ItemNumber).FirstOrDefault();
                            }
                            itemsList = context.itemMasters.Where(x => x.Number == ds.ItemNumber && x.WarehouseId == warehouseid).ToList();
                            var items = itemsList.Where(x => x.Number == ds.ItemNumber).FirstOrDefault();

                            
                            //var items = itemsList.Where(x => x.ItemMultiMRPId == ds.ItemMultiMRPId).FirstOrDefault();
                            DamageOrderDetails od = new DamageOrderDetails();
                            od.CustomerId = cust.CustomerId;
                            od.CustomerName = cust.Name;
                            od.City = cust.City;
                            od.CityId = cust.Cityid;
                            od.Mobile = cust.Mobile;
                            od.OrderDate = indianTime;
                            od.Status = "Pending";
                            od.CompanyId = companyid;
                            od.WarehouseId = cust.Warehouseid ?? 0;
                            od.WarehouseName = cust.WarehouseName;
                            //if (stock.PurchasePrice > 0)
                            //{
                            //    od.NetPurchasePrice = stock.PurchasePrice;
                            //}
                            //else
                            //{
                                od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                            //}
                            od.SellingSku = items.SellingSku;
                            od.ItemId = items.ItemId;
                            od.Itempic = items.LogoUrl;
                            od.itemname = stock.ItemName;
                            od.itemcode = items.itemcode;
                            od.itemNumber = items.Number;
                            od.Barcode = items.itemcode;
                            od.UnitPrice = ds.UnitPrice;//meaan seeling prices
                            od.DefaultUnitPrice = ds.DefaultUnitPrice > 0 ? ds.DefaultUnitPrice : stock.UnitPrice;// dt.UnitPrice;
                            od.price = items.price;
                            od.MinOrderQty = 1;
                            int MOQ = items.MinOrderQty;
                            od.MinOrderQtyPrice = MOQ * ds.UnitPrice;
                            od.qty = Convert.ToInt32(ds.qty);
                            int qty = 0;
                            qty = Convert.ToInt32(od.qty);
                            od.SizePerUnit = items.SizePerUnit;
                            od.TaxPercentage = items.TotalTaxPercentage;
                            //........CALCULATION FOR NEW SHOPKIRANA.............................
                            od.Noqty = qty; // for total qty (no of items)
                                            // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
                            od.TotalAmt = System.Math.Round(od.UnitPrice * qty, 2);
                            // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * qty) / (1 + od.TaxPercentage / 100)) / 100;

                            // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);

                            //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;

                            //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
                            od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                            //...............Calculate Discount.............................
                            od.DiscountPercentage = items.PramotionalDiscount;
                            od.DiscountAmmount = (od.NetAmmount * items.PramotionalDiscount) / 100;
                            double DiscountAmmount = od.DiscountAmmount;
                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            double TaxAmmount = od.TaxAmmount;
                            od.Purchaseprice = items.price;
                            //od.VATTax = items.VATTax;
                            od.CreatedDate = indianTime;
                            od.UpdatedDate = indianTime;
                            od.ABCClassification = ds.ABCClassification;
                            od.Deleted = false;
                            od.ItemMultiMRPId = stock.ItemMultiMRPId;
                            TotalAmount += od.TotalAmt;
                            od.StockBatchMasterId = ds.StockBatchMasterId;
                            objOrderMaster.DamageorderDetails.Add(od);

                            finaltotal = finaltotal + od.TotalAmt;
                            finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                            finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                            finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;
                        }
                    }
                    else
                    {
                        var stock = DamageStockList.FirstOrDefault(x => x.DamageStockId == ds.DamageStockId);
                        if (ds.qty != 0 && ds.qty > 0 && stock != null && ds.qty <= stock.DamageInventory)
                        {
                            var items = itemsList.Where(x => x.Number == ds.ItemNumber).FirstOrDefault();
                            DamageOrderDetails od = new DamageOrderDetails();
                            od.CustomerId = cust.CustomerId;
                            od.CustomerName = cust.Name;
                            od.City = cust.City;
                            od.CityId = cust.Cityid;
                            od.Mobile = cust.Mobile;
                            od.OrderDate = indianTime;
                            od.Status = "Pending";
                            od.CompanyId = companyid;
                            od.WarehouseId = cust.Warehouseid ?? 0;
                            od.WarehouseName = cust.WarehouseName;
                            if (stock.PurchasePrice > 0)
                            {
                                od.NetPurchasePrice = stock.PurchasePrice;
                            }
                            else
                            {
                                od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                            }
                            od.SellingSku = items.SellingSku;
                            od.ItemId = items.ItemId;
                            od.Itempic = items.LogoUrl;
                            od.itemname = stock.ItemName;
                            od.itemcode = items.itemcode;
                            od.itemNumber = stock.ItemNumber;
                            od.Barcode = items.itemcode;
                            od.UnitPrice = ds.UnitPrice;//meaan seeling prices
                            od.DefaultUnitPrice = ds.DefaultUnitPrice > 0 ? ds.DefaultUnitPrice : stock.UnitPrice;// dt.UnitPrice;
                            od.price = stock.MRP;
                            od.MinOrderQty = 1;
                            int MOQ = items.MinOrderQty;
                            od.MinOrderQtyPrice = MOQ * ds.UnitPrice;
                            od.qty = Convert.ToInt32(ds.qty);
                            int qty = 0;
                            qty = Convert.ToInt32(od.qty);
                            od.SizePerUnit = items.SizePerUnit;
                            od.TaxPercentage = items.TotalTaxPercentage;
                            //........CALCULATION FOR NEW SHOPKIRANA.............................
                            od.Noqty = qty; // for total qty (no of items)
                                            // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
                            od.TotalAmt = System.Math.Round(od.UnitPrice * qty, 2);
                            // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * qty) / (1 + od.TaxPercentage / 100)) / 100;

                            // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);

                            //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;

                            //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
                            od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                            //...............Calculate Discount.............................
                            od.DiscountPercentage = items.PramotionalDiscount;
                            od.DiscountAmmount = (od.NetAmmount * items.PramotionalDiscount) / 100;
                            double DiscountAmmount = od.DiscountAmmount;
                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            double TaxAmmount = od.TaxAmmount;
                            od.Purchaseprice = items.price;
                            //od.VATTax = items.VATTax;
                            od.CreatedDate = indianTime;
                            od.UpdatedDate = indianTime;
                            od.ABCClassification = ds.ABCClassification;
                            od.Deleted = false;
                            od.ItemMultiMRPId = stock.ItemMultiMRPId;
                            TotalAmount += od.TotalAmt;
                            od.StockBatchMasterId = ds.StockBatchMasterId;
                            objOrderMaster.DamageorderDetails.Add(od);

                            finaltotal = finaltotal + od.TotalAmt;
                            finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                            finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                            finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;
                        }
                    }
                       
                   
                }
                objOrderMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
               // objOrderMaster.DiscountAmount = finalTotalTaxAmount - finaltotal;
               
                objOrderMaster.TotalAmount = Convert.ToInt32(TotalAmount);
                objOrderMaster.GrossAmount = System.Math.Round(finalGrossAmount, 0); ;
                // objOrderMaster.invoice_no = "Od_" + Convert.ToString(order.DamageOrderId + 1);
                context.DamageOrderMasterDB.Add(objOrderMaster);
                if (context.Commit() > 0) 
                {
                    return objOrderMaster;

                };
            }
            return objOrderMaster;
        }

     
    }
}


