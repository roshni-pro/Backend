

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CurrentNetStockController', CurrentNetStockController);

    CurrentNetStockController.$inject = ['CurrentStockService', 'WarehouseService', '$modal', '$scope', "$filter", "$http", "ngTableParams"];

    function CurrentNetStockController(CurrentStockService, WarehouseService, $modal, $scope, $filter, $http, ngTableParams) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            console.log(" Current Stock Controller reached");
            $scope.Warehouseid = 1;
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.compid = $scope.UserRole.compid;

            //$scope.getWarehosues = function () {
            //    var url = serviceBase + 'api/Warehouse';
            //    $http.get(url)
            //        .success(function (response) {
            //            $scope.warehouse = response;
            //            $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            //            $scope.CityName = $scope.warehouse[0].CityName;
            //            $scope.Warehousetemp = angular.copy(response);
            //        }, function (error) {
            //        });

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
                    });

            };
            $scope.wrshse();

            $scope.currentPageStores = {};
            $scope.Getstock = [];

            $scope.getcurrentstock = function (WarehouseId) {
                
                $scope.NetTotalamount = 0;
                $scope.Getstock = [];
                $scope.UseForFilter = [];
                var Id = parseInt(WarehouseId);
                var url = serviceBase + 'api/CurrentNetStock?WarehouseId=' + Id;
                $http.get(url).success(function (results) {
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.value === Id;
                    });
                    $scope.CityName = $scope.filterData[0].CityName;
                    $scope.Getstock = results;
                    $scope.UseForFilter = angular.copy($scope.Getstock);

                    for (var i = 0; i < $scope.Getstock.length; i++) {
                        if ($scope.Getstock[i].CurrentNetStockAmount) {
                            $scope.NetTotalamount += $scope.Getstock[i].CurrentNetStockAmount;
                        } else { console.log($scope.Getstock[i].CurrentNetStockAmount + "  item :    " + $scope.Getstock[i].ItemName); }


                    }
                    $scope.callmethod();
                }, function (error) {
                });
            };

            ///Filter
            $scope.getFilterData = function (type) {
                
                $scope.Fdata = [];

                if (type == 'Active') {
                    $scope.Fdata = $filter('filter')($scope.UseForFilter, function (value) {
                        return value.IsActive === true;
                    });
                }
                else if (type == 'InActive') {
                    $scope.Fdata = $filter('filter')($scope.UseForFilter, function (value) {
                        return value.IsActive === false;
                    });
                }
                else if (type == '') {
                    $scope.Fdata = $scope.UseForFilter;
                }
                $scope.CityName = $scope.filterData[0].CityName;
                $scope.Getstock = $scope.Fdata;
                for (var i = 0; i < $scope.Getstock.length; i++) {
                    if ($scope.Getstock[i].CurrentNetStockAmount >= 0) {
                        $scope.NetTotalamount += $scope.Getstock[i].CurrentNetStockAmount;
                    } else { console.log($scope.Getstock[i].CurrentNetStockAmount + "  item :    " + $scope.Getstock[i].ItemName); }
                }
                $scope.callmethod();
            };

            //*************************************************************************************************************//
            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }
            $scope.exportData1 = function () {

             
                alasql('SELECT WarehouseName, ItemNumber,ItemMultiMrpId,MRP,StockId,ItemName,NetInventory,CurrentNetInventory,LiveQty,CurrentInventory,OpenPOQTy,CurrentDeliveryCanceledInventory,FreestockNetInventory,Unitprice,CurrentNetStockAmount,CreationDate, Case when IsActive >0 then True else False end as IsActive,AverageAging,AgingAvgPurchasePrice,AveragePurchasePrice,ABCClassification,MarginPercent,ItemlimitQty,ItemLimitSaleQty,PurchaseMinOrderQty,CategoryName,SubCategoryName,SubsubCategoryName,StoreName INTO XLSX("CurrentNetstock.xlsx",{headers:true}) FROM ?', [$scope.Getstock]);
            };


            $scope.AverageDetail = function (data) {
                

                if (data.AverageAging===0) {
                    alert("No data available.");
                    return false;
                }
                $http.get(serviceBase + 'api/CurrentNetStock/GetAllAveraginglist?warehouseId=' + $scope.WarehouseId  + '&itemMultiMRPId=' + data.ItemMultiMrpId).then(function (results) {

                   var data = results.data;

                    setTimeout(function () {
                        console.log("Modal opened Orderdetails");
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                templateUrl: "myAveragedetail.html",

                                controller: "AveragedetailsController", resolve: { Aeging: function () { return data } }
                            }), modalInstance.result.then(function (selectedItem) {

                                console.log("modal close");
                                console.log(selectedItem);

                            },
                                function () {
                                    console.log("Cancel Condintion");

                                });
                    }, 500);

                });





                console.log("Order Detail Dialog called ...");
            };



















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


            $scope.exportDeliveryCancelDetails = function (ItemMultiMrpId) {
                
                //$scope.DownloadDeliveryCancelDetail = DeliveryCancelDetail;

                $http.get(serviceBase + 'api/CurrentNetStock/CurrentDelieveryCancel?WarehouseId=' + $scope.WarehouseId + '&ItemMultiMrpId=' + ItemMultiMrpId).then(function (results) {

                    $scope.DownloadDeliveryCancelDetail = results.data;
                
                    alasql('SELECT OrderId,itemNumber,ItemMultiMRPId,qty, WarehouseId,IsFreeItem INTO XLSX("DeliveryCancelDetails.xlsx",{headers:true}) FROM ?', [$scope.DownloadDeliveryCancelDetail]);

                   

                });


                //$scope.DownloadDeliveryCancelDetail = [$scope.json_dataDC];

            };


        }
    }
})();
//(function () {
//    
//    'use strict';

//    angular
//        .module('app')
//        .controller('AveragedetailsController', AveragedetailsController);

//    AveragedetailsController.$inject = ['$scope', '$filter', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', 'Aeging', '$modalInstance'];

   

//    function AveragedetailsController($scope, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService, $modal, $modalInstance, Aeging) {
//        $scope.ok = function () { $modalInstance.close(); };
//        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
//        console.log("AveragedetailsController start loading OrderDetailsService");

//        $scope.data = Aeging;


//        $scope.exportaverage = function () {
//            
//            //$scope.DownloadAverageagingexcel = Averageagingexcel;
//            //$scope.DownloadDeliveryCancelDetail = [$scope.json_dataDC];

//            alasql('SELECT ItemMultiMRPId, WarehouseId,closingamount INTO XLSX("Averageagingexcel.xlsx",{headers:true}) FROM ?', [$scope.DownloadAverageagingexcel]);
//        };



//    }
//})();


app.controller("AveragedetailsController", ['$scope', '$filter', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', '$modalInstance', 'Aeging', function ($scope, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $modalInstance,Aeging) {
    $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    console.log("AveragedetailsController start loading OrderDetailsService");
    
    $scope.Aeging = Aeging;


        $scope.exportaverage = function () {
            
            $scope.DownloadAverageagingexcel = Aeging;
            //$scope.DownloadDeliveryCancelDetail = [$scope.json_dataDC];

            alasql('SELECT WarehouseName,ItemMultiMRPId,InDate,Ageing,ClosingQty,ClosingAmount INTO XLSX("Averageagingexcel.xlsx",{headers:true}) FROM ?', [$scope.DownloadAverageagingexcel]);
        };



    }

]);