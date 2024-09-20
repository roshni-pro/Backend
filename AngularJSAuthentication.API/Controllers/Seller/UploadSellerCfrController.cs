using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using AngularJSAuthentication.API.ControllerV7;
using ClosedXML.Excel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AngularJSAuthentication.API.Controllers.Seller
{
    public class UploadSellerCfrController : ApiController
    {
        [Route("UploadSellerCfr")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult UploadSellerCfrExcel()
        {

            bool flag = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (HttpContext.Current.Request.Files.Count > 0 && userid > 0)
            {
                var SubCatId = HttpContext.Current.Request.Form["SubCatId"];
                var httpPostedFile = HttpContext.Current.Request.Files["xlsfile"];

                if (httpPostedFile != null)
                {
                    string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/CFR/");
                    string a1, b;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                    b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/CFR/"), a1);
                    httpPostedFile.SaveAs(b);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/UploadedFiles/CFR/", b);

                    byte[] buffer = new byte[httpPostedFile.ContentLength];
                    using (BinaryReader br = new BinaryReader(File.OpenRead(b)))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    List<uploaditemlistDTO> uploaditemlist = new List<uploaditemlistDTO>();

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
                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)
                            {
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    string strJSON = null;
                                    string field = string.Empty;
                                    field = rowData.GetCell(0).ToString();
                                    if (field != "ItemNumber")
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                        strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                        return Ok(strJSON);
                                    }
                                }
                            }
                            else
                            {
                                uploaditemlistDTO additem = new uploaditemlistDTO();
                                rowData = sheet.GetRow(iRowIdx);
                                cellData = rowData.GetCell(0);
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null && cellData != null)
                                {
                                    string col = null;
                                    cellData = rowData.GetCell(0);
                                    col = cellData == null ? "" : cellData.ToString();
                                    additem.ItemNumber = col.Trim();
                                    additem.SubCatId = Convert.ToInt32(SubCatId);
                                    uploaditemlist.Add(additem);
                                }
                            }
                        }
                    }
                    if (uploaditemlist != null && uploaditemlist.Any())
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            UploadCfrArticleController controller = new UploadCfrArticleController();
                            flag = controller.UploadCFr(uploaditemlist, userid, context);
                            if (flag)
                            {
                                return Ok("Your Excel data is uploaded succesfully.");
                            }
                           
                        }
                    }
                }
            }
            return Ok("Something went wrong");
        }
    }
}
