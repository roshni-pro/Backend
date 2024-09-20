using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/MessageApi")]
    public class MessageController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("")]
        public IEnumerable<Message> Get()
        {
            using (var context = new AuthContext())
            {

                logger.Info("start Message: ");
                List<Message> MessageList = new List<Message>();
                try
                {
                    MessageList = context.GetAllMessage().ToList();
                    logger.Info("End  Message: ");
                    return MessageList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Message " + ex.Message);
                    logger.Info("End  Message: ");
                    return null;
                }
            }
        }

        [Route("")]
        public Message Get(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start single Message: ");
                Message message = new Message();
                try
                {
                    logger.Info("in Message");

                    message = context.GetMessagebyId(id);
                    logger.Info("End Get message by item id: " + message.MessageTitle);
                    return message;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get message by item id " + ex.Message);
                    logger.Info("End  single message: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(Message))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Message add(Message message)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Add message: ");
                try
                {
                    if (message == null)
                    {
                        throw new ArgumentNullException("message");
                    }

                    context.AddMessage(message);
                    logger.Info("End  Add message: ");
                    return message;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add message " + ex.Message);

                    return null;
                }
            }
        }

        //[ResponseType(typeof(Message))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Message Put(Message message)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    return context.PutMessage(message);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put Message " + ex.Message);
                    return null;
                }
            }
        }


        ////[ResponseType(typeof(Message))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public string Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("DELETE Remove: ");
                try
                {
                    if (context.DeleteMessage(id))
                    {
                        return "success";
                    }
                    else
                    {
                        return "error";
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove Message " + ex.Message);
                    return "error";
                }
            }
        }

    }



}
