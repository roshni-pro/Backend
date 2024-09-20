using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.Accounts;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
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
    [RoutePrefix("api/AgentCommissionPayment")]
    public class AgentCommissionPaymentController : BaseApiController
    {
        [Route("AddNewPayment")]
        [HttpPost]
        public long AddNewPayment(AgentCommissionPaymentDc agentCommissionPayment)
        {
            int userid = 0;
            if (agentCommissionPayment != null)
            {
                AgentCommissionPayment payment = new AgentCommissionPayment
                {
                    AgentId = agentCommissionPayment.AgentId,
                    AgentLedgerId = agentCommissionPayment.AgentLedgerId,
                    BankLedgerId = agentCommissionPayment.BankLedgerId,
                    Amount = agentCommissionPayment.Amount,
                    PaymentDate = agentCommissionPayment.PaymentDate,
                    RefNumber = agentCommissionPayment.RefNumber,
                    Narration = agentCommissionPayment.Narration,
                    Status = "Paid",
                    SettledStatus = "UnSettled",
                    SettledAmount = 0,

                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                };

                using (var context = new AuthContext())
                {
                    context.AgentCommissionPaymentDB.Add(payment);
                    context.Commit();
                    return payment.Id;
                }
            }
            else
            {
                return 0;
            }
        }


        [Route("GetPeople")]
        [HttpGet]
        public async Task<List<PeopleAll>> GetPeople()
        {
            using (var authContext = new AuthContext())
            {
                var ledgerTypeList = authContext.AgentCommissionPaymentDB.ToList();

                var query = from ppl in authContext.Peoples
                            select new PeopleAll()
                            {
                                PeopleID = ppl.PeopleID,
                                DisplayName = ppl.DisplayName
                            };
                return query.OrderByDescending(x => x.PeopleID).ToList();
            }

        }

        [Route("GetPeopleByName")]
        [HttpGet]
        public IHttpActionResult GetPeopleByName(string name)
        {
            using (var authContext = new AuthContext())
            {
                //var peopleList = authContext.Peoples.ToList();

                var query = from ppl in authContext.Peoples
                            where (ppl.DisplayName.ToLower().Contains(name.ToLower()))
                            select new
                            {
                                ppl.PeopleID,
                                ppl.DisplayName,
                                ppl.CompanyId,
                                ppl.WarehouseId,
                                ppl.Email,
                                ppl.UserName,
                                ppl.Mobile
                            };
                var peopleList = query.ToList();
                return Ok(peopleList);
            }
        }


        [Route("CancelPayment/{agentCommissionPaymentId}")]
        [HttpGet]
        public bool CancePayment(long agentCommissionPaymentId)
        {
            int userid = 0;
            using (var context = new AuthContext())
            {
                AgentCommissionPayment payment = context.AgentCommissionPaymentDB.FirstOrDefault(x => x.Id == agentCommissionPaymentId);
                if (payment != null && payment.SettledStatus == "UnSettled")
                {

                    var ledgerEntryList = context.LadgerEntryDB.Where(x => x.ObjectID == payment.Id && x.ObjectType == "AgentCommissionPayment").ToList();

                    if (ledgerEntryList != null && ledgerEntryList.Count > 0)
                    {
                        LadgerHelper helper = new LadgerHelper();
                        VoucherType voucherType = helper.GetOrCreateVoucherType("PaymentCancel", 0, context);
                        foreach (var entity in ledgerEntryList)
                        {
                            context.Entry(entity).State = EntityState.Detached;

                            entity.ID = 0;
                            long tempLedgerId = entity.LagerID.Value;
                            entity.LagerID = entity.AffectedLadgerID;
                            entity.AffectedLadgerID = tempLedgerId;
                            entity.Date = DateTime.Now;
                            entity.VouchersTypeID = voucherType.ID;
                            context.LadgerEntryDB.Add(entity);
                            context.Commit();
                        }

                    }


                    payment.Status = "Cancelled";
                    payment.ModifiedBy = userid;
                    payment.ModifiedDate = DateTime.Now;
                    context.Commit();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        [Route("GetAgentCommissionPaymentList/skip/{skip}/take/{take}")]
        [HttpGet]
        public AgentCommissionPager GetAgentCommissionPaymentList(int skip, int take)
        {
            AgentCommissionPager acPager = new AgentCommissionPager();

            using (var authContext = new AuthContext())
            {
                var ledgerTypeList = authContext.AgentCommissionPaymentDB.ToList();
                var query = from lb in authContext.AgentCommissionPaymentDB
                            where (lb.IsActive == true && lb.IsDeleted == false)
                            select new AgentCommissionPaymentDc()
                            {
                                Id = lb.Id,
                                Narration = lb.Narration,
                                Status = lb.Status,
                                RefNumber = lb.RefNumber,
                                AgentId = lb.AgentId,
                                BankLedgerId = lb.BankLedgerId,
                                PaymentDate = lb.PaymentDate,
                                Amount = lb.Amount,
                                SettledStatus = lb.SettledStatus,
                                SettledAmount = lb.SettledAmount
                            };
                acPager.NetRecords = query.Count();
                acPager.AgentList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                return acPager;
            }
        }

        [Route("GetAgentCommissionPaymentListByName/name/{name}/status/{status}/skip/{skip}/take/{take}")]
        [HttpGet]
        public AgentCommissionPager GetAgentCommissionPaymentListByName(string name, string status, int skip, int take)
        {
            AgentCommissionPager acPager = new AgentCommissionPager();
            using (var authContext = new AuthContext())
            {
                var agentCommissionPaymentList = authContext.AgentCommissionPaymentDB.ToList();
                if (status == "nostatus")
                {

                    if (name != "noname")
                    {
                        var query =
                        from ac in authContext.AgentCommissionPaymentDB
                        join ppl in authContext.Peoples
                        on ac.AgentId equals ppl.PeopleID
                        where (ppl.DisplayName.ToLower().Contains(name.ToLower()))
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
                        acPager.NetRecords = query.Count();
                        acPager.AgentList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }

                    else
                    {
                        var query =
                     from ac in authContext.AgentCommissionPaymentDB
                     join ppl in authContext.Peoples
                     on ac.AgentId equals ppl.PeopleID

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
                        acPager.NetRecords = query.Count();
                        acPager.AgentList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }


                }

                else
                {

                    if (name != "noname")
                    {
                        var query =
                            from ac in authContext.AgentCommissionPaymentDB
                            join ppl in authContext.Peoples
                            on ac.AgentId equals ppl.PeopleID
                            where (ppl.DisplayName.ToLower().Contains(name.ToLower()) && ac.Status == status)
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
                        acPager.NetRecords = query.Count();
                        acPager.AgentList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                    else
                    {
                        var query =
                    from ac in authContext.AgentCommissionPaymentDB
                    join ppl in authContext.Peoples
                    on ac.AgentId equals ppl.PeopleID
                    where (ac.Status == status)
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
                        acPager.NetRecords = query.Count();
                        acPager.AgentList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                }


                return acPager;
            }
        }





        
    }
}

