using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/SellerEditPrice")]
    public class SellerEditPriceController : ApiController
    {
        [Route("CityItem")]
        [HttpGet]
        public List<CityBaseItemDc> GetCityItem(int CityId, string keyword)
        {
            var SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@CityId", CityId));
                paramList.Add(new SqlParameter("@Keyword", keyword));
                paramList.Add(new SqlParameter("@SubCatId", SubCatId));
                List<CityBaseItemDc> ItemList = context.Database.SqlQuery<CityBaseItemDc>("exec GetCitySubCatItem @CityId, @Keyword,@SubCatId", paramList.ToArray()).ToList();
                return ItemList;
            }
        }

        [Route("ItemByMRPId")]
        [HttpGet]
        public List<Singleitemdetaildc> ItemByMRPId(int ItemMultiMrpId, int CityId, string Number)
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

        [Route("UpdateCityItems")]
        [HttpPost]
        public string UpdateCityItemsAllHub(List<postupdateitemDC> itemUpdateDc)
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

    }
}
