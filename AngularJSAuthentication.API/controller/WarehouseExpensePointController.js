

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WarehouseExpensePointController', WarehouseExpensePointController);

    WarehouseExpensePointController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal'];

    function WarehouseExpensePointController($scope, $filter, $http, ngTableParams, $modal) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        // new pagination 
        //$scope.pageno = 1; //initialize page no to 1
        //$scope.total_count = 0;

        //$scope.itemsPerPage = 30;  //this could be a dynamic value from a drop down

        //$scope.numPerPageOpt = [30, 60, 90, 100];  //dropdown options for no. of Items per page

        //$scope.onNumPerPageChange = function () {
        //    $scope.itemsPerPage = $scope.selected;

        //}
        //$scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

        //$scope.$on('$viewContentLoaded', function () {
        //    // $scope.WalletHistory($scope.pageno);
        //});

        //$scope.CustomerId = 0;
        //$scope.WalletExpenseHistory = function (data) {

        //    $scope.CustomerId = data.CustomerId;
        //    $scope.WalletHistory($scope.pageno);
        //}

        $scope.warehouseWalletExpenseHistory = function () {

            $scope.OldStockData = [];
            var url = serviceBase + "api/WarehouseExpensePoint";
            $http.get(url).success(function (result) {

                $scope.WWExpenseData = result;

            })
                .error(function (data) {
                })
        }
        $scope.warehouseWalletExpenseHistory();
    }
})();


