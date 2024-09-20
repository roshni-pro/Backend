'use strict';
app.controller('InActiveCustOrderMasterController', ['$scope', 'OrderMasterService', 'OrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, OrderMasterService, OrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {

            $scope.statuses = [];
            $scope.orders = [];
            $scope.customers = [];
            $scope.selectedd = {};
            $scope.statusname = {};
            $scope.srch = { skcode: "", mobile: "", status: "", WarehouseId: 0 };
            // status based filter
            $scope.filterOptions = {
                stores: [
                    { id: 1, name: 'All' },
                    { id: 2, name: 'Dummy Order' },
                    { id: 3, name: 'Order Confirmed' },
                    { id: 4, name: 'Order Canceled' },
                    { id: 5, name: 'Inactive' },
                    { id: 6, name: 'Dummy Order Cancelled' },
                ]
            };

            $scope.filterItem = {
                store: $scope.filterOptions.stores[0]
            }
            $scope.customFilter = function (data) {
                if (data.Status === $scope.filterItem.store.name) {
                    return true;
                } else if ($scope.filterItem.store.name === 'Show All') {
                    return true;
                } else {
                    return false;
                }
            };



            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;
            $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown
            $scope.warehouse = [];
            OrderMasterService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

                //$scope.srch.WarehouseId = $scope.warehouse[0].WarehouseId;

                $scope.getData($scope.pageno);

            }, function (error) {
            });

            $scope.getData = function (pageno) {

                if ($scope.srch.WarehouseId > 0 && $scope.srch.skcode == "" && $scope.srch.mobile == "" && !$scope.srch.status && pageno != undefined) {

                    $scope.customers = [];
                    //var url = serviceBase + "api/InActiveCustOrderMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.srch.WarehouseId;
                    //$http.get(url).success(function (response) {

                    //    $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data
                    //    $scope.orders = $scope.customers;
                    //    $scope.total_count = response.total_count;
                    //});

                    var WarehouseIds = [parseInt($scope.srch.WarehouseId)];

                    $http.post(serviceBase + "api/InActiveCustOrderMaster?list=" + $scope.itemsPerPage + " &page=" + pageno, WarehouseIds).success(function (response, status) {
                        debugger
                        $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                    });

                }
                else {

                    $scope.searchdata($scope.srch, pageno);
                }
            };
            $scope.Refresh = function () {

                $scope.srch = "";
                $scope.getData($scope.pageno);
            }
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });
                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");


                });
            });
            //$(function () {
            //    $('input[name="daterangedata"]').daterangepicker({
            //        timePicker: true,
            //        timePickerIncrement: 5,
            //        timePicker12Hour: true,
            //        format: 'MM/DD/YYYY h:mm A'
            //    });
            //    $('.input-group-addon').click(function () {
            //        $('input[name="daterange"]').trigger("select");
            //        //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            //    });
            //});
            $scope.InitialData = [];
            $scope.searchdata = function (data, pageno) {
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && data.WarehouseId == null && (data.mobile == "" || data.skcode == "" || data.status == "")) {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                else if ($scope.srch == "" && $('#dat').val()) {
                    $scope.srch = { skcode: "", shopName: "", mobile: "", status: '' }
                }
                else if ($scope.srch != "" && !$('#dat').val()) {
                    start = null;
                    end = null;

                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }

                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                else {

                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                $scope.orders = [];
                $scope.customers = [];

                var stts = "";
                if ($scope.statusname.name && $scope.statusname.name != "All") {
                    stts = $scope.statusname.name;
                }
                var WarehouseIds = [parseInt($scope.srch.WarehouseId)];
                if (!pageno)
                    pageno = 1; //initialize page no to                
                $scope.itemsPerPage = 20;

                $http.post(serviceBase + "api/InActiveCustOrderMaster/Search?list=" + $scope.itemsPerPage + "&page=" + pageno + "&start=" + start + "&end=" + end + "&Skcode=" + $scope.srch.skcode + "&Mobile=" + $scope.srch.mobile + "&status=" + stts, WarehouseIds).success(function (response, status) {
                    $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data                 
                    $scope.total_count = response.total_count;
                });
                //var url = serviceBase + "api/InActiveCustOrderMaster/Search?start=" + start + "&end=" + end + "&Skcode=" + $scope.srch.skcode + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
                //$http.get(url).success(function (response) {

                //    $scope.customers = response;  //ajax request to fetch data into vm.data
                //    $scope.total_count = response.length;

                //});

            }


            $scope.Export = function (data) {

                var start = "";
                var end = "";
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                if (!$('#dat').val()) {
                    end = '';
                    start = '';

                }
                else {
                    start = f.val();
                    end = g.val();
                }


                $scope.datareport = [];
                var WarehouseIds = [parseInt(data)];

                $http.post(serviceBase + "api/InActiveCustOrderMaster/getInActiveorder?start=" + start + " &end=" + end, WarehouseIds).success(function (data, status) {

                    $scope.datareport = data;
                    angular.forEach($scope.datareport, function (value, key) {
                        if (value.OrderType == 0 || value.OrderType == 1)
                            value.OrderTypestr = "General";
                        else if (value.OrderType == 2)
                            value.OrderTypestr = "Bundle";
                        else if (value.OrderType == 3)
                            value.OrderTypestr = "Return";
                        else if (value.OrderType == 4)
                            value.OrderTypestr = "Distributor";
                        else
                            value.OrderTypestr = "C";
                    });
                    alasql('SELECT OrderId,WarehouseName,SalesPerson,SalesMobile,CustomerId,CustomerName,Skcode ,ShopName,invoice_no,Status,GrossAmount, ShippingAddress,CreatedDate,OrderTypestr INTO XLSX("InActiveCustomerOrderMaster.xlsx",{headers:true}) FROM ?', [$scope.datareport]);

                });

                // $http.get(serviceBase + "api/InActiveCustOrderMaster/getInActiveorder?WarehouseId=" + data  + "&start=" + start + " &end=" + end   )
                //    .success(function (data) {
                //    $scope.datareport = data;
                //     alasql('SELECT OrderId,WarehouseName,SalesPerson,SalesMobile,CustomerId,CustomerName,Skcode ,ShopName,invoice_no,Status,GrossAmount, ShippingAddress,CreatedDate INTO XLSX("InActiveCustomerOrderMaster.xlsx",{headers:true}) FROM ?', [$scope.datareport]);
                //})

                //  .error(function (data) {
                //  })

            };


            $scope.showDetail = function (data) {
                $http.get(serviceBase + 'api/InActiveCustOrderMaster?id=' + data.OrderId).then(function (results) {

                    $scope.myData = results.data;
                    OrderMasterService.save($scope.myData)
                    setTimeout(function () {
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                templateUrl: "myInactiveOrderdetail.html",
                                controller: "InactiveOrderorderdetailsController", resolve: { Ord: function () { return $scope.items } }
                            }), modalInstance.result.then(function (selectedItem) {

                                console.log("modal close");

                            },
                                function () {
                                })
                    }, 500);

                });
                console.log("Order Detail Dialog called ...");
            };
        }
    }]);

