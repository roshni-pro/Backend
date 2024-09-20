using AngularJSAuthentication.BatchManager.Actions;
using AngularJSAuthentication.BatchManager.Actions.BackendOrderActions;
using AngularJSAuthentication.BatchManager.Actions.InvoiceActions;
using AngularJSAuthentication.BatchManager.Constants;
using AngularJSAuthentication.BatchManager.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager
{
    public class Subscriber
    {
        static string LedgerQueName = $"[[entity]]LedgerQueue";
        FileWriter fileWriter = new FileWriter();
        public void Subscribe()
        {

            RabbitSubscriber helper = new RabbitSubscriber();
            ElasticItemHelper elastisItemHelper = new ElasticItemHelper();
            LedgerHelper ledgerHelper = new LedgerHelper();
            List<string> ledgerQueueNames = new List<string>();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString))
            {
                string sqlquery = "select distinct EntityName from LedgerConfigurationMasters with(nolock) where IsActive = 1 and IsDeleted = 0  and (isPublished is null or IsPublished = isPublished)";
                var dataTable = new DataTable();



                using (var command = new SqlCommand(sqlquery.ToString(), connection))
                {
                    command.CommandTimeout = 1000;

                    if (connection.State != ConnectionState.Open)
                        connection.Open();


                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(dataTable);
                    da.Dispose();
                }


                connection.Close();

                if (dataTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        ledgerQueueNames.Add(LedgerQueName.Replace("[[entity]]", dataTable.Rows[i].Field<string>("EntityName")));
                    }

                }

                fileWriter.WriteToFile($"Configuration fetched from db {dataTable.Rows.Count}");

            }

            if (ledgerQueueNames.Any())
            {
                fileWriter.WriteToFile($"inside ledger queue names");

                foreach (var item in ledgerQueueNames)
                {
                    helper.Subscribe(item, new Action<string>((s) =>
                    {
                        ledgerHelper.CreateLedger(s, item);
                    }));
                }

            }

            helper.Subscribe(Queues.BatchCodeQueue, new Action<BatchCodeSubject>(async (s) =>
            {
                await OnSubscribe(s);
            }));

            helper.Subscribe(Queues.OrderInvoiceQueue, new Action<BatchCodeSubject>(async (s) =>
            {
                await OnSubscribe(s);
            }));
            helper.Subscribe(Queues.BackendOrderQueue, new Action<BatchCodeSubject>(async (s) =>
            {
                await OnSubscribe(s);
            }));
            helper.Subscribe(Queues.ZilaOrderQueue, new Action<BatchCodeSubject>(async (s) =>
            {
                await OnSubscribe(s);
            }));
            helper.Subscribe(Queues.itemMqName, new Action<ItemIdCls>((s) =>
           {
               elastisItemHelper.InsertInElasticWithItemId(s);
           }));

            helper.Subscribe(Queues.itemLimitMqName, new Action<MultiMrpIdCls>((s) =>
            {
                elastisItemHelper.InsertInElasticWithMrpId(s);
            }));

        }
        public async Task<bool> OnSubscribe(BatchCodeSubject batchCodeSubject)
        {

            bool result = false;
            BatchCodeHelper batchCodeHelper = new BatchCodeHelper();
            BatchCodeAction acion = null;
            try
            {
                switch (batchCodeSubject.TransactionType)
                {
                    case "CurrentGRN":
                        acion = new GRNCurrentStockAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "DamageGRN":
                        acion = new GRNDamageStockAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "ExpiryGRN":
                        acion = new GRNExpiryStockAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "FreeGRN":
                        acion = new GRNFreeStockAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderOutCurrent":
                        acion = new OrderOutCurrentAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderOutDamage":
                        acion = new OrderOutDamageAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderOutFree":
                        acion = new OrderOutFreeÁction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedOutCurrent":
                        acion = new OrderPlannedOutCurrentAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedOutFree":
                        acion = new OrderPlannedOutFreeAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedInCurrent":
                        acion = new OrderPlannedInCurrentAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedInFree":
                        acion = new OrderPlannedInFreeAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInCurrent":
                        acion = new OrderInCurrentAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInFree":
                        acion = new OrderInFreeAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInDamage":
                        acion = new OrderInDamageAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderOutClearance":
                        acion = new OrderOutClearanceAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInClearance":
                        acion = new OrderInClearanceAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedOutClearance":
                        acion = new OrderPlannedOutClearanceAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderPlannedInClearance":
                        acion = new OrderPlannedInClearanceAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInvoiceQueue":
                        acion = new InvoiceNoGenerateAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "Test":
                        acion = new TestBatchAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "OrderInNonSellable":
                        acion = new OrderInNonSellableStockAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "BackendOrderQueue":
                        acion = new BackendOrderProcessAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                    case "ZilaOrderQueue":
                        acion = new ZilaOrderProcessAction();
                        result = await acion.FinalRun(batchCodeSubject);
                        break;
                }
                //throw new Exception("exception occurss");
            }
            catch (Exception ex)
            {
                string error =  ex.InnerException != null ? "OnSubscribeError  " + ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                //batchCodeHelper.InsertSubjectInMongoError(batchCodeSubject);
                await LogHelper.ErrorLog(new ErrorLog
                {
                    CoRelationId = "",
                    IP = "",
                    ForwardedIps = "",
                    Message = error,
                    CreatedDate = DateTime.Now,
                    xForwardedHttpHeader = JsonConvert.SerializeObject(batchCodeSubject)
                });


                FileWriter fileWriter = new FileWriter();
                fileWriter.WriteToFile("Error occurs : " + error);

                MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
                mongoDbHelper.UpdateWhenSubscriberErrorOccurs(batchCodeSubject.HashCode, true, error);

            }
            return result;
        }
    }
}
