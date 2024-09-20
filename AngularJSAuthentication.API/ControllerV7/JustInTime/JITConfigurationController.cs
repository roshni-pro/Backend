
using AngularJSAuthentication.BusinessLayer.Managers.JustInTime;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.Model.JustInTime;
using Microsoft.Extensions.Logging;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.ControllerV7.JustInTime
{
    [RoutePrefix("api/JITConfiguration")]
    public class JITConfigurationController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private string strJSON;

        [HttpPost]
        [Route("GetBrandList")]
        public async Task<List<JITConfigurationListDc>> GetBrandList(BrandListVm brandListVm)
        {
            JITConfigManager manager = new JITConfigManager();

            var list = await manager.GetBrandList(brandListVm);
            return list;
        }

        [HttpGet]
        [Route("DeleteJITConfig")]
        public async Task<JITConfigurationMsg> DeleteJITConfig(long Id)
        {
            JITConfigManager manager = new JITConfigManager();

            var list = await manager.DeleteJITConfig(Id);
            return list;
        }

        [HttpPost]
        [Route("JITUploadFile")]
        public IHttpActionResult JITUploadFile()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (userid > 0)
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    List<JITConfiguration> JITUploaded = new List<JITConfiguration>();
                    var formData1 = HttpContext.Current.Request.Form["compid"];
                    logger.Info("start Transfer Order Upload Exel File: ");


                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var httpPostedFile = HttpContext.Current.Request.Files[i];

                        if (httpPostedFile != null)
                        {
                            //var dashboardDt = ClassToDataTable.CreateDataTable(JITUploaded);
                            var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                            //ExcelGenerator.DataTable_To_Excel(dashboardDt, "JITUPLoadData", FileUrl);
                            // Validate the uploaded image(optional)
                            string ext = Path.GetExtension(httpPostedFile.FileName);
                            if (ext == ".xlsx")
                            {
                                byte[] buffer = new byte[httpPostedFile.ContentLength];

                                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                                {
                                    br.Read(buffer, 0, buffer.Length);
                                }
                                XSSFWorkbook hssfwb;
                                //   XSSFWorkbook workbook1;
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    BinaryFormatter binForm = new BinaryFormatter();
                                    memStream.Write(buffer, 0, buffer.Length);
                                    memStream.Seek(0, SeekOrigin.Begin);
                                    hssfwb = new XSSFWorkbook(memStream);
                                }

                                return ReadJITUploadedFile(hssfwb, userid);
                            }
                            else
                            {
                                return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                            }
                            httpPostedFile.SaveAs(FileUrl);
                        }
                    }

                }
                return Created("Error", "Error");
            }
            else
            {
                return Created("Error", "Error");
            }
        }

        public IHttpActionResult ReadJITUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            string res = "";
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);
            string col0, col1, col2, col3;
            IRow rowData;
            ICell cellData = null;

            List<JITConfiguration> JITUploaded = new List<JITConfiguration>();
            int? brandIdCellIndex = null;
            int? brandCellIndex = null;
            int? ShowTypeCellIndex = null;
            int? ConfigurationCellIndex = null;
            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            {
                if (iRowIdx == 0)
                {
                    rowData = sheet.GetRow(iRowIdx);

                    if (rowData != null)
                    {
                        brandIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("BrandId").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("BrandId").Trim().ToLower()).ColumnIndex : (int?)null;
                        if (!brandIdCellIndex.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BrandId does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        brandCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("BrandName").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("BrandName").Trim().ToLower()).ColumnIndex : (int?)null;
                        if (!brandCellIndex.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("BrandName does not exist..try again");
                            return Created(strJSON, strJSON);
                        }

                        ShowTypeCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("ShowType").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("ShowType").Trim().ToLower()).ColumnIndex : (int?)null;
                        if (!ShowTypeCellIndex.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ShowType does not exist..try again");
                            return Created(strJSON, strJSON);
                        }

                        ConfigurationCellIndex = rowData.Cells.Any(x => x.ToString().Trim().ToLower() == ("Configuration").Trim().ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim().ToLower() == ("Configuration").Trim().ToLower()).ColumnIndex : (int?)null;
                        if (!ConfigurationCellIndex.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Configuration  does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                    }
                }
                else
                {
                    rowData = sheet.GetRow(iRowIdx);

                    if (rowData != null)
                    {
                        cellData = rowData.GetCell(0);
                        rowData = sheet.GetRow(iRowIdx);
                        JITConfiguration JITUploader = new JITConfiguration();

                        cellData = rowData.GetCell(brandIdCellIndex.Value);
                        col0 = cellData == null ? "" : cellData.ToString();
                        JITUploader.BrandId = Convert.ToInt32(col0);
                        logger.Info("BrandId :" + JITUploader.BrandId);

                        cellData = rowData.GetCell(ShowTypeCellIndex.Value);
                        col2 = cellData == null ? "" : cellData.ToString();
                        JITUploader.ShowType = Convert.ToString(col2);
                        logger.Info("ShowType :" + JITUploader.ShowType);

                        cellData = rowData.GetCell(ConfigurationCellIndex.Value);
                        col3 = cellData == null ? "" : cellData.ToString();
                        JITUploader.Percentage = Convert.ToInt64(col3);
                        logger.Info("Configuration. :" + JITUploader.Percentage);
                        JITUploader.CreatedBy = userid;

                        if(JITUploader != null && JITUploader.ShowType == "")
                        {
                            //string str = "Showtype cannot be null!";
                            //return Created(str, str);
                        }

                        if (JITUploaded != null && !JITUploaded.Any(x => x.BrandId == JITUploader.BrandId && x.ShowType.Trim().ToLower() == JITUploader.ShowType.Trim().ToLower()) && JITUploader.ShowType != "" && JITUploader.Percentage > 0 && JITUploader.Percentage <= 100)
                        {
                            JITUploaded.Add(JITUploader);
                        }
                        else if(JITUploaded.Any(x=>x.ShowType == ""))
                        {
                            //res = "Showtype cannot be null!";
                            //return Created(res, res);
                        }
                    }
                }
            }
            
            if (JITUploaded != null && JITUploaded.Any() && userid > 0)
            {
                res = AddJITUploaded(JITUploaded, userid);
            }
            
            return Created(res, res);

        }

        public string AddJITUploaded(List<JITConfiguration> jitCollection, int userId)
        {
            string result = "";
            logger.Info("start addbulk");
            using (var context = new AuthContext())
            {
                var peopledata = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();
                if (peopledata.Active)
                {

                    var BrandIds = jitCollection.Select(x => x.BrandId).ToList();
                    var classification = jitCollection.Select(x => x.ShowType).ToList();
                    var Brands = context.SubsubCategorys.Where(x => BrandIds.Contains(x.SubsubCategoryid) && x.IsActive && x.Deleted == false).ToList();
                    if (Brands != null && Brands.Any())
                    {
                        List<JITConfiguration> addList = new List<JITConfiguration>();
                        List<JITConfiguration> UpdateList = new List<JITConfiguration>();

                        var jitData = context.JITConfigurations.Where(x => BrandIds.Contains(x.BrandId) && classification.Contains(x.ShowType) && x.IsActive == true && x.IsDeleted == false).ToList();
                        foreach (var item in jitCollection)
                        {
                            if(item.ShowType.ToUpper() != "FAST" && item.ShowType.ToUpper() != "SLOW" && item.ShowType != "")
                            {
                                item.ShowType = "Non Moving";
                            }
                            if (item.ShowType.ToUpper() == "FAST" || item.ShowType.ToUpper() == "SLOW")
                            {
                                item.ShowType = item.ShowType.ToUpper() == "FAST" ? "Fast" : "Slow";
                            }
                            var Brand = Brands.FirstOrDefault(x => x.SubsubCategoryid == item.BrandId && x.Deleted == false );
                            if (item.BrandId > 0 && item.Percentage>=0 && item.Percentage <=100 && item.ShowType != "")
                            {
                                var df = jitData.FirstOrDefault(x => x.BrandId == item.BrandId && x.ShowType == item.ShowType);
                                if (df != null && item.Percentage>0)
                                {
                                    df.Percentage = item.Percentage;
                                    df.ModifiedDate = DateTime.Now;
                                    df.ModifiedBy = userId;
                                    UpdateList.Add(df);
                                }
                                else if (item.Percentage > 0)
                                {                                    
                                    JITConfiguration pd = new JITConfiguration();
                                    pd.Percentage = item.Percentage;
                                    pd.BrandId = item.BrandId;
                                    pd.ShowType = item.ShowType;
                                    pd.CreatedDate = DateTime.Now;
                                    pd.IsActive = true;
                                    pd.IsDeleted = false;
                                    pd.CreatedBy = userId;
                                    addList.Add(pd);
                                }
                            }
                        }

                        if (UpdateList != null && UpdateList.Any())
                        {
                            foreach (var item in UpdateList)
                            {
                                context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        if (addList != null && addList.Any())
                        {
                            context.JITConfigurations.AddRange(addList);
                        }
                        if (context.Commit() > 0) { result = "Record Updatd SuccessFully"; }
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("UpdatePercentange")]
        public async Task<JITConfigurationMsg> UpdatePercentange(int Id, double Percentage,string showType,int BrandId)
        {
            var res = new JITConfigurationMsg()
            {
                Status = false,
                Message = ""
            };
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if(Id > 0  && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    JITConfiguration SelectedJitConfig = new JITConfiguration();
                    var JitConfig = context.JITConfigurations.Where(x => x.ShowType == showType && x.BrandId == BrandId && x.IsDeleted == false).FirstOrDefault();
                    if (JitConfig == null)
                    {
                        SelectedJitConfig = context.JITConfigurations.Where(x => x.BrandId == BrandId && x.Id == Id && x.IsDeleted == false).FirstOrDefault();
                    }
                    else
                    {
                        SelectedJitConfig = context.JITConfigurations.Where(x => x.BrandId == BrandId && x.Id == Id && x.ShowType == showType && x.IsDeleted == false).FirstOrDefault();
                    }

                    if (SelectedJitConfig != null)
                    {
                        if (SelectedJitConfig.Percentage == Percentage && SelectedJitConfig.ShowType == showType && SelectedJitConfig.BrandId == BrandId)
                        {
                            res.Message = "Already Exists!";
                            return res;
                        }
                        if (Percentage < 0 && Percentage > 100)
                        {
                            res.Status = false;
                            res.Message = "Percentage value must be greater than 0!!!";
                            return res;
                        }
                        SelectedJitConfig.Percentage = Percentage;
                        SelectedJitConfig.ShowType = showType;
                        SelectedJitConfig.ModifiedBy = userid;
                        SelectedJitConfig.ModifiedDate = DateTime.Now;

                        context.Entry(SelectedJitConfig).State = System.Data.Entity.EntityState.Modified;
                        context.Commit();
                        res.Status = true;
                        res.Message = "Updated SuccessFully!";
                    }
                    else
                    {
                        res.Message = "Already Exists!";
                    }
                    return res;
                }
            }
            else
            {
                res.Status = false;
                res.Message = "Error!";
                return res;
            }
        }

        [HttpPost]
        [Route("InsertJITConfig")]
        public async Task<JITConfigurationMsg> InsertJITConfig(InsertJITConfigDc insertJITConfigDc)
        {
            var res = new JITConfigurationMsg()
            {
                Status = false,
                Message = ""
            };
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            
            if(userid > 0)
            {
                insertJITConfigDc.UserId = userid;
                JITConfigManager manager = new JITConfigManager();
                using (var context = new AuthContext())
                {
                    var JitConfig = context.JITConfigurations.FirstOrDefault(x => x.BrandId == insertJITConfigDc.BrandId && x.ShowType == insertJITConfigDc.ShowType && x.IsDeleted == false);
                    if (JitConfig != null)
                    {
                        res = new JITConfigurationMsg()
                        {
                            Status = false,
                            Message = "Already Exists!!!"
                        };
                    }
                    else if (insertJITConfigDc.Percentage <= 0)
                    {
                        res = new JITConfigurationMsg()
                        {
                            Status = false,
                            Message = "Percentage value must be greater than 0!!!"
                        };
                    }
                    else
                    {
                        res = await manager.InsertJITConfig(insertJITConfigDc);
                    }
                }
            }
            else
            {
                res = new JITConfigurationMsg()
                {
                    Status = false,
                    Message = "Error!!!"
                };
            }
                      
            return res;
        }
     
        [HttpPost]
        [Route("GetJITConfigList")]
        public async Task<APIResponse> GetJITConfigList(GetListVm getListVm)
        {
            JITConfigManager manager = new JITConfigManager();

            var list = await manager.GetJITConfigList(getListVm);
            return list;
        }

        [HttpGet]
        [Route("ActiveInactiveJITConfig")]
        public async Task<JITConfigurationMsg> ActiveInactiveJITConfig(int Id, bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            JITConfigurationMsg res;
            JITConfigManager manager = new JITConfigManager();

            if(Id > 0 && userid > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    JITConfiguration jitData = context.JITConfigurations.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();
                    if (jitData != null)
                    {
                        jitData.IsActive = IsActive;
                        jitData.ModifiedBy = userid;
                        jitData.ModifiedDate = DateTime.Now;
                    }
                    context.Entry(jitData).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        res = new JITConfigurationMsg()
                        {
                            Status = true,
                            Message = IsActive ? "Active Succesfully" : " Inactive Successfully!!"
                        };
                    }
                    else
                    {
                        res = new JITConfigurationMsg()
                        {
                            Status = false,
                            Message = "Something went wrong!!"
                        };
                    }
                }
            }
            else
            {
                res = new JITConfigurationMsg()
                {
                    Status = false,
                    Message = "Something went wrong!!"
                };
            }
         
            //var list = await manager.ActiveInactiveJITConfig(Id, IsActive);
            return res;
        }

        [HttpGet]
        [Route("BrandListByStore")]
        public async Task<List<BrandByStoreIdDc>> BrandListByStore(int GroupId)
        {
            JITConfigManager manager = new JITConfigManager();

            var list = await manager.BrandListByStore(GroupId);
            return list;
        }

        [HttpPost]
        [Route("ExportJITConfig")]
        public async Task<string> ExportJITConfig(BrandListVm brandListVm)
        {
            string fileUrl = string.Empty;
            JITConfigManager manager = new JITConfigManager();

            var list = await manager.ExportJITConfig(brandListVm);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_JITConfigurationListExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_JITConfigurationListExport.csv";
            DataTable dt = ListtoDataTableConverter.ToDataTable(list);

            // rearrange DataTable columns
            dt.Columns["BrandName"].SetOrdinal(0);
            dt.Columns["ShowType"].SetOrdinal(1);
            dt.Columns["Percentage"].SetOrdinal(2);
            dt.Columns["IsActive"].SetOrdinal(3);

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);


            return $"/ExcelGeneratePath/{fileName}";

        }
    }


}
