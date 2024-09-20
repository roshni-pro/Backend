using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.IRN
{

    [RoutePrefix("api/IRNReGenerate")]
    public class IRNReGenerateController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpGet]
        [AllowAnonymous]
        [Route("GenerateExcelIRN")]
        public async Task<string> GenerateExcelIRN(int orderid)
        {
            string result = "";
            IRNExcelHelper iRNExcelHelper = new IRNExcelHelper();
            result = await iRNExcelHelper.CreateExcelData(orderid);

            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetSearchOrderMaster")]
        public PaggingData GetSearchOrderMaster(FilterOrderDTO param)
        {
            return SearchOrderMaster(param);
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("RegenerateIRN")]
        public async Task<bool> ReGenerateIRN(FilterOrderDTO param)
        {
            bool result = false;
            if (string.IsNullOrEmpty(param.OrderId.ToString()) && param.OrderId <= 0)
            {
                return result;
            }
            try
            {
                logger.Info("start ReGenerateIRN: ");
                using (var context = new AuthContext())
                {
                    var clearTax = context.ClearTaxIntegrations.FirstOrDefault(x => x.Id == param.GenerationId);
                    if (clearTax != null)
                    {
                        clearTax.IsActive = false;
                        context.Entry(clearTax).State = System.Data.Entity.EntityState.Modified;
                    }
                    ClearTaxIntegration clear = new ClearTaxIntegration()
                    {
                        OrderId = param.OrderId,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        APIType = clearTax.APIType,
                        IsProcessed = false,
                        IsOnline = true,
                    };
                    context.ClearTaxIntegrations.Add(clear);
                    context.Commit();
                }

                IRNHelper iRNHelper = new IRNHelper();
                result = await iRNHelper.PostIRNToClearTax(param.OrderId);
                logger.Info("End ReGenerateIRN: ");
                return result;
            }
            catch (Exception ex)
            {
                logger.Info("End ReGenerateIRN: ");
                logger.Error("Error in ReGenerateIRN " + ex.Message);
                return result;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GettingEInvoicebyIRN")]
        public async Task<bool> GettingEInvoicebyIRN(GetIRNNoDC param)
        {
            bool result = false;
            try
            {
                string irn = string.Empty;
                using (var context = new AuthContext())
                {
                    logger.Info("Start GettingEInvoicebyIRN: ");
                    var clearTax = context.ClearTaxIntegrations.FirstOrDefault(x => x.Id == param.GenerationId && x.IsActive == true);
                    if (clearTax != null)
                    {
                        irn = clearTax.IRNNo;
                        clearTax.IsActive = false;
                        context.Entry(clearTax).State = System.Data.Entity.EntityState.Modified;
                    }
                    ClearTaxIntegration clear = new ClearTaxIntegration()
                    {
                        OrderId = param.OrderId,
                        IRNNo = param.irn,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        APIType = clearTax.APIType == "GenerateCN" ? "GetCN" : "GetIRN",
                        IsProcessed = false,
                        IsOnline = true,
                    };
                    context.ClearTaxIntegrations.Add(clear);
                    context.Commit();
                }

                IRNHelper iRNHelper = new IRNHelper();
                result = await iRNHelper.PostIRNToClearTax(param.OrderId);
                logger.Info("End GettingEInvoicebyIRN: ");

            }
            catch (Exception ex)
            {
                logger.Info("End GettingEInvoicebyIRN: ");
                logger.Error("Error in GettingEInvoicebyIRN " + ex.Message);
                return result;
            }
            return result;
            //return JsonConvert.DeserializeObject( ErrorJson);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ConvertB2C")]
        public bool ConvertB2C(GetIRNNoDC param)
        {
            bool result = false;
            try
            {
                logger.Info("Start ConvertB2C: ");
                using (var context = new AuthContext())
                {
                    var ODM = context.OrderDispatchedMasters.Where(x => x.OrderId == param.OrderId && x.active).FirstOrDefault();
                    ODM.IsGenerateIRN = false;
                    context.Entry(ODM).State = System.Data.Entity.EntityState.Modified;
                    context.Commit();
                    result = true;
                    logger.Info("End ConvertB2C: ");
                }
            }
            catch (Exception ex)
            {
                logger.Info("End ConvertB2C: ");
                logger.Error("Error in ConvertB2C " + ex.Message);
                result = false;
            }
            return result;
        }

        public PaggingData SearchOrderMaster(FilterOrderDTO filterOrderDTO)
        {
            PaggingData paggingData = new PaggingData();
            try
            {
                logger.Info("start SearchOrderMasterIRN: ");
                int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;
                int take = filterOrderDTO.ItemPerPage;
                List<IRNSearchOrderMaster> result = null;

                using (var context = new AuthContext())
                {
                    DataTable dtWH = new DataTable();
                    dtWH.Columns.Add("IntValue");
                    filterOrderDTO.WarehouseIds.ForEach(x =>
                    {
                        DataRow dr = dtWH.NewRow();
                        dr["IntValue"] = x;
                        dtWH.Rows.Add(dr);
                    });
                    DataTable dtCity = new DataTable();
                    dtCity.Columns.Add("IntValue");

                    filterOrderDTO.Cityids.ForEach(x =>
                    {
                        DataRow dr = dtCity.NewRow();
                        dr["IntValue"] = x;
                        dtCity.Rows.Add(dr);
                    });

                    var OrderID = new SqlParameter
                    {
                        ParameterName = "OrderID",
                        Value = filterOrderDTO.OrderId
                    };
                    var WarehouseID = new SqlParameter
                    {
                        TypeName = "dbo.intvalues",
                        ParameterName = "WarehouseID",
                        Value = dtWH
                    };
                    var CityID = new SqlParameter
                    {
                        TypeName = "dbo.intvalues",
                        ParameterName = "CityID",
                        Value = dtCity
                    };
                    var Skip = new SqlParameter
                    {
                        ParameterName = "skip",
                        Value = skip
                    };
                    var Take = new SqlParameter
                    {
                        ParameterName = "Take",
                        Value = take
                    };
                    result = context.Database.SqlQuery<IRNSearchOrderMaster>("exec GetOrdersListforIRNReGenerate @OrderID,@WarehouseID,@CityID,@skip,@Take", OrderID, WarehouseID, CityID, Skip, Take).ToList();

                    //  result = context.Database.SqlQuery<IRNSearchOrderMaster>("exec GetOrdersListforIRNReGenerate @OrderID,@WarehouseID,@CityID", OrderID, WarehouseID, CityID).ToList();

                    paggingData.ordermaster = result;
                    paggingData.total_count = result.Select(x => x.TotalRecords).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in SearchOrderMasterIRN " + ex.Message);
                logger.Info("End  SearchOrderMasterIRN: ");
                paggingData.ordermaster = null;
                paggingData.total_count = 0;
            }

            return paggingData;
        }
    }
    public class GetIRNNoDC
    {
        public long GenerationId { get; set; }
        public string irn { get; set; }
        public int OrderId { get; set; }
    }
    public class IRNSearchOrderMaster
    {
        public long GenerationId { get; set; }
        public int TotalRecords { get; set; }
        public int OrderId { get; set; }

        public int OrderType { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string paymentMode { get; set; }
        public string WarehouseName { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public string Customerphonenum { get; set; }
        public double OrderAmount { get; set; }
        public double DispatchAmount { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string Error { get; set; }
        public string IRNStatus { get; set; }
    }
    public class Errors
    {
        public string error_code { get; set; }
        public string error_message { get; set; }
        public object error_id { get; set; }
        public string error_source { get; set; }
    }
}
