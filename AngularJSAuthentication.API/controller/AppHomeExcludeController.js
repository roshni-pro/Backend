

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ApphomeExcludeController', ApphomeExcludeController);

    ApphomeExcludeController.$inject = ['$scope', 'StateService', 'SubsubCategoryService', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal', "WarehouseService"];

    function ApphomeExcludeController($scope, StateService, SubsubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal, WarehouseService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log(" App Home Exclude Controller reached");
        $scope.currentPageStores = {};
        $scope.Sectiondata = {};

        $scope.appHome = function (Email) {

            var url = serviceBase + "api/Apphome/V2?lang=hi&wid=1";
            $http.get(url)
                .success(function (data) {

                    $scope.AppHomeData = data;
                    console.log(data);
                });
        };

        $scope.appHome();
    }
})();