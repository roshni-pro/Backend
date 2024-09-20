
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
    [RoutePrefix("api/customerupload")]

    public class CustomerUploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14;
        [HttpPost]
        public string UploadFile()
        {
            return "";
        }
    }

}



//using AngularJSAuthentication.Model;
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

//namespace AngularJSAuthentication.API.Controllers
//{
//    [RoutePrefix("api/customerupload")]
//    public class CustomerUploadController : ApiController
//    {
//        [HttpPost]
//        public void UploadFile()
//        {
//            if (HttpContext.Current.Request.Files.AllKeys.Any())
//            {
//                var identity = User.Identity as ClaimsIdentity;
//                int compid = 0, userid = 0;
//                // Access claims
//                foreach (Claim claim in identity.Claims)
//                {
//                    if (claim.Type == "compid")
//                    {
//                        compid = int.Parse(claim.Value);
//                    }
//                    if (claim.Type == "userid")
//                    {
//                        userid = int.Parse(claim.Value);
//                    }
//                }
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
//                            for (int iRowIdx = 1; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
//                            {
//                                rowData = sheet.GetRow(iRowIdx);

//                                if (rowData != null)
//                                {
//                                    Customer Cust   = new Customer();

//                                    Cust.CompanyId = 3; // compid;
//                                    cellData = rowData.GetCell(3);

//                                    try
//                                    {
//                                        Cust.Name = cellData == null ? "" : cellData.ToString();

//                                        cellData = rowData.GetCell(9);
//                                        Cust.CustomerType = cellData == null ? "" : cellData.ToString();

//                                    }
//                                    catch (Exception ex) { }

//                                    try
//                                    {
//                                        cellData = rowData.GetCell(10);
//                                        if (cellData != null)
//                                        {
//                                            Cust.BillingAddress = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(11);
//                                        if (cellData != null)
//                                        {
//                                            Cust.ShippingAddress = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(12);
//                                        if (cellData != null)
//                                        {
//                                            Cust.City = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(1);
//                                        if (cellData != null)
//                                        {
//                                            Cust.Name = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(17);
//                                        if (cellData != null)
//                                        {
//                                            Cust.Emailid = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(19);
//                                        if (cellData != null)
//                                        {
//                                            Cust.CustomerCategoryName = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(15);
//                                        Cust.RefNo = cellData == null ? "" : cellData.ToString();

//                                        cellData = rowData.GetCell(16);
//                                        Cust.OfficePhone = cellData == null ? "" : cellData.ToString();


//                                    }
//                                    catch (Exception ex) { }
//                                    context.AddCustomerExcel(Cust);


//                                }
//                            }
//                            // _UpdateStatus = true;
//                        }
//                        catch (Exception ex)
//                        {
//                            //  logger.Error("Error loading URL for " + URL + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);

//                        }
//                    }

//                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

//                    httpPostedFile.SaveAs(FileUrl);
//                }
//            }
//        }
//    }

//}
