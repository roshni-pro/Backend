using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/AssignmentReport")]
    public class AssignmentReportController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        [HttpGet]
        public AssignmentReportDTO Get(int WarehouseId)
        {
            
            logger.Info("start get all Sales Executive: ");
            AssignmentReportDTO dbReport = new AssignmentReportDTO();
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //Add Foreach for mltiple warehouse list
                   
                    
                        if (Warehouseid == 0 && Warehouseid > 0)
                        {
                            Warehouseid = Warehouseid;
                        }
                    

                    //  List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(datefrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dateto)).ToList();
                    List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid).ToList();

                    var SavedAsDraft = DeliveryIssuance.Where(i => i.Status == "SavedAsDraft").ToList();
                    dbReport.SavedAsDraft = SavedAsDraft.Count;
                    dbReport.SavedAsDraftAmount = SavedAsDraft.Sum(x => x.TotalAssignmentAmount);
                    dbReport.SavedAsDraftExcel = SavedAsDraft.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();


                    ////totalamnt
                    //double? sums = 0;
                    //foreach (var a in SavedAsDraft)
                    //{
                    //    sums = sums + a.TotalAssignmentAmount;
                    //}
                    //dbReport.SavedAsDraftAmount = sums;





                    //Assigned
                    var Assigned = DeliveryIssuance.Where(s => s.Status == "Assigned").ToList();
                    dbReport.Assigned = Assigned.Count;
                    dbReport.AssignedAmount = Assigned.Sum(x => x.TotalAssignmentAmount);
                    dbReport.AssignedExcel = Assigned.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();
                    ////totalamnt
                    //double? sum1 = 0;
                    //foreach (var b in Assigned)
                    //{
                    //    sum1 = sum1 + b.TotalAssignmentAmount;
                    //}
                    //dbReport.AssignedAmount = sum1;


                    //Accepted
                    var Accepted = DeliveryIssuance.Where(s => s.Status == "Accepted").ToList();
                    dbReport.Accepted = Accepted.Count;
                    dbReport.AcceptedAmount = Accepted.Sum(x => x.TotalAssignmentAmount);
                    dbReport.AcceptedExcel = Accepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();

                    //double? sum2 = 0;
                    //foreach (var c in Accepted)
                    //{
                    //    sum2 = sum2 + c.TotalAssignmentAmount;
                    //}
                    //dbReport.AcceptedAmount = sum2;

                    //Rejected
                    var Rejected = DeliveryIssuance.Where(p => p.Status == "Rejected").ToList();
                    dbReport.Rejected = Rejected.Count;
                    dbReport.RejectedAmount = Rejected.Sum(x => x.TotalAssignmentAmount);
                    dbReport.RejectedExcel = Rejected.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();

                    //double? sum3 = 0;
                    //foreach (var d in Rejected)
                    //{
                    //    sum3 = sum3 + d.TotalAssignmentAmount;
                    //}
                    //dbReport.RejectedAmount = sum3;


                    //Pending
                    var Pending = DeliveryIssuance.Where(D => D.Status == "Pending").ToList();
                    dbReport.Pending = Pending.Count;
                    dbReport.PendingAmount = Pending.Sum(x => x.TotalAssignmentAmount);
                    dbReport.PendingExcel = Pending.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();

                    //double? sum4 = 0;
                    //foreach (var a in Pending)
                    //{
                    //    sum4 = sum4 + a.TotalAssignmentAmount;
                    //}
                    //dbReport.PendingAmount = sum4;


                    //Submitted
                    var Submitted = DeliveryIssuance.Where(D => D.Status == "Submitted").ToList();
                    dbReport.Submitted = Submitted.Count;
                    dbReport.SubmittedAmount = Submitted.Sum(x => x.TotalAssignmentAmount);
                    dbReport.SubmittedExcel = Submitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();

                    //double? sum5 = 0;
                    //foreach (var a in Submitted)
                    //{
                    //    sum5 = sum5 + a.TotalAssignmentAmount;
                    //}
                    //dbReport.SubmittedAmount = sum5;

                    //Payment
                    var PaymentAccepted = DeliveryIssuance.Where(D => D.Status == "Payment Accepted").ToList();
                    dbReport.PaymentAccepted = PaymentAccepted.Count;
                    dbReport.PaymentAcceptedAmount = PaymentAccepted.Sum(x => x.TotalAssignmentAmount);
                    dbReport.PaymentAcceptedExcel = PaymentAccepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();




                    var PaymentSubmitted = DeliveryIssuance.Where(D => D.Status == "Payment Submitted").ToList();
                    dbReport.PaymentSubmitted = PaymentSubmitted.Count;
                    dbReport.PaymentSubmittedAmount = PaymentSubmitted.Sum(x => x.TotalAssignmentAmount);
                    dbReport.PaymentSubmittedExcel = PaymentSubmitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();

                    //double? sum6 = 0;
                    //foreach (var e in Payment)
                    //{
                    //    sum6 = sum6 + e.TotalAssignmentAmount;
                    //}
                    //dbReport.PaymentAmount = sum6;

                    //Freezed
                    var Freezed = DeliveryIssuance.Where(k => k.Status == "Freezed").ToList();
                    dbReport.Freezed = Freezed.Count;
                    dbReport.FreezedAmount = Freezed.Sum(x => x.TotalAssignmentAmount);
                    dbReport.FreezedExcel = Freezed.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();



                    // var Export = DeliveryIssuance;
                    dbReport.AllExport = DeliveryIssuance.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate }).ToList();
                    //double? sum7 = 0;
                    //foreach (var f in Freezed)
                    //{
                    //    sum7 = sum7 + f.TotalAssignmentAmount;
                    //}
                    //dbReport.FreezedAmount = sum7;

                    return dbReport;



                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall  Customer " + ex.Message);
                    logger.Info("End get all Customer: ");
                    return null;
                }
            }
        }

        // Added by Anoop 3/2/2021

        [Route("GetReportData")]
        [AcceptVerbs("POST")]
        public async Task<AssignmentReportDTO> SearchReportListData(string warehouseId)
        {

            using (AuthContext context = new AuthContext())
            {
                AssignmentReportDTO dbReport = new AssignmentReportDTO();
                using (var db = new AuthContext())
                {
                    try
                    {

                        var WarehouseIds = warehouseId.Split(',').Select(Int32.Parse).ToList();
                        var warehouse = db.Warehouses.Where(x => WarehouseIds.Contains(x.WarehouseId)).ToList();
                        //  List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(datefrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dateto)).ToList();
                        List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => WarehouseIds.Contains(x.WarehouseId ?? 0)).ToList();

                        // Added 19/2/2021
                        DeliveryIssuance.ForEach(x =>
                        {
                            x.WarehouseName = warehouse.Where(i => i.WarehouseId == x.WarehouseId).Select(z => z.WarehouseName).FirstOrDefault();
                        });

                        var SavedAsDraft = DeliveryIssuance.Where(i => i.Status == "SavedAsDraft").ToList();
                        dbReport.SavedAsDraft = SavedAsDraft.Count;
                        dbReport.SavedAsDraftAmount = SavedAsDraft.Sum(x => x.TotalAssignmentAmount);
                        dbReport.SavedAsDraftExcel = SavedAsDraft.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        ////totalamnt
                        //double? sums = 0;
                        //foreach (var a in SavedAsDraft)
                        //{
                        //    sums = sums + a.TotalAssignmentAmount;
                        //}
                        //dbReport.SavedAsDraftAmount = sums;
                        //Assigned
                        var Assigned = DeliveryIssuance.Where(s => s.Status == "Assigned").ToList();
                        dbReport.Assigned = Assigned.Count;
                        dbReport.AssignedAmount = Assigned.Sum(x => x.TotalAssignmentAmount);
                        dbReport.AssignedExcel = Assigned.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        ////totalamnt
                        //double? sum1 = 0;
                        //foreach (var b in Assigned)
                        //{
                        //    sum1 = sum1 + b.TotalAssignmentAmount;
                        //}
                        //dbReport.AssignedAmount = sum1;


                        //Accepted
                        var Accepted = DeliveryIssuance.Where(s => s.Status == "Accepted").ToList();
                        dbReport.Accepted = Accepted.Count;
                        dbReport.AcceptedAmount = Accepted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.AcceptedExcel = Accepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum2 = 0;
                        //foreach (var c in Accepted)
                        //{
                        //    sum2 = sum2 + c.TotalAssignmentAmount;
                        //}
                        //dbReport.AcceptedAmount = sum2;

                        //Rejected
                        var Rejected = DeliveryIssuance.Where(p => p.Status == "Rejected").ToList();
                        dbReport.Rejected = Rejected.Count;
                        dbReport.RejectedAmount = Rejected.Sum(x => x.TotalAssignmentAmount);
                        dbReport.RejectedExcel = Rejected.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum3 = 0;
                        //foreach (var d in Rejected)
                        //{
                        //    sum3 = sum3 + d.TotalAssignmentAmount;
                        //}
                        //dbReport.RejectedAmount = sum3;


                        //Pending
                        var Pending = DeliveryIssuance.Where(D => D.Status == "Pending").ToList();
                        dbReport.Pending = Pending.Count;
                        dbReport.PendingAmount = Pending.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PendingExcel = Pending.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum4 = 0;
                        //foreach (var a in Pending)
                        //{
                        //    sum4 = sum4 + a.TotalAssignmentAmount;
                        //}
                        //dbReport.PendingAmount = sum4;


                        //Submitted
                        var Submitted = DeliveryIssuance.Where(D => D.Status == "Submitted").ToList();
                        dbReport.Submitted = Submitted.Count;
                        dbReport.SubmittedAmount = Submitted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.SubmittedExcel = Submitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum5 = 0;
                        //foreach (var a in Submitted)
                        //{
                        //    sum5 = sum5 + a.TotalAssignmentAmount;
                        //}
                        //dbReport.SubmittedAmount = sum5;

                        //Payment
                        var PaymentAccepted = DeliveryIssuance.Where(D => D.Status == "Payment Accepted").ToList();
                        dbReport.PaymentAccepted = PaymentAccepted.Count;
                        dbReport.PaymentAcceptedAmount = PaymentAccepted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PaymentAcceptedExcel = PaymentAccepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();




                        var PaymentSubmitted = DeliveryIssuance.Where(D => D.Status == "Payment Submitted").ToList();
                        dbReport.PaymentSubmitted = PaymentSubmitted.Count;
                        dbReport.PaymentSubmittedAmount = PaymentSubmitted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PaymentSubmittedExcel = PaymentSubmitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum6 = 0;
                        //foreach (var e in Payment)
                        //{
                        //    sum6 = sum6 + e.TotalAssignmentAmount;
                        //}
                        //dbReport.PaymentAmount = sum6;

                        //Freezed
                        var Freezed = DeliveryIssuance.Where(k => k.Status == "Freezed").ToList();
                        dbReport.Freezed = Freezed.Count;
                        dbReport.FreezedAmount = Freezed.Sum(x => x.TotalAssignmentAmount);
                        dbReport.FreezedExcel = Freezed.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();



                        // var Export = DeliveryIssuance;
                        dbReport.AllExport = DeliveryIssuance.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        //double? sum7 = 0;
                        //foreach (var f in Freezed)
                        //{
                        //    sum7 = sum7 + f.TotalAssignmentAmount;
                        //}
                        //dbReport.FreezedAmount = sum7;

                        return dbReport;

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in getall  Customer " + ex.Message);
                        logger.Info("End get all Customer: ");
                        return null;
                    }
                }

            }

        }

        // Added by Anoop with Date 22/2/2021

        [Route("GetReportData")]
        [AcceptVerbs("POST")]
        public async Task<AssignmentReportDTO> SearchReportListData(string warehouseId, DateTime? start, DateTime? end)
        {

            using (AuthContext context = new AuthContext())
            {
                AssignmentReportDTO dbReport = new AssignmentReportDTO();
                using (var db = new AuthContext())
                {
                    try
                    {

                        var WarehouseIds = warehouseId.Split(',').Select(Int32.Parse).ToList();
                        var warehouse = db.Warehouses.Where(x => WarehouseIds.Contains(x.WarehouseId)).ToList();
                        //  List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(datefrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dateto)).ToList();
                        List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => WarehouseIds.Contains(x.WarehouseId ?? 0) && x.CreatedDate > start && x.CreatedDate <= end).ToList();

                        // Added 19/2/2021
                        DeliveryIssuance.ForEach(x => {
                            x.WarehouseName = warehouse.Where(i => i.WarehouseId == x.WarehouseId).Select(z => z.WarehouseName).FirstOrDefault();
                        });

                        var SavedAsDraft = DeliveryIssuance.Where(i => i.Status == "SavedAsDraft").ToList();
                        dbReport.SavedAsDraft = SavedAsDraft.Count;
                        dbReport.SavedAsDraftAmount = SavedAsDraft.Sum(x => x.TotalAssignmentAmount);
                        dbReport.SavedAsDraftExcel = SavedAsDraft.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        ////totalamnt
                        //double? sums = 0;
                        //foreach (var a in SavedAsDraft)
                        //{
                        //    sums = sums + a.TotalAssignmentAmount;
                        //}
                        //dbReport.SavedAsDraftAmount = sums;
                        //Assigned
                        var Assigned = DeliveryIssuance.Where(s => s.Status == "Assigned").ToList();
                        dbReport.Assigned = Assigned.Count;
                        dbReport.AssignedAmount = Assigned.Sum(x => x.TotalAssignmentAmount);
                        dbReport.AssignedExcel = Assigned.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        ////totalamnt
                        //double? sum1 = 0;
                        //foreach (var b in Assigned)
                        //{
                        //    sum1 = sum1 + b.TotalAssignmentAmount;
                        //}
                        //dbReport.AssignedAmount = sum1;


                        //Accepted
                        var Accepted = DeliveryIssuance.Where(s => s.Status == "Accepted").ToList();
                        dbReport.Accepted = Accepted.Count;
                        dbReport.AcceptedAmount = Accepted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.AcceptedExcel = Accepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum2 = 0;
                        //foreach (var c in Accepted)
                        //{
                        //    sum2 = sum2 + c.TotalAssignmentAmount;
                        //}
                        //dbReport.AcceptedAmount = sum2;

                        //Rejected
                        var Rejected = DeliveryIssuance.Where(p => p.Status == "Rejected").ToList();
                        dbReport.Rejected = Rejected.Count;
                        dbReport.RejectedAmount = Rejected.Sum(x => x.TotalAssignmentAmount);
                        dbReport.RejectedExcel = Rejected.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum3 = 0;
                        //foreach (var d in Rejected)
                        //{
                        //    sum3 = sum3 + d.TotalAssignmentAmount;
                        //}
                        //dbReport.RejectedAmount = sum3;


                        //Pending
                        var Pending = DeliveryIssuance.Where(D => D.Status == "Pending").ToList();
                        dbReport.Pending = Pending.Count;
                        dbReport.PendingAmount = Pending.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PendingExcel = Pending.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum4 = 0;
                        //foreach (var a in Pending)
                        //{
                        //    sum4 = sum4 + a.TotalAssignmentAmount;
                        //}
                        //dbReport.PendingAmount = sum4;


                        //Submitted
                        var Submitted = DeliveryIssuance.Where(D => D.Status == "Submitted").ToList();
                        dbReport.Submitted = Submitted.Count;
                        dbReport.SubmittedAmount = Submitted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.SubmittedExcel = Submitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum5 = 0;
                        //foreach (var a in Submitted)
                        //{
                        //    sum5 = sum5 + a.TotalAssignmentAmount;
                        //}
                        //dbReport.SubmittedAmount = sum5;

                        //Payment
                        var PaymentAccepted = DeliveryIssuance.Where(D => D.Status == "Payment Accepted").ToList();
                        dbReport.PaymentAccepted = PaymentAccepted.Count;
                        dbReport.PaymentAcceptedAmount = PaymentAccepted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PaymentAcceptedExcel = PaymentAccepted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();




                        var PaymentSubmitted = DeliveryIssuance.Where(D => D.Status == "Payment Submitted").ToList();
                        dbReport.PaymentSubmitted = PaymentSubmitted.Count;
                        dbReport.PaymentSubmittedAmount = PaymentSubmitted.Sum(x => x.TotalAssignmentAmount);
                        dbReport.PaymentSubmittedExcel = PaymentSubmitted.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();

                        //double? sum6 = 0;
                        //foreach (var e in Payment)
                        //{
                        //    sum6 = sum6 + e.TotalAssignmentAmount;
                        //}
                        //dbReport.PaymentAmount = sum6;

                        //Freezed
                        var Freezed = DeliveryIssuance.Where(k => k.Status == "Freezed").ToList();
                        dbReport.Freezed = Freezed.Count;
                        dbReport.FreezedAmount = Freezed.Sum(x => x.TotalAssignmentAmount);
                        dbReport.FreezedExcel = Freezed.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();



                        // var Export = DeliveryIssuance;
                        dbReport.AllExport = DeliveryIssuance.Select(x => new AssignmentExel { AssinmentID = x.DeliveryIssuanceId, DBoy_Name = x.DisplayName, TotalAssignment_Amount = x.TotalAssignmentAmount, Status = x.Status, OrderIds = x.OrderIds, CreatedDate = x.CreatedDate, UpdateDate = x.UpdatedDate, WarehouseName = x.WarehouseName }).ToList();
                        //double? sum7 = 0;
                        //foreach (var f in Freezed)
                        //{
                        //    sum7 = sum7 + f.TotalAssignmentAmount;
                        //}
                        //dbReport.FreezedAmount = sum7;

                        return dbReport;

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in getall  Customer " + ex.Message);
                        logger.Info("End get all Customer: ");
                        return null;
                    }
                }

            }

        }



        [Route("Export")]
        [HttpGet]
        public dynamic Getexport(int WarehouseId, string value)
        {
            logger.Info("start get all Sales Executive: ");
            //AssignmentReportDTO dbReport = new AssignmentReportDTO();
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouseid = 0;
                    dynamic result = null;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouseid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouseid == 0 && WarehouseId > 0)
                    {
                        Warehouseid = WarehouseId;
                    }

                    //  List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(datefrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dateto)).ToList();
                    List<DeliveryIssuance> DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.WarehouseId == Warehouseid).ToList();

                   
                    if (value != null)
                    {
                        result = DeliveryIssuance.Where(x => x.Status == value).ToList();
                    }
                    return result;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall  Customer " + ex.Message);
                    logger.Info("End get all Customer: ");
                    return null;
                }
            }
        }

        [Route("ExportAssignmentReport")]
        [HttpPost]
        public List<ExportAssignmentDetails> ExportAssignmentReport(string warehouseId, DateTime start, DateTime end)
        {
            List < ExportAssignmentDetails > list = new List<ExportAssignmentDetails>();
            using (var db = new AuthContext())
            {
                var WarehouseIds = warehouseId.Split(',').Select(Int32.Parse).ToList();
                var WIds = new DataTable();
                WIds.Columns.Add("IntValue");
                foreach (var item in WarehouseIds)
                {
                    var dr = WIds.NewRow();
                    dr["IntValue"] = item;
                    WIds.Rows.Add(dr);
                }
                var WId = new SqlParameter("@WarehouseId", WIds);
                WId.SqlDbType = SqlDbType.Structured;
                WId.TypeName = "dbo.IntValues";
                var startDate = new SqlParameter("@StartDate", start);
                var endDate = new SqlParameter("@EndDate", end);
                list = db.Database.SqlQuery<ExportAssignmentDetails>("EXEC GetAssignmentReportInDetails @WarehouseId,@StartDate,@EndDate", WId, startDate, endDate).ToList();
                return list;
            }
        }
    }
}


