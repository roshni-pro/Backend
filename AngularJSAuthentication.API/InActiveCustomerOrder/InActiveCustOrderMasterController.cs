using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Base.Audit;
using GenricEcommers.Models;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AngularJSAuthentication.DataLayer.Repositories.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using System.Data.Entity.Core.Objects;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/InActiveCustOrderMaster")]
    public class InActiveCustOrderMasterController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        [HttpPost]
        public PaggingDataInActive Get(int list, int page, List<int> WarehouseIds)
        {

            using (AuthContext context = new AuthContext())
            {
                List<InActiveCustomerOrderMaster> newdata = new List<InActiveCustomerOrderMaster>();
                var listOrders = new List<InActiveCustomerOrderMaster>(); //context.InActiveCustomerOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();

                //var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId && (x.Status == "Inactive" || x.Status == "Dummy Order Cancelled")).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();

                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                int skip = (page - 1) * list;
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId) && (x.Status == "Inactive" || x.Status == "Dummy Order Cancelled"));
                int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                var orderMasters = new List<MongoOrderMaster>();

                orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), skip, list, collectionName: "OrderMaster").ToList();


                if (listOrders == null || !listOrders.Any())
                    listOrders = new List<InActiveCustomerOrderMaster>();
                DateTime? InactivecustCountDate = context.Companies.Where(x => x.Id == 1).Select(x => x.InActiveCustomerCountDate).FirstOrDefault();

                listOrders.AddRange(orderMasters.Select(x => new InActiveCustomerOrderMaster
                {
                    orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                    {
                        OrderDetailsId = z.OrderDetailsId,
                        OrderId = z.OrderId,
                        CustomerId = z.CustomerId,
                        CustomerName = z.CustomerName,
                        City = z.City,
                        Mobile = z.Mobile,
                        OrderDate = z.OrderDate,
                        CompanyId = z.CompanyId,
                        CityId = z.CityId,
                        WarehouseId = z.WarehouseId,
                        WarehouseName = z.WarehouseName,
                        CategoryName = z.CategoryName,
                        SubcategoryName = z.SubcategoryName,
                        SubsubcategoryName = z.SubsubcategoryName,
                        SellingSku = z.SellingSku,
                        ItemId = z.ItemId,
                        Itempic = z.Itempic,
                        itemname = z.itemname,
                        SellingUnitName = z.SellingUnitName,
                        itemcode = z.itemcode,
                        itemNumber = z.itemNumber,
                        HSNCode = z.HSNCode,
                        Barcode = z.Barcode,
                        price = z.price,
                        UnitPrice = z.UnitPrice,
                        Purchaseprice = z.Purchaseprice,
                        MinOrderQty = z.MinOrderQty,
                        MinOrderQtyPrice = z.MinOrderQtyPrice,
                        qty = z.qty,
                        Noqty = z.Noqty,
                        AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                        AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                        TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                        NetAmmount = z.NetAmmount,
                        DiscountPercentage = z.DiscountPercentage,
                        DiscountAmmount = z.DiscountAmmount,
                        NetAmtAfterDis = z.NetAmtAfterDis,
                        TaxPercentage = z.TaxPercentage,
                        TaxAmmount = z.TaxAmmount,
                        SGSTTaxPercentage = z.SGSTTaxPercentage,
                        SGSTTaxAmmount = z.SGSTTaxAmmount,
                        CGSTTaxPercentage = z.CGSTTaxPercentage,
                        CGSTTaxAmmount = z.CGSTTaxAmmount,
                        TotalAmt = z.TotalAmt,
                        CreatedDate = z.CreatedDate,
                        UpdatedDate = z.UpdatedDate,
                        Deleted = z.Deleted,
                        Status = z.Status,
                        SizePerUnit = z.SizePerUnit,
                        marginPoint = z.marginPoint,
                        promoPoint = z.promoPoint,
                        NetPurchasePrice = z.NetPurchasePrice,
                        ItemMultiMRPId = z.ItemMultiMRPId,


                    }).ToList(),
                    OrderId = x.OrderId,
                    CompanyId = x.CompanyId,
                    SalesPersonId = x.SalesPersonId,
                    SalesPerson = x.SalesPerson,
                    SalesMobile = x.SalesMobile,
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Skcode = x.Skcode,
                    ShopName = x.ShopName,
                    Status = x.Status,
                    invoice_no = x.invoice_no,
                    Trupay = x.Trupay,
                    LandMark = x.LandMark,
                    Customerphonenum = x.Customerphonenum,
                    BillingAddress = x.BillingAddress,
                    ShippingAddress = x.ShippingAddress,
                    TotalAmount = x.TotalAmount,
                    GrossAmount = x.GrossAmount,
                    DiscountAmount = x.DiscountAmount,
                    TaxAmount = x.TaxAmount,
                    SGSTTaxAmmount = x.SGSTTaxAmmount,
                    CGSTTaxAmmount = x.CGSTTaxAmmount,
                    CityId = x.CityId,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.WarehouseName,
                    active = x.active,
                    CreatedDate = x.CreatedDate,
                    Deliverydate = x.Deliverydate,
                    UpdatedDate = x.UpdatedDate,
                    Deleted = x.Deleted,
                    ReasonCancle = x.ReasonCancle,
                    ClusterId = x.ClusterId,
                    ClusterName = x.ClusterName,
                    deliveryCharge = x.deliveryCharge,
                    WalletAmount = x.WalletAmount,
                    walletPointUsed = x.walletPointUsed,
                    UsedPoint = x.UsedPoint,
                    RewardPoint = x.RewardPoint,
                    ShortAmount = x.ShortAmount,
                    comments = x.comments,
                    OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    Tin_No = x.Tin_No,
                    ShortReason = x.ShortReason,
                    Savingamount = x.Savingamount,
                    OnlineServiceTax = x.OnlineServiceTax,
                    paymentThrough = x.paymentThrough,
                    IsNewOrder = true,
                    PaymentMode = x.paymentMode,
                    OrderType = x.OrderType
                }));
                if (listOrders.Any())

                    foreach (var listorder in listOrders)
                    {
                        foreach (var order in orderMasters)
                        {
                            if (order.OrderId == listorder.OrderId)
                            {

                                listorder.InactiveCustOrderCount = context.DbOrderMaster.Any(p => p.CustomerId == order.CustomerId && p.CreatedDate >= InactivecustCountDate) ? context.DbOrderMaster.Where(p => p.CustomerId == order.CustomerId && p.CreatedDate >= InactivecustCountDate && p.CreatedDate <= order.CreatedDate).Count() : 0;
                            }

                        }


                    }
                newdata = listOrders.OrderByDescending(x => x.OrderId).ToList();
                PaggingDataInActive obj = new PaggingDataInActive();


                obj.total_count = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                obj.total_count += context.InActiveCustomerOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId)).Count();
                obj.ordermaster = newdata;
                return obj;
            }

        }
        [Route("Search")]
        [HttpPost]
        public PaggingDataInActive SearchOrders(int list, int page, DateTime? start, DateTime? end, string Skcode, string Mobile, string status, List<int> WarehouseIds) //get search orders for delivery
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    PaggingDataInActive obj = new PaggingDataInActive();
                    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                    int skip = (page - 1) * list;
                    var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Deleted == false && (x.Status == "Inactive" || x.Status == "Dummy Order Cancelled"));
                    var InActiveCustomerOrderPredicate = PredicateBuilder.New<InActiveCustomerOrderMaster>(x => x.Deleted == false);

                    if (WarehouseIds != null && WarehouseIds.Any(x => x > 0))
                    {
                        orderPredicate = orderPredicate.And(x => WarehouseIds.Contains(x.WarehouseId));
                        InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => WarehouseIds.Contains(x.WarehouseId));
                    }
                    if (Skcode != null)
                    {
                        orderPredicate = orderPredicate.And(x => x.Skcode == Skcode);
                        InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => x.Skcode == Skcode);
                    }
                    if (status != null)
                    {
                        orderPredicate = orderPredicate.And(x => x.Status.Equals(status));
                        InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => x.Status.Equals(status));
                    }
                    if (Mobile != null)
                    {
                        orderPredicate = orderPredicate.And(x => x.Customerphonenum.Equals(Mobile));
                        InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => x.Customerphonenum.Equals(Mobile));
                    }
                    //if (start.HasValue && end.HasValue)
                    //    orderPredicate = orderPredicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end));
                    //    InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end));
                    if (start.HasValue && end.HasValue)
                    {
                        orderPredicate = orderPredicate.And(x => x.CreatedDate >= start && x.CreatedDate <= end);
                        InActiveCustomerOrderPredicate = InActiveCustomerOrderPredicate.And(x => x.CreatedDate >= start && x.CreatedDate <= end);
                    }
                    var listOrders = context.InActiveCustomerOrderMasterDB.Where(InActiveCustomerOrderPredicate).Include("orderDetails").ToList();

                    int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                    var orderMasters = new List<MongoOrderMaster>();

                    //orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), skip, list, collectionName: "OrderMaster").ToList();
                      orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), skip, list, collectionName: "OrderMaster").ToList();


                    if (listOrders == null || !listOrders.Any())
                        listOrders = new List<InActiveCustomerOrderMaster>();
                    DateTime? InactivecustCountDate = context.Companies.Where(x => x.Id == 1).Select(x => x.InActiveCustomerCountDate).FirstOrDefault();
                    listOrders.AddRange(orderMasters.Select(x => new InActiveCustomerOrderMaster
                    {

                        orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                        {
                            OrderDetailsId = z.OrderDetailsId,
                            OrderId = z.OrderId,
                            CustomerId = z.CustomerId,
                            CustomerName = z.CustomerName,
                            City = z.City,
                            Mobile = z.Mobile,
                            OrderDate = z.OrderDate,
                            CompanyId = z.CompanyId,
                            CityId = z.CityId,
                            WarehouseId = z.WarehouseId,
                            WarehouseName = z.WarehouseName,
                            CategoryName = z.CategoryName,
                            SubcategoryName = z.SubcategoryName,
                            SubsubcategoryName = z.SubsubcategoryName,
                            SellingSku = z.SellingSku,
                            ItemId = z.ItemId,
                            Itempic = z.Itempic,
                            itemname = z.itemname,
                            SellingUnitName = z.SellingUnitName,
                            itemcode = z.itemcode,
                            itemNumber = z.itemNumber,
                            HSNCode = z.HSNCode,
                            Barcode = z.Barcode,
                            price = z.price,
                            UnitPrice = z.UnitPrice,
                            Purchaseprice = z.Purchaseprice,
                            MinOrderQty = z.MinOrderQty,
                            MinOrderQtyPrice = z.MinOrderQtyPrice,
                            qty = z.qty,
                            Noqty = z.Noqty,
                            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                            NetAmmount = z.NetAmmount,
                            DiscountPercentage = z.DiscountPercentage,
                            DiscountAmmount = z.DiscountAmmount,
                            NetAmtAfterDis = z.NetAmtAfterDis,
                            TaxPercentage = z.TaxPercentage,
                            TaxAmmount = z.TaxAmmount,
                            SGSTTaxPercentage = z.SGSTTaxPercentage,
                            SGSTTaxAmmount = z.SGSTTaxAmmount,
                            CGSTTaxPercentage = z.CGSTTaxPercentage,
                            CGSTTaxAmmount = z.CGSTTaxAmmount,
                            TotalAmt = z.TotalAmt,
                            CreatedDate = z.CreatedDate,
                            UpdatedDate = z.UpdatedDate,
                            Deleted = z.Deleted,
                            Status = z.Status,
                            SizePerUnit = z.SizePerUnit,
                            marginPoint = z.marginPoint,
                            promoPoint = z.promoPoint,
                            NetPurchasePrice = z.NetPurchasePrice,
                            ItemMultiMRPId = z.ItemMultiMRPId,
                        }).ToList(),
                        OrderId = x.OrderId,
                        CompanyId = x.CompanyId,
                        //SalesPersonId = x.SalesPersonId,
                        SalesPerson = string.Join(",", x.orderDetails.Select(z => z.ExecutiveName).Distinct().ToList()),
                        //SalesMobile = x.SalesMobile,
                        CustomerId = x.CustomerId,
                        CustomerName = x.CustomerName,
                        Skcode = x.Skcode,
                        ShopName = x.ShopName,
                        Status = x.Status,
                        invoice_no = x.invoice_no,
                        Trupay = x.Trupay,
                        LandMark = x.LandMark,
                        Customerphonenum = x.Customerphonenum,
                        BillingAddress = x.BillingAddress,
                        ShippingAddress = x.ShippingAddress,
                        TotalAmount = x.TotalAmount,
                        GrossAmount = x.GrossAmount,
                        DiscountAmount = x.DiscountAmount,
                        TaxAmount = x.TaxAmount,
                        SGSTTaxAmmount = x.SGSTTaxAmmount,
                        CGSTTaxAmmount = x.CGSTTaxAmmount,
                        CityId = x.CityId,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName,
                        active = x.active,
                        CreatedDate = x.CreatedDate,
                        Deliverydate = x.Deliverydate,
                        UpdatedDate = x.UpdatedDate,
                        Deleted = x.Deleted,
                        ReasonCancle = x.ReasonCancle,
                        ClusterId = x.ClusterId,
                        ClusterName = x.ClusterName,
                        deliveryCharge = x.deliveryCharge,
                        WalletAmount = x.WalletAmount,
                        walletPointUsed = x.walletPointUsed,
                        UsedPoint = x.UsedPoint,
                        RewardPoint = x.RewardPoint,
                        ShortAmount = x.ShortAmount,
                        comments = x.comments,
                        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                        Tin_No = x.Tin_No,
                        ShortReason = x.ShortReason,
                        Savingamount = x.Savingamount,
                        OnlineServiceTax = x.OnlineServiceTax,
                        paymentThrough = x.paymentThrough,
                        PaymentMode = x.paymentMode,
                        IsNewOrder = true,
                        OrderType = x.OrderType
                        //InactiveCustOrderCount= context.DbOrderMaster.Any(p => p.CustomerId == x.CustomerId && p.CreatedDate >= InactivecustCountDate) ? context.DbOrderMaster.Where(p => p.CustomerId == x.CustomerId && x.CreatedDate >= InactivecustCountDate && x.CreatedDate <= x.CreatedDate).Count() : 0,
                    }));

                    if (listOrders.Any())

                        foreach (var listorder in listOrders)
                        {
                            foreach (var order in orderMasters)
                            {
                                if (order.OrderId == listorder.OrderId)
                                {

                                    listorder.InactiveCustOrderCount = context.DbOrderMaster.Any(p => p.CustomerId == order.CustomerId && p.CreatedDate >= InactivecustCountDate) ? context.DbOrderMaster.Where(p => p.CustomerId == order.CustomerId && p.CreatedDate >= InactivecustCountDate && p.CreatedDate <= order.CreatedDate).Count() : 0;
                                }
                            }
                        }
                    //listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();
                    listOrders = listOrders.OrderByDescending(x => x.CreatedDate).ToList();
                    //foreach (var order in listOrders)
                    //{

                    //    order.InactiveCustOrderCount = listOrders.Any(p => p.CustomerId == order.CustomerId && p.CreatedDate >= InactivecustCountDate) ? listOrders.Where(y => y.CustomerId == order.CustomerId && y.CreatedDate >= InactivecustCountDate && y.CreatedDate <= order.CreatedDate).Count() : 0;
                    //}

                    //foreach (var order in listOrders)
                    //{

                    //    order.inactivecustordercount = listOrders.any(p => p.customerid == order.customerid && p.createddate >= inactivecustcountdate) ? listOrders.where(x => x.customerid == order.customerid && x.createddate >= inactivecustcountdate && x.createddate <= order.createddate).count() : 0;

                    //}

                    obj.total_count = dataCount;
                    obj.total_count += context.InActiveCustomerOrderMasterDB.Where(InActiveCustomerOrderPredicate).Count();
                    obj.ordermaster = listOrders;
                    return obj;
                    #region OldCode
                    //if (Skcode != null)
                    //{
                    //    var listOrders = context.InActiveCustomerOrderMasterDB.Where(a => a.Deleted == false && a.WarehouseId == WarehouseId && a.Skcode == Skcode).Include("orderDetails").ToList();
                    //    var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.Skcode == Skcode && (x.Status == "Inactive" || x.Status == "Dummy Order Cancelled")).Include("orderDetails").ToList();

                    //    if (listOrders == null || !listOrders.Any())
                    //        listOrders = new List<InActiveCustomerOrderMaster>();

                    //    listOrders.AddRange(orders.Select(x => new InActiveCustomerOrderMaster
                    //    {
                    //        orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                    //        {
                    //            OrderDetailsId = z.OrderDetailsId,
                    //            OrderId = z.OrderId,
                    //            CustomerId = z.CustomerId,
                    //            CustomerName = z.CustomerName,
                    //            City = z.City,
                    //            Mobile = z.Mobile,
                    //            OrderDate = z.OrderDate,
                    //            CompanyId = z.CompanyId,
                    //            CityId = z.CityId,
                    //            WarehouseId = z.WarehouseId,
                    //            WarehouseName = z.WarehouseName,
                    //            CategoryName = z.CategoryName,
                    //            SubcategoryName = z.SubcategoryName,
                    //            SubsubcategoryName = z.SubsubcategoryName,
                    //            SellingSku = z.SellingSku,
                    //            ItemId = z.ItemId,
                    //            Itempic = z.Itempic,
                    //            itemname = z.itemname,
                    //            SellingUnitName = z.SellingUnitName,
                    //            itemcode = z.itemcode,
                    //            itemNumber = z.itemNumber,
                    //            HSNCode = z.HSNCode,
                    //            Barcode = z.Barcode,
                    //            price = z.price,
                    //            UnitPrice = z.UnitPrice,
                    //            Purchaseprice = z.Purchaseprice,
                    //            MinOrderQty = z.MinOrderQty,
                    //            MinOrderQtyPrice = z.MinOrderQtyPrice,
                    //            qty = z.qty,
                    //            Noqty = z.Noqty,
                    //            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                    //            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                    //            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                    //            NetAmmount = z.NetAmmount,
                    //            DiscountPercentage = z.DiscountPercentage,
                    //            DiscountAmmount = z.DiscountAmmount,
                    //            NetAmtAfterDis = z.NetAmtAfterDis,
                    //            TaxPercentage = z.TaxPercentage,
                    //            TaxAmmount = z.TaxAmmount,
                    //            SGSTTaxPercentage = z.SGSTTaxPercentage,
                    //            SGSTTaxAmmount = z.SGSTTaxAmmount,
                    //            CGSTTaxPercentage = z.CGSTTaxPercentage,
                    //            CGSTTaxAmmount = z.CGSTTaxAmmount,
                    //            TotalAmt = z.TotalAmt,
                    //            CreatedDate = z.CreatedDate,
                    //            UpdatedDate = z.UpdatedDate,
                    //            Deleted = z.Deleted,
                    //            Status = z.Status,
                    //            SizePerUnit = z.SizePerUnit,
                    //            marginPoint = z.marginPoint,
                    //            promoPoint = z.promoPoint,
                    //            NetPurchasePrice = z.NetPurchasePrice,
                    //            ItemMultiMRPId = z.ItemMultiMRPId,
                    //        }).ToList(),
                    //        OrderId = x.OrderId,
                    //        CompanyId = x.CompanyId,
                    //        SalesPersonId = x.SalesPersonId,
                    //        SalesPerson = x.SalesPerson,
                    //        SalesMobile = x.SalesMobile,
                    //        CustomerId = x.CustomerId,
                    //        CustomerName = x.CustomerName,
                    //        Skcode = x.Skcode,
                    //        ShopName = x.ShopName,
                    //        Status = x.Status,
                    //        invoice_no = x.invoice_no,
                    //        Trupay = x.Trupay,
                    //        LandMark = x.LandMark,
                    //        Customerphonenum = x.Customerphonenum,
                    //        BillingAddress = x.BillingAddress,
                    //        ShippingAddress = x.ShippingAddress,
                    //        TotalAmount = x.TotalAmount,
                    //        GrossAmount = x.GrossAmount,
                    //        DiscountAmount = x.DiscountAmount,
                    //        TaxAmount = x.TaxAmount,
                    //        SGSTTaxAmmount = x.SGSTTaxAmmount,
                    //        CGSTTaxAmmount = x.CGSTTaxAmmount,
                    //        CityId = x.CityId,
                    //        WarehouseId = x.WarehouseId,
                    //        WarehouseName = x.WarehouseName,
                    //        active = x.active,
                    //        CreatedDate = x.CreatedDate,
                    //        Deliverydate = x.Deliverydate,
                    //        UpdatedDate = x.UpdatedDate,
                    //        Deleted = x.Deleted,
                    //        ReasonCancle = x.ReasonCancle,
                    //        ClusterId = x.ClusterId,
                    //        ClusterName = x.ClusterName,
                    //        deliveryCharge = x.deliveryCharge,
                    //        WalletAmount = x.WalletAmount,
                    //        walletPointUsed = x.walletPointUsed,
                    //        UsedPoint = x.UsedPoint,
                    //        RewardPoint = x.RewardPoint,
                    //        ShortAmount = x.ShortAmount,
                    //        comments = x.comments,
                    //        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    //        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    //        Tin_No = x.Tin_No,
                    //        ShortReason = x.ShortReason,
                    //        Savingamount = x.Savingamount,
                    //        OnlineServiceTax = x.OnlineServiceTax,
                    //        paymentThrough = x.paymentThrough,
                    //        IsNewOrder = true,
                    //    }));

                    //    if (listOrders.Any())
                    //        listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();

                    //    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                    //}
                    //else if (status != null)
                    //{
                    //    var listOrders = context.InActiveCustomerOrderMasterDB.Where(a => a.Deleted == false && a.WarehouseId == WarehouseId && a.Status.Equals(status)).Include("orderDetails").ToList();
                    //    if (listOrders.Any())
                    //        listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();
                    //    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                    //}
                    //else if (Mobile != null)
                    //{
                    //    var listOrders = context.InActiveCustomerOrderMasterDB.Where(a => a.Deleted == false && a.WarehouseId == WarehouseId && a.Customerphonenum.Equals(Mobile)).Include("orderDetails").ToList();

                    //    var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.Customerphonenum.Equals(Mobile) && x.Status == "Inactive").Include("orderDetails").ToList();

                    //    if (listOrders == null || !listOrders.Any())
                    //        listOrders = new List<InActiveCustomerOrderMaster>();

                    //    listOrders.AddRange(orders.Select(x => new InActiveCustomerOrderMaster
                    //    {
                    //        orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                    //        {
                    //            OrderDetailsId = z.OrderDetailsId,
                    //            OrderId = z.OrderId,
                    //            CustomerId = z.CustomerId,
                    //            CustomerName = z.CustomerName,
                    //            City = z.City,
                    //            Mobile = z.Mobile,
                    //            OrderDate = z.OrderDate,
                    //            CompanyId = z.CompanyId,
                    //            CityId = z.CityId,
                    //            WarehouseId = z.WarehouseId,
                    //            WarehouseName = z.WarehouseName,
                    //            CategoryName = z.CategoryName,
                    //            SubcategoryName = z.SubcategoryName,
                    //            SubsubcategoryName = z.SubsubcategoryName,
                    //            SellingSku = z.SellingSku,
                    //            ItemId = z.ItemId,
                    //            Itempic = z.Itempic,
                    //            itemname = z.itemname,
                    //            SellingUnitName = z.SellingUnitName,
                    //            itemcode = z.itemcode,
                    //            itemNumber = z.itemNumber,
                    //            HSNCode = z.HSNCode,
                    //            Barcode = z.Barcode,
                    //            price = z.price,
                    //            UnitPrice = z.UnitPrice,
                    //            Purchaseprice = z.Purchaseprice,
                    //            MinOrderQty = z.MinOrderQty,
                    //            MinOrderQtyPrice = z.MinOrderQtyPrice,
                    //            qty = z.qty,
                    //            Noqty = z.Noqty,
                    //            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                    //            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                    //            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                    //            NetAmmount = z.NetAmmount,
                    //            DiscountPercentage = z.DiscountPercentage,
                    //            DiscountAmmount = z.DiscountAmmount,
                    //            NetAmtAfterDis = z.NetAmtAfterDis,
                    //            TaxPercentage = z.TaxPercentage,
                    //            TaxAmmount = z.TaxAmmount,
                    //            SGSTTaxPercentage = z.SGSTTaxPercentage,
                    //            SGSTTaxAmmount = z.SGSTTaxAmmount,
                    //            CGSTTaxPercentage = z.CGSTTaxPercentage,
                    //            CGSTTaxAmmount = z.CGSTTaxAmmount,
                    //            TotalAmt = z.TotalAmt,
                    //            CreatedDate = z.CreatedDate,
                    //            UpdatedDate = z.UpdatedDate,
                    //            Deleted = z.Deleted,
                    //            Status = z.Status,
                    //            SizePerUnit = z.SizePerUnit,
                    //            marginPoint = z.marginPoint,
                    //            promoPoint = z.promoPoint,
                    //            NetPurchasePrice = z.NetPurchasePrice,
                    //            ItemMultiMRPId = z.ItemMultiMRPId,
                    //        }).ToList(),
                    //        OrderId = x.OrderId,
                    //        CompanyId = x.CompanyId,
                    //        SalesPersonId = x.SalesPersonId,
                    //        SalesPerson = x.SalesPerson,
                    //        SalesMobile = x.SalesMobile,
                    //        CustomerId = x.CustomerId,
                    //        CustomerName = x.CustomerName,
                    //        Skcode = x.Skcode,
                    //        ShopName = x.ShopName,
                    //        Status = x.Status,
                    //        invoice_no = x.invoice_no,
                    //        Trupay = x.Trupay,
                    //        LandMark = x.LandMark,
                    //        Customerphonenum = x.Customerphonenum,
                    //        BillingAddress = x.BillingAddress,
                    //        ShippingAddress = x.ShippingAddress,
                    //        TotalAmount = x.TotalAmount,
                    //        GrossAmount = x.GrossAmount,
                    //        DiscountAmount = x.DiscountAmount,
                    //        TaxAmount = x.TaxAmount,
                    //        SGSTTaxAmmount = x.SGSTTaxAmmount,
                    //        CGSTTaxAmmount = x.CGSTTaxAmmount,
                    //        CityId = x.CityId,
                    //        WarehouseId = x.WarehouseId,
                    //        WarehouseName = x.WarehouseName,
                    //        active = x.active,
                    //        CreatedDate = x.CreatedDate,
                    //        Deliverydate = x.Deliverydate,
                    //        UpdatedDate = x.UpdatedDate,
                    //        Deleted = x.Deleted,
                    //        ReasonCancle = x.ReasonCancle,
                    //        ClusterId = x.ClusterId,
                    //        ClusterName = x.ClusterName,
                    //        deliveryCharge = x.deliveryCharge,
                    //        WalletAmount = x.WalletAmount,
                    //        walletPointUsed = x.walletPointUsed,
                    //        UsedPoint = x.UsedPoint,
                    //        RewardPoint = x.RewardPoint,
                    //        ShortAmount = x.ShortAmount,
                    //        comments = x.comments,
                    //        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    //        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    //        Tin_No = x.Tin_No,
                    //        ShortReason = x.ShortReason,
                    //        Savingamount = x.Savingamount,
                    //        OnlineServiceTax = x.OnlineServiceTax,
                    //        paymentThrough = x.paymentThrough,
                    //        IsNewOrder = true,
                    //    }));
                    //    if (listOrders.Any())
                    //        listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();

                    //    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                    //}
                    //else if (WarehouseId > 0)
                    //{
                    //    var listOrders = context.InActiveCustomerOrderMasterDB.Where(a => a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.WarehouseId == WarehouseId).Include("orderDetails").ToList();

                    //    var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.Status == "Inactive").Include("orderDetails").ToList();

                    //    if (listOrders == null || !listOrders.Any())
                    //        listOrders = new List<InActiveCustomerOrderMaster>();

                    //    listOrders.AddRange(orders.Select(x => new InActiveCustomerOrderMaster
                    //    {
                    //        orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                    //        {
                    //            OrderDetailsId = z.OrderDetailsId,
                    //            OrderId = z.OrderId,
                    //            CustomerId = z.CustomerId,
                    //            CustomerName = z.CustomerName,
                    //            City = z.City,
                    //            Mobile = z.Mobile,
                    //            OrderDate = z.OrderDate,
                    //            CompanyId = z.CompanyId,
                    //            CityId = z.CityId,
                    //            WarehouseId = z.WarehouseId,
                    //            WarehouseName = z.WarehouseName,
                    //            CategoryName = z.CategoryName,
                    //            SubcategoryName = z.SubcategoryName,
                    //            SubsubcategoryName = z.SubsubcategoryName,
                    //            SellingSku = z.SellingSku,
                    //            ItemId = z.ItemId,
                    //            Itempic = z.Itempic,
                    //            itemname = z.itemname,
                    //            SellingUnitName = z.SellingUnitName,
                    //            itemcode = z.itemcode,
                    //            itemNumber = z.itemNumber,
                    //            HSNCode = z.HSNCode,
                    //            Barcode = z.Barcode,
                    //            price = z.price,
                    //            UnitPrice = z.UnitPrice,
                    //            Purchaseprice = z.Purchaseprice,
                    //            MinOrderQty = z.MinOrderQty,
                    //            MinOrderQtyPrice = z.MinOrderQtyPrice,
                    //            qty = z.qty,
                    //            Noqty = z.Noqty,
                    //            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                    //            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                    //            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                    //            NetAmmount = z.NetAmmount,
                    //            DiscountPercentage = z.DiscountPercentage,
                    //            DiscountAmmount = z.DiscountAmmount,
                    //            NetAmtAfterDis = z.NetAmtAfterDis,
                    //            TaxPercentage = z.TaxPercentage,
                    //            TaxAmmount = z.TaxAmmount,
                    //            SGSTTaxPercentage = z.SGSTTaxPercentage,
                    //            SGSTTaxAmmount = z.SGSTTaxAmmount,
                    //            CGSTTaxPercentage = z.CGSTTaxPercentage,
                    //            CGSTTaxAmmount = z.CGSTTaxAmmount,
                    //            TotalAmt = z.TotalAmt,
                    //            CreatedDate = z.CreatedDate,
                    //            UpdatedDate = z.UpdatedDate,
                    //            Deleted = z.Deleted,
                    //            Status = z.Status,
                    //            SizePerUnit = z.SizePerUnit,
                    //            marginPoint = z.marginPoint,
                    //            promoPoint = z.promoPoint,
                    //            NetPurchasePrice = z.NetPurchasePrice,
                    //            ItemMultiMRPId = z.ItemMultiMRPId,
                    //        }).ToList(),
                    //        OrderId = x.OrderId,
                    //        CompanyId = x.CompanyId,
                    //        SalesPersonId = x.SalesPersonId,
                    //        SalesPerson = x.SalesPerson,
                    //        SalesMobile = x.SalesMobile,
                    //        CustomerId = x.CustomerId,
                    //        CustomerName = x.CustomerName,
                    //        Skcode = x.Skcode,
                    //        ShopName = x.ShopName,
                    //        Status = x.Status,
                    //        invoice_no = x.invoice_no,
                    //        Trupay = x.Trupay,
                    //        LandMark = x.LandMark,
                    //        Customerphonenum = x.Customerphonenum,
                    //        BillingAddress = x.BillingAddress,
                    //        ShippingAddress = x.ShippingAddress,
                    //        TotalAmount = x.TotalAmount,
                    //        GrossAmount = x.GrossAmount,
                    //        DiscountAmount = x.DiscountAmount,
                    //        TaxAmount = x.TaxAmount,
                    //        SGSTTaxAmmount = x.SGSTTaxAmmount,
                    //        CGSTTaxAmmount = x.CGSTTaxAmmount,
                    //        CityId = x.CityId,
                    //        WarehouseId = x.WarehouseId,
                    //        WarehouseName = x.WarehouseName,
                    //        active = x.active,
                    //        CreatedDate = x.CreatedDate,
                    //        Deliverydate = x.Deliverydate,
                    //        UpdatedDate = x.UpdatedDate,
                    //        Deleted = x.Deleted,
                    //        ReasonCancle = x.ReasonCancle,
                    //        ClusterId = x.ClusterId,
                    //        ClusterName = x.ClusterName,
                    //        deliveryCharge = x.deliveryCharge,
                    //        WalletAmount = x.WalletAmount,
                    //        walletPointUsed = x.walletPointUsed,
                    //        UsedPoint = x.UsedPoint,
                    //        RewardPoint = x.RewardPoint,
                    //        ShortAmount = x.ShortAmount,
                    //        comments = x.comments,
                    //        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    //        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    //        Tin_No = x.Tin_No,
                    //        ShortReason = x.ShortReason,
                    //        Savingamount = x.Savingamount,
                    //        OnlineServiceTax = x.OnlineServiceTax,
                    //        paymentThrough = x.paymentThrough,
                    //        IsNewOrder = true,
                    //    }));
                    //    if (listOrders.Any())
                    //        listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();
                    //    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                    //}
                    //else
                    //{
                    //    var listOrders = context.InActiveCustomerOrderMasterDB.Where(a => a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end).Include("orderDetails").ToList();

                    //    var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.CreatedDate >= start && x.CreatedDate <= end && x.Status == "Inactive").Include("orderDetails").ToList();

                    //    if (listOrders == null || !listOrders.Any())
                    //        listOrders = new List<InActiveCustomerOrderMaster>();

                    //    listOrders.AddRange(orders.Select(x => new InActiveCustomerOrderMaster
                    //    {
                    //        orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                    //        {
                    //            OrderDetailsId = z.OrderDetailsId,
                    //            OrderId = z.OrderId,
                    //            CustomerId = z.CustomerId,
                    //            CustomerName = z.CustomerName,
                    //            City = z.City,
                    //            Mobile = z.Mobile,
                    //            OrderDate = z.OrderDate,
                    //            CompanyId = z.CompanyId,
                    //            CityId = z.CityId,
                    //            WarehouseId = z.WarehouseId,
                    //            WarehouseName = z.WarehouseName,
                    //            CategoryName = z.CategoryName,
                    //            SubcategoryName = z.SubcategoryName,
                    //            SubsubcategoryName = z.SubsubcategoryName,
                    //            SellingSku = z.SellingSku,
                    //            ItemId = z.ItemId,
                    //            Itempic = z.Itempic,
                    //            itemname = z.itemname,
                    //            SellingUnitName = z.SellingUnitName,
                    //            itemcode = z.itemcode,
                    //            itemNumber = z.itemNumber,
                    //            HSNCode = z.HSNCode,
                    //            Barcode = z.Barcode,
                    //            price = z.price,
                    //            UnitPrice = z.UnitPrice,
                    //            Purchaseprice = z.Purchaseprice,
                    //            MinOrderQty = z.MinOrderQty,
                    //            MinOrderQtyPrice = z.MinOrderQtyPrice,
                    //            qty = z.qty,
                    //            Noqty = z.Noqty,
                    //            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                    //            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                    //            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                    //            NetAmmount = z.NetAmmount,
                    //            DiscountPercentage = z.DiscountPercentage,
                    //            DiscountAmmount = z.DiscountAmmount,
                    //            NetAmtAfterDis = z.NetAmtAfterDis,
                    //            TaxPercentage = z.TaxPercentage,
                    //            TaxAmmount = z.TaxAmmount,
                    //            SGSTTaxPercentage = z.SGSTTaxPercentage,
                    //            SGSTTaxAmmount = z.SGSTTaxAmmount,
                    //            CGSTTaxPercentage = z.CGSTTaxPercentage,
                    //            CGSTTaxAmmount = z.CGSTTaxAmmount,
                    //            TotalAmt = z.TotalAmt,
                    //            CreatedDate = z.CreatedDate,
                    //            UpdatedDate = z.UpdatedDate,
                    //            Deleted = z.Deleted,
                    //            Status = z.Status,
                    //            SizePerUnit = z.SizePerUnit,
                    //            marginPoint = z.marginPoint,
                    //            promoPoint = z.promoPoint,
                    //            NetPurchasePrice = z.NetPurchasePrice,
                    //            ItemMultiMRPId = z.ItemMultiMRPId,
                    //        }).ToList(),
                    //        OrderId = x.OrderId,
                    //        CompanyId = x.CompanyId,
                    //        SalesPersonId = x.SalesPersonId,
                    //        SalesPerson = x.SalesPerson,
                    //        SalesMobile = x.SalesMobile,
                    //        CustomerId = x.CustomerId,
                    //        CustomerName = x.CustomerName,
                    //        Skcode = x.Skcode,
                    //        ShopName = x.ShopName,
                    //        Status = x.Status,
                    //        invoice_no = x.invoice_no,
                    //        Trupay = x.Trupay,
                    //        LandMark = x.LandMark,
                    //        Customerphonenum = x.Customerphonenum,
                    //        BillingAddress = x.BillingAddress,
                    //        ShippingAddress = x.ShippingAddress,
                    //        TotalAmount = x.TotalAmount,
                    //        GrossAmount = x.GrossAmount,
                    //        DiscountAmount = x.DiscountAmount,
                    //        TaxAmount = x.TaxAmount,
                    //        SGSTTaxAmmount = x.SGSTTaxAmmount,
                    //        CGSTTaxAmmount = x.CGSTTaxAmmount,
                    //        CityId = x.CityId,
                    //        WarehouseId = x.WarehouseId,
                    //        WarehouseName = x.WarehouseName,
                    //        active = x.active,
                    //        CreatedDate = x.CreatedDate,
                    //        Deliverydate = x.Deliverydate,
                    //        UpdatedDate = x.UpdatedDate,
                    //        Deleted = x.Deleted,
                    //        ReasonCancle = x.ReasonCancle,
                    //        ClusterId = x.ClusterId,
                    //        ClusterName = x.ClusterName,
                    //        deliveryCharge = x.deliveryCharge,
                    //        WalletAmount = x.WalletAmount,
                    //        walletPointUsed = x.walletPointUsed,
                    //        UsedPoint = x.UsedPoint,
                    //        RewardPoint = x.RewardPoint,
                    //        ShortAmount = x.ShortAmount,
                    //        comments = x.comments,
                    //        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    //        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    //        Tin_No = x.Tin_No,
                    //        ShortReason = x.ShortReason,
                    //        Savingamount = x.Savingamount,
                    //        OnlineServiceTax = x.OnlineServiceTax,
                    //        paymentThrough = x.paymentThrough,
                    //        IsNewOrder = true,
                    //    }));
                    //    if (listOrders.Any())
                    //        listOrders = listOrders.OrderByDescending(x => x.OrderId).ToList();

                    //    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                    //}
                    #endregion
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [Route("")]
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync(int id)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var ass = context.InActiveCustomerOrderMasterDB.Where(x => x.OrderId == id).Include("orderDetails").FirstOrDefault();

                    if (ass == null)
                    {

                        var x = context.DbOrderMaster.Where(y => y.OrderId == id).Include("orderDetails").FirstOrDefault();

                        //var manager = new ItemLedgerManager();
                        //List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                        //foreach (var data in x.orderDetails)
                        //{
                        //    ItemClassificationDC obj = new ItemClassificationDC();
                        //    obj.WarehouseId = data.WarehouseId;
                        //    obj.ItemNumber = data.itemNumber;
                        //    objItemClassificationDClist.Add(obj);

                        //}
                        //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);


                        List<int> itemIds = x.orderDetails.Select(xy => xy.ItemId).ToList();
                        List<ItemMaster> ObjItemMaster = context.itemMasters.Where(y => itemIds.Contains(y.ItemId)).ToList();
                        ass = new InActiveCustomerOrderMaster
                        {
                            orderDetails = x.orderDetails.Select(z => new InActiveCustomerOrderDetails
                            {
                                OrderDetailsId = z.OrderDetailsId,
                                OrderId = z.OrderId,
                                CustomerId = z.CustomerId,
                                CustomerName = z.CustomerName,
                                City = z.City,
                                Mobile = z.Mobile,
                                OrderDate = z.OrderDate,
                                CompanyId = z.CompanyId,
                                CityId = z.CityId,
                                WarehouseId = z.WarehouseId,
                                WarehouseName = z.WarehouseName,
                                CategoryName = z.CategoryName,
                                SubcategoryName = z.SubcategoryName,
                                SubsubcategoryName = z.SubsubcategoryName,
                                SellingSku = z.SellingSku,
                                ItemId = z.ItemId,
                                Itempic = z.Itempic,
                                itemname = z.itemname,
                                SellingUnitName = z.SellingUnitName,
                                itemcode = z.itemcode,
                                itemNumber = z.itemNumber,
                                HSNCode = z.HSNCode,
                                Barcode = z.Barcode,
                                price = z.price,
                                UnitPrice = z.UnitPrice,
                                Purchaseprice = z.Purchaseprice,
                                MinOrderQty = z.MinOrderQty,
                                MinOrderQtyPrice = z.MinOrderQtyPrice,
                                qty = z.qty,
                                Prevqty = z.qty,
                                Noqty = z.Noqty,
                                AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                                PrevAmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                                AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                                PrevAmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                                TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                                PrevTotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                                NetAmmount = z.NetAmmount,
                                OrderQty = z.qty,
                                DiscountPercentage = z.DiscountPercentage,
                                DiscountAmmount = z.DiscountAmmount,
                                NetAmtAfterDis = z.NetAmtAfterDis,
                                TaxPercentage = z.TaxPercentage,
                                TaxAmmount = z.TaxAmmount,
                                PrevTaxAmmount = z.TaxAmmount,
                                SGSTTaxPercentage = z.SGSTTaxPercentage,
                                SGSTTaxAmmount = z.SGSTTaxAmmount,
                                CGSTTaxPercentage = z.CGSTTaxPercentage,
                                CGSTTaxAmmount = z.CGSTTaxAmmount,
                                TotalAmt = z.TotalAmt,
                                PrevTotalAmt = z.TotalAmt,

                                CreatedDate = z.CreatedDate,
                                UpdatedDate = z.UpdatedDate,
                                Deleted = z.Deleted,
                                Status = z.Status,
                                SizePerUnit = z.SizePerUnit,
                                marginPoint = z.marginPoint,
                                promoPoint = z.promoPoint,
                                NetPurchasePrice = z.NetPurchasePrice,
                                ItemMultiMRPId = z.ItemMultiMRPId,
                                ISItemLimit = IsItemLimit(z.ItemMultiMRPId, z.itemNumber, z.WarehouseId),
                                ItemLimitQty = GetItemLimitQty(z.ItemMultiMRPId, ObjItemMaster, z.WarehouseId, z.itemNumber),
                                PaymentMode = x.paymentMode,
                                ItemActive = ISitemActive(ObjItemMaster, z.ItemId),
                                ISEditable = ISEditable(z.ItemMultiMRPId, z.itemNumber, z.WarehouseId, x.paymentMode, x.OrderId),
                                //Cateogry = _objItemClassificationDClist.Where(y=>y.ItemNumber==z.itemNumber).Select(y=>y.Category).FirstOrDefault(),
                                Cateogry = z.ABCClassification,
                                IsFreeItem = z.IsFreeItem
                            }).ToList(),
                            OrderId = x.OrderId,
                            CompanyId = x.CompanyId,
                            //SalesPersonId = x.SalesPersonId,
                            SalesPerson = string.Join(",", x.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct()),
                            //SalesMobile = x.SalesMobile,
                            CustomerId = x.CustomerId,
                            CustomerName = x.CustomerName,
                            Skcode = x.Skcode,
                            ShopName = x.ShopName,
                            Status = x.Status,
                            invoice_no = x.invoice_no,
                            Trupay = x.Trupay,
                            LandMark = x.LandMark,
                            Customerphonenum = x.Customerphonenum,
                            BillingAddress = x.BillingAddress,
                            ShippingAddress = x.ShippingAddress,
                            TotalAmount = x.TotalAmount,
                            GrossAmount = x.GrossAmount,
                            DiscountAmount = x.DiscountAmount,
                            TaxAmount = x.TaxAmount,
                            SGSTTaxAmmount = x.SGSTTaxAmmount,
                            CGSTTaxAmmount = x.CGSTTaxAmmount,
                            CityId = x.CityId,
                            WarehouseId = x.WarehouseId,
                            WarehouseName = x.WarehouseName,
                            active = x.active,
                            CreatedDate = x.CreatedDate,
                            Deliverydate = x.Deliverydate,
                            UpdatedDate = x.UpdatedDate,
                            Deleted = x.Deleted,
                            ReasonCancle = x.ReasonCancle,
                            ClusterId = x.ClusterId,
                            ClusterName = x.ClusterName,
                            deliveryCharge = x.deliveryCharge,
                            WalletAmount = x.WalletAmount,
                            walletPointUsed = x.walletPointUsed,
                            UsedPoint = x.UsedPoint,
                            RewardPoint = x.RewardPoint,
                            ShortAmount = x.ShortAmount,
                            comments = x.comments,
                            OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                            OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                            Tin_No = x.Tin_No,
                            ShortReason = x.ShortReason,
                            Savingamount = x.Savingamount,
                            OnlineServiceTax = x.OnlineServiceTax,
                            paymentThrough = x.paymentThrough,
                            IsNewOrder = true,
                            PaymentMode = x.paymentMode,
                            ISPaymentStatusFailed = ISPaymentStatusFailed(x.OrderId),
                            OrderType = x.OrderType
                        };
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }



            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return Request.CreateResponse(HttpStatusCode.OK, "Null");

            }
        }
        [Route("Customer")]
        [HttpGet]
        public HttpResponseMessage CheckCustomer(int CustomerId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    Customer Cust = context.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();

                    return Request.CreateResponse(HttpStatusCode.OK, Cust);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return Request.CreateResponse(HttpStatusCode.OK, "Null");

            }
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage Post(ShoppingCartForInactiveOrder sc)
        {
            using (AuthContext context = new AuthContext())
            {
                bool IsPaymentFailed = false;
                int PaymentStatus = 0;
                int WarehouseId = Convert.ToInt32(sc.itemDetails[0].WarehouseId);
                Customer cust = context.Customers.Where(c => c.Skcode == sc.Skcode).Select(c => c).FirstOrDefault();
                Warehouse wh = context.Warehouses.Where(c => c.WarehouseId == WarehouseId).Select(c => c).SingleOrDefault();
                List<ItemMaster> ObjItemMaster = context.itemMasters.Where(y => y.WarehouseId == WarehouseId).ToList();

                List<TaxGroup> ObjTaxGroup = context.DbTaxGroup.ToList();
                List<TaxGroupDetails> Objtaxgroupdetails = context.DbTaxGroupDetails.ToList();
                var oldWarehouseId = cust.Warehouseid.Value;

                if (!sc.IsNewOrder)
                {
                    if (sc.status == "Order Confirmed" || sc.status == "Order Confirmed With InTransit")
                    {
                        if (cust.Warehouseid != sc.itemDetails[0].WarehouseId)
                        {
                            cust.Warehouseid = wh.WarehouseId;
                            cust.WarehouseName = wh.WarehouseName;
                            cust.UpdatedDate = indianTime;
                            //context.Customers.Attach(cust);
                            context.Entry(cust).State = EntityState.Modified;
                            context.Commit();
                            try
                            {
                                InActiveCustomerOrderMaster InData = context.InActiveCustomerOrderMasterDB.Where(x => x.Deleted == false && x.OrderId == sc.OrderId).Include("orderDetails").FirstOrDefault();
                                bool PostOrder = context.AddOrderMasterForInactive(sc, oldWarehouseId, InData);
                                if (PostOrder)
                                {
                                    InData.UpdatedDate = indianTime;
                                    InData.Status = sc.status;
                                    context.InActiveCustomerOrderMasterDB.Attach(InData);
                                    context.Entry(InData).State = EntityState.Modified;
                                    context.Commit();
                                }

                                return Request.CreateResponse(HttpStatusCode.OK, true);
                            }
                            catch (Exception es)
                            {
                            }
                        }
                        else
                        {
                            InActiveCustomerOrderMaster InData = context.InActiveCustomerOrderMasterDB.Where(x => x.Deleted == false && x.OrderId == sc.OrderId).Include("orderDetails").FirstOrDefault();
                            bool PostOrder = context.AddOrderMasterForInactive(sc, oldWarehouseId, InData);
                            if (PostOrder)
                            {
                                InData.UpdatedDate = indianTime;
                                InData.Status = sc.status;
                                //context.InActiveCustomerOrderMasterDB.Attach(InData);
                                context.Entry(InData).State = EntityState.Modified;
                                context.Commit();
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, true);
                        }
                    }
                    else
                    {
                        InActiveCustomerOrderMaster InData = context.InActiveCustomerOrderMasterDB.FirstOrDefault(x => x.Deleted == false && x.OrderId == sc.OrderId);
                        InData.UpdatedDate = indianTime;
                        InData.Status = "Dummy Order Cancelled";
                        //context.InActiveCustomerOrderMasterDB.Attach(InData);
                        context.Entry(InData).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                else
                {
                    var order = context.DbOrderMaster.Include("orderDetails").FirstOrDefault(x => x.OrderId == sc.OrderId && x.Status == "Inactive");

                    if (order != null)
                    {
                        if (cust.Warehouseid != sc.itemDetails[0].WarehouseId)
                        {
                            cust.Warehouseid = wh.WarehouseId;
                            cust.WarehouseName = wh.WarehouseName;
                            cust.UpdatedDate = indianTime;
                            //context.Customers.Attach(cust);
                            context.Entry(cust).State = EntityState.Modified;
                            order.WarehouseId = wh.WarehouseId;
                            order.WarehouseName = wh.WarehouseName;

                            var itemMultiMrpIds = order.orderDetails.Select(x => x.ItemMultiMRPId).ToList();
                            var itemNumbers = order.orderDetails.Select(x => x.itemNumber).ToList();

                            var newWhItems = context.itemMasters.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId) && itemNumbers.Contains(x.Number) && x.WarehouseId == cust.Warehouseid).ToList();

                            foreach (var item in order.orderDetails)
                            {
                                var itemMaster = newWhItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.Number == item.itemNumber);
                                item.WarehouseId = wh.WarehouseId;
                                item.ItemId = itemMaster.ItemId;
                                item.UpdatedDate = indianTime;
                                item.CreatedDate = indianTime;
                                item.Status = sc.status == "Order Confirmed With InTransit" ? "InTransit" : "Pending";
                                context.Entry(item).State = EntityState.Modified;
                            }

                        }

                        int i = 0;

                        if (order.paymentMode.ToUpper().Equals("ONLINE") || order.paymentMode == "" || string.IsNullOrEmpty(order.paymentMode))
                        {
                            IsPaymentFailed = ISPaymentStatusFailed(order.OrderId);
                            if (IsPaymentFailed)
                            {
                                order.paymentMode = "COD";
                                PaymentStatus = 1;
                            }
                        }
                        if (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK"))
                        {
                            foreach (var data in order.orderDetails)
                            {
                                bool ItemActive = true;
                                if (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK"))
                                {
                                    ItemActive = ISitemActive(ObjItemMaster, data.ItemId);

                                }
                                bool ISItemLimit = IsItemLimit(data.ItemMultiMRPId, data.itemNumber, WarehouseId);
                                data.ISItemLimit = ISItemLimit;
                                data.Status = sc.status == "Order Confirmed With InTransit" ? "InTransit" : "Pending";
                                if (ISItemLimit && (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK")))
                                {
                                    if (!ItemActive)
                                    {
                                        sc.itemDetails[i].qty = 0;
                                    }

                                    int EditQty = sc.itemDetails[i].qty;
                                    if (data.qty != EditQty)
                                    {
                                        data.qty = EditQty;
                                        data.Noqty = EditQty;
                                    }
                                    int ItemLimitQty = GetItemLimitQty(data.ItemMultiMRPId, ObjItemMaster, WarehouseId, data.itemNumber);
                                    if (data.qty > ItemLimitQty && (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK")))
                                    {
                                        data.qty = ItemLimitQty;
                                        data.Noqty = ItemLimitQty;
                                    }
                                    data.ItemLimitQty = ItemLimitQty;
                                    bool Result = UpdateItemLimit(data.ItemMultiMRPId, WarehouseId, data.qty, data.itemNumber);



                                    #region OldCalculation
                                    //if (Result || !ItemActive)
                                    //{

                                    //    ItemMaster itemMaster = ObjItemMaster.Where(x => x.ItemId == data.ItemId && x.WarehouseId == WarehouseId).Select(x => x).FirstOrDefault();

                                    //    int TaxGroupId = ObjTaxGroup.Where(x => x.GruopID == itemMaster.GruopID).Select(x => x.GruopID).FirstOrDefault();

                                    //    double Taxpercent = Objtaxgroupdetails.Where(x => x.GruopID == TaxGroupId).Select(x => x.TaxgrpDetailID).FirstOrDefault();

                                    //    data.TotalAmt = data.UnitPrice * data.qty;
                                    //    data.TaxPercentage = itemMaster.TotalTaxPercentage;
                                    //    if (data.TaxPercentage >= 0)
                                    //    {
                                    //        data.SGSTTaxPercentage = data.TaxPercentage / 2;
                                    //        data.CGSTTaxPercentage = data.TaxPercentage / 2;
                                    //    }

                                    //    if (itemMaster.TotalCessPercentage > 0)
                                    //    {
                                    //        data.TotalCessPercentage = itemMaster.TotalCessPercentage;
                                    //        double tempPercentagge = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;

                                    //        data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge / 100)) / 100;


                                    //        data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                    //        data.CessTaxAmount = (data.AmtWithoutAfterTaxDisc * data.TotalCessPercentage) / 100;
                                    //    }

                                    //    double tempPercentagge2 = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;

                                    //    data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                    //    data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                    //    data.TaxAmmount = (data.AmtWithoutAfterTaxDisc * data.TaxPercentage) / 100;
                                    //    if (data.TaxAmmount >= 0)
                                    //    {
                                    //        data.SGSTTaxAmmount = data.TaxAmmount / 2;
                                    //        data.CGSTTaxAmmount = data.TaxAmmount / 2;
                                    //    }
                                    //    //for cess
                                    //    if (data.CessTaxAmount > 0)
                                    //    {
                                    //        double tempPercentagge3 = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;
                                    //        //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                    //        data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                    //        data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                    //        data.TotalAmountAfterTaxDisc = data.AmtWithoutAfterTaxDisc + data.CessTaxAmount + data.TaxAmmount;
                                    //    }
                                    //    else
                                    //    {
                                    //        data.TotalAmountAfterTaxDisc = data.AmtWithoutAfterTaxDisc + data.TaxAmmount;
                                    //    }
                                    //    data.DiscountPercentage = 0;// items.PramotionalDiscount;
                                    //    data.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
                                    //    double DiscountAmmount = data.DiscountAmmount;
                                    //    double NetAmtAfterDis = (data.NetAmmount - DiscountAmmount);
                                    //    data.NetAmtAfterDis = (data.NetAmmount - DiscountAmmount);
                                    //    double TaxAmmount = data.TaxAmmount;
                                    //    data.Purchaseprice = itemMaster.PurchasePrice;
                                    //    order.TotalAmount = System.Math.Round(order.orderDetails.Sum(x => x.TotalAmt));
                                    //    order.TaxAmount = System.Math.Round(order.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                                    //    order.SGSTTaxAmmount = System.Math.Round(order.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                                    //    order.CGSTTaxAmmount = System.Math.Round(order.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                                    //    order.GrossAmount = System.Math.Round(order.TotalAmount, 0);



                                    //    var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == WarehouseId && x.isDeleted == false).ToList();
                                    //    double DeliveryAmount = 0;
                                    //    if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount))
                                    //    {
                                    //        DeliveryAmount = Convert.ToDouble(deliveryCharges.FirstOrDefault(x => x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount).del_Charge);
                                    //    }
                                    //    order.deliveryCharge = DeliveryAmount;
                                    //    order.TotalAmount += DeliveryAmount;
                                    //    order.GrossAmount += DeliveryAmount;


                                    //}
                                    //else
                                    //{
                                    //    throw new Exception("Something went wrong please try again later!!");
                                    //}
                                    #endregion

                                }
                                else if (ISItemLimit && (order.paymentMode.ToUpper().Equals("ONLINE") || order.paymentMode.ToUpper().Equals("GULLAK")))
                                {
                                    bool Result = UpdateItemLimit(data.ItemMultiMRPId, WarehouseId, data.qty, data.itemNumber);
                                }
                                else if (!ItemActive)
                                {
                                    data.qty = 0;
                                    data.Noqty = 0;
                                }
                                if (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK"))
                                {
                                    ItemMaster itemMaster = ObjItemMaster.Where(x => x.ItemId == data.ItemId && x.WarehouseId == WarehouseId).Select(x => x).FirstOrDefault();

                                    int TaxGroupId = ObjTaxGroup.Where(x => x.GruopID == itemMaster.GruopID).Select(x => x.GruopID).FirstOrDefault();

                                    double Taxpercent = Objtaxgroupdetails.Where(x => x.GruopID == TaxGroupId).Select(x => x.TaxgrpDetailID).FirstOrDefault();

                                    data.TotalAmt = data.UnitPrice * data.qty;
                                    data.TaxPercentage = itemMaster.TotalTaxPercentage;
                                    if (data.TaxPercentage >= 0)
                                    {
                                        data.SGSTTaxPercentage = data.TaxPercentage / 2;
                                        data.CGSTTaxPercentage = data.TaxPercentage / 2;
                                    }

                                    if (itemMaster.TotalCessPercentage > 0)
                                    {
                                        data.TotalCessPercentage = itemMaster.TotalCessPercentage;
                                        double tempPercentagge = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;

                                        data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge / 100)) / 100;


                                        data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                        data.CessTaxAmount = (data.AmtWithoutAfterTaxDisc * data.TotalCessPercentage) / 100;
                                    }

                                    double tempPercentagge2 = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;

                                    data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                    data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                    data.TaxAmmount = (data.AmtWithoutAfterTaxDisc * data.TaxPercentage) / 100;
                                    if (data.TaxAmmount >= 0)
                                    {
                                        data.SGSTTaxAmmount = data.TaxAmmount / 2;
                                        data.CGSTTaxAmmount = data.TaxAmmount / 2;
                                    }
                                    //for cess
                                    if (data.CessTaxAmount > 0)
                                    {
                                        double tempPercentagge3 = itemMaster.TotalCessPercentage + itemMaster.TotalTaxPercentage;
                                        //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                        data.AmtWithoutTaxDisc = ((100 * data.UnitPrice * data.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                        data.AmtWithoutAfterTaxDisc = (100 * data.AmtWithoutTaxDisc) / (100 + itemMaster.PramotionalDiscount);
                                        data.TotalAmountAfterTaxDisc = data.AmtWithoutAfterTaxDisc + data.CessTaxAmount + data.TaxAmmount;
                                    }
                                    else
                                    {
                                        data.TotalAmountAfterTaxDisc = data.AmtWithoutAfterTaxDisc + data.TaxAmmount;
                                    }
                                    data.DiscountPercentage = 0;// items.PramotionalDiscount;
                                    data.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
                                    double DiscountAmmount = data.DiscountAmmount;
                                    double NetAmtAfterDis = (data.NetAmmount - DiscountAmmount);
                                    data.NetAmtAfterDis = (data.NetAmmount - DiscountAmmount);
                                    double TaxAmmount = data.TaxAmmount;
                                    data.Purchaseprice = itemMaster.PurchasePrice;
                                    data.SavingAmount = (data.price - data.UnitPrice) * data.qty;
                                    order.TotalAmount = System.Math.Round(order.orderDetails.Sum(x => x.TotalAmt));
                                    order.TaxAmount = System.Math.Round(order.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                                    order.SGSTTaxAmmount = System.Math.Round(order.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                                    order.CGSTTaxAmmount = System.Math.Round(order.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                                    order.GrossAmount = System.Math.Round(order.TotalAmount, 0);




                                    i = i + 1;
                                }

                            }

                            double DeliveryAmount = 0;
                            if (order.OrderType != 4)
                            {
                                var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == WarehouseId && x.isDeleted == false).ToList();

                                var storeIds = order.orderDetails.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                                if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount))
                                {
                                    if (storeIds.All(x => x == storeIds.Max(y => y))
                                         && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount)
                                        )
                                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount).Max(x => x.del_Charge));
                                    else
                                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= order.TotalAmount && x.min_Amount <= order.TotalAmount).Max(x => x.del_Charge));

                                }
                            }
                            else
                            {
                                DeliveryAmount = order.deliveryCharge ?? 0;
                            }
                            order.deliveryCharge = DeliveryAmount;
                            order.TotalAmount += DeliveryAmount;
                            order.GrossAmount += DeliveryAmount;
                            if (PaymentStatus == 0)
                            {
                                order.BillDiscountAmount = order.BillDiscountAmount.HasValue ? order.BillDiscountAmount.Value : 0;
                                order.TotalAmount = order.TotalAmount - ((order.BillDiscountAmount ?? 0) + (order.WalletAmount ?? 0));
                                order.GrossAmount = System.Math.Round(order.TotalAmount, 0);
                            }
                            if (!order.paymentMode.ToUpper().Equals("ONLINE") && !order.paymentMode.ToUpper().Equals("GULLAK"))
                            {

                                var AggregateSum = order.orderDetails.GroupBy(x => new { x.itemNumber, x.ISItemLimit, x.ItemLimitQty }).Select(x => new GroupItemLimitDc
                                {
                                    itemNumber = x.Key.itemNumber,
                                    qty = x.Sum(y => y.qty),
                                    ItemLimitQty = x.Key.ItemLimitQty,
                                    ISItemLimit = x.Key.ISItemLimit
                                });
                                if (AggregateSum.Any(x => x.qty > x.ItemLimitQty && x.ISItemLimit == true))
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sum of Quantity of same Moq cannot be greater than ItemLimit Quantity");
                                    //throw new Exception("Sum of Quantity of same Moq cannot be greater than ItemLimit Quantity");
                                }

                                bool Res = UpdatePaymentResponse(order.OrderId, order.GrossAmount, context);
                                if (!Res)
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong please try again later!!");
                                }
                                else
                                {
                                    order.Savingamount = System.Math.Round(order.orderDetails.Sum(x => x.SavingAmount), 2);
                                }

                            }

                            if (PaymentStatus == 1)
                            {
                                order.paymentMode = "COD";
                                order.paymentThrough = "CASH";
                                order.TotalAmount = order.orderDetails.Sum(x => x.TotalAmt);
                                order.GrossAmount = System.Math.Round(order.TotalAmount);
                                order.TaxAmount = order.orderDetails.Sum(x => x.TaxAmmount);
                                order.Savingamount = System.Math.Round(order.orderDetails.Sum(x => x.SavingAmount), 2);
                                order.SGSTTaxAmmount = order.TaxAmount / 2;
                                order.CGSTTaxAmmount = order.TaxAmount / 2;
                                order.SGSTTaxAmmount = System.Math.Round(order.SGSTTaxAmmount, 2);
                                order.CGSTTaxAmmount = System.Math.Round(order.CGSTTaxAmmount, 2);
                                order.DiscountAmount = order.orderDetails.Sum(x => x.DiscountAmmount);

                                PaymentResponseRetailerApp ObjpaymentRetailer = new PaymentResponseRetailerApp();
                                ObjpaymentRetailer.OrderId = order.OrderId;
                                ObjpaymentRetailer.amount = order.GrossAmount;
                                ObjpaymentRetailer.status = "Success";
                                ObjpaymentRetailer.PaymentFrom = "Cash";
                                ObjpaymentRetailer.PaymentThrough = "Offline";
                                ObjpaymentRetailer.CreatedDate = DateTime.Now;
                                ObjpaymentRetailer.UpdatedDate = DateTime.Now;
                                context.PaymentResponseRetailerAppDb.Add(ObjpaymentRetailer);

                            }
                        }
                        else
                        {
                            foreach (var data in order.orderDetails)
                            {
                                bool ISItemLimit = IsItemLimit(data.ItemMultiMRPId, data.itemNumber, WarehouseId);

                                if (ISItemLimit)
                                {
                                    bool Result = UpdateItemLimit(data.ItemMultiMRPId, WarehouseId, data.qty, data.itemNumber);
                                    if (!Result)
                                    {
                                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong please try again later!!");

                                    }

                                }
                            }
                        }
                        order.Status = sc.status == "Order Confirmed" ? "Pending" : sc.status == "Order Confirmed With InTransit" ? "InTransit" : "Dummy Order Cancelled";
                        if (order.GrossAmount <= 0)
                        {
                            order.Status = "Dummy Order Cancelled";
                            foreach (var oitem in order.orderDetails)
                            {
                                oitem.status = order.Status; context.Entry(oitem).State = EntityState.Modified;
                            }
                        }


                        order.UpdatedDate = indianTime;
                        order.CreatedDate = indianTime;
                        order.Deliverydate = indianTime.AddHours(48);
                        //IsCustFirstOrder
                        if (order.Status == "Pending")
                        {
                            if (context.IsCustFirstOrder(order.CustomerId))
                            {
                                order.IsFirstOrder = true;
                            };
                        }
                        context.Entry(order).State = EntityState.Modified;
                    }
                    context.Commit();
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }

        }

        [Route("getInActiveorder")]
        [HttpPost]
        public List<MongoOrderMaster> getInActivedata(DateTime? start, DateTime? end, List<int> WarehouseIds)
        {
            logger.Info("start InActiveCustomerOrderMaster: ");

            List<OrderMaster> ass = new List<OrderMaster>();
            using (var context = new AuthContext())
            {

                if (WarehouseIds != null && WarehouseIds.Any())
                {
                    //var orders = context.DbOrderMaster.Where(x => x.Deleted == false && x.CreatedDate >= start && x.CreatedDate <= end && x.WarehouseId == WarehouseId && (x.Status == "Dummy Order Cancelled" || x.Status == "Inactive" || x.Status == "Order Confirmed")).ToList();
                    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                    var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Deleted == false && x.CreatedDate >= start && x.CreatedDate <= end && WarehouseIds.Contains(x.WarehouseId) && (x.Status == "Dummy Order Cancelled" || x.Status == "Inactive" || x.Status == "Order Confirmed"));
                    int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                    var orderMasters = new List<MongoOrderMaster>();

                    orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), collectionName: "OrderMaster").ToList();
                    orderMasters.ForEach(x => x.SalesPerson = string.Join(",", x.orderDetails.Select(z => z.ExecutiveName).Distinct().ToList()));
                    return orderMasters;
                }
                else
                {
                    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                    var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Deleted == false && x.CreatedDate >= start && x.CreatedDate <= end && (x.Status == "Dummy Order Cancelled" || x.Status == "Inactive" || x.Status == "Order Confirmed"));
                    int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                    var orderMasters = new List<MongoOrderMaster>();

                    orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), collectionName: "OrderMaster").ToList();
                    orderMasters.ForEach(x => x.SalesPerson = string.Join(",", x.orderDetails.Select(z => z.ExecutiveName).Distinct().ToList()));
                    return orderMasters;
                }
                //return orders;

            }
        }


        [Route("getTwoMnthInActiveOrder")]
        [HttpGet]
        public HttpResponseMessage getTwoMnthInActiveOrder()
        {
            try
            {
                DateTime date = DateTime.Now;
                DateTime datestart = date.AddMonths(-2);
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Deleted == false && x.CreatedDate >= datestart && x.CreatedDate <= date && (x.Status == "Dummy Order Cancelled" || x.Status == "Inactive" || x.Status == "Order Confirmed"));
                int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                var orderMasters = new List<MongoOrderMaster>();

                orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), collectionName: "OrderMaster").ToList();

                DataTable dt = ListtoDataTableConverter.ToDataTable(orderMasters);
                List<DataTable> Tables = DatatableConvertIntoChunks(dt);
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "InActiveExcelOrderSixtydays.xls");
                ExcelGenerator.DataTable_To_Excel(Tables, @"" + path);

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, false);
            }
        }
        public List<DataTable> DatatableConvertIntoChunks(DataTable dt)
        {
            List<DataTable> ndt = new List<DataTable>();
            Int64 ExcelRowsLength = 65535;
            Int64 i = 0;
            int j = 1;
            DataTable newDt = dt.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();
            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == ExcelRowsLength)
                {
                    ndt.Add(newDt);
                    j++;
                    newDt = dt.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            if (newDt.Rows.Count > 0)
            {
                ndt.Add(newDt);
                j++;
                newDt = dt.Clone();
                newDt.TableName = "Table_" + j;
                newDt.Clear();

            }
            return ndt;
        }

        [Route("GetOrderCount")]
        [HttpGet]
        public OrderCountInfo GetOrderCount(int OrderId)
        {
            using (var context = new AuthContext())
            {
                var data = (from i in context.Customers
                            join k in context.DbOrderMaster on i.CustomerId equals k.CustomerId
                            join com in context.Companies on i.CompanyId equals com.Id
                            where k.OrderId == OrderId && i.CustomerVerify == "Temporary Active"
                            select new OrderCountInfo
                            {
                                OrderCount = (context.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.CreatedDate >= com.InActiveCustomerCountDate && x.CreatedDate <= k.CreatedDate).Count()),
                                MaxOrderLimit = com.InActiveCustomerCount ?? 0,
                            }).FirstOrDefault();
                return data;
            }

        }

        private bool IsItemLimit(int ItemMultiMrpId, string ItemNumber, int WareHouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                bool Result = false;
                //int ItemId = ObjItemMaster.Where(x => x.ItemNumber == ItemNumber && x.WarehouseId == WareHouseId).Select(x => x.ItemId).FirstOrDefault();
                ItemLimitMaster ObjitemLimitMaster = db.ItemLimitMasterDB.Where(x => x.ItemMultiMRPId == ItemMultiMrpId && x.IsItemLimit == true && x.WarehouseId == WareHouseId && x.ItemNumber == ItemNumber).FirstOrDefault();
                if (ObjitemLimitMaster != null)
                {
                    Result = true;

                }
                return Result;
            }
        }

        private int GetItemLimitQty(int ItemMultiMrpId, List<ItemMaster> ObjItemMaster, int WareHouseid, string ItemNumber)

        {
            using (AuthContext db = new AuthContext())
            {
                //int ItemId = ObjItemMaster.Where(x => x.ItemNumber == ItemNumber && x.WarehouseId == WareHouseid).Select(x => x.ItemId).FirstOrDefault();
                //ItemLimitMaster ObjitemLimitMaster = context.ItemLimitMasterDB.Where(x => x.itemId == ItemId && x.IsItemLimit == true).FirstOrDefault();
                int Qty = db.ItemLimitMasterDB.Where(x => x.ItemMultiMRPId == ItemMultiMrpId && x.WarehouseId == WareHouseid && x.ItemNumber == ItemNumber && x.IsItemLimit == true).Select(x => x.ItemlimitQty).FirstOrDefault();
                return Qty;
            }
        }

        private bool ISitemActive(List<ItemMaster> ObjItemMaster, int ItemId)
        {


            bool ISitemActive = ObjItemMaster.Where(x => x.ItemId == ItemId).Select(x => x.active).FirstOrDefault();

            return ISitemActive;
        }
        private bool UpdateItemLimit(int ItemMultiMRPId, int Warehouseid, int Qty, string ItemNumber)
        {
            bool Result = false;
            bool Res = true;
            using (AuthContext context = new AuthContext())
            {
                var itemLimit = context.ItemLimitMasterDB.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMRPId && x.WarehouseId == Warehouseid && x.ItemNumber == ItemNumber);
                if (itemLimit != null)
                {
                    itemLimit.ItemlimitQty = (itemLimit.ItemlimitQty - Qty);

                    if (itemLimit.ItemlimitQty <= 0)
                    {
                        itemLimit.ItemlimitQty = 0;
                        itemLimit.IsItemLimit = false;
                        Res = DeactiveItem(ItemNumber, Warehouseid);

                    }
                    context.Entry(itemLimit).State = EntityState.Modified;
                    if (Res)
                    {
                        Result = true;
                    }
                }
                return Result;
            }
        }

        private bool DeactiveItem(string ItemNumber, int Warehouseid)
        {
            bool Result = false;
            using (AuthContext context = new AuthContext())
            {
                var data = context.itemMasters.Where(x => x.Number == ItemNumber && x.WarehouseId == Warehouseid).ToList();

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.active = false;
                        context.Entry(item).State = EntityState.Modified;
                    }

                    Result = true;
                }

                return Result;
            }
        }

        private bool ISEditable(int ItemMultiMrpId, string ItemNumber, int WarehouseId, string PaymentMode, int OrderId)
        {
            bool Isedit = false;
            bool IsItemLimitRes = IsItemLimit(ItemMultiMrpId, ItemNumber, WarehouseId);
            bool Status = ISPaymentStatusFailed(OrderId);
            if (IsItemLimitRes)
            {
                if (!PaymentMode.ToUpper().Equals("ONLINE") && !PaymentMode.ToUpper().Equals("GULLAK"))
                {
                    Isedit = true;
                }
                else if (PaymentMode.ToUpper().Equals("ONLINE") && Status)
                {
                    Isedit = true;
                }
            }
            return Isedit;

        }

        private bool UpdatePaymentResponse(int OrderId, double GrossAmount, AuthContext context)
        {
            bool Result = true;
            var data = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == OrderId && x.status == "Success").FirstOrDefault();
            if (data != null)
            {
                if (data.amount != GrossAmount)
                {
                    Result = false;
                    data.status = "Failed";
                    data.UpdatedDate = DateTime.Now;
                    context.Entry(data).State = EntityState.Modified;
                    PaymentResponseRetailerApp ObjpaymentRetailer = new PaymentResponseRetailerApp();
                    ObjpaymentRetailer.OrderId = data.OrderId;
                    ObjpaymentRetailer.amount = GrossAmount;
                    ObjpaymentRetailer.status = "Success";
                    ObjpaymentRetailer.PaymentFrom = data.PaymentFrom;
                    ObjpaymentRetailer.PaymentThrough = data.PaymentThrough;
                    ObjpaymentRetailer.currencyCode = data.currencyCode;
                    ObjpaymentRetailer.statusDesc = data.statusDesc;
                    ObjpaymentRetailer.currencyCode = data.currencyCode;
                    ObjpaymentRetailer.IsOnline = data.PaymentFrom != "Cash" && data.PaymentFrom != "Cheque";
                    ObjpaymentRetailer.CreatedDate = DateTime.Now;
                    ObjpaymentRetailer.UpdatedDate = DateTime.Now;
                    context.PaymentResponseRetailerAppDb.Add(ObjpaymentRetailer);
                    Result = true;
                }
            }
            return Result;
        }
        private bool ISPaymentStatusFailed(int OrderId)
        {
            bool Result = false;
            using (AuthContext context = new AuthContext())
            {
                string Status = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == OrderId).Select(x => x.status).FirstOrDefault();
                if (Status.ToUpper().Equals("FAILED"))
                {
                    Result = true;
                }
                return Result;
            }
        }

        //private bool ISFreeItem(int OrderId, int OrderDetailsId, int CompanyId, int WarehouseID, int ItemId, int CustomerId, string Itemname, int MinOrderQty, int NoOfQty)
        //{
        //    bool Res = true;

        //    List<OfferItem> ObjOfferItemList = new List<OfferItem>();

        //    Res = false;
        //    Offer objoffer = context.OfferDb.Where(x => x.itemId == ItemId && x.FreeItemId == ItemId && x.IsDeleted == false).FirstOrDefault();
        //    OfferItem offer = context.OfferItemDb.Where(x => x.itemId == ItemId && x.OrderId == OrderId).FirstOrDefault();
        //    if (offer == null && objoffer != null)
        //    {
        //        OfferItem ff = new OfferItem();
        //        ff.CompanyId = CompanyId;
        //        ff.WarehouseId = WarehouseID;
        //        ff.itemId = objoffer.itemId;
        //        ff.itemname = objoffer.itemname;
        //        ff.MinOrderQuantity = objoffer.MinOrderQuantity;
        //        ff.NoOffreeQuantity = NoOfQty;
        //        ff.FreeItemId = objoffer.FreeItemId;
        //        ff.FreeItemName = objoffer.itemname;
        //        ff.FreeItemMRP = objoffer.FreeItemMRP;
        //        ff.IsDeleted = false;
        //        ff.CreatedDate = indianTime;
        //        ff.UpdateDate = indianTime;
        //        ff.CustomerId = CustomerId;
        //        ff.OrderId = OrderId;
        //        ff.OfferType = "ItemMaster";
        //        ff.ReferOfferId = objoffer.OfferId;
        //        ObjOfferItemList.Add(ff);
        //    }

        //    if (ObjOfferItemList.Any())
        //    {
        //        Res = true;
        //        context.OfferItemDb.AddRange(ObjOfferItemList);
        //    }

        //    return Res;
        //}

        private async System.Threading.Tasks.Task<List<ItemClassificationDC>> GetItemClassificationsAsync(ICollection<Model.OrderDetails> OrderDetails)
        {
            List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();

            foreach (var data in OrderDetails)
            {
                ItemClassificationDC obj = new ItemClassificationDC();
                obj.WarehouseId = data.WarehouseId;
                obj.ItemNumber = data.itemNumber;
                objItemClassificationDClist.Add(obj);

            }
            var manager = new ItemLedgerManager();
            List<ItemClassificationDC> _Result = await manager.GetItemClassificationsAsync(objItemClassificationDClist);

            return _Result;
        }
        public class PaggingDataInActive
        {
            public int total_count { get; set; }
            public dynamic ordermaster { get; set; }
        }

        public class OrderCountInfo
        {
            public int OrderCount { get; set; }

            public int MaxOrderLimit { get; set; }

        }

        public class GroupItemLimitDc
        {
            public string itemNumber { get; set; }
            public int qty { get; set; }
            public bool ISItemLimit { get; set; }
            public int ItemLimitQty { get; set; }
        }

        public class OrderRes
        {
            public string OldValue { get; set; }
            public string FieldName { get; set; }
            public string NewValue { get; set; }
        }
    }
}