using AngularJSAuthentication.DataContracts.Transaction.AutoNotification;
using AngularJSAuthentication.Model.AutoNotification;
using System;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class AutoNotificationHelper
    {

        public AutoNotificationVM ConvertToVM(AutoNotification autoNotification, bool isCreated)
        {
            AutoNotificationVM autoNotificationVM = null;
            if (autoNotification != null)
            {
                autoNotificationVM = new AutoNotificationVM();
                autoNotificationVM.ANEventType = autoNotification.ANEventType;
                //autoNotificationVM.ANFrequencyMaster
                autoNotificationVM.ANFrequencyMasterId = autoNotification.ANFrequencyMasterId;
                //autoNotificationVM.ANScheduleMaster
                autoNotificationVM.ANScheduleMasterId = autoNotification.ANScheduleMasterId;
                autoNotificationVM.ANType = autoNotification.ANType;
                autoNotificationVM.AutoDialAudioFile = autoNotification.AutoDialAudioFile;
                autoNotificationVM.AutoDialAudioText = autoNotification.AutoDialAudioText;
                autoNotificationVM.AutoDialUrl = autoNotification.AutoDialUrl;
                //autoNotificationVM.AutoNotificationConditions
                autoNotificationVM.CityId = autoNotification.CityId;
                autoNotificationVM.ClusterId = autoNotification.ClusterId;
                autoNotificationVM.DbObjectName = autoNotification.DbObjectName;
                autoNotificationVM.EndDate = autoNotification.EndDate;
                autoNotificationVM.EntityName = autoNotification.EntityName;
                autoNotificationVM.EntityAction = autoNotification.EntityAction;
                autoNotificationVM.FCMNotification = autoNotification.FCMNotification;
                if (!isCreated)
                {
                    autoNotificationVM.Id = autoNotification.Id;
                }
                autoNotificationVM.IsActive = autoNotification.IsActive;
                autoNotificationVM.IsDeleted = autoNotification.IsDeleted;
                autoNotificationVM.IsPublish = autoNotification.IsPublish;
                autoNotificationVM.IsReminder = autoNotification.IsReminder;
                autoNotificationVM.PublishBy = autoNotification.PublishBy;
                autoNotificationVM.PublishDate = autoNotification.PublishDate;
                autoNotificationVM.RecurEvery = autoNotification.RecurEvery;
                autoNotificationVM.ReminderCount = autoNotification.ReminderCount;
                autoNotificationVM.ReminderDays = autoNotification.ReminderDays;
                autoNotificationVM.SendToDetailFiedId = autoNotification.SendToDetailFiedId;
                autoNotificationVM.SqlQuery = autoNotification.SqlQuery;
                autoNotificationVM.StartDate = autoNotification.StartDate;
                autoNotificationVM.TextMessage = autoNotification.TextMessage;
                autoNotificationVM.WarehouseId = autoNotification.WarehouseId;
                autoNotificationVM.AutoNotificationTitle = autoNotification.AutoNotificationTitle;
            }


            return autoNotificationVM;
        }

        public AutoNotification ConvertToEntity(AutoNotificationVM autoNotificationVM, bool isAddedNew, int userid)
        {
            AutoNotification autoNotification = null;
            if (autoNotificationVM != null)
            {
                autoNotification = new AutoNotification();
                autoNotification.ANEventType = autoNotificationVM.ANEventType;
                //autoNotification.ANFrequencyMaster
                autoNotification.ANFrequencyMasterId = autoNotificationVM.ANFrequencyMasterId;
                //autoNotification.ANScheduleMaster
                autoNotification.ANScheduleMasterId = autoNotificationVM.ANScheduleMasterId;
                autoNotification.ANType = autoNotificationVM.ANType;
                autoNotification.AutoDialAudioFile = autoNotificationVM.AutoDialAudioFile;
                autoNotification.AutoDialAudioText = autoNotificationVM.AutoDialAudioText;
                autoNotification.AutoDialUrl = autoNotificationVM.AutoDialUrl;
                //autoNotification.AutoNotificationConditions
                autoNotification.CityId = autoNotificationVM.CityId;
                autoNotification.ClusterId = autoNotificationVM.ClusterId;
                autoNotification.DbObjectName = autoNotificationVM.DbObjectName;
                autoNotification.EndDate = autoNotificationVM.EndDate;
                autoNotification.EntityName = autoNotificationVM.EntityName;
                autoNotification.FCMNotification = autoNotificationVM.FCMNotification;
                autoNotification.EntityAction = autoNotificationVM.EntityAction;
                if (!isAddedNew)
                {
                    autoNotification.Id = autoNotificationVM.Id;
                    autoNotification.ModifiedBy = userid;
                    autoNotification.ModifiedDate = DateTime.Now;
                }
                else
                {
                    autoNotification.CreatedBy = userid;
                    autoNotification.CreatedDate = DateTime.Now;
                    autoNotification.ModifiedBy = userid;
                    autoNotification.ModifiedDate = DateTime.Now;
                }
                autoNotification.IsActive = autoNotificationVM.IsActive;
                autoNotification.IsDeleted = autoNotificationVM.IsDeleted;
                autoNotification.IsPublish = autoNotificationVM.IsPublish;
                autoNotification.IsReminder = autoNotificationVM.IsReminder;
                autoNotification.PublishBy = autoNotificationVM.PublishBy;
                autoNotification.PublishDate = autoNotificationVM.PublishDate;
                autoNotification.RecurEvery = autoNotificationVM.RecurEvery;
                autoNotification.ReminderCount = autoNotificationVM.ReminderCount;
                autoNotification.ReminderDays = autoNotificationVM.ReminderDays;
                autoNotification.SendToDetailFiedId = autoNotificationVM.SendToDetailFiedId;
                autoNotification.SqlQuery = autoNotificationVM.SqlQuery;
                autoNotification.StartDate = autoNotificationVM.StartDate;
                autoNotification.TextMessage = autoNotificationVM.TextMessage;
                autoNotification.WarehouseId = autoNotificationVM.WarehouseId;
                autoNotification.AutoNotificationTitle = autoNotificationVM.AutoNotificationTitle;
                autoNotification.ClassName = autoNotificationVM.ClassName;
            }


            return autoNotification;
        }

        public AutoNotificationConditionVM ConvertToAutoNotificationConditionVM(AutoNotificationCondition condition, bool isCreated)
        {
            AutoNotificationConditionVM vm = null;
            if (condition != null)
            {
                vm = new AutoNotificationConditionVM();
                vm.AutoNotificationId = condition.AutoNotificationId;
                vm.DbObjectFieldName = condition.DbObjectFieldName;
                vm.FieldName = condition.FieldName;
                vm.FieldType = condition.FieldType;
                vm.OperatorSign = condition.OperatorSign;
                vm.SqlQuery = condition.SqlQuery;
                vm.Value1 = condition.Value1;
                vm.Value2 = condition.Value2;
            }
            return vm;
        }

        public AutoNotificationCondition ConvertToAutoNotificationCondition(AutoNotificationConditionVM conditionVM, bool isCreated, int userID)
        {
            AutoNotificationCondition vm = null;
            if (conditionVM != null)
            {
                vm = new AutoNotificationCondition();
                vm.AutoNotificationId = conditionVM.AutoNotificationId;
                vm.DbObjectFieldName = conditionVM.DbObjectFieldName;
                vm.FieldName = conditionVM.FieldName;
                vm.FieldType = conditionVM.FieldType;
                vm.OperatorSign = conditionVM.OperatorSign;
                vm.SqlQuery = conditionVM.SqlQuery;
                vm.Value1 = conditionVM.Value1;
                vm.Value2 = conditionVM.Value2;

                if (isCreated)
                {
                    vm.CreatedBy = userID;
                    vm.CreatedDate = DateTime.Now;
                }
                else
                {
                    vm.CreatedBy = userID;
                    vm.CreatedDate = DateTime.Now;
                    vm.ModifiedBy = userID;
                    vm.ModifiedDate = DateTime.Now;
                }
                vm.IsActive = true;
                vm.IsDeleted = false;
            }
            return vm;
        }

        public void UpdateSendToDetailFiedId(AutoNotificationVM autoNotificationVM)
        {
            long? id = null;
            using (var authContext = new AuthContext())
            {


                if (autoNotificationVM.IsCustomerNotification == true)
                {
                    var query = from fld in authContext.ANFieldMaster
                                join emas in authContext.ANEntityMaster
                                on fld.ANEntityMasterId equals emas.Id
                                where emas.EntityName == autoNotificationVM.EntityName
                                        && fld.IsCustomer == true
                                select fld.Id;

                    id = query.FirstOrDefault();
                }
                else if (autoNotificationVM.IsSupplierNotification == true)
                {
                    var query = from fld in authContext.ANFieldMaster
                                join emas in authContext.ANEntityMaster
                                on fld.ANEntityMasterId equals emas.Id
                                where emas.EntityName == autoNotificationVM.EntityName
                                        && fld.IsSupplier == true
                                select fld.Id;

                    id = query.FirstOrDefault();
                }
                else
                {
                    var query = from fld in authContext.ANFieldMaster
                                join emas in authContext.ANEntityMaster
                                on fld.ANEntityMasterId equals emas.Id
                                where emas.EntityName == autoNotificationVM.EntityName
                                        && fld.IsPeople == true
                                select fld.Id;

                    id = query.FirstOrDefault();
                }
            }
            if (id.HasValue)
            {
                autoNotificationVM.SendToDetailFiedId = id.Value;
            }

        }
    }
}