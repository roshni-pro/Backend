using AngularJSAuthentication.API.ControllerV1;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/RTGSOrdersApprove")]
    public class RTGSOrdersApproveController : ApiController
    {


        [Route("GetRTGSOrderList")]
        [HttpPost]
        [AllowAnonymous]
        public PaggingData GetRTGSOrderList(RTGSOrderDetailsDC item)
        {
            PaggingData paggingData = new PaggingData();
            try
            {
                int skip = (item.Skip - 1) * item.Take;

                using (var context = new AuthContext())
                {
                    string query = "exec GetRTGSPayOrderDetails @RTGSNo,@type,@skCode,@PaymentFrom,@WarehouseID,@Skip,@Take";

                    List<object> parameters = new List<object>();

                    DataTable dtWH = new DataTable();
                    dtWH.Columns.Add("IntValue");
                    item.WarehouseIds.ForEach(x =>
                    {
                        DataRow dr = dtWH.NewRow();
                        dr["IntValue"] = x;
                        dtWH.Rows.Add(dr);
                    });
                    if (string.IsNullOrEmpty(item.RefNo))
                    {
                        item.RefNo = "";
                    }
                    if (string.IsNullOrEmpty(item.Skcode))
                    {
                        item.Skcode = "";
                    }
                    var agentIdParam = new SqlParameter
                    {
                        ParameterName = "@RTGSNo",
                        Value = item?.RefNo
                    };
                    parameters.Add(agentIdParam);
                    var typeParam = new SqlParameter
                    {
                        ParameterName = "@type",
                        Value = item.type
                    };
                    parameters.Add(typeParam);
                    var CustomerIdParam = new SqlParameter
                    {
                        ParameterName = "@skCode",
                        Value = item?.Skcode
                    };
                    parameters.Add(CustomerIdParam);
                    var SkipParam = new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = skip
                    };
                    parameters.Add(SkipParam);
                    var TakeParam = new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = item.Take
                    };
                    parameters.Add(TakeParam);
                    var WarehouseID = new SqlParameter

                    {
                        TypeName = "dbo.intvalues",
                        ParameterName = "@WarehouseID",
                        Value = dtWH
                    };
                    parameters.Add(WarehouseID);
                    var PaymentFromParam = new SqlParameter
                    {
                        ParameterName = "@PaymentFrom",
                        Value = item.PaymentFrom
                    };
                    parameters.Add(PaymentFromParam);
                    List<RTGSOrderDetailsDC> assignmentlist = context.Database.SqlQuery<RTGSOrderDetailsDC>(query, parameters.ToArray()).ToList();

                    paggingData.ordermaster = assignmentlist;
                    paggingData.total_count = assignmentlist.Select(x => x.TotalRecords).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                paggingData.ordermaster = null;
                paggingData.total_count = 0;
            }

            return paggingData;
        }

        [Route("GetRTGSOrderIdWise")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetRTGSOrderIdWise(string Refno, int type, string PaymentFrom)
        {
            try
            {

                using (var context = new AuthContext())
                {
                    string query = "exec GetRTGSPayOrderIdWise @RefNo,@Type,@PaymentFrom";

                    var RefNoParam = new SqlParameter
                    {
                        ParameterName = "@RefNo",
                        Value = Refno
                    };
                    var Typeparam = new SqlParameter
                    {
                        ParameterName = "@Type",
                        Value = type
                    };
                    var PaymentFromparam = new SqlParameter
                    {
                        ParameterName = "@PaymentFrom",
                        Value = PaymentFrom
                    };

                    List<RTGSOrderDetailsDC> Resultlist = context.Database.SqlQuery<RTGSOrderDetailsDC>(query, RefNoParam, Typeparam, PaymentFromparam).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, Resultlist);
                }
            }
            catch (Exception ee)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ee.Message.ToString());
            }
        }

        [Route("UpdateRTGSRefNo")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateRTGSRefNo(RTGSpostDC rTGSpostDC)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                try                    
                {
                    var Ordermaster = context.OrderDispatchedMasters.Where(x => x.OrderId == rTGSpostDC.OrderId).FirstOrDefault();
                    if (Ordermaster != null)
                    {
                        DeliveryTaskController deliveryTaskController = new DeliveryTaskController();
                        bool response = await deliveryTaskController.VerifyAssignmentRefNo(Ordermaster.DeliveryIssuanceIdOrderDeliveryMaster??0, rTGSpostDC.NewRefNo, rTGSpostDC.OrderId);
                        if (!response)
                        {
                            string query = "exec updateRTGSReferenceNo @RefNo,@NewRefNo ,@paymentMode ,@OrderId ";
                            List<object> parameters = new List<object>();


                            var refnoparam = new SqlParameter
                            {
                                ParameterName = "@RefNo",
                                Value = rTGSpostDC.RefNo
                            };
                            parameters.Add(refnoparam);


                            var paymodeparam = new SqlParameter
                            {
                                ParameterName = "@paymentMode",
                                Value = rTGSpostDC.PayMode
                            };
                            parameters.Add(paymodeparam);

                            var NewRTGSNoParam = new SqlParameter
                            {
                                ParameterName = "@NewRefNo",
                                Value = rTGSpostDC.NewRefNo
                            };
                            parameters.Add(NewRTGSNoParam);
                            var OrderIdParam = new SqlParameter
                            {
                                ParameterName = "@OrderId",
                                Value = rTGSpostDC.OrderId
                            };
                            parameters.Add(OrderIdParam);

                            var rowsaffected = context.Database.ExecuteSqlCommand(query, parameters.ToArray());
                            if (rowsaffected > 0)
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return result;
                }
            }
            return result;
        }


        [Route("GetRTGSOrderList1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage GetRTGSOrderList1(RTGSOrderDetailsDC item)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    item.Skip = 0;
                    item.Take = 20;
                    string query = "exec spGetRTGSOrderDetails @RTGSNo,@type,@CustomerId,@OrderId,@PaymentFrom,@Skip,@Take";

                    List<object> parameters = new List<object>();
                    var agentIdParam = new SqlParameter
                    {
                        ParameterName = "@RTGSNo",
                        Value = item.RefNo
                    };
                    parameters.Add(agentIdParam);
                    var typeParam = new SqlParameter
                    {
                        ParameterName = "@type",
                        Value = item.type
                    };
                    parameters.Add(typeParam);
                    var CustomerIdParam = new SqlParameter
                    {
                        ParameterName = "@CustomerId",
                        Value = item.CustomerId
                    };
                    parameters.Add(CustomerIdParam);
                    var SkipParam = new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = item.Skip
                    };
                    parameters.Add(SkipParam);
                    var TakeParam = new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = item.Take
                    };
                    parameters.Add(TakeParam);
                    var OrderIdParam = new SqlParameter
                    {
                        ParameterName = "@OrderId",
                        Value = item.OrderId
                    };
                    parameters.Add(OrderIdParam);
                    var PaymentFromParam = new SqlParameter
                    {
                        ParameterName = "@PaymentFrom",
                        Value = item.PaymentFrom
                    };
                    parameters.Add(PaymentFromParam);
                    List<RTGSOrderDetailsDC> assignmentlist = context.Database.SqlQuery<RTGSOrderDetailsDC>(query, parameters.ToArray()).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, assignmentlist);
                }
            }
            catch (Exception ee)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ee.Message.ToString());
            }
        }
        [Route("ApproveRTGSNumber")]
        [HttpGet]
        [AllowAnonymous]
        public bool ApproveRTGSNumber(string RTGSNo)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    string query = "exec ApproveRTGSNumbers @RTGSno,@ApproveBy ";
                    List<object> parameters = new List<object>();
                    var RTGSNoParam = new SqlParameter
                    {
                        ParameterName = "@RTGSno",
                        Value = RTGSNo
                    };
                    parameters.Add(RTGSNoParam);
                    var NewRTGSNoParam = new SqlParameter
                    {
                        ParameterName = "@ApproveBy",
                        Value = userid
                    };
                    parameters.Add(NewRTGSNoParam);
                    var rowsaffected = context.Database.ExecuteSqlCommand(query, parameters.ToArray());
                    if (rowsaffected > 0)
                    {
                        result = true;
                    }
                    #region AutoSettleOrderData


                    var ReferencenoParam = new SqlParameter
                    {
                        ParameterName = "@Refrencenumber",
                        Value = RTGSNo
                    };



                    var data = context.Database.SqlQuery<PaymentModeData>("exec [GetPaymentMode] @Refrencenumber ", ReferencenoParam).ToList();


                    if (data != null)
                    {
                        var itemRtgs = data.Any(x => (x.IsApproved == 0));
                        if (!itemRtgs)
                        {


                            foreach (var ptr in data)
                            {
                                var AssingStatus = (from A in context.DeliveryIssuanceDb
                                                    join D in context.OrderDispatchedMasters
                                                 on A.DeliveryIssuanceId equals D.DeliveryIssuanceIdOrderDeliveryMaster
                                                    where D.OrderId == ptr.OrderId
                                                    select new
                                                    {
                                                        A.Status,
                                                        D.CustomerId
                                                    }).FirstOrDefault();
                                if (AssingStatus.Status == "Freezed")
                                {
                                    if (context.AutoSettleOrderDetailDB.FirstOrDefault(z => z.OrderId == ptr.OrderId) == null)
                                    {
                                        AutoSettleOrderDetail autoSettleOrderDetail = new AutoSettleOrderDetail();
                                        autoSettleOrderDetail.OrderId = ptr.OrderId;
                                        autoSettleOrderDetail.IsProcess = false;
                                        autoSettleOrderDetail.RetryCount = 0;
                                        autoSettleOrderDetail.IsActive = true;
                                        autoSettleOrderDetail.InsertedDate = DateTime.Now;
                                        autoSettleOrderDetail.CustomerId = AssingStatus.CustomerId;

                                        context.AutoSettleOrderDetailDB.Add(autoSettleOrderDetail);
                                        context.Commit();
                                        //OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                        //Opdl.OrderId = item.OrderId;
                                        //Opdl.IsPaymentSuccess = true;
                                        //Opdl.IsLedgerAffected = "Yes";
                                        //Opdl.PaymentDate = DateTime.Now;
                                        //Opdl.IsActive = true;
                                        //Opdl.CustomerId = q.CustomerId;
                                        //context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                    }
                                }
                            }
                        }


                    }
                    else
                    {
                        AutoSettle(RTGSNo, context);
                    }




                    #endregion

                }
                catch (Exception ex)
                {
                    return result;
                }
            }
            return result;
        }

        private void AutoSettle(string RTGSNo, AuthContext context)
        {
            var PaymentResponseTb = context.PaymentResponseRetailerAppDb.Where(x => x.GatewayTransId == RTGSNo && x.IsApproved == 1
                                                            && x.status == "Success" && (x.PaymentFrom.ToUpper() == "MPOS" || x.PaymentFrom.ToUpper() == "RTGS/NEFT")).ToList();



            foreach (var ptr in PaymentResponseTb)
            {
                var AssingStatus = (from A in context.DeliveryIssuanceDb
                                    join D in context.OrderDispatchedMasters
                                 on A.DeliveryIssuanceId equals D.DeliveryIssuanceIdOrderDeliveryMaster
                                    where D.OrderId == ptr.OrderId
                                    select new
                                    {
                                        A.Status,
                                        D.CustomerId
                                    }).FirstOrDefault();
                if (AssingStatus.Status == "Freezed")
                {
                    if (context.AutoSettleOrderDetailDB.FirstOrDefault(z => z.OrderId == ptr.OrderId) == null)
                    {
                        AutoSettleOrderDetail autoSettleOrderDetail = new AutoSettleOrderDetail();
                        autoSettleOrderDetail.OrderId = ptr.OrderId;
                        autoSettleOrderDetail.IsProcess = false;
                        autoSettleOrderDetail.RetryCount = 0;
                        autoSettleOrderDetail.IsActive = true;
                        autoSettleOrderDetail.InsertedDate = DateTime.Now;
                        autoSettleOrderDetail.CustomerId = AssingStatus.CustomerId;

                        context.AutoSettleOrderDetailDB.Add(autoSettleOrderDetail);
                        context.Commit();
                        //OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                        //Opdl.OrderId = item.OrderId;
                        //Opdl.IsPaymentSuccess = true;
                        //Opdl.IsLedgerAffected = "Yes";
                        //Opdl.PaymentDate = DateTime.Now;
                        //Opdl.IsActive = true;
                        //Opdl.CustomerId = q.CustomerId;
                        //context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                    }
                }
            }
        }

        [Route("GetApproverName")]
        [HttpGet]
        [AllowAnonymous]
        public List<int> Peoplename(string keyword)
        {
            List<int> list = new List<int>();
            using (var context = new AuthContext())
            {
                string query = "select PeopleID from People where DisplayName like '%'+@SearchText + '%'";
                var keywordParam = new SqlParameter
                {
                    ParameterName = "@SearchText",
                    Value = keyword
                };
                list = context.Database.SqlQuery<int>(query, keywordParam).ToList();

            }
            return list;
        }
        [Route("GetCustomerid")]
        [HttpGet]
        [AllowAnonymous]
        public List<CustDC> GetCustomerId(string keyword)
        {
            List<CustDC> list = new List<CustDC>();
            using (var context = new AuthContext())
            {
                string query = "select Cust.Name +'-'+Cust.Skcode  as CustomerName, Cust.CustomerId from Customers (nolock) Cust inner join OrderDispatchedMasters OD on Cust.CustomerId =Od.CustomerId where Cust.Skcode like  '%'+@keyword+ '%' OR OrderId like  '%'+@keyword+ '%' or Cust.Mobile like '%'+@keyword+ '%' or Cust.Name like '%'+@keyword+ '%' group by Cust.CustomerId,Cust.Name +'-'+Cust.Skcode";
                var keywordParam = new SqlParameter
                {
                    ParameterName = "@keyword",
                    Value = keyword
                };
                list = context.Database.SqlQuery<CustDC>(query, keywordParam).ToList();
            }
            return list;
        }

        [Route("GenExcel")]
        [HttpPost]
        [AllowAnonymous]
        public string GenExcel(RTGSOrderDetailsDC item)
        {
            string result = "";
            List<ExcelData> data = new List<ExcelData>();
            var list = GetRTGSOrderList(item).ordermaster;
            foreach (var i in list)
            {
                ExcelData excelData = new ExcelData()
                {
                    amount = i.amount,
                    PaymentFrom = i.PaymentFrom,
                    RefNo = i.RefNo,
                    WarehouseName = i.WarehouseName
                };
                data.Add(excelData);
            }
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            var fileName = "IRNGenerateFilebyClearTax.xlsx";
            string filePath = ExcelSavePath + fileName;
            var returnPath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port
                                                            , string.Format("ExcelGeneratePath/{0}", fileName));

            var dataTables = new List<DataTable>();
            var IRNExcelDC = ClassToDataTable.CreateDataTable(data);
            IRNExcelDC.TableName = "genIRNExcelDC";
            dataTables.Add(IRNExcelDC);
            if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
            {
                result = returnPath;
            }
            return result;
        }

        public class CustDC
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
        }
        public class ExcelData
        {
            public string RefNo { get; set; }
            public double amount { get; set; }
            public string WarehouseName { get; set; }
            public string PaymentFrom { get; set; }

        }

    }
    public class PaymentModeData
    {
        public int OrderId { get; set; }
        public string PaymentFrom { get; set; }
        public int IsApproved { get; set; }
    }
    public class RTGSpostDC
    {
        public string NewRefNo { get; set; }
        public int OrderId { get; set; }
        public string PayMode { get; set; }
        public string RefNo { get; set; }
    }
}

