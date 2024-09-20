

(function () {
    'use strict';

    angular
        .module('app')
        .controller('homeController', homeController);

    homeController.$inject = ['$scope'];

    function homeController($scope) {
        $scope.pageClass = 'page-home';
        console.log("Home page is loading...");

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

    }
})();
