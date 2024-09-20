using AngularJSAuthentication.API.EwayBill;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc;
using AngularJSAuthentication.DataContracts.EwayBill;
using System.Web.Script.Serialization;
using RestSharp.Extensions;
using OpenHtmlToPdf;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Runtime.Serialization.Formatters.Binary;
using iTextSharp.text.pdf.qrcode;
using System.Text.RegularExpressions;
using iTextSharp.text.html.simpleparser;
using System.Configuration;
using System.Globalization;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.Model.TripPlanner;

namespace AngularJSAuthentication.API.Helper.EwayBill
{


    public class EwayBillHelper
    {
        public string BaseUrl = string.Empty;
        public static string authtoken = string.Empty;
        //public string BaseUrl = "https://api-sandbox.clear.in/einv/";
        //private string gstin = "09AAFCD5862R006";
        //private string authtoken = "1.7007a747-e980-4fb7-94b5-ca0d4aed9e8f_110a705996a75dadd0756517d2bf44b0ba3384e9243430b52ba88f57ffc783c0";
        public CompanyDetails GetcompanyDetails(AuthContext authContext)
        {
            CompanyDetails companyDetails = authContext.CompanyDetailsDB.Where(x => x.IsDeleted == false && x.IsActive == true && x.eWayBillEnable == true).FirstOrDefault();
            return companyDetails;
        }
        public Warehouse GetWarehouseDetails(int WarehouseId, AuthContext authContext)
        {
            Warehouse warehouse = authContext.Warehouses.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true).FirstOrDefault();
            return warehouse;
        }
        #region B2B

        public async Task<bool> GenrateEwayBillByIRN(EwayBillBackendDc ewayBillBackendDc, AuthContext authContext, int userid, int orderid = 0)
        {
            bool status = false;
            AuthTokenGenerationDc authTokenGenerationDc = new AuthTokenGenerationDc();
            var CompanyDetails = GetcompanyDetails(authContext);
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;

                string errorMessage = "";
                List<string> errorMessageList = new List<string>();
                List<EwayBillOrderList> OrderList = new List<EwayBillOrderList>();
                OrderList = await this.GetEwayBillOrderList(ewayBillBackendDc.TripPlannerConfirmedMasterid, authContext, orderid, ewayBillBackendDc.VehicleId);
                if (OrderList != null && OrderList.Any())
                {
                    string gstin = OrderList.FirstOrDefault().GSTin;
                    var B2BCustomer = OrderList.Where(x => x.CustomerGstFlag == true).ToList();
                    var B2CCustomer = OrderList.Where(x => x.CustomerGstFlag == false).ToList();
                    string vehicleNumber = "";
                    if (!string.IsNullOrEmpty(ewayBillBackendDc.ReplacementVehicleNo))
                    {
                        vehicleNumber = ewayBillBackendDc.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                    }
                    else if (!string.IsNullOrEmpty(ewayBillBackendDc.vehicleno))
                    {
                        vehicleNumber = ewayBillBackendDc.vehicleno.Trim().Replace(" ", "").ToUpper().ToString();
                    }
                    if (B2BCustomer != null && B2BCustomer.Any())
                    {
                        List<ClearTaxReqResp> clearTaxReqResponselist = new List<ClearTaxReqResp>();
                        List<ClearTaxReqResp> clearTaxReqErrorlist = new List<ClearTaxReqResp>();
                        List<EwayResponceDc> AllresponseEwayBillbyIRNDCList = new List<EwayResponceDc>();
                        EwayResponceDc responseEwayBillbyIRNDCList = new EwayResponceDc();
                        List<EwayResponceDcAll> responseEwayBillbyIRNDCAll = new List<EwayResponceDcAll>();
                        string vehicleNo = B2BCustomer.FirstOrDefault().VehicleNo;
                        ParallelLoopResult parellelResult = Parallel.ForEach(B2BCustomer, (item) =>
                        {
                            List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;
                            List<EwayResponceDc> responseEwayBillIRNDCList = null;
                            EwayResponceDc responseEwayBillbyIRNDC = null;
                            // ErrorResponseEwayBillByIRN errorResponseEwayBillByIRNDc = null;                       
                            ewaybillirnParam ewayBillPostDc = new ewaybillirnParam();
                            EwayResponceDcAll ewayResponceDc = new EwayResponceDcAll();
                            ewayBillPostDc.Irn = item.IRNNo;
                            ewayBillPostDc.Distance = item.WarehousePincode.Equals(item.ZipCode) == true ? 100 : ewayBillBackendDc.distance;
                            ewayBillPostDc.TransMode = "1";
                            ewayBillPostDc.TransId = ewayBillBackendDc.TransportGST;
                            ewayBillPostDc.TransName = ewayBillBackendDc.TransportName;
                            ewayBillPostDc.TransDocDt = "";
                            ewayBillPostDc.TransDocNo = "";
                            ewayBillPostDc.VehNo = !string.IsNullOrEmpty(vehicleNumber) ? vehicleNumber.Trim().Replace(" ", "").ToUpper().ToString() : item.VehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                            ewayBillPostDc.VehType = "R";
                            ewayBillPostDc.DispDtls = null;
                            var RequestjsonString = JsonConvert.SerializeObject(new List<ewaybillirnParam> { ewayBillPostDc });
                            // var res = InsertEwayBillRespone(item.OrderId, authContext);
                            ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration()
                            {
                                OrderId = item.OrderId,
                                IsActive = true,
                                CreateDate = DateTime.Now,
                                IsProcessed = false,
                                APIType = "GenerateEWB",
                                APITypes = 1,//GenerateEWB
                            };
                            ewayResponceDc.clearTaxIntegration = (clearTaxIntegration);
                            ClearTaxReqResp ctrp = new ClearTaxReqResp()
                            {
                                CreateDate = DateTime.Now,
                                IsActive = true,
                                Type = "Request",
                                OrderId = item.OrderId,
                                Json = RequestjsonString,
                                Url = BaseUrl
                            };
                            ewayResponceDc.OrderId = item.OrderId;
                            ewayResponceDc.ClearTaxRequest = ctrp;
                            extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                                {
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "x-cleartax-auth-token",
                                  new List<string> { authtoken }
                                ),
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "gstin",
                                  new List<string> { gstin }
                                ),
                                };
                            try
                            {
                                using (var client = new GenericRestHttpClient<List<ewaybillirnParam>, string>(BaseUrl, "v2/eInvoice/ewaybill", extraDataAsHeader) /*GetHttpClient(BaseUrl, eInvoiceAuthKey)*/)
                                {
                                    string badRequestResponse = "";
                                    responseEwayBillIRNDCList = client.PostAsyncWithHandleError<List<EwayResponceDc>>(new List<ewaybillirnParam> { ewayBillPostDc }, out badRequestResponse);
                                    //var clearTaxIntegrations = authContext.ClearTaxIntegrations.FirstOrDefault(x => x.OrderId == item.OrderId && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true);
                                    if (!string.IsNullOrEmpty(badRequestResponse))
                                    {
                                        ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                        {
                                            CreateDate = DateTime.Now,
                                            IsActive = true,
                                            Type = "Response",
                                            OrderId = item.OrderId,
                                            Json = JsonConvert.SerializeObject(badRequestResponse),
                                            Url = BaseUrl
                                        };
                                        errorMessage = badRequestResponse;
                                        ewayResponceDc.ClearTaxResponce = clearTaxReqResps;
                                        //if (clearTaxIntegrations != null)
                                        //{
                                        //    clearTaxIntegrations.RequestId = ctrp.Id;
                                        //    clearTaxIntegrations.ResponseId = clearTaxReqResp.Id;
                                        //    authContext.Entry(clearTaxIntegrations).State = EntityState.Modified;
                                        //}
                                        //status = false;
                                    }
                                    else
                                    {
                                        responseEwayBillbyIRNDC = responseEwayBillIRNDCList[0];
                                        if (responseEwayBillbyIRNDC != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                                        {
                                            //if (clearTaxIntegrations != null)
                                            //{
                                            //    clearTaxIntegrations.RequestId = ctrp.Id;
                                            //    clearTaxIntegrations.ResponseId = clearTaxReqResp.Id;
                                            //    authContext.Entry(clearTaxIntegrations).State = EntityState.Modified;
                                            //}
                                            ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                            {
                                                CreateDate = DateTime.Now,
                                                IsActive = true,
                                                Type = "Response",
                                                OrderId = item.OrderId,
                                                Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC),
                                                Url = BaseUrl
                                            };
                                            errorMessage = badRequestResponse;
                                            ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                            ewayResponceDc.ewayResponceDc = (responseEwayBillbyIRNDC);

                                        }
                                        else
                                        {
                                            ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                            {
                                                CreateDate = DateTime.Now,
                                                IsActive = true,
                                                Type = "Response",
                                                OrderId = item.OrderId,
                                                Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                                                Url = BaseUrl
                                            };
                                            ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                            //status = false;
                                            //errorMessage = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message));
                                        }
                                        responseEwayBillbyIRNDCAll.Add(ewayResponceDc);
                                    }
                                }
                            }
                            catch (Exception exe)
                            {
                                errorMessage = exe.ToString();
                            }
                        });
                        //var res = InsertEwayBillRespone(item.OrderId, authContext);

                        //ClearTaxReqResp clearTaxReqResp = new ClearTaxReqResp()
                        //{
                        //    CreateDate = DateTime.Now,
                        //    IsActive = true,
                        //    Type = "Response",
                        //    OrderId = item.OrderId,
                        //    //Json = JsonConvert.SerializeObject(responseIRNDC),
                        //    Url = BaseUrl
                        //};
                        //extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                        //        {
                        //            new KeyValuePair<string, IEnumerable<string>>
                        //            (
                        //              "x-cleartax-auth-token",
                        //              new List<string> { authtoken }
                        //            ),
                        //            new KeyValuePair<string, IEnumerable<string>>
                        //            (
                        //              "gstin",
                        //              new List<string> { gstin }
                        //            ),
                        //        };
                        //try
                        //{
                        //    using (var client = new GenericRestHttpClient<List<ewaybillirnParam>, string>(BaseUrl, "v2/eInvoice/ewaybill", extraDataAsHeader) /*GetHttpClient(BaseUrl, eInvoiceAuthKey)*/)
                        //    {
                        //        string badRequestResponse = "";
                        //        responseEwayBillIRNDCList = client.PostAsyncWithHandleError<List<EwayResponceDc>>(new List<ewaybillirnParam> { ewayBillPostDc }, out badRequestResponse);
                        //        var clearTaxIntegrations = authContext.ClearTaxIntegrations.FirstOrDefault(x => x.OrderId == item.OrderId && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true);
                        //        if (!string.IsNullOrEmpty(badRequestResponse))
                        //        {
                        //            clearTaxReqResp.Json = badRequestResponse;
                        //            errorMessage = badRequestResponse;
                        //            authContext.ClearTaxReqResps.Add(clearTaxReqResp);
                        //            if (clearTaxIntegrations != null)
                        //            {
                        //                clearTaxIntegrations.RequestId = ctrp.Id;
                        //                clearTaxIntegrations.ResponseId = clearTaxReqResp.Id;
                        //                authContext.Entry(clearTaxIntegrations).State = EntityState.Modified;
                        //            }
                        //            status = false;
                        //        }
                        //        else
                        //        {
                        //            responseEwayBillbyIRNDC = responseEwayBillIRNDCList[0];
                        //            clearTaxReqResp.Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC);
                        //            authContext.ClearTaxReqResps.Add(clearTaxReqResp);
                        //            if (responseEwayBillbyIRNDC != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                        //            {
                        //                if (clearTaxIntegrations != null)
                        //                {
                        //                    clearTaxIntegrations.RequestId = ctrp.Id;
                        //                    clearTaxIntegrations.ResponseId = clearTaxReqResp.Id;
                        //                    authContext.Entry(clearTaxIntegrations).State = EntityState.Modified;
                        //                }
                        //                var orderDispatchedMasters = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == item.OrderId);
                        //                if (orderDispatchedMasters != null)
                        //                {
                        //                    orderDispatchedMasters.EwayBillNumber = Convert.ToString(responseEwayBillbyIRNDC.govt_response.EwbNo);
                        //                    authContext.Entry(orderDispatchedMasters).State = EntityState.Modified;
                        //                }
                        //                OrderEwayBill bill = new OrderEwayBill
                        //                {
                        //                    OrderId = item.OrderId,
                        //                    EwayBillNo = Convert.ToString(responseEwayBillbyIRNDC.govt_response.EwbNo),
                        //                    EwayBillDate = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbDt),
                        //                    EwayBillValidTill = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbValidTill),
                        //                    IsCancelEwayBill = false,
                        //                    EwayBillCancelDate = null,
                        //                    IsActive = true,
                        //                    CreateDate = DateTime.Now,
                        //                    CreatedBy = userid,
                        //                    ModifiedBy = null,
                        //                    ModifiedDate = null,
                        //                    CustomerType = 0,//B2B
                        //                    VehicleNumber = !string.IsNullOrEmpty(ewayBillBackendDc.vehicleno) ? ewayBillBackendDc.vehicleno.Trim().Replace(" ", "").ToUpper().ToString() : item.VehicleNo.Trim().Replace(" ", "").ToUpper().ToString()
                        //                };
                        //                authContext.OrderEwayBills.Add(bill);
                        //                authContext.Commit();
                        //                status = true;
                        //            }
                        //            else
                        //            {
                        //                status = false;
                        //                errorMessage = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message));
                        //            }
                        //        }
                        //    }
                        //}
                        //catch (Exception exe)
                        //{
                        //    errorMessage = exe.ToString();
                        //}

                        if (parellelResult.IsCompleted && responseEwayBillbyIRNDCAll.Any() && responseEwayBillbyIRNDCList != null)
                        {
                            var OrderIds = responseEwayBillbyIRNDCAll.Select(x => x.OrderId).Distinct().ToList();
                            var orderDispatchedMasters = authContext.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                            var clearTaxIntegrations = authContext.ClearTaxIntegrations.Where(x => OrderIds.Contains(x.OrderId) && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true).ToList();
                            foreach (var item in responseEwayBillbyIRNDCAll)
                            {
                                var clearTaxIntegration1 = clearTaxIntegrations.FirstOrDefault(x => x.OrderId == item.OrderId);
                                var orderDispatchedMaster1 = orderDispatchedMasters.FirstOrDefault(x => x.OrderId == item.OrderId);
                                if (item.ewayResponceDc != null && item.ewayResponceDc.govt_response.Success == "Y")
                                {
                                    if (orderDispatchedMaster1 != null)
                                    {
                                        orderDispatchedMaster1.EwayBillNumber = Convert.ToString(item.ewayResponceDc.govt_response.EwbNo);
                                        authContext.Entry(orderDispatchedMaster1).State = EntityState.Modified;
                                    }
                                    OrderEwayBill bill = new OrderEwayBill
                                    {
                                        OrderId = item.OrderId,
                                        EwayBillNo = Convert.ToString(item.ewayResponceDc.govt_response.EwbNo),
                                        EwayBillDate = Convert.ToDateTime(item.ewayResponceDc.govt_response.EwbDt),
                                        EwayBillValidTill = Convert.ToDateTime(item.ewayResponceDc.govt_response.EwbValidTill),
                                        IsCancelEwayBill = false,
                                        EwayBillCancelDate = null,
                                        IsActive = true,
                                        CreateDate = DateTime.Now,
                                        CreatedBy = userid,
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        CustomerType = 0,//B2B
                                        VehicleNumber = !string.IsNullOrEmpty(vehicleNumber) ? vehicleNumber.Trim().Replace(" ", "").ToUpper().ToString() : vehicleNo.Trim().Replace(" ", "").ToUpper().ToString()
                                    };
                                    authContext.OrderEwayBills.Add(bill);
                                    authContext.ClearTaxIntegrations.Add(item.clearTaxIntegration);
                                    authContext.ClearTaxReqResps.Add(item.ClearTaxRequest);
                                    authContext.ClearTaxReqResps.Add(item.ClearTaxResponce);
                                    authContext.Commit();
                                    if (clearTaxIntegration1 == null)
                                    {
                                        var clearTax = authContext.ClearTaxIntegrations.Where(x => x.OrderId == item.OrderId && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true).FirstOrDefault();
                                        if (clearTax != null)
                                        {
                                            clearTax.RequestId = item.ClearTaxRequest.Id;
                                            clearTax.ResponseId = item.ClearTaxResponce.Id;
                                            authContext.Entry(clearTax).State = EntityState.Modified;
                                        }
                                    }
                                    else
                                    {
                                        clearTaxIntegration1.RequestId = item.ClearTaxRequest.Id;
                                        clearTaxIntegration1.ResponseId = item.ClearTaxResponce.Id;
                                        authContext.Entry(clearTaxIntegration1).State = EntityState.Modified;
                                    }
                                    status = true;
                                }
                                else
                                {
                                    authContext.ClearTaxIntegrations.Add(item.clearTaxIntegration);
                                    authContext.ClearTaxReqResps.Add(item.ClearTaxRequest);
                                    authContext.ClearTaxReqResps.Add(item.ClearTaxResponce);
                                    authContext.Commit();
                                    if (clearTaxIntegration1 == null)
                                    {
                                        var clearTax = authContext.ClearTaxIntegrations.Where(x => x.OrderId == item.OrderId && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true).FirstOrDefault();
                                        if (clearTax != null)
                                        {
                                            clearTax.RequestId = item.ClearTaxRequest.Id;
                                            clearTax.ResponseId = item.ClearTaxResponce.Id;
                                            authContext.Entry(clearTax).State = EntityState.Modified;
                                        }
                                    }
                                    else
                                    {
                                        clearTaxIntegration1.RequestId = item.ClearTaxRequest.Id;
                                        clearTaxIntegration1.ResponseId = item.ClearTaxResponce.Id;
                                        authContext.Entry(clearTaxIntegration1).State = EntityState.Modified;
                                    }
                                    if (item.ewayResponceDc != null)
                                    {
                                        errorMessage = JsonConvert.SerializeObject(item.ewayResponceDc.govt_response.ErrorDetails.Select(x => x.error_message));
                                    }
                                    errorMessageList.Add(errorMessage);
                                    status = false;
                                }
                                authContext.Commit();
                            }
                        }
                    }
                    if (B2CCustomer != null && B2CCustomer.Any())
                    {
                        //List<OrderEwayBillsGenError> orderEwayBillsGenErrors = new List<OrderEwayBillsGenError>();
                        //List<OrderEwayBill> orderEwayBills = new List<OrderEwayBill>();
                        //B2CResponseDc b2CResponseDc = new B2CResponseDc();
                        //authTokenGenerationDc = this.B2CEwayBillGenerate(userid, authContext, gstin);
                        //var B2Ccustorderids = B2CCustomer.Select(x => x.OrderId).Distinct().ToList();
                        //var orderIdDtw = new DataTable();
                        //orderIdDtw.Columns.Add("IntValue");
                        //foreach (var item in B2Ccustorderids)
                        //{
                        //    var dr = orderIdDtw.NewRow();
                        //    dr["IntValue"] = item;
                        //    orderIdDtw.Rows.Add(dr);
                        //}

                        //var orderids = new SqlParameter
                        //{
                        //    ParameterName = "orderid",
                        //    SqlDbType = SqlDbType.Structured,
                        //    TypeName = "dbo.IntValues",
                        //    Value = orderIdDtw
                        //};
                        //List<GetCustomerGenerateEwayBillDC> Ewaydata = authContext.Database.SqlQuery<GetCustomerGenerateEwayBillDC>("exec generateewaybilldata @orderid", orderids).ToList();
                        //Item Details
                        //var items = authContext.OrderDispatchedDetailss.Where(x => B2Ccustorderids.Contains(x.OrderId)).ToList();
                        //List<int> itemMultiMrpIds = items.Select(z => z.ItemMultiMRPId).Distinct().ToList();
                        //List<int> itemIds = items.Select(z => z.ItemId).Distinct().ToList();
                        //var mrpList = authContext.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();
                        //var itemMasterslist = authContext.itemMasters.Where(x => itemIds.Contains(x.ItemId)).ToList();
                        //var ewayBillErrorLists = authContext.EwayBillErrorLists.ToList();
                        //ParallelLoopResult parellelResult1 = Parallel.ForEach(B2CCustomer, async (item) =>
                        //{
                        //foreach (var item in B2CCustomer)
                        //{
                        //using (var db = new AuthContext())
                        //{
                        //    authTokenGenerationDc = this.B2CEwayBillGenerate(userid, db, gstin);
                        //}

                        foreach (var item in B2CCustomer)
                        {
                            Ewaygenerateparam ewaygenerateparam = new Ewaygenerateparam()
                            {
                                orderid = item.OrderId,//B2CCustomer.FirstOrDefault().OrderId,
                                vehicleno = !string.IsNullOrEmpty(vehicleNumber) ? vehicleNumber.Trim().Replace(" ", "") : item.VehicleNo.Trim().Replace(" ", "").ToUpper().ToString(),
                                transportergst = ewayBillBackendDc.TransportGST,
                                transportername = ewayBillBackendDc.TransportName,
                                distance = item.WarehousePincode.Equals(item.ZipCode) == true ? 100 : ewayBillBackendDc.distance,
                                topincode = !string.IsNullOrEmpty(item.ZipCode) ? Convert.ToInt32(item.ZipCode) : 0,//Customer
                                transporterdocdate = "",
                                transporterdocno = ""
                            };
                            //var orderdata = Ewaydata.Where(x => x.OrderId == B2CCustomer.FirstOrDefault().OrderId).ToList();
                            //b2CResponseDc = GeneratedEwayBill(authTokenGenerationDc.EWB_Response_authtoken, authTokenGenerationDc.DecryptedSek, ewaygenerateparam, authTokenGenerationDc.configres, userid, orderdata, items, itemMasterslist, mrpList, orderEwayBillsGenErrors, ewayBillErrorLists, orderEwayBills);
                            // Generate Eway Bill B2c NoN IRN
                            status = await GeneratedEwayBillNonIRN(authContext, ewaygenerateparam, userid);
                        }
                        //});
                        //if (parellelResult1.IsCompleted)
                        //{
                        //authContext.OrderEwayBills.AddRange(b2CResponseDc.orderEwayBills);
                        //authContext.OrderEwayBillsGenErrors.AddRange(b2CResponseDc.orderEwayBillsGenErrors);
                        //b2CResponseDc.orderEwayBills.ForEach(x =>
                        //{
                        //    var ewayupdate = authContext.OrderDispatchedMasters.Where(a => a.OrderId == x.OrderId).FirstOrDefault(); //order id
                        //    if (ewayupdate != null)
                        //    {
                        //        ewayupdate.EwayBillNumber = x.EwayBillNo;
                        //        authContext.Entry(ewayupdate).State = EntityState.Modified;
                        //    }
                        //});
                        //status = b2CResponseDc.orderEwayBillsGenErrors.Any() && b2CResponseDc.orderEwayBillsGenErrors.Count() > 0 ? false : true;
                        //}
                    }
                    TripPlannerVechicleAttandance vehicleAttandance = null;
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    GetReplaceVehicleListDc ReplaceVehicleList = null;
                    ReplaceVehicleList = tripPlannerHelper.GetReplaceVehicle(ewayBillBackendDc.TripPlannerConfirmedMasterid, authContext, null);

