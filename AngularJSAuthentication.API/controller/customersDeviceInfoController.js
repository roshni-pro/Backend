//tejas for agent device info view 24-05-19


(function () {
    'use strict';

    angular
        .module('app')
        .controller('customersDeviceInfoController', customersDeviceInfoController);

    customersDeviceInfoController.$inject = ['$scope', 'CityService', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log'];

    function customersDeviceInfoController($scope, CityService, $filter, $http, ngTableParams, FileUploader, $modal, $log) {

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
        $scope.GetWarehouses = function (city) {
            var url = serviceBase + 'api/inventory/GetWarehouseCityWise?cityId=' + city;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };

        $scope.cities = [];
        $scope.GetCity = function (region) {
            var url = serviceBase + 'api/inventory/GetCity?regionId=' + region;
            $http.get(url)
                .success(function (response) {
                    $scope.cities = response;
                });
        };

        $scope.CustomerDeviceHistroy = function (CustomerId) {


            $scope.datacustomerDeviceHistrory = [];
            var url = serviceBase + "api/Customers/customerDevicehistory?CustomerId=" + CustomerId;
            $http.get(url).success(function (response) {
                $scope.datacustomerDeviceHistrory = response;
                console.log($scope.datacustomerDeviceHistrory);
                $scope.AddTrack("View(History)", "CustomerId:", data.CustomerId);


            });
        };

        $scope.LineitemTotal = function (Cityid) {


            var url = serviceBase + 'api/Customers/GetCustomersDeviceInfo?Cityid=' + Cityid;

            $http.get(url)
                .success(function (response) {

                    $scope.TotalLineitem = response;
                    $scope.callmethod();
                });

        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.TotalLineitem;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               
            $scope.numPerPageOpt = [10, 20, 30, 100];
            $scope.numPerPage = $scope.numPerPageOpt[2];
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
