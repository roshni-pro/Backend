

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DialValuePointController', DialValuePointController);

    DialValuePointController.$inject = ['$scope', "$http", "localStorageService", "peoplesService", "Service", "$modal", "ngTableParams", "$filter"];

    function DialValuePointController($scope, $http, localStorageService, peoplesService, Service, $modal, ngTableParams, $filter) {

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
        $scope.getDialPoint = function () {
            $scope.DialPointData = [];
            var url = serviceBase + "api/DialPoint/value";
            $http.get(url).success(function (results) {
                $scope.DialPointData = results;

                $scope.callmethod();
            })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.getDialPoint();
        $scope.SearchDialPoint = function () {
            $scope.DialPointData = [];
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();
            var end = g.val();

            var url = serviceBase + "api/DialPoint/value/Search?start=" + start + "&end=" + end + "&skcode=" + $scope.skcode;
            $http.get(url).success(function (results) {
                $scope.DialPointData = results;

                $scope.callmethod();
            })
                .error(function (data) {
                    console.log(data);
                })
        };

      

        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.DialPointData;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";
               
            $scope.numPerPageOpt = [50, 100, 500, 100];
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
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData = function () {
            alasql('SELECT ShopName,Skcode,point,OrderAmount,OrderId, CreatedDate INTO XLSX("DialValuePoint.xlsx",{headers:true}) FROM ?', [$scope.DialPointData]);
        };
    }
})();

