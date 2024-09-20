

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ClusterWiseController', ClusterWiseController);

    ClusterWiseController.$inject = ['$scope', "$filter", 'ClusterWiseService', '$http', 'ngAuthSettings', "ngTableParams", '$modal', 'ClusterService'];

    function ClusterWiseController($scope, $filter, ClusterWiseService, $http, ngAuthSettings, ngTableParams, $modal, ClusterService) {
        
        $scope.dataPeopleHistrory;

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

        $scope.cities = [];
        $scope.GetCity = function (region) {
            var url = serviceBase + 'api/inventory/GetCity?regionId=' + region;
            $http.get(url)
                .success(function (response) {
                    $scope.cities = response;
                });
        };
        //Get Selecte rout
        var path = window.location.hash.substring(2);
        //Get list
        var url = serviceBase + "api/Menus/GetButtons?submenu=" + path;
        $http.get(url).success(function (response) {
            $scope.dataPeopleHistrory = response;
            console.log($scope.dataPeopleHistrory);
        })
        $scope.PermissionSet = JSON.parse(localStorage.getItem('PermissionSet'));

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
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });




       
        // call city based  clusters
        $scope.Clusters = [];

        $scope.Clustercitybased = function (CityId) {
           // 
            $scope.ClusterId = "";
            ClusterService.getCitycluster(CityId).then(function (results) {
                $scope.getcluster = results.data;
            }, function (error) {
            });
        };


        //$scope.cities = [];
        //ClusterWiseService.getcitys().then(function (results) {

        //    $scope.cities = results.data;
        //}, function (error) { });

        $scope.warehouse = [];
        $scope.Warehouse = function (clstid) {
            $scope.warehouse = [];
            $scope.WarehouseId = "";
            if (clstid) {
                var url = serviceBase + "api/ClusterWise/GetWarehouse?clstid=" + clstid;
                $http.get(url).success(function (results) {

                    $scope.warehouse = results;
                });
            }
        };
        $scope.vm = {};
        $scope.vm.agents = [];
        $scope.Agents = function (wid) {
           // 
            if (wid) {
                var url = serviceBase + "api/ClusterWise/GetAgentsHubwise?clstid=" + wid;
                $http.get(url).success(function (results) {

                    $scope.vm.agents = results;
                });
            }
            else
                $scope.vm.agents = [];
        };


        $scope.getcluster = [];
        $scope.Cluster = function (Warehouseid) {

            var url = serviceBase + "api/cluster/hubwise?WarehouseId=" + Warehouseid;
            $http.get(url).success(function (results) {

                $scope.getcluster = results;
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
                alert("Select Start and End Date")
                return;
            }
            else {
                start = f.val();
                end = g.val();
            }
            url = serviceBase + "api/ClusterWise/Get?clstid=" + ct + "&start=" + start + "&end=" + end;
            $http.get(url).success(function (data) {
                $scope.ClusterWiseData = data;
            });
        };


        $scope.reset = function () {
            $scope.suppliersearch = 0;
        };
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }

        $scope.ExportList = function () {
            alasql('SELECT INTO XLSX("ClusterReport.xlsx",{headers:true}) FROM ?', [$scope.ClusterWiseData]);
            //});
        };
    }
})();


