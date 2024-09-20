

(function () {
    'use strict';

    angular
        .module('app')
        .controller('confirmEmailController', confirmEmailController);

    confirmEmailController.$inject = ['$scope', "$filter", "$http"];

    function confirmEmailController($scope, $filter, $http) {

        console.log("confirmEmailController Controller reached");


    }
})();