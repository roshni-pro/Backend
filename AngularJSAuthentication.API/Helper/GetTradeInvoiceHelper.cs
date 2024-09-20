
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class GetTradeInvoiceHelper
    {

        public bool GetAndGenearteTradeInvoice(List<TradeInvoiceDc> OrderInvoice)
        {
            InvoicePdfResponse invoicePdfResponse = new InvoicePdfResponse();
     
            string fileUrl = "";
            using (var db = new AuthContext())
            {
                foreach (var order in OrderInvoice)
                {
                    invoicePdfResponse.barcode = order.barcode;
                    InvoicePdfRequest InvoicePdfRequest = new InvoicePdfRequest
                    {
                        InvoiceNo = order.invoice_no
                    };
                    string FileName = "";
                    FileName = order.invoice_no.Replace('/', '_') + ".pdf";
                    var folderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriReportDownloads");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    fileUrl = "/ZaruriReportDownloads" + "/" + FileName;
                    var fullPhysicalPath = folderPath + "\\" + FileName;
                    if (!File.Exists(fullPhysicalPath))
                    {
                        var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/TradeOrders/InvoicePdf";
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("CustomerId", "1");
                            client.DefaultRequestHeaders.Add("NoEncryption", "1");
                            var newJson = JsonConvert.SerializeObject(InvoicePdfRequest);
                            using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                            {
                                var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                                response.EnsureSuccessStatusCode();
                                string responseBody = response.Content.ReadAsStringAsync().Result;
                                var result = JsonConvert.DeserializeObject<ResponseMetaData>(responseBody);
                                if (result.Status != "Error")
                                {

                                    invoicePdfResponse.html = result.Data;
                                    string base64String = Convert.ToBase64String(invoicePdfResponse.barcode, 0, invoicePdfResponse.barcode.Length);
                                    //data:image/png;base64,"+ base64String + "
                                    invoicePdfResponse.html = invoicePdfResponse.html.Replace("@BARCODE", "<img src='data:image/png;base64," + base64String + "' style='height:70px;float: right;'>");
                                    byte[] pdf = Pdf
                                                    .From(invoicePdfResponse.html)
                                                    //.WithGlobalSetting("orientation", "Landscape")
                                                    //.WithObjectSetting("web.defaultEncoding", "utf-8")
                                                    .OfSize(PaperSize.A4)
                                                    .WithTitle("Invoice")
                                                    .WithoutOutline()
                                                    .WithMargins(PaperMargins.All(0.0.Millimeters()))
                                                    .Portrait()
                                                    .Comressed()
                                                    .Content();

                                    FileStream file = File.Create(fullPhysicalPath);
                                    file.Write(pdf, 0, pdf.Length);
                                    file.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        fileUrl = "/ZaruriReportDownloads" + "/" + FileName;
                    }
                }
            }
            return true;
        }


    }
    public class InvoicePdfRequest
    {
        public string InvoiceNo { get; set; }
    }

    public class InvoicePdfResponse
    {
        public string html { get; set; }
        public byte[] barcode { get; set; }
    }
}