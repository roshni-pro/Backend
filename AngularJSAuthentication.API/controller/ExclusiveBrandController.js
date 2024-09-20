

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ExclusiveBrandController', ExclusiveBrandController);

    ExclusiveBrandController.$inject = ['$scope', 'ItemPramotionService', 'WarehouseCategoryService', 'SubCategoryService', 'StateService', 'demandservice', "$filter", "$http", "ngTableParams", '$modal'];

    function ExclusiveBrandController($scope, ItemPramotionService, WarehouseCategoryService, SubCategoryService, StateService, demandservice, $filter, $http, ngTableParams, $modal) {


        console.log("pramotion Controller reached");

        $scope.currentPageStores = {};
        $scope.Allitempramotion = [];

        ItemPramotionService.getitempramotion().then(function (results) {

            $scope.Allitempramotion = results.data;

            $scope.currentPageStores = results.data;
            $scope.callmethod();
        }, function (error) {

        });

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
        //$scope.subcategory = [];
        //SubCategoryService.getsubcategorys().then(function (results) {
        //    console.log("subcategory is calling");
        //    console.log(results.data);
        //    $scope.subcategory = results.data;
        //}, function (error) {
        //});

        $scope.UpdateSubcategory = function (data) {
            console.log("update subcategory is calling");
            console.log(data);
            var url = serviceBase + "api/BrandPramotion";
            var dataToPost = data;
            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("success");
                    console.log(data);
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };

        $scope.UpdateSubcategory_city = function (data) {
            console.log("update subcategory is calling");
            console.log(data);
            var url = serviceBase + "api/BrandPramotion/PutExclusivebrand";
            var dataToPost = data;
            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("success");
                    console.log(data);
                    //if (data.id == 0) {
                    //    $scope.gotErrors = true;
                    //    if (data[0].exception == "Already") {
                    //        console.log("Got This User Already Exist");
                    //        $scope.AlreadyExist = true;
                    //    }

                    //}
                    //else {

                    //    $modalInstance.close(data);
                    //    //ReloadPage();
                    //}

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };



        $scope.warehouseCategory = [];
        $scope.subcategory = [];

        $scope.showsubcategory = function () {
            if ($scope.branddata.WarehouseId > 0) {
                $scope.tableshow = true;

                // api / SubCategory
                var url = serviceBase + "api/BrandPramotion?recordtype=warehouse&&warehouse=" + $scope.branddata.WarehouseId;



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
                WarehouseCategoryService.getwhcategorys().then(function (results) {
                    $scope.warehouseCategory = results.data;
                    console.log($scope.warehouseCategory);
                    console.log("warehouseCategory");
                }, function (error) {
                });
            }
            else {
                $scope.tableshow = false;
            }

        }


        //funtion for subcategories from citiy basis  ---------------------------------------------------------------------------------------------


        $scope.showsubcategory_city = function () {
            if ($scope.branddata.Cityid > 0) {
                $scope.tableshow = true;

                // api / SubCategory
                var url = serviceBase + "api/BrandPramotion?city=" + $scope.branddata.Cityid;

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
                WarehouseCategoryService.getwhcategorys().then(function (results) {
                    $scope.warehouseCategory = results.data;
                    console.log($scope.warehouseCategory);
                    console.log("warehouseCategory");
                }, function (error) {
                });
            }
            else {
                $scope.tableshow = false;
            }

        }



        $scope.open = function () {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "newitempramotion.html",
                    controller: "ModalInstanceCtrlPramotion", resolve: { category: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };




        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for category");
            // getitempramotion


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteCategory.html",
                    controller: "ModalInstanceCtrldeleteCategory", resolve: { category: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };



        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Allitempramotion;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

              

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select($scope.currentPage);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();
