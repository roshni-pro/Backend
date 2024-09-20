

(function () {
    'use strict';

    angular
        .module('app')
        .controller('trackuser', trackuser);

    trackuser.$inject = ['$scope', "$filter", "$http", "ngTableParams"];

    function trackuser($scope, $filter, $http, ngTableParams) {

        $scope.searchdata = function (data) {

            var url = serviceBase + "api/trackuser?pid=" + data;
            $http.get(url).success(function (response) {

                $scope.User = response;
                var vm = this;
                $scope.callmethod();



            });

        }

        $scope.searchType = function (data) {

            var url = serviceBase + "api/trackuser/Searchtype?type=" + data;
            $http.get(url).success(function (response) {

                $scope.User = response;
                $scope.callmethod();
            });

        }

        $scope.searchpage = function (data) {

            var url = serviceBase + "api/trackuser/searchpage?type=" + data;
            $http.get(url).success(function (response) {

                $scope.User = response;
                $scope.callmethod();
            });

        }
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });

        $scope.Search = function () {
            
            $scope.dataforsearch = { datefrom: "", dateto: "" };
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            $scope.dataforsearch.datefrom = f.val();
            $scope.dataforsearch.dateto = g.val();
            var url = serviceBase + "api/trackuser/SearchByDate?aaa=" + $scope.dataforsearch.datefrom + "&bbb=" + $scope.dataforsearch.dateto;
            $http.get(url).success(function (response) {

                $scope.User = response;

            });

        }
        $scope.callmethod = function () {

            var init;

            return $scope.stores = $scope.User,

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
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {

                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };

    }
})();
