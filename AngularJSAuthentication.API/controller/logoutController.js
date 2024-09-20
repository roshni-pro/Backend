

(function () {
    'use strict';

    angular
        .module('app')
        .controller('logoutController', logoutController);

    logoutController.$inject = ['$scope', '$location', 'authService', 'ngAuthSettings'];

    function logoutController($scope, $location, $window, authService, ngAuthSettings, $localStorage) {


        localStorage.hasReloaded = false;
        localStorage.clear();

        $location.path('/pages/signin');





    }
})();
