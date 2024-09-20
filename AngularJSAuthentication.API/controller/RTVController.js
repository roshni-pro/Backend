'use strict';
app.controller('RTVController', ['$scope', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal", 'WarehouseService', 'supplierService', '$rootScope',
    function ($scope, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, WarehouseService, supplierService) {


        $scope.vm = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };
        $scope.searchdata = {};
        //for get warehouse info
        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {

        //        $scope.warehouse = results.data;
        //    }, function (error) {
        //    })
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    console.log("aaaaaaaa",$scope.warehouse )
                });
        };

        $scope.MultiCityModel = [];
        $scope.MultiCityModelsettings = {
            displayProp: 'label', idProp: 'value',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };

        $scope.wrshse();
        //$scope.getWarehosues();
        // end Warehouse info

        //Get date range filter
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        //end
        $scope.GetRTV = function (searchdata) {
            debugger;
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                    searchdata.WarehouseId = citystts
                });
            }

            if (searchdata.WarehouseId.length > 0) {

                if ($('#dat').val() == null || $('#dat').val() == "") {
                    $('input[name=daterangepicker_start]').val("");
                    $('input[name=daterangepicker_end]').val("");
                }
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                var datatopost = {
                    rowsPerPage: $scope.vm.rowsPerPage,
                    currentPage: $scope.vm.currentPage,
                    WarehouseIds: searchdata.WarehouseId,
                    SupplierCode: searchdata.SupplierCode,
                    RTVId: searchdata.RTVId,
                    StartDate: start,
                    EndDate: end,
                    Status: searchdata.Status

                }
                var url = serviceBase + "api/rtv/GetRTV";

                $http.post(url, datatopost)
                    .success(function (data) {
                
                        $scope.GrMaster = data.rTVMasterGetDcs;

                        $scope.vm.count = data.TotalCount;
                        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
                    }).error(function (data) {
                    })
            } else {
                alert('Please select Warehouse');
            }
        }
        //$scope.GetRTV();
        //export for rtv

        $scope.exportData = function (searchdata) {
            if (searchdata.WarehouseId.length > 0) {
                if ($('#dat').val() == null || $('#dat').val() == "") {
                    $('input[name=daterangepicker_start]').val("");
                    $('input[name=daterangepicker_end]').val("");
                }
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                var datatopost = {
                    rowsPerPage: $scope.vm.rowsPerPage,
                    currentPage: $scope.vm.currentPage,
                    WarehouseIds: searchdata.WarehouseId,
                    SupplierCode: searchdata.SupplierCode,
                    RTVId: searchdata.RTVId,
                    StartDate: start,
                    EndDate: end,
                    Status: searchdata.Status

                }
                var url = serviceBase + "api/rtv/ExportRTV";

                $http.post(url, datatopost)
                    .success(function (data) {
                        $scope.GetRTVExportMaster = data;

                        alasql('SELECT Id,WarehouseName,SupplierName,DepoName,StockType,ItemName,ItemQty,ItemPrice,GSTAmount,TaxableAmount,TotalAmount,Status,CreatedBy,CreatedDate INTO XLSX("RTVmaster.xlsx",{headers:true}) FROM ? ', [$scope.GetRTVExportMaster]);

                    }).error(function (data) {
                    })
            } else {
                alert('Please select Warehouse');
            }
        }
        //end

        $scope.CreateRTV = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "CreateRTVForm.html",
                    controller: "CreateRTVController", resolve: { role: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };
        $scope.PrintRTV = function (data) {
         
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PrintRTVForm.html",
                    controller: "PrintRTVController", resolve: { RTVdata: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };
        $scope.onNumPerPageChange = function () {
            $scope.GetRTV($scope.searchdata);
        };

        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.GetRTV($scope.searchdata);
            }, 100);

        };
        //for open details page
        $scope.ShowRTVDetails = function (data) {
           
            $scope.itemDelivered = false;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myRTVdetail.html",
                    controller: "RTVdetailsController", resolve: { RTVdetailsdata: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };
        //end details 
        //RTV cancel option
        $scope.editRTV = function (canceldata) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myRTVCancel.html",
                    controller: "ModalInstanCtrlRTVCancel", resolve: { rtvstatuschange: function () { return canceldata } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });

        }
        //end
        //$rootScope.$on("CallParentMethod", function () {
        //    
        //    $scope.GetRTV(searchdata);
        //});

    }]);

