using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SurveyModule")]
    public class SurveyModuleController : ApiController
    {
        #region Global
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        #endregion

        [Route("AddServeyQuestionaries")]
        [HttpPost]
        public HttpResponseMessage AddServeyQuestionaries(ServeyData serveydata)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                var servey = new SurveyModule();
                servey.WarehouseId = serveydata.WarehouseId;
                servey.SurveyName = serveydata.SurveyName;
                servey.CreatedBy = userid;
                servey.CreatedDate = indianTime;
                servey.Publish = false;
                servey.IsActive = true;
                servey.IsDeleted = false;
                servey.StartDate = serveydata.StartDate;
                servey.EndDate = serveydata.EndDate;
                con.SurveyModuleDB.Add(servey);
                con.Commit();

                foreach (var item in serveydata.SQA)
                {
                    var SQ = new SurveyQuestion();
                    SQ.SurveyId = servey.Id;
                    SQ.QueName = item.QueName;
                    SQ.RightAnsId = item.RightAnsId;
                    SQ.Sequence = item.Sequence;
                    SQ.Point = item.Point;
                    SQ.isRequired = item.isRequired;
                    SQ.CreatedDate = indianTime;
                    SQ.CreatedBy = userid;
                    SQ.IsActive = true;
                    SQ.IsDeleted = false;
                    con.SurveyQuestionDB.Add(SQ);
                    con.Commit();
                    List<SurveyQuestionAnswer> SurveyQuestionAnswers = new List<SurveyQuestionAnswer>();
                    foreach (var Ans in item.AnswerList)
                    {
                        var AL = new SurveyQuestionAnswer();
                        AL.QuestionId = SQ.Id;
                        AL.Answer = Ans.Answer;
                        AL.Sequence = Ans.Sequence;
                        AL.IsActive = true;
                        AL.IsDeleted = false;
                        AL.CreatedDate = indianTime;
                        AL.CreatedBy = userid;
                        con.SurveyQuestionAnswerDB.Add(AL);
                        con.Commit();
                        SurveyQuestionAnswers.Add(AL);
                    }

                    SQ.RightAnsId = SurveyQuestionAnswers.FirstOrDefault(x => x.Sequence == item.RightAnsId).Id;
                    con.Commit();
                }

                var response = new
                {
                    Status = true,
                    Message = "Record Inserted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
        }

        [Route("UpdateServeyQuestionaries")]
        [HttpPost]
        public HttpResponseMessage UpdateServeyQuestionaries(ServeyData serveydata)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                foreach (var item in serveydata.SQA)
                {
                    var SQ = con.SurveyQuestionDB.Where(x => x.Id == item.QuestionId).FirstOrDefault();
                    if (SQ != null)
                    {
                        SQ.QueName = item.QueName;
                        SQ.Sequence = item.Sequence;
                        SQ.Point = item.Point;
                        SQ.isRequired = item.isRequired;
                        SQ.ModifiedDate = indianTime;
                        SQ.IsDeleted = item.IsDeleted;
                        SQ.ModifiedBy = userid;
                        con.Commit();
                        bool isupdate = false;
                        List<SurveyQuestionAnswer> SurveyQuestionAnswers = new List<SurveyQuestionAnswer>();
                        foreach (var Ans in item.AnswerList)
                        {
                            var sqa = con.SurveyQuestionAnswerDB.Where(x => x.QuestionId == SQ.Id && x.Id == Ans.Id).FirstOrDefault();
                            if (sqa != null)
                            {
                                sqa.Answer = Ans.Answer;
                                sqa.Sequence = Ans.Sequence;
                                sqa.IsActive = true;
                                sqa.IsDeleted = false;
                                sqa.ModifiedDate = indianTime;
                                sqa.ModifiedBy = userid;
                                con.Commit();
                            }
                            else
                            {
                                isupdate = true;
                                sqa = new SurveyQuestionAnswer();
                                sqa.QuestionId = SQ.Id;
                                sqa.Answer = Ans.Answer;
                                sqa.Sequence = Ans.Sequence;
                                sqa.IsActive = true;
                                sqa.IsDeleted = false;
                                sqa.CreatedBy = userid;
                                sqa.CreatedDate = indianTime;
                                con.SurveyQuestionAnswerDB.Add(sqa);
                                con.Commit();
                            }
                            SurveyQuestionAnswers.Add(sqa);
                        }
                        if (isupdate)
                        {
                            SQ.RightAnsId = SurveyQuestionAnswers.FirstOrDefault(x => x.Sequence == item.RightAnsId).Id;
                            con.Commit();
                        }
                    }
                    else
                    {
                        var SQA = new SurveyQuestion();
                        SQA.QueName = item.QueName;
                        SQA.RightAnsId = item.RightAnsId;
                        SQA.SurveyId = serveydata.SurveyId;
                        SQA.Sequence = item.Sequence;
                        SQA.Point = item.Point;
                        SQA.isRequired = item.isRequired;
                        SQA.CreatedDate = indianTime;
                        SQA.CreatedBy = userid;
                        SQA.IsActive = true;
                        SQA.IsDeleted = item.IsDeleted;
                        con.SurveyQuestionDB.Add(SQA);
                        con.Commit();
                        List<SurveyQuestionAnswer> SurveyQuestionAnswers = new List<SurveyQuestionAnswer>();
                        foreach (var Ans in item.AnswerList)
                        {
                            var AL = new SurveyQuestionAnswer();
                            AL.QuestionId = SQA.Id;
                            AL.Answer = Ans.Answer;
                            AL.Sequence = Ans.Sequence;
                            AL.IsActive = true;
                            AL.IsDeleted = false;
                            AL.CreatedDate = indianTime;
                            AL.CreatedBy = userid;
                            con.SurveyQuestionAnswerDB.Add(AL);
                            con.Commit();
                            SurveyQuestionAnswers.Add(AL);
                        }
                        SQA.RightAnsId = SurveyQuestionAnswers.FirstOrDefault(x => x.Sequence == item.RightAnsId).Id;
                        con.Commit();
                    }
                }

                var response = new
                {
                    Status = true,
                    Message = "Record Inserted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
        }

        [Route("GetServey")]
        [HttpGet]
        public HttpResponseMessage GetServey(int warehouseid)
        {
            using (var con = new AuthContext())
            {
                var warehousename = con.Warehouses.Where(x => x.WarehouseId == warehouseid).Select(x => x.WarehouseName).FirstOrDefault();
                var servey = con.SurveyModuleDB.Where(x => x.WarehouseId == warehouseid && x.IsActive == true).Select(x => new { SurveyName = x.SurveyName, SurveyId = x.Id, WarehouseId = x.WarehouseId, Publish = x.Publish, WarehouseName = warehousename, StartDate = x.StartDate, EndDate = x.EndDate }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, servey);
            }
        }

        [Route("GetServeyData")]
        [HttpGet]
        public List<ServeyData> GetServeyData(int serveyid)
        {
            var sdlist = new List<ServeyData>();
            using (var con = new AuthContext())
            {
                var servey = new List<SurveyModule>();
                if (serveyid > 0)
                    servey = con.SurveyModuleDB.Where(x => x.Id == serveyid && x.IsActive == true).ToList();
                else
                    servey = con.SurveyModuleDB.Where(x => x.IsActive == true).ToList();

                foreach (var sitem in servey)
                {
                    var sd = new ServeyData();
                    sd.WarehouseId = sitem.WarehouseId;
                    var warehousename = con.Warehouses.Where(x => x.WarehouseId == sd.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    sd.WarehouseName = warehousename;
                    sd.SurveyId = sitem.Id;
                    sd.StartDate = sitem.StartDate;
                    sd.EndDate = sitem.EndDate;
                    sd.SurveyName = sitem.SurveyName;
                    sd.Publish = sitem.Publish;
                    var question = con.SurveyQuestionDB.Where(x => x.SurveyId == sitem.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                    var qadl = new List<SurveyQuestionAnswerData>();
                    var answer = new List<SurveyQAD>();
                    foreach (var item in question)
                    {
                        var qad = new SurveyQuestionAnswerData();
                        qad.QuestionId = item.Id;
                        qad.QueName = item.QueName;
                        qad.RightAnsId = item.RightAnsId;
                        qad.isRequired = item.isRequired;
                        qad.Point = item.Point;
                        qad.IsDeleted = item.IsDeleted;
                        qad.Sequence = item.Sequence;
                        var al = con.SurveyQuestionAnswerDB.Where(x => x.QuestionId == item.Id && x.IsActive == true).Select(x => new SurveyQAD
                        { Answer = x.Answer, QuestionId = x.QuestionId, Sequence = x.Sequence, Id = x.Id }).ToList();
                        qad.AnswerList = al;
                        qadl.Add(qad);
                    }
                    sd.SQA = qadl;
                    sdlist.Add(sd);
                }
                return sdlist;
            }
        }

        [Route("CopySurvey")]
        [HttpGet]
        public HttpResponseMessage CopySurvey(int serveyid, int WarehouseId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var servey = new SurveyModule();
                servey = context.SurveyModuleDB.Where(x => x.Id == serveyid && x.IsActive == true).FirstOrDefault();

                servey.WarehouseId = WarehouseId;
                servey.CreatedBy = userid;
                servey.CreatedDate = indianTime;
                servey.Publish = false;
                context.SurveyModuleDB.Add(servey);
                context.Commit();

                var question = context.SurveyQuestionDB.Where(x => x.SurveyId == serveyid && x.IsActive == true && x.IsDeleted == false).ToList();
                foreach (var item in question)
                {
                    var QueAns = context.SurveyQuestionAnswerDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.QuestionId == item.Id).ToList();
                    item.SurveyId = servey.Id;
                    item.CreatedBy = userid;
                    item.CreatedDate = indianTime;
                    context.SurveyQuestionDB.Add(item);
                    context.Commit();
                    foreach (var qaitem in QueAns)
                    {
                        qaitem.QuestionId = item.Id;
                        qaitem.CreatedBy = userid;
                        qaitem.CreatedDate = indianTime;
                        context.SurveyQuestionAnswerDB.Add(qaitem);
                        context.Commit();
                    }

                }

                var response = new
                {
                    Status = true,
                    Message = "Record Inserted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("DeleteSurvey")]
        [HttpGet]
        public HttpResponseMessage DeleteSurvey(int serveyid, int WarehouseId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var servey = new SurveyModule();
                servey = context.SurveyModuleDB.Where(x => x.Id == serveyid && x.IsActive == true && x.WarehouseId == WarehouseId).FirstOrDefault();

                servey.IsActive = false;
                servey.IsDeleted = true;
                servey.ModifiedBy = userid;
                servey.ModifiedDate = indianTime;
                context.Commit();

                var response = new
                {
                    Status = true,
                    Message = "Record Deleted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("PublishSurvey")]
        [HttpGet]
        public HttpResponseMessage PublishSurvey(int serveyid, int WarehouseId, bool IsPublish)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var servey = new SurveyModule();
                // context.Database.ExecuteSqlCommand("Update SurveyModules SET Publish = 0 Where WarehouseId = "+ WarehouseId);
                servey = context.SurveyModuleDB.Where(x => x.Id == serveyid && x.IsActive == true && x.WarehouseId == WarehouseId).FirstOrDefault();
                servey.Publish = IsPublish;
                servey.ModifiedBy = userid;
                servey.ModifiedDate = indianTime;
                context.Commit();

                var response = new
                {
                    Status = true,
                    Message = "Record Deleted"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetServeybyName")]
        [HttpGet]
        public List<ServeyData> GetServeybyName(string serveyname)
        {
            var sdlist = new List<ServeyData>();
            using (var con = new AuthContext())
            {
                var servey = new List<SurveyModule>();
                if (!string.IsNullOrEmpty(serveyname))
                    servey = con.SurveyModuleDB.Where(x => x.SurveyName.Contains(serveyname) && x.IsActive == true).ToList();
                else
                    servey = con.SurveyModuleDB.Where(x => x.IsActive == true).ToList();

                foreach (var sitem in servey)
                {
                    var sd = new ServeyData();
                    sd.WarehouseId = sitem.WarehouseId;
                    var warehousename = con.Warehouses.Where(x => x.WarehouseId == sd.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    sd.WarehouseName = warehousename;
                    sd.SurveyId = sitem.Id;
                    sd.StartDate = sitem.StartDate;
                    sd.EndDate = sitem.EndDate;
                    sd.SurveyName = sitem.SurveyName;
                    sd.Publish = sitem.Publish;
                    var question = con.SurveyQuestionDB.Where(x => x.SurveyId == sitem.Id && x.IsActive == true).ToList();
                    var qadl = new List<SurveyQuestionAnswerData>();
                    var answer = new List<SurveyQAD>();
                    foreach (var item in question)
                    {
                        var qad = new SurveyQuestionAnswerData();
                        qad.QuestionId = item.Id;
                        qad.QueName = item.QueName;
                        qad.RightAnsId = item.RightAnsId;
                        qad.isRequired = item.isRequired;
                        qad.Point = item.Point;
                        qad.Sequence = item.Sequence;
                        var al = con.SurveyQuestionAnswerDB.Where(x => x.QuestionId == item.Id && x.IsActive == true).Select(x => new SurveyQAD
                        { Answer = x.Answer, QuestionId = x.QuestionId, Sequence = x.Sequence, Id = x.Id }).ToList();
                        qad.AnswerList = al;
                        qadl.Add(qad);
                    }
                    sd.SQA = qadl;
                    sdlist.Add(sd);
                }
                return sdlist;
            }
        }

    }

    public class ServeyData
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public long SurveyId { get; set; }
        public string SurveyName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Publish { get; set; }
        public List<SurveyQuestionAnswerData> SQA { get; set; }
    }



    public class SurveyQuestionAnswerData
    {
        public long QuestionId { get; set; }
        public string QueName { get; set; }
        public long RightAnsId { get; set; }
        public bool isRequired { get; set; }
        public int Point { get; set; }
        public int Sequence { get; set; }
        public bool? IsDeleted { get; set; }
        public List<SurveyQAD> AnswerList { get; set; }
    }

    public class SurveyQAD
    {
        public long Id { get; set; }
        public long QuestionId { get; set; }
        public string Answer { get; set; }
        public int Sequence { get; set; }
    }
}