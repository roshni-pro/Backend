using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class SMSTemplateHelper
    {
        
        //public SMSTemplate getTemplateForSMS(int AppType, string TemplateType)
        //{
        //    MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
        //    var searchTempPredicate = PredicateBuilder.New<SMSTemplate>(x => x.IsDeleted == false && x.AppType == AppType && x.TemplateType == TemplateType && x.IsActive);
        //    SMSTemplate Template = mongoDbHelper.Select(searchTempPredicate).FirstOrDefault();
        //    return Template;
        //}


        public static DLTSMS getTemplateText(int AppType, string TemplateType)
        {
            MongoDbHelper<SMSTemplate> mongoDbHelper = new MongoDbHelper<SMSTemplate>();
            var searchTempPredicate = PredicateBuilder.New<SMSTemplate>(x => x.IsDeleted == false && x.AppType == AppType && x.TemplateType == TemplateType && x.IsActive);
            SMSTemplate Template = mongoDbHelper.Select(searchTempPredicate).FirstOrDefault();
            DLTSMS dltSms = null;
            if (Template != null)
            {
                dltSms = new DLTSMS
                {
                    DLTId = Template.DLTID,
                    Template = Template.Template
                };
            }
            return dltSms;
        }


    }

    public class DLTSMS
    {
        public string Template { get; set; }
        public string DLTId { get; set; }
    }
}