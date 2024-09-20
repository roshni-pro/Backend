using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CityBaseCustomerReward")]
    [Authorize]
    public class CityBaseCustomerRewardController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("")]
        [HttpGet]
        public dynamic AgentnDboyDevicehistoryy(int CityId)
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {

                var data = context.CityBaseCustomerRewards.Where(cb => !cb.IsDeleted && cb.CityId == CityId).Select(cb => new CityBaseCustomerRewardDto
                {
                    CreatedDate = cb.CreatedDate,
                    EndDate = cb.EndDate,
                    StartDate = cb.StartDate,
                    UpdateDate = cb.UpdateDate,
                    Id = cb.Id,
                    CityId = cb.CityId,
                    Point = cb.Point,
                    RewardType = cb.RewardType,
                    IsActive = cb.IsActive,
                    CityName = cb.City.CityName,
                    Customers = cb.RewardedCustomers.Select(x => new CityBaseCustomerDTO
                    {
                        CustomerId = x.CustomerId,
                        Mobile = x.Customer.Mobile,
                        SKCode = x.Customer.Skcode
                    }).ToList()
                }).ToList();


                return data;
            }
        }

        [Route("")]
        [HttpPost]
        public bool AddCityReward(CityBaseCustomerReward CityBaseCustomerReward)
        {
            bool results = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                var found = context.CityBaseCustomerRewards.Any(x => x.IsDeleted == false && x.IsActive == true && x.StartDate >= CityBaseCustomerReward.StartDate && x.EndDate <= CityBaseCustomerReward.EndDate
                 && x.CityId == CityBaseCustomerReward.CityId && x.RewardType == CityBaseCustomerReward.RewardType);
                if (!found)
                {
                    CityBaseCustomerReward.CreatedDate = indianTime;
                    CityBaseCustomerReward.CreatedBy = userid;
                    context.CityBaseCustomerRewards.Add(CityBaseCustomerReward);
                    results = context.Commit() > 0;
                }
            }
            return results;
        }

        [Route("")]
        [HttpPut]
        public bool UpdateCityReward(CityBaseCustomerReward CityBaseCustomerReward)
        {
            bool results = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                var found = context.CityBaseCustomerRewards.Any(x => x.IsDeleted == false && x.IsActive == true && x.StartDate >= CityBaseCustomerReward.StartDate && x.EndDate <= CityBaseCustomerReward.EndDate
                 && x.CityId == CityBaseCustomerReward.CityId && x.RewardType == CityBaseCustomerReward.RewardType && x.Id != CityBaseCustomerReward.Id);
                if (!found)
                {
                    var exist = context.CityBaseCustomerRewards.Where(x => x.IsDeleted == false && x.Id == CityBaseCustomerReward.Id).FirstOrDefault();
                    if (exist != null)
                    {
                        exist.StartDate = CityBaseCustomerReward.StartDate;
                        exist.IsActive = CityBaseCustomerReward.IsActive;
                        exist.EndDate = CityBaseCustomerReward.EndDate;
                        exist.RewardType = CityBaseCustomerReward.RewardType;
                        exist.UpdateDate = indianTime;
                        exist.UpdateBy = userid;
                        context.Entry(exist).State = EntityState.Modified;
                        results = context.Commit() > 0;
                    }

                }
            }
            return results;
        }


        [Route("")]
        [HttpDelete]
        public bool DeleteCityReward(CityBaseCustomerReward CityBaseCustomerReward)
        {
            bool results = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                var found = context.CityBaseCustomerRewards.Any(x => x.IsDeleted == false && x.IsActive == true && x.StartDate >= CityBaseCustomerReward.StartDate && x.EndDate <= CityBaseCustomerReward.EndDate
                 && x.CityId == CityBaseCustomerReward.CityId && x.RewardType == CityBaseCustomerReward.RewardType);
                if (!found)
                {
                    CityBaseCustomerReward.UpdateDate = indianTime;
                    CityBaseCustomerReward.UpdateBy = userid;
                    CityBaseCustomerReward.IsDeleted = true;
                    CityBaseCustomerReward.IsActive = true;
                    context.CityBaseCustomerRewards.Add(CityBaseCustomerReward);
                    context.Entry(CityBaseCustomerReward).State = EntityState.Modified;
                    results = context.Commit() > 0;
                }
            }
            return results;
        }



        public class CityBaseCustomerRewardDto
        {
            public int Id { get; set; }
            public int CityId { get; set; }
            public int Point { get; set; }
            public string RewardType { get; set; }
            public bool IsActive { get; set; }
            public int CreatedBy { get; set; }
            public int? UpdateBy { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdateDate { get; set; }
            public string CityName { get; set; }
            public List<CityBaseCustomerDTO> Customers { get; set; }
        }

        public class CityBaseCustomerDTO
        {
            public int CustomerId { get; set; }
            public string SKCode { get; set; }
            public string Mobile { get; set; }
        }
    }
}

