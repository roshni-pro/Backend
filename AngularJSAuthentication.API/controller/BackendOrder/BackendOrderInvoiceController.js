'use strict';
app.controller('BackendOrderInvoiceController', ['$scope', '$timeout', '$routeParams', "$filter", "$http", "ngTableParams", '$modal', function ($scope, $timeout, $routeParams, $filter, $http, ngTableParams, $modal) {
    //

    $scope.OrderId = $routeParams.id;
    $scope.tottalll = 0;
    $scope.totalAmountOfLists = 0;
    $scope.mobilenumber = undefined;
    $scope.isqrenabled = false;
    $scope.isaddbtnenabled = false;
    $scope.isconsumerbtn = false;
        
    //$scope.ok = function () { $modalInstance.close(); },
    //    $scope.cancel = function () {
    //        $modalInstance.dismiss('canceled');
    //        console.log(hjkl);
    //    };
    $scope.Close = function () {
        $modalInstance.dismiss('cancel');  // Or use $modalInstance.close();
    };
    $scope.callOrder = function () {

        var url = serviceBase + 'api/OrderDispatchedMaster/GetBackendInvoiceData?id=' + $scope.OrderId;
        $http.get(url).success(function (data) {
            if (data) {
                $scope.OrderData = data;
                $scope.CustomerId = data.CustomerId;


                console.log($scope.OrderData, "orderData")
                $scope.tottalll = data.GrossAmount
                $scope.totalAmountOfLists = data.GrossAmount;
                if ($scope.OrderData.IsInvoiceSent == true) {
                    $scope.mobilenumber = $scope.OrderData.Customerphonenum;
                }
                var urls = serviceBase + 'api/BackendOrder/GetQrEnabledData?warehouseid=' + $scope.OrderData.WarehouseId;
                $http.get(urls).success(function (datas) {

                    $scope.isqrenabled = datas.IsQrEnabled;
                    if (datas.Storetype == 1) {
                        $scope.isconsumerbtn = true;
                    }

                })

            } else {
                alert("No Record found");
            }
        })
    }

    $scope.callOrder();
    $scope.Close = function () {
        var modalInstance

        modalInstance.dismiss();
    },
        $scope.IsSubmitbtn = false;
    $scope.ReturnQtyClose = function () {
        
        $scope.itmdetails.forEach(x => {
            if (x.Returnqty > 0) {
                alert("Please Click on Submit Btn or enter 0 or clear the Return Qty !!");
                $scope.IsSubmitbtn = true;
                return true;
            }
            if (x.Returnqty == 0 || x.Returnqty == null) {
                $scope.IsSubmitbtn = false;
            }
        })
        $scope.OrderData.orderDetails.forEach(x => {
            if (x.itemNumber == $scope.orderDetailTempData.itemNumber && x.OrderDetailsId == $scope.orderDetailTempData.OrderDetailsId) {
                x.barCode = "";
            }
        });
        if (!$scope.IsSubmitbtn) {
            $('#returnPopUp').modal('hide');
            //$modalInstance.close();
            //var modalInstance

            //modalInstance.dismiss();
        }

    },


        $scope.barCode = [];
    $scope.itmdetails = [];
    $scope.returnPopUp = false;
    $scope.orderDetailTempData = [];
    $scope.mis = false;
    $scope.returnOrderQty = function (Return, IsFreeItem, orderDetail) {
        
        if (Return == null || Return == undefined) {
            return false;
        }
        //console.log(Return.barCode, " $scope.barCode")
        $scope.orderDetailTempData = orderDetail
        //if (returnOrder.ReturnQty > 0) {
        //    if (returnOrder.qty < returnOrder.ReturnQty) {
        //        returnOrder.ReturnQty = 0;
        //        alert("You can not Enter more then Order Quantity")
        //        return false;
        //    }
        //}
        //else {
        //    returnOrder.ReturnQty = 0;
        //}
        
        if (Return.length < 5) {
            return false;
        }
        $scope.OrderData.orderDetails.forEach(x => {
            if (x.Barcode == Return || x.itemNumber == Return ) {
                //alert(x.Barcode)
                $scope.name = x.itemname + " " + x.itemNumber + " "  + x.UnitPrice;
                $scope.IsFreeItem = x.IsFreeItem;
                $scope.mis = true;
            }
        })
            if (!$scope.mis) {
                alert("Item mismatched!!!");
                return true;
            }
        
        //var filteredItem = $scope.OrderData.orderDetails.filter(x => (x.Barcode == Return || x.itemNumber == Return) && x.IsFreeItem == IsFreeItem)[0];
        //if (filteredItem == null) {
        //    alert("Item mismatched!!!");
        //    return true;
        //}

        
        //console.log(returnOrder, "returnOrder");
        $http.get(serviceBase + "api/BackendOrder/ReturnItemBatchList?keyword=" + Return + "&warehouseid=" + $scope.OrderData.WarehouseId + "&orderid=" + $scope.OrderId + '&IsFreeItem=' + $scope.IsFreeItem).then(function (Res) {
            if (Res.data.Status == true) {
                $scope.itmdetails = Res.data.ItemWiseBatchCode;
                console.log("$scope.itmdetails", $scope.itmdetails)
                $scope.returnPopUp = true
                $('#returnPopUp').modal('show');
            }
            $scope.ScanBarCode = undefined;

        });
    }
    $scope.Sum = 0;
    $scope.selectedItemNumber = "";
    $scope.BatchItemList = [];
    $scope.IsApprove = true;
    $scope.ReturnQty = function () {
        console.log('ReturnQty function is called');
        
        $scope.Sum = 0;
        $scope.OrderDetailsId = 0;
        $scope.itmdetails.forEach(x => {

            if (x.Returnqty > x.Quantity) {
                alert("Please enter The Qty less then Batch Qty!!");
                return true;
            }
            else if (x.Returnqty == undefined) {
                alert("Please enter the Return Qty!!");
                return true;
            }
            $scope.Sum += x.Returnqty;
            $scope.selectedItemNumber = x.itemNumber;
            $scope.OrderDetailsId = x.OrderDetailsId;
            $scope.BatchItemList.push(x);
        })
        $scope.OrderData.orderDetails.forEach(x => {
            if (x.itemNumber == $scope.selectedItemNumber && x.OrderDetailsId == $scope.OrderDetailsId) {
                x.retuenqty = $scope.Sum;
                if (x.retuenqty > 0) {
                    $scope.IsApprove = false;
                } else {
                    $scope.IsApprove = true;
                }
            }
        });

        console.log("$scope.OrderData.orderDetails", $scope.OrderData.orderDetails)
        $('#returnPopUp').modal('hide');

    }

    $scope.CheckReturnQty = function (Quantity, Returnqty) {
        if (Returnqty > Quantity) {
            alert("Please enter The Qty less then Batch Qty!!");
            return true;
        }
    }


    $scope.RTGSRefance = 0;
    $scope.isEnlarged = false;

    //$scope.toggleImageSize = function (event) {
    //    $scope.isEnlarged = !$scope.isEnlarged;

    //    // Adjust the image size based on the enlarged state
    //    var img = event.target;
    //    if ($scope.isEnlarged) {
    //        img.style.width = '300px'; // Set the enlarged width
    //        img.style.height = '300px'; // Set the enlarged height
    //    } else {
    //        img.style.width = '150px'; // Set the normal width
    //        img.style.height = '150px'; // Set the normal height
    //    }
    //};
    $scope.showqrcodelarge = function (item) {
        var popup = document.getElementById('imagePopup');
        var overlay = document.getElementById('overlay');

        popup.style.display = 'block';
        overlay.style.display = 'block';
    }
    $scope.showPopup = function () {
        var popup = document.getElementById('imagePopup');
        var overlay = document.getElementById('overlay');

        popup.style.display = 'block';
        overlay.style.display = 'block';
    };
    $scope.closePopup = function () {
        var popup = document.getElementById('imagePopup');
        var overlay = document.getElementById('overlay');

        popup.style.display = 'none';
        overlay.style.display = 'none';
    };

    $scope.hidde = false;
    $scope.Barcodeimg = false;
    $scope.qrrefance = false;
    /* start 22-12-2023*/
    $scope.QRCodeGen = function (item) {

        console.log($scope.dataItems)
        console.log(item)
        if (item.amount.upi == undefined || item.amount.upi == "" || item.amount.upi == null) {
            alert("Please Enter Amount");
            return false;
        }
        var PostData = {
            OrderId: $scope.OrderId,
            amount: item.amount.upi
        };

        console.log(PostData);

        var url = serviceBase + "UPI/GenerateBackEndAmtQRCode";

        $http.post(url, PostData).success(function (data) {

            console.log(data);
            if (data.Status == true) {
                item.qrCode = data.QRCodeurl;
                alert(data.msg);

                if (data.msg == "Qr Code Generated Successfully!!") {

                    //              if ($scope.dataItems.length > 0) {
                    //                 angular.forEach($scope.dataItems, function (data) {
                    //            if (item.$$hashKey == data.$$hashKey) {
                    //            
                    //            if (data.amount["upi"] > 0) {
                    //                if (data.ref["Upi"] == undefined || data.ref["Upi"] == null || data.ref["Upi"] == "" || data.ref["Upi"] == "undefined") {
                    //                    item.isupiamountbtn = false
                    //                }
                    //                else {
                    //                    item.isupiamountbtn = true
                    //                }
                    //            }

                    //        }
                    //    })
                    //}
                    if ($scope.dataItems.length > 0) {
                        angular.forEach($scope.dataItems, function (abc) {

                            if (abc.$$hashKey == item.$$hashKey) {
                                abc.isqrgenerated = true;
                            }
                        })
                    }
                    $scope.Barcodeimg = true;
                    item.qrrefance = true;
                } else {
                    item.qrrefance = false;
                }
            } else {
                alert(data.msg);
                $scope.Barcodeimg = false;
                $scope.qrrefance = false;
            }

        }).error(function (data) {
            console.log(data);
            alert("Failed.");
        });
    };


    $scope.RefanceNumber = 0;
    $scope.TNXNUBER = 0;
    $scope.RefanceNumberStatus = false;
    $scope.allfalsebtn = true;
    $scope.differenceamt = 0;
    $scope.StatusMsg = function (item) {
        var url = serviceBase + 'UPI/CheckUPIResponse?OrderId=' + $scope.OrderId + '&amount=' + item.amount.upi;
        $http.get(url)
            .success(function (data) {
                console.log(data);
                if (data.IsSuccess == true) {
                    $scope.differenceamt = 0;
                    $scope.isaddbtnenabled = true;
                    console.log($scope.dataItems);
                    if ($scope.dataItems.length > 0) {
                        //angular.forEach($scope.dataItems, function (ab) {
                        //    ab.isrefgenerated = false;
                        //    ab.ischeckstatus = false;
                        //})
                        if ($scope.dataItems.length > 0) {
                            angular.forEach($scope.dataItems, function (abc) {
                                abc.isqrgenerated = false;
                            })
                        }
                        angular.forEach($scope.dataItems, function (abc) {
                            if (abc.$$hashKey == item.$$hashKey) {
                                abc.isrefgenerated = false;
                                abc.ischeckstatus = true;
                                abc.ref["Upi"] = data.UPITxnID
                            }
                        })
                    }
                    item.isupiamountbtn = true;
                    //$scope.RefanceNumber = data.UPITxnID;
                    $scope.RefanceNumberStatus = true;
                    $scope.Barcodeimg = false;

                    $scope.allfalsebtn = true;
                    alert(data.amount + " Rs. Received Successfully");
                    if (item.amount.upi == data.amount) { }
                    else {
                        if (item.amount.upi > data.amount) {
                            $scope.differenceamt = item.amount.upi - data.amount;
                            $scope.tottalll += $scope.differenceamt;
                            angular.forEach($scope.dataItems, function (abc) {
                                if (abc.$$hashKey == item.$$hashKey) {
                                    abc.amount["upi"] = data.amount;
                                }
                            })
                        }
                    }
                } else {
                    $scope.isaddbtnenabled = false;
                    alert("payment is awaited Failed.");
                    $scope.RefanceNumberStatus = false;
                    item.isupiamountbtn = false;
                    $scope.Barcodeimg = true;
                    $scope.allfalsebtn = false;
                }

            }).error(function (data) {
                console.log(data);
                alert("Failed.");
            });
    };

    $scope.Data = {
        Cash: true,
        AmountRtgs: 0,
        AmountPOS: 0,
        AmountUPI: 0
        // other properties...
    };
    $scope.paymentmodeCash = function (mode) {

        $scope.modepaymentCash = mode;
    }

    $scope.modepaymentRTGS = false;
    $scope.paymentmodeRTGS = function (mode) {

        $scope.modepaymentRTGS = mode;
        if ($scope.modepaymentRTGS.RTGS == true) {

            $scope.modepaymentRTGS = true;
        } else {
            $scope.modepaymentRTGS = false;
            $scope.tottalll += $scope.Data.AmountRtgs;
            $scope.Data.AmountRtgs = 0;
            $scope.Data.RefanceRTGS = undefined;
            $scope.isrtgsamountbtn = false
        }
    }
    $scope.modepaymentPOS = false;
    $scope.paymentmodePOS = function (mode) {

        $scope.modepaymentPOS = mode

        if ($scope.modepaymentPOS.POS == true) {

            $scope.modepaymentPOS = true;
        } else {
            $scope.modepaymentPOS = false;
            $scope.tottalll += $scope.Data.AmountPOS;
            $scope.Data.AmountPOS = 0;
            $scope.Data.RefancePOS = undefined;
            $scope.isposamountbtn = false
        }

    }
    $scope.modepaymentUpi = false;
    $scope.dataItems = [];

    // Function to add a new row
    $scope.addSomething = function () {
        // Add a new data item
        $scope.isaddbtnenabled = false;
        $scope.dataItems.push({
            UPI: false,
            modepaymentUpi: false,
            isupiamountbtn: false,
            amount: { upi: 0 },
            Barcodeimg: false,
            ref: { Upi: '' },
            isqrgenerated: true,
            isrefgenerated: true,
            ischeckstatus: true
            //qrcode: '' // Add any default value for qrcode if needed
        });
    };

    // Initial row creation
    $scope.addSomething();

    // Function to remove the last row
    $scope.removeLast = function () {
        // Remove the last item from the array
        $scope.dataItems.pop();
    };

    // Function to handle checkbox change
    //$scope.paymentmodeUpi = function (item, index) {

    //    //alert($scope.isaddbtnenabled)
    //    //$scope.isaddbtnenabled = true

    //    item.modepaymentUpi = item.UPI;
    //    if (item.UPI == false) {
    //        if ($scope.isaddbtnenabled == false) {
    //            $scope.isaddbtnenabled = true
    //        }
    //        if ($scope.dataItems[index].amount["upi"] > 0) {
    //            $scope.tottalll += $scope.dataItems[index].amount["upi"]
    //        }
    //        $scope.dataItems.splice(index, 1)
    //    }

    //};
    $scope.paymentmodeUpi = function (item, index) {
        item.modepaymentUpi = item.UPI;

        if (item.UPI) {
            item.amount.upi = $scope.OrderData.GrossAmount;
            $scope.tottalll = 0;
            $scope.Data.AmountRtgs = 0;
            $scope.Data.AmountPOS = 0;
        } else {
            // UPI is unchecked
            $scope.tottalll += item.amount.upi;
            item.amount.upi = 0;
            $scope.showUPIInputBox = false;
        }

        checkAllModesTrue();
    };

    //$scope.modepaymentUpi = false;
    //$scope.paymentmodeUpi = function (mode) {
    //    $scope.modepaymentUpi = mode

    //    if ($scope.modepaymentUpi.UPI == true) {

    //        $scope.modepaymentUpi = true;
    //    } else {
    //        $scope.modepaymentUpi = false;

    //    }
    //}
    //$scope.dataItems = [];

    $scope.resetOtherModes = function () {
        $scope.Data.AmountPOS = 0;
        $scope.Data.AmountRtgs = 0;
        $scope.clearUPIAmounts();
        $scope.tottalll = 0;
    };

    $scope.clearUPIAmounts = function () {
        angular.forEach($scope.dataItems, function (item) {
            item.amount.upi = 0;
        });
    };

    //// Function to add a new row
    //$scope.addSomething = function () {
    //    // Add a new data item
    //    $scope.dataItems.push({
    //        UPI: false,
    //        modepaymentUpi: false,
    //        amount: { upi: 0 },
    //        Barcodeimg: false,
    //        ref: { Upi: '' }
    //    });
    //};

    //// Initial row creation
    //$scope.addSomething();
    //$scope.removeLast = function () {
    //    // Remove the last item from the array
    //    $scope.dataItems.pop();
    //};
    /*end 22-12-2023*/

    //$scope.paymentmode = function (mode) {

    //    $scope.modepayment = mode

    //    if ($scope.modepayment == "Cash") {
    //        $scope.Referen = document.getElementById("Referen").value = "";
    //        $scope.hidde = true;
    //    } else {
    //        $scope.hidde = false;
    //    }

    //}

    $scope.paymentmode = function (mode) {
        $scope.resetOtherModes();

        if (mode === "Cash") {
            $scope.tottalll = $scope.OrderData.GrossAmount;
            $scope.Data.AmountPOS = 0;
            $scope.Data.AmountRtgs = 0;
            $scope.clearUPIAmounts();
        } else {
            $scope.tottalll = 0;
        }

        $scope.hidde = (mode === "Cash");
        checkAllModesTrue();
    };

    $scope.open = function (data) {

        $scope.OrderId = data.OrderId

        console.log("Modal opened State");
        var modalInstance;

        modalInstance = $modal.open(
            {
                templateUrl: "BackendOrderInvoice.html",
                controller: "BackendOrderInvoiceController", resolve: { poapproval: function () { return $scope.items } }
            });
        modalInstance.result.then(function (selectedItem) {
        },
            function () {
                console.log("Cancel Condintion");
            })
    };
    $scope.paymentModeList = [];
    $scope.paymentlistamount = 0;
    $scope.Ispaymentupdateseen = false;
    $scope.IsAddPaymnet2 = false;
    $scope.Amount = 0;
    $scope.show = true;
    $scope.IsRefexist = true;
    $scope.IsAddPaymnetButtton = false;
    $scope.IsAddButtton = false;
    $scope.originalCashAmount = 0;
    $scope.tamount = 0;
    $scope.ttamount = 0;
    $scope.upiamount = 0;
    $scope.upiamounts = 0;
    $scope.isgreaterbtn = false;
    $scope.keypress = function (amount, OrderData) {

        $scope.tamount = 0;
        $scope.ttamount = 0;
        $scope.upiamounts = 0;
        $scope.upiamount = 0;
        // Store the original cash amount if not already stored
        if (!$scope.originalCashAmount && $scope.Data.Cash) {
            $scope.originalCashAmount = $scope.tottalll;
        }

        $scope.ttamount = $scope.Data.AmountRtgs + $scope.Data.AmountPOS;
        if ($scope.dataItems.length > 0) {
            angular.forEach($scope.dataItems, function (item) {
                if (item.UPI) { $scope.upiamount += item.amount.upi; }
            })
            if ($scope.upiamount > 0) {
                $scope.ttamount += $scope.upiamount;
            }
        }
        $scope.originalCashAmount = $scope.totalAmountOfLists;
        if ($scope.ttamount > $scope.totalAmountOfLists) {
            if ($scope.Data.AmountRtgs == amount) {
                $scope.Data.AmountRtgs = 0;

                $scope.tamount = $scope.originalCashAmount - $scope.Data.AmountRtgs - $scope.Data.AmountPOS - $scope.Data.AmountUPI;
                if ($scope.dataItems.length > 0) {
                    angular.forEach($scope.dataItems, function (item) {
                        if (item.UPI) { $scope.upiamounts += item.amount.upi; }
                    })
                    if ($scope.upiamount > 0) {
                        $scope.tamount -= $scope.upiamounts;
                    }
                }
                $scope.tottalll = $scope.tamount;
                alert("please Enter Correct Amount")
                return false;
            }
            if ($scope.Data.AmountPOS == amount) {
                $scope.Data.AmountPOS = 0;

                $scope.tamount = $scope.originalCashAmount - $scope.Data.AmountRtgs - $scope.Data.AmountPOS - $scope.Data.AmountUPI;
                if ($scope.dataItems.length > 0) {
                    angular.forEach($scope.dataItems, function (item) {
                        if (item.UPI) { $scope.upiamounts += item.amount.upi; }
                    })
                    if ($scope.upiamount > 0) {
                        $scope.tamount -= $scope.upiamounts;
                    }
                }
                $scope.tottalll = $scope.tamount;
                alert("please Enter Correct Amount")
                return false;
            }
            else {
                angular.forEach($scope.dataItems, function (item) {
                    if (item.UPI) {
                        if (item.amount.upi == amount) {
                            item.amount.upi = 0;
                            $scope.tamount = $scope.originalCashAmount - $scope.Data.AmountRtgs - $scope.Data.AmountPOS - $scope.Data.AmountUPI;
                            if ($scope.dataItems.length > 0) {
                                angular.forEach($scope.dataItems, function (item) {
                                    if (item.UPI) { $scope.upiamounts += item.amount.upi; }
                                })
                                if ($scope.upiamount > 0) {
                                    $scope.tamount -= $scope.upiamounts;
                                }
                            }
                            $scope.tottalll = $scope.tamount;
                            alert("please Enter Correct Amount")
                            return false;
                        }
                    }
                })

            }

        }
        $scope.tottalll = $scope.originalCashAmount - $scope.Data.AmountRtgs - $scope.Data.AmountPOS - $scope.Data.AmountUPI;
        // Add the amount back if UPI, RTGS, or POS amount is deleted
        if ($scope.Data.Cash && amount === "") {
            $scope.tottalll = $scope.originalCashAmount;
        } else if ($scope.Data.RTGS && amount === "") {
            $scope.tottalll = $scope.originalCashAmount - $scope.Data.AmountRtgs;
        } else if ($scope.Data.POS && amount === "") {
            $scope.tottalll = $scope.originalCashAmount - $scope.Data.AmountPOS;
        } else if ($scope.Data.UPI && amount === "") {
            // Assuming UPI amount is stored in Data.AmountUPI
            $scope.tottalll = $scope.originalCashAmount - $scope.Data.AmountUPI;
        }
        // Subtract additional amounts (UPI, RTGS, POS) if entered
        angular.forEach($scope.dataItems, function (item) {

            if (item.UPI) {

                $scope.tottalll -= item.amount.upi;
            }
            if (item.RTGS) {


                $scope.tottalll -= item.amount.rtgs;
            }
            if (item.POS) {

                $scope.tottalll -= item.amount.pos;
            }
        });

        // Calculate total amount for all payment modes
        var totalUpiAmount = 0;
        var totalRtgsAmount = $scope.Data.AmountRtgs || 0;
        var totalPosAmount = $scope.Data.AmountPOS || 0;

        angular.forEach($scope.dataItems, function (item) {
            totalUpiAmount += item.amount.upi || 0;
            totalRtgsAmount += item.amount.rtgs || 0;
            totalPosAmount += item.amount.pos || 0;
        });

        // Calculate total amount of all lists
        /*var totalAmountOfLists = $scope.tottalll + totalUpiAmount + totalRtgsAmount + totalPosAmount;*/
        $scope.totalAmountOfLists = $scope.tottalll + totalUpiAmount + totalRtgsAmount + totalPosAmount;
        /*console.log("Total Amount of All Lists:", $scope.totalAmountOfLists);*/
        if ($scope.tottalll > 0) {
            $scope.Data.Cash = true;
        }
        else {
            $scope.Data.Cash = false;
        }
        console.log($scope.dataItems)
    }
    $scope.CancelOrder = function (order) {

        var dataToPost = {
            BOPayments: $scope.paymentModeList,
            OrderId: $scope.OrderId,
            Status: 'Order Cancel'
        };

        var url = serviceBase + "api/BackendOrder/UpdatePaymentStatus";

        $http.post(url, dataToPost).success(function (data) {

            console.log(data)
            if (data.Status == true) {
                alert(data.Message)
                window.location.reload();
            } else {
                alert(data.Message)
                window.location.reload();
            }

            /* window.location = "#/BackedOrderInvoice";*/

        }).error(function (data) {
            console.log(data);

            alert("Failed.");
        });
    }

    $scope.popup = false;
    $scope.isClickedReturn = false;

    $scope.ShowPopUp = function () {
        $scope.popup = true;
        $scope.isClickedReturn = true;
        $scope.ConfirmPopup = true;
        $timeout(function () {
            var inputElement = document.getElementById('mobilenumber');
            if (inputElement) {
                inputElement.focus();

                //$http.get(serviceBase + "api/BackendOrder/ReturnItemBatchList?keyword=" + 'lux' + "&warehouseid=" + $scope.OrderData.WarehouseId + "&orderid=" + $scope.OrderId).then(function (Res) {

                //    if (Res.data.Status == true) {
                //        $scope.itmdetails = Res.data.ItemWiseBatchCode;
                //        console.log("$scope.itmdetails", $scope.itmdetails)
                //        $scope.returnPopUp = true
                //     }


                //    });

            }
        }, 0);
    };



    $scope.ConfirmPopup = false;
    $scope.ConfirmationPopup = function () {
        $scope.popup = false;
        $scope.ConfirmPopup = true;
    }

    $scope.returnamount = 0;
    $scope.ApproveReturn = function () {
        $('#isshowreturnpopup').modal('show');
        
        if ($scope.isClickedReturn) {
            var selectedReturnItem = $scope.OrderData.orderDetails.filter(x => x.retuenqty > 0)[0];
            
            if (selectedReturnItem == null) {
                alert("Please Scan Atleast 1 item!!");
                return true;
            }
            else {
                $scope.isClickedReturn = false;
            }
        }
        if (!$scope.isClickedReturn) {

            console.log($scope.OrderData.orderDetails)
            $scope.d = [];
            angular.forEach($scope.OrderData.orderDetails, function (item, key) {
                

                if (item.retuenqty > 0) { } else { item.retuenqty = 0; }
                var filterbatchcode = $scope.BatchItemList.filter(x => x.OrderDetailsId == item.OrderDetailsId && x.IsFreeItem == item.IsFreeItem);
                $scope.returnamount = $scope.returnamount + (item.retuenqty * parseInt(item.UnitPrice));
                if (item.retuenqty > 0) {
                    var returnItemData = {
                        "ItemId": item.ItemId,
                        "ItemMultiMRPId": item.ItemMultiMRPId,
                        "Returnqty": item.retuenqty,
                        "orderDetailsid": item.OrderDetailsId,
                        "IsFreeItem": item.IsFreeItem,
                        "ItemWiseBatchCodeLists": filterbatchcode
                    };
                    $scope.d.push(returnItemData);
                }



            })
            var dataToPost = {
                "OrderId": $scope.OrderId,
                "ReturnItemDetails": $scope.d
            };
            //var dataToPost = {
            //    "OrderId": $scope.OrderId,
            //    "ReturnItemDetails": $scope.d
            //}
            console.log("dataToPost", dataToPost)
            var url = serviceBase + "api/BackendOrder/CalculateReturnOrderBillDiscount";
            $http.post(url, dataToPost).success(function (Res) {
                console.log("CalculateReturnOrderBillDiscount Res", Res);
                $scope.ConfirmPopup = true;
                if (Res != null) {
                    
                    $scope.CalculateBillDiscount = Res.CalculateBillDiscount;
                    $scope.ValidTill = Res.ValidTill;
                    $scope.ReturnAmount = Res.ReturnAmount;


                }
            }).error(function (data) {
                alert("Failed.");
            });
        }
    }

    $scope.onCloseApprovalReturnpopup = function () {
        $('#isshowreturnpopup').modal('hide');
    }

    $scope.CreditNoteGenerated = function () {
        var dataToPost = {
            "OrderId": $scope.OrderId,
            "CustomerId": $scope.CustomerId,
            "ReturnItemDetails": $scope.d,
        };
        var url = serviceBase + "api/BackendOrder/CreateConsumerReturnOrder"
        $http.post(url, dataToPost).success(function (Res) {
            console.log("CreateConsumerReturnOrder", Res);
            //if (Res != null) {
                
            //    $scope.CalculateBillDiscount = Res.CalculateBillDiscount;
            //    $scope.ValidTill = Res.ValidTill;
            //    $scope.ReturnAmount = Res.ReturnAmount;
            if (Res.Status) {
                alert(Res.Message);
                window.location = "#/BackedOrderInvoice/" + Res.Data;
                window.location.reload();
            }



            //}
        }).error(function (data) {
            alert("Failed.");
        });

        console.log("orderDetail", $scope.orderDetail);



    }
    $scope.SendInvoice = function (Order) {

        if ($scope.OrderData.IsInvoiceSent == true) {
            if ($scope.mobilenumber.length == 10) {

            }
            else {
                alert("Please Enter 10 Digit Mobile Number")
                return false;
            }
        }
        else {
            $scope.mobilenumber = "";
        }
        var url = serviceBase + "api/BackendOrder/GetBackendOrderInvoiceHtml?id=" + Order.OrderId + "&Mobile=" + $scope.mobilenumber;
        $http.post(url).success(function (data) {
            alert(data);
            window.location.reload();
        })
    }

    $scope.onmobilenumberchange = function (a) {
        $scope.mobilenumber = a;
    }

    $scope.isrtgsamountbtn = false;
    $scope.RTGSRef = function (data) {
        if (data.AmountRtgs > 0) {
            console.log(data)
            if (data.RefanceRTGS == undefined || data.RefanceRTGS == null || data.RefanceRTGS == "" || data.RefanceRTGS == "undefined") {
                $scope.isrtgsamountbtn = false;
            }
            else {
                $scope.isrtgsamountbtn = true;
            }
        }

    }
    $scope.isposamountbtn = false;
    $scope.POSRef = function (data) {
        if (data.AmountPOS > 0) {
            console.log(data)
            if (data.RefancePOS == undefined || data.RefancePOS == null || data.RefancePOS == "" || data.RefancePOS == "undefined") {
                $scope.isposamountbtn = false;
            }
            else {
                $scope.isposamountbtn = true;
            }
        }
    }
    $scope.UPIRef = function (data) {
        console.log(data)
        console.log($scope.dataItems)
        console.log(data.$$hashKey)
        if ($scope.dataItems.length > 0) {
            angular.forEach($scope.dataItems, function (item) {
                if (item.$$hashKey == data.$$hashKey) {
                    if (item.amount["upi"] > 0) {
                        if (data.ref["Upi"] == undefined || data.ref["Upi"] == null || data.ref["Upi"] == "" || data.ref["Upi"] == "undefined") {
                            item.isupiamountbtn = false
                        }
                        else {
                            item.isupiamountbtn = true
                        }
                    }

                }
            })
        }
    }

    $scope.closepaymentdetail = function () {
        if (confirm("Are you sure want Close this Window")) {
            window.location.reload()
        }
    }

    $scope.updatepaymenttotalamount = 0;
    $scope.isupdatepaymentbtn = true;

    $scope.updatepayment = function (data) {
        
        $scope.paymentModeList = [];
        $scope.updatepaymenttotalamount = 0;
        $scope.isupdatepaymentbtn = true;
        if ($scope.tottalll > 0) {
            var dataToPost = {
                'PaymentMode': "Cash",
                'PaymentRefNo': "",
                'Amount': $scope.tottalll
            };
            $scope.paymentModeList.push(dataToPost);
        }
        if ($scope.Data.AmountPOS > 0) {
            if ($scope.Data.RefancePOS == undefined || $scope.Data.RefancePOS == "" || $scope.Data.RefancePOS == null || $scope.Data.RefancePOS == "undefined") {
                alert("Please Enter POS Reference Number")
                $scope.isupdatepaymentbtn = false;
                return false;
            }
            else {
                var dataToPost = {
                    'PaymentMode': "POS",
                    'PaymentRefNo': $scope.Data.RefancePOS,
                    'Amount': $scope.Data.AmountPOS
                };
                $scope.paymentModeList.push(dataToPost);
            }
        }
        if ($scope.Data.AmountRtgs > 0) {
            if ($scope.Data.RefanceRTGS == undefined || $scope.Data.RefanceRTGS == "" || $scope.Data.RefanceRTGS == null || $scope.Data.RefanceRTGS == "undefined") {
                alert("Please Enter RTGS Reference Number")
                $scope.isupdatepaymentbtn = false;
                return false;
            }
            else {
                var dataToPost = {
                    'PaymentMode': "RTGS",
                    'PaymentRefNo': $scope.Data.RefanceRTGS,
                    'Amount': $scope.Data.AmountRtgs
                };
                $scope.paymentModeList.push(dataToPost);
            }
        }
        angular.forEach($scope.dataItems, function (item) {

            if (item.amount["upi"] > 0) {

                if (item.ref["Upi"] == undefined || item.ref["Upi"] == "" || item.ref["Upi"] == null || item.ref["Upi"] == "undefined") {
                    alert("Please Enter UPI Refernce Number");
                    $scope.isupdatepaymentbtn = false;
                    return false;
                }
                else {
                    var dataToPost = {
                        'PaymentMode': "UPI",
                        'PaymentRefNo': item.ref["Upi"],
                        'Amount': item.amount["upi"]
                    };
                    $scope.paymentModeList.push(dataToPost);
                }
            }
        });

        if ($scope.isupdatepaymentbtn == true) {
            angular.forEach($scope.paymentModeList, function (item) {
                $scope.updatepaymenttotalamount += item.Amount
            })

            if ($scope.updatepaymenttotalamount == $scope.totalAmountOfLists) {
                var dataToPost = {
                    BOPayments: $scope.paymentModeList,
                    OrderId: $scope.OrderId,
                    Status: 'PaymentSuccess'
                };
                console.log("finallist", dataToPost)
                //if (OrderData.GrossAmount == $scope.paymentlistamount) {
                //    $scope.Ispaymentupdateseen = true;
                //    $scope.IsAddPaymnet2 = true;
                //}
                if (confirm("Are you sure want to Update the Payment")) {
                    var url = serviceBase + "api/BackendOrder/UpdatePaymentStatus";
                    $http.post(url, dataToPost).success(function (data) {
                        console.log(data)
                        if (data.Status == true) {
                            alert(data.Message)
                            window.location.reload();
                        } else {
                            alert(data.Message)
                            window.location.reload();
                        }
                    }).error(function (data) {
                        alert("Failed.");
                    });
                }

            }
            else {
                alert("Amount Mismatched")
                return false;
            }
        }
    }

    $scope.amountdecimal = function (e) {
        //
        var charCode = (event.which) ? event.which : event.keyCode;

        // Allow only digits (0-9), backspace, and a maximum of two decimal places
        if ((charCode < 48 || charCode > 57) && charCode !== 8 && charCode !== 46) {
            event.preventDefault();
        }

        // Allow only one decimal point
        if (charCode === 46 && event.target.value.indexOf('.') !== -1) {
            event.preventDefault();
        }

        // Allow only two digits after the decimal point
        var dotIndex = event.target.value.indexOf('.');
        if (dotIndex !== -1 && event.target.value.length - dotIndex > 2) {
            event.preventDefault();
        }
    }
    $scope.preventSpaceAndMinus = function (event) {
        var charCode = (event.which) ? event.which : event.keyCode;

        // Allow only digits and letters, and prevent invalid characters
        if (!((charCode >= 48 && charCode <= 57) || (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122))) {
            event.preventDefault();
        }
    };
    $scope.PreventMinus = function (e) {
        //
        if (e.keyCode != 45 || e.keyCode != 32) {
        }
        else {
            event.preventDefault();
        }
    }
    $scope.sort = {
        column: '',
        descending: false
    };

    $scope.changeSorting = function (column) {

        var sort = $scope.sort;

        if (sort.column == column) {
            sort.descending = !sort.descending;
        } else {
            sort.column = column;
            sort.descending = false;
        }
    };

    $scope.selectedCls = function (column) {
        return column == scope.sort.column && 'sort-' + scope.sort.descending;
    };

    $scope.showInvoice = function (data) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "BackendOrderInvoiceModel.html",
                controller: "ModalInstanceCtrlBackendOrderInvoice",
                resolve: {
                    order: function () {
                        return $scope.OrderData
                    }
                }
            });
        modalInstance.result.then(function () {
        },
            function () {
                console.log("Cancel Condintion");
            })
    };
    $scope.showZilaInvoice = function (data) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "BackendOrderInvoiceZilaModel.html",
                controller: "ModalInstanceCtrlBackendOrderInvoice",
                resolve: {
                    order: function () {
                        return $scope.OrderData
                    }
                }
            });
        modalInstance.result.then(function () {
        },
            function () {
                console.log("Cancel Condintion");
            })
    };
    $scope.PrintCreditNoteInvoice = null;
    $scope.creditNoteInvoice = function (data) {
        
        $http.get(serviceBase + "api/BackendOrder/PrintCreditNoteInvoice?Orderid=" + $scope.OrderData.OrderId).then(function (results) {
            
            $scope.PrintCreditNoteInvoice = results.data;
            console.log("PrintCreditNoteInvoice ", results.data);
            if ($scope.PrintCreditNoteInvoice.Status) {
                $scope.PrintCreditNoteNumber = $scope.PrintCreditNoteInvoice.CreditNoteNumber;
                $scope.PrintAmount = $scope.PrintCreditNoteInvoice.Amount;
                $scope.PrintCreditNoteCreatedDate = $scope.PrintCreditNoteInvoice.CreatedDate;
                $scope.PrintValidTill = $scope.PrintCreditNoteInvoice.CreditNoteValidTill;

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "BackendOrdercreditNoteInvoiceModel.html",
                        controller: "ModalInstanceCtrlBackendOrderInvoice",
                        resolve: {
                            order: function () {
                                var postData = {
                                    "PrintCreditNoteInvoice": $scope.PrintCreditNoteInvoice,
                                    "OrderData": $scope.OrderData,
                                }
                                return postData;
                            }
                        }
                    });
                modalInstance.result.then(function () {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            }

        })
    };

    $scope.CancelCreditPopop = function () {
        $modalInstance.close();


    }
    //$scope

}]);

