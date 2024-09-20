

(function () {
    'use strict';

    angular
        .module('app')
        .controller('OrderProcessStatus', OrderProcessStatus);

    OrderProcessStatus.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', 'DeliveryService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function OrderProcessStatus($scope, OrderMasterService, OrderDetailsService, DeliveryService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        $scope.od = 0;

        $scope.searchdata = function (data) {


            var url = serviceBase + "api/SearchOrder/OrderProcessStatus?OrderId=" + data.orderId;
            $http.get(url).success(function (response) {

                $scope.customers = response;
                $scope.total_count = response.length;
                $scope.od = $scope.customers.OrderId;
                //$scope.orders = response;
            });
        }
        $scope.UpDataStatus = function (data) {


            var url = serviceBase + "api/OrderStatusUpdate?OrderId=" + $scope.od + "&status=" + data;
            $http.post(url).success(function (response) {

                $scope.customers = response;
            });
        }



    }
})();