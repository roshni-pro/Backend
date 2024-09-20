using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using AngularJSAuthentication.Model;
using NPOI.SS.UserModel;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using AngularJSAuthentication.DataContracts.FileUpload;
using AngularJSAuthentication.Common.Helpers;
using System.Data.Entity;

namespace AngularJSAuthentication.API.Controllers.PTRItemScheme
{
    public class UploadItemSchemController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Uploader
        [AllowAnonymous]
        [Route("UploadItemSchemExcel")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadItemSchemExcel(bool IsCompanyCode, bool IsCompanyStockCode, int SubCatId, int SubSubCatId)
        {
            var msg = "";
            logger.Info("start Upload Exel File: ");
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];
                var Start = HttpContext.Current.Request.Form["StartDate"];
                var End = HttpContext.Current.Request.Form["EndDate"];
                //var CityIdis = HttpContext.Current.Request.Form["CityIds"];
                //List<int> CityIds = CityIdis.Split(',').Select(int.Parse).ToList();
                if (httpPostedFile != null && userid > 0) //&& SubCatId > 0
                {
                    var SaveFileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel"), httpPostedFile.FileName);
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel")))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel"));
                    }
                    httpPostedFile.SaveAs(SaveFileUrl);
                    var uploader = new List<Uploader> { new Uploader() };
                    uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                    uploader.FirstOrDefault().RelativePath = "~/UploadedFiles/ItemSchemExcel";
                    uploader.FirstOrDefault().SaveFileURL = SaveFileUrl;
                    uploader = await FileUploadHelper.UploadFileToOtherApi(uploader);
                    DateTime StartDate = DateTime.ParseExact(Start.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime EndDate = DateTime.ParseExact(End.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    byte[] buffer = new byte[httpPostedFile.ContentLength];
                    using (BinaryReader br = new BinaryReader(File.OpenRead(SaveFileUrl)))
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
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);
                        IRow rowData;
                        ICell cellData = null;
                        bool IsUploaded = false;

                        List<ItemSchemeExcelUploaderDetail> FinalAddItemSchemeUploaders = new List<ItemSchemeExcelUploaderDetail>();

                        using (AuthContext context = new AuthContext())
                        {
                            var people = context.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
                            //var Citylist = context.Cities.Where(x => CityIds.Contains(x.Cityid)).ToList();
                            if (people != null)
                            {

                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {
                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            string Validatedheader = ValidateHeader(rowData);
                                            if (Validatedheader != null)
                                            {
                                                return Ok(Validatedheader);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ItemSchemeExcelUploaderDetail AddItemSchemeUploaders = new ItemSchemeExcelUploaderDetail();
                                        rowData = sheet.GetRow(iRowIdx);
                                        string col = null;
                                        //cellData = rowData.GetCell(0);
                                        //col = cellData == null ? "" : cellData.ToString();
                                        //if (col != "")
                                        //    AddItemSchemeUploaders.EANNo = col.Trim();
                                        cellData = rowData.GetCell(0);

                                        col = cellData == null ? null : cellData.ToString();
                                        if (col != null)
                                        {
                                            AddItemSchemeUploaders.ClaimType = col.Trim();
                                        }

                                        cellData = rowData.GetCell(1);

                                        col = cellData == null ? null : cellData.ToString();
                                        if (col != null)
                                        {
                                            AddItemSchemeUploaders.CompanyCode = col.Trim();
                                        }


                                        col = string.Empty;
                                        cellData = rowData.GetCell(2);
                                        col = cellData == null ? null : cellData.ToString();

                                        if (col != null)
                                        {
                                            AddItemSchemeUploaders.CompanyStockCode = col.Trim();
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(3);
                                        col = cellData == null ? "" : cellData.ToString();
                                        AddItemSchemeUploaders.ItemName = col.Trim();

                                        col = string.Empty;
                                        cellData = rowData.GetCell(4);
                                        var itemCode = "";
                                        if (cellData == null)
                                        {
                                            msg = "itemCode field does not exist..try again";
                                            return Ok(msg);
                                        }
                                        else
                                        {
                                            col = cellData == null ? "" : cellData.ToString();
                                            if (col != "")
                                            {
                                                AddItemSchemeUploaders.ItemCode = col.Trim();
                                                itemCode = col.Trim();//mendatory
                                            }
                                        }

                                        var ItemMultiMRPId = 0; //mendatory
                                        col = string.Empty;
                                        cellData = rowData.GetCell(5);
                                        if (cellData == null)
                                        {
                                            msg = "ItemMultiMRPId field does not exist..try again";
                                            return Ok(msg);
                                        }
                                        else
                                        {
                                            col = cellData == null ? "" : cellData.ToString();
                                            if (col != "")
                                            {
                                                AddItemSchemeUploaders.ItemMultiMRP = Convert.ToInt32(col);
                                                ItemMultiMRPId = Convert.ToInt32(col);
                                            }
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(6);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.MRP = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(7);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.GST = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(8);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.PTR = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(9);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.TOTOnInvoice = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(10);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.Category = col.Trim();
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(11);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "" && col != "-")
                                        {
                                            AddItemSchemeUploaders.Subcategory = col.Trim();
                                        }
                                        else
                                        {
                                                msg = "Subcategory does not exist ItemMultiMRP:- "+ AddItemSchemeUploaders.ItemMultiMRP+ " ..try again";
                                                return Ok(msg);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(12);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.Warehouse = col.Trim();
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(13);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.Salesoffice = col.Trim();
                                        }

                                        //col = string.Empty;
                                        //cellData = rowData.GetCell(12);
                                        //col = cellData == null ? "" : cellData.ToString();
                                        //if (col != "")
                                        //{
                                        //    AddItemSchemeUploaders.City = col.Trim();
                                        //}
                                       
                                        col = string.Empty;
                                        cellData = rowData.GetCell(14);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.BaseScheme = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(15);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.PrimaryScheme = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(16);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabPurchaseQTY1 = Convert.ToInt32(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(17);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabScheme1 = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(18);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabPurchaseQTY2 = Convert.ToInt32(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(19);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabScheme2 = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(20);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabPurchaseQTY3 = Convert.ToInt32(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(21);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabScheme3 = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(22);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabPurchaseQTY4 = Convert.ToInt32(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(23);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.SlabScheme4 = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(24);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.QPSValueTarget = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(25);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.QPSValue = Convert.ToDouble(col);
                                        }

                                        col = string.Empty;
                                        cellData = rowData.GetCell(26);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.QPSQtyTarget = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(27);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.QPSQty = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(28);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.Promo = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(29);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.VisibilityPercentage = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(30);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.KVIANDNonKVI = col.Trim();
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(31);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.CustomerType = col.Trim();
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(32);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.AdditionalScheme = Convert.ToDouble(col);
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(33);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.LiquidationSupport = col.Trim();
                                        }
                                        col = string.Empty;
                                        cellData = rowData.GetCell(34);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.offinvoicemargin = Convert.ToDouble(col);

                                        }
                                        //---------------------------
                                        col = string.Empty;
                                        cellData = rowData.GetCell(35);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.StartDate = Convert.ToDateTime(col);
                                        }
                                        else 
                                            AddItemSchemeUploaders.StartDate = DateTime.Today;

                                        col = string.Empty;
                                        cellData = rowData.GetCell(36);
                                        col = cellData == null ? "" : cellData.ToString();
                                        if (col != "")
                                        {
                                            AddItemSchemeUploaders.EndDate = Convert.ToDateTime(col);
                                        }
                                        //------------------------------------

                                        FinalAddItemSchemeUploaders.Add(AddItemSchemeUploaders);
                                    }
                                }
                                var warehouse = FinalAddItemSchemeUploaders.Select(x => x.Warehouse.Trim()).Distinct().ToList();
                                var cityWarehouseList = context.Warehouses.Where(x => warehouse.Contains(x.WarehouseName) && x.active == true && x.Deleted == false).Select(x => new { x.Cityid,x.CityName,x.WarehouseId, x.WarehouseName }).Distinct().ToList();
                                
                                if(cityWarehouseList.Count() != warehouse.Count())
                                {
                                    string WarehouseNotExists = string.Join(",", warehouse.Where(x => !cityWarehouseList.Select(y=>y.WarehouseName).ToList().Contains(x)).Select(x=>x).ToList());
                                    return Ok("Warehouse does Not Exist:- "+ WarehouseNotExists);
                                }

                                //var CItyNameList = cityWarehouseList.Select(x => x.CityName).Distinct().ToList();
                                //if (CItyNameList.Count() > Citylist.Count())
                                //{
                                //    var cityname = string.Join(",",CItyNameList.Where(x=> !(Citylist.Select(y=>y.CityName).ToList().Contains(x))).ToList());
                                //    return Ok("City does Not Exist Please Select City :- " + cityname);
                                //}

                                FinalAddItemSchemeUploaders.ForEach(x=> {
                                    var data = cityWarehouseList.Where(y => y.WarehouseName.Trim() == x.Warehouse.Trim()).Select(y=>new { y.Cityid,y.CityName,y.WarehouseId }).FirstOrDefault();
                                    x.CityId = data.Cityid;
                                    x.City = data.CityName;
                                    x.WarehouseId = data.WarehouseId;
                                });

                                //var DistinctCTs = FinalAddItemSchemeUploaders.Where(x => x.City != null).Select(x => x.City).Distinct().ToList();

                                if (FinalAddItemSchemeUploaders != null && FinalAddItemSchemeUploaders.Any())
                                {

                                    List<ItemSchemeExcelUploaderMaster> InsertData = new List<ItemSchemeExcelUploaderMaster>();

                                    //foreach (var item in DistinctCTs)
                                    //{

                                        //if (FinalAddItemSchemeUploaders.Any(x => x.City != null && x.City.Trim().ToLower() == item.Trim().ToLower()) != null && FinalAddItemSchemeUploaders.Any(x => x.City.Trim().ToLower() == item.Trim().ToLower()))
                                        //{

                                            ItemSchemeExcelUploaderMaster ItemSchemeExcelUploaderMaster = new ItemSchemeExcelUploaderMaster();
                                            ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails = new List<ItemSchemeExcelUploaderDetail>();
                                            ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails = FinalAddItemSchemeUploaders.Any() ? FinalAddItemSchemeUploaders.ToList() : null;

                                            if (ItemSchemeExcelUploaderMaster != null && ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails.Any())
                                            {
                                                ItemSchemeExcelUploaderMaster.SubSubCatId = 0;
                                                ItemSchemeExcelUploaderMaster.SubCatId = Convert.ToInt32(SubCatId);
                                                ItemSchemeExcelUploaderMaster.Cityid = 0;
                                                ItemSchemeExcelUploaderMaster.CreatedBy = people.PeopleID;
                                                ItemSchemeExcelUploaderMaster.StartDate = FinalAddItemSchemeUploaders.Any(x=>x.StartDate != null) ? FinalAddItemSchemeUploaders.Select(x=>(DateTime)x.StartDate).FirstOrDefault() : DateTime.Today;
                                                ItemSchemeExcelUploaderMaster.EndDate = FinalAddItemSchemeUploaders.Any(x=>x.EndDate != null) ? FinalAddItemSchemeUploaders.Select(x=>x.EndDate).FirstOrDefault() : null;
                                                ItemSchemeExcelUploaderMaster.CreatedDate = indianTime;
                                                ItemSchemeExcelUploaderMaster.IsDeleted = false;
                                                ItemSchemeExcelUploaderMaster.IsActive = true;
                                                ItemSchemeExcelUploaderMaster.IsApproved = false;
                                                ItemSchemeExcelUploaderMaster.UploadedSheetUrl = "/UploadedFiles/ItemSchemExcel/" + httpPostedFile.FileName;
                                                ItemSchemeExcelUploaderMaster.Status = 0;
                                                InsertData.Add(ItemSchemeExcelUploaderMaster);
                                            }
                                        //}
                                    //}

                                    if (InsertData != null && InsertData.Any())
                                    {
                                        if (IsCompanyStockCode)
                                        {
                                            var itemmultimrpids = FinalAddItemSchemeUploaders.Where(x => x.ItemMultiMRP > 0).Select(x => x.ItemMultiMRP).Distinct().ToList();
                                            var itemmrplists = context.ItemMultiMRPDB.Where(x => itemmultimrpids.Contains(x.ItemMultiMRPId)).ToList();
                                            if (itemmrplists != null)
                                            {
                                                foreach (var item in itemmrplists)
                                                {
                                                    item.UpdatedDate = indianTime;
                                                    item.UpdateBy = people.DisplayName;
                                                    item.CompanyStockCode = FinalAddItemSchemeUploaders.Any(x => x.ItemMultiMRP == item.ItemMultiMRPId && x.CompanyStockCode != null) ? FinalAddItemSchemeUploaders.FirstOrDefault(x => x.ItemMultiMRP == item.ItemMultiMRPId && x.CompanyStockCode != null).CompanyStockCode : item.CompanyStockCode;
                                                    context.Entry(item).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                        if (IsCompanyCode)
                                        {
                                            var ItemCodeList = FinalAddItemSchemeUploaders.Where(x => x.ItemCode != null).Select(x => x.ItemCode).Distinct().ToList();
                                            var itemcentrallists = context.ItemMasterCentralDB.Where(x => ItemCodeList.Contains(x.Number)).ToList();
                                            if (itemcentrallists != null && itemcentrallists.Any())
                                            {
                                                foreach (var item in itemcentrallists)
                                                {
                                                    item.UpdatedDate = indianTime;
                                                    item.UpdateBy = people.DisplayName;
                                                    item.CompanyCode = FinalAddItemSchemeUploaders.Any(x => x.ItemCode == item.Number && x.CompanyCode != null) ? FinalAddItemSchemeUploaders.FirstOrDefault(x => x.ItemCode == item.Number && x.CompanyCode != null).CompanyCode : item.CompanyCode;
                                                    context.Entry(item).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                        context.ItemSchemeExcelUploaderMasters.AddRange(InsertData);

                                        DateTime todaydate = DateTime.Today;
                                        var data = context.ItemSchemeExcelUploaderMasters
                                                   .Where(x => x.StartDate <= todaydate && (x.EndDate == null || x.EndDate >= todaydate))
                                                   .ToList();
                                        if (data != null && data.Count > 0)
                                        {
                                            data.ForEach(y =>
                                            {
                                                y.EndDate = FinalAddItemSchemeUploaders.Any(x => x.CityId == y.Cityid) ? FinalAddItemSchemeUploaders.Where(x => x.CityId == y.Cityid).Select(x => (DateTime)x.StartDate).FirstOrDefault().AddDays(-1) : DateTime.Today.AddDays(-1);
                                                y.ModifiedBy = people != null ? people.PeopleID : 0;
                                                y.ModifiedDate = DateTime.Now;
                                                context.Entry(y).State = EntityState.Modified;
                                            });
                                        }

                                        context.Commit();
                                        IsUploaded = true;
                                    }
                                }
                            }
                        }
                        if (IsUploaded)
                        {
                            msg = "Your Excel data is uploaded succesfully.";
                            return Ok(msg);
                        }
                        else
                        {
                            msg = "Something went wrong";
                            return Ok(msg);
                        }
                    }
                }
                else { msg = "Something went wrong"; }
            }
            else { msg = "Something went wrong"; }
            return Ok(msg);

        }

        public string ValidateHeader(IRow rowData)
        {
            string strJSON = null;
            string field = string.Empty;
            //field = rowData.GetCell(0).ToString();//
            //if (field != "SubCategory")
            //{
            //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
            //    strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
            //    return strJSON;
            //}


            field = rowData.GetCell(0).ToString();
            if (field != "ClaimType")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(1).ToString();
            if (field != "CompanyCode")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(2).ToString();
            if (field != "CompanyStockCode")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(3).ToString();
            if (field != "ItemName")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(4).ToString();
            if (field.Trim().ToLower() != "ItemCode".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(5).ToString();
            if (field.Trim().ToLower() != "ItemMultiMRP".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(6).ToString();
            if (field != "MRP")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(7).ToString();
            if (field.Trim().ToLower() != "GST".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(8).ToString();
            if (field != "PTR")
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(9).ToString();
            if (field.Trim().ToLower() != "TOTOnInvoice".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(10).ToString();
            if (field.Trim().ToLower() != "Category".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(11).ToString();
            if (field.Trim().ToLower() != "Subcategory".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(12).ToString();
            if (field.Trim().ToLower() != "Warehouse".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(13).ToString();
            if (field.Trim().ToLower() != "SalesOffice".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(14).ToString();
            if (field.Trim().ToLower() != "BaseScheme".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(15).ToString();
            if (field.Trim().ToLower() != "PrimaryScheme".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(16).ToString();
            if (field.Trim().ToLower() != "SlabQTY1".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(17).ToString();
            if (field.Trim().ToLower() != "SlabScheme1".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(18).ToString();
            if (field.Trim().ToLower() != "SlabQTY2".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(19).ToString();
            if (field.Trim().ToLower() != "SlabScheme2".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(20).ToString();
            if (field.Trim().ToLower() != "SlabQTY3".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(21).ToString();
            if (field.Trim().ToLower() != "SlabScheme3".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(22).ToString();
            if (field.Trim().ToLower() != "SlabQTY4".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(23).ToString();
            if (field.Trim().ToLower() != "SlabScheme4".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(24).ToString();
            if (field.Trim().ToLower() != "QPSValueTarget".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(25).ToString();
            if (field.Trim().ToLower() != "QPSValue".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(26).ToString();
            if (field.Trim().ToLower() != "QPSQtyTarget".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(27).ToString();
            if (field.Trim().ToLower() != "QPSQty".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(28).ToString();
            if (field.Trim().ToLower() != "Promo".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(29).ToString();
            if (field.Trim().ToLower() != "Visibility".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(30).ToString();
            if (field.Trim().ToLower() != "KVIorNonKVI".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(31).ToString();
            if (field.Trim().ToLower() != "CustomerTYpe".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(32).ToString();
            if (field.Trim().ToLower() != "AdditionalScheme".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            
            field = string.Empty;
            field = rowData.GetCell(33).ToString();
            if (field.Trim().ToLower() != "LiquidationSupport".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(34).ToString();
            if (field.Trim().ToLower() != "OffInvoiceMargin".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }
            field = string.Empty;
            field = rowData.GetCell(35).ToString();
            if (field.Trim().ToLower() != "StartDate".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            field = string.Empty;
            field = rowData.GetCell(36).ToString();
            if (field.Trim().ToLower() != "EndDate".Trim().ToLower())
            {
                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                return strJSON;
            }

            return strJSON;
        }


        #region OldUploader

        //[AllowAnonymous]
        //[Route("UploadItemSchemExcel")]
        //[HttpPost]
        //public async Task<IHttpActionResult> UploadItemSchemExcel(bool IsCompanyCode, bool IsCompanyStockCode, int SubCatId, int SubSubCatId)
        //{
        //    var msg = "";
        //    logger.Info("start Upload Exel File: ");
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];
        //        var Start = HttpContext.Current.Request.Form["StartDate"];
        //        var End = HttpContext.Current.Request.Form["EndDate"];
        //        var CityIdis = HttpContext.Current.Request.Form["CityIds"];
        //        List<int> CityIds = CityIdis.Split(',').Select(int.Parse).ToList();
        //        if (httpPostedFile != null && userid > 0 && CityIds.Any()) //&& SubCatId > 0
        //        {
        //            var SaveFileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel"), httpPostedFile.FileName);
        //            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel")))
        //            {
        //                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedFiles/ItemSchemExcel"));
        //            }
        //            httpPostedFile.SaveAs(SaveFileUrl);
        //            var uploader = new List<Uploader> { new Uploader() };
        //            uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
        //            uploader.FirstOrDefault().RelativePath = "~/UploadedFiles/ItemSchemExcel";
        //            uploader.FirstOrDefault().SaveFileURL = SaveFileUrl;
        //            uploader = await FileUploadHelper.UploadFileToOtherApi(uploader);
        //            DateTime StartDate = DateTime.ParseExact(Start.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //            DateTime EndDate = DateTime.ParseExact(End.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //            byte[] buffer = new byte[httpPostedFile.ContentLength];
        //            using (BinaryReader br = new BinaryReader(File.OpenRead(SaveFileUrl)))
        //            {
        //                br.Read(buffer, 0, buffer.Length);
        //            }
        //            XSSFWorkbook hssfwb;
        //            using (MemoryStream memStream = new MemoryStream())
        //            {
        //                BinaryFormatter binForm = new BinaryFormatter();
        //                memStream.Write(buffer, 0, buffer.Length);
        //                memStream.Seek(0, SeekOrigin.Begin);
        //                hssfwb = new XSSFWorkbook(memStream);
        //                string sSheetName = hssfwb.GetSheetName(0);
        //                ISheet sheet = hssfwb.GetSheet(sSheetName);
        //                IRow rowData;
        //                ICell cellData = null;
        //                bool IsUploaded = false;

        //                List<ItemSchemeExcelUploaderDetail> FinalAddItemSchemeUploaders = new List<ItemSchemeExcelUploaderDetail>();

        //                using (AuthContext context = new AuthContext())
        //                {
        //                    var people = context.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
        //                    var Citylist = context.Cities.Where(x => CityIds.Contains(x.Cityid)).ToList();
        //                    if (people != null)
        //                    {

        //                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
        //                        {
        //                            if (iRowIdx == 0)
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null)
        //                                {
        //                                    string Validatedheader = ValidateHeader(rowData);
        //                                    if (Validatedheader != null)
        //                                    {
        //                                        return Ok(Validatedheader);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                ItemSchemeExcelUploaderDetail AddItemSchemeUploaders = new ItemSchemeExcelUploaderDetail();
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                string col = null;
        //                                cellData = rowData.GetCell(0);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                    AddItemSchemeUploaders.EANNo = col.Trim();


        //                                cellData = rowData.GetCell(1);

        //                                col = cellData == null ? null : cellData.ToString();
        //                                if (col != null)
        //                                {
        //                                    AddItemSchemeUploaders.CompanyCode = col.Trim();
        //                                }


        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(2);
        //                                col = cellData == null ? null : cellData.ToString();

        //                                if (col != null)
        //                                {
        //                                    AddItemSchemeUploaders.CompanyStockCode = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(3);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                AddItemSchemeUploaders.ItemName = col.Trim();

        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(4);
        //                                var itemCode = "";
        //                                if (cellData == null)
        //                                {
        //                                    msg = "itemCode field does not exist..try again";
        //                                    return Ok(msg);
        //                                }
        //                                else
        //                                {
        //                                    col = cellData == null ? "" : cellData.ToString();
        //                                    if (col != "")
        //                                    {
        //                                        AddItemSchemeUploaders.ItemCode = col.Trim();
        //                                        itemCode = col.Trim();//mendatory
        //                                    }
        //                                }

        //                                var ItemMultiMRPId = 0; //mendatory
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(5);
        //                                if (cellData == null)
        //                                {
        //                                    msg = "ItemMultiMRPId field does not exist..try again";
        //                                    return Ok(msg);
        //                                }
        //                                else
        //                                {
        //                                    col = cellData == null ? "" : cellData.ToString();
        //                                    if (col != "")
        //                                    {
        //                                        AddItemSchemeUploaders.ItemMultiMRP = Convert.ToInt32(col);
        //                                        ItemMultiMRPId = Convert.ToInt32(col);
        //                                    }
        //                                }

        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(6);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.MRP = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(7);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.PTR = Convert.ToDouble(col);
        //                                }

        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(8);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.GST = Convert.ToDouble(col);
        //                                }
        //                                var Category = "";
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(9);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    Category = col.Trim();
        //                                }
        //                                var Subcategory = "";
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(10);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    Subcategory = col.Trim();
        //                                }
        //                                var Brand = "";
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(11);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    Brand = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(12);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.City = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(13);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.Zone = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(14);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.BaseScheme = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(15);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.IsIncludeBaseSchmePOPrice = Convert.ToBoolean(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(16);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.IsIncludeMaxSlabPOPrice = Convert.ToBoolean(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(17);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabPurchaseQTY1 = Convert.ToInt32(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(18);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabScheme1 = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(19);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabPurchaseQTY2 = Convert.ToInt32(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(20);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabScheme2 = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(21);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabPurchaseQTY3 = Convert.ToInt32(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(22);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabScheme3 = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(23);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabPurchaseQTY4 = Convert.ToInt32(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(24);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.SlabScheme4 = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(25);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.FreeBaseItemQty = Convert.ToInt32(col);
        //                                }

        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(26);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.FreeChildItem = col.Trim();
        //                                }

        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(27);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.FreeChildItemCompanycode = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(28);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.FreeChildItemCompanyStockcode = col.Trim();
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(29);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.FreeItemQty = Convert.ToInt32(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(30);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.IsFreeStock = Convert.ToBoolean(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(31);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.QPSTarget = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(32);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.QPS = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(33);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.Promo = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(34);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.VisibilityPercentage = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(35);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.KVIANDNonKVI = col.Trim();

        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(36);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.AdditionalScheme = Convert.ToDouble(col);

        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(37);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.onvoiceMargin = Convert.ToDouble(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(38);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.offinvoicemargin = Convert.ToDouble(col);
        //                                }
        //                                //---------------------------
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(39);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.StartDate = Convert.ToDateTime(col);
        //                                }
        //                                col = string.Empty;
        //                                cellData = rowData.GetCell(40);
        //                                col = cellData == null ? "" : cellData.ToString();
        //                                if (col != "")
        //                                {
        //                                    AddItemSchemeUploaders.EndDate = Convert.ToDateTime(col);
        //                                }
        //                                //------------------------------------

        //                                FinalAddItemSchemeUploaders.Add(AddItemSchemeUploaders);
        //                            }
        //                        }
        //                        var DistinctCTs = FinalAddItemSchemeUploaders.Where(x => x.City != null).Select(x => x.City).Distinct().ToList();

        //                        if (FinalAddItemSchemeUploaders != null && FinalAddItemSchemeUploaders.Any() && DistinctCTs != null && DistinctCTs.Any())
        //                        {

        //                            List<ItemSchemeExcelUploaderMaster> InsertData = new List<ItemSchemeExcelUploaderMaster>();

        //                            foreach (var item in DistinctCTs)
        //                            {

        //                                if (FinalAddItemSchemeUploaders.Any(x => x.City != null && x.City.Trim().ToLower() == item.Trim().ToLower()) != null && FinalAddItemSchemeUploaders.Any(x => x.City.Trim().ToLower() == item.Trim().ToLower()))
        //                                {

        //                                    ItemSchemeExcelUploaderMaster ItemSchemeExcelUploaderMaster = new ItemSchemeExcelUploaderMaster();
        //                                    ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails = new List<ItemSchemeExcelUploaderDetail>();
        //                                    ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails = FinalAddItemSchemeUploaders.Any(x => x.City != null && x.City.Trim().ToLower() == item.Trim().ToLower()) ? FinalAddItemSchemeUploaders.Where(x => x.City != null && x.City.Trim().ToLower() == item.Trim().ToLower()).ToList() : null;

        //                                    if (ItemSchemeExcelUploaderMaster != null && ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails.Any())
        //                                    {
        //                                        ItemSchemeExcelUploaderMaster.SubSubCatId = 0;
        //                                        ItemSchemeExcelUploaderMaster.SubCatId = Convert.ToInt32(SubCatId);
        //                                        ItemSchemeExcelUploaderMaster.Cityid = Citylist.Any(x => x.CityName.Trim().ToLower() == item.Trim().ToLower()) ? Citylist.FirstOrDefault(x => x.CityName.Trim().ToLower() == item.Trim().ToLower()).Cityid : 0;
        //                                        ItemSchemeExcelUploaderMaster.CreatedBy = people.PeopleID;
        //                                        ItemSchemeExcelUploaderMaster.StartDate = StartDate;
        //                                        ItemSchemeExcelUploaderMaster.EndDate = EndDate;
        //                                        ItemSchemeExcelUploaderMaster.CreatedDate = indianTime;
        //                                        ItemSchemeExcelUploaderMaster.IsDeleted = false;
        //                                        ItemSchemeExcelUploaderMaster.IsActive = true;
        //                                        ItemSchemeExcelUploaderMaster.IsApproved = false;
        //                                        ItemSchemeExcelUploaderMaster.UploadedSheetUrl = "/UploadedFiles/ItemSchemExcel/" + httpPostedFile.FileName;
        //                                        ItemSchemeExcelUploaderMaster.Status = 0;
        //                                        InsertData.Add(ItemSchemeExcelUploaderMaster);
        //                                    }
        //                                }
        //                            }

        //                            if (InsertData != null && InsertData.Any())
        //                            {
        //                                if (IsCompanyStockCode)
        //                                {
        //                                    var itemmultimrpids = FinalAddItemSchemeUploaders.Where(x => x.ItemMultiMRP > 0).Select(x => x.ItemMultiMRP).Distinct().ToList();
        //                                    var itemmrplists = context.ItemMultiMRPDB.Where(x => itemmultimrpids.Contains(x.ItemMultiMRPId)).ToList();
        //                                    if (itemmrplists != null)
        //                                    {
        //                                        foreach (var item in itemmrplists)
        //                                        {
        //                                            item.UpdatedDate = indianTime;
        //                                            item.UpdateBy = people.DisplayName;
        //                                            item.CompanyStockCode = FinalAddItemSchemeUploaders.Any(x => x.ItemMultiMRP == item.ItemMultiMRPId && x.CompanyStockCode != null) ? FinalAddItemSchemeUploaders.FirstOrDefault(x => x.ItemMultiMRP == item.ItemMultiMRPId && x.CompanyStockCode != null).CompanyStockCode : item.CompanyStockCode;
        //                                            context.Entry(item).State = EntityState.Modified;
        //                                        }
        //                                    }
        //                                }
        //                                if (IsCompanyCode)
        //                                {
        //                                    var ItemCodeList = FinalAddItemSchemeUploaders.Where(x => x.ItemCode != null).Select(x => x.ItemCode).Distinct().ToList();
        //                                    var itemcentrallists = context.ItemMasterCentralDB.Where(x => ItemCodeList.Contains(x.Number)).ToList();
        //                                    if (itemcentrallists != null && itemcentrallists.Any())
        //                                    {
        //                                        foreach (var item in itemcentrallists)
        //                                        {
        //                                            item.UpdatedDate = indianTime;
        //                                            item.UpdateBy = people.DisplayName;
        //                                            item.CompanyCode = FinalAddItemSchemeUploaders.Any(x => x.ItemCode == item.Number && x.CompanyCode != null) ? FinalAddItemSchemeUploaders.FirstOrDefault(x => x.ItemCode == item.Number && x.CompanyCode != null).CompanyCode : item.CompanyCode;
        //                                            context.Entry(item).State = EntityState.Modified;
        //                                        }
        //                                    }
        //                                }
        //                                context.ItemSchemeExcelUploaderMasters.AddRange(InsertData);
        //                                context.Commit();
        //                                IsUploaded = true;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (IsUploaded)
        //                {
        //                    msg = "Your Excel data is uploaded succesfully.";
        //                    return Ok(msg);
        //                }
        //                else
        //                {
        //                    msg = "Something went wrong";
        //                    return Ok(msg);
        //                }
        //            }
        //        }
        //        else { msg = "Something went wrong"; }
        //    }
        //    else { msg = "Something went wrong"; }
        //    return Ok(msg);

        //}

        #endregion

        #region Old ValidateHeader
        //public string ValidateHeader(IRow rowData)
        //{
        //    string strJSON = null;
        //    string field = string.Empty;
        //    field = rowData.GetCell(0).ToString();//
        //    if (field != "EAN No.")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
        //        strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(1).ToString();
        //    if (field != "CompanyCode")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
        //        strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(2).ToString();
        //    if (field != "CompanyStockCode")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(3).ToString();
        //    if (field != "ItemName")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(4).ToString();
        //    if (field.Trim().ToLower() != "Item Code".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(5).ToString();
        //    if (field.Trim().ToLower() != "Item Multi MRP".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(6).ToString();
        //    if (field != "MRP")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(7).ToString();
        //    if (field != "PTR")
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(8).ToString();
        //    if (field.Trim().ToLower() != "GST".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(9).ToString();
        //    if (field.Trim().ToLower() != "Category".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(10).ToString();
        //    if (field.Trim().ToLower() != "Subcategory".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(11).ToString();
        //    if (field.Trim().ToLower() != "Brand".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }


        //    field = string.Empty;
        //    field = rowData.GetCell(12).ToString();
        //    if (field.Trim().ToLower() != "City".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }

        //    field = string.Empty;
        //    field = rowData.GetCell(13).ToString();
        //    if (field.Trim().ToLower() != "Zone".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(14).ToString();
        //    if (field.Trim().ToLower() != "BaseScheme".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(15).ToString();
        //    if (field.Trim().ToLower() != "IncludeBaseSchmeForPOPrice".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }

        //    field = string.Empty;
        //    field = rowData.GetCell(16).ToString();
        //    if (field.Trim().ToLower() != "IncludeMaxSlabForPOPrice".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(17).ToString();
        //    if (field.Trim().ToLower() != "SlabPurchaseQTY1".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }

        //    field = string.Empty;
        //    field = rowData.GetCell(18).ToString();
        //    if (field.Trim().ToLower() != "SlabScheme1".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(19).ToString();
        //    if (field.Trim().ToLower() != "SlabPurchaseQTY2".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(20).ToString();
        //    if (field.Trim().ToLower() != "SlabScheme2".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(21).ToString();
        //    if (field.Trim().ToLower() != "SlabPurchaseQTY3".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(22).ToString();
        //    if (field.Trim().ToLower() != "SlabScheme3".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(23).ToString();
        //    if (field.Trim().ToLower() != "SlabPurchaseQTY4".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(24).ToString();
        //    if (field.Trim().ToLower() != "SlabScheme4".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(25).ToString();
        //    if (field.Trim().ToLower() != "BaseItemQty".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(26).ToString();
        //    if (field.Trim().ToLower() != "ChildItem".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }

        //    field = string.Empty;
        //    field = rowData.GetCell(27).ToString();
        //    if (field.Trim().ToLower() != "ChildItemCompanycode".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(28).ToString();
        //    if (field.Trim().ToLower() != "ChildItemCompanyStockcode".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(29).ToString();
        //    if (field.Trim().ToLower() != "FreeQty".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(30).ToString();
        //    if (field.Trim().ToLower() != "FreeStockQty".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(31).ToString();
        //    if (field.Trim().ToLower() != "QPS Target".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(32).ToString();
        //    if (field.Trim().ToLower() != "QPS%".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(33).ToString();
        //    if (field.Trim().ToLower() != "Promo".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(34).ToString();
        //    if (field.Trim().ToLower() != "Visibility".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(35).ToString();
        //    if (field.Trim().ToLower() != "KVI/Non KVI".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(36).ToString();
        //    if (field.Trim().ToLower() != "Additional Scheme%".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(37).ToString();
        //    if (field.Trim().ToLower() != "onvoiceMargin".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }
        //    field = string.Empty;
        //    field = rowData.GetCell(38).ToString();
        //    if (field.Trim().ToLower() != "offinvoicemargin".Trim().ToLower())
        //    {
        //        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //        return strJSON;
        //    }

        //    return strJSON;
        //}
        #endregion

        #endregion
    }

}