app.controller("ModalInstanceCtrlBackendOrderInvoice", ["$scope", '$http', "$modalInstance", 'ngAuthSettings', 'order', "$rootScope",
    function ($scope, $http, $modalInstance, ngAuthSettings, order, $rootScope) {
        $scope.PrintCreditNoteInvoice = order.PrintCreditNoteInvoice;
        
        console.log($scope.OrderData1);
        $scope.isShoW = false;

        $scope.OrderData1 = order;
        $scope.Close = function () {
            $modalInstance.dismiss('cancel');  // Or use $modalInstance.close();
        };
        //$scope.customerbackendorderinvoice = function () { // This would fetch the data on page change.
        //    var url = serviceBase + "api/BackendOrder/GetBackendOrderInvoiceHtml?id=" + $scope.OrderData1.OrderId;
        //    $http.post(url).success(function (response) {

        //    });
        //};
        //$scope.customerbackendorderinvoice();
        $scope.wid = $scope.OrderData1.WarehouseId != null ? $scope.OrderData1.WarehouseId : $scope.OrderData1.OrderData.WarehouseId ;
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

        $scope.ok = function () { $modalInstance.close(); }

        //$scope.Itemcount = 0;

        //$scope.offerbill = function () { // This would fetch the data on page change.           
        //    var url = serviceBase + "api/offer/GetOfferBill?oderid=" + $scope.OrderData1.OrderId;
        //    $http.get(url).success(function (results) {
        //        $scope.InvoiceOrderOffer = results //ajax request to fetch data into vm.data                
        //    });
        //};

        //$scope.offerbill();
        $scope.HideCessColumn = false;
        //function to remove zero cess  item column from invoice print
        angular.forEach($scope.OrderData1.orderDetails, function (item) {
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
        $scope.totalfilterprice = 0;
        _.map($scope.OrderData1.orderDetails, function (obj) {
            //$scope.Wdata();
            console.log("count total");
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log(obj.TotalAmt);
            console.log($scope.totalfilterprice);
        });

        $scope.paymentdetail = [];

        $scope.GetPayment = function () {
            // 
            var url = serviceBase + 'api/OrderMastersAPI/Getpaymentstatus?OrderId=' + $scope.OrderData1.OrderId;
            $http.get(url).success(function (response) {
                // 
                $scope.paymentdetail = response;
                console.log(' $scope.paymentdetail: ', $scope.paymentdetail);

            });
        };
        $scope.GetPayment();

        $scope.CustomerCriticalInfo = "";
        $scope.CustomerCount = {};
        $scope.SumDataHSNDetails = function () {
            $rootScope.InvoiceAmountInWord = "";
            var amount = $scope.OrderData1.GrossAmount - ($scope.OrderData1.DiscountAmount ? $scope.OrderData1.DiscountAmount : 0);
            $http.get(serviceBase + 'api/OrderMaster/GetInvoiceAmountToWord?Amount=' + amount).then(function (results) {
                $rootScope.InvoiceAmountInWord = results.data;
            });

            var url = serviceBase + "api/OrderMaster/RTDgetSuminvoiceHSNCodeData?OrderId=" + $scope.OrderData1.OrderId;
            $http.get(url).success(function (results) {
                $scope.SumDataHSN = results;
            });
        };
        $scope.hsnsummarydc = [];
        $scope.ZillaInvoice = function () {
            
            $http.get(serviceBase + "api/OrderDispatchedMaster/GetBackendZilaInvoiceData?id=" + $scope.OrderData1.OrderId).then(function (results) {
                $scope.GetBackendZilaInvoiceData = results.data;
                $scope.hsnsummarydc = results.data[0].HsnSummaryDCs
                console.log("GetBackendZilaInvoiceData", $scope.hsnsummarydc);
                console.log("GetBackendZilaInvoiceData", $scope.GetBackendZilaInvoiceData);
            });
        }

        $scope.ZillaInvoice();
        $scope.indexToLetter = function (index) {
            const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            let letter = '';
            while (index >= 0) {
                letter = alphabet[index % 26] + letter;
                index = Math.floor(index / 26) - 1;
            }
            return letter;
        };

        $scope.SumDataHSNDetails();
        $scope.getTotalTax = function (data) {
            var totaltax = 0;
            data.forEach(x => {
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

        $scope.getTotalIOverall = function (data) {

            var TotalIOverall = 0;
            data.forEach(x => {

                TotalIOverall = TotalIOverall + x.AmtWithoutTaxDisc + x.SGSTTaxAmmount + x.CGSTTaxAmmount + x.CessTaxAmount;
            });
            return TotalIOverall;
        }


        //000000//


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

        //.....................................//
        //added end new//


    }]);