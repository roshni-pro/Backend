using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.Accounts;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.Agentcommision;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class AgentCommissionHelper
    {
        //private AgentCommissionPayment agentCommissionPayment { get; set; }
        //private List<AssignmentCommission> assignmentCommissionList { get; set; }
        public ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> AddPaymentSettlement(List<AgentPaymentSettlementDc> agentPaymentSettlementList, int userId)
        {
            ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> response= null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (var scope = new TransactionScope())
            {
                using (var context = new AuthContext())
                {
                    DateTime currentDateTime = DateTime.Now;
                    int agentCommissionPaymentId = agentPaymentSettlementList.First().AgentCommissionPaymentId;
                    AgentCommissionPayment agentCommissionPayment
                        = context.AgentCommissionPaymentDB.FirstOrDefault(x => x.Id == agentCommissionPaymentId);



                    List<long> AssignmentCommissionIdList = agentPaymentSettlementList.Select(x => (long)x.AssignmentCommissionId).ToList();


                    List<AssignmentCommission> assignmentCommissionList
                        = context.AssignmentCommissionDb.Where(x => AssignmentCommissionIdList.Any(y => y == x.Id)).ToList();


                    response = IsPaymentHaveAnyIssue(agentPaymentSettlementList, assignmentCommissionList, agentCommissionPayment);
                    if (response.IsSuccess)
                    {
                        List<AgentPaymentSettlement> agentPaymentSettlementlist = new List<AgentPaymentSettlement>();
                        //Guid obj = Guid.NewGuid();
                        foreach (var settlement in agentPaymentSettlementList)
                        {
                            AgentPaymentSettlement agentPaymentSettlement = new AgentPaymentSettlement();
                            agentPaymentSettlement.AgentCommissionPaymentId = settlement.AgentCommissionPaymentId;
                            agentPaymentSettlement.AssignmentCommissionId = settlement.AssignmentCommissionId;
                            agentPaymentSettlement.IsActive = true;
                            agentPaymentSettlement.IsDeleted = false;
                            agentPaymentSettlement.SettleDate = settlement.SettleDate;
                            agentPaymentSettlement.Amount = settlement.Amount;
                            agentPaymentSettlement.CreatedDate = currentDateTime;
                            agentPaymentSettlement.CreatedBy = userId;
                            //agentPaymentSettlement.GUID = obj;
                            agentPaymentSettlementlist.Add(agentPaymentSettlement);

                            AssignmentCommission assignmentCommission
                                = assignmentCommissionList.First(x => x.Id == agentPaymentSettlement.AssignmentCommissionId);
                            assignmentCommission.PaidAmount += agentPaymentSettlement.Amount;
                            if(assignmentCommission.PaidAmount == assignmentCommission.CommissionAmount)
                            {
                                assignmentCommission.Status = "Settled";
                            }
                            else
                            {
                                assignmentCommission.Status = "Partially Settled";
                            }
                            
                            assignmentCommission.ModifiedBy = userId;
                            assignmentCommission.ModifiedDate = currentDateTime;
                            //context.Commit();

                        }
                        context.AgentCommissionPaymentSettlementDB.AddRange(agentPaymentSettlementlist);
                        agentCommissionPayment.SettledAmount = agentCommissionPayment.SettledAmount + (int) agentPaymentSettlementlist.Sum(x => x.Amount);
                        if (agentCommissionPayment.SettledAmount == agentCommissionPayment.Amount)
                        {
                            agentCommissionPayment.SettledStatus = "Settled";
                        }
                        else
                        {
                            agentCommissionPayment.SettledStatus = "Partially Settled";
                        }


                        

                       


                        response.OutputList = agentPaymentSettlementlist;
                    }

                    context.Commit();

                }

                scope.Complete();
            }
            return response;
        }




        private bool UpdateAgentCommission(AgentPaymentSettlementDc agentPaymentSettlementDc, AuthContext context)
        {
            AgentCommissionPayment agentCommissionPayment = context.AgentCommissionPaymentDB.Where(x => x.Id == agentPaymentSettlementDc.AssignmentCommissionId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            agentCommissionPayment.SettledStatus = agentPaymentSettlementDc.Status;
            agentCommissionPayment.Amount = agentPaymentSettlementDc.Amount;
            context.Entry(agentCommissionPayment).State = EntityState.Modified;
            if (context.Commit() > 0)
            {

                return true;
            }
            return false;

        }
        private ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> IsPaymentHaveAnyIssue(List<AgentPaymentSettlementDc> agentPaymentSettlementList, List<AssignmentCommission> assignmentCommissionList, AgentCommissionPayment agentCommissionPayment)
        {
            ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> response
                = new ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc>();


            response.IsSuccess = true;
            response.ErrorMessage = "";


            if (agentPaymentSettlementList != null && agentPaymentSettlementList.Any())
            {

                foreach (AgentPaymentSettlementDc item in agentPaymentSettlementList)
                {
                    AssignmentCommission commission = assignmentCommissionList.FirstOrDefault(x => x.Id == item.AssignmentCommissionId);
                    if(((int)commission.CommissionAmount.Value - commission.PaidAmount.Value) < (int)item.Amount)
                    {
                        response.ErrorMessage = "Issue in assignment " + commission.AssignmentID.ToString() + ", paying more amount than remaining";
                        response.IsSuccess = false;
                        break;
                    }
                }
            }

            if(agentPaymentSettlementList.Sum(x => x.Amount) > (agentCommissionPayment.Amount - agentCommissionPayment.SettledAmount) )
            {
                response.ErrorMessage = "Issue in total payment , that is less than settled amount" ;
                response.IsSuccess = false;
            }
            return response;
        }

    }
}