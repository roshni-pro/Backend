using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc;
using AngularJSAuthentication.API.Helper.EwayBill;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity;
using AngularJSAuthentication.Model.ClearTax;

namespace AngularJSAuthentication.API.EwayBill
{

    [RoutePrefix("api/InternalEwaybill")]
    public class InternalEwaybillController : ApiController
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("InternalIRNGenerate")]
        public async Task<bool> InternalIRNGenerateAndEwayBill(string TransportGST=null, string TransportName=null, int? TransferOrderId = null)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            bool status = false;
            using (var db = new AuthContext())
            {
                InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                status = await internalEwayBillHelper.PostInternalTransferIRNToClearTax(TransportGST, TransportName, TransferOrderId);
            }
            return status;
        }

        #region  InternalTransferEway bill By IRN
        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateEwaybillByIRN")] //interstate and having orderamount>states table
        public async Task<bool> GenerateEwaybillByIRNInternal(InternalTransferEwaybillParam param)
        {
            bool flag = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                var data = context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == param.TransferOrderId).FirstOrDefault(); //irn

                var ware = context.Warehouses.Where(x => x.WarehouseId == data.WarehouseId).FirstOrDefault(); //gst

                flag = await internalEwayBillHelper.GenrateEwayBillByIRN(context, data, ware, param, userid);
            }
            return flag;

        }
        #endregion

        //#region  InternalTransfer Eway bill Without IRN
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("InternalTransferEwayBillNonIRN")]  //intrastate and having orderamount>states table
        //public async Task<bool> InternalTransferEwayBillNonIRN(InternalTransferEwaybillParam param)
        //{
        //    bool flag = false;
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    using (AuthContext context = new AuthContext())
        //    {
        //        InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
        //        var data = await context.TransferWHOrderDispatchedMasterDB.FirstOrDefaultAsync(x => x.TransferOrderId == param.TransferOrderId); //

        //        var Sellerware = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == data.RequestToWarehouseId); //seller 

        //        flag = await internalEwayBillHelper.GenrateEwayBillNonIRN(context, data, Sellerware, param, userid);
        //    }
        //    return flag;
        //}
        //#endregion



        #region  RegenerateInternalTransferEway bill
        [AllowAnonymous]
        [HttpPost]
        [Route("ReGenerateEwaybillInternalTransfer")] //interstate and having orderamount>states table
        public async Task<bool> ReGenerateEwaybillInternalTransfer(InternalTransferEwaybillParam param)
        {
            bool flag = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                var data = context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == param.TransferOrderId).FirstOrDefault(); //irn
                var Sellerware = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == data.RequestToWarehouseId); //seller 
                var ware = context.Warehouses.Where(x => x.WarehouseId == data.WarehouseId).FirstOrDefault(); //gst

                string gstin = Sellerware.GSTin.Substring(0, 2);
                string gstin1 = ware.GSTin.Substring(0, 2);
                ClearTaxIntegration clear = new ClearTaxIntegration()
                {
                    OrderId = param.TransferOrderId,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    APIType = "GenerateIRN",
                    IsProcessed = false,
                    IsOnline = false,
                    APITypes = 2
                };
                context.ClearTaxIntegrations.Add(clear);
                context.Commit();
                if (gstin != gstin1)
                {
                    //flag = await internalEwayBillHelper.GenrateEwayBillByIRN(context, data, ware, param, userid);
                    flag = await InternalIRNGenerateAndEwayBill(param.TransportGST, param.TransportName, param.TransferOrderId);
                }
                else
                {
                    flag = await internalEwayBillHelper.GenrateEwayBillNonIRN(clear, context, data, Sellerware, param, userid);
                }
            }
            return flag;

        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("GetInternalPageData")]
        public async Task<InternalEwayBillOrderListAll> GetEwayBillOrderListInternal(InternalTransferParam param)
        {

            InternalEwayBillOrderListAll alldata = new InternalEwayBillOrderListAll();
            using (var context = new AuthContext())
            {
                List<InternalEwayBillOrderList> newdata = new List<InternalEwayBillOrderList>();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in param.Warehouseids)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var WarehouseidParam = new SqlParameter("Warehouseids", IdDt);
                WarehouseidParam.SqlDbType = SqlDbType.Structured;
                WarehouseidParam.TypeName = "dbo.IntValues";

                var StartDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = param.StartDate == null ? DBNull.Value : (object)param.StartDate
                };
                var TransferId = new SqlParameter
                {
                    ParameterName = "TransferOrderId",
                    Value = param.TransferOrderId
                };
                var EndDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = param.EndDate == null ? DBNull.Value : (object)param.EndDate
                };

                var invoice = new SqlParameter
                {
                    ParameterName = "invoicenumber",
                    Value = param.invoicenumber == null ? DBNull.Value : (object)param.invoicenumber
                };
                var skip = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = param.Skip
                };
                var take = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = param.Take
                };

                newdata = await context.Database.SqlQuery<InternalEwayBillOrderList>("InternalTransferEwayBill @Warehouseids,@StartDate,@EndDate,@TransferOrderId,@invoicenumber,@Skip,@Take", WarehouseidParam, StartDateParam, EndDateParam, TransferId, invoice, skip, take).ToListAsync();
                alldata.internalEwayBillOrderLists = newdata;
                alldata.TotalCount = newdata != null && newdata.Any() ? newdata.FirstOrDefault().TotalCount : 0;
                return alldata;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateInternalTransferVehicle")]
        public async Task<EwaybillBackendResponceDc> UpdateVehiclePartB(UpdatePartBInternalRequest updatePartBParam)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    var CompanyDetails = ewayBillHelper.GetcompanyDetails(context);

                    if (CompanyDetails != null)
                    {
                        var warehouseId = await context.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == updatePartBParam.TransferOrderId).Select(x => x.WarehouseId).FirstOrDefaultAsync();
                        var warehouse = ewayBillHelper.GetWarehouseDetails(warehouseId.Value, context);
                        if (warehouse != null)
                        {
                            string newReasonCode = "";
                            if (updatePartBParam.ReasonCode == "1") { newReasonCode = "BREAKDOWN"; }
                            else if (updatePartBParam.ReasonCode == "2") { newReasonCode = "TRANSSHIPMENT"; }
                            else if (updatePartBParam.ReasonCode == "3") { newReasonCode = "OTHERS"; }
                            else if (updatePartBParam.ReasonCode == "4") { newReasonCode = "FIRST_TIME"; }

                            UpdatePartBInternalRequest updatePartB2B = new UpdatePartBInternalRequest();
                            updatePartB2B.TransferOrderId = updatePartBParam.TransferOrderId;
                            updatePartB2B.EwbNumber = updatePartBParam.EwbNumber;
                            updatePartB2B.FromPlace = updatePartBParam.FromPlace;
                            updatePartB2B.FromState = Convert.ToInt32(warehouse.GSTin.Substring(0, 2));//updatePartBParam.FromState;
                            updatePartB2B.ReasonCode = newReasonCode;
                            updatePartB2B.ReasonRemark = "partb";
                            updatePartB2B.TransDocNo = updatePartBParam.TransDocNo;
                            updatePartB2B.TransDocDt = updatePartBParam.TransDocDt;
                            updatePartB2B.TransMode = "ROAD";
                            updatePartB2B.DocumentNumber = "";
                            updatePartB2B.DocumentDate = "";
                            updatePartB2B.VehicleType = "REGULAR";
                            updatePartB2B.VehNo = updatePartBParam.VehNo.Trim().Replace(" ", "").ToUpper().ToString();
                            // insert dc

                            InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                            res = await internalEwayBillHelper.UpdateInternalVehiclePartBIRN(updatePartB2B, context, userid, warehouse, CompanyDetails);

                        }
                    }
                    else
                    {
                        res.Message = "Eway Bill Service Not Started!!";
                        res.status = false;
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("ExtendInternalEwaybill")]
        public async Task<EwaybillBackendResponceDc> ExtendEwaybill(InternalTransferExtendRequest extendparam)
        {
            try
            {
                EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
                using (var authContext = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                    return res = await internalEwayBillHelper.ExtendEwayBillInternalTransfer(extendparam, authContext, userid);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("CancelInternalEwayBill")]
        public async Task<EwaybillBackendResponceDc> CancelInternalEwayBill(InternalCancelEwaybillParam cancelParam)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            try
            {
                EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
                using (var authContext = new AuthContext())
                {
                    InternalEwayBillHelper internalEwayBillHelper = new InternalEwayBillHelper();
                    return res = await internalEwayBillHelper.CancelEwayBillInternalbyIRN(cancelParam, authContext, userid);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("GetEWaybillPDF")]
        //public async Task<EwaybillBackendResponceDc> GetEWaybillPDF(PostPDFParam postPDFParam)
        //{
        //    EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
        //    using (var db = new AuthContext())
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        EwayBillHelper ewayBillHelper = new EwayBillHelper();

        //        res = await ewayBillHelper.GetEwayBillPdf(postPDFParam, db, userid);
        //    }
        //    return res;
        //}


        public class InternalTransferParam
        {
            public List<int> Warehouseids { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Status { get; set; }
            public int TransferOrderId { get; set; }
            public string invoicenumber { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }

        }

        public class InternalTransferEwaybillParam
        {
            public int TransferOrderId { get; set; }
            public string TransportGST { get; set; }
            public string TransportName { get; set; }
            public string vehicleno { get; set; }
            public string TransDocNo { get; set; }
            public DateTime? TransDocDt { get; set; }
            public string IrnNo { get; set; }
            public int distance { get; set; }
        }
        public class UpdatePartBInternalRequest
        {
            public int TransferOrderId { get; set; }
            public long EwbNumber { get; set; }
            public string FromPlace { get; set; }
            public int FromState { get; set; }
            public string ReasonCode { get; set; }
            public string ReasonRemark { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string TransMode { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentType { get; set; }
            public string DocumentDate { get; set; }
            public string VehicleType { get; set; }
            public string VehNo { get; set; }
            public int Customertype { get; set; }
        }

        public class InternalTransferExtendRequest //internal transfer 
        {
            public int TransferOrderid { get; set; }
            public long EwbNumber { get; set; }
            public string FromPlace { get; set; }
            public string FromState { get; set; }
            public string FromPincode { get; set; }
            public string ReasonCode { get; set; }
            public string ReasonRemark { get; set; }
            public string TransDocNo { get; set; }
            public string TransDocDt { get; set; }
            public string TransMode { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentType { get; set; }
            public string RemainingDistance { get; set; }
            public string ConsignmentStatus { get; set; }
            public string DocumentDate { get; set; }
            public string VehicleType { get; set; }
            public string VehNo { get; set; }
        }

        public class InternalCancelEwaybillParam //internal
        {
            public int TransferOrderid { get; set; }
            public long ewbNo { get; set; }
            public int cancelRsnCode { get; set; }
            public string cancelRmrk { get; set; }
        }


        //public class  extendError
        //{
        //    public int error_code { get; set; }
        //    public string error_message { get; set; }
        //}
    }


}

