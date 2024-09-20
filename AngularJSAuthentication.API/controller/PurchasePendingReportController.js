
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PurchasePendingReportController', PurchasePendingReportController);

    PurchasePendingReportController.$inject = ['$scope', 'PurchaseReportingService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function PurchasePendingReportController($scope, PurchaseReportingService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        $scope.vm = {};
        $scope.numPerPageOpt = [15, 30, 50, 100];
        $scope.vm.currentPage = 1;
        $scope.vm.numPerPage = $scope.numPerPageOpt[0];
        $scope.vm.totalRecords = 0;
        $scope.vm.warehouseid = null;  // nullable parameter
        $scope.vm.monthList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
        $scope.vm.month = $scope.vm.monthList[0];
        $scope.vm.currentPage = 1;
        $scope.vm.reportData = [];
        $scope.MonthYear = null;
        $scope.vm.warehouseList = [];


        $scope.initialize = function () {
            
            $scope.search();
            //$scope.getWarehouse();
        }

        $(function () {
            
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });


        $scope.onNumPerPageChange = function () {
            $scope.search(1);
            $scope.vm.currentPage = 1;
        }

        $scope.search = function (pageNumber) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            {
                var start = f.val();
                var end = g.val();


                if (pageNumber) {
                    $scope.vm.currentPage = pageNumber;
                } else {
                    $scope.vm.currentPage = 1;
                }
                $scope.vm.reportData = [];
                $scope.vm.warehouseid = $scope.WarehouseId;
                PurchaseReportingService.GetPurchasePendingReportData($scope.vm.numPerPage, $scope.vm.currentPage, $scope.vm.warehouseid, start, end)
                    .then(function (result) {
                        $scope.vm.reportData = result.data.PurchasePendingReportDatas;
                        $scope.vm.totalRecords = result.data.TotalCount;
                        console.log('result: ', result.data);

                    });
            }
        }
        //$scope.getWarehouse = function () {
        //    WarehouseService.getwarehouse().then(function (wList) {
        //        $scope.vm.warehouseList = wList.data;
        //        console.log(' $scope.vm.warehouseList: ', $scope.vm.warehouseList);
        //    });
        //};

        $scope.zones = [];
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

        $scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //}; --warehouse standardization

        $scope.GetWarehouses = function (warehouse) {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseCommonByRegion?RegionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };




        //$scope.clusters = [];
        //$scope.GetClusters = function (cluster) {
        //    var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.clusters = response;
        //        });
        //};

        $scope.initialize();

        //for use export list
        alasql.fn.myfmt = function (n) {

            return Number(n).toFixed(2);
        };
        $scope.exportData = function () {
            //
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            {
                var start = f.val();
                var end = g.val();
                $scope.vm.reportExportData = [];
                $scope.vm.warehouseid = $scope.WarehouseId;
                PurchaseReportingService.GetPurchasePendingReportExportData($scope.vm.warehouseid, start, end)
                    .then(function (result) {
                        $scope.vm.reportExportData = result.data.PurchasePendingReportDatas;
                        $scope.vm.totalRecords = result.data.TotalCount;
                        console.log('result: ', result.data);

                    });

                console.log($scope.vm.reportExportData);
                alasql('SELECT WarehouseId,WarehouseName,itemname,DemandQty,Stock,Duestock,OtherHubStock INTO XLSX("Report.xlsx",{headers:true}) FROM ?', [$scope.vm.reportExportData]);
            }
        };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('FillRateCutReportController', FillRateCutReportController);

    FillRateCutReportController.$inject = ['$scope', 'PurchaseReportingService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function FillRateCutReportController($scope, PurchaseReportingService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        

        $scope.vm = {};
        $scope.numPerPageOpt = [15, 30, 50, 100];
        $scope.vm.currentPage = 1;
        $scope.vm.numPerPage = $scope.numPerPageOpt[0];
        $scope.vm.totalRecords = 0;
        $scope.vm.warehouseid = null;  // nullable parameter
        $scope.vm.monthList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
        $scope.vm.month = $scope.vm.monthList[0];
        $scope.vm.currentPage = 1;
        $scope.vm.reportData = [];
        $scope.MonthYear = null;
        $scope.vm.warehouseList = [];

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        $scope.initialize = function () {
            
            $scope.search();
            $scope.GetWarehouses();
            //$scope.GetZones();
        };

        $scope.zones = [];
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

        $scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
            
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //}; --warehouse standardizaTION

        $scope.GetWarehouses = function (warehouse) {

            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseCommonByRegion?RegionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };



        $scope.onNumPerPageChange = function () {

            $scope.search();
            $scope.vm.currentPage = 1;
        };

        $scope.search = function (pageNumber) {
            
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            {
                var start = f.val();
                var end = g.val();

                if (pageNumber) {
                    $scope.vm.currentPage = pageNumber;
                } else {
                    $scope.vm.currentPage = 1;
                }
                $scope.vm.reportData = [];

                PurchaseReportingService.GetFillRateCutReportData($scope.vm.numPerPage, $scope.vm.currentPage, $scope.vm.warehouseid, start, end)
                    .then(function (result) {
                        
                        $scope.vm.reportData = result.data.FillRateCutReportDatas;
                        $scope.vm.totalRecords = result.data.TotalCount;
                        console.log('result: ', result.data);

                    });
            }
        };

        
        //$scope.getWarehouse = function () {
        //    WarehouseService.getwarehouse().then(function (wList) {
        //        $scope.vm.warehouseList = wList.data;
        //        console.log(' $scope.vm.warehouseList: ', $scope.vm.warehouseList);
        //    });
        //}

        //$scope.initialize();

        //for use export list
    
        $scope.exportData1 = function(warehouseid) {
            
           
            if (warehouseid == null || warehouseid == "" || warehouseid == undefined) {
                alert('please select warehouse');
                return;
            }  
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            {
                var start = f.val();
                var end = g.val();
            }
            if (start == undefined || end == undefined) {
                alert('please select Date Range');
                return;
            }
            
            var url = serviceBase + 'api/PurchaseReporting/ExportGetFillRateCutReportData?warehouseid=' + warehouseid + '&startDate=' + start + '&endDate='+end;
            $http.get(url)
                .success(function(response) {
                    
                    $scope.Alldata = response;
                    alasql('SELECT WarehouseId,Warehouse,SupplierName,ItemName,OrderQty,supplyQty,quantityNotDispatch,CancelOrderQty,DamageStockQuantity,NotInStockQuantity,NotInStockComment,ItemName,DamageStockQuantity,DamageComment,Billsaffected INTO XLSX("Report.xlsx",{headers:true}) FROM ?', [$scope.Alldata]);
                });
        };

    }
})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('PurchaseReportingService', PurchaseReportingService);

