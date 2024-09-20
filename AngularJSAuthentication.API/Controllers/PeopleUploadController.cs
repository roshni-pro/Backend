
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
    [RoutePrefix("api/peopleupload")]

    public class PeopleUploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6;
        [HttpPost]
        public string UploadFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                logger.Info("start Item Upload Exel File: ");
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    // Validate the uploaded image(optional)
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
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);
                        using (AuthContext context = new AuthContext())
                        {
                            IRow rowData;
                            ICell cellData = null;
                            try
                            {
                                List<People> CustCollection = new List<People>();
                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {
                                    if (iRowIdx == 0)
                                    {
                                    }
                                    else
                                    {
                                        rowData = sheet.GetRow(iRowIdx);
                                        cellData = rowData.GetCell(0);
                                        rowData = sheet.GetRow(iRowIdx);
                                        if (rowData != null)
                                        {
                                            People cust = new People();
                                            try
                                            {
                                                cust.CompanyId = 1;
                                                cellData = rowData.GetCell(5);
                                                col0 = cellData == null ? "" : cellData.ToString();
                                                People custo = context.Peoples.Where(x => x.Deleted == false).Where(x => x.Mobile == col0).FirstOrDefault();
                                                if (custo == null)
                                                {
                                                    char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                                                    cellData = rowData.GetCell(1);
                                                    col1 = cellData == null ? "" : cellData.ToString();
                                                    string[] words = col1.Split(delimiterChars);

                                                    cust.PeopleFirstName = words[0];
                                                    cust.PeopleLastName = words[1];

                                                    cellData = rowData.GetCell(2);
                                                    col2 = cellData == null ? "" : cellData.ToString();
                                                    if (col2 == "Active" || col2 == "active")
                                                    {
                                                        cust.Active = true;
                                                    }
                                                    else { cust.Active = false; }

                                                    cellData = rowData.GetCell(3);
                                                    col3 = cellData == null ? "" : cellData.ToString();
                                                    DateTime date = DateTime.Parse(col3);
                                                    cust.CreatedDate = date;

                                                    cellData = rowData.GetCell(4);
                                                    col4 = cellData == null ? "" : cellData.ToString();
                                                    cust.Type = col4;
                                                    cust.Department = col4;

                                                    cellData = rowData.GetCell(5);
                                                    col5 = cellData == null ? "" : cellData.ToString();
                                                    cust.Mobile = col5;

                                                    cellData = rowData.GetCell(6);
                                                    col6 = cellData == null ? "" : cellData.ToString();
                                                    int id = Convert.ToInt32(col6);

                                                    Warehouse wh = context.Warehouses.Where(w => w.WarehouseId == id).FirstOrDefault();
                                                    logger.Info("getting warehouse id " + id + " of Skcode" + col0);
                                                    if (wh == null)
                                                        break;
                                                    cust.WarehouseId = id;
                                                    cust.Password = "123456";

                                                    cust.CompanyId = compid;

                                                    CustCollection.Add(cust);
                                                }
                                                else
                                                {
                                                    logger.Info("skcode Alredy Exist Skcode=:-" + col0);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                msgitemname = ex.Message;
                                                logger.Error("Error adding People in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace + cust.Mobile);
                                            }
                                        }
                                    }
                                }
                                //context.AddBulkItemMaster(CustCollection);
                                context.AddBulkpeople(CustCollection);
                                string m = "save collection";
                                logger.Info(m);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading URL for  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(FileUrl);
                }
            }
            if (msgitemname != null)
            {
                return msgitemname;
            }
            msg = "Your Exel data is succesfully saved";
            return msg;
        }
    }
}
