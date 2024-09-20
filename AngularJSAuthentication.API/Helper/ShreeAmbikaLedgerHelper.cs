using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class ShreeAmbikaLedgerHelper
    {
        public static void InsertFromExcel()
        {

            try
            {

                string query = "SELECT * FROM ImportSupplier";

                using (var authContext = new AuthContext())
                {
                    List<ShreeAmbikaLedgerDTO> list = authContext.Database.SqlQuery<ShreeAmbikaLedgerDTO>(query).ToList();

                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            long ladgerID = 99;
                            long bankLadgerID = authContext.LadgerDB.Where(x => x.LadgertypeID != 1 && x.Name == "Bank").First().ID;
                            long purchaseDiscountLadgerID = authContext.LadgerDB.Where(x => x.LadgertypeID != 1 && x.Name == "PurchaseDiscount").First().ID;
                            long paymentVoucherTypeID = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == "Payment").ID;
                            long journalVoucherTypeID = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == "Journal").ID;

                            //IRHelper.DebitLedgerEntry(99, authContext, irMaster.TotalAmount, irMaster.InvoiceNumber, irMaster.IRID, 1, irMaster.Discount);
                            LadgerEntry ladgerEntry = new LadgerEntry();
                            LadgerEntry aftladgerEntry = new LadgerEntry();
                            ladgerEntry.Active = true;
                            if (item.VchType.Trim() == "Payment")
                            {
                                ladgerEntry.AffectedLadgerID = bankLadgerID;
                                ladgerEntry.VouchersTypeID = paymentVoucherTypeID;
                                ladgerEntry.Particulars = "Payment " + item.Debit;

                                ladgerEntry.LagerID = ladgerID;
                                ladgerEntry.CreatedBy = 1;
                                ladgerEntry.CreatedDate = DateTime.Now;
                                ladgerEntry.UpdatedBy = 1;
                                ladgerEntry.UpdatedDate = DateTime.Now;
                                ladgerEntry.Date = item.Date;
                                ladgerEntry.Credit = null;
                                ladgerEntry.Debit = item.Debit;
                                ladgerEntry.ObjectType = "Import";
                                authContext.LadgerEntryDB.Add(ladgerEntry);

                                aftladgerEntry.AffectedLadgerID = ladgerID;
                                aftladgerEntry.VouchersTypeID = paymentVoucherTypeID;
                                aftladgerEntry.Particulars = "Payment " + item.Debit;

                                aftladgerEntry.LagerID = bankLadgerID;
                                aftladgerEntry.CreatedBy = 1;
                                aftladgerEntry.CreatedDate = DateTime.Now;
                                aftladgerEntry.UpdatedBy = 1;
                                aftladgerEntry.UpdatedDate = DateTime.Now;
                                aftladgerEntry.Date = item.Date;
                                aftladgerEntry.Credit = null;
                                aftladgerEntry.Credit = item.Debit;
                                aftladgerEntry.ObjectType = "Import";

                                authContext.LadgerEntryDB.Add(aftladgerEntry);


                            }
                            else
                            {
                                ladgerEntry.AffectedLadgerID = purchaseDiscountLadgerID;
                                ladgerEntry.VouchersTypeID = journalVoucherTypeID;
                                ladgerEntry.Particulars = "Discount " + item.Debit;

                                ladgerEntry.LagerID = ladgerID;
                                ladgerEntry.CreatedBy = 1;
                                ladgerEntry.CreatedDate = DateTime.Now;
                                ladgerEntry.UpdatedBy = 1;
                                ladgerEntry.UpdatedDate = DateTime.Now;
                                ladgerEntry.Date = item.Date;
                                ladgerEntry.Credit = null;
                                ladgerEntry.Debit = item.Debit;
                                ladgerEntry.ObjectType = "Import";
                                authContext.LadgerEntryDB.Add(ladgerEntry);

                                aftladgerEntry.AffectedLadgerID = ladgerID;
                                aftladgerEntry.VouchersTypeID = journalVoucherTypeID;
                                aftladgerEntry.Particulars = "Discount " + item.Debit;

                                aftladgerEntry.LagerID = purchaseDiscountLadgerID;
                                aftladgerEntry.CreatedBy = 1;
                                aftladgerEntry.CreatedDate = DateTime.Now;
                                aftladgerEntry.UpdatedBy = 1;
                                aftladgerEntry.UpdatedDate = DateTime.Now;
                                aftladgerEntry.Date = item.Date;
                                aftladgerEntry.Credit = null;
                                aftladgerEntry.Credit = item.Debit;
                                aftladgerEntry.ObjectType = "Import";
                                authContext.LadgerEntryDB.Add(aftladgerEntry);
                            }








                        }
                        int i = authContext.Commit();
                    }
                }
            }
            catch (Exception ex)
            {


            }
        }



    }



    public class ShreeAmbikaLedgerDTO
    {
        public DateTime Date { get; set; }
        public string BankName { get; set; }
        public string VchType { get; set; }
        public string VchNo { get; set; }
        public double Debit { get; set; }

    }
}