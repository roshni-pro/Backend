

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PNLController', PNLController);

    PNLController.$inject = ['$scope', "$filter", 'ClusterWiseService', '$http', 'ngAuthSettings', "ngTableParams", '$modal'];

    function PNLController($scope, $filter, ClusterWiseService, $http, ngAuthSettings, ngTableParams, $modal) {
        var url = serviceBase + "api/Warehouse";//pz
        function ReloadPage() {
            location.reload();
        }



        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

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
        //$scope.GetWarehouses = function (city) {
        //    var url = serviceBase + 'api/inventory/GetWarehouseCityWise?cityId=' + city;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //}; (warehouse standardization) 

           $scope.GetWarehouses = function (city) {
               var url = serviceBase + 'api/DeliveyMapping/GetWarehoueList/' + city;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };

        $scope.cities = [];
        //$scope.GetCity = function (region) {
        //    var url = serviceBase + 'api/inventory/GetCity?regionId=' + region;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.cities = response;
        //        });
        //}; --old api for city (warehouse standardization)

              $scope.GetCity = function (region) {
                  var url = serviceBase + 'api/DeliveyMapping/GetCityByRegion?RegionId=' + region;
            $http.get(url)
                .success(function (response) {
                    $scope.cities = response;
                });
        };



        //$scope.cities = [];
        //ClusterWiseService.getcitys().then(function (results) {
        //    $scope.cities = results.data;
        //}, function (error) { });

        //$scope.warehouse = [];
        //$scope.Warehouse = function (Cityid) {

        //    var url = serviceBase + "api/Warehouse/GetWarehouseCity?Cityid=" + Cityid;
        //    $http.get(url).success(function (results) {

        //        $scope.warehouse = results;
        //    });
        //};
        //$scope.getcluster = [];
        //$scope.Cluster = function (Warehouseid) {

        //    var url = serviceBase + "api/cluster/hubwise?WarehouseId=" + Warehouseid;
        //    $http.get(url).success(function (results) {

        //        $scope.getcluster = results;
        //    });
        //};


        $scope.GetPNLReport = function (wid, type) {

            var url = serviceBase + "api/ClusterWise/PNLReport?warehouseid=" + wid + "&Type=" + type;
            $http.get(url).success(function (data) {
                $scope.PNLReportData = data;
            });
        };
        //ClusterWiseService.getwarehouse().then(function (results) {
        //    $scope.warehouse = results.data;
        //}, function (error) { });


        //ClusterWiseService.getclusters().then(function (results) {
        //    $scope.getcluster = results.data;
        //}, function (error) { });


        var data = [];
        $scope.Get = function (ct) {

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
            var url1 = serviceBase + "api/ClusterWise/Get?clstid=" + ct + "&start=" + start + "&end=" + end;
            $http.get(url1).success(function (data) {
                $scope.ClusterWiseData = data;
            });
        };


        $scope.reset = function () {
            $scope.suppliersearch = 0;
        };

    }
})();

