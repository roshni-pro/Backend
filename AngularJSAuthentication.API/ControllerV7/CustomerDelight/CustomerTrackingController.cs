using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.CRM;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using static AngularJSAuthentication.DataContracts.Masters.CustomerDelightDc;

namespace AngularJSAuthentication.API.ControllerV7.CustomerDelight
{
    [RoutePrefix("api/CustomerTracking")]
    public class CustomerTrackingController : ApiController
    {
        [Route("GetCustomerTrackingList")]
        [HttpPost]
        public CustomerDelightRes GetCustomerTrackingList(CustomerDelightFilterDc obj)
        {
            CustomerDelightRes result = new CustomerDelightRes();
            CustomersManager manager = new CustomersManager();
            result = manager.GetCustomerTrackingList(obj);

            var SkCodeList = result.CustomerDelightListDcs.Select(y => y.Skcode).Distinct().ToList();
            var TagData = CRMCustomerTag(SkCodeList);
            result.CustomerDelightListDcs.ForEach(x =>
            {
                x.CRMTag = TagData.Result.FirstOrDefault(y => y.Skcode == x.Skcode)?.CRMTags;
            });
            return result;
        }

        [Route("GetCustomeDetailbyId")]
        [HttpGet]
        [AllowAnonymous]
        public CustomerDelightListDc GetCustomeDetailbyId(int CustomerId, int Id)
        {

            CustomerDelightRes res = new CustomerDelightRes();
            try
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@CustomerId", CustomerId);
                    var paramId = new SqlParameter("@Id", Id);

                    var data = context.Database.SqlQuery<CustomerDelightListDc>("Exec GetDelightCustomerDetails  @CustomerId,@Id", param, paramId).FirstOrDefault();
                    List<string> SkCodeList = new List<string>();
                    SkCodeList.Add(data.Skcode);
                    var TagData = CRMCustomerTag(SkCodeList);
                    data.CRMTag = TagData.Result.FirstOrDefault(y => y.Skcode == data.Skcode)?.CRMTags;

                    return data;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("GetCustomeDetailbyIdNew")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CustomerTrackingWrapper> GetCustomeDetailbyIdNew(int CustomerId)
        {
            String baseUrl = string.Concat(HttpContext.Current.Request.Url.Scheme, "://", HttpContext.Current.Request.Url.Authority);
            try
            {
                CustomersManager customerManager = new CustomersManager();
                var result = await customerManager.GetCustomerList(CustomerId);
                if (result != null)
                {
                    result.Customer.Shopimage = CustomerImageHelper.GetImageFullPath(result.Customer.Shopimage, baseUrl);
                    foreach (var res in result.RequestList)
                    {
                        res.CaptureImagePath = CustomerImageHelper.GetTrackingImageFullPath(res.CaptureImagePath, baseUrl);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //[Route("Reject")]
        //[HttpPost]
        //public bool Reject(UpdateDelightDc obj)
        //{
        //    CustomerDelightRes res = new CustomerDelightRes();
        //    bool result = false;
        //    int userid = 0;
        //    userid = GetUserId();
        //    using (var context = new AuthContext())
        //    {
        //        var customerLatLngVerify = context.CustomerLatLngVerify.FirstOrDefault(x => x.CustomerId == obj.CustomerId && x.IsActive && x.Id == obj.Id);
        //        if (customerLatLngVerify != null && customerLatLngVerify.Status != 2)
        //        {
        //            customerLatLngVerify.Status = 3;   // 3 rejected
        //            customerLatLngVerify.Nodocument = obj.Nodocument;
        //            customerLatLngVerify.Comment = obj.Comment;
        //            customerLatLngVerify.ModifiedBy = userid;
        //            customerLatLngVerify.ModifiedDate = DateTime.Now;
        //            context.Entry(customerLatLngVerify).State = EntityState.Modified;
        //        }
        //        if (context.Commit() > 0)
        //        {
        //            result = true;
        //        }
        //        return result;
        //    }

        //}
        //[Route("Verified")]
        //[HttpPost]
        //[AllowAnonymous]
        //public bool Verified(UpdateDelightDc obj)
        //{
        //    CustomerDelightRes res = new CustomerDelightRes();
        //    int userid = 0;
        //    userid = GetUserId();
        //    try
        //    {
        //        bool result = false;
        //        using (var context = new AuthContext())
        //        {
        //            var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
        //            var customerVerify = context.CustomerLatLngVerify.FirstOrDefault(x => x.CustomerId == obj.CustomerId && x.IsActive && x.Id == obj.Id);

        //            if (customer != null && customerVerify != null)
        //            {
        //                customer.City = obj.City;

        //                #region to assign cluster ID and determine if it is in cluster or not.                   
        //                if (customer.lat != obj.Newlat || customer.lg != obj.Newlg)
        //                {
        //                    var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(obj.Newlat).Append("', '").Append(obj.Newlg).Append("')");
        //                    var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
        //                    if (!clusterId.HasValue)
        //                    {
        //                        customer.InRegion = false;
        //                    }
        //                    else
        //                    {
        //                        customer.ClusterId = clusterId;
        //                        var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
        //                        customer.ClusterName = dd.ClusterName;
        //                        customer.InRegion = true;
        //                        customer.Warehouseid = dd.WarehouseId;
        //                        customer.WarehouseName = dd.WarehouseName;
        //                        customer.Cityid = dd.CityId;
        //                    }
        //                }
        //                #endregion
        //                //if (customer.ShippingAddress != obj.Address)
        //                //{
        //                //    GeoHelper geoHelper = new GeoHelper();
        //                //    decimal? lat, longitude;
        //                //    geoHelper.GetLatLongWithZipCode(obj.Address, obj.City, obj.ZipCode, out lat, out longitude);
        //                //    if (lat.HasValue && longitude.HasValue)
        //                //    {
        //                //        customer.Addresslat = (double)lat;
        //                //        customer.Addresslg = (double)longitude;
        //                //    }
        //                //    else
        //                //    {
        //                //        customer.Addresslat = 0;
        //                //        customer.Addresslg = 0;
        //                //    }
        //                //    customer.Distance = 0;
        //                //}
        //                //if (customer.Addresslat.HasValue && customer.Addresslg.HasValue && customer.Addresslat.Value > 0 && customer.Addresslg.Value > 0 && obj.Newlat > 0 && obj.Newlg > 0)
        //                //{
        //                //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(customer.Addresslat.Value, customer.Addresslg.Value);
        //                //    var destination = new System.Device.Location.GeoCoordinate(obj.Newlat, obj.Newlg);
        //                //    customer.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
        //                //}


        //                customer.ShippingAddress = obj.Address;
        //                customer.lat = obj.Newlat;
        //                customer.lg = obj.Newlg;
        //                if (customer.lat != obj.Newlat && customer.lg != obj.Newlg)
        //                {
        //                    customer.Shoplat = obj.Newlat;
        //                    customer.Shoplg = obj.Newlg;
        //                }
        //                customer.CustomerVerify = obj.CustomerVerify;
        //                customer.Active = obj.IsActive;
        //                customer.AreaName = obj.AreaName;

        //                customer.State = obj.State;
        //                customer.ZipCode = obj.ZipCode;

        //                customer.BillingAddress = obj.BillingAddress;
        //                customer.BillingAddress1 = obj.BillingAddress1;
        //                customer.BillingState = obj.BillingState;
        //                customer.BillingZipCode = obj.BillingZipCode;
        //                customer.BillingCity = obj.BillingCity;
        //                if (obj.CustomerType != null)
        //                {
        //                    customer.CustomerType = obj.CustomerType;

        //                }
        //                context.Entry(customer).State = EntityState.Modified;

        //                customerVerify.Status = 2; // Verified
        //                customerVerify.Nodocument = obj.Nodocument;
        //                customerVerify.Comment = obj.Comment;
        //                //customerVerify.ShippingAddress = obj.Address;
        //                customerVerify.ModifiedBy = userid;
        //                context.Entry(customerVerify).State = EntityState.Modified;

        //            }
        //            if (context.Commit() > 0)
        //            {
        //                result = true;
        //            }
        //            return result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [Route("ExportCustomerDelight")]
        [HttpPost]
        [AllowAnonymous]
        public List<CustomerDelightExportDc> ExportCustomerDelight(CustomerDelightFilterDc obj)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    string whereclause = "  c.Deleted=0 ";

                    if (obj.CityID > 0)
                    {
                        whereclause += " and c.CityID =" + obj.CityID;
                    }
                    if (obj.ClusterId > 0)
                    {
                        whereclause += " and c.ClusterId =" + obj.ClusterId;
                    }
                    if (obj.IsActive > -1)
                    {
                        whereclause += " and c.Active =" + obj.IsActive;
                    }
                    if (obj.CustomerType != "All")
                    {
                        whereclause += " and c.CustomerType =" + " '" + obj.CustomerType + "'";
                    }
                    if (obj.Status > 0 && obj.Status != 5 && obj.Status != 4 && obj.Status != 6 && obj.Status != 7)
                    {
                        whereclause += " and cust.Status =" + obj.Status;
                    }
                    if (obj.Status == 4)
                    {
                        whereclause += " and cust.ShopFound = 1 ";
                    }
                    if (obj.Status == 5)
                    {
                        whereclause += " and cust.ShopFound = 0 ";
                    }
                    if (obj.Status == 6)
                    {
                        whereclause += " and isnull(cust.status,0) = 0 ";
                    }
                    if (obj.Status == 7) //NO Document
                    {
                        whereclause += " and cust.Nodocument = 1 ";
                    }
                    if (obj.Keyward != null)
                    {
                        whereclause += " and (c.Skcode like '%" + obj.Keyward + "%' or c.Mobile like '%" + obj.Keyward + "%')";
                    }

                    List<CustomerDelightExportDc> List = new List<CustomerDelightExportDc>();
                    var param = new SqlParameter("@whereclause", whereclause);
                    List = context.Database.SqlQuery<CustomerDelightExportDc>("Exec SpExportCustomerDelight @whereclause", param).ToList();

                    var SkCodeList = List.Select(y => y.Skcode).Distinct().ToList();
                    var TagData = CRMCustomerTag(SkCodeList);
                    List.ForEach(x =>
                    {
                        x.CRMTag = TagData.Result.FirstOrDefault(y => y.Skcode == x.Skcode)?.CRMTags;
                    });
                    return List;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("GetDelightCount")]
        [HttpGet]
        [AllowAnonymous]
        public DataSet GetDelightCount(int clusterId, int CityID)
        {

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString);
            DataSet ds = new DataSet();

            SqlCommand cmd = new SqlCommand("DelightCustomerCount", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@clusterId", clusterId);
            cmd.Parameters.AddWithValue("@CityID", CityID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                return ds;
            }
            else
            {
                return null;
            }
        }

        [Route("GetCustomerLocations")]
        [HttpGet]
        public DataContracts.Masters.CustomerLocationDc GetCustomerLocations(int customerId)
        {
            var todayDate = DateTime.Now.Date;
            using (var context = new AuthContext())
            {
                var customerLat = context.CustomerLocations.FirstOrDefault(x => x.CustomerId == customerId && !x.IsDeleted && EntityFunctions.TruncateTime(x.CreatedDate) == todayDate);
                var customerLatDc = Mapper.Map(customerLat).ToANew<DataContracts.Masters.CustomerLocationDc>();
                return customerLatDc;
            }
        }

        [Route("RemoveAllCustomerLocations")]
        [HttpGet]
        public bool RemoveAllCustomerLocations(int customerId)
        {
            var todayDate = DateTime.Now.Date;
            using (var context = new AuthContext())
            {
                var customerLat = context.CustomerLocations.Where(x => x.CustomerId == customerId && !x.IsDeleted).ToList();

                foreach (var ap in customerLat)
                {
                    ap.IsDeleted = true;
                    context.Entry(ap).State = EntityState.Modified;
                }
                return context.Commit() > 0;
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

        [Route("UpdateCustomerBillingCity")]
        [HttpGet]
        public string updateCustomerBillingCity(string billingCity, string billingState, int CustomerID)
        {

            string result = "";
            using (var context = new AuthContext())
            {
                var Customer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerID);
                string refNo = Customer.RefNo.Trim();
                if (Customer != null && refNo.Any() && (Customer.RefNo != "" || Customer.RefNo != "N/A" || Customer.RefNo != "NA" || Customer.RefNo != null))
                {
                    Customer.BillingState = billingState;
                    Customer.BillingCity = billingCity;
                    context.Entry(Customer).State = EntityState.Modified;
                    if (context.Commit() > 0) { result = "Billing City/State updated successfully"; };
                }
                else
                {
                    result = "Customer ID not found";
                    //if(Customer == null)
                    //{
                    //    result = "Customer ID not found";
                    //}else if(Customer.IsGSTCustomer == false)
                    //{
                    //    result = "Customer is not a GST Customer";
                    //}
                    //else
                    //{
                    //    result = "Error - Cannot Update";
                    //}
                }
                return result;
            }
        }

        #region Customer address 
        [Route("UpdateShippingAddress")]
        [HttpPost]
        public async Task<string> UpdateShippingAddress(CustomerAddressVM obj)
        {
            int userid = 0;
            userid = GetUserId();
            string result = "";
            if (userid > 0 && obj != null && obj.AddressLat > 0 && obj.AddressLng > 0)
            {
                using (var context = new AuthContext())
                {

                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
                    if (customer != null && (customer.CustomerVerify != "Full Verified" || customer.CustomerVerify != "Parial Verified") && customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified)
                    {
                        var customerVerify = context.CustomerAddressDB.FirstOrDefault(x => x.CustomerId == obj.CustomerId);

                        if (customerVerify != null)
                        {
                            customerVerify.AreaPlaceId = obj.AreaPlaceId;
                            customerVerify.AreaText = obj.AreaText;
                            customerVerify.AreaLat = (double)obj.AreaLat;
                            customerVerify.AreaLng = (double)obj.AreaLng;
                            customerVerify.AddressPlaceId = obj.AddressPlaceId;
                            customerVerify.AddressText = obj.AddressText;
                            customerVerify.AddressLat = (double)obj.AddressLat;
                            customerVerify.AddressLng = (double)obj.AddressLng;
                            customerVerify.AddressLineOne = obj.AddressLineOne;
                            customerVerify.AddressLineTwo = obj.AddressLineTwo;
                            customerVerify.ModifiedDate = DateTime.Now;
                            customerVerify.ModifiedBy = userid;
                            customerVerify.CityPlaceId = obj.CityPlaceId;
                            context.Entry(customerVerify).State = EntityState.Modified;
                        }
                        else
                        {
                            var CustomerAddress = new CustomerAddress
                            {
                                AreaPlaceId = obj.AreaPlaceId,
                                AreaText = obj.AreaText,
                                AreaLat = (double)obj.AreaLat,
                                AreaLng = (double)obj.AreaLng,
                                AddressPlaceId = obj.AddressPlaceId,
                                AddressText = obj.AddressText,
                                AddressLat = (double)obj.AddressLat,
                                AddressLng = (double)obj.AddressLng,
                                AddressLineOne = obj.AddressLineOne,
                                AddressLineTwo = obj.AddressLineTwo,
                                CustomerId = obj.CustomerId,
                                ZipCode = obj.ZipCode,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                CityPlaceId = obj.CityPlaceId

                            };
                            context.CustomerAddressDB.Add(CustomerAddress);
                        }

                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(obj.AddressLat).Append("', '").Append(obj.AddressLng).Append("')");
                        var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            customer.InRegion = false;
                            result = "can't change due to updated Lat Lng is out of cluster";
                            return result;
                        }
                        else
                        {
                            var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                            customer.ClusterId = dd.ClusterId;
                            customer.ClusterName = dd.ClusterName;
                            customer.InRegion = true;
                            customer.Warehouseid = dd.WarehouseId;
                            customer.WarehouseName = dd.WarehouseName;
                            customer.Cityid = dd.CityId;
                            customer.City = dd.CityName;
                            customer.ShippingCity = dd.CityName;

                        }

                        customer.ShippingAddress = null;
                        if (!string.IsNullOrEmpty(obj.AddressLineOne))
                        {
                            customer.ShippingAddress = obj.AddressLineOne + ",";
                        }
                        if (!string.IsNullOrEmpty(obj.AddressLineTwo))
                        {
                            customer.ShippingAddress += obj.AddressLineTwo + ",";
                        }
                        if (!string.IsNullOrEmpty(obj.AddressText))
                        {
                            customer.ShippingAddress += obj.AddressText;
                        }
                        if (!string.IsNullOrEmpty(obj.ZipCode))
                        {
                            customer.ShippingAddress += "," + obj.ZipCode;
                        }
                        customer.lat = (double)obj.AddressLat;
                        customer.lg = (double)obj.AddressLng;
                        customer.Addresslat = customer.lat;
                        customer.Addresslg = customer.lg;
                        customer.UpdatedDate = DateTime.Now;
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            result = "shipping address updated successfully";
                        }

                    }
                    else { result = "something went wrong"; }
                }
            }
            else { result = "something went wrong"; }
            return result;
        }


        [Route("GetTrackingCustomerDocument/{CustomerId}")]
        [HttpGet]
        public async Task<TrackingCustomerDocumentDc> GetTrackingCustomerDocument(int CustomerId)
        {
            String baseUrl = string.Concat(HttpContext.Current.Request.Url.Scheme, "://", HttpContext.Current.Request.Url.Authority);
            var result = new TrackingCustomerDocumentDc();

            if (CustomerId > 0)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@CustomerId", CustomerId);
                    result = context.Database.SqlQuery<TrackingCustomerDocumentDc>("Exec GetTrackingCustomerDocument  @CustomerId", param).FirstOrDefault();
                    if (result != null)
                    {
                        result.OtherDocumetImage = CustomerImageHelper.GetImageFullPath(result.OtherDocumetImage, baseUrl);
                        result.GSTImage = CustomerImageHelper.GetImageFullPath(result.GSTImage, baseUrl);
                    }

                }
            }
            return result;
        }


        [Route("UpdateTrackingCustomerDocument")]
        [HttpPost]
        public async Task<string> UpdateTrackingCustomerDocument(TrackingCustomerDocumentDc obj)
        {
            string result = "";
            int userid = 0;
            userid = GetUserId();
            if (userid > 0 && obj != null)
            {
                using (var context = new AuthContext())
                {
                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
                    if (customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified && !string.IsNullOrEmpty(obj.GSTNo) && customer.IsGSTCustomer == false && obj.IsGSTCustomer)
                    {
                        var panno = "";
                        if (!string.IsNullOrEmpty(obj.GSTNo))
                        {
                            var checkgst = context.Customers.Where(x => x.RefNo == obj.GSTNo && x.CustomerId != obj.CustomerId).Count();
                            if (checkgst > 0)
                            {
                                result = "Gst Already Exists.";
                                return result;
                            }
                            panno = obj.GSTNo.Substring(2, 10);
                            var ExistsSkcode = context.Customers.FirstOrDefault(x => x.PanNo == panno && x.CustomerId != obj.CustomerId)?.Skcode;
                            if (ExistsSkcode != null)
                            {
                                result = "PanNo Already Exists in " + ExistsSkcode;
                                return result;
                            }
                        }
                        CustGstDTOList gstreponse = await CheckCustomerGSTVerify(obj.GSTNo);
                        if (gstreponse.Status)
                        {
                            var state = context.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstreponse.custverify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstreponse.custverify.State.ToLower().Trim());
                            customer.RefNo = obj.GSTNo;
                            customer.PanNo = panno;
                            customer.IsPanVerified = true;
                            customer.UploadGSTPicture = obj.GSTImage;
                            customer.ShopName = gstreponse.custverify.ShopName;
                            customer.NameOnGST = gstreponse.custverify.Name;
                            customer.BillingCity = gstreponse.custverify.City;
                            customer.BillingState = state != null ? state.StateName : gstreponse.custverify.State;
                            customer.BillingZipCode = gstreponse.custverify.Zipcode;
                            customer.BillingAddress = gstreponse.custverify.ShippingAddress;
                            customer.GstExpiryDate = obj.GstExpiryDate;
                            //customer.ShopName = gstreponse.custverify.ShopName;
                            //customer.Name = gstreponse.custverify.Name;
                            //customer.CustomerDocumentStatus = (int)CustomerDocumentStatusEnum.Verified;

                            var custdoc = context.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId && x.CustomerDocTypeMasterId == obj.DocTypeId && x.IsActive == true).FirstOrDefault();
                            if (custdoc != null)
                            {
                                custdoc.DocPath = customer.UploadGSTPicture;
                                custdoc.ModifiedBy = userid;
                                custdoc.ModifiedDate = DateTime.Now;
                                context.Entry(custdoc).State = EntityState.Modified;
                            }
                            else
                            {
                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customer.CustomerId,
                                    IsActive = true,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = obj.DocTypeId,
                                    DocPath = customer.UploadGSTPicture,
                                    IsDeleted = false
                                });
                            }

                            context.Entry(customer).State = EntityState.Modified;
                            if (context.Commit() > 0) { result = "Document Status verified Successfully"; } else { result = "something wnet wrong"; }
                        }
                        else
                        {
                            result = gstreponse.Message;
                        }
                    }
                    else if (customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified && obj.IsGSTCustomer == false)
                    {
                        if (!string.IsNullOrEmpty(obj.LicenceNo))
                        {
                            var checkgst = context.Customers.Where(x => x.LicenseNumber == obj.LicenceNo && x.CustomerId != obj.CustomerId).Count();
                            if (checkgst > 0)
                            {
                                result = "LicenceNo Already Exists.";
                                return result;
                            }
                        }

                        //customer.UploadRegistration = obj.OtherDocumetImage;  changes on 13/12/2023 as per ravi sir
                        customer.LicenseNumber = obj.LicenceNo;
                        customer.LicenseExpiryDate = obj.LicenseExpiryDate;
                        //customer.CustomerDocumentStatus = (int)CustomerDocumentStatusEnum.Verified;
                        var custdoc = context.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId && x.CustomerDocTypeMasterId == obj.DocTypeId && x.IsActive == true).FirstOrDefault();
                        if (custdoc != null)
                        {
                            custdoc.DocPath = obj.OtherDocumetImage;
                            custdoc.ModifiedBy = userid;
                            custdoc.ModifiedDate = DateTime.Now;
                            context.Entry(custdoc).State = EntityState.Modified;
                        }
                        else
                        {
                            context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = customer.CustomerId,
                                IsActive = true,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = obj.DocTypeId,
                                DocPath = obj.OtherDocumetImage,
                                IsDeleted = false
                            });
                        }
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0) { result = "Document Status verified Successfully"; } else { result = "something went wrong"; }
                    }
                    else
                    {
                        result = "something went wrong or document already  verified";
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("VerifyTrackingCustomerDocument")]
        public async Task<string> VerifyTrackingCustomerDocument(int customerId, bool isGSTDocument)
        {
            string result = "";
            int userid = 0;
            userid = GetUserId();
            if (userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    if (customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified
                        && isGSTDocument
                        && !string.IsNullOrEmpty(customer.RefNo)
                        && !string.IsNullOrEmpty(customer.UploadGSTPicture)
                        && customer.GstExpiryDate != null
                        && customer.GstExpiryDate > DateTime.Today.AddMonths(-6))
                    {
                        CustGstDTOList gstreponse = await CheckCustomerGSTVerify(customer.RefNo);
                        if (gstreponse.Status)
                        {
                            var state = context.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstreponse.custverify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstreponse.custverify.State.ToLower().Trim());
                            customer.NameOnGST = gstreponse.custverify.Name;
                            customer.BillingCity = gstreponse.custverify.City;
                            customer.BillingState = state != null ? state.StateName : gstreponse.custverify.State;
                            customer.BillingZipCode = gstreponse.custverify.Zipcode;
                            customer.BillingAddress = gstreponse.custverify.ShippingAddress;
                            customer.IsGSTCustomer = true;
                            customer.CustomerDocumentStatus = (int)CustomerDocumentStatusEnum.Verified;

                            if (context.Commit() > 0) { result = "Document Status verified Successfully"; } else { result = "something wnet wrong"; }
                        }
                        else
                        {
                            result = gstreponse.Message;
                        }
                    }
                    else if (customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified
                        && isGSTDocument == false
                        && !string.IsNullOrEmpty(customer.UploadRegistration)
                        && !string.IsNullOrEmpty(customer.LicenseNumber)
                        && customer.LicenseExpiryDate != null
                        && (customer.LicenseExpiryDate > DateTime.Today.AddMonths(-6)))
                    {

                        //customer.CustomerDocumentStatus = (int)CustomerDocumentStatusEnum.Verified;
                        customer.CustomerDocumentStatus = (int)CustomerDocumentStatusEnum.Verified;
                        customer.IsGSTCustomer = false;
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0) { result = "Document Status verified Successfully"; } else { result = "something went wrong"; }
                    }
                    else
                    {
                        if (isGSTDocument && customer.GstExpiryDate < DateTime.Today.AddMonths(-6))
                        {
                            result = "expiry date issue";
                        }
                        else if (!isGSTDocument && customer.LicenseExpiryDate < DateTime.Today.AddMonths(-6))
                        {
                            result = "expiry date issue";
                        }
                        else
                        {
                            result = "something went wrong or document already  verified";
                        }

                    }
                }
            }
            return result;
        }


        public async Task<CustGstDTOList> CheckCustomerGSTVerify(string GSTNO)
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
                        //GSTdata.ShippingAddress=gst.taxpayerInfo.pradr.
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
                        string CityM, CityD;
                        CityM = GSTdata.City;
                        CityD = gst.taxpayerInfo.pradr?.addr?.dst;
                        CityM = CityM.ToUpper();
                        CityD = CityD.ToUpper();
                        if (String.Compare(CityM, CityD) == 0)
                        {

                            CityD = "";
                        };

                        db.CustGSTverifiedRequestDB.Add(GSTdata);
                        db.Commit();

                        if (!string.IsNullOrEmpty(GSTdata.City) && !string.IsNullOrEmpty(GSTdata.State))
                        {
                            Managers.CustomerAddressRequestManager manager = new Managers.CustomerAddressRequestManager();
                            manager.AddGSTCityAndState(GSTdata.City, GSTdata.Zipcode, GSTdata.State, GSTdata.RefNo.Substring(0, 2), db);
                        }

                        CustGstVerify Cust = new CustGstVerify()
                        {
                            id = GSTdata.GSTVerifiedRequestId,
                            RefNo = gst.taxpayerInfo.gstin,
                            Name = gst.taxpayerInfo.lgnm,
                            ShopName = gst.taxpayerInfo.tradeNam,
                            //if (GSTdata.City== gst.taxpayerInfo.pradr?.addr?.dst)
                            //{ };
                            ShippingAddress = string.Format("{0}, {1}, {2}, {3},{4},{5},{6}-{7}", GSTdata.HomeNo, GSTdata.PlotNo, GSTdata.HomeName, GSTdata.ShippingAddress, GSTdata.City, CityD, GSTdata.State, GSTdata.Zipcode),
                            State = gst.taxpayerInfo.pradr?.addr?.stcd,
                            City = gst.taxpayerInfo.pradr?.addr?.loc,
                            Active = gst.taxpayerInfo.sts,
                            lat = gst.taxpayerInfo.pradr?.addr?.lt,
                            lg = gst.taxpayerInfo.pradr?.addr?.lg,
                            Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd
                        };
                        if (Cust.Active == "Active")
                        {
                            CustGstDTOList Custlist = new CustGstDTOList()
                            {
                                custverify = Cust,
                                Status = true,
                                Message = "Customer GST Number Is Verify Successfully."
                            };
                            return Custlist;
                        }
                        else
                        {
                            CustGstDTOList Custlist = new CustGstDTOList()
                            {
                                custverify = Cust,
                                Status = false,
                                Message = "Customer GST Number Is " + Cust.Active
                            };
                            return Custlist;
                        }


                    }
                }

                else
                {
                    CustGstDTOList Custlist = new CustGstDTOList()
                    {
                        custverify = null,
                        Status = false,
                        Message = "Customer GST Number not valid."
                    };
                    return Custlist;
                }
            }

        }


        [Route("CustomerVerifyRequest")]
        [HttpPost]
        public async Task<string> CustomerVerifyRequest(CustomerVerifyRequestVM obj)
        {
            int userid = 0;
            userid = GetUserId();
            string result = "";
            if (userid > 0 && obj != null && obj.Id > 0)
            {
                using (var context = new AuthContext())
                {
                    var customerVerify = context.CustomerLatLngVerify.FirstOrDefault(x => x.Id == obj.Id);
                    if (customerVerify != null && customerVerify.Status == 2) { result = "request already verified"; }
                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerVerify.CustomerId);
                    if (customer != null && customerVerify != null && (customer.CustomerVerify != "Full Verified" || customer.CustomerVerify != "Partial Verified"))
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customerVerify.Newlat).Append("', '").Append(customerVerify.Newlg).Append("')");
                        var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            customer.InRegion = false;
                            result = "can't change due to updated Lat Lng is out of cluster";
                            return result;
                        }

                        var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                        customer.ClusterId = dd.ClusterId;
                        customer.ClusterName = dd.ClusterName;
                        customer.InRegion = true;
                        customer.Warehouseid = dd.WarehouseId;
                        customer.WarehouseName = dd.WarehouseName;
                        customer.Cityid = dd.CityId;
                        customer.City = dd.CityName;

                        customer.lat = customerVerify.Newlat;
                        customer.lg = customerVerify.Newlg;
                        customer.UpdatedDate = DateTime.Now;

                        customer.ShippingAddressStatus = customerVerify.AppType == (int)AppEnum.SarthiApp ? (int)ShippingAddressStatusEnum.PhysicalVerified : (int)ShippingAddressStatusEnum.VirtualVerified;

                        context.Entry(customer).State = EntityState.Modified;

                        customerVerify.Status = 2;
                        customerVerify.Aerialdistance = obj.Aerialdistance;
                        customerVerify.ModifiedBy = userid;
                        customerVerify.ModifiedDate = DateTime.Now;
                        context.Entry(customerVerify).State = EntityState.Modified;

                        //other request get rejected
                        var customerLatLngVerifylist = context.CustomerLatLngVerify.Where(x => x.CustomerId == customer.CustomerId && x.Id != obj.Id && x.Status != 2).ToList();
                        if (customerLatLngVerifylist != null && customerLatLngVerifylist.Any())
                        {
                            customerLatLngVerifylist.ForEach(x =>
                            {
                                x.Status = 3;
                                x.ModifiedBy = userid;
                                x.ModifiedDate = DateTime.Now;
                                context.Entry(x).State = EntityState.Modified;
                            });
                        }
                        if (context.Commit() > 0)
                        {
                            result = "request verified successfully";
                            return result;
                        }

                    }
                    if (customer != null && (customer.CustomerVerify == "Full Verified" || customer.CustomerVerify == "Partial Verified"))
                    {
                        result = "request already verified";
                    }
                    else
                    {
                        result = "something went wrong";
                    }
                }
            }
            else { result = "something went wrong"; }
            return result;
        }

        [Route("VirtuallyVerify")]
        [HttpGet]
        public async Task<string> VirtuallyVerify(int customerId)
        {
            int userid = 0;
            userid = GetUserId();
            string result = "";
            if (userid > 0 && customerId > 0)
            {
                using (var context = new AuthContext())
                {

                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    if (customer != null && customer.CustomerVerify != "Full Verified" && customer.Shoplat != null && customer.Shoplg != null)
                    {
                        customer.lat = customer.Shoplat.Value;
                        customer.lg = customer.Shoplg.Value;

                        customer.UpdatedDate = DateTime.Now;
                        customer.ShippingAddressStatus = (int)ShippingAddressStatusEnum.VirtualVerified;

                        context.Entry(customer).State = EntityState.Modified;

                        if (context.Commit() > 0)
                        {
                            result = "request verified successfully";
                        }

                    }
                    else { result = "something went wrong"; }
                }
            }
            else { result = "something went wrong"; }
            return result;
        }

        [Route("RejectAllRequest/{CustomerId}")]
        [HttpGet]
        public async Task<string> RejectAllRequest(int CustomerId)
        {
            string result = "";
            int userid = 0;
            userid = GetUserId();
            if (CustomerId > 0 && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var customerLatLngVerifylist = context.CustomerLatLngVerify.Where(x => x.CustomerId == CustomerId && x.Status != 2).ToList();
                    if (customerLatLngVerifylist != null && customerLatLngVerifylist.Any())
                    {
                        customerLatLngVerifylist.ForEach(x =>
                       {
                           x.Status = 3;
                           x.ModifiedBy = userid;
                           x.ModifiedDate = DateTime.Now;
                           context.Entry(x).State = EntityState.Modified;
                       });

                        #region Previous status update on reject by Anurag 16/02/2023

                        var status = context.CustomerStatusHistoryDb.Where(x => x.CustomerId == CustomerId && x.AppType == 3 && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (status != null)
                        {
                            var customerStatus = context.Customers.Where(x => x.Active == true && x.Deleted == false).FirstOrDefault();
                            Customer obj = new Customer();
                            obj.CustomerVerify = status.CustomerVerify;
                            context.Entry(obj).State = EntityState.Modified;

                            CustomerStatusHistory statushistory = new CustomerStatusHistory();
                            statushistory.IsActive = false;
                            statushistory.IsDeleted = true;
                            statushistory.ModifiedBy = userid;
                            statushistory.ModifiedDate = DateTime.Now;
                            context.Entry(statushistory).State = EntityState.Modified;
                        }
                        #endregion

                    }
                    if (context.Commit() > 0)
                    {
                        result = "request rejected successfully";
                    }
                }
            }
            return result;
        }

        [Route("UpdateCustomerBillingAddress")]
        [HttpPost]
        public async Task<string> UpdateCustomerBillingAddress(CustomerBillingAddressDc obj)
        {
            int userid = 0;
            userid = GetUserId();
            string result = "";
            if (userid > 0 && obj != null && obj.CustomerId > 0)
            {
                using (var context = new AuthContext())
                {
                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
                    if (customer != null && (customer.CustomerVerify != "Full Verified" || customer.CustomerVerify != "Partial Verified") && string.IsNullOrEmpty(customer.RefNo))
                    {
                        customer.BillingAddress1 = obj.BillingAddress1;
                        customer.BillingAddress = obj.BillingAddress;
                        customer.BillingCity = obj.BillingCity;
                        customer.BillingState = obj.BillingState;
                        customer.BillingZipCode = obj.BillingZipCode;
                        customer.UpdatedDate = DateTime.Now;
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            result = "Billing address updated successfully";
                        }
                    }
                    else if (!string.IsNullOrEmpty(customer.RefNo))
                    {
                        result = "Billing address can't change due to Gstn Customer ";
                    }
                    else
                    { result = "Billing address can't change due to current status is :" + customer.CustomerVerify; }
                }
            }
            else { result = "something went wrong"; }
            return result;
        }

        [Route("UpdateCustomerStatus")]
        [HttpPost]
        public async Task<string> UpdateCustomerStatus(CustomerStatusUpdateDc obj)
        {
            int userid = 0;
            userid = GetUserId();
            string result = "";
            if (userid > 0 && obj != null && obj.CustomerId > 0)
            {
                using (var context = new AuthContext())
                {
                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
                    if (customer != null && customer.CustomerVerify != "Full Verified")
                    {
                        customer.Active = obj.IsActive;
                        customer.CustomerVerify = obj.CustomerVerify;
                        customer.CustomerType = obj.CustomerType;
                        customer.CustomerDocumentStatus = obj.CustomerDocumentStatus;
                        customer.ShippingAddressStatus = obj.ShippingAddressStatus;
                        customer.ShopName = obj.ShopName;
                        customer.TypeOfBuissness = obj.TypeOfBuissness;
                        customer.UpdatedDate = DateTime.Now;
                        customer.IsSignup = string.IsNullOrEmpty(obj.ShopName) ? false : true;
                        //customer.ChannelMasterId=obj.ChannelMasterId;
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            result = "Customer Status updated successfully";
                        }
                    }
                    else { result = "something went wrong"; }
                }
            }
            else { result = "something went wrong"; }
            return result;
        }

        [Route("IsDocumentNotExists")]
        [HttpGet]
        public async Task<bool> IsDocumentNotExists(int CustomerId, string DocumentNo)
        {
            bool result = true;
            if (CustomerId > 0 && DocumentNo != null)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@CustomerId", CustomerId);
                    var param1 = new SqlParameter("@DocumentNo", DocumentNo);
                    int count = context.Database.SqlQuery<int>("Exec IsDocumentNotExists @CustomerId, @DocumentNo", param, param1).FirstOrDefault();
                    if (count > 0)
                    {
                        result = false;
                    }
                    else
                    { result = true; }
                }
            }
            return result;
        }


        #endregion

        public async Task<List<CRMCustomerWithTag>> CRMCustomerTag(List<string> SkCoddeList)
        {
            //List<CRMCustomerWithTag> list = new List<CRMCustomerWithTag>();
            string CrmPlatformId = CRMPlatformConstants.CustomerTrackingPage;
            CRMManager manager = new CRMManager();
            var list = await manager.GetCRMCustomerWithTag(SkCoddeList, CrmPlatformId);
            return list;
        }
    }
}


