

(function () {
    'use strict';

    angular
        .module('app')
        .controller('IRSupplierCtrl', IRSupplierCtrl);

    IRSupplierCtrl.$inject = ['$scope', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', 'supplierService'];

    function IRSupplierCtrl($scope, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, supplierService) {
        $scope.isShow = false;
        supplierService.getsuppliers().then(function (results) {
            $scope.supplier = results.data;
        });
        $scope.supplietIRGR = function (SupplierId) {
            $scope.IR_Total = 0;
            $scope.GR_Total = 0;
            $http.get(serviceBase + 'api/IR/getIRSupplier?id=' + SupplierId).success(function (data) {
                $scope.data = [];
                $scope.IR_Total = 0;
                $scope.GR_Total = 0;
                if (data.length > 0) {
                    $scope.isShow = true;
                    _.map(data, function (obj) {
                        $scope.IR_Total += obj.IRTotal;
                        $scope.GR_Total += obj.GRTotal;
                    });
                    $scope.data = data;
                }
                else {
                    $scope.isShow = false;
                    $scope.data = null;
                    alert("Data not present");
                }
                $timeout(function () {
                  
                }, 3000)

            }).error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            })
        }
    }
})();