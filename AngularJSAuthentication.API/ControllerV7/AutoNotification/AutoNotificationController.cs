using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.AutoNotification;
using AngularJSAuthentication.Model.AutoNotification;
using Hangfire;
using Nito.AspNetBackgroundTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.AutoNotification
{
    [RoutePrefix("api/AutoNotification")]
    public class AutoNotificationController : ApiController
    {
        [Route("ANEntityMaster")]
        [HttpGet]
        public List<ANEntityMaster> GetANEntityMasterList()
        {
            using (var authContext = new AuthContext())
            {
                var anEntityMasterList = authContext.ANEntityMaster.ToList();
                return anEntityMasterList;
            }
        }

        [Route("City")]
        [HttpGet]
        public dynamic GetCityList()
        {
            using (var authContext = new AuthContext())
            {
                var cityList = authContext.Cities.Where(x => x.active == true && x.Deleted == false).ToList();
                return cityList;
            }

        }

        [Route("Warehouse/cityid/{cityid}")]
        [HttpGet]
        public dynamic GetWarehouseList(int cityid)
        {
            using (var authContext = new AuthContext())
            {
                var warehouseList = authContext.Warehouses.Where(x => x.active == true && x.Deleted == false && x.Cityid == cityid).ToList();
                return warehouseList;
            }

        }

        [Route("Cluster/warehouseid/{warehouseid}")]
        [HttpGet]
        public dynamic GetClusterList(int warehouseid)
        {
            using (var authContext = new AuthContext())
            {
                var clusterList = authContext.Clusters.Where(x => x.Active == true && x.Deleted == false && x.WarehouseId == warehouseid).ToList();
                return clusterList;
            }

        }

        [Route("ANScheduleMaster")]
        [HttpGet]
        public dynamic GetANScheduleMasterList()
        {
            using (var authContext = new AuthContext())
            {
                var clusterList = authContext.ANScheduleMaster.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return clusterList;
            }

        }


        [Route("ANFrequencyMasters")]
        public dynamic GetANFrequencyMasters()
        {
            using (var authContext = new AuthContext())
            {
                var frequencyMasterList = authContext.ANFrequencyMaster.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return frequencyMasterList;
            }
        }


        [Route("ANFieldMaster/EntityName/{entityName}")]
        public dynamic GetANFrequencyMasters(string entityName)
        {
            using (var authContext = new AuthContext())
            {
                var query = from fm in authContext.ANFieldMaster
                            join e in authContext.ANEntityMaster
                            on fm.ANEntityMasterId equals e.Id
                            where fm.IsActive == true && fm.IsDeleted == false && e.EntityName == entityName
                            select fm;

                var anFieldMasterList = query.ToList();
                return anFieldMasterList;
            }
        }


        [Route("FieldTypeMaster/FieldType/{fieldType}")]
        public dynamic GetFieldTypeMaster(string fieldType)
        {
            using (var authContext = new AuthContext())
            {
                var operatorMaster = authContext.FieldTypeMaster.Where(x => x.IsActive == true && x.IsDeleted == false && x.FieldType == fieldType).SelectMany(x => x.OperatorFieldMappings.Select(z => z.OperatorMaster)).ToList();

                return operatorMaster;
            }
        }

        [Route("AutoNotification")]
        [HttpPost]
        public AutoNotificationVM SaveAutoNotification(AutoNotificationVM autoNotificationVM)
        {
            int userid = 1;

            DateTime date = DateTime.Now;
            AutoNotificationHelper helper = new AutoNotificationHelper();
            Model.AutoNotification.AutoNotification autoNotification = new Model.AutoNotification.AutoNotification();
            helper.UpdateSendToDetailFiedId(autoNotificationVM);
            autoNotification = helper.ConvertToEntity(autoNotificationVM, true, userid);
            using (var authContext = new AuthContext())
            {
                autoNotification.IsReminder = false;
                autoNotification.IsPublish = false;
                autoNotification.CreatedBy = userid;
                autoNotification.CreatedDate = date;
                autoNotification.ANFrequencyMaster = null;
                autoNotification.ANScheduleMaster = null;
                autoNotification.NotificationTransactions = null;

                if (autoNotificationVM.AutoNotificationConditions != null && autoNotificationVM.AutoNotificationConditions.Any())
                {
                    autoNotification.AutoNotificationConditions = new List<AutoNotificationCondition>();
                    foreach (var condition in autoNotificationVM.AutoNotificationConditions)
                    {
                        AutoNotificationCondition conditionNew = helper.ConvertToAutoNotificationCondition(condition, true, userid);
                        autoNotification.AutoNotificationConditions.Add(conditionNew);
                    }
                }
                autoNotification = authContext.AutoNotification.Add(autoNotification);
                authContext.Commit();
            }
            autoNotificationVM = helper.ConvertToVM(autoNotification, true);
            if (autoNotification != null && autoNotification.AutoNotificationConditions != null && autoNotification.AutoNotificationConditions.Count > 0)
            {
                List<AutoNotificationConditionVM> autoNotificationConditionList = new List<AutoNotificationConditionVM>();
                foreach (var item in autoNotification.AutoNotificationConditions)
                {
                    AutoNotificationConditionVM temp = helper.ConvertToAutoNotificationConditionVM(item, true);
                    autoNotificationConditionList.Add(temp);
                }
            }



            return autoNotificationVM;
        }



        [Route("AutoNotificationCheck")]
        [HttpPost]
        public IHttpActionResult SaveAutoNotificationCheck(AutoNotificationVM autoNotificationVM)
        {
            int userid = 1;

            DateTime date = DateTime.Now;
            AutoNotificationHelper helper = new AutoNotificationHelper();
            Model.AutoNotification.AutoNotification autoNotification = new Model.AutoNotification.AutoNotification();
            helper.UpdateSendToDetailFiedId(autoNotificationVM);
            autoNotification = helper.ConvertToEntity(autoNotificationVM, true, userid);
            using (var authContext = new AuthContext())
            {
                //AutoNotificationVM autoNotificationData = new AutoNotificationVM();
                var AutoNotificationVM = authContext.AutoNotification.Where(c => c.AutoNotificationTitle == autoNotificationVM.AutoNotificationTitle).Count();
                if (AutoNotificationVM == 0)
                {
                    autoNotification.IsReminder = false;
                    autoNotification.IsPublish = false;
                    autoNotification.CreatedBy = userid;
                    autoNotification.CreatedDate = date;
                    autoNotification.ANFrequencyMaster = null;
                    autoNotification.ANScheduleMaster = null;
                    autoNotification.NotificationTransactions = null;

                    if (autoNotificationVM.AutoNotificationConditions != null && autoNotificationVM.AutoNotificationConditions.Any())
                    {
                        autoNotification.AutoNotificationConditions = new List<AutoNotificationCondition>();
                        foreach (var condition in autoNotificationVM.AutoNotificationConditions)
                        {
                            AutoNotificationCondition conditionNew = helper.ConvertToAutoNotificationCondition(condition, true, userid);
                            autoNotification.AutoNotificationConditions.Add(conditionNew);
                        }
                    }
                    autoNotification = authContext.AutoNotification.Add(autoNotification);
                    authContext.Commit();

                    autoNotificationVM = helper.ConvertToVM(autoNotification, true);
                    if (autoNotification != null && autoNotification.AutoNotificationConditions != null && autoNotification.AutoNotificationConditions.Count > 0)
                    {
                        List<AutoNotificationConditionVM> autoNotificationConditionList = new List<AutoNotificationConditionVM>();
                        foreach (var item in autoNotification.AutoNotificationConditions)
                        {
                            AutoNotificationConditionVM temp = helper.ConvertToAutoNotificationConditionVM(item, true);
                            autoNotificationConditionList.Add(temp);
                        }
                        return Ok(autoNotificationVM);
                    }

                }
                else
                {
                    return Ok("Data Already Exist");
                }
                return Ok(autoNotification);
            }
        }


        [Authorize]
        [Route("GetMatch")]
        [HttpGet]
        public dynamic add(string Name)
        {
            using (AuthContext context = new AuthContext())
            {
                var AutoNotification = context.AutoNotification.Where(c => c.AutoNotificationTitle == Name).Select(y => y.AutoNotificationTitle).FirstOrDefault();
                if (AutoNotification == null)
                {

                    return Ok(Name);

                }
                else
                {
                    return null;
                }

            }
        }







        [Route("AutoNotificationList")]
        [HttpGet]
        public List<AutoNotificationListVM> GetAutoNotificationList()
        {
            List<AutoNotificationListVM> list = null;
            using (var authContext = new AuthContext())
            {
                var query = from not in authContext.AutoNotification.Where(x => x.IsActive == true && x.IsDeleted == false)
                            select new AutoNotificationListVM
                            {
                                Id = not.Id,
                                IsDeleted = not.IsDeleted,
                                CreatedBy = not.CreatedBy,
                                CreatedDate = not.CreatedDate,
                                EndDate = not.EndDate,
                                IsActive = not.IsActive,
                                IsPublish = not.IsPublish,
                                ModifiedBy = not.ModifiedBy,
                                ModifiedDate = not.ModifiedDate,
                                PublishBy = not.PublishBy,
                                PublishDate = not.PublishDate,
                                StartDate = not.StartDate,
                                EntityName = not.EntityName,
                                AutoNotificationTitle = not.AutoNotificationTitle
                            };
                list = query.ToList();
            }
            return list;
        }






        [Route("GetIsActiveWise")]
        [HttpGet]
        public dynamic GetIsActiveWise(int? active)
        {
            using (var con = new AuthContext())
            {

                string condition = active.HasValue ? " and a.IsActive=" + active.Value : "";

                string query = "select AutoNotificationTitle,Id, EntityName, StartDate,EndDate,IsPublish, IsActive from AutoNotifications where IsActive = '" + active + "'";



                var count = con.Database.SqlQuery<countNotification>(query).ToList();

                return count;

            }


        }



        [Route("PublishNotification/{autoNotificationID}")]
        [HttpGet]
        public bool PublishNotification(long autoNotificationID)
        {
            int userid = 1;
            DateTime currentTime = DateTime.Now;
            using (var context = new AuthContext())
            {
                var autNotification = context.AutoNotification.Where(x => x.Id == autoNotificationID).Include(x => x.ANFrequencyMaster).Include(x => x.ANScheduleMaster).Include(x => x.AutoNotificationConditions).FirstOrDefault();

                if (autNotification != null)
                {
                    autNotification.IsPublish = true;
                    autNotification.PublishDate = currentTime;
                    autNotification.PublishBy = userid;
                    autNotification.ModifiedBy = userid;
                    autNotification.ModifiedDate = currentTime;
                    context.Commit();

                    if (autNotification.IsActive && !autNotification.IsDeleted && autNotification.IsPublish && autNotification.ANEventType == "Schedule")
                    {
                        BackgroundTaskManager.Run(() => { ScheduleAutoNotification(autNotification); });
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [Route("ActiveInactive/{autoNotificationID}/{isActive}")]
        [HttpGet]
        public bool ActiveInactive(long autoNotificationID, bool isActive)
        {
            int userid = 1;
            DateTime currentTime = DateTime.Now;
            using (var context = new AuthContext())
            {
                var autNotification = context.AutoNotification.FirstOrDefault(x => x.Id == autoNotificationID);

                if (autNotification != null)
                {
                    autNotification.IsActive = isActive;
                    autNotification.ModifiedBy = userid;
                    autNotification.ModifiedDate = currentTime;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        #region Internal Methods
        private void ScheduleAutoNotification(Model.AutoNotification.AutoNotification autoNotification)
        {
            if (autoNotification.ANScheduleMaster.Name == "One Time" && autoNotification.StartDate.HasValue)
            {
                var jobId = BackgroundJob.Schedule(
                                    () => ExecuteAutoNotification(autoNotification),
                          TimeSpan.FromMinutes((autoNotification.StartDate.Value - DateTime.Now).TotalMinutes));
            }
            else if (autoNotification.ANScheduleMaster.Name == "Recurring" && autoNotification.StartDate.HasValue && autoNotification.RecurEvery.HasValue)
            {
                string cronExpression = "";
                switch (autoNotification.ANFrequencyMaster.Name)
                {
                    case "Minute":
                        cronExpression = "0 0/" + autoNotification.RecurEvery + " * 1/1 * ? *";
                        break;
                    case "Hourly":
                        cronExpression = "0 0 0/" + autoNotification.RecurEvery + " 1/1 * ? *";
                        break;
                    case "Daily":
                        cronExpression = "0 " + autoNotification.StartDate.Value.Minute + " " + autoNotification.StartDate.Value.Hour + " 1/" + autoNotification.RecurEvery + " * ? *";
                        break;
                    case "Monthly":
                        cronExpression = "0 " + autoNotification.StartDate.Value.Minute + " " + autoNotification.StartDate.Value.Hour + " 1/" + (autoNotification.RecurEvery * 30) + " * ? *";
                        break;
                    case "Weekly":
                        cronExpression = "0 " + autoNotification.StartDate.Value.Minute + " " + autoNotification.StartDate.Value.Hour + " 1/" + (autoNotification.RecurEvery * 7) + " * ? *";
                        break;
                    case "Yearly":
                        cronExpression = "0 " + autoNotification.StartDate.Value.Minute + " " + autoNotification.StartDate.Value.Hour + " " + autoNotification.StartDate.Value.Day + " " + autoNotification.StartDate.Value.Month + " ? *";
                        break;
                }

                RecurringDateRangeJob.AddOrUpdate(autoNotification.AutoNotificationTitle,
                                              () => ExecuteAutoNotification(autoNotification), cronExpression,
                                              startDate: autoNotification.StartDate,
                                             endDate: autoNotification.EndDate
                            );
            }
        }


        public bool ExecuteAutoNotification(Model.AutoNotification.AutoNotification autoNotification)
        {
            bool result = true;
            var allFieldsTable = new DataTable();
            using (var context = new AuthContext())
            {
                if (context.AutoNotification.Count(x => x.Id == autoNotification.Id && x.IsActive && !x.IsDeleted && x.IsPublish) > 0)
                {
                    if (autoNotification.NotificationTransactions == null)
                        autoNotification.NotificationTransactions = new List<NotificationTransaction>();

                    var detailFieldIds = autoNotification.SendToDetailFiedId;
                    var fieldMasters = context.ANFieldMaster.FirstOrDefault(x => x.Id == autoNotification.SendToDetailFiedId);
                    if (autoNotification.ANType == "Promotional") ///
                    {
                        if (fieldMasters.IsCustomer)
                        {
                            var allfieldquery = "select Mobile,fcmId from customers Where Deleted=0";
                            if (autoNotification.ClusterId.HasValue)
                            {
                                allfieldquery += " And clusterId=" + autoNotification.ClusterId.Value;

                            }
                            else if (autoNotification.WarehouseId.HasValue)
                            {
                                allfieldquery += " And WarehouseId=" + autoNotification.WarehouseId.Value;

                            }
                            else if (autoNotification.CityId.HasValue)
                            {
                                allfieldquery += " And CityId=" + autoNotification.CityId.Value;

                            }
                            var customers = context.Database.SqlQuery<NotificationSendTo>(allfieldquery).ToList();
                            foreach (var customer in customers)
                            {
                                autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                {
                                    FCMId = customer.FcmId,
                                    Mobile = customer.Mobile,
                                    TextMessage = autoNotification.TextMessage,
                                    FCMNotification = autoNotification.FCMNotification,
                                    AutoDialUrl = autoNotification.AutoDialUrl,
                                    CreatedDate = DateTime.Now,
                                    AutoNotificationId = autoNotification.Id
                                });
                            }
                        }
                        else if (fieldMasters.IsPeople)
                        {
                            var allfieldquery = "select Mobile,fcmId from people where Deleted=0";
                            if (autoNotification.WarehouseId.HasValue)
                            {
                                allfieldquery += " And WarehouseId=" + autoNotification.WarehouseId.Value;

                            }
                            else if (autoNotification.CityId.HasValue)
                            {
                                allfieldquery += " And CityId=" + autoNotification.CityId.Value;

                            }
                            var peoples = context.Database.SqlQuery<NotificationSendTo>(allfieldquery).ToList();
                            foreach (var people in peoples)
                            {
                                autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                {
                                    FCMId = people.FcmId,
                                    Mobile = people.Mobile,
                                    TextMessage = autoNotification.TextMessage,
                                    FCMNotification = autoNotification.FCMNotification,
                                    AutoDialUrl = autoNotification.AutoDialUrl,
                                    CreatedDate = DateTime.Now,
                                    AutoNotificationId = autoNotification.Id
                                });
                            }
                        }
                        else if (fieldMasters.IsSupplier)
                        {
                            var allfieldquery = "select MobileNo as Mobile,fcmId from Suppliers where Deleted=0";
                            if (autoNotification.WarehouseId.HasValue)
                            {
                                allfieldquery += " And WarehouseId=" + autoNotification.WarehouseId.Value;

                            }
                            else if (autoNotification.CityId.HasValue)
                            {
                                allfieldquery += " And CityId=" + autoNotification.CityId.Value;

                            }
                            var suppliers = context.Database.SqlQuery<NotificationSendTo>(allfieldquery).ToList();
                            foreach (var supplier in suppliers)
                            {
                                autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                {
                                    FCMId = supplier.FcmId,
                                    Mobile = supplier.Mobile,
                                    TextMessage = autoNotification.TextMessage,
                                    FCMNotification = autoNotification.FCMNotification,
                                    AutoDialUrl = autoNotification.AutoDialUrl,
                                    CreatedDate = DateTime.Now,
                                    AutoNotificationId = autoNotification.Id
                                });
                            }
                        }
                    }
                    else if (autoNotification.ANType == "Event") ///
                    {

                        var conditions = autoNotification.AutoNotificationConditions;
                        var entityMaster = context.ANEntityMaster.FirstOrDefault(x => x.DbObjectName == autoNotification.DbObjectName);
                        var allFields = context.ANFieldMaster.Where(x => x.ANEntityMasterId == entityMaster.Id && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                        var query = "select cast(" + fieldMasters.DbObjectFieldName + " as varchar(500)) from " + autoNotification.DbObjectName + " where ";

                        var allfieldquery = "select " + string.Join(",", allFields) + " from " + autoNotification.DbObjectName + " where ";

                        int i = 0;
                        foreach (var item in conditions)
                        {
                            if (i == 0)
                            {
                                allfieldquery += item.DbObjectFieldName + " " + item.OperatorSign + item.Value1 + (!string.IsNullOrEmpty(item.Value2) ? " and " + item.Value2 : "");
                                query += item.DbObjectFieldName + " " + item.OperatorSign + item.Value1 + (!string.IsNullOrEmpty(item.Value2) ? " and " + item.Value2 : "");
                            }
                            else
                            {
                                allfieldquery += " and " + item.DbObjectFieldName + " " + item.OperatorSign + item.Value1 + (!string.IsNullOrEmpty(item.Value2) ? " and " + item.Value2 : "");
                                query += " and " + item.DbObjectFieldName + " " + item.OperatorSign + item.Value1 + (!string.IsNullOrEmpty(item.Value2) ? " and " + item.Value2 : "");
                            }
                            ++i;
                        }

                        if (autoNotification.ClusterId.HasValue)
                        {
                            allfieldquery += " And clusterId=" + autoNotification.ClusterId.Value;

                        }
                        else if (autoNotification.WarehouseId.HasValue)
                        {
                            allfieldquery += " And WarehouseId=" + autoNotification.WarehouseId.Value;

                        }
                        else if (autoNotification.CityId.HasValue)
                        {
                            allfieldquery += " And CityId=" + autoNotification.CityId.Value;

                        }


                        var queryResult = context.Database.SqlQuery<string>(query).ToList();

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = allfieldquery;
                        cmd.Connection.Open();
                        allFieldsTable.Load(cmd.ExecuteReader());


                        //string path = HttpRuntime.AppDomainAppPath;
                        //string pathToDomain = Path.Combine(path, @"bin");

                        //pathToDomain += "\\" + "AngularJSAuthentication.Model.dll";
                        //Assembly domainAssembly = Assembly.LoadFrom(pathToDomain);

                        //Type entityType = domainAssembly.GetType(autoNotification.ClassName);

                        //var instance = Activator.CreateInstance(entityType);
                        //PropertyInfo[] dbProperties = entityType.GetProperties();

                        Dictionary<string, object> values = new Dictionary<string, object>();
                        for (int j = 0; j < allFieldsTable.Columns.Count; j++)
                        {
                            values.Add(allFieldsTable.Columns[j].ColumnName, allFieldsTable.Rows.Cast<DataRow>().Select(k => k[allFieldsTable.Columns[j]]));
                        }
                        //allFieldsTable.AsEnumerable()
                        //            .ToDictionary<DataRow, string, object>(row => row.Field<string>(0), row => row.Field<object>(1));



                        foreach (var item in queryResult)
                        {

                            if (!string.IsNullOrEmpty(autoNotification.TextMessage))
                            {
                                foreach (var auditField in values)
                                {
                                    if (auditField.Value != null && autoNotification.TextMessage.Contains("[[" + auditField.Key + "]]"))
                                        autoNotification.TextMessage = autoNotification.TextMessage.Replace("[[" + auditField.Key + "]]", auditField.Value.ToString());
                                }

                            }

                            if (!string.IsNullOrEmpty(autoNotification.FCMNotification))
                            {
                                foreach (var auditField in values)
                                {
                                    if (auditField.Value != null && autoNotification.FCMNotification.Contains("[[" + auditField.Key + "]]"))
                                        autoNotification.FCMNotification = autoNotification.FCMNotification.Replace("[[" + auditField.Key + "]]", auditField.Value.ToString());
                                }
                            }
                            if (!string.IsNullOrEmpty(autoNotification.AutoDialUrl))
                            {
                                foreach (var auditField in values)
                                {
                                    if (auditField.Value != null && autoNotification.AutoDialUrl.Contains("[[" + auditField.Key + "]]"))
                                        autoNotification.AutoDialUrl = autoNotification.AutoDialUrl.Replace("[[" + auditField.Key + "]]", auditField.Value.ToString());
                                }
                            }

                            if (fieldMasters.IsCustomer)
                            {
                                var customer = context.Database.SqlQuery<NotificationSendTo>("select Mobile,fcmId from customers Where  customerid = " + item).FirstOrDefault();

                                if (customer != null)
                                {
                                    autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                    {
                                        FCMId = customer.FcmId,
                                        Mobile = customer.Mobile,
                                        TextMessage = autoNotification.TextMessage,
                                        FCMNotification = autoNotification.FCMNotification,
                                        AutoDialUrl = autoNotification.AutoDialUrl,
                                        CreatedDate = DateTime.Now,
                                        AutoNotificationId = autoNotification.Id
                                    });
                                }

                            }
                            else if (fieldMasters.IsPeople)
                            {
                                var people = context.Database.SqlQuery<NotificationSendTo>("select Mobile,fcmId from people where peopleid = " + item).FirstOrDefault();

                                if (people != null)
                                {
                                    autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                    {
                                        FCMId = people.FcmId,
                                        Mobile = people.Mobile,
                                        TextMessage = autoNotification.TextMessage,
                                        FCMNotification = autoNotification.FCMNotification,
                                        AutoDialUrl = autoNotification.AutoDialUrl,
                                        CreatedDate = DateTime.Now,
                                        AutoNotificationId = autoNotification.Id
                                    });
                                }
                            }
                            else if (fieldMasters.IsSupplier)
                            {
                                var supplier = context.Database.SqlQuery<NotificationSendTo>("select MobileNo as Mobile,fcmId from Suppliers where SupplierId = " + item).FirstOrDefault();
                                if (supplier != null)
                                {
                                    autoNotification.NotificationTransactions.Add(new NotificationTransaction
                                    {
                                        FCMId = supplier.FcmId,
                                        Mobile = supplier.Mobile,
                                        TextMessage = autoNotification.TextMessage,
                                        FCMNotification = autoNotification.FCMNotification,
                                        AutoDialUrl = autoNotification.AutoDialUrl,
                                        CreatedDate = DateTime.Now,
                                        AutoNotificationId = autoNotification.Id
                                    });
                                }
                            }
                        }

                    }

                    context.SendNotification(new List<Model.AutoNotification.AutoNotification> { autoNotification });



                }
                else
                {
                    RecurringDateRangeJob.RemoveIfExists(autoNotification.AutoNotificationTitle);
                }
            }
            return result;
        }



        #endregion


        public class AutoNotificationListDTO
        {
            public long Id { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

        }


        public class countNotification
        {
            //public int activelist { get; set; }
            public string AutoNotificationTitle { get; set; }
            public long Id { get; set; }
            public string EntityName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool IsPublish { get; set; }
            public bool IsActive { get; set; }

        }

    }
}
