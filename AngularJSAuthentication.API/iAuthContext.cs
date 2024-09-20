using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.ControllerV1;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using System;
using System.Collections.Generic;
using AngularJSAuthentication.Model.PurchaseOrder;
//using GenricEcommers.Models;
//using AngularJSAuthentication.API.SK.Models;

namespace AngularJSAuthentication.API
{
    /// <summary>
    /// This interface contains all defination of AuthContext Class 
    /// </summary>
    public interface iAuthContext
    {
        #region For Pagination  Currency
        PaggingData_ctin AllCurrencyHistoryIN(int list, int page, string status);
        PaggingData_ctout AllCurrencyHistoryOut(int list, int page, string status);

        #endregion
        #region dial Point
        List<DialPoint> GetDialData(int CustomerId);
        DialPoint updateDialPoint(int Id);

        #endregion
        #region Supplier Brands 

        List<SupplierBrands> GetmyBrands(int sid);
        List<SupplierBrandsDTO> getBrands(int sid);
        bool ADUDBrands(List<SupplierBrandsDTO> obj);

        #endregion
        #region for Brand Item 
        List<ItemMaster> ItembyBrand(int SubSubCategoryId, int WarehouseId);
        List<SubsubCategory> subsubcategorybyWarehouse(int id, int compid);
        List<ItemMaster> UpdateItemMaster(List<ItemMaster> item, int compid);
        #endregion



        temOrderQBcode AssignmentGenerateBarcode(string OrderId);//for barcode interfaces


        IEnumerable<CurrencyBankSettle> Imagegetview(int id);
        IEnumerable<CurrencyHistory> TotalStockCurrencys();
        //IEnumerable<CurrencyHistory> TotalStockCurrencys(int id);
        IEnumerable<CheckCurrency> AllStockCurrencyscheck(string status);
        IEnumerable<CurrencyStock> AllStockCurrencys(string Stock_status);
        IEnumerable<DocumentList> AllDocumentWid(int compid, int warehouse_id);
        //By Sachin
        IEnumerable<CurrencyHistory> GetAllStockCurrencys();
        IEnumerable<DocumentList> AllDocument(int compid);
        bool IsDocExists(DocumentList doc);
        int DocumentAdd(int compid, int warehouse_id, DocumentList aj);
        //By Sachin Jaiswal
        IEnumerable<CurrencyStock> DelivaryBoyTotalData();
        IEnumerable<CurrencyStock> GetDboyCurrencyData();


        IEnumerable<CurrencyBankSettle> AllBankStockCurrencysByDate();


        string InsertCurrencyData(CurrencyData cc);
        IEnumerable<CurrencyBankSettle> AllBankStockCurrencys();

        //By Sachin For Transfering Currency history to Opening

        //Customer addassighncustsupplier(Customer custex);
        List<CustSupplier> addcustsuppliermapping(List<CustSupplier> obj, int compid, int wid);
        List<CustomerDTO> getcust2assin(int CityId, int Warehouseid, string SubsubCode, int CompanyId);
        List<CustomerDTOM> getmycustomer(int compid);
        List<CustomerDTOM> getmycustomerWid(int compid, int Warehouse_id);
        CustomerDTOM AllSalePersonRetailer(string srch, int id1);
        CustSupplierRequest addCustSupplierRequest(CustSupplierRequest obj);
        CustSupplierRequest addCustSupplierRequestput(CustSupplierRequest obj);
        List<CustSupplierRequest> GetcmRequest();
        List<Customer> GetCustomerbyClusterId(int ClusterID);

        //By Sachin For Getting Current Stock of Free Item
        int GetFreeItemStockOnOrderId(int OrderId);
        List<CustSupplier> RemoveCustSupplier(List<CustSupplier> obj, int compid);
        IEnumerable<People> AllPeoplesDep(string dep, int CompanyId);
        string skcode();
        string deliveryIssuance(DeliveryIssuance OBJ);
        List<People> AllDBoy(int CompanyId);
        List<People> AllDBoyWid(int CompanyId, int Warehouse_id);
        People AddDboys(People city);
        People PutDboys(People city);
        bool DeleteDboys(int id, int ComapnyId);
        // bool AllOrderMasterspriority(List<OrderMaster> assignedorders, int warehouseid, string Mobile);

