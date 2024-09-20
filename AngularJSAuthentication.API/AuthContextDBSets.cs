using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.AutoNotification;
using AngularJSAuthentication.Model.Base;
using AngularJSAuthentication.Model.Base.Audit;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.CashManagement;
using AngularJSAuthentication.Model.Clusteredashboard;
using AngularJSAuthentication.Model.Expense;
using AngularJSAuthentication.Model.KisanDan;
using AngularJSAuthentication.Model.Login;
using AngularJSAuthentication.Model.Permission;
using AngularJSAuthentication.Model.LegderCorrection;
using GenricEcommers.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using static AngularJSAuthentication.API.Controllers.pointConversionController;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.Gullak;
using AngularJSAuthentication.Model.Agentcommision;
using AngularJSAuthentication.Model.Stocks.Configuration;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;

using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using AngularJSAuthentication.BusinessLayer.Ecollection;
using AngularJSAuthentication.Model.Ticket;
using AngularJSAuthentication.BusinessLayer.SMELoan.BO;
using AngularJSAuthentication.Model.Agent;
using AngularJSAuthentication.Model.RazorPay;
using AngularJSAuthentication.Model.Store;
using AngularJSAuthentication.Model.Others;
using AngularJSAuthentication.Model.ClearTax;
using AngularJSAuthentication.BusinessLayer.FinBox;
using AngularJSAuthentication.Model.Seller;
using AngularJSAuthentication.Model.GDN;
using AngularJSAuthentication.Model.Item;
using AngularJSAuthentication.Model.FleetMaster;
using AngularJSAuthentication.Model.DeliveryOptimization;
using AngularJSAuthentication.Model.TripPlanner;
using AngularJSAuthentication.Model.Salescommission;
using AngularJSAuthentication.Model.Rating;
using AngularJSAuthentication.Model.CustomerDelight;
using AngularJSAuthentication.Model.CustomerReferral;
using AngularJSAuthentication.Model.VAN;
using AngularJSAuthentication.Model.PaymentRefund;
using AngularJSAuthentication.Model.Forecasting;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.Model.Stocks.Batch;
using AngularJSAuthentication.Model.DeliveryCapacityOptimization;
using AngularJSAuthentication.Model.ClearanceNonSellable;
using AngularJSAuthentication.Model.UPIPayment;
using AngularJSAuthentication.Model.WarehouseUtilization;
using AngularJSAuthentication.Model.CRM;
using AngularJSAuthentication.Model.RetailerApp;
using AngularJSAuthentication.Model.JustInTime;
using AngularJSAuthentication.Model.InventoryProvisioning;
using AngularJSAuthentication.Model.ROC;
using AngularJSAuthentication.Model.NonRevenueOrders;
using AngularJSAuthentication.Model.ProductPerfomanceDash;
using AngularJSAuthentication.Model.DeliveryOptimization.FleetMaster;
using AngularJSAuthentication.Model.Arthmate;
using AngularJSAuthentication.Model.BackendOrder;
using AngularJSAuthentication.Model.ScaleUp;
using AngularJSAuthentication.Model.Consumer;
using AngularJSAuthentication.Model.Zila.OperationCapacity;

namespace AngularJSAuthentication.API
{
    public partial class AuthContext : IdentityDbContext<IdentityUser>, iAuthContext
    {
        #region All DB Sets

        public DbSet<JITRiskQuantityHistory> JITRiskQuantityHistories { get; set; }
        public DbSet<JITRiskQty> JITRiskQtys { get; set; }
        public DbSet<WarehouseBasedMRPSensitive> WarehouseBasedMRPSensitives { get; set; }
        public DbSet<WarehouseBasedQuadarant> WarehouseBasedQuadarants { get; set; }

        public DbSet<GullakEnable> GullakEnables { get; set; }
        public DbSet<ClearanceAutoApprovedOrders> ClearanceAutoApprovedOrders { get; set; }
        public DbSet<SalesItemNotification> SalesItemNotifications { get; set; }

        public DbSet<ItemPriceDrop> ItemPriceDrops { get; set; }
        public DbSet<OfferTypeDefaultConfig> OfferTypeDefaultConfigs { get; set; }
        public DbSet<CustomerCheckInData> CustomerCheckInDataDB { get; set; }
        public DbSet<SalesSuggestedCategoryMapping> SalesSuggestedCategoryMappings { get; set; }

        public DbSet<EwayBillErrorList> EwayBillErrorLists { get; set; }
        public DbSet<AuthTokenGeneration> AuthTokenGenerations { get; set; }
        public DbSet<OrderEwayBillsGenError> OrderEwayBillsGenErrors { get; set; }
        public DbSet<EwayBillGeneration> EwayBillGenerations { get; set; }

        public DbSet<EwayBillConfiguration> EwayBillConfigurations { get; set; }

        public DbSet<BatchMasterTemp> BatchMasterTemps { get; set; }

        public DbSet<ItemIncentiveClassificationMaster> ItemIncentiveClassificationMasters { get; set; }
        public DbSet<ItemCityWiseIncentiveClassification> ItemCityWiseIncentiveClassifications { get; set; }
        public DbSet<ItemCityWiseIncentivePlaningClassification> ItemCityWiseIncentivePlaningClassifications { get; set; }
        public DbSet<ClearancePickerTimer> ClearancePickerTimers { get; set; }


        public DbSet<InventoryReserveStock> InventoryReserveStocks { get; set; }
        public DbSet<InventoryReserveStockHistory> InventoryReserveStockHistorys { get; set; }

        #region  UPITransactions
        public DbSet<UPITransaction> UPITransactions { get; set; }

        #endregion
        #region Clearance 



        public DbSet<ClearanceNonSaleableShelfConfiguration> ClearanceNonsShelfConfigurations { get; set; }
        public DbSet<ClearanceNonSaleablePrepareItem> ClearanceNonSaleablePrepareItems { get; set; }
        public DbSet<ClearanceNonSaleableMovementOrder> ClearanceNonSaleableMovementOrders { get; set; }
        public DbSet<ClearanceNonSaleableMovementOrderDetail> ClearanceNonSaleableMovementOrderDetails { get; set; }

        public DbSet<ClNSShelfConfigurationTemp> ClNSShelfConfigurationTemps { get; set; }
        #endregion

        #region Batch

        public DbSet<ItemBarcode> ItemBarcodes { get; set; }
        public DbSet<BatchMaster> BatchMasters { get; set; }
        public DbSet<StockBatchMaster> StockBatchMasters { get; set; }
        public DbSet<StockBatchTransaction> StockBatchTransactions { get; set; }
        public DbSet<GRBatch> GRBatchs { get; set; }
        public DbSet<StockTxnTypeMaster> StockTxnTypeMasters { get; set; }
        public DbSet<DeliveryCancelledItemBatch> DeliveryCancelledItemBatchs { get; set; }

        #endregion
        public DbSet<ClearanceOrderDetail> ClearanceOrderDetails { get; set; }


        public DbSet<CLCustomer> CLCustomers { set; get; } //This table contain only customize customer (like Chqbook, EpayLater etc)
        public DbSet<CODLimitCustomer> CODLimitCustomers { get; set; }
        public DbSet<PaymentRefundRequest> PaymentRefundRequests { get; set; }
        public DbSet<LedgerCorrection> LedgerCorrections { get; set; }
        public DbSet<PaymentRefundApi> PaymentRefundApis { get; set; }
        public DbSet<PaymentRefundHistory> PaymentRefundHistories { get; set; }


        public DbSet<VANResponse> VANResponses { get; set; }
        public DbSet<VANTransaction> VANTransactiones { get; set; }
        public DbSet<WarehouseStoreMapping> WarehouseStoreMappings { get; set; }
        public DbSet<PeopleSentNotification> PeopleSentNotifications { get; set; }
        public DbSet<TripCustomerVoiceRecord> TripCustomerVoiceRecords { get; set; }
        public DbSet<ConfigureNotify> ConfigureNotifys { get; set; }
        //mbd 12 july 2022
        //public DbSet<SalesGroup> SalesGroups { get; set; }

        public DbSet<TargetDetails> TargetDetailss { get; set; }

        public DbSet<TargetHead> TargetHeads { get; set; }
        public DbSet<TopSKUsItem> TopSKUsItems { get; set; }
        public DbSet<ProductCatalog> ProductCatalogs { get; set; }
        public DbSet<ProductCatalogItem> ProductCatalogItems { get; set; }
        public DbSet<CatelogConfig> CatelogConfigs { get; set; }
        public DbSet<CustomerRemark> CustomerRemarks { get; set; }
        public DbSet<BeatEditConfig> BeatEditConfigs { get; set; }
        public DbSet<AttendanceRuleConfig> AttendanceRuleConfigs { get; set; }
        public DbSet<AttendanceRuleConfigsLog> AttendanceRuleConfigsLogs { get; set; }
        public DbSet<OrderConcern> OrderConcernDB { get; set; }
        public DbSet<OrderConcernMaster> OrderConcernMasterDB { get; set; }
        public DbSet<ExecutiveAttendanceRule> ExecutiveAttendanceRules { get; set; }


        #region Cluster Store Beat Mapping 28 Feb 2022
        public DbSet<ClusterStoreBeatMapping> ClusterStoreBeatMappings { get; set; }
        public DbSet<BeatScheduler> BeatSchedulers { get; set; }

        #endregion
        #region Rating system
        public DbSet<RatingMaster> RatingMasters { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }
        #endregion



        #region group2 seller
        public DbSet<NotificationRequest> NotificationRequests { get; set; }
        public DbSet<BrandStoreRequest> BrandStoreRequests { get; set; }
        public DbSet<AppBannerRequest> AppBannerRequests { get; set; }
        public DbSet<MurliRequest> MurliRequests { get; set; }
        public DbSet<FlashDealRequest> FlashDealRequests { get; set; }

        public DbSet<FlashDealRequestItem> FlashDealRequestItems { get; set; }

        #endregion


        public DbSet<OutBoundDeliveryMapping> OutBoundDeliveryMappings { get; set; }
        public DbSet<OutBoundDeliveryDetail> OutBoundDeliveryDetails { get; set; }

        public DbSet<SellerSubCatOffer> SellerSubCatOffers { get; set; }

        public DbSet<SellerMonthlyChargeMaster> SellerMonthlyChargeMasters { get; set; }
        public DbSet<SellerMonthlyCharge> SellerMonthlyCharges { get; set; }

