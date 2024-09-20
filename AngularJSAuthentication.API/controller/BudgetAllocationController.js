

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BudgetAllocationController', BudgetAllocationController);

    BudgetAllocationController.$inject = ['$scope', 'WarehouseService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', "localStorageService"];

    function BudgetAllocationController($scope, WarehouseService, CityService, StateService, $filter, $http, ngTableParams, $modal, localStorageService) {
        $scope.city = {};

        $scope.RunVirtualEnviornment = function () {
            var url = serviceBase + 'api/TargetModule/ExecuteVirtualEnviornment';
            $http.get(url)
                .success(function (response) {
                    
                    alert(response);
                });
        };

        $scope.KillVirtualEnviornment = function () {
            var url = serviceBase + 'api/TargetModule/KillVirtualEnviornment';
            $http.get(url)
                .success(function (response) {
                    
                    alert(response);
                });
        };
        //$scope.ExportData();
        // function for get Warehosues from warehouse service
        $scope.getCities = function () {
            CityService.getcitys().then(function (results) {
                $scope.city = results.data;
            }, function (error) {
            });
        };
        $scope.getCities();

        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function (data) {
            WarehouseService.warehousecitybased(data).then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        //$scope.getWarehosues();

        $scope.getYear = function () {
            
            var cy = new Date();
            var y = cy.getFullYear();
            var ty = y + 11;
            var yearlist = [];
            for (var i = 2018; i < ty; i++) {
                y = i + 1;
                yearlist.push(y);
            }
            $scope.year = yearlist;
        };
        $scope.getYear();


        $scope.month = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");

        $scope.band = new Array("band 1", "band 2", "band 3", "band 4");

        $scope.level = new Array("level_0", "level_1", "level_2", "level_3", "level_4", "level_5");

        $scope.apidata = {};
        $scope.mon = 0;
        $scope.GetBAData = function (data) {
            if (data.month == null) {
                alert('Please Fill Month ');
            }
            else if (data.year == null) {
                alert('Please Select Year ');
            }
            else if (data.band == null) {
                alert('Please Enter Band ');
            }
            else if (data.amount == null) {
                alert('Please Enter Amount ');
            }
            else if (data.lamount == null) {
                alert('Please Enter L0 Amount ');
            }
            else if (data.levels == null) {
                alert('Please Select Level ');
            }
            else if (data.Cityid == null) {
                alert('Please Select City ');
            }
            else if (data.WarehouseId == null) {
                alert('Please Select Warehouse ');
            }
            else
            {
                $scope.apidata = data;
                var monthid = $scope.month.indexOf(data.month) + 1;
                $scope.mon = monthid;
                monthid = monthid - 1;
                var levelid = $scope.level.indexOf(data.levels);
                var bandid = $scope.band.indexOf(data.band) + 1;
                //var url = 'https://127.0.0.1:5000/allocation?month=' + monthid + '&year=' + data.year + '&band=' + bandid + '&amount=' + data.amount + '&l0amount=' + data.lamount + '&levels=' + levelid + '&cityid=' + data.Cityid + '&warehouseid=' + data.WarehouseId;

                var url = 'api/TargetModule/GetMLData?month=' + monthid + '&year=' + data.year + '&band=' + bandid + '&amount=' + data.amount + '&l0amount=' + data.lamount + '&levels=' + levelid + '&cityid=' + data.Cityid + '&warehouseid=' + data.WarehouseId;
                $http.get(url).success(function (results) {

                    $scope.Allocation = results;
                    if ($scope.Allocation == null) {
                        alert("Data Not Found");
                    }
                }, function (error) {
                    alert("dudget error: " + error);
                });
            }
        };

        $scope.AddData = function (alldata, itemdata)
        {
            

            var url = serviceBase + "api/TargetModule/SaveBudgetAllocation";

            var datatopost = [];

            for (var i = 0; i < alldata.length; i++) {
                var post = {
                    WarehouseId: itemdata.WarehouseId,
                    SkCode: alldata[i].SkCode,
                    levels: alldata[i].levels,
                    status: alldata[i].status,
                    allocation: alldata[i].allocation
                };
                datatopost.push(post);
            }

            $http.post(url, datatopost).success(function (results) {
                
                //localStorage.setItem("apidata", JSON.stringify($scope.apidata));
                //localStorage.setItem("mon", JSON.stringify($scope.mon));
                alert(results.Message);
                //window.location = "#/BudgetAllocatonList";
                //window.location.reload();
            });
        };
    }
})();

