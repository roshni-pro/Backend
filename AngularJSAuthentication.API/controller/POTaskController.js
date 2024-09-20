

(function () {
    'use strict';

    angular
        .module('app')
        .controller('POTaskController', POTaskController);

    POTaskController.$inject = ['$scope', 'OrderMasterService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function POTaskController($scope, OrderMasterService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        ///////////////////////////////////////////////////////////    



        $scope.warehouse ="";
        //OrderMasterService.getwarehouse().then(function (results) {
        //    $scope.warehouse = results.data;
        //    $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
        //    $scope.getData($scope.WarehouseId);
        //}, function (error) {
        //});

        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                    $scope.getData($scope.WarehouseId);
                });

        };
        $scope.wrshse();



        $scope.currentPageStores = {};

        $scope.getData = function (WarehouseId) {

            $scope.WarehouseId = WarehouseId;
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderMaster/Pending48hr" + "?WarehouseId=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                //$scope.currentPageStores = $scope.itemMasters;
                $scope.callmethod();
            });
        };

        $scope.getData();
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.itemMasters;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";



            $scope.numPerPageOpt = [30, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
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