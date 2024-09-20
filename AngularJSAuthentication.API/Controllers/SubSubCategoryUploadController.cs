
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
    [RoutePrefix("api/subsubcategoryupload")]
    public class SubSubCategoryUploadController : ApiController
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
                                List<Supplier> supCollection = new List<Supplier>();
                                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
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
                                            Category cat = null;
                                            SubCategory subcat = null;
                                            SubsubCategory subsubcat = new SubsubCategory();
                                            BaseCategory basecat = null;
                                            try
                                            {
                                                // sup.CompanyId = 1;
                                                cellData = rowData.GetCell(0);
                                                col0 = cellData == null ? "" : cellData.ToString();
                                                cat = context.Categorys.Where(x => x.CategoryName.ToLower().Equals(col0.ToLower())).FirstOrDefault();
                                                if (cat == null)
                                                {
                                                    cat = new Category();
                                                    cat.CategoryName = col0;
                                                    cellData = rowData.GetCell(1);
                                                    col1 = cellData == null ? "" : cellData.ToString();
                                                    cat.Code = col1;
                                                    cat = context.AddCategory(cat);

                                                }
                                                cellData = rowData.GetCell(2);
                                                col2 = cellData == null ? "" : cellData.ToString();
                                                subcat = context.SubCategorys.Where(x => x.SubcategoryName.ToLower().Equals(col2.ToLower()) && x.CategoryName.Equals(cat.CategoryName)).FirstOrDefault();
                                                if (subcat == null)
                                                {
                                                    subcat = new SubCategory();

                                                    subcat.SubcategoryName = col2;
                                                    subcat.CategoryName = cat.CategoryName;
                                                    subcat.Categoryid = cat.Categoryid;
                                                    subcat = context.AddSubCategory(subcat);


                                                }
                                                cellData = rowData.GetCell(3);
                                                col3 = cellData == null ? "" : cellData.ToString();

                                                subsubcat = context.SubsubCategorys.Where(x => x.SubsubcategoryName.ToLower().Equals(col3.ToLower()) && x.CategoryName.Equals(x.CategoryName) && x.SubcategoryName.ToLower().Equals(subcat.SubcategoryName.ToLower())).FirstOrDefault();
                                                if (subsubcat == null)
                                                {
                                                    subsubcat = new SubsubCategory();
                                                    subsubcat.SubsubcategoryName = col3;
                                                    subsubcat.SubCategoryId = subcat.SubCategoryId;
                                                    cellData = rowData.GetCell(4);
                                                    col4 = cellData == null ? "" : cellData.ToString();
                                                    subsubcat.Code = col4;
                                                    subsubcat.SubcategoryName = col2;
                                                    subsubcat.CategoryName = cat.CategoryName;
                                                    subsubcat.Categoryid = cat.Categoryid;

                                                    subsubcat = context.AddSubsubCat(subsubcat);


                                                }

                                                cellData = rowData.GetCell(4);
                                                col4 = cellData == null ? "" : cellData.ToString();


                                                basecat = context.BaseCategoryDb.Where(x => x.BaseCategoryName.ToLower().Equals(col4.ToLower())).FirstOrDefault();
                                                if (basecat == null)
                                                {
                                                    basecat = new BaseCategory();
                                                    basecat.BaseCategoryName = col4;

                                                    basecat = context.AddBaseCategory(basecat);


                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error("Error adding customer in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                            }


                                        }
                                    }

                                }

                                context.AddBulkSupplier(supCollection);
                                string m = "save collection";
                            }



                            catch (Exception ex)
                            {
                                //  logger.Error("Error loading URL for " + URL + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);

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





