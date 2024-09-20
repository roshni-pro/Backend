using AngularJSAuthentication.API.Controllers;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using AngularJSAuthentication.Model.PurchaseOrder;

namespace AngularJSAuthentication.API.Helper
{
    public class SupplierLedgerHelper
    {
        public static void UpdateSupplierLedger()
        {
            using (var authContext = new AuthContext())
            {
                try
                {
                    DateTime sd = Convert.ToDateTime("04/01/2019");
                    DateTime ed = Convert.ToDateTime("06/30/2019");
                    //var query = from irm in authContext.IRMasterDB
                    //            join ii in authContext.InvoiceImageDb
                    //            on irm.IRID equals ii.InvoiceNumber
                    //            join pom in authContext.DPurchaseOrderMaster
                    //            on ii.PurchaseOrderId equals pom.PurchaseOrderId
                    //            where (irm.IRStatus == "Approved from Buyer side" || irm.IRStatus == "Paid") && irm.supplierId == 167 && ii.InvoiceDate >= sd && ii.InvoiceDate <= ed
                    //            && irm.Discount > 0
                    //            //&& pom.SupplierId==99
                    //            select new SupplierLedgerViewModel
                    //            {
                    //                supplierId = irm.supplierId,
                    //                Gstamt = irm.Gstamt,
                    //                InvoiceDate = ii.InvoiceDate,
                    //                InvoiceNumber = ii.InvoiceNumber,
                    //                IRID = irm.Id,
                    //                Discount = irm.Discount,
                    //                TotalAmount = irm.TotalAmount
                    //            };

                    var query = @"select distinct 
                                        IRM.ID, 
		                                cast(II.InvoiceDate as date) InvoiceDate, 
		                                IRM.ID as IRID,
		                                irm.supplierId,  
		                                irm.Gstamt,
		                                ii.InvoiceNumber,
		                                irm.Discount,
		                                irm.TotalAmount
                                from IRMasters IRM
                                left join InvoiceImages II
                                on II.InvoiceNumber = IRM.IRID
                                and II.PurchaseOrderId = IRM.PurchaseOrderId
                                where IRM.IRStatus = 'Pending from Buyer side'
                                and Month(II.InvoiceDate) > 3 and Month(II.InvoiceDate) < 7";

                    //List<IRMaster> irMasterList = authContext.IRMasterDB.Where(x => x.PaymentStatus == "Paid").ToList();
                    List<SupplierLedgerViewModel> irMasterList = authContext.Database.SqlQuery<SupplierLedgerViewModel>(query).ToList();
                    foreach (var irMaster in irMasterList)
                    {
                        IRMaster IRR = authContext.IRMasterDB.Where(a => a.Id == irMaster.IRID).SingleOrDefault();

                        var amount = (irMaster.TotalAmount ?? 0) + (irMaster.Discount ?? 0) - (irMaster.Gstamt ?? 0);

                        if (IRR.OtherAmount > 0)
                        {
                            if (IRR.OtherAmountType == "ADD")
                            {
                                amount = amount - IRR.OtherAmount ?? 0;
                            }
                            else if (IRR.OtherAmountType == "MINUS")
                            {
                                amount = amount + IRR.OtherAmount ?? 0;
                            }
                        }
                        if (IRR.ExpenseAmount > 0)
                        {
                            if (IRR.ExpenseAmountType == "ADD")
                            {
                                amount = amount - IRR.ExpenseAmount ?? 0;
                            }
                            else if (IRR.ExpenseAmountType == "MINUS")
                            {
                                amount = amount + IRR.ExpenseAmount ?? 0;
                            }
                        }
                        if (IRR.RoundofAmount > 0)
                        {
                            if (IRR.RoundoffAmountType == "ADD")
                            {
                                amount = amount - IRR.RoundofAmount ?? 0;
                            }
                            else if (IRR.RoundoffAmountType == "MINUS")
                            {
                                amount = amount + IRR.RoundofAmount ?? 0;
                            }
                        }

                        IRHelper.UpdateLedgerEntry(irMaster.supplierId, authContext, amount, irMaster.Gstamt, 1, irMaster.InvoiceDate, irMaster.InvoiceNumber, irMaster.IRID, (irMaster.Discount ?? 0), IRR);

                        //UpdateLedgerEntry
                        //IRHelper.DebitLedgerEntry(irMaster.supplierId, authContext, irMaster.TotalAmount, irMaster.InvoiceNumber, irMaster.IRID, 1, irMaster.Discount);
                    }
                }
                catch (Exception ex)
                {
                }
            }


        }


        public static string GenerateSupplierPayment()
        {
            string filepath = string.Empty;
            string returnfilepath = string.Empty;
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "SupplierPayment.rdlc");