app.controller("CreateRTVController", ["$scope", '$http', '$window', 'role', '$modalInstance',
    function ($scope, $http, $window, role, $modalInstance) {
        $scope.IsDisabled = false;
        $scope.Warehouseid = 1;
        $scope.SearchSupplier = function (data) {

            $scope.Warehouseid = data.WarehouseId;
            $scope.Suppliers = [];
            var url = serviceBase + "api/Suppliers/search?key=" + data.supplier + "&WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (data) {
                $scope.Supplier = {
                    SupplierId: data.SupplierId,
                    SupplierName: data.SupplierName
                };
                $scope.data = $scope.Supplier.BuyerId;

                console.log($scope.Supplier);
                $scope.Suppliers.push($scope.Supplier);
                console.log($scope.Suppliers);
            })
        };
        $scope.GetDepo = function (data) {

            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {
                $scope.datadepomaster = response;
            }).error(function (data) {
                console.log("Error in get supplier depo.")
            })
        };
        $scope.getWarehosuesAddPo = function () {
            var url = serviceBase + 'api/Warehouse';
            $http.get(url).success(function (response) {
                $scope.warehouseAddPO = response;
            }).error(function (error) {
                console.log("Error in get warehouse.")
            });
        };
        $scope.getWarehosuesAddPo();
        $scope.Search = function (key, data) {
            //debugger
            $scope.GetStockBatchMastersList = [];
            var url = serviceBase + "api/rtv/SerachItemStockForRTV?key=" + key + "&WarehouseId=" + $scope.Warehouseid + "&StockType=" + data.StockType;
            $http.get(url).success(function (data) {
                $scope.itemData = data;
                console.log($scope.itemData);
                $scope.idata = angular.copy($scope.itemData);
            })
        };
        $scope.SearchAppPrice = function (MultiMrpId) {
            var url = serviceBase + "api/rtv/SerachApp_priceForRTV?MultiMrpId=" + MultiMrpId + "&WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (data) {
                $scope.APP_Price = data;
                if ($scope.GetStockBatchMastersList) {
                    for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                        if ($scope.GetStockBatchMastersList.length != null) {
                            $scope.GetStockBatchMastersList[c].APP_Price = $scope.APP_Price;
                        }
                    }
                }
            })
        };
        $scope.Minqtry = function (key, StockType) {
            //debugger
            $scope.StockTypes = "";
            if (StockType == "Damage") {
                $scope.StockTypes = "D";
            } else if (StockType == "Current") {
                $scope.StockTypes = "C";
            }
            else if (StockType == "Clearance") {
                $scope.StockTypes = "CL";
            }
            else if (StockType == "NonSellableStocks") {
                $scope.StockTypes = "N";
            }
            var vale = JSON.parse(key);
            $scope.itmdata = [];
            $scope.Searchitemdata = [];
            $scope.iidd = Number(vale.StockId);
            for (var c = 0; c < $scope.idata.length; c++) {
                if ($scope.idata.length != null) {
                    if ($scope.idata[c].StockId == $scope.iidd) {
                        $scope.Searchitemdata = [];
                        $scope.Searchitemdata = $scope.idata[c];
                        $scope.itmdata.push($scope.idata[c]);
                    }
                }
            }
            if ($scope.Searchitemdata && $scope.StockTypes) {
                var url = serviceBase + "api/CurrentStock/GetStockBatchMastersData?ItemMultiMRPId=" + $scope.Searchitemdata.MultiMrpId + "&WarehouseId=" + $scope.Warehouseid + "&stockType=" + $scope.StockTypes;
                $http.get(url).success(function (data) {
                    $scope.GetStockBatchMastersList = data;
                })
            }
            $scope.APP_Price = 0;
            $scope.SearchAppPrice(vale.MultiMrpId);
        }

        $scope.ClearList = function ()
        {    
            //debugger;
            $scope.itemData = undefined;
            $scope.GetStockBatchMastersList = []
           
        }

        $scope.POdata = [];
        $scope.AddData = function (item, app_price, selectedTarde) {
            debugger
            $scope.isExist = false;
            if (item.PeopleID == null && item.PeopleID == undefined) {
                item.PeopleID = $scope.currentBuyerId;
            }
            if (selectedTarde == undefined) {
                alert('Please select batch code!!');
                return false;
            }
            item.StockId = $scope.iidd;
            $scope.supplierData = true;
            $scope.supplierData1 = true;
            $scope.Depo = true;//by Anushka
            $scope.DepoH = true;//by Anushka
            $scope.buyerdata = true;
            var data = 0;
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.GetStockBatchMastersList[i].check == false) {
                    if ($scope.GetStockBatchMastersList[i].StockBatchMasterId == selectedTarde.StockBatchMasterId) {
                        $scope.selectedTarde = [];
                    }
                }
            };
            // Validate Item is Already Added //
            for (var c = 0; c < $scope.POdata.length; c++) {
                if ($scope.POdata[c].StockBatchMasterId == selectedTarde.StockBatchMasterId) {
                    data = 1;
                    item.Noofset = "";
                    item.PurchaseMinOrderQty = "";
                    $scope.isExist = true;
                    break;
                }
            }
            // Validate stock should be greater then 0 //
            for (var c = 0; c < $scope.itmdata.length; c++) {
                if ($scope.itmdata[c].StockId == item.StockId) {
                    if ($scope.itmdata[c].CurrentInventory <= 0) {
                        data = 2;
                        break;
                    }
                }
            }
            // Validate Pieces should be same or less then stock //
            for (var c = 0; c < $scope.itmdata.length; c++) {
                if ($scope.itmdata[c].StockId == item.StockId && data == 0) {
                    if ($scope.itmdata[c].CurrentInventory < item.Noofset) {
                        data = 3;
                        break;
                    }
                }
            }
            if (data == 1) {
                alert("Item is Already Added");
                item.Noofset = "";
                item.PurchaseMinOrderQty = "";
                return false;
            }
            else if (data == 2) {
                alert("stock should be greater then 0");
                item.Noofset = "";
                item.PurchaseMinOrderQty = "";
                return false;
            }
            else if (data == 3) {
                alert("Pieces should be same or less then stock.");
                item.Noofset = "";
                item.PurchaseMinOrderQty = "";
                return false;
            };

            //$scope.POdata = [];
            for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                if ($scope.GetStockBatchMastersList[i].check == true && !$scope.GetStockBatchMastersList[i].alredyAdded) {
                    if ($scope.GetStockBatchMastersList[i].Noofset == undefined || $scope.GetStockBatchMastersList[i].Noofset == "") {
                        alert('Please Enter Valid Number')
                    }
                    else if ($scope.GetStockBatchMastersList[i].APP_Price == undefined || $scope.GetStockBatchMastersList[i].APP_Price == "0") {
                        alert('App Price Can not Zero or Blank');
                    }
                    else {
                        var itemname;
                        var itemnumber;
                        var MultiMrpId;
                        var CurrentInventory;
                        for (var c = 0; c < $scope.itmdata.length; c++) {
                            if ($scope.itmdata[c].StockId == item.StockId) {
                                itemname = $scope.itmdata[c].itemname;
                                itemnumber = $scope.itmdata[c].ItemNumber;
                                MultiMrpId = $scope.itmdata[c].MultiMrpId;
                                CurrentInventory = $scope.itmdata[c].CurrentInventory;
                                break;
                            }
                        }
                        if (!$scope.isExist) {
                            if (data == 0) {
                                $scope.GetStockBatchMastersList[i].selectedcheck = true;
                                $scope.GetStockBatchMastersList[i].alredyAdded = true;
                                $scope.POdata.push({
                                    StockId: item.StockId,
                                    Itemname: itemname,
                                    ItemNumber: itemnumber,
                                    MultiMrpId: MultiMrpId,
                                    Noofset: $scope.GetStockBatchMastersList[i].Noofset,
                                    CurrentInventory: $scope.GetStockBatchMastersList[i].Qty,
                                    WarehouseId: item.WarehouseId,
                                    App_price: $scope.GetStockBatchMastersList[i].APP_Price,
                                    StockBatchMasterId: $scope.GetStockBatchMastersList[i].StockBatchMasterId,
                                    BatchCode: $scope.GetStockBatchMastersList[i].BatchCode,
                                });
                                item.Noofset = "";
                                $scope.APP_Price = 0;
                            }
                        }
                        //else {
                        //    alert('Please enter qty greater than zero ');
                        //    return false;
                        //}
                    }
                }
            }
        };
        $scope.checkqty = function (StockBatchMasterId, qty, noOfqty, selectedTarde) {
            //debugger;
            $scope.selectedTarde = [];
            $scope.selectedTarde = selectedTarde;
         
            if (qty < noOfqty) {
                $scope.selectStockBatchMasterId = Number(StockBatchMasterId);
                for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                    if ($scope.GetStockBatchMastersList.length != null) {
                        if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                            $scope.GetStockBatchMastersList[c].Noofset = '';
                            alert('qty should not be greater than Stock qty!!');
                            return false;
                        }
                    }
                }
            }            
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.Searchsave = function (mData) {
            debugger;
            $scope.IsDisabled = true;
            var datatoPost = {
                WarehouseId: mData.WarehouseId,
                SupplierId: mData.SupplierId,
                DepoId: mData.DepoId,
                StockType: mData.StockType,
                Detail: $scope.POdata
            };
            var url = serviceBase + 'api/rtv/postRtv';
            if ($window.confirm("Please confirm?"))
            {
                $http.post(url, datatoPost).success(function (result) {

                    alert(result);
                    if (result) {
                        $modalInstance.close();
                        $window.location.reload();
                    }

                }).error(function (result) {
                    alert(result.ErrorMessage);
                    $modalInstance.close();
                    console.log(result.ErrorMessage);
                });
            }
            else {
                $scope.IsDisabled = false;
            }

            //$modalInstance.close();
            //$window.location.reload();

        };
        $scope.remove = function (item) {
            debugger;
            var index = $scope.POdata.indexOf(item);
            if ($window.confirm("Please confirm?")) {
                for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
                    if ($scope.GetStockBatchMastersList[i].StockBatchMasterId == item.StockBatchMasterId) {
                        $scope.GetStockBatchMastersList[i].Noofset = "";
                        $scope.GetStockBatchMastersList[i].check = false;
                        $scope.GetStockBatchMastersList[i].APP_Price = "";
                        $scope.GetStockBatchMastersList[i].selectedcheck = false;
                        $scope.GetStockBatchMastersList[i].alredyAdded = false;
                    }
                }
                $scope.POdata.splice(index, 1);
            }
        };
    }]);

