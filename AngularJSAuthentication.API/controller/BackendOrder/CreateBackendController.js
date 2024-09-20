'use strict';

app.controller('CreateBackendController', ['$scope', 'WarehouseService', "customerService", "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', 'DeliveryService', function ($scope, WarehouseService, customerService, $filter, $http, ngTableParams, $modal, FileUploader, DeliveryService) {

    $scope.serviceBase = serviceBase;
    /*$scope.baseurl = baseurl;*/
    $scope.customertype = null;
    $scope.Searchkey = '';
    $scope.warehouse = [];
    $scope.warehouseId = '';
    $scope.customer;
    $scope.selectedCustomerName = "";
    $scope.stateName = '';
    $scope.TotalAmount = 0.0;
    $scope.itemList = [];
    $scope.keyword = '';
    $scope.selecteditem = null;
    $scope.shoppingCart = {};
    $scope.IsCustomize = false;
    $scope.IsSelected = false;
    $scope.Batchqtydata = [];
    $scope.itmbackendbtches = [];
    if ($scope.IsCustomize === false) {
        $scope.Searchkey = '';
    }

    $scope.IsWareHouse = false;
    $scope.Warehouseid = [];
    $scope.wrshse = function () {
        var url = serviceBase + 'api/DeliveyMapping/GetStoreWarehouse';
        $http.get(url)
            .success(function (data) {
                if (data.length == 1) {
                    $scope.warehouse = data;
                    $scope.warehouseId = data[0].value;
                }
                else {
                    $scope.warehouse = data;
                }
                console.log("$scope.warehouse ", $scope.warehouse)
            });
    };
    $scope.wrshse();
    $scope.IsCustomerAddbtn = false;
    $scope.IsWarehouseMisMatchbtn = false;
    $scope.BillDiscountDetails = [];
    $scope.BillDiscountOfferAmount = 0;
    $scope.OfferWalletConfig = 0;
    $scope.OfferWalletValue = 0;
    $scope.RetailerWalletConfig = 0;
    $scope.ConsumerWalletConfig = 0;
    //$scope.open = function () {
    //    var modalInstance;

    //    modalInstance = $modal.open(
    //        {
    //            templateUrl: "CreateBackendOrder.html",
    //            controller: "CreateBackendController", resolve: { poapproval: function () { return $scope.items } }
    //        });
    //    modalInstance.result.then(function (selectedItem) {
    //    },
    //        function () {
    //            console.log("Cancel Condintion");
    //        })
    //}
    //$scope.ok = function () {
    //    var modalInstance

    //    modalInstance.dismiss();
    //},

    $scope.getCustData = function (WarehouseId, key) {

        $scope.warehouseId = WarehouseId;
    }

    $scope.Deliveryboy = function (WarehouseId) {

        //DeliveryService.getWarehousebyId(WarehouseId).then(function (results) {
        //    $scope.DBoys = results.data;
        //    console.log($scope.DBoys);
        //}, function (error) {
        //});

        var url = serviceBase + 'api/BackendOrder/GetDelieveryBoy/' + WarehouseId;

        $http.get(url)
            .success(function (data) {

                if (data) {
                    $scope.DBoys = data;

                }

            });
    }


    //$scope.getItemList = function () {
    //    console.log('log is: ', $scope.keyword);
    //    if ($scope.keyword && $scope.keyword.length > 2) {
    //        // ;
    //        var url = serviceBase + 'api/BackendOrder/GetItemList/' + $scope.keyword + '/' + $scope.warehouseId;
    //        $http.get(url).success(function (data) {

    //            $scope.itemList = data;
    //            console.log('$scope.itemList', $scope.itemList);
    //        });
    //    }

    //}


    $scope.Close = function () {
        $scope.checkList = undefined;
        $scope.PopupKeyword = undefined;
        $scope.CheckItemList = [];
    }
    $scope.checkPrice = function () {
        if ($scope.PopupKeyword == undefined || $scope.PopupKeyword == "") {
            alert("Please search Item"); return false;
        }
        if ($scope.PopupKeyword && $scope.PopupKeyword.length > 2) {

            var url = serviceBase + 'api/BackendOrder/GetItemList?keyword=' + $scope.PopupKeyword + '&warehouseId=' + $scope.warehouseId + '&MobileNo=' + null;
            $http.get(url).success(function (data) {
                $scope.checkList = data;
                console.log("$scope.checkList", $scope.checkList)
                if ($scope.checkList.length > 0) {
                    if ($scope.checkList.length == 1) {
                        $scope.itmss = $scope.checkList[0].ItemId;
                        $scope.itmm = $scope.checkList[0];
                        $scope.CheckItemList = [];
                        $scope.CheckItemList.push($scope.itmm);
                        console.log($scope.CheckItemList[0].MoqItems)
                    }
                }
                else {
                    $scope.checkList = undefined;
                }
            })
        }
        else {
            $scope.checkList = undefined;
            $scope.itmm = undefined;
            $scope.CheckItemList = [];
        }
    }
    $scope.CheckItemList = [];
    $scope.onSelectCheck = function (itmm) {
        $scope.CheckItemList = [];
        itmm = JSON.parse(itmm)
        $scope.CheckItemList.push(itmm);
    }
    $scope.isseenornot = true;
    $scope.lst = [];
    $scope.AddedItembatchstock = [];
    $scope.filterBatchData = [];
    $scope.itms = 0;
    $scope.batchid = 0;
    $scope.itemmoqs = [];
    $scope.batchqty = 1; 
    $scope.getItemList = function () {
        $scope.customercheck();
        $scope.itemList = [];

        console.log("Modal Barcode" + $scope.keyword);

        if ($scope.shoppingCart.Customerphonenum == undefined || $scope.shoppingCart.Customerphonenum == 0 || $scope.shoppingCart.Customerphonenum.length < 10 || $scope.shoppingCart.Customerphonenum == "") {
            alert("Please Select Mobile Number"); return false;
        }
        if ($scope.warehouseId == undefined || $scope.warehouseId == "") {
            alert("Please Select Warehouse."); return false;
        }
        else if ($scope.keyword == "" || $scope.keyword == undefined) {
            alert("Please search Item."); return false;
        }
        $scope.Batchqtydata = []
        $scope.isseenornot = true;
        console.log('log is: ', $scope.keyword);
        $scope.itemList = [];
        $scope.Batch = [];
        $scope.itms = 0;
        $scope.batchid = 0;
        setTimeout(function () {
            if ($scope.keyword && $scope.keyword.length > 2) {
                var url = serviceBase + 'api/BackendOrder/GetItemList?keyword=' + $scope.keyword + '&warehouseId=' + $scope.warehouseId + '&MobileNo=' + $scope.shoppingCart.Customerphonenum;
                $http.get(url).success(function (data) {

                    $scope.itemList = data;
                    $scope.Batchqtydata = []
                    $scope.Batch = [];
                    
                    console.log('$scope.itemList', $scope.itemList);
                    if ($scope.itemList.length == 1) {
                        debugger
                        $scope.itms = $scope.itemList[0].ItemId;
                        $scope.itm = $scope.itemList[0]
                        $scope.items1.push($scope.itm)
                        $scope.Batchqtydata = $scope.itemList[0].BackendItemBatchs
                        if ($scope.Batchqtydata.length == 1) {
                            $scope.batchid = $scope.Batchqtydata[0].StockBatchMasterId;
                            $scope.Batch = JSON.stringify($scope.Batchqtydata[0])
                            $scope.batchqty = 1
                        }
                    }

                    console.log($scope.AddItem)
                    if ($scope.AddItem.length > 0) {

                        $scope.AddedItembatchstock = [];
                        angular.forEach($scope.AddItem, function (item, key) {
                            angular.forEach(item.batchdetails, function (bacth, key) {
                                $scope.AddedItembatchstock.push(bacth)
                            })
                        })

                        angular.forEach($scope.itemList, function (value, key) {
                            angular.forEach(value.BackendItemBatchs, function (bvalue, key) {
                                $scope.filterBatchData = $filter('filter')($scope.AddedItembatchstock, function (batchstock) {
                                    return batchstock.StockBatchMasterId == bvalue.StockBatchMasterId;
                                });

                                if ($scope.filterBatchData.length > 0) {
                                    bvalue.Remaingbatchqty = bvalue.Batchqty - $scope.filterBatchData[0].qty
                                }
                                else {
                                    bvalue.Remaingbatchqty = bvalue.Batchqty
                                }
                            })
                        })
                    }

                    else {
                        angular.forEach($scope.itemList, function (value, key) {
                            angular.forEach(value.BackendItemBatchs, function (bvalue, key) {
                                bvalue.Remaingbatchqty = bvalue.Batchqty
                            })
                        })
                    }
                });
                console.log('$scope.itemList', $scope.itemList);
            }
        }, 500);
    }
    $scope.Remaingbatchqty = 0;
    $scope.onBatchchnage = function (Batch) {

        Batch = JSON.parse(Batch)
        $scope.batch1.push(Batch)
        $scope.Remaingbatchqty = Batch.Remaingbatchqty
        console.log("itemList", $scope.itemList)
        $scope.batchqty = 1
    }
    $scope.onSelectItem = function (itm) {
        debugger
        if (itm) {
            $scope.Batch = [];
            itm = JSON.parse(itm)
            $scope.items1.push(itm)
            $scope.Batchqtydata = [];
            $scope.isseenornot = false;
            $scope.itm =itm;
            $scope.itemmoqs = itm.MoqItems;
            $scope.Batchqtydata = itm.BackendItemBatchs;
            if ($scope.Batchqtydata.length == 1) {
                $scope.batchid = $scope.Batchqtydata[0].StockBatchMasterId;
                $scope.Batch = JSON.stringify($scope.Batchqtydata[0])
                $scope.batchqty = 1
            }
            console.log($scope.Batchqtydata, "dropdown")
            
        }
    }

    $scope.tempamount = 0;
    $scope.Deleteitem = function (d) {

        $scope.AddItem.forEach(x => {

            if (x.ItemId == d.ItemId) {

                $scope.tempamount = x.qty * x.UnitPrice;
                $scope.AddItem = $scope.AddItem.filter(y => y.ItemId != d.ItemId)
            }
        })

        $scope.totalamount = $scope.totalamount - $scope.tempamount
        if ($scope.totalamount == 'NaN') {
            $scope.totalamount = 0;
        }

        $scope.itemList.forEach(x => {

            if (x.ItemId == d.ItemId) {

                x.BackendItemBatchs.forEach(y => {

                    y.Remaingbatchqty = y.Batchqty;

                })
            }
        })
        console.log($scope.itemList)
        $scope.appliedOffer = [];
        $scope.BillDiscountOfferAmount = 0;
    }

    $scope.batch1 = [];
    $scope.items1 = [];
    $scope.itmdetails = [];
    $scope.Edititem = function (d) {
        debugger
        var id = d.ItemId;
        d.batchdetails.forEach(x => {
            debugger
            console.log($scope.itmbackendbtches)
            $scope.itmbackendbtches.forEach(y => {
                if (y.ItemId == d.ItemId) {
                    var selectedbatch = y.BackendItemBatchs.filter(y => { return y.StockBatchMasterId == x.StockBatchMasterId });
                    x.batch = selectedbatch
                    x.item = $scope.items1.filter(y => { return id == y.ItemId })[0]
                }
            })
            
        })
        $scope.itmdetails = d.batchdetails;
        console.log($scope.itmdetails, "ddddddddddd")
    }


    $scope.PlusIcon = function (Batch, batchqty, itm, i) {
        debugger
        Batch = Batch[0];
        i.qty = batchqty + 1;
        if (i.qty > Batch.Batchqty) {
            alert("Quantity Can't be greater than Batch quantity");
            i.qty = batchqty;
            return false;
        }
        Batch = JSON.stringify(Batch);
        batchqty = 1;
        console.log($scope.customertype)
        if (batchqty == undefined || batchqty == 0) {
            alert("Please Enter Qty"); return false;
        }
        Batch = JSON.parse(Batch)
        if ($scope.customertype != 'Consumer') {
            if (itm.MoqItems.length == 0) {
                alert("Please Fill at Least One Moq."); return false;
            }
            $scope.itemmoq = itm.MoqItems;
        }


        itm.BackendItemBatchs.forEach(y => {

            debugger
            if (y.StockBatchMasterId == Batch.StockBatchMasterId) {
                $scope.Remaingbatchqtydata = y.Remaingbatchqty

            }
        })

        if ($scope.Remaingbatchqtydata >= batchqty) {

            if ($scope.customertype == 'Consumer') {
                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == itm.ItemId;
                });


                if ($scope.filterData.length > 0) {
                    debugger
                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty
                            
                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Addbatchdetails.push({
                                            StockBatchMasterId: Batch.StockBatchMasterId,
                                            qty: $scope.BchQty
                                        })
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {

                    $scope.Addbatchdetails = [];
                    angular.forEach($scope.Batchqtydata, function (value, key) {

                        if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                            $scope.Addbatchdetails.push({
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty
                            })
                        }
                    })

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })
                    console.log()
                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.itm.UnitPrice,
                        batchdetails: $scope.Addbatchdetails
                    })
                    
                }
                debugger
                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = 0;
                angular.forEach($scope.AddItem, function (i, key) {
                    $scope.totalamount = $scope.totalamount + (i.qty * i.UnitPrice)
                })
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }
            else {

                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == itm.ItemId;
                });

                $scope.ischecked = false;
                $scope.lastmoqqty = 0;
                if ($scope.filterData.length > 0) {


                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty;
                            //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                            //angular.forEach($scope.itemmoq, function (moq, keys) {
                            //       
                            //    if ($scope.ischecked == false) {
                            //        if (moq.MinOrderQty >= value.qty) {
                            //            value.UnitPrice = moq.UnitPrice;

                            //            $scope.ischecked = true;
                            //        }
                            //        $scope.lastmoqqty = moq.UnitPrice;
                            //    }
                            //})
                            //if ($scope.ischecked == false) {
                            //    value.UnitPrice = $scope.lastmoqqty
                            //}
                            value.UnitPrice = $scope.itemmoq[0].UnitPrice;
                            angular.forEach($scope.itemmoq, function (moq, key) {

                                //if (moq.MinOrderQty >= value.qty && $scope.ischecked == false) {
                                //    value.UnitPrice = moq.UnitPrice;
                                //    $scope.ischecked = true;
                                //}
                                //else {
                                //    $scope.temp = moq.UnitPrice;
                                //}
                                if (value.qty >= moq.MinOrderQty) {
                                    value.UnitPrice = moq.UnitPrice;
                                }
                            })

                            //value.UnitPrice = $scope.unitprice;

                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Addbatchdetails.push({
                                            StockBatchMasterId: Batch.StockBatchMasterId,
                                            qty: $scope.BchQty
                                        })
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {
                    $scope.unitprice = 0;

                    $scope.Addbatchdetails = [];
                    angular.forEach($scope.Batchqtydata, function (value, key) {

                        if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                            $scope.Addbatchdetails.push({
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty
                            })
                        }
                    })

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //       
                    //    if ($scope.ischecked == false) {
                    //        if (moq.MinOrderQty >= batchqty) {
                    //            $scope.unitprice = moq.UnitPrice;
                    //            $scope.ischecked = true;
                    //        }
                    //        $scope.lastmoqqty = moq.UnitPrice;
                    //    }

                    //})
                    //if ($scope.ischecked == false) {
                    //    $scope.unitprice = $scope.lastmoqqty;
                    //}
                    $scope.unitprice = $scope.itemmoq[0].UnitPrice;
                    angular.forEach($scope.itemmoq, function (moq, key) {
                        //if (moq.MinOrderQty >= batchqty && $scope.ischecked == false) {
                        //    $scope.unitprice = moq.UnitPrice;
                        //    $scope.ischecked = true;
                        //}
                        //else {
                        //    $scope.temp = moq.UnitPrice;
                        //}
                        if (batchqty >= moq.MinOrderQty) {
                            $scope.unitprice = moq.UnitPrice;
                        }
                    })
                    //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //    if (moq.MinOrderQty >= value.qty) {
                    //        value.UnitPrice = moq.UnitPrice;
                    //        return;
                    //    }
                    //})
                    //value.UnitPrice = $scope.unitprice;

                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.unitprice,
                        batchdetails: $scope.Addbatchdetails
                    })
                }
                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = 0;
                angular.forEach($scope.AddItem, function (i, key) {
                    $scope.totalamount = $scope.totalamount + (i.qty * i.UnitPrice)
                })
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }

        }
        else {
            alert("Qty is greater then Remaing Batch Qty")
            $scope.batchqty = 0;
            //$scope.keyword = undefined;
        }
        //           ;
        $scope.itemList = [];
        $scope.Batchqtydata = [];
        $scope.batchqty = 0;
        $scope.keyword = undefined;
        $scope.freebiesupload();
        $scope.appliedOffer = [];
        $scope.BillDiscountOfferAmount = 0;
    }
    $scope.minusIcon = function (Batch, batchqty, itm, i) {
        debugger
        Batch = JSON.stringify(Batch);
        i.qty = batchqty - 1;
        if (i.qty < 0) {
            i.qty = 0;
            alert("Quantity Can't be negative.");
            return false;
        }
        batchqty = -1;
        console.log($scope.customertype)
        if (batchqty == undefined || batchqty == 0) {
            alert("Please Enter Qty"); return false;
        }
        Batch = JSON.parse(Batch)

        if ($scope.customertype != 'Consumer') {
            if (itm.MoqItems.length == 0) {
                alert("Please Fill at Least One Moq."); return false;
            }
            $scope.itemmoq = itm.MoqItems;
        }


        itm.BackendItemBatchs.forEach(y => {


            if (y.StockBatchMasterId == Batch.StockBatchMasterId) {
                $scope.Remaingbatchqtydata = y.Remaingbatchqty

            }
        })

        if ($scope.Remaingbatchqtydata >= batchqty) {

            if ($scope.customertype == 'Consumer') {
                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == itm.ItemId;
                });


                if ($scope.filterData.length > 0) {

                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty
                            if (value.qty < 0) { value.qty = 0; return false }
                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Addbatchdetails.push({
                                            StockBatchMasterId: Batch.StockBatchMasterId,
                                            qty: $scope.BchQty
                                        })
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {

                    $scope.Addbatchdetails = [];
                    angular.forEach($scope.Batchqtydata, function (value, key) {

                        if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                            $scope.Addbatchdetails.push({
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty
                            })
                        }
                    })

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })
                    console.log()
                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.itm.UnitPrice,
                        batchdetails: $scope.Addbatchdetails
                    })
                }
                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = 0;
                angular.forEach($scope.AddItem, function (i, key) {
                    $scope.totalamount = $scope.totalamount + (i.qty * i.UnitPrice)
                })
                /*$scope.totalamount = $scope.totalamount + (batchqty * $scope.itm.UnitPrice);*/
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }
            else {

                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == itm.ItemId;
                });

                $scope.ischecked = false;
                $scope.lastmoqqty = 0;
                if ($scope.filterData.length > 0) {


                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty;
                            if (value.qty < 0) { value.qty = 0; return false }
                            //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                            //angular.forEach($scope.itemmoq, function (moq, keys) {
                            //       
                            //    if ($scope.ischecked == false) {
                            //        if (moq.MinOrderQty >= value.qty) {
                            //            value.UnitPrice = moq.UnitPrice;

                            //            $scope.ischecked = true;
                            //        }
                            //        $scope.lastmoqqty = moq.UnitPrice;
                            //    }
                            //})
                            //if ($scope.ischecked == false) {
                            //    value.UnitPrice = $scope.lastmoqqty
                            //}
                            value.UnitPrice = $scope.itemmoq[0].UnitPrice;
                            angular.forEach($scope.itemmoq, function (moq, key) {

                                //if (moq.MinOrderQty >= value.qty && $scope.ischecked == false) {
                                //    value.UnitPrice = moq.UnitPrice;
                                //    $scope.ischecked = true;
                                //}
                                //else {
                                //    $scope.temp = moq.UnitPrice;
                                //}
                                if (value.qty >= moq.MinOrderQty) {
                                    value.UnitPrice = moq.UnitPrice;
                                }
                            })

                            //value.UnitPrice = $scope.unitprice;

                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Addbatchdetails.push({
                                            StockBatchMasterId: Batch.StockBatchMasterId,
                                            qty: $scope.BchQty
                                        })
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {
                    $scope.unitprice = 0;

                    $scope.Addbatchdetails = [];
                    angular.forEach($scope.Batchqtydata, function (value, key) {

                        if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                            $scope.Addbatchdetails.push({
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty
                            })
                        }
                    })

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //       
                    //    if ($scope.ischecked == false) {
                    //        if (moq.MinOrderQty >= batchqty) {
                    //            $scope.unitprice = moq.UnitPrice;
                    //            $scope.ischecked = true;
                    //        }
                    //        $scope.lastmoqqty = moq.UnitPrice;
                    //    }

                    //})
                    //if ($scope.ischecked == false) {
                    //    $scope.unitprice = $scope.lastmoqqty;
                    //}
                    $scope.unitprice = $scope.itemmoq[0].UnitPrice;
                    angular.forEach($scope.itemmoq, function (moq, key) {
                        //if (moq.MinOrderQty >= batchqty && $scope.ischecked == false) {
                        //    $scope.unitprice = moq.UnitPrice;
                        //    $scope.ischecked = true;
                        //}
                        //else {
                        //    $scope.temp = moq.UnitPrice;
                        //}
                        if (batchqty >= moq.MinOrderQty) {
                            $scope.unitprice = moq.UnitPrice;
                        }
                    })
                    //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //    if (moq.MinOrderQty >= value.qty) {
                    //        value.UnitPrice = moq.UnitPrice;
                    //        return;
                    //    }
                    //})
                    //value.UnitPrice = $scope.unitprice;

                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.unitprice,
                        batchdetails: $scope.Addbatchdetails
                    })
                }
                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = 0;
                angular.forEach($scope.AddItem, function (i, key) {
                    $scope.totalamount = $scope.totalamount + (i.qty * i.UnitPrice)
                })
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }

        }
        else {
            alert("Qty is greater then Remaing Batch Qty")
            $scope.batchqty = 0;
            //$scope.keyword = undefined;
        }
        //           ;
        $scope.itemList = [];
        $scope.Batchqtydata = [];
        $scope.batchqty = 0;
        $scope.keyword = undefined;
        $scope.freebiesupload();
        $scope.appliedOffer = [];
        $scope.BillDiscountOfferAmount = 0;
    }
    $scope.postlist = [];
    $scope.selectedbatchlist = [];
    $scope.AddItem = [];
    $scope.Addbatchdetails = [];
    $scope.filterData = [];
    $scope.filteredstock = [];
    $scope.totalamount = 0;
    $scope.Remaingbatchqtydata = 0;
    $scope.itemmoq = [];
    $scope.FreebiesitemList = [];
    $scope.onadd = function (Batch, batchqty, itm) {
        debugger
        $scope.customercheck();
        if (itm.itemDataDCs.length > 0) {
            if ($scope.FreebiesitemList.length == 0) {
                angular.forEach(itm.itemDataDCs, function (value, key) {
                    $scope.FreebiesitemList.push({
                        ItemId: value.ItemId,
                        Itemname: value.itemname,
                        MinOrderQty: value.OfferMinimumQty,
                        OfferFreeItemName: value.OfferFreeItemName,
                        OfferFreeItemUrl: value.OfferFreeItemImage,
                        OfferWalletPoint: value.OfferWalletPoint,
                        OfferFreeitemQty: value.OfferFreeItemQuantity,
                        OfferOn: value.OfferFreeItemQuantity > 0 ? 'Item' : 'Walletpoint',
                        OfferQty: 0,
                        OfferFreeItemId: value.OfferFreeItemId
                    })
                })

            }
            else {
                angular.forEach(itm.itemDataDCs, function (value, key) {

                    angular.forEach($scope.FreebiesitemList, function (freebies, key) {

                        if (freebies.ItemId == value.ItemId) {

                        }
                        else {
                            $scope.FreebiesitemList.push({
                                ItemId: value.ItemId,
                                Itemname: value.itemname,
                                MinOrderQty: value.OfferMinimumQty,
                                OfferFreeItemName: value.OfferFreeItemName,
                                OfferFreeItemUrl: value.OfferFreeItemImage,
                                OfferWalletPoint: value.OfferWalletPoint,
                                OfferFreeitemQty: value.OfferFreeItemQuantity,
                                OfferOn: value.OfferFreeItemQuantity > 0 ? 'Item' : 'Walletpoint',
                                OfferQty: 0,
                                OfferFreeItemId: value.OfferFreeItemId
                            })
                        }
                    })
                })
            }



            console.log($scope.FreebiesitemList)
        }
        console.log($scope.customertype)
        if (batchqty == undefined || batchqty == 0) {
            alert("Please Enter Qty"); return false;
        }
        Batch = JSON.parse(Batch)

        if ($scope.customertype != 'Consumer') {
            if (itm.MoqItems.length == 0) {
                alert("Please Fill at Least One Moq."); return false;
            }
            $scope.itemmoq = itm.MoqItems;
        }
        itm.BackendItemBatchs.forEach(y => {
            if (y.StockBatchMasterId == Batch.StockBatchMasterId) {
                $scope.Remaingbatchqtydata = y.Remaingbatchqty
            }
        })

        if ($scope.Remaingbatchqtydata >= batchqty) {
            if ($scope.customertype == 'Consumer') {
                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == $scope.itm.ItemId;
                });


                if ($scope.filterData.length > 0) {

                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty
                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Batchqtydata.forEach(y => {
                                            if (y.StockBatchMasterId == Batch.StockBatchMasterId) {
                                                let a = {
                                                    StockBatchMasterId: Batch.StockBatchMasterId,
                                                    qty: $scope.BchQty,
                                                    BatchCode: Batch.BatchCode
                                                }
                                                $scope.Addbatchdetails.push(a)
                                            } else {
                                                if (y.StockBatchMasterId != Batch.StockBatchMasterId) {
                                                    let a = {
                                                        StockBatchMasterId: Batch.StockBatchMasterId,
                                                        qty: 0,
                                                        BatchCode: Batch.BatchCode
                                                    }
                                                    $scope.Addbatchdetails.push(a)
                                                }
                                            }
                                        })

                                        //    $scope.Addbatchdetails.push({
                                        //        StockBatchMasterId: Batch.StockBatchMasterId,
                                        //        qty: $scope.BchQty,
                                        //        BatchCode: Batch.BatchCode
                                        //})
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {
                    $scope.Addbatchdetails = [];
                    ;
                    $scope.Batchqtydata.forEach(y => {
                        if (y.StockBatchMasterId == Batch.StockBatchMasterId) {
                            let a = {
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty,
                                BatchCode: Batch.BatchCode
                            }
                            $scope.Addbatchdetails.push(a)
                        } else {
                            if (y.StockBatchMasterId != Batch.StockBatchMasterId) {
                                let a = {
                                    StockBatchMasterId: y.StockBatchMasterId,
                                    qty: 0,
                                    BatchCode: y.BatchCode
                                }
                                $scope.Addbatchdetails.push(a)
                            }
                        }
                    })
                    //angular.forEach($scope.Batchqtydata, function (value, key) {

                    //    if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                    //        $scope.Addbatchdetails.push({
                    //            StockBatchMasterId: Batch.StockBatchMasterId,
                    //            qty: $scope.BchQty,
                    //            BatchCode: Batch.BatchCode
                    //        })
                    //    }
                    //})

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })
                    console.log()
                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.itm.UnitPrice,
                        batchdetails: $scope.Addbatchdetails,
                    })

                }

                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = $scope.totalamount + (batchqty * $scope.itm.UnitPrice);
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }
            else {

                $scope.BchQty = batchqty;

                $scope.filterData = $filter('filter')($scope.AddItem, function (value) {
                    return value.ItemId == $scope.itm.ItemId;
                });

                $scope.ischecked = false;
                $scope.lastmoqqty = 0;
                if ($scope.filterData.length > 0) {


                    angular.forEach($scope.AddItem, function (value, key) {
                        if (value.ItemId == $scope.filterData[0].ItemId) {

                            value.qty = value.qty + batchqty;
                            //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                            //angular.forEach($scope.itemmoq, function (moq, keys) {
                            //       
                            //    if ($scope.ischecked == false) {
                            //        if (moq.MinOrderQty >= value.qty) {
                            //            value.UnitPrice = moq.UnitPrice;

                            //            $scope.ischecked = true;
                            //        }
                            //        $scope.lastmoqqty = moq.UnitPrice;
                            //    }
                            //})
                            //if ($scope.ischecked == false) {
                            //    value.UnitPrice = $scope.lastmoqqty
                            //}
                            value.UnitPrice = $scope.itemmoq[0].UnitPrice;
                            angular.forEach($scope.itemmoq, function (moq, key) {

                                //if (moq.MinOrderQty >= value.qty && $scope.ischecked == false) {
                                //    value.UnitPrice = moq.UnitPrice;
                                //    $scope.ischecked = true;
                                //}
                                //else {
                                //    $scope.temp = moq.UnitPrice;
                                //}
                                if (value.qty >= moq.MinOrderQty) {
                                    value.UnitPrice = moq.UnitPrice;
                                }
                            })

                            //value.UnitPrice = $scope.unitprice;

                            angular.forEach($scope.Batchqtydata, function (batchvalue, key) {

                                if (batchvalue.StockBatchMasterId == Batch.StockBatchMasterId) {

                                    $scope.filteredstock = $filter('filter')(value.batchdetails, function (addbatchvalue) {
                                        return addbatchvalue.StockBatchMasterId == Batch.StockBatchMasterId;
                                    });
                                    if ($scope.filteredstock.length > 0) {
                                        ;
                                        $scope.filteredstock[0].qty = $scope.filteredstock[0].qty + $scope.BchQty
                                    }
                                    else {
                                        ;
                                        $scope.Addbatchdetails.push({
                                            StockBatchMasterId: Batch.StockBatchMasterId,
                                            qty: $scope.BchQty,
                                            BatchCode: Batch.BatchCode
                                        })
                                    }
                                }
                            })
                        }
                    })
                    angular.forEach($scope.Batchqtydata, function (itemmvalue, key) {

                        if (itemmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itemmvalue.Remaingbatchqty = itemmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                }
                else {
                    $scope.unitprice = 0;

                    $scope.Addbatchdetails = [];
                    angular.forEach($scope.Batchqtydata, function (value, key) {

                        if (value.StockBatchMasterId == Batch.StockBatchMasterId) {

                            $scope.Addbatchdetails.push({
                                StockBatchMasterId: Batch.StockBatchMasterId,
                                qty: $scope.BchQty,
                                BatchCode: Batch.BatchCode
                            })
                        }
                    })

                    angular.forEach($scope.Batchqtydata, function (itmvalue, key) {

                        if (itmvalue.StockBatchMasterId == Batch.StockBatchMasterId) {
                            itmvalue.Remaingbatchqty = itmvalue.Remaingbatchqty - $scope.BchQty
                        }
                    })

                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //       
                    //    if ($scope.ischecked == false) {
                    //        if (moq.MinOrderQty >= batchqty) {
                    //            $scope.unitprice = moq.UnitPrice;
                    //            $scope.ischecked = true;
                    //        }
                    //        $scope.lastmoqqty = moq.UnitPrice;
                    //    }

                    //})
                    //if ($scope.ischecked == false) {
                    //    $scope.unitprice = $scope.lastmoqqty;
                    //}
                    $scope.unitprice = $scope.itemmoq[0].UnitPrice;
                    angular.forEach($scope.itemmoq, function (moq, key) {
                        //if (moq.MinOrderQty >= batchqty && $scope.ischecked == false) {
                        //    $scope.unitprice = moq.UnitPrice;
                        //    $scope.ischecked = true;
                        //}
                        //else {
                        //    $scope.temp = moq.UnitPrice;
                        //}
                        if (batchqty >= moq.MinOrderQty) {
                            $scope.unitprice = moq.UnitPrice;
                        }
                    })
                    //$scope.unitprice = $scope.unitpricecal($scope.itemmoq, value.qty);
                    //angular.forEach($scope.itemmoq, function (moq, keys) {
                    //    if (moq.MinOrderQty >= value.qty) {
                    //        value.UnitPrice = moq.UnitPrice;
                    //        return;
                    //    }
                    //})
                    //value.UnitPrice = $scope.unitprice;

                    $scope.AddItem.push({
                        ItemId: $scope.itm.ItemId,
                        ItemName: $scope.itm.ItemName,
                        qty: batchqty,
                        WarehouseId: $scope.itm.WarehouseId,
                        UnitPrice: $scope.unitprice,
                        batchdetails: $scope.Addbatchdetails
                    })

                }
                console.log("Batchqtydatareamining", $scope.Batchqtydata)
                console.log("AddItemList", $scope.AddItem);
                $scope.totalamount = 0;
                angular.forEach($scope.AddItem, function (i, key) {
                    $scope.totalamount = $scope.totalamount + (i.qty * i.UnitPrice)
                })
                $scope.batchqty = 0;
                //$scope.keyword = undefined;
            }

        }
        else {
            alert("Qty is greater then Remaing Batch Qty")
            $scope.batchqty = 0;
            //$scope.keyword = undefined;
        }
        //           ;
        debugger
        if ($scope.itmbackendbtches.length > 0) {
            $scope.itmbackendbtches.forEach(y => {
                if (y.ItemId == itm.ItemId) {

                }
                else {
                    $scope.itmbackendbtches.push(itm);
                }
            })
        }
        else {
            $scope.itmbackendbtches.push(itm);
        }
        
        $scope.itemList = [];
        $scope.Batchqtydata = [];
        $scope.batchqty = 0;
        $scope.keyword = undefined;
        $scope.freebiesupload();

    }

    $scope.ee = [];
    $scope.Remaingbatchqtydatas = 0;
    $scope.KeywordValidation = function (k) {
        if (k == "") {
            $scope.itemList = [];
            $scope.Batchqtydata = [];
            $scope.batchqty = undefined;
        }
    }
    $scope.UsedWalletPoint = 0;
    $scope.shoppingCart.UsedWalletPoint = 0;
    $scope.calculatewalletpoint = 0;
    $scope.WalletValidation = function (UsedWalletPoint) {
        //totalamount - shoppingCart.BillDiscount

        if ($scope.customertype == null) {
            alert("Please Select Customer Type");
            return false;
        }

        if ($scope.shoppingCart.BillDiscount == undefined) {
            $scope.shoppingCart.BillDiscount = 0;
        }
        if ($scope.customer.WalletPoint == 0) {
            alert("You have" + 0 + "Wallet Point");
            $scope.shoppingCart.UsedWalletPoint = 0;
            return false;
        }
        if (UsedWalletPoint > $scope.customer.WalletPoint) {
            alert("You can not Enter Greater then " + $scope.customer.WalletPoint + " Wallet Amount");
            $scope.shoppingCart.UsedWalletPoint = undefined;
            return false;
        }

        if ($scope.totalamount - $scope.shoppingCart.BillDiscount - ($scope.BillDiscountOfferAmount) < (UsedWalletPoint / ($scope.customertype == 'Consumer' ? $scope.ConsumerWalletConfig : $scope.RetailerWalletConfig))) {
            alert("Total Discount should not greater then total amount");
            $scope.shoppingCart.UsedWalletPoint = 0;
            return false;
        }
        if ($scope.OfferWalletConfig > 0) {
            debugger
            $scope.calculatewalletpoint = ((($scope.totalamount - $scope.shoppingCart.BillDiscount - ($scope.BillDiscountOfferAmount)) * ($scope.OfferWalletConfig)) / 100) * (($scope.customertype == 'Consumer' ? $scope.ConsumerWalletConfig : $scope.RetailerWalletConfig))
            $scope.calculatewalletpoint = $scope.calculatewalletpoint.toFixed(2);
            if ($scope.calculatewalletpoint < UsedWalletPoint) {
                alert("You can not Enter Greater then " + $scope.calculatewalletpoint + " Wallet Amount");
                $scope.shoppingCart.UsedWalletPoint = 0;
                return false;
            }
        }
        if ($scope.OfferWalletValue > 0) {
            if (UsedWalletPoint > $scope.OfferWalletValue) {
                alert("You can not Enter Greater then " + $scope.OfferWalletValue + " Wallet Amount");
                $scope.shoppingCart.UsedWalletPoint = undefined;
                return false;
            }
        }


        // alert($scope.shoppingCart.UsedWalletPoint)
    }

    $scope.BatchValidation = function (batch, batchqty, itm, e) {
        if (batchqty > 0) {

            if (batch == undefined || batch == "") {
                alert("Please Select At Lease One Batch Code")
                $scope.batchqty = 0;
                return false
            }
            if (e.keyCode != 45 && e.keyCode != 46 && e.keyCode != 43) {
                batch = JSON.parse(batch)
                itm.BackendItemBatchs.forEach(y => {
                    if (y.StockBatchMasterId == batch.StockBatchMasterId) {
                        $scope.Remaingbatchqtydatas = y.Remaingbatchqty
                    }
                })
                if ($scope.Remaingbatchqtydatas >= batchqty) {
                }
                else {
                    alert("Qty is greater then Remaing Batch Qty")
                    $scope.batchqty = 0;
                    return false
                }
            } else {
                event.preventDefault();
            }
        }

    }

    $scope.NumberValidation = function (e) {
        if ((e.keyCode < 48 || e.keyCode > 57) && e.keyCode !== 8 && e.keyCode !== 46) {
            event.preventDefault();
        }
    }
    $scope.PreventMinus = function (e) {
        if (e.keyCode != 45 && e.keyCode != 46 && e.keyCode != 43) {
        }
        else {
            event.preventDefault();
        }
    }


    $scope.amountdecimal = function (e) {
        var charCode = (event.which) ? event.which : event.keyCode;

        if ((charCode < 48 || charCode > 57) && charCode !== 8 && charCode !== 46) {
            event.preventDefault();
        }

        if (charCode === 46 && event.target.value.indexOf('.') !== -1) {
            event.preventDefault();
        }

        var dotIndex = event.target.value.indexOf('.');
        if (dotIndex !== -1 && event.target.value.length - dotIndex > 2) {
            event.preventDefault();
        }
    }

    $scope.IsDist = false;
    $scope.ApplyBillDiscount = function (BillDiscount) {
        if ($scope.AddItem.length == 0) {
            alert("Please Select Atleast One Item"); return false;
        } else {

            if ($scope.shoppingCart.BillDiscount == undefined) {
                $scope.shoppingCart.BillDiscount = BillDiscount
            }
            if ($scope.shoppingCart.BillDiscount != undefined && $scope.shoppingCart.BillDiscount != 0) {
                var url = serviceBase + 'api/BackendOrder/BOBillDiscount?TotalAmount=' + $scope.totalamount + '&discount=' + $scope.shoppingCart.BillDiscount + '&wareid=' + $scope.warehouseId;
                $http.get(url)
                    .success(function (res) {
                        console.log(res);
                        $scope.BOBillDiscountResult = res;
                        if (res.Status) {
                            alert(res.Message)
                        }
                        else {
                            alert(res.Message);
                            return false;
                        }
                    });
            } else {
                alert("Please Enter BillDiscount.");
            }
        }
    }

    $scope.ClearItemList = function () {
        $scope.keyword = undefined;
        $scope.itemList = [];
        $scope.Batchqtydata = [];
        $scope.batchqty = 0;
    }

    $scope.filtitemMaster = function (data) {

        $scope.selecteditem = JSON.parse(data.ItemId);
        $scope.AmountCalculation($scope.selecteditem);
    };

    $scope.TotalAmount = 0.0;
    $scope.AmountCalculation = function (data) {

        var j = Math.floor(data.NoOfSet);
        if (data.NoOfSet != j) {
            alert("You cant add decimal value in No Of Set");
            data.NoOfSetQty = 0;
            return false;
        }
        if (data.NoOfSet > 0 && data.MinOrderQty) {

            if (data.NoOfSet * data.MinOrderQty > data.CurrentInventory) {
                alert("You Can't add Qty :" + data.NoOfSet * data.MinOrderQty + " more than available Qty :" + data.CurrentInventory);
                data.NoOfSetQty = 0;
                return false;
            }

            if (data.NoOfSet != 0 && data.NoOfSet != null && data.NoOfSet == j) {

                $scope.TotalAmount = (data.NoOfSet * data.MinOrderQty * data.EditUnitPrice);
                $scope.TotalAmount = $scope.TotalAmount.toFixed(2)

            }
            else {
                $scope.TotalAmount = (data.NoOfSet * data.MinOrderQty * data.EditUnitPrice);
                $scope.TotalAmount = $scope.TotalAmount.toFixed(2)
                console.log("Total amount" + $scope.TotalAmount);
            }
        }
    }

    $scope.DOdata = [];
    $scope.itemdata = [];
    $scope.Amt = 0.0;

    $scope.amtcheck = function (Disc) {
        if ($scope.customertype == null) {
            alert("Please Select Customer Type");
            return false;
        }
        if (Disc > $scope.totalamount) {
            alert("BillDiscount is not greater then TotalAmount");
            $scope.shoppingCart.BillDiscount = 0;
            return false;
        }
        if ($scope.totalamount - $scope.shoppingCart.BillDiscount < ($scope.shoppingCart.UsedWalletPoint / ($scope.customertype == 'Consumer' ? $scope.ConsumerWalletConfig : $scope.RetailerWalletConfig))) {
            alert("Total Discount should not greater then total amount");
            $scope.shoppingCart.BillDiscount = 0;
            return false;
        }
    }
    $scope.GstData = "";
    $scope.custverifyData = "";
    $scope.EnteredCreditNote = "";
    $scope.Searchsave = function () {
        debugger
        $scope.customercheck();
        if ($scope.shoppingCart.BillDiscount == undefined || $scope.shoppingCart.BillDiscount == "" || $scope.shoppingCart.BillDiscount == 0) {
            $scope.shoppingCart.BillDiscount = 0;
        }
        //if ($scope.shoppingCart.BillDiscount > 0) {
        var url = serviceBase + 'api/BackendOrder/BOBillDiscount?TotalAmount=' + $scope.totalamount + '&discount=' + $scope.shoppingCart.BillDiscount + '&wareid=' + $scope.warehouseId;
        $http.get(url)
            .success(function (res) {
                console.log(res);
                $scope.BOBillDiscountResult = res;
                if (res.Status) {

                    //else {
                    //    alert("Please Enter BillDiscount.");
                    //    return false;
                    //}
                    if ($scope.customertype == null) {
                        alert("Please Select Customer Type");
                        return false;
                    }
                    console.log($scope.BOBillDiscountResult)
                    if ($scope.totalamount > 49999) {
                        alert("You can't place order above ₹50,000 "); return false;
                    }
                    if ($scope.shoppingCart.Customerphonenum == undefined || $scope.shoppingCart.Customerphonenum == "" || $scope.shoppingCart.Customerphonenum.length < 10) {
                        alert("Please Enter Customer phone number"); return false;
                    }
                    //else if ($scope.shoppingCart.CustomerName == undefined || $scope.shoppingCart.CustomerName == "") {
                    //    alert("Please Enter Name"); return false;
                    //}
                    else if ($scope.shoppingCart.ShippingAddress == undefined || $scope.shoppingCart.ShippingAddress == "") {
                        alert("Please Enter Shipping Address"); return false;
                    }

                    else if (($scope.custverifyData.Status == false || $scope.custverifyData.Status == undefined) && ($scope.shoppingCart.RefNo != "" && $scope.shoppingCart.RefNo != null)) {
                        alert("Please Verify Gst Number"); return false;
                    }
                    else if ($scope.AddItem.length == 0) {
                        alert("Please Select Atleast One Item"); return false;
                    }


                    else {
                        if ($scope.shoppingCart.UsedWalletPoint > 0) {
                            $scope.walletP = $scope.shoppingCart.UsedWalletPoint / ($scope.customertype == 'Consumer' ? $scope.ConsumerWalletConfig : $scope.RetailerWalletConfig);
                        }
                        else {
                            $scope.walletP = 0;
                        }
                        $scope.NetAmount = $scope.totalamount - $scope.shoppingCart.BillDiscount - $scope.walletP - $scope.BillDiscountOfferAmount
                        console.log($scope.shoppingCart.BillDiscount)
                        debugger
                        var dataTopost = {
                            itemDetails: $scope.AddItem,
                            CustomerName: $scope.shoppingCart.CustomerName,
                            ShippingAddress: $scope.shoppingCart.ShippingAddress,
                            BillDiscountAmount: $scope.shoppingCart.BillDiscount,
                            MobileNo: $scope.shoppingCart.Customerphonenum,
                            UsedWalletAmount: $scope.walletP,
                            NetAmount: $scope.NetAmount,
                            RefNo: $scope.GstData == undefined ? 0 : $scope.GstData,
                            OfferIds: $scope.appliedOffer,
                            CreditNoteNumber: $scope.EnteredCreditNote
                        }
                        console.log('$scope.EnteredCreditNote', $scope.EnteredCreditNote)
                        var url = serviceBase + "api/BackendOrder/CreateBO"; //"api/damageorder/createDS"  
                        $http.post(url, dataTopost).success(function (result) {
                            console.log(result);
                            if (result.Status) {
                                debugger
                                alert(result.Message + ' - ' + result.Data.OrderId)
                                window.location = "#/BackedOrderInvoice/" + result.Data.OrderId;
                                //window.location = "layout/POS-System/BackedOrderInvoice/" + result.Data.OrderId;
                                //var preURI = saralUIPortal
                                //window.location = preURI + "/layout/POS-System/BackedOrderInvoice/" + result.Data.OrderId;
                            }
                            else {
                                alert(result.Message)
                            }
                        })
                            .error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);

                            })
                        // }

                    }
                    //  $('#myOverlay').hide();
                }
                else {
                    alert(res.Message);
                    return false;
                }
            });
        //}
    }
    $scope.cid = 0;
    $scope.offerList = [];
    $scope.OfferBtn = function () {

        $scope.customercheck();
        if ($scope.shoppingCart.CustomerId > 0) {
            $scope.cid = $scope.shoppingCart.CustomerId;
        }
        else {
            $scope.cid = 0;
        }
        var url = serviceBase + 'api/BackendOrder/GetAllOffer?CustomerId=' + $scope.cid + '&WarehouseId=' + $scope.warehouseId;//  + '&WarehouseId=' + $scope.warehouseId;
        $http.get(url).success(function (data) {

            console.log(data, "data")
            if (data.Status) {
                $scope.offerList = data.offer;
                if ($scope.appliedOffer.length > 0) {
                    for (var data of $scope.offerList) {
                        var abc = $scope.appliedOffer.filter(x => { if (x == data.OfferId) return x });
                        (abc.length > 0) ? data.showRemove = true : data.showRemove = false
                    }
                };
                console.log($scope.offerList)
            }
            else {
                alert(data.Message)
                console.log($scope.offerList)
            }
        })
    }
    $scope.appliedOffer = [];
    $scope.ApplyNowBtn = function (offer) {

        if ($scope.shoppingCart.CustomerId > 0) {
            $scope.cid = $scope.shoppingCart.CustomerId;
        }
        else {
            $scope.cid = 0;
        }
        var dataTopost = {
            WarehouseId: $scope.warehouseId,
            CustomerId: $scope.cid,
            OfferId: offer.OfferId,
            ExistingOfferId: $scope.appliedOffer,
            iBODetails: $scope.AddItem
        }
        var url = serviceBase + 'api/backendorder/applynewoffer';
        $http.post(url, dataTopost).success(function (data) {
            $scope.BillDiscountDetails = [];
            if (data.Status) {
                offer.showRemove = true;
                $scope.appliedOffer.push(offer.OfferId);
                alert(data.Message);
                console.log(data.Cart.DiscountDetails)
                $scope.BillDiscountDetails = data.Cart.DiscountDetails;
                console.log($scope.BillDiscountDetails)
                $scope.calculateofferdiscount();
            }
            else {
                alert(data.Message);
            }
        })
    }
    $scope.AppOfferList = [];
    $scope.AppliedOfferList = function () {
        $scope.AppOfferList = [];
        $scope.offerList.forEach(x => {
            let offerid = $scope.appliedOffer.filter(y => { return y == x.OfferId });
            if (offerid.length > 0) {
                $scope.AppOfferList.push(x);
            }
        })
    }

    $scope.RemoveBtn = function (offer) {
        alert("Offer Remove Successfully");
        $scope.appliedOffer = $scope.appliedOffer.filter(x => { return x != offer.OfferId });
        offer.showRemove = false;
        $scope.BillDiscountDetails = $scope.BillDiscountDetails.filter(x => { return x.OfferId != offer.OfferId });
        console.log($scope.appliedOffer)
        console.log($scope.BillDiscountDetails)
        $scope.calculateofferdiscount();
    }

    $scope.unlockScratch = function (offer) {
        if ($scope.shoppingCart.CustomerId > 0) {
            $scope.cid = $scope.shoppingCart.CustomerId;
        }
        else {
            $scope.cid = 0;
        }
        var url = serviceBase + 'api/ScratchBillDiscountOfferApp/UpdateScratchOfferById?OfferId=' + offer.OfferId + '&CustomerId=' + $scope.cid + '&IsScartched=' + true;
        $http.put(url).success(function (data) {

            if (data.Status) {
                offer.IsScratchBDCode = true;
                alert(data.Message);
            }
            else {
                alert(data.Message);
            }
        })
    }

    $scope.cancel = function () { window.location.reload(); }

    $scope.remove = function (item) {

        $('#myOverlay').show();
        var index = $scope.DOdata.indexOf(item);
        var cal = item.EditUnitPrice * item.qty;
        $scope.DOdata.splice(index, 1);
        $scope.total = $scope.total - cal;
        console.log('TotalAmt' + $scope.total);

        alert("Item is Deleted from List");
        $('#myOverlay').hide();
    };
    $scope.GetInvoiceData = function (orderdata) {

        window.location = "#/BackedOrderInvoice?id=" + orderdata.OrderId;
    };

    $scope.GetWarehouseBySkcode = function (SKcode) {
        var url = serviceBase + 'api/BackendOrder/GetWarehouseBySkcode/' + SKcode;

        $http.get(url)
            .success(function (data) {

                if (data) {
                    console.log("SKcode", data);
                    $scope.warehouse = data;
                }
                else {
                    alert("no record found or customer is not active");
                    window.location.reload();
                }
            });
    }

    //$scope.CustomerGstVerify = function (Mobile, RefNo) {
    //       ;
    //    if (RefNo == undefined) {
    //        alert("please enter reference number")
    //        return false;

    //    }
    //    else if (RefNo.length == 15) {
    //        var url = serviceBase + 'api/BackendOrder/CheckDuplicateGST?Mobile=' + Mobile + '&GST=' + RefNo;
    //        $http.get(url)
    //            .success(function (data) {
    //                //console.log(' $scope.RefNo', $scope.RefNo);

    //                console.log(data, 'data');
    //                $scope.custverifyData = data;
    //                alert($scope.custverifyData.msg)
    //                $scope.GstData = RefNo;
    //                console.log($scope.GstData, "GetData")
    //                //if (data.custverify.RefNo == "") {
    //                //    alert("Please Enter Gst No");
    //                //}
    //            });

    //    }
    //}
    $scope.IsvalidGST = false;
    $scope.ValidationGSTcheck = function (GSTno) {
        if (GSTno == undefined) {
            $scope.IsvalidGST = true;
        }
        if (GSTno.length < 15) {
            $scope.IsvalidGST = true;
        }
    };

    $scope.GstVerify = function (Mobile, RefNo) {

        if (RefNo.length > 0) {
            var url = serviceBase + 'api/BackendOrder/CheckDuplicateGST?Mobile=' + Mobile + '&GST=' + RefNo;
            $http.get(url)
                .success(function (data) {
                    //console.log(' $scope.RefNo', $scope.RefNo);
                    if (data.Status) {
                        console.log(data, 'data');
                        $scope.custverifyData = data;
                        $scope.GstData = RefNo;

                        var url = serviceBase + 'api/RetailerApp/CustomerGSTVerify?GSTNO=' + RefNo;
                        $http.get(url)
                            .success(function (data) {
                                if (data.Status) {
                                    //console.log(' $scope.RefNo', $scope.RefNo);
                                    console.log(data, 'data');
                                    $scope.GstData = data.custverify.RefNo;
                                    console.log($scope.GstData, "GetData")
                                    alert("Gst Number Verified");
                                }
                                else {
                                    alert("Pls enter valid GST/TIN_No/Ref No. For eg. - 23AAVCS1981Q1ZE");
                                    $scope.GstData = '';
                                    $scope.shoppingCart.RefNo = "";
                                }
                            });
                    } else {
                        $scope.IsvalidGST = true;
                        alert(data.msg);
                        $scope.GstData = '';
                        $scope.shoppingCart.RefNo = "";
                    }
                });
        }
    }
    $scope.WalletPointCheck = function () {
        var url = serviceBase + 'api/BackendOrder/WalletPointCheck?WarehouseId=' + $scope.warehouseId;
        $http.get(url).success(function (data) {
            $scope.OfferWalletConfig = data.OfferWalletConfig;
            $scope.OfferWalletValue = data.OfferWalletValue;
            $scope.RetailerWalletConfig = data.RetailerWalletPoint;
            $scope.ConsumerWalletConfig = data.ConsumerWalletPoint;
        });
    }
    //$scope.shoppingCart.Customerphonenum=0
    $scope.isenabled = false;
    $scope.isShow = false;
    $scope.checkNumber = function (MobileNo) {
        if (MobileNo.length == 10) {
        console.log(MobileNo);
            var url = serviceBase + 'api/BackendOrder/GetCustomerByMobile?MobileNo=' + MobileNo + '&WarehouseId=' + $scope.warehouseId;
            $http.get(url)
                .success(function (data) {
                    if (data.Status == true) {
                        //if (MobileNo)
                        $scope.isenabled = true;
                        $scope.isShow = true;
                        $scope.customer = data.getCutomerDetailsDC;
                        $scope.shoppingCart.CustomerId = $scope.customer.CustomerId;
                        $scope.shoppingCart.CustomerName = $scope.customer.Name;
                        $scope.Customerphonenum = $scope.customer.Mobile;
                        $scope.shoppingCart.ShopName = $scope.customer.Mobile;
                        $scope.shoppingCart.ShippingAddress = $scope.customer.ShippingAddress;
                        $scope.shoppingCart.ShippingAddress = $scope.customer.ShippingAddress;
                        $scope.shoppingCart.WalletPoint = $scope.customer.WalletPoint;
                        $scope.shoppingCart.RefNo = $scope.customer.RefNo;
                        $scope.selectedCustomerName = $scope.customer.Name + '(' + $scope.customer.Skcode + ')';
                        $scope.customertype = $scope.customer.CustomerType;
                        $scope.IsCustomerAddbtn = false;
                        $scope.IsWarehouseMisMatchbtn = false;
                        console.log(' $scope.customers: ', $scope.customer);
                    }
                    else {
                        if (data.Message == 'Customer Warehouse Mismatch') {
                            $scope.isenabled = false;
                            $scope.isShow = false;
                            $scope.IsWarehouseMisMatchbtn = true;
                            $scope.IsCustomerAddbtn = false;
                        }
                        else {
                            $scope.isenabled = true;
                            $scope.customertype = 'Consumer';
                            $scope.IsWarehouseMisMatchbtn = false;
                            $scope.IsCustomerAddbtn = true;
                            //$scope.shoppingCart.Customerphonenum = $scope.customer.Mobile;
                            //$scope.Customerphonenum = $scope.customer.Mobile;

                        }

                        alert(data.Message);
                        $scope.shoppingCart.CustomerId = undefined;
                        $scope.shoppingCart.CustomerName = undefined;
                        $scope.shoppingCart.ShopName = undefined;
                        $scope.shoppingCart.ShippingAddress = undefined;
                        $scope.shoppingCart.WalletPoint = 0;
                        $scope.selectedCustomerName = undefined;
                        $scope.shoppingCart.RefNo = undefined;
                    }
                });



        }
        else {
            $scope.shoppingCart.CustomerId = undefined;
            $scope.shoppingCart.CustomerName = undefined;
            $scope.shoppingCart.ShopName = undefined;
            $scope.shoppingCart.ShippingAddress = undefined;
            $scope.selectedCustomerName = undefined;
            $scope.shoppingCart.WalletPoint = 0;
            $scope.shoppingCart.RefNo = undefined;
            console.log(' $scope.customers: ', $scope.customer);
        }
    }


    $scope.checkqty = function (selectedTarde) {
        $scope.selectedTarde = [];
        $scope.selectedTarde = selectedTarde;
        if ($scope.selectedTarde.Batchqty < $scope.selectedTarde.Noofset) {
            alert('qty should not be greater than Stock qty!!');
            return false;
        }
    }


    $scope.unitpricecal = function (moqlist, qty) {

        angular.forEach(moqlist, function (moq, keys) {
            if (moq.MinOrderQty >= qty) {
                $scope.unitprice = moq.UnitPrice;
                return $scope.unitprice;
            }
        })
    }
    $scope.freeqty = 0;
    $scope.freebiesupload = function () {


        console.log($scope.AddItem)
        console.log($scope.FreebiesitemList)
        angular.forEach($scope.FreebiesitemList, function (freebies, key) {

            angular.forEach($scope.AddItem, function (add, keys) {


                if (freebies.ItemId == add.ItemId) {
                    add.Freebiesitem = [];
                    $scope.freeqty = 0;
                    if (freebies.OfferOn == "Item") {
                        $scope.freeqty = Math.floor((add.qty / freebies.MinOrderQty), 0);
                        freebies.OfferQty = 0;
                        if ($scope.freeqty > 0) {
                            freebies.OfferQty = freebies.OfferFreeitemQty * $scope.freeqty;
                        }
                        add.Freebiesitem.push({
                            Message: 'Buy ' + freebies.MinOrderQty + ' Get ' + freebies.OfferFreeitemQty + ' Free ',
                            OfferQty: freebies.OfferQty,
                            OfferOn: freebies.OfferOn,
                            OfferFreeItemId: freebies.OfferFreeItemId,
                            OfferFreeItemName: freebies.OfferFreeItemName,
                            UnitPrice: 0.0001,
                            Amount: 0
                            //OfferFreeitemUrl: freebies.OfferFreeItemUrl,
                            //OfferOn: freebies.OfferOn,
                            //OfferQty: freebies.OfferQty,
                            //Itemname: freebies.Itemname,
                            //MinOrderQty: freebies.MinOrderQty,
                            //OfferFreeItemName: freebies.OfferFreeItemName,
                            //OfferFreeitemQty: freebies.OfferFreeitemQty,
                            //OfferWalletPoint: freebies.OfferWalletPoint
                        })
                    }
                    else {
                        $scope.freeqty = Math.floor((add.qty / freebies.MinOrderQty), 0);
                        freebies.OfferQty = 0;
                        if ($scope.freeqty > 0) {
                            freebies.OfferQty = freebies.OfferWalletPoint * $scope.freeqty;
                        }
                        add.Freebiesitem.push({
                            Message: 'Buy ' + freebies.MinOrderQty + ' Get ' + freebies.OfferWalletPoint + ' Wallet Point Free',
                            OfferQty: freebies.OfferQty + ' Wallet Point',
                            OfferOn: freebies.OfferOn,
                            OfferFreeItemId: 0,
                            OfferFreeItemName: 'Wallet Point',
                            UnitPrice: 0,
                            Amount: 0
                            //OfferFreeitemUrl: freebies.OfferFreeItemUrl,
                            //OfferOn: freebies.OfferOn,
                            //OfferQty: freebies.OfferQty,
                            //Itemname: freebies.Itemname,
                            //MinOrderQty: freebies.MinOrderQty,
                            //OfferFreeItemName: freebies.OfferFreeItemName,
                            //OfferFreeitemQty: freebies.OfferFreeitemQty,
                            //OfferWalletPoint: freebies.OfferWalletPoint
                        })
                    }

                }
            })
        })

        console.log($scope.FreebiesitemList)
        console.log($scope.AddItem)
        console.log($scope.AddItem.Freebiesitem)

    }

    $scope.CheckCereditBtn = false;
    $scope.CheckCredit = function () {
        debugger
        var url = serviceBase + "api/BackendOrder/CheckCreditNote?CreditNoteNo=" + $scope.EnteredCreditNote;
        $http.get(url).success(function (result) {
            debugger
            console.log(result, "EnteredCreditNote")
            $scope.CheckCreditNote = result;
            if ($scope.CheckCreditNote.Status) {
                $scope.CheckCereditBtn = true;
                alert($scope.CheckCreditNote.Message);
                $scope.CheckCreditAmount = $scope.CheckCreditNote.Amount;
                $scope.CheckCreditValidTill = $scope.CheckCreditNote.CreditNoteValidTill;


            } else {
                $scope.EnteredCreditNote = '';
                $scope.CheckCereditMessage = $scope.CheckCreditNote.Message;
                $scope.CheckCereditBtn = false;
            }
            console.log(result);


        });
    }
    //$scope.CheckCreditValidTill = '';
    //$scope.CheckCreditAmount = 0;
    //$scope.EnteredCreditNote = '';
    $scope.onCloseApprovalReturnpopup = function () {
        $('#CheckCredit').modal('hide');
        //$scope.EnteredCreditNote = '';
        //$scope.CheckCreditValidTill = '';
        //$scope.CheckCreditAmount = 0;
    }
    $scope.Clear = function () {      
        $scope.EnteredCreditNote = '';
        $scope.CheckCreditValidTill = '';
        $scope.CheckCreditAmount = 0;
    }
    $scope.customercheck = function () {
        if ($scope.IsCustomerAddbtn == true) {
            alert("Please Add Customer First");
            return false;
        }
        else if ($scope.IsWarehouseMisMatchbtn == true) {
            alert("Warehouse MisMatch");
            return false;
        }
    }

    $scope.createcustomer = function () {
        if ($scope.shoppingCart.Customerphonenum == undefined || $scope.shoppingCart.Customerphonenum == "" || $scope.shoppingCart.Customerphonenum.length < 10) {
            alert("Please Enter Customer phone number"); return false;
        }
        else if ($scope.shoppingCart.CustomerName == undefined || $scope.shoppingCart.CustomerName == "") {
            alert("Please Enter Name"); return false;
        }
        else if ($scope.shoppingCart.ShippingAddress == undefined || $scope.shoppingCart.ShippingAddress == "") {
            alert("Please Enter Shipping Address"); return false;
        }
        else if (($scope.custverifyData.Status == false || $scope.custverifyData.Status == undefined) && ($scope.shoppingCart.RefNo != "" && $scope.shoppingCart.RefNo != null)) {
            alert("Please Verify Gst Number"); return false;
        }
        else if ($scope.warehouseId <= 0) {
            alert("Please Select Warehouse"); return false;
        }
        else {
            var savecustomer = {
                WarehouseId: $scope.warehouseId,
                MobileNo: $scope.shoppingCart.Customerphonenum,
                CustomerName: $scope.shoppingCart.CustomerName,
                ShippingAddress: $scope.shoppingCart.ShippingAddress,
                RefNo: $scope.shoppingCart.RefNo
            }
            var url = serviceBase + "api/BackendOrder/CreateNewCustomer";
            $http.post(url, savecustomer).success(function (result) {

                console.log(result);
                if (result.Status) {

                    $scope.isenabled = true;
                    $scope.isShow = true;
                    $scope.customer = result.getCutomerDetailsDC;
                    $scope.shoppingCart.CustomerId = $scope.customer.CustomerId;
                    $scope.shoppingCart.CustomerName = $scope.customer.Name;
                    $scope.Customerphonenum = $scope.customer.Mobile;
                    $scope.shoppingCart.ShopName = $scope.customer.Mobile;
                    $scope.shoppingCart.ShippingAddress = $scope.customer.ShippingAddress;
                    $scope.shoppingCart.ShippingAddress = $scope.customer.ShippingAddress;
                    $scope.shoppingCart.WalletPoint = $scope.customer.WalletPoint;
                    $scope.shoppingCart.RefNo = $scope.customer.RefNo;
                    $scope.selectedCustomerName = $scope.customer.Name + '(' + $scope.customer.Skcode + ')';
                    $scope.customertype = $scope.customer.CustomerType;
                    $scope.IsCustomerAddbtn = false;
                    alert(result.Message)
                    /* window.location = "layout/POS-System/BackedOrderInvoice/" + result.Data.OrderId;*/
                    //var preURI = saralUIPortal
                    //window.location = preURI + "/layout/POS-System/BackedOrderInvoice/" + result.Data.OrderId;
                }
                else {
                    alert(result.Message)
                    $scope.IsCustomerAddbtn = true;
                }
            })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        }
    }

    $scope.calculateofferdiscount = function () {
        if ($scope.BillDiscountDetails.length > 0) {
            $scope.BillDiscountOfferAmount = 0;
            $scope.BillDiscountDetails.forEach(x => {
                debugger
                $scope.BillDiscountOfferAmount += x.DiscountAmount;
                
            })

            if ($scope.BillDiscountOfferAmount > 0) {
                $scope.BillDiscountOfferAmount = $scope.BillDiscountOfferAmount.toFixed(2);
            }
        }
        else {
            $scope.BillDiscountOfferAmount = 0;
        }
    }


}]);
