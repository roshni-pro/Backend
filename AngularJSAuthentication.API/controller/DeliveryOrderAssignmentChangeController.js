'use strict';
app.controller('DeliveryOrderAssignmentChangeController', ['$scope', "DeliveryService", "localStorageService", "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService', '$routeParams', function ($scope, DeliveryService, localStorageService, $filter, $http, ngTableParams, $modal, WarehouseService, $routeParams) {
    $scope.DeliveryIssuanceId = $routeParams.id;
    $scope.UserRoleDA = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.items = [];
    $scope.delivereddata = [];
    $scope.cancelddata = [];
    $scope.DeliveryCanceledRequest = [];
    $scope.redispatcheddata = [];
    $scope.shippedData = [];
    $scope.DeliveryIssuanceStatus = '';
    //$scope.hostUrl = window.location.origin;
    $scope.AssignmentPaymentEnable = true;

    if ($scope.DeliveryIssuanceId > 0) {

        var url = serviceBase + "api/DeliveryOrderAssignmentChange?DeliveryIssuanceId=" + $scope.DeliveryIssuanceId;
        $http.get(url).success(function (response) {
            if (response.length > 0) {
                $scope.TotalAssignmentAmount = 0;
                $scope.CreatedDate = response[0].CreatedDate;
                $scope.DBoyId = response[0].DBoyId;
                $scope.DBoyName = response[0].DBoyName;
                $scope.DboyMobileNo = response[0].DboyMobileNo;
                $scope.DeliveryIssuanceStatus = response[0].DeliveryIssuanceStatus;
                if ($scope.DeliveryIssuanceStatus == 'Freezed' || $scope.DeliveryIssuanceStatus == 'Payment Accepted' || $scope.DeliveryIssuanceStatus == 'Payment Submitted') {
                    $("#exampleModalCenter").prop("disabled", true); //disabled
                    let obj = {
                        DeliveryIssuanceId: $scope.DeliveryIssuanceId,
                        DBoyId: $scope.DBoyId
                    }
                    debugger;
                    var rl = serviceBase + "api/TestCashCollection/OTPVerificationPopupCLoseById?AssignmentId=" + $scope.DeliveryIssuanceId;
                    $http.get(rl).success(function (res) {
                        if (res == "OTP Verified!") {
                            debugger;
                            alert(res);
                        }
                        else {
                            var otpmodalInstance;

                            otpmodalInstance = $modal.open(
                                {
                                    templateUrl: "otpDialog.html",
                                    controller: "ModalInstanceCtrlOTP",
                                    backdrop: 'static',
                                    keyboard: false,
                                    resolve:
                                    {
                                        order: function () {
                                            return obj;
                                        }
                                    }
                                });
                            otpmodalInstance.result.then(function () {

                            },
                                function () {
                                    console.log("Cancel Condintion");

                                })
                            $scope.AssignmentOTPEnable = true;
                        }
                    });
                }
                $scope.AssignmentBarcodeImage = response[0].AssignmentBarcodeImage;

                $scope.UploadedFileName = response[0].UploadedFileName;

                $scope.items = response;//response

                $scope.TotalDeliveredOrder = 0;
                $scope.TotalDeliveredOrderAmount = 0;
                $scope.TotalDeliveredCashAmount = 0;
                $scope.TotalDeliveredChequeAmount = 0;
                $scope.TotalDeliveredElectronicAmount = 0;
                $scope.TotalDeliveredEpayLater = 0;
                $scope.TotalDeliveredmPos = 0;
                $scope.TotalDeliveredGullak = 0;
                $scope.date = response.OrderedDate;
                $scope.TotalRedispatchOrder = 0;
                $scope.TotalRedispatchOrderAmount = 0;

                $scope.TotalCanceledOrder = 0;
                $scope.TotalDeliveryCanceledRequestOrder = 0;
                $scope.TotalCanceledOrderAmount = 0;
                $scope.TotalDeliveryCanceledRequestAmount = 0;
                $scope.isShowPrintBtn = false;
                for (var i = 0; i < response.length; i++) {

                    if (response[i].Status == "Delivered" || response[i].Status == "Account settled" || response[i].Status == "sattled" || response[i].Status == "Partial receiving -Bounce") {
                        $scope.delivereddata.push(response[i]);
                        $scope.TotalAssignmentAmount = $scope.TotalAssignmentAmount + response[i].GrossAmount;
                    }
                    if (response[i].Status == "Delivery Redispatch") {
                        $scope.redispatcheddata.push(response[i]);
                        $scope.TotalAssignmentAmount = $scope.TotalAssignmentAmount + response[i].GrossAmount;
                        $scope.isShowPrintBtn = true;
                    }
                    if (response[i].Status == "Shipped" || response[i].Status == "Issued" || response[i].Status == "Assigned") {
                        $scope.shippedData.push(response[i]);
                        $scope.TotalAssignmentAmount = $scope.TotalAssignmentAmount + response[i].GrossAmount;

                    }
                    if (response[i].Status == "Delivery Canceled" || response[i].Status == "Order Canceled") {
                        $scope.cancelddata.push(response[i]);
                        $scope.TotalAssignmentAmount = $scope.TotalAssignmentAmount + response[i].GrossAmount;
                        $scope.isShowPrintBtn = true;
                    }
                    if (response[i].Status == "Delivery Canceled Request" || response[i].Status == "Order Canceled") {
                        $scope.DeliveryCanceledRequest.push(response[i]);
                        $scope.TotalAssignmentAmount = $scope.TotalAssignmentAmount + response[i].GrossAmount;

                    }
                    if ($scope.AssignmentPaymentEnable && response[i].OrderRejectStatus)
                        $scope.AssignmentPaymentEnable = false;
                }

                for (var d = 0; d < $scope.delivereddata.length; d++) {

                    $scope.TotalDeliveredOrderAmount = $scope.TotalDeliveredOrderAmount + $scope.delivereddata[d].GrossAmount;
                    $scope.TotalDeliveredOrder = $scope.TotalDeliveredOrder + 1;
                    $scope.delivereddata[d].PaymentDetails.forEach(function (payment) {

                        if (payment.PaymentFrom == "Cash")
                            $scope.TotalDeliveredCashAmount = $scope.TotalDeliveredCashAmount + payment.Amount;
                        if (payment.PaymentFrom == "Cheque")
                            $scope.TotalDeliveredChequeAmount = $scope.TotalDeliveredChequeAmount + payment.Amount;
                        if (payment.PaymentFrom == "EPaylater")
                            $scope.TotalDeliveredEpayLater = $scope.TotalDeliveredEpayLater + payment.Amount;
                        if (payment.PaymentFrom == "MPos")
                            $scope.TotalDeliveredmPos = $scope.TotalDeliveredmPos + payment.Amount;
                        if (payment.PaymentFrom == "Gullak")
                            $scope.TotalDeliveredGullak = $scope.TotalDeliveredGullak + payment.Amount;
                        if (payment.PaymentFrom == "Online")
                            $scope.TotalDeliveredElectronicAmount = $scope.TotalDeliveredElectronicAmount + payment.Amount;

                    });


                    $scope.delivereddata[d].CashAmount = $scope.delivereddata[d].PaymentDetails[0].Amount;

                    $scope.delivereddata[d].ChequeAmount = $scope.delivereddata[d].PaymentDetails[1].Amount;
                    $scope.delivereddata[d].ChequeNo = $scope.delivereddata[d].PaymentDetails[1].TransRefNo;

                    $scope.delivereddata[d].EpayLaterAmount = $scope.delivereddata[d].PaymentDetails[2].Amount;
                    $scope.delivereddata[d].EpayLaterTransId = $scope.delivereddata[d].PaymentDetails[2].TransRefNo;


                    $scope.delivereddata[d].GullakAmount = $scope.delivereddata[d].PaymentDetails[3].Amount;
                    //$scope.delivereddata[d].GatewayOrderId = $scope.delivereddata[d].PaymentDetails[3].GatewayOrderId;
                    $scope.delivereddata[d].GatewayOrderId = $scope.delivereddata[d].PaymentDetails[3].TransRefNo;

                    $scope.delivereddata[d].mPosAmount = $scope.delivereddata[d].PaymentDetails[4].Amount;
                    $scope.delivereddata[d].mPosTransId = $scope.delivereddata[d].PaymentDetails[4].TransRefNo;

                    $scope.delivereddata[d].ElectronicAmount = $scope.delivereddata[d].PaymentDetails[5].Amount;
                    $scope.delivereddata[d].ElectronicTransId = $scope.delivereddata[d].PaymentDetails[5].TransRefNo;


                }
                for (var e = 0; e < $scope.redispatcheddata.length; e++) {
                    $scope.isShowPrintBtn = true;
                    $scope.TotalRedispatchOrder = $scope.TotalRedispatchOrder + 1;
                    $scope.TotalRedispatchOrderAmount = $scope.TotalRedispatchOrderAmount + $scope.redispatcheddata[e].GrossAmount;


                    $scope.redispatcheddata[e].CashAmount = $scope.redispatcheddata[e].PaymentDetails[0].Amount;

                    $scope.redispatcheddata[e].ChequeAmount = $scope.redispatcheddata[e].PaymentDetails[1].Amount;
                    $scope.redispatcheddata[e].ChequeNo = $scope.redispatcheddata[e].PaymentDetails[1].TransRefNo;

                    $scope.redispatcheddata[e].EpayLaterAmount = $scope.redispatcheddata[e].PaymentDetails[2].Amount;
                    $scope.redispatcheddata[e].EpayLaterTransId = $scope.redispatcheddata[e].PaymentDetails[2].TransRefNo;

                    $scope.redispatcheddata[e].GullakAmount = $scope.redispatcheddata[e].PaymentDetails[3].Amount;
                    $scope.redispatcheddata[e].GatewayOrderId = $scope.redispatcheddata[e].PaymentDetails[3].GatewayOrderId;

                    $scope.redispatcheddata[e].mPosAmount = $scope.redispatcheddata[e].PaymentDetails[4].Amount;
                    $scope.redispatcheddata[e].mPosTransId = $scope.redispatcheddata[e].PaymentDetails[4].TransRefNo;


                    $scope.redispatcheddata[e].ElectronicAmount = $scope.redispatcheddata[e].PaymentDetails[5].Amount;
                    $scope.redispatcheddata[e].ElectronicTransId = $scope.redispatcheddata[e].PaymentDetails[5].TransRefNo;


                }

                for (var e = 0; e < $scope.cancelddata.length; e++) {
                    $scope.isShowPrintBtn = true;
                    $scope.TotalCanceledOrder = $scope.TotalCanceledOrder + 1;
                    $scope.TotalCanceledOrderAmount = $scope.TotalCanceledOrderAmount + $scope.cancelddata[e].GrossAmount;

                    $scope.cancelddata[e].CashAmount = $scope.cancelddata[e].PaymentDetails[0].Amount;

                    $scope.cancelddata[e].ChequeAmount = $scope.cancelddata[e].PaymentDetails[1].Amount;
                    $scope.cancelddata[e].ChequeNo = $scope.cancelddata[e].PaymentDetails[1].TransRefNo;

                    $scope.cancelddata[e].EpayLaterAmount = $scope.cancelddata[e].PaymentDetails[2].Amount;
                    $scope.cancelddata[e].EpayLaterTransId = $scope.cancelddata[e].PaymentDetails[2].TransRefNo;

                    $scope.cancelddata[e].GullakAmount = $scope.cancelddata[e].PaymentDetails[3].Amount;
                    $scope.cancelddata[e].GatewayOrderId = $scope.cancelddata[e].PaymentDetails[3].GatewayOrderId;

                    $scope.cancelddata[e].mPosAmount = $scope.cancelddata[e].PaymentDetails[4].Amount;
                    $scope.cancelddata[e].mPosTransId = $scope.cancelddata[e].PaymentDetails[4].TransRefNo;

                    $scope.cancelddata[e].ElectronicAmount = $scope.cancelddata[e].PaymentDetails[5].Amount;
                    $scope.cancelddata[e].ElectronicTransId = $scope.cancelddata[e].PaymentDetails[5].TransRefNo;
                }

                for (var e = 0; e < $scope.DeliveryCanceledRequest.length; e++) {

                    $scope.TotalDeliveryCanceledRequestOrder = $scope.TotalDeliveryCanceledRequestOrder + 1;
                    $scope.TotalDeliveryCanceledRequestAmount = $scope.TotalDeliveryCanceledRequestAmount + $scope.DeliveryCanceledRequest[e].GrossAmount;

                    $scope.DeliveryCanceledRequest[e].CashAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[0].Amount;

                    $scope.DeliveryCanceledRequest[e].ChequeAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[1].Amount;
                    $scope.DeliveryCanceledRequest[e].ChequeNo = $scope.DeliveryCanceledRequest[e].PaymentDetails[1].TransRefNo;

                    $scope.DeliveryCanceledRequest[e].EpayLaterAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[2].Amount;
                    $scope.DeliveryCanceledRequest[e].EpayLaterTransId = $scope.DeliveryCanceledRequest[e].PaymentDetails[2].TransRefNo;

                    $scope.DeliveryCanceledRequest[e].GullakAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[3].Amount;
                    $scope.DeliveryCanceledRequest[e].GatewayOrderId = $scope.DeliveryCanceledRequest[e].PaymentDetails[3].GatewayOrderId;

                    $scope.DeliveryCanceledRequest[e].mPosAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[4].Amount;
                    $scope.DeliveryCanceledRequest[e].mPosTransId = $scope.DeliveryCanceledRequest[e].PaymentDetails[4].TransRefNo;

                    $scope.DeliveryCanceledRequest[e].ElectronicAmount = $scope.DeliveryCanceledRequest[e].PaymentDetails[5].Amount;
                    $scope.DeliveryCanceledRequest[e].ElectronicTransId = $scope.DeliveryCanceledRequest[e].PaymentDetails[5].TransRefNo;
                }
            }
            else {
                alert("something went wrong ");
            }
        })
            .error(function (response) {
                console.log(response);
            })

    }

    $scope.orderStatusUpdate = function (order, status) {
        var url = serviceBase + "api/DeliveryTask/AssignmentOrderStatusUpdate?assignmentId=" + $scope.DeliveryIssuanceId + "&orderId=" + order.OrderId + "&status=" + status;
        $http.get(url).success(function (response) {
            if (response) {
                alert("Order " + (status == 1 ? "Rejected" : "Accepted") + " successfully.");
                window.location.reload();
            }
            else {
                alert("Payment already submitted.");
                window.location.reload();
            }
        });
    }

    $scope.printItemAssignment = function (printSectionId) {


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
            var popupWin = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin2
            popupWin.document.open();
            popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
            popupWin.document.close();
        }
        popupWin.document.close();
        return true;
    }

    $scope.printToCart = function () {

        //var printContents = document.getElementById(printSectionId).innerHTML;
        var originalContents = document.body.innerHTML;
        if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
            var popupWin = window.open('', '_blank', 'width=800,height=600,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');


            popupWin.window.focus();
            popupWin.onbeforeunload = function (event) {
                popupWin.close();
                return '.\n';
            };
            popupWin.onabort = function (event) {
                popupWin.document.close();
                popupWin.close();
            }
        } else {

            popupWin.document.open();

            popupWin.document.close();
        }
        popupWin.document.close();
        return true;
    };

    $scope.cancel = function () {
        window.location = "#/DeliveryAssignment";
    }

    //AssignmentPayment
    $scope.AssignmentPayment = function (DeliveryIssuanceId) {

        if (DeliveryIssuanceId != 'undefined' && DeliveryIssuanceId != '') {
            debugger;
            $("#AssignmentPayment").prop("disabled", true);
            var url = serviceBase + "api/DeliveryAssignment/AssignmentPayment?DeliveryIssuanceId=" + DeliveryIssuanceId;
            $http.put(url)
                .success(function (data) {

                    if (!data.Status) {
                        alert("There is Some Problem");
                    }
                    else {
                        alert(data.Message);
                        //window.location = "#/DeliveryAssignment";
                        window.location.reload();
                    }
                })
                .error(function (data) {

                });
        }
    };

}]);


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOTP', ModalInstanceCtrlOTP);

    ModalInstanceCtrlOTP.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', '$compile', 'order'];

    function ModalInstanceCtrlOTP($scope, $http, $modalInstance, ngAuthSettings, $compile, order) {
        debugger;
        $scope.DeliveryIssuanceId = order.DeliveryIssuanceId;
        $scope.DBoyId = order.DBoyId;
        $scope.Verification = function (otp, DeliveryBoyID, AssignmentID) {

            var url = serviceBase + "api/TestCashCollection/ValidateOTPForDBoy?otp=" + otp + "&DeliveryBoyID=" + DeliveryBoyID + "&AssignmentID=" + AssignmentID;

            $http.get(url).success(function (response) {              
                if (response.Message == "Submit Code Sucessfully!!") {
                    alert(response.Message);
                    $modalInstance.close();
                } else if (response.Message == "You are not authorize to subimt OTP !!") {
                    alert(response.Message);
                    $modalInstance.close();
                }
                else {
                    alert(response.Message);
                }

            });
        }

        $scope.close = function () {
            $modalInstance.close();
            window.location.href = "#/DeliveryAssignment";
        }
    }
})();