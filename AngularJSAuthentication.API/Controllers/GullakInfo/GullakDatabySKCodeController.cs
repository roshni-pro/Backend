using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.GullakInfo
{
    [RoutePrefix("api/getCustomerDetail")]
    public class GullakDatabySKCodeController : ApiController
    {
            [Route("GetCustomerList")]
            [HttpGet]
            public List<SKCodeDetailDC> GetCustomerList(string skcode)
            {
                List<SKCodeDetailDC> result = new List<SKCodeDetailDC>();
                using (var myContext = new AuthContext())
                {

                if (!string.IsNullOrEmpty(skcode))
                {
                    var code = new SqlParameter("@skcode", skcode);
                    result = myContext.Database
                       .SqlQuery<SKCodeDetailDC>("EXEC GetCustomerdetails @skcode", code)
                       .ToList();
                }
                else
                {
                    var code = new SqlParameter("@skcode", "");
                    result = myContext.Database
                   .SqlQuery<SKCodeDetailDC>("EXEC GetCustomerdetails @skcode", code)
                   .ToList();
                }  
                }
            return result;
            }

        [Route("GetCustomerComment")]
        [HttpGet]
        public List<SKCodeCommentDC> GetCustomercomment(string skcode)
        {
            List<SKCodeCommentDC> result = new List<SKCodeCommentDC>();
            using (var myContext = new AuthContext())
            {

                if (!string.IsNullOrEmpty(skcode))
                {
                    var code = new SqlParameter("@skcode", skcode);
                    result = myContext.Database
                       .SqlQuery<SKCodeCommentDC>("EXEC getCustomercommments @skcode", code)
                       .ToList();
                }
                
            }
            return result;
        }

        public class SKCodeDetailDC
        {
            public string Skcode { get; set; }
            public double TotalDr { get; set; }
            public double TotalCr { get; set; }
            public double Balance { get; set; }
            public double CurrentGullakAmount { get; set; }
        }
        public class SKCodeCommentDC
        {
            public string Skcode { get; set; }
            public string TransactionId { get; set; }
            public string Comment { get; set; }
            public DateTime Transactiondate { get; set; }
            public double Dr { get; set; }
            public double Cr { get; set; }
            public double Balance { get; set; }


        }
    }


}

