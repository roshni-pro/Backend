
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PODashboardMainController', PODashboardMainController);

    PODashboardMainController.$inject = ['$scope', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];

    function PODashboardMainController($scope, $filter, $http, ngAuthSettings, ngTableParams, $modal) {

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });

        //$scope.zones = [];
        //$scope.GetZones = function () {
        //    var url = serviceBase + 'api/inventory/GetZone';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.zones = response;
        //        });
        //};
        //$scope.GetZones();

        //$scope.regions = [];
        //$scope.GetRegions = function (zone) {
        //    var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.regions = response;
        //        });
        //};

        //$scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //};

        $scope.warehouse = [];
        //$scope.Warehouse = function () {

        //    var url = serviceBase + "api/Warehouse";
        //    $http.get(url).success(function (results) {

        //        $scope.warehouse = results;
        //    });
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                });

        };
        $scope.wrshse();
        //$scope.Warehouse();

        $scope.suppliers = [];
        $scope.GetSupplier = function (WarehouseId) {

            var url = serviceBase + "api/Suppliers/POSupplier?wareHouseId=" + WarehouseId;
            $http.get(url).success(function (results) {

                $scope.suppliers = results;
            });
        };


        $scope.Dashboarddata = [];
        $scope.Get = function (data) {
           
            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var url = "";

            if (!$('#dat').val()) {
                end = '';
                start = '';
                alert("Select Start and End Date");
                return;
            }
            else {
                start = f.val();
                end = g.val();
            }
            url = serviceBase + "api/PurchaseOrderMaster/podashboard?start=" + start + "&end=" + end + "&wid=" + data.WarehouseId;
                //+ "&SupplierId=" + data.SuppplierId;
            $http.get(url).success(function (data) {
                $scope.Dashboarddata = data;
            });
        };




    }
})();





