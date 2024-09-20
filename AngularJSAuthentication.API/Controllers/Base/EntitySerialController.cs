using AngularJSAuthentication.Model.Base;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Base
{
    [RoutePrefix("api/EntitySerial")]
    public class EntitySerialController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("AddUpdateEntitySerialNumber")]
        [HttpPost]
        public async Task<bool> AddUpdateEntitySerialNumber(EntitySerialMaster entity)
        {
            if (entity == null)
                return false;

            var identity = User.Identity as ClaimsIdentity;
            int userid = Convert.ToInt32(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var authContext = new AuthContext())
            {

                if (entity.Id > 0)
                {
                    var srMaster = authContext.EntitySerialMasters.FirstOrDefault(x => x.Id == entity.Id);
                    srMaster.UpdatedDate = indianTime;
                    srMaster.ModidfiedBy = userid;

                    authContext.Entry(srMaster).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    entity.CreatedDate = indianTime;
                    entity.CreatedBy = userid;
                    authContext.EntitySerialMasters.Add(entity);
                }
                await authContext.CommitAsync();
            }

            return true;
        }


        [Route("GetEntitySerialNumbers")]
        [HttpGet]
        public async Task<List<EntitySerialMaster>> GetEntitySerialNumbers(long EntityId)
        {
            using (var authContext = new AuthContext())
            {
                return await authContext.EntitySerialMasters.Where(x => x.EntityId == EntityId).ToListAsync();
            }

        }

        [Route("GetEntitySerialNumber")]
        [HttpGet]
        public async Task<EntitySerialMaster> GetEntitySerialNumber(long Id)
        {
            using (var authContext = new AuthContext())
            {
                return await authContext.EntitySerialMasters.FirstOrDefaultAsync(x => x.Id == Id);
            }

        }
    }
}
