

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TargetAllocationBandsController', TargetAllocationBandsController);

    TargetAllocationBandsController.$inject = ['$scope', "$http", "ngTableParams", 'FileUploader', '$modal', '$log'];

    function TargetAllocationBandsController($scope, $http, ngTableParams, $modal) {
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


