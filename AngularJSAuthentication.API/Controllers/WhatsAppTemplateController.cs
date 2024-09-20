using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.RequestAccess;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CRM;
using AngularJSAuthentication.Model.Permission;
using AspNetIdentity.WebApi.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{

    [RoutePrefix("api/WhatsAppTemplate")]
    public class WhatsAppTemplateController : BaseApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        static readonly HttpClient client = new HttpClient();
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();


        #region WhatsApp template from local DB 

        [HttpGet]
        [Route("GetTemplatebyId")]
        public async Task<WhatsAppTemplate> GetTemplatebyId()
        {
            using (var db = new AuthContext())
            {
                WhatsAppTemplate whatsAppTemplate = db.WhatsAppTemplates.FirstOrDefault();
                if (whatsAppTemplate != null)
                {
                    whatsAppTemplate.WhatsAppTemplateVariableDetails = db.WhatsAppTemplateVariableDetails.Where(x => x.TemplateId == whatsAppTemplate.TemplateId).ToList();
                }
                return whatsAppTemplate;
            }
        }

        [HttpGet]
        [Route("GetAllTemplateAsync")]
        public async Task<List<WhatsAppTemplate>> GetAllTemplateAsync()
        {
            using (var db = new AuthContext())
            {
                List<WhatsAppTemplate> whatsAppTemplates = db.WhatsAppTemplates.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                if (whatsAppTemplates != null && whatsAppTemplates.Any())
                {
                    foreach (var item in whatsAppTemplates)
                    {
                        item.WhatsAppTemplateVariableDetails = db.WhatsAppTemplateVariableDetails.Where(x => x.TemplateId == item.TemplateId).ToList();
                    }
                }
                return whatsAppTemplates;
            }
        }
        [AllowAnonymous]
        [Route("AddUpdateWhatsAppTemplate")]
        [HttpPost]
        public async Task<ResponseMsg> AddUpdateWhatsAppTemplate(WhatsAppTemplateDc whatsAppTemplateDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            logger.Info("User ID : {0} , userid");

            WhatsAppTemplate whatsAppTemplate = new WhatsAppTemplate
            {
                ImageName = whatsAppTemplateDc.ImageName,
                ImagePath = whatsAppTemplateDc.ImagePath,
                IsActive = true,
                IsDeleted = false,
                Language = whatsAppTemplateDc.Language,
                TemplateDescription = whatsAppTemplateDc.TemplateDescription,
                TemplateId = whatsAppTemplateDc.TemplateId,
                TemplateJson = whatsAppTemplateDc.TemplateJson,
                TemplateName = whatsAppTemplateDc.TemplateNewName,
                CreatedBy = userid,
                CreatedDate = DateTime.Now,
                WhatsAppTemplateVariableDetails = whatsAppTemplateDc.WhatsAppTemplateVariableDetails
            };

            bool isEdit = false;
            ResponseMsg result = new ResponseMsg();
           

            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            int newId = 0;
            if (whatsAppTemplate != null)
            {

                WhatsAppMessageDc message = new WhatsAppMessageDc
                {
                    name = whatsAppTemplateDc.TemplateName,
                    @namespace = ConfigurationManager.AppSettings["WhatsAppTemplateNamespace"],
                    language = new WhatsAppMessageLanguageDc
                    {
                        code = whatsAppTemplate.Language,
                        policy = ConfigurationManager.AppSettings["WhatsAppTemplatePolicy"]
                    },
                    components = new List<WhatsAppMessageComponentDc>()
                };
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {

                        if (!string.IsNullOrEmpty(whatsAppTemplate.ImageName))
                        {

                            var component = new WhatsAppMessageComponentDc
                            {
                                type = "header",
                                parameters = new List<WhatsAppMessageParameterDc>()

                            };


                            component.parameters.Add(new WhatsAppMessageParameterDc
                            {
                                type = "image",
                                image = new WhatsAppMessageImageDc
                                {
                                    filename = whatsAppTemplate.ImageName,
                                    link = whatsAppTemplate.ImagePath
                                }
                            });
                            message.components.Add(component);
                        }
                        else
                        {



                        }

                        var ExistTemplate = context.WhatsAppTemplates.Where(x => x.TemplateName == whatsAppTemplateDc.TemplateNewName && x.TemplateId != whatsAppTemplate.TemplateId && x.IsActive == true && x.IsDeleted ==false).FirstOrDefault();
                        if (ExistTemplate == null)
                        {
                            if (whatsAppTemplate.TemplateId == 0)
                            {
                                whatsAppTemplate.IsActive = true;
                                whatsAppTemplate.IsDeleted = false;
                                whatsAppTemplate.CreatedDate = DateTime.Now;
                                whatsAppTemplate.CreatedBy = userid;
                                context.WhatsAppTemplates.Add(whatsAppTemplate);

                                context.Commit();
                                newId = whatsAppTemplate.TemplateId;
                                //dbContextTransaction.Complete();

                                //if (newId > 0)
                                //{
                                //    result.Status = true;
                                //    result.Message = "Success";
                                //}
                            }
                            else
                            {
                                isEdit = true;
                                List<WhatsAppTemplateVariableDetail> whatsAppTemplateVariableDetails = context.WhatsAppTemplateVariableDetails.Where(x => x.TemplateId == whatsAppTemplate.TemplateId).ToList();
                                if (whatsAppTemplateVariableDetails != null && whatsAppTemplateVariableDetails.Any())
                                {
                                    foreach (WhatsAppTemplateVariableDetail item in whatsAppTemplateVariableDetails)
                                    {
                                        context.WhatsAppTemplateVariableDetails.Remove(item);
                                    }
                                }
                                //string Logourl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                //                               , HttpContext.Current.Request.Url.DnsSafeHost
                                //                               , HttpContext.Current.Request.Url.Port, whatsAppTemplate.ImagePath);
                                WhatsAppTemplate obj = context.WhatsAppTemplates.Where(x => x.TemplateId == whatsAppTemplate.TemplateId).FirstOrDefault();
                                obj.TemplateName = whatsAppTemplate.TemplateName;
                                obj.TemplateDescription = whatsAppTemplate.TemplateDescription;
                                obj.Language = whatsAppTemplate.Language;
                                obj.ImageName = whatsAppTemplate.ImageName;
                                obj.ImagePath = whatsAppTemplate.ImagePath;
                                obj.ModifiedDate = DateTime.Now;
                                obj.ModifiedBy = userid;

                                newId = obj.TemplateId;
                                context.Entry(obj).State = EntityState.Modified;
                                context.Commit();

                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "TemplateName Already Exists!";
                            return result;
                        }
                        if (whatsAppTemplate.WhatsAppTemplateVariableDetails != null && whatsAppTemplate.WhatsAppTemplateVariableDetails.Any())
                        {




                            var bodycomponent = new WhatsAppMessageComponentDc
                            {
                                type = "body",
                                parameters = new List<WhatsAppMessageParameterDc>()

                            };
                            message.components.Add(bodycomponent);
                            foreach (WhatsAppTemplateVariableDetail item in whatsAppTemplate.WhatsAppTemplateVariableDetails)
                            {
                                item.TemplateId = newId;

                                if (isEdit)
                                {
                                    context.WhatsAppTemplateVariableDetails.Add(item);
                                }

                                bodycomponent.parameters.Add(new WhatsAppMessageParameterDc
                                {
                                    type = "text",
                                    text = item.VariableType
                                });

                            }
                        }

                        var template = context.WhatsAppTemplates.FirstOrDefault(x => x.TemplateId == newId);
                        template.TemplateJson = JsonConvert.SerializeObject(message);
                        context.Entry(template).State = EntityState.Modified;
                        context.Commit();

                        dbContextTransaction.Complete();
                        result.Status = true;
                        result.Message = "Success";


                    }
                }
            }
            return result;
        }




        [HttpDelete]
        [Route("DeleteWhatsAppTemplate")]
        public bool DeleteWhatsAppTemplate(int Id)
        {
            bool isSuccess = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            logger.Info("User ID : {0} , userid");

            using (var db = new AuthContext())
            {
                WhatsAppTemplate whatsAppTemplate = db.WhatsAppTemplates.Find(Id);
                List<WhatsAppTemplateVariableDetail> whatsAppTemplateVariableDetails = db.WhatsAppTemplateVariableDetails.Where(x => x.TemplateId == Id).ToList();
                if (whatsAppTemplate == null)
                {
                    isSuccess = false;
                    return isSuccess;
                }
                if (whatsAppTemplateVariableDetails != null && whatsAppTemplateVariableDetails.Any())
                {
                    foreach (var item in whatsAppTemplateVariableDetails)
                    {
                        db.WhatsAppTemplateVariableDetails.Remove(item);
                    }
                }

                whatsAppTemplate.ModifiedDate = DateTime.Now;
                whatsAppTemplate.ModifiedBy = userid;
                whatsAppTemplate.IsActive = false;
                whatsAppTemplate.IsDeleted = true;
                db.Entry(whatsAppTemplate).State = EntityState.Modified;
                db.Commit();
                isSuccess = true;
                return isSuccess;
            }
        }

        [HttpGet]
        [Route("GetWATemplateValConfigurationList")]
        public List<WhatsAppTemplateValConfiguration> GetWATemplateValConfigurationList()
        {
            using (var db = new AuthContext())
            {
                List<WhatsAppTemplateValConfiguration> whatsAppTemplateValConfigurations = db.WhatsAppTemplateValConfigurations.ToList();
                return whatsAppTemplateValConfigurations;
            }
        }

        #endregion

        #region third party intgartion (WhatsApp integration)
        [AllowAnonymous]
        [HttpGet]
        [Route("getWhatsAppTemplateListFromThirdParty")]
        public async Task<string> getWhatsAppTemplateListFromThirdParty()
        {
            WhatsAppTemplateManager whatsAppTemplateManager = new WhatsAppTemplateManager();
            return await whatsAppTemplateManager.getWhatsAppTemplateListFromThirdParty();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("WhatsAppAPIPostBulkData/{TemplateId}")]
        public async Task<bool> WhatsAppAPIPostBulkData([FromUri] int TemplateId, [FromBody] List<string> skCodes)
        {
            string filePath = string.Empty;
            if (TemplateId > 0 && skCodes != null)
            {
                WhatsAppTemplateManager whatsAppTemplateManager = new WhatsAppTemplateManager();
                List<WhatsAppTemplateExcelDc> whatsAppTemplateExcelDcList = new List<WhatsAppTemplateExcelDc>();

                whatsAppTemplateExcelDcList = await whatsAppTemplateManager.GetWhatsAppTemplateDetailById(TemplateId, skCodes);
                if (whatsAppTemplateExcelDcList != null)
                {
                    DataTable dashboardDt = ClassToDataTable.CreateDataTable(whatsAppTemplateExcelDcList);

                    if (whatsAppTemplateExcelDcList != null && whatsAppTemplateExcelDcList.Count > 0)
                    {
                        dashboardDt = new DataTable();
                        dashboardDt.Columns.Add("From Number");
                        dashboardDt.Columns.Add("API_KEY");
                        dashboardDt.Columns.Add("To Number");
                        dashboardDt.Columns.Add("Template JSON");
                        foreach (var item in whatsAppTemplateExcelDcList)
                        {
                            DataRow row = dashboardDt.NewRow();
                            row[0] = item.From_Number;
                            row[1] = item.API_KEY;
                            row[2] = item.To_Number;
                            row[3] = item.Template_JSON;
                            dashboardDt.Rows.Add(row);
                        }
                    }
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/UploadedWhatsAppExcel");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);
                    var fileName = "WhatsAppBulkData" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    filePath = ExcelSavePath + "\\" + fileName;
                    ExcelGenerator.DataTable_To_Excel(dashboardDt, "UploadedWhatsAppExcel", filePath);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        bool result = whatsAppTemplateManager.PostWhatsAppBulkData(filePath);
                    }
                }
            }
            return true;
        }


        #endregion


        [HttpPost]
        [Route("GetGroupList")]
        public List<groupListDC> GetGroupList(List<int> warehouseIds)
        {
            WhatsAppTemplateManager obj = new WhatsAppTemplateManager();
            List<groupListDC> res = new List<groupListDC>();
            res = obj.GetGroupList(warehouseIds);
            return res;

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("InsertWhatsAppGrpNotify")]
        public async Task<APIResponse> InsertWhatsAppGrpNotify(WhatsappGroupNotifDc whatsappGroupNotifDc)
        {
            bool IsEdit = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                if (whatsappGroupNotifDc.Id > 0)
                {
                    var updateWhatsAppGroupNotificationMasters = db.WhatsAppGroupNotificationMasters.FirstOrDefault(x => x.Id == whatsappGroupNotifDc.Id);
                    updateWhatsAppGroupNotificationMasters.ModifiedBy = userid;
                    updateWhatsAppGroupNotificationMasters.ModifiedDate = DateTime.Now;
                    updateWhatsAppGroupNotificationMasters.CityIds = whatsappGroupNotifDc.CityIds;
                    updateWhatsAppGroupNotificationMasters.TemplateId = whatsappGroupNotifDc.TemplateId;
                    updateWhatsAppGroupNotificationMasters.WarehouseIds = whatsappGroupNotifDc.WarehouseIds;
                    updateWhatsAppGroupNotificationMasters.NotificationName = whatsappGroupNotifDc.NotificationName;
                    updateWhatsAppGroupNotificationMasters.GroupIds = whatsappGroupNotifDc.GroupIds;
                    updateWhatsAppGroupNotificationMasters.IsActive = true;
                    updateWhatsAppGroupNotificationMasters.IsDeleted = false;
                    updateWhatsAppGroupNotificationMasters.IsSend = false;

                    db.Entry(updateWhatsAppGroupNotificationMasters).State = EntityState.Modified;
                    IsEdit = true;
                }
                else
                {
                    db.WhatsAppGroupNotificationMasters.Add(new Model.CRM.WhatsAppGroupNotificationMaster()
                    {
                        CityIds = whatsappGroupNotifDc.CityIds,
                        TemplateId = whatsappGroupNotifDc.TemplateId,
                        WarehouseIds = whatsappGroupNotifDc.WarehouseIds,
                        NotificationName = whatsappGroupNotifDc.NotificationName,
                        Id = whatsappGroupNotifDc.Id,
                        GroupIds = whatsappGroupNotifDc.GroupIds,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        IsSend = false,
                        CreatedBy = userid
                    });
                }
                db.Commit();
            }
            if (IsEdit == true)
            {
                return new APIResponse { Status = true, Message = "Updated Successfully!" };
            }
            else
            {
                return new APIResponse { Status = true, Message = "Saved Successfully!" };
            }

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("WhatsAppGroupNotificationList")]
        public WhatsappGroupNotifyList WhatsAppGroupNotificationList(int skip, int take)
        {
            using (var db = new AuthContext())
            {
                WhatsappGroupNotifyList List = new WhatsappGroupNotifyList();

                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetWhatsAppGroupNotificationMasters]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Skip", value: skip));
                cmd.Parameters.Add(new SqlParameter("@Take", value: take));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)db).ObjectContext.Translate<WhatsappGroupNotifDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    List.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                }
                List.whatsappGroupNotifDcs = data;
                return List;
            }

        }

        [Route("DeleteWhatsAppGroupNotificationMasters")]
        [HttpGet]
        public async Task<MsgDc> DeleteWhatsAppGroupNotificationMasters(long Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            MsgDc msgdc = new MsgDc();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                var DeleteWhatsAppListbyId = context.WhatsAppGroupNotificationMasters.Where(x => x.Id == Id).FirstOrDefault();
                DeleteWhatsAppListbyId.ModifiedDate = DateTime.Now;
                DeleteWhatsAppListbyId.ModifiedBy = userid;
                DeleteWhatsAppListbyId.IsActive = false;
                DeleteWhatsAppListbyId.IsDeleted = true;
                context.Entry(DeleteWhatsAppListbyId).State = EntityState.Modified;
                var a = context.Commit();
                if (a > 0)
                {
                    msgdc.Msg = "Deleted Successfully";
                }
                else
                {
                    msgdc.Msg = "Something went wrong!";
                }

                return msgdc;
            }

        }

        [AllowAnonymous]
        [Route("SendNotification")]
        [HttpGet]
        public async Task<bool> SendNotification(long Id)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                WhatsAppGroupNotificationMaster whatsAppGroupNotificationMaster = db.WhatsAppGroupNotificationMasters.FirstOrDefault(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false);
                if (whatsAppGroupNotificationMaster != null)
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();

                    List<int> warehouseids = whatsAppGroupNotificationMaster.WarehouseIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                    List<int> groupIds = whatsAppGroupNotificationMaster.GroupIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();

                    //WarehouseIds
                    var warehouseIdDt = new DataTable();
                    warehouseIdDt.Columns.Add("IntValue");

                    foreach (var item in warehouseids)
                    {
                        var dr = warehouseIdDt.NewRow();
                        dr["IntValue"] = item;
                        warehouseIdDt.Rows.Add(dr);
                    }
                    var WarehouseIdparam = new SqlParameter("warehouseIds", warehouseIdDt);
                    WarehouseIdparam.SqlDbType = SqlDbType.Structured;
                    WarehouseIdparam.TypeName = "dbo.IntValues";

                    //GroupIds
                    var groupIdDt = new DataTable();
                    groupIdDt.Columns.Add("IntValue");

                    foreach (var item in groupIds)
                    {
                        var dr = groupIdDt.NewRow();
                        dr["IntValue"] = item;
                        groupIdDt.Rows.Add(dr);
                    }
                    var groupIparam = new SqlParameter("groupIds", groupIdDt);
                    groupIparam.SqlDbType = SqlDbType.Structured;
                    groupIparam.TypeName = "dbo.IntValues";

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[dbo].[GetWhatsAppGroupCustomerLists]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(WarehouseIdparam);
                    cmd.Parameters.Add(groupIparam);

                    var reader = cmd.ExecuteReader();
                    var data = ((IObjectContextAdapter)db).ObjectContext.Translate<WhatsAppGroupCustomer>(reader).ToList();
                    db.Database.Connection.Close();
                    if (data != null && data.Any())
                    {
                        WhatsAppTemplateManager whatsAppTemplateManager = new WhatsAppTemplateManager();
                        await whatsAppTemplateManager.WhatsAppAPIPostBulkData((int)whatsAppGroupNotificationMaster.TemplateId, data.Select(x => x.Skcode).ToList(), HttpContext.Current.Server.MapPath("~/UploadedWhatsAppExcel"));
                        List<int> CustomerIdList = data.Select(x => x.CustomerId).ToList();
                        foreach (var items in CustomerIdList)
                        {
                            WhatsAppGroupNotificationDetails whatsAppGroupNotificationDetails = new WhatsAppGroupNotificationDetails
                            {
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                WhatsAppGroupNotificationMasterId = whatsAppGroupNotificationMaster.Id,
                                CustomerId = items
                            };
                            db.WhatsAppGroupNotificationDetails.Add(whatsAppGroupNotificationDetails);
                        }
                    }
                    whatsAppGroupNotificationMaster.IsSend = true;
                    db.Entry(whatsAppGroupNotificationMaster).State = EntityState.Modified;
                    db.Commit();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}


