using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Common.Helpers.ReportMaker;
using AngularJSAuthentication.DataContracts.Masters.Seller;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Store;
using GenricEcommers.Models;
using LinqKit;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.InActiveCustOrderMasterController;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;
using static AngularJSAuthentication.API.Controllers.OrderMastersAPIController;

namespace AngularJSAuthentication.API.Controllers.External.Other
{
    [RoutePrefix("api/SellerStore")]
    public class SellerStoreController : ApiController

    {
        [Route("MarkClusterAsSeller")]
        [HttpPost]
        public async Task<bool> MarkClusterAsSeller(List<IsClusterWithSeller> sellerClusters)
        {
            bool result = false;
            using (var authContext = new AuthContext())
            {
                var clusterIds = sellerClusters.Select(x => x.ClusterId);
                var clusters = authContext.Clusters.Where(x => clusterIds.Contains(x.ClusterId)).ToList();
                if (clusters != null && clusters.Any())
                {
                    foreach (var cluster in clusters)
                    {
                        var sellercluster = sellerClusters.FirstOrDefault(x => x.ClusterId == cluster.ClusterId);
                        cluster.IsSelleravailable = sellercluster.IsSellerAvailable;
                        cluster.UpdatedDate = DateTime.Now;
                        authContext.Entry(cluster).State = EntityState.Modified;
                    }
                    result = await authContext.CommitAsync() > 0;
                }
            }
            return result;
        }

        [Route("GetAllCluster")]
        [HttpGet]
        public async Task<List<SellerCluster>> GetAllCluster(int? cityId = null)
        {
            List<SellerCluster> clustDTOs = new List<SellerCluster>();
            using (var authContext = new AuthContext())
            {
                var predicate = PredicateBuilder.True<Cluster>();
                predicate = predicate.And(x => x.Active && x.Deleted == false);
                if (cityId.HasValue)
                {
                    predicate = predicate.And(x => x.CityId == cityId.Value);
                }
                clustDTOs = await authContext.Clusters.Where(predicate).Select(x =>
                    new SellerCluster
                    {
                        CityId = x.CityId,
                        CityName = x.CityName,
                        ClusterId = x.ClusterId,
                        ClusterName = x.ClusterName,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName
                        //SellerCluserLtLng= x.LtLng.Any()?x.LtLng.Select(y=> )
                    }
                ).ToListAsync();
            };
            return clustDTOs;
        }


        [Route("GetChangeCustomers")]
        [HttpGet]
        public async Task<List<SellerCustomerDc>> GetChangeCustomers()
        {
            List<SellerCustomerDc> customerDTOs = new List<SellerCustomerDc>();
            using (var authContext = new AuthContext())
            {
                authContext.Database.CommandTimeout = 600;
                customerDTOs = await authContext.Database.SqlQuery<SellerCustomerDc>("Exec getUpdateCustomerForSeller").ToListAsync();
            };
            return customerDTOs;
        }