public class AssignmentReportDTO
{
    public int Pending { get; set; }//Pending
    public double? PendingAmount { get; set; }
    public List<AssignmentExel> PendingExcel { get; set; }

    public int PaymentAccepted { get; set; }//Payment
    public double? PaymentAcceptedAmount { get; set; }
    public List<AssignmentExel> PaymentAcceptedExcel { get; set; }

    public int PaymentSubmitted { get; set; }//Payment Submitted 
    public double? PaymentSubmittedAmount { get; set; }
    public List<AssignmentExel> PaymentSubmittedExcel { get; set; }

    public int SavedAsDraft { get; set; }//SavedAsDraft
    public double? SavedAsDraftAmount { get; set; }
    public List<AssignmentExel> SavedAsDraftExcel { get; set; }

    public int Assigned { get; set; }//Assigned
    public double? AssignedAmount { get; set; }
    public List<AssignmentExel> AssignedExcel { get; set; }


    public int Freezed { get; set; }//Freezed
    public double? FreezedAmount { get; set; }
    public List<AssignmentExel> FreezedExcel { get; set; }


    public int Rejected { get; set; }//Rejected
    public double? RejectedAmount { get; set; }
    public List<AssignmentExel> RejectedExcel { get; set; }


    public int Submitted { get; set; } //Submitted
    public double? SubmittedAmount { get; set; }
    public List<AssignmentExel> SubmittedExcel { get; set; }

