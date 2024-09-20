using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/AssignmentPayment")]
    public class AssignmentPaymentController : ApiController
    {
        #region get Assignment List   
        /// <summary>
        /// Created date 16/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("GetAssignment")]
        [HttpPost]
        public List<AssignmentListDC> GetAssignmentList(OldAssignmentPaymentPager pager)
        {
            using (var context = new AuthContext())
            {

                string query = "exec OldAssignemntGet @AgentId, @DeliveryIssuanceId, @Paymentstatus, @StartDate ,@EndDate";
                List<object> parameters = new List<object>();
                var agentIdParam = new SqlParameter
                {
                    ParameterName = "AgentId",
                    Value = pager.AgentId
                };
                parameters.Add(agentIdParam);


                var deliveryIssuranceIDParam = new SqlParameter
                {
                    ParameterName = "DeliveryIssuanceId",
                    Value = pager.DeliveryIssuanceId ?? 0
                };
                parameters.Add(deliveryIssuranceIDParam);


                var paymentStatusParam = new SqlParameter
                {
                    ParameterName = "Paymentstatus",
                    Value = pager.paymentstatus ?? "Unpaid"
                };
                parameters.Add(paymentStatusParam);


                //pager.StartDate = TimeZoneInfo.ConvertTimeFromUtc(pager.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                //pager.StartDate = pager.StartDate.Value.Date;

                var startDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = pager.StartDate ?? (object)DBNull.Value
                };
                parameters.Add(startDateParam);

                //pager.EndDate = TimeZoneInfo.ConvertTimeFromUtc(pager.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                //pager.EndDate = pager.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                var endDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = pager.EndDate ?? (object)DBNull.Value
                };
                parameters.Add(endDateParam);

                List<AssignmentListDC> assignmentlist = context.Database.SqlQuery<AssignmentListDC>(query, parameters.ToArray()).ToList();
                return assignmentlist;
            }
        }
        #endregion

        #region get Order List   
        /// <summary>
        /// Created date 16/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Route("GetOrderList")]
        [HttpGet]
        public List<AssignmentOrderListPayDC> GetOrderIdList(int DeliveryIssuanceId)
        {
            using (var context = new AuthContext())
            {

                var param = new SqlParameter("DeliveryIssuanceId", DeliveryIssuanceId);
                List<AssignmentOrderListPayDC> orderList = context.Database.SqlQuery<AssignmentOrderListPayDC>("exec OldAssignemntOrderPaymentGet @DeliveryIssuanceId", param).ToList();
                OldAssignmentPayDC oldAssignmentPayDC = new OldAssignmentPayDC();



                return orderList;
            }
        }
        #endregion

        #region save Assignment payment details and update ledger entry    
        /// <summary>
        /// Created date 16/11/2019
        /// Created by raj
        /// </summary>
        /// <param name="">detailsdata</param>
        /// <returns>detailsdata</returns>
        [Route("assignmentpayment")]
        [HttpPost]
        public OldAssignmentPayDC GetOrderIdList(OldAssignmentPayDC detailsdata)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, option))
           // using (var transactionScope = new TransactionScope())
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int userid = 0;
                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                        AssignmentpaymentHelper assignmentpayment = new AssignmentpaymentHelper();
                        assignmentpayment.AssignmentPaymentupdate(detailsdata, context, userid);
                        transactionScope.Complete();
                        return detailsdata;
                    }
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    throw ex;
                }
            }

        }
        #endregion
    }

    public class OldAssignmentPaymentPager
    {
        public int AgentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? DeliveryIssuanceId { get; set; }
        public string paymentstatus { get; set; }
    }
}
