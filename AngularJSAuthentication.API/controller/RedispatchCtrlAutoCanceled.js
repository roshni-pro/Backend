

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RedispatchCtrlAutoCanceled', RedispatchCtrlAutoCanceled);

    RedispatchCtrlAutoCanceled.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'DeliveryService', 'WarehouseService'];

    function RedispatchCtrlAutoCanceled($scope, OrderMasterService, OrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal, DeliveryService, WarehouseService) {
        console.log("RedispatchCtrlAutoCanceled start loading OrderDetailsService");
        $scope.currentPageStores = {};
        $scope.warehouse = [];
        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {
        //        $scope.warehouse = results.data;
        //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

        //        //  $scope.getdborders($scope.pageno);

        //    }, function (error) {
        //    })
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                $scope.warehouse = data;
                $scope.WarehouseId = $scope.warehouse[0].value;

                //  $scope.getdborders($scope.pageno);
                });

        };
        $scope.wrshse();
        //$scope.getWarehosues();

        $scope.DBoys = [];
        DeliveryService.getdboys().then(function (results) {

            $scope.DBoys = results.data;
        }, function (error) {
        });
        var WarehouseId = $scope.WarehouseId;
        $scope.getdborders = function (mob) {
            $http.get(serviceBase + 'api/Redispatch/auto1?mob=' + mob + '&WarehouseId=' + $scope.WarehouseId).then(function (results) {
                $scope.allOrders = results.data;
                $scope.callmethod();

            });
        }


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.allOrders,

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
        }

    }
})();