        public DbSet<SellerActiveProduct> SellerActiveProducts { get; set; }
        public DbSet<ServiceCharge> ServiceCharges { get; set; }
        public DbSet<SalesTarget> SalesTargets { get; set; }
        public DbSet<SalesReturnConfig> SalesReturnConfigs { get; set; }
        public DbSet<SalesReturnRequest> SalesReturnRequests { get; set; }
        public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
        public DbSet<SalesReturnItemImage> SalesReturnItemImages { get; set; }
        public DbSet<SellerRegistration> SellerRegistrations { get; set; }
        public DbSet<OTPMaster> OTPMasters { get; set; }
        public DbSet<PeopleSubcatMapping> PeopleSubcatMappings { get; set; }//
        //public DbSet<SellerRegistration> SellerRegistrations { get; set; }
        public DbSet<ItemSchemeExcelUploaderMaster> ItemSchemeExcelUploaderMasters { get; set; }//
        public DbSet<ItemSchemeBackMarginCalculation> ItemSchemeBackMarginCalculations { get; set; }//

        public DbSet<ItemSchemeMaster> ItemSchemeMasters { get; set; }//
        public DbSet<DeliveredOrderToFranchise> DeliveredOrderToFranchises { get; set; }//

        public DbSet<CustomerExecutiveMapping> CustomerExecutiveMappings { get; set; }//

        public DbSet<ClusterStoreExecutive> ClusterStoreExecutives { get; set; }//
        public DbSet<CfrArticle> CfrArticles { get; set; }//
        public DbSet<DeliveryRedispatchChargeConf> DeliveryRedispatchChargeConfs { get; set; }//

        public DbSet<AgentPitchMaster> AgentPitchMasters { get; set; }//

        public DbSet<ItemLiveDashboard> ItemLiveDashboards { get; set; }//

        public DbSet<MrpStockTransfer> MrpStockTransfers { get; set; }//
        public DbSet<CRMCustomerLevel> CRMCustomerLevels { get; set; }
        public DbSet<InventoryCycleHistory> InventoryCycleHistoryDB { get; set; }
        public DbSet<InventCycleDataHistory> InventCycleDataHistoryDB { get; set; }
        public DbSet<PvReconcillationHistory> PvReconcillationHistoryDB { get; set; }



        #region Vehicle Master
        public DbSet<VehicleMaster> VehicleMasterDB { get; set; }
        public DbSet<FleetMaster> FleetMasterDB { get; set; }
        public DbSet<FleetMasterDetail> FleetMasterDetailDB { get; set; }
        public DbSet<FleetMasterAccountDetail> fleetMasterAccountDetailDB { get; set; }
        #endregion

        #region PackingMaterial
        public DbSet<SellerOrderDelivered> SellerOrderDelivereds { get; set; }
        public DbSet<MaterialItemMaster> MaterialItemMaster { get; set; }
        public DbSet<MaterialItemDetails> MaterialItemDetails { get; set; }
        public DbSet<OuterBagMaster> OuterBagMaster { get; set; }
        public DbSet<BagDescription> BagDescription { get; set; }
        public DbSet<RawMaterialMaster> RawMaterialMaster { get; set; }
        public DbSet<RawMaterialDetails> RawMaterialDetails { get; set; }
        public DbSet<RawMaterialDetailsHistory> RawMaterialDetailsHistory { get; set; }
        #endregion

        #region ECollection
        public DbSet<MISData> MISData { get; set; }
        #endregion

        #region SMELoan
        public DbSet<SMECustomerLoan> SMECustomerLoan { get; set; }
        public DbSet<SMECustomerLoanHistory> SMECustomerLoanHistory { get; set; }

        public DbSet<SMERequestResponse> SMERequestResponse { get; set; }

        #endregion
        #region cluster Refresh
        public DbSet<ClusterRefreshRequest> ClusterRefreshRequest { get; set; }
        public DbSet<ClusterRefershCustomer> ClusterRefershCustomer { get; set; }

        #endregion

        public DbSet<PRApprovelsStatus> PRApprovelsStatus { get; set; }//
        public DbSet<IRApprovalStatus> IRApprovalStatus { get; set; }//
        public DbSet<PRPaymentAppoved> PRPaymentAppoved { get; set; }//
        #region Auto Notification
        public DbSet<OperatorMaster> OperatorMaster { get; set; }
        public DbSet<OperatorFieldMapping> OperatorFieldMapping { get; set; }
        public DbSet<FieldTypeMaster> FieldTypeMaster { get; set; }
        public DbSet<AutoNotification> AutoNotification { get; set; }
        public DbSet<ANScheduleMaster> ANScheduleMaster { get; set; }
        public DbSet<AutoNotificationCondition> AutoNotificationCondition { get; set; }
        public DbSet<ANFieldMaster> ANFieldMaster { get; set; }
        public DbSet<ANEntityMaster> ANEntityMaster { get; set; }
        public DbSet<ANFrequencyMaster> ANFrequencyMaster { get; set; }
        #endregion

        #region For Create DbSet


        //public DbSet<CallSMSRequest> CallSMSRequestDB { get; set; }    //CallSMSRequest   tejas
        //public DbSet<CallSMSCustomerList> CallSMSCustomerListDB { get; set; }    //CallSMSCustomerList   tejas 


        public DbSet<OrderDeliveryOTP> OrderDeliveryOTP { get; set; }//FreeStock
        public DbSet<MakerCheckerMaster> MakerCheckerMaster { get; set; }//FreeStock

        public DbSet<FreeStock> FreeStockDB { get; set; }//FreeStock
        public DbSet<FreeStockHistory> FreeStockHistoryDB { get; set; }//FreestockHistory
        public DbSet<InventoryFormEdit> InventoryFormEditDB { get; set; }//InventoryFormEdit
        public DbSet<InventoryFormEditDetail> InventoryFormEditDetailDB { get; set; }//InventoryFormEditDetail
        public DbSet<CustFavoriteItem> CustFavoriteItems { get; set; }

        public DbSet<EntityMaster> EntityMasters { get; set; }

        public DbSet<EntitySerialMaster> EntitySerialMasters { get; set; }

        public DbSet<CustomerLoginDevice> CustomerLoginDeviceDB { get; set; }
        public DbSet<Audit> Audit { get; set; }
        public DbSet<AuditFields> AuditFields { get; set; }
        public DbSet<MurliItemsDetails> MurliItemsDetailsDB { get; set; }// murli by raj
        public DbSet<MurliWarehouseTopItem> MurliWarehouseTopItemDB { get; set; }//murli by raj



        #region Epaylater
        public DbSet<EpayLaterForm> EpayLaterFormDB { get; set; }
        public DbSet<EpayLaterPartner> EpayLaterPartnerDB { get; set; }
        #endregion
        #region Upload Online transaction
        public DbSet<UploadFileReconcile> UploadFileReconcileDB { get; set; }
        public DbSet<HDFCUpload> HDFCUploadDB { get; set; }
        public DbSet<ICICIUpload> ICICIUploadDB { get; set; }

        public DbSet<UPIUpload> UPIUploadDB { get; set; }
        public DbSet<EpaylaterUpload> EpaylaterUploadDB { get; set; }
        public DbSet<MposUpload> MposUploadDB { get; set; }
        // public DbSet<HDFCUPIUpload> HDFCUPIUploadDB { get; set; }
        public DbSet<HDFCNetBankingUpload> HDFCNetBankingUploadDB { get; set; }


        public DbSet<SupplierBrandMap> SupplierBrandMaps { get; set; }

        #endregion

        #region manualwallet
        public DbSet<ManualWallet> ManualWallets { get; set; }
        #endregion


        #region orangeBook
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 04 October 2019
        /// </summary>

        public DbSet<OrangeBookCategory> OrangeBookCategoryDB { get; set; }
        public DbSet<OrangeBookSubCategory> OrangeBookSubCategoryDB { get; set; }
        public DbSet<OrangeBook> OrangeBookDB { get; set; }
        public DbSet<OrangeBookAccepted> OrangeBookAcceptedDB { get; set; }


        #endregion



        //public DbSet<TraceLog> TraceLog { get; set; }
        //public DbSet<ErrorLog> ErrorLog { get; set; }

        //public DbSet<DebugLog> DebugLog { get; set; }

        #region TargetModule
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 09 July 2019
        /// </summary>
        public DbSet<LevelMaster> LevelMasterDB { get; set; }
        public DbSet<CustomerBands> CustomerBandsDB { get; set; }
        public DbSet<TargetBandAllocation> TargetBandAllocationDB { get; set; }
        public DbSet<CustomerLevel> CustomerLevelDB { get; set; }
        public DbSet<CustomersTarget> CustomersTargetDB { get; set; }
        public DbSet<TargetAllocation> TargetAllocationDB { get; set; }
        #endregion

