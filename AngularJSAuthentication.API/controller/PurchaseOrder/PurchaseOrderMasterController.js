(function () {
    'use strict';
    angular
        .module('app')
        .controller('PurchaseOrderMasterController', PurchaseOrderMasterController);

    PurchaseOrderMasterController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function PurchaseOrderMasterController($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        ///////////////////////////////////////////////////////////
        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [20];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.Ecount = 0;
        $scope.vm = {};
        $scope.vm.Excelupload = false;
        $scope.refresh = function () {

            $scope.currentPageStores = $scope.itemMasters;
            $scope.pagenoOne = 0;
            $scope.getData1($scope.pageno);
            $scope.selectedstatus = "";
        };

        //For Status

        $scope.getallstatus = function () {
            debugger
            var url = serviceBase + 'api/PurchaseOrderNew/GetAllStatus';
            $http.get(url)
                .success(function (data) {
                    $scope.GetAllStatus = data;
                    console.log($scope.GetAllStatus);
                });
        };
        $scope.getallstatus();
        //$scope.Warehouseid = 1;
        var currentWarehouse = localStorage.getItem('currentWarehouse');

        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse);
        }

        //$scope.getWarehosues = function () {


        //    var url = serviceBase + 'api/Warehouse';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouse = response;
        //            $scope.WarehouseId = $scope.Warehouseid;
        //            $scope.Warehousetemp = angular.copy(response);
        //            $scope.getData1($scope.pageno);
        //        }, function (error) {
        //        })


        //};  //old api for warehouse

        $scope.WarehouseId = $scope.Warehouseid ;
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.Warehouseid;
                    $scope.Warehousetemp = angular.copy(data);
                    $scope.getData1($scope.pageno);
                });

        };
        //$scope.wrshse();

        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'label', idProp: 'value',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
        // get Warehouses Add for PO
        $scope.getWarehosuesAddPo = function () {

            //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouseAddPO = response;


                }, function (error) {
                })
        };
        console.log($scope.warehouseAddPO);
        $scope.getWarehosuesAddPo();
        $scope.enNumSet = function () {
            $scope.Ecount = 1;
        };

        $scope.wrshse();
        //$scope.getWarehosues();
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        }
        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.currentPageStores = {};
        // PO History get data 
        $scope.PoHistroy = function (data) {

            $scope.datapomasterHistrory = [];
            var url = serviceBase + "api/PurchaseOrderList/POAddHistory?PurchaseorderId=" + data.PurchaseOrderId;
            $http.get(url).success(function (response) {
                $scope.datapomasterHistrory = response;
                console.log($scope.datapomasterHistrory);
            })
                .error(function (data) {
                })
        }
        //end PO History 
        //PO item History get data 
        $scope.PoitemHistroy = function (PurchaseorderId) {
            $scope.dataPoitemDetail = [];
            var url = serviceBase + "api/PurchaseOrderList/Itemdetailshistory?PurchaseorderId=" + PurchaseorderId;
            $http.get(url).success(function (response) {
                $scope.dataPoitemDetail = response;
                console.log($scope.dataPoitemDetail);
            })
                .error(function (data) {
                })
        }
        //end PO Item History 
        $scope.getData1 = function (pageno) {
            //debugger;
            if ($scope.selectedstatus == "undefined" || $scope.selectedstatus == null || $scope.selectedstatus == "NaN" || $scope.selectedstatus == "") {
                $scope.selectedstatus = "";
            }
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);// just seeeion 
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId + "&Status=" + $scope.selectedstatus;
            $http.get(url).success(function (response) {
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.value == id;
                });


                $scope.CityName = $scope.filterData[0].CityName;
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                //$scope.currentPageStores = $scope.itemMasters;
                $scope.callmethod();
            });
        };


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
        $scope.open = function (data) {
            window.location = "#/PurchaseOrderDetail?id=" + data.PurchaseOrderId;
        };
        $scope.invoice = function (data) {

            // window.location = "#/PurchaseInvoice?id=" + data.PurchaseOrderId;
            SearchPOService.view(data).then(function (results) {


                console.log("master invoice fn");
                console.log(results);
            }, function (error) {
            });
        };
        $scope.goodrecived = function (data) {

            SearchPOService.goodsrecived(data).then(function (results) {
                console.log("master invoice fn");
                console.log(results);
            }, function (error) {
            });
        };
        $scope.IRrecived = function (PurchaseorderDetails) {
            window.location = "#/IRNew?id=" + PurchaseorderDetails.PurchaseOrderId;
            //$scope.PurchaseOrderData = PurchaseorderDetails;
            //var url = serviceBase + 'api/IR/IsIgst?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId;
            //$http.get(url).success(function (data) {
            //    $scope.PurchaseOrderData.IsIgstIR = data;
            //    SearchPOService.IRopen($scope.PurchaseOrderData).then(function (results) {
            //        console.log("master invoice fn");
            //        console.log(results);
            //    }, function (error) {
            //    });
            //});
        };
        $scope.PRDetail = function (PurchaseorderDetails) {
            window.location = "#/AdvancePurchaseDetails?PRId=" + PurchaseorderDetails.PurchaseOrderId;
        };
        $scope.openmodel = function (data) { // calling same controller here, its not good.

            window.location = "#/AddPurchaseOrder";


            //$scope.$modalInstance = $modal.open(
            //    {

            //        templateUrl: "mySearchModal.html",
            //        controller: "searchPOController",
            //        //backdrop: 'static',
            //        resolve: { role: function () { return data } }
            //    }), $scope.$modalInstance.result.then(function (selectedItem) { })
        };
        $scope.ok = function () {

            $scope.$modalInstance.close();
        };
        $scope.cancel = function () {

            $scope.$modalInstance.dismiss('canceled');
        }
        $scope.searchKey = '';
        $scope.searchData = function () {
            if ($scope.searchKey == '') {
                alert("insert Po Number");
                return false;
            }
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderMaster/SearchPo?PoId=" + $scope.searchKey;
            $http.get(url).success(function (data) {
                $scope.Porders = data;
                $scope.callmethod();
            })
        };
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
        $scope.exportData = function (wid) {

            var ids = [];
            _.each($scope.examplemodel, function (o2) {

                console.log(o2);
                for (var i = 0; i < $scope.warehouse.length; i++) {
                    if ($scope.warehouse[i].value == o2.id) {
                        var Row =
                        {
                            "id": o2.id
                        };
                        ids.push(Row);
                    }
                }
            });
            if (ids.length == 0) {
                alert("Please Select Warehouse");
                return;
            }
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            if (start != null && start != "") {
                $scope.POM = [];
                var url = serviceBase + "api/PurchaseOrderMaster/Export";
                // var url = serviceBase + "api/PurchaseOrderMaster/GetNewExprt";
                var dataToPost = {
                    "From": start,
                    "TO": end,
                    ids: ids
                };
                $http.post(url, dataToPost)
                    .success(function (data) {

                        $scope.POM = data;
                        console.log(data);
                        // alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,ItemNumber,ItemName,QtyRecivedTotal,PriceRecived,QtyRecived1,Price1,Gr1Date,QtyRecived2,Price2,Gr2Date,QtyRecived3,Price3,Gr3Date,QtyRecived4,Price4,Gr4Date,QtyRecived5,Price5,Gr5Date,SupplierName,Status,POItemFillRate,TotalTAT,AverageTAT INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);
                        //alasql('SELECT PurchaseOrderId,WarehouseName,SupplierName,DepoName,ItemNumber,MOQ,Price,MRP,Total_No_Pieces,CreationDate INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);
                        alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,GrDate,ItemNumber,ItemName,Category as ABC_Classification,CompanyStockCode,PriceRecived,GRNo,TotalQuantity,Qty,Price,SupplierName,GRStatus,POItemFillRate,TotalTAT,AverageTAT,Category as ABC_Classification,PickerType,storeName,VehicleNo,VehicleType,InvoiceNumber,InvoiceDate,FreightCharge INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);
                    })
                    .error(function (data) {
                    });
            }
            else {
                alert('Please select Date parameter');
            }
        };
        /// Self Approval PO 
        $scope.selfapproved = function (data) {
            $scope.datapomasterHistrory = [];
            var url = serviceBase + "api/PurchaseOrderList/POAddHistory?PurchaseorderId=" + data.PurchaseOrderId;
            $http.get(url).success(function (response) {
                $scope.datapomasterHistrory = response;
                console.log($scope.datapomasterHistrory);
            }).error(function (data) {
            })
        };
        /// Send PO for Approval
        $scope.sendapproval = function (data) {

            $scope.datapomasterHistrory = [];
            var url = serviceBase + "api/PurchaseOrderList/POAddHistory?PurchaseorderId=" + data.PurchaseOrderId;
            $http.get(url).success(function (response) {
                $scope.datapomasterHistrory = response;
                console.log($scope.datapomasterHistrory);
            }).error(function (data) {
            })
        };
        //End Excel upload
        $scope.downloadexcelformat = function () {

            $scope.storesitem = [];
            alasql('SELECT ItemId,Itemname,Noofset,PurchaseMinOrderQty,Advance_Amt,SupplierName INTO XLSX("POItemList.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

        };
        $scope.Pochecker = function () {

            window.location = "#/GRApproval";

        };

        $scope.OpenClone = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "POClone.html",
                    controller: "POCloneController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        }
    }
})();

