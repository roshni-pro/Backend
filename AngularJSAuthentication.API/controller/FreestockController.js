
(function () {
    // 'use strict';

    angular
        .module('app')
        .controller('FreestockController', FreestockController);

    FreestockController.$inject = ['FreeStockService', 'WarehouseService', '$modal', '$scope', "$filter", "$http", "ngTableParams", "$interval"];

    function FreestockController(FreeStockService, WarehouseService, $modal, $scope, $filter, $http, ngTableParams, $interval) {
       
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            console.log(" Current Stock Controller reached");
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.compid = $scope.UserRole.compid;

            //$scope.getWarehosues = function () {
            //    WarehouseService.getwarehouse().then(function (results) {
            //        $scope.warehouse = results.data;
            //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            //        $scope.CityName = $scope.warehouse[0].CityName;
            //        $scope.Warehousetemp = angular.copy(results.data);
            //        $scope.getFreestock($scope.WarehouseId);
            //    }, function (error) {
            //    });
            //};
            //$scope.getWarehosues();
            $scope.wrshse = function () {
                var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
                $http.get(url)
                    .success(function (data) {
                        $scope.warehouse = data;
                        $scope.WarehouseId = $scope.warehouse[0].value;
                        $scope.CityName = $scope.warehouse[0].label;
                        $scope.Warehousetemp = angular.copy(data);
                        $scope.getFreestock($scope.WarehouseId);
                    });

            };
            $scope.wrshse();

            $scope.currentPageStores = {};
            $scope.Getstock = [];
            $scope.GetstockParent = [];
            $scope.getFreestock = function (WarehouseId) {

                FreeStockService.getstockWarehousebased(WarehouseId).then(function (results) {
                    var id = parseInt(WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.value === id;
                    });
                    $scope.Getstock = results.data;
                    $scope.GetstockParent = angular.copy(results.data);
                    $scope.callmethod();
                }, function (error) {
                });
            };

            $scope.CurrentStock = function ()
            {
                var preURI = saralUIPortal
                window.location = preURI + "/layout/current-stock/current-stock";
            }

            //****************upload****************************************************************************************///
            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.Getstock;

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


            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;

            $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [20, 30, 90, 100];  //dropdown options for no. of Items per page

            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;

            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown



            $scope.exportData = function () {
                alasql('SELECT FreeStockId,ItemNumber,ItemMultiMRPId,itemname,MRP,CurrentInventory,PlannedStock,WarehouseName INTO XLSX("FreeCurrentstock.xlsx",{headers:true}) FROM ?', [$scope.stores]);


            };


            $scope.TransferCurrentStock = function (item) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "TransfertoFreeStockModal.html",
                        controller: "TransfertoCurrentStockController", resolve: { inventory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.Getstock.push(selectedItem);
                    _.find($scope.Getstock, function (inventory) {
                        if (inventory.FreeStockId == selectedItem.FreeStockId) {
                            inventory = selectedItem;
                        }
                    });
                    $scope.Getstock = _.sortBy($scope.Getstock, 'FreeStockId').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.onClickCurrentInventory = function (item) {
                var modalInstance;
                debugger;
                modalInstance = $modal.open(
                    {
                        templateUrl: "CurrentInventoryStockMasterModal.html",
                        controller: "CurrentInventoryStockMasterController", resolve: { inventory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    debugger;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };


            //Historuy
            $scope.FreeStockId = 0;
            $scope.FreeoldStock = function (data) {
      
                $scope.ItemNumber = data.ItemNumber;
                $scope.WarehouseId = data.WarehouseId;
                $scope.FreeStockId = data.FreeStockId;
                $scope.FreeoldStocks($scope.pageno);
            }

            $scope.FreeoldStocks = function (pageno) {
                
                $scope.OldStockData = [];
                var url = serviceBase + "api/freestocks" + "?ItemNumber=" + $scope.ItemNumber + "&list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId + "&FreeStockId=" + $scope.FreeStockId;

                $http.get(url).success(function (response) {

                    if (response.total_count > 0) {

                        $scope.OldStockData = response.freestock;
                        $scope.total_count = response.total_count;
                    } else { alert("No record found"); }
                }, function (error) {
                        alert("Some thing went wrong");
                });
            };

            $scope.HistoryexportData = function (StockId) {

                $scope.exportDataRecord = [];

                var url = serviceBase + "api/freestocks/Export" + "?StockId=" + StockId + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {

                    $scope.exportDataRecord = response;
                    alasql('SELECT ItemMultiMRPId,FreeStockId,ItemNumber,itemname,ManualInventoryIn,InventoryIn,InventoryOut,TotalInventory,OdOrPoId,CreationDate INTO XLSX("FreeItemstockHistory.xlsx",{headers:true}) FROM ?', [$scope.exportDataRecord]);
                })
                    .error(function (data) {

                    })
            }


        }
        
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TransfertoCurrentStockController', TransfertoCurrentStockController);

    TransfertoCurrentStockController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function TransfertoCurrentStockController($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        debugger;
        //End User Tracking
        if (inventory) {
            $scope.inventory = inventory;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        var stockType = "F";
        var url = serviceBase + "api/CurrentStock/GetStockBatchMastersDataNew?ItemMultiMRPId=" + $scope.inventory.ItemMultiMRPId + "&WarehouseId=" + $scope.inventory.WarehouseId + "&stockType=" + stockType;
        $http.get(url).success(function (data) {
            $scope.GetStockBatchMastersList = data;
        });
        $scope.TransfertoCurrentStock = function (ManualReason) {
            if ($scope.GetStockBatchMastersList.length == 0) {
                alert('Please select batch code!!');
                return false;
            }
            $scope.DOdata = [];
            debugger;
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.GetStockBatchMastersList[i].check == true) {
                    if (ManualReason == null) {
                        alert('Please Enter Manual Reason !!');
                        return false;
                    }
                    if ($scope.inventory.CurrentInventory < $scope.GetStockBatchMastersList[i].DamageInventory) {
                        alert('Quantity must be less than Available Quantity ');
                        return false;
                    }
                    if ($scope.GetStockBatchMastersList[i].Qty < $scope.GetStockBatchMastersList[i].DamageInventory) {
                        alert('Quantity must be less than Available Quantity ');
                        return false;
                    }
                    if ($scope.GetStockBatchMastersList[i].DamageInventory > 0) {
                        $scope.DOdata.push({
                            BatchCode: $scope.GetStockBatchMastersList[i].BatchCode,
                            WarehouseId: $scope.inventory.WarehouseId,
                            Transferinventory: $scope.GetStockBatchMastersList[i].DamageInventory,
                            StockBatchMasterId: $scope.GetStockBatchMastersList[i].StockBatchMasterId,
                            ManualReason: ManualReason,
                            ItemMultiMRPId: $scope.inventory.ItemMultiMRPId,
                            ItemNumber: $scope.inventory.ItemNumber,
                        });

                    } else {
                        alert('Please Enter Quantity in ' + ' ' + $scope.GetStockBatchMastersList[i].BatchCode + ' ' + ' BatchCode');
                        return false;
                    }
                }
                else {

                }
            }
            if ($scope.DOdata && $scope.DOdata.length > 0 && ManualReason) {               
                //var dataToPost =
                //{
                //    ItemNumber: $scope.inventory.ItemNumber,
                //    ItemMultiMRPId: $scope.inventory.ItemMultiMRPId,
                //    WarehouseId: $scope.inventory.WarehouseId,
                //    Transferinventory: Transferinventory,
                //    ManualReason: ManualReason
                //}
                var url = serviceBase + "api/freestocks/TransferToCurrentStockV7";
                console.log($scope.DOdata);
                $http.put(url, $scope.DOdata)
                    .success(function (data) {
                        debugger;
                        alert(data);
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        alert(data);
                        debugger;
                    })

            }
            else {
                if ($scope.DOdata.length <= 0) {
                    alert(" Please Select Atleast one BatchCode");
                } else {
                    alert(" Please Enter Manual Reason");
                }
                
            }


        }
        $scope.onChangeQty = function (trade, DamageInventory) {
            debugger;
            if (trade.DamageInventory <= 0) {
                alert("Please Enter Damage inventory greater then 0!");
                trade.DamageInventory = null;
                return false;
            }
        }
        $scope.checkqty = function (StockBatchMasterId, qty, noOfqty, stockbatch) {
            debugger;
            $scope.selectedStockBatch = stockbatch;
            if (qty < noOfqty) {
                $scope.selectStockBatchMasterId = Number(StockBatchMasterId);
                for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                    if ($scope.GetStockBatchMastersList.length != null) {
                        if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                            $scope.GetStockBatchMastersList[c].DamageInventory = '';
                            alert('Free Qty cannot be greater then Qty!!');
                            return false;
                        }
                    }
                }
            }
        }
    }
})();



(function () {
    'use strict';

    angular
        .module('app')
        .controller('CurrentInventoryStockMasterController', CurrentInventoryStockMasterController);

    CurrentInventoryStockMasterController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function CurrentInventoryStockMasterController($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        debugger;
        //End User Tracking
        if (inventory) {
            $scope.inventory = inventory;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.batchMasterList = {};
        //Od.Id
        var url = serviceBase + 'api/CurrentStock/GetStockBatchMastersDataNew?ItemMultiMRPId=' + $scope.inventory.ItemMultiMRPId + '&WarehouseId=' + $scope.inventory.WarehouseId + '&stockType=' + 'F';
        $http.get(url)
            .success(function (data) {
                if (data != null) {
                    $scope.isDataNotFound = false;
                    $scope.batchMasterList = data;
                } else {
                    $scope.isDataNotFound = true;
                }

            });
    }
})();



