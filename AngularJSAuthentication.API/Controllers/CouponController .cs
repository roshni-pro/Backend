using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Crete")]
    public class CouponController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("")]
        public IEnumerable<Coupon> Get()
        {
            logger.Info("start coupons: ");
            using (AuthContext context = new AuthContext())
            {
                List<Coupon> CouponList = new List<Coupon>();
                try
                {
                    //return context.GetAllCouopons().ToList();
                    CouponList = context.GetAllCouopons().ToList();
                    logger.Info("End  Coupon: ");
                    return CouponList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in coupons " + ex.Message);
                    logger.Info("End  coupons: ");
                    return null;
                }
            }
        }

        [Route("")]
        public Coupon Get(string id)
        {
            logger.Info("start single User: ");
            using (AuthContext context = new AuthContext())
            {
                Coupon coupon = new Coupon();

                try
                {
                    logger.Info("in user");

                    coupon = context.GetCouponbyId(id);
                    logger.Info("End Get coupon by id: " + coupon.OfferId);
                    return coupon;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }
        [ResponseType(typeof(Coupon))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Coupon add(Coupon coupon)
        {
            logger.Info("Add categories: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    if (coupon == null)
                    {
                        throw new ArgumentNullException("coupon");
                    }

                    context.AddCoupon(coupon);
                    logger.Info("End  Add coupons: ");
                    return coupon;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add coupon " + ex.Message);

                    return null;
                }
            }
        }

        //[ResponseType(typeof(Coupon))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Coupon Put(Coupon coupons)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    return context.PutCoupon(coupons);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put Coupon " + ex.Message);
                    return null;
                }
            }
        }

        //[ResponseType(typeof(Coupon))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    context.DeleteCoupon(id);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove Coupon " + ex.Message);

                }
            }
        }
    }
}
