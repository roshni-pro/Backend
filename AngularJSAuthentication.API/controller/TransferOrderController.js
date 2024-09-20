'use strict';
app.controller('TransferOrderController', ['$scope', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'localStorageService', '$rootScope', function ($scope, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modal, localStorageService, $rootScope) {

    $scope.dataPeopleHistrory;
    //Get Selecte rout
    var path = window.location.hash.substring(2);
    //Get list
    var url = serviceBase + "api/Menus/GetButtons?submenu=" + path;
    $http.get(url).success(function (response) {
        $scope.dataPeopleHistrory = response;
        console.log($scope.dataPeopleHistrory);
    })
    $scope.PermissionSet = JSON.parse(localStorage.getItem('PermissionSet'));
    //Function For History//
    $scope.TransferSendHistroy = function (data) {
        $scope.datatransferHistrory = [];
        var url = serviceBase + "api/TransferOrder/TransferHistory?transferOrderId=" + data.TransferOrderId;
        $http.get(url).success(function (response) {
            $scope.datatransferHistrory = response;
            console.log($scope.datatransferHistrory);
            //   $scope.AddTrack("View(History)", "TransferOrderId:", data.TransferOrderId);
        })
            .error(function (data) {
            })
    }

    //End..Function//
    $scope.refresh = function () {

        window.location.reload();
        //$scope.currentPageStores = $scope.itemMasters;
        //$scope.pagenoOne = 0;
        //$scope.getData1($scope.pageno);
    };
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A',
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });
    });
    //$scope.Warehouseid = 0;
    //$scope.getAllwarehouse = function () {

    //    console.log("in warehouse service");
    //    var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
    //    $http.get(url)
    //        .success(function (results) {
    //            $scope.Allwarehouse = results;
    //            $scope.RequestWH = results;
    //            console.log($scope.Allwarehouse);
    //            console.log(results);

    //        }, function (error) {
    //        })
    //};
    //$scope.getAllwarehouse();
    //$scope.getWarehosues = function () {

    //    //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
    //    var url = serviceBase + 'api/Warehouse';
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.warehouse = response;
    //            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
    //            if ($scope.Warehouseid > 0) { $scope.getData1($scope.pageno); }

    //            //$scope.getData1($scope.pageno);
    //        }, function (error) {
    //        })
    //};

    $scope.Warehouseid = [];
    //$scope.getWarehosues = function () {

    //    console.log("in warehouse service");
    //    var url = serviceBase + 'api/Warehouse/GetWarehouseWOKPP';
    //    $http.get(url)
    //        .success(function (results) {
    //            $scope.warehouse = results;
    //            console.log("abcc", $scope.warehouse)
    //            //$scope.RequestWH = results;
    //            //console.log($scope.warehouse);
    //            //console.log(results);

    //        }, function (error) {
    //        })
    //};

    $scope.getWarehosues = function () {
        var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
        $http.get(url)
            .success(function (data) {
                $scope.warehouse = data;
            });

    };
    /* $scope.getWarehosues();*/





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

        if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
            alert("Please select atleast 1 Warehouse");
            return;
        }
        //debugger;
        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
    }

    //$scope.getWarehosues();
    $scope.getWarehosues();
    $scope.pagenoOne = 0;
    $scope.pageno = 1; // initialize page no to 1
    $scope.total_count = 0;
    $scope.numPerPageOpt = [100];//dropdown options for no. of Items per page
    $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
    $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selectedPagedItem;
        $scope.getData1($scope.pageno);
    }
    $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
    $scope.currentPageStores = {};

    $scope.getData1 = function (pageno, Status, TransferOrdeId) {
        //debugger;

        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
        console.log("wareidddd", $scope.Warehouseid)

        Search(pageno, Status, TransferOrdeId);
    }

    function Search(pageno, Status, TransferOrdeId) {
        //debugger;
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        if ($scope.Warehouseid > 0) {
            //debugger;
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();


            if (!$('#dat').val()) {
                start = "";
                end = "";


            }
            if (Status === undefined || Status == "" || Status == null) {
                Status = "";
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            //debugger;
            var url = serviceBase + "api/TransferOrder" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            $http.get(url).success(function (response) {
                //debugger;
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.callmethod();
            });
        }
    }

    //Added by Anoop 25/2/2021
    //$rootScope.$on("CallParentMethod", function () {
    //    $scope.getData1();
    //});
    $scope.totalC = 0
    $scope.totalExport = [];
    $scope.Porders = [];
    $scope.getData1 = function (pageno, Status, TransferOrdeId) { // This would fetch the data on page change.
        debugger;
        $scope.getWareitemMaster();
        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
        console.log("wareidddd", $scope.Warehouseid)


        // var wid = $rootScope.getAddTO;
        //$rootScope.getAddTO = dataom.WarehouseId;
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        //if (wid != null && wid != undefined) {
        //    $scope.Warehouseid = wid;
        //}
        if ($scope.Warehouseid != undefined) {
            debugger;
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();

            if (Status == true) {
                $scope.statusname = null;
                Status = "";
                start = null;
                end = null;
            }

            if (!$('#dat').val()) {
                start = null;
                end = null;


            }
            if (Status === undefined || Status == "" || Status == null) {
                Status = null;
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/TransferOrder" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            $http.get(url).success(function (response) {
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.totalC = response.total_count;
                $scope.totalExport = $scope.itemMasters;
                $scope.callmethod();
            });
        }
    };
    $scope.Porderss = [];
    $scope.ExportSend = function (pageno, Status, TransferOrdeId) {
        debugger;
        if ($scope.totalC == undefined || $scope.totalC == 0) {
            alert("Please search the data first")
            return false;
        }

        $scope.getWareitemMaster();
        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
        console.log("wareidddd", $scope.Warehouseid)


        // var wid = $rootScope.getAddTO;
        //$rootScope.getAddTO = dataom.WarehouseId;
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        //if (wid != null && wid != undefined) {
        //    $scope.Warehouseid = wid;
        //}

        if ($scope.Warehouseid != undefined) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();

            if (Status == true) {
                $scope.statusname = null;
                Status = "";
                start = null;
                end = null;
            }

            if (!$('#dat').val()) {
                start = null;
                end = null;


            }
            if (Status === undefined || Status == "" || Status == null) {
                Status = null;
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            //$scope.itemMasters = [];
            $scope.Porderss = [];

            var url = serviceBase + "api/TransferOrder/GetTransferOrderMastersendExport" + "?list=" + $scope.totalC + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            $http.get(url).success(function (response) {

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                //$scope.total_count = response.total_count;
                $scope.Porderss = $scope.itemMasters;
                console.log("vnwkkk", $scope.Porderss)
                $scope.callmethod();

                alasql('SELECT TransferOrderId,Type,Status,WarehouseName,RequestAmount,DispatchAmount,CreationDate,VehicleNo,RequestQty,DispatchQty INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porderss]);
            });
        }


        //// alasql('SELECT TransferOrderId,Status,WarehouseName,CreationDate INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porders]);
        ////   alasql('SELECT TransferOrderId,Status,WarehouseName,CreationDate INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porders]);
        //debugger;
        //$scope.Porders = [];
        //var url = serviceBase + "api/TransferOrder" + "?list=" + $scope.total_count + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
        //$http.get(url).success(function (response) {
        //    //debugger;
        //    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
        //    $scope.Porders = $scope.itemMasters;
        //    console.log("get current Page items:");
        //    alasql('SELECT TransferOrderId,Status,WarehouseName,RequestAmount,DispatchAmount,CreationDate,VehicleNo INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porders]);

        //    //$scope.total_count = response.total_count;
        //});

    }

    $scope.Export = function (pageno, Status, TransferOrdeId) {

        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        if ($scope.Warehouseid > 0) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();


            if (!$('#dat').val()) {
                start = null;
                end = null;


            }
            if (Status === undefined) {
                Status = "";
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/TransferOrder" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            $http.get(url).success(function (response) {
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                alasql('SELECT TransferOrderId,Status,WarehouseName,CreationDate,RequestQty,DispatchQty INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porders]);
            });
        }
    }
    //*****************************Download Excel Format************************************//
    alasql.fn.myfmt = function (n) {
        return Number(n).toFixed(2);
    }
    $scope.exportExcelData = function () {
        alasql('SELECT ItemNumber,itemname,TotalQuantity,RequestToWarehouseId INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.uploadshow]);
    };

    //***************************************************************//
    //*****************Warehouse to warehouse Sheet Upload Function****************//
    // Function END
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
            $scope.numPerPageOpt = [100],
            $scope.numPerPage = $scope.numPerPageOpt[0],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            $scope.currentPageStores = $scope.stores;
        (init = function () {
            return $scope.search(), $scope.select($scope.currentPage)
        });

    }
    $scope.open = function (data) {

        console.log("Modal opened Role");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myTODetailModal.html",
                controller: "TransferOrderDetailController", resolve: { data: function () { return data } }
            }), modalInstance.result.then(function (selectedItem) { debugger; })
    };
    $scope.openDispatchedDetail = function (data) {

        console.log("Modal opened Role");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myTODispatchedDetailModal.html",
                controller: "TransferOrderDispatchedDetailController", resolve: { data: function () { return data } }
            }), modalInstance.result.then(function (selectedItem) { })
    };

    $scope.openmodel = function (data) {

        $scope.supplierData = false;
        $scope.supplierData1 = false;
        console.log("Modal opened Role");
        var modalInstance;

        modalInstance = $modal.open(
            {
                //backdrop: 'static',
                templateUrl: "mySearchModal.html",
                controller: "CreateTransferOrderController", resolve: { role: function () { return $scope.data } }
            }), modalInstance.result.then(function (selectedItem) { })
    };
    $scope.ok = function () { modalInstance.close(); },
        $scope.cancel = function () { modalInstance.dismiss('Canceled'); },
        $scope.POdata = [];
    $scope.SeledtedWid = {};
    $scope.whselected = false;
    $scope.searchdata = function (data) {

        if (data !== "") {
            var url = serviceBase + "api/TransferOrder/SearchTransferOrder?key=" + data;
            $http.get(url).success(function (response) {

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                $scope.currentPageStores = response.ordermaster;

            });
        }
        else {
            $scope.callmethod();
        }
    };


}]);

