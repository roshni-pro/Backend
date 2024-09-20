using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/GroupNotification")]
    public class GroupNotificationController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();


        [Route("")]
        public IEnumerable<GroupNotification> Get()
        {
            logger.Info("start Store: ");
            List<GroupNotification> Stores = new List<GroupNotification>();
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    Stores = context.GetAllGroupNotification().ToList();
                    logger.Info("End  Store: ");
                    return Stores;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Store " + ex.Message);
                    logger.Info("End  Store: ");
                    return null;
                }
            }
        }


        //[Route("")]

        //public Slider Get(int id)
        //{
        //    logger.Info("start single Slider: ");
        //    Slider slider = new Slider();
        //    try
        //    {
        //        logger.Info("in Slider");

        //        slider = context.GetBySliderId(id);
        //        logger.Info("End Get Slider by  id: " + slider.Type);
        //        return slider;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Get Slider by Slider id " + ex.Message);
        //        logger.Info("End  single Slider: ");
        //        return null;
        //    }
        //}

        [ResponseType(typeof(GroupNotification))]
        [Route("")]
        [AcceptVerbs("POST")]
        public GroupNotification add(GroupNotification store)
        {
            logger.Info("Add Store: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    if (store == null)
                    {
                        throw new ArgumentNullException("Store");
                    }
                    List<Customer> customers = new List<Customer>();
                    foreach (var item in store.Customer)
                    {
                        Customer cuslist = context.Customers.Where(x => x.CustomerId == item.CustomerId).FirstOrDefault();
                        customers.Add(cuslist);
                    }
                    store.Customer = customers;
                    context.AddGroupNotification(store);
                    logger.Info("End  Add Store: ");
                    return store;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Store " + ex.Message);

                    return null;
                }
            }
        }

        //[ResponseType(typeof(Slider))]
        //[Route("")]
        //[AcceptVerbs("PUT")]
        //public GroupNotification Put(GroupNotification store)
        //{
        //    try
        //    {
        //        return context.PutGroupNotification(store);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Put Slider " + ex.Message);
        //        return null;
        //    }
        //}


        [ResponseType(typeof(GroupNotification))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    context.DeleteGroupNotification(id);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove Store " + ex.Message);

                }
            }
        }


    }
}



