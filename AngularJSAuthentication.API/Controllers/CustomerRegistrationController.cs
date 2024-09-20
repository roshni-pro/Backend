using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/signup")]
    public class CustomerRegistrationController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        public IList<CustomerDetails> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    logger.Info("get customer from registration");
                    var customers = (from a in context.CustomerRegistrations
                                     join e in context.Warehouses on a.WarehouseId equals e.WarehouseId
                                     select new CustomerDetails
                                     {
                                         CustomerName = a.CustomerName,
                                         Email = a.Email,
                                         Mobile = a.Mobile,
                                         Country = a.Country,
                                         State = a.State,
                                         City = a.City,
                                         CreatedDate = a.CreatedDate,
                                         WarehouseName = e.WarehouseName
                                     }).ToList();
                    return customers;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting customer" + ex.Message);
                    return null;
                }
            }
        }

        [Route("")]
        public Customer Get(string mob)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    Customer customers = context.getcustomers(mob);
                    if (customers == null)
                    {
                        return null;
                    }
                    else
                    {
                        return customers;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting customer by mobile" + ex.Message);
                    return null;
                }

            }
        }

        [Route("")]
        [AcceptVerbs("PUT")]
        public Customer Put(Customer cust)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    return context.CustomerUpdate(cust);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        [ResponseType(typeof(CustomerRegistration))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Customer Post(Customer customer)
        {
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();

                Customer newcustomer = new Customer();
                Customer cust = context.Customers.Where(x => x.Deleted == false).Where(x => x.Mobile == customer.Mobile && x.Password == customer.Password).SingleOrDefault();
                if (cust == null)
                {
                    return newcustomer;
                }
                else
                {
                    if (customer.fcmId != null && customer.fcmId.Trim() != "" && customer.fcmId.Trim().ToUpper() != "NULL")
                    {
                        cust.fcmId = customer.fcmId;
                        //context.Customers.Attach(cust);
                        context.Entry(cust).State = EntityState.Modified;
                        context.Commit();

                        //try
                        //{
                        //    var wallet = context.GetWalletbyCustomerid(cust.CustomerId);
                        //    var reward = context.GetRewardbyCustomerid(cust.CustomerId);
                        //}
                        //catch (Exception ex)
                        //{

                        //}

                    }
                    return cust;
                }
            }
        }

        #region Update customer 
        /// <summary>
        /// Updated by 26/12/2018
        /// Authcontext>>CustomerUpdate
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("V2")]
        [AllowAnonymous]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Putc(Customer cust)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    Customer custdata = context.CustomerUpdate(cust);

                    #region for trade customer update   tejas
                    var mobile = new SqlParameter("@mobile", cust.Mobile);
                    context.Database.ExecuteSqlCommand("UpdateTkCustomerLtLng @mobile", mobile);
                    //TradeAppController output = new TradeAppController();
                    //output.updateCxtoTrade(cust.Mobile);
                    #endregion
                    res = new customerDetail()
                    {
                        customers = custdata,
                        Status = true,
                        Message = "Customer updated."
                    };


                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    customerDetail res;
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = true,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Update Customer SingUp
        /// <summary>
        /// Updated by 08/11/2019
        /// Authcontext>>CustomerUpdate
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("V3")]
        [AllowAnonymous]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage PutcustV3(CustomerDC cust)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    Customer custdata = context.CustomerUpdateV3(cust);
                    res = new customerDetail()
                    {
                        customers = custdata,
                        Status = true,
                        Message = "Customer updated."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    customerDetail res;
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = true,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Update customer from sales app
        /// <summary>
        /// Created by 16/01/2019
        /// Authcontext>> CustomerUpdate
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("Putcust")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Putcust(Customer cust)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    Customer customer = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Mobile.Trim().Equals(cust.Mobile.Trim()) && c.Deleted == false).SingleOrDefault();
                    if (customer != null)
                    {
                        customer.UploadRegistration = cust.UploadRegistration;
                        customer.ResidenceAddressProof = cust.ResidenceAddressProof;
                        context.Entry(customer).State = EntityState.Modified;
                        context.Commit();

                        res = new customerDetail()
                        {
                            customers = customer,
                            Status = true,
                            Message = "Customer updated."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    customerDetail res;
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = true,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Customer put fcmid 

        /// <summary>
        /// updated by 28/12/2018
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [ResponseType(typeof(CustomerRegistration))]
        [Route("V2")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public HttpResponseMessage Postc(Customer customer)
        {
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();
                customerDetail res;
                Customer newcustomer = new Customer();

                if (!string.IsNullOrEmpty(customer.deviceId))
                {
                    var deviceIdCheck = context.Peoples.Where(x => x.DeviceId == customer.deviceId).Count();
                    if (deviceIdCheck > 0)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not authorized to login on this device."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                if (!string.IsNullOrEmpty(customer.RefNo))
                {
                    var gstCheck = context.Customers.Where(x => x.RefNo == customer.RefNo).Count();
                    if (gstCheck > 0)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "GST Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                Customer custExist = context.Customers.Where(x => x.Deleted == false).Where(x => x.Mobile == customer.Mobile).SingleOrDefault();
                if (custExist != null)
                {
                    Customer cust = custExist;//context.Customers.Where(x => x.Deleted == false).Where(x => x.Mobile == customer.Mobile).SingleOrDefault();

                    if (cust != null && cust.Password != customer.Password)
                        cust = null;

                    if (cust == null)
                    {
                        res = new customerDetail()
                        {
                            customers = newcustomer,
                            Status = false,
                            Message = "Incorrect password. Try again"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        if (cust.IsKPP && context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId))
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "You are not eligible to access this app. Please contact customer care."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }

                        if (customer.fcmId != null && customer.fcmId.Trim() != "" && customer.fcmId.Trim().ToUpper() != "NULL")
                        {
                            cust.CurrentAPKversion = customer.CurrentAPKversion;              // tejas to save device info 
                            cust.PhoneOSversion = customer.PhoneOSversion;
                            cust.UserDeviceName = customer.UserDeviceName;
                            cust.fcmId = customer.fcmId;
                            cust.imei = customer.imei;//  to save device info 
                                                      //context.Customers.Attach(cust);
                            context.Entry(cust).State = EntityState.Modified;
                            context.Commit();
                            #region Device History
                            var Customerhistory = context.Customers.Where(x => x.Mobile == customer.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.CustomerId = Customerhistory.CustomerId;
                                    phonerecord.Name = Customerhistory.Name;
                                    phonerecord.Skcode = Customerhistory.Skcode;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.IMEI = Customerhistory.imei;
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
                            //try
                            //{
                            //    var wallet = context.GetWalletbyCustomerid(cust.CustomerId);
                            //    var reward = context.GetRewardbyCustomerid(cust.CustomerId);
                            //}
                            //catch (Exception ex)
                            //{
                            //    res = new customerDetail()
                            //    {
                            //        Status = false,
                            //        Message = "Something went wrong in wallet and reward point."
                            //    };
                            //    return Request.CreateResponse(HttpStatusCode.OK, res);
                            //}
                        }

                        var registeredApk = context.GetAPKUserAndPwd("RetailersApp");
                        cust.RegisteredApk = registeredApk;

                        res = new customerDetail()
                        {
                            customers = cust,
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new customerDetail()
                    {
                        customers = newcustomer,
                        Status = false,
                        Message = "Customer not exist."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }

        }
        #endregion

        [ResponseType(typeof(Customer))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Customer Put(Customer cust, string rs)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    //var context = new AuthContext(new AuthContext());
                    return context.Resetpassword(cust);
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }
        [ResponseType(typeof(CustomerRegistration))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Customer Post(string type, Customer customer)
        {
            using (AuthContext context = new AuthContext())
            {
                Customer newcustomer = new Customer();
                try
                {
                    if (type == "ids")
                    {
                        Customer cust = context.Customers.Where(x => x.Deleted == false).Where(x => x.Mobile == customer.Mobile).SingleOrDefault();
                        if (cust == null)
                        {
                            newcustomer = context.CustomerRegistration(customer);
                        }
                        else
                        {
                            if (customer.Mobile == cust.Mobile && customer.Password == cust.Password)
                            {
                                return cust;
                            }
                            else if (customer.Mobile == cust.Mobile && customer.Password != cust.Password)
                            {
                                newcustomer.Mobile = customer.Mobile;
                                newcustomer.Password = null;
                                return newcustomer;
                            }
                        }
                    }
                }
                catch
                {
                    return null;
                }
                return newcustomer;
            }
        }

        #region Update  address  customer 
        /// <summary>
        /// Updated by 13/09/2019
        /// Authcontext>>CustomerUpdate
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("Customeraddupdate")]
        [AllowAnonymous]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Customeraddupdate(customeraddDTO cust)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    Customer custdata = context.Customers.Where(x => x.CustomerId == cust.CustomerId).FirstOrDefault();

                    custdata.lat = cust.lat;
                    custdata.lg = cust.lg;
                    custdata.ShippingAddress = cust.ShippingAddress;
                    custdata.BillingAddress1 = cust.BillingAddress1;
                    custdata.ShippingAddress1 = cust.ShippingAddress1;
                    if (custdata.lat != 0 && custdata.lg != 0 && custdata.Cityid > 0)
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(custdata.lat).Append("', '").Append(custdata.lg).Append("')");
                        var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            custdata.InRegion = false;
                        }
                        else
                        {
                            var agent = context.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                            if (agent != null && agent.AgentId > 0)
                                custdata.AgentCode = Convert.ToString(agent.AgentId);

                            custdata.ClusterId = clusterId;
                            var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                            custdata.ClusterName = dd.ClusterName;
                            custdata.InRegion = true;
                            custdata.Warehouseid = dd.WarehouseId;
                        }
                    }


                    context.Entry(custdata).State = EntityState.Modified;
                    context.Commit();
                    res = new customerDetail()
                    {
                        customers = custdata,
                        Status = true,
                        Message = "Customer updated."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    customerDetail res;
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = true,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion



        ///// <summary>
        ///// Get Customer GST Verify
        ///// </summary>
        ///// <param name></param>
        ///// <param name="GSTNO"></param>
        ///// <returns></returns>
        [Route("GSTVerify")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<dynamic> GetCustomerGSTVerify(string GSTNO)
        {

            string path = ConfigurationManager.AppSettings["GetCustomerGstUrl"];
            path = path.Replace("[[GstNo]]", GSTNO);
            var gst = new CustomerGst();

            using (GenericRestHttpClient<CustomerGst, string> memberClient
                   = new GenericRestHttpClient<CustomerGst, string>(path,
                   string.Empty, null))
            {
                try
                {
                    gst = await memberClient.GetAsync();
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("GST API error: " + ex.ToString());
                    gst.error = true;
                }

                if (gst.error == false)
                {
                    using (AuthContext db = new AuthContext())
                    {
                        CustGSTverifiedRequest GSTdata = new CustGSTverifiedRequest();
                        GSTdata.RequestPath = path;
                        GSTdata.RefNo = gst.taxpayerInfo.gstin;
                        GSTdata.Name = gst.taxpayerInfo.lgnm;
                        GSTdata.ShopName = gst.taxpayerInfo.tradeNam;
                        GSTdata.Active = gst.taxpayerInfo.sts;
                        GSTdata.ShippingAddress = gst.taxpayerInfo.pradr?.addr?.st;
                        GSTdata.State = gst.taxpayerInfo.pradr?.addr?.stcd;
                        GSTdata.City = gst.taxpayerInfo.pradr?.addr?.loc;
                        GSTdata.lat = gst.taxpayerInfo.pradr?.addr?.lt;
                        GSTdata.lg = gst.taxpayerInfo.pradr?.addr?.lg;
                        GSTdata.Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd;
                        GSTdata.RegisterDate = gst.taxpayerInfo.rgdt;
                        GSTdata.LastUpdate = gst.taxpayerInfo.lstupdt;
                        GSTdata.HomeName = gst.taxpayerInfo.pradr?.addr?.bnm;
                        GSTdata.HomeNo = gst.taxpayerInfo.pradr?.addr?.bno;
                        GSTdata.CustomerBusiness = gst.taxpayerInfo.nba != null && gst.taxpayerInfo.nba.Any() ? gst.taxpayerInfo.nba[0] : "";
                        GSTdata.Citycode = gst.taxpayerInfo.ctjCd;
                        GSTdata.PlotNo = gst.taxpayerInfo.pradr?.addr?.flno;
                        GSTdata.Message = gst.error;
                        GSTdata.UpdateDate = DateTime.Now;
                        GSTdata.CreateDate = DateTime.Now;
                        GSTdata.Delete = false;
                        db.CustGSTverifiedRequestDB.Add(GSTdata);
                        db.Commit();
                        if (!string.IsNullOrEmpty(GSTdata.City) && !string.IsNullOrEmpty(GSTdata.State))
                        {
                            Managers.CustomerAddressRequestManager manager = new Managers.CustomerAddressRequestManager();
                            manager.AddGSTCityAndState(GSTdata.City, GSTdata.Zipcode, GSTdata.State, GSTdata.RefNo.Substring(0, 2), db);
                        }
                        

                        CustomerGSTVerify cust = new CustomerGSTVerify()
                        {
                            id = GSTdata.GSTVerifiedRequestId,
                            RefNo = gst.taxpayerInfo.gstin,
                            Name = gst.taxpayerInfo.lgnm,
                            ShopName = gst.taxpayerInfo.tradeNam,
                            ShippingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", GSTdata.HomeNo, GSTdata.HomeName, GSTdata.ShippingAddress, GSTdata.City, GSTdata.State, GSTdata.Zipcode),
                            State = gst.taxpayerInfo.pradr?.addr?.stcd,
                            City = gst.taxpayerInfo.pradr?.addr?.loc,
                            Active = gst.taxpayerInfo.sts,
                            lat = gst.taxpayerInfo.pradr?.addr?.lt,
                            lg = gst.taxpayerInfo.pradr?.addr?.lg,
                            Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd
                        };

                        if (cust.Active == "Active")
                        {
                            custDTOList Custlist = new custDTOList()
                            {
                                custverify = cust,
                                Status = true,
                                Message = "Customer GST Number Is Verify Successfully."
                            };
                            return Custlist;
                        }
                        else
                        {
                            custDTOList Custlist = new custDTOList()
                            {
                                custverify = cust,
                                Status = true,
                                Message = "Customer GST Number Is " + cust.Active
                            };

                            return Custlist;
                        }

                    }
                }

                else
                {
                    custDTOList Custlist = new custDTOList()
                    {
                        custverify = null,
                        Status = false,
                        Message = "Customer GST Number not valid."
                    };
                    return Custlist;
                }
            }

        }

        [Route("GSTUpdateCustomer")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public HttpResponseMessage GSTUpdateCustomer(GSTChangeRequestDC GSTChangeRequest)
        {
            customerGSTDetail res;
            using (AuthContext context = new AuthContext())
            {
                if (GSTChangeRequest != null)
                {
                    var checkPendingstatus = context.GSTChangeRequestDB.Where(x =>  x.Status == "Pending" && x.CustomerId==GSTChangeRequest.CustomerId && x.Delete==false).Count();
                    var checksApproval = context.GSTChangeRequestDB.Where(x => x.Status == "Approval" && x.CustomerId == GSTChangeRequest.CustomerId &&  x.Delete == false).Count();
                    var checkGst = context.GSTChangeRequestDB.Where(x => x.GSTNo==GSTChangeRequest.GSTNo && x.Delete == false && x.Status!="Reject").Count();
                    if (checksApproval == 0)
                    {
                        if (checkPendingstatus == 0)
                        {
                            if (checkGst == 0)
                            {
                                GSTChangeRequest Gstupdate = new GSTChangeRequest();
                                Gstupdate.CustomerId = GSTChangeRequest.CustomerId;
                                Gstupdate.GSTImage = GSTChangeRequest.GSTImage;
                                Gstupdate.GSTNo = GSTChangeRequest.GSTNo;
                                Gstupdate.GSTVerifiedRequestId = GSTChangeRequest.GSTVerifiedRequestId;
                                Gstupdate.Comments = GSTChangeRequest.Comments;
                                Gstupdate.Status = GSTChangeRequest.Status;
                                Gstupdate.CreateDate = DateTime.Now;
                                Gstupdate.Updateby = null;
                                Gstupdate.UpdateDate = DateTime.Now;
                                Gstupdate.PANNo = GSTChangeRequest.PANNo;
                                context.GSTChangeRequestDB.Add(Gstupdate);
                                context.Commit();
                                res = new customerGSTDetail()
                                {
                                    GSTChangeRequest = Gstupdate,
                                    Status = true,
                                    CheckStatus = false,
                                    Message = "Customer GST Details updated."
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                res = new customerGSTDetail()
                                {
                                    GSTChangeRequest = null,
                                    Status = true,
                                    CheckStatus = false,
                                    CheckApproval = true,
                                    Message = "Customer GST Number Is Already Exists"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            res = new customerGSTDetail()
                            {
                                GSTChangeRequest = null,
                                Status = true,
                                CheckStatus = true,
                                Message = "Customer GST Number Is Already in Progress."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }

                    }
                    else
                    {
                        if (checkGst == 0)
                        {
                            GSTChangeRequest Gstupdate = new GSTChangeRequest();
                            Gstupdate.CustomerId = GSTChangeRequest.CustomerId;
                            Gstupdate.GSTImage = GSTChangeRequest.GSTImage;
                            Gstupdate.GSTNo = GSTChangeRequest.GSTNo;
                            Gstupdate.GSTVerifiedRequestId = GSTChangeRequest.GSTVerifiedRequestId;
                            Gstupdate.Comments = GSTChangeRequest.Comments;
                            Gstupdate.Status = GSTChangeRequest.Status;
                            Gstupdate.CreateDate = DateTime.Now;
                            Gstupdate.Updateby = null;
                            Gstupdate.UpdateDate = DateTime.Now;
                            Gstupdate.PANNo = GSTChangeRequest.PANNo;
                            context.GSTChangeRequestDB.Add(Gstupdate);
                            context.Commit();
                            res = new customerGSTDetail()
                            {
                                GSTChangeRequest = Gstupdate,
                                Status = true,
                                CheckStatus = false,
                                Message = "Customer GST Details updated."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new customerGSTDetail()
                            {
                                GSTChangeRequest = null,
                                Status = true,
                                CheckStatus = false,
                                CheckApproval = true,
                                Message = "Customer GST Number Is Already Exists."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }

                }
                else
                {
                    res = new customerGSTDetail()
                    {
                        GSTChangeRequest = null,
                        Status = false,
                        CheckStatus = false,
                        Message = "Somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("GetAllGSTCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public GSTCustomerDC GetAllGSTCustomer(CustomersGSTFilters GSTCustomer)
        {
            GSTCustomerDC searchByCustomerDc = new GSTCustomerDC();
            using (var authContext = new AuthContext())
            {
                List<GSTCustomers> newdata = new List<GSTCustomers>();

                var query = " select  c.Skcode,w.WarehouseName," +
                    "CGCR.GSTVerifiedRequestId,C.City,CGCR.RefNo,GCR.CreateDate,GCR.GSTImage,GCR.Status,GCR.CustomerId,GCR.Updateby  " +
                  " from CustGSTverifiedRequests CGCR join GSTChangeRequests GCR on CGCR.GSTVerifiedRequestId=GCR.GSTVerifiedRequestId " +
                  "  inner join Customers c on c.CustomerId=GCR.CustomerId inner join Warehouses w on w.WarehouseId = c.Warehouseid where  c.Deleted=0 ";

                /*var Searchquery = "select COUNT(c.Skcode) from CustGSTverifiedRequests CGCR join GSTChangeRequests " +
                       " GCR on CGCR.GSTVerifiedRequestId=GCR.GSTVerifiedRequestId inner join Customers c " +
                       "  on c.CustomerId=GCR.CustomerId  where c.Deleted=0 ";*/
                var Searchquery = "select COUNT(c.Skcode) from CustGSTverifiedRequests CGCR join GSTChangeRequests GCR on CGCR.GSTVerifiedRequestId=GCR.GSTVerifiedRequestId " +
                    "inner join Customers c on c.CustomerId = GCR.CustomerId " +
                    "inner join Warehouses w on w.WarehouseId = c.Warehouseid " +
                    "where c.Deleted = 0";



                string searchclause = "";
                if (GSTCustomer.WarehouseId != null)
                {

                    searchclause += " and  c.Warehouseid=" + GSTCustomer.WarehouseId + "";
                }
                if (!string.IsNullOrEmpty(GSTCustomer.Skcode))
                {
                    searchclause += " and  c.Skcode='" + GSTCustomer.Skcode + "'";
                }


                if (!string.IsNullOrEmpty(searchclause))
                {
                    query += "  " + searchclause;

                    Searchquery += " " + searchclause;
                }

                query += " order by CreateDate desc offset " + GSTCustomer.Skip + " rows fetch next " + GSTCustomer.Take + "  rows only";

                searchByCustomerDc.GSTCustomersAll = authContext.Database.SqlQuery<GSTCustomers>(query).ToList();

                //searchByCustomerDc.GSTCustomersAll.ForEach(x => {
                //    if (!string.IsNullOrEmpty(x.GSTImage) && x.GSTImage.Contains("http"))
                //      {
                //        x.GSTImage =string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                //                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                //                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                //                                                      , "/UploadedImages/"+ x.GSTImage);
                //    }
                //});

                searchByCustomerDc.total_count = authContext.Database.SqlQuery<int>(Searchquery).FirstOrDefault();

            }
            return searchByCustomerDc;
        }


        [Route("GetAllGSTCustomerdetails")]
        [HttpGet]
        [AllowAnonymous]
        public GSTCustomersdetails GetAllGSTCustomerdetails(int GSTVerifiedRequestId)
        {
            using (var authContext = new AuthContext())
            {


                var query = "select gs.CustomerId,gs.Status,gs.GSTVerifiedRequestId,cg.CustomerBusiness,gs.GSTNo,gs.GSTImage,cg.Name,cg.ShopName,cg.ShippingAddress,cg.City,cg.Active,cg.HomeName,cg.HomeNo,cg.Citycode,cg.LastUpdate,cg.RegisterDate,cg.State,cg.lat,cg.lg,cg.Zipcode from CustGSTverifiedRequests cg inner join  GSTChangeRequests gs on cg.GSTVerifiedRequestId=gs.GSTVerifiedRequestId where gs.GSTVerifiedRequestId= " + GSTVerifiedRequestId;


                var result = authContext.Database.SqlQuery<GSTCustomersdetails>(query).FirstOrDefault();

                return result;

            }


        }
        [Route("updateGSTCustomerdetails")]
        [HttpPost]
        [AllowAnonymous]
        public dynamic updateGSTCustomerdetails(updatedetails updatedetail)
        {
            using (var authContext = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var DisplayName = authContext.Peoples.Where(x => x.PeopleID == userid).Select(x => x.DisplayName).FirstOrDefault();
                GSTChangeRequest cust = authContext.GSTChangeRequestDB.Where(x => x.GSTVerifiedRequestId == updatedetail.GSTVerifiedRequestId).SingleOrDefault();
                cust.Status = updatedetail.status;
                cust.Updateby = DisplayName;
                cust.UpdateDate = DateTime.Now;
                authContext.Entry(cust).State = EntityState.Modified;
                authContext.Commit();
                var custGstVerifys = authContext.CustGSTverifiedRequestDB.Where(x => x.RefNo == updatedetail.GSTNo).ToList();

                if (updatedetail != null && updatedetail.status == "Approval" && custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                {
                    var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                    var state = authContext.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                    Customer customers = authContext.Customers.Where(x => x.CustomerId == updatedetail.CustomerId).FirstOrDefault();
                    customers.RefNo = updatedetail.GSTNo;
                    customers.PanNo = updatedetail.PANNo;
                    customers.IsPanVerified = true;
                    customers.BillingCity = gstVerify.City;
                    customers.BillingState = state != null ? state.StateName : gstVerify.State;
                    customers.BillingZipCode = gstVerify.Zipcode;
                    customers.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                    customers.UploadGSTPicture = updatedetail.GSTImage;
                    authContext.Entry(customers).State = EntityState.Modified;

                    var IsDistributorExists = authContext.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == updatedetail.CustomerId);
                    if (IsDistributorExists != null )
                    {
                        IsDistributorExists.GSTNo = updatedetail.GSTNo;
                        IsDistributorExists.PANNo = updatedetail.PANNo;
                        authContext.Entry(IsDistributorExists).State = EntityState.Modified;
                    }
                    authContext.Commit();
                    authContext.Database.ExecuteSqlCommand("Exec UpdatePendingOrderGST " + updatedetail.CustomerId);
                }
                return cust;

            }
        }

        #region DTO 

    }
    public class GST
    {
        public string GSTNO { get; set; }
    }
    public class updatedetails
    {
        public int GSTVerifiedRequestId { get; set; }
        public int CustomerId { get; set; }
        public string status { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public string GSTImage { get; set; }

    }
    public class custDTOList
    {
        public CustomerGSTVerify custverify { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class custGstResponse
    {
        public string Skcode { get; set; }
        public string GSTNo { get; set; }
        public bool Status { get; set; }
        public string message { get; set; }
        public CustomerGst GstResponse { get; set; }
    }

    public class GSTCustomerDC
    {
        public int total_count { get; set; }
        public List<GSTCustomers> GSTCustomersAll { get; set; }
    }
    public class GSTCustomers
    {
        public int CustomerId { get; set; }
        public int GSTVerifiedRequestId { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string City { get; set; }
        public string GSTImage { get; set; }
        public string RefNo { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string Updateby { get; set; }
    }


    public class GSTCustomersdetails
    {
        public int CustomerId { get; set; }
        public int GSTVerifiedRequestId { get; set; }
        public string GSTNo { get; set; }
        public string GSTImage { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string ZipCode { get; set; }
        public string Active { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Citycode { get; set; }
        public string RegisterDate { get; set; }
        public string CustomerBusiness { get; set; }
        public string HomeName { get; set; }
        // public string HomeNo { get; set; }
        //  public DateTime LastUpdate { get; set; }
        public string PlotNo { get; set; }
    }
    public class CustomersGSTFilters
    {
        public string Skcode { get; set; }
        public int? WarehouseId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

    }



    public class CustomerDC
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string UploadProfilePichure { get; set; }
        public string Mobile { get; set; }
        public string Emailid { get; set; }
        public string ShippingAddress { get; set; }
        public string ZipCode { get; set; }
        public string Password { get; set; }
        public int Cityid { get; set; }
        public string Skcode { get; set; }
        public string RefNo { get; set; }
        public string UploadGSTPicture { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string deviceId { get; set; }
        public string imei { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string City { get; set; }
        public string ShippingAddress1 { get; set; }
        public string BillingAddress1 { get; set; }
        public string LandMark { get; set; }
        public string LicenseNumber { get; set; }
        public string UploadRegistration { get; set; }
        // trade 
        public string IfscCode { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ReferralSkCode { get; set; }
        public bool? IsReferral { get; set; }
        public string fcmId { get; set; }
        public int GrabbedBy { get; set; }
        public string CustomerVerify { get; set; }
        public string StatusSubType { get; set; }
    }
    public class customeritems
    {
        public People ps { get; set; }
        public Customer cs { get; set; }
        public IEnumerable<Basecats> Basecats { get; set; }
        public List<Categories> Categories { get; set; }
        public IEnumerable<SubCategories> SubCategories { get; set; }
        public IEnumerable<SubSubCategories> SubSubCategories { get; set; }
    }
    public class Basecats
    {
        public int BaseCategoryId { get; set; }

        public string BaseCategoryName { get; set; }
        public string HindiName { get; set; }

        public string LogoUrl { get; set; }

    }
    public class Categories
    {
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
    }
    public class SubCategories
    {
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
    }

    public class SubSubCategories
    {
        public int SubSubCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubSubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
    }
    public class customerDetail
    {
        public Customer customers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string CriticalInfoMissingMsg { get; set; }

    }
    public class CustomerPassword
    {
        public string Password { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string CriticalInfoMissingMsg { get; set; }

    }
    public class customerGSTDetail
    {
        public GSTChangeRequest GSTChangeRequest { get; set; }
        public bool Status { get; set; }
        public bool CheckStatus { get; set; }
        public bool CheckApproval { get; set; }
        public string Message { get; set; }
    }
    public class customeraddDTO
    {
        public int CustomerId { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingAddress1 { get; set; }
        public string BillingAddress1 { get; set; }
    }


    public class CustomerGSTVerify
    {
        public int id { get; set; }
        public string RefNo { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string LandMark { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public int? Cityid { get; set; }
        public string City { get; set; }

        public DateTime CreatedDate { get; set; }
        // public DateTime UpdatedDate { get; set; }      
        public string Zipcode { get; set; }
        public string Active { get; set; }
        public string lat { get; set; }
        public string lg { get; set; }

        //public bool IsSignup { get; set; }

    }
    public class GSTChangeRequestDC
    {
        public int CustomerId { get; set; }
        public string GSTImage { get; set; }
        public string GSTNo { get; set; }
        public int? GSTVerifiedRequestId { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string PANNo { get; set; }

    }

}


#endregion
