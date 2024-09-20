
(function () {
    'use strict';
    angular
        .module('app')
        .controller('WarehousesubCategoryController', WarehousesubCategoryController);

    WarehousesubCategoryController.$inject = ['$scope', 'WarehouseSubCategoryService', "WarehouseCategoryService", 'CityService', 'StateService', "WarehouseService", "SubCategoryService", "$filter", "$http", "ngTableParams", '$modal'];

    function WarehousesubCategoryController($scope, WarehouseSubCategoryService, WarehouseCategoryService, CityService, StateService, WarehouseService, SubCategoryService, $filter, $http, ngTableParams, $modal) {
        console.log("WarehouseSubCategoryController reached");
        $scope.currentPageStores = {};


        ///----------------------------Mapping Warehouse Based ------------------------------////
        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

                $scope.getData($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();
        $scope.WarhouseStates = [];
        WarehouseService.getwarehousedistinctstates().then(function (results) {

            console.log("This is warehouse state");
            console.log(results.data);
            $scope.WarhouseStateSub = results.data;
        }, function (error) {
        });

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            console.log("This is city in city master");
            $scope.citys = results.data;
        }, function (error) {
        });

        $scope.warehouses = []
        WarehouseService.getwarehouse().then(function (results) {
            console.log("This is Warehouse name in Warehouse master");
            $scope.warehouses = results.data;
            console.log($scope.warehouses);

        }, function (error) {

        });


        $scope.WhCategoryData = {};



        $scope.WhCategorynew = [];
        $scope.whcount = 0;
        $scope.GetWarhouseSubCategory = function (WareHouse) {

            console.log(WareHouse);

            console.log("get Warehousecategory controller");
            console.log(WareHouse);

            $scope.WhCategoryold = [];
            $scope.WhCategoryAll = [];
            $scope.Wcategorys = [];

            WarehouseSubCategoryService.getWarhouseSubCategory(WareHouse).then(function (results) {

                console.log(results.data);
                console.log("get Warehousecategory Wcategorys");
                $scope.Wcategorys = results.data;
                // $scope.GetCategory();

            }, function (error) { })
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.checkAll = function () {

                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.Wcategorys, function (category) {
                    $scope.checkboxes.items[category.SubCategoryId] = $scope.selectedAll;
                });

            };

        $scope.checkboxes = { 'checked': false, items: {} };
        $scope.$watch('checkboxes.checked', function (value) {

            angular.forEach($scope.Wcategorys, function (category) {
                if (angular.isDefined(category.SubCategoryId)) {
                    $scope.checkboxes.items[category.SubCategoryId] = value;
                }
            });
        });

        $scope.$watch('checkboxes.items', function (values) {

            if (!$scope.Wcategorys) {
                return;
            }
            var checked = 0, unchecked = 0,
                total = $scope.Wcategorys.length;
            angular.forEach($scope.Wcategorys, function (category) {
                checked += ($scope.checkboxes.items[category.SubCategoryId]) || 0;
                unchecked += (!$scope.checkboxes.items[category.SubCategoryId]) || 0;
            });
            if ((unchecked == 0) || (checked == 0)) {
                $scope.checkboxes.checked = (checked == total);
            }
            angular.element(document.getElementById("select_all")).prop("indeterminate", (checked != 0 && unchecked != 0));
        }, true);



        $scope.AddWhSubCategory = function () {

            $("#po").prop("disabled", true);
            var modalInstance;
            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value == "true") {
                    $scope.CkdId.push(id);
                }
            }
            $scope.plist = [];
            for (var j = 0; j < $scope.CkdId.length; j++) {
                _.each($scope.Wcategorys, function (o2) {
                    if (o2.SubCategoryId == $scope.CkdId[j]) {
                        $scope.plist.push(o2);
                    }
                });
            }




            console.log($scope.WhCategoryold);
            var url = serviceBase + "api/WarehouseCategory/SubCategory";

            var dataToPost = $scope.plist;

            console.log("DATA:");
            console.log(dataToPost);

            dataToPost[0].WarehouseId = $scope.WhCategoryData.WarehouseId;
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    alert('Successfully Mapped');
                    location.reload();
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }


                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };


        //----------------------Mapping End Warehouse Based-------------------------///



        $scope.whcategorys = [];
        $scope.getData = function (WarehouseId) {
            WarehouseSubCategoryService.getwhsubcategoryswid(WarehouseId).then(function (results) {
                $scope.whcategorys = results.data;
                $scope.callmethod();
            }, function (error) {

            });
        }




        $scope.Activecate = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "activeSubwarehousecmodal.html",
                    controller: "WarehousesubCategoryeditctrl", resolve: { whcategory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.whcategorys.push(selectedItem);
                    _.find($scope.whcategorys, function (whcategory) {
                        if (whcategory.id == selectedItem.id) {
                            whcategory = selectedItem;
                        }
                    });
                    $scope.whcategorys = _.sortBy($scope.whcategorys, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    });
        };


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.whcategorys,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WarehousesubCategoryeditctrl', WarehousesubCategoryeditctrl);

    WarehousesubCategoryeditctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "whcategory", 'FileUploader'];

    function WarehousesubCategoryeditctrl($scope, $http, ngAuthSettings, $modalInstance, whcategory, FileUploader) {
        console.log("category");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();

        $scope.CategoryData = whcategory;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.putwarehousesubcategory = function (data) {
                $scope.loogourl = whcategory.LogoUrl;
                console.log("Update category");

                var url = serviceBase + "api/WarehouseCategory/WHSubCatAct";
                var dataToPost = {
                    WhSubCategoryId: data.WhSubCategoryId,
                    IsActive: data.IsActive,
                };
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
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })


            };
    }
})();

