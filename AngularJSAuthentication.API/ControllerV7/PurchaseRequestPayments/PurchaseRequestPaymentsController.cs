using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.PurchaseOrder;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.supplier;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using Common.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using LogManager = NLog.LogManager;

namespace AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments
{

    [RoutePrefix("api/PurchaseRequestPayments")]
    public class PurchaseRequestPaymentsController : BaseApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("GetPrListByFilter")]
        [HttpPost]
        public dynamic GetPrListByFilter(PoApprovalPaginator paginator)
        {
            try
            {
                POApprovalPager pager = new POApprovalPager();
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    bool TDSApplyOnAdvancePayment = Convert.ToBoolean(ConfigurationManager.AppSettings["TDSApplyOnAdvancePayment"]);

                    
                    string sqlquery = "";
                    
                    if (TDSApplyOnAdvancePayment)
                    {
                        
                        sqlquery = @"SELECT 
                            POM.Advance_Amt - ISNULL(TAmount, 0) TotalAmount, 
                            POM.SupplierId,
                            POM.PurchaseOrderId,
                            POM.WarehouseId,
                            POM.WarehouseName,
                            POM.Advance_Amt - ISNULL(TAmount, 0) Advance_Amt,
                            POM.CreationDate,
                            POM.SupplierName,
                            POM.PoInvoiceNo,
                            POM.PRStatus,
                            case when (ISNULL( s.TINNo ,'')<>'' OR ISNULL(s.Pancard,'')<>'') then cast( 0.1 as float) else cast( 5 as float) end  as TDSPercentage
                            ,case when (prr.PaidAmount) > irr.IRAmount then prr.PaidAmount else irr.IRAmount end as PaymentTillDate
                            FROM PurchaseOrderMasters POM with(nolock)
                            INNER JOIN PRPaymentAppoveds PRPA with(nolock) 
                                ON POM.PurchaseOrderId = PRPA.PRId 
                             INNER JOIN Suppliers S with(nolock) on POM.SupplierId = S.SupplierId
                             left join IRMasters im with(nolock) on im.supplierId = S.SupplierId
                            OUTER APPLY(
                                SELECT SUM(TOT.TransferredAmount) - ISNULL(SUM(FROMT.TransferredAmount), 0) TAmount
                                FROM PrPaymentTransfers TOT with(nolock)
                            	INNER JOIN PurchaseRequestPayments PRP with(nolock)
                            		ON TOT.SourcePurchaseRequestPaymentId = PRP.Id  AND PRP.PrPaymentStatus <> 'Rejected' 
                                LEFT JOIN PrPaymentTransfers FROMT with(nolock) ON TOT.ToPurchaseOrderId = FROMT.FromPurchaseOrderId
                                WHERE TOT.ToPurchaseOrderId = POM.PurchaseOrderId	
                            )Temp
                            cross apply(
                            	SELECT   CAST (ISNULL(SUM(LE.Debit), 0) as float)   as PaidAmount 
                            	FROM LadgerEntries LE with(nolock)
                            	INNER JOIN Ladgers L with(nolock) ON LE.LagerID = L.ID AND L.ObjectType = 'Supplier'    
                            	INNER JOIN VoucherTypes V with(nolock) ON LE.VouchersTypeID = V.Id 
		                    	 cross apply(
		                     select  ss.SupplierId from Suppliers ss with(nolock) 
		                     inner join Suppliers sp with(nolock) on ss.Pancard=sp.Pancard --and ss.SupplierId=s.SupplierId
		                     where sp.SupplierId=S.SupplierId
		                     )St
                            	WHERE  V.Name = 'Payment' and L.ObjectID in (St.SupplierId)
                            	--AND LE.ObjectType ='PR'
                            	--and L.ObjectID in (select SupplierId from Suppliers with(nolock) where Pancard in (select Pancard from Suppliers with(nolock) where SupplierId=S.SupplierId and Active=1))
                            	AND LE.Date  
                            	between (case when cast(GETDATE() as date)<  cast(dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0))    as date)    
                            	then CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE())-1, 0))   as date)    
                            	else CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)) as date) end)        
                            	and       
                            	(case when cast(GETDATE() as date)< cast(dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0))   as date)    
                            	then CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), -1))    as date)    
                            	else CAST(  dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE())+1, -1)) as date) end ) 
                            )prr
                            cross apply(
                            SELECT round(cast(ISNULL(sum(ir.IRAmountWithTax),0) as float),2) as IRAmount
       
                            
                            FROM IRMasters IR with(nolock)
                             cross apply(
		                     select  ss.SupplierId from Suppliers ss with(nolock) 
		                     inner join Suppliers sp with(nolock) on ss.Pancard=sp.Pancard --and ss.SupplierId=s.SupplierId
		                     where  sp.SupplierId=S.SupplierId
		                     )St
                            WHERE IR.supplierId in (St.SupplierId)
		                    --in (select SupplierId from Suppliers with(nolock) where Pancard in (select Pancard from Suppliers with(nolock) where SupplierId=S.SupplierId and Active=1))
                            and IR.InvoiceDate 
                            between
                            (case when cast(GETDATE() as date)<  cast(dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0))    as date)    
                            then CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE())-1, 0))   as date)    
                            else CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)) as date) end)        
                            and       
                            (case when cast(GETDATE() as date)< cast(dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0))   as date)    
                            then CAST( dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), -1))    as date)    
                            else CAST(  dateadd(MONTH,3,DATEADD(yy, DATEDIFF(yy, 0, GETDATE())+1, -1)) as date) end ) 
                            AND  IR.IRStatus in ('Approved from Buyer side','Paid') and IR.Deleted=0 
                            	
                            )irr
                            
                            where PRPA.IsApproved = 1 
                                and POM.PRStatus = 5 
                                and ( POM.Status IS NULL OR (POM.Status <> 'Auto Closed' AND POM.Status <> 'Closed') )
                                and POM.PRPaymentType = 'AdvancePR' 
                                and PRPA.IsActive = 1  
                                and POM.PurchaseOrderId 
                                not in (
                                    select PurchaseRequestPayments.PurchaseOrderId 
                                    from PurchaseRequestPayments 
                                    where PurchaseRequestPayments.PrPaymentStatus != 'Rejected'
                                )
                                AND POM.Advance_Amt - ISNULL(TAmount, 0)  > 0
                            	group by POM.Advance_Amt,Temp.TAmount,POM.SupplierId,
                            POM.PurchaseOrderId,
                            POM.WarehouseId,
                            POM.WarehouseName,POM.CreationDate,
                            POM.SupplierName,
                            POM.PoInvoiceNo,POM.PRStatus,S.TINNo,S.Pancard,prr.PaidAmount,irr.IRAmount ";

                    }
                    else
                    {
                        sqlquery = @"SELECT 
                                POM.Advance_Amt - ISNULL(TAmount, 0) TotalAmount, 
                                POM.SupplierId,
                                POM.PurchaseOrderId,
                                POM.WarehouseId,
                                POM.WarehouseName,
                                POM.Advance_Amt - ISNULL(TAmount, 0) Advance_Amt,
                                POM.CreationDate,
                                POM.SupplierName,
                                POM.PoInvoiceNo,
                                POM.PRStatus,
                                cast( 0 as float) as TDSPercentage
                                FROM PurchaseOrderMasters POM
                                INNER JOIN PRPaymentAppoveds PRPA 
	                                ON POM.PurchaseOrderId = PRPA.PRId 
                                 INNER JOIN Suppliers S on POM.SupplierId = S.SupplierId
                                OUTER APPLY(
	                                SELECT SUM(TOT.TransferredAmount) - ISNULL(SUM(FROMT.TransferredAmount), 0) TAmount
	                                FROM PrPaymentTransfers TOT
									INNER JOIN PurchaseRequestPayments PRP
										ON TOT.SourcePurchaseRequestPaymentId = PRP.Id  AND PRP.PrPaymentStatus <> 'Rejected' 
	                                LEFT JOIN PrPaymentTransfers FROMT ON TOT.ToPurchaseOrderId = FROMT.FromPurchaseOrderId
	                                WHERE TOT.ToPurchaseOrderId = POM.PurchaseOrderId	
                                )Temp
                                where PRPA.IsApproved = 1 
	                                and POM.PRStatus = 5 
                                    and ( POM.Status IS NULL OR (POM.Status <> 'Auto Closed' AND POM.Status <> 'Closed') )
	                                and POM.PRPaymentType = 'AdvancePR' 
	                                and PRPA.IsActive = 1  
	                                and POM.PurchaseOrderId 
	                                not in (
		                                select PurchaseRequestPayments.PurchaseOrderId 
		                                from PurchaseRequestPayments 
		                                where PurchaseRequestPayments.PrPaymentStatus != 'Rejected'
	                                )
	                                AND POM.Advance_Amt - ISNULL(TAmount, 0)  > 0";
                    }
                    string whereclase = "";

                    if (paginator.WarehouseId.HasValue)
                    {
                        whereclase += " and POM.WarehouseId= " + paginator.WarehouseId.Value;
                    }

                    if (!string.IsNullOrEmpty(paginator.Search))
                    {
                        if (int.TryParse(paginator.Search, out _))
                        {
                            whereclase += " and POM.PurchaseOrderId like '%" + paginator.Search + "%'";
                        }
                        else
                        {
                            whereclase += " and POM.SupplierName like '%" + paginator.Search + "%'";
                        }

                    }

                    if (paginator.StartDate.HasValue && paginator.EndDate.HasValue)
                    {
                        string startdate = "'" + paginator.StartDate.Value.ToString("yyyy-MM-dd") + "'";

                        string enddate = "'" + paginator.EndDate.Value.ToString("yyyy-MM-dd") + "'";
                        //whereclase += "  and PurchaseOrderMasters.CreationDate >= '" + paginator.StartDate.Value.ToString("yyyy-MM-dd") + "' and PurchaseOrderMasters.CreationDate <=  '" + paginator.EndDate.Value.ToString("yyyy-MM-dd") + " ' ";
                        //  whereclase += "and convert(date, PurchaseOrderMasters.CreationDate,103)  >= " + paginator.StartDate.Value.ToString("yyyy-MM-dd") + "and convert(date, PurchaseOrderMasters.CreationDate,103) <= " + paginator.EndDate.Value.ToString("yyyy-MM-dd") ;
                        whereclase += "and convert(date, POM.CreationDate,103)  >= " + startdate + "and convert(date, POM.CreationDate,103) <= " + enddate;

                    }

                    sqlquery += whereclase;

                    List<PurchaseOrderMasterDTO> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).ToList();
                    pager.Count = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).Count();
                    pager.PrList = db.Database.SqlQuery<PurchaseOrderMasterDTO>(sqlquery).OrderByDescending(x => x.PurchaseOrderId).Skip(paginator.SkipCount).Take(paginator.Take).ToList();

                    return pager;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Route("GetPRPaymentsListByDate")]
        [HttpPost]
        public dynamic GetPRPaymentsListByDate(PoApprovalPaginator paginator)
        {
            try
            {
                PurchaseRequestPaymentsPager pager = new PurchaseRequestPaymentsPager();
                using (var db = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    //string sqlquery = "SELECT * FROM PurchaseOrderMasters INNER JOIN PRPaymentAppoveds ON PurchaseOrderMasters.PurchaseOrderId = PRPaymentAppoveds.PRId where PRPaymentAppoveds.IsApproved = 1 and PurchaseOrderMasters.PRStatus = 5 and PRPaymentAppoveds.IsActive = 1 and PurchaseOrderMasters.PurchaseOrderId not in (select PurchaseRequestPayments.PurchaseOrderId from PurchaseRequestPayments where PurchaseRequestPayments.PrPaymentStatus != 'Approved' or PurchaseRequestPayments.PrPaymentStatus != 'Pending')";

                    string sqlquery = "SELECT * FROM PurchaseRequestPayments where ";
                    string whereclase = "";

                    if (paginator.StartDate.HasValue && paginator.EndDate.HasValue)
                    {
                        string startdate = "'" + paginator.StartDate.Value.ToString("yyyy-MM-dd") + "'";
                        string enddate = "'" + paginator.EndDate.Value.ToString("yyyy-MM-dd") + "'";
                        whereclase += " convert(date, PurchaseRequestPayments.PaymentDate,103)  >= " + startdate + "and convert(date, PurchaseRequestPayments.PaymentDate,103) <= " + enddate;
                    }

                    sqlquery += whereclase;

                    pager.Count = db.Database.SqlQuery<PurchaseRequestPaymentDC>(sqlquery).Count();
                    pager.PrList = db.Database.SqlQuery<PurchaseRequestPaymentDC>(sqlquery).OrderByDescending(x => x.PurchaseOrderId).ToList();

                    return pager;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        [Authorize]
        [Route("GetPaymentSummaryList")]
        [HttpPost]
        public PRPaymentSummaryPage GetPaymentSummaryList(PrPaymentSummaryPaginator paginator)
        {
            PRPaymentSummaryPage pRPaymentSummaryPage = new PRPaymentSummaryPage();
            try
            {
                using (var authContext = new AuthContext())
                {
                    pRPaymentSummaryPage.Count = authContext.PRPaymentSummaryDB.Where(x => x.IsActive == true && x.Deleted == false && x.TotalAmount > 0).ToList().Count();
                    pRPaymentSummaryPage.summaryList = authContext.PRPaymentSummaryDB.Where(x => x.IsActive == true && x.Deleted == false && x.TotalAmount > 0).OrderByDescending(x => x.Id).Skip(paginator.SkipCount).Take(paginator.Take).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return pRPaymentSummaryPage;

        }

        [Route("SupplierPaymentReport/{prPaymentSummaryId}")]
        [HttpGet]
        public string SupplierPaymentReport(int prPaymentSummaryId)
        {

            //N,,{#ACCOUNTNO#},{#AMOUNT#},{#SUPPLERNAME#},,,,,,,,Shop Kirana E Tradin,{#SUPPLERNAME#},,,,,,,,,{#DD/MM/YYYY#},,{#IFSC#},,,{#EMAIL#}
            string bankPaymentString = AppConstants.BankPayment;

            try
            {
                using (var context = new AuthContext())
                {
                    List<PRPaymentSummaryBankReceipt> list = context.Database.SqlQuery<PRPaymentSummaryBankReceipt>("EXEC PRPaymentSummaryBankReceiptGet @PRPaymentSummaryId", new SqlParameter("PRPaymentSummaryId", prPaymentSummaryId)).ToList();

                    if (list != null && list.Any())
                    {
                        string fullBankReceipt = "";
                        foreach (var item in list)
                        {
                            string supplierCodeString = SupplierPaymentHelper.GetPaymentPerticular(item.SupplierCode, item.SupplierName, item.WarehouseName);

                            string entry = bankPaymentString.Replace("{#ACCOUNTNO#}", !string.IsNullOrEmpty(item.AccountNumber) ? item.AccountNumber.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#AMOUNT#}", item.TotalAmount.HasValue ? ((int)item.TotalAmount).ToString().Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#SUPPLERNAME#}", !string.IsNullOrEmpty(item.SupplierName) ? item.SupplierName.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#DD/MM/YYYY#}", !string.IsNullOrEmpty(item.PaymentDate) ? item.PaymentDate.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#IFSC#}", !string.IsNullOrEmpty(item.IFSC) ? item.IFSC.Trim(new char[] { '\n', '\r' }) : "");
                            entry = entry.Replace("{#EMAIL#}", "sourcingteam@shopkirana.com");
                            entry = entry.Replace("{#SUPPLERCODE#}", supplierCodeString);
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

        [Authorize]
        [Route("MakePRPayment")]
        [HttpPost]
        public bool MakePRPayment(PrOutstandingPayment payment)
        {
            int userid = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            Guid guid = Guid.NewGuid();
            if (payment != null && payment.PrList != null && payment.PrList.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                //using (var scope = new TransactionScope())
                {
                    try
                    {
                        List<SupplierPaymentssDC> paymentt = new List<SupplierPaymentssDC>();
                        bool TDSApplyOnAdvancePayment = Convert.ToBoolean(ConfigurationManager.AppSettings["TDSApplyOnAdvancePayment"]);
                        using (var context = new AuthContext())
                        {
                            
                            PRPaymentSummary summary = new PRPaymentSummary()
                            {
                                Createby = userid,
                                Deleted = false,
                                IsActive = true,
                                PaymentDate = payment.PaymentDate,
                                TotalAmount = payment.PrList.Sum(x => x.PaidAmount),
                                Updateby = null,
                                UpdateDate = null,
                                IsPROutstandingPending = true
                            };

                            context.PRPaymentSummaryDB.Add(summary);
                            context.Commit();

                            foreach (int s in payment.PrList.Select(x=>x.SupplierId).Distinct())
                            {
                                SupplierPaymentssDC obj = new SupplierPaymentssDC();
                                var param = new SqlParameter
                                {
                                    ParameterName = "Supplierid",
                                    Value = s
                                };
                                var data = context.Database.SqlQuery<double>("exec SP_GetTotalValuebySupplierId @Supplierid", param).FirstOrDefault();
                                obj.SupplierId = s;
                                obj.PaymentTillDate = data;
                                paymentt.Add(obj);
                            }

                            foreach (var item in payment.PrList)
                            {

                                List<PRPaymentDc> settlePaymentList = null;
                                double tdsper = 0;
                                double tds = 0;

                                if (TDSApplyOnAdvancePayment)
                                {
                                    var suppliergst = context.Suppliers.Where(x => x.Active == true && x.SupplierId == item.SupplierId).FirstOrDefault();
                                    if (suppliergst != null)
                                    {
                                        if (!string.IsNullOrEmpty(suppliergst.Pancard) || !string.IsNullOrEmpty(suppliergst.TINNo))
                                        {

                                            var supplierPayment = paymentt.Where(x => x.SupplierId == item.SupplierId).First();

                                            if (supplierPayment.PaymentTillDate > 5000000)
                                            {
                                                tdsper = 0.1;
                                                tds = item.PaidAmount * tdsper / 100;
                                                supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;
                                            }
                                            else
                                            {
                                                if (supplierPayment.PaymentTillDate + item.PaidAmount > 5000000)
                                                {
                                                    tdsper = 0.1;
                                                    tds = ((supplierPayment.PaymentTillDate + item.PaidAmount) - 5000000) * tdsper / 100;
                                                    supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;
                                                }
                                                else
                                                {
                                                    tdsper = 0;
                                                    tds = item.PaidAmount * tdsper / 100;
                                                    supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tdsper = 5;
                                            tds = item.PaidAmount * tdsper / 100;
                                        }
                                    }

                                    item.PaidAmount = item.PaidAmount - tds;
                                }

                                if (item.SettledAmount > 0)
                                {
                                    List<SqlParameter> paramList = new List<SqlParameter>();
                                    string supplierIdString = string.Join(",", item.SupplierId.ToString());
                                    paramList.Add(new SqlParameter("@SupplierIdList", supplierIdString));
                                    paramList.Add(new SqlParameter("@IsGetSummary", false));
                                    settlePaymentList = context.Database.SqlQuery<PRPaymentDc>("GetAdvanceOutstanding @SupplierIdList, @IsGetSummary", paramList.ToArray()).ToList();
                                }

                                if (item.SettledAmount > 0 && settlePaymentList != null && settlePaymentList.Any())
                                {
                                    settlePaymentList = settlePaymentList.OrderBy(x => x.Total).ToList();
                                    double settleAmt = item.SettledAmount;
                                    while (settleAmt > 0)
                                    {
                                        PRPaymentDc settlePayment = settlePaymentList.Where(x => x.Total > 0).FirstOrDefault();
                                        double transferredAmount = settleAmt > settlePayment.Total ? settlePayment.Total : settleAmt;
                                        settlePayment.Total -= transferredAmount;
                                        PrPaymentTransfer prPaymentTransfer = new PrPaymentTransfer
                                        {
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedBy = userid,
                                            CreatedDate = currentTime,
                                            FromPurchaseOrderId = settlePayment.PurchaseOrderId,
                                            ModifiedBy = null,
                                            ModifiedDate = null,
                                            SettledAmount = 0,
                                            SourcePurchaseRequestPaymentId = settlePayment.SourcePurchaseRequestPaymentId,
                                            ToPurchaseOrderId = item.PurchaseOrderId,
                                            TransferredAmount = transferredAmount,
                                            IsTDSDeducted = settlePayment.IsTDSDeducted
                                        };

                                        PrPaymentTransfer oldPRPaymentTransfer = context.PrPaymentTransferDB.First(x => x.Id == settlePayment.PRPaymentTransferId);
                                        oldPRPaymentTransfer.OutAmount = oldPRPaymentTransfer.OutAmount.HasValue ? (oldPRPaymentTransfer.OutAmount + prPaymentTransfer.TransferredAmount) : prPaymentTransfer.TransferredAmount;
                                        settleAmt -= prPaymentTransfer.TransferredAmount;
                                        context.PrPaymentTransferDB.Add(prPaymentTransfer);
                                        context.Commit();
                                    }
                                }

                                if (item.PaidAmount > 0)
                                {
                                    PurchaseRequestPayment detail = new PurchaseRequestPayment()
                                    {
                                        BankId = payment.BankId,
                                        PurchaseOrderId = item.PurchaseOrderId,
                                        BankName = payment.BankName,
                                        Createby = userid,
                                        CreatedDate = currentTime,
                                        Deleted = false,
                                        Guid = guid.ToString(),
                                        PRList = JsonConvert.SerializeObject(item),
                                        PRPaymentSummaryId = summary.Id,
                                        IsActive = true,
                                        PaymentDate = summary.PaymentDate,
                                        RefNo = payment.RefNo,
                                        Remark = payment.Remark,
                                        SupplierId = item.SupplierId.Value,
                                        TotalAmount = (int)item.PaidAmount + (int)item.SettledAmount,
                                        WarehouseId = item.WarehouseId,
                                        PrPaymentStatus = (item.PaidAmount == 0) ? "Approved" : "Pending",
                                        IsPROutstandingPending = true,
                                        SupplierName = item.SupplierName,
                                        PoInvoiceNo = item.PoInvoiceNo,
                                        UpdateDate = DateTime.Now,
                                        Updateby = userid,
                                        PaidAmount = (int)item.PaidAmount,
                                        TDSAmount = tds

                                    };
                                    ConfirmPayment cPmt = new ConfirmPayment();
                                    cPmt.Comment = detail.Remark;
                                    cPmt.PurchaseOrderId = item.PurchaseOrderId;
                                    cPmt.IsPaymentDone = false;
                                    //this.Confirmpayment(cPmt);
                                    context.PurchaseRequestPaymentsDB.Add(detail);
                                    context.Commit();

                                    PrPaymentTransfer prPaymentTransfer = new PrPaymentTransfer
                                    {
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userid,
                                        CreatedDate = currentTime,
                                        FromPurchaseOrderId = null,
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        SettledAmount = 0,
                                        SourcePurchaseRequestPaymentId = detail.Id,
                                        ToPurchaseOrderId = detail.PurchaseOrderId,
                                        TransferredAmount = detail.PaidAmount,
                                        IsTDSDeducted = TDSApplyOnAdvancePayment
                                    };
                                    context.PrPaymentTransferDB.Add(prPaymentTransfer);


                                }
                                else
                                {
                                    PRPaymentAppoved prPaymentAppoved = context.PRPaymentAppoved.First(x => x.PRId == item.PurchaseOrderId && x.IsActive == true && x.IsApproved == true);
                                    prPaymentAppoved.IsPaymentDone = true;
                                    prPaymentAppoved.ModifiedBy = userid;
                                    prPaymentAppoved.ModifiedDate = currentTime;
                                }
                                context.Commit();

                            }
                        }

                        scope.Complete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw ex;
                    }
                }
            }
            else
            {
                return false;
            }
        }


        [Route("PaymentConfirm")]
        [HttpPost]
        public POResult Confirmpayment(ConfirmPayment confirm)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            POResult pOResult = new POResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                People pdata = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                var PRPayment = db.PRPaymentAppoved.Where(x => x.PRId == confirm.PurchaseOrderId).FirstOrDefault();
                if (PRPayment != null)
                {
                    PRPayment.Comment = confirm.Comment;
                    PRPayment.IsPaymentDone = confirm.IsPaymentDone;
                    PRPayment.ModifiedBy = pdata.PeopleID;
                    PRPayment.ModifiedDate = DateTime.Now;
                    db.Entry(PRPayment).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        pOResult.Status = true;
                        pOResult.Message = "PR Payment Confirmed";
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "Something went Wrong";
                }

            }
            return pOResult;
        }


        [Route("GetBySummaryId/{prPaymentSummaryId}")]
        [HttpGet]
        public List<PurchaseRequestPaymentDC> GetBySummaryId(int prPaymentSummaryId)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var query = from prd in context.PurchaseRequestPaymentsDB
                                join wh in context.Warehouses on prd.WarehouseId equals wh.WarehouseId
                                join sup in context.Suppliers on prd.SupplierId equals sup.SupplierId
                                where
                               prd.PRPaymentSummaryId == prPaymentSummaryId
                                && prd.IsActive == true && prd.Deleted == false
                                && prd.PrPaymentStatus == "Pending"
                                select new PurchaseRequestPaymentDC
                                {
                                    BankId = prd.BankId,
                                    PurchaseOrderId = prd.PurchaseOrderId,
                                    BankName = prd.BankName,
                                    CreatedDate = prd.CreatedDate,
                                    Id = prd.Id,
                                    PRList = prd.PRList,
                                    RefNo = prd.RefNo,
                                    Remark = prd.Remark,
                                    SupplierId = prd.SupplierId,
                                    TotalAmount = prd.TotalAmount,
                                    SupplierCodes = sup.SUPPLIERCODES,
                                    PaymentDate = prd.PaymentDate,
                                    IsPROutstandingPending = prd.IsPROutstandingPending,
                                    PrPaymentStatus = prd.PrPaymentStatus,
                                    SupplierName = prd.SupplierName,
                                    PaidAmount = prd.PaidAmount,
                                    WarehouseName = wh.WarehouseName,
                                    Bank_Ifsc = sup.Bank_Ifsc,
                                    Bank_AC_No = sup.Bank_AC_No
                                };
                    var list = query.ToList();
                    return list;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Route("GetAllBySummaryId/{prPaymentSummaryId}")]
        [HttpGet]
        public List<PurchaseRequestPaymentDC> GetAllBySummaryId(int prPaymentSummaryId)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var query = from prd in context.PurchaseRequestPaymentsDB
                                join wh in context.Warehouses on prd.WarehouseId equals wh.WarehouseId
                                join sup in context.Suppliers on prd.SupplierId equals sup.SupplierId
                                where
                                prd.PRPaymentSummaryId == prPaymentSummaryId
                                && prd.IsActive == true && prd.Deleted == false
                                select new PurchaseRequestPaymentDC
                                {
                                    BankId = prd.BankId,
                                    PurchaseOrderId = prd.PurchaseOrderId,
                                    BankName = prd.BankName,
                                    CreatedDate = prd.CreatedDate,
                                    Id = prd.Id,
                                    PRList = prd.PRList,
                                    RefNo = prd.RefNo,
                                    Remark = prd.Remark,
                                    SupplierId = prd.SupplierId,
                                    TotalAmount = prd.TotalAmount,
                                    SupplierCodes = sup.SUPPLIERCODES,
                                    PaymentDate = prd.PaymentDate,
                                    IsPROutstandingPending = prd.IsPROutstandingPending,
                                    PrPaymentStatus = prd.PrPaymentStatus,
                                    SupplierName = prd.SupplierName,
                                    PaidAmount = prd.PaidAmount,
                                    WarehouseName = wh.WarehouseName,
                                    TDSAmount = prd.TDSAmount
                                };
                    var list = query.ToList();
                    return list;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        [Route("UpdatePRPayment")]
        [HttpPost]
        public bool UpdatePRPayment(List<PurchaseRequestPayment> detailList)
        {
            int userid = GetLoginUserId();
            bool retVal = false;
            long summaryId = 0;
            PurchaseRequestPayment lastdetail = null;
            if (detailList != null && detailList.Any())
            {

                try
                {
                    using (var context = new AuthContext())
                    {
                        foreach (PurchaseRequestPayment detail in detailList)
                        {
                            lastdetail = detail;
                            PurchaseRequestPayment dbDetail = context.PurchaseRequestPaymentsDB.FirstOrDefault(x => x.Id == detail.Id);
                            summaryId = dbDetail.PRPaymentSummaryId;

                            PRPaymentSummary pmtsummary = context.PRPaymentSummaryDB.FirstOrDefault(x => x.Id == summaryId);

                            if (detail.PrPaymentStatus == "Approved" && dbDetail.PrPaymentStatus == "Pending")
                            {

                                dbDetail.PaymentDate = detail.PaymentDate;
                                long supplierLedgerId = 0;
                                var supplierLedger = context.LadgerDB.FirstOrDefault(x => x.ObjectID == detail.SupplierId && x.ObjectType == "Supplier");
                                if (supplierLedger != null)
                                {
                                    supplierLedgerId = supplierLedger.ID;
                                }
                                else
                                {
                                    LadgerHelper ladgerHelper = new LadgerHelper();
                                    Ladger ledger
                                        = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", detail.SupplierId, userid, context);
                                    supplierLedgerId = ledger.ID;
                                }
                                var tdsLedgerID = context.LadgerDB.FirstOrDefault(x => x.Name == "TDS").ID;
                                LadgerEntry debitEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = detail.BankId,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Date = dbDetail.PaymentDate,
                                    UpdatedDate = DateTime.Now,
                                    Credit = null,
                                    Debit = dbDetail.PaidAmount,
                                    ObjectID = Convert.ToInt32(detail.Id),
                                    ObjectType = "PR",
                                    IsSupplierAdvancepay = true,
                                    Particulars = detail.PoInvoiceNo,
                                    RefNo = detail.RefNo,
                                    Remark = detail.Remark,
                                    VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                                    VouchersNo = null,
                                    LagerID = supplierLedgerId,
                                    UploadGUID = dbDetail.Guid,
                                    UpdatedBy = userid,
                                    PRPaymentId = detail.Id
                                };
                                context.LadgerEntryDB.Add(debitEntry);

                                LadgerEntry creditEntry = new LadgerEntry
                                {
                                    Active = true,
                                    AffectedLadgerID = supplierLedgerId,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Date = dbDetail.PaymentDate,
                                    UpdatedDate = DateTime.Now,
                                    Credit = dbDetail.PaidAmount,
                                    Debit = null,
                                    ObjectID = Convert.ToInt32(detail.Id),
                                    ObjectType = "PR",
                                    IsSupplierAdvancepay = true,
                                    Particulars = detail.PoInvoiceNo,
                                    RefNo = detail.RefNo,
                                    Remark = detail.Remark,
                                    VouchersTypeID = debitEntry.VouchersTypeID,
                                    VouchersNo = null,
                                    LagerID = detail.BankId,
                                    UploadGUID = dbDetail.Guid,
                                    UpdatedBy = userid,
                                    PRPaymentId = detail.Id
                                };
                                context.LadgerEntryDB.Add(creditEntry);

                                #region TDS entry
                                if (dbDetail.TDSAmount > 0)
                                {
                                    LadgerEntry debitEntrytds = new LadgerEntry
                                    {
                                        Active = true,
                                        AffectedLadgerID = tdsLedgerID,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        Date = dbDetail.PaymentDate,
                                        UpdatedDate = DateTime.Now,
                                        Credit = null,
                                        Debit = dbDetail.TDSAmount,
                                        ObjectID = Convert.ToInt32(detail.Id),
                                        ObjectType = "PR",
                                        IsSupplierAdvancepay = true,
                                        Particulars = "POId-" + detail.PurchaseOrderId.ToString(),
                                        RefNo = detail.RefNo,
                                        Remark = detail.Remark,
                                        VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "Tax").First().ID,
                                        VouchersNo = null,
                                        LagerID = supplierLedgerId,
                                        UploadGUID = dbDetail.Guid,
                                        UpdatedBy = userid,
                                        PRPaymentId = detail.Id
                                    };
                                    context.LadgerEntryDB.Add(debitEntrytds);

                                    LadgerEntry creditEntrytds = new LadgerEntry
                                    {
                                        Active = true,
                                        AffectedLadgerID = supplierLedgerId,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        Date = dbDetail.PaymentDate,
                                        UpdatedDate = DateTime.Now,
                                        Credit = dbDetail.TDSAmount,
                                        Debit = null,
                                        ObjectID = Convert.ToInt32(detail.Id),
                                        ObjectType = "PR",
                                        IsSupplierAdvancepay = true,
                                        Particulars = "POId-" + detail.PurchaseOrderId.ToString(),// detail.PoInvoiceNo,
                                        RefNo = detail.RefNo,
                                        Remark = detail.Remark,
                                        VouchersTypeID = debitEntrytds.VouchersTypeID,
                                        VouchersNo = null,
                                        LagerID = tdsLedgerID,
                                        UploadGUID = dbDetail.Guid,
                                        UpdatedBy = userid,
                                        PRPaymentId = detail.Id
                                    };
                                    context.LadgerEntryDB.Add(creditEntrytds);
                                }
                                #endregion


                                //context.Commit();

                                //entry in purchaserequestpayment
                                dbDetail.RefNo = detail.RefNo;
                                dbDetail.Remark = detail.Remark;
                                dbDetail.IsPROutstandingPending = false;
                                dbDetail.PrPaymentStatus = "Approved";
                                context.Commit();

                                //entry in summary
                                // summary.IsPROutstandingPending = false;
                                //context.Commit();

                                retVal = this.finalPRPayment(dbDetail.PurchaseOrderId, dbDetail.PaidAmount, context);
                                if (retVal == false)
                                {
                                    //scope.Dispose();
                                    return false;
                                }
                            }

                            else if (detail.PrPaymentStatus == "Rejected" && dbDetail.PrPaymentStatus == "Pending")
                            {

                                dbDetail.PrPaymentStatus = detail.PrPaymentStatus;
                                dbDetail.IsPROutstandingPending = false;
                                //context.Commit();

                                //dbDetail.IsPROutstandingPending = true;
                                //dbDetail.PrPaymentStatus = "Rejected";
                                //context.Commit();

                                // summary.IsPROutstandingPending = true;
                                // context.Commit();
                                retVal = false;
                            }

                            else
                            {
                                //scope.Dispose();
                                return false;
                            }
                        }
                        PRPaymentSummary summary = context.PRPaymentSummaryDB.FirstOrDefault(x => x.Id == summaryId);
                        summary.IsPROutstandingPending = false;

                        context.Commit();

                        //scope.Complete();
                        return true;
                    }

                }
                catch (Exception e)
                {
                    //scope.Dispose();
                }

            }
            return false;
        }

        [Route("RejectPRPayment/{purchaseRequestPaymentId}")]
        [HttpGet]
        public PurchaseRequestPaymentApiResponse<string> RejectPRPayment(long purchaseRequestPaymentId)
        {
            PurchaseRequestPaymentApiResponse<string> response = new PurchaseRequestPaymentApiResponse<string>();
            try
            {
                using (var authContext = new AuthContext())
                {
                    PurchaseRequestPayment payment
                        = authContext.PurchaseRequestPaymentsDB.FirstOrDefault(x => x.Id == purchaseRequestPaymentId);

                    if (payment != null && payment.Deleted == false && payment.IsActive == true && payment.PrPaymentStatus == "Pending")
                    {
                        payment.PrPaymentStatus = "Rejected";
                        payment.IsPROutstandingPending = false;
                        authContext.Commit();
                        response.IsSuccess = true;
                        response.ErrorMessage = "";
                        return response;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ErrorMessage = "Payment already rejected!!!!";
                        return response;
                    }
                }
            }
            catch
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Something went wrong, please try after some time!!!!";
                return response;
            }
        }

        public bool finalPRPayment(long purchaseorderid, int PaidAmount, AuthContext db)
        {
            bool retVal = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            var pRPaymentAppoved = db.PRPaymentAppoved.Where(x => x.PRId == purchaseorderid && x.IsActive == true).FirstOrDefault();
            var POMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == purchaseorderid).FirstOrDefault();
            var POItemDetails = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == purchaseorderid).ToList();

            pRPaymentAppoved.IsPaymentDone = true;
            pRPaymentAppoved.PaymentApprovedDate = DateTime.Now;
            db.Entry(pRPaymentAppoved).State = EntityState.Modified;

            //POMaster.PRStatus = 5;

            double advanceAmount = POMaster.Advance_Amt;
            POMaster.PRType = 0;
            //POMaster.Advance_Amt = PaidAmount;

            db.Entry(POMaster).State = EntityState.Modified;

            if (db.Commit() > 0)
            {
                //PurchaseRequestMaster pm = new PurchaseRequestMaster();
                //pm.SupplierId = POMaster.SupplierId;
                //pm.SupplierName = POMaster.SupplierName;
                //pm.CreationDate = DateTime.Now;
                //pm.WarehouseId = POMaster.WarehouseId;
                //pm.CompanyId = POMaster.CompanyId;
                //pm.WarehouseName = POMaster.WarehouseName;
                //pm.PoType = "Manual";
                //pm.ETotalAmount = POMaster.ETotalAmount;
                //pm.BuyerId = POMaster.BuyerId;
                //pm.BuyerName = POMaster.BuyerName;
                //pm.Active = true;
                //pm.IsCashPurchase = POMaster.IsCashPurchase;
                //pm.CashPurchaseName = POMaster.CashPurchaseName;
                //pm.Advance_Amt = advanceAmount;
                //pm.DepoId = POMaster.DepoId;
                //pm.DepoName = POMaster.DepoName;
                //pm.CreatedBy = POMaster.CreatedBy;
                //pm.PurchaseOrderId = POMaster.PurchaseOrderId;
                //pm.PoInvoiceNo = POMaster.PoInvoiceNo;
                //pm.PurchaseOrderRequestDetail = new List<PurchaseOrderRequestDetail>();

                //foreach (var data in POItemDetails)
                //{
                //    PurchaseOrderRequestDetail pd = new PurchaseOrderRequestDetail();
                //    pd.ItemId = data.ItemId;
                //    pd.ItemNumber = data.ItemNumber;
                //    pd.itemBaseName = data.itemBaseName;
                //    pd.ItemMultiMRPId = data.ItemMultiMRPId;
                //    pd.HSNCode = data.HSNCode;
                //    pd.MRP = data.MRP;
                //    pd.SellingSku = data.SellingSku;
                //    pd.ItemName = data.ItemName;
                //    pd.PurchaseQty = data.PurchaseQty;
                //    pd.CreationDate = DateTime.Now;
                //    pd.Status = "ordered";
                //    pd.MOQ = data.MOQ;
                //    pd.Price = data.Price;
                //    pd.WarehouseId = data.WarehouseId;
                //    pd.CompanyId = data.CompanyId;
                //    pd.WarehouseName = data.WarehouseName;
                //    pd.SupplierId = data.SupplierId;
                //    pd.SupplierName = data.SupplierName;
                //    pd.TotalQuantity = data.TotalQuantity;
                //    pd.PurchaseName = data.PurchaseName;
                //    pd.PurchaseSku = data.PurchaseSku;
                //    pd.DepoId = data.DepoId;
                //    pd.DepoName = data.DepoName;
                //    pd.ConversionFactor = data.ConversionFactor;
                //    pd.PurchaseOrderId = data.PurchaseOrderId;
                //    pm.PurchaseOrderRequestDetail.Add(pd);

                //}
                // db.PurchaseRequestMasterDB.Add(pm);
                //db.Commit();
                try
                {

                    Sms s = new Sms();
                    string msg = " ShopKirana " + Environment.NewLine + " PO id: " + POMaster.PurchaseOrderId + " Advance Amount Received. PR Converted to PO";
                    string Mob = db.Peoples.Where(q => q.PeopleID == POMaster.Approval1).Select(q => q.Mobile).FirstOrDefault();
                    if (Mob != null) { s.sendOtp(Mob, msg, ""); }
                }
                catch (Exception ex)
                {

                }

                retVal = true;

            }
            return retVal;
        }

        [Route("GetSupplierPRPaymentExport/{prPaymentSummaryId}")]
        [HttpGet]
        public List<SupplierPaymentExportDc> GetSupplierPRPaymentExport(long prPaymentSummaryId)
        {
            SupplierPaymentHelper helper = new SupplierPaymentHelper();
            return helper.GetSupplierPRPaymentExport(prPaymentSummaryId);
        }

        [Route("GetSupplierIRPaymentExport/{irPaymentSummaryId}")]
        [HttpGet]
        public List<SupplierPaymentExportDc> GetSupplierIRPaymentExport(long irPaymentSummaryId)
        {
            SupplierPaymentHelper helper = new SupplierPaymentHelper();
            return helper.GetSupplierIRPaymentExport(irPaymentSummaryId);
        }

        [Route("updatebankname")]
        [HttpGet]
        public string UpdateBankName(int Id, int BankId)
        {
            try
            {
                string result = "";
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);
                using (var context = new AuthContext())
                {
                    var dataa = context.LadgerDB.Where(x => x.LadgertypeID == 7 && x.ID == BankId).FirstOrDefault();
                    var data = context.PurchaseRequestPaymentsDB.Where(x => x.Id == Id).FirstOrDefault();
                    if (data != null)
                    {
                        var alldata = context.PurchaseRequestPaymentsDB.Where(x => x.PRPaymentSummaryId == data.PRPaymentSummaryId && x.IsActive == true && x.Deleted == false && x.IsPROutstandingPending == true && x.PrPaymentStatus == "Pending").ToList();
                        if (alldata.Count > 0)
                        {
                            foreach(var d in alldata)
                            {
                                d.BankId = BankId;
                                d.BankName = dataa.Name + " - " + dataa.Alias;
                                d.Updateby = userid;
                                d.UpdateDate = DateTime.Now;

                                //data.BankName = BankName;
                                context.Entry(d).State = EntityState.Modified;
                                context.Commit();
                                result= "Updated";
                            }
                           
                        }
                        else
                        {
                            result = "Data Not Exists";
                        }
                        
                    }
                    else
                    {
                        result= "Data Not Exists";
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }

        [Route("GetBankNumberByPurchaseOrderId")]
        [HttpGet]
        public string GetBankNumber(int PurchaseOrderId)
        {
            string result = null;
            using (var db = new AuthContext())
            {
                var purchaseorderdata = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.IsDeleted == false).FirstOrDefault();
                if (purchaseorderdata.DepoId > 0)
                {
                    var data = db.DepoMasters.Where(x => x.DepoId == purchaseorderdata.DepoId && x.IsActive == true && x.Deleted == false).FirstOrDefault();
                    if (data.Bank_AC_No != "")
                    {
                        result = data.Bank_AC_No;
                    }
                    else
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }


    }



    public class PurchaseRequestPaymentApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T SuccessResponseObject { get; set; }
        public List<T> SuccessResponseList { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class POApprovalPager
    {
        public List<PurchaseOrderMasterDTO> PrList { get; set; }
        public int Count { get; set; }
    }

    public class PurchaseRequestPaymentsPager
    {
        public List<PurchaseRequestPaymentDC> PrList { get; set; }
        public int Count { get; set; }
    }

    public class PoApprovalPaginator
    {
        public int? WarehouseId { get; set; }
        public string Search { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SkipCount { get; set; }
        public int Take { get; set; }
    }

    public class PurchaseRequestPaymentDC
    {
        public long Id { get; set; }
        public int SupplierId { get; set; }
        public long BankId { get; set; }
        public long PurchaseOrderId { get; set; }
        public string BankName { get; set; }
        public string RefNo { get; set; }
        public int TotalAmount { get; set; }
        public string Remark { get; set; }
        public string PRList { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Guid { get; set; }
        public bool Deleted { get; set; }
        public bool IsActive { get; set; }
        public int? Createby { get; set; }
        public int? Updateby { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? WarehouseId { get; set; }
        public int PRPaymentSummaryId { get; set; }
        public string PoInvoiceNo { get; set; }

        public string SupplierName { get; set; }
        public string SupplierCodes { get; set; }

        public bool? IsPROutstandingPending { get; set; }
        public string PrPaymentStatus { get; set; }

        public int? PaidAmount { get; set; }
        public string WarehouseName { get; set; }
        public double TDSAmount { get; set; }
        public string Bank_Ifsc { get; set; }
        public string Bank_AC_No { get; set; }
    }

    public class PRPaymentDc
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public long PRPaymentTransferId { get; set; }
        public double Total { get; set; }
        public long SourcePurchaseRequestPaymentId { get; set; }
        public bool check { get; set; }
        public bool IsTDSDeducted { get; set; }

    }


    public class SupplierPaymentssDC
    {
        public long SupplierId { get; set; }
        public double PaymentTillDate { get; set; }

    }

}
