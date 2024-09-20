

(function () {
    'use strict';

    angular
        .module('app')
        .controller('InventoryReportController', InventoryReportController);

    InventoryReportController.$inject = ['$scope', "$http", '$modal', 'ngTableParams', '$routeParams', "localStorageService", "WarehouseService", '$filter'];

    function InventoryReportController($scope, $http, $modal, ngTableParams, $routeParams, localStorageService, WarehouseService, $filter) {
        $scope.currentPage = {};

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });

       

        $scope.ExportAllD = function () {

            alasql('SELECT ItemCode,ItemName,Tax,OpeningStock,OpeningStockAmount,InWardTotal,InwardTotalAmountWithoutTax,OutwardQuantity,OutwardWithoutTaxAmount,ClosingQty,AdjClosingAmount,DifferenceQty,pilferageAmount,GrossMargin INTO XLSX("InventoryReport.xlsx",{headers:true}) FROM ?', [$scope.InventoryReport]);
        }

        $scope.InventoryReport = [];
        $scope.ExportData = function (warehouseid) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            var url = serviceBase + 'api/inventory/GetInventoryReport?warehouseId=' + warehouseid + '&from=' + start + '&to=' + end;
            $http.get(url)
                .success(function (response) {
                    
                   // 
                    $scope.InventoryReport = response;
                    $scope.callmethod();
                });
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
        $scope.GetWarehouses = function (warehouse) {
            var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
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


        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });
        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.InventoryReport;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }

        $scope.select = function (page) {        
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);
            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();
