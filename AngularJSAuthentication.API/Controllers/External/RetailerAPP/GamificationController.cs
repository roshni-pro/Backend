using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model.RetailerApp;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.DataContracts.External.GamificationDc;

namespace AngularJSAuthentication.API.Controllers.External.Gamification
{
    [RoutePrefix("api/Gamification")]
    public class GamificationController : ApiController
    {
        int IncreaseDay = 10;
        int StreakDuration = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public GamificationController()
        {
            MongoDbHelper<GameConditionMastersMongo> mongohelperIncreaseDay = new MongoDbHelper<GameConditionMastersMongo>();

            using (var db = new AuthContext())
            {
                var GameConditionMasters = mongohelperIncreaseDay.Select(x => x.IsActive == true && x.IsDeleted == false).ToList();

                IncreaseDay = GameConditionMasters.Where(x => x.Name == "IncreaseDay").Select(x => x.Value).FirstOrDefault();
                StreakDuration = GameConditionMasters.Where(x => x.Name == "StreakDuration").Select(x => x.Value).FirstOrDefault();
            }
        }


        #region getdataforgamification

        #region Not use this api
        [Route("GetData")]
        [HttpGet]
        public HttpResponseMessage GetData(int skip, int take)
        {
            using (var db = new AuthContext())
            {
                GameBucketRewards gb = new GameBucketRewards();
                var data = db.Database.SqlQuery<GameBucketRewardsDC>("Sp_GetDataForGameBucketrRewards ").ToList();
                //var data = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (data != null)
                {
                    if (skip == 0 && take == 0)
                    {
                        gb.gameBucketRewards = data.ToList();
                    }
                    else
                    {
                        gb.gameBucketRewards = data.Skip(skip).Take(take).ToList();
                    }
                    gb.TotalRecords = data.Count();
                    var res = new
                    {
                        status = true,
                        Message = gb
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        #endregion

        [Route("PostData")]
        [HttpPost]
        public HttpResponseMessage PostData(PostGameBucketRewards post)
        {


            var currentdate = DateTime.Now;

            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            using (var db = new AuthContext())
            {
                //GameBucketReward gameBucketReward = new GameBucketReward();
                var data = db.GameBucketRewards.Any(x => x.BucketNo == post.BucketNo && x.IsActive == true && x.IsDeleted == false);
                if (data)
                {
                    var res = new
                    {
                        status = false,
                        Message = "Bucket No is Already Exists"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                //if (post.GameConditionMasterId <= 0)
                //{
                //    var res = new
                //    {
                //        status = false,
                //        Message = "Please Select At least One Condition."
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
                if (post.value <= 0)
                {
                    var res = new
                    {
                        status = false,
                        Message = "Value Should be greater then 0."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                if (post.IsFix == false)
                {
                    if (post.StartDate == null)
                    {
                        var res = new
                        {
                            status = false,
                            Message = "Please Enter Start Date."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    if (post.EndDate == null)
                    {
                        var res = new
                        {
                            status = false,
                            Message = "Please Enter End Date."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                if (post != null)
                {
                    GameBucketReward br = new GameBucketReward();
                    br.BucketNo = post.BucketNo;
                    br.IsFix = post.IsFix;
                    br.RewardType = post.RewardType;
                    br.value = post.value;
                    br.StartDate = post.StartDate;
                    br.EndDate = post.EndDate;
                    br.CreatedDate = currentdate;
                    br.IsActive = true;
                    br.IsDeleted = false;
                    br.CreatedBy = userid;
                    br.RewardApproveStatus = post.RewardApproveStatus;
                    db.GameBucketRewards.Add(br);
                    foreach (var item in post.GameConditionLists)
                    {
                        BucketRewardCondition bucketRewardCondition = new BucketRewardCondition();
                        bucketRewardCondition.GameConditionMasterId = item.GameConditionMasterId;
                        bucketRewardCondition.GameBucketRewardId = br.Id;
                        bucketRewardCondition.value = item.value;
                        bucketRewardCondition.IsActive = true;
                        bucketRewardCondition.IsDeleted = false;
                        bucketRewardCondition.CreatedBy = userid;
                        bucketRewardCondition.CreatedDate = currentdate;
                        db.BucketRewardConditions.Add(bucketRewardCondition);
                    }
                    if (db.Commit() > 0)
                    {
                        var res = new
                        {
                            status = true,
                            Message = "Add Successfully"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            status = false,
                            Message = "Something Went Wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "Something Went Wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }

        }


        [Route("EditData")]
        [HttpPost]
        public HttpResponseMessage EditData(EditGameBucketRewards post)
        {
            try
            {

                var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                using (var db = new AuthContext())
                {
                    var res = new
                    {
                        status = true,
                        Message = ""
                    };
                    if (post.value <= 0)
                    {
                        res = new
                        {
                            status = false,
                            Message = "Value Should be greater then 0."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    //if (post.GameConditionMasterId <= 0)
                    //{
                    //    res = new
                    //    {
                    //        status = false,
                    //        Message = "Please Select At least One Condition."
                    //    };
                    //    return Request.CreateResponse(HttpStatusCode.OK, res);
                    //}
                    if (post.IsFix == false)
                    {
                        if (post.StartDate == null)
                        {
                            res = new
                            {
                                status = false,
                                Message = "Please Enter Start Date."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (post.EndDate == null)
                        {
                            res = new
                            {
                                status = false,
                                Message = "Please Enter End Date."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    var data = db.GameBucketRewards.FirstOrDefault(x => x.Id == post.BucketRewardConditionsID);
                    if (data != null)
                    {
                        data.RewardType = post.RewardType;
                        data.value = post.value;
                        data.IsFix = post.IsFix;
                        data.StartDate = post.StartDate;
                        data.EndDate = post.EndDate;
                        data.RewardApproveStatus = post.RewardApproveStatus;
                        db.Entry(data).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            res = new
                            {
                                status = true,
                                Message = "Edit Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new
                            {
                                status = false,
                                Message = "Something went wrong"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }

                    }
                    else
                    {
                        res = new
                        {
                            status = false,
                            Message = "No Data Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    //var data = db.BucketRewardConditions.Where(x => x.Id == post.BucketRewardConditionsID && x.IsActive == true && x.IsDeleted == false).Include(y => y.GameBucketRewards).SingleOrDefault();

                    //if (data != null)
                    //{
                    //    if (data.GameConditionMasterId != post.GameConditionMasterId)
                    //    {
                    //        var datas = db.BucketRewardConditions.Where(x => x.GameBucketRewardId == data.GameBucketRewardId  && x.IsActive == true && x.IsDeleted == false).Count();
                    //        if (datas > 0)
                    //        {
                    //            res = new
                    //            {
                    //                status = false,
                    //                Message = "Please Select Another Condition."
                    //            };
                    //            return Request.CreateResponse(HttpStatusCode.OK, res);

                    //        }
                    //        else
                    //        {
                    //            data.GameConditionMasterId = post.GameConditionMasterId;
                    //            data.value = post.GameValue;
                    //            data.ModifiedBy = userid;
                    //            data.ModifiedDate = DateTime.Now;
                    //            data.GameBucketRewards.IsFix = post.IsFix;
                    //            data.GameBucketRewards.RewardType = post.RewardType;
                    //            data.GameBucketRewards.StartDate = post.StartDate;
                    //            data.GameBucketRewards.EndDate = post.EndDate;
                    //            data.GameBucketRewards.RewardApproveStatus = post.RewardApproveStatus;
                    //            data.GameBucketRewards.value = post.value;
                    //            data.GameBucketRewards.ModifiedDate = DateTime.Now;
                    //            data.GameBucketRewards.ModifiedBy = userid;
                    //            db.Entry(data).State = EntityState.Modified;
                    //            if (db.Commit() > 0)
                    //            {
                    //                res = new
                    //                {
                    //                    status = true,
                    //                    Message = "Edit Successfully"
                    //                };
                    //                return Request.CreateResponse(HttpStatusCode.OK, res);
                    //            }
                    //            else
                    //            {
                    //                res = new
                    //                {
                    //                    status = false,
                    //                    Message = "Something Went wrong"
                    //                };
                    //                return Request.CreateResponse(HttpStatusCode.OK, res);
                    //            }

                    //        }
                    //    }
                    //    else
                    //    {
                    //        data.GameConditionMasterId = post.GameConditionMasterId;
                    //        data.value = post.GameValue;
                    //        data.ModifiedBy = userid;
                    //        data.ModifiedDate = DateTime.Now;
                    //        data.GameBucketRewards.IsFix = post.IsFix;
                    //        data.GameBucketRewards.RewardType = post.RewardType;
                    //        data.GameBucketRewards.StartDate = post.StartDate;
                    //        data.GameBucketRewards.EndDate = post.EndDate;
                    //        data.GameBucketRewards.RewardApproveStatus = post.RewardApproveStatus;
                    //        data.GameBucketRewards.value = post.value;
                    //        data.GameBucketRewards.ModifiedDate = DateTime.Now;
                    //        data.GameBucketRewards.ModifiedBy = userid;
                    //        db.Entry(data).State = EntityState.Modified;
                    //    }
                    //    if (db.Commit() > 0)
                    //    {
                    //        res = new
                    //        {
                    //            status = true,
                    //            Message = "Edit Successfully"
                    //        };
                    //        return Request.CreateResponse(HttpStatusCode.OK, res);
                    //    }
                    //    else
                    //    {
                    //        res = new
                    //        {
                    //            status = false,
                    //            Message = "Something Went wrong"
                    //        };
                    //        return Request.CreateResponse(HttpStatusCode.OK, res);
                    //    }
                    //}
                    //else
                    //{

                    //    res = new
                    //    {
                    //        status = false,
                    //        Message = "Please Select Another Condition"
                    //    };
                    //    return Request.CreateResponse(HttpStatusCode.OK, res);
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[Route("EditBucketDataWithConditions")]
        //[HttpPost]
        //public HttpResponseMessage EditBucketDataWithConditions(EditGameBucketRewardsWithConditions postData)
        //{
        //    var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


        //    using (var db = new AuthContext())
        //    {
        //        var res = new
        //        {
        //            status = true,
        //            Message = ""
        //        };
        //        if (postData.value <= 0)
        //        {
        //            res = new
        //            {
        //                status = false,
        //                Message = "Value Should be greater then 0."
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //        if (postData.IsFix == false)
        //        {
        //            if (postData.StartDate == null)
        //            {
        //                res = new
        //                {
        //                    status = false,
        //                    Message = "Please Enter Start Date."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            if (postData.EndDate == null)
        //            {
        //                res = new
        //                {
        //                    status = false,
        //                    Message = "Please Enter End Date."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //        var data = db.GameBucketRewards.Where(x => x.Id == postData.BucketRewardConditionsID).FirstOrDefault();
        //        if (data != null)
        //        {
        //            data.RewardType = postData.RewardType;
        //            data.value = postData.value;
        //            data.IsFix = postData.IsFix;
        //            data.StartDate = postData.StartDate;
        //            data.EndDate = postData.EndDate;
        //            data.RewardApproveStatus = postData.RewardApproveStatus;
        //            foreach (var item in postData.GameConditionLists)
        //            {

        //                var conditionData = db.BucketRewardConditions.Where(x => x.GameBucketRewardId == postData.BucketRewardConditionsID && x.GameConditionMasterId == item.GameConditionMasterId).FirstOrDefault();
        //                if (conditionData == null)
        //                {
        //                    BucketRewardCondition bucketRewardCondition = new BucketRewardCondition();
        //                    bucketRewardCondition.GameConditionMasterId = item.GameConditionMasterId;
        //                    bucketRewardCondition.GameBucketRewardId = data.Id;
        //                    bucketRewardCondition.value = item.value;
        //                    bucketRewardCondition.IsActive = true;
        //                    bucketRewardCondition.IsDeleted = false;
        //                    bucketRewardCondition.CreatedBy = userid;
        //                    bucketRewardCondition.CreatedDate = DateTime.Now;
        //                    db.BucketRewardConditions.Add(bucketRewardCondition);
        //                }
        //            }
        //            db.Entry(data).State = EntityState.Modified;
        //            if (db.Commit() > 0)
        //            {
        //                res = new
        //                {
        //                    status = true,
        //                    Message = "Edit Successfully"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                res = new
        //                {
        //                    status = false,
        //                    Message = "Something went wrong"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }

        //        }
        //        else
        //        {
        //            res = new
        //            {
        //                status = false,
        //                Message = "No Data Found"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //    }
        //}

        [Route("DeleteGameConditions")]
        [HttpGet]
        public HttpResponseMessage DeleteGameConditions(long BucketRewardConditionsID)
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var res = new
                {
                    status = true,
                    Message = ""
                };
                var BucketRewardConditiondata = db.BucketRewardConditions.FirstOrDefault(x => x.Id == BucketRewardConditionsID && x.IsActive == true && x.IsDeleted == false);
                if (BucketRewardConditiondata != null)
                {
                    var count = db.BucketRewardConditions.Where(x => x.IsActive == true && x.IsDeleted == false && x.GameBucketRewardId == BucketRewardConditiondata.GameBucketRewardId).Count();
                    if (count > 1)
                    {
                        BucketRewardConditiondata.IsActive = false;
                        BucketRewardConditiondata.IsDeleted = true;
                        BucketRewardConditiondata.ModifiedDate = DateTime.Now;
                        BucketRewardConditiondata.ModifiedBy = userid;
                    }
                    else
                    {
                        BucketRewardConditiondata.IsActive = false;
                        BucketRewardConditiondata.IsDeleted = true;
                        BucketRewardConditiondata.ModifiedDate = DateTime.Now;
                        BucketRewardConditiondata.ModifiedBy = userid;
                        BucketRewardConditiondata.GameBucketRewards.IsActive = false;
                        BucketRewardConditiondata.GameBucketRewards.IsDeleted = true;
                        BucketRewardConditiondata.GameBucketRewards.ModifiedDate = DateTime.Now;
                        BucketRewardConditiondata.GameBucketRewards.ModifiedBy = userid;
                    }
                    db.Entry(BucketRewardConditiondata).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        res = new
                        {
                            status = true,
                            Message = "Deleted Successfully"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new
                        {
                            status = false,
                            Message = "Something Went Wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new
                    {
                        status = false,
                        Message = "Data Not Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }


        #endregion

        [Route("GetRewardType")]
        [HttpGet]
        public async Task<List<RewardTypeDc>> GetRewardTypeList()
        {
            List<RewardTypeDc> returnRewardType = new List<RewardTypeDc>();

            using (var context = new AuthContext())
            {
                string sqlquery = "SELECT * from vwRewardType ";
                returnRewardType = context.Database.SqlQuery<RewardTypeDc>(sqlquery).ToList();
            }

            return returnRewardType;
        }

        [Route("GetDataForConditionMasters")]
        [HttpGet]
        public HttpResponseMessage GetDataForConditionMasters()
        {
            using (var db = new AuthContext())
            {
                GameConditionMaster gm = new GameConditionMaster();
                var data = db.GameConditionMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.SqlQuery != "").ToList();
                if (data != null)
                {

                    var res = new
                    {
                        status = true,
                        Message = data
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("PostBucketCondition")]
        [HttpPost]
        public HttpResponseMessage PostBucketCondition(long GameConditionMasterId, long GameBucketRewardId, int value)
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var res = new
                {
                    status = true,
                    Message = ""
                };

                BucketRewardCondition br = new BucketRewardCondition();
                if (GameConditionMasterId > 0 && GameBucketRewardId > 0)
                {
                    var data = db.BucketRewardConditions.FirstOrDefault(x => x.GameConditionMasterId == GameConditionMasterId && x.GameBucketRewardId == GameBucketRewardId && x.IsActive == true && x.IsDeleted == false);
                    if (data != null)
                    {
                        res = new
                        {
                            status = false,
                            Message = "Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        br.value = value;
                        br.GameConditionMasterId = GameConditionMasterId;
                        br.GameBucketRewardId = GameBucketRewardId;
                        br.CreatedDate = DateTime.Now;
                        br.IsActive = true;
                        br.IsDeleted = false;
                        br.CreatedBy = userid;
                        db.BucketRewardConditions.Add(br);
                        if (db.Commit() > 0)
                        {
                            res = new
                            {
                                status = true,
                                Message = "Add Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new
                            {
                                status = false,
                                Message = "Something Went Wrong"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }

                }
                else
                {
                    res = new
                    {
                        status = false,
                        Message = "Something Went Wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        #region Not Use This API
        [HttpGet]
        [Route("GetAllGameCondition")]
        public HttpResponseMessage GetAllGameCondition(int skip, int take, int BucketNo)
        {
            using (var db = new AuthContext())
            {
                GameCondition game = new GameCondition();

                var bno = new SqlParameter("@BucketNo", BucketNo);
                var data = db.Database.SqlQuery<GameConditionDC>("Sp_getAllCondition @BucketNo", bno).ToList();
                //var fskip = new SqlParameter("@skip", skip);
                //var ftake = new SqlParameter("@take", take);
                //var data = db.Database.SqlQuery<GameConditionDC>("Sp_getAllCondition @BucketNo, @skip, @take", bno, fskip, ftake).ToList();

                if (data != null)
                {
                    if (skip == 0 && take == 0)
                    {
                        game.gameCondition = data.ToList();
                    }
                    else
                    {
                        game.gameCondition = data.Skip(skip).Take(take).ToList();
                    }
                    game.TotalRecords = data.Count();
                    var res = new
                    {
                        status = true,
                        Message = game
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion 

        [HttpGet]
        [Route("DeleteGameCondition")]
        public HttpResponseMessage DeleteGameCondition(long Id)
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var res = new
                {
                    status = true,
                    Message = ""
                };
                GameCondition game = new GameCondition();
                var data = db.BucketRewardConditions.FirstOrDefault(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false);
                if (data != null)
                {
                    data.IsActive = false;
                    data.IsDeleted = true;
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = userid;
                    db.Entry(data).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        res = new
                        {
                            status = true,
                            Message = "Delete Successfully"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new
                        {
                            status = false,
                            Message = "Something Went Wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [HttpGet]
        [Route("GetCRMDataJob")]
        public async Task<bool> GetCRMDataJob()
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var curdate = DateTime.Now;
                    MongoDbHelper<GameCustomerBucketHdr> mongohelper = new MongoDbHelper<GameCustomerBucketHdr>();

                    var SkcodeList = mongohelper.Select(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.SkCode).Distinct().ToList();


                    var today = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    string query = $"SELECT ccode skcode,bucketno,lastorderdate,startdate,enddate   from sk_crm_platform_ccodedata where enddate >= '{today}'";
                    ElasticSqlHelper<ElasticCRMData> elasticSqlHelperData = new ElasticSqlHelper<ElasticCRMData>();

                    //----_S---- For CRM -------
                    var CRMData = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());
                    //var CRMData = context.Customers.Where(x => x.Active == true && x.Deleted == false).ToList();

                    //var CRMData = context.Database.SqlQuery<crmjobdc>("EXEC spGetCustomerList").ToList();
                    //----E-----------------

                    var data = CRMData.Where(x => !SkcodeList.Contains(x.skcode)).ToList();
                    var custIds = data.Select(x => x.skcode).Distinct().ToList();
                    var CustomerList = context.Customers.Where(x => custIds.Contains(x.Skcode)).Select(x => new { x.CustomerId, x.Skcode }).Distinct().ToList();


                    //string sql = "Select Value from GameConditionMasters with(Nolock) Where Name='IncreaseDay' and IsActive=1 and IsDeleted=0 ";
                    //int IncreaseDay = context.Database.SqlQuery<Int32>(sql).FirstOrDefault();

                    List<GameCustomerBucketHdr> cust = new List<GameCustomerBucketHdr>();

                    foreach (var item in data)
                    {
                        GameCustomerBucketHdr obj = new GameCustomerBucketHdr();
                        obj.SkCode = item.skcode;
                        obj.CustomerId = CustomerList.Where(x => x.Skcode.ToLower() == item.skcode.ToLower()).Select(x => x.CustomerId).FirstOrDefault();
                        obj.CRMBucketNo = item.bucketno + 1;
                        obj.BucketNo = item.bucketno + 1;
                        obj.NextBucketNo = 0;
                        obj.GameBucketNo = 1;
                        obj.LastOrderDate = item.lastorderdate.Date == curdate.Date ? curdate.AddDays(-1) : item.lastorderdate.Date;
                        //obj.BucketStartDate = curdate;
                        //obj.BucketEndDate = curdate.AddDays(IncreaseDay - 1);
                        obj.BucketStartDate = curdate.AddDays(-1);
                        obj.BucketEndDate = curdate.AddDays(-1);

                        obj.CreatedDate = DateTime.Now;
                        obj.IsActive = true;
                        obj.IsDeleted = false;

                        cust.Add(obj);
                    };
                    var flag = mongohelper.InsertMany(cust);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }


        [HttpGet]
        [Route("GetCustomerBucketGames")]
        public async Task<RetailerBucketGame> GetCustomerBucketGames(int CustomerId, int warehouseId, int skip, int take, string lang)
        {
            var todaydate = DateTime.Now.Date;
            RetailerBucketGame retailerBucketGame = new RetailerBucketGame();

            int pageno = (skip == 0 ? 1 : (skip / take) + 1);
            //var buckets = Enumerable.Range(skip + 1, take * pageno).Select(x => (x - 1) + 1).ToList();
            var buckets = Enumerable.Range(skip + 1, take).Select(x => (x - 1) + 1).ToList();

            MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();

            MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
            var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

            //var bucketcustomers = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false).GroupBy(x => x.BucketNo).Select(x => new { BucekNo = x.Key, TotalCustomers = x.Count() }).ToList();
            var bucketcustomers = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false).GroupBy(x => x.NextBucketNo >= 0 ? x.GameBucketNo - 1 : x.GameBucketNo).Select(x => new { BucekNo = x.Key, TotalCustomers = x.Count() }).ToList();

            MongoDbHelper<GameStreakCustomerTransaction> mongoDbHelperStreakCustomerTransaction = new MongoDbHelper<GameStreakCustomerTransaction>();
            var dataCustomerStreakPeridList = mongoDbHelperStreakCustomerTransaction.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();

            if (dataCustomerBucketHdrList != null)
            {
                using (var db = new AuthContext())
                {
                    //long GameBucketRewardId = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.BucketNo==dataCustomerBucketHdrList.BucketNo && x.GameBucketRewardId>0 && x.IsActive == true && x.IsDeleted == false).Select (x => x.GameBucketRewardId).Distinct().FirstOrDefault();

                    int start = dataCustomerBucketHdrList.CRMBucketNo + (skip == 0 ? 0 : (skip + 1));
                    int end = dataCustomerBucketHdrList.CRMBucketNo + take * pageno;
                    GameBucketReward gameBucketReward = new GameBucketReward();

                    skip = (dataCustomerBucketHdrList.CRMBucketNo) + skip;
                    double pagenoReset = (skip == 0 ? 1 : (Convert.ToDouble(skip) / Convert.ToDouble(take)) + 1);

                    //var fPageno = new SqlParameter("@pageno", pageno);
                    var fPageno = new SqlParameter("@pageno", pagenoReset);
                    var fskip = new SqlParameter("@skip", skip);
                    var ftake = new SqlParameter("@take", take);
                    var bucketConditions = db.Database.SqlQuery<GameBucketConditionData>("exec sp_bucketConditions @pageno, @skip, @take", fPageno, fskip, ftake).ToList();

                    var gameBuckets = buckets.Select(x =>
                            new GameBucket
                            {
                                BucketNo = x,
                                CRMBucketNo = (dataCustomerBucketHdrList.CRMBucketNo + (x - 1)),
                                Status = "InComplete"
                            }
                           ).ToList();
                    if (bucketConditions != null)
                    {
                        var dataOrderDtlList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).GroupBy(x => new { x.GameBucketNo, x.RewardCredit, x.RewardStatus, x.BucketNo });


                        //var rewards = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                        var rewards = db.GameBucketRewards.ToList();

                        var dataOrderDtlBucketList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.GameBucketRewardId > 0
                        && x.IsActive == true && x.IsDeleted == false).GroupBy(x => new { x.BucketNo, x.GameBucketNo, x.GameBucketRewardId }).ToList();


                        gameBuckets.ForEach(x =>
                        {
                            List<GameBucketRewardCondition> bucketRewardConditionList = new List<GameBucketRewardCondition>();

                            if (bucketcustomers.Any(y => y.BucekNo == x.BucketNo))
                            {
                                x.TotalCustomers = bucketcustomers.FirstOrDefault(y => y.BucekNo == x.BucketNo).TotalCustomers;
                            }

                            //x.RewardType = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).RewardType == 0 ? "WalletPoint" : "Other";
                            //x.RewardValue = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).value != 0 ? rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).value : 0;
                            if (dataOrderDtlBucketList.Any(z => z.Key.BucketNo == x.CRMBucketNo))
                            {
                                x.RewardType = rewards.FirstOrDefault(y => y.Id == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId) != null && rewards.FirstOrDefault(y => y.Id == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId).RewardType == 0 ? "WalletPoint" : "Other";
                                x.RewardValue = rewards.FirstOrDefault(y => y.Id == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId) != null && rewards.FirstOrDefault(y => y.Id == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId).value != 0 ? rewards.FirstOrDefault(y => y.Id == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId).value : 0;
                                x.GameBucketRewardConditions = bucketConditions.Where(y => y.GameBucketRewardId == dataOrderDtlBucketList.FirstOrDefault(z => z.Key.BucketNo == x.CRMBucketNo).Key.GameBucketRewardId).Select(y => new GameBucketRewardCondition
                                {
                                    ConditionId = y.ConditionId,
                                    AppDesc = y.AppDesc,
                                    Value = y.ConditionValue  //y.Value
                                }).ToList();

                                string BucketStatus = "";
                                //Current Bucket
                                if (dataOrderDtlList.Where(z => z.Key.BucketNo == x.CRMBucketNo && z.Key.BucketNo == dataCustomerBucketHdrList.BucketNo).Select(z => z.Key.RewardStatus).Count() > 0)
                                {
                                    BucketStatus = dataOrderDtlList.Where(z => z.Key.BucketNo == x.CRMBucketNo && z.Key.BucketNo == dataCustomerBucketHdrList.BucketNo).Select(z => z.Key.RewardStatus).FirstOrDefault();
                                    if (BucketStatus == "Cancelled") { BucketStatus = "InProcess"; }
                                }
                                else
                                {
                                    string curstatus = "";
                                    //Future Bucket
                                    curstatus = dataOrderDtlList.Where(z => z.Key.BucketNo == x.CRMBucketNo && z.Key.BucketNo > dataCustomerBucketHdrList.BucketNo).Select(z => z.Key.RewardStatus).FirstOrDefault();
                                    if (curstatus == "InProcess")
                                    { BucketStatus = x.Status; }
                                    else
                                    {
                                        //Past Bucket
                                        curstatus = dataOrderDtlList.Where(z => z.Key.BucketNo == x.CRMBucketNo && z.Key.BucketNo < dataCustomerBucketHdrList.BucketNo).Select(z => z.Key.RewardStatus).FirstOrDefault();
                                        //if (curstatus != "InProcess") { BucketStatus = curstatus; }
                                        if (curstatus == "InProcess")
                                        { BucketStatus = "NotFullFill"; }
                                        else
                                        //{ BucketStatus = x.Status; }
                                        {
                                            if (curstatus != "")
                                            { BucketStatus = curstatus; }
                                            else
                                            { BucketStatus = x.Status; }
                                        }
                                    }
                                }
                                x.Status = BucketStatus;
                            }
                            else
                            {
                                x.RewardType = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false).RewardType == 0 ? "WalletPoint" : "Other";
                                x.RewardValue = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false).value != 0 ? rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false).value : 0;
                                x.GameBucketRewardConditions = bucketConditions.Where(y => y.bucketNo == x.CRMBucketNo && y.IsActive == true && y.IsDeleted == false).Select(y => new GameBucketRewardCondition
                                {
                                    ConditionId = y.ConditionId,
                                    AppDesc = y.AppDesc,
                                    Value = y.ConditionValue  //y.Value
                                }).ToList();
                            }
                            x.GameBucketRewardConditions.Add(new GameBucketRewardCondition
                            {
                                ConditionId = 0,
                                AppDesc = "Place an order every " + IncreaseDay + " days",
                                Value = 0,
                            });
                        });
                    }
                    retailerBucketGame.GameBucket = gameBuckets;
                }


                //retailerBucketGame.CurrentBucket = dataCustomerBucketHdrList.NextBucketNo == 0 ? dataCustomerBucketHdrList.GameBucketNo : dataCustomerBucketHdrList.GameBucketNo - 1;
                retailerBucketGame.CurrentBucket = dataCustomerBucketHdrList.NextBucketNo > 0 ? dataCustomerBucketHdrList.GameBucketNo - 1
                    : dataCustomerBucketHdrList.GameBucketNo == 1 && dataCustomerBucketHdrList.NextBucketNo == 0 ? 1 : dataCustomerBucketHdrList.GameBucketNo;
                ////retailerBucketGame.CurrentBucket = dataCustomerBucketHdrList.NextBucketNo == 0 ? dataCustomerBucketHdrList.BucketNo : dataCustomerBucketHdrList.NextBucketNo - 1;
                //retailerBucketGame.LevelUpBucket = dataCustomerBucketHdrList.NextBucketNo != 0 ? dataCustomerBucketHdrList.GameBucketNo : 0;
                retailerBucketGame.LevelUpBucket = dataCustomerBucketHdrList.NextBucketNo != 0 ? dataCustomerBucketHdrList.GameBucketNo : retailerBucketGame.CurrentBucket;

                int custbucketperioddate = dataCustomerBucketHdrList.BucketEndDate.Date >= todaydate ? Convert.ToInt32((dataCustomerBucketHdrList.BucketEndDate.Date - todaydate).TotalDays) + 1 : 0;
                // retailerBucketGame.NextOrderDay = dataCustomerStreakPeridList == null && dataCustomerStreakPeridList.Count == 0 ? IncreaseDay : (custbucketperioddate <= IncreaseDay ? custbucketperioddate : 0);
                retailerBucketGame.NextOrderDay = custbucketperioddate;

                var dataOrderDtl = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.RewardStatus == "Completed" && x.IsDeleted == false).GroupBy(x => new { x.GameBucketNo, x.RewardCredit, x.RewardStatus, x.BucketNo });
                if (dataOrderDtl != null)
                {
                    foreach (var item in retailerBucketGame.GameBucket)
                    {
                        //if (dataOrderDtl.Any(x => x.Key.GameBucketNo == item.BucketNo))
                        if (dataOrderDtl.Any(x => x.Key.BucketNo == item.CRMBucketNo))
                        {
                            item.Status = dataOrderDtl.FirstOrDefault(x => x.Key.BucketNo == item.CRMBucketNo).Key.RewardStatus;
                            item.RewardCredited = dataOrderDtl.FirstOrDefault(x => x.Key.BucketNo == item.CRMBucketNo).Key.RewardCredit;
                        }
                        //if (retailerBucketGame.CurrentBucket == item.BucketNo && item.GameBucketRewardConditions != null && item.GameBucketRewardConditions.Any())
                        if (retailerBucketGame.CurrentBucket == item.CRMBucketNo && item.GameBucketRewardConditions != null && item.GameBucketRewardConditions.Any())
                        {
                            using (var db = new AuthContext())
                            {
                                //var custGameConditions = db.GameCurrentLevelProgresses.Where(x => x.CustomerId == CustomerId && x.BucketNo == item.BucketNo && x.IsActive).Select(x => new { x.ConditionId, x.SqlOutPut }).ToList();
                                var custGameConditions = db.GameCurrentLevelProgresses.Where(x => x.CustomerId == CustomerId && x.BucketNo == item.CRMBucketNo && x.IsActive).Select(x => new { x.ConditionId, x.SqlOutPut }).ToList();
                                foreach (var condition in item.GameBucketRewardConditions)
                                {
                                    if (custGameConditions.Any(x => x.ConditionId == condition.ConditionId))
                                    {
                                        condition.AchiveValue = custGameConditions.FirstOrDefault(x => x.ConditionId == condition.ConditionId).SqlOutPut;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return retailerBucketGame;
        }

        private List<GameStreakLevelConfigDetailDc> GetStreakConfiguration(AuthContext db, int BucketNo, DateTime? createDt, int IsActiveCurrent)
        {
            var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
            var CreatedDate = new SqlParameter("@CreatedDate", createDt ?? (object)DBNull.Value);
            var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent);

            var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetailAll @BucketNoFrom,@CreatedDate, @IsActiveCurrent", fBucketNoFrom, CreatedDate, fIsActiveCurrent).ToList();
            return SqlStreakLevelConfigMasters;
        }

        [HttpGet]
        [Route("GetCustomerGameStreak")]
        public async Task<GameMainListDc> GetCustomerGameStreak(int CustomerId, string lang)
        {
            GameMainListDc retailerBucketGame = new GameMainListDc();
            using (var db = new AuthContext())
            {

                //int streakConfig = db.GameStreakLevelConfigMasters.Count(x => x.IsActive == true && x.IsDeleted == false);
                //if (streakConfig == 0)
                //{
                //    return retailerBucketGame;
                //}
                DateTime curDate = DateTime.Now.Date;

                #region Customer wise Streak Level Data


                MongoDbHelper<GameStreakLevelRewardValue> mongoDbHelperStreakRewardCredited = new MongoDbHelper<GameStreakLevelRewardValue>();

                MongoDbHelper<GameStreakCustomerTransaction> mongoDbHelperStreakCustomerTransaction = new MongoDbHelper<GameStreakCustomerTransaction>();
                var dataCustomerStreakPeridList = mongoDbHelperStreakCustomerTransaction.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();

                if (dataCustomerStreakPeridList != null && dataCustomerStreakPeridList.Count > 0)
                {

                    //int StreakDuration = 0;
                    //StreakDuration = db.GameConditionMasters.Where(x => x.Name == "StreakDuration" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();

                    //var StreakLevel = Enumerable.Range(1, 9).Select(x => (x - 1) + 1).ToList();
                    var StreakLevel = Enumerable.Range(1, StreakDuration).Select(x => (x - 1) + 1).ToList();

                    int IsActiveCurrent1 = 0;
                    Int32 BucketNo = dataCustomerStreakPeridList.Select(x => x.BucketNo).Distinct().FirstOrDefault();
                    int BucketNoFrom = 0;

                    //int BucketNoTo = 0; int skip1 = 0; int take1 = 0; 
                    //var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
                    //var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                    //var fskip = new SqlParameter("@skip", skip1);
                    //var ftake = new SqlParameter("@take", take1);
                    //var createDate = new SqlParameter("@CreatedDate", DBNull.Value);
                    //var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);                   
                    //var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, createDate, fIsActiveCurrent).ToList();
#if !DEBUG
                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                var SqlStreakLevelConfigMst = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.GameConfigCacheKey(), () => GetStreakConfiguration(db, BucketNoFrom,null,IsActiveCurrent1));
#else
                    var SqlStreakLevelConfigMst = GetStreakConfiguration(db, BucketNoFrom, null, IsActiveCurrent1);
#endif
                    var SqlStreakLevelConfigMasters = SqlStreakLevelConfigMst.Where(x => BucketNo >= x.BucketNoFrom && BucketNo <= x.BucketNoTo).ToList(); 

                    if (SqlStreakLevelConfigMasters != null && SqlStreakLevelConfigMasters.Count > 0)
                    {
                        var dataStreakRewardCreditedList = mongoDbHelperStreakRewardCredited.Select(x => x.CustomerId == CustomerId && x.BucketNo == BucketNo && x.IsActive == true && x.IsDeleted == false).ToList();
                        var dataRewardCreditedIDs = dataStreakRewardCreditedList.Select(x => x.GameStreakLevelConfigMasterId).ToList();

                        var dataStreakLevelConfigMasters = SqlStreakLevelConfigMasters.Where(x => dataRewardCreditedIDs.Contains(x.Id)).ToList();

                        retailerBucketGame.CustomerStreakDataList = StreakLevel.Select(x =>
                            new GameStreakDc
                            {
                                StreakId = x,
                                Status = "InComplete"
                            }
                           ).ToList();

                        retailerBucketGame.CustomerStreakDataList.ForEach(x =>
                        {
                            x.BucketNo = BucketNo;
                            //x.StreakBucketStartDate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketStartDate;
                            //x.StreakBucketEndDate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketEndDate;
                            var startdate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketStartDate;
                            var Enddate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketEndDate;
                            //x.DaysLeft = (curDate >= startdate.Date && Enddate.Date >= curDate) ? (Enddate.Date - curDate).Days : (Enddate.Date - curDate).Days;
                            x.DaysLeft = (curDate >= startdate.Date && Enddate.Date >= curDate) ? (Enddate.Date - curDate).Days + 1 : 0;

                            var LeftStreakPeriod = dataCustomerStreakPeridList.Where(d => d.BucketEndDate < curDate && d.StreakId == x.StreakId).ToList();
                            x.Status = dataCustomerStreakPeridList.Count(y => y.StreakId == x.StreakId && y.IsRewardProvided) > 0 ? "Completed"
                            //: x.DaysLeft > 0 ? "InProcess"
                            : (curDate >= startdate.Date && Enddate.Date >= curDate) ? "InProcess"
                            : dataCustomerStreakPeridList.Count(y => y.StreakId == x.StreakId && !y.IsRewardProvided && y.BucketEndDate < curDate) > 0 ? "NotFullFill" : x.Status;

                            // x.Status = dataCustomerStreakPeridList.Count(y => y.StreakId == x.StreakId && y.IsRewardProvided) > 0 ? "Completed" : x.DaysLeft > 0 ? "InProcess" : x.Status;
                            //x.RewardCredited = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).RewardValueCr : 0;
                            x.RewardCredited = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).RewardValue : 0;
                            // x.GameStreakLevelConfigMasterId = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).GameStreakLevelConfigMasterId : 0;
                            var GameStreakLevelConfigMasterId = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).GameStreakLevelConfigMasterId : 0;
                            x.StreakIdFrom = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).StreakIdFrom : 0;
                            x.StreakIdTo = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).StreakIdTo : 0;
                            x.IndivitualRewardValue = SqlStreakLevelConfigMasters.FirstOrDefault(z => z.Type == "Level-Individual" && z.LevelNo == x.StreakId && z.BucketNoFrom <= x.BucketNo && z.BucketNoTo >= x.BucketNo && dataRewardCreditedIDs.Contains(z.Id)) != null ? SqlStreakLevelConfigMasters.FirstOrDefault(z => z.Type == "Level-Individual" && z.LevelNo == x.StreakId && z.BucketNoFrom <= x.BucketNo && z.BucketNoTo >= x.BucketNo && dataRewardCreditedIDs.Contains(z.Id)).Reward : "";

                            x.GameStreakConfigData = dataStreakLevelConfigMasters.Where(z => z.Id == GameStreakLevelConfigMasterId && z.LevelNo == x.StreakId).Select(y => new GameStreakConfig
                            {
                                Id = y.Id,
                                Type = y.Type,
                                RewardTypeName = y.RewardTypeName,
                                Reward = y.Reward,
                                Condition = y.Condition,
                                StreakDescr = y.StreakDescr

                            }).ToList();
                        });

                        #region StreakConfigDataList
                        var StreakConfigIds = dataCustomerStreakPeridList.Select(x => x.GameStreakLevelConfigMasterId).Distinct().FirstOrDefault();
                        var dataStreakLevelConfigMastersAll = SqlStreakLevelConfigMasters.Where(x => StreakConfigIds.Contains(x.Id)).ToList();

                        int TotalStreakDuration = SqlStreakLevelConfigMasters.Where(x => StreakConfigIds.Contains(x.Id) && x.OutOf_OutOfBucket > 0).Select(y => y.OutOf_OutOfBucket).FirstOrDefault();
                        //int TillPeriodFullFill = dataCustomerStreakPeridList.Where(x => x.BucketEndDate.Date <= curDate).Count();
                        //var StreakStartdate = dataCustomerStreakPeridList.Select(x => x.BucketStartDate.Date).FirstOrDefault();
                        var StreakEnddate = dataCustomerStreakPeridList.Select(x => x.BucketEndDate.Date).LastOrDefault();
                        //TimeSpan  TotStreakDurationTo = StreakEnddate.Date  - StreakStartdate.Date;
                        TimeSpan TotStreakDurationFrom = StreakEnddate.Date - curDate;
                        int TillPeriodFullFill = Convert.ToInt32(TotStreakDurationFrom.TotalDays); //Convert.ToInt32(TotStreakDurationTo.TotalDays - TotStreakDurationFrom.TotalDays);

                        var customerStreaks = dataCustomerStreakPeridList.Where(x => !x.IsExpired && x.IsActive == true && x.IsDeleted == false).ToList();
                        var applicablebuckets = customerStreaks.Where(x => x.BucketEndDate.Date <= curDate);

                        var applicablebucketsFilltered = applicablebuckets.Select(x => new GameStreakCustomerTransactionDc
                        {
                            BucketEndDate = x.BucketEndDate,
                            BucketNo = x.BucketNo,
                            BucketStartDate = x.BucketStartDate,
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            CustomerId = x.CustomerId,
                            GameStreakLevelConfigMasterId = x.GameStreakLevelConfigMasterId,
                            //Id=x.Id, 
                            IsActive = x.IsActive,
                            IsDeleted = x.IsDeleted,
                            IsExpired = x.IsExpired,
                            IsOrderCancel = x.IsOrderCancel,
                            IsRewardProvided = x.IsRewardProvided,
                            ModifiedBy = x.ModifiedBy,
                            ModifiedDate = x.ModifiedDate,
                            OrderId = x.OrderId,
                            StreakId = x.StreakId,
                            StreakIsFulfill = x.StreakIsFulfill,
                            IsStreakContinue = 0
                        }).ToList();


                        applicablebucketsFilltered.ToList().ForEach(x =>
                        {
                            if (x.StreakIsFulfill)
                            {
                                x.IsStreakContinue = applicablebucketsFilltered.Any(y => y.StreakId > x.StreakId) ?
                                  (applicablebucketsFilltered.Where(y => y.StreakId < x.StreakId).OrderByDescending(y => y.StreakId).Select(y => y.IsStreakContinue).FirstOrDefault() + 1) : 1;
                            }
                            else
                                x.IsStreakContinue = 0;
                        }
                        );





                        List<AchiveConfigDc> achivelist = new List<AchiveConfigDc>();
                        foreach (var StreakLevelConfig in dataStreakLevelConfigMastersAll.ToList())
                        {
                            GameStreakLevelRewardValueDC objInsertStreakLevelReward = new GameStreakLevelRewardValueDC();
                            CreateCustomerLedgerDc objNewLedgerEntry = new CreateCustomerLedgerDc();

                            int StreakIdFrom = 0;
                            int StreakIdTo = 0;
                            double RewardValue = 0;

                            //----S------------
                            var RewardValueData = StreakLevelReward_ValidateStreakConditionType(StreakLevelConfig.StreakConditionType, StreakLevelConfig, applicablebucketsFilltered, dataStreakLevelConfigMasters, true);
                            StreakIdFrom = RewardValueData.StreakIdFrom;
                            StreakIdTo = RewardValueData.StreakIdTo;
                            RewardValue = RewardValueData.RewardValue;
                            //----E------------

                            if (StreakIdTo != 0)
                            {
                                AchiveConfigDc obj = new AchiveConfigDc();
                                obj.StreakAchivedCount = (StreakIdTo - StreakIdFrom) + 1;
                                obj.type = StreakLevelConfig.Type;
                                obj.Id = StreakLevelConfig.Id;
                                //obj.RewardTypeName = StreakLevelConfig.RewardTypeName;
                                obj.Condition = StreakLevelConfig.Condition;
                                achivelist.Add(obj);
                            }
                        }

                        retailerBucketGame.StreakConfigDataList = SqlStreakLevelConfigMasters.Where(x => StreakConfigIds.Contains(x.Id)).Select(y =>
                          new GameStreakConfig
                          {
                              Id = y.Id,
                              Type = y.Type,
                              RewardTypeName = y.RewardTypeName,
                              Reward = y.Reward,
                              Condition = y.Condition,
                              StreakDescr = y.StreakDescr,
                              StreakLevelValueFirst = y.Type == "Level-Individual" ? y.LevelNo : y.Type == "Outof" ? y.OutOf_TotalBucket : y.Streak_StreakCount,
                              StreakLevelValueLast = TotalStreakDuration,
                              DayLeft = y.Type == "Level-Individual" ? TotalStreakDuration - TillPeriodFullFill : TillPeriodFullFill,
                              StreakAchivedCount = achivelist.FirstOrDefault(x => x.Id == y.Id && x.type == y.Type && x.Condition == y.Condition) != null ? achivelist.FirstOrDefault(x => x.Id == y.Id && x.type == y.Type && x.Condition == y.Condition).StreakAchivedCount : 0,
                          }
                         ).ToList();

                        #endregion 
                    }

                }
                else
                {


                    var maxbucketDate = mongoDbHelperStreakCustomerTransaction.Select(x => x.CustomerId == CustomerId
                                                                 && x.IsActive == true && x.IsDeleted == false && x.IsExpired).OrderByDescending(x => x.BucketEndDate).FirstOrDefault()?.BucketEndDate;

                    //using (var db = new AuthContext())
                    {
                        int i = 0;
                        //int StreakDuration = 0;
                        //StreakDuration = db.GameConditionMasters.Where(x => x.Name == "StreakDuration" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();

                        do
                        {
                            if (maxbucketDate == null)
                                maxbucketDate = curDate;
                            else
                                maxbucketDate = maxbucketDate.Value.AddDays(IncreaseDay);

                            i++;
                            if (i == 6)
                                i = 0;
                        }
                        while (curDate >= maxbucketDate);

                        var Rangebuckets = Enumerable.Range(1, StreakDuration).Select(x => (x - 1) + 1).ToList();
                        List<KeyValuePair<int, DateTime>> StreakLevel = new List<KeyValuePair<int, DateTime>>();
                        maxbucketDate = maxbucketDate == curDate ?
                                curDate : maxbucketDate.Value.AddDays(((i * (-1)) * (IncreaseDay)) + 1);
                        foreach (var item in Rangebuckets)
                        {
                            StreakLevel.Add(new KeyValuePair<int, DateTime>(item, maxbucketDate.Value.AddDays((item - 1) * IncreaseDay)));
                        }

                        int BucketNo = 0;
                        BucketNo = dataCustomerStreakPeridList.Select(x => x.BucketNo).Distinct().FirstOrDefault();
                        if (BucketNo == 0)
                        {
                            MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                            var BucketNoList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            BucketNo = BucketNoList!=null? BucketNoList.BucketNo: 1;
                        }

                        int IsActiveCurrent1 = 1;
                        //int BucketNoTo = 0; int skip1 = 0; int take1 = 0; 
                        //var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
                        //var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                        //var fskip = new SqlParameter("@skip", skip1);
                        //var ftake = new SqlParameter("@take", take1);
                        //var CreatedDate = new SqlParameter("@CreatedDate", DBNull.Value);
                        //var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);
                        ////var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakConfig>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate, @IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, CreatedDate, fIsActiveCurrent).ToList();
                        //var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate, @IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, CreatedDate, fIsActiveCurrent).ToList();
                        //#if !DEBUG
                        //                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                        //                var SqlStreakLevelConfigMasters = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.GameConfigCacheKey(), () => GetStreakConfiguration(db, BucketNo,IsActiveCurrent1));
                        //#else
                        var SqlStreakLevelConfigMasters = GetStreakConfiguration(db, BucketNo, null, IsActiveCurrent1);
                        //#endif

                        if (SqlStreakLevelConfigMasters != null)
                            if (SqlStreakLevelConfigMasters != null && SqlStreakLevelConfigMasters.Count > 0)
                            {

                                var dataStreakLevelConfigMasters = SqlStreakLevelConfigMasters.ToList();


                                retailerBucketGame.CustomerStreakDataList = StreakLevel.Select(x =>
                                new GameStreakDc
                                {
                                    StreakId = x.Key,
                                    DaysLeft = (curDate >= x.Value && x.Value.AddDays(IncreaseDay).Date >= curDate) ? (x.Value.AddDays(IncreaseDay).Date - DateTime.Now.Date).Days : 0,
                                    Status = (curDate >= x.Value && x.Value.AddDays(IncreaseDay).Date >= curDate) ? "InProcess" : "InComplete"
                                }
                                ).ToList();

                                retailerBucketGame.CustomerStreakDataList.ForEach(x =>
                                {
                                    x.BucketNo = BucketNo;
                                    //var startdate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketStartDate;
                                    //var Enddate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketEndDate;
                                    //x.DaysLeft = (DateTime.Now.Date >= startdate.Date && Enddate.Date >= DateTime.Now.Date) ? (Enddate.Date - DateTime.Now.Date).Days : 0;
                                    x.Status = dataCustomerStreakPeridList.Count(y => y.StreakId == x.StreakId && y.IsRewardProvided) > 0 ? "Completed" : x.Status;
                                    x.RewardCredited = 0;
                                    x.StreakIdFrom = 0;
                                    x.StreakIdTo = 0;
                                    x.IndivitualRewardValue = Convert.ToString(SqlStreakLevelConfigMasters.FirstOrDefault(z => z.LevelNo == x.StreakId) != null ? SqlStreakLevelConfigMasters.FirstOrDefault(z => z.LevelNo == x.StreakId).LevelValue : 0);
                                    x.GameStreakConfigData = new List<GameStreakConfig>();
                                });
                                //retailerBucketGame.CurrentStreakBucket = retailerBucketGame.CustomerStreakDataList.Where(x => x.DaysLeft > 0).FirstOrDefault().StreakId > 0 ? retailerBucketGame.CustomerStreakDataList.Where(x => x.DaysLeft > 0).FirstOrDefault().StreakId : 0;

                                var StreakConfigIds = dataCustomerStreakPeridList.Select(x => x.GameStreakLevelConfigMasterId).Distinct().FirstOrDefault();
                                //int TotalStreakDuration = SqlStreakLevelConfigMasters.Where(x => StreakConfigIds.Contains(x.Id) && x.OutOf_OutOfBucket > 0).Select(y => y.OutOf_OutOfBucket).FirstOrDefault();
                                int TotalStreakDuration = SqlStreakLevelConfigMasters.Where(x => x.OutOf_OutOfBucket > 0).Select(y => y.OutOf_OutOfBucket).FirstOrDefault();

                                //retailerBucketGame.StreakConfigDataList = SqlStreakLevelConfigMasters.Where(x => StreakConfigIds.Contains(x.Id)).Select(y =>
                                retailerBucketGame.StreakConfigDataList = SqlStreakLevelConfigMasters.Select(y =>
                                  new GameStreakConfig
                                  {
                                      Id = y.Id,
                                      Type = y.Type,
                                      RewardTypeName = y.RewardTypeName,
                                      Reward = y.Reward,
                                      Condition = y.Condition,
                                      StreakDescr = y.StreakDescr,
                                      StreakLevelValueFirst = y.Type == "Level-Individual" ? y.LevelNo : y.Type == "Outof" ? y.OutOf_TotalBucket : y.Streak_StreakCount,
                                      StreakLevelValueLast = TotalStreakDuration,
                                      DayLeft = y.Type == "Level-Individual" ? 0 : IncreaseDay * StreakDuration
                                  }
                                 ).ToList();

                            }
                            else
                            {
                                retailerBucketGame.CustomerStreakDataList = StreakLevel.Select(x =>
                                new GameStreakDc
                                {
                                    StreakId = x.Key,
                                    DaysLeft = (curDate >= x.Value && x.Value.AddDays(IncreaseDay).Date >= curDate) ? (x.Value.AddDays(IncreaseDay).Date - curDate).Days : 0,
                                    Status = (curDate >= x.Value && x.Value.AddDays(IncreaseDay).Date >= curDate) ? "InProcess" : "InComplete"
                                }
                                ).ToList();
                            }
                    }


                }
            }
            #endregion
            return retailerBucketGame;
        }

        [HttpGet]
        [Route("GetCustomerGamesHistory")]
        public async Task<RewardEarningHistoryDc> GetCustomerGamesHistory(int CustomerId)
        {
            using (var db = new AuthContext())
            {
                RewardEarningHistoryDc Earning = new RewardEarningHistoryDc();

                //int BucketNo = 0; int BucketNoTo = 1; int skip1 = 0; int take1 = 0; int IsActiveCurrent1 = 0;
                //var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
                //var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                //var fskip = new SqlParameter("@skip", skip1);
                //var ftake = new SqlParameter("@take", take1);
                //var CreatedDate = new SqlParameter("@CreatedDate", DBNull.Value);
                //var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);
                //var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, CreatedDate, fIsActiveCurrent).ToList();

                int BucketNo = 0; int IsActiveCurrent1 = 0;
                var SqlStreakLevelConfigMasters = GetStreakConfiguration(db, BucketNo, null, IsActiveCurrent1);


                MongoDbHelper<GameCustomerLedger> mongoDbHelperCustomerLedger = new MongoDbHelper<GameCustomerLedger>();
                var dataCustomerLedgerList = mongoDbHelperCustomerLedger.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).ToList();

                var UpCommingPoint = dataCustomerLedgerList.Where(x => x.IsUpComingReward == true).Sum(x => x.RewardValue);
                var TotalEarningPoint = dataCustomerLedgerList.Where(x => x.IsCompleted == true).Sum(x => x.RewardValue);

                var GamesHistory = dataCustomerLedgerList.Select(x =>
                new CreateCustomerHistoryDc
                {
                    CustomerId = x.CustomerId,
                    GameBucketNo = x.GameBucketNo > 0 ? x.GameBucketNo - 1 : dataCustomerLedgerList.Where(z => z.BucketNo == x.BucketNo && z.GameBucketNo > 0).Select(z => z.GameBucketNo).FirstOrDefault() - 1,
                    BucketNo = x.BucketNo,
                    ForRewardStrack = x.ForRewardStrack == 1 ? "Reward" : x.ForRewardStrack == 2 ? "Strack" : "Redeem",   //"Reward =1  / Strack=2 / Redeem = 3
                    GameBucketRewardId = x.GameBucketRewardId,
                    GameStreakLevelConfigMasterId = x.GameStreakLevelConfigMasterId,
                    GameStreakLevelConfigDetailId = x.GameStreakLevelConfigDetailId,
                    StreakIdFrom = x.StreakIdFrom,
                    StreakIdTo = x.StreakIdTo,
                    RewardValue = x.RewardValue,
                    RewardStatus = x.IsUpComingReward == true ? "UpComing" : x.IsCompleted == true ? "Completed" : x.IsCanceled == true ? "Cancelled" : "",
                    RewardStatusDate = x.IsUpComingReward == true ? x.IsUpComingRewardDate : x.IsCompleted == true ? x.IsCompletedDate : x.IsCanceled == true ? x.IsCanceledDate : null,
                    BucketStartDate = x.BucketStartDate,
                    BucketEndDate = x.BucketEndDate,
                    GameType = x.ForRewardStrack == 1 ? "Reward"
                    : x.ForRewardStrack == 3 ? "Redeem"
                    : SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Type).FirstOrDefault(),
                    GameCondition = x.ForRewardStrack == 1 ? "-" : x.ForRewardStrack == 3 ? "-" :
                    SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Type).FirstOrDefault() == "Outof"
                    ? SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Condition).FirstOrDefault() + " / " + SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.OutOf_OutOfBucket).FirstOrDefault()
                    : SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Condition).FirstOrDefault(),

                }
               ).ToList();

                Earning.TotalEarningPoint = TotalEarningPoint;
                Earning.UpCommingPoint = UpCommingPoint;
                Earning.CreateCustomerHistoryDcs = GamesHistory.Count > 0 ? GamesHistory : new List<CreateCustomerHistoryDc>();

                return Earning;
            }

        }

        //public async Task<RetailerBucketGame> GetCustomerBucketGames(int CustomerId, int warehouseId, int skip, int take, string lang)
        //{
        //    int pageno = (skip == 0 ? 1 : (skip / take) + 1);
        //    var buckets = Enumerable.Range(skip + 1, take * pageno).Select(x => (x - 1) + 1).ToList();
        //    RetailerBucketGame retailerBucketGame = new RetailerBucketGame();
        //    MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
        //    var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

        //    //var bucketcustomers = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false).GroupBy(x => x.BucketNo).Select(x => new { BucekNo = x.Key, TotalCustomers = x.Count() }).ToList();
        //    var bucketcustomers = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false).GroupBy(x => x.NextBucketNo >= 0 ? x.GameBucketNo - 1 : x.GameBucketNo).Select(x => new { BucekNo = x.Key, TotalCustomers = x.Count() }).ToList();

        //    if (dataCustomerBucketHdrList != null)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            int start = dataCustomerBucketHdrList.CRMBucketNo + (skip == 0 ? 0 : (skip + 1));
        //            int end = dataCustomerBucketHdrList.CRMBucketNo + take * pageno;
        //            GameBucketReward gameBucketReward = new GameBucketReward();
        //            //string sql = "select a.bucketNo,a.IsFix,a.RewardType,a.Value,a.StartDate,a.EndDate,a.RewardApproveStatus, " +
        //            //             "c.Id ConditionId, c.Name Condition, c.Value ConditionValue, c.AppDesc, c.SqlQuery  from GameBucketRewards a " +
        //            //             "left join BucketRewardConditions b on a.Id = b.GameBucketRewardId " +
        //            //             "left join GameConditionMasters c on b.GameConditionMasterId = c.Id " +
        //            //             "where a.IsActive = 1 and a.IsDeleted = 0 and b.IsActive = 1 and b.IsDeleted = 0 " +
        //            //             "and c.IsActive = 1 and c.IsDeleted = 0 and a.bucketNo>=" + (skip + 1) + " And a.bucketNo<=" + (take * pageno);
        //            //var bucketConditions = db.Database.SqlQuery<GameBucketConditionData>(sql).ToList();

        //            var fPageno = new SqlParameter("@pageno", pageno);
        //            var fskip = new SqlParameter("@skip", skip);
        //            var ftake = new SqlParameter("@take", take);
        //            var bucketConditions = db.Database.SqlQuery<GameBucketConditionData>("exec sp_bucketConditions @pageno, @skip, @take", fPageno, fskip, ftake).ToList();



        //            var gameBuckets = buckets.Select(x =>
        //                    new GameBucket
        //                    {
        //                        BucketNo = x,
        //                        CRMBucketNo = (dataCustomerBucketHdrList.CRMBucketNo + (x - 1)),
        //                        Status = "InComplete"
        //                    }
        //                   ).ToList();
        //            if (bucketConditions != null)
        //            {
        //                var rewards = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();

        //                gameBuckets.ForEach(x =>
        //                {
        //                    if (bucketcustomers.Any(y => y.BucekNo == x.BucketNo))
        //                    {
        //                        x.TotalCustomers = bucketcustomers.FirstOrDefault(y => y.BucekNo == x.BucketNo).TotalCustomers;
        //                    }
        //                    //if (bucketConditions.Any(y => y.bucketNo == x.CRMBucketNo))
        //                    //{
        //                    //    //x.RewardType = bucketConditions.FirstOrDefault(y => y.bucketNo == x.CRMBucketNo).RewardType == 0 ? "WalletPoint" : "Other";
        //                    //    //x.RewardValue = bucketConditions.FirstOrDefault(y => y.bucketNo == x.CRMBucketNo).Value;

        //                    //    //x.RewardType = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).RewardType == 0 ? "WalletPoint" : "Other";
        //                    //    //x.RewardValue = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).value;
        //                    //}
        //                    x.RewardType = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).RewardType == 0 ? "WalletPoint" : "Other";
        //                    x.RewardValue = rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo) != null && rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).value != 0 ? rewards.FirstOrDefault(y => y.BucketNo == x.CRMBucketNo).value : 0;


        //                    x.GameBucketRewardConditions = bucketConditions.Where(y => y.bucketNo == x.CRMBucketNo).Select(y => new GameBucketRewardCondition
        //                    {
        //                        ConditionId = y.ConditionId,
        //                        AppDesc = y.AppDesc,
        //                        Value = y.Value
        //                    }).ToList();
        //                    x.GameBucketRewardConditions.Add(new GameBucketRewardCondition
        //                    {
        //                        ConditionId = 0,
        //                        AppDesc = "Place an order every " + IncreaseDay + " days",
        //                        Value = 0,
        //                    });
        //                });
        //            }
        //            retailerBucketGame.GameBucket = gameBuckets;
        //        }


        //        retailerBucketGame.CurrentBucket = dataCustomerBucketHdrList.NextBucketNo == 0 ? dataCustomerBucketHdrList.GameBucketNo : dataCustomerBucketHdrList.GameBucketNo - 1;
        //        retailerBucketGame.LevelUpBucket = dataCustomerBucketHdrList.NextBucketNo != 0 ? dataCustomerBucketHdrList.GameBucketNo : 0;
        //        retailerBucketGame.NextOrderDay = Convert.ToInt32((dataCustomerBucketHdrList.BucketEndDate - dataCustomerBucketHdrList.LastOrderDate.Value).TotalDays) <= IncreaseDay ? Convert.ToInt32((dataCustomerBucketHdrList.BucketEndDate - dataCustomerBucketHdrList.LastOrderDate.Value).TotalDays) : 0;
        //        MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
        //        var dataOrderDtl = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).GroupBy(x => new { x.GameBucketNo, x.RewardCredit, x.RewardStatus, x.BucketNo });
        //        if (dataOrderDtl != null)
        //        {
        //            foreach (var item in retailerBucketGame.GameBucket)
        //            {
        //                //if (dataOrderDtl.Any(x => x.Key.GameBucketNo == item.BucketNo))
        //                if (dataOrderDtl.Any(x => x.Key.BucketNo == item.CRMBucketNo))
        //                {
        //                    //item.Status = dataOrderDtl.FirstOrDefault(x => x.Key.GameBucketNo == item.BucketNo).Key.RewardStatus;
        //                    item.Status = dataOrderDtl.FirstOrDefault(x => x.Key.BucketNo == item.CRMBucketNo).Key.RewardStatus;
        //                    //item.RewardCredited = dataOrderDtl.FirstOrDefault(x => x.Key.GameBucketNo == item.BucketNo).Key.RewardCredit;
        //                    item.RewardCredited = dataOrderDtl.FirstOrDefault(x => x.Key.BucketNo == item.CRMBucketNo).Key.RewardCredit;
        //                }
        //                if (retailerBucketGame.CurrentBucket == item.BucketNo && item.GameBucketRewardConditions != null && item.GameBucketRewardConditions.Any())
        //                {
        //                    using (var db = new AuthContext())
        //                    {
        //                        var custGameConditions = db.GameCurrentLevelProgresses.Where(x => x.CustomerId == CustomerId && x.BucketNo == item.BucketNo && x.IsActive).Select(x => new { x.ConditionId, x.SqlOutPut }).ToList();
        //                        foreach (var condition in item.GameBucketRewardConditions)
        //                        {
        //                            if (custGameConditions.Any(x => x.ConditionId == condition.ConditionId))
        //                            {
        //                                condition.AchiveValue = custGameConditions.FirstOrDefault(x => x.ConditionId == condition.ConditionId).SqlOutPut;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return retailerBucketGame;
        //}

        //[HttpGet]
        //[Route("GetCustomerStreakReward")]
        //public async Task<GameMainListDc> GetCustomerStreakReward(int CustomerId)
        //{
        //    #region Customer wise Streak Level Data
        //    GameMainListDc MainListDc = new GameMainListDc();

        //    var StreakLevel = Enumerable.Range(1, 9).Select(x => (x - 1) + 1).ToList();

        //    MongoDbHelper<GameStreakLevelRewardValue> mongoDbHelperStreakRewardCredited = new MongoDbHelper<GameStreakLevelRewardValue>();

        //    MongoDbHelper<GameStreakCustomerTransaction> mongoDbHelperStreakCustomerTransaction = new MongoDbHelper<GameStreakCustomerTransaction>();
        //    var dataCustomerStreakPeridList = mongoDbHelperStreakCustomerTransaction.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();

        //    if (dataCustomerStreakPeridList != null)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            //GameBucketReward gameBucketReward = new GameBucketReward();
        //            Int32 BucketNo = dataCustomerStreakPeridList.Select(x => x.BucketNo).Distinct().FirstOrDefault();

        //            string sql = "Select a.Id  , a.BucketNoFrom, a.BucketNoTo" +
        //                ", Case When a.StreakConditionType = 1 then 'Level-Individual' When a.StreakConditionType = 2 then 'Streak' When a.StreakConditionType = 3 then 'Outof' End Type " +
        //                ", case when a.RewardType = 0 then 'wallet' when a.RewardType = 1 then 'RI' when a.RewardType = 2 then 'offer' end RewardType " +
        //                ", IsNull(case when a.Streak_ChooseReward=1 then 'Multiplier' when a.Streak_ChooseReward=2 then 'Fixed' end,'') As StreakDescr " +
        //                ", case When a.StreakConditionType = 1 then Rtrim(Ltrim(Cast(IsNull(b.LevelValue,0) as char)))   " +
        //                "WHen a.StreakConditionType = 2 and a.Streak_ChooseReward = 1  then Rtrim(Ltrim(Cast(a.RewardValue as char))) +'x'  " +
        //                "else Rtrim(Ltrim(Cast(a.RewardValue as char))) end Reward  " +
        //                ", case When a.StreakConditionType = 1 then 'Streak Level ' + Rtrim(Ltrim(Cast(IsNull(b.LevelNo, 0) as char)))  " +
        //                "WHen a.StreakConditionType = 2 then 'Continuous ' + Rtrim(Ltrim(Cast(a.Streak_StreakCount as char)))           " +
        //                "when a.StreakConditionType = 3 then 'Out-of ' + Rtrim(Ltrim(Cast(a.OutOf_TotalBucket as char))) end Condition  " +
        //                " From GameStreakLevelConfigMasters a with(NoLock)  " +
        //                " Left Join GameStreakLevelConfigDetails b  with(NoLock) on a.Id = b.GameStreakLevelConfigMasterId  " +
        //                " Where  " + BucketNo + " >= a.BucketNoFrom and " + BucketNo + " <= a.BucketNoTo ";
        //            var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakConfig>(sql).ToList();
        //            if (SqlStreakLevelConfigMasters != null)
        //            {
        //                var dataStreakRewardCreditedList = mongoDbHelperStreakRewardCredited.Select(x => x.CustomerId == CustomerId && x.BucketNo == BucketNo && x.IsActive == true && x.IsDeleted == false).ToList();
        //                var dataRewardCreditedIDs = dataStreakRewardCreditedList.Select(x => x.GameStreakLevelConfigMasterId).ToList();

        //                var dataStreakLevelConfigMasters = SqlStreakLevelConfigMasters.Where(x => dataRewardCreditedIDs.Contains(x.Id)).ToList();

        //                MainListDc.CustomerStreakDataList = StreakLevel.Select(x =>
        //                    new GameStreakDc
        //                    {
        //                        StreakId = x,
        //                        Status = "InComplete"
        //                    }
        //                   ).ToList();

        //                MainListDc.CustomerStreakDataList.ForEach(x =>
        //                {
        //                    x.BucketNo = BucketNo;
        //                    x.StreakBucketStartDate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketStartDate;
        //                    x.StreakBucketEndDate = dataCustomerStreakPeridList.FirstOrDefault(y => y.StreakId == x.StreakId).BucketEndDate;
        //                    x.DaysLeft = (x.StreakBucketEndDate.Date - DateTime.Now.Date).Days;
        //                    x.Status = dataCustomerStreakPeridList.Count(y => y.StreakId == x.StreakId && y.IsRewardProvided) > 0 ? "Completed" : "InComplete";
        //                    x.RewardCredited = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).RewardValue : 0;
        //                    x.GameStreakLevelConfigMasterId = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).GameStreakLevelConfigMasterId : 0;
        //                    x.StreakIdFrom = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).StreakIdFrom : 0;
        //                    x.StreakIdTo = dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId) != null ? dataStreakRewardCreditedList.FirstOrDefault(z => z.StreakIdTo == x.StreakId).StreakIdTo : 0;

        //                    x.GameStreakConfigData = dataStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId).Select(y => new GameStreakConfig
        //                    {
        //                        Id = y.Id,
        //                        Type = y.Type,
        //                        RewardType = y.RewardType,
        //                        Reward = y.Reward,
        //                        Condition = y.Condition,
        //                        StreakDescr = y.StreakDescr
        //                    }).ToList();
        //                });


        //                MainListDc.StreakConfigDataList = dataStreakLevelConfigMasters.Select(y =>
        //                  new GameStreakConfig
        //                  {
        //                      Id = y.Id,
        //                      Type = y.Type,
        //                      RewardType = y.RewardType,
        //                      Reward = y.Reward,
        //                      Condition = y.Condition,
        //                      StreakDescr = y.StreakDescr
        //                  }
        //                 ).ToList();

        //            }
        //        }

        //    }

        //    return MainListDc;
        //    #endregion

        //}

        [HttpPost]
        [Route("InsertOrder")]
        public HttpResponseMessage InsertOrder(int CustomerId, int OrderId, DateTime OrderCreateDate)
        {
            bool flag = false;
            string Msg = "";
            var dt = OrderCreateDate.ToString("yyyy-MM-dd");  // 23:59:59

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            var res = new
            {
                status = true,
                Message = ""
            };

            //if (dataCustomerBucketHdr == null)
            //{
            //    res = new
            //    {
            //        status = false,
            //        Message = "Customer Not Found"
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, res);
            //}
            //else
            using (var context = new AuthContext())
            {
                var resuslt = context.GameBucketRewards.Where(x => x.IsFix == false && x.StartDate <= OrderCreateDate.Date && x.EndDate >= OrderCreateDate.Date && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (resuslt != null) // For Dynamic
                {
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();

            //var dataCustomerBucketHdr = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.BucketStartDate.Value.Date <= OrderCreateDate && x.BucketEndDate.Value.Date >= OrderCreateDate && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            var dataCustomerBucketHdr = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

            var startdt = dataCustomerBucketHdr.BucketStartDate.Date;
            var enddt = dataCustomerBucketHdr.BucketEndDate.Date;

            if (startdt <= OrderCreateDate.Date && enddt >= OrderCreateDate.Date)
            {

                int BucketNo = dataCustomerBucketHdr.BucketNo;
                DateTime BacktStartDate = (DateTime)dataCustomerBucketHdr.BucketStartDate;
                DateTime BacktEndDate = (DateTime)dataCustomerBucketHdr.BucketEndDate;
                int NextBucktNo = dataCustomerBucketHdr.NextBucketNo;
                int GameBucketNo = dataCustomerBucketHdr.GameBucketNo;

                MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                var dataOrderDtl = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.BucketNo == BucketNo && x.IsActive == true && x.IsDeleted == false).ToList();
                if (dataOrderDtl.Count == 0)
                {
                    GameBucketNo = GameBucketNo + 1;
                    dataCustomerBucketHdr.NextBucketNo = BucketNo + 1;
                    dataCustomerBucketHdr.GameBucketNo = GameBucketNo + 1;
                    flag = mongoDbHelperCustomerBucketHdr.Replace(dataCustomerBucketHdr.Id, dataCustomerBucketHdr);
                }

                var bucketOrderexists = dataOrderDtl.Any(z => z.OrderId == OrderId);
                if (!bucketOrderexists)
                {
                    GameCustomerBucketHdrOrderDetail obj = new GameCustomerBucketHdrOrderDetail();
                    obj.CustomerId = CustomerId;
                    obj.OrderId = OrderId;
                    obj.BucketNo = BucketNo;
                    obj.GameBucketNo = GameBucketNo;
                    obj.BucketStartDate = BacktStartDate;
                    obj.BucketEndDate = BacktEndDate;
                    obj.RewardCredit = 0;
                    obj.RewardStatus = "InProcess";
                    obj.IsCompleted = false;
                    obj.CreatedBy = userid;
                    obj.CreatedDate = DateTime.Now;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    obj.ModifiedBy = userid;
                    obj.ModifiedDate = DateTime.Now;
                    flag = mongoDbHelperCustomerBucketHdrOrderDetail.Insert(obj);

                    dataCustomerBucketHdr.LastOrderDate = OrderCreateDate;
                    flag = mongoDbHelperCustomerBucketHdr.Replace(dataCustomerBucketHdr.Id, dataCustomerBucketHdr);

                    Msg = "Successfully Added!";
                }
            }
            res = new
            {
                status = flag,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [HttpGet]
        [Route("UpdateCutomerRewardStatusJob")]
        public async Task<string> UpdateCutomerRewardStatus()
        {
            string flag = "";
            DateTime curDate = DateTime.Now.Date;

            try
            {
                using (var db = new AuthContext())
                {
                    GameBucketReward gameBucketReward = new GameBucketReward();
                    //string sql = "select a.bucketNo,a.IsFix,a.RewardType,a.Value,a.StartDate,a.EndDate,a.RewardApproveStatus, " +
                    //             "c.Id ConditionId,c.Name Condition, c.Value ConditionValue, c.AppDesc, c.SqlQuery  " +
                    //             "from GameBucketRewards a left join BucketRewardConditions b on a.Id = b.GameBucketRewardId " +
                    //             "left join GameConditionMasters c on b.GameConditionMasterId = c.Id " +
                    //             "where a.IsActive = 1 and a.IsDeleted = 0 and b.IsActive = 1 and b.IsDeleted = 0 " +
                    //             "and c.IsActive = 1 and c.IsDeleted = 0 ";

                    int pageno = 0; int skip = 0; int take = 0;
                    var fPageno = new SqlParameter("@pageno", pageno);
                    var fskip = new SqlParameter("@skip", skip);
                    var ftake = new SqlParameter("@take", take);
                    var bucketConditions = db.Database.SqlQuery<GameBucketConditionData>("exec sp_bucketConditions @pageno, @skip, @take", fPageno, fskip, ftake).ToList();

                    MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();

                    //Validate & Update Cancelled status to InProcess status
                    #region Validate & Update Cancelled status to InProcess
                    var InprocessCustomerDataList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(y => y.OrderId > 0 && y.RewardStatus == "InProcess").Select(x => new { x.CustomerId, x.BucketNo, x.GameBucketNo }).Distinct().ToList();
                    foreach (var data in InprocessCustomerDataList)
                    {
                        long GameBucketRewardId = 0;

                        //var completedcustomer = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsCompleted == true && x.RewardStatus == "Cancelled" && x.CustomerId == data.CustomerId && x.BucketNo == data.BucketNo && x.GameBucketNo == data.GameBucketNo).FirstOrDefault();
                        //if (completedcustomer != null)
                        {
                            var customerwiselist = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == data.CustomerId && x.RewardStatus == "Cancelled" && x.BucketNo == data.BucketNo && x.GameBucketNo == data.GameBucketNo).ToList();
                            if (customerwiselist.Count > 0)
                            {
                                GameBucketRewardId = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == data.CustomerId && x.BucketNo == data.BucketNo && x.GameBucketNo == data.GameBucketNo && x.GameBucketRewardId > 0).Select(x => x.GameBucketRewardId).FirstOrDefault();

                                foreach (var itemt in customerwiselist)
                                {
                                    itemt.RewardCredit = 0;
                                    itemt.RewardStatus = "InProcess";
                                    itemt.IsCompleted = false;
                                    itemt.ModifiedDate = null;
                                    bool flag11 = mongoDbHelperCustomerBucketHdrOrderDetail.Replace(itemt.Id, itemt);
                                }


                                #region CustomerLedger Update Order Status Canceled to Upcoming.
                                UpdateCustomerLedgerStatusDc objLedgerInput = new UpdateCustomerLedgerStatusDc();
                                objLedgerInput.CustomerId = data.CustomerId;
                                objLedgerInput.GameBucketNo = data.GameBucketNo;
                                objLedgerInput.BucketNo = data.BucketNo;
                                objLedgerInput.ForRewardStrack = 1;
                                objLedgerInput.StreakIdFrom = 0;
                                objLedgerInput.StreakIdTo = 0;
                                objLedgerInput.GameBucketRewardId = GameBucketRewardId;
                                objLedgerInput.GameStreakLevelConfigMasterId = 0;
                                objLedgerInput.GameStreakLevelConfigDetailId = 0;
                                objLedgerInput.IsUpComingReward = true;
                                objLedgerInput.IsCompleted = false;
                                objLedgerInput.IsCanceled = false;
                                var b = await CustomerLedger_UpdateStatus(objLedgerInput);
                                #endregion
                            }
                        }
                    }
                    #endregion

                    //--Customer Order Transation
                    var dataOrderDtl = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsCompleted == false
                    && x.IsActive == true && x.IsDeleted == false).OrderBy(x => x.CustomerId).ThenBy(x => x.BucketNo).ToList();

                    if (dataOrderDtl.Count == 0)
                    { flag = "Rrcord Not Exist CustomerBucketHdrOrderDetail tbl"; }
                    else
                    {
                        #region GameCurrentLevelProgresses
                        //------------S-------- Delete ------------------
                        string newQuery = "delete from GameCurrentLevelProgresses ";
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["authcontext"].ToString()))
                        {
                            SqlCommand command = new SqlCommand(newQuery, connection);
                            command.CommandType = System.Data.CommandType.Text;
                            connection.Open();
                            int i = command.ExecuteNonQuery();
                        }
                        //------------E-------- Delete -----------------

                        var CustomerList = dataOrderDtl.Where(y => y.OrderId > 0 && y.GameBucketRewardId > 0).Select(x => new { x.CustomerId, x.BucketNo, x.GameBucketNo, x.GameBucketRewardId }).Distinct().ToList();
                        foreach (var item in CustomerList)
                        {
                            Int32 iCustomerId = 0; Int32 iBucketNo = 0; Int32 GameBucketNo = 0; long BucketRewardId = 0;
                            string strOrderId = ""; string RewardApproveStatus = "";

                            iCustomerId = item.CustomerId;
                            iBucketNo = item.BucketNo;
                            BucketRewardId = item.GameBucketRewardId;
                            GameBucketNo = item.GameBucketNo;

                            var OrderDtlList = dataOrderDtl.Where(x => x.CustomerId == iCustomerId && x.BucketNo == iBucketNo && x.OrderId > 0).Select(x => x.OrderId).Distinct().ToList();
                            strOrderId = string.Join(",", OrderDtlList);

                            //-----S-----SQL Condition -------------------
                            Int32 curRewardCredit = 0; string curRewardStatus = "";
                            string strSqlQuery = "";
                            string strQuery = "";
                            int TotOrderCount = strOrderId.Split(',').ToList().Count();
                            string OrderStatus = "";


                            //var SqlQueryList = bucketConditions.Where(x => x.bucketNo == iBucketNo && x.SqlQuery != "").ToList();
                            var SqlQueryList = bucketConditions.Where(x => x.GameBucketRewardId == BucketRewardId && x.SqlQuery != "").ToList();
                            if (SqlQueryList.Count > 0)
                            {
                                curRewardCredit = SqlQueryList.Select(x => x.Value).FirstOrDefault();
                                RewardApproveStatus = SqlQueryList.Select(x => x.RewardApproveStatus).FirstOrDefault();
                                strSqlQuery = SqlQueryList.Select(x => x.SqlQuery).FirstOrDefault();
                                //strQuery = strSqlQuery;
                                //strSqlQuery = strSqlQuery.Replace("@OrderId", strOrderId);
                                //strSqlQuery = strSqlQuery.Replace("@Status", RewardApproveStatus);
                            }

                            if (strSqlQuery != string.Empty)
                            {
                                //string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                                //DataTable DT = new DataTable();
                                //using (var connection = new SqlConnection(connectionString))
                                //{
                                //    using (var command = new SqlCommand(strSqlQuery, connection))
                                //    {
                                //        command.CommandTimeout = 300;
                                //        if (connection.State != ConnectionState.Open)
                                //            connection.Open();

                                //        SqlDataAdapter da = new SqlDataAdapter(command);

                                //        da.Fill(DT);
                                //        da.Dispose();
                                //        connection.Close();
                                //    }
                                //}

                                //List<MappingRersultDc> MappintResultList = new List<MappingRersultDc>();
                                //if (DT.Rows.Count > 0)
                                //{
                                //    foreach (DataRow row in DT.Rows)
                                //    {
                                //        foreach (DataColumn col in DT.Columns)
                                //        {
                                //            string ConditionName = col.ColumnName;
                                //            double ResConditionValue = (double)row[col] != 0 ? (double)row[col] : 0;
                                //            //table.AddCell(value.ToString());

                                //            //var x = SqlQueryList.Where(w => w.bucketNo == item.BucketNo && w.Condition == ConditionName).FirstOrDefault();
                                //            var x = SqlQueryList.Where(w => w.GameBucketRewardId == BucketRewardId && w.Condition == ConditionName).FirstOrDefault();
                                //            //foreach (var x in SqlQueryList)
                                //            if (x != null)
                                //            {
                                //                MappingRersultDc obj = new MappingRersultDc();
                                //                obj.MappingCondiName = x.Condition;
                                //                obj.SqlOutPut = ResConditionValue; // x.ConditionValue;
                                //                obj.SqlResultStatus = ResConditionValue >= x.ConditionValue ? "Success" : "Failed";
                                //                //if (ResConditionValue >= x.ConditionValue)
                                //                //{ obj.SqlResultStatus = "Success"; }
                                //                //else
                                //                //{ obj.SqlResultStatus = "Failed"; }

                                //                obj.CustomerId = iCustomerId;
                                //                obj.BucketNo = iBucketNo;
                                //                obj.GameBucketNo = GameBucketNo;
                                //                obj.BucketRerward = curRewardCredit;
                                //                obj.ConditionId = x.ConditionId;
                                //                obj.ApiDes = x.AppDesc;
                                //                obj.CreatedDate = DateTime.Now;
                                //                obj.IsActive = true;
                                //                obj.CreatedBy = 1717;
                                //                MappintResultList.Add(obj);
                                //            }

                                //        }
                                //    }
                                //}
                                //-----E-----SQL COndition -------------------

                                List<MappingRersultDc> MappintResultList = new List<MappingRersultDc>();
                                MappintResultList = await ValidateCancelOrder(strSqlQuery, strOrderId, BucketRewardId, iCustomerId, iBucketNo, GameBucketNo, bucketConditions);

                                Int32 ResultConditionCount = MappintResultList.Where(y => y.SqlResultStatus == "Failed"
                                || y.SqlResultStatus == "Cancelled").Count();
                                if (ResultConditionCount == 0)
                                {
                                    curRewardStatus = "Completed";

                                    MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetailxx = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                                    var dataOrderDtlxx = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsCompleted == false && x.IsActive == true && x.IsDeleted == false && x.CustomerId == iCustomerId && x.BucketNo == iBucketNo).OrderBy(x => x.CustomerId).ThenBy(x => x.BucketNo).ToList();

                                    foreach (var itemt in dataOrderDtlxx)
                                    {
                                        itemt.RewardCredit = curRewardCredit;
                                        itemt.RewardStatus = curRewardStatus;
                                        itemt.IsCompleted = true;
                                        itemt.ModifiedDate = DateTime.Now;
                                        bool flag11 = mongoDbHelperCustomerBucketHdrOrderDetailxx.Replace(itemt.Id, itemt);
                                    }

                                    #region update wallet amount

                                    if (curRewardStatus == "Completed")
                                    {
                                        var wallet = db.WalletDb.Where(x => x.CustomerId == iCustomerId && x.Deleted == false).FirstOrDefault();
                                        if (wallet != null)
                                        {
                                            var Walltamount = wallet.TotalAmount;
                                            wallet.TotalAmount = Walltamount + curRewardCredit;

                                            db.Entry(wallet).State = EntityState.Modified;
                                        }
                                    }

                                    #endregion

                                    #region CustomerLedger Update Order Status Completed
                                    if (dataOrderDtlxx != null && dataOrderDtlxx.Count > 0)
                                    {
                                        UpdateCustomerLedgerStatusDc objLedgerInput = new UpdateCustomerLedgerStatusDc();
                                        objLedgerInput.CustomerId = iCustomerId;
                                        objLedgerInput.GameBucketNo = GameBucketNo;
                                        objLedgerInput.BucketNo = iBucketNo;
                                        objLedgerInput.ForRewardStrack = 1;
                                        objLedgerInput.StreakIdFrom = 0;
                                        objLedgerInput.StreakIdTo = 0;
                                        objLedgerInput.GameBucketRewardId = BucketRewardId;
                                        objLedgerInput.GameStreakLevelConfigMasterId = 0;
                                        objLedgerInput.GameStreakLevelConfigDetailId = 0;
                                        objLedgerInput.IsUpComingReward = false;
                                        objLedgerInput.IsCompleted = true;
                                        var b = await CustomerLedger_UpdateStatus(objLedgerInput);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    foreach (var m in MappintResultList)
                                    {
                                        GameCurrentLevelProgress obj = new GameCurrentLevelProgress();
                                        obj.ConditionId = m.ConditionId;
                                        obj.BucketNo = m.BucketNo;
                                        obj.CreatedBy = m.CreatedBy;
                                        obj.BucketRerward = m.BucketRerward;
                                        obj.CreatedDate = m.CreatedDate;
                                        obj.CustomerId = m.CustomerId;
                                        obj.GameBucketNo = m.GameBucketNo;
                                        obj.SqlOutPut = Convert.ToInt32(Math.Round(m.SqlOutPut, 0));
                                        obj.ResultStatus = m.SqlResultStatus;
                                        obj.IsActive = m.IsActive;
                                        obj.IsDeleted = false;
                                        db.GameCurrentLevelProgresses.Add(obj);
                                    }

                                    Int32 ResultConditionCountCancel = MappintResultList.Where(y => y.SqlResultStatus == "Cancelled").Count();
                                    if (ResultConditionCountCancel > 0)
                                    {
                                        #region GameCustomerBucketHdrOrderDetail Order Status Canceled
                                        MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetailxx = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                                        var dataOrderDtlxx = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsCompleted == false && x.IsActive == true && x.IsDeleted == false && x.RewardStatus == "InProcess" && x.CustomerId == iCustomerId && x.BucketNo == iBucketNo).OrderBy(x => x.CustomerId).ThenBy(x => x.BucketNo).ToList();
                                        foreach (var itemt in dataOrderDtlxx)
                                        {
                                            itemt.RewardCredit = 0;
                                            itemt.RewardStatus = "Cancelled";
                                            itemt.IsCompleted = true;
                                            itemt.ModifiedDate = DateTime.Now;
                                            bool flag11 = mongoDbHelperCustomerBucketHdrOrderDetailxx.Replace(itemt.Id, itemt);
                                        }
                                        #endregion

                                        #region CustomerLedger Update Order Status Canceled
                                        UpdateCustomerLedgerStatusDc objLedgerInput = new UpdateCustomerLedgerStatusDc();
                                        objLedgerInput.CustomerId = iCustomerId;
                                        objLedgerInput.GameBucketNo = GameBucketNo;
                                        objLedgerInput.BucketNo = iBucketNo;
                                        objLedgerInput.ForRewardStrack = 1;
                                        objLedgerInput.StreakIdFrom = 0;
                                        objLedgerInput.StreakIdTo = 0;
                                        objLedgerInput.GameBucketRewardId = BucketRewardId;
                                        objLedgerInput.GameStreakLevelConfigMasterId = 0;
                                        objLedgerInput.GameStreakLevelConfigDetailId = 0;
                                        objLedgerInput.IsUpComingReward = false;
                                        objLedgerInput.IsCompleted = false;
                                        objLedgerInput.IsCanceled = true;
                                        var b = await CustomerLedger_UpdateStatus(objLedgerInput);
                                        #endregion
                                    }
                                }
                            } //-----E----if (strSqlQuery != string.Empty)

                        }  ////--E-- Customer Order Transation

                        //                        

                        db.Commit();
                        #endregion
                    }

                    #region inprocessupdateinmongo
                    if (dataOrderDtl.Count > 0)
                    {
                        MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelper = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                        var InprocessCustomerList = dataOrderDtl.Where(y => y.OrderId > 0 && y.RewardStatus == "InProcess").Select(x => new { x.CustomerId, x.BucketNo, x.GameBucketNo }).Distinct().ToList();
                        //var inprocessdistinctcustomerlist = InprocessCustomerList.Select(x=>new {x.CustomerId}).Distinct().ToList();
                        //var CompletedCustomerList = dataOrderDtl.Where(y => y.OrderId > 0  && y.RewardStatus == "Completed").Select(x => new { x.CustomerId, x.BucketNo, x.GameBucketNo, x.GameBucketRewardId, x.IsCompleted, x.RewardStatus, x.RewardCredit }).Distinct().ToList();
                        foreach (var data in InprocessCustomerList)
                        {
                            var completedcustomer = mongoDbHelper.Select(x => x.IsCompleted == true && x.RewardStatus == "Completed" && x.CustomerId == data.CustomerId && x.BucketNo == data.BucketNo && x.GameBucketNo == data.GameBucketNo).FirstOrDefault();
                            if (completedcustomer != null)
                            {
                                var customerwiselist = mongoDbHelper.Select(x => x.CustomerId == data.CustomerId && x.RewardStatus == "InProcess" && x.BucketNo == data.BucketNo && x.GameBucketNo == data.GameBucketNo).ToList();
                                if (customerwiselist.Count > 0)
                                {
                                    foreach (var itemt in customerwiselist)
                                    {
                                        itemt.RewardCredit = completedcustomer.RewardCredit;
                                        itemt.RewardStatus = completedcustomer.RewardStatus;
                                        itemt.IsCompleted = true;
                                        itemt.ModifiedDate = DateTime.Now;
                                        bool flag11 = mongoDbHelper.Replace(itemt.Id, itemt);
                                    }
                                }


                            }
                        }

                        //foreach(var data in CompletedCustomerList)
                        //{
                        //    var inprocessdata = InprocessCustomerList.Where(x => x.CustomerId == data.CustomerId && x.BucketNo==data.BucketNo && x.GameBucketNo==data.GameBucketNo).ToList();
                        //    if (inprocessdata.Count > 0)
                        //    {
                        //        MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongocustomerdetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                        //        var customer = mongocustomerdetail.Select(x => x.CustomerId == data.CustomerId && x.BucketNo==data.BucketNo && x.GameBucketNo==data.GameBucketNo).ToList();
                        //        foreach (var itemt in customer)
                        //        {

                        //            itemt.RewardCredit = data.RewardCredit;
                        //            itemt.RewardStatus = data.RewardStatus;
                        //            itemt.IsCompleted = true;
                        //            itemt.ModifiedDate = DateTime.Now;
                        //            bool flag11 = mongocustomerdetail.ReplaceWithoutFind(itemt.Id, itemt);
                        //        }
                        //    }
                        //}
                    }
                    #endregion

                    #region StreakLevelReward_Insert
                    bool flagreturn = await StreakLevelReward_Insert();
                    #endregion


                    #region Update Streak Reward Status
                    MongoDbHelper<GameStreakCustomerTransaction> mongoDbGameStreakCustomerTransaction = new MongoDbHelper<GameStreakCustomerTransaction>();

                    MongoDbHelper<GameStreakLevelRewardValue> mongoDbGameStreakLevelRewardValue = new MongoDbHelper<GameStreakLevelRewardValue>();
                    var dataGameStreakList = mongoDbGameStreakLevelRewardValue.Select(x => x.IsCompleted == false && x.IsActive == true && x.IsDeleted == false).OrderBy(x => x.CustomerId).ThenBy(x => x.BucketNo).ToList();

                    foreach (var listItems in dataGameStreakList.GroupBy(x => new { x.CustomerId, x.BucketNo }))
                    {
                        var applicablebuckets = dataGameStreakList.Where(x => x.CustomerId == listItems.Key.CustomerId && x.BucketNo == listItems.Key.BucketNo).ToList();

                        foreach (var Items in applicablebuckets)
                        {
                            var OrderStreakCustomerTransactionList = mongoDbGameStreakCustomerTransaction.Select(x => x.CustomerId == Items.CustomerId
                            && x.BucketNo == Items.BucketNo && x.StreakId >= Items.StreakIdFrom && x.StreakId <= Items.StreakIdTo && x.StreakIsFulfill == true).ToList();

                            if (OrderStreakCustomerTransactionList != null && OrderStreakCustomerTransactionList.Count > 0)
                            {
                                var OrderDtlList = OrderStreakCustomerTransactionList.SelectMany(x => x.OrderId);
                                //string strOrderId = string.Join(",", OrderDtlList);

                                var GameBucketRewardId = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == Items.CustomerId && x.BucketNo == Items.BucketNo && x.GameBucketRewardId > 0).Select(x => x.GameBucketRewardId).FirstOrDefault();
                                var OrderStatusCondition = db.GameBucketRewards.Where(x => x.Id == GameBucketRewardId).Select(x => x.RewardApproveStatus).FirstOrDefault();
                                //var OrderStatusCondition = "Delivered";

                                var MultilpeOrderList = db.DbOrderMaster.Where(x => OrderDtlList.Contains(x.OrderId) && x.Status == OrderStatusCondition).ToList();
                                var OrderList = MultilpeOrderList.Where(x => x.Status == OrderStatusCondition).ToList();
                                var OrderListCancel = MultilpeOrderList.Where(x => x.Status == "Order Canceled"
                                || x.Status == "Post Order Canceled" || x.Status == "Delivery Canceled").ToList();

                                //var OrderStatus = OrderList.Where(x => x.Status == OrderStatusCondition).Count();
                                //if (OrderList.Count() == OrderStatus)
                                if (OrderList != null && OrderList.Count() > 0)
                                {
                                    string RewardStatus = "Completed";

                                    //Past Bucket
                                    var chkPastStreak = OrderStreakCustomerTransactionList.Where(z => z.BucketEndDate.Date < curDate).Count();
                                    if (chkPastStreak > 0)
                                    {
                                        if (MultilpeOrderList.Count() == OrderListCancel.Count())
                                        {
                                            RewardStatus = "Cancelled";
                                        }
                                    }

                                    Items.IsCompleted = true;
                                    Items.RewardStatus = RewardStatus;
                                    Items.ModifiedDate = curDate;
                                    Items.ModifiedBy = 1;
                                    mongoDbGameStreakLevelRewardValue.ReplaceWithoutFind(Items.Id, Items);

                                    foreach (var m in OrderStreakCustomerTransactionList)
                                    {
                                        m.IsRewardProvided = true;
                                        mongoDbGameStreakCustomerTransaction.Replace(m.Id, m);
                                    }

                                    #region CustomerLedger Update Streak Status Completed
                                    UpdateCustomerLedgerStatusDc objLedgerInput = new UpdateCustomerLedgerStatusDc();
                                    objLedgerInput.CustomerId = Items.CustomerId;
                                    objLedgerInput.GameBucketNo = 0;
                                    objLedgerInput.BucketNo = Items.BucketNo;
                                    objLedgerInput.ForRewardStrack = 2;
                                    objLedgerInput.StreakIdFrom = Items.StreakIdFrom;
                                    objLedgerInput.StreakIdTo = Items.StreakIdTo;
                                    objLedgerInput.GameBucketRewardId = 0;// GameBucketRewardId;
                                    objLedgerInput.GameStreakLevelConfigMasterId = Items.GameStreakLevelConfigMasterId;
                                    objLedgerInput.GameStreakLevelConfigDetailId = Items.GameStreakLevelConfigDetailId;
                                    objLedgerInput.IsUpComingReward = false;
                                    if (RewardStatus == "Cancelled")
                                    {
                                        objLedgerInput.IsCompleted = false;
                                        objLedgerInput.IsCanceled = true;
                                    }
                                    else
                                    {
                                        objLedgerInput.IsCompleted = true;
                                    }
                                    var b = await CustomerLedger_UpdateStatus(objLedgerInput);
                                    #endregion
                                }
                            }
                        }
                    }

                    #endregion
                }



            }
            catch (Exception ex)
            {

                throw ex;
            }

            return flag;
        }



        public async Task<List<MappingRersultDc>> ValidateCancelOrder(string SqlQuery, string OrderIds, long BucketRewardId
                 , int iCustomerId, int iBucketNo, int GameBucketNo, List<GameBucketConditionData> bucketConditions)
        {
            bool flag = false;
            DateTime curDate = DateTime.Now.Date;
            string RewardApproveStatus = "";
            List<MappingRersultDc> MappintResultList = new List<MappingRersultDc>();


            using (var db = new AuthContext())
            {
                //-----S-----SQL Condition -------------------
                Int32 curRewardCredit = 0; string curRewardStatus = "";
                string strSqlQuery = "";
                string strQuery = "";
                int TotOrderCount = OrderIds.Split(',').ToList().Count();
                string OrderStatus = "";


                //var SqlQueryList = bucketConditions.Where(x => x.bucketNo == iBucketNo && x.SqlQuery != "").ToList();
                var SqlQueryList = bucketConditions.Where(x => x.GameBucketRewardId == BucketRewardId && x.SqlQuery != "").ToList();
                if (SqlQueryList.Count > 0)
                {
                    curRewardCredit = SqlQueryList.Select(x => x.Value).FirstOrDefault();
                    RewardApproveStatus = SqlQueryList.Select(x => x.RewardApproveStatus).FirstOrDefault();
                    strSqlQuery = SqlQueryList.Select(x => x.SqlQuery).FirstOrDefault();

                    strQuery = "";
                    strQuery = strSqlQuery;
                    strQuery = strQuery.Replace("@OrderId", OrderIds);
                    var resultOrderWithNoCondition = db.Database.SqlQuery<ResultQueryDc>(strQuery).FirstOrDefault();

                    if (resultOrderWithNoCondition != null)
                    {
                        strQuery = "";
                        strQuery = strSqlQuery;
                        strQuery = strQuery.Replace("@OrderId", OrderIds);
                        //strQuery = strQuery.Replace("@Status", RewardApproveStatus);
                        strQuery = strQuery + " and a.Status in ('" + RewardApproveStatus + "') ";
                        strQuery = strQuery + " and a.active=1 and a.Deleted=0 ";
                        var resultOrderWithCondition_Status = db.Database.SqlQuery<ResultQueryDc>(strQuery).FirstOrDefault();

                        strQuery = "";
                        strQuery = strSqlQuery;
                        strQuery = strQuery.Replace("@OrderId", OrderIds);
                        strQuery = strQuery + " and a.Status in ('Order Canceled','Post Order Canceled','Delivery Canceled') ";
                        var resultOrderWithCancelStatus = db.Database.SqlQuery<ResultQueryDc>(strQuery).FirstOrDefault();

                        MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                        var CustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == iCustomerId && x.BucketNo == iBucketNo
                        && x.IsActive == true && x.IsDeleted == false).ToList(); //&& x.BucketEndDate < curDate && x.NextBucketNo > 0

                        foreach (var t in SqlQueryList)
                        {
                            string SqlResultStatus = "";
                            double ConigrationConditionValue = t.ConditionValue;
                            double ResConditionValue = 0;
                            double WithNoCondition = 0; double WithCancelStatus = 0;

                            if (t.Condition == "OrderCount")
                            {
                                WithNoCondition = resultOrderWithNoCondition.OrderCount;
                                WithCancelStatus = resultOrderWithCancelStatus.OrderCount;
                                ResConditionValue = resultOrderWithCondition_Status.OrderCount;
                                SqlResultStatus = ResConditionValue >= ConigrationConditionValue ? "Success" : "Failed";
                            }

                            if (t.Condition == "TotalOrderAmount")
                            {
                                WithNoCondition = resultOrderWithNoCondition.TotalOrderAmount;
                                WithCancelStatus = resultOrderWithCancelStatus.TotalOrderAmount;
                                ResConditionValue = resultOrderWithCondition_Status.TotalOrderAmount;
                                SqlResultStatus = ResConditionValue >= ConigrationConditionValue ? "Success" : "Failed";
                            }

                            if (t.Condition == "OrderDeliveryAmount")
                            {
                                WithNoCondition = resultOrderWithNoCondition.OrderDeliveryAmount;
                                WithCancelStatus = resultOrderWithCancelStatus.OrderDeliveryAmount;
                                ResConditionValue = resultOrderWithCondition_Status.OrderDeliveryAmount;
                                SqlResultStatus = ResConditionValue >= ConigrationConditionValue ? "Success" : "Failed";
                            }

                            if (t.Condition == "AvgLineItem")
                            {
                                WithNoCondition = resultOrderWithNoCondition.AvgLineItem;
                                WithCancelStatus = resultOrderWithCancelStatus.AvgLineItem;
                                ResConditionValue = resultOrderWithCondition_Status.AvgLineItem;
                                SqlResultStatus = ResConditionValue >= ConigrationConditionValue ? "Success" : "Failed";
                            }


                            if (SqlResultStatus == "Failed")
                            {
                                if (CustomerBucketHdrList.Count > 0)
                                {
                                    //Useing For Current Bucket only.
                                    if (WithNoCondition == WithCancelStatus)
                                    {
                                        SqlResultStatus = "Cancelled";
                                        ResConditionValue = WithCancelStatus;
                                    }
                                }
                                else
                                {
                                    double resReminValues = WithNoCondition >= ConigrationConditionValue
                                        ? WithNoCondition - WithCancelStatus : 0;
                                    SqlResultStatus = WithNoCondition >= ConigrationConditionValue
                                        ? ConigrationConditionValue > resReminValues ? "Cancelled" : "Failed"
                                        : "Failed";
                                    if (SqlResultStatus == "Cancelled") { ResConditionValue = WithCancelStatus; }
                                }
                            }


                            if (SqlResultStatus != string.Empty)
                            {
                                MappingRersultDc obj = new MappingRersultDc();
                                obj.MappingCondiName = t.Condition;
                                obj.SqlOutPut = ResConditionValue; // x.ConditionValue;
                                obj.SqlResultStatus = SqlResultStatus;// ResConditionValue >= x.ConditionValue ? "Success" : "Failed";
                                obj.CustomerId = iCustomerId;
                                obj.BucketNo = iBucketNo;
                                obj.GameBucketNo = GameBucketNo;
                                obj.BucketRerward = curRewardCredit;
                                obj.ConditionId = t.ConditionId;
                                obj.ApiDes = t.AppDesc;
                                obj.CreatedDate = DateTime.Now;
                                obj.IsActive = true;
                                obj.CreatedBy = 1717;
                                MappintResultList.Add(obj);
                            }
                        }

                    }
                }

            }



            return MappintResultList;
        }


        [HttpGet]
        [Route("BucketLevelPeriodUpJob")]
        public async Task<bool> BucketLevelPeriodUpJob_MidNight()
        {
            bool flag = false;
            DateTime curDate = DateTime.Now.Date;
            using (var db = new AuthContext())
            {
                MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();

                #region CustomerBucketHdr ==> Change BucktNo & NextBucketNo
                var CustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true
                && x.IsDeleted == false && x.BucketEndDate < curDate && x.NextBucketNo > 0).ToList();

                foreach (var Hdr in CustomerBucketHdrList)
                {
                    Hdr.BucketNo = Hdr.NextBucketNo;
                    Hdr.NextBucketNo = 0;
                    Hdr.ModifiedDate = DateTime.Now;
                    flag = mongoDbHelperCustomerBucketHdr.ReplaceWithoutFind(Hdr.Id, Hdr);
                }
                #endregion 

                #region GameStreakCustomerTransaction => Expired Streak
                MongoHelper<GameStreakCustomerTransaction> mongohelper = new MongoHelper<GameStreakCustomerTransaction>();
                var customerStreaks = (await mongohelper.SelectAsync(x => !x.IsExpired && x.IsActive == true
                   && x.IsDeleted == false)).ToList();
                foreach (var listItems in customerStreaks.GroupBy(x => x.CustomerId))
                {
                    if (listItems.Max(x => x.BucketEndDate.Date) < curDate)
                    {
                        #region Expired All Streak
                        foreach (var item in listItems.ToList())
                        {
                            item.IsExpired = true;
                            item.ModifiedDate = curDate;
                            mongohelper.Replace(item.Id, item);
                        }
                        #endregion

                        #region Reset Streak field for using create New stricks
                        var CustomerBucketHdr = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false
                        && x.CustomerId == listItems.Key && x.IsStreakCreated == true).FirstOrDefault();
                        if (CustomerBucketHdr != null)
                        {
                            CustomerBucketHdr.IsStreakCreated = false;
                            flag = mongoDbHelperCustomerBucketHdr.ReplaceWithoutFind(CustomerBucketHdr.Id, CustomerBucketHdr);
                        }
                        #endregion 
                    }
                }
                #endregion 
            }



            return flag;
        }

        public async Task<bool> StreakLevelReward_Insert()
        {
            bool flag = false;
            DateTime curDate = DateTime.Now;

            MongoHelper<GameStreakCustomerTransaction> mongohelper = new MongoHelper<GameStreakCustomerTransaction>();
            var customerStreaks = (await mongohelper.SelectAsync(x => !x.IsExpired && x.IsActive == true
               && x.IsDeleted == false)).ToList();

            using (var db = new AuthContext())
            {
                foreach (var listItems in customerStreaks.GroupBy(x => x.CustomerId))
                {
                    ///Validate and wallet 
                    var applicablebuckets = listItems.Where(x => x.BucketEndDate <= curDate);
                    if (listItems.Where(x => x.BucketEndDate <= curDate).Any())
                    {
                        #region Validate and wallet 

                        List<GameStreakLevelRewardValueDC> InsertStreakLevelRewardList = new List<GameStreakLevelRewardValueDC>();
                        List<CreateCustomerLedgerDc> InsertNewCustomerLedgerList = new List<CreateCustomerLedgerDc>();

                        //int BucketNo = applicablebuckets.Select(x => x.BucketNo).Distinct().FirstOrDefault();
                        //int BucketNoTo = 0; int skip1 = 0; int take1 = 0; int IsActiveCurrent1 = 0;
                        //var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
                        //var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                        //var fskip = new SqlParameter("@skip", skip1);
                        //var ftake = new SqlParameter("@take", take1);
                        //var CreatedDate = new SqlParameter("@CreatedDate", DBNull.Value);
                        //var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);
                        ////var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakConfig>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take", fBucketNoFrom, fBucketNoTo, fskip, ftake).ToList();
                        //var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, CreatedDate, fIsActiveCurrent).ToList();

                        int BucketNo = applicablebuckets.Select(x => x.BucketNo).Distinct().FirstOrDefault();
                        int IsActiveCurrent1 = 0;
                        var SqlStreakLevelConfigMasters = GetStreakConfiguration(db, BucketNo, null, IsActiveCurrent1);

                        var dataStreakLevelConfigIDs = applicablebuckets.Select(x => x.GameStreakLevelConfigMasterId).Distinct().FirstOrDefault();

                        MongoDbHelper<GameStreakLevelRewardValue> gamesterak = new MongoDbHelper<GameStreakLevelRewardValue>();
                        var LevelRewardValue = gamesterak.Select(x => x.BucketNo == BucketNo && x.CustomerId == listItems.Key).Select(x => new { x.GameStreakLevelConfigMasterId, x.GameStreakLevelConfigDetailId }).ToList();
                        var LvlRewardValSingle = LevelRewardValue.Where(x => x.GameStreakLevelConfigDetailId == 0).Select(d => d.GameStreakLevelConfigMasterId).Distinct().ToList();
                        var dataStreakLevelConfigMastersXXX = SqlStreakLevelConfigMasters.Where(x => !LvlRewardValSingle.Contains(x.Id)).ToList();
                        var IndividaulLvlId = LevelRewardValue.Where(x => x.GameStreakLevelConfigDetailId > 0).Select(d => d.GameStreakLevelConfigMasterId).Distinct().FirstOrDefault();
                        var IndividaulLvlIdList = LevelRewardValue.Where(x => x.GameStreakLevelConfigDetailId > 0).Select(d => d.GameStreakLevelConfigDetailId).Distinct().ToList();
                        var dataStreakLevelConfigMastersYYY = dataStreakLevelConfigMastersXXX.Where(x => !IndividaulLvlIdList.Contains(x.GameStreakLevelConfigDetailId)).ToList();

                        //var dataStreakLevelConfigMasters = SqlStreakLevelConfigMasters.Where(x => dataStreakLevelConfigIDs.Contains(x.Id)).OrderBy(x => x.StreakConditionType).ToList();
                        var dataStreakLevelConfigMasters = dataStreakLevelConfigMastersYYY.Where(x => dataStreakLevelConfigIDs.Contains(x.Id)).OrderBy(x => x.StreakConditionType).ToList();
                        foreach (var StreakLevelConfig in dataStreakLevelConfigMasters.ToList())
                        {
                            GameStreakLevelRewardValueDC objInsertStreakLevelReward = new GameStreakLevelRewardValueDC();
                            CreateCustomerLedgerDc objNewLedgerEntry = new CreateCustomerLedgerDc();

                            int StreakIdFrom = 0;
                            int StreakIdTo = 0;
                            double RewardValue = 0;

                            //----S------------
                            var applicablebucketsFilltered = applicablebuckets.Select(x => new GameStreakCustomerTransactionDc
                            {
                                BucketEndDate = x.BucketEndDate,
                                BucketNo = x.BucketNo,
                                BucketStartDate = x.BucketStartDate,
                                CreatedBy = x.CreatedBy,
                                CreatedDate = x.CreatedDate,
                                CustomerId = x.CustomerId,
                                GameStreakLevelConfigMasterId = x.GameStreakLevelConfigMasterId,
                                //Id=x.Id, 
                                IsActive = x.IsActive,
                                IsDeleted = x.IsDeleted,
                                IsExpired = x.IsExpired,
                                IsOrderCancel = x.IsOrderCancel,
                                IsRewardProvided = x.IsRewardProvided,
                                ModifiedBy = x.ModifiedBy,
                                ModifiedDate = x.ModifiedDate,
                                OrderId = x.OrderId,
                                StreakId = x.StreakId,
                                StreakIsFulfill = x.StreakIsFulfill,
                                IsStreakContinue = 0
                            }).ToList();


                            applicablebucketsFilltered.ToList().ForEach(x =>
                            {
                                if (x.StreakIsFulfill)
                                {
                                    x.IsStreakContinue = applicablebucketsFilltered.Any(y => y.StreakId > x.StreakId) ?
                                      (applicablebucketsFilltered.Where(y => y.StreakId < x.StreakId).OrderByDescending(y => y.StreakId).Select(y => y.IsStreakContinue).FirstOrDefault() + 1) : 1;
                                }
                                else
                                    x.IsStreakContinue = 0;
                            }
                            );

                            var RewardValueData = StreakLevelReward_ValidateStreakConditionType(StreakLevelConfig.StreakConditionType, StreakLevelConfig, applicablebucketsFilltered, dataStreakLevelConfigMasters, false);
                            StreakIdFrom = RewardValueData.StreakIdFrom;
                            StreakIdTo = RewardValueData.StreakIdTo;
                            RewardValue = RewardValueData.RewardValue;
                            //----E------------

                            if (StreakIdFrom != 0 && StreakIdTo != 0 && RewardValue != 0)
                            {
                                var CustomerIdInsert = applicablebuckets.FirstOrDefault().CustomerId;
                                var BucketNoInsert = applicablebuckets.FirstOrDefault().BucketNo;
                                var BucketStartDateInsert = applicablebuckets.FirstOrDefault(x => x.StreakId == StreakIdFrom).BucketStartDate;
                                var BucketEndDateInsert = applicablebuckets.FirstOrDefault(x => x.StreakId == StreakIdTo).BucketEndDate;

                                objInsertStreakLevelReward.CustomerId = CustomerIdInsert;
                                objInsertStreakLevelReward.BucketNo = BucketNoInsert;
                                objInsertStreakLevelReward.StreakIdFrom = StreakIdFrom;
                                objInsertStreakLevelReward.StreakIdTo = StreakIdTo;
                                objInsertStreakLevelReward.GameStreakLevelConfigMasterId = StreakLevelConfig.Id;
                                objInsertStreakLevelReward.GameStreakLevelConfigDetailId = StreakLevelConfig.GameStreakLevelConfigDetailId;
                                objInsertStreakLevelReward.RewardValueCr = RewardValue;
                                objInsertStreakLevelReward.RewardValueDr = 0;
                                objInsertStreakLevelReward.RemaningRewardAmount = 0;
                                objInsertStreakLevelReward.ReferGameStreakLevelConfigMasterId = 0;
                                objInsertStreakLevelReward.RewardStatus = "InProcess";
                                objInsertStreakLevelReward.IsCompleted = false;
                                objInsertStreakLevelReward.IsCancelRewardSettled = false;///false: done, true: pending for settle of cancel order.
                                objInsertStreakLevelReward.BucketStartDate = BucketStartDateInsert; //applicablebuckets.FirstOrDefault(x => x.StreakId == objInsertStreakLevelReward.StreakIdFrom).BucketStartDate;
                                objInsertStreakLevelReward.BucketEndDate = BucketEndDateInsert;  //applicablebuckets.FirstOrDefault(x => x.StreakId == objInsertStreakLevelReward.StreakIdTo).BucketEndDate;

                                InsertStreakLevelRewardList.Add(objInsertStreakLevelReward);


                                objNewLedgerEntry.CustomerId = CustomerIdInsert;
                                objNewLedgerEntry.GameBucketNo = 0;
                                objNewLedgerEntry.BucketNo = BucketNoInsert;
                                objNewLedgerEntry.ForRewardStrack = 2;//"Reward=1  / Strack=2
                                objNewLedgerEntry.StreakIdFrom = StreakIdFrom;
                                objNewLedgerEntry.StreakIdTo = StreakIdTo;
                                objNewLedgerEntry.GameBucketRewardId = 0;
                                objNewLedgerEntry.GameStreakLevelConfigMasterId = StreakLevelConfig.Id;
                                objNewLedgerEntry.GameStreakLevelConfigDetailId = StreakLevelConfig.GameStreakLevelConfigDetailId;
                                objNewLedgerEntry.RewardValue = RewardValue;
                                objNewLedgerEntry.IsUpComingReward = true;
                                objNewLedgerEntry.IsCompleted = false;
                                objNewLedgerEntry.IsCanceled = false;
                                objNewLedgerEntry.BucketStartDate = BucketStartDateInsert;
                                objNewLedgerEntry.BucketEndDate = BucketEndDateInsert;
                                InsertNewCustomerLedgerList.Add(objNewLedgerEntry);
                            }
                        }

                        if (InsertStreakLevelRewardList != null && InsertStreakLevelRewardList.Count > 0)
                        {
                            InsertGameStreakLevelRewardValue(InsertStreakLevelRewardList);
                            var b = await CustomerLedger_Create(InsertNewCustomerLedgerList);
                        }
                        #endregion
                    }
                }
            }
            return flag;
        }

        public class StreakLevelRewardDC
        {
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public double RewardValue { get; set; }
        }

        public StreakLevelRewardDC StreakLevelReward_ValidateStreakConditionType(int StreakConditionType, GameStreakLevelConfigDetailDc StreakLevelConfig
            , List<GameStreakCustomerTransactionDc> applicablebuckets, dynamic dataStreakLevelConfigMasters, bool StreakAchivedCount)
        {
            int StreakIdFrom = 0;
            int StreakIdTo = 0;
            double RewardValue = 0;
            StreakLevelRewardDC objReturn = new StreakLevelRewardDC();

            if (StreakLevelConfig.StreakConditionType == 1) //Level-Individual
            {
                //var IndividualLevelValue = ((IEnumerable<dynamic>)applicablebuckets).Count(z => z.StreakId == StreakLevelConfig.LevelNo && z.StreakIsFulfill == true) > 0 ? StreakLevelConfig.LevelValue : 0;
                var IndividualLevelValue = applicablebuckets.Count(z => z.StreakId == StreakLevelConfig.LevelNo && z.IsStreakContinue != 0 && z.StreakIsFulfill == true) > 0 ? StreakLevelConfig.LevelValue : 0;
                if (IndividualLevelValue > 0)
                {
                    StreakIdFrom = StreakLevelConfig.LevelNo;
                    StreakIdTo = StreakLevelConfig.LevelNo;
                    RewardValue = StreakLevelConfig.LevelValue;
                }
            }
            if (StreakLevelConfig.StreakConditionType == 3) //Outof
            {
                var applicablebucketsCount = 0;
                //applicablebucketsCount = ((IEnumerable<dynamic>)applicablebuckets).Count(z => z.StreakIsFulfill == true);
                applicablebucketsCount = applicablebuckets.Count(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0);

                if (applicablebucketsCount >= StreakLevelConfig.OutOf_TotalBucket)
                {
                    StreakIdFrom = ((IEnumerable<dynamic>)applicablebuckets).Min(x => x.StreakId);
                    StreakIdTo = ((IEnumerable<dynamic>)applicablebuckets).Max(x => x.StreakId);
                    RewardValue = StreakLevelConfig.RewardValue;
                }

                if (StreakAchivedCount == true && StreakIdFrom == 0 && StreakIdTo == 0 && RewardValue == 0)
                {
                    StreakIdTo = applicablebucketsCount - 1;
                    if (applicablebucketsCount == 1)
                    {
                        StreakIdTo = applicablebucketsCount;
                        StreakIdFrom = StreakIdTo;
                    }
                }
            }
            if (StreakLevelConfig.StreakConditionType == 2) //Streak
            {
                double creditReward = 0;
                if (applicablebuckets.Any(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0 && z.IsStreakContinue == StreakLevelConfig.Streak_StreakCount))
                {
                    StreakIdFrom = applicablebuckets.Where(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0).Min(z => z.IsStreakContinue);
                    StreakIdTo = applicablebuckets.Where(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0 && z.IsStreakContinue == StreakLevelConfig.Streak_StreakCount).Max(z => z.IsStreakContinue);

                    StreakIdTo = applicablebuckets.FirstOrDefault(z => z.IsStreakContinue == StreakIdTo).StreakId;
                    StreakIdFrom = applicablebuckets.Where(x => x.IsStreakContinue == StreakIdFrom && x.StreakId < StreakIdTo).Max(x => x.StreakId);

                    creditReward = StreakLevelConfig.RewardValue;

                    if (StreakLevelConfig.Streak_ChooseReward == 1)//Multiplier
                    {
                        creditReward = ((IEnumerable<dynamic>)dataStreakLevelConfigMasters).Where(s => s.LevelNo >= StreakIdFrom && s.LevelNo <= StreakIdTo).Sum(x => x.LevelValue);
                        creditReward = creditReward * StreakLevelConfig.RewardValue;
                    }

                    RewardValue = creditReward;
                }
                if (StreakAchivedCount == true && StreakIdFrom==0 && StreakIdTo==0)
                {
                    StreakIdFrom = applicablebuckets.Where(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0).Min(z => z.IsStreakContinue);
                    //StreakIdTo = applicablebuckets.Where(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0).Max(z=>z.IsStreakContinue);
                    StreakIdTo = applicablebuckets.Where(z => z.StreakIsFulfill == true && z.IsStreakContinue != 0).OrderByDescending(z => z.StreakId).Select(z => z.StreakId).FirstOrDefault();

                    //StreakIdTo = applicablebuckets.FirstOrDefault(z => z.IsStreakContinue == StreakIdTo).StreakId;
                    StreakIdFrom = applicablebuckets.Where(x => x.IsStreakContinue == StreakIdFrom && x.StreakId<= StreakIdTo).Max(x => x.StreakId);
                }

                //var TotalLoop = ((IEnumerable<dynamic>)applicablebuckets).Where(x => x.StreakIsFulfill == true).OrderBy(x => x.StreakId).ToList();
                //int i = 1;
                //int iSecond = 0;
                //int iLoopFor = 1;

                //foreach (var iLoop in TotalLoop)
                //{
                //    if (iLoop.StreakId == (i + iSecond))
                //    {
                //        if ((StreakLevelConfig.Streak_StreakCount == i && iSecond == 0)
                //            || (StreakLevelConfig.Streak_StreakCount == (iSecond)))
                //        {
                //            if (iSecond == 0)
                //            {
                //                iSecond = i;
                //                i = 1;
                //            }
                //            else
                //            {
                //                i++;
                //                iSecond = iLoop.StreakId;
                //            }

                //            creditReward = StreakLevelConfig.RewardValue;

                //            if (StreakLevelConfig.Streak_ChooseReward == 1)//Multiplier
                //            {
                //                creditReward = ((IEnumerable<dynamic>)dataStreakLevelConfigMasters).Where(s => s.LevelNo >= i && s.LevelNo <= iSecond).Sum(x => x.LevelValue);
                //                creditReward = creditReward * StreakLevelConfig.RewardValue;
                //            }

                //            StreakIdFrom = i;
                //            StreakIdTo = iSecond;
                //            RewardValue = creditReward;
                //            break;
                //        }
                //    }
                //    else
                //    {
                //        i = iLoop.StreakId - 1;
                //        iSecond = 0;
                //        iSecond = 1;// 1 + 1;
                //    }

                //    if (StreakAchivedCount == true && iLoopFor == TotalLoop.Count())
                //    {
                //        if (iSecond == 0)
                //        { StreakIdTo = i; StreakIdFrom = 1; }
                //        else
                //        { StreakIdTo = iSecond - 1; }
                //    }

                //    if (iSecond == 0)
                //    { i++; }
                //    else
                //    { iSecond++; }

                //    iLoopFor++;
                //}

                //    //    //int count = StreakLevelConfig.Streak_StreakCount;
                //    //    List<int> numbers = applicablebuckets
                //    //       .Where(p => applicablebuckets.Where(q => q.StreakId >= p.StreakId && q.StreakId < p.StreakId + StreakLevelConfig.Streak_StreakCount && p.StreakIsFulfill == true).Count() == StreakLevelConfig.Streak_StreakCount)
                //    //       .Select(p => Enumerable.Range(p.StreakId, StreakLevelConfig.Streak_StreakCount).ToList())
                //    //       .Distinct().FirstOrDefault();

                //    //    if (numbers != null && numbers.Count > 0)
                //    //    {
                //    //        creditReward = StreakLevelConfig.RewardValue;
                //    //        StreakIdFrom = numbers.FirstOrDefault();
                //    //        StreakIdTo = numbers.LastOrDefault();

                //    //        if (StreakLevelConfig.Streak_ChooseReward == 1)//Multiplier
                //    //        {
                //    //            creditReward = dataStreakLevelConfigMasters.Where(s => s.LevelNo >= StreakIdFrom && s.LevelNo <= StreakIdTo).Sum(x => x.LevelValue);
                //    //            creditReward = creditReward * StreakLevelConfig.RewardValue;
                //    //        }
                //    //        RewardValue = creditReward;
                //    //    }

            }



            objReturn.StreakIdFrom = StreakIdFrom;
            objReturn.StreakIdTo = StreakIdTo;
            objReturn.RewardValue = RewardValue;
            return objReturn;
        }


        [HttpGet]
        [Route("UpdateCancelOrderStreakRewardJob")]
        public async Task<string> UpdateCancelOrderStreakReward()
        {
            string flag = "";
            DateTime curDate = DateTime.Now.Date;

            using (var db = new AuthContext())
            {
                List<GameStreakLevelRewardValueDC> InsertStreakLevelRewardList = new List<GameStreakLevelRewardValueDC>();

                MongoHelper<GameStreakCustomerTransaction> mongohelper = new MongoHelper<GameStreakCustomerTransaction>();
                var customerStreaks = (await mongohelper.SelectAsync(x => !x.IsExpired //&& x.IsOrderCancel == false
                   && x.IsActive == true && x.IsDeleted == false)).ToList();
                foreach (var listItems in customerStreaks.GroupBy(x => x.CustomerId))
                {
                    var applicablebuckets = listItems.ToList().Where(x => x.BucketEndDate < curDate && x.OrderId != null);
                    //var dataOrderIDs = applicablebuckets.Select(x => new {x.StreakId, x.OrderId}).Distinct();
                    var dataOrderIDs = applicablebuckets.SelectMany(x => x.OrderId).Distinct();

                    //List<int> dataOrderIDs1 = new List<int>();
                    //foreach (var item in dataOrderIDs)
                    //{dataOrderIDs1.AddRange(item);}

                    List<int> CancelOrderStreakId = new List<int>();
                    var CancelOrder = db.DbOrderMaster.Where(x => x.Status == "Order Canceled" && dataOrderIDs.Contains(x.OrderId)).Select(x => x.OrderId).ToList();  //&& dataOrderIDs.Contains(x.OrderId)
                    if (CancelOrder.Count > 0)
                    {
                        CancelOrder.ForEach(order =>
                        {
                            CancelOrderStreakId.Add(applicablebuckets.Where(x => x.OrderId.Contains(order)).Select(x => x.StreakId).FirstOrDefault());
                        });


                        foreach (var itemt in listItems)
                        {
                            if (CancelOrderStreakId.Where(x => x == itemt.StreakId).Count() > 0)
                            {
                                itemt.IsOrderCancel = true;
                                itemt.ModifiedDate = DateTime.Now;
                                itemt.ModifiedBy = 1;
                                bool flag11 = mongohelper.Replace(itemt.Id, itemt);


                                GameStreakLevelRewardValueDC objInsertStreakLevelReward = new GameStreakLevelRewardValueDC();
                                objInsertStreakLevelReward.CustomerId = itemt.CustomerId;
                                objInsertStreakLevelReward.BucketNo = itemt.BucketNo;
                                objInsertStreakLevelReward.StreakIdFrom = itemt.StreakId;
                                objInsertStreakLevelReward.StreakIdTo = itemt.StreakId;
                                objInsertStreakLevelReward.GameStreakLevelConfigMasterId = 0;
                                objInsertStreakLevelReward.RewardValueCr = 0;

                                objInsertStreakLevelReward.RewardValueDr = 0;
                                objInsertStreakLevelReward.RemaningRewardAmount = 0;
                                objInsertStreakLevelReward.ReferGameStreakLevelConfigMasterId = 0;
                                objInsertStreakLevelReward.IsCancelRewardSettled = true; ///false: done, true: pending for settle of cancel order.

                                InsertStreakLevelRewardList.Add(objInsertStreakLevelReward);
                            }
                        }

                        if (InsertStreakLevelRewardList != null && InsertStreakLevelRewardList.Count > 0)
                        { InsertGameStreakLevelRewardValue(InsertStreakLevelRewardList); }

                    }
                }
            }
            return flag;
        }

        #region Test Method

        [Route("Temp")]
        [HttpGet]
        public bool temp()
        {
            MongoDbHelper<GameCustomerBucketHdr> mongohelper = new MongoDbHelper<GameCustomerBucketHdr>();
            var dataCustomerBucketHdrList = mongohelper.Select(x => x.IsActive == true && x.IsDeleted == false).ToList();

            foreach (var item in dataCustomerBucketHdrList)
            {
                item.CRMBucketNo = 2;
                bool flag11 = mongohelper.Replace(item.Id, item);
            }

            return true;
        }

        [HttpGet]
        [Route("InsertGameConditionMastersMongo")]
        public async Task<bool> InsertGameConditionMastersMongo()
        {
            try
            {
                MongoDbHelper<GameConditionMastersMongo> mongohelper = new MongoDbHelper<GameConditionMastersMongo>();
                List<GameConditionMastersMongo> Condition = new List<GameConditionMastersMongo>();

                using (AuthContext context = new AuthContext())
                {

                    for (int i = 1; i <= 2; i++)
                    {
                        GameConditionMastersMongo obj = new GameConditionMastersMongo();
                        if (i == 1)
                        {
                            obj.Name = "IncreaseDay";
                            obj.Value = 10;
                        }

                        if (i == 2)
                        {
                            obj.Name = "StreakDuration";
                            obj.Value = 9;
                        }
                        //if (i == 3)
                        //{
                        //    obj.Name = "OrderDate";
                        //    obj.Date = Convert.ToDateTime("2023-06-27");
                        //}
                        obj.CreatedDate = DateTime.Now;
                        obj.CreatedBy = 1;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        Condition.Add(obj);
                    }
                }

                var flag = mongohelper.InsertMany(Condition);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }


        [Route("GetCustomerAchiveLevel")]
        [HttpGet]
        public async Task<List<CustomerAchiveLevelDc>> GetCustomerAchiveLevel(int CustomerId, int skip, int take, string lang)
        {
            using (var db = new AuthContext())
            {
                List<CustomerAchiveLevelDc> list = new List<CustomerAchiveLevelDc>();

                MongoDbHelper<GameCustomerBucketHdr> mongohelper = new MongoDbHelper<GameCustomerBucketHdr>();
                var dataCustomerBucketHdrList = mongohelper.Select(x => x.NextBucketNo > 0 && x.IsActive == true && x.IsDeleted == false && x.CustomerId != CustomerId).OrderByDescending(x => x.BucketNo).Skip(skip).Take(take).ToList();
                var custids = dataCustomerBucketHdrList.Select(x => x.CustomerId).Distinct().ToList();
                var customer = await db.Customers.Where(x => custids.Contains(x.CustomerId)).ToListAsync();

                MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongohelperOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();


                foreach (var item in dataCustomerBucketHdrList.OrderByDescending(x => x.BucketNo))
                {
                    var res = customer.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                    var Reward = mongohelperOrderDetail.Select(x => x.CustomerId == item.CustomerId && x.IsCompleted == true && x.GameBucketNo == item.GameBucketNo).FirstOrDefault();


                    CustomerAchiveLevelDc obj = new CustomerAchiveLevelDc();
                    obj.CustomerName = res != null ? res.Name : null;
                    obj.ShopName = res != null ? res.ShopName : null;
                    obj.Level = item.NextBucketNo == 0 ? item.GameBucketNo : item.GameBucketNo - 1;
                    obj.Reward = Reward != null ? Reward.RewardCredit : 0;
                    //obj.Image = res.UploadProfilePichure != null && res.UploadProfilePichure != "" ? res.UploadProfilePichure : res.Shopimage;
                    var Image = res != null ? (res.UploadProfilePichure != null && res.UploadProfilePichure != "" ? res.UploadProfilePichure : res.Shopimage) : null;
                    obj.Image = (Image != null && Image != "") ? string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , "UploadedImages/" + Image) : null;

                    list.Add(obj);
                }
                return list.OrderByDescending(x => x.Level).ThenBy(x => x.Reward).ToList();
            }

        }


        [HttpGet]
        [Route("GetCRMDataJobTesting")]
        public async Task<bool> GetCRMDataJobTesting()
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    MongoDbHelper<GameCustomerBucketHdr> mongohelper = new MongoDbHelper<GameCustomerBucketHdr>();

                    //var SkcodeList = context.CustomerBucketGames.Select(x => x.SkCode).Distinct().ToList();
                    var SkcodeList = mongohelper.Select(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.SkCode).Distinct().ToList();


                    var today = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    //string query = "SELECT ccode skcode,bucketno,lastorderdate,startdate,enddate   from sk_crm_platform_ccodedata where enddate >= '{today}'";
                    //ElasticSqlHelper<ElasticCRMData> elasticSqlHelperData = new ElasticSqlHelper<ElasticCRMData>();
                    //var CRMData = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

                    var CRMData = context.Customers.Where(x => x.Active == true && x.Deleted == false).ToList();

                    var data = CRMData.Where(x => !SkcodeList.Contains(x.Skcode)).ToList();
                    var custIds = data.Select(x => x.Skcode).Distinct().ToList();
                    var CustomerList = context.Customers.Where(x => x.Deleted == false && x.Active == true).Select(x => new { x.CustomerId, x.Skcode }).Distinct().ToList();

                    List<GameCustomerBucketHdr> cust = new List<GameCustomerBucketHdr>();

                    foreach (var item in data)
                    {
                        Random random = new Random();
                        int test = random.Next(1, 10);

                        GameCustomerBucketHdr obj = new GameCustomerBucketHdr();
                        obj.SkCode = item.Skcode;
                        obj.CustomerId = CustomerList.Where(x => x.Skcode.ToLower() == item.Skcode.ToLower()).Select(x => x.CustomerId).FirstOrDefault();
                        obj.CRMBucketNo = test;
                        obj.BucketNo = test;
                        obj.NextBucketNo = 0;
                        obj.GameBucketNo = 1;
                        obj.LastOrderDate = DateTime.Now.AddDays(-1);
                        obj.BucketStartDate = DateTime.Now;
                        obj.BucketEndDate = DateTime.Now.AddDays(2);

                        obj.CreatedDate = DateTime.Now;
                        obj.IsActive = true;
                        obj.IsDeleted = false;

                        cust.Add(obj);
                    };
                    var flag = mongohelper.InsertMany(cust);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return true;
        }

        #endregion





        [HttpGet]
        [Route("GetLevelCustomerwise")]
        public HttpResponseMessage GetLevelCustomerwise(int? CustomerId)
        {
            using (var db = new AuthContext())
            {
                if (CustomerId > 0)
                {
                    GameCustomerLevesDC games = new GameCustomerLevesDC();
                    MongoDbHelper<GameCustomerBucketHdr> mongodbCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                    MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongodbCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                    var gameCustomerBucketHdr = mongodbCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (gameCustomerBucketHdr != null)
                    {
                        var gameCustomerBucketHdrOrderDetail = mongodbCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == gameCustomerBucketHdr.CustomerId && x.BucketNo == gameCustomerBucketHdr.BucketNo && x.IsActive == true && x.IsDeleted == false && x.IsCompleted == false).FirstOrDefault();
                        games.CustomerId = gameCustomerBucketHdr.CustomerId;
                        games.BucketNo = gameCustomerBucketHdr.BucketNo;
                        games.GameBucketNo = gameCustomerBucketHdr.GameBucketNo;
                        if (gameCustomerBucketHdrOrderDetail != null)
                        {
                            var Id = new SqlParameter("@CustomerId", gameCustomerBucketHdr.CustomerId);
                            var data = db.Database.SqlQuery<GetlevelcustomerwiseDC>("Sp_getlevelcustomerwise @CustomerId", Id).FirstOrDefault();
                            games.AppDesc = data.AppDesc;
                            games.RequiredAcheiveValue = data.RequiredAcheiveValue;
                            games.AcheiveValue = data.AcheiveValue;
                        }
                        else
                        {
                            var gameCustomerBucketHdrOrderDetails = mongodbCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == gameCustomerBucketHdr.CustomerId && x.BucketNo == gameCustomerBucketHdr.BucketNo && x.IsActive == true && x.IsDeleted == false && x.IsCompleted == true).FirstOrDefault();
                            if (gameCustomerBucketHdrOrderDetails != null)
                            {
                                games.IsCompleted = gameCustomerBucketHdrOrderDetails.IsCompleted;
                                games.RewardStatus = gameCustomerBucketHdrOrderDetails.RewardStatus;
                                games.RewardCredit = gameCustomerBucketHdrOrderDetails.RewardCredit;
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, games);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Data Found");
                    }

                }
                else
                {
                    List<GameCustomerLevesDC> gamelevel = new List<GameCustomerLevesDC>();
                    GameCustomerLevesDC games = new GameCustomerLevesDC();
                    MongoDbHelper<GameCustomerBucketHdr> mongodbCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                    MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongodbCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                    var gameCustomerBucketHdr = mongodbCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false).ToList();
                    if (gameCustomerBucketHdr.Count > 0)
                    {
                        foreach (var hdr in gameCustomerBucketHdr)
                        {
                            var gameCustomerBucketHdrOrderDetail = mongodbCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == hdr.CustomerId && x.BucketNo == hdr.BucketNo && x.IsActive == true && x.IsDeleted == false && x.IsCompleted == false).FirstOrDefault();
                            games.CustomerId = hdr.CustomerId;
                            games.BucketNo = hdr.BucketNo;
                            games.GameBucketNo = hdr.GameBucketNo;
                            if (gameCustomerBucketHdrOrderDetail != null)
                            {
                                var Id = new SqlParameter("@CustomerId", hdr.CustomerId);
                                var data = db.Database.SqlQuery<GetlevelcustomerwiseDC>("Sp_getlevelcustomerwise @CustomerId", Id).FirstOrDefault();
                                if (data != null)
                                {
                                    games.AppDesc = data.AppDesc;
                                    games.RequiredAcheiveValue = data.RequiredAcheiveValue;
                                    games.AcheiveValue = data.AcheiveValue;
                                }
                            }
                            else
                            {
                                var gameCustomerBucketHdrOrderDetails = mongodbCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == hdr.CustomerId && x.BucketNo == hdr.BucketNo && x.IsActive == true && x.IsDeleted == false && x.IsCompleted == true).FirstOrDefault();
                                if (gameCustomerBucketHdrOrderDetails != null)
                                {
                                    games.IsCompleted = gameCustomerBucketHdrOrderDetails.IsCompleted;
                                    games.RewardStatus = gameCustomerBucketHdrOrderDetails.RewardStatus;
                                    games.RewardCredit = gameCustomerBucketHdrOrderDetails.RewardCredit;
                                }

                            }
                            gamelevel.Add(games);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, gamelevel);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Data Found");
                    }
                }
            }

        }

        [Route("GetDataShowforConditionMaster")]
        [HttpGet]
        public HttpResponseMessage GetDataShowforConditionMaster(int skip, int take)
        {
            using (var db = new AuthContext())
            {
                GameBucketCondition gb = new GameBucketCondition();
                var data = db.Database.SqlQuery<GameBucketConditionDC>("Sp_GetDataForGameConditionMaster ").ToList();
                //var data = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (data != null)
                {
                    if (skip == 0 && take == 0)
                    {
                        gb.gamebucketcondition = data.ToList();
                    }
                    else
                    {
                        gb.gamebucketcondition = data.Skip(skip).Take(take).ToList();
                    }
                    gb.TotalRecords = data.Count();
                    var res = new
                    {
                        status = true,
                        Message = gb
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [Route("GetSearchDataByBucketNo")]
        [HttpGet]
        public HttpResponseMessage GetSearchDataByBucketNo(int FromBucketNo, int EndBucketNo, int filtertype)
        {
            using (var db = new AuthContext())
            {
                GameBucketRewards gb = new GameBucketRewards();
                if (FromBucketNo <= 0)
                {
                    var res = new
                    {
                        status = false,
                        Message = "Please Enter Start Bucket No."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                if (EndBucketNo <= 0)
                {
                    var res = new
                    {
                        status = false,
                        Message = "Please Enter End Bucket No."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                if (FromBucketNo > EndBucketNo)
                {
                    var res = new
                    {
                        status = false,
                        Message = "Please Enter Correct Bucket No."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                var FromBucket = new SqlParameter("@FromBucketNo", FromBucketNo);
                var EndBucket = new SqlParameter("@EndBucketNo", EndBucketNo);
                var data = db.Database.SqlQuery<GameBucketRewardsDC>("Sp_GetDataForGameBucketrRewardsByBucketNo @FromBucketNo,@EndBucketNo", FromBucket, EndBucket).ToList();
                //var data = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (data != null)
                {
                    //if (skip == 0 && take == 0)
                    //{
                    //gb.gameBucketRewards = data.ToList();
                    //gb.TotalRecords = data.Count();
                    //}
                    //else
                    //{
                    //    gb.gameBucketRewards = data.Skip(skip).Take(take).ToList();
                    //}



                    if (filtertype == 0)
                    {
                        gb.gameBucketRewards = data.ToList();
                        gb.TotalRecords = gb.gameBucketRewards.Count();
                    }
                    if (filtertype == 1)
                    {
                        gb.gameBucketRewards = data.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                        gb.TotalRecords = gb.gameBucketRewards.Count();
                    }
                    if (filtertype == 2)
                    {
                        gb.gameBucketRewards = data.Where(x => x.IsActive == false && x.IsDeleted == true).ToList();
                        gb.TotalRecords = gb.gameBucketRewards.Count();
                    }
                    var res = new
                    {
                        status = true,
                        Message = gb
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        [Route("GetBucketNoCondition")]
        [HttpGet]
        public HttpResponseMessage GetBucketNoCondition(int FromBucketNo, int EndBucketNo, long BucketId)
        {
            using (var db = new AuthContext())
            {
                List<GameBucketNoConditionDC> gb = new List<GameBucketNoConditionDC>();

                var FromBucket = new SqlParameter("@FromBucketNo", FromBucketNo);
                var EndBucket = new SqlParameter("@EndBucketNo", EndBucketNo);
                var BucketIdd = new SqlParameter("@Id", BucketId);
                var data = db.Database.SqlQuery<GameBucketNoConditionDC>("Sp_GetDataForGameBucketrRewardsByBucketNoDetail @FromBucketNo,@EndBucketNo,@Id", FromBucket, EndBucket, BucketIdd).ToList();
                if (data != null)
                {
                    gb = data.ToList();
                    var res = new
                    {
                        status = true,
                        Message = gb
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [Route("GetDistinctBucketNo")]
        [HttpGet]
        public HttpResponseMessage GetDistinctBucketNo()
        {
            using (var db = new AuthContext())
            {
                var data = db.GameBucketRewards.Where(x => x.IsActive == true && x.IsDeleted == false).OrderBy(x => x.BucketNo).ToList();
                if (data.Count > 0)
                {
                    var res = new
                    {
                        status = true,
                        Message = data
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("GetGameDashboardDetail")]
        [HttpPost]
        public APIResponse GetGameDashboardDetail(GameDashboard obj)
        {
            using (var db = new AuthContext())
            {
                APIResponse res = new APIResponse();
                GameSkcodeDC gameSkcodeDC = new GameSkcodeDC();
                MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                List<GameCustomerBucketHdrOrderDetailDC> gameSkcodeShowDClist = new List<GameCustomerBucketHdrOrderDetailDC>();
                DateTime curdate = DateTime.Now.Date;
                //if (obj.Warehouseid.Count > 0)
                //{
                //    if (obj.Skcode != null)
                //    {
                //        gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && x.Skcode.ToLower() == obj.Skcode.ToLower() && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
                //    }
                //    else
                //    {
                //        gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
                //    }
                //}
                int pageno = 0; int skip = 0; int take = 0;
                var fPageno = new SqlParameter("@pageno", pageno);
                var fskip = new SqlParameter("@skip", skip);
                var ftake = new SqlParameter("@take", take);
                var bucketConditions = db.Database.SqlQuery<GameBucketConditionData>("exec sp_bucketConditions @pageno, @skip, @take", fPageno, fskip, ftake).ToList();
                if (obj.Skcode != null)
                {
                    gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && x.Skcode.ToLower() == obj.Skcode.ToLower() && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).FirstOrDefault();
                }

                if (gameSkcodeDC != null)
                {

                    //var customerOrderBucketList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsActive == true && x.IsDeleted == false && x.BucketNo >= obj.FromBucketNo && x.BucketNo <= obj.EndBucketNo && game.CustomerId == x.CustomerId).Select(x => new GameSkcodeShowDC { CustomerId = x.CustomerId, BucketNo = x.BucketNo, GameBucketNo = x.GameBucketNo, IsCompleted = x.IsCompleted, RewardStatus = x.RewardStatus, RewardCredit = x.RewardCredit, Skcode = game.Skcode, ShopName = game.ShopName }).Distinct().ToList();
                    var customerOrderBucketListxx = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsActive == true && x.IsDeleted == false && gameSkcodeDC.CustomerId == x.CustomerId).Distinct().ToList();
                    var distinctcustomerOrderBucketList = customerOrderBucketListxx.Select(x => new { x.CustomerId, x.BucketNo, x.GameBucketNo, x.IsCompleted, x.RewardStatus, x.RewardCredit, x.BucketEndDate, x.BucketStartDate, x.GameBucketRewardId }).Distinct().ToList();
                    var temp = distinctcustomerOrderBucketList.Select(x => new GameCustomerBucketHdrOrderDetailDC { Skcode = gameSkcodeDC.Skcode, CustomerId = x.CustomerId, ShopName = gameSkcodeDC.ShopName, BucketNo = x.BucketNo, GameBucketNo = x.GameBucketNo, IsCompleted = x.IsCompleted, RewardCredit = x.RewardCredit, RewardStatus = x.RewardStatus, CurrentBucket = x.GameBucketNo - 1, LevelUpBucket = 0, BucketEndDate = x.BucketEndDate, BucketStartDate = x.BucketStartDate, GameBucketRewardId = x.GameBucketRewardId }).Distinct().ToList();
                    //List<GameCustomerBucketHdrOrderDetailDC> temp = distinctcustomerOrderBucketList.ToList(); 
                    //var customerOrderBucketList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsActive == true && x.IsDeleted == false && gameSkcodeDC.CustomerId == x.CustomerId).Select(x => new GameCustomerBucketHdrOrderDetailDC { CustomerId = x.CustomerId, BucketNo = x.BucketNo, GameBucketNo = x.GameBucketNo, IsCompleted = x.IsCompleted, RewardStatus = x.RewardStatus, RewardCredit = x.RewardCredit, Skcode = gameSkcodeDC.Skcode, ShopName = gameSkcodeDC.ShopName, OrderId = Convert.ToString(x.OrderId), CurrentBucket = x.GameBucketNo - 1, LevelUpBucket = 0, BucketStartDate = x.BucketStartDate, BucketEndDate = x.BucketEndDate }).Distinct().ToList();
                    //var customerOrderBucketList = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.IsActive == true && x.IsDeleted == false && gameSkcodeDC.CustomerId == x.CustomerId).Select(x => new GameCustomerBucketHdrOrderDetailDC { CustomerId = x.CustomerId, BucketNo = x.BucketNo, GameBucketNo = x.GameBucketNo, IsCompleted = x.IsCompleted, RewardStatus = x.RewardStatus, RewardCredit = x.RewardCredit, Skcode = gameSkcodeDC.Skcode, ShopName = gameSkcodeDC.ShopName, OrderId = Convert.ToString(x.OrderId), CurrentBucket = x.GameBucketNo - 1, LevelUpBucket = 0, BucketStartDate = x.BucketStartDate, BucketEndDate = x.BucketEndDate }).Distinct().ToList();
                    //var distinctcustomerOrderBucketList = customerOrderBucketList.Select(x => new  { CustomerId = x.CustomerId,BucketNo=x.BucketNo}).Distinct().ToList();

                    if (distinctcustomerOrderBucketList != null && distinctcustomerOrderBucketList.Any())
                    {
                        foreach (var data in temp)
                        {
                            if (data.GameBucketRewardId > 0)
                            {
                                var orders = customerOrderBucketListxx.Where(x => x.CustomerId == data.CustomerId && x.BucketNo == data.BucketNo).Select(x => x.OrderId).ToList();
                                foreach (var order in orders)
                                {
                                    if (data.OrderId == null) data.OrderId = Convert.ToString(order);
                                    else data.OrderId += "," + Convert.ToString(order);
                                }
                                if (data.RewardStatus == "InProcess")
                                {
                                    if (data.BucketEndDate < curdate)
                                    {
                                        data.RewardStatus = "NotFullFill";
                                    }
                                }

                                var custGameConditions = db.GameCurrentLevelProgresses.Where(y => y.CustomerId == data.CustomerId && y.BucketNo == data.BucketNo && y.IsActive).Select(z => new { z.ConditionId, z.SqlOutPut }).ToList();
                                if (custGameConditions.Count > 0)
                                {
                                    if (data.RewardStatus != "Completed")
                                    {
                                        data.gameBucketRewardCondition = "";
                                        var a = "";
                                        var datas = bucketConditions.Where(x => x.bucketNo == data.BucketNo && x.GameBucketRewardId == data.GameBucketRewardId).ToList();
                                        foreach (var d in datas)
                                        {
                                            //GameBucketRewardCondition gamecondition = new GameBucketRewardCondition();
                                            //gamecondition.ConditionId = d.ConditionId;
                                            //gamecondition.AppDesc = d.AppDesc == "" ? "" : d.AppDesc;
                                            //gamecondition.Value = d.Value;
                                            //gamecondition.AchiveValue = custGameConditions.FirstOrDefault(x => x.ConditionId == gamecondition.ConditionId).SqlOutPut;
                                            //data.gameBucketRewardCondition.Add(gamecondition);

                                            string Conditions1 = "";
                                            var Sqloutput = custGameConditions.Where(x => x.ConditionId == d.ConditionId).FirstOrDefault();
                                            if (Sqloutput != null)
                                            {
                                                Conditions1 = "(" + d.ConditionId + ") " + d.AppDesc == "" ? "" : d.AppDesc + " Req.: " + d.ConditionValue + " / " + Sqloutput.SqlOutPut;
                                                if (Conditions1 == null) a = Conditions1;
                                                else a += "," + Conditions1;

                                            }

                                        }
                                        data.gameBucketRewardCondition = a;


                                    }
                                }
                                gameSkcodeShowDClist.Add(data);
                            }

                        }

                        //customerOrderBucketList.ForEach(x =>
                        //{
                        //    var custGameConditions = db.GameCurrentLevelProgresses.Where(y => y.CustomerId == x.CustomerId && y.BucketNo == x.BucketNo && y.IsActive).Select(z => new { z.ConditionId, z.SqlOutPut }).ToList();
                        //    if (x.RewardStatus != "Completed")
                        //    {
                        //        x.gameBucketRewardCondition = bucketConditions.Where(y => y.bucketNo == x.BucketNo).Select(y => new GameBucketRewardCondition
                        //        {
                        //            ConditionId = y.ConditionId,
                        //            AppDesc = y.AppDesc,
                        //            Value = y.Value,
                        //            AchiveValue = custGameConditions.FirstOrDefault(a => a.ConditionId == y.ConditionId).SqlOutPut
                        //        }).ToList();

                        //    }
                        //});
                        //gameSkcodeShowDClist.AddRange(results);
                    }

                    /*
                    if (distinctcustomerOrderBucketList != null && distinctcustomerOrderBucketList.Any())
                    {

                        foreach (var data in distinctcustomerOrderBucketList)
                        {
                            GameCustomerBucketHdrOrderDetailDC results = new GameCustomerBucketHdrOrderDetailDC();

                            results.Skcode = gameSkcodeDC.Skcode;
                            results.CustomerId = data.CustomerId;
                            results.ShopName = gameSkcodeDC.ShopName;
                            results.BucketNo = data.BucketNo;
                            results.GameBucketNo = data.GameBucketNo;
                            results.IsCompleted = data.IsCompleted;
                            results.RewardStatus = data.RewardStatus;
                            results.RewardCredit = data.RewardCredit;
                            results.CurrentBucket = data.GameBucketNo - 1;
                            results.LevelUpBucket = 0;
                            results.BucketStartDate = data.BucketStartDate;
                            results.BucketEndDate = data.BucketEndDate;
                            var orders = customerOrderBucketListxx.Where(x => x.CustomerId == data.CustomerId && x.BucketNo==data.BucketNo).Select(x => x.OrderId).ToList();
                            foreach (var order in orders)
                            {
                                results.OrderId += string.Join(",", order);
                            }

                            var custGameConditions = db.GameCurrentLevelProgresses.Where(y => y.CustomerId == data.CustomerId && y.BucketNo == data.BucketNo && y.IsActive).Select(z => new { z.ConditionId, z.SqlOutPut }).ToList();
                            if (custGameConditions.Count > 0)
                            {
                                if (data.RewardStatus != "Completed")
                                {
                                    results.gameBucketRewardCondition = "";
                                    var a = "";
                                    var datas = bucketConditions.Where(x => x.bucketNo == data.BucketNo).ToList();
                                    foreach (var d in datas)
                                    {
                                        //GameBucketRewardCondition gamecondition = new GameBucketRewardCondition();
                                        //gamecondition.ConditionId = d.ConditionId;
                                        //gamecondition.AppDesc = d.AppDesc == "" ? "" : d.AppDesc;
                                        //gamecondition.Value = d.Value;
                                        //gamecondition.AchiveValue = custGameConditions.FirstOrDefault(x => x.ConditionId == gamecondition.ConditionId).SqlOutPut;
                                        //data.gameBucketRewardCondition.Add(gamecondition);

                                        string Conditions1 = "";
                                        var Sqloutput = custGameConditions.Where(x => x.ConditionId == d.ConditionId).FirstOrDefault();
                                        if (Sqloutput != null)
                                        {
                                            Conditions1 = "(" + d.ConditionId + ") " + d.AppDesc == "" ? "" : d.AppDesc + ":Required" + d.Value + " / " + Sqloutput.SqlOutPut;
                                            a += string.Join(",", Conditions1);
                                        }

                                    }
                                    results.gameBucketRewardCondition = a;

                                }
                            }
                            gameSkcodeShowDClist.Add(results);
                        }

                        //customerOrderBucketList.ForEach(x =>
                        //{
                        //    var custGameConditions = db.GameCurrentLevelProgresses.Where(y => y.CustomerId == x.CustomerId && y.BucketNo == x.BucketNo && y.IsActive).Select(z => new { z.ConditionId, z.SqlOutPut }).ToList();
                        //    if (x.RewardStatus != "Completed")
                        //    {
                        //        x.gameBucketRewardCondition = bucketConditions.Where(y => y.bucketNo == x.BucketNo).Select(y => new GameBucketRewardCondition
                        //        {
                        //            ConditionId = y.ConditionId,
                        //            AppDesc = y.AppDesc,
                        //            Value = y.Value,
                        //            AchiveValue = custGameConditions.FirstOrDefault(a => a.ConditionId == y.ConditionId).SqlOutPut
                        //        }).ToList();

                        //    }
                        //});
                        //gameSkcodeShowDClist.AddRange(results);
                    }
                    */

                    if (gameSkcodeShowDClist != null && gameSkcodeShowDClist.Any())
                    {
                        res.Status = false;
                        res.Data = gameSkcodeShowDClist;
                        return res;

                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "No Data Found";
                        return res;
                    }
                }
                else
                {
                    res.Status = false;
                    res.Message = "No Data Found";
                    return res;
                }





            }
        }


        //[Route("GetGameDashboard")]
        //[HttpPost]
        //public APIResponse GetGameDashboard(GameDashboard obj)
        //{
        //    bool resfalse = false;
        //    string resMsg = "No Data Found";
        //    int TotalRecords = 0;
        //    using (var db = new AuthContext())
        //    {
        //        APIResponse res = new APIResponse();
        //        List<GameSkcodeDC> gameSkcodeDC = new List<GameSkcodeDC>();

        //        //MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
        //        MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();

        //        List<GameDashboardHeaderDC> gameSkcodeShowDClist = new List<GameDashboardHeaderDC>();

        //        if (obj.Warehouseid.Count > 0)
        //        {
        //            if (obj.Skcode != null)
        //            {
        //                gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && x.Skcode.ToLower() == obj.Skcode.ToLower() && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
        //            }
        //            else
        //            {
        //                gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
        //            }
        //        }
        //        if (gameSkcodeDC != null && gameSkcodeDC.Any())
        //        {
        //            foreach (var game in gameSkcodeDC)
        //            {
        //                if (obj.FromBucketNo > 0 && obj.EndBucketNo > 0)
        //                {
        //                    var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false && x.CustomerId == game.CustomerId && x.BucketNo >= obj.FromBucketNo && x.BucketNo <= obj.EndBucketNo).Select(x => new GameDashboardHeaderDC { SkCode = game.Skcode, CustomerId = x.CustomerId, ShopName = game.ShopName, CrmBucketNo = x.CRMBucketNo, BucketNo = x.BucketNo, NextBucketNo = x.NextBucketNo, GameBucketNo = x.GameBucketNo, LastOrderDate = x.LastOrderDate, BucketStartDate = x.BucketStartDate, BucketEndDate = x.BucketEndDate }).ToList();
        //                    if (dataCustomerBucketHdrList != null && dataCustomerBucketHdrList.Any()) gameSkcodeShowDClist.AddRange(dataCustomerBucketHdrList);
        //                }
        //                else
        //                {
        //                    var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false && x.CustomerId == game.CustomerId).Select(x => new GameDashboardHeaderDC { SkCode = game.Skcode, CustomerId = x.CustomerId, ShopName = game.ShopName, CrmBucketNo = x.CRMBucketNo, BucketNo = x.BucketNo, NextBucketNo = x.NextBucketNo, GameBucketNo = x.GameBucketNo, LastOrderDate = x.LastOrderDate, BucketStartDate = x.BucketStartDate, BucketEndDate = x.BucketEndDate }).ToList();
        //                    if (dataCustomerBucketHdrList != null && dataCustomerBucketHdrList.Any()) gameSkcodeShowDClist.AddRange(dataCustomerBucketHdrList);
        //                }
        //                //var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.IsActive == true && x.IsDeleted == false && x.CustomerId==game.CustomerId).Select(x => new GameDashboardHeaderDC {SkCode = game.Skcode,CustomerId = x.CustomerId, ShopName = game.ShopName,CrmBucketNo=x.CRMBucketNo, BucketNo = x.BucketNo, NextBucketNo = x.NextBucketNo,GameBucketNo = x.GameBucketNo, LastOrderDate = x.LastOrderDate,BucketStartDate = x.BucketStartDate,BucketEndDate = x.BucketEndDate}).ToList();
        //                //if (dataCustomerBucketHdrList != null && dataCustomerBucketHdrList.Any()) gameSkcodeShowDClist.AddRange(dataCustomerBucketHdrList);
        //            }

        //            if (gameSkcodeShowDClist != null && gameSkcodeShowDClist.Any())
        //            {
        //                resfalse = true;
        //                resMsg = "Sucess";
        //                TotalRecords = gameSkcodeShowDClist.Count();
        //                res.Data = gameSkcodeShowDClist.Skip(obj.Skip).Take(obj.Take).ToList();
        //            }

        //            res.Status = resfalse;
        //            res.Message = resMsg;
        //            return res;

        //        }
        //        else
        //        {
        //            res.Status = false;
        //            res.Message = "No Data Found";
        //            return res;
        //        }





        //    }
        //}



        [Route("GetGameDashboard")]
        [HttpPost]
        public HttpResponseMessage GetGameDashboard(GameDashboard obj)
        {
            //bool resfalse = false;
            //string resMsg = "No Data Found";

            using (var db = new AuthContext())
            {
                obj.Skip = (obj.Skip - 1) * obj.Take;
                //APIResponse res = new APIResponse();
                GameDashboardHeader result = new GameDashboardHeader();
                List<GameSkcodeDC> gameSkcodeDC = new List<GameSkcodeDC>();

                //MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();
                MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                List<GameCustomerBucketHdr> list = new List<GameCustomerBucketHdr>();

                List<GameDashboardHeaderDC> gameSkcodeShowDClist = new List<GameDashboardHeaderDC>();

                if (obj.Warehouseid.Count > 0)
                {
                    if (obj.Skcode != null)
                    {
                        gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && x.Skcode.ToLower() == obj.Skcode.ToLower() && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
                    }
                    else
                    {
                        gameSkcodeDC = db.Customers.Where(x => x.Active == true && x.Deleted == false && obj.CityId == x.Cityid && obj.Warehouseid.Contains(x.Warehouseid)).Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
                    }
                }
                //--------------------------------
                var objCustomerList = gameSkcodeDC.Select(x => x.CustomerId).Distinct().ToList();

                if (obj.FromBucketNo > 0 && obj.EndBucketNo > 0)
                {
                    var data = mongoDbHelperCustomerBucketHdr.Select(x => objCustomerList.Contains(x.CustomerId) && x.BucketNo >= obj.FromBucketNo && x.BucketNo <= obj.EndBucketNo).ToList();
                    list = data;
                }

                else
                {
                    var data = mongoDbHelperCustomerBucketHdr.Select(x => objCustomerList.Contains(x.CustomerId)).ToList();
                    list = data;
                }
                //    var data = mongoDbHelperCustomerBucketHdr.Select(x => objCustomerList.Contains(x.CustomerId)).ToList();
                //list = data;

                var tmp = list.Select(z => new GameDashboardHeaderDC { SkCode = z.SkCode, CustomerId = z.CustomerId, ShopName = gameSkcodeDC.Where(s => s.CustomerId == z.CustomerId).Select(s => s.ShopName).Distinct().FirstOrDefault(), CrmBucketNo = z.CRMBucketNo - 1, BucketNo = z.BucketNo, NextBucketNo = z.NextBucketNo, GameBucketNo = z.GameBucketNo, LastOrderDate = z.LastOrderDate, BucketStartDate = z.BucketStartDate, BucketEndDate = z.BucketEndDate, CurrentBucket = z.NextBucketNo == 0 ? z.GameBucketNo : z.GameBucketNo - 1, LevelUpBucket = z.NextBucketNo != 0 ? z.GameBucketNo : 0 }).ToList();
                if (tmp != null && tmp.Any())
                {
                    result.TotalRecords = tmp.Count();
                    result.DashboardHeader = tmp.Skip(obj.Skip).Take(obj.Take).ToList();
                    var res = new
                    {
                        Status = true,
                        Message = result
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        Status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        public async Task<bool> CreateStreakCustomerTransactionPeriod(StreakCustomerDc obj)
        {
            bool resFlag = false;
            MongoHelper<GameStreakCustomerTransaction> mongohelper = new MongoHelper<GameStreakCustomerTransaction>();
            var customerStreakcount = mongohelper.Count(x => x.CustomerId == obj.CustomerId && !x.IsExpired && x.IsActive == true && x.IsDeleted == false);
            var BucketStartDate = obj.StartDate;
            var BucketEndDate = obj.EndDate;
            DateTime curdate = DateTime.Now;
            if (customerStreakcount == 0)
            {
                using (var db = new AuthContext())
                {
                    //int StreakDuration = 0;
                    //StreakDuration = db.GameConditionMasters.Where(x => x.Name == "StreakDuration" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();


                    var validatStreakLevelConfig = db.GameStreakLevelConfigMasters.Where(x => obj.BucketNo >= x.BucketNoFrom && obj.BucketNo <= x.BucketNoTo && x.IsActiveCurrent && x.IsActive && x.IsDeleted == false).ToList();
                    if (validatStreakLevelConfig.Count > 0)
                    {
                        List<long> id = new List<long>();
                        validatStreakLevelConfig.ForEach(x =>
                        {
                            id.Add(x.Id);
                        });
                        List<GameStreakCustomerTransaction> lst = new List<GameStreakCustomerTransaction>();
                        for (int i = 1; i <= StreakDuration; i++)
                        {
                            GameStreakCustomerTransaction data = new GameStreakCustomerTransaction();
                            data.CustomerId = obj.CustomerId;
                            data.BucketStartDate = BucketStartDate;
                            data.BucketEndDate = BucketEndDate;
                            data.StreakId = i;
                            data.StreakIsFulfill = false;
                            data.IsExpired = false;
                            data.CreatedBy = 1;
                            data.CreatedDate = curdate;
                            data.IsActive = true;
                            data.IsDeleted = false;
                            data.BucketNo = obj.BucketNo;
                            data.GameStreakLevelConfigMasterId = id;
                            lst.Add(data);
                            BucketStartDate = Convert.ToDateTime(BucketEndDate).AddDays(1);
                            BucketEndDate = Convert.ToDateTime(BucketStartDate).AddDays(IncreaseDay - 1);
                        }
                        resFlag = await mongohelper.InsertManyAsync(lst);
                    }
                }
            }

            return resFlag;
        }


        public async Task<bool> InsertStreakOrder(int CustomerId, int OrderId, DateTime OrderDate)
        {
            bool flag = false;
            using (var context = new AuthContext())
            {

                MongoHelper<GameStreakCustomerTransaction> mongohelper = new MongoHelper<GameStreakCustomerTransaction>();
                //var streakTransaction = (await mongohelper.SelectAsync(x => x.CustomerId == CustomerId
                //&& !x.IsExpired && x.IsActive && x.IsDeleted == false
                //&& EntityFunctions.TruncateTime(x.BucketStartDate) <= OrderDate.Date
                //&& EntityFunctions.TruncateTime(x.BucketEndDate) >= OrderDate.Date)).FirstOrDefault();
                var dataStreakCustomerTransactionList = (mongohelper.Select(x => x.CustomerId == CustomerId
                && !x.IsExpired && x.IsActive && x.IsDeleted == false).OrderBy(x => x.StreakId).ToList());
                if (dataStreakCustomerTransactionList.Count > 0)
                {
                    var streakTransaction = dataStreakCustomerTransactionList.Where(x => x.BucketStartDate.Date <= OrderDate.Date && x.BucketEndDate.Date >= OrderDate.Date).FirstOrDefault();

                    if (streakTransaction != null)
                    {
                        if (streakTransaction.OrderId != null)
                        {
                            streakTransaction.OrderId.Add(OrderId);
                        }
                        else
                        {
                            streakTransaction.OrderId = new List<int> { OrderId };
                        }
                        streakTransaction.StreakIsFulfill = true;
                        streakTransaction.ModifiedBy = CustomerId;
                        streakTransaction.ModifiedDate = DateTime.Now;
                        flag = mongohelper.ReplaceWithoutFind(streakTransaction.Id, streakTransaction);
                    }
                }
            }
            return flag;
        }

        [Route("EditBucketCondition")]
        [HttpPost]
        public HttpResponseMessage EditBucketCondition(long Id, int value)
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var res = new
                {
                    status = true,
                    Message = ""
                };

                BucketRewardCondition br = new BucketRewardCondition();
                if (Id > 0)
                {
                    var data = db.BucketRewardConditions.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.value = value;
                        //br.GameConditionMasterId = GameConditionMasterId;
                        //br.GameBucketRewardId = GameBucketRewardId;
                        //br.CreatedDate = DateTime.Now;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        data.IsActive = true;
                        data.IsDeleted = false;
                        //br.CreatedBy = userid;
                        //db.BucketRewardConditions.Add(br);
                        db.Entry(data).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            res = new
                            {
                                status = true,
                                Message = "Edit Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new
                            {
                                status = false,
                                Message = "Something Went Wrong"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        res = new
                        {
                            status = false,
                            Message = "Data Not Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new
                    {
                        status = false,
                        Message = "Something Went Wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [HttpGet]
        [Route("DeleteBucketwithConditions")]
        public HttpResponseMessage DeleteBucketwithConditions(long Id)
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var res = new
                {
                    status = true,
                    Message = ""
                };
                var data = db.GameBucketRewards.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (data != null)
                {
                    var conditionlist = db.BucketRewardConditions.Where(x => x.GameBucketRewardId == data.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (conditionlist.Count > 0)
                    {
                        data.IsActive = false;
                        data.IsDeleted = true;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        db.Entry(data).State = EntityState.Modified;
                        foreach (var d in conditionlist)
                        {
                            d.IsActive = false;
                            d.IsDeleted = true;
                            d.ModifiedBy = userid;
                            d.ModifiedDate = DateTime.Now;
                            db.Entry(d).State = EntityState.Modified;
                        }
                        if (db.Commit() > 0)
                        {
                            res = new
                            {
                                status = true,
                                Message = "Deleted Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new
                            {
                                status = false,
                                Message = "No Data Found"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        data.IsActive = false;
                        data.IsDeleted = true;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        db.Entry(data).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            res = new
                            {
                                status = true,
                                Message = "Deleted Successfully"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new
                            {
                                status = false,
                                Message = "No Data Found"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                }
                else
                {
                    res = new
                    {
                        status = false,
                        Message = "No Data Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                //GameCondition game = new GameCondition();
                //var data = db.BucketRewardConditions.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                //if (data != null)
                //{
                //    data.IsActive = false;
                //    data.IsDeleted = true;
                //    data.ModifiedDate = DateTime.Now;
                //    data.ModifiedBy = userid;
                //    db.Entry(data).State = EntityState.Modified;
                //    if (db.Commit() > 0)
                //    {
                //        res = new
                //        {
                //            status = true,
                //            Message = "Delete Successfully"
                //        };
                //        return Request.CreateResponse(HttpStatusCode.OK, res);
                //    }
                //    else
                //    {
                //        res = new
                //        {
                //            status = false,
                //            Message = "Something Went Wrong"
                //        };
                //        return Request.CreateResponse(HttpStatusCode.OK, res);
                //    }
                //}
                //else
                //{
                //    res = new
                //    {
                //        status = false,
                //        Message = "No Data Found"
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
            }
        }


        [HttpGet]
        [Route("GetGameStreakLevelConfigMaster")]
        public async Task<List<GameStreakLevelConfigMasterDc>> GetGameStreakLevelConfigMaster(int BucketNoFrom, int BucketNoTo, int skip, int take, int filtertype)
        {
            List<GameStreakLevelConfigMasterDc> StreakLevelConfigMaster = new List<GameStreakLevelConfigMasterDc>();

            using (var db = new AuthContext())
            {
                //filtertype 0-both 1-active 2-inactive
                var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNoFrom);
                var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                var fskip = new SqlParameter("@skip", skip);
                var ftake = new SqlParameter("@take", take);
                StreakLevelConfigMaster = db.Database.SqlQuery<GameStreakLevelConfigMasterDc>("exec sp_GameStreakLevelConfigMaster @BucketNoFrom,@BucketNoTo, @skip, @take", fBucketNoFrom, fBucketNoTo, fskip, ftake).ToList();
                if (filtertype == 1)
                {
                    StreakLevelConfigMaster = StreakLevelConfigMaster.Where(x => x.IsActiveCurrent == true).ToList();
                }
                if (filtertype == 2)
                {
                    StreakLevelConfigMaster = StreakLevelConfigMaster.Where(x => x.IsActiveCurrent == false).ToList();
                }
            }

            return StreakLevelConfigMaster;
        }



        [HttpGet]
        [Route("GetGameStreakLevelConfigDetail")]
        public async Task<List<GameStreakLevelConfigDetailDc>> GetGameStreakLevelConfigDetail(int BucketNoFrom, int BucketNoTo, int skip, int take, DateTime CreatedDate)
        {
            List<GameStreakLevelConfigDetailDc> StreakLevelConfigDetail = new List<GameStreakLevelConfigDetailDc>();

            using (var db = new AuthContext())
            {
                int IsActiveCurrent1 = 0;
                var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNoFrom);
                var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                var fskip = new SqlParameter("@skip", skip);
                var ftake = new SqlParameter("@take", take);
                var fCreateddate = new SqlParameter("@CreatedDate", CreatedDate);
                var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);

                StreakLevelConfigDetail = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, fCreateddate, fIsActiveCurrent).ToList();
            }

            return StreakLevelConfigDetail;
        }

        [HttpPost]
        [Route("InsertGameStreakLevelConfig")]
        public HttpResponseMessage InsertGameStreakLevelConfig(List<GameStreakLevelConfigDetailDc> obj)
        {
            bool flag = false;
            string Msg = "";
            DateTime currentdate = DateTime.Now;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            var res = new
            {
                status = true,
                Message = ""
            };


            string chkerror = ValidateConfig(obj);
            if (chkerror != string.Empty)
            {
                flag = false;
                Msg = chkerror;
            }
            else
            {
                bool IndividualLevel = false;

                using (var db = new AuthContext())
                {

                    long GameStreakLevelConfigMasterId = 0;
                    List<GameStreakLevelConfigDetail> StreakLevelConfigDetailList = new List<GameStreakLevelConfigDetail>();
                    foreach (var item in obj.OrderBy(x => x.StreakConditionType))
                    {
                        if (item.StreakConditionType != 1) { IndividualLevel = false; }

                        if (IndividualLevel == false)
                        {
                            IndividualLevel = true;

                            GameStreakLevelConfigMaster StreakLevelConfigM = new GameStreakLevelConfigMaster();
                            StreakLevelConfigM.BucketNoFrom = item.BucketNoFrom;
                            StreakLevelConfigM.BucketNoTo = item.BucketNoTo;
                            StreakLevelConfigM.StreakConditionType = item.StreakConditionType; // 1-Level-Individual, 2-Streak, 3-Outof 
                            StreakLevelConfigM.RewardType = item.RewardType;//wallet-0/RI-1/offer-2 
                            StreakLevelConfigM.RewardValue = item.StreakConditionType == 1 ? 0 : item.RewardValue;
                            StreakLevelConfigM.Streak_StreakCount = item.StreakConditionType == 2 ? item.Streak_StreakCount : 0;
                            StreakLevelConfigM.Streak_ChooseReward = item.StreakConditionType == 2 ? item.Streak_ChooseReward : 0; //1-Multiplier / 2-Fixed
                            StreakLevelConfigM.OutOf_OutOfBucket = item.StreakConditionType == 3 ? item.OutOf_OutOfBucket : 0;
                            StreakLevelConfigM.OutOf_TotalBucket = item.StreakConditionType == 3 ? item.OutOf_TotalBucket : 0;
                            StreakLevelConfigM.IsActiveCurrent = true;
                            StreakLevelConfigM.IsActive = true;
                            StreakLevelConfigM.IsDeleted = false;
                            StreakLevelConfigM.CreatedBy = userid;
                            StreakLevelConfigM.CreatedDate = currentdate;

                            db.GameStreakLevelConfigMasters.Add(StreakLevelConfigM);
                            db.Commit();
                            GameStreakLevelConfigMasterId = StreakLevelConfigM.Id;
                        }
                        if (item.StreakConditionType == 1)
                        {
                            var gameStreakLevelConfigMasterId = new SqlParameter("@GameStreakLevelConfigMasterId", GameStreakLevelConfigMasterId);
                            var levelNo = new SqlParameter("@LevelNo", item.LevelNo);
                            var levelValue = new SqlParameter("@LevelValue", item.LevelValue);
                            var createdBy = new SqlParameter("@CreatedBy", userid);
                            var CreatedDate = new SqlParameter("@CreatedDate", currentdate);
                            var result = db.Database.SqlQuery<bool>("exec Sp_insertGameStreakLevelConfigDetails @GameStreakLevelConfigMasterId,@LevelNo,@LevelValue,@CreatedBy,@CreatedDate", gameStreakLevelConfigMasterId, levelNo, levelValue, createdBy, CreatedDate).FirstOrDefault();

                        }
                    }

#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.GameConfigCacheKey());                    
#endif

                    flag = true;
                    Msg = "Add Successfully";

                }
            }


            res = new
            {
                status = flag,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }


        public string ValidateConfig(List<GameStreakLevelConfigDetailDc> obj)
        {
            string Msg = "";
            using (var db = new AuthContext())
            {
                //StreakConditionType // 1-Level-Individual, 2-Streak, 3-Outof 
                //Streak_ChooseReward =  //1-Multiplier / 2-Fixed
                var chkStreakMultiplier = obj.Count(x => x.StreakConditionType == 2 && x.Streak_ChooseReward == 1);
                if (chkStreakMultiplier > 0)
                {
                    var resCount = obj.Count(x => x.StreakConditionType == 1);
                    if (resCount == 0 || resCount == null)
                    { Msg = "Atleast one row maintain for Level-Individual"; return Msg; }
                }

                //// Type -- Condition not same : 
                var chkConditionTypeRewardValue = obj.Select(y => new { y.StreakConditionType, y.RewardValue }).GroupBy(x => new { x.StreakConditionType, x.RewardValue }).Count();


                ////  Bucket From/To : .
                int BucketFrom = obj.Select(x => x.BucketNoFrom).FirstOrDefault();
                int BucketTo = obj.Select(x => x.BucketNoTo).FirstOrDefault();
                if (BucketFrom < 0)
                {
                    Msg = "Please Enter Bucket From No.";
                    return Msg;
                }
                if (BucketTo < 0)
                {
                    Msg = "Please Enter Bucket To No.";
                    return Msg;
                }
                if (BucketFrom > BucketTo)
                {
                    Msg = "Please Enter Correct Bucket No";
                    return Msg;
                }
                if (BucketTo >= BucketFrom)
                {
                    List<GameStreakLevelConfigMasterDc> StreakLevelConfigMaster = new List<GameStreakLevelConfigMasterDc>();
                    int Skip = 0;
                    int Take = 10;
                    var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketFrom);
                    var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketTo);
                    var fskip = new SqlParameter("@skip", Skip);
                    var ftake = new SqlParameter("@take", Take);
                    StreakLevelConfigMaster = db.Database.SqlQuery<GameStreakLevelConfigMasterDc>("exec sp_GameStreakLevelConfigMaster @BucketNoFrom,@BucketNoTo, @skip, @take", fBucketNoFrom, fBucketNoTo, fskip, ftake).ToList();
                    //var chkBucketNo = context.GameStreakLevelConfigMasters.Where(x => BucketFrom >= x.BucketNoFrom && BucketFrom <= x.BucketNoTo).Count();
                    //if (chkBucketNo > 0)
                    if (StreakLevelConfigMaster.Where(x => x.IsActiveCurrent == true).Count() > 0)
                    {
                        Msg = "Bucket No Already Exists in Range";
                        return Msg;
                    }
                }
                //foreach (var item in obj.Select(y => new { y.BucketNoFrom, y.BucketNoTo }).Distinct())
                //{
                //    var chkBucketNo = db.GameStreakLevelConfigMasters.Where(x => item.BucketNoFrom >= x.BucketNoFrom && item.BucketNoFrom <= x.BucketNoTo).Count();
                //    if (chkBucketNo > 0)
                //    { Msg = "Bucket Level exist in Range"; return Msg; }
                //}

                ////  Bucket From/To : .
                if (obj.Where(x => x.StreakConditionType == 2).FirstOrDefault() != null)
                {
                    var chk = obj.Where(x => x.StreakConditionType == 2 && x.Streak_ChooseReward == 1).FirstOrDefault();
                    //&& x.LevelNo <= chk.Streak_StreakCount
                    var resCountxxx = obj.Where(x => x.StreakConditionType == 1).Count();
                    if (resCountxxx == 0 || resCountxxx == null)
                    { Msg = "Please Create an Individual Level first"; return Msg; }
                }


            }

            return Msg;
        }


        [HttpGet]
        [Route("InActiveActiveGameStreakMaster")]
        public HttpResponseMessage InActiveActiveGameStreakMaster(int BucketNoFrom, int BucketNoTo, bool ConfigIsActive, DateTime CreatedDate)
        {
            bool flag = false;
            string Msg = "";
            string result = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var context = new AuthContext())
            {
                if (ConfigIsActive == true)
                {
                    result = "Active";
                    List<GameStreakLevelConfigMasterDc> StreakLevelConfigMaster = new List<GameStreakLevelConfigMasterDc>();
                    int Skip = 0;
                    int Take = 10;
                    var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNoFrom);
                    var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                    var fskip = new SqlParameter("@skip", Skip);
                    var ftake = new SqlParameter("@take", Take);
                    StreakLevelConfigMaster = context.Database.SqlQuery<GameStreakLevelConfigMasterDc>("exec sp_GameStreakLevelConfigMaster @BucketNoFrom,@BucketNoTo, @skip, @take", fBucketNoFrom, fBucketNoTo, fskip, ftake).ToList();
                    //var chkBucketNo = context.GameStreakLevelConfigMasters.Where(x => BucketFrom >= x.BucketNoFrom && BucketFrom <= x.BucketNoTo).Count();
                    //if (chkBucketNo > 0)
                    if (StreakLevelConfigMaster.Where(x => x.IsActiveCurrent == true).Count() > 0)
                    {
                        flag = false;
                        Msg = "Bucket No Already Exists in Range";

                    }
                    else
                    {
                        var configmaster = context.GameStreakLevelConfigMasters.Where(x => x.BucketNoFrom == BucketNoFrom && x.BucketNoTo == BucketNoTo && x.IsActive == true && x.IsDeleted == false && x.CreatedDate == CreatedDate).ToList();
                        if (configmaster.Any() && configmaster.Count > 0)
                        {
                            foreach (var config in configmaster)
                            {
                                config.IsActiveCurrent = ConfigIsActive;
                                config.ModifiedDate = DateTime.Now;
                                config.ModifiedBy = userid;
                                context.Entry(config).State = EntityState.Modified;
                                var configdetail = context.GameStreakLevelConfigDetails.Where(x => x.GameStreakLevelConfigMasterId == config.Id).ToList();
                                if (configdetail.Any() && configdetail.Count > 0)
                                {
                                    foreach (var detail in configdetail)
                                    {
                                        detail.IsActiveCurrent = ConfigIsActive;
                                        detail.ModifiedDate = DateTime.Now;
                                        detail.ModifiedBy = userid;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }
                                }
                            }
                            if (context.Commit() > 0)
                            {


#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.GameConfigCacheKey());                    
#endif
                                flag = true;
                                Msg = result + "Successfully";
                            }
                            else
                            {
                                flag = false;
                                Msg = "Something Went Wrong";
                            }
                        }
                        else
                        {
                            flag = false;
                            Msg = "Data Not Found";
                        }
                    }
                }
                else
                {
                    result = "InActive";
                    var configmasters = context.GameStreakLevelConfigMasters.Where(x => x.BucketNoFrom == BucketNoFrom && x.BucketNoTo == BucketNoTo && x.IsActive == true && x.IsDeleted == false && x.CreatedDate == CreatedDate).ToList();
                    if (configmasters.Any() && configmasters.Count > 0)
                    {
                        foreach (var config in configmasters)
                        {
                            config.IsActiveCurrent = ConfigIsActive;
                            config.ModifiedDate = DateTime.Now;
                            config.ModifiedBy = userid;
                            context.Entry(config).State = EntityState.Modified;
                            var configdetail = context.GameStreakLevelConfigDetails.Where(x => x.GameStreakLevelConfigMasterId == config.Id).ToList();
                            if (configdetail.Any() && configdetail.Count > 0)
                            {
                                foreach (var detail in configdetail)
                                {
                                    detail.IsActiveCurrent = ConfigIsActive;
                                    detail.ModifiedDate = DateTime.Now;
                                    detail.ModifiedBy = userid;
                                    context.Entry(detail).State = EntityState.Modified;
                                }
                            }
                        }
                        if (context.Commit() > 0)
                        {

#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.GameConfigCacheKey());                    
#endif
                            flag = true;
                            Msg = result + "Successfully";
                        }
                        else
                        {
                            flag = false;
                            Msg = "Something Went Wrong";
                        }
                    }
                    else
                    {
                        flag = false;
                        Msg = "Data Not Found";
                    }

                }

                var res = new
                {
                    status = flag,
                    Message = Msg
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        [HttpGet]
        [Route("InActiveActiveGameBucketRewards")]
        public HttpResponseMessage InActiveActiveGameBucketRewards(int Id, bool IsActive)
        {
            bool flag = false;
            string Msg = "";
            string result = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                if (IsActive == true)
                {
                    result = "Active";
                }
                else
                {
                    result = "InActive";
                }
                var configmaster = context.GameBucketRewards.Where(x => x.Id == Id).ToList();
                if (configmaster.Any() && configmaster.Count > 0)
                {
                    foreach (var config in configmaster)
                    {
                        config.IsActive = IsActive;
                        if (config.IsActive == true)
                        {
                            config.IsDeleted = false;
                        }
                        else
                        {
                            config.IsDeleted = true;
                        }
                        config.ModifiedDate = DateTime.Now;
                        config.ModifiedBy = userid;
                        context.Entry(config).State = EntityState.Modified;

                        // var configdetail = context.GameStreakLevelConfigDetails.Where(x => x.GameStreakLevelConfigMasterId == config.Id).ToList();

                    }
                    if (context.Commit() > 0)
                    {
                        flag = true;
                        Msg = result + "Successfully";
                    }
                    else
                    {
                        flag = false;
                        Msg = "Something Went Wrong";
                    }
                }
                else
                {
                    flag = false;
                    Msg = "Data Not Found";
                }
                var res = new
                {
                    status = flag,
                    Message = Msg
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [HttpGet]
        [Route("CheckActiveInactiveBucket")]
        public HttpResponseMessage CheckActiveInactiveBucket(int BucketNo, bool isActive, int Id)
        {
            bool flag = false;
            string Msg = "";
            string result = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (isActive == true)
            {
                result = "Active";
            }
            else
            {
                result = "InActive";
            }
            using (var context = new AuthContext())
            {
                var Data = context.GameBucketRewards.Where(x => x.BucketNo == BucketNo).ToList();
                if (Data != null)
                {
                    if (isActive == true)
                    {
                        foreach (var item in Data)
                        {
                            if (item.IsActive == true)
                            {
                                flag = true;
                            }
                        }
                        if (flag == true)
                        {
                            var res = new
                            {
                                status = false,
                                Message = "Already Active Bucket Exists"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        //else
                        //{
                        //    var BucketData = Data.Where(x => x.Id == Id).FirstOrDefault();
                        //    BucketData.IsActive = isActive;
                        //    if (BucketData.IsActive == true)
                        //    {
                        //        BucketData.IsDeleted = false;
                        //    }
                        //    else
                        //    {
                        //        BucketData.IsDeleted = true;
                        //    }
                        //}
                    }
                    if (flag == false || isActive == false)
                    {
                        var BucketData = Data.Where(x => x.Id == Id).FirstOrDefault();
                        BucketData.IsActive = isActive;
                        if (BucketData.IsActive == true)
                        {
                            BucketData.IsDeleted = false;
                        }
                        else
                        {
                            BucketData.IsDeleted = true;
                        }
                    }
                }
                if (context.Commit() > 0)
                {
                    flag = true;
                    Msg = result + "Successfully";
                }
                else
                {
                    flag = false;
                    Msg = "Something Went Wrong";
                }
                var reesult = new
                {
                    status = flag,
                    Message = Msg
                };
                return Request.CreateResponse(HttpStatusCode.OK, reesult);
            }
        }





        [HttpGet]
        [Route("DeleteGameStreakMaster")]
        public HttpResponseMessage DeleteGameStreakMaster(int BucketNoFrom, int BucketNoTo)
        {
            bool flag = false;
            string Msg = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var context = new AuthContext())
            {

                var configmaster = context.GameStreakLevelConfigMasters.Where(x => x.BucketNoFrom == BucketNoFrom && x.BucketNoTo == BucketNoTo && x.IsActive == true && x.IsDeleted == false).ToList();
                if (configmaster.Any() && configmaster.Count > 0)
                {
                    foreach (var config in configmaster)
                    {
                        config.IsActive = false;
                        config.IsDeleted = true;
                        config.ModifiedDate = DateTime.Now;
                        config.ModifiedBy = userid;
                        context.Entry(config).State = EntityState.Modified;
                        var configdetail = context.GameStreakLevelConfigDetails.Where(x => x.GameStreakLevelConfigMasterId == config.Id).ToList();
                        if (configdetail.Any() && configdetail.Count > 0)
                        {
                            foreach (var detail in configdetail)
                            {
                                detail.IsActive = false;
                                detail.IsDeleted = true;
                                detail.ModifiedDate = DateTime.Now;
                                detail.ModifiedBy = userid;
                                context.Entry(detail).State = EntityState.Modified;
                            }
                        }
                    }
                    if (context.Commit() > 0)
                    {

#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.GameConfigCacheKey());                    
#endif
                        flag = true;
                        Msg = "Deleted Successfully";
                    }
                    else
                    {
                        flag = false;
                        Msg = "Something Went Wrong";
                    }
                }
                else
                {
                    flag = false;
                    Msg = "Data Not Found";
                }
                var res = new
                {
                    status = flag,
                    Message = Msg
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        //[HttpPost]
        //[Route("InsertGameStreakLevelRewardValue")]
        private void InsertGameStreakLevelRewardValue(List<GameStreakLevelRewardValueDC> dataStreakLevelRewardValue)
        {
            //List<GameStreakLevelRewardValueDC> InsertStreakLevelRewardList = new List<GameStreakLevelRewardValueDC>();
            string msg = "";
            MongoDbHelper<GameStreakLevelRewardValue> gamesterak = new MongoDbHelper<GameStreakLevelRewardValue>();

            foreach (var obj in dataStreakLevelRewardValue)
            {
                var CustomerStreakLevelRewardValueList = gamesterak.Select(x => x.CustomerId == obj.CustomerId && x.BucketNo == obj.BucketNo
                                    && x.StreakIdFrom == obj.StreakIdFrom && x.StreakIdTo == obj.StreakIdTo
                                    && x.GameStreakLevelConfigMasterId == obj.GameStreakLevelConfigMasterId && x.GameStreakLevelConfigDetailId == obj.GameStreakLevelConfigDetailId
                                    && x.IsActive == true && x.IsDeleted == false).ToList();
                //&& x.GameBucketNo == obj.GameBucketNo 

                //if (CustomerStreakLevelRewardValueList == null && CustomerStreakLevelRewardValueList.Count() == 0)
                if (CustomerStreakLevelRewardValueList.Count() == 0)
                {
                    GameStreakLevelRewardValue gameStreakLevelRewardValue = new GameStreakLevelRewardValue();
                    gameStreakLevelRewardValue.CustomerId = obj.CustomerId;
                    gameStreakLevelRewardValue.BucketNo = obj.BucketNo;
                    gameStreakLevelRewardValue.StreakIdFrom = obj.StreakIdFrom;
                    gameStreakLevelRewardValue.StreakIdTo = obj.StreakIdTo;
                    gameStreakLevelRewardValue.GameStreakLevelConfigMasterId = obj.GameStreakLevelConfigMasterId;
                    gameStreakLevelRewardValue.GameStreakLevelConfigDetailId = obj.GameStreakLevelConfigDetailId;
                    gameStreakLevelRewardValue.ReferGameStreakLevelConfigMasterId = obj.ReferGameStreakLevelConfigMasterId;
                    gameStreakLevelRewardValue.RewardStatus = obj.RewardStatus;
                    gameStreakLevelRewardValue.IsCompleted = obj.IsCompleted;
                    gameStreakLevelRewardValue.RewardValue = obj.RewardValueCr;
                    //gameStreakLevelRewardValue.RewardValueCr = obj.RewardValueCr;
                    //gameStreakLevelRewardValue.RewardValueDr = obj.RewardValueDr;
                    //gameStreakLevelRewardValue.RemaningRewardAmount = obj.RemaningRewardAmount; 
                    //gameStreakLevelRewardValue.IsCancelRewardSettled = obj.IsCancelRewardSettled;  ///false: done, true: pending for settle of cancel order.
                    gameStreakLevelRewardValue.BucketStartDate = obj.BucketStartDate;
                    gameStreakLevelRewardValue.BucketEndDate = obj.BucketEndDate;
                    gameStreakLevelRewardValue.IsActive = true;
                    gameStreakLevelRewardValue.IsDeleted = false;
                    gameStreakLevelRewardValue.CreatedDate = DateTime.Now;
                    gameStreakLevelRewardValue.CreatedBy = 1;

                    var flag = gamesterak.InsertAsync(gameStreakLevelRewardValue);
                    if (flag != null)
                    {
                        msg = "Add successfully";
                    }
                }
            }

            //return msg;
        }

        [HttpGet]
        [Route("CheckBucketNo")]
        public string CheckBucketNo(int BucketFrom, int BucketTo)
        {
            string result = "";
            using (var context = new AuthContext())
            {
                if (BucketFrom < 0)
                {
                    result = "Please Enter Bucket From No.";
                    return result;
                }
                if (BucketTo < 0)
                {
                    result = "Please Enter Bucket To No.";
                    return result;
                }
                if (BucketFrom > BucketTo)
                {
                    result = "Please Enter Correct Bucket No";
                    return result;
                }
                if (BucketTo >= BucketFrom)
                {
                    List<GameStreakLevelConfigMasterDc> StreakLevelConfigMaster = new List<GameStreakLevelConfigMasterDc>();
                    int Skip = 0;
                    int Take = 10;
                    var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketFrom);
                    var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketTo);
                    var fskip = new SqlParameter("@skip", Skip);
                    var ftake = new SqlParameter("@take", Take);
                    StreakLevelConfigMaster = context.Database.SqlQuery<GameStreakLevelConfigMasterDc>("exec sp_GameStreakLevelConfigMaster @BucketNoFrom,@BucketNoTo, @skip, @take", fBucketNoFrom, fBucketNoTo, fskip, ftake).ToList();
                    //var chkBucketNo = context.GameStreakLevelConfigMasters.Where(x => BucketFrom >= x.BucketNoFrom && BucketFrom <= x.BucketNoTo).Count();
                    //if (chkBucketNo > 0)
                    if (StreakLevelConfigMaster.Where(x => x.IsActiveCurrent == true).Count() > 0)
                    {
                        result = "Bucket No Already Exists in Range";
                        return result;

                    }
                    else
                    {
                        result = "true";
                        return result;
                    }
                }
                else
                {
                    result = "Please Enter Correct Bucket No";
                    return result;
                }
                return result;
            }
        }


        [HttpPost]
        [Route("EditNew")]
        public HttpResponseMessage EditNew(PostGameBucketRewards post)
        {
            using (var context = new AuthContext())
            {
                var Data = context.GameBucketRewards.Where(x => x.BucketNo == post.BucketNo && x.IsActive == true && x.IsDeleted == false && x.Id == post.BucketRewardConditionsID).FirstOrDefault();
                if (Data.IsActive == true)
                {
                    Data.IsActive = false;
                    Data.IsDeleted = true;
                }
                if (context.Commit() > 0)
                {
                    PostData(post);
                }
            }
            var reesult = new
            {
                status = true,
                Message = "EditSuccessFully"
            };
            return Request.CreateResponse(HttpStatusCode.OK, reesult);
        }

        public int GetStreakDurationValue()
        {
            ////int StreakDuration = 0;
            //using (var db = new AuthContext())
            //{
            //    //StreakDuration = db.GameConditionMasters.Where(x => x.Name == "StreakDuration" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();
            //    MongoDbHelper<GameConditionMastersMongo> mongohelperZ = new MongoDbHelper<GameConditionMastersMongo>();
            //    StreakDuration = mongohelperZ.Select(x => x.Name == "StreakDuration" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();

            //}
            return StreakDuration;
        }


        public async Task<bool> CustomerLedger_Create(List<CreateCustomerLedgerDc> dataList)
        {
            bool resFlag = false;
            DateTime curdate = DateTime.Now;

            MongoHelper<GameCustomerLedger> mongohelper = new MongoHelper<GameCustomerLedger>();
            foreach (var obj in dataList)
            {
                var CustomerLedgerList = mongohelper.Select(x => x.CustomerId == obj.CustomerId && x.BucketNo == obj.BucketNo
                                    && x.ForRewardStrack == obj.ForRewardStrack && x.StreakIdFrom == obj.StreakIdFrom && x.StreakIdTo == obj.StreakIdTo
                                    && x.GameBucketRewardId == obj.GameBucketRewardId
                                    && x.GameStreakLevelConfigMasterId == obj.GameStreakLevelConfigMasterId && x.GameStreakLevelConfigDetailId == obj.GameStreakLevelConfigDetailId
                                    && x.IsActive == true && x.IsDeleted == false).ToList();
                //&& x.GameBucketNo == obj.GameBucketNo 

                if (CustomerLedgerList != null && CustomerLedgerList.Count() > 0)
                {
                    foreach (var item in CustomerLedgerList)
                    {
                        List<int> OrderIds = new List<int>();
                        if (OrderIds.Count > 0)
                        {
                            obj.OrderIdList.ForEach(x =>
                            {
                                OrderIds.Add(x);
                            });

                            if (item.ForRewardStrack == 1)
                            { item.GameBucketNo = obj.GameBucketNo; }

                            item.OrderIdList.AddRange(OrderIds);
                            mongohelper.Replace(item.Id, item);
                        }
                    }
                }
                else
                {
                    //List<long> iOrderIdsd = new List<long>();
                    //validatStreakLevelConfig.ForEach(x =>
                    //{
                    //    OrderIds.Add(x.Id);
                    //});

                    List<GameCustomerLedger> lst = new List<GameCustomerLedger>();
                    GameCustomerLedger data = new GameCustomerLedger();
                    data.CustomerId = obj.CustomerId;
                    data.GameBucketNo = obj.GameBucketNo;
                    data.BucketNo = obj.BucketNo;
                    data.ForRewardStrack = obj.ForRewardStrack; //"Reward=1  / Strack=2 / Redeem = 3
                    data.StreakIdFrom = obj.StreakIdFrom;
                    data.StreakIdTo = obj.StreakIdTo;
                    data.GameBucketRewardId = obj.GameBucketRewardId;
                    data.GameStreakLevelConfigMasterId = obj.GameStreakLevelConfigMasterId;
                    data.GameStreakLevelConfigDetailId = obj.GameStreakLevelConfigDetailId;
                    data.RewardValue = obj.RewardValue;

                    data.IsUpComingReward = true;
                    data.IsUpComingRewardDate = curdate;
                    data.IsCompleted = false;
                    data.IsCanceled = false;

                    data.BucketStartDate = obj.BucketStartDate;
                    data.BucketEndDate = obj.BucketEndDate;
                    data.OrderIdList = obj.OrderIdList;
                    data.GameStreakLevelConfigMasterIdList = obj.GameStreakLevelConfigMasterIdList;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    data.CreatedBy = 1;
                    data.CreatedDate = curdate;

                    lst.Add(data);
                    resFlag = await mongohelper.InsertManyAsync(lst);
                }
            }
            return resFlag;
        }

        public async Task<bool> CustomerLedger_InsertOrderId(int OrderId, UpdateCustomerLedgerStatusDc obj)
        {
            bool resFlag = false;
            DateTime curdate = DateTime.Now;

            MongoHelper<GameCustomerLedger> mongohelper = new MongoHelper<GameCustomerLedger>();
            var CustomerLedgerList = mongohelper.Select(x => x.CustomerId == obj.CustomerId && x.GameBucketNo == obj.GameBucketNo && x.BucketNo == obj.BucketNo
                                && x.ForRewardStrack == obj.ForRewardStrack && x.StreakIdFrom == obj.StreakIdFrom && x.StreakIdTo == obj.StreakIdTo
                                && x.GameBucketRewardId == obj.GameBucketRewardId
                                && x.GameStreakLevelConfigMasterId == obj.GameStreakLevelConfigMasterId && x.GameStreakLevelConfigDetailId == obj.GameStreakLevelConfigDetailId
                                && x.IsActive == true && x.IsDeleted == false).ToList();

            if (CustomerLedgerList != null && CustomerLedgerList.Count() > 0)
            {
                foreach (var item in CustomerLedgerList)
                {
                    List<int> OrderIdList = new List<int>();
                    //validatStreakLevelConfig.ForEach(x =>
                    //{
                    OrderIdList.Add(OrderId);
                    //});

                    item.OrderIdList = OrderIdList;
                    mongohelper.Replace(item.Id, item);
                }
            }

            return resFlag;
        }

        public async Task<bool> CustomerLedger_UpdateStatus(UpdateCustomerLedgerStatusDc obj)
        {

            bool resFlag = false;
            DateTime curdate = DateTime.Now;

            MongoHelper<GameCustomerLedger> mongohelper = new MongoHelper<GameCustomerLedger>();
            var CustomerLedgerList = mongohelper.Select(x => x.CustomerId == obj.CustomerId && x.GameBucketNo == obj.GameBucketNo && x.BucketNo == obj.BucketNo
                                && x.ForRewardStrack == obj.ForRewardStrack && x.StreakIdFrom == obj.StreakIdFrom && x.StreakIdTo == obj.StreakIdTo
                                && x.GameBucketRewardId == obj.GameBucketRewardId
                                && x.GameStreakLevelConfigMasterId == obj.GameStreakLevelConfigMasterId && x.GameStreakLevelConfigDetailId == obj.GameStreakLevelConfigDetailId
                                && x.IsActive == true && x.IsDeleted == false).ToList();

            if (CustomerLedgerList != null && CustomerLedgerList.Count() > 0)
            {
                foreach (var item in CustomerLedgerList)
                {
                    if (item.IsUpComingReward == false && item.IsCompleted == false && item.IsCanceled == true && item.IsRedeemedReward == false
                        && obj.IsUpComingReward == true && obj.IsCompleted == false && obj.IsCanceled == false)
                    {
                        item.IsUpComingReward = true;
                        item.IsUpComingRewardDate = curdate;
                        item.IsCanceled = false;
                        item.IsCanceledDate = null;
                    }

                    if (item.IsUpComingReward == true && item.IsCompleted == false
                        && obj.IsUpComingReward == false && obj.IsCompleted == true)
                    {
                        item.IsUpComingReward = false;
                        item.IsCompleted = true;
                        item.IsCompletedDate = curdate;
                    }

                    if (item.IsCanceled == false && item.IsCompleted == false
                        && obj.IsCanceled == true && obj.IsUpComingReward == false && obj.IsCompleted == false)
                    {
                        item.IsUpComingReward = false;
                        item.IsCanceled = true;
                        item.IsCanceledDate = curdate;
                    }

                    mongohelper.Replace(item.Id, item);
                }

            }

            return resFlag;
        }

        public async Task<bool> CustomerLedger_RedeemReward(int CustomerId, double RedeemValue)
        {
            bool resFlag = false;
            DateTime curdate = DateTime.Now;

            MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
            var dataCustomerBucketHdrList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();


            MongoHelper<GameCustomerLedger> mongohelper = new MongoHelper<GameCustomerLedger>();
            foreach (var obj in dataCustomerBucketHdrList)
            {
                var CustomerLedgerList = mongohelper.Select(x => x.CustomerId == CustomerId && x.BucketNo == obj.BucketNo && x.GameBucketNo == obj.GameBucketNo
                && x.IsRedeemedReward == true && x.IsActive == true && x.IsDeleted == false).ToList();
                //&& x.GameBucketNo == obj.GameBucketNo 

                if (!CustomerLedgerList.Any())
                {
                    List<GameCustomerLedger> lst = new List<GameCustomerLedger>();
                    GameCustomerLedger data = new GameCustomerLedger();
                    data.CustomerId = obj.CustomerId;
                    data.GameBucketNo = obj.GameBucketNo;
                    data.BucketNo = obj.BucketNo;
                    data.ForRewardStrack = 3; //"Reward=1  / Strack=2 / Redeem = 3
                    data.StreakIdFrom = 0;
                    data.StreakIdTo = 0;
                    data.GameBucketRewardId = 0;
                    data.GameStreakLevelConfigMasterId = 0;
                    data.GameStreakLevelConfigDetailId = 0;
                    data.RewardValue = RedeemValue;

                    data.IsUpComingReward = false;
                    data.IsCompleted = false;
                    data.IsCanceled = false;
                    data.IsRedeemedReward = true;
                    data.IsRedeemedRewardDate = curdate;

                    data.BucketStartDate = obj.BucketStartDate;
                    data.BucketEndDate = obj.BucketEndDate;
                    //data.OrderIdList = obj.OrderIdList;
                    //data.GameStreakLevelConfigMasterIdList = obj.GameStreakLevelConfigMasterIdList;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    data.CreatedBy = 1;
                    data.CreatedDate = curdate;

                    lst.Add(data);
                    resFlag = await mongohelper.InsertManyAsync(lst);
                }
            }
            return resFlag;
        }

        [HttpGet]
        [Route("GameBanner")]
        public async Task<CustomerUpcomingRewardsDc> CustomerUpcomingRewards(int CustomerId)
        {
            DateTime curDate = DateTime.Now.Date;
            CustomerUpcomingRewardsDc FillData = new CustomerUpcomingRewardsDc();

            using (var db = new AuthContext())
            {
                MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                var BucketNoList = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (BucketNoList != null)
                {
                    int CustBucketNo = BucketNoList.BucketNo;

                    var data = db.GameBucketRewards.Where(x => x.BucketNo > CustBucketNo && x.IsActive == true && x.IsDeleted == false).OrderBy(x => x.BucketNo).FirstOrDefault();
                    if (data != null)
                    {
                        FillData.WalletRewardValue = data.value;
                        //int WalletDaysReqired = (data.StartDate.Value - curDate).Days; 
                    }
                }
            }
            return FillData;
        }

        //[HttpGet]
        //[Route("NewCustomerSignUp")]
        public async Task<bool> NewCustomerSignUp(int customerid, string skcode)
        {
            DateTime today = DateTime.Now.Date;
            int IncreaseDay = 0;
            MongoDbHelper<GameConditionMastersMongo> mongohelperIncreaseDay = new MongoDbHelper<GameConditionMastersMongo>();
            IncreaseDay = mongohelperIncreaseDay.Select(x => x.Name == "IncreaseDay" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();

            MongoDbHelper<GameCustomerBucketHdr> mongohelper = new MongoDbHelper<GameCustomerBucketHdr>();
            GameCustomerBucketHdr Gamecustomer = new GameCustomerBucketHdr();
            Gamecustomer.SkCode = skcode;
            Gamecustomer.CustomerId = customerid;
            Gamecustomer.CRMBucketNo = 1;
            Gamecustomer.BucketNo = 1;
            Gamecustomer.NextBucketNo = 0;
            Gamecustomer.GameBucketNo = 1;
            Gamecustomer.LastOrderDate = today.AddDays(-1);
            ////Issue Fixed: NextOrderDays shold be come 0.
            //Gamecustomer.BucketStartDate = today;
            //Gamecustomer.BucketEndDate = today.AddDays(IncreaseDay - 1);
            Gamecustomer.BucketStartDate = today.AddDays(-1);
            Gamecustomer.BucketEndDate = today.AddDays(-1);


            Gamecustomer.CreatedDate = DateTime.Now;
            Gamecustomer.IsActive = true;
            Gamecustomer.IsDeleted = false;
            Gamecustomer.CreatedBy = customerid;

            var flag = mongohelper.Insert(Gamecustomer);
            return flag;
        }

        [HttpGet]
        [Route("GetStreakDashboard")]
        public SingleMapview GetStreakDashboard(int CustomerId)
        {
            SingleMapview result = new SingleMapview();
            using (var db = new AuthContext())
            {
                MongoDbHelper<GameStreakCustomerTransaction> mongoDbHelper = new MongoDbHelper<GameStreakCustomerTransaction>();
                var customerwisedate = mongoDbHelper.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false && x.IsExpired == false).ToList();
                if (customerwisedate.Count() > 0)
                {
                    //List<long> ids = new List<long>();
                    //ids = customerwisedate.Select(x => x.GameStreakLevelConfigMasterId).Distinct().ToList();
                    result.getStreakDashboardDCs = customerwisedate.Select(y => new GetStreakDashboardDC { StreakId = y.StreakId, StartDate = y.BucketStartDate, EndDate = y.BucketEndDate, Status = y.StreakIsFulfill }).ToList();
                    var ids = customerwisedate.Select(x => x.GameStreakLevelConfigMasterId).Distinct().FirstOrDefault();
                    //var ids = customerwisedate.GroupBy(y=>y.BucketNo).Distinct().ToList();
                    //long Id = ids.FirstOrDefault();
                    //var data = db.GameStreakLevelConfigMasters.Where(x => x.Id == Id).FirstOrDefault();
                    var data = db.GameStreakLevelConfigMasters.Where(x => x.Id == ids.FirstOrDefault()).FirstOrDefault();
                    if (data != null)
                    {
                        int Bfrom = data.BucketNoFrom; int skip = 0; int take = 0; int IsActiveCurrent1 = 0;
                        int Bto = data.BucketNoTo;
                        DateTime bdate = data.CreatedDate;
                        var fBucketNoFrom = new SqlParameter("@BucketNoFrom", Bfrom);
                        var fBucketNoTo = new SqlParameter("@BucketNoTo", Bto);
                        var fskip = new SqlParameter("@skip", skip);
                        var ftake = new SqlParameter("@take", take);
                        var fCreateddate = new SqlParameter("@CreatedDate", bdate);
                        var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);
                        result.gameStreakLevelConfigDetailDcs = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, fCreateddate, fIsActiveCurrent).ToList();
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("GetCustomerwithSkCode")]
        public List<GameSkcodeDC> GetCustomerwithSkCode()
        {
            List<GameSkcodeDC> gameSkcodeDCs = new List<GameSkcodeDC>();
            using (var context = new AuthContext())
            {
                gameSkcodeDCs = context.Customers.Select(x => new GameSkcodeDC { CustomerId = x.CustomerId, Skcode = x.Skcode, ShopName = x.ShopName }).ToList();
                return gameSkcodeDCs;
            }
        }

    }
}
