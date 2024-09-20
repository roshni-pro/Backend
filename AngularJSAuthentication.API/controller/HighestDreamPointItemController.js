

(function () {
    'use strict';

    angular
        .module('app')
        .controller('HighestDreamPointItemController', HighestDreamPointItemController);

    HighestDreamPointItemController.$inject = ['$scope', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function HighestDreamPointItemController($scope, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {

        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.wid = User.Warehouseid;
        $http.get(serviceBase + 'api/itemMaster/FindItemHighDPForWeb').then(function (results) {
            $scope.HighDPItem = results.data;
            $scope.checkAll = function () {

                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.HighDPItem, function (trade) {
                    trade.check = $scope.selectedAll;
                });
            };
            $scope.getselected = function (data1) {
                $scope.assignedCusts = []
                for (var i = 0; i < data1.length; i++) {
                    if (data1[i].check == true) {
                        var cs = {
                            ItemId: data1[i].ItemId,
                        }
                        $scope.assignedCusts.push(cs);
                    }
                }
                if ($scope.assignedCusts.length > 0) {
                    $http.post(serviceBase + "api/itemMaster/SelectedItem", $scope.assignedCusts).then(function (results) {
                        alert("Added");
                        window.location.reload();
                    }, function (error) {
                        alert("Error Got Heere is ");
                    })
                } else {
                    alert("Please select checkBox");
                }
            }
        }, function (error) {
        });
    }
})();