        [Route("GetSellerMrpItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SellerStoreMrpItemDc>> GetSellerMrpItem(string keyword)
        {
            List<SellerStoreMrpItemDc> result = new List<SellerStoreMrpItemDc>();
            if (keyword != null)
            {
                using (var context = new AuthContext())
                {
                   // var key = new SqlParameter("@keyword", keyword);

                    var keywordParam = new SqlParameter
                    {
                        ParameterName = "keyword",
                        Value = keyword
                    };

                    //var CityIdParam = new SqlParameter
                    //{
                    //    ParameterName = "CityId",
                    //    Value = CityId

                    //};
                    result = context.Database.SqlQuery<SellerStoreMrpItemDc>("EXEC GetSellerStoreMrpItem @keyword", keywordParam).ToList();

                }
            }
            return result;

        }



        public void AddClusterToSeller(List<Cluster> Cluster)
        {
            List<SellerCluster> sellerCluster = Cluster.Select(x => new SellerCluster
            {
                CityId = x.CityId,
                CityName = x.CityName,
                ClusterId = x.ClusterId,
                ClusterName = x.ClusterName,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.WarehouseName
            }).ToList();
            BackgroundTaskManager.Run(() =>
            {
                try
                {
                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/Clusters/AddSkCluster";
                    using (GenericRestHttpClient<List<SellerCluster>, string> memberClient = new GenericRestHttpClient<List<SellerCluster>, string>(tradeUrl, "", null))
                    {
                        AsyncContext.Run(() => memberClient.PostAsync(sellerCluster));
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("Error while saving cluster in Seller: " + ex.ToString());
                }
            });
        }

        public void UpdateClusterToSeller(Cluster Cluster)
        {
            SellerCluster sellerCluster = new SellerCluster
            {
                CityId = Cluster.CityId,
                CityName = Cluster.CityName,
                ClusterId = Cluster.ClusterId,
                ClusterName = Cluster.ClusterName,
                WarehouseId = Cluster.WarehouseId,
                WarehouseName = Cluster.WarehouseName
            };
            BackgroundTaskManager.Run(() =>
            {
                try
                {
                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/Clusters/UpdateSkCluster";
                    using (GenericRestHttpClient<SellerCluster, string> memberClient = new GenericRestHttpClient<SellerCluster, string>(tradeUrl, "", null))
                    {
                        AsyncContext.Run(() => memberClient.PostAsync(sellerCluster));
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("Error while updating cluster in Seller: " + ex.ToString());
                }
            });
        }

        public void InActiveClusterToSeller(int clusterId)
        {
            BackgroundTaskManager.Run(() =>
            {
                try
                {
                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/Clusters/DeleteSkCluster";
                    using (GenericRestHttpClient<string, string> memberClient = new GenericRestHttpClient<string, string>(tradeUrl, "", null))
                    {
                        AsyncContext.Run(() => memberClient.PostAsync(clusterId.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("Error while deleting cluster in Seller: " + ex.ToString());
                }
            });
        }

        public void RefereshClusterToSeller(List<SkCustomer> skCustomers)
        {
            BackgroundTaskManager.Run(() =>
            {
                try
                {
                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/Clusters/RefereshSkCluster";
                    using (GenericRestHttpClient<List<SkCustomer>, string> memberClient = new GenericRestHttpClient<List<SkCustomer>, string>(tradeUrl, "", null))
                    {
                        AsyncContext.Run(() => memberClient.PostAsync(skCustomers));
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("Error while Update Customer cluster in Seller: " + ex.ToString());
                }
            });
        }

        [Route("GeneratedOrderDetail")]
        [HttpGet]
        public async Task<SellerOrderMasteDC> GeneratedOrderDetail(int OrderId)
        {
            using (var context = new AuthContext())
            {
                SellerOrderMasteDC result = new SellerOrderMasteDC();
                var orderId = new SqlParameter("@OrderId", OrderId);
                var orderData = await context.Database.SqlQuery<SellerOrderMasteSPDC>("GeneratedOrderDetailsData @OrderId", orderId).ToListAsync();
                if (orderData != null)
                {
                    result.OrderId = orderData.FirstOrDefault().OrderId;
                    result.TCSAmount = orderData.FirstOrDefault().TCSAmount;
                    result.GrossAmount = orderData.FirstOrDefault().GrossAmount;
                    result.TaxAmount = orderData.FirstOrDefault().TaxAmount;
                    result.CreatedDate = orderData.FirstOrDefault().CreatedDate;
                    result.Status = orderData.FirstOrDefault().Status;
                    result.DeliveredDate = orderData.FirstOrDefault().DeliveredDate != null ? orderData.FirstOrDefault().DeliveredDate : null;
                    result.UpdatedDate = orderData.FirstOrDefault().UpdatedDate;
                    result.deliveryCharge = orderData.FirstOrDefault().deliveryCharge;
                    result.sellerOrderDetailsDCs = Mapper.Map(orderData).ToANew<List<SellerOrderDetailsDC>>();
                }
                return result;
            }
        }


        [Route("SellerStoreOrderAPI")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage SellerStoreOrderAPI(SellerShoppingCart sellerShoppingCart)
        {
            using (var context = new AuthContext())
            {
                OrderMaster objOrderMaster = new OrderMaster();
                var placeSellerOrderResponse = new SellerOrderResponse();
                placeSellerOrderResponse = context.ValidateSellerCart(sellerShoppingCart, context, out objOrderMaster);
                return Request.CreateResponse(HttpStatusCode.OK, placeSellerOrderResponse);
            }
        }

        [Route("GetPoOrderListAsync")]
        [HttpPost]
        public POOrderMasterResponseDC GetPoOrderListAsync(GetPoOrderListDc getPoOrderListDc)
        {
            using (var context = new AuthContext())
            {
                POOrderMasterResponseDC result = new POOrderMasterResponseDC();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                //Skcode
                var DaysIdDt = new DataTable();
                DaysIdDt.Columns.Add("stringValues");
                if (getPoOrderListDc.Skcode != null && getPoOrderListDc.Skcode.Any())
                {
                    foreach (var item in getPoOrderListDc.Skcode)
                    {
                        var dr = DaysIdDt.NewRow();
                        dr["stringValues"] = item;
                        DaysIdDt.Rows.Add(dr);
                    }
                }
                var Skcodeparam = new SqlParameter("Skcode", DaysIdDt);
                Skcodeparam.SqlDbType = SqlDbType.Structured;
                Skcodeparam.TypeName = "dbo.stringValues";

                //City
                var CityIdIdDts = new DataTable();
                CityIdIdDts.Columns.Add("IntValue");

                if (getPoOrderListDc.CityId != null && getPoOrderListDc.CityId.Any())
                {
                    foreach (var item in getPoOrderListDc.CityId)
                    {
                        var dr = CityIdIdDts.NewRow();
                        dr["IntValue"] = item;
                        CityIdIdDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("CityId", CityIdIdDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.IntValues";

                //Status
                var statusIdDt = new DataTable();
                statusIdDt.Columns.Add("stringValues");
                if (getPoOrderListDc.Status != null && getPoOrderListDc.Status.Any())
                {
                    foreach (var item in getPoOrderListDc.Status)
                    {
                        var dr = statusIdDt.NewRow();
                        dr["stringValues"] = item;
                        statusIdDt.Rows.Add(dr);
                    }
                }
                var Statusparam = new SqlParameter("Status", statusIdDt);
                Statusparam.SqlDbType = SqlDbType.Structured;
                Statusparam.TypeName = "dbo.stringValues";

                //string skcodes = string.Join(",", getPoOrderListDc.Skcode);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetPoOrderListData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Skcodeparam);
                cmd.Parameters.Add(Daysparam);
                cmd.Parameters.Add(Statusparam);
                cmd.Parameters.Add(new SqlParameter("@skip", getPoOrderListDc.skip));
                cmd.Parameters.Add(new SqlParameter("@take", getPoOrderListDc.take));
                cmd.Parameters.Add(new SqlParameter("@Keyword", getPoOrderListDc.Keyword));
                cmd.Parameters.Add(new SqlParameter("@Fromdate", getPoOrderListDc.Fromdate));
                cmd.Parameters.Add(new SqlParameter("@Todate", getPoOrderListDc.Todate));
                cmd.Parameters.Add(new SqlParameter("@SortField", getPoOrderListDc.SortField));
                cmd.Parameters.Add(new SqlParameter("@SortOrder", getPoOrderListDc.SortOrder));
                // cmd.Parameters.Add(new SqlParameter("@Status", getPoOrderListDc.Status));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<PoOrderMasterDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    result.totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }
                result.PoOrderMasterDCList = data;
                return result;
            }
        }


        [Route("ExportPoOrderListAsync")]
        [HttpPost]
        public List<ExportPoOrderMasterDC> ExportPoOrderListAsync(GetPoOrderListDc getPoOrderListDc)
        {
            using (var context = new AuthContext())
            {
                List<ExportPoOrderMasterDC> result = new List<ExportPoOrderMasterDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                //Skcode
                var DaysIdDt = new DataTable();
                DaysIdDt.Columns.Add("stringValues");
                if (getPoOrderListDc.Skcode != null && getPoOrderListDc.Skcode.Any())
                {
                    foreach (var item in getPoOrderListDc.Skcode)
                    {
                        var dr = DaysIdDt.NewRow();
                        dr["stringValues"] = item;
                        DaysIdDt.Rows.Add(dr);
                    }
                }
                var Skcodeparam = new SqlParameter("Skcode", DaysIdDt);
                Skcodeparam.SqlDbType = SqlDbType.Structured;
                Skcodeparam.TypeName = "dbo.stringValues";

                //City
                var CityIdIdDts = new DataTable();
                CityIdIdDts.Columns.Add("IntValue");

                if (getPoOrderListDc.CityId != null && getPoOrderListDc.CityId.Any())
                {
                    foreach (var item in getPoOrderListDc.CityId)
                    {
                        var dr = CityIdIdDts.NewRow();
                        dr["IntValue"] = item;
                        CityIdIdDts.Rows.Add(dr);
                    }
                }
                var CityIdparam = new SqlParameter("CityId", CityIdIdDts);
                CityIdparam.SqlDbType = SqlDbType.Structured;
                CityIdparam.TypeName = "dbo.IntValues";

                //Status
                var statusIdDt = new DataTable();
                statusIdDt.Columns.Add("stringValues");
                if (getPoOrderListDc.Status != null && getPoOrderListDc.Status.Any())
                {
                    foreach (var item in getPoOrderListDc.Status)
                    {
                        var dr = statusIdDt.NewRow();
                        dr["stringValues"] = item;
                        statusIdDt.Rows.Add(dr);
                    }
                }
                var Statusparam = new SqlParameter("Status", statusIdDt);
                Statusparam.SqlDbType = SqlDbType.Structured;
                Statusparam.TypeName = "dbo.stringValues";

                //string skcodes = string.Join(",", getPoOrderListDc.Skcode);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[ExportPoOrderListData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Skcodeparam);
                cmd.Parameters.Add(CityIdparam);
                cmd.Parameters.Add(Statusparam);
                cmd.Parameters.Add(new SqlParameter("@Skip", getPoOrderListDc.skip));
                cmd.Parameters.Add(new SqlParameter("@Take", getPoOrderListDc.take));
                cmd.Parameters.Add(new SqlParameter("@Keyword", getPoOrderListDc.Keyword));
                cmd.Parameters.Add(new SqlParameter("@Fromdate", getPoOrderListDc.Fromdate));
                cmd.Parameters.Add(new SqlParameter("@Todate", getPoOrderListDc.Todate));
                cmd.Parameters.Add(new SqlParameter("@SortField", getPoOrderListDc.SortField));
                cmd.Parameters.Add(new SqlParameter("@SortOrder", getPoOrderListDc.SortOrder));
                // cmd.Parameters.Add(new SqlParameter("@Status", getPoOrderListDc.Status));

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context).ObjectContext.Translate<ExportPoOrderMasterDC>(reader).ToList();                
                return result;
            }
        }

        [Route("GetOrderInvoiceHtml")]
        [HttpPost]
        public async Task<string> GetOrderInvoiceHtml(int id, int warehouseId, int customerId)
        {
            OrderInvoice invoice = new OrderInvoice();
            string expiredHtml = string.Empty;
            string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/orderMasterInvoice.html";
            string content = File.ReadAllText(pathToHTMLFile);
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                int CompanyId = compid;

                OrderDispatchedMaster order = new OrderDispatchedMaster();
                Warehouse warehouseDetail = new Warehouse();
                warehouseDetail = db.getwarehousebyid(warehouseId, CompanyId);
                var Query = "exec GetOrderpayment " + id;
                var paymentdetail = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();
                var invoiceOrderOffer = new InvoiceOrderOffer();
                List<InvoiceOrderOffer> invoiceOrderOffers = new List<InvoiceOrderOffer>();
                var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + id + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + id + " group by orderid";
                invoiceOrderOffers = db.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
                if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
                {
                    var offerCodes = invoiceOrderOffers.Select(x => x.OfferCode).ToList();
                    invoiceOrderOffer.OfferCode = string.Join(",", offerCodes);
                    double totalBillDicount = 0;
                    foreach (var item in invoiceOrderOffers)
                    {
                        if (item.BillDiscountAmount > 0)
                            totalBillDicount += item.BillDiscountAmount;
                        else
                            totalBillDicount += item.BillDiscountTypeValue;
                    }
                    invoiceOrderOffer.BillDiscountAmount = totalBillDicount > 0 ? totalBillDicount / 10 : 0;
                }
                var CustomerCount = (from i in db.Customers
                                     join k in db.DbOrderMaster on i.CustomerId equals k.CustomerId
                                     join com in db.Companies on i.CompanyId equals com.Id
                                     where k.OrderId == id && i.CustomerVerify == "Temporary Active"
                                     select new OrderCountInfo
                                     {
                                         OrderCount = (db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.CreatedDate >= com.InActiveCustomerCountDate && x.CreatedDate <= k.CreatedDate).Count()),
                                         MaxOrderLimit = com.InActiveCustomerCount ?? 0,
                                     }).FirstOrDefault();
                OrderMaster OrderMasterData = db.DbOrderMaster.Where(x => x.OrderId == id).Include("OrderDetails").FirstOrDefault();
                var AddDatalists = OrderMasterData.orderDetails.Where(z => z.OrderId == id).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
                {
                    HSNCode = x.Key.HSNCode,
                    AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
                    SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
                    CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
                    TaxAmmount = x.Sum(y => y.TaxAmmount),
                    CessTaxAmount = x.Sum(y => y.CessTaxAmount),
                    TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
                }).ToList();
                string SumDataHSNDetailRows = "";
                if (AddDatalists != null && AddDatalists.Count() > 0)
                {
                    int rowNumber = 1;
                    foreach (var SumDataHSNDetail in AddDatalists)
                    {
                        SumDataHSNDetailRows += @"<tr>"
                                + "<td>" + rowNumber.ToString() + "</td>"
                                + "<td>" + SumDataHSNDetail.HSNCode + "</td>"
                                + "<td>" + SumDataHSNDetail.AmtWithoutTaxDisc + "</td>"
                                + "<td>" + (order.IsIgstInvoice == false ? SumDataHSNDetail.CGSTTaxAmmount : 0) + "</td>"
                                + "<td>" + (order.IsIgstInvoice == false ? SumDataHSNDetail.SGSTTaxAmmount : 0) + "</td>"
                                + "<td>" + (order.IsIgstInvoice == true ? (SumDataHSNDetail.TaxAmmount + SumDataHSNDetail.CessTaxAmount) : 0) + "</td>"
                                + "<td>" + (SumDataHSNDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? (SumDataHSNDetail.CessTaxAmount) : 0) + "</td>"
                                + "<td>" + (SumDataHSNDetail.AmtWithoutTaxDisc + SumDataHSNDetail.SGSTTaxAmmount + SumDataHSNDetail.CGSTTaxAmmount + SumDataHSNDetail.CessTaxAmount) + "</td>"
                            + "</tr>";

                        rowNumber++;
                    }
                }
                else
                {
                    //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                }
                string result = "";
                var cust = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.RefNo, x.UploadGSTPicture, x.UploadLicensePicture, x.lat, x.lg }).FirstOrDefault();
                if (cust != null)
                {
                    bool Ainfo = true, Binfo = true;
                    if (cust.lat == 0 || cust.lg == 0)
                    {
                        Ainfo = false;
                    }
                    if (string.IsNullOrEmpty(cust.RefNo) && (string.IsNullOrEmpty(cust.UploadGSTPicture) || string.IsNullOrEmpty(cust.UploadLicensePicture)))
                    {
                        Binfo = false;
                    }
                    if (!Ainfo || !Binfo)
                        result = "Your Critical info " + (!Ainfo ? "GPS" : "") + (!Ainfo && !Binfo ? " & " : "") + (!Binfo ? "Shop Licence/GST#" : "") + " is Missing. Your account can be blocked anytime.";

                }

                if (id != null)
                {
                    int Id = Convert.ToInt32(id);
                    order = db.OrderDispatchedMasters.Where(x => x.OrderId == Id).Include("orderDetails").FirstOrDefault();

                    #region add offer type
                    if (order != null)
                    {
                        try
                        {
                            var ordermasters = db.DbOrderMaster.Where(orm => orm.OrderId == Id).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer, x.IsFirstOrder }).FirstOrDefault();

                            order.IsPrimeCustomer = ordermasters.IsPrimeCustomer;
                            order.IsFirstOrder = ordermasters.IsFirstOrder;

                            #region offerdiscounttype
                            if (order.BillDiscountAmount > 0)
                            {
                                var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == order.OrderId && x.CustomerId == order.CustomerId).Select(z => z.OfferId).ToList();
                                if (billdiscountOfferId.Count > 0)
                                {
                                    List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                                    order.offertype = string.Join(",", offeron);
                                }
                            }
                            #endregion
                            //for igst case if true then apply condion to hide column of cgst sgst cess
                            if (!string.IsNullOrEmpty(order.Tin_No) && order.Tin_No.Length >= 11)
                            {
                                string CustTin_No = order.Tin_No.Substring(0, 2);

                                //if (!CustTin_No.StartsWith("0"))
                                //{
                                order.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == order.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
                                //}

                            }

                        }
                        catch (Exception ex) { }

                        //#region RazorPayQR get on order

                        //var RazorPayQR = db.RazorpayVirtualAccounts.Where(x => x.OrderId == order.OrderId && x.IsActive == true).FirstOrDefault();
                        //if (RazorPayQR != null && RazorPayQR.QrCodeUrl != null)
                        //{
                        //    order.RazorPayQR = RazorPayQR.QrCodeUrl;
                        //}
                        //#endregion
                    }
                    #endregion

                    if (order != null && order.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && order.DeliveryIssuanceIdOrderDeliveryMaster.Value > 0)
                    {
                        DeliveryIssuance DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == order.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                        if (DeliveryIssuance.Status == "Payment Accepted" || DeliveryIssuance.Status == "Submitted" || DeliveryIssuance.Status == "Pending")
                        {
                            order.DeliveryIssuanceStatus = DeliveryIssuance.Status;
                            if (order != null)
                            {
                                order.WalletAmount = order.WalletAmount > 0 ? order.WalletAmount : 0;
                                order.offertype = order.offertype != null ? order.offertype : "";
                                order.EwayBillNumber = order.EwayBillNumber != null ? order.EwayBillNumber : "";
                                order.IRNQRCodeUrl = order.IRNQRCodeUrl != null ? order.IRNQRCodeUrl : "";
                                order.POCIRNQRCodeURL = order.POCIRNQRCodeURL != null ? order.POCIRNQRCodeURL : "";
                                order.IRNNo = order.IRNNo != null ? order.IRNNo : "";
                                order.POCIRNNo = order.POCIRNNo != null ? order.POCIRNNo : "";
                                order.PocCreditNoteNumber = order.PocCreditNoteNumber != null ? order.PocCreditNoteNumber : "";
                                var Amount = order.GrossAmount - (order.DiscountAmount > 0 ? order.DiscountAmount : 0);
                                order.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);
                            }
                            if (invoiceOrderOffer.OfferCode == null)
                            {
                                invoiceOrderOffer.OfferCode = "";
                            }
                            if (CustomerCount != null)
                            {
                                expiredHtml = content.Replace("[CustomerCount.MaxOrderLimit]", CustomerCount.MaxOrderLimit.ToString()).Replace("[CustomerCount.OrderCount]", CustomerCount.OrderCount.ToString());
                            }
                            expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", order.InvoiceBarcodeImage.ToString()).Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString()).Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString()).Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString()).Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString()).Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString()).Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString()).Replace("[OrderData1.ShopName]", order.ShopName.ToString()).Replace("[OrderData1.BillingAddress]", order.BillingAddress.ToString()).Replace("[OrderData1.Tin_No]", order.Tin_No.ToString()).Replace("[OrderData1.CustomerName]", order.CustomerName.ToString())
                                .Replace("[OrderData1.Skcode]", order.Skcode.ToString()).Replace("[OrderData1.Customerphonenum]", order.Customerphonenum.ToString()).Replace("[OrderData1.BillingStateName]", order.BillingStateName.ToString()).Replace("[OrderData1.BillingStateCode]", order.BillingStateCode.ToString()).Replace("[OrderData1.IsPrimeCustomer]", order.IsPrimeCustomer.ToString()).Replace("[OrderData1.ShippingAddress]", order.ShippingAddress.ToString()).Replace("[OrderData1.shippingStateName]", order.shippingStateName.ToString()).Replace("[OrderData1.shippingStateCode]", order.shippingStateCode.ToString()).Replace("[OrderData1.invoice_no]", order.invoice_no.ToString()).Replace("[OrderData1.CreatedDate]", order.CreatedDate.ToString()).Replace("[OrderData1.OrderId]", order.OrderId.ToString()).Replace("[OrderData1.OrderedDate]", order.OrderedDate.ToString()).Replace("[OrderData1.PocCreditNoteDate]", order.PocCreditNoteDate.ToString()).Replace("[OrderData1.SalesPerson]", order.SalesPerson.ToString())
                                .Replace("[OrderData1.SalesMobile]", order.SalesMobile.ToString()).Replace("[OrderData1.DboyName]", order.DboyName.ToString()).Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", order.DeliveryIssuanceIdOrderDeliveryMaster.ToString()).Replace("[OrderData1.OrderType]", order.OrderType.ToString()).Replace("[OrderData1.IsIgstInvoice]", order.IsIgstInvoice.ToString()).Replace("[OrderData1.deliveryCharge]", order.deliveryCharge.ToString()).Replace("[OrderData1.paymentThrough]", order.paymentThrough.ToString())
                                .Replace("[OrderData1.WalletAmount]", order.WalletAmount.ToString()).Replace("[OrderData1.PocCreditNoteNumber]", order.PocCreditNoteNumber.ToString()).Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", order.InvoiceAmountInWord.ToString())
                                .Replace("[OrderData1.BillDiscountAmount]", order.BillDiscountAmount.ToString()).Replace("[OrderData1.TCSAmount]", order.TCSAmount.ToString()).Replace("[OrderData1.GrossAmount]", order.GrossAmount.ToString()).Replace("[OrderData1.DiscountAmount]", order.DiscountAmount.ToString()).Replace("[OrderData1.Status]", order.Status.ToString())
                                .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                                .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                                .Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[paymentdetail]", order.Customerphonenum.ToString())
                            .Replace("[OrderData1.EwayBillNumber]", order.EwayBillNumber.ToString()).Replace("[OrderData1.offertype]", order.offertype.ToString())
                            .Replace("[OrderData1.IRNQRCodeUrl]", order.IRNQRCodeUrl.ToString()).Replace("[OrderData1.POCIRNQRCodeURL]", order.POCIRNQRCodeURL.ToString()).Replace("[OrderData1.IRNNo]", order.IRNNo.ToString()).Replace("[OrderData1.POCIRNNo]", order.POCIRNNo.ToString())
                                             ;
                            return expiredHtml;
                        }
                    }
                    if (order != null)
                    {
                        var ExecutiveIds = db.DbOrderDetails.Where(z => z.OrderId == Id && z.ExecutiveId > 0).Select(z => z.ExecutiveId).ToList();
                        if (ExecutiveIds != null && ExecutiveIds.Any())
                        {
                            var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                            order.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                            order.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                        }

                    }
                    if (order != null)
                    {
                        MastersManager mastersManager = new MastersManager();
                        TripPickerIdDc tripPickerIdDc = await mastersManager.GetPickerId_TripId(order.OrderId);
                        if (tripPickerIdDc != null)
                        {
                            order.OrderPickerMasterId = tripPickerIdDc.PickerId == null ? 0 : tripPickerIdDc.PickerId.OrderPickerMasterId;
                            order.TripPlannerMasterId = tripPickerIdDc.TripId == null ? 0 : tripPickerIdDc.TripId.TripPlannerMasterId;
                        }
                    }

                    //---------S-------------------
                    if (order != null)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("IntValue");
                        var dr = dt.NewRow();
                        dr["IntValue"] = order.CustomerId;
                        dt.Rows.Add(dr);
                        var param = new SqlParameter("CustomerId", dt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

                        if (GetStateCodeList != null)
                        {
                            order.shippingStateName = GetStateCodeList.shippingStateName;
                            order.shippingStateCode = GetStateCodeList.shippingStateCode;
                            order.BillingStateName = GetStateCodeList.BillingStateName;
                            order.BillingStateCode = GetStateCodeList.BillingStateCode;
                        }
                    }
                    //---------E-------------------

                }
                if (order != null)
                {
                    order.WalletAmount = order.WalletAmount > 0 ? order.WalletAmount : 0;
                    order.offertype = order.offertype != null ? order.offertype : "";
                    order.EwayBillNumber = order.EwayBillNumber != null ? order.EwayBillNumber : "";
                    order.IRNQRCodeUrl = order.IRNQRCodeUrl != null ? order.IRNQRCodeUrl : "";
                    order.POCIRNQRCodeURL = order.POCIRNQRCodeURL != null ? order.POCIRNQRCodeURL : "";
                    order.IRNNo = order.IRNNo != null ? order.IRNNo : "";
                    order.POCIRNNo = order.POCIRNNo != null ? order.POCIRNNo : "";
                    order.PocCreditNoteNumber = order.PocCreditNoteNumber != null ? order.PocCreditNoteNumber : "";
                    var Amount = order.GrossAmount - (order.DiscountAmount > 0 ? order.DiscountAmount : 0);
                    order.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);
                }
                if (invoiceOrderOffer.OfferCode == null)
                {
                    invoiceOrderOffer.OfferCode = "";
                }
                double totalTaxableValue = 0;
                double totalIGST = 0;
                double totalCGST = 0;
                double totalSGST = 0;
                double totalCess = 0;
                double TotalIOverall = 0;
                double totalAmtIncTaxes = 0;
                var OrderData = order.orderDetails;
                foreach (var i in OrderData)
                {
                    totalTaxableValue = totalTaxableValue + i.AmtWithoutTaxDisc;
                    totalIGST = totalIGST + i.TaxAmmount + i.CessTaxAmount;
                    totalCGST = totalCGST + i.CGSTTaxAmmount;
                    totalSGST = totalSGST + i.SGSTTaxAmmount;
                    totalCess = totalCess + i.CessTaxAmount;
                    TotalIOverall = TotalIOverall + i.AmtWithoutTaxDisc + i.SGSTTaxAmmount + i.CGSTTaxAmmount + i.CessTaxAmount;
                    totalAmtIncTaxes = totalAmtIncTaxes + i.TotalAmt;
                }
                string OrderDataRows = "";
                if (OrderData != null && OrderData.Count() > 0)
                {
                    int rowNumber = 1;
                    foreach (var orderDetail in OrderData)
                    {
                        OrderDataRows += @"<tr>"
                                + "<td>" + rowNumber.ToString() + "</td>"
                                + "<td>" + orderDetail.itemname + (orderDetail.IsFreeItem ? "Free Item" : "") + "</td>"
                                + "<td>" + orderDetail.price + "</td>"
                                + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? orderDetail.price / (1 + orderDetail.PTR / 100) : 0) + "</td>"
                                + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 : 0) + "</td>"
                                + "<td>" + ((orderDetail.UnitPrice == 0.0001 || orderDetail.UnitPrice == 0.01) ? (orderDetail.UnitPrice) : (orderDetail.UnitPrice)) + "</td>"
                                + "<td>" + orderDetail.MinOrderQty + "</td>"
                                + "<td>" + orderDetail.qty / orderDetail.MinOrderQty + "</td>"
                                + "<td>" + orderDetail.Noqty + "</td>"
                                + "<td>" + orderDetail.AmtWithoutTaxDisc + "</td>"
                                + "<td>" + orderDetail.HSNCode + "</td>"
                                + "<td>" + orderDetail.DiscountAmmount + "</td>"
                                 + "<td>" + orderDetail.HSNCode + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == true ? orderDetail.TaxPercentage + orderDetail.TotalCessPercentage : 0) + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == true ? (orderDetail.TaxAmmount + orderDetail.CessTaxAmount) : 0) + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == false ? orderDetail.SGSTTaxPercentage : 0) + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == false ? (orderDetail.SGSTTaxAmmount) : 0) + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == false ? orderDetail.CGSTTaxPercentage : 0) + "</td>"
                                  + "<td>" + (order.IsIgstInvoice == false ? (orderDetail.CGSTTaxAmmount) : 0) + "</td>"
                                  + "<td>" + (orderDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? orderDetail.TotalCessPercentage : 0) + "</td>"
                                  + "<td>" + (orderDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? orderDetail.CessTaxAmount : 0) + "</td>"
                                  + "<td>" + orderDetail.TotalAmt + "</td>"
                            + "</tr>";

