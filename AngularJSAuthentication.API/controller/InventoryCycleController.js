
(function () {
    'use strict';

    angular
        .module('app')
        .controller('InventoryCycleController', InventoryCycleController);

    InventoryCycleController.$inject = ['$scope', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal'];

    function InventoryCycleController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log(" txt Controller reached");
        //User Tracking
        $scope.CurrentDate = [];
        var currentdate = new Date().toISOString().slice(0, 10);
        $scope.dateNow = currentdate;
        $scope.searchdata = {};

        //var url = serviceBase + "api/InventoryCycle/GetInventoryCount/V1";

        //$http.get(url)
        //    .success(function (results) {

        //        $scope.searchdatadata = results;
        //        if ($scope.searchdatadata && $scope.searchdatadata.length > 0) {
        //            $scope.searchdatadata.forEach(function (item) {
        //                var createdDate = new Date(item.CreatedDate);
        //                var createdDateMonth = createdDate.getMonth() + 1;
        //                var createdDateDay = createdDate.getDate();
        //                var createdDateYear = createdDate.getFullYear();
        //                var createdDateText = createdDateYear + "-" + createdDateMonth + "-" + createdDateDay;
        //                item.CreatedDate = createdDateText;
        //            });
        //        }

        //        var today = new Date();
        //        var todayMonth = today.getMonth() + 1;
        //        var todayDay = today.getDate();
        //        var todayYear = today.getFullYear();
        //        var todayDateText = todayYear + "-" + todayMonth + "-" + todayDay;
        //        $scope.CurrentDate = todayDateText;

        //    })
        //    .error(function (data) {
        //        console.log(data);
        //    });


        $scope.getWarehosues = function () {
            WarehouseService.getwarehouseOnAssign().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        $scope.getWarehosues();

        // $scope.Comment = "";
        $scope.exportData = function () {

            $scope.storesitem = $scope.searchdatadata;
            $scope.storesitem.forEach(function (item, index) {
                Object.keys(item).forEach(function (key, ind) {
                    ($scope.storesitem[index][key] == null || $scope.storesitem[index][key] == '') ? $scope.storesitem[index][key] = '-' : $scope.storesitem[index][key] = $scope.storesitem[index][key];
                });
            });

            alasql('SELECT WarehouseName,ItemName,ABCClassification,InventoryCount,MRP,ItemMultiMRPId,PastInventory,CurrentInventory,Comment,CreatedDate INTO XLSX("InventoryCycleReport.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

        };


        $scope.printToCart = function (printSectionId) {
            var innerContents = document.getElementById(printSectionId).innerHTML;
            var popupWinindow = window.open('', '_blank', 'width=600,height=700,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');
            popupWinindow.document.open();
            popupWinindow.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + innerContents + '</html>');
            popupWinindow.document.close();
        };


        $scope.getInventory = function (WarehouseId) {

            if (WarehouseId == '' || WarehouseId.length == 0) {
                alert("Please select the Warehouse");
                return;
            }
            $scope.data = [];
            // $scope.ItemId = data.ItemId;
            //var url = serviceBase + "api/InventoryCycle/GetInvent?Warehouseid=" + data.WarehouseId + "&itemid=" + data.ItemId + "&Date=" + data.CreatedDate;
            var url = serviceBase + "api/InventoryCycle/GetInvent?Warehouseid=" + WarehouseId + '&Date=' + Date;

            $http.get(url)
                .success(function (results) {

                    $scope.searchdatadata = results;
                })
                .error(function (data) {
                    console.log(data);
                });
        };


        $scope.WarehouseId = [];
        // $scope.ItemId = [];        
        $scope.getInventCount = function (data) {
            if (data == null || data.WarehouseId === 'undefined' || data.WarehouseId == null || data.WarehouseId.length == 0) {
                alert("Please select the Warehouse");
                return;
            }

            $scope.data = [];
            $scope.WarehouseId = data.WarehouseId;
            //  $scope.ItemId = data.ItemId;
            var Dateforformat = data.Date;
            Dateforformat = Dateforformat ? moment(Dateforformat).format('MM/DD/YYYY') : null;
            var url = serviceBase + "api/InventoryCycle/GetInvent?Warehouseid=" + data.WarehouseId + "&Date=" + Dateforformat;

            $http.get(url)
                .success(function (results) {
                    console.log("After api hit" + $scope.searchdata);
                    $scope.searchdatadata = results;
                    if ($scope.searchdatadata && $scope.searchdatadata.length > 0) {
                        $scope.searchdatadata.forEach(function (item) {
                            var createdDate = new Date(item.CreatedDate);
                            var createdDateMonth = createdDate.getMonth() + 1;
                            var createdDateDay = createdDate.getDate();
                            var createdDateYear = createdDate.getFullYear();
                            var createdDateText = createdDateYear + "-" + createdDateMonth + "-" + createdDateDay;
                            item.CreatedDate = createdDateText;
                        });
                    }

                    var today = new Date();
                    var todayMonth = today.getMonth() + 1;
                    var todayDay = today.getDate();
                    var todayYear = today.getFullYear();
                    var todayDateText = todayYear + "-" + todayMonth + "-" + todayDay;
                    $scope.CurrentDate = todayDateText;

                })
                .error(function (data) {
                    console.log(data);
                });
        };

        console.log("After  respnse api hit" + $scope.searchdata);
        $scope.data = [];
        //$scope.searchdata = [];
        $scope.Put = function (trade) {

            //$scope.data = trade;
            //for (var i = 0; i < $scope.data.length; i++) {
            //    var dataToPost = {
            //        WarehouseId: $scope.WarehouseId,
            //        ItemId: $scope.data[i].ItemId,
            //        InventoryCount: $scope.data[i].InventoryCount,
            //        Comment: $scope.data[i].Comment,
            //        CreatedDate: $scope.data[i].CreatedDate,
            //       // UpdatedDate: $scope.data[i].UpdatedDate
            //    };
            //    $scope.searchdata.push(dataToPost);
            //    console.log($scope.searchdata);
            //}
            var negative = false;
            angular.forEach(trade, function (value, key) {
                if (value.InventoryCount < 0) {
                    alert('Please enter positive value');
                    negative = true;

                };
            });

            if (negative == false) {

                var url = serviceBase + "api/InventoryCycle/EditInventCount";
                $http.put(url, trade)
                    .success(function (data) {

                        if (data) {
                            alert("Inventory update Successfully.");
                            window.location.reload();
                        }
                        else {
                            alert("Inventory update failed.");
                        }
                    })
                    .error(function (data) {
                        alert(data.ErrorMessage);
                    });
            }

        };

    }
})();

