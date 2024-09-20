
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AreaUpload")]

    public class AreaUploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col0, col1;
        [HttpPost]
        public string UploadFile()
        {
            //using (var context = new AuthContext())
            //{
            //    if (HttpContext.Current.Request.Files.AllKeys.Any())
            //    {
            //        logger.Info("start Item Upload Exel File: ");
            //        var identity = User.Identity as ClaimsIdentity;
            //        int compid = 0, userid = 0;
            //        // Access claims
            //        foreach (Claim claim in identity.Claims)
            //        {
            //            if (claim.Type == "compid")
            //            {
            //                compid = int.Parse(claim.Value);
            //            }
            //            if (claim.Type == "userid")
            //            {
            //                userid = int.Parse(claim.Value);
            //            }
            //        }
            //        // Get the uploaded image from the Files collection
            //        System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

            //        if (httpPostedFile != null)
            //        {
            //            // Validate the uploaded image(optional)
            //            byte[] buffer = new byte[httpPostedFile.ContentLength];
            //            using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
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
            //                try
            //                {
            //                    List<Area> CustCollection = new List<Area>();
            //                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            //                    {
            //                        if (iRowIdx == 0)
            //                        {
            //                        }
            //                        else
            //                        {
            //                            rowData = sheet.GetRow(iRowIdx);
            //                            cellData = rowData.GetCell(0);
            //                            rowData = sheet.GetRow(iRowIdx);
            //                            if (rowData != null)
            //                            {
            //                                Area cust = new Area();
            //                                try
            //                                {

            //                                    cellData = rowData.GetCell(0);
            //                                    col0 = cellData == null ? "" : cellData.ToString();
            //                                    if (col0.Trim() == "") break;
            //                                    cust.AreaName = col0.Trim();

            //                                    CustCollection.Add(cust);


            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    msgitemname = ex.Message;
            //                                    logger.Error("Error adding People in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace + cust.AreaName);
            //                                }
            //                            }
            //                        }
            //                    }

            //                    context.AddBulkArea(CustCollection);
            //                    string m = "save collection";
            //                    logger.Info(m);
            //                }
            //                catch (Exception ex)
            //                {
            //                    logger.Error("Error loading URL for  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
            //                }
            //            }
            //            var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
            //            httpPostedFile.SaveAs(FileUrl);
            //        }
            //    }
            //    if (msgitemname != null)
            //    {
            //        return msgitemname;
            //    }
            //    msg = "Your Exel data is succesfully saved";
            //    return msg;
            //}
            return "";
        }

    }
}
