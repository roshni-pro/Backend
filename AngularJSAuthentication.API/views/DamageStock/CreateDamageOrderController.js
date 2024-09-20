'use strict';


app.controller('CreateDamageOrderController', ['$scope', 'WarehouseService', "customerService", "$filter", "$http", "$window", "ngTableParams", '$modal', 'FileUploader', function ($scope, WarehouseService, customerService, $filter, $http, $window, ngTableParams, $modal, FileUploader) {


    $scope.warehouse = [];
    $scope.warehouseId = '';
    $scope.Searchkey = '';
    WarehouseService.getwarehouse().then(function (results) {

        $scope.warehouse = results.data;
    }, function (error) {
    });

    $scope.customers = [];
    $scope.Customerid = '';
    $scope.issetdisabled = false;
    $scope.isdisabled = function () {
        
        $scope.issetdisabled = true;
    }
    $scope.isdisabled();

    $scope.setordertype = function () {

        $scope.issetdisabled = false;
    }

    $scope.getCustData = function (WarehouseId, key) {


        $scope.customers = [];

        $scope.Searchkey = key;

        $scope.warehouseId = WarehouseId;
        //var url = serviceBase + 'api/damagestock/Custall?WarehouseId=' + $scope.warehouseId;
        var url = serviceBase + 'api/damagestock/GetWarehouseCustomer/' + $scope.warehouseId + '/' + $scope.Searchkey;

        $http.get(url)
            .success(function (data) {
                if (data) {
                    $scope.customers = data;

                }
                else {
                    alert("no record found or customer is not active");
                    window.location.reload();
                }
            });
    }


    $scope.getitemMaster = function () {

        //$scope.getDamageItem();
    }



    //$scope.getDamageItem = function () {

    //    $scope.DamageItemData = [];
    //    $scope.itemss = [];
    //    var url = serviceBase + 'api/damagestock/getall?WarehouseId=' + $scope.warehouseId;
    //    $http.get(url)
    //        .success(function (data) {
    //            $scope.DamageItemData = data;
    //            $scope.itemss = data;

    //        });
    //}

    
    $scope.isenablesearchreason = false;
    $scope.SearchReson = function (data) {
        
        
        if (data == "Other") {
            $scope.isenablesearchreason = true;
        }
        else {
            $scope.isenablesearchreason = false;
            $scope.reasonmessage = null;
        }
    }


    $scope.SearchDamageItem = function (data) {
        
        if ($scope.OrderType == "NR") {
            document.getElementById("nonSellableOrder").disabled = true;
            document.getElementById("damageOrder").disabled = true;
        }
        else if ($scope.OrderType == "N") {
            document.getElementById("nonRevenueOrder").disabled = true;
            document.getElementById("damageOrder").disabled = true;
        }
        else {
            document.getElementById("nonRevenueOrder").disabled = true;
            document.getElementById("nonSellableOrder").disabled = true;
        }
 
        $scope.DamageItemData = [];
       
        $scope.Warehouseid = data.WarehouseId;
        $scope.isValid = true;
        var url = serviceBase + "api/damagestock/getall?key=" + data.itemkey + "&WarehouseId=" + $scope.warehouseId + "&OrderType=" + $scope.OrderType;
        $http.get(url).success(function (data) {
          
            $scope.DamageItemData = data;
            $scope.itemss = data;
            console.log($scope.itemss,"$scope.itemss")
          

        });

    };

    $scope.isUnitPrice = false;
    $scope.isMRPPrice = false;
    $scope.filtitemMaster = function (data) {
      
        $scope.GetStockBatchMastersList = [];

        $scope.selecteditem = JSON.parse(data.ItemId);
        var stockType = "D";
        if ($scope.OrderType == "N") {
            stockType = "N";
        } else if ($scope.OrderType == "NR") {
            stockType = "NR";
        }
        
        var url = serviceBase + "api/CurrentStock/GetStockBatchMastersDataNew?ItemMultiMRPId=" + $scope.selecteditem.ItemMultiMRPId + "&WarehouseId=" + $scope.selecteditem.WarehouseId + "&stockType=" + stockType;
        $http.get(url).success(function (data) {
           
            $scope.GetStockBatchMastersList = data;
            console.log($scope.GetStockBatchMastersList,"$scope.GetStockBatchMastersList")
           
            var url = serviceBase + "api/CurrentStock/GetAPPIteMultiMrpWhWise?ItemMultiMRPId=" + $scope.selecteditem.ItemMultiMRPId + "&WarehouseId=" + $scope.selecteditem.WarehouseId + "&stockType=" + stockType;
            $http.get(url).success(function (appData) {
                for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                    if (appData != null && appData.APP > 0) {
                        $scope.isUnitPrice = true;
                        $scope.isMRPPrice = false;
                        $scope.GetStockBatchMastersList[i].UnitPrice = appData.APP;
                        $scope.GetStockBatchMastersList[i].SellingPrice = appData.APP;
                    } else if ($scope.selecteditem.UnitPrice > 0) {
                        $scope.isUnitPrice = false;
                        $scope.isMRPPrice = false;
                        $scope.GetStockBatchMastersList[i].UnitPrice = $scope.selecteditem.UnitPrice;
                        $scope.GetStockBatchMastersList[i].SellingPrice = $scope.selecteditem.DefaultUnitPrice ? $scope.selecteditem.DefaultUnitPrice : $scope.selecteditem.UnitPrice;
                    } else {
                        $scope.isUnitPrice = false;
                        $scope.isMRPPrice = true;
                        $scope.GetStockBatchMastersList[i].UnitPrice = $scope.selecteditem.MRP;
                        $scope.GetStockBatchMastersList[i].SellingPrice = $scope.selecteditem.MRP;
                    }
                }
                
            });
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.selecteditem.UnitPrice == 0) {
                   
                } else {
                    $scope.isUnitPrice = false;
                    $scope.GetStockBatchMastersList[i].UnitPrice = $scope.selecteditem.UnitPrice;
                }   
            }
            $scope.AmountCalculation($scope.selecteditem);
        });
    };

    $scope.TotalAmount = 0.0;
    $scope.AmountCalculation = function (data, BatchCode) {
       
        if (data.DamageInventory != 0 && data.DamageInventory != null) {

            console.log("Total amount" + $scope.TotalAmount);
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.GetStockBatchMastersList[i].BatchCode == BatchCode) {
                    //if () {
                        $scope.GetStockBatchMastersList[i].UnitPrice = data.UnitPrice;
                        $scope.GetStockBatchMastersList[i].TotalAmount = parseFloat(data.DamageInventory * data.UnitPrice).toFixed(2);//(data.DamageInventory * data.UnitPrice);
                    //}
                }
            }
        }
        else {
            $scope.TotalAmount = (data.DamageInventory * data.UnitPrice);
            console.log("Total amount" + $scope.TotalAmount);
        }


    }

    $scope.DOdata = [];
    $scope.itemdata = [];
    $scope.isValid = false;
    $scope.isAddData = false;
    $scope.checkqty = function (StockBatchMasterId, qty, noOfqty, stockbatch) {
       
        $scope.selectedStockBatch = stockbatch;
        if (qty < noOfqty) {
            $scope.selectStockBatchMasterId = Number(StockBatchMasterId);
            for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
               
                if ($scope.GetStockBatchMastersList.length != null) {
                    if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                       
                        $scope.GetStockBatchMastersList[c].DamageInventory = '';
                       
                        alert('Inventory cannot be greater then Qty!!');
                        return false;
                    }
                }
            }
        }
    }
    $scope.AddData = function (item, CustomerId) {
        
        //$scope.DOdata = [];
        console.log($scope.itemss,"$scope.itemss")
     
        $scope.isAddData = true;
        $scope.selecteditem;
        if ($scope.GetStockBatchMastersList.length == 0) {
            alert('Please select batch code!!');
            return false;
        }

        if ($scope.OrderType == "NR" && ($scope.reasontype == "Other" || $scope.reasontype == "" || $scope.reasontype == undefined) && ($scope.reasonmessage == undefined || $scope.reasonmessage == "" || $scope.reasonmessage == null)) {
            $scope.isValid = false;
            alert("Please Specify Reason");
            return;
        }

        for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
            if ($scope.GetStockBatchMastersList[i].check == true) {
                //if (ManualReason == null) {
                //    alert('Please Enter Manual Reason !!');
                //    return false;
                //}
                if ($scope.selecteditem.DamageInventory < $scope.GetStockBatchMastersList[i].DamageInventory) {
                    alert('Quantity must be less than Available Quantity ');
                    return false;
                }
                if ($scope.GetStockBatchMastersList[i].Qty < $scope.GetStockBatchMastersList[i].DamageInventory) {
                    alert('Quantity must be less than Available Quantity ');
                    return false;
                }
                if ($scope.GetStockBatchMastersList[i].DamageInventory <= 0) {
                    /* alert('Please Enter Quantity in ' + ' ' + $scope.GetStockBatchMastersList[i].BatchCode + ' ' + ' BatchCode');*/
                    alert('Please Enter Quantity!!');
                    return false;
                }
            }
        }
       //if (item.UnitPrice <= 0 || item.UnitPrice == null) {
       //     alert('Please enter unit price greater than Zero ');
       //     return false;
       // }
       
        /* $scope.DOdata = [];*/
            if ($scope.DOdata.length > 0) {
                for (var k = 0; k < $scope.DOdata.length; k++) {
                    if ($scope.DOdata[k].StockBatchMasterId == $scope.selectedStockBatch.StockBatchMasterId) {
                        alert("This Item is already Exist Please Select Another Item!");
                        $scope.isExist = true;
                        return false;
                    }
                }
            }
        //$scope.DOdata = [];
        for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {          
            if ($scope.GetStockBatchMastersList[i].check == true) {
                $scope.isValid = false;
                $scope.isExist = false;
                var AvQty;
                AvQty = item.DamageInventory;
                item.selectedQty = $scope.GetStockBatchMastersList[i].DamageInventory;
                if (item.selectedQty <= 0 || item.selectedQty == null) {
                    alert('Please enter qty greater than zero ');
                    return false;
                }
                if (item.selectedQty > AvQty) {
                    $scope.isValid = true;
                    alert('Quantity should not more then available quantity.');
                    return false;
                }
                if ($scope.GetStockBatchMastersList[i].UnitPrice <= 0 || $scope.GetStockBatchMastersList[i].UnitPrice == null) {
                    alert('Please enter unit price greater than Zero ');
                    return false;
                }
                if ($scope.GetStockBatchMastersList[i].DamageInventory > 0) {
                    
                    //if (AvQty < $scope.GetStockBatchMastersList[i].DamageInventory) {
                    //    //$scope.selectStockBatchMasterId = Number(StockBatchMasterId);
                    //    for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                    //        if ($scope.GetStockBatchMastersList.length != null) {
                    //            if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.GetStockBatchMastersList[i].StockBatchMasterId) {
                    //                $scope.GetStockBatchMastersList[c].DamageInventory = '';
                    //                alert('Damage Inventory cannot be greater then Qty!!');
                    //                return false;
                    //            }
                    //        }
                    //    }
                    //}
                   
                    if ($scope.isExist == false) {
                        
                        $scope.GetStockBatchMastersList[i].selectedcheck = true;
                        $scope.GetStockBatchMastersList[i].check = false;
                        $scope.DOdata.push({
                            CustomerId: CustomerId,
                            WarehouseId: item.WarehouseId,
                            WarehouseName: item.WarehouseName,
                            DamageStockId: $scope.OrderType == "N" ? item.NonSellableStockId : item.DamageStockId,
                            qty: $scope.GetStockBatchMastersList[i].DamageInventory,
                            ItemName: item.ItemName,
                            ItemNumber: item.ItemNumber,
                            ItemId: item.ItemId,
                            UnitPrice: $scope.GetStockBatchMastersList[i].UnitPrice,
                            TotalAmount: $scope.GetStockBatchMastersList[i].TotalAmount,
                            ABCClassification: item.ABCClassification,
                            BatchCode: $scope.GetStockBatchMastersList[i].BatchCode,
                            StockBatchMasterId: $scope.GetStockBatchMastersList[i].StockBatchMasterId,
                            ItemMultiMRPId: item.ItemMultiMRPId,
                            DefaultUnitPrice: $scope.GetStockBatchMastersList[i].SellingPrice,
                            NonRevenueStockId: $scope.OrderType == "NR" ? item.Id : 0,
                            OrderType: $scope.OrderType,
                            Reason: $scope.reasontype,
                            Comments: $scope.reasonmessage

                        });
                    }
                } else {
                    alert('Please enter qty greater than zero ');
                    return false;
                }

                //for (var c = 0; c < $scope.DamageItemData.length; c++) {
                //    if ($scope.DamageItemData[c].DamageStockId == item.DamageStockId) {
                //        AvQty = $scope.DamageItemData[c].DamageInventory;
                //        break;
                //    }
                //}
                if (item.selectedQty <= 0 || item.selectedQty == null) {
                    alert('Please enter qty greater than zero ');
                    return false;
                }
                if (item.selectedQty > AvQty) {
                    $scope.isValid = true;
                        alert('Quantity should not more then available quantity.');
                        return false;
                    } else {
                    var data = true;
                    $scope.isValid = false;
                        for (var c = 0; c < $scope.DOdata.length; c++) {
                            if ($scope.DOdata[c].DamageStockId == item.DamageStockId) {
                                data = false;
                                break;
                            }
                        }
                        var BatchCode;
                        var getStockBatchMasterId;
                        for (var m = 0; m < $scope.GetStockBatchMastersList.length; m++) {
                            if ($scope.GetStockBatchMastersList[m].DamageStockId = item.DamageStockId) {
                                BatchCode = $scope.GetStockBatchMastersList[m].BatchCode;
                                getStockBatchMasterId = $scope.GetStockBatchMastersList[m].StockBatchMasterId;
                            }
                        }
                        var data = true;
                        for (var c = 0; c < $scope.DOdata.length; c++) {
                             if ($scope.DOdata[c].StockBatchMasterId == $scope.GetStockBatchMastersList[i].StockBatchMasterId) {
                                data = false;
                                break;
                            }
                        }
                        //if (data == true) {
                        //    $scope.DOdata.push({
                        //        CustomerId: CustomerId,
                        //        WarehouseId: item.WarehouseId,
                        //        WarehouseName: item.WarehouseName,
                        //        DamageStockId: item.DamageStockId,
                        //        qty: $scope.GetStockBatchMastersList[i].DamageInventory,
                        //        ItemName: item.ItemName,
                        //        ItemNumber: item.ItemNumber,
                        //        ItemId: item.ItemId,
                        //        UnitPrice: $scope.GetStockBatchMastersList[i].UnitPrice,
                        //        TotalAmount: $scope.GetStockBatchMastersList[i].TotalAmount,
                        //        ABCClassification: item.ABCClassification,
                        //        BatchCode: BatchCode,
                        //        StockBatchMasterId: getStockBatchMasterId,
                        //    });
                        //    item.Noofset = "";
                        //    item.PurchaseMinOrderQty = "";
                        //    item.ItemId = "";
                        //}
                        //else {
                        //    alert("Item is Already Added");
                        //    item.Noofset = "";
                        //    item.PurchaseMinOrderQty = "";
                        //    item.ItemId = "";
                        //}
                    }
                
            }
        }
        if ($scope.DOdata.length == 0) {
            alert('Please select batch code!!');
            return false;
        }
    }


    $scope.Searchsave = function (CustomerId) {
        
        if ($scope.DOdata.length == 0) {
            alert('Please Add batch code!!');
            return false;
        }
        if ($scope.OrderType == "NR" && ($scope.reasontype == "Other" || $scope.reasontype == "" || $scope.reasontype == undefined) && ($scope.reasonmessage == undefined || $scope.reasonmessage == "" || $scope.reasonmessage == null)) {
            $scope.isValid = false;
            alert("Please Specify Reason");
            return;
        }
      
        // else if ($scope.reasonmessage != undefined) {
        //    if ($scope.reasonmessage.length >= 100 && $scope.reasonmessage.length != undefined && $scope.reasonmessage.length !='') {
        //        alert("length should be less than 100 characters");
        //        $scope.isValid = false;
        //        return;

        //    }
        //}
        else if ($scope.OrderType == '' || $scope.OrderType == undefined) {
            alert("Please select OrderType");
            $scope.isValid = false;
            return;
        }
        var data = $scope.DOdata;
        data.Customerid = CustomerId;

        if (data.length > 0) {
            
            data.forEach(function (val) {
                
                val.isDamageOrder = $scope.OrderType == "NR" ? false : $scope.OrderType == "N" ? true : false ;
            });
        }
        

        if ($scope.DOdata && $scope.DOdata.length > 0) {
            
            console.log($scope.DOdata)
            var url = serviceBase + "api/damageorder/createDS";
            $http.post(url, data).success(function (result) {
                console.log("Error Got Here");
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
                    if ($scope.OrderType == "N") {
                        alert(' Successfully created Non Sellable order id : ' + result.DamageOrderId);
                    } else if ($scope.OrderType == "D") {
                        alert('Successfully created Damage order id : ' + result.DamageOrderId);
                    } else if ($scope.OrderType == "NR") {
                        alert('Successfully created Non Revenue order id : ' + result.DamageOrderId);
                    }
                    window.location.reload()
                }
            })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        } else {
            if ($scope.DOdata.length <= 0) {
                alert(" Please Select Atleast one BatchCode");
            } else {
                alert(" Please Enter Manual Reason");
            }

        }
        
    };


    $scope.remove = function (item) {
        
        if ($scope.DOdata.length == 1) {
            $scope.isAddData = false;
        }        
        $('#myOverlay').show();
        var index = $scope.DOdata.indexOf(item);
        if ($window.confirm("Please confirm?")) {
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.GetStockBatchMastersList[i].StockBatchMasterId == item.StockBatchMasterId) {
                    $scope.GetStockBatchMastersList[i].Noofset = "";
                    $scope.GetStockBatchMastersList[i].check = false;
                    $scope.GetStockBatchMastersList[i].selectedcheck = false;
                    $scope.GetStockBatchMastersList[i].DamageInventory = null;
                }
            }
            var cal = item.UnitPrice * item.qty;
            $scope.DOdata.splice(index, 1);
            $scope.total = $scope.total - cal;
            console.log('TotalAmt' + $scope.total);
           
            alert("Item is Deleted from List");
        }
       
        $('#myOverlay').hide();
    };

    $scope.cancel = function () {
        location.reload();
    };

}]);
