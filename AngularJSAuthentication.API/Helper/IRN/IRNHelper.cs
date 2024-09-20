using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.ClearTax;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class IRNHelper
    {
        public string BaseUrl = string.Empty;
        public static string eInvoiceAuthKey = string.Empty;
        public string eInvoiceVersion = string.Empty;

        public async Task<bool> PostIRNToClearTax(int? orderId = null)
        {
            bool result = false;

            using (var context = new AuthContext())
            {
                var CompanyDetails = context.CompanyDetailsDB.Where(x => x.IsDeleted == false && x.IsActive == true && x.eInvoiceEnable == true).FirstOrDefault();
                if (CompanyDetails != null)
                {
                    BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                    eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;
                    eInvoiceVersion = CompanyDetails.eInvoiceVersion;
                }

                var uomMappings = context.GstPortalUomMappings.Where(x => x.IsActive).ToList();
                var CtInt = new List<ClearTaxIntegration>();

                if (!orderId.HasValue || orderId.Value == 0)
                    CtInt = context.ClearTaxIntegrations.Where(x => !x.IsProcessed && x.IsActive && x.APIType != "GetIRN" && !x.IsOnline && x.APITypes!=2)
                                .OrderBy(x => x.CreateDate).Skip(0).Take(100).ToList();
                else
                    CtInt = context.ClearTaxIntegrations.Where(x => !x.IsProcessed && x.IsActive && x.OrderId == orderId.Value)
                                .OrderBy(x => x.CreateDate).Skip(0).Take(100).ToList();


                // Creating a request Body Data.
                foreach (var item in CtInt)
                {
                    result = await ProcessClearTaxAPI(item, context, uomMappings);
                }


                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IRNErrorEmailIds"]) && CtInt.Any(x => !string.IsNullOrEmpty(x.Error)))
                {
                    var errorList = CtInt.Where(x => !string.IsNullOrEmpty(x.Error)).Select(x => "<li>" + x.OrderId + "<br/> Error:" + x.Error + "</li>").ToList();

                    string error = string.Format("IRN generation failed for Below Order Ids: {0} <ul>{1}</ul>", "<br/>"
                                                        , string.Join("", errorList));

                    EmailHelper.SendMail(AppConstants.MasterEmail, ConfigurationManager.AppSettings["IRNErrorEmailIds"], "",
                                        ConfigurationManager.AppSettings["Environment"] + " IRN Generation Failed "
                                        , error
                                        , "");
                }

                context.Commit();

            }
            return result;
        }

        private async Task<bool> ProcessClearTaxAPI(ClearTaxIntegration item, AuthContext context, List<GstPortalUomMapping> uomMappings)
        {
            bool result = false;
            try
            {
                var orderDispachedMaster = await context.OrderDispatchedMasters.FirstOrDefaultAsync(x => x.OrderId == item.OrderId);

                // WareHouse
                var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == orderDispachedMaster.WarehouseId);

                string qrCodeRelativePath = "~/images/eInvoiceQrCodes";
                string qrCodeSavePath = HostingEnvironment.MapPath(qrCodeRelativePath);
                ClearTaxResponseDc clearTaxResponse = new ClearTaxResponseDc();

                if (!Directory.Exists(qrCodeSavePath))
                    Directory.CreateDirectory(qrCodeSavePath);

                if (item.APIType == "GenerateCN" || item.APIType == "GenerateIRN")
                {
                    clearTaxResponse = await GenerateCnOrIrn(context, item, uomMappings, qrCodeRelativePath, qrCodeSavePath, orderDispachedMaster, warehouse);
                }
                else if (item.APIType == "GetIRN" || item.APIType == "GetCN")
                {
                    clearTaxResponse = await GettingEInvoicebyIRN(context, item, warehouse);
                }

                item.RequestId = clearTaxResponse.RequestId;
                item.ResponseId = clearTaxResponse.ResponseId;
                item.Error = clearTaxResponse.ErrorMessage;
                item.IRNNo = item.APIType != "GetIRN" ? clearTaxResponse.IrnNo : item.IRNNo;
                item.IsProcessed = true;
                item.ProcessDate = DateTime.Now;
                context.Entry(item).State = EntityState.Modified;

                if (item.RequestId > 0 && item.ResponseId.HasValue && item.ResponseId.Value > 0
                    && !string.IsNullOrEmpty(item.IRNNo) && string.IsNullOrEmpty(item.Error))
                {
                    UpdateOrderDispatchMaster(orderDispachedMaster, context, qrCodeSavePath, qrCodeRelativePath, clearTaxResponse);
                    result = true;
                }
            }
            catch (Exception exe)
            {
                item.Error = exe.ToString();
                item.IsProcessed = true;
                item.ProcessDate = DateTime.Now;
                context.Entry(item).State = EntityState.Modified;
            }

            context.Commit();

            return result;
        }

        async Task<ClearTaxResponseDc> GettingEInvoicebyIRN(AuthContext context, ClearTaxIntegration item, Warehouse warehouse)
        {
            ResponseIRNDC responseIRNDC = null;
            List<ResponseIRNDC> responseIRNDCList = null;
            var RequestjsonString = item.IRNNo;
            string errorMessage = "", irnNo = "";
            string qrCode = "";

            ClearTaxReqResp ctrp = new ClearTaxReqResp()
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = "Request",
                OrderId = item.OrderId,
                Json = RequestjsonString,
                Url = BaseUrl + "v2/eInvoice/get?irn=" + item.IRNNo
            };
            context.ClearTaxReqResps.Add(ctrp);

            ClearTaxReqResp clearTaxReqResp = new ClearTaxReqResp()
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = "Response",
                OrderId = item.OrderId,
                //Json = JsonConvert.SerializeObject(responseIRNDC),
                Url = BaseUrl + "v2/eInvoice/get?irn=" + item.IRNNo
            };


            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("x-cleartax-auth-token", eInvoiceAuthKey);
                    client.DefaultRequestHeaders.Add("x-cleartax-product", "EInvoice");
                    client.DefaultRequestHeaders.Add("owner_id", warehouse.CleartaxEntityId);
                    client.DefaultRequestHeaders.Add("gstin", warehouse.GSTin);

                    BaseUrl = BaseUrl + "/v2/eInvoice/get?irn=" + item.IRNNo;

                    HttpResponseMessage httpResponseMessage = await client.GetAsync(BaseUrl);
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonString = (await httpResponseMessage.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                        responseIRNDCList = JsonConvert.DeserializeObject<List<ResponseIRNDC>>(jsonString);

                        responseIRNDC = responseIRNDCList.FirstOrDefault();
                        clearTaxReqResp.Json = JsonConvert.SerializeObject(responseIRNDC);
                        context.ClearTaxReqResps.Add(clearTaxReqResp);

                        if (responseIRNDC != null && responseIRNDC.govt_response != null
                                && !string.IsNullOrEmpty(responseIRNDC.govt_response.Success)
                                && responseIRNDC.govt_response.Success == "Y")
                        {
                            irnNo = responseIRNDC.govt_response.Irn;
                            qrCode = responseIRNDC.govt_response.SignedQRCode;
                        }
                        else
                        {
                            errorMessage = JsonConvert.SerializeObject(responseIRNDC.govt_response.ErrorDetails);
                        }
                    }
                }

                #region GetAPICALLOLD
                //List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;

                //extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                //                {
                //                    new KeyValuePair<string, IEnumerable<string>>
                //                    (
                //                      "x-cleartax-auth-token",
                //                      new List<string> { eInvoiceAuthKey }
                //                    ),
                //                    new KeyValuePair<string, IEnumerable<string>>
                //                    (
                //                      "x-cleartax-product",
                //                      new List<string> { "EInvoice" }
                //                    ),
                //                    new KeyValuePair<string, IEnumerable<string>>
                //                    (
                //                      "owner_id",
                //                      new List<string> { warehouse.CleartaxEntityId }
                //                    ),
                //                    new KeyValuePair<string, IEnumerable<string>>
                //                    (
                //                      "gstin",
                //                      new List<string> { warehouse.GSTin }
                //                    ),
                //                };

                //using (var client = new GenericRestHttpClient<List<ResponseIRNDC>, string>(BaseUrl, "v2/eInvoice/get?irn=" + item.IRNNo, extraDataAsHeader) /*GetHttpClient(BaseUrl, eInvoiceAuthKey)*/)
                //{
                //    string badRequestResponse = "";




                //    var strResult = await client.GetStringAsync();

                //    responseIRNDCList = JsonConvert.DeserializeObject<List<ResponseIRNDC>>(strResult);
                //    if (!string.IsNullOrEmpty(badRequestResponse))
                //    {
                //        clearTaxReqResp.Json = badRequestResponse;
                //        errorMessage = badRequestResponse;
                //        context.ClearTaxReqResps.Add(clearTaxReqResp);
                //    }
                //    else
                //    {
                //        responseIRNDC = responseIRNDCList.FirstOrDefault();
                //        clearTaxReqResp.Json = JsonConvert.SerializeObject(responseIRNDC);
                //        context.ClearTaxReqResps.Add(clearTaxReqResp);

                //        if (responseIRNDC != null && responseIRNDC.govt_response != null
                //                && !string.IsNullOrEmpty(responseIRNDC.govt_response.Success)
                //                && responseIRNDC.govt_response.Success == "Y")
                //        {
                //            irnNo = responseIRNDC.govt_response.Irn;
                //            qrCode = responseIRNDC.govt_response.SignedQRCode;
                //        }
                //        else
                //        {
                //            errorMessage = JsonConvert.SerializeObject(responseIRNDC.govt_response.ErrorDetails);
                //        }
                //    }
                //}
                #endregion
            }
            catch (Exception exe)
            {
                errorMessage = exe.ToString();
            }

            context.Commit();

            ClearTaxResponseDc response = new ClearTaxResponseDc
            {
                ApiType = item.APIType,
                ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : null,
                IrnNo = irnNo,
                QrCode = qrCode,
                RequestId = ctrp.Id,
                ResponseId = clearTaxReqResp.Id
            };

            return response;

        }

        async Task<ClearTaxResponseDc> GenerateCnOrIrn(AuthContext context, ClearTaxIntegration item, List<GstPortalUomMapping> uomMappings, string qrCodeRelativePath, string qrCodeSavePath, OrderDispatchedMaster orderDispachedMaster, Warehouse warehouse)
        {
            RequestIRNDC requestIRNDC = null;
            List<ResponseIRNDC> responseIRNDCList = null;
            ResponseIRNDC responseIRNDC = null;
            string errorMessage = "", irnNo = "";
            string qrCode = "";

            var StateList = await context.States.ToListAsync();

            var citylist = await context.Cities.ToListAsync();

            var customer = context.Customers.Where(x => x.CustomerId == orderDispachedMaster.CustomerId).FirstOrDefault();

            if (string.IsNullOrEmpty(customer.RefNo))
            {
                ClearTaxResponseDc errorResponse = new ClearTaxResponseDc
                {
                    ApiType = item.APIType,
                    ErrorMessage = "Customer's Gst No is empty",

                };
                return errorResponse;
            }

            var items = context.OrderDispatchedDetailss.Where(x => x.OrderId == item.OrderId).ToList();
            List<int> itemMultiMrpIds = items.Select(z => z.ItemMultiMRPId).Distinct().ToList();
            List<int> itemIds = items.Select(z => z.ItemId).Distinct().ToList();
            var mrpList = context.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();

            var itemMasterslist = await context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).ToListAsync();

            requestIRNDC = CreateIRNRequest(item, orderDispachedMaster, warehouse, customer, items, orderDispachedMaster, StateList, citylist, uomMappings, mrpList, itemMasterslist);

            var RequestjsonString = JsonConvert.SerializeObject(new List<RequestIRNDC> { requestIRNDC });
            ClearTaxReqResp ctrp = new ClearTaxReqResp()
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = "Request",
                OrderId = item.OrderId,
                Json = RequestjsonString,
                Url = BaseUrl
            };
            context.ClearTaxReqResps.Add(ctrp);

            ClearTaxReqResp clearTaxReqResp = new ClearTaxReqResp()
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = "Response",
                OrderId = item.OrderId,
                //Json = JsonConvert.SerializeObject(responseIRNDC),
                Url = BaseUrl
            };

            List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;

            extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                            {
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "x-cleartax-auth-token",
                                  new List<string> { eInvoiceAuthKey }
                                ),
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "x-cleartax-product",
                                  new List<string> { "EInvoice" }
                                ),
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "owner_id",
                                  new List<string> { warehouse.CleartaxEntityId }
                                ),
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "gstin",
                                  new List<string> { warehouse.GSTin }
                                ),
                            };

            try
            {

                using (var client = new GenericRestHttpClient<List<RequestIRNDC>, string>(BaseUrl, "v2/eInvoice/generate", extraDataAsHeader) /*GetHttpClient(BaseUrl, eInvoiceAuthKey)*/)
                {
                    string badRequestResponse = "";
                    responseIRNDCList = client.PutAsyncWithHandleError<List<ResponseIRNDC>>(new List<RequestIRNDC> { requestIRNDC }, out badRequestResponse);

                    if (!string.IsNullOrEmpty(badRequestResponse))
                    {
                        clearTaxReqResp.Json = badRequestResponse;
                        errorMessage = badRequestResponse;
                        context.ClearTaxReqResps.Add(clearTaxReqResp);
                    }
                    else
                    {
                        responseIRNDC = responseIRNDCList.FirstOrDefault();
                        clearTaxReqResp.Json = JsonConvert.SerializeObject(responseIRNDC);
                        context.ClearTaxReqResps.Add(clearTaxReqResp);

                        if (responseIRNDC != null && responseIRNDC.govt_response != null
                                && !string.IsNullOrEmpty(responseIRNDC.govt_response.Success)
                                && responseIRNDC.govt_response.Success == "Y")
                        {


                            irnNo = responseIRNDC.govt_response.Irn;
                            qrCode = responseIRNDC.govt_response.SignedQRCode;
                        }
                        else
                        {
                            errorMessage = JsonConvert.SerializeObject(responseIRNDC.govt_response.ErrorDetails);
                        }
                    }
                }
            }
            catch (Exception exe)
            {
                errorMessage = exe.ToString();
            }

            context.Commit();

            ClearTaxResponseDc response = new ClearTaxResponseDc
            {
                ApiType = item.APIType,
                ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : null,
                IrnNo = irnNo,
                QrCode = qrCode,
                RequestId = ctrp.Id,
                ResponseId = clearTaxReqResp.Id
            };

            return response;
        }

        bool UpdateOrderDispatchMaster(OrderDispatchedMaster orderDispachedMaster, AuthContext context, string qrCodeSavePath, string qrCodeRelativePath, ClearTaxResponseDc clearTaxResponse)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(clearTaxResponse.QrCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCodee = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCodee.GetGraphic(20);
            string imagePath = qrCodeSavePath + @"\" + clearTaxResponse.IrnNo + ".jpg";

            qrCodeImage.Save(imagePath);

            if (clearTaxResponse.ApiType == "GenerateIRN" || clearTaxResponse.ApiType == "GetIRN")
            {
                orderDispachedMaster.IRNNo = clearTaxResponse.IrnNo;
                orderDispachedMaster.IRNQRCode = clearTaxResponse.QrCode;
                orderDispachedMaster.IRNQRCodeUrl = qrCodeRelativePath.Replace("~", "") + "/" + clearTaxResponse.IrnNo + ".jpg";
            }
            else if (clearTaxResponse.ApiType == "GenerateCN" || clearTaxResponse.ApiType == "GetCN")
            {
                orderDispachedMaster.POCIRNNo = clearTaxResponse.IrnNo;
                orderDispachedMaster.POCIRNQRCode = clearTaxResponse.QrCode;
                orderDispachedMaster.POCIRNQRCodeURL = qrCodeRelativePath.Replace("~", "") + "/" + clearTaxResponse.IrnNo + ".jpg";
            }

            context.Entry(orderDispachedMaster).State = System.Data.Entity.EntityState.Modified;

            return true;
        }


        private RequestIRNDC CreateIRNRequest(ClearTaxIntegration clearTaxIntegration, OrderDispatchedMaster orderDispachedMaster, Warehouse warehouse, Customer customer, List<OrderDispatchedDetails> items, OrderDispatchedMaster orderDispatchedMaster, List<State> StateList, List<City> citylist, List<GstPortalUomMapping> uomMappings, List<ItemMultiMRP> multiMRPs, List<ItemMaster> itemMasters)
        {
            RequestIRNDC requestIRNDC;
            RefDtls refDtls = null;

            bool Igst = false;

            string whStateCode = warehouse.GSTin.Substring(0, 2);
            string custStateCode = customer.RefNo.Substring(0, 2);

            if (whStateCode != custStateCode)
            {
                Igst = true;
            }

            TranDtls tranDtls = new TranDtls()
            {
                TaxSch = "GST",
                SupTyp = "B2B",
                RegRev = "N",
                EcmGstin = null,
                IgstOnIntra = "N" // Igst ? "N" : "Y"
            };
            DocDtls docDtls = new DocDtls()
            {
                Typ = clearTaxIntegration.APIType == "GenerateIRN" ? "INV" : clearTaxIntegration.APIType == "GenerateCN" ? "CRN" : "",
                No = clearTaxIntegration.APIType == "GenerateIRN"
                        ? orderDispachedMaster.invoice_no
                        : clearTaxIntegration.APIType == "GenerateCN"
                            ? orderDispachedMaster.PocCreditNoteNumber
                            : "",
                Dt = clearTaxIntegration.APIType == "GenerateIRN"
                        ? orderDispachedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                        : clearTaxIntegration.APIType == "GenerateCN"
                            ? orderDispachedMaster.PocCreditNoteDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : ""

            };
            SellerDtls sellerDtls = new SellerDtls()
            {
                Gstin = warehouse.GSTin,
                LglNm = warehouse.CompanyName,
                TrdNm = warehouse.CompanyName,
                Addr1 = warehouse.Address,
                Addr2 = null,
                Loc = warehouse.CityName,
                Pin = warehouse.PinCode,
                Stcd = whStateCode,
                Em = null,
                Ph = null
            };

            var cityId = citylist.Where(z => z.Cityid == customer.Cityid).FirstOrDefault();

            BuyerDtls buyerDtls = new BuyerDtls()
            {
                Gstin = customer.RefNo,
                LglNm = customer.NameOnGST,
                TrdNm = customer.ShopName,
                Pos = custStateCode, //change as pr deepak k sir whStateCode,//"stateIdofWarehouse",
                Addr1 = StringTrim(customer.BillingAddress, 100),
                Addr2 = null,
                Loc = customer.BillingCity,
                Pin = !string.IsNullOrEmpty(customer.BillingZipCode) ? Convert.ToInt32(customer.BillingZipCode) : 0,
                Stcd = custStateCode,
                Ph = null,
                Em = null
            };
            DispDtls dispDtls = new DispDtls()
            {
                Nm = warehouse.CompanyName,
                Addr1 = warehouse.Address,
                Addr2 = null,
                Loc = warehouse.CityName,
                Pin = warehouse.PinCode,
                Stcd = whStateCode,
            };

            //var statename =  citylist.Where(z => z.CityName == customer.ShippingCity).FirstOrDefault()?.StateName;
            ShipDtls shipDtls = new ShipDtls()
            {
                Gstin = customer.RefNo,
                LglNm = customer.NameOnGST,
                TrdNm = customer.ShopName,
                Addr1 = StringTrim(customer.ShippingAddress, 100),
                Addr2 = null,
                Loc = customer.ShippingCity,
                Pin = !string.IsNullOrEmpty(customer.ZipCode) ? Convert.ToInt32(customer.ZipCode) : 0,
                Stcd = StateList.FirstOrDefault(x => x.StateName == customer.State)?.ClearTaxStateCode
            };



            List<ItemList> itemLists = new List<ItemList>();


            int cnt = 1;
            foreach (var item in items)
            {
                var multiMrp = multiMRPs.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
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

                ItemList itemList = new ItemList()
                {
                    SlNo = Convert.ToString(cnt),
                    PrdDesc = item.itemname,
                    IsServc = "N",
                    HsnCd = itemMasters.FirstOrDefault(x => x.ItemId == item.ItemId)?.HSNCode,
                    Barcde = null,
                    Qty = item.qty,
                    FreeQty = 0,
                    Unit = "PCS",///uomMappings.FirstOrDefault(x => x.SKUom == multiMrp.UOM)?.GstUom,//"Cleartaxunit",
                    UnitPrice = Math.Round(withoutTaxUnitPrice, 3),
                    TotAmt = item.AmtWithoutTaxDisc,
                    Discount = 0,
                    PreTaxVal = 0,
                    AssAmt = item.AmtWithoutTaxDisc,
                    GstRt = item.CGSTTaxPercentage + item.SGSTTaxPercentage,
                    IgstAmt = igstamt,
                    SgstAmt = SGSTTaxAmmount,
                    CgstAmt = CGSTTaxAmmount,
                    CesRt = item.TotalCessPercentage,
                    CesAmt = item.CessTaxAmount,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal = item.TotalAmt,
                    OrdLineRef = null,
                    OrgCntry = null,
                    PrdSlNo = null,
                    BchDtls = null,
                    AttribDtls = null
                };

                itemLists.Add(itemList);
                cnt++;
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

            ValDtls valDtls = new ValDtls()
            {
                AssVal = itemLists.Sum(x => x.AssAmt),
                CgstVal = CGSTTaxAmmount1,
                SgstVal = SGSTTaxAmmount1,
                CesVal = itemLists.Sum(x => x.CesAmt),
                IgstVal = igstamt1,
                StCesVal = 0,
                Discount = (orderDispatchedMaster.BillDiscountAmount ?? 0) + (orderDispachedMaster.WalletAmount ?? 0),
                OthChrg = orderDispatchedMaster.deliveryCharge + orderDispachedMaster.TCSAmount,
                RndOffAmt = 0,
                TotInvVal = orderDispatchedMaster.GrossAmount,
                TotInvValFc = 0
            };

            if (clearTaxIntegration.APIType == "GenerateCN")
            {
                refDtls = new RefDtls
                {
                    PrecDocDtls = new List<PrecDocDtl>
                    {
                        new PrecDocDtl
                        {
                            InvNo=orderDispachedMaster.invoice_no,
                            InvDt=orderDispachedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),

                        }
                    }
                };
            }

            Transaction transactions = new Transaction()
            {
                Version = eInvoiceVersion,
                TranDtls = tranDtls,
                DocDtls = docDtls,
                SellerDtls = sellerDtls,
                BuyerDtls = buyerDtls,
                DispDtls = dispDtls,
                ShipDtls = shipDtls,
                ItemList = itemLists,
                ValDtls = valDtls,
                PayDtls = null,
                RefDtls = refDtls,
                AddlDocDtls = null,
                ExpDtls = null,
                EwbDtls = null,
                custom_fields = null
            };

            requestIRNDC = new RequestIRNDC()
            {
                transaction = transactions
            };
            return requestIRNDC;
        }

        public bool IsGenerateIRN(AuthContext context, int customerId)
        {
            var param = new SqlParameter("@customerId", customerId);
            bool result = context.Database.SqlQuery<bool>("exec [Picker].[IsGenerateIRN] @customerId ", param).FirstOrDefault();

            return result;
        }

        public async Task<bool> IsGenerateIRNAsync(AuthContext context, int customerId)
        {
            var customer = await context.Customers.Where(x => x.CustomerId == customerId).FirstOrDefaultAsync();

            var custverify = await context.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).FirstOrDefaultAsync();
            return custverify != null;
        }
        public string StringTrim(string input, int maxLength)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Length > maxLength)
                    return input.Substring(0, maxLength);
            }
            return input;
        }



    }
}