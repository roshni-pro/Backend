

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ReturnOrderdetailsController', ReturnOrderdetailsController);

    ReturnOrderdetailsController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "peoplesService"];

    function ReturnOrderdetailsController($scope, OrderMasterService, OrderDetailsService, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService) {

        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));
        
        $scope.currentPageStores = {};
        $scope.OrderDetails = {};
        $scope.OrderData = {};        
        var d = OrderMasterService.getDeatil();
        var totalAmount=Math.round(d.TotalAmount);
        $scope.backpage = function () {

            if (d.Page == "Redispatch") {
                window.location.href = "#/Redispatch";
            } else { window.location.href = "#/orderMaster"; }
        }
        $scope.count = 1;
        $scope.OrderData = d;
        $scope.OrderData.TotalAmount = totalAmount;
        $scope.orderDetails = d.orderDetails;
        $scope.checkInDispatchedID = $scope.orderDetails[0].OrderId;

        $scope.callForDropdown = function () {

            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.checkInDispatchedID;
            $http.get(url)
                .success(function (data) {

                    $scope.DBname = {};
                    $scope.DBname = data.DboyName;

                    if (data.DiscountAmount != 0) {
                        // document.getElementById("btnSaveReturn").disabled = true;
                        $scope.discountmsg = "Discount applied !! not able to return products"
                    }
                    else if (data.DeliveryIssuanceStatus == 'Payment Accepted' || data.DeliveryIssuanceStatus == 'Submitted' || data.DeliveryIssuanceStatus == 'Pending') {
                        document.getElementById("btnSaveReturn").disabled = true;
                        $scope.discountmsg = "Cannot return products due to assignment still not Freezed:" + data.DeliveryIssuanceIdOrderDeliveryMaster;
                        alert("This order can not return products due to assignment still not Freezed. Assignment #No  " + data.DeliveryIssuanceIdOrderDeliveryMaster);
                    }
                });
        };

        $scope.callForDropdown();
        // check Order is returned or not
        var url = serviceBase + 'api/OrderDispatchedDetailsReturn?id=' + $scope.checkInDispatchedID;
        $http.get(url)
            .success(function (data) {
                if (data.length > 0) {
                    $scope.count = 0;
                    $scope.orderDetails = data;
                    //document.getElementById("btnSaveReturn").hidden = true;
                } else {
                    //document.getElementById("btnSaveReturn").hidden = false;
                    var url = serviceBase + 'api/OrderDispatchedDetails?id=' + $scope.checkInDispatchedID;
                    $http.get(url)
                        .success(function (data) {
                            if (data.length > 0) {
                                $scope.count = 0;
                                $scope.orderDetails = data;
                                //document.getElementById("btnSaveReturn").hidden = false;
                            } else {
                                //.getElementById("btnSaveReturn").hidden = true;
                            }
                        });
                }
            });
        $scope.Itemcount = 0;
        //get peoples for delivery boy
        peoplesService.getpeoples().then(function (results) {
            $scope.User = results.data;
            console.log("Got people collection");
            console.log($scope.User);
        }, function (error) {
        });
        // end
        for (var i = 0; i < $scope.orderDetails.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }
        _.map($scope.OrderData.orderDetails, function (obj) {
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log("$scope.OrderData");
            console.log($scope.totalfilterprice);
        });
        $scope.set_color = function (orderDetail) {
            if (orderDetail.qty > orderDetail.CurrentStock) {
                return { background: "#ff9999" }
            }
        };
        $scope.saveReturn = function () { 
            $("#btnSaveReturn").prop("disabled", true);

           
            //$scope.OrderData();
            angular.forEach($scope.orderDetails, function (value, key) {
                console.log(key + ': ');
                console.log(value);
            });
            var url = serviceBase + 'api/OrderDispatchedDetailsReturn';
            $http.post(url, $scope.orderDetails)
                .success(function (data) {
                    
                    if (data == "null") {
                        alert("some thing went wrong plz go back and try again / Assignment not freezd");
                        window.location = "#/orderMaster";
                    }
                    else {

                        alert("successfully return!!");
                        window.location = "#/orderMaster";
                    }

                })
                .error(function (data) {
                    alert(data.ErrorMessage);
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
    }
})();