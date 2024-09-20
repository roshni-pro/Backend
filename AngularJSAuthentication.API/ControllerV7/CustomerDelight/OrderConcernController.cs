using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Model.CustomerDelight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Masters;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Data.SqlClient;
using System.Configuration;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using NLog;
using System.Data.Entity;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.ControllerV7.CustomerDelight
{
    [RoutePrefix("api/OrderConcern")]
    public class OrderConcernController : BaseApiController
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        [Route("GetOrderConcern")]
        [HttpPost]
        public OrderConcernResDC GetOrderConcern(OrderConcernCount obj)
        {
            OrderConcernResDC res = new OrderConcernResDC();
            int totalItems = 0;
            List<OrderConcernDc> concernDc = new List<OrderConcernDc>();
            var manager = new OrderConcernManager();
            concernDc = manager.GetOrderConcern(obj, out totalItems);
            res.orderConcernDcs = concernDc;
            res.TotalCount = totalItems;
            return res;
        }

        [Route("PostOrderConcern")]
        [HttpPost]
        public bool PostOrderConcern(int OrderId, int userid)
        {
            OrderConcernManager manager = new OrderConcernManager();
            bool result = manager.PostOrderConcern(OrderId, userid);
            return result;
        }
        [Route("Test")]
        [HttpGet]
        public string Test()
        {
            string OrderConcernMessageFlag = Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
            return OrderConcernMessageFlag;
        }


        [Route("PostOrderForStatus")]
        [HttpPost]
        public bool PostOrderForStatus(OrderForStatus orderForStatus)
        {
            bool isResult;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            OrderConcernManager manager = new OrderConcernManager();
            isResult = manager.PostOrderForStatus(orderForStatus, userid);
            return isResult;
        }

        [Route("GetOrderConcernByOrderId")]
        [HttpGet]
        public OrderConcernDc GetOrderConcernByOrderId(int OrderId)
        {
            OrderConcernManager manager = new OrderConcernManager();
            var result = manager.GetOrderConcernByOrderId(OrderId);
            return result;
        }

        [Route("CustomerRaiseOrder")]
        [HttpPost]
        public OrderConcern CustomerRaiseOrder(CustomerRaiseCommentDc customerRaiseCommentDc)
        {
            OrderConcernManager manager = new OrderConcernManager();
            var list = manager.CustomerRaiseOrder(customerRaiseCommentDc);
            return list;
        }

        //sendmessage
        [Route("GenerateMessageForOrderConcern")]
        [HttpGet]
        public bool GenerateMessageForOrderConcern(int OrderId)
        {
            bool result = false;
            using (AuthContext context = new AuthContext())
            {
                var param = new SqlParameter("@OrderId", OrderId);
                var orderConcernData = context.Database.SqlQuery<OrderConcernDc>("OrderConcernByOrderId @OrderId", param).FirstOrDefault();
                if (!string.IsNullOrEmpty(orderConcernData.CustomerMobile))
                {
                    if (orderConcernData.IsCustomerRaiseConcern == false)
                    {
                        //var msg = "Link For Raise order Concern http://localhost:4200/#/order-concern/" + OrderId + "/" + orderConcernData.LinkId;
                        //var msg = "Link For Raise order Concern " + ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + OrderId + "/" + orderConcernData.LinkId;
                        // var msg = "Hi, Your Order Number  " + OrderId + ", has been delivered. In case of any concerns, please click on the link below " + ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + OrderId + "/" + orderConcernData.LinkId;// + ". ShopKirana";
                       
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivered_Concern");
                        string message = dltSMS == null ? "" : dltSMS.Template;
                        message = message.Replace("{#var1#}", orderConcernData.ShopName);
                        message = message.Replace("{#var2#}", OrderId.ToString());
                        string shortUrl = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + OrderId + "/" + orderConcernData.LinkId);
                        message = message.Replace("{#var3#}", shortUrl);

                        if (!string.IsNullOrEmpty(shortUrl) && dltSMS!=null)
                        {
                            Common.Helpers.SendSMSHelper.SendSMS(orderConcernData.CustomerMobile, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                            result = true;
                        }
                        else
                        {
                            logger.Error("Ërror during create url for delivery concern sms.");
                        }
                    }
                }
            }
            return result;
        }

        [Route("InsertOrderConcernMaster")]
        [HttpPost]
        public OrderConcernMasterDc InsertOrderConcernMaster(OrderConcernMasterDc orderConcernMasterDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                OrderConcernMaster list = new OrderConcernMaster();
                    list.CreatedDate = DateTime.Now;
                    list.CreatedBy = userid; 
                    list.IsActive = orderConcernMasterDc.IsActive;
                    list.Description = orderConcernMasterDc.Description;

                   db.Entry(list).State = EntityState.Added;
                    var a = db.Commit();
                    if (a > 0)
                    {
                    orderConcernMasterDc.Msg = "Data Added Sucesfully";
                    }
                    else
                    {
                    orderConcernMasterDc.Msg = "Something went wrong!";
                    }
 
                return orderConcernMasterDc;
            }

        }

        [Route("getOrderConcernMasterData")]
        [HttpGet]
        public async Task<List<OrderConcernMasterListDC>> getOrderConcernMasterData()
        {
            using (var context = new AuthContext())
            {
                List< OrderConcernMasterListDC> result = new List<OrderConcernMasterListDC>();
                result = await context.Database.SqlQuery<OrderConcernMasterListDC>("GetOrderConcernMasterDetail").ToListAsync();
                return result;
            }
        }

        [Route("getConcernListDataForWebView")]
        [HttpGet]
        public async Task<List<OrderConcernMasterListDC>> getConcernListDataForWebView()
        {
            using (var context = new AuthContext())
            {
                List<OrderConcernMasterListDC> result = new List<OrderConcernMasterListDC>();
                result = await context.Database.SqlQuery<OrderConcernMasterListDC>("GetOrderConcernMasterList").ToListAsync();
                return result;
            }
        }

        [Route("UpdateOrderConcernMaster")]
        [HttpPost]
        public async Task<OrderConcernMasterDc> UpdateOrderConcernMaster(OrderConcernMasterDc orderConcernMasterDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            
            using (var context = new AuthContext())
            {
                var concernData = context.OrderConcernMasterDB.Where(x => x.Id == orderConcernMasterDc.Id).FirstOrDefault();
                concernData.ModifiedDate = DateTime.Now;
                concernData.ModifiedBy = userid;
                concernData.IsActive = orderConcernMasterDc.IsActive;
                concernData.Description = orderConcernMasterDc.Description;

                context.Entry(concernData).State = EntityState.Modified;
                var a = context.Commit();
                if (a > 0)
                {
                    orderConcernMasterDc.Msg = "Data Update Sucesfully";
                }
                else
                {
                    orderConcernMasterDc.Msg = "Something went wrong!";
                }

                return orderConcernMasterDc;
            }


        }

        [Route("DeleteOrderConcernMaster")]
        [HttpGet]
        public async Task<OrderConcernMasterDc> DeleteOrderConcernMaster(int Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            OrderConcernMasterDc orderConcernMasterDc = new OrderConcernMasterDc();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var concernData = context.OrderConcernMasterDB.Where(x => x.Id == Id ).FirstOrDefault();
                concernData.ModifiedDate = DateTime.Now;
                concernData.ModifiedBy = userid;
                concernData.IsActive = false;
                concernData.IsDeleted = true;
                context.Entry(concernData).State = EntityState.Modified;
                var a = context.Commit();
                if (a > 0)
                {
                    orderConcernMasterDc.Msg = "Deleted Successfully";
                }
                else
                {
                    orderConcernMasterDc.Msg = "Something went wrong!";
                }

                return orderConcernMasterDc;
            }

        }
    }
}
