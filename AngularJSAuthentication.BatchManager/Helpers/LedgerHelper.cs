using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class LedgerHelper
    {
        public async Task<bool> CreateLedger(string hashVal, string entityName)
        {
            try
            {
                List<LedgerQueue> ledgerQueue = new List<LedgerQueue>();
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString))
                {
                    string sqlquery = $"Select top 1 * from LedgerQueue with(nolock) where hashval='{hashVal}' and auditentity='{entityName.Replace("LedgerQueue","")}'";
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
                            try
                            {
                                ledgerQueue.Add(new LedgerQueue
                                {
                                    AuditAction = dataTable.Rows[i].Field<string>("AuditAction"),
                                    AuditDate = dataTable.Rows[i].Field<DateTime>("AuditDate"),
                                    AuditEntity = dataTable.Rows[i].Field<string>("AuditEntity"),
                                    AuditFields = dataTable.Rows[i].Field<string>("AuditFields"),
                                    AuditId = dataTable.Rows[i].Field<long>("AuditId"),
                                    GUID = dataTable.Rows[i].Field<string>("GUID"),
                                    HashVal = dataTable.Rows[i].Field<string>("HashVal"),
                                    PkFieldName = dataTable.Rows[i].Field<string>("PkFieldName"),
                                    PkValue = dataTable.Rows[i].Field<string>("PkValue"),
                                    TableName = dataTable.Rows[i].Field<string>("TableName"),
                                    UserName = dataTable.Rows[i].Field<string>("UserName")

                                });
                            }
                            catch (Exception ex)
                            {
                                FileWriter fileWriter = new FileWriter();
                                fileWriter.WriteToFile("Error occurs : " + ex.ToString());
                                //TextFileLogHelper.LogError($"Error for Order: {dataTable.Rows[i].Field<int>("orderid")} :: {Environment.NewLine} {ex.ToString()}");
                            }

                        }

                    }

                }
                var url = ConfigurationManager.AppSettings["ServiceAddress"];

                if (ledgerQueue.Any())
                {
                    FileWriter fileWriter = new FileWriter();
                    fileWriter.WriteToFile($"Url Call CreateLedgerEntry : {string.Join(",",ledgerQueue.Select(d=>d.PkValue))}");

                    using (GenericHttpClient<List<LedgerQueue>, string> memberClient = new GenericHttpClient<List<LedgerQueue>, string>(url + "/api/LadgerEntryV7/CreateLedgerEntry", "", null))
                    {
                        var abc = AsyncContext.Run(() => memberClient.PostAsync<bool>(ledgerQueue));
                    }


                    //HttpClient client = new HttpClient();
                    //client.DefaultRequestHeaders.Accept.Clear();
                    //var myContent = JsonConvert.SerializeObject(ledgerQueue);
                    //var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                    //var byteContent = new ByteArrayContent(buffer);
                    //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    //HttpResponseMessage response = await client.PostAsync(url + "/api/LadgerEntryV7/CreateLedgerEntry", byteContent);
                    
                }
            }
            catch (Exception ex)
            {
                FileWriter fileWriter = new FileWriter();
                fileWriter.WriteToFile("Error occurs : " + ex.ToString());
            }
            return true;

        }
    }


    public class LedgerQueue
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
