using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.DataContracts.Transaction.BackendOrder;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PlaceOrder;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/KppOrder")]
    public class KppOrderController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Generate  Order 
        [Route("Generate")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<bool> GenerateOrder(KppOrderShoppingCartDc sc)
        {

            int compid = 1;
            var result = new OrderMaster();
            var ShoppingCarts = new KppOrderShoppingCartDc();

            List<string> NumberLists = null;
            List<double> MRPLists = null;
            List<KppOrderShoppingCartItemsDc> ListofItemNotexits = null;
            List<ItemMaster> ItemMastersList = null;
            List<ItemMasterCentral> ItemMastersCentralList = null;
            List<ItemMultiMRP> ItemMultiMRPList = null;
            Warehouse warehouse = null;
            People people = null;
            Customer cust = null;
            List<ItemMaster> NewaddItemList = null;
            List<ItemMaster> CombineNewItem = null;
            if (sc.itemDetails.Any() && sc.CustomerId > 0 && sc.WarehouseId > 0 && sc.DboyId > 0 && sc.PeopleId > 0)
            {
                //Check List contain null value of ItemNumber, ItemName
                bool IsItemNumberNull = sc.itemDetails.Any(o => o.ItemNumber == null || o.ItemNumber == "" || o.ItemName == null || o.ItemName == "");
                if (IsItemNumberNull)
                {
                    return false;
                }
                //Check QtyCheck <=
                var QtyCheck = sc.itemDetails.Sum(o => o.Qty);
                if (QtyCheck<=0)
                {
                    return false;
                }
                using (var context = new AuthContext())
                {
                    //Is Already posted Check
                    bool IsPosted = context.DbOrderMaster.Any(c => c.invoice_no == sc.InvoiceNo);
                    if (IsPosted)
                    {
                        return true;
                    }

                    NumberLists = sc.itemDetails.Select(w => w.ItemNumber).ToList();
                    MRPLists = sc.itemDetails.Select(w => w.MRP).ToList();

                    people = context.Peoples.FirstOrDefault(c => c.PeopleID == sc.PeopleId);
                    cust = context.Customers.FirstOrDefault(c => c.CustomerId == sc.CustomerId);
                    warehouse = context.Warehouses.FirstOrDefault(w => w.WarehouseId == sc.WarehouseId);

                    ItemMastersList = context.itemMasters.Where(w => NumberLists.Contains(w.Number) && MRPLists.Contains(w.price) && w.WarehouseId == warehouse.WarehouseId).ToList();

                    var ss = sc.itemDetails.Select(x => new ItemMaster
                    {
                        MRP = x.MRP,
                        Number = x.ItemNumber,
                        itemname = x.ItemName,
                        UnitPrice = x.UnitPrice
                    }).ToList();

                    var ItemNotexits = ss.Where(a => !ItemMastersList.Any(b => b.Number == a.ItemNumber && b.price == a.MRP)).ToList();
                    //var ItemNotexits = sc.itemDetails.Where(p => ItemMastersList.All(p2 => p2.Number != p.ItemNumber && p2.price != p.MRP));
                    if (ItemNotexits != null && ItemNotexits.Any())
                    {
                        ListofItemNotexits = ItemNotexits.Select(x => new KppOrderShoppingCartItemsDc
                        {
                            ItemNumber = x.ItemNumber,
                            MRP = x.MRP,
                            ItemName = x.itemname,
                            UnitPrice = x.UnitPrice
                        }).ToList();

                        var reqNumberLists = ListofItemNotexits.Select(w => w.ItemNumber).ToList();
                        var reqMRP = ListofItemNotexits.Select(w => w.MRP).ToList();

                        ItemMultiMRPList = context.ItemMultiMRPDB.Where(w => reqNumberLists.Contains(w.ItemNumber) && reqMRP.Contains(w.MRP)).ToList();
                        ItemMastersCentralList = context.ItemMasterCentralDB.Where(w => reqNumberLists.Contains(w.Number)).ToList();

                        NewaddItemList = GenerteItemsInItemMaster(ListofItemNotexits, ItemMastersCentralList, ItemMultiMRPList, warehouse, context, compid, people);//Post order
                    }

                    if (NewaddItemList != null)
                    {
                        var ItemLists = ItemMastersList.Concat(NewaddItemList);
                        CombineNewItem = Mapper.Map(ItemLists).ToANew<List<ItemMaster>>();
                    }
                    else
                    {
                        CombineNewItem = Mapper.Map(ItemMastersList).ToANew<List<ItemMaster>>();
                    }
                    ShoppingCarts.itemDetails = sc.itemDetails.Select(x => new KppOrderShoppingCartItemsDc
                    {
                        Qty = x.Qty,
                        MRP = x.MRP,
                        ItemId = CombineNewItem.FirstOrDefault(f => f.Number == x.ItemNumber && f.price == x.MRP).ItemId,
                        CompanyId = compid,
                        WarehouseId = warehouse.WarehouseId,
                        UnitPrice = x.UnitPrice,
                        ItemName = x.ItemName,
                        ItemNumber = x.ItemNumber
                    }).ToList();

                    ShoppingCarts.Customerphonenum = cust.Mobile;
                    ShoppingCarts.CustomerName = cust.Name;
                    ShoppingCarts.ShippingAddress = cust.ShippingAddress;
                    ShoppingCarts.TotalAmount = 0;
                    ShoppingCarts.CreatedDate = sc.CreatedDate;
                    ShoppingCarts.paymentMode = sc.paymentMode;
                    ShoppingCarts.TrupayTransactionId = sc.TrupayTransactionId;
                    ShoppingCarts.paymentThrough = sc.paymentThrough;
                    ShoppingCarts.TotalAmount = sc.TotalAmount;
                    ShoppingCarts.DboyId = sc.DboyId;
                    ShoppingCarts.InvoiceNo = sc.InvoiceNo;

                    ShoppingCarts.DeliveryCharge = sc?.DeliveryCharge > 0 ? sc.DeliveryCharge.Value : 0;

                    ShoppingCarts.BillDiscountAmount = sc?.BillDiscountAmount > 0 ? sc.BillDiscountAmount.Value : 0;

                    result = GenearteKppOrder(ShoppingCarts, context, cust, people, warehouse, CombineNewItem);//Post order
                    if (result != null && result.OrderId > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private OrderMaster GenearteKppOrder(KppOrderShoppingCartDc sc, AuthContext context, Customer Customers, People people, Warehouse warehouse, List<ItemMaster> ItemLists)
        {
            OrderMaster objOrderMaster = new OrderMaster();
            objOrderMaster.orderDetails = new List<OrderDetails>();
            var cust = Customers;
            #region Prepair order 
            objOrderMaster.CompanyId = warehouse.CompanyId;
            objOrderMaster.WarehouseId = warehouse.WarehouseId;
            objOrderMaster.WarehouseName = warehouse.WarehouseName;
            objOrderMaster.CustomerCategoryId = 2;
            objOrderMaster.Status = "Ready to Dispatch";
            objOrderMaster.CustomerName = sc.CustomerName;
            objOrderMaster.ShopName = cust.ShopName;
            objOrderMaster.BillingAddress = sc.ShippingAddress;
            objOrderMaster.ShippingAddress = sc.ShippingAddress;
            objOrderMaster.Customerphonenum = sc.Customerphonenum;
            objOrderMaster.LandMark = cust.LandMark;
            objOrderMaster.Skcode = cust.Skcode;
            objOrderMaster.Tin_No = cust.RefNo;
            objOrderMaster.CustomerType = cust.CustomerType;
            objOrderMaster.CustomerId = cust.CustomerId;
            objOrderMaster.CityId = warehouse.Cityid;
            objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
            if (cust.ClusterId > 0)
            {
                objOrderMaster.ClusterName = cust.ClusterName;
            }
            else
            {
                var clstr = context.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
                if (clstr != null)
                {
                    objOrderMaster.ClusterName = clstr.ClusterName;
                }
            }
            objOrderMaster.OrderTakenSalesPersonId = 0;
            objOrderMaster.OrderTakenSalesPerson = "Self";
            objOrderMaster.active = true;
            objOrderMaster.CreatedDate = sc.CreatedDate;
            if (sc.CreatedDate.Hour > 16)
            {
                objOrderMaster.Deliverydate = sc.CreatedDate.AddDays(2);
            }
            else
            {
                objOrderMaster.Deliverydate = sc.CreatedDate.AddDays(1);
            }

            objOrderMaster.UpdatedDate = indianTime;
            objOrderMaster.Deleted = false;
            #endregion
            var itemMastersList = ItemLists;
            var itemNumbers = itemMastersList.GroupBy(x => new { x.Number, x.ItemMultiMRPId }).ToList();
            foreach (var it in sc.itemDetails.Where(c => c.Qty > 0))
            {
                var items = itemMastersList.Where(x => x.ItemId == it.ItemId).FirstOrDefault();
                OrderDetails od = new OrderDetails();
                od.CustomerName = sc.CustomerName;
                od.Mobile = sc.Customerphonenum;
                od.CustomerId = cust.CustomerId;
                od.CityId = warehouse.Cityid;
                od.OrderDate = indianTime;
                od.Status = "Ready to Dispatch";
                od.CompanyId = warehouse.CompanyId;
                od.WarehouseId = warehouse.WarehouseId;
                od.WarehouseName = warehouse.WarehouseName;
                od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                od.ItemId = items.ItemId;
                od.ItemMultiMRPId = items.ItemMultiMRPId;
                od.Itempic = items.LogoUrl;
                od.itemname = items.itemname;
                od.SupplierName = items.SupplierName;
                od.SellingUnitName = items.SellingUnitName;
                od.CategoryName = items.CategoryName;
                od.SubsubcategoryName = items.SubsubcategoryName;
                od.SubcategoryName = items.SubcategoryName;
                od.SellingSku = items.SellingSku;
                od.City = items.CityName;
                od.itemcode = items.itemcode;
                od.HSNCode = items.HSNCode;
                od.itemNumber = items.Number;
                od.Barcode = items.itemcode;

                od.UnitPrice = it.UnitPrice;

                od.price = items.price;
                od.MinOrderQty = 1;//items.MinOrderQty;
                od.MinOrderQtyPrice = (od.MinOrderQty * od.UnitPrice);
                od.qty = Convert.ToInt32(it.Qty);
                od.SizePerUnit = items.SizePerUnit;
                od.TaxPercentage = items.TotalTaxPercentage;
                if (od.TaxPercentage >= 0)
                {
                    od.SGSTTaxPercentage = od.TaxPercentage / 2;
                    od.CGSTTaxPercentage = od.TaxPercentage / 2;
                }
                od.Noqty = od.qty; // for total qty (no of items)    
                od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

                if (items.TotalCessPercentage > 0)
                {
                    od.TotalCessPercentage = items.TotalCessPercentage;
                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                    od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                }


                double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                if (od.TaxAmmount >= 0)
                {
                    od.SGSTTaxAmmount = od.TaxAmmount / 2;
                    od.CGSTTaxAmmount = od.TaxAmmount / 2;
                }
                //for cess
                if (od.CessTaxAmount > 0)
                {
                    double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                    od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                }
                else
                {
                    od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                }
                od.DiscountPercentage = 0;
                od.DiscountAmmount = 0;
                double DiscountAmmount = od.DiscountAmmount;
                double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                double TaxAmmount = od.TaxAmmount;
                od.Purchaseprice = items.PurchasePrice;
                od.CreatedDate = sc.CreatedDate;
                od.UpdatedDate = indianTime;
                od.Deleted = false;
                od.marginPoint = 0;
                objOrderMaster.orderDetails.Add(od);
            }
            objOrderMaster.DiscountAmount = 0;
            objOrderMaster.ClusterId = cust.ClusterId ?? 0;
            objOrderMaster.ClusterName = cust.ClusterName;
            objOrderMaster.paymentMode = sc.paymentMode;// "COD";
            objOrderMaster.Status = "Ready to Dispatch";
            objOrderMaster.OrderType = 5;
            objOrderMaster.invoice_no = sc.InvoiceNo;
            double DeliveryAmount = 0;
            DeliveryAmount = sc?.DeliveryCharge > 0 ? sc.DeliveryCharge.Value : 0;

           
            objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
            objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
            objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
            objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt));
           
            objOrderMaster.deliveryCharge = DeliveryAmount;
            objOrderMaster.TotalAmount += DeliveryAmount;


            objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0);
            objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
            objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);



            context.DbOrderMaster.Add(objOrderMaster);
            context.Commit();

            if (objOrderMaster.OrderId != 0)
            {
                objOrderMaster.orderProcess = true;
                if (sc.paymentMode.ToLower() == "cod")
                {
                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                    {
                        amount = Math.Round((sc.TotalAmount + DeliveryAmount), 0),
                        CreatedDate = sc.CreatedDate,
                        currencyCode = "INR",
                        OrderId = objOrderMaster.OrderId,
                        PaymentFrom = "Cash",
                        status = "Success",
                        statusDesc = "Order Place",
                        UpdatedDate = sc.CreatedDate,
                        IsRefund = false
                    });
                }
                else if (sc.paymentMode.ToLower() == "online")
                {
                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                    {
                        amount = Math.Round(sc.TotalAmount, 0),
                        CreatedDate = sc.CreatedDate,
                        currencyCode = "INR",
                        OrderId = objOrderMaster.OrderId,
                        PaymentFrom = "hdfc",//sc.paymentMode, we set to Hdfc to show Online in Delivery app 
                        PaymentThrough = sc.paymentMode,
                        GatewayTransId = sc.TrupayTransactionId,
                        status = "Success",
                        statusDesc = "Order Place and set to (Hdfc) due to show Online in Delivery app",
                        UpdatedDate = sc.CreatedDate,
                        IsRefund = false,
                        IsOnline = true
                    });
                    if (DeliveryAmount > 0 && sc.TotalAmount != objOrderMaster.TotalAmount)
                    {
                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                        {
                            amount = Math.Round((DeliveryAmount), 0),
                            CreatedDate = sc.CreatedDate,
                            currencyCode = "INR",
                            OrderId = objOrderMaster.OrderId,
                            PaymentFrom = "Cash",
                            status = "Success",
                            statusDesc = "Order Place add DeliveryAmount",
                            UpdatedDate = sc.CreatedDate,
                            IsRefund = false
                        });
                    }
                }
                string Borderid = Convert.ToString(objOrderMaster.OrderId);
                string BorderCodeId = Borderid.PadLeft(11, '0');
                temOrderQBcode code = context.GetBarcode(BorderCodeId);
                objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
                context.Entry(objOrderMaster).State = EntityState.Modified;

                ///// Do  ReadytoDispatched
                OrderDispatchedMaster postOrderDispatch = new OrderDispatchedMaster();
                postOrderDispatch = Mapper.Map(objOrderMaster).ToANew<OrderDispatchedMaster>();
                if (sc.DboyId > 0)
                {
                    var BackendCOD_Dboy = context.Peoples.FirstOrDefault(x => x.PeopleID == sc.DboyId && x.WarehouseId == warehouse.WarehouseId);
                    if (BackendCOD_Dboy != null)
                    {
                        postOrderDispatch.DboyMobileNo = BackendCOD_Dboy.Mobile;
                        postOrderDispatch.DboyName = BackendCOD_Dboy.DisplayName;
                        postOrderDispatch.DBoyId = BackendCOD_Dboy.PeopleID;
                    }
                    else
                    {
                        postOrderDispatch.DboyMobileNo = people.Mobile;
                        postOrderDispatch.DboyName = people.DisplayName;
                        postOrderDispatch.DBoyId = people.PeopleID;
                    }
                }
                else
                {
                    postOrderDispatch.DboyMobileNo = people.Mobile;
                    postOrderDispatch.DboyName = people.DisplayName;
                    postOrderDispatch.DBoyId = people.PeopleID;
                }
                postOrderDispatch.Status = "Ready to Dispatch";
                postOrderDispatch.CreatedDate = indianTime;
                postOrderDispatch.OrderedDate = objOrderMaster.CreatedDate;

                double finaltotal = 0;
                double finalTaxAmount = 0;
                double finalSGSTTaxAmount = 0;
                double finalCGSTTaxAmount = 0;
                double finalGrossAmount = 0;
                double finalTotalTaxAmount = 0;
                double finalCessTaxAmount = 0;
                foreach (var pc in postOrderDispatch.orderDetails.Where(x => x.qty > 0))
                {
                    int MOQ = pc.MinOrderQty;
                    pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;
                    pc.TaxPercentage = pc.TaxPercentage;
                    pc.TotalCessPercentage = pc.TotalCessPercentage;

                    if (pc.TaxPercentage >= 0)
                    {
                        pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
                        pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
                    }

                    pc.HSNCode = pc.HSNCode;
                    pc.Noqty = pc.qty;
                    pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
                    if (pc.TotalCessPercentage > 0)
                    {
                        pc.TotalCessPercentage = pc.TotalCessPercentage;
                        double tempPercentagge = pc.TotalCessPercentage + pc.TaxPercentage;

                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                        pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
                    }
                    double tempPercentagge2 = pc.TotalCessPercentage + pc.TaxPercentage;
                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
                    pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
                    if (pc.TaxAmmount >= 0)
                    {
                        pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
                        pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
                    }

                    if (pc.CessTaxAmount > 0)
                    {
                        double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;
                    }
                    else
                    {
                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
                    }
                    finalGrossAmount = finalGrossAmount + pc.TotalAmountAfterTaxDisc;
                    finalTotalTaxAmount = finalTotalTaxAmount + pc.TotalAmountAfterTaxDisc;
                    pc.DiscountAmmount = 0;
                    pc.NetAmtAfterDis = 0;
                    pc.Purchaseprice = 0;
                    pc.CreatedDate = indianTime;
                    pc.UpdatedDate = indianTime;
                    pc.Deleted = false;

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
                finaltotal = finaltotal + postOrderDispatch.deliveryCharge;
                finalGrossAmount = finalGrossAmount + postOrderDispatch.deliveryCharge;
                if (postOrderDispatch.WalletAmount > 0)
                {
                    postOrderDispatch.TotalAmount = Math.Round(finaltotal, 2) - postOrderDispatch.WalletAmount.GetValueOrDefault();
                    postOrderDispatch.TaxAmount = Math.Round(finalTaxAmount, 2);
                    postOrderDispatch.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                    postOrderDispatch.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                    postOrderDispatch.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - postOrderDispatch.WalletAmount.GetValueOrDefault()), 0);
                }
                else
                {
                    postOrderDispatch.TotalAmount = Math.Round(finaltotal, 2);
                    postOrderDispatch.TaxAmount = Math.Round(finalTaxAmount, 2);
                    postOrderDispatch.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                    postOrderDispatch.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                    postOrderDispatch.GrossAmount = Math.Round((finalGrossAmount), 0);
                }


                OrderMasterHistories h1 = new OrderMasterHistories();
                h1.orderid = objOrderMaster.OrderId;
                h1.Status = objOrderMaster.Status;
                h1.Reasoncancel = "No Change"; ;
                h1.Warehousename = objOrderMaster.WarehouseName;
                if (people.DisplayName != null)
                {
                    h1.username = people.DisplayName;
                }
                else
                {
                    h1.username = people.PeopleFirstName;

                }
                h1.userid = people.PeopleID;
                h1.CreatedDate = DateTime.Now;
                context.OrderMasterHistoriesDB.Add(h1);

                #region Billdiscountamount
                double offerDiscountAmount = 0;
                offerDiscountAmount = objOrderMaster.BillDiscountAmount ?? 0;
                postOrderDispatch.TotalAmount = postOrderDispatch.TotalAmount - offerDiscountAmount;
                postOrderDispatch.BillDiscountAmount = offerDiscountAmount;
                postOrderDispatch.GrossAmount = System.Math.Round(postOrderDispatch.TotalAmount, 0);
                #endregion
                context.OrderDispatchedMasters.Add(postOrderDispatch);
                if (context.Commit() > 0)
                {
                    return objOrderMaster;
                }
            }
            return objOrderMaster;
        }
        #endregion
        #region GenerateItem
        public List<ItemMaster> GenerteItemsInItemMaster(List<KppOrderShoppingCartItemsDc> Kppitem, List<ItemMasterCentral> ItemMastersCentralList, List<ItemMultiMRP> ItemMultiMRPList, Warehouse warehouse, AuthContext context, int CompanyId, People people)
        {
            List<ItemMaster> addItem = new List<ItemMaster>();
            foreach (var itemmaster in Kppitem)
            {
                var centralitem = ItemMastersCentralList.FirstOrDefault(f => f.Number == itemmaster.ItemNumber);
                ItemMultiMRP MultiMRPitem = null;
                var recordExits = ItemMultiMRPList.FirstOrDefault(x => x.ItemNumber == itemmaster.ItemNumber && x.Deleted == false && x.MRP == itemmaster.MRP);
                if (recordExits != null)
                {
                    MultiMRPitem = recordExits;
                }
                else
                {
                    ItemMultiMRP Immrp = new ItemMultiMRP();
                    Immrp.CompanyId = CompanyId;
                    Immrp.ItemNumber = itemmaster.ItemNumber;
                    Immrp.itemname = itemmaster.ItemName;
                    Immrp.itemBaseName = centralitem.itemBaseName;
                    Immrp.MRP = itemmaster.MRP;
                    Immrp.UnitofQuantity = centralitem.UnitofQuantity;
                    Immrp.UOM = centralitem.UOM;
                    Immrp.CreatedDate = indianTime;
                    Immrp.UpdatedDate = indianTime;
                    context.ItemMultiMRPDB.Add(Immrp);
                    context.Commit();
                    MultiMRPitem = Immrp;
                }
                ItemMaster it = new ItemMaster();
                it.GruopID = centralitem.GruopID;
                it.TGrpName = centralitem.TGrpName;
                it.TotalTaxPercentage = centralitem.TotalTaxPercentage;
                it.CessGrpID = centralitem.GruopID;
                it.CessGrpName = centralitem.TGrpName;
                it.TotalCessPercentage = centralitem.TotalCessPercentage;
                it.CatLogoUrl = centralitem.LogoUrl;
                it.WarehouseId = warehouse.WarehouseId;
                it.WarehouseName = warehouse.WarehouseName;
                it.BaseCategoryid = centralitem.BaseCategoryid;
                it.LogoUrl = centralitem.LogoUrl;
                it.UpdatedDate = indianTime;
                it.CreatedDate = indianTime;
                it.CategoryName = centralitem.CategoryName;
                it.Categoryid = centralitem.Categoryid;
                it.SubcategoryName = centralitem.SubcategoryName;
                it.SubCategoryId = centralitem.SubCategoryId;
                it.SubsubcategoryName = centralitem.SubsubcategoryName;
                it.SubsubCategoryid = centralitem.SubsubCategoryid;
                it.SubSubCode = centralitem.SubSubCode;
                it.itemcode = centralitem.itemcode;
                it.marginPoint = centralitem.marginPoint;
                it.Number = centralitem.Number;
                it.PramotionalDiscount = centralitem.PramotionalDiscount;
                it.MinOrderQty = 1; //centralitem.MinOrderQty;
                it.NetPurchasePrice = centralitem.NetPurchasePrice;
                it.GeneralPrice = centralitem.GeneralPrice;
                it.promoPerItems = centralitem.promoPerItems;
                it.promoPoint = centralitem.promoPoint;
                it.PurchaseMinOrderQty = 1;// centralitem.PurchaseMinOrderQty;
                it.PurchasePrice = centralitem.PurchasePrice;
                it.SizePerUnit = centralitem.SizePerUnit;
                it.VATTax = centralitem.VATTax;
                it.HSNCode = centralitem.HSNCode;
                it.HindiName = centralitem.HindiName;
                it.CompanyId = centralitem.CompanyId;
                it.Reason = centralitem.Reason;
                it.DefaultBaseMargin = centralitem.DefaultBaseMargin;
                it.Deleted = false;
                it.active = false;
                it.UOM = centralitem.UOM;
                it.UnitofQuantity = centralitem.UnitofQuantity;
                it.IsSensitive = centralitem.IsSensitive;
                it.IsSensitiveMRP = centralitem.IsSensitiveMRP;
                it.ShelfLife = centralitem.ShelfLife;
                it.IsReplaceable = centralitem.IsReplaceable;
                it.BomId = centralitem.BomId;
                it.Type = centralitem.Type;
                //string sAllCharacter = ItemMastersCentralList.FirstOrDefault(c => c.Id == (ItemMastersCentralList.Where(u => u.Number == it.Number).Max(u => u.Id))).Number;
                //char LastChar = sAllCharacter[sAllCharacter.Length - 1];
                //char nextChar = (char)((int)LastChar + 1);

                it.UnitPrice = itemmaster.UnitPrice;
                it.itemBaseName = centralitem.itemBaseName;
                it.itemname = itemmaster.ItemName;
                it.SellingUnitName = it.itemname + " " + it.MinOrderQty + "Unit";//item selling unit name
                it.PurchaseUnitName = it.itemname + " " + it.PurchaseMinOrderQty + "Unit";//
                it.SellingSku = it.ItemNumber;// + nextChar;
                it.PurchaseSku = it.ItemNumber + 'P';
                it.price = MultiMRPitem.MRP;
                it.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                addItem.Add(it);
            }
            context.itemMasters.AddRange(addItem);

            if (context.Commit() > 0) { return addItem; } else { return addItem; };
        }
        #endregion

    }
}
