using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using AngularJSAuthentication.Model.WarehouseUtilization;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.WarehouseUtilization
{
    [RoutePrefix("api/LMDVendor")]
    public class LMDVendorController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [HttpGet]
        [Route("InsertLMDVendor")]
        public async Task<UtilResponseDc> InsertLMDVendor(string Name)
        {
            UtilResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                LMDVendor lMDVendor = new LMDVendor
                {
                    VendorName = Name,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };
                db.LMDVendorDb.Add(lMDVendor);
                db.Commit();

                res = new UtilResponseDc
                {
                    Status = true,
                    Message = "Insert SuccessFully!! "
                };
                return res;
            }
        }

        [HttpGet]
        [Route("GetLMDVendorList")]
        public async Task<List<LmdVendorList>> GetLMDVendorList(string Keyword)
        {
            List<LmdVendorList> LmdVendorList = new List<LmdVendorList>();

            using (var context = new AuthContext())
            {
                LmdVendorList = context.LMDVendorDb.Where(x => x.IsActive == true && x.IsDeleted == false && (string.IsNullOrEmpty(Keyword) || x.VendorName.ToLower().Contains(Keyword.ToLower())))
                    .Select(y => new LmdVendorList
                    {
                        VendorName = y.VendorName,
                        Id = y.Id
                    }).ToList();
            }
            return LmdVendorList;
        }

       
        [HttpPost]
        [Route("UpdateLMDVendor")]
        public async Task<UtilResponseDc> UpdateLMDVendor(LmdVendorList input)
        {
            UtilResponseDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var VendorData = context.LMDVendorDb.Where(x => x.Id == input.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (VendorData != null)
                {
                    VendorData.VendorName = input.VendorName;
                    VendorData.ModifiedBy = userid;
                    VendorData.ModifiedDate = DateTime.Now;
                    context.Entry(VendorData).State = EntityState.Modified;
                    context.Commit();

                    res = new UtilResponseDc
                    {
                        Message = "Updated Sucessfully !!",
                        Status = true
                    };
                    return res;
                }
                else
                {
                    res = new UtilResponseDc
                    {
                        Message = "Failed!!",
                        Status = false
                    };
                    return res;
                }
            }
        }

        
        [HttpGet]
        [Route("DeleteLMDVendor")]
        public async Task<bool> DeleteLMDVendor(int Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var vendordata = context.LMDVendorDb.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (vendordata != null)
                {
                    vendordata.IsActive = false;
                    vendordata.IsDeleted = true;
                    context.Entry(vendordata).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return true;
        }
    }
}
