using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Myudhar")]
    public class MyUdharController : BaseAuthController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        #region  Post api for app side data post
        /// <summary>
        /// app side data post for RBL customer
        /// </summary>
        /// <param name="MU"></param>
        /// <returns></returns>
        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(Myudhar MU)
        {

            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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

                    Myudhar cust = db.MyudharDB.Where(x => x.CustomerId == MU.CustomerId && x.Deleted == false).FirstOrDefault();

                    // mdata.url = MU.PanCardurl;
                    if (cust == null)
                    {

                        Myudhar mdata = new Myudhar();
                        mdata.CustomerId = MU.CustomerId;
                        mdata.PanCardurl = MU.PanCardurl.Trim('"');
                        mdata.BackImageUrl = MU.BackImageUrl.Trim('"');
                        mdata.PanCardNo = MU.PanCardNo;
                        mdata.AddressProofUrl = MU.AddressProofUrl.Trim('"');
                        mdata.AnnualTurnOver = MU.AnnualTurnOver;
                        mdata.Gender = MU.Gender;
                        mdata.BusinessVintage = MU.BusinessVintage;
                        mdata.othercustdoc = MU.othercustdoc;
                        mdata.Deleted = MU.Deleted;
                        mdata.CreatedDate = indianTime;
                        mdata.UpdatedDate = indianTime;
                        mdata.Termcondition = true;
                        db.MyudharDB.Add(mdata);
                        db.Commit();
                        logger.Info("End  Myudhar: ");
                        customerDetailDTO MUData = new customerDetailDTO()
                        {
                            MUget = mdata,
                            Status = true,
                            Message = "Save  successfully."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, MUData);
                    }
                    else
                    {
                        cust.CustomerId = MU.CustomerId;
                        cust.PanCardurl = MU.PanCardurl.Trim('"');
                        cust.BackImageUrl = MU.BackImageUrl.Trim('"');
                        cust.PanCardNo = MU.PanCardNo;
                        cust.AddressProofUrl = MU.AddressProofUrl.Trim('"');
                        cust.AnnualTurnOver = MU.AnnualTurnOver;
                        cust.Gender = MU.Gender;
                        cust.BusinessVintage = MU.BusinessVintage;
                        cust.othercustdoc = MU.othercustdoc;
                        cust.Deleted = MU.Deleted;
                        cust.UpdatedDate = indianTime;
                        db.MyudharDB.Attach(cust);
                        db.Entry(cust).State = EntityState.Modified;
                        db.Commit();
                        customerDetailDTO MUData = new customerDetailDTO()
                        {
                            MUget = cust,
                            Status = true,
                            Message = "Update  successfully."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, MUData);
                    }

                }
                catch (Exception ex)
                {
                    customerDetailDTO MUData = new customerDetailDTO()
                    {
                        MUget = null,
                        Status = false,
                        Message = "Something Went Wrong."
                    };

                    logger.Error("Error in Add data Myudhar " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, MUData);


                }
            }
        }
        #endregion
        #region get All data MyUdhar 
        /// <summary>
        /// Get all data
        /// </summary>
        /// <returns></returns>
        [Route("getall")]
        public HttpResponseMessage Get()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Myudhar: ");
                List<Myudhar> MU = new List<Myudhar>();
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

                    MU = db.MyudharDB.Where(x => x.Deleted == false).ToList();
                    List<customerDetailgetdDTO> udharcust = new List<customerDetailgetdDTO>();

                    var custIds = MU.Select(x => x.CustomerId).ToList();
                    var customers = db.Customers.Where(x => custIds.Contains(x.CustomerId)).ToList();
                    var whIds = customers.Where(x => x.Warehouseid.HasValue && x.Warehouseid.Value > 0).Select(z => z.Warehouseid.Value).ToList();
                    var warehouses = db.Warehouses.Where(x => whIds.Contains(x.WarehouseId)).ToList();

                    foreach (var custdata in MU)
                    {
                        var cust = customers.Where(x => x.CustomerId == custdata.CustomerId).FirstOrDefault();
                        var warehouseName = warehouses != null && warehouses.Any() ? warehouses.FirstOrDefault(x => x.WarehouseId == cust.Warehouseid)?.WarehouseName : "";
                        customerDetailgetdDTO CU = new customerDetailgetdDTO();
                        if (cust != null)
                        {
                            CU.Name = cust.Name;
                            CU.Address = cust.ShippingAddress;
                            CU.DOB = cust.DOB;
                            CU.SkCode = cust.Skcode;
                            CU.Mobile = cust.Mobile;
                            CU.WarehouseName = warehouseName;
                            CU.cityName = cust.City;
                            CU.postalcode = cust.ZipCode;
                            CU.PanCardNo = custdata.PanCardNo;
                            CU.PanCardurl = custdata.PanCardurl;
                            CU.AddressProofUrl = custdata.AddressProofUrl;
                            CU.BackImageUrl = custdata.BackImageUrl;
                            CU.AnnualTurnOver = custdata.AnnualTurnOver;
                            CU.BusinessVintage = custdata.BusinessVintage;
                            CU.CreatedDate = custdata.CreatedDate;
                            CU.UpdatedDate = custdata.UpdatedDate;
                            udharcust.Add(CU);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, udharcust);

                }
                catch (Exception ex)
                {


                    logger.Error("Error in getdata Myudhar " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, MU);

                }
            }
        }
        #endregion
        #region get by warehouseId data MyUdhar 
        /// <summary>
        /// Get all data
        /// </summary>
        /// <returns></returns>
        [Route("getwhid")]
        public HttpResponseMessage GetWHid(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Myudhar: ");
                List<Myudhar> MU = new List<Myudhar>();
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

                    MU = db.MyudharDB.Where(x => x.Deleted == false).ToList();
                    List<customerDetailgetdDTO> udharcust = new List<customerDetailgetdDTO>();
                    var custIds = MU.Select(x => x.CustomerId).ToList();
                    var customers = db.Customers.Where(x => custIds.Contains(x.CustomerId)).ToList();
                    var whIds = customers.Where(x => x.Warehouseid.HasValue && x.Warehouseid.Value > 0).Select(z => z.Warehouseid.Value).ToList();
                    var warehouses = db.Warehouses.Where(x => whIds.Contains(x.WarehouseId)).ToList();

                    foreach (var custdata in MU)
                    {

                        var cust = customers.Where(x => x.CustomerId == custdata.CustomerId && x.Warehouseid == WarehouseId).FirstOrDefault();
                        var warehouseName = warehouses != null && warehouses.Any() ? warehouses.FirstOrDefault(x => x.WarehouseId == cust.Warehouseid)?.WarehouseName : "";
                        customerDetailgetdDTO CU = new customerDetailgetdDTO();
                        if (cust != null)
                        {
                            CU.Name = cust.Name;
                            CU.Address = cust.ShippingAddress;
                            CU.DOB = cust.DOB;
                            CU.SkCode = cust.Skcode;
                            CU.Mobile = cust.Mobile;
                            CU.WarehouseName = warehouseName;
                            CU.cityName = cust.City;
                            CU.postalcode = cust.ZipCode;
                            CU.PanCardNo = custdata.PanCardNo;
                            CU.PanCardurl = custdata.PanCardurl;
                            CU.AddressProofUrl = custdata.AddressProofUrl;
                            CU.BackImageUrl = custdata.BackImageUrl;
                            CU.AnnualTurnOver = custdata.AnnualTurnOver;
                            CU.BusinessVintage = custdata.BusinessVintage;
                            CU.CreatedDate = custdata.CreatedDate;
                            CU.UpdatedDate = custdata.UpdatedDate;
                            udharcust.Add(CU);
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, udharcust);

                }
                catch (Exception ex)
                {


                    logger.Error("Error in getdata Myudhar " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, MU);

                }
            }
        }
        #endregion
        #region get by Customer MyUdhar 
        /// <summary>
        /// get single data
        /// created Date:07/03/2019
        /// </summary>
        /// <param name="Customerid"></param>
        /// <returns></returns>
        [Route("")]
        public HttpResponseMessage Getcust(int Customerid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Myudhar: ");

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

                    var MU = db.MyudharDB.Where(x => x.CustomerId == Customerid && x.Deleted == false).FirstOrDefault();
                    customerDetailgetsingleDTO MUgetData = new customerDetailgetsingleDTO()
                    {
                        Myudhcust = MU,
                        Status = true,
                        Message = "success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, MUgetData);

                }
                catch (Exception ex)
                {
                    customerDetailgetsingleDTO MUgetData = new customerDetailgetsingleDTO()
                    {
                        Myudhcust = null,
                        Status = false,
                        Message = "Something Went Wrong."
                    };

                    logger.Error("Error in getdata Myudhar " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, MUgetData);

                }
            }
        }
        #endregion
        public class customerDetailgetsingleDTO
        {
            public Myudhar Myudhcust { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class customerDetailgetdDTO
        {
            public string SkCode { get; set; }
            public string Mobile { get; set; }
            public string WarehouseName { get; set; }
            public string cityName { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public DateTime? DOB { get; set; }
            public int MyudharId { get; set; }
            public int CustomerId { get; set; }
            public string postalcode { get; set; }
            public string PanCardNo { get; set; }
            public string PanCardurl { get; set; }
            public string AddressProofUrl { get; set; }
            public string BackImageUrl { get; set; }
            public bool Termcondition { get; set; }
            public string AnnualTurnOver { get; set; }
            public string BusinessVintage { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }


        }
        public class customerDetailDTO
        {
            public Myudhar MUget { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class customerDetailgetDTO
        {
            public List<Myudhar> Myudh { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
    }
}
