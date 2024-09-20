

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BudgetAllocationListController', BudgetAllocationListController);

    BudgetAllocationListController.$inject = ['$scope', 'WarehouseService', "$http", '$modal', '$routeParams', "localStorageService"];

    function BudgetAllocationListController($scope, WarehouseService, $http, $modal, $routeParams, localStorageService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.data = {};
        $scope.month = 0;
        $scope.currentPage = {};
        //if (localStorage.getItem('apidata') != null) {
        //    
        //    $scope.data = JSON.parse(localStorage.getItem('apidata'));
        //    $scope.month = JSON.parse(localStorage.getItem('mon'));
        //}

        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {
           // 
            WarehouseService.getwarehousewokpp().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        $scope.getWarehosues();

        $scope.band = new Array("band 1", "band 2", "band 3", "band 4");

        $scope.level = new Array("level_0", "level_1", "level_2", "level_3", "level_4", "level_5");



        $scope.CustomerTarget = [];
        $scope.ViewData = function (WarehouseId, level) {
           // 

            var url = serviceBase + 'api/TargetModule/GetMarkettingBudget?WarehouseId=' + WarehouseId + '&Level=' + level;
            $http.get(url)
                .success(function (response) {
                   // 
                    $scope.CustomerTarget = response.GetExportData;
                    //$scope.callmethod();
                });
        };

        //$scope.callmethod = function () {

        //    var init;
        //    return $scope.stores = $scope.CustomerTarget,

        //        $scope.searchKeywords = "",
        //        $scope.filteredStores = [],
        //        $scope.row = "",

        //        $scope.select = function (page) {

        //            var end, start; console.log("select"); console.log($scope.stores);
        //            return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        //        },

        //        $scope.onFilterChange = function () {
        //            console.log("onFilterChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
        //        },

        //        $scope.onNumPerPageChange = function () {
        //            console.log("onNumPerPageChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1
        //        },

        //        $scope.onOrderChange = function () {
        //            console.log("onOrderChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1
        //        },

        //        $scope.search = function () {
        //            console.log("search");
        //            console.log($scope.stores);
        //            console.log($scope.searchKeywords);
        //            return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
        //        },

        //        $scope.order = function (rowName) {
        //            console.log("order"); console.log($scope.stores);
        //            return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
        //        },

        //        $scope.numPerPageOpt = [3, 5, 10, 20],
        //        $scope.numPerPage = $scope.numPerPageOpt[2],
        //        $scope.currentPage = 1,
        //        $scope.currentPageStores = [],
        //        (init = function () {
        //            return $scope.search(), $scope.select($scope.currentPage)
        //        })
        //            ()
        //}
        //$scope.ExportData();

        $scope.Export = function (data) {
            // 
            $scope.export = data;
            alasql('SELECT * INTO XLSX("MarkettingBudget.xlsx",{headers:true}) FROM ?', [$scope.export]);
        };
    }
})();