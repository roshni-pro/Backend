
(function () {
    'use strict';

    angular
        .module('app')
        .controller('headerController', headerController);

    headerController.$inject = ['$scope', '$location', '$timeout', 'authService'];

    function headerController($scope, $location, $timeout, authService) {
        console.log("user name in header");
        console.log(authService.authentication.userName);
        $scope.userName = authService.authentication.userName;
    }
})();