        //By Sachin

        IEnumerable<OrderMaster> PendingOrderByDate(DateTime startdate, DateTime enddate, int warehouseid);
        List<Vehicle> AllVehicles(int compid);
        List<Vehicle> AllVehiclesWid(int compid, int Warehouse_id);
        Vehicle AddVehicle(Vehicle city);
        Vehicle PutVehicle(Vehicle city);
        bool DeleteVehicle(int id, int CompanyId);
        SalesPersonBeat Addsalesbeat(SalesPersonBeat obj);
        IList<EditPriceHistory> filteredEditPriceHistory(DateTime? start, DateTime? end, string cityid, string categoryid, string subcategoryid, string subsubcategoryid, int compid);
        IList<Customer> filteredCustomerReport(DateTime? datefrom, DateTime? dateto);
        IList<OrderDetails> OrderMonthReport(DateTime? datefrom, DateTime? dateto);
        IEnumerable<OrderMaster> AllOrderMasters(int compid);
        IEnumerable<DamageOrderMaster> AllDOrderMasters(int compid);

        //search Orders
        List<OrderMaster> searchorderbycustomer(DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, int compid);
        List<OrderMaster> searchorderbycustomerwid(DateTime? start, DateTime? end, int OrderId, string Skcode, string ShopName, string Mobile, string status, int compid, int warehouseid);
        List<OrderHistory> getDBoyOrdersHistory(string mob, DateTime? start, DateTime? end, int dboyId);
        List<OrderDispatchedMasterDTOM> getAcceptedOrders(string mob);
        //OrderDispatchedMaster orderdeliveredreturn(OrderDispatchedMaster obj);
        //OrderPlaceDTO orderdeliveredreturnV2(OrderPlaceDTO obj);
        List<OrderDispatchedMaster> changeDBoy(List<OrderDispatchedMaster> objlist, string mob, int compid, int userid);
        List<OrderDispatchedMaster> getdboysOrder(string mob, int compid);
        List<DBoyCurrency> getdboysCurrency(int PeopleID);
        Customer Resetpassword(Customer customer);
        IEnumerable<OrderDispatchedMaster> AllDispatchedOrderMaster(int compid);
        PurchaseOrderMaster addPurchaseOrderMaster(List<TempPO> OBJ);
       // PurchaseOrder AddPurchaseItem(PurchaseOrder poItem);
        GpsCoordinate Addgps(GpsCoordinate obj);
        PurchaseOrderDetailRecived AddPurchaseOrderDetailsRecived(PurchaseOrderDetailRecived pd, int count);
        PurchaseOrderMasterRecived AddPurchaseOrderMasterRecived(PurchaseOrderMasterRecived po);
        PurchaseOrderMaster AllPOrderDetails1(int i, int compid);
        IEnumerable<FinalOrderDispatchedDetails> AllFOrderDispatchedDetails(int i, int compid);
        OrderDispatchedMaster AddOrderDispatchedMaster(OrderDispatchedMaster dm);
        OrderDispatchedDetails AddOrderDispatchedDetails(OrderDispatchedDetails dd);
        OrderDispatchedMaster UpdateOrderDispatchedMaster(OrderDispatchedMaster om);
        IEnumerable<OrderDispatchedDetails> AllPOrderDispatchedDetails(int i, int compid);
        IEnumerable<ReturnOrderDispatchedDetails> AllReturnOrderDispatchedDetails(int i, int compid);
        CurrentStock UpdateCurrentStock(CurrentStock stock);
        OrderMaster PutOrderMaster(OrderMaster city);
        Feedback AddFeedBack(Feedback obj);
        RequestItem AddRequestItem(RequestItem obj);
        List<Favorites> AllFavorites(string mob, int compid);
        Favorites AddFavorites(Favorites Favt);
        PaggingData AllItemMasterForPaging(int list, int page, int CompanyId, string status);
        PaggingData AllItemMasterForPagingWid(int list, int page, int Warehouse_id, int CompanyId, string status,string type);
        PaggingData AllItemUnMappedSupplierForPagingWid(int list, int page, int Warehouse_id, int CompanyId, string status);
        InvoiceRow AddInvoiceDetail(InvoiceRow e);
        AllInvoice AddInvoice(AllInvoice customer);
        FinalOrderDispatchedMaster AddFinalOrderDispatchedMaster(FinalOrderDispatchedMaster final);
        List<OrderDispatchedDetailsFinalController.filtered> AllFOrderDispatchedReportDetails(DateTime datefrom, DateTime dateto, int compid);
        List<OrderDispatchedMaster> AllFOrderDispatchedDeliveryDetails(DateTime datefrom, DateTime dateto, int compid);
        #region Customer
        People CheckPeople(string mob, string password);
        People CheckPeopleSalesPersonData(string Mobile);
        IEnumerable<Customer> AllCustomers(int compid);
        IEnumerable<CustomerRegistration> Allcustomers();
        IEnumerable<Customer> AllCustomerbyCompanyId(int cmpid);
        Customer AddCustomer(Customer customer);
        Customer PutCustomer(Customer customer);
        Customer GetClientforProjectId(int projId);
        bool DeleteCustomer(int id);
        //IList<Customer> filteredCustomerMaster(string Cityid, string Warehouseid, DateTime? datefrom, DateTime? dateto);
        IList<Customer> filteredCustomerMaster(string Cityid, DateTime? datefrom, DateTime? dateto, string mobile, string skcode, List<int> ids);
        object getCustomerbyid(object id);
        List<Customer> AddBulkcustomer(List<Customer> CustCollection);
        List<People> AddBulkpeople(List<People> CustCollection);
        #endregion
        List<OrderDispatchedMaster> AllFOrderDispatchedDeliveryBoyDetails(DateTime datefrom, DateTime dateto, string DboyName, int compid);
        Customer GetCustomerbyId(string Mobile);