app.controller("PrintRTVController", ["$scope", '$http', 'RTVdata', 'supplierService', 'SearchPOService', '$modalInstance',
    function ($scope, $http, RTVdata, supplierService, SearchPOService, $modalInstance) {
        debugger;
        $scope.RTV = RTVdata;
        supplierService.getsuppliersbyid($scope.RTV.SupplierId).then(function (results) {
            console.log("ingetfn");
            console.log(results);
            $scope.supaddress = results.data.BillingAddress;
            $scope.SupContactPerson = results.data.ContactPerson;
            $scope.supMobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.TINNo;
        }, function (error) {
        });
        SearchPOService.getWarehousebyid($scope.RTV.WarehouseId).then(function (results) {

            console.log("get warehouse id");
            console.log(results);
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
            $scope.WhGst = results.data.GSTin;
        }, function (error) {
        });
        supplierService.getdepobyid($scope.RTV.DepoId).then(function (results) {

            console.log("ingetfn");
            console.log(results);
            $scope.depoaddress = results.data.Address;
            $scope.depoContactPerson = results.data.ContactPerson;
            $scope.depoMobileNo = results.data.Phone;
            $scope.depoGSTin = results.data.GSTin;
        }, function (error) {
        });
        $scope.ok = function () { $modalInstance.close(); };
    }]);
