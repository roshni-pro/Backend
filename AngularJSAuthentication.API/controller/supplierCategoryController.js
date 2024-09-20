


    (function () {
        'use strict';

        angular
            .module('app')
            .controller('supplierCategoryController', supplierCategoryController);

        supplierCategoryController.$inject = ['$scope', 'supplierCategoryService', "$filter", "$http", "ngTableParams", '$modal', '$log'];

        function supplierCategoryController($scope, supplierCategoryService, $filter, $http, ngTableParams, $modal, $log) {
            console.log("supplier category");
            $scope.currentPageStores = {};

            $scope.open = function () {
                console.log("add modal open");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "mysupplierCategoryModal.html",
                        controller: "ModalInstanceCtrlsupplier", resolve: { supplierCategory: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                        console.log("Cancel Condintion");
                        $log.info("Modal dismissed at: " + new Date());
                    });
            };


            $scope.opendelete = function (data, $index) {
                console.log(data);
                console.log("Open Dialog called");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeletesupplierCategory.html",
                        controller: "ModalInstanceCtrldeletesupplierCategory", resolve: { supplierCategory: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    });
            };

            $scope.edit = function (item) {
                console.log("Open Dialog called");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "mysupplierCategoryModalPut.html",
                        controller: "ModalInstanceCtrlsupplier", resolve: { supplierCategory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.supplierCategory.push(selectedItem);
                    _.find($scope.supplierCategory, function (supplierCategory) {
                        if (supplierCategory.id == selectedItem.id) {
                            supplierCategory = selectedItem;
                        }
                    });

                    $scope.supplierCategory = _.sortBy($scope.supplierCategory, 'Id').reverse();
                    $scope.selected = selectedItem;



                },
                    function () {
                        console.log("Cancel Condintion");
                    });
            };



            $scope.supplierCategory = [];

            supplierCategoryService.getsupplierCategory().then(function (results) {

                $scope.supplierCategory = results.data;
                $scope.callmethod();
            }, function (error) {
                //alert(error.data.message);
            });
            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.supplierCategory;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

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

                $scope.numPerPageOpt = [3, 5, 10, 20];
                $scope.numPerPage = $scope.numPerPageOpt[2];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
            }

        }
    })();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlsupplier', ModalInstanceCtrlsupplier);

    ModalInstanceCtrlsupplier.$inject = ["$scope", 'supplierCategoryService', '$http', "$modalInstance", "supplierCategory", 'ngAuthSettings'];

    function ModalInstanceCtrlsupplier($scope, supplierCategoryService, $http, $modalInstance, supplierCategory, ngAuthSettings) {
        console.log("hiiii");
        console.log(supplierCategory);

        $scope.supplierCategoryData = {

        };
        if (supplierCategory) {
            $scope.supplierCategoryData = supplierCategory;
            console.log($scope.supplierCategoryData.CategoryName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };



            $scope.AddsupplierCategory = function (data) {


                console.log("supplierCategory");
                var url = serviceBase + "api/SupplierCategory";
                var dataToPost = { SupplierCaegoryId: $scope.supplierCategoryData.SupplierCaegoryId, CategoryName: $scope.supplierCategoryData.CategoryName };
                console.log(dataToPost);


                $http.post(url, dataToPost)
                    .success(function (data) {

                        console.log("Error Gor Here");
                        console.log(data);
                        if (data.id == 0) {

                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }

                        }
                        else {
                            //console.log(data);
                            //  console.log(data);
                            $modalInstance.close(data);
                        }

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })

            };


        $scope.PutsupplierCategory = function (data) {
            $scope.supplierCategoryData = {

            };
            if (supplierCategory) {
                $scope.supplierCategoryData = supplierCategory;
                console.log("found Puttt supplierCategory");
                console.log(supplierCategory);
                console.log($scope.supplierCategoryData);

            }

            console.log("Update supplierCategory");
            var url = serviceBase + "api/SupplierCategory";
            var dataToPost = { SupplierCaegoryId: $scope.supplierCategoryData.SupplierCaegoryId, CategoryName: $scope.supplierCategoryData.CategoryName };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        //console.log(data);
                        //  console.log(data);
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };




    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeletesupplierCategory', ModalInstanceCtrldeletesupplierCategory);

    ModalInstanceCtrldeletesupplierCategory.$inject = ["$scope", '$http', "$modalInstance", "supplierCategoryService", 'ngAuthSettings', "supplierCategory"];

    function ModalInstanceCtrldeletesupplierCategory($scope, $http, $modalInstance, supplierCategoryService, ngAuthSettings, supplierCategory) {
        console.log("delete modal opened");

        $scope.supplierCategoryData = {};

        function ReloadPage() {
            location.reload();
        }

        if (supplierCategory) {
            $scope.supplierCategoryData = supplierCategory;
            console.log("found tasktype");
            console.log(supplierCategory);
            console.log($scope.supplierCategoryData);
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };




            $scope.deletesupplierCategorys = function (dataToPost, $index) {

                console.log("Delete Project controller");
                supplierCategoryService.deletesupplierCategorys(dataToPost).then(function (results) {
                    console.log("Del");

                    $modalInstance.close(dataToPost);
                    //ReloadPage();
                }, function (error) {
                    alert(error.data.message);
                });
            }

    }
})();


