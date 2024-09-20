(function () {
    // 'use strict';
    angular
        .module('app')
        .controller('SafetystockController', SafetystockController);

    SafetystockController.$inject = ['CityService', 'CurrentStockService', 'WarehouseService', '$modal', '$scope', "$filter", "$http"];

    function SafetystockController(CityService, CurrentStockService, WarehouseService, $modal, $scope, $filter, $http) {

        $scope.cities = [];
        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });

        $scope.getWarehosues = function () { // This would fetch the data on page change.
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.CityName = $scope.warehouse[0].CityName;
                $scope.Warehousetemp = angular.copy(results.data);

                $scope.getcurrentstock($scope.WarehouseId);
            }, function (error) {
            });
        };
        $scope.getWarehosues();
        $scope.currentPageStores = {};
        $scope.Exportstock = [];
        $scope.Getstock = [];

        $scope.getcurrentstock = function (WarehouseId) {
            
            $scope.Exportstock = [];
            $scope.SGetstock = [];
            $scope.currentPageStores = [];
            CurrentStockService.getstockWarehousebased(WarehouseId).then(function (results) {

                var id = parseInt(WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId === id;
                });
                $scope.SGetstock = results.data;
                $scope.callmethod();
            }, function (error) {
            });
        };


        $scope.SafetystockHistory = function (data) {
            var url = serviceBase + "api/CurrentStock/safetystockhistory?StockId=" + data.StockId;
            $http.get(url).success(function (data) {
                $scope.SafetyStock = data;
            }, function (error) {
            });
        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.SGetstock;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.numPerPageOpt = [30, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
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

        $scope.edit = function (item) {
            
            console.log("Edit Dialog called survey");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "MySafetyStockPut.html",
                    controller: "SafetyStockModalInstanceCtrlInventoryedit", resolve: { inventory: function () { return item } }
                });
        }
        $scope.exportData1 = function () {

            alasql('SELECT ABCClassification, itemname,ItemNumber,ItemMultiMRPId,MRP,SafetystockfQuantity INTO XLSX("SafetyStock.xlsx",{headers:true}) FROM ?', [$scope.SGetstock]);
        };

    }
})();

(function () {

    'use strict';
    angular
        .module('app')
        .controller('SafetyStockModalInstanceCtrlInventoryedit', SafetyStockModalInstanceCtrlInventoryedit);

    SafetyStockModalInstanceCtrlInventoryedit.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function SafetyStockModalInstanceCtrlInventoryedit($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        
        $scope.SafetystockData = inventory;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.PutsSafetyStock = function (data) {

            if ($scope.SafetystockData !== null) {
                var dataToPost = {
                    StockId: $scope.SafetystockData.StockId,
                    SafetystockfQuantity: data.SafetystockfQuantity
                };
                console.log(dataToPost);
                var url = serviceBase + "api/CurrentStock/SafetyStock";
                $http.put(url, dataToPost)
                    .success(function (data) {
                        if (data) {
                            alert(data);
                            $modalInstance.close(data);
                            window.location.reload();

                        } else { alert("Something went wrong"); }

                    })
            }
        }
    }
})();