'use strict';
app.controller('AddPOController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", function ($modal, $scope, $filter, $http, ngTableParams) {

    $scope.pmdata = {};
    $scope.supplierData = false;
    $scope.supplierData1 = false;
    $scope.Depo = false;//by Anushka
    $scope.DepoH = false;//by Anushka
    $scope.data = {}
    $scope.SupplierOutStandingAmount = { OutstandingAmount: 0, AdvanceAmount: 0, AdvanceSettledAmount: 0, OutstandingAdvanceAmount: 0 };
    $scope.IsPOPRCreateStop = false;
    $scope.DepoPRPOStopLimit = 0;
    $scope.getWarehosuesAddPo = function () {
        var url = serviceBase + 'api/Warehouse';
        $http.get(url)
            .success(function (response) {
                $scope.warehouseAddPO = response;
            }, function (error) {
            });
    };
    $scope.getWarehosuesAddPo();

    $scope.Buyer = {};
    $scope.getBuyer = function () {

        var url = serviceBase + 'api/Suppliers/GetBuyer';
        $http.get(url)
            .success(function (response) {
                $scope.Buyer = response;
            });
    };
    $scope.getBuyer();
    $scope.data = [];
    $scope.SearchSupplier = function (data) {
        
        $scope.Warehouseid = data.WarehouseId;
        $scope.Suppliers = [];
        var url = serviceBase + "api/Suppliers/searchSupplier?key=" + data.supplier + "&Warehouseid=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {

            $scope.Supplier = {
                SupplierId: data.SupplierId,
                SupplierName: data.Name,
                PaymentTerms: data.PaymentTerms

            };
            //$scope.data = $scope.Supplier.BuyerId;
            $scope.data.SupplierCreditDay = $scope.Supplier.PaymentTerms;
            $scope.Suppliers.push($scope.Supplier);
        });
        
    };

    $scope.GetSupplierOutStandingAmount = function (supplierId) {
        var url = serviceBase + "api/PurchaseOrderNew/GetSupplierOutStandingAmount?supplierId=" + supplierId;
        $http.get(url).success(function (data) {
            $scope.SupplierOutStandingAmount = data;
        });
    };

    $scope.GetDepo = function (data) {
        $scope.GetSupplierOutStandingAmount(data);
        $scope.fillbuyer(data);
        $scope.datadepomaster = [];
        var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
        $http.get(url).success(function (response) {
            $scope.datadepomaster = response;
        }).error(function (data) {
        })
    }
    $scope.CheckPRPOCreateStatus = function (depoId) {

        var url = serviceBase + "api/Suppliers/CheckPRPOStopBySupplierDepo?depoId=" + depoId;
        $http.get(url).success(function (response) {
            $scope.DepoPRPOStopLimit = response;

            $scope.IsPOPRCreateStop = response > 0;
        }).error(function (data) {
        })
    }


    $scope.idata = {};
    $scope.iidd = 0;
    
    $scope.Minqtry = function (key) {
        $scope.itmdata = [];
        $scope.iidd = Number(key);

        PackingMaterial(key);

        if ($scope.idata.length != null) {

            for (var c = 0; c < $scope.idata.length; c++) {
                if ($scope.idata[c].ItemId == $scope.iidd) {
                    $scope.itmdata.push($scope.idata[c]);
                    $scope.data.price = $scope.idata[c].price;
                    $scope.data.NetPurchasePrice = $scope.idata[c].NetPurchasePrice;
                    $scope.data.POPurchasePrice = $scope.idata[c].POPurchasePrice;
                }
            }
        }
    }

    function PackingMaterial(key) {
        var url = serviceBase + "api/Item/GetItemDetails?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (response) {

            if (response) {
                $scope.pmdata = response;
            } else {
                $scope.pmdata = null;
            }

        }).error(function (data) {
        })
    }

    $scope.enNumSet = function () {
        $scope.Ecount = 1;
    };

    $scope.POdata = [];
    $scope.AddData = function (item) {

        if (item.PeopleId == null || item.PeopleId == undefined || item.PeopleId == "") {
            item.PeopleID = $scope.currentBuyerId;
            item.PeopleId = $scope.currentBuyerId;
        }
        $scope.supplierData = true;
        $scope.supplierData1 = true;
        $scope.Depo = true;//by Anushka
        $scope.DepoH = true;//by Anushka
        $scope.buyerdata = true;
        var IsItemNPP = false;
        var IsItemPOP = false;


        IsItemNPP = item.NetPurchasePrice > 0;
        IsItemPOP = item.POPurchasePrice > 0;
        if (IsItemPOP == true && IsItemNPP == true) {

            if (item.Noofset == undefined || item.Noofset == "") {
                alert('Please fill Number of Set')
            }
            else if (item.Noofset == undefined || item.Noofset == "") {
                alert('Please fill PurchaseMinOrderQty')
            }
            else if (item.Noofset == undefined || item.Noofset == "") {
                alert('Please fill Item Name')
            }
            else if (item.DepoId < 0 || item.DepoId == "undefined" || item.DepoId == null) {
                alert('Please Select Depo');
                return false;
            }
            else {
                var itemname;
                for (var c = 0; c < $scope.itmdata.length; c++) {
                    if ($scope.itmdata[c].ItemId == item.ItemId) {
                        itemname = $scope.itmdata[c].itemname;
                        break;
                    }
                }
                var data = true;
                for (var c = 0; c < $scope.POdata.length; c++) {
                    if ($scope.POdata[c].ItemId == item.ItemId) {
                        data = false;
                        break;
                    }
                }
                console.log("checkkkkkkk")
                console.log($scope.pmdata)
                if (data == true) {
                    console.log()
                    $scope.POdata.push({
                        Itemname: itemname,
                        ItemId: item.ItemId,

                        Noofset: item.Noofset,
                        ConvertPurchaseOrder: item.PurchaseMinOrderQty,
                        PurchaseMinOrderQty: $scope.pmdata != null && $scope.pmdata.MaterialItemMaster.FromConversion == 'Kg' ? $scope.pmdata.MaterialItemMaster.ToValue * item.PurchaseMinOrderQty : item.PurchaseMinOrderQty,
                        SupplierId: $scope.Supplier.SupplierId,
                        Advance_Amt: item.Advance_Amt,
                        BuyerId: item.PeopleId,
                        WarehouseId: item.WarehouseId,
                        DepoId: item.DepoId, //by Anushka
                        DepoName: item.DepoName, //by Anushka
                        CashPurchaseName: item.CashPurchaseName,
                        IsCashPurchase: item.IsCashPurchase,
                        price: item.price,
                        NetPurchasePrice: item.NetPurchasePrice,
                        PurchasePrice: item.POPurchasePrice,
                        IsDraft: false,
                        PRType: 0,
                        PickerType: item.PickerType,
                        SupplierCreditDay: item.SupplierCreditDay

                    });
                    item.Noofset = "";
                    item.PurchaseMinOrderQty = "";
                    item.ItemId = "";
                    item.price = 0;
                    item.NetPurchasePrice = 0;
                }
                else {
                    alert("Item is Already Added");
                    item.Noofset = "";
                    item.PurchaseMinOrderQty = "";
                    item.ItemId = "";
                    item.price = 0;
                    item.NetPurchasePrice = 0;
                }
            }
        }
        else {
            alert('NPP or POPrice is 0 .Please Update Purchase Price');
        }

    };
    $scope.Searchsave = function () {

        var data = $scope.POdata;
        var url = serviceBase + 'api/PurchaseOrderNew/SavePO';
        if (data.length > 0) {
            $scope.count = 1;
            $http.post(url, data).success(function (result) {

                if (result.Status) {
                    alert(result.Message);
                    $('#msgModel').modal('hide');
                    window.location = "#/PurchaseOrderMaster";
                }
                else {
                    //$modalInstance.close(data);
                    alert(result.Message);
                    window.location.reload();
                }
            });
        } else {
            alert("Please fill data.");
        }
    };
    $scope.Draftsave = function () {

        var data = $scope.POdata;
        _.each(data, function (item) {
            item.IsDraft = true;
        });

        var url = serviceBase + 'api/PurchaseOrderNew/SavePO';
        if (data.length > 0)/////
        {
            $scope.count = 1;
            $http.post(url, data).success(function (result) {

                if (result.Status) {
                    alert(result.Message);
                    window.location = "#/PurchaseOrderMaster";
                }
                else {
                    alert(result.Message);
                    window.location.reload();
                }
            })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        } else {
            alert("Please fill data.");
        }
    };
    //Remove item from list
    $scope.remove = function (item) {
        var index = $scope.POdata.indexOf(item);
        $scope.POdata.splice(index, 1);
    };
    $scope.currentBuyerId = {};
    $scope.fillbuyer = function (sid) {
        var url = serviceBase + 'api/Suppliers/SupDetail?sid=' + sid + '';
        $http.get(url)
            .success(function (response) {
                $scope.Buyerdata = response;
                $scope.currentBuyerId = response.PeopleID;

            });
    };
    $scope.Search = function (key) {

        var url = serviceBase + "api/itemMaster/SearchinitemPOadd?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {
            $scope.itemData = data;
            $scope.idata = angular.copy($scope.itemData);
        })
    };

    $scope.cancel = function () {
        window.location = "#/PurchaseOrderMaster";
    }
}]);

