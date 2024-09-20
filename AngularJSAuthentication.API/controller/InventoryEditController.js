

(function () {
    // 'use strict';

    angular
        .module('app')
        .controller('InventoryEditController', InventoryEditController);

    InventoryEditController.$inject = ['CurrentStockService', 'WarehouseService', 'InventoryPageService', '$modal', '$scope', "$filter", "$http","$window", "ngTableParams", "$interval"];

    function InventoryEditController(CurrentStockService, WarehouseService, InventoryPageService, $modal, $scope, $filter, $http, $window, ngTableParams, $interval) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.WarehouseId = {};
        $scope.itemData = [];
        $scope.isDisabled = true;
        $scope.InventoryStockData = [];
        //$scope.currentPageStores = {};
      //  $scope.pager = [];

        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;


            }, function (error) {
            });
        };
        $scope.getWarehosues();


        $scope.getInventory = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
            CurrentStockService.getstockWarehousebased(WarehouseId).then(function (results) {
                var id = parseInt(WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId === id;
                });

            }, function (error) {
            });
        };

       
        //Add Inventory to Show on InventoryEdit
        $scope.AddInventory = function (Item) {
            var count = 0;
            $scope.TotalPrice = 0;
            var item = JSON.parse(Item);
            
            if (item.CurrentInventory >= 0) {
                if (item !== null) {
                    $scope.isDisabled = false;
                } else {
                    $scope.isDisabled = true;
                }
                item.ManualReason = "";
                if ($scope.InventoryStockData.length < 1) {
                    var TotalPrice = 0;
                }
                if ($scope.InventoryStockData.length !== 0) {
                    for (var i = 0; i < $scope.InventoryStockData.length; i++) {
                        if ($scope.InventoryStockData[i].StockId === item.StockId) {
                            count++;
                            alert("Given item is already added.Please Check.");
                            break;
                        } else {
                            count = 0;
                        }
                    }
                }
                if ($scope.InventoryStockData.length === 0 || count === 0) {
                    
                    $scope.InventoryStockData.push(item);
                    for (var j = 0; j < $scope.InventoryStockData.length; j++) {
                        $scope.TotalPrice = $scope.TotalPrice + $scope.InventoryStockData[j].TotalAmount;
                    }
                    //$scope.callmethod();

                }
            } else { alert("Current Item can't add, Due to Current Inventory is Negative "); return false; }
        };
        //$scope.callmethod = function () {

        //    var init;
        //    $scope.stores = $scope.InventoryStockData;

        //    $scope.searchKeywords = "";
        //    $scope.filteredStores = [];
        //    $scope.row = "";
        //    $scope.numPerPageOpt = [5,10,30,50];
        //    $scope.numPerPage = $scope.numPerPageOpt[1];
        //    $scope.currentPage = 1;
        //    $scope.currentPageStores = [];
        //    $scope.search(); $scope.select(1);
        //}
        //$scope.select = function (page) {
        //    
        //    var end, start; console.log("select"); console.log($scope.stores);
        //    start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        //}

        //$scope.onFilterChange = function () {
        //    
        //    console.log("onFilterChange"); console.log($scope.stores);
        //    $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        //}

        //$scope.onNumPerPageChange = function () {
        //    
        //    console.log("onNumPerPageChange"); console.log($scope.stores);
        //    $scope.select(1); $scope.currentPage = 1;
        //}

        //$scope.onOrderChange = function () {
        //    
        //    console.log("onOrderChange"); console.log($scope.stores);
        //    $scope.select(1); $scope.currentPage = 1;
        //}

        //$scope.search = function () {
        //    console.log("search");
        //    console.log($scope.stores);
        //    console.log($scope.searchKeywords);

        //    $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        //}


        //Search Inventory available in Current Stock.
        $scope.SearchInventory = function (key) {
            if (key == null || key == "") {
                alert("Please enter right keyword first.");
            }
            else {
                var dataToPost = {
                    First: 0,
                    Last: 1000,
                    ColumnName: "StockId",
                    IsAscending: true,
                    Contains: key
                };
                // $scope.pager.push(dataToPost);         
                var url = serviceBase + "api/InventoryEditController/getItemlistCurrentStock?WarehouseId=" + $scope.WarehouseId + "&key=" + key;
                $('#myOverlay').show();
                $http.get(url).success(function (data) {
                    $('#myOverlay').hide();
                    if (data) {
                        $scope.itemData = data;
                    }
                  else {
                        alert("No Items found.please enter right keyword.");
                    }

                    //if (data.TotalRecords > 0) {
                    //    $scope.itemData = data.CurrentStockPagerList;
                    //}
                    //else {
                    //    alert("No Items found.please enter right keyword.");
                    //}
                    // $scope.idata = angular.copy($scope.itemData);
                });
            }
        }


        //Save Inventory
        $scope.SendInventory = function (data) {
            
            if (data.length == 0) {
                $("#clickAndDisable").prop("disabled", false);
                alert("select item");
                return false;

            }

           

            var count = 0;
            var reason = 0;
            $scope.inventoryEditData = [];
            for (var j = 0; j < data.length; j++) {
                if (data[j].ManualReason === 'other') {
                    data[j].ManualReason = data.ManualReason;
                    var Reason = data[j].ManualReason;
                }
                else {
                    Reason = data[j].ManualReason;
                }
                var inventory = data[j].ManualInventory;
                if (inventory === undefined || inventory === 0 || inventory == null) {
                    count++;
                }
                if (Reason === undefined || Reason === "") {
                    reason++;
                }
            }

            if (count === 0 && reason === 0)
            {
                var IsPostest = true;
                for (var i = 0; i < data.length; i++)
                {
                    
                    if (data[i].CurrentInventory == 0 && data[i].ManualInventory < 0)
                    {
                        IsPostest = false;
                        alert("Please remove Negative qty in Zero Inventory Item : " + data[i].itemname);
                        $("#clickAndDisable").prop("disabled", false);
                        return false;
                    } else {
                        var dataToPost = {
                            StockId: data[i].StockId,
                            WarehouseId: $scope.WarehouseId,
                            ManualInventory: data[i].ManualInventory,
                            ManualReason: data[i].ManualReason,
                            ItemNumber: data[i].ItemNumber
                        };
                        $scope.inventoryEditData.push(dataToPost);
                        console.log(dataToPost);
                        
                    }
                }

                if (IsPostest) { 
                if ($window.confirm("Please confirm?"))
                {
                    var url = serviceBase + "api/InventoryEditController/addStock";
                    console.log(dataToPost);
                    $http.post(url, $scope.inventoryEditData).success(function (results) {

                        if (results != null) {
                            alert(results.Message);
                            // window.open("#/InventoryApprovalPage");
                            if (results.Status)
                                window.location.reload();
                        }
                        else {
                            alert("Something went Wrong.");
                        }
                        //    $scope.inventoryEditData = results;
                        //});
                    }, function (error) {
                    });

                    } else { $("#clickAndDisable").prop("disabled", false); }
                }

            }
            else {


                alert("Reason and inventory both are Compulsary.then send for approval.")
                $("#clickAndDisable").prop("disabled", false);
            }

        };

        //onwarehousechnage
        $scope.WarehouseChange = function () {
            if ($scope.InventoryStockData.length > 0 || $scope.itemData.length > 0)
                if (confirm("Are you sure? to change warehouse")) {
                    $scope.InventoryStockData = [];
                    $scope.itemData = [];
                }
        };

        //Remove item from list
        $scope.remove = function (item) {
            $scope.TotalPrice = 0;
            var index = $scope.InventoryStockData.indexOf(item);
            $scope.InventoryStockData.splice(index, 1);
            for (var j = 0; j < $scope.InventoryStockData.length; j++) {
                $scope.TotalPrice = $scope.TotalPrice + $scope.InventoryStockData[j].TotalAmount;
            }
            $scope.callmethod();
            alert("Successfully Deleted.");
        };

        $scope.ValidateQty = function (InventoryStock) {           
            var ManualInventory = 0;
            if (InventoryStock.ManualInventory < 0) {
                ManualInventory = -1 * InventoryStock.ManualInventory;
                if (ManualInventory > InventoryStock.CurrentInventory) {
                    alert("Inventory should not less then current Inventory");
                    InventoryStock.ManualInventory = 0;
                }
            }
        };
    }

})();

