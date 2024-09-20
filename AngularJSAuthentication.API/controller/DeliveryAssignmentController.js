
(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeliveryAssignmentController', DeliveryAssignmentController);

    DeliveryAssignmentController.$inject = ['$scope', "DeliveryService", "localStorageService", "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function DeliveryAssignmentController($scope, DeliveryService, localStorageService, $filter, $http, ngTableParams, $modal, WarehouseService) {

        console.log(" VehicleAssissmentController reached");
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
        $scope.warehouse = [];
        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {

        //        $scope.warehouse = results.data;
        //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

        //        //$scope.getWarehousebyId($scope.WarehouseId, $scope.Active);
        //    }, function (error) {
        //    });
        //};
        $scope.WarehouseId = "";
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                    $scope.getWarehousebyId($scope.WarehouseId, $scope.Active);
                });

        };
        $scope.wrshse();


        $scope.getWarehousebyId = function (WarehouseId, Active) {
            if (Active == undefined || Active == "") {
                $scope.Mobile = {};
                $scope.DBoys = {};
                $scope.Mobile = "";
            } else {
                $scope.Mobile = "";
                //DeliveryService.getDBoyWarehousebyId($scope.WarehouseId, $scope.Active).then(function (resultsdboy) {
                DeliveryService.getDBoyIsActiveVicebyId($scope.WarehouseId, $scope.Active).then(function (resultsdboy) {
                    $scope.DBoys = resultsdboy.data;
                }, function (error) {
                });
            }
        };
        $scope.onChangeWarehouse = function (WarehouseId) {
            $scope.Mobile = {};
            $scope.DBoys = {};
            $scope.Mobile = "";
            $scope.Active = "";
        }
        $scope.wrshse();
        $scope.totalproducts = false;
        $scope.chkdb = true;
        $scope.oldpords = false;
        $scope.totalpercent = 0;
        $scope.dbysz = {};
        $scope.totalproductspace = 0;
        $scope.totalAmountofallproducts = 0;
        $scope.hostUrl = window.location.origin;
        $scope.open = function (items) {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "assissmentSummaryModal.html",
                    controller: "assissmentSummaryDboyctrl", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };




        //$scope.DBoys = [];
        //DeliveryService.getdboys().then(function (results) {

        //    $scope.DBoys = results.data;


        //}, function (error) {
        //});
        $scope.deliveryBoy = {};
        $scope.getdborders = function (DB) {
            //debugger;
            $scope.deliveryBoy = JSON.parse(DB);
            localStorageService.set('DBoysData', $scope.deliveryBoy);
            $scope.chkdb = false;
        };
        $scope.onClick = function (Active) {
            //debugger;
            if (Active == undefined || Active == "") {
                alert('Please Select Status First!!!!');
                $scope.Mobile = {};
                $scope.DBoys = {};
                $scope.Mobile = "";
            }
        };

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
        };

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
                });
        };


        $scope.prodetails = function (items) {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "ProductsAssignmentModal.html",
                    controller: "ProductsAssignmentModalCtrl", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                });
        };

        $scope.DeliveryHistroy = function (data) {

            $scope.dataDeliveryHistroy = [];
            var url = serviceBase + "api/DeliveryIssuance/DeliveryIssuanceHistory?DeliveryIssuanceId=" + data.DeliveryIssuanceId;
            $http.get(url).success(function (response) {

                $scope.dataDeliveryHistroy = response;
                console.log($scope.dataDeliveryHistroy);

            })
                .error(function (data) {
                });
        };
        $scope.summary = function (items) {

            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "SummaryModal.html",
                    controller: "SummaryOrderdetails", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };
        $scope.Export = function () {

            $scope.DelIssuancetemp = $scope.DelIssuance.historyamount;
            if ($scope.DelIssuancetemp != undefined) {
                $scope.NewExportData = [];
                for (var i = 0; i < $scope.DelIssuancetemp.length; i++) {
                    var tts = { DeliveryBoy: '', DeliveryIssuanceId: '', TotalAssignmentAmount: '', CreatedDate: '', Status: '', VehicleNumber: '', ProductType: '' }
                    tts.DeliveryIssuanceId = $scope.DelIssuancetemp[i].DeliveryIssuanceId;
                    tts.TotalAssignmentAmount = $scope.DelIssuancetemp[i].TotalAssignmentAmount;
                    tts.CreatedDate = $scope.DelIssuancetemp[i].CreatedDate;
                    tts.Status = $scope.DelIssuancetemp[i].Status;
                    tts.VehicleNumber = $scope.DelIssuancetemp[i].VehicleNumber;
                    tts.ProductType = $scope.DelIssuancetemp[i].details.length;
                    tts.DeliveryBoy = $scope.DelIssuancetemp[i].DisplayName;
                    $scope.NewExportData.push(tts);
                }
                alasql.fn.myfmt = function (n) {
                    return Number(n).toFixed(2);
                };
                alasql('SELECT DeliveryBoy,DeliveryIssuanceId,TotalAssignmentAmount,CreatedDate,Status,VehicleNumber,ProductType INTO XLSX("VehicleAssigmentReport.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
            } else { alert('Please select parameter'); }
        };

        $scope.AddShortItems = function (items) {

            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddShortItemsModal.html",
                    controller: "AddShortItemsCtrl", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };

        $scope.summarys = function (items) {

            $scope.VehicleItem = [];
            $scope.VehicleItem = items;
            DeliveryOrderAssignmentChangeService.DeliveryOrderAssignmentChangeService($scope.VehicleItem);

        }



        $scope.PrintproductAssignment = function (items) {

            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PrintproductAssignment.html",
                    controller: "PrintproductAssignmentCtrl", resolve: { obj: function () { return items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                });
        };

        $scope.Upload = function (Id, Icfile, IsIcVerified, IsPhysicallyVerified) {

            var datato =
            {
                Id: Id,
                Icfile: Icfile,
                IsIcVerified: IsIcVerified,
                IsPhysicallyVerified: IsPhysicallyVerified
            }

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myUploadImageModal.html",
                    controller: "UploadIcCtrl", resolve: {
                        datato: function () { return datato; }
                    }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    });

        };

        $scope.AssignMentSubmit = function (DeliveryIssuanceId) {
            var url = serviceBase + "api/BackendOrder/AssignMentSubmit?AssignmentId=" + DeliveryIssuanceId ;
            $http.get(url)
                .success(function (res) {
                    console.log("res", res)
                    if (res.Status) {
                        alert(res.Message)
                        window.location.reload();
                    }
                    else {
                        alert(res.Message)
                    }
                })
        }
        $scope.AssignmentIds = null;
        $scope.OrderId = null;

        $scope.SearchAssignmentByIds = function (AssignmentIds, OrderId) {
            var orderId = OrderId == null ? 0 : OrderId;
            var AssignmentIds = AssignmentIds == null ? 0 : AssignmentIds;
            if (AssignmentIds + orderId == 0) {
                alert(" enter assignment id /Order Id "); return false;
            }
            //if (!$scope.deliveryBoy.PeopleID) { alert("select delivery boy "); return false; }
            var orderNumber = OrderId == null ? "" : OrderId
         
            var url = serviceBase + "api/DeliveryIssuance/SearchAssignment?AssignmentIds=" + AssignmentIds + "&OrderId=" + orderNumber;// + "&PeopleID=" + $scope.deliveryBoy.PeopleID;

            $http.get(url)
                .success(function (data) {
                    if (data.historyamount.length > 0) {
                        $scope.oldorders = [];
                        $scope.total_count = [];
                        $scope.currentPageStores = [];
                        $scope.DelIssuance = [];
                        $scope.oldpords = false;


                        $scope.oldorders = data.historyamount;
                        $scope.total_count = data.total_count;
                        $scope.currentPageStores = $scope.oldorders;
                        $scope.DelIssuance = data;
                        $scope.oldpords = true;
                    } else {
                        alert("no record found");
                    }

                })
                .error(function (data) {
                    console.log(data);
                });
        };


        $scope.clearSearch = function (AssignmentIds, OrderId) {
            if (AssignmentIds) {
                $scope.AssignmentIds = null;
                $scope.oldorders = [];
                $scope.total_count = [];
                $scope.currentPageStores = [];
                $scope.DelIssuance = [];
                $scope.oldpords = false;
            }
            if (OrderId) {
                $scope.OrderId = null;
                $scope.oldorders = [];
                $scope.total_count = [];
                $scope.currentPageStores = [];
                $scope.DelIssuance = [];
                $scope.oldpords = false;
            }

        }

        $scope.GetDirection = function (assignmentId) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AssignmentDirection.html",
                    controller: "AssignmentDirectionController", resolve: { assignmentId: function () { return assignmentId } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };
    }




})();



