

(function () {
    'use strict';

    angular
        .module('app')
        .controller('refreshController', refreshController);

    refreshController.$inject = ['$scope', '$location', 'authService'];

    function refreshController($scope, $location, authService) {

        $scope.authentication = authService.authentication;
        $scope.tokenRefreshed = false;
        $scope.tokenResponse = null;

        $scope.refreshToken = function () {

            authService.refreshToken().then(function (response) {
                $scope.tokenRefreshed = true;
                $scope.tokenResponse = response;
            },
                function (err) {
                    $location.path('/login');
                });
        };

    }
})();
