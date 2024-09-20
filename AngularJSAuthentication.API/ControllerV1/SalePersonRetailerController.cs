using AngularJSAuthentication.API.Controllers.Base;
using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SalePersonRetailer")]
    public class SalePersonRetailerController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public HttpResponseMessage Get(int ExecutiveId, string srch) //get customers by cluster
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var clist = context.AllSalePersonRetailer(srch, ExecutiveId);
                    if (clist == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Customers");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, clist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("V2")]
        public HttpResponseMessage GetV2(int ExecutiveId, string srch) //get customers by cluster
        {
            using (var context = new AuthContext())
            {
                try
                {
                    custdetail res;
                    var clist = context.AllSalePersonRetailer(srch, ExecutiveId);
                    if (clist == null)
                    {
                        res = new custdetail
                        {
                            customer = clist,
                            Message = "No Customers",
                            Status = false
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    res = new custdetail
                    {
                        customer = clist,
                        Message = "Success.",
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    custdetail res = new custdetail
                    {
                        customer = null,
                        Message = "Failed.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
    }
    public class CustomerDTOM
    {
        public int CustomerId { get; set; }
        //  public int Id { get; set; }
        public int CustSupplierid { get; set; }

        public string Skcode { get; set; }
        public string CompanyName { get; set; }
        public string Day { get; set; }
        public int? BeatNumber { get; set; }
        public int? ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string ShopName { get; set; }
       // public string Mobile { get; set; }
       // public string Name { get; set; }
        public int SubsubCategoryid { get; set; } //brand Id
        public string SubsubcategoryName { get; set; } //brand name
        public bool IsMine { get; set; }

        public int? CompanyId { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string Emailid { get; set; }
        public bool IsAssigned { get; set; }

        public int? Cityid { get; set; }
        public string City { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public bool check { get; set; }
        public int Areaid { get; set; }
        public double? TotalAmount { get; set; }
        public double? NewAddedAmount { get; set; }
        public double? NewOutAmount { get; set; }

        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string ClusterName { get; set; }
        public int? ClusterId { get; set; }





    }

    public class custdetail
    {
        public CustomerDTOM customer { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
}



