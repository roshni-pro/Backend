using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using AngularJSAuthentication.API.Controllers.PurchaseOrder;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ForCast;
using AngularJSAuthentication.DataContracts.ROC;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Forecasting;
using AngularJSAuthentication.Model.PurchaseOrder;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OpenHtmlToPdf;
using SqlBulkTools;




namespace AngularJSAuthentication.API.Controllers.InventoryForcast
{
    [RoutePrefix("api/BuyerForecast")]
    public class BuyerForcastController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [HttpGet]
        [Route("BuyerData")]
        //int Createdby, DateTime? CreatedDate

        public List<BuyerForecastdataDC> BuyerForecastview(string GroupName, string uploadby, DateTime? uploadDate, int skip, int take)
        {
            using (var context = new AuthContext())
            {

                //if (uploadDate.HasValue)
                //    uploadDate = null;
                string upload = uploadby;
                string gnme = GroupName;
                if (gnme != null)
                {
                    gnme = GroupName;
                }
                else
                {
                    gnme = "";
                }
                if (upload != null)
                {
                    upload = uploadby;
                }
                else
                {
                    upload = "";
                }
                List<BuyerForecastdataDC> BuyerData = new List<BuyerForecastdataDC>();
                int Skiplist = (skip - 1) * take;
                var gname = new SqlParameter("@GroupName", gnme);
                var Keytype = new SqlParameter("@uploadby", upload);
                var startDate = new SqlParameter("@uploadDate", uploadDate ?? (object)DBNull.Value);
                var Skip = new SqlParameter("@Skip", Skiplist);
                var Take = new SqlParameter("@Take", take);
                BuyerData = context.Database.SqlQuery<BuyerForecastdataDC>("EXEC ForCastingdata @GroupName,@uploadby,@uploadDate,@Skip,@Take", gname, Keytype, startDate, Skip, Take).ToList();
                if (BuyerData != null && BuyerData.Any())
                {
                    return BuyerData;
                }
                return BuyerData;

                //  string query = "Exec GetGullakCashBack " + customerId + "," + amount;

            }
        }


        [Route("GetBuyerViewList")]
        [HttpGet]
        public List<BuyerForeCastDC> GetVantransationList(int UplodeId, int skip, int take)
        {
            using (var myContext = new AuthContext())
            {
                var ParamId = new SqlParameter("@UploadId", UplodeId);
                // var gname = new SqlParameter("@GroupName", GroupName);
                //List<BuyerForecastDC> BuyersList = new List<BuyerForecastDC>();
                int Skiplist = (skip - 1) * take;
                var Skip = new SqlParameter("@Skip", Skiplist);
                var Take = new SqlParameter("@Take", take);
                string query = "EXEC GetBuyerForecast " + UplodeId + "," + Skiplist + "," + take;
                // var BuyersList = myContext.Database.SqlQuery<BuyerForeCastDC>("EXEC GetBuyerForecast @UploadId,@Skip,@Take", ParamId, skip, take).ToList();
                var BuyersList = myContext.Database.SqlQuery<BuyerForeCastDC>(query).ToList();
                if (BuyersList != null)
                {
                    return BuyersList;
                }
                return BuyersList;
            }
        }

        [Route("GetBuyerViewListExport")]
        [HttpGet]
        public List<BuyerForeCastDCExport> GetVantransationListExport(int UplodeId)
        {
            using (var myContext = new AuthContext())
            {
                var ParamId = new SqlParameter("@UploadId", UplodeId);

                string query = "EXEC GetBuyerForecastBrandDetailExport " + UplodeId;
                //var BuyersList = myContext.Database.SqlQuery<BuyerForeCastDC>("EXEC GetBuyerForecastBrandDetailExport @UploadId", ParamId).ToList();
                var BuyersList = myContext.Database.SqlQuery<BuyerForeCastDCExport>(query).ToList();
                if (BuyersList != null)
                {
                    return BuyersList;
                }
                return BuyersList;
            }
        }
        [Route("GetBuyerSummary")]
        [HttpGet]
        public SingleMapview GetBuyerSummaryList(int UploadId)
        {
            using (var myContext = new AuthContext())
            {
                SingleMapview list = new SingleMapview();
                SalesAppManager salesAppManager = new SalesAppManager();
                list = salesAppManager.GetBuyerSummaryList(UploadId);
                return list;
            }
        }

        [Route("GetSalesIntentApproval")]
        [HttpGet]
        public SalesIntentApprovalMainListDC GetSalesIntentApproval(string productname, DateTime? month, int skip, int take)
        {
            using (var myContext = new AuthContext())
            {
                int Skiplist = (skip - 1) * take;
                SalesIntentApprovalMainListDC salesIntentApprovalMainListDC = new SalesIntentApprovalMainListDC();
                ForcastManager forcastManager = new ForcastManager();
                salesIntentApprovalMainListDC = forcastManager.GetSalesIntent(productname, month, Skiplist, take);
                //forcastManager.GetSalesIntent(productname, month, Skiplist, take);
                return salesIntentApprovalMainListDC;
            }
        }


        [Route("UpdateSalesIntentReq")]
        [HttpGet]
          //public bool UpdateSalesIntentRequest(int Id, int ApproverBy, int btnApprovedReject)
        public Response UpdateSalesIntentRequest(int Id, int ApproverBy, int btnApprovedReject)
        {
            bool status = false;
            string msgstr = "";
            Response response = new Response();

            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var salesIntentRequests = db.SalesIntentRequestDb.Where(x => (x.Status == 0 || x.Status == 1) && x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                
                

                if (salesIntentRequests != null)
                {
                    if (ApproverBy == 0) //"sales"
                    {
                       
                        if (btnApprovedReject == 0)
                        {
                            //salesIntentRequests.Status = Convert.ToInt32(SalesIntentReqStatus.PendingForBuyer);

                            var SalesIntentRequestData = db.SalesIntentRequestDb.Where(I => I.Id == Id).FirstOrDefault();
                            var warehouse = new SqlParameter("@warehouseIds", SalesIntentRequestData.Warehouseid);
                            var multimrpid = new SqlParameter("@ItemMultiMRPId", SalesIntentRequestData.ItemMultiMRPId);
                            var data = db.Database.SqlQuery<SalesIntentValidationDc>("Exec Sp_SalesIntentValidationCheck @warehouseIds,@ItemMultiMRPId", warehouse, multimrpid).FirstOrDefault();

                            if (salesIntentRequests.RequestQty < data.CalculatedQuantity)
                            {
                                salesIntentRequests.Status = Convert.ToInt32(SalesIntentReqStatus.PendingForBuyer);
                                salesIntentRequests.SalesLeadApproveID = userid;
                                salesIntentRequests.SalesApprovedDate = DateTime.Now;
                                msgstr = "sales Lead Approved Successfully";
                                status = true;
                            }
                            else
                            {
                                msgstr = "Your allowed qty for this item is " + data.CalculatedQuantity + " we have current net stock " + data.CurrentNetStock + "  and Intransite qty " + data.OpenPOQTy + " HenceRejected";
                                status = false;
                            }
                        }
                        else if (btnApprovedReject == 1)
                        {
                            salesIntentRequests.Status = Convert.ToInt32(SalesIntentReqStatus.Rejected);
                            salesIntentRequests.SalesLeadApproveID = userid;
                            salesIntentRequests.SalesApprovedDate = DateTime.Now;
                            msgstr = "sales Lead Rejected";
                            status = true;
                        }
                    }
                    if (ApproverBy == 1) //"buyers"
                    {
                        salesIntentRequests.BuyerApproveID = userid;
                        salesIntentRequests.BuyerApprovedDate = DateTime.Now;
                        if (btnApprovedReject == 1)
                        {
                            salesIntentRequests.Status = Convert.ToInt32(SalesIntentReqStatus.Rejected);
                            msgstr = "Buyer Lead Rejected";
                            status = true;
                        }
                        else if (btnApprovedReject == 0)
                        {
                            //var ItemForecastDtl = db.ItemForecastDetailDb.Where(I => I.ItemMultiMRPId == salesIntentRequests.ItemMultiMRPId && I.WarehouseId == salesIntentRequests.Warehouseid && I.IsActive == true && I.IsDeleted == false).FirstOrDefault();
                            var ItemForecastDtl = db.ItemForecastDetailDb.Where(I => I.ItemMultiMRPId == salesIntentRequests.ItemMultiMRPId && I.WarehouseId == salesIntentRequests.Warehouseid && I.IsActive == true && I.IsDeleted == false && I.CreatedDate.Month == DateTime.Now.Month && I.CreatedDate.Year == DateTime.Now.Year).FirstOrDefault();
                            if (ItemForecastDtl != null)
                            {
                                ItemForecastDtl.SalesIntent = ItemForecastDtl.SalesIntent + salesIntentRequests.RequestQty;
                                ItemForecastDtl.ModifiedDate = DateTime.Now;
                                ItemForecastDtl.ModifiedBy = userid;
                                db.Entry(ItemForecastDtl).State = EntityState.Modified;
                            }
                            salesIntentRequests.Status = Convert.ToInt32(SalesIntentReqStatus.Approved);
                            salesIntentRequests.ItemForecastDetailId = ItemForecastDtl.Id;

                            msgstr = "Buyer Lead Approved Successfully";
                            status = true;
                        }
                    }
                    if (status == true)
                    {
                        salesIntentRequests.ModifiedDate = DateTime.Now;
                        salesIntentRequests.ModifiedBy = userid;
                        db.Entry(salesIntentRequests).State = EntityState.Modified;

                        if (db.Commit() > 0)
                        {
                            //status = true;
                        }
                    }
                }
                else
                {
                    msgstr = "Record Not Found";
                    status = false;
                }

                //return status;
                response.Data = msgstr;
                response.status = status;
                return response;
            }
        }