(function () {
    'use strict';

    angular
        .module('app')
        .controller('ProductsAssignmentModalCtrl', ProductsAssignmentModalCtrl);

    ProductsAssignmentModalCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", "localStorageService", "OrderMasterService", "$filter", '$modal'];

    function ProductsAssignmentModalCtrl($scope, $http, ngAuthSettings, $modalInstance, obj, localStorageService, OrderMasterService, $filter, $modal) {
        // 
        //
        $scope.DBoyData = {};
        $scope.orderdetails = [];
        $scope.Orderidss = [];
        $scope.orderamountdata = [];
        $scope.warehouses = [];
        //OrderMasterService.getwarehouse().then(function (results) {

        //    $scope.warehouses = results.data;

        //}, function (error) {
        //});

        $scope.wrshse = function () {
          
            var url = serviceBase + "api/Warehouse"; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                 
                    $scope.warehouses = data;
                });

        };



        $scope.wrshse(); //for role wise warehouse show  - 2023
        $scope.getTotalTax = function (data) {
            var totaltax = 0;
            data.forEach(x => {

                //totaltax = totaltax + x.AmtWithoutTaxDisc;
                totaltax = totaltax + (x.TaxAmmount + x.CessTaxAmount);
            });
            return totaltax;
        }
        $scope.getTotalQty = function (data) {
            var totalQty = 0;
            data.forEach(x => {

                totalQty = totalQty + x.Noqty;
            });
            return totalQty;
        }
        $scope.getTotalAWOTD = function (data) {
            var totalAWOTD = 0;
            data.forEach(x => {

                totalAWOTD = totalAWOTD + x.AmtWithoutTaxDisc;
            });
            return totalAWOTD;
        }
        $scope.getTotalAmtIncTaxes = function (data) {
            var totalAmtIncTaxes = 0;
            data.forEach(x => {

                totalAmtIncTaxes = totalAmtIncTaxes + x.TotalAmt;
            });
            return totalAmtIncTaxes;
        }
        $scope.getTotalTaxableValue = function (data) {
            var totalTaxableValue = 0;
            data.forEach(x => {
                totalTaxableValue = totalTaxableValue + x.AmtWithoutTaxDisc;

            });
            return totalTaxableValue;
        }
        $scope.getTotalIGST = function (data) {
            var totalIGST = 0;
            data.forEach(x => {

                totalIGST = totalIGST + x.TaxAmmount + x.CessTaxAmount;
            });
            return totalIGST;
        }
        $scope.getTotalSGST = function (data) {
            var totalSGST = 0;
            data.forEach(x => {

                totalSGST = totalSGST + x.SGSTTaxAmmount;
            });
            return totalSGST;
        }
        $scope.getTotalCGST = function (data) {
            var totalCGST = 0;
            data.forEach(x => {

                totalCGST = totalCGST + x.CGSTTaxAmmount;
            });
            return totalCGST;
        }
        $scope.getTotalCess = function (data) {
            var totalCess = 0;
            data.forEach(x => {

                totalCess = totalCess + x.CessTaxAmount;
            });
            return totalCess;
        }

        $scope.getTotalIOverall = function (data) {

            var TotalIOverall = 0;
            data.forEach(x => {

                TotalIOverall = TotalIOverall + x.AmtWithoutTaxDisc + x.SGSTTaxAmmount + x.CGSTTaxAmmount + x.CessTaxAmount;
            });
            return TotalIOverall;
        }


        if (obj) {

            $scope.DBoyData = obj;

            //Get Assignment Product List
            var Url = serviceBase + "api/DeliveryIssuance/AssignmentProductsList?AssignmentId=" + obj.DeliveryIssuanceId;
            $http.get(Url)
                .success(function (response) {
                   
                    $scope.orderdetails = [];
                    $scope.orderdetails = response;
                    for (var i = 0; i < $scope.orderdetails.length; i++) {
                        var xboxes = $scope.orderdetails[i].qty / $scope.orderdetails[i].PurchaseMinOrderQty;
                        var xpieces = $scope.orderdetails[i].qty % $scope.orderdetails[i].PurchaseMinOrderQty;
                        //var decimals = xboxes - Math.floor(xboxes);
                        //var xboxes = xboxes.toFixed(1);
                        var str = xboxes.toString();
                        var numarray = str.split('.');
                        $scope.orderdetails[i].Boxes = numarray[0];
                        $scope.orderdetails[i].piece = xpieces;//numarray[1];
                        console.log('$scope.orderdetails[i]' + $scope.orderdetails[i]);
                     
                    }
                });


            /// Get order amount behalf of orderdispeched id- YN
            //var urla = serviceBase + "api/DeliveryIssuance/GetOrderamount?ids=" + $scope.DBoyData.OrderIds;
            var urla = serviceBase + "api/DeliveryIssuance/GetAssingmentamount?id=" + obj.DeliveryIssuanceId
            $http.get(urla)
                .success(function (response) {

                    $scope.orderamountdata = response;
                    //$scope.orderdetails = $scope.DBoyData.details;
                    var ids = $scope.DBoyData.OrderIds;
                    var str_array = ids.split(',');
                    $scope.Orderidss = $scope.orderamountdata;
                    console.log($scope.Orderidss);
                });
            ///////////////////////////////////    

            ///////////////////////////////////


            $scope.AssOrder = {};
            $scope.StatesData = {};
            $http.get(serviceBase + "api/DeliveryIssuance/GetAssOrder?AssignmentId=" + obj.DeliveryIssuanceId + "")
                .success(function (response) {
                    //   
                    //
                    //debugger;
                    //$http.get(serviceBase + 'api/OrderMaster/GetStateCode' + "?WarehouseId=" + master.WarehouseId).then(function (results) {
                    //    debugger;
                    //    $scope.statesData = results.data[0];
                    //    console.log("-----------------------------------------", results.data[0]);
                    //})


                    if (response && response.OrderTYpe == 5) {
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                templateUrl: "AssignmentTradeInvoiceModel.html",
                                controller: "ModalInstanceCtrlAssignmentTraedeInvoiceDispatch",
                                resolve:
                                {
                                    order: function () {
                                        return response.PdfPath;
                                    }
                                }
                            });
                        modalInstance.result.then(function () {
                        },
                            function () {
                                console.log("Cancel Condintion");
                            });

                    }
                    else {
                        $scope.AssOrder = response.OrderDispatchedMasters;
                        if ($scope.DBoyData.WarehouseId > 0) {
                            $scope.GetWdetails($scope.DBoyData.WarehouseId);

                        }
                    }


                });
        }

        $scope.GetWdetails = function (id) {
          
            var id1 = parseInt(id);//instead of id used id1
            $scope.filterData = $filter('filter')($scope.warehouses, function (value) {
                return value.WarehouseId === id1;
            });
            $scope.warehouses = $scope.filterData;
            if ($scope.warehouses != null) {

                $scope.HidetaxCessColumn();
            }
        };


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
        };

        //function to remove zero qty item from invoice print
        $scope.RemoveZeroQtyItemInvoice = function (prop, val) {
            return function (item) {
                if (item[prop] > val) return true;
            };
        };

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
                };
            } else {
                var popupWin = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin1
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();
            return true;
        };

        //Print assignment Function in Bulk

        $scope.printItemAssignment = function (printItemAssignment) {


            var printContents = document.getElementById(printItemAssignment).innerHTML;
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
                var popupWin = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin2
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
        .controller('SummaryOrderdetails', SummaryOrderdetails);

    SummaryOrderdetails.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", "localStorageService"];

    function SummaryOrderdetails($scope, $http, ngAuthSettings, $modalInstance, obj, localStorageService) {


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
            //debugger;
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
                        if (response[i].Status == "Shipped" || response[i].Status == "Issued" || response[i].Status == "Assigned") {
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
                                        var ODD = $scope.selectedorders[k].orderDetails[j];//instead of OD used ODD
                                        ODD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();
                                        $scope.allproducts.push(ODD);
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
                    for (var c = 0; c < data.length; c++) {//instead of i used c
                        if (data[c].Status == "Delivery Redispatch") {
                            $scope.TotalRedispatchedOrderAmount = $scope.TotalRedispatchedOrderAmount + data[c].GrossAmount;

                            for (var d = 0; d < data[c].orderDetails.length; d++) {//instead of o used d
                                $scope.itemdetail.push(data[c].orderDetails[d]);
                                $scope.TotalRedispatchOrderqty = $scope.TotalRedispatchOrderqty + data[c].orderDetails[d].qty;

                            }
                            $scope.TotalRedispatchOrder = $scope.TotalRedispatchOrder + 1;
                            $scope.itemdetailsredispatched.push(data[c]);
                        }
                    }
                    if ($scope.itemdetailsredispatched.length > 0) {
                        $scope.selectedorders = angular.copy($scope.itemdetailsredispatched);
                        console.log($scope.itemdetailsredispatched);
                        var firstreqs = true;
                        for (var e = 0; e < $scope.selectedorders.length; e++) {//instead of k used e
                            for (var v = 0; v < $scope.selectedorders[e].orderDetails.length; v++) {//instead of j used v
                                if (firstreqs) {
                                    var ODE = $scope.selectedorders[e].orderDetails[v];//instead of OD used ODE
                                    ODE.OrderQty = ($scope.selectedorders[e].orderDetails[v].OrderId + " - " + $scope.selectedorders[e].orderDetails[v].qty).toString();

                                    $scope.allproductredispatched.push(ODE);
                                    firstreqs = false;
                                } else {
                                    var checkprod1 = true;//instead of checkprod used checkprod1
                                    _.map($scope.allproductredispatched, function (prod) {


                                        // if ($scope.selectedorders[k].orderDetails[j].itemNumber == prod.itemNumber) {
                                        if ($scope.selectedorders[e].orderDetails[v].ItemMultiMRPId == prod.ItemMultiMRPId) {
                                            prod.OrderQty += ", " + $scope.selectedorders[e].orderDetails[v].OrderId + " - " + $scope.selectedorders[e].orderDetails[v].qty;
                                            prod.qty = $scope.selectedorders[e].orderDetails[v].qty + prod.qty;
                                            prod.TotalAmt = $scope.selectedorders[e].orderDetails[v].TotalAmt + prod.TotalAmt;
                                            checkprod1 = false;
                                        }
                                    })
                                    if (checkprod1) {
                                        var OD1 = $scope.selectedorders[e].orderDetails[v];//instead of OD used OD1
                                        OD1.OrderQty = ($scope.selectedorders[e].orderDetails[v].OrderId + " - " + $scope.selectedorders[e].orderDetails[v].qty).toString();
                                        $scope.allproductredispatched.push(OD1);
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
                var popupWin1 = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin1
                popupWin1.document.open();
                popupWin1.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin1.document.close();
            }
            //// popupWin.document.close();
            return true;
        }

        $scope.AssignmentPayment = function (DeliveryIssuanceId) {

            if (DeliveryIssuanceId != 'undefined' && DeliveryIssuanceId != '') {
                $("#AssignmentPayment").prop("disabled", true);
                var url = serviceBase + "api/DeliveryAssignment/AssignmentPayment?DeliveryIssuanceId=" + DeliveryIssuanceId;
                $http.put(url)
                    .success(function (data) {
                        //   
                        if (!data) {
                            alert("There is Some Problem");
                        }
                        else {
                            location.reload();
                        }
                    })
                    .error(function (data) {
                        $scope.finalizebtn = false;
                    })
            }
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddShortItemsCtrl', AddShortItemsCtrl);

    AddShortItemsCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", "localStorageService"];

    function AddShortItemsCtrl($scope, $http, ngAuthSettings, $modalInstance, obj, localStorageService) {

        $scope.AssignmentShortItem = [];
        $scope.AssignmentShortItem.length = 0;
        $scope.IsShortItemSave = false;
        $scope.orderdetails = [];
        if (obj) {
            $scope.DBoyData = obj;
        }
        //debugger;
        console.log($scope.DBoyData);
        $scope.orderdetails = $scope.DBoyData.details;
        var ids = $scope.DBoyData.OrderIds;

        for (var i = 0; i < $scope.orderdetails.length; i++) {
            var xboxes = $scope.orderdetails[i].qty / $scope.orderdetails[i].PurchaseMinOrderQty;
            var xpieces = $scope.orderdetails[i].qty % $scope.orderdetails[i].PurchaseMinOrderQty;
            var str = xboxes.toString();
            var numarray = str.split('.');
            $scope.orderdetails[i].Boxes = numarray[0];
            $scope.orderdetails[i].piece = xpieces;//numarray[1];
            console.log('$scope.orderdetails[i]' + $scope.orderdetails[i]);
        }

        $scope.GetOrderId = function (data) {

            $scope.detailsdata = JSON.parse(data);
            $scope.OrderIds = [];
            $scope.OrderAsData = {};

            for (var i = 0; i < $scope.orderdetails.length; i++) {
                if ($scope.detailsdata.ItemId == $scope.orderdetails[i].ItemId) {
                    $scope.OrderQty = $scope.detailsdata.OrderQty.split(',');
                    //var array = $scope.detailsdata.OrderQty.split(',');
                    for (var j = 0; j < $scope.OrderQty.length; j++) {
                        var orderids = $scope.OrderQty[j].split("-")[0];
                        $scope.OrderIds.push(orderids);
                    }
                    break;
                }
            }

        }


        //get OrderItem 
        $scope.OrderAsData = {};

        $scope.Shortdata = { NotinStockQty: 0, DamageStockQty: 0, DamageComment: "", NotInStockComment: "", OrderId: "" };

        $scope.getOrderItem = function (OrderId, data) {

            $scope.OrderAsData = {};
            $scope.itemdetailsdata = JSON.parse(data);
            var url = serviceBase + 'api/DeliveryAssignment/GetOrderItems?OrderId=' + OrderId + "&ItemId=" + $scope.itemdetailsdata.ItemId + "&itemNumber=" + $scope.itemdetailsdata.itemNumber;
            $http.get(url).success(function (response) {

                if (response.OrderDispatchedDetailsId > 0) {
                    $scope.OrderAsData = response;
                }
                else {
                    alert("you can't do short qty in this Order : " + OrderId);
                    $scope.Shortdata = {};
                }

            });
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        //temp insert on js 
        $scope.InsertTempShortItem = function (data, DamageStockQty, NotinStockQty, OrderId, DamageComment, NotInStockComment, Orderqty) {


            $scope.shortItem = JSON.parse(data);
            if (OrderId > 0) {
                if (!NotinStockQty)
                    NotinStockQty = 0;

                if (!DamageStockQty)
                    DamageStockQty = 0;

                if (!DamageComment && DamageStockQty > 0) {
                    alert("Please insert damage stock comment.");
                    return false;
                }

                if (!NotInStockComment && NotinStockQty > 0) {
                    alert("Please insert not in stock comment.");
                    return false;
                }

                if ($scope.shortItem.ItemId > 0 && Orderqty >= parseInt(DamageStockQty + NotinStockQty)) {
                    $scope.exits = false;

                    angular.forEach($scope.AssignmentShortItem, function (item) {

                        if (item.itemNumber == $scope.shortItem.itemNumber && item.OrderId == OrderId) {
                            if (item.Orderqty >= DamageStockQty + NotinStockQty) {
                                item.NotinStockQty = NotinStockQty;
                                item.DamageStockQty = DamageStockQty;
                                item.DamageComment = DamageComment;
                                item.NotInStockComment = NotInStockComment;
                                $scope.exits = true;
                                $scope.OrderAsData = {};
                            }
                            else {
                                alert("Short item Qty Can't be greater then Assignment Qty");
                                $scope.exits = true;
                                return;
                            }
                        }
                    });

                    if (!$scope.exits) {

                        $scope.shortItem.NotinStockQty = NotinStockQty;
                        $scope.shortItem.DamageStockQty = DamageStockQty;
                        $scope.shortItem.DamageComment = DamageComment;
                        $scope.shortItem.NotInStockComment = NotInStockComment;
                        $scope.shortItem.Orderqty = Orderqty;
                        $scope.shortItem.ItemId = $scope.OrderAsData.ItemId;

                        $scope.AssignmentShortItem.push({
                            DeliveryIssuanceId: $scope.DBoyData.DeliveryIssuanceId,
                            DboyId: $scope.DBoyData.PeopleID,
                            DamageStockQty: $scope.shortItem.DamageStockQty,
                            NotinStockQty: $scope.shortItem.NotinStockQty,
                            DamageComment: $scope.shortItem.DamageComment,
                            NotInStockComment: $scope.shortItem.NotInStockComment,
                            Orderqty: $scope.shortItem.Orderqty,// $scope.shortItem.qty,
                            itemname: $scope.shortItem.itemname,
                            ItemId: $scope.shortItem.ItemId,//$scope.OrderAsData
                            itemNumber: $scope.shortItem.itemNumber,
                            OrderId: OrderId

                        });



                    }
                    setTimeout(function () {
                        $scope.$apply(function () {

                            $scope.Shortdata = {};
                            $scope.OrderAsData = {};
                        })
                    });

                }
                else {
                    alert("Short item Qty Can't be greater then Assignment Qty or blank");
                }

            }
            else { alert("Please select order"); }

        }

        //Get alerady AssignmentShortItemList
        $scope.getShortItem = function () {

            var url = serviceBase + 'api/DeliveryAssignment/GetShortItems?DeliveryIssuanceId=' + $scope.DBoyData.DeliveryIssuanceId;

            $http.get(url).success(function (response) {


                $scope.AssignmentShortItem = response;
                if ($scope.AssignmentShortItem.length > 0) {
                    $scope.IsShortItemSave = true;
                }
                if (response != "true") {
                    console.log();

                }
                else {

                }
            });
        }
        $scope.getShortItem();
        //Post AssignmentShortItemList
        $scope.SavedShortItem = function () {


            $("#SavedShortItem").prop("disabled", true);
            var url = serviceBase + 'api/DeliveryAssignment/InsertShortItems';


            var datatopost =
            {
                AssignmentShortItems: $scope.AssignmentShortItem,
                DeliveryIssuanceId: $scope.DBoyData.DeliveryIssuanceId,
                DBoyId: $scope.DBoyData.PeopleID
            };
            $http.post(url, datatopost).success(function (response) {
                //
                if (!response) {
                    alert("DATA Not SAVED SUCCESSFULLY!!!");
                }
                else {
                    $scope.IsShortItemSave = true;
                    alert("DATA SAVED SUCCESSFULLY!!!");
                }
            });
        }

        $scope.Finalization = function () {

            if ($scope.AssignmentShortItem.length > 0 && !$scope.IsShortItemSave) {
                alert('Please save short item first then finalize.');
                return false;
            }

            //  $scope.DBoyData.DeliveryIssuanceId = 0;
            if ($scope.DBoyData.DeliveryIssuanceId != "undefined" && $scope.DBoyData.DeliveryIssuanceId > 0) {

                $("#SavedShortItem").prop("disabled", true);
                $("#FinalizationAssignment").prop("disabled", true);
                var url = serviceBase + 'api/DeliveryAssignment/Finalization?DeliveryIssuanceId=' + $scope.DBoyData.DeliveryIssuanceId;
                $http.post(url).success(function (response) {

                    if (!response) {
                        alert("Assignment Finalization not done");

                    }
                    else {
                        alert("Assignment Finalization SUCCESSFULLY!!!");
                        window.location.reload();
                    }
                });
            }
            else {
                alert("there Is issue in Finalization");
            }
        }

    }
})();



app.controller('UploadIcCtrl', ['$scope', "$filter", "$http", '$modal', '$modalInstance', "FileUploader", "datato", function ($scope, $filter, $http, $modal, $modalInstance, FileUploader, datato) {


    $scope.vm = {};
    $scope.AssignmentsInfo = datato;




    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        $scope.PutImage = function (data) {

            if (data.RejectionComment == null || data.RejectionComment == undefined || data.RejectionComment == "" || data.IsPhysicallyVerified != false) {
                if (data.IsPhysicallyVerified == true) {
                    var url = serviceBase + "api/DeliveryAssignment/IcVerified";
                    var dataToPost = {
                        IsIcVerified: data.IsIcVerified,
                        IsPhysicallyVerify: data.IsPhysicallyVerified,
                        DeliveryIssuanceId: data.Id,
                        Comment: data.RejectionComment
                        // IcUploadedFile: $scope.AssignmentsInfo.uploadedfileName
                    };

                    $http.put(url, dataToPost)
                        .success(function (data) {
                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                                alert('Add Successfully');
                                $modalInstance.close(data);
                                location.reload();
                            }
                        })
                }
                else if (data.IsIcVerified == null || data.IsIcVerified == false) {
                    alert("Please Approve Assignment, Physical verify or Comment");
                }
                else {
                    var url = serviceBase + "api/DeliveryAssignment/IcVerified";
                    var dataToPost = {
                        IsIcVerified: data.IsIcVerified,
                        IsPhysicallyVerify: data.IsPhysicallyVerified,
                        DeliveryIssuanceId: data.Id,
                        Comment: data.RejectionComment
                        // IcUploadedFile: $scope.AssignmentsInfo.uploadedfileName
                    };

                    $http.put(url, dataToPost)
                        .success(function (data) {
                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                                alert('Add Successfully');
                                $modalInstance.close(data);
                                location.reload();
                            }
                        })
                }

            }
            else {
                var url1 = serviceBase + "api/DeliveryAssignment/IcVerified";
                var dataToPost1 = {
                    IsIcVerified: data.IsIcVerified,
                    IsPhysicallyVerify: data.IsPhysicallyVerified,
                    DeliveryIssuanceId: data.Id,
                    Comment: data.RejectionComment
                    // IcUploadedFile: $scope.AssignmentsInfo.uploadedfileName

                };

                $http.put(url1, dataToPost1)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            alert('Add Successfully');
                            $modalInstance.close(data);
                            location.reload();
                        }
                    })
            }

        };

    var uploader = $scope.uploader = new FileUploader({

        url: serviceBase + 'api/AssignmentICUpload/UploadFile'
    });
    //FILTERS

    uploader.filters.push({

        name: 'customFilter',
        fn: function (item /*{File|FileLikeObject}*/, options) {
            return this.queue.length < 10;
        }
    });

    //CALLBACKS

    uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        console.info('onWhenAddingFileFailed', item, filter, options);
    };
    uploader.onAfterAddingFile = function (fileItem) {
        console.info('onAfterAddingFile', fileItem);
        var fileExtension = '.' + fileItem.file.name.split('.').pop();
        fileItem.file.name = "IC_" + $scope.uploadIcImage.DeliveryIssuanceId;
    };
    uploader.onAfterAddingAll = function (addedFileItems) {
        console.info('onAfterAddingAll', addedFileItems);
    };
    uploader.onBeforeUploadItem = function (item) {
        console.info('onBeforeUploadItem', item);
    };
    uploader.onProgressItem = function (fileItem, progress) {
        console.info('onProgressItem', fileItem, progress);
    };
    uploader.onProgressAll = function (progress) {
        console.info('onProgressAll', progress);
    };
    uploader.onSuccessItem = function (fileItem, response, status, headers) {

        console.info('onSuccessItem', fileItem, response, status, headers);
        $scope.AssignmentsInfo.uploadedfileName = fileItem._file.name;
    };
    uploader.onErrorItem = function (fileItem, response, status, headers) {
        console.info('onErrorItem', fileItem, response, status, headers);
    };
    uploader.onCancelItem = function (fileItem, response, status, headers) {
        console.info('onCancelItem', fileItem, response, status, headers);
    };
    uploader.onCompleteItem = function (fileItem, response, status, headers) {

        console.info('onCompleteItem', fileItem, response, status, headers);
        console.log("File Name :" + fileItem._file.name);
        $scope.uploadedfileName = fileItem._file.name;
        $scope.IcImage = response.slice(1, -1); //For remove
        $scope.AssignmentsInfo.push($scope.IcImage);

        alert("File Uploaded Successfully");
    };
    uploader.onCompleteAll = function () {
        console.info('onCompleteAll');
    };
    console.info('uploader', uploader);
    //
}]);


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAssignmentTraedeInvoiceDispatch', ModalInstanceCtrlAssignmentTraedeInvoiceDispatch);

    ModalInstanceCtrlAssignmentTraedeInvoiceDispatch.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', '$compile', 'order', '$modal'];

    function ModalInstanceCtrlAssignmentTraedeInvoiceDispatch($scope, $http, $modalInstance, ngAuthSettings, $compile, order, $modal) {

        $scope.Data = order;
    }
})();