app.controller("RTVdetailsController", ["$scope", '$http', 'RTVdetailsdata', 'supplierService', '$modalInstance', "$modal", '$rootScope',
    function ($scope, $http, RTVdetailsdata, supplierService, $modalInstance, $modal) {
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $scope.RTVDetails = [];
           $scope.RTVDetails = RTVdetailsdata;
           $scope.RTVDetails.TotalAmount = Number($scope.RTVDetails.TotalAmount).toFixed(2);
           supplierService.getsuppliersbyid($scope.RTVDetails.SupplierId).then(function (results) {
            console.log(results);
            $scope.BillingAddress = results.data.BillingAddress;
            $scope.ContactPerson = results.data.ContactPerson;
            $scope.MobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.TINNo;
            $scope.SupplierName = results.data.Name;
        }, function (error) {
        });
        $scope.GetDetails = function () {
            var url = serviceBase + "api/rtv/GetRTVDetails?Id=" + $scope.RTVDetails.Id + "&StockType=" + $scope.RTVDetails.StockType + "&WarehouseId=" + $scope.RTVDetails.WarehouseId;
            $http.get(url)
                .success(function (data) {
                    $scope.RTVDetails = null;
                    $scope.RTVDetails = data;
                    $scope.RTVDetails.TotalAmount = Number($scope.RTVDetails.TotalAmount).toFixed(2);
                    //$scope.RTVDetails.Detail = data.detail;
                }).error(function (data) {
                })

        }
        $scope.GetDetails();
        $scope.DispatchRTV = function (data) {
            debugger;
            data.Status="Dispatched"
            console.log(data, "data")
            $scope.itemDelivered = true;
            var url = serviceBase + "api/rtv/DispatchRTV";
            $http.put(url, data)
                .success(function (data) {
                    debugger
                    $scope.IsChangeStatus = data;
                    if ($scope.IsChangeStatus) {
                        alert('RTV Status Update Successfully');
                        $modalInstance.close();

                    }

                }).error(function (data) {
                })
        }
        var s_col = false;
        var del = '';
        $scope.set_color = function (Detail) {
            if (Detail.Quantity > Detail.Stock) {
                s_col = true;
                return { background: "#ff9999" }
            }
            else { s_col = false; }
        }
        //$scope.RTVDetails.Detail = $scope.Details;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.ManualEdit = function (item) {
            console.log("Edit Dialog called survey");
            var modalInstance;
            var itemreturn = {
                item: item,
                Details: $scope.Details
            };
            modalInstance = $modal.open(
                {
                    templateUrl: "ManualEditRtv.html",
                    controller: "ModalInstanceCtrlForManualEditRTV", resolve: { inventory: function () { return itemreturn } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.IsDelivered = false;
        $scope.deliverdRTV = function (data) {
            debugger;
            console.log(data,"data")
            $scope.itemDelivered = true;
            var url = serviceBase + "api/rtv/DeliveredRtv";
            $http.post(url, data)
                .success(function (response) {
                    if (response != null && response == "RTV Done supplier ledger affected") {
                        $scope.IsDelivered = true;
                        $scope.itemDelivered = true;
                        if ($scope.IsDelivered) {
                            alert(response);
                            $scope.IsDelivered = true;
                            $modalInstance.close();
                            location.reload();
                        }
                    }
                    else {
                        alert(response);
                        $modalInstance.close();
                        location.reload();
                    }
                }).error(function (data) {
                })
        }

    }]);

app.controller("RTVCancelController", ["$scope", '$http', "$modalInstance", "RTVCanceldata", "ngAuthSettings", "$modal", "supplierService",
    function ($scope, $http, $modalInstance, RTVCanceldata, ngAuthSettings, $modal, supplierService) {
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $scope.RTVDetails = RTVCanceldata;
        console.log($scope.RTVDetails,"$scope.RTVDetails")

        supplierService.getsuppliersbyid($scope.RTVDetails.SupplierId).then(function (results) {
            console.log(results);
            $scope.BillingAddress = results.data.BillingAddress;
            $scope.ContactPerson = results.data.ContactPerson;
            $scope.MobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.TINNo;

        }, function (error) {
        });
        $scope.Savedata = function (RtVCancel) {
            var url = serviceBase + "api/rtv/RTVCancel";
            $http.put(url, RtVCancel)
                .success(function (data) {
                    debugger
                    $scope.IsChangeStatus = data;
                    if ($scope.IsChangeStatus) {
                        alert('RTV Status Update Successfully');
                        $modalInstance.close();

                    }

                }).error(function (data) {
                })

        }


    }]);


app.controller("ModalInstanceCtrlForManualEditRTV", ["$scope", '$http', "$modalInstance", "inventory", "ngAuthSettings", "$modal",
    function ($scope, $http, $modalInstance, inventory, ngAuthSettings, $modal) {

        $scope.xy = true;
        $scope.qtee = 0;
        $scope.inventoryData = {};
        if (inventory) {

            $scope.Item = inventory.item;
            $scope.Details = inventory.Details;
        }
        $scope.updatelineitem = function (data) {
            debugger;
            if (data.Quantity >= 0) {
                if ($scope.Item.Quantity <= 0) {

                    alert("Quantiy should not be negative");
                    $scope.Item.Quantity = 0;
                } else {
                    $scope.Item.Quantity = data.Quantity - 1;

                }
            }

        }
        $scope.updatelineitem1 = function (data) {
            debugger;
            if (data.Quantity != data.Stock) {
                $scope.Item.Quantity = data.Quantity + 1;
            }
            else {
                alert("qty should not be greator then Stock qty!!");
            }
        }
        $scope.ok = function () {
            $modalInstance.close();
        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    }]);

app.controller("ModalInstanCtrlRTVCancel", ["$scope", '$http', "$modalInstance", "rtvstatuschange", "ngAuthSettings", "$modal",
    function ($scope, $http, $modalInstance, rtvstatuschange, ngAuthSettings, $modal) {
        $scope.rtvstatuschange = rtvstatuschange;
        $scope.rtvstatuschange.Status = "";
        $scope.ok = function () {
            $modalInstance.close();
        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Savedata = function (RtVCancel) {
            debugger
            if ($scope.rtvstatuschange.Status == "") alert("please select status");
            var url = serviceBase + "api/rtv/RTVCancel";
            $http.put(url, RtVCancel)
                .success(function (data) {
                    $scope.IsChangeStatus = data;
                    if ($scope.IsChangeStatus) {
                        alert('RTV Status Update Successfully');
                        $modalInstance.close();

                    }

                }).error(function (data) {
                })
        }

    }]);