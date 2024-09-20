

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BrandwisepramotionController', BrandwisepramotionController);

    BrandwisepramotionController.$inject = ['$scope', 'ItemPramotionService', 'WarehouseCategoryService', 'SubCategoryService', 'StateService', 'demandservice', "$filter", "$http", "ngTableParams", '$modal'];

    function BrandwisepramotionController($scope, ItemPramotionService, WarehouseCategoryService, SubCategoryService, StateService, demandservice, $filter, $http, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.states = [];
            StateService.getstates().then(function (results) {
                console.log("mine");
                console.log(results.data);
                $scope.states = results.data;
            }, function (error) {
            });
            $scope.cities = [];
            demandservice.getcitys().then(function (results) {
                $scope.cities = results.data;
            }, function (error) {
            });
            $scope.warehouse = [];
            demandservice.getwarehouse().then(function (results) {
                console.log(results.data);
                console.log("data");
                $scope.warehouse = results.data;
            }, function (error) {
            });
            $scope.UpdateItemMaster = function (data) {
                var url = serviceBase + "api/Brandwisepramotion";
                var dataToPost = data;
                $http.put(url, dataToPost)
                    .success(function (data) {
                        console.log("success");
                        console.log(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                    })
            };
            $scope.warehouseCategory = [];
            $scope.subsubcategory = [];
            $scope.showsubcategory = function () {
                if ($scope.branddata.WarehouseId > 0) {
                    var url = serviceBase + "api/Brandwisepramotion?recordtype=warehouse&&warehouseid=" + $scope.branddata.WarehouseId;
                    $http.get(url)
                        .success(function (data) {
                            console.log("success");
                            console.log(data);
                            $scope.subsubcategory = data;
                        })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);

                        })
                }
                else {
                    $scope.tableshow = false;
                }

            }
            $scope.showBrandWise = function () {
                $scope.subcategory = [];
                if ($scope.branddata.SubsubCategoryid > 0) {
                    $scope.tableshow = true;
                    var url = serviceBase + "api/Brandwisepramotion?recordtype=warehouse&&SubSubCategoryId=" + $scope.branddata.SubsubCategoryid + "&&WarehouseId=" + $scope.branddata.WarehouseId;
                    $http.get(url)
                        .success(function (data) {
                            console.log("success");
                            console.log(data);

                            $scope.subcategory = data;
                        })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);
                            // return $scope.showInfoOnSubmit = !0, $scope.revert()
                        })
                }
            }
            $scope.AllBrandAssigned = function () {
                $scope.asignData = [];
                var url = serviceBase + "api/Brandwisepramotion";
                $http.get(url)
                    .success(function (data) {

                        console.log("success");
                        console.log(data);
                        $scope.asignData = data;
                    })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                    })
            }
            $scope.AllBrandAssigned();
            $scope.SetActive = function (item) {
                var url = serviceBase + "api/Brandwisepramotion/activedeactive";
                var dataToPost = item;

                $http.put(url, dataToPost)
                    .success(function (data) {
                        console.log("success");
                        console.log(data);
                        $scope.AllBrandAssigned();
                    })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                    })
            }
        }
        
    }
})();