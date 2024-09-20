using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/ExcelOrder")]
    public class ExcelOrderController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getAppOrders(DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status) //get search orders for delivery
        {
            using (var context = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var DBoyorders = context.searchorderbycustomer(start, end, OrderId, Skcode, ShopName, Mobile, status, compid);
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage getExports(string type, DateTime? start, DateTime? end, int Warehouseid) //get search orders for delivery
        {
            using (var db = new AuthContext())
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

                    List<OrderDetailsExport> newdata = new List<OrderDetailsExport>();

                    db.Database.Log = s => Debug.WriteLine(s);

                    if (Warehouseid != 0)
                    {
                        newdata = (from od in db.DbOrderDetails
                                   join odm in db.OrderDispatchedMasters on od.OrderId equals odm.OrderId
                                   where od.Deleted == false && od.OrderDate >= start && od.OrderDate <= end && od.WarehouseId == Warehouseid
                                   select new OrderDetailsExport
                                   {
                                       ItemId = od.ItemId,
                                       itemname = od.itemname,
                                       itemNumber = od.itemNumber,
                                       WarehouseId = od.WarehouseId,
                                       HSNCode = od.HSNCode,
                                       CustomerName = od.CustomerName,
                                       ShopName = odm.ShopName,
                                       OrderId = od.OrderId,
                                       UnitPrice = od.UnitPrice,
                                       Mobile = od.Mobile,
                                       qty = od.qty,
                                       TaxAmmount = od.TaxAmmount,
                                       WarehouseName = od.WarehouseName,
                                       Date = od.OrderDate,
                                       Status = odm.Status,
                                       TotalAmt = od.TotalAmt,
                                       Tin_No = odm.Tin_No,
                                       DboyName = odm.DboyName,
                                       DboyMobileNo = odm.DboyMobileNo,
                                       CategoryName = od.CategoryName,
                                       BrandName = od.SubsubcategoryName,
                                       // for all tax 
                                       AmtWithoutTaxDisc = od.AmtWithoutTaxDisc,
                                       SGSTTaxPercentage = od.SGSTTaxPercentage,
                                       SGSTTaxAmmount = od.SGSTTaxAmmount,
                                       CGSTTaxPercentage = od.CGSTTaxPercentage,
                                       CGSTTaxAmmount = od.CGSTTaxAmmount,
                                       IGSTTaxAmount = od.SGSTTaxAmmount + od.CGSTTaxAmmount,
                                       IGSTTaxPercent = od.CGSTTaxPercentage + od.SGSTTaxPercentage,
                                       TotalCessPercentage = od.TotalCessPercentage,
                                       CessTaxAmount = od.CessTaxAmount,
                                       DeliveryIssuanceIdOrderDeliveryMaster = odm.DeliveryIssuanceIdOrderDeliveryMaster,
                                       invoice_no = odm.invoice_no,
                                       orderDispatchedDetailsExport = (from d in db.OrderDispatchedDetailss
                                                                       where d.Deleted == false && d.OrderDetailsId == od.OrderDetailsId
                                                                       select new OrderDispatchedDetailsExport
                                                                       {
                                                                           ItemId = d.ItemId,
                                                                           itemname = d.itemname,
                                                                           itemNumber = d.itemNumber,
                                                                           CustomerName = d.CustomerName,
                                                                           Mobile = d.Mobile,
                                                                           TaxAmmount = d.TaxAmmount,
                                                                           QtyChangeReason = d.QtyChangeReason,
                                                                           OrderId = d.OrderId,
                                                                           dUnitPrice = d.UnitPrice,
                                                                           TaxPercentage = d.TaxPercentage,
                                                                           dqty = d.qty,
                                                                           WarehouseName = d.WarehouseName,
                                                                           Date = d.OrderDate,
                                                                           dTotalAmt = d.TotalAmt,
                                                                       }).ToList()
                                   }).OrderByDescending(x => x.OrderId).ToList();
                    }
                    else
                    {
                        newdata = (from od in db.DbOrderDetails
                                   where od.WarehouseId.Equals(Warehouseid)
                                   join item in db.itemMasters on od.ItemId equals item.ItemId
                                   //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                   //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                   join OM in db.OrderDispatchedMasters on od.OrderId equals OM.OrderId
                                   select new OrderDetailsExport
                                   {
                                       ItemId = od.ItemId,
                                       itemname = item.itemname,
                                       itemNumber = item.Number,
                                       sellingSKU = item.SellingSku,
                                       WarehouseId = item.WarehouseId,
                                       price = od.price,
                                       UnitPrice = od.UnitPrice,
                                       MinOrderQtyPrice = od.UnitPrice * od.MinOrderQty,
                                       qty = od.qty,
                                       DiscountPercentage = od.DiscountPercentage,
                                       DiscountAmmount = od.DiscountAmmount,
                                       TaxPercentage = od.TaxPercentage,
                                       TaxAmmount = od.TaxAmmount,
                                       TotalAmt = od.TotalAmt,
                                       Tin_No = OM.Tin_No,
                                       CategoryName = item.CategoryName,
                                       BrandName = item.SubsubcategoryName,
                                       // for all tax 
                                       AmtWithoutTaxDisc = od.AmtWithoutTaxDisc,
                                       SGSTTaxPercentage = od.SGSTTaxPercentage,
                                       SGSTTaxAmmount = od.SGSTTaxAmmount,
                                       CGSTTaxPercentage = od.CGSTTaxPercentage,
                                       CGSTTaxAmmount = od.CGSTTaxAmmount,
                                       IGSTTaxAmount = od.SGSTTaxAmmount + od.CGSTTaxAmmount,
                                       IGSTTaxPercent = od.CGSTTaxPercentage + od.SGSTTaxPercentage,
                                       TotalCessPercentage = od.TotalCessPercentage,
                                       CessTaxAmount = od.CessTaxAmount,
                                       DeliveryIssuanceIdOrderDeliveryMaster = OM.DeliveryIssuanceIdOrderDeliveryMaster,
                                       invoice_no = OM.invoice_no,
                                       ShopName = OM.ShopName,
                                       orderDispatchedDetailsExport = (from d in db.OrderDispatchedDetailss
                                                                       join items in db.itemMasters on od.ItemId equals item.ItemId
                                                                       //join cat in db.Categorys on item.Categoryid equals cat.Categoryid
                                                                       //join sbcat in db.SubsubCategorys on item.SubsubCategoryid equals sbcat.SubsubCategoryid
                                                                       select new OrderDispatchedDetailsExport
                                                                       {
                                                                           ItemId = d.ItemId,
                                                                           itemname = items.itemname,
                                                                           itemNumber = items.Number,
                                                                           sellingSKU = items.SellingSku,
                                                                           price = d.price,
                                                                           dUnitPrice = d.UnitPrice,
                                                                           MinOrderQtyPrice = d.UnitPrice * d.MinOrderQty,
                                                                           dqty = d.qty,
                                                                           DiscountPercentage = d.DiscountPercentage,
                                                                           DiscountAmmount = d.DiscountAmmount,
                                                                           TaxPercentage = d.TaxPercentage,
                                                                           TaxAmmount = d.TaxAmmount,
                                                                           dTotalAmt = d.TotalAmt,
                                                                           CategoryName = items.CategoryName,
                                                                           BrandName = items.SubsubcategoryName
                                                                       }).ToList()     /*a.orderDetails,*/
                                   }).OrderByDescending(x => x.OrderId).ToList();
                    }

                    if (newdata != null && newdata.Count > 0)
                    {

                        List<WarehouseMin> Warehouses = new List<WarehouseMin>();

                        string warehouseids = string.Join(",", newdata.Select(x => x.WarehouseId).Distinct().ToList());

                        string sql = "Select WarehouseId,GSTin from Warehouses where warehouseid in (" + warehouseids + ")";
                        Warehouses = db.Database.SqlQuery<WarehouseMin>(sql).ToList();
                        foreach (var orders in newdata)
                        {
                            if (!string.IsNullOrEmpty(orders.Tin_No) && orders.Tin_No.Length >= 11)
                            {
                                string CustTin_No = orders.Tin_No.Substring(0, 2);

                                if (!CustTin_No.StartsWith("0"))
                                {
                                    if (!Warehouses.Any(x => x.GSTin != null && x.WarehouseId == orders.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No))
                                    {
                                        orders.IGSTTaxAmount = orders.TaxAmmount;
                                        orders.IGSTTaxPercent = orders.TaxPercentage;
                                        orders.SGSTTaxPercentage = 0;
                                        orders.SGSTTaxAmmount = 0;
                                        orders.CGSTTaxPercentage = 0;
                                        orders.CGSTTaxAmmount = 0;
                                        orders.CGSTTaxAmmount = 0;
                                        orders.CGSTTaxPercentage = 0;
                                    }
                                }
                            }
                        }
                    }

                    if (newdata.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, newdata);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, newdata);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("GenrateExcelOrderReport")]
        [HttpGet]
        public HttpResponseMessage GenrateExcelOrderReport()
        {
            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 6000;
                try
                {
                    DateTime date = DateTime.Now;
                    DateTime datestart = date.AddMonths(-2);

                    string sqlquery = "select od.OrderId,odm.invoice_no,od.WarehouseId,od.WarehouseName,odm.PocCreditNoteNumber,odm.PocCreditNoteDate,odm.ClusterName,od.CustomerName,odm.Skcode,odm.ShopName,odm.DboyName,odm.DboyMobileNo,od.price,od.SellingSku,od.MinOrderQtyPrice,odm.DeliveryIssuanceIdOrderDeliveryMaster," +
                        "od.DiscountPercentage,od.DiscountAmmount,od.TotalCessPercentage,od.CessTaxAmount,od.ItemId,od.itemNumber,od.itemname, od.HSNCode , od.ItemMultiMRPId, " +
                                      "od.UnitPrice,od.Mobile,od.qty,od.TaxAmmount,od.OrderDate,odm.Status,od.TotalAmt,odm.Tin_No," +
                                      "od.CategoryName,od.SubsubcategoryName,od.AmtWithoutTaxDisc,od.SGSTTaxPercentage," +
                                      "od.SGSTTaxAmmount,od.CGSTTaxPercentage,od.CGSTTaxAmmount,od.SGSTTaxAmmount + od.CGSTTaxAmmount as IGSTTaxAmount," +
                                      "od.CGSTTaxPercentage + od.SGSTTaxPercentage as IGSTTaxPercent,od.TotalCessPercentage,od.CessTaxAmount," +
                                      "odd.QtyChangeReason,odd.UnitPrice as DispatchedUnitPrice, odd.qty as DispatchedQty,odd.TotalAmt as DispatchedTotalAmt, odd.TaxPercentage, odd.CreatedDate as DispatchedDate ,Isnull(st.Name,'Other') StoreName,o.Deliverydate " +
                                      " from OrderDetails od  with(nolock) Inner join OrderDispatchedMasters odm  with(nolock) on od.OrderId = odm.OrderId inner join OrderMasters o with(nolock) on od.OrderId=o.OrderId  " +
                                      "join OrderDispatchedDetails odd  with(nolock) on od.OrderDetailsId = odd.OrderDetailsId  Left Join Stores st on od.storeid=st.Id where od.CreatedDate >= '" + datestart + "' and od.CreatedDate <= '" + date + "'";

                    var newdata = db.Database.SqlQuery<OrderDetailsExportDTO>(sqlquery).ToList();

                    if (newdata != null && newdata.Count > 0)
                    {
                        List<WarehouseMin> Warehouses = new List<WarehouseMin>();

                        string warehouseids = string.Join(",", newdata.Select(x => x.WarehouseId).Distinct().ToList());

                        string sql = "Select WarehouseId,GSTin from Warehouses where warehouseid in (" + warehouseids + ")";
                        Warehouses = db.Database.SqlQuery<WarehouseMin>(sql).ToList();
                        foreach (var orders in newdata)
                        {
                            if (!string.IsNullOrEmpty(orders.Tin_No) && orders.Tin_No.Length >= 11)
                            {
                                string CustTin_No = orders.Tin_No.Substring(0, 2);

                                if (!CustTin_No.StartsWith("0"))
                                {
                                    if (!Warehouses.Any(x => x.GSTin != null && x.WarehouseId == orders.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No))
                                    {
                                        orders.IGSTTaxAmount = orders.TaxAmmount;
                                        orders.IGSTTaxPercent = orders.TaxPercentage;
                                        orders.SGSTTaxPercentage = 0;
                                        orders.SGSTTaxAmmount = 0;
                                        orders.CGSTTaxPercentage = 0;
                                        orders.CGSTTaxAmmount = 0;
                                        orders.CGSTTaxAmmount = 0;
                                        orders.CGSTTaxPercentage = 0;
                                    }
                                    else
                                    {
                                        orders.IGSTTaxAmount = 0;
                                        orders.IGSTTaxPercent = 0;
                                    }
                                }
                            }
                            else
                            {
                                orders.IGSTTaxAmount = 0;
                                orders.IGSTTaxPercent = 0;
                            }
                        }
                    }

                    DataTable dt = ListtoDataTableConverter.ToDataTable(newdata);
                    string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "ExcelOrderSixtydays.csv");
                    dt.WriteToCsvFile(path);
                    //List<DataTable> Tables = DatatableConvertIntoChunks(dt);

                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(dt);

                    //ExcelGenerator.DataTable_To_Excel(Tables, @"" + path);
                    // ExportServices.DataSet_To_Excel(ds, @"C:\SVN\trunk\AngularJSAuthentication.API\Reports\ExcelOrderSixtydays.xls");

                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }

        public List<DataTable> DatatableConvertIntoChunks(DataTable dt)
        {
            List<DataTable> ndt = new List<DataTable>();
            Int64 ExcelRowsLength = 65535;
            Int64 i = 0;
            int j = 1;
            DataTable newDt = dt.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();
            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == ExcelRowsLength)
                {
                    ndt.Add(newDt);
                    j++;
                    newDt = dt.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            if (newDt.Rows.Count > 0)
            {
                ndt.Add(newDt);
                j++;
                newDt = dt.Clone();
                newDt.TableName = "Table_" + j;
                newDt.Clear();

            }
            return ndt;
        }
    }

    public class WarehouseMin
    {
        public int WarehouseId { get; set; }
        public string GSTin { get; set; }
    }


    public class OrderDetailsExportDTO
    {
        public int OrderId { get; set; }
        public string invoice_no { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public string ClusterName { get; set; }
        //public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        //public string Mobile { get; set; }
        public string Status { get; set; }
        public string DboyName { get; set; }
        public string DboyMobileNo
        {
            get; set;
        }
        public string CategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string BrandName { get; set; }
        public int ItemId { get; set; }
        public string itemNumber { get; set; }
        public string itemname { get; set; }
        public string sellingSKU { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public double MinOrderQtyPrice { get; set; }
        public int qty { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmmount { get; set; }
        public double TaxPercentage { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        //for cess
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public double IGSTTaxAmount { get; set; }
        public double IGSTTaxPercent { get; set; }
        public double TotalAmt { get; set; }
        public string HSNCode { get; set; } // add for HSN code in Excel
        public string Tin_No { get; set; }// add for Gst No. in Excel
        // for export ftr
        public double DispatchedUnitPrice;
        public int DispatchedQty;
        public int? ItemMultiMRPId { get; set; }     
        public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public DateTime DispatchedDate { get; set; }
        public string QtyChangeReason { get; set; }

        public string StoreName { get; set; }
        public DateTime Deliverydate { get; set; }

    }
}
