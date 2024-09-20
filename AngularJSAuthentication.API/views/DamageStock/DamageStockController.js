'use strict';
app.controller('DamageStockController', ['$scope', 'itemMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', function ($scope, itemMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {
    console.log(" DamageStockController Controller reached");


    $scope.warehouse = [];
    WarehouseService.getwarehouse().then(function (results) {
        console.log(results.data);
        console.log("data");
        $scope.warehouse = results.data;
    }, function (error) {
    });
    $scope.wid = '';
    $scope.WarehouseFilterData = [];
    $scope.examplemodel = [];
    $scope.exampledata = $scope.WarehouseFilterData;
    $scope.examplesettings = {
        displayProp: 'itemname', idProp: 'StockId',
        //externalIdProp: 'Mobile',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.getWareitemMaster = function (data) {

        // $scope.WarehouseFilterData = [];
        //$scope.WarehouseFilterData = {};
        console.log(data);
        $scope.WarehouseFilter = [];
        $scope.itemMasters = [];
        $scope.dataselect = [];
        $scope.wid = data.WarehouseId;

        if ($scope.wid > 0) {

            itemMasterService.getWarehouseStockItem($scope.wid).then(function (results) {
                console.log("gett");
                $scope.itemMasters = results.data;
                $scope.dataselect = results.data;
                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    if (obj) {
                        obj.itemname = obj.itemname + " >> '" + obj.ItemNumber + "' >>MRPID> '" + obj.ItemMultiMRPId + "' >> Qty:  '" + obj.CurrentInventory + "'";
                        return obj.WarehouseId == data.WarehouseId;
                    }
                })
                results.WarehouseFilterData;


            },
                function (error) {
                    console.log("exel file is not uploaded...");
                });
        }
    };

    $scope.onchange = function () {
        if ($scope.Stocktype != 0) {
            $scope.disableBtn = false
        }
        else {
            $scope.disableBtn = true
        }
    }

    $scope.Search = function (data) {
        debugger;
        //console.log($scope.Stocktype);
        $scope.Warehouseid = $scope.wid;

        var ids = [];
        _.each($scope.examplemodel, function (o2) {
            console.log(o2);
            for (var i = 0; i < $scope.dataselect.length; i++) {
                if ($scope.dataselect[i].StockId === o2.id) {
                    //var Row =
                    //{
                    //    "id": o2.id
                    //};
                    ids.push(o2.id);
                }
            }
        })
        var datatopost = {
            Warehouseid: $scope.Warehouseid,
            ids: ids
        }
        if (ids.length == 0) {
            alert('Please Parameter ');
            return false;
        }
        if ($scope.Stocktype == 0 || $scope.Stocktype == undefined) {
            alert('Please Select Stock Type');
            return false;
        }

        var url = serviceBase + "api/damagestock/filtre";
        $http.post(url, datatopost)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                    console.log(data);
                }
                else {
                    console.log("error");
                    //Allbydate = data;
                }
                console.log("Data:", data);
                $scope.item = data;

            });

    }
    $scope.IsConfirmDisble = false;
    $scope.Addamagestock = function (data) {
        debugger
        if ($scope.IsConfirmDisble == false) {
            $scope.IsConfirmDisble = false;
                if (data.qty == 0 || data.qty == undefined) {
                    alert('Please Enter Quantity');
                    $scope.IsConfirmDisble = false;
                }
                else if (data.reasontotransfer == '' || data.reasontotransfer == undefined) {
                    alert('Please Enter a reason for transfer');
                    $scope.IsConfirmDisble = false;
                }
                //else if (data.qty > data.CurrentInventory) {
                //    alert('Your qty greater than current stock qty');
                //    $scope.IsConfirmDisble = false;
                //}
                else if (data.qty > data.Qty) {
                    alert('Your qty greater than current stock qty');
                    $scope.IsConfirmDisble = false;
                }
                else {
                    if (confirm("Please confirm?")) {
                    var url = serviceBase + "api/damagestock/damage";
                    var dataToPost = {
                        Warehouseid: data.WarehouseId,
                        WarehouseName: data.WarehouseName,
                        StockId: data.StockId,
                        ItemNumber: data.ItemNumber,
                        ItemName: data.itemname,
                        DamageInventory: data.qty,
                        ReasonToTransfer: data.reasontotransfer,
                        ItemMultiMRPId: data.ItemMultiMRPId,
                        ABCClassification: data.ABCClassification,
                        stocktype: $scope.Stocktype,
                        StockBatchMasterId: data.StockBatchMasterId
                    };
                    console.log(dataToPost);
                    $http.post(url, dataToPost)
                        .success(function (response) {

                            $scope.items = response;
                            if (response != null) {
                                alert($scope.items.Message);
                                $scope.IsConfirmDisble = false;
                                $("#st" + response.StockId).prop("disabled", true);
                                location.reload();
                            }
                            else {

                                alert(data.ItemNumber + 'Item not transfered');
                            }
                        });
                    }
        } 
        }
    }
    $scope.currentPageStores = {};
    $scope.pageno = 1; // initialize page no to 1
    $scope.total_count = 0;
    $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
    $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page
    $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selected;
        $scope.getdamagedata($scope.pageno);
    }
    $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

    $scope.$on('$viewContentLoaded', function () {
        //$scope.getdamagedata($scope.pageno);
    });




}]);
