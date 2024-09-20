using AngularJSAuthentication.Model.CashManagement;
using Microsoft.Reporting.WebForms;
using NLog;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.WebAPIHelper
{
    public class ChequeHistoryHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Generate pdf Bank Settlement
        /// </summary>
        /// <param name="currencySettlementSource"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Generatepdf(CurrencySettlementSource currencySettlementSource, AuthContext context)
        {
            string filepath = string.Empty;
            string returnfilepath = string.Empty;
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "ChequeWithdraw.rdlc");

                // Variables
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                // Setup the report viewer object and get the array of bytes
                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = path;


                ReportParameterCollection reportParameters = new ReportParameterCollection();


                if (currencySettlementSource.HandOverPerson > 0)
                {
                    var withdrawperson = context.Peoples.Where(x => x.PeopleID == currencySettlementSource.HandOverPerson).Select(x => x.DisplayName).FirstOrDefault();
                    reportParameters.Add(new ReportParameter("WithdrawPerson", withdrawperson));
                }
                reportParameters.Add(new ReportParameter("WithdrawDate", currencySettlementSource.SettlementDate.ToString("dd/MM/yyyy hh:mm tt")));
                reportParameters.Add(new ReportParameter("BankName", currencySettlementSource.SettlementSource));
                reportParameters.Add(new ReportParameter("Comments", currencySettlementSource.Note));


                int cashSum = 0;
                decimal chequeSum = 0;
                DataSet Ds = new DataSet("CashCollectionDS");


                // currencySettlementSource.DepositType = 0//0 BOTH,1 CASH, 2 CHEQUE

                DataTable dt = new DataTable("CashData");
                dt.Columns.Add("Title");
                dt.Columns.Add("Count");
                dt.Columns.Add("Total");
                dt.Columns.Add("Sno");
                if (currencySettlementSource.CashSettlements != null && currencySettlementSource.CashSettlements.Any())
                {

                    var currencyDenominations = context.CurrencyDenomination.Where(x=>x.IsActive).ToList();
                    int count = 1;
                    foreach (var item in currencySettlementSource.CashSettlements)
                    {
                        var denomiation = currencyDenominations.FirstOrDefault(x => x.Id == item.CurrencyDenominationId);
                        DataRow dr = dt.NewRow();
                        dr[0] = denomiation.Title;
                        dr[1] = item.CurrencyCount;
                        dr[2] = denomiation.Value * item.CurrencyCount;
                        dr[3] = count++;
                        dt.Rows.Add(dr);

                        cashSum += denomiation.Value * item.CurrencyCount;
                    }

                }

                if (dt.Rows.Count > 0)
                {
                    reportParameters.Add(new ReportParameter("ShowCashTable", "true"));
                }
                else
                {
                    reportParameters.Add(new ReportParameter("ShowCashTable", "false"));
                }

                Ds.Tables.Add(dt);

                dt = new DataTable("ChequeData");
                dt.Columns.Add("ChequeNo");
                dt.Columns.Add("ChequeDate");
                dt.Columns.Add("ChequeAmt");
                dt.Columns.Add("BankName");
                dt.Columns.Add("Sno");
                if (currencySettlementSource.ChequeCollections != null && currencySettlementSource.ChequeCollections.Any())
                {

                    int count = 1;
                    foreach (var item in currencySettlementSource.ChequeCollections)
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = item.ChequeNumber;
                        dr[1] = item.ChequeDate.ToString("dd/MM/yyyy");
                        dr[2] = item.ChequeAmt;
                        dr[3] = item.ChequeBankName;
                        dr[4] = count++;
                        dt.Rows.Add(dr);

                        chequeSum += item.ChequeAmt;
                    }

                }

                if (dt.Rows.Count > 0)
                {
                    reportParameters.Add(new ReportParameter("ShowChequeTable", "true"));
                }
                else
                {
                    reportParameters.Add(new ReportParameter("ShowChequeTable", "false"));
                }

                Ds.Tables.Add(dt);
                //
                //bankname
                //comment
                reportParameters.Add(new ReportParameter("CashSum", ((decimal)cashSum).ToString()));
                reportParameters.Add(new ReportParameter("ChequeSum", chequeSum.ToString()));

                //currencySettlementSource.DepositType = 0//0 BOTH,1 CASH, 2 CHEQUE
                string displayDepositType = "";
                //if (currencySettlementSource.DepositType == 0)
                //{
                //    displayDepositType = "BOTH";
                //}
                //else if (currencySettlementSource.DepositType == 1)
                //{
                //    displayDepositType = "CASH";
                //}
                //if (currencySettlementSource.DepositType == 2)
                //{
                //    displayDepositType = "CHEQUE";
                //}
                displayDepositType = "BOTH";


                reportParameters.Add(new ReportParameter("DisplayDepositType", displayDepositType));

                viewer.LocalReport.SetParameters(reportParameters);
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(new ReportDataSource("CashData", Ds.Tables[0]));
                viewer.LocalReport.DataSources.Add(new ReportDataSource("ChequeData", Ds.Tables[1]));
                viewer.LocalReport.Refresh();



                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                string filename = "WithdrawSlip_" + currencySettlementSource.Id + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".pdf";
                filepath = HttpContext.Current.Server.MapPath(@"~\BankWithdrawDepositFile\") + filename;
                FileStream file = File.Create(filepath);
                file.Write(bytes, 0, bytes.Length);
                file.Close();
                returnfilepath = "/BankWithdrawDepositFile/" + filename;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Generatepdf Method: " + ex.Message);
            }

            return returnfilepath;
        }

    }
}