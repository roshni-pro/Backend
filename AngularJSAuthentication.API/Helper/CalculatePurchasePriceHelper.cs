using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.Model.JustInTime;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class CalculatePurchasePriceHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        #region Get  CalculatePurchasePrice 
        public async Task<List<CalculatePurchasePriceDc>> GetCalculatePurchasePrice(AuthContext context, int WarehouseId, List<int> ItemMultiMrpIds)
        {
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            List<CalculatePurchasePriceDc> result = new List<CalculatePurchasePriceDc>();
            var orderIdDt = new DataTable();
            orderIdDt.Columns.Add("IntValue");
            foreach (var item in ItemMultiMrpIds)
            {
                var dr = orderIdDt.NewRow();
                dr["IntValue"] = item;
                orderIdDt.Rows.Add(dr);
            }
            var param = new SqlParameter("ItemMultiMrpIds", orderIdDt);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "dbo.IntValues";
            var wid = new SqlParameter("@WarehouseId", WarehouseId);
            result = await context.Database.SqlQuery<CalculatePurchasePriceDc>("exec GetCalculatePurchasePrice @WarehouseId,  @ItemMultiMrpIds", wid, param).ToListAsync();
            return result;
        }
        #endregion
        #region On CalculatePPOnInternalTransfer  CalculatePurchasePriceDaily) 
        public bool CalculatePPOnInternalTransferForRisk(AuthContext context, List<RiskCalculatePurchasePriceDc> items, int RequestToWarehouseId, int TType, int UserId, bool IsManual = false)
        {
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemMultiMRPId");
            dt.Columns.Add("WarehouseId");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Price");
            foreach (var item in items)
            {
                var dr = dt.NewRow();
                dr["ItemMultiMRPId"] = item.ItemMultiMrpId;
                dr["WarehouseId"] = item.WarehouseId;
                dr["Qty"] = item.Qty;
                dr["Price"] = item.Price;
                dt.Rows.Add(dr);
            }
            var sTableParam = new SqlParameter("RiskItem", dt);
            sTableParam.SqlDbType = SqlDbType.Structured;
            sTableParam.TypeName = "dbo.RiskItemType";
            var FromWarehouseId = new SqlParameter("FromWarehouseId", RequestToWarehouseId);
            var type = new SqlParameter("type", TType);
            var pIsManual = new SqlParameter("IsManual", IsManual);
            var pUserId = new SqlParameter("UserId", UserId);
            context.Database.CommandTimeout = 300;
            context.Database.ExecuteSqlCommand("CalculatePurchasePriceDailyWithRisk  @RiskItem , @FromWarehouseId, @type , @IsManual, @UserId", sTableParam, FromWarehouseId, type, pIsManual, pUserId);

            List<int> ItemMultiMrpIds = new List<int>();
            ItemMultiMrpIds.AddRange(items.Select(x => x.ItemMultiMrpId).ToList());
            GetCalculatePPAndUpdateItemMaster(context, items.FirstOrDefault().WarehouseId, ItemMultiMrpIds);


            return true;
        }
        public bool CalculatePPOnInternalTransferForIR(AuthContext context, int WarehouseId, int ObjectType, int ObjectId, int userId)
        {
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            var PWarehouseId = new SqlParameter("WarehouseId", WarehouseId);
            var PObjectType = new SqlParameter("Objecttype", ObjectType);
            var PObjectId = new SqlParameter("ObjectId", ObjectId);
            var uid = new SqlParameter("UserId", userId);

            context.Database.CommandTimeout = 300;
            context.Database.ExecuteSqlCommand("CalculatePurchasePriceDaily  @WarehouseId , @ObjectType, @ObjectId, @UserId", PWarehouseId, PObjectType, PObjectId, uid);
            return true;
        }
        #endregion
        public bool UpdateRisk(AuthContext context, List<UpdateRiskDc> UpdateRisk, int UserId)
        {

            foreach (var item in UpdateRisk)
            {
                List<RiskCalculatePurchasePriceDc> Items = new List<RiskCalculatePurchasePriceDc>();
                Items.Add(new RiskCalculatePurchasePriceDc
                {
                    ItemMultiMrpId = item.ItemMultiMrpId,
                    Price = item.RiskPurchasePrice,
                    Qty = item.RiskQuantity,
                    WarehouseId = item.WarehouseId
                });
                CalculatePPOnInternalTransferForRisk(context, Items, 0, item.RiskType, UserId, true);
            }

            List<int> ItemMultiMrpIds = new List<int>();
            ItemMultiMrpIds.Add(UpdateRisk.FirstOrDefault().ItemMultiMrpId);
            GetCalculatePPAndUpdateItemMaster(context, UpdateRisk.FirstOrDefault().WarehouseId, ItemMultiMrpIds);
            return true;
        }
        public bool GetCalculatePPAndUpdateItemMaster(AuthContext context, int WarehouseId, List<int> ItemMultiMrpIds)
        {
            List<CalculatePPAndUpdateItemMaster> result = new List<CalculatePPAndUpdateItemMaster>();
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            var orderIdDt = new DataTable();
            orderIdDt.Columns.Add("IntValue");
            foreach (var item in ItemMultiMrpIds)
            {
                var dr = orderIdDt.NewRow();
                dr["IntValue"] = item;
                orderIdDt.Rows.Add(dr);
            }
            var param = new SqlParameter("ItemMultiMrpIds", orderIdDt);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "dbo.IntValues";
            var wid = new SqlParameter("@WarehouseId", WarehouseId);
            result = context.Database.SqlQuery<CalculatePPAndUpdateItemMaster>("exec GetCalculatePPAndItemMaster @WarehouseId,  @ItemMultiMrpIds", wid, param).ToList();
            if (result != null && result.Any())
            {
                List<int> ItemIds = result.Select(x => x.ItemId).ToList();
                var itemmasters = context.itemMasters.Where(x => ItemIds.Contains(x.ItemId)).ToList();
                foreach (var item in itemmasters)
                {
                    var UpdateItem = result.Where(x => x.ItemId == item.ItemId).FirstOrDefault();
                    if (UpdateItem != null)
                    {
                        double oldNetPurchasePrice = item.NetPurchasePrice;
                        double oldWithTaxNetPurchasePrice = item.WithTaxNetPurchasePrice;
                        double oldPurchasePrice = item.PurchasePrice;
                        double oldUnitPrice = item.UnitPrice;
                        double oldTradePrice = item.TradePrice.HasValue && item.TradePrice > 0 ? (double)item.TradePrice : item.UnitPrice;
                        double oldWholeSalePrice = item.WholeSalePrice.HasValue && item.WholeSalePrice > 0 ? (double)item.WholeSalePrice : item.UnitPrice;

                        double withouttaxvalue = (UpdateItem.PurchasePrice / ((100 + item.TotalTaxPercentage) / 100));
                        double withouttax = Math.Round(withouttaxvalue, 3);
                        double netDiscountAmountvalue = (withouttax * (item.Discount / 100));
                        double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);

                        item.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax

                        item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax                                                                                            //Math.Round((withouttax - netDiscountAmount), 3);// with tax

                        item.PurchasePrice = UpdateItem.PurchasePrice;

                        item.UnitPrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * item.Margin / 100), 3);
                        item.TradePrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * (item.TradeMargin ?? 0) / 100), 3);
                        item.WholeSalePrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * (item.WholesaleMargin ?? 0) / 100), 3);
                        item.UpdatedDate = DateTime.Now;
                        if (item.PurchasePrice < item.price && (!(item.UnitPrice > item.MRP || item.TradePrice > item.MRP || item.WholeSalePrice > item.MRP))
                         //   && (item.UnitPrice > oldUnitPrice || item.TradePrice > oldTradePrice || item.WholeSalePrice > oldWholeSalePrice)
                            )
                        {
                            if (item.UnitPrice < oldUnitPrice)
                                item.UnitPrice = oldUnitPrice;

                            if (item.TradePrice < oldTradePrice)
                                item.TradePrice = oldTradePrice;

                            if (item.WholeSalePrice < oldWholeSalePrice)
                                item.WholeSalePrice = oldWholeSalePrice;

                            item.Reason = "System update on CalculatePP";
                            context.Entry(item).State = EntityState.Modified;
                        }
                        else
                        {
                            item.NetPurchasePrice = oldNetPurchasePrice;
                            item.WithTaxNetPurchasePrice = oldWithTaxNetPurchasePrice;
                            item.PurchasePrice = oldPurchasePrice;
                            item.UnitPrice = oldUnitPrice;
                            item.TradePrice = oldTradePrice;
                            item.WholeSalePrice = oldWholeSalePrice;
                        }
                    }
                }
                context.Commit();
            }
            return true;
        }

        public bool InsertUpdateRisk(AuthContext context, List<UpdateRiskDc> items, int UserId)
        {
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemMultiMRPId");
            dt.Columns.Add("WarehouseId");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Price");
            foreach (var item in items)
            {
                var dr = dt.NewRow();
                dr["ItemMultiMRPId"] = item.ItemMultiMrpId;
                dr["WarehouseId"] = item.WarehouseId;
                dr["Qty"] = item.RiskQuantity;
                dr["Price"] = item.RiskPurchasePrice;
                dt.Rows.Add(dr);

                var sTableParam = new SqlParameter("RiskItem", dt);
                sTableParam.SqlDbType = SqlDbType.Structured;
                sTableParam.TypeName = "dbo.RiskItemType";
                var type = new SqlParameter("type", item.RiskType);
                var pUserId = new SqlParameter("UserId", UserId);
                context.Database.CommandTimeout = 300;
                context.Database.ExecuteSqlCommand("InsertUpdateRisk @RiskItem , @type , @UserId", sTableParam, type, pUserId);
            }
            return true;
        }
    }
}