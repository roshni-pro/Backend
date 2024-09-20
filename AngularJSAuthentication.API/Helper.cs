using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;

namespace AngularJSAuthentication.API
{
    public class CommonHelper
    {
        protected static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public static void refreshItemMaster(int warid, int catid)
        {
            try
            {
                CommonHelper h = new CommonHelper();
                using (AuthContext context = new AuthContext())
                {
                    var cachePolicty = new CacheItemPolicy();
                    cachePolicty.AbsoluteExpiration = h.indianTime.AddSeconds(1);
                    ControllerV1.SubCatItemController.WRSITEM item = new ControllerV1.SubCatItemController.WRSITEM();
                    var cache = MemoryCache.Default;
                    var subsubcategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.Categoryid == catid).Select(x => new ControllerV1.SubCatItemController.factorySubSubCategory()
                    {
                        Categoryid = x.Categoryid,
                        SubCategoryId = x.SubCategoryId,
                        SubsubCategoryid = x.SubsubCategoryid,
                        SubsubcategoryName = x.SubsubcategoryName
                    }).ToList();

                    var ItemMasters = context.itemMasters.Where(x => x.active == true && x.Categoryid == catid && x.WarehouseId == warid).Select(x => new ControllerV1.SubCatItemController.factoryItemdata()
                    {
                        WarehouseId = x.WarehouseId,
                        CompanyId = x.CompanyId,
                        Categoryid = x.Categoryid,
                        Discount = x.Discount,
                        ItemId = x.ItemId,
                        ItemNumber = x.Number,
                        itemname = x.SellingUnitName,
                        LogoUrl = x.SellingSku,
                        MinOrderQty = x.MinOrderQty,
                        price = x.price,
                        SubCategoryId = x.SubCategoryId,
                        SubsubCategoryid = x.SubsubCategoryid,
                        TotalTaxPercentage = x.TotalTaxPercentage,
                        UnitPrice = x.UnitPrice,
                        VATTax = x.VATTax,
                        SellingUnitName = x.SellingUnitName,
                        SellingSku = x.SellingSku,
                        HindiName = x.HindiName,
                        marginPoint = x.marginPoint,
                        promoPerItems = x.promoPerItems

                    }).ToList();
                    cache.Remove("CAT" + warid.ToString() + catid.ToString());
                    item.SubsubCategories = subsubcategory;
                    item.ItemMasters = ItemMasters;
                    cache.Add("CAT" + warid.ToString() + catid.ToString(), item, cachePolicty);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
            }
        }

