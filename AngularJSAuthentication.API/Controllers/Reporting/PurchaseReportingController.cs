using AngularJSAuthentication.Common.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/PurchaseReporting")]
    public class PurchaseReportingController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
        #region Generate Purchase pending And Fill Rate Cut

        [Route("SendPurchasePendingData")]
        [HttpGet]
        public bool SendPurchasePendingData()
        {
            bool result = false;
            try
            {
                DataTable dataTable = new DataTable();
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                using (var connection = new SqlConnection(connectionString))
                {

                    string sqlquery = "select c.WarehouseId,c.WarehouseName+ ' ' +c.CityName [Warehouse],a.itemname,sum(a.qty) [Demand Qty], inv.CurrentInventory Stock, sum(a.qty)-inv.CurrentInventory[Due stock]"
                                    + " FROM OrderDetails a inner join Warehouses c on a.WarehouseId = c.WarehouseId and a.Status = 'Pending' and Month(a.CreatedDate) = Month(getdate()) "
                                    + " cross Apply (Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and a.WarehouseId= b.WarehouseId) inv "
                                    + " group by c.WarehouseId,c.WarehouseName , c.CityName,a.itemname,inv.CurrentInventory Order by sum(a.qty)-inv.CurrentInventory asc";

                    using (var command = new SqlCommand(sqlquery.ToString(), connection))
                    {


                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dataTable);
                        da.Dispose();
                        connection.Close();
                    }
                }

                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                if (dataTable.Rows.Count > 0)
                {
                    var data = dataTable.AsEnumerable()
                                          .GroupBy(r => new { Col1 = r["WarehouseId"] })
                                          .Select(g => g);
                    //.Select(g => g.OrderBy(r => r["itemname"]));
                    foreach (var item in data)
                    {
                        DataTable dt = item.CopyToDataTable();
                        string warehouse = dt.Rows[0]["Warehouse"].ToString();
                        string filePath = ExcelSavePath + "PurchasePending_" + warehouse + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        int warehouseid = Convert.ToInt32(dt.Rows[0]["WarehouseId"]);
                        dt.Columns.RemoveAt(0);
                        if (ExcelGenerator.DataTable_To_Excel(dt, warehouse, filePath))
                        {
                            string To = "", From = "", Bcc = "";
                            DataTable emaildatatable = new DataTable();
                            using (var connection = new SqlConnection(connectionString))
                            {
                                using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='PurchaseReporting' and WarehouseId=" + warehouseid, connection))
                                {

                                    if (connection.State != ConnectionState.Open)
                                        connection.Open();

                                    SqlDataAdapter da = new SqlDataAdapter(command);
                                    da.Fill(emaildatatable);
                                    da.Dispose();
                                    connection.Close();
                                }
                            }
                            if (emaildatatable.Rows.Count > 0)
                            {
                                To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                                From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                                Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                            }
                            string subject = DateTime.Now.ToString("dd MMM yyyy") + " purchase pending Report for warehouse " + warehouse;
                            string message = "Please find attach warehouse purchase pending Report";
                            if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                result = EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                            else
                                logger.Error("Purchase Pending Report To and From empty");

                        }
                        result = true;
                    }

                }
                else
                    result = true;

            }
            catch (Exception ex)
            {
                logger.Error("Error in SendPurchasePendingData Method: " + ex.Message);
            }

            return result;
        }


        [Route("FillRateCutReportData")]
        [HttpGet]
        public bool FillRateCutReportData()
        {
            bool result = false;
            try
            {
                DataTable dataTable = new DataTable();
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                using (var connection = new SqlConnection(connectionString))
                {

                    string sqlquery = " select d.WarehouseId,d.WarehouseName+ ' ' + d.CityName [Warehouse],	a.itemname [Item Name],i.SupplierName,	sum(a.qty) OrderQty,  isnull(sum(b.qty),0) supplyQty,	"
                                    + " sum(a.qty) - (isnull( sum(b.qty),0) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) [quantity Not Dispatch],  "
                                    + " sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end) AS [Cancel Order Qty], sum(isnull(c.DamageStockQty,0)) [Damage Stock Quantity], sum(isnull(c.notinStockQty,0)) [Not In Stock Quantity],  "
                                    + " replace(replace(STRING_AGG(isnull(c.DamageComment,'#'),', '),'#, ',''),'#','') as [Damage Comment], replace(replace(STRING_AGG(isnull(c.NotInStockComment,'#'),', '),'#, ',''),'#','') as [Not In Stock Comment],  "
                                    + " count(distinct (case when a.[Status] ='Order Canceled' then a.OrderId else null end))+ count(distinct c.orderid)	+count(distinct (case when 	a.qty-b.qty>0 then a.orderid else null end ))	[Bills affected]  "
                                    + " from OrderDetails a inner join Warehouses d on a.WarehouseId = d.WarehouseId  inner join ItemMasters i on a.ItemId=i.ItemId left join OrderDispatchedDetails b on a.OrderDetailsId=b.OrderDetailsId  "
                                    + " left join [dbo].[ShortItemAssignments] c  on c.OrderId=b.OrderId  and  c.ItemId=a.ItemId where "
                                    + " a.itemname is not null  and a.[Status] !='Pending'  and month(a.CreatedDate) = month(getdate()) and year(a.CreatedDate) = year(getdate())   group by d.WarehouseId,d.WarehouseName+ ' ' + d.CityName,a.itemname,i.SupplierName  "
                                    + " order by d.WarehouseId asc,sum(a.qty) -( sum(b.qty) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) desc,sum(isnull(c.DamageStockQty,0)) desc, sum(isnull(c.notinStockQty,0)) desc  ";



                    using (var command = new SqlCommand(sqlquery.ToString(), connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dataTable);
                        da.Dispose();
                        connection.Close();
                    }
                }

                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                if (dataTable.Rows.Count > 0)
                {
                    var data = dataTable.AsEnumerable()
                                          .GroupBy(r => new { Col1 = r["WarehouseId"] })
                                          .Select(g => g);
                    foreach (var item in data)
                    {
                        DataTable dt = item.CopyToDataTable();
                        string warehouse = dt.Rows[0]["Warehouse"].ToString();
                        string filePath = ExcelSavePath + "FillRateCutReport_" + warehouse + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        int warehouseid = Convert.ToInt32(dt.Rows[0]["WarehouseId"]);
                        dt.Columns.RemoveAt(0);
                        if (ExcelGenerator.DataTable_To_Excel(dt, warehouse, filePath))
                        {
                            string To = "", From = "", Bcc = "";
                            DataTable emaildatatable = new DataTable();
                            using (var connection = new SqlConnection(connectionString))
                            {
                                using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='FillRateCutReport' and WarehouseId=" + warehouseid, connection))
                                {

                                    if (connection.State != ConnectionState.Open)
                                        connection.Open();

                                    SqlDataAdapter da = new SqlDataAdapter(command);
                                    da.Fill(emaildatatable);
                                    da.Dispose();
                                    connection.Close();
                                }
                            }
                            if (emaildatatable.Rows.Count > 0)
                            {
                                To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                                From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                                Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                            }
                            string subject = DateTime.Now.ToString("dd MMM yyyy") + " fill rate cut report for warehouse " + warehouse;
                            string message = "Please find attach warehouse fill rate cut Report";
                            if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                result = EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                            else
                                logger.Error("fill rate cut Report To and From empty");
                        }
                        result = true;
                    }
                }
                else
                    result = true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in FillRateCutReportData Method: " + ex.Message);
            }

            return result;
        }
        #endregion

        #region Dashboard 

        [Route("GetPurchasePendingReportData")]
        [HttpGet]
        public ListPurchasePendingReportData GetPurchasePendingReportData(int totalitem, int page, int? warehouseid, DateTime startDate, DateTime endDate)
        {
            ListPurchasePendingReportData listPurchasePendingReportData = new ListPurchasePendingReportData();
            List<PurchasePendingReportData> PurchasePendingReportDatas = new List<PurchasePendingReportData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    int skip = (page - 1) * totalitem;
                    int take = totalitem;

                    //string sqlquery = "select c.WarehouseId,c.WarehouseName+ ' ' +c.CityName [WarehouseName],a.itemname,sum(a.qty) [DemandQty], inv.CurrentInventory Stock, inv.CurrentInventory-sum(a.qty) [Duestock],otherWarehouseInv.CurrentInventory OtherHubStock ,COUNT(*) OVER() AS total"
                    //               + " FROM OrderDetails a inner join Warehouses c on a.WarehouseId = c.WarehouseId and a.Status = 'Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'" + (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                    //               + " cross Apply (Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and a.WarehouseId= b.WarehouseId) inv "
                    //               + " cross apply (Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and  b.WarehouseId in (select WarehouseId from Warehouses w where w.WarehouseId<>a.WarehouseId and w.Cityid=a.CityId)) otherWarehouseInv "
                    //               + " group by c.WarehouseId,c.WarehouseName , c.CityName,a.itemname,inv.CurrentInventory,otherWarehouseInv.CurrentInventory Order by inv.CurrentInventory-sum(a.qty)  asc offset " + skip + " rows fetch next " + take + " rows only";

                    string sqlquery = "with Purchasepending as ( select c.WarehouseId,c.Cityid,c.WarehouseName + ' ' + c.CityName[WarehouseName],a.itemname,a.itemNumber,a.ItemMultiMRPId,sum(a.qty)[DemandQty], inv.CurrentInventory Stock, inv.CurrentInventory - sum(a.qty)[Duestock] "
                                    + " ,COUNT(*) OVER() AS total  FROM OrderDetails a inner join Warehouses c on a.WarehouseId = c.WarehouseId and a.Status = 'Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'" + (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                                    + " cross Apply(Select b.CurrentInventory from CurrentStocks b where  a.ItemMultiMRPId= b.ItemMultiMRPId and a.WarehouseId= b.WarehouseId) inv group by c.WarehouseId,c.WarehouseName,c.Cityid , c.CityName,a.itemname,a.itemNumber,a.ItemMultiMRPId,inv.CurrentInventory "
                                    + " Order by inv.CurrentInventory - sum(a.qty)  asc offset " + skip + " rows fetch next " + take + " rows only )"
                                    + " Select  c.WarehouseId,c.[WarehouseName],c.itemname,c.[DemandQty], c.Stock, c.[Duestock]  ,ISNULL(otherWarehouseInv.CurrentInventory, 0) OtherHubStock,c.total  From Purchasepending c "
                                    + " Outer apply(select b.CurrentInventory-sum(d.qty) CurrentInventory from   OrderDetails d inner join CurrentStocks b  on  d.ItemMultiMRPId = b.ItemMultiMRPId  and d.WarehouseId = b.WarehouseId "
                                    + " and d.itemNumber = c.ItemNumber and d.ItemMultiMRPId = c.ItemMultiMRPId and d.Status = 'Pending'  and CAST(d.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'"
                                    + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> c.WarehouseId and w.Cityid= c.CityId and w.WarehouseId= d.WarehouseId)  group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory "
                                    + " ) otherWarehouseInv  order by c.Duestock ";


                    PurchasePendingReportDatas = context.Database.SqlQuery<PurchasePendingReportData>(sqlquery).ToList();
                    listPurchasePendingReportData.TotalCount = 0;
                    if (PurchasePendingReportDatas != null && PurchasePendingReportDatas.Any())
                        listPurchasePendingReportData.TotalCount = PurchasePendingReportDatas.FirstOrDefault().total;
                    listPurchasePendingReportData.PurchasePendingReportDatas = PurchasePendingReportDatas;
                }
                return listPurchasePendingReportData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetPurchasePendingReportData Method: " + ex.Message);
                return null;
            }
        }


        [Route("GetPurchasePendingReportDataExport")]
        [HttpGet]
        public ListPurchasePendingReportData GetPurchasePendingReportDataExport(int? warehouseid, DateTime startDate, DateTime endDate)
        {
            ListPurchasePendingReportData listPurchasePendingReportData = new ListPurchasePendingReportData();
            List<PurchasePendingReportData> PurchasePendingReportDatas = new List<PurchasePendingReportData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    //string sqlquery = "select c.WarehouseId,c.WarehouseName+ ' ' +c.CityName [WarehouseName],a.itemname,sum(a.qty) [DemandQty], inv.CurrentInventory Stock, inv.CurrentInventory-sum(a.qty) [Duestock],otherWarehouseInv.CurrentInventory OtherHubStock ,COUNT(*) OVER() AS total"
                    //               + " FROM OrderDetails a inner join Warehouses c on a.WarehouseId = c.WarehouseId and a.Status = 'Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date +"'" +  (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                    //               + " cross Apply (Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and a.WarehouseId= b.WarehouseId) inv "
                    //               + " cross apply (Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and  b.WarehouseId in (select WarehouseId from Warehouses w where w.WarehouseId<>a.WarehouseId and w.Cityid=a.CityId)) otherWarehouseInv "
                    //               + " group by c.WarehouseId,c.WarehouseName , c.CityName,a.itemname,inv.CurrentInventory,otherWarehouseInv.CurrentInventory Order by inv.CurrentInventory-sum(a.qty)  asc offset " + skip + " rows fetch next " + take + " rows only";

                    string sqlquery = "with Purchasepending as ( select c.WarehouseId,c.Cityid,c.WarehouseName + ' ' + c.CityName[WarehouseName],a.itemname,a.itemNumber,a.ItemMultiMRPId,sum(a.qty)[DemandQty], inv.CurrentInventory Stock, inv.CurrentInventory - sum(a.qty)[Duestock] "
                                    + " ,0 AS total  FROM OrderDetails a inner join Warehouses c on a.WarehouseId = c.WarehouseId and a.Status = 'Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'" + (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                                    + " cross Apply(Select b.CurrentInventory from CurrentStocks b where a.itemNumber= b.ItemNumber and a.ItemMultiMRPId= b.ItemMultiMRPId and a.WarehouseId= b.WarehouseId) inv group by c.WarehouseId,c.WarehouseName,c.Cityid , c.CityName,a.itemname,a.itemNumber,a.ItemMultiMRPId,inv.CurrentInventory "
                                    + "  )  "
                                    + " Select  c.WarehouseId,c.[WarehouseName],c.itemname,c.[DemandQty], c.Stock, c.[Duestock]  ,ISNULL(otherWarehouseInv.CurrentInventory, 0) OtherHubStock,c.total  From Purchasepending c "
                                    + " Outer apply(select b.CurrentInventory-sum(d.qty) CurrentInventory from   OrderDetails d inner join CurrentStocks b  on d.itemNumber = b.ItemNumber and d.ItemMultiMRPId = b.ItemMultiMRPId  and d.WarehouseId = b.WarehouseId "
                                    + " and d.itemNumber = c.ItemNumber and d.ItemMultiMRPId = c.ItemMultiMRPId and d.Status = 'Pending'  and CAST(d.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'"
                                    + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> c.WarehouseId and w.Cityid= c.CityId and w.WarehouseId= d.WarehouseId)  group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory "
                                    + " ) otherWarehouseInv  order by c.Duestock ";


                    PurchasePendingReportDatas = context.Database.SqlQuery<PurchasePendingReportData>(sqlquery).ToList();
                    listPurchasePendingReportData.TotalCount = 0;
                    if (PurchasePendingReportDatas != null && PurchasePendingReportDatas.Any())
                        listPurchasePendingReportData.TotalCount = PurchasePendingReportDatas.FirstOrDefault().total;
                    listPurchasePendingReportData.PurchasePendingReportDatas = PurchasePendingReportDatas;
                }
                return listPurchasePendingReportData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetPurchasePendingReportData Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetFillRateCutReportData")]
        [HttpGet]
        public ListFillRateCutReportData GetFillRateCutReportData(int totalitem, int page, int? warehouseid, DateTime startDate, DateTime endDate)
        {
            ListFillRateCutReportData listFillRateCutReportData = new ListFillRateCutReportData();
            List<FillRateCutReportData> FillRateCutReportDatas = new List<FillRateCutReportData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    int skip = (page - 1) * totalitem;
                    int take = totalitem;

                    string sqlquery = " select d.WarehouseId,d.WarehouseName+ ' ' + d.CityName [Warehouse],	a.itemname [ItemName],i.SupplierName,	sum(a.qty) OrderQty,  isnull(sum(b.qty),0) supplyQty,	"
                                  + " sum(a.qty) - (isnull( sum(b.qty),0) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) [quantityNotDispatch],  "
                                  + " sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end) AS [CancelOrderQty], sum(isnull(c.DamageStockQty,0)) [DamageStockQuantity], sum(isnull(c.notinStockQty,0)) [NotInStockQuantity],  "
                                  + " replace(replace(STRING_AGG(isnull(c.DamageComment,'#'),', '),'#, ',''),'#','') as [DamageComment], replace(replace(STRING_AGG(isnull(c.NotInStockComment,'#'),', '),'#, ',''),'#','') as [NotInStockComment],  "
                                  + " count(distinct (case when a.[Status] ='Order Canceled' then a.OrderId else null end))+ count(distinct c.orderid)	+count(distinct (case when 	a.qty-b.qty>0 then a.orderid else null end ))	[Billsaffected]  "
                                  + " ,COUNT(*) OVER() AS total from OrderDetails a inner join Warehouses d on a.WarehouseId = d.WarehouseId  inner join ItemMasters i on a.ItemId=i.ItemId left join OrderDispatchedDetails b on a.OrderDetailsId=b.OrderDetailsId  "
                                  + " left join [dbo].[ShortItemAssignments] c  on c.OrderId=b.OrderId  and  c.ItemId=a.ItemId where "
                                  + " a.itemname is not null  and a.[Status] !='Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'" + (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                                  + " group by d.WarehouseId,d.WarehouseName+ ' ' + d.CityName,a.itemname,i.SupplierName  "
                                  + " order by d.WarehouseId asc,sum(a.qty) -( sum(b.qty) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) desc,sum(isnull(c.DamageStockQty,0)) desc, sum(isnull(c.notinStockQty,0)) desc  offset " + skip + " rows fetch next " + take + " rows only";

                    FillRateCutReportDatas = context.Database.SqlQuery<FillRateCutReportData>(sqlquery).ToList();
                    listFillRateCutReportData.TotalCount = 0;
                    if (FillRateCutReportDatas != null && FillRateCutReportDatas.Any())
                        listFillRateCutReportData.TotalCount = FillRateCutReportDatas.FirstOrDefault().total;
                    listFillRateCutReportData.FillRateCutReportDatas = FillRateCutReportDatas;
                }
                return listFillRateCutReportData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetFillRateCutReportData Method: " + ex.Message);
                return null;
            }
        }
        #endregion


        [Route("ExportGetFillRateCutReportData")]
        [HttpGet]
        public HttpResponseMessage ExportGetFillRateCutReportData(int? warehouseid, DateTime startDate, DateTime endDate)
        {
     
            List<FillRateCutReportData> FillRateCutReportDatas = new List<FillRateCutReportData>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                  string sqlquery = " select d.WarehouseId,d.WarehouseName+ ' ' + d.CityName [Warehouse],	a.itemname [ItemName],i.SupplierName,	sum(a.qty) OrderQty,  isnull(sum(b.qty),0) supplyQty,	"
                                  + " sum(a.qty) - (isnull( sum(b.qty),0) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) [quantityNotDispatch],  "
                                  + " sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end) AS [CancelOrderQty], sum(isnull(c.DamageStockQty,0)) [DamageStockQuantity], sum(isnull(c.notinStockQty,0)) [NotInStockQuantity],  "
                                  + " replace(replace(STRING_AGG(isnull(c.DamageComment,'#'),', '),'#, ',''),'#','') as [DamageComment], replace(replace(STRING_AGG(isnull(c.NotInStockComment,'#'),', '),'#, ',''),'#','') as [NotInStockComment],  "
                                  + " count(distinct (case when a.[Status] ='Order Canceled' then a.OrderId else null end))+ count(distinct c.orderid)	+count(distinct (case when 	a.qty-b.qty>0 then a.orderid else null end ))	[Billsaffected]  "
                                  + " ,COUNT(*) OVER() AS total from OrderDetails a inner join Warehouses d on a.WarehouseId = d.WarehouseId  inner join ItemMasters i on a.ItemId=i.ItemId left join OrderDispatchedDetails b on a.OrderDetailsId=b.OrderDetailsId  "
                                  + " left join [dbo].[ShortItemAssignments] c  on c.OrderId=b.OrderId  and  c.ItemId=a.ItemId where "
                                  + " a.itemname is not null  and a.[Status] !='Pending' and CAST(a.CreatedDate as Date) between '" + startDate.Date + "' and '" + endDate.Date + "'" + (warehouseid.HasValue ? " And a.WarehouseId=" + warehouseid + "" : " ")
                                  + " group by d.WarehouseId,d.WarehouseName+ ' ' + d.CityName,a.itemname,i.SupplierName  "
                                  + " order by d.WarehouseId asc,sum(a.qty) -( sum(b.qty) + sum(isnull(c.DamageStockQty,0))  + sum(isnull(c.notinStockQty,0)) + sum(case when a.[Status] ='Order Canceled' then a.qty else 0 end)) desc,sum(isnull(c.DamageStockQty,0)) desc, sum(isnull(c.notinStockQty,0)) desc ";

                    FillRateCutReportDatas = context.Database.SqlQuery<FillRateCutReportData>(sqlquery).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, FillRateCutReportDatas);
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetFillRateCutReportData Method: " + ex.Message);
                return null;
            }
        }
    }
    public class ListPurchasePendingReportData
    {
        public int TotalCount { get; set; }
        public List<PurchasePendingReportData> PurchasePendingReportDatas { get; set; }
    }


    public class PurchasePendingReportData
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string itemname { get; set; }
        public int DemandQty { get; set; }
        public int Stock { get; set; }
        public int Duestock { get; set; }
        public int OtherHubStock { get; set; }
        public int total { get; set; }

    }

    public class ListFillRateCutReportData
    {
        public int TotalCount { get; set; }
        public List<FillRateCutReportData> FillRateCutReportDatas { get; set; }
    }
    public class FillRateCutReportData
    {
        public int WarehouseId { get; set; }
        public string Warehouse { get; set; }
        public string ItemName { get; set; }
        public string SupplierName { get; set; }
        public int OrderQty { get; set; }
        public int supplyQty { get; set; }
        public int quantityNotDispatch { get; set; }
        public int CancelOrderQty { get; set; }
        public int DamageStockQuantity { get; set; }
        public int NotInStockQuantity { get; set; }
        public string DamageComment { get; set; }
        public string NotInStockComment { get; set; }
        public int Billsaffected { get; set; }
        public int total { get; set; }
    }


}
