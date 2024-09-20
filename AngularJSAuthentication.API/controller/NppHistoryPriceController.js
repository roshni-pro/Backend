

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NppHistoryPriceController', NppHistoryPriceController);

    NppHistoryPriceController.$inject = ['$scope', 'OrderMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function NppHistoryPriceController($scope, OrderMasterService, SubsubCategoryService, SubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal, FileUploader) {
        console.log(" NppHistoryPriceController Price Controller reached");
        $scope.currentPageStores = {};
        $scope.cities = [];
        OrderMasterService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });

        $scope.category = [];
        CategoryService.getcategorys().then(function (results) {
            console.log("get category here");
            console.log(results)
            $scope.category = results.data;
            console.log("result of category here");
            console.log($scope.category);
        }, function (error) {
        });

        $scope.subcategory = [];
        SubCategoryService.getsubcategorys().then(function (results) {
            console.log("get sub category here");
            $scope.subcategory = results.data;
            console.log("result of sub cat here");
            console.log($scope.subcategory);
        }, function (error) {
        });

        $scope.subsubcategory = [];
        SubsubCategoryService.getsubsubcats().then(function (results) {
            console.log("get sub sub category here");
            $scope.subsubcategory = results.data;
            console.log("result of sub sub cat here");
            console.log($scope.subsubcategory);
        }, function (error) {
        });
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });
        //....................................................//
        $scope.show = true;
        $scope.order = false;
        $scope.orders = [];
        $scope.dataforsearch = { Cityid: "", Categoryid: "", SubCategoryId: "", SubsubCategoryid: "" };
        $scope.Search = function (data) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            console.log("data for search here");
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.Categoryid = data.Categoryid;
            $scope.dataforsearch.SubCategoryId = data.SubCategoryId;
            $scope.dataforsearch.SubsubCategoryid = data.SubsubCategoryid;
            $scope.orders = [];
            $http.get(serviceBase + 'api/NppHistoryPrice?start=' + start + '&end=' + end + '&&' + 'Cityid=' + data.Cityid + '&&' + 'Categoryid=' + data.Categoryid + '&&' + 'SubCategoryId=' + data.SubCategoryId + '&&' + 'SubsubCategoryid=' + data.SubsubCategoryid).then(function (results) {
                console.log("filter details");
                console.log(results.data);

                $scope.orders = results.data;
                if (results.data == null) {
                    alert("Net Purchase Price history found");
                }
                $scope.callmethod();
            });
        }


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.orders;
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
