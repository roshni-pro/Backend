using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{

    [RoutePrefix("api/EwayBillOrderController")]
    public class EwayBillOrderController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();


        [Route("GetEwaybillOrderSearchMongo")]
        [HttpPost]
        public dynamic GetOrdersFromMongo(EwayBillpaginationDTO filterOrderDTO)
        {
            PagginationData paggingData = new PagginationData();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            List<string> statuslst = new List<string> { "Ready to Dispatch", "Issued", "Shipped", "Delivery Redispatch" };
            using (AuthContext context = new AuthContext())
            {
                if (filterOrderDTO.Cityid > 0)
                {
                    string whereclause = "";
                    if (filterOrderDTO.Cityid > 0)
                        whereclause += " and o.Cityid =" + filterOrderDTO.Cityid;

                    if (filterOrderDTO.WarehouseId > 0)
                        whereclause += " and o.WarehouseId=" + filterOrderDTO.WarehouseId;

                    if (filterOrderDTO.OrderId > 0)
                        whereclause += " and o.OrderId=" + filterOrderDTO.OrderId;

                    if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))
                        whereclause += " and o.Skcode=" + "'" + filterOrderDTO.Skcode + "'";

                    if (!string.IsNullOrEmpty(filterOrderDTO.status))
                        whereclause += " and o.status=" + "'" + filterOrderDTO.status + "'";

                    if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
                        whereclause += " and (o.CreatedDate >= " + "'" + filterOrderDTO.FromDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  o.CreatedDate <=" + "'" + filterOrderDTO.ToDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";

                    //filterOrderDTO.Skip++;
                    int skip = filterOrderDTO.Skip;// (filterOrderDTO.Skip - 1) * filterOrderDTO.Take;
                    int take = filterOrderDTO.Take;
                    //and o.GrossAmount >= 50000
                    string sqlquery = "Select CustomerId, OrderId,OrderDispatchedMasterId,  CustomerName,Skcode,Customerphonenum ,GrossAmount,Status ,EwayBillNumber  from OrderDispatchedMasters  o (nolock) where  o.Deleted = 0    " + whereclause
                      + " Order by o.OrderId desc offset " + skip + " rows fetch next " + take + " rows only";

                    string sqlCountQuery = "Select Count(*) from OrderDispatchedMasters o (nolock)  where  o.Deleted = 0   " + whereclause;//and o.GrossAmount >= 50000
                    //and o.status  in ('Ready to Dispatch', 'Issued', 'Shipped', 'Delivery Redispatch')
                    //and o.status  in ('Ready to Dispatch', 'Issued', 'Shipped', 'Delivery Redispatch')
                    List<OrderMasterDc> newdata = context.Database.SqlQuery<OrderMasterDc>(sqlquery).ToList();

                    int dataCount = context.Database.SqlQuery<int>(sqlCountQuery).FirstOrDefault();

                    //Generate Eway-bill number when Distance greater than 9 KM.
                    paggingData.total_count = 0;
                    if (newdata != null && newdata.Any())
                    {
                        paggingData.total_count = dataCount;
                        var customerids = newdata.Select(x => x.CustomerId).Distinct().ToList();
                        var warehouseids = newdata.Select(x => x.WarehouseId).Distinct().ToList();
                        CustomersManager manager = new CustomersManager();
                        var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
                        newdata.ForEach(x =>
                        {
                            if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                            {
                                var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                                var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                                if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lg != 0)
                                {
                                    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                                    var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                                    var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                    var distanceValue = Math.Round(dist, 1);
                                    if (distanceValue >= 9.0)
                                    {
                                        x.Distance = Math.Round(dist, 2).ToString();
                                    }
                                }
                            }

                        });
                    }

                    paggingData.ordermaster = newdata.ToList();//.Where(x => x.GrossAmount >= 50000)
                    return paggingData;
                }
                else
                {
                    return filterOrderDTO;
                }
            }
            //using (AuthContext context = new AuthContext())
            //{
            //    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

            //    var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => statuslst.Contains(x.Status) && !x.Deleted && x.GrossAmount > 50000);
            //    if (filterOrderDTO.Cityid > 0)
            //    {
            //        if (filterOrderDTO.Cityid > 0)
            //            orderPredicate.And(x => x.CityId == filterOrderDTO.Cityid);

            //        if (filterOrderDTO.WarehouseId > 0)
            //            orderPredicate.And(x => x.WarehouseId == filterOrderDTO.WarehouseId);

            //        if (filterOrderDTO.OrderId > 0)
            //            orderPredicate.And(x => x.OrderId == filterOrderDTO.OrderId);

            //        if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))
            //            orderPredicate.And(x => x.Skcode.Contains(filterOrderDTO.Skcode));

            //        if (!string.IsNullOrEmpty(filterOrderDTO.status))
            //            orderPredicate.And(x => x.Status == filterOrderDTO.status);

            //        if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
            //            orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.FromDate.Value && x.CreatedDate <= filterOrderDTO.ToDate.Value);

            //        int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");

            //        var Ewayorder = new List<MongoOrderMaster>();
            //        Ewayorder = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), filterOrderDTO.Skip, filterOrderDTO.Take, collectionName: "OrderMaster").ToList();
            //        #region Prepare data                        
            //        var newdata = Ewayorder.Select(x => new OrderMasterDc
            //        {
            //            OrderId = x.OrderId,
            //            CompanyId = x.CompanyId,
            //            SalesPersonId = x.SalesPersonId,
            //            //SalesPerson = x.SalesPerson,
            //            SalesPerson = string.Join(",", x.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct().ToList()),
            //            SalesMobile = x.SalesMobile,
            //            CustomerId = x.CustomerId,
            //            CustomerName = x.CustomerName,
            //            Skcode = x.Skcode,
            //            ShopName = x.ShopName,
            //            Status = x.Status,
            //            invoice_no = x.invoice_no,
            //            Trupay = x.Trupay,
            //            paymentThrough = x.paymentThrough,
            //            TrupayTransactionId = x.TrupayTransactionId,
            //            paymentMode = x.paymentMode,
            //            //PaymentFrom = x.PaymentFrom,
            //            CustomerCategoryId = x.CustomerCategoryId,
            //            CustomerCategoryName = x.CustomerCategoryName,
            //            CustomerType = x.CustomerType,
            //            LandMark = x.LandMark,
            //            Customerphonenum = x.Customerphonenum,
            //            BillingAddress = x.BillingAddress,
            //            ShippingAddress = x.ShippingAddress,
            //            TotalAmount = x.TotalAmount,
            //            GrossAmount = x.GrossAmount,
            //            DiscountAmount = x.DiscountAmount,
            //            TaxAmount = x.TaxAmount,
            //            SGSTTaxAmmount = x.SGSTTaxAmmount,
            //            CGSTTaxAmmount = x.CGSTTaxAmmount,
            //            CityId = x.CityId,
            //            WarehouseId = x.WarehouseId,
            //            WarehouseName = x.WarehouseName,
            //            active = x.active,
            //            CreatedDate =
            //            TimeZoneInfo.ConvertTime((x.OrderMasterHistories.Any(y => y.Status == "Pending") ? x.OrderMasterHistories.FirstOrDefault(y => y.Status == "Pending").CreatedDate :
            //            x.CreatedDate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //            Deliverydate = TimeZoneInfo.ConvertTime(x.Deliverydate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //            UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //            ReadytoDispatchedDate = x.ReadytoDispatchedDate.HasValue ? TimeZoneInfo.ConvertTime(x.ReadytoDispatchedDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")) : (DateTime?)null,
            //            DeliveredDate = x.DeliveredDate.HasValue ? TimeZoneInfo.ConvertTime(x.DeliveredDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")) : (DateTime?)null,
            //            Deleted = x.Deleted,
            //            ReDispatchCount = x.ReDispatchCount,
            //            DivisionId = x.DivisionId,
            //            ReasonCancle = x.ReasonCancle,
            //            ClusterId = x.ClusterId,
            //            ClusterName = x.ClusterName,
            //            deliveryCharge = x.deliveryCharge,
            //            WalletAmount = x.WalletAmount,
            //            walletPointUsed = x.walletPointUsed,
            //            UsedPoint = x.UsedPoint,
            //            RewardPoint = x.RewardPoint,
            //            ShortAmount = x.ShortAmount,
            //            comments = x.comments,
            //            OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
            //            OrderTakenSalesPerson = x.OrderTakenSalesPerson,
            //            Tin_No = x.Tin_No,
            //            ShortReason = x.ShortReason,
            //            orderProcess = x.orderProcess,
            //            accountProcess = x.accountProcess,
            //            chequeProcess = x.chequeProcess,
            //            epaymentProcess = x.epaymentProcess,
            //            Savingamount = x.Savingamount,
            //            OnlineServiceTax = x.OnlineServiceTax,
            //            InvoiceBarcodeImage = x.InvoiceBarcodeImage,
            //            userid = x.userid,
            //            Description = x.Description,
            //            IsLessCurrentStock = x.IsLessCurrentStock,
            //            BillDiscountAmount = x.BillDiscountAmount,
            //            offertype = x.offertype,
            //            OrderDispatchedMasterId = x.OrderDispatchedMasterId,
            //            DispatchAmount = x.DispatchAmount,
            //            OfferCode = x.OfferCode,
            //            OrderType = x.OrderType,
            //            EwayBillNumber = x.EwayBillNumber,

            //        }).ToList();
            //        #endregion

            //        //Generate Eway-bill number when Distance greater than 9 KM.
            //        paggingData.total_count = 0;
            //        if (newdata != null && newdata.Any())
            //        {
            //            paggingData.total_count = dataCount;
            //            var customerids = newdata.Select(x => x.CustomerId).Distinct().ToList();
            //            var warehouseids = newdata.Select(x => x.WarehouseId).Distinct().ToList();
            //            CustomersManager manager = new CustomersManager();
            //            var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
            //            newdata.ForEach(x =>
            //         {
            //             if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
            //             {
            //                 var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
            //                 var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

            //                 if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lg != 0)
            //                 {
            //                     var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
            //                     var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
            //                     var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
            //                     var distanceValue = Math.Round(dist, 1);
            //                     if (distanceValue >= 9.0)
            //                     {
            //                         x.Distance = Math.Round(dist, 2).ToString();
            //                     }
            //                 }
            //             }

            //         });
            //            // paggingData.Ewayorderlist = Ewayorder;
            //        }

            //        paggingData.ordermaster = newdata.Where(x => x.GrossAmount >= 50000).ToList();
            //        return paggingData;
            //    }
            //    else
            //    {
            //        return filterOrderDTO;
            //    }
            //}

        }


        [Route("UploadEwayBillDocV7")]
        [HttpPost]
        public IHttpActionResult UploadEwayBillDoc()
        {
            logger.Info("start image upload");

            string LogoUrl = "";
            string ImageUrl = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            // string filename = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    var filename = "ewayBill_" + httpPostedFile.FileName;   //+ "_orderId";
                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/EwayBillImages/"), filename);

                    httpPostedFile.SaveAs(LogoUrl);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/EwayBillImages/", LogoUrl);

                    return Created(LogoUrl, LogoUrl);
                }
                return null;
            }
            return null;
        }




        [Route("updateEwaybillOrder")]
        [HttpPost]
        public async Task<string> GenerateAndUpdateEwaybill(GenerateUpdateEwaybillDc ewayBilldata)
        {
            string result = "";
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var updateOrderDispatched = context.OrderDispatchedMasters.Where(x => x.OrderId == ewayBilldata.OrderId && x.OrderDispatchedMasterId == ewayBilldata.OrderDispatchedMasterId).SingleOrDefault();
                if (updateOrderDispatched != null && userid > 0)
                {
                    //if (updateOrderDispatched.CreatedDate.Year < 2021)
                    //{
                    updateOrderDispatched.EwayBillNumber = ewayBilldata.EwayBillNumber;
                    if (ewayBilldata.EwayBillFileUrl != null)
                    {
                        updateOrderDispatched.EwayBillFileUrl = "/images/EwayBillImages/" + ewayBilldata.EwayBillFileUrl;
                    }
                    updateOrderDispatched.UpdatedDate = indianTime;
                    context.Entry(updateOrderDispatched).State = EntityState.Modified;
                    if (context.Commit() > 0) { result = "Updated Successfully"; };
                    //}
                    //else
                    //{
                    //    #region   GenerateUpdateEwaybill
                    //    if (updateOrderDispatched.IRNNo != null)
                    //    {
                    //        GenerateUpdateEwaybillDc DatatoPost = new GenerateUpdateEwaybillDc();
                    //        DatatoPost.Irn = updateOrderDispatched.IRNNo;
                    //        DatatoPost.Distance = Convert.ToInt32(ewayBilldata.Distance);
                    //        DatatoPost.TransMode = ewayBilldata.TransMode;
                    //        DatatoPost.TransId = ewayBilldata.TransId;
                    //        DatatoPost.TransName = ewayBilldata.TransName;
                    //        DatatoPost.TransDocNo = ewayBilldata.TransDocNo;
                    //        DatatoPost.TransDocDt = ewayBilldata.TransDocDt;
                    //        DatatoPost.VehNo = ewayBilldata.VehNo;
                    //        DatatoPost.VehType = ewayBilldata.VehType;
                    //        var IsActiveEwayBillexist = context.OrderEwayBills.Any(x => x.OrderId == updateOrderDispatched.OrderId && x.IsCancelEwayBill == false && x.IsActive == false);
                    //        if (IsActiveEwayBillexist)
                    //        {
                    //            return result = " Please cancel previous EwayBil to new generate";
                    //        }
                    //        else
                    //        {
                    //            //var GenerateEwaybillresponse = GenerateEwaybill(DatatoPost);
                    //            //if (GenerateEwaybillresponse != null && GenerateEwaybillresponse.Success = "Y")
                    //            //{
                    //            //    OrderEwayBill InsertEwayBil = new OrderEwayBill();
                    //            //    InsertEwayBil.OrderId = updateOrderDispatched.OrderId;
                    //            //    InsertEwayBil.EwayBillNo = GenerateEwaybillresponse.EwayBillNo;
                    //            //    InsertEwayBil.EwayBillDate = GenerateEwaybillresponse.EwayBillDate;
                    //            //    InsertEwayBil.EwayBillValidTill = GenerateEwaybillresponse.EwayBillValidTill;
                    //            //    InsertEwayBil.IsCancelEwayBill = false;
                    //            //    InsertEwayBil.IsActive = true;
                    //            //    InsertEwayBil.CreateDate = indianTime;
                    //            //    InsertEwayBil.CreatedBy = userid;

                    //            //    if (context.Commit() > 0)
                    //            //    {
                    //            //        updateOrderDispatched.EwayBillNumber = GenerateEwaybillresponse.EwbNo;
                    //            //        updateOrderDispatched.EwayBillId = Convert.ToInt32(InsertEwayBil.EwayBillId);
                    //            //        updateOrderDispatched.UpdatedDate = indianTime;
                    //            //        context.Entry(updateOrderDispatched).State = EntityState.Modified;
                    //            //        if (context.Commit() > 0) { result = "Updated Successfully"; };
                    //            //    }
                    //            //}
                    //        }
                    //    }
                    //    else { result = "IRN No not exist"; }
                    //    #endregion
                    //}
                }
                else { result = "Something went wrong.."; }
                return result;
            }
        }



    }



    public class GenerateUpdateEwaybillDc
    {
        public int OrderDispatchedMasterId { get; set; }
        public int OrderId { get; set; }
        public string EwayBillNumber { get; set; }
        public string EwayBillFileUrl { get; set; }
        public string Irn { get; set; }
        public int Distance { get; set; }
        public string TransDocDt { get; set; }


        public string TransMode { get; set; }//
        public string TransId { get; set; }//  gst of Transpoter
        public string VehType { get; set; }//
        public string TransDocNo { get; set; }//
        public string TransName { get; set; }//
        public string VehNo { get; set; } //

    }







    public class orderMasterDTO
    {
        public ObjectId Id { get; set; }
        public Guid GUID { get; set; }
        public int? OrderDispatchedMasterId { get; set; }
        public string MongoId { get; set; }
        public int OrderId { get; set; }
        public string SalesPerson { get; set; }
        public string SalesMobile { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string Customerphonenum { get; set; }
        public string ShippingAddress { get; set; }

        [StringLength(300)]
        public string EwayBillNumber { get; set; }


    }
    public class EwayBilldataDTO
    {
        public int OrderId { get; set; }
        public int? OrderDispatchedMasterId { get; set; }
        public string EwayBillFileUrl { get; set; }

        [StringLength(300)]
        public string EwayBillNumber { get; set; }
    }

    public class EwayBillpaginationDTO
    {
        public int? Cityid { get; set; }
        public int? WarehouseId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool Active { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class PagginationData
    {
        public int total_count { get; set; }
        public dynamic ordermaster { get; set; }

        public dynamic Ewayorderlist { get; set; }

    }
}
