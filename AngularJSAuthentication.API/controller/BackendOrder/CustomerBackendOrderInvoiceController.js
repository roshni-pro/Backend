'use strict';
app.controller('CustomerBackendOrderInvoiceController', ['$scope', '$routeParams', "$filter", "$http", "ngTableParams", '$modal', function ($scope, $routeParams, $filter, $http, ngTableParams, $modal) {
    
    
    $scope.OrderId = $routeParams.id;
    
    $scope.OrderData1 = [];
    $scope.FromWarehouseDetail = [];
    $scope.callOrder = function () {

        var url = serviceBase + 'api/OrderDispatchedMaster/GetBackendInvoiceData?id=' + $scope.OrderId;
        $http.get(url).success(function (data) {
            if (data) {
                
                $scope.OrderData1 = data;
                $scope.wid = $scope.OrderData1.WarehouseId;
                
                
                $scope.getWarehousedetail = function () { 
                    var url = serviceBase + "api/Warehouse" + "?id=" + $scope.wid;
                    $http.get(url).success(function (response) {
                        
                        $scope.FromWarehouseDetail = response; //ajax request to fetch data into vm.data
                        console.log($scope.FromWarehouseDetail)
                        console.log("get current Page items:");
                    });
                };
                $scope.getWarehousedetail();

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

                $scope.SumDataHSNDetails = function () {
                    
                    $scope.InvoiceAmountInWord = "";
                    var amount = $scope.OrderData1.GrossAmount - ($scope.OrderData1.DiscountAmount ? $scope.OrderData1.DiscountAmount : 0);
                    $http.get(serviceBase + 'api/OrderMaster/GetInvoiceAmountToWord?Amount=' + amount).then(function (results) {
                        $scope.InvoiceAmountInWord = results.data;
                    });

                    //var url = serviceBase + "api/OrderMaster/RTDgetSuminvoiceHSNCodeData?OrderId=" + $scope.OrderData1.OrderId;
                    //$http.get(url).success(function (results) {
                    //    $scope.SumDataHSN = results;
                    //});
                };
                $scope.SumDataHSNDetails();
                console.log($scope.OrderData1)
            } else {
                alert("No Record found");
            }
        })
    }

    $scope.callOrder();

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

}])