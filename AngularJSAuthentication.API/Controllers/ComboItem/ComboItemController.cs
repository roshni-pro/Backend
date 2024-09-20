using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model.ComboItem;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.ComboItem
{
    [RoutePrefix("api/ComboItem")]
    public class ComboItemController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [AllowAnonymous]
        [Route("getItem")]
        [HttpGet]
        public dynamic Getlist(string name, int warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                if (warehouseid > 0)
                {
                    var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.active == true && x.itemname.ToLower().Contains(name.ToLower())).Select(x => new { x.UnitPrice, x.MinOrderQty, x.ItemId, itemnameWithMOQ = x.itemname + "  ( MOQ=" + x.MinOrderQty + ")", itemname = x.itemname, x.LogoUrl, x.MRP }).Take(50).ToList();
                    //var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.active == true && x.itemname.ToLower().Contains(name.ToLower())).Select(x => new { x.UnitPrice, x.MinOrderQty, x.ItemId, itemname = x.itemname, x.LogoUrl }).Take(50).ToList();
                    return whdata;
                }
                else
                {
                    return null;
                }

            }
        }



        [AllowAnonymous]
        [Route("UploadcomboImage")]
        [HttpPost]
        public IHttpActionResult UploadcomboImage()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/ComboImage")))
                        //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/ComboImage"));

                        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/ComboImage"), httpPostedFile.FileName);
                        //string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                        //httpPostedFile.SaveAs(LogoUrl);

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/ComboImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/ComboImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/ComboImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        var uploader = new System.Collections.Generic.List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/images/ComboImage";


                        uploader.FirstOrDefault().SaveFileURL = LogoUrl;

                        uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));



                        LogoUrl = "/images/ComboImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in Kisandan Method: " + ex.Message);
                return null;
            }
        }

        [AllowAnonymous]
        [Route("saveItem")]
        [HttpPost]

        public Combo insert(Combo comboItemDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                MongoDbHelper<Combo> mongoDbHelper = new MongoDbHelper<Combo>();
                MongoDbHelper<ComboItemlist> mongoDbHelperitem = new MongoDbHelper<ComboItemlist>();
                var warehouses = db.Warehouses.Where(x => x.WarehouseId == comboItemDc.WarehouseId).FirstOrDefault();
                List<int> Itemids = comboItemDc.ComboItemList.Select(x => x.ItemId).ToList();
                List<AngularJSAuthentication.Model.ItemMaster> itemmaster = new List<AngularJSAuthentication.Model.ItemMaster>();
                if (Itemids != null && Itemids.Any())
                {
                    itemmaster = db.itemMasters.Where(x => Itemids.Contains(x.ItemId) && x.WarehouseId == comboItemDc.WarehouseId).ToList();
                }
                double qty = comboItemDc.ComboItemList.Sum(x => Convert.ToDouble(x.Qty));
                // ComboItemAB item = new ComboItemAB();
                foreach (var item in comboItemDc.ComboItemList)
                {
                    double mrp = 0;
                    mrp = itemmaster != null && itemmaster.Any(x => x.ItemId == item.ItemId) ? itemmaster.FirstOrDefault(x => x.ItemId == item.ItemId).MRP : 0;
                    item.MrpPrice = mrp * Convert.ToDouble(item.Qty);
                    item.RetailerMargin = mrp > 0 && item.MrpPrice >= Convert.ToDouble(item.AfterPercentage) ? Math.Round(((item.MrpPrice - Convert.ToDouble(item.AfterPercentage)) / (item.MrpPrice)) * 100,0) : 0;
                }


                Combo combo = new Combo();
                combo.IsActive = true;
                combo.IsDeleted = false;
                combo.CreatedDate = DateTime.Now;
                combo.CreatedBy = userid;
                combo.WarehouseId = comboItemDc.WarehouseId;
                combo.WarehouseName = warehouses.WarehouseName;
                combo.CreatedDate = DateTime.Now;
                combo.ComboPrice = Math.Round(comboItemDc.ComboPrice, 2);
                combo.ComboName = comboItemDc.ComboName;
                combo.ComboImage = comboItemDc.ComboImage;
                combo.TotalComboPrice = Math.Round(comboItemDc.TotalComboPrice, 2);
                combo.Discount = comboItemDc.Discount;
                combo.ComboImage = comboItemDc.ComboImage;
                combo.SellQty = comboItemDc.SellQty;
                combo.ComboMrpPrice = comboItemDc.ComboItemList.Sum(x => x.MrpPrice);
                combo.ComboRetailerMargin = combo.ComboMrpPrice >= combo.ComboPrice ? Math.Round(((combo.ComboMrpPrice - combo.ComboPrice) / (combo.ComboMrpPrice)) * 100,0) : 0;
                combo.ComboItemList = comboItemDc.ComboItemList;
                mongoDbHelper.Insert(combo);

                return combo;
            }

        }

        [AllowAnonymous]
        [Route("GetList")]
        [HttpGet]
        public HttpResponseMessage GetList()

        {
            try
            {
                using (var con = new AuthContext())
                {
                    var combolist = new MongoDbHelper<Combo>();

                    var combo = combolist.Select(x => x.IsDeleted == false).OrderByDescending(x => x.CreatedDate).ToList();
                    string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    foreach (var item in combo)
                    {
                        item.ComboImage = baseUrl + "" + item.ComboImage;
                    }



                    return Request.CreateResponse(HttpStatusCode.OK, combo);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }


        }

        [AllowAnonymous]
        [Route("GetListforSearch")]
        [HttpGet]
        public async Task<List<Combo>> GetListforSearch(int Warehouseid)
        {
            using (var con = new AuthContext())
            {
                var combolist = new MongoDbHelper<Combo>();
                var combo = combolist.Select(x => x.IsActive == true && x.IsDeleted == false && x.WarehouseId == Warehouseid).ToList();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in combo)
                {
                    item.ComboImage = baseUrl + "" + item.ComboImage;
                }



                return combo;
            }
        }

        [AllowAnonymous]
        [Route("GetListforMobile")]
        [HttpGet]
        public async Task<List<Combo>> GetListforMobile(int Customerid, int Warehouseid)
        {
            using (var con = new AuthContext())
            {
                var combolist = new MongoDbHelper<Combo>();
                var combo = combolist.Select(x => x.IsActive == true && x.IsDeleted == false && x.IsPublish == true && x.WarehouseId == Warehouseid).ToList();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in combo)
                {
                    item.ComboImage = baseUrl + "" + item.ComboImage;
                }



                return combo;
            }
        }




        [AllowAnonymous]
        [Route("GetListforSearchComboName")]
        [HttpGet]
        public async Task<List<Combo>> GetListforSearchComboName(string ComboName)
        {

            using (var con = new AuthContext())
            {
                var combolist = new MongoDbHelper<Combo>();

                var combo = combolist.Select(x => x.IsActive == true && x.IsDeleted == false && x.ComboName.ToLower().Contains(ComboName.ToLower())).ToList();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in combo)
                {
                    item.ComboImage = baseUrl + "" + item.ComboImage;
                }


                return combo;


            }

        }

        [AllowAnonymous]
        [Route("GetListforItem")]
        [HttpGet]
        public async Task<List<Combo>> GetListforItem(string Id)
        {

            using (var con = new AuthContext())
            {
                var combolist = new MongoDbHelper<Combo>();
                var cobid = new ObjectId(Id);

                var combo = combolist.Select(x => x.IsActive == true && x.IsDeleted == false && x.IsPublish == true && x.Id == cobid).ToList();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in combo)
                {
                    item.ComboImage = baseUrl + "" + item.ComboImage;
                }


                return combo;


            }

        }


        [AllowAnonymous]
        [Route("EditGetList")]
        [HttpGet]
        public colistDC GetList(string Id)
        {
            try
            {
                using (var con = new AuthContext())
                {
                    var combolist = new MongoDbHelper<Combo>();
                    var cobid = new ObjectId(Id);

                    //x.Id=Id
                    var combo = combolist.Select(x => x.Id == cobid && x.IsDeleted == false).FirstOrDefault();
                    if (combo != null)
                    {
                        List<int> Itemids = combo.ComboItemList.Select(x => x.ItemId).ToList();
                        List<AngularJSAuthentication.Model.ItemMaster> itemmaster = new List<AngularJSAuthentication.Model.ItemMaster>();
                        if (Itemids != null && Itemids.Any())
                        {
                            itemmaster = con.itemMasters.Where(x => Itemids.Contains(x.ItemId) && x.WarehouseId == combo.WarehouseId).ToList();
                        }

                        string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                        combo.ComboImage = baseUrl + "" + combo.ComboImage;
                        colistDC colistDC = new colistDC
                        {
                            Id = combo.Id.ToString(),
                            ComboImage = combo.ComboImage,
                            ComboName = combo.ComboName,
                            ComboPrice = combo.ComboPrice,
                            CreatedDate = combo.CreatedDate,
                            IsActive = combo.IsActive,
                            IsDeleted = combo.IsDeleted,
                            IsPublish = combo.IsPublish,
                            UpdatedDate = combo.UpdatedDate,
                            WarehouseId = combo.WarehouseId,
                            WarehouseName = combo.WarehouseName,
                            TotalComboPrice = combo.TotalComboPrice,
                            Discount = combo.Discount,
                            SellQty = combo.SellQty,
                            CoItemlist = combo.ComboItemList.Select(x => new comboItemlistDC
                            {
                                ItemId = x.ItemId,
                                itemname = x.itemname,
                                MinOrderQty = x.MinOrderQty,
                                Parcentage = x.Parcentage,
                                ItemImage = x.ItemImage,
                                Qty = x.Qty,
                                UnitPrice = x.UnitPrice,
                                TotalPriceItem = x.TotalPriceItem,
                                AfterPercentage = x.AfterPercentage,
                                Mrp = itemmaster!= null && itemmaster.Any(i=>i.ItemId == x.ItemId)? itemmaster.FirstOrDefault(i => i.ItemId == x.ItemId).MRP:0
                            }).ToList()
                        };
                        return colistDC;
                    }
                    else
                        return null;


                }
            }
            catch (Exception ex)
            {
                return null;
            }


        }

        [HttpPut]
        [Route("EditCombo")]
        public HttpResponseMessage EditCombodash(colistDC comboid)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                MongoDbHelper<Combo> mongoDbHelper = new MongoDbHelper<Combo>();
                if (!string.IsNullOrEmpty(comboid.Id))
                {
                    List<int> Itemids = comboid.CoItemlist.Select(x => x.ItemId).ToList();
                    List<AngularJSAuthentication.Model.ItemMaster> itemmaster = new List<AngularJSAuthentication.Model.ItemMaster>();
                    if (Itemids != null && Itemids.Any())
                    {
                        itemmaster = db.itemMasters.Where(x => Itemids.Contains(x.ItemId) && x.WarehouseId == comboid.WarehouseId).ToList();
                    }

                    var id = new ObjectId(comboid.Id);

                    Combo combo = mongoDbHelper.Select(x => x.Id == id).FirstOrDefault();
                    combo.ComboItemList = new List<ComboItemlist>();
                    combo.UpdatedDate = DateTime.Now;
                    combo.UpdatedBy = userid;
                    combo.SellQty = comboid.SellQty;
                    combo.TotalComboPrice = Math.Round(comboid.TotalComboPrice, 2);
                    combo.ComboPrice = Math.Round(comboid.ComboPrice, 2);
                    decimal totalprice = 0;
                    if (comboid.CoItemlist != null && comboid.CoItemlist.Any())
                    {
                        combo.ComboItemList = comboid.CoItemlist.Select(x => new ComboItemlist
                        {

                            AfterPercentage = x.AfterPercentage,
                            ItemId = x.ItemId,
                            itemname = x.itemname,
                            MinOrderQty = x.MinOrderQty,
                            Parcentage = x.Parcentage,
                            Qty = x.Qty,
                            TotalPriceItem = x.TotalPriceItem,
                            UnitPrice = x.UnitPrice,
                            MrpPrice = itemmaster != null && itemmaster.Any(y => y.ItemId == x.ItemId) ? itemmaster.FirstOrDefault(y => x.ItemId == y.ItemId).MRP * Convert.ToDouble(x.Qty) : 0,
                            RetailerMargin = itemmaster != null && itemmaster.Any(y => y.ItemId == x.ItemId) ? itemmaster.FirstOrDefault(y => x.ItemId == y.ItemId).MRP * Convert.ToDouble(x.Qty) >= Convert.ToDouble(x.AfterPercentage) ? Math.Round((((itemmaster.FirstOrDefault(y => x.ItemId == y.ItemId).MRP * Convert.ToDouble(x.Qty)) - Convert.ToDouble(x.AfterPercentage)) / (itemmaster.FirstOrDefault(y => x.ItemId == y.ItemId).MRP * Convert.ToDouble(x.Qty))) * 100,0) : 0: 0

                        }).ToList();
                        foreach (var item in combo.ComboItemList)
                        {
                            totalprice += item.AfterPercentage;
                        }
                    }
                    combo.ComboPrice = Math.Round(Convert.ToDouble(totalprice), 2);
                    combo.ComboMrpPrice = combo.ComboItemList.Sum(x => x.MrpPrice);
                    combo.ComboRetailerMargin = combo.ComboMrpPrice >= combo.ComboPrice ? Math.Round(((combo.ComboMrpPrice - combo.ComboPrice) / (combo.ComboMrpPrice)) * 100 ,0): 0;
                    mongoDbHelper.ReplaceWithoutFind(combo.Id, combo);
                }



                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
        }



        /// <summary>
        /// Anushka
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsPublish"></param>
        /// <returns></returns>
        [Route("Publishcombo")]
        [HttpGet]
        public async Task<Combo> Publishcombo(string Id, bool IsPublish)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var con = new AuthContext())
            {
                MongoDbHelper<Combo> mongoDbHelper = new MongoDbHelper<Combo>();
                var com = new MongoDbHelper<Combo>();
                var cobid = new ObjectId(Id);
                //x.Id=Id
                var combo = com.Select(x => x.Id == cobid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (combo != null)
                {
                    combo.IsPublish = IsPublish;
                    combo.PublishBy = userid;
                    var res = await mongoDbHelper.ReplaceAsync(combo.Id, combo);
                }
                return combo;
            }

        }


        /// <summary>
        /// Anushka
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsPublish"></param>
        /// <returns></returns>
        [Route("Activecombo")]
        [HttpGet]
        public async Task<Combo> Activecombo(string Id, bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                MongoDbHelper<Combo> mongoDbHelper = new MongoDbHelper<Combo>();
                var com = new MongoDbHelper<Combo>();
                var cobid = new ObjectId(Id);

                var combo = com.Select(x => x.Id == cobid && x.IsDeleted == false).FirstOrDefault();
                if (combo != null)
                {
                    combo.IsActive = IsActive;
                    combo.ActiveBy = userid;
                    var res = await mongoDbHelper.ReplaceAsync(combo.Id, combo);
                }
                return combo;
            }

        }



    }
}