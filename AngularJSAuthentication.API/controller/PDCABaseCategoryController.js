
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PDCABaseCategoryController', PDCABaseCategoryController);

    PDCABaseCategoryController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function PDCABaseCategoryController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {
        var url = serviceBase + "api/PDCA";
        $scope.totalamt = 0;

        // function for get PDCA base category data according to BaseCategoryid
        $scope.selectedItemChanged = function (data) {

            var url = serviceBase + 'api/PDCA/GetWarehouseReport?basecategoryid=' + data.BaseCategoryid;
            $http.get(url)
                .success(function (response) {

                    $scope.Sectiondata = response;
                });
        };

        // function for calculate percentage total
        $scope.Calculation = function (data) {
            $scope.totalamt = 0;
            var total = 0;
            for (var i = 0; i < data.length; i++) {


                total = total + parseFloat(data[i].Percentage);

            }
            $scope.totalamt = total;
        };

        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {

            WarehouseService.getwarehousewokpp().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        $scope.getWarehosues();

        // function for fill base category combo hubwise(warehouse)
        $scope.getBaseCategory = function (WarehouseId) {
            var url = serviceBase + 'api/PDCA/GetBaseCategory?warehouseid=' + WarehouseId;
            $http.get(url)
                .success(function (response) {

                    $scope.BaseCategory = response;
                    $scope.Calculation(response);

                });
        };

        // function for add data in PDCA Base category
        $scope.AddData = function (data, WarehouseId) {

            if ($scope.totalamt == 100) {
                var dtpost = {
                    WarehouseReportlist: data,
                    WarehouseId: WarehouseId
                };

                var url = serviceBase + 'api/PDCA/AddPDCABaseCategory';
                $http.post(url, dtpost)
                    .success(function (response) {
                        $scope.BaseCategory = response;
                        window.location.reload();
                        alert(response);
                    });

            }
            else {
                alert('Total Percent Not Equal 100');
            }
        };
    }
})();