app.controller('TransferOrderDetailController', ['$scope', 'data', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", "$modalInstance", '$modal', '$window', function ($scope, data, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modalInstance, $modal, $window) {
    $scope.fread = 0;
    $scope.TODetails = [];
    $scope.TOMaster = data;
    var DetailId = data.TransferOrderId;
    var WarehouseIdd = data.Warehouseid;


    $scope.getMaster = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/TransferOrder/DispatchedMaster?masterId=" + DetailId;
        $http.get(url).success(function (response) {

            $scope.TOMaster = response.result; //ajax request to fetch data into vm.data
            console.log("get current Page items:");
        });
    };
    $scope.getMaster();

    $scope.showEwayBill = false;
    $scope.getData1 = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/TransferOrder/Detail" + "?DetailId=" + DetailId + "&Warehouseid=" + WarehouseIdd;
        $http.get(url).success(function (response) {
            //transferordersend
            $scope.TODetails = response; //ajax request to fetch data into vm.data
            $scope.TransferTotalPrice = response[0].TotalPrice;         //Vinayak
            $scope.InterStateLimit = response[0].InterStateLimit;
            $scope.IntraStateLimit = response[0].IntraStateLimit;
            for (var i = 0; i < $scope.TODetails.length; i++) {
                //var xboxes = $scope.TODetails[i].UnitofQuantity / $scope.TODetails[i].PurchaseMinOrderQty; TotalQuantity
                if ($scope.TODetails[i].DispatchedQty == 0) {
                    var xboxes = $scope.TODetails[i].TotalQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                    var xpieces = $scope.TODetails[i].TotalQuantity % $scope.TODetails[i].PurchaseMinOrderQty;
                    //var xboxes = xboxes.toFixed(1);
                    var str = xboxes.toString();
                    var numarray = str.split('.');
                    $scope.TODetails[i].Boxes = numarray[0];
                    $scope.TODetails[i].piece = xpieces;
                } else {
                    var xboxes = $scope.TODetails[i].DispatchedQty / $scope.TODetails[i].PurchaseMinOrderQty;
                    var xpieces = $scope.TODetails[i].DispatchedQty % $scope.TODetails[i].PurchaseMinOrderQty;
                    //var xboxes = xboxes.toFixed(1);
                    var str = xboxes.toString();
                    var numarray = str.split('.');
                    $scope.TODetails[i].Boxes = numarray[0];
                    $scope.TODetails[i].piece = xpieces;
                }
            }
            console.log("get current Page items:");

            if ($scope.TOMaster == null) { $scope.fread = $scope.TODetails[0].fread; } else { $scope.fread = $scope.TOMaster.fread; }

        });
    };
    $scope.getData1();

    $scope.showTOSendInvoice = function (data) {
        $scope.OrderDetail = data;
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myModaldeleteTOrderInvoice.html",
                controller: "ModalInstanceCtrlTOrderInvoice",
                resolve:
                {
                    OrderDetail: function () {
                        return data
                    }
                }
            }), modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condition");
                })
    };

    $scope.Delivered = function (orderDetail) {
        $scope.count = 1;
        $("#DiliveredId").prop("disabled", true);
        var url = serviceBase + 'api/TransferOrder/DeliveredOrder';
        if (orderDetail[0].Status == "Dispatched") {
            if ($window.confirm("Please confirm?")) {
                $http.post(url, orderDetail).success(function (result) {

                    alert('Received Done');
                    window.location.reload();
                })
                    .error(function (data) {
                        alert(data.ErrorMessage);
                        window.location.reload();
                    });
            } else { $("#DiliveredId").prop("disabled", false); }
        }
        else {
            alert("Data already exist.");
            window.location.reload();
        }
    }
    $scope.Rejected = function (orderDetail) {

        $("#DiliveredId").prop("disabled", true);

        var url = serviceBase + 'api/TransferOrder/AddRejectedOrder';

        $http.post(url, orderDetail).success(function (result) {
            console.log("Error Got Here");
            alert('Order Rejected');
            window.location.reload();
        })
            .error(function (data) {

                alert(data);
                window.location.reload();
                console.log("Error Got Heere is ");
                console.log(data);
                // return $scope.showInfoOnSubmit = !0, $scope.revert()
            })
    }
    $scope.cancelAndaddingstock = function (orderDetail) {

        $scope.count = 1;
        var url = serviceBase + 'api/TransferOrder/CancelOrder';
        if ($window.confirm("Please confirm?")) {
            $http.post(url, orderDetail).success(function (result) {
                console.log("Error Got Here");
                alert('TTransfer Order Canceled Done');
                window.location.reload();
            });
        } else { console.log("Error Got Here"); }
    }
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); }
    $scope.RevokeOrder = function (orderDetailData) {
        $("#DispechedId").prop("disabled", true);

        var url = serviceBase + 'api/TransferOrder/RevokeOrder?TransferOrderId=' + orderDetailData[0].TransferOrderId + '';

        if ($window.confirm("Please confirm?")) {
            $http.get(url).success(function (result) {
                alert(result);
                window.location.reload();
            }).error(function (data) {
                alert(data.ErrorMessage);
                window.location.reload();

            });
        } else { $("#DispechedId").prop("disabled", false); }
    }

}]);

