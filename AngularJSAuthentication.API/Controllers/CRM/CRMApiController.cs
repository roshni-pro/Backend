using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.BusinessLayer.CRM;
using AngularJSAuthentication.DataContracts.CRM;
using AngularJSAuthentication.DataContracts.External.CRM;
using AngularJSAuthentication.DataLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.CRM
{
    [AllowAnonymous]
    [RoutePrefix("api/crmapi")]
    public class CRMApiController : ApiController
    {
        [HttpGet]
        [Route("GetCustomerRawDataForCSV")]
        public async Task<List<CustomerRawDataForCsvDC>> GetCustomerRawDataForCSVAsync(DateTime CreatedDate)
        {
            CRMApiManager cRMApiManager = new CRMApiManager();
            return await cRMApiManager.GetCustomerRawDataForCSVAsync(CreatedDate);
        }


        [HttpPost]
        [Route("UpdateCRMSalesData")]
        public async Task<bool> UpdateCRMSalesData(List<SalesGroupSegmentDc> salesGroupSegmentList)
        {
            bool result = false;
            using (var context = new UnitOfWork())
            {
                result  = await context.CRMApiRepository.UpdateCRMSalesData(salesGroupSegmentList.First());
                context.Commit();
            }
            return result;
        }
        [HttpPost]
        [Route("Test")]
        public async Task<List<string>> GetCRMCustomer(dc dc)
        {
            CRMManager cRMManager = new CRMManager();
            return await cRMManager.GetCRMCustomer(dc.CrMId, dc.CrMPlatformIdList);
        }
        public class dc
        {
            public long CrMId { get; set; }
            public List<long> CrMPlatformIdList { get; set; }
        }
    }
}
