
(function () {
    'use strict';

    angular
        .module('app')
        .controller('TargetAllocatonBandsListController', TargetAllocatonBandsListController);

    TargetAllocatonBandsListController.$inject = ['$scope', "$http", '$modal', '$routeParams'];

    function TargetAllocatonBandsListController($scope, $http, $modal, $routeParams) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.wid = $routeParams.wid;
        $scope.CustomerTarget = [];
        $scope.ExportData = function (warehouse) {

            var url = serviceBase + 'api/TargetModule/GetCustomerTarget?WarehouseId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                  //  
                    $scope.CustomerTarget = response.GetCustomerTarget;
                });
        };

        //$scope.ExportData();

        $scope.getwarehouse = function () {
            
            var url = serviceBase + 'api/Warehouse/GetAllWarehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                });
        };

        $scope.getwarehouse();

    }
})();