        public static void refreshItemMaster(int warehouseid)
        {
            CommonHelper h = new CommonHelper();
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();

                var cachePolicty = new CacheItemPolicy();
                cachePolicty.AbsoluteExpiration = h.indianTime.AddHours(1);
                var cache = MemoryCache.Default;
                var dbware = context.DbWarehousesubsubcats.Where(x => x.WarehouseId == warehouseid).ToList();

                List<Categories> categories = new List<Categories>();
                foreach (var d in dbware)
                {
                    var subsubcategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.SubsubCategoryid == d.SubsubCategoryid).FirstOrDefault();
                    var cat = context.Categorys.Where(x => x.IsActive == true && x.Deleted == false && x.Categoryid == subsubcategory.Categoryid).FirstOrDefault();
                    if (cat != null)
                    {
                        categories.Add(new Categories()
                        {
                            Categoryid = cat.Categoryid,
                            CategoryName = cat.CategoryName,
                            BaseCategoryId = cat.BaseCategoryId,
                            LogoUrl = cat.LogoUrl
                        });
                    }
                }
                cache.Remove(warehouseid.ToString());
                ibjtosend.Basecats = context.BaseCategoryDb.Where(x => x.IsActive == true && x.Deleted == false).Select(s => new Basecats() { BaseCategoryId = s.BaseCategoryId, BaseCategoryName = s.BaseCategoryName, LogoUrl = s.LogoUrl });
                ibjtosend.Categories = categories;
                ibjtosend.SubCategories = context.SubCategorys.Where(x => x.IsActive == true && x.Deleted == false).Select(s => new SubCategories() { SubCategoryId = s.SubCategoryId, SubcategoryName = s.SubcategoryName, Categoryid = s.Categoryid });
                cache.Add(warehouseid.ToString(), ibjtosend, cachePolicty);
            }
        }

        public static customeritems getItemMaster(int warehouseid)
        {
            CommonHelper h = new CommonHelper();
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();

                var cachePolicty = new CacheItemPolicy();
                cachePolicty.AbsoluteExpiration = h.indianTime.AddSeconds(1);

                var cache = MemoryCache.Default;
                if (cache.Get("CategoryItem".ToString()) == null)
                {
                    cache.Remove("CategoryItem".ToString());
                    /// Warehouse
                    ibjtosend.Basecats = context.BaseCategoryDb.Where(x => x.IsActive == true).Select(s => new Basecats() { BaseCategoryId = s.BaseCategoryId, BaseCategoryName = s.BaseCategoryName, LogoUrl = s.LogoUrl });
                    List<Categories> categories = new List<Categories>();
                    List<SubCategories> subcategories = new List<SubCategories>();
                    var cat = context.Categorys.Where(x => x.IsActive == true).ToList();

                    var itemmasters = context.itemMasters.Where(x => x.active == true && x.Deleted == false && x.WarehouseId == warehouseid).ToList();
                    foreach (var kk in cat)
                    {
                        foreach (var d in itemmasters)
                        {
                            if (kk.Categoryid == d.Categoryid)
                            {
                                if (categories.Count != 0)
                                {
                                    foreach (var dd in categories)
                                    {
                                        if (dd.Categoryid == kk.Categoryid)
                                        {
                                            break;
                                        }
                                    }
                                }
                                categories.Add(new Categories()
                                {
                                    Categoryid = kk.Categoryid,
                                    CategoryName = kk.CategoryName,
                                    BaseCategoryId = kk.BaseCategoryId,
                                    LogoUrl = kk.LogoUrl
                                });
                                break;
                            }
                        }
                    }
                    //foreach (var d in cat)
                    //{
                    //    categories.Add(new Categories()
                    //    {
                    //        Categoryid = d.Categoryid,
                    //        CategoryName = d.CategoryName,
                    //        BaseCategoryId = d.BaseCategoryId,
                    //        LogoUrl = d.LogoUrl
                    //    });
                    //}

                    ibjtosend.Categories = categories;
                    var subcate = context.SubCategorys.Where(x => x.IsActive == true).Select(s => new SubCategories() { SubCategoryId = s.SubCategoryId, SubcategoryName = s.SubcategoryName, Categoryid = s.Categoryid });
                    foreach (var kk in subcate)
                    {
                        foreach (var d in itemmasters)
                        {
                            if (kk.SubCategoryId == d.SubCategoryId)
                            {
                                if (subcategories.Count != 0)
                                {
                                    foreach (var dd in subcategories)
                                    {
                                        if (dd.SubCategoryId == kk.SubCategoryId)
                                        {
                                            break;
                                        }
                                    }
                                }
                                subcategories.Add(new SubCategories()
                                {
                                    SubCategoryId = kk.SubCategoryId,
                                    SubcategoryName = kk.SubcategoryName,
                                    Categoryid = kk.Categoryid,
                                });
                                break;
                            }
                        }
                    }
                    ibjtosend.SubCategories = subcategories;
                    cache.Add("CategoryItem".ToString(), ibjtosend, cachePolicty);
                }
                else
                {
                    ibjtosend = (customeritems)cache.Get("CategoryItem".ToString());
                }
                return ibjtosend;
            }
        }

        /// <summary>
        /// Version 2
        /// Create by 01/02/2019 for new app version 2
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static customeritems getItemMasterv2(int warehouseid, string lang)
        {
            CommonHelper h = new CommonHelper();
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();

                var cachePolicty = new CacheItemPolicy();
                cachePolicty.AbsoluteExpiration = h.indianTime.AddSeconds(1);

                var cache = MemoryCache.Default;
                if (cache.Get("Category".ToString()) == null)
                {
                    cache.Remove("Category".ToString());

                    ibjtosend.Basecats = context.BaseCategoryDb.Where(x => x.IsActive == true).Select(s => new Basecats() { BaseCategoryId = s.BaseCategoryId, BaseCategoryName = s.BaseCategoryName, LogoUrl = s.LogoUrl, HindiName = s.HindiName });
                    foreach (var kk in ibjtosend.Basecats)
                    {
                        if (lang == "hi")
                        {
                            if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                            {
                                kk.BaseCategoryName = kk.HindiName;
                            }
                        }
                    }
                    List<Categories> categories = new List<Categories>();
                    List<SubCategories> subcategories = new List<SubCategories>();
                    List<SubSubCategories> subsubcategories = new List<SubSubCategories>();

                    var cat = context.Categorys.Where(x => x.IsActive == true).ToList();
                    var itemmasters = context.itemMasters.Where(x => x.active == true && x.Deleted == false && x.WarehouseId == warehouseid).ToList();
                    var subCategoryQuery = "select distinct d.Categoryid,  d.CategoryName, a.[SubCategoryId],a.[SubcategoryName] ,a.[HindiName] from SubCategories a inner join SubcategoryCategoryMappings b on a.subCategoryid=b.subCategoryid inner join Categories d on b.Categoryid=d.Categoryid and a.IsActive=1 and b.IsActive =1 and a.Deleted=0 and b.Deleted=0 and d.IsActive=1 and d.Deleted=0";
                    var brandQuery = "select distinct d.SubCategoryId, e.Categoryid, e.CategoryName , a.[SubsubCategoryid],a.[SubsubcategoryName],a.HindiName"
                                        + " from SubsubCategories a inner"
                                        + " join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
                                        + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
                                        + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
                                        + " inner join Categories e on e.Categoryid = c.Categoryid"
                                        + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
                                        " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0";

                    var subcate = context.Database.SqlQuery<SubCategories>(subCategoryQuery).ToList();
                    var subsubcat = context.Database.SqlQuery<SubSubCategories>(brandQuery).ToList();

                    foreach (var kk in cat)
                    {
                        if (lang == "hi")
                        {
                            if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                            {
                                kk.CategoryName = kk.HindiName;
                            }
                        }
                        foreach (var d in itemmasters)
                        {
                            if (kk.Categoryid == d.Categoryid)
                            {
                                if (categories.Count != 0)
                                {
                                    foreach (var dd in categories)
                                    {
                                        if (dd.Categoryid == kk.Categoryid)
                                        {
                                            break;
                                        }
                                    }
                                }
                                categories.Add(new Categories()
                                {
                                    Categoryid = kk.Categoryid,
                                    CategoryName = kk.CategoryName,
                                    BaseCategoryId = kk.BaseCategoryId,
                                    LogoUrl = kk.LogoUrl
                                });
                                break;
                            }
                        }
                    }
                    ibjtosend.Categories = categories;
                    //var subcate = context.SubCategorys.Where(x => x.IsActive == true).Select(s => new SubCategories() { SubCategoryId = s.SubCategoryId, SubcategoryName = s.SubcategoryName, Categoryid = s.Categoryid, HindiName = s.HindiName });
                    foreach (var kk in subcate)
                    {
                        if (lang == "hi")
                        {
                            if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                            {
                                kk.SubcategoryName = kk.HindiName;
                            }
                        }
                        foreach (var d in itemmasters)
                        {
                            if (kk.SubCategoryId == d.SubCategoryId && kk.Categoryid == d.Categoryid)
                            {
                                if (subcategories.Count != 0)
                                {
                                    foreach (var dd in subcategories)
                                    {
                                        if (dd.SubCategoryId == kk.SubCategoryId)
                                        {
                                            break;
                                        }
                                    }
                                }
                                subcategories.Add(new SubCategories()
                                {
                                    SubCategoryId = kk.SubCategoryId,
                                    SubcategoryName = kk.SubcategoryName,
                                    Categoryid = kk.Categoryid,
                                });
                                break;
                            }
                        }
                    }
                    ibjtosend.SubCategories = subcategories;
                    //var subsubcat = context.SubsubCategorys.Where(x => x.IsActive == true).Select(x => new SubSubCategories() { SubSubCategoryId = x.SubsubCategoryid, SubSubcategoryName = x.SubsubcategoryName, HindiName = x.HindiName, SubCategoryId = x.SubCategoryId, Categoryid = x.Categoryid });
                    foreach (var kk in subsubcat)
                    {
                        if (lang == "hi")
                        {
                            if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                            {
                                kk.SubSubcategoryName = kk.HindiName;
                            }
                        }
                        foreach (var d in itemmasters)
                        {
                            if (kk.SubSubCategoryId == d.SubsubCategoryid && kk.Categoryid == d.Categoryid && kk.SubCategoryId == d.SubCategoryId)
                            {
                                if (subsubcategories.Count != 0)
                                {
                                    foreach (var dd in subsubcategories)
                                    {
                                        if (dd.SubSubCategoryId == kk.SubSubCategoryId)
                                        {
                                            break;
                                        }
                                    }
                                }
                                subsubcategories.Add(new SubSubCategories()
                                {
                                    SubSubCategoryId = kk.SubSubCategoryId,
                                    SubCategoryId = kk.SubCategoryId,
                                    SubSubcategoryName = kk.SubSubcategoryName,
                                    Categoryid = kk.Categoryid,
                                    HindiName = kk.HindiName,
                                });
                                break;
                            }
                        }

                    }
                    ibjtosend.SubSubCategories = subsubcategories;

                    cache.Add("Category".ToString(), ibjtosend, cachePolicty);
                }
                else
                {
                    ibjtosend = (customeritems)cache.Get("Category".ToString());
                }
                return ibjtosend;
            }
        }

        public static void refreshCategory()
        {
            using (AuthContext context = new AuthContext())
            {
                var dbware = context.Warehouses.Where(x => x.Deleted == false).ToList();
                foreach (var d in dbware)
                {
                    refreshItemMaster(d.WarehouseId);
                }
            }
        }

        public static void refreshsubsubCategory(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                List<Warehouse> warehouse = context.Warehouses.Where(x => x.Deleted == false).ToList();
                foreach (var w in warehouse)
                {
                    refreshItemMaster(w.WarehouseId, id);
                }
            }
        }
    }
}