        #region SurveyModule
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 11 Nov 2019
        /// </summary>
        public DbSet<SurveyModule> SurveyModuleDB { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestionDB { get; set; }
        public DbSet<SurveyQuestionAnswer> SurveyQuestionAnswerDB { get; set; }
        public DbSet<CustomerSurvey> CustomerSurveyDB { get; set; }
        public DbSet<CustomerSurveyAnswer> CustomerSurveyAnswerDB { get; set; }

        #endregion

        #region Temporary current stock & history : Use in GR process
        public DbSet<TemporaryCurrentStock> TemporaryCurrentStockDB { get; set; }
        public DbSet<TemporaryCurrentStockHistory> TemporaryCurrentStockHistoryDB { get; set; }

        #endregion

        public DbSet<OrderPickerMaster> OrderPickerMasterDb { get; set; }
        public DbSet<PickerTimer> PickerTimerDb { get; set; }


        public DbSet<DeptOrderCancellation> DeptOrderCancellationDb { get; set; }
        public DbSet<OrderPickerDetails> OrderPickerDetailsDb { get; set; }

        public DbSet<freelancerAgents> freelancerAgentDB { get; set; }    //tejas 03/07/2019
        public DbSet<GroupMapping> GroupMappings { get; set; }
        public DbSet<GroupSMS> GroupsSms { get; set; }
        public DbSet<OrderTransactionProcessing> OrderTransactionProcessingDB { get; set; }

        public DbSet<OrderDeliveryMasterHistories> OrderDeliveryMasterHistoriesDB { get; set; }

        public DbSet<ShortItemAssignment> ShortItemAssignmentDB { get; set; }

        public DbSet<ItemMultiMRP> ItemMultiMRPDB { get; set; }
        public DbSet<DeliveryEclipseTime> DeliveryEclipseTimeDB { get; set; }
        public DbSet<InActiveCustomerOrderDetails> InActiveCustomerOrderDetailsDB { get; set; }
        public DbSet<InActiveCustomerOrderMaster> InActiveCustomerOrderMasterDB { get; set; }
        public DbSet<CashConversionHistory> CashConversionHistoryDB { get; set; }
        public DbSet<OrderPunchMaster> OrderPunchMasterDB { get; set; }
        public DbSet<OrderPunchDetails> OrderPunchDetailsDB { get; set; }
        public DbSet<RBLCustomerInformation> RBLCustomerInformationDB { get; set; }
        public DbSet<ItemMasterCentral> ItemMasterCentralDB { get; set; }
        public DbSet<OrderDeliveryMaster> OrderDeliveryMasterDB { get; set; }
        public DbSet<TrupayTransaction> TrupayTransactionDB { get; set; }
        public DbSet<AgentData> AgentDataDB { get; set; }
        public DbSet<DialValuePoint> DialValuePointDB { get; set; }
        public DbSet<DialPoint> DialPointDB { get; set; }
        public DbSet<CheckCurrency> CheckCurrencyDB { get; set; }
        public DbSet<CurrencyHistory> CurrencyHistoryDB { get; set; }
        public DbSet<CustomerWalletHistory> CustomerWalletHistoryDb { get; set; }
        public DbSet<WarehouseWallet> WarehouseWalletDb { get; set; }
        public DbSet<CustWarehouse> CustWarehouseDB { get; set; }
        public DbSet<CurrencyStock> CurrencyStockDB { get; set; }
        public DbSet<CurrencyData> CurrencyDataDB { get; set; }
        public DbSet<DBoyCurrency> DBoyCurrencyDB { get; set; }
        public DbSet<CurrencyBankSettle> CurrencyBankSettleDB { get; set; }
        public DbSet<WarehousePoint> WarehousePointDB { get; set; }
        public DbSet<WarehousePointLimit> WarehousePointLimitDB { get; set; }
        public DbSet<CustSupplier> CustSupplierDb { get; set; }
        public DbSet<SupplierBrands> SupplierBrandsDb { get; set; }
        public DbSet<CustSupplierRequest> CustSupplierRequestDb { get; set; }
        public DbSet<DamageStock> DamageStockDB { get; set; }
        public DbSet<DamageStockHistory> DamageStockHistoryDB { get; set; }

        public DbSet<NonSellableStock> NonSellableStockDB { get; set; }
        public DbSet<NonSellableStockHistory> NonSellableStockHistoryDB { get; set; }
        public DbSet<ClearanceStockNew> ClearanceStockNewDB { get; set; }
        public DbSet<ClearanceStockNewHistory> ClearanceStockNewHistoryDB { get; set; }
        public DbSet<MovementDetailStock> MovementDetailStockDB { get; set; }

        //public DbSet<ClearanceStockHistory> ClearanceStockHistoryDB { get; set; }

        public DbSet<DamageOrderMaster> DamageOrderMasterDB { get; set; }
        public DbSet<DamageOrderDetails> DamageOrderDetailsDB { get; set; }
        public DbSet<Area> AreaDb { get; set; }
        public DbSet<CompanyHoliday> CompanyHolidaysDB { get; set; }
        public DbSet<ReqService> ReqServiceDB { get; set; }
        public DbSet<IR_Confirm> IR_ConfirmDb { get; set; }
        public DbSet<GoodsReceivedDetail> GoodsReceivedDetail { get; set; }

        public DbSet<GoodsDescripancyNote> GoodsDescripancyNote { get; set; } // Added 10/2/2021
        public DbSet<InvoiceReceiptDetail> InvoiceReceiptDetail { get; set; }
        public DbSet<IRCreditNoteMaster> IRCreditNoteMaster { get; set; }
        public DbSet<IRCreditNoteDetail> IRCreditNoteDetail { get; set; }

        public DbSet<UnitEconomic> UnitEconomicDb { get; set; }
        public DbSet<InvoiceReceive> InvoiceReceiveDb { get; set; }
        public DbSet<InvoiceReceiptImage> InvoiceReceiptImage { get; set; }
        public DbSet<InvoiceImage> InvoiceImageDb { get; set; }

        public DbSet<POReturnRequest> POReturnRequestDb { get; set; }

        public DbSet<FreeItem> FreeItemDb { get; set; }
        public DbSet<SKFreeItem> SKFreeItemDb { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturnDb { get; set; }
        public DbSet<Target> TargetDb { get; set; }
        //public DbSet<CustomerMap> CustomerMapDB { get; set; }        
        public DbSet<ConsumerappVersion> ConsumerAppVersionDb { get; set; }
        public DbSet<appVersion> appVersionDb { get; set; }
        public DbSet<SarthiAppVersion> SarthiAppVersionDb { get; set; }
        public DbSet<AvgInventory> AvgInventoryDb { get; set; }
        public DbSet<InventCycle> InventCycleDb { get; set; }

        public DbSet<InventCycleBatch> InventCycleBatchDb { get; set; }
        public DbSet<InventCycleBatchSentForApproval> InventCycleBatchSentForApprovals { get; set; }

        public DbSet<PickItemForBarcode> PickItemForBarcodeDb { get; set; }

        public DbSet<InventoryCycleMaster> InventoryCycleMasterDb { get; set; }
        public DbSet<ShortSetttle> ShortSetttleDb { get; set; }
        public DbSet<ItemMasterHistory> ItemMasterHistoryDb { get; set; }
        public DbSet<CurrentStockHistory> CurrentStockHistoryDb { get; set; }
        #region offers
        public DbSet<Offer> OfferDb { get; set; }
        public DbSet<DistributorOffer> DistributorOffer { get; set; }
        public DbSet<BillDiscountFreeItem> BillDiscountFreeItem { get; set; }
        public DbSet<CustomerEstimationOffer> CustomerEstimationOffer { get; set; }
        #endregion
        public DbSet<DocumentList> DocumentLists { get; set; }
        public DbSet<OfferItem> OfferItemDb { get; set; }
        public DbSet<Wallet> WalletDb { get; set; }
        public DbSet<HDFCPayment> HDFCPaymentDb { get; set; }
        public DbSet<RewardPoint> RewardPointDb { get; set; }
        public DbSet<RPConversion> RPConversionDb { get; set; }
        public DbSet<MilestonePoint> MilestonePointDb { get; set; }
        public DbSet<RetailerShare> RetailerShareDb { get; set; }
        public DbSet<CashConversion> CashConversionDb { get; set; }
        public DbSet<promoPurConv> promoPurConvDb { get; set; }
        public DbSet<supplierPoint> supplierPointDb { get; set; }
        public DbSet<RewardItems> RewardItemsDb { get; set; }
        public DbSet<DreamOrder> DreamOrderDb { get; set; }
        public DbSet<DreamItem> DreamItemDb { get; set; }
        public DbSet<ActionTask> ActionTaskDb { get; set; }
        public DbSet<CustomerIssue> CustomerIssuedb { get; set; }
        public DbSet<SalesPersonBeat> SalesPersonBeatDb { get; set; }
        public DbSet<DeliveryCharge> DeliveryChargeDb { get; set; }
        public DbSet<DailyEssential> DailyEssentialDb { get; set; }
        public DbSet<GpsCoordinate> GpsCoordinateDb { get; set; }
        public DbSet<DailyItemEdit> DailyItemCancelDb { get; set; }
        public DbSet<OrderNotes> OrderNotesDb { get; set; }
        public DbSet<ItemMaster> itemMasters { get; set; }
        public DbSet<ItemScheme> ItemSchemes { get; set; }
        public DbSet<BaseCategory> BaseCategoryDb { get; set; }
        public DbSet<DeliveryIssuance> DeliveryIssuanceDb { get; set; }
        public DbSet<IssuanceDetails> IssuanceDetailsDb { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<SubCategory> SubCategorys { get; set; }
        public DbSet<SubsubCategory> SubsubCategorys { get; set; }
        public DbSet<SubcategoryCategoryMapping> SubcategoryCategoryMappingDb { get; set; }
        public DbSet<BrandCategoryMapping> BrandCategoryMappingDb { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehousePVHistory> DBWarehousePVHistory { get; set; }
        public DbSet<WarehouseCategory> DbWarehouseCategory { get; set; }
        public DbSet<WarehouseSubCategory> DbWarehouseSubCategory { get; set; }
        public DbSet<WarehouseSubsubCategory> DbWarehousesubsubcats { get; set; }
        public DbSet<WarehouseSupplier> DbWarehouseSupplier { get; set; }
        public DbSet<OrderMaster> DbOrderMaster { get; set; }
        public DbSet<OrderDetails> DbOrderDetails { get; set; }
        public DbSet<FinalOrderDispatchedMaster> FinalOrderDispatchedMasterDb { get; set; }
        public DbSet<Cluster> Clusters { get; set; }
        public DbSet<LtLng> LtLngs { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<People> Peoples { get; set; }
        public DbSet<Role> UserRole { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerLatLngVerify> CustomerLatLngVerify { get; set; }
        public DbSet<CustomerLocation> CustomerLocations { get; set; }
        public DbSet<InsuranceCustomer> InsuranceCustomers { get; set; }
        public DbSet<CustomerDoc> CustomerDocs { get; set; }
        public DbSet<CustomerDocTypeMaster> CustomerDocTypeMasters { get; set; }
        public DbSet<PhoneRecordHistory> PhoneRecordHistoryDB { get; set; }  //tejas 21-05-2019
        //public DbSet<RetailerPhoneRecordHistory> RetailerPhoneRecordHistoryDB { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<AllInvoice> invoices { get; set; }
        public DbSet<InvoiceRow> InvoiceRows { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<CustomerCategory> CustomerCategorys { get; set; }
        public DbSet<ItemBrand> DbItemBrand { get; set; }
        public DbSet<FinancialYear> DbFinacialYear { get; set; }
        public DbSet<CustomerRegistration> CustomerRegistrations { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<DepoMaster> DepoMasters { get; set; }  /*----------------------------------tejas */
        public DbSet<SupplierCategory> SupplierCategory { get; set; }
        public DbSet<TaxGroupDetails> DbTaxGroupDetails { get; set; }
        public DbSet<TaxGroup> DbTaxGroup { get; set; }
        public DbSet<TaxMaster> DbTaxMaster { get; set; }
        public DbSet<DemandMaster> dbDemandMasters { get; set; }
        public DbSet<DemandDetails> dbDemandDetails { get; set; }
        public DbSet<ItemPramotions> itempramotions { get; set; }
        public DbSet<BillPramotion> BillPramotions { get; set; }
        public DbSet<PurchaseOrder> DbPurchaseOrder { get; set; }
        public DbSet<PurchaseOrderMaster> DPurchaseOrderMaster { get; set; }
        public DbSet<PurchaseOrderDetail> DPurchaseOrderDeatil { get; set; }
        public DbSet<PurchaseRequestMaster> PurchaseRequestMasterDB { get; set; }
        public DbSet<POClosedApproval> POClosedApproval { get; set; }
        public DbSet<POClosedApprovalRequest> POClosedApprovalRequest { get; set; }
        public DbSet<PurchaseOrderRequestDetail> PurchaseOrderRequestDetailDB { get; set; }
        public DbSet<CurrentStock> DbCurrentStock { get; set; }
        public DbSet<Message> DbMessage { get; set; }
        public DbSet<Favorites> Favoritess { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<RequestItem> RequestItems { get; set; }
        public DbSet<PurchaseOrderMasterRecived> PurchaseOrderMasterRecivedes { get; set; }
        public DbSet<PurchaseOrderDetailRecived> PurchaseOrderRecivedDetails { get; set; }
        public DbSet<OrderDispatchedDetails> OrderDispatchedDetailss { get; set; }
        public DbSet<OrderDispatchedMaster> OrderDispatchedMasters { get; set; }
        public DbSet<AssignmentRechangeOrder> AssignmentRechangeOrder { get; set; }
        public DbSet<Slider> SliderDb { get; set; }
        public DbSet<ReturnOrderDispatchedDetails> ReturnOrderDispatchedDetailsDb { get; set; }
        public DbSet<FinalOrderDispatchedDetails> FinalOrderDispatchedDetailsDb { get; set; }
        public DbSet<Notification> NotificationDb { get; set; }
        public DbSet<NotificationByDeviceId> NotificationByDeviceIdDb { get; set; }
        public DbSet<DeviceNotification> DeviceNotificationDb { get; set; }
        public DbSet<ApplicationIdNotification> ApplicationIdNotificationDb { get; set; }
        public DbSet<GroupNotification> GroupNotificationDb { get; set; }
        public DbSet<Coupon> CouponDb { get; set; }
        public DbSet<Division> DivisionDb { get; set; }
        public DbSet<News> NewsDb { get; set; }
        public DbSet<Vehicle> VehicleDb { get; set; }
        public DbSet<RedispatchWarehouse> RedispatchWarehouseDb { get; set; }
        public DbSet<EditPriceHistory> EditPriceHistoryDb { get; set; }
        public DbSet<AgentAmount> AgentAmountDb { get; set; }
        public DbSet<MarginImage> MarginImageDB { get; set; }
        public DbSet<CategoryImage> CategoryImageDB { get; set; }
        public DbSet<BankDisposable> BankDisposableDB { get; set; }
        public DbSet<TransferWHOrderMaster> TransferWHOrderMasterDB { get; set; }
        public DbSet<TransferWHOrderDetails> TransferWHOrderDetailsDB { get; set; }
        public DbSet<TransferWHOrderDispatchedMaster> TransferWHOrderDispatchedMasterDB { get; set; }
        public DbSet<TransferWHOrderDispatchedDetail> TransferWHOrderDispatchedDetailDB { get; set; }
        public DbSet<CustomerVoice> CustomerVoiceDB { get; set; }
        public DbSet<CustomerVoiceReply> CustomerVoiceReplyDB { get; set; }
        public DbSet<UserTraking> UserTrakings { get; set; }
        public DbSet<UserAccessPermission> UserAccessPermissionDB { get; set; }
        public DbSet<ItemCode> ItemCodeDb { get; set; }
        public DbSet<AppHomeItem> AppHomeItemDb { get; set; }
        public DbSet<AppHomeDynamic> AppHomeDynamicDb { get; set; }
        public DbSet<CaseComment> CaseComment { get; set; }
        public DbSet<CaseProject> CaseProject { get; set; }
        public DbSet<CaseImage> CaseImage { get; set; }
        public DbSet<CaseModule> Cases { get; set; }
        public DbSet<PSalary> PeoplesSalaryDB { get; set; }//By Shoaib 07/12/2018
        public DbSet<PeopleDocument> PeopleDocumentDB { get; set; }//By Shoaib 07/12/2018
        public DbSet<Department> Departments { get; set; }//By Hemant 11/12/2018
        public DbSet<Designation> DesignationsDB { get; set; }//By Shoaib 07/12/2018
        public DbSet<Skill> Skills { get; set; }
        public DbSet<TaxMasterHistories> TaxMasterHistoriesDB { get; set; }
        public DbSet<OrderMasterHistories> OrderMasterHistoriesDB { get; set; }
        public DbSet<SupplierHistory> SupplierHistoryDB { get; set; }
        public DbSet<CustomerHistory> CustomerHistoryDB { get; set; }
        public DbSet<CaseHistory> CaseHistoryDB { get; set; }
        public DbSet<OrderItemHistory> OrderItemHistoryDB { get; set; }
        public DbSet<POHistory> POHistoryDB { get; set; }
        public DbSet<PoEditItemHistory> PoEditItemHistoryDB { get; set; }
        public DbSet<WarehouseBaseCategory> WarehouseBaseCategoryDB { get; set; }
        public DbSet<SupplierAppVersion> SupplierAppVersionDB { get; set; }
        public DbSet<DeliveryAppVersion> DeliveryAppVersionDB { get; set; }
        public DbSet<NewDeliveryAppVersion> NewDeliveryAppVersionDB { get; set; }
        public DbSet<WuduAppVersion> WuduAppVersionDB { get; set; }
        public DbSet<TradeAppVersion> TradeAppVersionDB { get; set; }

        public DbSet<SalesappVersion> SalesappVersionDB { get; set; }
        public DbSet<CustomerTrackingAppVersion> CustomerTrackingAppVersionDB { get; set; }
        public DbSet<Myudhar> MyudharDB { get; set; }
        public DbSet<PoApproval> PoApprovalDB { get; set; }
        public DbSet<SupplierWarehouse> SupplierWarehouseDB { get; set; }
        public DbSet<IssueCategory> IssueCategoryDB { get; set; }//Add detail
        public DbSet<IssueSubCategory> IssueSubCategoryDB { get; set; }//Add detail
        public DbSet<SalespersonNotification> SalespersonNotificationDB { get; set; }
        public DbSet<SupplierPaymentData> SupplierPaymentDataDB { get; set; }//By Sachin
        public DbSet<FullSupplierPaymentData> FullSupplierPaymentDataDB { get; set; }//By Sachin
        public DbSet<GameQuestion> GameQuestionDB { get; set; }//By Sachin
        public DbSet<GameLevel> GameLevelDB { get; set; }//By Sachin
        public DbSet<CustGame> CustGameDB { get; set; }//By Sachin        
        public DbSet<SalesAppCounter> SalesAppCounterDB { get; set; }
        public DbSet<PaymentResponseRetailerApp> PaymentResponseRetailerAppDb { get; set; } //tejas 
        public DbSet<Peoplecluster> PeopleclusterDB { get; set; }
        public DbSet<PeopleHistory> PeopleHistoryDB { get; set; }
        public DbSet<OfferHistory> OfferHistoryDB { get; set; } // by Sachin
        public DbSet<IRMaster> IRMasterDB { get; set; }
        public DbSet<OnHoldGR> OnHoldGRDB { get; set; } //by Ashwin
        public DbSet<GeoFence> GeoFences { get; set; }
        public DbSet<ClusterHub> ClusterHubs { get; set; }
        public DbSet<CallHistroy> CallHistoryDB { get; set; }
        public DbSet<POApprovalHistory> POApprovalHistoryDB { get; set; }
        public DbSet<ItemLimitMaster> ItemLimitMasterDB { get; set; }
        public DbSet<SafetyStockHistory> SafetyStockHistoryDB { get; set; }//pooja k

        public DbSet<FlashDealItemConsumed> FlashDealItemConsumedDB { get; set; }
        public DbSet<TransferOrderHistory> TransferOrderHistoryDB { get; set; }//By Ashwin
        public DbSet<CurrencyVerification> CurrencyVerification { get; set; }
        public DbSet<DriverDetails> driverDetails { get; set; }
        public DbSet<SecurityDetails> securityDetails { get; set; }
        public DbSet<TrainingDevelopment> trainingDevelopment { get; set; }
        public DbSet<AgentLicense> agentLicense { get; set; }

        public DbSet<SupplierChat> SupplierChatDB { get; set; }   // tejas
        public DbSet<HubPhasedata> HubPhasedataDb { get; set; } //By Vinayak

        public DbSet<Country> Countries { get; set; } // By pooja.Z
        public DbSet<Zone> zone { get; set; } // By pooja.Z

        public DbSet<MurliImage> MurliImageDB { get; set; }// murli by raj
        public DbSet<MurliAudioImage> MurliAudioImageDB { get; set; }// murli by raj

        public DbSet<OnlinePaymentDtlsForLedger> OnlinePaymentDtlsForLedgerDB { get; set; } // by shailesh
        public DbSet<AutoSettleOrderDetail> AutoSettleOrderDetailDB { get; set; } // by shailesh
        public DbSet<HDFCCreditUpload> HDFCCreditUploadDB { get; set; }//by sudhir
        public DbSet<RazorpayQRUpload> RazorpayQRUploadDB { get; set; }//by sudhirrer
        public DbSet<LocationResumeDetail> LocationResumeDetails { get; set; }//by Shailesh
        public DbSet<ClearTaxIntegration> ClearTaxIntegrations { get; set; } //by Shailesh
        public DbSet<ClearTaxReqResp> ClearTaxReqResps { get; set; } //by Shailesh
        public DbSet<OrderEwayBill> OrderEwayBills { get; set; } //by Shailesh
        public DbSet<GstPortalUomMapping> GstPortalUomMappings { get; set; }//by Shailesh
        public DbSet<YesterdayDemandForPR> YesterdayDemandForPRDB { get; set; }//by Shailesh
        public DbSet<YesterdayDemandPO_Mapping> YesterdayDemandPO_MappingDB { get; set; }//by Shailesh

        public DbSet<ChequeUpload> ChequeUploadDB { get; set; }

        public DbSet<ChqBookUpload> ChqBookUploads { get; set; }
        public DbSet<MonthPTR> MonthPTR { get; set; }
        public DbSet<WarehouseUpdateCapacity> WarehouseUpdateCapacities { get; set; }
        public DbSet<WarehouseHoliday> WarehouseHolidays { get; set; }
        public DbSet<WarehouseCapacity> WarehouseCapacities { get; set; }
        public DbSet<CustomerSalesReturnConfig> CustomerSalesReturnConfigs { get; set; }

        #region [GDN]
        public DbSet<GDNConversation> GDNConversations { get; set; }//by Shailesh
                                                                    //public DbSet<GoodsDescripancyNoteDtls> GoodsDescripancyNoteDtlDB { get; set; }//by Shailesh
                                                                    //by Shailesh//public DbSet<GoodsDescripancyNoteMst> GoodsDescripancyNoteMstDB { get; set; }//by Shailesh
        public DbSet<GoodsDescripancyNoteMaster> GoodsDescripancyNoteMasterDB { get; set; }
        public DbSet<GoodsDescripancyNoteDetail> GoodsDescripancyNoteDetailDB { get; set; }//by Shailesh



        #endregion

        //Supplier new 
        #region New Supplier on baording
        public DbSet<SupplierTemp> SupplierTempDB { get; set; }//by Shailesh
        public DbSet<SupplierOnboardHistory> SupplierOnboardHistoryDB { get; set; }//by Shailesh
        public DbSet<DepoMasterTemp> DepoMasterTempDB { get; set; }//by Shailesh
        public DbSet<SupplierDocument> SupplierDocumentDB { get; set; }//by Shailesh
        public DbSet<DepoDocument> DepoDocumentDB { get; set; }//by Shailesh



        #endregion

        #region GameModule  
        // Author : Praveen Goswami
        // Created Date : 20 Feb 2019
        public DbSet<GameAttemptedQuestion> GameAttemptedQuestionDB { get; set; }
        public DbSet<LevelCrossedDetails> LevelCrossedDetailsDB { get; set; }
        public DbSet<TotalEarnedPoints> TotalEarnedPointsDB { get; set; }
        public DbSet<GameWallet> GameWalletDB { get; set; }
        public DbSet<GameWarehouseWallet> GameWarehouseWalletDB { get; set; }
        public DbSet<GameCustomerWalletHistory> GameCustomerWalletHistoryDB { get; set; }
        #endregion
        #region WarehouseReport
        // Author : Praveen Goswami
        // Created Date : 15 April 2019
        public DbSet<WarehouseReport> WarehouseReportDB { get; set; }
        public DbSet<WarehouseReportCategory> WarehouseReportCategoryDB { get; set; }
        public DbSet<WarehouseReportDetails> WarehouseReportDetailsDB { get; set; }
        public DbSet<WarehouseReportHistoryDetails> WarehouseReportHistoryDetailsDB { get; set; }
        public DbSet<ItemDealPriceQty> ItemDealPriceQtyDB { get; set; } // by yogendra
        #endregion

        #endregion
        #region Cash Management DBSet
        public DbSet<CurrencyDenomination> CurrencyDenomination { get; set; }
        public DbSet<CurrencyCollection> CurrencyCollection { get; set; }
        public DbSet<CashCollection> CashCollection { get; set; }
        public DbSet<ChequeCollection> ChequeCollection { get; set; }
        public DbSet<OnlineCollection> OnlineCollection { get; set; }
        public DbSet<CashSettlement> CashSettlement { get; set; }
        public DbSet<CurrencySettlementSource> CurrencySettlementSource { get; set; }
        public DbSet<CurrencyHubStock> CurrencyHubStock { get; set; }
        public DbSet<HubCashCollection> HubCashCollection { get; set; }
        public DbSet<ExchangeHubCashCollection> ExchangeHubCashCollection { get; set; }
        public DbSet<CurrencySettlementImages> CurrencySettlementImages { get; set; }
        public DbSet<CurrencySettlementBank> CurrencySettlementBank { get; set; }
        public DbSet<AgentSettelment> AgentSettelment { get; set; }
        public DbSet<AgentCollection> AgentCollection { get; set; }
        public DbSet<CashBalanceCollection> CashBalanceCollection { get; set; }
        public DbSet<ChequeFineAppoved> ChequeFineAppoved { get; set; }
        public DbSet<CashBalanceVerify> CashBalanceVerify { get; set; }

        #endregion

        public DbSet<SaleDefaultCategory> SaleDefaultCategories { get; set; }
        public DbSet<CheckOut> CheckOuts { get; set; }
        public DbSet<CheckOutReason> CheckOutReasons { get; set; }

        #region Agentcommision 10/02/2020
        public DbSet<CityWiseActivationConfiguration> CityWiseActivationConfiguration { get; set; }
        public DbSet<AgentCommissionforCity> AgentCommissionforCityDB { get; set; }
        #endregion
        public DbSet<DeliveryCancelledOrderItem> DeliveryCancelledOrderItemDb { get; set; }
        public DbSet<SchedulerTask> SchedulerTask { get; set; }
        public DbSet<EmailRecipients> EmailRecipients { get; set; }
        public DbSet<TopSellingItem> TopSellingItem { get; set; }
        public DbSet<TopSellingItemScheduler> TopSellingItemScheduler { get; set; }

        public DbSet<NotificationUpdated> NotificationUpdatedDb { get; set; }
        public DbSet<StoreNotification> StoreNotifications { get; set; }
        public DbSet<ExecutiveDeviceNotification> ExecutiveDeviceNotifications { get; set; }
        public DbSet<PeopleGroupMapping> PeopleGroupMappings { get; set; }
        public DbSet<RegisteredApk> RegisteredApk { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DailyStock> DailyStock { get; set; }
        public DbSet<CompanyDetails> CompanyDetailsDB { get; set; }
        public DbSet<ConsumerCompanyDetails> ConsumerCompanyDetailDB { get; set; }
        public DbSet<ConsumerCreditnote> ConsumerCreditnoteDb { get; set; }
        public DbSet<ConsumerAddress> ConsumerAddressDb { get; set; }

        public DbSet<MemberShip> MemberShips { get; set; }
        public DbSet<CustomerMemberShipBucket> CustomerMemberShipBuckets { get; set; }
        public DbSet<ClusterStoreExecutiveHistory> ClusterStoreExecutiveHistories { get; set; }

        public DbSet<CRMNotifiationCustomer> CRMNotifiationCustomers { get; set; }

        public DbSet<NotificationScheduler> NotificationSchedulers { get; set; }

        #region BillDiscount
        public DbSet<BillDiscount> BillDiscountDb { get; set; }
        public DbSet<OfferItemsBillDiscount> OfferItemsBillDiscountDB { get; set; }
        public DbSet<BillDiscountOfferSection> BillDiscountOfferSectionDB { get; set; }
        public DbSet<CustomerAddress> CustomerAddressDB { get; set; }

        #endregion

        #region DistributorApp
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 30 Jan 2020
        /// </summary>
        //public DbSet<Distributor> DistributorDB { get; set; }
        public DbSet<DistributorVerification> DistributorVerificationDB { get; set; }

        public DbSet<DONPinCode> DONPinCodeDB { get; set; }
        public DbSet<DistributorCompanydetail> DistributorCompanydetailDB { get; set; }
        #endregion
        #region Gullak
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 30 Jan 2020
        /// </summary>
        public DbSet<Gullak> GullakDB { get; set; }
        public DbSet<GullakInPayment> GullakInPaymentDB { get; set; }
        public DbSet<GullakTransaction> GullakTransactionDB { get; set; }
        public DbSet<GullakCashBack> GullakCashBackDB { get; set; }

        #endregion
        #region GrowthModule
        /// <summary>
        /// Author : Praveen Goswami
        /// Created Date : 11 June 2019
        /// </summary>
        public DbSet<GMLegalDocMaster> GMLegalDocMasterDB { get; set; }
        public DbSet<GMTaskListMaster> GMTaskListMasterDB { get; set; }
        public DbSet<GMDivisionMaster> GMDivisionMasterDB { get; set; }
        public DbSet<GMProductPartnerMaster> GMProductPartnerMasterDB { get; set; }
        public DbSet<GMCityMappingMaster> GMCityMappingMasterDB { get; set; }
        public DbSet<GMWarehouseProgress> GMWarehouseProgressDB { get; set; }
        public DbSet<GMInfrastructure> GMInfrastructureDB { get; set; }
        public DbSet<GMInfrastructureLegal> GMInfrastructureLegalDB { get; set; }
        public DbSet<GMInfrastructureTask> GMInfrastructureTaskDB { get; set; }
        public DbSet<GMPeople> GMPeopleDB { get; set; }
        public DbSet<GMProductPartners> GMProductPartnersDB { get; set; }
        public DbSet<GMTrainingDevlopment> GMTrainingDevlopmentDB { get; set; }
        public DbSet<GMCityMapping> GMCityMappingDB { get; set; }
        public DbSet<GMInfraTaskImages> GMInfraTaskImagesDB { get; set; }
        public DbSet<GMInfraLegalImages> GMInfraLegalImagesDB { get; set; }
        public DbSet<GMTrainingDevlopmentImages> GMTrainingDevlopmentImagesDB { get; set; }
        public DbSet<GMLogin> GMLoginDB { get; set; }
        public DbSet<WarehousePermission> WarehousePermissionDB { get; set; }

        public DbSet<InactivePeople> InactivePeopleDB { get; set; }

        #endregion
        #region New App home        
        public DbSet<NoFlashDealImage> NoFlashDealImage { get; set; }
        public DbSet<AppHomeSections> AppHomeSectionsDB { get; set; }
        public DbSet<AppHomeSectionItems> AppHomeSectionItemsDB { get; set; }
        public DbSet<PublishAppHome> PublishAppHomeDB { get; set; }
        #endregion

        #region Store
        public DbSet<Store> StoreDB { get; set; }
        public DbSet<StoreBrand> StoreBrandDB { get; set; }
        public DbSet<OrderInvoice> OrderInvoiceDB { get; set; }
        #endregion
        #region CustomerReferral
        public DbSet<CustomerReferralConfiguration> CustomerReferralConfigurationDb { get; set; }

        //public DbSet<CustomerReferralStatus> CustomerReferralStatusDb { get; set; }

        public DbSet<ReferralWallet> ReferralWalletDb { get; set; }
        public DbSet<CustomerReferralStatus> CustomerReferralStatusDb { get; set; }
        #endregion

        #region  Inventory Forecasting
        public DbSet<BuyerForecastUploder> BuyerForecastUploderDb { get; set; }
        public DbSet<BuyerForecastUploderDetail> BuyerForecastUploderDetailDb { get; set; }
        public DbSet<BrandForecastDetail> BrandForecastDetailDb { get; set; }
        public DbSet<ItemForecastDetail> ItemForecastDetailDb { get; set; }
        public DbSet<SalesIntentRequest> SalesIntentRequestDb { get; set; }
        public DbSet<ItemForecastPRRequest> ItemForecastPRRequestDb { get; set; }
        public DbSet<ItemForeCastCity> ItemForeCastCitys { get; set; }
        public DbSet<SystemItemForecast> SystemItemForecasts { get; set; }
        public DbSet<FutureMrpMapping> FutureMrpMappings { get; set; }
        public DbSet<ForecastInventoryDay> ForecastInventoryDayDb { get; set; }

        public DbSet<ItemFullfillmentComment> ItemFullfillmentCommentDB { get; set; }
        public DbSet<ItemForecastPRRequestSaveDraft> ItemForecastPRRequestSaveDraftDB { get; set; }

        public DbSet<BrandIndentrestriction> BrandIndentrestrictionDB { get; set; }
        public DbSet<HistoryPurchaseForecast> HistoryPurchaseForecastDB { get; set; }

        public DbSet<InventoryRestriction> InventoryRestrictionDB { get; set; }

        #endregion


        #region  Multiple cashier login - Cash Collection
        public DbSet<OTPVerification> OTPVerificationDb { get; set; }
        public DbSet<CMSCashierVerification> CMSCashierVerificationDB { get; set; }
        public DbSet<CMSPageAccess> CMSPageAccessDB { get; set; }
        #endregion



        public DbSet<ReturnChequeCollection> ReturnChequeCollection { get; set; }

        public DbSet<CityBaseCustomerReward> CityBaseCustomerRewards { get; set; }
        public DbSet<RewardedCustomer> RewardedCustomers { get; set; }

        public DbSet<ItemFutureEarning> ItemFutureEarning { get; set; }
        public DbSet<SupplierPaymentRequest> SupplierPaymentRequestDc { get; set; }
        public DbSet<RTVMaster> RTVMasterDB { get; set; }
        public DbSet<RTVMasterDetail> RTVMasterDetailDB { get; set; }
        public DbSet<MurliStory> MurliStoryDB { get; set; }
        public DbSet<MurliStoryPage> MurliStoryPageDB { get; set; }
        public DbSet<CustomerSms> CustomerSmsDB { get; set; }
        public DbSet<CustomerDocument> CustomerDocumentDB { get; set; }
        public DbSet<CustomerFeedbackQuestion> CustomerFeedbackQuestionDB { get; set; } // By sudhir
        public DbSet<CustomerOrderFeedback> CustomerOrderFeedbackDB { get; set; } // By sudhir
        public DbSet<RegionZone> Regions { get; set; } // By pooja.Z
        public DbSet<PageRequest> pageRequests { get; set; }
        public DbSet<CustomizedPrepaidOrders> CustomizedPrepaidOrders { get; set; }
        public DbSet<CompanyWheelConfiguration> CompanyWheelConfiguration { get; set; }
        public DbSet<WheelPointWeightPercentConfig> WheelPointWeightPercentConfig { get; set; }

        public DbSet<ManualSalesOrder> ManualSalesOrderDB { get; set; }

        public DbSet<GrDraftInvoice> GrDraftInvoiceDB { get; set; }
        public DbSet<PRApproval> PRApprovalDB { get; set; }
        public DbSet<CreatePRApproval> CreatePRApprovalDB { get; set; }

        public DbSet<OrderRedispatchCountApproval> orderRedispatchCountApprovalDB { get; set; }

        public DbSet<IRPaymentDetailHistory> IRPaymentDetailHistoryDB { get; set; }
        public DbSet<AgentTDS> agentTDSDB { get; set; }
        public DbSet<PoEditHistory> PoEditHistoryDB { get; set; }
        public DbSet<ClusterDashBoardCustomerLogin> ClusterDashBoardCustomerLogin { get; set; }//Rakshit
        public DbSet<ConfigurationPlan> ConfigurationPlan { get; set; }//Rakshit
        public DbSet<RegisteredCustomer> RegisteredCustomer { get; set; }//Rakshit
        public DbSet<CustomerPayment> CustomerPayment { get; set; }//Rakshit
        public DbSet<GSTChangeRequest> GSTChangeRequestDB { get; set; }

        public DbSet<CustGSTverifiedRequest> CustGSTverifiedRequestDB { get; set; }
        public DbSet<InventoryCycleConfiguration> InventoryCycleConfigurationDB { get; set; }

        public DbSet<IncidentReport> IncidentReport { get; set; }

        public DbSet<DBoyAssignmentDeposit> DboyAssignmentDepositDB { get; set; }
        public DbSet<DBoyAssignmentDepositMaster> DBoyAssignmentDepositMasterDB { get; set; }

        public DbSet<ApprovalConfiguration> ApprovalConfigurations { get; set; }
        public DbSet<DamageRequest> DamageRequests { get; set; }
        public DbSet<ClearanceRequest> ClearanceRequests { get; set; }
        public DbSet<CustomerBrandAcess> CustomerBrandAcessDB { get; set; }
        public DbSet<CustomerRetentionConfiguration> CustomerRetentionConfigurations { get; set; }
        public DbSet<DeliveryCanceledRequestHistory> DeliveryCanceledRequestHistoryDb { get; set; }
        public DbSet<TripPlannerVehicles> TripPlannerVehicleDb { get; set; }//by sudhir
        public DbSet<TripPlannerVehicleHistory> TripPlannerVehicleHistoryDb { get; set; }//by sudhir
        public DbSet<ReadyToPickOrderDetails> ReadyToPickOrderDetailDb { get; set; }
        public DbSet<TripPickerAssignmentMapping> TripPickerAssignmentMapping { get; set; }
        public DbSet<CustomerUnloadLocation> CustomerUnloadLocationDb { get; set; }//by Sudhir
        public DbSet<TripPlannerApprovalRequest> TripPlannerApprovalRequestDb { get; set; }
        public DbSet<CustomerAddressOperationRequest> CustomerAddressOperationRequestDb { get; set; }
        public DbSet<CustomerStatusHistory> CustomerStatusHistoryDb { get; set; }


        #region expense
        public DbSet<Expense> ExpenseDB { get; set; }
        public DbSet<ExpenseTDSMaster> ExpenseTDSMasterDB { get; set; }
        public DbSet<ExpenseGSTMaster> ExpenceGSTMasterDB { get; set; }
        public DbSet<ExpenseDetails> ExpenseDetailsDB { get; set; }
        public DbSet<BookExpense> BookExpenseDB { get; set; }
        public DbSet<BookExpenseDetail> BookExpenseDetailDB { get; set; }
        public DbSet<WorkingCompany> WorkingCompanyDB { get; set; }
        public DbSet<WorkingLocation> WorkingLocationDB { get; set; }
        public DbSet<Vendor> VendorDB { get; set; }
        public DbSet<BookExpensePayment> BookExpensePaymentDB { get; set; }
        #endregion


        #region Agent Commission Module
        public DbSet<AgentCommissionPayment> AgentCommissionPaymentDB { get; set; }

        public DbSet<AgentPaymentSettlement> AgentCommissionPaymentSettlementDB { get; set; }

        #endregion


        #region For KK KKReturnReplaceRequests
        public DbSet<KKReturnReplaceRequest> KKReturnReplaceRequests { get; set; }
        public DbSet<KKReturnReplaceDetail> KKReturnReplaceDetails { get; set; }

        public DbSet<KKRequestReplaceHistory> KKRequestReplaceHistorys { get; set; }

        public DbSet<KKReturnOrderStatus> DbKKReturnOrderStatus { get; set; }
        public DbSet<ReturnOrderBillDiscount> ReturnOrderBillDiscounts { get; set; }

        #endregion
        #region  Multi Stock Transaction (Configuration) Dbset : 03-03-2020
        public DbSet<StockTransactionMaster> StockTransactionMasterDB { get; set; }
        public DbSet<StockTransactionCondition> StockTransactionConditionDB { get; set; }
        public DbSet<StockTransactionSpParameter> StockTransactionSpParameterDB { get; set; }
        public DbSet<ManualStockUpdateRequest> ManualStockUpdateRequestDB { get; set; }

        public DbSet<StockConfigMaster> StockConfigMasterDB { get; set; }
        public DbSet<StockConfigDetail> StockConfigDetailDB { get; set; }



        #region  Multiple Stock (Table)Dbset   Date: 03-03-2020
        // public DbSet<DamagedStock> DamagedStockDB { get; set; }
        public DbSet<DeliveredStock> DeliveredStockDB { get; set; }
        public DbSet<ExpiredStock> ExpiredStockDB { get; set; }
        // public DbSet<FreebieStock> FreebieStockDB { get; set; }
        public DbSet<InReceivedStock> InReceivedStockDB { get; set; }
        public DbSet<InTransitStock> InTransitStockDB { get; set; }
        public DbSet<ReservedStock> ReservedStockDB { get; set; }
        public DbSet<RTDStock> RTDStockDB { get; set; }
        public DbSet<RTVStock> RTVStockDB { get; set; }
        public DbSet<StockDetail> StockDetailDB { get; set; }
        public DbSet<VirtualStock> VirtualStockDB { get; set; }
        //public DbSet<MultiMrpTransferStock> MultiMrpTransferStockDB { get; set; }

        public DbSet<DeliveryCancelStock> DeliveryCancelStockDB { get; set; }
        public DbSet<DeliveryRedispatchStock> DeliveryRedispatchStockDB { get; set; }
        public DbSet<IssuedStock> IssuedStockDB { get; set; }
        public DbSet<ITIssueStock> ITIssueStockDB { get; set; }
        public DbSet<LostAndFoundStock> LostAndFoundStockDB { get; set; }
        public DbSet<PlannedStock> PlannedStockDB { get; set; }
        public DbSet<QuarantineStock> QuarantineStockDB { get; set; }
        public DbSet<ShippedStock> ShippedStockDB { get; set; }
        public DbSet<DeliveryCanceledRequestStock> DeliveryCanceledRequestStockDB { get; set; }
        public DbSet<ClearanceLiveItem> ClearanceLiveItemDB { get; set; }

        public DbSet<ClearanceLiveItemOffer> ClearanceLiveItemOffers { get; set; }
        public DbSet<ClearancePlannedStock> ClearancePlannedStocks { get; set; }



        #endregion

        #endregion



        #region Ticket
        public DbSet<TicketCategory> TicketCategory { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketsAssigned> TicketsAssigned { get; set; }
        public DbSet<TicketActivityLog> TicketActivityLog { get; set; }
        public DbSet<SubCatTarget> subCatTargets { get; set; }
        public DbSet<SubCatTargetSpacificCust> SubCatTargetSpacificCusts { get; set; }
        public DbSet<SubCatTargetBrand> SubCatTargetBrands { get; set; }
        public DbSet<SubCatTargetDetail> SubCatTargetDetails { get; set; }
        public DbSet<SubCategoryTargetCustomer> SubCategoryTargetCustomers { get; set; }
        public DbSet<TargetCustomerBrand> TargetCustomerBrands { get; set; }
        public DbSet<TargetCustomerItem> TargetCustomerItems { get; set; }

        #endregion


        #region Sales Commission       
        public DbSet<SalesComissionTransaction> SalesComissionTransactions { get; set; }
        public DbSet<SalesComTransDetail> SalesComTransDetails { get; set; }
        public DbSet<ExecutiveSalesCommission> ExecutiveSalesCommission { get; set; }
        public DbSet<SalesCommissionEventMaster> SalesCommissionEventMasters { get; set; }
        public DbSet<SalesCommissionCatMaster> SalesCommissionCatMasters { get; set; }

        #endregion
        public DbSet<ErpPageVisits> ErpPageVisits { get; set; }

        #region Cluster Agent and Vehicle
        public DbSet<ClusterAgent> ClusterAgent { get; set; }
        public DbSet<ClusterVehicle> ClusterVehicle { get; set; }

        #endregion


        public DbSet<KisanDanDescription> KisanDanDescription { get; set; }
        public DbSet<KisanDaanGallary> KisanDaanGallary { get; set; }
        public DbSet<Ladger> LadgerDB { get; set; }
        public DbSet<LadgerEntry> LadgerEntryDB { get; set; }
        public DbSet<VoucherType> VoucherTypeDB { get; set; }
        public DbSet<LadgerType> LadgerTypeDB { get; set; }
        public DbSet<FiscalYear> FiscalYearDB { get; set; }
        public DbSet<LedgerOpeningBalance> LedgerOpeningBalanceDB { get; set; }
        public DbSet<AccountGroup> AccountGroups { get; set; }
        public DbSet<Voucher> VoucherDB { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<AccountClassification> AccountClassifications { get; set; }
        public DbSet<IRPaymentSummary> IRPaymentSummaryDB { get; set; }

        public DbSet<RequestAccess> requestAccess { get; set; }
        public DbSet<BrandBuyer> BrandBuyerDB { get; set; }
        public DbSet<BrandBuyerHistory> BrandBuyerHistory { get; set; }
        //public DbSet<RegionZone> Regions { get; set; } // By pooja.Z

        public DbSet<MasterExportRequest> MasterExportRequestDB { get; set; }
        public DbSet<CustomerSegment> CustomerSegmentDb { get; set; }
        public DbSet<RequestRole> requestRoles { get; set; }

        public DbSet<IRPaymentDetails> IRPaymentDetailsDB { get; set; }
        public DbSet<OldAssignmentPayment> OldAssignmentPaymentDB { get; set; }
        public DbSet<OldAssignmentPaymentDetails> OldAssignmentPaymentDetailsDB { get; set; }
        public DbSet<DeliveryCancelledDraft> DeliveryCancelledDraftDB { get; set; }

        #region Permission
        public DbSet<PageMaster> PageMaster { get; set; }
        public DbSet<ButtonMaster> ButtonMaster { get; set; }
        public DbSet<PageButton> PageButton { get; set; }
        public DbSet<RolePagePermission> RolePagePermission { get; set; }
        public DbSet<PeoplePageAccessPermission> PeoplePageAccessPermission { get; set; }
        public DbSet<OverrideRolePagePermission> OverrideRolePagePermission { get; set; }

        #endregion

        public DbSet<ExportMasterOwner> MasterOwners { get; set; }
        public DbSet<ExportMaster> Masters { get; set; }
        public DbSet<LedgerConfigurationMaster> LedgerConfigurationMasterDB { get; set; }
        public DbSet<LedgerConfigurationMasterCondition> LedgerConfigurationMasterConditionDB { get; set; }
        public DbSet<LedgerConfigurationDetail> LedgerConfigurationDetailDB { get; set; }
        public DbSet<LedgerConfigurationParmameter> LedgerConfigurationParmameterDB { get; set; }
        public DbSet<CustomerLedgerConsentMaster> CustomerLedgerConsentMasterDB { get; set; }
        public DbSet<CustomerLedgerConsentDetails> CustomerLedgerConsentDetailsDB { get; set; }
        public DbSet<CustomerConsentConfiguration> CustomerOtpConfigurationDB { get; set; }

        #region
        public DbSet<CustomerKisanDan> CustomerKisanDan { get; set; }
        public DbSet<kisanDanMaster> kisanDanMaster { get; set; }
        #endregion

        public DbSet<AssignmentCommission> AssignmentCommissionDb { get; set; }
        public DbSet<AssignmentCommissionDetail> AssignmentCommissionDetailDb { get; set; }

        public DbSet<PRPaymentSummary> PRPaymentSummaryDB { get; set; }

        public DbSet<PurchaseRequestPayment> PurchaseRequestPaymentsDB { get; set; }
        public DbSet<PrPaymentTransfer> PrPaymentTransferDB { get; set; }

        public DbSet<PrimeCustomer> PrimeCustomers { get; set; }

        public DbSet<PrimeRegistrationDetail> PrimeRegistrationDetail { get; set; }
        public DbSet<PrimeItemDetail> PrimeItemDetails { get; set; }

        public DbSet<DealItem> DealItems { get; set; }

        public DbSet<BlockBrand> BlockBrands { get; set; }
        //mbd 20 July 2022
        public DbSet<CustomerExecutiveMappingsReschedule> CustomerExecutiveMappingsRescheduleDb { get; set; }

        public DbSet<CustomerExecutiveMappingsBeatEdit> CustomerExecutiveMappingsBeatEditDb { get; set; }
        public DbSet<ExecutiveBeatHistory> ExecutiveBeatHistories { get; set; }
        public DbSet<ExecutiveStoreChangeHistory> ExecutiveStoreChangeHistories { get; set; }
        public DbSet<StoreProductiveOrders> StoreProductiveOrders { get; set; }
        public DbSet<SalesGroup> SalesGroupDb { get; set; }
        public DbSet<SalesGroupCustomer> SalesGroupCustomerDb { get; set; }


        #region Raorpay
        public DbSet<RazorpayVirtualAccounts> RazorpayVirtualAccounts { get; set; }
        public DbSet<RazorPayCustomerReqResponse> RazorPayCustomerReqResponse { get; set; }
        public DbSet<RazorpayWebhookRequest> RazorpayWebhookRequest { get; set; }

        #endregion


        public DbSet<CaseMemoDetail> caseMemoDetails { get; set; }
        public DbSet<ImportedContacts> ImportedContacts { get; set; }
        public DbSet<InventCycleBatchSettlement> InventCycleBatchSettlement { get; set; }

        #region FinBox
        public DbSet<FinBoxRequestResponse> FinBoxRequestResponse { get; set; }
        public DbSet<FinBoxConfig> FinBoxConfigs { get; set; }
        public DbSet<Webhook> Webhook { get; set; }

        #endregion

        #region Driver Master/ Dboy Master
        public DbSet<DriverMaster> DriverMasters { get; set; }
        public DbSet<DboyMaster> DboyMasters { get; set; }
        #endregion

        #region PO Adjustment
        public DbSet<AdjustmentPODetail> AdjustmentPODetails { get; set; }
        #endregion

        #region TripPlanner
        public DbSet<TripPlannerMaster> TripPlannerMasters { get; set; }
        public DbSet<TripPlannerDetail> TripPlannerDetails { get; set; }
        public DbSet<TripPlannerOrder> TripPlannerOrders { get; set; }
        public DbSet<TripPlannerConfirmedMaster> TripPlannerConfirmedMasters { get; set; }
        public DbSet<TripPlannerConfirmedDetail> TripPlannerConfirmedDetails { get; set; }
        public DbSet<TripPlannerConfirmedOrder> TripPlannerConfirmedOrders { get; set; }
        public DbSet<TripPlannerDroppedOrder> TripPlannerDroppedOrders { get; set; }
        public DbSet<TripPlannerItemCheckList> TripPlannerItemCheckListDb { get; set; }
        public DbSet<TripPaymentResponseApps> TripPaymentResponseAppDb { get; set; }
        public DbSet<TripPlannerVechicleAttandance> TripPlannerVechicleAttandanceDb { get; set; }//by Anshika 
        public DbSet<VehicleType> VehicleTypesDb { get; set; }//by Anshika 
        public DbSet<WarehouseUtilization> WarehouseUtilizationDb { get; set; }//by Anshika
        public DbSet<WarehouseUtilizationDetails> WarehouseUtilizationDetailsDb { get; set; }//by Anshika
        public DbSet<TransporterPayment> TransporterPaymentDb { get; set; }//by Anshika
        public DbSet<TransporterPaymentDetail> TransporterPaymentDetailDb { get; set; }//by Anshika
        public DbSet<TransporterPaymentActionHistory> TransporterPaymentActionHistoriesDb { get; set; }
        public DbSet<LMDVendor> LMDVendorDb { get; set; }//by Anshika

        #endregion

        #region SalesKPI
        public DbSet<SalesKPI> SalesKPIs { get; set; }
        public DbSet<SalesKPISlab> SalesKPISlabs { get; set; }
        public DbSet<SalesPersonKPI> SalesPersonKPIs { get; set; }
        #endregion

        public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }
        public DbSet<WhatsAppTemplateVariableDetail> WhatsAppTemplateVariableDetails { get; set; }
        public DbSet<WhatsAppTemplateValConfiguration> WhatsAppTemplateValConfigurations { get; set; }

        public DbSet<CRMWhatsappMessage> CRMWhatsappMessages { get; set; }
        public DbSet<WhatsAppGroupNotificationMaster> WhatsAppGroupNotificationMasters { get; set; }
        public DbSet<WhatsAppGroupNotificationDetails> WhatsAppGroupNotificationDetails { get; set; }

        public DbSet<ExecutiveAttendance> ExecutiveAttendances { get; set; }
        public DbSet<ItemIncentiveClassificationMasterLog> ItemIncentiveClassificationMasterLogs { get; set; }
        public DbSet<ItemCityWiseIncentiveClassificationLog> ItemCityWiseIncentiveClassificationLogs { get; set; }

        public DbSet<ClusterHoliday> ClusterHolidays { get; set; }
        public DbSet<CustomerHoliday> CustomerHolidays { get; set; }
        public DbSet<ClonePo> ClonePos { get; set; }

        public DbSet<JITConfiguration> JITConfigurations { get; set; }
        public DbSet<InventroyProvisioningConfiguration> InventroyProvisioningConfigurationDB { get; set; }
        public DbSet<InventroyProvisioningData> InventroyProvisioningDataDB { get; set; }
        public DbSet<ItemFrontMarginClosing> ItemFrontMarginClosingDB { get; set; }

        // gamification
        public DbSet<GameConditionMaster> GameConditionMasters { get; set; }
        public DbSet<GameBucketReward> GameBucketRewards { get; set; }
        public DbSet<BucketRewardCondition> BucketRewardConditions { get; set; }
        public DbSet<CustomerBucketGame> CustomerBucketGames { get; set; }
        public DbSet<CustomerBucketOrderDetail> CustomerBucketOrderDetails { get; set; }
        public DbSet<GameCurrentLevelProgress> GameCurrentLevelProgresses { get; set; }
        public DbSet<GameStreakLevelConfigMaster> GameStreakLevelConfigMasters { get; set; }
        public DbSet<GameStreakLevelConfigDetail> GameStreakLevelConfigDetails { get; set; }
        public DbSet<InventroyProvisioningConfigurationDetails> InventroyProvisioningConfigurationDetails { get; set; }
        public DbSet<ROCBucket> ROCBuckets { get; set; }
        public DbSet<ROCItemRawData> ROCItemRawDatas { get; set; }
        public DbSet<ItemTagging> ItemTaggings { get; set; }
        public DbSet<CRM> CRMs { get; set; }
        public DbSet<CRMDetail> CRMDetails { get; set; }
        public DbSet<CRMPlatform> CRMPlatforms { get; set; }
        public DbSet<CRMPlatformMapping> CRMPlatformMappings { get; set; }
        public DbSet<CRMCustomerData> CRMCustomerDatas { get; set; }

        #endregion

        public DbSet<SupplierRetailerMapping> SupplierRetailerMappings { get; set; }

        public DbSet<IrExtendInvoiceDateApproval> IrExtendInvoiceDateApprovals { get; set; }


        #region Non Revenue Orders
        //public DbSet<NonRevenueOrderMaster> NonRevenueOrderMasters { get; set; }
        // public DbSet<NonRevenueOrderDetail> NonRevenueOrderDetails { get; set; }
        public DbSet<NonRevenueOrderStock> NonRevenueOrderStocks { get; set; }
        public DbSet<NonRevenueOrderStockHistory> NonRevenueOrderStockHistories { get; set; }

        #endregion


        public DbSet<SKPKPPCommision> SKPKPPCommisions { get; set; }
        public DbSet<PackageMaterialCost> PackageMaterialCosts { get; set; }

        public DbSet<AccountTallyLadger> AccountTallyLadgers { get; set; }
        public DbSet<AccountDepartment> AccountDepartments { get; set; }
        public DbSet<AccountVertical> AccountVerticals { get; set; }
        public DbSet<AccountCanvasHead> AccountCanvasHeads { get; set; }
        public DbSet<AccountExpenseMISHead> AccountExpenseMISHeads { get; set; }
        public DbSet<AccountMISHead> AccountMISHeads { get; set; }
        public DbSet<AccountCostMISHead> AccountCostMISHeads { get; set; }
        public DbSet<AccountFinancialHead> AccountFinancialHeads { get; set; }
        public DbSet<AccountCM5Head> AccountCM5Heads { get; set; }
        public DbSet<AccountMISDataUpload> AccountMISDataUploads { get; set; }
        public DbSet<AccountMISInsert> AccountMISInserts { get; set; }
        public DbSet<TransporterPaymentDetailDoc> TransporterPaymentDetailDocs { get; set; }


        #region Product Performance Dashboard
        public DbSet<QuadrantOverAllMedian> QuadrantOverAllMedians { get; set; }
        public DbSet<QuadrantPercentage> QuadrantPercentage { get; set; }
        public DbSet<QuadrantDetail> QuadrantDetails { get; set; }
        public DbSet<QuadrantItemDetailHistory> QuadrantItemDetailHistories { get; set; }
        #endregion

        #region DFR Pending order details
        public DbSet<DFRPendingOrderDetails> DFRPendingOrderDetails { get; set; }

        #endregion

        #region Order2OrderReconcillation
        public DbSet<OrderReconcillationFileUploadDetail> OrderReconcillationFileUploadDetails { get; set; }
        public DbSet<OrderReconcillationDetail> OrderReconcillationDetails { get; set; }
        public DbSet<OrderReconcillationHistory> orderReconcillationHistories { get; set; }
        public DbSet<OrderReconcillationBankDetail> orderReconcillationBankDetails { get; set; }
        public DbSet<OrderReconcillationDirectUdharAmount> OrderReconcillationDirectUdharAmounts { get; set; }
        #endregion

        public DbSet<CalculateItemPurchasePrice> CalculateItemPurchasePrices { get; set; }
        public DbSet<CalculateItemPriceHistory> CalculateItemPriceHistories { get; set; }
        public DbSet<SalesBuyerForcastConfig> SalesBuyerForcastConfigs { get; set; }

        public DbSet<SubCategoryStopPo> SubCategoryStopPos { get; set; }
        //Loan Management ArthMate
        public DbSet<LeadMaster> LeadMasters { get; set; }
        public DbSet<ArthMateActivityMaster> ArthMateActivityMasters { get; set; }
        public DbSet<ArthMateDocumentMaster> ArthMateDocumentMasters { get; set; }
        public DbSet<LeadBackgroundRun> LeadBackgroundRuns { get; set; }


        //djfjd
        public DbSet<ArthmateReqResp> ArthmateReqResp { get; set; }
        public DbSet<ArthmateApiConfig> ArthmateApiConfig { get; set; }
        public DbSet<LeadActivityMasterProgress> LeadActivityMasterProgress { get; set; }
        public DbSet<LeadLoanDocument> LeadDocument { get; set; }
        public DbSet<LeadActivityHistory> LeadActivityHistories { get; set; }
        public DbSet<ArthMateActivitySequence> ArthMateActivitySequence { get; set; }
        public DbSet<LeadPanDetailMapping> LeadPanDetailMapping { get; set; }
        public DbSet<LeadPanDetail> LeadPanDetail { get; set; }
        public DbSet<CoLenderResponse> CoLenderResponse { get; set; }
        public DbSet<AScore> AScore { get; set; }
        public DbSet<CeplrBankList> CeplrBankList { get; set; }
        public DbSet<KYCValidationResponse> KYCValidationResponse { get; set; }

        public DbSet<CeplrPdfReports> CeplrPdfReports { get; set; }
        public DbSet<LoanConfiguration> LoanConfiguration { get; set; }
        public DbSet<LeadLoan> LeadLoan { get; set; }
        public DbSet<ArthmateDisbursement> ArthmateDisbursements { get; set; }
        public DbSet<RepaymentSchedule> RepaymentSchedule { get; set; }

        public DbSet<eSignDetail> eSignDetail { get; set; }
        public DbSet<CompositeDisbursementWebhookResponse> CompositeDisbursementWebhookResponse { get; set; }
        public DbSet<LeadActivityProgressesHistory> leadActivityProgressesHistories { get; set; }

        public DbSet<LoanInsuranceConfiguration> LoanInsuranceConfiguration { get; set; }
        public DbSet<ArthmateNegativePincodeAreaMaster> ArthmateNegativePincodeAreaMaster { get; set; }
        public DbSet<ArthmateRepaymentUpdate> ArthmateRepaymentUpdate { get; set; }

        public DbSet<SalesPersonOTPDetails> SalesPersonOTPDetails { get; set; }
        public DbSet<LeadBankStatement> LeadBankStatement { get; set; }
        public DbSet<SeasonalConfig> seasonalConfigs { get; set; }
        public DbSet<ExclusiveOfferGroup> ExclusiveOfferGroups { get; set; }
        public DbSet<BackendOrderBillDiscountConfiguration> BackendOrderBillDiscountConfigurations { get; set; }
        public DbSet<BackendOrderBillDiscountDetails> BackendOrderBillDiscountDetails { get; set; }
        public DbSet<ChannelMaster> ChannelMasters { get; set; }
        public DbSet<StorePriceConfiguration> StorePriceConfigurationDb { get; set; }
        public DbSet<ScaleUpConfig> ScaleUpConfig { get; set; }
        public DbSet<CustomerChannelMapping> CustomerChannelMappings { get; set; }
        public DbSet<ScaleUpCustomer> ScaleUpCustomers { get; set; }
        public DbSet<ArthmateSlaLbaStampDetail> ArthmateSlaLbaStampDetail { get; set; }
        public DbSet<StoreCreditLimit> StoreCreditLimitDb { get; set; }
        public DbSet<PayLaterCollection> PayLaterCollectionDb { get; set; }
        public DbSet<PayLaterCollectionHistory> PayLaterCollectionHistoryDb { get; set; }
        public DbSet<ReadyToPickOrder> ReadyToPickOrders { get; set; }
        public DbSet<ReadyToPickHoldOrder> ReadyToPickHoldOrders { get; set; }
        public DbSet<PayLaterRequestResponseMsg> PayLaterRequestResponseMsgs { get; set; }
        public DbSet<StoreWarehouseOpeningDetail> StoreWarehouseOpeningDetailDb { get; set; }
        public DbSet<PriorityWarehouseStore> PriorityWarehouseStores { get; set; }
        public DbSet<PriorityWarehouse> PriorityWarehouses { get; set; }
        public DbSet<RazorpayOrder> RazorpayOrders { get; set; }
        public DbSet<GrQualityInvoice> GrQualityInvoices { get; set; }
        public DbSet<GrQualityConfiguration> GrQualityConfigurations { get; set; }
        public DbSet<MRPMedia> MRPMedias { get; set; }
        public DbSet<ConsumerRazorPayOrder> ConsumerRazorPayOrders { get; set; }


        #region Zila Models Starts 
        public DbSet<ZilaTripMaster> ZilaTripMasters { get; set; }
        public DbSet<ZilaTripDetail> ZilaTripDetails { get; set; }
        public DbSet<ZilaTripOrder> ZilaTripOrders { get; set; }
        public DbSet<ZilaTripVehicle> ZilaTripVehicles { get; set; }


        #endregion Zila Models Ends

        public DbSet<WarehouseQrDevice> WarehouseQrDevices { get; set; }


        #region RazorPayPos
        public DbSet<WarehousePosMachine> WarehousePosMachines { get; set; }
        public DbSet<RazorPayPosCredential> RazorPayPosCredentials { get; set; }
        public DbSet<RazorPayPosNotificationRequest> RazorPayPosNotificationRequests { get; set; }
        #endregion
    }
}