        PaggingNotiByDevice GetAllNotification(int list, int page);

        PaggingData AllOrderMaster(int list, int page, int compid);

        PaggingData_st AllItemHistory(int list, int page, string ItemNumber, int WarehouseId, int StockId);

        PaggingData_Delivery AllDataDeliveryIssurance(int list, int page, int id, DateTime? start, DateTime? end);


        PaggingData AllOrderMasterWid(int list, int page, int compid, int Warehouse_id);
        PaggingData AllDamageOrderMaster(int list, int page, int compid, int WarehouseId);
        bool DeleteOrderMaster(int id, int compid);
        IEnumerable<OrderDetails> AllOrderDetails(int i, int compid);
        IEnumerable<DamageOrderDetails> AllDOrderDetails(int i, int compid);
        IList<DemandDetailsNew> AllfilteredOrderDetails(string Cityid, string Warehouseid, DateTime datefrom, DateTime dateto, int compid);
        //IList<PurchaseOrderList> AllfilteredOrderDetails2(string Cityid, int Warehouse_id, DateTime? datefrom, DateTime? dateto, int compid);
        IList<OrderMaster> filteredOrderMasters1(string Warehouseid, DateTime datefrom, DateTime dateto, int compid);
        IList<OrderMaster> filteredOrderMaster(string Cityid, string Warehouseid, DateTime datefrom, DateTime dateto, string search, string status, string deliveryboy, int compid);

