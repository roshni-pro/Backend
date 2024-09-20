using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.WarehouseUtilization;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.FleetMaster;
using AngularJSAuthentication.Model.WarehouseUtilization;
using Microsoft.Extensions.Logging;
using NLog;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.WarehouseUtilization
{
    [RoutePrefix("api/TransporterPayment")]
    public class TransporterPaymentController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Utility
        [HttpGet]
        [AllowAnonymous]
        [Route("TransporterPaymentInsert")]
        public async Task<bool> TransporterPaymentInsert()
        {
            DateTime TodayDate = DateTime.Today;
            TodayDate = TodayDate.AddMonths(-1);
            DateTime startdate = new DateTime(TodayDate.Year, TodayDate.Month, 1);
            TransporterPaymentManager manager = new TransporterPaymentManager();
            List<paymentWarehouseDc> warehouseList = new List<paymentWarehouseDc>();
            using (AuthContext db = new AuthContext())
            {
                var WCWH = "select w.WarehouseId,w.WarehouseName, w.RegionId,w.Cityid from Warehouses " +
                "w inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1 " +
                "and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%' order by (w.WarehouseId)";
                warehouseList = db.Database.SqlQuery<paymentWarehouseDc>(WCWH).ToList();

            }
            if (warehouseList != null && warehouseList.Any())
            {
                foreach (paymentWarehouseDc wh in warehouseList)
                {
                    List<FleetMaster> fleetslist = new List<FleetMaster>();
                    using (AuthContext db = new AuthContext())
                    {
                        fleetslist = db.FleetMasterDB.Where(x => x.CityId == wh.Cityid && x.IsActive == true && x.IsDeleted == false).ToList();
                    }
                    if (fleetslist != null && fleetslist.Any())
                    {
                        foreach (FleetMaster fleet in fleetslist)
                        {
                            var now = DateTime.Now;
                            var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                            var Enddate = new DateTime(now.Year, now.Month, DaysInMonth);
                            bool IsVehicleExists = false;
                            using (AuthContext db = new AuthContext())
                            {
                                var FleetIdIdParam = new SqlParameter
                                {
                                    ParameterName = "FleetMasterId",
                                    Value = fleet.Id
                                };
                                var WarehIdParam = new SqlParameter
                                {
                                    ParameterName = "warehouseId",
                                    Value = wh.WarehouseId
                                };
                                IsVehicleExists = db.Database.SqlQuery<bool>("exec Operation.TransporterPaymentISVehicleExist @FleetMasterId,@warehouseId", FleetIdIdParam, WarehIdParam).FirstOrDefault();
                            }
                            if (IsVehicleExists == true)
                            {
                                long TransporterPaymentId = await manager.TransporterPaymentInsert(startdate, fleet.Id, wh.WarehouseId);
                                var list = await manager.TransporterPaymentDetailInsert(TransporterPaymentId, startdate, Enddate);
                            }
                        }
                    }
                }
            }

            return true;
        }

        #endregion

        #region For WarehouseLead
        [HttpPost]
        [Route("GetTranspoterPaymentDetailList")]
        public async Task<TransporterPayWithDetailDc> GetTranspoterPaymentDetailList(TransporterPaymentVm transporterPaymentVm)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            TransporterPayWithDetailDc transporterPayWithDetailDc = new TransporterPayWithDetailDc();
            var list = await manager.GetTranspoterPaymentDetailList(transporterPaymentVm);
            transporterPayWithDetailDc = await manager.TransporterPayWithDetail(transporterPaymentVm);
            if (transporterPayWithDetailDc != null)
            {
                transporterPayWithDetailDc.getTranspoterPaymentDetailList = list;
            }
            return transporterPayWithDetailDc;

        }

        [HttpGet]
        [Route("GetFleetListByWhId")]
        public async Task<List<GetFleetListDc>> GetFleetListByWhId(int WarehouseId)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetFleetListByWhId(WarehouseId);
            return list;
        }

        [HttpPost]
        [Route("GetTransporterPaymentVehicleAttadanceList")]
        public async Task<List<TransporterPayVehicleAttadanceListDc>> GetTransporterPaymentVehicleAttadanceList(TransporterVehicleAttadanceDc transporterVehicleAttadanceDc)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetTransporterPaymentVehicleAttadanceList(transporterVehicleAttadanceDc);
            return list;
        }

        [HttpPost]
        [Route("UpdateTransporterPaymentDetailById")]

        public async Task<TransporterResponseDc> UpdateTransporterPaymentDetailById(TransporterPaymentDetailVm transporterPaymentDetailVm)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            if (!roleNames.Any(x => x == "Hub service lead" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = true,
                    Message = "User not have Hub service lead Role!! "
                };
                return res;
            }
            using (var db = new AuthContext())
            {
                var updatedata = db.TransporterPaymentDetailDb.Where(x => x.Id == transporterPaymentDetailVm.TransporterPaymentDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (updatedata != null)
                {
                    var TransporterStatus = db.TransporterPaymentDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == updatedata.TransporterPaymentId).FirstOrDefault();
                    if (TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.Pending || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional
                        || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount)
                    {
                        updatedata.TollAmount = transporterPaymentDetailVm.TollAmount;
                        updatedata.OtherExpense = transporterPaymentDetailVm.OtherExpense;
                        updatedata.Remark = transporterPaymentDetailVm.Remark;

                        if (transporterPaymentDetailVm.MonthlyContractAmount > 0)
                        {
                            updatedata.MonthlyContractAmount = transporterPaymentDetailVm.MonthlyContractAmount;
                            updatedata.UtilizedAmount = transporterPaymentDetailVm.UtilizedAmount;
                        }

                        if (updatedata.TollAmount + updatedata.OtherExpense + updatedata.UtilizedAmount + updatedata.ExtraKmAmt < 0)
                        {
                            res = new TransporterResponseDc
                            {
                                Status = false,
                                Message = "Amount should be greater than 0!! "
                            };
                            return res;
                        }
                        updatedata.ModifiedBy = userid;
                        updatedata.ModifiedDate = DateTime.Now;
                        db.Entry(updatedata).State = EntityState.Modified;
                        db.Commit();
                        res = new TransporterResponseDc
                        {
                            Status = true,
                            Message = "Updated SuccessFully!! "
                        };
                        return res;
                    }
                    else
                    {
                        res = new TransporterResponseDc
                        {
                            Status = true,
                            Message = "Not Updated Because payment is not pending/RejectedByRegional!! "

                        };
                        return res;
                    }
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Not Updated Because Payment not found!! "

                    };
                    return res;
                }
            }
        }

        [HttpPost]
        [Route("InsertTransporterPaymentDetailDoc")]
        public bool InsertTransporterPaymentDetailDoc(List<TransporterPayDetailDocDc> transporterPayDetailDocList)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                foreach (var item in transporterPayDetailDocList)
                {
                    TransporterPaymentDetailDoc transporterPaymentDetailDoc = new TransporterPaymentDetailDoc
                    {
                        DocPath = item.DocPath,
                        DocType = item.DocType,
                        TransporterPaymentDetailId = item.TransporterPaymentDetailId,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    db.TransporterPaymentDetailDocs.Add(transporterPaymentDetailDoc);
                    db.Commit();
                }
            }
            return true;
        }

        [HttpPost]
        [Route("DeleteTransporterPaymentDetailDocById")]
        public bool DeleteTransporterPaymentDetailDocById(long Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var deletedata = db.TransporterPaymentDetailDocs.Where(x => x.Id == Id).FirstOrDefault();
                if (deletedata != null)
                {
                    deletedata.IsActive = false;
                    deletedata.IsDeleted = true;
                    deletedata.ModifiedBy = userid;
                    deletedata.ModifiedDate = DateTime.Now;
                    db.Entry(deletedata).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [HttpPost]
        [Route("Fianlized")]
        public async Task<TransporterResponseDc> Fianlized(FinalizeVm finalizeVm)
        {
            double RemainingAmount = 0;
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Hub service lead" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Hub service lead Role!! "
                };
                return res;
            }
            using (var db = new AuthContext())
            {
                var PaymentIdParam = new SqlParameter
                {
                    ParameterName = "paymentId",
                    Value = finalizeVm.TransporterPaymentId
                };
                bool IsIGST = db.Database.SqlQuery<bool>("exec Operation.TransporterPaymentIsGSTApplicable @paymentId", PaymentIdParam).FirstOrDefault();
                double TaxPercentage = 6;
                var amount = db.TransporterPaymentDetailDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.TransporterPaymentId == finalizeVm.TransporterPaymentId)
                   .Select(y => y.UtilizedAmount + y.OtherExpense + y.TollAmount + y.ExtraKmAmt)
                   .Sum();
                if (finalizeVm.TaxType == "RCM")
                {
                    TaxPercentage = 2.5;
                }
                var TransporterStatus = db.TransporterPaymentDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == finalizeVm.TransporterPaymentId).FirstOrDefault();
                var fleetMaster = db.FleetMasterDB.Where(x => x.Id == TransporterStatus.FleetMasterId).FirstOrDefault();
                var PanNum = fleetMaster.PanNo;
                double TDSper = 0;
                if (fleetMaster == null || fleetMaster.IsAppprovedByAccount == false)
                {
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Please Approve All Account Information in FleetMaster!! "
                    };
                    return res;
                }
                if (string.IsNullOrEmpty(PanNum) || PanNum.Length < 4)
                {
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Please Update the PAN Number!! "
                    };
                    return res;
                }
                else
                {
                    if (PanNum[3] == 'P' || PanNum[3] == 'H')
                    {
                        TDSper = 1;
                    }
                    else //   C||F
                    {
                        TDSper = 2;
                    }
                }
                if (TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.Pending || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional
                    || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead || TransporterStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount)
                {
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = "Finalized",
                        TransporterPaymentId = TransporterStatus.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "Hub service lead",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    if (IsIGST)
                    {
                        TransporterStatus.IGST = amount * (2 * TaxPercentage) / 100.0;
                        TransporterStatus.CGST = 0;
                        TransporterStatus.SGST = 0;
                        TransporterStatus.IGSTPer = 2 * TaxPercentage;
                        TransporterStatus.CGSTPer = 0;
                        TransporterStatus.SGSTPer = 0;
                    }
                    else
                    {
                        TransporterStatus.IGST = 0;
                        TransporterStatus.CGST = amount * (TaxPercentage) / 100.0;
                        TransporterStatus.SGST = amount * (TaxPercentage) / 100.0;
                        TransporterStatus.IGSTPer = 0;
                        TransporterStatus.CGSTPer = TaxPercentage;
                        TransporterStatus.SGSTPer = TaxPercentage;
                    }
                    var AccountDetail = db.fleetMasterAccountDetailDB.Where(x => x.FleetMasterId == TransporterStatus.FleetMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (AccountDetail != null)
                    {
                        TransporterStatus.HolderName = AccountDetail.HolderName;
                        TransporterStatus.IFSCcode = AccountDetail.IFSCcode;
                        TransporterStatus.BranchName = AccountDetail.BranchName;
                        TransporterStatus.BankName = AccountDetail.BankName;
                        TransporterStatus.AccountNo = AccountDetail.AccountNo;
                    }
                    TransporterStatus.TransporterInvoicePath = finalizeVm.TransporterInvoicePath;
                    TransporterStatus.ModifiedBy = userid;
                    TransporterStatus.ModifiedDate = DateTime.Now;
                    TransporterStatus.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.Finalized;
                    TransporterStatus.TaxType = finalizeVm.TaxType;
                    TransporterStatus.InvoiceDate = finalizeVm.InvoiceDate;
                    TransporterStatus.InvoiceNumber = finalizeVm.InvoiceNumber;
                    TransporterStatus.TDSAmount = amount * TDSper / 100;
                    TransporterStatus.TDSPer = TDSper;
                    TransporterStatus.PaidAmount = amount;
                    TransporterStatus.BookingDate = finalizeVm.InvoiceDate;
                    if (finalizeVm.TaxType == "RCM")
                    {
                        RemainingAmount = TransporterStatus.PaidAmount;
                    }
                    else
                    {
                        RemainingAmount = TransporterStatus.PaidAmount; // + (TransporterStatus.CGST + TransporterStatus.IGST + TransporterStatus.SGST);
                    }
                    TransporterStatus.RemainingAmount = RemainingAmount;
                    //if (TransporterStatus.TaxType == "FCM")
                    //{
                    //    TransporterStatus.PaidAmount = amount + TransporterStatus.IGST + TransporterStatus.CGST + TransporterStatus.SGST - TransporterStatus.TDSAmount;
                    //}
                    //else //RCM
                    //{
                    //    TransporterStatus.PaidAmount = amount - TransporterStatus.TDSAmount;
                    //}
                    db.Entry(TransporterStatus).State = EntityState.Modified;
                    db.Commit();
                    TransporterStatus.GeneratedInvoicePath = await GenerateInvoicePartial(TransporterStatus.Id, TransporterStatus.TransporterMonthlyDate);
                    db.Entry(TransporterStatus).State = EntityState.Modified;
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Updated SuccessFully!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "User not have Hub service lead Role!! "
                    };
                    return res;
                }
            }

        }

        #endregion

        #region For Regional
        [HttpGet]
        [Route("GetRegionalList")]
        public async Task<List<GetRegionalListDc>> GetRegionalList(int Warehouseid, DateTime ForDate)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetRegionalList(Warehouseid, ForDate);
            return list;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetRegionalListByAllWh")]
        public async Task<List<GetRegionalListByAllWh>> GetRegionalListByAllWh(RegionalAllWHInput input)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetRegionalListByAllWh(input);
            return list;
        }

        [HttpPost]
        [Route("GetRegionalSummayList")]
        public async Task<List<GetRegionalListDc>> GetRegionalSummaryList(ReginalSummaryInput input)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetRegionalSummaryList(input);
            return list;
        }



        [HttpPost]
        [Route("RegionalTranspoterPaymentDetailList")]
        public async Task<List<GetTranspoterPaymentDetailDc>> RegionalTranspoterPaymentDetailList(TransporterPaymentVm transporterPaymentVm)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetTranspoterPaymentDetailList(transporterPaymentVm);
            return list;
        }

        [HttpPost]
        [Route("ApproveRejectByRegional")]
        public async Task<TransporterResponseDc> ApproveRejectByRegional(ApprovedRejByRegionalVm approvedRejByRegionalVm)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Regional Outbound Lead" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Regional Outbound Lead!! "
                };
                return res;
            }

            using (var db = new AuthContext())
            {
                var transporterPayment = db.TransporterPaymentDb.Where(x => x.Id == approvedRejByRegionalVm.TransporterPaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (transporterPayment != null && transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.Finalized)
                {
                    if (approvedRejByRegionalVm.IsApprove == true)
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional;
                    }
                    else
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional;
                        transporterPayment.GeneratedInvoicePath = "";
                    }
                    transporterPayment.RegionalComment = approvedRejByRegionalVm.Comment;
                    transporterPayment.ModifiedBy = userid;
                    transporterPayment.ModifiedDate = DateTime.Now;
                    db.Entry(transporterPayment).State = EntityState.Modified;
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = approvedRejByRegionalVm.IsApprove == true ? "Approved" : "Rejected",
                        TransporterPaymentId = transporterPayment.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "Regional Outbound Lead",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Success!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Payment is not Finalized!! "
                    };
                    return res;
                }
            }
        }
        #endregion

        #region For HQOps Lead
        [HttpPost]
        [Route("ApproveRejectByOpsLead")]
        public async Task<TransporterResponseDc> ApproveRejectByOpsLead(ApprovedRejByRegionalVm approvedRejByRegionalVm)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "HQ Ops Lead" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have HQ Ops Lead!! "
                };
                return res;
            }

            using (var db = new AuthContext())
            {
                var transporterPayment = db.TransporterPaymentDb.Where(x => x.Id == approvedRejByRegionalVm.TransporterPaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var amount = transporterPayment.PaidAmount + transporterPayment.CGST + transporterPayment.IGST + transporterPayment.SGST;
                if (transporterPayment != null && transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional && amount > 200000)
                {
                    if (approvedRejByRegionalVm.IsApprove == true)
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.ApprovedByHQOpsLead;
                    }
                    else
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead;
                        transporterPayment.GeneratedInvoicePath = "";
                    }
                    transporterPayment.HQOpsLeadComment = approvedRejByRegionalVm.Comment;
                    transporterPayment.ModifiedBy = userid;
                    transporterPayment.ModifiedDate = DateTime.Now;
                    db.Entry(transporterPayment).State = EntityState.Modified;
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = approvedRejByRegionalVm.IsApprove == true ? "Approved" : "Rejected",
                        TransporterPaymentId = transporterPayment.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "HQ Ops Lead",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Success!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Regional not approved yet!! "
                    };
                    return res;
                }
            }
        }
        #endregion

        #region For Accounts

        [HttpPost]
        [Route("ApproveRejectByAccounts")]
        public async Task<TransporterResponseDc> ApproveRejectByAccounts(ApprovedRejByRegionalVm approvedRejByRegionalVm)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Accounts executive" || x == "Accounts associates" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Accounts executive or Accounts associates!! "
                };
                return res;
            }

            using (var db = new AuthContext())
            {
                var transporterPayment = db.TransporterPaymentDb.Where(x => x.Id == approvedRejByRegionalVm.TransporterPaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var amount = db.TransporterPaymentDetailDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.TransporterPaymentId == transporterPayment.Id)
                    .Select(y => y.UtilizedAmount + y.OtherExpense + y.TollAmount + y.ExtraKmAmt)
                    .Sum();
                if (transporterPayment != null && ((transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional && amount <= 200000)
                    || (transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByHQOpsLead && amount >= 200000)
                    || transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByAccountLead))
                {
                    if (approvedRejByRegionalVm.IsApprove == true)
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.ApporvedByAccount;
                    }
                    else
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount;
                        transporterPayment.GeneratedInvoicePath = "";
                    }
                    transporterPayment.AccountComment = approvedRejByRegionalVm.Comment;
                    transporterPayment.ModifiedBy = userid;
                    transporterPayment.ModifiedDate = DateTime.Now;
                    db.Entry(transporterPayment).State = EntityState.Modified;
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = approvedRejByRegionalVm.IsApprove == true ? "Approved" : "Rejected",
                        TransporterPaymentId = transporterPayment.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "Accounts executive/Accounts associates",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Success!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Regional not Approved Yet!! "
                    };
                    return res;
                }
            }
        }

        [HttpPost]
        [Route("ApproveRejectByAccountLead")]
        public async Task<TransporterResponseDc> ApproveRejectByAccountLead(ApprovedRejByRegionalVm approvedRejByRegionalVm)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "HQ Accounts Lead" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Accounts executive or Accounts associates!! "
                };
                return res;
            }

            using (var db = new AuthContext())
            {
                LadgerHelper ladgerHelper = new LadgerHelper();
                var transporterPayment = db.TransporterPaymentDb.Where(x => x.Id == approvedRejByRegionalVm.TransporterPaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                string deliveryLogisticsLedgerName = db.Warehouses.Where(x => x.WarehouseId == transporterPayment.WarehouseId).FirstOrDefault().WarehouseName;
                if (transporterPayment != null && transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApporvedByAccount)
                {
                    if (approvedRejByRegionalVm.IsApprove == true)
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.ApprovedByAccountLead;
                        //var deliveryLogisticLedgerType = ladgerHelper.GetOrCreateLadgerType(, userid, db);
                        var Transporterladger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Transporter", (int)transporterPayment.FleetMasterId, userid, db);
                        var TransporterAffectedladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("Delivery Logistics", 0, userid, db, "Delivery Logistics");

                        var purchaseExpenseVoucherType = ladgerHelper.GetOrCreateVoucherType("Purchase Expense", userid, db);
                        LadgerEntry amountladgerEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = TransporterAffectedladger.ID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Credit = transporterPayment.PaidAmount,
                            Date = transporterPayment.BookingDate,
                            Debit = 0,
                            ID = 0,
                            IrPaymentDetailsId = null,
                            IsSupplierAdvancepay = null,
                            LagerID = Transporterladger.ID,
                            ObjectID = transporterPayment.Id,
                            ObjectType = "TransporterPayment",
                            ParentId = null,
                            Particulars = "",
                            PRPaymentId = null,
                            RefNo = "",
                            Remark = "",
                            UpdatedBy = userid,
                            UpdatedDate = DateTime.Now,
                            UploadGUID = null,
                            VouchersNo = null,
                            VouchersTypeID = purchaseExpenseVoucherType.ID
                        };
                        ladgerHelper.Createadjustladgerentry(amountladgerEntry, userid, db);
                        amountladgerEntry = Mapper.Map(amountladgerEntry).ToANew<LadgerEntry>();
                        var ladgerid = amountladgerEntry.LagerID;
                        amountladgerEntry.LagerID = amountladgerEntry.AffectedLadgerID;
                        amountladgerEntry.AffectedLadgerID = ladgerid;

                        var debitamount = amountladgerEntry.Debit;
                        amountladgerEntry.Debit = amountladgerEntry.Credit;
                        amountladgerEntry.Credit = debitamount;
                        amountladgerEntry.ID = 0;
                        ladgerHelper.Createadjustladgerentry(amountladgerEntry, userid, db);

                        List<LadgerEntry> ladgerEntries = new List<LadgerEntry>();
                        //var taxLadgerType = ladgerHelper.GetOrCreateLadgerType("Tax", userid, db);

                        var taxVoucherType = ladgerHelper.GetOrCreateVoucherType("Tax", userid, db);

                        if (transporterPayment.TaxType == "FCM")
                        {

                            if (transporterPayment.IGST > 0)
                            {
                                var IGSTladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("Tax", 0, userid, db, "T-IGST-" + transporterPayment.IGSTPer.ToString());
                                LadgerEntry IGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = IGSTladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = transporterPayment.IGST,
                                    Date = transporterPayment.BookingDate,
                                    Debit = 0,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = Transporterladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(IGSTladgerEntry);

                                LadgerEntry ReverseIGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = Transporterladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = 0,
                                    Date = transporterPayment.BookingDate,
                                    Debit = transporterPayment.IGST,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = IGSTladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(ReverseIGSTladgerEntry);
                            }
                            else
                            {
                                var CGSTladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("Tax", 0, userid, db, "T-CGST-" + transporterPayment.CGSTPer.ToString());
                                LadgerEntry IGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = CGSTladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = transporterPayment.CGST,
                                    Date = transporterPayment.BookingDate,
                                    Debit = 0,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = Transporterladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(IGSTladgerEntry);

                                LadgerEntry ReverseCGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = Transporterladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = 0,
                                    Date = transporterPayment.BookingDate,
                                    Debit = transporterPayment.CGST,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = CGSTladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(ReverseCGSTladgerEntry);

                                var SGSTladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("Tax", 0, userid, db, "T-SGST-" + transporterPayment.SGSTPer.ToString());
                                LadgerEntry SGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = SGSTladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = transporterPayment.SGST,
                                    Date = transporterPayment.BookingDate,
                                    Debit = 0,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = Transporterladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(SGSTladgerEntry);

                                LadgerEntry ReverseSGSTladgerEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = Transporterladger.ID,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Credit = 0,
                                    Date = transporterPayment.BookingDate,
                                    Debit = transporterPayment.SGST,
                                    ID = 0,
                                    IrPaymentDetailsId = null,
                                    IsSupplierAdvancepay = null,
                                    LagerID = SGSTladger.ID,
                                    ObjectID = transporterPayment.Id,
                                    ObjectType = "TransporterPayment",
                                    ParentId = null,
                                    Particulars = "",
                                    PRPaymentId = null,
                                    RefNo = "",
                                    Remark = "",
                                    UpdatedBy = userid,
                                    UpdatedDate = DateTime.Now,
                                    UploadGUID = null,
                                    VouchersNo = null,
                                    VouchersTypeID = taxVoucherType.ID
                                };
                                ladgerEntries.Add(ReverseSGSTladgerEntry);
                            }
                        }


                        var TDSladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("Tax", 0, userid, db, "T-TDS-" + transporterPayment.TDSPer.ToString());
                        LadgerEntry TDSladgerEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = TDSladger.ID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Credit = 0,
                            Date = transporterPayment.BookingDate,
                            Debit = transporterPayment.TDSAmount,
                            ID = 0,
                            IrPaymentDetailsId = null,
                            IsSupplierAdvancepay = null,
                            LagerID = Transporterladger.ID,
                            ObjectID = transporterPayment.Id,
                            ObjectType = "TransporterPayment",
                            ParentId = null,
                            Particulars = "",
                            PRPaymentId = null,
                            RefNo = "",
                            Remark = "",
                            UpdatedBy = userid,
                            UpdatedDate = DateTime.Now,
                            UploadGUID = null,
                            VouchersNo = null,
                            VouchersTypeID = taxVoucherType.ID
                        };
                        ladgerEntries.Add(TDSladgerEntry);

                        LadgerEntry ReverseTDSladgerEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = Transporterladger.ID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Credit = transporterPayment.TDSAmount,
                            Date = transporterPayment.BookingDate,
                            Debit = 0,
                            ID = 0,
                            IrPaymentDetailsId = null,
                            IsSupplierAdvancepay = null,
                            LagerID = TDSladger.ID,
                            ObjectID = transporterPayment.Id,
                            ObjectType = "TransporterPayment",
                            ParentId = null,
                            Particulars = "",
                            PRPaymentId = null,
                            RefNo = "",
                            Remark = "",
                            UpdatedBy = userid,
                            UpdatedDate = DateTime.Now,
                            UploadGUID = null,
                            VouchersNo = null,
                            VouchersTypeID = taxVoucherType.ID
                        };
                        ladgerEntries.Add(ReverseTDSladgerEntry);


                        if (ladgerEntries != null && ladgerEntries.Any())
                        {
                            foreach (var item in ladgerEntries)
                            {
                                ladgerHelper.Createadjustladgerentry(item, userid, db);
                            }
                        }
                    }
                    else
                    {
                        transporterPayment.ApprovalStatus = (int)TransporterPaymentApprovalStatusEnum.RejectedByAccountLead;
                    }
                    transporterPayment.AccountLeadComment = approvedRejByRegionalVm.Comment;
                    transporterPayment.ModifiedBy = userid;
                    transporterPayment.ModifiedDate = DateTime.Now;
                    db.Entry(transporterPayment).State = EntityState.Modified;
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = approvedRejByRegionalVm.IsApprove == true ? "Approved" : "Rejected",
                        TransporterPaymentId = transporterPayment.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "HQ Accounts Lead",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Success!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "HQ Accounts Lead not Approved Yet!! "
                    };
                    return res;
                }
            }
        }

        [HttpGet]
        [Route("UpdateBookingDate")]
        public async Task<TransporterResponseDc> UpdateBookingDate(long PaymentId, DateTime Date)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Accounts executive" || x == "Accounts associates" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Accounts executive or Accounts associates!! "
                };
                return res;
            }

            using (var db = new AuthContext())
            {
                var transporterPayment = db.TransporterPaymentDb.Where(x => x.Id == PaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var amount = db.TransporterPaymentDetailDb.Where(x => x.IsActive == true && x.IsDeleted == false && x.TransporterPaymentId == transporterPayment.Id)
                    .Select(y => y.UtilizedAmount + y.OtherExpense + y.TollAmount)
                    .Sum();
                if (transporterPayment != null && ((transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional && amount <= 200000)
                    || (transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByHQOpsLead && amount >= 200000)
                    || transporterPayment.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByAccountLead))
                {

                    transporterPayment.BookingDate = Date;
                    transporterPayment.ModifiedBy = userid;
                    transporterPayment.ModifiedDate = DateTime.Now;
                    db.Entry(transporterPayment).State = EntityState.Modified;
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Success!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Regional not Approved Yet!! "
                    };
                    return res;
                }
            }
        }


        [HttpPost]
        [Route("UpdateInvoice")]
        public async Task<TransporterResponseDc> UpdateInvoice(UpdateInvoiceDc updateInvoiceDc)
        {
            TransporterResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (!roleNames.Any(x => x == "Accounts executive" || x == "Accounts associates" || x == "HQ Master login"))
            {
                res = new TransporterResponseDc
                {
                    Status = false,
                    Message = "User not have Accounts executive or Accounts associates!! "
                };
                return res;
            }
            using (var db = new AuthContext())
            {
                LadgerHelper ladgerHelper = new LadgerHelper();
                double PaidAmount = 0;
                var updateTransactionId = db.TransporterPaymentDb.Where(x => x.Id == updateInvoiceDc.PaymentId).FirstOrDefault();
                if (updateTransactionId != null && updateTransactionId.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.ApprovedByAccountLead
                    && updateTransactionId.PaymentStatus == (int)TransporterPaymentPaymentStatusEnum.Pending)
                {
                    PaidAmount = updateTransactionId.PaidAmount;
                    if (updateTransactionId.TaxType == "FCM")
                    {
                        PaidAmount = PaidAmount + updateTransactionId.CGST + updateTransactionId.SGST + updateTransactionId.IGST;
                    }
                    PaidAmount = PaidAmount - updateTransactionId.TDSAmount;
                    updateTransactionId.TransactionId = updateInvoiceDc.TransactionId;
                    updateTransactionId.PaymentStatus = (int)TransporterPaymentPaymentStatusEnum.PaymentDone;
                    updateTransactionId.PaymentDate = updateInvoiceDc.PaymentDate;
                    updateTransactionId.ModifiedBy = userid;
                    updateTransactionId.ModifiedDate = DateTime.Now;
                    updateTransactionId.RemainingAmount = PaidAmount - updateInvoiceDc.PaidAmount;
                    db.Entry(updateTransactionId).State = EntityState.Modified;

                    var Transporterladger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Transporter", (int)updateTransactionId.FleetMasterId, userid, db);
                    var TransporterAffectedladger = ladgerHelper.GetOrCreateLadgerTypeAndLadgerWithLedgerName("TransporterBank", 0, userid, db, "TransporterBank");
                    double paidAmount = 0;
                    if (updateTransactionId.TaxType == "FCM")
                    {
                        paidAmount = updateTransactionId.PaidAmount + updateTransactionId.CGST + updateTransactionId.SGST + updateTransactionId.IGST - updateTransactionId.TDSAmount - updateTransactionId.RemainingAmount;
                    }
                    else
                    {
                        paidAmount = updateTransactionId.PaidAmount - updateTransactionId.TDSAmount - updateTransactionId.RemainingAmount;
                    }

                    var paymentVoucherType = ladgerHelper.GetOrCreateVoucherType("Payment", userid, db);

                    LadgerEntry amountladgerEntry = new LadgerEntry
                    {
                        Active = true,
                        AffectedLadgerID = TransporterAffectedladger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = 0,
                        Date = updateTransactionId.PaymentDate,
                        Debit = paidAmount,
                        ID = 0,
                        IrPaymentDetailsId = null,
                        IsSupplierAdvancepay = null,
                        LagerID = Transporterladger.ID,
                        ObjectID = updateTransactionId.Id,
                        ObjectType = "TransporterPayment",
                        ParentId = null,
                        Particulars = "",
                        PRPaymentId = null,
                        RefNo = updateTransactionId.TransactionId,
                        Remark = "",
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        UploadGUID = null,
                        VouchersNo = null,
                        VouchersTypeID = paymentVoucherType.ID
                    };
                    ladgerHelper.Createadjustladgerentry(amountladgerEntry, userid, db);
                    amountladgerEntry = Mapper.Map(amountladgerEntry).ToANew<LadgerEntry>();
                    var ladgerid = amountladgerEntry.LagerID;
                    amountladgerEntry.LagerID = amountladgerEntry.AffectedLadgerID;
                    amountladgerEntry.AffectedLadgerID = ladgerid;

                    var debitamount = amountladgerEntry.Debit;
                    amountladgerEntry.Debit = amountladgerEntry.Credit;
                    amountladgerEntry.Credit = debitamount;
                    amountladgerEntry.ID = 0;
                    ladgerHelper.Createadjustladgerentry(amountladgerEntry, userid, db);
                    TransporterPaymentActionHistory transporterPaymentActionHistory = new TransporterPaymentActionHistory
                    {
                        Action = "Paid",
                        TransporterPaymentId = updateTransactionId.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Role = "Accounts executive/Accounts associates",
                        ModifiedBy = userid,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    db.TransporterPaymentActionHistoriesDb.Add(transporterPaymentActionHistory);
                    db.Commit();
                    res = new TransporterResponseDc
                    {
                        Status = true,
                        Message = "Updated SuccesFully!! "
                    };
                    return res;
                }
                else
                {
                    res = new TransporterResponseDc
                    {
                        Status = false,
                        Message = "Not Approved By Account Lead or Already Paid!! "
                    };
                    return res;
                }
            }
        }

        #endregion

        #region For FileWork
        [HttpPost]
        [Route("ExportTallyFile")]
        public async Task<string> TallyFilegenerated(TallyFileExportDc tallyFileExportDc)
        {
            string fileUrl = string.Empty;
            TransporterPaymentManager manager = new TransporterPaymentManager();
            List<TallyFileListDc> tallyFileList = new List<TallyFileListDc>();
            tallyFileList = await TallyFileExport(tallyFileExportDc);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TallyFileExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TallyFileExport.csv";
            //DataTable dt = ListtoDataTableConverter.ToDataTable(list);
            DataTable dt1 = new DataTable();

            dt1.Columns.Add("Invoice Numbers", typeof(string));
            dt1.Columns.Add("Invoice Date", typeof(string));
            dt1.Columns.Add("Booking Date", typeof(string));
            dt1.Columns.Add("Expense Ledger", typeof(string));
            dt1.Columns.Add("Taxable Value", typeof(string));
            dt1.Columns.Add("Tax Rate", typeof(string));
            dt1.Columns.Add("CGST-2.5%", typeof(string));
            dt1.Columns.Add("SGST-2.5%", typeof(string));
            dt1.Columns.Add("IGST-5%", typeof(string));
            dt1.Columns.Add("CGST-6%", typeof(string));
            dt1.Columns.Add("SGST-6%", typeof(string));
            dt1.Columns.Add("IGST-12%", typeof(string));
            dt1.Columns.Add("CGST-9%", typeof(string));
            dt1.Columns.Add("SGST-9%", typeof(string));
            dt1.Columns.Add("IGST-18%", typeof(string));
            dt1.Columns.Add("CGST-14%", typeof(string));
            dt1.Columns.Add("SGST-14%", typeof(string));
            dt1.Columns.Add("IGST-28%", typeof(string));
            dt1.Columns.Add("TDS-194C", typeof(string));
            dt1.Columns.Add("TDS-194J Professional", typeof(string));
            dt1.Columns.Add("TDS-194J Technical", typeof(string));
            dt1.Columns.Add("TDS-194H", typeof(string));
            dt1.Columns.Add("TDS-194I", typeof(string));
            dt1.Columns.Add("RCM CGST@2.5%", typeof(string));
            dt1.Columns.Add("RCM SGST@2.5%", typeof(string));
            dt1.Columns.Add("RCM CGST@2.5% PAYABLE", typeof(string));
            dt1.Columns.Add("RCM SGST@2.5% PAYABLE", typeof(string));
            dt1.Columns.Add("RCM IGST @5%", typeof(string));
            dt1.Columns.Add("RCM IGST @5% PAYABLE", typeof(string));
            dt1.Columns.Add("TCS on Purchase", typeof(string));
            dt1.Columns.Add("Round off", typeof(string));
            dt1.Columns.Add("Net Payable Amount", typeof(string));
            dt1.Columns.Add("Cost Centre", typeof(string));
            dt1.Columns.Add("Department", typeof(string));
            dt1.Columns.Add("Vendor Name", typeof(string));
            dt1.Columns.Add("Approved By", typeof(string));
            dt1.Columns.Add("Narration", typeof(string));
            dt1.Columns.Add("Ref No.", typeof(string));
            //dt1.Columns.Add("RCM applicable", typeof(string));
            //dt1.Columns.Add("Billing Address", typeof(string));
            //dt1.Columns.Add("Cost Center Group1", typeof(string));
            //dt1.Columns.Add("Finly ref Number", typeof(string));
            //dt1.Columns.Add("Approval Date", typeof(string));

            if (tallyFileList != null && tallyFileList.Any())
            {
                foreach (var item in tallyFileList)
                {
                    DataRow workRow = dt1.NewRow();
                    workRow["Invoice Numbers"] = item.InvoiceNumbers;
                    workRow["Invoice Date"] = item.InvoiceDate;
                    workRow["Booking Date"] = item.BookingDate;
                    workRow["Expense Ledger"] = item.ExpenseLedger;
                    workRow["Taxable Value"] = item.TaxableValue;
                    workRow["Tax Rate"] = item.TaxRate;
                    workRow["CGST-2.5%"] = item.CGST25Per;
                    workRow["SGST-2.5%"] = item.SGST25per;
                    workRow["IGST-5%"] = item.IGST5per;
                    workRow["CGST-6%"] = item.CGST6per;
                    workRow["SGST-6%"] = item.SGST6per;
                    workRow["IGST-12%"] = item.IGST12per;
                    workRow["CGST-9%"] = item.CGST9per;
                    workRow["SGST-9%"] = item.SGST9per;
                    workRow["IGST-18%"] = item.IGST18per;
                    workRow["CGST-14%"] = item.CGST14per;
                    workRow["SGST-14%"] = item.SGST14per;
                    workRow["IGST-28%"] = item.IGST28per;
                    workRow["TDS-194C"] = item.TDS194C;
                    workRow["TDS-194J Professional"] = item.TDS194JProfessional;
                    workRow["TDS-194J Technical"] = item.TDS194JTechnical;
                    workRow["TDS-194H"] = item.TDS194H;
                    workRow["TDS-194I"] = item.TDS194I;
                    workRow["RCM CGST@2.5%"] = item.RCMCGST25per;
                    workRow["RCM SGST@2.5%"] = item.RCMSGST25per;
                    workRow["RCM CGST@2.5% PAYABLE"] = item.RCMCGST25PerPAYABLE;
                    workRow["RCM SGST@2.5% PAYABLE"] = item.RCMSGST25perPAYABLE;
                    workRow["RCM IGST @5%"] = item.RCMIGST5per;
                    workRow["RCM IGST @5% PAYABLE"] = item.RCMIGST5perPAYABLE;
                    workRow["TCS on Purchase"] = item.TCSonPurchase;
                    workRow["Round off"] = Math.Round(Math.Round(item.NetPayableAmount) - item.NetPayableAmount, 2);
                    workRow["Net Payable Amount"] = Math.Round(item.NetPayableAmount);
                    workRow["Cost Centre"] = item.CostCentre;
                    workRow["Department"] = item.Department;
                    workRow["Vendor Name"] = item.VendorName;
                    // workRow["Approved By"] = item.VendorName;
                    workRow["Narration"] = $"Invoice No:- {item.InvoiceNumbers}  Date:- {item.InvoiceDate} Being Delivery Logistics Expenses For The Month Of {item.TransporterMonthlyDate.ToString("MMMM")} - {item.TransporterMonthlyDate.Year.ToString()} For {item.WarehouseName}";
                    workRow["Ref No."] = item.RefNo;
                    //workRow["RCM applicable"] = item.RCMapplicable;
                    //workRow["Billing Address"] = item.BillingAddress;
                    //workRow["Cost Center Group1"] = item.CostCenterGroup1;
                    //workRow["Finly ref Number"] = item.FinlyrefNumber;
                    //workRow["Approval Date"] = item.ApprovalDate;

                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";

        }

        private async Task<List<TallyFileListDc>> TallyFileExport(TallyFileExportDc tallyFileExportDc)
        {
            using (var context = new AuthContext())
            {
                List<TallyFileListDc> result = new List<TallyFileListDc>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var PaymentIdIdDts = new DataTable();
                PaymentIdIdDts.Columns.Add("IntValue");

                if (tallyFileExportDc.paymentId != null && tallyFileExportDc.paymentId.Any())
                {
                    foreach (var item in tallyFileExportDc.paymentId)
                    {
                        var dr = PaymentIdIdDts.NewRow();
                        dr["IntValue"] = item;
                        PaymentIdIdDts.Rows.Add(dr);
                    }
                }
                var PaymentIdparam = new SqlParameter("paymentId", PaymentIdIdDts);
                PaymentIdparam.SqlDbType = SqlDbType.Structured;
                PaymentIdparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[Operation].[TransporterPaymentTallyFileGetExport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(PaymentIdparam);

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context).ObjectContext.Translate<TallyFileListDc>(reader).ToList();

                return result;
            }
        }

        [HttpPost]
        [Route("ExportPaymentFileV2")]
        public async Task<string> ExportPaymentFileV2(RegionalAllWHInput input)
        {
            string fileUrl = string.Empty;
            TransporterPaymentManager manager = new TransporterPaymentManager();
            List<PaymentListDc> PaymentList = new List<PaymentListDc>();
            PaymentList = await manager.ExportPaymentFile(input);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.xls";
            DataTable dt1 = new DataTable();

            //dt1.Columns.Add("Payment", typeof(string));
            //dt1.Columns.Add("Payment order", typeof(string));
            //dt1.Columns.Add("Payment order name", typeof(string));
            dt1.Columns.Add("Invoice", typeof(string));
            dt1.Columns.Add("Beneficiary name", typeof(string));
            dt1.Columns.Add("Beneficiary account name", typeof(string));
            dt1.Columns.Add("Beneficiary account number", typeof(string));
            dt1.Columns.Add("Beneficiary ifsc", typeof(string));
            dt1.Columns.Add("Payment mode", typeof(string));
            dt1.Columns.Add("Initiator email", typeof(string));
            dt1.Columns.Add("Invoice number", typeof(string));
            dt1.Columns.Add("Invoice date", typeof(string));

            dt1.Columns.Add("Net payable amount", typeof(string));
            dt1.Columns.Add("Bank reference number", typeof(string));
            dt1.Columns.Add("Payment status", typeof(string));
            dt1.Columns.Add("HubName", typeof(string));

            if (PaymentList != null && PaymentList.Any())
            {
                foreach (var item in PaymentList)
                {
                    DataRow workRow = dt1.NewRow();
                    //workRow["Payment"] = item.Payment;
                    workRow["Beneficiary name"] = item.Beneficiaryname;
                    workRow["Beneficiary account name"] = item.Beneficiaryaccountname;
                    workRow["Beneficiary account number"] = item.Beneficiaryaccountnumber.ToString();
                    workRow["Beneficiary ifsc"] = item.Beneficiaryifsc;
                    workRow["Payment mode"] = item.Paymentmode;
                    workRow["Initiator email"] = item.Initiatoremail;
                    workRow["Invoice number"] = item.Invoicenumber;
                    workRow["Invoice date"] = item.Invoicedate;
                    workRow["Invoice"] = item.Payment;
                    workRow["Net payable amount"] = Math.Round(item.Netpayableamount);
                    workRow["Bank reference number"] = "";
                    workRow["Payment status"] = "";
                    workRow["HubName"] = item.HubName;

                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            ExcelGenerator.DataTable_To_Excel(dt1, "sheet1", path);
            //dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";

        }


        [HttpPost]
        [Route("ExportPaymentFileV3")]
        public async Task<string> ExportPaymentFileV3(TallyFileExportDc input)
        {
            string fileUrl = string.Empty;
            List<PaymentListDc> PaymentList = new List<PaymentListDc>();
            PaymentList = await PaymentFileExport(input);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.xls";
            DataTable dt1 = new DataTable();

            //dt1.Columns.Add("Payment", typeof(string));
            //dt1.Columns.Add("Payment order", typeof(string));
            //dt1.Columns.Add("Payment order name", typeof(string));
            dt1.Columns.Add("Invoice", typeof(string));
            dt1.Columns.Add("Beneficiary name", typeof(string));
            dt1.Columns.Add("Beneficiary account name", typeof(string));
            dt1.Columns.Add("Beneficiary account number", typeof(string));
            dt1.Columns.Add("Beneficiary ifsc", typeof(string));
            dt1.Columns.Add("Payment mode", typeof(string));
            dt1.Columns.Add("Initiator email", typeof(string));
            dt1.Columns.Add("Invoice number", typeof(string));
            dt1.Columns.Add("Invoice date", typeof(string));

            dt1.Columns.Add("Net payable amount", typeof(string));
            dt1.Columns.Add("Bank reference number", typeof(string));
            dt1.Columns.Add("Payment status", typeof(string));
            dt1.Columns.Add("HubName", typeof(string));

            if (PaymentList != null && PaymentList.Any())
            {
                foreach (var item in PaymentList)
                {
                    DataRow workRow = dt1.NewRow();
                    //workRow["Payment"] = item.Payment;
                    workRow["Beneficiary name"] = item.Beneficiaryname;
                    workRow["Beneficiary account name"] = item.Beneficiaryaccountname;
                    workRow["Beneficiary account number"] = item.Beneficiaryaccountnumber.ToString();
                    workRow["Beneficiary ifsc"] = item.Beneficiaryifsc;
                    workRow["Payment mode"] = item.Paymentmode;
                    workRow["Initiator email"] = item.Initiatoremail;
                    workRow["Invoice number"] = item.Invoicenumber;
                    workRow["Invoice date"] = item.Invoicedate;
                    workRow["Invoice"] = item.Payment;
                    workRow["Net payable amount"] = Math.Round(item.Netpayableamount);
                    workRow["Bank reference number"] = "";
                    workRow["Payment status"] = "";
                    workRow["HubName"] = item.HubName;

                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            ExcelGenerator.DataTable_To_Excel(dt1, "sheet1", path);
            //dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";

        }

        private async Task<List<PaymentListDc>> PaymentFileExport(TallyFileExportDc tallyFileExportDc)
        {
            using (var context = new AuthContext())
            {
                List<PaymentListDc> result = new List<PaymentListDc>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var PaymentIdIdDts = new DataTable();
                PaymentIdIdDts.Columns.Add("IntValue");

                if (tallyFileExportDc.paymentId != null && tallyFileExportDc.paymentId.Any())
                {
                    foreach (var item in tallyFileExportDc.paymentId)
                    {
                        var dr = PaymentIdIdDts.NewRow();
                        dr["IntValue"] = item;
                        PaymentIdIdDts.Rows.Add(dr);
                    }
                }
                var PaymentIdparam = new SqlParameter("paymentId", PaymentIdIdDts);
                PaymentIdparam.SqlDbType = SqlDbType.Structured;
                PaymentIdparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[Operation].[TransporterPayment_PaymentFileGetV3]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(PaymentIdparam);

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)context).ObjectContext.Translate<PaymentListDc>(reader).ToList();

                return result;
            }
        }

        [HttpGet]
        [Route("ExportPaymentFile")]
        public async Task<string> PaymentFilegenerated(int warehouseid, DateTime Fordate)
        {
            string fileUrl = string.Empty;
            TransporterPaymentManager manager = new TransporterPaymentManager();
            List<PaymentListDc> PaymentList = new List<PaymentListDc>();
            PaymentList = await manager.ExportPaymentFile(warehouseid, Fordate);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_PaymentFileExport.xls";
            DataTable dt1 = new DataTable();

            dt1.Columns.Add("Payment", typeof(string));
            dt1.Columns.Add("Payment order", typeof(string));
            dt1.Columns.Add("Payment order name", typeof(string));
            dt1.Columns.Add("Beneficiary name", typeof(string));
            dt1.Columns.Add("Beneficiary account name", typeof(string));
            dt1.Columns.Add("Beneficiary account number", typeof(string));
            dt1.Columns.Add("Beneficiary ifsc", typeof(string));
            dt1.Columns.Add("Payment mode", typeof(string));
            dt1.Columns.Add("Initiator email", typeof(string));
            dt1.Columns.Add("Invoice number", typeof(string));
            dt1.Columns.Add("Invoice date", typeof(string));
            dt1.Columns.Add("Invoice", typeof(string));
            dt1.Columns.Add("Net payable amount", typeof(string));
            dt1.Columns.Add("Bank reference number", typeof(string));
            dt1.Columns.Add("Payment status", typeof(string));
            dt1.Columns.Add("HubName", typeof(string));

            if (PaymentList != null && PaymentList.Any())
            {
                foreach (var item in PaymentList)
                {
                    DataRow workRow = dt1.NewRow();
                    workRow["Payment"] = item.Payment;
                    workRow["Beneficiary name"] = item.Beneficiaryname;
                    workRow["Beneficiary account name"] = item.Beneficiaryaccountname;
                    workRow["Beneficiary account number"] = item.Beneficiaryaccountnumber.ToString();
                    workRow["Beneficiary ifsc"] = item.Beneficiaryifsc;
                    workRow["Payment mode"] = item.Paymentmode;
                    workRow["Initiator email"] = item.Initiatoremail;
                    workRow["Invoice number"] = item.Invoicenumber;
                    workRow["Invoice date"] = item.Invoicedate;
                    workRow["Invoice"] = item.Invoice;
                    workRow["Net payable amount"] = Math.Round(item.Netpayableamount);
                    workRow["Bank reference number"] = "";
                    workRow["Payment status"] = "";
                    workRow["HubName"] = item.HubName;

                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            ExcelGenerator.DataTable_To_Excel(dt1, "sheet1", path);
            //dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";

        }

        [HttpPost]
        [Route("UploadTransporterDocument")]
        public IHttpActionResult LogbookImage()
        {

            string url = "";

            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    //var httpPostedFile2 = HttpContext.Current.Request.Files["file2"];
                    //var httpPostedFile3 = HttpContext.Current.Request.Files["file3"];
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedTransporterDOC")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedTransporterDOC"));
                    if (httpPostedFile != null)
                    {



                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedTransporterDOC"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/UploadedTransporterDOC", LogoUrl);
                        //urlList = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                        //                                       , HttpContext.Current.Request.Url.DnsSafeHost
                        //                                       , HttpContext.Current.Request.Url.Port
                        //                                           , string.Format("UploadedTransporterDOC/{0}", fileName));

                        url = string.Format("UploadedTransporterDOC/{0}", fileName);

                        // LogoUrl = "/UploadedTransporterDOC/" + fileName;
                        //LogoUrl;
                    }


                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in UploadedTransporterDOC Method: " + ex.Message);
            }

            return Created(url, url);

            //return LogoUrl;
        }

        [HttpPost]
        [Route("UploadedInvoice")]
        public IHttpActionResult uploadInvoice()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedInvoice")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedInvoice"));
                    if (httpPostedFile != null)
                    {



                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedInvoice"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/UploadedInvoice", LogoUrl);
                        LogoUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port
                                                                   , string.Format("UploadedInvoice/{0}", fileName));

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in UploadedTransporterDOC Method: " + ex.Message);
            }
            return Created<string>(LogoUrl, LogoUrl);

        }
        #endregion

        #region For AllRoles
        [HttpGet]
        [Route("GetDocList")]
        public async Task<List<TransporterPaymentDetailDocDc>> GetDocList(int TransporterPayDetailId)
        {
            using (var db = new AuthContext())
            {
                List<TransporterPaymentDetailDocDc> transporterPaymentDetailDocDcs = db.TransporterPaymentDetailDocs
                     .Where(x => x.TransporterPaymentDetailId == TransporterPayDetailId && x.IsActive == true && x.IsDeleted == false)
                     .Select(y => new TransporterPaymentDetailDocDc
                     {
                         TransporterPaymentDetailId = y.TransporterPaymentDetailId,
                         DocPath = y.DocPath,
                         DocType = y.DocType,
                         TransporterPaymentDetailDocId = y.Id
                     }).ToList();
                return transporterPaymentDetailDocDcs;
            }

        }

        [Route("GetTransporterPaymentHistoryList")]
        public async Task<List<TransporterPaymentHistoryListDc>> GetTransporterPaymentHistoryList(int paymentId)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetTransporterPaymentHistoryList(paymentId);
            return list;
        }

        [HttpGet]
        [Route("RegionalExport")]
        public async Task<string> RegionalExport(int Warehouseid, DateTime ForDate)
        {
            string fileUrl = string.Empty;
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var ExportList = await manager.GetRegionalList(Warehouseid, ForDate);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RegionalListExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RegionalListExport.csv";
            DataTable dt1 = new DataTable();

            dt1.Columns.Add("SystemId", typeof(string));
            dt1.Columns.Add("InvoiceNumber", typeof(string));
            dt1.Columns.Add("InvoiceDate", typeof(string));
            dt1.Columns.Add("TransportertName", typeof(string));
            dt1.Columns.Add("UtilizedAmount", typeof(string));
            dt1.Columns.Add("ExtraKmAmount", typeof(string));
            dt1.Columns.Add("TollAmount", typeof(string));
            dt1.Columns.Add("OtherCharges", typeof(string));
            dt1.Columns.Add("TaxableAmount", typeof(string));
            dt1.Columns.Add("TaxType", typeof(string));
            dt1.Columns.Add("CGST", typeof(string));
            dt1.Columns.Add("SGST", typeof(string));
            dt1.Columns.Add("IGST", typeof(string));
            dt1.Columns.Add("TDS", typeof(string));
            dt1.Columns.Add("NetPayable", typeof(string));
            dt1.Columns.Add("RemainingAmount", typeof(string));
            dt1.Columns.Add("FleetType", typeof(string));
            dt1.Columns.Add("ApprovalStatus", typeof(string));
            dt1.Columns.Add("PaymentStatus", typeof(string));
            dt1.Columns.Add("UTRNumber", typeof(string));

            if (ExportList != null && ExportList.Any())
            {
                foreach (var item in ExportList)
                {
                    DataRow workRow = dt1.NewRow();
                    workRow["SystemId"] = item.PaymentId;
                    workRow["InvoiceNumber"] = item.InvoiceNumber;
                    workRow["InvoiceDate"] = item.InvoiceDate;
                    workRow["TransportertName"] = item.TransportertName;
                    workRow["UtilizedAmount"] = item.UtilizedAmount;
                    workRow["ExtraKmAmount"] = item.ExtraKmAmount;
                    workRow["TollAmount"] = item.TollAmount;
                    workRow["OtherCharges"] = item.OtherCharges;
                    workRow["TaxableAmount"] = item.TaxableAmount;
                    workRow["TaxType"] = item.TaxType;
                    workRow["CGST"] = item.CGST;
                    workRow["SGST"] = item.SGST;
                    workRow["IGST"] = item.IGST;
                    workRow["TDS"] = item.TDS;
                    workRow["NetPayable"] = item.TotalPayable;
                    workRow["RemainingAmount"] = item.RemainingAmount;
                    workRow["FleetType"] = item.FleetType;
                    //workRow["ApprovalStatus"] = item.ApprovalStatus;
                    //workRow["PaymentStatus"] = item.PaymentStatus;
                    workRow["UTRNumber"] = item.TransactionId;

                    switch (item.ApprovalStatus)
                    {
                        case (int)TransporterPaymentApprovalStatusEnum.ApporvedByAccount:
                            workRow["ApprovalStatus"] = "ApporvedByAccount";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByAccountLead:
                            workRow["ApprovalStatus"] = "ApprovedByAccountLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByHQOpsLead:
                            workRow["ApprovalStatus"] = "ApprovedByHQOpsLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional:
                            workRow["ApprovalStatus"] = "ApprovedByRegional";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.Finalized:
                            workRow["ApprovalStatus"] = "Finalized";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.Pending:
                            workRow["ApprovalStatus"] = "Pending";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount:
                            workRow["ApprovalStatus"] = "RejectedByAccount";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByAccountLead:
                            workRow["ApprovalStatus"] = "RejectedByAccountLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead:
                            workRow["ApprovalStatus"] = "RejectedByHQOpsLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional:
                            workRow["ApprovalStatus"] = "RejectedByRegional";
                            break;
                    }
                    switch (item.PaymentStatus)
                    {
                        case (int)TransporterPaymentPaymentStatusEnum.Pending:
                            workRow["PaymentStatus"] = "Pending";
                            break;
                        case (int)TransporterPaymentPaymentStatusEnum.PaymentPending:
                            workRow["PaymentStatus"] = "PaymentPending";
                            break;
                        case (int)TransporterPaymentPaymentStatusEnum.PaymentDone:
                            workRow["PaymentStatus"] = "PaymentDone";
                            break;
                    }
                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";
        }

        [HttpGet]
        [Route("RegionalExportV2")]
        public async Task<string> RegionalExportV2(RegionalAllWHInput input)
        {
            string fileUrl = string.Empty;
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var ExportList = await manager.GetRegionalListV2(input);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RegionalListExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RegionalListExport.csv";
            DataTable dt1 = new DataTable();

            dt1.Columns.Add("SystemId", typeof(string));
            dt1.Columns.Add("InvoiceNumber", typeof(string));
            dt1.Columns.Add("InvoiceDate", typeof(string));
            dt1.Columns.Add("TransportertName", typeof(string));
            dt1.Columns.Add("UtilizedAmount", typeof(string));
            dt1.Columns.Add("ExtraKmAmount", typeof(string));
            dt1.Columns.Add("TollAmount", typeof(string));
            dt1.Columns.Add("OtherCharges", typeof(string));
            dt1.Columns.Add("TaxableAmount", typeof(string));
            dt1.Columns.Add("TaxType", typeof(string));
            dt1.Columns.Add("CGST", typeof(string));
            dt1.Columns.Add("SGST", typeof(string));
            dt1.Columns.Add("IGST", typeof(string));
            dt1.Columns.Add("TDS", typeof(string));
            dt1.Columns.Add("NetPayable", typeof(string));
            dt1.Columns.Add("RemainingAmount", typeof(string));
            dt1.Columns.Add("FleetType", typeof(string));
            dt1.Columns.Add("ApprovalStatus", typeof(string));
            dt1.Columns.Add("PaymentStatus", typeof(string));
            dt1.Columns.Add("UTRNumber", typeof(string));

            if (ExportList != null && ExportList.Any())
            {
                foreach (var item in ExportList)
                {
                    DataRow workRow = dt1.NewRow();
                    workRow["SystemId"] = item.PaymentId;
                    workRow["InvoiceNumber"] = item.InvoiceNumber;
                    workRow["InvoiceDate"] = item.InvoiceDate;
                    workRow["TransportertName"] = item.TransportertName;
                    workRow["UtilizedAmount"] = item.UtilizedAmount;
                    workRow["ExtraKmAmount"] = item.ExtraKmAmount;
                    workRow["TollAmount"] = item.TollAmount;
                    workRow["OtherCharges"] = item.OtherCharges;
                    workRow["TaxableAmount"] = item.TaxableAmount;
                    workRow["TaxType"] = item.TaxType;
                    workRow["CGST"] = item.CGST;
                    workRow["SGST"] = item.SGST;
                    workRow["IGST"] = item.IGST;
                    workRow["TDS"] = item.TDS;
                    workRow["NetPayable"] = item.TotalPayable;
                    workRow["RemainingAmount"] = item.RemainingAmount;
                    workRow["FleetType"] = item.FleetType;
                    //workRow["ApprovalStatus"] = item.ApprovalStatus;
                    //workRow["PaymentStatus"] = item.PaymentStatus;
                    workRow["UTRNumber"] = item.TransactionId;

                    switch (item.ApprovalStatus)
                    {
                        case (int)TransporterPaymentApprovalStatusEnum.ApporvedByAccount:
                            workRow["ApprovalStatus"] = "ApporvedByAccount";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByAccountLead:
                            workRow["ApprovalStatus"] = "ApprovedByAccountLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByHQOpsLead:
                            workRow["ApprovalStatus"] = "ApprovedByHQOpsLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.ApprovedByRegional:
                            workRow["ApprovalStatus"] = "ApprovedByRegional";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.Finalized:
                            workRow["ApprovalStatus"] = "Finalized";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.Pending:
                            workRow["ApprovalStatus"] = "Pending";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount:
                            workRow["ApprovalStatus"] = "RejectedByAccount";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByAccountLead:
                            workRow["ApprovalStatus"] = "RejectedByAccountLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead:
                            workRow["ApprovalStatus"] = "RejectedByHQOpsLead";
                            break;
                        case (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional:
                            workRow["ApprovalStatus"] = "RejectedByRegional";
                            break;
                    }
                    switch (item.PaymentStatus)
                    {
                        case (int)TransporterPaymentPaymentStatusEnum.Pending:
                            workRow["PaymentStatus"] = "Pending";
                            break;
                        case (int)TransporterPaymentPaymentStatusEnum.PaymentPending:
                            workRow["PaymentStatus"] = "PaymentPending";
                            break;
                        case (int)TransporterPaymentPaymentStatusEnum.PaymentDone:
                            workRow["PaymentStatus"] = "PaymentDone";
                            break;
                    }
                    dt1.Rows.Add(workRow);
                }
            }

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt1.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";
        }


        #endregion

        #region Vehicle Add In Transporter

        [HttpGet]
        [Route("GetTranspoterPaymentVehicleList")]
        public async Task<List<TransporterPaymentVehicleList>> GetTranspoterPaymentVehicleList(long TranspoterPaymentId)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetTranspoterPaymentVehicleList(TranspoterPaymentId);
            return list;
        }

        [HttpGet]
        [Route("GetTransporterPayVehicleInfo")]
        public async Task<TransporterPayVehicleInfo> GetTransporterPayVehicleInfo(long VehicleMasterId)
        {
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var list = await manager.GetTransporterPayVehicleInfo(VehicleMasterId);
            return list;
        }

        [HttpPost]
        [Route("PaymentDetailVehicleInsert")]
        public async Task<ResultViewModel<long>> PaymentDetailVehicleInsert(TransporterPayDetailVehicleInsertInput input)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            TransporterPaymentManager manager = new TransporterPaymentManager();
            var vehicleinfo = await manager.GetTransporterPayVehicleInfo(input.VehicleMasterId);
            using (var context = new AuthContext())
            {
                var PaymentStatus = context.TransporterPaymentDb.Where(x => x.Id == input.TransporterPaymentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (PaymentStatus != null && (PaymentStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.Pending
                    || PaymentStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByRegional
                    || PaymentStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByAccount
                    || PaymentStatus.ApprovalStatus == (int)TransporterPaymentApprovalStatusEnum.RejectedByHQOpsLead
                ))
                {
                    var IdExsist = context.TransporterPaymentDetailDb.Where(x => x.Id == input.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (IdExsist != null)
                    {
                        IdExsist.TransporterPaymentId = input.TransporterPaymentId;
                        IdExsist.VehicleMasterId = input.VehicleMasterId;
                        IdExsist.UtilizedKm = input.UtilizedKm;
                        IdExsist.UtilizedAmount = input.UtilizedAmount;
                        IdExsist.TollAmount = input.TollAmount;
                        IdExsist.OtherExpense = input.OtherExpense;
                        IdExsist.ExtraKm = input.ExtraKm;
                        IdExsist.ExtraKmAmt = input.ExtraKmAmt;
                        IdExsist.PayableAmount = input.UtilizedAmount + input.OtherExpense + input.TollAmount + input.ExtraKmAmt;
                        IdExsist.ModifiedBy = userid;
                        IdExsist.ModifiedDate = DateTime.Now;

                        context.Entry(IdExsist).State = EntityState.Modified;
                        context.Commit();

                        return new ResultViewModel<long>
                        {
                            ErrorMessage = "",
                            IsSuceess = true,
                            ResultItem = IdExsist.Id,
                            SuccessMessage = "Updated Successfullt"
                        };
                    }
                    else
                    {
                        var ExsistVehicle = context.TransporterPaymentDetailDb.Where(x => x.TransporterPaymentId == input.TransporterPaymentId && x.VehicleMasterId == input.VehicleMasterId
                        && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                        if (ExsistVehicle == null)
                        {
                            TransporterPaymentDetail paymentDetail = new TransporterPaymentDetail
                            {
                                ExtraKm = input.ExtraKm,
                                ExtraKmAmt = input.ExtraKmAmt,
                                OtherExpense = input.OtherExpense,
                                TollAmount = input.TollAmount,
                                UtilizedAmount = input.UtilizedAmount,
                                UtilizedKm = input.UtilizedKm,
                                TransporterPaymentId = input.TransporterPaymentId,
                                IsActive = true,
                                IsDeleted = false,
                                MonthlyContractAmount = vehicleinfo.MonthlyContactAmt.Value,
                                MonthlyContractKm = vehicleinfo.MonthlyContactAmt.Value,
                                IsManuallyEdit = true,
                                ApprovalStatus = 1,
                                VehicleMasterId = input.VehicleMasterId,
                                RegionalComment = "",
                                WhLeadComment = "",
                                PayableAmount = input.UtilizedAmount + input.OtherExpense + input.TollAmount + input.ExtraKmAmt,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now
                            };
                            context.TransporterPaymentDetailDb.Add(paymentDetail);
                            context.Commit();
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = "",
                                IsSuceess = true,
                                ResultItem = paymentDetail.Id,
                                SuccessMessage = "Added Successfullt"
                            };
                        }
                        else
                        {
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = "Vehicle already exists in this payment",
                                IsSuceess = false,
                                ResultItem = 0,
                                SuccessMessage = ""
                            };
                        }
                    }
                }
                else
                {
                    return new ResultViewModel<long>
                    {
                        ErrorMessage = "Cannot edit this payment",
                        IsSuceess = false,
                        ResultItem = 0,
                        SuccessMessage = ""
                    };
                }
            }
        }


        [HttpGet]
        [Route("TransporterPayDetailList")]
        public async Task<TransporterDetailListDc> TransporterPayDetailList(int Id)
        {
            TransporterDetailListDc transporterDetailListDc = new TransporterDetailListDc();

            using (var context = new AuthContext())
            {
                var result = context.TransporterPaymentDetailDb.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (result != null)
                {
                    transporterDetailListDc = new TransporterDetailListDc
                    {
                        Id = result.Id,
                        VehicleMasterId = result.VehicleMasterId,
                        TransporterPaymentId = result.TransporterPaymentId,
                        ExtraKm = result.ExtraKm,
                        ExtraKmAmt = result.ExtraKmAmt,
                        MonthlyContactAmt = result.MonthlyContractAmount,
                        MonthlyContactKm = result.MonthlyContractKm,
                        UtilizedAmount = result.UtilizedAmount,
                        UtilizedKm = result.UtilizedKm,
                        OtherExpense = result.OtherExpense,
                        PayableAmount = result.PayableAmount,
                        TollAmount = result.TollAmount
                    };
                }
            }
            return transporterDetailListDc;
        }

        #endregion

        private async Task<string> GenerateInvoicePartial(int paymentid, DateTime startdate)
        {
            string returnfile = "";
            TransporterPaymentManager manager = new TransporterPaymentManager();
            var InvoiceData = await manager.GenerateInvoice(paymentid, startdate);
            var totalamount = InvoiceData.Amount + InvoiceData.OtherCharges + InvoiceData.ExtraKmCharges;
            bool IsIGSTApplied = false;
            double TaxableAmount = totalamount;
            if (InvoiceData.IsRCMTax == "Yes")
            {

            }
            else //FCM
            {
                totalamount = InvoiceData.CGST + InvoiceData.SGST + InvoiceData.IGST + totalamount;
            }
            //if (InvoiceData.IGST > 0)
            //{
            //    IsIGSTApplied = true;
            //    TaxableAmount = InvoiceData.IGST;
            //}
            //else
            //{
            //    TaxableAmount = InvoiceData.CGST + InvoiceData.SGST;
            //}
            //totalamount = totalamount - InvoiceData.TDSAmount;
            string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/TransporterInvoice.html";
            string content = File.ReadAllText(pathToHTMLFile);
            if (!string.IsNullOrEmpty(content))
            {
                var amt = Math.Round(totalamount);
                var AmountInWord = NumberToWordsHelper.ConvertAmount((int)amt);
                content = content.Replace("@InvoiceNo", InvoiceData.InvoiceNumber.ToString());
                content = content.Replace("@InvoiceDate", InvoiceData.InvoiceDate.ToString("dd/MM/yyyy"));
                content = content.Replace("@TransporterAddress", InvoiceData.TransporterAddress);
                content = content.Replace("@TransporterCity", InvoiceData.TransporterCity);
                content = content.Replace("@TransporterState", InvoiceData.TranporterState);
                content = content.Replace("@TransporterPAN", InvoiceData.TransporterPanNumber);
                content = content.Replace("@TransporterGSTIN", InvoiceData.TransporterGSTIN);
                content = content.Replace("@TransporterRCM", InvoiceData.IsRCMTax);
                content = content.Replace("@VendorName", InvoiceData.VendorName);
                content = content.Replace("@HubAddress", InvoiceData.HubAddress);
                content = content.Replace("@HubState", InvoiceData.HubState);
                content = content.Replace("@HubPAN", InvoiceData.HubPanNo);
                content = content.Replace("@HubGSTIN", InvoiceData.HubGSTIN);
                content = content.Replace("@TransporterName", InvoiceData.TransporterName);
                content = content.Replace("@UtilAmount", InvoiceData.Amount.ToString());
                content = content.Replace("@ExtraKmCharges", InvoiceData.ExtraKmCharges.ToString());
                content = content.Replace("@OtherCharges", InvoiceData.OtherCharges.ToString());
                content = content.Replace("@TaxableAmount", TaxableAmount.ToString());
                content = content.Replace("@TotalAmountNumber", ((int)amt).ToString());
                content = content.Replace("@TotalAmountInWords", AmountInWord);
                if (InvoiceData.IsRCMTax == "Yes")
                {
                    content = content.Replace("@TaxRowsForFCM", "");//Rcm

                    var query = @"<tr>
                                    <td colspan='4' style='padding:3px 5px;text-align:center;'>GST @5% to be paid under RCM</td>                                
                                   </tr>";

                    query += @" <tr>
                                    <td style='padding:3px 5px;'>Tax Type</td>
                                    <td style='padding:3px 5px;'>Taxable Amount</td>
                                    <td style='padding:3px 5px;'>Rate</td>
                                    <td style='padding:3px 5px;'>Amount</td>
                                   </tr>";
                    if (InvoiceData.IGST > 0)
                    {


                        query += $@" <tr>
                                    <td style='padding:3px 5px;'>IGST</td>
                                    <td style='padding:3px 5px;'>{TaxableAmount} </td>
                                    <td style='padding:3px 5px;'> @ {InvoiceData.IGSTPer.ToString()}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.IGST.ToString()}</td>
                                   </tr>";

                    }
                    else//Fcm
                    {
                        query += $@" <tr>
                                    <td style='padding:3px 5px;'>CGST</td>
                                    <td style='padding:3px 5px;'>{TaxableAmount}</td>
                                    <td style='padding:3px 5px;'>CGST @ {InvoiceData.CGSTPer}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.CGST}</td>
                                   </tr>";

                        query += $@" <tr>
                                    <td style='padding:3px 5px;'>SGST</td>
                                    <td style='padding:3px 5px;'>{TaxableAmount}</td>
                                    <td style='padding:3px 5px;'>SGST @ {InvoiceData.SGSTPer}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.SGST}</td>
                                   </tr>";
                    }

                    content = content.Replace("@TaxRowsForRCM", query);
                }
                else
                {
                    content = content.Replace("@TaxRowsForRCM", "");

                    var query = @"";
                    if (InvoiceData.IGST > 0)
                    {


                        query += $@" <tr>
                                    <td style='padding:3px 5px;'></td>
                                    <td style='padding:3px 5px;'> </td>
                                    <td style='padding:3px 5px;'> IGST @ {InvoiceData.IGSTPer.ToString()}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.IGST.ToString()}</td>
                                   </tr>";

                    }
                    else
                    {
                        query += $@" <tr>
                                    <td style='padding:3px 5px;'></td>
                                    <td style='padding:3px 5px;'></td>
                                    <td style='padding:3px 5px;'>CGST @ {InvoiceData.CGSTPer}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.CGST}</td>
                                   </tr>";

                        query += $@" <tr>
                                    <td style='padding:3px 5px;'></td>
                                    <td style='padding:3px 5px;'></td>
                                    <td style='padding:3px 5px;'>SGST @ {InvoiceData.CGSTPer}%</td>
                                    <td style='padding:3px 5px;'>{InvoiceData.SGST}</td>
                                   </tr>";
                    }

                    content = content.Replace("@TaxRowsForFCM", query);

                }

                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/TransporterInvoicepdf")))
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/TransporterInvoicepdf"));

                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_TransporterInvoice.pdf";

                var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/TransporterInvoicepdf"), fileName);

                byte[] pdf = null;

                pdf = Pdf
                      .From(content)
                      .OfSize(PaperSize.A4)
                      .WithTitle("Invoice")
                      .WithoutOutline()
                      .WithMargins(PaperMargins.All(0.0.Millimeters()))
                      .Portrait()
                      .Comressed()
                      .Content();
                FileStream file = File.Create(OutPutFile);
                file.Write(pdf, 0, pdf.Length);
                file.Close();

                returnfile = "/TransporterInvoicepdf/" + fileName;
            }

            return returnfile;
        }

        [HttpGet]
        [Route("GenerateInvoice")]
        public async Task<string> GenerateInvoice(int paymentid, DateTime startdate)
        {
            return await GenerateInvoicePartial(paymentid, startdate);
        }

        #region Document

        [HttpGet]
        [Route("TransporterPaymentDocument")]
        public async Task<string> TransporterDocument(DateTime month)
        {

            Regex rgx = new Regex("[^a-zA-Z0-9]");

            List<TransporterDocumentDc> transporterDocData = new List<TransporterDocumentDc>();
            using (var context = new AuthContext())
            {
                var Param = new SqlParameter
                {
                    ParameterName = "Month",
                    Value = month
                };
                transporterDocData = context.Database.SqlQuery<TransporterDocumentDc>("EXEC  dbo.TransporterPaymentDocGet @Month", Param).ToList();

                string PaymentDoc = HttpContext.Current.Server.MapPath("~/PaymentDocs/");

                if (!Directory.Exists(PaymentDoc))
                {
                    Directory.CreateDirectory(PaymentDoc);
                }
                else
                {
                    Directory.Delete(PaymentDoc, true);
                    Directory.CreateDirectory(PaymentDoc);
                }
                string[] files = Directory.GetFiles(PaymentDoc);

                if (transporterDocData != null)
                {
                    foreach (var item in transporterDocData)
                    {
                        item.TransportName = rgx.Replace(item.TransportName, "");

                        string TransportNamepath = HttpContext.Current.Server.MapPath("~/PaymentDocs/" + item.TransportName);  
                        
                        if (!Directory.Exists(TransportNamepath))
                        {
                            Directory.CreateDirectory(TransportNamepath);
                        }
                        if(item.GeneratedInvoicePath !=null)
                        {
                            string SourcePath = HttpContext.Current.Server.MapPath("~" + item.GeneratedInvoicePath);
                            string DestinationPath = HttpContext.Current.Server.MapPath("~/PaymentDocs/" + item.TransportName + "/"+ item.GeneratedInvoicePath.Split('/').ToList().Last());

                            if (!File.Exists(DestinationPath) && File.Exists(SourcePath))
                            {
                                File.Copy(SourcePath, DestinationPath);
                            }
                        }

                        if(item.TransporterInvoicePath != null)
                        {
                            string SourcePath1 = HttpContext.Current.Server.MapPath("~/" + item.TransporterInvoicePath);
                            string DestinationPath1 = HttpContext.Current.Server.MapPath("~/PaymentDocs/" + item.TransportName + "/" + item.TransporterInvoicePath.Split('/').ToList().Last());

                            if (!File.Exists(DestinationPath1) && File.Exists(SourcePath1))
                            {
                                File.Copy(SourcePath1, DestinationPath1);
                            }
                        }

                        #region For Detail Docs

                        var Param1 = new SqlParameter
                        {
                            ParameterName = "TransporterPaymentId",
                            Value = item.TransporterId
                        };
                        var TransporterPayDetaildoc = context.Database.SqlQuery<string>("EXEC  dbo.TransporterPaymentDetailDocGet @TransporterPaymentId", Param1).ToList();

                        foreach (var Detaildoc in TransporterPayDetaildoc)
                        {
                            if (Detaildoc != null)
                            {
                                string SourcePath = HttpContext.Current.Server.MapPath("~/" + Detaildoc);
                                string DestinationPath = HttpContext.Current.Server.MapPath("~/PaymentDocs/" + item.TransportName + "/" + Detaildoc.Split('/').ToList().Last());

                                if (!File.Exists(DestinationPath) && File.Exists(SourcePath))
                                {
                                    File.Copy(SourcePath, DestinationPath);
                                }
                            }
                        }

                        #endregion
                    }
                }
            }
            return "";
        }

        #endregion

    }
}

