using AngularJSAuthentication.Common.Helpers;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class IRNExcelHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public async Task<string> CreateExcelData(int OrderId)
        {
            string result = "";
            try
            {
                logger.Info("start Create Excel for IRN Order ID : " + OrderId.ToString());
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);
                var fileName = "IRNGenerateFilebyClearTax" + OrderId.ToString() + ".xlsx";
                string filePath = ExcelSavePath + fileName;
                var returnPath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , HttpContext.Current.Request.Url.Port
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
                List<GenIRNExcelDC> genIRNExcelDCs = new List<GenIRNExcelDC>();
                using (var context = new AuthContext())
                {

                    var orderDispachedMaster = await context.OrderDispatchedMasters.FirstOrDefaultAsync(x => x.OrderId == OrderId);
                    var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == orderDispachedMaster.WarehouseId);
                    var StateList = await context.States.ToListAsync();
                    var citylist = await context.Cities.ToListAsync();
                    var customer = context.Customers.Where(x => x.CustomerId == orderDispachedMaster.CustomerId).FirstOrDefault();
                    var items = context.OrderDispatchedDetailss.Where(x => x.OrderId == OrderId).ToList();


                    if (string.IsNullOrEmpty(customer.RefNo))
                    {
                        return "Customer's Gst No is empty";
                    }

                    string custStateCode = customer.RefNo.Substring(0, 2);
                    string whStateCode = warehouse.GSTin.Substring(0, 2);
                    int cnt = 0;
                    bool Igst = false;
                    if (whStateCode != custStateCode)
                    {
                        Igst = true;
                    }

                    double igstamt1 = 0;
                    double CGSTTaxAmmount1 = 0;
                    double SGSTTaxAmmount1 = 0;
                    if (Igst)
                    {
                        igstamt1 = orderDispachedMaster.CGSTTaxAmmount + orderDispachedMaster.SGSTTaxAmmount;
                    }
                    else
                    {
                        CGSTTaxAmmount1 = orderDispachedMaster.CGSTTaxAmmount;
                        SGSTTaxAmmount1 = orderDispachedMaster.SGSTTaxAmmount;
                    }
                    double TotalAmtWithoutTaxDisc = 0;
                      
                    TotalAmtWithoutTaxDisc = items.Sum(x => x.AmtWithoutAfterTaxDisc);

                    foreach (var item in items)
                    {
                        double igstamt = 0;
                        double CGSTTaxAmmount = 0;
                        double SGSTTaxAmmount = 0;
                        if (Igst)
                        {
                            igstamt = item.CGSTTaxAmmount + item.SGSTTaxAmmount;
                        }
                        else
                        {
                            CGSTTaxAmmount = item.CGSTTaxAmmount;
                            SGSTTaxAmmount = item.SGSTTaxAmmount;
                        }
                        cnt = cnt + 1;
                        var withoutTaxUnitPrice = item.CGSTTaxPercentage + item.SGSTTaxPercentage > 0 ?
                                                item.UnitPrice / (1 + ((item.CGSTTaxPercentage + item.SGSTTaxPercentage + item.TotalCessPercentage) / 100))
                                                : item.UnitPrice;



                        GenIRNExcelDC genIRNExcelDC = new GenIRNExcelDC()
                        {
                            DocumentDate = orderDispachedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            DocumentNumber = orderDispachedMaster.invoice_no,
                            DocumentTypeCode = "INV",
                            SupplyTypeCode = "B2B",
                            RecipientLegalName = customer.NameOnGST,
                            RecipientTradeName = customer.ShopName,
                            RecipientGSTIN = customer.RefNo,
                            PlaceofSupply = custStateCode,
                            RecipientAddress1 = customer.BillingAddress1,
                            RecipientPlace = customer.BillingCity,
                            RecipientStateCode = custStateCode,
                            RecipientPINCode = customer.BillingZipCode,
                            //item
                            SlNo = cnt.ToString(),
                            ItemDescription = item.itemname,
                            IstheitemaGOODGOrSERVICES = "G",
                            HSNorSACcode = item.HSNCode,
                            Quantity = item.qty,
                            UnitofMeasurement = "PCS",
                            ItemPrice = Math.Round(withoutTaxUnitPrice, 2),
                            GrossAmount = item.AmtWithoutTaxDisc,
                            ItemTaxableValue = item.AmtWithoutTaxDisc,
                            GSTRate = item.CGSTTaxPercentage + item.SGSTTaxPercentage,
                            IGSTAmount = igstamt,
                            CGSTAmount = CGSTTaxAmmount,
                            SGSTUTGSTAmount = SGSTTaxAmmount,
                            CompCessAmountAdValorem = item.CessTaxAmount,
                            StateCessAmountAdValorem = 0,
                            OtherCharges = 0,
                            ItemTotalAmount = item.TotalAmt,
                            //item end
                            //Values 
                            TotalTaxableValue = TotalAmtWithoutTaxDisc,
                            IGSTAmountTotal = Math.Round(igstamt1, 3),
                            CGSTAmountTotal = Math.Round(CGSTTaxAmmount1, 3),
                            SGSTUTGSTAmountTotal = Math.Round(SGSTTaxAmmount1, 3),
                            CompCessAmountTotal = 0,
                            StateCessAmountTotal = 0,
                            OtherCharge = orderDispachedMaster.deliveryCharge,
                            RoundOffAmount = 0,
                            TotalInvoiceValueinINR = orderDispachedMaster.TotalAmount,
                            Isreversechargeapplicable = "",
                            IsSec7IGSTActapplicable = "",
                            PrecedingDocumentNumber = "",
                            PrecedingDocumentDate = "",
                            // sup details
                            SupplierLegalName = warehouse.CompanyName,
                            GSTINofSupplier = warehouse.GSTin,
                            SupplierAddress1 = warehouse.Address,
                            SupplierPlace = warehouse.CityName,
                            SupplierStateCode = whStateCode,
                            SupplierPINCode = warehouse.PinCode.ToString(),
                            TypeofExport = "",
                            ShippingPortCode = "",
                            ShippingBillNumber = "",
                            ShippingBillDate = "",
                            PayeeBankAccountNumber = "",
                            PayeeName = "",
                            ModeofPayment = "",
                            BankBranchCode = "",
                            PaymentTerms = "",
                            PaymentInstruction = "",
                            CreditTransferTerms = "",
                            DirectDebitTerms = "",
                            CreditDays = "",
                            ShipToLegalName = customer.NameOnGST,
                            ShipToGSTIN = customer.RefNo,
                            ShipToAddress1 = customer.ShippingAddress,
                            ShipToPlace = customer.City,
                            ShipToPincode = customer.ZipCode,
                            ShipToStateCode = StateList.FirstOrDefault(x => x.StateName == customer.State)?.ClearTaxStateCode,
                            DispatchFromName = warehouse.CompanyName,
                            DispatchFromAddress1 = warehouse.Address,
                            DispatchFromPlace = warehouse.CityName,
                            DispatchFromStateCode = whStateCode,
                            DispatchFromPincode = warehouse.PinCode.ToString(),
                            DocumentPeriodStartDate = orderDispachedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            DocumentPeriodEndDate = orderDispachedMaster.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),

                        };
                        genIRNExcelDCs.Add(genIRNExcelDC);

                    }
                    var dataTables = new List<DataTable>();
                    var IRNExcelDC = ClassToDataTable.CreateDataTable(genIRNExcelDCs);
                    IRNExcelDC.TableName = "genIRNExcelDC";
                    dataTables.Add(IRNExcelDC);
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        logger.Info("end Create Excel for IRN Order ID : " + OrderId.ToString());
                        result = returnPath;
                    }

                }
            }
            catch (Exception ex)
            {
                result = "";
                logger.Info("Error Create Excel for IRN Order ID : " + OrderId.ToString());
                logger.Error("Error in Create Excel for IRN " + ex.Message);
            }
            return result;
        }


    }
}