        IEnumerable<OrderDetails> Allorddetails(int compid);
        IEnumerable<DamageOrderDetails> AllDorddetails(int compid);
        IEnumerable<PurchaseOrder> AllPurchaseOrder(int compid);
        List<PurchaseOrderList> AddPurchaseOrder(List<PurchaseOrderList> po, int compid);
        List<ReturnOrderDispatchedDetails> add(List<ReturnOrderDispatchedDetails> po);
        IEnumerable<ItemMaster> AllItemMaster(int CompanyId);
        IEnumerable<ItemMaster> AllItemMasterWid(int CompanyId, int Warehouse_id);
        IEnumerable<ItemMaster> itembyid(int id, int CompanyId);
        //List<ItemMaster> AddBulkItemMaster(List<ItemMaster> itemCollection);
        ItemMasterCentral AddItemMaster(ItemMasterCentral itemmaster);
        List<ItemMaster> AddItemMove(List<MoveWarehouse> item, int warehid);
        ItemMasterCentral PutCentralItemMaster(ItemMasterCentral itemmaster);
        ItemMaster PutItemMaster(ItemMaster itemmaster);
        //ItemMaster Saveediteditem(List<ItemMaster> itemmasterList, int compid);
        bool DeleteItemMaster(int id, int CompanyId);
        IList<ItemMaster> filteredItemMaster(string categoryid, string subcategoryid, string subsubcategoryid, int CompanyId);
        IList<ItemMaster> filteredItemMasterWid(string categoryid, string subcategoryid, string subsubcategoryid, int CompanyId, int Warehouse_id);
        People getPersonIdfromEmail(string email);
        IEnumerable<Company> AllCompanies { get; }
        Company AddCompany(Company company);
        Company PutCompany(Company company);
        Company GetCompanybyCompanyId(int id);
        bool DeleteCompany(int id);
        bool CompanyExists(string companyName);
        IEnumerable<ProjectTask> AllProjectTask { get; }
        IEnumerable<ProjectTask> AllProjectTaskbyCompanyId(int cmpid);
        List<ProjectTask> AllProjectTaskByuserId(int userid);
        ProjectTask GetProjectTaskById(int id);
        ProjectTask AddProjectTask(ProjectTask projectTask);
        ProjectTask PutProjectTask(ProjectTask projectTask);
        bool DeleteProjectTask(int id);
        IEnumerable<TaxMaster> AllTaxMaster(int compid);
        TaxMaster AddTaxMaster(TaxMaster taxMaster);
        TaxMaster PutTaxMaster(TaxMaster taxMaster);
        bool DeleteTaxMaster(int id, int CompanyId);
        IEnumerable<TaxGroup> AllTaxGroup(int compid);
        TaxGroup AddTaxGroup(TaxGroup taxGroup);
        TaxGroup PutTaxGroup(TaxGroup taxGroup);
        bool DeleteTaxGroup(int id, int CompanyId);
        TaxGroupDetails AddTaxGRPDetail(TaxGroupDetails taxGroupDetails);
        IEnumerable<TaxGroupDetails> AlltaxgroupDetails(int i, int compid);
        IEnumerable<Event> AllEvents(int userid);
        IEnumerable<Event> FilteredEvents(DateTime startDate, DateTime endDate);
        IEnumerable<Event> FilteredEvents(DateTime startDate, DateTime endDate, int compid);
        IEnumerable<Event> FilteredEvents(DateTime startDate, DateTime endDate, int userid, int compid);
        Event AddEvent(Event e);
        Event UpdateEvent(Event e);
        bool DeleteEvent(int id);
        Event UpdateEventByViewModel(DayEventViewModel model, string d, int userid, int compid);
        IEnumerable<Project> AllProjects { get; }
        //IEnumerable<object> DeletePeopleDb { get; set; }
        IEnumerable<Project> AllProjectsbyCompanyId(int cmpid);
        IEnumerable<Project> AllActiveProjectsbyCompanyId(int cmpid);
        Project AddProject(Project project);
        Project PutProject(Project project);
        bool DeleteProject(int id);
        IEnumerable<TaskType> AllTaskTypes(int compid);
        IEnumerable<TaskType> AllTaskTypesbyCompanyId(int cmpid);
        TaskType AddTaskType(TaskType customer);
        TaskType PutTaskType(TaskType customer);
        TaskType GetTaskTypeById(int id);
        bool DeleteTaskType(int id);
        IEnumerable<Leave> AllLeaves(int compid);
        Leave AddLeave(Leave leave);
        void AddOrderMaster(OrderMaster item);
        Leave PutLeave(Leave leave);
        bool DeleteLeave(int id);
        IEnumerable<ItemPramotions> AllItemPramotion(int compid);
        IEnumerable<ItemPramotions> AllItemPramotionWid(int compid, int Warehouse_id);
        IEnumerable<CustomerCategory> AllCustomerCategory(int compid);
        CustomerCategory AddCustomerCategory(CustomerCategory Cust);
        CustomerCategory PutCustomerCategory(CustomerCategory Cust);
        bool DeleteCustomerCategory(int id);
        //void ActiveDeletedpeople(int peopleid);
        IEnumerable<People> AllPeoples(int compid);
        IEnumerable<People> AllPeoplesWid(int compid, int Warehouse_id);
        IEnumerable<People> AllPeoplesWidAgent(int compid, int Warehouse_id);
        IEnumerable<People> AllPeoplesAgent(int compid);
        IEnumerable<People> AllPeoplesWidActiveAgent(int compid, int Warehouse_id);//call only active agent warehouse based
        IEnumerable<People> AllPeoplesActiveAgent(int compid);// call only active agent
        UserAccessPermission getRoleDetail(string RoleName);
        List<People> GetPeoplebyCompanyId(int id);
        People GetPeoplebyId(int compid, string email);
        People GetPeoplebyIdWid(int compid, int Warehouse_id, string email);
        People AddPeople(People people);
        People AddPeoplebyAdmin(People people);
        People PutPeople(People people);
        People PutPeoplebyAdmin(People people);
        // bool DeletePeople(int id, int CompanyId,string UserName);
        bool DeletePeople(int id, string UserName, string DeleteComment);//By Danish-----19/04/2019
        bool Deleteredeemitem(int id);
        bool ActiveDeletedpeoples(int peopleid);//By Danish-----19/04/2019
                                                // bool DeleteCompanyPeople(int id, int CompanyId, string UserName);
        IEnumerable<SubsubCategory> AllSubsubCat(int compid);
        SubsubCategory AddSubsubCat(SubsubCategory asset);
        SubsubCategory AddQuesAnsxl(SubsubCategory asset);
        SubsubCategory PutSubsubCat(SubsubCategory asset);
        bool DeleteSubsubCat(int id, int CompanyId);
        IEnumerable<Category> AllCategory(int compid);
        IEnumerable<SubsubCategory> sAllCategory(int compid);
        Category AddCategory(Category category);
        Category PutCategory(Category category);
        //bool DeleteCategory(int id,int CompanyId);


