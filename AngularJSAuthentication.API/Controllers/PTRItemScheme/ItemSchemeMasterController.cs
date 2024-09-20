using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers.PTRItemScheme
{
    [RoutePrefix("api/ItemSchemeMaster")]
    public class ItemSchemeMasterController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region ItemSchemeExcelUploaderMaster and  UpdateForReErrorChecking
        [Route("GetUploadedItemScheme")]
        [HttpGet]
        public async Task<List<ItemSchemeExcelUploaderMastersDc>> GetUploadedItemScheme(int CityId, int SubsubCategoryid, int skip, int take)
        {
            List<ItemSchemeExcelUploaderMastersDc> ItemSchemeDatas = new List<ItemSchemeExcelUploaderMastersDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetUploadedItemScheme]";
                cmd.Parameters.Add(new SqlParameter("@CityId", CityId));
                cmd.Parameters.Add(new SqlParameter("@SubsubCategoryid", SubsubCategoryid));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                ItemSchemeDatas = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchemeExcelUploaderMastersDc>(reader).ToList();
            }
            return ItemSchemeDatas;
        }

        [Route("ItemSchemeUploadedMasterById/{Id}")]
        [HttpGet]
        public async Task<ItemSchemeExcelUploaderMastersDc> getItemSchemeUploadedMasterById(long Id)
        {
            ItemSchemeExcelUploaderMastersDc result = new ItemSchemeExcelUploaderMastersDc();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetUploadedItemSchemeMaster]";
                cmd.Parameters.Add(new SqlParameter("@Id", Id));

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                var ItemSchemeDatas = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchemeExcelUploaderMastersDc>(reader).FirstOrDefault();


                var ItemSchemeMaster = context.ItemSchemeExcelUploaderMasters.Where(x => x.IsDeleted == false && x.Id == Id)
                                      .Include(c => c.ItemSchemeExcelUploaderDetails)
                                      .FirstOrDefault();

                result = Mapper.Map(ItemSchemeMaster).ToANew<ItemSchemeExcelUploaderMastersDc>();
                if (ItemSchemeDatas != null)
                {
                    result.BrandName = ItemSchemeDatas.BrandName;
                    result.CreatedBy = ItemSchemeDatas.CreatedBy;
                    result.ApprovedBy = ItemSchemeDatas.ApprovedBy;
                }
            }
            return result;
        }

        #endregion

        #region ItemSchemeMaster and Approved process
        [Route("GetItemSchemeMaster")]
        [HttpGet]
        public async Task<List<ItemSchemeMasterDC>> GetItemSchemeMaster(int CityId, int SubsubCategoryid, int skip, int take)
        {
            List<ItemSchemeMasterDC> ItemSchemeDatas = new List<ItemSchemeMasterDC>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemSchemeMaster]";
                cmd.Parameters.Add(new SqlParameter("@CityId", CityId));
                cmd.Parameters.Add(new SqlParameter("@SubsubCategoryid", SubsubCategoryid));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                ItemSchemeDatas = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchemeMasterDC>(reader).ToList();
            }
            return ItemSchemeDatas;
        }


        [Route("GetItemSchemeMasterById/{Id}")]
        [HttpGet]
        public async Task<ItemSchemeMasterDC> GetItemSchemeMaster(long Id)
        {
            ItemSchemeMasterDC result = null;
            if (Id > 0)
            {
                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemSchemeMasterById]";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var ItemSchemeDatas = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchemeMasterDC>(reader).FirstOrDefault();

                    var ItemSchemeMaster = context.ItemSchemeMasters.Where(x => x.IsDeleted == false && x.Id == Id)
                                     .Include(c => c.ItemSchemeDetails)
                                     .Include("ItemSchemeDetails.Slabs")
                                     .Include("ItemSchemeDetails.ItemSchemeFreebiess")
                                     .FirstOrDefault();

                    result = Mapper.Map(ItemSchemeMaster).ToANew<ItemSchemeMasterDC>();
                    if (ItemSchemeDatas != null)
                    {
                        result.BrandName = ItemSchemeDatas.BrandName;
                        result.CreatedBy = ItemSchemeDatas.CreatedBy;
                        result.ApprovedBy = ItemSchemeDatas.ApprovedBy;
                    }
                }
            }
            return result;
        }

        [Route("ApproveItemSchemMaster")]
        [HttpPut]
        public async Task<string> ApproveItemSchemMaster(List<ItemSchemeMasterdetailsDc> ItemSchemeDetail)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "Approve ItemSchemMaster Stopped by @Sijo sir from 2023-03-09"; 
            return result;
            //int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            //if (ItemSchemeDetail.Count > 0 && ItemSchemeDetail.Any() && userid > 0 && SubCatId>0)
            //{
            //    var MasterId = ItemSchemeDetail.FirstOrDefault().ItemSchemeMasterId;
            //    TransactionOptions option = new TransactionOptions();
            //    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            //    option.Timeout = TimeSpan.FromSeconds(90);
            //    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            //    {
            //        using (var context = new AuthContext())
            //        {
            //            var ItemSchemeMasters = context.ItemSchemeMasters.Where(x => x.IsDeleted == false && x.Id == MasterId && x.IsApproved == false && x.IsActive == false)
            //                             .Include(c => c.ItemSchemeDetails)
            //                             .Include("ItemSchemeDetails.Slabs")
            //                             .Include("ItemSchemeDetails.ItemSchemeFreebiess")
            //                             .FirstOrDefault();

            //            if (ItemSchemeMasters != null)
            //            {
            //                var dIds = ItemSchemeDetail.Where(x => x.IsActive == true).Select(x => x.ItemSchemeDetailId).Distinct().ToList();
            //                ItemSchemeMasters.ItemSchemeDetails = ItemSchemeMasters.ItemSchemeDetails.Where(x => dIds.Contains(x.Id)).ToList();
            //                var warehouses = context.Warehouses.Where(x => ItemSchemeMasters.Cityid == x.Cityid && x.active==true && x.IsKPP==false).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
            //                var itemCompanyCodelist = ItemSchemeMasters.ItemSchemeDetails.Select(y => y.CompanyCode).ToList();
            //                var itemStockCodelist = ItemSchemeMasters.ItemSchemeDetails.Select(y => y.CompanyStockCode).ToList();
            //                var FreeitemCompanyCodelist = ItemSchemeMasters.ItemSchemeDetails.SelectMany(x => x.ItemSchemeFreebiess.Where(z => !string.IsNullOrEmpty(z.ItemCompanyCode)).Select(y => y.ItemCompanyCode)).ToList();
            //                var FreeitemStockCodelist = ItemSchemeMasters.ItemSchemeDetails.SelectMany(x => x.ItemSchemeFreebiess.Where(z => !string.IsNullOrEmpty(z.ItemCompanyStockCode)).Select(y => y.ItemCompanyStockCode)).ToList();
            //                var CompanyCodes = itemCompanyCodelist.Concat(FreeitemCompanyCodelist);
            //                var StockCode = itemStockCodelist.Concat(FreeitemStockCodelist);
            //                var itemcentrallst = context.ItemMasterCentralDB.Where(x => CompanyCodes.Contains(x.CompanyCode) && x.Deleted == false).ToList();
            //                var itemStocklst = context.ItemMultiMRPDB.Where(x => StockCode.Contains(x.CompanyStockCode)).ToList();
            //                ItemSchemeMasters.IsActive = true;
            //                ItemSchemeMasters.ModifiedBy = userid;
            //                ItemSchemeMasters.ModifiedDate = indianTime;
            //                ItemSchemeMasters.ApprovedBy = userid;
            //                ItemSchemeMasters.ApprovedDate = indianTime;
            //                ItemSchemeMasters.IsApproved = true;
            //                foreach (var item in ItemSchemeMasters.ItemSchemeDetails.Where(x => dIds.Contains(x.Id)))
            //                {
            //                    //Appliy schem on all item citymaste on companycode/CompanyStockCode and //Create offer
            //                    string IsUpdated = await UpdateCityItems(ItemSchemeMasters, item, context, itemcentrallst, itemStocklst, userid, SubCatId);
            //                    if (IsUpdated != null && IsUpdated == "Updated")
            //                    {
            //                        item.IsActive = true;
            //                    }
            //                    else
            //                    {
            //                        item.IsActive = false;
            //                        result = IsUpdated;//error message
            //                        dbContextTransaction.Dispose();
            //                        return result;
            //                    }
            //                }
            //                context.Entry(ItemSchemeMasters).State = EntityState.Modified;
            //                if (context.Commit() > 0)
            //                {
            //                    dbContextTransaction.Complete();
            //                    result = "Approved Successfully";
            //                }
            //                else
            //                {
            //                    dbContextTransaction.Dispose();
            //                    result = "Something went wrong";
            //                }
            //            }
            //            else
            //            {
            //                result = "Already Approved Successfully";
            //            }
            //        }
            //    }
            //}
            //return result;
        }
        #endregion


        #region ItemMaster
        public async Task<string> UpdateCityItems(ItemSchemeMaster ItemSchemeMaster, ItemSchemeDetail schemeitem, AuthContext context, List<ItemMasterCentral> itemcentrallst, List<ItemMultiMRP> itemStocklst, int userid, int SubCatId)
        {
            string result = null;
            if (ItemSchemeMaster.Cityid > 0 && userid > 0 && schemeitem != null)
            {
                List<FreebiesUploader> GeneretFreebies = new List<FreebiesUploader>();
                var ItemCityItemLists = context.itemMasters.Where(x => x.Cityid == ItemSchemeMaster.Cityid && x.ItemMultiMRPId == schemeitem.ItemMultiMRPId).ToList();
                var wid = ItemCityItemLists.Select(x => x.WarehouseId).Distinct().ToList();
                var Number = ItemCityItemLists.Select(x => x.Number).FirstOrDefault();
                var MRP = ItemCityItemLists.Where(x => x.price > 0).Select(x => x.price).FirstOrDefault();
                if (schemeitem.ItemSchemeFreebiess.Count > 0 && schemeitem.ItemSchemeFreebiess.Any())
                {
                    foreach (var Frerbiesitem in schemeitem.ItemSchemeFreebiess)
                    {
                        foreach (var whid in wid)
                        {
                            if (Frerbiesitem.BaseItemQty > 0 && Frerbiesitem.Qty > 0 && Frerbiesitem.StockQty > 0)
                            {
                                FreebiesUploader fre = new FreebiesUploader();
                                fre.CityId = ItemSchemeMaster.Cityid;
                                fre.WarehouseId = whid;
                                fre.Itemnumber = Number;
                                fre.MRP = MRP;
                                fre.FreeItemnumber = Number;
                                fre.MinimumOrderQty = Frerbiesitem.BaseItemQty;
                                fre.NoOfFreeitemQty = Frerbiesitem.Qty;
                                fre.FreeitemMRP = MRP;
                                fre.StartDate = ItemSchemeMaster.StartDate;
                                fre.EndDate = ItemSchemeMaster.EndDate;
                                fre.QtyAvaiable = Frerbiesitem.StockQty;
                                fre.IsFreeStock = Frerbiesitem.IsFreeStock;
                                GeneretFreebies.Add(fre);
                            }
                        }
                    }
                }

                if (GeneretFreebies.Count > 0 && GeneretFreebies.Any())
                {
                    OfferController CreateFree = new OfferController();
                    List<FreebiesUploader> FreeBiesCreated = CreateFree.AddFreebiesDuringItemScheme(GeneretFreebies, userid, context, SubCatId);
                    if (FreeBiesCreated.Any(x => !string.IsNullOrEmpty(x.Error)))
                    {
                        return result = "Scheme not Activated due to offer not created for Free ItemNumber: " + string.Join(",", FreeBiesCreated.Where(x => x.Error != null).Select(x => x.FreeItemnumber).Distinct());
                    }
                    else
                    {
                        schemeitem.OfferIds = string.Join(",", FreeBiesCreated.Select(x => x.OfferId).Distinct());
                        schemeitem.OfferIds = schemeitem.OfferIds.TrimEnd(',');
                        //schemeitem.ItemIds = string.Join(",", FreeBiesCreated.Select(x => x.ItemId).Distinct());
                    }
                }
                List<ItemMaster> UpdateItemLists = new List<ItemMaster>();
                var CityItemLists = context.itemMasters.Where(x => x.Cityid == ItemSchemeMaster.Cityid && x.ItemMultiMRPId == schemeitem.ItemMultiMRPId).ToList();

                foreach (var item in CityItemLists)
                {
                    double SchemRate = 0;
                    double BRate = 0;
                    double slrate = 0;
                    if (schemeitem.PTR > 0)
                    {
                        SchemRate = Math.Round(item.price / (1 + schemeitem.PTR / 100), 3);
                    }
                    if (schemeitem.BaseScheme > 0)
                    {
                        BRate = Math.Round(SchemRate * (schemeitem.BaseScheme / 100), 3);
                    }
                    if (schemeitem.Slabs.Count > 0 && schemeitem.Slabs.Any())
                    {
                        var slabSchem = schemeitem.Slabs.FirstOrDefault(x => x.SlabPurchaseQTY == item.MinOrderQty);
                        if (slabSchem != null)
                        {
                            slrate = Math.Round(SchemRate * (slabSchem.SlabScheme / 100), 3);
                        }
                    }

                    SchemRate = SchemRate - (BRate + slrate);

                    double Poprice = 0; //ptr price



                    double Ptrprice = 0;
                    double baseprice = 0;
                    double Slabprice = 0;

                    double ptrandOnivoiceprice = 0;
                    //case 1
                    if (schemeitem.PTR > 0 && schemeitem.IsIncludeOnInvoiceMarginPOPrice)
                    {
                        Ptrprice = Math.Round(item.price / (1 + (schemeitem.PTR) / 100), 3);

                        ptrandOnivoiceprice = Math.Round(item.price / (1 + (schemeitem.onvoiceMargin + schemeitem.PTR) / 100), 3);

                        if (schemeitem.IsIncludeMaxSlabPOPrice)
                        {
                            if (schemeitem.Slabs.Any())
                            {
                                Slabprice = Math.Round(Ptrprice * (schemeitem.Slabs.Max(x => x.SlabScheme) / 100), 3);
                            }
                        }
                        Poprice = ptrandOnivoiceprice - Slabprice;

                        if (schemeitem.IsIncludeBaseSchmePOPrice)
                        {
                            if (schemeitem.BaseScheme > 0)
                            {
                                baseprice = Math.Round(Ptrprice * (schemeitem.BaseScheme / 100), 3);
                            }
                        }
                        Poprice = Poprice - baseprice;
                    }


                    //case 2
                    else if (schemeitem.PTR > 0 && !schemeitem.IsIncludeOnInvoiceMarginPOPrice)
                    {
                        ptrandOnivoiceprice = Math.Round(item.price / (1 + (schemeitem.PTR) / 100), 3);

                        if (schemeitem.IsIncludeMaxSlabPOPrice)
                        {
                            if (schemeitem.Slabs.Any())
                            {
                                Slabprice = Math.Round(ptrandOnivoiceprice * (schemeitem.Slabs.Max(x => x.SlabScheme) / 100), 3);
                            }
                        }
                        Poprice = ptrandOnivoiceprice - Slabprice;

                        if (schemeitem.IsIncludeBaseSchmePOPrice)
                        {
                            if (schemeitem.BaseScheme > 0)
                            {
                                baseprice = Math.Round(ptrandOnivoiceprice * (schemeitem.BaseScheme / 100), 3);
                            }
                        }
                        Poprice = Poprice - baseprice;
                    }

                    item.POPurchasePrice = Math.Round(Poprice, 3);
                    item.PurchasePrice = Math.Round(Poprice, 3);

                    item.NetPurchasePrice = Math.Round(item.PurchasePrice / ((100 + item.TotalTaxPercentage) / 100), 3);//without tax value

                    item.UnitPrice = Math.Round(SchemRate, 3);
                    item.active = true;
                    UpdateItemLists.Add(item);
                }
                if (UpdateItemLists.Count > 0 && UpdateItemLists.Any())
                {
                    foreach (var item in UpdateItemLists)
                    {
                        item.ModifiedBy = userid;
                        item.UpdatedDate = indianTime;
                        context.Entry(item).State = EntityState.Modified;
                    }

                    schemeitem.ItemIds = string.Join(",", UpdateItemLists.Select(x => x.ItemId).Distinct());

                    if (context.Commit() > 0) { result = "Updated"; } else { result = "Something went wrong in UpdateCityItems"; }
                }

            }
            return result;
        }
        #endregion


        #region Scheme Deactivation 
        [Route("DeActivateSchemeMaster")]
        [HttpGet]
        public async Task<string> DeActivateItemSchemeMasterById(long Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            if (userid > 0)
            {
                ItemSchemeHelper manager = new ItemSchemeHelper();
                string IsUpdate = await manager.DeActivateItemSchemeMasterById(Id, userid);
                if (IsUpdate != null)
                {
                    result = IsUpdate;
                }
                else
                {
                    result = "Something went wrong";

                }
            }
            return result;
        }

        [Route("DeActivateSchemeDetail")]
        [HttpGet]
        public async Task<string> DeActivateItemSchemeOnDetailById(long MasterId, long DetailId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            if (userid > 0)
            {
                ItemSchemeHelper manager = new ItemSchemeHelper();
                string IsUpdate = await manager.DeActivateItemSchemeOnDetailById(MasterId, DetailId, userid);
                if (IsUpdate != null)
                {
                    result = IsUpdate;
                }
                else
                {
                    result = "Something went wrong";
                }
            }
            return result;
        }
        #endregion

        [Route("UpdateForReErrorChecking")]
        [HttpGet]
        public async Task<string> UpdateForReErrorChecking(long MasterId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";

            if (MasterId > 0 && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var ItemSchemeMasters = context.ItemSchemeExcelUploaderMasters.FirstOrDefault(x => x.IsDeleted == false && x.Id == MasterId && x.Status == 1 && x.IsApproved == false && x.IsActive == false);
                    if (ItemSchemeMasters != null)
                    {
                        ItemSchemeMasters.Status = 0;
                        ItemSchemeMasters.ModifiedBy = userid;
                        ItemSchemeMasters.ModifiedDate = indianTime;
                        context.Entry(ItemSchemeMasters).State = EntityState.Modified;
                        if (context.Commit() > 0) { result = "Record Updated For ReErrorChecking"; } else { result = "Something went wrong"; }
                    }
                }
            }
            return result;
        }


        [Route("DeActivateFreebiesDetailId")]
        [HttpGet]
        public async Task<string> DeActivateFreebiesDetailId(long MasterId, long DetailId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            if (userid > 0)
            {
                ItemSchemeHelper manager = new ItemSchemeHelper();
                string IsUpdate = await manager.DeActivateFreebiesByDetailId(MasterId, DetailId, userid);
                if (IsUpdate != null)
                {
                    result = IsUpdate;
                }
                else
                {
                    result = "Something went wrong";
                }
            }
            return result;
        }

        [Route("GetExcelUploadItemScheme")]
        [HttpGet]
        public async Task<List<ItemSchemeExcelUploaderMastersDc>> GetExcelUploadItemScheme(int CityId, int skip, int take)
        {
            List<ItemSchemeExcelUploaderMastersDc> ItemSchemeUploadMasterData = new List<ItemSchemeExcelUploaderMastersDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetExcelUploadItemScheme]";
                cmd.Parameters.Add(new SqlParameter("@CityId", CityId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                ItemSchemeUploadMasterData = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchemeExcelUploaderMastersDc>(reader).ToList();
            }
            return ItemSchemeUploadMasterData;
        }

        [Route("GetItemSchemeExcelUploaderDetailsList")]
        [HttpGet]
        public async Task<List<ItemSchExcelUploaderDetails>> GetItemSchemeExcelUploaderDetailsList(int excelUploaderMasterId, int skip, int take)
        {
            List<ItemSchExcelUploaderDetails> ItemSchemeUploadDetailData = new List<ItemSchExcelUploaderDetails>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[ItemSchemeExcelUploaderDetailsList]";
                cmd.Parameters.Add(new SqlParameter("@ExcelUploaderMasterId", excelUploaderMasterId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                ItemSchemeUploadDetailData = ((IObjectContextAdapter)context).ObjectContext.Translate<ItemSchExcelUploaderDetails>(reader).ToList();
            }
            return ItemSchemeUploadDetailData;
        }

    }


}