app.controller('TransferOrderDispatchedDetailController', ['$scope', 'data', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", "$modalInstance", '$modal', function ($scope, data, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modalInstance, $modal) {

    $scope.TODetails = [];
    $scope.TOMaster = data;
    var DetailId = data.TransferOrderId;
    var WarehouseIdd = data.WarehouseId;
    $scope.getData1 = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/TransferOrder/DispatchedDetail" + "?DetailId=" + DetailId + "&Warehouseid=" + WarehouseIdd;
        $http.get(url).success(function (response) {
            //debugger;
            $scope.TODetails = response; //ajax request to fetch data into vm.data
            console.log("get current Page items:");
        });
    };
    $scope.getData1();
    $scope.Delivered = function (orderDetail) {
        $scope.count = 1;
        $("#DiliveredId").prop("disabled", true);
        var url = serviceBase + 'api/TransferOrder/DeliveredOrder';
        if (orderDetail[0].Status == "Dispatched") {
            $http.post(url, orderDetail).success(function (result) {
                console.log("Error Got Here");
                alert('Received Done');
                window.location.reload();
            })
                .error(function (data) {
                    alert(data);
                    window.location.reload();
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        } else {
            alert("Data already exist.");
            window.location.reload();
        }
    }
    $scope.Rejected = function (orderDetail) {

        $("#DiliveredId").prop("disabled", true);

        var url = serviceBase + 'api/TransferOrder/AddRejectedOrder';

        $http.post(url, orderDetail).success(function (result) {
            console.log("Error Got Here");
            alert('Order Rejected');
            window.location.reload();
        })
            .error(function (data) {

                alert(data);
                window.location.reload();
                console.log("Error Got Heere is ");
                console.log(data);
                // return $scope.showInfoOnSubmit = !0, $scope.revert()
            })
    }
    $scope.cancelAndaddingstock = function (orderDetail) {

        $scope.count = 1;
        var url = serviceBase + 'api/TransferOrder/CancelOrder';
        $http.post(url, orderDetail).success(function (result) {
            console.log("Error Got Here");
            alert('TTransfer Order Canceled Done');
            window.location.reload();
        });
    }

    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);

app.controller('TransferOrderRequestController', ['$scope', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', function ($scope, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

    $scope.pageno = 1; // initialize page no to 1
    $scope.pagenoo = 1;
    $scope.refresh = function () {
        window.location.reload();

        //$scope.statusname = null;
        //$scope.currentPageStores = $scope.itemMasters;
        //$scope.pagenoOne = 0;
        //$scope.getData1($scope.pageno);
    };


    $scope.Warehouseid = [];
    //$scope.getWarehosues = function () {

    //    console.log("in warehouse service");
    //    var url = serviceBase + 'api/Warehouse/GetWarehouseWOKPP';
    //    $http.get(url)
    //        .success(function (results) {
    //            $scope.warehouse = results;
    //            console.log("abcc", $scope.warehouse)
    //            //$scope.RequestWH = results;
    //            //console.log($scope.warehouse);
    //            //console.log(results);

    //        }, function (error) {
    //        })
    //};

    $scope.getWarehosues = function () {
        var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
        $http.get(url)
            .success(function (data) {
                $scope.warehouse = data;
            });

    };
    /* $scope.wrshse();*/




    $scope.MultiWarehouseModel = [];
    $scope.MultiWarehouse = $scope.warehouse;
    $scope.MultiWarehouseModelsettings = {
        displayProp: 'label', idProp: 'value',
        scrollableHeight: '450px',
        scrollableWidth: '550px',
        enableSearch: true,
        scrollable: true
    };
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A',
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });
    });

    $scope.wid = '';
    //$scope.getWareitemMaster = function (data) 
    //$scope.getWareitemMaster = function () {


    //    if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
    //        alert("Please select atleast 1 Warehouse");
    //        return;
    //    }
    //    $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
    //}

    //$scope.getWarehosues = function () {

    //    //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
    //    var url = serviceBase + 'api/Warehouse';
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.warehouse = response;
    //            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
    //            $scope.CityName = $scope.warehouse[0].CityName;
    //            if ($scope.Warehouseid > 0) { $scope.getData1($scope.pageno); }


    //        }, function (error) {
    //        })
    //};
    $scope.getWarehosues();
    //$scope.getWareitemMaster();
    $scope.pagenoOne = 0;
    $scope.total_count = 0;
    $scope.numPerPageOpt = [100];//dropdown options for no. of Items per page
    $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
    $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selectedPagedItem;
        // $scope.getData1($scope.pageno);
    };
    $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
    $scope.currentPageStores = {};
    $scope.Porders = [];
    $scope.totalC = 0
    $scope.totalExport = [];

    $scope.getData1 = function (pageno, Status, TransferOrdeId) {

        //if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
        //    alert("Please select atleast 1 Warehouse");
        //    return;
        //}

        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
        if ($scope.Warehouseid.length == 0) {
            alert("Please Select atleast 1 Warehouse");
            return false;
        }
        console.log("wareidddd", $scope.Warehouseid)
        // This would fetch the data on page change.
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        if ($scope.Warehouseid != undefined) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();

            if (Status == true) {
                $scope.statusname = null;
                Status = "";
                start = null;
                end = null;
            }

            if (!$('#dat').val()) {
                start = null;
                end = null;


            }
            if (Status === undefined || Status == "" || Status == null) {
                Status = null;
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/TransferOrder/GetRequests" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;

            $http.get(url).success(function (response) {
                var om = response
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.totalExport = $scope.itemMasters;
                $scope.totalC = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.callmethod();
            });
        }
    };
    /*$scope.getData1($scope.pageno);*/
    $scope.pagenoOne = 0;
    $scope.pordersss = [];

    $scope.ExportRequest = function (pagenoo, Status, TransferOrdeId) {
        debugger
        if ($scope.totalC == undefined || $scope.totalC == 0) {
            alert("Please search the data first")
            return false;
        }



        console.log($scope.Porders);
        $scope.Warehouseid = $scope.MultiWarehouseModel.map(a => a.id);
        if ($scope.Warehouseid.length == 0) {
            alert("Please Select atleast 1 Warehouse");
            return false;
        }
        console.log("wareidddd", $scope.Warehouseid)
        // This would fetch the data on page change.
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        if ($scope.Warehouseid != undefined) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();

            if (Status == true) {
                $scope.statusname = null;
                Status = "";
                start = null;
                end = null;
            }

            if (!$('#dat').val()) {
                start = null;
                end = null;


            }
            if (Status === undefined || Status == "" || Status == null) {
                Status = null;
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            /*$scope.pagenoOne = pagenoo;*/
            $scope.itemMasters = [];
            $scope.pordersss = [];
            debugger
            //var url = serviceBase + "api/TransferOrder/GetRequests" + "?list=" + $scope.totalC + "&page=" + pagenoo + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            var url = serviceBase + "api/TransferOrder/GetRequestsExport" + "?list=" + $scope.totalC + "&page=" + pagenoo + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            $http.get(url).success(function (response) {
                var om = response
                debugger
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.pordersss = $scope.itemMasters;
                $scope.callmethod(); 
                console.log($scope.pordersss)
                alasql('SELECT TransferOrderId,Status,RequestToWarehouseName,RequestAmount,DispatchAmount,CreationDate,VehicleNo,DispatchQty,RequestQty,Type INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.pordersss]);
            });
        }



    }

    // order History get data 
    $scope.TransferHistroy = function (data) {
        debugger
        $scope.datatransferHistrory = [];
        var url = serviceBase + "api/TransferOrder/TransferHistory?transferOrderId=" + data.TransferOrderId;
        $http.get(url).success(function (response) {

            $scope.datatransferHistrory = response;
            console.log($scope.datatransferHistrory);
            $scope.AddTrack("View(History)", "TransferOrderId:", data.TransferOrderId);
        })
            .error(function (data) {
            })
    }
    //export pdf
    $scope.EwayBillPDF = function (data) {
        debugger
        $scope.ewaybill = 0;
        $scope.ewaybill = parseInt(data.EwaybillNumber)
        if (data.EwaybillNumber != 0) {
            var myArray = [];
            {
                myArray.push(data.EwaybillNumber);
            }
            var DataForPdf = {
                OrderId: data.TransferOrderId,
                ewb_numbers: myArray,
                Apitypes: 2,
            }
            console.log(DataForPdf)
         /*   $("#DispechedId").prop("disabled", true);*/
            var url = serviceBase + 'api/Ewaybill/GetEWaybillPDF';
            //{
            $http.post(url, DataForPdf).success(function (result) {
                if (result.status) {
                    // console.log(serviceBase + result.EwayBillPDF)
                    window.open(serviceBase + result.EwayBillPDF);
                }
                else {
                    alert("EwayBill Pdf Not Generated!");
                }
            })
                .error(function (data) {
                    alert(data.ErrorMessage);
                    window.location.reload();

                });
        }
        else {
            alert("No Eway Bill Number")
        }
    }
    //end order History 
    $scope.TransferItem = function (TransferOrderId) {
        $scope.datatransferitem = [];
        var url = serviceBase + "api/TransferOrder/TransferItem?transferOrderId=" + TransferOrderId;
        $http.get(url).success(function (response) {

            $scope.datatransferitem = response;
            console.log($scope.datatransferitem);
            $scope.AddTrack("View(History)", "TransferOrderId:", data.TransferOrderId);
        })
            .error(function (data) {
            })
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
            $scope.numPerPageOpt = [100],
            $scope.numPerPage = $scope.numPerPageOpt[0],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            $scope.currentPageStores = $scope.stores;
        (init = function () {
            return $scope.search(), $scope.select($scope.currentPage)
        })
    };
    $scope.open = function (data) {

        console.log("Modal opened Role");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myTODetailModal.html",
                controller: "TransferOrderDetailController", resolve: { data: function () { debugger; return data } }
            }), modalInstance.result.then(function (selectedItem) { })
    };


    $scope.openmodel = function (data) {

        $scope.supplierData = false;
        $scope.supplierData1 = false;
        console.log("Modal opened Role");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "mySearchModal.html",
                controller: "TransferOrderController", resolve: { role: function () { return data } }
            }), modalInstance.result.then(function (selectedItem) { })
    };
    $scope.ok = function () {
        $scope.$modalInstance.close();
    };
    $scope.cancel = function () {
        $scope.$modalInstance.dismiss('cancel');
    };


    $scope.showDetail = function (data) {
        console.log("Modal opened Orderdetails");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myTODetailModal.html",
                controller: "TransferOrderDetailOpenController", resolve: { data: function () { return data } }
            }), modalInstance.result.then(function (selectedItem) { })
        console.log("Order Detail Dialog called ...");

    };

    $scope.searchdata = function (data) {
        if (data != "") {
            var url = serviceBase + "api/TransferOrder/SearchTransferOrderRequest?key=" + data;
            $http.get(url).success(function (response) {

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                $scope.currentPageStores = response.ordermaster;

            });
        }
        else {
            $scope.callmethod();
        }
    };
    $scope.pageno = 1;
    $scope.Export = function (pageno, Status, TransferOrdeId) {
        //debugger;
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");

        });
        if ($scope.Warehouseid > 0) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();

            var end = g.val();


            if (!$('#dat').val()) {
                start = null;
                end = null;

            }
            if (Status === undefined) {
                Status = "";
            }

            if (TransferOrdeId === undefined || TransferOrdeId == "" || TransferOrdeId == null) {
                TransferOrdeId = 0;
            }
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            // alasql('SELECT TransferOrderId,Status,WarehouseName,CreationDate INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.itemMasters]);

            //var url = serviceBase + "api/TransferOrder/GetRequests" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + Status + "&TransferOrderId=" + TransferOrdeId;
            //$http.get(url).success(function (response) {
            //    debugger;
            //    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
            //    console.log("get current Page items:");
            //    $scope.total_count = response.total_count;
            //    $scope.Porders = $scope.itemMasters;

            //});

            alasql('SELECT TransferOrderId,Status,WarehouseName,CreationDate INTO XLSX("Transferorder.xlsx",{headers:true}) FROM ?', [$scope.Porders]);
        }
    }
}]);
app.controller('BatchCodeOpenController', ['$scope', 'data', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", "$modalInstance", '$modal', '$window', function ($scope, data, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modalInstance, $modal, $window) {

    $scope.TransferOrderItemData = data;
    $scope.TotalQtyEablebutton = true;
    $scope.ItemQty = $scope.TransferOrderItemData.TotalQuantity;
    $scope.ItemName = $scope.TransferOrderItemData.itemname;
    $scope.GetStockBatchMastersList = [];
    $scope.ok = function () {
        $scope.$modalInstance.close();
    };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); }

    $scope.getbatchcode = function () {
        var url = serviceBase + "api/CurrentStock/GetStockBatchMastersData?ItemMultiMRPId=" + $scope.TransferOrderItemData.ItemMultiMRPId + "&WarehouseId=" + $scope.TransferOrderItemData.RequestToWarehouseId + "&stockType=" + "C";
        $http.get(url)
            .success(function (results) {

                if (results.length > 0 && $scope.TransferOrderItemData.BatchITransferDetailDc.length > 0) {

                    for (var i = 0; i < results.length; i++) {
                        for (var j = 0; j < $scope.TransferOrderItemData.BatchITransferDetailDc.length; j++) {
                            if ($scope.TransferOrderItemData.BatchITransferDetailDc[j].StockBatchMasterId == results[i].StockBatchMasterId) {

                                results[i].selectedcheck = true;
                                results[i].check = true;
                                results[i].Noofpics = $scope.TransferOrderItemData.BatchITransferDetailDc[j].Qty;
                            }
                        }
                    }
                }
                $scope.GetStockBatchMastersList = results;

            }, function (error) {
            })
    };
    $scope.getbatchcode();

    $scope.remove = function (item) {

        var index = $scope.GetStockBatchMastersList.indexOf(item);
        for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
            if ($scope.GetStockBatchMastersList[i].StockBatchMasterId == item.StockBatchMasterId) {
                $scope.GetStockBatchMastersList[i].Noofpics = "";
                $scope.GetStockBatchMastersList[i].check = false;
                $scope.GetStockBatchMastersList[i].selectedcheck = false;
                $scope.GetStockBatchMastersList[i].alredyAdded = false;
            }
        }
        for (var i = 0; i < $scope.TransferOrderItemData.BatchITransferDetailDc.length; i++) {
            if ($scope.TransferOrderItemData.BatchITransferDetailDc[i].StockBatchMasterId == item.StockBatchMasterId) {
                var index = $scope.TransferOrderItemData.BatchITransferDetailDc.indexOf($scope.TransferOrderItemData.BatchITransferDetailDc[i]);
                $scope.TransferOrderItemData.IsManualButton = true;
                $scope.TransferOrderItemData.BatchITransferDetailDc.splice(index, 1);
            }
        }
    };
    $scope.checkqty = function (StockBatchMasterId, batchCodeQty, qty, noOfqty, selectedTarde) {

        $scope.TotalQtyEablebutton = true;
        var totalbatchqty = 0;
        $scope.selectedTarde = [];
        $scope.selectedTarde = selectedTarde;
        $scope.selectStockBatchMasterId = Number(StockBatchMasterId);
        if (noOfqty <= batchCodeQty) {
            if (qty < noOfqty) {
                for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                    if ($scope.GetStockBatchMastersList.length != null) {
                        if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                            $scope.GetStockBatchMastersList[c].Noofpics = '';
                            alert('qty should not be greater than Stock qty!!');
                            return false;
                        }
                    }
                }
            } else {
                for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                    if ($scope.GetStockBatchMastersList.length != null) {
                        //if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                        if ($scope.GetStockBatchMastersList[c].check == true && $scope.GetStockBatchMastersList[c].Noofpics > 0) {
                            totalbatchqty = totalbatchqty + $scope.GetStockBatchMastersList[c].Noofpics;
                            if (qty == totalbatchqty) {
                                $scope.TotalQtyEablebutton = false;
                            } else {
                                $scope.TotalQtyEablebutton = true;
                            }
                        }
                        //}
                    }
                }
            }
        } else {
            for (var c = 0; c < $scope.GetStockBatchMastersList.length; c++) {
                if ($scope.GetStockBatchMastersList.length != null) {
                    if ($scope.GetStockBatchMastersList[c].StockBatchMasterId == $scope.selectStockBatchMasterId) {
                        $scope.GetStockBatchMastersList[c].Noofpics = '';
                        alert('qty should not be greater than batch Stock qty!!');
                        return false;
                    }
                }
            }
        }
    }
    $scope.AddDataBatchCode = function (Item, selectedTarde) {

        var totalTOQty = 0;
        var totalBatchAddedQty = 0;
        $scope.TransferOrderItemData.BatchITransferDetailDc = [];
        $scope.isExist = false;
        if (selectedTarde == undefined) {
            alert('Please select batch code!!');
            return false;
        }
        for (var i = 0; i < $scope.GetStockBatchMastersList.length; i++) {
            if ($scope.GetStockBatchMastersList[i].check == true && !$scope.GetStockBatchMastersList[i].alredyAdded) {
                if ($scope.GetStockBatchMastersList[i].Noofpics === "") {
                    alert('Please fill Number of pieces');
                    return false;
                }
                else {
                    if (!$scope.isExist) {
                        $scope.GetStockBatchMastersList[i].selectedcheck = true;
                        $scope.GetStockBatchMastersList[i].alredyAdded = true;
                        $scope.TransferOrderItemData.BatchITransferDetailDc.push({
                            TransferOrderDetailId: Item.TransferOrderDetailId,
                            StockBatchMasterId: $scope.GetStockBatchMastersList[i].StockBatchMasterId,
                            Qty: $scope.GetStockBatchMastersList[i].Noofpics,
                        });
                    }
                }
            }
        }
        //$scope.TransferOrderItemData.TOBatchDetails.forEach(x => {
        //    totalTOQty = totalTOQty + x.TotalQuantity;
        //});
        $scope.GetStockBatchMastersList.forEach(x => {
            if (x.Noofpics) {
                totalBatchAddedQty = totalBatchAddedQty + x.Noofpics;
            }
        });
        if (Item.TotalQuantity == totalBatchAddedQty) {
            $scope.TransferOrderItemData.IsDispechedButton = false;
            $scope.TransferOrderItemData.IsManualButton = false;
        }
        if (!$scope.isExist) {
            $scope.cancel();
        }
    }
}]);
app.controller('TransferOrderDetailOpenController', ['$scope', 'data', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", "$modalInstance", '$modal', '$window', function ($scope, data, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modalInstance, $modal, $window) {
    $scope.isenabled = false;
    $scope.getuserroleforfreight = function () {

        var url = serviceBase + "api/PurchaseOrderNew/GetRole";
        $http.get(url).success(function (data) {
            console.log("data" + data);
            if (data != null) {
                $scope.isenabled = data;
            }
        })
    }
    $scope.getuserroleforfreight();
    //User Tracking
    //End User Tracking
    /* $scope.IsDispechedButton = true;*/

    $scope.TODetails = [];
    $scope.TODispechedDetails = [];
    $scope.TOMasterData = [];
    $scope.WarehouseDetail = [];
    $scope.TOMaster = data;
    $scope.IsNotInterState = false;
    var DetailId = data.TransferOrderId;
    var WarehouseIdd = data.Warehouseid;
    var CityName = data.CityName;
    $scope.IsNotInterState = false;
    $scope.isdateexpired = false;
    $scope.getWarehousedetail = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/Warehouse" + "?id=" + WarehouseIdd;
        $http.get(url).success(function (response) {

            $scope.WarehouseDetail = response; //ajax request to fetch data into vm.data
            console.log("get current Page items:");
        });
    };
    $scope.getWarehousedetail();
    $scope.getTOMasterdetail = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/TransferOrder/TOMaster" + "?TransferOrderId=" + DetailId + "&Warehouseid=" + WarehouseIdd;
        $http.get(url).success(function (response) {
            $scope.TOMasterData = response; //ajax request to fetch data into vm.data
            console.log("get current Page items:");
        });

    };
    $scope.getTOMasterdetail();


    $scope.getData1 = function () { // This would fetch the data on page change.
        debugger
        var url = serviceBase + "api/TransferOrder/Detail" + "?DetailId=" + DetailId + "&Warehouseid=" + WarehouseIdd;
        $http.get(url).success(function (response) {
            console.log(response, "response");
            if (response[0].queryCount > 1) {
                $scope.IsNotInterState = true;
                $scope.TODetails = response;


                for (var i = 0; i < $scope.TODetails.length; i++) {

                    $scope.TODetails[i].IsDispechedButton = true;
                    $scope.TODetails[i].IsManualButton = true;
                    //var xboxes = $scope.TODetails[i].UnitofQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                    if ($scope.TODetails[i].DispatchedQty == 0) {
                        var xboxes = $scope.TODetails[i].TotalQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                        var xpieces = $scope.TODetails[i].TotalQuantity % $scope.TODetails[i].PurchaseMinOrderQty;
                        //var xboxes = xboxes.toFixed(1);
                        var str = xboxes.toString();
                        var numarray = str.split('.');
                        $scope.TODetails[i].Boxes = numarray[0];
                        $scope.TODetails[i].piece = xpieces;
                    } else {
                        $scope.TODetails[i].IsDispechedButton = true;
                        $scope.TODetails[i].IsManualButton = true;
                        var xboxes = $scope.TODetails[i].DispatchedQty / $scope.TODetails[i].PurchaseMinOrderQty;
                        var xpieces = $scope.TODetails[i].DispatchedQty % $scope.TODetails[i].PurchaseMinOrderQty;
                        //var xboxes = xboxes.toFixed(1);
                        var str = xboxes.toString();
                        var numarray = str.split('.');
                        $scope.TODetails[i].Boxes = numarray[0];
                        $scope.TODetails[i].piece = xpieces;
                    }

                }
            }
            else if (response[0].queryCount == 0) {
                alert("Something went Wrong.");
            }
            else {
                $scope.IsNotInterState = false;
                $scope.TODetails = response; //ajax request to fetch data into vm.data
                $scope.TODispechedDetails = response; //ajax request to fetch data into vm.data
                $scope.TransferTotalPrice = response[0].TotalPrice;   /* Implementation vinayak*/

                if ($scope.TransferTotalPrice >= $scope.TODetails[0].InterStateLimit || $scope.TransferTotalPrice >= $scope.TODetails[0].IntraStateLimit) {
                    $scope.showEwayBill = true;
                }
                for (var i = 0; i < $scope.TODetails.length; i++) {
                    $scope.TODetails[i].IsDispechedButton = true;
                    $scope.TODetails[i].IsManualButton = true;
                    //var xboxes = $scope.TODetails[i].UnitofQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                    if ($scope.TODetails[i].DispatchedQty == 0) {
                        var xboxes = $scope.TODetails[i].TotalQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                        var xpieces = $scope.TODetails[i].TotalQuantity % $scope.TODetails[i].PurchaseMinOrderQty;
                        //var xboxes = xboxes.toFixed(0);
                        var str = xboxes.toString();
                        var numarray = str.split('.');
                        $scope.TODetails[i].Boxes = numarray[0];
                        $scope.TODetails[i].piece = xpieces;
                    } else {
                        $scope.TODetails[i].IsDispechedButton = true;
                        $scope.TODetails[i].IsManualButton = true;
                        var xboxes = $scope.TODetails[i].DispatchedQty / $scope.TODetails[i].PurchaseMinOrderQty;
                        var xpieces = $scope.TODetails[i].DispatchedQty % $scope.TODetails[i].PurchaseMinOrderQty;
                        //var xboxes = xboxes.toFixed(0);
                        var str = xboxes.toString();
                        var numarray = str.split('.');
                        $scope.TODetails[i].Boxes = numarray[0];
                        $scope.TODetails[i].piece = xpieces;
                    }
                }
                console.log("get current Page items:");
            }

        });
    };
    $scope.getData1();

    $scope.savechanges = function (fread, id) {
        var url = serviceBase + "api/TransferOrder/UpdateFreightForDelivered" + "?fread=" + fread + "&id=" + id;
        $http.post(url).success(function (data) {
            if (data == true) {
                alert("Updated Successfully");
                window.location.reload();
            }
            else {
                alert("Data Not Found");
                window.location.reload();
            }
        });
    }

    $scope.getdeliverydate = function (fread, id) {
        var url = serviceBase + "api/TransferOrder/GetDeliverdays" + "?id=" + DetailId;
        $http.get(url).success(function (data) {

            if (data == true) {
                $scope.isdateexpired = data;
                //alert("Updated Successfully");

            }
            else {
                $scope.isdateexpired = data;
                //alert("Data Not Found");
            }
        });
    }
    $scope.getdeliverydate();
    $scope.openPopupBatch = function (data) {
        if (!data.BatchITransferDetailDc) {
            data.BatchITransferDetailDc = [];
        }
        if ($scope.TODetails) {
            data.TOBatchDetails = [];
            data.TOBatchDetails = $scope.TODetails;
        }
        if (!data.IsDispechedButton) {
            data.IsDispechedButton = true;
        }
        if (data.IsManualButton) {
            data.IsManualButton = true;
        }
        console.log("Modal opened Orderdetails");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "BatchCodeModal.html",
                controller: "BatchCodeOpenController", resolve: {
                    data: function () { return data }
                }
            }), modalInstance.result.then(function (selectedItem) { })
        console.log("Order Detail Dialog called ...");
    };
    //if (data.IsDispechedButton != undefined) {
    //    $scope.IsDispechedButton = data.IsDispechedButton;
    //}
    //Color Code for CurrentStock
    var s_col = false;
    var del = '';

    var ss_col = '';

    $scope.set_color = function (orderDetailq) {
        if (orderDetailq) {
            //if (ss_col == orderDetail.itemNumber) before multi mrp
            if (ss_col == orderDetailq.ItemMultiMRPId) {
                return { background: "#e08d8d" }
            }
            //set color for red for less inventroy for dispatched
            else if (orderDetailq.TotalQuantity > orderDetailq.CurrentStock) {
                s_col = true;
                return { background: "#e08d8d" }
            }
            else {
                s_col = false;
            }
        }
    };


    $scope.getDispechedData = function () { // This would fetch the data on page change.

        var url = serviceBase + "api/TransferOrder/DispatchedDetail" + "?DetailId=" + DetailId + "&Warehouseid=" + WarehouseIdd;
        $http.get(url).success(function (response) {

            $scope.TODispechedDetails = response; //ajax request to fetch data into vm.data
            /*   $scope.TransferTotalPrice = response[0].TotalPrice;   *//* Implementation vinayak*/

            console.log("get current Page items:");
        });
    };
    $scope.getDispechedData();
    $scope.edit = function (orderDetail) {
        $scope.dataedit = orderDetail;
        console.log("Edit Dialog called survey");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "TranferOrderQuantityedit.html",
                controller: "TranferOrderCtrlQuantityedit", resolve: {
                    order: function () {
                        return orderDetail
                    }
                }
            }), modalInstance.result.then(function (selectedItem) {
                console.log("True Condintion");
            },
                function () {
                    debugger;
                    $scope.TransferTotalPrice = 0;
                    for (var i = 0; i < $scope.TODetails.length; i++) {
                        if ($scope.TODetails[i].DispatchedQty == 0) {
                            var xboxes = $scope.TODetails[i].TotalQuantity / $scope.TODetails[i].PurchaseMinOrderQty;
                            var xpieces = $scope.TODetails[i].TotalQuantity % $scope.TODetails[i].PurchaseMinOrderQty;
                            //var xboxes = xboxes.toFixed(1);
                            var str = xboxes.toString();
                            var numarray = str.split('.');
                            $scope.TODetails[i].Boxes = numarray[0];
                            $scope.TODetails[i].piece = xpieces;
                        } else {
                            var xboxes = $scope.TODetails[i].DispatchedQty / $scope.TODetails[i].PurchaseMinOrderQty;
                            var xpieces = $scope.TODetails[i].DispatchedQty % $scope.TODetails[i].PurchaseMinOrderQty;
                            //var xboxes = xboxes.toFixed(1);
                            var str = xboxes.toString();
                            var numarray = str.split('.');
                            $scope.TODetails[i].Boxes = numarray[0];
                            $scope.TODetails[i].piece = xpieces;
                        }
                        debugger;
                        if (!$scope.TODetails[i].IsCnF) {
                            $scope.TransferTotalPrice += $scope.TODetails[i].PriceofItem;
                        } else {
                            $scope.TransferTotalPrice += $scope.TODetails[i].TotalQuantity * $scope.TODetails[i].UnitPrice;
                        }

                    }

                    for (var i = 0; i < $scope.TODispechedDetails.length; i++) {
                        if ($scope.TODispechedDetails[i].DispatchedQty == 0) {
                            var xboxes = $scope.TODispechedDetails[i].TotalQuantity / $scope.TODispechedDetails[i].PurchaseMinOrderQty;
                            var xpieces = $scope.TODispechedDetails[i].TotalQuantity % $scope.TODispechedDetails[i].PurchaseMinOrderQty;
                            //var xboxes = xboxes.toFixed(1);
                            var str = xboxes.toString();
                            var numarray = str.split('.');
                            $scope.TODetails[i].Boxes = numarray[0];
                            $scope.TODetails[i].piece = xpieces;
                        } else {
                            var xboxes = $scope.TODispechedDetails[i].DispatchedQty / $scope.TODispechedDetails[i].PurchaseMinOrderQty;
                            var xpieces = $scope.TODispechedDetails[i].DispatchedQty % $scope.TODispechedDetails[i].PurchaseMinOrderQty;
                            //var xboxes = xboxes.toFixed(1);
                            var str = xboxes.toString();
                            var numarray = str.split('.');
                            $scope.TODetails[i].Boxes = numarray[0];
                            $scope.TODetails[i].piece = xpieces;
                        }
                        debugger;
                        if (!$scope.TODispechedDetails[i].IsCnF) {
                            $scope.TransferTotalPrice += $scope.TODispechedDetails[i].PriceofItem;
                        } else {
                            $scope.TransferTotalPrice += $scope.TODispechedDetails[i].TotalQuantity * $scope.TODispechedDetails[i].UnitPrice;
                        }

                    }

                    console.log("Cancel Condintion");
                })
    }
    $scope.isPatternValid;
    $scope.Dispeched = function (orderDetail, vehicleNumber, ref, transportername) {
        if (vehicleNumber != undefined || vehicleNumber != '') {

            var trigger = vehicleNumber,
                regexp = new RegExp('^[A-Z]{2}[ -][0-9]{1,2}(?: [A-Z])?(?: [A-Z]*)? [0-9]{4}$'),
                test = regexp.test(trigger);

            if (test == true) {
                $scope.isPatternValid = false;
            }
            else {

                $scope.isPatternValid = true;
                return false;
            }
        }
        if ($scope.IsNotInterState == true) {
            if (orderDetail[0].Status == 'Dispatched' && orderDetail[0].TotalPrice > 50000) {
                if ((orderDetail.EwaybillNumber == undefined || orderDetail.EwaybillNumber == "") && (vehicleNumber == undefined || vehicleNumber == "")) {
                    $scope.isVehicleNo = true;
                    $scope.isEwaybillNo = true;
                    $scope.isPatternValid = true;
                    return;
                }
                if (orderDetail.EwaybillNumber == undefined || orderDetail.EwaybillNumber == "") {
                    $scope.isEwaybillNo = true;
                    $scope.isVehicleNo = false;
                    /*$scope.isPatternValid = false;*/
                    return;
                }
                if (vehicleNumber == undefined || vehicleNumber == "") {
                    $scope.isVehicleNo = true;
                    $scope.isEwaybillNo = false;
                    return;
                }

                if ((orderDetail.EwaybillNumber != undefined || orderDetail.EwaybillNumber != "") && (vehicleNumber != undefined || vehicleNumber != "")) {
                    $scope.isPatternValid = false;
                    $scope.isVehicleNo = false;
                    $scope.isEwaybillNo = false;

                    var dataToPost = {
                        TransferOrderId: orderDetail[0].TransferOrderId,
                        TransferWHOrderDetailss: orderDetail,
                        vehicleType: orderDetail.vehicleType,
                        vehicleNumber: vehicleNumber,
                        fread: orderDetail[0].fread,
                        EwaybillNumber: orderDetail.EwaybillNumber,
                    }
                    $("#DispechedId").prop("disabled", true);
                    var url = serviceBase + 'api/TransferOrder/DispachedOrderUpdated';
                    if ($window.confirm("Please confirm?")) {
                        $http.post(url, dataToPost).success(function (result) {

                            alert(result);
                            window.location.reload();
                        })
                            .error(function (data) {
                                alert(data.ErrorMessage);
                                window.location.reload();
                            });
                    } else { $("#DispechedId").prop("disabled", false); }
                }

            }
        }
        if (orderDetail[0].Status != 'Dispatched') {

            $scope.isPatternValid = false;
            $scope.isEwaybillNo = false;
            $scope.isVehicleNo = false;
            //$scope.isEwaybillNo = false;
            for (var i = 0; i < orderDetail.length; i++) {
                if (orderDetail[i].IsDispechedButton == true) {
                    alert(" Please select Batch code qty for item :  " + orderDetail[i].itemname);
                    return false;
                }
                delete orderDetail[i].TOBatchDetails;
            }
            $scope.PostBatchITransferDetail = []
            for (var i = 0; i < orderDetail.length; i++) {
                if (orderDetail[i].BatchITransferDetailDc && orderDetail[i].BatchITransferDetailDc.length > 0) {
                    for (var j = 0; j < orderDetail[i].BatchITransferDetailDc.length; j++) {
                        $scope.PostBatchITransferDetail.push(orderDetail[i].BatchITransferDetailDc[j]);
                    }
                }
            }
            var dataToPost = {
                TransferOrderId: orderDetail[0].TransferOrderId,
                TransferWHOrderDetailss: orderDetail,
                vehicleType: orderDetail.vehicleType,
                vehicleNumber: vehicleNumber,
                TransporterGstin: ref,
                TransporterName: transportername,
                fread: orderDetail[0].fread,
                EwaybillNumber: orderDetail.EwaybillNumber,
                BatchITransferDetails: $scope.PostBatchITransferDetail

            }
            $("#DispechedId").prop("disabled", true);
            var url = serviceBase + 'api/TransferOrder/DispachedOrder';
            if ($window.confirm("Please confirm?")) {
                $http.post(url, dataToPost).success(function (result) {

                    alert(result);
                    window.location.reload();
                })
                    .error(function (data) {
                        alert(data.ErrorMessage);
                        window.location.reload();

                    });
            } else { $("#DispechedId").prop("disabled", false); }

            //}


        }
        else {

            if (orderDetail[0].Status == 'Dispatched' && orderDetail[0].TotalPrice > 50000) {
                if ((orderDetail.EwaybillNumber == undefined || orderDetail.EwaybillNumber == "") && (vehicleNumber == undefined || vehicleNumber == "")) {
                    $scope.isVehicleNo = true;
                    $scope.isEwaybillNo = true;
                    return;
                }
                if (orderDetail.EwaybillNumber == undefined || orderDetail.EwaybillNumber == "") {
                    $scope.isEwaybillNo = true;
                    $scope.isVehicleNo = false;
                    return;
                }
                if (vehicleNumber == undefined || vehicleNumber == "") {
                    $scope.isVehicleNo = true;
                    $scope.isEwaybillNo = false;
                    return;
                }

                if ((orderDetail.EwaybillNumber != undefined || orderDetail.EwaybillNumber != "") && (vehicleNumber != undefined || vehicleNumber != "") && $scope.isPatternValid == false) {
                    $scope.isVehicleNo = false;
                    $scope.isEwaybillNo = false;
                    var dataToPost = {
                        TransferOrderId: orderDetail[0].TransferOrderId,
                        TransferWHOrderDetailss: orderDetail,
                        vehicleType: orderDetail.vehicleType,
                        vehicleNumber: vehicleNumber,
                        Fread: orderDetail[0].fread,
                        EwaybillNumber: orderDetail.EwaybillNumber,
                    }
                    $("#DispechedId").prop("disabled", true);
                    var url = serviceBase + 'api/TransferOrder/DispachedOrderUpdated';
                    if ($window.confirm("Please confirm?")) {
                        $http.post(url, dataToPost).success(function (result) {

                            alert(result);
                            window.location.reload();
                        })
                            .error(function (data) {
                                alert(data.ErrorMessage);
                                window.location.reload();

                            });
                    } else { $("#DispechedId").prop("disabled", false); }
                }
            }
            else {
                $scope.isEwaybillNo = false;
                $scope.isVehicleNo = false;
                var data = {
                    TransferOrderId: orderDetail[0].TransferOrderId,
                    TransferWHOrderDetailss: orderDetail,
                    vehicleType: orderDetail.vehicleType,
                    vehicleNumber: orderDetail.vehicleNumber,
                    Fread: orderDetail.Fread,
                    EwaybillNumber: orderDetail.EwaybillNumber,
                }
                $("#DispechedId").prop("disabled", true);
                var urls = serviceBase + 'api/TransferOrder/DispachedOrder';
                if ($window.confirm("Please confirm?")) {
                    $http.post(urls, data).success(function (result) {
                        alert(result);
                        window.location.reload();
                    })
                        .error(function (data) {
                            alert(data.ErrorMessage);
                            window.location.reload();

                        });
                } else { $("#DispechedId").prop("disabled", false); }
            }
        }
    }

    $scope.Delivered = function (orderDetail) {

        $("#DiliveredId").prop("disabled", true);

        var url = serviceBase + 'api/TransferOrder/DeliveredOrder';
        $http.post(url, orderDetail).success(function (result) {

            alert(result);
            window.location.reload();
        })
            .error(function (data) {

                alert(result.Message);
                window.location.reload();

            })
    }
    $scope.showInvoice = function (data) {
        $scope.OrderDetail = data;
        console.log(data);

        console.log("Transfer Order Invoice  called ...");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myModaldeleteTOrderInvoice.html",
                controller: "ModalInstanceCtrlTOrderInvoice",
                resolve:
                {
                    OrderDetail: function () {
                        return data
                    }
                }
            }), modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condition");
                })
    };

    $scope.RefNo
    $scope.transportername
    //    $scope.VarifiedCustomerGSTNO = function (data) {
    //        debugger;
    //        console.log(data)
    //        if (data.length == 15 ) {
    //        var url = serviceBase + 'api/RetailerApp/CustomerGSTVerify?GSTNO='+data;
    //        $http.get(url).success(function (response) {
    //            console.log(response);
    //            if (response.Status == true && response.custverify.Active == "Active") {
    //                $scope.GSTCustomerName = response.custverify.Name;
    //            }
    //            else {
    //                $scope.GSTCustomerName = null;
    //                alert("Invalid GST");
    //            }

    //        });
    //        }
    //        else {
    //            alert("Pls enter valid GST/TIN_No/Ref No. For eg. - 23AAVCS1981Q1ZE");
    ///*            this.GSTdisplayy = false;*/
    //        }
    //        //return this.http.get < any > (this.apiURL + 'api/RetailerApp/CustomerGSTVerify?GSTNO=' + GSTNO);
    //    }

    $scope.cancel = function () { $modalInstance.dismiss('canceled'); }

    $scope.Revoke = function (orderDetail) {

        $("#DispechedId").prop("disabled", true);

        var url = serviceBase + 'api/TransferOrder/RevokeOrder?TransferOrderId=' + orderDetail[0].TransferOrderId;

        if ($window.confirm("Please confirm?")) {
            $http.get(url).success(function (result) {
                alert(result);
                window.location.reload();
            }).error(function (data) {
                alert(data.ErrorMessage);
                window.location.reload();

            });
        } else { $("#DispechedId").prop("disabled", false); }
    }

}]);

