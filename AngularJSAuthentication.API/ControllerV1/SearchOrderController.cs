using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/SearchOrder")]
    public class SearchOrderController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //public HttpResponseMessage GetData()
        //{

        //    using (AuthContext dbe = new AuthContext())
        //    {

        //    //    var blogs = ((IObjectContextAdapter)db)
        //    //.ObjectContext
        //    //.Translate<List<string>>(reader, "Foo", MergeOption.AppendOnly);
        //    //    var obj = dbe.Database.SqlQuery<List<string>>("GetData").ToList();
        //    //         return Request.CreateResponse(HttpStatusCode.OK, obj);

        //    }


        //}

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getAppOrders(DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, int WarehouseId) //get search orders for delivery
        {
            using (var context = new AuthContext())
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

                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var DBoyorders = context.searchorderbycustomerwid(start, end, OrderId, Skcode, ShopName, Mobile, status, compid, Warehouse_id);
                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }
                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        Warehouse_id = WarehouseId;
                        var DBoyorders = context.searchorderbycustomerwid(start, end, OrderId, Skcode, ShopName, Mobile, status, compid, Warehouse_id);

                        //   var DBoyorders = context.searchorderbycustomer(start, end, OrderId, Skcode, ShopName, Mobile, status, compid);
                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getExports(string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, int WarehouseId) //get search orders for delivery
        {
            using (var db = new AuthContext())
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
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        List<OrderMasterDTO> newdata = new List<OrderMasterDTO>();
                        if ((Mobile != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && (a.Customerphonenum.Contains(Mobile)))
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           CustomerId = a.CustomerId,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode, // add in excel Hsn code
                                                                     ItemMultiMRPId = od.ItemMultiMRPId, // ItemMultiMRPId
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((Skcode != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.Skcode.Contains(Skcode))
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((ShopName != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.ShopName.Contains(ShopName))
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode, // add in excel Hsn code
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((OrderId != 0) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.OrderId.Equals(OrderId))
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((status != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.Status.Equals(status))

                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Mobile != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Customerphonenum.Contains(Mobile) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Skcode != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Skcode.Contains(Skcode) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (ShopName != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.ShopName.Contains(ShopName) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (OrderId != 0)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.OrderId.Equals(OrderId) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (status != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Status.Equals(status) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       //join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id)
                                       // join i in db.Customers on a.CustomerId equals i.CustomerId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = a.Skcode,
                                           ShopName = a.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = a.CustomerId,
                                           // Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        if (newdata.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, newdata);


                    }
                    else
                    {
                        List<OrderMasterDTO> newdata = new List<OrderMasterDTO>();
                        if ((Mobile != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid
                                       && (a.Customerphonenum.Contains(Mobile)))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           CustomerId = i.CustomerId,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((Skcode != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid
                                       && a.Skcode.Contains(Skcode))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((ShopName != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid
                                       && a.ShopName.Contains(ShopName))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((OrderId != 0) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid
                                       && a.OrderId.Equals(OrderId))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((status != null) && start != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid
                                       && a.Status.Equals(status))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Mobile != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Customerphonenum.Contains(Mobile) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Skcode != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Skcode.Contains(Skcode) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (ShopName != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.ShopName.Contains(ShopName) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (OrderId != 0)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.OrderId.Equals(OrderId) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId// // add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (status != null)
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where a.Status.Equals(status) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else
                        {
                            newdata = (from a in db.DbOrderMaster
                                       where (a.Deleted == false && a.CreatedDate >= start && a.CreatedDate <= end && a.CompanyId == compid)
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.CreatedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           SalesPerson = "",
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           Tin_No = a.Tin_No,// add GST No in Excel
                                           comments = a.comments,
                                           ReasonCancle = a.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     SGSTTaxPercentage = od.SGSTTaxPercentage,
                                                                     CGSTTaxPercentage = od.CGSTTaxPercentage,
                                                                     SGSTTaxAmmount = od.SGSTTaxAmmount,
                                                                     CGSTTaxAmmount = od.CGSTTaxAmmount,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName,
                                                                     SubsubcategoryName = od.SubsubcategoryName,
                                                                     HSNCode = od.HSNCode,
                                                                     ItemMultiMRPId = od.ItemMultiMRPId,// ItemMultiMRPId//// add in excel Hsn code
                                                                     ExecutiveName = od.ExecutiveName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        if (newdata.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, newdata);
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

        }

        // from ordispatchmaster
        [Route("all")]
        [HttpGet]
        public HttpResponseMessage getExportsAll(string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status) //get search orders for delivery
        {
            using (var db = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    List<OrderMasterDTO> newdata = new List<OrderMasterDTO>();
                    if (Warehouse_id > 0)
                    {
                        if ((Mobile != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && (a.Customerphonenum.Contains(Mobile)))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           CustomerId = i.CustomerId,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((Skcode != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.Skcode.Contains(Skcode))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((ShopName != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.ShopName.Contains(ShopName))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((OrderId != 0) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.OrderId.Equals(OrderId))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((status != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       && a.Status.Equals(status))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubsubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Mobile != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Customerphonenum.Contains(Mobile) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId && z.CustomerId == a.CustomerId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Skcode != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Skcode.Contains(Skcode) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (ShopName != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.ShopName.Contains(ShopName) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (OrderId != 0)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.OrderId.Equals(OrderId) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (status != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Status.Equals(status) && a.CompanyId == compid && a.WarehouseId == Warehouse_id
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderedDate >= start && a.OrderedDate <= end && a.CompanyId == compid && a.WarehouseId == Warehouse_id)
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        if (newdata.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, newdata);


                    }
                    else
                    {
                        if ((Mobile != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid
                                       && (a.Customerphonenum.Contains(Mobile)))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           CustomerId = i.CustomerId,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubsubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((Skcode != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid
                                       && a.Skcode.Contains(Skcode))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((ShopName != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid
                                       && a.ShopName.Contains(ShopName))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((OrderId != 0) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid
                                       && a.OrderId.Equals(OrderId))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if ((status != null) && start != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderDate >= start && a.OrderDate <= end && a.CompanyId == compid
                                       && a.Status.Equals(status))
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Mobile != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Customerphonenum.Contains(Mobile) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (Skcode != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Skcode.Contains(Skcode) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (ShopName != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.ShopName.Contains(ShopName) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (OrderId != 0)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.OrderId.Equals(OrderId) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else if (status != null)
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where a.Status.Equals(status) && a.CompanyId == compid
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId
                                       join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = clstr.ClusterName,
                                           ClusterId = clstr.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           OfferCode = db.OfferDb.Where(x => x.OfferId == (db.BillDiscountDb.Where(z => z.OrderId == a.OrderId).Select(c => c.OfferId).FirstOrDefault())).Select(d => d.OfferCode).FirstOrDefault(),
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                 join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                 join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                 join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = item.itemname,
                                                                     itemNumber = item.Number,
                                                                     sellingSKU = item.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = cat.CategoryName,
                                                                     BrandName = sbcat.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        else
                        {
                            newdata = (from a in db.OrderDispatchedMasters
                                       where (a.Deleted == false && a.OrderedDate >= start && a.OrderedDate <= end && a.CompanyId == compid)
                                       join i in db.Customers on a.CustomerId equals i.CustomerId
                                       join j in db.DbOrderMaster on a.OrderId equals j.OrderId

                                       //join clstr in db.Clusters on i.ClusterId equals clstr.ClusterId
                                       select new OrderMasterDTO
                                       {
                                           Skcode = i.Skcode,
                                           ShopName = i.ShopName,
                                           //CustomerName = a.CustomerName,
                                           OrderBy = a.OrderTakenSalesPerson,
                                           CustomerId = i.CustomerId,
                                           //Customerphonenum = a.Customerphonenum,
                                           WarehouseId = a.WarehouseId,
                                           WarehouseName = a.WarehouseName,
                                           ClusterName = a.ClusterName,
                                           ClusterId = a.ClusterId,
                                           OrderId = a.OrderId,
                                           CreatedDate = a.OrderedDate,
                                           BillingAddress = a.BillingAddress,
                                           CityId = a.CityId,
                                           CompanyId = a.CompanyId,
                                           Deliverydate = a.Deliverydate,
                                           DiscountAmount = a.DiscountAmount,
                                           DivisionId = a.DivisionId,
                                           GrossAmount = a.GrossAmount,
                                           invoice_no = a.invoice_no,
                                           ReDispatchCount = a.ReDispatchCount,
                                           //SalesPerson = a.SalesPerson,
                                           //SalesPersonId = a.SalesPersonId,
                                           ShippingAddress = a.ShippingAddress,
                                           deliveryCharge = a.deliveryCharge,
                                           Status = a.Status,
                                           comments = a.comments,
                                           ReasonCancle = j.ReasonCancle,
                                           orderDetailsExport = (from od in a.orderDetails
                                                                     //join item in db.itemMasters on od.ItemId equals item.ItemId
                                                                     //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                     //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                 select new OrderDetailsExport
                                                                 {
                                                                     ItemId = od.ItemId,
                                                                     itemname = od.itemname,
                                                                     itemNumber = od.itemNumber,
                                                                     sellingSKU = od.SellingSku,
                                                                     price = od.price,
                                                                     UnitPrice = od.UnitPrice,
                                                                     MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                                                     qty = od.qty,
                                                                     DiscountPercentage = od.DiscountPercentage,
                                                                     DiscountAmmount = od.DiscountAmmount,
                                                                     TaxPercentage = od.TaxPercentage,
                                                                     TaxAmmount = od.TaxAmmount,
                                                                     TotalAmt = od.TotalAmt,
                                                                     CategoryName = od.CategoryName,
                                                                     BrandName = od.SubcategoryName
                                                                 }).ToList()     /*a.orderDetails,*/
                                       }).OrderByDescending(x => x.OrderId).ToList();
                        }
                        if (newdata.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, newdata);
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }


        }

        /// <summary>
        /// ExportOrders
        /// </summary>
        /// <param name="26/08/2019"></param>
        /// <param name=""></param>
        /// created by Vinayak
        /// <returns></returns>
        /// 
        [Route("ExportWHwise")]
        [HttpGet]
        public dynamic getExportsWarehouse(int Cityid, int WarehouseId, string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status) //get search orders for delivery
        {
            using (var db = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    int CompanyId = compid;

                    string whereclause = compid > 0 ? " and a.CompanyId = " + compid : "";

                    //List<OrderMasterDTO> newdata = new List<OrderMasterDTO>();

                    if (Cityid > 0)
                        whereclause += " and a.Cityid  = " + Cityid;

                    if (WarehouseId > 0)
                        whereclause += " and a.WarehouseId=" + WarehouseId;

                    if (!string.IsNullOrEmpty(Skcode))
                        whereclause += " and a.Skcode Like" + "'%" + Skcode + "%'";

                    if (!string.IsNullOrEmpty(Mobile))
                        whereclause += " and a.Customerphonenum Like" + "'%" + Mobile + "%'";

                    if (OrderId > 0)
                        whereclause += " and a.OrderId=" + OrderId;

                    if (!string.IsNullOrEmpty(ShopName))
                        whereclause += " and a.ShopName Like" + "'%" + ShopName + "%'";

                    if (!string.IsNullOrEmpty(status))
                        whereclause += " and a.status Like" + "'" + status + "'";

                    if (start.HasValue && end.HasValue)
                        whereclause += " and (a.createdDate >=" + "'" + start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And a.createdDate <= " + "'" + end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";


                    var sqlquery = "select a.OrderId,dispatch.DeliveryIssuanceIdOrderDeliveryMaster,ISNULL(dispatch.Skcode,a.Skcode) Skcode,ISNULL(dispatch.ShopName,a.ShopName) ShopName,ISNULL(dispatch.CustomerName,a.CustomerName) CustomerName," +
                        "ISNULL(dispatch.CustomerId,a.CustomerId) CustomerId, ISNULL(dispatch.OrderBy, a.OrderTakenSalesPerson) OrderBy, ISNULL(dispatch.Customerphonenum, a.Customerphonenum) Customerphonenum," +
                        "ISNULL(dispatch.WarehouseId, a.WarehouseId) WarehouseId,ISNULL(dispatch.WarehouseName, a.WarehouseName) WarehouseName,ISNULL(dispatch.ClusterName, a.ClusterName) ClusterName," +
                       "ISNULL(dispatch.ClusterId, a.ClusterId) ClusterId,ISNULL(dispatch.CreatedDate, a.CreatedDate) CreatedDate,ISNULL(dispatch.BillingAddress, a.BillingAddress) BillingAddress,ISNULL(dispatch.CityId, a.CityId) CityId," +
                      "ISNULL(dispatch.CompanyId, a.CompanyId) CompanyId, ISNULL(dispatch.Deliverydate, a.Deliverydate) Deliverydate," +
                      "ISNULL(dispatch.DiscountAmount, a.DiscountAmount) DiscountAmount," +
                      "ISNULL(dispatch.DivisionId, a.DivisionId) DivisionId,ISNULL(dispatch.GrossAmount, a.GrossAmount) GrossAmount,ISNULL(dispatch.invoice_no, a.invoice_no) invoice_no,ISNULL(dispatch.ReDispatchCount, a.ReDispatchCount) ReDispatchCount," +
                     "ISNULL(dispatch.SalesPerson, a.SalesPerson) SalesPerson,ISNULL(dispatch.SalesPersonId, a.SalesPersonId) SalesPersonId,ISNULL(dispatch.ShippingAddress, a.ShippingAddress) ShippingAddress," +
                     "ISNULL(dispatch.deliveryCharge, a.deliveryCharge) deliveryCharge,ISNULL(dispatch.Status, a.Status) Status,ISNULL(dispatch.comments, a.comments) comments," +
                      "a.ReasonCancle,  offer.OfferCode,  od.ItemId, od.itemname, od.itemNumber, od.SellingSku, od.price, od.UnitPrice, od.UnitPrice* od.MinOrderQty MinOrderQtyPrice,od.qty,od.DiscountPercentage,od.DiscountAmmount," +
                      "od.TaxPercentage, od.TaxAmmount, od.TotalAmt,od.CategoryName,od.SubcategoryName, od.SubsubcategoryName BrandName,od.ItemMultiMRPId,od.HSNCode,dispatch.deliveryCharge,a.Tin_No,a.CGSTTaxAmmount,a.SGSTTaxAmmount," +
                      " od.CGSTTaxPercentage,od.SGSTTaxPercentage from OrderMasters a  inner join Customers b on a.CustomerId=b.CustomerId and a.Deleted =0" + whereclause +
                     "inner join orderDetails od on a.OrderId = od.OrderId outer apply (" +
                     " select  c.Skcode, c.ShopName,c.CustomerName,c.CustomerId , c.OrderTakenSalesPerson OrderBy, c.Customerphonenum ,c.WarehouseId , c.WarehouseName , c.ClusterName , c.ClusterId ,c.OrderId ," +
                     " c.OrderedDate CreatedDate, c.BillingAddress ,c.CityId , c.CompanyId ,c.Deliverydate ,c.DiscountAmount , c.DivisionId , c.GrossAmount , c.invoice_no , c.ReDispatchCount,c.SalesPerson ," +
                     "c.SalesPersonId , c.ShippingAddress , c.deliveryCharge , c.Status , c.comments,c.DeliveryIssuanceIdOrderDeliveryMaster from OrderDispatchedMasters c where c.OrderId = a.OrderId)  dispatch " +
                     " outer apply(select STRING_AGG(o.OfferCode, ',') OfferCode from BillDiscounts bill inner join Offers o on bill.OfferId = o.OfferId and bill.OrderId = a.orderid group by bill.orderid ) offer";


                    var newdata = db.Database.SqlQuery<OrderMasterDTOExport>(sqlquery).ToList();
                    return newdata;


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }


        }

        //[Route("ExportOrderMaster")]
        //[HttpPost]
        //public async Task<string> ExportOrderMaster(string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, string LevelName, CityWhDTO cityWhDTO, int OrderType, string CustomerType) //get search orders for delivery        
        //{

        //    if (start.HasValue && end.HasValue)
        //    {
        //        int compid = 0; //string LevelName = "Level 0";
        //        var identity = User.Identity as ClaimsIdentity;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        var peopleId = identity.Claims.Any(x => x.Type == "userid") ? identity.Claims.FirstOrDefault(x => x.Type == "userid").Value : "";

        //        string zipfilename = peopleId + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderExport.zip";
        //        try
        //        {


        //            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

        //            //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.CompanyId == compid);
        //            //IMongoDatabase db = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
        //            var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");

        //            var orderMasters = new List<MongoOrderMaster>();

        //            var builder = Builders<BsonDocument>.Filter;
        //            var filter = builder.Eq("CompanyId", compid);



        //            var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

        //            var customerlevel = new MonthlyCustomerLevel();

        //            customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

        //            var levels = customerlevel?.CustomerLevels;

        //            bool paramFound = false;

        //            if (cityWhDTO.CityIds.Count() > 0)
        //            {
        //                filter = filter & builder.AnyIn("CityId", cityWhDTO.CityIds);
        //                paramFound = true;
        //            }


        //            if (cityWhDTO.WarehouseIds.Count() > 0)
        //            {
        //                filter = filter & builder.AnyIn("WarehouseId", cityWhDTO.WarehouseIds);
        //                paramFound = true;
        //            }

        //            if (!string.IsNullOrEmpty(Skcode))
        //            {
        //                filter = filter & builder.Regex("Skcode", new Regex(Skcode));
        //                paramFound = true;
        //            }


        //            if (!string.IsNullOrEmpty(LevelName) && levels != null && levels.Any())
        //            {
        //                List<int> ids = levels.Where(x => x.LevelName == LevelName).Select(x => x.CustomerId).ToList();
        //                filter = filter & builder.AnyIn("CustomerId", ids);
        //                paramFound = true;
        //            }


        //            if (!string.IsNullOrEmpty(Mobile))
        //            {
        //                filter = filter & builder.Regex("Customerphonenum", new Regex(Mobile));
        //                paramFound = true;
        //            }

        //            if (OrderId > 0)
        //            {
        //                filter = filter & builder.Eq("OrderId", OrderId);
        //                paramFound = true;
        //            }

        //            if (!string.IsNullOrEmpty(ShopName))
        //            {
        //                filter = filter & builder.Regex("ShopName", new Regex(ShopName));
        //                paramFound = true;
        //            }

        //            if (!string.IsNullOrEmpty(status))
        //            {
        //                filter = filter & builder.Eq("Status", status);
        //                paramFound = true;
        //            }

        //            if (start.HasValue && end.HasValue)
        //            {
        //                DateTime edate = end.Value.Date.AddDays(1).AddMilliseconds(-1);
        //                DateTime sdate = start.Value.Date;
        //                filter = filter & builder.Gte("CreatedDate", sdate) & builder.Lte("CreatedDate", edate);
        //                paramFound = true;
        //            }

        //            if (OrderType > 0 && OrderType != 7)
        //            {
        //                filter = filter & builder.Eq("OrderType", OrderType);
        //                paramFound = true;
        //            }
        //            if (!string.IsNullOrEmpty(CustomerType) && CustomerType != "undefined")
        //            {
        //                filter = filter & builder.Eq("CustomerType", CustomerType);
        //                paramFound = true;
        //            }

        //            //if (OrderType == 7)
        //            //{
        //            //    filter = filter & builder.Eq("ParentOrderId", "Not Null");
        //            //    paramFound = true;
        //            //    // orderPredicate.And(x => x.ParentOrderId != null);
        //            //}


        //            if (cityWhDTO.PaymentFrom.Count() > 0)//&& cityWhDTO.PaymentFrom.Any(x => x != "UPI")
        //            {
        //                filter = filter & builder.AnyIn("paymentThrough", cityWhDTO.PaymentFrom);
        //                paramFound = true;
        //            }
        //            if (!paramFound)
        //            {
        //                filter = filter & builder.Gte("CreatedDate", DateTime.Now.Date.AddMonths(-1)) & builder.Lte("CreatedDate", DateTime.Now);
        //            }


        //            #region  get Live Data From Db 
        //            //if (status == "Ready to Dispatch")
        //            //{
        //            //    FilterOrderDTO Sfilter = new FilterOrderDTO();
        //            //    Sfilter.WarehouseIds = cityWhDTO.WarehouseIds;
        //            //    Sfilter.status = status;
        //            //    Sfilter.start = start.Value;
        //            //    Sfilter.end = end.Value;
        //            //    var exportDatsa = await ExportRTDOrderMaster(Sfilter);
        //            //    exportData = Mapper.Map(exportDatsa).ToANew<List<OrderMasterDTOExport>>();
        //            //    return exportData;
        //            //}
        //            #endregion


        //            FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
        //            {
        //                BatchSize = 5000,
        //                NoCursorTimeout = false
        //            };

        //            using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter, options))
        //            {
        //                //while (await cursor.MoveNextAsync())
        //                //{
        //                //    IEnumerable<BsonDocument> batch = cursor.Current;
        //                //    var result = new ConcurrentBag<MongoOrderMaster>();
        //                //    ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
        //                //    {
        //                //        var myObj = BsonSerializer.Deserialize<MongoOrderMaster>(document);
        //                //        result.Add(myObj);
        //                //    });

        //                //    if (loopResult.IsCompleted)
        //                //        orderMasters.AddRange(result.ToList());
        //                //}

        //                await cursor.ForEachAsync(document =>
        //                {
        //                    orderMasters.Add(BsonSerializer.Deserialize<MongoOrderMaster>(document));
        //                });
        //            }
        //            List<PaymentResponseRetailerApp> paymentResponseRetailerAppDb = new List<PaymentResponseRetailerApp>();
        //            //if (cityWhDTO.PaymentFrom.Any(x => x == "UPI"))
        //            //{
        //            //    using (var authContext = new AuthContext())
        //            //    {
        //            //        if (authContext.Database.Connection.State != ConnectionState.Open)
        //            //            authContext.Database.Connection.Open();
        //            //        var paymentResAppData = authContext.Database.Connection.CreateCommand();
        //            //        paymentResAppData.CommandText = "[dbo].[GetPaymentResponseRetailerApp]";
        //            //        paymentResAppData.CommandType = System.Data.CommandType.StoredProcedure;
        //            //        using (var reader = paymentResAppData.ExecuteReader())
        //            //        {
        //            //            paymentResponseRetailerAppDb = ((IObjectContextAdapter)authContext)
        //            //                            .ObjectContext
        //            //                            .Translate<PaymentResponseRetailerApp>(reader).ToList();
        //            //            var UPIOrderIds = paymentResponseRetailerAppDb.Select(z => z.OrderId).ToList();
        //            //            orderMasters = orderMasters.Where(x => UPIOrderIds.Contains(x.OrderId) && x.active == true && x.Deleted == false).ToList();
        //            //        }
        //            //    }
        //            //}


        //            if (OrderType == 7)
        //            {
        //                orderMasters = orderMasters.Where(x => x.ParentOrderId != null).ToList();
        //            }

        //            List<CurrentStockMinDc> currentStocks = new List<CurrentStockMinDc>();
        //            var pendingOrderList = orderMasters.Select(x => x.OrderId).ToList();
        //            var pendingOrders = orderMasters.Where(x => x.Status == "Pending").OrderBy(x => x.OrderId).ToList();
        //            var pendingOrderIds = pendingOrders.Select(z => z.OrderId).ToList();
        //            var multimrpIds = orderMasters.SelectMany(z => z.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsDispatchedFreeStock }).Select(x => new { x.Key.ItemMultiMRPId, x.Key.WarehouseId, x.Key.IsDispatchedFreeStock }).Distinct().ToList();
        //            var itemNumbers = orderMasters.SelectMany(z => z.orderDetails).Select(x => x.itemNumber).Distinct().ToList();



        //            using (var authContext = new AuthContext())
        //            {
        //                if (authContext.Database.Connection.State != ConnectionState.Open)
        //                    authContext.Database.Connection.Open();
        //                var orderIdDt = new DataTable();
        //                orderIdDt.Columns.Add("ItemMultiMRPId");
        //                orderIdDt.Columns.Add("WarehouseId");
        //                orderIdDt.Columns.Add("IsFreeItem");
        //                foreach (var item in multimrpIds)
        //                {
        //                    var dr = orderIdDt.NewRow();
        //                    dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
        //                    dr["WarehouseId"] = item.WarehouseId;
        //                    dr["IsFreeItem"] = item.IsDispatchedFreeStock;
        //                    orderIdDt.Rows.Add(dr);
        //                }
        //                var param = new SqlParameter("Items", orderIdDt);
        //                param.SqlDbType = SqlDbType.Structured;
        //                param.TypeName = "dbo.itemtype";
        //                var cmd = authContext.Database.Connection.CreateCommand();
        //                cmd.CommandText = "[dbo].[GetCurrentStock]";
        //                cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                cmd.Parameters.Add(param);
        //                //var OrderPaymantList = authContext.PaymentResponseRetailerAppDb.Where(x => pendingOrderIds.Contains(x.OrderId) && x.status == "Success").Select(x => new { x.OrderId, x.PaymentFrom }).ToList();

        //                // Run the sproc
        //                using (var reader = cmd.ExecuteReader())
        //                {
        //                    currentStocks = ((IObjectContextAdapter)authContext)
        //                                    .ObjectContext
        //                                    .Translate<CurrentStockMinDc>(reader).ToList();
        //                }
        //                if (authContext.Database.Connection.State != ConnectionState.Open)
        //                    authContext.Database.Connection.Open();
        //                var orderIdS = new DataTable();
        //                orderIdS.Columns.Add("OrderId");
        //                foreach (var item in pendingOrderList)
        //                {
        //                    var dr = orderIdS.NewRow();
        //                    dr["OrderId"] = item;
        //                    orderIdS.Rows.Add(dr);
        //                }
        //                var param1 = new SqlParameter("OrderId", orderIdS);
        //                param1.SqlDbType = SqlDbType.Structured;
        //                param1.TypeName = "dbo.intvalues";
        //                var paymentResAppData = authContext.Database.Connection.CreateCommand();
        //                paymentResAppData.CommandText = "[dbo].[GetPaymentResponseRetailerApp]";
        //                paymentResAppData.CommandType = System.Data.CommandType.StoredProcedure;
        //                paymentResAppData.Parameters.Add(param1);
        //                using (var reader = paymentResAppData.ExecuteReader())
        //                {
        //                    paymentResponseRetailerAppDb = ((IObjectContextAdapter)authContext)
        //                                    .ObjectContext
        //                                    .Translate<PaymentResponseRetailerApp>(reader).ToList();
        //                }
        //            }

        //            var Orderitems = pendingOrders.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
        //            var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
        //                                                       && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");

        //            allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
        //            List<PendingOrderColor> allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsDispatchedFreeStock/*, x.qty */}).Select(x => new PendingOrderColor
        //            {
        //                itemNumber = x.Key.itemNumber,
        //                ItemMultiMRPId = x.Key.ItemMultiMRPId,
        //                WarehouseId = x.Key.WarehouseId,
        //                //  x.Key.qty,
        //                IsFreeItem = x.Key.IsDispatchedFreeStock,
        //                TotalReqQty = x.Sum(y => y.qty),
        //                TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
        //                Orders = x.Select(y => new orders { OrderDetailsId = y.OrderDetailsId, OrderId = y.OrderId, qty = y.qty }).ToList()
        //            }).ToList();



        //            //foreach (var item in pendingOrders)
        //            //{
        //            //    var items = allPendingItems.Where(x => x.WarehouseId == item.WarehouseId && x.Orders.Any(y => y.OrderId == item.OrderId)).ToList();

        //            //    if (items.All(y => y.TotalAvlQty >= y.Orders.Where(s => s.OrderId < item.OrderId).Sum(s => s.qty)))
        //            //    {
        //            //        item.Description = "true";
        //            //        item.IsLessCurrentStock = false;
        //            //    }
        //            //    else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
        //            //    {
        //            //        item.IsLessCurrentStock = true;
        //            //        item.Description = "false";
        //            //    }
        //            //    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
        //            //    {
        //            //        item.Description = "true";
        //            //        item.IsLessCurrentStock = true;
        //            //    }
        //            //}

        //            List<ItemClassificationDC> ABCitemsList = orderMasters.SelectMany(x => x.orderDetails)
        //                                        .GroupBy(x => new { x.itemNumber, x.WarehouseId })
        //                                        .Select(item => new ItemClassificationDC
        //                                        {
        //                                            ItemNumber = item.Key.itemNumber,
        //                                            WarehouseId = item.Key.WarehouseId
        //                                        }).ToList();

        //            var manager = new ItemLedgerManager();
        //            var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);

        //            var exportData = new ConcurrentBag<OrderMasterDTOExport>();

        //            ParallelLoopResult loopResult1 = Parallel.ForEach(orderMasters.OrderByDescending(x => x.OrderId).ToList(), (x) =>
        //            {
        //                var lstOrders = x.orderDetails.Select(z => new OrderMasterDTOExport
        //                {

        //                    OrderId = x.OrderId,
        //                    CompanyId = x.CompanyId,
        //                    Mobile = x.Customerphonenum,
        //                    SalesPersonId = z.ExecutiveId,
        //                    SalesPerson = z.ExecutiveName,
        //                    CustomerId = x.CustomerId,
        //                    //CustomerName = x.CustomerName,
        //                    Skcode = x.Skcode,
        //                    ShopName = x.ShopName,
        //                    Status = x.Status,
        //                    invoice_no = x.invoice_no,
        //                    CustomerCategoryId = x.CustomerCategoryId,
        //                    CustomerCategoryName = x.CustomerCategoryName,
        //                    CustomerType = x.CustomerType,
        //                    //Customerphonenum = x.Customerphonenum,
        //                    BillingAddress = x.BillingAddress,
        //                    ShippingAddress = x.ShippingAddress,
        //                    TotalAmount = x.TotalAmount,
        //                    GrossAmount = x.GrossAmount,
        //                    DiscountAmount = x.DiscountAmount,
        //                    TaxAmmount = x.TaxAmount,
        //                    CityId = x.CityId,
        //                    WarehouseId = x.WarehouseId,
        //                    WarehouseName = x.WarehouseName,
        //                    active = x.active,
        //                    CreatedDate = x.CreatedDate,
        //                    ETADate = x.Deliverydate,
        //                    DeliveredDate = x.DeliveredDate,
        //                    UpdatedDate = x.UpdatedDate,
        //                    Deleted = x.Deleted,
        //                    ReDispatchCount = x.ReDispatchCount,
        //                    DivisionId = x.DivisionId,
        //                    ReasonCancle = x.ReasonCancle,
        //                    comments = x.comments,
        //                    ClusterName = x.ClusterName,
        //                    ClusterId = x.ClusterId,
        //                    GSTN_No = x.Tin_No,
        //                    deliveryCharge = x.deliveryCharge,
        //                    OrderBy = x.OrderTakenSalesPerson,
        //                    OfferCode = x.OfferCode,
        //                    ItemId = z.ItemId,
        //                    ItemMultiMRPId = z.ItemMultiMRPId,
        //                    itemname = z.itemname,
        //                    itemNumber = z.itemNumber,
        //                    sellingSKU = z.SellingSku,
        //                    price = z.price,
        //                    UnitPrice = z.UnitPrice,
        //                    MinOrderQtyPrice = z.MinOrderQtyPrice,
        //                    qty = z.qty,
        //                    DiscountPercentage = z.DiscountPercentage,
        //                    TaxPercentage = z.TaxPercentage,
        //                    TotalAmt = z.TotalAmt,
        //                    OrderedTotalAmt = z.OrderedTotalAmt,
        //                    CategoryName = z.CategoryName,
        //                    SubcategoryName = z.SubcategoryName,
        //                    BrandName = z.SubsubcategoryName,
        //                    SubsubcategoryName = z.SubsubcategoryName,
        //                    HSNCode = z.HSNCode,
        //                    SGSTTaxAmmount = z.SGSTTaxAmmount,
        //                    SGSTTaxPercentage = z.SGSTTaxPercentage,
        //                    CGSTTaxPercentage = z.CGSTTaxPercentage,
        //                    IGSTTaxAmount = z.IGSTTaxAmount,
        //                    IGSTTaxPercent = z.IGSTTaxPercent,
        //                    DeliveryIssuanceIdOrderDeliveryMaster = x.DeliveryIssuanceIdOrderDeliveryMaster,
        //                    // ColourCode = levels != null && levels.Any(a => a.CustomerId == x.CustomerId) ? levels.Where(a => a.CustomerId == x.CustomerId).Select(a => a.ColourCode).FirstOrDefault() : null,
        //                    IsLessCurrentStock = x.IsLessCurrentStock,
        //                    Description = x.Description,
        //                    CreditNoteNumber = x.CreditNoteNumber,
        //                    CreditNoteDate = x.CreditNoteDate,
        //                    OrderType = x.OrderType,
        //                    IsPrimeCustomer = x.IsPrimeCustomer,
        //                    StoreId = z.StoreId,
        //                    StoreName = z.StoreName,
        //                    IsFreeItem = z.IsFreeItem,
        //                    // OrderColor = x.IsLessCurrentStock ? "Red" : (x.Description == "true" ? "Blue" : "White"),
        //                    OrderItemColor = x.Status != "Pending" ? "" : ReturnColor(z.itemNumber, z.ItemMultiMRPId, z.qty, z.IsDispatchedFreeStock, currentStocks, x.OrderId, x.WarehouseId, allPendingItems),
        //                    paymentThrough = x.paymentThrough,
        //                    ParentOrderId = x.ParentOrderId,
        //                    IsFirstOrder = x.IsFirstOrder

        //                }).ToList();
        //                lstOrders.ToList().ForEach(y =>
        //                {

        //                    if (y.CreatedDate != null)
        //                    {
        //                        if (y.OrderItemColor == "Red")
        //                        {
        //                            var items = allPendingItems.Where(s => s.Orders.Any(z => z.OrderId == y.OrderId)).ToList();
        //                            y.OrderItemColor = lstOrders.Where(z => z.OrderId == y.OrderId).Sum(z => z.TotalAmt) * 3 / 100 >= lstOrders.Where(z => z.OrderId == y.OrderId && z.OrderItemColor == "Red").Sum(z => z.TotalAmt) ? "Yellow" : "Red";
        //                            if (items != null && items.Any() && items.Any(s => s.itemNumber == y.itemNumber && s.ItemMultiMRPId == y.ItemMultiMRPId && y.IsFreeItem == s.IsFreeItem))
        //                            {
        //                                y.AvailableStockAmt = (items.FirstOrDefault(s => s.itemNumber == y.itemNumber && s.ItemMultiMRPId == y.ItemMultiMRPId && y.IsFreeItem == s.IsFreeItem).TotalAvlQty) * y.UnitPrice;
        //                            }
        //                        }
        //                        if (GetItem != null && GetItem.Any())
        //                        {
        //                            if (GetItem.Any(z => z.ItemNumber == y.itemNumber && z.WarehouseId == x.WarehouseId))
        //                            {
        //                                y.ABC_Classification = GetItem.FirstOrDefault(z => z.ItemNumber == y.itemNumber && z.WarehouseId == x.WarehouseId).Category;
        //                            }
        //                            else { y.ABC_Classification = "D"; }
        //                        }
        //                        else { y.ABC_Classification = "D"; }

        //                        if (y.OrderType == 0 || y.OrderType == 1)
        //                            y.OrderTypestr = "General";
        //                        else if (y.OrderType == 2)
        //                            y.OrderTypestr = "Bundle";
        //                        else if (y.OrderType == 3)
        //                            y.OrderTypestr = "Return";
        //                        else if (y.OrderType == 4)
        //                            y.OrderTypestr = "Distributor";
        //                        else if (y.OrderType == 5)
        //                            y.OrderTypestr = "Zaruri";
        //                        else if (y.OrderType == 6)
        //                            y.OrderTypestr = "Damage";
        //                        else if (y.OrderType == 8)
        //                            y.OrderTypestr = "Clearance";
        //                        else if (y.OrderType == 9)
        //                            y.OrderTypestr = "NonSellable";

        //                        if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "Red"))
        //                            y.MainOrderColor = "Red";
        //                        else if (lstOrders.Where(z => z.OrderId == y.OrderId).All(z => z.OrderItemColor == "Blue"))
        //                            y.MainOrderColor = "Blue";
        //                        else if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "Yellow"))
        //                            y.MainOrderColor = "Yellow";
        //                        else if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "White" || z.OrderItemColor == "Blue"))
        //                            y.MainOrderColor = "White";
        //                        y.paymentThrough = paymentResponseRetailerAppDb.FirstOrDefault(p => p.OrderId == y.OrderId) != null ? paymentResponseRetailerAppDb.FirstOrDefault(p => p.OrderId == y.OrderId).PaymentFrom : "";
        //                        exportData.Add(y);
        //                    }
        //                });

        //            });


        //            string fileUrl = string.Empty;
        //            if (loopResult1.IsCompleted)
        //            {
        //                var fileName = peopleId + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderExport.csv";

        //                var newexportdata = exportData.ToList().Select(item => new
        //                {
        //                    item.MainOrderColor,
        //                    item.OrderItemColor,
        //                    item.OrderTypestr,
        //                    item.OrderId,
        //                    item.OfferCode,
        //                    item.Skcode,
        //                    //item.Customerphonenum,
        //                    item.ShopName,
        //                    item.SalesPerson,
        //                    //item.CustomerName,
        //                    item.invoice_no,
        //                    item.CreditNoteNumber,
        //                    item.CreditNoteDate,
        //                    item.ItemId,
        //                    item.itemname,
        //                    item.ABC_Classification,
        //                    item.CategoryName,
        //                    item.SubcategoryName,
        //                    item.BrandName,
        //                    item.HSNCode,
        //                    item.sellingSKU,
        //                    item.WarehouseName,
        //                    item.ClusterName,
        //                    item.CreatedDate,
        //                    item.OrderBy,
        //                    item.TotalAmt,
        //                    item.AvailableStockAmt,
        //                    item.OrderedTotalAmt,
        //                    item.UnitPrice,
        //                    item.MinOrderQtyPrice,
        //                    item.qty,
        //                    item.DiscountAmount,
        //                    item.DiscountPercentage,
        //                    item.TaxAmmount,
        //                    item.TaxPercentage,
        //                    item.SGSTTaxAmmount,
        //                    item.SGSTTaxPercentage,
        //                    item.CGSTTaxPercentage,
        //                    item.IGSTTaxAmount,
        //                    item.IGSTTaxPercent,
        //                    item.deliveryCharge,
        //                    item.GSTN_No,
        //                    item.Status,
        //                    item.ReasonCancle,
        //                    item.comments,
        //                    item.ItemMultiMRPId,
        //                    item.DeliveryIssuanceIdOrderDeliveryMaster,
        //                    item.IsPrimeCustomer,
        //                    item.StoreName,
        //                    item.paymentThrough,
        //                    item.ParentOrderId,
        //                    item.ETADate,
        //                    item.DeliveredDate,
        //                    item.IsFirstOrder
        //                }).ToList();


        //                DataTable dt = ListtoDataTableConverter.ToDataTable(newexportdata);
        //                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
        //                dt.WriteToCsvFile(path);

        //                string zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

        //                using (ZipArchive archive = ZipFile.Open(zipCreatePath, ZipArchiveMode.Create))
        //                {
        //                    archive.CreateEntryFromFile(path, fileName);
        //                }

        //                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
        //                                                                , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                                                , string.Format("ExcelGeneratePath/{0}", zipfilename));
        //                File.Delete(path);
        //                return fileUrl;
        //            }
        //        }


        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //    else
        //    {
        //        return "Please Enter Value of End date and Start date";
        //    }
        //    return "";
        //}

        [Route("ExportOrderMaster")]
        [HttpPost]
        public async Task<string> ExportOrderMaster(string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, string LevelName, CityWhDTO cityWhDTO, int OrderType, string CustomerType) //get search orders for delivery        
        {

            if (start.HasValue && end.HasValue)
            {
                List<ChannelTypeDC> ChannelTypes = new List<ChannelTypeDC>();

                List<GetItemCityWiseIncentiveClassificationDc> itemCityWiseIncentiveClassificationList = new List<GetItemCityWiseIncentiveClassificationDc>();
                List<KeyValuePair<int, int>> MonthYear = new List<KeyValuePair<int, int>>();
                if (start.Value.Month == end.Value.Month)
                {
                    MonthYear.Add(new KeyValuePair<int, int>(start.Value.Month, start.Value.Year));
                }
                else
                {
                    MonthYear.Add(new KeyValuePair<int, int>(start.Value.Month, start.Value.Year));
                    MonthYear.Add(new KeyValuePair<int, int>(end.Value.Month, end.Value.Year));
                }
                int compid = 0; //string LevelName = "Level 0";
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                var peopleId = identity.Claims.Any(x => x.Type == "userid") ? identity.Claims.FirstOrDefault(x => x.Type == "userid").Value : "";

                string zipfilename = peopleId + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderExport.zip";
                try
                {


                    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                    //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.CompanyId == compid);
                    //IMongoDatabase db = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                    var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");

                    var orderMasters = new List<MongoOrderMaster>();

                    var builder = Builders<BsonDocument>.Filter;
                    var filter = builder.Eq("CompanyId", compid);



                    var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                    var customerlevel = new MonthlyCustomerLevel();

                    customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                    var levels = customerlevel?.CustomerLevels;

                    bool paramFound = false;

                    if (cityWhDTO.CityIds.Count() > 0)
                    {
                        filter = filter & builder.AnyIn("CityId", cityWhDTO.CityIds);
                        paramFound = true;
                    }


                    if (cityWhDTO.WarehouseIds.Count() > 0)
                    {
                        filter = filter & builder.AnyIn("WarehouseId", cityWhDTO.WarehouseIds);
                        paramFound = true;
                    }

                    if (!string.IsNullOrEmpty(Skcode))
                    {
                        filter = filter & builder.Regex("Skcode", new Regex(Skcode));
                        paramFound = true;
                    }


                    if (!string.IsNullOrEmpty(LevelName) && levels != null && levels.Any())
                    {
                        List<int> ids = levels.Where(x => x.LevelName == LevelName).Select(x => x.CustomerId).ToList();
                        filter = filter & builder.AnyIn("CustomerId", ids);
                        paramFound = true;
                    }


                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        filter = filter & builder.Regex("Customerphonenum", new Regex(Mobile));
                        paramFound = true;
                    }

                    if (OrderId > 0)
                    {
                        filter = filter & builder.Eq("OrderId", OrderId);
                        paramFound = true;
                    }

                    if (!string.IsNullOrEmpty(ShopName))
                    {
                        filter = filter & builder.Regex("ShopName", new Regex(ShopName));
                        paramFound = true;
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        filter = filter & builder.Eq("Status", status);
                        paramFound = true;
                    }

                    if (start.HasValue && end.HasValue)
                    {
                        DateTime edate = end.Value.Date.AddDays(1).AddMilliseconds(-1);
                        DateTime sdate = start.Value.Date;
                        filter = filter & builder.Gte("CreatedDate", sdate) & builder.Lte("CreatedDate", edate);
                        paramFound = true;
                    }

                    if (OrderType > 0 && OrderType != 7)
                    {
                        filter = filter & builder.Eq("OrderType", OrderType);
                        paramFound = true;
                    }
                    if (!string.IsNullOrEmpty(CustomerType) && CustomerType != "undefined")
                    {
                        filter = filter & builder.Eq("CustomerType", CustomerType);
                        paramFound = true;
                    }

                    //if (OrderType == 7)
                    //{
                    //    filter = filter & builder.Eq("ParentOrderId", "Not Null");
                    //    paramFound = true;
                    //    // orderPredicate.And(x => x.ParentOrderId != null);
                    //}


                    if (cityWhDTO.PaymentFrom.Count() > 0)//&& cityWhDTO.PaymentFrom.Any(x => x != "UPI")
                    {
                        filter = filter & builder.AnyIn("paymentThrough", cityWhDTO.PaymentFrom);
                        paramFound = true;
                    }
                    if (!paramFound)
                    {
                        filter = filter & builder.Gte("CreatedDate", DateTime.Now.Date.AddMonths(-1)) & builder.Lte("CreatedDate", DateTime.Now);
                    }


                    #region  get Live Data From Db 
                    //if (status == "Ready to Dispatch")
                    //{
                    //    FilterOrderDTO Sfilter = new FilterOrderDTO();
                    //    Sfilter.WarehouseIds = cityWhDTO.WarehouseIds;
                    //    Sfilter.status = status;
                    //    Sfilter.start = start.Value;
                    //    Sfilter.end = end.Value;
                    //    var exportDatsa = await ExportRTDOrderMaster(Sfilter);
                    //    exportData = Mapper.Map(exportDatsa).ToANew<List<OrderMasterDTOExport>>();
                    //    return exportData;
                    //}
                    #endregion


                    FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
                    {
                        BatchSize = 5000,
                        NoCursorTimeout = false
                    };

                    using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter, options))
                    {
                        //while (await cursor.MoveNextAsync())
                        //{
                        //    IEnumerable<BsonDocument> batch = cursor.Current;
                        //    var result = new ConcurrentBag<MongoOrderMaster>();
                        //    ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
                        //    {
                        //        var myObj = BsonSerializer.Deserialize<MongoOrderMaster>(document);
                        //        result.Add(myObj);
                        //    });

                        //    if (loopResult.IsCompleted)
                        //        orderMasters.AddRange(result.ToList());
                        //}

                        await cursor.ForEachAsync(document =>
                        {
                            orderMasters.Add(BsonSerializer.Deserialize<MongoOrderMaster>(document));
                        });
                    }
                    List<PaymentResponseRetailerApp> paymentResponseRetailerAppDb = new List<PaymentResponseRetailerApp>();
                    //if (cityWhDTO.PaymentFrom.Any(x => x == "UPI"))
                    //{
                    //    using (var authContext = new AuthContext())
                    //    {
                    //        if (authContext.Database.Connection.State != ConnectionState.Open)
                    //            authContext.Database.Connection.Open();
                    //        var paymentResAppData = authContext.Database.Connection.CreateCommand();
                    //        paymentResAppData.CommandText = "[dbo].[GetPaymentResponseRetailerApp]";
                    //        paymentResAppData.CommandType = System.Data.CommandType.StoredProcedure;
                    //        using (var reader = paymentResAppData.ExecuteReader())
                    //        {
                    //            paymentResponseRetailerAppDb = ((IObjectContextAdapter)authContext)
                    //                            .ObjectContext
                    //                            .Translate<PaymentResponseRetailerApp>(reader).ToList();
                    //            var UPIOrderIds = paymentResponseRetailerAppDb.Select(z => z.OrderId).ToList();
                    //            orderMasters = orderMasters.Where(x => UPIOrderIds.Contains(x.OrderId) && x.active == true && x.Deleted == false).ToList();
                    //        }
                    //    }
                    //}


                    if (OrderType == 7)
                    {
                        orderMasters = orderMasters.Where(x => x.ParentOrderId != null).ToList();
                    }

                    List<CurrentStockMinDc> currentStocks = new List<CurrentStockMinDc>();
                    var pendingOrderList = orderMasters.Select(x => x.OrderId).ToList();
                    var AllcustomerIds = orderMasters.Select(x => x.CustomerId).ToList();
                    var pendingOrders = orderMasters.Where(x => x.Status == "Pending").OrderBy(x => x.OrderId).ToList();
                    var pendingOrderIds = pendingOrders.Select(z => z.OrderId).ToList();
                    var multimrpIds = orderMasters.SelectMany(z => z.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsDispatchedFreeStock })
                                                                                    .Select(x => new { x.Key.ItemMultiMRPId, x.Key.WarehouseId, x.Key.IsDispatchedFreeStock }).Distinct().ToList();
                    var itemNumbers = orderMasters.SelectMany(z => z.orderDetails).Select(x => x.itemNumber).Distinct().ToList();

                    var RTPQtyInExportList = new List<RTPQtyInExport>();

                    List<PTRDc> PTRData = new List<PTRDc>();
                    List<AllCityListDc> AllCitylist = new List<AllCityListDc>();
                    List<CustomerSegment> customerSegment = new List<CustomerSegment>();
                    List<PeopleListDc> peopledata = new List<PeopleListDc>();
                    using (var authContext = new AuthContext())
                    {

                        if (authContext.Database.Connection.State != ConnectionState.Open)
                            authContext.Database.Connection.Open();
                        authContext.Database.CommandTimeout = 180;
                        #region   rtpOrders

                        var RTPOrderId = orderMasters.Where(x => x.Status == "ReadyToPick").Select(x => x.OrderId).Distinct().ToList();
                        if (RTPOrderId != null && RTPOrderId.Any())
                        {
                            var rorderIdS = new DataTable();
                            rorderIdS.Columns.Add("OrderId");
                            foreach (var item in RTPOrderId)
                            {
                                var dr = rorderIdS.NewRow();
                                dr["OrderId"] = item;
                                rorderIdS.Rows.Add(dr);
                            }
                            var rtpparam1 = new SqlParameter("OrderId", rorderIdS);
                            rtpparam1.SqlDbType = SqlDbType.Structured;
                            rtpparam1.TypeName = "dbo.intvalues";
                            var Rtpdata = authContext.Database.Connection.CreateCommand();
                            Rtpdata.CommandText = "[dbo].[GetRTPOrderQtyInExport]";
                            Rtpdata.CommandType = System.Data.CommandType.StoredProcedure;
                            Rtpdata.Parameters.Add(rtpparam1);
                            using (var reader = Rtpdata.ExecuteReader())
                            {
                                RTPQtyInExportList = ((IObjectContextAdapter)authContext)
                                                .ObjectContext
                                                .Translate<RTPQtyInExport>(reader).ToList();
                            }

                        }
                        #endregion

                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("ItemMultiMRPId");
                        orderIdDt.Columns.Add("WarehouseId");
                        orderIdDt.Columns.Add("IsFreeItem");
                        foreach (var item in multimrpIds)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                            dr["WarehouseId"] = item.WarehouseId;
                            dr["IsFreeItem"] = item.IsDispatchedFreeStock;
                            orderIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("Items", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.itemtype";
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetCurrentStock]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        //var OrderPaymantList = authContext.PaymentResponseRetailerAppDb.Where(x => pendingOrderIds.Contains(x.OrderId) && x.status == "Success").Select(x => new { x.OrderId, x.PaymentFrom }).ToList();

                        // Run the sproc
                        using (var reader = cmd.ExecuteReader())
                        {
                            currentStocks = ((IObjectContextAdapter)authContext)
                                            .ObjectContext
                                            .Translate<CurrentStockMinDc>(reader).ToList();
                        }


                        var orderIdS = new DataTable();
                        orderIdS.Columns.Add("OrderId");
                        foreach (var item in pendingOrderList)
                        {
                            var dr = orderIdS.NewRow();
                            dr["OrderId"] = item;
                            orderIdS.Rows.Add(dr);
                        }
                        var param1 = new SqlParameter("OrderId", orderIdS);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.intvalues";
                        var paymentResAppData = authContext.Database.Connection.CreateCommand();
                        paymentResAppData.CommandText = "[dbo].[GetPaymentResponseRetailerApp]";
                        paymentResAppData.CommandType = System.Data.CommandType.StoredProcedure;
                        paymentResAppData.Parameters.Add(param1);
                        using (var reader = paymentResAppData.ExecuteReader())
                        {
                            paymentResponseRetailerAppDb = ((IObjectContextAdapter)authContext)
                                            .ObjectContext
                                            .Translate<PaymentResponseRetailerApp>(reader).ToList();
                        }

                        string query = @"select ItemMultiMRPId,Cityid,max(PTR) as Ptrprice from ItemSchemes b with(nolock) where  b.IsActive=1 and b.IsDeleted=0
                                     and b.Cityid in (" + string.Join(",", cityWhDTO.CityIds) + ")group by ItemMultiMRPId,Cityid";
                        PTRData = authContext.Database.SqlQuery<PTRDc>(query).ToList();

                        string sqlquery = @"select c.Cityid,c.CityName
                                            from Warehouses w with(nolock)
                                            inner join GMWarehouseProgresses b with(nolock) on w.WarehouseId = b.WarehouseID and b.IsLaunched = 1
                                            Inner join Cities c with(nolock) on w.Cityid = c.Cityid
                                            and w.active = 1 and w.Deleted = 0 and w.IsKPP = 0
                                            group by c.Cityid,c.CityName";
                        AllCitylist = authContext.Database.SqlQuery<AllCityListDc>(sqlquery).ToList();


                        var CustIDs = orderMasters.Select(y => y.CustomerId).Distinct().ToList();
                        #region For Customer ChannelType

                        var ChannelTypeOrderIdS = new DataTable();
                        ChannelTypeOrderIdS.Columns.Add("CustomerIds");
                        foreach (var item in CustIDs)
                        {
                            var ChannelTypedr = ChannelTypeOrderIdS.NewRow();
                            ChannelTypedr["CustomerIds"] = item;
                            ChannelTypeOrderIdS.Rows.Add(ChannelTypedr);
                        }
                        var ChannelTypeparam1 = new SqlParameter("CustomerIds", ChannelTypeOrderIdS);
                        ChannelTypeparam1.SqlDbType = SqlDbType.Structured;
                        ChannelTypeparam1.TypeName = "dbo.intvalues";
                       
                        var data = authContext.Database.Connection.CreateCommand();
                        data.CommandText = "[dbo].[CustomerChannelTypeDetailsV1]";
                        data.CommandType = System.Data.CommandType.StoredProcedure;
                        data.Parameters.Add(ChannelTypeparam1);
                        using (var reader = data.ExecuteReader())
                        {
                            ChannelTypes = ((IObjectContextAdapter)authContext)
                                            .ObjectContext
                                            .Translate<ChannelTypeDC>(reader).ToList();
                        }

                        #endregion
                      
                        customerSegment = authContext.CustomerSegmentDb.Where(y => CustIDs.Contains(y.CustomerId) && y.IsActive && y.IsDeleted == false).ToList();


                        var salespersondata = orderMasters.SelectMany(z => z.orderDetails).GroupBy(x => new { x.ExecutiveId }).Where(x => x.Key.ExecutiveId > 0)
                                                                                        .Select(x => x.Key.ExecutiveId).Distinct().ToList();
                        if (salespersondata != null && salespersondata.Count > 0)
                        {
                            peopledata = authContext.Peoples.Where(x => salespersondata.Contains(x.PeopleID)).Select(x => new PeopleListDc { PeopleID = x.PeopleID, Empcode = x.Empcode }).ToList();
                        }

                    }

                    var Orderitems = pendingOrders.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                    var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                                                               && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");

                    allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
                    List<PendingOrderColor> allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsDispatchedFreeStock/*, x.qty */}).Select(x => new PendingOrderColor
                    {
                        itemNumber = x.Key.itemNumber,
                        ItemMultiMRPId = x.Key.ItemMultiMRPId,
                        WarehouseId = x.Key.WarehouseId,
                        //  x.Key.qty,
                        IsFreeItem = x.Key.IsDispatchedFreeStock,
                        TotalReqQty = x.Sum(y => y.qty),
                        TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                        Orders = x.Select(y => new orders { OrderDetailsId = y.OrderDetailsId, OrderId = y.OrderId, qty = y.qty }).ToList()
                    }).ToList();



                    //foreach (var item in pendingOrders)
                    //{
                    //    var items = allPendingItems.Where(x => x.WarehouseId == item.WarehouseId && x.Orders.Any(y => y.OrderId == item.OrderId)).ToList();

                    //    if (items.All(y => y.TotalAvlQty >= y.Orders.Where(s => s.OrderId < item.OrderId).Sum(s => s.qty)))
                    //    {
                    //        item.Description = "true";
                    //        item.IsLessCurrentStock = false;
                    //    }
                    //    else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                    //    {
                    //        item.IsLessCurrentStock = true;
                    //        item.Description = "false";
                    //    }
                    //    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                    //    {
                    //        item.Description = "true";
                    //        item.IsLessCurrentStock = true;
                    //    }
                    //}

                    List<ItemClassificationDC> ABCitemsList = orderMasters.SelectMany(x => x.orderDetails)
                                                .GroupBy(x => new { x.itemNumber, x.WarehouseId })
                                                .Select(item => new ItemClassificationDC
                                                {
                                                    ItemNumber = item.Key.itemNumber,
                                                    WarehouseId = item.Key.WarehouseId
                                                }).ToList();

                    var manager = new ItemLedgerManager();
                    var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);

                    var exportData = new ConcurrentBag<OrderMasterDTOExport>();
                    ParallelLoopResult parellelResult = Parallel.ForEach(MonthYear, (x) =>
                    {
                        try
                        {
                            using (var context = new AuthContext())
                            {
                                var wIdS = new DataTable();
                                wIdS.Columns.Add("WarehouseIds");
                                foreach (var item in cityWhDTO.WarehouseIds)
                                {
                                    var dr = wIdS.NewRow();
                                    dr["WarehouseIds"] = item;
                                    wIdS.Rows.Add(dr);
                                }
                                var WarehouseIds = new SqlParameter("WarehouseIds", wIdS);
                                var month = new SqlParameter("month", x.Key);
                                var year = new SqlParameter("year", x.Value);

                                if (context.Database.Connection.State != ConnectionState.Open)
                                    context.Database.Connection.Open();
                                var cmd2 = context.Database.Connection.CreateCommand();
                                cmd2.CommandText = "GetItemCityWiseIncentiveClassification";
                                cmd2.Parameters.Add(WarehouseIds);
                                cmd2.Parameters.Add(month);
                                cmd2.Parameters.Add(year);
                                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd2.CommandTimeout = 300;
                                // Run the sproc
                                var reader = cmd2.ExecuteReader();
                                var reportdata = ((IObjectContextAdapter)context)
                                .ObjectContext
                                .Translate<GetItemCityWiseIncentiveClassificationDc>(reader).ToList();
                                itemCityWiseIncentiveClassificationList.AddRange(reportdata);
                            }
                        }
                        catch (Exception ex)
                        {

                            TextFileLogHelper.LogError("Error During Order Item Classification for :" + ex.ToString());
                        }
                    });
                    if (parellelResult.IsCompleted) { };

                    ParallelLoopResult loopResult1 = Parallel.ForEach(orderMasters.OrderByDescending(x => x.OrderId).ToList(), (x) =>
                    {
                        var lstOrders = x.orderDetails.Select(z => new OrderMasterDTOExport
                        {

                            OrderId = x.OrderId,
                            CompanyId = x.CompanyId,
                            Mobile = x.Customerphonenum,
                            SalesPersonId = z.ExecutiveId,
                            SalesPerson = z.ExecutiveName,
                            CustomerId = x.CustomerId,
                            //CustomerName = x.CustomerName,
                            Skcode = x.Skcode,
                            ShopName = x.ShopName,
                            Status = x.Status,
                            invoice_no = x.invoice_no,
                            CustomerCategoryId = x.CustomerCategoryId,
                            CustomerCategoryName = x.CustomerCategoryName,
                            CustomerType = x.CustomerType,
                            //Customerphonenum = x.Customerphonenum,
                            BillingAddress = x.BillingAddress,
                            ShippingAddress = x.ShippingAddress,
                            TotalAmount = x.TotalAmount,
                            GrossAmount = x.GrossAmount,
                            DiscountAmount = x.DiscountAmount,
                            TaxAmmount = x.TaxAmount,
                            CityId = x.CityId,
                            WarehouseId = x.WarehouseId,
                            WarehouseName = x.WarehouseName,
                            active = x.active,
                            CreatedDate = x.CreatedDate,
                            ETADate = x.Deliverydate,
                            DeliveredDate = x.DeliveredDate,
                            UpdatedDate = x.UpdatedDate,
                            Deleted = x.Deleted,
                            ReDispatchCount = x.ReDispatchCount,
                            DivisionId = x.DivisionId,
                            ReasonCancle = x.ReasonCancle,
                            comments = x.comments,
                            ClusterName = x.ClusterName,
                            ClusterId = x.ClusterId,
                            GSTN_No = x.Tin_No,
                            deliveryCharge = x.deliveryCharge,
                            OrderBy = x.OrderTakenSalesPerson,
                            OfferCode = x.OfferCode,
                            ItemId = z.ItemId,
                            ItemMultiMRPId = z.ItemMultiMRPId,
                            itemname = z.itemname,
                            itemNumber = z.itemNumber,
                            sellingSKU = z.SellingSku,
                            price = z.price,
                            UnitPrice = z.UnitPrice,
                            PTRPrice = PTRData != null && PTRData.Any(e => e.Cityid == z.CityId && e.ItemMultiMRPId == z.ItemMultiMRPId)
                            ? z.PTR > 0 ? (z.price / (1 + (z.PTR / 100)))
                            : PTRData.FirstOrDefault(e => e.Cityid == z.CityId && e.ItemMultiMRPId == z.ItemMultiMRPId).Ptrprice > 0
                            ? (z.price / PTRData.FirstOrDefault(e => e.Cityid == z.CityId && e.ItemMultiMRPId == z.ItemMultiMRPId).Ptrprice)
                            : z.UnitPrice : 0,
                            MinOrderQtyPrice = z.MinOrderQtyPrice,
                            qty = z.qty,
                            RTPqty = RTPQtyInExportList != null && RTPQtyInExportList.Count() > 0 && RTPQtyInExportList.Any(e => e.OrderDetailsId == z.OrderDetailsId) ? RTPQtyInExportList.FirstOrDefault(e => e.OrderDetailsId == z.OrderDetailsId).RTPqty : 0,
                            DiscountPercentage = z.DiscountPercentage,
                            TaxPercentage = z.TaxPercentage,
                            TotalAmt = z.TotalAmt,
                            OrderedTotalAmt = z.OrderedTotalAmt,
                            CategoryName = z.CategoryName,
                            SubcategoryName = z.SubcategoryName,
                            BrandName = z.SubsubcategoryName,
                            SubsubcategoryName = z.SubsubcategoryName,
                            HSNCode = z.HSNCode,
                            SGSTTaxAmmount = z.SGSTTaxAmmount,
                            SGSTTaxPercentage = z.SGSTTaxPercentage,
                            CGSTTaxPercentage = z.CGSTTaxPercentage,
                            IGSTTaxAmount = z.IGSTTaxAmount,
                            IGSTTaxPercent = z.IGSTTaxPercent,
                            DeliveryIssuanceIdOrderDeliveryMaster = x.DeliveryIssuanceIdOrderDeliveryMaster,
                            // ColourCode = levels != null && levels.Any(a => a.CustomerId == x.CustomerId) ? levels.Where(a => a.CustomerId == x.CustomerId).Select(a => a.ColourCode).FirstOrDefault() : null,
                            IsLessCurrentStock = x.IsLessCurrentStock,
                            Description = x.Description,
                            CreditNoteNumber = x.CreditNoteNumber,
                            CreditNoteDate = x.CreditNoteDate,
                            OrderType = x.OrderType,
                            IsPrimeCustomer = x.IsPrimeCustomer,
                            StoreId = z.StoreId,
                            StoreName = z.StoreName,
                            IsFreeItem = z.IsFreeItem,
                            // OrderColor = x.IsLessCurrentStock ? "Red" : (x.Description == "true" ? "Blue" : "White"),
                            OrderItemColor = x.Status != "Pending" ? "" : ReturnColor(z.itemNumber, z.ItemMultiMRPId, z.qty, z.IsDispatchedFreeStock, currentStocks, x.OrderId, x.WarehouseId, allPendingItems),
                            paymentThrough = x.paymentThrough,
                            ParentOrderId = x.ParentOrderId,
                            IsFirstOrder = x.IsFirstOrder,
                            IsDigitalOrder = x.IsDigitalOrder,
                            ChannelType = (ChannelTypes != null && ChannelTypes.Count() > 0) ? ChannelTypes.FirstOrDefault(y => y.CustomerId == x.CustomerId && y.StoreId == z.StoreId) != null ? ChannelTypes.FirstOrDefault(y => y.CustomerId == x.CustomerId  && y.StoreId == z.StoreId).ChannelType : "" : "",
                            EmpCode = z.ExecutiveId > 0 ? peopledata.FirstOrDefault(y => y.PeopleID == z.ExecutiveId).Empcode : ""
                        }).ToList();

                        if (lstOrders != null)
                        {
                            lstOrders.ToList().ForEach(y =>
                            {
                                DateTime Orderdate = y.CreatedDate.AddMonths(-1);
                                y.IncentiveClassification = itemCityWiseIncentiveClassificationList.Any() && itemCityWiseIncentiveClassificationList.Any(e => e.ItemMultiMRPId == y.ItemMultiMRPId && e.WarehouseId == y.WarehouseId && e.Month == Orderdate.Month && e.Year == Orderdate.Year) ? itemCityWiseIncentiveClassificationList.FirstOrDefault(e => e.ItemMultiMRPId == y.ItemMultiMRPId && e.WarehouseId == y.WarehouseId && e.Month == Orderdate.Month && e.Year == Orderdate.Year).Classification : "General";
                                y.CityName = AllCitylist.Any() && AllCitylist.Any(w => w.Cityid == y.CityId) ? AllCitylist.FirstOrDefault(w => w.Cityid == y.CityId).CityName : "";
                                if (y.CreatedDate != null)
                                {
                                    if (y.OrderItemColor == "Red")
                                    {
                                        var items = allPendingItems.Where(s => s.Orders.Any(z => z.OrderId == y.OrderId)).ToList();
                                        y.OrderItemColor = lstOrders.Where(z => z.OrderId == y.OrderId).Sum(z => z.TotalAmt) * 3 / 100 >= lstOrders.Where(z => z.OrderId == y.OrderId && z.OrderItemColor == "Red").Sum(z => z.TotalAmt) ? "Yellow" : "Red";
                                        if (items != null && items.Any() && items.Any(s => s.itemNumber == y.itemNumber && s.ItemMultiMRPId == y.ItemMultiMRPId && y.IsFreeItem == s.IsFreeItem))
                                        {
                                            y.AvailableStockAmt = (items.FirstOrDefault(s => s.itemNumber == y.itemNumber && s.ItemMultiMRPId == y.ItemMultiMRPId && y.IsFreeItem == s.IsFreeItem).TotalAvlQty) * y.UnitPrice;
                                        }
                                    }
                                    if (GetItem != null && GetItem.Any())
                                    {
                                        if (GetItem.Any(z => z.ItemNumber == y.itemNumber && z.WarehouseId == x.WarehouseId))
                                        {
                                            y.ABC_Classification = GetItem.FirstOrDefault(z => z.ItemNumber == y.itemNumber && z.WarehouseId == x.WarehouseId).Category;
                                        }
                                        else { y.ABC_Classification = "D"; }
                                    }
                                    else { y.ABC_Classification = "D"; }

                                    if (y.OrderType == 0 || y.OrderType == 1)
                                        y.OrderTypestr = "General";
                                    else if (y.OrderType == 2)
                                        y.OrderTypestr = "Bundle";
                                    else if (y.OrderType == 3)
                                        y.OrderTypestr = "Return";
                                    else if (y.OrderType == 4)
                                        y.OrderTypestr = "Distributor";
                                    else if (y.OrderType == 5)
                                        y.OrderTypestr = "Zaruri";
                                    else if (y.OrderType == 6)
                                        y.OrderTypestr = "Damage";
                                    else if (y.OrderType == 8)
                                        y.OrderTypestr = "Clearance";
                                    else if (y.OrderType == 9)
                                        y.OrderTypestr = "NonSellable";
                                    else if (y.OrderType == 10)
                                        y.OrderTypestr = "NonRevenue";

                                    if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "Red"))
                                        y.MainOrderColor = "Red";
                                    else if (lstOrders.Where(z => z.OrderId == y.OrderId).All(z => z.OrderItemColor == "Blue"))
                                        y.MainOrderColor = "Blue";
                                    else if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "Yellow"))
                                        y.MainOrderColor = "Yellow";
                                    else if (lstOrders.Where(z => z.OrderId == y.OrderId).Any(z => z.OrderItemColor == "White" || z.OrderItemColor == "Blue"))
                                        y.MainOrderColor = "White";
                                    y.paymentThrough = paymentResponseRetailerAppDb.FirstOrDefault(p => p.OrderId == y.OrderId) != null ? paymentResponseRetailerAppDb.FirstOrDefault(p => p.OrderId == y.OrderId).PaymentFrom : "";

                                    var CustSegment = customerSegment.OrderByDescending(z => z.CreatedDate).Where(z => z.CustomerId == y.CustomerId).FirstOrDefault();
                                    if (CustSegment != null)
                                    {
                                        switch (CustSegment.Segment)
                                        {
                                            case 1:
                                                y.CustomerSegment = "Platinum";
                                                break;
                                            case 2:
                                                y.CustomerSegment = "Gold";
                                                break;
                                            case 3:
                                                y.CustomerSegment = "Silver";
                                                break;
                                            case 4:
                                                y.CustomerSegment = "GT";
                                                break;
                                            case 5:
                                                switch (CustSegment.PotentialSegment)
                                                {
                                                    case 1:
                                                        y.CustomerSegment = "Potential Platinum";
                                                        break;
                                                    case 2:
                                                        y.CustomerSegment = "Potential Gold";
                                                        break;
                                                    case 3:
                                                        y.CustomerSegment = "Potential Silver";
                                                        break;
                                                    case 4:
                                                        y.CustomerSegment = "Potential GT";
                                                        break;
                                                    case 5:
                                                        y.CustomerSegment = "Digital";
                                                        break;
                                                }
                                                break;
                                        }
                                    }
                                    exportData.Add(y);
                                }
                            });
                        }


                    });


                    string fileUrl = string.Empty;
                    if (loopResult1.IsCompleted)
                    {
                        var fileName = peopleId + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderExport.csv";

                        #region CRMTAG

                        /*var skcodeList = exportData.Select(x => x.Skcode).Distinct().ToList();

                        CRMManager cRMManager = new CRMManager();
                        var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);
                        */
                        foreach (var x in exportData)
                        {
                            x.CRMTags = x.IsDigitalOrder.HasValue && x.IsDigitalOrder.Value ? "Digital" : null;
                            //x.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;
                        }

                        #endregion

                        var newexportdata = exportData.ToList().Select(item => new
                        {
                            item.MainOrderColor,
                            item.OrderItemColor,
                            item.OrderTypestr,
                            item.OrderId,
                            item.OfferCode,
                            item.Skcode,
                            item.CRMTags,
                            //item.Customerphonenum,
                            item.ShopName,
                            item.SalesPersonId,
                            item.SalesPerson,
                            item.EmpCode,
                            //item.CustomerName,
                            item.invoice_no,
                            item.CreditNoteNumber,
                            item.CreditNoteDate,
                            item.ItemId,
                            item.itemname,
                            item.ABC_Classification,
                            item.CategoryName,
                            item.SubcategoryName,
                            item.BrandName,
                            item.HSNCode,
                            item.sellingSKU,
                            item.CityName,
                            item.WarehouseName,
                            item.ClusterName,
                            item.CreatedDate,
                            item.OrderBy,
                            item.TotalAmt,
                            item.AvailableStockAmt,
                            item.OrderedTotalAmt,
                            item.UnitPrice,
                            item.PTRPrice,
                            item.MinOrderQtyPrice,
                            item.qty,
                            item.RTPqty,
                            item.DiscountAmount,
                            item.DiscountPercentage,
                            item.TaxAmmount,
                            item.TaxPercentage,
                            item.SGSTTaxAmmount,
                            item.SGSTTaxPercentage,
                            item.CGSTTaxPercentage,
                            item.IGSTTaxAmount,
                            item.IGSTTaxPercent,
                            item.deliveryCharge,
                            item.GSTN_No,
                            item.Status,
                            item.ReasonCancle,
                            item.comments,
                            item.ItemMultiMRPId,
                            item.DeliveryIssuanceIdOrderDeliveryMaster,
                            item.IsPrimeCustomer,
                            item.StoreName,
                            item.paymentThrough,
                            item.ParentOrderId,
                            item.ETADate,
                            item.DeliveredDate,
                            item.IsFirstOrder,
                            item.IncentiveClassification,
                            item.ChannelType,
                            item.CustomerSegment,
                            item.CustomerType
                        }).ToList();




                        DataTable dt = ListtoDataTableConverter.ToDataTable(newexportdata);
                        string basePath = ConfigurationManager.AppSettings["BasePath"];
                        string WebUrl = ConfigurationManager.AppSettings["ERPAdminWebsite"];
                        // string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                        string path = Path.Combine(basePath+ "/ExcelGeneratePath", fileName);
                        dt.WriteToCsvFile(path);

                        string zipCreatePath = basePath + "/ExcelGeneratePath/" + zipfilename;

                        using (ZipArchive archive = ZipFile.Open(zipCreatePath, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(path, fileName);
                        }

                        /* fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , string.Format("ExcelGeneratePath/{0}", zipfilename));*/
                        //fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                        //                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                        //                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                        //                                                 , string.Format("ExcelGeneratePath/{0}", zipfilename));
                        fileUrl = WebUrl + string.Format("ExcelGeneratePath/{0}", zipfilename);
                        File.Delete(path);
                        return fileUrl;
                    }
                }


                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return "Please Enter Value of End date and Start date";
            }
            return "";
        }



        [Route("ExportSalesOrderMaster")]
        [HttpPost]
        public async Task<List<OrderMasterDTOExport>> ExportSalesOrderMaster(string type, DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, string LevelName, CityWhDTO cityWhDTO) //get search orders for delivery
        {

            using (var db = new AuthContext())
            {
                if (start.HasValue && end.HasValue)
                {

                    try
                    {
                        int compid = 0; //string LevelName = "Level 0";
                        var identity = User.Identity as ClaimsIdentity;
                        MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                        //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.CompanyId == compid);
                        //IMongoDatabase db = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                        var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");

                        var orderMasters = new List<MongoOrderMaster>();

                        var builder = Builders<BsonDocument>.Filter;
                        var filter = builder.Eq("CompanyId", compid);

                        var exportData = new List<OrderMasterDTOExport>();

                        var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                        var customerlevel = new MonthlyCustomerLevel();

                        customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                        var levels = customerlevel?.CustomerLevels;

                        if (cityWhDTO.CityIds.Count() > 0)
                            filter = filter & builder.AnyIn("CityId", cityWhDTO.CityIds);

                        if (cityWhDTO.WarehouseIds.Count() > 0)
                            filter = filter & builder.AnyIn("WarehouseId", cityWhDTO.WarehouseIds);

                        if (!string.IsNullOrEmpty(Skcode))
                        {
                            filter = filter & builder.Regex("Skcode", new Regex(Skcode));
                        }

                        if (!string.IsNullOrEmpty(LevelName) && levels != null && levels.Any())
                        {
                            List<int> ids = levels.Where(x => x.LevelName == LevelName).Select(x => x.CustomerId).ToList();
                            filter = filter & builder.AnyIn("CustomerId", ids);
                        }

                        if (!string.IsNullOrEmpty(Mobile))
                            filter = filter & builder.Regex("Customerphonenum", new Regex(Mobile));

                        if (OrderId > 0)
                            filter = filter & builder.Eq("OrderId", OrderId);

                        if (!string.IsNullOrEmpty(ShopName))
                            filter = filter & builder.Regex("ShopName", new Regex(ShopName));

                        if (!string.IsNullOrEmpty(status))
                            filter = filter & builder.Eq("Status", status);

                        if (start.HasValue && end.HasValue)
                            filter = filter & builder.Gte("CreatedDate", start.Value) & builder.Lte("CreatedDate", end.Value);


                        FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
                        {
                            BatchSize = 5000,
                            NoCursorTimeout = false
                        };

                        using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter, options))
                        {
                            while (await cursor.MoveNextAsync())
                            {
                                IEnumerable<BsonDocument> batch = cursor.Current;
                                var result = new ConcurrentBag<MongoOrderMaster>();
                                ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
                                {
                                    var myObj = BsonSerializer.Deserialize<MongoOrderMaster>(document);
                                    result.Add(myObj);
                                });

                                if (loopResult.IsCompleted)
                                    orderMasters.AddRange(result.ToList());
                            }
                        }
                        // var ChannelType = db.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).ToList();

                        orderMasters.OrderByDescending(x => x.OrderId).ToList().ForEach(x =>
                        {
                            //var ChannelMasterId = db.Customers.FirstOrDefault(y => y.CustomerId == x.CustomerId && y.Active == true && y.Deleted == false).ChannelMasterId;
                            var lstOrders = x.orderDetails.Select(z => new OrderMasterDTOExport
                            {
                                OrderId = x.OrderId,
                                CompanyId = x.CompanyId,
                                Mobile = x.Customerphonenum,
                                SalesPersonId = z.ExecutiveId,
                                SalesPerson = z.ExecutiveName,
                                CustomerId = x.CustomerId,
                                //CustomerName = x.CustomerName,
                                Skcode = x.Skcode,
                                ShopName = x.ShopName,
                                Status = x.Status,
                                invoice_no = x.invoice_no,
                                CustomerCategoryId = x.CustomerCategoryId,
                                CustomerCategoryName = x.CustomerCategoryName,
                                CustomerType = x.CustomerType,
                                //Customerphonenum = x.Customerphonenum,
                                BillingAddress = x.BillingAddress,
                                ShippingAddress = x.ShippingAddress,
                                TotalAmount = x.TotalAmount,
                                GrossAmount = x.GrossAmount,
                                DiscountAmount = x.DiscountAmount,
                                TaxAmmount = x.TaxAmount,
                                CityId = x.CityId,
                                WarehouseId = x.WarehouseId,
                                WarehouseName = x.WarehouseName,
                                active = x.active,
                                CreatedDate = x.CreatedDate,
                                ETADate = x.Deliverydate,
                                UpdatedDate = x.UpdatedDate,
                                Deleted = x.Deleted,
                                ReDispatchCount = x.ReDispatchCount,
                                DivisionId = x.DivisionId,
                                ReasonCancle = x.ReasonCancle,
                                comments = x.comments,
                                ClusterName = x.ClusterName,
                                ClusterId = x.ClusterId,
                                GSTN_No = x.Tin_No,
                                deliveryCharge = x.deliveryCharge,
                                OrderBy = x.OrderTakenSalesPerson,
                                OfferCode = x.OfferCode,
                                ItemId = z.ItemId,
                                ItemMultiMRPId = z.ItemMultiMRPId,
                                itemname = z.itemname,
                                itemNumber = z.itemNumber,
                                sellingSKU = z.SellingSku,
                                price = z.price,
                                UnitPrice = z.UnitPrice,
                                MinOrderQtyPrice = z.MinOrderQtyPrice,
                                qty = z.qty,
                                DiscountPercentage = z.DiscountPercentage,
                                TaxPercentage = z.TaxPercentage,
                                TotalAmt = z.TotalAmt,
                                OrderedTotalAmt = z.OrderedTotalAmt,
                                CategoryName = z.CategoryName,
                                SubcategoryName = z.SubcategoryName,
                                BrandName = z.SubsubcategoryName,
                                SubsubcategoryName = z.SubsubcategoryName,
                                HSNCode = z.HSNCode,
                                SGSTTaxAmmount = z.SGSTTaxAmmount,
                                SGSTTaxPercentage = z.SGSTTaxPercentage,
                                CGSTTaxPercentage = z.CGSTTaxPercentage,
                                IGSTTaxAmount = z.IGSTTaxAmount,
                                IGSTTaxPercent = z.IGSTTaxPercent,
                                DeliveryIssuanceIdOrderDeliveryMaster = x.DeliveryIssuanceIdOrderDeliveryMaster,
                                // ColourCode = levels != null && levels.Any(a => a.CustomerId == x.CustomerId) ? levels.Where(a => a.CustomerId == x.CustomerId).Select(a => a.ColourCode).FirstOrDefault() : null,
                                IsLessCurrentStock = x.IsLessCurrentStock,
                                Description = x.Description,
                                CreditNoteNumber = x.CreditNoteNumber,
                                CreditNoteDate = x.CreditNoteDate,
                                OrderType = x.OrderType,
                                IsPrimeCustomer = x.IsPrimeCustomer,
                                StoreId = z.StoreId,
                                StoreName = z.StoreName,
                                // OrderColor = x.IsLessCurrentStock ? "Red" : (x.Description == "true" ? "Blue" : "White"),
                                OrderItemColor = "",
                                IsFirstOrder = x.IsFirstOrder,
                                //  ChannelType = ((ChannelType != null && ChannelType.Count() > 0) && ChannelMasterId > 0) ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == ChannelMasterId) != null ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == ChannelMasterId).ChannelType : "" : ""

                            }).ToList();

                            exportData.AddRange(lstOrders);
                        });
                        List<ItemClassificationDC> ABCitemsList = exportData.Select(item => new ItemClassificationDC { ItemNumber = item.itemNumber, WarehouseId = item.WarehouseId }).ToList();

                        #region CRMTAG

                        var skcodeList = exportData.Select(x => x.Skcode).Distinct().ToList();

                        CRMManager cRMManager = new CRMManager();
                        var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);

                        #endregion

                        var manager = new ItemLedgerManager();
                        var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                        foreach (var item in exportData)
                        {
                            item.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == item.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;

                            if (GetItem != null && GetItem.Any())
                            {
                                if (GetItem.Any(x => x.ItemNumber == item.itemNumber))
                                {
                                    item.ABC_Classification = GetItem.FirstOrDefault(x => x.ItemNumber == item.itemNumber).Category;
                                }
                                else { item.ABC_Classification = "D"; }
                            }
                            else { item.ABC_Classification = "D"; }

                        }

                        return exportData;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    return null;
                }
            }

        }


        private bool IsLessCureentStock(string ItemNumber, int ItemMultiMRPId, int Qty, List<CurrentStockMinDc> currentStock, int OrderId, List<MongoOrderMaster> Om, string Status, DataContracts.Transaction.Mongo.OrderDetails detail)
        {
            bool Result = false;
            if (Status.ToUpper() != "READY TO DISPATCH")
            {
                int warehouseid = Om.Where(x => x.OrderId == OrderId).Select(x => x.WarehouseId).FirstOrDefault();
                int currentstockqty = currentStock.Where(x => x.ItemNumber == ItemNumber && x.ItemMultiMRPId == ItemMultiMRPId && x.WarehouseId == warehouseid && x.IsFreeItem == detail.IsDispatchedFreeStock).Select(x => x.CurrentInventory).FirstOrDefault();

                if (Qty > currentstockqty)
                {
                    Result = true;
                }
            }

            return Result;
        }


        private string ReturnColor(string ItemNumber, int itemMultiMRPId, int Qty, bool IsDispatchedFreeStock, List<CurrentStockMinDc> currentStock, int OrderId, int warehouseId, List<PendingOrderColor> allPendingItems)
        {
            string Result = "";
            int currentstockqty = currentStock.Where(x => x.ItemNumber == ItemNumber && x.ItemMultiMRPId == itemMultiMRPId && x.WarehouseId == warehouseId && x.IsFreeItem == IsDispatchedFreeStock).Select(x => x.CurrentInventory).FirstOrDefault();

            if (Qty > currentstockqty)
            {
                Result = "Red";
            }
            else
            {
                var items = allPendingItems.Where(x => x.WarehouseId == warehouseId && x.ItemMultiMRPId == itemMultiMRPId && x.Orders.Any(y => y.OrderId == OrderId)).FirstOrDefault();
                //if (currentstockqty >= items.Orders.Where(s => s.OrderId <= OrderId).Sum(s => s.qty))
                if (currentstockqty >= (items == null ? 0 : items.Orders.Sum(s => s.qty)))
                {
                    Result = "Blue";
                }
                else
                {
                    Result = "White";
                }

            }
            return Result;
        }


        #region OrderProcessStatus
        [Route("OrderProcessStatus")]
        [HttpGet]
        public object OrderProcessStatus(int OrderId) //get search orders for delivery
        {
            using (var dc = new AuthContext())
            {
                try
                {
                    var data = dc.DbOrderMaster.Where(s => s.OrderId == OrderId).SingleOrDefault();
                    return (data);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        #endregion


        [Route("ExportOrderMasterDb")]
        [HttpPost]
        public async Task<string> ExportOrderMaster(FilterOrderDTO filterOrderDTO)
        {
            var manager = new ItemLedgerManager();
            var recordCount = await manager.ExportOrderMasterCount(filterOrderDTO);
            List<SearchOrderExport> exportDataList = new List<SearchOrderExport>();
            string fileUrl = "";
            int batchCount = 1000;
            if (recordCount > 0)
            {
                int i = 0;
                var result = new ConcurrentBag<SearchOrderExport>();

                batchCount = batchCount > recordCount ? recordCount : batchCount;
                ParallelLoopResult loopResult = Parallel.For(0, recordCount / batchCount, (index) =>
                  {
                      var skip = index * batchCount;
                      int take = batchCount;

                      var exportData = manager.ExportOrderMaster(filterOrderDTO, skip, take);
                      foreach (var item in exportData)
                      {
                          result.Add(item);
                      }
                  });


                if (loopResult.IsCompleted)
                    exportDataList.AddRange(result.ToList());


                var identity = User.Identity as ClaimsIdentity;
                var peopleId = identity.Claims.Any(x => x.Type == "userid") ? identity.Claims.FirstOrDefault(x => x.Type == "userid").Value : "";
                var fileName = peopleId + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(exportDataList);
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , HttpContext.Current.Request.Url.Port
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));

            }



            return fileUrl;
        }




        public async Task<List<SearchOrderExport>> ExportRTDOrderMaster(FilterOrderDTO filterOrderDTO)
        {
            var manager = new ItemLedgerManager();
            var recordCount = await manager.ExportOrderMasterCount(filterOrderDTO);
            List<SearchOrderExport> exportDataList = new List<SearchOrderExport>();
            int batchCount = 1000;
            if (recordCount > 0)
            {
                int i = 0;
                var result = new ConcurrentBag<SearchOrderExport>();
                batchCount = batchCount > recordCount ? recordCount : batchCount;
                ParallelLoopResult loopResult = Parallel.For(0, recordCount / batchCount, (index) =>
                {
                    var skip = index * batchCount;
                    int take = batchCount;

                    var exportData = manager.ExportOrderMaster(filterOrderDTO, skip, take);
                    foreach (var item in exportData)
                    {
                        result.Add(item);
                    }
                });
                if (loopResult.IsCompleted)
                    exportDataList.AddRange(result.ToList());
            }
            return exportDataList;
        }


        public class CityWhDTO
        {
            public List<int> WarehouseIds { get; set; }
            public List<int> CityIds { get; set; }
            public List<string> PaymentFrom { get; set; }
        }

        public class RTPQtyInExport
        {
            public int OrderDetailsId { get; set; }
            public int RTPqty { get; set; }
        }


        public class OrderMasterDTOExport
        {
            public int OrderId { get; set; }
            public int CompanyId { get; set; }
            public string Mobile { get; set; }
            public int? SalesPersonId { get; set; }
            public string SalesPerson { get; set; }
            public int CustomerId { get; set; }
            //public string CustomerName { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public string Status { get; set; }
            public string invoice_no { get; set; }
            public int CustomerCategoryId { get; set; }
            public string CustomerCategoryName { get; set; }
            public string CustomerType { get; set; }
            //public string Customerphonenum { get; set; }

            public string CreditNoteNumber { get; set; }
            public DateTime? CreditNoteDate { get; set; }
            public string BillingAddress { get; set; }
            public string ShippingAddress { get; set; }
            public double TotalAmount { get; set; }
            public double GrossAmount { get; set; }
            public double DiscountAmount { get; set; }
            public double TaxAmmount { get; set; }
            public double? ReadytoDdiffhours { get; set; }
            public double? ReadytoDelivereddiffhours { get; set; }
            public double? Deliverydiffhours { get; set; }
            public int? CityId { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public bool active { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ETADate { get; set; }
            public DateTime UpdatedDate { get; set; }
            public bool Deleted { get; set; }
            public int ReDispatchCount { get; set; }
            public int DivisionId { get; set; }
            public string ReasonCancle { get; set; }
            public string comments { get; set; }
            public string ClusterName { get; set; }
            public int ClusterId { get; set; }
            public string GSTN_No { get; set; }// add for Gst No. in Excel
            public double? deliveryCharge { get; set; }
            public string OrderBy { get; set; }
            public DateTime ReadyToDistpatch_Time { get; set; }//added in DTO
            public DateTime Issued_Time { get; set; }//added in DTO
            public DateTime Shipped_Time { get; set; }//added in DTO
            public DateTime Delivered_Time { get; set; }//added in DTO
            public DateTime FirstRedispatch_Time { get; set; }//added in DTO
            public DateTime SecondRedispatch_Time { get; set; }//added in DTO
            public DateTime ThirdRedispatch_Time { get; set; }//added in DTO
            public int? ItemMultiMRPId { get; set; }
            public string OfferCode { get; set; }
            public DateTime Date { get; set; }

            public int ItemId { get; set; }
            public string itemname { get; set; }
            public string itemNumber { get; set; }
            public string sellingSKU { get; set; }
            public double price { get; set; }
            public double UnitPrice { get; set; }
            public double PTR { get; set; }
            public double PTRPrice { get; set; }
            public double MinOrderQtyPrice { get; set; }
            public int qty { get; set; }
            public double DiscountPercentage { get; set; }
            public double TaxPercentage { get; set; }
            public double TotalAmt { get; set; }
            public double AvailableStockAmt { get; set; }
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public string BrandName { get; set; }
            public string SubsubcategoryName { get; set; }
            public string HSNCode { get; set; }
            public double SGSTTaxAmmount { get; set; }
            public double SGSTTaxPercentage { get; set; }
            public double CGSTTaxPercentage { get; set; }
            public double IGSTTaxAmount { get; set; }
            public double IGSTTaxPercent { get; set; }
            public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
            public double? OrderedTotalAmt { get; set; }
            public string ColourCode { get; set; }
            public string Description { get; set; }
            public bool IsLessCurrentStock { get; set; }
            public string OrderItemColor { get; set; }
            public string MainOrderColor { get; set; }
            public int? OrderType { get; set; }
            public string OrderTypestr { get; set; }
            public string ABC_Classification { get; set; }
            public bool? IsPrimeCustomer { get; set; }
            public long StoreId { get; set; }
            public string StoreName { get; set; }

            public bool IsFreeItem { get; set; }
            public string paymentThrough { get; set; }
            public int? ParentOrderId { get; set; }
            public bool IsFirstOrder { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public int? RTPqty { get; set; }
            public string IncentiveClassification { get; set; }
            public string CRMTags { get; set; }
            public string CityName { get; set; }
            public bool? IsDigitalOrder { get; set; }
            public string ChannelType { get; set; }
            public string CustomerSegment { get; set; }
            public string EmpCode { get; set; }

        }
    }


    public class PendingOrderColor
    {
        public string itemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsFreeItem { get; set; }
        public int TotalReqQty { get; set; }
        public int TotalAvlQty { get; set; }
        public List<orders> Orders { get; set; }
    }

    public class orders
    {
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }

        public int qty { get; set; }
    }

    public class Cls
    {
        public string Name { get; set; }
        public Cls2 DOb { get; set; }
    }

    public class Cls2
    {
        public string DOb { get; set; }
    }
    public class PTRDc
    {
        public int ItemMultiMRPId { get; set; }
        public int Cityid { get; set; }
        public double Ptrprice { get; set; }
    }
    public class PeopleListDc
    {
        public int PeopleID { get; set; }
        public string Empcode { get; set; }
    }
    public class GetItemCityWiseIncentiveClassificationDc
    {
        public string Classification { get; set; }
        public long StoreId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class AllCityListDc
    {
        public int Cityid { get; set; }
        public string CityName { get; set; }
    }
    public class ChannelTypeDC
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int StoreId { get; set; }
        public string ChannelType { get; set; }
    }
}
