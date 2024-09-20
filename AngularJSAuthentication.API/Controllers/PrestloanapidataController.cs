using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Prestloanapidata")]
    public class PrestloanapidataController : ApiController
    {
        
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get()//get all Issuances which are active for the delivery boy
        {
            using (var context = new AuthContext())
            {

                try
                {

                    var CustomerData = context.Customers.Where(x => x.Active == true && x.Deleted == false && x.RefNo != null && x.MonthlyTurnOver > 10000).ToList();

                    var PrestaData = (from a in CustomerData
                                      select new Prestloanapidata
                                      {
                                          CustomerId = a.CustomerId,
                                          ShopOwnerName = a.Name,
                                          ShopName = a.ShopName,
                                          Mobile = a.Mobile,
                                          ShopAddress = a.ShippingAddress,
                                          TinGst = a.RefNo,
                                          Gpslocation = a.SAGPSCoordinates,
                                          Refrenceno = a.Mobile,
                                          SixMonthsTurnOver = Convert.ToString(a.MonthlyTurnOver),
                                          ResidenceAddress = a.BillingAddress,
                                          EmailId = a.Emailid,
                                          BankName = a.BankName,
                                          BranchName = a.BranchName,
                                          IfscCode = a.IfscCode,
                                          AadharNo = a.AadharNo,
                                          PanNo = a.PanNo,
                                          ResidenceAddressProof = a.ResidenceAddressProof
                                      }).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, PrestaData);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(int CustomerId)//get all Issuances which are active for the delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var CustomerOrderData = context.DbOrderMaster.Where(x => x.CustomerId == CustomerId && x.Status == "Pending").ToList();

                    var PrestaData = (from a in CustomerOrderData
                                      select new Prestloanapidata
                                      {
                                          CustomerId = a.CustomerId,
                                          OrderId = a.OrderId,
                                          InvoiceNo = a.invoice_no,
                                          TotalAmount = a.GrossAmount,
                                          TotalTaxAmount = a.TaxAmount,
                                      }).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, PrestaData);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }




        [Route("Loan")]
        [HttpGet, HttpPost]
        public HttpResponseMessage GetLoan()//get all Issuances which are active for the delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://prestloans.com/Loan/GetSkCustomerDetail?id=0");
                    request.Headers.Add("requestverificationtoken", "V06N/g78WjqTSB8rvOnFgA==");
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "POST";

                    using (Stream st = request.GetRequestStream())
                    {
                        using (WebResponse tResponse = request.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String responseServer = tReader.ReadToEnd();


                                    var resultDTO = JsonConvert.DeserializeObject(responseServer);

                                    return Request.CreateResponse(HttpStatusCode.OK, resultDTO);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

        }
        public class Prestloanapidata
        {
            public int CustomerId { get; set; }
            public string ShopOwnerName { get; set; }
            public string ShopName { get; set; }
            public string Mobile { get; set; }
            public string ShopAddress { get; set; }
            public string TinGst { get; set; }
            public string Gpslocation { get; set; }
            public string Refrenceno { get; set; }
            public string SixMonthsTurnOver { get; set; }
            public string ResidenceAddress { get; set; }
            public string EmailId { get; set; }
            public string BankName { get; set; }
            public string BranchName { get; set; }
            public string IfscCode { get; set; }
            public string AadharNo { get; set; }
            public string PanNo { get; set; }
            public string ResidenceAddressProof { get; set; }
            public int OrderId { get; set; }
            public string InvoiceNo { get; set; }
            public double TotalAmount { get; set; }
            public double TotalTaxAmount { get; set; }
        }
    }
}