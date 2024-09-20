using AngularJSAuthentication.API.DataContract;
using Microsoft.Reporting.WebForms;
using NLog;
using System;
using System.Data;
using System.IO;
using System.Web;

namespace AngularJSAuthentication.API.WebAPIHelper
{
    public class BounceChequeHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Generate pdf Bank Settlement
        /// </summary>
        /// <param name="currencySettlementSource"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Generatepdfbounce(ChequeBounceDc ChequeBounceDc, AuthContext context)
        {
            string filepath = string.Empty;
            string returnfilepath = string.Empty;
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "ChequeBounce.rdlc");

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


                //ReportParameterCollection reportParameters = new ReportParameterCollection();


                //reportParameters.Add(new ReportParameter("Date", ChequeBounceDc.Date.ToString("dd/MM/yyyy hh:mm tt")));
                //reportParameters.Add(new ReportParameter("ChequeNumber", ChequeBounceDc.ChequeNumber));
                //reportParameters.Add(new ReportParameter("SKCode", ChequeBounceDc.SKCode));
                //reportParameters.Add(new ReportParameter("Address", ChequeBounceDc.Address));
                //reportParameters.Add(new ReportParameter("ReturnComment", ChequeBounceDc.ReturnComment));

                DataTable dt = new DataTable("DataSet1");
                dt.Columns.Add("Address");
                dt.Columns.Add("Skcode");
                dt.Columns.Add("ChequeNumber");
                dt.Columns.Add("ChequeAmt");
                dt.Columns.Add("Orderid");
                dt.Columns.Add("Date");
                dt.Columns.Add("PartyName");
                dt.Columns.Add("ReturnComment");

                if (ChequeBounceDc != null)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = ChequeBounceDc.Address;
                    dr[1] = ChequeBounceDc.SKCode;
                    dr[2] = ChequeBounceDc.ChequeNumber;
                    dr[3] = ChequeBounceDc.ChequeAmt;
                    dr[4] = ChequeBounceDc.Orderid;
                    dr[5] = ChequeBounceDc.Date;
                    dr[6] = ChequeBounceDc.PartyName;
                    dr[7] = ChequeBounceDc.ReturnComment;

                    dt.Rows.Add(dr);
                }

                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dt));
                viewer.LocalReport.Refresh();



                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/GenrateCertificateBounce")))
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/GenrateCertificateBounce"));

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                string filename = "genrateCertificateslip_" + ChequeBounceDc.Orderid + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".pdf";
                filepath = HttpContext.Current.Server.MapPath(@"~\GenrateCertificateBounce\") + filename;
                FileStream file = File.Create(filepath);
                file.Write(bytes, 0, bytes.Length);
                file.Close();
                returnfilepath = "/GenrateCertificateBounce/" + filename;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Generatepdf Method: " + ex.Message);
            }

            return returnfilepath;
        }

    }
}