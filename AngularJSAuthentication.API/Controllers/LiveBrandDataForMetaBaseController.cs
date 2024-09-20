using AgileObjects.AgileMapper;
using AngularJSAuthentication.BusinessLayer.Managers;
using AngularJSAuthentication.DataContracts.Masters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/LiveBrandDataForMetaBase")]
    public class LiveBrandDataForMetaBaseController : ApiController
    {
        [Route("GetLiveBrandDataForMetaBaseList")]
        [HttpPost]
        [AllowAnonymous]
        public LiveBrandDataForMetaRes GetLiveBrandDataForMetaBaseList(LiveBrandDataForMetaBaseFilterDC obj)
        {
            int Skiplist = (obj.Skip - 1) * obj.Take;
            LiveBrandDataForMetaRes res = new LiveBrandDataForMetaRes();
            try
            {
                using (var context = new AuthContext())
                {
                    using (AuthContext db = new AuthContext())
                    {
                        LiveBrandDataForMetaRes liveBrandDataForMetaRes = new LiveBrandDataForMetaRes();
                        int totalItems = 0;
                        List<LiveBrandDataForMetaBaseDC> LiveBrandDataForMetabaseList = new List<LiveBrandDataForMetaBaseDC>();
                        var manager = new LiveBrandDataForMetabaseManager();
                        LiveBrandDataForMetabaseList = manager.GetLiveBrandDataForMetaBaseList(obj, out totalItems);
                        liveBrandDataForMetaRes.LiveBrandDataForMetaBaseDC = LiveBrandDataForMetabaseList;
                        liveBrandDataForMetaRes.TotalCount = totalItems;
                        return liveBrandDataForMetaRes;


                        //var param = new SqlParameter("@Skip", Skiplist);
                        //var param2 = new SqlParameter("@Take", obj.Take);
                        //var param3 = new SqlParameter("@brandIds", obj.BrandIds);
                        //var lst = context.Database.SqlQuery<LiveBrandDataForMetaBaseDC>("Exec GetLiveBrandData @brandIds, @Skip, @Take", param3, param, param2).ToList();
                        ////var _result = Mapper.Map(lst).ToANew<List<LiveBrandDataForMetaBaseDC>>();
                        //res.TotalCount = lst.Count();
                        //res.LiveBrandDataForMetaBaseDC = lst;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
    }
}