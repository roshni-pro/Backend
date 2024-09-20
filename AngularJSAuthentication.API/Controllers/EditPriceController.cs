using AngularJSAuthentication.BusinessLayer.Managers.JustInTime;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.SalesApp;
using Nito.AspNetBackgroundTasks;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/editPrice")]
    public class EditPriceController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("")]
        public IEnumerable<ItemMaster> Get()
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllItemMaster(CompanyId).ToList();
                    logger.Info("End  Item Master: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }
        [Authorize]
        [ResponseType(typeof(ItemMaster))]
        [Route("")]
        public IEnumerable<ItemMaster> Get(string cityid, string categoryid, string subcategoryid, string subsubcategoryid)
        {
            logger.Info("start ItemMaster: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.filteredItemMaster(categoryid, subcategoryid, subsubcategoryid, CompanyId).ToList();
                    logger.Info("End ItemMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        //[Authorize]
        //[ResponseType(typeof(ItemMaster))]
        //[Route("")]
        //[AcceptVerbs("PUT")]
        //public ItemMaster Put(List<ItemMaster> item)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        return context.Saveediteditem(item,compid);
        //        //context.Saveediteditem(item);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        [Authorize]
        [Route("byid")]
        [HttpPut]
        public async Task<ItemResponseMsg> marginupdate(FutureItemMasterDTO data)
        {
            var result = new ItemResponseMsg();
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                List<string> Roles = new List<string>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var UserName = db.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active).DisplayName;
                var item = db.itemMasters.FirstOrDefault(i => i.ItemId == data.ItemId && i.WarehouseId == data.WarehouseId);
                if (item != null && UserName != null)
                {
                    #region ItemIncentiveClassificationMargin
                    //var numbers = new List<string>();
                    //numbers.Add(item.ItemNumber);
                    var itemMultiMRPIds = new List<int>();
                    itemMultiMRPIds.Add(item.ItemMultiMRPId);
                    //ItemMasterManager itemMasterManager = new ItemMasterManager();
                    //var ItemIncentiveClassificationMargin = await itemMasterManager.GetItemIncentiveClassificationMargin(data.WarehouseId, itemMultiMRPIds);
                    JITLiveItemManager jitManager = new JITLiveItemManager();
                   
                    OpenMoqFilterDc openMoqFilterDc = new OpenMoqFilterDc();
                    openMoqFilterDc.WarehouseId = data.WarehouseId;
                    openMoqFilterDc.ItemMultiMrpId = item.ItemMultiMRPId;
                    var ItemIncentiveClassificationMargin = await jitManager.GetOpenMoqListAsync(openMoqFilterDc);

                    if (ItemIncentiveClassificationMargin != null && ItemIncentiveClassificationMargin.Any(x => x.RetailerRMargin > 0 && data.ItemId == x.ItemId))
                    {
                        if (data.Margin < ItemIncentiveClassificationMargin.FirstOrDefault(x=> x.RetailerRMargin > 0 && data.ItemId == x.ItemId).RetailerRMargin)
                        {
                            result.Status = false;
                            result.Message = "Margin can't be less than company MinMarginPercent. :" + item.itemname + "  FOR MOQ : " + item.MinOrderQty;
                            return result;
                        }
                    }
                    #endregion
                    if (item.POPurchasePrice != data.POPurchasePrice || item.POPurchasePrice == null)
                    {
                        item.POPurchasePrice = data.POPurchasePrice;
                        var itemList = db.itemMasters.Where(i => i.ItemMultiMRPId == item.ItemMultiMRPId && i.WarehouseId == data.WarehouseId && i.ItemId != item.ItemId && i.Deleted == false).ToList();
                        foreach (var otheritem in itemList)
                        {
                            otheritem.POPurchasePrice = item.POPurchasePrice;
                            otheritem.UpdatedDate = indianTime;
                            db.Entry(otheritem).State = EntityState.Modified;
                        }
                    }
                    double withouttaxvalue = (data.PurchasePrice / ((100 + data.TotalTaxPercentage) / 100));
                    double withouttax = Math.Round(withouttaxvalue, 3);
                    double netDiscountAmountvalue = (withouttax * (data.Discount / 100));
                    double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);
                    item.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax
                    item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (data.TotalTaxPercentage / 100)), 3);//With tax                                                                                            //Math.Round((withouttax - netDiscountAmount), 3);// with tax
                    item.Discount = data.Discount;
                    item.Margin = data.Margin;
                    item.price = data.price;
                    item.PurchasePrice = data.PurchasePrice;
                    var value = data.PurchasePrice + (data.PurchasePrice * data.Margin / 100);
                    item.UnitPrice = Math.Round(value, 3);
                    item.userid = userid;
                    item.UpdatedDate = indianTime;
                    if (item.Margin > 0)
                    {
                        var rs = db.RetailerShareDb.Where(r => r.cityid == item.Cityid).FirstOrDefault();
                        if (rs != null)
                        {
                            var cf = db.RPConversionDb.FirstOrDefault();
                            try
                            {
                                double mv = (item.PurchasePrice * (item.Margin / 100) * (rs.share / 100) * cf.point);
                                var value1 = Math.Round(mv, MidpointRounding.AwayFromZero);
                                item.marginPoint = Convert.ToInt32(value1);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }
                        }
                    }
                    //futurepay
                    if (data.eFutureType != null && data.eFutureValue > 0)
                    {
                        ItemFutureEarning ItemFutureEarning = new ItemFutureEarning();

                        ItemFutureEarning.WarehouseId = item.WarehouseId;
                        ItemFutureEarning.ItemNumber = item.ItemNumber;

                        ItemFutureEarning.eFutureType = data.eFutureType;
                        ItemFutureEarning.eFutureValue = data.eFutureValue;

                        ItemFutureEarning.CreatedBy = item.userid;
                        ItemFutureEarning.SupplierId = item.SupplierId;
                        ItemFutureEarning.CreatedDate = indianTime;
                        ItemFutureEarning.Deleted = false;

                        db.ItemFutureEarning.Add(ItemFutureEarning);

                        //update in itemmasters
                        item.eFutureType = data.eFutureType;
                        item.eFutureValue = data.eFutureValue;
                    }
                    #region item price drop Entry for sales app
                    if (item.UnitPrice > 0 && data.UnitPrice > 0)
                    {
                        if (item.UnitPrice < data.UnitPrice)
                        {
                            ItemPriceDrop itemPriceDrop = new ItemPriceDrop();
                            itemPriceDrop.UnitPrice = item.UnitPrice;
                            itemPriceDrop.OldUnitPrice = data.UnitPrice;
                            itemPriceDrop.WarehouseId = item.WarehouseId;
                            itemPriceDrop.ItemId = item.ItemId;
                            itemPriceDrop.IsActive = true;
                            itemPriceDrop.IsDeleted = false;
                            itemPriceDrop.CreatedBy = userid;
                            itemPriceDrop.CreatedDate = DateTime.Now;
                            db.ItemPriceDrops.Add(itemPriceDrop);
                        }
                    }
                    #endregion
                    db.Entry(item).State = EntityState.Modified;
                    result.Status = db.Commit() > 0;
                    result.Message = "record updated successfully.";
                    result.data = item;
                    #region // Send Notification

                    if (item.UnitPrice <= item.WithTaxNetPurchasePrice)
                    {
                        var msg = "Selling price should be always greater than or equal to Net purchase price . Current Selling price is less than from Net Purchase price";
                        BackgroundTaskManager.Run(() => SendMailEditPriceNotifications(item, msg, UserName));

                    }
                    else if (item.UnitPrice >= item.price)
                    {
                        var msg = "Selling price should be less than or equal to MRP price. Current Selling price is greater from MRP Price";
                        BackgroundTaskManager.Run(() => SendMailEditPriceNotification(item, msg, UserName));
                    }
                    #endregion
                }
                else
                {
                    result.Message = "something went wrong or user is inactive.";

                }
                return result;
            }
        }

        #region SendMailCreditWalletNotifications
        //SendMailCreditWalletNotification
        public static void SendMailEditPriceNotifications(ItemMaster item, string message, string UserName)
        {

            try
            {

                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];

                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! " + item.itemname + "</h3> " + message;
                body += "Hello,";
                body += "<p><strong>";
                body += item.UnitPrice + "</strong>" + " : Is Selling Price and Net Purchase Price is :" + item.WithTaxNetPurchasePrice + "</p>";
                body += "<p>Price Updated By user Name : <strong>" + UserName + "</strong> Date <strong>" + item.UpdatedDate + "</strong></p>";
                body += "Thanks,";
                body += "<br />";
                body += "<b>IT Team</b>";
                body += "</div>";

                var Subj = "Alert! " + item.itemname + "  " + message;
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                //msg.To.Add("deepak@shopkirana.com");
                msg.To.Add("manasi@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);

            }
            catch (Exception ss)
            {

            }


        }
        #endregion

        #region SendMailCreditWalletNotification
        //SendMailCreditWalletNotification
        public static void SendMailEditPriceNotification(ItemMaster item, string message, string UserName)
        {
            try
            {

                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];

                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! " + item.itemname + "</h3> " + message;
                body += "Hello,";
                body += "<p><strong>";
                body += item.UnitPrice + "</strong>" + " : Is Selling Price and MRP Price is :" + item.price + "</p>";
                body += "<p>Price Updated By user Name : <strong>" + UserName + "</strong> Date <strong>" + item.UpdatedDate + "</strong></p>";
                body += "Thanks,";
                body += "<br />";
                body += "<b>IT Team</b>";
                body += "</div>";

                var Subj = "Alert! " + item.itemname + "  " + message;
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                //msg.To.Add("deepak@shopkirana.com");
                msg.To.Add("manasi@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);

            }
            catch (Exception ss) { }


        }
        #endregion

        [Route("GetSubCategoryMapping")]
        [HttpGet]
        public List<SubCategoryDcF> GetSubCategoryMapping()
        {
            using (var db = new AuthContext())
            {
                var result = db.Database.SqlQuery<SubCategoryDcF>("exec GetSubCategoryMapping").ToList();


                //var Catv = db.Categorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                //var Scatv = db.SubCategorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                //var SubcategoryCategoryMapping = db.SubcategoryCategoryMappingDb.Where(x => x.IsActive == true && x.Deleted == false).ToList();
                //var subCatCategoryDcs = (from a in Scatv
                //                         join b in SubcategoryCategoryMapping on a.SubCategoryId equals b.SubCategoryId
                //                         join c in Catv on b.Categoryid equals c.Categoryid
                //                         select new SubCategory
                //                         {
                //                             Categoryid = c.Categoryid,
                //                             CategoryName = c.CategoryName,
                //                             CreatedDate = a.CreatedDate,
                //                             Code = a.Code,
                //                             CreatedBy = a.CreatedBy,
                //                             Deleted = a.Deleted,
                //                             Discription = a.Discription,
                //                             HindiName = a.HindiName,
                //                             IsActive = a.IsActive,
                //                             IsPramotional = a.IsPramotional,
                //                             LogoUrl = a.LogoUrl,
                //                             SubCategoryId = a.SubCategoryId,
                //                             SubcategoryName = a.SubcategoryName,
                //                             UpdateBy = a.UpdateBy,
                //                             UpdatedDate = a.UpdatedDate
                //                         }).Distinct().ToList();

                return result;
            }
        }

        [Route("GetSubsubCategoryMapping")]
        [HttpGet]
        public List<SubSubCategoryDcF> GetSubsubCategoryMapping()
        {
            using (var db = new AuthContext())
            {
                var result = db.Database.SqlQuery<SubSubCategoryDcF>("exec GetSubsubCategoryMapping").ToList();


                //var Catv = db.Categorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                //var Scatv = db.SubCategorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                //var SsCatv = db.SubsubCategorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                //var SubcategoryCategoryMapping = db.SubcategoryCategoryMappingDb.Where(x => x.IsActive == true && x.Deleted == false).ToList();
                //var SubsubcategoryCategoryMapping = db.BrandCategoryMappingDb.Where(x => x.IsActive == true && x.Deleted == false).ToList();
                //var subSubCatCategoryDcs = (from a in SsCatv
                //                            join b in SubsubcategoryCategoryMapping on a.SubsubCategoryid equals b.SubsubCategoryId
                //                            join c in SubcategoryCategoryMapping on b.SubCategoryMappingId equals c.SubCategoryMappingId
                //                            join d in Scatv on c.SubCategoryId equals d.SubCategoryId
                //                            join e in Catv on c.Categoryid equals e.Categoryid
                //                            select new SubsubCategory
                //                            {
                //                                AgentCommisionPercent = a.AgentCommisionPercent,
                //                                BaseCategoryId = a.BaseCategoryId,
                //                                Categoryid = e.Categoryid,
                //                                CategoryName = e.CategoryName,
                //                                CommisionPercent = a.CommisionPercent,
                //                                Deleted = a.Deleted,
                //                                Code = a.Code,
                //                                HindiName = a.HindiName,
                //                                IsActive = a.IsActive,
                //                                LogoUrl = a.LogoUrl,
                //                                IsExclusive = a.IsExclusive,
                //                                SubCategoryId = d.SubCategoryId,
                //                                SubcategoryName = d.SubcategoryName,
                //                                SubsubCategoryid = a.SubsubCategoryid,
                //                                SubsubcategoryName = a.SubsubcategoryName,
                //                                Type = a.Type
                //                            }).Distinct().ToList();

                return result;
            }
        }


        [Route("UpdateSupplier")]
        [HttpPut]

        public HttpResponseMessage UpdateSupplierOnItem(ItemEditDc itemdata)
        {

            using (AuthContext db = new AuthContext())
            {
                if (itemdata.ItemId > 0 && itemdata.SupplierId > 0 && itemdata.DepoId > 0)
                {
                    var item = db.itemMasters.Where(i => i.ItemId == itemdata.ItemId && i.Deleted == false).SingleOrDefault();
                    if (item != null)
                    {
                        var supplier = db.Suppliers.Where(x => x.SupplierId == itemdata.SupplierId).SingleOrDefault();
                        var depo = db.DepoMasters.Where(x => x.DepoId == itemdata.DepoId).SingleOrDefault();
                        if (supplier != null && depo != null)
                        {

                            item.DepoId = depo.DepoId;
                            item.SupplierId = supplier.SupplierId;
                            var itemList = db.itemMasters.Where(i => i.Deleted == false && i.WarehouseId == item.WarehouseId && i.Number == item.Number).ToList();
                            foreach (var otheritem in itemList)
                            {
                                otheritem.SupplierId = itemdata.SupplierId;
                                otheritem.DepoId = itemdata.DepoId;
                                otheritem.UpdatedDate = indianTime;
                                db.Entry(otheritem).State = EntityState.Modified;
                            }

                            item.UpdatedDate = indianTime;
                            db.Entry(item).State = EntityState.Modified;
                            db.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, "Item Updated Successfully");
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Item Updated Successfully");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Item not found");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Some Thing Went wrong");
                }
            }

        }

        public class FutureItemMasterDTO
        {
            public int ItemId { get; set; }
            public int WarehouseId { get; set; }
            public double price { get; set; }
            public double Discount { get; set; }
            public double UnitPrice { get; set; }
            public string Number { get; set; }
            public double PurchasePrice { get; set; }
            public double Margin { get; set; }
            public double WithTaxNetPurchasePrice { get; set; }
            public double TotalTaxPercentage { get; set; }
            public string eFutureType { get; set; }
            public double eFutureValue { get; set; }
            public double POPurchasePrice { get; set; }


            public int SupplierId { get; set; }
            public int DepoId { get; set; }


        }

        public class subCatCategoryDc
        {
            public int CategoryId { get; set; }
            public int SubCategoryId { get; set; }
        }


        public class subSubCatCategoryDc
        {
            public int CategoryId { get; set; }
            public int SubCategoryId { get; set; }
            public int SubSubCategoryId { get; set; }
        }


        public class ItemEditDc
        {
            public int ItemId { get; set; }
            public int DepoId { get; set; }
            public int SupplierId { get; set; }
        }
        public class SubCategoryDcF
        {
            public int SubCategoryId { get; set; }
            public string SubcategoryName { get; set; }
            public int Categoryid { get; set; }
            public string CategoryName { get; set; }
        }
        public class SubSubCategoryDcF
        {
            public int SubsubCategoryid { get; set; }
            public string SubsubcategoryName { get; set; }
            public int SubCategoryId { get; set; }
            public string SubcategoryName { get; set; }
            public int Categoryid { get; set; }
            public string CategoryName { get; set; }

        }


    }
}