                    if (ReplaceVehicleList != null && ReplaceVehicleList.IsAlreadyEwaybillGenerate)
                    {
                        vehicleAttandance = authContext.TripPlannerVechicleAttandanceDb.FirstOrDefault(x => x.VehicleMasterId == ewayBillBackendDc.VehicleId && x.AttendanceDate == ReplaceVehicleList.TripDate && x.IsActive && x.IsDeleted == false);
                        if (vehicleAttandance != null)
                        {
                            vehicleAttandance.IsReplacementVehicleNo = ewayBillBackendDc.IsReplacementVehicleNo;
                            vehicleAttandance.ReplacementVehicleNo = ewayBillBackendDc.ReplacementVehicleNo;
                            vehicleAttandance.ModifiedDate = DateTime.Now;
                            vehicleAttandance.ModifiedBy = userid;
                            authContext.Entry(vehicleAttandance).State = EntityState.Modified;
                        }
                    }
                    authContext.Commit();
                }
            }
            return status;
        }
        public async Task<EwaybillBackendResponceDc> CancelEwayBillbyIRN(CancelRequestParam cancelewaybillirn, AuthContext context, int userid, Warehouse warehouse, CompanyDetails CompanyDetails)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;
                CancelParam cancelewaybillirndc = CancelEwayBillPostDc(cancelewaybillirn);
                //ewayBillPostDc.Irn = "df63a28b7575f39818b478a4c52d6abc7aa7f4c17e956af3cd4a856adafaf415";
                //var res1 = await InsertEwayBillRespone(cancelewaybillirn.orderid, context);
                var RequestjsonString = JsonConvert.SerializeObject(cancelewaybillirndc);
                //ClearTaxReqResp ctrp = new ClearTaxReqResp()
                //{
                //    CreateDate = DateTime.Now,
                //    IsActive = true,
                //    Type = "Request",
                //    OrderId = cancelewaybillirn.orderid,
                //    Json = RequestjsonString,
                //    Url = BaseUrl
                //};
                //context.ClearTaxReqResps.Add(ctrp);

                //ClearTaxReqResp clearTaxReqResp = new ClearTaxReqResp()
                //{
                //    CreateDate = DateTime.Now,
                //    IsActive = true,
                //    Type = "Response",
                //    OrderId = cancelewaybillirn.orderid,
                //    //Json = JsonConvert.SerializeObject(responseIRNDC),
                //    Url = BaseUrl
                //};


                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2BCancelApi",
                    Orderid = cancelewaybillirn.orderid,
                    ErrorDescription = RequestjsonString,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);
                var client = new RestClient(BaseUrl + "/v2/eInvoice/ewaybill/cancel");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Cleartax-Auth-Token", authtoken);
                request.AddHeader("gstin", warehouse.GSTin);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                CancelEwayResponse RESS = JsonConvert.DeserializeObject<CancelEwayResponse>(response.Content);

                var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2BCancelApi" && x.Orderid == cancelewaybillirn.orderid && x.IsActive == true && x.IsDeleted == false);
                if (orderEwayBillsGenErrors == null)
                {
                    OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                    ewayerror.ErrorCode = 0;
                    ewayerror.Orderid = cancelewaybillirn.orderid;
                    ewayerror.IsActive = true;
                    ewayerror.IsDeleted = false;
                    ewayerror.Remarks = "B2BCancelApi";
                    ewayerror.ModifiedBy = null;
                    ewayerror.ModifiedDate = null;
                    ewayerror.ErrorDescription = JsonConvert.SerializeObject(RESS.errorDetails);
                    ewayerror.CreatedBy = userid;
                    ewayerror.CreatedDate = DateTime.Now;
                    ewayerror.Type = "Response";
                    context.OrderEwayBillsGenErrors.Add(ewayerror);
                }
                if (RESS.ewbStatus == "CANCELLATION_FAILED" && RESS.errorDetails != null)
                {
                    if (orderEwayBillsGenErrors != null)
                    {
                        orderEwayBillsGenErrors.ErrorDescription = JsonConvert.SerializeObject(RESS.errorDetails);
                        orderEwayBillsGenErrors.ModifiedBy = userid;
                        orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                        context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                    }
                    res.Message = "Eway Bill Not Canceled";
                    res.status = false;
                    return res;
                }
                else if (RESS.ewbStatus == "CANCELLED" && RESS.errorDetails == null) //response ==true
                {
                    if (orderEwayBillsGenErrors != null)
                    {
                        orderEwayBillsGenErrors.ErrorDescription = JsonConvert.SerializeObject(RESS.errorDetails);
                        orderEwayBillsGenErrors.ModifiedBy = userid;
                        orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                        context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                    }
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == cancelewaybillirn.orderid && !string.IsNullOrEmpty(x.EwayBillNo) && x.IsCancelEwayBill == false).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                    var orderDispatchedMaster = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == cancelewaybillirn.orderid && !string.IsNullOrEmpty(x.EwayBillNumber)); //order id
                    if (ewayupdate != null)
                    {
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.ewbNumber);
                        ewayupdate.ModifiedDate = DateTime.Now;
                        ewayupdate.ModifiedBy = userid;
                        ewayupdate.IsCancelEwayBill = true;
                        ewayupdate.EwayBillCancelDate = DateTime.Now;
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    if (orderDispatchedMaster != null)
                    {
                        orderDispatchedMaster.EwayBillNumber = null;
                        context.Entry(orderDispatchedMaster).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "EwayBillCanceled  =" + RESS.ewbNumber + ",orderId=" + cancelewaybillirn.orderid;
                    res.status = true;
                    return res;
                }
                else
                {
                    res.Message = RESS.errorDetails.error_message;
                    res.status = false;
                    return res;
                }
            }
            else
            {
                res.Message = "Eway Bill Service Not Started!!";
                res.status = false;
                return res;
            }
            return res;
        }
        public async Task<EwaybillBackendResponceDc> ExtendEwayBillbyIRN(ExtendRequestParam extendRequestParam, AuthContext context, int userid, Warehouse warehouse, int orderid, CompanyDetails CompanyDetails)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;
                var client = new RestClient(BaseUrl + "/v1/ewaybill/update?action=EXTEND_VALIDITY");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("X-Cleartax-Auth-Token", authtoken);
                request.AddHeader("gstin", warehouse.GSTin);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                var newJson = JsonConvert.SerializeObject(extendRequestParam);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2BExtendApi",
                    Orderid = orderid,
                    ErrorDescription = newJson,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);

                request.AddParameter("application/json", newJson, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                ExtendResponseDc RESS = JsonConvert.DeserializeObject<ExtendResponseDc>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK && RESS.errors != null)
                {
                    var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2BExtendApi" && x.Orderid == orderid && x.IsActive == true && x.IsDeleted == false);
                    if (orderEwayBillsGenErrors == null)
                    {
                        OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                        ewayerror.ErrorCode = 0;
                        ewayerror.Orderid = orderid;
                        ewayerror.IsActive = true;
                        ewayerror.IsDeleted = false;
                        ewayerror.Remarks = "B2BExtendApi";
                        ewayerror.ModifiedBy = null;
                        ewayerror.ModifiedDate = null;
                        ewayerror.ErrorDescription = response.Content;
                        ewayerror.CreatedBy = userid;
                        ewayerror.CreatedDate = DateTime.Now;
                        ewayerror.Type = "Response";
                        context.OrderEwayBillsGenErrors.Add(ewayerror);
                    }
                    else
                    {
                        orderEwayBillsGenErrors.ErrorDescription = response.Content;
                        orderEwayBillsGenErrors.ModifiedBy = userid;
                        orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                        context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                    }
                    context.Commit();

                    res.Message = "EwayBill  Not Extended.....try again";
                    res.status = false;
                    return res;
                }
                else if (response.StatusCode == HttpStatusCode.OK && RESS.errors == null)
                {

                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                    {
                        ErrorCode = 0,
                        Orderid = orderid,
                        IsActive = true,
                        IsDeleted = false,
                        Remarks = "B2BExtendApi",
                        ModifiedBy = null,
                        ModifiedDate = null,
                        ErrorDescription = response.Content,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Type = "Response"
                    };
                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == orderid && !string.IsNullOrEmpty(x.EwayBillNo)).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                    if (ewayupdate != null)
                    {
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                        ewayupdate.ModifiedDate = Convert.ToDateTime(RESS.UpdatedDate, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);//Convert.ToDateTime(RESS.UpdatedDate);
                        ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat); //Convert.ToDateTime(RESS.ValidUpto);
                        ewayupdate.IsExtendEwayBill = true;
                        ewayupdate.EwayBillExtendDate = DateTime.Now;
                        ewayupdate.VehicleNumber = extendRequestParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "EwayBillExtended";
                    res.status = true;
                    return res;
                }
                //Console.WriteLine(response.Content);
            }
            else
            {
                res.Message = "Eway Bill Service Not Started!!";
                res.status = false;
                return res;
            }
            return res;
        }
        //updateVEhicleB2B
        public async Task<EwaybillBackendResponceDc> UpdateVehiclePartBIRN(UpdatePartBRequest updatePartBParam, AuthContext context, int userid, Warehouse warehouse, CompanyDetails CompanyDetails)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;
                var client = new RestClient(BaseUrl + "/v1/ewaybill/update?action=PARTB");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("X-Cleartax-Auth-Token", authtoken);
                request.AddHeader("gstin", warehouse.GSTin);
                request.AddHeader("Content-Type", "application/json");
                var newJson = JsonConvert.SerializeObject(updatePartBParam);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2BUpdateVehicleApi",
                    Orderid = updatePartBParam.orderid,
                    ErrorDescription = newJson,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);

                request.AddParameter("application/json", newJson, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                UpdatePartBResponse RESS = JsonConvert.DeserializeObject<UpdatePartBResponse>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK && RESS.errors != null)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2BUpdateVehicleApi" && x.Orderid == updatePartBParam.orderid && x.IsActive == true && x.IsDeleted == false);
                        if (orderEwayBillsGenErrors == null)
                        {
                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                            ewayerror.ErrorCode = 0;
                            ewayerror.Orderid = updatePartBParam.orderid;
                            ewayerror.IsActive = true;
                            ewayerror.IsDeleted = false;
                            ewayerror.Remarks = "B2BUpdateVehicleApi";
                            ewayerror.ModifiedBy = null;
                            ewayerror.ModifiedDate = null;
                            ewayerror.ErrorDescription = response.Content;
                            ewayerror.CreatedBy = userid;
                            ewayerror.CreatedDate = DateTime.Now;
                            ewayerror.Type = "Response";
                            context.OrderEwayBillsGenErrors.Add(ewayerror);
                        }
                        else
                        {
                            orderEwayBillsGenErrors.ErrorDescription = response.Content;
                            orderEwayBillsGenErrors.ModifiedBy = userid;
                            orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                            context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                        }
                        context.Commit();
                    }
                    res.Message = string.Join(",", RESS.errors.Select(x => x.error_message));
                    res.status = false;
                    return res;
                }
                else if (response.StatusCode == HttpStatusCode.OK && RESS.errors == null)
                {
                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                    {
                        ErrorCode = 0,
                        Orderid = updatePartBParam.orderid,
                        IsActive = true,
                        IsDeleted = false,
                        Remarks = "B2BUpdateVehicleApi",
                        ModifiedBy = null,
                        ModifiedDate = null,
                        ErrorDescription = response.Content,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Type = "Response"
                    };
                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == updatePartBParam.orderid).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                    if (ewayupdate != null)
                    {
                        ewayupdate.VehicleNumber = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                        ewayupdate.ModifiedDate = Convert.ToDateTime(RESS.UpdatedDate, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);//Convert.ToDateTime(RESS.UpdatedDate);
                        ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);// Convert.ToDateTime(RESS.ValidUpto);
                        ewayupdate.ModifiedBy = userid;
                        ewayupdate.ModifiedDate = DateTime.Now;
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "Vehicle Updated Successfully";
                    res.status = true;
                    return res;
                }
                //Console.WriteLine(response.Content);
            }
            else
            {
                res.Message = "Eway Bill Service Not Started!!";
                res.status = false;
                return res;
            }
            return res;
        }
        #endregion


        public EwaybillBackendResponceDc UpdateTransporterPartA(UpdatePartA updatePartAParam, AuthContext context, int userid, Warehouse warehouse, CompanyDetails CompanyDetails)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;
                var client = new RestClient(BaseUrl + "/v1/ewaybill/update?action=UPDATE_TRANSPORTER_ID");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("X-Cleartax-Auth-Token", authtoken);
                request.AddHeader("gstin", warehouse.GSTin);
                request.AddHeader("Content-Type", "application/json");
                var newJson = JsonConvert.SerializeObject(updatePartAParam);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2BUpdateTransporterIdApi",
                    Orderid = updatePartAParam.Orderid,
                    ErrorDescription = newJson,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);

                request.AddParameter("application/json", newJson, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                UpdatePartBResponse RESS = JsonConvert.DeserializeObject<UpdatePartBResponse>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK && RESS.errors != null)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2BUpdateTransporterIdApi" && x.Orderid == updatePartAParam.Orderid && x.IsActive == true && x.IsDeleted == false);
                        if (orderEwayBillsGenErrors == null)
                        {
                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                            ewayerror.ErrorCode = 0;
                            ewayerror.Orderid = updatePartAParam.Orderid;
                            ewayerror.IsActive = true;
                            ewayerror.IsDeleted = false;
                            ewayerror.Remarks = "B2BUpdateTransporterIdApi";
                            ewayerror.ModifiedBy = null;
                            ewayerror.ModifiedDate = null;
                            ewayerror.ErrorDescription = response.Content;
                            ewayerror.CreatedBy = userid;
                            ewayerror.CreatedDate = DateTime.Now;
                            ewayerror.Type = "Response";
                            context.OrderEwayBillsGenErrors.Add(ewayerror);
                        }
                        else
                        {
                            orderEwayBillsGenErrors.ErrorDescription = response.Content;
                            orderEwayBillsGenErrors.ModifiedBy = userid;
                            orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                            context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                        }
                        context.Commit();
                    }
                    res.Message = string.Join(",", RESS.errors.Select(x => x.error_message));
                    res.status = false;
                    return res;
                }
                else if (response.StatusCode == HttpStatusCode.OK && RESS.errors == null)
                {
                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                    {
                        ErrorCode = 0,
                        Orderid = updatePartAParam.Orderid,
                        IsActive = true,
                        IsDeleted = false,
                        Remarks = "B2BUpdateTransporterIdApi",
                        ModifiedBy = null,
                        ModifiedDate = null,
                        ErrorDescription = response.Content,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Type = "Response"
                    };
                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == updatePartAParam.Orderid).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                    if (ewayupdate != null)
                    {
                        //ewayupdate.VehicleNumber = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                        ewayupdate.ModifiedDate = Convert.ToDateTime(RESS.UpdatedDate, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);//Convert.ToDateTime(RESS.UpdatedDate);
                        //ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);// Convert.ToDateTime(RESS.ValidUpto);
                        ewayupdate.ModifiedBy = userid;
                        ewayupdate.ModifiedDate = DateTime.Now;
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "Transporter GST Updated Successfully";
                    res.status = true;
                    return res;
                }
                //Console.WriteLine(response.Content);
            }
            else
            {
                res.Message = "Eway Bill Service Not Started!!";
                res.status = false;
                return res;
            }
            return res;
        }


        #region  GetEwayBillPdf
        public async Task<EwaybillBackendResponceDc> GetEwayBillPdf(PostPDFParam postPDFParam, AuthContext context, int userid)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            var compantDetails = GetcompanyDetails(context);
            if (compantDetails != null)
            {
                BaseUrl = compantDetails.eInvoiceBaseUrl;
                authtoken = compantDetails.eInvoiceAuthKey;
                //int warehouseId = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == postPDFParam.OrderId).WarehouseId;

                var warehouseId = postPDFParam.Apitypes == 0 ? await context.OrderDispatchedMasters.Where(x => x.OrderId == postPDFParam.OrderId).Select(x => x.WarehouseId).FirstOrDefaultAsync() : await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == postPDFParam.OrderId).Select(x => x.RequestToWarehouseId.Value).FirstOrDefaultAsync();
                if (warehouseId > 0)
                {
                    var wh = this.GetWarehouseDetails(warehouseId, context);
                    string PdfSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/EwayBillInvoice/");
                    if (!Directory.Exists(PdfSavePath))
                        Directory.CreateDirectory(PdfSavePath);
                    if ((Directory.Exists(PdfSavePath + postPDFParam.ewb_numbers[0] + ".pdf")))
                    {
                        res.status = true;
                        res.Message = "EWayBill PDF Genrated!!";
                        res.EwayBillPDF = "/ExcelGeneratePath/EwayBillInvoice/" + postPDFParam.ewb_numbers[0] + ".pdf";
                        return res;
                    }
                    if (wh != null)
                    {
                        if (postPDFParam.custometType == 0)//B2B
                        {
                            res = await this.B2BEwayBillPDFGenrate(postPDFParam, PdfSavePath, wh);
                        }
                        else
                        {
                            res = await this.B2CEwayBillPDFGenerate(postPDFParam.ewb_numbers[0].ToString(), userid, context, wh);
                        }
                    }
                }
            }
            return res;
        }
        public async Task<EwaybillBackendResponceDc> B2BEwayBillPDFGenrate(PostPDFParam postPDFParam, string PdfSavePath, Warehouse wh)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            using (var httpClient = new HttpClient())
            {
                GetPDFParam getPDFParam = new GetPDFParam()
                {
                    ewb_numbers = postPDFParam.ewb_numbers,
                    print_type = "DETAILED"
                };
                var newJson = JsonConvert.SerializeObject(getPDFParam);
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), BaseUrl + "/v2/eInvoice/ewaybill/print?format=PDF"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("NoEncryption", "1");
                    request.Headers.TryAddWithoutValidation("X-Cleartax-Auth-Token", authtoken);
                    request.Headers.TryAddWithoutValidation("gstin", wh.GSTin);
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    request.Content = new StringContent(newJson);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var Response = await httpClient.SendAsync(request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string filePath = PdfSavePath + postPDFParam.ewb_numbers[0] + ".pdf";
                        using (var file = System.IO.File.Create(filePath))
                        { // create a new file to write to
                            var contentStream = await Response.Content.ReadAsStreamAsync(); // get the actual content stream
                            await contentStream.CopyToAsync(file); // copy that stream to the file stream
                        }
                        res.status = true;
                        res.Message = "EWayBill PDF Genrated!!";
                        res.EwayBillPDF = "/ExcelGeneratePath/EwayBillInvoice/" + postPDFParam.ewb_numbers[0] + ".pdf";
                        return res;
                    }
                    else
                    {
                        res.status = false;
                        res.Message = "EWayBill PDF Not Genrated!!";
                        res.EwayBillPDF = "";
                        return res;
                    }
                }
            }
            return res;
        }
        #endregion

        #region   B2CPDF 
        public async Task<EwaybillBackendResponceDc> B2CEwayBillPDFGenerate(string ewbNo, int userid, AuthContext context, Warehouse wh)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();

            var configres = GetEwayBillConfiguration(wh.GSTin, context);
            if (configres != null)
            {
                string EWB_New_Generate_url = configres.URL + "/v1.03/ewayapi/GetEwayBill?ewbNo=" + ewbNo + "";
                var AuthTokenRequests = context.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (AuthTokenRequests != null)
                {
                    bool IsAuthGenrate = true;
                    string EWB_Response_authtoken = string.Empty;
                    string EWB_Response_sek = string.Empty;
                    string generateSecureKey = string.Empty;
                    var date = AuthTokenRequests.CreatedDate;
                    DateTime next = DateTime.Today; //new DateTime(date.Year, date.Month, date.Day, 00, 00, 00);            
                    var clockOut = DateTime.Today.AddDays(1).AddTicks(-1);
                    List<DateTime> add = new List<DateTime>();
                    add.Add(next);
                    while ((next = next.AddHours(6)) < clockOut)
                    {
                        add.Add(next);
                    }
                    add.ForEach(x =>
                    {
                        if (x >= date)
                        {
                            IsAuthGenrate = false;
                        };
                    });

                    if (!IsAuthGenrate)
                    {
                        ////DateTime oldDate = new DateTime(2011, 12, 6);
                        //DateTime oldDate = AuthTokenRequests.CreatedDate;
                        //DateTime newDate = DateTime.Now;
                        //// Difference in days, hours
                        //TimeSpan duration = newDate - oldDate;
                        //// Difference in days.
                        //int differenceInDays = duration.Days;
                        //int differenceInHours = duration.Hours;
                        EWB_Response_authtoken = (string)AuthTokenRequests.AuthToken;
                        EWB_Response_sek = (string)AuthTokenRequests.sek;
                        generateSecureKey = (string)AuthTokenRequests.generateSecureKey;
                    }
                    else
                    {
                        AuthKeyReturn authKeyReturn = this.GenerateAuth(userid, configres);
                        EWB_Response_sek = authKeyReturn.EWB_Response_sek;
                        EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                        generateSecureKey = authKeyReturn.generateSecureKey;
                    }
                }
                else
                {
                    res.Message = "AuthToken Not Generated";
                    res.status = false;
                    res.EwayBillPDF = "";
                    return res;
                }
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Generate_url);
                request.Method = "GET";
                request.KeepAlive = true;
                request.AllowAutoRedirect = false;
                request.Accept = "*/*";
                request.ContentType = "application/json";
                request.Headers.Add("client-id", configres.clientID);
                request.Headers.Add("client-secret", configres.ClintSecretID);
                request.Headers.Add("gstin", wh.GSTin);
                request.Headers.Add("authtoken", AuthTokenRequests.AuthToken);

                string straesKeySaveing = (string)AuthTokenRequests.generateSecureKey;
                string EWB_Response_sekNew = (string)AuthTokenRequests.sek;
                byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                string EncryptedSekNew = EWB_Response_sekNew;
                string DecryptedSekNew = this.DecryptBySymmerticKey(EncryptedSekNew, _aeskeySaveing_Values);

                WebResponse response = await request.GetResponseAsync();  //response from Api

                JavaScriptSerializer serial1 = new JavaScriptSerializer();
                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var newdata = serial1.Deserialize<EwayBillApiResponse>(result);

                if (Convert.ToInt32(newdata.status) == 1)
                {
                    string rek = this.DecryptBySymmerticKey(newdata.rek, Convert.FromBase64String(DecryptedSekNew));
                    string data = DecryptBySymmerticKey(newdata.data, Convert.FromBase64String(rek));
                    byte[] reqDatabytes = Convert.FromBase64String(data);
                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                    EwaybillDetails ewbres_final = JsonConvert.DeserializeObject<EwaybillDetails>(requestData);

                    //To create HTML template to give pdf

                    string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\EwaybillInvoiceB2C.html");
                    string htmldata = "";
                    string replacetext = "";

                    if (File.Exists(path))
                    {
                        htmldata = File.ReadAllText(path);
                        if (!string.IsNullOrEmpty(htmldata))
                        {
                            replacetext = $" EWay Bill No: {ewbres_final.ewbNo} ";
                            htmldata = htmldata.Replace("{{Ewaybillno}}", replacetext);

                            replacetext = $" Generated Date: {ewbres_final.ewayBillDate} ";
                            htmldata = htmldata.Replace("{{GeneratedDate}}", replacetext);

                            replacetext = $" Generated By: {ewbres_final.fromGstin} ";
                            htmldata = htmldata.Replace("{{GeneratedBy}}", replacetext);

                            replacetext = $" Valid Upto: {ewbres_final.validUpto} ";
                            htmldata = htmldata.Replace("{{ValidUpTo}}", replacetext);

                            replacetext = $" Approx Distance: {ewbres_final.actualDist} ";
                            htmldata = htmldata.Replace("{{ApproxDistance}}", replacetext);

                            replacetext = $" Document Details: {ewbres_final.docNo} ";
                            htmldata = htmldata.Replace("{{DocumentDetails}}", replacetext);

                            replacetext = $" Transaction Type: {ewbres_final.transactionType} ";
                            htmldata = htmldata.Replace("{{TransactionType}}", replacetext);

                            replacetext = $": {ewbres_final.extendedTimes} ";
                            htmldata = htmldata.Replace("{{extendedTimes}}", replacetext);

                            //FROM =>Company Details 
                            replacetext = $" : {ewbres_final.fromGstin} ";
                            htmldata = htmldata.Replace("{{fromGstina}}", replacetext);

                            replacetext = $" : {ewbres_final.fromTrdName} ";
                            htmldata = htmldata.Replace("{{fromTrdName}}", replacetext);

                            replacetext = $" : {ewbres_final.fromPlace} ";
                            htmldata = htmldata.Replace("{{fromPlace}}", replacetext);

                            replacetext = $" : {ewbres_final.fromAddr1} ";
                            htmldata = htmldata.Replace("{{fromAddr1}}", replacetext);  //dispatchFrom

                            //TO =>Customer Details
                            replacetext = $" : {ewbres_final.toGstin} ";
                            htmldata = htmldata.Replace("{{toGstin}}", replacetext);

                            replacetext = $" : {ewbres_final.toTrdName} ";
                            htmldata = htmldata.Replace("{{toTrdName}}", replacetext);

                            replacetext = $": {ewbres_final.toPlace} ";
                            htmldata = htmldata.Replace("{{toPlace}}", replacetext);

                            replacetext = $"  : {ewbres_final.toAddr1} ";
                            htmldata = htmldata.Replace("{{toAddr1}}", replacetext);

                            replacetext = $" : {ewbres_final.toAddr2} ";
                            htmldata = htmldata.Replace("{{toAddr2}}", replacetext);


                            // =>Items Details
                            foreach (var item in ewbres_final.itemList)
                            {
                                replacetext = $" {item.hsnCode} ";
                                htmldata = htmldata.Replace("{{hsnCode}}", replacetext);

                                replacetext = $" {item.productName} ";
                                htmldata = htmldata.Replace("{{productName}}", replacetext);

                                replacetext = $" {item.quantity} ";
                                htmldata = htmldata.Replace("{{quantity}}", replacetext);

                                replacetext = $"{item.qtyUnit} ";
                                htmldata = htmldata.Replace("{{qtyUnit}}", replacetext);

                                var taxSum = item.cgstRate + item.igstRate + item.cessRate + item.cessNonadvol;

                                replacetext = $"{taxSum} ";
                                htmldata = htmldata.Replace("{{taxSum}}", replacetext);

                                replacetext = $"{item.taxableAmount} ";
                                htmldata = htmldata.Replace("{{taxableAmount}}", replacetext);
                            }

                            // =>Amount Details

                            replacetext = $" : {ewbres_final.cgstValue} ";
                            htmldata = htmldata.Replace("{{cgstValue}}", replacetext);

                            replacetext = $" : {ewbres_final.sgstValue} ";
                            htmldata = htmldata.Replace("{{sgstValue}}", replacetext);

                            replacetext = $" : {ewbres_final.igstValue} ";
                            htmldata = htmldata.Replace("{{igstValue}}", replacetext);

                            replacetext = $" : {ewbres_final.cessValue} ";
                            htmldata = htmldata.Replace("{{cessValue}}", replacetext);

                            replacetext = $" : {ewbres_final.totInvValue} ";
                            htmldata = htmldata.Replace("{{totInvValue}}", replacetext);

                            replacetext = $" : {ewbres_final.cessNonAdvolValue} ";
                            htmldata = htmldata.Replace("{{cessNonAdvolValue}}", replacetext);

                            replacetext = $" : {ewbres_final.totalValue} ";
                            htmldata = htmldata.Replace("{{totalValue}}", replacetext);

                            replacetext = $" : {ewbres_final.otherValue} ";
                            htmldata = htmldata.Replace("{{otherValue}}", replacetext);

                            //TO =>Transporter Details

                            replacetext = $"{ewbres_final.transporterId} ";
                            htmldata = htmldata.Replace("{{transporterId}}", replacetext);

                            replacetext = $"{ewbres_final.transporterName} ";
                            htmldata = htmldata.Replace("{{transporterName}}", replacetext);

                            // =>Vehicle Details

                            foreach (var veh in ewbres_final.VehiclListDetails)
                            {
                                replacetext = $" {veh.vehicleNo} ";
                                htmldata = htmldata.Replace("{{vehicleNo}}", replacetext);

                                replacetext = $"{veh.enteredDate} ";
                                htmldata = htmldata.Replace("{{enteredDate}}", replacetext);

                                replacetext = $"{veh.transDocNo} ";
                                htmldata = htmldata.Replace("{{transDocNo}}", replacetext);

                                replacetext = $"{veh.transDocDate} ";
                                htmldata = htmldata.Replace("{{transDocDate}}", replacetext);

                                replacetext = $"{veh.fromPlace} ";
                                htmldata = htmldata.Replace("{{fromPlace}}", replacetext);

                                replacetext = $" {veh.transMode} ";
                                htmldata = htmldata.Replace("{{transMode}}", replacetext);

                                replacetext = $"{veh.fromState} ";
                                htmldata = htmldata.Replace("{{fromState}}", replacetext);

                                replacetext = $"{veh.userGSTINTransin} ";
                                htmldata = htmldata.Replace("{{userGSTINTransin}}", replacetext);

                                replacetext = $" {veh.tripshtNo} ";
                                htmldata = htmldata.Replace("{{tripshtNo}}", replacetext);
                            }
                        }
                    }
                    string OutPutFile = null;
                    string fullPhysicalPath = "";
                    if (!string.IsNullOrEmpty(htmldata))
                    {
                        string fileUrl = "";

                        string thFileName = "";
                        string TartgetfolderPath = "";

                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ExcelGeneratePath\EwayBillInvoice");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);

                        thFileName = "Ewaybill" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
                        fileUrl = "/PDFForeCast" + "/" + thFileName;
                        fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                        OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/EwayBillInvoice"), thFileName);

                        byte[] pdf = null;

                        pdf = Pdf
                              .From(htmldata)
                              .OfSize(OpenHtmlToPdf.PaperSize.A4)
                              .WithTitle("Invoice")
                              .WithoutOutline()
                              .WithMargins(PaperMargins.All(0.0.Millimeters()))
                              .Portrait()
                              .Comressed()
                              .Content();
                        FileStream file = File.Create(OutPutFile);
                        file.Write(pdf, 0, pdf.Length);
                        file.Close();
                    }
                    res.Message = "B2CEwayBill Pdf Generated";
                    res.status = true;
                    res.EwayBillPDF = "/ExcelGeneratePath/EwayBillInvoice/" + ewbres_final.ewbNo + ".pdf";
                }
                else if (Convert.ToInt32(newdata.status) == 0)
                {
                    var ewbres_error = JsonConvert.DeserializeObject<Item>(result);
                    byte[] reqDatabytes = Convert.FromBase64String(ewbres_error.error);
                    string requestData1 = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                    res.EwayBillPDF = "";
                    res.Message = "EwaybillPdf Not Generated";
                    res.status = false;
                    return res;
                }
            }
            else
            {
                res.Message = "EwayBill Service Is Not Enabled.";
                return res;
            }
            return res;
        }
        #endregion
        #region B2C Ewaybill Genarate
        public async Task<EwaybillBackendResponceDc> CancelEwayBillB2BandB2C(CancelRequestParam CancelRequestParam, AuthContext context, int userid)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            try
            {
                var CompanyDetails = GetcompanyDetails(context);
                if (CompanyDetails != null)
                {
                    var warehouseId = CancelRequestParam.APITypes == 1 ? await context.OrderDispatchedMasters.Where(x => x.OrderId == CancelRequestParam.orderid).Select(x => x.WarehouseId).FirstOrDefaultAsync() : await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == CancelRequestParam.orderid).Select(x => x.RequestToWarehouseId.Value).FirstOrDefaultAsync();

                    var warehouse = this.GetWarehouseDetails((int)warehouseId, context);
                    if (warehouse != null)
                    {
                        if (CancelRequestParam.Customertype == 1)//B2CCustomer != null && B2CCustomer.Any())
                        {
                            var configres = this.GetEwayBillConfiguration(warehouse.GSTin, context);
                            if (configres != null)
                            {
                                string EWB_New_Generate_url = configres.URL + "/v1.03/ewayapi";
                                var AuthTokenRequests = context.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                string EWB_Response_authtoken = string.Empty;
                                string EWB_Response_sek = string.Empty;
                                string generateSecureKey = string.Empty;
                                if (AuthTokenRequests != null)
                                {

                                    var date = AuthTokenRequests.CreatedDate;

                                    this.TokenGenerateTime(Convert.ToDateTime(date), out bool IsAuthGenrate);

                                    if (!IsAuthGenrate)
                                    {
                                        EWB_Response_authtoken = (string)AuthTokenRequests.AuthToken;
                                        EWB_Response_sek = (string)AuthTokenRequests.sek;
                                        generateSecureKey = (string)AuthTokenRequests.generateSecureKey;
                                    }
                                    else
                                    {
                                        AuthKeyReturn authKeyReturn = this.GenerateAuth(userid, configres);
                                        EWB_Response_sek = authKeyReturn.EWB_Response_sek;
                                        EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                                    }
                                }


                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                       | SecurityProtocolType.Tls11
                                       | SecurityProtocolType.Tls12
                                       | SecurityProtocolType.Ssl3;

                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Generate_url);
                                request.Method = "POST";
                                request.Timeout = 30000;
                                request.KeepAlive = true;
                                /*Optional*/
                                //request.KeepAlive = false;
                                request.AllowAutoRedirect = false;
                                request.Accept = "*/*";
                                request.ContentType = "application/json";
                                request.Headers.Add("client-id", configres.clientID);
                                request.Headers.Add("client-secret", configres.ClintSecretID);
                                request.Headers.Add("gstin", configres.GSTin); //configres.Select(x => x.GSTin).FirstOrDefault());
                                request.Headers.Add("authtoken", AuthTokenRequests.AuthToken);
                                EwayBillApiRequest ewbReq = new EwayBillApiRequest();
                                ewbReq.action = "CANEWB";
                                JavaScriptSerializer serial1 = new JavaScriptSerializer();
                                //string jsonData = System.Text.Json.JsonSerializer.Serialize(CancelRequestParam);
                                string jsonData = "{\"ewbNo\":" + CancelRequestParam.ewbNo + ",\"cancelRsnCode\":" + CancelRequestParam.cancelRsnCode + ",\"cancelRmrk\":\"" + CancelRequestParam.cancelRmrk + "\"}";

                                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                                {
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Request",
                                    Remarks = "B2CCancelEwayBillApi",
                                    Orderid = CancelRequestParam.orderid,
                                    ErrorDescription = jsonData,
                                };
                                context.OrderEwayBillsGenErrors.Add(ctrp);

                                //OrderEwayBillsGenError clearTaxReqResp = new OrderEwayBillsGenError()
                                //{
                                //    CreatedDate = DateTime.Now,
                                //    IsActive = true,
                                //    Type = "Response",
                                //    Remarks = "B2CCancelEwayBillApi",
                                //    Orderid = CancelRequestParam.orderid,
                                //    //error = JsonConvert.SerializeObject(responseIRNDC),
                                //};
                                string straesKeySaveing = (string)AuthTokenRequests.generateSecureKey;
                                string EWB_Response_sekNew = (string)AuthTokenRequests.sek;
                                byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                                string EncryptedSekNew = EWB_Response_sekNew;
                                string DecryptedSekNew = this.DecryptBySymmerticKey(EncryptedSekNew, _aeskeySaveing_Values);
                                ewbReq.data = this.EncryptBySymmetricKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData)), DecryptedSekNew);
                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    string json = serial1.Serialize(ewbReq);
                                    streamWriter.Write(json);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }
                                WebResponse response = request.GetResponse();
                                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                var ewbres_data1 = JsonConvert.DeserializeObject<EwayBillApiResponse>(result);
                                var ewbres_error = JsonConvert.DeserializeObject<Item>(result);
                                if (Convert.ToInt32(ewbres_data1.status) == 0)
                                {
                                    byte[] reqDatabytes = Convert.FromBase64String(ewbres_error.error);
                                    string requestData1 = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    var errorList = JsonConvert.DeserializeObject<errorCodeB2C>(requestData1);
                                    string ST = errorList.errorCodes.ToString();
                                    string[] authorsList = ST.Split(',');
                                    List<string> vs = authorsList.ToList();
                                    var Eids = vs.Where(x => !string.IsNullOrEmpty(x)).Select(Int32.Parse).ToList();
                                    var errorMessageList = context.EwayBillErrorLists.Where(x => Eids.Contains(x.Eid)).Select(x => x.Description).ToList();
                                    var error = string.Join(",", errorMessageList);
                                    if (!string.IsNullOrEmpty(requestData1))
                                    {
                                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2CCancelEwayBillApi" && x.Orderid == CancelRequestParam.orderid && x.IsActive == true && x.IsDeleted == false);
                                        if (orderEwayBillsGenErrors == null)
                                        {
                                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                                            ewayerror.ErrorCode = 0;
                                            ewayerror.Orderid = CancelRequestParam.orderid;
                                            ewayerror.IsActive = true;
                                            ewayerror.IsDeleted = false;
                                            ewayerror.Remarks = "B2CCancelEwayBillApi";
                                            ewayerror.ModifiedBy = null;
                                            ewayerror.ModifiedDate = null;
                                            ewayerror.Type = "Response";
                                            ewayerror.ErrorDescription = error; //insert json
                                            ewayerror.CreatedBy = userid;
                                            ewayerror.CreatedDate = DateTime.Now;
                                            context.OrderEwayBillsGenErrors.Add(ewayerror);
                                        }
                                        else
                                        {
                                            orderEwayBillsGenErrors.ErrorDescription = error;
                                            orderEwayBillsGenErrors.ModifiedBy = userid;
                                            orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                                            context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    res.Message = "Ewaybill  Not Cancelled.....try again with proper parameter";
                                    res.status = false;
                                    return res;
                                }
                                else if (Convert.ToInt32(ewbres_data1.status) == 1)
                                {
                                    string data = this.DecryptBySymmerticKey(ewbres_data1.data, Convert.FromBase64String(DecryptedSekNew));
                                    byte[] reqDatabytes = Convert.FromBase64String(data);
                                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    CanceledResponse ewbres_final = JsonConvert.DeserializeObject<CanceledResponse>(requestData);
                                    //clearTaxReqResp.ErrorDescription = JsonConvert.SerializeObject(ewbres_final);
                                    //context.OrderEwayBillsGenErrors.Add(clearTaxReqResp);
                                    //context.Commit();
                                    //res.Message = "EwayBill Cancelled Successfully";
                                    //res.status = true;
                                    //return res;

                                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                                    {
                                        ErrorCode = 0,
                                        Orderid = CancelRequestParam.orderid,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Remarks = "B2CCancelEwayBillApi",
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        ErrorDescription = requestData,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        Type = "Response"
                                    };
                                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == CancelRequestParam.orderid && x.IsActive == true).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                                    var orderDispatchedMaster = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == CancelRequestParam.orderid && !string.IsNullOrEmpty(x.EwayBillNumber)); //order id
                                    if (ewayupdate != null)
                                    {
                                        CanceledResponse RESS = JsonConvert.DeserializeObject<CanceledResponse>(requestData);
                                        //ewayupdate.EwayBillNo = Convert.ToString(RESS.ewayBillNo);
                                        ewayupdate.ModifiedDate = DateTime.Now;
                                        ewayupdate.ModifiedBy = userid;
                                        ewayupdate.IsCancelEwayBill = true;
                                        ewayupdate.EwayBillCancelDate = RESS.cancelDate;
                                        context.Entry(ewayupdate).State = EntityState.Modified;
                                    }
                                    if (orderDispatchedMaster != null)
                                    {
                                        orderDispatchedMaster.EwayBillNumber = null;
                                        context.Entry(orderDispatchedMaster).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                    res.Message = "Cancelled Successfully";
                                    res.status = true;
                                    return res;
                                }
                            }
                        }
                        else if (CancelRequestParam.Customertype == 0)//B2B
                        {
                            res = await this.CancelEwayBillbyIRN(CancelRequestParam, context, userid, warehouse, CompanyDetails);
                        }
                    }
                }
                else
                {
                    res.Message = "Eway Bill Service Not Started!!";
                    res.status = false;
                    return res;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return res;
        }
        //B2C
        public async Task<EwaybillBackendResponceDc> UpdateVehiclePartB(UpdatePartBRequest updatePartBParam, AuthContext context, int userid)
        {

            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            try
            {
                var CompanyDetails = GetcompanyDetails(context);
                if (CompanyDetails != null)
                {
                    var warehouseId = updatePartBParam.APITypes == 1 ? await context.OrderDispatchedMasters.Where(x => x.OrderId == updatePartBParam.orderid).Select(x => x.WarehouseId).FirstOrDefaultAsync() : await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == updatePartBParam.orderid).Select(x => x.RequestToWarehouseId.Value).FirstOrDefaultAsync();

                    var warehouse = this.GetWarehouseDetails((int)warehouseId, context);
                    if (warehouse != null)
                    {
                        if (updatePartBParam.Customertype == 1)//B2CCustomer != null && B2CCustomer.Any())
                        {
                            var configres = GetEwayBillConfiguration(warehouse.GSTin, context);
                            if (configres != null)
                            {
                                string EWB_New_Generate_url = configres.URL + "/v1.03/ewayapi";
                                // AuthKeyReturn authKeyReturn = GenerateAuth( userid,  configres);
                                UpdatePartbB2C B2Crequest = new UpdatePartbB2C()
                                {
                                    ewbNo = updatePartBParam.EwbNumber,
                                    fromPlace = updatePartBParam.FromPlace,
                                    fromState = Convert.ToInt32(warehouse.GSTin.Substring(0, 2)),
                                    reasonCode = Convert.ToInt32(updatePartBParam.ReasonCode),
                                    reasonRem = updatePartBParam.ReasonRemark,
                                    transDocDate = updatePartBParam.TransDocDt,
                                    transDocNo = updatePartBParam.TransDocNo,
                                    transMode = 1,
                                    vehicleNo = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString(),
                                    vehicleType = "R"
                                };
                                var AuthTokenRequests = context.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                string EWB_Response_authtoken = string.Empty;
                                string EWB_Response_sek = string.Empty;
                                string generateSecureKey = string.Empty;
                                if (AuthTokenRequests != null)
                                {
                                    //DateTime oldDate = AuthTokenRequests.CreatedDate;
                                    //DateTime newDate = DateTime.Now;

                                    //// Difference in days, hours
                                    //TimeSpan duration = newDate - oldDate;
                                    //// Difference in days.
                                    //int differenceInDays = duration.Days;
                                    //int differenceInHours = duration.Hours;

                                    var date = AuthTokenRequests.CreatedDate;
                                    // TokenTimeEwaybillHelper tokenTimeEwaybillHelper = new TokenTimeEwaybillHelper();

                                    this.TokenGenerateTime(Convert.ToDateTime(date), out bool IsAuthGenrate);
                                    if (!IsAuthGenrate)
                                    {
                                        EWB_Response_authtoken = (string)AuthTokenRequests.AuthToken;
                                        EWB_Response_sek = (string)AuthTokenRequests.sek;
                                        generateSecureKey = (string)AuthTokenRequests.generateSecureKey;
                                    }
                                    else if (IsAuthGenrate)
                                    {
                                        AuthKeyReturn authKeyReturn = this.GenerateAuth(userid, configres);
                                        EWB_Response_sek = authKeyReturn.EWB_Response_sek;
                                        EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                                        generateSecureKey = authKeyReturn.generateSecureKey;
                                    }
                                }
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                       | SecurityProtocolType.Tls11
                                       | SecurityProtocolType.Tls12
                                       | SecurityProtocolType.Ssl3;
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Generate_url);
                                request.Method = "POST";
                                request.KeepAlive = true;
                                request.AllowAutoRedirect = false;
                                request.Accept = "*/*";
                                request.ContentType = "application/json";
                                request.Headers.Add("client-id", configres.clientID);
                                request.Headers.Add("client-secret", configres.ClintSecretID);
                                request.Headers.Add("gstin", configres.GSTin); //configres.Select(x => x.GSTin).FirstOrDefault());
                                request.Headers.Add("authtoken", EWB_Response_authtoken);
                                EwayBillApiRequest ewbReq = new EwayBillApiRequest();
                                ewbReq.action = "VEHEWB";
                                JavaScriptSerializer serial1 = new JavaScriptSerializer();
                                string jsonData = System.Text.Json.JsonSerializer.Serialize(B2Crequest);
                                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                                {
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Request",
                                    Remarks = "B2CUpdateVehicleApi",
                                    Orderid = updatePartBParam.orderid,
                                    ErrorDescription = jsonData,
                                };
                                context.OrderEwayBillsGenErrors.Add(ctrp);

                                //OrderEwayBillsGenError clearTaxReqResp = new OrderEwayBillsGenError()
                                //{
                                //    CreatedDate = DateTime.Now,
                                //    IsActive = true,
                                //    Remarks = "B2CUpdateVehicleApi",
                                //    Type = "Response",
                                //    Orderid = updatePartBParam.orderid,
                                //    //error = JsonConvert.SerializeObject(responseIRNDC),
                                //};
                                string straesKeySaveing = (string)AuthTokenRequests.generateSecureKey;
                                string EWB_Response_sekNew = (string)AuthTokenRequests.sek;
                                byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                                string EncryptedSekNew = EWB_Response_sekNew;
                                string DecryptedSekNew = this.DecryptBySymmerticKey(EncryptedSekNew, _aeskeySaveing_Values);

                                ewbReq.data = this.EncryptBySymmetricKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData)), DecryptedSekNew);

                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    string json = serial1.Serialize(ewbReq);
                                    streamWriter.Write(json);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }
                                WebResponse response = request.GetResponse();
                                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                var ewbres_data1 = JsonConvert.DeserializeObject<EwayBillApiResponse>(result);
                                var ewbres_error = JsonConvert.DeserializeObject<Item>(result);
                                if (Convert.ToInt32(ewbres_data1.status) == 0)
                                {
                                    byte[] reqDatabytes = Convert.FromBase64String(ewbres_error.error);
                                    string requestData1 = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    var errorList = JsonConvert.DeserializeObject<errorCodeB2C>(requestData1);
                                    string ST = errorList.errorCodes.ToString();
                                    string[] authorsList = ST.Split(',');
                                    List<string> vs = authorsList.ToList();
                                    var Eids = vs.Where(x => !string.IsNullOrEmpty(x)).Select(Int32.Parse).ToList();
                                    var errorMessageList = context.EwayBillErrorLists.Where(x => Eids.Contains(x.Eid)).Select(x => x.Description).ToList();
                                    var error = string.Join(",", errorMessageList);
                                    if (!string.IsNullOrEmpty(requestData1))
                                    {
                                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2CUpdateVehicleApi" && x.Orderid == updatePartBParam.orderid && x.IsActive == true && x.IsDeleted == false);
                                        if (orderEwayBillsGenErrors == null)
                                        {
                                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                                            ewayerror.ErrorCode = 0;
                                            ewayerror.Orderid = updatePartBParam.orderid;
                                            ewayerror.IsActive = true;
                                            ewayerror.IsDeleted = false;
                                            ewayerror.Remarks = "B2CUpdateVehicleApi";
                                            ewayerror.ModifiedBy = null;
                                            ewayerror.Type = "Response";
                                            ewayerror.ModifiedDate = null;
                                            ewayerror.ErrorDescription = error;
                                            ewayerror.CreatedBy = userid;
                                            ewayerror.CreatedDate = DateTime.Now;
                                            context.OrderEwayBillsGenErrors.Add(ewayerror);
                                        }
                                        else
                                        {
                                            orderEwayBillsGenErrors.ErrorDescription = error;
                                            orderEwayBillsGenErrors.ModifiedBy = userid;
                                            orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                                            context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                                        }
                                        context.Commit();
                                    }

                                    res.Message = error;
                                    res.status = false;
                                    return res;
                                }
                                else if (Convert.ToInt32(ewbres_data1.status) == 1)
                                {
                                    string data = this.DecryptBySymmerticKey(ewbres_data1.data, Convert.FromBase64String(DecryptedSekNew));
                                    byte[] reqDatabytes = Convert.FromBase64String(data);
                                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    var ewbres_final = JsonConvert.DeserializeObject<vehicledata>(requestData);

                                    //clearTaxReqResp.ErrorDescription = JsonConvert.SerializeObject(ewbres_final);
                                    //context.OrderEwayBillsGenErrors.Add(clearTaxReqResp);
                                    //context.Commit();
                                    //res.Message = "Vehicle Updated Successfully";
                                    //res.status = true;
                                    //return res;
                                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                                    {
                                        ErrorCode = 0,
                                        Orderid = updatePartBParam.orderid,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Remarks = "B2CUpdateVehicleApi",
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        ErrorDescription = requestData,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        Type = "Response"
                                    };
                                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == updatePartBParam.orderid && x.IsActive == true).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                                    if (ewayupdate != null)
                                    {
                                        UpdatePartBResponse RESS = JsonConvert.DeserializeObject<UpdatePartBResponse>(requestData);
                                        //ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                                        ewayupdate.ModifiedBy = userid;
                                        ewayupdate.ModifiedDate = DateTime.Now;
                                        ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat); //Convert.ToDateTime(RESS.ValidUpto);
                                        ewayupdate.VehicleNumber = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                                        //ewayupdate.IsCancelEwayBill = true;
                                        //ewayupdate.EwayBillCancelDate = RESS.cancelDate;
                                        context.Entry(ewayupdate).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                    res.Message = "Vehicle Updated Successfully";
                                    res.status = true;
                                    return res;
                                }
                            }
                            //return res;
                        }    //B2C
                        else if (updatePartBParam.Customertype == 0)//B2B
                        {
                            string newReasonCode = "";
                            if (updatePartBParam.ReasonCode == "1") { newReasonCode = "BREAKDOWN"; }
                            else if (updatePartBParam.ReasonCode == "2") { newReasonCode = "TRANSSHIPMENT"; }
                            else if (updatePartBParam.ReasonCode == "3") { newReasonCode = "OTHERS"; }
                            else if (updatePartBParam.ReasonCode == "4") { newReasonCode = "FIRST_TIME"; }

                            UpdatePartBRequest updatePartB2B = new UpdatePartBRequest();
                            updatePartB2B.orderid = updatePartBParam.orderid;
                            updatePartB2B.EwbNumber = updatePartBParam.EwbNumber;
                            updatePartB2B.FromPlace = updatePartBParam.FromPlace;
                            updatePartB2B.FromState = Convert.ToInt32(warehouse.GSTin.Substring(0, 2));//updatePartBParam.FromState;
                            updatePartB2B.ReasonCode = newReasonCode;
                            updatePartB2B.ReasonRemark = "partb";
                            updatePartB2B.TransDocNo = updatePartBParam.TransDocNo;
                            updatePartB2B.TransDocDt = updatePartBParam.TransDocDt;
                            updatePartB2B.TransMode = "ROAD";
                            //updatePartB2B.TransporterGstin = updatePartBParam.TransporterGstin;
                            updatePartB2B.DocumentNumber = "";
                            updatePartB2B.DocumentDate = "";
                            updatePartB2B.VehicleType = "REGULAR";
                            updatePartB2B.VehNo = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                            // insert dc
                            res = await UpdateVehiclePartBIRN(updatePartB2B, context, userid, warehouse, CompanyDetails);
                        }
                    }
                }
                else
                {
                    res.Message = "Eway Bill Service Not Started!!";
                    res.status = false;
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }


        public EwaybillBackendResponceDc UpdateVehiclePartA(UpdatePartA updatePartA, AuthContext context, int userid)
        {

            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            try
            {
                var CompanyDetails = GetcompanyDetails(context);
                if (CompanyDetails != null)
                {
                    var warehouseId = updatePartA.APITypes == 1 ? context.OrderDispatchedMasters.Where(x => x.OrderId == updatePartA.Orderid).Select(x => x.WarehouseId).FirstOrDefault() : context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == updatePartA.Orderid).Select(x => x.RequestToWarehouseId.Value).FirstOrDefault();

                    var warehouse = this.GetWarehouseDetails((int)warehouseId, context);
                    if (warehouse != null)
                    {
                        // insert dc
                        res = UpdateTransporterPartA(updatePartA, context, userid, warehouse, CompanyDetails);
                    }
                }
                else
                {
                    res.Message = "Eway Bill Service Not Started!!";
                    res.status = false;
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        public async Task<EwaybillBackendResponceDc> ExtendValidity(ExtendRequestParamB2C extendparam, AuthContext context, int userid)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            try
            {
                var CompanyDetails = GetcompanyDetails(context);
                if (CompanyDetails != null)
                {
                    var warehouseId = extendparam.APITypes == 1 ? await context.OrderDispatchedMasters.Where(x => x.OrderId == extendparam.orderid).Select(x => x.WarehouseId).FirstOrDefaultAsync() : await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == extendparam.orderid).Select(x => x.WarehouseId.Value).FirstOrDefaultAsync();

                    //int pin1 = 0;
                    //int pin2 = 0;
                    //if (extendparam.APITypes == 2)
                    //{
                    //    int wareid = await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == extendparam.orderid).Select(x => x.RequestToWarehouseId.Value).FirstOrDefaultAsync();
                    //    pin1 = await context.Warehouses.Where(x => x.WarehouseId == warehouseId).Select(x => x.PinCode).FirstOrDefaultAsync();
                    //    pin2 = await context.Warehouses.Where(x => x.WarehouseId == wareid).Select(x => x.PinCode).FirstOrDefaultAsync();
                    //}
                    var warehouse = this.GetWarehouseDetails((int)warehouseId, context);
                    if (warehouse != null)
                    {
                        if (extendparam.Customertype == 1)//B2CCustomer != null && B2CCustomer.Any())
                        {
                            var configres = GetEwayBillConfiguration(warehouse.GSTin, context);
                            if (configres != null)
                            {
                                string EWB_New_Generate_url = configres.URL + "/v1.03/ewayapi";
                                var AuthTokenRequests = context.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                string EWB_Response_authtoken = string.Empty;
                                string EWB_Response_sek = string.Empty;
                                string generateSecureKey = string.Empty;
                                if (AuthTokenRequests != null)
                                {
                                    var date = AuthTokenRequests.CreatedDate;
                                    //TokenTimeEwaybillHelper tokenTimeEwaybillHelper = new TokenTimeEwaybillHelper();
                                    this.TokenGenerateTime(Convert.ToDateTime(date), out bool IsAuthGenrate);
                                    if (!IsAuthGenrate)
                                    {
                                        EWB_Response_authtoken = (string)AuthTokenRequests.AuthToken;
                                        EWB_Response_sek = (string)AuthTokenRequests.sek;
                                        generateSecureKey = (string)AuthTokenRequests.generateSecureKey;
                                    }
                                    else
                                    {

                                        AuthKeyReturn authKeyReturn = this.GenerateAuth(userid, configres);
                                        EWB_Response_sek = authKeyReturn.EWB_Response_sek;
                                        EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                                        generateSecureKey = authKeyReturn.generateSecureKey;
                                    }
                                }
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                       | SecurityProtocolType.Tls11
                                       | SecurityProtocolType.Tls12
                                       | SecurityProtocolType.Ssl3;
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Generate_url);
                                request.Method = "POST";
                                /*Optional*/
                                request.KeepAlive = true;
                                request.AllowAutoRedirect = false;
                                request.Accept = "*/*";
                                request.ContentType = "application/json";
                                request.Headers.Add("client-id", configres.clientID);
                                request.Headers.Add("client-secret", configres.ClintSecretID);
                                request.Headers.Add("gstin", configres.GSTin); //configres.Select(x => x.GSTin).FirstOrDefault());
                                request.Headers.Add("authtoken", AuthTokenRequests.AuthToken);
                                EwayBillApiRequest ewbReq = new EwayBillApiRequest();
                                ewbReq.action = "EXTENDVALIDITY";
                                JavaScriptSerializer serial1 = new JavaScriptSerializer();
                                string jsonData = System.Text.Json.JsonSerializer.Serialize(extendparam);
                                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                                {
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Request",
                                    Remarks = "B2CExtendValidityApi",
                                    Orderid = extendparam.orderid,
                                    ErrorDescription = jsonData,
                                };
                                context.OrderEwayBillsGenErrors.Add(ctrp);

                                //OrderEwayBillsGenError clearTaxReqResp = new OrderEwayBillsGenError()
                                //{
                                //    CreatedDate = DateTime.Now,
                                //    IsActive = true,
                                //    Remarks = "B2CExtendValidityApi",
                                //    Type = "Response",
                                //    Orderid = extendparam.orderid,
                                //    //error = JsonConvert.SerializeObject(responseIRNDC),
                                //};
                                string straesKeySaveing = (string)AuthTokenRequests.generateSecureKey;
                                string EWB_Response_sekNew = (string)AuthTokenRequests.sek;
                                byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                                string EncryptedSekNew = EWB_Response_sekNew;

                                string DecryptedSekNew = this.DecryptBySymmerticKey(EncryptedSekNew, _aeskeySaveing_Values);

                                ewbReq.data = this.EncryptBySymmetricKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData)), DecryptedSekNew);
                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    string json = serial1.Serialize(ewbReq);
                                    streamWriter.Write(json);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }
                                WebResponse response = request.GetResponse();
                                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                var ewbres_data1 = JsonConvert.DeserializeObject<EwayBillApiResponse>(result);
                                var ewbres_error = JsonConvert.DeserializeObject<Item>(result);
                                if (Convert.ToInt32(ewbres_data1.status) == 0)
                                {
                                    byte[] reqDatabytes = Convert.FromBase64String(ewbres_error.error);
                                    string requestData1 = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    var errorList = JsonConvert.DeserializeObject<errorCodeB2C>(requestData1);
                                    string ST = errorList.errorCodes.ToString();
                                    string[] authorsList = ST.Split(',');
                                    List<string> vs = authorsList.ToList();
                                    var Eids = vs.Where(x => !string.IsNullOrEmpty(x)).Select(Int32.Parse).ToList();
                                    var errorMessageList = context.EwayBillErrorLists.Where(x => Eids.Contains(x.Eid)).Select(x => x.Description).ToList();
                                    var error = string.Join(",", errorMessageList);
                                    if (!string.IsNullOrEmpty(requestData1))
                                    {
                                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "B2CExtendValidityApi" && x.Orderid == extendparam.orderid && x.IsActive == true && x.IsDeleted == false);
                                        if (orderEwayBillsGenErrors == null)
                                        {
                                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                                            ewayerror.ErrorCode = 0;
                                            ewayerror.Orderid = extendparam.orderid;
                                            ewayerror.IsActive = true;
                                            ewayerror.IsDeleted = false;
                                            ewayerror.Remarks = "B2CExtendApi";
                                            ewayerror.ModifiedBy = null;
                                            ewayerror.Type = "Response";
                                            ewayerror.ModifiedDate = null;
                                            ewayerror.ErrorDescription = error;//requestData1;
                                            ewayerror.CreatedBy = userid;
                                            ewayerror.CreatedDate = DateTime.Now;
                                            context.OrderEwayBillsGenErrors.Add(ewayerror);
                                        }
                                        else
                                        {
                                            orderEwayBillsGenErrors.ErrorDescription = error;//requestData1;
                                            orderEwayBillsGenErrors.ModifiedBy = userid;
                                            orderEwayBillsGenErrors.ModifiedDate = DateTime.Now;
                                            context.Entry(orderEwayBillsGenErrors).State = EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    res.Message = "Ewaybill Not Extended.....try again";
                                    res.status = false;
                                    return res;
                                }
                                else if (Convert.ToInt32(ewbres_data1.status) == 1)
                                {
                                    string data = this.DecryptBySymmerticKey(ewbres_data1.data, Convert.FromBase64String(DecryptedSekNew));
                                    byte[] reqDatabytes = Convert.FromBase64String(data);
                                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                                    var ewbres_final = JsonConvert.DeserializeObject<ExtendEwayBillresponse>(requestData);

                                    //var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == extendparam.orderid && x.EwayBillNo != null).FirstOrDefault(); //order id
                                    //if (ewayupdate != null)
                                    //{
                                    //    ewayupdate.EwayBillNo = ewbres_final.ewayBillNo;
                                    //    ewayupdate.CreateDate = Convert.ToDateTime(ewbres_final.updatedDate);
                                    //    ewayupdate.EwayBillValidTill = Convert.ToDateTime(ewbres_final.validUpto);
                                    //    ewayupdate.IsExtendEwayBill = true;
                                    //    ewayupdate.EwayBillExtendDate = DateTime.Now;
                                    //    context.Entry(ewayupdate).State = EntityState.Modified;
                                    //}
                                    //clearTaxReqResp.ErrorDescription = JsonConvert.SerializeObject(ewbres_final);
                                    //context.OrderEwayBillsGenErrors.Add(clearTaxReqResp);
                                    //context.Commit();
                                    //res.Message = " Ewaybill Extended Successfully";
                                    //res.status = true;
                                    //return res;
                                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                                    {
                                        ErrorCode = 0,
                                        Orderid = extendparam.orderid,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Remarks = "B2CExtendApi",
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        ErrorDescription = requestData,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        Type = "Response"
                                    };
                                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == extendparam.orderid && x.IsActive == true).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                                    if (ewayupdate != null)
                                    {
                                        ExtendResponseDc RESS = JsonConvert.DeserializeObject<ExtendResponseDc>(requestData);
                                        ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                                        ewayupdate.ModifiedDate = DateTime.Now;
                                        ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto);
                                        ewayupdate.IsExtendEwayBill = true;
                                        ewayupdate.EwayBillExtendDate = Convert.ToDateTime(RESS.UpdatedDate);
                                        ewayupdate.VehicleNumber = extendparam.vehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                                        context.Entry(ewayupdate).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                    res.Message = "Extended Successfully";
                                    res.status = true;
                                    return res;
                                }
                            }
                        }
                        else if (extendparam.Customertype == 0)//B2B
                        {
                            //double distance = 0;
                            //int distanceConvert = 0;
                            //if (extendparam.remainingDistance.ToString() == "0")
                            //{
                            //    try
                            //    {
                            //        distance = Convert.ToDouble(Common.Helpers.GeoHelper.GetDistanceInKM(pin1, pin2));
                            //        distanceConvert = Convert.ToInt32(Math.Round(distance, 0));
                            //    }
                            //    catch (Exception ex)
                            //    {

                            //        throw ex;
                            //    }
                            //}
                            ExtendRequestParam extendRequestParam = new ExtendRequestParam() // insert dc
                            {
                                ConsignmentStatus = "MOVEMENT",
                                DocumentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                DocumentNumber = extendparam.transDocNo,//doubt
                                DocumentType = "INV",
                                EwbNumber = extendparam.ewbNo,
                                FromPincode = warehouse.PinCode.ToString(),
                                FromPlace = warehouse.CityName,
                                FromState = warehouse.GSTin.Substring(0, 2),
                                ReasonCode = extendparam.extnRsnCode.ToString(),
                                ReasonRemark = extendparam.extnRemarks,
                                RemainingDistance = extendparam.remainingDistance.ToString(),
                                TransDocDt = extendparam.transDocDate,
                                TransDocNo = extendparam.transDocNo,
                                TransMode = "ROAD",
                                VehicleType = "REGULAR",
                                VehNo = extendparam.vehicleNo.Trim().Replace(" ", "").ToUpper().ToString(),
                            };
                            res = await ExtendEwayBillbyIRN(extendRequestParam, context, userid, warehouse, extendparam.orderid, CompanyDetails);
                        }
                    }
                }
                else
                {
                    res.Message = "Eway Bill Service Not Started!!";
                    res.status = false;
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        #endregion

        //public async Task<bool> GenrateEwayBillByIRN(string IrnNo)
        //{
        //    string BaseUrl = "https://api-sandbox.clear.in/einv/";
        //    string authtoken = "1.7007a747-e980-4fb7-94b5-ca0d4aed9e8f_110a705996a75dadd0756517d2bf44b0ba3384e9243430b52ba88f57ffc783c0";
        //    ewaybillirnParam ewayBillPostDc = await EwayBillPostDc();
        //    ewayBillPostDc.Irn = IrnNo;
        //    var newJson = JsonConvert.SerializeObject(ewayBillPostDc);
        //    using (var httpClient = new HttpClient())
        //    {
        //        using (var request = new HttpRequestMessage(new HttpMethod("POST"), BaseUrl + "v2/eInvoice/ewaybill"))
        //        {
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //            //request.Headers.TryAddWithoutValidation("Accept", "*/*");
        //            //request.Headers.TryAddWithoutValidation("noencryption", "1");
        //            request.Headers.TryAddWithoutValidation("X-Cleartax-Auth-Token", authtoken);
        //            request.Headers.TryAddWithoutValidation("gstin", "09AAFCD5862R006");
        //            request.Content = new StringContent(newJson);
        //            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        //            try
        //            {
        //                var response = await httpClient.SendAsync(request);
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex.Message;
        //            }

        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                string jsonString = (await response.Content.ReadAsStringAsync());
        //                var ExtrResult = JsonConvert.DeserializeObject<ewaybillirnParam>(jsonString);

        //            }
        //            else
        //            {

        //            }
        //        }
        //    }
        //    return true;
        //}

        //UpdatePartBRequest

        public CancelParam CancelEwayBillPostDc(CancelRequestParam cancelewaybillirn1)
        {
            CancelParam cancelewaybillirn = new CancelParam()
            {
                cancelRmrk = cancelewaybillirn1.cancelRmrk,
                cancelRsnCode = cancelewaybillirn1.cancelRmrk,
                ewbNo = cancelewaybillirn1.ewbNo,
            };
            return cancelewaybillirn;
        }
        public ClearTaxIntegration InsertEwayBillRespone(int OrderId, AuthContext authContext)
        {
            #region ClearTaxIntegrations
            var data = authContext.ClearTaxIntegrations.Where(x => x.OrderId == OrderId && x.APIType == "GenerateEWB").FirstOrDefault();
            if (data == null)
            {
                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                clearTaxIntegration.OrderId = OrderId;
                clearTaxIntegration.IsActive = true;
                clearTaxIntegration.CreateDate = DateTime.Now;
                clearTaxIntegration.IsProcessed = false;
                clearTaxIntegration.APIType = "GenerateEWB";
                clearTaxIntegration.APITypes = 1;//GenerateEWB
                authContext.ClearTaxIntegrations.Add(clearTaxIntegration);
            }
            #endregion
            return data;
        }
        public async Task<List<EwayBillOrderList>> GetEwayBillOrderList(long tripPlannerConfirmedMasterId, AuthContext authContext, int orderid, long VehicleId)
        {
            var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            var Orderid = new SqlParameter("@orderid", orderid);
            var vehicleId = new SqlParameter("@VehicleId", VehicleId);
            var orderList = await authContext.Database.SqlQuery<EwayBillOrderList>("Operation.TripPlanner_GetEWayBillOrder @TripPlannerConfirmedMasterId,@orderid,@VehicleId", tripPlannerConfirmedMasterid, Orderid, vehicleId).ToListAsync();
            return orderList;
        }
        public static byte[] generateSecureKey()
        {
            Aes KEYGEN = Aes.Create();
            byte[] secretKey = KEYGEN.Key;
            return secretKey;
        }
        public EwayBillConfigDC GetEwayBillConfiguration(string taskk, AuthContext context)
        {
            try
            {
                var param = new SqlParameter("@Taskk", taskk);
                //var result = new List<EwayBillConfigDC>();
                EwayBillConfigDC result = context.Database.SqlQuery<EwayBillConfigDC>("exec EwayBillConfig @Taskk", param).FirstOrDefault();
                return result;

            }
            catch (Exception ec)
            {
                throw ec;
            }
        }
        public AuthTokenGenerationDc B2CEwayBillGenerate(int userid, AuthContext db, string gstin)
        {
            AuthTokenGenerationDc authTokenGenerationDc = new AuthTokenGenerationDc();
            bool status = false;
            byte[] _aeskey = generateSecureKey();
            var configres = GetEwayBillConfiguration(gstin, db);
            if (configres != null)
            {
                authTokenGenerationDc.configres = configres;
                var AuthTokenRequests = db.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (AuthTokenRequests != null)
                {
                    var date = AuthTokenRequests.CreatedDate;
                    TokenTimeEwaybillHelper tokenTimeEwaybillHelper = new TokenTimeEwaybillHelper();
                    this.TokenGenerateTime(Convert.ToDateTime(date), out bool IsAuthGenrate);

                    if (!IsAuthGenrate)
                    {
                        authTokenGenerationDc.EWB_Response_authtoken = (string)AuthTokenRequests.AuthToken;
                        string EWB_Response_sek = (string)AuthTokenRequests.sek;

                        string straesKeySaveing = (string)AuthTokenRequests.generateSecureKey;
                        byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                        string EncryptedSek = EWB_Response_sek;
                        authTokenGenerationDc.DecryptedSek = DecryptBySymmerticKey(EncryptedSek, _aeskeySaveing_Values);
                        //status = GeneratedEwayBill(configres.URL, EWB_Response_authtoken, DecryptedSek, mainparam, configres, userid, db);

                    }
                    else if (IsAuthGenrate)
                    {
                        //Generate Auto/Sec

                        AuthKeyReturn authKeyReturn = GenerateAuth(userid, configres);
                        if (authKeyReturn.Isgenereted == true)
                        {
                            //var straesKey = Convert.ToBase64String(_aeskey);
                            string straesKeySaveing = authKeyReturn.generateSecureKey;
                            byte[] _aeskeySaveing_Values = Convert.FromBase64String(straesKeySaveing);
                            string EncryptedSek = authKeyReturn.EWB_Response_sek;
                            string DecryptedSek = DecryptBySymmerticKey(EncryptedSek, _aeskeySaveing_Values);

                            authTokenGenerationDc.EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                            authTokenGenerationDc.DecryptedSek = DecryptedSek;


                            // status = GeneratedEwayBill(configres.URL, authKeyReturn.EWB_Response_authtoken, DecryptedSek, straesKeySaveing, mainparam, configres, userid, db);
                        }
                        else
                        {
                            status = false;
                        }
                    }
                }
                else
                {
                    AuthKeyReturn authKeyReturn = GenerateAuth(userid, configres);

                    if (authKeyReturn.Isgenereted == true)
                    {
                        var straesKey = Convert.ToBase64String(_aeskey);
                        string EncryptedSek = authKeyReturn.EWB_Response_sek;
                        string DecryptedSek = DecryptBySymmerticKey(EncryptedSek, _aeskey);

                        authTokenGenerationDc.EWB_Response_authtoken = authKeyReturn.EWB_Response_authtoken;
                        authTokenGenerationDc.DecryptedSek = DecryptedSek;
                        //status = GeneratedEwayBill(configres.URL, authKeyReturn.EWB_Response_authtoken, DecryptedSek, straesKey, mainparam, configres, userid, db);
                    }
                    else
                    {
                        status = false;
                    }
                }
            }
            return authTokenGenerationDc;
        }
        #region Generate Eway Bill No. Parent ==>> Child
        public B2CResponseDc GeneratedEwayBill(string EWB_authtoken, string sek, Ewaygenerateparam param, EwayBillConfigDC configres, int userid, List<GetCustomerGenerateEwayBillDC> Ewaydata, List<OrderDispatchedDetails> items, List<ItemMaster> itemMasterslist, List<ItemMultiMRP> mrpList, List<OrderEwayBillsGenError> orderEwayBillsGenErrors, List<EwayBillErrorList> EwayBillErrorLists, List<OrderEwayBill> orderEwayBills)
        {
            B2CResponseDc b2CResponseDc = new B2CResponseDc();
            try
            {
                string EWB_New_Generate_url = configres.URL + "/v1.03/ewayapi";
                //Step 1: Prepare the request object by setting the orderEwayBills, client-id, client secret, GSTIN, and Auth token received from the previous API.

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Generate_url);
                request.Method = "POST";
                request.KeepAlive = true;
                request.AllowAutoRedirect = false;
                request.Accept = "*/*";
                request.ContentType = "application/json";
                request.Headers.Add("client-id", configres.clientID);
                request.Headers.Add("client-secret", configres.ClintSecretID);
                request.Headers.Add("gstin", configres.GSTin);//configres.Select(x => x.GSTin).FirstOrDefault());
                request.Headers.Add("authtoken", EWB_authtoken);

                //Step 2: Prepare the JSON string with all parameters.Post the request and receive the response. Refer the annexure for the parameter details.

                EwayBillApiRequest ewbReq = new EwayBillApiRequest();
                ewbReq.action = "GENEWAYBILL";
                //string EWB_Gstin = "05AAAAH2226G1Z9";  //right data  ***** togst=23AAPPD5509E1Z1
                //string jsonDataee = "{\"supplyType\":\"O\",\"subSupplyType\":\"1\",\"DocType\":\"INV\",\"docNo\":\"UP202110\",\"docDate\":\"19/04/2023\",\"fromGstin\":\"" + EWB_Gstin + "\",\"actFromStateCode\":\"9\",\"actToStateCode\":\"9\",\"totInvValue\":\"63940\",\"fromTrdName\":\"ShopKirana E Trading Pvt. Ltd\",\"fromAddr1\":\"indore\",\"fromAddr2\":\"GROUND FLOOR OSBORNE ROAD\",\"fromPlace\":\"indore\",\"fromPincode\":226028,\"fromStateCode\":9,\"toGstin\":\"URP\",\"toTrdName\":\"hsn verification27\",\"toAddr1\":\"45B 46A\",\"toAddr2\":\"INDUSTRIAL AREA,SANWER ROAD\",\"toPlace\":\"INDORE\",\"toPincode\":226020,\"toStateCode\":9,\"transactionType\":1,\"otherValue\":-100,\"totalValue\":63857,\"cgstValue\":0,\"sgstValue\":0,\"igstValue\":0,\"cessValue\":0,\"transporterName\":\"Shyam\",\"transporterId\":\"23AAPPD5509E1Z1\",\"transDocNo\":\"12345\",\"transMode\":\"1\",\"transDocDate\":\"19/04/2023\",\"transDistance\":\"11\",\"vehicleType\":\"R\",\"vehicleNo\":\"MP09AA1234\",\"ItemList\":[{\"productName\":\"Hasty Tasty Indori Poha 50 MRP 500 Gm\",\"hsnCode\":19041090,\"quantity\":14,\"qtyUnit\":\"PCS\",\"cgstRate\":0,\"sgstRate\":0,\"igstRate\":0,\"cessRate\":0,\"cessAdvol\":0,\"taxableAmount\":63856.80}]}";
                //var paramx = new SqlParameter("@orderid", param.orderid);
                DataContracts.EwayBill.EwaybillByIRNDc.generateewaybilldataDC generateewaybilldataDC = new DataContracts.EwayBill.EwaybillByIRNDc.generateewaybilldataDC();
                List<DataContracts.EwayBill.EwaybillByIRNDc.EwayBillDataListnew> listitemdn = new List<DataContracts.EwayBill.EwaybillByIRNDc.EwayBillDataListnew>();
                //List<DataContracts.EwayBill.EwaybillByIRNDc.GetCustomerGenerateEwayBillDC> Ewaydata = context.Database.SqlQuery<DataContracts.EwayBill.EwaybillByIRNDc.GetCustomerGenerateEwayBillDC>("exec generateewaybilldata @orderid", paramx).ToList();
                double distance = 0;
                int distanceConvert = 0;
                if (param.distance == 0)
                {
                    try
                    {
                        distance = Convert.ToDouble(Common.Helpers.GeoHelper.GetDistanceInKM(Ewaydata.FirstOrDefault().fromPincode, Convert.ToInt32(param.topincode)));
                        distanceConvert = Convert.ToInt32(Math.Round(distance, 0));
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                generateewaybilldataDC.supplyType = "O";
                generateewaybilldataDC.subSupplyType = "1";
                generateewaybilldataDC.subSupplyDesc = "";
                generateewaybilldataDC.DocType = "INV";
                generateewaybilldataDC.docNo = Ewaydata.FirstOrDefault().docNo;
                generateewaybilldataDC.docDate = Ewaydata.FirstOrDefault().docDate;
                //warehouse Details Part
                generateewaybilldataDC.fromGstin = Ewaydata.FirstOrDefault().fromGstin;//warehouse                               
                generateewaybilldataDC.fromTrdName = Ewaydata.FirstOrDefault().fromTrdName;
                generateewaybilldataDC.fromAddr1 = Ewaydata.FirstOrDefault().fromAddr1;
                generateewaybilldataDC.fromAddr2 = "";//Ewaydata.FirstOrDefault().fromAddr1;
                generateewaybilldataDC.fromPlace = Ewaydata.FirstOrDefault().fromPlace;
                generateewaybilldataDC.fromPincode = Ewaydata.FirstOrDefault().fromPincode;
                generateewaybilldataDC.actFromStateCode = Convert.ToInt32(Ewaydata.FirstOrDefault().actFromStateCode);//w  
                generateewaybilldataDC.fromStateCode = Convert.ToInt32(Ewaydata.FirstOrDefault().fromStateCode);//W
                                                                                                                //Customer Details Part
                generateewaybilldataDC.toGstin = !string.IsNullOrEmpty(Ewaydata.FirstOrDefault().toGstin) ? Ewaydata.FirstOrDefault().toGstin : "URP";
                generateewaybilldataDC.toTrdName = Ewaydata.FirstOrDefault().toTrdName;
                generateewaybilldataDC.toAddr1 = Ewaydata.FirstOrDefault().toAddr1;
                generateewaybilldataDC.toAddr2 = "";//Ewaydata.FirstOrDefault().toAddr1;
                generateewaybilldataDC.toPlace = Ewaydata.FirstOrDefault().toPlace;
                generateewaybilldataDC.toPincode = param.topincode; //Ewaydata.FirstOrDfault().toPincode;
                generateewaybilldataDC.actToStateCode = Convert.ToInt32(Ewaydata.FirstOrDefault().actToStateCode);//c
                generateewaybilldataDC.toStateCode = Convert.ToInt32(Ewaydata.FirstOrDefault().toStateCode);//c
                                                                                                            //Order Details 
                generateewaybilldataDC.transactionType = 1;
                generateewaybilldataDC.totInvValue = Ewaydata.FirstOrDefault().totInvValue;
                generateewaybilldataDC.otherValue = Ewaydata.FirstOrDefault().OtherAmount;
                generateewaybilldataDC.totalValue = Ewaydata.FirstOrDefault().totalValue;
                generateewaybilldataDC.sgstValue = Ewaydata.FirstOrDefault().sgstValue;
                generateewaybilldataDC.cgstValue = Ewaydata.FirstOrDefault().cgstValue;
                generateewaybilldataDC.igstValue = Ewaydata.FirstOrDefault().igstValue;
                generateewaybilldataDC.cessValue = Ewaydata.FirstOrDefault().cessValue;
                generateewaybilldataDC.cessNonAdvolValue = Ewaydata.FirstOrDefault().cessNonAdvolValue;
                // Transport information
                generateewaybilldataDC.transporterId = "";
                generateewaybilldataDC.transporterName = param.transportername;
                generateewaybilldataDC.transDocNo = param.transportergst;
                generateewaybilldataDC.transMode = 1;
                generateewaybilldataDC.transDistance = distanceConvert > 0 && param.distance == 0 ? distanceConvert : param.distance;
                generateewaybilldataDC.transDocDate = DateTime.Now.ToString("dd/MM/yyyy");
                generateewaybilldataDC.vehicleNo = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString();
                generateewaybilldataDC.vehicleType = "R";
                // Order Item List
                bool Igst = false;

                string whStateCode = Ewaydata.FirstOrDefault().fromGstin.Substring(0, 2);
                string custStateCode = !string.IsNullOrEmpty(Ewaydata.FirstOrDefault().toGstin) ? Ewaydata.FirstOrDefault().toGstin.Substring(0, 2) : "";

                if (whStateCode != custStateCode && !string.IsNullOrEmpty(Ewaydata.FirstOrDefault().toGstin))
                {
                    Igst = true;
                }


                foreach (var item in items)
                {
                    var multiMrp = mrpList.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                    double igstRate = 0;
                    if (Igst)
                    {
                        igstRate = item.SGSTTaxPercentage + item.CGSTTaxPercentage;
                    }
                    DataContracts.EwayBill.EwaybillByIRNDc.EwayBillDataListnew listi = new DataContracts.EwayBill.EwaybillByIRNDc.EwayBillDataListnew()
                    {
                        productName = item.itemname,
                        productDesc = item.itemname,
                        hsnCode = itemMasterslist.FirstOrDefault(x => x.ItemId == item.ItemId)?.HSNCode,
                        quantity = item.qty,
                        qtyUnit = "PCS",
                        cgstRate = item.CGSTTaxPercentage,
                        sgstRate = item.CGSTTaxPercentage,
                        igstRate = igstRate,
                        cessRate = item.TotalCessPercentage,
                        cessAdvol = 0,
                        taxableAmount = item.AmtWithoutTaxDisc
                    };
                    listitemdn.Add(listi);
                }
                generateewaybilldataDC.ItemList = listitemdn;

                string jsonData = System.Text.Json.JsonSerializer.Serialize(generateewaybilldataDC);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2CGenerateEwayBillApi",
                    Orderid = param.orderid,
                    ErrorDescription = jsonData,
                };
                orderEwayBillsGenErrors.Add(ctrp);
                OrderEwayBillsGenError clearTaxReqResp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Response",
                    Remarks = "B2CGenerateEwayBillApi",
                    Orderid = param.orderid,
                    //error = JsonConvert.SerializeObject(responseIRNDC),
                };

                ewbReq.data = EncryptBySymmetricKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData)), sek);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(ewbReq);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response = request.GetResponse();
                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var ewbres_data1 = JsonConvert.DeserializeObject<EwayBillApiResponse>(result);
                var ewbres_error = JsonConvert.DeserializeObject<Item>(result);
                if (Convert.ToInt32(ewbres_data1.status) == 0)
                {
                    byte[] reqDatabytes = Convert.FromBase64String(ewbres_error.error);
                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                    var errorList = JsonConvert.DeserializeObject<errorCodeB2C>(requestData);
                    string ST = errorList.errorCodes.ToString();
                    string[] authorsList = ST.Split(',');
                    List<string> vs = authorsList.ToList();
                    var Eids = vs.Where(x => !string.IsNullOrEmpty(x)).Select(Int32.Parse).ToList();
                    var errorMessageList = EwayBillErrorLists.Where(x => Eids.Contains(x.Eid)).Select(x => x.Description).ToList();
                    if (Eids.Any(x => x == 238))//Invalid auth token
                    {
                        AuthKeyReturn authKeyReturn = GenerateAuth(userid, configres);
                    }
                    var error = string.Join(",", errorMessageList);
                    if (!string.IsNullOrEmpty(requestData))
                    {
                        OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                        ewayerror.ErrorCode = 0;
                        ewayerror.Orderid = param.orderid;
                        ewayerror.IsActive = true;
                        ewayerror.IsDeleted = false;
                        ewayerror.Remarks = "B2CGenerateEwayBillApi";
                        ewayerror.Type = "response";
                        ewayerror.ModifiedBy = null;
                        ewayerror.ModifiedDate = null;
                        ewayerror.ErrorDescription = error;//requestData;
                        ewayerror.CreatedBy = userid;
                        ewayerror.CreatedDate = DateTime.Now;
                        orderEwayBillsGenErrors.Add(ewayerror);
                    }
                }
                else if (Convert.ToInt32(ewbres_data1.status) == 1)
                {
                    string data = DecryptBySymmerticKey(ewbres_data1.data, Convert.FromBase64String(sek));
                    byte[] reqDatabytes = Convert.FromBase64String(data);
                    string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                    var ewbres_final = JsonConvert.DeserializeObject<ewayBill_data>(requestData);
                    if (!string.IsNullOrEmpty(ewbres_final.ewayBillNo))
                    {
                        OrderEwayBill bill = new OrderEwayBill
                        {
                            OrderId = param.orderid,
                            EwayBillNo = ewbres_final.ewayBillNo,
                            EwayBillDate = Convert.ToDateTime(ewbres_final.ewayBillDate, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                            EwayBillValidTill = Convert.ToDateTime(ewbres_final.validUpto, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                            IsCancelEwayBill = false,
                            EwayBillCancelDate = null,
                            IsActive = true,
                            CreateDate = Convert.ToDateTime(ewbres_final.ewayBillDate, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                            CreatedBy = userid,
                            ModifiedBy = null,
                            ModifiedDate = null,
                            CustomerType = 1,//B2C
                            VehicleNumber = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString()
                        };
                        orderEwayBills.Add(bill);
                        clearTaxReqResp.ErrorDescription = JsonConvert.SerializeObject(ewbres_final);
                        orderEwayBillsGenErrors.Add(clearTaxReqResp);
                    }
                }
                b2CResponseDc.orderEwayBillsGenErrors = orderEwayBillsGenErrors;
                b2CResponseDc.orderEwayBills = orderEwayBills;
                return b2CResponseDc;
            }
            catch (Exception webex)
            {
                throw webex;
            }
        }
        #endregion


        #region  Generate Eway Bill NONIRN B2C

        public async Task<bool> GeneratedEwayBillNonIRN(AuthContext auth, Ewaygenerateparam param, int userid/* EwayBillBackendDcNonIRN param,*/ )
        {
            bool status = false;
            var OrderDispatchMaster = auth.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == param.orderid);

            var ware = this.GetWarehouseDetails(OrderDispatchMaster.WarehouseId, auth);

            var CompanyDetails = this.GetcompanyDetails(auth);

            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                authtoken = CompanyDetails.eInvoiceAuthKey;

                OrderB2CNonIRNResponse responseEwayBillbyIRNDC = null;
                List<EwayResponceDcAll> responseEwayBillbyIRNDCAll = new List<EwayResponceDcAll>();

                List<OrderB2CNonIRNRequestDc> responseEwayBillIRNDCList = null;
                EwayResponceDcAll ewayResponceDc = new EwayResponceDcAll();

                //Creating Request Data

                OrderB2CNonIRNRequestDc requestdata = await CreateNonIRNRequestDataB2C(auth, OrderDispatchMaster, ware, param);
                var RequestjsonString = JsonConvert.SerializeObject(requestdata);

                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration()
                {
                    OrderId = OrderDispatchMaster.OrderId,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    IsProcessed = false,
                    APIType = "GenerateEWB",
                    APITypes = 1,//GenerateEWBInternal
                };
                auth.ClearTaxIntegrations.Add(clearTaxIntegration);
                ewayResponceDc.clearTaxIntegration = (clearTaxIntegration);

                ClearTaxReqResp ctrp = new ClearTaxReqResp()
                {
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    OrderId = OrderDispatchMaster.OrderId,
                    Json = RequestjsonString,
                    Url = BaseUrl
                };
                auth.ClearTaxReqResps.Add(ctrp);
                ewayResponceDc.OrderId = OrderDispatchMaster.OrderId;
                ewayResponceDc.ClearTaxRequest = ctrp;

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BaseUrl + "/v3/ewaybill/generate"))
                        {
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("NoEncryption", "1");
                            request.Headers.TryAddWithoutValidation("X-Cleartax-Auth-Token", authtoken);
                            request.Headers.TryAddWithoutValidation("gstin", ware.GSTin);
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Content = new StringContent(RequestjsonString);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var Response = await httpClient.SendAsync(request);

                            if (Response.StatusCode == HttpStatusCode.OK && Response.IsSuccessStatusCode)
                            {
                                string responseBody = Response.Content.ReadAsStringAsync().Result;
                                responseEwayBillbyIRNDC = JsonConvert.DeserializeObject<OrderB2CNonIRNResponse>(responseBody);

                                if (Response.Content != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                                {
                                    ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                    {
                                        CreateDate = DateTime.Now,
                                        IsActive = true,
                                        Type = "Response",
                                        OrderId = param.orderid,
                                        Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC),
                                        Url = BaseUrl
                                    };
                                    auth.ClearTaxReqResps.Add(clearTaxReqResps);
                                    ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                    //ClearTaxIntegration clearTaxAdd = new ClearTaxIntegration()
                                    //{
                                    //    OrderId = OrderDispatchMaster.OrderId,
                                    //    IsActive = true,
                                    //    CreateDate = DateTime.Now,
                                    //    IsProcessed = false,
                                    //    APIType = "GenerateEWB",
                                    //    APITypes = 1,//GenerateEWBInternal
                                    //};
                                    //auth.ClearTaxIntegrations.Add(clearTaxAdd);
                                    //ewayResponceDc.clearTaxIntegration = (clearTaxIntegration);

                                    OrderEwayBill bill = new OrderEwayBill
                                    {
                                        OrderId = param.orderid,
                                        EwayBillNo = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString(),
                                        EwayBillDate = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbDt, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                                        EwayBillValidTill = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbValidTill, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                                        IsCancelEwayBill = false,
                                        EwayBillCancelDate = null,
                                        IsActive = true,
                                        CreateDate = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbDt, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                                        CreatedBy = userid,
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        CustomerType = 1,//B2C
                                        VehicleNumber = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString()
                                    };
                                    auth.OrderEwayBills.Add(bill);
                                    var ewayupdate = auth.OrderDispatchedMasters.Where(x => x.OrderId == param.orderid).FirstOrDefault(); //order id
                                    if (ewayupdate != null)
                                    {
                                        ewayupdate.EwayBillNumber = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString();
                                    }
                                    auth.Entry(ewayupdate).State = EntityState.Modified;
                                    status = true;
                                }
                                else
                                {
                                    ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                    {
                                        CreateDate = DateTime.Now,
                                        IsActive = true,
                                        Type = "Response",
                                        OrderId = param.orderid,
                                        Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                                        Url = BaseUrl
                                    };
                                    ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                    auth.ClearTaxReqResps.Add(clearTaxReqResps);
                                }
                                responseEwayBillbyIRNDCAll.Add(ewayResponceDc);

                            }
                            else
                            {
                                ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                {
                                    CreateDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Response",
                                    OrderId = param.orderid,
                                    Json = Response.Content.ReadAsStringAsync().Result,//JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                                    Url = BaseUrl
                                };
                                ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                auth.ClearTaxReqResps.Add(clearTaxReqResps);
                                status = false;
                            }
                            responseEwayBillbyIRNDCAll.Add(ewayResponceDc);
                            auth.Commit();
                        }
                    }

                }
                catch (Exception exe)
                {
                    throw exe;
                }
            }

            return status;
        }

        #endregion

        #region   Creating Json Request Non IRN B2C //auth, OrderDispatchMaster, ware, param
        public async Task<OrderB2CNonIRNRequestDc> CreateNonIRNRequestDataB2C(AuthContext authcontext, OrderDispatchedMaster orderDispatchedMaster, Warehouse ware, Ewaygenerateparam param  /* EwayBillBackendDcNonIRN param*/)
        {
            var StateList = await authcontext.States.ToListAsync();
            var citylist = await authcontext.Cities.ToListAsync();
            var customer = authcontext.Customers.Where(x => x.CustomerId == orderDispatchedMaster.CustomerId).FirstOrDefault();

            var statecode = authcontext.Warehouses.FirstOrDefault(x => x.WarehouseId == orderDispatchedMaster.WarehouseId);
            string custStateCode = StateList.FirstOrDefault(s => s.Stateid == statecode.Stateid).ClearTaxStateCode;
            string whStateCode = ware.GSTin.Substring(0, 2);
            //string custStateCode = ClearTaxStateCode;

            OrderB2CNonIRNRequestDc responseDc = new OrderB2CNonIRNRequestDc();
            List<EwaybillByIRNDc.ItemList> listtoadd = new List<EwaybillByIRNDc.ItemList>();
            bool Igst = false;
            SellerDtls sellerDtls = new SellerDtls()
            {
                Gstin = ware.GSTin,
                LglNm = ware.CompanyName,
                TrdNm = ware.CompanyName,
                Addr1 = ware.Address,
                Addr2 = null,
                Loc = ware.CityName,
                Pin = ware.PinCode,
                Stcd = whStateCode,
            };

            BuyerDtls buyerDtls = new BuyerDtls()
            {
                Gstin = "URP",
                LglNm = customer.NameOnGST,
                TrdNm = customer.ShopName,
                Addr1 = customer.BillingAddress,
                Addr2 = null,
                Loc = customer.BillingCity,
                Pin = !string.IsNullOrEmpty(customer.BillingZipCode) ? Convert.ToInt32(customer.BillingZipCode) : 0,
                Stcd = custStateCode
            };
            if (whStateCode != custStateCode && !string.IsNullOrEmpty(customer.RefNo))
            {
                Igst = true;
            }
            var items = authcontext.OrderDispatchedDetailss.Where(x => x.OrderId == param.orderid).ToList();
            List<int> itemMultiMrpIds = items.Select(z => z.ItemMultiMRPId).Distinct().ToList();
            List<int> itemIds = items.Select(z => z.ItemId).Distinct().ToList();
            var mrpList = authcontext.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();
            var itemMasterslist = authcontext.itemMasters.Where(x => itemIds.Contains(x.ItemId)).ToList();

            foreach (var item in items)
            {
                // var multiMrp = multiMRPs.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                double igstamt = 0;
                double CGSTTaxAmmount = 0;
                double SGSTTaxAmmount = 0;

                var withoutTaxUnitPrice = item.CGSTTaxPercentage + item.SGSTTaxPercentage > 0 ?
                                        item.UnitPrice / (1 + ((item.CGSTTaxPercentage + item.SGSTTaxPercentage + item.TotalCessPercentage) / 100))
                                        : item.UnitPrice;
                if (Igst)
                {
                    igstamt = item.CGSTTaxAmmount + item.SGSTTaxAmmount;
                }
                else
                {
                    CGSTTaxAmmount = item.CGSTTaxAmmount;
                    SGSTTaxAmmount = item.SGSTTaxAmmount;
                }

                EwaybillByIRNDc.ItemList listi = new EwaybillByIRNDc.ItemList()
                {
                    ProdName = item.itemname,//rr
                    ProdDesc = item.itemname,//rr
                    HsnCd = itemMasterslist.FirstOrDefault(x => x.ItemId == item.ItemId)?.HSNCode,//rr
                    Qty = Convert.ToInt32(item.qty),//rr
                    Unit = "PCS",
                    CgstRt = item.CGSTTaxPercentage,
                    CgstAmt = CGSTTaxAmmount,
                    SgstRt = item.CGSTTaxPercentage,
                    SgstAmt = SGSTTaxAmmount,
                    IgstRt = 0,
                    IgstAmt = igstamt,
                    CesRt = item.TotalCessPercentage,
                    CesAmt = 0,
                    CesNonAdvAmt = 0,
                    AssAmt = item.AmtWithoutTaxDisc, //rr
                    OthChrg = 0,
                };
                listtoadd.Add(listi);
            }

            double igstamt1 = 0;
            double CGSTTaxAmmount1 = 0;
            double SGSTTaxAmmount1 = 0;
            if (Igst)
            {
                igstamt1 = orderDispatchedMaster.CGSTTaxAmmount + orderDispatchedMaster.SGSTTaxAmmount;
            }
            else
            {
                CGSTTaxAmmount1 = orderDispatchedMaster.CGSTTaxAmmount;
                SGSTTaxAmmount1 = orderDispatchedMaster.SGSTTaxAmmount;
            }

            responseDc.SupplyType = "O";
            responseDc.SubSupplyType = "1";
            responseDc.SubSupplyTypeDesc = "";
            responseDc.DocumentType = "INV";
            responseDc.DocumentNumber = orderDispatchedMaster.invoice_no;
            responseDc.DocumentDate = orderDispatchedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            responseDc.TransactionType = "Regular";
            responseDc.Distance = param.distance;
            //Warehouse details
            responseDc.SellerDtls = sellerDtls;
            //Buyer(customer details)
            responseDc.BuyerDtls = buyerDtls;
            //itemdetils
            responseDc.ItemList = listtoadd;
            //Invoice 
            responseDc.TotalInvoiceAmount = orderDispatchedMaster.GrossAmount;
            responseDc.OtherAmount = (orderDispatchedMaster.deliveryCharge) + (-Math.Abs((orderDispatchedMaster.BillDiscountAmount ?? 0) + (orderDispatchedMaster.WalletAmount ?? 0)));
            responseDc.OtherTcsAmount = orderDispatchedMaster.TCSAmount;
            responseDc.TotalAssessableAmount = listtoadd.Sum(x => x.AssAmt);
            responseDc.TotalSgstAmount = SGSTTaxAmmount1;
            responseDc.TotalCgstAmount = CGSTTaxAmmount1;
            responseDc.TotalIgstAmount = igstamt1;
            responseDc.TotalCessAmount = listtoadd.Sum(x => x.CesAmt);
            responseDc.TotalCessNonAdvolAmount = 0;
            //transporter details
            responseDc.TransId = param.transportergst;
            responseDc.TransName = param.transportername;
            responseDc.TransMode = "ROAD";
            responseDc.TransDocDt = DateTime.Now.ToString("dd/MM/yyyy");
            responseDc.VehNo = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString();
            responseDc.VehType = "R";
            string jsonData = System.Text.Json.JsonSerializer.Serialize(responseDc);
            //OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
            //{
            //    CreatedDate = DateTime.Now,
            //    IsActive = true,
            //    Type = "Request",
            //    Remarks = "B2CGenerateEwayBillApi",
            //    Orderid = param.orderid,
            //    ErrorDescription = jsonData,
            //};
            //authcontext.OrderEwayBillsGenErrors.Add(ctrp);
            return responseDc;
        }
        #endregion

        #region Generate Auth Token Key
        public AuthKeyReturn GenerateAuth(int userid, EwayBillConfigDC configres)
        {
            AuthKeyReturn datareturn = new AuthKeyReturn();
            datareturn.Isgenereted = false;

            using (AuthContext context = new AuthContext())
            {
                HttpClient client = new HttpClient();
                byte[] _aeskey = generateSecureKey();
                var straesKey = Convert.ToBase64String(_aeskey);

                dynamic Request_Json = new JObject();
                Request_Json.action = "ACCESSTOKEN";
                Request_Json.UserName = configres.UserName;
                Request_Json.Password = configres.Password;
                Request_Json.app_key = straesKey;
                string EINV_Json = Request_Json.ToString();

                string Request_Body_payload = Encrypt(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(EINV_Json)), configres.Publickey);

                //cls.WriteLog("Encrypted JSON :" + Request_Body_payload);
                dynamic Request_Body_payload_Encrypted = new JObject();
                Request_Body_payload_Encrypted.action = "ACCESSTOKEN";
                Request_Body_payload_Encrypted.Data = Request_Body_payload;

                string str_Request_Body_payload_Encrypted = Request_Body_payload_Encrypted.ToString();

                string EWB_New_Auth_url = configres.URL + "/v1.03/auth";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EWB_New_Auth_url);
                //HttpStatusCode statusCode;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = str_Request_Body_payload_Encrypted.Length;
                request.Headers.Add("client-id", configres.clientID);
                request.Headers.Add("client-secret", configres.ClintSecretID);
                request.Headers.Add("Gstin", configres.GSTin); //configres.Select(x => x.GSTin).FirstOrDefault());

                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(str_Request_Body_payload_Encrypted);
                requestWriter.Close();

                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string response = responseReader.ReadToEnd();
                responseReader.Close();
                //statusCode = webResponse.StatusCode;

                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    JObject obj_webResponse = JObject.Parse(response);
                    string Response_status = (string)obj_webResponse["status"];

                    if (Response_status == "1")
                    {
                        //both will be inserted in table authtoken,sek
                        datareturn.EWB_Response_authtoken = (string)obj_webResponse["authtoken"];
                        datareturn.EWB_Response_sek = (string)obj_webResponse["sek"];
                        datareturn.generateSecureKey = straesKey;
                        string AuthToken = datareturn.EWB_Response_authtoken;
                        string EncryptedSek = datareturn.EWB_Response_sek;
                        string DecryptedSek = DecryptBySymmerticKey(EncryptedSek, _aeskey);
                        AuthTokenGeneration auth1 = new AuthTokenGeneration();
                        var Deauth = context.AuthTokenGenerations.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                        if (Deauth.Count() > 0)
                        {
                            Deauth.ForEach(x =>
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                                context.Entry(x).State = EntityState.Modified;
                            });
                        }
                        AuthTokenGeneration auth = new AuthTokenGeneration
                        {
                            AuthToken = datareturn.EWB_Response_authtoken,
                            sek = datareturn.EWB_Response_sek,
                            generateSecureKey = straesKey,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = null,
                            ModifiedBy = null,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = userid,
                        };
                        context.AuthTokenGenerations.Add(auth);
                        if (context.Commit() > 0)
                        {
                            datareturn.Isgenereted = true;
                        }
                    }
                    else
                    {
                        JObject obj_webResponse1 = JObject.Parse(response);
                        string error = (string)obj_webResponse["error"];
                        byte[] reqDatabytes = Convert.FromBase64String(error);
                        string requestData = System.Text.Encoding.UTF8.GetString(reqDatabytes);
                    }
                }
            }
            return datareturn;
        }
        #endregion


        public string EncryptBySymmetricKey(string text, string sek)
        {
            try
            {
                byte[] dataToEncrypt = Convert.FromBase64String(text);
                var keyBytes = Convert.FromBase64String(sek);
                AesManaged tdes = new AesManaged();
                tdes.KeySize = 256;
                tdes.BlockSize = 128;
                tdes.Key = keyBytes;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform encrypt__1 = tdes.CreateEncryptor();
                byte[] deCipher = encrypt__1.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                tdes.Clear();
                string EK_result = Convert.ToBase64String(deCipher);
                return EK_result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DecryptBySymmerticKey(string encryptedText, byte[] key)
        {
            try
            {
                byte[] dataToDecrypt = Convert.FromBase64String(encryptedText);
                var keyBytes = key;
                AesManaged tdes = new AesManaged();
                tdes.KeySize = 256;
                tdes.BlockSize = 128;
                tdes.Key = keyBytes;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform decrypt__1 = tdes.CreateDecryptor();
                byte[] deCipher = decrypt__1.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
                tdes.Clear();
                string EK_result = Convert.ToBase64String(deCipher);
                // var EK = Convert.FromBase64String(EK_result);
                // return EK;
                return EK_result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Encrypt(string data, string key)
        {
            byte[] keyBytes =
            Convert.FromBase64String(key); // your key here
            AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(keyBytes);
            RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;
            RSAParameters rsaParameters = new RSAParameters();
            rsaParameters.Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned();
            rsaParameters.Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParameters);
            byte[] plaintext = Encoding.UTF8.GetBytes(data);
            byte[] ciphertext = rsa.Encrypt(plaintext, false);
            string cipherresult = Convert.ToBase64String(ciphertext);
            //string cipherresult = Encoding.ASCII.GetString(ciphertext);
            return cipherresult;
        }

        public bool TokenGenerateTime(DateTime oldTime, out bool IsAuthGenrate)
        {


            List<TimeRange> timeRangeList = new List<TimeRange>();

            TimeRange timeRange = new TimeRange();

            timeRange.Id = 0;
            timeRange.FromTime = new TimeSpan(0, 0, 0);
            timeRange.ToTime = new TimeSpan(05, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 2;
            timeRange.FromTime = new TimeSpan(06, 00, 00);
            timeRange.ToTime = new TimeSpan(11, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 3;
            timeRange.FromTime = new TimeSpan(12, 00, 00);
            timeRange.ToTime = new TimeSpan(17, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 4;
            timeRange.FromTime = new TimeSpan(18, 00, 00);
            timeRange.ToTime = new TimeSpan(23, 59, 59);
            timeRangeList.Add(timeRange);

            DateTime currentTime = DateTime.Now;
            //  DateTime oldTime = Convert.ToDateTime("2023-05-19 17:00:00.000");//tokan Time

            //var timerangeData = timeRangeList.Where(x => x.FromTime.Hours > currentTime.Hour && x.ToTime.Hours <= currentTime.Hour).FirstOrDefault();

            var timerangeDataNew = timeRangeList.FirstOrDefault(x => currentTime.Hour >= x.FromTime.Hours && currentTime.Hour <= x.ToTime.Hours);

            if (oldTime.Hour >= timerangeDataNew.FromTime.Hours && oldTime.Hour <= timerangeDataNew.ToTime.Hours)
            {
                IsAuthGenrate = false;
            }
            else
            {
                IsAuthGenrate = true;
            }
            return IsAuthGenrate;
        }

        public class TimeRange
        {
            public int Id { get; set; }
            public TimeSpan FromTime { get; set; }
            public TimeSpan ToTime { get; set; }
        }
        public class AuthTokenGenerationDc
        {
            public string EWB_Response_authtoken { get; set; }
            public string DecryptedSek { get; set; }
            public EwayBillConfigDC configres { get; set; }
        }
        public class errorCodeB2C
        {
            public string errorCodes { get; set; }
        }
        public class B2CResponseDc
        {
            public List<OrderEwayBillsGenError> orderEwayBillsGenErrors { get; set; }
            public List<OrderEwayBill> orderEwayBills { get; set; }
        }
    }
}
