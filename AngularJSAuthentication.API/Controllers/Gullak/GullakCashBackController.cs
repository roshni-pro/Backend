using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.Gullak;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/GullakCashBack")]
    public class GullakCashBackController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [HttpGet]
        [Route("GetGullakCashBackList")]
        [AllowAnonymous]
        public ResponseGullakCashBackDc GullakCashBackList( int warehouseid, int Customertype)
        {
            ResponseGullakCashBackDc obj = new ResponseGullakCashBackDc();

            using (var Context = new AuthContext())
            {
               
               List <GullakCashBackDc> GullakCashBackList = new List<GullakCashBackDc>();
                var param = new SqlParameter("@WarehouseId", warehouseid);
                var param2 = new SqlParameter("@CustomerType", Customertype);
                GullakCashBackList = Context.Database.SqlQuery<GullakCashBackDc>("Exec SPGullakCashBackList @WarehouseId, @CustomerType",param,param2).ToList();
               // GullakCashBackList = Mapper.Map(data).ToANew<List<GullakCashBackDc>>();
                int count = GullakCashBackList.Count();
                obj.GullakCashBackDcs = GullakCashBackList;
                obj.TotalItem = count;
            }
            return obj;
        }

        [HttpPost]
        [Route("GullakCashBackSave")]
        [AllowAnonymous]
        public ResGullakCashBackSave GullakCashBackSave(GullakCashBackDc obj)
        {
            ResGullakCashBackSave res = new ResGullakCashBackSave
            {
                msg = "",
                Result = false
            };
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var CheckCashBack3 = context.GullakCashBackDB.Where(x => x.WarehouseId == obj.WarehouseId  && ((x.EndDate >= obj.EndDate && x.StartDate<=obj.EndDate) || (x.EndDate >= obj.StartDate && x.StartDate <= obj.EndDate))).ToList();

                if (CheckCashBack3 != null && CheckCashBack3.Any())
                {
                    if (CheckCashBack3.Any(x => x.AmountFrom <= obj.AmountFrom && x.AmountTo >= obj.AmountFrom && (x.CustomerType == obj.CustomerType || x.CustomerType == 3)))
                    {
                        res.Result = false;
                        res.msg = "Gullak CashBack Already Saved on this date.";
                        return res;
                    }
                    if (CheckCashBack3.Any(x => x.AmountFrom <= obj.AmountTo && x.AmountTo >= obj.AmountTo && (x.CustomerType == obj.CustomerType || x.CustomerType == 3)))
                    {
                        res.Result = false;
                        res.msg = "Gullak CashBack Already Saved on this date.";
                        return res;
                    }
                }

                var gullakcashback = context.GullakCashBackDB.FirstOrDefault(x => x.WarehouseId == obj.WarehouseId && x.Id==obj.Id && x.IsDeleted==false);
                if (gullakcashback != null)
                {
                        gullakcashback.WarehouseId = obj.WarehouseId;
                        gullakcashback.StartDate = obj.StartDate;
                        gullakcashback.EndDate = obj.EndDate;
                        gullakcashback.AmountFrom = obj.AmountFrom;
                        gullakcashback.AmountTo = obj.AmountTo;
                        gullakcashback.MaximumCashBackAmount = obj.MaximumCashBackAmount;
                        gullakcashback.CashBackPercent = obj.CashBackPercent;
                        gullakcashback.IsActive = obj.IsActive;
                        gullakcashback.IsDeleted = false;
                        gullakcashback.ModifiedBy = userid;
                        gullakcashback.ModifiedDate = indianTime;
                        gullakcashback.CustomerType = obj.CustomerType;
                       context.Entry(gullakcashback).State = EntityState.Modified;
                }
                else
                {
                    gullakcashback = new GullakCashBack
                    {
                        WarehouseId = obj.WarehouseId,
                        StartDate = obj.StartDate,
                        EndDate = obj.EndDate,
                        AmountFrom = obj.AmountFrom,
                        AmountTo = obj.AmountTo,
                        MaximumCashBackAmount = obj.MaximumCashBackAmount,
                        CashBackPercent = obj.CashBackPercent,
                        IsActive = obj.IsActive,
                        IsDeleted = false,
                        CreatedBy = userid,
                        CreatedDate = indianTime,
                        CustomerType=obj.CustomerType
                    };
                    context.GullakCashBackDB.Add(gullakcashback);
                }


                if (context.Commit() > 0)
                {
                    res.Result = true;
                    res.msg = "Gullak CashBack save successfully.";
                }
                else
                {
                    res.Result = false;
                    res.msg = "Gullak CashBack not save please try after some time.";
                }
                return res;
            }

        }

        [HttpGet]
        [Route("GullakCashBackStatusChange")]
        [AllowAnonymous]
        public ResGullakCashBackSave GullakCashBackStatusChange(int Id, bool status)
        {
            ResGullakCashBackSave res = new ResGullakCashBackSave
            {
                msg = "",
                Result = false
            };
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var gullakcashback = context.GullakCashBackDB.FirstOrDefault(x=>x.Id == Id);
                if (gullakcashback != null)
                {
                    gullakcashback.IsActive = status;
                    gullakcashback.IsDeleted = false;
                    gullakcashback.ModifiedBy = userid;
                    gullakcashback.ModifiedDate = indianTime;
                    context.Entry(gullakcashback).State = EntityState.Modified;
                    res.Result = context.Commit() > 0;
                    string strstatu = status ? "Active" : "Inactive";
                    res.msg = "Gullak CashBack " + strstatu + " successfully.";
                }
                else
                {
                    res.Result = false;
                    res.msg = "Gullak CashBack Item Not Found";
                }
                return res;
            }

        }
    }
}
