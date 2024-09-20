
//using AngularJSAuthentication.Model;
//using NLog;
//using NPOI.HSSF.UserModel;
//using NPOI.SS.UserModel;
//using NPOI.XSSF.UserModel;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;
//using System.Web.Script.Serialization;

//namespace AngularJSAuthentication.API.Controllers
//{
//    //Created By Vinayak (16/09/2019)

//    [RoutePrefix("api/freeStockupload")]
//    public class freeStockuploadController : ApiController
//    {

//        public static Logger logger = LogManager.GetCurrentClassLogger();
//        string msg, msgitemname;
//        string strJSON = null;
//        string col0, col1, col2, col3, col4, col5, col6;
//        [HttpPost]
//        public string UploadFile()
//        {

//            if (HttpContext.Current.Request.Files.AllKeys.Any())
//            {



//                logger.Info("start free stock Upload Exel File: ");
//                var identity = User.Identity as ClaimsIdentity;
//                //int userid = 0;
//                int compid = 1;// Convert.ToInt32(formData1);
//                // Access claims              

//                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
//                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

//                var formData = HttpContext.Current.Request.Form["WareHouseId"];
//                var formData1 = HttpContext.Current.Request.Form["compid"];
//                var userid = HttpContext.Current.Request.Form["userid"];
//                int Warehouse_id = Convert.ToInt32(formData);
//                var i = int.Parse(formData);
//                // Get the uploaded image from the Files collection
//                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

//                if (httpPostedFile != null)
//                {
//                    // Validate the uploaded image(optional)
//                    byte[] buffer = new byte[httpPostedFile.ContentLength];

//                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))

//                    {

//                        br.Read(buffer, 0, buffer.Length);

//                    }
//                    XSSFWorkbook hssfwb;
//                    //   XSSFWorkbook workbook1;
//                    using (MemoryStream memStream = new MemoryStream())
//                    {
//                        BinaryFormatter binForm = new BinaryFormatter();
//                        memStream.Write(buffer, 0, buffer.Length);
//                        memStream.Seek(0, SeekOrigin.Begin);
//                        hssfwb = new XSSFWorkbook(memStream);
//                        string sSheetName = hssfwb.GetSheetName(0);
//                        ISheet sheet = hssfwb.GetSheet(sSheetName);

//                        IRow rowData;
//                        ICell cellData = null;
//                        try
//                        {
//                            List<freeStockuploadDTO> currentstkcollection = new List<freeStockuploadDTO>();
//                            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
//                            {
//                                if (iRowIdx == 0)
//                                {
//                                    rowData = sheet.GetRow(iRowIdx);
//                                    if (rowData != null)
//                                    {
//                                        string field = string.Empty;

//                                        field = rowData.GetCell(0).ToString();
//                                        if (field != "ItemNumber")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = string.Empty;
//                                        field = rowData.GetCell(1).ToString();
//                                        if (field != "ItemMultiMRPId")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = rowData.GetCell(2).ToString();
//                                        if (field != "FreeStockId")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = rowData.GetCell(3).ToString();
//                                        if (field != "itemname")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = rowData.GetCell(4).ToString();
//                                        if (field != "DiffFreeStock")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = rowData.GetCell(5).ToString();
//                                        if (field != "Reason")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                        field = rowData.GetCell(6).ToString();
//                                        if (field != "WarehouseName")
//                                        {
//                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
//                                            return strJSON;
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    rowData = sheet.GetRow(iRowIdx);
//                                    cellData = rowData.GetCell(0);
//                                    rowData = sheet.GetRow(iRowIdx);
//                                    if (rowData != null)
//                                    {
//                                        freeStockuploadDTO currntstk = new freeStockuploadDTO();
//                                        try
//                                        {
//                                            int cstid, multimrpid;
//                                            cellData = rowData.GetCell(0);
//                                            col0 = cellData == null ? "" : cellData.ToString();
//                                            currntstk.ItemNumber = col0.Trim();

//                                            cellData = rowData.GetCell(1);
//                                            col1 = cellData == null ? "" : cellData.ToString();
//                                            if ((col1 == null) || (col1 == ""))
//                                            {
//                                                multimrpid = 0;
//                                            }
//                                            else
//                                            {
//                                                multimrpid = Convert.ToInt32(col1);

//                                            }
//                                            currntstk.ItemMultiMRPId = multimrpid;
//                                            cellData = rowData.GetCell(2);
//                                            col2 = cellData == null ? "" : cellData.ToString();
//                                            if ((col2 == null) || (col2 == ""))
//                                            {
//                                                cstid = 0;
//                                            }
//                                            else
//                                            {
//                                                cstid = Convert.ToInt32(col2);

//                                            }
//                                            currntstk.FreeStockId = cstid;

//                                            cellData = rowData.GetCell(3);
//                                            col3 = cellData == null ? "" : cellData.ToString();
//                                            currntstk.itemname = col3.Trim();

//                                            cellData = rowData.GetCell(4);
//                                            col4 = cellData == null ? "" : cellData.ToString();
//                                            currntstk.DiffFreeStock = Convert.ToInt32(col4);

//                                            cellData = rowData.GetCell(5);
//                                            col5 = cellData == null ? "" : cellData.ToString();
//                                            if (col4 == "0")
//                                            {
//                                                col5 = "";
//                                            }
//                                            else if (col4 != null && col5 == "")
//                                            {
//                                                msg = "Something went wrong";
//                                                return msg;
//                                            }
//                                            currntstk.Reason = col5.Trim();

//                                            cellData = rowData.GetCell(6);
//                                            col6 = cellData == null ? "" : cellData.ToString();
//                                            currntstk.WarehouseName = col6.Trim();

//                                            currntstk.WarehouseId = Warehouse_id;
//                                            currntstk.CompanyId = compid;

//                                            if (currntstk.DiffFreeStock == 0)
//                                            {
//                                            }
//                                            else
//                                            {
//                                                currentstkcollection.Add(currntstk);
//                                            }
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            msg = ex.Message;
//                                            logger.Error("Error adding customer in collection." + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
//                                            return msg;
//                                        }
//                                    }
//                                }
//                            }
//                            bool IsUploaded = false;

//                            if (currentstkcollection.Count > 0)
//                            {
//                                IsUploaded = context.AddfreeStock(currentstkcollection, userid);
//                            }
//                            if (IsUploaded)
//                            {
//                                msgitemname = "Your Exel data is succesfully saved";
//                                return msgitemname;
//                            }
//                            else
//                            {
//                                msg = "Something went wrong";
//                                return msg;
//                            }

//                        }
//                        catch (Exception ex)
//                        {
//                            logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
//                        }
//                    }
//                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

//                    httpPostedFile.SaveAs(FileUrl);
//                }
//            }
//            if (msgitemname != null)
//            {
//                msgitemname = "Your Exel data is succesfully saved";
//                return msgitemname;
//            }
//            msg = "Something went wrong";
//            return msg;
//        }


//    }
//}



