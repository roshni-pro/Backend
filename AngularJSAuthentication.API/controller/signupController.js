

(function () {
    'use strict';

    angular
        .module('app')
        .controller('signupController', signupController);

    signupController.$inject = ['$scope', '$location', '$timeout', 'authService', '$http'];

    function signupController($scope, $location, $timeout, authService, $http) {
        $scope.pageClass = 'page-about';


        $scope.savedSuccessfully = false;
        $scope.message = "";

        $scope.registration = {
            userName: "",
            email: "",
            password: "",
            confirmPassword: ""
        };
        $scope.validateFreeEmail = function (email) {
            var reg = /^([\w-\.]+@(?!gmail.com)(?!yahoo.com)(?!hotmail.com)([\w-]+\.)+[\w-]{2,4})?$/
            if (reg.test(email)) {
                return true;
            }
            else {
                return false;
            }
        }
        $scope.signUp = function () {

            var errors = [];
            if (!$scope.validateFreeEmail($scope.registration.email)) {
                errors.push("Not a valid company email address");
                $scope.message = "Failed to register user due to:" + errors.join(' ');
            } else {
                authService.saveRegistration($scope.registration).then(function (response) {
                    console.log(response);
                    $scope.savedSuccessfully = true;
                    $scope.message = "User has been registered successfully, you will be redicted to login page in 2 seconds.";
                    startTimer();

                },
                    function (response) {
                        console.log(response);
                        console.log(response.data.ModelState);
                        for (var key in response.data.ModelState) {
                            console.log(key);
                            for (var i = 0; i < response.data.ModelState[key].length; i++) {
                                console.log(response.data.ModelState[key]);
                                errors.push(response.data.ModelState[key][i]);
                            }
                        }
                        $scope.message = "Failed to register user due to:" + errors.join(' ');
                    });
            }
        };

        var startTimer = function () {
            var timer = $timeout(function () {
                $timeout.cancel(timer);
                $location.path('/pages/signin');
            }, 2000);
        }

    }
})();
