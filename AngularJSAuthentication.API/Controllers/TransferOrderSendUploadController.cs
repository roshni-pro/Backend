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
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TransferOrderSendUpload")]
    public class TransferOrderSendUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();//Api for Upload Sheet on Warehouse to warehouse transfer
        string msg, msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3;
        [Authorize]
        [HttpPost]
        public string UploadFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {

                var formData = HttpContext.Current.Request.Form["WareHouseId"];
                var formData1 = HttpContext.Current.Request.Form["compid"];
                int Warehouse_id = Convert.ToInt32(formData);
                int compid = 1;// Convert.ToInt32(formData1);

                logger.Info("start Transfer Order Upload Exel File: ");
                var identity = User.Identity as ClaimsIdentity;

                int userid = 0;
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
                                List<TransferWHOrderDetails> transferordercollection = new List<TransferWHOrderDetails>();
                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {
                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);


                                        if (rowData != null)
                                        {
                                            string field = string.Empty;

                                            field = rowData.GetCell(0).ToString();
                                            if (field != "ItemNumber")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }

                                            field = string.Empty;
                                            field = rowData.GetCell(1).ToString();
                                            if (field != "itemname")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }


                                            field = rowData.GetCell(2).ToString();
                                            if (field != "TotalQuantity")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(3).ToString();
                                            if (field != "RequestToWarehouseId")
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
                                            TransferWHOrderDetails trnfrorder = new TransferWHOrderDetails();
                                            try
                                            {

                                                int requestTowarehouseId;
                                                cellData = rowData.GetCell(0);
                                                col0 = cellData == null ? "" : cellData.ToString();
                                                trnfrorder.ItemNumber = Convert.ToString(col0);
                                                logger.Info("ItemNumber :" + trnfrorder.ItemNumber);


                                                cellData = rowData.GetCell(1);
                                                col1 = cellData == null ? "" : cellData.ToString();
                                                trnfrorder.itemname = col1.Trim();

                                                cellData = rowData.GetCell(2);
                                                col2 = cellData == null ? "" : cellData.ToString();
                                                trnfrorder.TotalQuantity = Convert.ToInt32(col2);



                                                cellData = rowData.GetCell(3);
                                                col3 = cellData == null ? "" : cellData.ToString();
                                                if ((col3 == null) || (col3 == ""))
                                                {
                                                    requestTowarehouseId = 0;
                                                }
                                                else
                                                {
                                                    requestTowarehouseId = Convert.ToInt32(col3);

                                                }
                                                trnfrorder.RequestToWarehouseId = requestTowarehouseId;

                                                trnfrorder.userid = userid;
                                                trnfrorder.CompanyId = compid;
                                                trnfrorder.WarehouseId = Warehouse_id;


                                                if (trnfrorder.WarehouseId != trnfrorder.RequestToWarehouseId)
                                                {
                                                    transferordercollection.Add(trnfrorder);

                                                }
                                                else
                                                {

                                                    msg1 = "Request To Warehouse Id and your warehouse id should not be same";
                                                    return msg1;
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                msgitemname = ex.Message;
                                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace + trnfrorder.itemname);
                                            }
                                        }
                                    }
                                }
                                context.Addtransferorder(transferordercollection);
                                string m = "save collection";

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
            if (msgitemname != null)
            {
                return msgitemname;

            }
            else
            {
                msg1 = "Excel Successfully Uploaded.";
                return msg1;
            }
        }
    }
}