        IEnumerable<Country> Allcountries();
        bool IsCountryExists(Country country);
        Country AddCountry(Country country);
        // Country PutCountry(Country objCountry);
        bool DeleteCountry(int id);

        Country PutCountry(Country item);

        IEnumerable<RegionZone> Allregion();
        bool IsRegionExists(RegionZone region);
        RegionZone AddRegion(RegionZone region);
        bool DeleteRegion(int id);

        RegionZone PutRegion(RegionZone item);
        IEnumerable<State> Allstates();
        bool IsStateExists(State state);

        State AddState(State state,int userid);
        State PutState(State state);
        bool DeleteState(int id);
        IEnumerable<Role> AllRoles(int compid);
        bool IsRoleExists(Role role);
        Role AddRole(Role role);
        Role PutRoles(Role role);
        bool DeleteRole(int id);
        CurrentStock AddCurrentStock(CurrentStock CurrentStock);
        CurrentStock PutCurrentStock(CurrentStock CurrentStock);
        //OrderDispatchedDetails PutOrderQuantityStock(OrderDispatchedDetails OrderDetails);
        bool DeleteCurrentStock(int id, int CompanyId);
        IEnumerable<City> AllCitys();
        IEnumerable<City> AllCity(int sid);
        City AddCity(City city,int userid);
        City PutCity(City city);
        bool DeleteCity(int id);
        IEnumerable<Warehouse> AllWarehouse(int compid);
        IEnumerable<Warehouse> AllWarehouse(int compid, bool IsKPP);
        IEnumerable<Warehouse> AllWarehouseWid(int compid, int Warehouse_id);
        IEnumerable<Warehouse> AllWarehouseWid(int compid, int Warehouse_id, bool IsKPP);
        Warehouse AddWarehouse(Warehouse warehouse);
        Warehouse PutWarehouse(Warehouse warehouse);
        bool DeleteWarehouse(int id, int CompanyId);
        IEnumerable<Warehouse> AllWHouseforapp(int CompanyId);
        Cluster Addcluster(Cluster item);
        IEnumerable<Cluster> AllCluster(int compid);
        Cluster getClusterbyid(int id);
        Cluster UpdateCluster(Cluster item);
        bool DeleteCluster(int id, int CompanyId);
        IEnumerable<WarehouseSupplier> AllWarehouseSupplier(int compid);
        WarehouseSupplier AddWarehouseSupplier(WarehouseSupplier warehouseSupplier);
        WarehouseSupplier PutWarehouseSupplier(WarehouseSupplier warehouseSupplier);
        bool DeleteWarehouseSupplier(int id, int compid);
        WarehouseCategory Addwarehousecatxl(WarehouseCategory warehousecategory);
        //IEnumerable<WarehouseCategory> AllWarehouseCategory(int compid);
        //IEnumerable<WarehouseCategory> AllWarehouseCategoryWid(int compid, int Warehouse_id);
        IEnumerable<WarehouseCategory> AllWhCategory();