                // Variables
                Microsoft.Reporting.WebForms.Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                // Setup the report viewer object and get the array of bytes
                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = path;
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                viewer.LocalReport.SetParameters(reportParameters);
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.DataSources.Clear();
                DataTable dt = new DataTable("Payment");
                viewer.LocalReport.DataSources.Add(new ReportDataSource("Payment", dt));
                dt = null;
                double debitSum = 0, creditSum = 0;
                viewer.LocalReport.Refresh();
                reportParameters = new ReportParameterCollection();


                reportParameters.Add(new ReportParameter("DebitSum", debitSum.ToString()));
                reportParameters.Add(new ReportParameter("CreditSum", creditSum.ToString()));


                //viewer.LocalReport.SetParameters(reportParameters);



                string fileExtension = ".xls";
                string reportType = "Excel";


                byte[] bytes = viewer.LocalReport.Render(reportType, null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                //string filename = "customerLedger_" + DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss") + fileExtension;
                //filepath = HttpContext.Current.Server.MapPath(@"~\BankWithdrawDepositFile\") + filename;

                var FileName = "supplierPayemnt_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fileExtension;
                var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);


                var fullPhysicalPath = folderPath + "\\" + FileName;
                var fileUrl = "/ReportDownloads" + "/" + FileName;

                FileStream file = File.Create(fullPhysicalPath);


                file.Write(bytes, 0, bytes.Length);
                file.Close();
                returnfilepath = fileUrl;
            }
            catch (Exception ex)
            {
                //logger.Error("Error in Generatepdf Method: " + ex.Message);
            }

            return returnfilepath;
        }
    }


    public class SupplierLedgerViewModel
    {
        public int supplierId { get; set; }
        public double? Gstamt { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int IRID { get; set; }
        public double? Discount { get; set; }
        public double? TotalAmount { get; set; }
    }

}



//public static void GRList(int month, int year, int day)
//{
//    using (var authContext = new AuthContext())
//    {
//        var ledgerList = authContext.LadgerDB.ToList();
//        var monthParam = new SqlParameter("@Month", month);
//        var yearParam = new SqlParameter("@year", year);
//        var dayParam = new SqlParameter("@Day", day);

//        var ledgerTypeList = authContext.LadgerTypeDB.ToList();
//        var voucherTypelList = authContext.VoucherTypeDB.ToList();
//        var purchaseLedgerTypeID = ledgerTypeList.First(y => y.code.ToLower() == "purchase").ID;
//        var purchaseLedgerID = ledgerList.Where(x => x.LadgertypeID == purchaseLedgerTypeID).First().ID;


//        var poLedgerViewModelList = authContext.Database.SqlQuery<POLedgerViewModel>("POLedgerUpdate @Day,@Month,@year", dayParam, monthParam, yearParam).ToList();


//        if (poLedgerViewModelList != null && poLedgerViewModelList.Count > 0)
//        {
//            List<LadgerEntry> ladgerEntryList = new List<LadgerEntry>();
//            var purchaseVoucherID = voucherTypelList.Where(x => x.Name.ToLower() == "purchase").First().ID;
//            foreach (POLedgerViewModel vm in poLedgerViewModelList)
//            {
//                var supplierLedgerID = ledgerList.Where(x => x.ObjectID == vm.SupplierId && x.LadgertypeID == ledgerTypeList.Where(y => y.code.ToLower() == "supplier").First().ID).First().ID;

//                if (vm.GR1_Date != null && vm.GR1_Amount > 0)
//                {
//                    var amount = (vm.GR1_Amount ?? 0) - (vm.discount1 ?? 0);
//                    var ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderID", "", null, amount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR1PersonID);
//                    ledgerEntry.Date = vm.GR1_Date;
//                    ladgerEntryList.Add(ledgerEntry);


//                    ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PrchaseOrderID", "", amount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR1PersonID);
//                    ledgerEntry.Date = vm.GR1_Date;
//                    ladgerEntryList.Add(ledgerEntry);
//                }

//                if (vm.GR2_Date != null && vm.GR2_Amount > 0)
//                {
//                    var amount = (vm.GR2_Amount ?? 0) - (vm.discount2 ?? 0);
//                    var ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderID", "", null, amount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR2PersonID);
//                    ledgerEntry.Date = vm.GR2_Date;
//                    ladgerEntryList.Add(ledgerEntry);


//                    ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PrchaseOrderID", "", amount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR2PersonID);
//                    ledgerEntry.Date = vm.GR2_Date;
//                    ladgerEntryList.Add(ledgerEntry);
//                }

//                if (vm.GR3_Date != null && vm.GR3_Amount > 0)
//                {
//                    var amount = (vm.GR3_Amount ?? 0) - (vm.discount3 ?? 0);
//                    var ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderID", "", null, amount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR3PersonID);
//                    ledgerEntry.Date = vm.GR3_Date;
//                    ladgerEntryList.Add(ledgerEntry);


//                    ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PrchaseOrderID", "", amount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR3PersonID);
//                    ledgerEntry.Date = vm.GR3_Date;
//                    ladgerEntryList.Add(ledgerEntry);
//                }

