using NLog;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ItemMasterUpload")]
    public class ItemMasterUploadController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg, msgitemname;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15, col16, col17, col18, col19, col20, col21, col22, col23, col24, col25, col26, col27, col28, col29, col30, col31, col32, col33;
        //[HttpPost]
        //public string UploadFile()
        //{
        //    var formData = HttpContext.Current.Request.Form["WareHouseId"];
        //    var formData1 = HttpContext.Current.Request.Form["compid"];
        //    int Warehouse_id = Convert.ToInt32(formData);
        //    int compid = Convert.ToInt32(formData1);
        //    try
        //    {
        //        if (HttpContext.Current.Request.Files.AllKeys.Any())
        //        {
        //            // Get the uploaded image from the Files collection
        //            System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];
        //            if (httpPostedFile != null)

        //            {
        //                // Validate the uploaded image(optional)
        //                byte[] buffer = new byte[httpPostedFile.ContentLength];
        //                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
        //                {
        //                    br.Read(buffer, 0, buffer.Length);
        //                }
        //                XSSFWorkbook hssfwb;
        //                using (MemoryStream memStream = new MemoryStream())
        //                {
        //                    BinaryFormatter binForm = new BinaryFormatter();
        //                    memStream.Write(buffer, 0, buffer.Length);
        //                    memStream.Seek(0, SeekOrigin.Begin);
        //                    hssfwb = new XSSFWorkbook(memStream);
        //                    string sSheetName = hssfwb.GetSheetName(0);
        //                    ISheet sheet = hssfwb.GetSheet(sSheetName);

        //                    IRow rowData;
        //                    ICell cellData = null;
        //                    try
        //                    {
        //                        List<ItemMaster> ItemCollection = new List<ItemMaster>();
        //                        List<SubsubCategory> wcats = new List<SubsubCategory>();
        //                        List<Category> wcat = new List<Category>();
        //                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
        //                        {
        //                            if (iRowIdx == 0)
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);

        //                                if (rowData != null)
        //                                {
        //                                    string field = string.Empty;
        //                                    field = rowData.GetCell(0).ToString();
        //                                    if (field != "CityName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(1).ToString();
        //                                    if (field != "Cityid")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(2).ToString();
        //                                    if (field != "CategoryName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(3).ToString();
        //                                    if (field != "CategoryCode")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(4).ToString();
        //                                    if (field != "SubcategoryName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }

        //                                    field = rowData.GetCell(5).ToString();
        //                                    if (field != "SubsubcategoryName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(6).ToString();
        //                                    if (field != "BrandCode")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(7).ToString();
        //                                    if (field != "itemname")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(8).ToString();
        //                                    if (field != "itemcode")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(9).ToString();
        //                                    if (field != "Number")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(10).ToString();
        //                                    if (field != "SellingSku")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(11).ToString();
        //                                    if (field != "price")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(12).ToString();
        //                                    if (field != "PurchasePrice")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(13).ToString();
        //                                    if (field != "UnitPrice")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(14).ToString();
        //                                    if (field != "MinOrderQty")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(15).ToString();
        //                                    if (field != "SellingUnitName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(16).ToString();
        //                                    if (field != "PurchaseMinOrderQty")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(17).ToString();
        //                                    if (field != "StoringItemName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(18).ToString();
        //                                    if (field != "PurchaseSku")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(19).ToString();
        //                                    if (field != "PurchaseUnitName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(20).ToString();
        //                                    if (field != "SupplierName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(21).ToString();
        //                                    if (field != "SUPPLIERCODES")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(22).ToString();
        //                                    if (field != "BaseCategoryName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(23).ToString();
        //                                    if (field != "TGrpName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(24).ToString();
        //                                    if (field != "TotalTaxPercentage")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }

        //                                    field = rowData.GetCell(25).ToString();
        //                                    if (field != "WarehouseName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(26).ToString();
        //                                    if (field != "HindiName")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(27).ToString();
        //                                    if (field != "SizePerUnit")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(28).ToString();
        //                                    if (field != "Barcode")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(29).ToString();
        //                                    if (field != "Active")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(30).ToString();
        //                                    if (field != "Deleted")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(31).ToString();
        //                                    if (field != "Margin")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                    field = rowData.GetCell(32).ToString();
        //                                    if (field != "PromoPoint")
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
        //                                        return strJSON;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                cellData = rowData.GetCell(0);
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null)
        //                                {
        //                                    ItemMaster item = new ItemMaster();
        //                                    try
        //                                    {
        //                                        item.CompanyId = compid;
        //                                        Category cat = null;
        //                                        SubCategory subcat = null;
        //                                        SubsubCategory subsubcat = new SubsubCategory();
        //                                        BaseCategory basecat = null;

        //                                        //City Name -1
        //                                        cellData = rowData.GetCell(0);
        //                                        col0 = cellData == null ? "" : cellData.ToString();
        //                                        if (col0.Trim() == "") break;
        //                                        item.CityName = col0.Trim();

        //                                        //City Code -2
        //                                        cellData = rowData.GetCell(1);
        //                                        col1 = cellData == null ? "" : cellData.ToString();
        //                                        string CityCode = col1.Trim();
        //                                        City city = context.Cities.Where(x => x.CityName.Trim().ToLower() == item.CityName.Trim().ToLower()).SingleOrDefault();
        //                                        if (city != null)
        //                                            item.Cityid = city.Cityid;

        //                                        //Warehouse Name 26
        //                                        cellData = rowData.GetCell(25);
        //                                        col25 = cellData == null ? "" : cellData.ToString();
        //                                        item.WarehouseName = col25.Trim();

        //                                        Warehouse w = context.Warehouses.Where(x => x.WarehouseName.Trim().ToLower() == item.WarehouseName.Trim().ToLower() && x.CompanyId == compid).SingleOrDefault();
        //                                        if (w != null)
        //                                            item.WarehouseId = w.WarehouseId;

        //                                        //base Category Name - 23
        //                                        cellData = rowData.GetCell(22);
        //                                        col22 = cellData == null ? "" : cellData.ToString();
        //                                        string basecategory = col22.Trim();
        //                                        basecat = context.BaseCategoryDb.Where(x => x.BaseCategoryName.ToLower().Equals(basecategory.ToLower())).SingleOrDefault();
        //                                        if (basecat == null && col22.Trim() != "")
        //                                        {
        //                                            basecat = new BaseCategory();
        //                                            basecat.BaseCategoryName = basecategory;
        //                                            //basecat.CompanyId = item.CompanyId;
        //                                            if (w != null)
        //                                            {
        //                                                //basecat.Warehouseid = w.Warehouseid;
        //                                            }
        //                                            else
        //                                            {
        //                                               // basecat.Warehouseid = 1;
        //                                            }
        //                                            basecat.IsActive = true;
        //                                            basecat = context.AddBaseCategory(basecat);
        //                                        }
        //                                        item.BaseCategoryName = basecat.BaseCategoryName;
        //                                        item.BaseCategoryid = basecat.BaseCategoryId;

        //                                        //Category Name -3
        //                                        cellData = rowData.GetCell(2);
        //                                        col2 = cellData == null ? "" : cellData.ToString();
        //                                        item.CategoryName = col2.Trim();

        //                                        //Category Code -4
        //                                        cellData = rowData.GetCell(3);
        //                                        col3 = cellData == null ? "" : cellData.ToString();
        //                                        string CategoryCode = col3.Trim();


        //                                        cat = context.Categorys.Where(x => x.CategoryName.Trim().Equals(col2.Trim())).SingleOrDefault();
        //                                        if (cat == null && col3.Trim() != "")
        //                                        {
        //                                            cat = new Category();
        //                                            cat.CategoryName = col2.Trim();
        //                                            cat.Code = CategoryCode;
        //                                            cat.BaseCategoryId = basecat.BaseCategoryId;                                                   
        //                                            cat.IsActive = true;
        //                                            cat = context.AddCategory(cat);
        //                                            var wc = wcat.Where(x => x.Categoryid == cat.Categoryid).FirstOrDefault();
        //                                            if (wc == null)
        //                                            {
        //                                                wcat.Add(cat);
        //                                            }
        //                                        }
        //                                        else
        //                                        {                                                    
        //                                            cat.IsActive = true;
        //                                            var wc = wcat.Where(x => x.Categoryid == cat.Categoryid).FirstOrDefault();
        //                                            if (wc == null)
        //                                            {
        //                                                wcat.Add(cat);
        //                                            }
        //                                        }
        //                                        item.Categoryid = cat.Categoryid;
        //                                        item.CategoryName = cat.CategoryName;
        //                                        //SubCategory Name - 5
        //                                        cellData = rowData.GetCell(4);
        //                                        col4 = cellData == null ? "" : cellData.ToString();
        //                                        item.SubcategoryName = col4.Trim();
        //                                        subcat = context.SubCategorys.Where(x => x.SubcategoryName.Trim().ToLower().Equals(col4.Trim().ToLower()) && x.Categoryid.Equals(cat.Categoryid) ).SingleOrDefault();
        //                                        if (subcat == null && col4.Trim() != "")
        //                                        {
        //                                            subcat = new SubCategory();
        //                                            subcat.SubcategoryName = col4;
        //                                            subcat.CategoryName = cat.CategoryName;
        //                                            subcat.Categoryid = cat.Categoryid;
        //                                            subcat.IsActive = true;
        //                                            subcat = context.AddSubCategory(subcat);
        //                                        }
        //                                        item.SubCategoryId = subcat.SubCategoryId;
        //                                        item.SubcategoryName = subcat.SubcategoryName;


        //                                        //Brand Name ie. SubSubCategory - 6
        //                                        cellData = rowData.GetCell(5);
        //                                        col5 = cellData == null ? "" : cellData.ToString();
        //                                        item.SubsubcategoryName = col5.Trim();

        //                                        //Brand Code  -7
        //                                        cellData = rowData.GetCell(6);
        //                                        col6 = cellData == null ? "" : cellData.ToString();
        //                                        string BrandCode = col6.Trim();

        //                                        subsubcat = context.SubsubCategorys.Where(x => x.SubsubcategoryName.Trim().ToLower().Equals(col5.Trim().ToLower()) && x.Categoryid.Equals(x.Categoryid) && x.SubCategoryId.Equals(subcat.SubCategoryId)).FirstOrDefault();
        //                                        if (subsubcat == null && col6.Trim() != "")
        //                                        {
        //                                            subsubcat = new SubsubCategory();
        //                                            subsubcat.SubsubcategoryName = col5;
        //                                            subsubcat.SubCategoryId = subcat.SubCategoryId;
        //                                            subsubcat.Code = col6;
        //                                            subsubcat.SubcategoryName = col4;
        //                                            subsubcat.CategoryName = cat.CategoryName;
        //                                            subsubcat.Categoryid = cat.Categoryid;
        //                                            subsubcat.IsActive = true;
        //                                            subsubcat.IsActive = true;
        //                                            subsubcat = context.AddSubsubCat(subsubcat);
        //                                            var wccc = wcats.Where(x => x.SubsubCategoryid == subsubcat.SubsubCategoryid).FirstOrDefault();
        //                                            if (wccc == null)
        //                                            {
        //                                                wcats.Add(subsubcat);
        //                                            }
        //                                        }         
        //                                        subsubcat.IsActive = true;

        //                                        var wcc = wcats.Where(x => x.SubsubCategoryid == subsubcat.Categoryid).FirstOrDefault();
        //                                        if (wcc == null)
        //                                        {
        //                                            wcats.Add(subsubcat);
        //                                        }

        //                                        item.SubsubCategoryid = subsubcat.SubsubCategoryid;
        //                                        item.SubsubcategoryName = subsubcat.SubsubcategoryName;
        //                                        item.SubSubCode = subsubcat.Code;

        //                                        //Item Name - 8
        //                                        cellData = rowData.GetCell(7);
        //                                        col7 = cellData == null ? "" : cellData.ToString();
        //                                        item.itemname = col7.Trim();

        //                                        //Item Code - 9
        //                                        cellData = rowData.GetCell(8);
        //                                        col8 = cellData == null ? "" : cellData.ToString();
        //                                        item.itemcode = col8.Trim();

        //                                        //Item Number - 10
        //                                        cellData = rowData.GetCell(9);
        //                                        col9 = cellData == null ? "" : cellData.ToString();
        //                                        if (col9.Trim() == "" || col9 == null || col9 == "null")
        //                                            break;
        //                                        item.Number = col9.Trim();

        //                                        //selling Sku - 11
        //                                        cellData = rowData.GetCell(10);
        //                                        col10 = cellData == null ? "" : cellData.ToString();
        //                                        if (col10.Trim() == "" || col10 == null || col10 == "null")
        //                                            break;
        //                                        item.SellingSku = col10.Trim();

        //                                        //MRP -12
        //                                        cellData = rowData.GetCell(11);
        //                                        col11 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.price = Convert.ToDouble(col11);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        // Purchase Price - 13
        //                                        cellData = rowData.GetCell(12);
        //                                        col12 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.PurchasePrice = Convert.ToDouble(col12);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        //Selling Price - 14
        //                                        cellData = rowData.GetCell(13);
        //                                        col13 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.UnitPrice = Convert.ToDouble(col13);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        //selling MOQ -15
        //                                        cellData = rowData.GetCell(14);
        //                                        col14 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.MinOrderQty = Convert.ToInt32(col14);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        //selling Unit -16
        //                                        cellData = rowData.GetCell(15);
        //                                        col15 = cellData == null ? "" : cellData.ToString();
        //                                        item.SellingUnitName = col15.Trim();

        //                                        //ConversionFactor ,purchase qty  -17
        //                                        cellData = rowData.GetCell(16);
        //                                        col16 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.PurchaseMinOrderQty = int.Parse(col16);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        //StoringItemName -18
        //                                        cellData = rowData.GetCell(17);
        //                                        col17 = cellData == null ? "" : cellData.ToString();
        //                                        item.StoringItemName = col17.Trim();

        //                                        //purchase sku -19
        //                                        cellData = rowData.GetCell(18);
        //                                        col18 = cellData == null ? "" : cellData.ToString();
        //                                        if (col18.Trim() == "" || col18 == null || col18 == "null")
        //                                            break;
        //                                        item.PurchaseSku = col18.Trim();

        //                                        //Purchase Unit -20
        //                                        cellData = rowData.GetCell(19);
        //                                        col19 = cellData == null ? "" : cellData.ToString();
        //                                        item.PurchaseUnitName = col19.Trim();

        //                                        //Supplier -21
        //                                        cellData = rowData.GetCell(20);
        //                                        col20 = cellData == null ? "" : cellData.ToString();
        //                                        string spname = col20.Trim();

        //                                        //Supplier code -22
        //                                        cellData = rowData.GetCell(21);
        //                                        col21 = cellData == null ? "" : cellData.ToString();
        //                                        string sp_code = col21.Trim();
        //                                        Supplier supplier = null;

        //                                        supplier = context.Suppliers.Where(x => x.SUPPLIERCODES.Trim().Equals(sp_code.Trim()) && x.CompanyId == compid).SingleOrDefault();
        //                                        if (supplier == null && col21.Trim() != "")
        //                                        {
        //                                            supplier = new Supplier();
        //                                            supplier.Name = spname;
        //                                            supplier.SUPPLIERCODES = sp_code;
        //                                            supplier.CompanyId = compid;
        //                                            supplier.WarehouseId = Warehouse_id;
        //                                            supplier = context.AddSupplier(supplier);
        //                                        }
        //                                        item.SupplierId = supplier.SupplierId;
        //                                        item.SupplierName = supplier.Name;

        //                                        item.SUPPLIERCODES = supplier.SUPPLIERCODES;

        //                                        // TaxGroup -24
        //                                        cellData = rowData.GetCell(23);
        //                                        col23 = cellData == null ? "" : cellData.ToString();
        //                                        TaxGroup grp = context.DbTaxGroup.Where(x => x.TGrpName.Trim().ToLower().Equals(col23.Trim().ToLower()) && x.CompanyId == compid).SingleOrDefault();
        //                                        item.TGrpName = col23.Trim();
        //                                        if (grp != null)
        //                                        {
        //                                            item.GruopID = grp.GruopID;
        //                                        }
        //                                        // TotalTaxPercentage -25
        //                                        cellData = rowData.GetCell(24);
        //                                        col24 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.TotalTaxPercentage = double.Parse(col24);
        //                                        }
        //                                        catch (Exception e) { }

        //                                        //item.active = true;

        //                                        //hindi Nmae -27
        //                                        cellData = rowData.GetCell(26);
        //                                        col26 = cellData == null ? "" : cellData.ToString();
        //                                        item.HindiName = col26.Trim();

        //                                        //Sizeperunit Area-28
        //                                        cellData = rowData.GetCell(27);
        //                                        col27 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.SizePerUnit = double.Parse(col27);
        //                                        }
        //                                        catch (Exception e)
        //                                        {
        //                                            item.SizePerUnit = 0;
        //                                        }

        //                                        //barcode -29
        //                                        cellData = rowData.GetCell(28);
        //                                        col28 = cellData == null ? "" : cellData.ToString();
        //                                        item.Barcode = col28.Trim();

        //                                        cellData = rowData.GetCell(29);
        //                                        col29 = cellData == null ? "" : cellData.ToString();
        //                                        if (col29.Trim().ToLower() == "true")
        //                                        {
        //                                            item.active = true;
        //                                        }
        //                                        else if (col29.Trim().ToLower() == "false")
        //                                        {
        //                                            item.active = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            item.active = false;
        //                                        }
        //                                        cellData = rowData.GetCell(30);
        //                                        col30 = cellData == null ? "" : cellData.ToString();
        //                                        if (col30.Trim().ToLower() == "true")
        //                                        {
        //                                            item.Deleted = true;
        //                                        }
        //                                        else if (col30.Trim().ToLower() == "false")
        //                                        {
        //                                            item.Deleted = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            item.Deleted = false;
        //                                        }
        //                                        cellData = rowData.GetCell(31);
        //                                        col31 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.Margin = double.Parse(col31);
        //                                        }
        //                                        catch (Exception e)
        //                                        {
        //                                            item.Margin = 0;
        //                                        }

        //                                        cellData = rowData.GetCell(32);
        //                                        col32 = cellData == null ? "" : cellData.ToString();
        //                                        try
        //                                        {
        //                                            item.promoPoint = int.Parse(col32);
        //                                        }
        //                                        catch (Exception e)
        //                                        {
        //                                            item.promoPoint = 0;
        //                                        }
        //                                        item.CompanyId = compid;
        //                                        cellData = rowData.GetCell(33);
        //                                        col33 = cellData == null ? "" : cellData.ToString();
        //                                        item.HSNCode = col33.Trim();


        //                                        ItemCollection.Add(item);
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        logger.Error("Error adding item in collection " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace + item.itemname);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        List<WarehouseSubsubCategory> wc1 = new List<WarehouseSubsubCategory>();
        //                        foreach (SubsubCategory c in wcats)
        //                        {
        //                            Warehouse w = context.Warehouses.Where(x => x.WarehouseId == Warehouse_id && x.CompanyId == compid).SingleOrDefault();
        //                            WarehouseSubsubCategory wc = new WarehouseSubsubCategory();
        //                            wc.SubsubCategoryid = c.SubsubCategoryid;
        //                            wc.SubsubCode = c.Code;
        //                            wc.SubsubcategoryName = c.SubsubcategoryName;
        //                            wc.WarehouseId = w.WarehouseId;
        //                            wc.WarehouseName = w.WarehouseName;
        //                            wc.CompanyId = compid;
        //                            wc.Stateid = w.Stateid;
        //                            wc.Cityid = w.Cityid;
        //                            wc.IsActive = true;
        //                            wc1.Add(wc);
        //                        }
        //                        context.AddWarehouseCategory(wc1, "", compid);
        //                        context.AddBulkItemMaster(ItemCollection);

        //                        try
        //                        {
        //                            var ass = context.AllWarehouseCategory(compid).ToList();
        //                            foreach (var a in ass)
        //                            {
        //                                CommonHelper.refreshItemMaster(a.WarehouseId, a.SubsubCategoryid);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            logger.Error("Error in update Cache " + ex.Message);
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logger.Error("Error in update Cache " + ex.Message);
        //                    }
        //                }
        //                var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

        //                httpPostedFile.SaveAs(FileUrl);
        //            }


        //        }
        //        if (msgitemname != null)
        //        {
        //            return msgitemname;
        //        }
        //       msg = "Your Exel data is succesfully saved";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Add Item Master " + ex.Message);
        //        return null;
        //    }
        //    return msg;
        //}
    }
}
