
(function () {
    'use strict';

    angular
        .module('app')
        .controller('indexController', indexController);

    indexController.$inject = ['$scope', '$location', 'authService', '$http'];

    function indexController($scope, $location, authService, $http) {
        // Any function returning a promise object can be used to load values asynchronously
        $scope.getLocation = function (val) {
            console.log("Change event is calling");
            console.log(val);
            return $http.get('http://maps.googleapis.com/maps/api/geocode/json', {
                params: {
                    address: val,
                    sensor: false
                }
            }).then(function (response) {
                return response.data.results.map(function (item) {
                    return item.formatted_address;
                });
            });
        };
        $scope.logOut = function () {
            authService.logOut();
            $location.path('/home');
        }

        $scope.authentication = authService.authentication;
    }
})();
