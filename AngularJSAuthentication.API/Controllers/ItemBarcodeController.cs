using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using BarcodeLib;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    //By pooja k 
    //For generating Skcode Barcode 
    [RoutePrefix("api/ItemBarcodeA7")]
    public class ItemBarcodeController : BaseAuthController
    {
        [Route("GetBarcode")]
        [HttpGet]
        public temOrderQBcode GetBarcode(string Number)
        {
            temOrderQBcode obj = new temOrderQBcode();
            try
            {


                string ItemBorderCode = Number.PadLeft(13, '0');


                //Barcode image into your system
                var barcodeLib = new BarcodeLib.Barcode(ItemBorderCode);
                barcodeLib.Height = 100;
                barcodeLib.Width = 400;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                barcodeLib.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;//
                System.Drawing.Font font = new System.Drawing.Font("Arial Black", 20f);//
                barcodeLib.LabelFont = font;
                barcodeLib.IncludeLabel = true;
                barcodeLib.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                Image imeg = barcodeLib.Encode(TYPE.CODE128, ItemBorderCode);//bytestream
                obj.BarcodeImage = (byte[])(new ImageConverter()).ConvertTo(imeg, typeof(byte[]));

                return obj;
            }

            catch (Exception err)
            {
                return obj;
            }
        }

        [Route("GetItemBarcode")]
        [HttpGet]
        [AllowAnonymous]
        public List<ItemBarcodeDetailDc> GetItemBarcode(int skip, int take, string keyword, bool IsExport)
        {
            List<ItemBarcodeDetailDc> data = new List<ItemBarcodeDetailDc>();
            if (keyword == null)
            {
                keyword = "";
            }
            using (var context = new AuthContext())
            {
                var key = new SqlParameter("@keyword", keyword);
                var skippara = new SqlParameter("@skip", skip);
                var takepara = new SqlParameter("@take", take);
                var IsExportparam = new SqlParameter("@IsExport", IsExport);


                data = context.Database.SqlQuery<ItemBarcodeDetailDc>("EXEC [BatchCode].[GetItemMasterBarcode] @skip, @take, @keyword, @IsExport", skippara, takepara, key, IsExportparam).ToList();
            }
            return data;
        }

        //For generating Barcode number wise Barcode 
        [Route("GetDataBarcode")]
        [HttpGet]
        public temOrderQBcode GetDataBarcode(string Barcode)
        {
            temOrderQBcode obj = new temOrderQBcode();
            if (Barcode != null)
            {
                try
                {


                    string ItemBorderCode = Barcode.PadLeft(13, '0');


                    //Barcode image into your system
                    var barcodeLib = new BarcodeLib.Barcode(ItemBorderCode);
                    barcodeLib.Height = 100;
                    barcodeLib.Width = 400;
                    barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                    barcodeLib.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;//
                    System.Drawing.Font font = new System.Drawing.Font("verdana", 14f);//
                    barcodeLib.LabelFont = font;
                    barcodeLib.IncludeLabel = true;
                    barcodeLib.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                    barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                    Image imeg = barcodeLib.Encode(TYPE.CODE128, ItemBorderCode);//bytestream
                    obj.BarcodeImage = (byte[])(new ImageConverter()).ConvertTo(imeg, typeof(byte[]));

                    return obj;
                }

                catch (Exception err)
                {
                    return obj;
                }
            }
            else
            {
                return null;
            }
        }


        [HttpGet]
        [Route("GetItemForBarcodeA7")]
        public IEnumerable<ItemMaster> GetItemForBarcodeA7(string key)
        {
            List<ItemMaster> ass = new List<ItemMaster>();
            try
            {
                using (AuthContext db = new AuthContext())
                {

                    ass = db.itemMasters.Where(t => t.Deleted == false && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.Number.ToLower().Contains(key.Trim().ToLower()))).ToList();
                    return ass;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class ItemBarcodeDetailDc
    {
        public string Number { get; set; }
        public string itemBaseName { get; set; }
        public string Barcode { get; set; }
        public string LogoUrl { get; set; }
        public int total_rows { get; set; }

        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }

    }
}

