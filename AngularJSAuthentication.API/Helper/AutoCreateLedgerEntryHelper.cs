using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AngularJSAuthentication.Model;
using NLog;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Transactions;
using System.Data.SqlClient;
using AngularJSAuthentication.Accounts.Managers;
using AngularJSAuthentication.Model.Account;

namespace AngularJSAuthentication.API.Helper
{
    public class AutoCreateLedgerEntryHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();


        public bool AutoCreateLedgerEntry()
        {
            bool result = true;
            //int AutoSettleLedgerEntryCount = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSettleLedgerEntryCount"]);


            using (var context = new AuthContext())
            {
                //var param = new SqlParameter("take", AutoSettleLedgerEntryCount);
                //var LedgerQueueData = context.Database.SqlQuery<LedgerQueueDc>("Exec sp_GetLedgerQueueData @take", param).ToList();

                var LedgerQueueData = context.Database.SqlQuery<LedgerQueueDc>("Exec sp_GetLedgerQueueData").ToList();

                foreach (var item in LedgerQueueData)
                {                                   
                    //using (var dbContextTransaction = context.Database.BeginTransaction())
                    //{
                        try
                        {                                
                            LedgerComposerManager ledgerComposerManager = new LedgerComposerManager();
                                LedgerQueue ledgerQueue = new LedgerQueue();

                            ledgerQueue.AuditId = item.AuditId; 
                            ledgerQueue.PkValue = item.PkValue;
                            ledgerQueue.PkFieldName = item.PkFieldName;
                            ledgerQueue.AuditDate = item.AuditDate;
                            ledgerQueue.UserName = item.UserName;
                            ledgerQueue.AuditAction = item.AuditAction;                            
                            ledgerQueue.AuditEntity = item.AuditEntity;
                            ledgerQueue.TableName = item.TableName;
                            ledgerQueue.GUID = item.GUID;
                            ledgerQueue.HashVal = item.HashVal;
                            ledgerQueue.AuditFields = item.AuditFields;
                        
                            ledgerComposerManager.CheckAndCreateLedger(ledgerQueue, isRuningFromUtillity:true );

                            //dbContextTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            //dbContextTransaction.Rollback();
                            logger.Error("Error in CreateLedgerEntry " + ex.Message + " Audit Id" + item.AuditId);
                        }
                    //}
                    
                }

                return result;
            }
        }









        public class LedgerQueueDc
        {
            public long AuditId { get; set; }
            [MaxLength(20)]
            public string PkValue { get; set; }

            [MaxLength(150)]
            public string PkFieldName { get; set; }
            public DateTime AuditDate { get; set; }

            [MaxLength(150)]
            public string UserName { get; set; }

            [MaxLength(150)]
            public string AuditAction { get; set; }

            [MaxLength(150)]
            public string AuditEntity { get; set; }

            [MaxLength(150)]
            public string TableName { get; set; }

            [MaxLength(500)]
            public string GUID { get; set; }

            [MaxLength(2000)]
            public string HashVal { get; set; }
            public string AuditFields { get; set; }
        }
    }
}