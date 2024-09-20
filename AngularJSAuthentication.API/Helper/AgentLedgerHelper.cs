using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class AgentLedgerHelper
    {
        Logger logger = null;
        string salesVoucherType = "AgentSales";
        string receiptVoucherType = "AgentReceipt";
        string cancelVoucherType = "AgentCancel";
        string redispatchVoucherType = "AgentRedispatch";
        string agentChequeReturn = "AgentChequeReturn";
        string revertAgentChequeReturn = "RevertAgentChequeReturn";
        string agentChequeFine = "AgentChequeFine";
        string revertAgentChequeFine = "RevertAgentChequeFine";
        string commissionVoucherType = "AgentCommission";


        string agentLederType = "Agent";
        string transactionLederType = "Transaction";
        string agentCashLederType = "AgentCash";
        string agentCommissionLederType = "AgentCommission";
        string agentTDSLederType = "AgentTDS";

        //string agentcash


        #region insert ledger entry 
        public void OnChequeCancel(DeliveryIssuance assignment, int userid, double amount, ChequeCollectionDc chequeCollection)
        {
            AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
            CustomerChequeBounceLedgerHelper customerChequeBounceLedgerHelper = new CustomerChequeBounceLedgerHelper();
            if (assignment != null && assignment.AgentId > 0)
            {
                agentLedgerHelper.OnChequeCancel(assignment.AgentId, Convert.ToDouble(chequeCollection.ChequeAmt), chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.OrderId.ToString());
                agentLedgerHelper.OnChequeFine(assignment.AgentId, amount, chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.OrderId.ToString());

            }
            if (chequeCollection.OrderId > 0 && !string.IsNullOrEmpty(chequeCollection.ChequeNumber))
            {
                List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> chequeBounceVM = new List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> {
                                          new AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM{
                                           Amount=Convert.ToDouble(chequeCollection.ChequeAmt),
                                           ChequeNumber=chequeCollection.ChequeNumber,
                                           Date=chequeCollection.ChequeDate
                                          }
                                         };
                customerChequeBounceLedgerHelper.OnBounce(chequeCollection.OrderId.Value, chequeBounceVM, amount, userid);
            }
        }

        public void OnPaymentAccept(DeliveryIssuance assignment, int userid, AngularJSAuthentication.API.External.DeliveryAPP.CurrencyCollectionDc dbCurrencyCollection)
        {

            AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
            if (assignment != null && assignment.AgentId > 0)
            {
                double totalCollectedAmt = Convert.ToDouble(dbCurrencyCollection.TotalCashAmt + dbCurrencyCollection.TotalCheckAmt + dbCurrencyCollection.TotalOnlineAmt);
                agentLedgerHelper.OnPaymentAccepted(assignment.AgentId, totalCollectedAmt, assignment.DisplayName, userid, assignment.DeliveryIssuanceId, assignment.OrderIds);
            }

        }

        public void OnGetCommisiondata(DeliveryIssuance assignment, int userid, AngularJSAuthentication.API.External.DeliveryAPP.CurrencyCollectionDc dbCurrencyCollection)
        {

            AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
            if (assignment != null && assignment.AgentId > 0)
            {
                double totalCollectedAmt = Convert.ToDouble(dbCurrencyCollection.TotalCashAmt + dbCurrencyCollection.TotalCheckAmt + dbCurrencyCollection.TotalOnlineAmt);
                agentLedgerHelper.OnGetCommision(assignment.DeliveryIssuanceId, userid, assignment.AgentId, assignment.DisplayName);
            }

        }

        public void Onordercancelandredispatch(DeliveryIssuance assignment, int userid)
        {
            using (AuthContext context = new AuthContext())
            {
                var canceledOrderList = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == assignment.DeliveryIssuanceId && x.Status == "Delivery Canceled").ToList();
                var redispatchOrderList = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == assignment.DeliveryIssuanceId && x.Status == "Delivery Redispatch").ToList();




                if (assignment.AgentId > 0)
                {
                    AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                    if (canceledOrderList != null && canceledOrderList.Count > 0)
                    {
                        foreach (var item in canceledOrderList)
                        {
                            agentLedgerHelper.OnOrderCancel(assignment.AgentId, item.TotalAmount, assignment.DisplayName, 0, assignment.DeliveryIssuanceId, item.OrderId.ToString());
                        }
                    }
                }




                if (assignment.AgentId > 0)
                {
                    AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                    if (redispatchOrderList != null && redispatchOrderList.Count > 0)
                    {
                        foreach (var item in redispatchOrderList)
                        {
                            agentLedgerHelper.OnOrderRedispatch(assignment.AgentId, item.TotalAmount, assignment.DisplayName, 0, assignment.DeliveryIssuanceId, item.OrderId.ToString());
                        }
                    }
                }


            }
        }
        #endregion

        public void UpdateAgentCommission(int deliveryIssuanceId)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    int rows = context.Database.ExecuteSqlCommand("AgentCommissionGet @DeliveryIssuanceId", new SqlParameter("@DeliveryIssuanceId", deliveryIssuanceId));
                    context.Commit();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public AgentLedgerHelper()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public bool OnAssignmentAccepted(int agentId, double amount, string dBoyname, int userid, int deliveryIssuranceID, string orderIDList, DateTime? date = null)
        {

            try
            {

                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == salesVoucherType);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();
                    //logger.Info("transactionLadger added successfully: " + transactionLadger.ID.ToString(), transactionLadger.ID);
                    //logger.Info("agentLadgerType added successfully: " + agentLedger.ID.ToString(), agentLedger.ID);
                    //logger.Info("vch is added successfully: " + vch.ID.ToString(), vch.ID);
                    //logger.Info("voucherType successfully: " + voucherType.ID.ToString(), voucherType.ID);

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date
                    };
                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();

                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }

        }

        public bool OnPaymentAccepted(int agentId, double amount, string dBoyname, int userid, int deliveryIssuranceID, string orderIDList, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);


                    var agentCashLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentCashLederType);
                    Ladger cashLadger = GetAddLedger(0, userid, agentCashLadgerType.ID, agentCashLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == receiptVoucherType);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = cashLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date
                    };
                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = cashLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnOrderCancel(int agentId, double amount, string dBoyname, int userid, int deliveryIssuranceID, string orderIDList, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == cancelVoucherType);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date,
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date,
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnOrderRedispatch(int agentId, double amount, string dBoyname, int userid, int deliveryIssuranceID, string orderIDList, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == redispatchVoucherType);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date,
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = dBoyname,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = date,
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnChequeCancel(int agentId, double amount, string chequeNumber, int userid, int deliveryIssuranceID, string orderIDList)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == agentChequeReturn);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now
                    };
                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();

                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnRevertChequeCancel(int agentId, double amount, string chequeNumber, int userid, int deliveryIssuranceID, string orderIDList)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == revertAgentChequeReturn);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();

                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnChequeFine(int agentId, double amount, string chequeNumber, int userid, int deliveryIssuranceID, string orderIDList)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == agentChequeFine);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now,
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();

                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool OnRevertChequeFine(int agentId, double amount, string chequeNumber, int userid, int deliveryIssuranceID, string orderIDList)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var transactionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == transactionLederType);
                    Ladger transactionLadger = GetAddLedger(0, userid, transactionLadgerType.ID, transactionLadgerType.code);

                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == revertAgentChequeFine);

                    Voucher vch = new Voucher
                    {
                        Active = true,
                        Code = orderIDList,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.VoucherDB.Add(vch);
                    authContext.Commit();

                    LadgerEntry ladgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = transactionLadger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = amount,
                        Debit = null,
                        LagerID = agentLedger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now,
                    };

                    LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                    {
                        Active = true,
                        AffectedLadgerID = agentLedger.ID,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Credit = null,
                        Debit = amount,
                        LagerID = transactionLadger.ID,
                        ObjectID = deliveryIssuranceID,
                        ObjectType = "Assignment",
                        Particulars = "ChequeNumber: " + chequeNumber,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        VouchersTypeID = voucherType.ID,
                        VouchersNo = vch.ID,
                        Date = DateTime.Now,
                    };

                    authContext.LadgerEntryDB.Add(ladgerEntry);
                    authContext.LadgerEntryDB.Add(oppositeLadgerEntry);

                    authContext.Commit();

                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// If ladger already exists then return id
        /// else create new ledger and return its new id
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="userid"></param>
        /// <param name="ledgerTypeID"></param>
        /// <param name="ledgerTypeName"></param>
        /// <returns></returns>
        private Ladger GetAddLedger(int objectId, int userid, int ledgerTypeID, string ledgerTypeName)
        {
            Ladger ladger = null;
            using (var authContext = new AuthContext())
            {
                ladger = authContext.LadgerDB.FirstOrDefault(x => x.ObjectID == objectId && x.ObjectType == ledgerTypeName);

                if (ladger == null && objectId != 0)
                {
                    var agent = authContext.Peoples.FirstOrDefault(x => x.PeopleID == objectId);
                    ladger = new Ladger
                    {
                        Active = true,
                        Alias = agent.AgentCode,
                        Name = agent.DisplayName,
                        Address = "",
                        Country = "",
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        GroupID = null,
                        GSTno = "",
                        InventoryValuesAreAffected = false,
                        LadgertypeID = ledgerTypeID,
                        ObjectID = agent.PeopleID,
                        ObjectType = ledgerTypeName,
                        ProvidedBankDetails = false,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.LadgerDB.Add(ladger);
                    authContext.Commit();
                }
                else if (ladger == null && objectId == 0)
                {
                    ladger = new Ladger
                    {
                        Active = true,
                        Alias = ledgerTypeName,
                        Name = ledgerTypeName,
                        Address = "",
                        Country = "",
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        GroupID = null,
                        GSTno = "",
                        InventoryValuesAreAffected = false,
                        LadgertypeID = ledgerTypeID,
                        ObjectID = objectId,
                        ObjectType = ledgerTypeName,
                        ProvidedBankDetails = false,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now
                    };

                    authContext.LadgerDB.Add(ladger);
                    authContext.Commit();
                }
            }
            return ladger;
        }

        public bool OnGetCommision(int deliveryIssuranceID, int userid, int agentId, string dBoyname, DateTime? date = null)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    if (!date.HasValue)
                    {
                        date = DateTime.Now;
                    }
                    var deliveryIssuanceIdParam = new SqlParameter
                    {
                        ParameterName = "DeliveryIssuanceId",
                        Value = deliveryIssuranceID
                    };
                    List<AcceptAssignmentAgentLedger> list = authContext.Database.SqlQuery<AcceptAssignmentAgentLedger>("exec AgentLedgerOnAcceptAssignment @DeliveryIssuanceId", deliveryIssuanceIdParam).ToList();

                    var agentLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentLederType);
                    Ladger agentLedger = GetAddLedger(agentId, userid, agentLadgerType.ID, agentLadgerType.code);

                    var commissionLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentCommissionLederType);
                    Ladger commissionLadger = GetAddLedger(0, userid, commissionLadgerType.ID, commissionLadgerType.code);

                    var agentTDSLadgerType = authContext.LadgerTypeDB.FirstOrDefault(x => x.code == agentTDSLederType);
                    Ladger agentTDSLadger = GetAddLedger(0, userid, agentTDSLadgerType.ID, agentTDSLadgerType.code);


                    var voucherType = authContext.VoucherTypeDB.FirstOrDefault(x => x.Name == commissionVoucherType);


                    List<LadgerEntry> ladgerEntryList = new List<LadgerEntry>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            Voucher vch = new Voucher
                            {
                                Active = true,
                                Code = item.OrderId.ToString(),
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                UpdatedBy = userid,
                                UpdatedDate = DateTime.Now
                            };

                            authContext.VoucherDB.Add(vch);
                            authContext.Commit();

                            LadgerEntry ladgerEntry = new LadgerEntry()
                            {
                                Active = true,
                                AffectedLadgerID = commissionLadger.ID,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                Credit = item.CommissionAmount,
                                Debit = null,
                                LagerID = agentLedger.ID,
                                ObjectID = deliveryIssuranceID,
                                ObjectType = "Assignment",
                                Particulars = dBoyname,
                                UpdatedBy = userid,
                                UpdatedDate = DateTime.Now,
                                VouchersTypeID = voucherType.ID,
                                VouchersNo = vch.ID,
                                Date = date
                            };

                            LadgerEntry oppositeLadgerEntry = new LadgerEntry()
                            {
                                Active = true,
                                AffectedLadgerID = agentLedger.ID,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                Credit = null,
                                Debit = item.CommissionAmount,
                                LagerID = commissionLadger.ID,
                                ObjectID = deliveryIssuranceID,
                                ObjectType = "Assignment",
                                Particulars = dBoyname,
                                UpdatedBy = userid,
                                UpdatedDate = DateTime.Now,
                                VouchersTypeID = voucherType.ID,
                                VouchersNo = vch.ID,
                                Date = date
                            };
                            LadgerEntry oppositeTDSLadgerEntry = new LadgerEntry();
                            LadgerEntry tdsLadgerEntry = new LadgerEntry();
                            decimal percentage = authContext.agentTDSDB.Where(x => x.fromDate <= DateTime.Now && x.toDate >= DateTime.Now).Select(x => x.Percentage).FirstOrDefault();
                            if (percentage > 0)
                            {
                                double amount = (Convert.ToDouble(percentage) * item.CommissionAmount ?? 0) / 100;

                                oppositeTDSLadgerEntry.Active = true;
                                oppositeTDSLadgerEntry.AffectedLadgerID = agentTDSLadger.ID;
                                oppositeTDSLadgerEntry.CreatedBy = userid;
                                oppositeTDSLadgerEntry.CreatedDate = DateTime.Now;
                                oppositeTDSLadgerEntry.Credit = null;
                                oppositeTDSLadgerEntry.Debit = amount;
                                oppositeTDSLadgerEntry.LagerID = agentLedger.ID;
                                oppositeTDSLadgerEntry.ObjectID = deliveryIssuranceID;
                                oppositeTDSLadgerEntry.ObjectType = "Assignment";
                                oppositeTDSLadgerEntry.Particulars = dBoyname;
                                oppositeTDSLadgerEntry.UpdatedBy = userid;
                                oppositeTDSLadgerEntry.UpdatedDate = DateTime.Now;
                                oppositeTDSLadgerEntry.VouchersTypeID = voucherType.ID;
                                oppositeTDSLadgerEntry.VouchersNo = vch.ID;
                                oppositeTDSLadgerEntry.Date = date;


                                tdsLadgerEntry.Active = true;
                                tdsLadgerEntry.AffectedLadgerID = agentLedger.ID;
                                tdsLadgerEntry.CreatedBy = userid;
                                tdsLadgerEntry.CreatedDate = DateTime.Now;
                                tdsLadgerEntry.Credit = amount;
                                tdsLadgerEntry.Debit = null;
                                tdsLadgerEntry.LagerID = agentTDSLadger.ID;
                                tdsLadgerEntry.ObjectID = deliveryIssuranceID;
                                tdsLadgerEntry.ObjectType = "Assignment";
                                tdsLadgerEntry.Particulars = dBoyname;
                                tdsLadgerEntry.UpdatedBy = userid;
                                tdsLadgerEntry.UpdatedDate = DateTime.Now;
                                tdsLadgerEntry.VouchersTypeID = voucherType.ID;
                                tdsLadgerEntry.VouchersNo = vch.ID;
                                tdsLadgerEntry.Date = date;

                            }
                            ladgerEntryList.Add(ladgerEntry);
                            ladgerEntryList.Add(oppositeLadgerEntry);
                            ladgerEntryList.Add(oppositeTDSLadgerEntry);
                            ladgerEntryList.Add(tdsLadgerEntry);
                        }

                        authContext.LadgerEntryDB.AddRange(ladgerEntryList);
                        authContext.Commit();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

        }

        public class AcceptAssignmentAgentLedger
        {
            public double? CommissionAmount { get; set; }
            public int OrderId { get; set; }
        }


        #region old assignment ledgers

        public bool updateassignmentledgerentry(int userid, int assignmentId, AuthContext context)
        {

            var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == assignmentId).SingleOrDefault();
            var canceledOrderList = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == assignmentId && (x.Status == "Delivery Canceled" || x.Status == "Post Order Canceled")).ToList();
            var redispatchOrderList = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == assignmentId && x.Status == "Delivery Redispatch").ToList();
            if (DBoyorders != null)
            {
                if (DBoyorders.AgentId > 0)
                {
                    //delete ledger entry 
                    List<LadgerEntry> ladgerentrydata = context.LadgerEntryDB.Where(x => x.ObjectID == assignmentId && x.ObjectType == "Assignment").ToList();
                    context.LadgerEntryDB.RemoveRange(ladgerentrydata);
                    context.Commit();
                    //end
                    //Assignement accept 
                    OnAssignmentAccepted(DBoyorders.AgentId, Convert.ToDouble(DBoyorders.TotalAssignmentAmount), DBoyorders.DisplayName, userid, DBoyorders.DeliveryIssuanceId, DBoyorders.OrderIds.ToString(), DBoyorders.CreatedDate);
                    //end
                    // update ledger for cancel order
                    if (canceledOrderList != null && canceledOrderList.Count > 0)
                    {
                        foreach (var item in canceledOrderList)
                        {
                            OnOrderCancel(DBoyorders.AgentId, Convert.ToDouble(item.TotalAmount), DBoyorders.DisplayName, userid, DBoyorders.DeliveryIssuanceId, item.OrderId.ToString(), item.UpdatedDate);

                        }
                    }
                    //end 
                    //update ledger  for redispatch order
                    if (redispatchOrderList != null && redispatchOrderList.Count > 0)
                    {
                        foreach (var item in redispatchOrderList)
                        {
                            OnOrderRedispatch(DBoyorders.AgentId, Convert.ToDouble(item.TotalAmount), DBoyorders.DisplayName, userid, DBoyorders.DeliveryIssuanceId, item.OrderId.ToString(), item.UpdatedDate);
                        }
                    }
                    //end 
                }

            }

            return true;
        }

        #endregion



    }
}