(function () {
    'use strict';
    angular
        .module('app')
        .controller('AdvancePurchaseRequestController', AdvancePurchaseRequestController);

    AdvancePurchaseRequestController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function AdvancePurchaseRequestController($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [20];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.Ecount = 0;
        $scope.vm = {};
        $scope.vm.Excelupload = false;
        $scope.refresh = function () {
            $scope.currentPageStores = $scope.itemMasters;
            $scope.pagenoOne = 0;
            $scope.getData1($scope.pageno);
        };
        //$scope.Warehouseid = 1;
        var currentWarehouse = localStorage.getItem('currentWarehouse');

        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse);
        }

        $scope.getWarehosues = function () {
            
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                    $scope.WarehouseId = $scope.Warehouseid;
                    $scope.Warehousetemp = angular.copy(response);
                    $scope.getData1($scope.pageno);
                }, function (error) {
                })


        }; //old api for warehouse -2023
        //$scope.wrshse();

        $scope.getWarehosuesAddPo = function () {
            //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouseAddPO = response;
                }, function (error) {
                })
        };
        console.log($scope.warehouseAddPO);
        $scope.getWarehosuesAddPo();
        $scope.enNumSet = function () {
            $scope.Ecount = 1;
        };

        $scope.getWarehosues();
        //$scope.wrshse();
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        }
        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.currentPageStores = {};

        $scope.getData1 = function (pageno) {
            
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);// just seeeion 
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderNew/GetPRlist" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {
                
                
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId == id;
                });


                $scope.CityName = $scope.filterData[0].CityName;
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                //$scope.currentPageStores = $scope.itemMasters;
                $scope.callmethod();
            });
        };


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
        $scope.open = function (data) {
            window.location = "#/AdvancePurchaseRequestDetail?id=" + data.PurchaseOrderId;
        };
        $scope.openDetails = function (data) {
            window.location = "#/AdvancePurchaseDetails?id=" + data.PurchaseOrderId;
            //SearchPOService.view(data).then(function (results) {


            //    console.log("master invoice fn");
            //    console.log(results);
            //}, function (error) {
            //});
        };

        $scope.openmodel = function (data) { // calling same controller here, its not good.

            window.location = "#/AddAdvancePORequest";
        };
        $scope.ok = function () {

            $scope.$modalInstance.close();
        };
        $scope.cancel = function () {

            $scope.$modalInstance.dismiss('canceled');
        }
        $scope.searchKey = '';
        $scope.searchData = function () {
            if ($scope.searchKey == '') {
                alert("insert PR Number");
                return false;
            }
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderNew/SearchPR?PoId=" + $scope.searchKey;
            $http.get(url).success(function (data) {
                $scope.Porders = data;
                $scope.callmethod();
            })
        };
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
        $scope.exportData = function (wid) {
            
            //var ids = [];
            //_.each($scope.examplemodel, function (o2) {

            //    console.log(o2);
            //    for (var i = 0; i < $scope.warehouse.length; i++) {
            //        if ($scope.warehouse[i].WarehouseId == o2.id) {
            //            var Row =
            //                {
            //                    "id": o2.id
            //                };
            //            ids.push(Row);
            //        }
            //    }
            //});
            //if (ids.length == 0) {
            //    alert("Please Select Warehouse");
            //    return;
            //}
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            if (start != null && start != "") {
                $scope.POM = [];
                
                var url = serviceBase + "api/PurchaseOrderNew/AdvExport";
                var dataToPost = {
                    "From": start,
                    "TO": end,
                    WarehouseId: wid
                };
                
                $http.post(url, dataToPost)
                    .success(function (data) {
                        $scope.POM = data;
                        alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,ItemNumber,CompanyStockCode,ItemName,MRP,Price,TotalQuantity,SupplierName,FreightCharge INTO XLSX("AdvancePurchaseRequest.xlsx",{headers:true}) FROM ?', [$scope.POM]);

                    })
                    .error(function (data) {
                    });
            }
            else {
                alert('Please select Date parameter');
            }
        };
        /// Self Approval PO 
        $scope.selfapproved = function (data) {
            $scope.datapomasterHistrory = [];
            var url = serviceBase + "api/PurchaseOrderList/POAddHistory?PurchaseorderId=" + data.PurchaseOrderId;
            $http.get(url).success(function (response) {
                $scope.datapomasterHistrory = response;
                console.log($scope.datapomasterHistrory);
            }).error(function (data) {
            })
        };
        /// Send PO for Approval
        $scope.sendapproval = function (data) {

            $scope.datapomasterHistrory = [];
            var url = serviceBase + "api/PurchaseOrderList/POAddHistory?PurchaseorderId=" + data.PurchaseOrderId;
            $http.get(url).success(function (response) {
                $scope.datapomasterHistrory = response;
                console.log($scope.datapomasterHistrory);
            }).error(function (data) {
            })
        };
        //End Excel upload
        $scope.downloadexcelformat = function () {

            $scope.storesitem = [];
            alasql('SELECT ItemId,Itemname,Noofset,PurchaseMinOrderQty,Advance_Amt,SupplierName INTO XLSX("POItemList.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

        };
        $scope.Pochecker = function () {

            window.location = "#/GRApproval";

        };



        $scope.CancelModel = function (canceldata) { // calling same controller here, its not good.

            //window.location = "#/AddAdvancePORequest";

            console.log("Modal opened chequedetails");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCancelModal.html",
                    controller: "ModalInstanceCtrlCancel", resolve: { canceldata: function () { return canceldata } }
                });
            modalInstance.result.then(function (selectedItem) {
                //$scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                });
        };



        $scope.OpenClone = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PRClone.html",
                    controller: "PRCloneController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        }

    }




})();
'use strict';
app.controller('AddAdvancePORequestController', ['$modal', '$scope', "$filter", "$http", "$routeParams", "ngTableParams", 'WarehouseService',
    function ($modal, $scope, $filter, $http, $routeParams, ngTableParams, WarehouseService) {

        $scope.EligibleQtyForPo = {};
        var _Whid = $routeParams.whid;
        var SupName = $routeParams.SupName;
        $scope.isSaveBtnhide = true;
        $scope.AutoPRbtn = false;
        $scope.supplierData = false;
        $scope.supplierData1 = false;
        $scope.Depo = false;//by Anushka
        $scope.DepoH = false;//by Anushka
        $scope.data = {}
        $scope.SupplierOutStandingAmount = { OutstandingAmount: 0, AdvanceAmount: 0, AdvanceSettledAmount: 0, OutstandingAdvanceAmount: 0 };
        $scope.IsPOPRCreateStop = false;
        $scope.DepoPRPOStopLimit = 0;
        $scope.itemWeight = null;
        $scope.showfreight = true;

        $scope.onfreightchange = function (data) {
            debugger
            if (data == "DeliveryfromSupplier") {
                $scope.showfreight = false;
            }
            else {
                $scope.showfreight = true;
            }
        };
       
        $scope.City = function () {
            var url = serviceBase + 'api/City/GetPoCityWithoutKpp';
            $http.get(url)
                .success(function (response) {
                    $scope.CityAddPO = response;
                }, function (error) {
                });
        };
        $scope.City();

        $scope.warehouseAddPO = [];
        $scope.warehouseQty = [];
        $scope.getWarehosues = function (CityId) {
            WarehouseService.warehousecitybased(CityId).then(function (results) {
                $scope.warehouseAddPO = results.data;
                $scope.GetForeCastWarehouseIds(CityId);
            }, function (error) {
            });

        }
        $scope.refreshAll = function () {
            window.location.reload();
        };

        //$scope.getWarehosuesAddPo = function () {
        //    var url = serviceBase + 'api/Warehouse';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouseAddPO = response;
        //        }, function (error) {
        //        });
        //};
        //$scope.getWarehosuesAddPo();
        // $scope.warehouseAddPO = [];
        $scope.subexamplemodel = [];
        $scope.subexamplemodel = $scope.warehouseAddPO;
        $scope.subexamplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };

        $scope.Buyer = {};
        $scope.getBuyer = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyer = response;
                });
        };
        $scope.getBuyer();
        $scope.data = [];
        $scope.SearchSupplier = function (data) {
            if (data.Cityid == undefined || data.Cityid == "") {
                alert("Please Select city.");
                return false;
            }
            if ($scope.subexamplemodel.length == 0) {
                alert("Please Select at least one warehouse.");
                return false;
            }
            $scope.ETAWhList = [];
            for (var i = 0; i < $scope.subexamplemodel.length; i++) {

                var list = [];
                list = {
                    Whid: $scope.subexamplemodel[i].id,
                    Whname: $scope.warehouseAddPO.find(o => o.WarehouseId === $scope.subexamplemodel[i].id).WarehouseName //$scope.warehouseAddPO[i].WarehouseName

                }
                $scope.ETAWhList.push(list);
            }

            $scope.warehouseN = $scope.warehouseAddPO[0].WarehouseId;
            // $scope.Warehouseid = data[0].WarehouseId;
            $scope.Suppliers = [];
            var url = serviceBase + "api/Suppliers/searchSupplierForPRadd?key=" + data.supplier + "&Warehouseid=" + $scope.warehouseN;
            $http.get(url).success(function (data) {

                //$scope.Supplier = {
                //    SupplierId: data.SupplierId,
                //    SupplierName: data.Name,
                //    PaymentTerms: data.PaymentTerms
                //};
                //$scope.data = $scope.Supplier.BuyerId;
                // $scope.data.SupplierCreditDay = $scope.Supplier.PaymentTerms;
                $scope.Suppliers.push(data);
            });
        };
        $scope.GetSupplierOutStandingAmount = function (supplierId) {
            var url = serviceBase + "api/PurchaseOrderNew/GetSupplierOutStandingAmount?supplierId=" + supplierId;
            $http.get(url).success(function (data) {
                $scope.SupplierOutStandingAmount = data;
            });
        };
        $scope.GetDepo = function (data) {
            $scope.GetSupplierOutStandingAmount(data);
            $scope.fillbuyer(data);
            $scope.datadepomaster = [];

            var SuppData = $scope.Suppliers[0];
            for (var i = 0; i < SuppData.length; i++) {
                if (SuppData[i].SupplierId == data) {
                    $scope.Supplier = {
                        SupplierId: data,
                        SupplierName: SuppData[i].Name,
                        PaymentTerms: SuppData[i].PaymentTerms
                    };
                    $scope.data.SupplierCreditDay = SuppData[i].PaymentTerms;
                }
            }

            var url = serviceBase + "api/Suppliers/GetDepoForPR?id=" + data;
            $http.get(url).success(function (response) {
                $scope.datadepomaster = response;

            }).error(function (data) {
            })
        }
        $scope.ETAList = [];
        var whid = 0;
        var etadt = 0;
        $scope.ETAconfirm = function (wh, date) {

            whid = wh;
            etadt = date;
            if (date == '' || date == undefined) {
                alert("Please enter ETA Date...");
                return false;
            }

            var varDate = new Date(date); //dd-mm-YYYY
            varDate.setHours(0, 0, 0, 0);
            var today = new Date();
            today.setHours(0, 0, 0, 0);
            if (varDate < today) {
                //Do something..
                alert("ETA date not less than today date!");
                return false;
            }
            var ETAcont = 0;
            var postData = {
                WarehouseId: wh,
                ETADate: date,
            };

            $http.post(serviceBase + "api/PurchaseOrderNew/GetETADateCountWHWise", postData).success(function (data, status) {

                if (data != null) {
                    $scope.ETAdetailslist = data;
                    ETAcont = data.length;
                }
                if (ETAcont >= 5) {
                    $('#ModelETA').modal('show');
                    $scope.etamsg = "Already " + ETAcont + " GR ETA is lined up for this date " + varDate + " The slot for this PO might not be available.Do you still want to continue?"
                    //alert("Already 5 GR ETA is lined up for this date " + date + " The slot for this PO might not be available.Do you still want to continue?");
                }
                else {
                    var eta = {
                        WHid: wh,
                        EtaDate: date
                    }
                    for (var i = 0; i < $scope.ETAList.length; i++) {
                        if ($scope.ETAList[i].WHid == wh) {
                            $scope.ETAList.splice(i, 1);
                        }
                    }
                    $scope.ETAList.push(eta);
                    alert('ETA confirmed');
                }
            }).error(function (data) {

                alert("error: ", data);
            });

        }

        $scope.ETAmorethaFiveConfirmation = function () {

            if (confirm($scope.etamsg)) {
                var eta = {
                    WHid: whid,
                    EtaDate: etadt
                }
                for (var i = 0; i < $scope.ETAList.length; i++) {
                    if ($scope.ETAList[i].WHid == whid) {
                        $scope.ETAList.splice(i, 1);
                    }
                }
                $scope.ETAList.push(eta);
                alert('ETA confirmed');
                $('#ModelETA').modal('hide');
            }
        }
        $scope.CheckPRPOCreateStatus = function (depoId) {

            var url = serviceBase + "api/Suppliers/CheckPRPOStopBySupplierDepo?depoId=" + depoId;
            $http.get(url).success(function (response) {
                $scope.DepoPRPOStopLimit = response;
                $scope.IsPOPRCreateStop = response > 0;
            }).error(function (data) {
            })
        }
        $scope.itemWeight = null;


        
        $scope.idata = {};
        $scope.iidd = 0;
        $scope.Minqtry = function (key) {
            console.log($scope.warehouseN)
            debugger;
            $scope.iidd = Number(key);
            if ($scope.iidd > 0) {
                var url = serviceBase + "api/PurchaseOrderNew/GetBueridthroughitemid?warehouseid=" + $scope.warehouseN + "&itemid=" + $scope.iidd;
                $http.get(url).success(function (response) {
                    $scope.data.PeopleId = response.PeopleId;
                }).error(function (data) {

                })
            }
            $scope.EligibleQtyForPo = null;
            $scope.getItemWeight(key);
            $scope.CheckEligibleQtyForPo(key);
            console.log('$scope.data', $scope.data)
            $scope.itmdata = [];
            $scope.iidd = Number(key);
            if ($scope.idata.length != null) {
                for (var c = 0; c < $scope.idata.length; c++) {
                    if ($scope.idata[c].ItemId == $scope.iidd) {
                        $scope.itmdata.push($scope.idata[c]);
                        $scope.data.price = $scope.idata[c].price;
                        $scope.data.NetPurchasePrice = $scope.idata[c].NetPurchasePrice;
                        $scope.data.POPurchasePrice = $scope.idata[c].POPurchasePrice;
                        $scope.data.WarehouseName = $scope.idata[c].WarehouseName;
                        $scope.data.WarehouseId = $scope.idata[c].WarehouseId;
                        $scope.data.Type = $scope.idata[c].Type;
                        $scope.data.MaterialItemId = $scope.idata[c].MaterialItemId;
                        $scope.data.Category = $scope.idata[c].Category;
                        $scope.warehouseQty = [];
                        _.each($scope.subexamplemodel, function (o2) {

                            _.each($scope.warehouseAddPO, function (warehosue) {

                                if (warehosue.WarehouseId == o2.id) {
                                    var Row = {};
                                    Row.WarehouseId = warehosue.WarehouseId;
                                    Row.WarehouseName = warehosue.WarehouseName;
                                    Row.Noofset = 0;
                                    Row.Type = $scope.data.Type;
                                    Row.MaterialItemId = $scope.data.MaterialItemId;
                                    // Row.Category = $scope.data.Category;
                                    $scope.warehouseQty.push(Row);
                                }
                            });
                        });

                    }
                }
            }
        }

        $scope.enNumSet = function () {
            $scope.Ecount = 1;
        };
        $scope.getItemWeight = function () {
            $scope.itemWeight = null;
            if ($scope.data && $scope.data.ItemId) {
                var url = serviceBase + "api/PurchaseOrderNew/GetWeight?itemId=" + $scope.data.ItemId;
                $http.get(url)
                    .success(function (response) {
                        console.log('response issssssss:', response);
                        debugger
                        $scope.itemWeight = response;
                        if ($scope.itemWeight && !$scope.itemWeight.weighttype) {
                            $scope.itemWeight.weighttype = "";
                        }
                    })
                    .error(function (data) {

                    });
            } else {
                $scope.itemWeight = null;
            }
        }


        //Region start ForeCast

        $scope.EligibleQtyForPo = null;
        $scope.CheckEligibleQtyForPo = function (ItemId) {
            $scope.EligibleQtyForPo = null;
            var itemmrp = $scope.itemData.find(record => record.ItemId === parseInt(ItemId));
            if (itemmrp.WarehouseId > 0 && itemmrp.ItemMultiMRPId > 0) {
                var url = serviceBase + "api/PurchaseOrderNew/EligibleQtyForPo/" + itemmrp.WarehouseId + "/" + itemmrp.ItemMultiMRPId;
                $http.get(url).success(function (data) {
                    if (data != null) {
                        $scope.EligibleQtyForPo = data;
                    }
                });
            }
        };
        //end  ForeCast
        $scope.POdata = [];
        $scope.AddDataPR = function (item) {
            debugger
            if ($scope.itemWeight.weight == "0" || !$scope.itemWeight.weighttype || !$scope.itemWeight.weight || ($scope.itemWeight.weighttype == 'pc' && !$scope.itemWeight.WeightInGram)) {
                alert('please select correct Single Item Unit and Single Item Value and weight in gram');
                return false;
            }
            if (item.PeopleId == null || item.PeopleId == undefined || item.PeopleId == "") {
                item.PeopleID = $scope.currentBuyerId;
                item.PeopleId = $scope.currentBuyerId;
            }
            if ($scope.showfreight == false) {
                if ($scope.FreightCharge == null || $scope.FreightCharge == undefined) {
                    alert("Please Enter Freight Charge");
                    return false;
                }
                else if ($scope.FreightCharge < 0) {
                    alert("Freight Charge not be negetive");
                    return false;
                }
            }
            if (item.ItemId > 0) {
                var url = serviceBase + "api/PurchaseOrderNew/PoCheckbySubcatid?warehouseid=" + item.WarehouseId + "&ItemId=" + item.ItemId + "&SubcategoryId=" + 0 + "&SubsubcategoryId=" + 0 + "&Multimrpid=" + 0;
                $http.get(url)
                    .success(function (response) {
                        debugger
                        if (response.StopPo == true) {
                            alert(response.CompanyBrand);
                            return false;
                        }
                        else {
                            $scope.test();
                            $scope.supplierData = true;
                            $scope.supplierData1 = true;
                            $scope.Depo = true;//by Anushka
                            $scope.DepoH = true;//by Anushka
                            $scope.buyerdata = true;
                            var IsItemNPP = false;
                            var IsItemPOP = false;
                            var WarehouseIds = [];
                            var selectedWsQty = [];
                            _.each($scope.warehouseQty, function (wsqty) {
                                if (parseInt(wsqty.Noofset) > 0) {
                                    var Row = wsqty.WarehouseId;
                                    WarehouseIds.push(Row);
                                    selectedWsQty.push(wsqty);
                                }
                            });
                            if (item.SupplierId > 0) {
                                var itemmrp = $scope.itemData.find(record => record.ItemId === parseInt(item.ItemId));
                                var url = serviceBase + "api/PurchaseOrderNew/GetRetailerForSupplierItem?SupplierId=" + item.SupplierId + "&itemmultiMrpIds=" + itemmrp.ItemMultiMRPId;
                                $http.get(url)
                                    .success(function (response) {
                                        if (!response) {
                                            if ($scope.ETAList != undefined) {
                                                if ($scope.ETAList.length != $scope.subexamplemodel.length) {
                                                    alert('ETA date not Confirm. Please Confirm ETA date');
                                                    return false;
                                                }
                                            }
                                            else {
                                                alert('ETA date not Confirm. Please Confirm ETA date');
                                                return false;
                                            }

                                            IsItemNPP = item.NetPurchasePrice > 0;
                                            IsItemPOP = item.POPurchasePrice > 0;

                                            if (IsItemNPP == true && IsItemPOP == true) {
                                                if (selectedWsQty.length == 0) {
                                                    alert('Please fill atleast one warehouse Number of Set');
                                                    return false;
                                                }
                                                else if (item.PRPaymentType == "" || item.PRPaymentType == "undefined" || item.PRPaymentType == null) {
                                                    alert('Please Select PR Payment Type');
                                                    return false;
                                                }
                                                else if (item.DepoId < 0 || item.DepoId == "undefined" || item.DepoId == null) {
                                                    alert('Please Select Depo');
                                                    return false;
                                                }

                                                else {
                                                    var data = true;
                                                    for (var c = 0; c < $scope.POdata.length; c++) {

                                                        if ($scope.POdata[c].ItemId == item.ItemId) {

                                                            data = false;
                                                            break;
                                                        }
                                                    }



                                                    if (data === true) {


                                                        var url = serviceBase + "api/PurchaseOrderNew/GetWarehouseItemById?itemId=" + item.ItemId;
                                                        $http.post(url, WarehouseIds).success(function (result) {


                                                            _.each(result, function (wsItem) {
                                                                var Noofset;
                                                                var packingmterialconversion = 1;
                                                                _.each($scope.warehouseQty, function (wsqty) {
                                                                    if (wsqty.WarehouseId === wsItem.WarehouseId) {
                                                                        Noofset = wsqty.Noofset;

                                                                        var Qty = item.PurchaseMinOrderQty * Noofset;
                                                                        if (wsItem.Caterogy == "C" || wsItem.Caterogy == "D") {
                                                                            if (Qty > wsItem.Qty) {

                                                                                alert(" If C & D Category Item Qty greater than  Sold Qty then  PR approval will to send to Deepak Dhanotiya")
                                                                            }
                                                                        }
                                                                    }

                                                                });



                                                                if ($scope.EligibleQtyForPo != null && $scope.EligibleQtyForPo.QtyForAction < Noofset * Number(item.PurchaseMinOrderQty) && $scope.EligibleQtyForPo.QtyForAction > 0) {
                                                                    alert("you can purchase item qty upto : " + $scope.EligibleQtyForPo.QtyForAction);
                                                                    return false;
                                                                }
                                                                else if ($scope.EligibleQtyForPo != null && $scope.EligibleQtyForPo.QtyForAction < 0) {
                                                                    alert("you can't purchase item  (forecast item)");
                                                                    return false;
                                                                }

                                                                $scope.POdata.push({
                                                                    Itemname: wsItem.itemname,
                                                                    ItemId: wsItem.ItemId,
                                                                    Type: wsItem.Type,
                                                                    Conversionvalue: wsItem.ConversionValue,
                                                                    Noofset: Noofset,
                                                                    PurchaseMinOrderQty: item.PurchaseMinOrderQty,
                                                                    SupplierId: $scope.Supplier.SupplierId,//item.SupplierId,
                                                                    Advance_Amt: item.Advance_Amt,
                                                                    BuyerId: item.PeopleId,
                                                                    WarehouseId: wsItem.WarehouseId,
                                                                    WarehouseName: wsItem.WarehouseName,
                                                                    DepoId: item.DepoId, //by Anushka
                                                                    DepoName: item.DepoName, //by Anushka
                                                                    CashPurchaseName: item.CashPurchaseName,
                                                                    IsCashPurchase: item.IsCashPurchase,
                                                                    price: wsItem.price,
                                                                    NetPurchasePrice: wsItem.NetPurchasePrice,
                                                                    PurchasePrice: wsItem.POPurchasePrice,
                                                                    IsDraft: false,
                                                                    PRType: 1,
                                                                    PickerType: item.PickerType,
                                                                    PRPaymentType: item.PRPaymentType,
                                                                    SupplierCreditDay: item.SupplierCreditDay,
                                                                    Category: wsItem.Caterogy,
                                                                    Qty: wsItem.Qty,
                                                                    IsAdjustmentPo: $scope.chkselct,
                                                                    inventryCount: wsItem.inventryCount,
                                                                    DemandQty: wsItem.DemandQty,
                                                                    OpenPOQTy: wsItem.OpenPOQTy,
                                                                    ETAdate: $scope.ETAList.find(o => o.WHid === wsItem.WarehouseId).EtaDate,
                                                                    WeightType: $scope.itemWeight.weighttype,
                                                                    Weight: $scope.itemWeight.weight,
                                                                    WeightInGram: $scope.itemWeight.WeightInGram,
                                                                    BusinessType: $scope.directsupplier
                                                                });
                                                            });

                                                            item.Noofset = "";
                                                            item.PurchaseMinOrderQty = "";
                                                            item.ItemId = "";
                                                            item.price = 0;
                                                            item.NetPurchasePrice = 0;
                                                        });

                                                    }
                                                    else {
                                                        alert("Item is Already Added");
                                                        item.Noofset = "";
                                                        item.PurchaseMinOrderQty = "";
                                                        item.ItemId = "";
                                                        item.price = 0;
                                                        item.NetPurchasePrice = 0;
                                                    }
                                                }
                                            } else {
                                                alert('NPP or POPrice is 0.Please Update Purchase price');
                                            }

                                        } else {
                                            alert('This item cannot be added due to crossbuying.');
                                            return false;
                                        }
                                    })
                            }
                        }
                    })
            }
            
        };


        $scope.getTotal = function (wId) {

            var total = 0;
            for (var i = 0; i < $scope.POdata.length; i++) {
                var product = $scope.POdata[i];
                if (product.WarehouseId == wId) {
                    total += (product.PurchaseMinOrderQty * product.Noofset * product.PurchasePrice);
                }
            }
            $scope.TotalAmount = total;
            return total;
        }

        $scope.test = function () {

            $scope.ClosePO = false;
            $scope.chkselct == false;
            var message = '';


            $scope.AdvanceOutstandingChecked = [];
            if ($scope.AdvanceOutstanding != undefined) {
                $scope.chkselct == true;
                for (var i = 0; i < $scope.AdvanceOutstanding.length; i++) {
                    if ($scope.AdvanceOutstanding[i].check == true) {
                        var PurchaseOrderId = $scope.AdvanceOutstanding[i].PurchaseOrderId;
                        var Total = $scope.AdvanceOutstanding[i].Total;
                        message += "PO id: " + PurchaseOrderId + " amt: " + Total + "\n";
                        $scope.AdvanceOutstandingChecked.push($scope.AdvanceOutstanding[i]);
                        $scope.ClosePO = true;
                    }
                }
            }
            //alert(message);
            //console.log($scope.AdvanceOutstandingChecked);
        };

        $scope.ExpiryDays = 0 

        $scope.Searchsave = function () {

            var data = $scope.POdata;
            if ($scope.showfreight == false) {
                if ($scope.FreightCharge == undefined || $scope.FreightCharge == null) {
                    alert('Freight Charge required');
                    return false;
                }
                if ($scope.FreightCharge < 0) {
                    alert('Freight Charge can not be negative.');
                    $scope.FreightCharge = 0;
                    return false;
                }
            }
            

            for (var i = 0; i < data.length; i++) {
                data[i].FreightCharge = $scope.FreightCharge;
            }

            if ($scope.AdvanceOutstandingChecked.length > 0) {
                data.push({
                    PRCloseDc: $scope.AdvanceOutstandingChecked
                });
                $scope.chkselct == true;
            }
            data.PRType = 1;
            var url = serviceBase + 'api/PurchaseOrderNew/SavePR';
            if (data.length > 0) {
                $scope.count = 1;
                $http.post(url, data).success(function (result) {

                    if (result.Status) {
                        alert(result.Message);
                        window.location = "#/AdvancePurchaseRequestMaster";
                    }
                    else {
                        //$modalInstance.close(data);
                        alert(result.Message);
                        window.location.reload();
                    }
                });
            } else {
                alert("Please fill data.");
            }
        };

        $scope.SavePR = function (data) {

            $scope.TobePaidAmt = 0;
            $scope.test();
            $scope.msg = "";
            if ($scope.AdvanceOutstandingChecked.length > 0) {
                var chr = "";
                var settleAmount = 0;
                for (var i = 0; i < $scope.AdvanceOutstandingChecked.length; i++) {
                    settleAmount += $scope.AdvanceOutstandingChecked[i].Total;
                }
                if ($scope.TotalAmount <= settleAmount) {
                    data.PRPaymentType = "AdvancePR";
                    $scope.msg = "You are creating Adjustment PO. PR Total amount is less or equal from Closed PO amount. This PR will be treated as paid after Payment Approval ";
                }
                if ($scope.TotalAmount > settleAmount) {
                    $scope.TobePaidAmt = $scope.TotalAmount - settleAmount;
                    if (data.PRPaymentType == "AdvancePR")
                        chr = "will be paid after Advance Payment";
                    else
                        chr = "will be paid after IR Approval";
                    $scope.msg = "You are creating Adjustment PO. PR Total amount is greater than from Closed PO amount. Remaining PR amount " + chr;
                }
                $scope.settleAmount = settleAmount;
                $('#msgModel').modal('show');
            }
            else {
                $scope.Searchsave();
            }

        }
        $scope.saveFinalPR = function () {
            if (confirm("Are you sure want to Process?")) {
                $('#msgModel').modal('hide');
                $scope.Searchsave();
            }
        }
        $scope.Draftsave = function () {
            var data = $scope.POdata;
            data.PRType = 1;
            _.each(data, function (item) {
                item.IsDraft = true;
            });


            var url = serviceBase + 'api/PurchaseOrderNew/SavePR';
            if (data.length > 0)/////
            {
                $scope.count = 1;
                $http.post(url, data).success(function (result) {

                    if (result.Status) {
                        alert(result.Message);
                        window.location = "#/AdvancePurchaseRequestMaster";
                    }
                    else {
                        alert(result.Message);
                        window.location.reload();
                    }
                })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    });
            } else {
                alert("Please fill data.");
            }
        };
        //Remove item from list
        $scope.remove = function (item) {
            var index = $scope.POdata.indexOf(item);
            $scope.POdata.splice(index, 1);
        };
        $scope.currentBuyerId = {};
        $scope.fillbuyer = function (sid) {
            var date = new Date(), y = date.getFullYear(), m = date.getMonth();
       //     debugger;
            var url = serviceBase + 'api/Suppliers/SupDetail?sid=' + sid + '';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyerdata = response;
                    $scope.ExpiryDays = moment(new Date(y, m, date.getDate() + $scope.Buyerdata.ExpiryDays)).format('DD/MM/YYYY') 
					$scope.currentBuyerId = response.PeopleID;
                    $scope.BussinessType = response.bussinessType
                    $scope.checkboxModel = { value1: true }
                    $scope.DirectSup();

                });
        };

        
        //$scope.ds = true
        $scope.dws = false
        $scope.DirectSup = function () {
            debugger;

            if ($scope.BussinessType == "Direct from company") {
                if ($scope.checkboxModel.value1 == true) {
                    $scope.dws = true

                    debugger;

                    $scope.directsupplier = "Direct from company"
                    return false;
                }
                if ($scope.checkboxModel.value1 == false) {
                    $scope.dws=true
                   $scope.directsupplier = "NULL"
                }
               
            }
            else {
                debugger;
                //$scope.ds = false
                $scope.dws = false
                $scope.directsupplier = "NULL" //aartimukati
                
            }
            
        }


        



        $scope.Search = function (key) {
            $scope.data.ItemId = null;
            $scope.data.PurchaseMinOrderQty = null;
            $scope.data.Noofset = null;
            $scope.EligibleQtyForPo = null;
            $scope.itemWeight = null;
            var ids = [];
            _.each($scope.subexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })

            var url = serviceBase + "api/PurchaseOrderNew/SearchinitemPRadd?key=" + key + "&ids=" + ids[0];

            $http.get(url).success(function (data) {

                $scope.itemData = data;
                $scope.idata = angular.copy($scope.itemData);
            })
        };

        $scope.cancel = function () {
            window.location = "#/AdvancePurchaseRequestMaster";
        }

        $scope.getPOdetails = function (SupplierId) {
            var SupplierIdList = [];
            if ($scope.AdvanceOutstandingChecked != undefined) {
                $('#ModelAcion').modal('show');
            }
            else {

                SupplierIdList.push(SupplierId);

                $http.post(serviceBase + "api/PurchaseRequestSettlement/GetAdvanceOutstandingForAdj", SupplierIdList).success(function (data, status) {
                    if (data.length > 0) {
                        $scope.AdvanceOutstanding = data;
                        $('#ModelAcion').modal('show');
                    }
                    else {
                        alert("No Closed Po Exists...");
                    }
                    console.log(data); //ajax request to fetch data into vm.data
                });
            }
            //if ($scope.chkselct == true) {

            //    $('#ModelAcion').modal('show');
            //    var SupplierIdList = [];
            //    SupplierIdList.push(SupplierId);

            //    $http.post(serviceBase + "api/PurchaseRequestSettlement/GetAdvanceOutstandingForAdj", SupplierIdList).success(function (data, status) {

            //        $scope.AdvanceOutstanding = data;

            //        console.log(data); //ajax request to fetch data into vm.data
            //    });
            //}
            //else {
            //    $scope.AdvanceOutstandingChecked = [];
            //    $scope.chkselct = false;
            //    alert('Outstanding Advance Amount is Zero');
            //}
        };


        if (_Whid != null && _Whid !== "undefined" && _Whid !== undefined && SupName != null && SupName !== undefined) {
            $scope.AutoPRbtn = true;
            $scope.isSaveBtnhide = false;
            $scope.supplierData = true;
            $scope.supplierData1 = true;
            $scope.buyerdata = true;
            var prlist = JSON.parse(localStorage.getItem('PRCreateList'));

            if (prlist) {

                $scope.data.supplier = SupName;
                $scope.ETAWhList = [];
                var list = [];
                list = {
                    Whid: _Whid,
                    Whname: prlist[0].WarehouseName //$scope.warehouseAddPO[i].WarehouseName
                }
                $scope.ETAWhList.push(list);

                for (var i = 0; i < prlist.length; i++) {
                    $scope.POdata.push({
                        Itemname: prlist[i].ItemName,
                        ItemId: prlist[i].ItemId,
                        Type: prlist[i].Type,
                        // Conversionvalue: prlist[i].ConversionValue,
                        Noofset: prlist[i].NoofSet,
                        PurchaseMinOrderQty: prlist[i].conversionfactor,
                        SupplierId: prlist[i].SupplierId,
                        Advance_Amt: prlist[i].NoofSet * prlist[i].conversionfactor * prlist[i].POPurchasePrice,
                        BuyerId: prlist[i].BuyerId,
                        WarehouseId: prlist[i].warehouseid,
                        WarehouseName: prlist[i].WarehouseName,
                        //DepoId: item.DepoId, //by Anushka
                        //DepoName: item.DepoName, //by Anushka
                        //CashPurchaseName: item.CashPurchaseName,
                        //IsCashPurchase: item.IsCashPurchase,
                        price: prlist[i].price,
                        NetPurchasePrice: prlist[i].NetPurchasePrice,
                        PurchasePrice: prlist[i].POPurchasePrice,
                        IsDraft: false,
                        PRType: 1,
                        //PickerType: item.PickerType,
                        //PRPaymentType: item.PRPaymentType,
                        //SupplierCreditDay: item.SupplierCreditDay,
                        Category: prlist[i].ABCClassification,
                        Qty: prlist[i].Qty,
                        //IsAdjustmentPo: $scope.chkselct,
                        inventryCount: prlist[i].currentinventory,// wsItem.inventryCount,
                        DemandQty: prlist[i].YesDemand,
                        OpenPOQTy: prlist[i].openpoqty,
                        //ETAdate: $scope.ETAList.find(o => o.WHid === wsItem.WarehouseId).EtaDate
                        YesDemandId: prlist[i].YesDemandId
                    });
                }

                $scope.Suppliers = [];
                var url = serviceBase + "api/Suppliers/searchSupplierForPRadd?key=" + SupName + "&Warehouseid=" + _Whid;
                $http.get(url).success(function (data) {

                    $scope.Supplier = {
                        SupplierId: data.SupplierId,
                        SupplierName: data.Name,
                        PaymentTerms: data.PaymentTerms
                    };
                    //$scope.data = $scope.Supplier.BuyerId;
                    $scope.data.SupplierCreditDay = $scope.Supplier.PaymentTerms;
                    $scope.Suppliers.push($scope.Supplier);
                    $scope.data.SupplierId = $scope.Supplier.SupplierId;

                    $scope.GetDepo($scope.Supplier.SupplierId);

                    $scope.data.PeopleId = prlist[0].BuyerId;
                    $scope.getTotal(_Whid);
                    $scope.test();
                    $scope.subexamplemodel.push({
                        id: _Whid
                    })
                    $scope.data.Cityid = prlist[0].Cityid;
                });

            }

        }

        $scope.BacktoDemand = function () {
            window.location = "#/demandreport";
        }

        $scope.newSave = function (item) {


            if ($scope.ETAList == undefined || $scope.ETAList.length == 0) {
                alert('ETA date not Confirm. Please Confirm ETA date');
                return false;
            }
            else if (item.PRPaymentType == "" || item.PRPaymentType == "undefined" || item.PRPaymentType == null) {
                alert('Please Select PR Payment Type');
                return false;
            }
            else if (item.DepoId < 0 || item.DepoId == "undefined" || item.DepoId == null) {
                alert('Please Select Depo');
                return false;
            }
            else if (item.PickerType < 0 || item.PickerType == "undefined" || item.PickerType == null) {
                alert('Please Select Picker Type');
                return false;
            }

            for (var i = 0; i < $scope.POdata.length; i++) {
                $scope.POdata[i].DepoId = item.DepoId;
                $scope.POdata[i].DepoName = item.DepoName;
                $scope.POdata[i].PickerType = item.PickerType;
                $scope.POdata[i].PRPaymentType = item.PRPaymentType;
                $scope.POdata[i].SupplierCreditDay = item.SupplierCreditDay;
                $scope.POdata[i].ETAdate = $scope.ETAList.find(o => o.WHid === _Whid).EtaDate;
            }
            $scope.Searchsave();

            localStorage.removeItem('PRCreateList');
        }

    }]);


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AdvancePurchaseDetailsController', AdvancePurchaseDetailsController);

    AdvancePurchaseDetailsController.$inject = ['$scope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', '$routeParams'];

    function AdvancePurchaseDetailsController($scope, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $routeParams) {

        $scope.POId = $routeParams.id;
        $scope.PRID = $routeParams.PRId;
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = {};
        $scope.PurchaseOrderData = [];

        $scope.PurchaseOrderData = {};
        getPODetail($scope.POId);
        getPRDetail($scope.PRID);


        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });

        function getPRDetail(id) {
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPRWithDetial?id=' + id).then(function (results) {
                $scope.PurchaseOrderData = results.data;
                $scope.PurchaseorderDetails = results.data.PurchaseOrderRequestDetail;
                $scope.totalfilterprice = 0;

                $scope.SupWareDepData();
                _.map($scope.PurchaseorderDetails, function (obj) {
                    $scope.totalfilterprice = $scope.totalfilterprice + ((obj.Price) * (obj.TotalQuantity));
                })
                //$scope.callmethod();
            });
        }
        $scope.AdvanceAmountDetails = {};
        $scope.GetAdvanceAmount = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetAdvanceAmount?PurchaseOrderId=' + $scope.PRID;
            $http.get(url)
                .success(function (data) {
                    $scope.AdvanceAmountDetails = data;
                });

        };
        $scope.GetAdvanceAmount();

        $scope.ClosedPODetails = {};
        $scope.GetClosedPOAmount = function () {

            var id = '';
            if ($scope.PRID != undefined) {
                id = $scope.PRID
            }
            else {
                id = $scope.POId
            }
            var url = serviceBase + 'api/PurchaseOrderNew/GetClosedPOAmount?PurchaseOrderId=' + id;
            $http.get(url)
                .success(function (data) {
                    $scope.ClosedPODetails = data;
                });
        };
        $scope.GetClosedPOAmount();
        console.log($scope.ClosedPODetails);
        function getPODetail(id) {
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {

                $scope.PurchaseOrderData = results.data;
                $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                $scope.totalfilterprice = 0;

                $scope.SupWareDepData();
                _.map($scope.PurchaseorderDetails, function (obj) {
                    $scope.totalfilterprice = $scope.totalfilterprice + ((obj.Price) * (obj.TotalQuantity));
                })
                $scope.callmethod();
            });
        }

        $scope.SupplierData = {};
        $scope.WarehouseData = {};
        $scope.DepoData = {};
        $scope.SupWareDepData = function () {
            $scope.SupplierData = {};
            $scope.WarehouseData = {};
            $scope.DepoData = {};
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {

                $scope.SupplierData = results.data;

            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {

                $scope.WarehouseData = results.data;

            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {

                $scope.DepoData = results.data;

            }, function (error) {
            });

        }


        $scope.Buyer = {};
        $scope.getpeopleAsBuyer = function () {
            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };
        $scope.getpeopleAsBuyer();

        $scope.GetUserRole = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetuserRole';
            $http.get(url)
                .success(function (data) {
                    $scope.Role = data;
                });
        };
        $scope.GetUserRole();

        // display depo in invoice by Anushka
        $scope.GetDepo = function (data) {

            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {

                $scope.fillbuyer(data);

                $scope.datadepomaster = response;
                console.log($scope.datadepomaster);
            })
                .error(function (data) {
                })
        }

        // for Approved PO 
        $scope.sendapproval = function (data) {
            var status = data;
            var url = serviceBase + "api/PRdashboard/sendtoReviewerNew";
            $http.put(url, status).success(function (response) {
                alert("Send to Reviewer.")
                window.location = "#/PRApprover&PRReviewer";

            });
        };


        $scope.savechangebuyer = function (obj) {

            var url = serviceBase + 'api/PurchaseOrderNew/savechangebuyer';
            var dataToPost = {
                PurchaseOrderId: $scope.POId,
                PeopleID: obj.BuyerId
            };
            $http.put(url, dataToPost)
                .success(function (response) {
                    alert("Buyer changed.");
                    window.location.reload();
                }).error(function (data) {
                    alert("Something went wrong.")
                });
        };

        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.PurchaseorderDetails,

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

                $scope.numPerPageOpt = [20, 50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }

        //----------------------------------------------------------------------------------------------------

        $scope.kot = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "kot.html",
                    controller: "Kotpopupctrls", resolve: { object: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                })
        };

        $scope.downloadpdf = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "printpdf.html",
                    controller: "printPdf", resolve: { object: function () { return $scope.PurchaseOrderData } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });

        };
        //-----------------------------------------------------------------------------------------------------

        $scope.open = function () {
            var modalInstance;
            var data = {}
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "PurchaseOrdeADDController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.edit = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myEditmodal.html",
                    controller: "PurchaseOrdeEditController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.removeitem = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myRemovemodal.html",
                    controller: "PurchaseOrdeRemoveItemController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.invoice = function (invoice) {

            console.log("in invoice Section");
            console.log(invoice);
        };

        /// save and send to approval PO from Draft.
        $scope.Save = function (data) {

            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/addPofromDraft';
            $http.post(url, data).success(function (result) {
                console.log("Error Got Here");
                console.log(data);
                window.location = "#/AdvancePurchaseRequestMaster";
            })
        };
        /// send to supplier app.
        $scope.Send = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendToSuppApp?pid=' + $scope.purchasedetail.PurchaseOrderId;
            $http.get(url).success(function (result) {
                alert("Send to supplier app.");
                window.location = "#/AdvancePurchaseRequestMaster";
            })
        };

        /// send pdf to supplier mail.
        $scope.SendPdf = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendPdf?pid=' + $scope.purchasedetail.PurchaseOrderId;
            $http.get(url).success(function (result) {
                alert("Send Pdf to supplier.");
                window.location = "#/AdvancePurchaseRequestMaster";
            })
        };

        $scope.reSendPdf = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myResendmodal.html",
                    controller: "PurchaseOrdeResendPdfController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.LockPO = function (condition) {

            var PO = $scope.PurchaseOrderData;
            var dataToPost = {
                PurchaseOrderId: PO.PurchaseOrderId,
                Condition: condition
            }
            console.log(dataToPost);
            var url = serviceBase + "api/PurchaseOrderList/isPoLockOrNot";
            $http.put(url, dataToPost).success(function (data) {
                $scope.data = $scope.PurchaseList;
                alert("Ok.");
            }).error(function (data) {
                alert("Failed.");
            })
        };
    }
})();

