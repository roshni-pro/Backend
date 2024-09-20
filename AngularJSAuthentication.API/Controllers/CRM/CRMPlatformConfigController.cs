using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.DataContracts.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.CRM
{
    [RoutePrefix("api/CRMPlatformConfig")]
    public class CRMPlatformConfigController : ApiController
    {
        #region For CRM

        [HttpGet]
        [Route("CRMPlatformConfigGetList")]
        public async Task<List<CRMPlatformConfigListDc>> CRMPlatformConfigGetList()
        {
            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.CRMPlatformConfigGetList();
        }

        [HttpGet]
        [Route("ActiveInactiveCRMTag")]
        public async Task<bool> ActiveInactiveCRMTag(long Id, bool IsActive)
        {
            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.ActiveInactiveCRMTag(Id, IsActive);
        }

        [HttpPost]
        [Route("InsertCRMPlatformConfig")]
        public async Task<bool> InsertCRMPlatformConfig(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.InsertCRMPlatformConfig(insertCRMPlatformConfigDc, userid);
        }

        [HttpPost]
        [Route("UpdateCRMPlatformConfig")]
        public async Task<bool> UpdateCRMPlatformConfig(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.UpdateCRMPlatformConfig(insertCRMPlatformConfigDc, userid);
        }

        [HttpGet]
        [Route("DeleteCRMPlatformConfig")]
        public async Task<bool> DeleteCRMPlatformConfig(long Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.DeleteCRMPlatformConfig(Id,userid);
        }

        [HttpGet]
        [Route("GetCRMPlatformConfigById")]
        public async Task<List<string>> GetCRMPlatformConfigById(int Id)
        {
            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.GetCRMPlatformConfigById(Id);
        }

        #endregion

        #region For CRMPlatform
        [HttpPost]
        [Route("InsertCRMPlatform")]
        public async Task<bool> InsertCRMPlatform(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.InsertCRMPlatform(insertCRMPlatformConfigDc, userid);
        }

        [HttpPost]
        [Route("UpdateCRMPlatform")]
        public async Task<bool> UpdateCRMPlatform(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.UpdateCRMPlatform(insertCRMPlatformConfigDc, userid);
        }

        [HttpGet]
        [Route("CRMPlatformGetList")]
        public async Task<List<CRMPlatformListDc>> CRMPlatformGetList()
        {
            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.CRMPlatformGetList();
        }

        [HttpGet]
        [Route("DeleteCRMPlatform")]
        public async Task<bool> DeleteCRMPlatform(int Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.DeleteCRMPlatform(Id, userid);
        }

        #endregion

        #region For CRMPlatformMapping

        [HttpGet]
        [Route("CRMPlatformMappingGetList")]
        public async Task<List<CRMPlatformMappingListDc>> CRMPlatformMappingGetList()
        {
            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.CRMPlatformMappingGetList();
        }

        [HttpGet]
        [Route("CRMPlatformMappingGetList")]
        public async Task<bool> ActiveInactiveCRMPlatformMapping(long CrmId, int CrmPlatformId, bool UpdateCrmPlatformMapping)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CRMPlatformConfigManager cRMPlatformConfigManager = new CRMPlatformConfigManager();
            return await cRMPlatformConfigManager.ActiveInactiveCRMPlatformMapping(CrmId, CrmPlatformId, UpdateCrmPlatformMapping, userid);
        }
        #endregion
    }
}
