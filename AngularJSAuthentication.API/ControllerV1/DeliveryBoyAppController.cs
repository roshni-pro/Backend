using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.NotMapped;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AngularJSAuthentication.API.Helper.Notification;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DBSignup")]
    public class DeliveryBoyAppController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        //#region  DeliveryBoy: Version 2 DeliveryBoy Login
        ///// <summary>
        ///// DeliveryBoy Login
        ///// </summary>
        ///// <param name="Mobile"></param>
        ///// <param name="Password"></param>
        ///// <returns>Peopel Object</returns>
        //[Route("V2")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage PostV2(People customer)
        //{
        //    ResDTO res;
        //    People People = new People();

        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            string query = "select distinct p.PeopleID, p.PeopleID,STRING_AGG(r.Name,',') RoleName from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where Mobile='" + customer.Mobile + "'  and ur.isActive=1 and p.Active=1 and p.Deleted=0 group by p.PeopleID having STRING_AGG(r.Name,',') like '%Delivery Boy%'";
        //            PeopleRole peopleRole = context.Database.SqlQuery<PeopleRole>(query).FirstOrDefault();

        //            int? PeopleId = peopleRole != null && peopleRole.RoleName.Split(',').Contains("Delivery Boy") ? peopleRole.peopleId : (int?)null;
        //            if (!PeopleId.HasValue)
        //            {
        //                res = new ResDTO()
        //                {
        //                    P = null,
        //                    Status = false,
        //                    Message = "Not a Registered Delivery Boy"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            People = context.Peoples.Where(x => x.PeopleID == PeopleId).SingleOrDefault();
        //            if (People == null)
        //            {
        //                res = new ResDTO()
        //                {
        //                    P = null,
        //                    Status = false,
        //                    Message = "Not a Registered Delivery Boy"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else if (People.Password != customer.Password)
        //            {
        //                res = new ResDTO()
        //                {
        //                    P = null,
        //                    Status = false,
        //                    Message = "Wrong Password"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else if (People.Password == customer.Password && People.Mobile == customer.Mobile)
        //            {


        //                if (People.DeviceId == null)
        //                {
        //                    People.FcmId = customer.FcmId;
        //                    People.DeviceId = customer.DeviceId;
        //                    People.UpdatedDate = indianTime;
        //                    People.CurrentAPKversion = customer.CurrentAPKversion;    // tejas to save device info 
        //                    People.PhoneOSversion = customer.PhoneOSversion;
        //                    People.IMEI = customer.IMEI;// Sudhir to save device info 
        //                    People.UserDeviceName = customer.UserDeviceName;
        //                    //context.Peoples.Attach(People);
        //                    context.Entry(People).State = EntityState.Modified;
        //                    context.Commit();

        //                }
        //                else if (People.DeviceId.Trim().ToLower() == customer.DeviceId.Trim().ToLower())
        //                {
        //                    if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
        //                    {
        //                        People.FcmId = customer.FcmId;
        //                        People.UpdatedDate = indianTime;
        //                        People.CurrentAPKversion = customer.CurrentAPKversion;      // tejas to save device info 
        //                        People.PhoneOSversion = customer.PhoneOSversion;
        //                        People.IMEI = customer.IMEI;// Sudhir to save device info 
        //                        People.UserDeviceName = customer.UserDeviceName;
        //                        //context.Peoples.Attach(People);
        //                        context.Entry(People).State = EntityState.Modified;
        //                        context.Commit();
        //                        #region Device History
        //                        var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
        //                        try
        //                        {
        //                            PhoneRecordHistory phonerecord = new PhoneRecordHistory();
        //                            if (Customerhistory != null)
        //                            {
        //                                phonerecord.PeopleID = Customerhistory.PeopleID;
        //                                phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
        //                                phonerecord.Department = Customerhistory.Department;
        //                                phonerecord.Mobile = Customerhistory.Mobile;
        //                                phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
        //                                phonerecord.IMEI = Customerhistory.IMEI;
        //                                phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
        //                                phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
        //                                phonerecord.UpdatedDate = DateTime.Now;
        //                                context.PhoneRecordHistoryDB.Add(phonerecord);
        //                                int id = context.Commit();
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                        }
        //                        #endregion
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        SendLogOutNotification(People); //send Notification Dboy for Sign out
        //                    }
        //                    catch (Exception ds)
        //                    {
        //                    }

        //                    People.FcmId = customer.FcmId;
        //                    People.DeviceId = customer.DeviceId;
        //                    People.UpdatedDate = indianTime;
        //                    People.CurrentAPKversion = customer.CurrentAPKversion;        // tejas to save device info 
        //                    People.PhoneOSversion = customer.PhoneOSversion;
        //                    People.IMEI = customer.IMEI;// Sudhir to save device info 
        //                    People.UserDeviceName = customer.UserDeviceName;
        //                    //context.Peoples.Attach(People);
        //                    context.Entry(People).State = EntityState.Modified;
        //                    #region Device History
        //                    var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
        //                    try
        //                    {
        //                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
        //                        if (Customerhistory != null)
        //                        {
        //                            phonerecord.PeopleID = Customerhistory.PeopleID;
        //                            phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
        //                            phonerecord.Department = Customerhistory.Department;
        //                            phonerecord.Mobile = Customerhistory.Mobile;
        //                            phonerecord.IMEI = Customerhistory.IMEI;
        //                            phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
        //                            phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
        //                            phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
        //                            phonerecord.UpdatedDate = DateTime.Now;
        //                            context.PhoneRecordHistoryDB.Add(phonerecord);
        //                            int id = context.Commit();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                    }
        //                    #endregion
        //                    context.Commit();
        //                }

        //                //TEMP obj
        //                PeopleTemp PtData = new PeopleTemp();
        //                PtData.PeopleID = People.PeopleID;
        //                PtData.Skcode = People.Skcode;
        //                PtData.CompanyId = People.CompanyId;
        //                PtData.WarehouseId = People.WarehouseId;
        //                PtData.PeopleFirstName = People.PeopleFirstName;
        //                PtData.PeopleLastName = People.PeopleLastName;
        //                PtData.Email = People.Email;
        //                PtData.DisplayName = People.DisplayName;
        //                PtData.Mobile = People.Mobile;
        //                PtData.Password = People.Password;
        //                PtData.VehicleId = People.VehicleId;
        //                PtData.VehicleName = People.VehicleName;
        //                PtData.VehicleNumber = People.VehicleNumber;
        //                PtData.VehicleCapacity = People.VehicleCapacity;
        //                PtData.CreatedDate = People.CreatedDate;
        //                PtData.UpdatedDate = People.UpdatedDate;
        //                PtData.DeviceId = People.DeviceId;
        //                PtData.FcmId = People.FcmId;
        //                PtData.ImageUrl = People.ImageUrl;
        //                PtData.Role = peopleRole.RoleName;
        //                if (PtData != null)
        //                {
        //                    var registeredApk = context.GetAPKUserAndPwd("DeliveryApp");
        //                    PtData.RegisteredApk = registeredApk;
        //                }

        //                res = new ResDTO()
        //                {
        //                    P = PtData,
        //                    Status = true,
        //                    Message = "Success."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                res = new ResDTO()
        //                {
        //                    P = null,
        //                    Status = false,
        //                    Message = "Failed. Something went wrong"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //        catch (Exception es)
        //        {
        //            res = new ResDTO()
        //            {
        //                P = null,
        //                Status = false,
        //                Message = ("Not a Registered Delivery Boy : " + es)
        //            };
        //        };
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, res);

        //}
        //#endregion

        #region  DeliveryBoy: Version 2 New Last Mile DeliveryBoy Login
        /// <summary>
        /// DeliveryBoy Login
        /// </summary>
        /// <param name="Mobile"></param>
        /// <param name="Password"></param>
        /// <returns>Peopel Object</returns>
        [Route("NewDeliveryV2")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage NewDeliveryV2(People customer)
        {
            ResDTO res;
            People People = new People();

            using (var context = new AuthContext())
            {
                try
                {
                    string query = "select distinct p.PeopleID, p.PeopleID,STRING_AGG(r.Name,',') RoleName from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where Mobile='" + customer.Mobile + "'  and ur.isActive=1 and p.Active=1 and p.Deleted=0 group by p.PeopleID having STRING_AGG(r.Name,',') like '%Delivery Boy%'";
                    PeopleRole peopleRole = context.Database.SqlQuery<PeopleRole>(query).FirstOrDefault();

                    int? PeopleId = peopleRole != null && peopleRole.RoleName.Split(',').Contains("Delivery Boy") ? peopleRole.peopleId : (int?)null;
                    if (!PeopleId.HasValue)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered Delivery Boy"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    People = context.Peoples.Where(x => x.PeopleID == PeopleId).SingleOrDefault();
                    if (People == null)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered Delivery Boy"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password != customer.Password)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Wrong Password"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password == customer.Password && People.Mobile == customer.Mobile)
                    {


                        if (People.DeviceId == null)
                        {
                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.UpdatedDate = indianTime;
                            People.CurrentAPKversion = customer.CurrentAPKversion;    // tejas to save device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI;// Sudhir to save device info 
                            People.UserDeviceName = customer.UserDeviceName;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            context.Commit();

                        }
                        else if (People.DeviceId.Trim().ToLower() == customer.DeviceId.Trim().ToLower())
                        {
                            if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
                            {
                                People.FcmId = customer.FcmId;
                                People.UpdatedDate = indianTime;
                                People.CurrentAPKversion = customer.CurrentAPKversion;      // tejas to save device info 
                                People.PhoneOSversion = customer.PhoneOSversion;
                                People.IMEI = customer.IMEI;// Sudhir to save device info 
                                People.UserDeviceName = customer.UserDeviceName;
                                //context.Peoples.Attach(People);
                                context.Entry(People).State = EntityState.Modified;
                                context.Commit();
                                #region Device History
                                var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                                try
                                {
                                    PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                    if (Customerhistory != null)
                                    {
                                        phonerecord.PeopleID = Customerhistory.PeopleID;
                                        phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                        phonerecord.Department = Customerhistory.Department;
                                        phonerecord.Mobile = Customerhistory.Mobile;
                                        phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                        phonerecord.IMEI = Customerhistory.IMEI;
                                        phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                        phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                        phonerecord.UpdatedDate = DateTime.Now;
                                        context.PhoneRecordHistoryDB.Add(phonerecord);
                                        int id = context.Commit();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            try
                            {
                                SendLogOutNotification(People); //send Notification Dboy for Sign out
                            }
                            catch (Exception ds)
                            {
                            }

                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.UpdatedDate = indianTime;
                            People.CurrentAPKversion = customer.CurrentAPKversion;        // tejas to save device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI;// Sudhir to save device info 
                            People.UserDeviceName = customer.UserDeviceName;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            #region Device History
                            var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.PeopleID = Customerhistory.PeopleID;
                                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                    phonerecord.Department = Customerhistory.Department;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.IMEI = Customerhistory.IMEI;
                                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                    int id = context.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion
                            context.Commit();
                        }

                        //TEMP obj
                        PeopleTemp PtData = new PeopleTemp();
                        PtData.PeopleID = People.PeopleID;
                        PtData.Skcode = People.Skcode;
                        PtData.CompanyId = People.CompanyId;
                        PtData.WarehouseId = People.WarehouseId;
                        PtData.PeopleFirstName = People.PeopleFirstName;
                        PtData.PeopleLastName = People.PeopleLastName;
                        PtData.Email = People.Email;
                        PtData.DisplayName = People.DisplayName;
                        PtData.Mobile = People.Mobile;
                        PtData.Password = People.Password;
                        PtData.VehicleId = People.VehicleId;
                        PtData.VehicleName = People.VehicleName;
                        PtData.VehicleNumber = People.VehicleNumber;
                        PtData.VehicleCapacity = People.VehicleCapacity;
                        PtData.CreatedDate = People.CreatedDate;
                        PtData.UpdatedDate = People.UpdatedDate;
                        PtData.DeviceId = People.DeviceId;
                        PtData.FcmId = People.FcmId;
                        PtData.ImageUrl = People.ImageUrl;
                        PtData.Role = peopleRole.RoleName;
                        if (PtData != null)
                        {
                            var registeredApk = context.GetAPKUserAndPwd("DeliveryApp");
                            PtData.RegisteredApk = registeredApk;
                        }

                        res = new ResDTO()
                        {
                            P = PtData,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Failed. Something went wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception es)
                {
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = ("Not a Registered Delivery Boy : " + es)
                    };
                };
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }
        #endregion

        #region  Growth module login api
        /// <summary>
        /// DeliveryBoy Login
        /// </summary>
        /// <param name="Mobile"></param>
        /// <param name="Password"></param>
        /// <returns>Peopel Object</returns>
        [Route("LoginGM")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage LoginGM(People customer)
        {
            ResDTO res;
            People People = new People();
            using (var context = new AuthContext())
            {
                People = context.Peoples.Where(x => x.Mobile == customer.Mobile && x.Deleted == false && x.Active == true).FirstOrDefault();

                try
                {
                    if (People == null)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered Delivery Boy"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password != customer.Password)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Wrong Password"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password == customer.Password && People.Mobile == customer.Mobile)
                    {


                        if (People.DeviceId == null)
                        {
                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.UpdatedDate = indianTime;
                            People.CurrentAPKversion = customer.CurrentAPKversion;    // tejas to save device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI;// sudhir to save device info 
                            People.UserDeviceName = customer.UserDeviceName;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            context.Commit();

                        }
                        else if (People.DeviceId.Trim().ToLower() == customer.DeviceId.Trim().ToLower())
                        {
                            if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
                            {
                                People.FcmId = customer.FcmId;
                                People.UpdatedDate = indianTime;
                                People.CurrentAPKversion = customer.CurrentAPKversion;      // tejas to save device info 
                                People.PhoneOSversion = customer.PhoneOSversion;
                                People.IMEI = customer.IMEI;// sudhir to save device info 
                                People.UserDeviceName = customer.UserDeviceName;
                                //context.Peoples.Attach(People);
                                context.Entry(People).State = EntityState.Modified;
                                context.Commit();
                                #region Device History
                                var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                                try
                                {
                                    PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                    if (Customerhistory != null)
                                    {
                                        phonerecord.PeopleID = Customerhistory.PeopleID;
                                        phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                        phonerecord.Department = Customerhistory.Department;
                                        phonerecord.Mobile = Customerhistory.Mobile;
                                        phonerecord.IMEI = Customerhistory.IMEI;
                                        phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                        phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                        phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                        phonerecord.UpdatedDate = DateTime.Now;
                                        context.PhoneRecordHistoryDB.Add(phonerecord);
                                        int id = context.Commit();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            try
                            {
                                SendLogOutNotification(People); //send Notification Dboy for Sign out
                            }
                            catch (Exception ds)
                            {
                            }

                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.UpdatedDate = indianTime;
                            People.CurrentAPKversion = customer.CurrentAPKversion;        // tejas to save device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI;// sudhir to save device info 
                            People.UserDeviceName = customer.UserDeviceName;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            #region Device History
                            var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.PeopleID = Customerhistory.PeopleID;
                                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                    phonerecord.Department = Customerhistory.Department;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.IMEI = Customerhistory.IMEI;//
                                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                    int id = context.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion
                            context.Commit();
                        }

                        //TEMP obj
                        PeopleTemp PtData = new PeopleTemp();
                        PtData.PeopleID = People.PeopleID;
                        PtData.Skcode = People.Skcode;
                        PtData.CompanyId = People.CompanyId;
                        PtData.WarehouseId = People.WarehouseId;
                        PtData.PeopleFirstName = People.PeopleFirstName;
                        PtData.PeopleLastName = People.PeopleLastName;
                        PtData.Email = People.Email;
                        PtData.DisplayName = People.DisplayName;
                        PtData.Mobile = People.Mobile;
                        PtData.Password = People.Password;
                        PtData.VehicleId = People.VehicleId;
                        PtData.VehicleName = People.VehicleName;
                        PtData.VehicleNumber = People.VehicleNumber;
                        PtData.VehicleCapacity = People.VehicleCapacity;
                        PtData.CreatedDate = People.CreatedDate;
                        PtData.UpdatedDate = People.UpdatedDate;
                        PtData.DeviceId = People.DeviceId;
                        PtData.FcmId = People.FcmId;
                        PtData.ImageUrl = People.ImageUrl;

                        res = new ResDTO()
                        {
                            P = PtData,
                            Status = true,
                            Message = "Success."
                        };




                    }
                    else
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Failed. Something went wrong"
                        };



                    }
                }
                catch (Exception es)
                {
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = ("Not a Registered Delivery Boy : " + es)
                    };
                };
            }



            return Request.CreateResponse(HttpStatusCode.OK, res);

        }
        #endregion

        #region DeliveryBoy: Forgot Password
        /// <summary>
        /// Created by 30/01/2019
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        [Route("forgot/v2")]
        [HttpGet]
        public HttpResponseMessage GetForgrtV2(string Mobile)
        {
            ResDTO res;
            People People = new People();
            using (var context = new AuthContext())
            {
                try
                {
                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + Mobile + "' and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    People = context.Database.SqlQuery<People>(query).SingleOrDefault();
                    //People = context.Peoples.Where(x => x.Mobile == Mobile && x.Department == "Delivery Boy" && x.Deleted == false && x.Active == true).SingleOrDefault();
                    if (People != null)
                    {
                        new Sms().sendOtp(People.Mobile, " Hi " + People.DisplayName + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + People.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com","");

                        //TEMP obj
                        PeopleTemp PtData = new PeopleTemp();
                        PtData.PeopleID = People.PeopleID;
                        PtData.Skcode = People.Skcode;
                        PtData.CompanyId = People.CompanyId;
                        PtData.WarehouseId = People.WarehouseId;
                        PtData.PeopleFirstName = People.PeopleFirstName;
                        PtData.PeopleLastName = People.PeopleLastName;
                        PtData.Email = People.Email;
                        PtData.DisplayName = People.DisplayName;
                        PtData.Mobile = People.Mobile;
                        PtData.Password = People.Password;
                        PtData.VehicleId = People.VehicleId;
                        PtData.VehicleName = People.VehicleName;
                        PtData.VehicleNumber = People.VehicleNumber;
                        PtData.VehicleCapacity = People.VehicleCapacity;
                        PtData.CreatedDate = People.CreatedDate;
                        PtData.UpdatedDate = People.UpdatedDate;
                        PtData.DeviceId = People.DeviceId;
                        PtData.FcmId = People.FcmId;
                        PtData.ImageUrl = People.ImageUrl;
                        res = new ResDTO()
                        {
                            P = PtData,
                            Status = true,
                            Message = ("Message send to your registered mobile number")
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = true,
                            Message = ("No record found")
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = ("Something went wrong")
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region DeliveryBoy: Send logout Notification 
        /// <summary>
        /// Created by 18/03/2019
        /// </summary>
        /// <param name="people"></param>
        /// <returns></returns>
        private void SendLogOutNotification(People people)
        {
            try
            {
                //Notification notification = new Notification();
                //notification.title = "true";
                //notification.Message = " चेतावनी ! आपका सत्र समाप्त हो चुका है!";
                //notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
                //notification.priority = "high";
                string Key = "";
                Key = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
                //string id11 = ConfigurationManager.AppSettings["DFcmApiId"];


                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";

                //// Added Bt Harry & Harshita On request chnage 17/06/2019
                //var objNotification = new
                //{
                //    to = people.FcmId,
                //    //notification = new
                //    //{
                //    //    title = notification.title,
                //    //    body = notification.Message,
                //    //},
                //    data = new
                //    {
                //        title = notification.title,
                //        icon = notification.Pic,
                //        priority = notification.priority
                //    }
                //};
                var data = new FCMData
                {
                    title = "true",
                    body = " चेतावनी ! आपका सत्र समाप्त हो चुका है!",
                    icon = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png",
                    typeId = 0,
                    priority = "high",
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "LogOut",
                    // OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                              // OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(people.FcmId, data);
                if (result != null)
                {
                }
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1)
                //                {
                //                    Console.Write(response);
                //                }
                //                else if (response.failure == 1)
                //                {
                //                    Console.Write(response);
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error("Error 2001 :Delivery Notification not send due to" + ex.Message);
            }
        }

        #endregion

        #region  DeliveryBoy: Version 1 Assignment DeliveryEclipseTime start & end time (start close assignment time)
        /// <summary>
        /// DeliveryEclipseTime
        /// </summary>
        /// <returns>DeliveryEclipseTime Object</returns>
        [Route("DeliveryEclipseTime")]
        [HttpPost]
        public HttpResponseMessage PostV2(DeliveryEclipseTime DeliveryEclipseTime)
        {
            DETDTO res;
            DeliveryEclipseTime DETdata = new DeliveryEclipseTime();

            using (var context = new AuthContext())
            {
                DETdata = context.DeliveryEclipseTimeDB.Where(x => x.DeliveryIssuanceId == DeliveryEclipseTime.DeliveryIssuanceId).SingleOrDefault();
                DeliveryIssuance DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryEclipseTime.DeliveryIssuanceId).SingleOrDefault();
                if (DETdata == null && DeliveryEclipseTime.DeliveryIssuanceId > 0 && DeliveryEclipseTime.Start == true && DeliveryIssuance.Status == "Accepted")
                {

                    DeliveryEclipseTime.StartDateTime = indianTime;
                    DeliveryEclipseTime.Start = true;
                    DeliveryEclipseTime.CreatedDate = indianTime;
                    DeliveryEclipseTime.UpdatedDate = indianTime;
                    DeliveryEclipseTime.EndDateTime = null;
                    DeliveryEclipseTime.Deleted = false;
                    context.DeliveryEclipseTimeDB.Add(DeliveryEclipseTime);


                    if (DeliveryIssuance != null)
                    {
                        DeliveryIssuance.Status = "Pending";
                        DeliveryIssuance.UpdatedDate = indianTime;
                        context.Entry(DeliveryIssuance).State = EntityState.Modified;


                        #region  DeliveryHistory
                        OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                        AssginDeli.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                        //AssginDeli.OrderId = delivery.o
                        AssginDeli.Cityid = DeliveryIssuance.Cityid;
                        AssginDeli.city = DeliveryIssuance.city;
                        AssginDeli.DisplayName = DeliveryIssuance.DisplayName;
                        AssginDeli.Status = DeliveryIssuance.Status;
                        AssginDeli.WarehouseId = DeliveryIssuance.WarehouseId;
                        AssginDeli.PeopleID = DeliveryIssuance.PeopleID;
                        AssginDeli.VehicleId = DeliveryIssuance.VehicleId;
                        AssginDeli.VehicleNumber = DeliveryIssuance.VehicleNumber;
                        AssginDeli.RejectReason = DeliveryIssuance.RejectReason;
                        AssginDeli.OrderdispatchIds = DeliveryIssuance.OrderdispatchIds;
                        AssginDeli.OrderIds = DeliveryIssuance.OrderIds;
                        AssginDeli.Acceptance = DeliveryIssuance.Acceptance;
                        AssginDeli.IsActive = DeliveryIssuance.IsActive;
                        AssginDeli.IdealTime = DeliveryIssuance.IdealTime;
                        AssginDeli.TravelDistance = DeliveryIssuance.TravelDistance;
                        AssginDeli.CreatedDate = indianTime;
                        AssginDeli.UpdatedDate = indianTime;
                        AssginDeli.userid = DeliveryIssuance.PeopleID;
                        AssginDeli.UpdatedBy = DeliveryIssuance.DisplayName;
                        context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                        context.Commit();
                        #endregion
                    }
                    res = new DETDTO()
                    {
                        P = DeliveryEclipseTime,
                        Status = true,
                        Message = "start time record created"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else if (DeliveryEclipseTime.End == true && DeliveryEclipseTime.DeliveryIssuanceId > 0 && DETdata != null && DeliveryIssuance.Status == "Pending")
                {
                    string query = " SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT)FROM OrderDeliveryMasters where DeliveryIssuanceId =" + DeliveryEclipseTime.DeliveryIssuanceId + " and Status in ('Shipped', 'Issued','Delivery Canceled Request')";
                    bool IsPending = context.Database.SqlQuery<bool>(query).First();
                    if (!IsPending)
                    {
                        DETdata.End = true;
                        DETdata.EndCoordinates = DeliveryEclipseTime.EndCoordinates;
                        DETdata.UpdatedDate = indianTime;
                        DETdata.EndDateTime = indianTime;
                        //context.DeliveryEclipseTimeDB.Attach(DETdata);
                        context.Entry(DETdata).State = EntityState.Modified;


                        if (DeliveryIssuance != null)
                        {
                            DeliveryIssuance.Status = "Submitted";
                            DeliveryIssuance.UpdatedDate = indianTime;
                            //context.DeliveryIssuanceDb.Attach(DeliveryIssuance);
                            context.Entry(DeliveryIssuance).State = EntityState.Modified;

                            #region  DeliveryHistory
                            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                            AssginDeli.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                            //AssginDeli.OrderId = delivery.o
                            AssginDeli.Cityid = DeliveryIssuance.Cityid;
                            AssginDeli.city = DeliveryIssuance.city;
                            AssginDeli.DisplayName = DeliveryIssuance.DisplayName;
                            AssginDeli.Status = DeliveryIssuance.Status;
                            AssginDeli.WarehouseId = DeliveryIssuance.WarehouseId;
                            AssginDeli.PeopleID = DeliveryIssuance.PeopleID;
                            AssginDeli.VehicleId = DeliveryIssuance.VehicleId;
                            AssginDeli.VehicleNumber = DeliveryIssuance.VehicleNumber;
                            AssginDeli.RejectReason = DeliveryIssuance.RejectReason;
                            AssginDeli.OrderdispatchIds = DeliveryIssuance.OrderdispatchIds;
                            AssginDeli.OrderIds = DeliveryIssuance.OrderIds;
                            AssginDeli.Acceptance = DeliveryIssuance.Acceptance;
                            AssginDeli.IsActive = DeliveryIssuance.IsActive;
                            AssginDeli.IdealTime = DeliveryIssuance.IdealTime;
                            AssginDeli.TravelDistance = DeliveryIssuance.TravelDistance;
                            AssginDeli.CreatedDate = indianTime;
                            AssginDeli.UpdatedDate = indianTime;
                            AssginDeli.userid = DeliveryIssuance.PeopleID;
                            AssginDeli.UpdatedBy = DeliveryIssuance.DisplayName;
                            context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                            context.Commit();
                            #endregion
                        }
                        res = new DETDTO()
                        {
                            P = DETdata,
                            Status = true,
                            Message = "end time record created"
                        };
                    }
                    else
                    {
                        res = new DETDTO()
                        {
                            P = DETdata,
                            Status = false,
                            Message = "orders still in progress"
                        };

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

                else if (DETdata != null && DETdata.DeliveryIssuanceId > 0 && DETdata.Start == true)
                {
                    res = new DETDTO()
                    {
                        P = DETdata,
                        Status = true,
                        Message = "record found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res = new DETDTO()
                    {
                        P = DETdata,

                        Status = false,
                        Message = "record not created due to DeliveryIssuanceId or Start time not true)"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }

        }
        #endregion

        #region Generate Random OTP
        /// <summary>
        /// Created by 29/04/2019 by Sudhir
        /// Create rendom otp
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        #endregion
        #region Generate Delivery boy App OTP
        /// <summary>
        /// Created by 29/04/2019  by Sudhir
        /// OTP Genration code  app site
        /// </summary>
        /// <returns></returns>
        [Route("GenotpForDeliveryBoyApp")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage DeliveryBoyLoginByotp(People customer)
        {
            logger.Info("start Gen OTP: ");
            ResDTO res;
            string error = "";
            People People = new People();
            using (var context = new AuthContext())
            {
                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + customer.Mobile + "' and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                People = context.Database.SqlQuery<People>(query).FirstOrDefault();
                // People = context.Peoples.Where(x => x.Mobile == customer.Mobile && x.Department == "Delivery Boy" && x.Deleted == false && x.Active == true).FirstOrDefault();

                try
                {
                    if (People == null)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered Delivery Boy"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Mobile == customer.Mobile)
                    {
                        if (People.DeviceId == null)
                        {
                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.CurrentAPKversion = customer.CurrentAPKversion;   //tejas for device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI;//sudhir for device info
                            People.UserDeviceName = customer.UserDeviceName;
                            People.UpdatedDate = indianTime;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            context.Commit();
                            #region Device History
                            var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.PeopleID = Customerhistory.PeopleID;
                                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                    phonerecord.Department = Customerhistory.Department;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.IMEI = Customerhistory.IMEI;//
                                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                    int id = context.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion

                        }
                        else if (People.DeviceId.Trim().ToLower() == customer.DeviceId.Trim().ToLower())
                        {
                            if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
                            {
                                People.FcmId = customer.FcmId;
                                People.UpdatedDate = indianTime;
                                People.CurrentAPKversion = customer.CurrentAPKversion;    //tejas for device info 
                                People.IMEI = customer.IMEI;//sudhir for device info
                                People.PhoneOSversion = customer.PhoneOSversion;
                                People.UserDeviceName = customer.UserDeviceName;
                                //context.Peoples.Attach(People);
                                context.Entry(People).State = EntityState.Modified;
                                context.Commit();
                                #region Device History
                                var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                                try
                                {
                                    PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                    if (Customerhistory != null)
                                    {
                                        phonerecord.PeopleID = Customerhistory.PeopleID;
                                        phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                        phonerecord.Department = Customerhistory.Department;
                                        phonerecord.Mobile = Customerhistory.Mobile;
                                        phonerecord.IMEI = Customerhistory.IMEI;//
                                        phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                        phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                        phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                        phonerecord.UpdatedDate = DateTime.Now;
                                        context.PhoneRecordHistoryDB.Add(phonerecord);
                                        int id = context.Commit();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            try
                            {
                                SendLogOutNotification(People); //send Notification Dboy for Sign out
                            }
                            catch (Exception ds)
                            {
                            }
                            People.FcmId = customer.FcmId;
                            People.DeviceId = customer.DeviceId;
                            People.CurrentAPKversion = customer.CurrentAPKversion;    //tejas for device info 
                            People.PhoneOSversion = customer.PhoneOSversion;
                            People.IMEI = customer.IMEI; //sudhir for device info 
                            People.UserDeviceName = customer.UserDeviceName;
                            People.UpdatedDate = indianTime;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            context.Commit();
                            #region Device History
                            var Customerhistory = context.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.PeopleID = Customerhistory.PeopleID;
                                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                    phonerecord.Department = Customerhistory.Department;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.IMEI = Customerhistory.IMEI;
                                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                    int id = context.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion
                        }

                        //TEMP obj

                        try
                        {
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                            // string OtpMessage = " is Your login Code. :). ShopKirana";
                            string OtpMessage = "";//"{#var1#} is Your login Code. {#var2#}. ShopKirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.DeliveryApp, "Login_Code");
                            OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                            OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                            OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                            //string CountryCode = "91";
                            //string Sender = "SHOPKR";
                            //string authkey = Startup.smsauthKey;// "100498AhbWDYbtJT56af33e3";
                            //int route = 4;
                            //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + People.Mobile + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                            ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

                            //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                            //webRequest.Method = "GET";
                            //webRequest.ContentType = "application/json";
                            //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            //webRequest.ContentLength = 0; // added per comment 
                            //webRequest.Credentials = CredentialCache.DefaultCredentials;
                            //webRequest.Accept = "*/*";
                            //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                            bool result = dltSMS==null?false: Common.Helpers.SendSMSHelper.SendSMS(People.Mobile, (" " + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                            if (!result)
                            {
                                logger.Info("OTP Genrated: " + sRandomOTP);
                            }
                            else
                            {
                                logger.Info("OTP Genrated: " + sRandomOTP);
                                PeopleTemp PtData = new PeopleTemp();
                                PtData.PeopleID = People.PeopleID;
                                PtData.OtpNumbers = sRandomOTP;
                                PtData.Skcode = People.Skcode;
                                PtData.CompanyId = People.CompanyId;
                                PtData.WarehouseId = People.WarehouseId;
                                PtData.PeopleFirstName = People.PeopleFirstName;
                                PtData.PeopleLastName = People.PeopleLastName;
                                PtData.Email = People.Email;
                                PtData.DisplayName = People.DisplayName;
                                PtData.Mobile = People.Mobile;
                                PtData.Password = People.Password;
                                PtData.VehicleId = People.VehicleId;
                                PtData.VehicleName = People.VehicleName;
                                PtData.VehicleNumber = People.VehicleNumber;
                                PtData.VehicleCapacity = People.VehicleCapacity;
                                PtData.CreatedDate = People.CreatedDate;
                                PtData.UpdatedDate = People.UpdatedDate;
                                PtData.DeviceId = People.DeviceId;
                                PtData.FcmId = People.FcmId;
                                PtData.ImageUrl = People.ImageUrl;

                                var registeredApk = context.GetAPKUserAndPwd("SalesApp");
                                PtData.RegisteredApk = registeredApk;
                                res = new ResDTO()
                                {
                                    P = PtData,
                                    Status = true,
                                    Message = "Success."
                                };
                                res = new ResDTO()
                                {
                                    P = PtData,
                                    Status = true,
                                    Message = "Success."
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);

                            }

                        }
                        catch (Exception sdf)
                        {

                        }
                    }
                }
                catch (Exception es)
                {
                    error = error + es;
                }
            }
            res = new ResDTO()
            {
                P = null,
                Status = false,
                Message = ("This is something went wrong Delivery Boy : " + error)
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }
        #endregion


    }

    public class ResDTO
    {
        public PeopleTemp P { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class PeopleTemp
    {
        public int PeopleID { get; set; }
        public string Skcode { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleNumber { get; set; }
        public double VehicleCapacity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string DeviceId { get; set; }
        public string FcmId { get; set; }
        public string ImageUrl { get; set; }
        public string Role { get; set; }
        public string OtpNumbers { get; set; }
        [NotMapped]
        public ApkNamePwdResponse RegisteredApk { get; set; }
        public RolesDc Roles { get; set; }

    }

    public class PeopleRole
    {
        public int peopleId { get; set; }
        public string RoleName { get; set; }
    }

    //
    public class DETDTO
    {
        public DeliveryEclipseTime P { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

}
