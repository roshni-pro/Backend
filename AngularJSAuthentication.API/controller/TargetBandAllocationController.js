

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TargetBandAllocationController', TargetBandAllocationController);

    TargetBandAllocationController.$inject = ['$scope', "$http", "ngTableParams", '$modal'];

    function TargetBandAllocationController($scope, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.getAll = function () {
           // 
            var url = serviceBase + 'api/TargetBandAllocation/Get';
            $http.get(url).success(function (results) {
                $scope.Alldata = results;
            });
        };
        $scope.getAll();
    }
})();
