
(function () {
    'use strict';

    angular
        .module('app')
        .controller('mappController', mappController);

    mappController.$inject = ['$scope', 'mappService', 'WarehouseCategoryService', '$http', '$filter', '$location'];

    function mappController($scope, mappService, WarehouseCategoryService, $http, $filter, $location) {
        console.log(" controller start loading");

        $scope.currentPageStores = {};

        $scope.mapp = function (t) {
            console.log("in mapping");
            //WarehouseCategoryService.getwhcategorys().then(function (results) {
            //                console.log(results.data);
            //}, function (error) {
            //});
            $scope.Whcategorys = [];

            mappService.getallmapp().then(function (results) {
                console.log(results.data);
                $scope.Whcategorys = results.data;

                $scope.callmethod();

            }, function (error) {

            });

        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Whcategorys;

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
        };

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


