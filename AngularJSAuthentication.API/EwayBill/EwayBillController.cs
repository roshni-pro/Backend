using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.API.Controllers;
using System.Data.Entity;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Converters;
using AngularJSAuthentication.API.Helper.EwayBill;
using static AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc;
using System.Threading.Tasks;
using static AngularJSAuthentication.API.Helper.EwayBill.EwayBillHelper;

namespace AngularJSAuthentication.API.EwayBill
{

    [RoutePrefix("api/Ewaybill")]
    public class EwayBillController : ApiController
    {

        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateEwayBYIRN")]
        public async Task<bool> EwaybillGeneratebyIRN(EwayBillBackendDc ewayBillPostDc)
        {
            bool res = false;
            using (var authContext = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                EwayBillHelper ewayBillHelper = new EwayBillHelper();
                res = await ewayBillHelper.GenrateEwayBillByIRN(ewayBillPostDc, authContext, userid);
            }
            return res;
        }

        //#region   Eway bill Without IRN
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("OrderEwayBillNonIRN")]
        //public async Task<bool> InternalTransferEwayBillNonIRN(EwayBillBackendDcNonIRN param)
        //{
        //    bool flag = false;
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    using (AuthContext context = new AuthContext())
        //    {
        //        EwayBillHelper ewayBillHelper = new EwayBillHelper();

        //        flag = await ewayBillHelper.GeneratedEwayBillNonIRN(context, param, userid);
        //    }
        //    return flag;
        //}
        //#endregion


        [AllowAnonymous]
        [HttpPost]
        [Route("UpdatePartB")]
        public async Task<EwaybillBackendResponceDc> UpdatePartB(UpdatePartBRequest updatePartBParam)
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
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    return res = await ewayBillHelper.UpdateVehiclePartB(updatePartBParam, authContext, userid);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        [Route("UpdatePartA")]

