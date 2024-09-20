

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DamageorderdetailsController', DamageorderdetailsController);

    DamageorderdetailsController.$inject = ['$scope', 'DamageOrderMasterService', 'DamageOrderDetailsService', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "peoplesService", '$modal', 'BillPramotionService', "$modalInstance"];

    function DamageorderdetailsController($scope, DamageOrderMasterService, DamageOrderDetailsService, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService, $modal, BillPramotionService, $modalInstance) {
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        
        console.log("orderdetailsController start loading OrderDetailsService");
        $scope.Payments = {};
        $scope.currentPageStores = {};
        $scope.isShoW = false;
        $scope.finaltable = false;
        $scope.dispatchtable = false;
        $scope.orderDetails = {};
        $scope.OrderData = {};
        $scope.OrderData1 = {};
        $scope.DamageStock = [];       
        var d = DamageOrderMasterService.getDeatil();
        console.log(d);
        $scope.signdata = {};
        $scope.viewsign = function () {
            var Signimg = $scope.signdata.Signimg;
            console.log("$scope.signdata");
            if (Signimg != null) {
                window.open(Signimg);
            } else { alert("no sign present") }
        }
        $scope.count = 1;
        $scope.OrderData = d;
        console.log("orderdata",$scope.OrderData);
        console.log("$scope.OrderDatamk");
        console.log($scope.OrderData);
        if ($scope.OrderData.Status == "Order Canceled") { //$scope.OrderData.Status == "Cancel" ||
            $scope.finaltable = false;
            $scope.dispatchtable = false;
            //document.getElementById("btnSave").hidden = true;
        }
        if ($scope.OrderData.Status == "Ready to Dispatch") {
            //document.getElementById("tbl").hidden = true;
            //need to change
        }
        $scope.orderDetails = d.DamageorderDetails;
        $scope.orderDetailsINIT = d.DamageorderDetails;
        $scope.checkInDispatchedID = $scope.orderDetails.length > 0 ? $scope.orderDetails[0].DamageOrderId : 0;

        $scope.pramotions = {};
        $scope.selectedpramotion = {};
        BillPramotionService.getbillpramotion().then(function (results) {
            $scope.pramotions = results.data;
        }, function (error) {
        });
        $scope.damageStockGet = function () {

            var url1 = serviceBase + 'api/DamageOrderMaster/GetStock?id=' + $scope.OrderData.DamageOrderId;//instead of url used url1
            $http.get(url1).success(function (damageStock) {

                $scope.DamageStock = damageStock;
                if ($scope.DamageStock != null) {
                    angular.forEach($scope.orderDetails, function (value, key) {
                        angular.forEach($scope.DamageStock, function (valuestock, key) {
                            if (value.DamageOrderDetailsId == valuestock.DamageOrderDetailsId) {
                             
                                value.CurrentNetStock = valuestock.Damagestock;
                                value.CurrentStock = valuestock.CurrentBatchStock;
                            };
                        });

                    });
                }
            });


        };
        $scope.damageStockGet();
        //for display info in print final order
        DamageOrderMasterService.saveinfo($scope.OrderData);
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
                        if ($scope.OrderData1.ReDispatchCount > 1) {
                            $scope.ReDispatchButton = true;
                        }
                    }
                });
        };
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
        });
        //end

        // check Final master Sattled order done
        var url1 = serviceBase + 'api/OrderDispatchedMasterFinal?id=' + $scope.checkInDispatchedID;//instead of url used url1
        $http.get(url1).success(function (data1) {
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
            var dboyMob = "obj.Mobile";
            console.log(dboyMob);
            $scope.Did = $scope.orderDetailsDisp[0].OrderDispatchedMasterId;
            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.Did + '&DboyNo=' + 'obj.Mobile';
            $http.put(url)
                .success(function (data) {

                    if (data.length > 0) {

                    }
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

        var url2 = serviceBase + 'api/OrderDispatchedDetailsReturn?id=' + $scope.checkInDispatchedID; //inste3ad of url used url2
        $http.get(url2).success(function (data) {

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

        //check for final detail Sattled
        var url3 = serviceBase + 'api/OrderDispatchedDetailsFinal?id=' + $scope.checkInDispatchedID;//instead of url used url3
        $http.get(url3).success(function (data) {
            if (data.length > 0) {

                $scope.count = 0;
                $scope.orderDetailsFinal = data;
                $scope.finaltable = true;
                $scope.dispatchtable = false;
                $scope.finalAmountLAST = 0;
                $scope.finalTaxAmountLAST = 0;
                $scope.finalGrossAmountLAST = 0;
                $scope.finalTotalTaxAmountLAST = 0;
                $scope.finalLast = true;

                if ($scope.orderDetailsFinal[0].FinalOrderDispatchedMasterId == 0) {
                    for (var i = 0; i < $scope.orderDetailsFinal.length; i++) {
                        $scope.finalAmountLAST = $scope.finalAmountLAST + $scope.orderDetailsFinal[i].TotalAmt;
                        $scope.finalTaxAmountLAST = $scope.finalTaxAmountLAST + $scope.orderDetailsFinal[i].TaxAmmount;
                        $scope.finalGrossAmountLAST = $scope.finalGrossAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
                        $scope.finalTotalTaxAmountLAST = $scope.finalTotalTaxAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
                    }


                    $scope.TotalAmount = $scope.finalAmountLAST;
                    $scope.TaxAmount = $scope.finalTaxAmountLAST;
                    $scope.GrossAmount = $scope.finalGrossAmountLAST;
                    $scope.DiscountAmount = $scope.finalTotalTaxAmountLAST - $scope.finalAmountLAST;


                    $scope.myDetail = {};
                    $scope.myMaster = {};
                    $scope.myDetail = $scope.orderDetailsFinal;
                    $scope.myMaster = [];
                    var newdata = angular.copy($scope.OrderData1);
                    $scope.myMaster = newdata;

                    $scope.myMaster.TotalAmount = $scope.TotalAmount;
                    $scope.myMaster.TaxAmount = $scope.TaxAmount;
                    $scope.myMaster.GrossAmount = $scope.GrossAmount;
                    $scope.myMaster.DiscountAmount = $scope.DiscountAmount;
                    $scope.FinalinvoiceButtonLast = true;
                    $scope.showpaybleButton = true;
                    $scope.finalLastReturn = true;

                }

            }
            else {
                $scope.finalLastReturn = true;
                $scope.FinalinvoiceButtonWithoutMasterLast = true;
            }
        });

        $scope.showInvoiceWithoutMasterFinal = function () {
            OrderMasterService.saveDispatch($scope.myDetail);
            OrderMasterService.save1($scope.myMaster);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteOrderInvoice1.html",
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
        $scope.OrderCancelled = function (oid) {
            

            if (oid > 0) {
                var r = confirm("Do you want to perform this action ?");
                if (r == true) {
                    var url = serviceBase + 'api/NonRevenue/CancelOrders?Orderid=' + oid;
                    $http.get(url)
                        .success(function (data) {
                            
                            if (data.Status == true) {
                                debugger
                                alert(data.Message);
                                location.reload();
                                window.close();
                             
                            }
                            else {
                                alert(data.Message);
                            }

                            
                        })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);

                        })
                }
               
            }
            
        };


        $scope.showInvoice = function (data) {

            DamageOrderMasterService.save1(data);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteOrderInvoice1.html",
                    controller: "ModalInstanceCtrlOrderInvoice11",
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
        $scope.showInvoiceDispatch = function (Detail, master) {

            OrderMasterService.saveDispatch(Detail);
            OrderMasterService.save1(master);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteOrderInvoice1.html",
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
        };
        //get peoples for delivery boy
        $scope.GetDboy = function () {

            //$http.get(serviceBase + 'api/Peoples/Warehousebased?WarehouseId=' + $scope.OrderData.WarehouseId).then(function (results) {
            $http.get(serviceBase + 'api/Peoples/WarehouseRolebased?WarehouseId=' + $scope.OrderData.WarehouseId).then(function (results) {
               
                $scope.User = results.data;
                console.log("Got people collection");
                console.log($scope.User);
                angular.forEach($scope.User, function (value, key) {

                    if ($scope.OrderData.DboyId == value.PeopleID) {

                        $scope.DBname = value.DisplayName;
                    }
                });

            }, function (error) {

            });
        }
        $scope.GetDboy();
        // end
        for (var i = 0; i < $scope.orderDetails.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }
        _.map($scope.OrderData.orderDetails, function (obj) {
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log("$scope.OrderData");
            console.log($scope.totalfilterprice);
        })
        var s_col = false;
        var del = '';
        $scope.set_color = function (orderDetail) {

            if (orderDetail.qty > orderDetail.CurrentNetStock && orderDetail.Status == "Pending") {
                s_col = true;
                return { background: "#ff9999" }
            }
            else { s_col = false; }
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

        $scope.save = function () {

            console.log($scope.Dboy);
            var data = $scope.orderDetails;
            var flag = true;
            for (var i = 0; i <= $scope.orderDetails.length; i++) {
                data = $scope.orderDetails[i];
                if (data.qty > data.CurrentNetStock && data.Deleted == false) {
                    alert("your stock not sufficient please purchase or remove item then dispatched");
                    flag = false;
                    break;
                }
                else if ($scope.Dboy == undefined) {
                    alert("Please Select Delivery Boy");
                    flag = false;
                }
            }
            if (flag == true) {
                try {
                    var obj = ($scope.Dboy);
                } catch (err) {
                    alert("Please Select Delivery Boy");
                    console.log(err);
                }
                $scope.OrderData.DboyName = "obj.DisplayName";
                $scope.OrderData.DboyMobileNo = "obj.Mobile";
                $scope.OrderData.DboyId = "obj.PeopleID";
                $scope.OrderData();
                console.log($scope.OrderData);
                console.log("save orderdetailfunction");
                console.log("Selected Pramtion");
                console.log($scope.selectedpramotion);
                for (var g = 0; g < $scope.orderDetails.length; g++) {//instead of i used g
                    console.log($scope.orderDetails[g]);
                    console.log($scope.orderDetails[g].DiscountPercentage);
                    $scope.orderDetails[g].DiscountPercentage = $scope.selectedpramotion;
                    console.log($scope.orderDetails[g].DiscountPercentage);
                }
                var url = serviceBase + 'api/OrderDispatchedMaster';
                $http.post(url, $scope.OrderData)
                    .success(function (data) {
                        $scope.dispatchedMasterID = data.OrderDispatchedMasterId;
                        $scope.orderDetails = data.orderDetails;
                        $scope.dispatchedDetail();
                        $modalInstance.close(data);

                        console.log("Error Gor Here");
                        console.log(data);
                        if (data.id == 0) {
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
        }

        $scope.dispatchedDetail = function () {
            for (var i = 0; i < $scope.orderDetails.length; i++) {
                $scope.orderDetails[i].OrderDispatchedMasterId = $scope.dispatchedMasterID;
                if ($scope.orderDetails[i].qty > $scope.orderDetails[i].CurrentNetStock) {
                    delete $scope.orderDetails[i];
                }
            }
            $scope.orderDetails();

            var url = serviceBase + 'api/OrderDispatchedDetails';
            $http.post(url, $scope.orderDetails)
                .success(function (data) {

                    alert('insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

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
        };
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
            $scope.Payments();
            $scope.Payments.PaymentAmount = $scope.OrderData1.GrossAmount
            $scope.finalDataMaster = $scope.OrderData1;
            $scope.finalDataMaster.PaymentAmount = $scope.Payments.PaymentAmount;
            $scope.finalDataMaster.CheckNo = $scope.Payments.CheckNo;
            $scope.finalDataMaster.CheckAmount = $scope.Payments.CheckAmount;
            $scope.finalDataMaster.ElectronicPaymentNo = $scope.Payments.ElectronicPaymentNo;
            $scope.finalDataMaster.ElectronicAmount = $scope.Payments.ElectronicAmount;
            $scope.finalDataMaster.CashAmount = $scope.Payments.CashAmount;


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

            $scope.orderDetailsDisp();

            var url = serviceBase + 'api/OrderDispatchedDetailsFinal';
            $http.post(url, $scope.orderDetailsDisp)
                .success(function (data) {

                    alert('insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

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
            $scope.PaymentsLAST();


            var payamount = parseInt($scope.myMaster.GrossAmount);
            $scope.PaymentsLAST.PaymentAmount = payamount;



            $scope.finalDataMasterLAST = $scope.OrderData1;

            if ($scope.finalAmountLAST > 0) {
                $scope.finalDataMasterLAST.TotalAmount = $scope.finalAmountLAST;
            }


            $scope.finalDataMasterLAST.PaymentAmount = $scope.PaymentsLAST.PaymentAmount;
            $scope.finalDataMasterLAST.CheckNo = $scope.PaymentsLAST.CheckNo;
            $scope.finalDataMasterLAST.CheckAmount = $scope.PaymentsLAST.CheckAmount;
            $scope.finalDataMasterLAST.ElectronicPaymentNo = $scope.PaymentsLAST.ElectronicPaymentNo;
            $scope.finalDataMasterLAST.ElectronicAmount = $scope.PaymentsLAST.ElectronicAmount;
            $scope.finalDataMasterLAST.CashAmount = $scope.PaymentsLAST.CashAmount;
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

        $scope.dispatchedDetailFinalLAST = function () {


            var url = serviceBase + 'api/OrderDispatchedDetailsFinal?oID=' + $scope.FdispatchedMasterORDERIDLAST + '&fID=' + $scope.FdispatchedMasterIDLAST;
            $http.put(url)
                .success(function (data) {

                    alert('insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

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
        $scope.freeitem1 = function (item) {

            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "addFreeItem.html",
                controller: "ModalInstanceCtrlFreeItems", resolve: { order: function () { return item } }
            });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.ManualEdit = function (item) {

            console.log("Edit Dialog called survey");
            var modalInstance;
            var itemreturn = {
                item: item,
                orderDetails: $scope.orderDetails
            };
            modalInstance = $modal.open(
                {
                    templateUrl: "ManualEdit.html",
                    controller: "ModalInstanceCtrlForManualEdit", resolve: { inventory: function () { return itemreturn } }
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
        $scope.deliverdOrder = function (orderDetail) {

            
            var data = $scope.orderDetails;
            $scope.Disptotalqty = 0;
            for (var d = 0; d < $scope.orderDetails.length; d++) {

                $scope.Disptotalqty += $scope.orderDetails[d].qty;
            }
            if ($scope.Disptotalqty > 0) {
                $("#btnSave").prop("disabled", true);

                var flag = true;
                for (var i = 0; i < $scope.orderDetails.length; i++) {

                    data = $scope.orderDetails[i];
                    if ((data.qty || data.qty == null || data.qty == undefined) > data.CurrentNetStock && data.Deleted == false) {
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
                        else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId) {
                            var Stockcount = 0;
                            Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;
                            debugger;
                            if (Stockcount > $scope.orderDetails[j].CurrentNetStock && Stockcount > 0) {
                                alert("your stock not sufficient please purchase or remove item: ( " + $scope.orderDetails[j].itemname + " )This much Qty required : " + Stockcount);
                                $("#btnSave").removeAttr("disabled");
                                flag = false;
                                return false;
                            } else if ($scope.Dboy == undefined) {
                                alert("Please Select Delivery Boy");
                                flag = false;
                            }
                        }
                    }

                }

                
                if (flag == true) {

                    try {
                        var obj = ($scope.Dboy);
                            if (obj == undefined) {
                                alert("Please Select Delivery Boy");
                                return;
                            }
                    } catch (err) {
                        alert("Please Select Delivery Boy");
                        console.log(err);
                    }
                    $scope.OrderData.DboyId = obj.PeopleID;


                    $scope.OrderData;
                    $("#btnSave").prop("disabled", true);
                    var url = serviceBase + 'api/DamageOrderMaster/DeliverdDamage';
                    $http.put(url, $scope.OrderData)
                        .success(function (data) {
                            if (data.Status) {
                                alert(data.Message);
                                $modalInstance.close(data);
                                location.reload();
                            } else {
                                alert(data.Message);
                                $modalInstance.close(data);
                            }

                        })
                        .error(function (data) {
                            alert(data.Message);

                        });
                }


            } else { alert(" You can't dispatched zero qty line item"); return false; }
        };


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOrderInvoice11', ModalInstanceCtrlOrderInvoice11);

    ModalInstanceCtrlOrderInvoice11.$inject = ["$scope", '$http', 'DamageOrderMasterService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrlOrderInvoice11($scope, $http, DamageOrderMasterService, $modalInstance, ngAuthSettings, order) {
        console.log("order invoice modal opened");


        $scope.OrderDetails = {};
        $scope.OrderData = {};
        var d = DamageOrderMasterService.getDeatil();
        $scope.OrderData = d;
        var info = DamageOrderMasterService.getDeatilinfo();

        $scope.OrderData1 = info;

        if (info.Status == 'Pending')
            $scope.OrderData1.OrderedDate = info.CreatedDate;
        $scope.Itemcount = 0;

        for (var i = 0; i < $scope.OrderData.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.OrderData[i].qty;
        }

        $scope.totalfilterprice = 0;
        _.map($scope.OrderData, function (obj) {
            console.log("count total");
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log(obj.TotalAmt);
            console.log($scope.totalfilterprice);
        })

        if ($scope.OrderData1.Status == 'Pending' || $scope.OrderData1.Status == 'Process' || $scope.OrderData1.Status == 'Cancel') {
            setTimeout(function () {
                $(".taxtable").remove();
            }, 500)

        }
        $scope.Wdata = function () {

            $scope.warehou = {};
            $http.get(serviceBase + 'api/Warehouse?id=' + $scope.OrderData1.WarehouseId).then(function (results) {

                $scope.warehou = results.data;
            })
        }
        $scope.Wdata();

    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlForManualEdit', ModalInstanceCtrlForManualEdit);

    ModalInstanceCtrlForManualEdit.$inject = ["$scope", '$http', "$modalInstance", "inventory", "ngAuthSettings"];

    function ModalInstanceCtrlForManualEdit($scope, $http, $modalInstance, inventory, ngAuthSettings) {
        console.log("ModalInstanceCtrlForManualEdit modal opened");


        //$scope.OrderDetails = {};
        $scope.xy = true;
        $scope.qtee = 0;
        $scope.inventoryData = {};
        if (inventory) {
            console.log("category if conditon");
            $scope.Quantity = inventory.item;
            $scope.orderDetails = inventory.orderDetails;
        }
        $scope.updatelineitem = function (data) {

            if (data.qty >= 0) {
                if ($scope.Quantity.qty <= 0) {

                    alert("Quantity should not be negative");
                    $scope.Quantity.qty = 0;
                } else {
                    $scope.Quantity.qty = data.qty - 1;

                }
            }

        }
        $scope.updatelineitem1 = function (data) {

            if (data.MinOrderQty == 0) {
                $scope.Quantity.qty = data.qty + 1;
            }
            else if (data.qty != data.Noqty) {
                $scope.Quantity.qty = data.qty + 1;
            }
            else {
                alert("Quantity should not be greater than ordered qty");
            }
        }
        $scope.ok = function () {


            if ($scope.Quantity.QtyChangeReason == undefined) {
                alert("Select reason");
            } else {

                $modalInstance.close();
            }
        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Putinventory = function (data) {

            $scope.orderDetail.qty = data.qty;
            $scope.orderDetail.QtyChangeReason = data.OrderDetailsId;


        }
    }
})();

