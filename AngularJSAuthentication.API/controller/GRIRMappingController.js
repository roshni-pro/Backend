(function () {
    'use strict';
    angular
        .module('app')
        .controller('GRIRMappingController', GRIRMappingController);
    GRIRMappingController.$inject = ['$scope', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];
    function GRIRMappingController($scope, $filter, $http, ngAuthSettings, ngTableParams, $modal) {
        
        $scope.PurchaseOrder = {};
        $scope.SettledData = {};
        $scope.searchData = function (Poid) {            
            var url = serviceBase + "api/PurchaseOrder/GRIR?Poid=" + Poid;
            $http.get(url).success(function (results) {
                $scope.PurchaseOrder = results;
            });
            $scope.GetSettledData(Poid);
        };
        $scope.Settled = function (data) {            
            var url = serviceBase + "api/PurchaseOrder/SettledGRIR";
            $http.put(url, data).success(function (results) {                
                alert('Settled.');
                $scope.GetSettledData(data.GRIdsObj[0].PurchaseOrderId);
            }).error(function (results) {                
                alert(results.ErrorMessage);
            });
        };
        $scope.GetSettledData = function (Poid) {            
            var url = serviceBase + "api/PurchaseOrder/SettledData?Poid=" + Poid;
            $http.get(url).success(function (results) {                
                $scope.SettledData = results;
                $scope.Message = null;
            }).error(function (results) {                
                $scope.SettledData = null;
                $scope.Message = ' ('+results.ErrorMessage+")";
                alert(results.ErrorMessage);
            });
        };
        $scope.UnSettled = function (data) {            
            var url = serviceBase + "api/PurchaseOrder/UnSettledGRIR";
            $http.put(url, data).success(function (results) {                
                alert('UnSettled.');
                $scope.GetSettledData(data.GRIdsObj[0].PurchaseOrderId);
            }).error(function (results) {                
                alert(results.ErrorMessage);
            });
        };
    }
})();