        public EwaybillBackendResponceDc UpdatePartA(UpdatePartA updatePartA)
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
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    return res = ewayBillHelper.UpdateVehiclePartA(updatePartA, authContext, userid);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CancelEwayBill")]
        public async Task<EwaybillBackendResponceDc> CancelEwayBill(CancelRequestParam cancelParam)
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
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    return res = await ewayBillHelper.CancelEwayBillB2BandB2C(cancelParam, authContext, userid);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ExtendEwaybill")]
        public async Task<EwaybillBackendResponceDc> ExtendEwaybill(ExtendRequestParamB2C extendparam)
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
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    return res = await ewayBillHelper.ExtendValidity(extendparam, authContext, userid);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ExtendValidityEwaybill")]
        public async Task<EwaybillBackendResponceDc> ExtendValidityEwaybill(ExtendRequestParamB2C extendparam)
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
                    EwayBillHelper ewayBillHelper = new EwayBillHelper();
                    return res = await ewayBillHelper.ExtendValidity(extendparam, authContext, userid);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Ewaybillorder data
        [AllowAnonymous]
        [HttpPost]
        [Route("Ewaybillorder")]
        public HttpResponseMessage Ewaybillorder(Ewayparam ewayparam)
        {
            //EwayTotaldata


            bool flag = false;
            EwayTotaldata ewayTotaldata = new EwayTotaldata();
            EWayBillOrderDC ewaydata = new EWayBillOrderDC();
            List<EWayBillOrderDC> Ewaybilldatalist = new List<EWayBillOrderDC>();
            List<EWayBillOrderDC> Ewaybill = new List<EWayBillOrderDC>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                if (ewayparam.skcode == "null")
                {
                    ewayparam.skcode = null;
                }

                if (ewayparam.Status == "null")
                {
                    ewayparam.Status = null;
                }

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in ewayparam.warehouseid)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var WarehouseidParam = new SqlParameter("warehouseid", IdDt);
                WarehouseidParam.SqlDbType = SqlDbType.Structured;
                WarehouseidParam.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in ewayparam.cityid)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var ciid = new SqlParameter("cityid", IdDt);
                ciid.SqlDbType = SqlDbType.Structured;
                ciid.TypeName = "dbo.IntValues";

                var orderidss = new SqlParameter
                {
                    ParameterName = "@OrderIds",
                    Value = ewayparam.orderid

                };

                var skcodess = new SqlParameter
                {
                    ParameterName = "SkCodes",
                    Value = ewayparam.skcode == null ? DBNull.Value : (object)ewayparam.skcode
                };

                var statuss = new SqlParameter
                {
                    ParameterName = "Status",
                    Value = ewayparam.Status == null ? DBNull.Value : (object)ewayparam.Status
                };

                var StartDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = ewayparam.startdate == null ? DBNull.Value : (object)ewayparam.startdate
                };

                var EndDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = ewayparam.EndDate == null ? DBNull.Value : (object)ewayparam.EndDate

                };

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[EwayBillOrderNew]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(ciid);
                cmd.Parameters.Add(WarehouseidParam);
                cmd.Parameters.Add(orderidss);
                cmd.Parameters.Add(skcodess);
                cmd.Parameters.Add(statuss);
                cmd.Parameters.Add(StartDateParam);
                cmd.Parameters.Add(EndDateParam);
                cmd.Parameters.Add(new SqlParameter("@skip", ewayparam.skip));
                cmd.Parameters.Add(new SqlParameter("@take", ewayparam.take));

                // Run the sproc
                var reader = cmd.ExecuteReader();
                Ewaybilldatalist = ((IObjectContextAdapter)context).ObjectContext.Translate<EWayBillOrderDC>(reader).ToList();
                reader.NextResult();
                ewayTotaldata.getEwaydata = Ewaybilldatalist;
                ewayTotaldata.TotalRecord = Ewaybilldatalist != null && Ewaybilldatalist.Any() ? Ewaybilldatalist.FirstOrDefault().totalRecord : 0;
                //if (reader.Read())
                //{
                //= Convert.ToInt32(reader["TotalRecords"]);
                //}

                //foreach (var item in Ewaybilldatalist)
                //{
                //    var st = context.States.Where(x => x.Stateid == item.Stateid).FirstOrDefault();
                //    if (st != null)
                //    {
                //        if (item.orderamount >= st.IntrastateAmount)
                //        {
                //            //EWayBillOrderDC obj = new EWayBillOrderDC();
                //            //obj.IRNStatus = item.IRNStatus;
                //            item.flag = true;
                //        }
                //    }
                //    else
                //    {
                //        item.flag = false;
                //    }
                //}
            }
            return Request.CreateResponse(HttpStatusCode.OK, ewayTotaldata);
        }
        #endregion

        #region FailedEwaybill data
        [AllowAnonymous]
        [HttpPost]
        [Route("FailedEwaybill")]
        public FailedEwayTotaldata FailedEwaybillorder(FailedEwayparam failedEwayparaM)
        {

            FailedEwayTotaldata failedEwayTotaldatanew = new FailedEwayTotaldata();
            List<FailedEWayBillOrderDC> EwayDc = new List<FailedEWayBillOrderDC>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                //string ProcedureName = "Sp_FailedEwayBill @cityid,@warehouseid,@orderid,@ordertype,@startdate,@enddate,@skip,@take";

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in failedEwayparaM.warehouseId)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var WarehouseidParam = new SqlParameter("warehouseid", IdDt);
                WarehouseidParam.SqlDbType = SqlDbType.Structured;
                WarehouseidParam.TypeName = "dbo.IntValues";

                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in failedEwayparaM.cityid)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var ciid = new SqlParameter("cityid", IdDt);
                ciid.SqlDbType = SqlDbType.Structured;
                ciid.TypeName = "dbo.IntValues";

                var orderidss = new SqlParameter
                {
                    ParameterName = "orderid",
                    Value = failedEwayparaM.orderid
                };

                var type = new SqlParameter
                {
                    ParameterName = "ordertype",
                    Value = failedEwayparaM.ordertype == null ? DBNull.Value : (object)failedEwayparaM.ordertype
                };

                var StartDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = failedEwayparaM.Startdate == null ? DBNull.Value : (object)failedEwayparaM.Startdate
                };

                var EndDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = failedEwayparaM.EndDate == null ? DBNull.Value : (object)failedEwayparaM.EndDate
                };
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_FailedEwayBill]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(ciid);
                cmd.Parameters.Add(WarehouseidParam);
                cmd.Parameters.Add(orderidss);
                cmd.Parameters.Add(type);
                cmd.Parameters.Add(StartDateParam);
                cmd.Parameters.Add(EndDateParam);
                cmd.Parameters.Add(new SqlParameter("@type", failedEwayparaM.type));
                cmd.Parameters.Add(new SqlParameter("@skip", failedEwayparaM.skip));
                cmd.Parameters.Add(new SqlParameter("@take", failedEwayparaM.take));

                // Run the sproc
                var reader = cmd.ExecuteReader();
                EwayDc = ((IObjectContextAdapter)context).ObjectContext.Translate<FailedEWayBillOrderDC>(reader).ToList();
                reader.NextResult();
                failedEwayTotaldatanew.failedEWayBillOrderDCs = EwayDc;
                if (reader.Read())
                {
                    failedEwayTotaldatanew.TotalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }
            }
            return failedEwayTotaldatanew;
        }

        #endregion

        #region Get EwayBillInternalTransfer
        [Route("GetEwayBillInternalTransfer")]
        [HttpGet]
        public List<EwayBillInternalTransfer> GetVantransationListExport(int Warehouseid, int TransferOrderId, string InvoiceNumber, string Startdate, string EndDate, int skip, int take)
        {
            using (var myContext = new AuthContext())
            {
                var Wid = new SqlParameter("@Warehouseid", Warehouseid);
                var TrId = new SqlParameter("@TransferOrderId", TransferOrderId);
                var INo = new SqlParameter("@InvoiceNumber", InvoiceNumber);
                var Sdate = new SqlParameter("@StartDate", Startdate);
                var Edate = new SqlParameter("@EndDate", EndDate);
                var Skip = new SqlParameter("@Skip", skip);
                var Take = new SqlParameter("@Take", take);

                var GetList = myContext.Database.SqlQuery<EwayBillInternalTransfer>("EXEC EwayBill_Internal_Transfer @Warehouseid,@TransferOrderId,@InvoiceNumber,@StartDate,@EndDate,@Skip,@Take", Wid, TrId, INo, Sdate, Edate, Skip, Take).ToList();

                if (GetList != null)
                {
                    return GetList;
                }

                return GetList;
            }
        }

        #endregion

        #region  lnternal_Transfer_new
        [Route("internaldatanew")]
        [HttpPost]
        public EwayBilldataInternalTransfer ewayBillInternalTransfernews(InternalTransferOBJ InternalTransferOBJ)
        {
            List<EwayBillInternalTransfernew> data = new List<EwayBillInternalTransfernew>();
            EwayBilldataInternalTransfer maindata = new EwayBilldataInternalTransfer();

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in InternalTransferOBJ.Warehouseid)
                {
                    if (InternalTransferOBJ.Warehouseid.Count > 0)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                }
                var WarehouseidParam = new SqlParameter("Warehouseid", IdDt);
                WarehouseidParam.SqlDbType = SqlDbType.Structured;
                WarehouseidParam.TypeName = "dbo.IntValues";

                var TransferOrderIdss = new SqlParameter
                {
                    ParameterName = "@TransferOrderId",
                    Value = InternalTransferOBJ.TransferOrderId
                };
                var InvoiceNumberss = new SqlParameter
                {
                    ParameterName = "@InvoiceNumber",
                    Value = InternalTransferOBJ.InvoiceNumber
                };

                var StartDatess = new SqlParameter
                {
                    ParameterName = "@StartDate",
                    Value = InternalTransferOBJ.Startdate == null ? DBNull.Value : (object)InternalTransferOBJ.Startdate
                };
                var EndDatess = new SqlParameter
                {
                    ParameterName = "@EndDate",
                    Value = InternalTransferOBJ.EndDate == null ? DBNull.Value : (object)InternalTransferOBJ.EndDate
                };
                var skips = new SqlParameter("@Skip", InternalTransferOBJ.skip);
                var takes = new SqlParameter("@Take", InternalTransferOBJ.take);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[EwayBillInternalTransferNew]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(WarehouseidParam);
                cmd.Parameters.Add(InvoiceNumberss);
                cmd.Parameters.Add(TransferOrderIdss);
                cmd.Parameters.Add(StartDatess);
                cmd.Parameters.Add(EndDatess);
                cmd.Parameters.Add(skips);
                cmd.Parameters.Add(takes);
                var reader = cmd.ExecuteReader();

                data = ((IObjectContextAdapter)context).ObjectContext.Translate<EwayBillInternalTransfernew>(reader).ToList();
                reader.NextResult();
                maindata.ewayBillInternalTransfernews = data;
                if (reader.Read())
                {
                    maindata.TotalRecords = Convert.ToInt32(reader["TotalRecord"]);
                }
                //maindata.TotalRecords = data.Select(x => x.total).FirstOrDefault();    
                //maindata.TotalRecords = data.Select(x => x.total).FirstOrDefault();
                return maindata;
            }
        }
        #endregion

        #region Near Expiry data
        [AllowAnonymous]
        [HttpPost]
        [Route("NearExpiry")]
        public NearExpiryDTO NearExpiry(NearExpiryObj nearExpiryObj)
        {

            List<NearExpiryDc> expirydata = new List<NearExpiryDc>();
            NearExpiryDTO nearExpiryDTO = new NearExpiryDTO();

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in nearExpiryObj.Warehouseid)
                {
                    if (nearExpiryObj.Warehouseid.Count > 0)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                }
                var WarehouseidParam = new SqlParameter("Warehouseid", IdDt);
                WarehouseidParam.SqlDbType = SqlDbType.Structured;
                WarehouseidParam.TypeName = "dbo.IntValues";
                var orderidss = new SqlParameter
                {
                    ParameterName = "@OrderId",
                    Value = nearExpiryObj.Orderid

                };
                var StartDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = nearExpiryObj.Startdate == null ? DBNull.Value : (object)nearExpiryObj.Startdate
                };
                var EndDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = nearExpiryObj.EndDate == null ? DBNull.Value : (object)nearExpiryObj.EndDate
                };
                var type = new SqlParameter
                {
                    ParameterName = "ordertype",
                    Value = nearExpiryObj.ordertype == null ? DBNull.Value : (object)nearExpiryObj.ordertype
                };
                var Showtype = new SqlParameter("type", nearExpiryObj.type);
                var skips = new SqlParameter("Skip", nearExpiryObj.skip);
                var takes = new SqlParameter("Take", nearExpiryObj.take);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[NearExpiryBills]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(type);
                cmd.Parameters.Add(WarehouseidParam);
                cmd.Parameters.Add(orderidss);
                cmd.Parameters.Add(StartDateParam);
                cmd.Parameters.Add(EndDateParam);
                cmd.Parameters.Add(skips);
                cmd.Parameters.Add(takes);
                cmd.Parameters.Add(Showtype);
                // Run the sproc
                var reader = cmd.ExecuteReader();
                expirydata = ((IObjectContextAdapter)context).ObjectContext.Translate<NearExpiryDc>(reader).ToList();
                reader.NextResult();
                nearExpiryDTO.nearExpiryDcs = expirydata;
                if (reader.Read())
                {
                    nearExpiryDTO.TotalRecord = Convert.ToInt32(reader["TotalRecord"]);
                }
                return nearExpiryDTO;
            }
        }
        #endregion

        //in notepad++
        #region Regenerate Eway bill 
        [AllowAnonymous]
        [HttpPost]
        [Route("RegenerateEwaybill")]
        public async Task<bool> RegenerateEwaybill(Ewaygenerateparam ewaygenerateparam)
        {
            bool status = false;
            AuthTokenGenerationDc authTokenGenerationDc = new AuthTokenGenerationDc();
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                EwayBillHelper ewayBillHelper = new EwayBillHelper();
                EwayBillBackendDc ewayBillBackendDc = new EwayBillBackendDc()
                {
                    TripPlannerConfirmedMasterid = 0,//Case Regenerate Eway bill
                    TransportGST = ewaygenerateparam.transportergst,
                    TransportName = ewaygenerateparam.transportername,
                    vehicleno = ewaygenerateparam.vehicleno.Trim().Replace(" ", "").ToUpper().ToString(),
                    distance= ewaygenerateparam.distance

                };
                var query = from c in context.OrderDispatchedMasters
                            join w in context.Warehouses on c.WarehouseId equals w.WarehouseId
                            where c.active == true && c.Deleted != true && c.OrderId == ewaygenerateparam.orderid
                            select new
                            {
                                GSTin = w.GSTin
                            };

                var result = query.FirstOrDefault();
                ewaygenerateparam.vehicleno = ewaygenerateparam.vehicleno.Trim().Replace(" ", "").ToUpper().ToString();
                if (ewaygenerateparam.CustomerType == 0)//B2B
                {
                    status = await ewayBillHelper.GenrateEwayBillByIRN(ewayBillBackendDc, context, userid, ewaygenerateparam.orderid);
                }
                if (ewaygenerateparam.CustomerType == 1 && result != null)//B2C
                {
                    authTokenGenerationDc = ewayBillHelper.B2CEwayBillGenerate(userid, context, result.GSTin);
                }
            }
            return status;
        }

        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("GetEWaybillPDF")]
        public async Task<EwaybillBackendResponceDc> GetEWaybillPDF(PostPDFParam postPDFParam)
        {
            EwaybillBackendResponceDc res = new EwaybillBackendResponceDc();
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                EwayBillHelper ewayBillHelper = new EwayBillHelper();
                if (postPDFParam.custometType == 1)
                {
                    postPDFParam.custometType = 0;
                }
                res = await ewayBillHelper.GetEwayBillPdf(postPDFParam, db, userid);
            }
            return res;
        }
    }
    public class Ewaygenerateparam
    {
        public int orderid { get; set; }
        public string vehicleno { get; set; }
        public string transportername { get; set; }
        public string transportergst { get; set; }
        public string transporterdocno { get; set; }
        public string transporterdocdate { get; set; }
        public int distance { get; set; }
        public int topincode { get; set; }
        public int CustomerType { get; set; }
    }

    public class AuthKeyReturn
    {
        public bool Isgenereted { get; set; }
        public string EWB_Response_authtoken { get; set; }
        public string EWB_Response_sek { get; set; }
        public string generateSecureKey { get; set; }
    }

    public class generateewaybilldataDC
    {
        public string supplyType { get; set; }
        public int subSupplyType { get; set; }
        public string fromgstin { get; set; }
        public string invoice_no { get; set; }
        public string docDate { get; set; }
        public double totInvValue { get; set; }
        public string fromTrdName { get; set; }
        public string BillingAddress { get; set; }
        public string Fromplace { get; set; }
        public int frompincode { get; set; }
        public string fromstatecode { get; set; }
        public string qtyUnit { get; set; }
        public double cessRate { get; set; }
        public string shippingaddress { get; set; }
        public string toAddr1 { get; set; }
        public string toTrdName { get; set; }
        public string toGstin { get; set; }
        public double totalValue { get; set; }
        public double sgstvalue { get; set; }
        public double cgstvalue { get; set; }
        public string StateName { get; set; }
        //28-03-2023
        public string actfromstatecode { get; set; }
        public string acttostatecode { get; set; }
        public string toplace { get; set; }
        public string tostatecode { get; set; }
        //jshfjhf
        public string DocType { get; set; }
        public string productname { get; set; }
        public string HSNCode { get; set; }
        public int quantity { get; set; }
        public double sgstRate { get; set; }
        public double cgstRate { get; set; }
        public int igstRate { get; set; }
        public int cessAdvol { get; set; }
        public double taxableAmount { get; set; }
        //public int orderid { get; set; }
        public string vehicleno { get; set; }
        public string transportername { get; set; }
        public string transportergst { get; set; }
        public string transporterdocno { get; set; }
        public string transporterdocdate { get; set; }
        public int distance { get; set; }
        public string topincode { get; set; }

        public List<EwayBillDataListnew> itemList { get; set; }
    }
    public class EwayBillDataListnew
    {
        public string productname { get; set; }
        public string HSNCode { get; set; }
        public int quantity { get; set; }
        public string qtyUnit { get; set; }
        public double sgstRate { get; set; }
        public double cgstRate { get; set; }
        public int igstRate { get; set; }
        public int cessAdvol { get; set; }
        public double taxableAmount { get; set; }

    }
    public class EwayBillConfigDC
    {
        public string Task { get; set; }
        public string URL { get; set; }
        public string Publickey { get; set; }
        public string AuthURL { get; set; }
        public string clientID { get; set; }
        public string ClintSecretID { get; set; }
        public string GSTin { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class ExtendEwayBillresponse
    {
        public string ewayBillNo { get; set; }
        public string updatedDate { get; set; }
        public string validUpto { get; set; }
    }
    public class CanceledResponse

    {
        public string ewayBillNo { get; set; }
        public DateTime cancelDate { get; set; }
    }
    //class RootObject
    //{
    //    public List<Item> response { get; set; }
    //}
    class Item
    {
        public string status { get; set; }
        public string error { get; set; }
        public string info { get; set; }
    }
    //public class ErrorDc
    //{
    //    public string errorCodes { get; set; }
    //    public string errormessage { get; set; }
    //}

    public class EwayBillApiRequest
    {
        public string action { get; set; }
        public string data { get; set; }
    }
    //public class RequestPayloadN
    //{
    //    public string Data { get; set; }
    //}
    public class ewayBill_data
    {
        public string ewayBillNo { get; set; }
        public string ewayBillDate { get; set; }
        public string validUpto { get; set; }
        public string alert { get; set; }
    }
    public class vehicledata
    {
        public string vehUpdDate { get; set; }
        public string validUpto { get; set; }
    }
    public class VehicleDc
    {
        public string transDocNo { get; set; }
        public DateTime transDocDate { get; set; }
        public int fromstate { get; set; }
        public string fromplace { get; set; }

    }
    public class EwayBillApiResponse
    {
        public int status { get; set; }
        public string alert { get; set; }
        public string data { get; set; }
        public string rek { get; set; }
    }
    public class Auth
    {
        public string Password { get; set; }
        public string App_Key { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
    }

    public class Error
    {
        public string error_code { get; set; }
        public string error_message { get; set; }
        public string error_source { get; set; }
    }

    public class EwayBillHistoryDc
    {
        public long EwbNumber { get; set; }
        public object UpdatedDate { get; set; }
        public object ValidUpto { get; set; }
        public List<Error> errors { get; set; }
    }
    public class jsondata
    {
        public string supplyType { get; set; }
        public string subSupplyType { get; set; }
        public string subSupplyDesc { get; set; }
        public string docType { get; set; }
        public string docNo { get; set; }
        public string docDate { get; set; }
        public string fromGstin { get; set; }
        public string fromTrdName { get; set; }
        public string fromAddr1 { get; set; }
        public string fromAddr2 { get; set; }
        public string fromPlace { get; set; }
        public int fromPincode { get; set; }
        public int actFromStateCode { get; set; }
        public int fromStateCode { get; set; }
        public string toGstin { get; set; }
        public string toTrdName { get; set; }
        public string toAddr1 { get; set; }
        public string toAddr2 { get; set; }
        public string toPlace { get; set; }
        public int toPincode { get; set; }
        public int actToStateCode { get; set; }
        public int toStateCode { get; set; }
        public int transactionType { get; set; }
        public double otherValue { get; set; }
        public double totalValue { get; set; }
        public double cgstValue { get; set; }
        public double sgstValue { get; set; }
        public double igstValue { get; set; }
        public double cessValue { get; set; }
        public double cessNonAdvolValue { get; set; }
        public double totInvValue { get; set; }
        public string transporterId { get; set; }
        public string transporterName { get; set; }
        public string transDocNo { get; set; }
        public int transMode { get; set; }//
        public int transDistance { get; set; }
        public string transDocDate { get; set; }
        public string vehicleNo { get; set; }
        public string vehicleType { get; set; }
        public List<Itemlist> itemList { get; set; }

    }

    public class OrderTypeDC
    {
        public int Id { get; set; }
        public string OrderType { get; set; }
    }

    public class NearExpiryObj
    {
        public List<int> Warehouseid { get; set; }
        public int Orderid { get; set; }
        public string Startdate { get; set; }
        public string EndDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public string ordertype { get; set; }
        public int type { get; set; }
    }

    public class NearExpiryDTO
    {
        public int TotalRecord { get; set; }
        public List<NearExpiryDc> nearExpiryDcs { get; set; }
    }

    public class NearExpiryDc
    {
        public long EwayBillId { get; set; }
        public int OrderId { get; set; }
        public double OrderAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderStatus { get; set; }

        public string EwaybillNumber { get; set; }
        public DateTime? EwayBillDate { get; set; }
        public DateTime? EwayBillValidTill { get; set; }
        public double? Distance { get; set; }
        public string EwayBillStatus { get; set; }
        public int CustomerType { get; set; }
        public string CustomerTypeName { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
    }
    public class FailedEwayparam
    {
        public List<int> cityid { get; set; }
        public List<int> warehouseId { get; set; }
        public int orderid { get; set; }
        public string ordertype { get; set; }
        public string Startdate { get; set; }
        public string EndDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public int type { get; set; }

    }
    public class EwayTotaldata
    {
        public int TotalRecord { get; set; }
        public List<EWayBillOrderDC> getEwaydata { get; set; }
    }
    public class UpdateVehicleDC
    {
        public int ewbNo { get; set; }
        public string vehicleNo { get; set; }
        public int reasonCode { get; set; }
        public string reasonRem { get; set; }
    }

    public class FailedEwayTotaldata
    {
        public int TotalRecords { get; set; }
        public List<FailedEWayBillOrderDC> failedEWayBillOrderDCs { get; set; }
    }

    public class FailedEWayBillOrderDC
    {
        public int OrderId { get; set; }
        public double orderamount { get; set; }
        public string InvoiceNo { get; set; }
        public string OrderStatus { get; set; }
        public string OrderType { get; set; }
        public string EwayBillNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EwayBillDate { get; set; }
        public DateTime? EwayBillValidTill { get; set; }
        public string err { get; set; }
        //public int TotalRecords { get; set; }
        public string CustomerTypeName { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
    }
    public class EWayBillOrderDC
    {
        public long? EwayBillId { get; set; }
        public int OrderId { get; set; }
        //public int OrderDispatchedMasterId { get; set; }
        public double orderamount { get; set; }
        public string InvoiceNo { get; set; }
        public string OrderStatus { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string EwayBillNumber { get; set; }
        public DateTime? EwayBillDate { get; set; }
        public DateTime CreatedDate { get; set; }
        // public string IRNStatus { get; set; }
        public DateTime? EwayBillValidTill { get; set; }
        //public int Stateid { get; set; }
        //public string StateName { get; set; }
        //public bool flag { get; set; }
        public int CustomerType { get; set; }
        public string CustomerTypeName { get; set; }//B2B=0,B2C=1
        public int totalRecord { get; set; }
        public bool IsExtendEwayBill { get; set; }
        public string Distance { get; set; }
        public bool IsCancelEwayBill { get; set; }

        //field added for exports
        //public int CustomerId { get; set; }
        public string RefNo { get; set; }
        //public double InterstateAmount { get; set; }
        //public double IntrastateAmount { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime? EwayBillCancelDate { get; set; }
    }

    public class Ewayparam
    {
        public List<int> cityid { get; set; }
        public List<int> warehouseid { get; set; }
        public int orderid { get; set; }
        public string skcode { get; set; }
        public string Status { get; set; }
        public string startdate { get; set; }
        public string EndDate { get; set; }

        public int skip { get; set; }
        public int take { get; set; }
    }

    public class EwayBillInternalTransfer
    {
        public int TransferOrderId { get; set; }
        public double PriceofItem { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderStatus { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
        public string EwaybillNumber { get; set; }
        public DateTime EwayBillGenerate { get; set; }

    }


    public class EwayBilldataInternalTransfer
    {
        public int TotalRecords { get; set; }
        public List<EwayBillInternalTransfernew> ewayBillInternalTransfernews { get; set; }
    }
    public class InternalTransferOBJ
    {
        public List<int> Warehouseid { get; set; }
        public string EndDate { get; set; }
        public string Startdate { get; set; }
        public string InvoiceNumber { get; set; }
        public int TransferOrderId { get; set; }
        public int take { get; set; }
        public int skip { get; set; }

    }
    public class OrderPageHistoryDataDC
    {
        //public string UpdatedByUser { get; set; }
        //public DateTime UpdateDateTime { get; set; }
        public int OrderID { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        //public string InvoiceAmt { get; set; }
        public string InvoiceNo { get; set; }
        //public DateTime DistatchDate { get; set; }
        //public DateTime ShippedDate { get; set; }
        //public DateTime DeliveredDate { get; set; }
        public string Warehouse { get; set; }
        public string CustomerType { get; set; }
        public string IRNNumber { get; set; }
        //public string EwayBillNo { get; set; }
        //public string EwayBillStatus { get; set; }

    }
    public class ITHistoryDataDC  //aartimukati
    {
        //public string UpdatedByUser { get; set; }
        //public DateTime UpdateDateTime { get; set; }
        public int TransferOrderID { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        //public string VehicleNo { get; set; }
        //public string InternalTransferInvoiceNo { get; set; }
        //public DateTime ITInvoiceCreatedDate { get; set; }
        //public string InternalTransferNoCN { get; set; }
        //public DateTime DeliveryChallanCreatedDate { get; set; }
        //public string IRNNumber { get; set; }
        //public string EwayBillNo { get; set; }
        //public string EwayBillStatus { get; set; }
    }
    public class EwayBillInternalTransfernew
    {
        public int TransferOrderId { get; set; }//
        public int TransferDispatchedOrderId { get; set; }//
        public double OrderAmount { get; set; }//
        public string InvoiceNumber { get; set; }//
        public string OrderStatus { get; set; }//
        public string FromWarehouse { get; set; }//
        public string RequestToWarehouseName { get; set; }//
        public string EwaybillNumber { get; set; }
        public DateTime? EwayBillDate { get; set; }
        public DateTime? EwayBillValidTill { get; set; }
        public int total { get; set; }
    }

}



