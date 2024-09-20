using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.SupplierOnboard;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.supplier;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Suppliers")]
    public class SupplierController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region [new supplier onboarding]
        [AllowAnonymous]
        [Route("AddSupplier/V7")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddSupplierAsync(SupplierOnBoardDC supplierdc)
        {
            try
            {
                SupplierHelper supplierHelper = new SupplierHelper();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }

                if (supplierdc == null)
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Please enter required Fields"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                //---commented by anjali
                //if (string.IsNullOrEmpty(supplierdc.SUPPLIERCODES))
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "Please enter required Fields"
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}
                //--
                //else if ki jagah if karenge
                //--else if likha hua tha neeche
                if (string.IsNullOrEmpty(supplierdc.SupplierName))
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Please enter required Fields"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else if (string.IsNullOrEmpty(supplierdc.PanNumber))
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Please enter required Fields"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else if (string.IsNullOrEmpty(supplierdc.EmailId))
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Please enter required Fields"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else if (string.IsNullOrEmpty(supplierdc.MobileNo))
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Please enter required Fields"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                //else if (string.IsNullOrEmpty(supplierdc.FSSAINO))
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "Please enter required Fields"
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}
                //else if (supplierdc.SellingBrandDCs.Count == 0)
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "Selling Brands not found..please select selling brnads"
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}


                Regex regex = new Regex("([A-Z]){5}([0-9]){4}([A-Z]){1}$");
                if (!regex.IsMatch(supplierdc.PanNumber.ToUpper()))
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Pan Card Not Valid...."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                //--commented by anjali
                //---ye hatani padegi
                //-gst details hataya
                //GSTdetailsDc i = await supplierHelper.GSTVeifyAsync(supplierdc.GSTINNO);
                //if (!i.IsVerify)
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "GSTN Number Not Valid...."
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}

                using (var context = new AuthContext())
                {
                    //--commented by anjali
                    //--ye hatani padegi
                    //bool anysuppliercodeExists = context.Suppliers.Any(x => x.SUPPLIERCODES == supplierdc.SUPPLIERCODES);
                    //if (anysuppliercodeExists)
                    //{
                    //    var response = new
                    //    {
                    //        Status = false,
                    //        Message = "Supplier Code already Exists..." + supplierdc.SUPPLIERCODES,
                    //    };
                    //    return Request.CreateResponse(HttpStatusCode.OK, response);
                    //}
                    //---
                    bool anyPanNumberExists = context.Suppliers.Any(x => x.Pancard == supplierdc.PanNumber);
                    //bool anyPanNumberExistsinsuppliertemp = context.SupplierTempDB.Any(x => x.Pancard == supplierdc.PanNumber);
                    //if (anysuppliercodeExists)
                    //|| anyPanNumberExistsinsuppliertemp
                    if (anyPanNumberExists)
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Pan Number already Exists..." + supplierdc.PanNumber,
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    //--commented by anjali
                    //--hatani hai
                    //bool anyGstnNumberExists = context.Suppliers.Any(x => x.TINNo == supplierdc.GSTINNO);
                    //if (anyGstnNumberExists)
                    //{
                    //    var response = new
                    //    {
                    //        Status = false,
                    //        Message = "GSTN Number already Exists..." + supplierdc.GSTINNO,
                    //    };
                    //    return Request.CreateResponse(HttpStatusCode.OK, response);
                    //}
                    //--
                }

                supplierdc.userid = (supplierdc.userid == 0) ? userid : supplierdc.userid;
                supplierdc.CompanyId = compid;

                bool result = supplierHelper.AddnewSupplier(supplierdc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Supplier Added"
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Supplier not Added"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Supplier not Added " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("SupplierOnboardList/V7")]
        [HttpGet]
        public HttpResponseMessage SupplierOnboardList(string status, string KeyWord, int skip, int take)
        {
            try

            {
                SupplierHelper supplier = new SupplierHelper();
                SupplierOnBoardDCList result = supplier.GetSupplierOnboardList(status, KeyWord, skip, take);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("GetSupplierByid/V7")]
        [HttpGet]
        public HttpResponseMessage GetSupplierOnboardByid(int id, int supplierid)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                SupplierOnBoardDC result = supplier.GetSupplierOnboardByid(id, supplierid);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("GetSupplierBySupplierid/V7")]
        [HttpGet]
        public HttpResponseMessage GetSupplierOnboardBySupplierid(int id)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                SupplierOnBoardDC result = supplier.GetSupplierOnboardBySupplierid(id);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("ActionOnSupplier/V7")]
        [HttpPost]
        public HttpResponseMessage ActionOnSupplierOnboard(SupplierOnBoardActionDC supplierdc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {

                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                supplierdc.Userid = userid;
                SupplierHelper supplier = new SupplierHelper();
                bool result = supplier.ActionOnSupplierOnboard(supplierdc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Supplier " + supplierdc.Status + " Successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Something went wrong try again"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Something went wrong try again"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }


        [AllowAnonymous]
        [Route("EditSupplierTemp/V7")]
        [HttpPost]
        public async Task<HttpResponseMessage> EditSupplierTempAsync(SupplierOnBoardDC supplierdc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                SupplierHelper supplier = new SupplierHelper();
                if (supplierdc == null)
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Data not received"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                supplierdc.userid = userid;
                //GSTdetailsDc i = await supplier.GSTVeifyAsync(supplierdc.GSTINNO);
                //if (!i.IsVerify)
                //{
                //    var response = new
                //    {
                //        Status = false,
                //        Message = "GSTN Number Not Valid...."
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, response);
                //}

                bool result = supplier.EditSupplierTemp(supplierdc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Supplier Details Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Supplier Details not Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Supplier Details not updated"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

        }


        [AllowAnonymous]
        [Route("ActiveDecativeSupplier/V7")]
        [HttpPost]
        public HttpResponseMessage ActiveDecativeSupplier(SupplierOnBoardActionDC supplierdc)
        {
            try
            {
                if (supplierdc == null)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Data not received"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
                if (supplierdc.SupplierId > 0)
                {

                    SupplierHelper supplier = new SupplierHelper();
                    bool result = supplier.ActiveDecativeSupplier(supplierdc);
                    if (result)
                    {
                        var response = new
                        {
                            Status = true,
                            Message = "Supplier Details Updated"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Supplier Details not Updated"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Supplier Details not Updated due to Supplier not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Supplier Details not updated"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

        }



        [AllowAnonymous]
        [Route("SupplierOnboardHisotry/V7")]
        [HttpGet]
        public HttpResponseMessage GetSupplierOnboardHisotry(int supplierID, string SupplierCode)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                List<SupplierOnboardHisotryDC> result = supplier.GetSupplierOnboardHisotry(supplierID, SupplierCode);
                if (result != null && result.Any())
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        /// <summary>
        /// New Depo Onboarding
        /// </summary>
        /// <param name="depodc"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("AddNewDepo/V7")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddNewDepoAsync(DepoOnBoardingDC depodc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                SupplierHelper supplier = new SupplierHelper();
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                if (depodc == null)
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Data not received"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                Regex regex = new Regex("([A-Z]){5}([0-9]){4}([A-Z]){1}$");
                if (!string.IsNullOrEmpty(depodc.PANCardNo))
                {
                    if (!regex.IsMatch(depodc.PANCardNo.ToUpper()))
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Pan Card Not Valid...."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }

                depodc.GSTin = depodc.TINNo;
                GSTdetailsDc i = await supplier.GSTVeifyAsync(depodc.GSTin);
                if (!i.IsVerify)
                {
                    var response = new
                    {
                        Status = false,
                        Message = "GSTN Number Not Valid...."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                using (var context = new AuthContext())
                {
                    bool anydepocodeExists = context.DepoMasters.Any(x => x.DepoCodes == depodc.DepoCodes);
                    if (anydepocodeExists)
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Depo Code already Exists..." + depodc.DepoCodes,
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }

                depodc.CreatedBy = (depodc.CreatedBy == "0" || depodc.CreatedBy == null) ? userid.ToString() : depodc.CreatedBy;

                bool result = supplier.AddNewDepo(depodc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Depo Added"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Depo not Added"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Depo not Added " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }
        [AllowAnonymous]
        [Route("DepoOnboardList/V7")]
        [HttpGet]
        public HttpResponseMessage DepoOnboardList(string status, string KeyWord, int skip, int take)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                DepoOnBoardingDCList result = supplier.GetDepoOnboardList(status, KeyWord, skip, take);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("GetDepoOnboardByid/V7")]
        [HttpGet]
        public HttpResponseMessage GetDepoOnboardByid(int id, int DepId)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                DepoOnBoardingDC result = supplier.GetDepoOnboardByid(id, DepId);
                if (result != null)
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }


        [AllowAnonymous]
        [Route("DepoOnboardListBySupplierID/V7")]
        [HttpGet]
        public HttpResponseMessage GetDepoOnboardListBySupplierID(int SupplierId)
        {
            try
            {
                SupplierHelper supplier = new SupplierHelper();
                List<DepoOnBoardingDC> result = supplier.GetDepoOnboardListBySupplierID(SupplierId);
                if (result != null && result.Any())
                {
                    var response = new
                    {
                        Status = true,
                        data = result,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        data = "No Data found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("ActionOnDepoOnboard/V7")]
        [HttpPost]
        public HttpResponseMessage ActionOnDepoOnboard(DepoOnBoardingActionDC depodc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {

                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                depodc.Userid = depodc.Userid == 0 ? userid : depodc.Userid;
                SupplierHelper supplier = new SupplierHelper();
                bool result = supplier.ActionOnDepoOnboard(depodc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Supplier " + depodc.Status + " Successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Something went wrong try again"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Something went wrong try again " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("EditDepoTemp/V7")]
        [HttpPost]
        public HttpResponseMessage EditDepoTemp(DepoOnBoardingDC depodc)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (depodc == null)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Data not received"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                SupplierHelper supplier = new SupplierHelper();
                bool result = supplier.EditDepotemp(depodc);
                if (result)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Depo Details Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "Depo Details not Updated"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Depo Details not updated " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

        }
        [AllowAnonymous]
        [Route("ActiveDecativeDepo/V7")]
        [HttpPost]
        public HttpResponseMessage ActiveDecativeDepo(DepoOnBoardingActionDC depodc)
        {
            try
            {
                if (depodc == null)
                {
                    var response = new
                    {
                        Status = true,
                        Message = "Data not received"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                if (depodc.DepoId > 0)
                {

                    SupplierHelper supplier = new SupplierHelper();
                    bool result = supplier.ActiveDecativeDepo(depodc);
                    if (result)
                    {
                        var response = new
                        {
                            Status = true,
                            Message = "Depo Updated."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Depo Details not Updated"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
                else
                {
                    var response = new
                    {
                        Status = false,
                        Message = "depo Details not Updated due to depo not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "depo Details not updated " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

        }

        [AllowAnonymous]
        [Route("SupplierGSTVerify")]
        [HttpGet]
        public async Task<GSTdetailsDc> SupplierGSTVerify(string GSTNO)
        {
            SupplierHelper supplierHelper = new SupplierHelper();
            return await supplierHelper.GSTVeifyAsync(GSTNO);
        }


        [Route("DocumentImageUploadMulti")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage DocumentImageUploadMulti()
        {
            string LogoUrl = "";
            HttpResponseMessage result = null;
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {

                    var httpRequest = System.Web.HttpContext.Current.Request;
                    HttpFileCollection uploadFiles = httpRequest.Files;
                    var docfiles = new List<string>();

                    if (httpRequest.Files.Count > 0)
                    {
                        int i;
                        int cnt = 1;
                        var arr1 = httpRequest.Files.AllKeys;
                        for (i = 0; i < uploadFiles.Count; i++)
                        {
                            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/SupplierImage")))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/SupplierImage"));

                            HttpPostedFile postedFile = uploadFiles[i];

                            var name = arr1[i].ToString();
                            string extension = Path.GetExtension(postedFile.FileName);
                            string fileName = name + DateTime.Now.ToString("ddMMyyyyHHmmss") + cnt.ToString() + extension;
                            //string fileName = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + cnt.ToString() + extension;
                            LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/SupplierImage"), fileName);

                            postedFile.SaveAs(LogoUrl);

                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/SupplierImage", LogoUrl);

                            LogoUrl = "/SupplierImage/" + fileName;
                            docfiles.Add(LogoUrl);
                            cnt++;
                        }

                        result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                    }
                    else
                    {
                        result = Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                return result; // Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in DocumentImageUploadMulti Method: " + ex.Message);
                return null;
            }

        }


        [Route("DepoDocumentImageUploadMulti")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage DepoDocumentImageUploadMulti()
        {
            string LogoUrl = "";
            HttpResponseMessage result = null;
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpRequest = System.Web.HttpContext.Current.Request;
                    HttpFileCollection uploadFiles = httpRequest.Files;
                    var docfiles = new List<string>();
                    if (httpRequest.Files.Count > 0)
                    {
                        int i;
                        int cnt = 1;
                        var arr1 = httpRequest.Files.AllKeys;
                        for (i = 0; i < uploadFiles.Count; i++)
                        {
                            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/DepoImage")))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DepoImage"));

                            HttpPostedFile postedFile = uploadFiles[i];

                            var name = arr1[i].ToString();
                            string extension = Path.GetExtension(postedFile.FileName);
                            string fileName = name + DateTime.Now.ToString("ddMMyyyyHHmmss") + cnt.ToString() + extension;
                            //string fileName = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + cnt.ToString() + extension;
                            LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DepoImage"), fileName);

                            postedFile.SaveAs(LogoUrl);

                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/DepoImage", LogoUrl);
                            LogoUrl = "/DepoImage/" + fileName;
                            docfiles.Add(LogoUrl);
                            cnt++;
                        }

                        result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                    }
                    else
                    {
                        result = Request.CreateResponse(HttpStatusCode.BadRequest);
                    }


                    //    if (httpPostedFile != null)
                    //    {

                    //        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/DepoImage")))
                    //            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DepoImage"));

                    //        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DepoImage"), httpPostedFile.FileName);
                    //        //string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    //        string extension = Path.GetExtension(httpPostedFile.FileName);
                    //        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                    //        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DepoImage"), fileName);

                    //        httpPostedFile.SaveAs(LogoUrl);
                    //        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/DepoImage", LogoUrl);

                    //        LogoUrl = "/DepoImage/" + fileName;

                    //    }

                }
                return result;
                //return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in DocumentImageUploadMulti Depo Method: " + ex.Message);
                return null;
            }

        }


        #endregion



        [Authorize]
        [Route("")]
        public List<Supplier> Get()
        {
            List<Supplier> ass = new List<Supplier>();
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);


                ass = db.Suppliers.Where(x => x.Deleted == false).OrderByDescending(s => s.CreatedDate).ToList();
                return ass;

            }

        }
        [Route("GetSupplierList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<ItemMasterSupplierListDC>> GetSupplierList()
        {
            List<ItemMasterSupplierListDC> result = new List<ItemMasterSupplierListDC>();
            using (var myContext = new AuthContext())
            {
                result = myContext.Database.SqlQuery<ItemMasterSupplierListDC>("GetSupplierList").ToList();
                return result;
            }
        }

        [Authorize]
        [Route("")]
        public Supplier Get(string id)
        {
            logger.Info("start Supplier: ");
            Supplier sup = new Supplier();

            try
            {
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int supplierid = Convert.ToInt32(id);
                    int CompanyId = compid;


                    sup = context.Suppliers.Where(x => x.SupplierId == supplierid).SingleOrDefault();

                    return sup;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier " + ex.Message);
                logger.Info("End  Supplier: ");
                return null;
            }
        }

        [ResponseType(typeof(Supplier))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Supplier add(Supplier supplier)
        {
            logger.Info("start supplier: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                // Access claims
                foreach (Claim claim
                    in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                supplier.CompanyId = compid;
                supplier.WarehouseId = Warehouse_id;

                if (supplier == null)
                {
                    throw new ArgumentNullException("supplier");
                }
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    context.AddSupplier(supplier);
                    LadgerHelper ladgerHelper = new LadgerHelper();
                    ladgerHelper.GetOrCreateLadger("Supplier", 12, supplier.SupplierId, userid, context);
                    foreach (var a in supplier.SubBrandid)
                    {
                        var IsAdd = context.SupplierBrandMaps.Where(x => x.BrandId == a.id && x.SupplierId == supplier.SupplierId).Count();
                        if (IsAdd == 0)
                        {
                            var supplierbrandmap = new SupplierBrandMap();
                            supplierbrandmap.BrandId = a.id;
                            supplierbrandmap.SupplierId = supplier.SupplierId;
                            supplierbrandmap.Active = true;
                            supplierbrandmap.Deleted = false;
                            context.SupplierBrandMaps.Add(supplierbrandmap);
                            context.Commit();
                        }
                    }
                }
                logger.Info("End  addsupplier: ");
                return supplier;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addsupplier " + ex.GetBaseException().Message.ToString());
                logger.Info("End  addsupplier: ");
                return null;
            }
        }



        #region get selected Brand to edit Supplier  
        /// <summary>
        /// tejas to show selected brands in edit Supplier
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("GetAllSupplierForUI")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetAllSupplierForUI()
        {
            try
            {
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                using (AuthContext db = new AuthContext())
                {

                    db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                    var GetBrand = (from c in db.Suppliers.Where(a => a.Deleted == false)
                                    join p in db.SupplierBrandMaps.Where(x => x.Active == true && x.Deleted == false)
                                    on c.SupplierId equals p.SupplierId into ps
                                    from p in ps.DefaultIfEmpty()
                                    join f in db.SubCategorys
                                    on p.BrandId equals f.SubCategoryId into pf
                                    from f in pf.DefaultIfEmpty()
                                    select new SupplierBrandsVM
                                    {
                                        SubcategoryName = f.SubcategoryName,
                                        categorynameS = f.CategoryName,
                                        SupplierId = c.SupplierId,
                                        Name = c.Name,
                                        SUPPLIERCODES = c.SUPPLIERCODES,
                                        City = c.City,
                                        Cityid = c.Cityid,
                                        MobileNo = c.MobileNo,
                                        CreatedDate = c.CreatedDate,
                                        EmailId = c.EmailId,
                                        Active = c.Active,
                                        Amount = c.Amount,
                                        Avaiabletime = c.Avaiabletime,
                                        BankPINno = c.BankPINno,
                                        Bank_AC_No = c.Bank_AC_No,
                                        Bank_Ifsc = c.Bank_Ifsc,
                                        Bank_Name = c.Bank_Name,
                                        BillingAddress = c.BillingAddress,
                                        Brand = c.Brand,
                                        businessImageUrl = c.businessImageUrl,
                                        bussinessType = c.bussinessType,
                                        CategoryName = c.CategoryName,
                                        ChequeImageUrl = c.ChequeImageUrl,
                                        CityPincode = c.CityPincode,
                                        Comments = c.Comments,
                                        CompanyId = c.CompanyId,
                                        ContactImage = c.ContactImage,
                                        ContactPerson = c.ContactPerson,
                                        Deleted = c.Deleted,
                                        DepoId = c.DepoId,
                                        DepoName = c.DepoName,
                                        Description = c.Description,
                                        DeviceId = c.DeviceId,
                                        EstablishmentYear = c.EstablishmentYear,
                                        fcmId = c.fcmId,
                                        FSSAI = c.FSSAI,
                                        GstInNumber = c.GstInNumber,
                                        HeadOffice = c.HeadOffice,
                                        ImageUrl = c.ImageUrl,
                                        IsCityVerified = c.IsCityVerified,
                                        IsVerified = c.IsVerified,
                                        ManageAddress = c.ManageAddress,
                                        OfficePhone = c.OfficePhone,
                                        OpeningHours = c.OpeningHours,
                                        OwnerName = c.OwnerName,
                                        Pancard = c.Pancard,
                                        Password = c.Password,
                                        PaymentTerms = c.PaymentTerms,
                                        PeopleID = c.PeopleID,
                                        PhoneNumber = c.PhoneNumber,
                                        Pincode = c.Pincode,
                                        rating = c.rating,
                                        SalesManager = c.SalesManager,
                                        ShippingAddress = c.ShippingAddress,
                                        ShopName = c.ShopName,
                                        StartedBusiness = c.StartedBusiness,
                                        Stateid = c.Stateid,
                                        StateName = c.StateName,
                                        SupplierAddress = c.SupplierAddress,
                                        SupplierCaegoryId = c.SupplierCaegoryId,
                                        TINNo = c.TINNo,
                                        UpdatedDate = c.UpdatedDate,
                                        WarehouseId = c.WarehouseId,
                                        WarehouseName = c.WarehouseName,
                                        WebUrl = c.WebUrl,
                                        GstImage = baseUrl + c.GstImage,
                                        FSSAIImage = baseUrl + c.FSSAIImage,
                                        PanCardImage = baseUrl + c.PanCardImage,
                                        CancelCheque = baseUrl + c.CancelCheque,
                                        CibilScore = c.CibilScore,
                                        IsStopAdvancePr = c.IsStopAdvancePr,
                                        IsIRNInvoiceRequired = c.IsIRNInvoiceRequired
                                    });


                    var list = GetBrand.ToList();

                    var groupList = list.GroupBy(x => new
                    {
                        x.SupplierId,
                        x.Name,
                        x.SUPPLIERCODES,
                        x.City,
                        x.Cityid,
                        x.MobileNo,
                        x.CreatedDate,
                        x.EmailId,
                        x.Active,
                        x.Avaiabletime,
                        x.BankPINno,
                        x.Bank_AC_No,
                        x.Bank_Ifsc,
                        x.Bank_Name,
                        x.BillingAddress,
                        x.Brand,
                        x.businessImageUrl,
                        x.bussinessType,
                        x.CategoryName,
                        x.ChequeImageUrl,
                        x.CityPincode,
                        x.Comments,
                        x.CompanyId,
                        x.ContactImage,
                        x.ContactPerson,
                        x.Deleted,
                        x.DepoId,
                        x.DepoName,
                        x.Description,
                        x.DeviceId,
                        x.EstablishmentYear,
                        x.fcmId,
                        x.FSSAI,
                        x.GstInNumber,
                        x.HeadOffice,
                        x.ImageUrl,
                        x.IsCityVerified,
                        x.IsVerified,
                        x.ManageAddress,
                        x.OfficePhone,
                        x.OpeningHours,
                        x.OwnerName,
                        x.Pancard,
                        x.Password,
                        x.PaymentTerms,
                        x.PeopleID,
                        x.PhoneNumber,
                        x.Pincode,
                        x.rating,
                        x.SalesManager,
                        x.ShippingAddress,
                        x.ShopName,
                        x.StartedBusiness,
                        x.Stateid,
                        x.StateName,
                        x.SupplierAddress,
                        x.SupplierCaegoryId,
                        x.TINNo,
                        x.UpdatedDate,
                        x.WarehouseId,
                        x.WarehouseName,
                        x.WebUrl,
                        x.CibilScore,
                        x.IsStopAdvancePr


                    }).ToList();

                    List<SupplierBrandsVM> lst = new List<SupplierBrandsVM>();
                    foreach (var group in groupList)
                    {
                        var itm = group.First();
                        string brandName = "";
                        foreach (var item in group)
                        {

                            if (item.SubcategoryName != null)
                            {
                                if (string.IsNullOrEmpty(brandName))
                                {
                                    brandName = item.SubcategoryName;
                                }
                                else
                                {
                                    brandName = brandName + "  " + ", " + " " + item.SubcategoryName;
                                }

                            }

                        }
                        itm.SubcategoryName = brandName;
                        lst.Add(itm);
                    }
                    return lst.OrderByDescending(x => x.CreatedDate);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Brand " + ex.ToString());
                logger.Info("End  get brand: ");
                return 0;
            }
        }
        #endregion


        //#region get api for main suppplier angular 7 Page with search filters and pagination  
        ///// tejas
        //[Route("GetAllSupplierForUIA7")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<SupplierPaginationData> GetAllSupplierForUIA7(SupplierFilterDc supplierFilterDc)
        //{
        //    try
        //    {
        //        using (AuthContext db = new AuthContext())
        //        {
        //            SupplierPaginationData supplierPaginationData = new SupplierPaginationData();

        //            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

        //            var GetBrand = (from c in db.Suppliers.Where(x => x.Deleted == false)
        //                            join p in db.SupplierBrandMaps.Where(x => x.Active == true && x.Deleted == false)
        //                            on c.SupplierId equals p.SupplierId into ps
        //                            from p in ps.DefaultIfEmpty()
        //                            join f in db.SubCategorys
        //                            on p.BrandId equals f.SubCategoryId into pf
        //                            from f in pf.DefaultIfEmpty()
        //                            where (supplierFilterDc.cityid == null || (c.Cityid == supplierFilterDc.cityid))
        //                            && (supplierFilterDc.SubCaegoryId == null || (p.BrandId == supplierFilterDc.SubCaegoryId))
        //                            select new SupplierBrandsVM
        //                            {
        //                                SubcategoryName = f.SubcategoryName,
        //                                categorynameS = f.CategoryName,
        //                                SupplierId = c.SupplierId,
        //                                Name = c.Name,
        //                                SUPPLIERCODES = c.SUPPLIERCODES,
        //                                City = c.City,
        //                                Cityid = c.Cityid,
        //                                MobileNo = c.MobileNo,
        //                                CreatedDate = c.CreatedDate,
        //                                EmailId = c.EmailId,
        //                                Active = c.Active,
        //                                Amount = c.Amount,
        //                                Avaiabletime = c.Avaiabletime,
        //                                BankPINno = c.BankPINno,
        //                                Bank_AC_No = c.Bank_AC_No,
        //                                Bank_Ifsc = c.Bank_Ifsc,
        //                                Bank_Name = c.Bank_Name,
        //                                BillingAddress = c.BillingAddress,
        //                                Brand = c.Brand,
        //                                businessImageUrl = c.businessImageUrl,
        //                                bussinessType = c.bussinessType,
        //                                CategoryName = c.CategoryName,
        //                                ChequeImageUrl = c.ChequeImageUrl,
        //                                CityPincode = c.CityPincode,
        //                                Comments = c.Comments,
        //                                CompanyId = c.CompanyId,
        //                                ContactImage = c.ContactImage,
        //                                ContactPerson = c.ContactPerson,
        //                                Deleted = c.Deleted,
        //                                DepoId = c.DepoId,
        //                                DepoName = c.DepoName,
        //                                Description = c.Description,
        //                                DeviceId = c.DeviceId,
        //                                EstablishmentYear = c.EstablishmentYear,
        //                                fcmId = c.fcmId,
        //                                FSSAI = c.FSSAI,
        //                                GstInNumber = c.GstInNumber,
        //                                HeadOffice = c.HeadOffice,
        //                                ImageUrl = c.ImageUrl,
        //                                IsCityVerified = c.IsCityVerified,
        //                                IsVerified = c.IsVerified,
        //                                ManageAddress = c.ManageAddress,
        //                                OfficePhone = c.OfficePhone,
        //                                OpeningHours = c.OpeningHours,
        //                                OwnerName = c.OwnerName,
        //                                Pancard = c.Pancard,
        //                                Password = c.Password,
        //                                PaymentTerms = c.PaymentTerms,
        //                                PeopleID = c.PeopleID,
        //                                PhoneNumber = c.PhoneNumber,
        //                                Pincode = c.Pincode,
        //                                rating = c.rating,
        //                                SalesManager = c.SalesManager,
        //                                ShippingAddress = c.ShippingAddress,
        //                                ShopName = c.ShopName,
        //                                StartedBusiness = c.StartedBusiness,
        //                                Stateid = c.Stateid,
        //                                StateName = c.StateName,
        //                                SupplierAddress = c.SupplierAddress,
        //                                SupplierCaegoryId = c.SupplierCaegoryId,
        //                                TINNo = c.TINNo,
        //                                UpdatedDate = c.UpdatedDate,
        //                                WarehouseId = c.WarehouseId,
        //                                WarehouseName = c.WarehouseName,
        //                                WebUrl = c.WebUrl,
        //                            });
        //            //var list = GetBrand.ToList();
        //            supplierPaginationData.total = GetBrand.Count();
        //            var list = GetBrand.OrderByDescending(x => x.CreatedDate).Skip(supplierFilterDc.Skip).Take(supplierFilterDc.Take).ToList();
        //            // return supplierPaginationData;
        //            var groupList = list.GroupBy(x => new
        //            {
        //                x.SupplierId,
        //                x.Name,
        //                x.SUPPLIERCODES,
        //                x.City,
        //                x.Cityid,
        //                x.MobileNo,
        //                x.CreatedDate,
        //                x.EmailId,
        //                x.Active,
        //                x.Avaiabletime,
        //                x.BankPINno,
        //                x.Bank_AC_No,
        //                x.Bank_Ifsc,
        //                x.Bank_Name,
        //                x.BillingAddress,
        //                x.Brand,
        //                x.businessImageUrl,
        //                x.bussinessType,
        //                x.CategoryName,
        //                x.ChequeImageUrl,
        //                x.CityPincode,
        //                x.Comments,
        //                x.CompanyId,
        //                x.ContactImage,
        //                x.ContactPerson,
        //                x.Deleted,
        //                x.DepoId,
        //                x.DepoName,
        //                x.Description,
        //                x.DeviceId,
        //                x.EstablishmentYear,
        //                x.fcmId,
        //                x.FSSAI,
        //                x.GstInNumber,
        //                x.HeadOffice,
        //                x.ImageUrl,
        //                x.IsCityVerified,
        //                x.IsVerified,
        //                x.ManageAddress,
        //                x.OfficePhone,
        //                x.OpeningHours,
        //                x.OwnerName,
        //                x.Pancard,
        //                x.Password,
        //                x.PaymentTerms,
        //                x.PeopleID,
        //                x.PhoneNumber,
        //                x.Pincode,
        //                x.rating,
        //                x.SalesManager,
        //                x.ShippingAddress,
        //                x.ShopName,
        //                x.StartedBusiness,
        //                x.Stateid,
        //                x.StateName,
        //                x.SupplierAddress,
        //                x.SupplierCaegoryId,
        //                x.TINNo,
        //                x.UpdatedDate,
        //                x.WarehouseId,
        //                x.WarehouseName,
        //                x.WebUrl
        //            }).ToList();
        //            List<SupplierBrandsVM> lst = new List<SupplierBrandsVM>();
        //            foreach (var group in groupList)
        //            {
        //                var itm = group.First();
        //                string brandName = "";
        //                foreach (var item in group)
        //                {

        //                    if (item.SubcategoryName != null)
        //                    {
        //                        if (string.IsNullOrEmpty(brandName))
        //                        {
        //                            brandName = item.SubcategoryName;
        //                        }
        //                        else
        //                        {
        //                            brandName = brandName + "  " + ", " + " " + item.SubcategoryName;
        //                        }

        //                    }

        //                }
        //                itm.SubcategoryName = brandName;
        //                lst.Add(itm);
        //            }
        //            supplierPaginationData.SupplierListDc = lst;
        //            return supplierPaginationData;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Brand " + ex.ToString());
        //        logger.Info("End  get brand: ");
        //        return null;
        //    }
        //}
        //#endregion



        //#region get api for main suppplier angular 7 Page with search filters and pagination  
        ///// tejas
        //[Route("GetAllSupplierForUIA7V2")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<SupplierPaginationData> GetAllSupplierForUIA7V2(SupplierFilterDc supplierFilterDc)
        //{
        //    try
        //    {
        //        using (AuthContext db = new AuthContext())
        //        {
        //            SupplierPaginationData supplierPaginationData = new SupplierPaginationData();

        //            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

        //            var GetBrand = (from c in db.Suppliers.Where(x => x.Deleted == false)
        //                            where (supplierFilterDc.cityid == null || (c.Cityid == supplierFilterDc.cityid))
        //                            select new SupplierBrandsVM
        //                            {
        //                                SupplierId = c.SupplierId,
        //                                Name = c.Name,
        //                                SUPPLIERCODES = c.SUPPLIERCODES,
        //                                City = c.City,
        //                                Cityid = c.Cityid,
        //                                MobileNo = c.MobileNo,
        //                                CreatedDate = c.CreatedDate,
        //                                EmailId = c.EmailId,
        //                                Active = c.Active,
        //                                Amount = c.Amount,
        //                                Avaiabletime = c.Avaiabletime,
        //                                BankPINno = c.BankPINno,
        //                                Bank_AC_No = c.Bank_AC_No,
        //                                Bank_Ifsc = c.Bank_Ifsc,
        //                                Bank_Name = c.Bank_Name,
        //                                BillingAddress = c.BillingAddress,
        //                                Brand = c.Brand,
        //                                businessImageUrl = c.businessImageUrl,
        //                                bussinessType = c.bussinessType,
        //                                CategoryName = c.CategoryName,
        //                                ChequeImageUrl = c.ChequeImageUrl,
        //                                CityPincode = c.CityPincode,
        //                                Comments = c.Comments,
        //                                CompanyId = c.CompanyId,
        //                                ContactImage = c.ContactImage,
        //                                ContactPerson = c.ContactPerson,
        //                                Deleted = c.Deleted,
        //                                DepoId = c.DepoId,
        //                                DepoName = c.DepoName,
        //                                Description = c.Description,
        //                                DeviceId = c.DeviceId,
        //                                EstablishmentYear = c.EstablishmentYear,
        //                                fcmId = c.fcmId,
        //                                FSSAI = c.FSSAI,
        //                                GstInNumber = c.GstInNumber,
        //                                HeadOffice = c.HeadOffice,
        //                                ImageUrl = c.ImageUrl,
        //                                IsCityVerified = c.IsCityVerified,
        //                                IsVerified = c.IsVerified,
        //                                ManageAddress = c.ManageAddress,
        //                                OfficePhone = c.OfficePhone,
        //                                OpeningHours = c.OpeningHours,
        //                                OwnerName = c.OwnerName,
        //                                Pancard = c.Pancard,
        //                                Password = c.Password,
        //                                PaymentTerms = c.PaymentTerms,
        //                                PeopleID = c.PeopleID,
        //                                PhoneNumber = c.PhoneNumber,
        //                                Pincode = c.Pincode,
        //                                rating = c.rating,
        //                                SalesManager = c.SalesManager,
        //                                ShippingAddress = c.ShippingAddress,
        //                                ShopName = c.ShopName,
        //                                StartedBusiness = c.StartedBusiness,
        //                                Stateid = c.Stateid,
        //                                StateName = c.StateName,
        //                                SupplierAddress = c.SupplierAddress,
        //                                SupplierCaegoryId = c.SupplierCaegoryId,
        //                                TINNo = c.TINNo,
        //                                UpdatedDate = c.UpdatedDate,
        //                                WarehouseId = c.WarehouseId,
        //                                WarehouseName = c.WarehouseName,
        //                                WebUrl = c.WebUrl,
        //                                SubcategoryName = null,


        //                            }); 


        //            supplierPaginationData.total = GetBrand.Count();

        //            var list = GetBrand.OrderByDescending(x => x.CreatedDate).Skip(supplierFilterDc.Skip).Take(supplierFilterDc.Take).ToList();

        //            List<SupplierBrandsVM> brands = new List<SupplierBrandsVM>();
        //            foreach (var j in list)

        //            {

        //                 var Bbrands = (from c in db.SupplierBrandMaps.Where(x => x.Deleted == false && x.SupplierId == j.SupplierId)

        //                          join d in db.SubCategorys
        //                          on c.BrandId equals d.SubCategoryId
        //                          select new SupplierBrandsVM
        //                          {
        //                              SubcategoryName = d.SubcategoryName,
        //                              SupplierId = j.SupplierId,

        //                          }).FirstOrDefault();

        //                brands.Add(Bbrands);


        //            }
        //            foreach (var rere in brands)
        //            {


        //            }


        //            supplierPaginationData.SupplierListDc = GetBrand.OrderByDescending(x => x.CreatedDate).Skip(supplierFilterDc.Skip).Take(supplierFilterDc.Take).ToList();
        //            return supplierPaginationData;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Brand " + ex.ToString());
        //        logger.Info("End  get brand: ");
        //        return null;
        //    }
        //}
        //#endregion



        #region get selected Brand to edit Supplier                                                                                                        
        /// <summary>                                                                                                                                      
        /// tejas to show selected brands in edit Supplier                                                                                                 
        /// </summary>                                                                                                                                     
        /// <param name="SupplierId"></param>                                                                                                              
        /// <returns></returns>                                                                                                                            
        [Route("GetBrandEditSupplier")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetBrandEditSupplier(int SupplierId)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {

                    var GetBrand = (from c in db.SubCategorys.DistinctBy(x => x.SubcategoryName).Where(x => x.Deleted == false).OrderBy(x => x.SubcategoryName)
                                    join p in db.SupplierBrandMaps.Where(x => x.SupplierId == SupplierId && x.Active == true)
                                    on c.SubCategoryId equals p.BrandId into ps
                                    from p in ps.DefaultIfEmpty()
                                    select new
                                    {
                                        id = c.SubCategoryId,
                                        name = c.SubcategoryName,
                                        Selected = p == null ? false : true
                                    }).ToList();
                    return GetBrand;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Brand " + ex.ToString());
                logger.Info("End  get brand: ");
                return 0;
            }
        }
        #endregion

        #region get Brand for Supplier with category name                                                                                                       
        /// <summary>                                                                                                                                      
        /// tejas to show selected brands in edit Supplier                                                                                                 
        /// </summary>                                                                                                                                     
        /// <param name="SupplierId"></param>                                                                                                              
        /// <returns></returns>                                                                                                                            
        [Route("getBrandwithcategoryname")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic getBrandwithcategoryname()
        {

            try
            {
                using (AuthContext db = new AuthContext())
                {

                    var BrandWithCategory = from c in db.SubCategorys.DistinctBy(x => x.SubcategoryName).Where(x => x.Deleted == false)
                                            select new SubCategories
                                            {
                                                SubCategoryId = c.SubCategoryId,
                                                SubcategoryName = c.SubcategoryName

                                            };

                    var FinalBrandWithCategory = BrandWithCategory.OrderBy(a => a.SubcategoryName).ToList();

                    //string sqlquery = "SELECT [PurchaseId] FROM [SupplierChats]"
                    //             + "where PurchaseId is not null and SupplierId = " + SupplierId +
                    //              "GROUP BY [PurchaseId] ORDER BY MAX([ChatId]) DESC";



                    //List<int?> SupplierChat = db.Database.SqlQuery<int?>(sqlquery).ToList();




                    return FinalBrandWithCategory;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Brand " + ex.ToString());
                logger.Info("End  get brand: ");
                return 0;
            }
        }
        #endregion


        /// <summary>
        /// GetSupplierByWarehouseID
        /// 29/03/2019
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        [ResponseType(typeof(Supplier))]
        [Route("v1")]
        [AcceptVerbs("GET")]
        public IEnumerable<Supplier> Get1(int wareHouseId)
        {
            using (var db = new AuthContext())
            {
                //List<Supplier> ass=  db.Database.SqlQuery<Supplier>("Select a.* from Suppliers a where exists (Select b.warehouseid from itemmasters b  where  a.supplierid=b.supplierid and b.warehouseid=" + wareHouseId +")").ToList();
                List<Supplier> ass = db.Suppliers.Where(x => x.WarehouseId == wareHouseId && x.Deleted == false).ToList();
                return ass;
            }

        }

        [Route("POSupplier")]
        [AcceptVerbs("GET")]
        public IEnumerable<Supplier> POSupplier(int wareHouseId)
        {
            using (var db = new AuthContext())
            {
                List<Supplier> suppliers = db.Database.SqlQuery<Supplier>("Select a.* from Suppliers a where exists ( Select b.warehouseid from PurchaseOrderMasters b  where  a.supplierid=b.supplierid and b.warehouseid=" + wareHouseId + ")").ToList();
                return suppliers;
            }
        }


        [ResponseType(typeof(Supplier))]
        [Route("put")]
        [AcceptVerbs("PUT")]
        public Supplier Put(Supplier supplier)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }

                supplier.userid = userid;
                supplier.CompanyId = compid;
                supplier.WarehouseId = Warehouse_id;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {

                    if (supplier.SubBrandid.Count == 0)

                    {
                        var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplier.SupplierId).ToList();
                        if (Remove.Count != 0)
                        {
                            foreach (var b in Remove)

                            {
                                b.Active = false;
                                b.Deleted = true;
                                context.Entry(b).State = System.Data.Entity.EntityState.Modified;

                            }
                        }
                    }
                    else
                    {
                        var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplier.SupplierId).ToList();
                        if (Remove.Count != 0)
                        {
                            foreach (var b in Remove)

                            {
                                b.Active = false;
                                b.Deleted = true;
                                context.Entry(b).State = System.Data.Entity.EntityState.Modified;

                            }
                        }
                        foreach (var a in supplier.SubBrandid)
                        {
                            //var IsAdd = context.SupplierBrandMaps.Where(x => x.BrandId == a.id && x.SupplierId == supplier.SupplierId && x.Deleted == false && x.Active == true).FirstOrDefault();
                            //if (IsAdd == null)
                            //{
                            var supplierbrandmap = new SupplierBrandMap();
                            supplierbrandmap.BrandId = a.id;
                            supplierbrandmap.SupplierId = supplier.SupplierId;
                            supplierbrandmap.Active = true;
                            supplierbrandmap.Deleted = false;
                            context.SupplierBrandMaps.Add(supplierbrandmap);
                            context.Commit();

                            //}
                        }
                    }
                    return context.PutSupplier(supplier);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }
        #region Activate and Deactivated for code supplier
        /// <summary>
        /// supplier Activate and Deactivated  code 
        /// CreateDate 01/03/2019 
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        [ResponseType(typeof(Supplier))]
        [Route("Activate")]
        [AcceptVerbs("PUT")]
        public Supplier Putsupp(Supplier supplier)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (var db = new AuthContext())
                {
                    Supplier act = db.Suppliers.Where(x => x.SupplierId == supplier.SupplierId && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.Active = supplier.Active;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    return supplier;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }
        #endregion



        [Route("GetWarehouseCity")]
        [HttpGet]
        public IEnumerable<Supplier> GetWarehousecity(int cityid)
        {
            logger.Info("start Warehouse: ");
            List<Supplier> ass = new List<Supplier>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Supplier_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Supplier_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Supplier_id);
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    List<Supplier> whdata = db.Suppliers.Where(x => x.Cityid == cityid && x.Deleted == false).ToList();
                    //List<Supplier> whdata = db.Suppliers.Where(x => x.Cityid == Cityid && x.Deleted == false).ToList();

                    logger.Info("End  Supplier: ");
                    return whdata;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Warehouse " + ex.Message);
                logger.Info("End  Warehouse: ");
                return null;
            }
        }


        [Route("GetActiveAgentsForCityState")]
        [HttpGet]
        public dynamic AgentnDboyDeviceState()
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                using (var db = new AuthContext())
                {
                    var data = db.States.Where(x => x.active == true).Select(x => new { x.StateName, x.Stateid }).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        //[Route("GetActiveAgentsForBrand")]
        //[HttpGet]
        //public dynamic AgentnDboyDeviceBrand()
        //{

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0, Warehouse_id = 0;
        //        // Access claims
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }
        //        int CompanyId = compid;
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //        using (var db = new AuthContext())
        //        {
        //            var data = db.Suppliers.Where(x => x.Active == true).Select(x => new { x.Brand, x.BrandId }).ToList();
        //            return data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


        //[Route("GetWarehousebrand")]
        //[HttpGet]
        //public IEnumerable<SupplierBrandMap> GetWarehousebrand(int brandid)
        //{
        //    logger.Info("start Warehouse: ");
        //    List<SupplierBrandMap> ass = new List<SupplierBrandMap>();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Supplier_id = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Supplier_id = int.Parse(claim.Value);
        //            }
        //        }
        //        using (var db = new AuthContext())
        //        {
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Supplier_id);
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            List<SubCategory> whdata = db.SubCategorys.Where(x => x.IsActive == true && x.Deleted == false).Select new { SubCategoryId, SubcategoryName }.ToList();
        //            //List<Supplier> whdata = db.Suppliers.Where(x => x.Cityid == Cityid && x.Deleted == false).ToList();

        //            logger.Info("End  Supplier: ");
        //            return whdata;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Warehouse " + ex.Message);
        //        logger.Info("End  Warehouse: ");
        //        return null;
        //    }
        //}

        [Route("")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Get(DateTime start, int BrandId)
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<SupplierBrandMap> data = new List<SupplierBrandMap>();
                    List<int> CustomerId = new List<int>();
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    data = context.SupplierBrandMaps.Where(x => x.Deleted == false && x.BrandId == BrandId).ToList();

                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, data);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        //[Route("GetHistory")]
        //[AcceptVerbs("Post")]
        //public HttpResponseMessage GetHistory(hisDTO obj)
        //{
        //    logger.Info("start single  GetcusomerWallets: ");
        //    using (AuthContext context = new AuthContext())
        //    {
        //        SupplierController Item = new SupplierController();
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;

        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //            }
        //            List<CustomerWalletHistory> listobj = new List<CustomerWalletHistory>();

        //            foreach (widclass q in obj.ids)
        //            {
        //                List<SupplierBrandMap> h = context.SupplierBrandMaps.Where(a => a.BrandId == q.id ).ToList();

        //                foreach (SupplierBrandMap a in h)
        //                {
        //                    try
        //                    {
        //                        Supplier c = context.Suppliers.Where(s => s.SupplierId == a.SupplierId).SingleOrDefault();

        //                        SubCategory subcat = context.SubCategorys.Where(x => x.SubCategoryId == a.BrandId).SingleOrDefault();
        //                    }
        //                    catch { }
        //                    listobj.Add(a);
        //                }
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, listobj);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
        //            logger.Info("End  single GetcusomerWallets: ");
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
        //        }
        //    }
        //}

        //        public class widclass
        //        {
        //            public int id { get; set; }
        //        }

        //        public class hisDTO
        //        {
        //            public DateTime From { get; set; }
        //            public DateTime TO { get; set; }
        //            public List<widclass> ids { get; set; }
        //        }
        //    }
        //}



        [ResponseType(typeof(Supplier))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteSupplier: ");
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    context.DeleteSupplier(id, CompanyId);
                }
                logger.Info("End  delete Supplier: ");
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteSupplier " + ex.Message);
            }
        }

        [Authorize]
        [Route("search")]
        public IEnumerable<Supplier> GetSupplier(string key)
        {
            logger.Info("start Supplier: ");
            List<Supplier> ass = new List<Supplier>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int WarehouseId = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        WarehouseId = int.Parse(claim.Value);
                    }
                }

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                int CompanyId = compid;
                using (AuthContext context = new AuthContext())
                {

                    if (WarehouseId > 0)
                    {
                        ass = context.Suppliers.Where(s => s.SUPPLIERCODES.Contains(key) || s.Name.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId && s.WarehouseId == WarehouseId).ToList();
                        logger.Info("End  Supplier: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.Suppliers.Where(s => s.SUPPLIERCODES.Contains(key) || s.Name.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId).ToList();
                        logger.Info("End  Supplier: ");
                        return ass;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier " + ex.Message);
                logger.Info("End  Supplier: ");
                return null;
            }
        }





        #region OTP for Supplier APP
        /// <summary>
        /// to generate OTP for Supplier 
        /// </summary> tejas 07/2019
        /// <param name="MobileNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous] // dont Remove OTP/Signup will not Work for New Suppliers not returning token yet -  SHOULD BE DONE
        [Route("GenotpForSupplier")]
        public OTP GenotpForSupplier(string MobileNumber)
        {
            logger.Info("start Gen OTP: ");
            try
            {
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                //  string OtpMessage = " : is Your Verification Code. :).ShopKirana";
                string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "Customer_Verification_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;

                OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                //string CountryCode = "91";
                //string Sender = "SHOPKR";
                //string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";
                //int route = 4;
                //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

                //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                //webRequest.Method = "GET";
                //webRequest.ContentType = "application/json";
                //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                //webRequest.ContentLength = 0; // added per comment 
                //webRequest.Credentials = CredentialCache.DefaultCredentials;
                //webRequest.Accept = "*/*";
                //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
                //logger.Info("OTP Genrated: " + sRandomOTP);
                bool result = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, (sRandomOTP + " :" + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                OTP a = new OTP()
                {
                    OtpNo = sRandomOTP
                };
                return a;

            }
            catch (Exception ex)
            {
                logger.Error("Error in OTP Genration.");
                return null;
            }
        }
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

        #region RA: Forgot Password for upplier App
        /// <summary>
        /// Forget password for supplier app 
        /// mobile no input and if supplier exists we will send passord on registered mobile number 
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        [Route("forgetPasswordForSapp")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage forgetPasswordForSapp(string MobileNo)
        {
            logger.Info("start Forget password OTP: ");
            supplierDetailsFSApp cd;
            //Customer customer = new Customer();
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;

                    Supplier Supplierss = db.Suppliers.Where(c => c.MobileNo == MobileNo).SingleOrDefault();

                    if (Supplierss != null)
                    {
                        new Sms().sendOtp(Supplierss.MobileNo, "Hi " + Supplierss.Name + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + Supplierss.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com", "");

                        cd = new supplierDetailsFSApp()
                        {
                            Suppliers = Supplierss,
                            Status = true,
                            Message = "Message send to your registered mobile number."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, cd);
                    }
                    else
                    {
                        cd = new supplierDetailsFSApp()
                        {
                            Suppliers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, cd);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    cd = new supplierDetailsFSApp()
                    {
                        Suppliers = null,
                        Status = false,
                        Message = ex.Message
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, cd);
                }
            }
        }
        #endregion


        #region after OTP is verified used to register supplier //or OTP login
        /// <summary>
        ///  this api is used to register supplier phone number 
        /// </summary> tejas 07/2019
        /// <param name="MobileNumber"></param>
        /// <param name="IsOTPverified"></param>
        /// <returns></returns>
        [Route("GetLogedSupplier")]
        [AllowAnonymous]  // dont remove or signup supplier will not work 
        [HttpPost]
        public HttpResponseMessage GetLogedSupplier(string MobileNumber, bool IsOTPverified)
        {
            logger.Info("start Register supplier By Mobile: ");
            try
            {
                using (var db = new AuthContext())
                {
                    if (IsOTPverified == true)
                    {

                        var supplier = db.Suppliers.Where(k => k.MobileNo == MobileNumber).SingleOrDefault();
                        int id = 0;
                        Supplier s = new Supplier();
                        if (supplier == null)
                        {
                            var registeredApk = db.GetAPKUserAndPwd("SupplierApp");  // change "DeliveryApp" to "SupplierApp" register in asp.net user
                            s.RegisteredApk = registeredApk;
                            s.MobileNo = MobileNumber;
                            s.IsVerified = false;
                            db.Suppliers.Add(s);
                            id = db.Commit();

                            //token for further use of android app 
                            var res = new supplierDetail()
                            {
                                supplierId = s,
                                Status = true,
                                Message = "Supplier number saved Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            var res = new supplierDetail()
                            {
                                Status = false,
                                Message = "Supplier Already Exists."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        var res = new supplierDetail()
                        {
                            supplierId = null,
                            Status = false,
                            Message = "OTP is not Right."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }

        }

        [Route("SupplierMappedCityForMobile")]
        [HttpPost]
        public Supplier SupplierMappedCityForMobile(SupplierMappedCity obj)
        {
            logger.Info("start Mapped Supplier: ");
            try
            {
                using (var db = new AuthContext())
                {
                    var FirstCityWh = db.Warehouses.Where(a => a.Cityid == obj.CityId && a.active == true).FirstOrDefault();
                    if (FirstCityWh != null)
                    {
                        SupplierWarehouse sup = db.SupplierWarehouseDB.Where(x => x.SupplierId == obj.SupplierId && x.WarehouseId == FirstCityWh.WarehouseId).FirstOrDefault();
                        City City = db.Cities.Where(a => a.Cityid == obj.CityId).FirstOrDefault();
                        if (sup == null)
                        {
                            SupplierWarehouse sw = new SupplierWarehouse();
                            sw.SupplierId = obj.SupplierId;
                            sw.WarehouseId = FirstCityWh.WarehouseId;
                            sw.WarehouseName = FirstCityWh.WarehouseName;
                            sw.CompanyId = FirstCityWh.CompanyId;
                            sw.CreatedDate = DateTime.Now;
                            sw.UpdatedDate = DateTime.Now;
                            sw.CompanyId = FirstCityWh.CompanyId;
                            db.SupplierWarehouseDB.Add(sw);
                            int id = db.Commit();
                            Supplier s = db.Suppliers.Where(x => x.SupplierId == sw.SupplierId).FirstOrDefault();
                            s.WarehouseId = FirstCityWh.WarehouseId;
                            s.WarehouseName = FirstCityWh.WarehouseName;
                            s.Cityid = City.Cityid;
                            s.City = City.CityName;
                            s.CompanyId = FirstCityWh.CompanyId;
                            s.IsCityVerified = true;
                            s.UpdatedDate = DateTime.Now;
                            db.Commit();
                            return s;
                        }
                        else
                        {
                            Supplier s = db.Suppliers.Where(x => x.SupplierId == sup.SupplierId).FirstOrDefault();
                            s.WarehouseId = Convert.ToInt32(sup.WarehouseId);
                            s.WarehouseName = sup.WarehouseName;
                            return s;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        #region to Register Supplier and Update Supplier Profile
        /// <summary>
        /// this api is used to  Register Supplier and Update Supplier Profile
        /// from supplier app tejas 07/2019
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterSupplierMobile")]
        public IHttpActionResult RegisterSupplierMobile(Supplier userModel)
        {
            logger.Info("Update Profile and Register Supplier: ");
            try
            {
                using (var db = new AuthContext())
                {
                    var supplier = db.AllSupplier(userModel.CompanyId).Where(s => s.MobileNo == userModel.MobileNo);
                    if (supplier != null)
                    {
                        Supplier s = db.Suppliers.Where(x => x.MobileNo == userModel.MobileNo).SingleOrDefault();
                        s.Name = userModel.Name;
                        s.CompanyId = 1;   // to show suppler in backend if we remove this new suppliers wont show in backend>suppliers
                        s.SupplierCaegoryId = userModel.SupplierCaegoryId;
                        s.Brand = userModel.Brand;
                        s.OwnerName = userModel.OwnerName;
                        s.ContactPerson = userModel.ContactPerson;
                        s.MobileNo = userModel.MobileNo;
                        s.Password = userModel.Password;
                        s.SupplierAddress = userModel.SupplierAddress;
                        s.Pincode = userModel.Pincode;
                        s.TINNo = userModel.TINNo;   //this is GST
                        s.Pancard = userModel.Pancard;
                        s.HeadOffice = userModel.HeadOffice;
                        s.Bank_AC_No = userModel.Bank_AC_No;
                        s.Bank_Name = userModel.Bank_Name;
                        s.Bank_Ifsc = userModel.Bank_Ifsc;
                        s.OpeningHours = userModel.OpeningHours;
                        s.EmailId = userModel.EmailId;
                        s.ShopName = userModel.ShopName;
                        s.EstablishmentYear = userModel.EstablishmentYear;
                        s.bussinessType = userModel.bussinessType;
                        s.Description = userModel.Description;
                        s.StartedBusiness = userModel.StartedBusiness;
                        s.FSSAI = userModel.FSSAI;
                        s.ManageAddress = userModel.ManageAddress;
                        s.fcmId = userModel.fcmId;               // for notification 
                        s.DeviceId = userModel.DeviceId;                   // for notification 
                        s.CreatedDate = DateTime.Now;
                        s.UpdatedDate = DateTime.Now;
                        // db.Suppliers.Attach(s);
                        db.Entry(s).State = EntityState.Modified;
                        db.Commit();
                        var res = new supplierData()
                        {
                            supplier = s,
                            Status = true,
                            Message = "Supplier Updated"
                        };
                        return Content(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new supplierData()
                        {
                            Status = false,
                            Message = "Some problem"
                        };
                        return Content(HttpStatusCode.BadRequest, res);
                    }
                }
            }
            catch (Exception ee)
            {
                var res = new supplierData()
                {
                    Status = false,
                    Message = "Supplier Not Updated" + ee.Message
                };
                return Content(HttpStatusCode.BadRequest, res);
            }
        }
        #endregion


        #region SPA: Update Supplier
        /// <summary>
        /// created by 01/04/2019
        /// Update customer for sales person app
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(Supplier))]
        [Route("UpdateSupplier")]
        [AcceptVerbs("PUT")]
        [AllowAnonymous]
        public HttpResponseMessage UpdateSupplier(Supplier Supplier)
        {
            SupplierDetail res;
            try
            {
                using (var db = new AuthContext())
                {
                    var c = db.Suppliers.Where(s => s.SupplierId == Supplier.SupplierId && s.Deleted == false).FirstOrDefault();
                    c.Name = Supplier.Name;
                    c.EmailId = Supplier.EmailId;
                    c.BillingAddress = Supplier.BillingAddress;
                    c.ShippingAddress = Supplier.ShippingAddress;
                    c.MobileNo = Supplier.MobileNo;
                    c.Pincode = Supplier.Pincode;
                    c.Bank_AC_No = Supplier.Bank_AC_No;
                    c.Bank_Ifsc = Supplier.Bank_Ifsc;
                    c.Bank_Name = Supplier.Bank_Name;
                    c.Bank_Ifsc = Supplier.Bank_Ifsc;
                    c.BankPINno = Supplier.BankPINno;
                    c.ChequeImageUrl = Supplier.ChequeImageUrl;
                    c.SUPPLIERCODES = Supplier.SUPPLIERCODES;
                    c.ContactPerson = Supplier.ContactPerson;
                    c.CreatedDate = Supplier.CreatedDate;
                    c.TINNo = Supplier.TINNo;                    //this is GST
                    c.ShopName = Supplier.ShopName;
                    c.EstablishmentYear = Supplier.EstablishmentYear;
                    c.bussinessType = Supplier.bussinessType;
                    c.Description = Supplier.Description;
                    c.StartedBusiness = Supplier.StartedBusiness;
                    c.FSSAI = Supplier.FSSAI;
                    c.ManageAddress = Supplier.ManageAddress;
                    c.ImageUrl = Supplier.ImageUrl;
                    c.businessImageUrl = Supplier.businessImageUrl;
                    c.UpdatedDate = DateTime.Now;
                    db.Entry(c).State = EntityState.Modified;
                    db.Commit();
                    res = new SupplierDetail()
                    {
                        suppliers = c,
                        Status = true,
                        Message = "Update successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in UpdatSupplier " + ex.Message);
                res = new SupplierDetail()
                {
                    suppliers = null,
                    Status = false,
                    Message = "something went wrong."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion


        #region LoginSupplierMobile for new supplier app 
        /// <summary>
        /// this API is for supplier app For login 
        /// par MobileNumber and Password
        /// </summary>
        /// <param name="MobileNumber"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [AllowAnonymous]  // dont Remove this or APP login will stop ---------------------------------------------------------------------------
        [HttpPost]
        [Route("LoginSupplierMobile")]
        public IHttpActionResult LoginSupplierMobile(string MobileNumber, string Password, string fcmId, string DeviceId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Login with Supplier through Mobile: ");
                Supplier supplier = db.Suppliers.Where(x => x.MobileNo == MobileNumber.Trim() && x.Password == Password.Trim()).SingleOrDefault();
                if (supplier != null)
                {
                    var registeredApk = db.GetAPKUserAndPwd("SupplierApp");    // change "DeliveryApp" to "SupplierApp" register in asp.net user
                    supplier.RegisteredApk = registeredApk;
                    supplier.fcmId = fcmId;
                    supplier.DeviceId = DeviceId;
                    db.Entry(supplier).State = EntityState.Modified;
                    db.Commit();
                    var res = new supplierData()
                    {
                        supplier = supplier,
                        Status = true,
                        Message = "Login Successfull",
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new supplierData()
                    {
                        supplier = null,
                        Status = false,
                        Message = "Invalid UserName and Password"
                    };
                    return Content(HttpStatusCode.OK, res);
                }
            }

        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("LoginSupplierMobileviaOtp")]
        public IHttpActionResult LoginSupplierMobileviaOtp(string MobileNumber, string Otp, string fcmId, string DeviceId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Login with Supplier through Mobile: ");
                Supplier supplier = db.Suppliers.Where(x => x.MobileNo == MobileNumber.Trim() && x.Otp == Otp.Trim() && x.Active == true && x.Deleted == false).SingleOrDefault();
                if (supplier != null)
                {
                    var registeredApk = db.GetAPKUserAndPwd("SupplierApp");
                    supplier.RegisteredApk = registeredApk;
                    supplier.fcmId = fcmId;
                    supplier.DeviceId = DeviceId;
                    db.Entry(supplier).State = EntityState.Modified;
                    db.Commit();
                    var res = new supplierData()
                    {
                        supplier = supplier,
                        Status = true,
                        Message = "Login Successfull",
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new supplierData()
                    {
                        supplier = null,
                        Status = false,
                        Message = "Supplier Not Registered!",
                    };
                    return Content(HttpStatusCode.OK, res);
                }
            }

        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GenotpForSupplierLogin")]
        public SupplierOtp GenotpForSupplierLogin(string MobileNumber)
        {
            SupplierOtp a = new SupplierOtp { Message = "", OtpNo = "", Status = false, Supplier = null };
            logger.Info("start Gen OTP: ");
            try
            {
                using (var db = new AuthContext())
                {
                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                    string OtpMessage = "";
                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "Customer_Verification_Code");
                    OtpMessage = dltSMS == null ? "" : dltSMS.Template;

                    OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                    OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                    Supplier supplier = db.Suppliers.Where(x => x.MobileNo == MobileNumber.Trim()).SingleOrDefault();
                    if (supplier != null)
                    {
                        if (supplier != null && supplier.Active && !supplier.Deleted && !supplier.IsVerified)
                        {
                            supplier.Otp = sRandomOTP;
                            var registeredApk = db.GetAPKUserAndPwd("SupplierApp");
                            supplier.RegisteredApk = registeredApk;
                            db.Entry(supplier).State = EntityState.Modified;
                            db.Commit();
                            bool result = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, (sRandomOTP + " :" + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                            a = new SupplierOtp()
                            {

                                OtpNo = sRandomOTP,
                                Supplier = supplier,
                                Status = true,
                                Message = "Otp Sent Successfully",
                            };

                            return a;
                        }
                        else if (supplier != null && !supplier.Active && !supplier.Deleted && !supplier.IsVerified)
                        {
                            a = new SupplierOtp()
                            {

                                OtpNo = sRandomOTP,
                                Supplier = supplier,
                                Status = true,
                                Message = "User Is Inactive!",
                            };

                            return a;
                        }
                        else if (supplier != null && !supplier.Active && supplier.IsVerified)
                        {
                            a = new SupplierOtp()
                            {

                                OtpNo = sRandomOTP,
                                Supplier = supplier,
                                Status = false,
                                Message = "User Is Blocked!!",
                            };
                            return a;
                        }
                    }
                    else
                    {
                        a = new SupplierOtp()
                        {

                            OtpNo = sRandomOTP,
                            Supplier = supplier,
                            Status = false,
                            Message = "User Not Registered!",
                        };

                        return a;
                    }
                }
                return a;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OTP Genration.");
                return null;
            }
        }

        [Route("TotalOutstandingList")]
        [HttpGet]
        public List<TotalOutstandingDc> TotalOutstandingList(int SupplierId)
        {
            string spName = "GetTotalOutstandingListSp @SupplierId";
            using (var context = new AuthContext())
            {
                var supplierParam = new SqlParameter
                {
                    ParameterName = "SupplierId",
                    Value = SupplierId
                };

                var list = context.Database.SqlQuery<TotalOutstandingDc>(spName, supplierParam).ToList();
                return list;
            }
        }

        [Route("TotalOverDueList")]
        [HttpGet]
        public List<TotalOverDueDc> TotalOverDueList(int SupplierId)
        {
            string spName = "GetTotalOverDueListSp @SupplierId";
            using (var context = new AuthContext())
            {
                var supplierParam = new SqlParameter
                {
                    ParameterName = "SupplierId",
                    Value = SupplierId
                };

                var list = context.Database.SqlQuery<TotalOverDueDc>(spName, supplierParam).ToList();
                return list;
            }
        }



        #region supplier info for new supplier app 
        /// <summary>
        /// this API is for supplier app to get profile info of suppiler in respect to supplier ID supplier should not be deleted 
        /// </summary> tejas 07/2019
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpPost]
        [Route("GetSupplierUpdatedInfo")]
        public IHttpActionResult GetSupplierUpdatedInfo(int SupplierId)
        {
            using (var db = new AuthContext())
            {

                Supplier supplier = db.Suppliers.Where(x => x.SupplierId == SupplierId && x.Deleted == false).SingleOrDefault();
                if (supplier != null)
                {
                    var res = new supplierData()
                    {
                        supplier = supplier,
                        Status = true,
                        Message = "Supplier info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new supplierData()
                    {
                        supplier = null,
                        Status = false,
                        Message = "NO Supplier With the Supplier ID or supplier is deleted" + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion


        #region Warehouses for Supplier 
        /// <summary>
        /// this API is for supplier app to get profile info of suppiler in respect to supplier ID supplier should not be deleted 
        /// </summary> tejas 07/2019
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetSupplierWarehouses")]
        public HttpResponseMessage GetSupplierWarehouses(int SupplierId)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    SupplierWarehouseObj resobj;

                    List<SupplierWarehouseDTO> _result = new List<SupplierWarehouseDTO>();
                    string sqlquery = "select distinct(im.WarehouseId),im.WarehouseName from ItemMasters im with(nolock) where SupplierId =" + SupplierId + " ORDER BY  im.WarehouseId,im.WarehouseName";
                    _result = db.Database.SqlQuery<SupplierWarehouseDTO>(sqlquery).ToList();

                    if (_result.Count > 0)
                    {
                        resobj = new SupplierWarehouseObj()
                        {
                            whList = _result,
                            Status = true,
                            Message = "Success"
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, resobj);
                    }
                    else
                    {
                        resobj = new SupplierWarehouseObj()
                        {
                            whList = null,
                            Status = false,
                            Message = "Failed"
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, resobj);
                    }
                }
            }
            catch (Exception ex)
            {
                SupplierWarehouseObj resobj = new SupplierWarehouseObj()
                {
                    whList = null,
                    Status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, resobj);
            }
        }
        #endregion

        #region get chat info for new supplier app 
        /// <summary>
        /// this API is for supplier app to get profile info of suppiler in respect to supplier ID supplier should not be deleted 
        /// </summary> tejas 07/2019
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetChatForSupplier")]
        public IHttpActionResult GetChatForSupplier(int SupplierId, int PurchaseId)
        {
            using (var db = new AuthContext())
            {
                List<SupplierChat> supplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId && x.PurchaseId == PurchaseId).ToList();

                if (supplierChat != null)
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = supplierChat,
                        Status = true,
                        Message = "Chat info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = null,
                        Status = false,
                        Message = "NO chat With the Supplier ID " + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, res);
                }

            }
        }
        #endregion

        [AllowAnonymous]
        [Route("GetChatForSupplierDateWise")]
        [HttpGet]
        public dynamic GetChatForSupplierDateWise(int SupplierId, DateTime FromDate)
        {
            using (var db = new AuthContext())
            {

                //if (SupplierId == null)
                //{
                //    SupplierId = 0;
                //}
                string Start = FromDate.ToString("yyyy-MM-dd");
                //var Id = new SqlParameter("SupplierId", SupplierId ?? (object)DBNull.Value);
                //var StartDate = new SqlParameter("StartDate", FromDate ?? (object)DBNull.Value);
                List<SupplierChat> supplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId && x.ChatTime >= FromDate).ToList();
                if (supplierChat != null)
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = supplierChat,
                        Status = true,
                        Message = "Chat info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                return true;
            }

        }

        #region get chat info for new supplier app 
        /// <summary>
        /// this API is for supplier app to get profile info of suppiler in respect to supplier ID supplier should not be deleted 
        /// </summary> tejas 07/2019
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetChatForSupplierA7")]
        public IHttpActionResult GetChatForSupplier(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                List<SupplierChat> supplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId).ToList();
                if (supplierChat != null)
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = supplierChat,
                        Status = true,
                        Message = "Chat info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = null,
                        Status = false,
                        Message = "NO chat With the Supplier ID " + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion


        #region get chat info for new supplier app 
        /// <summary>
        /// this API is for supplier app to get profile info of suppiler in respect to supplier ID supplier should not be deleted 
        /// </summary> tejas 07/2019
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetChatForSupplierPOid")]
        public IHttpActionResult GetChatForSupplierPOid(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                //List<SupplierChat> supplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId).ToList();
                //List<int?> SupplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId && x.PurchaseId != null).OrderByDescending(x => x.ChatId).Select(q => q.PurchaseId).Distinct().ToList();
                string sqlquery = "SELECT [PurchaseId] FROM [SupplierChats]"
                                  + "where PurchaseId >0 and SupplierId = " + SupplierId +
                                   "GROUP BY [PurchaseId] ORDER BY MAX([ChatId]) DESC";



                List<int?> SupplierChat = db.Database.SqlQuery<int?>(sqlquery).ToList();

                if (SupplierChat != null)
                {
                    var res = new PoDISTINCT()
                    {
                        POid = SupplierChat,
                        Status = true,
                        Message = "Chat info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new PoDISTINCT()
                    {
                        POid = null,
                        Status = false,
                        Message = "NO chat With the Supplier ID " + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, res);
                }
            }
        }

        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetSupplierChat")]
        public IHttpActionResult GetSupplierChat(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                List<SupplierChat> supplierChats = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId && x.PurchaseId == 0).ToList();

                if (supplierChats != null)
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = supplierChats.OrderBy(x => x.ChatId).ToList(),
                        Status = true,
                        Message = "Chat info for " + SupplierId
                    };
                    return Content(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new ChatData()
                    {
                        ChatSupplier = null,
                        Status = false,
                        Message = "NO chat With the Supplier ID " + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region GetChatForSupplierPOidA7  
        /// <summary>
        /// 
        /// to get active threrads for chat 
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [AllowAnonymous]     // removethis for the finalcomit
        [HttpGet]
        [Route("GetChatForSupplierPOidA7")]
        public IHttpActionResult GetChatForSupplierPOidA7(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                //List<SupplierChat> supplierChat = db.SupplierChatDB.Where(x => x.SupplierId == SupplierId).ToList();
                string sqlquery = "SELECT [PurchaseId] FROM [SupplierChats]"
                                   + "where PurchaseId is not null and SupplierId = " + SupplierId +
                                    "GROUP BY [PurchaseId] ORDER BY MAX([ChatId]) DESC";



                List<int> SupplierChat = db.Database.SqlQuery<int>(sqlquery).ToList();


                return Content(HttpStatusCode.OK, SupplierChat);

            }
        }



        #endregion


        #region to add chat data for Supplier app
        /// <summary>
        /// this api is used  to add chat data for Supplier app
        /// from supplier app tejas 07/2019
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("PostChatFromSupplierApp")]
        public IHttpActionResult PostChatFromSupplierApp(ChatFromSappDTO ChatFromSapp)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    if (ChatFromSapp != null)
                    {
                        var chatD = new SupplierChat();
                        chatD.SupplierId = ChatFromSapp.SupplierId;
                        chatD.SupplierName = ChatFromSapp.SupplierName;
                        chatD.PurchaseId = ChatFromSapp.PurchaseId;
                        chatD.Chat = ChatFromSapp.Chat;
                        chatD.AudioURL = ChatFromSapp.AudioURL;
                        chatD.ImageURL = ChatFromSapp.ImageURL;
                        chatD.isfromSupplier = true;
                        chatD.ChatTime = DateTime.Now;
                        db.SupplierChatDB.Add(chatD);
                        db.Commit();


                        try
                        {

                            var Foremail = from c in db.DPurchaseOrderMaster.Where(X => X.PurchaseOrderId == ChatFromSapp.PurchaseId)
                                           join p in db.Peoples
                                           on c.BuyerId equals p.PeopleID
                                           select new PostChatFromSupplierAppDT0
                                           {
                                               PeopleID = p.PeopleID,
                                               Department = p.Department,
                                               warehouseID = c.WarehouseId,
                                               email = p.Email
                                           };
                            var ForemailV1 = Foremail.FirstOrDefault();
                            var suppliersData = db.Suppliers.Where(x => x.SupplierId == ChatFromSapp.SupplierId).FirstOrDefault();

                            string query = "select p.Email from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where warehouseid={0} and r.Name in ('HQ Purchase Executive','HQ Accounts Executive') group by p.Email";
                            query = string.Format(query, ForemailV1.warehouseID);

                            var emails = db.Database.SqlQuery<string>(query).ToList();

                            //var ExecutiveEmailPurchase = db.Peoples.Where(x => x.Permissions == "HQ Purchase Executive" && x.WarehouseId == ForemailV1.warehouseID).ToList();
                            //var ExecutiveEmailAccount = db.Peoples.Where(x => x.Permissions == "HQ Accounts Executive" && x.WarehouseId == ForemailV1.warehouseID).ToList();

                            string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                            string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                            string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                            body += "Hello,";
                            body += "<p>Supplier Code :" + suppliersData.SUPPLIERCODES + " </p>";
                            body += "<h3 style='background-color: rgb(241, 89, 34);'>New Meassage From Supplier! Supplier Name : " + suppliersData.Name + "</h3> ";
                            if (ChatFromSapp.PurchaseId != null)
                            {
                                body += "<p> Regarding Purchase Order ID :" + ChatFromSapp.PurchaseId + "</p>";
                            }
                            body += "<p>Message From Supplier : " + ChatFromSapp.Chat + " </p>";
                            body += "<p>Time of Message :" + chatD.ChatTime + "</p>";
                            body += "<p>Go to this Link to reply to the Message:" + "https://saral.shopkirana.in/" + "</p>";
                            body += "<p><strong>";
                            body += "Thanks,";
                            body += "<br />";
                            body += "<b>IT Team</b>";
                            body += "</div>";
                            var Subj = "Alert! " + suppliersData.Name + "  Has Sent a Chat, Supplier Code :  " + suppliersData.SUPPLIERCODES;
                            var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                            msg.To.Add(ForemailV1.email);

                            foreach (var av in emails)
                            {
                                msg.To.Add(av);
                            }
                            //foreach (var pu in ExecutiveEmailAccount)
                            //{
                            //    msg.To.Add(pu.Email);
                            //}
                            msg.IsBodyHtml = true;
                            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                            smtpClient.UseDefaultCredentials = true;
                            smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                            smtpClient.EnableSsl = true;
                            smtpClient.Send(msg);
                        }
                        catch (Exception ss)
                        {

                        }
                        var res = new supplierData()
                        {
                            Status = true,
                            Message = "chat Updated"
                        };
                        return Content(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new supplierData()
                        {
                            Status = false,
                            Message = "Some problem"
                        };
                        return Content(HttpStatusCode.BadRequest, res);
                    }
                }
            }
            catch (Exception ee)
            {
                var res = new supplierData()
                {
                    Status = false,
                    Message = "chat Not Updated" + ee.Message
                };
                return Content(HttpStatusCode.BadRequest, res);
            }
        }
        #endregion


        #region to add chat data for Supplier from backend 
        /// <summary>
        /// this api is used  to add chat data for  Supplier from backend 
        /// from supplier app tejas 07/2019
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("PostChatFromSupplierBack")]
        public IHttpActionResult PostChatFromSupplierBack(ChatFromSappDTO ChatFromSapp)
        {
            try

            {
                using (var db = new AuthContext())
                {
                    if (ChatFromSapp.PurchaseId != null)
                    {
                        var chatD = new SupplierChat();
                        chatD.SupplierId = ChatFromSapp.SupplierId;
                        chatD.Chat = ChatFromSapp.Chat;

                        chatD.isfromSupplier = false; // because it is from UI
                        chatD.ChatTime = DateTime.Now;
                        //----
                        chatD.PeopleID = ChatFromSapp.PeopleID;
                        chatD.PeopleName = ChatFromSapp.PeopleName;
                        chatD.PurchaseId = ChatFromSapp.PurchaseId;
                        db.SupplierChatDB.Add(chatD);
                        db.Commit();
                        try
                        {
                            db.SupplierNotificationChat(ChatFromSapp.SupplierId, ChatFromSapp.Chat, ChatFromSapp.PurchaseId);
                        }
                        catch { }


                        var res = new supplierData()
                        {
                            Status = true,
                            Message = "chat Updated"
                        };
                        return Content(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new supplierData()
                        {
                            Status = false,
                            Message = "Some problem"
                        };
                        return Content(HttpStatusCode.BadRequest, res);
                    }
                }
            }
            catch (Exception ee)
            {
                var res = new supplierData()
                {
                    Status = false,
                    Message = "chat Not Updated" + ee.Message
                };
                return Content(HttpStatusCode.BadRequest, res);
            }
        }
        #endregion

        #region get depo for Supplier App
        /// <summary>
        /// this API is for supplier app to get depo info of suppiler in respect to supplier ID
        /// </summary> depo should not be delted and should be active 
        /// tejas 07/2019 
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]     // removethis for the finalcomit
        [Route("GetDeposForSupplierApp")]
        public IHttpActionResult GetDeposForSupplierApp(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                List<DepoMasterDTO> _result = new List<DepoMasterDTO>();
                string sqlquery = " select distinct DepoId, im.DepoName,im.phone,im.CityName from DepoMasters im with(nolock) where SupplierId =" + SupplierId + "  and Deleted = 0 and IsActive = 1";

                _result = db.Database.SqlQuery<DepoMasterDTO>(sqlquery).ToList();

                if (_result != null)
                {
                    var rest = new supplierDataDepo()
                    {
                        Status = true,
                        Message = "Success",
                        depo = _result
                    };
                    return Content(HttpStatusCode.OK, rest);
                }
                else
                {
                    var rest = new supplierDataDepo()
                    {
                        depo = null,
                        Status = false,
                        Message = "no depos or depo deleted or deactive depo for Supplierid" + SupplierId
                    };
                    return Content(HttpStatusCode.BadRequest, rest);
                }
            }
        }
        #endregion

        [Route("ForgotPasswordSupplierMobile")]
        [HttpGet]
        public HttpResponseMessage ForgotPasswordSupplierMobile(string Mobile)
        {
            logger.Info("Forgot Password By Supplier: ");
            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    Supplier supplier = db.Suppliers.Where(x => x.MobileNo == Mobile).FirstOrDefault();
                    if (supplier != null)
                    {
                        new Sms().sendOtp(supplier.MobileNo, "Hi " + supplier.Name + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + supplier.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com", "");
                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier " + ex.Message);
                logger.Info("End  Customer: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("GetSupplierItemMobile")]
        [HttpGet]
        public IEnumerable<ItemMaster> GetSupplierItemMobile()
        {
            logger.Info("Showing Item to Supplier: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            try
            {
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {
                        ass = context.AllItemMasterWid(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.AllItemMaster(CompanyId).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("GetAllPOMobile")]
        public List<PurchaseOrderMaster> GetAllPOMobile(int SupplierId)
        {
            logger.Info("Get PO for Supplier: ");
            try
            {
                using (var db = new AuthContext())
                {

                    if (SupplierId != 0)
                    {
                        int supplierid = Convert.ToInt32(SupplierId);
                        var res = db.DPurchaseOrderMaster.Where(po => po.SupplierId == supplierid).ToList();
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception ee)
            {
                return null;
            }

        }

        [HttpGet]
        [Route("GetAllPODetailMobile")]
        public dynamic GetAllPODetailMobile(int PurchaseOrderId, int Supplierid)
        {
            logger.Info("Get PO Detail Supplier: ");
            try
            {
                using (var db = new AuthContext())
                {
                    if (PurchaseOrderId != 0 && Supplierid != 0)
                    {
                        int supplierid = Convert.ToInt32(Supplierid);
                        int purchaseOrderId = Convert.ToInt32(PurchaseOrderId);
                        var res = db.DPurchaseOrderDeatil.Where(po => po.SupplierId == supplierid && po.PurchaseOrderId == purchaseOrderId).ToList();
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ee)
            {
                return null;
            }

        }

        #region get Supllier Master History
        //[Route("supplierhistory")]
        //[HttpGet]
        //public dynamic supplierhistory(int SupplierId)
        //{

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0, Warehouse_id = 0;
        //        // Access claims
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }
        //        int CompanyId = compid;
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //        var data = odd.SupplierHistoryDB.Where(x => x.SupplierId == SupplierId).ToList();
        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        #endregion

        #region get the supplier payment data
        /// <summary>
        /// Created date 29/07/2019
        /// Created by raj
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("supplierPaymentData")]
        [HttpGet]
        public dynamic supplierhistory(int SupplierId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                int CompanyId = compid;
                using (AuthContext odd = new AuthContext())
                {
                    var data = odd.SupplierPaymentDataDB.Where(x => x.SupplierId == SupplierId && x.CompanyId == CompanyId).ToList();
                    logger.Info("Successfully get Supplier payment data ");
                    return data;

                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier payment data " + ex.Message);
                return false;
            }
        }
        #endregion
        [Route("SearchPayment")]
        [HttpGet]
        public dynamic SearchPayment(DateTime? start, DateTime? End, int SupplierId)
        {

            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var data = db.SupplierPaymentDataDB.Where(x => x.SupplierId == SupplierId/* && x.WarehouseId == Warehouse_id*/ && x.CompanyId == compid && x.InVoiceDate >= start && x.InVoiceDate <= End).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Route("SearchPaymentRefrence")]
        [HttpGet]
        public dynamic SearchPaymentRefrence(string RefrenceNumber, int SupplierId)
        {

            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var data = db.SupplierPaymentDataDB.Where(x => x.SupplierId == SupplierId /*&& x.WarehouseId == Warehouse_id*/ && x.CompanyId == compid && x.InVoiceNumber == RefrenceNumber).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Created By 25/02/2019 
        /// Add Supplier from Supplier App
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        [ResponseType(typeof(Supplier))]
        [Route("addSupp")]
        [AcceptVerbs("POST")]
        public Supplier addSup(Supplier supplier)
        {
            logger.Info("start supplier: ");
            using (var db = new AuthContext())
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int compid = 0, userid = 0;
                        int Warehouse_id = 0;
                        // Access claims
                        foreach (Claim claim in identity.Claims)
                        {
                            if (claim.Type == "compid")
                            {
                                compid = int.Parse(claim.Value);
                            }
                            if (claim.Type == "userid")
                            {
                                userid = int.Parse(claim.Value);
                            }
                            if (claim.Type == "Warehouseid")
                            {
                                Warehouse_id = int.Parse(claim.Value);
                            }

                        }
                        supplier.CompanyId = compid;
                        supplier.WarehouseId = Warehouse_id;

                        if (supplier == null)
                        {
                            throw new ArgumentNullException("supplier");
                        }
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        context.AddSupplier(supplier);
                        logger.Info("End  addsupplier: ");
                        return supplier;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addsupplier " + ex.Message);
                    logger.Info("End  addsupplier: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Created By 25/02/2019 
        /// Update Supplier from Supplier App
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        [ResponseType(typeof(Supplier))]
        [Route("PutSupp")]
        [AcceptVerbs("PUT")]
        public Supplier PutSup(Supplier supplier)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    supplier.userid = userid;
                    supplier.CompanyId = compid;
                    supplier.WarehouseId = Warehouse_id;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutSupplier(supplier);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get buyer from people
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBuyer")]
        public List<BuyerMinDc> GetBuyer()
        {
            logger.Info("Get Buyer from people: ");
            try
            {
                using (var db = new AuthContext())
                {

                    string query = string.Format("select  p.DisplayName,p.PeopleID from People p where p.Deleted=0 and exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name in ('{0}','{1}','{2}','{3}','{4}','{5}','{6}'))",
                                            "HQ Purchase Executive", "HQ Purchase Associate", "Purchase Administrator", "HQ Purchase Lead", "HQ Sourcing Associate", "HQ Sourcing Executive", "HQ Sourcing Lead");

                    List<BuyerMinDc> buyerMinDcs = db.Database.SqlQuery<BuyerMinDc>(query).ToList();
                    return buyerMinDcs;
                }
            }

            catch (Exception ee)
            {
                return null;
            }
        }
        [HttpGet]
        [Route("SupDetail")]
        public Supplier GetBuyer(int sid)
        {
            logger.Info("Get Buyer from people: ");
            try
            {
                using (var db = new AuthContext())
                {
                    Supplier supplier = db.Suppliers.Where(po => po.SupplierId == sid).SingleOrDefault();
                    return supplier;
                }
            }
            catch (Exception ee)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("GetSupp")]
        public List<Supplier> getsupp()
        {
            logger.Info("Get Buyer from people: ");
            try
            {
                using (var db = new AuthContext())
                {
                    List<Supplier> supplier = db.Suppliers.Where(po => po.Active == true).ToList();
                    return supplier;
                }
            }
            catch (Exception ee)
            {
                return null;
            }
        }

        #region Add depos  
        /// <summary> Tejas saraswat 20-05-2019
        /// to add depos with respect to supplierId to DepoMaster.cs
        /// </summary>    
        /// <param name="item"></param>
        /// <returns></returns>
        [Route("AddDepos")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Add(DepoMaster item)
        {
            logger.Info("start Add depos: ");
            try
            {
                using (var db = new AuthContext())
                {

                    DepoMaster dm = new DepoMaster();
                    var cityname = db.Cities.Where(x => x.Cityid == item.Cityid && x.Deleted == false).FirstOrDefault();
                    var statename = db.Cities.Where(x => x.Stateid == item.Stateid && x.Deleted == false).FirstOrDefault();
                    var TGrpName = db.DbTaxGroup.Where(x => x.GruopID == item.GruopID && x.Deleted == false).FirstOrDefault();
                    var suppliername = db.Suppliers.Where(x => x.SupplierId == item.SupplierId && x.Deleted == false).FirstOrDefault();
                    dm.SupplierCode = suppliername.SUPPLIERCODES;
                    dm.SupplierName = suppliername.Name;
                    dm.SupplierId = item.SupplierId;
                    dm.DepoName = item.DepoName;
                    dm.GSTin = item.GSTin;
                    dm.Address = item.Address;
                    dm.Email = item.Email;
                    dm.Phone = item.Phone;
                    dm.GruopID = item.GruopID;
                    dm.Stateid = item.Stateid;
                    dm.Cityid = item.Cityid;
                    dm.CityName = cityname.CityName;
                    dm.StateName = statename.StateName;
                    //--tejas
                    dm.FSSAI = item.FSSAI;
                    dm.CityPincode = item.CityPincode;
                    dm.Bank_Name = item.Bank_Name;
                    dm.Bank_AC_No = item.Bank_AC_No;
                    dm.BankAddress = item.BankAddress;
                    dm.Bank_Ifsc = item.Bank_Ifsc;
                    dm.BankPinCode = item.BankPinCode;
                    dm.PANCardNo = item.PANCardNo;
                    dm.OpeningHours = item.OpeningHours;
                    dm.GstImage = item.GstImage;
                    dm.PanCardImage = item.PanCardImage;
                    dm.FSSAIImage = item.FSSAIImage;
                    dm.CancelCheque = item.CancelCheque;
                    //--tejas
                    if (item.GSTin == null)
                    {
                        dm.IsActive = false;
                    }
                    else
                    {

                        dm.IsActive = item.IsActive;
                    }

                    dm.CreatedDate = DateTime.Now;
                    dm.ContactPerson = item.ContactPerson;
                    dm.DepoCodes = item.DepoCodes;
                    dm.PRPOStopAfterValue = item.PRPOStopAfterValue;
                    if (IsExistsDepo(dm.DepoCodes))
                    {
                        throw new Exception("This Depo is already assigned to this supplier");
                    }
                    db.DepoMasters.Add(dm);
                    db.Commit(doNotMakerChecker: false);

                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addDepo: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
                //return null;
            }
        }
        #endregion
        //#region View Depos
        ///// <summary>  tejas saraswat 20-05-2019
        ///// View Deposfor suppliers 
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("GetDepo")]
        //public List<DepoMaster> GetAllDepoForSupplier(int id)
        //{
        //    logger.Info("Get depo for Supplier: ");
        //    try
        //    {

        //        if (id != 0)
        //        {
        //            //int supplierid = Convert.ToInt32(id);
        //            var res = db.DepoMasters.Where(po => po.SupplierId == id).ToList();
        //            return res;
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }
        //    catch (Exception ee)
        //    {
        //        return null;
        //    }

        //}
        //#endregion

        #region Get  Depo in supplierId by Anushka
        /// <summary>
        /// Get Depo data 24-05-2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetDepo")]
        public List<DepoMaster> GetDepo(int id)
        {
            logger.Info("start Depo: ");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            int Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            int SupplierId = Convert.ToInt32(id);
            //int CompanyId = compid;
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            if (id != 0)
            {
                using (var db = new AuthContext())
                {
                    var depos = db.DepoMasters.Where(x => x.SupplierId == SupplierId).ToList();
                    string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    foreach (var item in depos)
                    {
                        item.GstImage = baseUrl + "" + item.GstImage;
                        item.FSSAIImage = baseUrl + "" + item.FSSAIImage;
                        item.PanCardImage = baseUrl + "" + item.PanCardImage;
                        item.CancelCheque = baseUrl + "" + item.CancelCheque;
                    }
                    logger.Info("End  Depo: ");
                    return depos;
                }
            }
            else
            {
                return null;
            }
        }

        [Authorize]
        [Route("GetDepoForPR")] // shailesh 
        public List<DepoMaster> GetDepoForPR(int id)
        {
            logger.Info("start Depo: ");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            int Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            int SupplierId = Convert.ToInt32(id);
            //int CompanyId = compid;
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            if (id != 0)
            {
                using (var db = new AuthContext())
                {
                    var depos = db.DepoMasters.Where(x => x.SupplierId == SupplierId && x.IsActive == true).ToList();
                    string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    foreach (var item in depos)
                    {
                        item.GstImage = baseUrl + "" + item.GstImage;
                        item.FSSAIImage = baseUrl + "" + item.FSSAIImage;
                        item.PanCardImage = baseUrl + "" + item.PanCardImage;
                        item.CancelCheque = baseUrl + "" + item.CancelCheque;
                    }
                    logger.Info("End  Depo: ");
                    return depos;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion
        #region Get Depo in depo Id by Anushka
        /// <summary>
        /// Get Depo data 24-05-2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetDepoData")]
        public DepoMaster GetDepoData(int id)
        {
            logger.Info("start Depo: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                int DepoId = Convert.ToInt32(id);
                //int CompanyId = compid;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                if (id != 0)
                {
                    using (var db = new AuthContext())
                    {
                        var depos = db.DepoMasters.Where(x => x.DepoId == DepoId).FirstOrDefault();
                        logger.Info("End  Depo: ");
                        return depos;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Depo" + ex.Message);
                logger.Info("End  Depo: ");
                return null;
            }

        }
        #endregion
        #region Update Supplier Depo by Anushka
        /// <summary>
        /// Created By 24-05-2019
        /// update supplier Depo 
        /// </summary>
        /// <param name="Depo"></param>
        /// <returns></returns>
        [ResponseType(typeof(DepoMaster))]
        [Route("PutDepo")]
        [AcceptVerbs("PUT")]
        public DepoMaster Put(DepoMaster EditDepo)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                EditDepo.CompanyId = compid;
                EditDepo.WarehouseId = Warehouse_id;
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    return context.PutDepos(EditDepo);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteDepo " + ex.Message);
                return null;
            }
        }

        #endregion
        #region Activate and Deactivated for code Depo
        /// <summary>
        /// supplier Activate and Deactivated  code 
        /// CreateDate 01/03/2019 
        /// </summary>
        /// <param name="Depo"></param>
        /// <returns></returns>
        [ResponseType(typeof(DepoMaster))]
        [Route("ActivateDepo")]
        [AcceptVerbs("PUT")]
        public DepoMaster Actsupp(DepoMaster Depo)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (var db = new AuthContext())
                {
                    DepoMaster act = db.DepoMasters.Where(x => x.DepoId == Depo.DepoId && x.Deleted == false).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsActive = Depo.IsActive;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    return Depo;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }
        #endregion
        #region  Export Supplier Nullable Fileds
        /// <summary>
        /// SB1-T270
        /// Export Supplier Nullable Fileds
        /// By Ashwin
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("Getmissingsupplier")]
        public IEnumerable<Supplier> getsupplierMissingDetailNull()
        {
            logger.Info("start Supplier: ");
            List<Supplier> ass = new List<Supplier>();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var list = db.Suppliers.Where(p => p.Name == null || p.CategoryName == null || p.PhoneNumber == null || p.Avaiabletime == 0 || p.BillingAddress == null || p.ShippingAddress == null || p.TINNo == null || p.OfficePhone == null || p.EmailId == null || p.SalesManager == null || p.ContactPerson == null).ToList();
                    logger.Info("End  Supplier: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Supplier " + ex.Message);
                    logger.Info("End  Supplier: ");
                    return null;
                }
            }
        }
        #endregion
        #region Get All  Depo in supplierId by Anushka
        /// <summary>
        /// Get Depo data 31-07-2019
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("GetAllDepo")]
        public List<DepoMaster> GetAllDepo()
        {
            logger.Info("start Depo: ");
            try
            {
                using (var db = new AuthContext())
                {
                    logger.Info("User ID : {0} , Company Id : {1}");
                    var depos = db.DepoMasters.Where(x => x.Deleted == false).ToList();
                    logger.Info("End  Depo: ");
                    return depos;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Depo" + ex.Message);
                logger.Info("End  Depo: ");
                return null;
            }

        }
        #endregion



        //#region get data PO dashboard for supplier app 
        ///// <summary>
        ///// Created Date:14/08/2019
        ///// Crreated By Tejas
        ///// </summary>
        ///// <returns></returns>
        //[Route("poSupplierDashboard")]
        //[HttpGet]
        //[AllowAnonymous]
        //public HttpResponseMessage poSupplierDashboard(DateTime? start, DateTime? end, /*int? wid,*/ int? SupplierId) //get search orders for delivery
        //{
        //    resobjdashbord resobj;
        //    logger.Info("start ItemMaster: ");
        //    try
        //    {

        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;
        //        double TotalTATTime = 0;
        //        double TotalFillRate = 0;
        //        long TotalManualPO = 0;
        //        long TotalAutoPO = 0;
        //        long TotalSKVehicle = 0;
        //        long TotalSupplierVehicle = 0;
        //        double count = 0;
        //        int TotalGRN = 0;

        //        using (var db = new AuthContext())
        //        {
        //            List<PurchaseOrderDetailRecived> newdata = new List<PurchaseOrderDetailRecived>();
        //            PODashboardDTO dashboard = new PODashboardDTO();
        //            if (SupplierId != null)
        //            {
        //                newdata = db.PurchaseOrderRecivedDetails.Where(x => /*x.WarehouseId == wid &&*/ x.CreationDate > start && x.CreationDate <= end && x.SupplierId == SupplierId).OrderByDescending(x => x.PurchaseOrderId).ToList();
        //                count = newdata.Count();
        //            }
        //            else
        //            {
        //                newdata = db.PurchaseOrderRecivedDetails.Where(x => /*x.WarehouseId == wid &&*/ x.CreationDate > start && x.CreationDate <= end).OrderByDescending(x => x.PurchaseOrderId).ToList();
        //                count = newdata.Count();

        //            }
        //            DateTime? RecivedDate;
        //            double? TotalRatioPo = 0;
        //            if (newdata.Count > 0)
        //            {


        //                #region GRN Beyond 
        //                List<PurchaseOrderMaster> podata = db.DPurchaseOrderMaster.Where(x => /*x.WarehouseId == wid &&*/ x.CreationDate > start && x.CreationDate <= end && (x.Status == "Received" || x.Status == "Partial Received")).ToList();
        //                foreach (var pocreateddate in podata)
        //                {
        //                    DateTime? CreatedDate = pocreateddate.CreationDate;
        //                    DateTime CD = DateTime.Now.AddHours(-48);
        //                    DateTime? ReceivedDate = null;
        //                    try
        //                    {
        //                        if (pocreateddate.Gr1_Date != null && pocreateddate.Gr2_Date == null)
        //                        {

        //                            ReceivedDate = pocreateddate.Gr1_Date;

        //                        }
        //                        else if (pocreateddate.Gr2_Date != null && pocreateddate.Gr3_Date == null)
        //                        {

        //                            ReceivedDate = pocreateddate.Gr2_Date;

        //                        }
        //                        else if (pocreateddate.Gr3_Date != null && pocreateddate.Gr4_Date == null)
        //                        {

        //                            ReceivedDate = pocreateddate.Gr3_Date;

        //                        }
        //                        else if (pocreateddate.Gr4_Date != null && pocreateddate.Gr5_Date == null)
        //                        {

        //                            ReceivedDate = pocreateddate.Gr4_Date;

        //                        }
        //                        else if (pocreateddate.Gr5_Date != null)
        //                        {

        //                            ReceivedDate = pocreateddate.Gr5_Date;

        //                        }
        //                        double hour = 0;
        //                        if (ReceivedDate != null)
        //                        {
        //                            String date = Convert.ToString(ReceivedDate - CreatedDate);
        //                            TimeSpan diff = TimeSpan.Parse(date);
        //                            hour = diff.TotalHours;
        //                        }
        //                        if (hour > 48)
        //                        {
        //                            double? ReceivedQty = 0;

        //                            double? TotalQty = 0;
        //                            double? ratioparticulerpo = 0;
        //                            List<PurchaseOrderDetailRecived> GRRecived = db.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == pocreateddate.PurchaseOrderId).ToList();
        //                            foreach (var ReQty in GRRecived)
        //                            {
        //                                ReceivedQty += ReQty.QtyRecived;
        //                                TotalQty += ReQty.TotalQuantity;
        //                                ratioparticulerpo += ReceivedQty / TotalQty;

        //                            }

        //                            TotalRatioPo += ratioparticulerpo;
        //                        }
        //                    }
        //                    catch (Exception Ex)
        //                    {

        //                    }
        //                }
        //                #endregion
        //                foreach (var ipO in newdata)
        //                {

        //                    var ItemQT = ipO.QtyRecived1 + ipO.QtyRecived2 + ipO.QtyRecived3 + ipO.QtyRecived4 + ipO.QtyRecived5;

        //                    double ItemFIlRate = Convert.ToDouble(ItemQT) * 100 / Convert.ToDouble(ipO.TotalQuantity);
        //                    TotalFillRate += ItemFIlRate;

        //                    var GRdata = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == ipO.PurchaseOrderId).FirstOrDefault();// add by raj,this is add because we required gr date



        //                    #region  Po to GR TAT  add by raj
        //                    try

        //                    {
        //                        string DiffDate = null;
        //                        if (GRdata != null)
        //                        {
        //                            if (GRdata.Gr5_Date != null && GRdata.Status == "Received")
        //                            {
        //                                RecivedDate = GRdata.Gr5_Date;
        //                                DiffDate = Convert.ToString(GRdata.Gr5_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr4_Date != null && GRdata.Status == "Received")
        //                            {
        //                                RecivedDate = GRdata.Gr4_Date;
        //                                DiffDate = Convert.ToString(GRdata.Gr4_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr3_Date != null && GRdata.Status == "Received")
        //                            {
        //                                RecivedDate = GRdata.Gr3_Date;
        //                                DiffDate = Convert.ToString(GRdata.Gr3_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr2_Date != null && GRdata.Status == "Received")
        //                            {
        //                                RecivedDate = GRdata.Gr2_Date;
        //                                DiffDate = Convert.ToString(GRdata.Gr2_Date - GRdata.CreationDate);
        //                            }
        //                            else if (GRdata.Gr1_Date != null && GRdata.Status == "Received")
        //                            {
        //                                RecivedDate = GRdata.Gr1_Date;
        //                                DiffDate = Convert.ToString(GRdata.Gr1_Date - GRdata.CreationDate);
        //                            }
        //                            if (GRdata.Status == "Received")
        //                            {
        //                                TotalGRN++;
        //                                double pogrdifftime = TimeSpan.Parse(DiffDate).TotalMinutes;
        //                                TimeSpan timeSpan = TimeSpan.FromMinutes(pogrdifftime);
        //                                TotalTATTime += timeSpan.TotalMilliseconds;
        //                                //TotalTATTime += Convert.ToDouble(timeSpan);

        //                            }
        //                        }
        //                    }
        //                    catch (Exception Ex)
        //                    {

        //                    }

        //                    #endregion
        //                    #region Total Vehicle
        //                    try
        //                    {
        //                        if (GRdata.VehicleType1 == "Supplier Vehicle" || GRdata.VehicleType2 == "Supplier Vehicle" || GRdata.VehicleType3 == "Supplier Vehicle" || GRdata.VehicleType4 == "Supplier Vehicle" || GRdata.VehicleType5 == "Supplier Vehicle")
        //                        {
        //                            TotalSupplierVehicle++;
        //                        }
        //                        else if (GRdata.VehicleType1 == "SK Vehicle" || GRdata.VehicleType2 == "SK Vehicle" || GRdata.VehicleType3 == "SK Vehicle" || GRdata.VehicleType4 == "SK Vehicle" || GRdata.VehicleType5 == "SK Vehicle")
        //                        {
        //                            TotalSKVehicle++;
        //                        }


        //                    }


        //                    catch (Exception ex)
        //                    {

        //                    }
        //                    #endregion
        //                }


        //                try
        //                {
        //                    var totalPO = db.DPurchaseOrderMaster.Where(x => x.SupplierId == SupplierId).ToList();
        //                    foreach (var a in totalPO)
        //                    {

        //                        #region Manual po And Automatic PO
        //                        if (a.PoType == "Manual")
        //                        {
        //                            TotalManualPO++;
        //                        }
        //                        else if (a.PoType == "Automated")
        //                        {
        //                            TotalAutoPO++;
        //                        }
        //                        #endregion

        //                    }

        //                }
        //                catch { }

        //                double minute = (TotalTATTime / 60000);
        //                var hours = Math.Round(Math.Floor(minute / 60) / TotalGRN);
        //                int tt = Convert.ToInt16(hours / TotalGRN);
        //                var minutes = Math.Round((minute - (hours * 60)) / TotalGRN, 0);
        //                var TotalPOGRhm = String.Format("{0:%h}", hours.ToString()) + ":" + String.Format("{0:%m}", minutes.ToString());

        //                // var totaltat = TotalPOGRhm / TotalGRN;

        //                dashboard.TotalFillRatePO = Math.Round((TotalFillRate / count), 2);
        //                dashboard.TotalTATPO = TotalPOGRhm;
        //                dashboard.TotalManualPO = TotalManualPO;
        //                dashboard.TotalAutoPO = TotalAutoPO;
        //                dashboard.TotalSKVehicle = TotalSKVehicle;
        //                dashboard.TotalSupplierVehicle = TotalSupplierVehicle;
        //                dashboard.GRNBeyond48hr = Convert.ToInt16(TotalRatioPo);

        //            }
        //            else
        //            {
        //                resobj = new resobjdashbord
        //                {
        //                    Result = null,
        //                    Status = false,
        //                    Message = "Faild."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, resobj);
        //            }
        //            resobj = new resobjdashbord
        //            {
        //                Result = dashboard,
        //                Status = true,
        //                Message = "Success."
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, resobj);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in ItemMaster " + ex.Message);
        //        logger.Info("End  ItemMaster: ");
        //        return null;
        //    }
        //}
        //#endregion

        #region SupplierAppContactUs
        [AllowAnonymous]
        [HttpGet]
        [Route("ContactUs")]
        public HttpResponseMessage ContactUs()
        {
            try
            {
                var responce = new ContactUs();
                using (var db = new AuthContext())
                {
                    //var res = db.PoApprovalDB.Where(p => p.Warehouseid == WarehouseId && p.Level == "Level1").Select(x => x.Approval1).FirstOrDefault();
                    //var people = db.Peoples.Where(x => x.PeopleID == res).Select(x => new { x.Mobile, x.Email, x.DisplayName }).FirstOrDefault();
                    //responce.WarehouseHeadName = people.DisplayName;
                    //responce.WarehouseHeadContact = people.Mobile;
                    //responce.WarehouseHeadEmailId = people.Email;
                    responce.AccountsHeadName = "";
                    responce.AccountsHeadContact = "8319938830";
                    responce.AccountsHeadEmailId = "";
                    responce.CentralHeadName = "Bhavik Parikh";
                    responce.CentralHeadEmailId = "bhavik.parikh@shopkirana.com";
                    var response = new
                    {
                        Status = true,
                        ContactUs = responce
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }
            }
            catch (Exception ee)
            {
                var response = new
                {
                    Status = false,
                    Message = ee
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion

        #region GetSuppliercityWiseAndbrand
        [Authorize]
        [Route("GetSuppliercityWiseAndbrand")]
        [HttpGet]
        public dynamic GetSuppliercityWiseAndbrand(int cityid, int? SubCaegoryId)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    if (SubCaegoryId == null)
                    {
                        var GetSuppCitywise = db.Suppliers.Where(x => x.Cityid == cityid && x.Active == true).ToList();
                        return GetSuppCitywise;
                    }
                    else
                    {
                        string sqlquery = "SELECT * FROM Suppliers LEFT JOIN SupplierBrandMaps"
                                    + " ON suppliers.SupplierId = SupplierBrandMaps.SupplierId where"
                                    + " Suppliers.Cityid = " + cityid + "  and  Suppliers.Active = 1 and  "
                                    + "SupplierBrandMaps.BrandId = " + SubCaegoryId + "  and SupplierBrandMaps.Active = 1  "
                                    + " and SupplierBrandMaps.Deleted = 0 ";

                        List<Supplier> Supplier = db.Database.SqlQuery<Supplier>(sqlquery).ToList();
                        return Supplier;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier " + ex.Message);
                logger.Info("End  Supplier: ");
                return null;
            }
        }
        #endregion

        #region GetSuppliercityWiseAndbrandV1
        [Authorize]
        [Route("GetSuppliercityWiseAndbrandV1")]
        [HttpGet]
        public dynamic GetSuppliercityWiseAndbrandV1(int cityid, int? SubCaegoryId)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    if (SubCaegoryId == null)
                    {
                        db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                        var GetBrand = (from c in db.Suppliers.Where(a => a.Deleted == false && a.Cityid == cityid)
                                        join p in db.SupplierBrandMaps.Where(x => x.Active == true && x.Deleted == false)
                                        on c.SupplierId equals p.SupplierId into ps
                                        from p in ps.DefaultIfEmpty()
                                        join f in db.SubCategorys
                                        on p.BrandId equals f.SubCategoryId into pf
                                        from f in pf.DefaultIfEmpty()
                                        select new SupplierBrandsVM
                                        {
                                            SubcategoryName = f.SubcategoryName,
                                            categorynameS = f.CategoryName,
                                            SupplierId = c.SupplierId,
                                            Name = c.Name,
                                            SUPPLIERCODES = c.SUPPLIERCODES,
                                            City = c.City,
                                            Cityid = c.Cityid,
                                            MobileNo = c.MobileNo,
                                            CreatedDate = c.CreatedDate,
                                            EmailId = c.EmailId,
                                            Active = c.Active,
                                            Amount = c.Amount,
                                            Avaiabletime = c.Avaiabletime,
                                            BankPINno = c.BankPINno,
                                            Bank_AC_No = c.Bank_AC_No,
                                            Bank_Ifsc = c.Bank_Ifsc,
                                            Bank_Name = c.Bank_Name,
                                            BillingAddress = c.BillingAddress,
                                            Brand = c.Brand,
                                            businessImageUrl = c.businessImageUrl,
                                            bussinessType = c.bussinessType,
                                            CategoryName = c.CategoryName,
                                            ChequeImageUrl = c.ChequeImageUrl,
                                            CityPincode = c.CityPincode,
                                            Comments = c.Comments,
                                            CompanyId = c.CompanyId,
                                            ContactImage = c.ContactImage,
                                            ContactPerson = c.ContactPerson,
                                            Deleted = c.Deleted,
                                            DepoId = c.DepoId,
                                            DepoName = c.DepoName,
                                            Description = c.Description,
                                            DeviceId = c.DeviceId,
                                            EstablishmentYear = c.EstablishmentYear,
                                            fcmId = c.fcmId,
                                            FSSAI = c.FSSAI,
                                            GstInNumber = c.GstInNumber,
                                            HeadOffice = c.HeadOffice,
                                            ImageUrl = c.ImageUrl,
                                            IsCityVerified = c.IsCityVerified,
                                            IsVerified = c.IsVerified,
                                            ManageAddress = c.ManageAddress,
                                            OfficePhone = c.OfficePhone,
                                            OpeningHours = c.OpeningHours,
                                            OwnerName = c.OwnerName,
                                            Pancard = c.Pancard,
                                            Password = c.Password,
                                            PaymentTerms = c.PaymentTerms,
                                            PeopleID = c.PeopleID,
                                            PhoneNumber = c.PhoneNumber,
                                            Pincode = c.Pincode,
                                            rating = c.rating,
                                            SalesManager = c.SalesManager,
                                            ShippingAddress = c.ShippingAddress,
                                            ShopName = c.ShopName,
                                            StartedBusiness = c.StartedBusiness,
                                            Stateid = c.Stateid,
                                            StateName = c.StateName,
                                            SupplierAddress = c.SupplierAddress,
                                            SupplierCaegoryId = c.SupplierCaegoryId,
                                            TINNo = c.TINNo,
                                            UpdatedDate = c.UpdatedDate,
                                            WarehouseId = c.WarehouseId,
                                            WarehouseName = c.WarehouseName,
                                            WebUrl = c.WebUrl,
                                        });


                        var list = GetBrand.ToList();

                        var groupList = list.GroupBy(x => new
                        {
                            x.SupplierId,
                            x.Name,
                            x.SUPPLIERCODES,
                            x.City,
                            x.Cityid,
                            x.MobileNo,
                            x.CreatedDate,
                            x.EmailId,
                            x.Active,
                            x.Avaiabletime,
                            x.BankPINno,
                            x.Bank_AC_No,
                            x.Bank_Ifsc,
                            x.Bank_Name,
                            x.BillingAddress,
                            x.Brand,
                            x.businessImageUrl,
                            x.bussinessType,
                            x.CategoryName,
                            x.ChequeImageUrl,
                            x.CityPincode,
                            x.Comments,
                            x.CompanyId,
                            x.ContactImage,
                            x.ContactPerson,
                            x.Deleted,
                            x.DepoId,
                            x.DepoName,
                            x.Description,
                            x.DeviceId,
                            x.EstablishmentYear,
                            x.fcmId,
                            x.FSSAI,
                            x.GstInNumber,
                            x.HeadOffice,
                            x.ImageUrl,
                            x.IsCityVerified,
                            x.IsVerified,
                            x.ManageAddress,
                            x.OfficePhone,
                            x.OpeningHours,
                            x.OwnerName,
                            x.Pancard,
                            x.Password,
                            x.PaymentTerms,
                            x.PeopleID,
                            x.PhoneNumber,
                            x.Pincode,
                            x.rating,
                            x.SalesManager,
                            x.ShippingAddress,
                            x.ShopName,
                            x.StartedBusiness,
                            x.Stateid,
                            x.StateName,
                            x.SupplierAddress,
                            x.SupplierCaegoryId,
                            x.TINNo,
                            x.UpdatedDate,
                            x.WarehouseId,
                            x.WarehouseName,
                            x.WebUrl


                        }).ToList();

                        List<SupplierBrandsVM> lst = new List<SupplierBrandsVM>();
                        foreach (var group in groupList)
                        {
                            var itm = group.First();
                            string brandName = "";
                            foreach (var item in group)
                            {
                                if (item.SubcategoryName != null)
                                {
                                    if (string.IsNullOrEmpty(brandName))
                                    {
                                        brandName = item.SubcategoryName;
                                    }
                                    else
                                    {
                                        brandName = brandName + "  " + ", " + " " + item.SubcategoryName;
                                    }

                                }
                            }
                            itm.SubcategoryName = brandName;
                            lst.Add(itm);
                        }
                        return lst.OrderByDescending(x => x.CreatedDate);
                    }
                    else
                    {
                        db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                        var GetBrand = (from c in db.Suppliers.Where(a => a.Deleted == false && a.Cityid == cityid)
                                        join p in db.SupplierBrandMaps.Where(x => x.BrandId == SubCaegoryId && x.Active == true && x.Deleted == false)
                                        on c.SupplierId equals p.SupplierId into ps
                                        from p in ps.ToList()
                                        join f in db.SubCategorys.Where(x => x.SubCategoryId == SubCaegoryId)
                                        on p.BrandId equals f.SubCategoryId into pf
                                        from f in pf.ToList()
                                        select new SupplierBrandsVM
                                        {
                                            SubcategoryName = f.SubcategoryName,
                                            categorynameS = f.CategoryName,
                                            SupplierId = c.SupplierId,
                                            Name = c.Name,
                                            SUPPLIERCODES = c.SUPPLIERCODES,
                                            City = c.City,
                                            Cityid = c.Cityid,
                                            MobileNo = c.MobileNo,
                                            CreatedDate = c.CreatedDate,
                                            EmailId = c.EmailId,
                                            Active = c.Active,
                                            Amount = c.Amount,
                                            Avaiabletime = c.Avaiabletime,
                                            BankPINno = c.BankPINno,
                                            Bank_AC_No = c.Bank_AC_No,
                                            Bank_Ifsc = c.Bank_Ifsc,
                                            Bank_Name = c.Bank_Name,
                                            BillingAddress = c.BillingAddress,
                                            Brand = c.Brand,
                                            businessImageUrl = c.businessImageUrl,
                                            bussinessType = c.bussinessType,
                                            CategoryName = c.CategoryName,
                                            ChequeImageUrl = c.ChequeImageUrl,
                                            CityPincode = c.CityPincode,
                                            Comments = c.Comments,
                                            CompanyId = c.CompanyId,
                                            ContactImage = c.ContactImage,
                                            ContactPerson = c.ContactPerson,
                                            Deleted = c.Deleted,
                                            DepoId = c.DepoId,
                                            DepoName = c.DepoName,
                                            Description = c.Description,
                                            DeviceId = c.DeviceId,
                                            EstablishmentYear = c.EstablishmentYear,
                                            fcmId = c.fcmId,
                                            FSSAI = c.FSSAI,
                                            GstInNumber = c.GstInNumber,
                                            HeadOffice = c.HeadOffice,
                                            ImageUrl = c.ImageUrl,
                                            IsCityVerified = c.IsCityVerified,
                                            IsVerified = c.IsVerified,
                                            ManageAddress = c.ManageAddress,
                                            OfficePhone = c.OfficePhone,
                                            OpeningHours = c.OpeningHours,
                                            OwnerName = c.OwnerName,
                                            Pancard = c.Pancard,
                                            Password = c.Password,
                                            PaymentTerms = c.PaymentTerms,
                                            PeopleID = c.PeopleID,
                                            PhoneNumber = c.PhoneNumber,
                                            Pincode = c.Pincode,
                                            rating = c.rating,
                                            SalesManager = c.SalesManager,
                                            ShippingAddress = c.ShippingAddress,
                                            ShopName = c.ShopName,
                                            StartedBusiness = c.StartedBusiness,
                                            Stateid = c.Stateid,
                                            StateName = c.StateName,
                                            SupplierAddress = c.SupplierAddress,
                                            SupplierCaegoryId = c.SupplierCaegoryId,
                                            TINNo = c.TINNo,
                                            UpdatedDate = c.UpdatedDate,
                                            WarehouseId = c.WarehouseId,
                                            WarehouseName = c.WarehouseName,
                                            WebUrl = c.WebUrl,
                                        });
                        var list = GetBrand.ToList();
                        var groupList = list.GroupBy(x => new
                        {
                            x.SupplierId,
                            x.Name,
                            x.SUPPLIERCODES,
                            x.City,
                            x.Cityid,
                            x.MobileNo,
                            x.CreatedDate,
                            x.EmailId,
                            x.Active,
                            x.Avaiabletime,
                            x.BankPINno,
                            x.Bank_AC_No,
                            x.Bank_Ifsc,
                            x.Bank_Name,
                            x.BillingAddress,
                            x.Brand,
                            x.businessImageUrl,
                            x.bussinessType,
                            x.CategoryName,
                            x.ChequeImageUrl,
                            x.CityPincode,
                            x.Comments,
                            x.CompanyId,
                            x.ContactImage,
                            x.ContactPerson,
                            x.Deleted,
                            x.DepoId,
                            x.DepoName,
                            x.Description,
                            x.DeviceId,
                            x.EstablishmentYear,
                            x.fcmId,
                            x.FSSAI,
                            x.GstInNumber,
                            x.HeadOffice,
                            x.ImageUrl,
                            x.IsCityVerified,
                            x.IsVerified,
                            x.ManageAddress,
                            x.OfficePhone,
                            x.OpeningHours,
                            x.OwnerName,
                            x.Pancard,
                            x.Password,
                            x.PaymentTerms,
                            x.PeopleID,
                            x.PhoneNumber,
                            x.Pincode,
                            x.rating,
                            x.SalesManager,
                            x.ShippingAddress,
                            x.ShopName,
                            x.StartedBusiness,
                            x.Stateid,
                            x.StateName,
                            x.SupplierAddress,
                            x.SupplierCaegoryId,
                            x.TINNo,
                            x.UpdatedDate,
                            x.WarehouseId,
                            x.WarehouseName,
                            x.WebUrl
                        }).ToList();
                        List<SupplierBrandsVM> lst = new List<SupplierBrandsVM>();
                        foreach (var group in groupList)
                        {
                            var itm = group.First();
                            string brandName = "";
                            foreach (var item in group)
                            {
                                if (item.SubcategoryName != null)
                                {
                                    if (string.IsNullOrEmpty(brandName))
                                    {
                                        brandName = item.SubcategoryName;
                                    }
                                    else
                                    {
                                        brandName = brandName + "  " + ", " + " " + item.SubcategoryName;
                                    }

                                }
                            }
                            itm.SubcategoryName = brandName;
                            lst.Add(itm);
                        }
                        return lst.OrderByDescending(x => x.CreatedDate);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Supplier " + ex.Message);
                logger.Info("End  Supplier: ");
                return null;
            }
        }
        #endregion



        #region get po id for supplier Id based
        /// <summary>
        /// created Date :16/09/2019
        /// Created by Raj
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetPoId")]
        [HttpGet]
        public async Task<List<int>> GetPoIddata(int supplierId)
        {
            using (var context = new AuthContext())
            {
                List<int> poids = new List<int>();
                var poid = context.DPurchaseOrderMaster.Where(a => a.SupplierId == supplierId && (a.Status == "pending" || a.Status == "Send for Approval" || a.Status == "Self Approved")).Select(x => x.PurchaseOrderId).ToList();
                if (poid != null)
                {
                    foreach (int podata in poid)
                    {
                        var supplierpayment = context.SupplierPaymentRequestDc.Where(x => x.POId == podata).FirstOrDefault();
                        if (supplierpayment == null)
                        {
                            poids.Add(podata);
                        }
                    }
                }
                return poids;
            }
        }

        #endregion

        /// <summary>
        /// Add Amount from Supplier
        /// Created By Vinayak.
        /// 30/08/2019
        /// </summary>
        /// <returns></returns>
        [Route("addSupplierPayment")]
        [AcceptVerbs("PUT")]
        public SupplierPaymentDC addSupplierPayment(SupplierPaymentDC addSupplierPayment)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                using (var context = new AuthContext())
                {
                    var DisplayName = context.Peoples.Where(x => x.PeopleID == userid).Select(x => x.DisplayName).FirstOrDefault();
                    var cityId = context.Suppliers.Where(x => x.SupplierId == addSupplierPayment.SupplierId).Select(x => x.Cityid).FirstOrDefault();
                    SupplierPaymentRequest supplierPaymentData = new SupplierPaymentRequest();

                    supplierPaymentData.SupplierId = addSupplierPayment.SupplierId;
                    supplierPaymentData.POId = addSupplierPayment.POId;
                    supplierPaymentData.Amount = addSupplierPayment.Amount;
                    supplierPaymentData.Cityid = cityId;
                    supplierPaymentData.CreatedDate = DateTime.Now;
                    supplierPaymentData.isDeleted = false;
                    supplierPaymentData.status = "Pending for Purchase";
                    supplierPaymentData.CreatedBy = DisplayName;
                    supplierPaymentData.userid = userid;
                    context.SupplierPaymentRequestDc.Add(supplierPaymentData);
                    context.Commit();

                    return addSupplierPayment;


                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Addsupplierpayments data " + ex.Message);
                return null;
            }
        }



        #region get Supplier payment Details
        /// <summary>
        /// Created Date 04/09/2019
        /// Cretaed By Raj
        /// </summary>
        /// <returns></returns>
        [Route("GetSupplierPaymentDetails")]
        [HttpGet]
        public dynamic GetSupplierPaymentDetails()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {

                    var supplierPaymentDetails = (from c in db.SupplierPaymentRequestDc
                                                  where c.status != "Reject"
                                                  join p in db.Suppliers.Where(x => x.Active == true && x.Deleted == false)
                                                  on c.SupplierId equals p.SupplierId into ps
                                                  from p in ps.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      paymentRequestid = c.paymentRequestid,
                                                      SupplierId = c.SupplierId,
                                                      SUPPLIERCODES = p.SUPPLIERCODES,
                                                      POId = c.POId,
                                                      Name = p.Name,
                                                      CityName = p.City,
                                                      Createdby = c.CreatedBy,
                                                      Amount = c.Amount,
                                                      status = c.status,
                                                      CreatedDate = c.CreatedDate,
                                                      Selected = p == null ? false : true
                                                  }).ToList();

                    return supplierPaymentDetails;

                }

            }
            catch (Exception ex)
            {
                logger.Info("Error  get supplierPaymentDetails: " + ex);
                return false;
            }
        }
        #endregion


        #region update status Supplier payment Details
        /// <summary>
        /// Created Date 05/09/2019
        /// Cretaed By Raj
        /// </summary>
        /// <returns></returns>
        [Route("approvedSupplierPaymentDetails")]
        [HttpPut]
        public HttpResponseMessage approvedSupplierPaymentDetails(SupplierPaymentRequest data)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;


                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var displayName = db.Peoples.Where(x => x.PeopleID == userid).Select(x => x.DisplayName).FirstOrDefault();
                    SupplierPaymentRequest spr = db.SupplierPaymentRequestDc.Where(x => x.SupplierId == data.SupplierId && x.paymentRequestid == data.paymentRequestid).FirstOrDefault();
                    spr.status = data.status;
                    spr.modifiedBy = displayName;
                    spr.UpdatedDate = DateTime.Now;
                    db.Entry(spr).State = EntityState.Modified;
                    db.Commit();
                    var response = new
                    {
                        Status = true,
                        ContactUs = spr
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }

            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion


        #region update payment Supplier payment Details
        /// <summary>
        /// Created Date 05/09/2019
        /// Cretaed By Raj
        /// </summary>
        /// <returns></returns>
        [Route("paySupplierPaymentDetails")]
        [HttpPut]
        public HttpResponseMessage paySupplierPaymentDetails(SupplierPaymentRequest data)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;


                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var displayName = db.Peoples.Where(x => x.PeopleID == userid).Select(x => x.DisplayName).FirstOrDefault();
                    SupplierPaymentRequest spr = db.SupplierPaymentRequestDc.Where(x => x.SupplierId == data.SupplierId && x.paymentRequestid == data.paymentRequestid).FirstOrDefault();
                    spr.status = data.status;
                    spr.modifiedBy = displayName;
                    spr.UpdatedDate = DateTime.Now;
                    db.Entry(spr).State = EntityState.Modified;

                    IRHelper.DebitLedgerEntry(spr.SupplierId, db, spr.Amount, "Advance amount", spr.paymentRequestid, userid, 0, "Advance amount");



                    var suppdata = db.FullSupplierPaymentDataDB.Where(x => x.SupplierId == data.SupplierId && x.Deleted == false).FirstOrDefault();
                    if (suppdata != null)
                    {
                        suppdata.InVoiceRemainingAmount = suppdata.InVoiceRemainingAmount - data.Amount;
                        if (suppdata.InVoiceRemainingAmount == 0)
                        {
                            suppdata.SupplierPaymentStatus = "Full Paid";

                        }
                        else
                        {
                            suppdata.SupplierPaymentStatus = "Partial Paid";

                        }
                        db.Entry(suppdata).State = EntityState.Modified;

                    }
                    db.Commit();
                    var response = new
                    {
                        Status = true,
                        ContactUs = spr
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }

            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion
        #region get Supplier outstanding amount
        /// <summary>
        /// Created Date 11/10/2019
        /// Cretaed By Raj
        /// </summary>
        /// <returns></returns>
        [Route("SupplierOutstandingamt")]
        [HttpGet]
        public dynamic SupplierOutstandingamt(int SupplierId, DateTime tilldate)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    double credit = 0, debit = 0, outstandingamt = 0;
                    DateTime todaydate = DateTime.Now;
                    tilldate = tilldate.AddHours(DateTime.Now.Hour);
                    tilldate = tilldate.AddMinutes(DateTime.Now.Minute);
                    tilldate = tilldate.AddSeconds(DateTime.Now.Second);

                    long Id = db.LadgerDB.Where(x => x.ObjectType == "Supplier" && x.ObjectID == SupplierId).Select(x => x.ID).FirstOrDefault();
                    if (Id > 0)
                    {
                        var ladgerentries = db.LadgerEntryDB.Where(x => x.LagerID == Id && x.ObjectType == "IR" && x.CreatedDate <= tilldate).ToList();
                        foreach (var data in ladgerentries)
                        {
                            credit += data.Credit ?? 0;
                            debit += data.Debit ?? 0;


                        }
                        outstandingamt = System.Math.Round((debit - credit), 2);


                    }
                    return outstandingamt;
                }
            }
            catch (Exception ex)
            {
                logger.Info("Error  get supplieroutstandingamount : " + ex);
                return null;
            }
        }
        #endregion
        #region get Supplier IR Data
        /// <summary>
        /// Created Date 11/10/2019
        /// Cretaed By Raj
        /// </summary>
        /// <returns></returns>
        [Route("SupplierIRList")]
        [HttpGet]
        public dynamic SupplierIRList(int SupplierId)
        {
            using (AuthContext db = new AuthContext())
            {
                var irdata = db.IRMasterDB.Where(x => x.supplierId == SupplierId && x.IRStatus == "Approved from Buyer side" && x.Deleted == false).ToList();
                return irdata;
            }

        }
        #endregion
        #region post Ir Payment details
        /// <summary>
        /// Created by Raj 
        /// Created Date:12/10/2019
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        [Route("PayemntIRDetails")]
        [HttpPost]
        public IRPaymentDetails PaymentIRDetails(IRPaymentDetails details)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            IRHelper irHelper = new IRHelper();
            IRPaymentDetails irdetails = irHelper.SupplierPaymentList(details, userid);

            return irdetails;

        }

        [Route("UploadPayemntList")]
        [HttpPost]
        public List<IRPaymentDetails> UploadPayemntList(List<IRPaymentDetails> paymentDetailsList)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            IRHelper irHelper = new IRHelper();

            List<IRPaymentDetails> irPaymentDetailsList = new List<IRPaymentDetails>();
            if (paymentDetailsList != null && paymentDetailsList.Count > 0)
            {
                foreach (var item in paymentDetailsList)
                {
                    IRPaymentDetails irPaymentDetails = irHelper.SupplierPaymentList(item, userid);
                    irPaymentDetailsList.Add(irPaymentDetails);
                }
            }
            return irPaymentDetailsList;

        }

        [Route("UploadPayemntListOnly")]
        [HttpPost]
        public List<IRPaymentDetails> UploadPayemntListOnly(List<IRPaymentDetails> paymentDetailsList)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            IRHelper irHelper = new IRHelper();

            List<IRPaymentDetails> irPaymentDetailsList = new List<IRPaymentDetails>();
            if (paymentDetailsList != null && paymentDetailsList.Count > 0)
            {
                foreach (var item in paymentDetailsList)
                {
                    IRPaymentDetails irPaymentDetails = irHelper.UploadIRPayment(item, userid);
                    irPaymentDetailsList.Add(irPaymentDetails);
                }
            }
            return irPaymentDetailsList;

        }

        #endregion

        /// <summary>
        /// It Will delete the ledger entries -- used when any error commit by developers
        /// </summary>
        /// <param name="irPaymentDetailID"></param>
        /// <returns></returns>
        [Route("ByDeveloperDeleteIRPaymentDetail/{irPaymentDetailID}")]
        [AllowAnonymous]
        [HttpPost]
        public bool ByDeveloperDeleteIRPaymentDetail(int irPaymentDetailID)
        {
            DateTime dateTime = DateTime.Now;
            int userid = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (var scope = new TransactionScope())
            {
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        long debitNoteVoucherTypeId = authContext.VoucherTypeDB.First(x => !string.IsNullOrEmpty(x.Name) && x.Name == "DebitNote").ID;
                        IRPaymentDetails irPaymentDetail = authContext.IRPaymentDetailsDB.FirstOrDefault(x => x.Id == irPaymentDetailID && x.IsActive == true && x.Deleted == false);
                        if (irPaymentDetail != null)
                        {
                            irPaymentDetail.IsActive = false;
                            irPaymentDetail.Deleted = true;
                            irPaymentDetail.Updateby = userid;
                            irPaymentDetail.UpdateDate = dateTime;
                            authContext.Commit();

                            List<LadgerEntry> ledgerEntryList
                                = authContext.LadgerEntryDB.Where(x => x.IrPaymentDetailsId == irPaymentDetailID).ToList();

                            if (ledgerEntryList != null && ledgerEntryList.Any())
                            {
                                List<LadgerEntry> debitIrEntryList = ledgerEntryList.Where(x => x.ObjectType == "IR" && x.ObjectID > 0).ToList();
                                if (debitIrEntryList != null && debitIrEntryList.Any())
                                {
                                    foreach (var item in debitIrEntryList)
                                    {
                                        IRMaster irMaster = authContext.IRMasterDB.FirstOrDefault(x => x.Id == item.ObjectID);
                                        irMaster.TotalAmountRemaining = irMaster.TotalAmountRemaining + (item.Debit.HasValue ? item.Debit.Value : 0);
                                        irMaster.IRStatus = "Approved from Buyer side";
                                        if (Math.Round(irMaster.TotalAmount) > Math.Round(irMaster.TotalAmountRemaining))
                                        {
                                            irMaster.PaymentStatus = "partial paid";
                                        }
                                        else
                                        {
                                            irMaster.PaymentStatus = "Unpaid";
                                        }
                                        authContext.Commit();
                                    }
                                }

                                authContext.LadgerEntryDB.RemoveRange(ledgerEntryList);
                            }


                            List<IRPaymentDetailHistory> historyList = authContext.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == irPaymentDetailID && x.IsActive == true && x.Deleted == false).ToList();
                            if (historyList != null && historyList.Any())
                            {
                                foreach (var history in historyList)
                                {
                                    history.IsActive = false;
                                    history.Deleted = true;
                                }
                            }

                        }
                        authContext.Commit();
                        //var parameters = new List<SqlParameter> { new SqlParameter("@IrPaymentDetailId", irPaymentDetailID) };
                        //authContext.Database.ExecuteSqlCommand("EXEC DeleteIrPaymentDetail @IrPaymentDetailId", parameters.ToArray());
                    }
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                }
            }

            return true;
        }



        /// <summary>
        /// It Will make reverse ledger entries -- used by UI when delete  payment
        /// </summary>
        /// <param name="irPaymentDetailID"></param>
        /// <returns></returns>
        [Route("DeleteIRPaymentDetail/{irPaymentDetailID}")]
        [HttpPost]
        public bool DeleteIRPaymentDetail(int irPaymentDetailID)
        {
            DateTime dateTime = DateTime.Now;
            int userid = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (var scope = new TransactionScope())
            {
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        long debitNoteVoucherTypeId = authContext.VoucherTypeDB.First(x => !string.IsNullOrEmpty(x.Name) && x.Name == "CreditNote").ID;
                        IRPaymentDetails irPaymentDetail = authContext.IRPaymentDetailsDB.FirstOrDefault(x => x.Id == irPaymentDetailID && x.IsActive == true && x.Deleted == false);
                        if (irPaymentDetail != null)
                        {
                            irPaymentDetail.IsActive = false;
                            irPaymentDetail.Deleted = true;
                            irPaymentDetail.Updateby = userid;
                            irPaymentDetail.UpdateDate = dateTime;
                            authContext.Commit();

                            List<LadgerEntry> ledgerEntryList
                                = authContext.LadgerEntryDB.Where(x => x.IrPaymentDetailsId == irPaymentDetailID).ToList();

                            if (ledgerEntryList != null && ledgerEntryList.Any())
                            {
                                List<LadgerEntry> debitIrEntryList = ledgerEntryList.Where(x => x.ObjectType == "IR" && x.ObjectID > 0).ToList();
                                if (debitIrEntryList != null && debitIrEntryList.Any())
                                {
                                    foreach (var item in debitIrEntryList)
                                    {
                                        IRMaster irMaster = authContext.IRMasterDB.FirstOrDefault(x => x.Id == item.ObjectID);
                                        irMaster.TotalAmountRemaining = irMaster.TotalAmountRemaining + (item.Debit.HasValue ? item.Debit.Value : 0);
                                        irMaster.IRStatus = "Approved from Buyer side";
                                        if (Math.Round(irMaster.TotalAmount) > Math.Round(irMaster.TotalAmountRemaining))
                                        {
                                            irMaster.PaymentStatus = "partial paid";
                                        }
                                        else
                                        {
                                            irMaster.PaymentStatus = "Unpaid";
                                        }
                                        authContext.Commit();
                                    }
                                }

                                foreach (var item in ledgerEntryList)
                                {
                                    double? debitAmount = item.Debit;
                                    authContext.Entry(item).State = EntityState.Detached;
                                    item.ID = 0;
                                    item.VouchersTypeID = debitNoteVoucherTypeId;
                                    item.Debit = item.Credit;
                                    item.Credit = debitAmount;
                                    item.Date = dateTime.Date;
                                    item.CreatedBy = userid;
                                    item.CreatedDate = dateTime;
                                }

                                authContext.LadgerEntryDB.AddRange(ledgerEntryList);
                            }


                            List<IRPaymentDetailHistory> historyList = authContext.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == irPaymentDetailID && x.IsActive == true && x.Deleted == false).ToList();
                            if (historyList != null && historyList.Any())
                            {
                                foreach (var history in historyList)
                                {
                                    history.IsActive = false;
                                    history.Deleted = true;
                                }
                            }

                        }
                        authContext.Commit();
                        //var parameters = new List<SqlParameter> { new SqlParameter("@IrPaymentDetailId", irPaymentDetailID) };
                        //authContext.Database.ExecuteSqlCommand("EXEC DeleteIrPaymentDetail @IrPaymentDetailId", parameters.ToArray());
                    }
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                }
            }

            return true;
        }


        [Route("DeleteIRPaymentDetailBySupplierId/{supplierId}")]
        [HttpPost]
        [AllowAnonymous]
        public bool DeleteIRPaymentDetailBySupplierId(int supplierId)
        {
            using (var authContext = new AuthContext())
            {
                string query = @"select * from IRPaymentDetails 
                                where supplierid = " + supplierId.ToString() + @" and IsActive = 1 and Deleted = 0 
                                and (IsIROutstandingPending IS NULL OR IsIROutstandingPending <> 1)
                                order by IRPaymentDetails.PaymentDate";

                List<IRPaymentDetails> detailList = authContext.Database.SqlQuery<IRPaymentDetails>(query).ToList();

                if (detailList != null && detailList.Any())
                {
                    foreach (IRPaymentDetails item in detailList)
                    {
                        var parameters = new List<SqlParameter> { new SqlParameter("@IrPaymentDetailId", item.Id) };
                        authContext.Database.ExecuteSqlCommand("EXEC DeleteIrPaymentDetail @IrPaymentDetailId", parameters.ToArray());
                    }
                }
            }
            return true;
        }


        [Route("MakeIRPayment")]
        [HttpPost]
        public bool MakeIRPayment(IrOutstandingPayment payment)
        {
            int userid = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            Guid guid = Guid.NewGuid();
            if (payment != null && payment.IrOutstandingList != null && payment.IrOutstandingList.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                //using (var scope = new TransactionScope())
                {
                    try
                    {
                        using (var context = new AuthContext())
                        {
                            IRPaymentSummary summary = new IRPaymentSummary()
                            {
                                Createby = userid,
                                Deleted = false,
                                IsActive = true,
                                PaymentDate = payment.PaymentDate,
                                TotalAmount = payment.IrOutstandingList.Sum(x => x.TotalAmount),
                                Updateby = null,
                                UpdateDate = null
                            };

                            context.IRPaymentSummaryDB.Add(summary);
                            context.Commit();

                            foreach (var item in payment.IrOutstandingList)
                            {
                                IRPaymentDetails detail = new IRPaymentDetails()
                                {
                                    BankId = payment.BankId,
                                    BankName = payment.BankName,
                                    Createby = userid,
                                    CreatedDate = currentTime,
                                    Deleted = false,
                                    Guid = guid.ToString(),
                                    IRList = JsonConvert.SerializeObject(item),
                                    IRPaymentSummaryId = summary.Id,
                                    IsActive = true,
                                    PaymentDate = summary.PaymentDate,
                                    RefNo = payment.RefNo,
                                    Remark = payment.Remark,
                                    SupplierId = item.SupplierId.Value,
                                    TotalAmount = (int)item.TotalAmount,
                                    TotalReaminingAmount = 0,
                                    WarehouseId = 0,
                                    IsIROutstandingPending = false,
                                    PaymentStatus = "Pending"
                                };
                                IRHelper helper = new IRHelper();
                                detail = helper.SupplierNewPayment(context, detail, guid, userid);

                                IRMasterDTO ir = context.IRMasterDB.Where(x => x.Id == item.Id).Select(x => new IRMasterDTO
                                {
                                    BuyerName = x.BuyerName,
                                    CreatedBy = x.CreatedBy,
                                    CreationDate = x.CreationDate,
                                    Discount = x.Discount,
                                    Gstamt = x.Gstamt,
                                    IRAmountWithOutTax = x.IRAmountWithOutTax,
                                    IRAmountWithTax = x.IRAmountWithTax,
                                    IRID = x.IRID,
                                    IRStatus = "Paid",
                                    IRType = x.IRType,
                                    PaymentStatus = "paid",
                                    PaymentTerms = x.PaymentTerms,
                                    PurchaseOrderId = x.PurchaseOrderId,
                                    ReamainingAmt = 0,
                                    TotalTaxPercentage = x.TotalTaxPercentage,
                                    TotalAmountRemaining = item.TotalAmount.Value,
                                    RejectedComment = x.RejectedComment,
                                    supplierId = x.supplierId,
                                    SupplierName = x.SupplierName,
                                    TotalAmount = x.TotalAmount,
                                    WarehouseId = x.WarehouseId,
                                    WarehouseName = "",

                                }).FirstOrDefault();

                                List<IRMasterDTO> list = new List<IRMasterDTO>();
                                list.Add(ir);
                                helper.MakeIrLedgerEntry(list, context, userid, detail, detail.Id);
                            }
                        }

                        scope.Complete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw ex;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        [Authorize]
        [Route("MakeIRPaymentRequest")]
        [HttpPost]
        public bool MakeIRPaymentRequest(IrOutstandingPayment payment)
        {
            int userid = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            Guid guid = Guid.NewGuid();
            if (payment != null && payment.IrOutstandingList != null && payment.IrOutstandingList.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                // using (var scope = new TransactionScope())
                {
                    try
                    {
                        List<SupplierPaymentssDC> paymentt = new List<SupplierPaymentssDC>();
                        using (var context = new AuthContext())
                        {

                            IRPaymentSummary summary = new IRPaymentSummary()
                            {
                                Createby = userid,
                                Deleted = false,
                                IsActive = true,
                                PaymentDate = payment.PaymentDate,
                                TotalAmount = payment.IrOutstandingList.Sum(x => x.PaidAmount),
                                Updateby = null,
                                UpdateDate = null,
                                IsIROutstandingPending = true
                            };

                            context.IRPaymentSummaryDB.Add(summary);
                            context.Commit();

                            //foreach (int s in payment.IrOutstandingList.Select(x => x.SupplierId).Distinct())
                            //{
                            //    SupplierPaymentssDC obj = new SupplierPaymentssDC();
                            //    var param = new SqlParameter
                            //    {
                            //        ParameterName = "Supplierid",
                            //        Value = s
                            //    };
                            //    var data = context.Database.SqlQuery<double>("exec SP_GetTotalValuebySupplierId @Supplierid", param).FirstOrDefault();
                            //    obj.SupplierId = s;
                            //    obj.PaymentTillDate = data;
                            //    paymentt.Add(obj);
                            //}


                            foreach (var item in payment.IrOutstandingList)
                            {
                                var irtotalamt = context.IRMasterDB.Where(x => x.Id == item.Id && x.Deleted == false).FirstOrDefault();
                                var irpayment = context.IRPaymentDetailsDB.Where(z => z.IRMasterId == item.Id && z.PaymentStatus == "Approved").ToList();
                                var IRMasterId = new SqlParameter("@IRMasterId", item.Id);
                                double DNAmount = context.Database.SqlQuery<double>("GetDebitNoteAmount @IRMasterId", IRMasterId).FirstOrDefault();
                                double ActualAmt = irtotalamt.TotalAmount - DNAmount;
                                var finaltotal = ActualAmt - ((irpayment != null && irpayment.Any()) ? irpayment.Sum(a => a.TotalAmount + a.TDSAmount) : 0);
                                //finaltotal = finaltotal < 0 ? 0 : finaltotal;
                                var totalremaingamt = Math.Round(finaltotal) - (item.PaidAmount + item.SettledAmount);

                                #region tds percentage 
                                double tdsper = 0; double tds = 0;
                                string TDSdeducationDate = ConfigurationManager.AppSettings["TDSdeducationDate"];
                                DateTime CompairDate = Convert.ToDateTime(TDSdeducationDate, new CultureInfo("en-GB"));
                                DateTime InvoiceDate = Convert.ToDateTime(irtotalamt.InvoiceDate, new CultureInfo("en-GB"));
                                if (InvoiceDate >= CompairDate)
                                {

                                    var irdata = context.IRMasterDB.Where(x => x.supplierId == item.SupplierId && x.Id == item.Id).FirstOrDefault();
                                    string prpaymenttype = context.DPurchaseOrderMaster.FirstOrDefault(x => x.PurchaseOrderId == item.PurchaseOrderId).PRPaymentType;
                                    if (irdata != null && prpaymenttype != "AdvancePR")
                                    {
                                        tds = irdata.TotalRemainingTDSAmount;
                                    }
                                    else
                                    {
                                        tds = item.TDSAmount;
                                    }





                                    //var suppliergst = context.Suppliers.Where(x => x.Active == true && x.SupplierId == item.SupplierId).FirstOrDefault();
                                    //if (suppliergst != null)
                                    //{
                                    //    var irdata = context.IRMasterDB.Where(x => x.supplierId == item.SupplierId && x.Id == item.Id).FirstOrDefault();
                                    //    if (!string.IsNullOrEmpty(suppliergst.Pancard) || !string.IsNullOrEmpty(suppliergst.TINNo))
                                    //    {
                                    //        var supplierPayment = paymentt.Where(x => x.SupplierId == item.SupplierId).First();

                                    //        if (supplierPayment.PaymentTillDate > 5000000)
                                    //        {
                                    //            tdsper = 0.1;
                                    //            tds = item.PaidAmount * tdsper / 100;
                                    //            supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;

                                    //            irdata.TotalTDSAmount = irdata.IRAmountWithOutTax * 0.1 / 100;
                                    //        }
                                    //        else
                                    //        {
                                    //            if (supplierPayment.PaymentTillDate + item.PaidAmount > 5000000)
                                    //            {
                                    //                tdsper = 0.1;
                                    //                tds = item.PaidAmount * tdsper / 100;
                                    //                supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;

                                    //                irdata.TotalTDSAmount = irdata.IRAmountWithOutTax * tdsper / 100;
                                    //            }
                                    //            else
                                    //            {
                                    //                tdsper = 0;
                                    //                tds = item.PaidAmount * tdsper / 100;
                                    //                supplierPayment.PaymentTillDate = supplierPayment.PaymentTillDate + item.PaidAmount;
                                    //            }
                                    //        }
                                    //        //tdsper = 0.1;
                                    //    }
                                    //    else
                                    //    {
                                    //        tdsper = 5;
                                    //        tds = item.PaidAmount *tdsper / 100;
                                    //    }


                                    //    context.Entry(irdata).State = EntityState.Modified;
                                    //}

                                    // tds = Math.Round(item.PaidAmount * tdsper / 100);
                                    irdata.TotalRemainingTDSAmount = irdata.TotalRemainingTDSAmount - tds;
                                    context.Entry(irdata).State = EntityState.Modified;
                                    context.Commit();
                                    item.PaidAmount = item.PaidAmount - tds;
                                    totalremaingamt = totalremaingamt - tds;
                                }
                                #endregion




                                IRPaymentDetails detail = new IRPaymentDetails()
                                {
                                    BankId = payment.BankId,
                                    BankName = payment.BankName,
                                    Createby = userid,
                                    CreatedDate = currentTime,
                                    Deleted = false,
                                    Guid = guid.ToString(),
                                    IRList = JsonConvert.SerializeObject(item),
                                    IRPaymentSummaryId = summary.Id,
                                    IsActive = true,
                                    PaymentDate = summary.PaymentDate,
                                    RefNo = payment.RefNo,
                                    Remark = payment.Remark,
                                    SupplierId = item.SupplierId.Value,
                                    TotalAmount = (int)item.PaidAmount,
                                    TotalReaminingAmount = totalremaingamt <= 0 ? 0 : totalremaingamt,
                                    WarehouseId = item.WarehouseId,
                                    IsIROutstandingPending = true,
                                    PaymentStatus = "Pending",
                                    IRMasterId = item.Id,
                                    TDSAmount = tds
                                };

                                if (item.SettledAmount > 0)
                                {
                                    PurchaseRequestSettlementHelper purchaseRequestSettlementHelper = new PurchaseRequestSettlementHelper();
                                    IRMaster iRMaster = context.IRMasterDB.First(x => x.Id == item.Id);
                                    //purchaseRequestSettlementHelper.SettleAmount(context, iRMaster, item.SettledAmount, userid, currentTime, guid, true);
                                    purchaseRequestSettlementHelper.SettleAmounts(context, iRMaster, item.SettledAmount, userid, currentTime, guid, true, item.TDSAmount, item.PurchaseOrderId);
                                }

                                IRHelper helper = new IRHelper();
                                detail = helper.SupplierNewPaymentRequest(context, detail, guid, userid);
                            }
                        }

                        scope.Complete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw ex;
                    }
                }
            }
            else
            {
                return false;
            }

        }



        [Route("UpdateIRPayment")]
        [HttpPost]
        public bool UpdateIRPayment(List<IRPaymentDetailsDC> detailList)
        {
            int userid = GetLoginUserId();
            long summaryId = 0;
            if (detailList != null && detailList.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                // using (var scope = new TransactionScope())
                {
                    using (var context = new AuthContext())
                    {
                        IRHelper helper = new IRHelper();
                        foreach (IRPaymentDetailsDC detail in detailList)
                        {
                            IRPaymentDetails dbDetail = context.IRPaymentDetailsDB.FirstOrDefault(x => x.Id == detail.Id);
                            summaryId = dbDetail.IRPaymentSummaryId;
                            if (detail.PaymentStatus == "Approved" && dbDetail.PaymentStatus == "Pending")
                            {

                                dbDetail.PaymentStatus = detail.PaymentStatus;
                                dbDetail.RefNo = detail.RefNo;
                                dbDetail.PaymentDate = detail.PaymentDate;
                                dbDetail.IsIROutstandingPending = false;
                                FullSupplierPaymentData fullSupplierPaymentData = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == dbDetail.SupplierId).FirstOrDefault();
                                if (fullSupplierPaymentData != null)
                                {
                                    fullSupplierPaymentData.InVoiceRemainingAmount = fullSupplierPaymentData.InVoiceRemainingAmount - dbDetail.TotalAmount;
                                    if (fullSupplierPaymentData.InVoiceRemainingAmount == 0)
                                    {
                                        fullSupplierPaymentData.SupplierPaymentStatus = "Full Paid";
                                    }
                                    else
                                    {
                                        fullSupplierPaymentData.SupplierPaymentStatus = "Partial Paid";
                                    }
                                    context.Entry(fullSupplierPaymentData).State = EntityState.Modified;
                                    context.Commit();
                                }

                                IrOutstandingDC item = JsonConvert.DeserializeObject<IrOutstandingDC>(dbDetail.IRList);
                                IRMasterDTO ir = context.IRMasterDB.Where(x => x.Id == item.Id).Select(x => new IRMasterDTO
                                {
                                    BuyerName = x.BuyerName,
                                    CreatedBy = x.CreatedBy,
                                    CreationDate = x.CreationDate,
                                    Discount = x.Discount,
                                    Gstamt = x.Gstamt,
                                    IRAmountWithOutTax = x.IRAmountWithOutTax,
                                    IRAmountWithTax = x.IRAmountWithTax,
                                    IRID = x.IRID,
                                    IRStatus = Math.Round(detail.TotalReaminingAmount.Value) <= 5 ? "Paid" : "Approved from Buyer side",
                                    IRType = x.IRType,
                                    PaymentStatus = Math.Round(detail.TotalReaminingAmount.Value) <= 5 ? "paid" : "partial paid",
                                    PaymentTerms = x.PaymentTerms,
                                    PurchaseOrderId = x.PurchaseOrderId,
                                    ReamainingAmt = (Math.Round(x.TotalAmountRemaining) - (detail.TotalAmount + detail.TDSAmount)),
                                    TotalTaxPercentage = x.TotalTaxPercentage,
                                    TotalAmountRemaining = detail.TotalAmount,
                                    // TotalAmountRemaining = (double)detail.TotalReaminingAmount,
                                    RejectedComment = x.RejectedComment,
                                    supplierId = x.supplierId,
                                    SupplierName = x.SupplierName,
                                    TotalAmount = x.TotalAmount,
                                    WarehouseId = x.WarehouseId,
                                    WarehouseName = "",
                                    TDSAmount = detail.TDSAmount,
                                }).FirstOrDefault();
                                List<IRMasterDTO> list = new List<IRMasterDTO>();
                                list.Add(ir);

                                PrPaymentTransfer prPaymentTransfer = new PrPaymentTransfer
                                {
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    FromPurchaseOrderId = null,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ModifiedBy = null,
                                    ModifiedDate = null,
                                    OutAmount = 0,
                                    SettledAmount = detail.TotalAmount,
                                    SourcePurchaseRequestPaymentId = dbDetail.Id,
                                    ToPurchaseOrderId = ir.PurchaseOrderId,
                                    TransferredAmount = detail.TotalAmount,
                                    IsTDSDeducted = dbDetail.TDSAmount == 0 ? false : true
                                };
                                context.PrPaymentTransferDB.Add(prPaymentTransfer);


                                helper.MakeIrLedgerEntry(list, context, userid, dbDetail, detail.Id);
                                context.Commit();

                            }
                            else if (detail.PaymentStatus == "Rejected" && dbDetail.PaymentStatus == "Pending")
                            {
                                dbDetail.PaymentStatus = detail.PaymentStatus;
                                dbDetail.IsIROutstandingPending = false;
                                context.Entry(dbDetail).State = EntityState.Modified;

                                var irpaymentdata = context.IRPaymentDetailsDB.Where(x => x.Id == detail.Id).FirstOrDefault();
                                var irmasterdata = context.IRMasterDB.Where(x => x.Id == irpaymentdata.IRMasterId).FirstOrDefault();
                                irmasterdata.TotalRemainingTDSAmount = irmasterdata.TotalRemainingTDSAmount + irpaymentdata.TDSAmount;


                                context.Entry(irmasterdata).State = EntityState.Modified;

                                context.Commit();
                            }
                            else
                            {
                                scope.Dispose();
                                return false;
                            }


                        }
                        IRPaymentSummary summary = context.IRPaymentSummaryDB.FirstOrDefault(x => x.Id == summaryId);
                        summary.IsIROutstandingPending = false;
                        context.Entry(summary).State = EntityState.Modified;

                        context.Commit();

                        scope.Complete();
                        return true;
                    }
                }
            }

            return false;
        }

        [Route("DocumentImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult DocumentImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/SupplierImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/SupplierImage"));

                        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/SupplierImage"), httpPostedFile.FileName);
                        //string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/SupplierImage"), fileName);



                        //string extension = Path.GetExtension(httpPostedFile.FileName);
                        //string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/SupplierImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/SupplierImage", LogoUrl);

                        LogoUrl = "/SupplierImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in Kisandan Method: " + ex.Message);
                return null;
            }

        }

        [Route("DepoDocumentImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult DepoDocumentImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/DepoImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DepoImage"));

                        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DepoImage"), httpPostedFile.FileName);
                        //string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DepoImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/DepoImage", LogoUrl);

                        LogoUrl = "/DepoImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in Kisandan Method: " + ex.Message);
                return null;
            }

        }



        [HttpGet]
        [Route("search")]
        public HttpResponseMessage GetSupplier(string key, int Warehouseid) // Add by Ravindra
        {
            logger.Info("start Item Master: ");

            try
            {
                using (var db = new AuthContext())
                {

                    logger.Info("User ID : {0} , Company Id : {1}", GetCompanyId(), GetUserId());
                    int CompanyId = GetCompanyId();

                    Supplier isactivee = new Supplier();

                    if (Warehouseid > 0)
                    {
                        ItemMaster objItemMaster = db.itemMasters.FirstOrDefault(s => s.SUPPLIERCODES.Contains(key) || s.SupplierName.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId && s.WarehouseId == Warehouseid);
                        if (objItemMaster.SupplierId > 0)
                        {
                            isactivee = db.Suppliers.Where(x => x.SupplierId == objItemMaster.SupplierId && x.Active == true && x.IsVerified == false).FirstOrDefault();

                        }
                        //MaterialItemMaster objMaterialItemMaster = db.MaterialItemMaster.Where(x => x.ItemNumber == objItemMaster.ItemNumber).FirstOrDefault();
                        //ItemmasterDTO ItemmasterDTO = new ItemmasterDTO
                        //{
                        //    ItemMaster = objItemMaster,
                        //    MaterialItemMaster = objMaterialItemMaster
                        //};
                        // Supplier  Supplier = db.Suppliers.Where(s => s.SupplierId == objItemMaster.SupplierId).FirstOrDefault();
                        if (isactivee != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, objItemMaster);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Item not found");
                        }

                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Item not found");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.GetBaseException().Message.ToString());
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("DashBoard")]
        public HttpResponseMessage DashBoard(int SupplierId)
        {
            try

            {
                SupplierDashBoard objSupplierDashBoard = DashboardResult(SupplierId);
                return Request.CreateResponse(HttpStatusCode.OK, objSupplierDashBoard);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [HttpGet]
        [Route("GetSupplierClosingAmount")]
        [Authorize]
        public double GetSupplierClosingAmount(int SupplierId)
        {
            double TotalOutstanding = GetTotalOutstanding(SupplierId);
            return TotalOutstanding;

        }

        private SupplierDashBoard DashboardResult(int SupplierId)
        {
            //double CalcTotalSalesCurrentQuarter = 0;
            //double CalcTotalSalesLastQuarter = 0;
            double TotalOutstanding = 0;
            double TotalOverdue = 0;
            double TotalIRPending = 0;

            QDC ObjTotalSalesCurrentQuarter = TotalSalesCurrentQuarter(SupplierId, "GetTotalSalesCurrentQuarter " + " " + "@SupplierId");
            //if (ObjTotalSalesCurrentQuarter != null && ObjTotalSalesCurrentQuarter.Any())
            //{
            //    var QC = ObjTotalSalesCurrentQuarter.Where(x => x.sd <= indianTime && x.ed > indianTime).FirstOrDefault();
            //    if (QC != null)
            //    {
            //        CalcTotalSalesCurrentQuarter = QC.Amount;
            //    }
            //    DateTime SalesLast = indianTime.AddMonths(-4);
            //    var SalesLastD = ObjTotalSalesCurrentQuarter.Where(x => x.sd <= SalesLast && x.ed > SalesLast).FirstOrDefault();
            //    if (SalesLastD != null)
            //    {
            //        CalcTotalSalesLastQuarter = SalesLastD.Amount;
            //    }
            //}
            //List<TotalSalesQuarter> ObjTotalSalesLastQuarter = TotalSalesCurrentQuarter(SupplierId, "GetTotalSalesLastQuarter " + " " + "@SupplierId");
            //CalcTotalSalesLastQuarter = ObjTotalSalesLastQuarter.Sum(x => x.Amount);

            CurrentPoCountStatusWise objCurrentPoCountStatusWise = GetCurrentPoCountStatusWise(SupplierId);
            FillRate objFillRate = GetFillRate(SupplierId);


            Supplier ObjSupplier = GetSupplierData(SupplierId);
            TotalIRPending = GetIRPending(SupplierId);
            TotalOverdue = GetTotalOverdue(SupplierId);
            TotalOutstanding = GetTotalOutstanding(SupplierId);

            SupplierDashBoard objSupplierDashBoard = new SupplierDashBoard
            {
                //TotalSalesCurrentQuarter = CalcTotalSalesCurrentQuarter,
                //TotalSalesLastQuarter = CalcTotalSalesLastQuarter,
                TCurrentMonthPOIssued = objCurrentPoCountStatusWise.TCurrentMonthPOIssued,
                TCurrentMonthPODelivered = objCurrentPoCountStatusWise.TCurrentMonthPODelivered,
                TCurrentMonthPOPending = objCurrentPoCountStatusWise.TCurrentMonthPOPending,
                AmountWiseFillRate = objFillRate.AmountWiseFillRate,
                ItemWiseFillRate = objFillRate.ItemWiseFillRate,
                Rating = ObjSupplier.rating,
                PaymentDays = ObjSupplier.PaymentTerms,
                TotalOutstanding = TotalOutstanding,
                IRPending = TotalIRPending,
                TotalOverdue = TotalOverdue,
                MTD = ObjTotalSalesCurrentQuarter.MTD,
                QTR = ObjTotalSalesCurrentQuarter.QTR,
                Yearly = ObjTotalSalesCurrentQuarter.Yearly

            };
            return objSupplierDashBoard;
        }

        private QDC TotalSalesCurrentQuarter(int SupplierId, string Procedure)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    QDC ObjTotalSalesQuarter = Context.Database.SqlQuery<QDC>(Procedure, SupplierIdParam).FirstOrDefault();
                    return ObjTotalSalesQuarter;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private CurrentPoCountStatusWise GetCurrentPoCountStatusWise(int SupplierId)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    CurrentPoCountStatusWise ObjCurrentPoCountStatusWise = Context.Database.SqlQuery<CurrentPoCountStatusWise>("GetCurrentPoCountStatusWise @SupplierId", SupplierIdParam).FirstOrDefault();
                    return ObjCurrentPoCountStatusWise;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private FillRate GetFillRate(int SupplierId)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {
                    FillRate ObjFillRate = new FillRate();
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    ObjFillRate = Context.Database.SqlQuery<FillRate>("GetFillRate @SupplierId", SupplierIdParam).FirstOrDefault();
                    if (ObjFillRate == null)
                    {
                        ObjFillRate = new FillRate();
                        ObjFillRate.AmountWiseFillRate = 0;
                        ObjFillRate.ItemWiseFillRate = 0;
                    }
                    return ObjFillRate;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Supplier GetSupplierData(int SupplierId)
        {
            try
            {

                using (AuthContext Context = new AuthContext())
                {
                    Supplier ObjSupplier = Context.Suppliers.Where(x => x.SupplierId == SupplierId).FirstOrDefault();
                    return ObjSupplier;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private double GetTotalOutstanding(int SupplierId)
        {
            try
            {
                double TotalOutstanding = 0;
                using (AuthContext Context = new AuthContext())
                {
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    TotalOutstanding = Context.Database.SqlQuery<double>("GetSupplierOutstanding @SupplierId", SupplierIdParam).FirstOrDefault();
                    return TotalOutstanding;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private double GetTotalOverdue(int SupplierId)
        {
            try
            {
                double TotalTotalOverdue = 0;
                using (AuthContext Context = new AuthContext())
                {
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    TotalTotalOverdue = Context.Database.SqlQuery<double>("GetTotalOverdue @SupplierId", SupplierIdParam).FirstOrDefault();
                    return TotalTotalOverdue;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private double GetIRPending(int SupplierId)
        {
            try
            {
                int TotalIRPending = 0;
                using (AuthContext Context = new AuthContext())
                {
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = SupplierId
                    };
                    TotalIRPending = Context.Database.SqlQuery<int>("GetTotalIRPending @SupplierId", SupplierIdParam).FirstOrDefault();
                    return Convert.ToDouble(TotalIRPending);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }



        #region Supplier data Get
        /// <summary>
        /// created Date :09/04/2020
        /// Created by Anushka
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>

        [Route("GetSupplierData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SupplierDc>> GetSupplierData()
        {
            using (var context = new AuthContext())
            {
                List<SupplierDc> SupplierDc = new List<SupplierDc>();

                SupplierDc = context.Database.SqlQuery<SupplierDc>("Getsellingbrands").ToList();

                //SupplierDc = (from s in context.Suppliers
                //              join d in context.DepoMasters
                //              on s.SupplierId equals d.SupplierId
                //              where (s.Active == true && s.Deleted == false)



                //              select new SupplierDc
                //              {
                //                  Name = s.Name,
                //                  SUPPLIERCODES = s.SUPPLIERCODES,
                //                  BillingAddress = s.BillingAddress,
                //                  City = s.City,
                //                  MobileNo = s.MobileNo,
                //                  DepoName = d.DepoName,
                //                  StateName = s.StateName,
                //                  businessType = s.bussinessType,
                //                  Active = s.Active,
                //                  CreatedDate = s.CreatedDate,
                //                  PaymentTerms = s.PaymentTerms,
                //                  SubcategoryName = s.Brand,
                //                  SupplierId = s.SupplierId,


                //                  EmailId = s.EmailId,
                //              }).ToList();

                return SupplierDc;
            }
        }

        #endregion

        [Route("GetSuppliersearch")]
        [HttpGet]
        [AllowAnonymous]
        public Supplier GetSuppliersearch(string key, int WarehouseId)
        {
            Supplier ass = new Supplier();

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);


            int CompanyId = compid;
            using (AuthContext context = new AuthContext())
            {

                if (WarehouseId > 0)
                {
                    ass = context.Suppliers.Where(s => s.SUPPLIERCODES.Contains(key) || s.Name.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId && s.WarehouseId == WarehouseId).FirstOrDefault();
                    return ass;
                }
                else
                {
                    ass = context.Suppliers.Where(s => s.SUPPLIERCODES.Contains(key) || s.Name.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId).FirstOrDefault();
                    return ass;
                }
            }
        }


        [HttpGet]
        [Route("searchSupplier")]
        public HttpResponseMessage GetSupplierData(string key, int Warehouseid) // Add by Ashwin
        {
            logger.Info("start Item Master: ");

            try
            {
                Supplier Supplier = null;
                ItemMaster objItemMaster = null;
                using (var db = new AuthContext())
                {

                    logger.Info("User ID : {0} , Company Id : {1}", GetCompanyId(), GetUserId());
                    int CompanyId = GetCompanyId();


                    if (Warehouseid > 0)
                    {
                        objItemMaster = db.itemMasters.FirstOrDefault(s => s.SUPPLIERCODES.Contains(key) || s.SupplierName.Contains(key) && s.Deleted == false && s.CompanyId == CompanyId && s.WarehouseId == Warehouseid);
                        if (objItemMaster != null)
                        {
                            Supplier = db.Suppliers.Where(s => s.SupplierId == objItemMaster.SupplierId).FirstOrDefault();

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, Supplier);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Item not found");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.GetBaseException().Message.ToString());
                logger.Info("End  Item Master: ");
                return null;
            }
        }


        [HttpGet]
        [Route("searchSupplierForPRadd")]
        public async Task<List<SupplierDc>> GetSupplierDataForPR(string key, int Warehouseid) // Add by shailesh
        {
            List<SupplierDc> result = new List<SupplierDc>();
            using (var db = new AuthContext())
            {
                var SupplierList = await db.Suppliers.Where(s => (s.SUPPLIERCODES.Contains(key) || s.Name.Contains(key)) && s.Active == true && s.Deleted == false).ToListAsync();
                result = SupplierList.GroupBy(x => new { x.Name, x.SUPPLIERCODES, x.SupplierId, x.BillingAddress, x.DepoName, x.City, x.MobileNo, x.EmailId, x.PaymentTerms, x.StateName, x.bussinessType, x.Active, x.CreatedDate }).Select(x => new SupplierDc
                {
                    Name = x.Key.Name,
                    SUPPLIERCODES = x.Key.SUPPLIERCODES,
                    SupplierId = x.Key.SupplierId,
                    BillingAddress = x.Key.BillingAddress,
                    DepoName = x.Key.DepoName,
                    City = x.Key.City,
                    MobileNo = x.Key.MobileNo,
                    EmailId = x.Key.EmailId,
                    CreatedDate = x.Key.CreatedDate,
                    Active = x.Key.Active,
                    StateName = x.Key.StateName,
                    PaymentTerms = x.Key.PaymentTerms,
                    businessType = x.Key.bussinessType
                }).ToList();
                // result = Mapper.Map(SupplierList).ToANew<List<SupplierDc>>();
            }
            return result;
        }

        private bool IsExistsDepo(string DepoCode)
        {
            using (AuthContext context = new AuthContext())
            {
                bool IsExists = false;

                int count = context.DepoMasters.Where(x => x.DepoCodes == DepoCode && x.Deleted == false).Count();
                if (count > 0)
                {
                    IsExists = true;
                }
                return IsExists;

            }
        }

        [HttpGet]
        [Route("CheckPRPOStopBySupplierDepo")]
        public int CheckPRPOStopBySupplierDepo(int depoId)
        {
            int PRPOStop = 0;
            using (AuthContext context = new AuthContext())
            {
                PRPOStop = context.Database.SqlQuery<int>("exec CheckPRPOStopBySupplierDepo " + depoId).FirstOrDefault();
            }
            return PRPOStop;
        }

        [HttpPost]
        [Route("SupplierPaymentAcknowledgement")]

        public HttpResponseMessage supplierPaymentAcknowledgement(SupplierFilterDC filterDC)
        {
            List<SupplierPaymentAcknowledgementDC> acknowledgementDCs = new List<SupplierPaymentAcknowledgementDC>();
            try
            {
                int Skip = (filterDC.Skip - 1) * filterDC.Take;
                using (var context = new AuthContext())
                {
                    var fromdate = new SqlParameter("@FromDate", filterDC.Fromdate);
                    var todate = new SqlParameter("@Todate", filterDC.Todate);

                    var suppliercode = new SqlParameter("@SupplierCode", filterDC.SupplierCode);
                    var skip = new SqlParameter("@skip", Skip);
                    var take = new SqlParameter("@take", filterDC.Take);
                    var iSExport = new SqlParameter("@ISExport", filterDC.ISExport);
                    if (string.IsNullOrEmpty(filterDC.SupplierCode))
                    {
                        suppliercode.Value = DBNull.Value;
                    }
                    acknowledgementDCs = context.Database.SqlQuery<SupplierPaymentAcknowledgementDC>("GetSupplierPaymentAcknowledgement @FromDate,@Todate,@SupplierCode,@skip,@take,@ISExport", fromdate, todate, suppliercode, skip, take, iSExport).ToList();

                    if (acknowledgementDCs != null && acknowledgementDCs.Any())
                    {
                        var response = new
                        {
                            Status = true,
                            data = acknowledgementDCs,
                            TotalCount = acknowledgementDCs.FirstOrDefault()?.totalCount
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            data = "No Data found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }

            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    data = "Error: " + ex.Message.ToString(),
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }


        [Route("CheckFieldForSupplierAndCustomer")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage fieldverifyforsuppliercustomer(string data, string fieldname, string type)
        {
            using (var context = new AuthContext())
            {
                var response = new { Status = true, Message = "" };
                if (type == "Supplier")
                {
                    if (fieldname == "mobilenumber")
                    {
                        bool anymobilenumberexists = context.Customers.Any(x => x.Mobile == data);
                        if (anymobilenumberexists)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Supplier is already registered as a Retailer"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Supplier is not registered as a Retailer"
                            };
                        }
                    }
                    else if (fieldname == "gstnumber")
                    {
                        bool anygstnumberexists = context.Customers.Any(x => x.RefNo == data);
                        if (anygstnumberexists)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Supplier is already registered as a Retailer"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Supplier is not registered as a Retailer"
                            };
                        }
                    }
                    else if (fieldname == "pannumber")
                    {
                        bool anypannumberexists = context.Customers.Any(x => x.PanNo == data);
                        if (anypannumberexists)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Supplier is already registered as a Retailer"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Supplier is not registered as a Retailer"
                            };
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }
                else
                {
                    if (fieldname == "mobilenumber")
                    {
                        bool anymobilenumberexists1 = context.SupplierTempDB.Any(x => x.MobileNo == data || x.proprietorphonenumber == data);
                        bool anymobilenumberexists2 = context.Suppliers.Any(x => x.MobileNo == data);
                        bool anymobilenumberexists3 = context.DepoMasterTempDB.Any(x => x.PhoneNumber == data);
                        bool anymobilenumberexists4 = context.DepoMasters.Any(x => x.PhoneNumber == data);
                        if (anymobilenumberexists2 || anymobilenumberexists1 || anymobilenumberexists3 || anymobilenumberexists4)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Retailer is already registered as a Supplier"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Retailer is not registered as a Supplier"
                            };
                        }
                    }
                    else if (fieldname == "pannumber")
                    {
                        bool anypannumberexists1 = context.SupplierTempDB.Any(x => x.Pancard == data || x.ProprietorPanNumber == data);
                        bool anypannumberexists2 = context.Suppliers.Any(x => x.Pancard == data || x.ProprietorPanNumber == data);
                        bool anypannumberexists3 = context.DepoMasterTempDB.Any(x => x.PANCardNo == data);
                        bool anypannumberexists4 = context.DepoMasters.Any(x => x.PANCardNo == data);
                        if (anypannumberexists1 || anypannumberexists2 || anypannumberexists3 || anypannumberexists4)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Retailer is already registered as a Supplier"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Retailer is not registered as a Supplier"
                            };
                        }
                    }
                    else if (fieldname == "gstnumber")
                    {
                        bool anygstnumberexists1 = context.SupplierTempDB.Any(x => x.GstInNumber == data || x.TINNo == data);
                        bool anygstnumberexists2 = context.Suppliers.Any(x => x.GstInNumber == data || x.TINNo == data);
                        bool anygstnumberexists3 = context.DepoMasterTempDB.Any(x => x.GSTin == data || x.TINNo == data);
                        bool anygstnumberexists4 = context.DepoMasters.Any(x => x.GSTin == data || x.TINNo == data);
                        if (anygstnumberexists1 || anygstnumberexists2 || anygstnumberexists3 || anygstnumberexists4)
                        {
                            response = new
                            {
                                Status = false,
                                Message = "This Retailer is already registered as a Supplier"
                            };
                        }
                        else
                        {
                            response = new
                            {
                                Status = true,
                                Message = "This Retailer is not registered as a Supplier"
                            };
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, response);

                }
            }
        }

        [Route("ExportSupplierList")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<ExportSupplierDC>> exportdata(string status, string keyword)
        {
            using (var context = new AuthContext())
            {
                if (keyword == "undefined" || keyword == null)
                {
                    keyword = "";
                }

                List<ExportSupplierDC> List = new List<ExportSupplierDC>();
                var paramStatus = new SqlParameter()
                {
                    ParameterName = "@Status",
                    Value = status
                };

                var paramuserid = new SqlParameter()
                {
                    ParameterName = "@Keyword",
                    Value = keyword
                };

                List = context.Database.SqlQuery<ExportSupplierDC>("Sp_ExportSupplierList @Status,@Keyword", paramStatus, paramuserid).ToList();
                return List;
            }
        }


        [Route("ExportFullSupplierList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<ExportSupplierDC>> exportfulldata()
        {
            using (var context = new AuthContext())
            {
                List<ExportSupplierDC> List = new List<ExportSupplierDC>();
                List = context.Database.SqlQuery<ExportSupplierDC>("Sp_ExportSupplierFullList").ToList();
                return List;
            }
        }

        #region SupplierRetailerCrossBuying

        [Route("SupplierRetailerAdd")]
        [HttpPost]
        public string SupplierRetailerAdd(List<SupplierRetailerAddDC> supplierretailer)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string result = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                if (supplierretailer.Count > 0)
                {
                    var supplierretailerdata = supplierretailer.Where(x => x.MasterId > 0).FirstOrDefault();
                    var id = supplierretailerdata.MasterId > 0 ? supplierretailerdata.MasterId : 0;
                    var type = supplierretailerdata.Type;
                    List<int> mappedid = supplierretailer.Select(x => x.MappedId).ToList();
                    if (id > 0)
                    {
                        var data = db.SupplierRetailerMappings.Where(x => x.Type == type && x.IsActive == true && x.IsDeleted == false && (x.MasterId == id || mappedid.Contains(x.MappedId) || mappedid.Contains(id) || x.MappedId == id)).ToList();
                        //var data = db.SupplierRetailerMappings.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && x.MasterId == id && x.Type==type);
                        if (data.Count() > 0)
                        {
                            result = "Already Exists";
                            return result;
                        }
                        else
                        {
                            foreach (var item in supplierretailer)
                            {
                                if (item.MasterId > 0)
                                {
                                    SupplierRetailerMapping supplierRetailerMapping = new SupplierRetailerMapping();
                                    supplierRetailerMapping.MasterId = item.MasterId;
                                    supplierRetailerMapping.MappedId = item.MappedId;
                                    supplierRetailerMapping.Type = item.Type;
                                    supplierRetailerMapping.IsActive = true;
                                    supplierRetailerMapping.IsDeleted = false;
                                    supplierRetailerMapping.CreatedDate = DateTime.Now;
                                    supplierRetailerMapping.CreatedBy = userid;
                                    db.SupplierRetailerMappings.Add(supplierRetailerMapping);
                                }
                            }
                            if (db.Commit() > 0)
                            {
                                result = "Add Successfully";
                                return result;
                            }
                            else
                            {
                                result = "Something Went Wrong";
                                return result;
                            }
                        }
                    }
                    else
                    {
                        result = "Please Select AtLeast One Data";
                        return result;
                    }
                }
                else
                {
                    result = "Data Not Found";
                    return result;
                }
            }
            return result;
        }

        [Route("SupplierRetailerGet")]
        [HttpGet]
        public SupplierRetailerDC SupplierRetailerGet(int Type, int Skip, int Take)
        {
            SupplierRetailerDC result = new SupplierRetailerDC();
            using (var db = new AuthContext())
            {
                string spName = "SupplierRetailerGets @Type,@Skip,@Take";
                if (Type == 1 || Type == 2)
                {
                    var type = new SqlParameter
                    {
                        ParameterName = "Type",
                        Value = Type
                    };
                    var skip = new SqlParameter
                    {
                        ParameterName = "Skip",
                        Value = Skip
                    };
                    var take = new SqlParameter
                    {
                        ParameterName = "Take",
                        Value = Take
                    };
                    result.supplierRetailerGetDCs = db.Database.SqlQuery<SupplierRetailerGetDC>(spName, type, skip, take).ToList();
                    result.TotalCount = result.supplierRetailerGetDCs.Any() ? result.supplierRetailerGetDCs.FirstOrDefault().TotalCount : 0;
                    return result;
                }
            }
            return result;
        }

        [Route("SupplierRetailerDeleteAllByMasterId")]
        [HttpGet]
        public string SupplierRetailerDeleteAllByMasterId(int MasterId, int Type)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string result = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var data = context.SupplierRetailerMappings.Where(x => x.MasterId == MasterId && x.IsActive == true && x.IsDeleted == false && x.Type == Type).ToList();
                if (data.Count > 0)
                {
                    foreach (var mapping in data)
                    {
                        mapping.IsActive = false;
                        mapping.IsDeleted = true;
                        mapping.ModifiedBy = userid;
                        mapping.ModifiedDate = DateTime.Now;
                        context.Entry(mapping).State = EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        result = "Deleted Successfully";
                        return result;
                    }
                    else
                    {
                        result = "Something Went Wrong";
                        return result;
                    }
                }
                else
                {
                    result = "Data Not Found";
                    return result;
                }
            }
        }

        [Route("SupplierRetailerDataByMasterId")]
        [HttpGet]
        public List<SupplierRetailerEditDC> SupplierRetailerDataByMasterId(int MasterId, int Type)
        {
            List<SupplierRetailerEditDC> supplierRetailerMappings = new List<SupplierRetailerEditDC>();
            using (var db = new AuthContext())
            {
                //var data = db.SupplierRetailerMappings.Where(x=>x.IsActive==true && x.IsDeleted==false && x.MasterId==MasterId).ToList();
                //if (data.Count > 0)
                //{
                //    supplierRetailerMappings = data;
                //    return supplierRetailerMappings;
                //}
                //else
                //{
                //    return supplierRetailerMappings;
                //}
                string spName = "SupplierRetailerDataByMasterId @MasterId,@Type";
                if (MasterId > 0 && Type > 0)
                {
                    var masterid = new SqlParameter { ParameterName = "MasterId", Value = MasterId };
                    var type = new SqlParameter { ParameterName = "Type", Value = Type };
                    supplierRetailerMappings = db.Database.SqlQuery<SupplierRetailerEditDC>(spName, masterid, type).ToList();
                    return supplierRetailerMappings;
                }
                else
                {
                    return supplierRetailerMappings;
                }
            }
        }

        [Route("SupplierRetailerDeleteById")]
        [HttpGet]
        public string SupplierRetailerDeleteById(int Id)
        {
            string result = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var data = db.SupplierRetailerMappings.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (data != null)
                {
                    data.IsActive = false;
                    data.IsDeleted = true;
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = userid;
                    db.Entry(data).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        result = "Delete Successfully";
                        return result;
                    }
                    else
                    {
                        result = "Something Went Wrong";
                        return result;
                    }
                }
                else
                {
                    result = "Data Not Found";
                    return result;
                }
            }
            return result;
        }

        [Route("SupplierRetailerEditById")]
        [HttpPost]
        public string SupplierRetailerEditById(int Id, int MappedId)
        {
            string res = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var data = db.SupplierRetailerMappings.Where(x => x.Id == Id).FirstOrDefault();
                if (data != null)
                {
                    data.MappedId = MappedId;
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = userid;
                    db.Entry(data).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        res = "Edit Successfully";
                        return res;
                    }
                    else
                    {
                        res = "Something went wrong";
                        return res;
                    }
                }
                else
                {
                    res = "Data Not Found";
                    return res;
                }
            }
        }

        [Route("SupplierRetailerSupplierSearch")]
        [HttpGet]
        public List<SupplierSearchDC> SupplierRetailerSupplierSearch(string key)
        {
            List<SupplierSearchDC> supplierSearchDCs = new List<SupplierSearchDC>();
            using (var db = new AuthContext())
            {
                var filterkey = new SqlParameter("@Key", key);
                var suppliertype = new SqlParameter("@type", 1);
                supplierSearchDCs = db.Database.SqlQuery<SupplierSearchDC>("exec Sp_GetRetailerCustomerSearch @Key,@type", filterkey, suppliertype).ToList();

                //supplierSearchDCs = db.Suppliers.Where(x => x.SUPPLIERCODES.ToLower().Contains(key.ToLower()) || x.Name.ToLower().Contains(key.ToLower())).Select(y=>new SupplierSearchDC { SupplierId=y.SupplierId,SupplierName=y.Name}).ToList();
                //if (supplierSearchDCs.Any())
                //{
                //    return supplierSearchDCs;
                //}
            }
            return supplierSearchDCs;
        }

        [Route("SupplierRetailerCustomerSearch")]
        [HttpGet]
        public List<CustomerSearchDC> SupplierRetailerCustomerSearch(string key)
        {
            List<CustomerSearchDC> customerSearchDCs = new List<CustomerSearchDC>();
            using (var db = new AuthContext())
            {
                var filterkey = new SqlParameter("@Key", key);
                var customertype = new SqlParameter("@type", 2);
                customerSearchDCs = db.Database.SqlQuery<CustomerSearchDC>("exec Sp_GetRetailerCustomerSearch @Key,@type", filterkey, customertype).ToList();
                //customerSearchDCs = db.Customers.Where(x => x.Skcode.ToLower().Contains(key.ToLower()) || x.ShopName.ToLower().Contains(key.ToLower())).Select(y=>new CustomerSearchDC { CustomerId=y.CustomerId,ShopName=y.ShopName}).ToList();
                //if (customerSearchDCs.Count > 0)
                //{
                //    return customerSearchDCs;
                //}
            }
            return customerSearchDCs;
        }

        [Route("SupplierRetailerAddNew")]
        [HttpPost]
        public string SupplierRetailerAddNew(SupplierRetailerAddDC supplier)
        {
            string result = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var data = db.SupplierRetailerMappings.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && (x.MappedId == supplier.MappedId || x.MasterId == supplier.MappedId) && x.Type == supplier.Type);
                if (data == null)
                {
                    SupplierRetailerMapping mapping = new SupplierRetailerMapping();
                    mapping.MasterId = supplier.MasterId;
                    mapping.MappedId = supplier.MappedId;
                    mapping.Type = supplier.Type;
                    mapping.CreatedDate = DateTime.Now;
                    mapping.CreatedBy = userid;
                    mapping.IsActive = true;
                    mapping.IsDeleted = false;
                    db.SupplierRetailerMappings.Add(mapping);
                    if (db.Commit() > 0)
                    {
                        result = "Add Successfully";
                        return result;
                    }
                    else
                    {
                        result = "Something Went Wrong";
                        return result;
                    }

                }
                else
                {
                    result = "Data Already Exists";
                    return result;
                }
            }
            return result;
        }

        [Route("SupplierRetailerSpecificSearch")]
        [HttpGet]
        public SupplierRetailerDC SupplierRetailerSpecificSearch(int Type, string Code, int Skip, int Take)
        {
            SupplierRetailerDC result = new SupplierRetailerDC();
            using (var db = new AuthContext())
            {

                if (Type == 1 || Type == 2)
                {
                    var type = new SqlParameter
                    {
                        ParameterName = "@type",
                        Value = Type
                    };
                    var code = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = Code
                    };
                    var skip = new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = Skip
                    };
                    var take = new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = Take
                    };
                    result.supplierRetailerGetDCs = db.Database.SqlQuery<SupplierRetailerGetDC>("exec Sp_getretailercustomerspcificsearchs @type,@Code,@Skip,@Take", type, code, skip, take).ToList();
                    result.TotalCount = result.supplierRetailerGetDCs.Any() ? result.supplierRetailerGetDCs.FirstOrDefault().TotalCount : 0;
                    return result;
                }

            }
            return result;
        }

        #endregion
        [HttpPost]
        [Route("SuppllierExcel")] //aartimukati

        public bool SuppllierExcel(SupplierFilterDC filterDC)
        {
            List<SupplierPaymentAcknowledgementDC> acknowledgementDCs = new List<SupplierPaymentAcknowledgementDC>();

            bool Result = false;
            string From = "";
            From = AppConstants.MasterEmail;
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            int Skip = (filterDC.Skip - 1) * filterDC.Take;
            using (var context = new AuthContext())
            {
                var SupplierEmail = context.Suppliers.FirstOrDefault(x => x.SUPPLIERCODES == filterDC.SupplierCode);
                if (SupplierEmail != null)
                {
                    var fromdate = new SqlParameter("@FromDate", filterDC.Fromdate);
                    var todate = new SqlParameter("@Todate", filterDC.Todate);

                    var suppliercode = new SqlParameter("@SupplierCode", filterDC.SupplierCode);
                    var skip = new SqlParameter("@skip", Skip);
                    var take = new SqlParameter("@take", filterDC.Take);
                    var iSExport = new SqlParameter("@ISExport", filterDC.ISExport);
                    if (string.IsNullOrEmpty(filterDC.SupplierCode))
                    {
                        suppliercode.Value = DBNull.Value;
                    }
                    acknowledgementDCs = context.Database.SqlQuery<SupplierPaymentAcknowledgementDC>("GetSupplierPaymentAcknowledgement @FromDate,@Todate,@SupplierCode,@skip,@take,@ISExport", fromdate, todate, suppliercode, skip, take, iSExport).ToList();
                    //-----------------------------------------------

                    if (acknowledgementDCs != null && acknowledgementDCs.Any())
                    {
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);

                        DataTable table = new DataTable();
                        List<SupplierPaymentAcknowledgementDC> data = new List<SupplierPaymentAcknowledgementDC>();
                        var datatables = new List<DataTable>();

                        foreach (var e in acknowledgementDCs)
                        {
                            data.Add(new SupplierPaymentAcknowledgementDC
                            {
                                PaymentDate = e.PaymentDate,
                                RefNo = e.RefNo,
                                BankName = e.BankName,
                                InvoiceNo = e.InvoiceNo,
                                SUPPLIERCODES = e.SUPPLIERCODES,
                                SupplierName = e.SupplierName,
                                VoucherType = e.VoucherType,
                                Amount = e.Amount,
                                totalCount = e.totalCount,
                                InvoiceDate = e.InvoiceDate
                            });
                        }
                        table = ClassToDataTable.CreateDataTable(data);
                        table.TableName = "DataALisT";
                        datatables.Add(table);

                        string filePath = ExcelSavePath + "abcListtttttttttt" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                        {

                            var sub = "this is subjectooooooooooooooo";
                            var msg = "this is msggggggggggggg";
                            Result = EmailHelper.SendMail(From, "aarti.mukati@shopkirana.com", "", sub, msg, filePath); //

                        }
                    }
                }
            }
            return Result;
        }

        [HttpPost]
        [Route("SupplierPaymentAcknowledgementMail")]
        public HttpResponseMessage SupplierPaymentAcknowledgementMail(SupplierFilterDC filterDC)
        {
            List<SupplierPaymentAcknowledgementDC> acknowledgementDCs = new List<SupplierPaymentAcknowledgementDC>();
            try
            {
                bool res = false;
                int Skip = (filterDC.Skip - 1) * filterDC.Take;
                using (var context = new AuthContext())
                {
                    var data = context.Suppliers.Where(x => x.SUPPLIERCODES == filterDC.SupplierCode).FirstOrDefault();
                    if(data!= null && data.EmailId != null) { }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "Email Not Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    var fromdate = new SqlParameter("@FromDate", filterDC.Fromdate);
                    var todate = new SqlParameter("@Todate", filterDC.Todate);

                    var suppliercode = new SqlParameter("@SupplierCode", filterDC.SupplierCode);
                    var skip = new SqlParameter("@skip", filterDC.Skip);
                    var take = new SqlParameter("@take", filterDC.Take);
                    var iSExport = new SqlParameter("@ISExport", 1);
                    if (string.IsNullOrEmpty(filterDC.SupplierCode))
                    {
                        suppliercode.Value = DBNull.Value;
                    }
                    acknowledgementDCs = context.Database.SqlQuery<SupplierPaymentAcknowledgementDC>("GetSupplierPaymentAcknowledgement @FromDate,@Todate,@SupplierCode,@skip,@take,@ISExport", fromdate, todate, suppliercode, skip, take, iSExport).ToList();

                    if (acknowledgementDCs != null && acknowledgementDCs.Any())
                    {
                        string From = "";
                        From = AppConstants.MasterEmail;
                        string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                        DataTable table = new DataTable();
                        var datatables = new List<DataTable>();
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);
                        List<SupplierPaymentAcknowledgementDCMail> datas = new List<SupplierPaymentAcknowledgementDCMail>();
                        foreach(var a in acknowledgementDCs)
                        {
                            datas.Add(new SupplierPaymentAcknowledgementDCMail
                            {
                                 PaymentDate = a.PaymentDate,
                                 RefNo =a.RefNo,
                                 BankName =a.BankName,
                                 InvoiceNo=a.InvoiceNo,
                                 SUPPLIERCODES =a.SUPPLIERCODES,
                                 SupplierName =a.SupplierName,
                                 VoucherType =a.VoucherType,
                                 Amount =a.Amount,
                                InvoiceDate =a.InvoiceDate,
                            });
                        }
                        table = ClassToDataTable.CreateDataTable(datas);
                        table.TableName = "Payment Advice";
                        datatables.Add(table);
                        string filePath = ExcelSavePath + "Payment Advice" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                        {

                            var sub = "Auto Payment Acknowledgment";
                            var msg = "Shopkirana Payment Acknowledgment";
                            res= EmailHelper.SendMail(From, data.EmailId, "", sub, msg, filePath);
                        }
                        if (res)
                        {
                            var response = new
                            {
                                Status = true,
                                Message = "mail Sent Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            var response = new
                            {
                                Status = false,
                                Message = "Something went wrong"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }

                    }
                    else
                    {
                        var response = new
                        {
                            Status = false,
                            Message = "No Data Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }

            }
            catch (Exception ex)
            {
                var response = new
                {
                    Status = false,
                    Message = "Error: " + ex.Message.ToString(),
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }

        [HttpGet]
        [Route("RemoveSupplierDocument")]
        public APIResponse RemoveSupplierDocument(int Id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                APIResponse res = new APIResponse();
                using (AuthContext context = new AuthContext())
                {
                    if (Id > 0)
                    {
                        var ExistData = context.SupplierDocumentDB.FirstOrDefault(x => x.Id == Id);
                        if (ExistData != null)
                        {
                            ExistData.IsActive = false;
                            ExistData.IsDeleted = true;
                            ExistData.ModifyBy = userid;
                            ExistData.ModifyDate = DateTime.Now;
                            context.Entry(ExistData).State = EntityState.Modified;
                        }

                        if (context.Commit() > 0)
                        {
                            res.Status = true;
                            res.Message = "Data Updated Saved ";
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Data Not Updated ";
                        }
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            };
        }

        [HttpGet]
        [Route("RemoveDepoDocument")]
        public APIResponse RemoveDepoDocument(int Id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                APIResponse res = new APIResponse();
                using (AuthContext context = new AuthContext())
                {
                    if (Id > 0)
                    {
                        var ExistData = context.DepoDocumentDB.FirstOrDefault(x => x.Id == Id);
                        if (ExistData != null)
                        {
                            ExistData.IsActive = false;
                            ExistData.IsDeleted = true;
                            ExistData.ModifyBy = userid;
                            ExistData.ModifyDate = DateTime.Now;
                            context.Entry(ExistData).State = EntityState.Modified;
                        }

                        if (context.Commit() > 0)
                        {
                            res.Status = true;
                            res.Message = "Data Updated Saved ";
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Data Not Updated ";
                        }
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            };
        }

        [Route("SuppLastPoFirstGrDate")]
        [HttpGet]
        public SuppLastPoFirstGrDateDC SuppLastPoFirstGrDate(int supplierid)
        {
            SuppLastPoFirstGrDateDC result = new SuppLastPoFirstGrDateDC();
            using (var db = new AuthContext())
            {
                var param1 = new SqlParameter("@supplierid", supplierid);
                result = db.Database.SqlQuery<SuppLastPoFirstGrDateDC>("exec SuppLastPoFirstGrDate @supplierid", param1).FirstOrDefault();
            }
            return result;
        }
    }


    public class SuppLastPoFirstGrDateDC
    {
        public int PurchaseOrderId { get; set; }
        public DateTime GrDate { get; set; }
    }
    public class SupplierDc
    {

        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string DepoName { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string BillingAddress { get; set; }
        public string MobileNo { get; set; }
        public string SubcategoryName { get; set; }
        public string StateName { get; set; }
        public bool Active { get; set; }
        public string businessType { get; set; }
        public int PaymentTerms { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string City { get; set; }
        public string EmailId { get; set; }
        public string SellingBrands { get; set; }
        public string ContactPerson { get; set; }
        public string GstInNumber { get; set; }


    }








    public class ContactUs
    {
        public string WarehouseHeadName { get; set; }
        public string WarehouseHeadEmailId { get; set; }
        public string WarehouseHeadContact { get; set; }
        public string AccountsHeadName { get; set; }
        public string AccountsHeadEmailId { get; set; }
        public string AccountsHeadContact { get; set; }
        public string CentralHeadName { get; set; }
        public string CentralHeadEmailId { get; set; }
    }
    public class resobjdashbord
    {
        public PODashboardDTO Result { get; set; }
        public string Message { get; set; }

        public bool Status { get; set; }
    }

    public class supplierDetail
    {
        public Supplier supplierId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class supplierData
    {
        public Supplier supplier { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class ChatData
    {
        public List<SupplierChat> ChatSupplier { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class PoDISTINCT
    {
        public List<int?> POid { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }

    public class PostChatFromSupplierAppDT0
    {
        public int PeopleID { get; set; }
        public string Department { get; set; }
        public int? warehouseID { get; set; }
        public string email { get; set; }
    }

    public class supplierDataDepo
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<DepoMasterDTO> depo { get; set; }
    }

    public class DepoMasterDTO
    {
        public int DepoId { get; set; }
        public string DepoName { get; set; }
        public string CityName { get; set; }
        public string phone { get; set; }

    }




    public class OTP
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string OtpNo { get; set; }
        public int? CustomerId { get; set; }
        public string SkCode { get; set; }
        public bool? CanUpdateCustomer { get; set; }
    }

    public class SupplierMappedCity
    {
        public int CityId { get; set; }
        public int SupplierId { get; set; }
    }


    public class ItemMasterSupplierListDC
    {
        public string Name { get; set; }
        public int SupplierId { get; set; }
    }


    public class SupplierDetail
    {
        public Supplier suppliers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class ChatFromSappDTO
    {

        public int SupplierId { get; set; }
        public string SUPPLIERCODES { get; set; }

        public string SupplierName { get; set; }
        public string Chat { get; set; }    // actual message 
        public string AudioURL { get; set; }    // link of audio sent in chat 
        public string ImageURL { get; set; }   // link of image sent in chat 
        public int? PeopleID { get; set; }    // chat from people name from backend
        public string PeopleName { get; set; }    // chat from people name from backend
        public int? PurchaseId { get; set; } // perticular Chat for Any Purchase Order ID
        public string ChatToPermission { get; set; } // chat to a perticular Department this Role will be same as role or 'department' in people table.
    }

    public class supplierDetailsFSApp
    {
        public Supplier Suppliers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class SupplierWarehouseObj
    {
        public List<SupplierWarehouseDTO> whList { get; set; }
        public string Message { get; set; }

        public bool Status { get; set; }
    }

    public class SupplierWarehouseDTO
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
    }





    public class supplieroutstandingamt
    {
        public int SupplierId { get; set; }
        public string TillDate { get; set; }
    }

    public class SupplierPaymentAcknowledgementDC
    {
        public DateTime PaymentDate { get; set; }
        public string RefNo { get; set; }
        public string BankName { get; set; }
        public string InvoiceNo { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string SupplierName { get; set; }
        public string VoucherType { get; set; }
        public double Amount { get; set; }
        public int totalCount { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string WarehouseName { get; set; }

    }
    public class SupplierFilterDC
    {
        public DateTime Fromdate { get; set; }
        public DateTime Todate { get; set; }
        public string SupplierCode { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int ISExport { get; set; }

    }

    public class SupplierOtp
    {
        public Supplier Supplier { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string OtpNo { get; set; }
    }

    public class TotalOutstandingDc
    {
        public int PoId { get; set; }
        public double Amount { get; set; }

    }

    public class TotalOverDueDc
    {
        public int PoId { get; set; }
        public double Amount { get; set; }
        public string InvoiceNumber { get; set; }

    }

    //    public class IRMasterDTO
    //{
    //    public int Id { get; set; }
    //    public int PurchaseOrderId { get; set; }
    //    public string IRID { get; set; }
    //    public int supplierId { get; set; }
    //    public string SupplierName { get; set; }
    //    public int WarehouseId { get; set; }
    //    public double TotalAmount { get; set; }
    //    public string IRStatus { get; set; }
    //    public double? Gstamt { get; set; }
    //    public double TotalTaxPercentage { get; set; }
    //    public double? Discount { get; set; }
    //    public double IRAmountWithTax { get; set; }
    //    public double IRAmountWithOutTax { get; set; }
    //    public double TotalAmountRemaining { get; set; }
    //    public string PaymentStatus { get; set; }
    //    public int PaymentTerms { get; set; }
    //    public string IRType { get; set; }
    //    public string CreatedBy { get; set; }
    //    public DateTime CreationDate { get; set; }
    //    public string WarehouseName { get; set; }
    //    public string RejectedComment { get; set; }
    //    public string BuyerName { get; set; }
    //}



    public class ExportSupplierDC
    {
        public int? SupplierId { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string Name { get; set; }
        public string GstInNumber { get; set; }
        public string Pancard { get; set; }
        public string ContactPerson { get; set; }
        public string EmailId { get; set; }
        public string SellingBrands { get; set; }
        public string Bank_AC_No { get; set; }
        public string Bank_Ifsc { get; set; }
        public string BankAddress { get; set; }
        public int? BankPINno { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string CityPincode { get; set; }
        public string DepoName { get; set; }
        public string BillingAddress { get; set; }
        public string FSSAI { get; set; }
        public int? EstablishmentYear { get; set; }
        public DateTime? StartedBusiness { get; set; }
        public string bussinessType { get; set; }
        public int? PaymentTerms { get; set; }
        //public string Status { get; set; }
        public string Statuss { get; set; }
        public string MobileNo { get; set; }
        public string Bank_Name { get; set; }
        public string MSMEType { get; set; }
    }

    public class SupplierRetailerAddDC
    {
        public int MasterId { get; set; }
        public int MappedId { get; set; }
        public int Type { get; set; } //1-Supplier 2-Customer 
    }
    public class SupplierRetailerGetDC
    {
        public int MasterId { get; set; }
        public string SkCode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int TotalCount { get; set; }
    }
    public class SupplierSearchDC
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
    public class CustomerSearchDC
    {
        public int CustomerId { get; set; }
        public string ShopName { get; set; }
    }
    public class SupplierRetailerEditDC
    {
        public int MasterId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public string PrimaryName { get; set; }
        public int MappedId { get; set; }
        public string MappedName { get; set; }
    }
    public class SupplierRetailerDC
    {
        public List<SupplierRetailerGetDC> supplierRetailerGetDCs { get; set; }
        public int TotalCount { get; set; }
    }

    public class SupplierPaymentAcknowledgementDCMail
    {
        public DateTime PaymentDate { get; set; }
        public string RefNo { get; set; }
        public string BankName { get; set; }
        public string InvoiceNo { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string SupplierName { get; set; }
        public string VoucherType { get; set; }
        public double Amount { get; set; }
        public DateTime? InvoiceDate { get; set; }

    }
}