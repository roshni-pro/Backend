

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DigitalSalesController', DigitalSalesController);

    DigitalSalesController.$inject = ['$scope', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];

    function DigitalSalesController($scope, $filter, $http, ngAuthSettings, ngTableParams, $modal) {
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
        $scope.GetWarehouses = function (warehouse) {
            var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };


        var data = [];
        $scope.GetDSData = function () {
            
            var ids = [];
            var selectedWarehouse = [];
            $scope.SelectedWarehouse = [];
            _.each($scope.examplemodel, function (o2) {
                ids.push(o2.id);
                for (var i = 0; i < $scope.warehouses.length; i++) {
                    if ($scope.warehouses[i].WarehouseId == o2.id) {
                        selectedWarehouse.push({ "Id": o2.id, "Name": $scope.warehouses[i].WarehouseName });
                    }
                }
            });

            var url = serviceBase + "api/ClusterWise/DigitalSalesAdvance";
            $http.post(url, ids).success(function (data) {
                $scope.DigitalSalesData = data;
            });
        };

        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouses;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };

    }
})();
