
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
    [RoutePrefix("api/supplierupload")]
    public class SupplierUploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11;
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
                                List<Supplier> supCollection = new List<Supplier>();
                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                {
                                    if (iRowIdx == 0)
                                    {
                                        rowData = sheet.GetRow(iRowIdx);

                                        if (rowData != null)
                                        {
                                            string field = string.Empty;
                                            field = rowData.GetCell(1).ToString().Trim();
                                            if (field != "SUPPLIERCODES")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(2).ToString().Trim();
                                            if (field != "Name")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(3).ToString().Trim();
                                            if (field != "Brand")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(4).ToString().Trim();
                                            if (field != "MobileNo")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(5).ToString().Trim();
                                            if (field != "OfficePhone")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(6).ToString().Trim();
                                            if (field != "BillingAddress")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(7).ToString().Trim();
                                            if (field != "TINNo")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(8).ToString().Trim();
                                            if (field != "Bank_AC_No")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(9).ToString().Trim();
                                            if (field != "Bank_Name")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(10).ToString().Trim();
                                            if (field != "Bank_Ifsc")
                                            {
                                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                                return strJSON;
                                            }
                                            field = rowData.GetCell(11).ToString().Trim();
                                            if (field != "ShippingAddress")
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
                                            Supplier sup = new Supplier();
                                            try
                                            {
                                                sup.CompanyId = 1;
                                                cellData = rowData.GetCell(1);
                                                col1 = cellData == null ? "" : cellData.ToString();
                                                Supplier supp = context.Suppliers.Where(x => x.SUPPLIERCODES == col1.Trim()).FirstOrDefault();
                                                if (supp == null)
                                                {
                                                    sup.SUPPLIERCODES = col1.Trim();

                                                    cellData = rowData.GetCell(2);
                                                    col2 = cellData == null ? "" : cellData.ToString();
                                                    sup.Name = col2.Trim();

                                                    cellData = rowData.GetCell(3);
                                                    col3 = cellData == null ? "" : cellData.ToString();
                                                    sup.Brand = col3.Trim();

                                                    cellData = rowData.GetCell(4);
                                                    col4 = cellData == null ? "" : cellData.ToString();
                                                    sup.MobileNo = (col4.Trim());

                                                    cellData = rowData.GetCell(5);
                                                    col5 = cellData == null ? "" : cellData.ToString();
                                                    sup.OfficePhone = col5.Trim();

                                                    cellData = rowData.GetCell(6);
                                                    col6 = cellData == null ? "" : cellData.ToString();
                                                    sup.BillingAddress = col6.Trim();

                                                    cellData = rowData.GetCell(7);
                                                    col7 = cellData == null ? "" : cellData.ToString();
                                                    sup.TINNo = col7.Trim();

                                                    cellData = rowData.GetCell(8);
                                                    col8 = cellData == null ? "" : cellData.ToString();
                                                    sup.Bank_AC_No = col8.Trim();

                                                    cellData = rowData.GetCell(9);
                                                    col9 = cellData == null ? "" : cellData.ToString();
                                                    sup.Bank_Name = col9.Trim();

                                                    cellData = rowData.GetCell(10);
                                                    col10 = cellData == null ? "" : cellData.ToString();
                                                    sup.Bank_Ifsc = col10.Trim();

                                                    cellData = rowData.GetCell(11);
                                                    col11 = cellData == null ? "" : cellData.ToString();
                                                    sup.ShippingAddress = col11.Trim();
                                                    sup.CompanyId = compid;
                                                    supCollection.Add(sup);
                                                }
                                                else
                                                {
                                                    logger.Info("skcode Alredy Exist Skcode=:-" + col0);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace + sup.Name);
                                            }
                                        }
                                    }
                                }
                                context.AddBulkSupplier(supCollection);
                                string m = "save collection";
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading URL for " + Url + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
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
//    [RoutePrefix("api/supplierupload")]
//    public class SupplierUploadController : ApiController
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
//                                    Supplier sup   = new Supplier();

//                                    sup.CompanyId = 1; // compid;
//                                    cellData = rowData.GetCell(3);

//                                    try
//                                    {
//                                        cellData = rowData.GetCell(2);
//                                        sup.SupplierCaegoryId=Convert.ToInt32(cellData == null ? "" : cellData.ToString());

//                                        cellData = rowData.GetCell(3);
//                                        sup.CategoryName = cellData == null ? "" : cellData.ToString();

//                                        sup.Name = cellData == null ? "" : cellData.ToString();

//                                        cellData = rowData.GetCell(4);
//                                        sup.Name = cellData == null ? "" : cellData.ToString();

//                                    }
//                                    catch (Exception ex) { }

//                                    try
//                                    {
//                                        cellData = rowData.GetCell(5);
//                                        if (cellData != null)
//                                        {
//                                            sup.Avaiabletime =Convert.ToInt32(cellData == null ? "" : cellData.ToString());
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(6);
//                                        if (cellData != null)
//                                        {
//                                            sup.PhoneNumber = Convert.ToInt32(cellData == null ? "" : cellData.ToString());

//                                           // sup.PhoneNumber = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(7);
//                                        if (cellData != null)
//                                        {
//                                            sup.PhoneNumber = Convert.ToInt32(cellData == null ? "" : cellData.ToString());

//                                            //sup.rating = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(8);
//                                        if (cellData != null)
//                                        {
//                                            sup.BillingAddress = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(9);
//                                        if (cellData != null)
//                                        {
//                                            sup.ShippingAddress = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(10);
//                                        if (cellData != null)
//                                        {
//                                            sup.Comments = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {
//                                        cellData = rowData.GetCell(11);
//                                        if (cellData != null)
//                                        {
//                                            sup.TINNo = Convert.ToInt32(cellData == null ? "" : cellData.ToString());
//                                            //sup.TINNo = cellData == null ? "" : cellData.ToString();
//                                        }
//                                    }
//                                    catch (Exception ex) { }
//                                    try
//                                    {

//                                        cellData = rowData.GetCell(12);
//                                        sup.OfficePhone = Convert.ToInt32(cellData == null ? "" : cellData.ToString());
//                                      //  sup.OfficePhone = cellData == null ? "" : cellData.ToString();



//                                        cellData = rowData.GetCell(13);
//                                        sup.MobileNo = Convert.ToInt32(cellData == null ? "" : cellData.ToString());

//                                        //sup.MobileNo = cellData == null ? "" : cellData.ToString();


//                                        sup.PhoneNumber = Convert.ToInt32(cellData == null ? "" : cellData.ToString());

//                                        cellData = rowData.GetCell(14);
//                                        sup.EmailId = cellData == null ? "" : cellData.ToString();

//                                    }
//                                    catch (Exception ex) { }
//                                    context.AddSupplierExcel(sup);


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
