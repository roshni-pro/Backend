using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ScaleUp
{
    public class ScaleUpDc
    {
        public class CustomerInfoDc
        {
            public int customerid { get; set; }
            public string MobileNo { get; set; }
            public string Skcode { get; set; }
            public DateTime CreateDate { get; set; }
        }
        public class InitiateLeadDetail
        {
            public string ProductCode { get; set; }
            public string AnchorCompanyCode { get; set; }
            public string MobileNumber { get; set; }
            public string Email { get; set; }
            public string CustomerReferenceNo { get; set; }
            public int VintageDays { get; set; }
            public string  City { get; set; }
            public string State { get; set; }
            public List<BuyingHistories> BuyingHistories { get; set; }

        }

        public class BuyingHistories
        {
            public DateTime MonthFirstBuyingDate { get; set; }
            public int TotalMonthInvoice { get; set; }
            public int MonthTotalAmount { get; set; }
        }
        public class ScaleUpLead
        {
            public long LeadId { get; set; }
        }
        public class LeadRequestPost
        {
            public string companyCode { get; set; }
            public string ProductCode { get; set; }
            public string Mobile { get; set; }
            public string CustomerReferenceNo { get; set; }
            //public string token { get; set; }
        }

        public class HtmltoPdfDc
        {
            public string Html { get; set; }
        }
        public class Responsedc
        {
            public byte[] result { get; set; }
        }
        public class CustomerInvoiceData
        {
            public int custid { get; set; }
            public int TotalMonthInvoice { get; set; }
            public double InvoiceAmount { get; set; }
            public DateTime Startdate { get; set; }
            public int VintageDays { get; set; }
            public int orderid { get; set; }

        }
        public class ScaleUpResponse
        {
            public bool status { get; set; }
            public object message { get; set; }
            public string response { get; set; }
            public string MobileNo { get; set; }
            public string Company { get; set; }
            public string Product { get; set; }
            public string BaiseUrl { get; set; }
        }

        public class RefundTransactionResponse
        {
            public bool status { get; set; }
            public string Message { get; set; }
            public object Result { get; set; }
        }
    }
}
