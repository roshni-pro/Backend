

(function () {
    'use strict';

    angular
        .module('app')
        .controller('FilterItemsController', FilterItemsController);

    FilterItemsController.$inject = ['$scope', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function FilterItemsController($scope, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {

        console.log(" itemMaster Controller reached");


        console.log("itemMaster");
        $scope.itemMasterData =
            {
            };

        $scope.unitmaster = [];
        unitMasterService.getunitMaster().then(function (results) {
            $scope.unitmaster = results.data;
        }, function (error) {
        });


        $scope.subsubcategory = [];
        SubsubCategoryService.getsubsubcats().then(function (results) {
            $scope.subsubcategory = results.data;
        }, function (error) {
        });

        $scope.subcategory = [];
        SubCategoryService.getsubcategorys().then(function (results) {
            $scope.subcategory = results.data;
        }, function (error) {
        });

        $scope.category = [];
        CategoryService.getcategorys().then(function (results) {
            $scope.category = results.data;
        }, function (error) {
        });

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });



        $scope.getWareitemMaster = function (data) {
            $scope.WarehouseFilterData = [];
            $scope.WarehouseFilterData = {};
            console.log(data);
            $scope.WarehouseFilter = [];
            $scope.itemMasters = [];
            itemMasterService.GetitemMaster(data).then(function (results) {
                console.log("gett");

                $scope.itemMasters = results.data;

                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    return obj.warehouse_id == data.Warehouseid;
                })

                results.WarehouseFilterData();
                console.log($scope.WarehouseFilterData);

            },

                function (error) {

                    console.log("exel file is not uploaded...");
                });

        };

        $scope.AdditemMoveWarehouse = function (data) {

            console.log("Add itemMaster");
            console.log(data);
            var url = serviceBase + "api/ItemCopy";
            var dataToPost = data;

            dataToPost[0].Warehouseid = $scope.itemMasterData.Warehouseid;


            console.log("Succesfully Add item Master");
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log("data Added Succesfully");
                    alert("Data Moved Succesfully");
                    // console.log($scope.itemMaster);

                    console.log(data);

                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {

                        //$modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        };

        $scope.itemMasters = [];
        itemMasterService.GetitemMaster().then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            $scope.callmethod();
        }, function (error) {

        });



        $scope.currentPageStores = {};


        $scope.update = function () {
            console.log($scope.selectedItem);
            if ($scope.selectedItem == "TextBox") {
            }
            else if ($scope.selectedItem == "RadioButton") {
            }
            else if ($scope.selectedItem == "MultiSelect") {
            }

        }

        $scope.open = function () {
            console.log("Modal open Add Item ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "WarehouseModal.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.WarehouseFilterData.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");


                    })

        };




        $scope.itemMasters = [];
        itemMasterService.GetitemMaster().then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            $scope.callmethod();
        }, function (error) {

        });

    }
})();

