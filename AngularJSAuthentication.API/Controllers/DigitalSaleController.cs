using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/digitalsale")]
    public class DigitalSaleController : BaseAuthController
    {
        [Route("ItemAgainstCityMRPId")]
        [HttpGet]
        public IEnumerable<Singleitemdetaildc> ItemAgainstCityMRPId(int ItemMultiMrpId, int CityId, string Number)
        {
            using (var context = new AuthContext())
            {
                ItemMaster itemMaster = new ItemMaster();
                List<Singleitemdetaildc> singleitemdetaildcs = new List<Singleitemdetaildc>();
                List<ItemMaster> itemlist = new List<ItemMaster>();

                if (CityId > 0 && ItemMultiMrpId > 0 && Number != null)
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@ItemMultiMrpId", ItemMultiMrpId));
                    paramList.Add(new SqlParameter("@CityId", CityId));
                    paramList.Add(new SqlParameter("@Number", Number));
                    List<Singleitemdetaildc> ItemList = context.Database.SqlQuery<Singleitemdetaildc>("Get_itemdetailonItemMultiMrpId @ItemMultiMrpId,@CityId,@Number", paramList.ToArray()).ToList();

                    return ItemList;
                }
                else
                {
                    return null;
                }
            }
        }

        [Route("UpdateSingleCityItemMultiHub")]
        [HttpPost]
        public string UpdateSingleCityItemMultiHub(List<postupdateitemDC> itemUpdateDc)
        {
            bool status = false; string Message;

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (itemUpdateDc != null && itemUpdateDc.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        List<SellerActiveProduct> AddSellerActiveProductList = new List<SellerActiveProduct>();

                        List<PrimeItemDc> updatePrimeItemList = new List<PrimeItemDc>();

                        List<ItemMaster> updateItemList = new List<ItemMaster>();
                        List<ItemLimitMaster> updateItemlimitList = new List<ItemLimitMaster>();
                        List<ItemLimitMaster> AddItemLimitList = new List<ItemLimitMaster>();
                        var user = context.Peoples.Where(e => e.PeopleID == userid && e.Active == true && e.Deleted == false).FirstOrDefault();
                        List<int> itemIds = itemUpdateDc.Select(x => x.ItemId).ToList();
                        List<ItemMaster> itemMasterList = context.itemMasters.Where(e => itemIds.Contains(e.ItemId)).ToList();

                        List<int> itemMultiMrpIds = itemMasterList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        List<int> wids = itemMasterList.Select(x => x.WarehouseId).Distinct().ToList();
                        int Cityid = itemMasterList[0].Cityid;
                        var rs = context.RetailerShareDb.Where(r => r.cityid == Cityid).FirstOrDefault();

                        var ItemLimitMasterList = context.ItemLimitMasterDB.Where(e => itemMultiMrpIds.Contains(e.ItemMultiMRPId) && wids.Contains(e.WarehouseId)).ToList();
                        if (itemMasterList != null && itemMasterList.Any())
                        {
                            foreach (var item in itemUpdateDc)
                            {
                                var itemMaster = itemMasterList.Where(e => e.ItemId == item.ItemId).SingleOrDefault();
                                if (itemMaster != null && itemMaster.ItemId > 0 && item.PurchasePrice > 0)
                                {

                                    itemMaster.PurchasePrice = item.PurchasePrice;
                                    double withouttaxvalue = (itemMaster.PurchasePrice / ((100 + itemMaster.TotalTaxPercentage) / 100));
                                    double withouttax = Math.Round(withouttaxvalue, 3);
                                    double netDiscountAmountvalue = (withouttax * (item.Discount / 100));
                                    double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);
                                    itemMaster.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax
                                    itemMaster.WithTaxNetPurchasePrice = Math.Round(itemMaster.NetPurchasePrice * (1 + (itemMaster.TotalTaxPercentage / 100)), 3);//With tax
                                    itemMaster.Discount = item.Discount;
                                    itemMaster.Margin = item.Margin;
                                    var value = itemMaster.PurchasePrice + (itemMaster.PurchasePrice * itemMaster.Margin / 100);
                                    itemMaster.UnitPrice = Math.Round(value, 3);
                                    itemMaster.active = item.active;
                                    itemMaster.ModifiedBy = user.PeopleID;
                                    if (itemMaster.Margin > 0)
                                    {
                                        if (rs != null)
                                        {
                                            var cf = context.RPConversionDb.FirstOrDefault();
                                            if (cf != null)
                                            {
                                                double mv = (itemMaster.PurchasePrice * (item.Margin / 100) * (rs.share / 100) * cf.point);
                                                var value1 = Math.Round(mv, MidpointRounding.AwayFromZero);
                                                itemMaster.marginPoint = Convert.ToInt32(value1);
                                            }
                                        }
                                    }
                                    if (itemMaster.POPurchasePrice != item.POPurchasePrice || itemMaster.POPurchasePrice == null)
                                    {
                                        itemMaster.POPurchasePrice = item.POPurchasePrice;
                                        var itemList = context.itemMasters.Where(i => i.Number == itemMaster.Number && i.ItemMultiMRPId == item.ItemMultiMRPId && i.WarehouseId == itemMaster.WarehouseId && i.ItemId != item.ItemId).ToList();
                                        foreach (var otheritem in itemList)
                                        {
                                            otheritem.POPurchasePrice = item.POPurchasePrice;
                                            otheritem.UpdatedDate = DateTime.Now;
                                            updateItemList.Add(otheritem);
                                        }
                                    }
                                    if (item.PrimePrice > 0)
                                    {

                                        if (!updatePrimeItemList.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == itemMaster.MinOrderQty))
                                        {
                                            PrimeItemDc Pitem = new PrimeItemDc();

                                            Pitem.Cityid = itemMaster.Cityid;
                                            Pitem.CityName = itemMaster.CityName;
                                            Pitem.itemname = itemMaster.itemname;
                                            Pitem.ItemMultiMRPId = itemMaster.ItemMultiMRPId;
                                            Pitem.MinOrderQty = itemMaster.MinOrderQty;
                                            Pitem.MRP = itemMaster.price;
                                            Pitem.UnitPrice = itemMaster.UnitPrice;
                                            Pitem.PrimePercent = 0;
                                            Pitem.PrimePrice = item.PrimePrice;
                                            Pitem.IsActive = item.IsPrimeActive;
                                            updatePrimeItemList.Add(Pitem);
                                        }
                                    }

                                    updateItemList.Add(itemMaster);



                                    var itemLimit = ItemLimitMasterList.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == itemMaster.WarehouseId);
                                    if (itemLimit != null && !updateItemlimitList.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == itemMaster.WarehouseId))
                                    {
                                        itemLimit.ItemlimitQty = item.ItemlimitQty;
                                        itemLimit.IsItemLimit = item.IsItemLimit;
                                        updateItemlimitList.Add(itemLimit);
                                    }
                                    else
                                    {
                                        if (itemLimit == null && !AddItemLimitList.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == itemMaster.WarehouseId))
                                        {
                                            ItemLimitMaster AddLimitData = new ItemLimitMaster();
                                            AddLimitData.CreatedBy = user.DisplayName;
                                            AddLimitData.ModifyBy = user.DisplayName;
                                            AddLimitData.WarehouseId = itemMaster.WarehouseId;
                                            AddLimitData.ItemNumber = itemMaster.ItemNumber;
                                            AddLimitData.ItemMultiMRPId = itemMaster.ItemMultiMRPId;
                                            AddLimitData.ItemlimitQty = item.ItemlimitQty;
                                            AddLimitData.IsItemLimit = item.IsItemLimit;
                                            AddLimitData.CreatedDate = DateTime.Now;
                                            AddLimitData.UpdateDate = DateTime.Now;
                                            AddItemLimitList.Add(AddLimitData);
                                        }
                                    }
                                }
                            }
                            if (updateItemList != null && updateItemList.Any())
                            {

                                if (updatePrimeItemList != null && updatePrimeItemList.Any())
                                {
                                    bool isupdated = updatePrimeItem(updatePrimeItemList, context);
                                }
                                foreach (var item in updateItemList)
                                {
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                if (updateItemlimitList != null && updateItemlimitList.Any())
                                {
                                    foreach (var Itemlimit in updateItemlimitList)
                                    {
                                        context.Entry(Itemlimit).State = EntityState.Modified;
                                    }
                                }
                                if (AddItemLimitList != null && AddItemLimitList.Any())
                                {
                                    context.ItemLimitMasterDB.AddRange(AddItemLimitList);
                                }

                                #region SellerActiveProduct place2
                                var ActiveItemlist = updateItemList.Where(x => x.active == true).ToList();
                                foreach (var ActiveItem in ActiveItemlist)
                                {
                                    if (ActiveItem.active)
                                    {
                                        bool IsSellerActiveProducts = context.SellerActiveProducts.Any(x => x.ItemNumber == ActiveItem.Number && x.WarehouseId == ActiveItem.WarehouseId);
                                        if (!IsSellerActiveProducts && !AddSellerActiveProductList.Any(x => x.ItemNumber == ActiveItem.Number && x.WarehouseId == ActiveItem.WarehouseId))
                                        {
                                            SellerActiveProduct SellerItem = new SellerActiveProduct();
                                            SellerItem.ItemNumber = ActiveItem.Number;
                                            SellerItem.CreatedBy = userid;
                                            SellerItem.ActivatedDate = DateTime.Now;
                                            SellerItem.WarehouseId = ActiveItem.WarehouseId;
                                            AddSellerActiveProductList.Add(SellerItem);
                                        }
                                    }
                                }
                                if (AddSellerActiveProductList != null && AddSellerActiveProductList.Any())
                                {
                                    context.SellerActiveProducts.AddRange(AddSellerActiveProductList);
                                }
                                #endregion
                            }
                            if (context.Commit() > 0)
                            {
                                status = true;
                                dbContextTransaction.Complete();
                            }
                        }
                    }
                }
            }
            if (status)
            {
                Message = "Record updated Successfuly";
                return Message;
            }
            else
            {
                Message = "Something went wrong.Do refresh and retry. ";
                return Message;
            }
        }

        [Route("CitySubcatMRPItem")]
        [HttpGet]
        public HttpResponseMessage CitySubcatMRPItem(int CityId, int SubCatId)
        {
            List<CitySubcatMRPItemDc> CitySubcatMRPItemList = new List<CitySubcatMRPItemDc>();
            if (CityId > 0 && SubCatId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@CityId", CityId));
                    paramList.Add(new SqlParameter("@SubCatId", SubCatId));
                    CitySubcatMRPItemList = context.Database.SqlQuery<CitySubcatMRPItemDc>("GetCitySubcatMRPItems @CityId, @SubCatId", paramList.ToArray()).ToList();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, CitySubcatMRPItemList);
        }

        [Route("GetPrimeItem")]
        [HttpGet]
        public List<PrimeItemDc> GetPrimeItem(int ItemMultiMrpId, int CityId)
        {
            List<PrimeItemDc> primeItemDcs = new List<PrimeItemDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GePrimeItems]";
                cmd.Parameters.Add(new SqlParameter("@itemMultiMRPId", ItemMultiMrpId));
                cmd.Parameters.Add(new SqlParameter("@cityId", CityId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                primeItemDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PrimeItemDc>(reader).ToList();

            }
            return primeItemDcs;
        }

        [Route("UpdatePrimeItems")]
        [HttpPost]
        public bool UpdatePrimeItems(List<PrimeItemDc> primeItemDcs)
        {
            bool result = false;
            if (primeItemDcs != null && primeItemDcs.Any())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (var context = new AuthContext())
                {
                    var cityid = primeItemDcs.FirstOrDefault().Cityid;
                    var itemmrpIds = primeItemDcs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    var minOrderqtys = primeItemDcs.Select(x => x.MinOrderQty).Distinct().ToList();
                    var primeitemdb = context.PrimeItemDetails.Where(x => x.CityId == cityid && itemmrpIds.Contains(x.ItemMultiMRPId) && minOrderqtys.Contains(x.MinOrderQty)).ToList();
                    foreach (var item in primeItemDcs)
                    {
                        if (primeitemdb != null && primeitemdb.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty))
                        {
                            var primeitem = primeitemdb.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty);
                            primeitem.PrimePrice = item.PrimePercent > 0 ? 0 : item.PrimePrice;
                            primeitem.PrimePercent = item.PrimePercent;
                            primeitem.IsActive = (item.PrimePercent > 0 || item.PrimePrice > 0) ? item.IsActive : false;
                            primeitem.IsDeleted = false;
                            primeitem.ModifiedBy = userid;
                            primeitem.ModifiedDate = DateTime.Now;
                            context.Entry(primeitem).State = EntityState.Modified;
                        }
                        else
                        {
                            var primeitem = new PrimeItemDetail
                            {
                                CreatedDate = DateTime.Now,
                                CreatedBy = userid,
                                CityId = item.Cityid,
                                IsActive = (item.PrimePercent > 0 || item.PrimePrice > 0) ? item.IsActive : false,
                                IsDeleted = false,
                                ItemMultiMRPId = item.ItemMultiMRPId,
                                MinOrderQty = item.MinOrderQty,
                                PrimePrice = item.PrimePercent > 0 ? 0 : item.PrimePrice,
                                PrimePercent = item.PrimePercent,
                                WarehouseId = 0,
                            };
                            context.PrimeItemDetails.Add(primeitem);
                        }
                        result = context.Commit() > 0;
                    }

                }
            }
            return result;
        }

        [Route("GetCityAllPrimeItem")]
        [HttpGet]
        public List<PrimeItemDc> GetCityAllPrimeItem(int CityId)
        {
            List<PrimeItemDc> primeItemDcs = new List<PrimeItemDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GeCityAllPrimeItems]";
                cmd.Parameters.Add(new SqlParameter("@cityId", CityId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                primeItemDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<PrimeItemDc>(reader).ToList();

            }
            return primeItemDcs;
        }




        public bool updatePrimeItem(List<PrimeItemDc> primeItemDcs, AuthContext context)
        {
            bool result = false;
            if (primeItemDcs != null && primeItemDcs.Any())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var cityid = primeItemDcs.FirstOrDefault().Cityid;
                var itemmrpIds = primeItemDcs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                var minOrderqtys = primeItemDcs.Select(x => x.MinOrderQty).Distinct().ToList();
                var primeitemdb = context.PrimeItemDetails.Where(x => x.CityId == cityid && itemmrpIds.Contains(x.ItemMultiMRPId) && minOrderqtys.Contains(x.MinOrderQty)).ToList();
                foreach (var item in primeItemDcs)
                {
                    if (primeitemdb != null && primeitemdb.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty))
                    {
                        var primeitem = primeitemdb.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty);
                        primeitem.PrimePrice = item.PrimePercent > 0 ? 0 : item.PrimePrice;
                        primeitem.PrimePercent = item.PrimePercent;
                        primeitem.IsActive = (item.PrimePercent > 0 || item.PrimePrice > 0) ? item.IsActive : false;
                        primeitem.IsDeleted = false;
                        primeitem.ModifiedBy = userid;
                        primeitem.ModifiedDate = DateTime.Now;
                        context.Entry(primeitem).State = EntityState.Modified;
                    }
                    else
                    {
                        var primeitem = new PrimeItemDetail
                        {
                            CreatedDate = DateTime.Now,
                            CreatedBy = userid,
                            CityId = item.Cityid,
                            IsActive = (item.PrimePercent > 0 || item.PrimePrice > 0) ? item.IsActive : false,
                            IsDeleted = false,
                            ItemMultiMRPId = item.ItemMultiMRPId,
                            MinOrderQty = item.MinOrderQty,
                            PrimePrice = item.PrimePercent > 0 ? 0 : item.PrimePrice,
                            PrimePercent = item.PrimePercent,
                            WarehouseId = 0,
                        };
                        context.PrimeItemDetails.Add(primeitem);
                    }
                    result = context.Commit() > 0;
                }
            }
            return result;
        }

    }
    public class ItemUpdateDc
    {
        public int WarehouseId { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Number { get; set; }
        public double price { get; set; }
        public int BillLimitQty { get; set; }
        public bool active { get; set; }
    }

    public class CitySubcatMRPItemDc
    {
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string itemname { get; set; }
    }
    public class Singleitemdetaildc
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Number { get; set; }
        public string SellingSku { get; set; }
        public string SellingUnitName { get; set; }
        public int? ItemlimitQty { get; set; }
        public bool? IsItemLimit { get; set; }

        public double MRP { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public double NetPurchasePrice { get; set; }
        public double PurchasePrice { get; set; }
        public double? POPurchasePrice { get; set; }
        public double? AveragePurchasePrice { get; set; }
        public double Margin { get; set; }
        public double TotalTaxPercentage { get; set; }
        public bool active { get; set; }
        public decimal PrimePrice { get; set; }
        public bool IsPrimeActive { get; set; }
        public int MinOrderQty { get; set; }


    }
    public class postupdateitemDC
    {
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double POPurchasePrice { get; set; }
        public double Discount { get; set; }
        public double Margin { get; set; }
        public double UnitPrice { get; set; }
        public int ItemlimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public bool active { get; set; }
        public double PurchasePrice { get; set; }
        public decimal PrimePrice { get; set; }
        public bool IsPrimeActive { get; set; }

    }

    public class PrimeItemDc
    {
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int MinOrderQty { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public decimal PrimePercent { get; set; }
        public decimal PrimePrice { get; set; }
        public bool IsActive { get; set; }
    }
}