app.controller('InactiveOrderorderdetailsController', ['$scope', 'OrderMasterService', 'OrderDetailsService', 'WarehouseService', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "peoplesService", '$modal', 'BillPramotionService', "$modalInstance", "$filter", function ($scope, OrderMasterService, OrderDetailsService, WarehouseService, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService, $modal, BillPramotionService, $modalInstance, $filter) {
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); }
    console.log("orderdetailsController start loading OrderDetailsService");

    //End User Tracking
    $scope.OrderDetails = {};
    $scope.OrderData = {};
    $scope.OrderData1 = {};
    //$scope.IsStatusDisable = true;
    var d = OrderMasterService.getDeatil();

    //if (d.PaymentMode != "COD") {
    //    $scope.IsStatusDisable = false;
    //}
    //else {
    //    $scope.IsStatusDisable = true;
    //}

    console.log(d);
    $scope.signdata = {};

    $scope.count = 1;
    $scope.OrderData = d;
    console.log("$scope.OrderDatamk");
    console.log($scope.OrderData);
    $scope.orderDetails = d.orderDetails;
    //for display info in print final order
    OrderMasterService.saveinfo($scope.OrderData);
    // end 
    $scope.wData = [];
    OrderMasterService.getwarehouse().then(function (results) {


        $scope.warehouseData = results.data;

        for (var i = 0; i < $scope.warehouseData.length; i++) {
            $scope.warehouseData[i].CityId = $scope.warehouseData[i]['Cityid'];
            delete $scope.warehouseData[i].Cityid;
        }


        $scope.filterData = $filter('filter')($scope.warehouseData, { CityId: $scope.OrderData.CityId });

        $scope.wData = [];
        $scope.wData = $scope.filterData;

    }, function (error) {
    });

    //Check Customer Active or not 
    $scope.Customers = "";
    $http.get(serviceBase + 'api/InActiveCustOrderMaster/Customer?CustomerId=' + $scope.OrderData.CustomerId).then(function (results) {

        $scope.Customers = results.data;

    });


    // Post Order
    $scope.PostOrder = function (WarehouseId, status) {





        if (status === undefined || status === "") {
            alert('Please Select Status');
            return false;
        }
        //else {
        //    debugger
        //    if (status === "Order Canceled") {
        //        if ($scope.orderDetails[0].PaymentMode != "COD") {
        //            alert("You Cannot Cancle this Order")
        //            return false;
        //        }
        //    }
            
        //}
        if (status === 'Order Confirmed' && $scope.Customers.CustomerVerify != 'Temporary Active') {
            if (status === 'Order Confirmed' && !$scope.Customers.Active) {
                alert('Please Active Customer first then process order');
                return false;
            }
        }

        if (($scope.orderDetails[0].PaymentMode === "Online" && !$scope.OrderData.ISPaymentStatusFailed) || $scope.orderDetails[0].PaymentMode === "Gullak") {
            DataPost();
            return false;
        }


        if (WarehouseId !== $scope.orderDetails[0].WarehouseId && WarehouseId !== undefined) {
            for (var i = 0; i < $scope.orderDetails.length; i++) {
                $scope.orderDetails[i].WarehouseId = WarehouseId;
            }
        }

        if (status === 'Order Confirmed' && ($scope.orderDetails[0].PaymentMode !== "Online" && !$scope.OrderData.ISPaymentStatusFailed) && $scope.orderDetails[0].PaymentMode !== "Gullak") {
            if ($scope.orderDetails.length === 1) {
                if ($scope.orderDetails[0].ItemActive === false) {
                    alert("Please cancel this order as item is deactive now");
                    return false;
                }
                if ($scope.orderDetails[0].ISItemLimit === true) {
                    if ($scope.orderDetails[0].ItemLimitQty === 0) {
                        alert("Please cancel this order as it has zero item limit");
                        return false;
                    }
                    else if ($scope.orderDetails[0].qty === 0) {
                        alert("Please cancel this order as it has zero quantity");
                        return false;
                    }
                    //else if ($scope.orderDetails[0].ItemActive === false && ($scope.orderDetails[0].PaymentMode !== "Online" && $scope.orderDetails[0].PaymentMode !== "Gullak")) {
                    //    alert("Please cancel this order as it is deactive now");
                    //    return false;
                    //}
                }

            }
        }
        else if (status === 'Order Confirmed' && ($scope.orderDetails[0].PaymentMode === "Online" && !$scope.OrderData.ISPaymentStatusFailed) || $scope.orderDetails[0].PaymentMode === "Gullak") {
            DataPost();
            return false;
        }
        //if (status === 'Order Confirmed') {
        //    if ($scope.orderDetails.length === 1) {

        //        if ($scope.orderDetails[0].ISItemLimit === true) {
        //            if ($scope.orderDetails[0].ItemLimitQty === 0) {
        //                alert("Please cancel this order as it has zero item limit");
        //                return false;
        //            }
        //            else if ($scope.orderDetails[0].qty === 0) {
        //                alert("Please cancel this order as it has zero quantity");
        //                return false;
        //            }
        //        }

        //    }
        //}
        //$scope.TempArray = [];
        //$scope.TempArray1 = [];
        //$scope.TempArray = angular.copy($scope.orderDetails);
        //$scope.TempArray1 = angular.copy($scope.orderDetails);

        //var x = 0;
        //for (var z = 0; z < $scope.orderDetails.length; z++) {
        //    if ($scope.orderDetails[z].ItemActive === false && ($scope.orderDetails[0].PaymentMode !== "Online" || $scope.orderDetails[0].PaymentMode !== "Gullak")) {

        //        $scope.TempArray.splice(z, 1);
        //        x = x + 1;
        //    }
        //}

        //if (x === $scope.orderDetails.length) {
        //    alert("All item is deactive,please cancel this order ");
        //    return false;
        //}
        //if (x === 1) {


        //    if ($scope.TempArray.length === 0) {
        //        alert("All item is deactive,please cancel this order ");
        //        return false;
        //    }

        //    else if ($window.confirm("Some of Item is deactive , Do you still want to proceed??")) {

        //        //$scope.orderDetails = angular.copy($scope.TempArray);
        //    }
        //    else {
        //        return false;
        //    }

        //}


        for (var j = 0; j < $scope.orderDetails.length; j++) {
            if ($scope.orderDetails[j].ISItemLimit === true) {

                if ($scope.orderDetails[j].ItemLimitQty < $scope.orderDetails[j].qty) {
                    if ($window.confirm("Order quantity is greater than Itemlimit quantity in one of order item, Order quantity may be differ after post order Do you still want to proceed??")) {

                        var DatatoPost =
                        {
                            CustomerName: $scope.OrderData.name,
                            OrderId: $scope.OrderData.OrderId,
                            DialEarnigPoint: 0,
                            Customerphonenum: $scope.OrderData.Customerphonenum,
                            SalesPersonId: 0,
                            ShippingAddress: $scope.OrderData.ShippingAddress,
                            ShopName: $scope.OrderData.Customerphonenum.shopName,
                            Skcode: $scope.OrderData.Skcode,
                            TotalAmount: $scope.OrderData.TotalAmount,
                            Savingamount: $scope.OrderData.Savingamount,
                            deliveryCharge: $scope.OrderData.deliveryCharge,
                            WalletAmount: 0,
                            walletPointUsed: 0,
                            DreamPoint: 0,
                            status: status,
                            itemDetails: $scope.orderDetails,
                            IsNewOrder: $scope.OrderData.IsNewOrder

                        };

                        DataPost();
                        return false;
                    }


                }
            }
        }


        for (var z = 0; z < $scope.orderDetails.length; z++) {

            if ($scope.orderDetails[z].IsFreeItem) {
                alert("This order also conatins free item");
                break;
            }
        }

        if ($window.confirm(" Order quantity may be differ after post order Do you still want to proceed??")) {
            DataPost();
        }

        function DataPost() {
            var DatatoPost =
            {
                CustomerName: $scope.OrderData.name,
                OrderId: $scope.OrderData.OrderId,
                DialEarnigPoint: 0,
                Customerphonenum: $scope.OrderData.Customerphonenum,
                SalesPersonId: 0,
                ShippingAddress: $scope.OrderData.ShippingAddress,
                ShopName: $scope.OrderData.Customerphonenum.shopName,
                Skcode: $scope.OrderData.Skcode,
                TotalAmount: $scope.OrderData.TotalAmount,
                Savingamount: $scope.OrderData.Savingamount,
                deliveryCharge: $scope.OrderData.deliveryCharge,
                WalletAmount: 0,
                walletPointUsed: 0,
                DreamPoint: 0,
                status: status,
                itemDetails: $scope.orderDetails,
                IsNewOrder: $scope.OrderData.IsNewOrder

            };

            var url = serviceBase + "api/InActiveCustOrderMaster";
            $http.post(url, DatatoPost).then(function (results) {

                if (results.data === true) {
                    alert('OrderPost Successfully');
                    $window.location.reload();
                }

                else {
                    alert('There Is Issue in OrderPost');
                    $window.location.reload();
                }
            }, function (error) {

                alert(error.data.ErrorMessage);
                //alert("error");
                return false;
            });
        }
    }

    $scope.ChangeAmount = function (qty, Amount, orderdetail, index) {


        if (qty > orderdetail.OrderQty) {
            alert("Quantity cannot be greater than Order quantity");
            orderdetail.qty = orderdetail.OrderQty;
            orderdetail.Noqty = orderdetail.OrderQty;
            return false;
        }
        else {

            orderdetail.qty = qty;
            orderdetail.Noqty = qty;
        }


        orderdetail.AmtWithoutTaxDisc = (orderdetail.PrevAmtWithoutTaxDisc / orderdetail.Prevqty) * qty;
        orderdetail.AmtWithoutAfterTaxDisc = (orderdetail.PrevAmtWithoutAfterTaxDisc / orderdetail.Prevqty) * qty;
        orderdetail.TotalAmountAfterTaxDisc = (orderdetail.PrevTotalAmountAfterTaxDisc / orderdetail.Prevqty) * qty;
        orderdetail.TaxAmmount = (orderdetail.PrevTotalAmountAfterTaxDisc / orderdetail.Prevqty) * qty;

        orderdetail.TotalAmt = (orderdetail.PrevTotalAmt / orderdetail.Prevqty) * qty;

        $scope.orderDetails.splice(index, 1, orderdetail);


        //$scope.orderDetails = [];
        //$scope.orderDetails.push(orderdetail);
    }

    $scope.PreventMinus = function (e) {

        if (e.keyCode != 45) {
        }
        else {
            event.preventDefault();
        }
    }




}]);




