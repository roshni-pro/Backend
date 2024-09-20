using AngularJSAuthentication.Accounts.Managers;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.AutoNotification;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.AutoNotification;
using AngularJSAuthentication.Model.Base;
using AngularJSAuthentication.Model.Base.Audit;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.API.Helper.Stock;
using System.Data;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.BatchManager;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API
{
    public partial class AuthContext : IdentityDbContext<IdentityUser>, iAuthContext
    {
        //nlogger

        List<MakerChecker> makerCheckerList = new List<MakerChecker>();
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private List<Audit> auditEntityList;
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        internal readonly IEnumerable<object> CaseModules;
        public double xPointValue = 0.02; //AppConstants.xPoint;
        readonly string itemMqName = "ItemElasticItemId";
        readonly string itemLimitMqName = "ItemElasticItemMultiMrpId";
        //public static List<int> retailercustomerIds = new List<int>();


        public AuthContext() : base("AuthContext")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;

        }
        public static AuthContext Create()
        {
            return new AuthContext();
        }

        //public override int SaveChanges()
        //{
        //    throw new Exception("Not in use. Please use Commit() method");
        //}

        //public override async Task<int> SaveChangesAsync()
        //{
        //    throw new Exception("Not in use. Please use Commit() method");
        //}

        public int Commit(bool? isApproveFromChecker = false, bool? doNotMakerChecker = true, StockEnum? stockEnum = null)
        {

            var MkChkentityList = new List<DbEntityEntry>();
            var entityList = new List<DbEntityEntry>();
            var addedEntities = new List<DbEntityEntry>();
            bool isError = false;

            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";

            if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "System" || loggedInUser == "RetailerApp" || loggedInUser == "SalesApp" || loggedInUser == "DeliveryApp")
                && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            {
                loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());
            }


            #region Audit

            try
            {
                var baseModelList = ChangeTracker.Entries<BaseModel>().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified).Select(dbEntityEntry => dbEntityEntry.Entity);

                foreach (var item in baseModelList)
                {
                    item.GUID = Guid.NewGuid();
                }

                entityList = ChangeTracker.Entries().Where(p => (p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified) && p.Entity.GetType().IsSubclassOf(typeof(BaseModel))).ToList();

                auditEntityList = FillAuditList(entityList, loggedInUser);
            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting first part of Audit Info: ").Append(ex.ToString()).ToString());
                isError = true;
            }


            #endregion

            var saveChanges = doNotMakerChecker.HasValue && doNotMakerChecker.Value;
            #region MakerChecker

            var makerCheckerMasters = new List<MakerCheckerMaster>();
            if (!saveChanges && (!isApproveFromChecker.HasValue || !isApproveFromChecker.Value))
            {
                makerCheckerMasters = GetMakerCheckerMasters();
                MkChkentityList = ChangeTracker.Entries().Where(p => (p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified)).ToList();

                if (makerCheckerMasters != null && makerCheckerMasters.Any() && MkChkentityList.Any(x => makerCheckerMasters.Select(z => z.EntityName).Contains(x.Entity.GetType().Name)))
                    saveChanges = false;
                else
                    saveChanges = true;

            }
            #endregion

            int result = 0;//

            if (saveChanges)
            {

                //TextFileLogHelper.LogError(JsonConvert.SerializeObject(activeNotifications), false);
                result = base.SaveChanges();
                if (stockEnum.HasValue)
                {
                    StockContextHelper stockContextHelper = new StockContextHelper();
                    bool stockEntriesResult = stockContextHelper.MakeEntries(auditEntityList, stockEnum.Value, this);
                    if (!stockEntriesResult)
                    {
                        return 0;
                    }
                }


                try
                {
                    if (auditEntityList != null && auditEntityList.Any(x => x.AuditAction == EntityState.Added.ToString()) && !isError)
                    {
                        auditEntityList.Where(x => x.AuditAction == EntityState.Added.ToString()).ToList().ForEach(x =>
                        {
                            var entry = entityList.FirstOrDefault(z => z.Entity.GetType().Name == x.AuditEntity && x.GUID == Convert.ToString(z.Entity.GetType().GetProperty("GUID").GetValue(z.Entity, null)));
                            var primaryKey = Convert.ToString(GetPrimaryKeyValue(entry));
                            var pkName = GetPrimaryKeyName(entry);

                            x.PkFieldName = pkName;
                            x.PkValue = primaryKey;

                            x.AuditFields.Where(z => z.FieldName == pkName).ToList().ForEach(z => z.NewValue = primaryKey);

                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(new StringBuilder("Error while getting second part of Audit Info : ").Append(ex.ToString()).ToString());
                    isError = true;
                }

                #region Elastic Search
                try
                {
                    if (result > 0 && auditEntityList.Any(x => x.AuditEntity == "ItemMaster"))
                    {
                        List<ItemIdCls> itemIds = auditEntityList.Where(x => x.AuditEntity == "ItemMaster").Select(x => new ItemIdCls { ItemId = Convert.ToInt32(x.PkValue) }).ToList();

                        foreach (var itemId in itemIds)
                        {
                            RabbitMqHelperNew rabbitMqHelper = new RabbitMqHelperNew();
                            rabbitMqHelper.Publish(itemMqName, itemId);
                        }

                        //DataTable dt = new DataTable();
                        //dt.Columns.Add("IntValue");
                        //foreach (var item in itemIds)
                        //{
                        //    var dr = dt.NewRow();
                        //    dr["IntValue"] = item;
                        //    dt.Rows.Add(dr);
                        //}
                        //var param = new SqlParameter("itemIds", dt);
                        //param.SqlDbType = SqlDbType.Structured;
                        //param.TypeName = "dbo.IntValues";
                        //var elasticSearchItems = this.Database.SqlQuery<DataContracts.ElasticSearch.ElasticSearchItem>("Exec GetItemForElasticSearch @itemIds", param).ToList();
                        //ElasticSearchHelper elasticHelper = new ElasticSearchHelper();
                        //elasticHelper.AddElasticProduct(elasticSearchItems);
                    }
                    if (result > 0 && auditEntityList.Any(x => x.AuditEntity == "ItemLimitMaster"))
                    {

                        var itemIds = auditEntityList.Where(x => x.AuditEntity == "ItemLimitMaster").ToList();

                        foreach (var item in itemIds)
                        {
                            var mrpId = new MultiMrpIdCls
                            {
                                itemmultimrpid = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "ItemMultiMRPId").NewValue),
                                warehouseid = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "WarehouseId").NewValue),
                                IsFreeItem = false
                            };

                            RabbitMqHelperNew rabbitMqHelper = new RabbitMqHelperNew();
                            rabbitMqHelper.Publish(itemLimitMqName, mrpId);

                        }


                        //DataTable dt = new DataTable();
                        //dt.Columns.Add("ItemMultiMRPId");
                        //dt.Columns.Add("WarehouseId");
                        //dt.Columns.Add("IsFreeItem");
                        //foreach (var item in itemIds)
                        //{
                        //    var dr = dt.NewRow();
                        //    dr["ItemMultiMRPId"] = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "ItemMultiMRPId").NewValue);
                        //    dr["WarehouseId"] = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "WarehouseId").NewValue);
                        //    dr["IsFreeItem"] = false;
                        //    dt.Rows.Add(dr);
                        //}
                        //var param = new SqlParameter("itemIds", dt);
                        //param.SqlDbType = SqlDbType.Structured;
                        //param.TypeName = "dbo.itemtype";
                        //var elasticSearchItems = this.Database.SqlQuery<DataContracts.ElasticSearch.ElasticSearchItem>("Exec GetItemForElasticSearchWithMRPId @itemIds", param).ToList();
                        //ElasticSearchHelper elasticHelper = new ElasticSearchHelper();
                        //elasticHelper.AddElasticProduct(elasticSearchItems);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(new StringBuilder("Error while updateing item on Elastic search : ").Append(ex.ToString()).ToString());
                }
                #endregion

                var activeNotifications = PrepareAutoNotification(auditEntityList);

                if (result > 0 && activeNotifications != null && activeNotifications.Any())
                {
                    TextFileLogHelper.LogError("result is > 0 and notifications is ANY", false);

                    BackgroundTaskManager.Run(() =>
                    {
                        SendNotification(activeNotifications);
                    });
                }

                if (!isError && auditEntityList != null && auditEntityList.Any(x => x.AuditFields.Any(z => (z.FieldName != "UpdatedDate" && z.FieldName != "UpdateDate" && z.FieldName != "UpdateBy") && z.NewValue != z.OldValue)))
                {
                    BackgroundTaskManager.Run(() =>
                    {
                        AuditDbHelper helper = new AuditDbHelper();
                        helper.InsertAuditLogs(auditEntityList);

                    });

                    var fetchFromMongo = ConfigurationManager.AppSettings["OrdersFetchAndStoreInMongo"];

                    if (fetchFromMongo == "true" && auditEntityList.Any(x => x.AuditEntity == "OrderMaster"))
                    {
                        //var order = auditEntityList.Where(x => x.AuditEntity == "OrderMaster").Select(x => new OrderUpdateQueue
                        //{
                        //    OrderId = Convert.ToInt32(x.PkValue),
                        //    Status = x.AuditFields.FirstOrDefault(z => z.FieldName == "Status").NewValue,
                        //    UpdateDate = indianTime
                        //}).ToList();

                        MongoDbHelper<OrdersToSync> mongoDbHelper = new MongoDbHelper<OrdersToSync>();

                        var order = auditEntityList.Where(x => x.AuditEntity == "OrderMaster").Select(x => new OrdersToSync
                        {
                            OrderId = Convert.ToInt32(x.PkValue),
                            NewStatus = x.AuditFields.FirstOrDefault(z => z.FieldName == "Status").NewValue,
                            CreateOrUpdateDate = indianTime,
                            IsProcessed = false,
                        }).ToList();

                        mongoDbHelper.InsertMany(order);

                        //OrderMasterChangeDetectManager orderMasterChangeDetectManager = new OrderMasterChangeDetectManager();
                        //try
                        //{
                        //    order.ForEach(x =>
                        //    {
                        //        var jobId = BackgroundJob.Schedule(() => orderMasterChangeDetectManager.UpdateOrderInMongo(x), TimeSpan.FromMilliseconds(150));
                        //    });
                        //}
                        //catch { }

                        //BackgroundTaskManager.Run(() =>
                        //{
                        //    RabbitMqHelper helper = new RabbitMqHelper();
                        //    order.ForEach(x =>
                        //    {
                        //        helper.Publish(x);
                        //    });
                        //});

                    }

                }
            }
            else
            {
                foreach (var item in MkChkentityList)
                {
                    if (makerCheckerMasters.Any(x => x.EntityName == item.Entity.GetType().Name))
                    {

                        var primaryKey = item.State != EntityState.Added ? Convert.ToString(GetPrimaryKeyValue(item)) : string.Empty;
                        var makerChecker = new MakerChecker
                        {
                            MakerBy = loggedInUser,
                            EntityId = primaryKey,
                            EntityName = item.Entity.GetType().Name,
                            MakerDate = indianTime,
                            NewJson = item.State != EntityState.Deleted ? JsonConvert.SerializeObject(item.CurrentValues.ToObject()) : string.Empty,
                            OldJson = item.State != EntityState.Added ? JsonConvert.SerializeObject(item.OriginalValues.ToObject()) : string.Empty,
                            Operation = item.State.ToString(),
                            Status = "Pending"
                        };

                        makerCheckerList.Add(makerChecker);
                    }

                }
                MongoDbHelper<MakerChecker> mongoDbHelper = new MongoDbHelper<MakerChecker>();

                mongoDbHelper.InsertMany(makerCheckerList);

            }


            #region ledger work
            //if (auditEntityList != null && auditEntityList.Any())
            //{
            //    LedgerComposerManager ledgerComposerManager = new LedgerComposerManager();
            //    //string data = JsonConvert.SerializeObject(auditEntityList);


            //    BackgroundJob.Enqueue(() => ledgerComposerManager.CheckAndCreateLedger(auditEntityList));
            //}
            if (auditEntityList != null && auditEntityList.Any())
            {


                LedgerComposerManager ledgerComposerManager = new LedgerComposerManager();

                List<Audit> auditGuidList = ledgerComposerManager.CheckLedgerMasterOnline(auditEntityList);

                if (auditGuidList != null && auditGuidList.Any())
                {
                    ledgerComposerManager.CreateLedgerOnline(auditGuidList);
                }

            }
            #endregion ledger work ended

            return result;
        }

        public async Task<int> CommitAsync(bool? isApproveFromChecker = false, bool? doNotMakerChecker = true)
        {

            var MkChkentityList = new List<DbEntityEntry>();
            var entityList = new List<DbEntityEntry>();
            var addedEntities = new List<DbEntityEntry>();
            bool isError = false;

            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";

            if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "System" || loggedInUser == "RetailerApp" || loggedInUser == "SalesApp" || loggedInUser == "DeliveryApp")
                && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            {
                loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());
            }


            #region Audit

            try
            {
                var baseModelList = ChangeTracker.Entries<BaseModel>().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified).Select(dbEntityEntry => dbEntityEntry.Entity);

                foreach (var item in baseModelList)
                {
                    item.GUID = Guid.NewGuid();
                }

                entityList = ChangeTracker.Entries().Where(p => (p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified) && p.Entity.GetType().IsSubclassOf(typeof(BaseModel))).ToList();

                auditEntityList = FillAuditList(entityList, loggedInUser);
            }
            catch (Exception ex)
            {
                logger.Error(new StringBuilder("Error while getting first part of Audit Info: ").Append(ex.ToString()).ToString());
                isError = true;
            }


            #endregion

            var saveChanges = doNotMakerChecker.HasValue && doNotMakerChecker.Value;
            #region MakerChecker

            var makerCheckerMasters = new List<MakerCheckerMaster>();
            if (!saveChanges && (!isApproveFromChecker.HasValue || !isApproveFromChecker.Value))
            {
                makerCheckerMasters = GetMakerCheckerMasters();
                MkChkentityList = ChangeTracker.Entries().Where(p => (p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified)).ToList();

                if (makerCheckerMasters != null && makerCheckerMasters.Any() && MkChkentityList.Any(x => makerCheckerMasters.Select(z => z.EntityName).Contains(x.Entity.GetType().Name)))
                    saveChanges = false;
                else
                    saveChanges = true;


            }
            #endregion

            int result = 0;//

            if (saveChanges)
            {

                //TextFileLogHelper.LogError(JsonConvert.SerializeObject(activeNotifications), false);
                result = await base.SaveChangesAsync();

                #region Elastic Search
                try
                {
                    if (result > 0 && auditEntityList.Any(x => x.AuditEntity == "ItemMaster"))
                    {
                        List<ItemIdCls> itemIds = auditEntityList.Where(x => x.AuditEntity == "ItemMaster").Select(x => new ItemIdCls { ItemId = Convert.ToInt32(x.PkValue) }).ToList();

                        foreach (var itemId in itemIds)
                        {
                            RabbitMqHelperNew rabbitMqHelper = new RabbitMqHelperNew();
                            rabbitMqHelper.Publish(itemMqName, itemId);
                        }
                    }
                    if (result > 0 && auditEntityList.Any(x => x.AuditEntity == "ItemLimitMaster"))
                    {

                        var itemIds = auditEntityList.Where(x => x.AuditEntity == "ItemLimitMaster").ToList();

                        foreach (var item in itemIds)
                        {
                            var mrpId = new MultiMrpIdCls
                            {
                                itemmultimrpid = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "ItemMultiMRPId").NewValue),
                                warehouseid = Convert.ToInt32(item.AuditFields.FirstOrDefault(x => x.FieldName == "WarehouseId").NewValue),
                                IsFreeItem = false
                            };

                            RabbitMqHelperNew rabbitMqHelper = new RabbitMqHelperNew();
                            rabbitMqHelper.Publish(itemLimitMqName, mrpId);

                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(new StringBuilder("Error while updateing item on Elastic search : ").Append(ex.ToString()).ToString());
                }
                #endregion

                try
                {
                    if (auditEntityList != null && auditEntityList.Any(x => x.AuditAction == EntityState.Added.ToString()) && !isError)
                    {
                        auditEntityList.Where(x => x.AuditAction == EntityState.Added.ToString()).ToList().ForEach(x =>
                        {
                            var entry = entityList.FirstOrDefault(z => z.Entity.GetType().Name == x.AuditEntity && x.GUID == Convert.ToString(z.Entity.GetType().GetProperty("GUID").GetValue(z.Entity, null)));
                            var primaryKey = Convert.ToString(GetPrimaryKeyValue(entry));
                            var pkName = GetPrimaryKeyName(entry);

                            x.PkFieldName = pkName;
                            x.PkValue = primaryKey;

                            x.AuditFields.Where(z => z.FieldName == pkName).ToList().ForEach(z => z.NewValue = primaryKey);

                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(new StringBuilder("Error while getting second part of Audit Info : ").Append(ex.ToString()).ToString());
                    isError = true;
                }

                var activeNotifications = PrepareAutoNotification(auditEntityList);

                if (result > 0 && activeNotifications != null && activeNotifications.Any())
                {
                    TextFileLogHelper.LogError("result is > 0 and notifications is ANY", false);

                    BackgroundTaskManager.Run(() =>
                    {
                        SendNotification(activeNotifications);
                    });
                }

                if (!isError && auditEntityList != null && auditEntityList.Any(x => x.AuditFields.Any(z => (z.FieldName != "UpdatedDate" && z.FieldName != "UpdateDate" && z.FieldName != "UpdateBy") && z.NewValue != z.OldValue)))
                {
                    BackgroundTaskManager.Run(() =>
                    {
                        AuditDbHelper helper = new AuditDbHelper();
                        helper.InsertAuditLogs(auditEntityList);

                    });

                    var fetchFromMongo = ConfigurationManager.AppSettings["OrdersFetchAndStoreInMongo"];

                    if (fetchFromMongo == "true" && auditEntityList.Any(x => x.AuditEntity == "OrderMaster"))
                    {


                        MongoDbHelper<OrdersToSync> mongoDbHelper = new MongoDbHelper<OrdersToSync>();

                        var order = auditEntityList.Where(x => x.AuditEntity == "OrderMaster").Select(x => new OrdersToSync
                        {
                            OrderId = Convert.ToInt32(x.PkValue),
                            NewStatus = x.AuditFields.FirstOrDefault(z => z.FieldName == "Status").NewValue,
                            CreateOrUpdateDate = indianTime,
                            IsProcessed = false,
                        }).ToList();

                        await mongoDbHelper.InsertManyAsync(order);
                        //var order = auditEntityList.Where(x => x.AuditEntity == "OrderMaster").Select(x => new OrderUpdateQueue
                        //{
                        //    OrderId = Convert.ToInt32(x.PkValue),
                        //    Status = x.AuditFields.FirstOrDefault(z => z.FieldName == "Status").NewValue,
                        //    UpdateDate = indianTime
                        //}).ToList();


                        //OrderMasterChangeDetectManager orderMasterChangeDetectManager = new OrderMasterChangeDetectManager();
                        //try
                        //{
                        //    order.ForEach(x =>
                        //    {
                        //        var jobId = BackgroundJob.Schedule(() => orderMasterChangeDetectManager.UpdateOrderInMongo(x), TimeSpan.FromMilliseconds(150));
                        //    });
                        //}
                        //catch { }

                        //BackgroundTaskManager.Run(() =>
                        //{
                        //    RabbitMqHelper helper = new RabbitMqHelper();
                        //    order.ForEach(x =>
                        //    {
                        //        helper.Publish(x);
                        //    });
                        //});

                    }

                }
            }
            else
            {
                foreach (var item in MkChkentityList)
                {
                    if (makerCheckerMasters.Any(x => x.EntityName == item.Entity.GetType().Name))
                    {

                        var primaryKey = item.State != EntityState.Added ? Convert.ToString(GetPrimaryKeyValue(item)) : string.Empty;
                        var makerChecker = new MakerChecker
                        {
                            MakerBy = loggedInUser,
                            EntityId = primaryKey,
                            EntityName = item.Entity.GetType().Name,
                            MakerDate = indianTime,
                            NewJson = item.State != EntityState.Deleted ? JsonConvert.SerializeObject(item.CurrentValues.ToObject()) : string.Empty,
                            OldJson = item.State != EntityState.Added ? JsonConvert.SerializeObject(item.OriginalValues.ToObject()) : string.Empty,
                            Operation = item.State.ToString(),
                            Status = "Pending"
                        };

                        makerCheckerList.Add(makerChecker);
                    }

                }
                MongoDbHelper<MakerChecker> mongoDbHelper = new MongoDbHelper<MakerChecker>();

                await mongoDbHelper.InsertManyAsync(makerCheckerList);

            }


            #region ledger work
            if (auditEntityList != null && auditEntityList.Any())
            {
                LedgerComposerManager ledgerComposerManager = new LedgerComposerManager();
                //List<AuditVM> auditGuidList = ledgerComposerManager.CheckLedgerMaster(auditEntityList);
                List<Audit> auditGuidList = ledgerComposerManager.CheckLedgerMasterOnline(auditEntityList);
                //EnqueuedState myQueueState = new Hangfire.States.EnqueuedState("ledger");
                //ledgerComposerManager.CreateLedger(auditGuidList);
                if (auditGuidList != null && auditGuidList.Any())
                {
                    ledgerComposerManager.CreateLedgerOnline(auditGuidList);
                    //new BackgroundJobClient().Create<LedgerComposerManager>(l => l.CreateLedger(auditGuidList), myQueueState);
                    //BackgroundJob.Schedule(() => ledgerComposerManager.CreateLedger(auditGuidList), TimeSpan.FromSeconds(1));
                    //, new EnqueuedState("queueName")
                }

            }
            #endregion ledger work ended

            return result;
        }

        #region Auto Notification
        public List<AutoNotification> PrepareAutoNotification(List<Audit> entity)
        {

            //var pkSqlQuery = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE " +
            //    " OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1 AND TABLE_NAME = '[[EntityName]]' ";
            try
            {
                var activeNotifications = new List<AutoNotification>();
                List<string> entityName = entity.Select(x => x.AuditEntity).ToList();

                var entityMasters = ANEntityMaster.Where(x => entityName.Contains(x.EntityName)).ToList();

                var AutoNotifications = AutoNotification.Where(x => x.IsActive && !x.IsDeleted && x.IsPublish && entityName.Contains(x.EntityName) && x.ANType == "Event" && x.ANEventType == "Transaction").Include(x => x.AutoNotificationConditions).ToList();
                if (AutoNotifications != null && AutoNotifications.Any())
                {
                    foreach (var item in entity)
                    {

                        //var entityPkSqlQuery = pkSqlQuery.Replace("[[EntityName]]", item.TableName).Replace("dbo.", "").Replace("[", "").Replace("]", "");
                        //var pkColumnName = this.Database.SqlQuery<string>(entityPkSqlQuery).FirstOrDefault();
                        if (item.AuditAction == EntityState.Added.ToString()
                            && AutoNotifications.Any(x => x.EntityName == item.AuditEntity && x.EntityAction == "A"))
                        {
                            var notifications = AutoNotifications.Where(x => x.EntityName == item.AuditEntity && (x.EntityAction == "A")).ToList();
                            foreach (var notification in notifications)
                            {
                                bool sendNotificationToThisRecord = true;
                                if (notification.ClusterId.HasValue && notification.ClusterId.Value > 0)
                                {
                                    var clusterId = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "clusterid")?.NewValue;

                                    if (string.IsNullOrEmpty(clusterId) || Convert.ToInt32(clusterId) != notification.ClusterId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }

                                }
                                else if (notification.WarehouseId.HasValue && notification.WarehouseId.Value > 0)
                                {
                                    var warehouseid = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "warehouseid")?.NewValue;

                                    if (string.IsNullOrEmpty(warehouseid) || Convert.ToInt32(warehouseid) != notification.WarehouseId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }
                                }
                                else if (notification.CityId.HasValue && notification.CityId.Value > 0)
                                {
                                    var cityId = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "cityid")?.NewValue;

                                    if (string.IsNullOrEmpty(cityId) || Convert.ToInt32(cityId) != notification.CityId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }
                                }

                                var condition = notification.AutoNotificationConditions.FirstOrDefault(x => item.AuditFields.Select(s => s.FieldName).Contains(x.DbObjectFieldName));

                                if (sendNotificationToThisRecord && condition != null && item.AuditFields.Any(x => x.FieldName == condition.DbObjectFieldName && x.NewValue != x.OldValue))
                                {
                                    var notificationNew = GetNotification(notification, condition, item);

                                    if (notificationNew != null && !activeNotifications.Any(z => z.Id == notificationNew.Id))
                                        activeNotifications.Add(notificationNew);
                                }
                                else if (sendNotificationToThisRecord && condition == null)
                                    activeNotifications.Add(notification);
                            }
                        }
                        else if (item.AuditAction == EntityState.Modified.ToString() && AutoNotifications.Any(x => x.EntityName == item.AuditEntity && (x.EntityAction == "E" || x.EntityAction == "D") && x.AutoNotificationConditions.Any(z => item.AuditFields.Select(s => s.FieldName).Contains(z.DbObjectFieldName))))
                        {
                            var notifications = AutoNotifications.Where(x => x.EntityName == item.AuditEntity && x.AutoNotificationConditions.Any(z => item.AuditFields.Select(s => s.FieldName).Contains(z.DbObjectFieldName))).ToList();
                            foreach (var notification in notifications)
                            {
                                bool sendNotificationToThisRecord = true;
                                if (notification.ClusterId.HasValue)
                                {
                                    var clusterId = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "clusterid")?.NewValue;

                                    if (string.IsNullOrEmpty(clusterId) || Convert.ToInt32(clusterId) != notification.ClusterId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }

                                }
                                else if (notification.WarehouseId.HasValue)
                                {
                                    var warehouseid = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "warehouseid")?.NewValue;

                                    if (string.IsNullOrEmpty(warehouseid) || Convert.ToInt32(warehouseid) != notification.WarehouseId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }
                                }
                                else if (notification.CityId.HasValue)
                                {
                                    var cityId = item.AuditFields.FirstOrDefault(x => x.FieldName.ToLower() == "cityid")?.NewValue;

                                    if (string.IsNullOrEmpty(cityId) || Convert.ToInt32(cityId) != notification.CityId.Value)
                                    {
                                        sendNotificationToThisRecord = false;
                                    }
                                }
                                var condition = notification.AutoNotificationConditions.FirstOrDefault(x => item.AuditFields.Select(s => s.FieldName).Contains(x.DbObjectFieldName));
                                if (sendNotificationToThisRecord && condition != null && item.AuditFields.Any(x => x.FieldName == condition.DbObjectFieldName && x.NewValue != x.OldValue))
                                {
                                    var notificationNew = GetNotification(notification, condition, item);

                                    if (notificationNew != null && !activeNotifications.Any(z => z.Id == notificationNew.Id))
                                        activeNotifications.Add(notification);
                                }
                            }
                        }

                        if (activeNotifications != null && activeNotifications.Any()
                            && ((item.AuditAction == EntityState.Added.ToString()
                                && AutoNotifications.Any(x => x.EntityName == item.AuditEntity && x.EntityAction == "A"
                                                            && (!x.AutoNotificationConditions.Any()
                                                            || x.AutoNotificationConditions.Any(z => item.AuditFields.Select(s => s.FieldName).Contains(z.DbObjectFieldName)))
                                ))
                                || (item.AuditAction == EntityState.Modified.ToString() && AutoNotifications.Any(x => x.EntityName == item.AuditEntity && (x.EntityAction == "E" || x.EntityAction == "D")
                                    && x.AutoNotificationConditions.Any(z => item.AuditFields.Select(s => s.FieldName).Contains(z.DbObjectFieldName))))
                            ))
                        {
                            PrepareNotificationTransaction(activeNotifications, item);
                        }
                    }
                }
                return activeNotifications;
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError("Error in PrepareAutoNotification: " + ex.ToString());
            }
            return null;
        }

        private AutoNotification GetNotification(AutoNotification notification, AutoNotificationCondition condition, Audit item)
        {
            var field = item.AuditFields.FirstOrDefault(x => x.FieldName == condition.DbObjectFieldName && x.NewValue != x.OldValue);

            switch (condition.OperatorSign)
            {
                case "=":
                    if (field.NewValue == condition.Value1)
                    {
                        return notification;
                    }
                    break;

                case "<":
                    if (ConvertValue(field.NewValue, condition.FieldType) < ConvertValue(condition.Value1, condition.FieldType))
                    {
                        return notification;
                    }
                    break;

                case "<=":
                    if (ConvertValue(field.NewValue, condition.FieldType) <= ConvertValue(condition.Value1, condition.FieldType))
                    {
                        return notification;
                    }
                    break;

                case ">":
                    if (ConvertValue(field.NewValue, condition.FieldType) > ConvertValue(condition.Value1, condition.FieldType))
                    {
                        return notification;
                    }
                    break;

                case ">=":
                    if (ConvertValue(field.NewValue, condition.FieldType) >= ConvertValue(condition.Value1, condition.FieldType))
                    {
                        return notification;
                    }
                    break;

                case "!=":
                    if (ConvertValue(field.NewValue, condition.FieldType) != ConvertValue(condition.Value1, condition.FieldType))
                    {
                        return notification;
                    }
                    break;

                case "between":
                    if (ConvertValue(field.NewValue, condition.FieldType) >= ConvertValue(condition.Value1, condition.FieldType) && ConvertValue(field.NewValue, condition.FieldType) <= ConvertValue(condition.Value2, condition.FieldType)
                       )
                    {
                        return notification;
                    }
                    break;
            }

            return null;

        }

        private void PrepareNotificationTransaction(List<AutoNotification> activeNotifications, Audit item)
        {
            var detailFieldIds = activeNotifications.Select(x => x.SendToDetailFiedId).ToList();
            var fieldMasters = ANFieldMaster.Where(x => detailFieldIds.Contains(x.Id));


            foreach (var activeNotification in activeNotifications)
            {
                if (activeNotification.NotificationTransactions == null)
                    activeNotification.NotificationTransactions = new List<NotificationTransaction>();

                var notificationTransaction = new NotificationTransaction();

                var field = fieldMasters.FirstOrDefault(x => x.Id == activeNotification.SendToDetailFiedId);
                var fieldValue = item.AuditFields.FirstOrDefault(x => x.FieldName == field.DbObjectFieldName).NewValue;

                if (!string.IsNullOrEmpty(field.SqlQuery) && string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(field.SqlParams))
                {
                    var fieldQuery = field.SqlQuery;
                    field.SqlParams.Split(',').ToList().ForEach(x =>
                    {
                        fieldQuery = fieldQuery.Replace(x, item.AuditFields.FirstOrDefault(z => z.FieldName.ToLower() == x.Replace("@", "").ToLower()).NewValue);
                    });

                    fieldValue = this.Database.SqlQuery<string>(fieldQuery).FirstOrDefault();
                }

                if (field.IsCustomer && !string.IsNullOrEmpty(fieldValue))
                {
                    var customer = this.Database.SqlQuery<NotificationSendTo>("select Mobile,fcmId from customers where customerid = " + fieldValue).FirstOrDefault();
                    if (customer != null)
                    {
                        notificationTransaction.FCMId = customer.FcmId;
                        notificationTransaction.Mobile = customer.Mobile;
                    }
                }
                else if (field.IsPeople && !string.IsNullOrEmpty(fieldValue))
                {
                    var people = this.Database.SqlQuery<NotificationSendTo>("select Mobile,fcmId from people where peopleid = " + fieldValue).FirstOrDefault();
                    if (people != null)
                    {
                        notificationTransaction.FCMId = people.FcmId;
                        notificationTransaction.Mobile = people.Mobile;
                    }
                }
                else if (field.IsSupplier && !string.IsNullOrEmpty(fieldValue))
                {
                    var supplier = this.Database.SqlQuery<NotificationSendTo>("select MobileNo as Mobile,fcmId from Suppliers where SupplierId = " + fieldValue).FirstOrDefault();
                    if (supplier != null)
                    {
                        notificationTransaction.FCMId = supplier.FcmId;
                        notificationTransaction.Mobile = supplier.Mobile;
                    }
                }


                if (!string.IsNullOrEmpty(activeNotification.TextMessage))
                {
                    foreach (var auditField in item.AuditFields)
                    {
                        if (!string.IsNullOrEmpty(auditField.NewValue) && activeNotification.TextMessage.Contains("[[" + auditField.FieldName + "]]"))
                            activeNotification.TextMessage = activeNotification.TextMessage.Replace("[[" + auditField.FieldName + "]]", auditField.NewValue);
                    }

                }

                if (!string.IsNullOrEmpty(activeNotification.FCMNotification))
                {
                    foreach (var auditField in item.AuditFields)
                    {
                        if (!string.IsNullOrEmpty(auditField.NewValue) && activeNotification.FCMNotification.Contains("[[" + auditField.FieldName + "]]"))
                            activeNotification.FCMNotification = activeNotification.FCMNotification.Replace("[[" + auditField.FieldName + "]]", auditField.NewValue);
                    }
                }
                if (!string.IsNullOrEmpty(activeNotification.AutoDialUrl))
                {
                    foreach (var auditField in item.AuditFields)
                    {
                        if (!string.IsNullOrEmpty(auditField.NewValue) && activeNotification.AutoDialUrl.Contains("[[" + auditField.FieldName + "]]"))
                            activeNotification.AutoDialUrl = activeNotification.AutoDialUrl.Replace("[[" + auditField.FieldName + "]]", auditField.NewValue);
                    }
                }

                notificationTransaction.TextMessage = activeNotification.TextMessage;
                notificationTransaction.AutoDialUrl = activeNotification.AutoDialUrl;
                notificationTransaction.FCMNotification = activeNotification.FCMNotification;
                notificationTransaction.AutoNotificationId = activeNotification.Id;
                notificationTransaction.CreatedDate = indianTime;
                activeNotification.NotificationTransactions.Add(notificationTransaction);
                this.Entry(activeNotification).State = EntityState.Unchanged;
            }
        }

        private dynamic ConvertValue(dynamic value, string type)
        {
            switch (type)
            {
                case "int16":
                    return Convert.ToInt16(value);
                case "int32":
                    return Convert.ToInt32(value);
                case "int64":
                    return Convert.ToInt64(value);
                case "uint16":
                    return Convert.ToUInt16(value);

                case "uint32":
                    return Convert.ToUInt32(value);

                case "uint64":
                    return Convert.ToUInt64(value);

                case "string":
                    return Convert.ToString(value);

                case "double":
                    return Convert.ToDouble(value);

                case "decimal":
                    return Convert.ToDecimal(value);

                case "datetime":
                    return Convert.ToDateTime(value);

                case "timespan":
                    DateTime dt = Convert.ToDateTime(value);
                    TimeSpan timespanVal = dt.TimeOfDay;
                    return timespanVal;


                case "boolean":
                    var boolVal = false;
                    bool.TryParse(Convert.ToString(value), out boolVal);
                    return boolVal;

                case "byte":
                    return Convert.ToByte(value);

                case "single":
                    return Convert.ToSingle(value);

                case "char":
                    return Convert.ToChar(value);

                case "sbyte":
                    return Convert.ToSByte(value);

                default:
                    return null;
            }

        }

        internal bool SendNotification(List<AutoNotification> autoNotifications)
        {
            TextFileLogHelper.LogError("Inside SendNotification: ", false);
            bool result = false;
            try
            {
                MongoDbHelper<NotificationTransaction> mongoDbHelper = new MongoDbHelper<NotificationTransaction>();
                var transactionList = new List<NotificationTransaction>();
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                string id = ConfigurationManager.AppSettings["FcmApiId"];

                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                foreach (var item in autoNotifications)
                {
                    if (item.NotificationTransactions != null && item.NotificationTransactions.Any())
                    {
                        for (int i = 0; i < item.NotificationTransactions.Count; i++)
                        {
                            var transactionBatch = item.NotificationTransactions.Skip(i).Take(50).ToList();
                            var mobileNumbers = transactionBatch.Where(x => !string.IsNullOrEmpty(x.Mobile)).Select(x => x.Mobile).ToList();

                            if (!string.IsNullOrEmpty(item.AutoDialUrl))
                            {
                                var knowLarityUrl = item.AutoDialUrl.Replace("{#number#}", string.Join(",", mobileNumbers));
                                try
                                {
                                    var webRequest = (HttpWebRequest)WebRequest.Create(knowLarityUrl);
                                    webRequest.Method = "GET";
                                    webRequest.ContentType = "application/json";
                                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                                    webRequest.ContentLength = 0;
                                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                                    webRequest.Accept = "*/*";
                                    var webResponse = (HttpWebResponse)webRequest.GetResponse();
                                    if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);

                                    transactionBatch.ForEach(x =>
                                    {
                                        x.IsCallSent = true;
                                    });
                                }
                                catch (Exception ex)
                                {
                                    transactionBatch.ForEach(x =>
                                    {
                                        x.IsCallSent = false;
                                        x.CallSendError = ex.ToString();
                                    });
                                }

                                transactionBatch.Where(x => string.IsNullOrEmpty(x.Mobile)).ToList().ForEach(x =>
                                {
                                    x.IsCallSent = false;
                                    x.CallSendError = "Mobile Number is empty";
                                });
                            }

                            if (!string.IsNullOrEmpty(item.TextMessage))
                            {
                                foreach (var trans in transactionBatch)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(trans.Mobile))
                                        {
                                            //string path = ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("[[Mobile]]", trans.Mobile).Replace("[[Message]]", HttpUtility.UrlEncode(trans.TextMessage));

                                            //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                                            //webRequest.Method = "GET";
                                            //webRequest.ContentType = "application/json";
                                            //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                                            //webRequest.ContentLength = 0;
                                            //webRequest.Credentials = CredentialCache.DefaultCredentials;
                                            //webRequest.Accept = "*/*";
                                            //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                                            //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);

                                            //Common.Helpers.SendSMSHelper.SendSMS(trans.Mobile, trans.TextMessage, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString());

                                            trans.IsTextMessageSent = true;
                                        }
                                        else
                                        {
                                            trans.IsTextMessageSent = false;
                                            trans.TextMessageSendError = "Mobile Number is empty";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.IsTextMessageSent = false;
                                        trans.TextMessageSendError = ex.ToString();
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(item.FCMNotification))
                            {
                                foreach (var trans in transactionBatch)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(trans.FCMId) && !string.IsNullOrEmpty(trans.FCMNotification))
                                        {
                                             FcmNotificationMaster fcmNotificationMaster = JsonConvert.DeserializeObject<FcmNotificationMaster>(trans.FCMNotification);
                                            //FcmNotification notifiation = new FcmNotification { data = fcmNotificationMaster, to = trans.FCMId };


                                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                            //tRequest.Method = "post";

                                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(notifiation);
                                            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                                            //tRequest.Headers.Add(string.Format("Sender: id={0}", id));
                                            //tRequest.ContentLength = byteArray.Length;
                                            //tRequest.ContentType = "application/json";
                                            //using (Stream dataStream = tRequest.GetRequestStream())
                                            //{
                                            //    dataStream.Write(byteArray, 0, byteArray.Length);
                                            //    using (WebResponse tResponse = tRequest.GetResponse())
                                            //    {
                                            //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                            //        {
                                            //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                            //            {
                                            //                String responseFromFirebaseServer = tReader.ReadToEnd();

                                            //                FCMResponse response = JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);

                                            //                if (response.success == 1)
                                            //                {
                                            //                    trans.IsNotificationSent = true;
                                            //                }
                                            //                else
                                            //                {
                                            //                    trans.IsNotificationSent = false;
                                            //                    trans.NotificationSendError = "Some error Occurred.";
                                            //                }


                                            //            }
                                            //        }
                                            //    }
                                            //}

                                            
                                            var data = new FCMData
                                            {
                                                title = fcmNotificationMaster.title,
                                                body = fcmNotificationMaster.body,
                                                image_url = fcmNotificationMaster.icon,

                                            };
                                            
                                            var res =  firebaseService.SendNotificationForApprovalAsync(trans.FCMId, data);
                                            if (res != null)
                                            {
                                                trans.IsNotificationSent  = true;
                                            }
                                            else
                                            {
                                                trans.IsNotificationSent = false;
                                            }
                                        }
                                        else
                                        {
                                            trans.IsNotificationSent = false;
                                            trans.NotificationSendError = "either FCMId is empty or FCM Notificaion text is empty";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.IsNotificationSent = false;
                                        trans.NotificationSendError = ex.ToString();
                                    }
                                }
                            }

                            mongoDbHelper.InsertMany(transactionBatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError("Error in PrepareAutoNotification: " + ex.ToString());
            }
            return result;
        }

        #endregion

        #region Audit

        private List<Audit> FillAuditList(List<DbEntityEntry> entityList, string loggedInUser)
        {
            auditEntityList = new List<Audit>();

            var now = DateTime.Now;

            entityList.ForEach(item =>
            {
                var entityName = item.Entity.GetType().Name;
                var primaryKey = string.Empty;
                var pkName = string.Empty;
                List<string> propertyNames = new List<string>();



                if (item.State != EntityState.Added)
                {
                    primaryKey = Convert.ToString(GetPrimaryKeyValue(item));
                    pkName = GetPrimaryKeyName(item);
                    propertyNames = item.OriginalValues.PropertyNames.ToList();
                }
                else
                    propertyNames = item.CurrentValues.PropertyNames.ToList();

                var audit = new Audit
                {
                    AuditAction = item.State.ToString(),
                    AuditDate = now,
                    AuditEntity = entityName,
                    TableName = GetTableName(item.Entity.GetType()),
                    GUID = Convert.ToString(item.Entity.GetType().GetProperty("GUID").GetValue(item.Entity, null)),
                    PkFieldName = pkName,
                    PkValue = primaryKey,
                    UserName = loggedInUser,
                    // Status = Convert.ToString(item.Entity.GetType().GetProperty("Status").GetValue(item.Entity, null))
                };

                if (item.State == EntityState.Added)
                {
                    foreach (var prop in propertyNames)
                    {
                        if (prop != pkName)
                        {
                            var currentValue = Convert.ToString(item.CurrentValues[prop]);
                            AuditFields log = new AuditFields()
                            {
                                FieldName = prop,
                                NewValue = currentValue,
                                AuditGuid = audit.GUID
                            };
                            audit.AuditFields.Add(log);
                        }
                    }
                }
                else if (item.State == EntityState.Modified)
                {
                    //var values = this.Entry(item).GetDatabaseValues();

                    foreach (var prop in propertyNames)
                    {
                        //if (prop != pkName)
                        //{
                        var originalValue = Convert.ToString(item.OriginalValues[prop]);
                        var currentValue = Convert.ToString(item.CurrentValues[prop]);
                        //if (originalValue != currentValue)
                        //{
                        AuditFields log = new AuditFields()
                        {
                            FieldName = prop,
                            OldValue = originalValue,
                            NewValue = currentValue,
                            AuditGuid = audit.GUID
                        };
                        audit.AuditFields.Add(log);
                        //}
                        //}
                    }
                }
                else if (item.State == EntityState.Deleted)
                {
                    foreach (var prop in item.OriginalValues.PropertyNames)
                    {
                        if (prop != pkName)
                        {
                            var originalValue = Convert.ToString(item.OriginalValues[prop]);
                            AuditFields log = new AuditFields()
                            {
                                FieldName = prop,
                                OldValue = originalValue,
                                AuditGuid = audit.GUID
                            };
                            audit.AuditFields.Add(log);
                        }
                    }
                }

                auditEntityList.Add(audit);

            });


            return auditEntityList;
        }

        internal bool Addcurrentstock(List<CurrentStockUploadDTO> currentstkcollection, string userid)
        {
            int PeopleId = Convert.ToInt32(userid);

            var User = Peoples.FirstOrDefault(x => x.PeopleID == PeopleId);

            foreach (var o in currentstkcollection)
            {
                CurrentStock cst = DbCurrentStock.FirstOrDefault(c => c.ItemNumber == o.ItemNumber && c.WarehouseId == o.WarehouseId && c.CompanyId == o.CompanyId && c.ItemMultiMRPId == o.ItemMultiMRPId);
                if (cst != null)
                {
                    cst.UpdatedDate = indianTime;
                    cst.CurrentInventory = (cst.CurrentInventory + (o.DiffStock));
                    this.Entry(cst).State = EntityState.Modified;

                    CurrentStockHistory Oss = new CurrentStockHistory();
                    Oss.StockId = cst.StockId;
                    Oss.ItemMultiMRPId = cst.ItemMultiMRPId;
                    Oss.ManualReason = "From Uploader";
                    Oss.ItemNumber = cst.ItemNumber;
                    Oss.itemBaseName = cst.itemBaseName;
                    Oss.itemname = cst.itemname;
                    Oss.TotalInventory = cst.CurrentInventory;
                    Oss.ManualInventoryIn = o.DiffStock;
                    Oss.WarehouseName = cst.WarehouseName;
                    Oss.Warehouseid = cst.WarehouseId;
                    Oss.CompanyId = cst.CompanyId;
                    Oss.CreationDate = indianTime;
                    Oss.ManualReason = o.Reason;
                    Oss.userid = User.PeopleID;
                    Oss.UserName = User.DisplayName;
                    CurrentStockHistoryDb.Add(Oss);
                    int id = this.Commit();
                }
            }
            return true;
        }

        internal void AddHubPhase(HubPhasedata item)
        {
            throw new NotImplementedException();
        }

        private object GetPrimaryKeyValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            return objectStateEntry.EntityKey.EntityKeyValues[0].Value;
        }

        private string GetPrimaryKeyName(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);

            return objectStateEntry.EntityKey.EntityKeyValues[0].Key;
        }

        private string GetTableName(Type t)
        {
            var context = ((IObjectContextAdapter)this).ObjectContext;
            var entityName = t.Name;
            var storageMetadata = context.MetadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace);

            foreach (var ecm in storageMetadata)
            {
                EntitySet entitySet;
                if (ecm.StoreEntityContainer.TryGetEntitySetByName(entityName, true, out entitySet))
                {
                    return $"{entitySet.Schema}.[{entitySet.Table}]";
                }
            }

            return null;
        }

        #endregion


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CurrentStock>().HasKey(t => new { t.StockId });
            modelBuilder.Entity<CurrentStock>().HasKey(t => new { t.WarehouseId, t.ItemMultiMRPId });

            modelBuilder.Entity<FreeStock>().HasKey(t => new { t.FreeStockId });
            modelBuilder.Entity<FreeStock>().HasKey(t => new { t.WarehouseId, t.ItemMultiMRPId });

            //modelBuilder.Entity<DamageStock>().HasKey(t => new { t.DamageStockId });
            //modelBuilder.Entity<DamageStock>().HasKey(t => new { t.WarehouseId, t.ItemMultiMRPId });

            modelBuilder.Entity<TemporaryCurrentStock>().HasKey(t => new { t.Id });
            modelBuilder.Entity<TemporaryCurrentStock>().HasKey(t => new { t.WarehouseId, t.ItemMultiMRPId });
            modelBuilder.Entity<MaterialItemMaster>().ToTable("MaterialItemMaster", schemaName: "PackingMaterial");
            modelBuilder.Entity<MaterialItemDetails>().ToTable("MaterialItemDetails", schemaName: "PackingMaterial");
            modelBuilder.Entity<OuterBagMaster>().ToTable("OuterBagMaster", schemaName: "PackingMaterial");
            modelBuilder.Entity<BagDescription>().ToTable("BagDescription", schemaName: "PackingMaterial");

            modelBuilder.Entity<RawMaterialMaster>().ToTable("RawMaterialMaster", schemaName: "PackingMaterial");
            modelBuilder.Entity<RawMaterialDetails>().ToTable("RawMaterialDetails", schemaName: "PackingMaterial");




            modelBuilder.Entity<ItemLimitMaster>().HasKey(t => new { t.Id });
            //modelBuilder.Entity<ItemLimitMaster>().HasIndex(t => new { t.WarehouseId, t.ItemMultiMRPId });

            //modelBuilder.Entity<Supplier>().HasKey(t => new { t.SupplierId });
            //modelBuilder.Entity<Supplier>().HasKey(t => new { t.SUPPLIERCODES, t.Deleted });

        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{

        //    modelBuilder.Entity<PageMaster>()
        //        .HasOptional(c => c.ParentPageMaster).WithMany(i => i.pa)
        //        .Map(t => t.MapLeftKey("CourseID")
        //            .MapRightKey("InstructorID")
        //            .ToTable("CourseInstructor"));

        //    base.OnModelCreating(modelBuilder);
        //}






    }



    // InventoryManager
    public class InventoryManager
    {

        public void UpdateInventory(AuthContext authContext, List<CurrentStock> currentStocks, List<CurrentStockHistory> stockHistories, List<FreeStock> freeStocks, List<FreeStockHistory> freestockHistories)
        {
            foreach (var item in currentStocks)
            {
                authContext.Entry(item).State = EntityState.Modified;
            }
            foreach (var fitem in freeStocks)
            {
                authContext.Entry(fitem).State = EntityState.Modified;
            }
            authContext.CurrentStockHistoryDb.AddRange(stockHistories);
            authContext.FreeStockHistoryDB.AddRange(freestockHistories);
        }
    }
    public class TransactiondataDTO
    {
        public string QcodeUrl { get; set; }
        public int orderId { get; set; }
    }
    public class temOrderQBcode
    {
        //public string QcodeUrl { get; set; }
        //public string BcodeUrl { get; set; }
        public byte[] BarcodeImage { get; set; }

    }

    public class CustomerWhatsAppDTO
    {
        public int OrderId { get; set; }
        public int TemppleteId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public int Password { get; set; }
        public DateTime OrderedDate { get; set; }
        public string DboyName { get; set; }
        public string DboyNumber { get; set; }
    }

}



