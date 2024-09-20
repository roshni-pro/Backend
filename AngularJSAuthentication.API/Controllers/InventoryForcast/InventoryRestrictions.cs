using AngularJSAuthentication.DataContracts.ForCast;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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

namespace AngularJSAuthentication.API.Controllers.InventoryForcast
{
    [RoutePrefix("api/InventoryRestrictions")]
    public class InventoryRestrictionsController : ApiController
    {
        string strJSON = null;

        [HttpPost]
        [Route("InventoryRestrictionGet")]
        public async Task<InventoryRestrictionResponsesDC> InventoryRestriction(Request_InventoryRestriction InvRestriction)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var IdwarehouseId = new DataTable();
                IdwarehouseId.Columns.Add("IntValue");
                foreach (var item in InvRestriction.Stores)
                {
                    var dr = IdwarehouseId.NewRow();
                    dr["IntValue"] = item;
                    IdwarehouseId.Rows.Add(dr);
                }
                var param0 = new SqlParameter("warehouseId", IdwarehouseId);
                param0.SqlDbType = SqlDbType.Structured;
                param0.TypeName = "dbo.IntValues";


                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in InvRestriction.Stores)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var param1 = new SqlParameter("StoreIds", IdDt);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.IntValues";


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetInvRestriction]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //cmd.Parameters.Add(new SqlParameter("@warehouseId", InvRestriction.warehouseId));
                cmd.Parameters.Add(param0);
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(new SqlParameter("@SearchKey", InvRestriction.SearchKey));
                int Skiplist = (InvRestriction.skip - 1) * InvRestriction.take;
                cmd.Parameters.Add(new SqlParameter("@skip", Skiplist));
                cmd.Parameters.Add(new SqlParameter("@take", InvRestriction.take));
                cmd.CommandTimeout = 1200;

                //// Run the sproc
                //List<InventoryRestrictionListDc> InventoryRestrictionData = new List<InventoryRestrictionListDc>();
                //InventoryRestrictionData = context.Database.SqlQuery<InventoryRestrictionListDc>("EXEC GetInvRestriction @warehouseId,@StoreIds,@SearchKey,@Skip,@Take", gname, Keytype, startDate, Skip, Take).ToList();
                //if (InventoryRestrictionData != null && InventoryRestrictionData.Any())
                //{
                //    return InventoryRestrictionData;
                //}
                //return InventoryRestrictionData;

                InventoryRestrictionResponsesDC Responses_InventoryRestriction = new InventoryRestrictionResponsesDC();
                List<InventoryRestrictionListDc> Responses_InventoryRestrictionList = new List<InventoryRestrictionListDc>();

                // Run the sproc
                var reader = cmd.ExecuteReader();
                Responses_InventoryRestrictionList = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<InventoryRestrictionListDc>(reader).ToList();
                reader.NextResult();
                Responses_InventoryRestriction.InventoryRestrictionList = Responses_InventoryRestrictionList;
                if (reader.Read())
                {
                    Responses_InventoryRestriction.TotalRecord = Convert.ToInt32(reader["TotalRecord"]);
                }
                return Responses_InventoryRestriction;

            }
        }


        [Route("InventoryRestrictionUpdate")]
        [HttpGet]
        public async Task<InventoryRestrictionUpdateStatusResponse> UpdateInvRestriction(int Id, int? NoOfInventoryDays)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            InventoryRestrictionUpdateStatusResponse InventoryRestrictionUpdateResponse = new InventoryRestrictionUpdateStatusResponse();

            using (var authContext = new AuthContext())
            {
                var TblInventoryRestriction= authContext.InventoryRestrictionDB.FirstOrDefault(x => x.Id == Id);

                if (TblInventoryRestriction != null)
                {

                    if (NoOfInventoryDays >= 0)
                    {
                        if (NoOfInventoryDays != TblInventoryRestriction.NoOfInvDays)
                        {
                            TblInventoryRestriction.NoOfInvDays = NoOfInventoryDays;
                            TblInventoryRestriction.ModifiedBy = userid;
                            TblInventoryRestriction.ModifiedDate = DateTime.Now;

                            authContext.Entry(TblInventoryRestriction).State = EntityState.Modified;
                        }

                        if (authContext.Commit() > 0)
                        {
                            InventoryRestrictionUpdateResponse.Status = true;
                            InventoryRestrictionUpdateResponse.msg = "No Of Inventory Days Updated successfully.";
                        }
                        else
                        {
                            InventoryRestrictionUpdateResponse.Status = false;
                            InventoryRestrictionUpdateResponse.msg = "No Of Inventory Days Data Not Updated.";
                        }
                    }
                }
                else
                {
                    InventoryRestrictionUpdateResponse.Status = false;
                    InventoryRestrictionUpdateResponse.msg = "Update Request Brand Record not Found.";
                }

            }
            return InventoryRestrictionUpdateResponse;
        }



        //[HttpPost]
        //[Route("InventoryRestrictionUploadFile")]
        //public IHttpActionResult UploadFile()
        //{
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        var formData1 = HttpContext.Current.Request.Form["compid"];
        //        // Access claims
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        // Get the uploaded image from the Files collection
        //        var httpPostedFile = HttpContext.Current.Request.Files["file"];

        //        if (httpPostedFile != null)
        //        {
        //            string ext = Path.GetExtension(httpPostedFile.FileName);
        //            if (ext == ".xlsx")
        //            {
        //                byte[] buffer = new byte[httpPostedFile.ContentLength];

        //                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
        //                {
        //                    br.Read(buffer, 0, buffer.Length);
        //                }
        //                XSSFWorkbook hssfwb;
        //                //   XSSFWorkbook workbook1;
        //                using (MemoryStream memStream = new MemoryStream())
        //                {
        //                    BinaryFormatter binForm = new BinaryFormatter();
        //                    memStream.Write(buffer, 0, buffer.Length);
        //                    memStream.Seek(0, SeekOrigin.Begin);
        //                    hssfwb = new XSSFWorkbook(memStream);
        //                }
        //                return ReadInveRestrictionUploadedFile(hssfwb, userid);
        //            }
        //            else
        //            {
        //                return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
        //            }
        //        }
        //    }

        //    return Created("Error", "Error");
        //}



        //public IHttpActionResult ReadInveRestrictionUploadedFile(XSSFWorkbook hssfwb, int userid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        string Msg = string.Empty;
        //        string sSheetName = hssfwb.GetSheetName(0);
        //        ISheet sheet = hssfwb.GetSheet(sSheetName);
        //        IRow rowData;
        //        ICell cellData = null;
        //        try
        //        {
        //            List<BuyersForecastUploadedFileDc> BuyersForecastUploader = new List<BuyersForecastUploadedFileDc>();
        //            int? CategoryNameCellIndex = null;
        //            int? StoreNameCellIndex = null;
        //            int? SubCategoryNameCellIndex = null;
        //            int? BrandNameCellIndex = null;
        //            int? NoOfInvDaysCellIndex = null;
        //            List<string> headerlst = new List<string>();
        //            List<BuyerForecastUploderDetail> Addlist = new List<BuyerForecastUploderDetail>();
        //            List<BuyersForecastUploadedFileDc> trnfrorders = new List<BuyersForecastUploadedFileDc>();
        //            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
        //            {
        //                if (iRowIdx == 0)
        //                {
        //                    rowData = sheet.GetRow(iRowIdx);
        //                    if (rowData != null)
        //                    {
        //                        foreach (var item in rowData.Cells)
        //                        {
        //                            headerlst.Add(item.ToString());
        //                        }
        //                        StoreNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Store_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Store_Name").ColumnIndex : (int?)null;
        //                        if (!StoreNameCellIndex.HasValue)
        //                        {
        //                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Store_Name does not exist..try again");
        //                            return Created(strJSON, strJSON); ;
        //                        }//changes

        //                        CategoryNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Category_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category_Name").ColumnIndex : (int?)null;
        //                        if (!CategoryNameCellIndex.HasValue)
        //                        {
        //                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
        //                            return Created(strJSON, strJSON); ;
        //                        }

        //                        SubCategoryNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Sub_Category_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Sub_Category_Name").ColumnIndex : (int?)null;
        //                        if (!SubCategoryNameCellIndex.HasValue)
        //                        {
        //                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sub Category does not exist..try again");
        //                            return Created(strJSON, strJSON); ;
        //                        }

        //                        BrandNameCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Brand_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand_Name").ColumnIndex : (int?)null;
        //                        if (!BrandNameCellIndex.HasValue)
        //                        {
        //                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
        //                            return Created(strJSON, strJSON);
        //                        }

        //                        NoOfInvDaysCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "No_Of_Inventory_Days_Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "No_Of_Inventory_Days_Name").ColumnIndex : (int?)null;
        //                        if (!NoOfInvDaysCellIndex.HasValue)
        //                        {
        //                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
        //                            return Created(strJSON, strJSON);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    rowData = sheet.GetRow(iRowIdx);
        //                    cellData = rowData.GetCell(0);
        //                    rowData = sheet.GetRow(iRowIdx);
        //                    if (rowData != null)
        //                    {

        //                        BuyersForecastUploadedFileDc trnfrorder = new BuyersForecastUploadedFileDc();
        //                        string dept = string.Empty, Category = string.Empty, brand = string.Empty; string group = string.Empty;
        //                        try
        //                        {
        //                            foreach (var cellDatas in rowData.Cells)
        //                            {
        //                                trnfrorder = new BuyersForecastUploadedFileDc();
        //                                if (cellDatas.ColumnIndex < 4)
        //                                {
        //                                    if (cellDatas.ColumnIndex == 0)
        //                                        group = cellDatas.ToString();
        //                                    else if (cellDatas.ColumnIndex == 1)
        //                                        dept = cellDatas.ToString();
        //                                    else if (cellDatas.ColumnIndex == 2)
        //                                        Category = cellDatas.ToString();
        //                                    else if (cellDatas.ColumnIndex == 3)
        //                                        brand = cellDatas.ToString();
        //                                }
        //                                else
        //                                {
        //                                    trnfrorder.Group = group;
        //                                    trnfrorder.Dept = dept;
        //                                    trnfrorder.Category = Category;
        //                                    trnfrorder.Brand = brand;
        //                                    trnfrorder.WarehouseName = headerlst[cellDatas.ColumnIndex];
        //                                    trnfrorder.PercentValue = Convert.ToDouble(cellDatas.ToString());
        //                                    trnfrorders.Add(trnfrorder);
        //                                }
        //                            }
        //                            BuyersForecastUploader.Add(trnfrorder);


        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            //msgitemname = ex.Message;
        //                            //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                        }
        //                    }
        //                }
        //            }

        //            if (trnfrorders != null && trnfrorders.Any())
        //            {
        //                var validgroup = trnfrorders.GroupBy(x => x.Group).Select(x => new BuyersGroupWisePerValDc
        //                {
        //                    GroupName = x.Key

        //                }).ToList();
        //                if (validgroup.Count == 1)
        //                {

        //                    var validexcel = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new BuyersBrandWisePerValDc
        //                    {
        //                        WarehouseName = x.Key,
        //                        TotalPercentValue = Math.Round(x.Sum(y => y.PercentValue))//x.Sum(y => y.PercentValue)
        //                    }).ToList();
        //                    foreach (var i in validexcel)
        //                    {
        //                        if (i.TotalPercentValue < 100)
        //                        {
        //                            Msg = i.WarehouseName + " PercentValue is not 100% ";
        //                            return Created(Msg, Msg);
        //                        }
        //                        if (i.TotalPercentValue > 100)
        //                        {
        //                            Msg = i.WarehouseName + " PercentValue should  not greater than 100% ";
        //                            return Created(Msg, Msg);
        //                        }
        //                    }

        //                    var BuyerForecastUploderDbs = context.BuyerForecastUploderDb.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                    if (BuyerForecastUploderDbs != null)
        //                    {
        //                        BuyerForecastUploderDbs.IsActive = false;
        //                        BuyerForecastUploderDbs.IsDeleted = true;
        //                        context.Entry(BuyerForecastUploderDbs).State = EntityState.Modified;
        //                    }
        //                    var BuyerForecastUploderDetailDbs = context.BuyerForecastUploderDetailDb.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year && x.IsActive == true && x.IsDeleted == false).ToList();
        //                    if (BuyerForecastUploderDetailDbs != null && BuyerForecastUploderDetailDbs.Any())
        //                    {
        //                        BuyerForecastUploderDetailDbs.ForEach(x =>
        //                        {
        //                            x.IsActive = false;
        //                            x.IsDeleted = true;
        //                            context.Entry(x).State = EntityState.Modified;
        //                        });
        //                    }

        //                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/BuyerforecastUpload");
        //                    if (!Directory.Exists(ExcelSavePath))
        //                        Directory.CreateDirectory(ExcelSavePath);

        //                    string fileUrl = "";
        //                    var BuyerForcastDt = ClassToDataTable.CreateDataTable(trnfrorders);
        //                    BuyerForcastDt.TableName = "Buyerforecast Upload Data";
        //                    var fileName = "BuyerforecastSheet" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
        //                    string filePath = ExcelSavePath + "\\" + fileName;
        //                    ExcelGenerator.DataTable_To_Excel(new List<DataTable> { BuyerForcastDt }, filePath);
        //                    fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
        //                                                            , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                            , HttpContext.Current.Request.Url.Port
        //                                                            , string.Format("UploadedFiles/BuyerforecastUpload/{0}", fileName));


        //                    var buyerForecastUploder = context.BuyerForecastUploderDb.Add(new BuyerForecastUploder
        //                    {
        //                        FilePath = fileUrl,
        //                        Status = 1,     //1 In Process
        //                        CreatedDate = DateTime.Now,
        //                        ModifiedDate = null,
        //                        IsActive = true,
        //                        IsDeleted = false,
        //                        CreatedBy = userid,
        //                        ModifiedBy = 0
        //                    });
        //                    var WarehouseList = trnfrorders.Select(x => x.WarehouseName).Distinct().ToList();
        //                    var WarehouseNameList = context.Warehouses.Where(x => WarehouseList.Contains(x.WarehouseName)).Select(x => x.WarehouseName).ToList();
        //                    var Grouplist = trnfrorders.Select(x => x.Group).Distinct().ToList();
        //                    var Groupnamelist = context.StoreDB.Where(x => x.Name.Contains(x.Name)).Select(x => x.Name).ToList();
        //                    string query = "EXEC GetSubcategoryCategoryMappings ";
        //                    var ValidList = context.Database.SqlQuery<GetSubcategoryCategoryMappingsDC>(query).ToList();
        //                    int status = 2;
        //                    trnfrorders.ForEach(x =>
        //                    {
        //                        string ErrorMsgs = "";
        //                        BuyerForecastUploderDetail buyerForecastUploderDetail = new BuyerForecastUploderDetail();
        //                        buyerForecastUploderDetail.UplodeId = buyerForecastUploder.Id;
        //                        buyerForecastUploderDetail.GroupName = x.Group;
        //                        buyerForecastUploderDetail.BrandName = x.Brand;
        //                        buyerForecastUploderDetail.WarehouseName = x.WarehouseName;
        //                        buyerForecastUploderDetail.CateName = x.Category;
        //                        buyerForecastUploderDetail.BaseCateName = x.Dept;
        //                        buyerForecastUploderDetail.PercentValue = x.PercentValue;


        //                        if (!ValidList.Any(y => y.CategoryName == x.Category))
        //                        {
        //                            ErrorMsgs = x.Category + " Category Not Found";
        //                            status = 3;
        //                        }
        //                        if (!ValidList.Any(y => y.BaseCategoryName == x.Dept))
        //                        {
        //                            ErrorMsgs = ErrorMsgs + "~" + x.Dept + " BaseCategoryName Not Found";
        //                            status = 3;
        //                        }
        //                        if (!ValidList.Any(y => y.SubsubcategoryName == x.Brand))
        //                        {
        //                            ErrorMsgs = ErrorMsgs + "~" + x.Brand + " Brand Not Found";
        //                            status = 3;
        //                        }
        //                        if (!WarehouseNameList.Any(y => y == x.WarehouseName))
        //                        {
        //                            ErrorMsgs = ErrorMsgs + "~" + x.WarehouseName + " WarehouseName Not Found";
        //                            status = 3;
        //                        }
        //                        if (!Groupnamelist.Any(Z => Z == x.Group))
        //                        {
        //                            ErrorMsgs = ErrorMsgs + "~" + x.Group + " GroupName Not Found";
        //                            status = 3;
        //                        }
        //                        buyerForecastUploderDetail.ErrorMsg = ErrorMsgs;
        //                        buyerForecastUploderDetail.CreatedDate = buyerForecastUploder.CreatedDate;
        //                        buyerForecastUploderDetail.IsActive = true;
        //                        buyerForecastUploderDetail.IsDeleted = false;
        //                        buyerForecastUploderDetail.CreatedBy = userid;
        //                        Addlist.Add(buyerForecastUploderDetail);
        //                    });
        //                    context.BuyerForecastUploderDetailDb.AddRange(Addlist);
        //                    context.Commit();
        //                    //var buyerForecastUploderDetailDbss = context.BuyerForecastUploderDetailDb.Where(x => x.UplodeId == buyerForecastUploder.Id && x.ErrorMsg != "").FirstOrDefault();
        //                    if (buyerForecastUploder.Id > 0)
        //                    {
        //                        var BuyerForecastUploderDbss = context.BuyerForecastUploderDb.Where(x => x.Id == buyerForecastUploder.Id).FirstOrDefault();
        //                        BuyerForecastUploderDbss.Status = status;

        //                        context.Entry(BuyerForecastUploderDbss).State = EntityState.Modified;
        //                        context.Commit();
        //                        if (status == 2)
        //                        {
        //                            var JObData = context.Database.ExecuteSqlCommand("EXEC InsertBrandForecastValues");
        //                        }
        //                    }

        //                    Msg = "Record Insert Successfully!!";
        //                }
        //                else
        //                {
        //                    Msg = "Multiple Group names are inserting  ";
        //                }
        //            }

        //            else
        //            {
        //                Msg = "No Record Found!!";
        //            }
        //            return Created(Msg, Msg);

        //        }
        //        catch (Exception ex)
        //        {

        //            //logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //        }
        //        return Created("Error", "Error");
        //    }
        //}






        public class Request_InventoryRestriction
        {
            //public int warehouseId { get; set; }
            public List<int> warehouseId { get; set; }
            public List<int> Stores { get; set; }
            public string SearchKey { get; set; }
            public int skip { get; set; }
            public int take { get; set; }

        }

    }
}