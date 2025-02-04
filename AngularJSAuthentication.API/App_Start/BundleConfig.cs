﻿using System.Configuration;
using System.Web.Optimization;

namespace AngularJSAuthentication.API.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Setting the bundle to use CDN path.  
            bundles.UseCdn = true;

            //Referencing the CDN path for the styleBundle (bootStrap).  
            bundles.Add(new StyleBundle("~/bundle/css", "http://netdna.bootstrapcdn.com/bootstrap/3.0.3/css/bootstrap.min.css"));



            //referencing the our page specific javascript files.  
            bundles.Add(new ScriptBundle("~/bundles/controllerJs").Include("~/controller/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/controllerLogJs").Include("~/controller/Logs/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/controllerPermissionJs").Include("~/controller/permission/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/controllerCashMgmtJs").Include("~/controller/CashManagement/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/servicesJs").Include("~/services/*.js"));


            bundles.Add(new ScriptBundle("~/bundles/rptJS").Include("~/ReportsControllers/Comparison1Ctrl.js"));


            bundles.Add(new ScriptBundle("~/bundles/workJS")
                           .Include("~/services/weektasktimesheetController.js")
                          .Include("~/controller/weektimesheetController.js")
               );

            bundles.Add(new ScriptBundle("~/bundles/AllNewJs")
                  .Include("~/services/GroupSmsServices.js")
                  .Include("~/controller/GroupSMSController.js")
                  .Include("~/controller/TargetAllocatonBandsListController.js")
                  .Include("~/controller/BudgetAllocationController.js")
                  .Include("~/controller/TargetBandAllocationController.js")
                  .Include("~/controller/TargetDashboardController.js")
                  .Include("~/controller/multipleimageController.js")
                  .Include("~/services/CurrencyStockService.js")
                  .Include("~/services/EditPriceService.js")
                  .Include("~/controller/editPriceController.js")
                  .Include("~/services/CurrentStockService.js")
                  .Include("~/services/ItemPramotionService.js")
                  .Include("~/services/DemandService.js")
                  .Include("~/services/authInterceptorService.js")
                  .Include("~/services/authService.js")
                  .Include("~/services/ordersService.js")
                  .Include("~/services/tokensManagerService.js")
                  .Include("~/services/projectsService.js")
                  .Include("~/services/customerService.js")
                  .Include("~/services/tasktypesService.js")
                  .Include("~/services/tasksService.js")
                  .Include("~/services/ClientProjectService.js")
                  .Include("~/services/peoplesService.js")
                  .Include("~/services/RoleService.js")
                  .Include("~/services/MessageService.js")
                  .Include("~/services/DeliveryChargeService.js")
                  .Include("~/services/SalesService.js")
                  .Include("~/services/Service.js")
                  .Include("~/services/DepartmentService.js")
                  .Include("~/services/designationservice.js")
                  .Include("~/controller/ReqServiceController.js")
                  .Include("~/services/settingService.js")
                  .Include("~/services/profilesService.js")
                  .Include("~/services/InvoiceProductService.js")
                  .Include("~/services/invoiceService.js")
                  .Include("~/services/leavesService.js")
                  .Include("~/services/travelRequestService.js")
                  .Include("~/services/AssetsCategoryService.js")
                  .Include("~/services/AssetsService .js")
                  .Include("~/controller/OnHoldGRController.js")
                  .Include("~/services/PurchaseOrderService.js")
                  .Include("~/services/supplierCategoryService.js")
                  .Include("~/services/CopyItemService.js")
                  .Include("~/services/OrderMasterService.js")
                  .Include("~/services/OrderDetailsService.js")
                  .Include("~/services/supplierService.js")
                  .Include("~/services/CustomerCategoryService.js")
                  .Include("~/services/CategoryService.js")
                  .Include("~/services/CityService.js")
                  .Include("~/services/StateService.js")
                  .Include("~/services/SubCategoryService.js")
                  .Include("~/services/SubsubCategoryService.js")
                  .Include("~/services/WarehouseService.js")
                  .Include("~/services/WarehouseCategoryService.js")
                  .Include("~/services/WarehouseSubCategoryService.js")
                  .Include("~/services/WarehouseSubsubCategoryService.js")
                  .Include("~/services/mappService.js")
                  .Include("~/services/unitMasterService.js")
                  .Include("~/services/itemMasterService.js")
                  .Include("~/services/FinancialYearService.js")
                  .Include("~/services/ItemBrandService.js")
                  .Include("~/services/WarehouseSupplierService.js")
                  .Include("~/services/TaxGroupDetailsService.js")
                  .Include("~/services/TaxGroupService.js")
                  .Include("~/services/TaxMasterService.js")
                  .Include("~/services/PurchaseOrderListService.js")
                  .Include("~/services/SearchPOService.js")
                  .Include("~/services/PurchaseODetailsService.js")
                  .Include("~/services/SliderService.js")
                  .Include("~/services/NotificationService.js")
                  .Include("~/services/NotificationByDeviceIdService.js")
                  .Include("~/services/GroupNotificationService.js")
                  .Include("~/services/DeviceNotificationService.js")
                  .Include("~/services/BillPramotionService.js")
                  .Include("~/services/ClusterService.js")
                  .Include("~/services/DeliveryService.js")
                  .Include("~/services/OfferService.js")
                  .Include("~/services/AreaService.js")
                  .Include("~/services/tripPlanner.js")                   

                  .Include("~/services/CustomerIssuesService.js")
                  .Include("~/services/CompanyService.js")
                  .Include("~/services/InventoryPageService.js")
                  .Include("~/services/WaitIndicatorService.js")
                  .Include("~/controller/ClusterWiseService.js")
                  .Include("~/services/PageMasterService.js")
                  .Include("~/services/RolePagePermissionService.js")
                  .Include("~/controller/InventoryReportController.js")
                  .Include("~/controller/PNLController.js")
                  .Include("~/controller/UPIDetailsController.js")
                    .Include("~/controller/TreeStructureController.js")//by ridhima
                                                                       //  .Include("~/controller/DigitalSalesController.js")
                  .Include("~/controller/NotificationDetailController.js")
                  .Include("~/controller/NotificationUpdatedController.js")
                  .Include("~/controller/PeopleGroup.js")
                  .Include("~/controller/SupplierGroup.js")
                  .Include("~/controller/CustomerGroup.js")
                  .Include("~/controller/GroupSMSController.js")
                  .Include("~/controller/AppHomeMobileController.js")
                  .Include("~/controller/AppHomeEditController.js")
                  .Include("~/controller/AgentsDashboardController.js")
                  .Include("~/controller/CreateCompanyController.js")
                  .Include("~/controller/CurrencyStockController.js")
                  .Include("~/controller/BankSettleController.js")
                  .Include("~/controller/CurrencySettleController.js")
                  .Include("~/controller/BounceCheqController.js")
                  .Include("~/controller/DeliveryBoyHistoryController.js")
                  .Include("~/controller/DeliveryBoyController.js")
                  .Include("~/controller/ChangeDBoyCtrl.js")
                  .Include("~/controller/RedispatchCtrl.js")
                  .Include("~/controller/VehicleController.js")
                  .Include("~/controller/basecategoryController.js")
                  .Include("~/controller/DeliveryController.js")
                  .Include("~/controller/SliderCtrl.js")
                  .Include("~/controller/PurchaseOrderListController.js")
                  .Include("~/controller/OrderSettleController.js")
                  .Include("~/controller/CurrentStockController.js")
                  .Include("~/controller/searchPOController.js")
                  .Include("~/controller/HDFCCreditDetailsController.js")
                  .Include("~/controller/SearchPODetailsController.js")
                  .Include("~/controller/DemandController.js")
                   .Include("~/controller/AppHomeExcludeController.js")
                  .Include("~/controller/orderdetailsController.js")
                  .Include("~/controller/OrderInvoiceController.js")
                  .Include("~/controller/orderMasterController.js")
                  .Include("~/controller/TaxGroupController.js")
                  .Include("~/controller/TaxmasterController.js")
                  .Include("~/controller/purchaseorderController.js")
                  .Include("~/controller/CopyItemController.js")
                  .Include("~/controller/RTVController.js")
                  .Include("~/controller/supplierCategoryController.js")
                  .Include("~/controller/supplierController.js")
                  .Include("~/controller/categoryController.js")
                  .Include("~/controller/deliveryChargeController.js")
                  .Include("~/controller/SalesSettlementHistoryController.js")
                  .Include("~/controller/GoodsRecivedController.js")
                  .Include("~/controller/cityController.js")
                  .Include("~/controller/stateController.js")
                  .Include("~/controller/subcategoryController.js")
                  .Include("~/controller/subsubCategoryController.js")
                  .Include("~/controller/FilterItemsController.js")
                  .Include("~/controller/warehouseController.js")
                   .Include("~/controller/warehouseSelectionController.js")//default warehouse Selection
                  .Include("~/controller/RazorpayQRDetailsController.js")
                  .Include("~/controller/WarehouseCategoryController.js")
                  .Include("~/controller/WarehousesubcategoryController.js")
                  .Include("~/controller/WarehousesubsubCategoryController.js")
                  .Include("~/controller/WarehouseSupplierController.js")
                  .Include("~/controller/orderMasterController.js")
                  .Include("~/controller/indexController.js")
                  .Include("~/controller/homeController.js")
                  .Include("~/controller/loginController.js")
                  .Include("~/controller/signupController.js")
                  .Include("~/controller/ordersController.js")
                  .Include("~/controller/refreshController.js")
                  .Include("~/controller/LoginTokenController.js")
                  .Include("~/controller/tokensManagerController.js")
                  .Include("~/controller/associateController.js")
                  .Include("~/controller/tasktypesController.js")
                  .Include("~/controller/customerController.js")
                  .Include("~/controller/projectsController.js")
                  .Include("~/controller/tasksController.js")
                  .Include("~/controller/peoplesController.js")
                  .Include("~/controller/ReturnOrderdetailsController.js")
                  .Include("~/controller/roleController.js")
                  .Include("~/controller/BudgetAllocationListController.js")
                   .Include("~/controller/WarehouseExpensePointController.js")
                   .Include("~/controller/OrderPaymentReportController.js")
                   .Include("~/controller/reportsController.js")
                   .Include("~/controller/VehicleAssissmentController.js")
                   .Include("~/controller/orderPendingController.js")
                   .Include("~/controller/SuggetionContoller.js")
                   .Include("~/controller/targetController.js")
                   .Include("~/controller/IRController.js")
                   // .Include("~/controller/IRSupplierController.js")
                   .Include("~/controller/ReturnItemCtrl.js")
                   .Include("~/controller/RewardPointController.js")
                   .Include("~/controller/promItemController.js")
                   .Include("~/controller/RewardItemCtrl.js")
                   .Include("~/controller/RedeemOrderCtrl.js")
                   .Include("~/controller/CustomerIssueController.js")
                   .Include("~/CRM/NetprofitController.js")
                   .Include("~/controller/PopUpController.js")
                    .Include("~/controller/ExcelOrderCtrl.js")
                    .Include("~/controller/customersDeviceInfoController.js")
                   .Include("~/controller/DboyphonedetailsController.js")
                     .Include("~/controller/AgentphoneinfoController.js")
                   .Include("~/ReportsControllers/ReportController.js")
                     .Include("~/ReportsControllers/Report3Controller.js")
                  .Include("~/ReportsControllers/RetailersReportCtrl.js")
                   .Include("~/ReportsControllers/DashboardReportController.js")
                    .Include("~/ReportsControllers/ComparisonCtrl.js")
                    .Include("~/ReportsControllers/DeliveryBoyReportCtrl.js")
                        .Include("~/UnitEconomics/unitEcoController.js")
                   .Include("~/UnitEconomics/unitEcoReportCtlr.js")
                   .Include("~/controller/MilestonePointController.js")
                   .Include("~/controller/NppHistoryPriceController.js")
                   .Include("~/controller/supplierPromo.js")
                   .Include("~/controller/PDCABaseCategoryController.js")
                   .Include("~/controller/PDCACategoryController.js")
                   .Include("~/controller/PDCADetailsController.js")
                   .Include("~/controller/PDCADataCompairController.js")
                    .Include("~/controller/SalesBounceController.js")
                    .Include("~/controller/SalesSettlementController.js")
                      .Include("~/controller/logoutController.js")
                      .Include("~/controller/ShortageSettleController.js")
                      .Include("~/controller/AreaMasterController.js")
                      .Include("~/controller/mappController.js")
                      .Include("~/controller/menuController.js")
                      .Include("~/controller/signupController.js")
                      .Include("~/controller/headerController.js")
                       .Include("~/controller/TFSController.js") //NOT FOUND
                    .Include("~/controller/invoiceController.js")
                    .Include("~/controller/invoicemoreyeahController.js")
                    .Include("~/controller/createRecurringInvoiceController.js")
                    .Include("~/controller/AddInvoiceController.js")
                    .Include("~/controller/GrowthModuleLoginController.js")
                    .Include("~/controller/leavesController.js")
                    .Include("~/controller/GrowthDashboardController.js")
                    .Include("~/controller/travelRequestController.js")
                    .Include("~/controller/assetsCategoryController.js")
                    .Include("~/controller/assetsController.js")
                    .Include("~/controller/clusterController.js")
                    .Include("~/controller/itemMasterController.js")
                    .Include("~/controller/ItemSupplierMappingController.js")
                    .Include("~/scripts/ngAutocomplete.js")
                    .Include("~/controller/OnHoldGRAddDataController.js")//NOT FOUND
                    .Include("~/controller/OnHoldGRController.js")
                    .Include("~/controller/customerCategoryController.js")
                    .Include("~/controller/MessageController.js")
                    .Include("~/controller/GroupNotificationController.js")
                    .Include("~/controller/NotificationByDeviceIdController.js")
                    .Include("~/controller/NotificationController.js")
                     .Include("~/services/CouponService.js")
                     .Include("~/services/NewsService.js")
                     .Include("~/controller/NewsController.js")
                     .Include("~/controller/weektimesheetController.js")
                       .Include("~/controller/daytimesheetController.js")
                   .Include("~/controller/monthtimesheetController.js")
                   .Include("~/controller/calenderController.js")//NOT FOUND
                   .Include("~/controller/typeaheadController.js")//NOT FOUND
                   .Include("~/controller/settingController.js")//NOT FOUND
                   .Include("~/controller/profilesController.js")
                   .Include("~/controller/userdashboardController.js")
                   .Include("~/controller/o365dashboardController.js")
                   .Include("~/controller/clientdashboardController.js")
                   .Include("~/controller/projectdashboardController.js")
                   .Include("~/controller/weektasktimesheetController.js")
                   .Include("~/controller/daytasktimesheetController.js")
                   .Include("~/controller/BillPramotionController.js")
                   .Include("~/controller/OfferController.js")
                   .Include("~/controller/OfferADDController.js")
                   .Include("~/controller/FreebiesUploderController.js")
                   .Include("~/controller/WalletController.js")
                   .Include("~/controller/NotOrderedController.js")
                   .Include("~/views/DamageStock/DamageStockController.js")
                   .Include("~/views/DamageStock/CreateDamageOrderController.js")
                   .Include("~/views/DamageStock/DamageorderMasterController.js")
                   .Include("~/views/DamageStock/DamageItemApprovalController.js")

                   .Include("~/controller/BackendOrder/BackendorderMasterController.js")
                   .Include("~/controller/BackendOrder/CreateBackendController.js")

                   .Include("~/services/DamageOrderMasterService.js")
                   .Include("~/services/DamageOrderDetailsService.js")
                   .Include("~/controller/DamageorderdetailsController.js")
                   .Include("~/views/DamageStock/DamageStockItemCntrl.js")
                   .Include("~/controller/OrderProcessReportController.js")
                   .Include("~/controller/AssignCustomersCtrl.js")
                   .Include("~/controller/CustomerExecutiveDetailsController.js")
                   .Include("~/controller/SupplierBrandsCtrl.js")
                   .Include("~/controller/AssignBulkcustomersController.js")
                   .Include("~/controller/PointInOutController.js")
                   .Include("~/controller/AssignBrandCustomerController.js")
                   .Include("~/controller/BrandwisepramotionController.js")
                   .Include("~/controller/DialPointController.js")
                   .Include("~/controller/DialValuePointController.js")
                   .Include("~/controller/NotVisitCtrl.js")
                   .Include("~/controller/AgentsController.js")
                   .Include("~/controller/PrestloanCustomerController.js")
                   .Include("~/controller/MarginImagePromotionController.js")
                   .Include("~/controller/TurnATimeController.js")
                   .Include("~/controller/ExclusiveBrandController.js")
                   .Include("~/controller/HighestDreamPointItemController.js")
                   .Include("~/controller/CiMatrixController.js")
                   .Include("~/controller/CategoryImageController.js")
                   .Include("~/controller/DocumentController.js")
                   .Include("~/controller/RedispatchCtrlAutoCanceled.js")
                   .Include("~/controller/TransferOrderController.js")
                   .Include("~/controller/trackuser.js")
                   .Include("~/controller/DeletepeoplesController.js")
                   .Include("~/controller/UserAccessPermissionController.js")
                   .Include("~/controller/CustomerVoiceController.js")
                   .Include("~/controller/casecontroller.js")
                   .Include("~/services/fileUploadDirective.js")//NOT FOUND
                   .Include("~/services/CaseService.js")
                   .Include("~/services/ProjectService.js")
                   .Include("~/controller/ChangePassword.js")
                   .Include("~/controller/AddPeopleCtrl.js")
                   .Include("~/controller/Departmentctrl.js")
                   .Include("~/controller/designationController.js")
                   .Include("~/controller/SkillCtrl.js")
                   .Include("~/controller/OrderProcessStatus.js")
                   .Include("~/controller/AppHomeController.js")
                   .Include("~/InActiveCustomerOrder/InActiveCustOrderMasterController.js")
                   .Include("~/CRM/OrderDataController.js")
                   .Include("~/controller/POapprovalController.js")
                   .Include("~/controller/MyUdharController.js")
                   .Include("~/controller/IRBuyerController.js")
                   .Include("~/controller/IRDetailBuyerController.js")
                   .Include("~/controller/WarehouseBaseCategoryController.js")
                   .Include("~/controller/PoDashboardController.js")
                   .Include("~/controller/StatusSupplierController.js")
                   .Include("~/controller/IRController.js")
                   .Include("~/controller/CurrentNetStockController.js")
                   .Include("~/controller/CaseViewController.js")
                   .Include("~/controller/FirstTimeOrderController.js")
                   .Include("~/controller/WalletHistoryController.js")
                   //.Include("~/controller/ViewPaymentsController.js")
                   .Include("~/controller/QuestionController.js")
                   .Include("~/controller/GameLevelController.js")
                   .Include("~/controller/AppHomeMobileController.js")
                   .Include("~/controller/IRViewController.js")
                   .Include("~/controller/MasterOnHoldGRController.js")
                   .Include("~/controller/OnHoldGRController.js")
                   .Include("~/controller/CallHistoryController.js")
                   .Include("~/controller/ClusterWiseController.js")
                   .Include("~/controller/RejectedIRController.js")
                   .Include("~/controller/RejectedIRDetailController.js")
                   .Include("~/controller/IRDraftController.js")
                   .Include("~/controller/ClusterMapController.js")
                   .Include("~/controller/ChannelPartnerController.js")
                   .Include("~/controller/AgentsPerformanceController.js")
                   .Include("~/controller/CreateDeliveryAssignmentController.js")
                   .Include("~/controller/DeliveryAssignmentController.js")
                   .Include("~/controller/DeliveryOrderAssignmentChangeController.js")
                   .Include("~/controller/AssignmentReportController.js")
                   .Include("~/controller/GrowthDashboardController.js")
                   .Include("~/controller/AdjustmentCurrentStockController.js")
                   .Include("~/controller/ItemMovementReportController.js")
                   .Include("~/controller/PODashboardMainController.js")
                   // .Include("~/controller/AppHomeUpdatedController.js")
                   .Include("~/controller/POTaskController.js")
                   .Include("~/controller/IRDraftController.js")
                   .Include("~/controller/BlankPOController.js")
                   .Include("~/services/BlankPOService.js")
                   .Include("~/controller/BlankPODetailsController.js")
                   .Include("~/controller/BlankPOEditController.js")
                   .Include("~/controller/BlankPOController.js")
                   .Include("~/services/BlankPOService.js")
                   .Include("~/controller/BlankPODetailsController.js")
                   .Include("~/controller/BlankPOEditController.js")
                   .Include("~/controller/IRDraftController.js")
                   .Include("~/controller/ClusterMapController.js")
                   .Include("~/controller/ViewDepoController.js")
                   .Include("~/controller/IRDraftController.js")
                   .Include("~/controller/PurchasePendingReportController.js")
                   .Include("~/controller/Logs/TraceLogController.js")
                   .Include("~/controller/HealthChartController.js")
                   .Include("~/controller/AddclusterController.js")
                   .Include("~/controller/TurnAroundTimeCotroller.js")
                   .Include("~/controller/CurrentStockReportController.js")
                   .Include("~/controller/TargetDashboardController.js")
                   .Include("~/controller/BudgetAllocationController.js")
                   .Include("~/controller/AutoProcessorderController.js")
                   .Include("~/controller/HubPhaseController.js")
                   .Include("~/controller/InventoryEditController.js")  //Vinayak (3/10/2019)                 
                   .Include("~/controller/InventoryApprovalPagecontroller.js")
                   .Include("~/controller/DispatchedOrderPageController.js")
                   .Include("~/controller/UpdateVersionController.js")
                   .Include("~/controller/CityBaseCustomerRewardController.js")
                   .Include("~/controller/permission/PageMasterController.js")
                   .Include("~/controller/permission/RolePagePermissionController.js")
                   .Include("~/controller/UnconfirmedGRController.js")
                   .Include("~/controller/TemporaryCurrentStockController.js")
                   .Include("~/controller/EpayLetterController.js")
                   .Include("~/CRM/OrderDataKKController.js")
                   .Include("~/controller/FlashDealReportController.js")
                   .Include("~/controller/OnlineTransactionDashBoardcontroller.js")
                   .Include("~/controller/PaymentUploadController.js")
                   .Include("~/controller/PaymentUploadDetailsController.js")
                   .Include("~/controller/HDFCDetailsController.js")
                   .Include("~/controller/MposDetailsController.js")
                   .Include("~/controller/HDFCUPIDetailsController.js")
                   .Include("~/controller/HDFCNetBankingBDetailsController.js")
                   .Include("~/controller/ClusterCityMapController.js")
                   .Include("~/controller/InventoryCycleController.js")
                   .Include("~/controller/CashManagement/AgentPaymentController.js")
                   .Include("~/controller/GRCancellationController.js")
                   .Include("~/controller/IRDashboardController.js")
                   .Include("~/controller/GRIRMappingController.js")
                   .Include("~/controller/IRMasterController.js")
                   .Include("~/controller/GSTReturnController.js")                   
                    .Include("~/controller/PurchaseOrder/GoodsRecivedControllerNew.js")
                    .Include("~/controller/PurchaseOrder/GRDraftDetailController.js")
                    .Include("~/controller/IRNRegerateCotroller.js")
                    .Include("~/controller/RTGSOrdersPaymentCotroller.js")
.Include("~/controller/PurchaseOrder/PurchaseOrderMasterController.js")
.Include("~/controller/PurchaseOrder/PurchaseOrderDetailNewController.js")
.Include("~/controller/PurchaseOrder/IRControllerNew.js")
.Include("~/controller/PurchaseOrder/CreditInvoiceController.js")
   .Include("~/controller/PurchaseOrder/PRApprovalController.js")
      .Include("~/controller/PurchaseOrder/PRPaymentApprovalController.js")
      .Include("~/controller/BackendOrder/BackendorderMasterController.js")
                   .Include("~/controller/BackendOrder/CreateBackendController.js")
                   .Include("~/controller/BackendOrder/BackendOrderInvoiceController.js")
                   .Include("~/controller/PurchaseOrder/PartialPOReportController.js")
                    .Include("~/controller/PurchaseOrder/GDNController.js")
                    .Include("~/controller/tripPlannerController.js")
                      .Include("~/controller/ChequeUploadDetailsController.js")
                      .Include("~/controller/BackendOrder/CustomerBackendOrderInvoiceController.js")

         );

            bundles.Add(new ScriptBundle("~/bundles/Alljs")
              .Include("~/services/SafetyStockService.js")
              .Include("~/controller/SafetystockController.js")
             .Include("~/services/FreeStockService.js")
             .Include("~/controller/FreestockController.js")
             .Include("~/controller/CashManagement/AgentPaymentController.js")
              .Include("~/controller/WelcomeController.js")
                .Include("~/controller/CustomerGroup.js")
                .Include("~/services/getsetdata.js")
                .Include("~/controller/DigitalSalesController.js")
                .Include("~/controller/SupplierPaymentDetailsController.js")
                .Include("~/controller/AgentExcelController.js")
                .Include("~/controller/ItemPramotionController.js")
               .Include("~/controller/AddDatainOnHoldGR.js")
                .Include("~/controller/FinancialYearController.js")
                .Include("~/controller/ItembrandController.js")
              .Include("~/controller/permission/PeoplePageAccessPermissionsController.js")
               .Include("~/controller/permission/PageButtonPermissionController.js")
              .Include("~/controller/IRSupplierController.js")
               .Include("~/CRM/CRMCtrl.js")
               .Include("~/CRM/CRMcust4ActionCtrl.js")
               .Include("~/CRM/ActionController.js")
               .Include("~/ReportsControllers/Comparison1Ctrl.js")
               .Include("~/controller/AsignDayController.js")
               .Include("~/controller/dashboardController.js")
               .Include("~/controller/DBoyOrderReportingController.js")
               .Include("~/controller/CouponController.js")
               .Include("~/controller/ViewPaymentsController.js")
               .Include("~/controller/CashManagement/WarehouseCashController.js")
               .Include("~/controller/CashManagement/WarehouseLiveDashboard.js")
               .Include("~/controller/CashManagement/WarehouseSettlement.js")
               .Include("~/controller/CashManagement/HQCurrencyController.js")
               .Include("~/controller/AppHomeUpdatedController.js")
               .Include("~/controller/DownloadItemLedgercontroller.js")

               );

            //For enabling optimization forcefully.  


            string environment = ConfigurationManager.AppSettings["Environment"];
            BundleTable.EnableOptimizations = true;
            if (environment == "development")
            {
                BundleTable.EnableOptimizations = false;
            }

        }
    }
}