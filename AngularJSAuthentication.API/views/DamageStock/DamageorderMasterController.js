'use strict';
app.controller('DamageorderMasterController', ['$scope', 'WarehouseService','DamageOrderMasterService', 'DamageOrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, WarehouseService, DamageOrderMasterService, DamageOrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        console.log("orderMasterController start loading OrderDetailsService");



        //$scope.OrderType = 0;
        $scope.statusCompgetData = [];
        $scope.zones = [];
        $scope.TrShow = false;
        $scope.GetZones = function () {
           
            var url = serviceBase + 'api/inventory/GetZone';
            $http.get(url)
                .success(function (response) {
                    $scope.zones = response;
                });
        };
        $scope.GetZones();

        $scope.regions = [];
        $scope.GetRegions = function (zone) {
           
            var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            $http.get(url)
                .success(function (response) {
                    $scope.regions = response;
                });
        };

        $scope.city = [];
        $scope.GetCityList = function () {
            var url = serviceBase + "api/Warehouse/getSpecificCitiesforuser";             //Vinayak refer to show specific cities for login user  
            $http.get(url).success(function (results) {
                $scope.city = results;
            });

            //OrderMasterService.getcitys().then(function (results) {
            //    $scope.cities = results.data;

            //});
        };
        $scope.GetCityList();

        $scope.MultiCityModel = [];
        $scope.MultiCity = $scope.city;
        $scope.MultiCityModelsettings = {
            displayProp: 'CityName', idProp: 'Cityid',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };


        //Added
        
        $scope.warehouseDSFilter = [];
        $scope.MultiWarehouseModel = [];
        //$scope.MultiWarehouse = $scope.warehouses;
        $scope.MultiWarehouse = $scope.warehouseDSFilter;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };


        //$scope.MultiWarehouseModel = [];
        $scope.warehouses = [];
        $scope.dataselect = [];
        $scope.GetWarehouses = function (warehouse) {
            
            var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
  
                    //Changed
                    //$scope.MultiWarehouseModel = response;
                    $scope.dataselect = response;
                   
                    $scope.WHMasters = response;
              
                    $scope.warehouseDSFilter = _.filter($scope.WHMasters, function (obj) {
                        if (obj) {
                            
                            obj.WarehouseName = obj.WarehouseName + " >> '";
                            return obj.RegionId == warehouse; 
                        }
                    })
                    response.warehouseDSFilter;
                });
        };
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var warehouseids = UserRole.Warehouseids;
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));
     
        $scope.getWarehosues = function (CityId) {
            $scope.MultiWarehouseModel = [];
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var url = serviceBase + "api/Warehouse/GetWarehouseCitiesOnOrder";
            if (citystts != '') {
                $http.post(url, citystts).success(function (response) {

                    var assignWarehouses = [];
                    var KPPassignWarehouses = [];
                    if (response) {
                        angular.forEach(response, function (value, key) {
                            if (!value.IsKPP) {
                                if (warehouseids.indexOf(value.WarehouseId) !== -1) {
                                    assignWarehouses.push(value);
                                }
                            } else if (value.IsKPP) {
                                if (warehouseids.indexOf(value.WarehouseId) !== -1) {
                                    KPPassignWarehouses.push(value);
                                }
                            }
                        });
                    }
                    $scope.warehouseDSFilter = assignWarehouses;
                    $scope.KPPwarehouse = KPPassignWarehouses;
                    if (assignWarehouses.length == 1 && warehouseids.indexOf(",") == -1) {
                        $scope.srch.WarehouseId = warehouseids;
                    }
                });
            }
        }

         // Added and commented

        //$scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    
        //    var url = serviceBase + 'api/inventory/PostWarehouseData';
        //    $http.post(url, warehouse)
        //        .success(function (response) {
        //            //$scope.warehouses = response;
        //            $scope.MultiWh = response;
        //        });
        //};

       


        //$scope.warehouse = [];
        //$scope.warehouseId = '';
        //WarehouseService.getwarehouse().then(function (results) {
        //    
        //    console.log(results.data);
        //    console.log("data");
        //   // $scope.warehouse = results.data;
        //    $scope.warehouse = MultiWarehouseModel;
        //}, function (error) {
        //});


        $scope.currentPageStores = {};
        $scope.statuses = [];
        $scope.orders = [];
        $scope.customers = [];
        $scope.selectedd = {};        

        // new pagination 
        $scope.pageno = 1; //initialize page no to 1
        $scope.total_count = 0;

        $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

        $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page

        $scope.onNumPerPageChange = function () {

            $scope.itemsPerPage = $scope.selected;

        }
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

        //$scope.$on('$viewContentLoaded', function () {
        //    $scope.getData($scope.pageno);
        //});
        
        //$scope.getData = function (pageno) {       
        //    // This would fetch the data on page change.
        //    //In practice this should be in a factory.
        //    
        //    console.log("In get data function");
        //    $scope.customers = [];
        //    $scope.dataList = [];
        //    var url = serviceBase + "api/DamageOrderMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.warehouseId;
        //    $http.get(url).success(function (response) {
        //        $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data
        //        $scope.dataList = angular.copy(response.ordermaster);
        //        $scope.orders = $scope.customers;
        //        $scope.total_count = response.total_count;
        //        $scope.tempuser = response.ordermaster;
        //        $scope.statusCompgetData = response.Status;

        //        if ($scope.total_count > 0) {
        //            $scope.TrShow = true;
        //        }
        //        else {
        //            $scope.TrShow = false;
        //        }                
        //    });
        //};

        // Added
        $scope.getData = function (pageno, stts) {
            
            // This would fetch the data on page change.
            //In practice this should be in a factory.
            
            console.log("In get data function");
            $scope.customers = [];
            $scope.dataList = [];

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            if ($scope.OrderType == '' ) {
                alert("Please select OrderType");
                return;
            }
            //var stts = "";
            //if ($scope.statusname.name && $scope.statusname.name != "Show All") {
            //    stts = $scope.statusname.name;
            //}

            let whids = $scope.MultiWarehouseModel.map(a => a.id);
            let itemsPP = $scope.itemsPerPage;
           // $scope.Warehouseid = $scope.wid;

            //var whids = [];
            //_.each($scope.MultiWarehouseModel, function (o2) {
            //    console.log(o2);
            //    for (var i = 0; i < $scope.dataselect.length; i++) {
            //        if ($scope.dataselect[i].WarehouseId === o2.id) {
            //          
            //            //var Row =
            //            //{
            //            //    "id": o2.id
            //            //};
            //            whids.push(o2.id);
            //        }
            //    }
            //})

            
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
               
                var url = serviceBase + "api/DamageOrderMaster/PostDamageOrderbyStatus" + "?list=" + itemsPP + "&page=" + pageno + "&WarehouseId=" + whids + "&Orderstatus=" + $scope.statusname.name + "&start=" + $scope.start + "&end=" + $scope.end + "&OrderType=" + $scope.OrderType;
            }
            else {
                
                var url = serviceBase + "api/DamageOrderMaster/PostDamageOrder" + "?list=" + itemsPP + "&page=" + pageno + "&WarehouseId=" + whids + "&start=" + $scope.start + "&end=" + $scope.end + "&OrderType=" + $scope.OrderType;
            }

        //    var url = serviceBase + "api/DamageOrderMaster/PostDamageOrder" + "?list=" + itemsPP + "&page=" + pageno + "&WarehouseId=" + whids;
            $http.post(url, whids)
                .success(function (response) {
                    
                $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data
                $scope.dataList = angular.copy(response.ordermaster);
                $scope.orders = $scope.customers;
                $scope.total_count = response.total_count;
                    $scope.tempuser = response.ordermaster;
                    console.log("rsponse", response.ordermaster)
                //$scope.statusCompgetData = response.Status;

                if ($scope.total_count > 0) {
                    $scope.TrShow = true;
                }
                else {
                    $scope.TrShow = false;
                }

            });
        };

        // Added

        //$scope.getWData = function (WarehouseId)
        $scope.getWData = function ()
        {
            
            if ($scope.OrderType == 0 || $scope.OrderType == 1 || $scope.OrderType == -1 || $scope.OrderType == 2) {

                $scope.AdvanceSearch();
                //$scope.warehouseId = WarehouseId;
                $scope.getData($scope.pageno, $scope.statusname.name);
            }
            else {
                alert("Please select OrderType");
                return;
            }
      
        }
        $scope.AdvanceSearch = function () {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            $scope.start = f.val();
            $scope.end = g.val();
            if (!$('#dat').val()) {
                $scope.start = null;
                $scope.end = null;
             /*   alert("Please select one parameter");*/
                return;
            } else if ($scope.start != null && $scope.end == null) {
                alert("Please select end date!");
            } else if ($scope.start == null && $scope.end != null) {
                alert("Please select start date!");
            }
        }
        // Added

        $scope.Export = function () {
           
            if ($scope.customers != null)
            {
                $scope.customers.forEach(x => {
                    if (x.APP != null && x.APP > 0) {
                        x.APP_UnitPrice_MRP = x.APP;
                        if (x.UnitPrice != null && x.UnitPrice > 0) {
                            x.SellingPrice = x.UnitPrice;
                        }                       
                    } else if (x.UnitPrice != null && x.UnitPrice > 0) {
                        x.SellingPrice = x.UnitPrice;
                        x.APP_UnitPrice_MRP = x.DefaultUnitPrice;
                    } else if (x.MRP != null && x.MRP > 0) {
                        x.APP_UnitPrice_MRP = x.MRP;
                        if (x.UnitPrice != null && x.UnitPrice > 0) {
                            x.SellingPrice = x.UnitPrice;
                        }
                        /*x.SellingPrice = x.MRP;*/
                    }
                });
            }
            alasql('SELECT WarehouseName,DamageOrderId,OrderId,invoice_no,Skcode,ShopName,ClusterName,SalesPerson,UpdatedDate,Status,TotalAmount,OrderType,APP_UnitPrice_MRP,SellingPrice INTO XLSX("DamageOrder.xlsx",{headers:true})FROM ?', [$scope.customers]);


        }

       
        //$scope.Refresh = function () {
        //    $scope.srch = "";
        //    $scope.getData($scope.pageno);
        //}
        
        $scope.getTemplate = function (data) {
            if (data.OrderId === $scope.selectedd.OrderId) {
                myfunc();
                return 'edit';
            }
            else return 'display';
        };

        // end pagination
        $scope.set_color = function (trade) {
            if (trade.Status == "Process") {
                return { background: "#81BEF7" }
            }
            else if (trade.Status == "Ready to Dispatch") {
                return { background: "#00FF7F" }
            }
            else if (trade.Status == "Order Canceled") {
                return { background: "#F78181" }
            }
        }

        // status based filter
        $scope.filterOptions = {
            stores: [
              { id: 1, name: 'Show All' },
              { id: 2, name: 'Pending' },
              { id: 3, name: 'Ready to Dispatch' },
              { id: 4, name: 'Issued' },
              { id: 12, name: 'Shipped' },
              { id: 5, name: 'Delivery Redispatch' },
              { id: 6, name: 'Delivered' },
              { id: 7, name: 'sattled' },
              { id: 8, name: 'Partial settled' },
              { id: 9, name: 'Account settled' },
              { id: 10, name: 'Delivery Canceled' },
              { id: 11, name: 'Order Canceled' }
            ]
        };
        
        $scope.filterItem = {
            store: $scope.filterOptions.stores[0]
        }
        $scope.customFilter = function (data) {
           
            if (data.Status === $scope.filterItem.store.name) {
                return true;
            } else if ($scope.filterItem.store.name === 'Show All') {
                return true;
            } else {
                return false;
            }
        };

        $scope.editstatus = function (data) {

            $scope.selectedd = data;
            if (data.Status == "Delivery Canceled") {
                $scope.statuses = [
                 { value: 1, text: "Order Canceled" }
                ];
            }
            else if (data.Status == "Pending") {
                $scope.statuses = [
                     { value: 1, text: "Order Canceled" }
                    // ,{ value: 2, text: "Process" }
                ];
            }
            else {
                // $scope.statuses = [
                //{ value: 1, text: data.Status },
                // ];
                $scope.statuses = data.Status;
            }
        };

        function myfunc() {
            $('#orderStatus').on('change', function () {
                $(".saveBtn").prop("disabled", false);
            })
        }

        $scope.reset = function () {
            $scope.selectedd = {};
        };

        //$scope.updatestatus = function (data) {
        //    OrderMasterService.editstatus(data).then(function (results) {
        //        $scope.reset();
        //        return results;
        //    });
        //}

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        //$scope.cities = [];
        //OrderMasterService.getcitys().then(function (results) {
        //    $scope.cities = results.data;
        //}, function (error) {
        //});

        //$scope.warehouse = [];
        //OrderMasterService.getwarehouse().then(function (results) {
        //    $scope.warehouse = results.data;
        //}, function (error) {
        //});

        $scope.InitialData = [];
        $scope.DemandDetails = [];
        $scope.srch = "";
        $scope.statusname = {};
        $scope.searchdata = function (data) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            
            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            else {
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            $scope.orders = [];
            $scope.customers = [];
            //// time cunsume code  
            var stts = "";
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                stts = $scope.statusname.name; 
            }
            var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts;
            $http.get(url).success(function (response) {
                
                $scope.customers = response;  //ajax request to fetch data into vm.data
                $scope.total_count = response.length;
                //$scope.orders = response;
            });

        }

        //............................Exel export Method...........................// Anil
        $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
        $scope.exportData = function (data) {
         
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            $scope.OrderByDate = [];           
            var start = f.val();
            var end = g.val();
            
            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            else {
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
            $http.get(url).success(function (response) {
                $scope.OrderByDate = response;
                console.log("export");
                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ")
                }
                else {
                    $scope.NewExportData = [];
                    for (var i = 0; i < $scope.OrderByDate.length; i++) {

                        var OrderId = $scope.OrderByDate[i].OrderId;
                        var Skcode = $scope.OrderByDate[i].Skcode;
                        var ShopName = $scope.OrderByDate[i].ShopName;
                        var OrderBy = $scope.OrderByDate[i].OrderBy;
                        var Mobile = $scope.OrderByDate[i].Customerphonenum;
                        var CustomerName = $scope.OrderByDate[i].CustomerName;
                        var CustomerId = $scope.OrderByDate[i].CustomerId;
                        var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                        var ClusterName = $scope.OrderByDate[i].ClusterName;
                        var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                        var CompanyId = $scope.OrderByDate[i].CompanyId;
                        var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                        var CreatedDate = $scope.OrderByDate[i].CreatedDate;
                        var SalesPerson = $scope.OrderByDate[i].SalesPerson;
                        var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                        var delCharge = $scope.OrderByDate[i].deliveryCharge;
                        var Status = $scope.OrderByDate[i].Status;
                        var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                        var comments = $scope.OrderByDate[i].comments;
                        var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                        for (var j = 0; j < orderDetails.length; j++) {
                            var tts = {
                                OrderId: '', Skcode: '', ShopName: '', Mobile: '', OrderBy: '', RetailerName: '', RetailerID: '', Warehouse: '', ClusterName: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                Excecutive: '', ShippingAddress: '', deliveryCharge: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                DiscountPercentage: '', TaxPercentage: '', Tax: '', TotalAmt: '', CategoryName: '', BrandName: '', Status: '', ReasonCancle: '', comments: '',
                            }
                            tts.ItemID = orderDetails[j].ItemId;
                            tts.ItemName = orderDetails[j].itemname;
                            tts.SKU = orderDetails[j].sellingSKU;
                            tts.itemNumber = orderDetails[j].itemNumber;
                            tts.MRP = orderDetails[j].price;
                            tts.UnitPrice = orderDetails[j].UnitPrice;
                            tts.Quantity = orderDetails[j].qty;
                            tts.MOQPrice = orderDetails[j].MinOrderQtyPrice;
                            tts.Discount = orderDetails[j].DiscountAmmount;
                            tts.DiscountPercentage = orderDetails[j].DiscountPercentage;
                            tts.TaxPercentage = orderDetails[j].TaxPercentage;
                            tts.Tax = orderDetails[j].TaxAmmount;
                            tts.TotalAmt = orderDetails[j].TotalAmt;
                            tts.CategoryName = orderDetails[j].CategoryName;
                            tts.BrandName = orderDetails[j].BrandName;

                            tts.OrderId = OrderId;
                            tts.RetailerID = CustomerId;
                            tts.OrderBy = OrderBy;
                            tts.Skcode = Skcode;
                            tts.Mobile = Mobile;
                            tts.RetailerName = CustomerName;
                            tts.Warehouse = WarehouseName;
                            tts.ClusterName = ClusterName;
                            tts.ShopName = ShopName;
                            tts.Deliverydate = Deliverydate;
                            tts.CompanyId = CompanyId;
                            tts.BillingAddress = BillingAddress;
                            tts.Date = CreatedDate;
                            tts.Excecutive = SalesPerson;
                            tts.ShippingAddress = ShippingAddress;
                            tts.deliveryCharge = delCharge;
                            tts.Status = Status;
                            tts.ReasonCancle = ReasonCancle;
                            tts.comments = comments;
                            $scope.NewExportData.push(tts);

                        }
                    }
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                    alasql('SELECT OrderId,Skcode,ShopName,RetailerName,Mobile,ItemID,ItemName,CategoryName,BrandName,SKU,Warehouse,ClusterName,Date,OrderBy,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,TotalAmt,deliveryCharge,Status,ReasonCancle,comments INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                }
            });
        };
        //-------------------------------------------------------------------------//

        //............................Exel export Method...........................// Anil
        $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
        $scope.exportDataAll = function (data) {
            
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            $scope.OrderByDate = [];
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            else {
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.skcode) {
                    $scope.srch.skcode = "";
                }
                if (!$scope.srch.shopName) {
                    $scope.srch.shopName = "";
                }
                if (!$scope.srch.mobile) {
                    $scope.srch.mobile = "";
                }
                if (!$scope.srch.status) {
                    $scope.srch.status = "";
                }
            }
            var url = serviceBase + "api/SearchOrder/all?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
            $http.get(url).success(function (response) {
                $scope.OrderByDate = response;
                console.log("export");
                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ")
                }
                else {
                    $scope.NewExportData = [];
                    for (var i = 0; i < $scope.OrderByDate.length; i++) {

                        var OrderId = $scope.OrderByDate[i].OrderId;
                        var Skcode = $scope.OrderByDate[i].Skcode;
                        var ShopName = $scope.OrderByDate[i].ShopName;
                        var OrderBy = $scope.OrderByDate[i].OrderBy;
                        var Mobile = $scope.OrderByDate[i].Customerphonenum;
                        var CustomerName = $scope.OrderByDate[i].CustomerName;
                        var CustomerId = $scope.OrderByDate[i].CustomerId;
                        var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                        var ClusterName = $scope.OrderByDate[i].ClusterName;
                        var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                        var CompanyId = $scope.OrderByDate[i].CompanyId;
                        var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                        var CreatedDate = $scope.OrderByDate[i].CreatedDate;
                        var SalesPerson = $scope.OrderByDate[i].SalesPerson;
                        var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                        var delCharge = $scope.OrderByDate[i].deliveryCharge;
                        var Status = $scope.OrderByDate[i].Status;
                        var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                        var comments = $scope.OrderByDate[i].comments;
                        var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                        for (var j = 0; j < orderDetails.length; j++) {
                            var tts = {
                                OrderId: '', Skcode: '', ShopName: '', Mobile: '', OrderBy: '', RetailerName: '', RetailerID: '', Warehouse: '', ClusterName: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                Excecutive: '', ShippingAddress: '', deliveryCharge: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                DiscountPercentage: '', TaxPercentage: '', Tax: '', TotalAmt: '', CategoryName: '', BrandName: '', Status: '', ReasonCancle: '', comments: '',
                            }
                            tts.ItemID = orderDetails[j].ItemId;
                            tts.ItemName = orderDetails[j].itemname;
                            tts.SKU = orderDetails[j].sellingSKU;
                            tts.itemNumber = orderDetails[j].itemNumber;
                            tts.MRP = orderDetails[j].price;
                            tts.UnitPrice = orderDetails[j].UnitPrice;
                            tts.Quantity = orderDetails[j].qty;
                            tts.MOQPrice = orderDetails[j].MinOrderQtyPrice;
                            tts.Discount = orderDetails[j].DiscountAmmount;
                            tts.DiscountPercentage = orderDetails[j].DiscountPercentage;
                            tts.TaxPercentage = orderDetails[j].TaxPercentage;
                            tts.Tax = orderDetails[j].TaxAmmount;
                            tts.TotalAmt = orderDetails[j].TotalAmt;
                            tts.CategoryName = orderDetails[j].CategoryName;
                            tts.BrandName = orderDetails[j].BrandName;

                            tts.OrderId = OrderId;
                            tts.RetailerID = CustomerId;
                            tts.OrderBy = OrderBy;
                            tts.Skcode = Skcode;
                            tts.Mobile = Mobile;
                            tts.RetailerName = CustomerName;
                            tts.Warehouse = WarehouseName;
                            tts.ClusterName = ClusterName;
                            tts.ShopName = ShopName;
                            tts.Deliverydate = Deliverydate;
                            tts.CompanyId = CompanyId;
                            tts.BillingAddress = BillingAddress;
                            tts.Date = CreatedDate;
                            tts.Excecutive = SalesPerson;
                            tts.ShippingAddress = ShippingAddress;
                            tts.deliveryCharge = delCharge;
                            tts.Status = Status;
                            tts.ReasonCancle = ReasonCancle;
                            tts.comments = comments;
                            $scope.NewExportData.push(tts);

                        }
                    }
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                    alasql('SELECT OrderId,Skcode,ShopName,RetailerName,Mobile,ItemID,ItemName,CategoryName,BrandName,SKU,Warehouse,ClusterName,Date,OrderBy,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,TotalAmt,deliveryCharge,Status,ReasonCancle,comments INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                }
            });
        };
        //-------------------------------------------------------------------------//




        $scope.show = true;
        $scope.order = false;

        $scope.showalldetails = function () {
            $scope.order = !$scope.order;
            $scope.show = !$scope.show;
        };


        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.orders,
            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
            },

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            },

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
            },

            $scope.numPerPageOpt = [3, 5, 10, 20],
            $scope.numPerPage = $scope.numPerPageOpt[2],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

        ()
            $scope.apply();

        };

        $scope.Return = function (data) {
            
            DamageOrderMasterService.saveReturn(data);
            console.log("Order Detail Dialog called ...");
        };
        $scope.mydata = {};

        $scope.showDetail = function (data) {
            
            $http.get(serviceBase + 'api/DamageOrderMaster?id=' + data.DamageOrderId + '&OrderType=' + $scope.OrderType).then(function (results) {
    
                $scope.myData = results.data;
                console.log($scope.myData,"$scope.myData")
                DamageOrderMasterService.save($scope.myData)
                setTimeout(function () {
                    console.log("Modal opened Orderdetails");
                    var modalInstance;
                   
                    modalInstance = $modal.open(
                        {
                            templateUrl: "myOrderdetail.html",
                            controller: "DamageorderdetailsController", resolve: { Ord: function () { return $scope.items } }
                        }), modalInstance.result.then(function (selectedItem) {
                         
                            console.log("modal close");
                            console.log(selectedItem);
                            if (selectedItem.Status == "Ready to Dispatch") {
                                $("#st" + selectedItem.OrderId).html(selectedItem.Status);
                                $("#st" + selectedItem.OrderId).removeClass('canceled');
                                $('#re' + selectedItem.OrderId).hide();
                            }
                                                       
                        },
                        function () {
                            console.log("Cancel Condintion");

                        })
                }, 500);
               
            }); 
            console.log("Order Detail Dialog called ...");
        };

        $scope.showInvoice = function (data) {
            
            DamageOrderMasterService.save1(data);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteOrderInvoice.html",
                    controller: "ModalInstanceCtrlOrderInvoice",
                    resolve:
                        {
                            order: function () {
                                return data
                            }
                        }
                }), modalInstance.result.then(function () {
                },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.open = function (data) {
            
            DamageOrderMasterService.save(data);
        };

        $scope.invoice = function (data) {
            DamageOrderMasterService.view(data).then(function (results) {
            }, function (error) {
            });
        };
        
        $scope.callmethoddetails = function () {
            var init;
            return $scope.stores = $scope.orderDetails,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
            ()
        }
        $scope.editOrder = function (data) {
            
            var myData = { all: $scope.orders, order1: data };
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalEditOrderDetails.html",
                    controller: "ModalInstanceCtrlEditOrder",
                    resolve:
                        {
                            order: function () {
                                return myData
                            }
                        }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                function () {
                    console.log("Cancel Condintion");
                })
        };

         
    }]);

