

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AssignBulkcustomersController', AssignBulkcustomersController);

    AssignBulkcustomersController.$inject = ['$scope', "$http", "localStorageService", "peoplesService", "Service", "$modal", "ngTableParams", "$filter"];

    function AssignBulkcustomersController($scope, $http, localStorageService, peoplesService, Service, $modal, ngTableParams, $filter) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.Warehouse = [];
            $scope.selectedWhId = [];

            Service.get("Warehouse").then(function (results) {

                $scope.Warehouse = results.data;
            }, function (error) {
            })
            $scope.getWareHouseCustomer = function (data) {
                $scope.data = [];
                $scope.selectedWhId = data.WarehouseId;
                Service.get("AssignBulkcustomers/GetCustomer?Warehouseid=" + data.WarehouseId).then(function (results) {

                    $scope.data = results.data;
                }, function (error) {
                })
            }

            $scope.checkAll = function () {
                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.data, function (trade) {
                    trade.check = $scope.selectedAll;
                });
            };
            $scope.getselected = function (data) {

                $scope.assignedCusts = [];
                for (var i = 0; i < $scope.data.length; i++) {
                    if ($scope.data[i].check == true) {

                        var cs = {
                            Id: $scope.data[i].Id,
                            CustomerId: $scope.data[i].CustomerId,
                            CompanyId: $scope.data[i].CompanyId,
                            WarehouseId: $scope.selectedWhId
                        }

                        $scope.assignedCusts.push(cs);
                    }
                }
                if ($scope.assignedCusts.length > 0) {

                    $http.post(serviceBase + "api/AssignBulkcustomers/AssignCustomer", $scope.assignedCusts).then(function (results) {

                        alert("Added");
                        window.location.reload();
                    }, function (error) {
                        alert("Error Got Heere is ");
                    })
                } else {
                    alert("Please select checkBox");
                }
            }
        }
        
    }
})();
