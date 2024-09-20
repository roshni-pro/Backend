using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
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
    [RoutePrefix("api/SMSTemplate")]
    public class SMSTemplateController : BaseAuthController
    {


        [HttpGet]
        [Route("TemplateList")]
        public List<SMSTemplateDc> TemplateList(int? AppType)
        {
            SMSTemplateDc obj = new SMSTemplateDc();
            MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
            List<SMSTemplate> Templist = new List<SMSTemplate>();
            MongoDbHelper<TemplateMaster> mongoDbTemplateHelper = new MongoDbHelper<TemplateMaster>();
            List<TemplateMaster> TemplateStoredlist = new List<TemplateMaster>();
            if (AppType > 0)
            {
                var searchPredicate = PredicateBuilder.New<SMSTemplate>(x => x.IsDeleted == false && x.AppType == AppType);
                Templist = mongoDbHelper.Select(searchPredicate).ToList();
            }
            else
            {
                var searchPredicate = PredicateBuilder.New<SMSTemplate>(x => x.IsDeleted == false);
                Templist = mongoDbHelper.Select(searchPredicate).ToList();
            }
            List<SMSTemplateDc> list = new List<SMSTemplateDc>();
            list = Mapper.Map(Templist).ToANew<List<SMSTemplateDc>>();
            var searchTemplatePredicate = PredicateBuilder.New<TemplateMaster>(x => x.IsDeleted == true);
            TemplateStoredlist = mongoDbTemplateHelper.Select(searchTemplatePredicate).ToList();
            List<TemplateMaster> temPlist = new List<TemplateMaster>();
            temPlist = Mapper.Map(TemplateStoredlist).ToANew<List<TemplateMaster>>();
            foreach (var lst in list)
            {
                if (lst.AppType == (int)AppEnum.RetailerApp)
                {
                    lst.AppTypeName = "Retailer App";
                }
                else if (lst.AppType == (int)AppEnum.DeliveryApp)
                {
                    lst.AppTypeName = "Delivery App";
                }
                else if (lst.AppType == (int)AppEnum.SalesApp)
                {
                    lst.AppTypeName = "Sales App";
                }
                else if (lst.AppType == (int)AppEnum.SarthiApp)
                {
                    lst.AppTypeName = "Sarthi App";
                }
                else if (lst.AppType == (int)AppEnum.Others)
                {
                    lst.AppTypeName = "Others";
                }

                foreach (var tem in temPlist)
                {
                    if (lst.TemplateType == tem.TemplateName)
                    {
                        lst.TemplateType = null;
                    }
                }

            }
            // obj.SMSTemplate = list;
            list.Reverse();
            return list;

        }

        [HttpGet]
        [Route("TemplateListById")]
        public SMSTemplateDc TemplateListById(string Id)
        {
            SMSTemplateDc obj = new SMSTemplateDc();
            MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
            SMSTemplate Templist = new SMSTemplate();

            var searchPredicate = PredicateBuilder.New<SMSTemplate>(x => x.IsDeleted == false);

            Templist = mongoDbHelper.Select(searchPredicate).FirstOrDefault();
            var SmsTemplate = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(Id)).FirstOrDefault();
            //  var plant = collection.Find(Builders<Employee>.Filter.Where(s => s.Id == id)).FirstOrDefault();
            // return Json(plant);
            SMSTemplateDc list = new SMSTemplateDc();
            list = Mapper.Map(SmsTemplate).ToANew<SMSTemplateDc>();
            // obj.SMSTemplate = list;

            return list;

        }

        //[Route("getTemplateForSMS")]
        //[HttpGet]
        //public SMSTemplate getTemplateForSMS(int AppType, string TemplateType)
        //{
        //    SMSTemplateHelper helper = new SMSTemplateHelper();
        //    var result = helper.getTemplateForSMS( AppType,TemplateType);
        //    return result;
        //}

        [Route("AddTemplate")]
        [HttpPost]
        public async Task<SMSTemplate> InsertTemplate(SMSTemplateDc smstemplate)
        {
            SMSTemplate temp = new SMSTemplate();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (userid > 0 && smstemplate != null && smstemplate.AppType > 0 && smstemplate.TemplateType != null && smstemplate.Template != null)
            {
                MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
                var data = mongoDbHelper.Select(x => x.AppType == smstemplate.AppType && x.TemplateType == smstemplate.TemplateType && x.IsDeleted == false).FirstOrDefault();
                if (data == null)
                {
                    temp.TemplateType = smstemplate.TemplateType;
                    temp.Template = smstemplate.Template;
                    temp.DLTID = smstemplate.DLTID;
                    temp.AppType = smstemplate.AppType;
                    temp.CreatedBy = userid;
                    temp.CreatedDate = DateTime.Now;
                    temp.IsActive = smstemplate.IsActive;
                    temp.IsDeleted = false;
                    temp.ModifiedBy = userid;
                    temp.ModifiedDate = DateTime.Now;
                    var Status = await mongoDbHelper.InsertAsync(temp);
                    temp.Msg = "Successfully Added!";
                }
                else
                {
                    temp.Msg = "Already Exist!!";
                }
            }
            return temp;
        }

        [Route("UpdateTemplate")]
        [HttpPost]
        public async Task<SMSTemplate> UpdateTemplate(SMSTemplateDc smstemplate)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool flag = false;
            var temp = new SMSTemplate();
            if (userid > 0 && smstemplate != null && smstemplate.AppType > 0 && smstemplate.TemplateType != null && smstemplate.Template != null)
            {
                MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
                temp = mongoDbHelper.Select(x => x.Id == ObjectId.Parse(smstemplate.Id)).FirstOrDefault();
                var recordfound = mongoDbHelper.Select(x => x.AppType == smstemplate.AppType && x.TemplateType == smstemplate.TemplateType && x.IsDeleted == false).FirstOrDefault();
                if (temp != null && recordfound != null)
                {
                    temp.IsActive = smstemplate.IsActive;
                    temp.IsDeleted = false;
                    temp.ModifiedBy = userid;
                    temp.ModifiedDate = DateTime.Now;
                    temp.Template = smstemplate.Template;
                    temp.TemplateType = smstemplate.TemplateType;
                    temp.DLTID = smstemplate.DLTID;
                    temp.AppType = smstemplate.AppType;
                    flag = mongoDbHelper.Replace(ObjectId.Parse(smstemplate.Id), temp);
                }
            }
            return temp;
        }

        [Route("DeleteTemplate")]
        [HttpGet]
        public SMSTemplateDc DeleteTemplate(string Id)
        {
            bool flag = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            SMSTemplateDc res = new SMSTemplateDc
            {
                msg = "",
                Result = false
            };
            if (userid > 0 && Id != null)
            {

                MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
                {

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
                            res.msg = "Template Deleted";
                        }
                    }
                }
            }
            return res;
        }

    }

}



