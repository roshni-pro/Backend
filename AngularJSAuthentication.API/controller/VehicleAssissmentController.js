
(function () {
    'use strict';

    angular
        .module('app')
        .controller('VehicleAssissmentController', VehicleAssissmentController);

    VehicleAssissmentController.$inject = ['$scope', "DeliveryService", "localStorageService", "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function VehicleAssissmentController($scope, DeliveryService, localStorageService, $filter, $http, ngTableParams, $modal, WarehouseService) {

        console.log(" VehicleAssissmentController reached");
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
        });
        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

                $scope.getWarehousebyId($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehousebyId = function (WarehouseId) {
            DeliveryService.getWarehousebyId($scope.WarehouseId).then(function (resultsdboy) {
                $scope.DBoys = resultsdboy.data;
            }, function (error) {
            });
        };
        $scope.getWarehosues();
        $scope.totalproducts = false;
        $scope.chkdb = true;
        $scope.oldpords = false;
        $scope.totalpercent = 0;
        $scope.dbysz = {};
        $scope.totalproductspace = 0;
        $scope.totalAmountofallproducts = 0;

        $scope.open = function (items) {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDboyModal.html",
                    controller: "Dboyctrl", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
        $scope.DBoys = [];
        DeliveryService.getdboys().then(function (results) {

            $scope.DBoys = results.data;


        }, function (error) {
        });
        $scope.deliveryBoy = {};
        $scope.getdborders = function (DB) {

            $scope.deliveryBoy = JSON.parse(DB);
            localStorageService.set('DBoysData', $scope.deliveryBoy);
            $scope.chkdb = false;
        }

        $scope.DBoysData = localStorageService.get('DBoysData');
        console.log($scope.DBoysData);

        $scope.oldorders = [];

        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [30, 50, 100, 200];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.onNumPerPageChange = function () {

            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getoldorders($scope.pageno);
        }

        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown



        $scope.getoldorders = function (pageno) {

            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var url = "";

            if (!$('#dat').val()) {
                end = '';
                start = '';
                //alert("Select Start and End Date")
                //return;
                url = serviceBase + "api/DeliveryIssuance?list=" + $scope.itemsPerPage + "&page=" + pageno + "&id=" + $scope.deliveryBoy.PeopleID + "&start=" + start + "&end=" + end;
            }
            else {
                start = f.val();
                end = g.val();
                url = serviceBase + "api/DeliveryIssuance?list=" + $scope.itemsPerPage + "&page=" + pageno + "&id=" + $scope.deliveryBoy.PeopleID + "&start=" + start + "&end=" + end;
            }
            $http.get(url)
                .success(function (data) {

                    $scope.oldorders = data.historyamount;
                    $scope.total_count = data.total_count;
                    $scope.currentPageStores = $scope.oldorders;
                    $scope.DelIssuance = data;

                    console.log($scope.oldorders);
                    console.log("$scope.oldorders");
                    $scope.oldpords = true;
                })
                .error(function (data) {
                    console.log(data);
                })
        }


        $scope.prodetails = function (items) {

            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDboyModal11.html",
                    controller: "VADboyctrl11", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        }
        $scope.summary = function (items) {

            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDboyModal.html",
                    controller: "VADboyctrlorderdetails", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        }

        $scope.Export = function () {

            if ($scope.DelIssuance != undefined) {
                $scope.NewExportData = [];
                for (var i = 0; i < $scope.DelIssuance.length; i++) {
                    var tts = {
                        DeliveryBoy: '', DeliveryIssuanceId: '', TotalAssignmentAmount: '', CreatedDate: '', Status: '', VehicleNumber: '', ProductType: ''
                    }
                    tts.DeliveryIssuanceId = $scope.DelIssuance[i].DeliveryIssuanceId;
                    tts.TotalAssignmentAmount = $scope.DelIssuance[i].TotalAssignmentAmount;
                    tts.CreatedDate = $scope.DelIssuance[i].CreatedDate;
                    tts.Status = $scope.DelIssuance[i].Status;
                    tts.VehicleNumber = $scope.DelIssuance[i].VehicleNumber;
                    tts.ProductType = $scope.DelIssuance[i].details.length;
                    tts.DeliveryBoy = $scope.DelIssuance[i].DisplayName;
                    $scope.NewExportData.push(tts);
                }
                alasql.fn.myfmt = function (n) {
                    return Number(n).toFixed(2);
                }
                alasql('SELECT DeliveryBoy,DeliveryIssuanceId,TotalAssignmentAmount,CreatedDate,Status,VehicleNumber,ProductType INTO XLSX("VehicleAssigmentReport.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
            } else { alert('Please select parameter'); }
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('VADboyctrl11', VADboyctrl11);

    VADboyctrl11.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", "localStorageService", "OrderMasterService", "$filter"];

    function VADboyctrl11($scope, $http, ngAuthSettings, $modalInstance, obj, localStorageService, OrderMasterService, $filter) {

        $scope.DBoyData = {};
        $scope.orderdetails = [];
        $scope.Orderidss = [];
        $scope.orderamountdata = [];
        $scope.warehouse = [];
        OrderMasterService.getwarehouse().then(function (results) {

            $scope.warehouse = results.data;

        }, function (error) {
        });

        if (obj) {

            $scope.DBoyData = obj;
            /// Get order amount behalf of orderdispeched id- YN
            var urla = serviceBase + "api/DeliveryIssuance/GetOrderamount?ids=" + $scope.DBoyData.OrderIds;
            $http.get(urla)
                .success(function (response) {

                    $scope.orderamountdata = response;
                    $scope.orderdetails = $scope.DBoyData.details;
                    var ids = $scope.DBoyData.OrderIds;
                    var str_array = ids.split(',');
                    $scope.Orderidss = $scope.orderamountdata;
                    console.log($scope.Orderidss);
                });
            ///////////////////////////////////    

            ///////////////////////////////////
            $scope.AssOrder = {};
            $http.get(serviceBase + "api/DeliveryIssuance/GetAssOrder?AssignmentId=" + obj.DeliveryIssuanceId + "")
                .success(function (response) {

                    $scope.AssOrder = response;
                    if ($scope.AssOrder.length > 0) {
                        $scope.GetWdetails($scope.AssOrder[0].WarehouseId);

                    }
                });
        }

        $scope.GetWdetails = function (id) {

            var id = parseInt(id);
            $scope.filterData = $filter('filter')($scope.warehouse, function (value) {
                return value.WarehouseId === id;
            });
            $scope.warehouse = $scope.filterData;
            if ($scope.warehouse != null) {

                $scope.HidetaxCessColumn();
            }


        }

        $scope.HidetaxCessColumnArray = [];

        //function to remove zero cess  item column from invoice print
        $scope.HidetaxCessColumn = function () {
            for (var i = 0; i < $scope.AssOrder.length; i++) {
                $scope.AssOrder[i].HideCessColumn = false;
                $scope.HidetaxCessColumnArray.push($scope.AssOrder[i]);
                for (var j = 0; j < $scope.AssOrder[i].orderDetails.length; j++) {
                    if ($scope.AssOrder[i].orderDetails[j].CessTaxAmount > 0) {
                        $scope.AssOrder[i].HideCessColumn = true;
                        break;
                    }
                }
            }
        }



        //function to remove zero qty item from invoice print
        $scope.RemoveZeroQtyItemInvoice = function (prop, val) {
            return function (item) {
                if (item[prop] > val) return true;
            }
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.printdetail = function (printDetailId) {

            var printContents = document.getElementById(printDetailId).innerHTML;
            var originalContents = document.body.innerHTML;

            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                var popupWin = window.open('', '_blank', 'width=800,height=600,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="style.css" />' +
                    '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
                popupWin.onbeforeunload = function (event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function (event) {
                    popupWin.document.close();
                    popupWin.close();
                }
            } else {
                var popupWin = window.open('', '_blank', 'width=800,height=600');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();
            return true;
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('VADboyctrlorderdetails', VADboyctrlorderdetails);

    VADboyctrlorderdetails.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", "localStorageService"];

    function VADboyctrlorderdetails($scope, $http, ngAuthSettings, $modalInstance, obj, localStorageService) {



        $scope.DBoysData = localStorageService.get('DBoysData');
        $scope.DBoyData = {};
        $scope.orderdetails = [];
        $scope.Orderids = [];
        $scope.delivereddata = [];
        $scope.cancelddata = [];
        $scope.redispatcheddata = [];
        $scope.shippedData = [];

        if (obj) {

            $scope.DBoyData = obj;
            $scope.DeliveryIssuanceId = localStorageService.set('DeliveryIssuanceId', $scope.DBoyData.DeliveryIssuanceId);
            $scope.DeliveryIssuanceId = localStorageService.get('DeliveryIssuanceId');
            $scope.TotalAssignmentAmount = localStorageService.set('TotalAssignmentAmount', $scope.DBoyData.TotalAssignmentAmount);
            $scope.TotalAssignmentAmount = localStorageService.get('TotalAssignmentAmount');
            console.log("kkkkkk");
            console.log($scope.DBoyData);
            $scope.orderdetails = $scope.DBoyData.details;
            var ids = $scope.DBoyData.OrderIds;
            var test = "test";
            var url = serviceBase + "api/vehicleassissment?ids=" + ids + "&DeliveryIssuanceId=" + $scope.DeliveryIssuanceId;
            $http.get(url)
                .success(function (response) {

                    //.success(function (data) {           
                    $scope.dboyordersdata = response;
                    $scope.TotalDeliveredOrder = 0;
                    $scope.TotalDeliveredOrderAmount = 0;
                    $scope.TotalDeliveredCashAmount = 0;
                    $scope.TotalDeliveredChequeAmount = 0;
                    $scope.TotalDeliveredElectronicAmount = 0;
                    //$scope.TotalRedispatchOrder = 0;
                    $scope.date = response.OrderedDate;
                    $scope.TotalRedispatchOrderAmount = 0;
                    $scope.TotalCanceledOrderAmount = 0;
                    for (var i = 0; i < response.length; i++) {

                        if (response[i].Status == "Delivered" || response[i].Status == "Account settled" || response[i].Status == "sattled" || response[i].Status == "Partial receiving -Bounce") {
                            $scope.delivereddata.push(response[i]);
                        }
                        if (response[i].Status == "Delivery Redispatch") {
                            $scope.redispatcheddata.push(response[i]);
                        }
                        if (response[i].Status == "Shipped" || response[i].Status == "Issued") {
                            $scope.shippedData.push(response[i]);
                        }
                        if (response[i].Status == "Delivery Canceled" || response[i].Status == "Order Canceled") {
                            $scope.cancelddata.push(response[i]);
                        }
                    }
                    for (var d = 0; d < $scope.delivereddata.length; d++) {

                        $scope.TotalDeliveredOrderAmount = $scope.TotalDeliveredOrderAmount + $scope.delivereddata[d].GrossAmount;
                        $scope.TotalDeliveredOrder = $scope.TotalDeliveredOrder + 1;
                        $scope.TotalDeliveredCashAmount = $scope.TotalDeliveredCashAmount + $scope.delivereddata[d].cashAmount;
                        $scope.TotalDeliveredChequeAmount = $scope.TotalDeliveredChequeAmount + $scope.delivereddata[d].chequeAmount;
                        $scope.TotalDeliveredElectronicAmount = $scope.TotalDeliveredElectronicAmount + $scope.delivereddata[d].ElectronicAmount;
                    }
                    for (var e = 0; e < $scope.redispatcheddata.length; e++) {

                        //$scope.TotalRedispatchOrder = $scope.TotalRedispatchOrder + 1;
                    }
                    $scope.extraData($scope.DeliveryIssuanceId, ids);
                })
                .error(function (response) {
                    console.log(response);
                })
        }
        $scope.extraData = function (DeliveryIssuanceId, ids) {
            // for odermastr canceal
            var url = serviceBase + "api/vehicleassissment?ids=" + ids + "&DeliveryIssuanceId=" + DeliveryIssuanceId + "&test=" + test;
            $http.get(url)
                .success(function (data) {

                    $scope.date = data[0].UpdatedDate;
                    $scope.dcanceldata = data;
                    $scope.itemdetail = [];
                    $scope.itemdetails = [];
                    $scope.itemdetailsredispatched = [];
                    $scope.TotalCancelOrder = 0;
                    $scope.TotalCanceledOrderqty = 0;
                    $scope.TotalCanceledOrderAmount = 0;
                    $scope.TotalRedispatchOrder = 0;
                    $scope.TotalRedispatchOrderqty = 0;
                    $scope.allproducts = [];
                    $scope.allproductredispatched = [];
                    for (var i = 0; i < data.length; i++) {

                        if (data[i].Status == "Delivery Canceled" || data[i].Status == "Order Canceled") {
                            $scope.TotalCanceledOrderAmount = $scope.TotalCanceledOrderAmount + data[i].GrossAmount;

                            for (var o = 0; o < data[i].orderDetails.length; o++) {
                                $scope.itemdetail.push(data[i].orderDetails[o]);
                                $scope.TotalCanceledOrderqty = $scope.TotalCanceledOrderqty + data[i].orderDetails[o].qty;
                            }
                            $scope.TotalCancelOrder = $scope.TotalCancelOrder + 1;
                            $scope.itemdetails.push(data[i]);
                        }
                    }
                    if ($scope.itemdetails.length > 0) {
                        $scope.selectedorders = angular.copy($scope.itemdetails);
                        console.log($scope.itemdetails);
                        var firstreq = true;
                        for (var k = 0; k < $scope.selectedorders.length; k++) {
                            for (var j = 0; j < $scope.selectedorders[k].orderDetails.length; j++) {
                                if (firstreq) {
                                    var OD = $scope.selectedorders[k].orderDetails[j];
                                    OD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();

                                    $scope.allproducts.push(OD);
                                    firstreq = false;
                                } else {
                                    var checkprod = true;
                                    _.map($scope.allproducts, function (prod) {
                                        // if ($scope.selectedorders[k].orderDetails[j].itemNumber == prod.itemNumber) {
                                        if ($scope.selectedorders[k].orderDetails[j].ItemMultiMRPId == prod.ItemMultiMRPId) {
                                            prod.OrderQty += ", " + $scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty;
                                            prod.qty = $scope.selectedorders[k].orderDetails[j].qty + prod.qty;
                                            prod.TotalAmt = $scope.selectedorders[k].orderDetails[j].TotalAmt + prod.TotalAmt;
                                            checkprod = false;
                                        }
                                    })
                                    if (checkprod) {
                                        var OD = $scope.selectedorders[k].orderDetails[j];
                                        OD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();
                                        $scope.allproducts.push(OD);
                                    }
                                }
                            }
                        }
                        console.log("Assissment total products");
                        console.log($scope.allproducts);
                    }
                    //else {
                    //    alert("Assissnment Data");
                    //}
                    $scope.TotalRedispatchedOrderAmount = 0;
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].Status == "Delivery Redispatch") {
                            $scope.TotalRedispatchedOrderAmount = $scope.TotalRedispatchedOrderAmount + data[i].GrossAmount;

                            for (var o = 0; o < data[i].orderDetails.length; o++) {
                                $scope.itemdetail.push(data[i].orderDetails[o]);
                                $scope.TotalRedispatchOrderqty = $scope.TotalRedispatchOrderqty + data[i].orderDetails[o].qty;

                            }
                            $scope.TotalRedispatchOrder = $scope.TotalRedispatchOrder + 1;
                            $scope.itemdetailsredispatched.push(data[i]);
                        }
                    }
                    if ($scope.itemdetailsredispatched.length > 0) {
                        $scope.selectedorders = angular.copy($scope.itemdetailsredispatched);
                        console.log($scope.itemdetailsredispatched);
                        var firstreqs = true;
                        for (var k = 0; k < $scope.selectedorders.length; k++) {
                            for (var j = 0; j < $scope.selectedorders[k].orderDetails.length; j++) {
                                if (firstreqs) {
                                    var OD = $scope.selectedorders[k].orderDetails[j];
                                    OD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();

                                    $scope.allproductredispatched.push(OD);
                                    firstreqs = false;
                                } else {
                                    var checkprod = true;
                                    _.map($scope.allproductredispatched, function (prod) {


                                        // if ($scope.selectedorders[k].orderDetails[j].itemNumber == prod.itemNumber) {
                                        if ($scope.selectedorders[k].orderDetails[j].ItemMultiMRPId == prod.ItemMultiMRPId) {
                                            prod.OrderQty += ", " + $scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty;
                                            prod.qty = $scope.selectedorders[k].orderDetails[j].qty + prod.qty;
                                            prod.TotalAmt = $scope.selectedorders[k].orderDetails[j].TotalAmt + prod.TotalAmt;
                                            checkprod = false;
                                        }
                                    })
                                    if (checkprod) {
                                        var OD = $scope.selectedorders[k].orderDetails[j];
                                        OD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();
                                        $scope.allproductredispatched.push(OD);
                                    }
                                }
                            }
                        }
                        console.log("Assissment redispatched total products");
                        console.log($scope.allproductredispatched);
                    }



                })
                .error(function (data) {
                    console.log(data);
                })


        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.printToCart = function (printSectionId) {
            var printContents = document.getElementById(printSectionId).innerHTML;
            var originalContents = document.body.innerHTML;
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                var popupWin = window.open('', '_blank', 'width=800,height=600,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="style.css" />' +
                    '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
                popupWin.onbeforeunload = function (event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function (event) {
                    popupWin.document.close();
                    popupWin.close();
                }
            } else {
                popupWin = window.open('', '_blank', 'width=800,height=600');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();
            return true;
        };

    }
})();

