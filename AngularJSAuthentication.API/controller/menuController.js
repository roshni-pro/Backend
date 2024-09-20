

(function () {
    'use strict';

    angular
        .module('app')
        .controller('menuController', menuController);

    menuController.$inject = ['$scope'];

    function menuController($scope) {

        console.log("Menu Page is loading...");



    }
})();