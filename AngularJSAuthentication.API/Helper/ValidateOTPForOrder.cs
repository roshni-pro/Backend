
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class ValidateOTPForOrder
    {
        public bool ValidateOTPForOrders(string otp, int OrderId, string Status, AuthContext context)
        {
            bool result = false;

            var Data = context.OrderDeliveryOTP.FirstOrDefault(x => x.OrderId == OrderId && x.Status == Status && x.OTP == otp && x.IsActive == true && x.IsUsed == false);
            //var UsedOtp = context.OrderDeliveryOTP.FirstOrDefault(x => x.OrderId == OrderId && x.IsUsed);
            if (Data != null)
            {
                DateTime TommorowDate = DateTime.Now.Date.AddDays(1);
                DateTime TodayDate = DateTime.Now.Date;
                var people = context.Peoples.Where(x => x.PeopleID == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                if (Data.UserType == "Customer")
                {
                    var Customer = context.Customers.Where(x => x.CustomerId == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                    MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
                    if (Customer != null)
                    {
                        OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                        {
                            CreatedBy = Customer.CustomerId,
                            CreatedByName = Customer.Name,
                            CreatedDate = DateTime.Now,
                            OrderId = OrderId,
                            OTP = otp,
                            OrderStatus = Status

                        };
                        mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    }

                    //var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId && x.CreatedDate >= TodayDate && x.CreatedDate < TommorowDate).ToList(); //&& x.OrderStatus == Status
                    //var news = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();
                    //if (news != null)
                    //{
                    //    foreach (var i in news)
                    //    {
                    //        mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                    //    }
                    //}
                    //if (UsedOtp == null)
                    //{
                    //    var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId).ToList();
                    //    var ExistEntry = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();
                    //    if (ExistEntry != null && ExistEntry.Count() > 0)
                    //    {
                    //        foreach (var i in ExistEntry)
                    //        {
                    //            mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                    //        }
                    //    }
                    //}
                    //if (Customer != null)
                    //{
                    //    OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                    //    {
                    //        CreatedBy = Customer.CustomerId,
                    //        CreatedByName = Customer.Name,
                    //        CreatedDate = DateTime.Now,
                    //        OrderId = OrderId,
                    //        OTP = otp,
                    //        OrderStatus = Status

                    //    };
                    //    mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    //}

                }
                else if (Data.UserType == "Sales"  || Data.UserType == "Digital")
                {
                    //var Sales = context.Peoples.Where(x => x.PeopleID == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                    MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
                    if (people != null)
                    {
                        OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                        {
                            CreatedBy = people.PeopleID,
                            CreatedByName = people.PeopleFirstName + " " + people.PeopleLastName,
                            CreatedDate = DateTime.Now,
                            OrderId = OrderId,
                            OTP = otp,
                            OrderStatus = Status

                        };
                        mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    }
                    //var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId && x.CreatedDate >= TodayDate && x.CreatedDate < TommorowDate).ToList();
                    //var news = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();
                    //if (news != null && news.Count() > 0)
                    //{
                    //    foreach (var i in news)
                    //    {
                    //        mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                    //    }
                    //}
                    //if (UsedOtp == null)
                    //{
                    //    var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId).ToList();
                    //    var ExistEntry = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();
                    //    if (ExistEntry != null && ExistEntry.Count() > 0)
                    //    {
                    //        foreach (var i in ExistEntry)
                    //        {
                    //            mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                    //        }
                    //    }
                    //}
                    //if (Sales != null)
                    //{
                    //    OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                    //    {
                    //        CreatedBy = Sales.PeopleID,
                    //        CreatedByName = Sales.PeopleFirstName + " " + Sales.PeopleLastName,
                    //        CreatedDate = DateTime.Now,
                    //        OrderId = OrderId,
                    //        OTP = otp,
                    //        OrderStatus = Status

                    //    };
                    //    mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    //}


                }
                else if (Data.UserType == "HQ Customer Delight" || Data.UserType == "HQ Operation"  || Data.UserType == "HQ Operation(ReAttempt)")
                {
                    //var people = context.Peoples.Where(x => x.PeopleID == Data.UserId).FirstOrDefault();
                    MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
                    if (people != null)
                    {
                        OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                        {
                            CreatedBy = people.PeopleID,
                            CreatedByName = people.PeopleFirstName + " " + people.PeopleLastName,
                            CreatedDate = DateTime.Now,
                            OrderId = OrderId,
                            OTP = otp,
                            OrderStatus = Status

                        };
                        mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    }


                    //var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId && x.CreatedDate >= TodayDate && x.CreatedDate < TommorowDate).ToList();
                    //var ExistEntry = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();

                    //var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId).ToList();
                    //var ExistEntry = momgodata.Where(x => x.OrderId == OrderId && x.OTP != otp).Select(y => y.Id).ToList();
                    //var NewEntry = momgodata.Where(x => x.OrderId == OrderId && x.OTP == otp).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                    ////if (ExistEntry != null && ExistEntry.Count() > 0)
                    ////{
                    ////    foreach (var i in ExistEntry)
                    ////    {
                    ////        mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");

                    ////    }
                    ////}
                    //if (UsedOtp == null)
                    //{

                    //    if (ExistEntry != null && ExistEntry.Count() > 0)
                    //    {
                    //        foreach (var i in ExistEntry)
                    //        {
                    //            mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                    //        }
                    //    }
                    //}
                    //if (NewEntry != null)
                    //{
                    //    OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                    //    {
                    //        CreatedBy = NewEntry.CreatedBy,
                    //        CreatedByName = NewEntry.CreatedByName,
                    //        CreatedDate = DateTime.Now,
                    //        OrderId = OrderId,
                    //        OTP = otp,
                    //        OrderStatus = Status

                    //    };
                    //    mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    //}

                }
                //else if (Data.UserType == "Sales(Reattempt)")
                //{
                //    MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
                //    if (people != null)
                //    {
                //        OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                //        {
                //            CreatedBy = people.PeopleID,
                //            CreatedByName = people.PeopleFirstName + " " + people.PeopleLastName,
                //            CreatedDate = DateTime.Now,
                //            OrderId = OrderId,
                //            OTP = otp,
                //            OrderStatus = Status

                //        };
                //        mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                //    }

                //    //var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId && x.CreatedDate >= TodayDate && x.CreatedDate < TommorowDate).ToList();
                //    //var ExistEntry = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();

                //    //if (ExistEntry != null && ExistEntry.Count() > 0)
                //    //{
                //    //    foreach (var i in ExistEntry)
                //    //    {
                //    //        mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");

                //    //    }
                //    //}
                //    //if (UsedOtp == null)
                //    //{
                //    //    var momgodata = mongoDbHelper.Select(x => x.OrderId == OrderId).ToList();
                //    //    var ExistEntry = momgodata.Where(x => x.OrderId == OrderId).Select(y => y.Id).ToList();
                //    //    if (ExistEntry != null && ExistEntry.Count() > 0)
                //    //    {
                //    //        foreach (var i in ExistEntry)
                //    //        {
                //    //            mongoDbHelper.Delete(i, "OrderDeliveryOTPHistory");
                //    //        }
                //    //    }
                //    //}
                //    //if (Sales != null)
                //    //{
                //    //    OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                //    //    {
                //    //        CreatedBy = Sales.PeopleID,
                //    //        CreatedByName = Sales.PeopleFirstName + " " + Sales.PeopleLastName,
                //    //        CreatedDate = DateTime.Now,
                //    //        OrderId = OrderId,
                //    //        OTP = otp,
                //    //        OrderStatus = Status

                //    //    };
                //    //    mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                //    //}
                //}
                if (Data.Status == "Delivered")
                {
                    //var people = context.Peoples.Where(x => x.PeopleID == Data.UserId).FirstOrDefault();
                    MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory> mongoDbHelper = new MongoDbHelper<OrderMasterrController.OrderDeliveryOTPHistory>();
                    if (people != null)
                    {
                        Data.UserType = people.Department;
                        OrderMasterrController.OrderDeliveryOTPHistory OrderDeliveryOTPHistory = new OrderMasterrController.OrderDeliveryOTPHistory
                        {
                            CreatedBy = people.PeopleID,
                            CreatedByName = people.PeopleFirstName + " " + people.PeopleLastName,
                            CreatedDate = DateTime.Now,
                            OrderId = OrderId,
                            OTP = otp,
                            OrderStatus = Status

                        };
                        mongoDbHelper.Insert(OrderDeliveryOTPHistory);
                    }
                }

                Data.IsUsed = true;
                Data.ModifiedBy = 0;
                Data.ModifiedDate = DateTime.Now;
                context.Entry(Data).State = EntityState.Modified;
                result = true;
                return result;

            }
            else
            {
                result = false;
                return result;
            }
        }
    }
}