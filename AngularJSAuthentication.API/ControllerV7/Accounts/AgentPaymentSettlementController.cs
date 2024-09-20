using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.Accounts;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.Agentcommision;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Accounts
{
    [RoutePrefix("api/AgentPaymentSettlement")]
    public class AgentPaymentSettlementController : BaseApiController
    {
        [Route("GetAgentCommissionPayments/{agentId}")]
        [HttpGet]
        public List<AgentCommissionPaymentDc> GetAgentCommissionPayments(int agentId)
        {
            using (var authContext = new AuthContext())
            {
                var agentPaymentList = authContext.AgentCommissionPaymentDB.ToList();


                var query =
                from ac in authContext.AgentCommissionPaymentDB
                where (ac.AgentId == agentId && ((ac.SettledStatus == "UnSettled" || ac.SettledStatus == "Partially Settled") && ac.Amount > ac.SettledAmount))
                select new AgentCommissionPaymentDc()
                {
                    Id = ac.Id,
                    Narration = ac.Narration,
                    Status = ac.Status,
                    RefNumber = ac.RefNumber,
                    AgentId = ac.AgentId,
                    BankLedgerId = ac.BankLedgerId,
                    PaymentDate = ac.PaymentDate,
                    Amount = ac.Amount,
                    SettledStatus = ac.SettledStatus,
                    SettledAmount = ac.SettledAmount
                };

                return query.OrderByDescending(x => x.Id).ToList();
            }

        }


        [Route("GetAssignmentCommissions/agentId/{agentId}/skip/{skip}/take/{take}")]
        [HttpGet]
        public AssignmentCommissionPager GetAssignmentCommissions(int agentId, int skip, int take)
        {
            AssignmentCommissionPager acPager = new AssignmentCommissionPager();

            using (var authContext = new AuthContext())
            {
                var AssignmentCommission = authContext.AssignmentCommissionDb.ToList();

                var query =
                 from ac in authContext.AssignmentCommissionDb
                 where (ac.AgentID == agentId && (ac.Status == "UnSettled" || ac.Status == "Partially Settled") && ac.IsActive == true && ac.IsDeleted == false)
                 select new AssignmentCommissionDc()
                 {
                     Id = ac.Id,
                     AssignmentID = ac.AssignmentID,
                     AgentID = ac.AgentID,
                     CommissionAmount = ac.CommissionAmount,
                     TDSAmount = ac.TDSAmount,
                     PaidAmount = ac.PaidAmount,
                     FreezeDate = ac.FreezeDate,
                     Status = ac.Status
                 };

                acPager.NetRecords = query.Count();
                acPager.AssignmentCommissionList = query.OrderByDescending(x => x.AssignmentID).Skip(skip).Take(take).ToList();

            }
            return acPager;
        }


        [Route("SaveAgentPaymentSettlements")]
        [HttpPost]

        public HttpResponseMessage SaveAgentPaymentSettlements(List<AgentPaymentSettlement> agentPaymentSettlementList)

        {
            using (var context = new AuthContext())
            {
                context.AgentCommissionPaymentSettlementDB.AddRange(agentPaymentSettlementList);
                context.Commit();
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Saved");
        }

        [Route("AddPaymentandUpdate")]
        [HttpPost]

        public ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> AddPaymentandUpdate(List<AgentPaymentSettlementDc> agentPaymentSettlementDc)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                AgentCommissionHelper agentCommissionHelper = new AgentCommissionHelper();

                ResponseWrapper<AgentPaymentSettlement, AgentPaymentSettlementDc> result = agentCommissionHelper.AddPaymentSettlement(agentPaymentSettlementDc, userid);

                return result;
            }

        }


        [Route("GetAssignmentCommissionDetails/{assignmentId}")]
        [HttpGet]
        public IHttpActionResult GetAssignmentCommissionDetails(int assignmentId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from acd in authContext.AssignmentCommissionDetailDb
                            join itm in authContext.itemMasters
                            on acd.ItemId equals itm.ItemId
                            where acd.AssignmentID == assignmentId
                            select new
                            {
                                ItemId = acd.ItemId,
                                OrderId = acd.OrderId,
                                SubsubCategoryId = acd.SubsubCategoryId,
                                TdsAmount = acd.TdsAmount,
                                TotalAmount = acd.TotalAmount,
                                AssignmentID = acd.AssignmentID,
                                CommissionAmount = acd.CommissionAmount,
                                CommissionPercentage = acd.CommissionPercentage,
                                ItemName = itm.itemname,
                                ItemNumber = itm.Number,
                                ItemMultiMRPId = itm.ItemMultiMRPId
                            };


                return Ok(query.ToList());
            }
        }

        [Route("GetAgentPaymentSettlementList/skip/{skip}/take/{take}")]
        [HttpGet]
        public AgentPaymentSettlementPager GetAgentPaymentSettlementList(int skip, int take)
        {
            AgentPaymentSettlementPager acPager = new AgentPaymentSettlementPager();

            using (var authContext = new AuthContext())
            {
                var query = from lb in authContext.AgentCommissionPaymentSettlementDB
                            join commission in authContext.AssignmentCommissionDb
                            on lb.AssignmentCommissionId equals commission.Id
                            join ppl in authContext.Peoples
                            on commission.AgentID equals ppl.PeopleID
                            where (lb.IsActive == true && lb.IsDeleted == false)
                            select new AgentPaymentSettlementDisplayDc()
                            {
                                Id = lb.Id,
                                Amount = lb.Amount,
                                SettleDate = lb.SettleDate,
                                Status = commission.Status,
                                AgentCommissionPaymentId = lb.AgentCommissionPaymentId,
                                AssignmentCommissionId = lb.AssignmentCommissionId,
                                AgentName = ppl.DisplayName,
                                AssignmentId = commission.AssignmentID

                            };
                acPager.NetRecords = query.Count();
                acPager.AgentSettlementList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                return acPager;
            }
        }


        [Route("GetAgentPaymentSettlementListByName/name/{name}/status/{status}/skip/{skip}/take/{take}")]
        [HttpGet]
        public AgentPaymentSettlementPager GetAgentPaymentSettlementListByName(string name, string status, int skip, int take)
        {
            AgentPaymentSettlementPager acPager = new AgentPaymentSettlementPager();
            using (var authContext = new AuthContext())
            {
                var agentCommissionPaymentList = authContext.AgentCommissionPaymentDB.ToList();
                if (status == "nostatus")
                {

                    if (name != "noname")
                    {
                        var query =
                       from lb in authContext.AgentCommissionPaymentSettlementDB
                       join commission in authContext.AssignmentCommissionDb
                       on lb.AssignmentCommissionId equals commission.Id
                       join ppl in authContext.Peoples
                       on commission.AgentID equals ppl.PeopleID
                       where (ppl.DisplayName.ToLower().Contains(name.ToLower()))
                       select new AgentPaymentSettlementDisplayDc()
                       {
                           Id = lb.Id,
                           Amount = lb.Amount,
                           SettleDate = lb.SettleDate,
                           Status = commission.Status,
                           AgentCommissionPaymentId = lb.AgentCommissionPaymentId,
                           AssignmentCommissionId = lb.AssignmentCommissionId,
                           AgentName = ppl.DisplayName,
                           AssignmentId = commission.AssignmentID

                       };
                        acPager.NetRecords = query.Count();
                        acPager.AgentSettlementList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }

                    else
                    {
                        var query =
                        from lb in authContext.AgentCommissionPaymentSettlementDB
                        join commission in authContext.AssignmentCommissionDb
                        on lb.AssignmentCommissionId equals commission.Id
                        join ppl in authContext.Peoples
                        on commission.AgentID equals ppl.PeopleID
                        where (lb.IsActive == true && lb.IsDeleted == false)
                        select new AgentPaymentSettlementDisplayDc()
                        {
                            Id = lb.Id,
                            Amount = lb.Amount,
                            SettleDate = lb.SettleDate,
                            Status = commission.Status,
                            AgentCommissionPaymentId = lb.AgentCommissionPaymentId,
                            AssignmentCommissionId = lb.AssignmentCommissionId,
                            AgentName = ppl.DisplayName,
                            AssignmentId = commission.AssignmentID

                        };
                        acPager.NetRecords = query.Count();
                        acPager.AgentSettlementList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                }

                else
                {

                    if (name != "noname")
                    {
                        var query =
                        from lb in authContext.AgentCommissionPaymentSettlementDB
                        join commission in authContext.AssignmentCommissionDb
                        on lb.AssignmentCommissionId equals commission.Id
                        join ppl in authContext.Peoples
                        on commission.AgentID equals ppl.PeopleID
                        where (ppl.DisplayName.ToLower().Contains(name.ToLower()) && commission.Status == status)
                        select new AgentPaymentSettlementDisplayDc()
                        {
                            Id = lb.Id,
                            Amount = lb.Amount,
                            SettleDate = lb.SettleDate,
                            Status = commission.Status,
                            AgentCommissionPaymentId = lb.AgentCommissionPaymentId,
                            AssignmentCommissionId = lb.AssignmentCommissionId,
                            AgentName = ppl.DisplayName,
                            AssignmentId = commission.AssignmentID
                        };
                        acPager.NetRecords = query.Count();
                        acPager.AgentSettlementList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                    else
                    {
                        var query =
                         from lb in authContext.AgentCommissionPaymentSettlementDB
                         join commission in authContext.AssignmentCommissionDb
                         on lb.AssignmentCommissionId equals commission.Id
                         join ppl in authContext.Peoples
                         on commission.AgentID equals ppl.PeopleID
                         where (commission.Status == status)
                         select new AgentPaymentSettlementDisplayDc()
                         {
                             Id = lb.Id,
                             Amount = lb.Amount,
                             SettleDate = lb.SettleDate,
                             Status = commission.Status,
                             AgentCommissionPaymentId = lb.AgentCommissionPaymentId,
                             AssignmentCommissionId = lb.AssignmentCommissionId,
                             AgentName = ppl.DisplayName,
                             AssignmentId = commission.AssignmentID
                         };
                        acPager.NetRecords = query.Count();
                        acPager.AgentSettlementList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                }


                return acPager;
            }
        }




    }
}