        [HttpPost]
        [Route("GetItemForecastDetail")]
        public async Task<ItemforcastEdit> GetItemForecastDetail(ItemForeCastRequest itemForeCastRequest)
        {
            ItemforcastEdit itemforcastEdit = new ItemforcastEdit();
            List<ItemForeCastResponse> ItemForeCastResponses = new List<ItemForeCastResponse>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("categoriesIds", IdDt);

                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subCategoriesIds", IdDt);

                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param4 = new SqlParameter("subSubCategoriesIds", IdDt);

                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForecastDetail]";
                //GetItemForecastDetailEnhancement
                //cmd.CommandText = "[dbo].[TestGetItemForecastDetail]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(new SqlParameter("@itemname", itemForeCastRequest.itemname));
                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                cmd.CommandTimeout = 1200;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ItemForeCastResponses = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastResponse>(reader).ToList();
                reader.NextResult();
                itemforcastEdit.ItemForeCastResponses = ItemForeCastResponses;
                if (reader.Read())
                {
                    itemforcastEdit.TotalRecord = Convert.ToInt32(reader["TotalRecord"]);
                    itemforcastEdit.AvgInventoryDays = Convert.ToInt32(reader["avgInventoryDays"]);
                    itemforcastEdit.BrandSalesValue = Convert.ToInt32(reader["BrandSalesValue"]);//changes
                    itemforcastEdit.BrandTotalValue = Convert.ToDouble(reader["BrandTotalValue"]);
                    itemforcastEdit.BrandTotalPercent = Convert.ToInt32(reader["BrandTotalPercent"]);
                    itemforcastEdit.TodayDay = Convert.ToInt32(reader["TodayDay"]);
                }
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["TotalLiveInventory"] is DBNull))
                    {
                        itemforcastEdit.TotalLiveInventory = 0;
                    }

                    else
                    {
                        itemforcastEdit.TotalLiveInventory = Convert.ToInt32(reader["TotalLiveInventory"]);

                    }

                }



                if (ItemForeCastResponses != null && ItemForeCastResponses.Any())
                {

                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = ItemForeCastResponses.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMrpId }).ToList();
                    var list = await tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null && list.Any())
                        foreach (var da in itemforcastEdit.ItemForeCastResponses)
                        {
                            da.Tag = list.Where(x => x.ItemMultiMRPId == da.ItemMultiMrpId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                }
                return itemforcastEdit;
            }
        }

        [Route("GetItemForecastHistory")]
        [HttpGet]
        public async Task<List<ItemForeCastHisResponse>> GetItemForecastHistory(int itemMultiMrpId, int warehouseId)
        {
            List<ItemForeCastHisResponse> itemForeCastHisResponses = new List<ItemForeCastHisResponse>();
            using (var authContext = new AuthContext())
            {
                DateTime today = DateTime.Now.AddMonths(-1);
                DateTime PreYearDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddYears(-1);
                DateTime EndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddMilliseconds(-1);
                DateTime StartDate = new DateTime(today.Year, today.Month, 1).AddMonths(-2);
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForecastHistory]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@itemmultimrpId", itemMultiMrpId));
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@startdate", StartDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", EndDate));
                cmd.Parameters.Add(new SqlParameter("@preYearDate", PreYearDate));
                var reader = cmd.ExecuteReader();
                itemForeCastHisResponses = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastHisResponse>(reader).ToList();
                return itemForeCastHisResponses;
            }
        }

        [Route("UpdateItemForecast")]
        [HttpGet]
        public async Task<ItemForeCastUpdateResponse> UpdateItemForecast(int Id, int PeopleId, double PercentValue, int InventoryDays, int? CalInventoryDays, double ASP, double TotalBrandTargetValue, double TotalBrandSalesValue, int NewBuyerEditForecastQty, double GrowthInAmount)
        {

            ItemForeCastUpdateResponse itemForeCastUpdateResponse = new ItemForeCastUpdateResponse();

            using (var authContext = new AuthContext())
            {
                var itemForecastDetail = authContext.ItemForecastDetailDb.FirstOrDefault(x => x.Id == Id);
                List<datas> res = new List<datas>();
                List<uploadWName> wName = new List<uploadWName>();
                string query = "Select *, sum(BuyerEdit) over(order by RowId) TotalBuyerEdit From ( ";
                query = query + "Select 101 as RowId, " + itemForecastDetail.Id + " as ID," + itemForecastDetail.ItemMultiMRPId + " as ItemMultiMRPId," + itemForecastDetail.BuyerEdit + " as BuyerEdit UNION ";
                query = query + "Select I.Id as RowId, I.ID,I.ItemMultiMRPId,I.BuyerEdit From FutureMrpMappings fmrp with(nolock) ";
                query = query + "Inner Join ItemforecastDetails I with(nolock)  on fmrp.MappingMRPId = I.ItemMultiMRPId  and I.isActive = 1 and I.IsDeleted = 0 and Month(I.CreatedDate)= Month(getdate()) and Year(I.CreatedDate)= Year(getdate()) ";
                query = query + "Where fmrp.WarehouseId = " + itemForecastDetail.WarehouseId + " and fmrp.IsActive = 1 And fmrp.ItemMultiMRPId = " + itemForecastDetail.ItemMultiMRPId + "";
                query = query + " ) Z Order By RowId Desc ";
                res = authContext.Database.SqlQuery<datas>(query).ToList();
                //Select * from BuyerForecastUploders where isactive =1  order by id desc
                long BuyerForecastUploder = authContext.BuyerForecastUploderDb.Where(x => x.IsActive == true && x.Status == 2).Select(x => x.Id).FirstOrDefault();

                //select d.WarehouseName,W.WarehouseId from BuyerForecastUploderDetails d  inner join  Warehouses w on d.WarehouseName=w.WarehouseName  where  d.IsActive=1 and w.active=1 and d.UplodeId=18
                //  string getres = "select distinct d.WarehouseName,W.WarehouseId from BuyerForecastUploderDetails d  inner join  Warehouses w on d.WarehouseName=w.WarehouseName  where  d.IsActive=1 and w.active=1 and d.UplodeId=" + BuyerForecastUploder + " and  W.WarehouseId = " + itemForecastDetail.WarehouseId;
                // wName = authContext.Database.SqlQuery<uploadWName>(getres).ToList();

                if (itemForecastDetail != null)
                {

                    double oldBrandAmount = GrowthInAmount;
                    double CalculateNew_BrandAmount = ASP * NewBuyerEditForecastQty;
                    double CalculateNew_TotalBrandSalesValue = (TotalBrandSalesValue - oldBrandAmount) + CalculateNew_BrandAmount;

                    double BrandPredictivePercent = CalculateNew_TotalBrandSalesValue > 0 || TotalBrandTargetValue > 0 ? ((CalculateNew_TotalBrandSalesValue / TotalBrandTargetValue) * 100) : 0;

                    //double BrandPredictivePercent = (CalculateNew_TotalBrandSalesValue / TotalBrandTargetValue) * 100; 

                    //if (BrandPredictivePercent > 100 && wName.Any() && wName != null)
                    //{
                    //    itemForeCastUpdateResponse.Status = false;
                    //    itemForeCastUpdateResponse.msg = "Brand Predictive Percent (" + BrandPredictivePercent.ToString()  + "%) is Above 100%";                      
                    //}
                    if (BrandPredictivePercent >= 0)
                    {
                        int TotalBuyerEdit = res.Select(x => x.BuyerEdit).Sum();
                        int RemainQty = 0;
                        int FinalRemainQty = 0;
                        int increLoop = 0;
                        bool RecordUpdate = false;
                        bool doexist = false;
                        if (NewBuyerEditForecastQty != TotalBuyerEdit)
                        {
                            foreach (var iLoop in res)
                            {

                                string SQL = "";
                                if (iLoop.Id == Id)
                                {
                                    SQL = "Update ItemforecastDetails SET BuyerEdit = " + NewBuyerEditForecastQty + " , ModifiedBy = " + PeopleId + ", ModifiedDate = getdate()  Where ID = " + iLoop.Id;
                                }
                                else
                                {
                                    SQL = "Update ItemforecastDetails SET BuyerEdit = 0 , ModifiedBy = " + PeopleId + ", ModifiedDate = getdate()  Where ID = " + iLoop.Id;
                                }

                                if (SQL != "")
                                {

                                    authContext.Database.ExecuteSqlCommand(SQL);

                                    var buyereditData = authContext.ItemForecastDetailDb.Where(x => x.Id == iLoop.Id).Select(x => x.BuyerEdit).FirstOrDefault();
                                    itemForecastDetail.BuyerEdit = buyereditData;
                                    itemForecastDetail.GrowthInAmount = CalculateNew_BrandAmount;
                                    itemForecastDetail.PercentValue = PercentValue;
                                    itemForecastDetail.ModifiedBy = PeopleId;
                                    itemForecastDetail.ModifiedDate = DateTime.Now;
                                    itemForecastDetail.InventoryDays = InventoryDays; // NEW ADD
                                    authContext.Entry(itemForecastDetail).State = EntityState.Modified;
                                    RecordUpdate = true;
                                }
                            }
                        }
                        else
                        {
                            itemForecastDetail.GrowthInAmount = CalculateNew_BrandAmount;
                            itemForecastDetail.PercentValue = PercentValue;
                            itemForecastDetail.InventoryDays = InventoryDays;

                            itemForecastDetail.ModifiedBy = PeopleId;
                            itemForecastDetail.ModifiedDate = DateTime.Now;
                            authContext.Entry(itemForecastDetail).State = EntityState.Modified;
                            //authContext.ItemForecastDetailDb.Add(itemForecastDetail);
                            RecordUpdate = true;

                        }


                        if (InventoryDays != itemForecastDetail.InventoryDays)
                        {
                            itemForecastDetail.CalculateInventoryDays = CalInventoryDays.HasValue ? CalInventoryDays.Value : 0;


                            itemForecastDetail.GrowthInAmount = CalculateNew_BrandAmount;
                            itemForecastDetail.PercentValue = PercentValue;
                            itemForecastDetail.InventoryDays = InventoryDays;

                            itemForecastDetail.ModifiedBy = PeopleId;
                            itemForecastDetail.ModifiedDate = DateTime.Now;

                            authContext.Entry(itemForecastDetail).State = EntityState.Modified;

                            RecordUpdate = true;
                        }


                        if (authContext.Commit() > 0)
                        {
                            if (RecordUpdate == true)
                            {

                                RecordUpdate = false;
                                itemForeCastUpdateResponse.Status = true;
                                itemForeCastUpdateResponse.msg = "Item forecast data updated successfully.";
                            }
                        }
                        else
                        {
                            itemForeCastUpdateResponse.Status = false;
                            itemForeCastUpdateResponse.msg = "Item forecast data not updated.";
                        }
                    }
                }
                else
                {
                    itemForeCastUpdateResponse.Status = false;
                    itemForeCastUpdateResponse.msg = "Update Request Item Forecast Record not Found.";
                }

            }
            return itemForeCastUpdateResponse;
        }

        [Route("GetPdcaSummary")]
        [HttpGet]
        public DataTable GetPdcaSummary(int UplodeId, int BaseCatID, int skip, int take, int Categoryid)
        {
            using (var myContext = new AuthContext())
            {


                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                DataTable data = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    //var Uplodeid = new SqlParameter("@UplodeId", UplodeId);
                    //var BasecatiD = new SqlParameter("@BaseCatID", BaseCatID);
                    var Catid = new SqlParameter("@CategoryID", Categoryid);
                    int Skiplist = (skip - 1) * take;
                    //var Skip = new SqlParameter("@Skip", Skiplist);
                    //var Take = new SqlParameter("@Take", take);
                    string query = "EXEC sp_GetPdcaSummary " + UplodeId + "," + BaseCatID + "," + Categoryid + "," + Skiplist + "," + take;

                    using (var command = new SqlCommand(query, connection))
                    {

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(data);
                        da.Dispose();
                        connection.Close();

                        return data;
                    }
                }

            }
        }

        [Route("BuyerSummary")]
        [HttpGet]
        public DataTable GetBuyerSummary(int UplodeId, int PeopleID, int skip, int take, int SubCategoryId)
        {
            using (var myContext = new AuthContext())
            {
                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                DataTable data = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    var subcatid = new SqlParameter("@SubCategoryId", SubCategoryId);
                    var Uplodeid = new SqlParameter("@UplodeId", UplodeId);
                    var peopleid = new SqlParameter("@PeopleID", PeopleID);

                    int Skiplist = (skip - 1) * take;
                    var Skip = new SqlParameter("@Skip", Skiplist);
                    var Take = new SqlParameter("@Take", take);
                    string query = "EXEC sp_GetBuyersSummary " + UplodeId + "," + PeopleID + "," + SubCategoryId + "," + Skiplist + "," + take;
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(data);
                        da.Dispose();
                        connection.Close();
                        return data;
                    }
                }

            }
        }
        [HttpPost]
        [Route("GetItemForeCastForPO")]
        public async Task<ItemforcastPOEdit> GetItemForeCastForPO(ItemForeCastRequest itemForeCastRequest)
        {
            ItemforcastPOEdit itemforcastPOEdit = new ItemforcastPOEdit();
            List<ItemForeCastPOResponse> itemForeCastPOResponses = new List<ItemForeCastPOResponse>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("categoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param4 = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForeCastForPO]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ItemForeCastPOData> itemForeCastPODataes = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastPOData>(reader).ToList();
                itemForeCastPOResponses = itemForeCastPODataes.GroupBy(x => x.Id).Select(x => new ItemForeCastPOResponse
                {
                    Id = x.Key,
                    AveragePurchasePrice = x.FirstOrDefault().AveragePurchasePrice,
                    BuyingPrice = x.FirstOrDefault().BuyingPrice,
                    CityId = x.FirstOrDefault().CityId,
                    CityName = x.FirstOrDefault().CityName,
                    CurrentStock = x.FirstOrDefault().CurrentStock,
                    FulfillThrow = x.FirstOrDefault().FulfillThrow,
                    ItemMultiMrpId = x.FirstOrDefault().ItemMultiMrpId,
                    ItemName = x.FirstOrDefault().ItemName,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    MRP = x.FirstOrDefault().MRP,
                    NetStock = x.FirstOrDefault().NetStock,
                    NoOfSet = x.FirstOrDefault().NoOfSet,
                    PRPaymentType = x.FirstOrDefault().PRPaymentType,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    RequiredQty = x.FirstOrDefault().RequiredQty,
                    SubsubcategoryName = x.FirstOrDefault().SubsubcategoryName,
                    SupplierId = x.FirstOrDefault().SupplierId,
                    WarehouseName = x.FirstOrDefault().WarehouseName,
                    YesterdayDemand = x.FirstOrDefault().YesterdayDemand,
                    SalesIntent = x.FirstOrDefault().SalesIntent,
                    OtherWarehouseDetails = x.Select(y => new OtherWarehouseDetails
                    {
                        OtherWarehouseId = y.OtherWarehouseId,
                        OtherWhDelCancel = y.OtherWhDelCancel,
                        OtherWhDemand = y.OtherWhDemand,
                        OtherWhNetDemand = y.OtherWhNetDemand,
                        OtherWhOpenPoQty = y.OtherWhOpenPoQty,
                        OtherWhStock = y.OtherWhStock,
                        OtherWarehouseName = y.OtherWarehouseNM
                    }).ToList()
                }).ToList();
                itemforcastPOEdit.ItemForeCastPOResponses = itemForeCastPOResponses;
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["TotalCount"] is DBNull))
                    {
                        itemforcastPOEdit.TotalRecord = 0;
                    }
                    else
                    {
                        itemforcastPOEdit.TotalRecord = Convert.ToInt32(reader["TotalCount"]);
                    }
                }
                return itemforcastPOEdit;
            }
        }
        //[Route("UpdateItemFulFillRequest")]
        //[HttpPost]
        //public async Task<ItemForeCastUpdateResponse> UpdateItemFulFillRequest(ItemForecastPRRequestDc item)
        //{
        //    ItemForeCastUpdateResponse response = new ItemForeCastUpdateResponse();
        //    bool result = false;
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    using (var context = new AuthContext())
        //    {
        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();


        //        var itemForecastDetail = await context.ItemForecastDetailDb.FirstOrDefaultAsync(x => x.Id == item.ItemForecastDetailId);
        //        if (itemForecastDetail != null)
        //        {

        //            double requireQty = 0;
        //            double otherhubReqQty = 0;
        //            int NoOfSet = 0;
        //            int PurchaseMoq = 0;
        //            int FinalQty = 0;

        //            NoOfSet = item.NoOfSet;
        //            PurchaseMoq = item.MinOrderQty;
        //            FinalQty = item.NoOfSet * item.MinOrderQty;

        //            if (NoOfSet == 0)
        //            {
        //                response.msg = "No of Set should not be zero!";
        //                return response;
        //            }

        //            if (PurchaseMoq == 0)
        //            {
        //                response.msg = "Purchase MOQ should not be zero!";
        //                return response;
        //            }

        //            if (FinalQty == 0)
        //            {
        //                response.msg = "Final Qty should not be zero!";
        //                return response;
        //            }

        //            var param = new SqlParameter("@itemforcastId", itemForecastDetail.Id);
        //            var cmd = context.Database.Connection.CreateCommand();

        //            cmd.CommandText = "[dbo].[GetItemForeCastOtherhubRequiredQty]";
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.Parameters.Add(param);
        //            var reader = cmd.ExecuteReader();

        //            if (reader.Read())
        //            {
        //                if ((reader["requireQty"] is DBNull))
        //                {
        //                    requireQty = 0;
        //                }

        //                else
        //                {
        //                    requireQty = Convert.ToDouble(reader["requireQty"]);

        //                }


        //            }
        //            reader.NextResult();
        //            if (reader.Read())
        //            {
        //                if ((reader["otherhubReqQty"] is DBNull))
        //                {
        //                    otherhubReqQty = 0;
        //                }

        //                else
        //                {
        //                    otherhubReqQty = Convert.ToDouble(reader["otherhubReqQty"]);

        //                }


        //            }


        //            var reqcase = Math.Ceiling((requireQty / item.MinOrderQty));
        //            var otherreqcase = Math.Ceiling((otherhubReqQty / item.MinOrderQty));
        //            var sum = reqcase + otherreqcase;
        //            if (sum >= item.NoOfSet)
        //            {
        //                ItemForecastPRRequest ItemForecastPRRequest = new ItemForecastPRRequest
        //                {
        //                    FulfillThrow = item.FulfillThrow,
        //                    BuyingPrice = item.FulfillThrow == 1 ? item.BuyingPrice : null,
        //                    ItemForecastDetailId = item.ItemForecastDetailId,
        //                    CreatedBy = userid,
        //                    CreatedDate = DateTime.Now,
        //                    Demand = item.Demand,
        //                    InternalTransferWHId = item.FulfillThrow == 2 ? item.InternalTransferWHId : null,
        //                    IsActive = true,
        //                    IsDeleted = false,
        //                    MinOrderQty = item.MinOrderQty,
        //                    NoOfSet = item.NoOfSet,
        //                    PRPaymentType = item.FulfillThrow == 1 ? item.PRPaymentType : null,
        //                    SalesIntentQty = item.SalesIntentQty,
        //                    FinalQty = item.NoOfSet * item.MinOrderQty,
        //                    SupplierId = item.FulfillThrow == 1 ? item.SupplierId : null,
        //                    Remainning = Convert.ToInt32(requireQty - (item.NoOfSet * item.MinOrderQty)),
        //                    BuyerId = item.PeopleID, //add for buyerid
        //                    ETADate = item.ETADate,
        //                    PickerType = item.PickerType,
        //                    DepoId = item.DepoId,
        //                    FreightCharge = item.FreightCharge,  //add for freightcharge
        //                    Demandcases = item.Demandcases,
        //                    AllowedQty = item.AllowedQty,
        //                    AllowedQtyOtherHub = item.AllowedQtyOtherHub,
        //                    bussinessType = item.bussinessType
        //                };
        //                context.ItemForecastPRRequestDb.Add(ItemForecastPRRequest);

        //            }
        //            else
        //            {
        //                response.Status = false;
        //                response.msg = item.Itemname + "'s No of cases is lower than Sum of all Cases";
        //                return response;
        //            }
        //        }

        //        context.Database.Connection.Close();
        //        result = (await context.CommitAsync()) > 0;
        //        if (!result)
        //        {
        //            response.Status = false;
        //            response.msg = "Some error occurred during save data please try after some time.";
        //        }
        //        else
        //        {

        //            response.Status = true;
        //            response.msg = "Record save successfully.";
        //        }

        //    }
        //    return response;
        //}
        [Route("UpdateItemFulFillRequest")]
        [HttpPost]
        public async Task<ItemForeCastUpdateResponse> UpdateItemFulFillRequest(ItemForecastPRRequestDc itemRequestDc)
        {
            ItemForeCastUpdateResponse response = new ItemForeCastUpdateResponse();
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var itemForecastDetail = await context.ItemForecastDetailDb.FirstOrDefaultAsync(x => x.Id == itemRequestDc.ItemForecastDetailId);
                List<ItemForecastPRRequest> ItemForecastPRRequest = new List<ItemForecastPRRequest>();
                if (itemForecastDetail != null)
                {

                    double requireQty = 0;

                    int NoOfSet = 0;
                    int PurchaseMoq = 0;
                    int FinalQty = 0;

                    NoOfSet = itemRequestDc.NoOfSet;
                    PurchaseMoq = itemRequestDc.MinOrderQty;
                    FinalQty = itemRequestDc.NoOfSet * itemRequestDc.MinOrderQty;

                    if (NoOfSet == 0)
                    {
                        response.msg = "No of Set should not be zero!";
                        return response;
                    }

                    if (PurchaseMoq == 0)
                    {
                        response.msg = "Purchase MOQ should not be zero!";
                        return response;
                    }

                    if (FinalQty == 0)
                    {
                        response.msg = "Final Qty should not be zero!";
                        return response;
                    }
                    requireQty = context.Database.SqlQuery<double>("Exec GetItemForeCastRequiredQty " + itemForecastDetail.Id).FirstOrDefault();
                    if (requireQty >= itemRequestDc.NoOfSet * itemRequestDc.MinOrderQty)
                    {
                        ItemForecastPRRequest ItemForecastPRRequestsd = new ItemForecastPRRequest
                        {
                            FulfillThrow = itemRequestDc.FulfillThrow,
                            BuyingPrice = itemRequestDc.FulfillThrow == 1 ? itemRequestDc.BuyingPrice : null,
                            ItemForecastDetailId = itemRequestDc.ItemForecastDetailId,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Demand = itemRequestDc.Demand,
                            InternalTransferWHId = (itemRequestDc.FulfillThrow == 2) || (itemRequestDc.FulfillThrow == 3) ? itemRequestDc.InternalTransferWHId : null,
                            IsActive = false,
                            IsDeleted = true,
                            MinOrderQty = itemRequestDc.MinOrderQty,
                            NoOfSet = itemRequestDc.NoOfSet,
                            PRPaymentType = itemRequestDc.FulfillThrow == 1 ? itemRequestDc.PRPaymentType : null,
                            SalesIntentQty = itemRequestDc.SalesIntentQty,
                            FinalQty = itemRequestDc.NoOfSet * itemRequestDc.MinOrderQty,
                            SupplierId = itemRequestDc.FulfillThrow == 1 ? itemRequestDc.SupplierId : null,
                            Remainning = Convert.ToInt32(requireQty - (itemRequestDc.NoOfSet * itemRequestDc.MinOrderQty)),
                            //  BuyerId = userid,
                            BuyerId = itemRequestDc.PeopleID, // for Buyer Name
                            ETADate = itemRequestDc.ETADate,
                            PickerType = itemRequestDc.PickerType,
                            DepoId = itemRequestDc.DepoId,
                            FreightCharge = itemRequestDc.FreightCharge,  //add for freightcharge
                            Demandcases = itemRequestDc.Demandcases,
                            AllowedQty = itemRequestDc.AllowedQty,
                            AllowedQtyOtherHub = itemRequestDc.AllowedQtyOtherHub,
                            bussinessType = itemRequestDc.bussinessType
                        };
                        ItemForecastPRRequest.Add(ItemForecastPRRequestsd);
                        //context.ItemForecastPRRequestDb.Add(ItemForecastPRRequestsd);
                        //result = (await context.CommitAsync()) > 0;

                        context.ItemForecastPRRequestDb.AddRange((IEnumerable<ItemForecastPRRequest>)ItemForecastPRRequest);
                        context.Database.Connection.Close();
                        result = (await context.CommitAsync()) > 0;
                        if (!result)
                            response.msg = "Some error occurred during save data please try after some time.";
                        else
                        {
                            var CreatePoOrInternalRes = new CreatePoOrInternalRes();
                            CreatePoOrInternalDc dsds = new CreatePoOrInternalDc();
                            dsds.Ids = ItemForecastPRRequest.Select(x => x.Id).ToList();
                            CreatePoOrInternalRes = await CreatePoOrInternal(dsds, context);
                            if (CreatePoOrInternalRes.Status == true)
                            {
                                foreach (var item in ItemForecastPRRequest)
                                {
                                    item.IsActive = true;
                                    item.IsDeleted = false;
                                    context.Entry(item).State = EntityState.Modified;
                                    //int fulfillmentId = Convert.ToInt32(i.Id);
                                    //rejectpurchaserequestid(fulfillmentId);
                                }
                                await context.CommitAsync();
                            }
                            response.msg = "Record save successfully for Internal !";
                            //   response.msg = CreatePoOrInternalRes.Message;
                            response.Status = true;
                        }
                    }
                    else
                    {
                        response.msg = "Remaining qty higher then required qty.";
                    }
                }
                response.Status = result;
            }
            return response;
        }

        [Route("GetItemMoQ")]
        [HttpGet]
        public async Task<ItemforecastRequiredData> GetItemMoQ(int itemMultiMrpId, int warehouseId)
        {
            ItemforecastRequiredData itemforecastRequiredData = new ItemforecastRequiredData();
            List<int> MOQLst = new List<int>();
            double POPurchasePrice = 0;
            double PurchasePrice = 0;
            List<supplierMinDc> supplierMinDcs = new List<supplierMinDc>();
            using (var context = new AuthContext())
            {

                string itemnumber = (await context.ItemMultiMRPDB.FirstOrDefaultAsync(x => x.ItemMultiMRPId == itemMultiMrpId))?.ItemNumber;
                // MOQLst = await context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.Number == itemnumber).Select(x => x.PurchaseMinOrderQty).Distinct().ToListAsync();

                var Multimrpid = new SqlParameter("@ItemMultiMRPId", itemMultiMrpId);
                var ware = new SqlParameter("@warehouseId", warehouseId);
                var number = new SqlParameter("@Number", itemnumber);
                MOQLst = context.Database.SqlQuery<int>("EXEC sp_GetITEMMOQ @ItemMultiMRPId,@warehouseId,@Number", Multimrpid, ware, number).ToList();

                supplierMinDcs = (await context.Suppliers.Where(x => x.Active == true && x.Deleted == false).Select(x => new supplierMinDc
                {
                    SupplierId = x.SupplierId,
                    SupplierName = x.Name + "-" + x.SUPPLIERCODES,
                    bussinessType = x.bussinessType,
                    Expirydays = x.ExpiryDays
                }).ToListAsync()).OrderBy(x => x.SupplierName).ToList();
                PurchasePrice = context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.ItemMultiMRPId == itemMultiMrpId).Select(x => x.PurchasePrice).FirstOrDefault();
                var POPrice = context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.ItemMultiMRPId == itemMultiMrpId).Select(x => x.POPurchasePrice).FirstOrDefault();
                POPurchasePrice = (POPrice == null || POPrice == 0) ? PurchasePrice : (double)POPrice;
            }
            itemforecastRequiredData.MOQLst = MOQLst;
            itemforecastRequiredData.POPurchasePrice = POPurchasePrice;
            itemforecastRequiredData.SupplierLst = supplierMinDcs;
            return itemforecastRequiredData;
        }

        //[Route("GetItemMoQForBussinessType")]
        //[HttpGet][AllowAnonymous]
        //public async Task<ItemforecastRequiredDataForBussinessType> GetItemMoQForBussinessType(int itemMultiMrpId, int warehouseId)
        //{
        //    ItemforecastRequiredDataForBussinessType itemforecastRequiredData = new ItemforecastRequiredDataForBussinessType();
        //    List<int> MOQLst = new List<int>();
        //    double POPurchasePrice = 0;
        //    double PurchasePrice = 0;
        //    List<supplierMin> supplierMinDcs = new List<supplierMin>();
        //    using (var context = new AuthContext()) 
        //    {

        //        string itemnumber = (await context.ItemMultiMRPDB.FirstOrDefaultAsync(x => x.ItemMultiMRPId == itemMultiMrpId))?.ItemNumber;
        //        MOQLst = await context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.Number == itemnumber).Select(x => x.PurchaseMinOrderQty).Distinct().ToListAsync();
        //        supplierMinDcs = (await context.Suppliers.Where(x => x.Active == true && x.Deleted == false).Select(x => new supplierMin
        //        {
        //            SupplierId = x.SupplierId,
        //            SupplierName = x.Name + "-" + x.SUPPLIERCODES,
        //            bussinessType= x.bussinessType
        //        }).ToListAsync()).OrderBy(x => x.SupplierName).ToList();
        //        PurchasePrice = context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.ItemMultiMRPId == itemMultiMrpId).Select(x => x.PurchasePrice).FirstOrDefault();
        //        var POPrice = context.itemMasters.Where(x => x.WarehouseId == warehouseId && x.ItemMultiMRPId == itemMultiMrpId).Select(x => x.POPurchasePrice).FirstOrDefault();
        //        POPurchasePrice = (POPrice == null || POPrice == 0) ? PurchasePrice : (double)POPrice;
        //    }
        //    itemforecastRequiredData.MOQLst = MOQLst;
        //    itemforecastRequiredData.POPurchasePrice = POPurchasePrice;
        //    itemforecastRequiredData.SupplierLst = supplierMinDcs;
        //    return itemforecastRequiredData;
        //}

        [Route("GetSupplierForForecast")]
        [HttpGet]
        public async Task<List<supplierMinDc>> GetSupplierForForecast(int warehouseId)
        {
            List<supplierMinDc> supplierMinDcs = new List<supplierMinDc>();
            using (var context = new AuthContext())
            {
                supplierMinDcs = await context.Suppliers.Where(x => x.WarehouseId == warehouseId && x.Active == true && x.Deleted == false).Select(x => new supplierMinDc
                {
                    SupplierId = x.SupplierId,
                    SupplierName = x.Name + "-" + x.SUPPLIERCODES
                }).ToListAsync();
            }
            return supplierMinDcs;
        }

        [Route("OldRequestData")]
        [HttpGet]
        public SalesIntentTotalApproavalOldDC OldRequestData(string productname, DateTime? month, int skip, int take)
        {
            //SalesIntentTotalApproavalOldDC
            using (var context = new AuthContext())
            {
                // SalesIntentTotalApproavalOldDC SalesIntentTotalApproavalOldDCs=new SalesIntentTotalApproavalOldDC();
                int Skiplist = (skip - 1) * take;
                SalesIntentTotalApproavalOldDC SalesIntentApprovalOldDCs = new SalesIntentTotalApproavalOldDC();
                ForcastManager forcastManager = new ForcastManager();
                SalesIntentApprovalOldDCs = forcastManager.GetOldRequest(productname, month, Skiplist, take);
                return SalesIntentApprovalOldDCs;
            }
        }

        [Route("SalesIndentSettledRequest")]
        [HttpPost]
        public SalesIntentTotalApproavalOldDC PendingRequestData(Settled_Request obj)
        {
            using (var authContext = new AuthContext())
            {
                //OldRequestData API WORKING

                int Skiplist = (obj.skip - 1) * obj.take;
                SalesIntentTotalApproavalOldDC SalesIntentApprovalOldDCs = new SalesIntentTotalApproavalOldDC();
                List<SalesIntentApprovalOldDC> SalesIntentApproval = new List<SalesIntentApprovalOldDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@ProductName", obj.productname);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change

                var cmd = authContext.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[ForecastSalesIndentSettled]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout


                var reader = cmd.ExecuteReader();
                SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentApprovalOldDC>(reader).ToList();

                reader.NextResult();

                SalesIntentApprovalOldDCs.SalesIntentApprovalOldDCs = SalesIntentApproval;
                if (reader.Read())
                {
                    SalesIntentApprovalOldDCs.TotRec = Convert.ToInt32(reader["TotRec"]);
                }
                if (SalesIntentApproval != null && SalesIntentApproval.Any())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = SalesIntentApproval.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId.Value }).ToList();
                    var list = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null)
                    {
                        foreach (var da in SalesIntentApproval)
                        {
                            da.Tag = list.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }
                }

                return SalesIntentApprovalOldDCs; ;
            }


        }

        [Route("SalesIndentDashBoard")]
        [HttpPost]
        public SalesIntentDashboardDC DashBoardRequestData(Dashboard_Request obj)
        {
            using (var authContext = new AuthContext())
            {
                //OldRequestData API WORKING

                int Skiplist = (obj.skip - 1) * obj.take;
                SalesIntentDashboardDC SalesIntentApprovalOldDCs = new SalesIntentDashboardDC();
                List<SalesIntentmtdDC> SalesIntentApproval = new List<SalesIntentmtdDC>();
                List<SalesIntentytdDC> SalesIntentytc = new List<SalesIntentytdDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@Warehouseid", obj.WarehouseId);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change

                var cmd = authContext.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[Sp_IndentPerformanceDashboard]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout


                var reader = cmd.ExecuteReader();
                SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentmtdDC>(reader).ToList();

                reader.NextResult();

                SalesIntentApprovalOldDCs.SalesIntentApprovalOldDCs = SalesIntentApproval;

                SalesIntentytc = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentytdDC>(reader).ToList();


                reader.NextResult();
                SalesIntentApprovalOldDCs.SalesIntentytdobjDC = SalesIntentytc;



                return SalesIntentApprovalOldDCs; ;
            }


        }


        [Route("SalesAllIndentDashBoard")]
        [HttpPost]
        public SalesIntentDashboardALLDC DashBoardALLRequestData(Dashboard_Request obj)
        {
            using (var authContext = new AuthContext())
            {
                //OldRequestData API WORKING

                int Skiplist = (obj.skip - 1) * obj.take;
                SalesIntentDashboardALLDC SalesIntentApprovalOldDCs = new SalesIntentDashboardALLDC();
                List<SalesIntentalldDC> SalesIntentApproval = new List<SalesIntentalldDC>();
                //List<SalesIntentytdDC> SalesIntentytc = new List<SalesIntentytdDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@Warehouseid", obj.WarehouseId);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change

                var cmd = authContext.Database.Connection.CreateCommand();

                // cmd.CommandText = "[dbo].[Sp_IndentPerformanceDashboard]";
                cmd.CommandText = "[dbo].[Sp_AllIndentPerformanceDashboard]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout


                var reader = cmd.ExecuteReader();
                SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentalldDC>(reader).ToList();

                //reader.NextResult();

                SalesIntentApprovalOldDCs.SalesIntentApprovalOldDCs = SalesIntentApproval;




                return SalesIntentApprovalOldDCs; ;
            }


        }




        [Route("SalesIndentDashBoardExport")]
        [HttpPost]
        public List<SalesIndentExportDC> DashBoardExportRequestData(Dashboard_Request obj)
        {
            using (var authContext = new AuthContext())
            {
                //OldRequestData API WORKING

                int Skiplist = (obj.skip - 1) * obj.take;
                List<SalesIndentExportDC> SalesIndentExport = new List<SalesIndentExportDC>();
                //List<SalesIntentmtdDC> SalesIntentApproval = new List<SalesIntentmtdDC>();
                //List<SalesIntentytdDC> SalesIntentytc = new List<SalesIntentytdDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@Warehouseid", obj.WarehouseId);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change

                var cmd = authContext.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[Sp_IndentPerformanceExport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout


                var reader = cmd.ExecuteReader();
                SalesIndentExport = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIndentExportDC>(reader).ToList();
                return SalesIndentExport;
            }


        }
        [Route("SalesIndentDashBoardYTDExport")]
        [HttpPost]
        public List<SalesIndentExportYTDDC> DashBoardExportYTDRequestData(Dashboard_Request obj)
        {
            using (var authContext = new AuthContext())
            {
                //OldRequestData API WORKING

                int Skiplist = (obj.skip - 1) * obj.take;
                List<SalesIndentExportYTDDC> SalesIndentExport = new List<SalesIndentExportYTDDC>();
                //List<SalesIntentmtdDC> SalesIntentApproval = new List<SalesIntentmtdDC>();
                //List<SalesIntentytdDC> SalesIntentytc = new List<SalesIntentytdDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@Warehouseid", obj.WarehouseId);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change

                var cmd = authContext.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[Sp_IndentPerformanceYTDExport]";  // Sp_IndentPerformanceExport
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout


                var reader = cmd.ExecuteReader();
                SalesIndentExport = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIndentExportYTDDC>(reader).ToList();
                return SalesIndentExport;
            }


        }


        //[Route("SalesIndentSettledRequestExport")]
        //[HttpPost]
        //public SalesIntentTotalApproavalOldDCExport PendingRequestDataExport(Settled_Request obj)
        //{
        //    using (var authContext = new AuthContext())
        //    {
        //        //OldRequestData API WORKING

        //        //int Skiplist = (obj.skip - 1) * obj.take;
        //        SalesIntentTotalApproavalOldDCExport SalesIntentApprovalOldDCs = new SalesIntentTotalApproavalOldDCExport();
        //        List<SalesIntentApprovalOldDCExport> SalesIntentApproval = new List<SalesIntentApprovalOldDCExport>();
        //        if (authContext.Database.Connection.State != ConnectionState.Open)
        //            authContext.Database.Connection.Open();

        //        var IdDt = new DataTable();
        //        IdDt.Columns.Add("IntValue");
        //        foreach (var item in obj.subSubCategoriesIds)
        //        {
        //            var dr = IdDt.NewRow();
        //            dr["IntValue"] = item;
        //            IdDt.Rows.Add(dr);
        //        }
        //        var param = new SqlParameter("subSubCategoriesIds", IdDt);
        //        param.SqlDbType = SqlDbType.Structured;
        //        param.TypeName = "dbo.IntValues";
        //        var param1 = new SqlParameter("@ProductName", obj.productname);
        //        var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
        //      //  var param3 = new SqlParameter("@Skip", Skiplist);
        //       // var param4 = new SqlParameter("@Take", obj.take);// new change

        //        var cmd = authContext.Database.Connection.CreateCommand();

        //        cmd.CommandText = "[dbo].[ForecastSalesIndentSettledExport]";
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        cmd.Parameters.Add(param);
        //        cmd.Parameters.Add(param1);
        //        cmd.Parameters.Add(param2);
        //      //  cmd.Parameters.Add(param3);
        //       // cmd.Parameters.Add(param4);
        //        cmd.CommandTimeout = 1200; // for Execute Timedout


        //        var reader = cmd.ExecuteReader();
        //        SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentApprovalOldDCExport>(reader).ToList();

        //        reader.NextResult();

        //        SalesIntentApprovalOldDCs.SalesIntentApprovalOldDCs = SalesIntentApproval;
        //        //if (reader.Read())
        //        //{
        //        //    SalesIntentApprovalOldDCs.TotRec = Convert.ToInt32(reader["TotRec"]);
        //        //}
        //        return SalesIntentApprovalOldDCs; ;
        //    }


        //}
        [Route("SalesIndentPendingApproval")]
        [HttpPost]
        public SalesIntentApprovalMainListDC GetSaSalesIndentPending(Pending_Request obj)
        {

            using (var authContext = new AuthContext())
            {
                int Skiplist = (obj.skip - 1) * obj.take;
                //GetSalesIntentApproval API Working
                SalesIntentApprovalMainListDC SalesIntentApprovalOldDCs = new SalesIntentApprovalMainListDC();
                List<SalesIntentApprovalDC> SalesIntentApproval = new List<SalesIntentApprovalDC>();
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var param1 = new SqlParameter("@ProductName", obj.productname);
                var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
                var param3 = new SqlParameter("@Skip", Skiplist);
                var param4 = new SqlParameter("@Take", obj.take);// new change
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[PendingSalesIntentRequest]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.CommandTimeout = 1200; // for Execute Timedout
                var reader = cmd.ExecuteReader();
                SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentApprovalDC>(reader).ToList();

                reader.NextResult();

                SalesIntentApprovalOldDCs.salesIntentApprovalDCs = SalesIntentApproval;
                if (reader.Read())
                {
                    SalesIntentApprovalOldDCs.TotalRec = Convert.ToInt32(reader["TotalRec"]);
                }
                //if (itemForeCastPOResponses != null && itemForeCastPOResponses.Any())
                //{
                //    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                //    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                //    var itemWarehouse = itemForeCastPOResponses.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMrpId }).ToList();
                //    var list = await tripPlannerHelper.RocTagValueGet(itemWarehouse);
                //    if (list != null && list.Any())
                //    {
                //        foreach (var da in itemforcastPOEdit.ItemForeCastPOResponses)
                //        {
                //            da.Tag = list.Where(x => x.ItemMultiMRPId == da.ItemMultiMrpId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                //        }
                //    }

                //}

                if (SalesIntentApproval != null && SalesIntentApproval.Any())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = SalesIntentApproval.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId.Value }).ToList();
                    var list = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null)
                    {
                        foreach (var da in SalesIntentApproval)
                        {
                            da.Tag = list.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }
                }

                return SalesIntentApprovalOldDCs; ;
            }

        }


        //[Route("SalesIndentPendingApprovalExpo")]
        //[HttpPost]
        //public SalesIntentApprovalMainListDCExpo GetSaSalesIndentPendingExpo(Pending_Request obj)
        //{ 

        //    using (var authContext = new AuthContext())
        //    {
        //        int Skiplist = (obj.skip - 1) * obj.take;
        //        //GetSalesIntentApproval API Working
        //        SalesIntentApprovalMainListDCExpo SalesIntentApprovalOldDCs = new SalesIntentApprovalMainListDCExpo();
        //        List<SalesIntentApprovalDCExpo> SalesIntentApproval = new List<SalesIntentApprovalDCExpo>();
        //        if (authContext.Database.Connection.State != ConnectionState.Open)
        //            authContext.Database.Connection.Open();

        //        var IdDt = new DataTable();
        //        IdDt.Columns.Add("IntValue");
        //        foreach (var item in obj.subSubCategoriesIds)
        //        {
        //            var dr = IdDt.NewRow();
        //            dr["IntValue"] = item;
        //            IdDt.Rows.Add(dr);
        //        }
        //        var param = new SqlParameter("subSubCategoriesIds", IdDt);
        //        param.SqlDbType = SqlDbType.Structured;
        //        param.TypeName = "dbo.IntValues";
        //        var param1 = new SqlParameter("@ProductName", obj.productname);
        //        var param2 = new SqlParameter("@Dates", obj.month ?? (object)DBNull.Value);
        //        //var param3 = new SqlParameter("@Skip", Skiplist);
        //        //var param4 = new SqlParameter("@Take", obj.take);// new change
        //        var cmd = authContext.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[PendingSalesIntentRequestExpo]";
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        cmd.Parameters.Add(param);
        //        cmd.Parameters.Add(param1);
        //        cmd.Parameters.Add(param2);
        //        //cmd.Parameters.Add(param3);
        //        //cmd.Parameters.Add(param4);
        //        cmd.CommandTimeout = 1200; // for Execute Timedout


        //        var reader = cmd.ExecuteReader();
        //        SalesIntentApproval = ((IObjectContextAdapter)authContext).ObjectContext.Translate<SalesIntentApprovalDCExpo>(reader).ToList();

        //        reader.NextResult();

        //        SalesIntentApprovalOldDCs.salesIntentApprovalDCs = SalesIntentApproval;
        //        //if (reader.Read())
        //        //{
        //        //    SalesIntentApprovalOldDCs.TotalRec = Convert.ToInt32(reader["TotalRec"]);
        //        //}
        //        return SalesIntentApprovalOldDCs; ;
        //    }

        //}

        [HttpPost]
        [Route("GetBuyerApproveItemForeCast")]
        public async Task<BuyerApproveItemForeCastData> GetBuyerApproveItemForeCast(PurchaseItemForeCastRequest itemForeCastRequest)
        {
            BuyerApproveItemForeCastData buyerApproveItemForeCastData = new BuyerApproveItemForeCastData();

            List<BuyerApproveItemForeCast> buyerApproveItemForeCasts = new List<BuyerApproveItemForeCast>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";


                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.supplierIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param7 = new SqlParameter("@supplierIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                if (string.IsNullOrEmpty(itemForeCastRequest.prType)) itemForeCastRequest.prType = "not";

                var param5 = new SqlParameter("@fulfillthrowId", itemForeCastRequest.fulfillthrowId);
                var param6 = new SqlParameter("@prType", itemForeCastRequest.prType);
                var param8 = new SqlParameter("@NetStock", itemForeCastRequest.NetStock);  // new change

                var cmd = authContext.Database.Connection.CreateCommand();
                //cmd.CommandText = "[dbo].[GetBuyerSaveForPO]"; old
                // cmd.CommandText = "[dbo].[GetPurchaseSaveForPO]";  //new at 26 Dec
                // cmd.CommandText = "[dbo].[GetPurchaseSaveForPOFrieght]";
                cmd.CommandText = "[dbo].[GetPurchaseSaveForPOFrieghtCharge]"; // new change for freight/weight/buyername/itemnumber
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);

                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);
                cmd.Parameters.Add(param7);
                cmd.Parameters.Add(param8);

                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                cmd.CommandTimeout = 1200; // for Execute Timedout

                // Run the sproc
                var reader = cmd.ExecuteReader();
                buyerApproveItemForeCasts = ((IObjectContextAdapter)authContext).ObjectContext.Translate<BuyerApproveItemForeCast>(reader).ToList();

                reader.NextResult();

                buyerApproveItemForeCastData.BuyerApproveItemForeCasts = buyerApproveItemForeCasts;
                if (reader.Read())
                {
                    buyerApproveItemForeCastData.TotalRecord = Convert.ToInt32(reader["TotalCount"]);
                }
                return buyerApproveItemForeCastData;
            }
        }


        [HttpPost]
        [Route("GetPurchaseApproveItemForeCastExport")]
        public async Task<BuyerApproveItemForeCastDataExport> GetBuyerApproveItemForeCastExport(PurchaseItemForeCastRequest itemForeCastRequest)
        {
            BuyerApproveItemForeCastDataExport buyerApproveItemForeCastData = new BuyerApproveItemForeCastDataExport();

            List<BuyerApproveItemForeCastExport> buyerApproveItemForeCasts = new List<BuyerApproveItemForeCastExport>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";


                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.supplierIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param7 = new SqlParameter("@supplierIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                if (string.IsNullOrEmpty(itemForeCastRequest.prType)) itemForeCastRequest.prType = "not";

                var param5 = new SqlParameter("@fulfillthrowId", itemForeCastRequest.fulfillthrowId);
                var param6 = new SqlParameter("@prType", itemForeCastRequest.prType);
                var param8 = new SqlParameter("@NetStock", itemForeCastRequest.NetStock);  // new change

                var cmd = authContext.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[GetPurchaseSaveExport]"; // new change for freight/weight/buyername/itemnumber
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);

                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);
                cmd.Parameters.Add(param7);
                cmd.Parameters.Add(param8);

                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                cmd.CommandTimeout = 1200;


                var reader = cmd.ExecuteReader();
                buyerApproveItemForeCasts = ((IObjectContextAdapter)authContext).ObjectContext.Translate<BuyerApproveItemForeCastExport>(reader).ToList();

                reader.NextResult();

                buyerApproveItemForeCastData.BuyerApproveItemForeCasts = buyerApproveItemForeCasts;
                if (reader.Read())
                {
                    buyerApproveItemForeCastData.TotalRecord = Convert.ToInt32(reader["TotalCount"]);
                }
                return buyerApproveItemForeCastData;
            }
        }

        [HttpGet]
        [Route("GenerateSystemForeCast")]
        public async Task<bool> GenerateSystemForeCast()
        {
            if (DateTime.Now.Day == 1)
            {
                List<ItemForSystemForecast> ItemForSystemForecasts = new List<ItemForSystemForecast>();
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemForSystemForecast]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var reader = cmd.ExecuteReader();
                    ItemForSystemForecasts = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<ItemForSystemForecast>(reader).ToList();
                }
                List<SystemForecast> FinalSystemForecasts = new List<SystemForecast>();
                double alpha = 0.00;
                List<double> alphas = new List<double>();
                while (alpha <= 1)
                {
                    alphas.Add(alpha);
                    alpha = Math.Round(alpha + 0.01, 2);
                }
                foreach (var item in ItemForSystemForecasts.GroupBy(x => new { x.WarehouseId, x.ItemMultiMRPId }).ToList())
                {
                    //ParallelLoopResult parellelResult = Parallel.ForEach(ItemForSystemForecasts.GroupBy(x => new { x.WarehouseId, x.ItemMultiMRPId }).ToList()
                    //    , (item) =>
                    //{
                    SystemForecast SystemForecast = new SystemForecast
                    {
                        ItemMultiMRPId = item.Key.ItemMultiMRPId,
                        WarehouseId = item.Key.WarehouseId,
                        AlphaCalculations = new List<AlphaCalculation>()
                    };
                    List<ItemForSystemForecast> calItemData = item.OrderBy(x => x.monthDate).ToList();
                    List<AlphaCalculation> AlphaCalculations = new List<AlphaCalculation>();
                    //var todayDate = DateTime.Now.AddMonths(-1);
                    //DateTime preMonthDate = new DateTime(todayDate.Year, todayDate.Month, 1);
                    //if (!calItemData.Any(x => x.monthDate == preMonthDate))
                    //{
                    //    calItemData.Add(new ItemForSystemForecast { Id = 0, ItemMultiMRPId = item.Key.ItemMultiMRPId, WarehouseId = item.Key.WarehouseId, monthDate = preMonthDate, OrderQty = 0 });
                    //}
                    //ParallelLoopResult parellelResult = Parallel.ForEach(alphas, (alph) =>
                    //{
                    if (calItemData.Count > 5)
                    {
                        foreach (var alph in alphas)
                        {
                            AlphaCalculations = new List<AlphaCalculation>();
                            AlphaCalculation alphaCal = new AlphaCalculation
                            {
                                Alpha = alph,
                            };
                            for (int i = 0; i < calItemData.Count; i++)
                            {
                                alphaCal = new AlphaCalculation
                                {
                                    Alpha = alph,
                                };
                                if (i == 0)
                                {
                                    alphaCal.Alpha = alph;
                                    alphaCal.ForeCastQty = 0;
                                    alphaCal.Map = Math.Abs(calItemData[i].OrderQty > 0 ? ((calItemData[i].OrderQty - alphaCal.ForeCastQty) / calItemData[i].OrderQty) : 0) * 100;
                                }
                                else if (i == 1)
                                {
                                    alphaCal.Alpha = alph;
                                    alphaCal.ForeCastQty = calItemData[i - 1].OrderQty;
                                    alphaCal.Map = Math.Abs(calItemData[i].OrderQty > 0 ? ((calItemData[i].OrderQty - alphaCal.ForeCastQty) / calItemData[i].OrderQty) : 0) * 100;

                                }
                                else
                                {
                                    alphaCal.Alpha = alph;
                                    alphaCal.ForeCastQty = (calItemData[i - 1].OrderQty * alph) + ((1 - alph) * AlphaCalculations[i - 1].ForeCastQty);
                                    alphaCal.Map = Math.Abs(calItemData[i].OrderQty > 0 ? ((calItemData[i].OrderQty - alphaCal.ForeCastQty) / calItemData[i].OrderQty) : 0) * 100;
                                }

                                AlphaCalculations.Add(alphaCal);
                            }

                            alphaCal.ForeCastQty = (calItemData[calItemData.Count - 1].OrderQty * alph) + ((1 - alph) * alphaCal.ForeCastQty);

                            AlphaCalculations.FirstOrDefault().Map = 0;

                            alphaCal.Map = AlphaCalculations.Skip(1).Take(AlphaCalculations.Count - 1).Average(x => x.Map);

                            SystemForecast.AlphaCalculations.Add(alphaCal);
                        }
                    }
                    else
                    {
                        AlphaCalculation alphaCal = new AlphaCalculation
                        {
                            Alpha = -1,
                            ForeCastQty = calItemData.Max(x => x.OrderQty),
                            Map = 0
                        };
                        SystemForecast.AlphaCalculations.Add(alphaCal);
                    }
                    //});

                    //if (parellelResult.IsCompleted)
                    {
                        var minMapForecast = SystemForecast.AlphaCalculations.FirstOrDefault(x => x.Map == SystemForecast.AlphaCalculations.Min(y => y.Map));
                        SystemForecast.ForeCastQty = minMapForecast.ForeCastQty;
                        SystemForecast.Map = minMapForecast.Map;
                        SystemForecast.Alpha = minMapForecast.Alpha;
                        FinalSystemForecasts.Add(SystemForecast);
                    }
                    // });
                }
                if (FinalSystemForecasts != null && FinalSystemForecasts.Any())
                {
                    using (var db = new AuthContext())
                    {
                        string query = "EXEC sp_SystemItemForecastsHistory ";
                        db.Database.ExecuteSqlCommand(query);
                    }

                    List<TblSystemItemForecast> tbl = FinalSystemForecasts.Select(x => new TblSystemItemForecast
                    {
                        ItemMultiMRPId = x.ItemMultiMRPId,
                        WarehouseId = x.WarehouseId,
                        ForeCastQty = Convert.ToInt32(x.ForeCastQty),
                        Alpha = x.Alpha,
                        Map = x.Map
                    }).ToList();
                    var bulkTarget = new BulkOperations();
                    bulkTarget.Setup<TblSystemItemForecast>(x => x.ForCollection(tbl))
                        .WithTable("SystemItemForecasts")
                        .WithBulkCopyBatchSize(4000)
                         .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                         .WithSqlCommandTimeout(720) // Default is 600 seconds
                         .AddAllColumns()
                          .BulkInsert();
                    bulkTarget.CommitTransaction("AuthContext");
                }
            }
            return true;
        }



        [Route("JobInsertBrandForecastValues")]
        [HttpGet]
        public bool InsertBrandForecastValues()
        {
            using (var db = new AuthContext())
            {
                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                db.Database.CommandTimeout = 1200;
                var JObData = db.Database.ExecuteSqlCommand("EXEC InsertBrandForecastValues");
            }
            return true;
        }


        [Route("JobCalculateItemForecastQty")]
        [HttpGet]
        public bool CalculateItemForecastQty()
        {
            if (DateTime.Now.Day == 1)
            {
                using (var db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();
                    db.Database.CommandTimeout = 1200;
                    var JObData = db.Database.ExecuteSqlCommand("EXEC CalculateItemForecastQty");
                }
            }
            return true;
        }

        [Route("GetFutureForcastItem")]
        [HttpGet]
        public async Task<List<FutureForcastItemDC>> FutureForcastItem(int Subsubcategoryid, string Number)
        {
            using (var context = new AuthContext())
            {
                if (Number == null || Number == "undefined")
                {
                    Number = "";
                }
                List<FutureForcastItemDC> list = new List<FutureForcastItemDC>();
                ForcastManager forcastManager = new ForcastManager();
                list = await forcastManager.GetFutureForcastItem(Subsubcategoryid, Number);
                return list;
            }
        }
        [Route("GetFutureForcastMapping")]
        [HttpGet]
        public async Task<futureMrpResponse> FutureForcastMapping(string ItemNumber, int warehouseId)
        {
            futureMrpResponse futureMrpResponse = new futureMrpResponse();
            using (var context = new AuthContext())
            {
                List<FutureForcastMappingDC> list = new List<FutureForcastMappingDC>();
                ForcastManager forcastManager = new ForcastManager();
                list = await forcastManager.GetFutureForcastMapping(ItemNumber);
                futureMrpResponse.FutureForcastMappingDCs = list;
                List<int> ids = list.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                var mapmrps = context.FutureMrpMappings.Where(x => x.WarehouseId == warehouseId && ids.Contains(x.ItemMultiMRPId) && x.IsActive).ToList();
                futureMrpResponse.MapMrpIds = mapmrps.GroupBy(x => x.ItemMultiMRPId).Select(x => new MapMrpId { ItemMultiMrpId = x.Key, MapMrpIds = x.Select(y => y.MappingMRPId).ToList() }).ToList();

                return futureMrpResponse;
            }
        }

        [Route("FutureForcastMappingAdd")]
        [HttpPost]
        public async Task<ForeCastUpdateResponse> ForcastMappingAdd(List<FutureForeCastAdd> FutureForeCast)
        {
            ForeCastUpdateResponse ForeCastUpdateResponse = new ForeCastUpdateResponse();
            List<FutureMrpMapping> addlist = new List<FutureMrpMapping>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var authContext = new AuthContext())
            {
                var ItemMultiMRPIds = FutureForeCast.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                var WarehouseIds = FutureForeCast.Select(x => x.WarehouseId).Distinct().ToList();
                var mrpNumber = authContext.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMRPIds.FirstOrDefault())?.ItemNumber;
                List<int> remainingmrpIds = new List<int>();
                List<FutureMrpMapping> deletedMrps = null;
                if (!string.IsNullOrEmpty(mrpNumber))
                {
                    var allmrpids = authContext.ItemMultiMRPDB.Where(x => x.ItemNumber == mrpNumber && x.Deleted == false).Select(x => x.ItemMultiMRPId).ToList();
                    remainingmrpIds = allmrpids.Except(ItemMultiMRPIds).ToList();
                    if (remainingmrpIds != null && remainingmrpIds.Any())
                        deletedMrps = authContext.FutureMrpMappings.Where(x => remainingmrpIds.Contains(x.ItemMultiMRPId)
                      && WarehouseIds.Contains(x.WarehouseId)).ToList();
                }

                var FutureMrpMappings = authContext.FutureMrpMappings.Where(x => ItemMultiMRPIds.Contains(x.ItemMultiMRPId)
                 && WarehouseIds.Contains(x.WarehouseId)).ToList();

                if (deletedMrps != null && deletedMrps.Any())
                {
                    foreach (var item in deletedMrps)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        item.ModifiedBy = userid;
                        item.ModifiedDate = DateTime.Now;
                        authContext.Entry(item).State = EntityState.Modified;
                    }
                    authContext.Commit();
                }

                if (FutureForeCast != null && FutureForeCast.Any())
                {
                    foreach (var item in FutureForeCast)
                    {
                        List<int> MappingIds = new List<int>();
                        var DbfilterFutureMrpMappings = FutureMrpMappings.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).ToList();
                        foreach (var mapping in item.MappingMRPId)
                        {
                            if (DbfilterFutureMrpMappings.Any(x => x.MappingMRPId == mapping))
                            {
                                var updatedItem = DbfilterFutureMrpMappings.FirstOrDefault(x => x.MappingMRPId == mapping);
                                updatedItem.IsActive = true;
                                updatedItem.IsDeleted = false;
                                updatedItem.ModifiedBy = userid;
                                updatedItem.ModifiedDate = DateTime.Now;
                                authContext.Entry(updatedItem).State = EntityState.Modified;
                            }
                            else
                            {
                                FutureMrpMapping addfutureMrpMapping = new FutureMrpMapping();
                                addfutureMrpMapping.ItemMultiMRPId = item.ItemMultiMRPId;
                                addfutureMrpMapping.MappingMRPId = mapping;
                                addfutureMrpMapping.WarehouseId = item.WarehouseId;
                                addfutureMrpMapping.CreatedDate = DateTime.Now;
                                addfutureMrpMapping.CreatedBy = userid;
                                addfutureMrpMapping.IsActive = true;
                                addfutureMrpMapping.IsDeleted = false;
                                addlist.Add(addfutureMrpMapping);
                            }
                        }

                        if (DbfilterFutureMrpMappings.Any(x => !item.MappingMRPId.Contains(x.MappingMRPId)))
                        {
                            foreach (var deleteditem in DbfilterFutureMrpMappings.Where(x => !item.MappingMRPId.Contains(x.MappingMRPId)))
                            {

                                deleteditem.IsActive = false;
                                deleteditem.IsDeleted = true;
                                deleteditem.ModifiedBy = userid;
                                deleteditem.ModifiedDate = DateTime.Now;
                                authContext.Entry(deleteditem).State = EntityState.Modified;
                            }
                        }
                    }
                    if (addlist.Count() > 0)
                    {
                        authContext.FutureMrpMappings.AddRange(addlist);
                    }
                    if (authContext.Commit() > 0)
                    {
                        ForeCastUpdateResponse.msg = "Updated successfully.";
                    }
                    else
                    {
                        ForeCastUpdateResponse.msg = "Record not updated. Please try after some time.";
                    }
                }
                else
                {

                    ForeCastUpdateResponse.msg = "There is no record for update.";
                }
            }
            return ForeCastUpdateResponse;
        }

        #region  Create Po Or Internal Transfer 
        //[Route("CreatePoOrInternal")]
        //[HttpPost]
        private async Task<CreatePoOrInternalRes> CreatePoOrInternal(CreatePoOrInternalDc obj, AuthContext context)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0, compid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            var CreatePoOrInternalRes = new CreatePoOrInternalRes();
            CreatePoOrInternalRes.Status = false;
            if (userid > 0)
            {
                //using (AuthContext context = new AuthContext())
                //{
                var people = context.Peoples.Where(a => a.PeopleID == userid && a.Active == true).FirstOrDefault();
                var ItemForecastPRRequests = context.ItemForecastPRRequestDb.Where(x => obj.Ids.Contains(x.Id)).ToList();
                if (ItemForecastPRRequests != null && ItemForecastPRRequests.Any(x => x.FulfillobjectId == null))
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in ItemForecastPRRequests.Where(x => x.FulfillobjectId == null).Select(x => x.Id).Distinct().ToList())
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("ItemForecastPRRequestIds", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemToCreatePoOrInternal]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    List<ItemsToCreatePoOrInternalDc> pdList = new List<ItemsToCreatePoOrInternalDc>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        pdList = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<ItemsToCreatePoOrInternalDc>(reader).ToList();
                    }

                    //for PO
                    if (pdList != null && pdList.Any() && ItemForecastPRRequests.Any(x => x.FulfillThrow == 1))
                    {
                        PurchaseOrderMaster poinvoicedata = new PurchaseOrderMaster();
                        if (pdList.Select(x => x.SupplierId).Distinct().Count() > 1)
                        {
                            CreatePoOrInternalRes.Message = "Can't generated due to request contain two different supplier items.";
                            return CreatePoOrInternalRes;
                        }
                        var PickerType = pdList.FirstOrDefault().PickerType;
                        var ETADate = pdList.FirstOrDefault().ETADate;
                        var BuyerId = pdList.FirstOrDefault().BuyerId;
                        var Buyer = context.Peoples.Where(x => x.PeopleID == BuyerId).FirstOrDefault();
                        var prpaymenttype = pdList.FirstOrDefault().PRPaymentType;

                        if (prpaymenttype == "Advance PR")
                        {
                            prpaymenttype = "AdvancePR";
                        }
                        else
                        {
                            prpaymenttype = "CreditPR";
                        }

                        var supplierId = pdList.FirstOrDefault().SupplierId;
                        var supplier = context.Suppliers.Where(s => s.SupplierId == supplierId).SingleOrDefault();
                        if (supplier != null &&
                            ((supplier.IsStopAdvancePr.HasValue && supplier.IsStopAdvancePr.Value)
                            || (supplier.CibilScore.HasValue && supplier.CibilScore.Value < 699))
                            && prpaymenttype == "AdvancePR")
                        {
                            CreatePoOrInternalRes.Message = "Advance PR can't generated for this supplier due to less Cibil Score (Less than 700) Or Stop Advance PR.";

                            return CreatePoOrInternalRes;
                        }
                        //for check supplier retailer cross buying
                        //#region checkcrossbuying
                        //string retailercrossname = null;
                        //if(pdList!=null && pdList.Any())
                        //{
                        //    foreach(var list in pdList)
                        //    {
                        //        var SupplierId = new SqlParameter("@SupplierId", list.SupplierId);
                        //        var ItemmultiMrpId = new SqlParameter("@itemmultiMrpIds", list.ItemMultiMRPId);
                        //        var result = context.Database.SqlQuery<int>("EXEC GetRetailerForSupplierItem @SupplierId,@itemmultiMrpIds", SupplierId, ItemmultiMrpId).FirstOrDefault();
                        //        if (result > 0)
                        //        {
                        //            if (retailercrossname == null) retailercrossname = list.ItemName;
                        //            else retailercrossname += "," + list.ItemName;
                        //        }
                        //    }
                        //    if(retailercrossname != null)
                        //    {
                        //        CreatePoOrInternalRes.Message = "These items "+retailercrossname+"can not be added due to Supplier Retailer CrossBuying";
                        //        return CreatePoOrInternalRes;
                        //    }
                        //}
                        //#endregion
                        #region checkcrossbuying
                        string retailercrossname = null;
                        if (pdList != null && pdList.Any())
                        {
                            var SupplierId = new SqlParameter("@SupplierId", pdList.FirstOrDefault().SupplierId);
                            var itemmultimrp = new DataTable();
                            itemmultimrp.Columns.Add("IntValue");
                            foreach (var item in pdList)
                            {
                                var dr = itemmultimrp.NewRow();
                                dr["IntValue"] = item.ItemMultiMRPId;
                                itemmultimrp.Rows.Add(dr);
                            }
                            var itemmultimrpids = new SqlParameter
                            {
                                ParameterName = "itemmultiMrpIds",
                                SqlDbType = SqlDbType.Structured,
                                TypeName = "dbo.IntValues",
                                Value = itemmultimrp
                            };
                            //foreach (var list in pdList)
                            //{

                            //    var ItemmultiMrpId = new SqlParameter("@itemmultiMrpIds", list.ItemMultiMRPId);

                            //}
                            var supplieritemlist = context.Database.SqlQuery<int>("EXEC GetRetailerForSupplierItem @SupplierId,@itemmultiMrpIds", SupplierId, itemmultimrpids).ToList();
                            if (supplieritemlist != null)
                            {
                                var data = pdList.Where(x => supplieritemlist.Contains(x.ItemMultiMRPId)).Select(x => x.ItemName).ToList();
                                if (data.Any() && data != null)
                                {
                                    retailercrossname = String.Join(",", data.ToList());
                                }
                            }
                            if (retailercrossname != null)
                            {
                                CreatePoOrInternalRes.Status = false;
                                CreatePoOrInternalRes.Message = "These items " + retailercrossname + " can not be added due to Supplier Retailer CrossBuying";
                                return CreatePoOrInternalRes;
                            }

                            //if (result > 0)
                            //{
                            //    if (retailercrossname == null) retailercrossname = list.ItemName;
                            //    else retailercrossname += "," + list.ItemName;
                            //}
                            //if (retailercrossname != null)
                            //{
                            //    CreatePoOrInternalRes.Message = "These items " + retailercrossname + "can not be added due to Supplier Retailer CrossBuying";
                            //    return CreatePoOrInternalRes;
                            //}
                        }
                        #endregion

                        var DepoId = pdList.FirstOrDefault().DepoId;

                        var Depo = context.DepoMasters.Where(d => d.DepoId == DepoId).FirstOrDefault();

                        if(Depo.SupplierId != supplier.SupplierId)
                        {
                            CreatePoOrInternalRes.Status = false;
                            CreatePoOrInternalRes.Message = "Supplier Depo is mismatched !! Please Refresh the Page";
                            return CreatePoOrInternalRes;
                        }

                        #region Stop New PR on Buyer wise 

                        SqlParameter BuyerIdparam = new SqlParameter()
                        {
                            ParameterName = "@buyerid",
                            Value = BuyerId
                        };
                        int setPOcloseCnt = Convert.ToInt32(ConfigurationManager.AppSettings["OpenPOcountBuyerforClose"]);
                        int DaysofPOcountBuyerforClose = Convert.ToInt32(ConfigurationManager.AppSettings["DaysofPOcountBuyerforClose"]);
                        int cnt = context.Database.SqlQuery<int>("getOpenPOcountBuyerWise @buyerid", BuyerIdparam).FirstOrDefault();
                        if (cnt >= setPOcloseCnt && cnt > 0)
                        {
                            CreatePoOrInternalRes.Message = "Your " + cnt + " PO are open more than " + DaysofPOcountBuyerforClose + " days so you can't create new PO ";
                            return CreatePoOrInternalRes;

                        }
                        #endregion
                        #region get Estimate amount
                        /// Get Estimate amount
                        double ETtlamt = 0;
                        #endregion
                        PurchaseOrderMaster pm = new PurchaseOrderMaster();
                        pm.SupplierId = supplier.SupplierId;
                        pm.SupplierName = supplier.Name;
                        pm.CreationDate = DateTime.Now;
                        pm.WarehouseId = pdList.FirstOrDefault().WarehouseId;
                        pm.CompanyId = 1;
                        pm.WarehouseName = pdList.FirstOrDefault().WarehouseName;

                        pm.PRType = 1;
                        pm.PRStatus = 1;
                        pm.PoType = "Auto PR";
                        pm.IsPR = true;
                        pm.BuyerId = BuyerId;
                        pm.BuyerName = Buyer.DisplayName;
                        pm.Active = true;

                        pm.IsCashPurchase = false;

                        pm.PickerType = PickerType;
                        PurchaseOrderNewController cntlr = new PurchaseOrderNewController();
                        SupplierOutstandingAmount outstanding = cntlr.GetSupplierOutStandingAmount(supplier.SupplierId);
                        pm.Advance_Amt = Convert.ToDouble(outstanding.AdvanceAmount);

                        pm.DepoId = Depo != null ? Depo.DepoId : 0;
                        pm.DepoName = Depo != null ? Depo.DepoName : null;
                        pm.CreatedBy = people.PeopleFirstName + " " + people.PeopleLastName;
                        pm.PRPaymentType = prpaymenttype;
                        if (prpaymenttype == "AdvancePR")
                        {
                            pm.SupplierCreditDay = 0;
                        }
                        else
                        {
                            pm.SupplierCreditDay = supplier.PaymentTerms;
                        }
                        pm.FreightCharge = 0;
                        foreach (var frc in ItemForecastPRRequests)
                        {
                            pm.FreightCharge += frc.FreightCharge;

                        }
                        //new businesstype
                        pm.IsDirectSupplier = pdList.FirstOrDefault().bussinessType;
                        //pm.ExpiryDays = context.Suppliers.Where(x => x.SupplierId == supplier.SupplierId).Select(y => y.ExpiryDays).FirstOrDefault();
                        pm.ExpiryDays = supplier.ExpiryDays > 0 ? supplier.ExpiryDays : 7;      //aarti
                        pm.PurchaseOrderDetail = new List<PurchaseOrderDetail>();
                        for (var i = 0; i < pdList.ToList().Count(); i++)
                        {
                            PurchaseOrderDetail pd = new PurchaseOrderDetail();
                            int qty = pdList[i].FinalQty;
                            ETtlamt += Convert.ToDouble(pdList[i].BuyingPrice) * qty;
                            pd.ItemId = pdList[i].Itemid;
                            pd.ItemNumber = pdList[i].ItemNumber;
                            pd.itemBaseName = pdList[i].itemBaseName;
                            pd.ItemMultiMRPId = pdList[i].ItemMultiMRPId;
                            pd.HSNCode = pdList[i].HSNCode;
                            pd.MRP = pdList[i].MRP;
                            pd.SellingSku = pdList[i].SellingSku;
                            pd.ItemName = pdList[i].ItemName;
                            pd.PurchaseQty = qty;
                            pd.CreationDate = DateTime.Now;
                            pd.Status = "ordered";
                            pd.MOQ = pdList[i].PurchaseMinOrderQty;
                            pd.Price = pdList[i].BuyingPrice ?? 0;
                            pd.WarehouseId = pdList[i].WarehouseId;
                            pd.CompanyId = pm.CompanyId;
                            pd.WarehouseName = pdList[i].WarehouseName;
                            pd.SupplierId = supplier.SupplierId;
                            pd.SupplierName = supplier.Name;
                            pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
                            pd.PurchaseName = pdList[i].PurchaseUnitName;
                            pd.PurchaseSku = pdList[i].PurchaseSku;
                            pd.DepoId = Depo != null ? Depo.DepoId : 0;
                            pd.DepoName = Depo != null ? Depo.DepoName : null;
                            pd.ConversionFactor = pdList[i].PurchaseMinOrderQty;
                            var manager = new ItemLedgerManager();
                            List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                            ItemClassificationDC obja = new ItemClassificationDC();
                            obja.WarehouseId = pd.WarehouseId ?? 0;
                            obja.ItemNumber = pd.ItemNumber;
                            objItemClassificationDClist.Add(obja);
                            List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                            pd.Category = _objItemClassificationDClist.Count == 0 ? "D" : _objItemClassificationDClist[0].Category;
                            var wquery = from m in context.itemMasters
                                         join c in context.ItemMasterCentralDB on m.Number equals c.Number
                                         where m.ItemId == pd.ItemId
                                         select new ItemWeight { weight = c.weight, weighttype = c.weighttype };
                            var witem = wquery.FirstOrDefault();
                            if (witem != null)
                            {
                                pd.Weight = witem.weight;
                                pd.WeightType = witem.weighttype;
                            }
                            pm.PurchaseOrderDetail.Add(pd);

                        }
                        pm.ETotalAmount = ETtlamt;
                        pm.ETADate = ETADate;
                        pm.Status = pm.ETotalAmount < 300000 ? "Self Approved" : "PR Send for Approval";
                        pm.progress = pm.ETotalAmount < 300000 ? "50" : "20";

                        if (pm.Status == "Self Approved")
                        {
                            pm.Status = "PR Payment Approval Pending";
                            pm.PRStatus = 3;
                            pm.PRType = 1;
                            //pm.Status = "Approved";
                            //pm.PRType = 0;
                            //pm.PRStatus = 5;
                            pm.Advance_Amt = pm.ETotalAmount;
                        }
                        context.DPurchaseOrderMaster.Add(pm);
                        if (context.Commit() > 0)
                        {
                            var POrderId = new SqlParameter("@Poid", pm.PurchaseOrderId);
                            poinvoicedata = context.Database.SqlQuery<PurchaseOrderMaster>("exec Sp_getpoinvoicenumber @Poid", POrderId).FirstOrDefault();
                            pm.PoInvoiceNo = poinvoicedata.PoInvoiceNo;
                            context.Entry(pm).State = EntityState.Modified;
                            // string smsTempmsg = " is waiting for your approval. ShopKirana";
                            string smsTempmsg = ""; //"ShopKirana PR id: {#var#} is waiting for your approval. ShopKirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
                            smsTempmsg = dltSMS == null ? "" : dltSMS.Template;
                            smsTempmsg = smsTempmsg.Replace("{#var#}", pm.PurchaseOrderId.ToString());

                            if (ETtlamt < 300000)
                            {
                                var PRStatusData = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == pm.BuyerId).FirstOrDefault();
                                if (PRStatusData == null)
                                {
                                    PRApprovelsStatus pr = new PRApprovelsStatus();
                                    pr.ApprovalID = pm.BuyerId.Value;
                                    pr.PurchaseOrderID = pm.PurchaseOrderId;
                                    pr.IsApprove = 1;
                                    pr.IsActive = true;
                                    pr.Comments = "Approved By :" + people.DisplayName;
                                    pr.CreatedDate = DateTime.Now;
                                    pr.ModifiedDate = DateTime.Now;
                                    context.PRApprovelsStatus.Add(pr);
                                }
                            }
                            else if (ETtlamt >= 300000 && ETtlamt < 1000000)
                            {

                                #region Payment
                                string ppquery = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name ='{0}') and p.Active = 1 ",
                                                                    "Supplier Payment Approver");

                                List<BuyerMinDc> pbuyerMinDcss = context.Database.SqlQuery<BuyerMinDc>(ppquery).ToList();

                                foreach (var BD in pbuyerMinDcss)
                                {
                                    var ActivePeople = context.Peoples.Where(x => x.PeopleID == BD.PeopleId && x.Active == true).FirstOrDefault();
                                    var WarePermission = context.WarehousePermissionDB.Where(x => x.WarehouseId == pm.WarehouseId && x.PeopleID == BD.PeopleId && x.IsDeleted == false).FirstOrDefault();
                                    if (WarePermission != null && ActivePeople != null)
                                    {

                                        var PRStatusData = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == BD.PeopleId).FirstOrDefault();
                                        if (PRStatusData == null)
                                        {
                                            PRApprovelsStatus pr = new PRApprovelsStatus();
                                            pr.ApprovalID = BD != null ? BD.PeopleId : 0;
                                            pr.PurchaseOrderID = pm.PurchaseOrderId;
                                            pr.IsApprove = 1;
                                            pr.IsActive = true;
                                            pr.Comments = "Approved By :" + ActivePeople.DisplayName;
                                            pr.CreatedDate = DateTime.Now;
                                            pr.ModifiedDate = DateTime.Now;
                                            context.PRApprovelsStatus.Add(pr);
                                            Sms s = new Sms();
                                            //ShopKirana PR id: {#var#} are waiting for your approval.
                                            //string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + " is waiting for your approval. ShopKirana";
                                            string msg = ""; //"ShopKirana PR id: {#var#} is waiting for your approval. ShopKirana";
                                            dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
                                            msg = dltSMS == null ? "" : dltSMS.Template;
                                            msg = msg.Replace("{#var#}", pm.PurchaseOrderId.ToString());

                                            string Mob = ActivePeople.Mobile;
                                            if (Mob != null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }
                                            context.Commit();

                                        }
                                    }
                                }
                                #endregion


                                #region Code Commnets
                                string query = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('{0}')) and p.Active=1",
                                                "Zonal Sourcing lead");

                                List<BuyerMinDc> buyerMinDcs = context.Database.SqlQuery<BuyerMinDc>(query).ToList();

                                foreach (var BD in buyerMinDcs)
                                {
                                    var ActivePeople = context.Peoples.Where(x => x.PeopleID == BD.PeopleId && x.Active == true).FirstOrDefault();
                                    var WarePermission = context.WarehousePermissionDB.Where(x => x.WarehouseId == pm.WarehouseId && x.PeopleID == BD.PeopleId && x.IsDeleted == false).FirstOrDefault();
                                    if (WarePermission != null && ActivePeople != null)
                                    {

                                        var PRStatusData = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == BD.PeopleId).FirstOrDefault();
                                        if (PRStatusData == null)
                                        {
                                            PRApprovelsStatus pr = new PRApprovelsStatus();
                                            pr.ApprovalID = BD != null ? BD.PeopleId : 0;
                                            pr.PurchaseOrderID = pm.PurchaseOrderId;
                                            pr.IsApprove = 1;
                                            pr.IsActive = true;
                                            pr.Comments = "Approved By :" + ActivePeople.DisplayName;
                                            pr.CreatedDate = DateTime.Now;
                                            pr.ModifiedDate = DateTime.Now;
                                            context.PRApprovelsStatus.Add(pr);
                                            Sms s = new Sms();
                                            string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smsTempmsg;
                                            string Mob = ActivePeople.Mobile;
                                            if (Mob != null) { s.sendOtp(Mob, msg, ""); }

                                            context.Commit();
                                        }
                                    }
                                }

                                #endregion

                                var itemIds = pm.PurchaseOrderDetail.Select(x => x.ItemId).ToList();
                                var items = context.itemMasters.Where(z => itemIds.Contains(z.ItemId)).ToList();
                                var SubsubCategoryid = items.Select(x => x.SubsubCategoryid).Distinct().ToList();

                                string querys = @"select  distinct s.StoreId from StoreBrands s with (nolock)
                                                                     inner join BrandCategoryMappings b with (nolock) on s.BrandCategoryMappingId=b.BrandCategoryMappingId
                                                                     inner join SubcategoryCategoryMappings sc with (nolock) on b.SubCategoryMappingId=sc.SubCategoryMappingId
                                                                     where b.IsActive=1 and b.Deleted=0 and s.IsActive=1 and s.IsDeleted=0
                                                                     and sc.IsActive=1 and sc.Deleted=0 and b.SubsubCategoryId in (" + string.Join(",", SubsubCategoryid) + ")";
                                List<long> storeIds = context.Database.SqlQuery<long>(querys).ToList();
                                var store = context.StoreDB.Where(z => storeIds.Contains(z.Id)).ToList();
                                foreach (var st in store)
                                {
                                    var ActivePeople = context.Peoples.Where(x => x.PeopleID == st.OwnerId && x.Active == true).FirstOrDefault();
                                    if (ActivePeople != null)
                                    {
                                        var PRStatusData1 = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == st.OwnerId).FirstOrDefault();
                                        if (PRStatusData1 == null)
                                        {
                                            PRApprovelsStatus pr = new PRApprovelsStatus();
                                            pr.ApprovalID = st != null ? st.OwnerId : 0;
                                            pr.PurchaseOrderID = pm.PurchaseOrderId;
                                            pr.IsApprove = 1;
                                            pr.IsActive = true;
                                            pr.Comments = "Approved By :" + ActivePeople.DisplayName;
                                            pr.CreatedDate = DateTime.Now;
                                            pr.ModifiedDate = DateTime.Now;
                                            context.PRApprovelsStatus.Add(pr);
                                            Sms s = new Sms();
                                            //string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smsTempmsg;
                                            string msg = smsTempmsg;
                                            string Mob = ActivePeople.Mobile;
                                            if (Mob != null && dltSMS != null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }
                                            context.Commit();
                                        }
                                    }
                                }
                                if (store.Count == 0)
                                {
                                    string queryNew = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('{0}','{1}')) and p.Active=1",
                                          "Zonal Sourcing lead", "Region sales lead");

                                    List<BuyerMinDc> buyerMinDcss = context.Database.SqlQuery<BuyerMinDc>(queryNew).ToList();

                                    foreach (var BD in buyerMinDcss)
                                    {
                                        var ActivePeople = context.Peoples.Where(x => x.PeopleID == BD.PeopleId && x.Active == true).FirstOrDefault();
                                        var WarePermission = context.WarehousePermissionDB.Where(x => x.WarehouseId == pm.WarehouseId && x.PeopleID == BD.PeopleId && x.IsDeleted == false).FirstOrDefault();
                                        if (WarePermission != null && ActivePeople != null)
                                        {

                                            var PRStatusData = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == BD.PeopleId).FirstOrDefault();
                                            if (PRStatusData == null)
                                            {
                                                PRApprovelsStatus pr = new PRApprovelsStatus();
                                                pr.ApprovalID = BD != null ? BD.PeopleId : 0;
                                                pr.PurchaseOrderID = pm.PurchaseOrderId;
                                                pr.IsApprove = 1;
                                                pr.IsActive = true;
                                                pr.Comments = "Approved By :" + ActivePeople.DisplayName;
                                                pr.CreatedDate = DateTime.Now;
                                                pr.ModifiedDate = DateTime.Now;
                                                context.PRApprovelsStatus.Add(pr);
                                                Sms s = new Sms();
                                                string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smsTempmsg;
                                                string Mob = ActivePeople.Mobile;
                                                if (Mob != null) { s.sendOtp(Mob, msg, ""); }
                                                context.Commit();
                                            }
                                        }

                                    }
                                }
                                pm.Status = "PR Payment Approval Pending";
                                pm.PRStatus = 3;
                                pm.PRType = 1;
                                context.Entry(pm).State = EntityState.Modified;
                            }
                            else if (ETtlamt >= 1000000)
                            {


                                string query = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name ='{0}') and p.Active = 1 ",
                                                "Supplier Payment Approver");

                                BuyerMinDc buyerMinDc = context.Database.SqlQuery<BuyerMinDc>(query).FirstOrDefault();
                                People peoples = context.Peoples.Where(q => q.PeopleID == buyerMinDc.PeopleId).FirstOrDefault();

                                var PRStatusData = context.PRApprovelsStatus.Where(x => x.PurchaseOrderID == pm.PurchaseOrderId && x.ApprovalID == buyerMinDc.PeopleId).FirstOrDefault();
                                if (PRStatusData == null)
                                {
                                    PRApprovelsStatus pr = new PRApprovelsStatus();
                                    pr.ApprovalID = buyerMinDc.PeopleId;
                                    pr.PurchaseOrderID = pm.PurchaseOrderId;
                                    pr.IsApprove = 1;
                                    pr.IsActive = true;
                                    pr.Comments = "Approved By :" + peoples.DisplayName;
                                    pr.CreatedDate = DateTime.Now;
                                    pr.ModifiedDate = DateTime.Now;
                                    context.PRApprovelsStatus.Add(pr);

                                    if (pm != null)
                                    {
                                        var itemIds = pm.PurchaseOrderDetail.Select(x => x.ItemId).ToList();
                                        var items = context.itemMasters.Where(z => itemIds.Contains(z.ItemId)).ToList();
                                        var SubsubCategoryids = items.Select(x => x.SubsubCategoryid).Distinct().ToList();
                                        List<BrandBuyer> BDs = context.BrandBuyerDB.Where(x => SubsubCategoryids.Contains(x.BrandId) && x.WarehosueId == pm.WarehouseId).ToList();

                                        int BuyerIds = BDs != null && BDs.Any() ? BDs.FirstOrDefault().BuyerId : 2088;
                                        People Name = context.Peoples.Where(x => x.PeopleID == BuyerIds).FirstOrDefault();

                                        pm.Status = "PR Payment Approval Pending";
                                        pm.PRStatus = 3;
                                        pm.PRType = 1;
                                        pm.BuyerId = Name.PeopleID;
                                        pm.BuyerName = Name.DisplayName;
                                        context.Entry(pm).State = EntityState.Modified;
                                        context.Commit();
                                        //if (context.Commit() > 0)
                                        //{

                                        //}
                                    }
                                }
                            }


                            //payment approval;
                            int warehouseid = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId).Select(x => x.WarehouseId).FirstOrDefault();
                            var itemIdss = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId && x.IsDeleted == false && x.CompanyId == compid && pm.WarehouseId == warehouseid).Select(x => x.ItemId).Distinct().ToList();
                            var itemss = context.itemMasters.Where(z => itemIdss.Contains(z.ItemId)).ToList();
                            var SubsubCategoryidss = itemss.Select(x => x.SubsubCategoryid).Distinct().ToList();

                            string querysss = @"select  distinct s.StoreId from StoreBrands s with (nolock)
                                                                     inner join BrandCategoryMappings b with (nolock) on s.BrandCategoryMappingId=b.BrandCategoryMappingId
                                                                     inner join SubcategoryCategoryMappings sc with (nolock) on b.SubCategoryMappingId=sc.SubCategoryMappingId
                                                                     where b.IsActive=1 and b.Deleted=0 and s.IsActive=1 and s.IsDeleted=0
                                                                     and sc.IsActive=1 and sc.Deleted=0 and b.SubsubCategoryId in (" + string.Join(",", SubsubCategoryidss) + ")";
                            List<long> storeIdss = context.Database.SqlQuery<long>(querysss).ToList();

                            var roleName = "";
                            foreach (var store in storeIdss)
                            {
                                roleName = context.PRApprovalDB.Where(x => x.AmountlmtMin <= pm.ETotalAmount && x.AmountlmtMax >= pm.ETotalAmount && !x.IsDeleted && x.StoreIds.Contains(store.ToString())).Select(x => x.RoleName).FirstOrDefault();
                            }
                            //var roleName = context.PRApprovalDB.Where(x => x.AmountlmtMin <= pm.ETotalAmount && x.AmountlmtMax >= pm.ETotalAmount && !x.IsDeleted).Select(x => x.RoleName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                string query = string.Format("select  p.DisplayName,p.PeopleID,p.Mobile from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('{0}')) and p.Active=1",
                                                           roleName);

                                BuyerMinDc buyerMinDcs = context.Database.SqlQuery<BuyerMinDc>(query).FirstOrDefault();
                                var check = context.PRPaymentAppoved.Where(x => x.ApprovedBY == buyerMinDcs.PeopleId && x.PRId == pm.PurchaseOrderId).FirstOrDefault();
                                if (check == null)
                                {
                                    PRPaymentAppoved pRPaymentAppoved = new PRPaymentAppoved();
                                    pRPaymentAppoved.ApprovedBY = buyerMinDcs.PeopleId;
                                    pRPaymentAppoved.CreatedBy = userid;
                                    pRPaymentAppoved.CreatedDate = DateTime.Now;
                                    pRPaymentAppoved.IsApproved = false;
                                    pRPaymentAppoved.IsActive = true;
                                    pRPaymentAppoved.PRId = pm.PurchaseOrderId;
                                    context.PRPaymentAppoved.Add(pRPaymentAppoved);
                                }
                                else
                                {

                                    check.IsApproved = false;
                                    check.IsActive = true;
                                    check.IsDeleted = false;
                                    check.ModifiedDate = DateTime.Now;
                                    context.Entry(check).State = EntityState.Modified;

                                }
                                Sms s = new Sms();
                                //  string msg = " ShopKirana " + Environment.NewLine + " PR id: " + pm.PurchaseOrderId + " are waiting for your Payment Approval.";
                                string msg = "";//"ShopKirana PR id: {#var#} are waiting for your Payment Approval.";
                                dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Payment_Waiting_Approval");
                                msg = dltSMS == null ? "" : dltSMS.Template;
                                msg = msg.Replace("{#var#}", pm.PurchaseOrderId.ToString());

                                string Mob = buyerMinDcs.Mobile;
                                if (Mob != null && dltSMS != null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }
                                context.Commit();
                            }
                            ItemForecastPRRequests.ForEach(x =>
                            {
                                x.FulfillobjectId = pm.PurchaseOrderId;
                            });
                            var updatingPRRequestsids = pdList.Select(x => x.Id).ToList();
                            foreach (var item in ItemForecastPRRequests.Where(x => updatingPRRequestsids.Contains(x.Id)))
                            {
                                item.ModifiedDate = DateTime.Now;
                                item.ModifiedBy = people.PeopleID;
                                context.Entry(item).State = EntityState.Modified;
                            }
                            if (pm.Status == "Self Approved" || pm.Status == "Approved")
                            {
                                var PRCheck = context.PurchaseRequestMasterDB.Where(x => x.PurchaseOrderId == pm.PurchaseOrderId).FirstOrDefault();
                                if (PRCheck == null)
                                {
                                    PurchaseRequestMaster prm = new PurchaseRequestMaster();
                                    prm.SupplierId = pm.SupplierId;
                                    prm.SupplierName = pm.SupplierName;
                                    prm.CreationDate = DateTime.Now;
                                    prm.WarehouseId = pm.WarehouseId;
                                    prm.CompanyId = pm.CompanyId;
                                    prm.WarehouseName = pm.WarehouseName;
                                    prm.PoType = "Auto PR";
                                    prm.ETotalAmount = pm.ETotalAmount;
                                    prm.BuyerId = pm.BuyerId;
                                    prm.BuyerName = pm.BuyerName;
                                    prm.Active = true;
                                    prm.IsCashPurchase = pm.IsCashPurchase;
                                    prm.CashPurchaseName = pm.CashPurchaseName;
                                    prm.Advance_Amt = pm.ETotalAmount;
                                    prm.DepoId = pm.DepoId;
                                    prm.DepoName = pm.DepoName;
                                    prm.CreatedBy = pm.CreatedBy;
                                    prm.PurchaseOrderId = pm.PurchaseOrderId;
                                    prm.PoInvoiceNo = pm.PoInvoiceNo;
                                    prm.PurchaseOrderRequestDetail = new List<PurchaseOrderRequestDetail>();

                                    foreach (var data in pm.PurchaseOrderDetail)
                                    {
                                        PurchaseOrderRequestDetail pd = new PurchaseOrderRequestDetail();
                                        pd.ItemId = data.ItemId;
                                        pd.ItemNumber = data.ItemNumber;
                                        pd.itemBaseName = data.itemBaseName;
                                        pd.ItemMultiMRPId = data.ItemMultiMRPId;
                                        pd.HSNCode = data.HSNCode;
                                        pd.MRP = data.MRP;
                                        pd.SellingSku = data.SellingSku;
                                        pd.ItemName = data.ItemName;
                                        pd.PurchaseQty = data.PurchaseQty;
                                        pd.CreationDate = DateTime.Now;
                                        pd.Status = "ordered";
                                        pd.MOQ = data.MOQ;
                                        pd.Price = data.Price;
                                        pd.WarehouseId = data.WarehouseId;
                                        pd.CompanyId = data.CompanyId;
                                        pd.WarehouseName = data.WarehouseName;
                                        pd.SupplierId = data.SupplierId;
                                        pd.SupplierName = data.SupplierName;
                                        pd.TotalQuantity = data.TotalQuantity;
                                        pd.PurchaseName = data.PurchaseName;
                                        pd.PurchaseSku = data.PurchaseSku;
                                        pd.DepoId = data.DepoId;
                                        pd.DepoName = data.DepoName;
                                        pd.ConversionFactor = data.ConversionFactor;
                                        pd.PurchaseOrderId = data.PurchaseOrderId;
                                        pd.Category = data.Category;
                                        prm.PurchaseOrderRequestDetail.Add(pd);
                                    }
                                    context.PurchaseRequestMasterDB.Add(prm);
                                }
                            }
                            context.Commit();
                        }
                        CreatePoOrInternalRes.Message = "PR Generated successfuly. PR # " + pm.PurchaseOrderId;
                        CreatePoOrInternalRes.Status = true;

                        var ctnam = context.Warehouses.Where(x => x.WarehouseId == pm.WarehouseId).Select(x => new { x.WarehouseName, x.Address, x.CityName, x.GSTin, x.Phone }).FirstOrDefault();

                        string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Invoice_Template.html");
                        string htmldata = "";
                        string replacetext = "";

                        if (File.Exists(path))
                        {
                            htmldata = File.ReadAllText(path);
                            if (!string.IsNullOrEmpty(htmldata))
                            {
                                replacetext = $"<span style='float:left'> <strong class='ng-binding'> Delivery at: {ctnam.WarehouseName} </strong></span><br>" +
                                $"<span style='float:left' class='ng-binding'>C/O: {ctnam.Address} </span><br>" +
                                $"<span style='float:left' class='ng-binding'>City:{ctnam.CityName}</span><br>" +
                                $"<span style='float:left' class='ng-binding'>Tel.No: {ctnam.Phone}</span><br>" +
                                $"<span style='float:left' class='ng-binding'>GSTIN: {ctnam.GSTin}</span>";
                                htmldata = htmldata.Replace("{{Deliveryat}}", replacetext);

                                replacetext = $"<span style = 'float:left' class='ng-binding'>Invoice:{pm.PoInvoiceNo}  </span> <br>" +
                                   $"<span style = 'float:left' class='ng-binding'>PO.No: {pm.PurchaseOrderId} </span> <br>" +
                                   $"<span style = 'float:left' class='ng-binding'>Date: {pm.CreationDate}</span> <br>" +
                                   $"<span style = 'float:left' class='ng-binding''>PO made by: {pm.CreatedBy}</span> <br>" +
                                   $"<span style = 'float:left' class='ng-binding'>Buyer: {Buyer.DisplayName}</span><br>" +
                                   $"<span style = 'float:left' class='ng-binding'>Picker Type: {PickerType}</span>";
                                htmldata = htmldata.Replace("{{InvoiceDetail}}", replacetext);

                                replacetext = $"<span style='float:left' class='ng-binding'>{supplier.Name}</span><br><br>" +
                                $"<span style='float:left' class='ng-binding'>C/O: {supplier.BillingAddress} </span><br>" +
                                $"<span style='float:left' class='ng-binding'>Tel.No: {supplier.MobileNo}</span><br>" +
                                $"<span style='float:left' class='ng-binding'>GSTIN: {supplier.GstInNumber}</span>";
                                htmldata = htmldata.Replace("{{SupplierDetail}}", replacetext);

                                replacetext = $"<span style='float:left' class='ng-binding'> {Depo.DepoName}</span><br>" +
                                $"<span style='float:left' class='ng-binding'>C/O:  {Depo.Address}</span><br>" +
                                $"<span style='float:left' class='ng-binding'>Tel.No: {Depo.OfficePhone}</span>&nbsp;<br>" +
                                $"<span style='float:left' class='ng-binding'>GSTIN: {Depo.GSTin}</span>";
                                htmldata = htmldata.Replace("{{DepoDetail}}", replacetext);

                                if (pm.PurchaseOrderDetail != null && pm.PurchaseOrderDetail.Any())
                                {
                                    string podetailhtml = "";
                                    double totalPrice = 0, totalWeight = 0;

                                    foreach (var item in pm.PurchaseOrderDetail)
                                    {
                                        var weightt = (item.WeightType == "Kg" ? item.Weight * item.TotalQuantity : (item.Weight * item.TotalQuantity) / 1000);

                                        podetailhtml += $"<tr >" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.PurchaseOrderDetailId}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.PurchaseSku}</td>" +
                                             //$"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'></td>" +
                                             $"<td width='300px' style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'><span style=' float:left;'>{item.ItemName}</span></td>" +
                                             //$"<td >{item.Category}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.HSNCode}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.Price}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.MOQ}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{((item.TotalQuantity) / (item.MOQ))}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{(item.MOQ * (item.TotalQuantity) / (item.MOQ))}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{item.Weight + " " + item.WeightType}</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'>{weightt}&nbsp;Kg</td>" +
                                             $"<td style='padding: 5px 15px; text - align: center; margin: 0px; min - width:100px; border: 1px solid #000 !important'><span style=' float:right;color: black;font-weight:bold; font-size:large;' >{(item.Price) * (item.TotalQuantity)}&nbsp;<i class='fa fa-inr'></i></span></td>" +
                                             $"</tr>";
                                        totalPrice += (item.Price) * (item.TotalQuantity);
                                        totalWeight += item.Weight.HasValue ? Convert.ToDouble(weightt) : 0;
                                    }
                                    if (!string.IsNullOrEmpty(podetailhtml))
                                    {
                                        podetailhtml += $"<tr> <td colspan='11'> <span style='float:left'><strong>Grand Total:</strong></span></td><td>" +
                                                        $"<span style=' float:right;color: black;font-weight:bold; font-size:large;' class='ng-binding'>{totalPrice}&nbsp;<i class='fa fa-inr'></i></span>" +
                                                        $"</td></tr><tr>" +
                                                        $"<td colspan='11'> <span style='float:left'><strong>Total Weight:</strong></span></td><td>" +
                                                        $"<span style=' float:right;color: black;font-weight:bold; font-size:large;' class='ng-binding'>{totalWeight}&nbsp;Kg </span>" +
                                                        $" </td></tr>";
                                    }
                                    htmldata = htmldata.Replace("{{POItemDetail}}", podetailhtml);
                                }


                            }
                        }


                        if (!string.IsNullOrEmpty(htmldata))
                        {
                            string fileUrl = "";
                            string fullPhysicalPath = "";
                            string thFileName = "";
                            string TartgetfolderPath = "";

                            TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\PDFForeCast");
                            if (!Directory.Exists(TartgetfolderPath))
                                Directory.CreateDirectory(TartgetfolderPath);


                            thFileName = "AutoPR_" + pm.PurchaseOrderId + "_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
                            fileUrl = "/PDFForeCast" + "/" + thFileName;
                            fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                            var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/PDFForeCast"), thFileName);

                            byte[] pdf = null;

                            pdf = Pdf
                                  .From(htmldata)
                                  //.WithGlobalSetting("orientation", "Landscape")
                                  //.WithObjectSetting("web.defaultEncoding", "utf-8")
                                  .OfSize(OpenHtmlToPdf.PaperSize.A4)
                                  .WithTitle("Invoice")
                                  .WithoutOutline()
                                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
                                  .Portrait()
                                  .Comressed()
                                  .Content();
                            FileStream file = File.Create(OutPutFile);
                            file.Write(pdf, 0, pdf.Length);
                            file.Close();


                            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                            string To = "", From = "", Bcc = "";
                            DataTable emaildatatable = new DataTable();

                            using (var connection = new SqlConnection(connectionString))
                            {
                                using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='PRRequest' and WarehouseId=" + pm.WarehouseId, connection))
                                {

                                    if (connection.State != ConnectionState.Open)
                                        connection.Open();

                                    SqlDataAdapter da = new SqlDataAdapter(command);

                                    da.Fill(emaildatatable);
                                    da.Dispose();
                                    connection.Close();
                                }
                            }
                            //logger.Info("Auto PR Checking before mail ");
                            if (emaildatatable.Rows.Count > 0)
                            {
                                To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                                From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                                Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                            }
                            string subject = DateTime.Now.ToString("dd MMM yyyy") + " Auto PR" + pm.PurchaseOrderId + " " + pm.WarehouseName; // 6 april
                            string message = "Please find the attached AutoPR Status";
                            if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            {
                                //logger.Info("Auto PR Checking ");
                                EmailHelper.SendMail(From, To, Bcc, subject, message, fullPhysicalPath);
                            }

                            else
                                logger.Error("Auto PR Invoice Receipt");

                        }

                    }
                    else if (pdList != null && pdList.Any() && ItemForecastPRRequests.Any(x => x.FulfillThrow != 1))
                    {

                        if (pdList.Select(x => x.InternalTransferWHId).Distinct().Count() > 1)
                        {
                            CreatePoOrInternalRes.Message = "Can't generated due to request contain two different warehouse items.";
                            return CreatePoOrInternalRes;
                        }

                        // for Internal
                        int RequestFromWarehouseId = pdList[0].WarehouseId; // Request
                        int RequestToWarehouseId = pdList[0].InternalTransferWHId;//Request full filler
                        var RequestFromWarehouse = context.Warehouses.Where(p => p.Deleted == false && p.WarehouseId == RequestFromWarehouseId && p.IsKPP == false && p.active == true).SingleOrDefault();
                        var RequestToWarehouse = context.Warehouses.Where(p => p.Deleted == false && p.WarehouseId == RequestToWarehouseId && p.IsKPP == false && p.active == true).SingleOrDefault();
                        TransferWHOrderMaster TOM = new TransferWHOrderMaster();
                        TOM.CreationDate = DateTime.Now;
                        TOM.WarehouseId = RequestFromWarehouse.WarehouseId;
                        TOM.CompanyId = 1;
                        TOM.WarehouseName = RequestFromWarehouse.WarehouseName + " (" + RequestFromWarehouse.CityName + ")";
                        TOM.Status = "Pending";
                        // TOM.Type = "Auto Internal";
                        TOM.RequestToWarehouseId = RequestToWarehouse.WarehouseId;
                        TOM.RequestToWarehouseName = RequestToWarehouse.WarehouseName + " (" + RequestToWarehouse.CityName + ")";
                        TOM.IsActivate = true;
                        TOM.IsDeleted = false;
                        TOM.UpdatedDate = DateTime.Now;
                        TOM.UpdateBy = "";
                        TOM.userid = userid;
                        TOM.CreatedBy = userid.ToString();
                        context.TransferWHOrderMasterDB.Add(TOM);
                        context.Commit();
                        TransferOrderHistory transferOrder = new TransferOrderHistory();
                        transferOrder.TransferOrderId = TOM.TransferOrderId;
                        transferOrder.Status = TOM.Status;
                        transferOrder.userid = TOM.userid;
                        if (people.DisplayName == null)
                        {
                            transferOrder.username = people.PeopleFirstName;
                        }
                        else
                        {
                            transferOrder.username = people.DisplayName;
                        }
                        transferOrder.CreatedDate = DateTime.Now;
                        context.TransferOrderHistoryDB.Add(transferOrder);
                        context.Commit();
                        var ItemMultiMrpIds = pdList.Select(x => x.ItemMultiMRPId).ToList();
                        var CurrentstockList = context.DbCurrentStock.Where(z => z.Deleted == false && ItemMultiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == RequestToWarehouseId).ToList();
                        var ItemNumberList = CurrentstockList.Select(x => x.ItemNumber).ToList();
                        var ItemList = context.itemMasters.Where(z => ItemNumberList.Contains(z.Number) && z.WarehouseId == RequestToWarehouse.WarehouseId).ToList();
                        List<TransferWHOrderDetails> AddTransferWHOrderDetails = new List<TransferWHOrderDetails>();
                        foreach (var tItem in pdList)
                        {
                            var stock = CurrentstockList.Where(z => z.ItemMultiMRPId == tItem.ItemMultiMRPId).FirstOrDefault();
                            TransferWHOrderDetails pd = new TransferWHOrderDetails();
                            pd.TransferOrderId = TOM.TransferOrderId;
                            pd.itemname = stock.itemname;
                            pd.ItemMultiMRPId = stock.ItemMultiMRPId;
                            pd.ItemNumber = stock.ItemNumber;
                            pd.TotalTaxPercentage = tItem.TotalTaxPercentage;
                            pd.TotalQuantity = tItem.FinalQty;
                            pd.PriceofItem = (pd.TotalQuantity) * ((tItem.NetPurchasePrice ?? 0) > 0 ? tItem.NetPurchasePrice ?? 0 : tItem.MRP);
                            pd.WarehouseId = RequestFromWarehouse.WarehouseId; //request from warehouse
                            pd.CreationDate = DateTime.Now;
                            pd.Status = "Pending";
                            //pd.Type= "Auto Internal";
                            pd.RequestToWarehouseId = stock.WarehouseId;

                            pd.itemBaseName = stock.itemBaseName;
                            pd.ItemHsn = tItem.HSNCode;
                            pd.NPP = tItem.NetPurchasePrice;
                            pd.MRP = stock.MRP;
                            pd.UnitofQuantity = stock.UnitofQuantity;
                            pd.UOM = stock.UOM;
                            pd.CompanyId = 1;
                            pd.WarehouseName = RequestFromWarehouse.WarehouseName;
                            pd.RequestToWarehouseName = RequestToWarehouse.WarehouseName;
                            pd.UpdatedDate = DateTime.Now;
                            var manager = new ItemLedgerManager();
                            List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                            ItemClassificationDC obja = new ItemClassificationDC();
                            obja.WarehouseId = tItem.WarehouseId;
                            obja.ItemNumber = stock.ItemNumber;
                            objItemClassificationDClist.Add(obja);
                            List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                            pd.ABCClassification = _objItemClassificationDClist.Count == 0 ? "D" : _objItemClassificationDClist[0].Category;
                            AddTransferWHOrderDetails.Add(pd);
                        }

                        if (AddTransferWHOrderDetails != null && AddTransferWHOrderDetails.Any())
                        {
                            context.TransferWHOrderDetailsDB.AddRange(AddTransferWHOrderDetails);
                            if (context.Commit() > 0)
                            {
                                ItemForecastPRRequests.ForEach(x =>
                                {
                                    x.FulfillobjectId = transferOrder.TransferOrderId;
                                });
                                var updatingPRRequestsids = pdList.Select(x => x.Id).ToList();
                                foreach (var item in ItemForecastPRRequests.Where(x => updatingPRRequestsids.Contains(x.Id)))
                                {
                                    item.ModifiedDate = DateTime.Now;
                                    item.ModifiedBy = people.PeopleID;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                context.Commit();
                                CreatePoOrInternalRes.Message = "Transfer Order Generated successfuly. No. # " + transferOrder.TransferOrderId;
                                CreatePoOrInternalRes.Status = true;
                            }
                        }
                    }
                }
                else
                {
                    CreatePoOrInternalRes.Message = "There is no request";
                }
                // }
            }
            return CreatePoOrInternalRes;
        }
        #endregion
        public void ExportToPdf(DataTable dt, string strFilePath)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(strFilePath, FileMode.Create));
            document.Open();
            iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 5);

            PdfPTable table = new PdfPTable(dt.Columns.Count);
            PdfPRow row = null;
            float[] widths = new float[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
                widths[i] = 4f;

            table.SetWidths(widths);

            table.WidthPercentage = 100;
            int iCol = 0;
            string colname = "";
            PdfPCell cell = new PdfPCell(new Phrase("Products"));

            cell.Colspan = dt.Columns.Count;

            foreach (DataColumn c in dt.Columns)
            {
                table.AddCell(new Phrase(c.ColumnName, font5));
            }

            foreach (DataRow r in dt.Rows)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int h = 0; h < dt.Columns.Count; h++)
                    {
                        table.AddCell(new Phrase(r[h].ToString(), font5));
                    }
                }
            }
            document.Add(table);
            document.Close();
        }
        [Route("CalculateSalesIntendOrder")]
        [HttpGet]
        public async Task<bool> CalculateSalesIntendOrder()
        {
            using (AuthContext context = new AuthContext())
            {
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var EndDate = startDate.AddMonths(1).AddMilliseconds(-1);
                //var salesIntents = await context.SalesIntentRequestDb.Where(x => x.Status == 3 && x.CreatedDate >= startDate && x.CreatedDate <= EndDate
                //&& x.ItemForecastDetailId.HasValue && x.ItemForecastDetailId.Value > 0 && (!x.TotalOrderQty.HasValue || x.RequestQty > x.TotalOrderQty)).ToListAsync();


                var startDatein = new SqlParameter("@startDate", startDate);
                var EndDatein = new SqlParameter("@EndDate", EndDate);
                var salesIntents = context.Database.SqlQuery<SalesIntentDc>("Exec sp_SalesIndentRequstCheck @startDate,@EndDate ", startDatein, EndDatein).ToList();
                if (salesIntents != null && salesIntents.Any())
                {
                    var minDate = salesIntents.Min(x => x.BuyerApprovedDate);
                    var itemMultiMrpIds = salesIntents.Select(x => new { x.Warehouseid, x.ItemMultiMRPId, x.PeopleId }).Distinct().ToList();
                    var peopeleIds = salesIntents.Select(x => x.PeopleId).Distinct().ToList();
                    var peopleClusters = context.ClusterStoreExecutives.Where(x => peopeleIds.Contains(x.ExecutiveId) && x.IsActive);
                    List<OrderItemForIntent> itemForeCastPODataes = new List<OrderItemForIntent>();
                    using (AuthContext authcontext = new AuthContext())
                    {
                        if (authcontext.Database.Connection.State != ConnectionState.Open)
                            authcontext.Database.Connection.Open();

                        var IdDt = new DataTable();
                        IdDt.Columns.Add("ItemMultiMRPId");
                        IdDt.Columns.Add("WarehouseId");
                        IdDt.Columns.Add("PeopleId");
                        foreach (var item in itemMultiMrpIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                            dr["WarehouseId"] = item.Warehouseid;
                            dr["PeopleId"] = item.PeopleId;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("item", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.SalesIntentitemtype";

                        var cmd = authcontext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetOrderDetailForSalesIntent]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@startDate", minDate));
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        itemForeCastPODataes = ((IObjectContextAdapter)authcontext)
                       .ObjectContext
                       .Translate<OrderItemForIntent>(reader).ToList();
                    }
                    if (itemForeCastPODataes != null && itemForeCastPODataes.Any())
                    {
                        foreach (var item in salesIntents)
                        {
                            if (!item.TotalOrderQty.HasValue)
                                item.TotalOrderQty = 0;
                            var isModified = false;
                            var clusterId = peopleClusters.FirstOrDefault(x => x.ExecutiveId == item.PeopleId)?.ClusterId;
                            if (clusterId.HasValue)
                            {
                                foreach (var order in itemForeCastPODataes.Where(x => x.ClusterId == clusterId.Value
                                                       && x.ItemMultiMRPId == item.ItemMultiMRPId && x.qty != 0 && x.CreatedDate >= item.BuyerApprovedDate))
                                {
                                    if (item.RequestQty > item.TotalOrderQty)
                                    {
                                        if (order.qty > (item.RequestQty - item.TotalOrderQty.Value))
                                        {
                                            item.TotalOrderQty += (item.RequestQty - item.TotalOrderQty.Value);
                                            order.qty -= (item.RequestQty - item.TotalOrderQty.Value);
                                            item.OrderId += string.IsNullOrEmpty(item.OrderId) ? order.orderid.ToString() : "," + order.orderid.ToString();
                                        }
                                        else
                                        {
                                            item.TotalOrderQty += order.qty;
                                            order.qty = 0;
                                            item.OrderId += string.IsNullOrEmpty(item.OrderId) ? order.orderid.ToString() : "," + order.orderid.ToString();
                                        }
                                        isModified = true;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (isModified)
                            {
                                var itemforecast = context.ItemForecastDetailDb.FirstOrDefault(x => x.Id == item.ItemForecastDetailId);
                                itemforecast.SalesIntent -= item.TotalOrderQty.Value;
                                itemforecast.ModifiedDate = DateTime.Now;
                                item.ModifiedDate = DateTime.Now;
                                context.Entry(itemforecast).State = EntityState.Modified;
                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        context.Commit();
                    }
                }

            }

            return true;
        }

        [Route("GetForcastCityByPeople")]
        [HttpGet]

        public List<GetForcastCityByPeople> GetForcastcity(int PeopleId)
        {
            using (var myContext = new AuthContext())
            {
                List<GetForcastCityByPeople> CityList = new List<GetForcastCityByPeople>();
                if (PeopleId > 0)
                {
                    string query = "EXEC GetForcastCityByPeopleId" + PeopleId;
                    CityList = myContext.Database.SqlQuery<GetForcastCityByPeople>(query).ToList();
                }
                return CityList;

            }
        }

        [Route("GetForcastCategorybyPeopleId")]
        [HttpGet]
        public List<ForcastCategoryDc> GetForcastCategory(GetForcastCategorybyPeopleDc GetForcastCategory)
        {
            List<ForcastCategoryDc> ForeCastPeopleResponses = new List<ForcastCategoryDc>();
            if (GetForcastCategory != null)
            {
                using (var authContext = new AuthContext())
                {
                    var IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in GetForcastCategory.warehouseIds)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var WarehouseIds = new SqlParameter
                    {
                        ParameterName = "@WarehouseIds",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = IdDt
                    };
                    var BuyerId = new SqlParameter("@BuyerId", GetForcastCategory.BuyerId);
                    var RoleName = new SqlParameter("@RoleName", GetForcastCategory.RoleName);
                    ForeCastPeopleResponses = authContext.Database.SqlQuery<ForcastCategoryDc>("EXEC GetForcastCategorybyPeopleId @WarehouseIds,@BuyerId,@RoleName", WarehouseIds, BuyerId, RoleName).ToList();
                }
            }
            return ForeCastPeopleResponses;
        }



        [Route("GetWarehouse")]
        [HttpGet]
        public List<Warehouse> GetWarehouse(int? cityId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string RoleNames = "";
                string Warehouseids = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                    Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;


                var wIds = context.GMWarehouseProgressDB.Where(x => x.IsLaunched).Select(x => x.WarehouseID).ToList();


                if (cityId.HasValue == false)
                {
                    if (RoleNames.Split(',').ToList().Contains("HQ Master login"))
                    {

                        ass = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        return ass;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Warehouseids))
                        {
                            List<int> assWIds = Warehouseids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            ass = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && assWIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        }
                        return ass;
                    }
                }
                else
                {
                    if (RoleNames.Split(',').ToList().Contains("HQ Master login"))
                    {

                        ass = context.Warehouses.Where(p => p.Deleted == false && p.Cityid == cityId.Value && wIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        return ass;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Warehouseids))
                        {
                            List<int> assWIds = Warehouseids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            ass = context.Warehouses.Where(p => p.Deleted == false && p.Cityid == cityId.Value && wIds.Contains(p.WarehouseId) && assWIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        }
                        return ass;
                    }
                }

            }
        }


        [Route("GetWarehouseMulti")]
        [HttpPost]
        public List<Warehouse> GetWarehouse(List<int> cityIds)
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> ass = new List<Warehouse>();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string RoleNames = "";
                string Warehouseids = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                    Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;


                var wIds = context.GMWarehouseProgressDB.Where(x => x.IsLaunched).Select(x => x.WarehouseID).ToList();


                if (!(cityIds != null && cityIds.Any()))
                {
                    if (RoleNames.Split(',').ToList().Contains("HQ Master login"))
                    {

                        ass = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        return ass;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Warehouseids))
                        {
                            List<int> assWIds = Warehouseids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            ass = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && assWIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        }
                        return ass;
                    }
                }
                else
                {
                    if (RoleNames.Split(',').ToList().Contains("HQ Master login"))
                    {

                        ass = context.Warehouses.Where(p => p.Deleted == false && cityIds.Contains(p.Cityid) && wIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        return ass;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Warehouseids))
                        {
                            List<int> assWIds = Warehouseids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            ass = context.Warehouses.Where(p => p.Deleted == false && cityIds.Contains(p.Cityid) && wIds.Contains(p.WarehouseId) && assWIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).ToList();
                        }
                        return ass;
                    }
                }

            }
        }

        [Route("GetCities")]
        [HttpGet]
        public List<GetForcastCityByPeople> GetCities()
        {
            List<GetForcastCityByPeople> GetForcastCityByPeoples = new List<GetForcastCityByPeople>();

            using (AuthContext context = new AuthContext())
            {
                //List<GetForcastCityByPeople> ass = new List<GetForcastCityByPeople>();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string RoleNames = "";
                string Warehouseids = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                    Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;


                var wIds = context.GMWarehouseProgressDB.Where(x => x.IsLaunched).Select(x => x.WarehouseID).ToList();


                if (RoleNames.Split(',').ToList().Contains("HQ Master login"))
                {

                    GetForcastCityByPeoples = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).Select(x => new GetForcastCityByPeople { Cityid = x.Cityid, CityName = x.CityName }).Distinct().OrderBy(x => x.CityName).ToList();

                }
                else
                {
                    if (!string.IsNullOrEmpty(Warehouseids))
                    {
                        List<int> assWIds = Warehouseids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                        GetForcastCityByPeoples = context.Warehouses.Where(p => p.Deleted == false && wIds.Contains(p.WarehouseId) && assWIds.Contains(p.WarehouseId) && (p.IsKPP == false || p.IsKppShowAsWH == true) && p.active == true).Select(x => new GetForcastCityByPeople { Cityid = x.Cityid, CityName = x.CityName }).Distinct().OrderBy(x => x.CityName).ToList();
                    }
                }
                // GetForcastCityByPeoples = ass.Select(x => new GetForcastCityByPeople { Cityid = x.Cityid, CityName = x.CityName }).ToList();                
            }
            return GetForcastCityByPeoples;
        }

        [Route("AllCategories")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<DataContracts.Masters.CategoryMinDc>> AllCategories()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string Warehouseids = "", RoleNames = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

            if (!(RoleNames.Split(',').ToList().Contains("HQ Master login") || RoleNames.Split(',').ToList().Contains("Inventory Forecasting Sales Senior Executive") || RoleNames.Split(',').ToList().Contains("CM5 lead")))
            {
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in Warehouseids.Split(','))
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("WarehouseIds", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetBrandbyPeopleId]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@BuyerId", userid));

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    List<ForcastPeopleMaster> forcastPeopleMaster = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<ForcastPeopleMaster>(reader).ToList();

                    List<DataContracts.Masters.CategoryMinDc> categoryMinDcs = forcastPeopleMaster.GroupBy(y => new { y.Categoryid, y.CategoryName }).Select(x => new DataContracts.Masters.CategoryMinDc { CategoryId = x.Key.Categoryid, CategoryName = x.Key.CategoryName }).OrderBy(x => x.CategoryName).ToList();
                    return categoryMinDcs;
                }
            }
            else
            {
                MastersManager manager = new MastersManager();
                return await manager.AllCategories();
            }
        }

        [Authorize]
        [Route("GetSubCategories")]
        [HttpPost]
        public async Task<List<DataContracts.Masters.SubCategoryMinDc>> SubCategories(List<int> categoryId)
        {
            List<DataContracts.Masters.SubCategoryMinDc> subcategoryMinDcs = new List<DataContracts.Masters.SubCategoryMinDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string Warehouseids = "", RoleNames = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

            if (!(RoleNames.Split(',').ToList().Contains("HQ Master login") || RoleNames.Split(',').ToList().Contains("Inventory Forecasting Sales Senior Executive") || RoleNames.Split(',').ToList().Contains("CM5 lead")))
            {
                if (categoryId != null && categoryId.Any())
                {
                    using (var authContext = new AuthContext())
                    {
                        if (authContext.Database.Connection.State != ConnectionState.Open)
                            authContext.Database.Connection.Open();

                        var IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");
                        foreach (var item in Warehouseids.Split(','))
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("WarehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetBrandbyPeopleId]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(new SqlParameter("@BuyerId", userid));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        List<ForcastPeopleMaster> forcastPeopleMaster = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<ForcastPeopleMaster>(reader).ToList();

                        subcategoryMinDcs = forcastPeopleMaster.Where(x => categoryId.Contains(x.Categoryid)).GroupBy(x => new { x.SubCategoryId, x.SubcategoryName }).Select(x => new DataContracts.Masters.SubCategoryMinDc { SubCategoryId = x.Key.SubCategoryId, SubCategoryName = x.Key.SubcategoryName }).OrderBy(x => x.SubCategoryName).ToList(); //changes at 26 dec
                    }
                }
                return subcategoryMinDcs;
            }
            else
            {
                MastersManager manager = new MastersManager();
                return await manager.SubCategories(categoryId);
            }
        }

        [Authorize]
        [Route("GetBrands")]
        [HttpPost]
        public async Task<List<DataContracts.Masters.BrandMinDc>> GetBrands(GetBrandsDc getBrandsDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string Warehouseids = "", RoleNames = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids"))
                Warehouseids = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;
            if (!(RoleNames.Split(',').ToList().Contains("HQ Master login") || RoleNames.Split(',').ToList().Contains("Inventory Forecasting Sales Senior Executive")|| RoleNames.Split(',').ToList().Contains("CM5 lead")))
            {
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();
               
                    var IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in Warehouseids.Split(','))
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("WarehouseIds", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetBrandbyPeopleId]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@BuyerId", userid));

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    List<ForcastPeopleMaster> forcastPeopleMaster = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<ForcastPeopleMaster>(reader).ToList();

                    // List<DataContracts.Masters.BrandMinDc> brandyMinDcs = forcastPeopleMaster.Where(x => getBrandsDc.categoryId.Contains(x.Categoryid) && getBrandsDc.subcategoryId.Contains(x.SubCategoryId)).Select(x => new DataContracts.Masters.BrandMinDc { BrandId = x.SubsubCategoryid, BrandName = x.SubsubcategoryName }).Distinct().ToList();
                    List<DataContracts.Masters.BrandMinDc> brandyMinDcs = forcastPeopleMaster.Where(x => getBrandsDc.categoryId.Contains(x.Categoryid) && getBrandsDc.subcategoryId.Contains(x.SubCategoryId)).GroupBy(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).Select(x => new DataContracts.Masters.BrandMinDc { BrandId = x.Key.SubsubCategoryid, BrandName = x.Key.SubsubcategoryName }).ToList();
                    return brandyMinDcs;
                }
            }
            else
            {
                MastersManager manager = new MastersManager();
                List<DataContracts.Masters.BrandMinDc> brandyMinDcs = await manager.SubSubCategories(getBrandsDc.categoryId, getBrandsDc.subcategoryId);
                return brandyMinDcs;
            }
        }




        [Authorize]
        [Route("GetBrandsForFullfillment")]
        [HttpPost]
        public async Task<List<DataContracts.Masters.BrandMinDc>> GetBrandsForFullfillment(GetBrandsDc getBrandsDc)
        {
            MastersManager manager = new MastersManager();
            List<DataContracts.Masters.BrandMinDc> brandyMinDcs = await manager.SubSubCategories(getBrandsDc.categoryId, getBrandsDc.subcategoryId);
            return brandyMinDcs;
        }

        [HttpPost]
        [Route("GetItemForecastInventoryDays")]
        public async Task<ItemForecastInventoryDaysEdit> GetItemForecastInventoryDays(ItemForecastInventoryDays ItemForecastInventoryDays)
        {
            ItemForecastInventoryDaysEdit itemforcastDaysEdit = new ItemForecastInventoryDaysEdit();
            List<ItemForecastInventoryDaysResponse> ItemForecastInventoryDaysResponses = new List<ItemForecastInventoryDaysResponse>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var IdDt = new DataTable();
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in ItemForecastInventoryDays.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in ItemForecastInventoryDays.StoreIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("@StoreIds", IdDt);
                param2.SqlDbType = SqlDbType.Structured;
                param2.TypeName = "dbo.IntValues";
                int Skiplist = (ItemForecastInventoryDays.skip - 1) * ItemForecastInventoryDays.take;
                var cmd = authContext.Database.Connection.CreateCommand();
                // cmd.CommandText = "[dbo].[ItemForecastInventoryDays]"; previous
                //cmd.CommandText = "[dbo].[ForecastInventoryDaysRestriction]";
                cmd.CommandText = "[dbo].[GetRestrictionInventoryDays]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(new SqlParameter("@BrandName", ItemForecastInventoryDays.BrandName));
                cmd.Parameters.Add(new SqlParameter("@skip", Skiplist));
                cmd.Parameters.Add(new SqlParameter("@take", ItemForecastInventoryDays.take));
                cmd.CommandTimeout = 1200;
                var reader = cmd.ExecuteReader();
                ItemForecastInventoryDaysResponses = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForecastInventoryDaysResponse>(reader).ToList();
                reader.NextResult();
                itemforcastDaysEdit.ItemForecastInventoryDaysResponses = ItemForecastInventoryDaysResponses;
                itemforcastDaysEdit.Total_Record = ItemForecastInventoryDaysResponses.Select(x => x.Total_Record).FirstOrDefault();
                return itemforcastDaysEdit;

            }
        }

        [HttpGet]
        [Route("GetGroupNames")]
        public List<Groupnames> getGroupName()
        {
            using (var authContext = new AuthContext())
            {
                List<Groupnames> GetGroupnames = new List<Groupnames>();

                var GetGroupname = authContext.StoreDB.Where(p => p.IsDeleted == false && p.IsActive == true).Select(
                    x => new Groupnames
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).Distinct().ToList();

                return GetGroupname;
            }
        }

        [HttpPost]
        [Route("ItemfullfillmentComment")]
        public ItemFullfillmentComment Fullfillmentcomment(int Id, string comments, int SupplierId)
        {
            using (var authContext = new AuthContext())
            {

                var itemcomments = authContext.ItemFullfillmentCommentDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.ItemforecastDetailId == Id).FirstOrDefault();
                if (itemcomments != null)
                {
                    itemcomments.IsActive = false;
                    itemcomments.IsDeleted = true;
                    itemcomments.ModifiedBy = SupplierId;
                    itemcomments.ModifiedDate = DateTime.Now;

                    authContext.Entry(itemcomments).State = EntityState.Modified;
                    authContext.Commit();

                    itemcomments.IsActive = true;
                    itemcomments.IsDeleted = false;
                    itemcomments.Comments = comments;
                    itemcomments.CreatedBy = SupplierId;
                    itemcomments.CreatedDate = DateTime.Now;
                    authContext.ItemFullfillmentCommentDB.Add(itemcomments);
                    authContext.Commit();
                    return itemcomments;
                }
                else
                {
                    ItemFullfillmentComment itemcomment = new ItemFullfillmentComment();
                    itemcomment.ItemforecastDetailId = Id;
                    itemcomment.Comments = comments;

                    itemcomment.IsActive = true;
                    itemcomment.IsDeleted = false;
                    itemcomment.CreatedBy = SupplierId;
                    itemcomment.CreatedDate = DateTime.Now;
                    authContext.ItemFullfillmentCommentDB.Add(itemcomment);
                    authContext.Commit();
                    return itemcomment;
                }



            }


        }
        [HttpPost]
        [Route("GetItemForeCastForPODetail")]
        public async Task<ItemforcastPOEdit> GetItemForeCastForPODetail(ItemFullfillmentRequest itemForeCastRequest)
        {
            ItemforcastPOEdit itemforcastPOEdit = new ItemforcastPOEdit();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            List<ItemForeCastPOResponse> itemForeCastPOResponses = new List<ItemForeCastPOResponse>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("categoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param4 = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                //            var param5 = new SqlParameter("@NetStock", itemForeCastRequest.NetStock);
                var NetStockDt = new DataTable();
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                NetStockDt = new DataTable();
                NetStockDt.Columns.Add("IntValue");

                if ((itemForeCastRequest.NetStock.Length) > 0)
                {
                    var dr = NetStockDt.NewRow();
                    if (itemForeCastRequest.NetStock == "planned")
                    { dr["IntValue"] = 1; }
                    if (itemForeCastRequest.NetStock == "demand")
                    { dr["IntValue"] = 2; }
                    if (itemForeCastRequest.NetStock == "all")
                    {
                        dr["IntValue"] = 0;
                    }

                    NetStockDt.Rows.Add(dr);
                }


                var param5 = new SqlParameter("NetStock", NetStockDt);



                var param6 = new SqlParameter("@SearchItem", itemForeCastRequest.SearchItem);
                var param7 = new SqlParameter("@userid", userid);

                var cmd = authContext.Database.Connection.CreateCommand();
                //cmd.CommandText = "[dbo].[GetItemForeCastForPODetail]";

                cmd.CommandText = "[dbo].[GetItemForeCastForPODetailwithtime]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);
                cmd.Parameters.Add(param7);
                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                cmd.CommandTimeout = 300; // for Execute Error
                                          // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ItemForeCastPOData> itemForeCastPODataes = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastPOData>(reader).ToList();
                itemForeCastPOResponses = itemForeCastPODataes.GroupBy(x => x.Id).Select(x => new ItemForeCastPOResponse
                {
                    Id = x.Key,
                    AveragePurchasePrice = x.FirstOrDefault().AveragePurchasePrice,
                    BuyingPrice = x.FirstOrDefault().BuyingPrice,
                    CityId = x.FirstOrDefault().CityId,
                    CityName = x.FirstOrDefault().CityName,
                    CurrentStock = x.FirstOrDefault().CurrentStock,
                    FulfillThrow = x.FirstOrDefault().FulfillThrow,
                    ItemMultiMrpId = x.FirstOrDefault().ItemMultiMrpId,
                    ItemName = x.FirstOrDefault().ItemName,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    MRP = x.FirstOrDefault().MRP,
                    NetStock = x.FirstOrDefault().NetStock,
                    NoOfSet = x.FirstOrDefault().NoOfSet,
                    PRPaymentType = x.FirstOrDefault().PRPaymentType,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    RequiredQty = x.FirstOrDefault().RequiredQty,
                    SubsubcategoryName = x.FirstOrDefault().SubsubcategoryName,
                    SupplierId = x.FirstOrDefault().SupplierId,
                    WarehouseName = x.FirstOrDefault().WarehouseName,
                    YesterdayDemand = x.FirstOrDefault().YesterdayDemand,
                    SalesIntent = x.FirstOrDefault().SalesIntent,
                    Comments = x.FirstOrDefault().Comments, //changes for comments
                    DisplayName = x.FirstOrDefault().DisplayName,
                    PrDate = x.FirstOrDefault().PrDate,
                    CreatedBy = x.FirstOrDefault().CreatedBy,
                    PRId = x.FirstOrDefault().PRId,
                    Categoryid = x.FirstOrDefault().Categoryid,
                    SubCategoryId = x.FirstOrDefault().SubCategoryId,
                    SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                    OPenQty = x.FirstOrDefault().OPenQty,//changes for OPenQty
                    OtherWarehouseDetails = x.Select(y => new OtherWarehouseDetails
                    {
                        OtherWarehouseId = y.OtherWarehouseId,
                        OtherWhDelCancel = y.OtherWhDelCancel,
                        OtherWhDemand = y.OtherWhDemand,
                        OtherWhNetDemand = y.OtherWhNetDemand,
                        OtherWhOpenPoQty = y.OtherWhOpenPoQty,
                        OtherWhStock = y.OtherWhStock,
                        OtherWarehouseName = y.OtherWarehouseNM,
                        otherWhReqQty = y.otherWhReqQty,
                    }).ToList()
                }).ToList();
                itemforcastPOEdit.ItemForeCastPOResponses = itemForeCastPOResponses;
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["TotalCount"] is DBNull))
                    {
                        itemforcastPOEdit.TotalRecord = 0;
                    }

                    else
                    {
                        itemforcastPOEdit.TotalRecord = Convert.ToInt32(reader["TotalCount"]);

                    }


                }
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["SaveasdraftCount"] is DBNull))
                    {
                        itemforcastPOEdit.SaveasdraftCount = 0;
                    }

                    else
                    {
                        itemforcastPOEdit.SaveasdraftCount = Convert.ToInt32(reader["SaveasdraftCount"]);

                    }


                }

                if (itemForeCastPOResponses != null && itemForeCastPOResponses.Any())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = itemForeCastPOResponses.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMrpId }).ToList();
                    var list = await tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null && list.Any())
                    {
                        foreach (var da in itemforcastPOEdit.ItemForeCastPOResponses)
                        {
                            da.Tag = list.Where(x => x.ItemMultiMRPId == da.ItemMultiMrpId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }

                }
                return itemforcastPOEdit;
            }
        }


        [HttpPost]
        [Route("GetItemFullfillmentExport")]
        public async Task<ItemforcastfullfillmetExport> GetItemFullfillmentExport(ItemFullfillmentRequest itemForeCastRequest)
        {
            ItemforcastfullfillmetExport itemforcastPOEdit = new ItemforcastfullfillmetExport();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            List<ItemForeCastfullfillmetExport> itemForeCastPOResponses = new List<ItemForeCastfullfillmetExport>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("categoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param4 = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var NetStockDt = new DataTable();
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                NetStockDt = new DataTable();
                NetStockDt.Columns.Add("IntValue");

                if ((itemForeCastRequest.NetStock.Length) > 0)
                {
                    var dr = NetStockDt.NewRow();
                    if (itemForeCastRequest.NetStock == "planned")
                    { dr["IntValue"] = 1; }
                    if (itemForeCastRequest.NetStock == "demand")
                    { dr["IntValue"] = 2; }
                    if (itemForeCastRequest.NetStock == "all")
                    {
                        dr["IntValue"] = 0;
                    }

                    NetStockDt.Rows.Add(dr);
                }


                var param5 = new SqlParameter("NetStock", NetStockDt);



                var param6 = new SqlParameter("@SearchItem", itemForeCastRequest.SearchItem);
                var param7 = new SqlParameter("@userid", userid);

                var cmd = authContext.Database.Connection.CreateCommand();


                cmd.CommandText = "[dbo].[GetItemFullfillmentExport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);
                cmd.Parameters.Add(param6);
                cmd.Parameters.Add(param7);
                cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                cmd.CommandTimeout = 300; // for Execute Error
                                           // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ItemForeCastPODataExcel> itemForeCastPODataes = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastPODataExcel>(reader).ToList();
                itemForeCastPOResponses = itemForeCastPODataes.GroupBy(x => x.Id).Select(x => new ItemForeCastfullfillmetExport
                {
                    Id = x.Key,
                    AveragePurchasePrice = x.FirstOrDefault().AveragePurchasePrice,
                    CurrentStock = x.FirstOrDefault().CurrentStock,
                    ItemMultiMrpId = x.FirstOrDefault().ItemMultiMrpId,
                    ItemName = x.FirstOrDefault().ItemName,
                    PurchaseMOQ = x.FirstOrDefault().PurchaseMOQ,
                    MRP = x.FirstOrDefault().MRP,
                    NetStock = x.FirstOrDefault().NetStock,
                    NoOfSet = x.FirstOrDefault().NoOfSet,
                    ExecutedQty = x.FirstOrDefault().ExecutedQty,
                    PRPaymentType = x.FirstOrDefault().PRPaymentType,
                    AllowedQty = x.FirstOrDefault().AllowedQty,
                    BrandName = x.FirstOrDefault().BrandName,
                    WarehouseName = x.FirstOrDefault().WarehouseName,
                    YesterdayDemand = x.FirstOrDefault().YesterdayDemand,
                    CatalogerName = x.FirstOrDefault().CatalogerName,
                    PickerType = x.FirstOrDefault().PickerType,
                    ETADate = x.FirstOrDefault().ETADate,
                    PrDate = x.FirstOrDefault().PrDate,
                    PRId = x.FirstOrDefault().PRId,
                    Supplier = x.FirstOrDefault().Supplier,
                    DepoName = x.FirstOrDefault().DepoName,
                    BuyerName = x.FirstOrDefault().BuyerName,
                    Comments = x.FirstOrDefault().Comments,
                }).ToList();
                itemforcastPOEdit.ItemForeCastPOResponses = itemForeCastPOResponses;
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["TotalCount"] is DBNull))
                    {
                        itemforcastPOEdit.TotalRecord = 0;
                    }

                    else
                    {
                        itemforcastPOEdit.TotalRecord = Convert.ToInt32(reader["TotalCount"]);

                    }
                }
                reader.NextResult();
                if (reader.Read())
                {
                    if ((reader["SaveasdraftCount"] is DBNull))
                    {
                        itemforcastPOEdit.SaveasdraftCount = 0;
                    }
                    else
                    {
                        itemforcastPOEdit.SaveasdraftCount = Convert.ToInt32(reader["SaveasdraftCount"]);

                    }
                }
                return itemforcastPOEdit;
            }
        }

        [Route("UpdateItemFulFillRequestBulkItem")]
        [HttpPost]
        public async Task<ItemForeCastUpdateResponse> UpdateItemFulFillRequestBulkItem(ItemForecastPRRequestForBulkDc itemRequestDc)
        {
            ItemForeCastUpdateResponse response = new ItemForeCastUpdateResponse();
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {

                List<ItemForecastPRRequest> itemForecastPRRequest = new List<ItemForecastPRRequest>();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                // List<ItemForecastPRRequestForBulkDc> itemRequestDc = new List<ItemForecastPRRequestForBulkDc>();
                var forecastIds = itemRequestDc.ItemForecastPRRequestForBulkobj.Select(x => x.ItemForecastDetailId).ToList();
                if (forecastIds.Any())
                {
                    var itemForecastDetails = await context.ItemForecastDetailDb.Where(x => forecastIds.Contains(x.Id)).ToListAsync();
                    
                    foreach (var item in itemRequestDc.ItemForecastPRRequestForBulkobj)
                    {
                        PurchaseOrderNewController cntlr = new PurchaseOrderNewController();
                        var res = await cntlr.PoCheckbySubcatId(item.Warehouseid.Value, 0, item.SubCategoryId.Value,item.SubsubCategoryId.Value,item.MultiMrpId);
                        if(res !=null && res.StopPo)
                        {
                            response.Status = false;
                            response.msg = res.CompanyBrand;
                            return response;
                        }
                        var itemForecastDetail = itemForecastDetails.FirstOrDefault(x => x.Id == item.ItemForecastDetailId);
                        if (itemForecastDetail != null)
                        {

                            double requireQty = 0;
                            double otherhubReqQty = 0;
                            int NoOfSet = 0;
                            int PurchaseMoq = 0;
                            int FinalQty = 0;

                            NoOfSet = item.NoOfSet;
                            PurchaseMoq = item.MinOrderQty;
                            FinalQty = item.NoOfSet * item.MinOrderQty;

                            if (NoOfSet == 0)
                            {
                                response.msg = "No of Set should not be zero!";
                                return response;
                            }

                            if (PurchaseMoq == 0)
                            {
                                response.msg = "Purchase MOQ should not be zero!";
                                return response;
                            }

                            if (FinalQty == 0)
                            {
                                response.msg = "Final Qty should not be zero!";
                                return response;
                            }

                            var param = new SqlParameter("@itemforcastId", itemForecastDetail.Id);
                            var cmd = context.Database.Connection.CreateCommand();
                            //cmd.CommandText = "[dbo].[GetItemForeCastForPODetail]";

                            cmd.CommandText = "[dbo].[GetItemForeCastOtherhubRequiredQty]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);
                            var reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                if ((reader["requireQty"] is DBNull))
                                {
                                    requireQty = 0;
                                }

                                else
                                {
                                    requireQty = Convert.ToDouble(reader["requireQty"]);

                                }


                            }
                            reader.NextResult();
                            if (reader.Read())
                            {
                                if ((reader["otherhubReqQty"] is DBNull))
                                {
                                    otherhubReqQty = 0;
                                }

                                else
                                {
                                    otherhubReqQty = Convert.ToDouble(reader["otherhubReqQty"]);

                                }


                            }

                            //requireQty = context.Database.SqlQuery<double>("Exec GetItemForeCastRequiredQty " + itemForecastDetail.Id).FirstOrDefault();//case 1 
                            //var id = new SqlParameter("@itemforcastId", itemForecastDetail.Id);
                            //var wid = new SqlParameter("@warehouseid", item.Warehouseid);
                            //otherhubReqQty = context.Database.SqlQuery<double>("Exec GetItemForeCastOtherhubRequiredQty @itemforcastId,@warehouseid", id, wid).FirstOrDefault(); //case 2 
                            var reqcase = Math.Ceiling((requireQty / item.MinOrderQty));
                            var otherreqcase = Math.Ceiling((otherhubReqQty / item.MinOrderQty));
                            var sum = reqcase + otherreqcase;
                            if (sum >= item.NoOfSet)
                            {
                                //ItemForecastPRRequest itemForecastPRRequest = new ItemForecastPRRequest
                                //{
                                ItemForecastPRRequest itemForecast = new ItemForecastPRRequest();
                                itemForecast.FulfillThrow = item.FulfillThrow;
                                itemForecast.BuyingPrice = item.FulfillThrow == 1 ? item.BuyingPrice : null;
                                itemForecast.ItemForecastDetailId = item.ItemForecastDetailId;
                                itemForecast.CreatedBy = userid;
                                itemForecast.CreatedDate = DateTime.Now;
                                itemForecast.Demand = item.Demand;
                                itemForecast.InternalTransferWHId = item.FulfillThrow == 2 ? item.InternalTransferWHId : null;
                                itemForecast.IsActive = false;
                                itemForecast.IsDeleted = true;
                                itemForecast.MinOrderQty = item.MinOrderQty;
                                itemForecast.NoOfSet = item.NoOfSet;
                                itemForecast.PRPaymentType = item.FulfillThrow == 1 ? item.PRPaymentType : null;
                                itemForecast.SalesIntentQty = item.SalesIntentQty;
                                itemForecast.FinalQty = item.NoOfSet * item.MinOrderQty;
                                itemForecast.SupplierId = item.FulfillThrow == 1 ? item.SupplierId : null;
                                itemForecast.Remainning = Convert.ToInt32(requireQty - (item.NoOfSet * item.MinOrderQty));
                                itemForecast.BuyerId = item.PeopleID; //add for buyerid
                                itemForecast.ETADate = item.ETADate;
                                itemForecast.PickerType = item.PickerType;
                                itemForecast.DepoId = item.DepoId;
                                itemForecast.FreightCharge = item.FreightCharge; //add for freightcharge
                                itemForecast.Demandcases = item.Demandcases;
                                itemForecast.AllowedQty = item.AllowedQty;
                                itemForecast.AllowedQtyOtherHub = item.AllowedQtyOtherHub;
                                itemForecast.bussinessType = item.bussinessType;
                                itemForecast.OPenQty = item.OPenQty;
                                itemForecast.RequiredQty = item.RequiredQty;
                                itemForecast.YesterdayDemand = item.YesterdayDemand;

                                //};
                                itemForecastPRRequest.Add(itemForecast);

                            }
                            else
                            {
                                response.Status = false;
                                response.msg = item.Itemname + "'s No of cases is lower than Sum of all Cases";
                                return response;
                            }
                        }
                        //  response.Status = result;
                    }

                    context.ItemForecastPRRequestDb.AddRange(itemForecastPRRequest);
                    context.Database.Connection.Close();
                    result = (await context.CommitAsync()) > 0;


                    if (!result)
                    {
                        response.Status = false;
                        response.msg = "Some error occurred during save data please try after some time.";
                    }
                    else
                    {
                        var CreatePoOrInternalRes = new CreatePoOrInternalRes();
                        CreatePoOrInternalDc dsds = new CreatePoOrInternalDc();
                        dsds.Ids = itemForecastPRRequest.Select(x => x.Id).ToList();
                        CreatePoOrInternalRes = await CreatePoOrInternal(dsds, context);
                        if (CreatePoOrInternalRes.Status == true)
                        {
                            foreach (var item in itemForecastPRRequest)
                            {
                                item.IsActive = true;
                                item.IsDeleted = false;
                                context.Entry(item).State = EntityState.Modified;
                                //int fulfillmentId = Convert.ToInt32(i.Id);
                                //rejectpurchaserequestid(fulfillmentId);
                            }
                            string res = null;
                            var exists = itemRequestDc.ItemForecastPRRequestForBulkobj.Where(x => x.Id > 0).Any();
                            if (exists)
                            {
                                var data = itemRequestDc.ItemForecastPRRequestForBulkobj.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                                var IdDt = new DataTable();
                                IdDt.Columns.Add("IntValue");
                                foreach (var item in data)
                                {
                                    var dr = IdDt.NewRow();
                                    dr["IntValue"] = item;
                                    IdDt.Rows.Add(dr);
                                }
                                var param = new SqlParameter("Ids", IdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.IntValues";
                                var param1 = new SqlParameter("@userid", userid);
                                res = context.Database.SqlQuery<string>("EXEC DeleteMultipleItemSavedraftInForecast @Ids,@userid", param, param1).FirstOrDefault();
                            }
                            await context.CommitAsync();
                            response.Status = true;
                            response.msg = CreatePoOrInternalRes.Message;

                        }
                        else
                        {
                            response.Status = false;
                            response.msg = CreatePoOrInternalRes.Message;
                        }
                    }
                }
                else
                {
                    response.Status = false;
                    response.msg = "Some error occurred during save data please try after some time.";
                }


            }
            return response;
        }

        [HttpPost]
        [Route("SavePurchaseApproveItemForeCast")]
        public PurchaseApprovedc SavePurchaseApproveItemForeCast(SavePurchaseApprovedc obj)
        {
            PurchaseApprovedc response = new PurchaseApprovedc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string res;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var authContext = new AuthContext())
            {
                var result = authContext.ItemForecastPRRequestDb.Where(x => x.ItemForecastDetailId == obj.Id && x.IsActive == true && x.IsDeleted == false && x.Id == obj.fulfillmentId).FirstOrDefault();
                if (result != null)
                {
                    long itemforecastdetaiid = result.ItemForecastDetailId;
                    long fullfillmentid = result.Id;
                    int supplierid = (int)result.SupplierId;
                    var sbusinesstype = result.bussinessType;
                    int depoid = (int)result.DepoId;
                    double frieght = (double)result.FreightCharge;
                    int moq = (int)result.MinOrderQty;
                    int noset = (int)result.NoOfSet;
                    int mby = userid;
                    var par1 = new SqlParameter("@fullfillmentid", fullfillmentid);
                    var par2 = new SqlParameter("@itemforecastdetaiid", itemforecastdetaiid);
                    var par3 = new SqlParameter("@supplierid", supplierid);
                    var par4 = new SqlParameter("@sbusinesstype", sbusinesstype ?? (object)DBNull.Value);
                    var par5 = new SqlParameter("@depoid", depoid);
                    var par6 = new SqlParameter("@frieght", frieght);
                    var par7 = new SqlParameter("@moq", moq);
                    var par8 = new SqlParameter("@noset", noset);
                    var par9 = new SqlParameter("@modifiedby", mby);
                    res = authContext.Database.SqlQuery<string>("EXEC sp_HistoryPurchaseInForecast @fullfillmentid,@itemforecastdetaiid,@supplierid,@sbusinesstype,@depoid,@frieght,@moq,@noset,@modifiedby", par1, par2, par3, par4, par5, par6, par7, par8, par9).FirstOrDefault();

                    result.MinOrderQty = obj.MinOrderQty;
                    result.NoOfSet = obj.NoOfSet;
                    result.SupplierId = obj.SupplierId;
                    result.DepoId = obj.DepoId;
                    result.FreightCharge = obj.FreightCharge;
                    result.ModifiedDate = DateTime.Now;
                    result.ModifiedBy = userid;
                    result.bussinessType = obj.bussinessType != null ? obj.bussinessType : null;  //add by Roshni
                    authContext.Entry(result).State = EntityState.Modified;
                    authContext.Commit();

                    response.msg = "Data Updated";
                    response.Status = true;
                }
                else
                {
                    response.msg = "Please retry";
                    response.Status = false;

                }

            }
            return response;

        }
        [HttpPost]
        [Route("POCreateStatus")]

        public List<CreatePOStatus> CreatePOStatussss(CreatePOStatusDC obj)
        {
            using (AuthContext db = new AuthContext())
            {
                var ware = new DataTable();

                if (obj.Status != null)
                {
                    ware.Columns.Add("IntValue");

                    foreach (var item in obj.Status)
                    {
                        var dr = ware.NewRow();
                        dr["IntValue"] = item;
                        ware.Rows.Add(dr);
                    }
                }
                var Po_Create_By = new SqlParameter
                {
                    ParameterName = "@Po_Create_By",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = ware
                };
                var Date = new SqlParameter("@Date", obj.PODate);
                List<CreatePOStatus> abc = new List<CreatePOStatus>();
                abc = db.Database.SqlQuery<CreatePOStatus>("exec  sp_CreatePoByForecast @Date,@Po_Create_By ", Date, Po_Create_By).ToList();
                return abc;
            }
        }

        [Route("GetSupplierForForecastBulk")]
        [HttpGet]
        public async Task<List<supplierMinDc>> GetSupplierForForecast()
        {
            List<supplierMinDc> supplierMinDcs = new List<supplierMinDc>();
            using (var context = new AuthContext())
            {
                supplierMinDcs = await context.Suppliers.Where(x => x.Active == true && x.Deleted == false).Select(x => new supplierMinDc
                {
                    SupplierId = x.SupplierId,
                    SupplierName = x.Name + "-" + x.SUPPLIERCODES
                }).OrderBy(x => x.SupplierName).ToListAsync();
            }
            return supplierMinDcs;
        }

        [HttpGet]
        [Route("AddNewItemInForecast")]

        public string AddNewItemInForecast(int ItemMultiMRPId, int warehouseId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                string res = null;
                var Multimrpid = new SqlParameter("@ItemMultiMRPId", ItemMultiMRPId);
                var ware = new SqlParameter("@warehouseId", warehouseId);
                res = db.Database.SqlQuery<string>("EXEC sp_AddNewItemInForecast @ItemMultiMRPId,@warehouseId", Multimrpid, ware).FirstOrDefault();

                return res;
            }
        }

        [HttpPost]
        [Route("GetItemForeCastForPOExport")]
        public async Task<ItemforcastPOEditExport> GetItemForeCastForPO(ItemForeCastRequestExport itemForeCastRequest)
        {
            ItemforcastPOEditExport itemforcastPOEdit = new ItemforcastPOEditExport();
            List<ItemForeCastPOResponseExport> itemForeCastPOResponses = new List<ItemForeCastPOResponseExport>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.cityIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("cityIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("categoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in itemForeCastRequest.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param4 = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = authContext.Database.Connection.CreateCommand();

                var param5 = new SqlParameter("@NetStock", itemForeCastRequest.NetStock);


                cmd.CommandText = "[dbo].[GetItemForeCastForPODetailwithtimeExport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);
                // cmd.Parameters.Add(new SqlParameter("@skip", itemForeCastRequest.skip));
                // cmd.Parameters.Add(new SqlParameter("@take", itemForeCastRequest.take));
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ItemForeCastPOData> itemForeCastPODataes = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<ItemForeCastPOData>(reader).ToList();
                itemForeCastPOResponses = itemForeCastPODataes.GroupBy(x => x.Id).Select(x => new ItemForeCastPOResponseExport
                {
                    Id = x.Key,
                    AveragePurchasePrice = x.FirstOrDefault().AveragePurchasePrice,
                    //BuyingPrice = x.FirstOrDefault().BuyingPrice,
                    // CityId = x.FirstOrDefault().CityId,
                    // CityName = x.FirstOrDefault().CityName,
                    CurrentStock = x.FirstOrDefault().CurrentStock,
                    //  FulfillThrow = x.FirstOrDefault().FulfillThrow,
                    ItemMultiMrpId = x.FirstOrDefault().ItemMultiMrpId,
                    ItemName = x.FirstOrDefault().ItemName,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    MRP = x.FirstOrDefault().MRP,
                    NetStock = x.FirstOrDefault().NetStock,
                    NoOfSet = x.FirstOrDefault().NoOfSet,
                    PRPaymentType = x.FirstOrDefault().PRPaymentType,
                    //  WarehouseId = x.FirstOrDefault().WarehouseId,
                    // RequiredQty = x.FirstOrDefault().RequiredQty,
                    //    SubsubcategoryName = x.FirstOrDefault().SubsubcategoryName,
                    //   SupplierId = x.FirstOrDefault().SupplierId,
                    WarehouseName = x.FirstOrDefault().WarehouseName,
                    YesterdayDemand = x.FirstOrDefault().YesterdayDemand,
                    //  SalesIntent = x.FirstOrDefault().SalesIntent,
                    //OtherWarehouseDetails = x.Select(y => new OtherWarehouseDetails
                    //{
                    //    OtherWarehouseId = y.OtherWarehouseId,
                    //    OtherWhDelCancel = y.OtherWhDelCancel,
                    //    OtherWhDemand = y.OtherWhDemand,
                    //    OtherWhNetDemand = y.OtherWhNetDemand,
                    //    OtherWhOpenPoQty = y.OtherWhOpenPoQty,
                    //    OtherWhStock = y.OtherWhStock,
                    //    OtherWarehouseName = y.OtherWarehouseNM
                    //}).ToList()
                }).ToList();
                itemforcastPOEdit.ItemForeCastPOResponses = itemForeCastPOResponses;
                reader.NextResult();

                return itemforcastPOEdit;
            }
        }
        [HttpPost]
        [Route("RejectPurchaseRequest")]

        public string rejectpurchaserequestid(int fulfillmentId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                string res = null;
                var fulfillmentIds = new SqlParameter("@fulfillmentId", fulfillmentId);

                res = db.Database.SqlQuery<string>("EXEC sp_RejectPurchaserequiesId @fulfillmentId", fulfillmentIds).FirstOrDefault();

                return res;
            }

        }

        [HttpPost]
        [Route("AddnewArticle")]
        public string AddnewArticlebuyeredit(int itemmrpid, int warehouseId, float MaxSellingPrice, int SystemSuggestedQty, int InventoryDays)
        {

            using (AuthContext db = new AuthContext())
            {
                string res = null;
                //var itemmrpid = db.ItemMultiMRPDB.Where(x => x.ItemNumber == ItemNumber && x.Deleted == false).Select(x => x.ItemMultiMRPId).FirstOrDefault();
                var chk = db.ItemForecastDetailDb.Where(x => x.ItemMultiMRPId == itemmrpid && x.WarehouseId == warehouseId && x.IsActive == true && x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year).FirstOrDefault();
                if (chk == null)
                {
                    var Multimrpid = new SqlParameter("@ItemMultiMRPId", itemmrpid);
                    var ware = new SqlParameter("@warehouseId", warehouseId);
                    var APP = new SqlParameter("@MaxSellingPrice", MaxSellingPrice);
                    var ssQty = new SqlParameter("@SystemSuggestedQty", SystemSuggestedQty);
                    var Idays = new SqlParameter("@InventoryDays", InventoryDays);
                    res = db.Database.SqlQuery<string>("EXEC sp_AddNewItemInBuyerEditForecast @ItemMultiMRPId,@warehouseId,@MaxSellingPrice,@SystemSuggestedQty,@InventoryDays", Multimrpid, ware, APP, ssQty, Idays).FirstOrDefault();
                    return res;
                }
                else
                {
                    return "ITemMultiMRPId Already Exists !";
                }

            }

        }
        [HttpPost]
        [Route("SearchNewItemNumber")]

        public List<searchnewitem> searchitemmrpid(string ItemNumber)
        {
            using (AuthContext db = new AuthContext())
            {

                var Multimrpid = new SqlParameter("@ItemNumber", ItemNumber);
                var data = db.Database.SqlQuery<searchnewitem>("EXEC ForecastNewArticle @ItemNumber", Multimrpid).ToList();
                return data;
            }

        }
        [HttpPost]
        [Route("GetSalesIndentComments")]

        public List<SalesIndentcomments> searchitemmrpid()
        {
            using (AuthContext db = new AuthContext())
            {
                string qry = "select * from Comment";
                var data = db.Database.SqlQuery<SalesIndentcomments>(qry).ToList();

                return data;
            }

        }

        [HttpPost]
        [Route("AddCommentsSalesIndent")]
        public AddSalesIndentComments AddComments(int Id, string Comments)
        {
            AddSalesIndentComments response = new AddSalesIndentComments();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var authContext = new AuthContext())
            {
                var result = authContext.SalesIntentRequestDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == Id).FirstOrDefault();
                if (result != null)
                {
                    result.Comments = Comments;
                    result.ModifiedDate = DateTime.Now;
                    result.ModifiedBy = userid;
                    authContext.Entry(result).State = EntityState.Modified;
                    authContext.Commit();
                    response.msg = "Comments Saved";
                    response.Status = true;
                }
                else
                {
                    response.msg = "Please retry";
                    response.Status = false;
                }

            }
            return response;


        }
        [Route("SaveASDraftItemfullfillment")]
        [HttpPost]
        public async Task<SaveDraftItemForeCastResponse> SaveASDraftItemFulFillRequestBulkItem(SaveasDraftRequest itemRequestDc)
        {
            SaveDraftItemForeCastResponse response = new SaveDraftItemForeCastResponse();
            bool result = false;
            bool results = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var count = 0;
                var msg = "";
                response.Idlist = new List<long>();
                foreach (var items in itemRequestDc.ItemForecastPRRequestForBulkobj.Where(x => x.Id == 0).ToList())
                {
                    var data = context.ItemForecastPRRequestSaveDraftDB.Where(x => x.WarehouseId == items.WarehouseId && x.CatID == items.Categoryid && x.SubCatID == items.SubCategoryId && x.SubSubCatID == items.SubsubCategoryid && x.IsActive == true && x.IsDeleted == false && x.ItemMultiMRPId == items.ItemMultiMRPId).FirstOrDefault();
                    if (data != null && data.CreatedBy != userid)
                    {
                        var name = context.Peoples.Where(x => x.PeopleID == data.CreatedBy).FirstOrDefault();
                        count = 1;
                        msg = msg + $"{data.ItemName}({name.DisplayName}),";
                        response.Idlist.Add(data.Id);
                    }
                    if (data != null && data.CreatedBy == userid)
                    {
                        count = 2;
                        msg = msg + (data.ItemName + ",");
                    }
                }
                if (count == 1)
                {
                    response.Status = false;
                    response.msg = "ItemName " + msg + "is already Draft by another user";
                    return response;
                }
                if (count == 2)
                {
                    response.Status = false;
                    response.msg = "ItemName " + msg + "is already Save by user";
                    return response;
                }
                foreach (var item in itemRequestDc.ItemForecastPRRequestForBulkobj)
                {
                    if (item.PickerType == null)
                    {
                        response.Status = false;
                        response.msg = "Please Select Picker Type";
                        return response;
                    }
                    if (item.ETADate == null)
                    {
                        response.Status = false;
                        response.msg = "Please Select ETA Date";
                        return response;
                    }
                    if (item.SupplierId == null)
                    {
                        response.Status = false;
                        response.msg = "Please Select Supplier";
                        return response;
                    }
                    if (item.DepoId <= 0)
                    {
                        response.Status = false;
                        response.msg = "Please Select Depo";
                        return response;
                    }
                    if (item.PRPaymentType == null)
                    {
                        response.Status = false;
                        response.msg = "Please Select Payment Type";
                        return response;
                    }
                    if (item.PeopleID <= 0)
                    {
                        response.Status = false;
                        response.msg = "Please Select BuyerID";
                        return response;
                    }
                    if (item.BuyingPrice == null)
                    {
                        response.Status = false;
                        response.msg = "Please Enter Price";
                        return response;
                    }
                    var itemForecastDetail = await context.ItemForecastDetailDb.FirstOrDefaultAsync(x => x.Id == item.ItemForecastDetailId);

                    var alert = await context.ItemForecastPRRequestSaveDraftDB.FirstOrDefaultAsync(x => x.IsDeleted == false && x.IsActive == true && x.WarehouseId == item.WarehouseId && x.CatID == item.Categoryid && x.SubCatID == item.SubCategoryId && x.SubSubCatID == item.SubsubCategoryid && x.ItemMultiMRPId == item.ItemMultiMRPId);

                    if ((alert == null) && (item.Id == 0))
                    {
                        if (itemForecastDetail != null)
                        {
                            double requireQty = 0;
                            int NoOfSet = 0;
                            int PurchaseMoq = 0;
                            NoOfSet = item.NoOfSet;
                            PurchaseMoq = item.MinOrderQty;
                            //double requireQty = 0;
                            //double otherhubReqQty = 0;
                            //int NoOfSet = 0;
                            //int PurchaseMoq = 0;
                            int FinalQty = 0;

                            NoOfSet = item.NoOfSet;
                            PurchaseMoq = item.MinOrderQty;
                            FinalQty = item.NoOfSet * item.MinOrderQty;

                            if (NoOfSet == 0)
                            {
                                response.msg = "No of Set should not be zero!";
                                return response;
                            }

                            if (PurchaseMoq == 0)
                            {
                                response.msg = "Purchase MOQ should not be zero!";
                                return response;
                            }

                            if (FinalQty == 0)
                            {
                                response.msg = "Final Qty should not be zero!";
                                return response;
                            }
                            requireQty = context.Database.SqlQuery<double>("Exec GetItemForeCastRequiredQty " + itemForecastDetail.Id).FirstOrDefault();
                            ItemForecastPRRequestSaveDraft ItemForecastPRRequest = new ItemForecastPRRequestSaveDraft
                            {
                                FulfillThrow = item.FulfillThrow,
                                BuyingPrice = item.FulfillThrow == 1 ? item.BuyingPrice : null,
                                ItemForecastDetailId = item.ItemForecastDetailId,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                Demand = item.Demand,
                                InternalTransferWHId = item.FulfillThrow == 2 ? item.InternalTransferWHId : null,
                                IsActive = true,
                                IsDeleted = false,
                                MinOrderQty = item.MinOrderQty,
                                NoOfSet = item.NoOfSet,
                                PRPaymentType = item.FulfillThrow == 1 ? item.PRPaymentType : null,
                                SalesIntentQty = item.SalesIntentQty,
                                FinalQty = item.NoOfSet * item.MinOrderQty,
                                SupplierId = item.FulfillThrow == 1 ? item.SupplierId : null,
                                Remainning = Convert.ToInt32(requireQty - (item.NoOfSet * item.MinOrderQty)),
                                BuyerId = item.PeopleID, //add for buyerid
                                ETADate = item.ETADate,
                                PickerType = item.PickerType,
                                DepoId = item.DepoId,
                                FreightCharge = item.FreightCharge,  //add for freightcharge
                                AllowedQtyOtherHub = item.AllowedQtyOtherHub,
                                Demandcases = item.Demandcases,
                                WarehouseId = item.WarehouseId,
                                CatID = item.Categoryid,
                                SubCatID = item.SubCategoryId,
                                SubSubCatID = item.SubsubCategoryid,
                                AllowedQty = item.AllowedQty,
                                ItemName = item.ItemName,
                                APP = item.APP,
                                ItemMultiMRPId = item.ItemMultiMRPId,
                                bussinessType = item.bussinessType, //add by Priyanka
                                YesterdayDemand = item.YesterdayDemand,//for report
                                RequiredQty = item.RequiredQty,//for report
                                OPenQty = item.OPenQty//for report

                            };
                            context.ItemForecastPRRequestSaveDraftDB.Add(ItemForecastPRRequest);
                            response.Status = true;
                            response.msg = "Record save successfully in Save as Draft";
                        }
                    }
                    if ((alert != null) && (item.Id > 0) && (alert.CreatedBy == userid))
                    {
                        if ((alert.FulfillThrow != item.FulfillThrow) || (alert.PickerType != item.PickerType) || (alert.ETADate != item.ETADate) || (alert.SupplierId != item.SupplierId) || (alert.DepoId != item.DepoId) || (alert.PRPaymentType != item.PRPaymentType) || (alert.BuyerId != item.PeopleID) || (alert.BuyingPrice != item.BuyingPrice) || (alert.MinOrderQty != item.MinOrderQty) || (alert.NoOfSet != item.NoOfSet) || (alert.bussinessType != item.bussinessType))
                        {
                            if (itemForecastDetail != null)
                            {
                                double requireQty = 0;

                                var data = context.ItemForecastPRRequestSaveDraftDB.Where(x => x.Id == item.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                requireQty = context.Database.SqlQuery<double>("Exec GetItemForeCastRequiredQty " + itemForecastDetail.Id).FirstOrDefault();
                                if (data != null)
                                {
                                    data.FulfillThrow = item.FulfillThrow;
                                    data.BuyingPrice = item.FulfillThrow == 1 ? item.BuyingPrice : null;
                                    data.ItemForecastDetailId = item.ItemForecastDetailId;
                                    data.ModifiedBy = userid;
                                    data.ModifiedDate = DateTime.Now;
                                    data.Demand = item.Demand;
                                    data.InternalTransferWHId = item.FulfillThrow == 2 ? item.InternalTransferWHId : null;
                                    data.IsActive = true;
                                    data.IsDeleted = false;
                                    data.MinOrderQty = item.MinOrderQty;
                                    data.NoOfSet = item.NoOfSet;
                                    data.PRPaymentType = item.FulfillThrow == 1 ? item.PRPaymentType : null;
                                    data.SalesIntentQty = item.SalesIntentQty;
                                    data.FinalQty = item.NoOfSet * item.MinOrderQty;
                                    data.SupplierId = item.FulfillThrow == 1 ? item.SupplierId : null;
                                    data.Remainning = Convert.ToInt32(requireQty - (item.NoOfSet * item.MinOrderQty));
                                    data.BuyerId = item.PeopleID; //add for buyerid
                                    data.ETADate = item.ETADate;
                                    data.PickerType = item.PickerType;
                                    data.DepoId = item.DepoId;
                                    data.FreightCharge = item.FreightCharge;  //add for freightcharge
                                    data.AllowedQtyOtherHub = item.AllowedQtyOtherHub;
                                    data.Demandcases = item.Demandcases;
                                    data.WarehouseId = item.WarehouseId;
                                    data.CatID = item.Categoryid;
                                    data.SubCatID = item.SubCategoryId;
                                    data.SubSubCatID = item.SubsubCategoryid;
                                    data.AllowedQty = item.AllowedQty;
                                    data.ItemName = item.ItemName;
                                    data.APP = item.APP;
                                    data.ItemMultiMRPId = item.ItemMultiMRPId;
                                    data.bussinessType = item.bussinessType; //add by Priyanka
                                    data.YesterdayDemand = item.YesterdayDemand;
                                    data.RequiredQty = item.RequiredQty;
                                    data.OPenQty = item.OPenQty;
                                    context.Entry(data).State = EntityState.Modified;
                                    response.Status = true;
                                    response.msg = "Record save successfully in Save as Draft";

                                }
                            }
                        }
                        else
                        {
                            results = true;
                        }
                    }




                }
                result = (await context.CommitAsync()) > 0;
                if (!result && !results)
                {
                    response.Status = false;
                    response.msg = "Some error occurred during save data please try after some time.";
                }
                else
                {
                    response.Status = true;
                    response.msg = "Record save successfully in Save as Draft";
                }
                return response;
            }

        }

        [Route("GetViewDraftByFilter")]
        [HttpPost]
        public List<ItemForecastPRRequestSaveDraft> GetViewDraftItem(ViewDraftRequest viewdraft)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var authcontext = new AuthContext())
            {

                if (authcontext.Database.Connection.State != ConnectionState.Open)
                    authcontext.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in viewdraft.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("warehouseIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in viewdraft.categoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("categoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in viewdraft.subCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param2 = new SqlParameter("subCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in viewdraft.subSubCategoriesIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param3 = new SqlParameter("subSubCategoriesIds", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var param4 = new SqlParameter("@userid", userid);
                var cmd = authcontext.Database.Connection.CreateCommand();
                //cmd.CommandText = "[dbo].[GetItemForeCastForPODetail]";

                cmd.CommandText = "[dbo].[ViewDraftItemByFilter]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ItemForecastPRRequestSaveDraft> ItemForecastPRRequestSaveDraftdata = ((IObjectContextAdapter)authcontext)
                .ObjectContext
                .Translate<ItemForecastPRRequestSaveDraft>(reader).ToList();
                return ItemForecastPRRequestSaveDraftdata;

            }
        }

        [HttpGet]
        [Route("DeleteItemSaveasDraft")]

        public string DeleteItemInForecast(int Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                string res = null;
                if (Id > 0)
                {
                    var id = new SqlParameter("@Id", Id);
                    res = db.Database.SqlQuery<string>("EXEC sp_DeleteItemSavedraftInForecast @Id", id).FirstOrDefault();
                }
                else
                {
                    res = "Data Not Found";
                }
                return res;
            }
        }

        [HttpPost]
        [Route("DeleteMultipleItemSaveasDraft")]
        public string DeleteItemInForecast(DeleteMultipleDraft deletedraft)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {

                string res = null;

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in deletedraft.Ids)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("Ids", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var param1 = new SqlParameter("@userid", userid);


                res = db.Database.SqlQuery<string>("EXEC DeleteMultipleItemSavedraftInForecast @Ids,@userid", param, param1).FirstOrDefault();
                return res;
            }
        }

        //[Route("InventoryDaysRestrictionUpdate")]
        //[HttpGet]
        //public async Task<InventoryRestrictionUpdateStatusResponse> UpdateInvRestriction(int WarehouseId, int storeid,int subcatid,int subsubcatid,int NoOfInventoryDays)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    InventoryRestrictionUpdateStatusResponse InventoryRestrictionUpdateResponse = new InventoryRestrictionUpdateStatusResponse();

        //    using (var authContext = new AuthContext())
        //    {
        //        var TblInventoryRestriction = authContext.ForecastInventoryDayDb.FirstOrDefault(x => x.WarehouseId == WarehouseId && x.SubCatID== subcatid && x.SubSubCatID==subsubcatid);
        //      //  var Tblitemforecastdetailid = authContext.ItemForecastDetailDb.FirstOrDefault(x => x.WarehouseId == WarehouseId && x. == subcatid && x.SubSubCatID == subsubcatid);
        //        if (TblInventoryRestriction != null )
        //        {

        //            if (NoOfInventoryDays >= 0)
        //            {
        //                if (NoOfInventoryDays != TblInventoryRestriction.InventoryDays)
        //                {
        //                    TblInventoryRestriction.InventoryDays = NoOfInventoryDays;
        //                    TblInventoryRestriction.ModifiedBy = userid;
        //                    TblInventoryRestriction.ModifiedDate = DateTime.Now;

        //                    authContext.Entry(TblInventoryRestriction).State = EntityState.Modified;
        //                }

        //                if (authContext.Commit() > 0)
        //                {
        //                    //InventoryRestrictionUpdateResponse.Status = true;
        //                    //InventoryRestrictionUpdateResponse.msg = "No Of Inventory Days Updated successfully.";
        //                    var param1 = new SqlParameter("@InventoryDays", NoOfInventoryDays);
        //                    var param2 = new SqlParameter("@WarehouseId", WarehouseId); 
        //                    var param3 = new SqlParameter("@SubCatID", subcatid);
        //                    var param4 = new SqlParameter("@SubSubCatID", subsubcatid);
        //                    var param5 = new SqlParameter("@userid", userid);
        //                    var param6 = new SqlParameter("@storeid", storeid);
        //                    var res = authContext.Database.SqlQuery<string>("EXEC sp_UpdateInventoryDaysRestriction @InventoryDays,@WarehouseId,@SubCatID,@SubSubCatID,@userid", param1, param2,param3,param4,param5).FirstOrDefault();
        //                    InventoryRestrictionUpdateResponse.Status = true;
        //                    InventoryRestrictionUpdateResponse.msg = "No Of Inventory Days Updated successfully.";

        //                }
        //                else
        //                {
        //                    InventoryRestrictionUpdateResponse.Status = false;
        //                    InventoryRestrictionUpdateResponse.msg = "No Of Inventory Days Data Not Updated.";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            InventoryRestrictionUpdateResponse.Status = false;
        //            InventoryRestrictionUpdateResponse.msg = "Update Request Brand Record not Found.";
        //        }

        //    }
        //    return InventoryRestrictionUpdateResponse;
        //}


        [Route("InventoryDaysRestrictionUpdate")]
        [HttpGet]
        public string UpdateInvRestriction(int WarehouseId, int storeid, int subcatid, int subsubcatid, int NoOfInventoryDays, int Id, string brandname, int cid, int SafetyDays)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string result = null;

            using (var authContext = new AuthContext())
            {

                if (NoOfInventoryDays >= 0)
                {
                    var param1 = new SqlParameter("@InventoryDays", NoOfInventoryDays);
                    var param10 = new SqlParameter("@SafetyDays", SafetyDays);
                    var param2 = new SqlParameter("@WarehouseId", WarehouseId);
                    var param3 = new SqlParameter("@SubCatID", subcatid);
                    var param4 = new SqlParameter("@SubSubCatID", subsubcatid);
                    var param5 = new SqlParameter("@userid", userid);
                    var param6 = new SqlParameter("@storeid", storeid);
                    var param7 = new SqlParameter("@Id", Id);
                    var param8 = new SqlParameter("@BrandName", brandname);
                    var param9 = new SqlParameter("@Cid", cid);

                    var res = authContext.Database.SqlQuery<string>("EXEC sp_UpdateInventoryDaysRestriction @InventoryDays,@SafetyDays,@WarehouseId,@SubCatID,@SubSubCatID,@userid,@storeid,@Id,@BrandName,@Cid", param1, param10, param2, param3, param4, param5, param6, param7, param8, param9).FirstOrDefault();
                    result = res;
                }
                else
                {

                    result = "No of Inventory Days should be greater than 0 .";
                }
                return result;
            }

        }

        [Route("GETPOIDforOpenQty")]
        [HttpGet]
        public GetIDforOPenPO GetPOforOpenPOQty(int ItemMultiMRPId, int warehouseId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();
                GetIDforOPenPO obj = new GetIDforOPenPO();

                var param1 = new SqlParameter("@ItemMultiMRPId", ItemMultiMRPId);
                var param2 = new SqlParameter("@Warehouseid", warehouseId);
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetPOIDforOPenPO]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                var reader = cmd.ExecuteReader();
                List<GetPOIDforOPenPOQtyDC> GetID = ((IObjectContextAdapter)db)
                .ObjectContext
                .Translate<GetPOIDforOPenPOQtyDC>(reader).ToList();
                reader.NextResult();
                List<GetInternalIDforOPenPOqtyDC> GetinternalID = ((IObjectContextAdapter)db)
                .ObjectContext
                .Translate<GetInternalIDforOPenPOqtyDC>(reader).ToList();
                obj.POIDs = GetID;
                obj.InternalIDs = GetinternalID;
                return obj;
            }
        }

        //getbrandrestriction

        [HttpPost]
        [Route("getBrandRestricted")]

        public List<searchbranddata> getbranddata()
        {
            using (AuthContext db = new AuthContext())
            {

                var data = db.Database.SqlQuery<searchbranddata>("EXEC getbrandrestriction").ToList();
                return data;
            }

        }

        [HttpPost]
        [Route("getAllBrand")]

        public List<GetSubcategoryCategoryMappings> getAllbranddata()
        {
            using (AuthContext db = new AuthContext())
            {

                var data = db.Database.SqlQuery<GetSubcategoryCategoryMappings>("EXEC GetSubcategoryCategoryMappings").ToList();
                return data;
            }

        }



        [HttpPost]
        [Route("AddRestrictedArticle")]
        public string AddRestrictedArticlebuyeredit(getList obj)
        {

            using (AuthContext db = new AuthContext())
            {
                string res = null;
                //var itemmrpid = db.ItemMultiMRPDB.Where(x => x.ItemNumber == ItemNumber && x.Deleted == false).Select(x => x.ItemMultiMRPId).FirstOrDefault();
                var chk = db.BrandIndentrestrictionDB.Where(x => x.Categoryid == obj.Categoryid && x.SubCategoryId == obj.SubCategoryId && x.SubSubCategoryId == obj.SubsubcategoryId).FirstOrDefault();
                if (chk == null)
                {
                    var cCategoryid = new SqlParameter("@CategoryId", obj.Categoryid);
                    var sSubCategoryId = new SqlParameter("@SubCategoryId", obj.SubCategoryId);
                    var sSubsubcategoryId = new SqlParameter("@SubSubCategoryId", obj.SubsubcategoryId);

                    res = db.Database.SqlQuery<string>("EXEC sp_AddRestrictredBrand @CategoryId,@SubCategoryId,@SubSubCategoryId", cCategoryid, sSubCategoryId, sSubsubcategoryId).FirstOrDefault();
                    return res;
                }
                else
                {
                    return "Brand Already Exists in Restricted Brand!";
                }

            }

        }
        [HttpPost]
        [Route("GetAutoPoManual")]
        [AllowAnonymous]
        public List<GetAutoPoManualDc> GetAutoPoManual(ReqData obj)
        {
            using (AuthContext db = new AuthContext())
            {
                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in obj.warehouseIds)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var wid = new SqlParameter("warehouseIds", IdDt);
                wid.SqlDbType = SqlDbType.Structured;
                wid.TypeName = "dbo.IntValues";
                var StartDate = new SqlParameter("@startdate", obj.startdate);
                var EndDate = new SqlParameter("@enddate", obj.enddate);
                List<GetAutoPoManualDc> abc = new List<GetAutoPoManualDc>();
                abc = db.Database.SqlQuery<GetAutoPoManualDc>("exec sp_GetAutoPoManualreport @warehouseIds,@startdate,@enddate ", wid, StartDate, EndDate).ToList();
                return abc;
            }
        }
        [Route("EmailForBuyer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> EmailForBuyer()
        {
            string From = "";
            From = AppConstants.MasterEmail;
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            using (AuthContext context = new AuthContext())
            {
                // var ClearanceNonSellableOrder = context.Database.SqlQuery<getClPendingOrd>("EXEC getClPendingOrdForMail ").ToList();
                var getSafetyQtydata = context.Database.SqlQuery<GetSafetyQtydc>("EXEC GetAllowedQtyforMail").ToList();
                if (getSafetyQtydata != null && getSafetyQtydata.Any())
                {
                    var email = getSafetyQtydata.Select(t => new { t.Email, t.ItemMultiMRPId }).Distinct().ToList();
                    // var itemmultimrp = getSafetyQtydata.Select(t => t.ItemMultiMRPId).Distinct().ToList();
                    foreach (var r in email)
                    {
                        DataTable table = new DataTable();
                        List<GetSafetyQtydc> data = new List<GetSafetyQtydc>();
                        var datatables = new List<DataTable>();
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);
                        foreach (var e in getSafetyQtydata.Where(y => y.Email == r.Email).ToList())
                        {
                            data.Add(new GetSafetyQtydc
                            {
                                BuyerName = e.BuyerName,
                                Email = e.Email,
                                WarehouseName = e.WarehouseName,
                                SafetyStockQty = e.SafetyStockQty,
                                //SafetyDays=e.SafetyDays,
                                AllowedQty = e.AllowedQty,
                                CurrentStock = e.CurrentStock,
                                ItemName = e.ItemName,
                                ItemMultiMRPId = e.ItemMultiMRPId
                            });
                        }
                        table = ClassToDataTable.CreateDataTable(data);
                        table.TableName = "SafetyStockAllowedList";
                        datatables.Add(table);
                        string filePath = ExcelSavePath + "SafetyStockAllowedList" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                        {
                            DateTime now = DateTime.Now;
                            var sub = "Urgent Purchase Order Request - Safety Stock Required";
                            var msg = "Dear " + getSafetyQtydata.FirstOrDefault(y => y.Email == r.Email).BuyerName + ",<br/>  We are reaching out to you with an urgent request to create a purchase order for the attached multiMRPID's:\n We have closely monitored our inventory levels, and unfortunately, the current stock of the above items has fallen below the safety stock threshold that we established together.";
                            EmailHelper.SendMail(From, getSafetyQtydata.FirstOrDefault(y => y.Email == r.Email).Email, "", sub, msg, filePath);
                        }

                    }
                }

            }

            return true;
        }

        public class ReqData
        {
            public List<int> warehouseIds { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
        }

        public class GetSafetyQtydc
        {

            public string WarehouseName { get; set; }
            public string BuyerName { get; set; }
            public string Email { get; set; }


            public string ItemName { get; set; }
            public int ItemMultiMRPId { get; set; }

            public int? CurrentStock { get; set; }
            // public int? SafetyDays  { get; set; }
            public int? AllowedQty { get; set; }
            public int? SafetyStockQty { get; set; }




        }
        public class CreatePOStatusDC
        {
            public DateTime PODate { get; set; }
            public List<int> Status { get; set; }
        }
        public class GetAutoPoManualDc
        {
            public int? POID { get; set; }
            public long? Id { get; set; }
            public string WarehouseName { get; set; }
            public string ItemName { get; set; }
            public int? ItemMultiMRPId { get; set; }
            public int? SystemSuggestedQty { get; set; }
            public int? GrowthForecastQty { get; set; }
            public int? BuyerEdit { get; set; }
            public int? YesterdayDemand { get; set; }
            public int? SalesIntentQty { get; set; }
            public int? OPenQty { get; set; }
            public int? AllowedQty { get; set; }
            public DateTime? PRDate { get; set; }
            public string buyer { get; set; }
            public int? FinalPOQty { get; set; }
            public string fullfilltype { get; set; }
            public DateTime? fullfillDate { get; set; }
            public int? GRQty { get; set; }
            public string ApprovedBy { get; set; }


        }
        public class CreatePOStatus
        {
            public long? ItemForecastPRRequestsID { get; set; }
            public int PurchaseOrderId { get; set; }
            public string WarehouseName { get; set; }
            public string Status { get; set; }
            public DateTime CreationDate { get; set; }
            public string CreatedBy { get; set; }
            public string Po_Create_By { get; set; }
        }



        public class datas
        {

            public int ItemMultiMRPId { get; set; }

            public long RowId { get; set; }
            public string itemname { get; set; }
            public double MRP { get; set; }
            public string UnitofQuantity { get; set; }
            public string UOM { get; set; }

            public long Id { get; set; }
            public int BuyerEdit { get; set; }

            public int TotalBuyerEdit { get; set; }
        }
        public class uploadWName
        {
            public string WarehouseName { get; set; }
            public int WarehouseId { get; set; }


        }
        public class ItemFullfillmentCommentdc
        {
            public int ItemFullfillmentId { get; set; }

            public string Comments { get; set; }

        }

        public class SavePurchaseApprovedc
        {

            public int? SupplierId { get; set; }
            public int DepoId { get; set; }
            public double? FreightCharge { get; set; }
            public int MinOrderQty { get; set; }
            public int NoOfSet { get; set; }
            public long Id { get; set; }
            public long fulfillmentId { get; set; }
            public string bussinessType { get; set; } //add by Priyanka

        }
        public class PurchaseApprovedc
        {

            public bool Status { get; set; }
            public string msg { get; set; }


        }
        public class searchnewitem
        {

            public bool Sel { get; set; }
            public int ItemMultiMRPId { get; set; }
            public double MRP { get; set; }
            public string itemname { get; set; }

            public double UnitPrice { get; set; }
            public double PurchasePrice { get; set; }
        }
        public class SalesIndentcomments
        {
            public int id { get; set; }
            public string Comments { get; set; }

        }
        public class AddSalesIndentComments
        {
            public bool Status { get; set; }
            public string msg { get; set; }

        }

        public class ViewDraftRequest
        {
            public List<int> warehouseIds { get; set; }
            public List<int> categoriesIds { get; set; }
            public List<int> subCategoriesIds { get; set; }
            public List<int> subSubCategoriesIds { get; set; }
        }
        public class SaveDraftItemForeCastResponse
        {
            public bool Status { get; set; }
            public string msg { get; set; }
            public List<long> Idlist { get; set; }
        }

        public class DeleteMultipleDraft
        {
            public List<long> Ids { get; set; }
        }

        public class Settled_Request
        {

            public List<int> subSubCategoriesIds { get; set; }
            public string productname { get; set; }
            public DateTime? month { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
        }
        public class Pending_Request
        {

            public List<int> subSubCategoriesIds { get; set; }
            public string productname { get; set; }
            public DateTime? month { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
        }
        public class GetIDforOPenPO
        {
            //public int PurchaseOrderId { get; set; }
            //public int OpenPOQTy { get; set; }
            //public int TransferOrderId { get; set; }
            //public int Qty { get; set; }


            public List<GetPOIDforOPenPOQtyDC> POIDs { get; set; }
            public List<GetInternalIDforOPenPOqtyDC> InternalIDs { get; set; }

        }
        public class GetPOIDforOPenPOQtyDC
        {
            public int PurchaseOrderId { get; set; }
            public int OpenPOQTy { get; set; }

        }
        public class GetInternalIDforOPenPOqtyDC
        {
            public int TransferOrderId { get; set; }
            public int Qty { get; set; }

        }

        public class searchbranddata
        {

            public long Id { get; set; }

            public string SubsubcategoryName { get; set; }
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public int warehouseId { get; set; }
            public int categoriesId { get; set; }
            public int subCategoriesId { get; set; }
            public int subSubCategoriesId { get; set; }
        }

        public class GetSubcategoryCategoryMappings
        {
            //public string BaseCategoryName { get; set; }
            public string CategoryName { get; set; }
            public string SubsubcategoryName { get; set; }
            //public int BaseCategoryId { get; set; }
            public int Categoryid { get; set; }
            public int SubsubcategoryId { get; set; }
            public string SubcategoryName { get; set; }
            //public int BrandId { get; set; }
            //public string BrandName { get; set; }
            public int SubCategoryId { get; set; }
            //public long StoreId { get; set; }
            //public string StoreName { get; set; }
        }
        public class getList
        {
            public int WarehouseId { get; set; }
            public int SubCategoryId { get; set; }
            public int Categoryid { get; set; }
            public int SubsubcategoryId { get; set; }


        }

        public class Dashboard_Request
        {

            public List<int> subSubCategoriesIds { get; set; }
            public int WarehouseId { get; set; }
            public DateTime? month { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
        }

        public class Response
        {
            public bool status { get; set; }
            public string msg { get; set; }
            public string Data { get; set; }
        }

        public class SalesIntentDc
        {
            public long Id { get; set; }
            public long? ItemForecastDetailId { get; set; }
            public int PeopleId { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int RequestQty { get; set; }
            public double RequestPrice { get; set; }
            public int SalesLeadApproveID { get; set; }
            public DateTime? SalesApprovedDate { get; set; }
            public int BuyerApproveID { get; set; }
            public DateTime? BuyerApprovedDate { get; set; }
            public int Status { get; set; }  // Pending for Lead = 0, Pending for buyer = 1, Rejected = 2, Approved = 3  
            public int Warehouseid { get; set; }
            public int? TotalOrderQty { get; set; }

            public string OrderId { get; set; }

            public int? MinOrderQty { get; set; } //New Change

            public int? NoOfSet { get; set; } //New Change

            public DateTime? ETADate { get; set; } //New Change

            public double? BuyingPrice { get; set; } // New Change

            public string Comments { get; set; } //New Change

            public int isReject { get; set; } //New Change
            public DateTime? ModifiedDate { get; set; }


        }

    }

}