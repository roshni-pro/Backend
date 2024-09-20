

(function () {
    'use strict';

    angular
        .module('app')
        .controller('GrowthModuleLoginController', GrowthModuleLoginController);

    GrowthModuleLoginController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function GrowthModuleLoginController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {
        $scope.warehouse = {};
        $scope.warehouseCategoryDetail = [];
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {
            WarehouseService.getwarehousewokpp().then(function (results) {
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;
                    }
                    $scope.warehouse = results.data;
                }
            }, function (error) {
            });
        };
        $scope.getWarehosues();



        // function for get People 
        $scope.getPeoples = function () {
            var url = serviceBase + 'api/GrowthModule/GetPeople';
            $http.get(url)
                .success(function (response) {
                    $scope.Peoples = response.peopleslist;
                });
        };
        $scope.getPeoples();

        // function for get People 
        $scope.loginpermission = function () {
            var url = serviceBase + 'api/GrowthModule/GetGMLoginpermission';
            $http.get(url)
                .success(function (response) {
                    $scope.permission = response;
                });
        };
        $scope.loginpermission();

        $scope.DeleteLoginpermission = function (id) {
            var url = serviceBase + 'api/GrowthModule/DeleteLoginpermission?id=' + id;
            $http.get(url)
                .success(function (response) {
                    alert("Permission remove successfully.");
                    window.location.reload();
                });
        };


        $scope.SelectedWarehouse = [];
        // function for get PDCA details data according to date range
        $scope.AddData = function (a) {

            var temp = [];
            temp = angular.fromJson(a);
            var ids = [];
            var selectedWarehouse = [];
            $scope.SelectedWarehouse = [];
            _.each($scope.examplemodel, function (o2) {
                ids.push(o2.id);
            });

            var url = serviceBase + 'api/GrowthModule/AddGMLoginData';
            var datatopost =
            {
                PeopleId: temp.PeopleID,
                PeopleName: temp.DisplayName,
                WarehouseIds: ids
            };
            $http.post(url, datatopost)
                .success(function (response) {
                    window.location.reload();
                    alert(response.Message);
                });
        };


        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
    }
})();
