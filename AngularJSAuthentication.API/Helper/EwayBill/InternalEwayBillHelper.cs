using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.ClearTax;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using Newtonsoft.Json;
using QRCoder;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Hosting;
using static AngularJSAuthentication.API.EwayBill.InternalEwaybillController;
using static AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc;
using static AngularJSAuthentication.DataContracts.EwayBill.InternalTransferEwaybillDc;

namespace AngularJSAuthentication.API.Helper.EwayBill
{

    public class InternalEwayBillHelper
    {
        public string BaseUrl = string.Empty;
        public static string eInvoiceAuthKey = string.Empty;
        public string eInvoiceVersion = string.Empty;

        #region Generate IRN For Internal
        public async Task<bool> PostInternalTransferIRNToClearTax(string TransportGST = null, string TransportName = null, int? TransferOrderId = null)
        {
            bool result = false;
            var CtInt = new List<ClearTaxIntegration>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var CompanyDetails = context.CompanyDetailsDB.Where(x => x.IsDeleted == false && x.IsActive == true && x.eInvoiceEnable == true).FirstOrDefault();
                if (CompanyDetails != null)
                {
                    BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                    eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;
                    eInvoiceVersion = CompanyDetails.eInvoiceVersion;
                }
                if (!TransferOrderId.HasValue || TransferOrderId.Value == 0)
                {
                    CtInt = context.ClearTaxIntegrations.Where(x => !x.IsProcessed && x.IsActive && x.APITypes == 2 && !x.IsOnline && x.APIType == "GenerateIRN").OrderBy(x => x.CreateDate).Skip(0).Take(5).ToList();  //APITypes=internal
                }
                else
                {
                    CtInt = context.ClearTaxIntegrations.Where(x => !x.IsProcessed && x.IsActive && x.OrderId == TransferOrderId.Value && x.APITypes == 2).OrderBy(x => x.CreateDate).Skip(0).Take(100).ToList();
                }
                foreach (var item in CtInt)
                {
                    double? amount = context.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderId == item.OrderId).Sum(x => x.NPP * x.TotalQuantity);

                    var Trdisptch = await context.TransferWHOrderDispatchedMasterDB.FirstOrDefaultAsync(x => x.TransferOrderId == item.OrderId);
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == Trdisptch.RequestToWarehouseId);
                    string wh1StateCode = warehouse.GSTin.Substring(0, 2);