        #region for warehouse subsusbcategory mapping

        IEnumerable<WarehouseSubsubCategory> AllWarehouseCategory(int compid);
        IEnumerable<WarehouseSubsubCategory> AllWarehouseCategoryWid(int compid, int Warehouse_id);
        List<WarehouseCategory> AddToWarehousesCategorys(List<WarehouseCategory> WhCategory);
        List<WarehouseBaseCategory> AddToWarehousesBaseCategorys(List<WarehouseBaseCategory> WhBaseCat);
        List<WarehouseSubCategory> AddToWarehousesSubCategorys(List<WarehouseSubCategory> WhSubCat);
        List<WarehouseSubsubCategory> AddToWarehousesSubSubCategorys(List<WarehouseSubsubCategory> WhSubSubCat);

        List<WarehouseSubsubCategory> AddWarehouseCategoryWid(List<WarehouseSubsubCategory> WarehouseCategory, string desc, int compid, int Warehouse_id);
        List<WarehouseSubsubCategory> AddWarehouseCategory(List<WarehouseSubsubCategory> WarehouseCategory, string desc, int compid);
        List<WarehouseSubsubCategory> PutWarehouseCategory(List<WarehouseSubsubCategory> WarehouseCategory, int compid);

        #endregion

        bool DeleteWarehouseCategory(int id, int compid);
        IEnumerable<WarehouseSubCategory> AllWarehouseSubCategory(int WarehouseSubCategoryid);
        WarehouseSubCategory AddWarehouseSubCategory(WarehouseSubCategory WarehouseSubCategory);
        WarehouseSubCategory PutWarehouseSubCategory(WarehouseSubCategory WarehouseSubCategory);
        bool DeleteWarehouseSubCategory(int id);
        IEnumerable<WarehouseSubsubCategory> AllWarehouseSubsubCat(int compid);
        WarehouseSubsubCategory AddWarehouseSubsubCat(WarehouseSubsubCategory asset);
        WarehouseSubsubCategory AddWhsubsubxl(WarehouseSubsubCategory asset);
        WarehouseSubsubCategory PutWarehouseSubsubCat(WarehouseSubsubCategory asset);
        bool DeletewarehouseSubsubCat(int id);
        IEnumerable<SubCategory> AllSubCategory(int compid);
        IEnumerable<SubCategory> AllSubCategoryy(int subcat, int CompanyId);
        SubCategory AddSubCategory(SubCategory subCategory);
        SubCategory PutSubCategory(SubCategory subCategory);
        bool DeleteSubCategory(int id, int CompanyId);
        IEnumerable<SupplierCategory> AllSupplierCategory(int compid);
        IEnumerable<SupplierCategory> AllSupplierCategoryWid(int compid, int Warehouse_id);
        SupplierCategory AddSupplierCategory(SupplierCategory supplierCategory);
        SupplierCategory PutSupplierCategory(SupplierCategory supplierCategory);
        bool DeleteSupplierCategory(int id, int compid);
        IEnumerable<Supplier> AllSupplier(int compid);
        IEnumerable<Supplier> AllSupplierWid(int compid);
        List<Supplier> AddBulkSupplier(List<Supplier> supCollection);
        Supplier AddSupplier(Supplier supplier);
        Supplier PutSupplier(Supplier supplier);
        bool DeleteSupplier(int id, int CompanyId);
        //By Anushka
        DepoMaster PutDepos(DepoMaster EditDepo);
        //By Anushka