app.controller("ModalInstanceCtrlCancel", ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$modalInstance', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'canceldata', function ($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $modalInstance, $http, ngAuthSettings, $filter, ngTableParams, $modal, canceldata) {


    $scope.data = canceldata;

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.CancelPR = function (data) {

            if (data.Comment != null && data.Comment != undefined && data.Comment != "") {

                $('#myOverlay').show();

                var url = serviceBase + "api/PurchaseOrderNew/PRCancel";
                var dataToPost = {
                    PurchaseOrderId: canceldata.PurchaseOrderId,
                    Comment: data.Comment
                };

                $http.post(url, dataToPost)
                    .success(function (data) {
                        $('#myOverlay').hide();
                        if (data.id == 0) {
                            alert("something Went wrong ");
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;

                            }
                        }
                        else {
                            alert('Cancel PR Succefully');
                            window.location.reload();
                        }
                    })
                    .error(function (data) {
                        $modalInstance.close();
                    });
            }
            else {
                alert("Please Enter Comment");
            }
        };

}]);


(function () {
    'use strict';

    angular
        .module('app')
        .controller('PRCloneController', PRCloneController);

    PRCloneController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function PRCloneController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.IRMasterDc = [];
        if (object) {
            $scope.saveData = object;
        }

        $scope.Clone = function (data) {

            var dataToPost = {
                PurchaseOrderId: data.PurchaseOrderId
            };
            var url = serviceBase + "api/PurchaseOrderNew/ClonePRtoPR";
            $http.post(url, dataToPost).success(function (response) {
                alert(response.Message);
                if (response.Status) {
                    $modalInstance.dismiss('canceled');
                    window.location = "#/AdvancePurchaseRequestMaster";
                }
            });

        };

        $scope.Ok = function () {
            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };
    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('POCloneController', POCloneController);

    POCloneController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function POCloneController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.IRMasterDc = [];
        if (object) {
            $scope.saveData = object;
        }

        $scope.Clone = function (data) {

            var dataToPost = {
                PurchaseOrderId: data.PurchaseOrderId
            };
            var url = serviceBase + "api/PurchaseOrderNew/ClonePOtoPR";
            $http.post(url, dataToPost).success(function (response) {
                alert(response.Message);
                if (response.Status) {
                    $modalInstance.dismiss('canceled');
                    window.location = "#/AdvancePurchaseRequestMaster";
                }
            });

        };

        $scope.Ok = function () {
            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };
    }
})();
