using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ManualSalesOrder")]
    public class ManualSalesOrderController : ApiController
    {



        [Route("getAll")]
        [HttpGet]
        public dynamic Get()
        {
            using (var context = new AuthContext())
            {

                List<ManualSalesOrder> MSorderList = new List<ManualSalesOrder>();
                try
                {
                    //MSorderList = context.ManualSalesOrderDB.Include("manualSalesDetails").Where(x => x.Deleted == false).ToList();
                    //return MSorderList;


                    var GetBrand = (from c in context.ManualSalesOrderDB.Include("manualSalesDetails").Where(x => x.Deleted == false).OrderBy(x => x.CreatedDate)
                                    join p in context.Peoples.Where(x => x.Deleted == false)
                                    on c.UserId equals p.PeopleID into ps
                                    from p in ps.DefaultIfEmpty()
                                    select new
                                    {
                                        c.Name,
                                        c.MobileNo,
                                        c.PaymentThrough,
                                        c.ManualSalesOrderId,
                                        c.UpdatedDate,
                                        c.CreatedDate,
                                        c.Discription,
                                        c.TotalOrderAmount,
                                        c.Address,
                                        c.manualSalesDetails,
                                        p.Email,
                                        p.DisplayName,
                                        p.Mobile,



                                    }).ToList();
                    return GetBrand;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [Route("postManualOrder")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage postManualOrder(ManualSalesOrder msoDC)
        {
            try
            {
                if (msoDC.TotalOrderAmount > 0)
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    using (var context = new AuthContext())
                    {
                        ManualSalesOrder manualSalesOrder = new ManualSalesOrder();
                        msoDC.CreatedDate = DateTime.Now;
                        msoDC.UpdatedDate = DateTime.Now;
                        msoDC.UserId = userid;
                        foreach (var a in msoDC.manualSalesDetails)
                        {
                            a.UpdatedDate = DateTime.Now;
                            a.CreatedDate = DateTime.Now;

                        }


                        context.ManualSalesOrderDB.Add(msoDC);
                        context.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, "done");
                    }


                }
                else

                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "fail");

                }



            }
            catch (Exception ee)
            {
                throw ee;
            }

        }
    }

}



