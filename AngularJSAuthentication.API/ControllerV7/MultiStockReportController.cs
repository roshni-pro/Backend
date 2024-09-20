using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/MultiStockReport")]
    public class MultiStockReportController : ApiController
    {
        [HttpGet]
        [Route("GetVirtualStockList")]
        public dynamic GetVirtualStockList()
        {
            using (AuthContext db = new AuthContext())
            {

                string sqlquery = "SELECT O.ItemMultiMRPID,O.WarehouseId,wh.WarehouseName As WarehouseName,item.itemname As ItemName, SUM(InOutQty) QTY from VirtualStocks O JOIN Warehouses wh ON O.WarehouseId = wh.WarehouseId JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId group by O.ItemMultiMRPID, O.WarehouseId,wh.WarehouseName,item.itemname having SUM(InOutQty) <> 0";
                List<VirtualDC> newdata = db.Database.SqlQuery<VirtualDC>(sqlquery).ToList();
                return newdata;
            }
        }


        [HttpPost]
        [Route("GetVirtualStockListByFilter")]
        public dynamic GetVirtualStockListByFilter(StockHistoryFilterDc filter)
        {
            using (AuthContext db = new AuthContext())
            {

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@ItemMultiMRPId", filter.ItemMultiMRPId));
                paramList.Add(new SqlParameter("@WarehouseId", filter.WarehouseId));
                string sqlquery = "Stock_Fetch_GetNegetiveVirtualStock @ItemMultiMRPId, @WarehouseId";
                List<VirtualDC> newdata = db.Database.SqlQuery<VirtualDC>(sqlquery, paramList.ToArray()).ToList();
                var people = newdata.Select(x => x.CreatedBy).Distinct().ToList();
                var peoplename = db.Peoples.Where(x => people.Contains(x.PeopleID)).ToList();
                foreach (var item in newdata)
                {
                    item.PersonName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                }
                return newdata;
            }
        }

        [HttpGet]
        [Route("GetVirtualMultiMrpIdList")]
        public dynamic GetVirtualMultiMrpIdList(int ItemMultiMRPID, int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {

                //string sqlquery = "SELECT O.ItemMultiMRPID,O.TransactionId,O.WarehouseId,item.WarehouseName As WarehouseName,item.itemname As ItemName, SUM(InOutQty) QTY from VirtualStocks O JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId group by O.ItemMultiMRPID, O.TransactionId,O.WarehouseId,item.WarehouseName,item.itemname having SUM(InOutQty) <> 0 and O.ItemMultiMRPId = " + ItemMultiMRPID;
                //string sqlquery = "SELECT O.ItemMultiMRPID,O.TransactionId,O.WarehouseId,item.WarehouseName As WarehouseName,item.itemname As ItemName, O.InOutQty As QTY from VirtualStocks O JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId group by O.ItemMultiMRPID, O.TransactionId,O.WarehouseId,item.WarehouseName,item.itemname,O.InOutQty having O.ItemMultiMRPId = " + ItemMultiMRPID;
                string sqlquery = "SELECT O.Id, O.ItemMultiMRPID,O.TransactionId,O.WarehouseId,O.CreatedBy,item.WarehouseName As WarehouseName,item.itemname As ItemName, O.InOutQty As QTY, O.Reason, O.CreatedDate,O.StockType from VirtualStocks O LEFT JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId where O.WarehouseId =" + WarehouseId + " and O.ItemMultiMRPId = " + ItemMultiMRPID
                                   + "AND O.TransactionId IN(SELECT VS.TransactionId FROM VirtualStocks VS WHERE VS.WarehouseId =" + WarehouseId
                                   + "and VS.ItemMultiMRPId = " + ItemMultiMRPID
                                   + " GROUP BY VS.TransactionId HAVING SUM(VS.InOutQty) <> 0)";
                List<VirtualStockTransactionDC> newdata = db.Database.SqlQuery<VirtualStockTransactionDC>(sqlquery).OrderByDescending(x => x.CreatedDate).ToList();
                var people = newdata.Select(x => x.CreatedBy).Distinct().ToList();
                var peoplename = db.Peoples.Where(x => people.Contains(x.PeopleID)).ToList();
                foreach (var item in newdata)
                {
                    item.PersonName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                }
                return newdata;
            }
        }
        [HttpGet]
        [Route("GetWarehousewiseList")]
        public dynamic GetWarehousewiseList(int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                string sqlquery = "SELECT O.ItemMultiMRPID,O.WarehouseId,item.WarehouseName As WarehouseName,O.CreatedDate,item.itemname As ItemName, SUM(InOutQty) QTY from VirtualStocks O JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId group by O.ItemMultiMRPID, O.WarehouseId,item.WarehouseName,O.CreatedDate,item.itemname having SUM(InOutQty) <> 0 and O.WarehouseId=" + WarehouseId;
                List<VirtualDC> newdata = db.Database.SqlQuery<VirtualDC>(sqlquery).OrderByDescending(x => x.CreatedDate).ToList();
                return newdata;
            }
        }
        [HttpGet]
        [Route("SearchByItemMultiMrpId")]
        public dynamic SearchByItemMultiMrpId(int ItemMultiMRPID, int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                string sqlquery = "SELECT O.ItemMultiMRPID,O.WarehouseId,item.WarehouseName As WarehouseName,O.CreatedDate,item.itemname As ItemName, SUM(InOutQty) QTY from VirtualStocks O JOIN CurrentStocks item ON O.ItemMultiMRPId = item.ItemMultiMRPId and O.WarehouseId = item.WarehouseId group by O.ItemMultiMRPID, O.WarehouseId,item.WarehouseName,item.itemname,O.CreatedDate having SUM(InOutQty) <> 0 and O.ItemMultiMRPId =" + ItemMultiMRPID + "and O.WarehouseId=" + WarehouseId;
                List<VirtualDC> newdata = db.Database.SqlQuery<VirtualDC>(sqlquery).OrderByDescending(x => x.CreatedDate).ToList();
                return newdata;
            }
        }
        [HttpGet]
        [Route("UnsettledVirtualItemList")]
        public async Task<List<UnsettledVirtualItemListDC>> UnsettledVirtualItemList(int warehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                var Warehouseid = new SqlParameter("warehouseId", warehouseId);
                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();
                var cmd2 = db.Database.Connection.CreateCommand();
                cmd2.CommandText = "[dbo].UnsettledVirtualItemList";
                cmd2.Parameters.Add(Warehouseid);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                cmd2.CommandTimeout = 1200;
                // Run the sproc
                var reader2 = cmd2.ExecuteReader();
                List<UnsettledVirtualItemListDC> reportdata = ((IObjectContextAdapter)db)
                .ObjectContext
                .Translate<UnsettledVirtualItemListDC>(reader2).ToList();
                // List<UnsettledVirtualItemListDC> itemList =await db.Database.SqlQuery<UnsettledVirtualItemListDC>("EXEC UnsettledVirtualItemList {0}", warehouseId).ToListAsync();
                return reportdata;
            }
        }
    }

    public class StockHistoryFilterDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
    }
    public class VirtualDC
    {
        public int ItemMultiMRPID { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int QTY { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string PersonName { get; set; }

    }
    public class VirtualStockTransactionDC
    {
        public int ItemMultiMRPID { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int QTY { get; set; }
        public string TransactionId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public string StockType { get; set; }
        public int CreatedBy { get; set; }
        public string PersonName { get; set; }
    }
    public class UnsettledVirtualItemListDC
    {
        public long Id { get; set; }
        public string TransactionId { get; set; }
        public int ItemMultiMRPID { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int QTY { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
    }
}
