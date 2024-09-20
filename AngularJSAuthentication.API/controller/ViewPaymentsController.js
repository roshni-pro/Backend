
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ViewPaymentsController', ViewPaymentsController);

    ViewPaymentsController.$inject = ["$scope", '$http', 'ngAuthSettings', "$filter", '$routeParams'];

    function ViewPaymentsController($scope, $http, ngAuthSettings, $filter, $routeParams) {
        //
        $scope.SupplierData = JSON.parse(localStorage.getItem('sample_data'));
        $scope.SupplierId = $routeParams.SupplierId;
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        function GetPaymentData() {

            var url = serviceBase + "api/Suppliers/supplierPaymentData?SupplierId=" + $scope.SupplierId;
            $http.get(url).success(function (response) {
                $scope.SupplierPaymentData = response;
                $scope.callmethod();
                //localStorage.removeItem('sample_data');
                console.log($scope.SupplierPaymentData);
            })
                .error(function (data) {
                });
        }
        GetPaymentData();

        $scope.SearchSupplierPaymentData = function () {
            //
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
                alert('Date is Required');
                return null;
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            start = f.val();
            end = g.val();
            var url = serviceBase + "api/Suppliers/SearchPayment?&Start=" + start + "&End=" + end + "&SupplierId=" + $scope.SupplierId;
            $http.get(url).success(function (results) {
                $scope.SupplierPaymentData = results;
                $scope.callmethod();
            })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.SearchRefrence = function (saveData) {
            var url = serviceBase + "api/Suppliers/SearchPaymentRefrence?&RefrenceNumber=" + saveData.RefrenceNumber + "&SupplierId=" + $scope.SupplierId;
            $http.get(url).success(function (results) {
                $scope.SupplierPaymentData = results;
                $scope.callmethod();
            })
                .error(function (data) {
                    console.log(data);
                })
        };

        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.SupplierPaymentData,
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

                $scope.numPerPageOpt = [20, 50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage);
                })
        };
    }
})();