    public int DeliveredRedispatched { get; set; }
    public double TotDeliveredRedispatchedAmnt { get; set; }
    public List<AssignmentExel> DeliveredRedispatchedExcel { get; set; }


    public int Accepted { get; set; }//Accepted
    public double? AcceptedAmount { get; set; }
    public List<AssignmentExel> AcceptedExcel { get; set; }

    public List<AssignmentExel> AllExport { get; set; }



}
public class AssignmentExel
{
    public int AssinmentID { get; set; }
    public string WarehouseName { get; set; }
    public string DBoy_Name { get; set; }
    public double? TotalAssignment_Amount { get; set; }
    public string Status { get; set; }
    public string OrderIds { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdateDate { get; set; }
}
public class ExportAssignmentDetails
{
    public int AssinmentID { get; set; }
    public string WarehouseName { get; set; }
    public string DBoy_Name { get; set; }
    public double? TotalAssignment_Amount { get; set; }
    public int NumberofOrders { get; set; }
    public string Status { get; set; }
    public string OrderIds { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public double CollectionDelivered { get; set; }
    public int DeliveredCount { get; set; }
    public double DeliveryCanceledAmount { get; set; }
    public int DeliveryCanceledCount { get; set; }
    public double RedispatchAmount { get; set; }
    public int RedispatchCount { get; set; }
    public string DeliveredPercentage { get; set; }
}





