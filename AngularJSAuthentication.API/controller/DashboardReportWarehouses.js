

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DashboardReportWarehouses', DashboardReportWarehouses);

    DashboardReportWarehouses.$inject = ['$scope', "$http", "ngTableParams", "WarehouseService"];

    function DashboardReportWarehouses($scope, $http, ngTableParams, WarehouseServic) {

        $scope.getdata = function () {

            $http.get(serviceBase + "api/DashboardReport/GetByWarehouses").then(function (response) {
                $scope.custdata = response.data;
                $scope.total_ActiveCust = 0;
                $scope.total_OSale = 0;
                $scope.total_Ordered = 0;
                $scope.total_Odeliver = 0;
                $scope.total_AvgInv = 0;

                for (var i = 0; i < $scope.custdata.length; i++) {
                    $scope.total_ActiveCust = $scope.total_ActiveCust + $scope.custdata[i].ActiveCust;
                    $scope.total_OSale = $scope.total_OSale + $scope.custdata[i].OSale;
                    $scope.total_Ordered = $scope.total_Ordered + $scope.custdata[i].Ordered;
                    $scope.total_Odeliver = $scope.total_Odeliver + $scope.custdata[i].Odeliver;
                    $scope.total_AvgInv = $scope.total_AvgInv + $scope.custdata[i].AvgInv;

                }
            })
        };
        $scope.getdata();
    }
})();