app.controller("ModalInstanceCtrlOrderDetail", ["$scope", '$http', 'DamageOrderMasterService', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http, DamageOrderMasterService, $modalInstance, ngAuthSettings, order) {
        console.log("order detail modal opened");

        
        $scope.orderDetails = [];
        if (order) {

            $scope.OrderData = order;
            $scope.orderDetails = $scope.OrderData.orderDetails;
            console.log("found order");

            console.log($scope.OrderData);
            console.log($scope.OrderData.orderDetails);
            console.log($scope.orderDetails);
        }
        $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.Details = function (dataToPost, $index) {

            console.log("Show Order Details  controller");
            console.log(dataToPost);

            DamageOrderMasterService.getDeatil.then(function (results) {

                console.log("Details");

                console.log("index of item " + $index);
                console.log($scope.order.length);
                $scope.order.splice($index, 1);
                console.log($scope.order.length);

                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }]);

app.controller("ModalInstanceCtrlOrderInvoice", ["$scope", '$http', 'DamageOrderMasterService', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http, DamageOrderMasterService, $modalInstance, ngAuthSettings, order) {
        console.log("order invoice modal opened");

        

        $scope.OrderDetails = {};
        $scope.OrderData = {};
        var d = OrderMasterService.getDeatil();

        $scope.OrderData = d;
        $scope.orderDetails = d.orderDetails;


        $scope.Itemcount = 0;


        for (var i = 0 ; i < $scope.orderDetails.length; i++) {

            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }

        $scope.totalfilterprice = 0;
        _.map($scope.OrderData.orderDetails, function (obj) {
            console.log("count total");

            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log(obj.TotalAmt);
            console.log($scope.totalfilterprice);

        })

    }]);

app.controller("ModalInstanceCtrlEditOrder", ["$scope", '$http', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http,  $modalInstance, ngAuthSettings, myData) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        };
        

        $scope.orders = [];


        if (myData) {

            $scope.orders = myData.order1;

        }
            $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


                $scope.Savedata = function (orderdata) {
                
                if (orderdata.OrderType == 'Damage Order') {
                    orderdata.OrderTypeNo = 0;
                } else if (orderdata.OrderType == 'Non Sellable Order') {
                    orderdata.OrderTypeNo = 1;
                } else if (orderdata.OrderType == 'Non Revenue Order') {
                    orderdata.OrderTypeNo = 2;
                }
               
                var dataToPost = {
                    Status: orderdata.Status,
                    DamageOrderId: orderdata.DamageOrderId,
                    ReasonCancle: orderdata.orderdata,
                    OrderType: orderdata.OrderTypeNo,
                    ReasonCancle: orderdata.ReasonCancle
                };
 
                $http.put(serviceBase + 'api/DamageOrderMaster/EditDamage', dataToPost).then(function (results) {
                    if (results.data) {
                        alert('Update Sucessfully');
                        $modalInstance.close();
                    } else {
                        alert('Not Updated');
                    }
                });

            };

    }]);
