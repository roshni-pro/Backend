using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Mongo;
using LinqKit;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TemplateMaster")]
    public class TemplateMasterController : BaseAuthController
    {

        [HttpGet]
        [Route("GetAllTemplateMasterList")]
        public List<TemplateMasterDc> GetAllTemplateMasterList()
        {
            TemplateMasterDc obj = new TemplateMasterDc();
            MongoDbHelper<TemplateMaster> mongoDbHelper = new MongoDbHelper<TemplateMaster>();
            List<TemplateMaster> TempMlist = new List<TemplateMaster>();
            List<TemplateMasterDc> lists = new List<TemplateMasterDc>();
            var searchPredicate = PredicateBuilder.New<TemplateMaster>(x => x.IsDeleted == false);

            TempMlist = mongoDbHelper.Select(searchPredicate).ToList();
            lists = Mapper.Map(TempMlist).ToANew<List<TemplateMasterDc>>();
            lists.Reverse();
            return lists;

        }

        [HttpGet]
        [Route("TemplateMasterListById")]
        public TemplateMasterDc TemplateMasterListById(string Id)
        {
            TemplateMasterDc obj = new TemplateMasterDc();
            MongoDbHelper<TemplateMaster> mongoDbHelper = new MongoDbHelper<TemplateMaster>();
            TemplateMaster TempMasterlist = new TemplateMaster();

            var searchPredicate = PredicateBuilder.New<TemplateMaster>(x => x.IsDeleted == false);

            TempMasterlist = mongoDbHelper.Select(searchPredicate).FirstOrDefault();
            var TemplateMaster = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(Id)).FirstOrDefault();
            TemplateMasterDc list = new TemplateMasterDc();
            list = Mapper.Map(TemplateMaster).ToANew<TemplateMasterDc>();

            return list;

        }


        [Route("AddTemplateMaster")]
        [HttpPost]
        public async Task<TemplateMaster> InsertTemplateMaster(TemplateMasterDc templateMaster)
        {
            TemplateMaster tempM = new TemplateMaster();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && templateMaster != null && templateMaster.TemplateName != null)
            {

                MongoDbHelper<TemplateMaster> mongoDbHelper = new MongoDbHelper<TemplateMaster>();
                var data = mongoDbHelper.Select(x => x.TemplateName.ToUpper() == templateMaster.TemplateName.ToUpper() && x.IsDeleted == false).FirstOrDefault();
                if (data == null)
                {

                    tempM.TemplateMasterId = templateMaster.TemplateMasterId;
                    tempM.TemplateName = templateMaster.TemplateName;
                    tempM.CreatedBy = userid;
                    tempM.CreatedDate = DateTime.Now;
                    tempM.IsActive = templateMaster.IsActive;
                    tempM.IsDeleted = false;
                    tempM.ModifiedBy = userid;
                    tempM.ModifiedDate = DateTime.Now;
                    var Status = await mongoDbHelper.InsertAsync(tempM);
                    tempM.Msg = "Successfully Added!";
                }
                else
                {
                    tempM.Msg = "Already Exist!!";
                } 
            }




            return tempM;
        }

        [Route("UpdateTemplateMaster")]
        [HttpPost]
        public async Task<TemplateMaster> UpdateTemplateMaster(TemplateMasterDc tempMaster)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool flag = false;

            if (tempMaster != null && tempMaster.TemplateMasterId > 0 && userid > 0)
            {
                MongoDbHelper<TemplateMaster> mongoDbHelper = new MongoDbHelper<TemplateMaster>();
                var temp = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(tempMaster.Id)).FirstOrDefault();
                if (temp != null)
                {
                    temp.IsActive = tempMaster.IsActive;
                    temp.ModifiedBy = userid;
                    temp.ModifiedDate = DateTime.Now;
                    temp.TemplateName = tempMaster.TemplateName;
                    flag = mongoDbHelper.Replace(ObjectId.Parse(tempMaster.Id), temp);
                    return temp;
                }

            }
            return null;
        }

        [Route("DeleteTemplateMaster")]
        [HttpGet]
        public TemplateMasterDc DeleteTemplateMaster(string Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            TemplateMasterDc res = new TemplateMasterDc
            {
                msg = "",
                Result = false
            };
            if (Id != null && userid > 0)
            {
                MongoDbHelper<TemplateMaster> mongoDbHelper = new MongoDbHelper<TemplateMaster>();
                {
                    bool flag = false;
                    var smstemp = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(Id)).FirstOrDefault();
                    if (smstemp != null)
                    {
                        smstemp.IsActive = false;
                        smstemp.IsDeleted = true;
                        smstemp.ModifiedBy = userid;
                        smstemp.ModifiedDate = DateTime.Now;

                        flag = mongoDbHelper.Replace(ObjectId.Parse(Id), smstemp);

                        if (flag)
                        {
                            res.msg = "TemplateMaster Deleted";
                        }
                    }
                }
            }
            return res;
        }
    }
}
