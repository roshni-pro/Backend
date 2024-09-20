
(function () {
    // 'use strict';

    angular
        .module('app')
        .controller('CurrentStockController', CurrentStockController);

    CurrentStockController.$inject = ['CurrentStockService', 'WarehouseService', '$modal', '$scope', "$filter", "$http", "ngTableParams", "$interval"];

    function CurrentStockController(CurrentStockService, WarehouseService, $modal, $scope, $filter, $http, ngTableParams, $interval) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        //$scope.zones = [];
        //$scope.GetZones = function () {
        //    var url = serviceBase + 'api/inventory/GetZone';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.zones = response;
        //        });
        //};
        //$scope.GetZones();

        //$scope.regions = [];
        //$scope.GetRegions = function (zone) {
        //    var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.regions = response;
        //        });
        //};

        //$scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //};

        {
            console.log(" Current Stock Controller reached");
            $scope.Warehouseid = 1;
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.compid = $scope.UserRole.compid;
            $scope.userid = $scope.UserRole.userid;
            //User Tracking
            //$scope.AddTrack = function (Atype, page, Detail) {

            //    console.log("Tracking Code");
            //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            //    $http.post(url).success(function (results) { });
            //}
            //End User Tracking
            $scope.getWarehosues = function () { // This would fetch the data on page change.
                //In practice this should be in a factory.
                WarehouseService.getwarehouse().then(function (results) {
                    $scope.warehouse = results.data;
                    $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                    $scope.CityName = $scope.warehouse[0].CityName;
                    $scope.Warehousetemp = angular.copy(results.data);

                    $scope.getcurrentstock($scope.WarehouseId);
                }, function (error) {
                });
            };
            $scope.getWarehosues();
            $scope.currentPageStores = {};
            $scope.Exportstock = [];
            $scope.Getstock = [];

            $scope.getcurrentstock = function (WarehouseId) {
                $scope.Exportstock = [];
                $scope.Getstock = [];
                $scope.currentPageStores = [];
                CurrentStockService.getstockWarehousebased(WarehouseId).then(function (results) {
                    
                    var id = parseInt(WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.WarehouseId === id;
                    });


                    $scope.CityName = $scope.filterData[0].CityName;
                    $scope.Getstock = results.data;
                    $scope.Exportstock = angular.copy(results.data);
                    $scope.callmethod();
                }, function (error) {
                });
            };

            //var c = 0;
            //$scope.message = "page refreshed " + c + " time.";
            //$interval(function ()
            //{
            //    

            //    $scope.message = "refreshed done" + c + " time.";
            //    c++;
            //}, 1000);

            //*************************************************************************************************************//
            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }
            $scope.UploaderExportDiffStock = function () {
                for (var i = 0; i < $scope.Exportstock.length; i++) {

                    $scope.Exportstock[i].DiffStock = 0;
                    $scope.Exportstock[i].Reason = "";

                }

                console.log($scope.Exportstock);
                alasql('SELECT ItemNumber,ItemMultiMRPId,StockId,itemname,DiffStock,Reason,WarehouseName INTO XLSX("Currentstock.xlsx",{headers:true}) FROM ?', [$scope.Exportstock]);
            };

            $scope.exportData = function () {

                alasql('SELECT ItemNumber,ItemMultiMRPId,itemname,CurrentInventory,PlannedStock,WarehouseName INTO XLSX("Currentstock.xlsx",{headers:true}) FROM ?', [$scope.stores]);

            };
            //***************************************************************************************************************
            $scope.uploadshow = true;
            $scope.toggle = function () {
                $scope.uploadshow = !$scope.uploadshow;
            }


            console.log("Vikash start");
            var rowCount = 0;


            $(document).ready(function () {
                var obj = $("#dragandrophandler");
                obj.on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    $(this).css('border', '2px solid #0B85A1');
                });
                obj.on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                obj.on('drop', function (e) {
                    $(this).css('border', '2px dotted #0B85A1');
                    e.preventDefault();
                    var files = e.originalEvent.dataTransfer.files;
                    //We need to send dropped files to Server
                    handleFileUpload(files, obj);
                });
                $(document).on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                $(document).on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    obj.css('border', '2px dotted #0B85A1');
                });
                $(document).on('drop', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
            });
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

            //-----for popupdial-----
            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;

            $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [20, 30, 90, 100];  //dropdown options for no. of Items per page

            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;

            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

            //$scope.$on('$viewContentLoaded', function () {
            //    $scope.oldStocks($scope.pageno);
            //});

            $scope.StockId = 0;
            $scope.oldStock = function (data) {

                $scope.ItemNumber = data.ItemNumber;
                $scope.WarehouseId = data.WarehouseId;
                $scope.StockId = data.StockId;
                $scope.oldStocks($scope.pageno);
            }

            $scope.oldStocks = function (pageno) {

                $scope.OldStockData = [];
                var url = serviceBase + "api/CurrentStock" + "?ItemNumber=" + $scope.ItemNumber + "&list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId + "&StockId=" + $scope.StockId;
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
            //for manual inventory
            $scope.edit = function (item) {
                console.log("Edit Dialog called survey");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myInventoryModalPut.html",
                        controller: "ModalInstanceCtrlInventoryedit", resolve: { inventory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.Getstock.push(selectedItem);
                    _.find($scope.Getstock, function (inventory) {
                        if (inventory.StockId == selectedItem.StockId) {
                            inventory = selectedItem;
                        }
                    });
                    $scope.Getstock = _.sortBy($scope.Getstock, 'StockId').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };
            $scope.transferstock = function (item) {
                console.log("transfer Dialog called survey");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myInventoryModaltransfer.html",
                        controller: "ModalInstanceCtrlInventorytransfer", resolve: { inventory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.Getstock.push(selectedItem);
                    _.find($scope.Getstock, function (inventory) {
                        if (inventory.StockId == selectedItem.StockId) {
                            inventory = selectedItem;
                        }
                    });
                    $scope.Getstock = _.sortBy($scope.Getstock, 'StockId').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            //
            $scope.TransfertoFreeStock = function (item) {
                
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "TransfertoFreeStockModal.html",
                        controller: "TransfertoFreeStockController", resolve: { inventory: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.Getstock.push(selectedItem);
                    _.find($scope.Getstock, function (inventory) {
                        if (inventory.StockId == selectedItem.StockId) {
                            inventory = selectedItem;
                        }
                    });
                    $scope.Getstock = _.sortBy($scope.Getstock, 'StockId').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };






            $scope.HistoryexportData = function (StockId) {
                
                $scope.exportDataRecord = [];

                var url = serviceBase + "api/CurrentStock/Export" + "?StockId=" + StockId + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {

                    $scope.exportDataRecord = response;
                    alasql('SELECT ItemMultiMRPId,StockId,ItemNumber,itemname,ManualInventoryIn,InventoryIn,InventoryOut,DamageInventoryOut,PurchaseInventoryOut,OrderCancelInventoryIn,TotalInventory,OdOrPoId,WarehouseName,UserName,CreationDate INTO XLSX("CurrentItemstockHistory.xlsx",{headers:true}) FROM ?', [$scope.exportDataRecord]);
                })
                    .error(function (data) {

                    })
            }

            function sendFileToServer(formData, status) {

                formData.append("WareHouseId", $scope.WarehouseId);
                formData.append("compid", $scope.compid);
                formData.append("userid", $scope.userid);
                var uploadURL = "/api/currentstockupload/post"; //Upload URL
                var extraData = {}; //Extra Data.
                var jqXHR = $.ajax({
                    xhr: function () {
                        var xhrobj = $.ajaxSettings.xhr();
                        if (xhrobj.upload) {
                            xhrobj.upload.addEventListener('progress', function (event) {
                                var percent = 0;
                                var position = event.loaded || event.position;
                                var total = event.total;
                                if (event.lengthComputable) {
                                    percent = Math.ceil(position / total * 100);
                                }
                                //Set progress
                                status.setProgress(percent);
                            }, false);
                        }
                        return xhrobj;
                    },
                    url: uploadURL,
                    type: "POST",
                    contentType: false,
                    processData: false,
                    cache: false,
                    data: formData,
                    success: function (data) {
                        alert(data);
                        $("#status1").append("Data from Server:" + data + "<br>");
                        window.location.reload();

                    }
                });

                status.setAbort(jqXHR);
            }
            function createStatusbar(obj) {
                rowCount++;
                var row = "odd";
                if (rowCount % 2 == 0) row = "even";
                this.statusbar = $("<div class='statusbar " + row + "'></div>");
                this.filename = $("<div class='filename'></div>").appendTo(this.statusbar);
                this.size = $("<div class='filesize'></div>").appendTo(this.statusbar);
                this.progressBar = $("<div class='progressBar'><div></div></div>").appendTo(this.statusbar);
                this.abort = $("<div class='abort'>Abort</div>").appendTo(this.statusbar);
                obj.after(this.statusbar);

                this.setFileNameSize = function (name, size) {
                    var sizeStr = "";
                    var sizeKB = size / 1024;
                    if (parseInt(sizeKB) > 1024) {
                        var sizeMB = sizeKB / 1024;
                        sizeStr = sizeMB.toFixed(2) + " MB";
                    }
                    else {
                        sizeStr = sizeKB.toFixed(2) + " KB";
                    }

                    this.filename.html(name);
                    this.size.html(sizeStr);
                }
                this.setProgress = function (progress) {
                    var progressBarWidth = progress * this.progressBar.width() / 100;
                    this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
                    if (parseInt(progress) >= 100) {
                        this.abort.hide();
                    }
                }
                this.setAbort = function (jqxhr) {
                    var sb = this.statusbar;
                    this.abort.click(function () {
                        jqxhr.abort();
                        sb.hide();
                    });
                }
            }
            function handleFileUpload(files, obj) {
                for (var i = 0; i < files.length; i++) {
                    var fd = new FormData();
                    fd.append('file', files[i]);
                    var status = new createStatusbar(obj); //Using this we can set progress.
                    status.setFileNameSize(files[i].name, files[i].size);
                    sendFileToServer(fd, status);
                }
            }
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlInventoryedit', ModalInstanceCtrlInventoryedit);

    ModalInstanceCtrlInventoryedit.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function ModalInstanceCtrlInventoryedit($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        console.log("Iventory");
        $scope.isDisable = false;
        $scope.xy = true;
        $scope.inventoryData = {};
        //User Tracking
        //$scope.AddTrack = function (Atype, page, Detail) {

        //    console.log("Tracking Code");
        //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
        //    $http.post(url).success(function (results) { });
        //}
        //End User Tracking
        if (inventory) {

            console.log("category if conditon");
            $scope.inventoryData = inventory;
        }
        $scope.updatelineitem = function (data) {

            $scope.inventoryData.CurrentInventory = data.CurrentInventory - 1;
        }
        $scope.updatelineitem1 = function (data) {
            $scope.inventoryData.CurrentInventory = data.CurrentInventory + 1;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Putinventory = function (data) {

            if ($scope.inventoryData.ManualReason != null) {

                var url = serviceBase + "api/CurrentStock/PUT";
                var dataToPost = {
                    CurrentInventory: $scope.inventoryData.CurrentInventory,
                    StockId: $scope.inventoryData.StockId,
                    ManualReason: $scope.inventoryData.ManualReason,
                };
                console.log(dataToPost);
                $http.put(url, dataToPost)
                    .success(function (data) {

                        $scope.AddTrack("Edit(CurrentStock)", "ManualEdit: StockId", dataToPost.StockId);
                        $modalInstance.close(data);
                    })
                    .error(function (data) {
                        console.log(data);
                    })
                $scope.isDisable = true;
            }
            else {

                alert('please enter reason for change Qty');
            }
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('EmptyCurrentController', EmptyCurrentController);

    EmptyCurrentController.$inject = ['CurrentStockService', 'WarehouseService', '$modal', '$scope', "$filter", "$http", "ngTableParams"];

    function EmptyCurrentController(CurrentStockService, WarehouseService, $modal, $scope, $filter, $http, ngTableParams) {

        $scope.currentPageStores = {};
        $scope.Getstock = [];
        //User Tracking
        //$scope.AddTrack = function (Atype, page, Detail) {

        //    console.log("Tracking Code");
        //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
        //    $http.post(url).success(function (results) { });
        //}
        //End User Tracking

        CurrentStockService.getEmptystock().then(function (results) {

            $scope.Getstock = results.data;
            console.log("orders..........");
            console.log($scope.Getstock);
            $scope.callmethod();
            $scope.AddTrack("View", "EmptyStockItem:", "");
        }, function (error) {
        });

        $scope.checkAll = function () {

            if ($scope.selectedAll) {
                $scope.selectedAll = false;
            } else {
                $scope.selectedAll = true;
            }
            angular.forEach($scope.Getstock, function (trade) {
                trade.check = $scope.selectedAll;
            });

        };

        $scope.getselected = function (data1) {

            $scope.assignedCusts = []
            for (var i = 0; i < data1.length; i++) {
                if (data1[i].check == true) {
                    var cs = {
                        StockId: data1[i].StockId,
                    }
                    $scope.assignedCusts.push(cs);
                }
            }
            if ($scope.assignedCusts.length > 0) {
                $http.post(serviceBase + "api/CurrentStock/SelectedItem", $scope.assignedCusts).then(function (results) {
                    alert("Added");
                    window.location.reload();
                }, function (error) {
                    alert("Error Got Heere is ");
                })
            } else {
                alert("Please select checkBox");
            }
        }


        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.Getstock,

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

                $scope.numPerPageOpt = [30, 50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }
        //-----for popupdial-----
        // new pagination 
        $scope.pageno = 1; //initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 30;  //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [30, 60, 90, 100];  //dropdown options for no. of Items per page
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
        }
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown
        $scope.$on('$viewContentLoaded', function () {
            $scope.oldStocks($scope.pageno);
        });
        $scope.StockId = 0;
        $scope.oldStock = function (data) {
            
            $scope.StockId = data.StockId;
            $scope.oldStocks($scope.pageno);
        }
        $scope.oldStocks = function (pageno) {
            
            $scope.OldStockData = [];
            var url = serviceBase + "api/CurrentStock" + "?StockId=" + $scope.StockId + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
            $http.get(url).success(function (response) {
                $scope.OldStockData = response.ordermaster;
                $scope.total_count = response.total_count;
                console.log($scope.OldStockData);

            })
                .error(function (data) {
                })
        }
    }
})();

// Add For this Controller  transfer stock

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlInventorytransfer', ModalInstanceCtrlInventorytransfer);

    ModalInstanceCtrlInventorytransfer.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function ModalInstanceCtrlInventorytransfer($scope, $http, ngAuthSettings, $modalInstance, inventory) {
        console.log("Iventory");
        $scope.isDisable = false;
        $scope.xy = true;
        $scope.inventoryData = {};


        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        if (inventory) {

            console.log("category if conditon");
            $scope.inventoryData = inventory;
            $scope.key = $scope.inventoryData.itemBaseName;
            $scope.currinventory = $scope.inventoryData.CurrentInventory;
        }

        //get for search Based Item Master
        $scope.idata = {};
        $scope.Search = function (key) {

            var url = serviceBase + "api/CurrentStock/WidCurrentStock?key=" + key + "&WarehouseId=" + $scope.inventoryData.WarehouseId;
            $http.get(url).success(function (data) {

                $scope.itemData = data;
                $scope.idata = angular.copy($scope.itemData);
            })
        };

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Transferinventory = function (Inventorystock, ItemData, ManualReason) {
            //   
            $scope.itemdetails = JSON.parse(ItemData);
            if ($scope.inventoryData.CurrentInventory < Inventorystock) {

                alert('please Quantity less than current Quantity ');
            }
            if (ManualReason == null && ManualReason == undefined) {
                alert('please enter the reason ');
            }
            var url = serviceBase + "api/CurrentStock/StockTransfer";
            var dataToPost = {
                CurrentInventory: Inventorystock,
                WarehouseId: $scope.inventoryData.WarehouseId,
                ItemNumber: $scope.inventoryData.ItemNumber,
                ItemMultiMRPId: $scope.inventoryData.ItemMultiMRPId,
                ItemNumberTrans: $scope.itemdetails.ItemNumber,
                ManualReason: ManualReason,
                ItemMultiMRPIdTrans: $scope.itemdetails.ItemMultiMRPId
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {

                    $modalInstance.close(data);
                })
                .error(function (data) {
                    alert(data.ErrorMessage);
                })
            $scope.isDisable = true;

        }

    }
})();


// Add For this Controller  transfer to freestock
(function () {
    'use strict';

    angular
        .module('app')
        .controller('TransfertoFreeStockController', TransfertoFreeStockController);

    TransfertoFreeStockController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "inventory"];

    function TransfertoFreeStockController($scope, $http, ngAuthSettings, $modalInstance, inventory) {

        //End User Tracking
        if (inventory) {
            $scope.inventory = inventory;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.TransfertoFreeStock = function (Transferinventory, ManualReason) {

            if (Transferinventory && ManualReason) {
                if ($scope.inventory.CurrentInventory < Transferinventory) {
                    alert('please Quantity less than current Quantity ');
                    return false;
                }
                var dataToPost =
                {
                    ItemNumber: $scope.inventory.ItemNumber,
                    ItemMultiMRPId: $scope.inventory.ItemMultiMRPId,
                    WarehouseId: $scope.inventory.WarehouseId,
                    Transferinventory: Transferinventory,
                    ManualReason: ManualReason
                }
                var url = serviceBase + "api/CurrentStock/TransferToFreeStock";
                console.log(dataToPost);
                $http.put(url, dataToPost)
                    .success(function (data) {
                        alert(data);
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        alert(data);

                    })

            }
            else {
                alert("enter reason and Transfer qty ");
            }


        }
    }
})();