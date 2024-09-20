using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API
{
    public class TransporterLedgerWorker : LedgerWorker
    {
        public override List<LadgerEntryVM> GetLedgerEntryList(DateTime toDate, DateTime fromDate, long ledgerID, double? openingBalance)
        {
            using (var context = new AuthContext())
            {
                //toDate = toDate.Date.AddDays(1);
                var query = from le in context.LadgerEntryDB
                            join l in context.LadgerDB
                            on le.LagerID equals l.ID
                            join ale in context.LadgerDB
                            on le.AffectedLadgerID equals ale.ID
                            join v in context.VoucherTypeDB
                            on le.VouchersTypeID equals v.ID
                            join lt in context.LadgerTypeDB
                            on ale.LadgertypeID equals lt.ID
                            //join ir in context.VoucherDB
                            //on le.VouchersNo equals ir.ID
                            //into ps
                            //from ir in ps.DefaultIfEmpty()
                            where (
                                le.LagerID == ledgerID
                                && le.Date >= fromDate.Date
                                && le.Date <= toDate
                                &&  (le.Debit >0 || le.Credit >0)
                            )

                            select new LadgerEntryVM
                            {
                                ID = le.ID,
                                Date = le.Date,
                                Particulars = le.Particulars,
                                LagerID = le.LagerID,
                                VouchersTypeID = le.VouchersTypeID,
                                VouchersNo = le.VouchersNo,
                                Debit = le.Debit,
                                Credit = le.Credit,
                                ObjectID = le.ObjectID.HasValue ? le.ObjectID.Value.ToString() : "",
                                ObjectType = le.ObjectType,
                                AffectedLadgerID = le.AffectedLadgerID,
                                Active = le.Active,
                                CreatedBy = le.CreatedBy,
                                CreatedDate = le.CreatedDate,
                                UpdatedBy = le.UpdatedBy,
                                UpdatedDate = le.UpdatedDate,
                                LadgerName = l.Name,
                                VoucherName = v.Name,
                                AffactedLadgerName = ale.Name,//ale.Name,
                                IRID = "",
                                LedgerTypeSequence = 0,
                                RefNo = le.RefNo
                            };


                List<LadgerEntryVM> ladgerEntryList = query.OrderBy(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ThenBy(x => x.Date.Value.Hour).ThenBy(x => x.Date.Value.Minute).ThenBy(x => x.LedgerTypeSequence).ToList<LadgerEntryVM>();



                return ladgerEntryList;
            }
            //context.Database.Log = s => Debug.WriteLine(s);

        }
        public override LedgerVM GetLadgerItem(long ledgerID, int ledgerTypeID)
        {
            using (var context = new AuthContext())
            {
                var ladgerQuery = from le in context.LadgerDB
                                  join f in context.FleetMasterDB 
                                    on le.ObjectID equals f.Id 
                                  where le.ID == ledgerID && le.LadgertypeID == ledgerTypeID
                                  select new LedgerVM
                                  {
                                      ID = le.ID,
                                      Name = f.TransportName,
                                      Alias = f.TransportName + " - " + f.FleetType,
                                      GroupID = le.GroupID,
                                      InventoryValuesAreAffected = le.InventoryValuesAreAffected,
                                      Address = f.Address,
                                      Country = "India",
                                      PinCode = null, //(!string.IsNullOrEmpty(c.ZipCode)) ?int.Parse(c.ZipCode): null,
                                      ProvidedBankDetails = le.ProvidedBankDetails,
                                      PAN = f.PanNo,
                                      RegistrationType = le.RegistrationType,
                                      GSTno = f.GSTIN,
                                      ObjectID = le.ObjectID,
                                      ObjectType = le.ObjectType,
                                      LadgertypeID = le.LadgertypeID
                                  };
                var ledger = ladgerQuery.FirstOrDefault();
                return ledger;
            }

        }


    }



}