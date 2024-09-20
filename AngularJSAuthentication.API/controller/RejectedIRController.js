

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RejectedIRController', RejectedIRController);

    RejectedIRController.$inject = ['$scope', 'WarehouseService', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal'];

    function RejectedIRController($scope, WarehouseService, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {

        

        var currentWarehouse = localStorage.getItem('currentWarehouse');
        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse)
        }

        $scope.warehouse = [];
        //$scope.getWarehosues = function () {
            
        //    WarehouseService.getwarehouse().then(function (results) {
        //        $scope.warehouse = results.data;
        //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
        //        $scope.WarehouseId = $scope.Warehouseid;
        //        $scope.Warehousetemp = angular.copy(results);
        //        $scope.GetIRDetail($scope.WarehouseId);
        //    }, function (error) {
        //    })
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                    $scope.WarehouseId = $scope.Warehouseid;
                    $scope.Warehousetemp = angular.copy(data);
                    $scope.GetIRDetail($scope.WarehouseId);
                });

        };
       /* $scope.wrshse();*/

        $(function () {

            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });
        $(function () {
            $('input[name="daterangedata"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });
       /* $scope.getWarehosues();*/
        $scope.wrshse();
        $scope.IrData = {};
        $scope.dataforsearch = { WarehouseId: "", datefrom: "", dateto: "" };
        $scope.GetIRDetail = function (WarehouseId) {
            
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);
            $scope.dataforsearch.WarehouseId = WarehouseId;
            var url = serviceBase + 'api/IR/getRejetcedIR?WarehouseId=' + WarehouseId;
            $http.get(url).success(function (data) {
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId == id;
                });
                $scope.IrData = data;
            });
        };

        $scope.openIr = function (data) {

            console.log("open fn");
            SearchPOService.OpenRejectedIr(data).then(function (results) {
                console.log("master save fn");
                console.log(results);
            }, function (error) {
            });
        };   

        $scope.SearchRejectedIRData = function (data) {
            var url = serviceBase + "api/IR/SearchRejectedIR?PurchaseOrderId=" + data;
            $http.get(url).success(function (results) {

                $scope.IrData = results;

            })
                .error(function (data) {
                    console.log(data);
                })
        };  
        $scope.openIR = function (data) {
            
            window.location = "#/RejectedIRDetail/" + data.PurchaseOrderId + "/" + data.Id;
        }


    }

})();
