using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Game")]
    public class GameController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
      
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Post API for Insert Question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [Route("AddQuestion")]
        [HttpPost]
        public HttpResponseMessage AddQuestion(GameQuestion question)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                question.CreatedDate = indianTime;
                question.UpdatedDate = indianTime;
                question.CompanyId = compid;
                question.Deleted = false;

                question.GameImage = question.GameImage;
                using (AuthContext db = new AuthContext())
                {
                    var Questiondata = db.GameQuestionDB.Where(x => x.GameLevelId == question.GameLevelId).ToList();
                    if (Questiondata.Count() != 0)
                    {
                        var qry = db.GameQuestionDB.Where(m => m.GameLevelId == question.GameLevelId).OrderByDescending(m => m.GameLevelId).Take(1).SingleOrDefault();
                        int sn = qry.GameQuestionSquence;
                        question.GameQuestionSquence = sn + 1;
                    }
                    else
                    {
                        question.GameQuestionSquence = 1;
                    }
                    db.GameQuestionDB.Add(question);
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Post API for Update Question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [Route("EditQuestion")]
        [HttpPost]
        public HttpResponseMessage EditQuestion(GameQuestion question)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    GameQuestion gameQuestion = db.GameQuestionDB.Where(x => x.GameQuestionId == question.GameQuestionId).SingleOrDefault();
                    gameQuestion.GameQuestionData = question.GameQuestionData;
                    gameQuestion.GameQuestionHindi = question.GameQuestionHindi;
                    gameQuestion.GameAnswer = question.GameAnswer;
                    gameQuestion.QuestionPoints = question.QuestionPoints;
                    gameQuestion.GameLevelId = question.GameLevelId;
                    gameQuestion.UpdatedDate = indianTime;
                    if (question.GameImage != null)
                    {
                        gameQuestion.GameImage = question.GameImage;
                    }
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Get API for GetAllQuestion
        /// </summary>
        /// <returns></returns>
        [Route("GetQuestion")]
        [HttpGet]
        public List<GameData> GetQuestion()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    List<GameData> gameQuestion = (from g in db.GameQuestionDB
                                                   join gl in db.GameLevelDB on g.GameLevelId equals gl.GameLevelId
                                                   where g.Deleted == false && g.IsActive == true
                                                   select new GameData
                                                   {
                                                       GameQuestionId = g.GameQuestionId,
                                                       GameLevelId = g.GameLevelId,
                                                       GameQuestionData = g.GameQuestionData,
                                                       GameQuestionHindi = g.GameQuestionHindi,
                                                       GameAnswer = g.GameAnswer,
                                                       QuestionPoints = g.QuestionPoints,
                                                       GameLevelName = gl.GameLevelName,
                                                       CreatedDate = g.CreatedDate,
                                                       GameImage = g.GameImage
                                                   }).ToList();
                    return gameQuestion;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get API for GetQuestionCount behalf of LevelID
        /// </summary>
        /// <param name="GameLevelid"></param>
        /// <returns></returns>
        [Route("GetGameLevelCount")]
        [HttpGet]
        public HttpResponseMessage GetQuestionCount(int GameLevelid)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    int gameQuestioncount = db.GameQuestionDB.Where(x => x.WarehouseId == Warehouse_id && x.CompanyId == compid && x.GameLevelId == GameLevelid && x.Deleted == false).Count();
                    var res = new
                    {
                        count = gameQuestioncount
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete API for Question
        /// </summary>
        /// <param name="GameQuestionId"></param>
        /// <returns></returns>
        [Route("DeleteGameQuestion")]
        [HttpDelete]
        public HttpResponseMessage DeleteGameQuestion(int GameQuestionId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    GameQuestion gl = db.GameQuestionDB.Where(x => x.GameQuestionId == GameQuestionId).SingleOrDefault();
                    gl.Deleted = true;
                    gl.UpdatedDate = indianTime;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Post API for Insert Game Level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [Route("AddGameLevel")]
        [HttpPost]
        public HttpResponseMessage AddGameLevel(GameLevel level)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    level.CreatedDate = indianTime;
                    level.UpdatedDate = indianTime;
                    level.CompanyId = compid;
                    level.Deleted = false;
                    db.GameLevelDB.Add(level);
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Post API for Edit Game Level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [Route("EditGameLevel")]
        [HttpPost]
        public HttpResponseMessage EditGameLevel(GameLevel level)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    GameLevel gl = db.GameLevelDB.Where(x => x.GameLevelId == level.GameLevelId).SingleOrDefault();
                    gl.GameLevelName = level.GameLevelName;
                    gl.GameLevelPoints = level.GameLevelPoints;
                    gl.UpdatedDate = indianTime;
                    gl.CompanyId = compid;
                    gl.Deleted = false;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Get API for GameLevel
        /// </summary>
        /// <returns></returns>
        [Route("GetGameLevel")]
        [HttpGet]
        public List<GameLevel> GetGameLevel()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    List<GameLevel> gameLevel = db.GameLevelDB.Where(x => x.WarehouseId == Warehouse_id && x.CompanyId == compid && x.Deleted == false).ToList();
                    return gameLevel;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get API for Game Level
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        [Route("GetGameLevelForMobile")]
        [HttpGet]
        public HttpResponseMessage GetGameLevelForMobile(int WarehouseId, int CompanyId)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    List<GameLevel> gameLevel = db.GameLevelDB.Where(x => x.Deleted == false && x.CompanyId == CompanyId && x.WarehouseId == WarehouseId).ToList();
                    var res = new
                    {
                        Gamelevel = gameLevel,
                        Status = true,
                        Message = "Get Game Level"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                var res = new
                {
                    Status = false,
                    Message = "Not Get Game Level"
                };
                return null;
            }
        }

        /// <summary>
        /// Delete API for Game Level
        /// </summary>
        /// <param name="GameLevelId"></param>
        /// <returns></returns>
        [Route("DeleteGameLevel")]
        [HttpDelete]
        public HttpResponseMessage DeleteGameLevel(int GameLevelId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }

                }
                using (AuthContext db = new AuthContext())
                {
                    GameLevel gl = db.GameLevelDB.Where(x => x.GameLevelId == GameLevelId).SingleOrDefault();
                    gl.Deleted = true;
                    gl.UpdatedDate = indianTime;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
            }
        }

        /// <summary>
        /// Post API for Insert Game Answer Mobile
        /// </summary>
        /// <param name="gameAnswerData"></param>
        /// <returns></returns>
        [Route("InsertGameAnswerMobile")]
        [HttpPost]
        public HttpResponseMessage GiveAnswar(GameAnswerData gameAnswerData)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var data = db.GameQuestionDB.Where(x => x.WarehouseId == gameAnswerData.WarehouseId && x.CompanyId == gameAnswerData.CompanyId && x.GameQuestionId == gameAnswerData.QuestionId && x.GameLevelId == gameAnswerData.LevelId).SingleOrDefault();
                    //if (gameAnswerData.Answer == data.GameAnswer)
                    //{
                    CustGame custGame = new CustGame();
                    custGame.CustomerId = gameAnswerData.CustomerId;
                    custGame.WarehouseId = gameAnswerData.WarehouseId;
                    custGame.CompanyId = gameAnswerData.CompanyId;
                    custGame.LevelId = gameAnswerData.LevelId;
                    custGame.QuestionId = gameAnswerData.QuestionId;
                    custGame.CreatedDate = indianTime;
                    custGame.UpdatedDate = indianTime;
                    custGame.Deleted = false;
                    custGame.IsActive = true;
                    db.CustGameDB.Add(custGame);
                    db.Commit();
                    var res = new
                    {
                        Status = true,
                        Message = "Answer is Correct"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

                //}
                //else
                //{
                //    var res = new
                //    {
                //        Status = false,
                //        Message = "Answer is Wrong"
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
            }

            catch (Exception ee)
            {
                var res = new
                {
                    Error = ee.Message,
                    Status = false,
                    Message = "Some went Wrong"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }

        /// <summary>
        /// Post API for Insert Game Question
        /// </summary>
        /// <param name="gameAnswerData"></param>
        /// <returns></returns>
        [Route("InsertGameAttemptedQuestion")]
        [HttpPost]
        public HttpResponseMessage GameAttemptedQuestion(GameAnswerData gameAnswerData)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var UserAnswer = db.GameQuestionDB.Where(x => x.GameQuestionId == gameAnswerData.QuestionId && x.GameAnswer == gameAnswerData.UserAnswer && x.GameLevelId == gameAnswerData.LevelId).SingleOrDefault();

                    if (UserAnswer != null)
                    {
                        var Attempeted = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == gameAnswerData.CustomerId && x.LevelId == gameAnswerData.LevelId && x.QuestionId == gameAnswerData.QuestionId).SingleOrDefault();
                        if (Attempeted == null)
                        {
                            GameAttemptedQuestion custGame = new GameAttemptedQuestion();
                            custGame.CustomerId = gameAnswerData.CustomerId;
                            custGame.WarehouseId = gameAnswerData.WarehouseId;
                            custGame.CompanyId = gameAnswerData.CompanyId;
                            custGame.LevelId = gameAnswerData.LevelId;
                            custGame.QuestionId = gameAnswerData.QuestionId;
                            custGame.UserAnswer = gameAnswerData.UserAnswer;
                            custGame.CreatedDate = indianTime;
                            custGame.IsActive = true;
                            custGame.Deleted = false;
                            db.GameAttemptedQuestionDB.Add(custGame);
                            db.Commit();
                        }
                        var res = new
                        {
                            Status = true,
                            Message = "Correct Answer"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            Status = true,
                            Message = "Wrong Answer"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            catch (Exception ex)
            {
                var res = new
                {
                    Status = true,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
            }
        }

        /// <summary>
        /// Post API for Insert Level crossed details
        /// </summary>
        /// <param name="levelCrossedData"></param>
        /// <returns></returns>
        [Route("InsertLevelCrossedDetails")]
        [HttpPost]
        public HttpResponseMessage LevelCrossedDetails(LevelCrossedData levelCrossedData)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var levelQuestion = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == levelCrossedData.CustomerId && x.LevelId == levelCrossedData.CurrentLevel).Count();
                    if (levelQuestion == 10)
                    {
                        LevelCrossedDetails LevelData = new LevelCrossedDetails();
                        LevelData.CustomerId = levelCrossedData.CustomerId;
                        LevelData.WarehouseId = levelCrossedData.WarehouseId;
                        LevelData.CompanyId = levelCrossedData.CompanyId;
                        LevelData.CurrentLevel = levelCrossedData.CurrentLevel;
                        LevelData.PointsEarned = LevelData.PointsEarned;
                        LevelData.CreatedDate = indianTime;
                        LevelData.IsActive = true;
                        LevelData.Deleted = false;
                        db.LevelCrossedDetailsDB.Add(LevelData);
                        db.Commit();

                        TotalEarnedPoints TotalEarnedPoint = db.TotalEarnedPointsDB.Where(x => x.CustomerId == levelCrossedData.CustomerId && x.WarehouseId == levelCrossedData.WarehouseId && x.CompanyId == levelCrossedData.CompanyId).FirstOrDefault();
                        if (TotalEarnedPoint != null)
                        {
                            TotalEarnedPoint.CurrentLevel = levelCrossedData.CurrentLevel;
                            TotalEarnedPoint.TotalPointsEarned = TotalEarnedPoint.TotalPointsEarned + levelCrossedData.PointsEarned;
                            TotalEarnedPoint.UpdatedDate = indianTime;
                            db.Commit();
                        }
                        else
                        {
                            TotalEarnedPoints tep = new TotalEarnedPoints();
                            tep.CustomerId = levelCrossedData.CustomerId;
                            tep.CurrentLevel = levelCrossedData.CurrentLevel;
                            tep.TotalPointsEarned = levelCrossedData.PointsEarned;
                            tep.CompanyId = levelCrossedData.CompanyId;
                            tep.WarehouseId = levelCrossedData.WarehouseId;
                            tep.IsActive = true;
                            tep.Deleted = false;
                            tep.CreatedDate = indianTime;
                            tep.UpdatedDate = indianTime;
                            db.TotalEarnedPointsDB.Add(tep);
                            db.Commit();
                        }
                        var res = new
                        {
                            Status = true,
                            PointsEarned = levelCrossedData.PointsEarned,
                            Message = "Level Cleared"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            Status = false,
                            PointsEarned = 0,
                            Message = "Try Again"
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                }
            }
            catch (Exception ex)
            {
                var res = new
                {
                    Status = false,
                    PointsEarned = 0,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
            }
        }

        /// <summary>
        /// Get API for Checking game answer
        /// </summary>
        /// <param name="gameAnswerData"></param>
        /// <returns></returns>
        [Route("GiveQuestionToLevelMobile")]
        [HttpGet]
        public HttpResponseMessage GiveQuestionToLevel(GameAnswerData gameAnswerData)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    int data = db.CustGameDB.Where(x => x.WarehouseId == gameAnswerData.WarehouseId && x.CompanyId == gameAnswerData.CompanyId && x.LevelId == gameAnswerData.LevelId && x.CustomerId == gameAnswerData.CustomerId && x != null).DefaultIfEmpty().Max(p => p == null ? 0 : p.QuestionId);
                    List<GameQuestion> gameQuestions = db.GameQuestionDB.Where(x => x.WarehouseId == gameAnswerData.WarehouseId && x.CompanyId == gameAnswerData.CompanyId && x.GameLevelId == gameAnswerData.LevelId && x.GameQuestionId > data).ToList();
                    var res = new
                    {
                        gameQuestions = gameQuestions,
                        Status = true,
                        Message = "Answer is Correct"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ee)
            {
                var res = new
                {
                    Error = ee.Message,
                    Status = false,
                    Message = "Some went Wrong"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }

        /// <summary>
        /// Get API for Level Data
        /// </summary>
        /// <param name="LevelID"></param>
        /// <param name="CustomerID"></param>
        /// <returns></returns>
        [Route("GetLevelData")]
        [HttpGet]
        public HttpResponseMessage GetLavelData(int LevelID, string Language)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    if (Language == "hindi")
                    {
                        var GameQuestion = db.GameQuestionDB.Where(x => x.Deleted == false && x.IsActive == true && x.GameLevelId == LevelID).ToList().Select(t => new
                        {
                            GameQuestionId = t.GameQuestionId,
                            GameQuestionData = t.GameQuestionHindi,
                            GameAnswer = t.GameAnswer,
                            QuestionPoints = t.QuestionPoints,
                            GameImage = t.GameImage
                        });

                        return Request.CreateResponse(HttpStatusCode.OK, GameQuestion);
                    }
                    else
                    {
                        var GameQuestion = db.GameQuestionDB.Where(x => x.Deleted == false && x.IsActive == true && x.GameLevelId == LevelID).ToList().Select(t => new
                        {
                            GameQuestionId = t.GameQuestionId,
                            GameQuestionData = t.GameQuestionData,
                            GameAnswer = t.GameAnswer,
                            QuestionPoints = t.QuestionPoints,
                            GameImage = t.GameImage
                        });

                        return Request.CreateResponse(HttpStatusCode.OK, GameQuestion);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }

        }

        /// <summary>
        /// Get API for User Current Level
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <returns></returns>
        [Route("GetUserCurrentLevel")]
        [HttpGet]
        public HttpResponseMessage GetUserCurrentLevel(int CustomerID, string Language)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    List<currentdata> response = new List<currentdata>();
                    var QuestionData = db.GameQuestionDB.ToList();
                    foreach (var Question in QuestionData)
                    {
                        var PassedLevel = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == CustomerID && x.LevelId == Question.GameLevelId && x.QuestionId == Question.GameQuestionId).ToList();
                        if (PassedLevel.Count == 0)
                        {
                            currentdata cd = new currentdata();
                            cd.GameLevelID = Question.GameLevelId;
                            cd.GameQuestionId = Question.GameQuestionId;
                            cd.GameQuestion = Language == "hindi" ? Question.GameQuestionHindi : Question.GameQuestionData;
                            cd.GameAnswer = Question.GameAnswer;
                            cd.GameImage = Question.GameImage;
                            response.Add(cd);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }

        }

        /// <summary>
        /// Get API for User Current Level
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="LevelID"></param>
        /// <returns></returns>
        [Route("GetCurrentLevelQuestion")]
        [HttpGet]
        public HttpResponseMessage GetCurrentLevelQuestion(int CustomerID, int LevelID, string Language)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    List<currentdata> response = new List<currentdata>();
                    List<GameQuestion> res = new List<GameQuestion>();
                    var PassedLevel = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == CustomerID && x.LevelId == LevelID).Count();
                    if (PassedLevel < 10)
                    {
                        var GameQuestion = db.GameQuestionDB.Where(x => x.Deleted == false && x.IsActive == true && x.GameLevelId == LevelID).ToList().Select(t => new
                        {
                            GameQuestionId = t.GameQuestionId,
                            GameQuestionData = Language == "hindi" ? t.GameQuestionHindi : t.GameQuestionData,
                            GameAnswer = t.GameAnswer,
                            QuestionPoints = t.QuestionPoints,
                            GameImage = t.GameImage
                        });

                        var Response = new
                        {
                            Status = true,
                            Message = "Data Found",
                            GameQuestion = GameQuestion
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Response);
                    }
                    else
                    {
                        var Response = new
                        {
                            Status = false,
                            Message = "No Data Found",
                            GameQuestion = res
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Response);
                    }
                }
            }
            catch (Exception ex)
            {
                var Response = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// Get API for Game Question behalf of level and customer
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="LevelID"></param>
        /// <returns></returns>
        [Route("GetLavelDataCustomer")]
        [HttpGet]
        public HttpResponseMessage GetLavelDataCustomer(int CustomerID, int LevelID, string Language)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    List<currentleveldata> Leveldata = new List<currentleveldata>();
                    var QuestionData = db.GameQuestionDB.Where(x => x.GameLevelId == LevelID).ToList();
                    foreach (var Question in QuestionData)
                    {
                        var PassedLevel = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == CustomerID && x.LevelId == Question.GameLevelId && x.QuestionId == Question.GameQuestionId).ToList();
                        if (PassedLevel.Count == 0)
                        {
                            currentleveldata cd = new currentleveldata();
                            cd.GameQuestionId = Question.GameQuestionId;
                            cd.GameQuestionData = Language == "hindi" ? Question.GameQuestionHindi : Question.GameQuestionData;
                            cd.GameAnswer = Question.GameAnswer;
                            cd.QuestionPoints = Question.QuestionPoints;
                            cd.GameImage = Question.GameImage;
                            Leveldata.Add(cd);
                        }
                    }

                    if (Leveldata.Count > 0)
                    {
                        var Responce = new
                        {
                            Status = true,
                            Leveldata = Leveldata
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Responce);
                    }
                    else
                    {
                        var Responce = new
                        {
                            Status = false,
                            Leveldata = Leveldata
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Responce);
                    }
                }
            }
            catch (Exception ex)
            {
                var Responce = new
                {
                    Status = false,
                    Error = ex
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, Responce);
            }
        }

        /// <summary>
        /// Get API for all lavel and question
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="WarehouseID"></param>
        /// <returns></returns>
        [Route("GetLavelandQueData")]
        [HttpGet]
        public HttpResponseMessage GetLavelandQueData(int CustomerID, int WarehouseID)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    List<LavelandQueData> response = new List<LavelandQueData>();
                    var GetLevel = db.GameLevelDB.Where(x => x.Deleted == false && x.IsActive == true).ToList();
                    var GetCustGame = db.GameAttemptedQuestionDB.Where(x => x.Deleted == false && x.IsActive == true && x.CustomerId == CustomerID && x.WarehouseId == WarehouseID).ToList();
                    foreach (var item in GetLevel)
                    {
                        int levelid = item.GameLevelId;
                        string levelName = item.GameLevelName;
                        double levelPoints = item.GameLevelPoints;
                        DateTime createdDate = item.CreatedDate;
                        var count = GetCustGame.Where(x => x.LevelId == levelid).Count();
                        bool clear = count < 10 ? false : true;
                        LavelandQueData data = new LavelandQueData();
                        data.LevelID = levelid;
                        data.LevelName = levelName;
                        data.LevelPoints = levelPoints;
                        data.CreatedDate = createdDate;
                        data.QuestionCount = count;
                        data.LevelCleared = clear;
                        response.Add(data);
                    }
                    if (response.Count > 0)
                    {
                        var Responce = new
                        {
                            Status = true,
                            Leveldata = response
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Responce);
                    }
                    else
                    {
                        var Responce = new
                        {
                            Status = false,
                            Leveldata = "No Data Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Responce);
                    }
                }
            }
            catch (Exception ex)
            {
                var Responce = new
                {
                    Status = false,
                    Error = ex
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, Responce);
            }
        }

        /// <summary>
        /// Get API for Users Rating
        /// </summary>
        /// <param name="CityId"></param>
        /// <returns></returns>
        [Route("GetUsersRatings")]
        [HttpGet]
        public HttpResponseMessage GetUsersRatings(int CityId, int WarehouseID)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    if (CityId > 0)
                    {
                        List<UsersRatings> response = new List<UsersRatings>();
                        var GetUsersRatings = db.TotalEarnedPointsDB.Where(x => x.Deleted == false && x.IsActive == true && x.CityId == CityId).ToList().OrderByDescending(a => a.TotalPointsEarned);
                        foreach (var item in GetUsersRatings)
                        {
                            int CustomerId = item.CustomerId;
                            var GetCustomer = db.Customers.Where(x => x.Deleted == false && x.Cityid == CityId && x.CustomerId == CustomerId).SingleOrDefault();
                            string CustomerName = GetCustomer.Name;
                            string ShopName = GetCustomer.ShopName;
                            string ProfileImage = GetCustomer.UploadProfilePichure;
                            UsersRatings data = new UsersRatings();
                            data.CustomerId = CustomerId;
                            data.CustomerName = CustomerName;
                            data.ShopName = ShopName;
                            data.ProfileImage = ProfileImage;
                            data.CurrentLevel = item.CurrentLevel;
                            data.PointsEarned = item.TotalPointsEarned;
                            response.Add(data);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else if (WarehouseID > 0)
                    {
                        List<UsersRatings> response = new List<UsersRatings>();
                        var GetUsersRatings = db.TotalEarnedPointsDB.Where(x => x.Deleted == false && x.IsActive == true && x.WarehouseId == WarehouseID).ToList().OrderByDescending(a => a.TotalPointsEarned);
                        foreach (var item in GetUsersRatings)
                        {
                            int CustomerId = item.CustomerId;
                            var GetCustomer = db.Customers.Where(x => x.Deleted == false && x.Cityid == CityId && x.CustomerId == CustomerId).SingleOrDefault();
                            string CustomerName = GetCustomer.Name;
                            string ShopName = GetCustomer.ShopName;
                            string ProfileImage = GetCustomer.UploadProfilePichure;
                            UsersRatings data = new UsersRatings();
                            data.CustomerId = CustomerId;
                            data.CustomerName = CustomerName;
                            data.ShopName = ShopName;
                            data.ProfileImage = ProfileImage;
                            data.CurrentLevel = item.CurrentLevel;
                            data.PointsEarned = item.TotalPointsEarned;
                            response.Add(data);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        List<UsersRatings> response = new List<UsersRatings>();
                        var GetUsersRatings = db.TotalEarnedPointsDB.Where(x => x.Deleted == false && x.IsActive == true).ToList().OrderByDescending(a => a.TotalPointsEarned);
                        foreach (var item in GetUsersRatings)
                        {
                            int CustomerId = item.CustomerId;
                            var GetCustomer = db.Customers.Where(x => x.Deleted == false && x.CustomerId == CustomerId).SingleOrDefault();
                            string CustomerName = GetCustomer.Name;
                            string ShopName = GetCustomer.ShopName;
                            string ProfileImage = GetCustomer.UploadProfilePichure;
                            UsersRatings data = new UsersRatings();
                            data.CustomerId = CustomerId;
                            data.CustomerName = CustomerName;
                            data.ShopName = ShopName;
                            data.ProfileImage = ProfileImage;
                            data.CurrentLevel = item.CurrentLevel;
                            data.PointsEarned = item.TotalPointsEarned;
                            response.Add(data);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Post API for Insert Game Question
        /// </summary>
        /// <param name="gameAnswerData"></param>
        /// <returns></returns>
        [Route("InsertGameAttemptedQuestion/V1")]
        [HttpPost]
        public HttpResponseMessage GameAttemptedQuestionV1(GameAnswer LevelCrossData)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    //foreach (var LevelCrossData in gameAnswerData)
                    //{
                    #region Insertion in Game Attempted Question
                    GameAttemptedQuestion custGame = new GameAttemptedQuestion();
                    var cityName = db.Cities.Where(x => x.Cityid == LevelCrossData.CityId).Select(y => y.CityName).SingleOrDefault();
                    var PointsEarned = db.GameLevelDB.Where(x => x.GameLevelId == LevelCrossData.LevelId && x.Deleted == false && x.IsActive == true).Select(y => y.GameLevelPoints).SingleOrDefault();
                    foreach (var gamedata in LevelCrossData.GameAnswerData)
                    {
                        custGame.CustomerId = LevelCrossData.CustomerId;
                        custGame.WarehouseId = LevelCrossData.WarehouseId;
                        custGame.CompanyId = LevelCrossData.CompanyId;
                        custGame.LevelId = LevelCrossData.LevelId;
                        custGame.CityId = LevelCrossData.CityId;
                        custGame.CityName = cityName;
                        custGame.QuestionId = gamedata.QuestionId;
                        custGame.UserAnswer = gamedata.UserAnswer;
                        custGame.CreatedDate = indianTime;
                        custGame.IsActive = true;
                        custGame.Deleted = false;
                        db.GameAttemptedQuestionDB.Add(custGame);
                        db.Commit();
                    }
                    #endregion

                    #region Insertion in Level Crossed Details

                    var levelQuestion = db.GameAttemptedQuestionDB.Where(x => x.CustomerId == LevelCrossData.CustomerId && x.LevelId == LevelCrossData.LevelId).Count();
                    if (levelQuestion == 10)
                    {
                        LevelCrossedDetails LevelData = new LevelCrossedDetails();
                        LevelData.CustomerId = LevelCrossData.CustomerId;
                        LevelData.WarehouseId = LevelCrossData.WarehouseId;
                        LevelData.CompanyId = LevelCrossData.CompanyId;
                        LevelData.CurrentLevel = LevelCrossData.LevelId;
                        LevelData.CityId = LevelCrossData.CityId;
                        LevelData.CityName = cityName;
                        LevelData.PointsEarned = Convert.ToDouble(PointsEarned);
                        LevelData.CreatedDate = indianTime;
                        LevelData.IsActive = true;
                        LevelData.Deleted = false;
                        db.LevelCrossedDetailsDB.Add(LevelData);
                        db.Commit();

                        TotalEarnedPoints TotalEarnedPoint = db.TotalEarnedPointsDB.Where(x => x.CustomerId == LevelCrossData.CustomerId && x.WarehouseId == LevelCrossData.WarehouseId && x.CompanyId == LevelCrossData.CompanyId).FirstOrDefault();

                        if (TotalEarnedPoint != null)
                        {
                            TotalEarnedPoint.CurrentLevel = LevelCrossData.LevelId;
                            TotalEarnedPoint.TotalPointsEarned = TotalEarnedPoint.TotalPointsEarned + Convert.ToDouble(PointsEarned);
                            TotalEarnedPoint.UpdatedDate = indianTime;
                            db.TotalEarnedPointsDB.Attach(TotalEarnedPoint);
                            db.Entry(TotalEarnedPoint).State = EntityState.Modified;
                            db.Commit();
                        }
                        else
                        {
                            TotalEarnedPoints tep = new TotalEarnedPoints();
                            tep.CustomerId = LevelCrossData.CustomerId;
                            tep.CurrentLevel = LevelCrossData.LevelId;
                            tep.TotalPointsEarned = Convert.ToDouble(PointsEarned);
                            tep.CompanyId = LevelCrossData.CompanyId;
                            tep.WarehouseId = LevelCrossData.WarehouseId;
                            tep.CityId = LevelCrossData.CityId;
                            tep.CityName = cityName;
                            tep.IsActive = true;
                            tep.Deleted = false;
                            tep.CreatedDate = indianTime;
                            tep.UpdatedDate = indianTime;
                            db.TotalEarnedPointsDB.Add(tep);
                            db.Commit();
                        }
                    }
                    #endregion

                    #region Insertion in Game Wallet  
                    var IsWalletEdit = db.WalletDb.Where(x => x.CustomerId == LevelCrossData.CustomerId).SingleOrDefault();
                    if (IsWalletEdit != null)
                    {
                        CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                        try
                        {
                            customerwallethistory.CustomerId = LevelCrossData.CustomerId;
                            customerwallethistory.WarehouseId = LevelCrossData.WarehouseId;
                            customerwallethistory.Through = "Cleared Game Level " + LevelCrossData.LevelId + " Reward";
                            customerwallethistory.CompanyId = LevelCrossData.CompanyId;
                            if (Convert.ToDouble(PointsEarned) >= 0)
                            {
                                customerwallethistory.NewAddedWAmount = Convert.ToDouble(PointsEarned);
                                customerwallethistory.TotalWalletAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(PointsEarned);
                            }
                            customerwallethistory.UpdatedDate = indianTime;
                            customerwallethistory.TransactionDate = indianTime;
                            customerwallethistory.CreatedDate = indianTime;
                            db.CustomerWalletHistoryDb.Add(customerwallethistory);
                            db.Commit();
                        }
                        catch (Exception ex)
                        {

                        }

                        IsWalletEdit.TotalAmount = IsWalletEdit.TotalAmount + Convert.ToDouble(PointsEarned);
                        IsWalletEdit.UpdatedDate = indianTime;
                        IsWalletEdit.TransactionDate = indianTime;
                        db.WalletDb.Attach(IsWalletEdit);
                        db.Entry(IsWalletEdit).State = EntityState.Modified;
                        db.Commit();
                    }
                    else
                    {
                        Wallet wallet = new Wallet();
                        wallet.CompanyId = LevelCrossData.CompanyId;
                        wallet.CustomerId = LevelCrossData.CustomerId;
                        wallet.TransactionDate = indianTime;
                        wallet.TotalAmount = Convert.ToDouble(PointsEarned);
                        wallet.CreatedDate = indianTime;
                        wallet.UpdatedDate = indianTime;
                        wallet.Deleted = false;
                        db.WalletDb.Add(wallet);
                        db.Commit();

                        CustomerWalletHistory customerwallethistory = new CustomerWalletHistory();
                        try
                        {
                            customerwallethistory.CustomerId = LevelCrossData.CustomerId;
                            customerwallethistory.WarehouseId = LevelCrossData.WarehouseId;
                            customerwallethistory.Through = "Cleared Game Level " + LevelCrossData.LevelId + " Reward";
                            customerwallethistory.CompanyId = LevelCrossData.CompanyId;
                            if (Convert.ToDouble(PointsEarned) >= 0)
                            {
                                customerwallethistory.NewAddedWAmount = Convert.ToDouble(PointsEarned);
                                customerwallethistory.TotalWalletAmount = Convert.ToDouble(PointsEarned);
                            }
                            customerwallethistory.UpdatedDate = indianTime;
                            customerwallethistory.TransactionDate = indianTime;
                            customerwallethistory.CreatedDate = indianTime;
                            db.CustomerWalletHistoryDb.Add(customerwallethistory);
                            db.Commit();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    #endregion
                    // }

                    var res = new
                    {
                        Status = true,
                        Message = "Level Cleared"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                var res = new
                {
                    Status = false,
                    Message = ex
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
    }

    public class GameAnswer
    {
        public int LevelId { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public int CompanyId { get; set; }
        public List<GameAnswerList> GameAnswerData { get; set; }
    }

    public class GameAnswerList
    {
        public int QuestionId { get; set; }
        public bool UserAnswer { get; set; }
    }

    public class GameData
    {
        public int GameQuestionId
        {
            get; set;
        }
        public int GameLevelId
        {
            get; set;
        }
        public string GameQuestionData
        {
            get; set;
        }
        public string GameQuestionHindi { get; set; }
        public bool GameAnswer
        {
            get; set;
        }
        public double QuestionPoints
        {
            get; set;
        }
        public string GameLevelName
        {
            get; set;
        }
        public DateTime CreatedDate
        {
            get; set;
        }
        public string GameImage
        {
            get; set;
        }
    }

    public class GameAnswerData
    {
        public int LevelId { get; set; }
        public int QuestionId { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public bool UserAnswer { get; set; }
    }

    public class LevelCrossedData
    {
        public int CustomerId { get; set; }
        public int CurrentLevel { get; set; }
        public int PointsEarned { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
    }

    public class LavelandQueData
    {
        public int LevelID { get; set; }
        public string LevelName { get; set; }
        public double LevelPoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionCount { get; set; }
        public bool LevelCleared { get; set; }
    }

    public class UsersRatings
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }

        public string ProfileImage { get; set; }

        public int CurrentLevel { get; set; }
        public double PointsEarned { get; set; }
    }

    public class currentleveldata
    {
        public int GameQuestionId { get; set; }
        public string GameQuestionData { get; set; }
        public bool GameAnswer { get; set; }
        public double QuestionPoints { get; set; }
        public string GameImage { get; set; }
    }

    public class currentdata
    {
        public int GameLevelID { get; set; }
        public int GameQuestionId { get; set; }
        public string GameQuestion { get; set; }
        public bool GameAnswer { get; set; }
        public string GameImage { get; set; }
    }

}