//                if (vm.GR4_Date != null && vm.GR4_Amount > 0)
//                {
//                    var amount = (vm.GR4_Amount ?? 0) - (vm.discount4 ?? 0);
//                    var ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderID", "", null, amount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR4PersonID);
//                    ledgerEntry.Date = vm.GR4_Date;
//                    ladgerEntryList.Add(ledgerEntry);


//                    ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PrchaseOrderID", "", amount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR4PersonID);
//                    ledgerEntry.Date = vm.GR4_Date;
//                    ladgerEntryList.Add(ledgerEntry);
//                }

//                if (vm.GR5_Date != null && vm.GR5_Amount > 0)
//                {
//                    var amount = (vm.GR5_Amount ?? 0) - (vm.discount5 ?? 0);
//                    var ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderID", "", null, amount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR5PersonID);
//                    ledgerEntry.Date = vm.GR5_Date;
//                    ladgerEntryList.Add(ledgerEntry);


//                    ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PrchaseOrderID", "", amount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, vm.GR5PersonID);
//                    ledgerEntry.Date = vm.GR5_Date;
//                    ladgerEntryList.Add(ledgerEntry);
//                }
//            }


//            if (ladgerEntryList != null && ladgerEntryList.Any())
//            {
//                authContext.LadgerEntryDB.AddRange(ladgerEntryList);
//                authContext.SaveChanges();

//            }
//        }

//    }
//}


//public static void IRList(int month, int year, int day)
//{
//    using (var authContext = new AuthContext())
//    {
//        List<LadgerEntry> ladgerEntryList = new List<LadgerEntry>();
//        var ledgerList = authContext.LadgerDB.ToList();
//        var monthParam = new SqlParameter("@Month", month);
//        var yearParam = new SqlParameter("@year", year);
//        var dayParam = new SqlParameter("@Day", day);

//        var ledgerTypeList = authContext.LadgerTypeDB.ToList();
//        var voucherTypelList = authContext.VoucherTypeDB.ToList();
//        var purchaseLedgerTypeID = ledgerTypeList.First(y => y.code.ToLower() == "purchase").ID;
//        var purchaseLedgerID = ledgerList.Where(x => x.LadgertypeID == purchaseLedgerTypeID).First().ID;

//        var grLedgerViewModelList = authContext.Database.SqlQuery<IRLedgerViewModel>("GRLedgerUpdate @Day,@Month,@year", dayParam, monthParam, yearParam).ToList();
//        if (grLedgerViewModelList != null && grLedgerViewModelList.Count > 0)
//        {
//            var purchaseVoucherID = voucherTypelList.Where(x => x.Name.ToLower() == "purchase").First().ID;
//            foreach (IRLedgerViewModel vm in grLedgerViewModelList)
//            {
//                var supplierLedgerID = ledgerList.Where(x => x.ObjectID == vm.SupplierId && x.LadgertypeID == ledgerTypeList.Where(y => y.code.ToLower() == "supplier").First().ID).First().ID;
//                var ledgerEntry = CreateLadgerEntry(supplierLedgerID, "PurchaseOrderId", vm.InVoiceNumber, vm.CreditInVoiceAmount, null, purchaseLedgerID, purchaseVoucherID, vm.PurchaseOrderId, null);
//                ledgerEntry.Date = vm.CreatedDate;
//                ladgerEntryList.Add(ledgerEntry);


//                ledgerEntry = CreateLadgerEntry(purchaseLedgerID, "PurchaseOrderId", vm.InVoiceNumber, null, vm.CreditInVoiceAmount, supplierLedgerID, purchaseVoucherID, vm.PurchaseOrderId, null);
//                ledgerEntry.Date = vm.CreatedDate;
//                ladgerEntryList.Add(ledgerEntry);
//            }
//        }


//        if (ladgerEntryList != null && ladgerEntryList.Any())
//        {
//            authContext.LadgerEntryDB.AddRange(ladgerEntryList);
//            authContext.SaveChanges();

//        }
//    }
//}


//private static LadgerEntry CreateLadgerEntry(long ledgerID, string objectType, string Particulars, double? credit, double? debit, long? affectedLedgerID, long? voucherTypeID, long? objectID, long? voucherNo)
//{

//    LadgerEntry entry = new LadgerEntry();
//    entry.LagerID = ledgerID;
//    entry.ObjectID = objectID;
//    entry.ObjectType = objectType;
//    entry.Particulars = Particulars;
//    entry.Credit = credit;
//    entry.Debit = debit;
//    entry.AffectedLadgerID = affectedLedgerID;
//    entry.VouchersTypeID = voucherTypeID;
//    entry.CreatedBy = 9999;
//    entry.CreatedDate = DateTime.Now;
//    entry.UpdatedBy = 9999;
//    entry.UpdatedDate = DateTime.Now;
//    entry.VouchersNo = voucherNo;
//    entry.Active = true;
//    return entry;
//}