app.filter('secondsToTime', function () {

    function padTime(t) {
        return t < 10 ? "0" + t : t;
    }

    return function (_seconds) {
        if (typeof _seconds !== "number" || _seconds < 0)
            return "00:00:00";

        var hours = Math.floor(_seconds / 3600),
            minutes = Math.floor((_seconds % 3600) / 60),
            seconds = Math.floor(_seconds % 60);

        return padTime(hours) + ":" + padTime(minutes) + ":" + padTime(seconds);
    };
});

app.controller('AssignmentDirectionController', ['$scope', "$http", '$modal', '$modalInstance', 'assignmentId', function ($scope, $http, $modal, $modalInstance, assignmentId) {
    var url = serviceBase + "api/DeliveryTask/AssignmentDirection?assignmentId=" + assignmentId;
    $scope.AssignmentsInfo = { AssignmentId: 0 };
    $http.get(url)
        .success(function (data) {
            if (data.Root != null) {
                initMap(data.Root);
                $scope.showMap = true;
                //document.getElementById("msgdiv").style.dispaly="none";
                $scope.AssignmentsInfo = data;
                var addresses = [];
                for (let i = 0; i < $scope.AssignmentsInfo.Root.routes[0].legs.length; i++) {
                    addresses.push({
                        start: $scope.AssignmentsInfo.Root.routes[0].legs[i].start_address,
                        end: $scope.AssignmentsInfo.Root.routes[0].legs[i].end_address
                    });
                }
                $scope.address = addresses;
                //var totalMinutes = $scope.AssignmentsInfo.AssignmentDuration;
                //var hours = Math.floor(totalMinutes / 60);
                //var minutes = totalMinutes % 60;
                //$scope.AssignmentsInfo.AssignmentDuration = parseFloat(hours + '.' + minutes);
                //totalMinutes = $scope.AssignmentsInfo.ReturnDuration;
                //hours = Math.floor(totalMinutes / 60);
                // minutes = totalMinutes % 60;
                //$scope.AssignmentsInfo.ReturnDuration = parseFloat(hours + '.' + minutes);
                //totalMinutes = $scope.AssignmentsInfo.TotalUnloadingDuration;
                //hours = Math.floor(totalMinutes / 60);
                //minutes = totalMinutes % 60;
                //$scope.AssignmentsInfo.TotalUnloadingDuration = parseFloat(hours + '.' + minutes);
            } else {
                //document.getElementById("mapdiv").style.dispaly = "none";            
                alert("Sorry we are unable to get direction for this assignment");
            }

        })
        .error(function (data) {
            //document.getElementById("mapdiv").style.dispaly = "none";            
            console.log(data);
        });

    function initMap(root) {
        const directionsService = new google.maps.DirectionsService();
        const directionsRenderer = new google.maps.DirectionsRenderer();
        const map = new google.maps.Map(document.getElementById("map11223"), {
            zoom: 6,
            center: { lat: root.routes[0].legs[0].start_location.lat, lng: root.routes[0].legs[0].start_location.lng },
        });
        directionsRenderer.setMap(map);


        calculateAndDisplayRoute(directionsService, directionsRenderer, root);

        //document.getElementById("submit").addEventListener("click", () => {
        //    calculateAndDisplayRoute(directionsService, directionsRenderer, root);
        //});
    }

    function calculateAndDisplayRoute(directionsService, directionsRenderer, root) {
        const waypts = [];
        //const checkboxArray = document.getElementById("waypoints");

        for (let i = 1; i < root.routes[0].legs.length; i++) {
            waypts.push({
                location: { lat: root.routes[0].legs[i].start_location.lat, lng: root.routes[0].legs[i].start_location.lng },
                stopover: true,
            });
        }

        //var lastLocation = root.routes[0].legs[0].start_location.lat + "," + root.routes[0].legs[0].start_location.lng;
        var pointA = new google.maps.LatLng(root.routes[0].legs[0].start_location.lat, root.routes[0].legs[0].start_location.lng);
        var radiusInKm = 0.01;
        var pointB = pointA.destinationPoint(90, radiusInKm);

        directionsService.route(
            {
                origin: root.routes[0].legs[0].start_location.lat + "," + root.routes[0].legs[0].start_location.lng, //document.getElementById("start").value,
                //destination: root.routes[0].legs[root.routes[0].legs.length - 1].start_location.lat + "," + root.routes[0].legs[root.routes[0].legs.length - 1].start_location.lng,
                destination: pointB,
                waypoints: waypts,
                optimizeWaypoints: true,
                travelMode: google.maps.TravelMode.DRIVING,
            },
            (response, status) => {
                if (status === "OK") {
                    directionsRenderer.setDirections(response);
                    const route = response.routes[0];
                    const summaryPanel = document.getElementById("directions-panel");
                    summaryPanel.innerHTML = "<br><b><u>Route </u></b><br>";

                    // For each route, display summary information.
                    for (let i = 0; i < route.legs.length; i++) {
                        //const routeSegment = i + 1;
                        //summaryPanel.innerHTML +=
                        //    //"<b>Route Segment: " + routeSegment + "</b><br>";
                        //    "";
                        summaryPanel.innerHTML += "<b>" + route.legs[i].start_address + "</b><br> to <br>";
                        summaryPanel.innerHTML += "<b>" + route.legs[i].end_address + "</b><br>";
                        summaryPanel.innerHTML += route.legs[i].distance.text + "<br><br>";
                    }
                } else {
                    window.alert("Directions request failed due to " + status);
                }
            }
        );
    }


    Number.prototype.toRad = function () {
        return this * Math.PI / 180;
    }

    Number.prototype.toDeg = function () {
        return this * 180 / Math.PI;
    }

    google.maps.LatLng.prototype.destinationPoint = function (brng, dist) {
        dist = dist / 6371;
        brng = brng.toRad();

        var lat1 = this.lat().toRad(), lon1 = this.lng().toRad();

        var lat2 = Math.asin(Math.sin(lat1) * Math.cos(dist) +
            Math.cos(lat1) * Math.sin(dist) * Math.cos(brng));

        var lon2 = lon1 + Math.atan2(Math.sin(brng) * Math.sin(dist) *
            Math.cos(lat1),
            Math.cos(dist) - Math.sin(lat1) *
            Math.sin(lat2));

        if (isNaN(lat2) || isNaN(lon2)) return null;

        return lat2.toDeg() + "," + lon2.toDeg();
        //return new google.maps.LatLng(lat2.toDeg(), lon2.toDeg());
    }

}]);