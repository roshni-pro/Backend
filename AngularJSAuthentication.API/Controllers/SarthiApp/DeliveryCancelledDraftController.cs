using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
//using System.Transactions;
using System.Web;
using System.Web.Http;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.DataContracts.BatchCode;
using static AngularJSAuthentication.API.Controllers.External.Other.SellerStoreController;
using Nito.AspNetBackgroundTasks;
using AngularJSAuthentication.DataContracts.Masters.Batch;
using System.Security.Claims;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Model.Gullak;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model.CashManagement;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;

namespace AngularJSAuthentication.API.Controllers
{

    [RoutePrefix("api/DeliveryCancelledDraft")]
    public class DeliveryCancelledDraftController : BaseApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<int> PocOrderInProcess = new List<int>();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetDeliveryCancelledAssignmentOrderList(int DeliveryIssuanceId, int WarehouseId)//get all 
        {
            bool status = false;
            string resultMessage = "";
            List<DeliveryCancelledDraftImageupload> FResult = new List<DeliveryCancelledDraftImageupload>();
            using (AuthContext context = new AuthContext())
            {
                string Query = "SELECT DI.DeliveryIssuanceId,DI.DisplayName,ODm.OrderId, ODM.OrderedDate,ODM.Status FROM DeliveryIssuances DI with(nolock) "
                                     + " inner JOIN OrderDeliveryMasters ODM with(nolock) ON DI.DeliveryIssuanceId = ODM.DeliveryIssuanceId "
                                     + " and DI.Status in ('Payment Submitted','Freezed') and ODM.Status = 'Delivery Canceled'  and  Di.WarehouseId=" + WarehouseId
                                     + " and  DI.DeliveryIssuanceId=" + DeliveryIssuanceId;
                var result = context.Database.SqlQuery<DeliveryCancelledDraftImageupload>(Query).ToList();
                if (result.Count > 0)
                {
                    string CheckQuery = "select OrderId from OrderDispatchedMasters with(nolock) where Status = 'Delivery Canceled' and  DeliveryIssuanceIdOrderDeliveryMaster=" + DeliveryIssuanceId;
                    var OrderIds = context.Database.SqlQuery<DeliveryCancelledOrderId>(CheckQuery).ToList();
                    if (OrderIds.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            if (OrderIds != null && OrderIds.Any(x => x.OrderId == item.OrderId))
                            {
                                string OrderItemQuery = "select odm.OrderId,odm.OrderDispatchedDetailsId,odm.ItemName,odm.ItemNumber,odm.Qty as DispatchedQty,DC.MatchedQty,cast((case when o.OrderType=8 then 1 else 0 end) as bit) IsClearance" +
                                    " from OrderDispatchedDetails odm with(nolock) inner join OrderMasters o with(nolock) on o.orderid=odm.OrderId "
                                     + " left join DeliveryCancelledOrderItems DC with(nolock) on odm.OrderDispatchedDetailsId = DC.OrderDispatchedDetailsId"
                                     + " where odm.OrderId =" + item.OrderId + " and odm.Qty > 0 group by odm.OrderId"
                                     + ", odm.OrderDispatchedDetailsId, odm.ItemName,odm.ItemNumber,odm.Qty,DC.MatchedQty,o.OrderType";
                                var OrderItemList = context.Database.SqlQuery<DeliveryCancelledOrderItemDTO>(OrderItemQuery).ToList();

                                if (OrderItemList.Sum(x => x.DispatchedQty) == OrderItemList.Sum(x => x.MatchedQty))
                                {
                                    item.IsPosted = true;
                                }
                                else
                                {
                                    item.IsPosted = false;
                                }
                                item.DeliveryCancelledOrderItem = OrderItemList;
                                FResult.Add(item);
                            }
                        }
                    }
                    else
                    {
                        status = true; resultMessage = "no record found";
                    }
                    status = true; resultMessage = "Record found";
                }
                else
                {
                    status = false; resultMessage = "no record found";
                }
                //var finalList = FResult.SelectMany(x => x.DeliveryCancelledOrderItem.Where(y => y.DispatchedQty > 0)).ToList();
                var res = new
                {
                    DeliveryCancelledDraftImageupload = result,
                    status = status,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }

        [Route("OrderDetailBarcode/{OrderId}")]
        [HttpGet]
        public async Task<List<ItemBarcodeDc>> GetOrderDetailBarcode(int OrderId)
        {
            List<ItemBarcodeDc> ItemBarcodes = new List<ItemBarcodeDc>();

            if (OrderId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter("@OrderId", OrderId);
                    ItemBarcodes = context.Database.SqlQuery<ItemBarcodeDc>("exec [BatchCode].[GetOrderDetailBarcodeList] @OrderId ", param).ToList();
                }
            }
            return ItemBarcodes;
        }

