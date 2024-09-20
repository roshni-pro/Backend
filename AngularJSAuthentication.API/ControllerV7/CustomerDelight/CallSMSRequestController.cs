using System;
using System.Collections.Generic;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.CustomerDelight
{
    [RoutePrefix("api/CallSMSRequest")]
    public class CallSMSRequestController : ApiController
    {



        //[Route("getCallSMSRequest")]
        //[HttpPost]
        //[AllowAnonymous]
        //public CallpageinationData Get(callpageination Callpageination)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        CallpageinationData callpageinationData = new CallpageinationData();

        //        List<CallSMSRequestDC> MSorderList = new List<CallSMSRequestDC>();
        //        try
        //        {


        //            var query = (from c in context.CallSMSRequestDB.Where(x => x.IsDeleted == false).OrderBy(x => x.CreatedDate)
        //                         join p in context.Peoples.Where(x => x.Deleted == false)
        //                         on c.CreatedBy equals p.PeopleID
        //                         select new CallSMSRequestDC
        //                         {
        //                             ID = c.ID,
        //                             Title = c.Title,
        //                             Description = c.Description,
        //                             OnDate = c.OnDate,
        //                             ErrorIfAny = c.ErrorIfAny,
        //                             RequestCompletedDate = c.RequestCompletedDate,
        //                             IsCompleted = c.IsCompleted,
        //                             IsErrorOccured = c.IsErrorOccured,
        //                             IsProcessed = c.IsProcessed,
        //                             VoiceMessageURL = c.VoiceMessageURL,
        //                             SMSOne = c.SMSOne,
        //                             SMSTwo = c.SMSTwo,
        //                             VoiceMessagePlaceholder = c.VoiceMessagePlaceholder,
        //                             IsActive = c.IsActive,
        //                             IsDeleted = c.IsDeleted,
        //                             CreatedDate = c.CreatedDate,
        //                             CreatedBy = c.CreatedBy,
        //                             ModifiedDate = c.ModifiedDate,
        //                             ModifiedBy = c.ModifiedBy,
        //                             DisplayName = p.DisplayName,
        //                             Mobile = p.Mobile,
        //                         });

        //            callpageinationData.Total_count = query.Count();
        //            callpageinationData.MSorderList = query.OrderByDescending(x => x.CreatedDate).Skip(Callpageination.Skip).Take(Callpageination.Take).ToList();




        //            return callpageinationData;
        //        }
        //        catch (Exception ex)
        //        {

        //            return null;
        //        }
        //    }
        //}




        //[Route("getCustomerCallSMSRequest")]
        //[HttpPost]
        //[AllowAnonymous]
        //public CXCallpageinationData getCustomerCallSMSRequest(Cxcallpageination cxcallpageination)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        CXCallpageinationData callpageinationData = new CXCallpageinationData();
        //        List<CallSMSRequestDC> MSorderList = new List<CallSMSRequestDC>();
        //        var predicate = PredicateBuilder.True<CXcallpageDTO>();
        //        if (cxcallpageination.Warehouseid != null)
        //        {
        //            predicate = predicate.And(x => x.Warehouseid == cxcallpageination.Warehouseid);
        //        }
        //        if (cxcallpageination.Cityid != null)
        //        {
        //            predicate = predicate.And(x => x.Cityid == cxcallpageination.Cityid);
        //        }
        //        if (cxcallpageination.Active == true)
        //        {
        //            predicate = predicate.And(x => x.Active == cxcallpageination.Active);
        //        }
        //        if (cxcallpageination.Active == false)
        //        {
        //            predicate = predicate.And(x => x.Active == cxcallpageination.Active);
        //        }
        //        //predicate = predicate.And(x => x.Deleted == false);
        //        try
        //        {
        //            var query = (from cx in context.Customers.Where(x => x.Deleted == false)
        //                         join wr in context.Warehouses
        //                         on cx.Warehouseid equals wr.WarehouseId
        //                         select new CXcallpageDTO
        //                         {
        //                             WarehouseName = wr.WarehouseName,
        //                             ShopName = cx.ShopName,
        //                             Skcode = cx.Skcode,
        //                             Name = cx.Name,
        //                             City = cx.City,
        //                             Mobile = cx.Mobile,
        //                             Active = cx.Active,
        //                             CreatedDate =cx.CreatedDate,
        //                             Cityid = cx.Cityid,
        //                             Warehouseid = cx.Warehouseid
        //                         });
        //            callpageinationData.Total_count = query.Where(predicate).Count();
        //            callpageinationData.CustomerPagerList = query.Where(predicate).OrderByDescending(x => x.CreatedDate).Skip(cxcallpageination.Skip).Take(cxcallpageination.Take).ToList();
        //            return callpageinationData;
        //        }
        //        catch (Exception ex)
        //        {

        //            return null;
        //        }
        //    }
        //}

    }

    public class callpageination
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }



    public class Cxcallpageination
    {
        public int? Cityid { get; set; }
        public int? Warehouseid { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool Active { get; set; }

    }

    public class CallpageinationData
    {
        public List<CallSMSRequestDC> MSorderList { get; set; }
        public int Total_count { get; set; }

    }


    public class CXCallpageinationData
    {
        public List<CXcallpageDTO> CustomerPagerList { get; set; }
        public int Total_count { get; set; }

    }

    public class CXcallpageDTO
    {
        public int? Cityid { get; set; }
        public int? Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }



    public class CallSMSRequestDC
    {

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? OnDate { get; set; }
        public string ErrorIfAny { get; set; }
        public DateTime? RequestCompletedDate { get; set; }
        public Boolean IsCompleted { get; set; }
        public Boolean IsErrorOccured { get; set; }
        public Boolean IsProcessed { get; set; }
        public string VoiceMessageURL { get; set; }
        public string SMSOne { get; set; }
        public string SMSTwo { get; set; }
        public string VoiceMessagePlaceholder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public string DisplayName { get; set; }
        public string Mobile { get; set; }

    }
}
