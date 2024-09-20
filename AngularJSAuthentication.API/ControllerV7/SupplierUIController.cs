using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction.supplier;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/SupplierLedger")]
    public class SupplierUIController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("GRListLedgerInsert/year/{year}/month/{month}/day/{day}")]
        public IHttpActionResult POLedgerInsert(int year, int month, int day)
        {
            //SupplierLedgerHelper.GRList(month, year, day);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("IRListLedgerInsert/year/{year}/month/{month}/day/{day}")]
        public IHttpActionResult IRLedgerInsert(int year, int month, int day)
        {
            //SupplierLedgerHelper.IRList(month, year, day);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateAllLedger")]
        public IHttpActionResult UpdateAllLedger()
        {

            SupplierLedgerHelper.UpdateSupplierLedger();

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Excel")]
        public IHttpActionResult Excel()
        {
            ShreeAmbikaLedgerHelper.InsertFromExcel();
            //SupplierLedgerHelper.GRList(month, year, day);
            return Ok();
        }


        [Route("GetByName/name/{name}")]
        [HttpGet]
        public IHttpActionResult GetByName(string name)
        {
            using (var authContext = new AuthContext())
            {
                //var ledgerTypeList = authContext.LadgerTypeDB.ToList();

                var query = from sup in authContext.Suppliers
                            where (sup.Active==true && sup.Deleted==false && (sup.SUPPLIERCODES.ToLower().Contains(name.ToLower()) || sup.Name.ToLower().Contains(name.ToLower()) || sup.Brand.ToLower().Contains(name.ToLower())))
                            select new
                            {
                                sup.SupplierId,
                                Name = sup.Name + " - " + sup.Brand + " - " + sup.SUPPLIERCODES
                            };
                var supplierList = query.Take(50).ToList();
                return Ok(supplierList);
            }
        }

        [Route("GetBySupplierName/name/{name}")]
        [HttpGet]
        public IHttpActionResult GetBySupplierName(string name)
        {
            using (var authContext = new AuthContext())
            {
                //var ledgerTypeList = authContext.LadgerTypeDB.ToList();

                var query = from lad in authContext.LadgerDB
                            join sup in authContext.Suppliers
                            on lad.ObjectID equals sup.SupplierId
                            where lad.ObjectType == "Supplier" && (sup.SUPPLIERCODES.Contains(name.ToLower()) || sup.Name.ToLower().Contains(name.ToLower()) || sup.Name.ToLower().Contains(name.ToLower()) || sup.Brand.ToLower().Contains(name.ToLower()))
                            select new
                            {
                                lad.ID,
                                Name = sup.Name + " - " + sup.Brand + " - " + sup.SUPPLIERCODES
                            };
                var supplierList = query.Take(50).ToList();
                return Ok(supplierList);
            }
        }


        [Route("GetGUID")]
        [HttpGet]
        public IHttpActionResult GetGUID()
        {
            using (var authContext = new AuthContext())
            {
                var query = authContext.LadgerEntryDB.Where(x => x.UploadGUID != null).Select(x => x.UploadGUID).Distinct().ToList();
                return Ok(query);
            }
        }

        [Route("GetManualLadger")]
        [HttpGet]
        public IHttpActionResult GetManualLadger()
        {
            using (var authContext = new AuthContext())
            {
                var query = authContext.LadgerEntryDB.Take(50).ToList();
                return Ok(query);
            }
        }
        [Route("Search")]
        [HttpPost]
        public IHttpActionResult Search(ManualSupplierLadger details)
        {
            using (var authContext = new AuthContext())
            {
                ManualSupplierLadgerDetailsDTO vm = new ManualSupplierLadgerDetailsDTO();
                var query = from lad in authContext.LadgerEntryDB
                            where (lad.UploadGUID != null && (details.ObjectID == 0 || (lad.LagerID == details.ObjectID)) && (details.UploadGUID == null || (lad.UploadGUID == details.UploadGUID)))
                            select lad;
                vm.TotalRecords = query.ToList().Count();
                //var list = query.ToList();
                var selectedLadgerEntryList = authContext.LadgerEntryDB.Where(lad => lad.UploadGUID != null && (details.ObjectID == 0 || (lad.LagerID == details.ObjectID)) && (details.UploadGUID == null || (lad.UploadGUID == details.UploadGUID))).OrderByDescending(x => x.ID).Skip(details.First).Take(details.Last - details.First).ToList();
                vm.LagerEntryDetailsList = selectedLadgerEntryList;
                return Ok(vm);
            }
        }

        [Route("UpdateManualLadger")]
        [HttpPost]
        public IHttpActionResult UpdateManualLadger(LadgerEntry details)
        {
            using (var authContext = new AuthContext())
            {

                LadgerEntry ladgerEntry = authContext.LadgerEntryDB.Where(x => x.ID == details.ID).FirstOrDefault();
                ladgerEntry.Particulars = details.Particulars;
                ladgerEntry.Credit = details.Credit;
                ladgerEntry.Debit = details.Debit;
                authContext.Entry(ladgerEntry).State = EntityState.Modified;
                authContext.Commit();
                return Ok();
            }
        }

        [Route("DeleteManualLadger")]
        [HttpDelete]
        public IHttpActionResult DeleteManualLadger(int id)
        {
            using (var authContext = new AuthContext())
            {

                LadgerEntry ladgerEntry = authContext.LadgerEntryDB.Where(x => x.ID == id).FirstOrDefault();
                authContext.LadgerEntryDB.Remove(ladgerEntry);
                authContext.Commit();
                return Ok();
            }
        }
        #region get supplier bank list
        /// <summary>
        /// Created Date:12/10/2019
        /// Created by raj 
        /// </summary>
        /// <returns></returns>

        [Route("SupplierBankList")]
        [HttpGet]
        public IHttpActionResult GetBankList()
        {
            using (var authContext = new AuthContext())
            {
                var query = authContext.LadgerDB.Where(x => x.LadgertypeID == 7).ToList();
                return Ok(query);
            }
        }
        #endregion
        #region get supplier payment details
        /// <summary>
        /// Created Date:15/10/2019
        /// Created by raj 
        /// </summary>
        /// <returns></returns>

        [Route("SupplierPaymentdetails")]
        [HttpGet]
        public IHttpActionResult SupplierPaymentdetails(int SupplierId)
        {
            using (var authContext = new AuthContext())
            {
                var query = authContext.IRPaymentDetailsDB.Where(x => x.SupplierId == SupplierId && x.TotalReaminingAmount != 0 && x.Deleted==false && x.IsActive==true).
                    Select(x => new { x.BankId, x.BankName, x.RefNo, x.SupplierId, x.TotalAmount, x.TotalReaminingAmount, x.Id, x.Guid }).ToList();
                return Ok(query);
            }
        }
        #endregion


        [Route("GetSupplierPaymentdetails")]
        [HttpPost]
        public SupplierPaymentListViewModel GetPaymentList(SupplierPaymentViewModel vm)
        {

            using (var context = new AuthContext())
            {
                if (vm.ToDate.HasValue)
                {
                    vm.ToDate = vm.ToDate.Value.AddDays(1).AddSeconds(-1);
                }
                SupplierPaymentListViewModel output = new SupplierPaymentListViewModel();

                //var query = from ird in context.IRPaymentDetailsDB
                //            join sup in context.Suppliers on ird.SupplierId equals sup.SupplierId
                //            join le in context.LadgerEntryDB on ird.Id equals le.IrPaymentDetailsId
                //            into gj from subpet in gj.DefaultIfEmpty()
                //            where (string.IsNullOrEmpty(vm.Contains)
                //            || (!string.IsNullOrEmpty(sup.SUPPLIERCODES) && sup.SUPPLIERCODES.ToLower().Contains(vm.Contains.ToLower()))
                //            || (!string.IsNullOrEmpty(sup.Name) && sup.Name.ToLower().Contains(vm.Contains.ToLower())))
                //            && (!vm.FromDate.HasValue || vm.ToDate.HasValue || (ird.CreatedDate >= vm.FromDate && ird.CreatedDate < vm.ToDate))
                //            && ird.IsActive == true && ird.Deleted == false
                //            group new { ird, sup, subpet }  by new { 
                //                ird.BankId, 
                //                ird.BankName,
                //                ird.CreatedDate,
                //                ird.Id,
                //                ird.IRList,
                //                ird.RefNo,
                //                ird.Remark,
                //                ird.SupplierId,
                //                ird.TotalAmount,
                //                ird.TotalReaminingAmount,
                //                sup.Name,
                //                sup.SUPPLIERCODES,
                //                ird.PaymentDate
                //            } into grouped

                //            select new IRPaymentDetailsDC
                //            {
                //                BankId = grouped.Key.BankId,
                //                BankName = grouped.Key.BankName,
                //                CreatedDate = grouped.Key.CreatedDate,
                //                Id = grouped.Key.Id,
                //                IRList = grouped.Key.IRList,
                //                RefNo = grouped.Key.RefNo,
                //                Remark = grouped.Key.Remark,
                //                SupplierId = grouped.Key.SupplierId,
                //                TotalAmount = grouped.Key.TotalAmount,
                //                TotalReaminingAmount = grouped.Key.TotalReaminingAmount,
                //                SupplierName = grouped.Key.Name,
                //                SupplierCodes = grouped.Key.SUPPLIERCODES,
                //                PaymentDate = grouped.Key.PaymentDate,
                //                IsApproved = string.IsNullOrEmpty(grouped.Min(g => (g.subpet.ObjectType))) ? false : true
                //            };

                //SupplierPaymentListViewModel output = new SupplierPaymentListViewModel();
                //output.PaymentList = query.OrderByDescending(x => x.Id).Skip(vm.Skip).Take(vm.Take).ToList();
                //foreach (var payment in output.PaymentList) {

                //    payment.IsLedger = context.LadgerEntryDB.Any(x => x.IrPaymentDetailsId == payment.Id && x.ObjectID != null) ? false : true; 
                //}

                //output.TotalRecords = query.Count();


                var query = "IRPaymentDetailGet @Keyword, @Skip, @Take, @IsGetCount";
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar) { Value = vm.Contains });
                paramList.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = vm.Skip });
                paramList.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = vm.Take });
                paramList.Add(new SqlParameter("@IsGetCount", SqlDbType.Int) { Value = 0 });
                if(vm.FromDate != null)
                {
                    paramList.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = vm.FromDate });
                    query = query + ", @FromDate";
                }
                if (vm.ToDate != null)
                {
                    paramList.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = vm.ToDate });
                    query = query + ", @ToDate";
                } 
                output.PaymentList = context.Database.SqlQuery<IRPaymentDetailsDC>(query, paramList.ToArray()).ToList();


                paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar) { Value = vm.Contains });
                paramList.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = vm.Skip });
                paramList.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = vm.Take });
                paramList.Add(new SqlParameter("@IsGetCount", SqlDbType.Int) { Value = 1 });
                if (vm.FromDate != null)
                {
                    paramList.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = vm.FromDate });
                    //query = query + ", @FromDate";
                }
                if (vm.ToDate != null)
                {
                    paramList.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = vm.ToDate });
                    //query = query + ", @ToDate";
                }
                output.TotalRecords = context.Database.SqlQuery<int>(query, paramList.ToArray()).First();
                return output;
            }
        }

        #region
        [Route("GetSupplierOutstandingamount")]
        [HttpPost]
        public SupplieroutstandingListViewModel Getsupplieroutstandingamount(SupplierOutStandingAmtViewModel vm)
        {

            using (var context = new AuthContext())
            {
                if (vm.ToDate.HasValue)
                {
                    vm.ToDate = vm.ToDate.Value.AddDays(1).AddSeconds(-1);
                }
                SupplieroutstandingListViewModel output = new SupplieroutstandingListViewModel();



                var query = from le in context.LadgerEntryDB
                            join l in context.LadgerDB
                            on le.LagerID equals l.ID
                            join sup in context.Suppliers
                            on l.ObjectID equals sup.SupplierId
                            where l.ObjectType == "Supplier"
                            && (!vm.ToDate.HasValue || le.CreatedDate <= vm.ToDate)
                            && (string.IsNullOrEmpty(vm.Contains) || sup.SUPPLIERCODES.ToLower().Contains(vm.Contains) || sup.Name.Contains(vm.Contains))
                            group new { le, sup } by new
                            {
                                Id = le.LagerID
                            }
                            into g
                            select new SupplierOutstandingamtDC
                            {
                                Credit = g.Sum(x => x.le.Credit),
                                Debit = g.Sum(x => x.le.Debit),
                                OutStandingAmt = (g.Sum(x => x.le.Debit) ?? 0) - (g.Sum(x => x.le.Credit) ?? 0),
                                SUPPLIERCODES = g.Max(x => x.sup.SUPPLIERCODES),
                                ID = g.Max(x => x.sup.SupplierId),
                                Name = g.Max(x => x.sup.Name)
                            };

                var data = query.OrderBy(x => x.Name).Skip(vm.Skip).Take(vm.Take).ToList();
                output.OutstandingList = data;
                output.TotalRecords = query.Count();


                return output;
            }
        }
        #endregion

        #region get supplier history ,irdata ,ledger entry
        /// <summary>
        /// Created date 01/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("GetHistorydata")]
        [HttpGet]
        public SupplierpaymenthistoryandIRdata supplierpaymenthistoryandIRdata(int IRPaymentdetailsId)
        {
            SupplierpaymenthistoryandIRdata irhistorydata = new SupplierpaymenthistoryandIRdata();
            using (var context = new AuthContext())
            {

                List<IRMasterdataDC> irdata = new List<IRMasterdataDC>();
                IRPaymentDetails irpaymentdetails = context.IRPaymentDetailsDB.Where(x => x.Id == IRPaymentdetailsId).FirstOrDefault();
                irhistorydata.irpaymenthistory = context.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == IRPaymentdetailsId).ToList();
                irhistorydata.ladgerEntries = context.LadgerEntryDB.Where(x => x.UploadGUID == irpaymentdetails.Guid).ToList();
                foreach (var id in irhistorydata.irpaymenthistory.Select(x => x.IRID))
                {
                    var irlistdata = context.IRMasterDB.Where(x => x.Id == id).Select(x => new { x.Id, x.IRID, x.PurchaseOrderId, x.TotalAmount, x.TotalAmountRemaining, x.IRType, x.PaymentStatus }).FirstOrDefault();
                    
                    if(irlistdata != null)
                    {
                        IRMasterdataDC irdatadc = new IRMasterdataDC();
                        irdatadc.Id = irlistdata.Id;
                        irdatadc.PurchaseOrderId = irlistdata.PurchaseOrderId;
                        irdatadc.IRID = irlistdata.IRID;
                        irdatadc.PaymentStatus = irlistdata.PaymentStatus;
                        irdatadc.IRType = irlistdata.IRType;
                        irdatadc.TotalAmountRemaining = irlistdata.TotalAmountRemaining;
                        irdatadc.TotalAmount = irlistdata.TotalAmount;
                        irdata.Add(irdatadc);
                    }
                    
                }
                irhistorydata.irmasterdatadc = irdata;
                return irhistorydata;
            }
        }
        #endregion

        #region update old assignment record in ledger table
        /// <summary>
        /// Created date 11/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("assignmentledger")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic updateassignmentledgerentry(int AssignmentId)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                AgentLedgerHelper agentledger = new AgentLedgerHelper();
                var data = agentledger.updateassignmentledgerentry(userid, AssignmentId, context);

                return data;
            }
        }
        #endregion
        #region update old  multiple assignment record in ledger table
        /// <summary>
        /// Created date 11/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("bulkassignmentledger")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic bulkupdateassignmentledgerentry(List<AssignmentListbulkDC> assignmentListbulk)
        {
            #region

            IList<int> dt = new List<int>();
            dt.Add(13267);
            dt.Add(26770);

            #endregion


            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                var data = false;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                AgentLedgerHelper agentledger = new AgentLedgerHelper();
                //foreach (var assignmentdata in assignmentListbulk)

                foreach (var assignmentdata in dt)
                {
                    try
                    {
                        int AssignmentId = Convert.ToInt32(assignmentdata);

                        data = agentledger.updateassignmentledgerentry(userid, AssignmentId, context);


                    }
                    catch (Exception ex)
                    {

                    }
                }
                return data;
            }
        }
        #endregion
        #region update  order record in ledger table
        /// <summary>
        /// Created date 11/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("orderledger")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic customerledger()
        {
            #region

            IList<int> dt = new List<int>();

            dt.Add(205203);
            dt.Add(205232);
            dt.Add(219575);
            dt.Add(223010);
            dt.Add(227473);
            dt.Add(230451);
            dt.Add(231017);
            dt.Add(231676);
            dt.Add(232826);
            dt.Add(234330);
            dt.Add(241048);
            dt.Add(244640);
            dt.Add(245453);
            dt.Add(245517);
            dt.Add(158501);
            dt.Add(223245);
            dt.Add(223785);
            dt.Add(227708);
            dt.Add(230370);
            dt.Add(232658);
            dt.Add(234679);
            dt.Add(238590);
            dt.Add(241098);
            dt.Add(241757);
            dt.Add(133819);
            dt.Add(190238);
            dt.Add(199726);
            dt.Add(211383);
            dt.Add(220222);
            dt.Add(223259);
            dt.Add(223407);
            dt.Add(224345);
            dt.Add(226572);
            dt.Add(227034);
            dt.Add(227969);
            dt.Add(235713);
            dt.Add(237075);
            dt.Add(239359);
            dt.Add(239748);
            dt.Add(241528);
            dt.Add(244147);
            dt.Add(244295);
            dt.Add(199446);
            dt.Add(206829);
            dt.Add(222651);
            dt.Add(227210);
            dt.Add(227422);
            dt.Add(231498);
            dt.Add(233664);
            dt.Add(233995);
            dt.Add(235360);
            dt.Add(237979);
            dt.Add(238577);
            dt.Add(239058);
            dt.Add(240327);
            dt.Add(240501);
            dt.Add(241683);
            dt.Add(243762);
            dt.Add(217793);
            dt.Add(217996);
            dt.Add(218002);
            dt.Add(219512);
            dt.Add(221983);
            dt.Add(223243);
            dt.Add(223673);
            dt.Add(227253);
            dt.Add(227648);
            dt.Add(227924);
            dt.Add(228104);
            dt.Add(228217);
            dt.Add(229820);
            dt.Add(231690);
            dt.Add(234524);
            dt.Add(235668);
            dt.Add(236446);
            dt.Add(239308);
            dt.Add(240156);
            dt.Add(240545);
            dt.Add(240882);
            dt.Add(244041);
            dt.Add(204846);
            dt.Add(206901);
            dt.Add(220255);
            dt.Add(220647);
            dt.Add(222084);
            dt.Add(222572);
            dt.Add(225885);
            dt.Add(227540);
            dt.Add(229352);
            dt.Add(230076);
            dt.Add(235073);
            dt.Add(235102);
            dt.Add(238662);
            dt.Add(238877);
            dt.Add(240467);
            dt.Add(240525);
            dt.Add(246826);
            dt.Add(152701);
            dt.Add(172825);
            dt.Add(220494);
            dt.Add(221339);
            dt.Add(224533);
            dt.Add(226644);
            dt.Add(227039);
            dt.Add(227309);
            dt.Add(227454);
            dt.Add(229976);
            dt.Add(231216);
            dt.Add(235188);
            dt.Add(239470);
            dt.Add(240765);
            dt.Add(241613);
            dt.Add(245138);
            dt.Add(85980);
            dt.Add(173741);
            dt.Add(217736);
            dt.Add(222466);
            dt.Add(223433);
            dt.Add(224876);
            dt.Add(225573);
            dt.Add(226177);
            dt.Add(227707);
            dt.Add(229037);
            dt.Add(229601);
            dt.Add(231050);
            dt.Add(234067);
            dt.Add(235121);
            dt.Add(235420);
            dt.Add(241993);
            dt.Add(243706);
            dt.Add(245155);
            dt.Add(246032);


            #endregion


            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                CustomerLedgerHelperAuto CUSTledger = new CustomerLedgerHelperAuto();
                //foreach (var assignmentdata in assignmentListbulk)

                foreach (var orderid in dt)
                {
                    try
                    {
                        int OrderId = Convert.ToInt32(orderid);
                        int customerId = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).Select(x => x.CustomerId).FirstOrDefault();

                        CUSTledger.OnSettle(OrderId, customerId, 183);


                    }
                    catch (Exception ex)
                    {

                    }
                }
                return true;
            }
        }
        #endregion
        #region Send Notification  for api
        /// <summary>
        /// Created date 11/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("assignmentledger")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic updateassignmentledgerentry()
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                NotificationHelper Notihelp = new NotificationHelper();
                int SupplierId = 189;
                string Notificationmessage = " GR done against your invoice number=45455 ---- dated=2020-01-10 ------";
                string smsmessage = "GR done against your invoice number=23122 ---- dated=2020-01-10  ------";
                bool sendSms = true;
                bool sendFcmNotification = true;
                string title = "GR Approved";
                var data = Notihelp.SendNotificationtoSupplier(SupplierId, Notificationmessage, smsmessage, sendSms, sendFcmNotification, title);

                return data;
            }
        }
        #endregion

        #region Send    for api
        /// <summary>
        /// Created date 11/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("SupplierPaymentReport/{irPaymentSummaryId}")]
        [HttpGet]
        public string SupplierPaymentReport(int irPaymentSummaryId)
        {

            //N,,{#ACCOUNTNO#},{#AMOUNT#},{#SUPPLERNAME#},,,,,,,,Shop Kirana E Tradin,{#SUPPLERNAME#},,,,,,,,,{#DD/MM/YYYY#},,{#IFSC#},,,{#EMAIL#}
            string bankPaymentString = AppConstants.BankPayment;

            try
            {
                using (var context = new AuthContext())
                {
                    List<IRPaymentSummaryBankReceipt> list = context.Database.SqlQuery<IRPaymentSummaryBankReceipt>("EXEC IRPaymentSummaryBankReceiptGet @IRPaymentSummaryId", new SqlParameter("IRPaymentSummaryId", irPaymentSummaryId)).ToList();

                    if (list != null && list.Any())
                    {
                        string fullBankReceipt = "";
                        foreach (var item in list)
                        {
                            string supplierCode = SupplierPaymentHelper.GetPaymentPerticular(item.SupplierCode, item.SupplierName, item.WarehouseName);
                            //string supplierCode = !string.IsNullOrEmpty(item.SupplierName) ? item.SupplierName.Trim(new char[] { '\n', '\r' }) : "";
                            supplierCode = supplierCode.Length> 20? supplierCode.Substring(0, 20): supplierCode;
                            string entry = bankPaymentString.Replace("{#ACCOUNTNO#}", !string.IsNullOrEmpty(item.AccountNumber) ? item.AccountNumber.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#AMOUNT#}", item.TotalAmount.HasValue ? ((int)item.TotalAmount).ToString().Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#SUPPLERNAME#}", !string.IsNullOrEmpty(item.SupplierName) ? item.SupplierName.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#DD/MM/YYYY#}", !string.IsNullOrEmpty(item.PaymentDate) ? item.PaymentDate.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#IFSC#}", !string.IsNullOrEmpty(item.IFSC) ? item.IFSC.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#EMAIL#}", "sourcingteam@shopkirana.com");
                            entry = entry.Replace("{#SUPPLERCODE#}", supplierCode);
                            fullBankReceipt = string.IsNullOrEmpty(fullBankReceipt) ? entry : (fullBankReceipt + "\n" + entry);
                        }


                        var FileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".txt";
                        var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);


                        var fullPhysicalPath = folderPath + "\\" + FileName;
                        var fileUrl = "/ReportDownloads" + "/" + FileName;


                        FileStream file = File.Create(fullPhysicalPath);

                        byte[] bytes = Encoding.ASCII.GetBytes(fullBankReceipt);
                        file.Write(bytes, 0, bytes.Length);
                        file.Close();
                        return fileUrl;

                    }

                }

            }
            catch (Exception ex)
            {

            }

            return null;
        }
        #endregion

        #region Create IR Ledger Entry
        [HttpGet]
        [Route("updateIRentry")]
        public void CreateIREntry() 
        {
            using (var context = new AuthContext()) {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                PutIrDTO putIrDTO = new PutIrDTO();
                var IRM = context.IRMasterDB.Where(x => x.Id == 18447).FirstOrDefault();

                putIrDTO.IrItem = null;
                putIrDTO.Id = IRM.Id;
                putIrDTO.discount = IRM.Discount ?? 0;
                putIrDTO.IRID = IRM.IRID;
                putIrDTO.IRType = null;
                putIrDTO.PurchaseOrderId = IRM.PurchaseOrderId;
                putIrDTO.OtherAmount = IRM.OtherAmount ?? 0;
                putIrDTO.OtherAmountRemark = IRM.OtherAmountRemark;
                putIrDTO.ExpenseAmount = IRM.ExpenseAmount;
                putIrDTO.ExpenseAmountRemark = IRM.ExpenseAmountRemark;
                putIrDTO.RoundofAmount = IRM.RoundofAmount ?? 0;
                putIrDTO.ExpenseAmountType = IRM.ExpenseAmountType;
                putIrDTO.OtherAmountType = IRM.OtherAmountType;
                putIrDTO.RoundoffAmountType = IRM.RoundoffAmountType;
                putIrDTO.CashDiscount = IRM.CashDiscount;

                          


                

                IRHelper.UpdateSupplierData(putIrDTO, 1, context, userid);
            }
        
        }
        #endregion

        #region Update Roundoff Ledger Entry
        [HttpGet]
        [Route("RoundOffentry")]
        [AllowAnonymous]
        public void RoundOffentry()
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    IRHelper.UpdateRoundoff();
             

            }

        }
        #endregion

        #region Bill to Bill payment
        [HttpGet]
        [Route("BillTOBillPayment")]
        [AllowAnonymous]
        public void BillTOBillPayment()
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                IRHelper.BillToBillPayment(userid);


            }

        }
        #endregion


    }

    public class ManualSupplierLadger
    {
        public string UploadGUID { get; set; }
        public int ObjectID { get; set; }
        public int First { get; set; }
        public int Last { get; set; }

    }

    public class ManualSupplierLadgerDetailsDTO
    {
        public List<LadgerEntry> LagerEntryDetailsList { get; set; }
        public int TotalRecords { get; set; }

    }


    public class SupplierPaymentViewModel
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Contains { get; set; }
        public int? SupplierID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class SupplierPaymentListViewModel
    {
        public List<IRPaymentDetailsDC> PaymentList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class SupplierOutStandingAmtViewModel
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Contains { get; set; }
        public int? SupplierID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class SupplieroutstandingListViewModel
    {
        public List<SupplierOutstandingamtDC> OutstandingList { get; set; }
        public int TotalRecords { get; set; }
    }
    public class SupplierpaymenthistoryandIRdata
    {
        public List<IRPaymentDetailHistory> irpaymenthistory { get; set; }
        public List<IRMasterdataDC> irmasterdatadc { get; set; }

        public List<LadgerEntry> ladgerEntries { get; set; }
    }

    public class IRMasterdataDC
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public string PaymentStatus { get; set; }
        public string IRType { get; set; }
        public double TotalAmountRemaining { get; set; }

        public double TotalAmount { get; set; }


    }
    public class AssignmentListbulkDC
    {

        public int AssignmentId { get; set; }
    }

}
