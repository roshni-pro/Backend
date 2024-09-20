'use strict';
app.controller('WelcomeController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", function ($modal, $scope, $filter, $http, ngTableParams) {
    $scope.WellCome = "WelCome to ShopKirana";
    function reload() {
        localStorage.hasReloaded = true;
        window.location.reload();
    }
    if (!localStorage.hasReloaded || localStorage.hasReloaded == "false") {

        reload();
    }
}]);