app.controller("ModalInstanceCtrlTOrderInvoice", ["$scope", '$http', 'OrderMasterService', 'WarehouseService', "$modalInstance", 'ngAuthSettings', 'OrderDetail',
    function ($scope, $http, OrderMasterService, WarehouseService, $modalInstance, ngAuthSettings, OrderDetail) {


        console.log("Transfer order invoice modal opened");
        $scope.OrderDetailsInvoice = OrderDetail;
        $scope.TOMasterData = [];
        $scope.WarehouseDetail = [];
        $scope.FromWarehouseDetail = [];
        $scope.getTOMasterdetail = function () { // This would fetch the data on page change.
            $scope.sumofqty = {};
            var i = 0;
            var url = serviceBase + "api/TransferOrder/DispatchedDetail" + "?DetailId=" + $scope.OrderDetailsInvoice[0].TransferOrderId + "&WarehouseId=" + $scope.OrderDetailsInvoice[0].WarehouseId;
            $http.get(url).success(function (response) {
                $scope.TOMasterData = response; //ajax request to fetch data into vm.data
                _.map($scope.TOMasterData, function (obj) {
                    i += obj.TotalQuantity;
                });
                $scope.sumofqty = i;

                var url = serviceBase + "api/TransferOrder/DispatchedMaster" + "?masterId=" + $scope.OrderDetailsInvoice[0].TransferOrderId;
                $http.get(url).success(function (response) {
                    if (response.Count > 1) {
                        $scope.IsNotInterState = true;
                        $scope.masterdata = response.result; //ajax request to fetch data into vm.data
                    }
                    else if (response.Count == 0) {
                        alert("Something went Wrong.");
                    }
                    else {
                        $scope.IsNotInterState = false;
                        $scope.masterdata = response.result; //ajax request to fetch data into vm.data                                         
                    }

                });

            });
        };
        $scope.getTOMasterdetail();
        $scope.getFromWarehousedetail = function () { // This would fetch the data on page change.

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.OrderDetailsInvoice[0].RequestToWarehouseId;
            $http.get(url).success(function (response) {

                $scope.FromWarehouseDetail = response; //ajax request to fetch data into vm.data
                console.log("get current Page items:");
            });
        };
        $scope.getFromWarehousedetail();
        $scope.getWarehousedetail = function () { // This would fetch the data on page change.

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.OrderDetailsInvoice[0].WarehouseId;
            $http.get(url).success(function (response) {

                $scope.WarehouseDetail = response; //ajax request to fetch data into vm.data
                console.log("get current Page items:");
            });
        };
        $scope.getWarehousedetail();



    }]);

