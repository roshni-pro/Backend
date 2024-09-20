

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PDCACategoryController', PDCACategoryController);

    PDCACategoryController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function PDCACategoryController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {
        $scope.totalamt = 0;

        // function for get data  from BaseCategoryid wise
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
                });
        };

        // function for getCategory from WarehouseId, BaseCategoryId wise

        $scope.getCategory = function (WarehouseId, BaseCategoryId) {

            var url = serviceBase + 'api/PDCA/GetPDCACategory?warehouseid=' + WarehouseId + '&basecategoryid=' + BaseCategoryId;
            $http.get(url)
                .success(function (response) {
                    $scope.Category = response;
                    $scope.Calculation(response);
                });
        };

        // function for add data in PDCA category
        $scope.AddData = function (data, WarehouseId, BaseCategoryId) {

            if ($scope.totalamt == 100) {
                var dtpost = {
                    WarehouseReportCategorylist: data,
                    WarehouseId: WarehouseId,
                    BaseCategoryId: BaseCategoryId
                };

                var url = serviceBase + 'api/PDCA/AddPDCACategory';
                $http.post(url, dtpost)
                    .success(function (response) {
                        $scope.BaseCategory = response;
                        response = response.slice(1, -1);
                        alert(response);
                        window.location.reload();
                    });

            } else {
                alert('Total Percent Not Equal 100');
            }
        };
    }
})();