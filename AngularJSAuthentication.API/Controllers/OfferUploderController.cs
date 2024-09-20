using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BillDiscount;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using GenricEcommers.Models;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("offer")]
    public class OfferUploderController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();


        [Route("AddOfferUsingUploader")]
        [HttpPost]
        public OfferResponseDC AddOfferUsingUploader()
        {
            OfferResponseDC offerResponseDC = new OfferResponseDC { status = true, msg = "", Offer = null, ShowValidationSkipmsg = false };
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            string UserName = string.Empty;

            List<OfferColumn> IncludeItemColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                };

            List<OfferColumn> ExcludeItemColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                };

            List<OfferColumn> MandatoryColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string)},
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                   new OfferColumn { ColumnName="ValueType",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ValueAmount",DataType=typeof(int) },
                };


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                UserName = (identity.Claims.FirstOrDefault(x => x.Type == "username").Value);
            List<Warehousedto> warehouses = null;
            List<int> warehouseids = null;

            List<OfferItemsBillDiscount> OfferItemsBillDiscounts = new List<OfferItemsBillDiscount>();
            List<BillDiscountOfferSection> BillDiscountOfferSections = new List<BillDiscountOfferSection>();
            List<OfferBillDiscountRequiredItem> OfferBillDiscountRequiredItems = new List<OfferBillDiscountRequiredItem>();
            var Jsondata = HttpContext.Current.Request.Form["Data"];
            OfferUploderDc offer = Newtonsoft.Json.JsonConvert.DeserializeObject<OfferUploderDc>(Jsondata);
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OfferExcelFile")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OfferExcelFile"));

                    string extension = Path.GetExtension(httpPostedFile.FileName);

                    string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OfferExcelFile"), filename);

                    httpPostedFile.SaveAs(ImageUrl);

                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }

                    ISheet Includesheet = hssfwb.GetSheet("Include Item");
                    ISheet Excludesheet = hssfwb.GetSheet("Exclude Item");
                    ISheet Mandatorysheet = hssfwb.GetSheet("Mandatory");
                    if (Includesheet == null)
                    {
                        offerResponseDC.msg = "Upload Offer File does not contain include item sheet.";
                        offerResponseDC.status = false;
                        return offerResponseDC;
                    }

                    if (Excludesheet == null)
                    {
                        offerResponseDC.msg = "Upload offer file does not contain exclude item sheet.";
                        offerResponseDC.status = false;
                        return offerResponseDC;
                    }
                    if (Mandatorysheet == null)
                    {
                        offerResponseDC.msg = "Upload offer file does not contain mandatory item sheet.";
                        offerResponseDC.status = false;
                        return offerResponseDC;
                    }
                    string ErrorMsg = "";
                    List<OfferColumn> cols = new List<OfferColumn>();
                    for (int i = 0; i < 3; i++)
                    {
                        string sSheetName = hssfwb.GetSheetName(i);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);

                        if (sSheetName == "Include Item")
                            cols = IncludeItemColumn;
                        else if (sSheetName == "Exclude Item")
                            cols = ExcludeItemColumn;
                        else if (sSheetName == "Mandatory")
                            cols = MandatoryColumn;

                        ErrorMsg += ValidateSheet(sheet, cols);
                    }
                    if (!string.IsNullOrEmpty(ErrorMsg))
                    {
                        offerResponseDC.msg = ErrorMsg;
                        offerResponseDC.status = false;
                        return offerResponseDC;
                    }

                    List<offeruploadItemDC> IncludeItemDcs = new List<offeruploadItemDC>();
                    List<offeruploadItemDC> ExcludeItemDcs = new List<offeruploadItemDC>();
                    List<offeruploadRequireItemDC> RequireItemDcs = new List<offeruploadRequireItemDC>();
                    IRow rowData;
                    offeruploadItemDC data = null;
                    if (Includesheet.LastRowNum >= 1)
                    {
                        for (int iRowIdx = 1; iRowIdx <= Includesheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            rowData = Includesheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                data = new offeruploadItemDC();
                                data.IsInclude = true;
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        if (cellDatas.ColumnIndex < 4)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                data.Category = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                data.SubCategory = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2 && !string.IsNullOrEmpty(cellDatas.ToString()))
                                                data.BrandName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3 && !string.IsNullOrEmpty(cellDatas.ToString()))
                                                data.ItemMultiMRPId = Convert.ToInt32(cellDatas.ToString());
                                        }
                                    }
                                    IncludeItemDcs.Add(data);
                                }
                                catch (Exception ex)
                                {
                                    offerResponseDC.msg = "Include Sheet Error: " + ex.Message;
                                    offerResponseDC.status = false;
                                    return offerResponseDC;
                                }
                            }
                        }
                    }
                    if (Excludesheet.LastRowNum >= 1)
                    {
                        for (int iRowIdx = 1; iRowIdx <= Excludesheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            rowData = Excludesheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                data = new offeruploadItemDC();
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        if (cellDatas.ColumnIndex < 4)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                data.Category = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                data.SubCategory = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2 && !string.IsNullOrEmpty(cellDatas.ToString()))
                                                data.BrandName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3 && !string.IsNullOrEmpty(cellDatas.ToString()))
                                                data.ItemMultiMRPId = Convert.ToInt32(cellDatas.ToString());
                                        }

                                    }
                                    ExcludeItemDcs.Add(data);
                                }
                                catch (Exception ex)
                                {
                                    offerResponseDC.msg = "Exclude Sheet Error: " + ex.Message;
                                    offerResponseDC.status = false;
                                    return offerResponseDC;
                                }
                            }
                        }
                    }
                    offeruploadRequireItemDC RequireItem = null;
                    if (Mandatorysheet.LastRowNum >= 1)
                    {
                        for (int iRowIdx = 1; iRowIdx <= Mandatorysheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            //rowData = Excludesheet.GetRow(iRowIdx);
                            rowData = Mandatorysheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                //data = new offeruploadItemDC();
                                RequireItem = new offeruploadRequireItemDC();
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        if (cellDatas.ColumnIndex < 6)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                                RequireItem.Category = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 1)
                                                RequireItem.SubCategory = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 2)
                                                RequireItem.BrandName = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 3 && !string.IsNullOrEmpty(cellDatas.ToString()))
                                                RequireItem.ItemMultiMRPId = Convert.ToInt32(cellDatas.ToString());
                                            else if (cellDatas.ColumnIndex == 4)
                                                RequireItem.ValueType = cellDatas.ToString();
                                            else if (cellDatas.ColumnIndex == 5)
                                                RequireItem.ValueAmount = Convert.ToInt32(cellDatas.ToString());
                                        }

                                    }

                                    RequireItemDcs.Add(RequireItem);
                                }
                                catch (Exception ex)
                                {
                                    offerResponseDC.msg = "Mandatory Sheet Error: " + ex.Message;
                                    offerResponseDC.status = false;
                                    return offerResponseDC;
                                }
                            }
                        }
                    }
                    using (AuthContext context = new AuthContext())
                    {
                        string query = "EXEC GetBrandCategoryMappings ";
                        var BrandCategoryMappings = context.Database.SqlQuery<GetBrandCategoryMappingsDC>(query).ToList();
                        var storeBrand = offer.StoreId > 0 ? context.StoreBrandDB.Where(x => x.StoreId == offer.StoreId && x.IsActive).ToList() : null;
                        if (!string.IsNullOrEmpty(offer.WarehouseIds))
                        {
                            warehouseids = offer.WarehouseIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            warehouses = context.Warehouses.Where(x => warehouseids.Contains(x.WarehouseId)).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, WarehouseName = y.WarehouseName, CityId = y.Cityid }).ToList();
                        }
                        if (IncludeItemDcs.Any())
                        {

                            if (IncludeItemDcs.Any(x => string.IsNullOrEmpty(x.Category) || string.IsNullOrEmpty(x.SubCategory)))
                            {
                                offerResponseDC.msg = "Some Category and SubCategory are blank in Include item sheet.";
                                offerResponseDC.status = false;
                                return offerResponseDC;
                            }

                            if ((IncludeItemDcs.Any(x => x.ItemMultiMRPId == 0) && !IncludeItemDcs.All(x => x.ItemMultiMRPId == 0))
                                 || (IncludeItemDcs.Any(x => string.IsNullOrEmpty(x.BrandName)) && !IncludeItemDcs.All(x => string.IsNullOrEmpty(x.BrandName)))
                                 || (IncludeItemDcs.Any(x => string.IsNullOrEmpty(x.SubCategory)) && !IncludeItemDcs.All(x => string.IsNullOrEmpty(x.SubCategory)))
                                )
                            {
                                offerResponseDC.msg = "Not In Similar Item in Include item sheet.";
                                offerResponseDC.status = false;
                                return offerResponseDC;
                            }


                            if (ExcludeItemDcs.Any(x => x.ItemMultiMRPId == 0) && !ExcludeItemDcs.All(x => x.ItemMultiMRPId == 0))
                            {
                                offerResponseDC.msg = "Item not added in the Exclude item sheet.";
                                offerResponseDC.status = false;
                                return offerResponseDC;
                            }


                            foreach (var item in IncludeItemDcs)
                            {
                                if (item.BrandName == null)
                                    item.BrandName = "";
                                if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() || x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower())))
                                {
                                    var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower());
                                    if (string.IsNullOrEmpty(item.BrandName))
                                        brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower());

                                    if (brandMapping != null)
                                    {
                                        item.BrandCategoryMappingId = string.IsNullOrEmpty(item.BrandName) ? 0 : brandMapping.BrandCategoryMappingId;

                                        item.SubCategoryMappingId = brandMapping.SubCategoryMappingId;
                                    }
                                    else
                                    {
                                        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in include Sheet.";
                                    }
                                    if (storeBrand != null && !storeBrand.Any(x => x.BrandCategoryMappingId == brandMapping.BrandCategoryMappingId))
                                    {
                                        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " not in selected store in include Sheet.";
                                    }
                                }
                                else
                                    ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in include Sheet.";
                            }


                            foreach (var item in ExcludeItemDcs)
                            {
                                //if (item.ItemMultiMRPId == 0)
                                //{
                                if (item != null)
                                    if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower())))
                                    {
                                        var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower());
                                        if (brandMapping != null)
                                        {
                                            item.BrandCategoryMappingId = brandMapping.BrandCategoryMappingId;
                                            item.SubCategoryMappingId = brandMapping.SubCategoryMappingId;
                                        }
                                        else
                                        {
                                            ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in exclude Sheet.";
                                        }
                                        if (!IncludeItemDcs.Any(x => x.SubCategoryMappingId == item.SubCategoryMappingId))
                                        {
                                            ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.ItemMultiMRPId + " not related include sheet item in exclude Sheet.";
                                        }


                                    }
                                    else
                                        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in exclude Sheet.";
                                //}
                            }

                            foreach (var item in RequireItemDcs)
                            {
                                if (item.ItemMultiMRPId == 0)
                                {
                                    if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower())))
                                    {
                                        var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == item.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == item.SubCategory.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == item.BrandName.Trim().ToLower());

                                        item.BrandId = brandMapping.BrandCategoryMappingId;

                                    }
                                    else
                                        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in Mandatory Sheet.";
                                }
                            }

                            if (IncludeItemDcs != null && IncludeItemDcs.Any())
                            {
                                var itemMultiMrpIds = IncludeItemDcs.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId);

                                var IncludeItems = itemMultiMrpIds.Any() ? context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.Deleted /*&& !x.IsDisContinued*/).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.SubsubCategoryid, x.SubCategoryId, x.Categoryid, x.WarehouseId, x.itemname, x.MRP }).ToList() : null;

                                if (storeBrand != null && storeBrand.Any() && IncludeItems != null && IncludeItems.Any())
                                {
                                    var storecatBrands = BrandCategoryMappings.Where(x => storeBrand.Select(y => y.BrandCategoryMappingId).Contains(x.BrandCategoryMappingId)).ToList();
                                    IncludeItems = IncludeItems.Where(y => storecatBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubsubcategoryId).Contains(y.Categoryid + "-" + y.SubCategoryId + "-" + y.SubsubCategoryid)).ToList();
                                }

                                if (ExcludeItemDcs != null && ExcludeItemDcs.Any() && ExcludeItemDcs.All(x => x.ItemMultiMRPId == 0))
                                {
                                    ExcludeItemDcs.ForEach(x => x.IsInclude = false);
                                    IncludeItemDcs.AddRange(ExcludeItemDcs);
                                    ExcludeItemDcs = new List<offeruploadItemDC>();
                                }

                                if ((IncludeItemDcs.Any(x => x.ItemMultiMRPId == 0) && !IncludeItemDcs.All(x => x.ItemMultiMRPId == 0))
                                 || (IncludeItemDcs.Any(x => string.IsNullOrEmpty(x.BrandName)) && !IncludeItemDcs.All(x => string.IsNullOrEmpty(x.BrandName)))
                                 || (IncludeItemDcs.Any(x => string.IsNullOrEmpty(x.SubCategory)) && !IncludeItemDcs.All(x => string.IsNullOrEmpty(x.SubCategory)))
                                )
                                {
                                    offerResponseDC.msg = "Not In Similar Item in Include item sheet.";
                                    offerResponseDC.status = false;
                                    return offerResponseDC;
                                }

                                foreach (var item in IncludeItemDcs)
                                {
                                    foreach (var wh in warehouses)
                                    {
                                        BillDiscountOfferSection includesection = new BillDiscountOfferSection();
                                        includesection.IsInclude = item.IsInclude;
                                        includesection.WarehouseId = wh.WarehouseId;
                                        if (IncludeItems != null && item.ItemMultiMRPId > 0 && IncludeItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            var dbItems = IncludeItems.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId).ToList();
                                            if (dbItems == null && !dbItems.Any())
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Include Item Sheet.";
                                            }
                                            else
                                            {

                                                includesection.ObjId = dbItems.FirstOrDefault().ItemId;
                                            }
                                        }
                                        else if (IncludeItems != null && item.ItemMultiMRPId > 0 && !IncludeItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Include Item Sheet.";
                                        }
                                        else
                                            includesection.ObjId = item.BrandCategoryMappingId > 0 ? item.BrandCategoryMappingId : item.SubCategoryMappingId;


                                        offer.BillDiscountType = item.ItemMultiMRPId > 0 ? "BillDiscount" : item.BrandCategoryMappingId > 0 ? "brand" : "subcategory";
                                        BillDiscountOfferSections.Add(includesection);
                                    }
                                }
                            }
                            var includeItemDcCopy = new List<offeruploadItemDC>();
                            if (IncludeItemDcs.All(x => x.ItemMultiMRPId > 0))
                            {
                                IncludeItemDcs.ForEach(x => x.IsInclude = true);
                                includeItemDcCopy.AddRange(IncludeItemDcs);
                                ExcludeItemDcs.AddRange(IncludeItemDcs);
                                IncludeItemDcs = new List<offeruploadItemDC>();
                                BillDiscountOfferSections = new List<BillDiscountOfferSection>();
                                offer.BillDiscountType = "items";
                            }

                            if (ExcludeItemDcs != null && ExcludeItemDcs.Any() && ExcludeItemDcs.All(x => x.ItemMultiMRPId > 0))
                            {
                                var itemMultiMrpIds = ExcludeItemDcs.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId);
                                var ExcludeItems = itemMultiMrpIds.Any() ? context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.Deleted /*&& !x.IsDisContinued*/).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.WarehouseId, x.itemname, x.MRP, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList() : null;

                                if (storeBrand != null && storeBrand.Any() && ExcludeItems != null && ExcludeItems.Any())
                                {
                                    var storecatBrands = BrandCategoryMappings.Where(x => storeBrand.Select(y => y.BrandCategoryMappingId).Contains(x.BrandCategoryMappingId)).ToList();
                                    ExcludeItems = ExcludeItems.Where(y => storecatBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubsubcategoryId).Contains(y.Categoryid + "-" + y.SubCategoryId + "-" + y.SubsubCategoryid)).ToList();
                                }

                                foreach (var item in ExcludeItemDcs)
                                {
                                    foreach (var wh in warehouses)
                                    {
                                        OfferItemsBillDiscount excludesection = new OfferItemsBillDiscount();
                                        excludesection.WarehouseId = wh.WarehouseId;
                                        if (ExcludeItems != null && ExcludeItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            var dbItems = ExcludeItems.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId).ToList();
                                            if (dbItems == null || !dbItems.Any())
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Exclude Sheet.";
                                            }
                                            else
                                            {
                                                excludesection.itemId = dbItems.FirstOrDefault().ItemId;
                                                excludesection.IsInclude = item.IsInclude;
                                                OfferItemsBillDiscounts.Add(excludesection);

                                                foreach (var otheritem in dbItems.Where(x => x.ItemId != excludesection.itemId))
                                                {
                                                    OfferItemsBillDiscount excludesectionmore = new OfferItemsBillDiscount();
                                                    excludesectionmore.IsInclude = item.IsInclude;
                                                    excludesectionmore.WarehouseId = wh.WarehouseId;
                                                    excludesectionmore.itemId = otheritem.ItemId;
                                                    OfferItemsBillDiscounts.Add(excludesectionmore);
                                                }
                                            }
                                        }
                                        else if (ExcludeItems != null && !ExcludeItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            if (!includeItemDcCopy.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Exclude Sheet.";
                                            }
                                        }
                                    }
                                }
                            }

                            if (RequireItemDcs != null && RequireItemDcs.Any())
                            {
                                var itemMultiMrpIds = RequireItemDcs.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId);
                                var MendatoryItems = itemMultiMrpIds.Any() ? context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.Deleted /*&& !x.IsDisContinued*/).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.WarehouseId, x.itemname, x.MRP }).ToList() : null;

                                foreach (var item in RequireItemDcs)
                                {
                                    foreach (var wh in warehouses)
                                    {
                                        OfferBillDiscountRequiredItem requiredItem = new OfferBillDiscountRequiredItem();
                                        requiredItem.ValueType = item.ValueType;
                                        requiredItem.ObjectValue = item.ValueAmount;
                                        requiredItem.WarehouseId = wh.WarehouseId;
                                        if (MendatoryItems != null && MendatoryItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            var dbItem = MendatoryItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId);
                                            if (dbItem == null)
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Mandatory Sheet.";
                                            }
                                            else
                                            {
                                                requiredItem.ObjectId = dbItem.ItemMultiMRPId.ToString();
                                                requiredItem.ObjectText = dbItem.itemname;
                                                requiredItem.ObjectType = "Item";
                                            }
                                        }
                                        else if (MendatoryItems != null && MendatoryItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Mandatory Sheet.";
                                        }
                                        else
                                        {
                                            requiredItem.ObjectType = "brand";
                                            requiredItem.ObjectId = item.BrandId.ToString();
                                            requiredItem.ObjectText = item.BrandName;
                                        }

                                        OfferBillDiscountRequiredItems.Add(requiredItem);
                                    }
                                }
                            }


                        }
                        else
                        {
                            offer.BillDiscountType = "BillDiscount";
                            foreach (var item in ExcludeItemDcs)
                            {
                                if (BrandCategoryMappings.Any(x => x.CategoryName == item.Category && (x.SubcategoryName == item.SubCategory && x.SubsubcategoryName == item.BrandName)))
                                {
                                    var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName == item.Category && x.SubcategoryName == item.SubCategory && x.SubsubcategoryName == item.BrandName);
                                    item.BrandCategoryMappingId = brandMapping.BrandCategoryMappingId;
                                    item.SubCategoryMappingId = brandMapping.SubCategoryMappingId;

                                }
                                else
                                    ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in Exclude Sheet.";
                            }

                            foreach (var item in RequireItemDcs)
                            {
                                if (item.ItemMultiMRPId == 0)
                                {
                                    if (BrandCategoryMappings.Any(x => x.CategoryName == item.Category && (x.SubcategoryName == item.SubCategory && x.SubsubcategoryName == item.BrandName)))
                                    {
                                        var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName == item.Category && x.SubcategoryName == item.SubCategory && x.SubsubcategoryName == item.BrandName);

                                        item.BrandId = brandMapping.BrandCategoryMappingId;
                                    }
                                    else
                                        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + item.Category + ", " + item.SubCategory + ", " + item.BrandName + " Mapping not correct in Mandatory Sheet.";
                                }
                            }


                            if (ExcludeItemDcs != null && ExcludeItemDcs.Any())
                            {
                                var itemMultiMrpIds = ExcludeItemDcs.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId);
                                var ExcludeItems = itemMultiMrpIds.Any() ? context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.Deleted /*&& !x.IsDisContinued*/).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.WarehouseId, x.itemname, x.MRP, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList() : null;

                                if (storeBrand != null && storeBrand.Any() && ExcludeItems != null && ExcludeItems.Any())
                                {
                                    var storecatBrands = BrandCategoryMappings.Where(x => storeBrand.Select(y => y.BrandCategoryMappingId).Contains(x.BrandCategoryMappingId)).ToList();
                                    ExcludeItems = ExcludeItems.Where(y => storecatBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubsubcategoryId).Contains(y.Categoryid + "-" + y.SubCategoryId + "-" + y.SubsubCategoryid)).ToList();
                                }

                                foreach (var item in ExcludeItemDcs)
                                {
                                    foreach (var wh in warehouses)
                                    {
                                        OfferItemsBillDiscount excludesection = new OfferItemsBillDiscount();
                                        excludesection.IsInclude = false;
                                        excludesection.WarehouseId = wh.WarehouseId;
                                        excludesection.itemId = item.BrandCategoryMappingId;
                                        if (ExcludeItems != null && ExcludeItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            var dbItems = ExcludeItems.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId).ToList();
                                            if (dbItems == null || !dbItems.Any())
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Exclude Sheet.";
                                            }
                                            else
                                            {
                                                offer.BillDiscountType = "items";
                                                excludesection.itemId = dbItems.FirstOrDefault().ItemId;
                                                foreach (var otheritem in dbItems.Where(x => x.ItemId != excludesection.itemId))
                                                {
                                                    OfferItemsBillDiscount excludesectionmore = new OfferItemsBillDiscount();
                                                    excludesectionmore.IsInclude = false;
                                                    excludesectionmore.WarehouseId = wh.WarehouseId;
                                                    excludesectionmore.itemId = otheritem.ItemId;
                                                    OfferItemsBillDiscounts.Add(excludesectionmore);
                                                }
                                            }
                                            OfferItemsBillDiscounts.Add(excludesection);
                                        }
                                        else
                                        {
                                            BillDiscountOfferSection includesection = new BillDiscountOfferSection();
                                            includesection.IsInclude = false;
                                            includesection.WarehouseId = wh.WarehouseId;
                                            includesection.ObjId = item.BrandCategoryMappingId > 0 ? item.BrandCategoryMappingId : item.SubCategoryMappingId;
                                            offer.BillDiscountType = item.BrandCategoryMappingId > 0 ? "brand" : "subcategory";
                                            BillDiscountOfferSections.Add(includesection);
                                        }


                                    }
                                }
                            }

                            if (RequireItemDcs != null && RequireItemDcs.Any())
                            {
                                var itemMultiMrpIds = RequireItemDcs.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId);
                                var MendatoryItems = itemMultiMrpIds.Any() ? context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.Deleted /*&& !x.IsDisContinued*/).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.WarehouseId, x.itemname, x.MRP }).ToList() : null;

                                foreach (var item in RequireItemDcs)
                                {
                                    foreach (var wh in warehouses)
                                    {
                                        OfferBillDiscountRequiredItem requiredItem = new OfferBillDiscountRequiredItem();
                                        requiredItem.ValueType = item.ValueType;
                                        requiredItem.ObjectValue = item.ValueAmount;
                                        requiredItem.WarehouseId = wh.WarehouseId;
                                        if (MendatoryItems != null && MendatoryItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                        {
                                            var dbItem = MendatoryItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId);
                                            if (dbItem == null)
                                            {
                                                ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + item.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Mandatory Sheet.";
                                            }
                                            else
                                            {
                                                requiredItem.ObjectId = dbItem.ItemMultiMRPId.ToString();
                                                requiredItem.ObjectText = dbItem.itemname;
                                                requiredItem.ObjectType = "Item";
                                            }
                                        }
                                        else
                                        {
                                            requiredItem.ObjectType = "brand";
                                            requiredItem.ObjectId = item.BrandId.ToString();
                                            requiredItem.ObjectText = item.BrandName;
                                        }

                                        OfferBillDiscountRequiredItems.Add(requiredItem);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(ErrorMsg))
                        {
                            offerResponseDC.msg = ErrorMsg;
                            offerResponseDC.status = false;
                            return offerResponseDC;
                        }

                    }
                }
                else
                {
                    offerResponseDC.msg = "Please Upload Offer File.";
                    offerResponseDC.status = false;
                    return offerResponseDC;
                }
            }
            else
            {
                offerResponseDC.msg = "Please Upload Offer File.";
                offerResponseDC.status = false;
                return offerResponseDC;
            }


            using (AuthContext context = new AuthContext())
            {
                List<Offer> offers = new List<Offer>();


                if (offer == null)
                {
                    throw new ArgumentNullException("offer");
                }


                try
                {

                    DateTime date1 = offer.start;
                    DateTime date2 = offer.end;
                    RewardItems rewardItems = new RewardItems();
                    ItemMaster itemMaster = new ItemMaster();
                    List<int> customeridlist = null;
                    List<long> channelidslist = null;
                    if (offer.StoreId > 0)
                    {
                        if (offer.ChannelIds != null && offer.ChannelIds != "")
                        {
                            channelidslist = offer.ChannelIds.Split(',').Select(x => Convert.ToInt64(x)).ToList();
                            if (channelidslist.Any() && channelidslist.Count() > 0)
                            {
                                string query = "select distinct CustomerId from CustomerChannelMappings where StoreId=" + offer.StoreId + "and ChannelMasterId in (" + offer.ChannelIds + ")and IsActive=1 and IsDeleted=0";
                                customeridlist = context.Database.SqlQuery<int>(query).ToList();
                                if (customeridlist.Count() > 0 && customeridlist.Any()) { }
                                else
                                {
                                    offerResponseDC.status = false;
                                    offerResponseDC.msg = "Please Assign customer for channel wise customer";
                                    return offerResponseDC;
                                }
                            }
                        }

                    }


                    if (!string.IsNullOrEmpty(offer.WarehouseIds))
                    {
                        List<BillDiscountFreeItemDc> billDiscountFreeMRPItems = new List<BillDiscountFreeItemDc>();
                        if (offer.BillDiscountOfferOn == "FreeItem")
                        {
                            if (offer.BillDiscountFreeItems != null && offer.BillDiscountFreeItems.Any())
                            {
                                var ItemId = offer.BillDiscountFreeItems.Select(x => x.ItemId).ToList();
                                billDiscountFreeMRPItems = context.itemMasters.Where(x => warehouseids.Contains(x.WarehouseId) && ItemId.Contains(x.ItemId) && !x.Deleted).Select(x => new BillDiscountFreeItemDc { ItemName = x.itemname, ItemId = x.ItemId, ItemMultiMrpId = x.ItemMultiMRPId, MRP = x.price, WarehouseId = x.WarehouseId }).Distinct().ToList();
                                foreach (var item in offer.BillDiscountFreeItems)
                                {
                                    foreach (var warehouse in warehouses)
                                    {
                                        if (!billDiscountFreeMRPItems.Any(x => x.ItemMultiMrpId == item.ItemMultiMrpId && x.WarehouseId == warehouse.WarehouseId))
                                        {
                                            offerResponseDC.status = false;
                                            offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "Free Item MRP " + item.MRP + " not live on " + warehouse.WarehouseName;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                offerResponseDC.status = false;
                                offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "Please add atleast one free item .";

                            }
                        }

                        if (offerResponseDC.status)
                        {
                            int i = 0;
                            foreach (var warehouse in warehouses)
                            {
                                var IsFreeStock = offer.BillDiscountOfferOn == "FreeItem" && offer.BillDiscountFreeItems.FirstOrDefault().StockType == 2;

                                Offer Newoffer = new Offer();
                                Newoffer.CompanyId = compid;
                                Newoffer.StoreId = offer.StoreId;
                                Newoffer.userid = userid;
                                Newoffer.WarehouseId = warehouse.WarehouseId;

                                if (offer.CustomerId > 0)
                                {
                                    Newoffer.ApplyType = "Customer";
                                }//Customer Base
                                else if (offer.GroupId > 0)
                                {
                                    Newoffer.ApplyType = "Group";
                                }//Group Base
                                else if (offer.CustomerId == -1)
                                {
                                    Newoffer.ApplyType = "Warehouse";
                                } // All Warehouse
                                else if (offer.CustomerId == -2)
                                {
                                    Newoffer.ApplyType = "KPPCustomer";
                                } // KPP 
                                else if (offer.CustomerId == -9)
                                {
                                    Newoffer.ApplyType = "PrimeCustomer";
                                }//Prime Customer
                                else if (offer.CustomerId < -2)
                                {
                                    Newoffer.ApplyType = "Level";
                                }//level Customer


                                Newoffer.start = date1;
                                Newoffer.end = date2;
                                Newoffer.itemId = 0;
                                Newoffer.IsDeleted = false;
                                Newoffer.IsActive = offer.IsActive;
                                Newoffer.OfferLogoUrl = "";
                                Newoffer.CreatedDate = indianTime;
                                Newoffer.UpdateDate = indianTime;
                                Newoffer.OfferCode = "";
                                Newoffer.CityId = warehouse.CityId.Value;
                                Newoffer.Description = offer.Description;
                                Newoffer.DiscountPercentage = offer.DiscountPercentage;
                                Newoffer.OfferName = offer.OfferName;
                                Newoffer.OfferWithOtherOffer = false;
                                Newoffer.BillDiscountOfferOn = offer.BillDiscountOfferOn;
                                Newoffer.BillDiscountWallet = offer.BillDiscountWallet;
                                Newoffer.IsMultiTimeUse = offer.IsMultiTimeUse;
                                Newoffer.IsUseOtherOffer = offer.IsUseOtherOffer;
                                Newoffer.OfferOn = offer.OfferOn;
                                Newoffer.FreeOfferType = offer.BillDiscountOfferOn;
                                Newoffer.FreeItemLimit = offer.FreeItemLimit; // add Item limit
                                Newoffer.OfferUseCount = offer.OfferUseCount;
                                Newoffer.OffersaleQty = 0;

                                //Newoffer.Category = offer.Category;
                                //Newoffer.subCategory = offer.subCategory;
                                //Newoffer.subSubCategory = offer.subSubCategory;

                                Newoffer.OfferAppType = offer.OfferAppType;
                                Newoffer.ApplyOn = offer.ApplyOn;
                                Newoffer.WalletType = offer.WalletType;
                                Newoffer.BillAmount = offer.BillAmount;
                                Newoffer.BillDiscountType = offer.BillDiscountType;
                                Newoffer.Description = offer.Description;


                                Newoffer.DistributorDiscountAmount = 0;
                                Newoffer.DistributorDiscountPercentage = 0;
                                Newoffer.DistributorOfferType = false;
                                Newoffer.FreeItemId = 0;
                                Newoffer.FreeItemMRP = 0;
                                Newoffer.FreeItemName = "";
                                //    Newoffer.FreeOfferType = "";
                                Newoffer.FreeWalletPoint = 0;
                                Newoffer.GroupId = offer.GroupId;
                                Newoffer.IsDispatchedFreeStock = IsFreeStock;
                                Newoffer.IsOfferOnCart = false;
                                Newoffer.ItemNumber = "";
                                Newoffer.LineItem = offer.LineItem;
                                Newoffer.MaxBillAmount = offer.MaxBillAmount;
                                Newoffer.MaxDiscount = offer.MaxDiscount;
                                Newoffer.MaxQtyPersonCanTake = 0;
                                Newoffer.MinOrderQuantity = 0;
                                Newoffer.NoOffreeQuantity = 0;
                                Newoffer.OfferAppType = offer.OfferAppType;
                                Newoffer.OfferCategory = "Offer";
                                Newoffer.IsAutoApply = offer.IsAutoApply;
                                Newoffer.IsActive = false;
                                if (offer.ChannelIds != null && offer.ChannelIds != "")
                                {
                                    if (offer.StoreId > 0 && channelidslist.Count() > 0 && channelidslist != null)
                                    {
                                        Newoffer.ApplyType = "Customer";
                                        Newoffer.ChannelIds = offer.ChannelIds;
                                    }
                                }
                                List<OfferScratchWeight> list = new List<OfferScratchWeight>();
                                if (offer.OfferScratchWeights != null && offer.OfferScratchWeights.Any())
                                {
                                    foreach (var item in offer.OfferScratchWeights)
                                    {
                                        OfferScratchWeight obj = new OfferScratchWeight();
                                        obj.WalletPoint = item.WalletPoint;
                                        obj.Weight = item.Weight;
                                        list.Add(obj);
                                    }
                                }
                                Newoffer.OfferScratchWeights = list;

                                Newoffer.ExcludeGroupId = offer.ExcludeGroupId ?? 0;
                                Newoffer.CombinedGroupId = offer.CombinedGroupId ?? 0;

                                List<BillDiscountFreeItem> BillDiscountFreeItems = new List<BillDiscountFreeItem>();
                                if (offer.BillDiscountFreeItems != null && offer.BillDiscountFreeItems.Any())
                                {
                                    foreach (var item in billDiscountFreeMRPItems.Where(x => x.WarehouseId == warehouse.WarehouseId))
                                    {
                                        var freeitem = offer.BillDiscountFreeItems.FirstOrDefault(x => x.ItemMultiMrpId == item.ItemMultiMrpId);
                                        BillDiscountFreeItem obj = new BillDiscountFreeItem();
                                        obj.ItemId = item.ItemId;
                                        obj.ItemMultiMrpId = item.ItemMultiMrpId;
                                        obj.ItemName = item.ItemName;
                                        obj.MRP = item.MRP;
                                        obj.OfferStockQty = freeitem.OfferStockQty;
                                        obj.Qty = freeitem.Qty;
                                        obj.RemainingOfferStockQty = 0;
                                        obj.StockQty = freeitem.StockQty;
                                        obj.StockType = freeitem.StockType;

                                        BillDiscountFreeItems.Add(obj);
                                    }
                                }
                                Newoffer.BillDiscountFreeItems = BillDiscountFreeItems;


                                Newoffer.QtyAvaiable = 0;
                                Newoffer.QtyConsumed = 0;
                                //Newoffer.ApplyType = offer.ApplyType;
                                if (Newoffer.BillDiscountOfferOn != "DynamicWalletPoint")
                                {
                                    Newoffer.OfferScratchWeights = new List<OfferScratchWeight>();
                                }
                                if (OfferBillDiscountRequiredItems != null && OfferBillDiscountRequiredItems.Any())
                                {
                                    Newoffer.OfferBillDiscountRequiredItems = OfferBillDiscountRequiredItems.Where(x => x.WarehouseId == Newoffer.WarehouseId).ToList();
                                }
                                else { Newoffer.OfferBillDiscountRequiredItems = new List<OfferBillDiscountRequiredItem>(); }

                                if (offer.OfferOn == "BillDiscount" && offer.OfferLineItemValues != null && offer.OfferLineItemValues.Any())
                                {
                                    List<OfferLineItemValue> offerLineItemValues = new List<OfferLineItemValue>();
                                    foreach (var item in offer.OfferLineItemValues)
                                    {
                                        OfferLineItemValue reqitem = new OfferLineItemValue
                                        {
                                            itemValue = item.itemValue
                                        };
                                        offerLineItemValues.Add(reqitem);
                                    }
                                    Newoffer.OfferLineItemValues = offerLineItemValues;
                                }
                                else { Newoffer.OfferLineItemValues = new List<OfferLineItemValue>(); }

                                offers.Add(Newoffer);

                            }

                            context.OfferDb.AddRange(offers);
                            string SuccessMsg = "";
                            if (context.Commit() > 0)
                            {
                                var offerbilldiscountitems = new List<OfferItemsBillDiscount>();
                                List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();
                                foreach (var offerdb in offers)
                                {

                                    OfferItemsBillDiscounts.Where(x => x.WarehouseId == offerdb.WarehouseId).ToList().ForEach(x => x.OfferId = offerdb.OfferId);
                                    BillDiscountOfferSections.Where(x => x.WarehouseId == offerdb.WarehouseId).ToList().ForEach(x => x.OfferId = offerdb.OfferId);

                                    string code = "";
                                    if (offerdb.OfferOn == "ScratchBillDiscount")
                                    {
                                        code = "SC_";
                                    }
                                    else if (offerdb.OfferOn == "BillDiscount")
                                    {
                                        code = "BD_";
                                    }
                                    else if (offerdb.OfferOn == "Item")
                                    {
                                        code = "ID_";
                                    }


                                    string offerCode = code + offerdb.OfferId;
                                    offerdb.OfferCode = offerCode;


                                    if (offerdb.OfferOn == "ScratchBillDiscount" || offerdb.OfferOn == "BillDiscount")
                                    {
                                        if (offerdb.OfferId > 0 && offer.CustomerId > 0)
                                        {
                                            double billAmount = 0;
                                            if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            {
                                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                foreach (var item in offerdb.OfferScratchWeights)
                                                {
                                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                }
                                                billAmount = itemDrops.GetRandom();
                                            }

                                            Customer customer = context.Customers.Where(x => x.CustomerId == offer.CustomerId).FirstOrDefault();
                                            BillDiscount BillDiscount = new BillDiscount();
                                            BillDiscount.CustomerId = customer.CustomerId;
                                            BillDiscount.OrderId = 0;
                                            BillDiscount.OfferId = offerdb.OfferId;
                                            BillDiscount.BillDiscountType = offerdb.OfferOn;
                                            if (offerdb.OfferOn == "ScratchBillDiscount")
                                            {
                                                BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                            }
                                            BillDiscount.BillDiscountAmount = 0;
                                            BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                            BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                            BillDiscount.CreatedDate = indianTime;
                                            BillDiscount.ModifiedDate = indianTime;
                                            BillDiscount.IsActive = true;
                                            BillDiscount.IsDeleted = false;
                                            BillDiscount.CreatedBy = offerdb.userid;
                                            BillDiscount.ModifiedBy = offerdb.userid;
                                            BillDiscount.IsScratchBDCode = false;//scratched or not
                                            BillDiscount.Category = offerdb.Category;
                                            BillDiscount.Subcategory = offerdb.subCategory;
                                            BillDiscount.subSubcategory = offerdb.subSubCategory;

                                            context.BillDiscountDb.Add(BillDiscount);

                                        }
                                        else if (offerdb.OfferId > 0 && offerdb.GroupId > 0)
                                        {
                                            //List<GroupMapping> groupmapp = context.SalesGroupCustomerDb.Where(x => x.GroupId == offerdb.GroupId.Value).ToList();
                                            string query = "select distinct a.CustomerID from SalesGroupCustomers a with(nolock) inner join Customers c with(nolock) on a.CustomerID=c.CustomerId and a.IsActive=1 and isnull(a.IsDeleted,0)=0 and a.GroupId=" + offerdb.GroupId + " and c.Warehouseid=" + offerdb.WarehouseId;
                                            List<int> groupmapp = context.Database.SqlQuery<int>(query).ToList();
                                            if (groupmapp.Count > 0)
                                            {
                                                double billAmount = 0;
                                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    foreach (var item in offerdb.OfferScratchWeights)
                                                    {
                                                        itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                    }
                                                }
                                                var customerids = groupmapp.Select(x => x).ToList();
                                                List<BillDiscount> customerdetails = context.BillDiscountDb.Where(x => customerids.Contains(x.CustomerId) && x.OfferId == offerdb.OfferId).ToList();
                                                List<BillDiscount> newbilldiscount = new List<BillDiscount>();
                                                foreach (var custdata in groupmapp)
                                                {
                                                    billAmount = 0;
                                                    if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                    {
                                                        billAmount = itemDrops.GetRandom();
                                                    }
                                                    var customerdetail = customerdetails.FirstOrDefault(x => x.CustomerId == custdata);
                                                    if (customerdetail == null)
                                                    {
                                                        BillDiscount BillDiscount = new BillDiscount();
                                                        BillDiscount.CustomerId = custdata;
                                                        BillDiscount.OrderId = 0;
                                                        BillDiscount.OfferId = offerdb.OfferId;
                                                        BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                        if (offerdb.OfferOn == "ScratchBillDiscount")
                                                        {
                                                            BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                                        }
                                                        BillDiscount.BillDiscountAmount = 0;
                                                        BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                        BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                        BillDiscount.CreatedDate = indianTime;
                                                        BillDiscount.ModifiedDate = indianTime;
                                                        BillDiscount.IsActive = true;
                                                        BillDiscount.IsDeleted = false;
                                                        BillDiscount.CreatedBy = offerdb.userid;
                                                        BillDiscount.ModifiedBy = offerdb.userid;
                                                        BillDiscount.IsScratchBDCode = false;//scratched or not
                                                        newbilldiscount.Add(BillDiscount);
                                                    }
                                                }

                                                if (newbilldiscount != null && newbilldiscount.Any())
                                                {
                                                    var BillDiscountsCustomers = new BulkOperations();
                                                    BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(newbilldiscount))
                                                        .WithTable("BillDiscounts")
                                                        .WithBulkCopyBatchSize(4000)
                                                        .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                        .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                        .AddAllColumns()
                                                        .BulkInsert();
                                                    BillDiscountsCustomers.CommitTransaction("AuthContext");
                                                }

                                                // context.BillDiscountDb.AddRange(newbilldiscount);
                                            }
                                        }
                                        else if (offerdb.OfferId > 0 && (offer.CustomerId == -1 || offer.CustomerId == -2))
                                        {
                                            if (offerdb.StoreId > 0)
                                            {
                                                if (offer.ChannelIds != null && offer.ChannelIds != "")
                                                {
                                                    if (customeridlist.Any() && customeridlist.Count() > 0)
                                                    {
                                                        double billAmount = 0;
                                                        if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                        {
                                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                            foreach (var item in offerdb.OfferScratchWeights)
                                                            {
                                                                itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                            }
                                                            billAmount = itemDrops.GetRandom();
                                                        }

                                                        List<Customer> customers = context.Customers.Where(x => customeridlist.Contains(x.CustomerId)).ToList();
                                                        List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                                                        foreach (var customer in customers)
                                                        {
                                                            BillDiscount BillDiscount = new BillDiscount();
                                                            BillDiscount.CustomerId = customer.CustomerId;
                                                            BillDiscount.OrderId = 0;
                                                            BillDiscount.OfferId = offerdb.OfferId;
                                                            BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                            if (offerdb.OfferOn == "ScratchBillDiscount")
                                                            {
                                                                BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                                            }
                                                            BillDiscount.BillDiscountAmount = 0;
                                                            BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                            BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                            BillDiscount.CreatedDate = indianTime;
                                                            BillDiscount.ModifiedDate = indianTime;
                                                            BillDiscount.IsActive = true;
                                                            BillDiscount.IsDeleted = false;
                                                            BillDiscount.CreatedBy = offerdb.userid;
                                                            BillDiscount.ModifiedBy = offerdb.userid;
                                                            BillDiscount.IsScratchBDCode = false;//scratched or not
                                                            BillDiscount.Category = offerdb.Category;
                                                            BillDiscount.Subcategory = offerdb.subCategory;
                                                            BillDiscount.subSubcategory = offerdb.subSubCategory;

                                                            BillDiscounts.Add(BillDiscount);
                                                        }

                                                        if (BillDiscounts != null && BillDiscounts.Any())
                                                        {
                                                            var BillDiscountsCustomers = new BulkOperations();
                                                            BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(BillDiscounts))
                                                                .WithTable("BillDiscounts")
                                                                .WithBulkCopyBatchSize(4000)
                                                                .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                                .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                                .AddAllColumns()
                                                                .BulkInsert();
                                                            BillDiscountsCustomers.CommitTransaction("AuthContext");
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        else if (offer.CustomerId < -2)
                                        {
                                            int Level = -1;
                                            switch (offerdb.CustomerId)
                                            {
                                                case -3:
                                                    Level = 0;
                                                    break;
                                                case -4:
                                                    Level = 1;
                                                    break;
                                                case -5:
                                                    Level = 2;
                                                    break;
                                                case -6:
                                                    Level = 3;
                                                    break;
                                                case -7:
                                                    Level = 4;
                                                    break;
                                                case -8:
                                                    Level = 5;
                                                    break;
                                            }

                                            var fromdate = DateTime.Now;

                                            fromdate = DateTime.Now.AddMonths(-1);
                                            string query = "Select distinct a.CustomerId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + offerdb.WarehouseId + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                            List<int> customerids = context.Database.SqlQuery<int>(query).ToList();


                                            double billAmount = 0;
                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                            if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            {
                                                foreach (var item in offerdb.OfferScratchWeights)
                                                {
                                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                }
                                            }
                                            List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                                            foreach (var item in customerids)
                                            {
                                                billAmount = 0;
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    billAmount = itemDrops.GetRandom();
                                                }
                                                BillDiscount BillDiscount = new BillDiscount();
                                                BillDiscount.CustomerId = item;
                                                BillDiscount.OrderId = 0;
                                                BillDiscount.OfferId = offerdb.OfferId;
                                                BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                BillDiscount.BillDiscountTypeValue = billAmount;
                                                BillDiscount.BillDiscountAmount = 0;
                                                BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                BillDiscount.CreatedDate = indianTime;
                                                BillDiscount.ModifiedDate = indianTime;
                                                BillDiscount.IsActive = true;
                                                BillDiscount.IsDeleted = false;
                                                BillDiscount.CreatedBy = offerdb.userid;
                                                BillDiscount.ModifiedBy = offerdb.userid;
                                                BillDiscount.IsScratchBDCode = false;//scratched or not
                                                BillDiscount.Category = offerdb.Category;
                                                BillDiscount.Subcategory = offerdb.subCategory;
                                                BillDiscount.subSubcategory = offerdb.subSubCategory;
                                                BillDiscounts.Add(BillDiscount);
                                            }

                                            if (BillDiscounts != null && BillDiscounts.Any())
                                            {
                                                var BillDiscountsCustomers = new BulkOperations();
                                                BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(BillDiscounts))
                                                    .WithTable("BillDiscounts")
                                                    .WithBulkCopyBatchSize(4000)
                                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                    .AddAllColumns()
                                                    .BulkInsert();
                                                BillDiscountsCustomers.CommitTransaction("AuthContext");
                                            }

                                        }

                                    }

                                    var wh = warehouses.FirstOrDefault(x => x.WarehouseId == offerdb.WarehouseId);
                                    SuccessMsg += (string.IsNullOrEmpty(SuccessMsg) ? "" : "\n") + wh.WarehouseName + " : " + offerdb.OfferCode;


                                    context.Entry(offerdb).State = EntityState.Modified;
                                }

                                if (OfferItemsBillDiscounts.Any())
                                    context.OfferItemsBillDiscountDB.AddRange(OfferItemsBillDiscounts);
                                //if (OfferItemsBillDiscounts.Any())
                                if (BillDiscountOfferSections.Any())
                                    context.BillDiscountOfferSectionDB.AddRange(BillDiscountOfferSections);

                                if (context.Commit() > 0)
                                {
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = true;
                                    offerResponseDC.msg = "Offer Added Successfully Initially InActive With Below Details: \n" + SuccessMsg;
                                }
                                else
                                {
                                    foreach (var offerdb in offers)
                                    {
                                        offerdb.IsActive = false;
                                        offerdb.IsDeleted = true;
                                        context.Entry(offerdb).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = false;
                                    offerResponseDC.msg = "Offer not added.";
                                }

                            }
                            else
                            {
                                offerResponseDC.Offer = null;
                                offerResponseDC.status = false;
                                offerResponseDC.msg = "Offer not added.";
                            }
                        }

                    }
                    else
                    {
                        offerResponseDC.Offer = null;
                        offerResponseDC.status = false;
                        offerResponseDC.msg = "Please select atleast one warehouse to add offer";
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error during add Offers:" + ex.ToString());
                    offerResponseDC.msg = "Some error occurred during save data.";
                    offerResponseDC.status = false;
                }


            }
            return offerResponseDC;
        }


        private string ValidateSheet(ISheet sheet, List<OfferColumn> Columns)
        {
            string errorMsg = "";
            IRow rowData;
            ICell cellData = null;
            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
            {
                rowData = sheet.GetRow(iRowIdx);
                if (iRowIdx == 0)
                {
                    if (rowData != null)
                    {
                        foreach (var col in Columns.Select(x => x.ColumnName))
                        {
                            if (!rowData.Cells.Any(x => x.ToString().Trim() == col))
                            {
                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col + " Does not exist in " + sheet.SheetName + " Sheet";
                            }
                        }
                    }
                }
                else
                {
                    if (rowData != null)
                    {
                        foreach (var col in Columns)
                        {
                            if (rowData.Cells.Any(x => x.ToString().Trim() == col.ColumnName))
                            {
                                cellData = rowData.GetCell(rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == col.ColumnName).ColumnIndex);

                                if (col.IsRequired && cellData == null && string.IsNullOrEmpty(cellData.ToString()))
                                {
                                    errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value Required for " + sheet.SheetName + " Sheet";
                                }
                                else
                                {
                                    try
                                    {
                                        if (col.IsRequired || (col.RequiredValues != null && col.RequiredValues.Any()))
                                        {
                                            var colValue = Convert.ChangeType(cellData.ToString(), col.DataType);
                                            if (col.RequiredValues != null && !col.RequiredValues.Any(x => x == colValue.ToString()))
                                            {
                                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value may be " + string.Join(",", col.RequiredValues) + " for " + sheet.SheetName + " Sheet";
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value type mismatch for " + sheet.SheetName + " Sheet";
                                    }
                                }
                            }
                            else if (col.IsRequired)
                            {
                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value Required for " + sheet.SheetName + " Sheet";
                            }
                        }
                    }
                }
            }
            return errorMsg;
        }
    }
}