app.controller('TranferOrderCtrlQuantityedit', ['$scope', 'order', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", "$modalInstance", '$modal', function ($scope, order, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modalInstance, $modal) {
    $scope.OrderDetails = order;
    $scope.cQ = $scope.OrderDetails.TotalQuantity;
    $scope.OrderDetails.IsDispechedButton = true;
    $scope.checkqtyManualEdit = function (qty) {
        if (qty != null && qty > 0) {
            if ($scope.cQ < qty) {
                alert("you are not enter more than request qty!!");
                $scope.OrderDetails.TotalQuantity = $scope.cQ;
                return false;
            }
        } else {
            $scope.OrderDetails.IsDispechedButton = false;
        }
    }
    $scope.cancel = function () {
        $modalInstance.dismiss('canceled');
    }

}]);

app.controller('CreateTransferOrderController', ['$scope', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'localStorageService', '$window', '$rootScope', function ($scope, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modal, localStorageService, $window, $rootScope) {

    //Added 25/2/2021
    $scope.currentPageStores = {};
    $scope.total_count = 0;
    $scope.pageno = 1; // initialize page no to 1

    $scope.numPerPageOpt = [100];//dropdown options for no. of Items per page
    $scope.itemsPerPage = $scope.numPerPageOpt[0];

    $scope.Percantage = 0;
    $scope.SearchPercentage = function (data) {
        var url = serviceBase + "api/TransferOrder/GetTransferWHPercentage?fromWarehouseId=" + data.RequestFromWarehouseId + "&RequestToWarehouseId=" + data.RequestToWarehouseId;
        $http.get(url)
            .success(function (results) {
                $scope.Percantage = results;
                //debugger;
                if (data.RequestToWarehouseId > 0) {
                    $scope.CheckIsCnF(data.RequestToWarehouseId);
                }
                console.log("this.percentage", Percantage);
            }, function (error) {
            })
    };
    $scope.getWarehosues = function () {
        var url = serviceBase + 'api/Warehouse';
        $http.get(url)
            .success(function (response) {
                $scope.warehouse = response;

            }, function (error) {
            })
    };
    $scope.getWarehosues();
    $scope.AllNewWarehouse = [];
    $scope.getNewWarehosues = function () {
        var url = serviceBase + 'api/Warehouse/GetAllWarehouse';
        $http.get(url)
            .success(function (response) {
                $scope.AllNewWarehouse = response;

            }, function (error) {
            })
    };
    $scope.getNewWarehosues();
    $scope.getAllwarehouse = function () {
        var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
        $http.get(url)
            .success(function (results) {
                $scope.Allwarehouse = results;
                $scope.RequestWH = results;
            }, function (error) {
            })
    };
    $scope.getAllwarehouse();

    $scope.idata = {};
    $scope.Search = function (key, StockWarehouseId) {


        if (StockWarehouseId) {
            if (!key) { alert("Search item"); return false; }
            var url = serviceBase + "api/TransferOrder/SearchStockitem?key=" + key + "&WarehouseId=" + StockWarehouseId;
            $http.get(url).success(function (data) {
                $scope.itemData = data;

                $scope.idata = angular.copy($scope.itemData);
            })
        } else { alert("Select RequestTowarehouse first"); }
    };
    $scope.iidd = 0;
    $scope.Minqtry = function (key) {
        $scope.itmdata = [];
        $scope.iidd = Number(key);
        for (var c = 0; c < $scope.idata.length; c++) {
            if ($scope.idata.length != null) {
                if ($scope.idata[c].StockId == $scope.iidd) {
                    $scope.itmdata.push($scope.idata[c]);
                }
            }

        }
    }
    $scope.SearchSupplier = function (key) {
        var url = serviceBase + "api/Suppliers/search?key=" + key;
        $http.get(url).success(function (data) {
            $scope.Suppliers = data;
        })
    };

    $scope.ok = function () { modalInstance.close(); },
        $scope.cancel = function () { modalInstance.dismiss('Canceled'); },
        $scope.POdata = [];
    $scope.SeledtedWid = {};
    $scope.whselected = false;
    $scope.IsCnF = false;
    $scope.CheckIsCnF = function (WarehouseId) {
        $scope.POdata = [];
        $scope.IsCnF = false;
        $scope.IsCnF = $scope.AllNewWarehouse.filter(function (item) {
            return item.WarehouseId === parseInt(WarehouseId);
        })[0].IsCnF;
    };



    $scope.AddData = function (item) {


        if (item == undefined) {
            alert("Insert Data");
            return false;
        }

        if (item.StockId == undefined) {
            alert("Choose Item");
            return false;
        }
        if (item.Noofpics == undefined) {
            alert("Choose No of pics");
            return false;
        }

        if (!item.PriceOfItem && $scope.IsCnF) {
            alert("Please Enter PriceOfItem");
            return false;
        }



        if (item.StockId == "") {
            alert("Choose Item");
            return false;
        }

        if (item.StockId) {

            if ($scope.POdata.length > 0) {
                _.find($scope.POdata, function (itemMaster) {
                    if (itemMaster.StockId == item.StockId) {
                        var qty = itemMaster.Noofpics + item.Noofpics;
                        itemMaster.Noofpics = qty;
                    }
                });
            }
        }

        $scope.supplierData = true;
        $scope.supplierData1 = true;
        if (item.Noofpics === "") {
            alert('Please fill Number of pics');

        }
        else if (item.RequestToWarehouseId === item.RequestFromWarehouseId) {
            alert('Request To Warehouse Id and your warehouse id should not be same.')
            return false;
        }
        else {

            $scope.whselected = true;
            var itemname;
            var ABCClassifcation;
            for (var c = 0; c < $scope.itmdata.length; c++) {
                if ($scope.itmdata[c].StockId == item.StockId) {
                    itemname = $scope.itmdata[c].itemname;
                    ABCClassifcation = $scope.itmdata[c].ABCClassification;
                    break;
                }
            }
            var data = true;
            for (var c = 0; c < $scope.POdata.length; c++) {
                if ($scope.POdata[c].StockId == item.StockId) {
                    data = false;
                    break;
                }
            }
            if (data == true) {

                $scope.POdata.push({
                    StockId: item.StockId,
                    Itemname: itemname,
                    Noofpics: item.Noofpics,
                    RequestToWarehouseId: item.RequestToWarehouseId,
                    RequestFromWarehouseId: item.RequestFromWarehouseId,
                    ABCClassifcation: ABCClassifcation,
                    fread: item.fread,
                    PriceOfItem: item.PriceOfItem

                });
                item.Noofpics = "";
                item.StockId = "";
                item.PriceOfItem = "";
            }
            else {
                alert("Item is Already Added");
                item.Noofpics = "";
                item.StockId = "";
                item.PriceOfItem = "";
            }
        }
    }


    // Added by Anoop 26/2/2021

    function getDataAddTranferOrder(pageno, Warehouseid) {

        if (Warehouseid > 0) {


            $scope.numPerPageOpt = [100];//dropdown options for no. of Items per page
            $scope.itemsPerPage = $scope.numPerPageOpt[0];
            var start = null;
            var end = null;
            var currentPageStores_TransferOrderId = 0;
            var currentPageStores_Status = null;
            $scope.pagenoOne = pageno;
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/TransferOrder" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + Warehouseid + "&StartDate=" + start + "&EndDate=" + end + "&Status=" + currentPageStores_Status + "&TransferOrderId=" + currentPageStores_TransferOrderId;
            $http.get(url).success(function (response) {
                //window.location.reload();
                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");

                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.callmethod();
            });
        }
    }






    $scope.Searchsave = function () {
        //debugger;
        $scope.count = 1;
        var data = $scope.POdata;
        if ($scope.POdata[0].RequestToWarehouseId != $scope.POdata[0].RequestFromWarehouseId) {
            var url = serviceBase + 'api/TransferOrder/AddTranferOrder';


            if ($window.confirm("Please confirm?")) {
                $http.post(url, data).success(function (result) {
                    console.log(result);
                    //debugger;
                    var dataom = result.ordermaster[0];
                    var currentPageStores_Status = dataom.Status;

                    var currentPageStores_TransferOrderId = dataom.TransferOrderId;

                    if (data.id == 0) {
                        alert('something went wrong');
                    }
                    else {
                        //debugger;
                        //$rootScope.getAddTO = dataom.WarehouseId;
                        window.location.reload();
                        // getDataAddTranferOrder(1, dataom.WarehouseId);


                        // Added by Anoop 25/2/2021
                        //$rootScope.$emit("CallParentMethod", 1, currentPageStores_Status, currentPageStores_TransferOrderId);
                        alert('Transfer Order Send');
                        //modalInstance.close();
                        //window.location.reload();

                        ////debugger;
                        ////getData1(pageno, currentPageStores_Status, currentPageStores_TransferOrderId);
                        //$scope.currentPageStores = results.damagest;

                        //$scope.currentPageStores = results.damagest;
                        //$scope.AllData = results.damagest;

                        //$scope.total_count = results.total_count;
                        //$scope.Porders = $scope.currentPageStores;


                        //getData1(pageno, statusname, searchKeywords)
                    }
                })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                    });
            } else { $("#clickAndDisable").prop("disabled", false); }

        } else {
            alert("Please change request to Warehouse.");
            window.location.reload();
        }
    };



    //Remove item from list
    $scope.remove = function (item) {
        var index = $scope.POdata.indexOf(item);
        $scope.POdata.splice(index, 1);
    };
}]);