                    var warehouse1 = context.Warehouses.FirstOrDefault(x => x.WarehouseId == Trdisptch.WarehouseId);
                    string wh2StateCode = warehouse1.GSTin.Substring(0, 2);
                    var statelist = await context.States.FirstOrDefaultAsync(x => x.Stateid == warehouse.Stateid);
                    InternalTransferEwaybillParam paramm = new InternalTransferEwaybillParam()
                    {
                        TransDocDt = DateTime.Now,
                        TransDocNo = "",
                        TransferOrderId = Trdisptch.TransferOrderId,
                        TransportGST = Trdisptch.TransporterGstin == null ? TransportGST : Trdisptch.TransporterGstin,
                        TransportName = Trdisptch.TransporterName == null ? TransportName : Trdisptch.TransporterName,
                        vehicleno = Trdisptch.VehicleNo.Trim().Replace(" ", "").ToUpper().ToString()
                    };
                    if (wh1StateCode != wh2StateCode) //interstate =>IRN will generate
                    {
                        result = await ProcessClearTaxAPI(item, context, Trdisptch, warehouse  /*from warehouse*/); //Auto Irn Generate Function

                        if (result && amount >= statelist.InterstateAmount)
                            result = await this.GenrateEwayBillByIRN(context, Trdisptch, warehouse, paramm, userid);
                        else
                            return false;
                    }
                    else if (amount >= statelist.IntrastateAmount)
                    {

                        result = await this.GenrateEwayBillNonIRN(item, context, Trdisptch, warehouse, paramm, userid);
                        if (!result)
                            return false;
                    }
                }
            }
            return result;
        }
        private async Task<bool> ProcessClearTaxAPI(ClearTaxIntegration item, AuthContext context, TransferWHOrderDispatchedMaster TransferorderDispachedMaster, Warehouse warehouse)
        {
            bool result = false;
            try
            {
                //TransferOrderId here
                //var TransferorderDispachedMaster = await context.TransferWHOrderDispatchedMasterDB.FirstOrDefaultAsync(x => x.TransferOrderId == item.OrderId);

                // WareHouse
                // var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == TransferorderDispachedMaster.WarehouseId);

                string qrCodeRelativePath = "~/images/eInvoiceQrCodes";
                string qrCodeSavePath = HostingEnvironment.MapPath(qrCodeRelativePath);
                ClearTaxResponseDc clearTaxResponse = new ClearTaxResponseDc();

                if (!Directory.Exists(qrCodeSavePath))
                    Directory.CreateDirectory(qrCodeSavePath);

                if (item.APIType == "GenerateCN" || item.APIType == "GenerateIRN")
                {
                    clearTaxResponse = await GenerateCnOrIrn(context, item, qrCodeRelativePath, qrCodeSavePath, TransferorderDispachedMaster, warehouse);
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
                    TransferWHOrderDispatchedMasters(TransferorderDispachedMaster, context, qrCodeSavePath, qrCodeRelativePath, clearTaxResponse);
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
        public async Task<ClearTaxResponseDc> GenerateCnOrIrn(AuthContext context, ClearTaxIntegration item, string qrCodeRelativePath, string qrCodeSavePath, TransferWHOrderDispatchedMaster TransferDispachedMaster, Warehouse warehouse)
        {
            try
            {
                RequestIRNDC requestIRNDC = null;
                List<ResponseIRNDC> responseIRNDCList = null;
                ResponseIRNDC responseIRNDC = null;
                string errorMessage = "", irnNo = "";
                string qrCode = "";

                var StateList = await context.States.ToListAsync();

                var citylist = await context.Cities.ToListAsync();

                //var customer = context.Customers.Where(x => x.CustomerId == orderDispachedMaster.CustomerId).FirstOrDefault();
                var customer = context.Warehouses.Where(x => x.WarehouseId == TransferDispachedMaster.WarehouseId).FirstOrDefault();


                if (string.IsNullOrEmpty(customer.GSTin))
                {
                    ClearTaxResponseDc errorResponse = new ClearTaxResponseDc
                    {
                        ApiType = item.APIType,
                        ErrorMessage = "Customer's Gst No is empty",
                    };
                    return errorResponse;
                }
                var items = context.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderId == item.OrderId).ToList();
                List<int> itemMultiMrpIds = items.Select(z => z.ItemMultiMRPId).Distinct().ToList();

                //List<string> itemIds = items.Select(z => new { z.ItemNumber ,z.WarehouseId }).Distinct().ToList();
                var itemIds = items.Select(z => z.ItemNumber).Distinct().ToList();
                var itemIds1 = items.Select(z => z.WarehouseId).Distinct().ToList();

                var mrpList = context.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();

                var itemMasterslist = await context.itemMasters.Where(x => itemIds.Contains(x.Number) && itemIds1.Contains(x.WarehouseId)).ToListAsync();
                //var itemMasterslist1 = await context.itemMasters.Where(x => itemIds1.Contains(x.WarehouseId)).ToListAsync();

                requestIRNDC = CreateIRNRequest(item, TransferDispachedMaster, customer, warehouse, items, TransferDispachedMaster, StateList, citylist, mrpList, itemMasterslist);

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
                            clearTaxReqResp.Json = JsonConvert.SerializeObject(responseIRNDC.govt_response);
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
                                errorMessage = JsonConvert.SerializeObject(responseIRNDC.govt_response.ErrorDetails.Select(x => x.error_message));
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
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private RequestIRNDC CreateIRNRequest(ClearTaxIntegration clearTaxIntegration, TransferWHOrderDispatchedMaster orderDispachedMaster, Warehouse customer, Warehouse warehouse, List<TransferWHOrderDispatchedDetail> items, TransferWHOrderDispatchedMaster TransferDispachedMaster, List<State> StateList, List<City> citylist, List<ItemMultiMRP> multiMRPs, List<ItemMaster> itemMasters)
        {
            RequestIRNDC requestIRNDC;
            RefDtls refDtls = null;

            bool Igst = false;

            string whStateCode = warehouse.GSTin.Substring(0, 2);
            string custStateCode = customer.GSTin.Substring(0, 2);

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
                //InternalTransferNo ,DeliveryChallanNo
                Typ = clearTaxIntegration.APIType == "GenerateIRN" ? "INV" : clearTaxIntegration.APIType == "GenerateCN" ? "CRN" : "",
                No = orderDispachedMaster.DeliveryChallanNo == null
                        ? orderDispachedMaster.InternalTransferNo : orderDispachedMaster.DeliveryChallanNo,
                Dt = clearTaxIntegration.APIType == "GenerateIRN"
                        ? orderDispachedMaster.CreationDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : ""
            };
            IRN.SellerDtls sellerDtls = new IRN.SellerDtls()
            {
                //excahngigng buyer ,seller details 
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

            IRN.BuyerDtls buyerDtls = new IRN.BuyerDtls()
            {
                Gstin = customer.GSTin,
                LglNm = customer.CompanyName,
                TrdNm = customer.CompanyName,
                Pos = custStateCode, //change as pr deepak k sir whStateCode,//"stateIdofWarehouse",
                Addr1 = customer.Address,//iRNHelper.StringTrim(customer.Address, 100),
                Addr2 = null,
                Loc = customer.CityName, //customer.BillingCity,
                Pin = Convert.ToInt32(customer.PinCode),
                Stcd = custStateCode,
                Ph = null,
                Em = null
            };
            IRN.DispDtls dispDtls = new IRN.DispDtls()
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
                Gstin = customer.GSTin,
                LglNm = customer.CompanyName,
                TrdNm = customer.CompanyName,
                Addr1 = customer.Address,//StringTrim(customer.ShippingAddress, 100),
                Addr2 = null,
                Loc = customer.CityName, //customer.ShippingCity,
                Pin = Convert.ToInt32(customer.PinCode),
                Stcd = StateList.FirstOrDefault(x => x.Stateid == customer.Stateid)?.ClearTaxStateCode
            };

            List<IRN.ItemList> itemLists = new List<IRN.ItemList>();

            double CGSTTaxAmmount = 0;
            double SGSTTaxAmmount = 0;
            int cnt = 1;
            foreach (var item in items)
            {
                var multiMrp = multiMRPs.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                double igstamt = 0;

                double SGSTTaxPercentage = 0;
                double CGSTTaxPercentage = 0;
                // var withoutTaxUnitPrice = 0;

                double TotalCessPercentage = itemMasters.FirstOrDefault(x => x.Number == item.ItemNumber).TotalCessPercentage;

                double TaxPercentage = itemMasters.FirstOrDefault(x => x.Number == item.ItemNumber).TotalTaxPercentage;

                //double TaxPercentage = itemMasters.Any(p => p.ItemNumber == item.ItemNumber) ? itemMasters.FirstOrDefault(p => p.ItemNumber == item.ItemNumber && p.TotalTaxPercentage > 0).TotalTaxPercentage : 0;
                //double TotalCessPercentage = itemMasters.Any(p => p.ItemNumber == item.ItemNumber) ? itemMasters.FirstOrDefault(p => p.ItemNumber == item.ItemNumber && p.TotalCessPercentage > 0).TotalCessPercentage : 0;// items.TotalCessPercentage;

                if (TaxPercentage >= 0)
                {
                    SGSTTaxPercentage = TaxPercentage / 2;
                    CGSTTaxPercentage = TaxPercentage / 2;
                }


                double AmtWithoutTaxDisc = 0;
                double AmtWithoutAfterTaxDisc = 0;
                double DiscountPercentage = 0;
                double CessTaxAmount = 0;
                double CGSTTaxAmmount1 = 0;
                double SGSTTaxAmmount1 = 0;

                if (TotalCessPercentage > 0)
                {
                    double tempPercentagge = TotalCessPercentage + TaxPercentage;
                    AmtWithoutTaxDisc = ((100 * item.NPP.Value * item.TotalQuantity) / (1 + tempPercentagge / 100)) / 100;
                    AmtWithoutAfterTaxDisc = (100 * AmtWithoutTaxDisc) / (100 + 0);
                    CessTaxAmount = (AmtWithoutAfterTaxDisc * TotalCessPercentage) / 100;
                }
                double tempPercentagge2 = TotalCessPercentage + TaxPercentage;
                AmtWithoutTaxDisc = ((100 * item.NPP.Value * item.TotalQuantity) / (1 + tempPercentagge2 / 100)) / 100;
                AmtWithoutAfterTaxDisc = (100 * AmtWithoutTaxDisc) / (100 + DiscountPercentage);
                double TaxAmmount = (AmtWithoutAfterTaxDisc * TaxPercentage) / 100;
                if (TaxAmmount >= 0)
                {
                    SGSTTaxAmmount = TaxAmmount / 2;
                    CGSTTaxAmmount = TaxAmmount / 2;
                }
                if (Igst)
                {
                    igstamt = SGSTTaxAmmount + CGSTTaxAmmount;
                }
                else
                {
                    CGSTTaxAmmount1 = SGSTTaxAmmount;
                    SGSTTaxAmmount1 = CGSTTaxAmmount;
                }
                IRN.ItemList itemList = new IRN.ItemList()
                {
                    SlNo = Convert.ToString(cnt),
                    PrdDesc = item.itemname,
                    IsServc = "N",
                    HsnCd = itemMasters.FirstOrDefault(x => x.Number == item.ItemNumber)?.HSNCode,
                    Barcde = null,
                    Qty = item.TotalQuantity,
                    FreeQty = 0,
                    Unit = "PCS",
                    UnitPrice = item.NPP.Value > 0 ? item.NPP.Value : 0,
                    TotAmt = AmtWithoutTaxDisc,//item.AmtWithoutTaxDisc,
                    Discount = 0,
                    PreTaxVal = 0,
                    AssAmt = AmtWithoutTaxDisc,//item.AmtWithoutTaxDisc,
                    GstRt = CGSTTaxPercentage + SGSTTaxPercentage,
                    IgstAmt = igstamt,
                    SgstAmt = SGSTTaxAmmount1,
                    CgstAmt = CGSTTaxAmmount1,
                    CesRt = TotalCessPercentage > 0 ? TotalCessPercentage : 0,//item.TotalCessPercentage,
                    CesAmt = CessTaxAmount,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal = item.NPP.Value > 0 ? Math.Round((item.TotalQuantity * item.NPP.Value), 0) : 0,
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
            double CGSTTaxAmmount2 = 0;
            double SGSTTaxAmmount2 = 0;
            if (Igst)
            {
                igstamt1 = CGSTTaxAmmount + SGSTTaxAmmount;//orderDispatchedMaster.CGSTTaxAmmount + orderDispatchedMaster.SGSTTaxAmmount;
            }
            else
            {
                CGSTTaxAmmount2 = SGSTTaxAmmount;
                SGSTTaxAmmount2 = CGSTTaxAmmount;
            }
            ValDtls valDtls = new ValDtls()
            {
                AssVal = itemLists.Sum(x => x.AssAmt),
                CgstVal = CGSTTaxAmmount2,
                SgstVal = SGSTTaxAmmount2,
                CesVal = itemLists.Sum(x => x.CesAmt),
                IgstVal = itemLists.Sum(x => x.IgstAmt),
                StCesVal = 0,
                Discount = 0,  //(orderDispatchedMaster.BillDiscountAmount ?? 0) + (orderDispachedMaster.WalletAmount ?? 0),
                OthChrg = itemLists.Sum(x => x.OthChrg),//orderDispatchedMaster.deliveryCharge + orderDispachedMaster.TCSAmount,
                RndOffAmt = 0,
                TotInvVal = itemLists.Sum(x => x.TotItemVal),//orderDispatchedMaster.GrossAmount,
                TotInvValFc = 0
            };

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

        bool TransferWHOrderDispatchedMasters(TransferWHOrderDispatchedMaster TrorderDispachedMaster, AuthContext context, string qrCodeSavePath, string qrCodeRelativePath, ClearTaxResponseDc clearTaxResponse)
        {
            var StateList = context.States.ToList();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(clearTaxResponse.QrCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCodee = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCodee.GetGraphic(20);
            string imagePath = qrCodeSavePath + @"\" + clearTaxResponse.IrnNo + ".jpg";
            qrCodeImage.Save(imagePath);

            if (clearTaxResponse.ApiType == "GenerateIRN" || clearTaxResponse.ApiType == "GetIRN")
            {
                TrorderDispachedMaster.IRNNo = clearTaxResponse.IrnNo;
                TrorderDispachedMaster.IRNQRCode = clearTaxResponse.QrCode;
                TrorderDispachedMaster.IRNQRCodeUrl = qrCodeRelativePath.Replace("~", "") + "/" + clearTaxResponse.IrnNo + ".jpg";
                TrorderDispachedMaster.IsGenerateIRN = true;
                var statecode = context.Warehouses.FirstOrDefault(x => x.WarehouseId == TrorderDispachedMaster.WarehouseId);
                var AmtToCheck = StateList.Where(s => s.Stateid == statecode.Stateid).Select(x => new { x.IntrastateAmount, x.InterstateAmount }).FirstOrDefault();
            }
            context.Entry(TrorderDispachedMaster).State = System.Data.Entity.EntityState.Modified;
            return true;
        }


        #endregion




        #region Generate Ewaybill By IRN
        public async Task<bool> GenrateEwayBillByIRN(AuthContext authContext, TransferWHOrderDispatchedMaster transferdisMaster, Warehouse ware, InternalTransferEwaybillParam param, int userid)
        {
            string errorMessage = null;
            bool status = false;
            EwayBillHelper ewayBillHelper = new EwayBillHelper();
            var CompanyDetails = ewayBillHelper.GetcompanyDetails(authContext);
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;

                List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;

                List<EwayResponceDcAll> responseEwayBillbyIRNDCAll = new List<EwayResponceDcAll>();
                List<EwayResponceDc> responseEwayBillIRNDCList = null;
                EwayResponceDc responseEwayBillbyIRNDC = null;

                ewaybillirnParam ewayBillPostDc = new ewaybillirnParam();
                EwayResponceDcAll ewayResponceDc = new EwayResponceDcAll();
                ewayBillPostDc.Irn = transferdisMaster.IRNNo;
                ewayBillPostDc.Distance = 0;//item.WarehousePincode.Equals(item.ZipCode) == true ? 100 : 0;
                ewayBillPostDc.TransMode = "1";
                ewayBillPostDc.TransId = param.TransportGST;
                ewayBillPostDc.TransName = param.TransportName;
                ewayBillPostDc.TransDocDt = "";
                ewayBillPostDc.TransDocNo = "";
                ewayBillPostDc.VehNo = param.vehicleno;
                ewayBillPostDc.VehType = "R";
                ewayBillPostDc.DispDtls = null;
                var RequestjsonString = JsonConvert.SerializeObject(new List<ewaybillirnParam> { ewayBillPostDc });

                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration()
                {
                    OrderId = transferdisMaster.TransferOrderId,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    IsProcessed = true,
                    APIType = "GenerateEWBInternal",
                    APITypes = 2,//GenerateEWBInternal
                };
                ewayResponceDc.clearTaxIntegration = (clearTaxIntegration);
                ClearTaxReqResp ctrp = new ClearTaxReqResp()
                {
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    OrderId = transferdisMaster.TransferOrderId,
                    Json = RequestjsonString,
                    Url = BaseUrl
                };
                ewayResponceDc.OrderId = transferdisMaster.TransferOrderId;
                ewayResponceDc.ClearTaxRequest = ctrp;
                extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                                {
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "x-cleartax-auth-token",
                                  new List<string> { eInvoiceAuthKey }
                                ),
                                new KeyValuePair<string, IEnumerable<string>>
                                (
                                  "gstin",
                                  new List<string> { ware.GSTin }
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
                                OrderId = transferdisMaster.TransferOrderId,
                                Json = JsonConvert.SerializeObject(badRequestResponse),
                                Url = BaseUrl
                            };
                            errorMessage = badRequestResponse;
                            ewayResponceDc.ClearTaxResponce = clearTaxReqResps;
                        }
                        else
                        {
                            responseEwayBillbyIRNDC = responseEwayBillIRNDCList[0];
                            if (responseEwayBillbyIRNDC != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                            {

                                ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                {
                                    CreateDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Response",
                                    OrderId = transferdisMaster.TransferOrderId,
                                    Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC),
                                    Url = BaseUrl
                                };

                                authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                                errorMessage = badRequestResponse;
                                ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                ewayResponceDc.ewayResponceDc = (responseEwayBillbyIRNDC);

                                OrderEwayBill bill = new OrderEwayBill
                                {
                                    OrderId = transferdisMaster.TransferOrderId,
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
                                authContext.OrderEwayBills.Add(bill);
                                var ewayupdate = authContext.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == transferdisMaster.TransferOrderId).FirstOrDefault(); //order id
                                if (ewayupdate != null)
                                {
                                    ewayupdate.EwaybillNumber = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString();
                                }
                                authContext.Entry(ewayupdate).State = EntityState.Modified;
                                // context.Commit();
                                status = true;
                            }
                            else
                            {
                                ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                {
                                    CreateDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Response",
                                    OrderId = transferdisMaster.TransferOrderId,
                                    Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                                    Url = BaseUrl
                                };
                                ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                                status = false;
                            }
                            responseEwayBillbyIRNDCAll.Add(ewayResponceDc);

                            authContext.Commit();
                        }
                    }
                }
                catch (Exception exe)
                {
                    errorMessage = exe.ToString();
                }
            }
            return status;
        }
        #endregion


        #region Generate Ewaybill NON IRN
        public async Task<bool> GenrateEwayBillNonIRN(ClearTaxIntegration item, AuthContext authContext, TransferWHOrderDispatchedMaster transferdisMaster, Warehouse ware, InternalTransferEwaybillParam param, int userid)
        {
            bool res = false;
            //string errorMessage = null;
            EwayBillHelper ewayBillHelper = new EwayBillHelper();
            var CompanyDetails = ewayBillHelper.GetcompanyDetails(authContext);
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;

                List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null;

                //List<EwayResponceDcAll> responseEwayBillbyIRNDCAll = new List<EwayResponceDcAll>();

                InternalNonIRNResponse responseEwayBillbyIRNDC = null;

                List<InternalNonIRNRequestDc> responseEwayBillIRNDCList = null;
                //EwayResponceDcAll ewayResponceDc = new EwayResponceDcAll();

                //Creating Request Data

                InternalNonIRNRequestDc requestdata = await CreateNonIRNRequestData(authContext, transferdisMaster, ware, param);

                var RequestjsonString = JsonConvert.SerializeObject(requestdata);


                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration()
                {
                    OrderId = transferdisMaster.TransferOrderId,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    IsProcessed = true,
                    APIType = "GenerateEWBInternal",
                    APITypes = 2,
                    //ProcessDate=DateTime.Now
                };
                authContext.ClearTaxIntegrations.Add(clearTaxIntegration);
                ClearTaxReqResp ctrp = new ClearTaxReqResp()
                {
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    OrderId = transferdisMaster.TransferOrderId,
                    Json = RequestjsonString,
                    Url = BaseUrl
                };
                authContext.ClearTaxReqResps.Add(ctrp);


                //ewayResponceDc.OrderId = transferdisMaster.TransferOrderId;
                //ewayResponceDc.ClearTaxRequest = ctrp;
                //extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>
                //                {
                //                new KeyValuePair<string, IEnumerable<string>>
                //                (
                //                  "x-cleartax-auth-token",
                //                  new List<string> { eInvoiceAuthKey }
                //                ),
                //                new KeyValuePair<string, IEnumerable<string>>
                //                (
                //                  "gstin",
                //                  new List<string> { ware.GSTin }
                //                ),
                //                };
                //try
                //{
                //using (var client = new GenericRestHttpClient<List<InternalNonIRNRequestDc>, string>(BaseUrl, "v3/ewaybill/generate", extraDataAsHeader))
                //{
                //    string badRequestResponse = "";
                //    List<InternalNonIRNResponse> response = client.PutAsyncWithHandleError<List<InternalNonIRNResponse>>(new List<InternalNonIRNRequestDc> { requestdata }, out badRequestResponse);
                //    //var clearTaxIntegrations = authContext.ClearTaxIntegrations.FirstOrDefault(x => x.OrderId == item.OrderId && x.APITypes == 1 && x.APIType == "GenerateEWB" && x.IsActive == true);

                //    if (!string.IsNullOrEmpty(badRequestResponse))
                //    {
                //        ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                //        {
                //            CreateDate = DateTime.Now,
                //            IsActive = true,
                //            Type = "Response",
                //            OrderId = transferdisMaster.TransferOrderId,
                //            Json = JsonConvert.SerializeObject(badRequestResponse),
                //            Url = BaseUrl
                //        };
                //        errorMessage = badRequestResponse;
                //        ewayResponceDc.ClearTaxResponce = clearTaxReqResps;
                //    }
                //    else
                //    {
                //        responseEwayBillbyIRNDC = response[0];
                //        if (responseEwayBillbyIRNDC != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                //        {

                //            ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                //            {
                //                CreateDate = DateTime.Now,
                //                IsActive = true,
                //                Type = "Response",
                //                OrderId = transferdisMaster.TransferOrderId,
                //                Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC),
                //                Url = BaseUrl
                //            };

                //            authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                //            errorMessage = badRequestResponse;
                //            ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                //            // ewayResponceDc.ewayResponceDc = (responseEwayBillbyIRNDC);

                //            OrderEwayBill bill = new OrderEwayBill
                //            {
                //                OrderId = transferdisMaster.TransferOrderId,
                //                EwayBillNo = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString(),
                //                EwayBillDate = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbDt, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                //                EwayBillValidTill = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbValidTill, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                //                IsCancelEwayBill = false,
                //                EwayBillCancelDate = null,
                //                IsActive = true,
                //                CreateDate = Convert.ToDateTime(responseEwayBillbyIRNDC.govt_response.EwbDt, CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat),
                //                CreatedBy = userid,
                //                ModifiedBy = null,
                //                ModifiedDate = null,
                //                CustomerType = 1,//B2C
                //                VehicleNumber = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString()
                //            };
                //            authContext.OrderEwayBills.Add(bill);
                //            var ewayupdate = authContext.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == transferdisMaster.TransferOrderId).FirstOrDefault(); //order id
                //            if (ewayupdate != null)
                //            {
                //                ewayupdate.EwaybillNumber = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString();
                //            }
                //            authContext.Entry(ewayupdate).State = EntityState.Modified;
                //            // context.Commit();
                //        }
                //        else
                //        {
                //            ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                //            {
                //                CreateDate = DateTime.Now,
                //                IsActive = true,
                //                Type = "Response",
                //                OrderId = transferdisMaster.TransferOrderId,
                //                Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                //                Url = BaseUrl
                //            };
                //            ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                //            authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                //        }
                //        //responseEwayBillbyIRNDCAll.Add(ewayResponceDc);

                //        authContext.Commit();
                //    }
                //}
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BaseUrl + "/v3/ewaybill/generate"))
                        {
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("NoEncryption", "1");
                            request.Headers.TryAddWithoutValidation("X-Cleartax-Auth-Token", eInvoiceAuthKey);
                            request.Headers.TryAddWithoutValidation("gstin", ware.GSTin);
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Content = new StringContent(RequestjsonString);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var Response = await httpClient.SendAsync(request);
                            if (Response.StatusCode == HttpStatusCode.OK)
                            {
                                string responseBody = Response.Content.ReadAsStringAsync().Result;
                                responseEwayBillbyIRNDC = JsonConvert.DeserializeObject<InternalNonIRNResponse>(responseBody);
                                if (Response.Content != null && responseEwayBillbyIRNDC.govt_response.Success == "Y")
                                {
                                    ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                    {
                                        CreateDate = DateTime.Now,
                                        IsActive = true,
                                        Type = "Response",
                                        OrderId = transferdisMaster.TransferOrderId,
                                        Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC),
                                        Url = BaseUrl
                                    };
                                    authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                                    //ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);


                                    OrderEwayBill bill = new OrderEwayBill
                                    {
                                        OrderId = transferdisMaster.TransferOrderId,
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
                                        CustomerType = 0,//internal
                                        VehicleNumber = param.vehicleno.Trim().Replace(" ", "").ToUpper().ToString()
                                    };
                                    authContext.OrderEwayBills.Add(bill);
                                    var ewayupdate = authContext.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == transferdisMaster.TransferOrderId).FirstOrDefault(); //order id
                                    if (ewayupdate != null)
                                    {
                                        ewayupdate.EwaybillNumber = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString();
                                    }
                                    authContext.Entry(ewayupdate).State = EntityState.Modified;
                                    //authContext.Commit();
                                    res = true;
                                }
                                else
                                {
                                    ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                    {
                                        CreateDate = DateTime.Now,
                                        IsActive = true,
                                        Type = "Response",
                                        OrderId = transferdisMaster.TransferOrderId,
                                        Json = JsonConvert.SerializeObject(responseEwayBillbyIRNDC.govt_response.ErrorDetails.Select(x => x.error_message)),
                                        Url = BaseUrl
                                    };
                                    //ewayResponceDc.ClearTaxResponce = (clearTaxReqResps);
                                    authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                                    // authContext.Commit();
                                }

                            }
                            else
                            {
                                var rr = Response.Content.ReadAsStringAsync().Result;
                                ClearTaxReqResp clearTaxReqResps = new ClearTaxReqResp()
                                {
                                    CreateDate = DateTime.Now,
                                    IsActive = true,
                                    Type = "Response",
                                    OrderId = transferdisMaster.TransferOrderId,
                                    Json = rr,
                                    Url = BaseUrl
                                };
                                authContext.ClearTaxReqResps.Add(clearTaxReqResps);
                                //ewayResponceDc.ClearTaxResponce = clearTaxReqResps;
                            }
                        }
                    }

                    //responseEwayBillbyIRNDCAll.Add(ewayResponceDc);
                    item.IsProcessed = true;
                    item.IRNNo = null;
                    item.IsProcessed = true;
                    item.ProcessDate = DateTime.Now;
                    item.EwayBillNo = responseEwayBillbyIRNDC.govt_response.EwbNo.ToString();
                    authContext.Entry(item).State = EntityState.Modified;
                    authContext.Commit();
                }

                catch (Exception exe)
                {
                    throw exe;
                }
            }
            return res;
        }
        #endregion


        #region  getNonIRNRequestData

        public async Task<InternalNonIRNRequestDc> CreateNonIRNRequestData(AuthContext authContext, TransferWHOrderDispatchedMaster transferdisMaster, Warehouse ware, InternalTransferEwaybillParam param)
        {
            //InternalNonIRNRequestDc requestData = null;

            var BuyerWarehouse = authContext.Warehouses.Where(x => x.WarehouseId == transferdisMaster.WarehouseId).FirstOrDefault();
            bool Igst = false;

            DataContracts.EwayBill.InternalTransferEwaybillDc.SellerDtls sellerDtls = new DataContracts.EwayBill.InternalTransferEwaybillDc.SellerDtls()
            {
                Gstin = ware.GSTin,
                LglNm = ware.WarehouseName,
                TrdNm = ware.CompanyName,
                Addr1 = ware.Address,
                Addr2 = null,
                Loc = ware.CityName,
                Pin = ware.PinCode,
                Stcd = ware.GSTin.Substring(0, 2)
            };

            DataContracts.EwayBill.InternalTransferEwaybillDc.BuyerDtls buyerDtls = new DataContracts.EwayBill.InternalTransferEwaybillDc.BuyerDtls()
            {
                Gstin = BuyerWarehouse.GSTin,
                LglNm = BuyerWarehouse.WarehouseName,
                TrdNm = BuyerWarehouse.CompanyName,
                Addr1 = BuyerWarehouse.Address,
                Addr2 = null,
                Loc = BuyerWarehouse.CityName,
                Pin = BuyerWarehouse.PinCode,
                Stcd = BuyerWarehouse.GSTin.Substring(0, 2),
            };

            var items = authContext.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderId == transferdisMaster.TransferOrderId).ToList();
            List<int> itemMultiMrpIds = items.Select(z => z.ItemMultiMRPId).Distinct().ToList();

            //List<string> itemIds = items.Select(z => new { z.ItemNumber ,z.WarehouseId }).Distinct().ToList();
            var itemIds = items.Select(z => z.ItemNumber).Distinct().ToList();
            var itemIds1 = items.Select(z => z.WarehouseId).Distinct().ToList();
            var mrpList = authContext.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();
            var itemMasterslist = await authContext.itemMasters.Where(x => itemIds.Contains(x.Number) && itemIds1.Contains(x.WarehouseId)).ToListAsync();

            List<DataContracts.EwayBill.InternalTransferEwaybillDc.ItemList> itemLists = new List<DataContracts.EwayBill.InternalTransferEwaybillDc.ItemList>();

            double CGSTTaxAmmount = 0;
            double SGSTTaxAmmount = 0;
            int cnt = 1;

            foreach (var item in items)
            {
                double igstamt = 0;
                double SGSTTaxPercentage = 0;
                double CGSTTaxPercentage = 0;

                double TotalCessPercentage = itemMasterslist.FirstOrDefault(x => x.Number == item.ItemNumber).TotalCessPercentage;

                double TaxPercentage = itemMasterslist.FirstOrDefault(x => x.Number == item.ItemNumber).TotalTaxPercentage;

                if (TaxPercentage >= 0)
                {
                    SGSTTaxPercentage = TaxPercentage / 2;
                    CGSTTaxPercentage = TaxPercentage / 2;
                }

                double AmtWithoutTaxDisc = 0;
                double AmtWithoutAfterTaxDisc = 0;
                double DiscountPercentage = 0;
                double CessTaxAmount = 0;
                if (TotalCessPercentage > 0)
                {
                    double tempPercentagge = TotalCessPercentage + TaxPercentage;
                    AmtWithoutTaxDisc = ((100 * item.NPP.Value * item.TotalQuantity) / (1 + tempPercentagge / 100)) / 100;
                    AmtWithoutAfterTaxDisc = (100 * AmtWithoutTaxDisc) / (100 + 0);
                    CessTaxAmount = (AmtWithoutAfterTaxDisc * TotalCessPercentage) / 100;
                }
                double tempPercentagge2 = TotalCessPercentage + TaxPercentage;
                AmtWithoutTaxDisc = ((100 * item.NPP.Value * item.TotalQuantity) / (1 + tempPercentagge2 / 100)) / 100;
                AmtWithoutAfterTaxDisc = (100 * AmtWithoutTaxDisc) / (100 + DiscountPercentage);
                double TaxAmmount = (AmtWithoutAfterTaxDisc * TaxPercentage) / 100;
                if (TaxAmmount >= 0)
                {
                    SGSTTaxAmmount = TaxAmmount / 2;
                    CGSTTaxAmmount = TaxAmmount / 2;
                }
                if (Igst)
                {
                    igstamt = SGSTTaxAmmount + CGSTTaxAmmount;
                }

                DataContracts.EwayBill.InternalTransferEwaybillDc.ItemList itemList = new DataContracts.EwayBill.InternalTransferEwaybillDc.ItemList()
                {

                    ProdDesc = item.itemname,
                    HsnCd = itemMasterslist.FirstOrDefault(x => x.Number == item.ItemNumber)?.HSNCode,
                    Qty = item.TotalQuantity,
                    Unit = "PCS",
                    //unit = item.NPP.Value > 0 ? item.NPP.Value : 0,
                    //TotAmt = AmtWithoutTaxDisc,//item.AmtWithoutTaxDisc,
                    //Discount = 0,
                    //PreTaxVal = 0,
                    AssAmt = AmtWithoutTaxDisc,//item.AmtWithoutTaxDisc,
                                               //GstRt = CGSTTaxPercentage + SGSTTaxPercentage,
                    IgstAmt = igstamt,
                    SgstAmt = SGSTTaxAmmount,
                    CgstAmt = CGSTTaxAmmount,
                    CesRt = TotalCessPercentage > 0 ? TotalCessPercentage : 0,//item.TotalCessPercentage,
                    CesAmt = CessTaxAmount,
                    CesNonAdvAmt = 0,
                    //StateCesRt = 0,
                    //StateCesAmt = 0,
                    //StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    CgstRt = CGSTTaxPercentage,
                    IgstRt = 0,
                    ProdName = item.itemname,
                    SgstRt = SGSTTaxPercentage
                    //TotItemVal = item.NPP.Value > 0 ? Math.Round((item.TotalQuantity * item.NPP.Value), 0) : 0,
                };
                itemLists.Add(itemList);
                cnt++;
            }

            double igstamt1 = 0;

            if (Igst)
            {
                igstamt1 = CGSTTaxAmmount + SGSTTaxAmmount;//orderDispatchedMaster.CGSTTaxAmmount + orderDispatchedMaster.SGSTTaxAmmount;
            }
            var pincode1 = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == transferdisMaster.WarehouseId).PinCode;
            var pincode2 = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == transferdisMaster.RequestToWarehouseId).PinCode;
            //double distance = 0;
            //int distanceConvert = 0;
            //if (param.distance == 0)
            //{
            //    try
            //    {
            //        distance = Convert.ToDouble(Common.Helpers.GeoHelper.GetDistanceInKM(pincode1, Convert.ToInt32(pincode2)));
            //        distanceConvert = Convert.ToInt32(Math.Round(distance, 0));
            //    }
            //    catch (Exception ex)
            //    {
            //        throw ex;
            //    }
            //}
            InternalNonIRNRequestDc ewayBillPostDc = new InternalNonIRNRequestDc()
            {
                Distance = param.distance == 0 ? 0 : param.distance,
                //pincode1.Equals(pincode2) == true ? 100 : 0,//itemMasterslist.WarehousePincode.Equals(itemMasterslist.ZipCode) == true ? 100 : 0,
                TransMode = "ROAD",
                TransId = param.TransportGST,
                TransName = param.TransportName,
                TransDocDt = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                TransDocNo = param.TransDocNo,
                VehNo = param.vehicleno,
                VehType = "REGULAR",
                //DispDtls = null,
                //ExpShipDtls = null,
                BuyerDtls = buyerDtls,
                SellerDtls = sellerDtls,
                DocumentDate = transferdisMaster.DCCreatedDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                DocumentNumber = transferdisMaster.DeliveryChallanNo == "" || transferdisMaster.DeliveryChallanNo == null ? transferdisMaster.InternalTransferNo : transferdisMaster.DeliveryChallanNo,
                DocumentType = ware.GSTin.Substring(0, 2) == BuyerWarehouse.GSTin.Substring(0, 2) ? "CHL" : "INV",
                ItemList = itemLists,
                OtherAmount = 0,
                OtherTcsAmount = 0,
                SubSupplyType = ware.GSTin.Substring(0, 2) == BuyerWarehouse.GSTin.Substring(0, 2) ? "OWN_USE" : "SUPPLY",
                SubSupplyTypeDesc = "",
                SupplyType = "OUTWARD",//"OUTWARD",
                TotalAssessableAmount = itemLists.Sum(x => x.AssAmt),
                TotalCessAmount = 0,
                TotalCessNonAdvolAmount = 0,
                TotalCgstAmount = itemLists.Sum(x => x.CgstAmt),
                TotalIgstAmount = itemLists.Sum(x => x.IgstAmt),
                TotalInvoiceAmount = (double)items.Sum(x => x.NPP * x.TotalQuantity),//item.NPP.Value > 0 ? Math.Round((item.TotalQuantity * item.NPP.Value), 0) : 0,,
                TotalSgstAmount = itemLists.Sum(x => x.SgstAmt),
                TransactionType = "Regular"
            };
            return ewayBillPostDc;
        }

        #endregion


        //updateVEhicleInternal
        #region Update Internal Vehicle
        public async Task<EwaybillBackendResponceDc> UpdateInternalVehiclePartBIRN(UpdatePartBInternalRequest updatePartBParam, AuthContext context, int userid, Warehouse warehouse, CompanyDetails CompanyDetails)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;
                var client = new RestClient(BaseUrl + "/v1/ewaybill/update?action=PARTB");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("X-Cleartax-Auth-Token", eInvoiceAuthKey);
                request.AddHeader("gstin", warehouse.GSTin);
                request.AddHeader("Content-Type", "application/json");
                var newJson = JsonConvert.SerializeObject(updatePartBParam);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "InternalUpdateVehicleApi",
                    Orderid = updatePartBParam.TransferOrderId,
                    ErrorDescription = newJson,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);

                request.AddParameter("application/json", newJson, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);
                UpdatePartBResponse RESS = JsonConvert.DeserializeObject<UpdatePartBResponse>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK && RESS.errors != null)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "InternalUpdateVehicleApi" && x.Orderid == updatePartBParam.TransferOrderId && x.IsActive == true && x.IsDeleted == false);
                        if (orderEwayBillsGenErrors == null)
                        {
                            OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                            ewayerror.ErrorCode = 0;
                            ewayerror.Orderid = updatePartBParam.TransferOrderId;
                            ewayerror.IsActive = true;
                            ewayerror.IsDeleted = false;
                            ewayerror.Remarks = "InternalUpdateVehicleApi";
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
                        Orderid = updatePartBParam.TransferOrderId,
                        IsActive = true,
                        IsDeleted = false,
                        Remarks = "InternalUpdateVehicleApi",
                        ModifiedBy = null,
                        ModifiedDate = null,
                        ErrorDescription = response.Content,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Type = "Response"
                    };
                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == updatePartBParam.TransferOrderId).OrderByDescending(x => x.EwayBillId).FirstOrDefault();
                    if (ewayupdate != null)
                    {
                        ewayupdate.VehicleNumber = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.EwbNumber);
                        ewayupdate.ModifiedDate = Convert.ToDateTime(RESS.UpdatedDate, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);
                        ewayupdate.EwayBillValidTill = Convert.ToDateTime(RESS.ValidUpto, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);
                        ewayupdate.ModifiedBy = userid;
                        ewayupdate.ModifiedDate = DateTime.Now;
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "Vehicle Updated Successfully";
                    res.status = true;
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
        #endregion


        #region


        public async Task<EwaybillBackendResponceDc> ExtendEwayBillInternalTransfer(InternalTransferExtendRequest extendRequestParam, AuthContext context, int userid)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            EwayBillHelper ewayBillHelper = new EwayBillHelper();

            var TransferOrderMaster = await context.TransferWHOrderDispatchedMasterDB.FirstOrDefaultAsync(x => x.TransferOrderId == extendRequestParam.TransferOrderid);
            Warehouse ware = await context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == TransferOrderMaster.WarehouseId);
            var CompanyDetails = ewayBillHelper.GetcompanyDetails(context);
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;
                var client = new RestClient(BaseUrl + "/v1/ewaybill/update?action=EXTEND_VALIDITY");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("X-Cleartax-Auth-Token", eInvoiceAuthKey);
                request.AddHeader("gstin", ware.GSTin);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");

                //InternalTransferExtendRequest exte = new InternalTransferExtendRequest();
                InternalTransferExtendRequest extendInternal = new InternalTransferExtendRequest() // insert dc
                {
                    ConsignmentStatus = "MOVEMENT",
                    DocumentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    DocumentNumber = extendRequestParam.TransDocNo,//doubt
                    DocumentType = "INV",
                    EwbNumber = extendRequestParam.EwbNumber,
                    FromPincode = ware.PinCode.ToString(),
                    FromPlace = ware.CityName,
                    FromState = ware.GSTin.Substring(0, 2),
                    ReasonCode = extendRequestParam.ReasonCode.ToString(),
                    ReasonRemark = extendRequestParam.ReasonRemark,
                    RemainingDistance = extendRequestParam.RemainingDistance.ToString(),
                    TransDocDt = extendRequestParam.TransDocDt,
                    TransDocNo = extendRequestParam.TransDocNo,
                    TransMode = "ROAD",
                    VehicleType = "REGULAR",
                    VehNo = extendRequestParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString(),
                };

                var newJson = JsonConvert.SerializeObject(extendInternal);
                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "InternalTransferExtendApi",
                    Orderid = extendRequestParam.TransferOrderid,
                    ErrorDescription = newJson,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);

                request.AddParameter("application/json", newJson, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);
                ExtendResponseDc RESS = JsonConvert.DeserializeObject<ExtendResponseDc>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK && RESS.errors != null)
                {
                    var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "InternalTransferExtendApi" && x.Orderid == extendRequestParam.TransferOrderid && x.IsActive == true && x.IsDeleted == false);
                    if (orderEwayBillsGenErrors == null)
                    {
                        OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                        ewayerror.ErrorCode = 0;
                        ewayerror.Orderid = extendRequestParam.TransferOrderid;
                        ewayerror.IsActive = true;
                        ewayerror.IsDeleted = false;
                        ewayerror.Remarks = "InternalTransferExtendApi";
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
                    res.Message = RESS.errors.ToString();     //"EwayBill  Not Extended.....try again";
                    res.status = false;
                    return res;
                }
                else if (response.StatusCode == HttpStatusCode.OK && RESS.errors == null)
                {

                    OrderEwayBillsGenError OrderEwayReqResp = new OrderEwayBillsGenError()
                    {
                        ErrorCode = 0,
                        Orderid = extendRequestParam.TransferOrderid,
                        IsActive = true,
                        IsDeleted = false,
                        Remarks = "InternalTransferExtendApi",
                        ModifiedBy = null,
                        ModifiedDate = null,
                        ErrorDescription = response.Content,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        Type = "Response"
                    };
                    context.OrderEwayBillsGenErrors.Add(OrderEwayReqResp);
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == extendRequestParam.TransferOrderid && !string.IsNullOrEmpty(x.EwayBillNo)).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
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
                    res.Message = "Internal Transfer EwayBillExtended";
                    res.status = true;
                    return res;
                }
                //Console.WriteLine(response.Content);
            }
            else
            {
                res.Message = "Eway Bill Service Not Started Yet!!";
                res.status = false;
                return res;
            }
            return res;
        }
        #endregion


        public async Task<EwaybillBackendResponceDc> CancelEwayBillInternalbyIRN(InternalCancelEwaybillParam cancelewaybillirn, AuthContext context, int userid)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();

            EwayBillHelper ewayBillHelper = new EwayBillHelper();

            var TransferOrderMaster = await context.TransferWHOrderDispatchedMasterDB.FirstOrDefaultAsync(x => x.TransferOrderId == cancelewaybillirn.TransferOrderid);
            Warehouse ware = await context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == TransferOrderMaster.WarehouseId);
            var CompanyDetails = ewayBillHelper.GetcompanyDetails(context);
            if (CompanyDetails != null)
            {
                BaseUrl = CompanyDetails.eInvoiceBaseUrl;
                eInvoiceAuthKey = CompanyDetails.eInvoiceAuthKey;


                InternalCancelEwaybillParam reqParam = new InternalCancelEwaybillParam()
                {
                    cancelRmrk = cancelewaybillirn.cancelRmrk,
                    cancelRsnCode = cancelewaybillirn.cancelRsnCode,
                    ewbNo = cancelewaybillirn.ewbNo
                };

                var RequestjsonString = JsonConvert.SerializeObject(reqParam);

                OrderEwayBillsGenError ctrp = new OrderEwayBillsGenError()
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Type = "Request",
                    Remarks = "B2BCancelApi",
                    Orderid = cancelewaybillirn.TransferOrderid,
                    ErrorDescription = RequestjsonString,
                };
                context.OrderEwayBillsGenErrors.Add(ctrp);
                var client = new RestClient(BaseUrl + "/v2/eInvoice/ewaybill/cancel");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Cleartax-Auth-Token", eInvoiceAuthKey);
                request.AddHeader("gstin", ware.GSTin);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                CancelEwayResponse RESS = JsonConvert.DeserializeObject<CancelEwayResponse>(response.Content);

                var orderEwayBillsGenErrors = context.OrderEwayBillsGenErrors.FirstOrDefault(x => x.Remarks == "InternalTransferCancelApi" && x.Orderid == cancelewaybillirn.TransferOrderid && x.IsActive == true && x.IsDeleted == false);
                if (orderEwayBillsGenErrors == null)
                {
                    OrderEwayBillsGenError ewayerror = new OrderEwayBillsGenError();
                    ewayerror.ErrorCode = 0;
                    ewayerror.Orderid = cancelewaybillirn.TransferOrderid;
                    ewayerror.IsActive = true;
                    ewayerror.IsDeleted = false;
                    ewayerror.Remarks = "InternalTransferCancelApi";
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
                    var ewayupdate = context.OrderEwayBills.Where(x => x.OrderId == cancelewaybillirn.TransferOrderid && !string.IsNullOrEmpty(x.EwayBillNo) && x.IsCancelEwayBill == false).OrderByDescending(x => x.EwayBillId).FirstOrDefault(); //order id
                    if (ewayupdate != null)
                    {
                        ewayupdate.EwayBillNo = Convert.ToString(RESS.ewbNumber);
                        ewayupdate.ModifiedDate = DateTime.Now;
                        ewayupdate.ModifiedBy = userid;
                        ewayupdate.IsCancelEwayBill = true;
                        ewayupdate.EwayBillCancelDate = DateTime.Now;
                        context.Entry(ewayupdate).State = EntityState.Modified;
                    }
                    context.Commit();
                    res.Message = "EwayBillCanceled For Number=" + RESS.ewbNumber + ",TransferOrderid=" + cancelewaybillirn.TransferOrderid;
                    res.status = true;
                    return res;
                }
                else
                {
                    res.Message = "Eway Bill Not Found";
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
            //return res;
        }
    }
}