        [Route("Image")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage postDeliveryCancelledDraft()
        {

            string ImageUrl = string.Empty;
            bool status = false;
            string resultMessage = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/DeliveryCancelledDraft/"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(ImageUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/DeliveryCancelledDraft/", ImageUrl);

                    status = true;
                    resultMessage = " file uploaded successfully.";
                    ImageUrl = "/images/DeliveryCancelledDraft/" + httpPostedFile.FileName;
                }
            }
            else
            {
                ImageUrl = "";
                status = false;
                resultMessage = "some thing went wrong";
            }
            var res = new
            {
                ImageUrl = ImageUrl,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("GetOrderImage")]
        [HttpGet]
        public async Task<DeliveryCancelledDraft> GetOrderImage(int OrderId)
        {
            using (var context = new AuthContext())
            {
                string Query = "select *from DeliveryCancelledDrafts where IsActive = 1 and IsDeleted = 0 and OrderId=" + OrderId;
                var result = await context.Database.SqlQuery<DeliveryCancelledDraft>(Query).SingleAsync();
                return result;
            }
        }

        #region//Add DeliveryCancelledOrderItem      
        [Route("AddDeliveryCancelledOrderItem")]
        [HttpPost]
        public HttpResponseMessage AddDeliveryCancelledOrderItem(DeliveryCancelledDraftImageupload DeliveryCancelledOrderItem)
        {
            bool IsValidate = true;
            bool IsValidateQTy = true;
            string resultMessage = "";
            using (AuthContext context = new AuthContext())
            {
                //using (var dbContextTransaction = context.Database.BeginTransaction())
                //{
                if (DeliveryCancelledOrderItem != null && DeliveryCancelledOrderItem.DeliveryCancelledOrderItem.Any())
                {
                    int peopleId = DeliveryCancelledOrderItem.UserId;
                    People _people = context.Peoples.Where(q => q.PeopleID == peopleId && q.Active == true).SingleOrDefault();
                    Int32 OrderId = DeliveryCancelledOrderItem.DeliveryCancelledOrderItem[0].OrderId;
                    Int32 DeliveryIssuanceId = DeliveryCancelledOrderItem.DeliveryIssuanceId;
                    var _OrderDispatchedMaster = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId && x.DeliveryIssuanceIdOrderDeliveryMaster == DeliveryIssuanceId).Include("orderDetails").SingleOrDefault();
                    var _OrderDispatchedDetails = _OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0).ToList();

                    #region SalesReturn

                    var Ordermaster = context.DbOrderMaster.Where(x => x.OrderId == OrderId && x.OrderType == 3).FirstOrDefault();
                    if (Ordermaster != null)
                    {
                        bool IsMismatch = false;
                        var OrderDispatchedDetailsId = DeliveryCancelledOrderItem.DeliveryCancelledOrderItem.Select(x => x.OrderDispatchedDetailsId).Distinct().ToList();
                        var OrderDetailsIds = _OrderDispatchedDetails.Where(x => OrderDispatchedDetailsId.Contains(x.OrderDispatchedDetailsId)).Select(x => x.OrderDetailsId).ToList();
                        var KKReturnDetails = context.KKReturnReplaceDetails.Where(x => OrderDetailsIds.Contains(x.NewOrderDetailsId)).ToList();

                        foreach (var x in DeliveryCancelledOrderItem.DeliveryCancelledOrderItem)
                        {
                            var item = _OrderDispatchedMaster.orderDetails.Where(a => a.OrderDispatchedDetailsId == x.OrderDispatchedDetailsId).SingleOrDefault();
                            var KKReturnDetailsData = KKReturnDetails.Where(y => y.NewOrderDetailsId == item.OrderDetailsId && y.Qty > 0).ToList();

                            foreach (var y in x.DeliveryCancelledOrderItemBatches)
                            {
                                var KKReturnQty = KKReturnDetailsData.Where(z => z.BatchId == y.BatchMasterId).FirstOrDefault().Qty;
                                if (y.Qty != KKReturnQty)
                                {
                                    resultMessage = "Batchcode quantity not matched with actual quantity for Batch : " + KKReturnDetailsData.Where(z => z.BatchId == y.BatchMasterId).FirstOrDefault().BatchCode;
                                    IsMismatch = true;
                                    break;
                                }
                            }
                        }
                        if (IsMismatch)
                        {
                            var result = new
                            {
                                status = false,
                                Message = resultMessage
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, result);
                        }
                    }

                    #endregion
                    if (_OrderDispatchedMaster != null && _people != null && (_OrderDispatchedMaster.Status == "Delivery Canceled") && _OrderDispatchedDetails.Any())
                    {
                        if (_OrderDispatchedDetails.Count == DeliveryCancelledOrderItem.DeliveryCancelledOrderItem.Count)
                        {
                            int _TotalDispatchedQty = _OrderDispatchedDetails.Sum(x => x.qty);
                            int? TotalMatchedRecQty = DeliveryCancelledOrderItem.DeliveryCancelledOrderItem.Sum(x => x.MatchedQty);
                            if (_TotalDispatchedQty < TotalMatchedRecQty)
                            {
                                resultMessage = "Matched Quantity should be Equal to Total Dispatched Qty ";
                                IsValidateQTy = false;
                            }
                            foreach (var a in DeliveryCancelledOrderItem.DeliveryCancelledOrderItem)
                            {
                                var item = _OrderDispatchedMaster.orderDetails.Where(x => x.OrderDispatchedDetailsId == a.OrderDispatchedDetailsId).SingleOrDefault();
                                if (item.qty == a.MatchedQty)
                                {
                                    IsValidateQTy = true;
                                }
                                else
                                {
                                    resultMessage = "Insert Quantity should be Equal to Total Dispatched Qty : " + item.itemname;
                                    IsValidateQTy = false;
                                    break;
                                }
                            }

                            if (IsValidateQTy && IsValidate)
                            {

                                bool result = InsertForPocReceived(DeliveryCancelledOrderItem, _people, context);
                                if (result)
                                {
                                    //dbContextTransaction.Commit();
                                    resultMessage = "Posted Succesfully";
                                    IsValidate = true;
                                }
                                else
                                {
                                    //dbContextTransaction.Rollback();
                                    resultMessage = "Something went wrong";
                                    IsValidate = false;
                                }
                            }
                        }
                        else
                        {
                            resultMessage = "Dispatched Qty and Return Qty not Matched";
                            IsValidate = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Can't Posted due to status in " + _OrderDispatchedMaster.Status;
                        IsValidate = false;
                    }

                }
                else
                {
                    resultMessage = "Order Item list is null";
                    IsValidate = false;
                }
                // }
                var res = new
                {

                    status = IsValidate,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        public bool InsertForPocReceived(DeliveryCancelledDraftImageupload DeliveryCancelledOrderItem, People people, AuthContext context)
        {
            List<DeliveryCancelledOrderItem> AddDeliveryCancelledOrderItem = new List<DeliveryCancelledOrderItem>();// 
            List<DeliveryCancelledItemBatch> AddDeliveryCancelledItemBatchs = new List<DeliveryCancelledItemBatch>();// 


            foreach (var OrderItem in DeliveryCancelledOrderItem.DeliveryCancelledOrderItem)
            {
                DeliveryCancelledOrderItem item = new DeliveryCancelledOrderItem();
                item.OrderDispatchedDetailsId = OrderItem.OrderDispatchedDetailsId;
                item.DispatchedQty = OrderItem.DispatchedQty;
                item.MatchedQty = OrderItem.MatchedQty;
                item.IsPoc = false;
                item.IsActive = false;
                item.IsDeleted = false;
                item.OrderId = OrderItem.OrderId;
                item.CreatedDate = indianTime;
                item.ModifiedDate = indianTime;
                item.CreatedBy = people.PeopleID;
                #region Batch code
                if (OrderItem.DeliveryCancelledOrderItemBatches != null && OrderItem.DeliveryCancelledOrderItemBatches.Any())
                {
                    OrderItem.DeliveryCancelledOrderItemBatches.ForEach(dcitem =>
                      AddDeliveryCancelledItemBatchs.Add(
                          new DeliveryCancelledItemBatch
                          {
                              Qty = dcitem.Qty,
                              BatchMasterId = dcitem.BatchMasterId,
                              OrderDispatchedDetailsId = OrderItem.OrderDispatchedDetailsId
                          }));
                }
                #endregion
                AddDeliveryCancelledOrderItem.Add(item);
            }
            DeliveryCancelledDraft DeliveryCancelledDraft = new DeliveryCancelledDraft();
            DeliveryCancelledDraft.OrderId = DeliveryCancelledOrderItem.OrderId;
            DeliveryCancelledDraft.CreatedBy = people.PeopleID;
            DeliveryCancelledDraft.ModifiedDate = indianTime;
            DeliveryCancelledDraft.CreatedDate = indianTime;
            DeliveryCancelledDraft.OrderItemImage = DeliveryCancelledOrderItem.Imageupload;
            DeliveryCancelledDraft.OrderInvoiceImage = DeliveryCancelledOrderItem.InvoiceImage;
            DeliveryCancelledDraft.IsActive = true;
            DeliveryCancelledDraft.IsDeleted = false;
            context.DeliveryCancelledDraftDB.Add(DeliveryCancelledDraft);
            context.DeliveryCancelledOrderItemDb.AddRange(AddDeliveryCancelledOrderItem);
            context.DeliveryCancelledItemBatchs.AddRange(AddDeliveryCancelledItemBatchs);

            if (context.Commit() > 0) { return true; } else { return false; }
        }
        #endregion

        #region get and do Approved Poc
        [Route("GetUnApprovedPoc")]
        [HttpGet]
        public HttpResponseMessage GetUnApprovedPocData(int WarehouseId)//GetUnApprovedPocData
        {
            bool status = false;
            string resultMessage = "";
            List<DeliveryCancelledOrderDTO> _result = new List<DeliveryCancelledOrderDTO>();
            using (AuthContext context = new AuthContext())
            {
                string CheckQuery = "select OrderId from OrderDispatchedMasters where Status = 'Delivery Canceled' and  WarehouseId=" + WarehouseId;
                var OrderIdsList = context.Database.SqlQuery<DeliveryCancelledOrderId>(CheckQuery).ToList();

                if (OrderIdsList.Count > 0)
                {
                    if (OrderIdsList != null && OrderIdsList.Any())
                    {
                        var OrderIds = string.Join(",", OrderIdsList.Select(x => x.OrderId));

                        //string OrderItemQuery = "select odm.OrderId,odm.OrderDispatchedDetailsId,odm.ItemName,odm.ItemNumber,odm.Qty as DispatchedQty,DC.MatchedQty"
                        //                  + " from OrderDispatchedDetails odm left join DeliveryCancelledOrderItems DC on odm.OrderDispatchedDetailsId = DC.OrderDispatchedDetailsId "
                        //                   //+ " left join ItemMasterCentrals IMC on odm.itemNumber = IMC.Number "
                        //                   + " where DC.OrderId in(" + OrderIds + ")  and DC.IsPoc = 0 group by odm.OrderId,odm.OrderDispatchedDetailsId, odm.ItemName,odm.ItemNumber,odm.Qty,DC.MatchedQty ";

                        string OrderItemQuery = "select odm.OrderId,odm.OrderDispatchedDetailsId,odm.ItemName,odm.ItemNumber,odm.Qty as DispatchedQty,DC.MatchedQty, cast((case when o.OrderType=8 then 1 else 0 end) as bit) IsClearance"
                                          + " from OrderDispatchedDetails odm inner join OrderMasters o on o.orderid=odm.OrderId left join DeliveryCancelledOrderItems DC on odm.OrderDispatchedDetailsId = DC.OrderDispatchedDetailsId "
                                           //+ " left join ItemMasterCentrals IMC on odm.itemNumber = IMC.Number "
                                           + " where DC.OrderId in(" + OrderIds + ")  and DC.IsPoc = 0 group by odm.OrderId,odm.OrderDispatchedDetailsId, odm.ItemName,odm.ItemNumber,odm.Qty,DC.MatchedQty,o.OrderType ";
                        var OrderItemList = context.Database.SqlQuery<DeliveryCancelledOrderItemDTO>(OrderItemQuery).ToList();

                        _result = OrderItemList.GroupBy(x => new
                        {
                            x.OrderId,
                        }).Select(item => new DeliveryCancelledOrderDTO
                        {
                            OrderId = item.Key.OrderId,
                            DeliveryCancelledOrderItem = item.Select(a => new DeliveryCancelledOrderItemDTO
                            {
                                OrderDispatchedDetailsId = a.OrderDispatchedDetailsId,
                                OrderId = a.OrderId,
                                DispatchedQty = a.DispatchedQty,
                                ItemName = a.ItemName,
                                ItemNumber = a.ItemNumber,
                                MatchedQty = a.MatchedQty,
                                Barcode = a.Barcode,
                                IsClearance = a.IsClearance,
                                DeliveryCancelledOrderItemBatches = GetDeliveryCancelledOrderItemBatches(context, a.OrderDispatchedDetailsId)
                            }).ToList()
                        }).ToList();
                    }
                }
                else
                {
                    status = true;
                    resultMessage = "no record found";
                }
                status = true;
                resultMessage = "Record found";
                var res = new
                {
                    DeliveryCancelledOrderDTO = _result,
                    status = status,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        public List<DeliveryCancelledItemBatchDc> GetDeliveryCancelledOrderItemBatches(AuthContext context, int OrderDispatchedDetailsId)
        {
            var result = new List<DeliveryCancelledItemBatchDc>();
            var param = new SqlParameter("OrderDispatchedDetailsId", OrderDispatchedDetailsId);
            result = context.Database.SqlQuery<DeliveryCancelledItemBatchDc>("exec BatchCode.GetDCOrderItemBatches @OrderDispatchedDetailsId", param).ToList();
            return result;
        }


        [Route("AddApprovedDoPoc")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> AddApprovedAndDoPoc(DeliveryCancelledOrderDTO DeliveryCancelledOrderDTO)
        {
            string resultMessage = "";
            bool IsValidateQTy = true;
            bool status = true;
            RewardPoint _rpoint = null;
            Wallet _Wallet = null;
            People _people = null;
            //if (PocOrderInProcess != null && PocOrderInProcess.Any(x => x == DeliveryCancelledOrderDTO.OrderId))
            //{
            //    resultMessage = "Already in Process..";
            //    var resa = new
            //    {
            //        status = status,
            //        Message = resultMessage
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, resa);
            //}
            //else
            //{
            //    PocOrderInProcess.Add(DeliveryCancelledOrderDTO.OrderId);
            //}

            try
            {
                OrderOutPublisher Publisher = new OrderOutPublisher();
                List<BatchCodeSubjectDc> PublisherPocStockList = new List<BatchCodeSubjectDc>();

                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        int UserId = DeliveryCancelledOrderDTO.UserId;
                        _people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
                        var _OrderDispatchedMaster = context.OrderDispatchedMasters.Where(x => x.OrderId == DeliveryCancelledOrderDTO.OrderId).Include("orderDetails").FirstOrDefault();

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == _OrderDispatchedMaster.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                        {
                            resultMessage = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                            var resa = new
                            {
                                status = status,
                                Message = resultMessage
                            };

                            //PocOrderInProcess.RemoveAll(x => x == DeliveryCancelledOrderDTO.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, resa);

                        }


                        var batchCodeIds = DeliveryCancelledOrderDTO.DeliveryCancelledOrderItem.SelectMany(x => x.DeliveryCancelledOrderItemBatches.Select(y => y.BatchMasterId)).ToList();
                        var batchMastersList = context.BatchMasters.Where(x => batchCodeIds.Contains(x.Id) && x.IsActive == true && x.IsDeleted == false).ToList();

                        if (_OrderDispatchedMaster != null && _people != null && _OrderDispatchedMaster.Status == "Delivery Canceled")
                        {
                            int _TotalDispatchedQty = _OrderDispatchedMaster.orderDetails.Sum(x => x.qty);
                            int? TotalValidateQty = DeliveryCancelledOrderDTO.DeliveryCancelledOrderItem.Sum(x => x.ValidateQty);
                            if (_TotalDispatchedQty < TotalValidateQty)
                            {
                                resultMessage = "Insert Quantity should be Equal to Total Dispatched Qty";
                                IsValidateQTy = false;
                            }
                            foreach (var a in DeliveryCancelledOrderDTO.DeliveryCancelledOrderItem)
                            {
                                var item = _OrderDispatchedMaster.orderDetails.Where(x => x.OrderDispatchedDetailsId == a.OrderDispatchedDetailsId).FirstOrDefault();
                                if (item.qty == a.ValidateQty)
                                {
                                    IsValidateQTy = true;
                                    if (!batchMastersList.Any(x => x.ItemNumber == item.itemNumber && a.DeliveryCancelledOrderItemBatches.Select(y => y.BatchMasterId).ToList().Contains(x.Id)))
                                    {
                                        resultMessage = "Please Select Correct Batch Code :" + item.itemname;
                                        IsValidateQTy = false;
                                        status = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    resultMessage = "Insert Quantity should be Equal to Total Dispatched Qty : " + item.itemname;
                                    IsValidateQTy = false;
                                    status = false;
                                    break;
                                }
                            }
                            if (IsValidateQTy && status)
                            {
                                var _OrderMaster = context.DbOrderMaster.Where(x => x.OrderId == _OrderDispatchedMaster.OrderId).Include("orderDetails").SingleOrDefault();//OrderMaster

                                #region Zaruri order
                                if (_OrderMaster.OrderType == 5)
                                {
                                    _OrderMaster.Status = "Post Order Canceled";
                                    _OrderDispatchedMaster.Status = "Post Order Canceled";


                                    _OrderMaster.UpdatedDate = indianTime;
                                    context.Entry(_OrderMaster).State = EntityState.Modified;

                                    _OrderDispatchedMaster.UpdatedDate = indianTime;
                                    context.Entry(_OrderDispatchedMaster).State = EntityState.Modified;

                                    #region Order Master History for Status Post Order Canceled  

                                    OrderMasterHistories h1 = new OrderMasterHistories();

                                    h1.orderid = _OrderDispatchedMaster.OrderId;
                                    h1.Status = _OrderDispatchedMaster.Status;
                                    h1.Reasoncancel = null;
                                    h1.Warehousename = _OrderDispatchedMaster.WarehouseName;
                                    h1.username = _people.DisplayName;
                                    h1.userid = _people.PeopleID;
                                    h1.CreatedDate = indianTime;
                                    context.OrderMasterHistoriesDB.Add(h1);

                                    if (context.Commit() > 0)
                                    {
                                        dbContextTransaction.Complete();
                                        try
                                        {
                                            UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                            {
                                                CartStatus = "Cancelled",
                                                InvoiceNo = _OrderDispatchedMaster.invoice_no
                                            };
                                            var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/ShoppingCart/SKUpdateCartStatus";
                                            using (var client = new HttpClient())
                                            {
                                                client.DefaultRequestHeaders.Add("CustomerId", "1");
                                                client.DefaultRequestHeaders.Add("NoEncryption", "1");
                                                var newJson = JsonConvert.SerializeObject(updateConsumerOrders);
                                                using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                                                {
                                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                                                    response.EnsureSuccessStatusCode();
                                                    string responseBody = response.Content.ReadAsStringAsync().Result;
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            TextFileLogHelper.LogError("Error while Update cart status in Trade: " + ex.ToString());
                                        }
                                        resultMessage = "Approved Succesfully";
                                        status = true;
                                        var resa = new
                                        {
                                            status = status,
                                            Message = resultMessage
                                        };

                                        //PocOrderInProcess.RemoveAll(x => x == DeliveryCancelledOrderDTO.OrderId);

                                        return Request.CreateResponse(HttpStatusCode.OK, resa);
                                    }

                                    #endregion
                                }
                                #endregion
                                if (_OrderMaster.RewardPoint > 0)
                                {
                                    _rpoint = context.RewardPointDb.Where(c => c.CustomerId == _OrderDispatchedMaster.CustomerId).FirstOrDefault();//EarnPoint
                                }
                                if (_OrderMaster.walletPointUsed > 0)
                                {
                                    _Wallet = context.WalletDb.Where(x => x.CustomerId == _OrderDispatchedMaster.CustomerId).SingleOrDefault(); //Wallet
                                }
                                var _DeliveryCancelledOrderItem = context.DeliveryCancelledOrderItemDb.Where(x => x.OrderId == _OrderDispatchedMaster.OrderId).ToList();

                                List<int> OrderDisDetailids = new List<int>();

                                if (_OrderDispatchedMaster.Status == _OrderMaster.Status && _OrderMaster.Status == "Delivery Canceled" && _OrderDispatchedMaster.Status == "Delivery Canceled")
                                {
                                    #region New BatchMasterId update
                                    if (_OrderMaster.OrderType != 3) // Not for Return order
                                    {
                                        OrderDisDetailids = DeliveryCancelledOrderDTO.DeliveryCancelledOrderItem.Select(x => x.OrderDispatchedDetailsId).Distinct().ToList();
                                        var DeliveryCancelledItemBatches = context.DeliveryCancelledItemBatchs.Where(x => OrderDisDetailids.Contains(x.OrderDispatchedDetailsId)).ToList();

                                        foreach (var item in DeliveryCancelledItemBatches.GroupBy(c => c.OrderDispatchedDetailsId).Select(x => new { OrderDispatchedDetailsId = x.Key, qty = x.Sum(y => y.Qty) }).ToList())
                                        {
                                            foreach (var dto in DeliveryCancelledOrderDTO.DeliveryCancelledOrderItem.Where(x => x.OrderDispatchedDetailsId == item.OrderDispatchedDetailsId).ToList())
                                            {
                                                var dtolist = dto.DeliveryCancelledOrderItemBatches.ToList();
                                                var batchlist = dtolist.Select(x => x.BatchMasterId).Distinct().ToList();

                                                //var totQty = dtolist.Sum(x => x.Qty);
                                                //int totQty = totalqty.FirstOrDefault(x => x.OrderDispatchedDetailsId == item.OrderDispatchedDetailsId).qty;
                                                //var DeliveryCancelledData = DeliveryCancelledItemBatches.Where(x => x.OrderDispatchedDetailsId == item.OrderDispatchedDetailsId).ToList();

                                                //var validQty = DeliveryCancelledData.Sum(x => x.Qty);

                                                var deleteBatch = DeliveryCancelledItemBatches.Where(x => x.OrderDispatchedDetailsId == dto.OrderDispatchedDetailsId && !batchlist.Contains(x.BatchMasterId)).ToList();

                                                if (deleteBatch.Count > 0)
                                                {
                                                    foreach (var b in deleteBatch)
                                                    {
                                                        b.Qty = 0;
                                                        context.Entry(b).State = EntityState.Modified;
                                                    }
                                                }

                                                if (item.qty == dto.ValidateQty)
                                                {
                                                    foreach (var ItemBatches in dtolist)
                                                    {
                                                        var BatchExists = DeliveryCancelledItemBatches.Where(x => x.BatchMasterId == ItemBatches.BatchMasterId && x.OrderDispatchedDetailsId == item.OrderDispatchedDetailsId).FirstOrDefault();
                                                        if (BatchExists == null)
                                                        {
                                                            DeliveryCancelledItemBatch obj = new DeliveryCancelledItemBatch();
                                                            obj.BatchMasterId = ItemBatches.BatchMasterId;
                                                            obj.OrderDispatchedDetailsId = item.OrderDispatchedDetailsId;
                                                            obj.Qty = ItemBatches.Qty;
                                                            context.DeliveryCancelledItemBatchs.Add(obj);
                                                        }
                                                        else
                                                        {
                                                            BatchExists.Qty = ItemBatches.Qty;
                                                            context.Entry(BatchExists).State = EntityState.Modified;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    resultMessage = "Insert Quantity should be Equal to Total Dispatched Qty";
                                                    IsValidateQTy = false;
                                                    status = false;
                                                    var resa = new
                                                    {
                                                        status = status,
                                                        Message = resultMessage
                                                    };
                                                    return Request.CreateResponse(HttpStatusCode.OK, resa);
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    _OrderDispatchedMaster.Status = "Post Order Canceled";
                                    _OrderDispatchedMaster.UpdatedDate = indianTime;
                                    context.Entry(_OrderDispatchedMaster).State = EntityState.Modified;

                                    _OrderMaster.Status = "Post Order Canceled";
                                    _OrderMaster.UpdatedDate = indianTime;
                                    context.Entry(_OrderMaster).State = EntityState.Modified;

                                    context.Commit();

                                    var result = await ApprovedAndDOPoc(_DeliveryCancelledOrderItem, _OrderDispatchedMaster, _OrderMaster, _rpoint, _Wallet, _people, context, dbContextTransaction, warehouse);
                                    if (!result.IsSuccess)
                                    {
                                        result.BatchCodeSubjects = null;
                                        dbContextTransaction.Dispose();
                                        status = false;
                                        resultMessage = "Something went wrong";
                                    }
                                    else
                                    {
                                        //gullak reference generate for sales return order

                                        #region gullak reference generate for sales return order
                                        if (_OrderMaster.OrderType == 3)
                                        {
                                            double totalBillDicount = 0;
                                            int porderid = Convert.ToInt32(_OrderMaster.ParentOrderId);
                                            var parentorderdata = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == porderid);
                                            #region wallet point return
                                            var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + porderid + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + porderid + " group by orderid";
                                            List<InvoiceOrderOffer> invoiceOrderOffers = context.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
                                            if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
                                            {
                                                foreach (var item in invoiceOrderOffers)
                                                {
                                                    if (item.BillDiscountAmount > 0)
                                                        totalBillDicount += item.BillDiscountAmount;
                                                    else
                                                        totalBillDicount += item.BillDiscountTypeValue;
                                                }
                                            }

                                            var wallet = context.WalletDb.FirstOrDefault(x => x.CustomerId == _OrderMaster.CustomerId);
                                            if (wallet != null)
                                            {
                                                if (_OrderMaster.WalletAmount > 0)
                                                    wallet.TotalAmount = wallet.TotalAmount + _OrderMaster.WalletAmount;
                                                if (totalBillDicount>0)
                                                    wallet.TotalAmount = wallet.TotalAmount - totalBillDicount;
                                                wallet.UpdatedDate = DateTime.Now;
                                                context.Entry(wallet).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                context.WalletDb.Add(new Wallet
                                                {
                                                    Skcode = _OrderMaster.Skcode,
                                                    CustomerId = _OrderMaster.CustomerId,
                                                    TotalAmount = _OrderMaster.WalletAmount ?? 0 - totalBillDicount,
                                                    CreatedDate = indianTime,
                                                    UpdatedDate = indianTime,
                                                    Deleted = false,
                                                    IsNotExpirable = true,
                                                });
                                            }
                                            if(_OrderMaster.WalletAmount > 0)
                                            {
                                                CustomerWalletHistory obj = new CustomerWalletHistory();
                                                obj.CustomerId = _OrderMaster.CustomerId;
                                                obj.WarehouseId = (int)_OrderMaster.WarehouseId;
                                                obj.OrderId = _OrderMaster.OrderId;
                                                obj.NewAddedWAmount = _OrderMaster.WalletAmount;
                                                obj.TotalWalletAmount += obj.NewAddedWAmount;
                                                obj.Through = "Backend Return order ";
                                                obj.TransactionDate = DateTime.Now;
                                                obj.CreatedDate = DateTime.Now;
                                                context.CustomerWalletHistoryDb.Add(obj);
                                            }
                                            if(totalBillDicount > 0)
                                            {
                                                CustomerWalletHistory obj = new CustomerWalletHistory();
                                                obj.CustomerId = _OrderMaster.CustomerId;
                                                obj.WarehouseId = (int)_OrderMaster.WarehouseId;
                                                obj.OrderId = _OrderMaster.OrderId;
                                                obj.NewAddedWAmount = (-1) * totalBillDicount;
                                                obj.TotalWalletAmount += obj.NewAddedWAmount;
                                                obj.Through = "Backend Return order ";
                                                obj.TransactionDate = DateTime.Now;
                                                obj.CreatedDate = DateTime.Now;
                                                context.CustomerWalletHistoryDb.Add(obj);
                                            }
                                            
                                            #endregion

                                            var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == _OrderDispatchedMaster.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                            if (customerGullak == null)
                                            {
                                                context.GullakDB.Add(new Gullak
                                                {
                                                    CustomerId = _OrderDispatchedMaster.CustomerId,
                                                    TotalAmount = _OrderMaster.GrossAmount,
                                                    CreatedDate = DateTime.Now,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    CreatedBy = UserId
                                                });
                                            }
                                            else
                                            {
                                                customerGullak.TotalAmount += _OrderMaster.GrossAmount;
                                                context.Entry(customerGullak).State = EntityState.Modified;
                                            }

                                            if (parentorderdata != null && parentorderdata.paymentThrough.ToLower() == "paylater")
                                            {
                                                PayLaterCollection payLaterCollection = new PayLaterCollection();
                                                List<PayLaterCollectionHistory> payLaterCollectionHistory = new List<PayLaterCollectionHistory>();
                                                int parentorderid = Convert.ToInt32(parentorderdata.OrderId);
                                                payLaterCollection = context.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == parentorderid);
                                                payLaterCollectionHistory = context.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();

                                                double returnamount = payLaterCollectionHistory.Any(x => x.Amount > 0 && x.Comment == "Return Order") ? payLaterCollectionHistory.Where(x => x.Amount > 0 && x.Comment == "Return Order").Sum(y => y.Amount) : 0;
                                                double paidamount = payLaterCollectionHistory.Any(x => x.Amount > 0 && x.Comment == null || (x.Comment != "Return Order" && x.Comment != "Gullak Return Order")) ? payLaterCollectionHistory.Where(x => x.Amount > 0 && x.Comment == null || (x.Comment != "Return Order" && x.Comment != "Gullak Return Order")).Sum(y => y.Amount) : 0;
                                                double gullakreturnamount = payLaterCollectionHistory.Any(x => x.Amount > 0 && x.Comment == "Gullak Refund") ? payLaterCollectionHistory.Where(x => x.Amount > 0 && x.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                                                double refundamount = Math.Round(_OrderDispatchedMaster.TotalAmount, 0);

                                                if (paidamount > 0)
                                                {
                                                    returnamount = returnamount > 0 ? returnamount : 0;
                                                    gullakreturnamount = gullakreturnamount > 0 ? gullakreturnamount : 0;
                                                    double amounts = paidamount - (payLaterCollection.Amount - returnamount - refundamount) - gullakreturnamount;
                                                    if (amounts > 0)
                                                    {
                                                        double amount = refundamount - amounts;
                                                        //int OrderId = DeliveryCancelledOrderDTO.OrderId;

                                                        PayLaterCollectionHistory ph = new PayLaterCollectionHistory();
                                                        ph.PayLaterCollectionId = payLaterCollection.Id;
                                                        ph.Amount = refundamount;
                                                        ph.PaymentMode = "Return";
                                                        ph.CreatedDate = DateTime.Now;
                                                        ph.IsActive = true;
                                                        ph.IsDeleted = false;
                                                        ph.CreatedBy = UserId;
                                                        ph.Comment = "Return Order";
                                                        ph.PaymentStatus = 1;
                                                        context.PayLaterCollectionHistoryDb.Add(ph);

                                                        PayLaterCollectionHistory phh = new PayLaterCollectionHistory();
                                                        phh.PayLaterCollectionId = payLaterCollection.Id;
                                                        phh.Amount = amounts;
                                                        phh.PaymentMode = "Gullak";
                                                        phh.CreatedDate = DateTime.Now;
                                                        phh.IsActive = true;
                                                        phh.IsDeleted = false;
                                                        phh.CreatedBy = UserId;
                                                        phh.Comment = "Gullak Refund";
                                                        phh.PaymentStatus = 1;
                                                        context.PayLaterCollectionHistoryDb.Add(phh);
                                                        if (amount > 0)
                                                        {
                                                            PaymentResponseRetailerApp paymentResponse = new PaymentResponseRetailerApp();
                                                            paymentResponse.amount = (-1) * amount;
                                                            paymentResponse.currencyCode = "INR";
                                                            paymentResponse.status = "Success";
                                                            paymentResponse.statusDesc = "Sales Return Order";
                                                            paymentResponse.PaymentFrom = "Gullak";
                                                            paymentResponse.OrderId = parentorderid;
                                                            paymentResponse.GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                                                            paymentResponse.GatewayOrderId = _OrderDispatchedMaster.CustomerId.ToString();
                                                            paymentResponse.CreatedDate = DateTime.Now;
                                                            paymentResponse.UpdatedDate = DateTime.Now;
                                                            context.PaymentResponseRetailerAppDb.Add(paymentResponse);
                                                            context.Commit();
                                                        }



                                                        PaymentResponseRetailerApp paymentResp = new PaymentResponseRetailerApp();
                                                        paymentResp.amount = (-1) * amounts;
                                                        paymentResp.currencyCode = "INR";
                                                        paymentResp.status = "Success";
                                                        paymentResp.statusDesc = "Gullak Return Order";
                                                        paymentResp.PaymentFrom = "Gullak";
                                                        paymentResp.OrderId = parentorderid;
                                                        paymentResp.GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                                                        paymentResp.GatewayOrderId = _OrderDispatchedMaster.CustomerId.ToString();
                                                        paymentResp.CreatedDate = DateTime.Now;
                                                        paymentResp.UpdatedDate = DateTime.Now;
                                                        context.PaymentResponseRetailerAppDb.Add(paymentResp);
                                                        context.Commit();

                                                        var Idparam = new SqlParameter("@PaymentRefundRequestId", paymentResp.id);
                                                        var OrderIdparam = new SqlParameter("@OrderId", parentorderid);
                                                        var refundamountparam = new SqlParameter("@refundamount", amounts);
                                                        int count = context.Database.ExecuteSqlCommand("exec GullakPaymentRefundForSalesReturnOrder @PaymentRefundRequestId,@OrderId,@refundamount ", Idparam, OrderIdparam, refundamountparam);

                                                        // Return Order Request Completed
                                                        var kkreturnReq = context.KKReturnReplaceRequests.Where(x => x.NewGeneratedOrderId == _OrderMaster.OrderId && x.Status == "Pending" && x.IsActive == true && x.IsDeleted == false).Include(x => x.Details).ToList();
                                                        if (kkreturnReq.Any())
                                                        {
                                                            foreach (var req in kkreturnReq)
                                                            {
                                                                req.Status = "Completed";
                                                                foreach (var d in req.Details.Where(x => x.Status == 6).ToList())
                                                                {
                                                                    d.Status = 7;  // for POC completed
                                                                }
                                                                context.Entry(req).State = EntityState.Modified;
                                                            }
                                                        }
                                                        context.Commit();
                                                    }
                                                    else
                                                    {
                                                        PayLaterCollectionHistory ph = new PayLaterCollectionHistory();
                                                        ph.PayLaterCollectionId = payLaterCollection.Id;
                                                        ph.Amount = refundamount;
                                                        ph.PaymentMode = "Return";
                                                        ph.CreatedDate = DateTime.Now;
                                                        ph.IsActive = true;
                                                        ph.IsDeleted = false;
                                                        ph.CreatedBy = UserId;
                                                        ph.Comment = "Return Order";
                                                        ph.PaymentStatus = 1;
                                                        context.PayLaterCollectionHistoryDb.Add(ph);
                                                        var kkreturnReq = context.KKReturnReplaceRequests.Where(x => x.NewGeneratedOrderId == _OrderMaster.OrderId && x.Status == "Pending" && x.IsActive == true && x.IsDeleted == false).Include(x => x.Details).ToList();
                                                        if (kkreturnReq.Any())
                                                        {
                                                            foreach (var req in kkreturnReq)
                                                            {
                                                                req.Status = "Completed";
                                                                foreach (var d in req.Details.Where(x => x.Status == 6).ToList())
                                                                {
                                                                    d.Status = 7;  // for POC completed
                                                                }
                                                                context.Entry(req).State = EntityState.Modified;
                                                            }
                                                        }
                                                        context.Commit();
                                                    }
                                                }
                                                else
                                                {
                                                    PayLaterCollectionHistory ph = new PayLaterCollectionHistory();
                                                    ph.PayLaterCollectionId = payLaterCollection.Id;
                                                    ph.Amount = refundamount;
                                                    ph.PaymentMode = "Return";
                                                    ph.CreatedDate = DateTime.Now;
                                                    ph.IsActive = true;
                                                    ph.IsDeleted = false;
                                                    ph.CreatedBy = UserId;
                                                    ph.Comment = "Return Order";
                                                    ph.PaymentStatus = 1;
                                                    context.PayLaterCollectionHistoryDb.Add(ph);
                                                    var kkreturnReq = context.KKReturnReplaceRequests.Where(x => x.NewGeneratedOrderId == _OrderMaster.OrderId && x.Status == "Pending" && x.IsActive == true && x.IsDeleted == false).Include(x => x.Details).ToList();
                                                    if (kkreturnReq.Any())
                                                    {
                                                        foreach (var req in kkreturnReq)
                                                        {
                                                            req.Status = "Completed";
                                                            foreach (var d in req.Details.Where(x => x.Status == 6).ToList())
                                                            {
                                                                d.Status = 7;  // for POC completed
                                                            }
                                                            context.Entry(req).State = EntityState.Modified;
                                                        }
                                                    }
                                                    context.Commit();
                                                }

                                                //PayLaterCollection payLaterCollections = new PayLaterCollection();
                                                //List<PayLaterCollectionHistory> payLaterCollectionHistorys = new List<PayLaterCollectionHistory>();
                                                int parentorderids = Convert.ToInt32(parentorderdata.OrderId);
                                                PayLaterCollection payLaterCollections = context.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == parentorderids);
                                                List<PayLaterCollectionHistory> payLaterCollectionHistorys = context.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollections.Id && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();
                                                if (payLaterCollectionHistorys != null && payLaterCollectionHistorys.Any())
                                                {
                                                    List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                                                    foreach (var item in payLaterCollectionHistorys)
                                                    {
                                                        PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                                        singlepayment.Amount = item.Amount;
                                                        singlepayment.Comment = item.Comment;
                                                        list.Add(singlepayment);
                                                    }
                                                    double getamount = 0;
                                                    CurrencyManagementController currencyManagementController = new CurrencyManagementController();
                                                    getamount = currencyManagementController.ReturnAmount(list);
                                                    if (payLaterCollections.Amount == getamount)
                                                    {
                                                        payLaterCollections.Status = 4;
                                                        payLaterCollections.ModifiedBy = UserId;
                                                        payLaterCollections.ModifiedDate = DateTime.Now;
                                                        context.Entry(payLaterCollections).State = EntityState.Modified;

                                                        #region ordermaster settle 
                                                        CashCollectionNewController ctrl = new CashCollectionNewController();
                                                        bool payres = ctrl.OrderSettle(context, payLaterCollections.OrderId);
                                                        #endregion

                                                        context.Commit();
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                double refundamount = _OrderDispatchedMaster.TotalAmount;
                                                int OrderId = DeliveryCancelledOrderDTO.OrderId;
                                                var paymentresponseapp = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == parentorderdata.OrderId && x.status == "Success" && x.PaymentFrom == "DirectUdhar").FirstOrDefault();
                                                if (paymentresponseapp != null)
                                                {
                                                    _OrderDispatchedMaster.PocCreditNoteNumber = "SR_" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + Guid.NewGuid().ToString("N").ToUpper().Substring(0, 10);
                                                    _OrderDispatchedMaster.PocCreditNoteDate = DateTime.Now;

                                                    PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                                    PaymentResponseRetailerApp paymentres = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == porderid && x.status == "Success").FirstOrDefault();
                                                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                                    {
                                                        amount = (-1) * refundamount,
                                                        CreatedDate = indianTime,
                                                        currencyCode = "INR",
                                                        OrderId = OrderId,
                                                        PaymentFrom = paymentres.PaymentFrom,
                                                        GatewayTransId = paymentres.GatewayTransId,
                                                        GatewayOrderId = paymentres.GatewayOrderId,
                                                        status = "Success",
                                                        UpdatedDate = indianTime,
                                                        IsRefund = false,
                                                        IsOnline = true,
                                                        statusDesc = "Refund Initiated"
                                                    };
                                                    context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                                    context.Commit();
                                                    // addd Refund request

                                                    //var PaymentRefundRequestDc = new AngularJSAuthentication.DataContracts.Masters.PaymentRefund.PaymentRefundRequestDc
                                                    //{
                                                    //    Amount = refundamount,
                                                    //    OrderId = PaymentResponseRetailerAppDb.OrderId,
                                                    //    Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                                    //    Status = (int)PaymentRefundEnum.Initiated,
                                                    //    ReqGatewayTransId = paymentres.GatewayTransId,
                                                    //    CreatedBy = UserId,
                                                    //    CreatedDate = indianTime,
                                                    //    IsActive = true,
                                                    //    IsDeleted = false,
                                                    //    ModifiedBy = UserId,
                                                    //    ModifiedDate = indianTime,
                                                    //    RefundType = (int)RefundTypeEnum.Auto,
                                                    //    PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                                    //};
                                                    //bool IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                                                }
                                                else
                                                {
                                                    if (parentorderdata.paymentThrough.ToLower() == "scaleup")
                                                    {
                                                        _OrderDispatchedMaster.PocCreditNoteNumber = "SR_" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + Guid.NewGuid().ToString("N").ToUpper().Substring(0, 10);
                                                        _OrderDispatchedMaster.PocCreditNoteDate = DateTime.Now;
                                                    }

                                                    PaymentResponseRetailerApp paymentResp = new PaymentResponseRetailerApp();
                                                    paymentResp.amount = (-1) * refundamount;
                                                    paymentResp.currencyCode = "INR";
                                                    paymentResp.status = "Success";
                                                    paymentResp.statusDesc = "Sales Return Order";
                                                    paymentResp.PaymentFrom = "Gullak";
                                                    paymentResp.OrderId = OrderId;
                                                    paymentResp.GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                                                    paymentResp.GatewayOrderId = _OrderDispatchedMaster.CustomerId.ToString();
                                                    paymentResp.CreatedDate = DateTime.Now;
                                                    paymentResp.UpdatedDate = DateTime.Now;
                                                    context.PaymentResponseRetailerAppDb.Add(paymentResp);
                                                    context.Commit();


                                                    var Idparam = new SqlParameter("@PaymentRefundRequestId", paymentResp.id);
                                                    var OrderIdparam = new SqlParameter("@OrderId", OrderId);
                                                    var refundamountparam = new SqlParameter("@refundamount", refundamount);
                                                    int count = context.Database.ExecuteSqlCommand("exec GullakPaymentRefundForSalesReturnOrder @PaymentRefundRequestId,@OrderId,@refundamount ", Idparam, OrderIdparam, refundamountparam);

                                                }



                                                // Return Order Request Completed
                                                var kkreturnReq = context.KKReturnReplaceRequests.Where(x => x.NewGeneratedOrderId == _OrderMaster.OrderId && x.Status == "Pending" && x.IsActive == true && x.IsDeleted == false).Include(x => x.Details).ToList();
                                                if (kkreturnReq.Any())
                                                {
                                                    foreach (var req in kkreturnReq)
                                                    {
                                                        req.Status = "Completed";
                                                        foreach (var d in req.Details.Where(x => x.Status == 6).ToList())
                                                        {
                                                            d.Status = 7;  // for POC completed
                                                        }
                                                        context.Entry(req).State = EntityState.Modified;
                                                    }
                                                }
                                            }
                                            context.Commit();
                                        }
                                        #endregion

                                        #region FY Parchase Calculate
                                        //MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                                        //string fy = (_OrderDispatchedMaster.CreatedDate.Month >= 4 ? _OrderDispatchedMaster.CreatedDate.Year + 1 : _OrderDispatchedMaster.CreatedDate.Year).ToString();
                                        //var tcsCustomer = mHelper.Select(x => x.CustomerId == _OrderDispatchedMaster.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                                        //if (tcsCustomer != null)
                                        //{
                                        //    tcsCustomer.TotalPurchase -= _OrderDispatchedMaster.GrossAmount;
                                        //    tcsCustomer.LastUpdatedDate = indianTime;
                                        //    mHelper.ReplaceWithoutFind(tcsCustomer.Id, tcsCustomer, "TCSCustomer");
                                        //}
                                        #endregion


                                        #region Update IRN Check 
                                        IRNHelper irnHelper = new IRNHelper();

                                        if (irnHelper.IsGenerateIRN(context, _OrderDispatchedMaster.CustomerId))
                                        {
                                            _OrderDispatchedMaster.IsGenerateCN = true;
                                            ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration
                                            {
                                                APIType = "GenerateCN",
                                                CreateDate = indianTime,
                                                OrderId = _OrderMaster.OrderId,
                                                IsActive = true,
                                            };

                                            context.ClearTaxIntegrations.Add(clearTaxIntegration);
                                            context.Entry(_OrderDispatchedMaster).State = EntityState.Modified;
                                            context.Commit();
                                        }
                                        #endregion
                                        #region chqbook Refund
                                        //var PaymentDetail = context.PaymentResponseRetailerAppDb.Where(x=>x.OrderId== DeliveryCancelledOrderDTO.OrderId && x.PaymentFrom == "chqbook" && x.status == "Success").FirstOrDefault();
                                        //if (PaymentDetail != null)
                                        //{
                                        //    if (PaymentDetail.PaymentFrom == "chqbook")
                                        //    {
                                        //        ChqbookBookRefundHelper chqbookBookRefundHelper = new ChqbookBookRefundHelper();
                                        //        var response = await chqbookBookRefundHelper.ChqbookBookRefund(Convert.ToInt32(PaymentDetail.GatewayTransId), PaymentDetail.OrderId);

                                        //        if (response.status)
                                        //        {
                                        //            PaymentDetail.IsRefund = true;
                                        //            PaymentDetail.statusDesc = "Refund Api through";
                                        //            context.Entry(PaymentDetail).State = EntityState.Modified;
                                        //            context.Commit();
                                        //        }
                                        //    }
                                        //}
                                        #endregion

                                        dbContextTransaction.Complete();


                                        #region Sellerstock update
                                        if (_OrderDispatchedMaster != null && _OrderDispatchedMaster.CustomerType == "SellerStore")
                                        {
                                            UpdateSellerStockOfCFRProduct Postobj = new UpdateSellerStockOfCFRProduct();
                                            Postobj.OrderId = _OrderDispatchedMaster.OrderId;
                                            Postobj.Skcode = _OrderDispatchedMaster.Skcode;
                                            Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                            foreach (var item in _OrderDispatchedMaster.orderDetails)
                                            {
                                                SellerItemDetailDc newitem = new SellerItemDetailDc();
                                                newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                                newitem.SellingPrice = item.UnitPrice;
                                                newitem.qty = (-1) * item.qty;
                                                Postobj.ItemDetailDc.Add(newitem);
                                            }
                                            BackgroundTaskManager.Run(() =>
                                            {
                                                try
                                                {
                                                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/sk/RetailerAppApi/UpdateStockOnDeliveryFromSkApp";
                                                    using (GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string> memberClient = new GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string>(tradeUrl, "", null))
                                                    {
                                                        AsyncContext.Run(() => memberClient.PostAsync(Postobj));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    TextFileLogHelper.LogError("Error while Update Seller Stock Of CFR Product: " + ex.ToString());
                                                }
                                            });
                                        }
                                        #endregion

                                        #region Insert in FIFO

                                        if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                        {
                                            List<GrDC> items = _OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0).Select(x => new GrDC
                                            {
                                                ItemMultiMrpId = x.ItemMultiMRPId,
                                                WarehouseId = _OrderDispatchedMaster.WarehouseId,
                                                Source = "Cancel In",
                                                CreatedDate = indianTime,
                                                POId = x.OrderId,
                                                Qty = x.qty,
                                                Price = x.UnitPrice,

                                            }).ToList();

                                            foreach (var item in items)
                                            {
                                                RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                                rabbitMqHelper.Publish("POC", item);
                                            }
                                        }

                                        #endregion

                                        if (_OrderMaster.OrderType != 9 && result.BatchCodeSubjects != null && result.BatchCodeSubjects.Any() && result.IsSuccess)
                                        {
                                            Publisher.PublishOrderIn(result.BatchCodeSubjects);
                                        }

                                        resultMessage = "Approved Succesfully";
                                        status = true;
                                    }
                                }

                            }
                        }
                        else
                        {
                            resultMessage = "Can't Approved Order Due to order status :" + _OrderDispatchedMaster.Status;
                            status = false;
                        }
                        var res = new
                        {
                            status = status,
                            Message = resultMessage
                        };




                        //PocOrderInProcess.RemoveAll(x => x == DeliveryCancelledOrderDTO.OrderId);
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            catch (Exception ex)
            {
                //PocOrderInProcess.RemoveAll(x => x == DeliveryCancelledOrderDTO.OrderId);
                throw ex;
            }
        }
        public async Task<ApprovedAndDOPocDc> ApprovedAndDOPoc(List<DeliveryCancelledOrderItem> _DeliveryCancelledOrderItems, OrderDispatchedMaster _ODM, OrderMaster _OrderMaster, /*List<CurrentStock> _Currentstock, List<FreeStock> _FreeStock,*/ RewardPoint _RewardPoint, Wallet _Wallet, People _people, AuthContext context, TransactionScope dbContextTransaction, Warehouse warehouse)
        {
            var result = new ApprovedAndDOPocDc();
            result.IsSuccess = true;
            result.BatchCodeSubjects = new List<BatchCodeSubjectDc>();
            try
            {
                List<FinalOrderDispatchedDetails> AddFinalobj = new List<FinalOrderDispatchedDetails>();
                List<OrderDispatchedDetails> dispatchedobj = new List<OrderDispatchedDetails>();
                List<DeliveryCancelledOrderItem> UpdateDeliveryCancelledOrderItem = new List<DeliveryCancelledOrderItem>();
                List<ReturnOrderDispatchedDetails> AddReturnOrderDispatchedDetails = new List<ReturnOrderDispatchedDetails>();
                dispatchedobj = _ODM.orderDetails.ToList();
                var orderDetail = _OrderMaster.orderDetails.ToList();
                for (var i = 0; i < dispatchedobj.Count; i++)
                {
                    FinalOrderDispatchedDetails newfinal = new FinalOrderDispatchedDetails();
                    newfinal.OrderDispatchedDetailsId = dispatchedobj[i].OrderDispatchedDetailsId;
                    newfinal.OrderDetailsId = dispatchedobj[i].OrderDetailsId;
                    newfinal.OrderId = dispatchedobj[i].OrderId;
                    newfinal.OrderDispatchedMasterId = dispatchedobj[i].OrderDispatchedMasterId;
                    newfinal.CustomerId = dispatchedobj[i].CustomerId;
                    newfinal.CustomerName = dispatchedobj[i].CustomerName;
                    newfinal.City = dispatchedobj[i].City;
                    newfinal.Mobile = dispatchedobj[i].Mobile;
                    newfinal.OrderDate = dispatchedobj[i].OrderDate;
                    newfinal.CompanyId = dispatchedobj[i].CompanyId;
                    newfinal.CityId = dispatchedobj[i].CityId;
                    newfinal.WarehouseId = dispatchedobj[i].WarehouseId;
                    newfinal.WarehouseName = dispatchedobj[i].WarehouseName;
                    newfinal.CategoryName = dispatchedobj[i].CategoryName;
                    newfinal.ItemId = dispatchedobj[i].ItemId;
                    newfinal.Itempic = dispatchedobj[i].Itempic;
                    newfinal.itemname = dispatchedobj[i].itemname;
                    newfinal.SellingUnitName = dispatchedobj[i].SellingUnitName;
                    newfinal.itemcode = dispatchedobj[i].itemcode;
                    newfinal.Barcode = dispatchedobj[i].Barcode;
                    newfinal.UnitPrice = dispatchedobj[i].UnitPrice;
                    newfinal.Purchaseprice = dispatchedobj[i].Purchaseprice;
                    newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
                    newfinal.MinOrderQtyPrice = dispatchedobj[i].MinOrderQtyPrice;
                    newfinal.qty = dispatchedobj[i].qty; //TempQty;// dispatchedobj[i].qty;
                    newfinal.price = dispatchedobj[i].price;
                    newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
                    int MOQ = dispatchedobj[i].MinOrderQty;
                    newfinal.MinOrderQtyPrice = MOQ * dispatchedobj[i].UnitPrice;
                    newfinal.qty = Convert.ToInt32(dispatchedobj[i].qty);
                    int qty = 0;
                    qty = Convert.ToInt32(newfinal.qty);
                    newfinal.TaxPercentage = dispatchedobj[i].TaxPercentage;
                    //........CALCULATION FOR NEW SHOPKIRANA.............................
                    newfinal.Noqty = qty; // for total qty (no of items)

                    // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
                    newfinal.TotalAmt = System.Math.Round(newfinal.UnitPrice * qty, 2);

                    // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
                    newfinal.AmtWithoutTaxDisc = ((100 * newfinal.UnitPrice * qty) / (1 + newfinal.TaxPercentage / 100)) / 100;

                    // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
                    newfinal.AmtWithoutAfterTaxDisc = dispatchedobj[i].AmtWithoutAfterTaxDisc;

                    //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
                    newfinal.TaxAmmount = (newfinal.AmtWithoutAfterTaxDisc * newfinal.TaxPercentage) / 100;

                    //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
                    newfinal.TotalAmountAfterTaxDisc = newfinal.AmtWithoutAfterTaxDisc + newfinal.TaxAmmount;

                    //...............Calculate Discount.............................
                    newfinal.DiscountPercentage = 0;
                    newfinal.DiscountAmmount = 0;
                    newfinal.NetAmtAfterDis = 0;
                    //...................................................................
                    newfinal.Purchaseprice = dispatchedobj[i].price;
                    //newfinal.VATTax = items.VATTax;
                    newfinal.CreatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
                    newfinal.UpdatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
                    newfinal.Deleted = false;
                    AddFinalobj.Add(newfinal);
                }
                #region //Update OrderMaster


                #endregion
                #region //DispatchedMaster

                _ODM.Status = "Post Order Canceled";

                #endregion
                #region Order Master History for Status Post Order Canceled  
                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                OrderMasterHistories.orderid = _ODM.OrderId;
                OrderMasterHistories.Status = _ODM.Status;
                OrderMasterHistories.Reasoncancel = null;
                OrderMasterHistories.DeliveryIssuanceId = _ODM.DeliveryIssuanceIdOrderDeliveryMaster;
                OrderMasterHistories.Warehousename = _ODM.WarehouseName;
                OrderMasterHistories.username = _people.DisplayName;
                OrderMasterHistories.userid = _people.PeopleID;
                OrderMasterHistories.CreatedDate = indianTime;
                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                #endregion
                foreach (var pc in dispatchedobj)
                {
                    //FreeStock; if both true mean freestock return
                    var isFreeItem = orderDetail.Where(x => x.ItemId == pc.ItemId && x.OrderDetailsId == pc.OrderDetailsId).FirstOrDefault();
                    if (isFreeItem != null && isFreeItem.IsFreeItem && isFreeItem.IsDispatchedFreeStock && pc.qty > 0)
                    {

                        pc.IsDispatchedFreeStock = true;
                        pc.IsFreeItem = true;

                        ReturnOrderDispatchedDetails RDD = new ReturnOrderDispatchedDetails();
                        RDD.OrderDispatchedDetailsId = pc.OrderDispatchedDetailsId;
                        RDD.OrderDetailsId = pc.OrderDetailsId;
                        RDD.OrderId = pc.OrderId;
                        RDD.OrderDispatchedMasterId = pc.OrderDispatchedMasterId;
                        RDD.CustomerId = pc.CustomerId;
                        RDD.CustomerName = pc.CustomerName;
                        RDD.City = pc.City;
                        RDD.Mobile = pc.Mobile;
                        RDD.OrderDate = pc.OrderDate;
                        RDD.CompanyId = pc.CompanyId;
                        RDD.CityId = pc.CityId;
                        RDD.WarehouseId = pc.WarehouseId;
                        RDD.WarehouseName = pc.WarehouseName;
                        RDD.CategoryName = pc.CustomerName;
                        RDD.ItemId = pc.ItemId;
                        RDD.Itempic = pc.Itempic;
                        RDD.itemname = pc.itemname;
                        RDD.itemcode = pc.itemcode;
                        RDD.Barcode = pc.Barcode;
                        RDD.price = pc.price;
                        RDD.UnitPrice = pc.UnitPrice;
                        RDD.Purchaseprice = pc.Purchaseprice;
                        RDD.MinOrderQty = pc.MinOrderQty;
                        RDD.MinOrderQtyPrice = pc.MinOrderQtyPrice;
                        RDD.qty = pc.qty;
                        RDD.NetAmmount = pc.NetAmmount;
                        RDD.DiscountPercentage = pc.DiscountPercentage;
                        RDD.DiscountAmmount = pc.DiscountAmmount;
                        RDD.NetAmtAfterDis = pc.NetAmtAfterDis;
                        RDD.TaxPercentage = pc.TaxPercentage;
                        RDD.TaxAmmount = pc.TaxAmmount;
                        RDD.TotalAmt = pc.TotalAmt;
                        RDD.TotalCessPercentage = pc.TotalCessPercentage;
                        RDD.CessTaxAmount = pc.CessTaxAmount;
                        RDD.UnitId = pc.UnitId;
                        RDD.Unitname = pc.Unitname;
                        RDD.CreatedDate = indianTime;
                        RDD.UpdatedDate = indianTime;
                        RDD.Deleted = false;
                        RDD.Status = _ODM.Status;
                        RDD.ItemMultiMRPId = pc.ItemMultiMRPId;
                        AddReturnOrderDispatchedDetails.Add(RDD);

                    }
                    else
                    {
                        if (pc.qty > 0)
                        {
                            pc.IsDispatchedFreeStock = false;
                            pc.IsFreeItem = false;
                        }
                        ReturnOrderDispatchedDetails RDD = new ReturnOrderDispatchedDetails();
                        RDD.OrderDispatchedDetailsId = pc.OrderDispatchedDetailsId;
                        RDD.OrderDetailsId = pc.OrderDetailsId;
                        RDD.OrderId = pc.OrderId;
                        RDD.OrderDispatchedMasterId = pc.OrderDispatchedMasterId;
                        RDD.CustomerId = pc.CustomerId;
                        RDD.CustomerName = pc.CustomerName;
                        RDD.City = pc.City;
                        RDD.Mobile = pc.Mobile;
                        RDD.OrderDate = pc.OrderDate;
                        RDD.CompanyId = pc.CompanyId;
                        RDD.CityId = pc.CityId;
                        RDD.WarehouseId = pc.WarehouseId;
                        RDD.WarehouseName = pc.WarehouseName;
                        RDD.CategoryName = pc.CustomerName;
                        RDD.ItemId = pc.ItemId;
                        RDD.Itempic = pc.Itempic;
                        RDD.itemname = pc.itemname;
                        RDD.itemcode = pc.itemcode;
                        RDD.Barcode = pc.Barcode;
                        RDD.price = pc.price;
                        RDD.UnitPrice = pc.UnitPrice;
                        RDD.Purchaseprice = pc.Purchaseprice;
                        RDD.MinOrderQty = pc.MinOrderQty;
                        RDD.MinOrderQtyPrice = pc.MinOrderQtyPrice;
                        RDD.qty = pc.qty;
                        RDD.NetAmmount = pc.NetAmmount;
                        RDD.DiscountPercentage = pc.DiscountPercentage;
                        RDD.DiscountAmmount = pc.DiscountAmmount;
                        RDD.NetAmtAfterDis = pc.NetAmtAfterDis;
                        RDD.TaxPercentage = pc.TaxPercentage;
                        RDD.TaxAmmount = pc.TaxAmmount;
                        RDD.TotalAmt = pc.TotalAmt;
                        RDD.TotalCessPercentage = pc.TotalCessPercentage;
                        RDD.CessTaxAmount = pc.CessTaxAmount;
                        RDD.UnitId = pc.UnitId;
                        RDD.Unitname = pc.Unitname;
                        RDD.CreatedDate = indianTime;
                        RDD.UpdatedDate = indianTime;
                        RDD.Deleted = false;
                        RDD.Status = _ODM.Status;
                        RDD.ItemMultiMRPId = pc.ItemMultiMRPId;
                        AddReturnOrderDispatchedDetails.Add(RDD);
                    }

                    var ord = _OrderMaster.orderDetails.Where(r => r.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
                    ord.Status = "Post Order Canceled";
                    ord.UpdatedDate = indianTime;
                    context.Entry(ord).State = EntityState.Modified;
                }
                #region _RewardPoint update
                if (_RewardPoint != null)
                {
                    if (_OrderMaster.RewardPoint > 0)
                    {
                        _RewardPoint.EarningPoint -= _OrderMaster.RewardPoint;
                        if (_RewardPoint.EarningPoint < 0)
                            _RewardPoint.EarningPoint = 0;
                        _RewardPoint.UpdatedDate = indianTime;
                        _RewardPoint.TransactionDate = indianTime;
                        context.Entry(_RewardPoint).State = EntityState.Modified;

                    }
                }
                #endregion
                #region Wallet Update
                if (_OrderMaster.walletPointUsed > 0 && _OrderMaster.walletPointUsed != null)
                {
                    _Wallet.TotalAmount = _Wallet.TotalAmount + _OrderMaster.walletPointUsed;
                    context.Entry(_Wallet).State = EntityState.Modified;
                    CustomerWalletHistory CWH = new CustomerWalletHistory();
                    CWH.PeopleId = _people.PeopleID;
                    CWH.PeopleName = _people.DisplayName;
                    CWH.OrderId = _OrderMaster.OrderId;
                    //op by Cust
                    CWH.WarehouseId = _OrderMaster.WarehouseId;
                    CWH.CompanyId = 1;
                    CWH.CustomerId = _Wallet.CustomerId;
                    CWH.NewAddedWAmount = _OrderMaster.walletPointUsed;
                    CWH.TotalWalletAmount = _Wallet.TotalAmount;
                    CWH.Through = "From post order cancellation";
                    CWH.CreatedDate = indianTime;
                    CWH.UpdatedDate = indianTime;
                    context.CustomerWalletHistoryDb.Add(CWH);
                }
                #endregion

                foreach (var OrderItem in _DeliveryCancelledOrderItems)
                {
                    OrderItem.IsPoc = true;
                    OrderItem.ModifiedDate = indianTime;
                    OrderItem.ModifiedBy = _people.PeopleID;
                    UpdateDeliveryCancelledOrderItem.Add(OrderItem);
                }
                if (UpdateDeliveryCancelledOrderItem != null && UpdateDeliveryCancelledOrderItem.Count > 0)
                {
                    foreach (var CancelledOrderItem in UpdateDeliveryCancelledOrderItem)
                    {
                        context.Entry(CancelledOrderItem).State = EntityState.Modified;
                    }
                }

                if (AddFinalobj != null && AddFinalobj.Count > 0)
                {
                    context.FinalOrderDispatchedDetailsDb.AddRange(AddFinalobj);
                }
                if (AddReturnOrderDispatchedDetails != null && AddReturnOrderDispatchedDetails.Count > 0)
                {
                    context.ReturnOrderDispatchedDetailsDb.AddRange(AddReturnOrderDispatchedDetails);
                }

                #region bat
                var OdddetailsId = dispatchedobj.Where(x => x.qty > 0).Select(x => x.OrderDispatchedDetailsId).ToList();
                var poststocklist = context.DeliveryCancelledItemBatchs.Where(x => OdddetailsId.Contains(x.OrderDispatchedDetailsId) && x.Qty > 0).ToList();

                #endregion


                if (context.Commit() > 0)
                {
                    #region stock Hit on poc
                    //for currentstock
                    MultiStockHelper<POCStockEntryDc> MultiStockHelpers = new MultiStockHelper<POCStockEntryDc>();
                    List<POCStockEntryDc> POCStockList = new List<POCStockEntryDc>();
                    List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                    foreach (var StockHit in dispatchedobj.Where(x => x.qty > 0))
                    {
                        var RefStockCode = _OrderMaster.OrderType == 8 ? "CL" : "C";
                        bool isFree = _OrderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                        if (isFree) { RefStockCode = "F"; }
                        else if (_OrderMaster.OrderType == 6) //6 Damage stock
                        {
                            RefStockCode = "D";
                        }
                        else if (_OrderMaster.OrderType == 9) //9 Non Sellable Stock
                        {
                            RefStockCode = "N";
                        }
                        POCStockList.Add(new POCStockEntryDc
                        {
                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                            OrderId = StockHit.OrderId,
                            Qty = StockHit.qty,
                            UserId = _people.PeopleID,
                            WarehouseId = StockHit.WarehouseId,
                            RefStockCode = RefStockCode
                        });


                        #region BatchCode
                        if (_OrderMaster.OrderType == 9) //9 Non Sellable Stock
                        {
                            var NonSellableStockId = context.NonSellableStockDB.FirstOrDefault(x => x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.WarehouseId == StockHit.WarehouseId).NonSellableStockId;
                            foreach (var item in poststocklist.Where(d => d.OrderDispatchedDetailsId == StockHit.OrderDispatchedDetailsId))
                            {
                                var StockBatchMasterId = context.StockBatchMasters.FirstOrDefault(x => x.StockId == NonSellableStockId && x.StockType == "N" && x.BatchMasterId == item.BatchMasterId).Id;
                                TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                {

                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                    Qty = item.Qty,
                                    StockBatchMasterId = StockBatchMasterId,
                                    WarehouseId = StockHit.WarehouseId,
                                    ObjectId = item.Id,
                                    ObjectIdDetailId = item.OrderDispatchedDetailsId
                                });
                            }
                        }
                        else
                        {
                            foreach (var item in poststocklist.Where(d => d.OrderDispatchedDetailsId == StockHit.OrderDispatchedDetailsId))
                            {
                                result.BatchCodeSubjects.Add(new BatchCodeSubjectDc
                                {
                                    ObjectDetailId = item.Id,
                                    ObjectId = StockHit.OrderDispatchedDetailsId,
                                    StockType = RefStockCode,
                                    Quantity = item.Qty,
                                    WarehouseId = StockHit.WarehouseId,
                                    ItemMultiMrpId = StockHit.ItemMultiMRPId
                                });
                            }
                        }
                        #endregion
                    }

                    if (POCStockList.Any())
                    {

                        BatchMasterManager batchMasterManager = new BatchMasterManager();
                        bool res = MultiStockHelpers.MakeEntry(POCStockList, "Stock_OnPOC_New", context, dbContextTransaction);
                        if (!res)
                        {
                            result.IsSuccess = false;
                            result.BatchCodeSubjects = null;
                            return result;
                        }
                        if ((_OrderMaster.OrderType == 9) && TransferOrderItemBatchMasterList != null && TransferOrderItemBatchMasterList.Any()) //9 Non Sellable Stock
                        {
                            var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "NonSellableIn" && x.IsDeleted == false);
                            bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, context, _people.PeopleID, StockTxnType.Id);
                            if (!batchRes)
                            {
                                result.IsSuccess = false;
                                result.BatchCodeSubjects = null;
                                return result;
                            }
                        }
                    }

                    #endregion

                    //Not Create Poc Note for Test Warehouse
                    string PocinvoiceNumber = " ";
                    if (_ODM.WarehouseId != 67 && _ODM.WarehouseId != 80 && _OrderMaster.OrderType != 3)
                    {
                        PocinvoiceNumber = context.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + warehouse.Stateid).FirstOrDefault();

                    }
                    // _ODM.PocCreditNoteNumber = _ODM.invoice_no + '/' + invoiceNumber;
                    _ODM.PocCreditNoteDate = _ODM.PocCreditNoteNumber != null ? _ODM.PocCreditNoteDate : indianTime;
                    _ODM.PocCreditNoteNumber = _ODM.PocCreditNoteNumber != null ? _ODM.PocCreditNoteNumber : PocinvoiceNumber;

                    var OnlineEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == _ODM.OrderId && z.IsOnline && z.status == "Success").ToList();
                    int POCCancelationCharges = 0;
                    if (_OrderMaster.OrderType == 4)
                    {
                        MongoDbHelper<DataContracts.Mongo.ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.ExtandedCompanyDetail>();
                        var extandedCompanyDetail = mongoDbHelper.Select(x => x.WarehouseId == _OrderMaster.WarehouseId && x.AppType == "DON").FirstOrDefault();

                        if (extandedCompanyDetail != null)
                            POCCancelationCharges = extandedCompanyDetail.DeliveryCancelationPer ?? 0;
                    }

                    if (!string.IsNullOrEmpty(_ODM.paymentThrough) && _ODM.paymentThrough.Trim().ToLower() == "paylater")
                    {
                        var paylaterdata = context.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == _OrderMaster.OrderId);
                        if (paylaterdata != null)
                        {
                            Model.CashManagement.PayLaterCollectionHistory history = new Model.CashManagement.PayLaterCollectionHistory();
                            history.Amount = paylaterdata.Amount;
                            history.PayLaterCollectionId = paylaterdata.Id;
                            history.CreatedDate = DateTime.Now;
                            history.RefNo = "";
                            history.IsActive = true;
                            history.IsDeleted = false;
                            history.CurrencyHubStockId = 0;
                            history.PaymentStatus = 1;
                            history.PaymentMode = "";
                            history.Comment = "Post Order Cancelled";
                            history.CreatedBy = _people.PeopleID;
                            context.PayLaterCollectionHistoryDb.Add(history);


                            var param = new SqlParameter("@OrderId", paylaterdata.OrderId);
                            AngularJSAuthentication.API.Controllers.OrderMasterrController.RefundPaylaterDc refund = context.Database.SqlQuery<AngularJSAuthentication.API.Controllers.OrderMasterrController.RefundPaylaterDc>("exec Sp_GetRefundDetail @OrderId", param).FirstOrDefault();
                            if (refund != null && refund.RefundAmount > 0)
                            {
                                PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                {
                                    amount = (-1) * (refund.RefundAmount),
                                    CreatedDate = DateTime.Now,
                                    currencyCode = "INR",
                                    OrderId = paylaterdata.OrderId,
                                    PaymentFrom = refund.PaymentMode,
                                    GatewayTransId = refund.RefNo,
                                    GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                    status = "Success",
                                    UpdatedDate = DateTime.Now,
                                    IsRefund = false,
                                    IsOnline = true,
                                    statusDesc = "Refund Initiated"
                                };
                                context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                {
                                    Amount = (refund.RefundAmount),
                                    OrderId = PaymentResponseRetailerAppDb.OrderId,
                                    Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                    ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                    Status = (int)PaymentRefundEnum.Initiated,
                                    CreatedBy = _people.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ModifiedBy = _people.PeopleID,
                                    ModifiedDate = DateTime.Now,
                                    PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                };
                                bool IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                            }
                            context.Commit();
                        }
                    }
                    else
                    {
                        if (warehouse.IsOnlineRefundEnabled)
                        {
                            if (context.PaymentRefundApis.Any(x => x.IsActive == true) && OnlineEntries != null && OnlineEntries.Any(x => x.IsOnline == true))
                            {
                                // Post order cancel payment refund  -- April2022
                                #region Post order cancel payment refund  -- April2022
                                //case 1 : online payment list                      
                                if (OnlineEntries != null && OnlineEntries.Any() && OnlineEntries.Sum(x => x.amount) > 0)
                                {
                                    var RefundDays = context.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                                    PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                    foreach (var item in OnlineEntries.GroupBy(c => c.GatewayTransId))
                                    {

                                        int RefundType = (int)RefundTypeEnum.Auto;
                                        var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.FirstOrDefault().PaymentFrom.Trim().ToLower());
                                        if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.FirstOrDefault().CreatedDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                        {
                                            RefundType = (int)RefundTypeEnum.Manual;
                                        }
                                        else if (PaymentRefundDays == null && item.FirstOrDefault().PaymentFrom.Trim().ToLower() != "gullak")
                                        {
                                            RefundType = (int)RefundTypeEnum.Manual;
                                        }
                                        double RefundAmount = item.Sum(e => e.amount);
                                        if (RefundAmount > 0)
                                        {
                                            var PaymentResponseRetailerApp = new PaymentResponseRetailerApp
                                            {
                                                amount = (-1) * RefundAmount,
                                                CreatedDate = DateTime.Now,
                                                currencyCode = "INR",
                                                OrderId = _ODM.OrderId,
                                                PaymentFrom = item.FirstOrDefault().PaymentFrom,
                                                GatewayTransId = item.FirstOrDefault().GatewayTransId,
                                                GatewayOrderId = item.FirstOrDefault().GatewayOrderId,
                                                status = "Success",
                                                UpdatedDate = DateTime.Now,
                                                IsRefund = false,
                                                IsOnline = true,
                                                statusDesc = "Refund Initiated"
                                            };
                                            context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerApp);
                                            context.Commit();
                                            // addd Refund request
                                            var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                            {
                                                Amount = RefundAmount,
                                                OrderId = PaymentResponseRetailerApp.OrderId,
                                                Source = PaymentResponseRetailerApp.PaymentFrom,
                                                Status = (int)PaymentRefundEnum.Initiated,
                                                ReqGatewayTransId = item.FirstOrDefault().GatewayTransId,
                                                CreatedBy = _people.PeopleID,
                                                CreatedDate = DateTime.Now,
                                                IsActive = true,
                                                IsDeleted = false,
                                                ModifiedBy = _people.PeopleID,
                                                ModifiedDate = DateTime.Now,
                                                RefundType = RefundType,
                                                PaymentResponseRetailerAppId = PaymentResponseRetailerApp.id
                                            };
                                            bool IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region Gullak Return

                            var cashOldEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == _OrderMaster.OrderId && z.PaymentFrom == "Gullak"
                                                          && z.status == "Success").ToList();
                            double totalAmount = 0;
                            if (cashOldEntries != null && cashOldEntries.Any())
                            {
                                totalAmount = cashOldEntries.Sum(x => x.amount);
                                foreach (var cash in cashOldEntries)
                                {
                                    cash.statusDesc = "Due to Post order cancel Gullak amount refunded.";
                                    context.Entry(cash).State = EntityState.Modified;
                                }
                            }

                            if (totalAmount > 0)
                            {
                                double cancelationAmt = 0;
                                if (POCCancelationCharges > 0)
                                {
                                    cancelationAmt = (totalAmount * POCCancelationCharges / 100);
                                    totalAmount = totalAmount - cancelationAmt;
                                }

                                var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == _OrderMaster.CustomerId);
                                if (customerGullak != null)
                                {
                                    context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                                    {
                                        CreatedDate = indianTime,
                                        CreatedBy = _OrderMaster.CustomerId,
                                        Comment = "Order cancel : " + _OrderMaster.OrderId.ToString() + (cancelationAmt > 0 ? (" with deducted cancelation charges:" + cancelationAmt) : ""),
                                        Amount = totalAmount,
                                        GullakId = customerGullak.Id,
                                        CustomerId = _OrderMaster.CustomerId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ObjectId = _OrderMaster.OrderId.ToString(),
                                        ObjectType = "Order"
                                    });

                                    customerGullak.TotalAmount += totalAmount;
                                    customerGullak.ModifiedBy = customerGullak.CustomerId;
                                    customerGullak.ModifiedDate = indianTime;
                                    context.Entry(customerGullak).State = EntityState.Modified;
                                }
                            }
                            #endregion}
                        }
                    }


                    context.Entry(_ODM).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        #region Ladger update
                        CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                        helper.OnCancel(_ODM.OrderId, _people.PeopleID, context, _OrderMaster.CustomerId);
                        if (_ODM.TotalAmount >= 50000 && _ODM.EwayBillNumber != null)
                        {
                            SendMailForEWAYBillDeactivateSarthi(_ODM.OrderId, _ODM.TotalAmount);
                        }
                        #endregion
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.BatchCodeSubjects = null;
                        return result;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.BatchCodeSubjects = null;
                return result;
            }
        }
        #endregion
        public bool SendMailForEWAYBillDeactivateSarthi(int orderId, double TotalAmount)
        {
            try
            {
                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];

                if (masteremail != null && masterpassword != null)
                {
                    string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                    body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                    body += "<h3 style='background-color: rgb(241, 89, 34);'>Deactivate EWAY Bill</h3>";
                    body += "Hello,";
                    body += "<p><strong>";
                    body += "</strong>" + " With Reference To" + orderId + "</p>";
                    body += "<p>Of Amount : <strong>" + TotalAmount + " </strong> <br/>Please Deactivate EWAYBill For The Same. </p>";
                    body += "Thanks,";
                    body += "<br />";
                    body += "<b></b>";
                    body += "</div>";

                    var Subj = "Alert! Deactivate EWAY Bill For " + orderId;
                    var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                    msg.To.Add("vijaykabra.accounts@shopkirana.com");
                    msg.To.Add("shweta.soni@shopkirana.com");
                    // msg.To.Add("ravikant.dhamne@shopkirana.com");
                    msg.IsBodyHtml = true;
                    var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msg);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ss) { return false; }
        }

        public bool RemovedFromPoc(int Orderid)
        {

            if (Orderid > 0 && PocOrderInProcess.Any(x => x == Orderid))
            {
                PocOrderInProcess.RemoveAll(x => x == Orderid);
                return true;
            }
            return false;
        }

        [Route("GetBatchListForPoc/{OrderDispatchedDetailsId}")]
        [HttpGet]
        public async Task<List<BatchDc>> GetBatchListForPoc(int OrderDispatchedDetailsId)
        {
            List<BatchDc> result = new List<BatchDc>();
            if (OrderDispatchedDetailsId > 0)
            {
                using (AuthContext db = new AuthContext())
                {
                    using (var context = new AuthContext())
                    {
                        var param = new SqlParameter("@OrderDispatchedDetailsId", OrderDispatchedDetailsId);
                        result = context.Database.SqlQuery<BatchDc>("exec BatchCode.GetBatchListForPoc @OrderDispatchedDetailsId ", param).ToList();
                    }
                }
            }
            return result;
        }

    }



    public class ApprovedAndDOPocDc
    {
        public bool IsSuccess { get; set; }
        public List<BatchCodeSubjectDc> BatchCodeSubjects { get; set; }
    }



    public class DeliveryCancelledDraftImageupload
    {
        public int DeliveryIssuanceId { get; set; }
        public int UserId { get; set; }
        public string Imageupload { get; set; }
        public string InvoiceImage { get; set; }
        public int OrderId { get; set; }
        public bool IsPosted { get; set; }//If Delivery Order posted then set true
        public DateTime? OrderedDate { get; set; }
        public List<DeliveryCancelledOrderItemDTO> DeliveryCancelledOrderItem { get; set; }
        public List<ItemBarcodeDc> ItemBarcodes { get; set; }

    }


    public class DeliveryCancelledOrderId
    {
        public int OrderId { get; set; }

    }

    public class DeliveryCancelledDraftobj
    {

        public List<DeliveryCancelledDraftImageupload> DeliveryCancelledDraftImageupload { get; set; }

    }

    public class DeliveryCancelledOrderItemDTO
    {
        public int OrderDispatchedDetailsId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public int DispatchedQty { get; set; }
        public int? MatchedQty { get; set; }
        public int OrderId { get; set; }
        public string ItemNumber { get; set; }
        public int? ValidateQty { get; set; }//add Checker side
        public bool IsClearance { get; set; }
        public List<DeliveryCancelledItemBatchDc> DeliveryCancelledOrderItemBatches { get; set; }
    }

    public class DeliveryCancelledItemBatchDc
    {
        public int Qty { get; set; }
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public long OldBatchMasterId { get; set; }
        public string OldBatchCode { get; set; }

    }

    public class DeliveryCancelledOrderDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }//add Checker side
        public List<DeliveryCancelledOrderItemDTO> DeliveryCancelledOrderItem { get; set; }

    }


}
