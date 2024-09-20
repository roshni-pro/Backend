

    (function () {
        'use strict';

        angular
            .module('app')
            .controller('PopUpController', PopUpController);

        PopUpController.$inject = ["$scope", "$modalInstance", 'message'];

        function PopUpController($scope, $modalInstance, message) {
            $scope.ok = function () { $modalInstance.close(); };
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            if (message) {
                $scope.alrt = message;
            }
        }
    })();