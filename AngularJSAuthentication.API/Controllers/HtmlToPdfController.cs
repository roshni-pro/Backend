using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.DataContracts.ScaleUp.ScaleUpDc;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/HtmlToPdf")]
    public class HtmlToPdfController : ApiController
    {
        readonly string platformIdxName = $"skorderdata_{AppConstants.Environment}";

        [HttpPost]
        [Route("ConvertHtmlToPdf")]
        [AllowAnonymous]
        public async Task<Responsedc> ConvertHtmlToPdf(HtmltoPdfDc obj)
        {
            Responsedc res = new Responsedc();
            TextFileLogHelper.TraceLog("call ConvertHtmlToPdf - 1");
            res.result = ConvertHtmlToPdf(obj.Html);
            return res;
        }

        public byte[] ConvertHtmlToPdf(string htmldata)
        {
            byte[] pdf = null;

            pdf = Pdf
                  .From(htmldata)
                  .OfSize(OpenHtmlToPdf.PaperSize.A4)
                  .WithTitle("Invoice")
                  .WithoutOutline()
                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
                  .Portrait()
                  .Comressed()
                  .Content();
            return pdf;
        }

       
    }
}