using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.KKReturnReplace
{
    [RoutePrefix("api/KKReturnReplaceHistory")]
    public class KKReturnReplaceHistoryController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool AddKKReturnReplaceHistory(KKRequestReplaceHistory kkHIstory)
        {
            var result = false;
            using (AuthContext context = new AuthContext())
            {
                context.KKRequestReplaceHistorys.Add(kkHIstory);
                if (context.Commit() > 0)
                {
                    result = true;
                    return result;
                }
                return result;
            }
        }
       
        [Route("GetReturnReplaceHistoryList")]
        [HttpGet]
        public async Task<List<GetReturnReplaceHistoryDC>> GetReturnReplaceHistoryList(int KKRequestId)
        {
            {
                KKReturnReplaceManager manager = new KKReturnReplaceManager();
                return await manager.GetReturnReplaceHistoryList(KKRequestId);
            }
        }
        
        [Route("GetReturnReplaceItemList")]
        [HttpGet]
        public async Task<List<GetReturnReplaceItemDc>> GetReturnReplaceItemList(long KKRequestId)
        {
            if (!IsSalesReturn())
            {
                return new List<GetReturnReplaceItemDc>();
            }
            var identity = User.Identity as ClaimsIdentity;
            List<string> roleNames = new List<string>();
            int roleId = 0;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (roleId == 0)
            {
                if (roleNames.Any(x => x == "Inbound Lead"))
                    roleId = 1;
                else if (roleNames.Any(x => x == "Buyer"))
                    roleId = 3;
                else if (roleNames.Any(x => x == "WH delivery planner" || x == "Hub delivery planner"))
                    roleId = 5;
                else
                    roleId = 0;
            }

            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetReturnReplaceItemList(KKRequestId,roleId, userid);
        }

        public bool IsSalesReturn()
        {
            bool IsReturn = false;
            MongoDbHelper<SalesReturnConfiguration> mongohelper = new MongoDbHelper<SalesReturnConfiguration>();
            var res = mongohelper.Select(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            if (res != null)
            {
                IsReturn = res.IsSalesReturn;
            }
            return IsReturn;
        }
    }
}
