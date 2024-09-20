

(function () {
    'use strict';

    angular
        .module('app')
        .controller('orderdetailsController', orderdetailsController);
    orderdetailsController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', '$filter', 'WarehouseService', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "peoplesService", '$modal', 'BillPramotionService', "$modalInstance", "PaymentType", "OrderColor", "$rootScope"];
    function orderdetailsController($scope, OrderMasterService, OrderDetailsService, $filter, WarehouseService, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService, $modal, BillPramotionService, $modalInstance, PaymentType, OrderColor, $rootScope) {
        $scope.ok = function () { $modalInstance.close(); };
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Payments = {};
        $scope.OrderColor = OrderColor;
        $scope.currentPageStores = {};
        $scope.isShoW = false;
        $scope.PaymentType = PaymentType;
        $scope.isShoWOffer = false;
        $scope.finaltable = false;
        $scope.dispatchtable = false;
        $scope.OrderDetails = {};
        $scope.OrderData = {};
        $scope.OrderData1 = {};
        //$scope.priorityorder = false;
        var d = OrderMasterService.getDeatil();
        $scope.ShowAlartMsg = false;
        if (d.OrderType == 4 && PaymentType.indexOf('Cash') > -1) {
            $scope.ShowAlartMsg = true;
        }
        $scope.signdata = {};
        $scope.viewsign = function () {

            var Signimg = $scope.signdata.Signimg;
            if (Signimg != null) {
                window.open(Signimg);
            } else { alert("no sign present") }
        }
        //$scope.showprio = function () {
        //    debugger
        //    var url = serviceBase + 'api/DeliveryCapacity/SetPriorityOrders?OrderId=' + d.OrderId;
        //    $http.get(url).then(function (results) {
        //        debugger
        //        $scope.priorityorder = results.data.status;
        //        console.log(results, "results");

        //    });
        //}
        //$scope.showprio();
        $scope.count = 1;
        $scope.OrderData = d;
        if ($scope.OrderData.Status == "Order Canceled") { //$scope.OrderData.Status == "Cancel" ||
            $scope.finaltable = false;
            $scope.dispatchtable = false;
            //document.getElementById("btnSave").hidden = true;
        }
        if ($scope.OrderData.Status == "Ready to Dispatch") {
            //document.getElementById("tbl").hidden = true;
            //need to change
        }

        $scope.orderDetails = d.orderDetails;
        $scope.orderDetailsINIT = d.orderDetails;
        $scope.checkInDispatchedID = $scope.orderDetails[0].OrderId;
        $scope.pramotions = {};
        $scope.selectedpramotion = {};
        BillPramotionService.getbillpramotion().then(function (results) {
            $scope.pramotions = results.data;
        }, function (error) {
        });
        //for display info in print final order
        OrderMasterService.saveinfo($scope.OrderData);
        // end 
        $scope.Finalbutton = false;
        $scope.displayDiscountamount = true;
        if ($scope.OrderData.Status == "Cancel") {
            $scope.cancledispatch = true;
            $scope.Finalbutton = true;
            $scope.ReDispatchButton = true;
        }
        $scope.callForDropdown = function () {

            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.checkInDispatchedID;
            $http.get(url)
                .success(function (data) {

                    if (data == "null") {
                        $scope.dispatchtable = false;
                    } else {
                        $scope.signdata = data;

                        OrderMasterService.saveDispatch(data);
                        $scope.dispatchtable = true;
                        $scope.DBname = {};
                        $scope.DBname = data.DboyName;
                        $scope.OrderData1 = data;
                        $scope.OrderData1.OrderType = d.OrderType;
                        if ($scope.OrderData1.ReDispatchCount > 1) {
                            $scope.ReDispatchButton = true;
                        }
                    }
                });
        };

        //..................ExportMethod......................//
        $scope.Export1 = function () {
            // 
            $scope.datacheck = $scope.orderDetailsDisp;
            alasql('SELECT ItemId,Category as ABCClassification,itemname,itemcode,UnitPrice,price,MinOrderQty, qty,AmtWithoutTaxDisc,AmtWithoutAfterTaxDisc,TaxPercentage,TaxAmmount,TotalAmountAfterTaxDisc,TotalAmt INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.datacheck]);
        };
        //.............ExportMethod.............//

        //..................ExportMethod......................//
        $scope.Export2 = function () {
            //  
            $scope.datacheck1 = $scope.orderDetails;

            alasql('SELECT ItemId,Category as ABCClassification,itemname,itemcode,UnitPrice,price,MinOrderQty, qty,AmtWithoutTaxDisc,AmtWithoutAfterTaxDisc,TaxPercentage,TaxAmmount,TotalAmountAfterTaxDisc,TotalAmt INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.datacheck1]);
        };
        //$scope.IsBtnClk = true
        //$scope.Prioritize = function (OrderData) {
        //    debugger
        //    if (confirm("Are you sure want to prioritize this order?")) {
        //        var url = serviceBase + 'api/DeliveryCapacity/SetPriorityOrders?OrderId=' + OrderData.OrderId + '&IsBtnClk=' + $scope.IsBtnClk;
        //        $http.get(url).then(function (results) { 
        //            if (results.data.Msg == 'Success') {
        //                alert("Success");
        //                $scope.Hidepriorityorder = results.data.Msg;
        //            }
        //        });

        //    } else {

        //    }

        //}
        //.............ExportMethod.............//
        $scope.callForDropdown();
        // checking order is dispatched or not here
        var url = serviceBase + 'api/OrderDispatchedDetails?id=' + $scope.checkInDispatchedID;
        $http.get(url).success(function (data1) {

            if (data1.length > 0) {
                $scope.count = 0;
                $scope.orderDetails11 = data1;
                $scope.orderDetailsDisp = data1;
                $scope.msg = "Order is Dispatched";
                document.getElementById("btnSave").hidden = true;
            }
            $http.get(serviceBase + "api/freeitem/SkFree?oderid=" + $scope.checkInDispatchedID).then(function (results) {

                $scope.freeitem = results.data;
                try {
                    if (results.data.length > 0) {
                        $scope.isShoW = true;
                    }
                } catch (ex) { }
            }, function (error) {
            });
            //
            $http.get(serviceBase + "api/offer/GetOfferItem?oderid=" + $scope.checkInDispatchedID).then(function (results) {
                //

                $scope.Offeritem = results.data;


                try {
                    if (results.data.length > 0) {
                        $scope.isShoWOffer = true;
                    }
                } catch (ex) { }
            }, function (error) {
            });
        });
        //end

        // check Final master Sattled order done
        var url = serviceBase + 'api/OrderDispatchedMasterFinal?id=' + $scope.checkInDispatchedID;
        $http.get(url).success(function (data1) {

            if (data1 == "null") {
                $scope.SHOWPAYMENT = false;
            } else {
                $scope.SHOWPAYMENT = true;
                $scope.SHOWPAYMENTTABLE = data1;
                $scope.Finalbutton = true;
                $scope.myMasterbutton = true;
                $scope.finalLastReturn = true;
                //$scope.cancledispatch = true;
                $scope.FinalbuttonLAST = true;
                $scope.cancledispatch = true;
                $scope.ReDispatchButton = true;
                $scope.FinalinvoiceButtonWithoutMasterLast = true;
            }
        });
        //end

        // update orderdispatch master
        $scope.ReDispatch = function (Dboy) {
            try { var obj = JSON.parse(Dboy); } catch (err) { alert("Select Delivery boy") }
            var dboyMob = obj.Mobile;
            console.log(dboyMob);
            $scope.Did = $scope.orderDetailsDisp[0].OrderDispatchedMasterId;
            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.Did + '&DboyNo=' + obj.Mobile;
            $http.put(url)
                .success(function (data) {

                    if (data.length > 0) { }
                    alert("Delivey Boy update successfully");
                    window.location = "#/orderMaster";
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                });

        }
        // end

        $scope.Itemcount = 0;
        ///check for return

        var url = serviceBase + 'api/OrderDispatchedDetailsReturn?id=' + $scope.checkInDispatchedID;
        $http.get(url).success(function (data) {

            if (data.length > 0) {
                $scope.count = 0;
                $scope.Finalbutton = true;  // disable chckbox
                //$scope.cancledispatch = true; // disable cancle button
                // $scope.dboyname = data.DboyName;
                $scope.orderDetailsR = data;
                $scope.myVar = true;
                $scope.myVar1 = true;
                $scope.finalobj = [];
                document.getElementById("FinalInvoice").hidden = false;
                for (var i = 0; i < $scope.orderDetailsINIT.length; i++) {
                    for (var j = 0; j < $scope.orderDetailsR.length; j++) {
                        if ($scope.orderDetailsINIT[i].ItemId == $scope.orderDetailsR[j].ItemId) {
                            //if ($scope.orderDetailsINIT[i].qty == $scope.orderDetails1[j].qty) {
                            //    $scope.orderDetailsINIT.splice(i, 1);
                            //} else {
                            $scope.orderDetailsINIT[i].qty = $scope.orderDetailsINIT[i].qty - $scope.orderDetailsR[j].qty;

                            //}
                            $scope.finalobj.push($scope.orderDetailsINIT[i]);
                        }
                    }
                }
            }
            else {
                $scope.myVar = false;
                //document.getElementById("btnFinalInvoice").hidden = true;
            }
        });
        //end check return
        $scope.settleAmountLast = function () {

            return $scope.PaymentsLAST.CheckAmount + $scope.PaymentsLAST.ElectronicAmount + $scope.PaymentsLAST.CashAmount;
        }

        $scope.duensettleequalLAST = function () {
            var payamount = Math.round($scope.myMaster.GrossAmount);
            if ($scope.settleAmountLast() == payamount) {
                return true;
            } else {

                return false;
            }
        }

        $scope.settleAmount = function () {

            return $scope.Payments.CheckAmount + $scope.Payments.ElectronicAmount + $scope.Payments.CashAmount;
        }

        $scope.duensettleequal = function () {

            if ($scope.settleAmount() == $scope.OrderData1.GrossAmount) {
                return true;
            } else {

                return false;
            }
        }

        ////check for final detail Sattled
        //var url = serviceBase + 'api/OrderDispatchedDetailsFinal?id=' + $scope.checkInDispatchedID;
        //$http.get(url).success(function (data) {

        //    if (data.length > 0) {

        //        $scope.count = 0;
        //        $scope.orderDetailsFinal = data;
        //        $scope.finaltable = true;
        //        $scope.dispatchtable = false;
        //        $scope.finalAmountLAST = 0;
        //        $scope.finalTaxAmountLAST = 0;
        //        $scope.finalGrossAmountLAST = 0;
        //        $scope.finalTotalTaxAmountLAST = 0;
        //        $scope.finalLast = true;

        //        if ($scope.orderDetailsFinal[0].FinalOrderDispatchedMasterId == 0) {
        //            for (var i = 0; i < $scope.orderDetailsFinal.length; i++) {
        //                $scope.finalAmountLAST = $scope.finalAmountLAST + $scope.orderDetailsFinal[i].TotalAmt;
        //                $scope.finalTaxAmountLAST = $scope.finalTaxAmountLAST + $scope.orderDetailsFinal[i].TaxAmmount;
        //                $scope.finalGrossAmountLAST = $scope.finalGrossAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
        //                $scope.finalTotalTaxAmountLAST = $scope.finalTotalTaxAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
        //            }


        //            $scope.TotalAmount = $scope.finalAmountLAST;
        //            $scope.TaxAmount = $scope.finalTaxAmountLAST;
        //            $scope.GrossAmount = $scope.finalGrossAmountLAST;
        //            $scope.DiscountAmount = $scope.finalTotalTaxAmountLAST - $scope.finalAmountLAST;


        //            $scope.myDetail = {};
        //            $scope.myMaster = {};
        //            $scope.myDetail = $scope.orderDetailsFinal;
        //            $scope.myMaster = [];
        //            var newdata = angular.copy($scope.OrderData1);
        //            $scope.myMaster = newdata;

        //            $scope.myMaster.TotalAmount = $scope.TotalAmount;
        //            $scope.myMaster.TaxAmount = $scope.TaxAmount;
        //            $scope.myMaster.GrossAmount = $scope.GrossAmount;
        //            $scope.myMaster.WalletAmount = $scope.WalletAmount;
        //            $scope.myMaster.DiscountAmount = $scope.DiscountAmount;
        //            $scope.FinalinvoiceButtonLast = true;
        //            $scope.showpaybleButton = true;
        //            $scope.finalLastReturn = true;

        //        }

        //    }
        //    else {
        //        $scope.finalLastReturn = true;
        //        $scope.FinalinvoiceButtonWithoutMasterLast = true;
        //    }
        //});

        $scope.showInvoiceWithoutMasterFinal = function () {

            OrderMasterService.saveDispatch($scope.myDetail);
            OrderMasterService.save1($scope.myMaster);
            console.log("Order Invoice  called ...");
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "OrderInvoiceModel.html",
                    controller: "ModalInstanceCtrlOrderInvoiceDispatch",
                    resolve:
                    {
                        order: function () {
                            return $scope.myDetail
                        }
                    }
                });
            modalInstance.result.then(function () {

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.showInvoice = function (data, OrderData) {
            $rootScope.InvoiceAmountInWord = "";
            OrderMasterService.save1(data);
            debugger;
            var amount = OrderData.GrossAmount - (OrderData.DiscountAmount ? OrderData.DiscountAmount : 0);
            $http.get(serviceBase + 'api/OrderMaster/GetInvoiceAmountToWord?Amount=' + amount).then(function (results) {
                $rootScope.InvoiceAmountInWord = results.data;
            });

            $rootScope.TCSPercent = 0;
            var url = serviceBase + "api/OrderMaster/GetTCSPercent?CustomerId=" + OrderData.CustomerId;
            $http.get(url).success(function (results) {
                $rootScope.TCSPercent = results;
            });


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "OrderInvoiceModel.html",
                    controller: "ModalInstanceCtrlOrderInvoice1",
                    resolve:
                    {
                        order: function () {
                            return data
                        }
                    }
                });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.statesDatas = [];
        $scope.showInvoiceDispatch = function (Detail, master) {
            $http.get(serviceBase + 'api/OrderMaster/GetStateCode' + "?WarehouseId=" + master.WarehouseId).then(function (results) {
                debugger;
                $scope.statesDatas = results.data[0];
                console.log("-----------------------------------------", results.data[0]);
            })
            if ($scope.OrderData.OrderType == 5) {
                var url = serviceBase + "api/OrderMaster/GetTradeInvoice?OrderId=" + master.OrderId;
                $http.get(url).success(function (response) {
                    //alert(response);
                    // window.open(response);

                    var modalInstance;
                    modalInstance = $modal.open(
                        {
                            templateUrl: "TradeInvoiceModel.html",
                            controller: "ModalInstanceCtrlTraedeInvoiceDispatch",
                            resolve:
                            {
                                order: function () {
                                    return response;
                                }
                            }
                        });
                    modalInstance.result.then(function () {
                    },
                        function () {
                            console.log("Cancel Condintion");
                        })
                });
            }
            else {
                OrderMasterService.saveDispatch(Detail);
                OrderMasterService.save1(master);

                $rootScope.InvoiceAmountInWord = "";
                var modalInstance;
                var amount = master.GrossAmount - (master.DiscountAmount ? master.DiscountAmount : 0);
                $http.get(serviceBase + 'api/OrderMaster/GetInvoiceAmountToWord?Amount=' + amount).then(function (results) {
                    $rootScope.InvoiceAmountInWord = results.data;
                });

                $rootScope.TCSPercent = 0;
                $scope.GetTCSPercent = function () {
                    var url = serviceBase + "api/OrderMaster/GetTCSPercent?CustomerId=" + $scope.OrderData1.CustomerId;
                    $http.get(url).success(function (results) {
                        $rootScope.TCSPercent = results;
                    });
                };

                $scope.GetTCSPercent();

                modalInstance = $modal.open(
                    {
                        templateUrl: "OrderInvoiceModel.html",
                        controller: "ModalInstanceCtrlOrderInvoiceDispatch",
                        resolve:
                        {
                            order: function () {
                                return Detail
                            }
                        }
                    });
                modalInstance.result.then(function () {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            }
        };




        $scope.DDeliveryCanceled = function (data) {

            console.log("Modal opened tax");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myImageModal.html",
                    controller: "ModalDeliveryCancelledDraftCtrl", resolve: { Image: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () { })

        };








        //$http.get(serviceBase + 'api/DeliveryBoy').then(function (results) {

        //    $scope.NewUser = results.data;
        //    var wid = parseInt(d.WarehouseId);
        //    $scope.DeliveryBoyfilterData = $filter('filter')($scope.NewUser, function (value) {
        //        return value.WarehouseId === wid;
        //    });
        var urlData = serviceBase + "api/DeliveryBoy/WarehousebasedDeliveryBoyRole?WarehouseId=" + d.WarehouseId;
        $http.get(urlData).success(function (response) {
            $scope.NewUser = response;
            $scope.DeliveryBoyfilterData = $scope.NewUser;

            $scope.User = [];
            $scope.User = $scope.DeliveryBoyfilterData;//changes on 17/9/2019
            console.log("Got Dboy people collection");
            console.log($scope.User);
        });

        // end
        for (var i = 0; i < $scope.orderDetails.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }

        $scope.totalfilterprice = 0;

        _.map($scope.OrderData.orderDetails, function (obj) {
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log("$scope.OrderData");
            console.log($scope.totalfilterprice);

        });

        var s_col = false;
        var del = '';

        var ss_col = '';

        $scope.set_color = function (orderDetail, orderData) {

            if (orderData.OrderType != 8) {
                if (!orderDetail.IsFreeItem) {
                    if (orderDetail.qty > orderDetail.CurrentStock || orderDetail.IsRed == true) {
                        return { background: "#ff9999" }
                    }
                }
                else if (orderDetail.IsFreeItem) {
                    if (orderDetail.qty > orderDetail.CurrentStock) {
                        return { background: "#ff9999" }
                    }
                }
            }
            else {
                return { background: "#ffffff" }
            }
        }

        $scope.giveDiscount = function (DISCOUNT) {
            //$scope.discountAmount = (DISCOUNT * $scope.OrderData.TotalAmount) / 100;
            //$scope.OrderData.DiscountAmount = $scope.discountAmount;
            //$scope.FinalTotalAmount = $scope.OrderData.TotalAmount - $scope.discountAmount;
            //$scope.displayDiscountamount = false;       
        }

        $scope.selcteddboy = function (db) {
            $scope.Dboy = JSON.parse(db);
        }


        $scope.selcteddQTR = function (data) {

            $scope.QtyChangeReason = data;
        }

        $scope.selectedItemChanged = function (id) {

            $('#' + id).removeClass('hd');
        }

        $scope.CheckForReason = function (id) {

            if ($('#' + id).QtyChangeReason == undefined) {
                $scope.skills = [];
                $scope.skills.push({ 'id': id })
                alert('Please fill the Reason');
            }
        }
        $scope.save = function (orderDetail) {
            var orderData = $scope.OrderData;
            let today = new Date();
            let orderdate = new Date(orderData.CreatedDate);
            debugger;
            let delivryDate = new Date(orderData.ExpectedRtdDate);
            if ($scope.OrderColor != "lightblue") {

                //var timeDiff = Math.abs(delivryDate.getTime() - orderdate.getTime());
                //var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
                //var diff = (delivryDate.getTime() - orderdate.getTime()) / 1000;
                //diff /= (60 * 60);
                //var hours = Math.abs(Math.round(diff));

                //if (hours > 48) {
                //    delivryDate.setDate(delivryDate.getDate() - 1);//24 hours
                //}
                //else {
                //    delivryDate.setDate(delivryDate.getDate() - 2);//48 hours
                //}
                if (delivryDate <= today) {
                    var data = $scope.orderDetails;
                    $scope.Disptotalqty = 0;
                    for (var d = 0; d < $scope.orderDetails.length; d++) {

                        $scope.Disptotalqty += $scope.orderDetails[d].qty;
                    }
                    if ($scope.Disptotalqty > 0) {
                        $("#btnSave").prop("disabled", true);
                        var url = serviceBase + 'api/OrderDispatchedMaster/GetFreeItemStock?orderId=' + data[0].OrderId;
                        $http.get(url).success(function (results) {

                            // if (results != "0") { 
                            console.log($scope.Dboy);
                            var flag = true;
                            angular.forEach($scope.skills, function (value, key) {
                                for (var i = 0; i < $scope.orderDetails.length; i++) {
                                    data = $scope.orderDetails[i];
                                    if (data.ItemId == value.id) {
                                        if (data.QtyChangeReason == undefined) {
                                            alert("Please Fill Reason");
                                            flag = false;
                                            break;
                                        }
                                    }
                                }
                            });
                            for (var i = 0; i < $scope.orderDetails.length; i++) {

                                data = $scope.orderDetails[i];
                                //if (data.IsFreeItem == true) {
                                //  data.CurrentStock = data.qty + 1;
                                //  break;
                                // }
                                //else 
                                if ((data.qty || data.qty == null || data.qty == undefined) > data.CurrentStock && data.Deleted == false && orderData.OrderType != 8) {
                                    alert("your stock not sufficient please purchase or remove item then dispatched: ( " + data.itemname + " )This much Qty required : " + data.qty);
                                    $("#btnSave").removeAttr("disabled");
                                    flag = false;
                                    break;
                                }
                            }

                            //double check for inventory
                            for (var i = 0; i < $scope.orderDetails.length; i++) {
                                $scope.CheckStockWithNumber = {};
                                var first = true;
                                for (var j = i; j < $scope.orderDetails.length; j++) {
                                    if (first) {
                                        $scope.CheckStockWithNumber = $scope.orderDetails[j];
                                        first = false;
                                    }
                                    //else if ($scope.CheckStockWithNumber.itemNumber == $scope.orderDetails[j].itemNumber) { before Mrp
                                    //else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId) {
                                    else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId && $scope.CheckStockWithNumber.IsFreeItem == $scope.orderDetails[j].IsFreeItem) {
                                        var Stockcount = 0;
                                        Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;

                                        if (Stockcount > $scope.orderDetails[j].CurrentStock && Stockcount > 0 && orderData.OrderType != 8) {
                                            alert("your stock not sufficient please purchase or remove item: ( " + $scope.orderDetails[j].itemname + " )This much Qty required : " + Stockcount);
                                            $("#btnSave").removeAttr("disabled");
                                            flag = false;
                                            return false;
                                        }

                                    }
                                }
                            }

                            if ($scope.Dboy == undefined) {
                                alert("Please Select Delivery Boy");
                                $("#btnSave").removeAttr("disabled");
                                flag = false;

                            }
                            if (flag == true) {
                                try {
                                    var obj = ($scope.Dboy);
                                } catch (err) {
                                    alert("Please Select Delivery Boy");
                                    console.log(err);

                                }
                                $scope.OrderData["DboyName"] = obj.DisplayName;
                                $scope.OrderData["DboyMobileNo"] = obj.Mobile;
                                $scope.OrderData["DBoyId"] = obj.PeopleID;
                                $scope.OrderData;
                                //console.log($scope.OrderData);
                                //console.log("save orderdetailfunction");
                                //console.log("Selected Pramtion");
                                //console.log($scope.selectedpramotion);
                                //for (var i = 0; i < $scope.orderDetails.length; i++) {
                                //    console.log($scope.orderDetails[i]);
                                //    console.log($scope.orderDetails[i].DiscountPercentage);
                                //    //$scope.orderDetails[i].DiscountPercentage = $scope.selectedpramotion; this code create issue in dispatched order
                                //    console.log($scope.orderDetails[i].DiscountPercentage);
                                //}
                                $("#btnSave").prop("disabled", true);
                                //var url = serviceBase + 'api/OrderDispatchedMaster';
                                var url = serviceBase + 'api/OrderDispatchedDetails/V1';
                                $http.post(url, $scope.OrderData)
                                    .success(function (data) {
                                        //$scope.dispatchedMasterID = data.OrderDispatchedMasterId;
                                        //$scope.orderDetails = data.orderDetails;
                                        // $scope.dispatchedDetail();
                                        //$modalInstance.close(data);
                                        //console.log("Error Gor Here");
                                        //console.log(data);
                                        if (data) {
                                            alert(data);
                                            $modalInstance.close(data);
                                        }

                                    })
                                    .error(function (data) {
                                        alert(data.ErrorMessage);

                                    })
                            }

                        });

                    } else {
                        debugger;
                        alert(" You can't dispatched zero qty line item"); return false;
                    }
                } else {
                    // var m = moment(orderData.Deliverydate, 'DD/MM/YYYY', true);                
                    //alert("Currently you are not able to dispatched this Order on " + delivryDate); return false;
                    alert("Currently, You can't dispatch this order. This order is available for dispatch on " + orderData.ExpectedRtdDate); return false;
                }
            }
            else {
                var data = $scope.orderDetails;
                $scope.Disptotalqty = 0;
                for (var d = 0; d < $scope.orderDetails.length; d++) {

                    $scope.Disptotalqty += $scope.orderDetails[d].qty;
                }
                if ($scope.Disptotalqty > 0) {
                    $("#btnSave").prop("disabled", true);
                    var url = serviceBase + 'api/OrderDispatchedMaster/GetFreeItemStock?orderId=' + data[0].OrderId;
                    $http.get(url).success(function (results) {

                        // if (results != "0") { 
                        console.log($scope.Dboy);
                        var flag = true;
                        angular.forEach($scope.skills, function (value, key) {
                            for (var i = 0; i < $scope.orderDetails.length; i++) {
                                data = $scope.orderDetails[i];
                                if (data.ItemId == value.id) {
                                    if (data.QtyChangeReason == undefined) {
                                        alert("Please Fill Reason");
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        });
                        for (var i = 0; i < $scope.orderDetails.length; i++) {

                            data = $scope.orderDetails[i];

                            if ((data.qty || data.qty == null || data.qty == undefined) > data.CurrentStock && data.Deleted == false && orderData.OrderType != 8) {
                                alert("your stock not sufficient please purchase or remove item then dispatched: ( " + data.itemname + " )This much Qty required : " + data.qty);
                                $("#btnSave").removeAttr("disabled");
                                flag = false;
                                break;
                            }
                        }

                        //double check for inventory
                        for (var i = 0; i < $scope.orderDetails.length; i++) {
                            $scope.CheckStockWithNumber = {};
                            var first = true;
                            for (var j = i; j < $scope.orderDetails.length; j++) {
                                if (first) {
                                    $scope.CheckStockWithNumber = $scope.orderDetails[j];
                                    first = false;
                                }
                                else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId && $scope.CheckStockWithNumber.IsFreeItem == $scope.orderDetails[j].IsFreeItem) {
                                    var Stockcount = 0;
                                    Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;

                                    if (Stockcount > $scope.orderDetails[j].CurrentStock && Stockcount > 0 && orderData.OrderType != 8) {
                                        alert("your stock not sufficient please purchase or remove item: ( " + $scope.orderDetails[j].itemname + " )This much Qty required : " + Stockcount);
                                        $("#btnSave").removeAttr("disabled");
                                        flag = false;
                                        return false;
                                    }

                                }
                            }
                        }

                        if ($scope.Dboy == undefined) {
                            alert("Please Select Delivery Boy");
                            $("#btnSave").removeAttr("disabled");
                            flag = false;

                        }
                        if (flag == true) {
                            try {
                                var obj = ($scope.Dboy);
                            } catch (err) {
                                alert("Please Select Delivery Boy");
                                console.log(err);

                            }
                            $scope.OrderData["DboyName"] = obj.DisplayName;
                            $scope.OrderData["DboyMobileNo"] = obj.Mobile;
                            $scope.OrderData["DBoyId"] = obj.PeopleID;
                            $scope.OrderData;
                            //console.log($scope.OrderData);

                            $("#btnSave").prop("disabled", true);
                            var url = serviceBase + 'api/OrderDispatchedDetails/V1';
                            $http.post(url, $scope.OrderData)
                                .success(function (data) {
                                    if (data) {
                                        alert(data);
                                        $modalInstance.close(data);
                                    }

                                })
                                .error(function (data) {
                                    alert(data.ErrorMessage);

                                })
                        }

                    });

                } else { alert(" You can't dispatched zero qty line item"); return false; }

            }
        };


        $scope.open = function (item) {
            console.log("in open");
            console.log(item);


        };

        $scope.invoice = function (invoice) {
            console.log("in invoice Section");
            console.log(invoice);

        };
        // cancle dispatch
        $scope.CancleDispatch = function () {
            var status = "cancle";

            var url = serviceBase + 'api/OrderDispatchedDetails?cancle=' + status;
            $http.put(url, $scope.orderDetailsDisp)
                .success(function (data) {

                    alert('Cancle successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
        //end

        // add payment FOR DISPATCH FINAL
        $scope.Payments = {
            PaymentAmount: null,
            CheckNo: null,
            CheckAmount: null,
            ElectronicPaymentNo: null,
            ElectronicAmount: null,
            CashAmount: null
        }
        // final bill sattled
        $scope.SaveFinalSattled = function (Payments) {

            $scope.Payments;
            $scope.Payments.PaymentAmount = $scope.OrderData1.GrossAmount
            $scope.finalDataMaster = $scope.OrderData1;
            $scope.finalDataMaster["PaymentAmount"] = $scope.Payments.PaymentAmount;
            $scope.finalDataMaster["CheckNo"] = $scope.Payments.CheckNo;
            $scope.finalDataMaster["CheckAmount"] = $scope.Payments.CheckAmount;
            $scope.finalDataMaster["ElectronicPaymentNo"] = $scope.Payments.ElectronicPaymentNo;
            $scope.finalDataMaster["ElectronicAmount"] = $scope.Payments.ElectronicAmount;
            $scope.finalDataMaster["CashAmount"] = $scope.Payments.CashAmount;


            var url = serviceBase + 'api/OrderDispatchedMasterFinal';
            $http.post(url, $scope.finalDataMaster)
                .success(function (data) {

                    $scope.FdispatchedMasterID = data.FinalOrderDispatchedMasterId;
                    alert('payment insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    $scope.dispatchedDetailFinal();
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.FinalOrderDispatchedMasterId == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
        // dispatch detail final add
        $scope.dispatchedDetailFinal = function () {
            for (var i = 0; i < $scope.orderDetailsDisp.length; i++) {
                $scope.orderDetailsDisp[i].FinalOrderDispatchedMasterId = $scope.FdispatchedMasterID;
                //if ($scope.orderDetailsDisp[i].qty > $scope.orderDetailsDisp[i].CurrentStock) {
                //    delete $scope.orderDetailsDisp[i];
                //}
            }

            $scope.orderDetailsDisp;

            //var url = serviceBase + 'api/OrderDispatchedDetailsFinal';
            //$http.post(url, $scope.orderDetailsDisp)
            //    .success(function (data) {

            //        alert('insert successfully');
            //        // location.reload();
            //        window.location = "#/orderMaster";
            //        console.log("Error Gor Here");
            //        console.log(data);
            //        if (data.id == 0) {

            //            $scope.gotErrors = true;
            //            if (data[0].exception == "Already") {
            //                console.log("Got This User Already Exist");
            //                $scope.AlreadyExist = true;
            //            }
            //        }
            //        else {
            //        }
            //    })
            //    .error(function (data) {
            //        console.log("Error Got Heere is ");
            //        console.log(data);

            //    })
        }
        // FOR AFTER RETURN LAST PAYMENT

        $scope.PaymentsLAST = {
            PaymentAmount: null,
            CheckNo: null,
            CheckAmount: null,
            ElectronicPaymentNo: null,
            ElectronicAmount: null,
            CashAmount: null
        }
        $scope.SaveFinalSattledLAST = function (PaymentsLAST) {
            $scope.PaymentsLAST;


            var payamount = parseInt($scope.myMaster.GrossAmount);
            $scope.PaymentsLAST.PaymentAmount = payamount;



            $scope.finalDataMasterLAST = $scope.OrderData1;

            if ($scope.finalAmountLAST > 0) {
                $scope.finalDataMasterLAST.TotalAmount = $scope.finalAmountLAST;
            }


            $scope.finalDataMasterLAST["PaymentAmount"] = $scope.PaymentsLAST.PaymentAmount;
            $scope.finalDataMasterLAST["CheckNo"] = $scope.PaymentsLAST.CheckNo;
            $scope.finalDataMasterLAST["CheckAmount"] = $scope.PaymentsLAST.CheckAmount;
            $scope.finalDataMasterLAST["ElectronicPaymentNo"] = $scope.PaymentsLAST.ElectronicPaymentNo;
            $scope.finalDataMasterLAST["ElectronicAmount"] = $scope.PaymentsLAST.ElectronicAmount;
            $scope.finalDataMasterLAST["CashAmount"] = $scope.PaymentsLAST.CashAmount;
            $scope.finalDataMasterLAST.TotalAmount = $scope.TotalAmount;
            $scope.finalDataMasterLAST.TaxAmount = $scope.TaxAmount;
            $scope.finalDataMasterLAST.GrossAmount = $scope.GrossAmount;
            $scope.finalDataMasterLAST.DiscountAmount = $scope.DiscountAmount;

            var url = serviceBase + 'api/OrderDispatchedMasterFinal';
            $http.post(url, $scope.finalDataMasterLAST)
                .success(function (data) {

                    $scope.FdispatchedMasterIDLAST = data.FinalOrderDispatchedMasterId;
                    $scope.FdispatchedMasterORDERIDLAST = data.OrderId;
                    alert('payment insert successfully');
                    // location.reload();
                    //   window.location = "#/orderMaster";
                    $scope.dispatchedDetailFinalLAST();
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.FinalOrderDispatchedMasterId == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }

        //$scope.dispatchedDetailFinalLAST = function () {


        //    var url = serviceBase + 'api/OrderDispatchedDetailsFinal?oID=' + $scope.FdispatchedMasterORDERIDLAST + '&fID=' + $scope.FdispatchedMasterIDLAST;
        //    $http.put(url)
        //        .success(function (data) {

        //            alert('insert successfully');
        //            // location.reload();
        //            window.location = "#/orderMaster";
        //            console.log("Error Gor Here");
        //            console.log(data);
        //            if (data.id == 0) {

        //                $scope.gotErrors = true;
        //                if (data[0].exception == "Already") {
        //                    console.log("Got This User Already Exist");
        //                    $scope.AlreadyExist = true;
        //                }
        //            }
        //            else {
        //            }
        //        })
        //        .error(function (data) {
        //            console.log("Error Got Heere is ");
        //            console.log(data);

        //        })
        //}
        $scope.Addfreeitem = function (item) {

            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "addFreeItem.html",
                controller: "ModalInstanceCtrlFreeItems", resolve: { order: function () { return item } }
            }), modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.moq = 0;

        $scope.OrderQuantityDic = function (moq) {

            if (moq.Noqty >= moq.MinOrderQty) {
                moq.Noqty = moq.Noqty - moq.MinOrderQty;
                alert("hhh");
            } else {

            }
        }

        $scope.OrderQuantityInc = function (moq) {
            moq.Noqty = moq.Noqty + moq.MinOrderQty;
            alert("hhh");
        }

        $scope.edit = function (item) {

            //if ($scope.orderDetails.length == 1)
            //{
            //  alert(" You can't cut the line item from single line item order."); return false;
            //}


            console.log("Edit Dialog called survey");
            var modalInstance;
            var itemreturn = {
                item: item,
                orderDetails: $scope.orderDetails
            };
            modalInstance = $modal.open(
                {
                    templateUrl: "ModalInstanceCtrlQuantityedit.html",
                    controller: "ModalInstanceCtrlQuantityedit", resolve: { inventory: function () { return itemreturn } }
                }), modalInstance.result.then(function (selectedItem) {

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

        $window.setInterval(function () {
            $scope.Chekstock();
        }, 2000);

        var first = true;//double dispatched check
        $scope.Chekstock = function () {

            for (var i = 0; i < $scope.orderDetails.length; i++) {
                $scope.CheckStockWithNumber = {};
                var first = true;
                for (var j = i; j < $scope.orderDetails.length; j++) {
                    if (first) {
                        $scope.CheckStockWithNumber = $scope.orderDetails[j];
                        first = false;
                    }
                    //else if ($scope.CheckStockWithNumber.itemNumber == $scope.orderDetails[j].itemNumber) { before multi mrp
                    //else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId) {
                    else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId && $scope.CheckStockWithNumber.IsFreeItem == $scope.orderDetails[j].IsFreeItem) {
                        var Stockcount = 0;
                        Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;
                        // if (Stockcount > data.CurrentStock && Stockcount > 0)
                        if (Stockcount > $scope.orderDetails[j].CurrentStock && Stockcount > 0) {
                            // alert("your stock not sufficient please purchase or remove item:" + $scope.orderDetails[j].itemname + " This much Qty required " + Stockcount);

                            $scope.$apply(function () {
                                //ss_col = $scope.orderDetails[j].itemNumber; before multi mrpid
                                ss_col = $scope.orderDetails[j].ItemMultiMRPId;
                                $scope.set_color($scope.orderDetails[j]);
                            });
                        }
                        else { ss_col = ''; }
                    }
                }
            }
        }
        $scope.getWarehousedetail = function () {

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.OrderData.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.FromWarehouseDetail = response;

            });
        };
        $scope.getWarehousedetail();
        // add by raj
        //Eway Bill Excel Function

        //$scope.ExcelEWayBill = function () {
        //    var Eway = OrderMasterService.getEwayBill();
        //    $scope.Ewaydata = Eway;
        //    
        //    $scope.OrderInvoiceDataDetails = [];

        //    $scope.OrderData;
        //    $scope.orderDetails = d.orderDetails;
        //    for (var i = 0; i < $scope.orderDetails.length; i++) {
        //        $scope.OrderInvoiceData = {};
        //        //company information
        //        $scope.OrderInvoiceData.SupplyType = "Outwards";
        //        $scope.OrderInvoiceData.SubType = "Supply";
        //        $scope.OrderInvoiceData.DocType = "Tax Invoice";
        //        $scope.OrderInvoiceData.DocNo = $scope.OrderData.invoice_no;
        //        $scope.OrderInvoiceData.DocDate = $filter('date')($scope.OrderData.CreatedDate, "dd-MM-yyyy");
        //        //$scope.OrderInvoiceData.DocDate = $scope.OrderData.CreatedDate;
        //        $scope.OrderInvoiceData.From_OtherPartyName = $scope.FromWarehouseDetail.CompanyName;
        //        $scope.OrderInvoiceData.From_GSTIN = $scope.FromWarehouseDetail.GSTin;
        //        $scope.OrderInvoiceData.From_Address1 = $scope.FromWarehouseDetail.CompanyName;
        //        $scope.OrderInvoiceData.From_Address2 = $scope.FromWarehouseDetail.CompanyName;
        //        $scope.OrderInvoiceData.From_Place = $scope.FromWarehouseDetail.CityName;
        //        $scope.OrderInvoiceData.From_PinCode = $scope.Ewaydata.ShipFromPinCode;
        //        $scope.OrderInvoiceData.From_State = $scope.FromWarehouseDetail.StateName;
        //        $scope.OrderInvoiceData.DispatchState = $scope.FromWarehouseDetail.StateName;
        //        //customer details
        //        $scope.OrderInvoiceData.To_OtherPartyName = $scope.OrderData.ShopName;
        //        $scope.OrderInvoiceData.To_GSTIN = $scope.OrderData.Tin_No;
        //        $scope.OrderInvoiceData.To_Address1 = $scope.OrderData.BillingAddress;
        //        $scope.OrderInvoiceData.To_Address2 = $scope.OrderData.BillingAddress;
        //        $scope.OrderInvoiceData.To_Place = $scope.FromWarehouseDetail.CityName;
        //        $scope.OrderInvoiceData.To_PinCode = $scope.Ewaydata.ShipToPinCode;
        //        $scope.OrderInvoiceData.To_State = $scope.FromWarehouseDetail.StateName;
        //        $scope.OrderInvoiceData.ShipToState = $scope.FromWarehouseDetail.StateName;
        //        //Item Information 
        //        $scope.OrderInvoiceData.Product = $scope.orderDetails[i].itemname;
        //        $scope.OrderInvoiceData.Description = $scope.orderDetails[i].itemname;
        //        $scope.OrderInvoiceData.HSN = $scope.orderDetails[i].HSNCode;
        //        $scope.OrderInvoiceData.Unit = "";
        //        $scope.OrderInvoiceData.Qty = $scope.orderDetails[i].qty;
        //        $scope.OrderInvoiceData.AssessableValue = $scope.orderDetails[i].UnitPrice;
        //        $scope.OrderInvoiceData.TaxRate = 0;
        //        $scope.OrderInvoiceData.CGSTAmount = $scope.orderDetails[i].CGSTTaxAmmount;
        //        $scope.OrderInvoiceData.SGSTAmount = $scope.orderDetails[i].SGSTTaxAmmount;
        //        $scope.OrderInvoiceData.IGSTAmount = 0;
        //        $scope.OrderInvoiceData.CESSAmount = 0;
        //        $scope.OrderInvoiceData.CESSNonAdvolAmount = 0;
        //        $scope.OrderInvoiceData.Others = 0;
        //        $scope.OrderInvoiceData.TotalInvoiceValue = $scope.orderDetails[i].TotalAmt;
        //        //transfer details
        //        $scope.OrderInvoiceData.TransMode = "Road";
        //        $scope.OrderInvoiceData.Distance = $scope.Ewaydata.Distance;
        //        $scope.OrderInvoiceData.TransName = "";
        //        $scope.OrderInvoiceData.TransID = 0;
        //        $scope.OrderInvoiceData.TransDocNo = $scope.Ewaydata.Docketno;
        //        $scope.OrderInvoiceData.TransDate = $filter('date')($scope.OrderData.CreatedDate, "dd-MM-yyyy");
        //        //$scope.OrderInvoiceData.TransDate = $scope.OrderData.CreatedDate;
        //        $scope.OrderInvoiceData.VehicleNo = $scope.Ewaydata.Vehicleno;
        //        $scope.OrderInvoiceData.VehicleType = "Regular";
        //        $scope.OrderInvoiceData.TRANSACTIONTYPE = "Regular";

        //        $scope.OrderInvoiceDataDetails.push($scope.OrderInvoiceData);
        //    };
        //    alasql('SELECT SupplyType,SubType,DocType,DocNo,DocDate,TRANSACTIONTYPE,From_OtherPartyName, From_GSTIN,From_Address1,From_Address2,From_Place,From_PinCode,From_State,DispatchState,To_OtherPartyName,To_GSTIN,To_Address1,To_Address2,To_Place,To_PinCode,To_State,ShipToState,Product,Description,HSN,Unit,Qty,AssessableValue,TaxRate,CGSTAmount,SGSTAmount,IGSTAmount,CESSAmount,CESSNonAdvolAmount,Others,TotalInvoiceValue,TransMode,Distance,TransName,TransID,TransDocNo,TransDate,VehicleNo,VehicleType INTO XLSX("OrderEWayBill' + $scope.OrderData.OrderId + '.xlsx", { headers: true }) FROM ? ', [$scope.OrderInvoiceDataDetails]);
        //};
        $scope.ExportInvoicedata = function (item) {

            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "EWayBillInFromation.html",
                controller: "ModalInstanceCtrlFreeItems", resolve: { orderDetail: function () { return d.orderDetails }, order: function () { return $scope.OrderData } }
            }), modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlQuantityedit', ModalInstanceCtrlQuantityedit);

    ModalInstanceCtrlQuantityedit.$inject = ["$scope", '$http', "$modalInstance", "inventory"];

    function ModalInstanceCtrlQuantityedit($scope, $http, $modalInstance, inventory) {
        console.log("ModalInstanceCtrlQuantityedit modal opened");


        //$scope.OrderDetails = {};
        $scope.xy = true;
        $scope.OdItem = inventory;
        $scope.inventoryData = {};
        if (inventory) {
            console.log("category if conditon");
            $scope.Quantity = inventory.item;
            $scope.orderDetails = inventory.orderDetails;
        }
        $scope.updatelineitem = function (data) {

            if (data.MinOrderQty <= data.qty) {
                if (data.qty >= 0) {
                    if ($scope.Quantity.qty <= 0) {

                        alert("Quantiy should not be negative");
                        $scope.Quantity.qty = 0;
                    } else {
                        if (data.MinOrderQty > 0) {
                            $scope.Quantity.qty = data.qty - data.MinOrderQty;
                        } else {
                            $scope.Quantity.qty = data.qty - 1;
                        }
                    }
                }
            }
            else {
                alert(" MinOrderQty is more than ordered qty ");
            }
        }
        $scope.updatelineitem1 = function (data) {
            debugger;
            if (data.MinOrderQty == 0) {
                $scope.Quantity.qty = data.qty + 1;
            }
            else if (data.qty != data.Noqty) {
                $scope.Quantity.qty = data.qty + data.MinOrderQty;
            }
            else {
                alert("Quantiy should not be greator then Max Quantiy")
            }
        }
        $scope.ok = function () {


            if ($scope.Quantity.QtyChangeReason == undefined) {
                alert("Select reason");
            } else {
                var url = serviceBase + 'api/OrderDispatchedMaster/GetFreebiesItem?OrderId=' + $scope.Quantity.OrderId + '&ParentItemId=' + $scope.Quantity.ItemId + '&WarehouseId=' + $scope.Quantity.WarehouseId;
                $http.get(url).success(function (results) {
                    var freebiesdata = results;
                    if (freebiesdata) {
                        angular.forEach($scope.orderDetails, function (value, key) {

                            if (freebiesdata.FreeItemId == 0) {
                                if (value.qty <= value.CurrentStock) {
                                    value.IsRed = false;

                                }
                            }

                            if (value.ItemId == freebiesdata.FreeItemId && value.IsFreeItem == true && value.FreeWithParentItemId == $scope.Quantity.ItemId) {

                                var multiply = $scope.Quantity.qty / freebiesdata.MinOrderQuantity;
                                var totalquantity = parseInt(multiply) * freebiesdata.NoOffreeQuantity;
                                value.qty = totalquantity;
                                value.Noqty = totalquantity;

                            };

                        });
                    }
                    else {
                        debugger;
                        angular.forEach($scope.orderDetails, function (value, key) {

                            if (value.IsFreeItem == false && value.FreeWithParentItemId != $scope.Quantity.ItemId) {

                                if (value.qty <= value.CurrentStock) {
                                    value.IsRed = false;

                                }
                            };
                        });
                    }
                });






                $modalInstance.close();
            }

        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Putinventory = function (data) {

            $scope.orderDetail.qty = data.qty;
            $scope.orderDetail.QtyChangeReason = data.OrderDetailsId;


            //$scope.changereson = data.QtyChangeReason;
            //$scope.Orderdetailid = data.OrderDetailsId;
            //$scope.qty = data.qty;
            //if ($scope.changereson != null) {
            //    var url = serviceBase + "api/OrderDetails/Changeqty";
            //    var dataToPost = {
            //        OrderDetailsId:  $scope.Orderdetailid,
            //        qty:  $scope.qty,                
            //        QtyChangeReason: $scope.changereson
            //    };
            //    console.log(dataToPost);
            //    $http.put(url, dataToPost)
            //    .success(function (data) {
            //        $modalInstance.close(data);
            //    })
            //     .error(function (data) {
            //         console.log(data);
            //     })
            //}
            //else {
            //    alert('please enter reason for change Qty');
            //}
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOrderInvoice1', ModalInstanceCtrlOrderInvoice1);

    ModalInstanceCtrlOrderInvoice1.$inject = ["$scope", '$http', 'OrderMasterService', 'WarehouseService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrlOrderInvoice1($scope, $http, OrderMasterService, WarehouseService, $modalInstance, ngAuthSettings, order) {

        var User = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.isShoWOffer = false;
        $scope.Wdata = function () {
            $scope.warehou = {};
            $http.get(serviceBase + 'api/Warehouse').then(function (results) {
                $scope.warehou = results.data;
            })
        }
        $scope.Wdata();
        $scope.OrderDetails = {};
        $scope.OrderData = {};
        var d = OrderMasterService.getDeatil();
        $scope.OrderData = d;

        var info = OrderMasterService.getDeatilinfo();

        $scope.OrderData1 = info;
        $scope.HideCessColumn = false;
        //function to remove zero cess  item column from invoice print
        angular.forEach($scope.OrderData, function (item) {
            if (item.CessTaxAmount > 0) {
                $scope.HideCessColumn = true;
            }
        });

        $scope.wid = $scope.OrderData1.WarehouseId;
        console.log("order invoice modal opened");
        $scope.getWarehousedetail = function () {

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.wid;
            $http.get(url).success(function (response) {
                $scope.FromWarehouseDetail = response;
            });
        };
        $scope.getWarehousedetail();
        $http.get(serviceBase + "api/offer/GetOfferItem?oderid=" + $scope.OrderData1.OrderId).then(function (results) {
            $scope.Offeritem = results.data;
            try {
                if (results.data.length > 0) {
                    $scope.isShoWOffer = true;
                }
            } catch (ex) {
            }
        }, function (error) {
        });


        $scope.offerbill = function () {
            var url = serviceBase + "api/offer/GetOfferBill?oderid=" + $scope.OrderData1.OrderId;
            $http.get(url).success(function (results) {
                $scope.InvoiceOrderOffer = results;
            });
        };
        $scope.offerbill();

        $scope.SumDataHSNDetails = function () {
            var url = serviceBase + "api/OrderMaster/getSuminvoiceHSNCodeData?OrderId=" + $scope.OrderData1.OrderId;
            $http.get(url).success(function (results) {
                $scope.SumDataHSN = results;
            });
        };
        $scope.SumDataHSNDetails();

        $scope.getTotalTax = function (data) {
            var totaltax = 0;
            data.forEach(x => {

                //totaltax = totaltax + x.AmtWithoutTaxDisc;
                totaltax = totaltax + (x.TaxAmmount + x.CessTaxAmount);
            });
            return totaltax;
        }

        $scope.getTotalTaxableValue = function (data) {
            var totalTaxableValue = 0;
            data.forEach(x => {
                totalTaxableValue = totalTaxableValue + x.AmtWithoutTaxDisc;

            });
            return totalTaxableValue;
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

        if (info.Status == 'Pending')
            $scope.OrderData1.OrderedDate = info.CreatedDate;
        $scope.Itemcount = 0;

        for (var i = 0; i < $scope.OrderData.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.OrderData[i].qty;
        }

        $scope.totalfilterprice = 0;
        _.map($scope.OrderData, function (obj) {
            $scope.Wdata();
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
        })

        if ($scope.OrderData1.Status == 'Pending' || $scope.OrderData1.Status == 'Process' || $scope.OrderData1.Status == 'Cancel') {
            setTimeout(function () {
                $(".taxtable").remove();
            }, 500)
        }

        $scope.paymentdetail = [];

        $scope.GetPayment = function () {
            // 

            var url = serviceBase + 'api/OrderMastersAPI/Getpaymentstatus?OrderId=' + $scope.OrderData1.OrderId;
            $http.get(url).success(function (response) {
                //debugger;
                $scope.paymentdetail = response;
                for (var j = 0; j < $scope.paymentdetail.length; j++) {
                    if ($scope.paymentdetail[j].amount < 0) {
                        $scope.paymentdetail[j].refundAmount = Math.abs($scope.paymentdetail[j].amount);
                    }
                }

            });
        };
        $scope.GetPayment();

        $scope.CustomerCriticalInfo = "";
        $scope.CheckCustomerCriticalInfo = function () {
            if ($scope.OrderData1 && $scope.OrderData1.CustomerId) {
                $http.get(serviceBase + "api/Customers/CheckCustomerCriticalInfo?customerId=" + $scope.OrderData1.CustomerId).then(function (results) {
                    $scope.CustomerCriticalInfo = results.data;
                }, function (error) {
                });
            }

        };

        $scope.CheckCustomerCriticalInfo();
        $scope.CustomerCount = {};
        $scope.GetOrderCountInfo = function () {

            if ($scope.OrderData1 && $scope.OrderData1.CustomerId) {
                $http.get(serviceBase + "api/InActiveCustOrderMaster/GetOrderCount?OrderId=" + $scope.OrderData1.OrderId).then(function (results) {
                    $scope.CustomerCount = results.data;
                }, function (error) {
                });
            }

        };

        $scope.GetOrderCountInfo();
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlTraedeInvoiceDispatch', ModalInstanceCtrlOrderInvoiceDispatch);

    ModalInstanceCtrlOrderInvoiceDispatch.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', '$compile', 'order'];

    function ModalInstanceCtrlOrderInvoiceDispatch($scope, $http, $modalInstance, ngAuthSettings, $compile, order) {
        debugger;
        $scope.Data = order;
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOrderInvoiceDispatch', ModalInstanceCtrlOrderInvoiceDispatch);

    ModalInstanceCtrlOrderInvoiceDispatch.$inject = ["$scope", '$http', 'OrderMasterService', 'WarehouseService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrlOrderInvoiceDispatch($scope, $http, OrderMasterService, WarehouseService, $modalInstance, ngAuthSettings, order) {
        console.log("order invoice modal opened");

        debugger;
        var User = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.Wdata = function () {
            $scope.warehou = {};
            $http.get(serviceBase + 'api/Warehouse').then(function (results) {

                $scope.warehou = results.data;

            })
        }
        $scope.Wdata();

        $scope.OrderData = {};
        $scope.isShoW = false;
        var d = OrderMasterService.getDeatil();

        $scope.OrderData1 = d;
        $scope.wid = $scope.OrderData1.WarehouseId;
        $scope.FromWarehouseDetail = [];
        $scope.getWarehousedetail = function () { // This would fetch the data on page change.

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.wid;
            $http.get(url).success(function (response) {

                $scope.FromWarehouseDetail = response; //ajax request to fetch data into vm.data
                console.log("get current Page items:");
            });
        };
        $scope.getWarehousedetail();
        $http.get(serviceBase + "api/offer/GetOfferItem?oderid=" + $scope.OrderData1.OrderId).then(function (results) {
            $scope.Offeritem = results.data;
            try {
                if (results.data.length > 0) {
                    $scope.isShoWOffer = true;
                }
            } catch (ex) { }
        }, function (error) {
        });

        var info = OrderMasterService.getDispatchMaster();
        debugger;
        $scope.OrderData = info;
        $scope.Itemcount = 0;
        $scope.offerbill = function () { // This would fetch the data on page change.           
            var url = serviceBase + "api/offer/GetOfferBill?oderid=" + $scope.OrderData1.OrderId;
            $http.get(url).success(function (results) {
                $scope.InvoiceOrderOffer = results //ajax request to fetch data into vm.data                
            });
        };

        $scope.offerbill();
        $scope.SumDataHSNDetails = function () {
            var url = serviceBase + "api/OrderMaster/RTDgetSuminvoiceHSNCodeData?OrderId=" + $scope.OrderData1.OrderId;
            $http.get(url).success(function (results) {
                $scope.SumDataHSN = results;
            });
        };
        $scope.SumDataHSNDetails();
        $scope.getTotalTax = function (data) {
            var totaltax = 0;
            data.forEach(x => {

                //totaltax = totaltax + x.AmtWithoutTaxDisc;
                totaltax = totaltax + (x.TaxAmmount + x.CessTaxAmount);
            });
            return totaltax;
        }
        $scope.getTotalTaxableValue = function (data) {
            var totalTaxableValue = 0;
            data.forEach(x => {
                totalTaxableValue = totalTaxableValue + x.AmtWithoutTaxDisc;

            });
            return totalTaxableValue;
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


        $scope.HideCessColumn = false;
        //function to remove zero cess  item column from invoice print
        angular.forEach($scope.OrderData, function (item) {
            if (item.CessTaxAmount > 0) {
                $scope.HideCessColumn = true;
            }
        });

        //function to remove zero qty item from invoice print
        $scope.RemoveZeroQtyItemInvoice = function (prop, val) {
            return function (item) {
                if (item[prop] > val) return true;
            }
        }

        var data = _.map(_.groupBy($scope.OrderData, function (cat) {
            return cat.TaxPercentage;
        }), function (grouped) {
            return grouped;
        });

        $scope.data1 = [];
        for (var j = 0; j < data.length; j++) {

            var tl = data[j];
            $scope.tts = { TaxPercentage: '', itmQty1: '', TaxAmt1: '', HSNCode: '', }
            var TaxPercentage = tl[0].TaxPercentage;
            //var SGSTTaxPercentage = tl[0].SGSTTaxPercentage;
            //var IGSTTaxPercentage = tl[0].IGSTTaxPercentage;
            //var HSNCode = tl[0].HSNCode;

            var ttlTax = 0;
            var tx = 0;
            var tx1 = 0;
            var tx2 = 0;
            var qt = 0;

            for (var i = 0; i < tl.length; i++) {
                tx += tl[i].TaxAmmount;
                //tx1 += tl[i].SGSTTaxAmmount;
                //tx2 += tl[i].IGSTTaxAmmount;
                qt += tl[i].qty;
            }
            var itmQty1 = qt;
            var TaxAmt1 = tx;
            //var SGSTTaxAmt = tx1;
            //var IGSTTaxAmt = tx2;

            $scope.tts.TaxPercentage = TaxPercentage;

            //$scope.tts.SGSTTaxPercentage = SGSTTaxPercentage;
            //$scope.tts.IGSTTaxPercentage = IGSTTaxPercentage;
            //$scope.tts.HSNCode = HSNCode;
            $scope.tts.itmQty1 = itmQty1;
            $scope.tts.TaxAmt1 = TaxAmt1;
            //$scope.tts.SGSTTaxAmt = SGSTTaxAmt;
            //$scope.tts.IGSTTaxAmt = IGSTTaxAmt;


            $scope.data1.push($scope.tts);
            console.log($scope.data1);
        }
        debugger;
        for (var i = 0; i < $scope.OrderData.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.OrderData[i].qty;
        }

        $scope.totalfilterprice = 0;
        _.map($scope.OrderData, function (obj) {
            $scope.Wdata();
            console.log("count total");
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log(obj.TotalAmt);
            console.log($scope.totalfilterprice);
        });

        $http.get(serviceBase + "api/freeitem/SkFree?oderid=" + $scope.OrderData1.OrderId).then(function (results) {



            $scope.freeitem = results.data;
            try {
                if (results.data.length > 0) {
                    $scope.isShoW = true;
                }
            } catch (ex) { }
        }, function (error) {
        });

        $scope.paymentdetail = [];

        $scope.GetPayment = function () {


            var url = serviceBase + 'api/OrderMastersAPI/Getpaymentstatus?OrderId=' + $scope.OrderData1.OrderId;
            $http.get(url).success(function (response) {
                debugger;
                $scope.paymentdetail = response;
                for (var j = 0; j < $scope.paymentdetail.length; j++) {
                    if ($scope.paymentdetail[j].amount < 0) {
                        $scope.paymentdetail[j].refundAmount = Math.abs($scope.paymentdetail[j].amount);
                    }
                }
                console.log(' $scope.paymentdetail: ', $scope.paymentdetail);

            });
        };
        $scope.GetPayment();

        $scope.CustomerCriticalInfo = "";
        $scope.CheckCustomerCriticalInfo = function () {

            if ($scope.OrderData1 && $scope.OrderData1.CustomerId) {
                $http.get(serviceBase + "api/Customers/CheckCustomerCriticalInfo?customerId=" + $scope.OrderData1.CustomerId).then(function (results) {
                    $scope.CustomerCriticalInfo = results.data;
                }, function (error) {
                });
            }

        };
        $scope.CheckCustomerCriticalInfo();
        $scope.CustomerCount = {};
        $scope.GetOrderCountInfo = function () {

            if ($scope.OrderData1 && $scope.OrderData1.OrderId) {
                $http.get(serviceBase + "api/InActiveCustOrderMaster/GetOrderCount?OrderId=" + $scope.OrderData1.OrderId).then(function (results) {

                    $scope.CustomerCount = results.data;
                }, function (error) {
                });
            }

        };

        $scope.GetOrderCountInfo();

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlFreeItems', ModalInstanceCtrlFreeItems);

    ModalInstanceCtrlFreeItems.$inject = ["$scope", '$http', '$filter', "$modalInstance", 'ngAuthSettings', 'order', 'itemMasterService', "OrderMasterService", 'orderDetail'];

    function ModalInstanceCtrlFreeItems($scope, $http, $filter, $modalInstance, ngAuthSettings, order, itemMasterService, OrderMasterService, orderDetail) {
        $scope.Ewaydata = {};
        $scope.OrderData = {};
        $scope.FreeitemMaster = [];
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () {
                $modalInstance.dismiss('canceled');
                console.log(hjkl);
            };

        if (order) {

            $scope.Temporder = order;
        }

        //Get Frees Stoklist
        var url = serviceBase + "api/freestocks/GetWarehouseFreeStock?WarehouseId=" + $scope.Temporder.WarehouseId;
        $http.get(url).then(function (results) {

            if (results.data.length > 0) {
                $scope.FreeitemMaster = results.data;

            }
            else {
                alert("No Free Stock Available");
                $modalInstance.close(data);
            }

        }, function (error) {
        });

        //Calculate
        $scope.TempFree = 0;
        $scope.getFreestock = function (data) {

            $scope.ItemObj = JSON.parse(data);
            if ($scope.ItemObj) {
                $scope.TempFree = $scope.ItemObj;

            } else { alert("Some thing Went wrong"); }

        }

        //Add for available freestock quantity
        $scope.SaveFreeitem = function (data) {

            if (data && data.Quantity > 0) {
                var url = serviceBase + "api/freestocks/addFreeItemOrder";
                var dataToPost = {
                    OrderId: $scope.Temporder.OrderId,
                    FreeStockId: $scope.TempFree.FreeStockId,
                    Quantity: data.Quantity
                };
                $http.post(url, dataToPost)
                    .success(function (data) {

                        if (!data) {
                            alert(data);
                            $modalInstance.close();

                        }
                        else {
                            alert("SomeThing Went wrong");
                            $modalInstance.close();
                        }
                    })
                    .error(function (error) {
                        alert("SomeThing Went wrong");
                        $modalInstance.close();
                    })
            }
        };

        // **********************
        $scope.getWarehousedetail = function () {

            var url = serviceBase + "api/Warehouse" + "?id=" + $scope.Temporder.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.FromWarehouseDetail = response;

            });
        };
        $scope.getWarehousedetail();
        $scope.Ewaydata = {
            ShipFromPinCode: 0,
            ShipToPinCode: 0,
            Distance: 0,
            GSTno: null,
            Vehicleno: null,
            Docketno: 0
        };
        $scope.SaveEwaydata = function (Ewaydata) {

            if (Ewaydata.ShipFromPinCode <= 0) {
                alert("Please Enter Ship From PinCode");
                return;
            } else
                if (Ewaydata.ShipToPinCode <= 0) {
                    alert("Please Enter Ship To PinCode");
                    return;
                } else
                    if (Ewaydata.Distance <= 0) {
                        alert("Please Enter Distance");
                        return;
                    }
                    else
                        if (Ewaydata.Vehicleno == null) {
                            alert("Please Enter Vehicle no.");
                            return;
                        }
                        else
                            if (Ewaydata.Docketno <= 0) {
                                alert("Please Enter Docketno");
                                return;
                            }
            $scope.DataEwayBill = Ewaydata;
            alert("Data Insert successfully");
            $scope.ExcelEWayBill();
            $modalInstance.close();
        };

        $scope.ExcelEWayBill = function () {
            var Eway = $scope.DataEwayBill;
            $scope.Ewaydata = Eway;
            $scope.OrderInvoiceDataDetails = [];
            debugger;
            $scope.OrderData = $scope.Temporder;

            $scope.orderDetails = orderDetail;
            for (var i = 0; i < $scope.orderDetails.length; i++) {
                $scope.OrderInvoiceData = {};
                //company information
                $scope.OrderInvoiceData.SupplyType = "Outwards";
                $scope.OrderInvoiceData.SubType = "Supply";
                $scope.OrderInvoiceData.DocType = "Tax Invoice";
                $scope.OrderInvoiceData.DocNo = $scope.OrderData.invoice_no;
                $scope.OrderInvoiceData.DocDate = $filter('date')($scope.OrderData.CreatedDate, "dd/MM/yyyy");
                //$scope.OrderInvoiceData.DocDate = $scope.OrderData.CreatedDate;
                $scope.OrderInvoiceData.From_OtherPartyName = $scope.FromWarehouseDetail.CompanyName;
                $scope.OrderInvoiceData.From_GSTIN = $scope.FromWarehouseDetail.GSTin;
                $scope.OrderInvoiceData.From_Address1 = $scope.FromWarehouseDetail.CompanyName;
                $scope.OrderInvoiceData.From_Address2 = $scope.FromWarehouseDetail.CompanyName;
                $scope.OrderInvoiceData.From_Place = $scope.FromWarehouseDetail.CityName;
                $scope.OrderInvoiceData.From_PinCode = $scope.Ewaydata.ShipFromPinCode;
                $scope.OrderInvoiceData.From_State = $scope.FromWarehouseDetail.StateName;
                $scope.OrderInvoiceData.DispatchState = $scope.FromWarehouseDetail.StateName;
                //customer details
                $scope.OrderInvoiceData.To_OtherPartyName = $scope.OrderData.ShopName;
                $scope.OrderInvoiceData.To_GSTIN = $scope.OrderData.Tin_No;
                $scope.OrderInvoiceData.To_Address1 = $scope.OrderData.BillingAddress;
                $scope.OrderInvoiceData.To_Address2 = $scope.OrderData.BillingAddress;
                $scope.OrderInvoiceData.To_Place = $scope.FromWarehouseDetail.CityName;
                $scope.OrderInvoiceData.To_PinCode = $scope.Ewaydata.ShipToPinCode;
                $scope.OrderInvoiceData.To_State = $scope.FromWarehouseDetail.StateName;
                $scope.OrderInvoiceData.ShipToState = $scope.FromWarehouseDetail.StateName;
                //Item Information 
                $scope.OrderInvoiceData.Product = $scope.orderDetails[i].itemname;
                $scope.OrderInvoiceData.Description = $scope.orderDetails[i].itemname;
                $scope.OrderInvoiceData.HSN = $scope.orderDetails[i].HSNCode;
                $scope.OrderInvoiceData.Unit = "";
                $scope.OrderInvoiceData.Qty = $scope.orderDetails[i].qty;
                $scope.OrderInvoiceData.DiscountPercentage = $scope.orderDetails[i].DiscountPercentage;
                $scope.OrderInvoiceData.DiscountAmmount = $scope.orderDetails[i].DiscountAmmount;
                $scope.OrderInvoiceData.AssessableValue = $scope.orderDetails[i].UnitPrice;
                $scope.OrderInvoiceData.TaxRate = 0;
                $scope.OrderInvoiceData.CGSTAmount = $scope.orderDetails[i].CGSTTaxAmmount;
                $scope.OrderInvoiceData.SGSTAmount = $scope.orderDetails[i].SGSTTaxAmmount;
                $scope.OrderInvoiceData.IGSTAmount = 0;
                $scope.OrderInvoiceData.CESSAmount = 0;
                $scope.OrderInvoiceData.CESSNonAdvolAmount = 0;
                $scope.OrderInvoiceData.Others = 0;
                $scope.OrderInvoiceData.TotalInvoiceValue = $scope.orderDetails[i].TotalAmt;
                //transfer details
                $scope.OrderInvoiceData.TransMode = "Road";
                $scope.OrderInvoiceData.Distance = $scope.Ewaydata.Distance;
                $scope.OrderInvoiceData.TransName = "";
                $scope.OrderInvoiceData.TransID = 0;
                $scope.OrderInvoiceData.TransDocNo = $scope.Ewaydata.Docketno;
                $scope.OrderInvoiceData.TransDate = $filter('date')($scope.OrderData.CreatedDate, "dd/MM/yyyy");
                //$scope.OrderInvoiceData.TransDate = $scope.OrderData.CreatedDate;
                $scope.OrderInvoiceData.VehicleNo = $scope.Ewaydata.Vehicleno;
                $scope.OrderInvoiceData.VehicleType = "Regular";
                $scope.OrderInvoiceData.TRANSACTIONTYPE = "Regular";

                $scope.OrderInvoiceDataDetails.push($scope.OrderInvoiceData);
            };
            alasql('SELECT SupplyType,SubType,DocType,DocNo,DocDate,TRANSACTIONTYPE,From_OtherPartyName, From_GSTIN,From_Address1,From_Address2,From_Place,From_PinCode,From_State,DispatchState,To_OtherPartyName,To_GSTIN,To_Address1,To_Address2,To_Place,To_PinCode,To_State,ShipToState,Product,Description,HSN,Unit,Qty,AssessableValue,TaxRate,CGSTAmount,SGSTAmount,IGSTAmount,CESSAmount,CESSNonAdvolAmount,Others,TotalInvoiceValue,TransMode,Distance,TransName,TransID,TransDocNo,TransDate,VehicleNo,VehicleType INTO XLSX("OrderEWayBill' + $scope.OrderData.OrderId + '.xlsx", { headers: true }) FROM ? ', [$scope.OrderInvoiceDataDetails]);
        };
    }
})();
(function () {

    'use strict';

    angular
        .module('app')
        .controller('ModalDeliveryCancelledDraftCtrl', ModalDeliveryCancelledDraftCtrl);

    ModalDeliveryCancelledDraftCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "Image", 'FileUploader'];

    function ModalDeliveryCancelledDraftCtrl($scope, $http, ngAuthSettings, $modalInstance, Image, FileUploader) {


        if (Image) {
            $scope.data = Image;
        };

        var url = serviceBase + 'api/DeliveryCancelledDraft/GetOrderImage?OrderId=' + $scope.data;
        $http.get(url).then(function (results) {

            $scope.DeliveryCancelledDraft = results.data;
        });
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();
