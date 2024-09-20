
using AngularJSAuthentication.DataContracts.Transaction.ServeyModule;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SurveyModuleAndroid")]
    public class SurveyModuleAndroidController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [AllowAnonymous]
        [Route("GetSurvey")]
        [HttpPost]
        public HttpResponseMessage GetSurvey(GetSurveyDc data)
        {
            var sd = new SurveyModuleDC();
            using (var con = new AuthContext())
            {
                var ValidCustomer = con.Customers.Where(x => x.CustomerId == data.CustomerId && x.Active == true && x.Deleted == false).Count();
                if (ValidCustomer > 0)
                {

                    List<SurveyQuestAnswDC> SurveyQuestAnswDCs = con.Database.SqlQuery<SurveyQuestAnswDC>("Exec GetSurvey " + data.CustomerId + "," + data.WarehouseId).ToList();

                    sd = SurveyQuestAnswDCs.GroupBy(x => new
                    {
                        x.surveyId,
                        x.SurveyName,
                        x.WarehouseId,
                        x.WarehouseName,
                        x.StartDate,
                        x.EndDate,
                        x.PointsEarned,
                    }).Select(survey => new SurveyModuleDC
                    {
                        EndDate = survey.Key.EndDate,
                        StartDate = survey.Key.StartDate,
                        SurveyId = Convert.ToInt32(survey.Key.surveyId),
                        SurveyName = survey.Key.SurveyName,
                        WarehouseId = survey.Key.WarehouseId,
                        WarehouseName = survey.Key.WarehouseName,
                        QuestionCount = survey.GroupBy(y => y.QuestionId).Count(),
                        PointsEarned = survey.Key.PointsEarned,
                        AnsweredCount = survey.Where(y => y.IsAnswerd).GroupBy(y => y.QuestionId).Count(),
                        CorrectAnswerCount = survey.Where(y => y.IsAnswerd && y.isRight).Count(),
                        SQA = survey.GroupBy(y => new { y.QuestionId, y.QueName, y.QuestionSequence, y.isRequired, y.IsAnswerd, y.Point, y.RightAnsId }).Select(quest => new SurveyQuestionAnswerDc
                        {
                            IsAnswerd = quest.Key.IsAnswerd,
                            isRequired = quest.Key.isRequired,
                            Point = quest.Key.Point,
                            QueName = quest.Key.QueName,
                            QuestionId = quest.Key.QuestionId,
                            Sequence = quest.Key.QuestionSequence,
                            RightAnsId = quest.Key.RightAnsId,
                            OptionCount = quest.Count(),
                            AnswerList = quest.Select(a => new SurveyQADList
                            {
                                Answer = a.Answer,
                                Sequence = a.answerSequence,
                                AnswerId = a.answerId
                            }).ToList()
                        }).ToList()
                    }).FirstOrDefault();

                    return Request.CreateResponse(HttpStatusCode.OK, sd);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [AllowAnonymous]
        [Route("SaveAnswer")]
        [HttpPost]
        public HttpResponseMessage SaveAnswer(SaveAnswerDc SaveAnswerDc)
        {
            using (var con = new AuthContext())
            {

                if (SaveAnswerDc.isComplete == true)

                {
                    var SurveyCheck = con.CustomerSurveyDB.Where(x => x.CustomerId == SaveAnswerDc.CustomerId && x.SurveyId == SaveAnswerDc.SurveyId && x.IsDeleted == false).FirstOrDefault();
                    if (SurveyCheck != null && !SurveyCheck.isComplete)
                    {
                        var Questionids = con.CustomerSurveyAnswerDB.Where(x => x.CustomerSurveyId == SurveyCheck.Id && x.isRight).Select(x => x.QueId).ToList();
                        var totalpoints = Questionids.Any() ? con.SurveyQuestionDB.Where(x => Questionids.Contains(x.Id)).Sum(x => x.Point) : 0;
                        SurveyCheck.Point = totalpoints;
                        SurveyCheck.isComplete = true;
                        con.Commit();

                        if (totalpoints == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }


                        var wallet = con.WalletDb.SingleOrDefault(c => c.CustomerId == SurveyCheck.CustomerId);

                        if (wallet == null)
                        {
                            Wallet w = new Wallet();
                            var cust = con.Customers.SingleOrDefault(c => c.CustomerId == SurveyCheck.CustomerId);

                            if (cust != null)
                            {
                                w.ShopName = cust.ShopName;
                                w.Skcode = cust.Skcode;
                            }
                            w.CustomerId = SurveyCheck.CustomerId;
                            w.TotalAmount = totalpoints;
                            w.CreatedDate = indianTime;
                            w.UpdatedDate = indianTime;
                            w.Deleted = false;
                            con.WalletDb.Add(w);
                        }
                        else
                        {
                            wallet.TotalAmount = wallet.TotalAmount + totalpoints;
                            con.Entry(wallet).State = EntityState.Modified;
                        }
                        con.Commit();
                    }
                }
                else
                {
                    var customersurvey = new CustomerSurvey();
                    var custSurveyCheck = con.CustomerSurveyDB.Where(x => x.CustomerId == SaveAnswerDc.CustomerId && x.SurveyId == SaveAnswerDc.SurveyId && x.IsDeleted == false).FirstOrDefault();

                    if (custSurveyCheck == null)
                    {
                        customersurvey.CustomerId = SaveAnswerDc.CustomerId;
                        customersurvey.SurveyId = SaveAnswerDc.SurveyId;
                        customersurvey.CreatedBy = SaveAnswerDc.CustomerId;
                        customersurvey.CreatedDate = DateTime.Now;
                        customersurvey.IsActive = true;
                        customersurvey.IsDeleted = false;
                        customersurvey.StartTime = DateTime.Now;
                        con.CustomerSurveyDB.Add(customersurvey);
                        con.Commit();
                    }
                    //else
                    //{
                    //    SurveyCheck.ModifiedBy = SaveAnswerDc.CustomerId;
                    //    SurveyCheck.ModifiedDate = DateTime.Now;
                    //    con.Commit();
                    //}

                    var customersurveyanswer = new CustomerSurveyAnswer();
                    if (custSurveyCheck == null)
                    {
                        customersurveyanswer.CustomerSurveyId = customersurvey.Id;
                    }
                    else
                    {
                        customersurveyanswer.CustomerSurveyId = custSurveyCheck.Id;
                    }
                    customersurveyanswer.isRight = SaveAnswerDc.isRight;
                    customersurveyanswer.AnswerId = SaveAnswerDc.AnswerId;
                    customersurveyanswer.QueId = SaveAnswerDc.QueId;
                    customersurveyanswer.IsActive = true;
                    customersurveyanswer.IsDeleted = false;
                    customersurveyanswer.CreatedDate = DateTime.Now;
                    customersurveyanswer.CreatedBy = SaveAnswerDc.CustomerId;
                    con.CustomerSurveyAnswerDB.Add(customersurveyanswer);
                    con.Commit();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }


    }
}


