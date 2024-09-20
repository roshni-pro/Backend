using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataLayer.Infrastructure;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CustomerDelight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
namespace AngularJSAuthentication.API.Managers
{
    public class OrderConcernManager
    {
        public List<OrderConcernDc> GetOrderConcern(OrderConcernCount orderConcernCount,out int rowCount)
        {

            using (var context = new AuthContext())
            {
                using (var unitOfWork = new UnitOfWork())
                {
                    var res = unitOfWork.CustomersRepository.GetOrderConcern(orderConcernCount,out rowCount);
                    return res;
                }
                //var param = new SqlParameter("@keyword", keyword);
                //var param1 = new SqlParameter("@Status", Status);
                //var param2 = new SqlParameter("@Month", Month);
                //var param3 = new SqlParameter("@skip", skip);
                //var param4 = new SqlParameter("@take", take);
                //var orderConcernFilterData = context.Database.SqlQuery<OrderConcernDc>("Exec OrderConcernFilter @keyword,@Status ,@Month,@skip,@take", param, param1, param2, param3, param4).ToList();
                //return orderConcernFilterData;
            }
        }
       

        public bool PostOrderConcern( int OrderId,int userid)
        {
            Guid guid = Guid.NewGuid();
            var guidData = guid.ToString();
            bool res = false;
            using (var db = new AuthContext())
            {
                    db.OrderConcernDB.Add(new OrderConcern()
                    {
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        LinkId = guidData,
                        OrderId = OrderId,
                        CreatedBy = userid,
                        IsCustomerRaiseConcern = false
                    });

                 var a = db.Commit();
                if(a > 0)
                {
                    res = true;
                }
            }
            return res;
        }

        public bool PostOrderForStatus(OrderForStatus orderForStatus,int userid)
        {
            bool result = false;
            using (var db = new AuthContext())
            {
                    var list = db.OrderConcernDB.FirstOrDefault(x => x.Id == orderForStatus.Id);
                    //var identity = User.Identity as ClaimsIdentity;
                    //int userid = 0;
                    //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (list != null)
                    {

                        list.ModifiedDate = DateTime.Now;
                        list.ModifiedBy = userid;
                        list.Status = orderForStatus.Status;
                        list.CDComment = orderForStatus.CDComment;
                     
                        db.Entry(list).State = EntityState.Modified;
                        var a = db.Commit();
                        if(a > 0)
                        {
                            result = true;                            
                        }
                    }
                return result;
            }
           
        }

        public OrderConcernDc GetOrderConcernByOrderId(int OrderId)
        {
            int orderConcernTimeInHour = Int32.Parse(ConfigurationManager.AppSettings["OrderConcernTimeInHour"].ToString());
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@OrderId", OrderId);
                var orderConcernData = context.Database.SqlQuery<OrderConcernDc>("OrderConcernByOrderId @OrderId", param).FirstOrDefault();
                if (orderConcernData.IsCustomerRaiseConcern == false && orderConcernData.CreatedDate.AddHours(orderConcernTimeInHour) < DateTime.Now)
                {
                    orderConcernData.Msg = "Link Expired";
                }
                else if (orderConcernData.IsCustomerRaiseConcern == true)
                {
                    orderConcernData.Msg = "FEEDBACK ALREADY SUBMITED";
                }
                return orderConcernData;
            }
        }

        //public OrderConcern GetOrderRaise(int OrderId)
        //{

        //    int orderConcernTimeInHour = Int32.Parse(ConfigurationManager.AppSettings["OrderConcernTimeInHour"].ToString());

        //    using (var db = new AuthContext())
        //    {
        //        var list = db.OrderConcernDB.FirstOrDefault(x => x.OrderId == OrderId);
        //        if(list.IsCustomerRaiseConcern == false && list.CreatedDate.AddHours(orderConcernTimeInHour) < DateTime.Now)
        //        {
        //            list.Msg = "Link Expired";
        //        }
        //        else
        //        {
        //            list.Msg = "FEEDBACK ALREADY SUBMITED";
        //        }
        //        return list;
        //    }
        //}

        public OrderConcern CustomerRaiseOrder(CustomerRaiseCommentDc customerRaiseCommentDc)
        {
            using (var db = new AuthContext())
            {
                    var list = db.OrderConcernDB.FirstOrDefault(x => x.OrderId == customerRaiseCommentDc.OrderId && x.IsCustomerRaiseConcern == false && x.LinkId == customerRaiseCommentDc.LinkId );
                    if (list != null)
                    {
                        list.CreatedDate = DateTime.Now;
                        list.Status = "Pending";
                        list.IsCustomerRaiseConcern = true;
                        list.CustComment = customerRaiseCommentDc.CustComment;
                        list.OrderConcernMasterId = customerRaiseCommentDc.OrderConcernMasterId;

                        db.Entry(list).State = EntityState.Modified;
                        var a = db.Commit();
                        if(a > 0)
                        {
                            list.Msg = "Data Added Sucesfully";
                        }
                        else
                        {
                            list.Msg = "Something went wrong!";
                        }

                    }
                    else
                    {
                    list = db.OrderConcernDB.FirstOrDefault(x => x.OrderId == customerRaiseCommentDc.OrderId && x.IsCustomerRaiseConcern == true && x.LinkId == customerRaiseCommentDc.LinkId);
                    list.Msg = "Feedback Already Submitted";
                    }
                    return list;
            }

        }


    }
}