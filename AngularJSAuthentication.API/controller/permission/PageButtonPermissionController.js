

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PageButtonPermissionController', PageButtonPermissionController);

    PageButtonPermissionController.$inject = ['$scope', 'WarehouseService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal'];

    function PageButtonPermissionController($scope, WarehouseService, CityService, StateService, $filter, $http, ngTableParams, $modal) {
       // 
        $scope.Pages = [];
        $scope.getPages = function () {
           // 

            var url = serviceBase + "api/PageMaster/GetAllPagesForDropDown";
            $http.get(url).success(function (results) {
                $scope.Pages = results;
                if ($scope.Pages == null) {
                    alert("Data Not Found");
                }
            });
        };

        $scope.getPages();

        $scope.Buttons = [];
        $scope.getButtons = function (id) {
           // 

            var url = serviceBase + "api/PageMaster/GetAllPageButton?pageMasterId=" + id;
            $http.get(url).success(function (results) {
                $scope.Buttons = results;
                if ($scope.Buttons == null) {
                    alert("Data Not Found");
                }
            });
        };

        $scope.SavePageButtonPermission = function (permission, id) {
          //  

            var url = serviceBase + "api/PageMaster/SavePageButton";
            var datatopost = [];
            for (var i = 0; i < permission.length; i++) {
                var post = {
                    ButtonMasterId: permission[i].Id,
                    PageMasterID: id,
                    IsActive: permission[i].IsChecked
                };
                datatopost.push(post);
            }

            $http.post(url, datatopost).success(function (results) {
                alert("Record Successfully Saved");
                //window.location.reload();
            });
        };

    }
})();