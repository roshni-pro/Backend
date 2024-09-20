
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/UploadAgentAssignToCust")]
    public class UploadAgentAssignToCustController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6;
        [HttpPost]
        public string UploadFile()
        {
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {

                    var formData = HttpContext.Current.Request.Form["WareHouseId"];
                    var formData1 = HttpContext.Current.Request.Form["compid"];
                    int Warehouse_id = Convert.ToInt32(formData);
                    int compid = Convert.ToInt32(formData1);
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
                        //   XSSFWorkbook workbook1;
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
                                    List<CustWarehouse> AgentCollection = new List<CustWarehouse>();
                                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                    {
                                        if (iRowIdx == 0)
                                        {
                                            rowData = sheet.GetRow(iRowIdx);

                                            if (rowData != null)
                                            {
                                                string field = string.Empty;
                                                field = rowData.GetCell(0).ToString();
                                                if (field != "CustomerId")
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                    return strJSON;
                                                }
                                                field = rowData.GetCell(1).ToString();
                                                if (field != "ExecutiveName")
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                    return strJSON;
                                                }
                                                field = rowData.GetCell(2).ToString();
                                                if (field != "Day")
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                    return strJSON;
                                                }
                                                field = rowData.GetCell(3).ToString();
                                                if (field != "BeatNumber")
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                    return strJSON;
                                                }
                                                field = rowData.GetCell(4).ToString();
                                                if (field != "AgentCode")
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                    return strJSON;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            rowData = sheet.GetRow(iRowIdx);

                                            cellData = rowData.GetCell(0);
                                            rowData = sheet.GetRow(iRowIdx);
                                            if (rowData != null)
                                            {
                                                CustWarehouse CustWA = new CustWarehouse();
                                                try
                                                {
                                                    int BeatNumber;
                                                    string data = null;
                                                    string Day = null;
                                                    string AgentCode = null;

                                                    cellData = rowData.GetCell(0);
                                                    col0 = cellData == null ? "" : cellData.ToString();
                                                    CustWA.CustomerId = Convert.ToInt32(col0);
                                                    cellData = rowData.GetCell(1);
                                                    col1 = cellData == null ? "" : cellData.ToString();
                                                    if ((col1 == null) || (col1 == ""))
                                                    {
                                                        data = null;
                                                    }
                                                    else
                                                    {
                                                        data = col1;
                                                    }
                                                    CustWA.ExecutiveName = data;

                                                    cellData = rowData.GetCell(2);
                                                    col2 = cellData == null ? "" : cellData.ToString();
                                                    if ((col2 == null) || (col2 == ""))
                                                    {
                                                        Day = null;
                                                    }
                                                    else
                                                    {
                                                        Day = col2;
                                                    }
                                                    CustWA.Day = Day;

                                                    cellData = rowData.GetCell(3);
                                                    col3 = cellData == null ? "" : cellData.ToString();
                                                    if ((col3 == null) || (col3 == ""))
                                                    {
                                                        BeatNumber = 0;
                                                    }
                                                    else
                                                    {
                                                        BeatNumber = Convert.ToInt32(col3);
                                                    }
                                                    CustWA.BeatNumber = Convert.ToInt32(BeatNumber);

                                                    cellData = rowData.GetCell(4);
                                                    col4 = cellData == null ? "" : cellData.ToString();
                                                    if ((col4 == null) || (col4 == ""))
                                                    {
                                                        AgentCode = null;
                                                    }
                                                    else
                                                    {
                                                        AgentCode = col4;
                                                    }
                                                    CustWA.AgentCode = AgentCode;



                                                    AgentCollection.Add(CustWA);

                                                }
                                                catch (Exception ex)
                                                {
                                                    msgitemname = ex.Message;
                                                    logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                                }
                                            }
                                        }
                                    }

                                    var customersToUpdate = AgentCollection.Select(x => new Customer
                                    {
                                        AgentCode = x.AgentCode,
                                        //BeatNumber = x.BeatNumber,
                                        //ExecutiveId = x.ExecutiveId,
                                        //Day = x.Day,
                                        CustomerId = x.CustomerId
                                    }).ToList();
                                    context.AddAgentToCust(customersToUpdate, Warehouse_id, compid);
                                    string m = "save collection";
                                    logger.Info(m);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error loading  for\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                            }
                        }
                        var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

                        httpPostedFile.SaveAs(FileUrl);
                    }
                }

            }
            catch (Exception ex)
            {
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

