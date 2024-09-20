using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Slider")]
    public class SliderController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();


        [Route("")]
        public IEnumerable<Slider> Get()
        {
            logger.Info("start Slider: ");
            List<Slider> Sliders = new List<Slider>();
            try
            {
                using (var context = new AuthContext())
                {
                    Sliders = context.GetAllSlider().ToList();
                }
                logger.Info("End  Slider: ");
                return Sliders;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Slider " + ex.Message);
                logger.Info("End  Slider: ");
                return null;
            }
        }


        [Route("")]

        public Slider Get(int id)
        {
            logger.Info("start single Slider: ");
            Slider slider = new Slider();
            try
            {
                logger.Info("in Slider");
                using (var db = new AuthContext())
                {
                    slider = db.GetBySliderId(id);
                    logger.Info("End Get Slider by  id: " + slider.Type);
                    return slider;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Get Slider by Slider id " + ex.Message);
                logger.Info("End  single Slider: ");
                return null;
            }
        }

        [ResponseType(typeof(Slider))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Slider add(Slider slider)
        {
            logger.Info("Add Slider: ");
            try
            {
                if (slider == null)
                {
                    throw new ArgumentNullException("Slider");
                }
                using (var db = new AuthContext())
                {
                    db.AddSlider(slider);
                    logger.Info("End  Add Slider: ");
                    return slider;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Slider " + ex.Message);

                return null;
            }
        }

        //[ResponseType(typeof(Slider))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Slider Put(Slider slider)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    return db.PutSlider(slider);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Put Slider " + ex.Message);
                return null;
            }
        }


        //[ResponseType(typeof(Groups))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            try
            {
                using (var db = new AuthContext())
                {
                    db.DeleteSlider(id);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Remove Slider " + ex.Message);

            }
        }


    }
}