        IEnumerable<BillPramotion> AllBillPramtion(int compid);
        BillPramotion AddBillPramtion(BillPramotion pramtion);
        BillPramotion PutBillPramtion(BillPramotion pramtion);
        bool DeleteBillPramtion(int id);

        IEnumerable<ItemBrand> AllItemBrand(int compid);
        ItemBrand AddItemBrand(ItemBrand itembrand);
        ItemBrand PutItemBrand(ItemBrand itembrand);
        bool DeleteItemBrand(int id, int compid);
        IEnumerable<FinancialYear> AllFinancialYear(int compid);
        FinancialYear AddFinancialYear(FinancialYear financialYear);
        FinancialYear PutFinancialYear(FinancialYear financialYear);
        bool DeleteFinancialYear(int id);
        IEnumerable<OrderMaster> OrderMasterbymobile(string mobile, int compid);
        Warehouse getwarehousebyid(int id, int CompanyId);
        List<SubsubCategory> subcategorybyWarehouse(int id, int compid);
        List<SubsubCategory> Updatebrands(List<SubsubCategory> sub, int compid);
        List<SubsubCategory> UpdateExclusivebrands(List<SubsubCategory> sub, int compid);
        List<SubsubCategory> subcategorybyPramotion(int id, int compid);
        List<SubsubCategory> subcategorybyPramotionExlusive();
        List<SubsubCategory> subcategorybycity(int id, int compid);
        List<SubsubCategory> PramotionalBrand(int warehouseid, int compid);
        //IEnumerable<PurchaseOrderMaster> AllPOMaster(int compid);
        //IEnumerable<PurchaseOrderMaster> AllPOMasterWid(int compid,int Warehouse_id);
        PaggingData AllPOMasterWid(int list, int page, int Warehouseid, int CompanyId);
        PaggingData AllBlankPOWid(int list, int page, int Warehouse_id, int CompanyId);
        PaggingData AllIRMasterWid(int list, int page, int Warehouse_id, int CompanyId, int userId);
        PaggingData AllTOMasterWid(int list, int page, int Warehouseid, int CompanyId);
        PaggingData AllTORequestMasterWid(int list, int page, int ReqWarehouse_id, int CompanyId);
        PaggingData AllPOMaster(int list, int page, int CompanyId);
        IEnumerable<PurchaseOrderDetail> AllPOdetails(int compid);
        IEnumerable<PurchaseOrderDetail> AllPOrderDetails(int i, int compid, int warehouseid);
        //CurrentStock GetCurrentStock(int id, int CompanyId);
        IEnumerable<CurrentStock> GetAllCurrentStock(int CompanyId);
        CurrentStockHistory PutCurrentStock(CurrentStockHistory CurrentStockHistory);
        IEnumerable<CurrentStock> GetAllCurrentStockWid(int CompanyId, int Warehouse_id);
        IEnumerable<CurrentStock> GetAllAdjCurrentStockWid(int CompanyId, int Warehouse_id);
        IEnumerable<CurrentStock> GetAllEmptyStockItem(int CompanyId, int Warehouse_id);
        IEnumerable<CurrentStock> GetAllEmptyStock();
        IEnumerable<CurrentStock> GetAllEmptyStockItemForWeb();
        IEnumerable<CurrentStock> GetAllEmptyStockItemForWeb(int CompanyId, int Warehouse_id);
        IEnumerable<ItemMaster> itembystring(string itemnm, int CompanyId);
        List<ItemMaster> itembystringWid(string itemnm, int CompanyId, int CustomerId);
        List<ItemMaster> SearchitemSaleman(string itemname, int PeopleID);
        IEnumerable<Department> Alldepartment();//BY Hemant 11/12/2018
        bool DeleteDepartment(int id);//BY Hemant 11/12/2018
        //By Sachin For Adding Category Images
        int AddCategoryImage(CategoryImage item);
        int PutCategoryImage(CategoryImage item);
        //By Sachin For To show Category Images
        List<CategoryImageData> AllCategoryImages();
        //By Sachin For Adding Agent
        int AddAgentAmount(int compid, int Warehouse_id, AgentAmount aj);

