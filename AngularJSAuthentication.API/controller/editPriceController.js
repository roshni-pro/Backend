

(function () {
    'use strict';

    angular
        .module('app')
        .controller('editPriceController', editPriceController);

    editPriceController.$inject = ['$scope', 'itemMasterService', 'editPriceService', 'OrderMasterService', 'supplierService', 'OrderDetailsService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function editPriceController($scope, itemMasterService, editPriceService, OrderMasterService, supplierService, OrderDetailsService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.warehouse = [];


        $scope.getWarehosues = function () {  // tejas for getSpecificWarehouses 01/11/2019


            //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {

                    console.log("whwhwhwyhh", response);
                    $scope.warehouse = response;


                }, function (error) {
                })
        };
        $scope.getWarehosues();



        console.log(" edit Price Controller reached");
        $scope.currentPageStores = {};
        $scope.cities = [];
        OrderMasterService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });
        $scope.category = [];
        $scope.categorytemp = [];
        CategoryService.getcategorys().then(function (results) {

            $scope.category = results.data;
            $scope.categorytemp = angular.copy(results.data);

        }, function (error) {
        });

        $scope.subcategorymapping = [];
        $scope.Getsubcategorymapping = function () {

            $http.get(serviceBase + 'api/editPrice/GetSubCategoryMapping').then(function (results) {
                $scope.subcategorymapping = results.data;
            });
        }

        $scope.Getsubcategorymapping();

        $scope.subsubcategorymapping = [];
        $scope.Getsubsubcategorymapping = function () {
            $http.get(serviceBase + 'api/editPrice/GetSubsubCategoryMapping').then(function (results) {
                $scope.subsubcategorymapping = results.data;
            });
        }

        $scope.Getsubsubcategorymapping();


        $scope.vm = {
            subcategoryss: [],
            subsubcategoryss: []
        };

        $scope.SelectSubCat = function (data) {

            $scope.vm.subcategoryss = [];
            angular.forEach($scope.subcategorymapping, function (cat, key) {
                if (cat.Categoryid == data.Categoryid) {
                    var exist = false;
                    angular.forEach($scope.vm.subcategoryss, function (subcat, key) {
                        if (subcat.SubCategoryId == cat.SubCategoryId) {
                            exist = true;
                            return false;
                        }
                    });
                    if (!exist)
                        $scope.vm.subcategoryss.push(cat);
                }
            });


        };

        $scope.SelectSubSubCat = function (data) {

            $scope.vm.subsubcategoryss = [];
            angular.forEach($scope.subsubcategorymapping, function (cat, key) {
                if (cat.SubCategoryId == data.SubCategoryId && cat.Categoryid == data.Categoryid) {
                    var exist = false;

                    angular.forEach($scope.vm.subsubcategoryss, function (subcat, key) {

                        console.log(subcat.SubsubCategoryid + "  -------- " + subcat.SubSubCategoryId);
                        if (subcat.SubsubCategoryid == cat.SubsubCategoryid) {
                            exist = true;
                            return false;
                        }
                    });
                    if (!exist) {
                        $scope.vm.subsubcategoryss.push(cat);
                    }

                }
            });
        };
        //....................................................//
        $scope.show = true;
        $scope.order = false;
        $scope.orders = [];
        $scope.dataforsearch = { WarehouseId: "", Categoryid: "", SubCategoryId: "", SubsubCategoryid: "" };

        $scope.Search = function (data) {
            console.log("data for search here");
            $scope.dataforsearch.WarehouseId = data.WarehouseId;
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.Categoryid = data.Categoryid;
            $scope.dataforsearch.SubCategoryId = data.SubCategoryId;
            $scope.dataforsearch.SubsubCategoryid = data.SubsubCategoryid;
            $scope.orders = [];
            itemMasterService.getfiltereditemmaster($scope.dataforsearch).then(function (results) {
                console.log("filter details");

                console.log(results.data);
                $scope.orders = results.data;
                //$scope.AddTrack("View", "EditPrice:", "");

                $scope.callmethod();
            });
        }

        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.orders;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.numPerPageOpt = [30, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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

        $scope.itemMasters = [];

        //...........Exel export Method............//
        $scope.currentPageStores = {};

        // single margin update
        $scope.marginupdate = function (data) {

          debugger;
            if (((data.POPurchasePrice - data.PurchasePrice) / data.POPurchasePrice) * 100 >= 2) {
                var FirstAlert = confirm(" Purchase price is  " + parseFloat(((data.POPurchasePrice - data.PurchasePrice) / data.POPurchasePrice) * 100).toFixed(2)  + " %  is less than of PO price , Do you want to process");
                if (FirstAlert == true) {
                    var SecondAlert = confirm("Purchase price is " + parseFloat(((data.POPurchasePrice - data.PurchasePrice) / data.POPurchasePrice) * 100).toFixed(2) +  " %  is less than of PO price , Do you want to process");
                    if (SecondAlert == true) {
                        console.log('Istrue');
                    }
                    else {
                        return;
                    }
                }
                else {
                    return;
                }
            }




            if (!data.SupplierId && !data.DepoId) {
                alert("Select Supplier ");
                return false;
            }
            if (data.UnitPrice > data.price) {
                alert("Unit price must be less than MRP");
                return false;
            }
            if (data.POPurchasePrice > data.price) {
                alert("PO Purchase price must be less than MRP");
                return false;
            }
            if (data.PurchasePrice > data.price) {
                alert("Purchase price must be less than MRP");
                return false;
            }
            else if (data.PurchasePrice == 0 || data.POPurchasePrice == 0) {
                alert("Purchase price can't be zero");
                return false;
            }
            var url = serviceBase + "api/editPrice/byid";
            var dataToPost = {
                Value: $scope.eFutureType = 'Percentage '
            };

            $http.put(url, data)
                .then(function (response) {
                    if (response.data.Status) {
                        $scope.itemMasters = response.data.data;
                        alert(response.data.Message);

                        var id = parseInt(data.ItemId);
                        $scope.filterData = $filter('filter')($scope.itemMasters, function (value) {
                            return value.ItemId === id;
                        });
                        $("#st" + data.ItemId).prop("disabled", true);
                        if ($scope.filterData[0].UnitPrice <= $scope.filterData[0].NetPurchasePrice) {
                            alert("Selling price should be always greater than or equal to Net purchase price . Current Selling price is less than from Net Purchase price");
                        }
                        else if ($scope.filterData[0].UnitPrice >= $scope.filterData[0].price) {
                            alert("Selling price should be less than or equal to MRP price. Current Selling price is greater from MRP Price");
                        }
                    }
                    else
                    {
                        alert(response.data.Message);
                    }
                });
        };



        $scope.IsWarehouseChange = function () {

            $scope.orders = [];
            $scope.vm = {
                subcategoryss: [],
                subsubcategoryss: []
            };
            $scope.currentPageStores = {};

            if ($scope.category.length > 0) {
                $scope.category = [];
                setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.category = $scope.categorytemp;

                    });
                }, 500);
            }

        };



        $scope.EditSupplier = function (item) {

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "UpdateItemSupplier.html",
                        controller: "UpdateItemSupplierCtrl", resolve: { itemMaster: function () { return item } }
                    }), modalInstance.result.then(function (selectedItem) {
                        $scope.itemMaster.push(selectedItem);
                        _.find($scope.itemMaster, function (itemMaster) {
                            if (itemMaster.id == selectedItem.id) {
                                itemMaster = selectedItem;
                            }
                        });
                        $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                        $scope.selected = selectedItem;
                    },
                        function () {
                        })
            
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('UpdateItemSupplierCtrl', editPriceController);
    editPriceController.$inject = ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', "$filter"];
    function editPriceController($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService, $filter) {

        $scope.Depo = [];
        $scope.GetDepodetails = function (data) {
            supplierService.getdepodata(data).then(function (results) {
                $scope.Depo = results.data;
            }, function (error) {
            });
        };
        if (itemMaster) {
            $scope.itemMasterData = itemMaster;
            $scope.GetDepodetails(itemMaster.SupplierId);
        }

        $scope.supplier = [];
        supplierService.getsuppliers().then(function (results) {
            $scope.supplier = results.data;
        }, function (error) {
        });
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); }


        // single  update
        $scope.MappedSupplier = function (ItemId, DepoId, SupplierId) {


            if (!ItemId) {
                alert("select item");
                return false;
            }
            else if (!DepoId) {
                alert("select Depo ");
                return false;
            }
            else if (!SupplierId) {
                alert("select supplier ");
                return false;
            }
            var url = serviceBase + "api/editPrice/UpdateSupplier";
            var dataToPost =
            {

                ItemId: ItemId,
                DepoId: DepoId,
                SupplierId: SupplierId

            }
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        alert('Updated Successfully');
                        //$scope.AddTrack("Edit(Item)", "ItemName:", dataToPost.itemname);
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                })
        };


    }
})();
