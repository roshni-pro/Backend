
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.TradeApp;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TradeApp")]
    public class TradeAppController : BaseAuthController
    {
     
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();



        #region  Add customer From Trade App
        /// 11-11-2019 
        [AllowAnonymous]
        [Route("addCustFromTrade")]
        [AcceptVerbs("POST")]
        public TradeCustomerDc addCustFromTrade(TradeCustomerDc tradeCustomerDc)
        {
            customerDetail res;
            logger.Info("start addCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer customer = new Customer();

                    customer.Name = tradeCustomerDc.CustomerName;
                    customer.ShopName = tradeCustomerDc.CustomerName;
                    customer.City = tradeCustomerDc.City;
                    customer.Mobile = tradeCustomerDc.Mobile;
                    customer.lat = Convert.ToDouble(tradeCustomerDc.Lat);
                    customer.lg = Convert.ToDouble(tradeCustomerDc.Lng);
                    customer.ShippingAddress = tradeCustomerDc.Address;
                    customer.RefNo = tradeCustomerDc.GstNo;
                    customer.Password = tradeCustomerDc.Password;
                    customer.Emailid = tradeCustomerDc.Email;
                    customer.AccountNumber = tradeCustomerDc.AccountNumber;
                    customer.BankName = tradeCustomerDc.BankName;
                    customer.ReferralSkCode = tradeCustomerDc.ReferralSkCode;
                    customer.IsReferral = tradeCustomerDc.IsReferral;
                    customer.IfscCode = tradeCustomerDc.IFSC;



                    Customer c = new Customer();
                    Customer customers = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    City city = db.Cities.Where(x => x.CityName == customer.City && x.Deleted == false).FirstOrDefault();

                    Warehouse wh = new Warehouse();

                    if (city != null)
                    {

                        wh = db.Warehouses.Where(x => x.Cityid == city.Cityid && x.Deleted == false).FirstOrDefault();
                        customer.Warehouseid = wh != null ? wh.WarehouseId : (int?)null;
                        customer.WarehouseName = wh != null ? wh.WarehouseName : null;
                    }

                    if (!string.IsNullOrEmpty(customer.RefNo))
                    {
                        var checkgst = db.Customers.Where(x => x.RefNo == customer.RefNo).Count();
                        if (checkgst > 0)
                        {
                            return null;    // changed this because of return type 
                        }
                    }


                    if (customers == null)
                    {
                        string s = string.Format("{0:N6}", customer.lat);
                        string t = string.Format("{0:N6}", customer.lg);
                        // var dd = GetClusters(Convert.ToDouble(s), Convert.ToDouble(t));
                        Cluster dd = null;

                        logger.Info("End  addCustomer: ");
                        c.Skcode = skcode();
                        c.BAGPSCoordinates = customer.BAGPSCoordinates;
                        c.BillingAddress = customer.BillingAddress;
                        c.ShippingAddress = customer.ShippingAddress;
                        //c.ExecutiveId = 183;        // changed this because of null exception 
                        c.FSAAI = customer.FSAAI;
                        c.LandMark = customer.LandMark;
                        c.Mobile = customer.Mobile;
                        c.MobileSecond = customer.MobileSecond;
                        c.MonthlyTurnOver = customer.MonthlyTurnOver;
                        c.Name = customer.Name;
                        c.Password = customer.Password;
                        c.RefNo = customer.RefNo;
                        c.SAGPSCoordinates = customer.SAGPSCoordinates;
                        c.ShopName = customer.ShopName;
                        c.SizeOfShop = customer.SizeOfShop;
                        c.UploadRegistration = customer.UploadRegistration;
                        c.lat = customer.lat;
                        c.lg = customer.lg;
                        c.City = customer.City;        // changed this because of null exception 
                        c.Cityid = city != null ? city.Cityid : (int?)null;   // changed this because of null exception 
                        c.Emailid = customer.Emailid;

                        #region to assign cluster ID and determine if it is in cluster or not.
                        // < summary >
                        // Updated by 28 - 06 - 2019
                        // </ summary > tejas to assign cluster and refine if cx is in region or not
                        if (customer.lat != 0 && customer.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("')");
                            var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                customer.InRegion = false;
                            }
                            else
                            {
                                var agent = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                                if (agent != null && agent.AgentId > 0)
                                    customer.AgentCode = Convert.ToString(agent.AgentId);


                                customer.ClusterId = clusterId;
                                dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                customer.ClusterName = dd.ClusterName;
                                customer.InRegion = true;
                            }
                        }
                        #endregion

                        if (dd != null)
                        {
                            c.Warehouseid = dd.WarehouseId;
                            c.WarehouseName = dd.WarehouseName;
                            c.ClusterId = dd.ClusterId;
                            c.ClusterName = dd.ClusterName;
                            c.Cityid = dd.CityId;

                        }

                        c.CompanyId = wh.CompanyId;
                        c.Shopimage = customer.Shopimage;
                        // c.Active = true;//change on demand of Salesman  
                        c.Active = false;//change on demand of Salesman 
                        c.IsCityVerified = true;
                        c.IsSignup = true;
                        c.CreatedBy = "FromTradeApp";       // changed this because of null exception 
                        c.CreatedDate = indianTime;
                        c.UpdatedDate = indianTime;
                        c.AnniversaryDate = customer.AnniversaryDate;
                        c.DOB = customer.DOB;
                        c.WhatsappNumber = customer.WhatsappNumber;
                        c.LicenseNumber = customer.LicenseNumber;     //tejas 07-06-19

                        db.Customers.Add(c);
                        int isCommit = db.Commit();

                        if (isCommit > 0)
                        {
                            TradeCustomerDc tradeCustomerDcSend = new TradeCustomerDc();
                            tradeCustomerDcSend.SkCode = c.Skcode;
                            tradeCustomerDcSend.Address = c.ShippingAddress;
                            tradeCustomerDcSend.City = c.City;
                            tradeCustomerDcSend.CustomerName = c.Name;
                            tradeCustomerDcSend.City = c.City;
                            tradeCustomerDcSend.Mobile = c.Mobile;
                            tradeCustomerDcSend.Lat = Convert.ToDecimal(c.lat);
                            tradeCustomerDcSend.Lng = Convert.ToDecimal(c.lg);
                            tradeCustomerDcSend.Address = c.ShippingAddress;
                            tradeCustomerDcSend.GstNo = c.RefNo;
                            tradeCustomerDcSend.CreatedDate = c.CreatedDate;
                            return tradeCustomerDcSend;
                        }
                        else
                        {
                            return null;

                        }
                    }
                    else
                    {
                        TradeCustomerDc tradeCustomerDcSend = new TradeCustomerDc();
                        tradeCustomerDcSend.SkCode = customers.Skcode;
                        tradeCustomerDcSend.Address = customers.ShippingAddress;
                        tradeCustomerDcSend.City = customers.City;
                        tradeCustomerDcSend.CustomerName = customers.Name;
                        tradeCustomerDcSend.City = customers.City;
                        tradeCustomerDcSend.Mobile = customers.Mobile;
                        tradeCustomerDcSend.Lat = Convert.ToDecimal(customers.lat);
                        tradeCustomerDcSend.Lng = Convert.ToDecimal(customers.lg);
                        tradeCustomerDcSend.Address = customers.ShippingAddress;
                        tradeCustomerDcSend.GstNo = customers.RefNo;
                        tradeCustomerDcSend.CustomerId = customers.CustomerId;
                        tradeCustomerDc.CreatedDate = customers.CreatedDate;
                        return tradeCustomerDcSend;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        #endregion


        #region SKCode genrate Function.
        public string skcode()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select max(cast(replace(skcode,'SK','') as bigint)) from customers ";
                var intSkCode = db.Database.SqlQuery<long>(query).FirstOrDefault();
                var skcode = "SK" + (intSkCode + 1);
                bool flag = false;
                while (flag == false)
                {
                    var check = db.Customers.Any(s => s.Skcode.Trim().ToLower() == skcode.Trim().ToLower());
                    
                    if (!check)
                    {
                        flag = true;
                        return skcode;
                    }
                    else
                    {
                        intSkCode += 1;
                        skcode = "SK" + intSkCode;
                    }
                }

                return skcode;
            }
        }
        #endregion





        public void updateCxtoTrade(string mobile)
        {
            try
            {
                TradeCustomerDc tradeCustomerDc = new TradeCustomerDc();
                List<KeyValuePair<string, IEnumerable<string>>> header = new List<KeyValuePair<string, IEnumerable<string>>>();
                List<string> val = new List<string>();
                val.Add("true");
                KeyValuePair<string, IEnumerable<string>> pair = new KeyValuePair<string, IEnumerable<string>>("NoEncryption", val);
                header.Add(pair);

                using (GenericRestHttpClient<dynamic, string> memberClient
                                 = new GenericRestHttpClient<dynamic, string>(AppConstants.tradeAPIurl,
                                 "api/TradeCustomer/updateTkCxfromSkApp?mobile=" + mobile))
                {
                    var result = memberClient.PostAsync(mobile);


                }
            }
            catch (Exception ee)
            {
                throw ee;

            }

        }








    }






}