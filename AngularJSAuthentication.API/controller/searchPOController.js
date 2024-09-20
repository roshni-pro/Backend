
(function () {
    'use strict';
    angular
        .module('app')
        .controller('searchPOController', searchPOController);

    searchPOController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function searchPOController($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
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
        };
        //$scope.Warehouseid = 1;
        var currentWarehouse = localStorage.getItem('currentWarehouse');

        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse)
        }

        $scope.getWarehosues = function () {

            //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                    $scope.WarehouseId = $scope.Warehouseid;
                    $scope.Warehousetemp = angular.copy(response);
                    $scope.getData1($scope.pageno);
                }, function (error) {
                })


            //WarehouseService.getwarehouse().then(function (results) {
            //    $scope.warehouse = results.data;
            //    $scope.WarehouseId = $scope.Warehouseid;
            //    //$scope.CityName = $scope.warehouse[0].CityName;
            //    $scope.Warehousetemp = angular.copy(results.data);

            //    $scope.getData1($scope.pageno);

            //}, function (error) {
            //})
        };
        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
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

        $scope.getWarehosues();

        $scope.Buyer = {};
        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyer = response;
                });
        };
        $scope.getpeople();
        $scope.currentBuyerId = {};
        $scope.fillbuyer = function (sid) {

            var url = serviceBase + 'api/Suppliers/SupDetail?sid=' + sid + '';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyerdata = response;
                    $scope.currentBuyerId = response.PeopleID;

                });
        };
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
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);// just seeeion 
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId;
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

        // display depo in invoice by Anushka
        $scope.GetDepo = function (data) {

            $scope.fillbuyer(data);
            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {
                $scope.datadepomaster = response;
                console.log($scope.datadepomaster);
            }).error(function (data) {
            })
        }


        ////////////////////////////////////////////
        //$scope.currentPageStores = {};
        // $scope.Porders = [];
        //SearchPOService.getPorders().then(function (results) {
        //    $scope.Porders = results.data;
        //    console.log("orders..........");
        //    console.log($scope.Porders);
        //    $scope.callmethod();
        //}, function (error) {
        //});
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
            
            SearchPOService.save(data).then(function (results) {                
            }, function (error) {
            });
        };


        $scope.invoice = function (data) {
          
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
            
            $scope.PurchaseOrderData = PurchaseorderDetails;
            var url = serviceBase + 'api/IR/IsIgst?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.PurchaseOrderData.IsIgstIR = data;
                SearchPOService.IRopen($scope.PurchaseOrderData).then(function (results) {
                    console.log("master invoice fn");
                    console.log(results);
                }, function (error) {
                });
            });
        };
        $scope.idata = {};
        $scope.Search = function (key) {


            var url = serviceBase + "api/itemMaster/SearchinitemPOadd?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (data) {

                $scope.itemData = data;
                $scope.idata = angular.copy($scope.itemData);
            })
        };
        $scope.iidd = 0;
        $scope.Minqtry = function (key) {
            $scope.itmdata = [];
            $scope.iidd = Number(key);
            for (var c = 0; c < $scope.idata.length; c++) {
                if ($scope.idata.length != null) {
                    if ($scope.idata[c].ItemId == $scope.iidd) {
                        $scope.itmdata.push($scope.idata[c]);
                    }
                }
                else {
                }
            }
        }
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



        $scope.openmodel = function (data) { // calling same controller here, its not good.

            $scope.supplierData = false;
            $scope.supplierData1 = false;
            $scope.Depo = false;//by Anushka
            $scope.DepoH = false;//by Anushka
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
            if ($scope.vm.Excelupload == false) {
                var url = serviceBase + 'api/PurchaseOrderList/checkNpp?ItemId=' + item.ItemId + '&WarehouseId=' + item.WarehouseId + '';
                $http.get(url).success(function (result) {
                    IsItemNPP = result;
                    console.log(IsItemNPP)

                    if (IsItemNPP == true) {
                        if (item.Noofset == undefined || item.Noofset == "") {
                            alert('Please fill Number of Set')
                        }
                        else if (item.Noofset == undefined || item.Noofset == "") {
                            alert('Please fill PurchaseMinOrderQty')
                        }
                        else if (item.Noofset == undefined || item.Noofset == "") {
                            alert('Please fill Item Name')
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
                            if (data == true) {
                                $scope.POdata.push({
                                    Itemname: itemname,
                                    ItemId: item.ItemId,
                                    Noofset: item.Noofset,
                                    PurchaseMinOrderQty: item.PurchaseMinOrderQty,
                                    SupplierId: item.SupplierId,
                                    Advance_Amt: item.Advance_Amt,
                                    BuyerId: item.PeopleId,
                                    WarehouseId: item.WarehouseId,
                                    DepoId: item.DepoId, //by Anushka
                                    DepoName: item.DepoName, //by Anushka
                                    CashPurchaseName: item.CashPurchaseName,
                                    IsCashPurchase: item.IsCashPurchase

                                });
                                item.Noofset = "";
                                item.PurchaseMinOrderQty = "";
                                item.ItemId = "";
                            }
                            else {
                                alert("Item is Already Added");
                                item.Noofset = "";
                                item.PurchaseMinOrderQty = "";
                                item.ItemId = "";
                            }
                        }
                    } else {
                        alert('NPP is 0.');
                    }
                });

            } else {
                //$scope.POdata.push(item);
                for (var j = 0; j < $scope.ItemInfo.length; j++) {

                    if ($scope.Supplier.SupplierName != $scope.ItemInfo[j].SupplierName) {
                        alert('Please fill Correct supplier name in excel')
                        return;
                    }
                }

                for (var i = 0; i < $scope.ItemInfo.length; i++) {
                    $scope.POdata.push({
                        Itemname: $scope.ItemInfo[i].Itemname,
                        ItemId: $scope.ItemInfo[i].ItemId,
                        Noofset: $scope.ItemInfo[i].Noofset,
                        PurchaseMinOrderQty: $scope.ItemInfo[i].PurchaseMinOrderQty,
                        SupplierId: item.SupplierId,
                        Advance_Amt: $scope.ItemInfo[i].Advance_Amt,
                        BuyerId: item.PeopleId,
                        WarehouseId: item.WarehouseId,
                        DepoId: item.DepoId, //by Anushka
                        DepoName: item.DepoName, //by Anushka
                        CashPurchaseName: item.CashPurchaseName,
                        IsCashPurchase: item.IsCashPurchase,
                    });
                }
            }
            //end import excel

        };
        $scope.Searchsave = function () {
           
            var data = $scope.POdata;
            var url = serviceBase + 'api/PurchaseOrderList/addPo';
            if (data.length != 0)/////
            {
                $scope.count = 1;
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
                        alert('PO Done');
                        window.location.reload()
                    }
                })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            } else {
                alert("Please fill data.");
            }
        };
        $scope.Draftsave = function () {

            var data = $scope.POdata;
            var url = serviceBase + 'api/PurchaseOrderList/addPodraft';
            if (data.length != 0)/////
            {
                $scope.count = 1;
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
                        alert('PO Save as draft');
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
                    if ($scope.warehouse[i].WarehouseId == o2.id) {
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
                var dataToPost = {
                    "From": start,
                    "TO": end,
                    ids: ids
                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                       
                        $scope.POM = data;
                        alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,ItemNumber,ItemName,QtyRecivedTotal,PriceRecived,QtyRecived1,Price1,Gr1Date,QtyRecived2,Price2,Gr2Date,QtyRecived3,Price3,Gr3Date,QtyRecived4,Price4,Gr4Date,QtyRecived5,Price5,Gr5Date,SupplierName,Status,POItemFillRate,TotalTAT,AverageTAT INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);

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
        $scope.Excelupdata = function () {
            $scope.vm.Excelupload = !$scope.vm.Excelupload;

        };
        // Start import excel add po
        $scope.SelectFile = function (file) {

            $scope.SelectedFile = file;
        };
        $scope.Upload = function () {
            var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xls|.xlsx)$/;
            if (regex.test($scope.SelectedFile.name.toLowerCase())) {
                if (typeof (FileReader) != "undefined") {
                    var reader = new FileReader();
                    //For Browsers other than IE.
                    if (reader.readAsBinaryString) {
                        reader.onload = function (e) {
                            $scope.ProcessExcel(e.target.result);
                        };
                        reader.readAsBinaryString($scope.SelectedFile);
                    } else {
                        //For IE Browser.
                        reader.onload = function (e) {
                            var data = "";
                            var bytes = new Uint8Array(e.target.result);
                            for (var i = 0; i < bytes.byteLength; i++) {
                                data += String.fromCharCode(bytes[i]);
                            }
                            $scope.ProcessExcel(data);
                        };
                        reader.readAsArrayBuffer($scope.SelectedFile);
                    }
                } else {
                    $window.alert("This browser does not support HTML5.");
                }
            } else {
                $window.alert("Please upload a valid Excel file.");
            }
        };

        $scope.ProcessExcel = function (data) {
            //Read the Excel File data.
            var workbook = XLSX.read(data, {
                type: 'binary'
            });

            //Fetch the name of First Sheet.
            var firstSheet = workbook.SheetNames[0];

            //Read all rows from First Sheet into an JSON array.
            var excelRows = XLSX.utils.sheet_to_row_object_array(workbook.Sheets[firstSheet]);

            //Display the data from Excel file in Table.
            $scope.$apply(function () {

                $scope.ItemInfo = excelRows;
                $scope.IsVisible = true;

            });
        };
        //End Excel upload
        $scope.downloadexcelformat = function () {

            $scope.storesitem = [];
            alasql('SELECT ItemId,Itemname,Noofset,PurchaseMinOrderQty,Advance_Amt,SupplierName INTO XLSX("POItemList.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

        };
        $scope.Pochecker = function () {

            window.location = "#/GRApproval";

        };


        //Remove item from list
        $scope.remove = function (item) {
            var index = $scope.POdata.indexOf(item);
            $scope.POdata.splice(index, 1);
        };


    }
})();