                        rowNumber++;
                    }
                }
                else
                {
                    //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                }
                expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", order.InvoiceBarcodeImage.ToString()).Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString()).Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString()).Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString()).Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString()).Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString()).Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString()).Replace("[OrderData1.ShopName]", order.ShopName.ToString()).Replace("[OrderData1.BillingAddress]", order.BillingAddress.ToString()).Replace("[OrderData1.Tin_No]", order.Tin_No.ToString()).Replace("[OrderData1.CustomerName]", order.CustomerName.ToString())
                    .Replace("[OrderData1.Skcode]", order.Skcode.ToString()).Replace("[OrderData1.Customerphonenum]", order.Customerphonenum.ToString()).Replace("[OrderData1.BillingStateName]", order.BillingStateName.ToString()).Replace("[OrderData1.BillingStateCode]", order.BillingStateCode.ToString()).Replace("[OrderData1.IsPrimeCustomer]", order.IsPrimeCustomer.ToString()).Replace("[OrderData1.ShippingAddress]", order.ShippingAddress.ToString()).Replace("[OrderData1.shippingStateName]", order.shippingStateName.ToString()).Replace("[OrderData1.shippingStateCode]", order.shippingStateCode.ToString()).Replace("[OrderData1.invoice_no]", order.invoice_no.ToString()).Replace("[OrderData1.CreatedDate]", order.CreatedDate.ToString()).Replace("[OrderData1.OrderId]", order.OrderId.ToString()).Replace("[OrderData1.OrderedDate]", order.OrderedDate.ToString()).Replace("[OrderData1.PocCreditNoteDate]", order.PocCreditNoteDate.ToString()).Replace("[OrderData1.SalesPerson]", order.SalesPerson.ToString())
                    .Replace("[OrderData1.SalesMobile]", order.SalesMobile.ToString()).Replace("[OrderData1.DboyName]", order.DboyName.ToString()).Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", order.DeliveryIssuanceIdOrderDeliveryMaster.ToString()).Replace("[OrderData1.OrderType]", order.OrderType.ToString()).Replace("[OrderData1.IsIgstInvoice]", order.IsIgstInvoice.ToString()).Replace("[OrderData1.deliveryCharge]", order.deliveryCharge.ToString()).Replace("[OrderData1.paymentThrough]", order.paymentThrough.ToString())
                    .Replace("[OrderData1.WalletAmount]", order.WalletAmount.ToString()).Replace("[OrderData1.PocCreditNoteNumber]", order.PocCreditNoteNumber.ToString()).Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", order.InvoiceAmountInWord.ToString())
                    .Replace("[OrderData1.BillDiscountAmount]", order.BillDiscountAmount.ToString()).Replace("[OrderData1.TCSAmount]", order.TCSAmount.ToString()).Replace("[OrderData1.GrossAmount]", order.GrossAmount.ToString()).Replace("[OrderData1.DiscountAmount]", order.DiscountAmount.ToString()).Replace("[OrderData1.Status]", order.Status.ToString())
                    .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                    .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                    .Replace("[CustomerCriticalInfo]", result.ToString())
                .Replace("[OrderData1.EwayBillNumber]", order.EwayBillNumber.ToString()).Replace("[OrderData1.offertype]", order.offertype.ToString())
                .Replace("[OrderData1.IRNQRCodeUrl]", order.IRNQRCodeUrl.ToString()).Replace("[OrderData1.POCIRNQRCodeURL]", order.POCIRNQRCodeURL.ToString()).Replace("[OrderData1.IRNNo]", order.IRNNo.ToString())
                .Replace("[OrderData1.POCIRNNo]", order.POCIRNNo.ToString())
                .Replace("[InvoiceAmt]", (order.GrossAmount - order.DiscountAmount).ToString())
                .Replace("[amount]", (paymentdetail[0].amount > 0 ? paymentdetail[0].amount : paymentdetail[0].amount).ToString())
                .Replace("[PaymentFrom]", (paymentdetail[0].PaymentFrom != null ? paymentdetail[0].PaymentFrom : paymentdetail[0].PaymentFrom).ToString())
                .Replace("[IsOnline]", (paymentdetail[0].amount > 0 && paymentdetail[0].IsOnline ? "Paid" : "Refund"))
                .Replace("##SumDataHSNROWS##", SumDataHSNDetailRows)
                .Replace("##OrderDataRows##", OrderDataRows)
                //.Replace("##getOrderDataRows##", getOrderDataRows)
                .Replace("[totalTaxableValue]", totalTaxableValue.ToString())
                .Replace("[totalIGST]", totalIGST.ToString())
                .Replace("[totalCGST]", totalCGST.ToString())
                .Replace("[totalSGST]", totalSGST.ToString())
                .Replace("[totalCess]", totalCess.ToString())
                .Replace("[TotalIOverall]", TotalIOverall.ToString())
                 .Replace("[OrderLimit]", (CustomerCount != null ? (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount).ToString() : ""))
                 .Replace("[CurrentorderCount]", (CustomerCount != null ? (CustomerCount.OrderCount / CustomerCount.MaxOrderLimit).ToString() : ""))
                .Replace("[CustomerCount.MaxOrderLimit]", (CustomerCount != null ? CustomerCount.MaxOrderLimit.ToString() : "")).Replace("[CustomerCount.OrderCount]", (CustomerCount != null ? CustomerCount.OrderCount.ToString() : ""));
                //string fileName = Guid.NewGuid() + ".pdf";
                //string downoadFolderName = ConfigurationManager.AppSettings["AllFileDownloadFolder"].ToString();
                //PDFMaker invoicePDFMaker = new PDFMaker(pathToHTMLFile, fileName, content,null,null,null);
                //invoicePDFMaker.SavePdf();
                string htmlUrlString = JsonConvert.SerializeObject(expiredHtml);
                var existingOrderId = db.OrderInvoiceDB.Where(x => x.orderId == id).FirstOrDefault();
                if (existingOrderId == null)
                {
                    invoice.CreatedDate = DateTime.Now;
                    invoice.CreatedBy = userid;
                    invoice.orderId = id;
                    invoice.invoiceURL = htmlUrlString;
                    invoice.IsActive = true;
                    db.Entry(invoice).State = EntityState.Added;
                    var a = db.Commit();
                }
            }
            //var PDF = Renderer.RenderHtmlAsPdf(expiredHtml);
            //// output to pdf
            //var OutputPath = Server.MapPath("HtmlToPDF.pdf");
            //PDF.SaveAs(OutputPath);
            //// after pdf saved it will open it in the default browser
            //System.Diagnostics.Process.Start(OutputPath);
            return expiredHtml;
        }

        [Route("GetOrderInVoiceData")]
        [HttpGet]
        public async Task<OrderInvoice> GetOrderInVoiceData(int Id)
        {
            OrderInvoice orderConcernMasterDc = new OrderInvoice();
            using (var context = new AuthContext())
            {
                var invoiceData = context.OrderInvoiceDB.Where(x => x.orderId == Id).FirstOrDefault();
                return invoiceData;
            }

        }


        [Route("UpdateSellerStorePriceByMultiMrpId")]
        [HttpGet]
        public string UpdateSellerStorePriceByMultiMrpId(int ItemMultiMrpId, double SellerStorePrice, int Cityid)
        {
            // bool result = false;
            var msg = "";
            using (var db = new AuthContext())
            {
                var list = db.itemMasters.Where(x => x.ItemMultiMRPId == ItemMultiMrpId && x.Cityid == Cityid && x.IsSellerStoreItem == true).ToList();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (list.Count == 0)
                {
                    msg = "No Item found on this MRP";
                }
                else if (list.Count > 0 && Cityid > 0)
                {
                    foreach (var lst in list)
                    {
                        lst.UpdatedDate = DateTime.Now;
                        lst.ModifiedBy = userid;
                        lst.SellerStorePrice = SellerStorePrice;
                        db.Entry(lst).State = EntityState.Modified;
                    }
                    db.Commit();
                    msg = "Seller Store Price Updated Successfully";
                }
                return msg;
            }
        }

        //[Route("GetBackendOrderInvoiceHtml")]
        //[HttpPost]
        //public async Task<string> GetOrderInvoiceHtml(int id)
        //{
        //    OrderInvoice invoice = new OrderInvoice();
        //    string expiredHtml = string.Empty;
        //    string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/CustomerBackendOrder.html";
        //    string content = File.ReadAllText(pathToHTMLFile);
        //    int warehouseId, customerId = 0;
        //    using (var db = new AuthContext())
        //    {
        //        var data = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == id);
        //        customerId = data.CustomerId;
        //        warehouseId = data.WarehouseId;
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 1, userid = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        int CompanyId = compid;

        //        OrderDispatchedMaster order = new OrderDispatchedMaster();
        //        Warehouse warehouseDetail = new Warehouse();
        //        warehouseDetail = db.getwarehousebyid(warehouseId, CompanyId);
        //        var Query = "exec GetOrderpayment " + id;
        //        var paymentdetail = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();
        //        var invoiceOrderOffer = new InvoiceOrderOffer();
        //        List<InvoiceOrderOffer> invoiceOrderOffers = new List<InvoiceOrderOffer>();
        //        var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + id + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + id + " group by orderid";
        //        invoiceOrderOffers = db.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
        //        if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
        //        {
        //            var offerCodes = invoiceOrderOffers.Select(x => x.OfferCode).ToList();
        //            invoiceOrderOffer.OfferCode = string.Join(",", offerCodes);
        //            double totalBillDicount = 0;
        //            foreach (var item in invoiceOrderOffers)
        //            {
        //                if (item.BillDiscountAmount > 0)
        //                    totalBillDicount += item.BillDiscountAmount;
        //                else
        //                    totalBillDicount += item.BillDiscountTypeValue;
        //            }
        //            invoiceOrderOffer.BillDiscountAmount = totalBillDicount > 0 ? totalBillDicount / 10 : 0;
        //        }
        //        var CustomerCount = (from i in db.Customers
        //                             join k in db.DbOrderMaster on i.CustomerId equals k.CustomerId
        //                             join com in db.Companies on i.CompanyId equals com.Id
        //                             where k.OrderId == id && i.CustomerVerify == "Temporary Active"
        //                             select new OrderCountInfo
        //                             {
        //                                 OrderCount = (db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.CreatedDate >= com.InActiveCustomerCountDate && x.CreatedDate <= k.CreatedDate).Count()),
        //                                 MaxOrderLimit = com.InActiveCustomerCount ?? 0,
        //                             }).FirstOrDefault();
        //        OrderMaster OrderMasterData = db.DbOrderMaster.Where(x => x.OrderId == id).Include("OrderDetails").FirstOrDefault();
        //        var AddDatalists = OrderMasterData.orderDetails.Where(z => z.OrderId == id).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
        //        {
        //            HSNCode = x.Key.HSNCode,
        //            AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
        //            SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
        //            CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
        //            TaxAmmount = x.Sum(y => y.TaxAmmount),
        //            CessTaxAmount = x.Sum(y => y.CessTaxAmount),
        //            TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
        //        }).ToList();
        //        string SumDataHSNDetailRows = "";
        //        if (AddDatalists != null && AddDatalists.Count() > 0)
        //        {
        //            int rowNumber = 1;
        //            foreach (var SumDataHSNDetail in AddDatalists)
        //            {
        //                SumDataHSNDetailRows += @"<tr>"
        //                        //+ "<td>" + rowNumber.ToString() + "</td>"
        //                        + "<td>" + SumDataHSNDetail.HSNCode + "</td>"
        //                        + "<td>" + Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc,2) + "</td>"
        //                        + "<td>" + (order.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.CGSTTaxAmmount,2) : 0) + "</td>"
        //                        + "<td>" + (order.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.SGSTTaxAmmount,2) : 0) + "</td>"
        //                        //+ "<td>" + (order.IsIgstInvoice == true ? Math.Round((SumDataHSNDetail.TaxAmmount + SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
        //                        //+ "<td>" + (SumDataHSNDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? Math.Round((SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
        //                        + "<td>" + (Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc, 2) + Math.Round(SumDataHSNDetail.SGSTTaxAmmount, 2) + Math.Round(SumDataHSNDetail.CGSTTaxAmmount, 2) + SumDataHSNDetail.CessTaxAmount) + "</td>"
        //                    + "</tr>";

        //                rowNumber++;
        //            }
        //        }
        //        else
        //        {
        //            //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
        //        }
        //        string result = "";
        //        var cust = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.RefNo, x.UploadGSTPicture, x.UploadLicensePicture, x.lat, x.lg }).FirstOrDefault();
        //        if (cust != null)
        //        {
        //            bool Ainfo = true, Binfo = true;
        //            if (cust.lat == 0 || cust.lg == 0)
        //            {
        //                Ainfo = false;
        //            }
        //            if (string.IsNullOrEmpty(cust.RefNo) && (string.IsNullOrEmpty(cust.UploadGSTPicture) || string.IsNullOrEmpty(cust.UploadLicensePicture)))
        //            {
        //                Binfo = false;
        //            }
        //            if (!Ainfo || !Binfo)
        //                result = "Your Critical info " + (!Ainfo ? "GPS" : "") + (!Ainfo && !Binfo ? " & " : "") + (!Binfo ? "Shop Licence/GST#" : "") + " is Missing. Your account can be blocked anytime.";

        //        }

        //        if (id != null)
        //        {
        //            int Id = Convert.ToInt32(id);
        //            order = db.OrderDispatchedMasters.Where(x => x.OrderId == Id).Include("orderDetails").FirstOrDefault();

        //            #region add offer type
        //            if (order != null)
        //            {
        //                try
        //                {
        //                    var ordermasters = db.DbOrderMaster.Where(orm => orm.OrderId == Id).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer, x.IsFirstOrder }).FirstOrDefault();

        //                    order.IsPrimeCustomer = ordermasters.IsPrimeCustomer;
        //                    order.IsFirstOrder = ordermasters.IsFirstOrder;

        //                    #region offerdiscounttype
        //                    if (order.BillDiscountAmount > 0)
        //                    {
        //                        var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == order.OrderId && x.CustomerId == order.CustomerId).Select(z => z.OfferId).ToList();
        //                        if (billdiscountOfferId.Count > 0)
        //                        {
        //                            List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
        //                            order.offertype = string.Join(",", offeron);
        //                        }
        //                    }
        //                    #endregion
        //                    //for igst case if true then apply condion to hide column of cgst sgst cess
        //                    if (!string.IsNullOrEmpty(order.Tin_No) && order.Tin_No.Length >= 11)
        //                    {
        //                        string CustTin_No = order.Tin_No.Substring(0, 2);

        //                        //if (!CustTin_No.StartsWith("0"))
        //                        //{
        //                        order.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == order.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
        //                        //}

        //                    }

        //                }
        //                catch (Exception ex) { }

        //                //#region RazorPayQR get on order

        //                //var RazorPayQR = db.RazorpayVirtualAccounts.Where(x => x.OrderId == order.OrderId && x.IsActive == true).FirstOrDefault();
        //                //if (RazorPayQR != null && RazorPayQR.QrCodeUrl != null)
        //                //{
        //                //    order.RazorPayQR = RazorPayQR.QrCodeUrl;
        //                //}
        //                //#endregion
        //            }
        //            #endregion

        //            if (order != null)
        //            {
        //                DataTable dt = new DataTable();
        //                dt.Columns.Add("IntValue");
        //                var dr = dt.NewRow();
        //                dr["IntValue"] = order.CustomerId;
        //                dt.Rows.Add(dr);
        //                var param = new SqlParameter("CustomerId", dt);
        //                param.SqlDbType = SqlDbType.Structured;
        //                param.TypeName = "dbo.IntValues";

        //                var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

        //                if (GetStateCodeList != null)
        //                {
        //                    order.shippingStateName = GetStateCodeList.shippingStateName != null ? GetStateCodeList.shippingStateName : " ";
        //                    order.shippingStateCode = GetStateCodeList.shippingStateCode != null ? GetStateCodeList.shippingStateCode : " "; ;
        //                    order.BillingStateName = GetStateCodeList.BillingStateName != null ? GetStateCodeList.BillingStateName : " "; ;
        //                    order.BillingStateCode = GetStateCodeList.BillingStateCode != null ? GetStateCodeList.BillingStateCode : " "; ;
        //                }
        //            }

        //            if (order != null && order.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && order.DeliveryIssuanceIdOrderDeliveryMaster.Value > 0)
        //            {
        //                DeliveryIssuance DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == order.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
        //                if (DeliveryIssuance.Status == "Payment Accepted" || DeliveryIssuance.Status == "Submitted" || DeliveryIssuance.Status == "Pending")
        //                {
        //                    order.DeliveryIssuanceStatus = DeliveryIssuance.Status;
        //                    if (order != null)
        //                    {
        //                        order.WalletAmount = order.WalletAmount > 0 ? order.WalletAmount : 0;
        //                        order.offertype = order.offertype != null ? order.offertype : "";
        //                        order.EwayBillNumber = order.EwayBillNumber != null ? order.EwayBillNumber : "";
        //                        order.IRNQRCodeUrl = order.IRNQRCodeUrl != null ? order.IRNQRCodeUrl : "";
        //                        order.POCIRNQRCodeURL = order.POCIRNQRCodeURL != null ? order.POCIRNQRCodeURL : "";
        //                        order.IRNNo = order.IRNNo != null ? order.IRNNo : "";
        //                        order.POCIRNNo = order.POCIRNNo != null ? order.POCIRNNo : "";
        //                        order.PocCreditNoteNumber = order.PocCreditNoteNumber != null ? order.PocCreditNoteNumber : "";
        //                        var Amount = order.GrossAmount - (order.DiscountAmount > 0 ? order.DiscountAmount : 0);
        //                        order.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);
        //                    }
        //                    if (invoiceOrderOffer.OfferCode == null)
        //                    {
        //                        invoiceOrderOffer.OfferCode = "";
        //                    }
        //                    if (CustomerCount != null)
        //                    {
        //                        expiredHtml = content.Replace("[CustomerCount.MaxOrderLimit]", CustomerCount.MaxOrderLimit.ToString()).Replace("[CustomerCount.OrderCount]", CustomerCount.OrderCount.ToString());
        //                    }
        //                    order.ShopName = order.ShopName != null ? order.ShopName : "";
        //                    order.Tin_No = order.Tin_No != null ? order.Tin_No : "";
        //                    order.invoice_no = order.invoice_no != null ? order.invoice_no : "";
        //                    //order.PocCreditNoteDate = order.PocCreditNoteDate != null ? order.PocCreditNoteDate : " ";
        //                    order.SalesMobile = order.SalesMobile != null ? order.SalesMobile : "";
        //                    order.SalesPerson = order.SalesPerson != null ? order.SalesPerson : "";
        //                    order.DeliveryIssuanceIdOrderDeliveryMaster = order.DeliveryIssuanceIdOrderDeliveryMaster != null ? order.DeliveryIssuanceIdOrderDeliveryMaster : 0;
        //                    order.paymentThrough = order.paymentThrough != null ? order.paymentThrough : " ";

        //                    expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", order.InvoiceBarcodeImage.ToString())
        //                        .Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString())
        //                        .Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString())
        //                        .Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString())
        //                        .Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString())
        //                        .Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString())
        //                        .Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString())
        //                        .Replace("[OrderData1.ShopName]", order.ShopName.ToString())
        //                        .Replace("[OrderData1.BillingAddress]", order.BillingAddress.ToString())
        //                        .Replace("[OrderData1.Tin_No]", order.Tin_No.ToString())
        //                        .Replace("[OrderData1.CustomerName]", order.CustomerName.ToString())
        //                        .Replace("[OrderData1.Skcode]", order.Skcode.ToString())
        //                        .Replace("[OrderData1.Customerphonenum]", order.Customerphonenum.ToString())
        //                        .Replace("[OrderData1.BillingStateName]", order.BillingStateName.ToString())
        //                        .Replace("[OrderData1.BillingStateCode]", order.BillingStateCode.ToString())
        //                        //.Replace("[OrderData1.IsPrimeCustomer]", order.IsPrimeCustomer.ToString())
        //                        .Replace("[OrderData1.ShippingAddress]", order.ShippingAddress.ToString())
        //                        .Replace("[OrderData1.shippingStateName]", order.shippingStateName.ToString())
        //                        .Replace("[OrderData1.shippingStateCode]", order.shippingStateCode.ToString())
        //                        .Replace("[OrderData1.invoice_no]", order.invoice_no.ToString())
        //                        .Replace("[OrderData1.CreatedDate]", order.CreatedDate.ToString())
        //                        .Replace("[OrderData1.OrderId]", order.OrderId.ToString())
        //                        .Replace("[OrderData1.OrderedDate]", order.OrderedDate.ToString())
        //                        //.Replace("[OrderData1.PocCreditNoteDate]", order.PocCreditNoteDate.ToString())
        //                        .Replace("[OrderData1.SalesPerson]", order.SalesPerson.ToString())
        //                        .Replace("[OrderData1.SalesMobile]", order.SalesMobile.ToString())
        //                        .Replace("[OrderData1.DboyName]", order.DboyName.ToString())
        //                        .Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", order.DeliveryIssuanceIdOrderDeliveryMaster.ToString())
        //                        .Replace("[OrderData1.OrderType]", order.OrderType.ToString())
        //                        .Replace("[OrderData1.IsIgstInvoice]", order.IsIgstInvoice.ToString())
        //                        .Replace("[OrderData1.deliveryCharge]", order.deliveryCharge.ToString())
        //                        .Replace("[OrderData1.paymentThrough]", order.paymentThrough.ToString())
        //                        .Replace("[OrderData1.WalletAmount]", order.WalletAmount.ToString())
        //                        .Replace("[OrderData1.PocCreditNoteNumber]", order.PocCreditNoteNumber.ToString())
        //                        .Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", order.InvoiceAmountInWord.ToString())
        //                        .Replace("[OrderData1.BillDiscountAmount]", order.BillDiscountAmount.ToString())
        //                        .Replace("[OrderData1.TCSAmount]", order.TCSAmount.ToString())
        //                        .Replace("[OrderData1.GrossAmount]", order.GrossAmount.ToString())
        //                        .Replace("[OrderData1.DiscountAmount]", order.DiscountAmount.ToString())
        //                        .Replace("[OrderData1.Status]", order.Status.ToString())
        //                        .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
        //                        .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
        //                        .Replace("[CustomerCriticalInfo]", result.ToString())
        //                        .Replace("[paymentdetail]", order.Customerphonenum.ToString())
        //                        .Replace("[OrderData1.EwayBillNumber]", order.EwayBillNumber.ToString())
        //                        .Replace("[OrderData1.offertype]", order.offertype.ToString())
        //                        .Replace("[OrderData1.IRNQRCodeUrl]", order.IRNQRCodeUrl.ToString())
        //                        .Replace("[OrderData1.POCIRNQRCodeURL]", order.POCIRNQRCodeURL.ToString())
        //                        //.Replace("[OrderData1.IRNNo]", order.IRNNo.ToString())
        //                        //.Replace("[OrderData1.POCIRNNo]", order.POCIRNNo.ToString())
        //                                     ;

        //                }
        //            }
        //            if (order != null)
        //            {
        //                var ExecutiveIds = db.DbOrderDetails.Where(z => z.OrderId == Id && z.ExecutiveId > 0).Select(z => z.ExecutiveId).ToList();
        //                if (ExecutiveIds != null && ExecutiveIds.Any())
        //                {
        //                    var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
        //                    order.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
        //                    order.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
        //                }

        //            }
        //            if (order != null)
        //            {
        //                MastersManager mastersManager = new MastersManager();
        //                TripPickerIdDc tripPickerIdDc = await mastersManager.GetPickerId_TripId(order.OrderId);
        //                if (tripPickerIdDc != null)
        //                {
        //                    order.OrderPickerMasterId = tripPickerIdDc.PickerId == null ? 0 : tripPickerIdDc.PickerId.OrderPickerMasterId;
        //                    order.TripPlannerMasterId = tripPickerIdDc.TripId == null ? 0 : tripPickerIdDc.TripId.TripPlannerMasterId;
        //                }
        //            }

        //            //---------S-------------------

        //            //---------E-------------------

        //        }
        //        if (order != null)
        //        {
        //            order.WalletAmount = order.WalletAmount > 0 ? order.WalletAmount : 0;
        //            order.offertype = order.offertype != null ? order.offertype : "";
        //            order.EwayBillNumber = order.EwayBillNumber != null ? order.EwayBillNumber : "";
        //            order.IRNQRCodeUrl = order.IRNQRCodeUrl != null ? order.IRNQRCodeUrl : "";
        //            order.POCIRNQRCodeURL = order.POCIRNQRCodeURL != null ? order.POCIRNQRCodeURL : "";
        //            order.IRNNo = order.IRNNo != null ? order.IRNNo : "";
        //            order.POCIRNNo = order.POCIRNNo != null ? order.POCIRNNo : "";
        //            order.PocCreditNoteNumber = order.PocCreditNoteNumber != null ? order.PocCreditNoteNumber : "";
        //            var Amount = order.GrossAmount - (order.DiscountAmount > 0 ? order.DiscountAmount : 0);
        //            order.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);
        //        }
        //        if (invoiceOrderOffer.OfferCode == null)
        //        {
        //            invoiceOrderOffer.OfferCode = "";
        //        }
        //        double totalTaxableValue = 0;
        //        double totalIGST = 0;
        //        double totalCGST = 0;
        //        double totalSGST = 0;
        //        double totalCess = 0;
        //        double TotalIOverall = 0;
        //        double totalAmtIncTaxes = 0;
        //        var OrderData = order.orderDetails;
        //        foreach (var i in OrderData)
        //        {
        //            totalTaxableValue = totalTaxableValue + i.AmtWithoutTaxDisc;
        //            totalIGST = Math.Round( totalIGST + i.TaxAmmount + i.CessTaxAmount,2);
        //            totalCGST = Math.Round((totalCGST + i.CGSTTaxAmmount), 2);
        //            totalSGST = Math.Round((totalSGST + i.SGSTTaxAmmount), 2);
        //            totalCess = Math.Round((totalCess + i.CessTaxAmount),2);
        //            TotalIOverall = Math.Round(TotalIOverall + i.AmtWithoutTaxDisc + i.SGSTTaxAmmount + i.CGSTTaxAmmount + i.CessTaxAmount,2);
        //            totalAmtIncTaxes = Math.Round(totalAmtIncTaxes + i.TotalAmt,2);
        //        }
        //        string OrderDataRows = "";
        //        if (OrderData != null && OrderData.Count() > 0)
        //        {
        //            int rowNumber = 1;
        //            foreach (var orderDetail in OrderData)
        //            {
        //                OrderDataRows += @"<tr>"
        //                        + "<td>" + rowNumber.ToString() + "</td>"
        //                        + "<td>" + orderDetail.itemname + (orderDetail.IsFreeItem ? "Free Item" : "") + "</td>"
        //                        + "<td>" + orderDetail.price + "</td>"
        //                        + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? orderDetail.price / (1 + orderDetail.PTR / 100) : 0) + "</td>"
        //                        + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 : 0) + "</td>"
        //                        + "<td>" + ((orderDetail.UnitPrice == 0.0001 || orderDetail.UnitPrice == 0.01) ? (orderDetail.UnitPrice) : (orderDetail.UnitPrice)) + "</td>"
        //                        + "<td>" + orderDetail.MinOrderQty + "</td>"
        //                        + "<td>" + orderDetail.qty / orderDetail.MinOrderQty + "</td>"
        //                        + "<td>" + orderDetail.Noqty + "</td>"
        //                        + "<td>" + Math.Round(orderDetail.AmtWithoutTaxDisc,2) + "</td>"
        //                        + "<td>" + orderDetail.HSNCode + "</td>"
        //                        //+ "<td>" + orderDetail.DiscountAmmount + "</td>"
        //                        // + "<td>" + orderDetail.HSNCode + "</td>"
        //                          //+ "<td>" + (order.IsIgstInvoice == true ? orderDetail.TaxPercentage + orderDetail.TotalCessPercentage : 0) + "</td>"
        //                          //+ "<td>" + (order.IsIgstInvoice == true ? Math.Round((orderDetail.TaxAmmount + orderDetail.CessTaxAmount),2) : 0) + "</td>"
        //                          + "<td>" + (order.IsIgstInvoice == false ? orderDetail.SGSTTaxPercentage : 0) + "</td>"
        //                          + "<td>" + (order.IsIgstInvoice == false ? Math.Round((orderDetail.SGSTTaxAmmount),2) : 0) + "</td>"
        //                          + "<td>" + (order.IsIgstInvoice == false ? orderDetail.CGSTTaxPercentage : 0) + "</td>"
        //                          + "<td>" + (order.IsIgstInvoice == false ? Math.Round((orderDetail.CGSTTaxAmmount),2) : 0) + "</td>"
        //                          //+ "<td>" + (orderDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? orderDetail.TotalCessPercentage : 0) + "</td>"
        //                          //+ "<td>" + (orderDetail.CessTaxAmount > 0 && order.IsIgstInvoice == false ? orderDetail.CessTaxAmount : 0) + "</td>"
        //                          + "<td>" + Math.Round(orderDetail.TotalAmt,2) + "</td>"
        //                    + "</tr>";

        //                rowNumber++;
        //            }
        //        }
        //        else
        //        {
        //            //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
        //        }
        //        string ordertype = "";
        //        if (order.OrderType == 1 || order.OrderType == 0)
        //        {
        //            ordertype = "General Order";
        //        }
        //        else if(order.OrderType == 2)
        //        {
        //            ordertype = "Bundle Order";
        //        }
        //        else if (order.OrderType == 3)
        //        {
        //            ordertype = "Return Order";
        //        }
        //        else if (order.OrderType == 4)
        //        {
        //            ordertype = "Distributer Order";
        //        }
        //        else if (order.OrderType == 6)
        //        {
        //            ordertype = "Damage Order";
        //        }
        //        else
        //        {
        //            ordertype = "Order";
        //        }
        //        expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", order.InvoiceBarcodeImage.ToString()).Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString()).Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString()).Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString()).Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString()).Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString()).Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString()).Replace("[OrderData1.ShopName]", order.ShopName.ToString()).Replace("[OrderData1.BillingAddress]", order.BillingAddress.ToString()).Replace("[OrderData1.Tin_No]", order.Tin_No.ToString()).Replace("[OrderData1.CustomerName]", order.CustomerName.ToString())
        //            .Replace("[OrderData1.Skcode]", order.Skcode.ToString()).Replace("[OrderData1.Customerphonenum]", order.Customerphonenum.ToString()).Replace("[OrderData1.BillingStateName]", order.BillingStateName.ToString()).Replace("[OrderData1.BillingStateCode]", order.BillingStateCode.ToString()).Replace("[OrderData1.IsPrimeCustomer]", order.IsPrimeCustomer.ToString()).Replace("[OrderData1.ShippingAddress]", order.ShippingAddress.ToString()).Replace("[OrderData1.shippingStateName]", order.shippingStateName.ToString()).Replace("[OrderData1.shippingStateCode]", order.shippingStateCode.ToString()).Replace("[OrderData1.invoice_no]", order.invoice_no.ToString()).Replace("[OrderData1.CreatedDate]", order.CreatedDate.ToString()).Replace("[OrderData1.OrderId]", order.OrderId.ToString()).Replace("[OrderData1.OrderedDate]", order.OrderedDate.ToString()).Replace("[OrderData1.PocCreditNoteDate]", order.PocCreditNoteDate.ToString()).Replace("[OrderData1.SalesPerson]", order.SalesPerson.ToString())
        //            .Replace("[OrderData1.SalesMobile]", order.SalesMobile.ToString()).Replace("[OrderData1.DboyName]", order.DboyName.ToString()).Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", order.DeliveryIssuanceIdOrderDeliveryMaster.ToString()).Replace("[OrderData1.IsIgstInvoice]", order.IsIgstInvoice.ToString()).Replace("[OrderData1.deliveryCharge]", order.deliveryCharge.ToString()).Replace("[OrderData1.paymentThrough]", order.paymentThrough.ToString())
        //            .Replace("[OrderData1.WalletAmount]", order.WalletAmount.ToString()).Replace("[OrderData1.PocCreditNoteNumber]", order.PocCreditNoteNumber.ToString()).Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", order.InvoiceAmountInWord.ToString())
        //            .Replace("[OrderData1.BillDiscountAmount]", order.BillDiscountAmount.ToString()).Replace("[OrderData1.TCSAmount]", order.TCSAmount.ToString()).Replace("[OrderData1.GrossAmount]", order.GrossAmount.ToString()).Replace("[OrderData1.DiscountAmount]", order.DiscountAmount.ToString()).Replace("[OrderData1.Status]", order.Status.ToString())
        //            .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
        //            .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
        //            .Replace("[CustomerCriticalInfo]", result.ToString())
        //        .Replace("[OrderData1.EwayBillNumber]", order.EwayBillNumber.ToString()).Replace("[OrderData1.offertype]", order.offertype.ToString())
        //        .Replace("[OrderData1.IRNQRCodeUrl]", order.IRNQRCodeUrl.ToString()).Replace("[OrderData1.POCIRNQRCodeURL]", order.POCIRNQRCodeURL.ToString()).Replace("[OrderData1.IRNNo]", order.IRNNo.ToString())
        //        .Replace("[OrderData1.POCIRNNo]", order.POCIRNNo.ToString())
        //        .Replace("[InvoiceAmt]", (order.GrossAmount - order.DiscountAmount).ToString())
        //        .Replace("[amount]", (paymentdetail[0].amount > 0 ? paymentdetail[0].amount : paymentdetail[0].amount).ToString())
        //        .Replace("[PaymentFrom]", (paymentdetail[0].PaymentFrom != null ? paymentdetail[0].PaymentFrom : paymentdetail[0].PaymentFrom).ToString())
        //        .Replace("[IsOnline]", (paymentdetail[0].amount > 0 && paymentdetail[0].IsOnline ? "Paid" : "Refund"))
        //        .Replace("##SumDataHSNROWS##", SumDataHSNDetailRows)
        //        .Replace("##OrderDataRows##", OrderDataRows)
        //        //.Replace("##getOrderDataRows##", getOrderDataRows)
                
        //        .Replace("[OrderData1.OrderType]", ordertype)
        //        .Replace("[totalTaxableValue]", totalTaxableValue.ToString())
        //        .Replace("[totalIGST]", totalIGST.ToString())
        //        .Replace("[totalCGST]", totalCGST.ToString())
        //        .Replace("[totalSGST]", totalSGST.ToString())
        //        .Replace("[totalCess]", totalCess.ToString())
        //        .Replace("[TotalIOverall]", TotalIOverall.ToString())
        //         .Replace("[OrderLimit]", (CustomerCount != null ? (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount).ToString() : ""))
        //         .Replace("[CurrentorderCount]", (CustomerCount != null ? (CustomerCount.OrderCount / CustomerCount.MaxOrderLimit).ToString() : ""))
        //        .Replace("[CustomerCount.MaxOrderLimit]", (CustomerCount != null ? CustomerCount.MaxOrderLimit.ToString() : "")).Replace("[CustomerCount.OrderCount]", (CustomerCount != null ? CustomerCount.OrderCount.ToString() : ""));
        //        //string fileName = Guid.NewGuid() + ".pdf";
        //        //string downoadFolderName = ConfigurationManager.AppSettings["AllFileDownloadFolder"].ToString();
        //        //PDFMaker invoicePDFMaker = new PDFMaker(pathToHTMLFile, fileName, content,null,null,null);
        //        //invoicePDFMaker.SavePdf();
        //        //string htmlUrlString = JsonConvert.SerializeObject(expiredHtml);
        //        //var existingOrderId = db.OrderInvoiceDB.Where(x => x.orderId == id).FirstOrDefault();
        //        //if (existingOrderId == null)
        //        //{
        //        //    invoice.CreatedDate = DateTime.Now;
        //        //    invoice.CreatedBy = userid;
        //        //    invoice.orderId = id;
        //        //    invoice.invoiceURL = htmlUrlString;
        //        //    invoice.IsActive = true;
        //        //    db.Entry(invoice).State = EntityState.Added;
        //        //    var a = db.Commit(); 
        //        //}
        //        if (!string.IsNullOrEmpty(expiredHtml))
        //        {
        //            string fileUrl = "";
        //            string fullPhysicalPath = "";
        //            string thFileName = "";
        //            string TartgetfolderPath = "";

        //            TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\CustomerBackendOrderInvoices");
        //            if (!Directory.Exists(TartgetfolderPath))
        //                Directory.CreateDirectory(TartgetfolderPath);


        //            thFileName = "CustomerBackendInvoice_" + id ;
        //            fileUrl = "/PDFForeCast" + "/" + thFileName;
        //            fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

        //            var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/CustomerBackendOrderInvoices"), thFileName);

        //            byte[] pdf = null;

        //            pdf = Pdf
        //                  .From(expiredHtml)
        //                  //.WithGlobalSetting("orientation", "Landscape")
        //                  //.WithObjectSetting("web.defaultEncoding", "utf-8")
        //                  .OfSize(OpenHtmlToPdf.PaperSize.A4)
        //                  .WithTitle("Invoice")
        //                  .WithoutOutline()
        //                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
        //                  .Portrait()
        //                  .Comressed()
        //                  .Content();
        //            FileStream file = File.Create(OutPutFile);
        //            file.Write(pdf, 0, pdf.Length);
        //            file.Close();
        //            if (!string.IsNullOrEmpty(data.Customerphonenum))
        //            {
        //                string Message = "";
        //                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "StoreOrderDelivered");
        //                Message = dltSMS == null ? "" : dltSMS.Template;
        //                Message = Message.Replace("{#var1#}", data.CustomerName);
        //                Message = Message.Replace("{#var2#}", data.OrderId.ToString());
        //                //string shortUrl = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "/" + Ordermaster.OrderId);//+ guidData
        //                //string URL = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "/CustomerBackendOrderInvoices/CustomerBackendInvoice_" + data.OrderId);
        //                string FileUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
        //                                            , HttpContext.Current.Request.Url.DnsSafeHost
        //                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                            , "/CustomerBackendOrderInvoices/CustomerBackendInvoice_" + data.OrderId);

        //                //string URL = ConfigurationManager.AppSettings["FileUploadApiHost"] + "/CustomerBackendOrderInvoices/CustomerBackendInvoice_" + data.OrderId ;
        //                //string URL = "http://localhost:26265/" + "/CustomerBackendOrderInvoices/CustomerBackendInvoice_" + data.OrderId ;
        //                Message = Message.Replace("{#var3#}", FileUrl);
        //                if (dltSMS != null)
        //                   Common.Helpers.SendSMSHelper.SendSMS(data.Customerphonenum, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                        

        //            }
        //        }
        //    }
        //    return expiredHtml;
        //}




        public class SellerStoreMrpItemDc
        {
            public int ItemMultiMRPId { get; set; }
            public string ItemNumber { get; set; }
            public string itemBaseName { get; set; }
            public double MRP { get; set; }
            public string UOM { get; set; }
            public string UnitofQuantity { get; set; }
            public string LogoUrl { get; set; }
        }
        public class SellerCluster
        {
            public int ClusterId { get; set; }
            public string ClusterName { get; set; }
            public int? WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public int? CityId { get; set; }
            public string CityName { get; set; }
            //public List<SellerCluserLtLng> SellerCluserLtLng { get; set; }
        }

        public class IsClusterWithSeller
        {
            public int ClusterId { get; set; }
            public bool IsSellerAvailable { get; set; }
        }


        public class SellerCluserLtLng
        {
            public int ClusterId { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string polygon { get; set; }
        }

        public class SkCustomer
        {
            public string Skcode { get; set; }
            public int ClusterId { get; set; }
        }

        public class SellerCustomerDc
        {
            public long Id { get; set; }
            public int? ClusterId { get; set; }
            public string ClusterName { get; set; }
            public string Skcode { get; set; }
            public string ShippingAddress { get; set; }
            public double lat { get; set; }
            public double lg { get; set; }
            public string ZipCode { get; set; }
            public string City { get; set; }
            public string Mobile { get; set; }
            public string State { get; set; }
        }
        public class SellerOrderResponse
        {
            public SellerShoppingCart SellerShoppingCart { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
            public int GeneratedOrderId { get; set; }
            public bool IsSuccess { get; set; }
        }
        public class SellerShoppingCart
        {
            public string Skcode { get; set; }
            public string CustomerType { get; set; }  //Sellertype
            public double TotalAmount { get; set; }
            public double DeliveryCharge { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
            public List<SellerItemDetail> itemDetails { get; set; }
        }
        public class SellerItemDetail
        {
            public int ItemMultiMrpId { get; set; }
            public int qty { get; set; }
            public double? SellingPrice { get; set; }
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }


        public class UpdateSellerStockOfCFRProduct
        {
            public string Skcode { get; set; }
            public int OrderId { get; set; }
            public List<SellerItemDetailDc> ItemDetailDc { get; set; }

        }

        public class SellerItemDetailDc
        {
            public int ItemMultiMrpId { get; set; }
            public int qty { get; set; }
            public double SellingPrice { get; set; }
        }

        public class UpdateSellerStockSp
        {
            public string Skcode { get; set; }
            public int OrderId { get; set; }
            public int ItemMultiMrpId { get; set; }
            public int qty { get; set; }
            public double UnitPrice { get; set; }
        }

        public class UpdateSellerStoreMrpIdDc
        {
            public int Cityid { get; set; }
            public int ItemMultiMRPId { get; set; }
            public double SellerStorePrice { get; set; }
            public string msg { get; set; }
        }

        //public class orderDetailDCs
        //{
        //    public double TaxAmmount { get; set; }
        //    public double CessTaxAmount { get; set; }
        //    public double CGSTTaxAmmount { get; set; }
        //    public double SGSTTaxAmmount { get; set; }
        //    public double AmtWithoutTaxDisc { get; set; }
        //    public double TotalAmt { get; set; }
        //}

    }
}
