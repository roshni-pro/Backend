

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DamageStockItemCntrl', DamageStockItemCntrl);

    DamageStockItemCntrl.$inject = ['$scope', 'itemMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];
    function DamageStockItemCntrl($scope, itemMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {
            console.log(" DamageStockItemCntrl Controller reached");
            
            $scope.warehouse = [];
            //WarehouseService.getwarehouse().then(function (results) {

            //    console.log(results.data);
            //    console.log("data");
            //    $scope.warehouse = results.data;
            //}, function (error) {
            //}); 

       





        $scope.wrshse = async function () {
            /*debugger*/;
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                });
            //$scope.warehouse = await fetch(url);

        };
        $scope.wrshse();
        $scope.MultiWarehouseModel = [];
        $scope.MultiWarehouse = $scope.warehouse;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'label', idProp: 'value',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };


        $scope.wid = '';
        //$scope.getWareitemMaster = function (data) 
        $scope.getWareitemMaster = function () {
           
                //$scope.WarehouseFilterData = [];
                //$scope.WarehouseFilterData = {};
               // console.log(data);
                $scope.WarehouseFilter = [];
                $scope.itemMasters = [];
                $scope.dataselect = [];
               // $scope.wid = data.WarehouseId;

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
               
            let whids = $scope.MultiWarehouseModel.map(a => a.id);
            //var url = serviceBase + 'api/damagestock/PostWarehousebasedDitem?WarehouseId=' + WarehouseId;
            //console.log(url);
            itemMasterService.getWarehousestock(whids).then(function (results) {
               console.log("gett");
            
                //$http.post(url, whids)
                //    .success(function (results) {
                //        debugger;
                //       // $scope.searchdta = results;
                //        $scope.itemMasters = results.data;
                //        $scope.dataselect = results.data;
                //    })
                //    .error(function (data) {
                //        console.log(data);
                //    });
                    $scope.itemMasters = results.data;
                    $scope.dataselect = results.data;


                    if ($scope.itemMasters.length > 0) {
                        $scope.getdamagedata($scope.pageno);
                    }
                },
                    function (error) {
                        console.log("exel file is not uploaded...");
                    });
            };

     

        $scope.oldStockHistory = function (pageno) {
         
            $scope.OldStockDataH = [];
            let whids = $scope.MultiWarehouseModel.map(a => a.id);
            var url = serviceBase + "api/damagestock/GetDamageHistoryAll?WarehouseId=" + whids;
            $http.post(url).success(function (response) {

                if (response.total_count > 0) {

                    // $scope.AddTrack("View(CurrentStock)", "History: StockId", $scope.StockId);
                }

                $scope.OldStockDataH = response.ordermasterHistory;
                $scope.total_count = response.total_count;
                console.log($scope.OldStockDataH);
                $scope.ExportHistoryAll();

            })
                .error(function (data) {
                })
        }


            $scope.currentPageStores = {};
            $scope.pageno = 1; // initialize page no to 1
            $scope.total_count = 0;
            $scope.itemsPerPage = 20; //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
                $scope.getdamagedata();
                $scope.search(); $scope.select(1);
            }
            $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

            $scope.$on('$viewContentLoaded', function () {
                $scope.getdamagedata();

            });
       
        
            $scope.getdamagedata = function () {
                //debugger
                let whids = $scope.MultiWarehouseModel.map(a => a.id);
                console.log(whids);
                if (whids.length>0) {
                    $scope.currentPageStores = {};
                    //var url = serviceBase + "api/damagestock/getList" + "?WarehouseId=" + $scope.wid;
                    var url = serviceBase + "api/damagestock/postList" + "?WarehouseId=" + whids;

                    $http.post(url)
                        .success(function (results) {

                            $scope.currentPageStores = results.damagest;
                            $scope.AllData = results.damagest;

                            $scope.total_count = results.total_count;
                            $scope.Porders = $scope.currentPageStores;
                            $scope.callmethod();
                        })
                        .error(function (data) {
                            console.log(data);
                        })
                }
            };
        
       
            //$scope.getdamagedata = function (pageno) {
            //    
            //    $scope.currentPageStores = {};
            //    var url = serviceBase + "api/damagestock/get" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.wid;

            //    $http.get(url)
            //    .success(function (results) {
            //        $scope.currentPageStores = results.damagest;

            //        $scope.total_count = results.total_count;
            //        $scope.Porders=$scope.currentPageStores;
            //        $scope.callmethod();
            //    })
            //     .error(function (data) {
            //         console.log(data);
            //     })
            //};


            $scope.StockId = 0;
        $scope.oldStock = function (data) {
            
                $scope.ItemNumber = data.ItemNumber;
                $scope.WarehouseId = data.WarehouseId;
                $scope.StockId = data.DamageStockId;
                $scope.oldStocks($scope.pageno);
            }

        $scope.oldStocks = function (pageno) {
           
                $scope.OldStockData = [];
                var url = serviceBase + "api/damagestock/GetDamageHistory?ItemNumber=" + $scope.ItemNumber + "&list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId + "&StockId=" + $scope.StockId;
                $http.get(url).success(function (response) {

                    if (response.total_count > 0) {

                        // $scope.AddTrack("View(CurrentStock)", "History: StockId", $scope.StockId);
                    }

                    $scope.OldStockData = response.ordermaster;
                    $scope.total_count = response.total_count;
                    console.log($scope.OldStockData);

                })
                    .error(function (data) {
                    })
            }

        $scope.Export = function () {
            
            alasql('SELECT WarehouseName,DamageStockId,ItemName,ItemNumber,ABCClassification,ItemMultiMRPId,DamageInventory,CreatedDate,UnitPrice,ReasonToTransfer,UpdatedDate as Date INTO XLSX("DamageStock.xlsx",{headers:true})FROM ?', [$scope.AllData]);
            //alasql('SELECT WarehouseName,DamageStockId,ItemName,ItemNumber,ABCClassification,ItemMultiMRPId,DamageInventory,CreatedDate,UnitPrice,ReasonToTransfer,InwordQty,OutwordQty,UserName as EditBy,UpdatedDate as Date INTO XLSX("DamageStock.xlsx",{headers:true})FROM ?', [$scope.OldStockData]);
        }

        
        $scope.onClickDamageInventory = function (item) {
            var modalInstance;
            debugger;
            modalInstance = $modal.open(
                {
                    templateUrl: "DamageInventoryStockMasterModal.html",
                    controller: "DamageInventoryStockMasterController", resolve: { inventory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                debugger;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


     
        $scope.ExportHistoryAll = function () {
            
            alasql('SELECT WarehouseName,DamageStockId,ItemName,ItemNumber,ItemMultiMRPId,DamageInventory,CreatedDate,UnitPrice,ReasonToTransfer,InwordQty,OutwordQty,OdOrPoId,UserName as EditBy,UpdatedDate as Date INTO XLSX("DamageStock.xlsx",{headers:true})FROM ?', [$scope.OldStockDataH]);
              }  

            $scope.callmethod = function () {

                var init;
                return $scope.stores = $scope.Porders,
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
                    $scope.numPerPageOpt = [20, 30, 40, 50],
                    $scope.numPerPage = $scope.numPerPageOpt[0],
                    $scope.currentPage = 1,
                    $scope.currentPageStores = [],
                    (init = function () {
                        return $scope.search(), $scope.select($scope.currentPage)
                    })
                        ()
            }

    }

})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DamageInventoryStockMasterController', DamageInventoryStockMasterController);

    DamageInventoryStockMasterController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function DamageInventoryStockMasterController($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        debugger;
        //End User Tracking
        if (inventory) {
            $scope.inventory = inventory;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.batchMasterList = {};
        //Od.Id
        var url = serviceBase + 'api/CurrentStock/GetStockBatchMastersDataNew?ItemMultiMRPId=' + $scope.inventory.ItemMultiMRPId + '&WarehouseId=' + $scope.inventory.WarehouseId + '&stockType=' + 'D';
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
