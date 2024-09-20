using AgileObjects.AgileMapper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{

    [RoutePrefix("api/DeliveryRedispatchCharge")]
    public class DeliveryRedispatchChargeController : ApiController
    {

        [Route("")]
        [HttpGet]
        public List<DeliveryRedispatchChargeConfDc> Get(int CityId)
        {
            List<DeliveryRedispatchChargeConfDc> result = null;
            if (CityId > 0)
            {
                using (var context = new AuthContext())
                {

                    var List = context.DeliveryRedispatchChargeConfs.Where(x => x.CityId == CityId && x.IsDeleted == false).ToList();
                    if (List != null && List.Any())
                    {
                        result = Mapper.Map(List).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                    }
                }
            }
            return result;

        }


        [Route("")]
        [HttpPost]
        public bool Add(DeliveryRedispatchChargeConfDc obj)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (obj != null && obj.CityId > 0 && obj.RedispatchCount > 0)
            {

                using (var context = new AuthContext())
                {
                    var IsCreated = context.DeliveryRedispatchChargeConfs.Any(x => x.CityId == obj.CityId && x.RedispatchCount == obj.RedispatchCount && x.IsDeleted == false);
                    if (!IsCreated)
                    {
                        DeliveryRedispatchChargeConf item = new DeliveryRedispatchChargeConf();
                        item.RedispatchCharge = obj.RedispatchCharge;
                        item.RedispatchCount = obj.RedispatchCount;
                        item.CityId = obj.CityId;
                        item.IsActive = true;
                        item.IsDeleted = false;
                        item.CreatedDate = DateTime.Now;
                        item.ModifiedDate = DateTime.Now;
                        item.CreatedBy = userid;
                        item.ModifiedBy = userid;
                        context.DeliveryRedispatchChargeConfs.Add(item);
                        result = context.Commit() > 0;
                    }
                    else
                    {
                        result = IsCreated;
                    }
                }
            }
            return result;
        }

        [Route("")]
        [HttpPut]
        public bool Update(DeliveryRedispatchChargeConfDc obj)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (obj != null && obj.RedispatchCount > 0 && obj.CityId > 0)
            {
                using (var context = new AuthContext())
                {
                    var Found = context.DeliveryRedispatchChargeConfs.Where(x => x.CityId == obj.CityId && x.RedispatchCount == obj.RedispatchCount && x.IsDeleted == false).FirstOrDefault();
                    if (Found != null)
                    {

                        Found.RedispatchCharge = obj.RedispatchCharge;
                        Found.IsActive = true;
                        Found.ModifiedDate = DateTime.Now;
                        Found.ModifiedBy = userid;
                        context.Entry(Found).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }
            }
            return result;

        }

    }
}