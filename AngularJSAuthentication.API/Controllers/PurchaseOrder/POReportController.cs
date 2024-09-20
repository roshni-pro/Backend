using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using NLog;

namespace AngularJSAuthentication.API.Controllers.PurchaseOrder
{
    [RoutePrefix("api/POReport")]
    public class POReportController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

   //     [Authorize]
   //     [Route("")]
   //     [AllowAnonymous]
   //     public IEnumerable<Warehouse> Get()
   //     {
   //         using (AuthContext context = new AuthContext())
   //         {
   //             List<Warehouse> ass = new List<Warehouse>();

   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             int Warehouse_id = 0;

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
   //                 Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (Warehouse_id > 0)
   //             {
   //                 ass = context.AllWarehouseWid(compid, Warehouse_id).OrderBy(a => a.CityName).ToList();
   //                 return ass;
   //             }
   //             else
   //             {
   //                 ass = context.AllWarehouse(compid).OrderBy(a => a.CityName).ToList();
   //                 return ass;
   //             }
   //         }
   //     }


   //     [Route("GetPO")]
   //     [HttpPost]
   //     public PaggingData GetPRlist(objDTIR obj)
   //     {
   //         logger.Info("start ItemMaster: ");

   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             int CompanyId = compid;
   //             logger.Info("User ID : {0} , Company Id : {1}", compid, userid, obj.WHID);
   //             using (var context = new AuthContext())
   //             {
   //                 PaggingData Result = new PaggingData();
   //                 if (obj.WHID > 0  && obj.value == 1)
   //                 {
   //                     List<GoodsReceivedDetailDTo> Polist = new List<GoodsReceivedDetailDTo>();
   //                     if (obj.From != null)
   //                     {
   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId " +
   //                              "where a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('Approved','Self Approved')";
   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     }
   //                     else
   //                     {
   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId " +
   //                              "where a.WarehouseId =" + obj.WHID + "and a.Status in  ('Approved','Self Approved')";
   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     }
   //                     //Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     Result.total_count = Polist.Count();
   //                     Result.ordermaster = Polist.OrderByDescending(x => x.PurchaseOrderId).Skip((obj.page - 1) * obj.list).Take(obj.list).ToList();
   //                     return Result;
   //                 }
   //                 if (obj.WHID > 0 && obj.value == 2)
   //                 {
   //                     List<GoodsReceivedDetailDTo> Polist = new List<GoodsReceivedDetailDTo>();
   //                     if (obj.From != null)
   //                     {
                            
   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                              " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where  a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('UN Partial Received','Partial Received','CN Partial Received')";
   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     }
   //                     else {

   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                              " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where a.WarehouseId =" + obj.WHID + "and a.Status in  ('UN Partial Received','Partial Received','CN Partial Received')";
   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();

   //                     }
   //                     Result.total_count = Polist.Count();
   //                     Result.ordermaster = Polist.OrderByDescending(x => x.PurchaseOrderId).Skip((obj.page - 1) * obj.list).Take(obj.list).ToList();
   //                     return Result;
   //                 }                
   //                     if (obj.WHID > 0 && obj.value == 3)
   //                     {
   //                         List<GoodsReceivedDetailDTo> Polist = new List<GoodsReceivedDetailDTo>();
   //                     if (obj.From != null)
   //                     {
   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                              " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('CN Received')";

   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     }
   //                     else
   //                     {
   //                         string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                            " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                             " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where a.WarehouseId =" + obj.WHID + "and a.Status in  ('CN Received')";

   //                         Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();

   //                     }
   //                         Result.total_count = Polist.Count();
   //                         Result.ordermaster = Polist.OrderByDescending(x => x.PurchaseOrderId).Skip((obj.page - 1) * obj.list).Take(obj.list).ToList();
   //                         return Result;
   //                     }
   //                 if (obj.WHID > 0)
   //                 {
   //                     List<GoodsReceivedDetailDTo> Polist = new List<GoodsReceivedDetailDTo>();
   //                     string query = "select distinct (a.PurchaseOrderId),a.SupplierName,a.WarehouseName,a.CreationDate,a.Status,a.DepoName" +
   //                         " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                          " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where  a.WarehouseId =" + obj.WHID + "and a.Status in  ('Approved','Self Approved','CN Received','UN Partial Received','Partial Received','CN Partial Received')";

   //                     Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     Result.total_count = Polist.Count();
   //                     Result.ordermaster = Polist.OrderByDescending(x => x.PurchaseOrderId).Skip((obj.page - 1) * obj.list).Take(obj.list).ToList();
   //                     return Result;
   //                 }
   //                 return Result;
   //             }

   //     }


   ///// <summary>
   ///// 
   ///// </summary>
   ///// <param name="obj"></param>
   ///// <returns></returns>
   //     [Route("GetPOExport")]
   //     [HttpPost]
   //     public HttpResponseMessage Getexport(ExportobjDTIR obj)
   //     {
   //         logger.Info("start ItemMaster: ");

   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             int CompanyId = compid;
   //             logger.Info("User ID : {0} , Company Id : {1}", compid, userid, obj.WHID);
   //             using (var context = new AuthContext())
   //             {
   //                 List<GoodsReceivedDetailCExport> result = new List<GoodsReceivedDetailCExport>();
   //                 PaggingData Result = new PaggingData();
   //                 if (obj.WHID > 0 && obj.value == 1 && obj.From != null)
   //                 {
   //                     string query = "select  a.PurchaseOrderId,a.SupplierName,a.WarehouseName,p.ItemName,p.Price,p.PurchaseQty,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId " +
   //                              "where a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('Approved','Self Approved')";
   //                     result = context.Database.SqlQuery<GoodsReceivedDetailCExport>(query).ToList();                        
   //                 }
   //                 if (obj.WHID > 0 && obj.value == 2 && obj.From != null)
   //                 {
   //                     List<GoodsReceivedDetailDTo> Polist = new List<GoodsReceivedDetailDTo>();
   //                     string query = "select a.PurchaseOrderId,a.SupplierName,a.WarehouseName,p.ItemName,p.Price,p.PurchaseQty,a.CreationDate,a.Status,a.DepoName" +
   //                           " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                            " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where  a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('UN Partial Received','Partial Received','CN Partial Received')";
   //                     result = context.Database.SqlQuery<GoodsReceivedDetailCExport>(query).ToList();

   //                 }


   //                 if (obj.WHID > 0 && obj.value == 3 && obj.From != null)
   //                 {

   //                     string query = "select a.PurchaseOrderId,a.SupplierName,a.WarehouseName,p.ItemName,p.Price,p.PurchaseQty,a.CreationDate,a.Status,a.DepoName" +
   //                             " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                              " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where a.CreationDate > '" + obj.From + "'and a.CreationDate <= ' " + obj.TO + "'and a.WarehouseId =" + obj.WHID + "and a.Status in  ('CN Received')";

   //                     result = context.Database.SqlQuery<GoodsReceivedDetailCExport>(query).ToList();
                       
   //                 }
   //                 //if (obj.WHID > 0)
   //                 //{
                        
   //                 //    string query = "select  a.PurchaseOrderId,a.SupplierName,a.WarehouseName,a.CreationDate,a.Status" +
   //                 //        " from PurchaseOrderMasters a  join PurchaseOrderDetails p on a.PurchaseOrderId = p.PurchaseOrderId  join GoodsReceivedDetails d on" +
   //                 //         " p.PurchaseOrderDetailId = d.PurchaseOrderDetailId where  a.WarehouseId =" + obj.WHID + "and a.Status in  ('Approved','Self Approved','CN Received','UN Partial Received','Partial Received','CN Partial Received')";

   //                 //    result = context.Database.SqlQuery<GoodsReceivedDetailCExport>(query).ToList();
                     
   //                 //}
   //                 return Request.CreateResponse(HttpStatusCode.OK, result);
   //             }


   //     }



   //     [Route("GetPOTAT")]
   //     [HttpGet]
   //     public PaggingData GetReport(int list,int page,int Warehouseid)
   //     {
   //         logger.Info("start ItemMaster: ");

   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             int CompanyId = compid;
   //             logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
   //             using (var context = new AuthContext())
   //             {
   //                 PaggingData Result = new PaggingData();

   //                     List<POTATReport> PoTAT = new List<POTATReport>();

   //                     string query = "SELECT a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status,Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, b.CreatedDate)/60.0) as decimal (10,2))[PotoGRTAT] ,Cast(Avg(DATEDIFF(MINUTE, b.CreatedDate, d.CreatedDate)/60.0) as decimal (10,2))[GRtoIRTAT],Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, d.CreatedDate)/60.0) as decimal (10,2))[POtoIRTAT] from" +
   //                                    " PurchaseOrderMasters c join PurchaseOrderDetails a on c.PurchaseOrderId = a.PurchaseOrderId  join GoodsReceivedDetails b on a.PurchaseOrderDetailId = b.PurchaseOrderDetailId join InvoiceReceiptDetails d on b.Id = d.GoodsReceivedDetailId where a.WarehouseId="+ Warehouseid + " group by a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status";
   //                     PoTAT = context.Database.SqlQuery<POTATReport>(query).ToList();

   //                     //Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                     Result.total_count = PoTAT.Count();
   //                     Result.ordermaster = PoTAT.OrderByDescending(x => x.PurchaseOrderId).Skip((page - 1) * list).Take(list).ToList();
   //                     return Result;
                    

   //             }


   //     }

   //     [Route("SearchPOTAT")]
   //     [HttpGet]
   //     public HttpResponseMessage SearchPOTAT(int PurchaseOrderId)
   //     {
   //         logger.Info("start ItemMaster: ");
   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             int CompanyId = compid;
   //             logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
   //             using (var context = new AuthContext())
   //             {
   //                 PaggingData Result = new PaggingData();

   //                 List<POTATReport> PoTAT = new List<POTATReport>();

   //                 string query = "SELECT a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status,Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, b.CreatedDate)/60.0) as decimal (10,2))[PotoGRTAT] ,Cast(Avg(DATEDIFF(MINUTE, b.CreatedDate, d.CreatedDate)/60.0) as decimal (10,2))[GRtoIRTAT],Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, d.CreatedDate)/60.0) as decimal (10,2))[POtoIRTAT] from" +
   //                                " PurchaseOrderMasters c join PurchaseOrderDetails a on c.PurchaseOrderId = a.PurchaseOrderId  join GoodsReceivedDetails b on a.PurchaseOrderDetailId = b.PurchaseOrderDetailId join InvoiceReceiptDetails d on b.Id = d.GoodsReceivedDetailId where a.PurchaseOrderId=" + PurchaseOrderId + " group by a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status";
   //                 PoTAT = context.Database.SqlQuery<POTATReport>(query).ToList();

   //                 //Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();

   //                 return Request.CreateResponse(HttpStatusCode.OK, PoTAT);


   //             }


   //     }

   //     [Route("ExportPOTAT")]
   //     [HttpGet]
   //     public HttpResponseMessage ExportPOTAT(int Warehouseid,DateTime From,DateTime To)
   //     {
   //         logger.Info("start ItemMaster: ");

   //             var identity = User.Identity as ClaimsIdentity;
   //             int compid = 0, userid = 0;
   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
   //                 compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

   //             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
   //                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

   //             int CompanyId = compid;
   //             logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
   //             using (var context = new AuthContext())
   //             {
   //                 PaggingData Result = new PaggingData();

   //                 List<POTATReport> PoTAT = new List<POTATReport>();
   //                 string query = "SELECT a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status,c.CreationDate,Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, b.CreatedDate)/60.0) as decimal (10,2))[PotoGRTAT] ,Cast(Avg(DATEDIFF(MINUTE, b.CreatedDate, d.CreatedDate)/60.0) as decimal (10,2))[GRtoIRTAT],Cast(Avg(DATEDIFF(MINUTE, a.CreationDate, d.CreatedDate)/60.0) as decimal (10,2))[POtoIRTAT] from" +
   //                                " PurchaseOrderMasters c join PurchaseOrderDetails a on c.PurchaseOrderId = a.PurchaseOrderId  join GoodsReceivedDetails b on a.PurchaseOrderDetailId = b.PurchaseOrderDetailId join InvoiceReceiptDetails d on b.Id = d.GoodsReceivedDetailId where c.CreationDate > '" + From + "'and c.CreationDate <= ' " + To + "'and c.WarehouseId=" + Warehouseid + " group by a.PurchaseOrderId,a.WarehouseName,a.SupplierName,c.Status,c.CreationDate";
   //                 PoTAT = context.Database.SqlQuery<POTATReport>(query).ToList();
   //                 //Polist = context.Database.SqlQuery<GoodsReceivedDetailDTo>(query).ToList();
   //                 return Request.CreateResponse(HttpStatusCode.OK, PoTAT);
   //             }          
   //     }
    


    }
    //public class GoodsReceivedDetailDTo
    //{
    //    public int PurchaseOrderId { get; set; }
    //    public string SupplierName { get; set; }
    //    public string Status { get; set; }
    //    public string GRNo { get; set; }
    //    public int TotalQuantity { get; set; }
    //    public int Qty { get; set; }
    //    public string WarehouseName { get; set; }
    //    public double PriceRecived { get; set; }
    //    public double Price { get; set; }
    //    public string ItemName { get; set; }
    //    public double MRP { get; set; }
    //    public DateTime GrDate { get; set; }
    //    public DateTime CreationDate { get; set; }
    //    public string ItemNumber { get; set; }
    //    public int ItemMultiMRPId { get; set; }
    //    public string DepoName { get; set; }
    //    public string AverageTAT { get; set; }

    //    public double? POItemFillRate { get; set; }
    //    public string POStatus { get; set; }
    //    public int PurchaseOrderDetailId { get; set; }

    //}

    //public class GoodsReceivedDetailCExport
    //{
    //    public int PurchaseOrderId { get; set; }
    //    public string SupplierName { get; set; }
    //    public string Status { get; set; }
    //    public string GRNo { get; set; }
    //    public int TotalQuantity { get; set; }
    //    public double PurchaseQty { get; set; }
    //    public string WarehouseName { get; set; }
    //    public double PriceRecived { get; set; }
    //    public double Price { get; set; }
    //    public string ItemName { get; set; }
    //    public double MRP { get; set; }
    //    public DateTime GrDate { get; set; }
    //    public DateTime CreationDate { get; set; }
    //    public string ItemNumber { get; set; }
    //    public int ItemMultiMRPId { get; set; }
    //    public string DepoName { get; set; }
    //    public string AverageTAT { get; set; }

    //    public double? POItemFillRate { get; set; }
    //    public string POStatus { get; set; }
    //    public int PurchaseOrderDetailId { get; set; }

    //}

    //public class objDTIR
    //{
    //    public int WHID { get; set; }
    //    public DateTime? From { get; set; }
    //    public DateTime? TO { get; set; }
    //    public int? value { get; set; }
    //    public int list { get; set; }
    //    public int page { get; set; }
    //}
    //public class ExportobjDTIR
    //{
    //    public int WHID { get; set; }
    //    public DateTime? From { get; set; }
    //    public DateTime? TO { get; set; }
    //    public int? value { get; set; }
    //}

    //public class POTATReport
    //{
    //    public int PurchaseOrderId { get; set; }
    //    public string WarehouseName { get; set; }
    //    public string SupplierName { get; set; }
    //    public string Status { get; set; }
    //    public decimal PotoGRTAT { get; set; }
    //    public decimal GRtoIRTAT { get; set; }
    //    public decimal POtoIRTAT { get; set; }
    //    public DateTime CreationDate { get; set; }
    //}
}
