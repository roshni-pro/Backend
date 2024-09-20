
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.Base.Audit;
using AngularJSAuthentication.Accounts.Managers;
using Newtonsoft.Json;

namespace AngularJSAuthentication.API.ControllerV7
{
    [Authorize]
    [RoutePrefix("api/LadgerEntryV7")]
    public class LadgerEntryUIController : ApiController
    {
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        int compid = 0, userid = 0;

        public LadgerEntryUIController()
        {
            var identity = User.Identity as ClaimsIdentity;
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

        }


        #region ledger for Retailer App 
        // <summary>
        // tejas 
        // </summary>
        // <param name = "viewModel" ></ param >
        // < returns ></ returns >
        [Route("CustomerLedgerForRetailerApp")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult CustomerLedgerForRetailerApp(LedgerInputViewModelForRetailerApp viewModelV1)
        {
            //var checkDate = new DateTime(2018, 4, 1);
            //if (viewModelV1.FromDate >= checkDate)
            //{
            //    using (var authContext = new AuthContext())
            //    {
            //        LedgerHistoryViewModel vm = null;
            //        var LedgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Customer" && x.ObjectID == viewModelV1.CustomerId).FirstOrDefault();
            //        if (LedgerID != null)
            //        {
            //            viewModelV1.ToDate = viewModelV1.ToDate.AddDays(1);

            //            List<LadgerType> ladgerTypeList = null;
            //            LedgerWorker worker;
            //            ladgerTypeList = authContext.LadgerTypeDB.ToList();
            //            string ledgerTypeCode = ladgerTypeList.FirstOrDefault(x => x.ID == viewModelV1.LedgerTypeID)?.code;
            //            var viewModel = new LedgerInputViewModel();
            //            viewModel.LedgerID = Convert.ToInt32(LedgerID.ID);
            //            viewModel.FromDate = viewModelV1.FromDate;
            //            viewModel.ToDate = viewModelV1.ToDate;
            //            viewModel.IsGenerateExcel = viewModelV1.IsGenerateExcel;
            //            viewModel.LedgerTypeID = viewModelV1.LedgerTypeID;
            //            viewModel.ReportCode = viewModelV1.ReportCode;
            //            worker = new CustomerLedgerWorker();
            //            vm = worker.GetLedger(viewModel);

            //        }

            //        var response = new
            //        {
            //            CustomerLedger = vm,
            //            Status = true,
            //            Message = "companydetails"
            //        };
            //        return Ok(response);
            //    }
            //}
            //else
            //{
            //    LedgerHistoryViewModel vm = null;
            //    var response = new
            //    {
            //        CustomerLedger = vm,
            //        Status = true,
            //        Message = "Please Change Date To This Financial Year"
            //    };
            //    return Ok(response);
            //}
            LedgerHistoryViewModel vm = null;
            var response = new
            {
                CustomerLedger = vm,
                Status = false,
                Message = "Services currently under maintenance!!"
            };
            return Ok(response);

        }
        #endregion

        [Route("PurchasePendingCustomerLedger/{customerid}")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult PurchasePendingCustomerLedger(int customerid)
        {
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            using (var authContext = new AuthContext())
            {
                var ledgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Customer" && x.ObjectID == customerid).FirstOrDefault();
                if (ledgerID != null)
                {
                    var query = from l in authContext.LadgerDB
                                join le in authContext.LadgerEntryDB
                                on l.ID equals le.LagerID
                                join al in authContext.LadgerDB
                                on le.AffectedLadgerID equals al.ID
                                join alt in authContext.LadgerTypeDB
                                on al.LadgertypeID equals alt.ID
                                join om in authContext.DbOrderMaster
                                on le.ObjectID equals om.OrderId
                                join v in authContext.VoucherTypeDB
                                on le.VouchersTypeID equals v.ID
                                where le.ObjectType == "Order"
                                && l.ObjectType == "Customer"
                                && om.Status == "Delivered"
                                && l.ID == ledgerID.ID
                                //orderby le.Date.Value.Year, le.Date.Value.Month, le.Date.Value.Day, alt.Sequence, v.VSequence
                                select new LadgerEntryVM
                                {
                                    Active = true,
                                    AffactedLadgerName = al.Name,
                                    AffectedLadgerID = le.AffectedLadgerID,
                                    CreatedBy = le.CreatedBy,
                                    CreatedDate = le.CreatedDate,
                                    Credit = le.Credit,
                                    CreditString = (le.Credit > 0) ? le.Credit.ToString() : "",
                                    Date = le.Date,
                                    DayBalance = 0,
                                    DayBalanceString = "",
                                    Debit = le.Debit,
                                    DebitString = (le.Debit > 0) ? le.Debit.ToString() : "",
                                    ID = le.ID,
                                    IRID = "",
                                    LadgerName = l.Name,
                                    LagerID = l.ID,
                                    ObjectID = le.ObjectID.HasValue ? le.ObjectID.ToString() : "",
                                    ObjectType = le.ObjectType,
                                    Particulars = le.Particulars,
                                    UpdatedBy = le.UpdatedBy,
                                    UpdatedDate = le.UpdatedDate,
                                    VoucherName = v.Name,
                                    VouchersTypeID = v.ID,
                                    VSequence = v.VSequence,
                                    LedgerTypeSequence = alt.Sequence,

                                };

                    vm.LadgerEntryList = query
                     .OrderBy(x => x.Date.Value.Year)
                     .ThenBy(x => x.Date.Value.Month)
                     .ThenBy(x => x.Date.Value.Day)
                     .ThenBy(x => x.LedgerTypeSequence)
                     .ThenBy(x => x.VSequence)
                     .ToList<LadgerEntryVM>();


                    if (vm.LadgerEntryList != null)
                    {
                        double balance = 0;
                        foreach (var item in vm.LadgerEntryList)
                        {
                            item.DayBalance = balance;
                            balance += (item.Debit.HasValue ? item.Debit.Value : 0);
                        }
                    }

                }
                var response = new
                {
                    CustomerLedger = vm,
                    Status = true,
                    Message = "companydetails"
                };
                return Ok(vm);
            }
        }




        #region ledger for Supplier App  

        [Route("SupplierLedgerForSupplier")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult SupplierLedgerForSupplier(LedgerInputViewModelForSupplierApp viewModelV1)
        {
            var checkDate = new DateTime(2019, 4, 1);
            if (viewModelV1.FromDate >= checkDate)
            {
                using (var authContext = new AuthContext())
                {
                    var LedgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Supplier" && x.ObjectID == viewModelV1.SupplierID).FirstOrDefault();

                    viewModelV1.ToDate = viewModelV1.ToDate.AddDays(1);
                    LedgerHistoryViewModel vm = null;
                    List<LadgerType> ladgerTypeList = null;
                    LedgerWorker worker;
                    ladgerTypeList = authContext.LadgerTypeDB.ToList();
                    string ledgerTypeCode = ladgerTypeList.FirstOrDefault(x => x.ID == viewModelV1.LedgerTypeID)?.code;
                    var viewModel = new LedgerInputViewModel();
                    viewModel.LedgerID = Convert.ToInt32(LedgerID.ID);
                    viewModel.FromDate = viewModelV1.FromDate;
                    viewModel.ToDate = viewModelV1.ToDate;
                    viewModel.IsGenerateExcel = viewModelV1.IsGenerateExcel;
                    viewModel.LedgerTypeID = viewModelV1.LedgerTypeID;
                    viewModel.ReportCode = viewModelV1.ReportCode;
                    viewModel.RefNo = viewModelV1.RefNo;
                    worker = new SupplierLedgerWorker();
                    vm = worker.GetLedger(viewModel);


                    var response = new
                    {
                        CustomerLedger = vm,
                        Status = true,
                        Message = "SupplierLadger"
                    };

                    return Ok(response);
                }
            }
            else
            {
                LedgerHistoryViewModel vm = null;
                var response = new
                {
                    CustomerLedger = vm,
                    Status = true,

                    Message = "Please Change Date To This Financial Year"

                };
                return Ok(response);
            }
        }
        #endregion



        [Route("")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetLadgerEntryAsync()
        {

            using (var db = new AuthContext())
            {
                var query = from Le in db.LadgerEntryDB
                                //join vt in db.VoucherTypes
                                //on Le.VouchersTypeID equals vt.ID
                                //where ac.
                            select new
                            {
                                ID = Le.ID,
                                Date = Le.Date,
                                Particulars = Le.Particulars,
                                VouchersNo = Le.VouchersNo,
                                Debit = Le.Debit,
                                Credit = Le.Credit,
                                VoucherTypeName = "", //Le.VouchersType.Name,
                                AccountTypeID = Le.VouchersTypeID,
                            };
                var result = await query.ToListAsync();
                return Ok(result);
            }

        }
        [Route("GetList")]
        [HttpPost]
        public IHttpActionResult GetList(PagerDataUIViewModel pager)
        {
            using (AuthContext context = new AuthContext())
            {
                SqlParameter containsParam = new SqlParameter("Contains", pager.Contains);
                SqlParameter firstParam = new SqlParameter("First", pager.First);
                SqlParameter lastParam = new SqlParameter("Last", pager.Last);
                SqlParameter columnNameParam = new SqlParameter("ColumnName", pager.ColumnName);
                SqlParameter isAscendingParam = new SqlParameter("IsAscending", pager.IsAscending);
                object[] parameters = new object[] { containsParam, firstParam, lastParam, columnNameParam, isAscendingParam };

                var list = context.Database.SqlQuery<LadgerEntryPaginatorViewModel>("LadgerEntryPaginator @Contains,@First,@Last,@ColumnName,@IsAscending", parameters).ToList();
                return Ok(list);
            }


        }

        [Route("GetByID/{id}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetLadgerEntryV7(long id)
        {
            using (var db = new AuthContext())
            {
                LadgerEntry ladgerEntry = db.LadgerEntryDB.Find(id);
                if (ladgerEntry == null)
                {
                    return NotFound();
                }

                return Ok(ladgerEntry);
            }
        }


        [Route("UpdateByID/{id}")]
        [AcceptVerbs("PUT")]
        public IHttpActionResult PutLadgerEntryV7(long id, LadgerEntry ladgerEntry)
        {
            using (var db = new AuthContext())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != ladgerEntry.ID)
                {
                    return BadRequest();
                }

                db.Entry(ladgerEntry).State = EntityState.Modified;

                try
                {
                    db.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return null;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [Route("")]
        [AcceptVerbs("POST")]
        public IHttpActionResult PostLadgerEntryV7(LadgerEntry ladgerEntry)
        {
            using (var db = new AuthContext())
            {

                if (ladgerEntry != null)
                {
                    LadgerHelper helper = new LadgerHelper();
                    List<LadgerEntry> ledgerlist = helper.Createadjustladgerentry(ladgerEntry, userid, db);
                }
                return Ok();
            }
        }


        [Route("")]
        [AcceptVerbs("Delete")]
        public IHttpActionResult DeleteLadgerEntryV7(long id)
        {
            using (var db = new AuthContext())
            {
                LadgerEntry ladgerEntry = db.LadgerEntryDB.Find(id);
                if (ladgerEntry == null)
                {
                    return NotFound();
                }

                db.LadgerEntryDB.Remove(ladgerEntry);
                db.Commit();

                return Ok(ladgerEntry);
            }
        }


        [Route("GetDataForExport/{fromdate}/{todate}/{type}")]
        [AcceptVerbs("Get")]
        //[HttpGet]
        public IHttpActionResult GetDataForExport(DateTime fromdate, DateTime todate, string type)
        {
            using (var db = new AuthContext())
            {
                var query = from Le in db.LadgerEntryDB
                            join vt in db.VoucherTypeDB
                            on Le.VouchersTypeID equals vt.ID
                            where Le.Date >= fromdate && Le.Date <= todate && type == "Credit" ? Le.Credit != null : Le.Debit != null
                            //where ac.
                            select new
                            {
                                ID = Le.ID,
                                Date = Le.Date,
                                Particulars = Le.Particulars,
                                VouchertypeName = vt.Name,
                                Debit = Le.Debit,
                                Credit = Le.Credit


                            };


                var result = query.ToList();

                DataTable dt = Common.Helpers.ListtoDataTableConverter.ToDataTable(result);

                DataSet ds = new DataSet();
                ds.Tables.Add(dt);

                Common.Helpers.ExportServices.DataSet_To_Excel(ds, @"C:\Users\ShopKirana\Desktop\angular_backend\Shopkirana-Backend\AngularJSAuthentication.API\Reports\abc" + DateTime.Now + ".xlsx");
                return Ok();
            }
        }

     
        [Route("CustomerLedger")]
        [HttpPost]
        public IHttpActionResult GetCustomerLedger(LedgerInputViewModel viewModel)
        {
            viewModel.ToDate = viewModel.ToDate.AddDays(1);
            viewModel.ToDate = viewModel.ToDate.AddMinutes(-1);
            viewModel.RefNo = !string.IsNullOrEmpty(viewModel.RefNo) ? viewModel.RefNo : "";
            LedgerHistoryViewModel vm = null;
            List<LadgerType> ladgerTypeList = null;
            LedgerWorker worker;
            using (var authContext = new AuthContext())
            {
                ladgerTypeList = authContext.LadgerTypeDB.ToList();
            }

            string ledgerTypeCode = ladgerTypeList.FirstOrDefault(x => x.ID == viewModel.LedgerTypeID)?.code;

            if (ledgerTypeCode == "Customer")
            {
                worker = new CustomerLedgerWorker();

            }
            else if (ledgerTypeCode == "Supplier")
            {
                worker = new SupplierLedgerWorker();
            }
            else if (ledgerTypeCode == "Agent")
            {
                worker = new AgentLedgerWorker();
            }
            else if (ledgerTypeCode == "Transporter")
            {
                worker = new TransporterLedgerWorker();
            }
            else
            {
                worker = new LedgerWorker();
            }
            vm = worker.GetLedger(viewModel);

            worker.UpdateTableToPrint(vm.LadgerEntryList);
            worker.UpdateOpeningAndClosingBalance(vm);
            return Ok(vm);
        }


        //#region ledger for Retailer App 
        ///// <summary>
        ///// tejas 
        ///// </summary>
        ///// <param name="viewModel"></param>
        ///// <returns></returns>
        //[Route("CustomerLedgerForRetailerApp")]
        //[HttpPost]
        //[AllowAnonymous]
        //public IHttpActionResult CustomerLedgerForRetailerApp(LedgerInputViewModelForRetailerApp viewModelV1)
        //{
        //    using (var authContext = new AuthContext())
        //    {
        //        var LedgerID = authContext.LadgerDB.Where(x => x.ObjectType == "Customer" && x.ObjectID == viewModelV1.CustomerId).SingleOrDefault();

        //        viewModelV1.ToDate = viewModelV1.ToDate.AddDays(1);
        //        LedgerHistoryViewModel vm = null;
        //        List<LadgerType> ladgerTypeList = null;
        //        LedgerWorker worker;
        //        ladgerTypeList = authContext.LadgerTypeDB.ToList();
        //        string ledgerTypeCode = ladgerTypeList.FirstOrDefault(x => x.ID == viewModelV1.LedgerTypeID)?.code;
        //        var viewModel = new LedgerInputViewModel();
        //        viewModel.LedgerID = Convert.ToInt32(LedgerID.ID);
        //        viewModel.FromDate = viewModelV1.FromDate;
        //        viewModel.ToDate = viewModelV1.ToDate;
        //        viewModel.IsGenerateExcel = viewModelV1.IsGenerateExcel;
        //        viewModel.LedgerTypeID = viewModelV1.LedgerTypeID;
        //        viewModel.ReportCode = viewModelV1.ReportCode;
        //        worker = new CustomerLedgerWorker();
        //        vm = worker.GetLedger(viewModel);

        //        return Ok(vm);
        //    }
        //}
        //#endregion
        [HttpGet]
        [Route("LedgerHistory/{id}")]
        public IHttpActionResult LedgerHistory(int id)
        {
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            using (AuthContext context = new AuthContext())
            {
                //vm.LadgerItem = context.Ladgers.Where(x => x.ID == id).FirstOrDefault();
                //vm.LadgerEntryList = context.LadgerEntries.Where(x => x.AffectedLadgerID == id).ToList();
            }
            return Ok(vm);
        }

        [Route("UploadAll")]
        [HttpPost]
        public IHttpActionResult UploadAll(List<LadgerEntry> ladgerEntryList)
        {
            List<LadgerEntry> newLadgerEntryList = new List<LadgerEntry>();
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    string guid = Guid.NewGuid().ToString();
                    long? ledgerid = null;
                    if (ladgerEntryList != null && ladgerEntryList.Count > 0)
                    {
                        long? id = ladgerEntryList.FirstOrDefault().LagerID;
                        ledgerid = context.LadgerDB.FirstOrDefault(x => x.ObjectID == id && x.ObjectType == "Supplier")?.ID;
                    }
                    foreach (var item in ladgerEntryList)
                    {
                        var query = from le in context.LadgerEntryDB
                                    join v in context.VoucherDB
                                    on le.VouchersNo equals v.ID
                                    join l in context.LadgerDB
                                    on le.LagerID equals l.ID
                                    where l.ObjectType.ToLower() == "supplier" && v.Code == item.Particulars && l.ID == ledgerid
                                    select new
                                    {
                                        VoucherNumber = v.ID,
                                        ObjectID = le.ObjectID,
                                        ObjectType = le.ObjectType

                                    };
                        var vchEntry = query.FirstOrDefault();
                        if (vchEntry != null && ledgerid.HasValue)
                        {
                            item.LagerID = ledgerid.Value;
                            item.ObjectID = vchEntry.ObjectID;
                            item.ObjectType = vchEntry.ObjectType;
                            item.VouchersNo = vchEntry.VoucherNumber;
                            item.CreatedBy = userid;
                            item.UpdatedBy = userid;
                            item.CreatedDate = DateTime.Now;
                            item.UpdatedDate = DateTime.Now;
                            item.Active = true;
                            item.UploadGUID = guid;
                            context.LadgerEntryDB.Add(item);
                            context.Commit();


                            LadgerEntry ladgerEntry = new LadgerEntry();
                            ladgerEntry.LagerID = item.AffectedLadgerID;
                            ladgerEntry.AffectedLadgerID = item.LagerID;
                            ladgerEntry.ObjectID = vchEntry.ObjectID;
                            ladgerEntry.ObjectType = vchEntry.ObjectType;
                            ladgerEntry.VouchersNo = vchEntry.VoucherNumber;
                            ladgerEntry.CreatedBy = userid;
                            ladgerEntry.UpdatedBy = userid;
                            ladgerEntry.CreatedDate = DateTime.Now;
                            ladgerEntry.UpdatedDate = DateTime.Now;
                            ladgerEntry.Active = true;
                            ladgerEntry.Date = item.Date;
                            ladgerEntry.Debit = item.Credit;
                            ladgerEntry.Credit = item.Debit;
                            ladgerEntry.Particulars = item.Particulars;
                            ladgerEntry.UploadGUID = guid;
                            ladgerEntry.VouchersTypeID = item.VouchersTypeID;
                            context.LadgerEntryDB.Add(ladgerEntry);
                            context.Commit();


                            if (ladgerEntry.VouchersTypeID == context.VoucherTypeDB.FirstOrDefault(x => x.Name == "Payment")?.ID)
                            {
                                var amount = item.Credit > item.Debit ? item.Credit : item.Debit;
                                var invoiceNumber = vchEntry.VoucherNumber;
                                var irID = vchEntry.ObjectID;



                                IRMaster irdata = context.IRMasterDB.Where(x => x.Id == irID && x.Deleted == false).FirstOrDefault();

                                SupplierPaymentData paydata = new SupplierPaymentData();
                                paydata.PurchaseOrderId = irdata.PurchaseOrderId;
                                paydata.InVoiceNumber = irdata.IRID;
                                paydata.CreditInVoiceAmount = amount ?? 0;
                                paydata.PaymentStatusCorD = "Credit";
                                paydata.VoucherType = "Payment";
                                paydata.ClosingBalance = amount ?? 0;
                                paydata.CompanyId = 1;
                                paydata.WarehouseId = irdata.WarehouseId;
                                paydata.InVoiceDate = irdata.CreationDate;
                                paydata.SupplierId = irdata.supplierId;
                                paydata.SupplierName = irdata.SupplierName;
                                paydata.CreatedDate = DateTime.Now;
                                paydata.UpdatedDate = DateTime.Now;
                                context.SupplierPaymentDataDB.Add(paydata);
                                FullSupplierPaymentData suppdata = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.supplierId && x.Deleted == false).FirstOrDefault();

                                suppdata.InVoiceRemainingAmount = suppdata.InVoiceRemainingAmount - (amount ?? 0);
                                if (suppdata.InVoiceRemainingAmount == 0)
                                {
                                    suppdata.SupplierPaymentStatus = "Full Paid";

                                }
                                else
                                {
                                    suppdata.SupplierPaymentStatus = "Partial Paid";

                                }
                                context.Entry(suppdata).State = EntityState.Modified;
                                irdata.PaymentStatus = "Paid";
                                irdata.IRStatus = "Paid";

                                context.Entry(irdata).State = EntityState.Modified;
                                context.Commit();


                            }



                        }


                        newLadgerEntryList.Add(item);



                    }
                    //vm.LadgerItem = context.Ladgers.Where(x => x.ID == id).FirstOrDefault();
                    //vm.LadgerEntryList = context.LadgerEntries.Where(x => x.AffectedLadgerID == id).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Ok(newLadgerEntryList);
        }

        [Route("GetSupplierOutstanding")]
        [HttpGet]
        public double GetSupplierOutstanding(int supplierId)
        {
            using (var context = new AuthContext())
            {
                var query = from le in context.LadgerEntryDB
                            join l in context.LadgerDB on le.LagerID equals l.ID
                            where l.ObjectID == supplierId && l.ObjectType == "Supplier"
                            select le;
                double amout = query.Sum(x => x.Debit - x.Credit).Value;
                return amout;
            }
        }


        [Route("CreateLedgerEntry")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> CreateLedgerEntry(List<LedgerQueue> auditList)
        {
            LedgerComposerManager ledgerComposerManager = new LedgerComposerManager();
            foreach (var item in auditList)
            {
                ledgerComposerManager.CheckAndCreateLedger( item);
            }


            return true;
        }


        [Route("AutoLedgerPosting")]
        [HttpGet]
        [AllowAnonymous]
        public bool AutoLedgerPosting()
        {
            AutoCreateLedgerEntryHelper AutoCreateLedgerHelper = new AutoCreateLedgerEntryHelper();
            return AutoCreateLedgerHelper.AutoCreateLedgerEntry();
        }
    }






    public class LedgerHistoryViewModel
    {
        public LedgerVM LadgerItem { get; set; }
        public List<LadgerEntryVM> LadgerEntryList { get; set; }
        public Double? OpeningBalance { get; set; }
        public Double? ClosingBalance { get; set; }

        public string OpeningBalanceString { get; set; }
        public string ClosingBalanceString { get; set; }
        public Double? IRTotalSum { get; set; }
        public Double? GRTotalSum { get; set; }
        public double TotalCredit { get; set; }
        public double TotalDebit { get; set; }
    }


    public class LedgerInputViewModel
    {
        public int LedgerID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsGenerateExcel { get; set; }
        public int LedgerTypeID { get; set; }
        public int POid { get; set; }
        public string ReportCode { get; set; }
        public string AgentReportType { get; set; }

        public bool? IsGetRawReportForExcel { get; set; }
        public int? DepoId { get; set; }
        public string RefNo { get; set; }
    }

    public class LedgerInputViewModelForRetailerApp
    {
        //public int LedgerID { get; set; }
        public int CustomerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsGenerateExcel { get; set; }
        public int LedgerTypeID { get; set; }
        public string ReportCode { get; set; }

        //public int LedgerID { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }
        //public bool IsGenerateExcel { get; set; }
        //public int LedgerTypeID { get; set; }
        //public string ReportCode { get; set; }
    }

    public class LedgerInputViewModelForSupplierApp
    {
        //public int LedgerID { get; set; }
        public int SupplierID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsGenerateExcel { get; set; }
        public int LedgerTypeID { get; set; }
        public string ReportCode { get; set; }
        public string RefNo { get; set; }

        //public int LedgerID { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }
        //public bool IsGenerateExcel { get; set; }
        //public int LedgerTypeID { get; set; }
        //public string ReportCode { get; set; }
    }
    public class PODetailsDc
    {
        public int PoId { get; set; }
        public string PurchaseSku { get; set; }
        public string CompanyStockCode { get; set; }
        public string ItemName { get; set; }
        public string ABcClassification { get; set; }
        public string HSNCode { get; set; }
        public double Price { get; set; }
        public int MOQ { get; set; }
        public double Qty { get; set; }
        public int NoOfPieces { get; set; }
        public double TotalAmount { get; set; }
    }
}