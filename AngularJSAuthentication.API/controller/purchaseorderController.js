

(function () {
    'use strict';

    angular
        .module('app')
        .controller('purchaseorderController', purchaseorderController);

    purchaseorderController.$inject = ['$scope', 'PurchaseOrderService', "$filter", '$http', 'ngAuthSettings', "ngTableParams"];

    function purchaseorderController($scope, PurchaseOrderService, $filter, $http, ngAuthSettings, ngTableParams) {

        console.log("purchaseorderController start loading OrderDetailsService");

        $scope.currentPageStores = {};

        $scope.purchaseorder = [];

        PurchaseOrderService.getpurchaseorders().then(function (results) {
            console.log("purchase order is calling");
            $scope.purchaseorder = results.data;
            console.log($scope.purchaseorder);
            console.log(results.data);
            $scope.callmethod();
        }, function (error) {
        });


        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                //locale: {
                format: 'MM/DD/YYYY h:mm A'
                //}
            });

        });

        //$scope.dataforsearch = { Status: "", datefrom: "", dateto: "" };
        //$scope.Search = function (data) {
        //    var f = $('input[name=daterangepicker_start]');
        //    var g = $('input[name=daterangepicker_end]');
        //    console.log("sumit");
        //    console.log(f);
        //    console.log(g);
        //    //alert(f.val());
        //    //alert(g.val());  
        //    $scope.dataforsearch.Status = data.Status;
        //    $scope.dataforsearch.datefrom = f.val();
        //    $scope.dataforsearch.dateto = g.val();
        //    $scope.orders = [];
        //    console.log("search is calling...");
        //    console.log(data);
        //    PurchaseOrderService.getfilteredpo($scope.dataforsearch).then(function (results) {
        //        console.log("filter details");
        //        console.log(results.data);
        //        $scope.orders = results.data;
        //        $scope.callmethod();
        //    });
        //}

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.purchaseorder;

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


