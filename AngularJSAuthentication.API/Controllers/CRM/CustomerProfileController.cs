using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.CRM;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.CRM
{
    [RoutePrefix("api/CustomerProfile")]
    public class CustomerProfileController : ApiController
    {
        [HttpGet]
        [Route("CallAndVisitHistoryDataGet")]
        public async Task<CallAndVisitHistoryDc> CallAndVisitHistoryDataGet(int CustomerId)
        {
            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            return await customerProfileManager.CallAndVisitHistoryDataGet(CustomerId);
        }


        [HttpGet]
        [Route("CallandVisitHistorySummaryGet")]
        public async Task<CallandHistorySummaryDc> CallandVisitHistorySummaryGet(long Id)
        {
            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            return await customerProfileManager.CallandVisitHistorySummaryGet(Id);
        }
        [Route("GetProfilingList")]
        [HttpPost]
        [AllowAnonymous]
        public CustomerProfileResponseDc GetProfilingList(CustomerProfileSearchDc customerProfileSearchDc)
        {
            CustomerProfileResponseDc customerProfileResponseDc = new CustomerProfileResponseDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0)
            {
                CustomerProfileManager customerProfileManager = new CustomerProfileManager();
                customerProfileResponseDc = customerProfileManager.GetProfilingList(customerProfileSearchDc, userid);
            }
            return customerProfileResponseDc;
        }

        [Route("Export")]
        [HttpPost]
        [AllowAnonymous]
        public string Export(CustomerProfileSearchDc customerProfileSearchDc)
        {
            CustomerProfileResponseDc customerProfileResponseDc = new CustomerProfileResponseDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            customerProfileResponseDc = customerProfileManager.GetProfilingList(customerProfileSearchDc, userid);
            if (customerProfileResponseDc != null && customerProfileResponseDc.customerProfileDcs != null && customerProfileResponseDc.customerProfileDcs.Any())
            {
                DataTable dt = ListtoDataTableConverter.ToDataTable(customerProfileResponseDc.customerProfileDcs);
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_CustomerProfilingExport.csv";
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);
                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }

        [Route("ExportAll")]
        [HttpPost]
        [AllowAnonymous]
        public string ExportAll(CustomerProfileSearchDc customerProfileSearchDc)
        {
            CustomerProfileResponseDc customerProfileResponseDc = new CustomerProfileResponseDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            customerProfileResponseDc = customerProfileManager.GetProfilingList(customerProfileSearchDc, userid, true);
            if (customerProfileResponseDc != null && customerProfileResponseDc.customerProfileDcs != null && customerProfileResponseDc.customerProfileDcs.Any())
            {
                DataTable dt = ListtoDataTableConverter.ToDataTable(customerProfileResponseDc.customerProfileDcs);
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_CustomerProfilingExportAll.csv";
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);
                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }

        [Route("IsPhysicalVisit")]
        [HttpGet]
        public bool IsPhysicalVisit(long CheckOutReasonId, bool IsPhysicalVisit)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var checkOutReason = context.CheckOutReasons.FirstOrDefault(x => x.Id == CheckOutReasonId);

                if (checkOutReason != null)
                {
                    checkOutReason.IsPhysicalVisit = IsPhysicalVisit;
                    checkOutReason.ModifiedBy = userid;
                    checkOutReason.ModifiedDate = DateTime.Today;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("CustomerProfilingInsertPhysicalVisit")]
        [HttpGet]
        public async Task<bool> CustomerProfilingInsertPhysicalVisit(long CustomerId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            return await customerProfileManager.CustomerProfilingInsertPhysicalVisit(CustomerId,userid);
        }

        [Route("IstellyCaller")]
        [HttpGet]
        public async Task<bool> IstellyCaller()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CustomerProfileManager customerProfileManager = new CustomerProfileManager();
            return await customerProfileManager.IstellyCaller(userid);
        }

    }
}
