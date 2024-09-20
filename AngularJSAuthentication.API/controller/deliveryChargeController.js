(function () {
    'use strict';
    angular
        .module('app')
        .controller('deliveryChargeController', deliveryChargeController);
    deliveryChargeController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'DeliveryChargeService', 'WarehouseService'];
    function deliveryChargeController($scope, $filter, $http, ngTableParams, $modal, DeliveryChargeService, WarehouseService) {
        console.log(" deliveryCharge Controller reached");
        $scope.currentPageStores = {};
        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getData($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.storeList = [];
        $scope.getStoreList = function () {
            var url = serviceBase + 'api/Store/GetAllStore';
            $http.get(url)
                .success(function (response) {
                    debugger;
                    $scope.storeList = response;
                });
        };
        $scope.getWarehosues();
        $scope.getStoreList();
        $scope.DeliveryData = [];

        $scope.getData = function (WarehouseId) {
            DeliveryChargeService.getWHBasedDeliveryData(WarehouseId).then(function (results) {
                $scope.DeliveryData = results.data;
                $scope.callmethod();
            }, function (error) {
            });
        };

        $scope.open = function () {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "muDeliveryAdd.html",
                    controller: "ModalInstanceCtrldelivery", resolve: { delivery: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                debugger;
                    $scope.storeList.forEach(store => {
                        debugger;
                        if (selectedItem.storeId == store.Id) {
                            selectedItem.storeName = store.Name;
                        }
                        //el.storeName = $scope.storeList.filter(re => re.Id == el.storeId)[0] ? $scope.storeId.filter(re => re.Id == el.storeId)[0].Name : 'Universal';
                    });
                //selectedItem.storeName = $scope.storeList.filter(re => re.Id == selectedItem.storeId) ? $scope.storeId.filter(re => re.Id == selectedItem.storeId)[0].Name : 'Universal';
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.edit = function (item) {
            console.log("Edit Dialog called survey");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "muDeliveryAdd.html",
                    controller: "ModalInstanceCtrldelivery", resolve: { delivery: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                debugger;
                $scope.DeliveryData.push(selectedItem);
                _.find($scope.DeliveryData, function (delivery) {
                    if (delivery.id == selectedItem.id) {
                        delivery = selectedItem;
                    }
                });
                $scope.DeliveryData = _.sortBy($scope.DeliveryData, 'Id').reverse();
                $scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.DeliveryData;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";
              
            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.tempPageStores = [];
            $scope.search(), $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage;
            $scope.tempPageStores = $scope.filteredStores.slice(start, end);
            $scope.tempPageStores.forEach(el => {
                debugger;
                $scope.storeList.forEach(store => {
                    debugger;
                    if (el.storeId == store.Id) {
                        el.storeName = store.Name;
                    }
                    //el.storeName = $scope.storeList.filter(re => re.Id == el.storeId)[0] ? $scope.storeId.filter(re => re.Id == el.storeId)[0].Name : 'Universal';
                });                
            });

            $scope.currentPageStores = $scope.tempPageStores;

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
(function () {
    'use strict';
    angular
        .module('app')
        .controller('ModalInstanceCtrldelivery', ModalInstanceCtrldelivery);

    ModalInstanceCtrldelivery.$inject = ["$scope", '$http', 'ngAuthSettings', "DeliveryChargeService", "$modalInstance", "delivery"];

    function ModalInstanceCtrldelivery($scope, $http, ngAuthSettings, DeliveryChargeService, $modalInstance, delivery) {
        console.log("delivery Charge");
        console.log(delivery);

        $scope.DeliveryData = {};
        if (delivery) {
            console.log("category if conditon");
            $scope.DeliveryData = delivery;
            console.log($scope.DeliveryData.id);
        }

        DeliveryChargeService.getWarhouse().then(function (results) {

            $scope.warehouse = results.data;
        }, function (error) {
        });
        $scope.storeList = [];
        $scope.getStoreList = function () {
            var url = serviceBase + 'api/Store/GetAllStore';
            $http.get(url)
                .success(function (response) {
                    debugger;
                    $scope.storeList = response;
                });
        };
        $scope.getStoreList();
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddDeliveryCharge = function () {
            
            if ($scope.DeliveryData.del_Charge == null || $scope.DeliveryData.del_Charge == "") {
                alert('Please Enter Delivary Charge');
                $modalInstance.open();
            }
            else if ($scope.DeliveryData.max_Amount == null || $scope.DeliveryData.max_Amount == "") {
                alert('Please Enter Max Amount');
                $modalInstance.open();
            }
            else if ($scope.DeliveryData.min_Amount == null || $scope.DeliveryData.min_Amount == "") {
                alert('Please Enter Minimum Amount');
                $modalInstance.open();
            }
            var url = serviceBase + "api/deliverycharge";
            if ($scope.DeliveryData.WarehouseId != null || $scope.DeliveryData.WarehouseId != undefined) {
                debugger;
                var dataToPost = {
                    min_Amount: $scope.DeliveryData.min_Amount,
                    max_Amount: $scope.DeliveryData.max_Amount,
                    del_Charge: $scope.DeliveryData.del_Charge,
                    WarehouseId: $scope.DeliveryData.WarehouseId,
                    //cluster_Id: $scope.DeliveryData.cluster_Id,
                    IsActive: $scope.DeliveryData.IsActive,
                    IsDistributor: $scope.DeliveryData.IsDistributor,
                    storeId: $scope.DeliveryData.storeId,
                    id: $scope.DeliveryData.id
                };
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
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })

            }
            else {
                alert("Select warehouse");
            }
        };
    }
})();

