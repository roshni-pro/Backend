using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.NonRevenueOrderDc;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.NonRevenueOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class NonRevenueOrderHelper
    {
        public DamageOrderMaster CreateNonRevenueOrder(List<DamageOrderCartDc> AddOrder, int UserId)
        {

            using (var context = new AuthContext())
            {
                DamageOrderMaster objOrderMaster = new DamageOrderMaster();
                List<DamageOrderDetails> Obj = new List<DamageOrderDetails>();
                objOrderMaster.DamageorderDetails = Obj;
                int Customerid = AddOrder[0].CustomerId;

                var customer = context.Customers.Where(c => c.CustomerId == Customerid && c.Active == true).FirstOrDefault();
                var people = context.Peoples.Where(c => c.PeopleID == UserId && c.Active == true).FirstOrDefault();

                if (AddOrder != null && AddOrder.Count() > 0 && UserId > 0 && people.PeopleID > 0 && customer.CustomerId > 0)
                {
                    List<int> StockIds = new List<int>();

                    int warehouseid = AddOrder[0].Warehouseid;
                    int OrderType = 2; //2 mean nonreveunev

                    StockIds = AddOrder.Select(x => x.NonRevenueStockId).Distinct().ToList();
                    var LongStockIds = StockIds.ConvertAll(x => (long)x);

                    TextFileLogHelper.LogError("Error NROrder LongStockIds" + LongStockIds);
                    
                    var NROStockIds = context.NonRevenueOrderStocks.Where(x => LongStockIds.Contains(x.Id) && x.NonRevenueInventory > 0).ToList();

                    if (customer != null && warehouseid > 0)
                    {
                        objOrderMaster.Status = "Pending";
                        objOrderMaster.CustomerName = customer.Name;
                        objOrderMaster.CompanyId = 1;
                        objOrderMaster.CustomerCategoryName = customer.CustomerCategoryName;
                        objOrderMaster.CustomerCategoryId = 2;
                        objOrderMaster.CustomerType = customer.CustomerType;
                        objOrderMaster.Customerphonenum = customer.Mobile;
                        objOrderMaster.ClusterName = customer.ClusterName;
                        objOrderMaster.ClusterId = Convert.ToInt32(customer.ClusterId);
                        objOrderMaster.CityId = customer.Cityid;
                        objOrderMaster.ShopName = customer.ShopName;
                        objOrderMaster.Skcode = customer.Skcode;
                        objOrderMaster.Tin_No = customer.RefNo;
                        objOrderMaster.invoice_no = "";
                        objOrderMaster.WarehouseId = customer.Warehouseid ?? 0;
                        objOrderMaster.CustomerId = customer.CustomerId;
                        objOrderMaster.BillingAddress = customer.ShippingAddress;
                        objOrderMaster.ShippingAddress = customer.ShippingAddress;
                        objOrderMaster.OrderTypes = OrderType;
                        objOrderMaster.ShortReason = AddOrder[0].Reason;
                        objOrderMaster.comments = AddOrder[0].Comments;
                        objOrderMaster.Createby = UserId;
                        objOrderMaster.CreatedDate = DateTime.Now;
                        objOrderMaster.UpdatedDate = DateTime.Now;
                        objOrderMaster.active = true;
                        objOrderMaster.Deleted = false;


                        foreach (var i in AddOrder.Where(x => x.qty > 0).Distinct().ToList())
                        {
                            var stock = NROStockIds.Where(x => x.Id == i.NonRevenueStockId).FirstOrDefault();
                            var items = context.itemMasters.Where(x => x.Number == i.ItemNumber && x.WarehouseId== warehouseid).FirstOrDefault();
                            if (i.qty > 0 && stock.NonRevenueInventory > 0 && i.qty <= stock.NonRevenueInventory && items != null)
                            {
                                DamageOrderDetails OrderDetail = new DamageOrderDetails();
                                OrderDetail.CustomerId = i.CustomerId;
                                OrderDetail.CustomerName = customer.Name;
                                OrderDetail.CompanyId = 1;
                                OrderDetail.Mobile = customer.Mobile;
                                OrderDetail.WarehouseId = customer.Warehouseid ?? 0;
                                OrderDetail.City = customer.City;
                                OrderDetail.SellingSku = items.SellingSku;
                                OrderDetail.ItemId = items.ItemId;
                                OrderDetail.itemcode = items.itemcode;
                                OrderDetail.Itempic = items.LogoUrl;
                                OrderDetail.itemNumber = i.ItemNumber;
                                OrderDetail.itemname = items.itemname;
                                OrderDetail.Barcode = items.itemcode;
                                OrderDetail.price = stock.MRP;
                                OrderDetail.UnitPrice = stock.UnitPrice;
                                OrderDetail.DefaultUnitPrice = i.DefaultUnitPrice > 0 ? i.DefaultUnitPrice : stock.UnitPrice;
                                OrderDetail.OrderDate = DateTime.Now;
                                OrderDetail.NetPurchasePrice = 0;
                                OrderDetail.MinOrderQty = 1;
                                OrderDetail.MinOrderQtyPrice = 0;
                                OrderDetail.qty = i.qty;
                                OrderDetail.Noqty = i.qty;
                                OrderDetail.TotalAmt = 0;
                                OrderDetail.Status = "Pending";
                                OrderDetail.ItemMultiMRPId = stock.ItemMultiMRPId;
                                OrderDetail.StockBatchMasterId = i.StockBatchMasterId;
                                OrderDetail.ABCClassification = i.ABCClassification;
                                OrderDetail.CreatedDate = DateTime.Now;
                                OrderDetail.UpdatedDate = DateTime.Now;

                                OrderDetail.ABCClassification = i.ABCClassification;
                                objOrderMaster.DamageorderDetails.Add(OrderDetail);

                            }
                        }

                        context.DamageOrderMasterDB.Add(objOrderMaster);
                        if (context.Commit() > 0)
                        {
                            return objOrderMaster;
                        }
                    }
                }
                return objOrderMaster;
            }

        }
    }
}