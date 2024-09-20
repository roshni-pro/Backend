
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ordersController', ordersController);

    ordersController.$inject = ['$scope', 'ordersService'];

    function ordersController($scope, ordersService) {

        $scope.orders = [];

        ordersService.getOrders().then(function (results) {

            $scope.orders = results.data;

        }, function (error) {
            //alert(error.data.message);
        });

    }
})();