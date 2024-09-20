using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/GSTReport")]
    public class GSTReportController : ApiController
    {
        [Route("B2B")]
        [HttpGet]
        public async Task<string> GetGst(DateTime? from, DateTime? to, int Stateid)
        {
            string asString = from?.ToString("dd/MM/yyyy");
            string asString1 = to?.ToString("dd/MM/yyyy");

            using (var db = new AuthContext())
            {
                var citylist = db.Cities.Where(x => x.Stateid == Stateid && x.active == true).Select(x => x.Cityid).ToList();
                var state = db.States.FirstOrDefault(x => x.Stateid == Stateid);

                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

                GstAllList data = new GstAllList();

                to = to.Value.Date.AddDays(1).AddMilliseconds(-1);
                GstReportManager gstReportManager = new GstReportManager();
                var orderMasters = await gstReportManager.GetGstReportDatasAsync(from.Value, to.Value, citylist);

                var customerIds = orderMasters.Where(x => !string.IsNullOrEmpty(x.Tin_No) && x.Tin_No.Length == 15)
                                  .Select(x => x.CustomerId).Distinct().ToList();

                var itemIds = orderMasters.Select(z => z.ItemId).Distinct().ToList();

                var customerGstList = new List<CustomerStateGst>();
                var itemsList = new List<ItemWithUOM>();

                var itemIdDt = new DataTable();
                itemIdDt.Columns.Add("IntValue");
                foreach (var item in itemIds)
                {
                    var dr = itemIdDt.NewRow();
                    dr["IntValue"] = item;
                    itemIdDt.Rows.Add(dr);
                }

                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("IntValue");
                foreach (var item in customerIds)
                {
                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = item;
                    orderIdDt.Rows.Add(dr);
                }

                using (var authContext = new AuthContext())
                {
                    var param = new SqlParameter("param", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    customerGstList = authContext.Database.SqlQuery<CustomerStateGst>("exec GetCustomerStateGst @param", param).ToList();

                    var param1 = new SqlParameter("param", itemIdDt);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.IntValues";
                    itemsList = authContext.Database.SqlQuery<ItemWithUOM>("exec GetItemsWithUOM @param", param1).ToList();
                }

                #region B2B
                data.gstList = orderMasters.Where(x => !string.IsNullOrEmpty(x.Tin_No) && x.Tin_No.Length == 15 && x.AmtWithoutTaxDisc > 0 && x.createddate >= from.Value && x.createddate <= to.Value)
                    .GroupBy(z => new { z.InvoiceNo, z.TaxPercentage, z.Tin_No, z.createddate, z.GrossAmount, z.CustomerId, z.ShopName })
                    .Select(x => new GSTREPORT
                    {
                        InvoiceNo = x.Key.InvoiceNo,
                        ApplicableofTaxRate = "",
                        CessTaxAmount = x.Sum(a => a.CessTaxAmount) != 0 ? Math.Round(x.Sum(a => a.CessTaxAmount), 3).ToString() : x.Sum(a => a.CessTaxAmount).ToString().Replace(".", string.Empty),
                        GSTIN = x.Key.Tin_No,
                        InvoiceDate = x.Key.createddate.ToString(@"dd-MM-yyyy"),
                        InvoiceType = "Regular",
                        InvoiceValue = x.Key.GrossAmount.ToString("#.##"),
                        PlaceOfSupply = string.Format("{0}-{1}", customerGstList.FirstOrDefault(a => a.CustomerId == x.Key.CustomerId)?.GSTNo, customerGstList.FirstOrDefault(a => a.CustomerId == x.Key.CustomerId)?.StateName),
                        Rate = x.Key.TaxPercentage.ToString().Replace(".", string.Empty),
                        ReverseCharge = "N",
                        TaxableValue = x.Sum(a => a.AmtWithoutAfterTaxDisc).ToString("#.##"),
                        ReceiverName = x.Key.ShopName,
                        ECommerceGSTIN = ""
                    }).ToList();

                #endregion

                #region B2C
                data.B2CSList = orderMasters.Where(x => (string.IsNullOrEmpty(x.Tin_No) || x.Tin_No.Length != 15) && x.createddate >= from.Value && x.createddate <= to.Value)
                  .GroupBy(z => new
                  {
                      PlaceOfSupply = string.Format("{0}-{1}", state.GSTNo, state.AliasName),
                      z.TaxPercentage
                  }).Select(z => new B2CS
                  {
                      Type = "OE",
                      ApplicableofTaxRate = "",
                      CessTaxAmount = z.Sum(a => a.CessTaxAmount) != 0 ? z.Sum(a => a.CessTaxAmount).ToString() : z.Sum(a => a.CessTaxAmount).ToString().Replace(".", string.Empty),
                      PlaceOfSupply = z.Key.PlaceOfSupply,
                      Rate = z.Key.TaxPercentage.ToString().Replace(".", string.Empty),
                      TaxableValue = z.Sum(a => a.AmtWithoutAfterTaxDisc) != 0 ? z.Sum(a => a.AmtWithoutAfterTaxDisc).ToString() : z.Sum(a => a.AmtWithoutAfterTaxDisc).ToString().Replace(".", string.Empty),
                      ECommerceGSTIN = ""

                  }).ToList();

                #endregion

                #region CREDIT_NOTE_DETAILS
                data.CreditDetailsList = orderMasters.Where(x => !string.IsNullOrEmpty(x.Tin_No) && x.Tin_No.Length == 15 && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value
                                            && x.Status == "Post Order Canceled")
                  .GroupBy(z => new
                  {
                      z.TaxPercentage,
                      z.Tin_No,
                      z.InvoiceNo,
                      z.PocCreditNoteNumber,
                      z.PocCreditNoteDate,
                      z.ShopName,
                      z.GrossAmount,
                      z.createddate,
                      z.CustomerId
                  }).Select(z => new CreditDetails
                  {
                      ApplicableofTaxRate = "",
                      CessTaxAmount = z.Sum(a => a.CessTaxAmount) != 0 ? Math.Round(z.Sum(a => a.CessTaxAmount), 3).ToString() : z.Sum(a => a.CessTaxAmount).ToString().Replace(".", string.Empty),
                      PlaceOfSupply = string.Format("{0}-{1}", customerGstList.FirstOrDefault(a => a.CustomerId == z.Key.CustomerId)?.GSTNo, customerGstList.FirstOrDefault(a => a.CustomerId == z.Key.CustomerId)?.StateName),
                      Rate = z.Key.TaxPercentage.ToString().Replace(".", string.Empty),
                      InvoiceValue = z.Key.GrossAmount.ToString("#.##"),
                      GSTIN = z.Key.Tin_No,
                      InvoiceDate = z.Key.createddate.ToString(@"dd-MM-yyyy"),
                      TaxableValue = z.Sum(a => a.AmtWithoutAfterTaxDisc).ToString("#.##"),
                      ReceiverName = z.Key.ShopName,
                      InvoiceNo = z.Key.InvoiceNo,
                      PreGST = "",
                      RefundVoucherNo = z.Key.PocCreditNoteNumber,
                      RefundVoucherDate = z.Key.PocCreditNoteDate.HasValue ? z.Key.PocCreditNoteDate.Value.ToString(@"dd-MM-yyyy") : "",
                      DocumentType = "C"
                  }).ToList();

                #endregion

                #region CREDIT_NOTE_DETAILS_B2C

                data.CreditDetailsListB2Cs = orderMasters.Where(x => (string.IsNullOrEmpty(x.Tin_No) || x.Tin_No.Length != 15)
                                            && x.Status == "Post Order Canceled" && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value)
                 .GroupBy(z => new
                 {
                     PlaceOfSupply = string.Format("{0}-{1}", state.GSTNo, state.AliasName),
                     z.TaxPercentage
                 }).Select(z => new CreditDetailsB2CS
                 {
                     Type = "OE",
                     ApplicableofTaxRate = "",
                     CessTaxAmount = z.Sum(a => a.CessTaxAmount) != 0 ? z.Sum(a => a.CessTaxAmount).ToString() : z.Sum(a => a.CessTaxAmount).ToString().Replace(".", string.Empty),
                     PlaceOfSupply = z.Key.PlaceOfSupply,
                     Rate = z.Key.TaxPercentage.ToString().Replace(".", string.Empty),
                     TaxableValue = z.Sum(a => a.AmtWithoutAfterTaxDisc) != 0 ? z.Sum(a => a.AmtWithoutAfterTaxDisc).ToString() : z.Sum(a => a.AmtWithoutAfterTaxDisc).ToString().Replace(".", string.Empty),
                     ECommerceGSTIN = ""

                 }).ToList();

                #endregion

                #region HSN Summary

                data.HSNList = orderMasters
                .GroupBy(z => new { z.HSNCode }).Select(z => new HSNSummary
                {
                    HSNCode = z.Key.HSNCode,
                    CentralTaxAmount = (z.Where(a => a.OrderStateId == a.CustomerStateId
                                                   && a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.CGSTTaxAmmount)
                                        -
                                        orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value && a.OrderStateId == a.CustomerStateId)
                                        .Sum(a => a.CGSTTaxAmmount)).ToString(),
                    CessAmount = (z.Where(x => x.createddate >= from.Value && x.createddate <= to.Value
                                                                     && ((!x.PocCreditNoteDate.HasValue && x.Status != "Post Order Canceled")
                                                                             || (x.Status == "Post Order Canceled" && x.PocCreditNoteDate.HasValue
                                                                             && x.PocCreditNoteDate.Value > to.Value))).Sum(a => a.CessTaxAmount)
                                  -
                                   orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value)
                                        .Sum(a => a.CessTaxAmount)).ToString()
                                  ,
                    IntegratedTaxAmount = (z.Where(a => a.OrderStateId != a.CustomerStateId
                                             && a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.SGSTTaxAmmount + a.CGSTTaxAmmount)
                                            -
                                             orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                                && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value && a.OrderStateId != a.CustomerStateId)
                                                .Sum(a => a.SGSTTaxAmmount + a.CGSTTaxAmmount)).ToString()
                                            ,
                    StateTaxAmount = (z.Where(a => a.OrderStateId == a.CustomerStateId
                                               && a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.SGSTTaxAmmount)
                                        -
                                        orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value && a.OrderStateId == a.CustomerStateId)
                                        .Sum(a => a.SGSTTaxAmmount)).ToString(),
                    TaxableValue = (z.Where(a => a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.AmtWithoutAfterTaxDisc)
                                    -
                                    orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value)
                                        .Sum(a => a.AmtWithoutAfterTaxDisc)).ToString()
                                    ,
                    TotalQuantity = (z.Where(a => a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.qty)
                                    -
                                    orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value)
                                        .Sum(a => a.qty)).ToString(),
                    TotalValue = (z.Where(a => a.createddate >= from.Value && a.createddate <= to.Value
                                                       && ((!a.PocCreditNoteDate.HasValue && a.Status != "Post Order Canceled")
                                                               || (a.Status == "Post Order Canceled" && a.PocCreditNoteDate.HasValue
                                                               && a.PocCreditNoteDate.Value > to.Value))).Sum(a => a.SGSTTaxAmmount + a.CessTaxAmount + a.CGSTTaxAmmount + a.AmtWithoutAfterTaxDisc)
                                  -
                                   orderMasters.Where(a => a.createddate < from.Value && a.PocCreditNoteDate.HasValue && a.HSNCode == z.Key.HSNCode
                                        && a.PocCreditNoteDate.Value >= from.Value && a.PocCreditNoteDate.Value <= to.Value)
                                        .Sum(a => a.SGSTTaxAmmount + a.CessTaxAmount + a.CGSTTaxAmmount + a.AmtWithoutAfterTaxDisc)).ToString()
                                  ,
                    UQC = itemsList.FirstOrDefault(s => s.HSNCode == z.Key.HSNCode)?.UOM
                }).ToList();

                #endregion

                #region Documents_issued

                data.document = new List<documentList> {
                new documentList
                {
                    SrNoFrom=orderMasters.Where(x=>x.createddate >= from.Value && x.createddate <= to.Value).OrderBy(x=>x.createddate).FirstOrDefault().InvoiceNo,
                    SrNoTo=orderMasters.Where(x=>x.createddate >= from.Value && x.createddate <= to.Value).OrderByDescending(x=>x.createddate).FirstOrDefault().InvoiceNo,
                    TotalNumber=orderMasters.Where(x=>x.createddate >= from.Value && x.createddate <= to.Value).Select(x=>x.InvoiceNo).Distinct().Count(),
                    Cancelled=orderMasters.Where(x=>x.Status == "Post Order Canceled" && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value).Select(x=>x.InvoiceNo).Distinct().Count(),
                    NameOfDocument="InvoiceNote"
                },
                new documentList
                {
                    SrNoFrom=orderMasters.Where(x=>x.Status == "Post Order Canceled" && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value).OrderBy(x=>x.createddate).FirstOrDefault().PocCreditNoteNumber,
                    SrNoTo=orderMasters.Where(x=>x.Status == "Post Order Canceled" && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value).OrderByDescending(x=>x.createddate).FirstOrDefault().PocCreditNoteNumber,
                    TotalNumber=orderMasters.Where(x=>x.Status == "Post Order Canceled" && x.PocCreditNoteDate >= from.Value && x.PocCreditNoteDate <= to.Value).Select(x=>x.PocCreditNoteNumber).Distinct().Count(),
                    Cancelled=0,
                    NameOfDocument="CreditNote"
                }};

                #endregion


                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                #region Create dataset and excel file for them
                var dataTables = new List<DataTable>();
                var gstDt = ClassToDataTable.CreateDataTable(data.gstList);
                gstDt.TableName = "B2B";
                dataTables.Add(gstDt);
                var b2CDt = ClassToDataTable.CreateDataTable(data.B2CSList);
                b2CDt.TableName = "B2CS";
                dataTables.Add(b2CDt);

                var creditDt = ClassToDataTable.CreateDataTable(data.CreditDetailsList);
                creditDt.TableName = "CREDIT_NOTE_DETAILS";
                dataTables.Add(creditDt);

                var creditDtB2CS = ClassToDataTable.CreateDataTable(data.CreditDetailsListB2Cs);
                creditDtB2CS.TableName = "CREDIT_NOTE_DETAILS_B2CS";
                dataTables.Add(creditDtB2CS);

                var hsnDt = ClassToDataTable.CreateDataTable(data.HSNList);
                hsnDt.TableName = "HSN_SUMMARY";
                dataTables.Add(hsnDt);

                var documentDt = ClassToDataTable.CreateDataTable(data.document);
                documentDt.TableName = "DOCUMENTS_ISSUED";
                dataTables.Add(documentDt);

                var fileName = "GSTReport_" + state.AliasName + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                string filePath = ExcelSavePath + fileName;

                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                #endregion


                var fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , HttpContext.Current.Request.Url.Port
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));

                return fileUrl;

            }
        }
    }

}