        //By Sachin For Adding Agent Amount
        int AddAgent(int compid, int Warehouse_id, People aj);

        //By Sachin For Deleting Agent Amount
        bool DeleteAgentAmount(int id, int CompanyId, int Warehouse_id);

        //By Sachin For Deleting Agent
        bool DeleteAgent(int AgentId, int CompanyId, int Warehouse_id);

        //By Sachin Update Agent Amount
        int UpdateAgentAmount(int compid, int Warehouse_id, AgentAmount aj);

        string UpdateBulkCurrentStock(int CompanyId, int Warehouse_id, List<CurrentStock> SelectedItem);

        //By Sachin 
        string UpdateBulkItemMaster(int CompanyId, int Warehouse_id, List<ItemMaster> SelectedItem);

        //By Sachin
        string UpdateHighDP(int CompanyId, int Warehouse_id, List<ItemMaster> SelectedItem);

        GeoFence AddGeoFence(GeoFence geofence);
        //By Sachin For Getting Data from AgentData
        List<AgentData> GetAgentData(string issurenceid, int compid, int Warehouse_id);
        PaggingData_AgentAmount GetAgentAllOrder(int Warehouse_id, int compid, string AgentCode, int list, int page);
        OrderMaster GetOrderMaster(int orderid, int compid);
        DamageOrderMaster GetDOrderMaster(int orderid, int compid);
        PaggingData_wt AllWalletHistory(int list, int page, int CustomerId, int CompanyId, int Warehouseid);
        PaggingData_wt AllWalletHistoryComp(int list, int page, int CustomerId, int CompanyId, int Warehouseid);
        List<OrderDetailForCP> getCiMatrix(int? WarehouseId, DateTime? start, DateTime? end);
        PurchaseOrderDetailRecived AddPurchaseOrderDetailsRecivedInTempCS(PurchaseOrderDetailRecived pd, int count);
        #region Case
        IEnumerable<CaseModule> AllCase();
        CaseModule AddCase(CaseModule cases);
        CaseModule SetCase(CaseModule cases);
        CaseModule Caseviewstatus(CaseModule cases);
        CaseModule PutCase(CaseModule casedata);
        CaseComment AddCommentCase(CaseComment commentdata);
        bool DeleteCase(int id);
        IEnumerable<CaseProject> AllProject();
        IEnumerable<CaseComment> AllComments(int CaseId, int UserId);
        IEnumerable<CaseImage> AllImages();
        IEnumerable<CaseImage> AllImagesByCase(string CaseNumber);
        #endregion
        #region skill
        bool DeleteSkill(int id);
        bool DeleteDocument(int id); //poojaZ
        IEnumerable<Skill> AllSkill();
        #endregion

    }
}