//    PurchaseReportingService.$inject = ['$http', 'ngAuthSettings'];

//    function PurchaseReportingService($http, ngAuthSettings) {
//        //
//        var serviceBase = ngAuthSettings.apiServiceBaseUri;

//        var serviceObject = {};

//        serviceObject.GetPurchasePendingReportData = function (totalitem, page, warehouseid, startDate, endDate) {
//            return $http.get(serviceBase + 'api/PurchaseReporting/GetPurchasePendingReportData?totalitem=' + totalitem + '&page=' + page + '&warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
//        };

//        serviceObject.GetFillRateCutReportData = function (totalitem, page, warehouseid, startDate, endDate) {
//            return $http.get(serviceBase + 'api/PurchaseReporting/GetFillRateCutReportData?totalitem=' + totalitem + '&page=' + page + '&warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
//        };
//        serviceObject.GetPurchasePendingReportExportData = function (warehouseid, startDate, endDate) {
//            //
//            return $http.get(serviceBase + 'api/PurchaseReporting/GetPurchasePendingReportDataExport?warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
//        };


//        return serviceObject;
//    }
//})();

(function () {
    'use strict';

    angular
        .module('app')
        .factory('PurchaseReportingService', PurchaseReportingService);

    PurchaseReportingService.$inject = ['$http', 'ngAuthSettings'];

    function PurchaseReportingService($http, ngAuthSettings) {
        //
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var serviceObject = {};

        serviceObject.GetPurchasePendingReportData = function (totalitem, page, warehouseid, startDate, endDate) {
            return $http.get(serviceBase + 'api/PurchaseReporting/GetPurchasePendingReportData?totalitem=' + totalitem + '&page=' + page + '&warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
        };

        serviceObject.GetFillRateCutReportData = function (totalitem, page, warehouseid, startDate, endDate) {
            return $http.get(serviceBase + 'api/PurchaseReporting/GetFillRateCutReportData?totalitem=' + totalitem + '&page=' + page + '&warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
        };
        serviceObject.GetPurchasePendingReportExportData = function (warehouseid, startDate, endDate) {
            //
            return $http.get(serviceBase + 'api/PurchaseReporting/GetPurchasePendingReportDataExport?warehouseid=' + warehouseid + '&startDate=' + startDate + '&endDate=' + endDate);
        };


        return serviceObject;
    }
})();



